using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class UTF8ArrayUnsafeUtility
{
	internal struct Comparison
	{
		public bool terminates;

		public int result;

		public Comparison(Unicode.Rune runeA, ConversionError errorA, Unicode.Rune runeB, ConversionError errorB)
		{
			if (errorA != 0)
			{
				runeA.value = 0;
			}
			if (errorB != 0)
			{
				runeB.value = 0;
			}
			if (runeA.value != runeB.value)
			{
				result = runeA.value - runeB.value;
				terminates = true;
			}
			else
			{
				result = 0;
				terminates = runeA.value == 0 && runeB.value == 0;
			}
		}
	}

	public unsafe static CopyError Copy(byte* dest, out int destLength, int destUTF8MaxLengthInBytes, char* src, int srcLength)
	{
		if (Unicode.Utf16ToUtf8(src, srcLength, dest, out destLength, destUTF8MaxLengthInBytes) == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static CopyError Copy(byte* dest, out ushort destLength, ushort destUTF8MaxLengthInBytes, char* src, int srcLength)
	{
		int utf8Length;
		ConversionError num = Unicode.Utf16ToUtf8(src, srcLength, dest, out utf8Length, destUTF8MaxLengthInBytes);
		destLength = (ushort)utf8Length;
		if (num == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static CopyError Copy(byte* dest, out int destLength, int destUTF8MaxLengthInBytes, byte* src, int srcLength)
	{
		int destLength2;
		ConversionError num = Unicode.Utf8ToUtf8(src, srcLength, dest, out destLength2, destUTF8MaxLengthInBytes);
		destLength = destLength2;
		if (num == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static CopyError Copy(byte* dest, out ushort destLength, ushort destUTF8MaxLengthInBytes, byte* src, ushort srcLength)
	{
		int destLength2;
		ConversionError num = Unicode.Utf8ToUtf8(src, srcLength, dest, out destLength2, destUTF8MaxLengthInBytes);
		destLength = (ushort)destLength2;
		if (num == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static CopyError Copy(char* dest, out int destLength, int destUCS2MaxLengthInChars, byte* src, int srcLength)
	{
		if (Unicode.Utf8ToUtf16(src, srcLength, dest, out destLength, destUCS2MaxLengthInChars) == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static CopyError Copy(char* dest, out ushort destLength, ushort destUCS2MaxLengthInChars, byte* src, ushort srcLength)
	{
		int utf16Length;
		ConversionError num = Unicode.Utf8ToUtf16(src, srcLength, dest, out utf16Length, destUCS2MaxLengthInChars);
		destLength = (ushort)utf16Length;
		if (num == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static FormatError AppendUTF8Bytes(byte* dest, ref int destLength, int destCapacity, byte* src, int srcLength)
	{
		if (destLength + srcLength > destCapacity)
		{
			return FormatError.Overflow;
		}
		UnsafeUtility.MemCpy(dest + destLength, src, srcLength);
		destLength += srcLength;
		return FormatError.None;
	}

	public unsafe static CopyError Append(byte* dest, ref ushort destLength, ushort destUTF8MaxLengthInBytes, byte* src, ushort srcLength)
	{
		int destLength2;
		ConversionError num = Unicode.Utf8ToUtf8(src, srcLength, dest + (int)destLength, out destLength2, destUTF8MaxLengthInBytes - destLength);
		destLength += (ushort)destLength2;
		if (num == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static CopyError Append(byte* dest, ref ushort destLength, ushort destUTF8MaxLengthInBytes, char* src, int srcLength)
	{
		int utf8Length;
		ConversionError num = Unicode.Utf16ToUtf8(src, srcLength, dest + (int)destLength, out utf8Length, destUTF8MaxLengthInBytes - destLength);
		destLength += (ushort)utf8Length;
		if (num == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static CopyError Append(char* dest, ref ushort destLength, ushort destUCS2MaxLengthInChars, byte* src, ushort srcLength)
	{
		int utf16Length;
		ConversionError num = Unicode.Utf8ToUtf16(src, srcLength, dest + (int)destLength, out utf16Length, destUCS2MaxLengthInChars - destLength);
		destLength += (ushort)utf16Length;
		if (num == ConversionError.None)
		{
			return CopyError.None;
		}
		return CopyError.Truncation;
	}

	public unsafe static int StrCmp(byte* utf8BufferA, int utf8LengthInBytesA, byte* utf8BufferB, int utf8LengthInBytesB)
	{
		int index = 0;
		int index2 = 0;
		Comparison comparison;
		do
		{
			Unicode.Rune rune;
			ConversionError errorA = Unicode.Utf8ToUcs(out rune, utf8BufferA, ref index, utf8LengthInBytesA);
			Unicode.Rune rune2;
			ConversionError errorB = Unicode.Utf8ToUcs(out rune2, utf8BufferB, ref index2, utf8LengthInBytesB);
			comparison = new Comparison(rune, errorA, rune2, errorB);
		}
		while (!comparison.terminates);
		return comparison.result;
	}

	internal unsafe static int StrCmp(byte* utf8BufferA, int utf8LengthInBytesA, Unicode.Rune* runeBufferB, int lengthInRunesB)
	{
		int index = 0;
		int index2 = 0;
		Comparison comparison;
		do
		{
			Unicode.Rune rune;
			ConversionError errorA = Unicode.Utf8ToUcs(out rune, utf8BufferA, ref index, utf8LengthInBytesA);
			Unicode.Rune rune2;
			ConversionError errorB = Unicode.UcsToUcs(out rune2, runeBufferB, ref index2, lengthInRunesB);
			comparison = new Comparison(rune, errorA, rune2, errorB);
		}
		while (!comparison.terminates);
		return comparison.result;
	}

	public unsafe static int StrCmp(char* utf16BufferA, int utf16LengthInCharsA, char* utf16BufferB, int utf16LengthInCharsB)
	{
		int index = 0;
		int index2 = 0;
		Comparison comparison;
		do
		{
			Unicode.Rune rune;
			ConversionError errorA = Unicode.Utf16ToUcs(out rune, utf16BufferA, ref index, utf16LengthInCharsA);
			Unicode.Rune rune2;
			ConversionError errorB = Unicode.Utf16ToUcs(out rune2, utf16BufferB, ref index2, utf16LengthInCharsB);
			comparison = new Comparison(rune, errorA, rune2, errorB);
		}
		while (!comparison.terminates);
		return comparison.result;
	}

	public unsafe static bool EqualsUTF8Bytes(byte* aBytes, int aLength, byte* bBytes, int bLength)
	{
		if (aLength == bLength)
		{
			return StrCmp(aBytes, aLength, bBytes, bLength) == 0;
		}
		return false;
	}

	public unsafe static int StrCmp(byte* utf8Buffer, int utf8LengthInBytes, char* utf16Buffer, int utf16LengthInChars)
	{
		int index = 0;
		int index2 = 0;
		Comparison comparison;
		do
		{
			Unicode.Rune rune;
			ConversionError errorA = Unicode.Utf8ToUcs(out rune, utf8Buffer, ref index, utf8LengthInBytes);
			Unicode.Rune rune2;
			ConversionError errorB = Unicode.Utf16ToUcs(out rune2, utf16Buffer, ref index2, utf16LengthInChars);
			comparison = new Comparison(rune, errorA, rune2, errorB);
		}
		while (!comparison.terminates);
		return comparison.result;
	}

	public unsafe static int StrCmp(char* utf16Buffer, int utf16LengthInChars, byte* utf8Buffer, int utf8LengthInBytes)
	{
		return -StrCmp(utf8Buffer, utf8LengthInBytes, utf16Buffer, utf16LengthInChars);
	}
}
