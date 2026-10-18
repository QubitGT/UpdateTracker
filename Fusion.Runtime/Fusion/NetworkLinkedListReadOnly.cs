#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fusion;

public ref struct NetworkLinkedListReadOnly<T>
{
	private const int COUNT = 0;

	private const int HEAD = 1;

	private const int TAIL = 2;

	private const int PREV = 0;

	private const int NEXT = 1;

	private const int INVALID = 0;

	private const int OFFSET = 1;

	public const int ELEMENT_WORDS = 2;

	public const int META_WORDS = 3;

	private unsafe int* _data;

	private int _stride;

	private int _capacity;

	private IElementReaderWriter<T> _rw;

	private unsafe int Head
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _data[1];
		}
	}

	private unsafe int Tail
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _data[2];
		}
	}

	public unsafe int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return *_data;
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
	}

	internal unsafe NetworkLinkedListReadOnly(byte* data, int capacity, IElementReaderWriter<T> rw)
	{
		Assert.Check(Native.IsPointerAligned(data, 4));
		_rw = rw;
		_data = (int*)data;
		_capacity = capacity;
		_stride = rw.GetElementWordCount() + 2;
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
}
