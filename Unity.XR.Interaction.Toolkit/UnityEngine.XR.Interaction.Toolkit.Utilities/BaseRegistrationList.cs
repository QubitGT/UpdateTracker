using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal abstract class BaseRegistrationList<T>
{
	private static readonly LinkedPool<List<T>> s_BufferedListPool = new LinkedPool<List<T>>(() => new List<T>(), null, delegate(List<T> list)
	{
		list.Clear();
	}, null, collectionCheck: false);

	protected List<T> m_BufferedAdd;

	protected List<T> m_BufferedRemove;

	public List<T> registeredSnapshot { get; } = new List<T>();


	public int flushedCount => registeredSnapshot.Count - bufferedRemoveCount + bufferedAddCount;

	protected int bufferedAddCount => m_BufferedAdd?.Count ?? 0;

	protected int bufferedRemoveCount => m_BufferedRemove?.Count ?? 0;

	protected void AddToBufferedAdd(T item)
	{
		if (m_BufferedAdd == null)
		{
			m_BufferedAdd = s_BufferedListPool.Get();
		}
		m_BufferedAdd.Add(item);
	}

	protected bool RemoveFromBufferedAdd(T item)
	{
		if (m_BufferedAdd != null)
		{
			return m_BufferedAdd.Remove(item);
		}
		return false;
	}

	protected void ClearBufferedAdd()
	{
		if (m_BufferedAdd != null)
		{
			s_BufferedListPool.Release(m_BufferedAdd);
			m_BufferedAdd = null;
		}
	}

	protected void AddToBufferedRemove(T item)
	{
		if (m_BufferedRemove == null)
		{
			m_BufferedRemove = s_BufferedListPool.Get();
		}
		m_BufferedRemove.Add(item);
	}

	protected bool RemoveFromBufferedRemove(T item)
	{
		if (m_BufferedRemove != null)
		{
			return m_BufferedRemove.Remove(item);
		}
		return false;
	}

	protected void ClearBufferedRemove()
	{
		if (m_BufferedRemove != null)
		{
			s_BufferedListPool.Release(m_BufferedRemove);
			m_BufferedRemove = null;
		}
	}

	public abstract bool IsRegistered(T item);

	public abstract bool IsStillRegistered(T item);

	public abstract bool Register(T item);

	public abstract bool Unregister(T item);

	public abstract void Flush();

	public abstract void GetRegisteredItems(List<T> results);

	public abstract T GetRegisteredItemAt(int index);

	public bool MoveItemImmediately(T item, int newIndex)
	{
		if (bufferedRemoveCount != 0 || bufferedAddCount != 0)
		{
			throw new InvalidOperationException("Cannot move item when there are pending registration changes that have not been flushed.");
		}
		int num = registeredSnapshot.IndexOf(item);
		if (num == newIndex)
		{
			return false;
		}
		if (num >= 0)
		{
			registeredSnapshot.RemoveAt(num);
		}
		registeredSnapshot.Insert(newIndex, item);
		OnItemMovedImmediately(item, newIndex);
		return num < 0;
	}

	protected virtual void OnItemMovedImmediately(T item, int newIndex)
	{
	}

	public void UnregisterAll()
	{
		List<T> v;
		using (s_BufferedListPool.Get(out v))
		{
			GetRegisteredItems(v);
			for (int num = v.Count - 1; num >= 0; num--)
			{
				Unregister(v[num]);
			}
		}
	}

	protected static void EnsureCapacity(List<T> list, int capacity)
	{
		if (list.Capacity < capacity)
		{
			list.Capacity = capacity;
		}
	}
}
