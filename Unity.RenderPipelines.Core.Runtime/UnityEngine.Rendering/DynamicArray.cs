using System;
using System.Diagnostics;

namespace UnityEngine.Rendering;

[DebuggerDisplay("Size = {size} Capacity = {capacity}")]
public class DynamicArray<T> where T : new()
{
	public struct Iterator
	{
		private readonly DynamicArray<T> owner;

		private int index;

		public ref T Current => ref owner[index];

		public Iterator(DynamicArray<T> setOwner)
		{
			owner = setOwner;
			index = -1;
		}

		public bool MoveNext()
		{
			index++;
			return index < owner.size;
		}

		public void Reset()
		{
			index = -1;
		}
	}

	public struct RangeEnumerable
	{
		public struct RangeIterator
		{
			private readonly DynamicArray<T> owner;

			private int index;

			private int first;

			private int last;

			public ref T Current => ref owner[index];

			public RangeIterator(DynamicArray<T> setOwner, int first, int numItems)
			{
				owner = setOwner;
				this.first = first;
				index = first - 1;
				last = first + numItems;
			}

			public bool MoveNext()
			{
				index++;
				return index < last;
			}

			public void Reset()
			{
				index = first - 1;
			}
		}

		public RangeIterator iterator;

		public RangeIterator GetEnumerator()
		{
			return iterator;
		}
	}

	public delegate int SortComparer(T x, T y);

	protected T[] m_Array;

	public int size { get; protected set; }

	public int capacity => m_Array.Length;

	public ref T this[int index] => ref m_Array[index];

	public DynamicArray()
	{
		m_Array = new T[32];
		size = 0;
	}

	public DynamicArray(int size)
	{
		m_Array = new T[size];
		this.size = size;
	}

	public DynamicArray(int capacity, bool resize)
	{
		m_Array = new T[capacity];
		size = (resize ? capacity : 0);
	}

	public DynamicArray(DynamicArray<T> deepCopy)
	{
		m_Array = new T[deepCopy.size];
		size = deepCopy.size;
		Array.Copy(deepCopy.m_Array, m_Array, size);
	}

	public void Clear()
	{
		size = 0;
	}

	public bool Contains(T item)
	{
		return IndexOf(item) != -1;
	}

	public int Add(in T value)
	{
		int num = size;
		if (num >= m_Array.Length)
		{
			T[] array = new T[Math.Max(m_Array.Length * 2, 1)];
			Array.Copy(m_Array, array, m_Array.Length);
			m_Array = array;
		}
		m_Array[num] = value;
		size++;
		BumpVersion();
		return num;
	}

	public void AddRange(DynamicArray<T> array)
	{
		int num = array.size;
		Reserve(size + num, keepContent: true);
		for (int i = 0; i < num; i++)
		{
			m_Array[size++] = array[i];
		}
		BumpVersion();
	}

	public void Insert(int index, T item)
	{
		if (index == size)
		{
			Add(in item);
			return;
		}
		Resize(size + 1, keepContent: true);
		Array.Copy(m_Array, index, m_Array, index + 1, size - index);
		m_Array[index] = item;
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num != -1)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if (index != size - 1)
		{
			Array.Copy(m_Array, index + 1, m_Array, index, size - index - 1);
		}
		size--;
		BumpVersion();
	}

	public void RemoveRange(int index, int count)
	{
		if (count != 0)
		{
			Array.Copy(m_Array, index + count, m_Array, index, size - index - count);
			size -= count;
			BumpVersion();
		}
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		int num = startIndex;
		while (num < size && count > 0)
		{
			if (match(m_Array[num]))
			{
				return num;
			}
			num++;
			count--;
		}
		return -1;
	}

	public int FindIndex(Predicate<T> match)
	{
		return FindIndex(0, size, match);
	}

	public int IndexOf(T item, int index, int count)
	{
		int num = index;
		while (num < size && count > 0)
		{
			if (m_Array[num].Equals(item))
			{
				return num;
			}
			num++;
			count--;
		}
		return -1;
	}

	public int IndexOf(T item, int index)
	{
		for (int i = index; i < size; i++)
		{
			if (m_Array[i].Equals(item))
			{
				return i;
			}
		}
		return -1;
	}

	public int IndexOf(T item)
	{
		return IndexOf(item, 0);
	}

	public void Resize(int newSize, bool keepContent = false)
	{
		Reserve(newSize, keepContent);
		size = newSize;
		BumpVersion();
	}

	public void ResizeAndClear(int newSize)
	{
		if (newSize > m_Array.Length)
		{
			Reserve(newSize);
		}
		else
		{
			Array.Clear(m_Array, 0, newSize);
		}
		size = newSize;
		BumpVersion();
	}

	public void Reserve(int newCapacity, bool keepContent = false)
	{
		if (newCapacity > m_Array.Length)
		{
			if (keepContent)
			{
				T[] array = new T[newCapacity];
				Array.Copy(m_Array, array, m_Array.Length);
				m_Array = array;
			}
			else
			{
				m_Array = new T[newCapacity];
			}
		}
	}

	[Obsolete("This is deprecated because it returns an incorrect value. It may returns an array with elements beyond the size. Please use Span/ReadOnly if you want safe raw access to the DynamicArray memory.", false)]
	public static implicit operator T[](DynamicArray<T> array)
	{
		return array.m_Array;
	}

	public static implicit operator ReadOnlySpan<T>(DynamicArray<T> array)
	{
		return new ReadOnlySpan<T>(array.m_Array, 0, array.size);
	}

	public static implicit operator Span<T>(DynamicArray<T> array)
	{
		return new Span<T>(array.m_Array, 0, array.size);
	}

	public Iterator GetEnumerator()
	{
		return new Iterator(this);
	}

	public RangeEnumerable SubRange(int first, int numItems)
	{
		RangeEnumerable result = default(RangeEnumerable);
		result.iterator = new RangeEnumerable.RangeIterator(this, first, numItems);
		return result;
	}

	protected internal void BumpVersion()
	{
	}
}
