using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Mathematics;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public struct StreamCompressionModel
{
	private static class SharedStaticCompressionModel
	{
		internal static readonly SharedStatic<StreamCompressionModel> Default = SharedStatic<StreamCompressionModel>.GetOrCreateUnsafe(0u, 6564095697914452312L, 0L);
	}

	internal static readonly byte[] k_BucketSizes = new byte[16]
	{
		0, 0, 1, 2, 3, 4, 6, 8, 10, 12,
		15, 18, 21, 24, 27, 32
	};

	internal static readonly uint[] k_BucketOffsets = new uint[16]
	{
		0u, 1u, 2u, 4u, 8u, 16u, 32u, 96u, 352u, 1376u,
		5472u, 38240u, 300384u, 2397536u, 19174752u, 153392480u
	};

	internal static readonly int[] k_FirstBucketCandidate = new int[33]
	{
		15, 15, 15, 15, 14, 14, 14, 13, 13, 13,
		12, 12, 12, 11, 11, 11, 10, 10, 10, 9,
		9, 8, 8, 7, 7, 6, 5, 4, 3, 2,
		1, 1, 0
	};

	internal static readonly byte[] k_DefaultModelData = new byte[19]
	{
		16, 2, 3, 3, 3, 4, 4, 4, 5, 5,
		5, 6, 6, 6, 6, 6, 6, 0, 0
	};

	internal const int k_AlphabetSize = 16;

	internal const int k_MaxHuffmanSymbolLength = 6;

	internal const int k_MaxContexts = 1;

	private byte m_Initialized;

	internal unsafe fixed ushort encodeTable[16];

	internal unsafe fixed ushort decodeTable[64];

	internal unsafe fixed byte bucketSizes[16];

	internal unsafe fixed uint bucketOffsets[16];

	public static StreamCompressionModel Default
	{
		get
		{
			if (SharedStaticCompressionModel.Default.Data.m_Initialized == 1)
			{
				return SharedStaticCompressionModel.Default.Data;
			}
			Initialize();
			SharedStaticCompressionModel.Default.Data.m_Initialized = 1;
			return SharedStaticCompressionModel.Default.Data;
		}
	}

	private unsafe static void Initialize()
	{
		for (int i = 0; i < 16; i++)
		{
			SharedStaticCompressionModel.Default.Data.bucketSizes[i] = k_BucketSizes[i];
			SharedStaticCompressionModel.Default.Data.bucketOffsets[i] = k_BucketOffsets[i];
		}
		NativeArray<byte> nativeArray = new NativeArray<byte>(k_DefaultModelData.Length, Allocator.Temp);
		for (int j = 0; j < k_DefaultModelData.Length; j++)
		{
			nativeArray[j] = k_DefaultModelData[j];
		}
		int num = 1;
		NativeArray<byte> nativeArray2 = new NativeArray<byte>(num * 16, Allocator.Temp);
		int num2 = 0;
		_ = nativeArray[num2++];
		for (int k = 0; k < 16; k++)
		{
			byte value = nativeArray[num2++];
			for (int l = 0; l < num; l++)
			{
				nativeArray2[num * l + k] = value;
			}
		}
		int num3 = nativeArray[num2] | (nativeArray[num2 + 1] << 8);
		num2 += 2;
		for (int m = 0; m < num3; m++)
		{
			int num4 = nativeArray[num2] | (nativeArray[num2 + 1] << 8);
			num2 += 2;
			_ = nativeArray[num2++];
			for (int n = 0; n < 16; n++)
			{
				byte value2 = nativeArray[num2++];
				nativeArray2[num * num4 + n] = value2;
			}
		}
		NativeArray<byte> symbolLengths = new NativeArray<byte>(16, Allocator.Temp);
		NativeArray<ushort> nativeArray3 = new NativeArray<ushort>(64, Allocator.Temp);
		NativeArray<byte> symbolCodes = new NativeArray<byte>(16, Allocator.Temp);
		for (int num5 = 0; num5 < num; num5++)
		{
			for (int num6 = 0; num6 < 16; num6++)
			{
				symbolLengths[num6] = nativeArray2[num * num5 + num6];
			}
			GenerateHuffmanCodes(symbolCodes, 0, symbolLengths, 0, 16, 6);
			GenerateHuffmanDecodeTable(nativeArray3, 0, symbolLengths, symbolCodes, 16, 6);
			for (int num7 = 0; num7 < 16; num7++)
			{
				SharedStaticCompressionModel.Default.Data.encodeTable[num5 * 16 + num7] = (ushort)((symbolCodes[num7] << 8) | nativeArray2[num * num5 + num7]);
			}
			for (int num8 = 0; num8 < 64; num8++)
			{
				SharedStaticCompressionModel.Default.Data.decodeTable[num5 * 64 + num8] = nativeArray3[num8];
			}
		}
	}

	private static void GenerateHuffmanCodes(NativeArray<byte> symbolCodes, int symbolCodesOffset, NativeArray<byte> symbolLengths, int symbolLengthsOffset, int alphabetSize, int maxCodeLength)
	{
		NativeArray<byte> nativeArray = new NativeArray<byte>(maxCodeLength + 1, Allocator.Temp);
		NativeArray<byte> nativeArray2 = new NativeArray<byte>((maxCodeLength + 1) * alphabetSize, Allocator.Temp);
		for (int i = 0; i < alphabetSize; i++)
		{
			int num = symbolLengths[i + symbolLengthsOffset];
			nativeArray2[(maxCodeLength + 1) * num + nativeArray[num]++] = (byte)i;
		}
		uint num2 = 0u;
		for (int j = 1; j <= maxCodeLength; j++)
		{
			int num3 = nativeArray[j];
			for (int k = 0; k < num3; k++)
			{
				int num4 = nativeArray2[(maxCodeLength + 1) * j + k];
				symbolCodes[num4 + symbolCodesOffset] = (byte)ReverseBits(num2++, j);
			}
			num2 <<= 1;
		}
	}

	private static uint ReverseBits(uint value, int num_bits)
	{
		value = ((value & 0x55555555) << 1) | ((value & 0xAAAAAAAAu) >> 1);
		value = ((value & 0x33333333) << 2) | ((value & 0xCCCCCCCCu) >> 2);
		value = ((value & 0xF0F0F0F) << 4) | ((value & 0xF0F0F0F0u) >> 4);
		value = ((value & 0xFF00FF) << 8) | ((value & 0xFF00FF00u) >> 8);
		value = (value << 16) | (value >> 16);
		return value >> 32 - num_bits;
	}

	private static void GenerateHuffmanDecodeTable(NativeArray<ushort> decodeTable, int decodeTableOffset, NativeArray<byte> symbolLengths, NativeArray<byte> symbolCodes, int alphabetSize, int maxCodeLength)
	{
		uint num = (uint)(1 << maxCodeLength);
		for (int i = 0; i < alphabetSize; i++)
		{
			int num2 = symbolLengths[i];
			if (num2 > 0)
			{
				uint num3 = symbolCodes[i];
				uint num4 = (uint)(1 << num2);
				do
				{
					decodeTable[(int)(decodeTableOffset + num3)] = (ushort)((i << 8) | num2);
					num3 += num4;
				}
				while (num3 < num);
			}
		}
	}

	public unsafe readonly int CalculateBucket(uint value)
	{
		int num = k_FirstBucketCandidate[math.lzcnt(value)];
		if (num + 1 < 16 && value >= bucketOffsets[num + 1])
		{
			num++;
		}
		return num;
	}

	public unsafe readonly int GetCompressedSizeInBits(uint value)
	{
		int num = CalculateBucket(value);
		int num2 = bucketSizes[num];
		return (encodeTable[num] & 0xFF) + num2;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckAlphabetSize(int alphabetSize)
	{
		if (alphabetSize != 16)
		{
			throw new InvalidOperationException("The alphabet size of compression models must be " + 16);
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckSymbolLength(NativeArray<byte> symbolLengths, int symbolLengthsOffset, int symbol, int length)
	{
		if (symbolLengths[symbol + symbolLengthsOffset] != length)
		{
			throw new InvalidOperationException("Incorrect symbol length");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckAlphabetAndMaxCodeLength(int alphabetSize, int maxCodeLength)
	{
		if (alphabetSize > 256 || maxCodeLength > 8)
		{
			throw new InvalidOperationException("Can only generate huffman codes up to alphabet size 256 and maximum code length 8");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckExceedMaxCodeLength(int length, int maxCodeLength)
	{
		if (length > maxCodeLength)
		{
			throw new InvalidOperationException("Maximum code length exceeded");
		}
	}
}
