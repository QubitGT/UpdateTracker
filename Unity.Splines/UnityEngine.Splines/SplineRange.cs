using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[Serializable]
public struct SplineRange : IEnumerable<int>, IEnumerable
{
	public struct SplineRangeEnumerator : IEnumerator<int>, IEnumerator, IDisposable
	{
		private int m_Index;

		private int m_Start;

		private int m_End;

		private int m_Count;

		private bool m_Reverse;

		public int Current
		{
			get
			{
				if (!m_Reverse)
				{
					return m_Start + m_Index;
				}
				return m_End - m_Index;
			}
		}

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			return ++m_Index < m_Count;
		}

		public void Reset()
		{
			m_Index = -1;
		}

		public SplineRangeEnumerator(SplineRange range)
		{
			m_Index = -1;
			m_Reverse = range.Direction == SliceDirection.Backward;
			int start = range.Start;
			int y = (m_Reverse ? (range.Start - range.Count) : (range.Start + range.Count));
			m_Start = math.min(start, y);
			m_End = math.max(start, y);
			m_Count = range.Count;
		}

		public void Dispose()
		{
		}
	}

	[SerializeField]
	private int m_Start;

	[SerializeField]
	private int m_Count;

	[SerializeField]
	private SliceDirection m_Direction;

	public int Start
	{
		get
		{
			return m_Start;
		}
		set
		{
			m_Start = value;
		}
	}

	public int End => this[Count - 1];

	public int Count
	{
		get
		{
			return m_Count;
		}
		set
		{
			m_Count = math.max(value, 0);
		}
	}

	public SliceDirection Direction
	{
		get
		{
			return m_Direction;
		}
		set
		{
			m_Direction = value;
		}
	}

	public int this[int index]
	{
		get
		{
			if (Direction != SliceDirection.Backward)
			{
				return m_Start + index;
			}
			return m_Start - index;
		}
	}

	public SplineRange(int start, int count)
		: this(start, count, (count < 0) ? SliceDirection.Backward : SliceDirection.Forward)
	{
	}

	public SplineRange(int start, int count, SliceDirection direction)
	{
		m_Start = start;
		m_Count = math.abs(count);
		m_Direction = direction;
	}

	public IEnumerator<int> GetEnumerator()
	{
		return new SplineRangeEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		return $"{{{Start}..{End}}}";
	}
}
