using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[GenerateTestsForBurstCompatibility]
public struct Unicode
{
	[GenerateTestsForBurstCompatibility]
	public struct Rune
	{
		public int value;

		public Rune(int codepoint)
		{
			value = codepoint;
		}

		public static implicit operator Rune(char codepoint)
		{
			Rune result = default(Rune);
			result.value = codepoint;
			return result;
		}

		public static bool operator ==(Rune lhs, Rune rhs)
		{
			return lhs.value == rhs.value;
		}

		[ExcludeFromBurstCompatTesting("Takes managed object")]
		public override bool Equals(object obj)
		{
			if (obj is Rune)
			{
				return value == ((Rune)obj).value;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return value;
		}

		public static bool operator !=(Rune lhs, Rune rhs)
		{
			return lhs.value != rhs.value;
		}

		public static bool IsDigit(Rune r)
		{
			return r.IsDigit();
		}

		internal bool IsAscii()
		{
			return value < 128;
		}

		internal bool IsLatin1()
		{
			return value < 256;
		}

		internal bool IsDigit()
		{
			if (value >= 48)
			{
				return value <= 57;
			}
			return false;
		}

		internal bool IsWhiteSpace()
		{
			if (IsLatin1())
			{
				if (value != 32 && (value < 9 || value > 13) && value != 160)
				{
					return value == 133;
				}
				return true;
			}
			if (value != 5760 && (value < 8192 || value > 8202) && value != 8232 && value != 8233 && value != 8239 && value != 8287)
			{
				return value == 12288;
			}
			return true;
		}

		internal Rune ToLowerAscii()
		{
			return new Rune(value + (((uint)(value - 65) <= 25u) ? 32 : 0));
		}

		internal Rune ToUpperAscii()
		{
			return new Rune(value - (((uint)(value - 97) <= 25u) ? 32 : 0));
		}

		public int LengthInUtf8Bytes()
		{
			if (value < 0)
			{
				return 4;
			}
			if (value <= 127)
			{
				return 1;
			}
			if (value <= 2047)
			{
				return 2;
			}
			if (value <= 65535)
			{
				return 3;
			}
			_ = value;
			_ = 2097151;
			return 4;
		}
	}

	public const int kMaximumValidCodePoint = 1114111;

	public static Rune ReplacementCharacter
	{
		get
		{
			Rune result = default(Rune);
			result.value = 65533;
			return result;
		}
	}

	public static Rune BadRune
	{
		get
		{
			Rune result = default(Rune);
			result.value = 0;
			return result;
		}
	}

	public static bool IsValidCodePoint(int codepoint)
	{
		if (codepoint > 1114111)
		{
			return false;
		}
		if (codepoint < 0)
		{
			return false;
		}
		return true;
	}

	public static bool NotTrailer(byte b)
	{
		return (b & 0xC0) != 128;
	}

	public unsafe static ConversionError Utf8ToUcs(out Rune rune, byte* buffer, ref int index, int capacity)
	{
		int num = 0;
		rune = ReplacementCharacter;
		if (index + 1 > capacity)
		{
			return ConversionError.Overflow;
		}
		if ((buffer[index] & 0x80) == 0)
		{
			rune.value = buffer[index];
			index++;
			return ConversionError.None;
		}
		if ((buffer[index] & 0xE0) == 192)
		{
			if (index + 2 > capacity)
			{
				index++;
				return ConversionError.Overflow;
			}
			num = buffer[index] & 0x1F;
			num = (num << 6) | (buffer[index + 1] & 0x3F);
			if (num < 128 || NotTrailer(buffer[index + 1]))
			{
				index++;
				return ConversionError.Encoding;
			}
			rune.value = num;
			index += 2;
			return ConversionError.None;
		}
		if ((buffer[index] & 0xF0) == 224)
		{
			if (index + 3 > capacity)
			{
				index++;
				return ConversionError.Overflow;
			}
			num = buffer[index] & 0xF;
			num = (num << 6) | (buffer[index + 1] & 0x3F);
			num = (num << 6) | (buffer[index + 2] & 0x3F);
			if (num < 2048 || !IsValidCodePoint(num) || NotTrailer(buffer[index + 1]) || NotTrailer(buffer[index + 2]))
			{
				index++;
				return ConversionError.Encoding;
			}
			rune.value = num;
			index += 3;
			return ConversionError.None;
		}
		if ((buffer[index] & 0xF8) == 240)
		{
			if (index + 4 > capacity)
			{
				index++;
				return ConversionError.Overflow;
			}
			num = buffer[index] & 7;
			num = (num << 6) | (buffer[index + 1] & 0x3F);
			num = (num << 6) | (buffer[index + 2] & 0x3F);
			num = (num << 6) | (buffer[index + 3] & 0x3F);
			if (num < 65536 || !IsValidCodePoint(num) || NotTrailer(buffer[index + 1]) || NotTrailer(buffer[index + 2]) || NotTrailer(buffer[index + 3]))
			{
				index++;
				return ConversionError.Encoding;
			}
			rune.value = num;
			index += 4;
			return ConversionError.None;
		}
		index++;
		return ConversionError.Encoding;
	}

	private unsafe static int FindUtf8CharStartInReverse(byte* ptr, ref int index)
	{
		do
		{
			if (index <= 0)
			{
				return 0;
			}
			index--;
		}
		while ((ptr[index] & 0xC0) == 128);
		return index;
	}

	internal unsafe static ConversionError Utf8ToUcsReverse(out Rune rune, byte* buffer, ref int index, int capacity)
	{
		int num = index;
		index--;
		index = FindUtf8CharStartInReverse(buffer, ref index);
		if (index == num)
		{
			rune = ReplacementCharacter;
			return ConversionError.Overflow;
		}
		int index2 = index;
		return Utf8ToUcs(out rune, buffer, ref index2, capacity);
	}

	private static bool IsLeadingSurrogate(char c)
	{
		if (c >= '\ud800')
		{
			return c <= '\udbff';
		}
		return false;
	}

	private static bool IsTrailingSurrogate(char c)
	{
		if (c >= '\udc00')
		{
			return c <= '\udfff';
		}
		return false;
	}

	public unsafe static ConversionError Utf16ToUcs(out Rune rune, char* buffer, ref int index, int capacity)
	{
		int num = 0;
		rune = ReplacementCharacter;
		if (index + 1 > capacity)
		{
			return ConversionError.Overflow;
		}
		if (!IsLeadingSurrogate(buffer[index]) || index + 2 > capacity)
		{
			rune.value = buffer[index];
			index++;
			return ConversionError.None;
		}
		num = buffer[index] & 0x3FF;
		if (!IsTrailingSurrogate(buffer[index + 1]))
		{
			rune.value = buffer[index];
			index++;
			return ConversionError.None;
		}
		num = (num << 10) | (buffer[index + 1] & 0x3FF);
		num += 65536;
		rune.value = num;
		index += 2;
		return ConversionError.None;
	}

	internal unsafe static ConversionError UcsToUcs(out Rune rune, Rune* buffer, ref int index, int capacity)
	{
		rune = ReplacementCharacter;
		if (index + 1 > capacity)
		{
			return ConversionError.Overflow;
		}
		rune = buffer[index];
		index++;
		return ConversionError.None;
	}

	public unsafe static ConversionError UcsToUtf8(byte* buffer, ref int index, int capacity, Rune rune)
	{
		if (!IsValidCodePoint(rune.value))
		{
			return ConversionError.CodePoint;
		}
		if (index + 1 > capacity)
		{
			return ConversionError.Overflow;
		}
		if (rune.value <= 127)
		{
			buffer[index++] = (byte)rune.value;
			return ConversionError.None;
		}
		if (rune.value <= 2047)
		{
			if (index + 2 > capacity)
			{
				return ConversionError.Overflow;
			}
			buffer[index++] = (byte)(0xC0u | (uint)(rune.value >> 6));
			buffer[index++] = (byte)(0x80u | ((uint)rune.value & 0x3Fu));
			return ConversionError.None;
		}
		if (rune.value <= 65535)
		{
			if (index + 3 > capacity)
			{
				return ConversionError.Overflow;
			}
			buffer[index++] = (byte)(0xE0u | (uint)(rune.value >> 12));
			buffer[index++] = (byte)(0x80u | ((uint)(rune.value >> 6) & 0x3Fu));
			buffer[index++] = (byte)(0x80u | ((uint)rune.value & 0x3Fu));
			return ConversionError.None;
		}
		if (rune.value <= 2097151)
		{
			if (index + 4 > capacity)
			{
				return ConversionError.Overflow;
			}
			buffer[index++] = (byte)(0xF0u | (uint)(rune.value >> 18));
			buffer[index++] = (byte)(0x80u | ((uint)(rune.value >> 12) & 0x3Fu));
			buffer[index++] = (byte)(0x80u | ((uint)(rune.value >> 6) & 0x3Fu));
			buffer[index++] = (byte)(0x80u | ((uint)rune.value & 0x3Fu));
			return ConversionError.None;
		}
		return ConversionError.Encoding;
	}

	public unsafe static ConversionError UcsToUtf16(char* buffer, ref int index, int capacity, Rune rune)
	{
		if (!IsValidCodePoint(rune.value))
		{
			return ConversionError.CodePoint;
		}
		if (index + 1 > capacity)
		{
			return ConversionError.Overflow;
		}
		if (rune.value >= 65536)
		{
			if (index + 2 > capacity)
			{
				return ConversionError.Overflow;
			}
			int num = rune.value - 65536;
			if (num >= 1048576)
			{
				return ConversionError.Encoding;
			}
			buffer[index++] = (char)(0xD800u | (uint)(num >> 10));
			buffer[index++] = (char)(0xDC00u | ((uint)num & 0x3FFu));
			return ConversionError.None;
		}
		buffer[index++] = (char)rune.value;
		return ConversionError.None;
	}

	public unsafe static ConversionError Utf16ToUtf8(char* utf16Buffer, int utf16Length, byte* utf8Buffer, out int utf8Length, int utf8Capacity)
	{
		utf8Length = 0;
		int index = 0;
		while (index < utf16Length)
		{
			Utf16ToUcs(out var rune, utf16Buffer, ref index, utf16Length);
			if (UcsToUtf8(utf8Buffer, ref utf8Length, utf8Capacity, rune) == ConversionError.Overflow)
			{
				return ConversionError.Overflow;
			}
		}
		return ConversionError.None;
	}

	public unsafe static ConversionError Utf8ToUtf8(byte* srcBuffer, int srcLength, byte* destBuffer, out int destLength, int destCapacity)
	{
		if (destCapacity >= srcLength)
		{
			UnsafeUtility.MemCpy(destBuffer, srcBuffer, srcLength);
			destLength = srcLength;
			return ConversionError.None;
		}
		destLength = 0;
		int index = 0;
		while (index < srcLength)
		{
			Utf8ToUcs(out var rune, srcBuffer, ref index, srcLength);
			if (UcsToUtf8(destBuffer, ref destLength, destCapacity, rune) == ConversionError.Overflow)
			{
				return ConversionError.Overflow;
			}
		}
		return ConversionError.None;
	}

	public unsafe static ConversionError Utf8ToUtf16(byte* utf8Buffer, int utf8Length, char* utf16Buffer, out int utf16Length, int utf16Capacity)
	{
		utf16Length = 0;
		int index = 0;
		while (index < utf8Length)
		{
			Utf8ToUcs(out var rune, utf8Buffer, ref index, utf8Length);
			if (UcsToUtf16(utf16Buffer, ref utf16Length, utf16Capacity, rune) == ConversionError.Overflow)
			{
				return ConversionError.Overflow;
			}
		}
		return ConversionError.None;
	}

	private unsafe static int CountRunes(byte* utf8Buffer, int utf8Length, int maxRunes = int.MaxValue)
	{
		int num = 0;
		int num2 = 0;
		while (num < maxRunes && num2 < utf8Length)
		{
			if ((utf8Buffer[num2] & 0xC0) != 128)
			{
				num++;
			}
			num2++;
		}
		return num;
	}
}
