#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fusion;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(NetworkDictionary<, >.DebuggerProxy))]
public struct NetworkDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>, IEnumerable, INetworkDictionary
{
	internal class DebuggerProxy : Dictionary<K, V>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Lazy<KeyValuePair<K, V>[]> _items;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public KeyValuePair<K, V>[] Items => _items.Value;

		public unsafe DebuggerProxy(NetworkDictionary<K, V> dict)
		{
			_items = new Lazy<KeyValuePair<K, V>[]>(() => (dict._data == null) ? Array.Empty<KeyValuePair<K, V>>() : dict.ToArray());
		}
	}

	public struct Enumerator : IEnumerator<KeyValuePair<K, V>>, IEnumerator, IDisposable
	{
		private int _bucket;

		private int _entry;

		private NetworkDictionary<K, V> _dict;

		public KeyValuePair<K, V> Current
		{
			get
			{
				if (_entry > 0 && _entry < _dict._capacity)
				{
					return new KeyValuePair<K, V>(_dict.GetKey(_entry), _dict.GetVal(_entry));
				}
				throw new InvalidOperationException();
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(NetworkDictionary<K, V> dict)
		{
			_dict = dict;
			_entry = 0;
			_bucket = -1;
		}

		public unsafe bool MoveNext()
		{
			while (true)
			{
				if (_entry == 0)
				{
					if (_bucket + 1 >= _dict._capacity)
					{
						return false;
					}
					_bucket++;
					_entry = (_dict._data + _dict._bucketsOffset)[_bucket];
					if (_entry != 0)
					{
						return true;
					}
				}
				else
				{
					_entry = _dict.GetNxt(_entry);
					if (_entry != 0)
					{
						break;
					}
				}
			}
			return true;
		}

		public void Reset()
		{
			_bucket = -1;
			_entry = 0;
		}

		public void Dispose()
		{
			_dict = default(NetworkDictionary<K, V>);
			_entry = -1;
			_bucket = -1;
		}
	}

	public const int META_WORD_COUNT = 3;

	private const int FREE_OFFSET = 0;

	private const int FREE_COUNT_OFFSET = 1;

	private const int USED_COUNT_OFFSET = 2;

	private const int INVALID_ENTRY = 0;

	private unsafe int* _data;

	private int _capacity;

	private int _nxtOffset;

	private int _keyOffset;

	private int _valOffset;

	private int _entryStride;

	private int _bucketsOffset;

	private int _entriesOffset;

	private IElementReaderWriter<K> _keyReaderWriter;

	private IElementReaderWriter<V> _valReaderWriter;

	private EqualityComparer<K> _equalityComparer;

	private unsafe int _free
	{
		get
		{
			return *_data;
		}
		set
		{
			*_data = value;
		}
	}

	private unsafe int _freeCount
	{
		get
		{
			return _data[1];
		}
		set
		{
			_data[1] = value;
		}
	}

	private unsafe int _usedCount
	{
		get
		{
			return _data[2];
		}
		set
		{
			_data[2] = value;
		}
	}

	public unsafe int Count
	{
		get
		{
			Assert.Check(_data);
			return _usedCount - _freeCount - 1;
		}
	}

	public int Capacity => _capacity - 1;

	public V this[K key]
	{
		get
		{
			return Get(key);
		}
		set
		{
			Set(key, value);
		}
	}

	public unsafe NetworkDictionary(int* data, int capacity, IElementReaderWriter<K> keyReaderWriter, IElementReaderWriter<V> valReaderWriter)
	{
		Assert.Check(Primes.IsPrime(capacity), "Capacity not prime {0}", capacity);
		int elementWordCount = keyReaderWriter.GetElementWordCount();
		int elementWordCount2 = valReaderWriter.GetElementWordCount();
		_keyReaderWriter = keyReaderWriter;
		_valReaderWriter = valReaderWriter;
		_data = data;
		_capacity = capacity;
		_nxtOffset = 0;
		_keyOffset = 1;
		_valOffset = 1 + elementWordCount;
		_entryStride = 1 + elementWordCount + elementWordCount2;
		_bucketsOffset = 3;
		_entriesOffset = _bucketsOffset + _capacity;
		_equalityComparer = EqualityComparer<K>.Default;
		if (_usedCount == 0)
		{
			_usedCount = 1;
		}
	}

	public unsafe NetworkDictionaryReadOnly<K, V> ToReadOnly()
	{
		return new NetworkDictionaryReadOnly<K, V>(_data, _capacity, _keyReaderWriter, _valReaderWriter);
	}

	public unsafe void Clear()
	{
		Assert.Check(_data);
		_usedCount = 1;
		_free = 0;
		_freeCount = 0;
		Native.MemClear(_data + _bucketsOffset, _capacity * 4);
	}

	public bool ContainsKey(K key)
	{
		return Find(key) != 0;
	}

	public bool ContainsValue(V value, IEqualityComparer<V> equalityComparer = null)
	{
		Enumerator enumerator = GetEnumerator();
		if (equalityComparer == null)
		{
			equalityComparer = EqualityComparer<V>.Default;
		}
		while (enumerator.MoveNext())
		{
			if (equalityComparer.Equals(enumerator.Current.Value, value))
			{
				return true;
			}
		}
		enumerator.Dispose();
		return false;
	}

	public V Get(K key)
	{
		if (TryGet(key, out var value))
		{
			return value;
		}
		throw new KeyNotFoundException();
	}

	public V Set(K key, V value)
	{
		int num = Find(key);
		if (num == 0)
		{
			Insert(key, value);
		}
		else
		{
			SetVal(num, value);
		}
		return value;
	}

	public unsafe bool Add(K key, V value)
	{
		Assert.Check(_data);
		if (Find(key) == 0)
		{
			Insert(key, value);
			return true;
		}
		return false;
	}

	public unsafe bool TryGet(K key, out V value)
	{
		Assert.Check(_data);
		int num = Find(key);
		if (num != 0)
		{
			value = GetVal(num);
			return true;
		}
		value = default(V);
		return false;
	}

	public bool Remove(K key)
	{
		V value;
		return Remove(key, out value);
	}

	public unsafe bool Remove(K key, out V value)
	{
		Assert.Check(_data);
		int* ptr = _data + _bucketsOffset;
		uint bucketFromHashCode = GetBucketFromHashCode(GetKeyHashCode(key));
		int num = ptr[bucketFromHashCode];
		int num2 = 0;
		while (num != 0)
		{
			if (_equalityComparer.Equals(GetKey(num), key))
			{
				if (num2 == 0)
				{
					ptr[bucketFromHashCode] = GetNxt(num);
				}
				else
				{
					SetNxt(num2, GetNxt(num));
				}
				value = GetVal(num);
				SetNxt(num, _free);
				_free = num;
				_freeCount++;
				return true;
			}
			num2 = num;
			num = GetNxt(num);
		}
		value = default(V);
		return false;
	}

	private unsafe int Insert(K key, V val)
	{
		int num = 0;
		if (_free != 0)
		{
			Assert.Check(_freeCount > 0);
			num = _free;
			_free = GetNxt(num);
			_freeCount--;
		}
		else
		{
			if (_usedCount == _capacity)
			{
				Assert.AlwaysFail("networked dictionary is full");
			}
			Assert.Check(_usedCount < _capacity);
			num = _usedCount++;
		}
		int* ptr = _data + _bucketsOffset;
		uint bucketFromHashCode = GetBucketFromHashCode(GetKeyHashCode(key));
		SetKey(num, key);
		SetVal(num, val);
		SetNxt(num, ptr[bucketFromHashCode]);
		ptr[bucketFromHashCode] = num;
		return num;
	}

	private unsafe int Find(K key)
	{
		Assert.Check(_capacity > 0);
		int* ptr = _data + _bucketsOffset;
		uint bucketFromHashCode = GetBucketFromHashCode(GetKeyHashCode(key));
		for (int num = ptr[bucketFromHashCode]; num != 0; num = GetNxt(num))
		{
			if (_equalityComparer.Equals(GetKey(num), key))
			{
				return num;
			}
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private uint GetBucketFromHashCode(int hash)
	{
		return (uint)hash % (uint)_capacity;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void ClrEntry(int entry)
	{
		Native.MemClear(_data + _entriesOffset + _entryStride * entry, _entryStride * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe K GetKey(int entry)
	{
		return _keyReaderWriter.Read((byte*)(_data + _entriesOffset + (_entryStride * entry + _keyOffset)), 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void SetKey(int entry, K key)
	{
		_keyReaderWriter.Write((byte*)(_data + _entriesOffset + (_entryStride * entry + _keyOffset)), 0, key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe V GetVal(int entry)
	{
		return _valReaderWriter.Read((byte*)(_data + _entriesOffset + (_entryStride * entry + _valOffset)), 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void SetVal(int entry, V val)
	{
		_valReaderWriter.Write((byte*)(_data + _entriesOffset + (_entryStride * entry + _valOffset)), 0, val);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe int GetNxt(int entry)
	{
		return (_data + _entriesOffset)[_entryStride * entry + _nxtOffset];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void SetNxt(int entry, int next)
	{
		(_data + _entriesOffset)[_entryStride * entry + _nxtOffset] = next;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetKeyHashCode(K key)
	{
		return _keyReaderWriter.GetElementHashCode(key);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void INetworkDictionary.Add(object item)
	{
		KeyValuePair<K, V> keyValuePair = (KeyValuePair<K, V>)item;
		Add(keyValuePair.Key, keyValuePair.Value);
	}

	public unsafe static implicit operator NetworkDictionaryReadOnly<K, V>(NetworkDictionary<K, V> value)
	{
		return new NetworkDictionaryReadOnly<K, V>(value._data, value.Capacity, value._keyReaderWriter, value._valReaderWriter);
	}
}
