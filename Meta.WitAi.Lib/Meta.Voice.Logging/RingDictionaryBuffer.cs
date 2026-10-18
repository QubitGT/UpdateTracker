using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Voice.Logging;

internal class RingDictionaryBuffer<TKey, TValue>
{
	private readonly int _capacity;

	private readonly ConcurrentDictionary<TKey, LinkedList<TValue>> _dictionary = new ConcurrentDictionary<TKey, LinkedList<TValue>>();

	private readonly ConcurrentDictionary<TKey, object> _valueLocks = new ConcurrentDictionary<TKey, object>();

	public ICollection<TValue> this[TKey key] => _dictionary[key];

	public RingDictionaryBuffer(int capacity)
	{
		_capacity = capacity;
	}

	public bool Add(TKey key, TValue value, bool unique = false)
	{
		if (!_dictionary.TryGetValue(key, out var value2))
		{
			value2 = new LinkedList<TValue>();
			_dictionary[key] = value2;
		}
		if (!_valueLocks.TryGetValue(key, out var value3))
		{
			value3 = new object();
			_valueLocks[key] = value3;
		}
		bool result = true;
		lock (value3)
		{
			if (unique && value2.Contains(value))
			{
				result = false;
				value2.Remove(value);
			}
			value2.AddFirst(value);
			while (value2.Count > Mathf.Max(0, _capacity))
			{
				value2.RemoveLast();
			}
			return result;
		}
	}

	public bool ContainsKey(TKey key)
	{
		return _dictionary.ContainsKey(key);
	}

	public IEnumerable<TValue> Extract(TKey key)
	{
		_valueLocks.TryRemove(key, out var _);
		if (!_dictionary.TryRemove(key, out var value2))
		{
			return new TValue[0];
		}
		return value2;
	}

	public IEnumerable<TValue> ExtractAll()
	{
		List<TValue> list = new List<TValue>();
		foreach (TKey item in new List<TKey>(_dictionary.Keys))
		{
			list.AddRange(Extract(item));
		}
		return list;
	}

	public void Clear()
	{
		_dictionary.Clear();
		_valueLocks.Clear();
	}
}
