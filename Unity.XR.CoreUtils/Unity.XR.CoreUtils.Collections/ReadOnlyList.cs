using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Unity.XR.CoreUtils.Collections;

public class ReadOnlyList<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, IEquatable<ReadOnlyList<T>>
{
	private static ReadOnlyList<T> s_EmptyList;

	private readonly List<T> m_List;

	public int Count => m_List.Count;

	public T this[int index] => m_List[index];

	public ReadOnlyList(List<T> list)
	{
		m_List = list ?? throw new ArgumentNullException("list");
	}

	public static ReadOnlyList<T> Empty()
	{
		if (s_EmptyList == null)
		{
			s_EmptyList = new ReadOnlyList<T>(new List<T>(0));
		}
		return s_EmptyList;
	}

	public List<T>.Enumerator GetEnumerator()
	{
		return m_List.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Equals(ReadOnlyList<T> other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this != other)
		{
			return object.Equals(m_List, other.m_List);
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((ReadOnlyList<T>)obj);
		}
		return false;
	}

	public static bool operator ==(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs)
	{
		if ((object)lhs == null && (object)rhs == null)
		{
			return true;
		}
		return lhs?.Equals(rhs) ?? false;
	}

	public static bool operator !=(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs)
	{
		return !(lhs == rhs);
	}

	public override int GetHashCode()
	{
		if (m_List == null)
		{
			return 0;
		}
		return m_List.GetHashCode();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("{");
		foreach (T item in m_List)
		{
			stringBuilder.AppendLine((item == null) ? "  null," : ("  " + item.ToString() + ","));
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
