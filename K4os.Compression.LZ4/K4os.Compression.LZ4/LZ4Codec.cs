using System;
using K4os.Compression.LZ4.Engine;

namespace K4os.Compression.LZ4;

public static class LZ4Codec
{
	public const int Version = 192;

	public static bool Enforce32
	{
		get
		{
			return LL.Enforce32;
		}
		set
		{
			LL.Enforce32 = value;
		}
	}

	public static int MaximumOutputSize(int length)
	{
		return LL.LZ4_compressBound(length);
	}

	public unsafe static int Encode(byte* source, int sourceLength, byte* target, int targetLength, LZ4Level level = LZ4Level.L00_FAST)
	{
		if (sourceLength <= 0)
		{
			return 0;
		}
		int num = ((level < LZ4Level.L03_HC) ? LLxx.LZ4_compress_fast(source, target, sourceLength, targetLength, 1) : LLxx.LZ4_compress_HC(source, target, sourceLength, targetLength, (int)level));
		if (num > 0)
		{
			return num;
		}
		return -1;
	}

	public unsafe static int Encode(ReadOnlySpan<byte> source, Span<byte> target, LZ4Level level = LZ4Level.L00_FAST)
	{
		int length = source.Length;
		if (length <= 0)
		{
			return 0;
		}
		int length2 = target.Length;
		fixed (byte* source2 = source)
		{
			fixed (byte* target2 = target)
			{
				return Encode(source2, length, target2, length2, level);
			}
		}
	}

	public unsafe static int Encode(byte[] source, int sourceOffset, int sourceLength, byte[] target, int targetOffset, int targetLength, LZ4Level level = LZ4Level.L00_FAST)
	{
		source.Validate(sourceOffset, sourceLength);
		target.Validate(targetOffset, targetLength);
		fixed (byte* ptr = source)
		{
			fixed (byte* ptr2 = target)
			{
				return Encode(ptr + sourceOffset, sourceLength, ptr2 + targetOffset, targetLength, level);
			}
		}
	}

	public unsafe static int Decode(byte* source, int sourceLength, byte* target, int targetLength)
	{
		if (sourceLength <= 0)
		{
			return 0;
		}
		int num = LLxx.LZ4_decompress_safe(source, target, sourceLength, targetLength);
		if (num > 0)
		{
			return num;
		}
		return -1;
	}

	public unsafe static int PartialDecode(byte* source, int sourceLength, byte* target, int targetLength)
	{
		if (sourceLength <= 0)
		{
			return 0;
		}
		int num = LLxx.LZ4_decompress_safe_partial(source, target, sourceLength, targetLength);
		if (num > 0)
		{
			return num;
		}
		return -1;
	}

	public unsafe static int Decode(byte* source, int sourceLength, byte* target, int targetLength, byte* dictionary, int dictionaryLength)
	{
		if (sourceLength <= 0)
		{
			return 0;
		}
		int num = LLxx.LZ4_decompress_safe_usingDict(source, target, sourceLength, targetLength, dictionary, dictionaryLength);
		if (num > 0)
		{
			return num;
		}
		return -1;
	}

	public unsafe static int PartialDecode(ReadOnlySpan<byte> source, Span<byte> target)
	{
		int length = source.Length;
		if (length <= 0)
		{
			return 0;
		}
		fixed (byte* source2 = source)
		{
			fixed (byte* target2 = target)
			{
				return PartialDecode(source2, length, target2, target.Length);
			}
		}
	}

	public unsafe static int Decode(ReadOnlySpan<byte> source, Span<byte> target)
	{
		int length = source.Length;
		if (length <= 0)
		{
			return 0;
		}
		int length2 = target.Length;
		fixed (byte* source2 = source)
		{
			fixed (byte* target2 = target)
			{
				return Decode(source2, length, target2, length2);
			}
		}
	}

	public unsafe static int Decode(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> dictionary)
	{
		int length = source.Length;
		if (length <= 0)
		{
			return 0;
		}
		int length2 = target.Length;
		int length3 = dictionary.Length;
		fixed (byte* source2 = source)
		{
			fixed (byte* target2 = target)
			{
				fixed (byte* dictionary2 = dictionary)
				{
					return Decode(source2, length, target2, length2, dictionary2, length3);
				}
			}
		}
	}

	public unsafe static int Decode(byte[] source, int sourceOffset, int sourceLength, byte[] target, int targetOffset, int targetLength)
	{
		source.Validate(sourceOffset, sourceLength);
		target.Validate(targetOffset, targetLength);
		fixed (byte* ptr = source)
		{
			fixed (byte* ptr2 = target)
			{
				return Decode(ptr + sourceOffset, sourceLength, ptr2 + targetOffset, targetLength);
			}
		}
	}

	public unsafe static int Decode(byte[] source, int sourceOffset, int sourceLength, byte[] target, int targetOffset, int targetLength, byte[]? dictionary, int dictionaryOffset, int dictionaryLength)
	{
		source.Validate(sourceOffset, sourceLength);
		target.Validate(targetOffset, targetLength);
		dictionary.Validate(dictionaryOffset, dictionaryLength, allowNullIfEmpty: true);
		fixed (byte* ptr = source)
		{
			fixed (byte* ptr2 = target)
			{
				fixed (byte* ptr3 = dictionary)
				{
					return Decode(ptr + sourceOffset, sourceLength, ptr2 + targetOffset, targetLength, ptr3 + dictionaryOffset, dictionaryLength);
				}
			}
		}
	}
}
