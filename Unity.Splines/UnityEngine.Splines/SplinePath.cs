using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public class SplinePath : SplinePath<SplineSlice<Spline>>
{
	public SplinePath(IEnumerable<SplineSlice<Spline>> slices)
		: base(slices)
	{
	}
}
public class SplinePath<T> : ISpline, IReadOnlyList<BezierKnot>, IEnumerable<BezierKnot>, IEnumerable, IReadOnlyCollection<BezierKnot>, IHasEmptyCurves where T : ISpline
{
	private T[] m_Splines;

	private int[] m_Splits;

	public IReadOnlyList<T> Slices
	{
		get
		{
			return m_Splines;
		}
		set
		{
			m_Splines = value.ToArray();
			BuildSplitData();
		}
	}

	public int Count
	{
		get
		{
			int num = 0;
			T[] splines = m_Splines;
			for (int i = 0; i < splines.Length; i++)
			{
				T val = splines[i];
				num += val.Count + (val.Closed ? 1 : 0);
			}
			return num;
		}
	}

	public BezierKnot this[int index] => this[GetBranchKnotIndex(index)];

	public BezierKnot this[SplineKnotIndex index]
	{
		get
		{
			T val = m_Splines[index.Spline];
			int index2 = (val.Closed ? (index.Knot % val.Count) : index.Knot);
			return val[index2];
		}
	}

	public bool Closed => false;

	public IReadOnlyList<int> EmptyCurves => m_Splits;

	public SplinePath(IEnumerable<T> slices)
	{
		m_Splines = slices.ToArray();
		BuildSplitData();
	}

	private void BuildSplitData()
	{
		m_Splits = new int[m_Splines.Length];
		int i = 0;
		int num = m_Splits.Length;
		int num2 = 0;
		for (; i < num; i++)
		{
			m_Splits[i] = (num2 += m_Splines[i].Count + (m_Splines[i].Closed ? 1 : 0)) - 1;
		}
	}

	public IEnumerator<BezierKnot> GetEnumerator()
	{
		T[] splines = m_Splines;
		for (int i = 0; i < splines.Length; i++)
		{
			T val = splines[i];
			foreach (BezierKnot item in val)
			{
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal SplineKnotIndex GetBranchKnotIndex(int knot)
	{
		knot = (Closed ? (knot % Count) : math.clamp(knot, 0, Count));
		int i = 0;
		int num = 0;
		for (; i < m_Splines.Length; i++)
		{
			T val = m_Splines[i];
			int num2 = val.Count + (val.Closed ? 1 : 0);
			if (knot < num + num2)
			{
				return new SplineKnotIndex(i, math.max(0, knot - num));
			}
			num += num2;
		}
		return new SplineKnotIndex(m_Splines.Length - 1, m_Splines[^1].Count - 1);
	}

	public float GetLength()
	{
		float num = 0f;
		int i = 0;
		for (int num2 = (Closed ? Count : (Count - 1)); i < num2; i++)
		{
			num += GetCurveLength(i);
		}
		return num;
	}

	private bool IsDegenerate(int index)
	{
		if (Array.BinarySearch(m_Splits, index) < 0)
		{
			return false;
		}
		return true;
	}

	public BezierCurve GetCurve(int knot)
	{
		SplineKnotIndex branchKnotIndex = GetBranchKnotIndex(knot);
		if (IsDegenerate(knot))
		{
			BezierKnot bezierKnot = new BezierKnot(this[branchKnotIndex].Position);
			return new BezierCurve(bezierKnot, bezierKnot);
		}
		BezierKnot a = this[branchKnotIndex];
		BezierKnot b = this.Next(knot);
		return new BezierCurve(a, b);
	}

	public float GetCurveLength(int index)
	{
		if (IsDegenerate(index))
		{
			return 0f;
		}
		SplineKnotIndex branchKnotIndex = GetBranchKnotIndex(index);
		T val = m_Splines[branchKnotIndex.Spline];
		if (branchKnotIndex.Spline >= m_Splines.Length - 1 && branchKnotIndex.Knot >= val.Count - 1)
		{
			return CurveUtility.CalculateLength(GetCurve(index));
		}
		return val.GetCurveLength(branchKnotIndex.Knot);
	}

	public float3 GetCurveUpVector(int index, float t)
	{
		if (IsDegenerate(index))
		{
			return 0f;
		}
		SplineKnotIndex branchKnotIndex = GetBranchKnotIndex(index);
		T val = m_Splines[branchKnotIndex.Spline];
		if (branchKnotIndex.Spline >= m_Splines.Length - 1 && branchKnotIndex.Knot >= val.Count - 1)
		{
			BezierKnot a = this[branchKnotIndex];
			BezierKnot b = this.Next(index);
			BezierCurve curve = new BezierCurve(a, b);
			float3 startUp = math.rotate(a.Rotation, math.up());
			float3 endUp = math.rotate(b.Rotation, math.up());
			return CurveUtility.EvaluateUpVector(curve, t, startUp, endUp);
		}
		return val.GetCurveUpVector(branchKnotIndex.Knot, t);
	}

	public float GetCurveInterpolation(int curveIndex, float curveDistance)
	{
		SplineKnotIndex branchKnotIndex = GetBranchKnotIndex(curveIndex);
		T val = m_Splines[branchKnotIndex.Spline];
		return val.GetCurveInterpolation(branchKnotIndex.Knot, curveDistance);
	}
}
