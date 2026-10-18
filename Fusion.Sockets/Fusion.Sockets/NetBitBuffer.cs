#define ENABLE_PROFILER
#define DEBUG
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Fusion.Sockets;

public struct NetBitBuffer : INetBitWriteStream, ILogDumpable
{
	public struct Offset
	{
		private int _offset;

		public unsafe Offset(NetBitBuffer* buffer)
		{
			_offset = buffer->OffsetBits;
		}

		public unsafe int GetLength(NetBitBuffer* buffer)
		{
			return buffer->OffsetBits - _offset;
		}
	}

	private const int BITCOUNT = 64;

	private const int USEDMASK = 63;

	private const int INDEXSHIFT = 6;

	private const int BYTESHIFT = 3;

	private const ulong MAXVALUE = ulong.MaxValue;

	public NetAddress Address;

	internal unsafe NetBitBuffer* Prev;

	internal unsafe NetBitBuffer* Next;

	internal unsafe NetBitBufferBlock* _block;

	internal unsafe NetBitBuffer* _allocNext;

	private int _group;

	private unsafe ulong* _data;

	private unsafe ulong* _dataBlockOriginal;

	private int _offsetBits;

	private int _lengthBits;

	private int _lengthBytes;

	internal short Group
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (short)(_group - 1);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			_group = value + 1;
		}
	}

	public unsafe ulong* Data
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _data;
		}
		internal set
		{
			_data = value;
		}
	}

	public int LengthBits
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _lengthBits;
		}
	}

	public int BytesRemaining => _lengthBytes - Maths.BytesRequiredForBits(_offsetBits);

	public int LengthBytes
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _lengthBytes;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal set
		{
			Assert.Check(value >= 0);
			_lengthBits = value << 3;
			_lengthBytes = value;
		}
	}

	public int OffsetBits
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal set
		{
			Assert.Check(value >= 0 && value <= _lengthBits);
			_offsetBits = value;
		}
	}

	public bool Done
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits == _lengthBits;
		}
	}

	public bool Overflow
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits > _lengthBits;
		}
	}

	internal bool OverflowOrLessThanOneByteRemaining
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits + 8 > _lengthBits;
		}
	}

	public int OffsetBitsUnsafe
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			_offsetBits = value;
		}
	}

	public bool DoneOrOverflow
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits >= _lengthBits;
		}
	}

	public bool MoreToRead
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits < _lengthBits;
		}
	}

	internal unsafe NetPacketType PacketType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return *(NetPacketType*)_data;
		}
		set
		{
			*(NetPacketType*)_data = value;
		}
	}

	public bool IsOnEvenByte
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _offsetBits % 8 == 0;
		}
	}

	public int OffsetBytes
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			Assert.Check(IsOnEvenByte);
			Assert.Check(Maths.BytesRequiredForBits(_offsetBits) == _offsetBits / 8);
			return _offsetBits / 8;
		}
	}

	public unsafe void ReplaceDataFromBlockWithTemp(int tempSize)
	{
		EngineProfiler.Begin("ReplaceDataFromBlockWithTemp");
		tempSize = Native.RoundToAlignment(tempSize, 8);
		if (_dataBlockOriginal == null)
		{
			_dataBlockOriginal = _data;
			_data = (ulong*)Native.MallocAndClear(tempSize + 1024);
			Native.MemCpy(_data, _dataBlockOriginal, _lengthBytes);
		}
		else
		{
			ulong* memory = _data;
			_data = (ulong*)Native.MallocAndClear(tempSize + 1024);
			Native.MemCpy(_data, memory, _lengthBytes);
			Native.Free(ref memory);
		}
		_lengthBytes = tempSize;
		_lengthBits = tempSize << 3;
		EngineProfiler.End();
	}

	public unsafe static Offset GetOffset(NetBitBuffer* buffer)
	{
		return new Offset(buffer);
	}

	public unsafe static NetBitBuffer* Allocate(int group, int size)
	{
		if (size <= 0)
		{
			throw new InvalidOperationException();
		}
		size = Native.RoundToAlignment(size, 8);
		Native.MallocAndClearBlock(sizeof(NetBitBuffer), size, out var ptr, out var ptr2);
		NetBitBuffer* ptr3 = (NetBitBuffer*)ptr;
		ptr3->_group = group;
		ptr3->SetBufferLengthBytes((ulong*)ptr2, size);
		return ptr3;
	}

	public unsafe static void ReleaseRef(ref NetBitBuffer* buffer)
	{
		if (buffer != null)
		{
			NetBitBuffer* buffer2 = buffer;
			buffer = null;
			Release(buffer2);
		}
	}

	public unsafe static void Release(NetBitBuffer* buffer)
	{
		if (buffer != null)
		{
			if (buffer->_dataBlockOriginal != null)
			{
				Native.Free(ref buffer->_data);
				buffer->_data = buffer->_dataBlockOriginal;
				buffer->_dataBlockOriginal = null;
			}
			if (buffer->_block != null)
			{
				buffer->_block->Release(buffer);
			}
			else
			{
				InternalLogStreams.LogDebug?.Warn("NetBitBuffer trying to release with a null block.");
			}
		}
	}

	public unsafe void SetBufferLengthBytes(ulong* buffer, int lenghtInBytes)
	{
		Assert.Check((long)buffer % 8L == 0);
		Assert.Check(lenghtInBytes % 8 == 0);
		_data = buffer;
		_lengthBits = lenghtInBytes << 3;
		_lengthBytes = lenghtInBytes;
	}

	public unsafe void Clear()
	{
		Assert.Check(_data != null);
		Assert.Check(_lengthBytes > 0);
		Native.MemClear(_data, _lengthBytes);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool WriteBoolean(bool value)
	{
		Write((ulong)(value ? 1 : 0), 1);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ReadBoolean()
	{
		return Read(1) == 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool PeekBoolean()
	{
		return Peek(1) == 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteByte(byte value, int bits = 8)
	{
		Assert.Check(bits >= 0 && bits <= 8);
		Write(value, bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte ReadByte(int bits = 8)
	{
		Assert.Check(bits >= 0 && bits <= 8, bits);
		return (byte)Read(bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteInt16(short value, int bits = 16)
	{
		Assert.Check(bits >= 0 && bits <= 16);
		Write((ulong)value, bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public short ReadInt16(int bits = 16)
	{
		Assert.Check(bits >= 0 && bits <= 16, bits);
		return (short)Read(bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteUInt16(ushort value, int bits = 16)
	{
		Assert.Check(bits >= 0 && bits <= 16);
		Write(value, bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ushort ReadUInt16(int bits = 16)
	{
		Assert.Check(bits >= 0 && bits <= 16, bits);
		return (ushort)Read(bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteInt32(int value, int bits = 32)
	{
		Assert.Check(bits >= 0 && bits <= 32);
		Write((ulong)value, bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ReadInt32(int bits = 32)
	{
		Assert.Check(bits >= 0 && bits <= 32, bits);
		return (int)Read(bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteUInt32(uint value, int bits = 32)
	{
		Assert.Check(bits >= 0 && bits <= 32);
		Write(value, bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteString(string value)
	{
		WriteString(value, Encoding.UTF8);
	}

	public void WriteString(string value, Encoding encoding)
	{
		if (!WriteBoolean(value == null))
		{
			byte[] bytes = encoding.GetBytes(value);
			WriteUInt16((ushort)bytes.Length);
			WriteBytesAligned(bytes, bytes.Length);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ReadString()
	{
		return ReadString(Encoding.UTF8);
	}

	public unsafe string ReadString(Encoding encoding)
	{
		if (ReadBoolean())
		{
			return null;
		}
		int num = ReadUInt16();
		SeekToByteBoundary();
		Assert.Check(IsOnEvenByte);
		if (num == 0)
		{
			return "";
		}
		int num2 = Advance(num * 8, writing: false);
		byte* bytes = (byte*)_data + num2 / 8;
		return encoding.GetString(bytes, num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CanWrite(int bits)
	{
		return CanRead(bits);
	}

	public bool CanRead(int bits)
	{
		return _offsetBits + bits <= _lengthBits;
	}

	public void PadToByteBoundary()
	{
		if (_offsetBits % 8 != 0)
		{
			WriteByte(0, 8 - _offsetBits % 8);
		}
	}

	public unsafe byte* GetDataPointer()
	{
		Assert.Check(IsOnEvenByte);
		return (byte*)_data + _offsetBits / 8;
	}

	public unsafe byte* PadToByteBoundaryAndGetPtr()
	{
		PadToByteBoundary();
		Assert.Check(_offsetBits % 8 == 0);
		return (byte*)_data + _offsetBits / 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckBitCount(int count)
	{
		return count >= 0 && OffsetBits + count <= _lengthBits;
	}

	public void SeekToByteBoundary()
	{
		_offsetBits = (_offsetBits + 7) & -8;
	}

	public unsafe void WriteBytesAligned(byte[] buffer, int length)
	{
		fixed (byte* buffer2 = buffer)
		{
			WriteBytesAligned(buffer2, length);
		}
	}

	public unsafe void WriteBytesAligned(void* buffer, int length)
	{
		PadToByteBoundary();
		Assert.Check(IsOnEvenByte);
		Assert.Check(OffsetBytes + length <= _lengthBytes, OffsetBytes + length, OffsetBytes, length, _lengthBytes);
		int num = Advance(length * 8, writing: true);
		int num2 = num >> 6;
		int num3 = num + length * 8 >> 6;
		if (num3 != num2)
		{
			_data[num3] = 0uL;
		}
		Native.MemCpy((byte*)_data + num / 8, buffer, length);
	}

	public unsafe void WriteBytesAligned(Span<byte> buffer)
	{
		PadToByteBoundary();
		Assert.Check(IsOnEvenByte);
		Assert.Check(OffsetBytes + buffer.Length <= _lengthBytes, OffsetBytes + buffer.Length, OffsetBytes, buffer.Length, _lengthBytes);
		int num = Advance(buffer.Length * 8, writing: true);
		int num2 = num >> 6;
		int num3 = num + buffer.Length * 8 >> 6;
		if (num3 != num2)
		{
			_data[num3] = 0uL;
		}
		Span<byte> d = new Span<byte>((byte*)_data + num / 8, buffer.Length);
		Native.MemCpy(d, buffer);
	}

	public unsafe void ReadBytesAligned(byte[] buffer, int length)
	{
		fixed (byte* buffer2 = buffer)
		{
			ReadBytesAligned(buffer2, length);
		}
	}

	public unsafe void ReadBytesAligned(Span<byte> buffer)
	{
		SeekToByteBoundary();
		Assert.Check(IsOnEvenByte);
		int num = Advance(buffer.Length * 8, writing: false);
		Native.MemCpy(buffer, new Span<byte>((byte*)_data + num / 8, buffer.Length));
	}

	public unsafe void ReadBytesAligned(void* buffer, int length)
	{
		SeekToByteBoundary();
		Assert.Check(IsOnEvenByte);
		int num = Advance(length * 8, writing: false);
		Native.MemCpy(buffer, (byte*)_data + num / 8, length);
	}

	public void WriteInt64VarLength(long value, int blockSize)
	{
		WriteUInt64VarLength((ulong)value, blockSize);
	}

	public void WriteInt32VarLength(int value)
	{
		WriteUInt32VarLength((uint)value);
	}

	public void WriteInt32VarLength(int value, int blockSize)
	{
		WriteUInt32VarLength((uint)value, blockSize);
	}

	public int ReadInt32VarLength()
	{
		return (int)ReadUInt32VarLength();
	}

	public long ReadInt64VarLength(int blockSize)
	{
		return (long)ReadUInt64VarLength(blockSize);
	}

	public int ReadInt32VarLength(int blockSize)
	{
		return (int)ReadUInt32VarLength(blockSize);
	}

	public uint ReadUInt32VarLength(int blockSize)
	{
		blockSize = Maths.Clamp(blockSize, 2, 16);
		int num = 1;
		while (!ReadBoolean() && !DoneOrOverflow)
		{
			num++;
		}
		if (DoneOrOverflow)
		{
			return 0u;
		}
		return ReadUInt32(num * blockSize);
	}

	public ulong ReadUInt64VarLength(int blockSize)
	{
		blockSize = Maths.Clamp(blockSize, 2, 16);
		int num = 1;
		while (!ReadBoolean() && !DoneOrOverflow)
		{
			num++;
		}
		if (DoneOrOverflow)
		{
			return 0uL;
		}
		return ReadUInt64(num * blockSize);
	}

	public void WriteUInt32VarLength(uint value, int blockSize)
	{
		blockSize = Maths.Clamp(blockSize, 2, 16);
		int num = (Maths.BitScanReverse(value) + blockSize) / blockSize;
		WriteUInt32((uint)(1 << num - 1), num);
		WriteUInt32(value, num * blockSize);
	}

	public void WriteUInt64VarLength(ulong value, int blockSize)
	{
		blockSize = Maths.Clamp(blockSize, 2, 16);
		int num = (Maths.BitScanReverse(value) + blockSize) / blockSize;
		WriteUInt32((uint)(1 << num - 1), num);
		WriteUInt64(value, num * blockSize);
	}

	public unsafe void WriteUInt32VarLength(uint value)
	{
		int num = 0;
		ulong value2 = 0uL;
		byte* ptr = (byte*)(&value2);
		while (true)
		{
			ptr[num] = (byte)(value & 0x7Fu);
			value >>= 7;
			if (value == 0)
			{
				break;
			}
			byte* num2 = ptr + num++;
			*num2 = (byte)(*num2 | 0x80u);
		}
		Write(value2, (num + 1) * 8);
	}

	public unsafe uint ReadUInt32VarLength()
	{
		Assert.Check(_offsetBits < _lengthBits);
		int num = _lengthBits - _offsetBits;
		if (num > 64)
		{
			num = 64;
		}
		ulong num2 = Peek(num);
		int num3 = 0;
		uint num4 = 0u;
		byte* ptr = (byte*)(&num2);
		while (true)
		{
			Assert.Check(num3 >= 0 && num3 <= 7);
			uint num5 = ptr[num3];
			num4 |= (num5 & 0x7F) << 7 * num3;
			if ((num5 & 0x80) != 128)
			{
				break;
			}
			num3++;
		}
		_offsetBits += (num3 + 1) * 8;
		return num4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint ReadUInt32(int bits = 32)
	{
		Assert.Check(bits >= 0 && bits <= 32, bits);
		return (uint)Read(bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteInt64(long value, int bits = 64)
	{
		Assert.Check(bits >= 0 && bits <= 64);
		Write((ulong)value, bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long ReadInt64(int bits = 64)
	{
		Assert.Check(bits >= 0 && bits <= 64, bits);
		return (long)Read(bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteUInt64(ulong value, int bits = 64)
	{
		Assert.Check(bits >= 0 && bits <= 64, bits);
		Write(value, bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ulong ReadUInt64(int bits = 64)
	{
		Assert.Check(bits >= 0 && bits <= 64, bits);
		return Read(bits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void WriteSingle(float value)
	{
		Write(*(uint*)(&value), 32);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe float ReadSingle()
	{
		ulong num = Read(32);
		return *(float*)(&num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void WriteDouble(double value)
	{
		Write(*(ulong*)(&value), 64);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe double ReadDouble()
	{
		ulong num = Read(64);
		return *(double*)(&num);
	}

	public void WriteInt32AtOffset(int value, int offset, int bits)
	{
		int offsetBits = _offsetBits;
		try
		{
			_offsetBits = offset;
			WriteSlow((uint)value, bits);
		}
		finally
		{
			_offsetBits = offsetBits;
		}
	}

	public void WriteUInt64AtOffset(ulong value, int offset, int bits)
	{
		int offsetBits = _offsetBits;
		try
		{
			_offsetBits = offset;
			WriteSlow(value, bits);
		}
		finally
		{
			_offsetBits = offsetBits;
		}
	}

	public unsafe void Write(ulong value, int bits)
	{
		Assert.Check(bits >= 0 && bits <= 64, bits);
		value &= ulong.MaxValue >> 64 - bits;
		Assert.Check(bits >= 0 && bits <= 64);
		int num = Advance(bits, writing: true);
		int num2 = num & 0x3F;
		int num3 = 64 - num2;
		Assert.Check(num2 + num3 == 64);
		ulong* ptr = _data + (num >> 6);
		bool arg = false;
		*ptr = (*ptr & (ulong)((1L << num2) - 1)) | (value << num2);
		if (num3 < bits)
		{
			arg = true;
			ptr[1] = value >> num3;
		}
		_offsetBits = num;
		ulong num4 = Read(bits);
		Assert.Check(num4 == value, num4, value, arg);
	}

	public unsafe void WriteSlow(ulong value, int bits)
	{
		Assert.Check(bits >= 0 && bits <= 64, bits);
		if (bits > 0)
		{
			value &= ulong.MaxValue >> 64 - bits;
			int num = Advance(bits, writing: false);
			int num2 = num >> 6;
			int num3 = num & 0x3F;
			int num4 = 64 - num3;
			int num5 = num4 - bits;
			ulong* data = _data;
			if (num5 >= 0)
			{
				ulong num6 = (ulong.MaxValue >> num4) | (ulong)(-1L << 64 - num5);
				data[num2] = (data[num2] & num6) | (value << num3);
			}
			else
			{
				data[num2] = (data[num2] & (ulong.MaxValue >> num4)) | (value << num3);
				data[num2 + 1] = (data[num2 + 1] & (ulong)(-1L << bits - num4)) | (value >> num4);
			}
		}
	}

	private unsafe ulong Read(int bits)
	{
		Assert.Check(bits >= 0 && bits <= 64, bits);
		if (bits <= 0)
		{
			return 0uL;
		}
		int num = Advance(bits, writing: false);
		int num2 = num >> 6;
		int num3 = num & 0x3F;
		ulong num4 = _data[num2] >> num3;
		int num5 = bits - (64 - num3);
		ulong result;
		if (num5 < 1)
		{
			result = num4 & (ulong.MaxValue >> 64 - bits);
		}
		else
		{
			ulong num6 = _data[num2 + 1] & (ulong.MaxValue >> 64 - num5);
			result = num4 | (num6 << bits - num5);
		}
		return result;
	}

	private unsafe ulong Peek(int bits)
	{
		Assert.Check(bits >= 0 && bits <= 64, bits);
		if (bits <= 0)
		{
			return 0uL;
		}
		if (!CheckBitCount(bits))
		{
			throw new InvalidOperationException($"Out of bounds. Bit position: {_offsetBits}, length: {bits}, capacity: {LengthBits}");
		}
		int offsetBits = _offsetBits;
		int num = offsetBits >> 6;
		int num2 = offsetBits & 0x3F;
		ulong num3 = _data[num] >> num2;
		int num4 = bits - (64 - num2);
		ulong result;
		if (num4 < 1)
		{
			result = num3 & (ulong.MaxValue >> 64 - bits);
		}
		else
		{
			ulong num5 = _data[num + 1] & (ulong.MaxValue >> 64 - num4);
			result = num3 | (num5 << bits - num4);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int Advance(int bits, bool writing)
	{
		int offsetBits = _offsetBits;
		_offsetBits += bits;
		if (_offsetBits > LengthBits)
		{
			if (!writing)
			{
				throw new InvalidOperationException($"Tried to read out of bounds, position: {offsetBits}, reading: {bits}, capacity: {LengthBits}");
			}
			ReplaceDataFromBlockWithTemp(LengthBytes * 2);
		}
		return offsetBits;
	}

	void ILogDumpable.Dump(StringBuilder builder)
	{
		builder.Append($"[Offset: {OffsetBits}]");
	}
}
