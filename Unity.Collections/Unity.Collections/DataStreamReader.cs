using System;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Collections;

[MovedFrom(true, "Unity.Networking.Transport", null, null)]
[GenerateTestsForBurstCompatibility]
public struct DataStreamReader
{
	private struct Context
	{
		public int m_ReadByteIndex;

		public int m_BitIndex;

		public ulong m_BitBuffer;

		public int m_FailedReads;
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe byte* m_BufferPtr;

	private Context m_Context;

	private int m_Length;

	public static bool IsLittleEndian => DataStreamWriter.IsLittleEndian;

	public readonly bool HasFailedReads => m_Context.m_FailedReads > 0;

	public readonly int Length => m_Length;

	public unsafe readonly bool IsCreated => m_BufferPtr != null;

	public DataStreamReader(NativeArray<byte> array)
	{
		Initialize(out this, array);
	}

	private unsafe static void Initialize(out DataStreamReader self, NativeArray<byte> array)
	{
		self.m_BufferPtr = (byte*)array.GetUnsafeReadOnlyPtr();
		self.m_Length = array.Length;
		self.m_Context = default(Context);
	}

	private static short ByteSwap(short val)
	{
		return (short)(((val & 0xFF) << 8) | ((val >> 8) & 0xFF));
	}

	private static int ByteSwap(int val)
	{
		return ((val & 0xFF) << 24) | ((val & 0xFF00) << 8) | ((val >> 8) & 0xFF00) | ((val >> 24) & 0xFF);
	}

	private unsafe void ReadBytesInternal(byte* data, int length)
	{
		if (GetBytesRead() + length > m_Length)
		{
			m_Context.m_FailedReads++;
			UnsafeUtility.MemClear(data, length);
		}
		else
		{
			Flush();
			UnsafeUtility.MemCpy(data, m_BufferPtr + m_Context.m_ReadByteIndex, length);
			m_Context.m_ReadByteIndex += length;
		}
	}

	public unsafe void ReadBytes(NativeArray<byte> array)
	{
		ReadBytesInternal((byte*)array.GetUnsafePtr(), array.Length);
	}

	public unsafe void ReadBytes(Span<byte> span)
	{
		fixed (byte* data = span)
		{
			ReadBytesInternal(data, span.Length);
		}
	}

	public int GetBytesRead()
	{
		return m_Context.m_ReadByteIndex - (m_Context.m_BitIndex >> 3);
	}

	public int GetBitsRead()
	{
		return (m_Context.m_ReadByteIndex << 3) - m_Context.m_BitIndex;
	}

	public void SeekSet(int pos)
	{
		if (pos > m_Length)
		{
			m_Context.m_FailedReads++;
			return;
		}
		m_Context.m_ReadByteIndex = pos;
		m_Context.m_BitIndex = 0;
		m_Context.m_BitBuffer = 0uL;
	}

	public unsafe byte ReadByte()
	{
		byte result = default(byte);
		ReadBytesInternal(&result, 1);
		return result;
	}

	public unsafe short ReadShort()
	{
		short result = default(short);
		ReadBytesInternal((byte*)(&result), 2);
		return result;
	}

	public unsafe ushort ReadUShort()
	{
		ushort result = default(ushort);
		ReadBytesInternal((byte*)(&result), 2);
		return result;
	}

	public unsafe int ReadInt()
	{
		int result = default(int);
		ReadBytesInternal((byte*)(&result), 4);
		return result;
	}

	public unsafe uint ReadUInt()
	{
		uint result = default(uint);
		ReadBytesInternal((byte*)(&result), 4);
		return result;
	}

	public unsafe long ReadLong()
	{
		long result = default(long);
		ReadBytesInternal((byte*)(&result), 8);
		return result;
	}

	public unsafe ulong ReadULong()
	{
		ulong result = default(ulong);
		ReadBytesInternal((byte*)(&result), 8);
		return result;
	}

	public void Flush()
	{
		m_Context.m_ReadByteIndex -= m_Context.m_BitIndex >> 3;
		m_Context.m_BitIndex = 0;
		m_Context.m_BitBuffer = 0uL;
	}

	public unsafe short ReadShortNetworkByteOrder()
	{
		short num = default(short);
		ReadBytesInternal((byte*)(&num), 2);
		if (!IsLittleEndian)
		{
			return num;
		}
		return ByteSwap(num);
	}

	public ushort ReadUShortNetworkByteOrder()
	{
		return (ushort)ReadShortNetworkByteOrder();
	}

	public unsafe int ReadIntNetworkByteOrder()
	{
		int num = default(int);
		ReadBytesInternal((byte*)(&num), 4);
		if (!IsLittleEndian)
		{
			return num;
		}
		return ByteSwap(num);
	}

	public uint ReadUIntNetworkByteOrder()
	{
		return (uint)ReadIntNetworkByteOrder();
	}

	public float ReadFloat()
	{
		UIntFloat uIntFloat = default(UIntFloat);
		uIntFloat.intValue = (uint)ReadInt();
		return uIntFloat.floatValue;
	}

	public double ReadDouble()
	{
		UIntFloat uIntFloat = default(UIntFloat);
		uIntFloat.longValue = (ulong)ReadLong();
		return uIntFloat.doubleValue;
	}

	public uint ReadPackedUInt(in StreamCompressionModel model)
	{
		return ReadPackedUIntInternal(6, in model);
	}

	private unsafe uint ReadPackedUIntInternal(int maxSymbolLength, in StreamCompressionModel model)
	{
		FillBitBuffer();
		uint num = (uint)((1 << maxSymbolLength) - 1);
		uint num2 = (uint)(int)m_Context.m_BitBuffer & num;
		ushort num3 = model.decodeTable[(int)num2];
		int num4 = num3 >> 8;
		int num5 = num3 & 0xFF;
		if (m_Context.m_BitIndex < num5)
		{
			m_Context.m_FailedReads++;
			return 0u;
		}
		m_Context.m_BitBuffer >>= num5;
		m_Context.m_BitIndex -= num5;
		uint num6 = model.bucketOffsets[num4];
		byte numbits = model.bucketSizes[num4];
		return ReadRawBitsInternal(numbits) + num6;
	}

	private unsafe void FillBitBuffer()
	{
		while (m_Context.m_BitIndex <= 56 && m_Context.m_ReadByteIndex < m_Length)
		{
			m_Context.m_BitBuffer |= (ulong)m_BufferPtr[m_Context.m_ReadByteIndex++] << m_Context.m_BitIndex;
			m_Context.m_BitIndex += 8;
		}
	}

	private uint ReadRawBitsInternal(int numbits)
	{
		if (m_Context.m_BitIndex < numbits)
		{
			m_Context.m_FailedReads++;
			return 0u;
		}
		int result = (int)((long)m_Context.m_BitBuffer & ((1L << numbits) - 1));
		m_Context.m_BitBuffer >>= numbits;
		m_Context.m_BitIndex -= numbits;
		return (uint)result;
	}

	public uint ReadRawBits(int numbits)
	{
		FillBitBuffer();
		return ReadRawBitsInternal(numbits);
	}

	public ulong ReadPackedULong(in StreamCompressionModel model)
	{
		return ReadPackedUInt(in model) | ((ulong)ReadPackedUInt(in model) << 32);
	}

	public int ReadPackedInt(in StreamCompressionModel model)
	{
		uint num = ReadPackedUInt(in model);
		return (int)((num >> 1) ^ (0 - (num & 1)));
	}

	public long ReadPackedLong(in StreamCompressionModel model)
	{
		ulong num = ReadPackedULong(in model);
		return (long)((num >> 1) ^ (0L - (num & 1)));
	}

	public float ReadPackedFloat(in StreamCompressionModel model)
	{
		return ReadPackedFloatDelta(0f, in model);
	}

	public double ReadPackedDouble(in StreamCompressionModel model)
	{
		return ReadPackedDoubleDelta(0.0, in model);
	}

	public int ReadPackedIntDelta(int baseline, in StreamCompressionModel model)
	{
		int num = ReadPackedInt(in model);
		return baseline - num;
	}

	public uint ReadPackedUIntDelta(uint baseline, in StreamCompressionModel model)
	{
		uint num = (uint)ReadPackedInt(in model);
		return baseline - num;
	}

	public long ReadPackedLongDelta(long baseline, in StreamCompressionModel model)
	{
		long num = ReadPackedLong(in model);
		return baseline - num;
	}

	public ulong ReadPackedULongDelta(ulong baseline, in StreamCompressionModel model)
	{
		ulong num = (ulong)ReadPackedLong(in model);
		return baseline - num;
	}

	public float ReadPackedFloatDelta(float baseline, in StreamCompressionModel model)
	{
		FillBitBuffer();
		if (ReadRawBitsInternal(1) == 0)
		{
			return baseline;
		}
		int numbits = 32;
		UIntFloat uIntFloat = default(UIntFloat);
		uIntFloat.intValue = ReadRawBitsInternal(numbits);
		return uIntFloat.floatValue;
	}

	public unsafe double ReadPackedDoubleDelta(double baseline, in StreamCompressionModel model)
	{
		FillBitBuffer();
		if (ReadRawBitsInternal(1) == 0)
		{
			return baseline;
		}
		int numbits = 32;
		UIntFloat uIntFloat = default(UIntFloat);
		uint* ptr = (uint*)(&uIntFloat.longValue);
		*ptr = ReadRawBitsInternal(numbits);
		FillBitBuffer();
		ptr[1] |= ReadRawBitsInternal(numbits);
		return uIntFloat.doubleValue;
	}

	public unsafe FixedString32Bytes ReadFixedString32()
	{
		FixedString32Bytes result = default(FixedString32Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadFixedStringInternal(data, result.Capacity);
		return result;
	}

	public unsafe FixedString64Bytes ReadFixedString64()
	{
		FixedString64Bytes result = default(FixedString64Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadFixedStringInternal(data, result.Capacity);
		return result;
	}

	public unsafe FixedString128Bytes ReadFixedString128()
	{
		FixedString128Bytes result = default(FixedString128Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadFixedStringInternal(data, result.Capacity);
		return result;
	}

	public unsafe FixedString512Bytes ReadFixedString512()
	{
		FixedString512Bytes result = default(FixedString512Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadFixedStringInternal(data, result.Capacity);
		return result;
	}

	public unsafe FixedString4096Bytes ReadFixedString4096()
	{
		FixedString4096Bytes result = default(FixedString4096Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadFixedStringInternal(data, result.Capacity);
		return result;
	}

	public unsafe ushort ReadFixedString(NativeArray<byte> array)
	{
		return ReadFixedStringInternal((byte*)array.GetUnsafePtr(), array.Length);
	}

	private unsafe ushort ReadFixedStringInternal(byte* data, int maxLength)
	{
		ushort num = ReadUShort();
		if (num > maxLength)
		{
			return 0;
		}
		ReadBytesInternal(data, num);
		return num;
	}

	public unsafe FixedString32Bytes ReadPackedFixedString32Delta(FixedString32Bytes baseline, in StreamCompressionModel model)
	{
		FixedString32Bytes result = default(FixedString32Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadPackedFixedStringDeltaInternal(data, result.Capacity, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
		return result;
	}

	public unsafe FixedString64Bytes ReadPackedFixedString64Delta(FixedString64Bytes baseline, in StreamCompressionModel model)
	{
		FixedString64Bytes result = default(FixedString64Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadPackedFixedStringDeltaInternal(data, result.Capacity, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
		return result;
	}

	public unsafe FixedString128Bytes ReadPackedFixedString128Delta(FixedString128Bytes baseline, in StreamCompressionModel model)
	{
		FixedString128Bytes result = default(FixedString128Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadPackedFixedStringDeltaInternal(data, result.Capacity, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
		return result;
	}

	public unsafe FixedString512Bytes ReadPackedFixedString512Delta(FixedString512Bytes baseline, in StreamCompressionModel model)
	{
		FixedString512Bytes result = default(FixedString512Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadPackedFixedStringDeltaInternal(data, result.Capacity, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
		return result;
	}

	public unsafe FixedString4096Bytes ReadPackedFixedString4096Delta(FixedString4096Bytes baseline, in StreamCompressionModel model)
	{
		FixedString4096Bytes result = default(FixedString4096Bytes);
		byte* data = (byte*)(&result) + 2;
		*(ushort*)(&result) = ReadPackedFixedStringDeltaInternal(data, result.Capacity, (byte*)(&baseline) + 2, *(ushort*)(&baseline), in model);
		return result;
	}

	public unsafe ushort ReadPackedFixedStringDelta(NativeArray<byte> data, NativeArray<byte> baseData, in StreamCompressionModel model)
	{
		return ReadPackedFixedStringDeltaInternal((byte*)data.GetUnsafePtr(), data.Length, (byte*)baseData.GetUnsafePtr(), (ushort)baseData.Length, in model);
	}

	private unsafe ushort ReadPackedFixedStringDeltaInternal(byte* data, int maxLength, byte* baseData, ushort baseLength, in StreamCompressionModel model)
	{
		uint num = ReadPackedUIntDelta(baseLength, in model);
		if (num > (uint)maxLength)
		{
			return 0;
		}
		if (num <= baseLength)
		{
			for (int i = 0; i < num; i++)
			{
				data[i] = (byte)ReadPackedUIntDelta(baseData[i], in model);
			}
		}
		else
		{
			for (int j = 0; j < baseLength; j++)
			{
				data[j] = (byte)ReadPackedUIntDelta(baseData[j], in model);
			}
			for (int k = baseLength; k < num; k++)
			{
				data[k] = (byte)ReadPackedUInt(in model);
			}
		}
		return (ushort)num;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	internal readonly void CheckRead()
	{
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckBits(int numBits)
	{
		if (numBits < 0 || numBits > 32)
		{
			throw new ArgumentOutOfRangeException($"Invalid number of bits specified: {numBits}! Valid range is (0, 32) inclusive.");
		}
	}
}
