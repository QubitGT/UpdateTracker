using System;

namespace Oculus.Interaction;

public class RingBuffer<T>
{
	private readonly T[] _buffer;

	private readonly int _capacity;

	private int _head;

	private int _count;

	public int Count => _count;

	public int Capacity => _capacity;

	public T this[int index]
	{
		get
		{
			if (_count == 0)
			{
				throw new InvalidOperationException("The buffer is empty.");
			}
			return _buffer[(index % _count + _count) % _count];
		}
	}

	public RingBuffer(int capacity)
	{
		_buffer = new T[capacity];
		_capacity = capacity;
		Clear();
	}

	public void Clear()
	{
		_head = -1;
		_count = 0;
	}

	public void Add(T item)
	{
		_head = (_head + 1) % _capacity;
		_buffer[_head] = item;
		if (_count < _capacity)
		{
			_count++;
		}
	}

	public T Peek(int offset = 0)
	{
		if (_count == 0)
		{
			throw new InvalidOperationException("The buffer is empty.");
		}
		return _buffer[((_head + offset) % _count + _count) % _count];
	}
}
