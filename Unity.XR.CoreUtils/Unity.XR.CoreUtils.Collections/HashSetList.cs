using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Unity.XR.CoreUtils.Collections;

public class HashSetList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ISerializable, IDeserializationCallback, ISet<T>, IReadOnlyCollection<T>
{
	private readonly List<T> m_InternalList;

	private readonly HashSet<T> m_InternalHashSet;

	private int m_Count;

	public int Count => m_Count;

	bool ICollection<T>.IsReadOnly => false;

	public T this[int index] => m_InternalList[index];

	public HashSetList(int capacity = 0)
	{
		m_InternalList = new List<T>(capacity);
		m_InternalHashSet = new HashSet<T>();
	}

	public List<T>.Enumerator GetEnumerator()
	{
		return m_InternalList.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return m_InternalList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void ICollection<T>.Add(T item)
	{
		if (m_InternalHashSet.Add(item))
		{
			m_InternalList.Add(item);
			m_Count++;
		}
	}

	public bool Add(T item)
	{
		bool num = m_InternalHashSet.Add(item);
		if (num)
		{
			m_InternalList.Add(item);
			m_Count++;
		}
		return num;
	}

	public bool Remove(T item)
	{
		if (m_Count == 0)
		{
			return false;
		}
		bool num = m_InternalHashSet.Remove(item);
		if (num)
		{
			m_InternalList.Remove(item);
			m_Count--;
		}
		return num;
	}

	public void ExceptWith(IEnumerable<T> other)
	{
		m_InternalHashSet.ExceptWith(other);
		RefreshList();
	}

	public void IntersectWith(IEnumerable<T> other)
	{
		m_InternalHashSet.IntersectWith(other);
		RefreshList();
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		return m_InternalHashSet.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		return m_InternalHashSet.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		return m_InternalHashSet.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		return m_InternalHashSet.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		return m_InternalHashSet.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		return m_InternalHashSet.SetEquals(other);
	}

	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		m_InternalHashSet.SymmetricExceptWith(other);
		RefreshList();
	}

	public void UnionWith(IEnumerable<T> other)
	{
		m_InternalHashSet.UnionWith(other);
		RefreshList();
	}

	public void Clear()
	{
		m_InternalHashSet.Clear();
		m_InternalList.Clear();
	}

	public bool Contains(T item)
	{
		return m_InternalHashSet.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_InternalList.CopyTo(array, arrayIndex);
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		m_InternalHashSet.GetObjectData(info, context);
		RefreshList();
	}

	public void OnDeserialization(object sender)
	{
		m_InternalHashSet.OnDeserialization(sender);
		RefreshList();
	}

	private void RefreshList()
	{
		m_InternalList.Clear();
		m_InternalList.AddRange(m_InternalHashSet);
		m_Count = m_InternalList.Count;
	}

	public IReadOnlyList<T> AsList()
	{
		return m_InternalList;
	}
}
