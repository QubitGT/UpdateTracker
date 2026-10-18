#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

internal class RingBuffer<T> : IEnumerable<T>, IEnumerable where T : struct
{
	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__29 : IEnumerator<T>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		public RingBuffer<T> _003C_003E4__this;

		private IList<ArraySegment<T>> _003Csegments_003E5__1;

		private IEnumerator<ArraySegment<T>> _003C_003Es__2;

		private ArraySegment<T> _003Csegment_003E5__3;

		private int _003Ci_003E5__4;

		T IEnumerator<T>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetEnumerator_003Ed__29(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Csegments_003E5__1 = null;
			_003C_003Es__2 = null;
			_003Csegment_003E5__3 = default(ArraySegment<T>);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					_003Ci_003E5__4++;
					goto IL_00c0;
				}
				_003C_003E1__state = -1;
				_003Csegments_003E5__1 = _003C_003E4__this.ToArraySegments();
				_003C_003Es__2 = _003Csegments_003E5__1.GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_00e4;
				IL_00c0:
				if (_003Ci_003E5__4 < _003Csegment_003E5__3.Count)
				{
					_003C_003E2__current = _003Csegment_003E5__3.Array[_003Csegment_003E5__3.Offset + _003Ci_003E5__4];
					_003C_003E1__state = 1;
					return true;
				}
				_003Csegment_003E5__3 = default(ArraySegment<T>);
				goto IL_00e4;
				IL_00e4:
				if (_003C_003Es__2.MoveNext())
				{
					_003Csegment_003E5__3 = _003C_003Es__2.Current;
					_003Ci_003E5__4 = 0;
					goto IL_00c0;
				}
				_003C_003Em__Finally1();
				_003C_003Es__2 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003Es__2 != null)
			{
				_003C_003Es__2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private readonly T[] _buffer;

	private int _front;

	private int _count;

	public int Count => _count;

	public int Capacity => _buffer.Length;

	public bool IsEmpty => _count == 0;

	public bool IsFull => _count == _buffer.Length;

	public T this[int index]
	{
		get
		{
			if (IsEmpty)
			{
				throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
			}
			if (index >= _count)
			{
				throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer has {_count} items.");
			}
			int num = InternalIndex(index);
			return _buffer[num];
		}
		set
		{
			if (IsEmpty)
			{
				throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
			}
			if (index >= _count)
			{
				throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer has {_count} items.");
			}
			int num = InternalIndex(index);
			_buffer[num] = value;
		}
	}

	public RingBuffer(int capacity)
		: this(capacity, new T[0])
	{
	}

	public RingBuffer(int capacity, T[] items)
	{
		if (capacity < 1)
		{
			throw new ArgumentException("Buffer cannot have negative or zero capacity.", "capacity");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (items.Length > capacity)
		{
			throw new ArgumentException("Number of items exceeds buffer capacity.", "items");
		}
		_buffer = new T[capacity];
		Array.Copy(items, _buffer, items.Length);
		_front = 0;
		_count = items.Length;
	}

	public ref readonly T Front()
	{
		ThrowIfEmpty();
		return ref _buffer[_front];
	}

	public ref T FrontMut()
	{
		ThrowIfEmpty();
		return ref _buffer[_front];
	}

	public ref readonly T Get(int index)
	{
		if (IsEmpty)
		{
			throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
		}
		if (index >= _count)
		{
			throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer has {_count} items.");
		}
		int num = InternalIndex(index);
		return ref _buffer[num];
	}

	public ref T GetMut(int index)
	{
		if (IsEmpty)
		{
			throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
		}
		if (index >= _count)
		{
			throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer has {_count} items.");
		}
		int num = InternalIndex(index);
		return ref _buffer[num];
	}

	public ref readonly T Back()
	{
		ThrowIfEmpty();
		int num = Decrement(BackIndex());
		return ref _buffer[num];
	}

	public ref T BackMut()
	{
		ThrowIfEmpty();
		int num = Decrement(BackIndex());
		return ref _buffer[num];
	}

	public void PushBack(T item)
	{
		int num = BackIndex();
		if (IsFull)
		{
			_buffer[num] = item;
			_front = Increment(num);
		}
		else
		{
			_buffer[num] = item;
			_count++;
		}
	}

	public void PushFront(T item)
	{
		_front = Decrement(_front);
		if (IsFull)
		{
			_buffer[_front] = item;
			return;
		}
		_buffer[_front] = item;
		_count++;
	}

	public T PopBack()
	{
		ThrowIfEmpty("Cannot take items from an empty buffer.");
		int num = Decrement(BackIndex());
		T result = _buffer[num];
		_buffer[num] = default(T);
		_count--;
		return result;
	}

	public T PopFront()
	{
		ThrowIfEmpty("Cannot take items from an empty buffer.");
		T result = _buffer[_front];
		_buffer[_front] = default(T);
		_front = Increment(_front);
		_count--;
		return result;
	}

	public void Clear()
	{
		_front = 0;
		_count = 0;
		Array.Clear(_buffer, 0, _buffer.Length);
	}

	public IList<ArraySegment<T>> ToArraySegments()
	{
		return new ArraySegment<T>[2]
		{
			SpanOne(),
			SpanTwo()
		};
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		int num = 0;
		IList<ArraySegment<T>> list = ToArraySegments();
		foreach (ArraySegment<T> item in list)
		{
			Array.Copy(item.Array, item.Offset, array, num, item.Count);
			num += item.Count;
		}
		return array;
	}

	[IteratorStateMachine(typeof(RingBuffer<>._003CGetEnumerator_003Ed__29))]
	public IEnumerator<T> GetEnumerator()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEnumerator_003Ed__29(0)
		{
			_003C_003E4__this = this
		};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private int FrontIndex()
	{
		return _front;
	}

	private int BackIndex()
	{
		return (_front + _count) % Capacity;
	}

	private int InternalIndex(int index)
	{
		return (_front + index) % Capacity;
	}

	private int Increment(int index)
	{
		Assert.Check(index >= 0 && index < Capacity);
		return (index != Capacity - 1) ? (index + 1) : 0;
	}

	private int Decrement(int index)
	{
		Assert.Check(index >= 0 && index < Capacity);
		return (index == 0) ? (Capacity - 1) : (index - 1);
	}

	private void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(message);
		}
	}

	private ArraySegment<T> SpanOne()
	{
		int num = BackIndex();
		if (IsEmpty)
		{
			return new ArraySegment<T>(new T[0]);
		}
		if (_front < num)
		{
			return new ArraySegment<T>(_buffer, _front, num - _front);
		}
		return new ArraySegment<T>(_buffer, _front, _buffer.Length - _front);
	}

	private ArraySegment<T> SpanTwo()
	{
		int num = BackIndex();
		if (IsEmpty)
		{
			return new ArraySegment<T>(new T[0]);
		}
		if (_front < num)
		{
			return new ArraySegment<T>(_buffer, num, 0);
		}
		return new ArraySegment<T>(_buffer, 0, num);
	}
}
