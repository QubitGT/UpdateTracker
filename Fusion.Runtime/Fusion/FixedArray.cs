using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Fusion;

[DebuggerDisplay("Length = {Length}")]
public struct FixedArray<T> : IEnumerable<T>, IEnumerable where T : unmanaged
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private int _index;

		private FixedArray<T> _array;

		public T Current
		{
			get
			{
				if ((uint)_index < (uint)_array.Length)
				{
					return _array[_index];
				}
				throw new IndexOutOfRangeException();
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(FixedArray<T> array)
		{
			_index = -1;
			_array = array;
		}

		public bool MoveNext()
		{
			return ++_index < _array.Length;
		}

		public void Reset()
		{
			_index = -1;
		}

		public void Dispose()
		{
			_array = default(FixedArray<T>);
			_index = -1;
		}
	}

	private unsafe T* _array;

	private int _length;

	private static StringBuilder _stringBuilderCached;

	public int Length => _length;

	public unsafe ref T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)_length)
			{
				throw new IndexOutOfRangeException();
			}
			return ref _array[index];
		}
	}

	public unsafe FixedArray(T* array, int length)
	{
		_array = array;
		_length = length;
	}

	public T[] ToArray()
	{
		T[] array = new T[_length];
		for (int i = 0; i < _length; i++)
		{
			array[i] = this[i];
		}
		return array;
	}

	public void CopyTo(List<T> list)
	{
		for (int i = 0; i < _length; i++)
		{
			list.Add(this[i]);
		}
	}

	public void CopyTo(T[] array, bool throwIfOverflow = true)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = array.Length;
		if (array.Length > num)
		{
			if (throwIfOverflow)
			{
				throw new ArgumentException($"Max array length {_length}, got: {num}", "array");
			}
			num = _length;
		}
		for (int i = 0; i < num; i++)
		{
			array[i] = this[i];
		}
	}

	public override string ToString()
	{
		return ToListString();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		for (int i = 0; i < _length; i++)
		{
			this[i] = default(T);
		}
	}

	public void CopyFrom(T[] source, int sourceOffset, int sourceCount)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sourceCount > _length)
		{
			throw new ArgumentException($"Max array length {_length}, got: {sourceCount}", "source");
		}
		if (source.Length < sourceOffset + sourceCount)
		{
			throw new ArgumentOutOfRangeException($"Source length is {sourceCount}, but offset ({sourceOffset}) and count {sourceCount}) are out of bounds", "sourceCount");
		}
		for (int i = 0; i < sourceCount; i++)
		{
			this[i] = source[i + sourceOffset];
		}
	}

	public void CopyFrom(List<T> source, int sourceOffset, int sourceCount)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sourceCount > _length)
		{
			throw new ArgumentException($"Max array length {_length}, got: {sourceCount}", "source");
		}
		if (source.Count < sourceOffset + sourceCount)
		{
			throw new ArgumentOutOfRangeException($"Source length is {sourceCount}, but offset ({sourceOffset}) and count {sourceCount}) are out of bounds", "sourceCount");
		}
		for (int i = 0; i < sourceCount; i++)
		{
			this[i] = source[i + sourceOffset];
		}
	}

	private unsafe string ToListString()
	{
		if (_length == 0)
		{
			return null;
		}
		if (_array == null)
		{
			return null;
		}
		if (_stringBuilderCached == null)
		{
			_stringBuilderCached = new StringBuilder();
		}
		else
		{
			_stringBuilderCached.Clear();
		}
		StringBuilder stringBuilderCached = _stringBuilderCached;
		int num = 0;
		while (true)
		{
			stringBuilderCached.Append(this[num].ToString());
			num++;
			if (num == _length)
			{
				break;
			}
			stringBuilderCached.Append("\n");
			bool flag = true;
		}
		return stringBuilderCached.ToString();
	}
}
public static class FixedArray
{
	public unsafe static FixedArray<T> CreateFromFieldSequence<T>(ref T firstField, ref T lastField) where T : unmanaged
	{
		fixed (T* ptr = &firstField)
		{
			fixed (T* ptr2 = &lastField)
			{
				return new FixedArray<T>(ptr, (int)(ptr2 - ptr) + 1);
			}
		}
	}

	public unsafe static FixedArray<T> Create<T>(ref T firstField, int length) where T : unmanaged
	{
		fixed (T* array = &firstField)
		{
			return new FixedArray<T>(array, length);
		}
	}

	public unsafe static FixedArray<TAdapted> Create<TActual, TAdapted>(ref TActual firstField, int length) where TActual : unmanaged where TAdapted : unmanaged
	{
		fixed (TActual* array = &firstField)
		{
			return new FixedArray<TAdapted>((TAdapted*)array, length);
		}
	}

	public static int IndexOf<T>(this FixedArray<T> array, T elem) where T : unmanaged, IEquatable<T>
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Equals(elem))
			{
				return i;
			}
		}
		return -1;
	}
}
