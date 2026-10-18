using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

internal class LinkedPool<T> : IDisposable where T : class
{
	internal class LinkedPoolItem
	{
		internal LinkedPoolItem poolNext;

		internal T value;
	}

	private readonly Func<T> m_CreateFunc;

	private readonly Action<T> m_ActionOnGet;

	private readonly Action<T> m_ActionOnRelease;

	private readonly Action<T> m_ActionOnDestroy;

	private readonly int m_Limit;

	private LinkedPoolItem m_PoolFirst;

	private LinkedPoolItem m_NextAvailableListItem;

	private readonly bool m_CollectionCheck;

	public int countInactive { get; private set; }

	public LinkedPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int maxSize = 10000)
	{
		if (createFunc == null)
		{
			throw new ArgumentNullException("createFunc");
		}
		if (maxSize <= 0)
		{
			throw new ArgumentException("Max size must be greater than 0", "maxSize");
		}
		m_CreateFunc = createFunc;
		m_ActionOnGet = actionOnGet;
		m_ActionOnRelease = actionOnRelease;
		m_ActionOnDestroy = actionOnDestroy;
		m_Limit = maxSize;
		m_CollectionCheck = collectionCheck;
	}

	public T Get()
	{
		T val;
		if (m_PoolFirst == null)
		{
			val = m_CreateFunc();
		}
		else
		{
			LinkedPoolItem poolFirst = m_PoolFirst;
			val = poolFirst.value;
			m_PoolFirst = poolFirst.poolNext;
			poolFirst.poolNext = m_NextAvailableListItem;
			m_NextAvailableListItem = poolFirst;
			m_NextAvailableListItem.value = null;
			int num = countInactive - 1;
			countInactive = num;
		}
		m_ActionOnGet?.Invoke(val);
		return val;
	}

	public PooledObject<T> Get(out T v)
	{
		return new PooledObject<T>(v = Get(), this);
	}

	public void Release(T item)
	{
		m_ActionOnRelease?.Invoke(item);
		if (countInactive < m_Limit)
		{
			LinkedPoolItem linkedPoolItem = m_NextAvailableListItem;
			if (linkedPoolItem == null)
			{
				linkedPoolItem = new LinkedPoolItem();
			}
			else
			{
				m_NextAvailableListItem = linkedPoolItem.poolNext;
			}
			linkedPoolItem.value = item;
			linkedPoolItem.poolNext = m_PoolFirst;
			m_PoolFirst = linkedPoolItem;
			int num = countInactive + 1;
			countInactive = num;
		}
		else
		{
			m_ActionOnDestroy?.Invoke(item);
		}
	}

	public void Clear()
	{
		if (m_ActionOnDestroy != null)
		{
			for (LinkedPoolItem linkedPoolItem = m_PoolFirst; linkedPoolItem != null; linkedPoolItem = linkedPoolItem.poolNext)
			{
				m_ActionOnDestroy(linkedPoolItem.value);
			}
		}
		m_PoolFirst = null;
		m_NextAvailableListItem = null;
		countInactive = 0;
	}

	public void Dispose()
	{
		Clear();
	}
}
