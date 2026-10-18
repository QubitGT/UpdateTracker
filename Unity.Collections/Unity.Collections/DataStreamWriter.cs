using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Collections;

[MovedFrom(true, "Unity.Networking.Transport", "Unity.Networking.Transport", null)]
[GenerateTestsForBurstCompatibility]
public struct DataStreamWriter
{
	private struct StreamData
	{
		public unsafe byte* buffer;

		public int length;

		public int capacity;

		public ulong bitBuffer;

		public int bitIndex;

		public int failedWrites;
	}

	[NativeDisableUnsafePtrRestriction]
	private StreamData m_Data;

	public IntPtr m_SendHandleData;

	public unsafe static bool IsLittleEndian
	{
		get
		{
			uint num = 1u;
			byte* ptr = (byte*)(&num);
			return *ptr == 1;
		}
	}

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.buffer != null;
		}
	}

	public readonly bool HasFailedWrites
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.failedWrites > 0;
		}
	}

	public readonly int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.capacity;
		}
	}

	public int Length
	{
		get
		{
			SyncBitData();
			return m_Data.length + (m_Data.bitIndex + 7 >> 3);
		}
	}

	public int LengthInBits
	{
		get
		{
			SyncBitData();
			return m_Data.length * 8 + m_Data.bitIndex;
		}
	}

	public DataStreamWriter(int length, AllocatorManager.AllocatorHandle allocator)
	{
		Initialize(out this, CollectionHelper.CreateNativeArray<byte>(length, allocator));
	}

	public DataStreamWriter(NativeArray<byte> data)
	{
		Initialize(out this, data);
	}

	public unsafe DataStreamWriter(byte* data, int length)
	{
		NativeArray<byte> data2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(data, length, Allocator.Invalid);
		Initialize(out this, data2);
	}

	public unsafe NativeArray<byte> AsNativeArray()
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(m_Data.buffer, Length, Allocator.Invalid);
	}

	private unsafe static void Initialize(out DataStreamWriter self, NativeArray<byte> data)
	{
		self.m_SendHandleData = IntPtr.Zero;
		self.m_Data.capacity = data.Length;
		self.m_Data.length = 0;
		self.m_Data.buffer = (byte*)data.GetUnsafePtr();
		self.m_Data.bitBuffer = 0uL;
		self.m_Data.bitIndex = 0;
		self.m_Data.failedWrites = 0;
	}

	private static short ByteSwap(short val)
	{
		return (short)(((val & 0xFF) << 8) | ((val >> 8) & 0xFF));
	}

	private static int ByteSwap(int val)
	{
		return ((val & 0xFF) << 24) | ((val & 0xFF00) << 8) | ((val >> 8) & 0xFF00) | ((val >> 24) & 0xFF);
	}

	private unsafe void SyncBitData()
	{
		int num = m_Data.bitIndex;
		if (num > 0)
		{
			ulong num2 = m_Data.bitBuffer;
			int num3 = 0;
			while (num > 0)
			{
				m_Data.buffer[m_Data.length + num3] = (byte)num2;
				num -= 8;
				num2 >>= 8;
				num3++;
			}
		}
	}

	public unsafe void Flush()
	{
		while (m_Data.bitIndex > 0)
		{
			m_Data.buffer[m_Data.length++] = (byte)m_Data.bitBuffer;
			m_Data.bitIndex -= 8;
			m_Data.bitBuffer >>= 8;
		}
		m_Data.bitIndex = 0;
	}

	private unsafe bool WriteBytesInternal(byte* data, int bytes)
	{
		if (m_Data.length + (m_Data.bitIndex + 7 >> 3) + bytes > m_Data.capacity)
		{
			m_Data.failedWrites++;
			return false;
		}
		Flush();
		UnsafeUtility.MemCpy(m_Data.buffer + m_Data.length, data, bytes);
		m_Data.length += bytes;
		return true;
	}

	public unsafe bool WriteByte(byte value)
	{
		return WriteBytesInternal(&value, 1);
	}

	public unsafe bool WriteBytes(NativeArray<byte> value)
	{
		return WriteBytesInternal((byte*)value.GetUnsafeReadOnlyPtr(), value.Length);
	}

	public unsafe bool WriteBytes(Span<byte> value)
	{
		fixed (byte* data = value)
		{
			return WriteBytesInternal(data, value.Length);
		}
	}

	public unsafe bool WriteShort(short value)
	{
		return WriteBytesInternal((byte*)(&value), 2);
	}

	public unsafe bool WriteUShort(ushort value)
	{
		return WriteBytesInternal((byte*)(&value), 2);
	}

	public unsafe bool WriteInt(int value)
	{
		return WriteBytesInternal((byte*)(&value), 4);
	}

	public unsafe bool WriteUInt(uint value)
	{
		return WriteBytesInternal((byte*)(&value), 4);
	}

	public unsafe bool WriteLong(long value)
	{
		return WriteBytesInternal((byte*)(&value), 8);
	}

	public unsafe bool WriteULong(ulong value)
	{
		return WriteBytesInternal((byte*)(&value), 8);
	}

	public unsafe bool WriteShortNetworkByteOrder(short value)
	{
		short num = (IsLittleEndian ? ByteSwap(value) : value);
		return WriteBytesInternal((byte*)(&num), 2);
	}

	public bool WriteUShortNetworkByteOrder(ushort value)
	{
		return WriteShortNetworkByteOrder((short)value);
	}

	public unsafe bool WriteIntNetworkByteOrder(int value)
	{
		int num = (IsLittleEndian ? ByteSwap(value) : value);
		return WriteBytesInternal((byte*)(&num), 4);
	}

	public bool WriteUIntNetworkByteOrder(uint value)
	{
		return WriteIntNetworkByteOrder((int)value);
	}

	public bool WriteFloat(float value)
	{
		UIntFloat uIntFloat = default(UIntFloat);
		uIntFloat.floatValue = value;
		return WriteInt((int)uIntFloat.intValue);
	}

	public bool WriteDouble(double value)
	{
		UIntFloat uIntFloat = default(UIntFloat);
		uIntFloat.doubleValue = value;
		return WriteLong((long)uIntFloat.longValue);
	}

	private unsafe void FlushBits()
	{
		while (m_Data.bitIndex >= 8)
		{
			m_Data.buffer[m_Data.length++] = (byte)m_Data.bitBuffer;
			m_Data.bitIndex -= 8;
			m_Data.bitBuffer >>= 8;
		}
	}

	private void WriteRawBitsInternal(uint value, int numbits)
	{
		m_Data.bitBuffer |= (ulong)value << m_Data.bitIndex;
		m_Data.bitIndex += numbits;
	}

	public bool WriteRawBits(uint value, int numbits)
	{
		if (m_Data.length + (m_Data.bitIndex + numbits + 7 >> 3) > m_Data.capacity)
		{
			m_Data.failedWrites++;
			return false;
		}
		WriteRawBitsInternal(value, numbits);
		FlushBits();
		return true;
	}

	public unsafe bool WritePackedUInt(uint value, in StreamCompressionModel model)
	{
		int num = model.CalculateBucket(value);
		uint num2 = model.bucketOffsets[num];
		int num3 = model.bucketSizes[num];
		ushort num4 = model.encodeTable[num];
		if (m_Data.length + (m_Data.bitIndex + (num4 & 0xFF) + num3 + 7 >> 3) > m_Data.capacity)
		{
			m_Data.failedWrites++;
			return false;
		}
		WriteRawBitsInternal((uint)(num4 >> 8), num4 & 0xFF);
		WriteRawBitsInternal(value - num2, num3);
		FlushBits();
		return true;
	}

	public unsafe bool WritePackedULong(ulong value, in StreamCompressionModel model)
	{
		uint* ptr = (uint*)(&value);
		return WritePackedUInt(*ptr, in model) & WritePackedUInt(ptr[1], in model);
	}

	public bool WritePackedInt(int value, in StreamCompressionModel model)
	{
		uint value2 = (uint)((value >> 31) ^ (value << 1));
		return WritePackedUInt(value2, in model);
	}

	public bool WritePackedLong(long value, in StreamCompressionModel model)
	{
		ulong value2 = (ulong)((value >> 63) ^ (value << 1));
		return WritePackedULong(value2, in model);
	}

	public bool WritePackedFloat(float value, in StreamCompressionModel model)
	{
		return WritePackedFloatDelta(value, 0f, in model);
	}

	public bool WritePackedDouble(double value, in StreamCompressionModel model)
	{
		return WritePackedDoubleDelta(value, 0.0, in model);
	}

	public bool WritePackedUIntDelta(uint value, uint baseline, in StreamCompressionModel model)
	{
		int value2 = (int)(baseline - value);
		return WritePackedInt(value2, in model);
	}

	public bool WritePackedIntDelta(int value, int baseline, in StreamCompressionModel model)
	{
		int value2 = baseline - value;
		return WritePackedInt(value2, in model);
	}

	public bool WritePackedLongDelta(long value, long baseline, in StreamCompressionModel model)
	{
		long value2 = baseline - value;
		return WritePackedLong(value2, in model);
	}

	public bool WritePackedULongDelta(ulong value, ulong baseline, in StreamCompressionModel model)
	{
		long value2 = (long)(baseline - value);
		return WritePackedLong(value2, in model);
	}

	public bool WritePackedFloatDelta(float value, float baseline, in StreamCompressionModel model)
	{
		int num = 0;
		if (value != baseline)
		{
			num = 32;
		}
		if (m_Data.length + (m_Data.bitIndex + 1 + num + 7 >> 3) > m_Data.capacity)
		{
			m_Data.failedWrites++;
			return false;
		}
		if (num == 0)
		{
			WriteRawBitsInternal(0u, 1);
		}
		else
		{
			WriteRawBitsInternal(1u, 1);
			UIntFloat uIntFloat = default(UIntFloat);
			uIntFloat.floatValue = value;
			WriteRawBitsInternal(uIntFloat.intValue, num);
		}
		FlushBits();
		return true;
	}

	public unsafe bool WritePackedDoubleDelta(double value, double baseline, in StreamCompressionModel model)
	{
		int num = 0;
		if (value != baseline)
		{
			num = 64;
		}
		if (m_Data.length + (m_Data.bitIndex + 1 + num + 7 >> 3) > m_Data.capacity)
		{
			m_Data.failedWrites++;
			return false;
		}
		if (num == 0)
		{
			WriteRawBitsInternal(0u, 1);
		}
		else
		{
			WriteRawBitsInternal(1u, 1);
			UIntFloat uIntFloat = default(UIntFloat);
			uIntFloat.doubleValue = value;
			uint* ptr = (uint*)(&uIntFloat.longValue);
			WriteRawBitsInternal(*ptr, 32);
			FlushBits();
			WriteRawBitsInternal(ptr[1], 32);
		}
		FlushBits();
		return true;
	}

	public unsafe bool WriteFixedString32(FixedString32Bytes str)
	{
		int bytes = *(ushort*)(&str) + 2;
		byte* data = (byte*)(&str);
		return WriteBytesInternal(data, bytes);
	}

	public unsafe bool WriteFixedString64(FixedString64Bytes str)
	{
		int bytes = *(ushort*)(&str) + 2;
		byte* data = (byte*)(&str);
		return WriteBytesInternal(data, bytes);
	}

	public unsafe bool WriteFixedString128(FixedString128Bytes str)
	{
		int bytes = *(ushort*)(&str) + 2;
		byte* data = (byte*)(&str);
		return WriteBytesInternal(data, bytes);
	}

	public unsafe bool WriteFixedString512(FixedString512Bytes str)
	{
		int bytes = *(ushort*)(&str) + 2;
		byte* data = (byte*)(&str);
		return WriteBytesInternal(data, bytes);
	}

	public unsafe bool WriteFixedString4096(FixedString4096Bytes str)
	{
		int bytes = *(ushort*)(&str) + 2;
		byte* data = (byte*)(&str);
		return WriteBytesInternal(data, bytes);
	}

	public unsafe bool WritePackedFixedString32Delta(FixedString32Bytes str, FixedString32Bytes baseline, in StreamCompressionModel model)
	{
		ushort length = *(ushort*)(&str);
		byte* data = (byte*)(&str) + 2;
		return WritePackedFixedStringDelta(data, length, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
	}

	public unsafe bool WritePackedFixedString64Delta(FixedString64Bytes str, FixedString64Bytes baseline, in StreamCompressionModel model)
	{
		ushort length = *(ushort*)(&str);
		byte* data = (byte*)(&str) + 2;
		return WritePackedFixedStringDelta(data, length, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
	}

	public unsafe bool WritePackedFixedString128Delta(FixedString128Bytes str, FixedString128Bytes baseline, in StreamCompressionModel model)
	{
		ushort length = *(ushort*)(&str);
		byte* data = (byte*)(&str) + 2;
		return WritePackedFixedStringDelta(data, length, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
	}

	public unsafe bool WritePackedFixedString512Delta(FixedString512Bytes str, FixedString512Bytes baseline, in StreamCompressionModel model)
	{
		ushort length = *(ushort*)(&str);
		byte* data = (byte*)(&str) + 2;
		return WritePackedFixedStringDelta(data, length, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
	}

	public unsafe bool WritePackedFixedString4096Delta(FixedString4096Bytes str, FixedString4096Bytes baseline, in StreamCompressionModel model)
	{
		ushort length = *(ushort*)(&str);
		byte* data = (byte*)(&str) + 2;
		return WritePackedFixedStringDelta(data, length, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
	}

	private unsafe bool WritePackedFixedStringDelta(byte* data, uint length, byte* baseData, uint baseLength, in StreamCompressionModel model)
	{
		StreamData data2 = m_Data;
		if (!WritePackedUIntDelta(length, baseLength, in model))
		{
			return false;
		}
		bool flag = false;
		if (length <= baseLength)
		{
			for (uint num = 0u; num < length; num++)
			{
				flag |= !WritePackedUIntDelta(data[num], baseData[num], in model);
			}
		}
		else
		{
			for (uint num2 = 0u; num2 < baseLength; num2++)
			{
				flag |= !WritePackedUIntDelta(data[num2], baseData[num2], in model);
			}
			for (uint num3 = baseLength; num3 < length; num3++)
			{
				flag |= !WritePackedUInt(data[num3], in model);
			}
		}
		if (flag)
		{
			m_Data = data2;
			m_Data.failedWrites++;
		}
		return !flag;
	}

	public void Clear()
	{
		m_Data.length = 0;
		m_Data.bitIndex = 0;
		m_Data.bitBuffer = 0uL;
		m_Data.failedWrites = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckRead()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckWrite()
	{
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckAllocator(AllocatorManager.AllocatorHandle allocator)
	{
		if (allocator.ToAllocator != Allocator.Temp)
		{
			throw new InvalidOperationException("DataStreamWriters can only be created with temp memory");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckBits(uint value, int numBits)
	{
		if (numBits < 0 || numBits > 32)
		{
			throw new ArgumentOutOfRangeException($"Invalid number of bits specified: {numBits}! Valid range is (0, 32) inclusive.");
		}
		ulong num = (ulong)(1L << numBits);
		if (value >= num)
		{
			throw new ArgumentOutOfRangeException($"Value {value} does not fit in the specified number of bits: {numBits}! Range (inclusive) is (0, {num - 1})!");
		}
	}
}
