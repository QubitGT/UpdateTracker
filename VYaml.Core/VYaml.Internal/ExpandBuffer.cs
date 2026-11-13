using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal;

internal class ExpandBuffer<T>
{
	private const int MinimumGrow = 4;

	private const int GrowFactor = 200;

	private T[] buffer;

	public int Length { get; private set; }

	public ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref buffer[index];
		}
	}

	public ExpandBuffer(int capacity)
	{
		buffer = new T[capacity];
		Length = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> AsSpan()
	{
		return MemoryExtensions.AsSpan(buffer, 0, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> AsSpan(int length)
	{
		if (length > buffer.Length)
		{
			SetCapacity(buffer.Length * 2);
		}
		return MemoryExtensions.AsSpan(buffer, 0, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		Length = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref T Peek()
	{
		return ref buffer[Length - 1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref T Pop()
	{
		if (Length == 0)
		{
			throw new InvalidOperationException("Cannot pop the empty buffer");
		}
		return ref buffer[--Length];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPop(out T value)
	{
		if (Length == 0)
		{
			value = default(T);
			return false;
		}
		value = Pop();
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T item)
	{
		if (Length >= buffer.Length)
		{
			Grow();
		}
		buffer[Length++] = item;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetCapacity(int newCapacity)
	{
		if (buffer.Length < newCapacity)
		{
			T[] array = new T[newCapacity];
			MemoryExtensions.AsSpan(buffer, 0, Length).CopyTo(array);
			buffer = array;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Grow()
	{
		int num = buffer.Length * 200 / 100;
		if (num < buffer.Length + 4)
		{
			num = buffer.Length + 4;
		}
		SetCapacity(num);
	}
}
