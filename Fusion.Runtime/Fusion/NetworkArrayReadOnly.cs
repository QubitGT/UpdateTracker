using System;

namespace Fusion;

public readonly ref struct NetworkArrayReadOnly<T>
{
	private unsafe readonly byte* _array;

	private readonly int _length;

	private readonly IElementReaderWriter<T> _readerWriter;

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
	}

	internal unsafe NetworkArrayReadOnly(byte* array, int length, IElementReaderWriter<T> readerWriter)
	{
		_array = array;
		_length = length;
		_readerWriter = readerWriter;
	}
}
