using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion;

public abstract class SerializableDictionary
{
	public static SerializableDictionary<TKey, TValue> Create<TKey, TValue>()
	{
		return new SerializableDictionary<TKey, TValue>();
	}
}
[Serializable]
public class SerializableDictionary<TKey, TValue> : SerializableDictionary, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ISerializationCallbackReceiver
{
	[Serializable]
	private struct Entry
	{
		public TKey Key;

		public TValue Value;
	}

	public const string ItemsPropertyPath = "_items";

	public const string EntryKeyPropertyPath = "Key";

	[SerializeField]
	private Entry[] _items;

	[NonSerialized]
	private List<(Entry, int)> _duplicatesAndNulls;

	[NonSerialized]
	private Dictionary<TKey, TValue> _dictionary;

	private Dictionary<TKey, TValue> Inner
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (_dictionary == null)
			{
				_dictionary = CreateDictionary();
			}
			return _dictionary;
		}
	}

	private ICollection<KeyValuePair<TKey, TValue>> DictionaryAsCollection => Inner;

	public TValue this[TKey key]
	{
		get
		{
			return Inner[key];
		}
		set
		{
			Inner[key] = value;
		}
	}

	public int Count => Inner.Count;

	public bool IsReadOnly => false;

	public Dictionary<TKey, TValue>.KeyCollection Keys => Inner.Keys;

	public Dictionary<TKey, TValue>.ValueCollection Values => Inner.Values;

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Inner.Keys;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => Inner.Values;

	public static SerializableDictionary<TKey, TValue> Wrap(Dictionary<TKey, TValue> dictionary)
	{
		return new SerializableDictionary<TKey, TValue>
		{
			_dictionary = dictionary
		};
	}

	public void Add(TKey key, TValue value)
	{
		Inner.Add(key, value);
	}

	public virtual void Clear()
	{
		_duplicatesAndNulls?.Clear();
		Inner.Clear();
	}

	public bool ContainsKey(TKey key)
	{
		return Inner.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		return Inner.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return Inner.TryGetValue(key, out value);
	}

	public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
	{
		return Inner.GetEnumerator();
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return Inner.GetEnumerator();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		DictionaryAsCollection.Add(item);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		return DictionaryAsCollection.Contains(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		DictionaryAsCollection.CopyTo(array, arrayIndex);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		return DictionaryAsCollection.Remove(item);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Inner.GetEnumerator();
	}

	private Dictionary<TKey, TValue> CreateDictionary()
	{
		_duplicatesAndNulls?.Clear();
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
		bool flag = !typeof(TKey).IsValueType;
		if (_items != null)
		{
			for (int i = 0; i < _items.Length; i++)
			{
				Entry item = _items[i];
				TKey key = item.Key;
				TValue value = item.Value;
				TKey val = key;
				if ((flag && dictionary.Comparer.Equals(val, default(TKey))) || dictionary.ContainsKey(val))
				{
					if (_duplicatesAndNulls == null)
					{
						_duplicatesAndNulls = new List<(Entry, int)>();
					}
					_duplicatesAndNulls.Add((item, i));
				}
				else
				{
					dictionary.Add(val, value);
				}
			}
		}
		return dictionary;
	}

	public void Reset()
	{
		_dictionary = null;
	}

	public void Store()
	{
		int num = Count + (_duplicatesAndNulls?.Count ?? 0);
		Entry[] items = _items;
		if (items == null || items.Length != num)
		{
			Array.Resize(ref _items, num);
		}
		int num2 = 0;
		using (Dictionary<TKey, TValue>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				_items[num2] = new Entry
				{
					Key = current.Key,
					Value = current.Value
				};
				num2++;
			}
		}
		if (_duplicatesAndNulls == null)
		{
			return;
		}
		foreach (var duplicatesAndNull in _duplicatesAndNulls)
		{
			for (int num3 = num2 - 1; num3 > duplicatesAndNull.Item2; num3--)
			{
				_items[num3] = _items[num3 - 1];
			}
			_items[duplicatesAndNull.Item2] = duplicatesAndNull.Item1;
			num2++;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		Reset();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		Store();
	}
}
