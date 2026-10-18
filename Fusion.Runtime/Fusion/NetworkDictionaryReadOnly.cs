#define DEBUG
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fusion;

public readonly ref struct NetworkDictionaryReadOnly<K, V>
{
	private const int INVALID_ENTRY = 0;

	private const int FREE_OFFSET = 0;

	private const int FREE_COUNT_OFFSET = 1;

	private const int USED_COUNT_OFFSET = 2;

	private unsafe readonly int* _data;

	private readonly int _capacity;

	private readonly int _nxtOffset;

	private readonly int _keyOffset;

	private readonly int _valOffset;

	private readonly int _entryStride;

	private readonly int _bucketsOffset;

	private readonly int _entriesOffset;

	private readonly IElementReaderWriter<K> _keyReaderWriter;

	private readonly IElementReaderWriter<V> _valReaderWriter;

	private readonly EqualityComparer<K> _equalityComparer;

	private unsafe int _free => *_data;

	private unsafe int _freeCount => _data[1];

	private unsafe int _usedCount => _data[2];

	public unsafe int Count
	{
		get
		{
			Assert.Check(_data);
			return _usedCount - _freeCount - 1;
		}
	}

	public int Capacity => _capacity - 1;

	internal unsafe NetworkDictionaryReadOnly(int* data, int capacity, IElementReaderWriter<K> keyReaderWriter, IElementReaderWriter<V> valReaderWriter)
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
	}

	public V Get(K key)
	{
		if (TryGet(key, out var value))
		{
			return value;
		}
		throw new KeyNotFoundException();
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

	private unsafe int Find(K key)
	{
		Assert.Check(_capacity > 0);
		int* ptr = _data + _bucketsOffset;
		uint bucketFromHashCode = GetBucketFromHashCode(key.GetHashCode());
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
	private unsafe K GetKey(int entry)
	{
		return _keyReaderWriter.Read((byte*)(_data + _entriesOffset + (_entryStride * entry + _keyOffset)), 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe V GetVal(int entry)
	{
		return _valReaderWriter.Read((byte*)(_data + _entriesOffset + (_entryStride * entry + _valOffset)), 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe int GetNxt(int entry)
	{
		return (_data + _entriesOffset)[_entryStride * entry + _nxtOffset];
	}
}
