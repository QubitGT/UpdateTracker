using System;

namespace Fusion.Sockets;

public struct NetBitBufferNull : INetBitWriteStream
{
	private int _offsetBits;

	public int OffsetBits
	{
		get
		{
			return _offsetBits;
		}
		set
		{
			_offsetBits = value;
		}
	}

	public void PadToByteBoundary()
	{
		if (_offsetBits % 8 != 0)
		{
			WriteByte(0, 8 - _offsetBits % 8);
		}
	}

	public void WriteByte(byte value, int bits = 8)
	{
		_offsetBits += bits;
	}

	public void WriteInt32(int value, int bits = 32)
	{
		_offsetBits += bits;
	}

	public void WriteInt32VarLength(int value)
	{
		WriteUInt32VarLength((uint)value);
	}

	public void WriteInt32VarLength(int value, int blockSize)
	{
		WriteUInt32VarLength((uint)value, blockSize);
	}

	public void WriteUInt32VarLength(uint value, int blockSize)
	{
		blockSize = Maths.Clamp(blockSize, 2, 16);
		int num = (Maths.BitScanReverse(value) + blockSize) / blockSize;
		_offsetBits += num + num * blockSize;
	}

	public void WriteUInt64VarLength(ulong value, int blockSize)
	{
		blockSize = Maths.Clamp(blockSize, 2, 16);
		int num = (Maths.BitScanReverse(value) + blockSize) / blockSize;
		_offsetBits += num + num * blockSize;
	}

	public void WriteUInt32VarLength(uint value)
	{
		int num = 0;
		while (true)
		{
			value >>= 7;
			if (value == 0)
			{
				break;
			}
			num++;
		}
		_offsetBits += (num + 1) * 8;
	}

	public bool WriteBoolean(bool b)
	{
		_offsetBits++;
		return b;
	}

	public unsafe void WriteBytesAligned(void* buffer, int length)
	{
		PadToByteBoundary();
		_offsetBits += length * 8;
	}

	public void WriteBytesAligned(Span<byte> buffer)
	{
		PadToByteBoundary();
		_offsetBits += buffer.Length * 8;
	}
}
