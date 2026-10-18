using System.Runtime.CompilerServices;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Engine;

internal class LL32 : LL
{
	protected static cParams_t[] clTable = new cParams_t[13]
	{
		new cParams_t(lz4hc_strat_e.lz4hc, 2u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 2u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 2u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 4u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 8u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 16u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 32u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 64u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 128u, 16u),
		new cParams_t(lz4hc_strat_e.lz4hc, 256u, 16u),
		new cParams_t(lz4hc_strat_e.lz4opt, 96u, 64u),
		new cParams_t(lz4hc_strat_e.lz4opt, 512u, 128u),
		new cParams_t(lz4hc_strat_e.lz4opt, 16384u, 4096u)
	};

	protected const int ALGORITHM_ARCH = 4;

	private static readonly uint[] _DeBruijnBytePos = new uint[32]
	{
		0u, 0u, 3u, 0u, 3u, 1u, 3u, 0u, 3u, 2u,
		2u, 1u, 3u, 2u, 0u, 1u, 3u, 3u, 1u, 2u,
		2u, 2u, 2u, 0u, 3u, 1u, 2u, 0u, 1u, 0u,
		1u, 1u
	};

	private unsafe static readonly uint* DeBruijnBytePos = Mem.CloneArray(_DeBruijnBytePos);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4_decompress_generic(byte* src, byte* dst, int srcSize, int outputSize, endCondition_directive endOnInput, earlyEnd_directive partialDecoding, dict_directive dict, byte* lowPrefix, byte* dictStart, uint dictSize)
	{
		return LZ4_decompress_generic(src, dst, srcSize, outputSize, endOnInput == endCondition_directive.endOnInputSize, partialDecoding == earlyEnd_directive.partial, dict, lowPrefix, dictStart, dictSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4_decompress_generic(byte* src, byte* dst, int srcSize, int outputSize, bool endOnInput, bool partialDecoding, dict_directive dict, byte* lowPrefix, byte* dictStart, uint dictSize)
	{
		if (src == null)
		{
			return -1;
		}
		byte* ptr = src;
		byte* ptr2 = ptr + srcSize;
		byte* ptr3 = dst;
		byte* ptr4 = ptr3 + outputSize;
		byte* ptr5 = ((dictStart == null) ? null : (dictStart + dictSize));
		bool flag = endOnInput;
		bool flag2 = flag && dictSize < 65536;
		byte* ptr6 = ptr2 - (endOnInput ? 14 : 8) - 2;
		byte* ptr7 = ptr4 - (endOnInput ? 14 : 8) - 18;
		if (endOnInput && outputSize == 0)
		{
			if (partialDecoding)
			{
				return 0;
			}
			if (srcSize != 1 || *ptr != 0)
			{
				return -1;
			}
			return 0;
		}
		if (!endOnInput && outputSize == 0)
		{
			if (*ptr != 0)
			{
				return -1;
			}
			return 1;
		}
		if (endOnInput && srcSize == 0)
		{
			return -1;
		}
		while (true)
		{
			uint num = *(ptr++);
			uint num2 = num >> 4;
			uint num3;
			byte* ptr9;
			byte* ptr8;
			if ((endOnInput ? (num2 != 15) : (num2 <= 8)) && (!endOnInput || ptr < ptr6) && ptr3 <= ptr7)
			{
				if (endOnInput)
				{
					Mem32.Copy16(ptr3, ptr);
				}
				else
				{
					Mem.Copy8(ptr3, ptr);
				}
				ptr3 += num2;
				ptr += num2;
				num2 = num & 0xFu;
				num3 = Mem.Peek2(ptr);
				ptr += 2;
				ptr8 = ptr3 - num3;
				if (num2 != 15 && num3 >= 8 && (dict == dict_directive.withPrefix64k || ptr8 >= lowPrefix))
				{
					Mem32.Copy18(ptr3, ptr8);
					ptr3 += num2 + 4;
					continue;
				}
			}
			else
			{
				if (num2 == 15)
				{
					variable_length_error variable_length_error = variable_length_error.ok;
					num2 += LL.LZ4_readVLE(&ptr, ptr2 - 15, endOnInput, endOnInput, &variable_length_error);
					if (variable_length_error == variable_length_error.initial_error || (flag && ptr3 + num2 < ptr3) || (flag && ptr + num2 < ptr))
					{
						break;
					}
				}
				ptr9 = ptr3 + num2;
				if ((endOnInput && (ptr9 > ptr4 - 12 || ptr + num2 > ptr2 - 8)) || (!endOnInput && ptr9 > ptr4 - 8))
				{
					if (partialDecoding)
					{
						if (ptr + num2 > ptr2 - 8 && ptr + num2 != ptr2)
						{
							break;
						}
						if (ptr9 > ptr4)
						{
							ptr9 = ptr4;
							num2 = (uint)(ptr4 - ptr3);
						}
					}
					else if ((!endOnInput && ptr9 != ptr4) || (endOnInput && (ptr + num2 != ptr2 || ptr9 > ptr4)))
					{
						break;
					}
					Mem.Move(ptr3, ptr, (int)num2);
					ptr += num2;
					ptr3 += num2;
					if (!partialDecoding || ptr9 == ptr4 || ptr == ptr2)
					{
						goto IL_04a7;
					}
				}
				else
				{
					Mem32.WildCopy8(ptr3, ptr, ptr9);
					ptr += num2;
					ptr3 = ptr9;
				}
				num3 = Mem.Peek2(ptr);
				ptr += 2;
				ptr8 = ptr3 - num3;
				num2 = num & 0xFu;
			}
			if (num2 == 15)
			{
				variable_length_error variable_length_error2 = variable_length_error.ok;
				num2 += LL.LZ4_readVLE(&ptr, ptr2 - 5 + 1, endOnInput, initial_check: false, &variable_length_error2);
				if (variable_length_error2 != 0 || (flag && ptr3 + num2 < ptr3))
				{
					break;
				}
			}
			num2 += 4;
			if (flag2 && ptr8 + dictSize < lowPrefix)
			{
				break;
			}
			if (dict == dict_directive.usingExtDict && ptr8 < lowPrefix)
			{
				if (ptr3 + num2 > ptr4 - 5)
				{
					if (!partialDecoding)
					{
						break;
					}
					num2 = LL.MIN(num2, (uint)(ptr4 - ptr3));
				}
				if (num2 <= (uint)(lowPrefix - ptr8))
				{
					Mem.Move(ptr3, ptr5 - (lowPrefix - ptr8), (int)num2);
					ptr3 += num2;
					continue;
				}
				uint num4 = (uint)(lowPrefix - ptr8);
				uint num5 = num2 - num4;
				Mem.Copy(ptr3, ptr5 - num4, (int)num4);
				ptr3 += num4;
				if (num5 > (uint)(ptr3 - lowPrefix))
				{
					byte* ptr10 = ptr3 + num5;
					byte* ptr11 = lowPrefix;
					while (ptr3 < ptr10)
					{
						*(ptr3++) = *(ptr11++);
					}
				}
				else
				{
					Mem.Copy(ptr3, lowPrefix, (int)num5);
					ptr3 += num5;
				}
				continue;
			}
			ptr9 = ptr3 + num2;
			if (partialDecoding && ptr9 > ptr4 - 12)
			{
				uint num6 = LL.MIN(num2, (uint)(ptr4 - ptr3));
				byte* num7 = ptr8 + num6;
				byte* ptr12 = ptr3 + num6;
				if (num7 > ptr3)
				{
					while (ptr3 < ptr12)
					{
						*(ptr3++) = *(ptr8++);
					}
				}
				else
				{
					Mem.Copy(ptr3, ptr8, (int)num6);
				}
				ptr3 = ptr12;
				if (ptr3 != ptr4)
				{
					continue;
				}
				goto IL_04a7;
			}
			if (num3 < 8)
			{
				*ptr3 = *ptr8;
				ptr3[1] = ptr8[1];
				ptr3[2] = ptr8[2];
				ptr3[3] = ptr8[3];
				ptr8 += LL.inc32table[num3];
				Mem.Copy4(ptr3 + 4, ptr8);
				ptr8 -= LL.dec64table[num3];
			}
			else
			{
				Mem.Copy8(ptr3, ptr8);
				ptr8 += 8;
			}
			ptr3 += 8;
			if (ptr9 > ptr4 - 12)
			{
				byte* ptr13 = ptr4 - 7;
				if (ptr9 > ptr4 - 5)
				{
					break;
				}
				if (ptr3 < ptr13)
				{
					Mem32.WildCopy8(ptr3, ptr8, ptr13);
					ptr8 += ptr13 - ptr3;
					ptr3 = ptr13;
				}
				while (ptr3 < ptr9)
				{
					*(ptr3++) = *(ptr8++);
				}
			}
			else
			{
				Mem.Copy8(ptr3, ptr8);
				if (num2 > 16)
				{
					Mem32.WildCopy8(ptr3 + 8, ptr8 + 8, ptr9);
				}
			}
			ptr3 = ptr9;
			continue;
			IL_04a7:
			if (endOnInput)
			{
				return (int)(ptr3 - dst);
			}
			return (int)(ptr - src);
		}
		return (int)(-(ptr - src)) - 1;
	}

	public unsafe static int LZ4_decompress_safe(byte* source, byte* dest, int compressedSize, int maxDecompressedSize)
	{
		return LZ4_decompress_generic(source, dest, compressedSize, maxDecompressedSize, endCondition_directive.endOnInputSize, earlyEnd_directive.full, dict_directive.noDict, dest, null, 0u);
	}

	public unsafe static int LZ4_decompress_safe_withPrefix64k(byte* source, byte* dest, int compressedSize, int maxOutputSize)
	{
		return LZ4_decompress_generic(source, dest, compressedSize, maxOutputSize, endCondition_directive.endOnInputSize, earlyEnd_directive.full, dict_directive.withPrefix64k, dest - 65536, null, 0u);
	}

	public unsafe static int LZ4_decompress_safe_withSmallPrefix(byte* source, byte* dest, int compressedSize, int maxOutputSize, uint prefixSize)
	{
		return LZ4_decompress_generic(source, dest, compressedSize, maxOutputSize, endCondition_directive.endOnInputSize, earlyEnd_directive.full, dict_directive.noDict, dest - prefixSize, null, 0u);
	}

	public unsafe static int LZ4_decompress_safe_doubleDict(byte* source, byte* dest, int compressedSize, int maxOutputSize, uint prefixSize, void* dictStart, uint dictSize)
	{
		return LZ4_decompress_generic(source, dest, compressedSize, maxOutputSize, endCondition_directive.endOnInputSize, earlyEnd_directive.full, dict_directive.usingExtDict, dest - prefixSize, (byte*)dictStart, dictSize);
	}

	public unsafe static int LZ4_decompress_safe_forceExtDict(byte* source, byte* dest, int compressedSize, int maxOutputSize, void* dictStart, uint dictSize)
	{
		return LZ4_decompress_generic(source, dest, compressedSize, maxOutputSize, endCondition_directive.endOnInputSize, earlyEnd_directive.full, dict_directive.usingExtDict, dest, (byte*)dictStart, dictSize);
	}

	public unsafe static int LZ4_decompress_safe_usingDict(byte* source, byte* dest, int compressedSize, int maxOutputSize, byte* dictStart, int dictSize)
	{
		if (dictSize == 0)
		{
			return LZ4_decompress_safe(source, dest, compressedSize, maxOutputSize);
		}
		if (dictStart + dictSize == dest)
		{
			if (dictSize >= 65535)
			{
				return LZ4_decompress_safe_withPrefix64k(source, dest, compressedSize, maxOutputSize);
			}
			return LZ4_decompress_safe_withSmallPrefix(source, dest, compressedSize, maxOutputSize, (uint)dictSize);
		}
		return LZ4_decompress_safe_forceExtDict(source, dest, compressedSize, maxOutputSize, dictStart, (uint)dictSize);
	}

	public unsafe static int LZ4_decompress_safe_partial(byte* src, byte* dst, int compressedSize, int targetOutputSize, int dstCapacity)
	{
		uint outputSize = LL.MIN((uint)targetOutputSize, (uint)dstCapacity);
		return LZ4_decompress_generic(src, dst, compressedSize, (int)outputSize, endCondition_directive.endOnInputSize, earlyEnd_directive.partial, dict_directive.noDict, dst, null, 0u);
	}

	public unsafe static int LZ4_decompress_safe_continue(LZ4_streamDecode_t* LZ4_streamDecode, byte* source, byte* dest, int compressedSize, int maxOutputSize)
	{
		int num;
		if (LZ4_streamDecode->prefixSize == 0)
		{
			num = LZ4_decompress_safe(source, dest, compressedSize, maxOutputSize);
			if (num <= 0)
			{
				return num;
			}
			LZ4_streamDecode->prefixSize = (uint)num;
			LZ4_streamDecode->prefixEnd = dest + num;
		}
		else if (LZ4_streamDecode->prefixEnd == dest)
		{
			num = ((LZ4_streamDecode->prefixSize >= 65535) ? LZ4_decompress_safe_withPrefix64k(source, dest, compressedSize, maxOutputSize) : ((LZ4_streamDecode->extDictSize != 0) ? LZ4_decompress_safe_doubleDict(source, dest, compressedSize, maxOutputSize, LZ4_streamDecode->prefixSize, LZ4_streamDecode->externalDict, LZ4_streamDecode->extDictSize) : LZ4_decompress_safe_withSmallPrefix(source, dest, compressedSize, maxOutputSize, LZ4_streamDecode->prefixSize)));
			if (num <= 0)
			{
				return num;
			}
			LZ4_streamDecode->prefixSize += (uint)num;
			LZ4_streamDecode->prefixEnd += num;
		}
		else
		{
			LZ4_streamDecode->extDictSize = LZ4_streamDecode->prefixSize;
			LZ4_streamDecode->externalDict = LZ4_streamDecode->prefixEnd - LZ4_streamDecode->extDictSize;
			num = LZ4_decompress_safe_forceExtDict(source, dest, compressedSize, maxOutputSize, LZ4_streamDecode->externalDict, LZ4_streamDecode->extDictSize);
			if (num <= 0)
			{
				return num;
			}
			LZ4_streamDecode->prefixSize = (uint)num;
			LZ4_streamDecode->prefixEnd = dest + num;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static int LZ4_compress_generic(LZ4_stream_t* cctx, byte* source, byte* dest, int inputSize, int* inputConsumed, int maxOutputSize, limitedOutput_directive outputDirective, tableType_t tableType, dict_directive dictDirective, dictIssue_directive dictIssue, int acceleration)
	{
		byte* ptr = source;
		uint currentOffset = cctx->currentOffset;
		byte* ptr2 = source - currentOffset;
		LZ4_stream_t* dictCtx = cctx->dictCtx;
		byte* ptr3 = ((dictDirective == dict_directive.usingDictCtx) ? dictCtx->dictionary : cctx->dictionary);
		uint num = ((dictDirective == dict_directive.usingDictCtx) ? dictCtx->dictSize : cctx->dictSize);
		uint num2 = ((dictDirective == dict_directive.usingDictCtx) ? (currentOffset - dictCtx->currentOffset) : 0u);
		bool flag = dictDirective == dict_directive.usingExtDict || dictDirective == dict_directive.usingDictCtx;
		uint num3 = currentOffset - num;
		byte* ptr4 = ptr3 + num;
		byte* ptr5 = source;
		byte* ptr6 = ptr + inputSize;
		byte* ptr7 = ptr6 - 12 + 1;
		byte* ptr8 = ptr6 - 5;
		byte* ptr9 = ((dictDirective == dict_directive.usingDictCtx) ? (ptr3 + num - dictCtx->currentOffset) : (ptr3 + num - currentOffset));
		byte* ptr10 = dest;
		byte* ptr11 = ptr10 + maxOutputSize;
		uint num4 = 0u;
		if (outputDirective == limitedOutput_directive.fillOutput && maxOutputSize < 1)
		{
			return 0;
		}
		if ((uint)inputSize > 2113929216u)
		{
			return 0;
		}
		if (tableType == tableType_t.byU16 && inputSize >= 65547)
		{
			return 0;
		}
		_ = 1;
		byte* ptr12 = source - ((dictDirective == dict_directive.withPrefix64k) ? num : 0);
		if (dictDirective == dict_directive.usingDictCtx)
		{
			cctx->dictCtx = null;
			cctx->dictSize = (uint)inputSize;
		}
		else
		{
			cctx->dictSize += (uint)inputSize;
		}
		cctx->currentOffset += (uint)inputSize;
		cctx->tableType = tableType;
		if (inputSize >= 13)
		{
			LZ4_putPosition(ptr, cctx->hashTable, tableType, ptr2);
			ptr++;
			uint num5 = LZ4_hashPosition(ptr, tableType);
			while (true)
			{
				byte* ptr14;
				if (tableType == tableType_t.byPtr)
				{
					byte* ptr13 = ptr;
					int num6 = 1;
					int num7 = acceleration << 6;
					while (true)
					{
						uint h = num5;
						ptr = ptr13;
						ptr13 += num6;
						num6 = num7++ >> 6;
						if (ptr13 > ptr7)
						{
							break;
						}
						ptr14 = LL.LZ4_getPositionOnHash(h, cctx->hashTable, tableType, ptr2);
						num5 = LZ4_hashPosition(ptr13, tableType);
						LL.LZ4_putPositionOnHash(ptr, h, cctx->hashTable, tableType, ptr2);
						if (ptr14 + 65535 < ptr || Mem.Peek4(ptr14) != Mem.Peek4(ptr))
						{
							continue;
						}
						goto IL_02f6;
					}
					break;
				}
				byte* ptr15 = ptr;
				int num8 = 1;
				int num9 = acceleration << 6;
				uint num10;
				uint num11;
				while (true)
				{
					uint h2 = num5;
					num10 = (uint)(ptr15 - ptr2);
					num11 = LL.LZ4_getIndexOnHash(h2, cctx->hashTable, tableType);
					ptr = ptr15;
					ptr15 += num8;
					num8 = num9++ >> 6;
					if (ptr15 > ptr7)
					{
						break;
					}
					switch (dictDirective)
					{
					case dict_directive.usingDictCtx:
						if (num11 < currentOffset)
						{
							num11 = LL.LZ4_getIndexOnHash(h2, dictCtx->hashTable, tableType_t.byU32);
							ptr14 = ptr9 + num11;
							num11 += num2;
							ptr12 = ptr3;
						}
						else
						{
							ptr14 = ptr2 + num11;
							ptr12 = source;
						}
						break;
					case dict_directive.usingExtDict:
						if (num11 < currentOffset)
						{
							ptr14 = ptr9 + num11;
							ptr12 = ptr3;
						}
						else
						{
							ptr14 = ptr2 + num11;
							ptr12 = source;
						}
						break;
					default:
						ptr14 = ptr2 + num11;
						break;
					}
					num5 = LZ4_hashPosition(ptr15, tableType);
					LL.LZ4_putIndexOnHash(num10, h2, cctx->hashTable, tableType);
					if ((dictIssue == dictIssue_directive.dictSmall && num11 < num3) || (tableType != tableType_t.byU16 && num11 + 65535 < num10) || Mem.Peek4(ptr14) != Mem.Peek4(ptr))
					{
						continue;
					}
					goto IL_02eb;
				}
				break;
				IL_02f6:
				byte* ptr16 = ptr;
				while (ptr > ptr5 && ptr14 > ptr12 && ptr[-1] == ptr14[-1])
				{
					ptr--;
					ptr14--;
				}
				uint num12 = (uint)(ptr - ptr5);
				byte* ptr17 = ptr10++;
				if (outputDirective == limitedOutput_directive.limitedOutput && ptr10 + num12 + 8 + num12 / 255 > ptr11)
				{
					return 0;
				}
				if (outputDirective == limitedOutput_directive.fillOutput && ptr10 + (num12 + 240) / 255 + num12 + 2 + 1 + 12 - 4 > ptr11)
				{
					ptr10--;
					break;
				}
				if (num12 >= 15)
				{
					int num13 = (int)(num12 - 15);
					*ptr17 = 240;
					while (num13 >= 255)
					{
						*(ptr10++) = byte.MaxValue;
						num13 -= 255;
					}
					*(ptr10++) = (byte)num13;
				}
				else
				{
					*ptr17 = (byte)(num12 << 4);
				}
				Mem32.WildCopy8(ptr10, ptr5, ptr10 + num12);
				ptr10 += num12;
				while (true)
				{
					if (outputDirective == limitedOutput_directive.fillOutput && ptr10 + 2 + 1 + 12 - 4 > ptr11)
					{
						ptr10 = ptr17;
						break;
					}
					if (flag)
					{
						Mem.Poke2(ptr10, (ushort)num4);
						ptr10 += 2;
					}
					else
					{
						Mem.Poke2(ptr10, (ushort)(ptr - ptr14));
						ptr10 += 2;
					}
					uint num14;
					if ((dictDirective == dict_directive.usingExtDict || dictDirective == dict_directive.usingDictCtx) && ptr12 == ptr3)
					{
						byte* ptr18 = ptr + (ptr4 - ptr14);
						if (ptr18 > ptr8)
						{
							ptr18 = ptr8;
						}
						num14 = LZ4_count(ptr + 4, ptr14 + 4, ptr18);
						ptr += num14 + 4;
						if (ptr == ptr18)
						{
							uint num15 = LZ4_count(ptr18, source, ptr8);
							num14 += num15;
							ptr += num15;
						}
					}
					else
					{
						num14 = LZ4_count(ptr + 4, ptr14 + 4, ptr8);
						ptr += num14 + 4;
					}
					if (outputDirective != 0 && ptr10 + 6 + (num14 + 240) / 255 > ptr11)
					{
						if (outputDirective != limitedOutput_directive.fillOutput)
						{
							return 0;
						}
						uint num16 = (uint)(14 + ((int)(ptr11 - ptr10) - 1 - 5) * 255);
						ptr -= num14 - num16;
						num14 = num16;
						if (ptr <= ptr16)
						{
							for (byte* ptr19 = ptr; ptr19 <= ptr16; ptr19++)
							{
								LL.LZ4_clearHash(LZ4_hashPosition(ptr19, tableType), cctx->hashTable, tableType);
							}
						}
					}
					if (num14 >= 15)
					{
						byte* intPtr = ptr17;
						*intPtr += 15;
						num14 -= 15;
						Mem.Poke4(ptr10, uint.MaxValue);
						while (num14 >= 1020)
						{
							ptr10 += 4;
							Mem.Poke4(ptr10, uint.MaxValue);
							num14 -= 1020;
						}
						ptr10 += num14 / 255;
						*(ptr10++) = (byte)(num14 % 255);
					}
					else
					{
						byte* intPtr2 = ptr17;
						*intPtr2 += (byte)num14;
					}
					ptr5 = ptr;
					if (ptr >= ptr7)
					{
						break;
					}
					LZ4_putPosition(ptr - 2, cctx->hashTable, tableType, ptr2);
					if (tableType == tableType_t.byPtr)
					{
						ptr14 = LZ4_getPosition(ptr, cctx->hashTable, tableType, ptr2);
						LZ4_putPosition(ptr, cctx->hashTable, tableType, ptr2);
						if (ptr14 + 65535 >= ptr && Mem.Peek4(ptr14) == Mem.Peek4(ptr))
						{
							ptr17 = ptr10++;
							*ptr17 = 0;
							continue;
						}
					}
					else
					{
						uint h3 = LZ4_hashPosition(ptr, tableType);
						uint num17 = (uint)(ptr - ptr2);
						uint num18 = LL.LZ4_getIndexOnHash(h3, cctx->hashTable, tableType);
						switch (dictDirective)
						{
						case dict_directive.usingDictCtx:
							if (num18 < currentOffset)
							{
								num18 = LL.LZ4_getIndexOnHash(h3, dictCtx->hashTable, tableType_t.byU32);
								ptr14 = ptr9 + num18;
								ptr12 = ptr3;
								num18 += num2;
							}
							else
							{
								ptr14 = ptr2 + num18;
								ptr12 = source;
							}
							break;
						case dict_directive.usingExtDict:
							if (num18 < currentOffset)
							{
								ptr14 = ptr9 + num18;
								ptr12 = ptr3;
							}
							else
							{
								ptr14 = ptr2 + num18;
								ptr12 = source;
							}
							break;
						default:
							ptr14 = ptr2 + num18;
							break;
						}
						LL.LZ4_putIndexOnHash(num17, h3, cctx->hashTable, tableType);
						if ((dictIssue != dictIssue_directive.dictSmall || num18 >= num3) && (tableType == tableType_t.byU16 || num18 + 65535 >= num17) && Mem.Peek4(ptr14) == Mem.Peek4(ptr))
						{
							ptr17 = ptr10++;
							*ptr17 = 0;
							if (flag)
							{
								num4 = num17 - num18;
							}
							continue;
						}
					}
					goto IL_0703;
				}
				break;
				IL_0703:
				num5 = LZ4_hashPosition(++ptr, tableType);
				continue;
				IL_02eb:
				if (flag)
				{
					num4 = num10 - num11;
				}
				goto IL_02f6;
			}
		}
		uint num19 = (uint)(ptr6 - ptr5);
		if (outputDirective != 0 && ptr10 + num19 + 1 + (num19 + 255 - 15) / 255 > ptr11)
		{
			if (outputDirective != limitedOutput_directive.fillOutput)
			{
				return 0;
			}
			num19 = (uint)((int)(ptr11 - ptr10) - 1);
			num19 -= (num19 + 240) / 255;
		}
		if (num19 >= 15)
		{
			uint num20 = num19 - 15;
			*(ptr10++) = 240;
			while (num20 >= 255)
			{
				*(ptr10++) = byte.MaxValue;
				num20 -= 255;
			}
			*(ptr10++) = (byte)num20;
		}
		else
		{
			*(ptr10++) = (byte)(num19 << 4);
		}
		Mem.Copy(ptr10, ptr5, (int)num19);
		ptr = ptr5 + num19;
		ptr10 += num19;
		if (outputDirective == limitedOutput_directive.fillOutput)
		{
			*inputConsumed = (int)(ptr - source);
		}
		return (int)(ptr10 - dest);
	}

	public unsafe static int LZ4_compress_fast_extState(LZ4_stream_t* state, byte* source, byte* dest, int inputSize, int maxOutputSize, int acceleration)
	{
		LZ4_stream_t* cctx = LL.LZ4_initStream(state);
		if (acceleration < 1)
		{
			acceleration = 1;
		}
		if (maxOutputSize >= LL.LZ4_compressBound(inputSize))
		{
			if (inputSize < 65547)
			{
				return LZ4_compress_generic(cctx, source, dest, inputSize, null, 0, limitedOutput_directive.notLimited, tableType_t.byU16, dict_directive.noDict, dictIssue_directive.noDictIssue, acceleration);
			}
			tableType_t tableType = ((sizeof(void*) < 8 && (nuint)source > (nuint)65535u) ? tableType_t.byPtr : tableType_t.byU32);
			return LZ4_compress_generic(cctx, source, dest, inputSize, null, 0, limitedOutput_directive.notLimited, tableType, dict_directive.noDict, dictIssue_directive.noDictIssue, acceleration);
		}
		if (inputSize < 65547)
		{
			return LZ4_compress_generic(cctx, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType_t.byU16, dict_directive.noDict, dictIssue_directive.noDictIssue, acceleration);
		}
		tableType_t tableType2 = ((sizeof(void*) < 8 && (nuint)source > (nuint)65535u) ? tableType_t.byPtr : tableType_t.byU32);
		return LZ4_compress_generic(cctx, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType2, dict_directive.noDict, dictIssue_directive.noDictIssue, acceleration);
	}

	public unsafe static int LZ4_compress_fast(byte* source, byte* dest, int inputSize, int maxOutputSize, int acceleration)
	{
		LZ4_stream_t lZ4_stream_t = default(LZ4_stream_t);
		return LZ4_compress_fast_extState(&lZ4_stream_t, source, dest, inputSize, maxOutputSize, acceleration);
	}

	public unsafe static int LZ4_compress_default(byte* src, byte* dst, int srcSize, int maxOutputSize)
	{
		return LZ4_compress_fast(src, dst, srcSize, maxOutputSize, 1);
	}

	public unsafe static int LZ4_compress_fast_continue(LZ4_stream_t* LZ4_stream, byte* source, byte* dest, int inputSize, int maxOutputSize, int acceleration)
	{
		byte* ptr = LZ4_stream->dictionary + LZ4_stream->dictSize;
		if (LZ4_stream->dirty)
		{
			return 0;
		}
		LZ4_renormDictT(LZ4_stream, inputSize);
		if (acceleration < 1)
		{
			acceleration = 1;
		}
		if (LZ4_stream->dictSize - 1 < 3 && ptr != source)
		{
			LZ4_stream->dictSize = 0u;
			LZ4_stream->dictionary = source;
			ptr = source;
		}
		byte* ptr2 = source + inputSize;
		if (ptr2 > LZ4_stream->dictionary && ptr2 < ptr)
		{
			LZ4_stream->dictSize = (uint)(ptr - ptr2);
			if (LZ4_stream->dictSize > 65536)
			{
				LZ4_stream->dictSize = 65536u;
			}
			if (LZ4_stream->dictSize < 4)
			{
				LZ4_stream->dictSize = 0u;
			}
			LZ4_stream->dictionary = ptr - LZ4_stream->dictSize;
		}
		if (ptr == source)
		{
			if (LZ4_stream->dictSize < 65536 && LZ4_stream->dictSize < LZ4_stream->currentOffset)
			{
				return LZ4_compress_generic(LZ4_stream, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType_t.byU32, dict_directive.withPrefix64k, dictIssue_directive.dictSmall, acceleration);
			}
			return LZ4_compress_generic(LZ4_stream, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType_t.byU32, dict_directive.withPrefix64k, dictIssue_directive.noDictIssue, acceleration);
		}
		int result;
		if (LZ4_stream->dictCtx == null)
		{
			result = ((LZ4_stream->dictSize >= 65536 || LZ4_stream->dictSize >= LZ4_stream->currentOffset) ? LZ4_compress_generic(LZ4_stream, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType_t.byU32, dict_directive.usingExtDict, dictIssue_directive.noDictIssue, acceleration) : LZ4_compress_generic(LZ4_stream, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType_t.byU32, dict_directive.usingExtDict, dictIssue_directive.dictSmall, acceleration));
		}
		else if (inputSize > 4096)
		{
			Mem.Copy((byte*)LZ4_stream, (byte*)LZ4_stream->dictCtx, sizeof(LZ4_stream_t));
			result = LZ4_compress_generic(LZ4_stream, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType_t.byU32, dict_directive.usingExtDict, dictIssue_directive.noDictIssue, acceleration);
		}
		else
		{
			result = LZ4_compress_generic(LZ4_stream, source, dest, inputSize, null, maxOutputSize, limitedOutput_directive.limitedOutput, tableType_t.byU32, dict_directive.usingDictCtx, dictIssue_directive.noDictIssue, acceleration);
		}
		LZ4_stream->dictionary = source;
		LZ4_stream->dictSize = (uint)inputSize;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint LZ4HC_countPattern(byte* ip, byte* iEnd, uint pattern32)
	{
		byte* ptr = ip;
		while (ip < iEnd - 3)
		{
			uint num = Mem32.PeekW(ip) ^ pattern32;
			if (num == 0)
			{
				ip += 4;
				continue;
			}
			ip += LZ4_NbCommonBytes(num);
			return (uint)(ip - ptr);
		}
		uint num2 = pattern32;
		while (ip < iEnd && *ip == (byte)num2)
		{
			ip++;
			num2 >>= 8;
		}
		return (uint)(ip - ptr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4HC_InsertAndGetWiderMatch(LZ4_streamHC_t* hc4, byte* ip, byte* iLowLimit, byte* iHighLimit, int longest, byte** matchpos, byte** startpos, int maxNbAttempts, bool patternAnalysis, bool chainSwap, dictCtx_directive dict, HCfavor_e favorDecSpeed)
	{
		ushort* table = hc4->chainTable;
		uint* num = hc4->hashTable;
		LZ4_streamHC_t* dictCtx = hc4->dictCtx;
		byte* @base = hc4->@base;
		uint dictLimit = hc4->dictLimit;
		byte* ptr = @base + dictLimit;
		uint num2 = (uint)(ip - @base);
		uint num3 = ((hc4->lowLimit + 65536 > num2) ? hc4->lowLimit : (num2 - 65535));
		byte* dictBase = hc4->dictBase;
		int num4 = (int)(ip - iLowLimit);
		int num5 = maxNbAttempts;
		uint num6 = 0u;
		uint num7 = Mem.Peek4(ip);
		repeat_state_e repeat_state_e = repeat_state_e.rep_untested;
		uint num8 = 0u;
		LL.LZ4HC_Insert(hc4, ip);
		uint num9 = num[LL.LZ4HC_hashPtr(ip)];
		while (num9 >= num3 && num5 != 0)
		{
			int num10 = 0;
			num5--;
			if (favorDecSpeed == HCfavor_e.favorCompressionRatio || num2 - num9 >= 8)
			{
				if (num9 >= dictLimit)
				{
					byte* ptr2 = @base + num9;
					if (Mem.Peek2(iLowLimit + longest - 1) == Mem.Peek2(ptr2 - num4 + longest - 1) && Mem.Peek4(ptr2) == num7)
					{
						int num11 = ((num4 != 0) ? LL.LZ4HC_countBack(ip, ptr2, iLowLimit, ptr) : 0);
						num10 = (int)(4 + LZ4_count(ip + 4, ptr2 + 4, iHighLimit));
						num10 -= num11;
						if (num10 > longest)
						{
							longest = num10;
							*matchpos = ptr2 + num11;
							*startpos = ip + num11;
						}
					}
				}
				else
				{
					byte* ptr3 = dictBase + num9;
					if (Mem.Peek4(ptr3) == num7)
					{
						byte* mMin = dictBase + hc4->lowLimit;
						int num12 = 0;
						byte* ptr4 = ip + (dictLimit - num9);
						if (ptr4 > iHighLimit)
						{
							ptr4 = iHighLimit;
						}
						num10 = (int)(LZ4_count(ip + 4, ptr3 + 4, ptr4) + 4);
						if (ip + num10 == ptr4 && ptr4 < iHighLimit)
						{
							num10 += (int)LZ4_count(ip + num10, ptr, iHighLimit);
						}
						num12 = ((num4 != 0) ? LL.LZ4HC_countBack(ip, ptr3, iLowLimit, mMin) : 0);
						num10 -= num12;
						if (num10 > longest)
						{
							longest = num10;
							*matchpos = @base + num9 + num12;
							*startpos = ip + num12;
						}
					}
				}
			}
			if (chainSwap && num10 == longest && (uint)((int)num9 + longest) <= num2)
			{
				int num13 = 4;
				uint num14 = 1u;
				int num15 = longest - 4 + 1;
				int num16 = 1;
				int num17 = 1 << num13;
				for (int i = 0; i < num15; i += num16)
				{
					uint num18 = LL.DELTANEXTU16(table, num9 + (uint)i);
					num16 = num17++ >> num13;
					if (num18 > num14)
					{
						num14 = num18;
						num6 = (uint)i;
						num17 = 1 << num13;
					}
				}
				if (num14 > 1)
				{
					if (num14 > num9)
					{
						break;
					}
					num9 -= num14;
					continue;
				}
			}
			uint num19 = LL.DELTANEXTU16(table, num9);
			if (patternAnalysis && num19 == 1 && num6 == 0)
			{
				uint num20 = num9 - 1;
				if (repeat_state_e == repeat_state_e.rep_untested)
				{
					if ((num7 & 0xFFFF) == num7 >> 16 && (num7 & 0xFF) == num7 >> 24)
					{
						repeat_state_e = repeat_state_e.rep_confirmed;
						num8 = LZ4HC_countPattern(ip + 4, iHighLimit, num7) + 4;
					}
					else
					{
						repeat_state_e = repeat_state_e.rep_not;
					}
				}
				if (repeat_state_e == repeat_state_e.rep_confirmed && num20 >= num3 && LL.LZ4HC_protectDictEnd(dictLimit, num20))
				{
					bool flag = num20 < dictLimit;
					byte* ptr5 = (flag ? dictBase : @base) + num20;
					if (Mem.Peek4(ptr5) == num7)
					{
						byte* ptr6 = dictBase + hc4->lowLimit;
						byte* ptr7 = (flag ? (dictBase + dictLimit) : iHighLimit);
						uint num21 = LZ4HC_countPattern(ptr5 + 4, ptr7, num7) + 4;
						if (flag && ptr5 + num21 == ptr7)
						{
							uint pattern = LL.LZ4HC_rotatePattern(num21, num7);
							num21 += LZ4HC_countPattern(ptr, iHighLimit, pattern);
						}
						byte* iLow = (flag ? ptr6 : ptr);
						uint num22 = LL.LZ4HC_reverseCountPattern(ptr5, iLow, num7);
						if (!flag && ptr5 - num22 == ptr && hc4->lowLimit < dictLimit)
						{
							uint pattern2 = LL.LZ4HC_rotatePattern(0 - num22, num7);
							num22 += LL.LZ4HC_reverseCountPattern(dictBase + dictLimit, ptr6, pattern2);
						}
						num22 = num20 - LL.MAX(num20 - num22, num3);
						uint num23 = num22 + num21;
						if (num23 >= num8 && num21 <= num8)
						{
							uint num24 = num20 + num21 - num8;
							num9 = ((!LL.LZ4HC_protectDictEnd(dictLimit, num24)) ? dictLimit : num24);
							continue;
						}
						uint num25 = num20 - num22;
						if (!LL.LZ4HC_protectDictEnd(dictLimit, num25))
						{
							num9 = dictLimit;
							continue;
						}
						num9 = num25;
						if (num4 != 0)
						{
							continue;
						}
						uint num26 = LL.MIN(num23, num8);
						if ((uint)longest < num26)
						{
							if ((uint)((int)(ip - @base) - (int)num9) > 65535u)
							{
								break;
							}
							longest = (int)num26;
							*matchpos = @base + num9;
							*startpos = ip;
						}
						uint num27 = LL.DELTANEXTU16(table, num9);
						if (num27 > num9)
						{
							break;
						}
						num9 -= num27;
						continue;
					}
				}
			}
			num9 -= LL.DELTANEXTU16(table, num9 + num6);
		}
		if (dict == dictCtx_directive.usingDictCtxHc && num5 != 0 && num2 - num3 < 65535)
		{
			uint num28 = (uint)(dictCtx->end - dictCtx->@base);
			uint num29 = dictCtx->hashTable[LL.LZ4HC_hashPtr(ip)];
			num9 = num29 + num3 - num28;
			while (num2 - num9 <= 65535 && num5-- != 0)
			{
				byte* ptr8 = dictCtx->@base + num29;
				if (Mem.Peek4(ptr8) == num7)
				{
					int num30 = 0;
					byte* ptr9 = ip + (num28 - num29);
					if (ptr9 > iHighLimit)
					{
						ptr9 = iHighLimit;
					}
					int num31 = (int)(LZ4_count(ip + 4, ptr8 + 4, ptr9) + 4);
					num30 = ((num4 != 0) ? LL.LZ4HC_countBack(ip, ptr8, iLowLimit, dictCtx->@base + dictCtx->dictLimit) : 0);
					num31 -= num30;
					if (num31 > longest)
					{
						longest = num31;
						*matchpos = @base + num9 + num30;
						*startpos = ip + num30;
					}
				}
				uint num32 = LL.DELTANEXTU16(dictCtx->chainTable, num29);
				num29 -= num32;
				num9 -= num32;
			}
		}
		return longest;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4HC_InsertAndFindBestMatch(LZ4_streamHC_t* hc4, byte* ip, byte* iLimit, byte** matchpos, int maxNbAttempts, bool patternAnalysis, dictCtx_directive dict)
	{
		byte* ptr = ip;
		return LZ4HC_InsertAndGetWiderMatch(hc4, ip, ip, iLimit, 3, matchpos, &ptr, maxNbAttempts, patternAnalysis, chainSwap: false, dict, HCfavor_e.favorCompressionRatio);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static LZ4HC_match_t LZ4HC_FindLongerMatch(LZ4_streamHC_t* ctx, byte* ip, byte* iHighLimit, int minLen, int nbSearches, dictCtx_directive dict, HCfavor_e favorDecSpeed)
	{
		LZ4HC_match_t result = default(LZ4HC_match_t);
		result.len = 0;
		result.off = 0;
		byte* ptr = null;
		int num = LZ4HC_InsertAndGetWiderMatch(ctx, ip, ip, iHighLimit, minLen, &ptr, &ip, nbSearches, patternAnalysis: true, chainSwap: true, dict, favorDecSpeed);
		if (num <= minLen)
		{
			return result;
		}
		if (favorDecSpeed != 0 && num > 18 && num <= 36)
		{
			num = 18;
		}
		result.len = num;
		result.off = (int)(ip - ptr);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4HC_encodeSequence(byte** ip, byte** op, byte** anchor, int matchLength, byte* match, limitedOutput_directive limit, byte* oend)
	{
		byte* ptr = (*op)++;
		uint num = (uint)(*ip - *anchor);
		if (limit != 0 && *op + num / 255 + num + 8 > oend)
		{
			return 1;
		}
		if (num >= 15)
		{
			uint num2 = num - 15;
			*ptr = 240;
			while (num2 >= 255)
			{
				*((*op)++) = byte.MaxValue;
				num2 -= 255;
			}
			*((*op)++) = (byte)num2;
		}
		else
		{
			*ptr = (byte)(num << 4);
		}
		Mem32.WildCopy8(*op, *anchor, *op + num);
		*op += num;
		Mem.Poke2(*op, (ushort)(*ip - match));
		*op += 2;
		num = (uint)(matchLength - 4);
		if (limit != 0 && *op + num / 255 + 6 > oend)
		{
			return 1;
		}
		if (num >= 15)
		{
			*ptr += 15;
			for (num -= 15; num >= 510; num -= 510)
			{
				*((*op)++) = byte.MaxValue;
				*((*op)++) = byte.MaxValue;
			}
			if (num >= 255)
			{
				num -= 255;
				*((*op)++) = byte.MaxValue;
			}
			*((*op)++) = (byte)num;
		}
		else
		{
			*ptr += (byte)num;
		}
		*ip += matchLength;
		*anchor = *ip;
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4HC_compress_hashChain(LZ4_streamHC_t* ctx, byte* source, byte* dest, int* srcSizePtr, int maxOutputSize, int maxNbAttempts, limitedOutput_directive limit, dictCtx_directive dict)
	{
		int num = *srcSizePtr;
		bool patternAnalysis = maxNbAttempts > 128;
		byte* ptr = source;
		byte* ptr2 = ptr;
		byte* ptr3 = ptr + num;
		byte* ptr4 = ptr3 - 12;
		byte* ptr5 = ptr3 - 5;
		byte* ptr6 = dest;
		byte* ptr7 = dest;
		byte* ptr8 = ptr7 + maxOutputSize;
		byte* ptr9 = null;
		byte* ptr10 = null;
		byte* ptr11 = null;
		byte* ptr12 = null;
		byte* ptr13 = null;
		*srcSizePtr = 0;
		if (limit == limitedOutput_directive.fillOutput)
		{
			ptr8 -= 5;
		}
		if (num >= 13)
		{
			while (ptr <= ptr4)
			{
				int num2 = LZ4HC_InsertAndFindBestMatch(ctx, ptr, ptr5, &ptr9, maxNbAttempts, patternAnalysis, dict);
				if (num2 < 4)
				{
					ptr++;
					continue;
				}
				byte* ptr14 = ptr;
				byte* ptr15 = ptr9;
				int num3 = num2;
				while (true)
				{
					int num4 = ((ptr + num2 > ptr4) ? num2 : LZ4HC_InsertAndGetWiderMatch(ctx, ptr + num2 - 2, ptr, ptr5, num2, &ptr11, &ptr10, maxNbAttempts, patternAnalysis, chainSwap: false, dict, HCfavor_e.favorCompressionRatio));
					int num7;
					if (num4 == num2)
					{
						ptr6 = ptr7;
						if (LZ4HC_encodeSequence(&ptr, &ptr7, &ptr2, num2, ptr9, limit, ptr8) == 0)
						{
							break;
						}
					}
					else
					{
						if (ptr14 < ptr && ptr10 < ptr + num3)
						{
							ptr = ptr14;
							ptr9 = ptr15;
							num2 = num3;
						}
						if (ptr10 - ptr < 3)
						{
							num2 = num4;
							ptr = ptr10;
							ptr9 = ptr11;
							continue;
						}
						while (true)
						{
							if (ptr10 - ptr < 18)
							{
								int num5 = num2;
								if (num5 > 18)
								{
									num5 = 18;
								}
								if (ptr + num5 > ptr10 + num4 - 4)
								{
									num5 = (int)(ptr10 - ptr) + num4 - 4;
								}
								int num6 = num5 - (int)(ptr10 - ptr);
								if (num6 > 0)
								{
									ptr10 += num6;
									ptr11 += num6;
									num4 -= num6;
								}
							}
							num7 = ((ptr10 + num4 > ptr4) ? num4 : LZ4HC_InsertAndGetWiderMatch(ctx, ptr10 + num4 - 3, ptr10, ptr5, num4, &ptr13, &ptr12, maxNbAttempts, patternAnalysis, chainSwap: false, dict, HCfavor_e.favorCompressionRatio));
							if (num7 == num4)
							{
								break;
							}
							if (ptr12 < ptr + num2 + 3)
							{
								if (ptr12 < ptr + num2)
								{
									ptr10 = ptr12;
									ptr11 = ptr13;
									num4 = num7;
									continue;
								}
								goto IL_0217;
							}
							if (ptr10 < ptr + num2)
							{
								if (ptr10 - ptr < 18)
								{
									if (num2 > 18)
									{
										num2 = 18;
									}
									if (ptr + num2 > ptr10 + num4 - 4)
									{
										num2 = (int)(ptr10 - ptr) + num4 - 4;
									}
									int num8 = num2 - (int)(ptr10 - ptr);
									if (num8 > 0)
									{
										ptr10 += num8;
										ptr11 += num8;
										num4 -= num8;
									}
								}
								else
								{
									num2 = (int)(ptr10 - ptr);
								}
							}
							ptr6 = ptr7;
							if (LZ4HC_encodeSequence(&ptr, &ptr7, &ptr2, num2, ptr9, limit, ptr8) == 0)
							{
								ptr = ptr10;
								ptr9 = ptr11;
								num2 = num4;
								ptr10 = ptr12;
								ptr11 = ptr13;
								num4 = num7;
								continue;
							}
							goto IL_043b;
						}
						if (ptr10 < ptr + num2)
						{
							num2 = (int)(ptr10 - ptr);
						}
						ptr6 = ptr7;
						if (LZ4HC_encodeSequence(&ptr, &ptr7, &ptr2, num2, ptr9, limit, ptr8) == 0)
						{
							ptr = ptr10;
							ptr6 = ptr7;
							if (LZ4HC_encodeSequence(&ptr, &ptr7, &ptr2, num4, ptr11, limit, ptr8) == 0)
							{
								break;
							}
						}
					}
					goto IL_043b;
					IL_043b:
					if (limit != limitedOutput_directive.fillOutput)
					{
						return 0;
					}
					goto IL_0440;
					IL_0217:
					if (ptr10 < ptr + num2)
					{
						int num9 = (int)(ptr + num2 - ptr10);
						ptr10 += num9;
						ptr11 += num9;
						num4 -= num9;
						if (num4 < 4)
						{
							ptr10 = ptr12;
							ptr11 = ptr13;
							num4 = num7;
						}
					}
					ptr6 = ptr7;
					if (LZ4HC_encodeSequence(&ptr, &ptr7, &ptr2, num2, ptr9, limit, ptr8) == 0)
					{
						ptr = ptr12;
						ptr9 = ptr13;
						num2 = num7;
						ptr14 = ptr10;
						ptr15 = ptr11;
						num3 = num4;
						continue;
					}
					goto IL_043b;
				}
				continue;
				IL_0440:
				ptr7 = ptr6;
				break;
			}
		}
		uint num10 = (uint)(ptr3 - ptr2);
		uint num11 = (num10 + 255 - 15) / 255;
		uint num12 = 1 + num11 + num10;
		if (limit == limitedOutput_directive.fillOutput)
		{
			ptr8 += 5;
		}
		if (limit != 0 && ptr7 + num12 > ptr8)
		{
			if (limit == limitedOutput_directive.limitedOutput)
			{
				return 0;
			}
			num10 = (uint)((int)(ptr8 - ptr7) - 1);
			num11 = (num10 + 255 - 15) / 255;
			num10 -= num11;
		}
		ptr = ptr2 + num10;
		if (num10 >= 15)
		{
			uint num13 = num10 - 15;
			*(ptr7++) = 240;
			while (num13 >= 255)
			{
				*(ptr7++) = byte.MaxValue;
				num13 -= 255;
			}
			*(ptr7++) = (byte)num13;
		}
		else
		{
			*(ptr7++) = (byte)(num10 << 4);
		}
		Mem.Copy(ptr7, ptr2, (int)num10);
		ptr7 += num10;
		*srcSizePtr = (int)(ptr - source);
		return (int)(ptr7 - dest);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4HC_compress_optimal(LZ4_streamHC_t* ctx, byte* source, byte* dst, int* srcSizePtr, int dstCapacity, int nbSearches, uint sufficient_len, limitedOutput_directive limit, bool fullUpdate, dictCtx_directive dict, HCfavor_e favorDecSpeed)
	{
		LZ4HC_optimal_t* ptr = stackalloc LZ4HC_optimal_t[4099];
		byte* ptr2 = source;
		byte* ptr3 = ptr2;
		byte* ptr4 = ptr2 + *srcSizePtr;
		byte* ptr5 = ptr4 - 12;
		byte* iHighLimit = ptr4 - 5;
		byte* ptr6 = dst;
		byte* ptr7 = dst;
		byte* ptr8 = ptr6 + dstCapacity;
		*srcSizePtr = 0;
		if (limit == limitedOutput_directive.fillOutput)
		{
			ptr8 -= 5;
		}
		if (sufficient_len >= 4096)
		{
			sufficient_len = 4095u;
		}
		while (true)
		{
			if (ptr2 > ptr5)
			{
				goto IL_06aa;
			}
			int num = (int)(ptr2 - ptr3);
			int num2 = 0;
			LZ4HC_match_t lZ4HC_match_t = LZ4HC_FindLongerMatch(ctx, ptr2, iHighLimit, 3, nbSearches, dict, favorDecSpeed);
			if (lZ4HC_match_t.len == 0)
			{
				ptr2++;
				continue;
			}
			if ((uint)lZ4HC_match_t.len > sufficient_len)
			{
				int len = lZ4HC_match_t.len;
				byte* match = ptr2 - lZ4HC_match_t.off;
				ptr7 = ptr6;
				if (LZ4HC_encodeSequence(&ptr2, &ptr6, &ptr3, len, match, limit, ptr8) == 0)
				{
					continue;
				}
				goto IL_0796;
			}
			for (int i = 0; i < 4; i++)
			{
				int price = LL.LZ4HC_literalsPrice(num + i);
				ptr[i].mlen = 1;
				ptr[i].off = 0;
				ptr[i].litlen = num + i;
				ptr[i].price = price;
			}
			int j = 4;
			int len2 = lZ4HC_match_t.len;
			int off = lZ4HC_match_t.off;
			for (; j <= len2; j++)
			{
				int price2 = LL.LZ4HC_sequencePrice(num, j);
				ptr[j].mlen = j;
				ptr[j].off = off;
				ptr[j].litlen = num;
				ptr[j].price = price2;
			}
			num2 = lZ4HC_match_t.len;
			for (int k = 1; k <= 3; k++)
			{
				ptr[num2 + k].mlen = 1;
				ptr[num2 + k].off = 0;
				ptr[num2 + k].litlen = k;
				ptr[num2 + k].price = ptr[num2].price + LL.LZ4HC_literalsPrice(k);
			}
			int num3 = 1;
			int num4;
			int off2;
			while (true)
			{
				if (num3 < num2)
				{
					byte* ptr9 = ptr2 + num3;
					if (ptr9 <= ptr5)
					{
						if (fullUpdate)
						{
							if (ptr[num3 + 1].price <= ptr[num3].price && ptr[num3 + 4].price < ptr[num3].price + 3)
							{
								goto IL_0591;
							}
						}
						else if (ptr[num3 + 1].price <= ptr[num3].price)
						{
							goto IL_0591;
						}
						LZ4HC_match_t lZ4HC_match_t2 = ((!fullUpdate) ? LZ4HC_FindLongerMatch(ctx, ptr9, iHighLimit, num2 - num3, nbSearches, dict, favorDecSpeed) : LZ4HC_FindLongerMatch(ctx, ptr9, iHighLimit, 3, nbSearches, dict, favorDecSpeed));
						if (lZ4HC_match_t2.len != 0)
						{
							if ((uint)lZ4HC_match_t2.len > sufficient_len || lZ4HC_match_t2.len + num3 >= 4096)
							{
								num4 = lZ4HC_match_t2.len;
								off2 = lZ4HC_match_t2.off;
								num2 = num3 + 1;
								break;
							}
							int litlen = ptr[num3].litlen;
							for (int l = 1; l < 4; l++)
							{
								int num5 = ptr[num3].price - LL.LZ4HC_literalsPrice(litlen) + LL.LZ4HC_literalsPrice(litlen + l);
								int num6 = num3 + l;
								if (num5 < ptr[num6].price)
								{
									ptr[num6].mlen = 1;
									ptr[num6].off = 0;
									ptr[num6].litlen = litlen + l;
									ptr[num6].price = num5;
								}
							}
							int len3 = lZ4HC_match_t2.len;
							for (int m = 4; m <= len3; m++)
							{
								int num7 = num3 + m;
								int off3 = lZ4HC_match_t2.off;
								int num8;
								int num9;
								if (ptr[num3].mlen == 1)
								{
									num8 = ptr[num3].litlen;
									num9 = ((num3 > num8) ? ptr[num3 - num8].price : 0) + LL.LZ4HC_sequencePrice(num8, m);
								}
								else
								{
									num8 = 0;
									num9 = ptr[num3].price + LL.LZ4HC_sequencePrice(0, m);
								}
								if (num7 > num2 + 3 || num9 <= (int)(ptr[num7].price - favorDecSpeed))
								{
									if (m == len3 && num2 < num7)
									{
										num2 = num7;
									}
									ptr[num7].mlen = m;
									ptr[num7].off = off3;
									ptr[num7].litlen = num8;
									ptr[num7].price = num9;
								}
							}
							for (int n = 1; n <= 3; n++)
							{
								ptr[num2 + n].mlen = 1;
								ptr[num2 + n].off = 0;
								ptr[num2 + n].litlen = n;
								ptr[num2 + n].price = ptr[num2].price + LL.LZ4HC_literalsPrice(n);
							}
						}
						goto IL_0591;
					}
				}
				num4 = ptr[num2].mlen;
				off2 = ptr[num2].off;
				num3 = num2 - num4;
				break;
				IL_0591:
				num3++;
			}
			int num10 = num3;
			int mlen = num4;
			int off4 = off2;
			while (true)
			{
				int mlen2 = ptr[num10].mlen;
				int off5 = ptr[num10].off;
				ptr[num10].mlen = mlen;
				ptr[num10].off = off4;
				mlen = mlen2;
				off4 = off5;
				if (mlen2 > num10)
				{
					break;
				}
				num10 -= mlen2;
			}
			int num11 = 0;
			while (num11 < num2)
			{
				int mlen3 = ptr[num11].mlen;
				int off6 = ptr[num11].off;
				if (mlen3 == 1)
				{
					ptr2++;
					num11++;
					continue;
				}
				num11 += mlen3;
				ptr7 = ptr6;
				if (LZ4HC_encodeSequence(&ptr2, &ptr6, &ptr3, mlen3, ptr2 - off6, limit, ptr8) == 0)
				{
					continue;
				}
				goto IL_0796;
			}
			continue;
			IL_0796:
			if (limit != limitedOutput_directive.fillOutput)
			{
				break;
			}
			ptr6 = ptr7;
			goto IL_06aa;
			IL_06aa:
			uint num12 = (uint)(ptr4 - ptr3);
			uint num13 = (num12 + 255 - 15) / 255;
			uint num14 = 1 + num13 + num12;
			if (limit == limitedOutput_directive.fillOutput)
			{
				ptr8 += 5;
			}
			if (limit != 0 && ptr6 + num14 > ptr8)
			{
				if (limit == limitedOutput_directive.limitedOutput)
				{
					return 0;
				}
				num12 = (uint)((int)(ptr8 - ptr6) - 1);
				num13 = (num12 + 255 - 15) / 255;
				num12 -= num13;
			}
			ptr2 = ptr3 + num12;
			if (num12 >= 15)
			{
				uint num15 = num12 - 15;
				*(ptr6++) = 240;
				while (num15 >= 255)
				{
					*(ptr6++) = byte.MaxValue;
					num15 -= 255;
				}
				*(ptr6++) = (byte)num15;
			}
			else
			{
				*(ptr6++) = (byte)(num12 << 4);
			}
			Mem.Copy(ptr6, ptr3, (int)num12);
			ptr6 += num12;
			*srcSizePtr = (int)(ptr2 - source);
			return (int)(ptr6 - dst);
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4HC_compress_generic_internal(LZ4_streamHC_t* ctx, byte* src, byte* dst, int* srcSizePtr, int dstCapacity, int cLevel, limitedOutput_directive limit, dictCtx_directive dict)
	{
		if (limit == limitedOutput_directive.fillOutput && dstCapacity < 1)
		{
			return 0;
		}
		if ((uint)(*srcSizePtr) > 2113929216u)
		{
			return 0;
		}
		ctx->end += *srcSizePtr;
		if (cLevel < 1)
		{
			cLevel = 9;
		}
		cLevel = LL.MIN(12, cLevel);
		cParams_t cParams_t = clTable[cLevel];
		HCfavor_e favorDecSpeed = (ctx->favorDecSpeed ? HCfavor_e.favorDecompressionSpeed : HCfavor_e.favorCompressionRatio);
		int num = ((cParams_t.strat != 0) ? LZ4HC_compress_optimal(ctx, src, dst, srcSizePtr, dstCapacity, (int)cParams_t.nbSearches, cParams_t.targetLength, limit, cLevel == 12, dict, favorDecSpeed) : LZ4HC_compress_hashChain(ctx, src, dst, srcSizePtr, dstCapacity, (int)cParams_t.nbSearches, limit, dict));
		if (num <= 0)
		{
			ctx->dirty = true;
		}
		return num;
	}

	public unsafe static int LZ4HC_compress_generic_noDictCtx(LZ4_streamHC_t* ctx, byte* src, byte* dst, int* srcSizePtr, int dstCapacity, int cLevel, limitedOutput_directive limit)
	{
		return LZ4HC_compress_generic_internal(ctx, src, dst, srcSizePtr, dstCapacity, cLevel, limit, dictCtx_directive.noDictCtx);
	}

	public unsafe static int LZ4HC_compress_generic_dictCtx(LZ4_streamHC_t* ctx, byte* src, byte* dst, int* srcSizePtr, int dstCapacity, int cLevel, limitedOutput_directive limit)
	{
		uint num = (uint)(int)(ctx->end - ctx->@base) - ctx->lowLimit;
		if (num >= 65536)
		{
			ctx->dictCtx = null;
			return LZ4HC_compress_generic_noDictCtx(ctx, src, dst, srcSizePtr, dstCapacity, cLevel, limit);
		}
		if (num == 0 && *srcSizePtr > 4096)
		{
			Mem.Copy((byte*)ctx, (byte*)ctx->dictCtx, sizeof(LZ4_streamHC_t));
			LL.LZ4HC_setExternalDict(ctx, src);
			ctx->compressionLevel = (short)cLevel;
			return LZ4HC_compress_generic_noDictCtx(ctx, src, dst, srcSizePtr, dstCapacity, cLevel, limit);
		}
		return LZ4HC_compress_generic_internal(ctx, src, dst, srcSizePtr, dstCapacity, cLevel, limit, dictCtx_directive.usingDictCtxHc);
	}

	public unsafe static int LZ4HC_compress_generic(LZ4_streamHC_t* ctx, byte* src, byte* dst, int* srcSizePtr, int dstCapacity, int cLevel, limitedOutput_directive limit)
	{
		if (ctx->dictCtx == null)
		{
			return LZ4HC_compress_generic_noDictCtx(ctx, src, dst, srcSizePtr, dstCapacity, cLevel, limit);
		}
		return LZ4HC_compress_generic_dictCtx(ctx, src, dst, srcSizePtr, dstCapacity, cLevel, limit);
	}

	public unsafe static int LZ4_compressHC_continue_generic(LZ4_streamHC_t* LZ4_streamHCPtr, byte* src, byte* dst, int* srcSizePtr, int dstCapacity, limitedOutput_directive limit)
	{
		if (LZ4_streamHCPtr->@base == null)
		{
			LL.LZ4HC_init_internal(LZ4_streamHCPtr, src);
		}
		if ((uint)(LZ4_streamHCPtr->end - LZ4_streamHCPtr->@base) > 2147483648u)
		{
			uint num = (uint)(int)(LZ4_streamHCPtr->end - LZ4_streamHCPtr->@base) - LZ4_streamHCPtr->dictLimit;
			if (num > 65536)
			{
				num = 65536u;
			}
			LL.LZ4_loadDictHC(LZ4_streamHCPtr, LZ4_streamHCPtr->end - num, (int)num);
		}
		if (src != LZ4_streamHCPtr->end)
		{
			LL.LZ4HC_setExternalDict(LZ4_streamHCPtr, src);
		}
		byte* ptr = src + *srcSizePtr;
		byte* ptr2 = LZ4_streamHCPtr->dictBase + LZ4_streamHCPtr->lowLimit;
		byte* ptr3 = LZ4_streamHCPtr->dictBase + LZ4_streamHCPtr->dictLimit;
		if (ptr > ptr2 && src < ptr3)
		{
			if (ptr > ptr3)
			{
				ptr = ptr3;
			}
			LZ4_streamHCPtr->lowLimit = (uint)(ptr - LZ4_streamHCPtr->dictBase);
			if (LZ4_streamHCPtr->dictLimit - LZ4_streamHCPtr->lowLimit < 4)
			{
				LZ4_streamHCPtr->lowLimit = LZ4_streamHCPtr->dictLimit;
			}
		}
		return LZ4HC_compress_generic(LZ4_streamHCPtr, src, dst, srcSizePtr, dstCapacity, LZ4_streamHCPtr->compressionLevel, limit);
	}

	public unsafe static int LZ4_compress_HC_continue(LZ4_streamHC_t* LZ4_streamHCPtr, byte* src, byte* dst, int srcSize, int dstCapacity)
	{
		if (dstCapacity < LL.LZ4_compressBound(srcSize))
		{
			return LZ4_compressHC_continue_generic(LZ4_streamHCPtr, src, dst, &srcSize, dstCapacity, limitedOutput_directive.limitedOutput);
		}
		return LZ4_compressHC_continue_generic(LZ4_streamHCPtr, src, dst, &srcSize, dstCapacity, limitedOutput_directive.notLimited);
	}

	public unsafe static int LZ4_compress_HC_continue_destSize(LZ4_streamHC_t* LZ4_streamHCPtr, byte* src, byte* dst, int* srcSizePtr, int targetDestSize)
	{
		return LZ4_compressHC_continue_generic(LZ4_streamHCPtr, src, dst, srcSizePtr, targetDestSize, limitedOutput_directive.fillOutput);
	}

	public unsafe static int LZ4_compress_HC_destSize(LZ4_streamHC_t* state, byte* source, byte* dest, int* sourceSizePtr, int targetDestSize, int cLevel)
	{
		LZ4_streamHC_t* ptr = LL.LZ4_initStreamHC(state);
		if (ptr == null)
		{
			return 0;
		}
		LL.LZ4HC_init_internal(ptr, source);
		LL.LZ4_setCompressionLevel(ptr, cLevel);
		return LZ4HC_compress_generic(ptr, source, dest, sourceSizePtr, targetDestSize, cLevel, limitedOutput_directive.fillOutput);
	}

	public unsafe static int LZ4_compress_HC_extStateHC_fastReset(LZ4_streamHC_t* state, byte* src, byte* dst, int srcSize, int dstCapacity, int compressionLevel)
	{
		if (((uint)state & (sizeof(void*) - 1)) != 0L)
		{
			return 0;
		}
		LL.LZ4_resetStreamHC_fast(state, compressionLevel);
		LL.LZ4HC_init_internal(state, src);
		if (dstCapacity < LL.LZ4_compressBound(srcSize))
		{
			return LZ4HC_compress_generic(state, src, dst, &srcSize, dstCapacity, compressionLevel, limitedOutput_directive.limitedOutput);
		}
		return LZ4HC_compress_generic(state, src, dst, &srcSize, dstCapacity, compressionLevel, limitedOutput_directive.notLimited);
	}

	public unsafe static int LZ4_compress_HC_extStateHC(LZ4_streamHC_t* state, byte* src, byte* dst, int srcSize, int dstCapacity, int compressionLevel)
	{
		if (LL.LZ4_initStreamHC(state) == null)
		{
			return 0;
		}
		return LZ4_compress_HC_extStateHC_fastReset(state, src, dst, srcSize, dstCapacity, compressionLevel);
	}

	public unsafe static int LZ4_compress_HC(byte* src, byte* dst, int srcSize, int dstCapacity, int compressionLevel)
	{
		PinnedMemory.Alloc(out var memory, sizeof(LZ4_streamHC_t), zero: false);
		try
		{
			return LZ4_compress_HC_extStateHC(memory.Reference<LZ4_streamHC_t>(), src, dst, srcSize, dstCapacity, compressionLevel);
		}
		finally
		{
			memory.Free();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static uint LZ4_NbCommonBytes(uint val)
	{
		return DeBruijnBytePos[(val & (0 - val)) * 125613361 >> 27];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static uint LZ4_count(byte* pIn, byte* pMatch, byte* pInLimit)
	{
		byte* ptr = pIn;
		if (pIn < pInLimit - 3)
		{
			uint num = Mem32.PeekW(pMatch) ^ Mem32.PeekW(pIn);
			if (num != 0)
			{
				return LZ4_NbCommonBytes(num);
			}
			pIn += 4;
			pMatch += 4;
		}
		while (pIn < pInLimit - 3)
		{
			uint num2 = Mem32.PeekW(pMatch) ^ Mem32.PeekW(pIn);
			if (num2 != 0)
			{
				return (uint)(pIn + LZ4_NbCommonBytes(num2) - ptr);
			}
			pIn += 4;
			pMatch += 4;
		}
		if (pIn < pInLimit - 1 && Mem.Peek2(pMatch) == Mem.Peek2(pIn))
		{
			pIn += 2;
			pMatch += 2;
		}
		if (pIn < pInLimit && *pMatch == *pIn)
		{
			pIn++;
		}
		return (uint)(pIn - ptr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static uint LZ4_hashPosition(void* p, tableType_t tableType)
	{
		return LL.LZ4_hash4(Mem.Peek4(p), tableType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static void LZ4_putPosition(byte* p, void* tableBase, tableType_t tableType, byte* srcBase)
	{
		LL.LZ4_putPositionOnHash(p, LZ4_hashPosition(p, tableType), tableBase, tableType, srcBase);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static byte* LZ4_getPosition(byte* p, void* tableBase, tableType_t tableType, byte* srcBase)
	{
		return LL.LZ4_getPositionOnHash(LZ4_hashPosition(p, tableType), tableBase, tableType, srcBase);
	}

	protected unsafe static void LZ4_renormDictT(LZ4_stream_t* LZ4_dict, int nextSize)
	{
		if ((uint)((int)LZ4_dict->currentOffset + nextSize) <= 2147483648u)
		{
			return;
		}
		uint num = LZ4_dict->currentOffset - 65536;
		byte* ptr = LZ4_dict->dictionary + LZ4_dict->dictSize;
		for (int i = 0; i < 4096; i++)
		{
			if (LZ4_dict->hashTable[i] < num)
			{
				LZ4_dict->hashTable[i] = 0u;
			}
			else
			{
				LZ4_dict->hashTable[i] -= num;
			}
		}
		LZ4_dict->currentOffset = 65536u;
		if (LZ4_dict->dictSize > 65536)
		{
			LZ4_dict->dictSize = 65536u;
		}
		LZ4_dict->dictionary = ptr - LZ4_dict->dictSize;
	}

	public unsafe int LZ4_loadDict(LZ4_stream_t* LZ4_dict, byte* dictionary, int dictSize)
	{
		byte* ptr = dictionary;
		byte* ptr2 = ptr + dictSize;
		LL.LZ4_initStream(LZ4_dict);
		LZ4_dict->currentOffset += 65536u;
		if (dictSize < 4)
		{
			return 0;
		}
		if (ptr2 - ptr > 65536)
		{
			ptr = ptr2 - 65536;
		}
		byte* srcBase = ptr2 - LZ4_dict->currentOffset;
		LZ4_dict->dictionary = ptr;
		LZ4_dict->dictSize = (uint)(ptr2 - ptr);
		LZ4_dict->tableType = tableType_t.byU32;
		for (; ptr <= ptr2 - 4; ptr += 3)
		{
			LZ4_putPosition(ptr, LZ4_dict->hashTable, tableType_t.byU32, srcBase);
		}
		return (int)LZ4_dict->dictSize;
	}
}
