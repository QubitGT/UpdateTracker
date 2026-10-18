using System.Collections.Generic;

namespace UnityEngine.ResourceManagement.Util;

public class LinkedListNodeCache<T>
{
	private int m_maxNodesAllowed = int.MaxValue;

	private int m_NodesCreated;

	private Stack<LinkedListNode<T>> m_NodeCache;

	internal int CreatedNodeCount => m_NodesCreated;

	internal int CachedNodeCount
	{
		get
		{
			if (m_NodeCache != null)
			{
				return m_NodeCache.Count;
			}
			return 0;
		}
		set
		{
			while (value < m_NodeCache.Count)
			{
				m_NodeCache.TryPop(out var _);
			}
			while (value > m_NodeCache.Count)
			{
				m_NodeCache.Push(new LinkedListNode<T>(default(T)));
			}
		}
	}

	public LinkedListNodeCache()
	{
		InitCache();
	}

	public LinkedListNodeCache(int maxNodesAllowed, int initialCapacity, int initialPreallocateCount)
	{
		InitCache(maxNodesAllowed, initialCapacity, initialPreallocateCount);
	}

	private void InitCache(int maxNodesAllowed = int.MaxValue, int initialCapacity = 10, int initialPreallocateCount = 0)
	{
		m_maxNodesAllowed = maxNodesAllowed;
		m_NodeCache = new Stack<LinkedListNode<T>>(initialCapacity);
		for (int i = 0; i < initialPreallocateCount; i++)
		{
			m_NodeCache.Push(new LinkedListNode<T>(default(T)));
			m_NodesCreated++;
		}
	}

	public LinkedListNode<T> Acquire(T val)
	{
		if (m_NodeCache.TryPop(out var result))
		{
			result.Value = val;
			return result;
		}
		m_NodesCreated++;
		return new LinkedListNode<T>(val);
	}

	public void Release(LinkedListNode<T> node)
	{
		if (m_NodeCache.Count < m_maxNodesAllowed)
		{
			node.Value = default(T);
			m_NodeCache.Push(node);
		}
	}
}
