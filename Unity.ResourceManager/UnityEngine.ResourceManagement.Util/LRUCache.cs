using System;
using System.Collections.Generic;

namespace UnityEngine.ResourceManagement.Util;

internal struct LRUCache<TKey, TValue> where TKey : IEquatable<TKey>
{
	public struct Key : IEquatable<Key>
	{
		private static Type typeType = typeof(Type);

		public TKey key;

		public Type type;

		public Key(TKey k, Type t)
		{
			key = k;
			type = t;
			if (typeType.IsAssignableFrom(type))
			{
				type = typeType;
			}
		}

		bool IEquatable<Key>.Equals(Key other)
		{
			if (key.Equals(other.key))
			{
				return type == other.type;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return key.GetHashCode() ^ type.GetHashCode();
		}
	}

	public struct Entry : IEquatable<Entry>
	{
		public LinkedListNode<Key> lruNode;

		public TValue Value;

		public bool Equals(Entry other)
		{
			return Value.Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	public int requestHits;

	public int requestCount;

	private int entryLimit;

	private Dictionary<Key, Entry> cache;

	private LinkedList<Key> lru;

	public LRUCache(int limit)
	{
		requestHits = (requestCount = 0);
		entryLimit = limit;
		cache = new Dictionary<Key, Entry>(limit);
		lru = new LinkedList<Key>();
	}

	public bool TryAdd(TKey id, TValue obj)
	{
		if (obj == null || entryLimit <= 0)
		{
			return false;
		}
		Key key = new Key(id, obj.GetType());
		LinkedListNode<Key> linkedListNode = new LinkedListNode<Key>(key);
		if (!cache.TryAdd(key, new Entry
		{
			Value = obj,
			lruNode = linkedListNode
		}))
		{
			return false;
		}
		lru.AddFirst(linkedListNode);
		while (lru.Count > entryLimit)
		{
			cache.Remove(lru.Last.Value);
			_ = lru.Last;
			lru.RemoveLast();
		}
		return true;
	}

	public bool TryGet(Type type, TKey id, out TValue val)
	{
		requestCount++;
		Key key = new Key(id, type);
		if (cache.TryGetValue(key, out var value))
		{
			val = value.Value;
			if (value.lruNode.Previous != null)
			{
				lru.Remove(value.lruNode);
				lru.AddFirst(value.lruNode);
			}
			requestHits++;
			return true;
		}
		val = default(TValue);
		return false;
	}
}
