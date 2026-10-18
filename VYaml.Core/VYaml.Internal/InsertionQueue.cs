using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal;

internal class InsertionQueue<T>
{
	private const int MinimumGrow = 4;

	private const int GrowFactor = 200;

	private T[] array;

	private int headIndex;

	private int tailIndex;

	public int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get;
		private set; }

	public InsertionQueue(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		array = new T[capacity];
		headIndex = (tailIndex = (Count = 0));
	}

	public void Clear()
	{
		int num2 = (Count = 0);
		headIndex = (tailIndex = num2);
	}

	public T Peek()
	{
		if (Count == 0)
		{
			ThrowForEmptyQueue();
		}
		return array[headIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Enqueue(T item)
	{
		if (Count == array.Length)
		{
			Grow();
		}
		array[tailIndex] = item;
		MoveNext(ref tailIndex);
		Count++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Dequeue()
	{
		if (Count == 0)
		{
			ThrowForEmptyQueue();
		}
		T result = array[headIndex];
		MoveNext(ref headIndex);
		Count--;
		return result;
	}

	public void Insert(int posTo, T item)
	{
		if (Count == array.Length)
		{
			Grow();
		}
		MoveNext(ref tailIndex);
		Count++;
		for (int num = Count - 1; num > posTo; num--)
		{
			int num2 = (headIndex + num) % array.Length;
			int num3 = ((num2 == 0) ? (array.Length - 1) : (num2 - 1));
			array[num2] = array[num3];
		}
		array[(posTo + headIndex) % array.Length] = item;
	}

	private void Grow()
	{
		int num = (int)((long)array.Length * 200L / 100);
		if (num < array.Length + 4)
		{
			num = array.Length + 4;
		}
		SetCapacity(num);
	}

	private void SetCapacity(int capacity)
	{
		T[] destinationArray = new T[capacity];
		if (Count > 0)
		{
			if (headIndex < tailIndex)
			{
				Array.Copy(array, headIndex, destinationArray, 0, Count);
			}
			else
			{
				Array.Copy(array, headIndex, destinationArray, 0, array.Length - headIndex);
				Array.Copy(array, 0, destinationArray, array.Length - headIndex, tailIndex);
			}
		}
		array = destinationArray;
		headIndex = 0;
		tailIndex = ((Count != capacity) ? Count : 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void MoveNext(ref int index)
	{
		index = (index + 1) % array.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ThrowForEmptyQueue()
	{
		throw new InvalidOperationException("EmptyQueue");
	}
}
