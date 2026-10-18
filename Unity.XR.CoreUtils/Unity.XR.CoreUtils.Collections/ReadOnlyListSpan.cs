using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Unity.XR.CoreUtils.Collections;

public struct ReadOnlyListSpan<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, IEquatable<ReadOnlyListSpan<T>>
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		internal IReadOnlyList<T> list;

		private int m_CurrentIndex;

		public int start { get; }

		public int end { get; }

		public T Current
		{
			get
			{
				if (m_CurrentIndex < start || m_CurrentIndex >= end)
				{
					throw new ArgumentOutOfRangeException();
				}
				return list[m_CurrentIndex];
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(IReadOnlyList<T> list)
			: this(list, 0, list.Count)
		{
		}

		internal Enumerator(IReadOnlyList<T> list, int start, int end)
		{
			this.list = list;
			this.start = start;
			this.end = end;
			m_CurrentIndex = this.start - 1;
		}

		public bool MoveNext()
		{
			m_CurrentIndex++;
			return m_CurrentIndex < end;
		}

		public void Reset()
		{
			m_CurrentIndex = start - 1;
		}

		void IDisposable.Dispose()
		{
		}
	}

	private static ReadOnlyListSpan<T> s_EmptyList;

	private Enumerator m_Enumerator;

	public int Count => m_Enumerator.end - m_Enumerator.start;

	public T this[int index]
	{
		get
		{
			index += m_Enumerator.start;
			if (index < m_Enumerator.start || index >= m_Enumerator.end)
			{
				throw new ArgumentOutOfRangeException();
			}
			return m_Enumerator.list[index];
		}
	}

	public ReadOnlyListSpan(IReadOnlyList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		m_Enumerator = new Enumerator(list);
	}

	public ReadOnlyListSpan(IReadOnlyList<T> list, int start, int length)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (start < 0 || start + length > list.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_Enumerator = new Enumerator(list, start, start + length);
	}

	public ReadOnlyListSpan<T> Slice(int start, int length)
	{
		int num = m_Enumerator.start + start;
		if (num < m_Enumerator.start || num + length > m_Enumerator.end)
		{
			throw new ArgumentOutOfRangeException();
		}
		return new ReadOnlyListSpan<T>(m_Enumerator.list, m_Enumerator.start + start, length);
	}

	public static ReadOnlyListSpan<T> Empty()
	{
		return s_EmptyList;
	}

	public Enumerator GetEnumerator()
	{
		return m_Enumerator;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Equals(ReadOnlyListSpan<T> other)
	{
		if (m_Enumerator.list == other.m_Enumerator.list && m_Enumerator.start == other.m_Enumerator.start)
		{
			return m_Enumerator.end == other.m_Enumerator.end;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ReadOnlyListSpan<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(ReadOnlyListSpan<T> lhs, ReadOnlyListSpan<T> rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(ReadOnlyListSpan<T> lhs, ReadOnlyListSpan<T> rhs)
	{
		return !(lhs == rhs);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_Enumerator.list, m_Enumerator.start, m_Enumerator.end);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("{");
		for (int i = m_Enumerator.start; i < m_Enumerator.end; i++)
		{
			T val = m_Enumerator.list[i];
			stringBuilder.AppendLine((val == null) ? "  null," : ("  " + m_Enumerator.list[i].ToString() + ","));
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
