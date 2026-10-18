using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Fusion;

[DebuggerDisplay("Length = {Length}")]
[DebuggerTypeProxy(typeof(NetworkArray<>.DebuggerProxy))]
public struct NetworkArray<T> : IEnumerable<T>, IEnumerable, INetworkArray
{
	internal class DebuggerProxy
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Lazy<T[]> _items;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items => _items.Value;

		public unsafe DebuggerProxy(NetworkArray<T> array)
		{
			_items = new Lazy<T[]>(() => (array._array == null) ? Array.Empty<T>() : array.ToArray());
		}
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private int _index;

		private NetworkArray<T> _array;

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

		public Enumerator(NetworkArray<T> array)
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
			_array = default(NetworkArray<T>);
			_index = -1;
		}
	}

	private unsafe byte* _array;

	private int _length;

	private IElementReaderWriter<T> _readerWriter;

	private static StringBuilder _stringBuilderCached;

	public int Length => _length;

	public unsafe T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)_length)
			{
				throw new IndexOutOfRangeException();
			}
			return _readerWriter.Read(_array, index);
		}
		set
		{
			if ((uint)index >= (uint)_length)
			{
				throw new IndexOutOfRangeException();
			}
			_readerWriter.Write(_array, index, value);
		}
	}

	object INetworkArray.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (T)value;
		}
	}

	public unsafe NetworkArray(byte* array, int length, IElementReaderWriter<T> readerWriter)
	{
		_array = array;
		_length = length;
		_readerWriter = readerWriter;
	}

	public unsafe NetworkArrayReadOnly<T> ToReadOnly()
	{
		return new NetworkArrayReadOnly<T>(_array, _length, _readerWriter);
	}

	public T Get(int index)
	{
		return this[index];
	}

	public T Set(int index, T value)
	{
		this[index] = value;
		return value;
	}

	internal unsafe ref T GetRef(int index)
	{
		if ((uint)index >= (uint)_length)
		{
			throw new IndexOutOfRangeException();
		}
		return ref _readerWriter.ReadRef(_array, index);
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

	public void CopyTo(NetworkArray<T> array)
	{
		int length = array.Length;
		if (array.Length > length)
		{
			throw new ArgumentException($"Max array length {_length}, got: {length}", "array");
		}
		for (int i = 0; i < length; i++)
		{
			array[i] = this[i];
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
			if (typeof(T).IsValueType)
			{
				stringBuilderCached.Append(Get(num).ToString());
			}
			else
			{
				stringBuilderCached.Append(Get(num));
			}
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

	public unsafe static implicit operator NetworkArrayReadOnly<T>(NetworkArray<T> value)
	{
		return new NetworkArrayReadOnly<T>(value._array, value.Length, value._readerWriter);
	}
}
