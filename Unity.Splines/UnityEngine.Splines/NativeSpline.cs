using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public struct NativeSpline : ISpline, IReadOnlyList<BezierKnot>, IEnumerable<BezierKnot>, IEnumerable, IReadOnlyCollection<BezierKnot>, IDisposable
{
	private struct Slice<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T> where T : struct
	{
		private NativeSlice<T> m_Slice;

		public int Count => m_Slice.Length;

		public T this[int index] => m_Slice[index];

		public Slice(NativeArray<T> array, int start, int count)
		{
			m_Slice = new NativeSlice<T>(array, start, count);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return m_Slice.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	[ReadOnly]
	private NativeArray<BezierKnot> m_Knots;

	[ReadOnly]
	private NativeArray<BezierCurve> m_Curves;

	[ReadOnly]
	private NativeArray<DistanceToInterpolation> m_SegmentLengthsLookupTable;

	[ReadOnly]
	private NativeArray<float3> m_UpVectorsLookupTable;

	private bool m_Closed;

	private float m_Length;

	private const int k_SegmentResolution = 30;

	public NativeArray<BezierKnot> Knots => m_Knots;

	public NativeArray<BezierCurve> Curves => m_Curves;

	public bool Closed => m_Closed;

	public int Count => m_Knots.Length;

	public BezierKnot this[int index] => m_Knots[index];

	public float GetLength()
	{
		return m_Length;
	}

	public IEnumerator<BezierKnot> GetEnumerator()
	{
		return m_Knots.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public NativeSpline(ISpline spline, Allocator allocator = Allocator.Temp)
		: this(spline, float4x4.identity, cacheUpVectors: false, allocator)
	{
	}

	public NativeSpline(ISpline spline, bool cacheUpVectors, Allocator allocator = Allocator.Temp)
		: this(spline, float4x4.identity, cacheUpVectors, allocator)
	{
	}

	public NativeSpline(ISpline spline, float4x4 transform, Allocator allocator = Allocator.Temp)
		: this(spline, (spline is IHasEmptyCurves hasEmptyCurves) ? hasEmptyCurves.EmptyCurves : null, spline.Closed, transform, cacheUpVectors: false, allocator)
	{
	}

	public NativeSpline(ISpline spline, float4x4 transform, bool cacheUpVectors, Allocator allocator = Allocator.Temp)
		: this(spline, (spline is IHasEmptyCurves hasEmptyCurves) ? hasEmptyCurves.EmptyCurves : null, spline.Closed, transform, cacheUpVectors, allocator)
	{
	}

	public NativeSpline(IReadOnlyList<BezierKnot> knots, bool closed, float4x4 transform, Allocator allocator = Allocator.Temp)
		: this(knots, null, closed, transform, cacheUpVectors: false, allocator)
	{
	}

	public NativeSpline(IReadOnlyList<BezierKnot> knots, bool closed, float4x4 transform, bool cacheUpVectors, Allocator allocator = Allocator.Temp)
		: this(knots, null, closed, transform, cacheUpVectors, allocator)
	{
	}

	public NativeSpline(IReadOnlyList<BezierKnot> knots, IReadOnlyList<int> splits, bool closed, float4x4 transform, Allocator allocator = Allocator.Temp)
		: this(knots, splits, closed, transform, cacheUpVectors: false, allocator)
	{
	}

	public NativeSpline(IReadOnlyList<BezierKnot> knots, IReadOnlyList<int> splits, bool closed, float4x4 transform, bool cacheUpVectors, Allocator allocator = Allocator.Temp)
	{
		int count = knots.Count;
		m_Knots = new NativeArray<BezierKnot>(count, allocator);
		m_Curves = new NativeArray<BezierCurve>(count, allocator);
		m_SegmentLengthsLookupTable = new NativeArray<DistanceToInterpolation>(count * 30, allocator);
		m_Closed = closed;
		m_Length = 0f;
		m_UpVectorsLookupTable = new NativeArray<float3>(cacheUpVectors ? (count * 30) : 0, allocator);
		NativeArray<DistanceToInterpolation> lookupTable = new NativeArray<DistanceToInterpolation>(30, Allocator.Temp);
		NativeArray<float3> upVectors = (cacheUpVectors ? new NativeArray<float3>(30, Allocator.Temp) : default(NativeArray<float3>));
		if (count <= 0)
		{
			return;
		}
		BezierKnot bezierKnot = knots[0].Transform(transform);
		for (int i = 0; i < count; i++)
		{
			BezierKnot bezierKnot2 = knots[(i + 1) % count].Transform(transform);
			m_Knots[i] = bezierKnot;
			if (splits != null && splits.Contains(i))
			{
				m_Curves[i] = new BezierCurve(new BezierKnot(bezierKnot.Position), new BezierKnot(bezierKnot.Position));
				float3 value = (cacheUpVectors ? math.rotate(bezierKnot.Rotation, math.up()) : float3.zero);
				for (int j = 0; j < 30; j++)
				{
					lookupTable[j] = default(DistanceToInterpolation);
					if (cacheUpVectors)
					{
						upVectors[j] = value;
					}
				}
			}
			else
			{
				m_Curves[i] = new BezierCurve(bezierKnot, bezierKnot2);
				CurveUtility.CalculateCurveLengths(m_Curves[i], lookupTable);
				if (cacheUpVectors)
				{
					float3 startUp = math.rotate(bezierKnot.Rotation, math.up());
					float3 endUp = math.rotate(bezierKnot2.Rotation, math.up());
					CurveUtility.EvaluateUpVectors(m_Curves[i], startUp, endUp, upVectors);
				}
			}
			if (m_Closed || i < count - 1)
			{
				m_Length += lookupTable[29].Distance;
			}
			for (int k = 0; k < 30; k++)
			{
				m_SegmentLengthsLookupTable[i * 30 + k] = lookupTable[k];
				if (cacheUpVectors)
				{
					m_UpVectorsLookupTable[i * 30 + k] = upVectors[k];
				}
			}
			bezierKnot = bezierKnot2;
		}
	}

	public BezierCurve GetCurve(int index)
	{
		return m_Curves[index];
	}

	public float GetCurveLength(int curveIndex)
	{
		return m_SegmentLengthsLookupTable[curveIndex * 30 + 30 - 1].Distance;
	}

	public float3 GetCurveUpVector(int index, float t)
	{
		if (m_UpVectorsLookupTable.Length == 0)
		{
			return this.CalculateUpVector(index, t);
		}
		int num = index * 30;
		float num2 = 1f / 29f;
		float num3 = 0f;
		for (int i = 0; i < 30; i++)
		{
			if (t <= num3 + num2)
			{
				return math.lerp(m_UpVectorsLookupTable[num + i], m_UpVectorsLookupTable[num + i + 1], (t - num3) / num2);
			}
			num3 += num2;
		}
		return m_UpVectorsLookupTable[num + 30 - 1];
	}

	public void Dispose()
	{
		m_Knots.Dispose();
		m_Curves.Dispose();
		m_SegmentLengthsLookupTable.Dispose();
		m_UpVectorsLookupTable.Dispose();
	}

	public float GetCurveInterpolation(int curveIndex, float curveDistance)
	{
		if (curveIndex < 0 || curveIndex >= m_SegmentLengthsLookupTable.Length || curveDistance <= 0f)
		{
			return 0f;
		}
		float curveLength = GetCurveLength(curveIndex);
		if (curveDistance >= curveLength)
		{
			return 1f;
		}
		int start = curveIndex * 30;
		return CurveUtility.GetDistanceToInterpolation(new Slice<DistanceToInterpolation>(m_SegmentLengthsLookupTable, start, 30), curveDistance);
	}
}
