using System;
using UnityEngine;

namespace Meta.WitAi.Data;

public class RingBuffer<T>
{
	public delegate void OnDataAdded(T[] data, int offset, int length);

	public delegate void ByteDataWriter(T[] buffer, int offset, int length);

	public class Marker
	{
		private long bufferDataIndex;

		private int index;

		private readonly RingBuffer<T> ringBuffer;

		public RingBuffer<T> RingBuffer => ringBuffer;

		public bool IsValid => ringBuffer.bufferDataLength - bufferDataIndex <= ringBuffer.Capacity;

		public long AvailableByteCount => Math.Min(ringBuffer.Capacity, RequestedByteCount);

		public long RequestedByteCount => ringBuffer.bufferDataLength - bufferDataIndex;

		public long CurrentBufferDataIndex => bufferDataIndex;

		public Marker(RingBuffer<T> ringBuffer, long markerPosition, int bufIndex)
		{
			this.ringBuffer = ringBuffer;
			bufferDataIndex = markerPosition;
			index = bufIndex;
		}

		public int Read(T[] buffer, int offset, int length, bool skipToNextValid = false)
		{
			int num = -1;
			if (!IsValid && skipToNextValid && ringBuffer.bufferDataLength > ringBuffer.Capacity)
			{
				bufferDataIndex = ringBuffer.bufferDataLength - ringBuffer.Capacity;
			}
			if (IsValid)
			{
				num = ringBuffer.Read(buffer, offset, length, bufferDataIndex);
				bufferDataIndex += num;
				index += num;
				if (index > buffer.Length)
				{
					index -= buffer.Length;
				}
			}
			return num;
		}

		public void ReadIntoWriters(params ByteDataWriter[] writers)
		{
			if (!IsValid && ringBuffer.bufferDataLength > ringBuffer.Capacity)
			{
				bufferDataIndex = ringBuffer.bufferDataLength - ringBuffer.Capacity;
			}
			index = ringBuffer.GetBufferArrayIndex(bufferDataIndex);
			int num = (int)(ringBuffer.bufferDataLength - bufferDataIndex);
			if (IsValid && num > 0)
			{
				for (int i = 0; i < writers.Length; i++)
				{
					ringBuffer.WriteFromBuffer(writers[i], index, num);
				}
			}
			bufferDataIndex += num;
			index = ringBuffer.GetBufferArrayIndex(bufferDataIndex);
		}

		public Marker Clone()
		{
			return new Marker(ringBuffer, bufferDataIndex, index);
		}

		public void Offset(int amount)
		{
			bufferDataIndex += amount;
			if (bufferDataIndex < 0)
			{
				bufferDataIndex = 0L;
			}
			if (bufferDataIndex > ringBuffer.bufferDataLength)
			{
				bufferDataIndex = ringBuffer.bufferDataLength;
			}
			index = ringBuffer.GetBufferArrayIndex(bufferDataIndex);
		}
	}

	public OnDataAdded OnDataAddedEvent;

	private readonly T[] buffer;

	private int bufferIndex;

	private long bufferDataLength;

	public int Capacity => buffer.Length;

	public T this[long bufferDataIndex] => buffer[GetBufferArrayIndex(bufferDataIndex)];

	public int GetBufferArrayIndex(long bufferDataIndex)
	{
		if (bufferDataLength <= bufferDataIndex)
		{
			return -1;
		}
		if (bufferDataLength - bufferDataIndex > buffer.Length)
		{
			return -1;
		}
		long num = bufferDataLength - bufferDataIndex;
		long num2 = bufferIndex - num;
		if (num2 < 0)
		{
			num2 = buffer.Length + num2;
		}
		return (int)num2;
	}

	public void Clear(bool eraseData = false)
	{
		bufferIndex = 0;
		bufferDataLength = 0L;
		if (eraseData)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = default(T);
			}
		}
	}

	public RingBuffer(int capacity)
	{
		buffer = new T[capacity];
	}

	private int CopyToBuffer(T[] data, int offset, int length, int newBufferIndex)
	{
		if (length > buffer.Length)
		{
			throw new ArgumentException("Push data exceeds buffer size.");
		}
		if (newBufferIndex + length < buffer.Length)
		{
			Array.Copy(data, offset, buffer, newBufferIndex, length);
			return newBufferIndex + length;
		}
		int num = Mathf.Min(length, buffer.Length);
		int num2 = buffer.Length - newBufferIndex;
		int num3 = num - num2;
		try
		{
			Array.Copy(data, offset, buffer, newBufferIndex, num2);
			Array.Copy(data, offset + num2, buffer, 0, num3);
			return num3;
		}
		catch (ArgumentException ex)
		{
			throw ex;
		}
	}

	public void WriteFromBuffer(ByteDataWriter writer, long newBufferIndex, int length)
	{
		lock (buffer)
		{
			if (newBufferIndex + length < buffer.Length)
			{
				writer(buffer, (int)newBufferIndex, length);
				return;
			}
			if (length > bufferDataLength)
			{
				length = (int)(bufferDataLength - newBufferIndex);
			}
			if (length > buffer.Length)
			{
				length = buffer.Length;
			}
			int num = Math.Min(buffer.Length, length);
			int num2 = (int)(buffer.Length - newBufferIndex);
			int length2 = num - num2;
			writer(buffer, (int)newBufferIndex, num2);
			writer(buffer, 0, length2);
		}
	}

	private int CopyFromBuffer(T[] data, int offset, int length, int newBufferIndex)
	{
		if (length > buffer.Length)
		{
			throw new ArgumentException($"Push data exceeds buffer size {length} < {buffer.Length}");
		}
		if (newBufferIndex + length < buffer.Length)
		{
			Array.Copy(buffer, newBufferIndex, data, offset, length);
			return newBufferIndex + length;
		}
		int num = Mathf.Min(buffer.Length, length);
		int num2 = buffer.Length - newBufferIndex;
		int num3 = num - num2;
		Array.Copy(buffer, newBufferIndex, data, offset, num2);
		Array.Copy(buffer, 0, data, offset + num2, num3);
		return num3;
	}

	public void Push(T[] data, int offset, int length)
	{
		lock (buffer)
		{
			bufferIndex = CopyToBuffer(data, offset, length, bufferIndex);
			bufferDataLength += length;
			OnDataAddedEvent?.Invoke(data, offset, length);
		}
	}

	public void Push(T data)
	{
		lock (buffer)
		{
			buffer[bufferIndex++] = data;
			if (bufferIndex >= buffer.Length)
			{
				bufferIndex = 0;
			}
			bufferDataLength++;
		}
	}

	public int Read(T[] data, int offset, int length, long bufferDataIndex)
	{
		if (bufferIndex == 0 && bufferDataLength == 0L)
		{
			return 0;
		}
		lock (buffer)
		{
			int result = (int)(Math.Min(bufferDataIndex + length, bufferDataLength) - bufferDataIndex);
			int num = bufferIndex - (int)(bufferDataLength - bufferDataIndex);
			if (num < 0)
			{
				num = buffer.Length + num;
			}
			CopyFromBuffer(data, offset, length, num);
			return result;
		}
	}

	public Marker CreateMarker(int offset = 0)
	{
		long num = bufferDataLength + offset;
		if (num < 0)
		{
			num = 0L;
		}
		int num2 = bufferIndex + offset;
		if (num2 < 0)
		{
			num2 = buffer.Length + num2;
		}
		if (num2 > buffer.Length)
		{
			num2 -= buffer.Length;
		}
		return new Marker(this, num, num2);
	}
}
