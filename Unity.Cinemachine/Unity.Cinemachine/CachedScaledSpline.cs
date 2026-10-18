using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

internal class CachedScaledSpline : ISpline, IReadOnlyList<BezierKnot>, IEnumerable<BezierKnot>, IEnumerable, IReadOnlyCollection<BezierKnot>, IDisposable
{
	private NativeSpline m_NativeSpline;

	private readonly Spline m_CachedSource;

	private readonly Vector3 m_CachedScale;

	private bool m_IsAllocated;

	public BezierKnot this[int index] => m_NativeSpline[index];

	public bool Closed => m_NativeSpline.Closed;

	public int Count => m_NativeSpline.Count;

	public CachedScaledSpline(Spline spline, Transform transform, Allocator allocator = Allocator.Persistent)
	{
		Vector3 vector = ((transform != null) ? transform.lossyScale : Vector3.one);
		m_CachedSource = spline;
		m_NativeSpline = new NativeSpline(spline, Matrix4x4.Scale(vector), allocator);
		m_CachedScale = vector;
		m_IsAllocated = true;
	}

	public void Dispose()
	{
		if (m_IsAllocated)
		{
			m_NativeSpline.Dispose();
		}
		m_IsAllocated = false;
	}

	public bool IsCrudelyValid(Spline spline, Transform transform)
	{
		Vector3 vector = ((transform != null) ? transform.lossyScale : Vector3.one);
		if (spline == m_CachedSource && (m_CachedScale - vector).AlmostZero())
		{
			return m_NativeSpline.Count == m_CachedSource.Count;
		}
		return false;
	}

	public bool KnotsAreValid(Spline spline, Transform transform)
	{
		if (m_NativeSpline.Count != spline.Count)
		{
			return false;
		}
		Matrix4x4 matrix4x = Matrix4x4.Scale((transform != null) ? transform.lossyScale : Vector3.one);
		IEnumerator<BezierKnot> enumerator = GetEnumerator();
		IEnumerator<BezierKnot> enumerator2 = spline.GetEnumerator();
		while (enumerator.MoveNext() && enumerator2.MoveNext())
		{
			if (!enumerator.Current.Equals(enumerator2.Current.Transform(matrix4x)))
			{
				return false;
			}
		}
		return true;
	}

	public BezierCurve GetCurve(int index)
	{
		return m_NativeSpline.GetCurve(index);
	}

	public float GetCurveInterpolation(int curveIndex, float curveDistance)
	{
		return m_NativeSpline.GetCurveInterpolation(curveIndex, curveDistance);
	}

	public float GetCurveLength(int index)
	{
		return m_NativeSpline.GetCurveLength(index);
	}

	public float3 GetCurveUpVector(int index, float t)
	{
		return m_NativeSpline.GetCurveUpVector(index, t);
	}

	public IEnumerator<BezierKnot> GetEnumerator()
	{
		return m_NativeSpline.GetEnumerator();
	}

	public float GetLength()
	{
		return m_NativeSpline.GetLength();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_NativeSpline.GetEnumerator();
	}
}
