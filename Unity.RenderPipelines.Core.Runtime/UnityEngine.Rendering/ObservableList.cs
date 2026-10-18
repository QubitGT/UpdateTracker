using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public class ObservableList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private List<T> m_List;

	private readonly Comparison<T> m_Comparison;

	public T this[int index]
	{
		get
		{
			return m_List[index];
		}
		set
		{
			OnEvent(this.ItemRemoved, index, m_List[index]);
			m_List[index] = value;
			OnEvent(this.ItemAdded, index, value);
		}
	}

	public int Count => m_List.Count;

	public bool IsReadOnly => false;

	public event ListChangedEventHandler<T> ItemAdded;

	public event ListChangedEventHandler<T> ItemRemoved;

	public ObservableList()
		: this(0, (Comparison<T>)null)
	{
	}

	public ObservableList(int capacity, Comparison<T> comparison = null)
	{
		m_List = new List<T>(capacity);
		m_Comparison = comparison;
	}

	public ObservableList(IEnumerable<T> collection, Comparison<T> comparison = null)
	{
		m_List = new List<T>(collection);
		m_Comparison = comparison;
		Sort();
	}

	private void OnEvent(ListChangedEventHandler<T> e, int index, T item)
	{
		e?.Invoke(this, new ListChangedEventArgs<T>(index, item));
	}

	public bool Contains(T item)
	{
		return m_List.Contains(item);
	}

	public int IndexOf(T item)
	{
		return m_List.IndexOf(item);
	}

	public void Add(T item)
	{
		m_List.Add(item);
		Sort();
		OnEvent(this.ItemAdded, m_List.IndexOf(item), item);
	}

	public void Add(params T[] items)
	{
		foreach (T item in items)
		{
			Add(item);
		}
	}

	public void Insert(int index, T item)
	{
		m_List.Insert(index, item);
		Sort();
		OnEvent(this.ItemAdded, index, item);
	}

	public bool Remove(T item)
	{
		int index = m_List.IndexOf(item);
		bool num = m_List.Remove(item);
		if (num)
		{
			OnEvent(this.ItemRemoved, index, item);
		}
		return num;
	}

	public int Remove(params T[] items)
	{
		if (items == null)
		{
			return 0;
		}
		int num = 0;
		foreach (T item in items)
		{
			num += (Remove(item) ? 1 : 0);
		}
		return num;
	}

	public void RemoveAt(int index)
	{
		T item = m_List[index];
		m_List.RemoveAt(index);
		OnEvent(this.ItemRemoved, index, item);
	}

	public void Clear()
	{
		while (Count > 0)
		{
			RemoveAt(Count - 1);
		}
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_List.CopyTo(array, arrayIndex);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_List.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void Sort()
	{
		if (m_Comparison != null)
		{
			m_List.Sort(m_Comparison);
		}
	}
}
