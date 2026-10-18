using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public struct SplineSlice<T> : ISpline, IReadOnlyList<BezierKnot>, IEnumerable<BezierKnot>, IEnumerable, IReadOnlyCollection<BezierKnot> where T : ISpline
{
	public T Spline;

	public SplineRange Range;

	public float4x4 Transform;

	public int Count
	{
		get
		{
			if (Spline.Closed)
			{
				return math.clamp(Range.Count, 0, Spline.Count + 1);
			}
			if (Range.Direction == SliceDirection.Backward)
			{
				return math.clamp(Range.Count, 0, Range.Start + 1);
			}
			return math.clamp(Range.Count, 0, Spline.Count - Range.Start);
		}
	}

	public bool Closed => false;

	public BezierKnot this[int index]
	{
		get
		{
			int num = Range[index];
			num = (num + Spline.Count) % Spline.Count;
			if (Range.Direction != SliceDirection.Backward)
			{
				return Spline[num].Transform(Transform);
			}
			return FlipTangents(Spline[num]).Transform(Transform);
		}
	}

	private static BezierKnot FlipTangents(BezierKnot knot)
	{
		return new BezierKnot(knot.Position, knot.TangentOut, knot.TangentIn, knot.Rotation);
	}

	public IEnumerator<BezierKnot> GetEnumerator()
	{
		int i = 0;
		int c = Range.Count;
		while (i < c)
		{
			yield return this[i];
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public SplineSlice(T spline, SplineRange range)
		: this(spline, range, float4x4.identity)
	{
	}

	public SplineSlice(T spline, SplineRange range, float4x4 transform)
	{
		Spline = spline;
		Range = range;
		Transform = transform;
	}

	public float GetLength()
	{
		float num = 0f;
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			num += GetCurveLength(i);
		}
		return num;
	}

	public BezierCurve GetCurve(int index)
	{
		int num = math.min(math.max(index + 1, 0), Range.Count - 1);
		BezierKnot a = this[index];
		BezierKnot b = this[num];
		if (index == num)
		{
			return new BezierCurve(a.Position, b.Position);
		}
		return new BezierCurve(a, b);
	}

	public float GetCurveLength(int index)
	{
		return CurveUtility.CalculateLength(GetCurve(index));
	}

	public float3 GetCurveUpVector(int index, float t)
	{
		return this.CalculateUpVector(index, t);
	}

	public float GetCurveInterpolation(int curveIndex, float curveDistance)
	{
		return CurveUtility.GetDistanceToInterpolation(GetCurve(curveIndex), curveDistance);
	}
}
