#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fusion;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(NetworkLinkedList<>.DebuggerProxy))]
public struct NetworkLinkedList<T> : IEnumerable<T>, IEnumerable, INetworkLinkedList
{
	internal class DebuggerProxy
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Lazy<T[]> _items;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items => _items.Value;

		public unsafe DebuggerProxy(NetworkLinkedList<T> list)
		{
			_items = new Lazy<T[]>(() => (list._data == null) ? Array.Empty<T>() : list.ToArray());
		}
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private bool _first;

		private int _head;

		private NetworkLinkedList<T> _list;

		public unsafe T Current
		{
			get
			{
				if (_head > 0 && _head <= _list._capacity)
				{
					return _list.Read(_list.Entry(_head));
				}
				throw new InvalidOperationException();
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(NetworkLinkedList<T> list)
		{
			_list = list;
			_head = 0;
			_first = true;
		}

		public unsafe bool MoveNext()
		{
			if (_first)
			{
				_first = false;
				_head = _list.Head;
			}
			else
			{
				if (_head == 0)
				{
					return false;
				}
				_head = _list.Entry(_head)[1];
			}
			return _head > 0 && _head <= _list._capacity;
		}

		public void Reset()
		{
			_head = 0;
		}

		public void Dispose()
		{
			_list = default(NetworkLinkedList<T>);
			_head = -1;
		}
	}

	public const int ELEMENT_WORDS = 2;

	public const int META_WORDS = 3;

	private unsafe int* _data;

	private int _stride;

	private int _capacity;

	private IElementReaderWriter<T> _rw;

	private const int COUNT = 0;

	private const int HEAD = 1;

	private const int TAIL = 2;

	private const int PREV = 0;

	private const int NEXT = 1;

	private const int INVALID = 0;

	private const int OFFSET = 1;

	private unsafe int Head
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _data[1];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			_data[1] = value;
		}
	}

	private unsafe int Tail
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _data[2];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			_data[2] = value;
		}
	}

	public unsafe int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return *_data;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private set
		{
			*_data = value;
		}
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _capacity;
		}
	}

	public unsafe T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Read(GetEntryByListIndex(index));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			Write(GetEntryByListIndex(index), value);
		}
	}

	public unsafe NetworkLinkedList(byte* data, int capacity, IElementReaderWriter<T> rw)
	{
		Assert.Check(Native.IsPointerAligned(data, 4));
		_rw = rw;
		_data = (int*)data;
		_capacity = capacity;
		_stride = rw.GetElementWordCount() + 2;
	}

	public unsafe NetworkLinkedList<T> Remap(void* list)
	{
		Assert.Check(_data);
		Assert.Check(_capacity > 0);
		Assert.Check(_rw);
		NetworkLinkedList<T> result = this;
		result._data = (int*)list;
		return result;
	}

	public unsafe void Clear()
	{
		Native.MemClear(_data, (3 + _stride * Capacity) * 4);
	}

	public bool Contains(T value)
	{
		return Contains(value, EqualityComparer<T>.Default);
	}

	public unsafe bool Contains(T value, IEqualityComparer<T> comparer)
	{
		int num = Head;
		while (num != 0)
		{
			int* ptr = Entry(num);
			T x = Read(ptr);
			if (comparer.Equals(x, value))
			{
				return true;
			}
			num = ptr[1];
		}
		return false;
	}

	public unsafe T Set(int index, T value)
	{
		Write(GetEntryByListIndex(index), value);
		return value;
	}

	public unsafe T Get(int index)
	{
		return Read(GetEntryByListIndex(index));
	}

	public int IndexOf(T value)
	{
		return IndexOf(value, EqualityComparer<T>.Default);
	}

	public unsafe int IndexOf(T value, IEqualityComparer<T> equalityComparer)
	{
		for (int i = 0; i < Capacity; i++)
		{
			if (equalityComparer.Equals(Read(GetEntryByListIndex(i)), value))
			{
				return i;
			}
		}
		return -1;
	}

	public bool Remove(T value)
	{
		return Remove(value, EqualityComparer<T>.Default);
	}

	public unsafe bool Remove(T value, IEqualityComparer<T> equalityComparer)
	{
		int num = Head;
		while (num != 0)
		{
			int* ptr = Entry(num);
			T x = Read(ptr);
			if (equalityComparer.Equals(x, value))
			{
				RemoveEntry(ptr, num);
				return true;
			}
			num = ptr[1];
		}
		return false;
	}

	public unsafe void Add(T value)
	{
		Assert.Check((uint)Count <= Capacity);
		if (Count == Capacity)
		{
			throw new InvalidOperationException("NetworkList is full");
		}
		int index;
		int* ptr = FindFreeEntry(out index);
		Assert.Check(ptr != null);
		int count = Count + 1;
		Count = count;
		Write(ptr, value);
		*ptr = Tail;
		ptr[1] = 0;
		if (Tail != 0)
		{
			Entry(Tail)[1] = index;
			Tail = index;
		}
		else
		{
			Head = index;
			Tail = index;
		}
	}

	private unsafe int* FindFreeEntry(out int index)
	{
		for (int i = 0; i < _capacity; i++)
		{
			int num = i + 1;
			if (num != Head && num != Tail)
			{
				int* ptr = Entry(num);
				if (*ptr == 0)
				{
					index = num;
					return ptr;
				}
			}
		}
		Assert.AlwaysFail("No free entry");
		index = 0;
		return null;
	}

	private unsafe void RemoveEntry(int* entry, int entryIndex)
	{
		Assert.Check((uint)Count <= Capacity);
		Assert.Check(Entry(entryIndex) == entry);
		if (*entry != 0)
		{
			Entry(*entry)[1] = entry[1];
		}
		if (entry[1] != 0)
		{
			*Entry(entry[1]) = *entry;
		}
		if (Tail == entryIndex)
		{
			Tail = *entry;
		}
		if (Head == entryIndex)
		{
			Head = entry[1];
		}
		*entry = 0;
		entry[1] = 0;
		int count = Count - 1;
		Count = count;
	}

	private unsafe int* GetEntryByListIndex(int listIndex)
	{
		int num = listIndex;
		int num2 = Head;
		while (num2 != 0)
		{
			int* ptr = Entry(num2);
			if (listIndex == 0)
			{
				return ptr;
			}
			num2 = ptr[1];
			listIndex--;
		}
		throw new IndexOutOfRangeException(num.ToString());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe int* Entry(int index)
	{
		Assert.Check(index >= 1 && index <= _capacity);
		return _data + 3 + _stride * (index - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe T Read(int* entry)
	{
		return _rw.Read((byte*)(entry + 2), 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void Write(int* entry, T value)
	{
		_rw.Write((byte*)(entry + 2), 0, value);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}

	void INetworkLinkedList.Add(object item)
	{
		Add((T)item);
	}
}
