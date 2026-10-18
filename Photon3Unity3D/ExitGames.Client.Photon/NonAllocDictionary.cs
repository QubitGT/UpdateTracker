using System;
using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon;

public class NonAllocDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable where K : IEquatable<K>
{
	public struct KeyIterator : IEnumerator<K>, IEnumerator, IDisposable
	{
		private int _index;

		private NonAllocDictionary<K, V> _dict;

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0)
				{
					throw new InvalidOperationException();
				}
				return _dict._nodes[_index].Key;
			}
		}

		public K Current
		{
			get
			{
				if (_index == 0)
				{
					return default(K);
				}
				return _dict._nodes[_index].Key;
			}
		}

		public KeyIterator(NonAllocDictionary<K, V> dictionary)
		{
			_index = 0;
			_dict = dictionary;
		}

		public KeyIterator GetEnumerator()
		{
			return this;
		}

		public void Reset()
		{
			_index = 0;
		}

		public bool MoveNext()
		{
			while (++_index < _dict._usedCount)
			{
				if (_dict._nodes[_index].Used)
				{
					return true;
				}
			}
			_index = 0;
			return false;
		}

		public void Dispose()
		{
		}
	}

	public struct ValueIterator : IEnumerator<V>, IEnumerator, IDisposable
	{
		private int _index;

		private NonAllocDictionary<K, V> _dict;

		public V Current
		{
			get
			{
				if (_index == 0)
				{
					return default(V);
				}
				return _dict._nodes[_index].Val;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0)
				{
					throw new InvalidOperationException();
				}
				return _dict._nodes[_index].Val;
			}
		}

		public ValueIterator(NonAllocDictionary<K, V> dictionary)
		{
			_index = 0;
			_dict = dictionary;
		}

		public ValueIterator GetEnumerator()
		{
			return this;
		}

		public void Reset()
		{
			_index = 0;
		}

		public bool MoveNext()
		{
			while (++_index < _dict._usedCount)
			{
				if (_dict._nodes[_index].Used)
				{
					return true;
				}
			}
			_index = 0;
			return false;
		}

		public void Dispose()
		{
		}
	}

	public struct PairIterator : IEnumerator<KeyValuePair<K, V>>, IEnumerator, IDisposable
	{
		private int _index;

		private NonAllocDictionary<K, V> _dict;

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0)
				{
					throw new InvalidOperationException();
				}
				return Current;
			}
		}

		public KeyValuePair<K, V> Current
		{
			get
			{
				if (_index == 0)
				{
					return default(KeyValuePair<K, V>);
				}
				return new KeyValuePair<K, V>(_dict._nodes[_index].Key, _dict._nodes[_index].Val);
			}
		}

		public PairIterator(NonAllocDictionary<K, V> dictionary)
		{
			_index = 0;
			_dict = dictionary;
		}

		public void Reset()
		{
			_index = 0;
		}

		public bool MoveNext()
		{
			while (++_index < _dict._usedCount)
			{
				if (_dict._nodes[_index].Used)
				{
					return true;
				}
			}
			_index = 0;
			return false;
		}

		public void Dispose()
		{
		}
	}

	private struct Node
	{
		public bool Used;

		public int Next;

		public uint Hash;

		public K Key;

		public V Val;
	}

	private static uint[] _primeTableUInt = new uint[30]
	{
		3u, 7u, 17u, 29u, 53u, 97u, 193u, 389u, 769u, 1543u,
		3079u, 6151u, 12289u, 24593u, 49157u, 98317u, 196613u, 393241u, 786433u, 1572869u,
		3145739u, 6291469u, 12582917u, 25165843u, 50331653u, 100663319u, 201326611u, 402653189u, 805306457u, 1610612741u
	};

	private int _freeHead;

	private int _freeCount;

	private int _usedCount;

	private uint _capacity;

	private int[] _buckets;

	private Node[] _nodes;

	private bool isReadOnly;

	private ICollection<K> keys;

	private ICollection<V> values;

	public KeyIterator Keys => new KeyIterator(this);

	ICollection<V> IDictionary<K, V>.Values => values;

	ICollection<K> IDictionary<K, V>.Keys => keys;

	public ValueIterator Values => new ValueIterator(this);

	public int Count => _usedCount - _freeCount - 1;

	public bool IsReadOnly => isReadOnly;

	public uint Capacity => _capacity;

	public V this[K key]
	{
		get
		{
			int num = FindNode(key);
			if (num != 0)
			{
				return _nodes[num].Val;
			}
			K val = key;
			throw new InvalidOperationException("Key does not exist: " + val);
		}
		set
		{
			int num = FindNode(key);
			if (num == 0)
			{
				Insert(key, value);
				return;
			}
			Assert(_nodes[num].Key.Equals(key));
			_nodes[num].Val = value;
		}
	}

	public NonAllocDictionary(uint capacity = 29u)
	{
		_capacity = (IsPrimeFromList(capacity) ? capacity : GetNextPrime(capacity));
		_usedCount = 1;
		_buckets = new int[_capacity];
		_nodes = new Node[_capacity];
	}

	public bool ContainsKey(K key)
	{
		return FindNode(key) != 0;
	}

	public bool Contains(KeyValuePair<K, V> item)
	{
		int num = FindNode(item.Key);
		if (num >= 0 && EqualityComparer<V>.Default.Equals(_nodes[num].Val, item.Value))
		{
			return true;
		}
		return false;
	}

	public bool TryGetValue(K key, out V val)
	{
		int num = FindNode(key);
		if (num != 0)
		{
			val = _nodes[num].Val;
			return true;
		}
		val = default(V);
		return false;
	}

	public void Set(K key, V val)
	{
		int num = FindNode(key);
		if (num == 0)
		{
			Insert(key, val);
			return;
		}
		Assert(_nodes[num].Key.Equals(key));
		_nodes[num].Val = val;
	}

	public void Add(K key, V val)
	{
		if (FindNode(key) == 0)
		{
			Insert(key, val);
			return;
		}
		K val2 = key;
		throw new InvalidOperationException("Duplicate key " + val2);
	}

	public void Add(KeyValuePair<K, V> item)
	{
		if (FindNode(item.Key) == 0)
		{
			Insert(item.Key, item.Value);
			return;
		}
		throw new InvalidOperationException("Duplicate key " + item.Key);
	}

	public bool Remove(K key)
	{
		uint hashCode = (uint)key.GetHashCode();
		int num = _buckets[hashCode % _capacity];
		int num2 = 0;
		while (num != 0)
		{
			if (_nodes[num].Hash == hashCode && _nodes[num].Key.Equals(key))
			{
				if (num2 == 0)
				{
					_buckets[hashCode % _capacity] = _nodes[num].Next;
				}
				else
				{
					_nodes[num2].Next = _nodes[num].Next;
				}
				_nodes[num].Used = false;
				_nodes[num].Next = _freeHead;
				_nodes[num].Val = default(V);
				_freeHead = num;
				_freeCount++;
				return true;
			}
			num2 = num;
			num = _nodes[num].Next;
		}
		return false;
	}

	public bool Remove(KeyValuePair<K, V> item)
	{
		if (Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
	{
		return new PairIterator(this);
	}

	public PairIterator GetEnumerator()
	{
		return new PairIterator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new PairIterator(this);
	}

	private int FindNode(K key)
	{
		uint hashCode = (uint)key.GetHashCode();
		for (int num = _buckets[hashCode % _capacity]; num != 0; num = _nodes[num].Next)
		{
			if (_nodes[num].Hash == hashCode && _nodes[num].Key.Equals(key))
			{
				return num;
			}
		}
		return 0;
	}

	private void Insert(K key, V val)
	{
		int num = 0;
		if (_freeCount > 0)
		{
			num = _freeHead;
			_freeHead = _nodes[num].Next;
			_freeCount--;
		}
		else
		{
			if (_usedCount == _capacity)
			{
				Expand();
			}
			num = _usedCount++;
		}
		uint hashCode = (uint)key.GetHashCode();
		uint num2 = hashCode % _capacity;
		_nodes[num].Used = true;
		_nodes[num].Hash = hashCode;
		_nodes[num].Next = _buckets[num2];
		_nodes[num].Key = key;
		_nodes[num].Val = val;
		_buckets[num2] = num;
	}

	private void Expand()
	{
		Assert(_buckets.Length == _usedCount);
		uint nextPrime = GetNextPrime(_capacity);
		Assert(nextPrime > _capacity);
		int[] array = new int[nextPrime];
		Node[] array2 = new Node[nextPrime];
		Array.Copy(_nodes, 0, array2, 0, _nodes.Length);
		for (int i = 1; i < _nodes.Length; i++)
		{
			Assert(array2[i].Used);
			uint num = array2[i].Hash % nextPrime;
			array2[i].Next = array[num];
			array[num] = i;
		}
		_nodes = array2;
		_buckets = array;
		_capacity = nextPrime;
	}

	public void Clear()
	{
		if (_usedCount > 1)
		{
			Array.Clear(_nodes, 0, _nodes.Length);
			Array.Clear(_buckets, 0, _buckets.Length);
			_freeHead = 0;
			_freeCount = 0;
			_usedCount = 1;
		}
	}

	void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index > array.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (array.Length - index < Count)
		{
			throw new ArgumentException("Array plus offset are too small to fit all items in.");
		}
		for (int i = 1; i < _nodes.Length; i++)
		{
			if (_nodes[i].Used)
			{
				array[index++] = new KeyValuePair<K, V>(_nodes[i].Key, _nodes[i].Val);
			}
		}
	}

	private static bool IsPrimeFromList(uint value)
	{
		for (int i = 0; i < _primeTableUInt.Length; i++)
		{
			if (_primeTableUInt[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	private static uint GetNextPrime(uint value)
	{
		for (int i = 0; i < _primeTableUInt.Length; i++)
		{
			if (_primeTableUInt[i] > value)
			{
				return _primeTableUInt[i];
			}
		}
		throw new InvalidOperationException("NonAllocDictionary can't get larger than" + _primeTableUInt[_primeTableUInt.Length - 1]);
	}

	private static void Assert(bool condition)
	{
		if (!condition)
		{
			throw new InvalidOperationException("Assert Failed");
		}
	}
}
