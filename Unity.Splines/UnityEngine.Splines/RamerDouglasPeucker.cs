using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Splines;

internal class RamerDouglasPeucker<T> where T : IList<float3>
{
	private struct Range
	{
		public int Start;

		public int Count;

		public int End => Start + Count - 1;

		public Range(int start, int count)
		{
			Start = start;
			Count = count;
		}

		public override string ToString()
		{
			return $"[{Start}, {End}]";
		}
	}

	private T m_Points;

	private bool[] m_Keep;

	private float m_Epsilon;

	private int m_KeepCount;

	public RamerDouglasPeucker(T points)
	{
		m_Points = points;
	}

	public void Reduce(List<float3> results, float epsilon)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		m_Epsilon = math.max(float.Epsilon, epsilon);
		m_KeepCount = m_Points.Count;
		m_Keep = new bool[m_KeepCount];
		for (int i = 0; i < m_KeepCount; i++)
		{
			Keep(i);
		}
		Reduce(new Range(0, m_KeepCount));
		results.Clear();
		if (results.Capacity < m_KeepCount)
		{
			results.Capacity = m_KeepCount;
		}
		for (int j = 0; j < m_Keep.Length; j++)
		{
			if (m_Keep[j])
			{
				results.Add(m_Points[j]);
			}
		}
	}

	private void Keep(int index)
	{
		m_Keep[index] = true;
	}

	private void Discard(Range range)
	{
		m_KeepCount -= range.Count;
		for (int i = range.Start; i <= range.End; i++)
		{
			m_Keep[i] = false;
		}
	}

	private void Reduce(Range range)
	{
		if (range.Count >= 3)
		{
			(int, float) tuple = FindFarthest(range);
			if (tuple.Item2 < m_Epsilon)
			{
				Discard(new Range(range.Start + 1, range.Count - 2));
				return;
			}
			Reduce(new Range(range.Start, tuple.Item1 - range.Start + 1));
			Reduce(new Range(tuple.Item1, range.End - tuple.Item1 + 1));
		}
	}

	private (int index, float distance) FindFarthest(Range range)
	{
		float num = 0f;
		int item = -1;
		for (int i = range.Start + 1; i < range.End; i++)
		{
			float num2 = SplineMath.DistancePointLine(m_Points[i], m_Points[range.Start], m_Points[range.End]);
			if (num2 > num)
			{
				num = num2;
				item = i;
			}
		}
		return (index: item, distance: num);
	}
}
