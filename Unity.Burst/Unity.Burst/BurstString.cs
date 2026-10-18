using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Burst;

internal static class BurstString
{
	internal class PreserveAttribute : Attribute
	{
	}

	private enum NumberBufferKind
	{
		Integer,
		Float
	}

	private struct NumberBuffer
	{
		private unsafe readonly byte* _buffer;

		public NumberBufferKind Kind;

		public int DigitsCount;

		public int Scale;

		public readonly bool IsNegative;

		public unsafe NumberBuffer(NumberBufferKind kind, byte* buffer, int digitsCount, int scale, bool isNegative)
		{
			Kind = kind;
			_buffer = buffer;
			DigitsCount = digitsCount;
			Scale = scale;
			IsNegative = isNegative;
		}

		public unsafe byte* GetDigitsPointer()
		{
			return _buffer;
		}
	}

	public enum NumberFormatKind : byte
	{
		General,
		Decimal,
		DecimalForceSigned,
		Hexadecimal
	}

	public struct FormatOptions
	{
		public NumberFormatKind Kind;

		public sbyte AlignAndSize;

		public byte Specifier;

		public bool Lowercase;

		public bool Uppercase => !Lowercase;

		public FormatOptions(NumberFormatKind kind, sbyte alignAndSize, byte specifier, bool lowercase)
		{
			this = default(FormatOptions);
			Kind = kind;
			AlignAndSize = alignAndSize;
			Specifier = specifier;
			Lowercase = lowercase;
		}

		public unsafe int EncodeToRaw()
		{
			FormatOptions formatOptions = this;
			return *(int*)(&formatOptions);
		}

		public int GetBase()
		{
			if (Kind == NumberFormatKind.Hexadecimal)
			{
				return 16;
			}
			return 10;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", "Kind", Kind, "AlignAndSize", AlignAndSize, "Specifier", Specifier, "Uppercase", Uppercase);
		}
	}

	public struct tBigInt
	{
		private const int c_BigInt_MaxBlocks = 35;

		public int m_length;

		public unsafe fixed uint m_blocks[35];

		public int GetLength()
		{
			return m_length;
		}

		public unsafe uint GetBlock(int idx)
		{
			return m_blocks[idx];
		}

		public void SetZero()
		{
			m_length = 0;
		}

		public bool IsZero()
		{
			return m_length == 0;
		}

		public unsafe void SetU64(ulong val)
		{
			if (val > uint.MaxValue)
			{
				m_blocks[0] = (uint)(val & 0xFFFFFFFFu);
				m_blocks[1] = (uint)((val >> 32) & 0xFFFFFFFFu);
				m_length = 2;
			}
			else if (val != 0L)
			{
				m_blocks[0] = (uint)(val & 0xFFFFFFFFu);
				m_length = 1;
			}
			else
			{
				m_length = 0;
			}
		}

		public unsafe void SetU32(uint val)
		{
			if (val != 0)
			{
				m_blocks[0] = val;
				m_length = ((val != 0) ? 1 : 0);
			}
			else
			{
				m_length = 0;
			}
		}

		public unsafe uint GetU32()
		{
			if (m_length != 0)
			{
				return m_blocks[0];
			}
			return 0u;
		}
	}

	public enum CutoffMode
	{
		Unique,
		TotalLength,
		FractionLength
	}

	public enum PrintFloatFormat
	{
		Positional,
		Scientific
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct tFloatUnion32
	{
		[FieldOffset(0)]
		public float m_floatingPoint;

		[FieldOffset(0)]
		public uint m_integer;

		public bool IsNegative()
		{
			return m_integer >> 31 != 0;
		}

		public uint GetExponent()
		{
			return (m_integer >> 23) & 0xFFu;
		}

		public uint GetMantissa()
		{
			return m_integer & 0x7FFFFFu;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct tFloatUnion64
	{
		[FieldOffset(0)]
		public double m_floatingPoint;

		[FieldOffset(0)]
		public ulong m_integer;

		public bool IsNegative()
		{
			return m_integer >> 63 != 0;
		}

		public uint GetExponent()
		{
			return (uint)((m_integer >> 52) & 0x7FF);
		}

		public ulong GetMantissa()
		{
			return m_integer & 0xFFFFFFFFFFFFFuL;
		}
	}

	private static readonly char[] SplitByColon = new char[1] { ':' };

	private static readonly byte[] logTable = new byte[256]
	{
		0, 0, 1, 1, 2, 2, 2, 2, 3, 3,
		3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7
	};

	private static readonly uint[] g_PowerOf10_U32 = new uint[8] { 1u, 10u, 100u, 1000u, 10000u, 100000u, 1000000u, 10000000u };

	private static readonly byte[] InfinityString = new byte[8] { 73, 110, 102, 105, 110, 105, 116, 121 };

	private static readonly byte[] NanString = new byte[3] { 78, 97, 78 };

	private const int SinglePrecision = 9;

	private const int DoublePrecision = 17;

	internal const int SingleNumberBufferLength = 10;

	internal const int DoubleNumberBufferLength = 18;

	private const int SinglePrecisionCustomFormat = 7;

	private const int DoublePrecisionCustomFormat = 15;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Preserve]
	public unsafe static void CopyFixedString(byte* dest, int destLength, byte* src, int srcLength)
	{
		int num = ((srcLength > destLength) ? destLength : srcLength);
		*(ushort*)(dest - 2) = (ushort)num;
		dest[num] = 0;
		UnsafeUtility.MemCpy(dest, src, num);
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, byte* src, int srcLength, int formatOptionsRaw)
	{
		FormatOptions formatOptions = *(FormatOptions*)(&formatOptionsRaw);
		if (!AlignLeft(dest, ref destIndex, destLength, formatOptions.AlignAndSize, srcLength))
		{
			int num = destLength - destIndex;
			int num2 = ((srcLength > num) ? num : srcLength);
			if (num2 > 0)
			{
				UnsafeUtility.MemCpy(dest + destIndex, src, num2);
				destIndex += num2;
				AlignRight(dest, ref destIndex, destLength, formatOptions.AlignAndSize, srcLength);
			}
		}
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, float value, int formatOptionsRaw)
	{
		FormatOptions formatOptions = *(FormatOptions*)(&formatOptionsRaw);
		ConvertFloatToString(dest, ref destIndex, destLength, value, formatOptions);
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, double value, int formatOptionsRaw)
	{
		FormatOptions formatOptions = *(FormatOptions*)(&formatOptionsRaw);
		ConvertDoubleToString(dest, ref destIndex, destLength, value, formatOptions);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, bool value, int formatOptionsRaw)
	{
		int length = (value ? 4 : 5);
		FormatOptions formatOptions = *(FormatOptions*)(&formatOptionsRaw);
		if (AlignLeft(dest, ref destIndex, destLength, formatOptions.AlignAndSize, length))
		{
			return;
		}
		if (value)
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 84;
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 114;
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 117;
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 101;
		}
		else
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 70;
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 97;
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 108;
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 115;
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 101;
		}
		AlignRight(dest, ref destIndex, destLength, formatOptions.AlignAndSize, length);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, char value, int formatOptionsRaw)
	{
		int num = ((value <= '\u007f') ? 1 : ((value <= '߿') ? 2 : 3));
		FormatOptions formatOptions = *(FormatOptions*)(&formatOptionsRaw);
		if (AlignLeft(dest, ref destIndex, destLength, formatOptions.AlignAndSize, 1))
		{
			return;
		}
		switch (num)
		{
		case 1:
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = (byte)value;
			break;
		case 2:
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = (byte)((uint)((int)value >> 6) | 0xC0u);
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = (byte)((value & 0x3Fu) | 0x80u);
			break;
		case 3:
			if (value >= '\ud800' && value <= '\udfff')
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = 239;
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = 191;
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = 189;
			}
			else
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = (byte)((uint)((int)value >> 12) | 0xE0u);
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = (byte)(((uint)((int)value >> 6) & 0x3Fu) | 0x80u);
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = (byte)((value & 0x3Fu) | 0x80u);
			}
			break;
		}
		AlignRight(dest, ref destIndex, destLength, formatOptions.AlignAndSize, 1);
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, byte value, int formatOptionsRaw)
	{
		Format(dest, ref destIndex, destLength, (ulong)value, formatOptionsRaw);
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, ushort value, int formatOptionsRaw)
	{
		Format(dest, ref destIndex, destLength, (ulong)value, formatOptionsRaw);
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, uint value, int formatOptionsRaw)
	{
		FormatOptions options = *(FormatOptions*)(&formatOptionsRaw);
		ConvertUnsignedIntegerToString(dest, ref destIndex, destLength, value, options);
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, ulong value, int formatOptionsRaw)
	{
		FormatOptions options = *(FormatOptions*)(&formatOptionsRaw);
		ConvertUnsignedIntegerToString(dest, ref destIndex, destLength, value, options);
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, sbyte value, int formatOptionsRaw)
	{
		FormatOptions options = *(FormatOptions*)(&formatOptionsRaw);
		if (options.Kind == NumberFormatKind.Hexadecimal)
		{
			ConvertUnsignedIntegerToString(dest, ref destIndex, destLength, (byte)value, options);
		}
		else
		{
			ConvertIntegerToString(dest, ref destIndex, destLength, value, options);
		}
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, short value, int formatOptionsRaw)
	{
		FormatOptions options = *(FormatOptions*)(&formatOptionsRaw);
		if (options.Kind == NumberFormatKind.Hexadecimal)
		{
			ConvertUnsignedIntegerToString(dest, ref destIndex, destLength, (ushort)value, options);
		}
		else
		{
			ConvertIntegerToString(dest, ref destIndex, destLength, value, options);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, int value, int formatOptionsRaw)
	{
		FormatOptions options = *(FormatOptions*)(&formatOptionsRaw);
		if (options.Kind == NumberFormatKind.Hexadecimal)
		{
			ConvertUnsignedIntegerToString(dest, ref destIndex, destLength, (uint)value, options);
		}
		else
		{
			ConvertIntegerToString(dest, ref destIndex, destLength, value, options);
		}
	}

	[Preserve]
	public unsafe static void Format(byte* dest, ref int destIndex, int destLength, long value, int formatOptionsRaw)
	{
		FormatOptions options = *(FormatOptions*)(&formatOptionsRaw);
		if (options.Kind == NumberFormatKind.Hexadecimal)
		{
			ConvertUnsignedIntegerToString(dest, ref destIndex, destLength, (ulong)value, options);
		}
		else
		{
			ConvertIntegerToString(dest, ref destIndex, destLength, value, options);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static void ConvertUnsignedIntegerToString(byte* dest, ref int destIndex, int destLength, ulong value, FormatOptions options)
	{
		uint @base = (uint)options.GetBase();
		if (@base >= 2 && @base <= 36)
		{
			int num = 0;
			ulong num2 = value;
			do
			{
				num2 /= @base;
				num++;
			}
			while (num2 != 0L);
			int num3 = num - 1;
			byte* ptr = stackalloc byte[(int)(uint)(num + 1)];
			num2 = value;
			do
			{
				ptr[num3--] = ValueToIntegerChar((int)(num2 % @base), options.Uppercase);
				num2 /= @base;
			}
			while (num2 != 0L);
			ptr[num] = 0;
			NumberBuffer number = new NumberBuffer(NumberBufferKind.Integer, ptr, num, num, isNegative: false);
			FormatNumber(dest, ref destIndex, destLength, ref number, options.Specifier, options);
		}
	}

	private static int GetLengthIntegerToString(long value, int basis, int zeroPadding)
	{
		int num = 0;
		long num2 = value;
		do
		{
			num2 /= basis;
			num++;
		}
		while (num2 != 0L);
		if (num < zeroPadding)
		{
			num = zeroPadding;
		}
		if (value < 0)
		{
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static void ConvertIntegerToString(byte* dest, ref int destIndex, int destLength, long value, FormatOptions options)
	{
		int @base = options.GetBase();
		if (@base >= 2 && @base <= 36)
		{
			int num = 0;
			long num2 = value;
			do
			{
				num2 /= @base;
				num++;
			}
			while (num2 != 0L);
			byte* ptr = stackalloc byte[(int)(uint)(num + 1)];
			num2 = value;
			int num3 = num - 1;
			do
			{
				ptr[num3--] = ValueToIntegerChar((int)(num2 % @base), options.Uppercase);
				num2 /= @base;
			}
			while (num2 != 0L);
			ptr[num] = 0;
			NumberBuffer number = new NumberBuffer(NumberBufferKind.Integer, ptr, num, num, value < 0);
			FormatNumber(dest, ref destIndex, destLength, ref number, options.Specifier, options);
		}
	}

	private unsafe static void FormatNumber(byte* dest, ref int destIndex, int destLength, ref NumberBuffer number, int nMaxDigits, FormatOptions options)
	{
		bool isCorrectlyRounded = number.Kind == NumberBufferKind.Float;
		if (number.Kind == NumberBufferKind.Integer && options.Kind == NumberFormatKind.General && options.Specifier == 0)
		{
			options.Kind = NumberFormatKind.Decimal;
		}
		NumberFormatKind kind = options.Kind;
		if (kind != 0 && kind - 1 <= NumberFormatKind.DecimalForceSigned)
		{
			int num = number.DigitsCount;
			int specifier = options.Specifier;
			int zeroPadding = 0;
			if (num < specifier)
			{
				zeroPadding = specifier - num;
				num = specifier;
			}
			bool flag = options.Kind == NumberFormatKind.DecimalForceSigned;
			num += ((number.IsNegative || flag) ? 1 : 0);
			if (!AlignLeft(dest, ref destIndex, destLength, options.AlignAndSize, num))
			{
				FormatDecimalOrHexadecimal(dest, ref destIndex, destLength, ref number, zeroPadding, flag);
				AlignRight(dest, ref destIndex, destLength, options.AlignAndSize, num);
			}
		}
		else
		{
			if (nMaxDigits < 1)
			{
				nMaxDigits = number.DigitsCount;
			}
			RoundNumber(ref number, nMaxDigits, isCorrectlyRounded);
			int num = GetLengthForFormatGeneral(ref number, nMaxDigits);
			if (!AlignLeft(dest, ref destIndex, destLength, options.AlignAndSize, num))
			{
				FormatGeneral(dest, ref destIndex, destLength, ref number, nMaxDigits, (byte)(options.Uppercase ? 69 : 101));
				AlignRight(dest, ref destIndex, destLength, options.AlignAndSize, num);
			}
		}
	}

	private unsafe static void FormatDecimalOrHexadecimal(byte* dest, ref int destIndex, int destLength, ref NumberBuffer number, int zeroPadding, bool outputPositiveSign)
	{
		if (number.IsNegative)
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 45;
		}
		else if (outputPositiveSign)
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 43;
		}
		for (int i = 0; i < zeroPadding; i++)
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 48;
		}
		int digitsCount = number.DigitsCount;
		byte* digitsPointer = number.GetDigitsPointer();
		for (int j = 0; j < digitsCount; j++)
		{
			if (destIndex >= destLength)
			{
				break;
			}
			dest[destIndex++] = digitsPointer[j];
		}
	}

	private static byte ValueToIntegerChar(int value, bool uppercase)
	{
		value = ((value < 0) ? (-value) : value);
		if (value <= 9)
		{
			return (byte)(48 + value);
		}
		if (value < 36)
		{
			return (byte)((uppercase ? 65 : 97) + (value - 10));
		}
		return 63;
	}

	private static void OptsSplit(string fullFormat, out string padding, out string format)
	{
		string[] array = fullFormat.Split(SplitByColon, StringSplitOptions.RemoveEmptyEntries);
		format = array[0];
		padding = null;
		if (array.Length == 2)
		{
			padding = format;
			format = array[1];
			return;
		}
		if (array.Length == 1)
		{
			if (format[0] == ',')
			{
				padding = format;
				format = null;
			}
			return;
		}
		throw new ArgumentException($"Format `{format}` not supported. Invalid number {array.Length} of :. Expecting no more than one.");
	}

	public static FormatOptions ParseFormatToFormatOptions(string fullFormat)
	{
		if (string.IsNullOrWhiteSpace(fullFormat))
		{
			return default(FormatOptions);
		}
		OptsSplit(fullFormat, out var padding, out var format);
		format = format?.Trim();
		padding = padding?.Trim();
		int result = 0;
		NumberFormatKind kind = NumberFormatKind.General;
		bool lowercase = false;
		int num = 0;
		if (!string.IsNullOrEmpty(format))
		{
			switch (format[0])
			{
			case 'G':
				kind = NumberFormatKind.General;
				break;
			case 'g':
				kind = NumberFormatKind.General;
				lowercase = true;
				break;
			case 'D':
				kind = NumberFormatKind.Decimal;
				break;
			case 'd':
				kind = NumberFormatKind.Decimal;
				lowercase = true;
				break;
			case 'X':
				kind = NumberFormatKind.Hexadecimal;
				break;
			case 'x':
				kind = NumberFormatKind.Hexadecimal;
				lowercase = true;
				break;
			default:
				throw new ArgumentException("Format `" + format + "` not supported. Only G, g, D, d, X, x are supported.");
			}
			if (format.Length > 1)
			{
				string text = format.Substring(1);
				if (!uint.TryParse(text, out var result2))
				{
					throw new ArgumentException("Expecting an unsigned integer for specifier `" + format + "` instead of " + text + ".");
				}
				num = (int)result2;
			}
		}
		if (!string.IsNullOrEmpty(padding))
		{
			if (padding[0] != ',')
			{
				throw new ArgumentException("Invalid padding `" + padding + "`, expecting to start with a leading `,` comma.");
			}
			string text2 = padding.Substring(1);
			if (!int.TryParse(text2, out result))
			{
				throw new ArgumentException("Expecting an integer for align/size padding `" + text2 + "`.");
			}
		}
		return new FormatOptions(kind, (sbyte)result, (byte)num, lowercase);
	}

	private unsafe static bool AlignRight(byte* dest, ref int destIndex, int destLength, int align, int length)
	{
		if (align < 0)
		{
			align = -align;
			return AlignLeft(dest, ref destIndex, destLength, align, length);
		}
		return false;
	}

	private unsafe static bool AlignLeft(byte* dest, ref int destIndex, int destLength, int align, int length)
	{
		if (align > 0)
		{
			while (length < align)
			{
				if (destIndex >= destLength)
				{
					return true;
				}
				dest[destIndex++] = 32;
				length++;
			}
		}
		return false;
	}

	private unsafe static int GetLengthForFormatGeneral(ref NumberBuffer number, int nMaxDigits)
	{
		int num = 0;
		int i = number.Scale;
		bool flag = false;
		if (i > nMaxDigits || i < -3)
		{
			i = 1;
			flag = true;
		}
		byte* ptr = number.GetDigitsPointer();
		if (number.IsNegative)
		{
			num++;
		}
		if (i > 0)
		{
			do
			{
				if (*ptr != 0)
				{
					ptr++;
				}
				num++;
			}
			while (--i > 0);
		}
		else
		{
			num++;
		}
		if (*ptr != 0 || i < 0)
		{
			num++;
			for (; i < 0; i++)
			{
				num++;
			}
			for (; *ptr != 0; ptr++)
			{
				num++;
			}
		}
		if (flag)
		{
			num++;
			int num2 = number.Scale - 1;
			if (num2 >= 0)
			{
				num++;
			}
			num += GetLengthIntegerToString(num2, 10, 2);
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static void FormatGeneral(byte* dest, ref int destIndex, int destLength, ref NumberBuffer number, int nMaxDigits, byte expChar)
	{
		int i = number.Scale;
		bool flag = false;
		if (i > nMaxDigits || i < -3)
		{
			i = 1;
			flag = true;
		}
		byte* digitsPointer = number.GetDigitsPointer();
		if (number.IsNegative)
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 45;
		}
		if (i > 0)
		{
			do
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = (byte)((*digitsPointer != 0) ? (*(digitsPointer++)) : 48);
			}
			while (--i > 0);
		}
		else
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 48;
		}
		if (*digitsPointer != 0 || i < 0)
		{
			if (destIndex >= destLength)
			{
				return;
			}
			dest[destIndex++] = 46;
			for (; i < 0; i++)
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = 48;
			}
			while (*digitsPointer != 0)
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = *(digitsPointer++);
			}
		}
		if (flag && destIndex < destLength)
		{
			dest[destIndex++] = expChar;
			int num = number.Scale - 1;
			FormatOptions options = new FormatOptions(NumberFormatKind.DecimalForceSigned, 0, 2, lowercase: false);
			ConvertIntegerToString(dest, ref destIndex, destLength, num, options);
		}
	}

	private unsafe static void RoundNumber(ref NumberBuffer number, int pos, bool isCorrectlyRounded)
	{
		byte* digitsPointer = number.GetDigitsPointer();
		int i;
		for (i = 0; i < pos && digitsPointer[i] != 0; i++)
		{
		}
		if (i == pos && ShouldRoundUp(digitsPointer, i, isCorrectlyRounded))
		{
			while (i > 0 && digitsPointer[i - 1] == 57)
			{
				i--;
			}
			if (i > 0)
			{
				byte* num = digitsPointer + (i - 1);
				(*num)++;
			}
			else
			{
				number.Scale++;
				*digitsPointer = 49;
				i = 1;
			}
		}
		else
		{
			while (i > 0 && digitsPointer[i - 1] == 48)
			{
				i--;
			}
		}
		if (i == 0)
		{
			number.Scale = 0;
		}
		digitsPointer[i] = 0;
		number.DigitsCount = i;
	}

	private unsafe static bool ShouldRoundUp(byte* dig, int i, bool isCorrectlyRounded)
	{
		byte b = dig[i];
		if (b == 0 || isCorrectlyRounded)
		{
			return false;
		}
		return b >= 53;
	}

	private static uint LogBase2(uint val)
	{
		uint num = val >> 24;
		if (num != 0)
		{
			return (uint)(24 + logTable[num]);
		}
		num = val >> 16;
		if (num != 0)
		{
			return (uint)(16 + logTable[num]);
		}
		num = val >> 8;
		if (num != 0)
		{
			return (uint)(8 + logTable[num]);
		}
		return logTable[val];
	}

	private unsafe static int BigInt_Compare(in tBigInt lhs, in tBigInt rhs)
	{
		int num = lhs.m_length - rhs.m_length;
		if (num != 0)
		{
			return num;
		}
		for (int num2 = lhs.m_length - 1; num2 >= 0; num2--)
		{
			if (lhs.m_blocks[num2] != rhs.m_blocks[num2])
			{
				if (lhs.m_blocks[num2] > rhs.m_blocks[num2])
				{
					return 1;
				}
				return -1;
			}
		}
		return 0;
	}

	private static void BigInt_Add(out tBigInt pResult, in tBigInt lhs, in tBigInt rhs)
	{
		if (lhs.m_length < rhs.m_length)
		{
			BigInt_Add_internal(out pResult, in rhs, in lhs);
		}
		else
		{
			BigInt_Add_internal(out pResult, in lhs, in rhs);
		}
	}

	private unsafe static void BigInt_Add_internal(out tBigInt pResult, in tBigInt pLarge, in tBigInt pSmall)
	{
		int length = pLarge.m_length;
		int length2 = pSmall.m_length;
		pResult.m_length = length;
		ulong num = 0uL;
		fixed (uint* ptr = pLarge.m_blocks)
		{
			fixed (uint* ptr3 = pSmall.m_blocks)
			{
				fixed (uint* ptr5 = pResult.m_blocks)
				{
					uint* ptr2 = ptr;
					uint* ptr4 = ptr3;
					uint* ptr6 = ptr5;
					uint* ptr7 = ptr2 + length;
					uint* ptr8 = ptr4 + length2;
					while (ptr4 != ptr8)
					{
						ulong num2 = num + *ptr2 + *ptr4;
						num = num2 >> 32;
						*ptr6 = (uint)(num2 & 0xFFFFFFFFu);
						ptr2++;
						ptr4++;
						ptr6++;
					}
					while (ptr2 != ptr7)
					{
						ulong num3 = num + *ptr2;
						num = num3 >> 32;
						*ptr6 = (uint)(num3 & 0xFFFFFFFFu);
						ptr2++;
						ptr6++;
					}
					if (num != 0L)
					{
						*ptr6 = 1u;
						pResult.m_length = length + 1;
					}
					else
					{
						pResult.m_length = length;
					}
				}
			}
		}
	}

	private static void BigInt_Multiply(out tBigInt pResult, in tBigInt lhs, in tBigInt rhs)
	{
		if (lhs.m_length < rhs.m_length)
		{
			BigInt_Multiply_internal(out pResult, in rhs, in lhs);
		}
		else
		{
			BigInt_Multiply_internal(out pResult, in lhs, in rhs);
		}
	}

	private unsafe static void BigInt_Multiply_internal(out tBigInt pResult, in tBigInt pLarge, in tBigInt pSmall)
	{
		int num = pLarge.m_length + pSmall.m_length;
		for (int i = 0; i < num; i++)
		{
			pResult.m_blocks[i] = 0u;
		}
		fixed (uint* ptr = pLarge.m_blocks)
		{
			uint* ptr2 = ptr + pLarge.m_length;
			fixed (uint* ptr6 = pResult.m_blocks)
			{
				fixed (uint* ptr3 = pSmall.m_blocks)
				{
					uint* ptr4 = ptr3;
					uint* ptr5 = ptr4 + pSmall.m_length;
					uint* ptr7 = ptr6;
					while (ptr4 != ptr5)
					{
						uint num2 = *ptr4;
						if (num2 != 0)
						{
							uint* ptr8 = ptr;
							uint* ptr9 = ptr7;
							ulong num3 = 0uL;
							do
							{
								ulong num4 = (ulong)(*ptr9 + (long)(*ptr8) * (long)num2) + num3;
								num3 = num4 >> 32;
								*ptr9 = (uint)(num4 & 0xFFFFFFFFu);
								ptr8++;
								ptr9++;
							}
							while (ptr8 != ptr2);
							*ptr9 = (uint)(num3 & 0xFFFFFFFFu);
						}
						ptr4++;
						ptr7++;
					}
					if (num > 0 && pResult.m_blocks[num - 1] == 0)
					{
						pResult.m_length = num - 1;
					}
					else
					{
						pResult.m_length = num;
					}
				}
			}
		}
	}

	private unsafe static void BigInt_Multiply(out tBigInt pResult, in tBigInt lhs, uint rhs)
	{
		uint num = 0u;
		fixed (uint* ptr = pResult.m_blocks)
		{
			fixed (uint* ptr3 = lhs.m_blocks)
			{
				uint* ptr2 = ptr;
				uint* ptr4 = ptr3;
				uint* ptr5 = ptr4 + lhs.m_length;
				while (ptr4 != ptr5)
				{
					ulong num2 = (ulong)((long)(*ptr4) * (long)rhs + num);
					*ptr2 = (uint)(num2 & 0xFFFFFFFFu);
					num = (uint)(num2 >> 32);
					ptr4++;
					ptr2++;
				}
				if (num != 0)
				{
					*ptr2 = num;
					pResult.m_length = lhs.m_length + 1;
				}
				else
				{
					pResult.m_length = lhs.m_length;
				}
			}
		}
	}

	private unsafe static void BigInt_Multiply2(out tBigInt pResult, in tBigInt input)
	{
		uint num = 0u;
		fixed (uint* ptr = pResult.m_blocks)
		{
			fixed (uint* ptr3 = input.m_blocks)
			{
				uint* ptr2 = ptr;
				uint* ptr4 = ptr3;
				uint* ptr5 = ptr4 + input.m_length;
				while (ptr4 != ptr5)
				{
					uint num2 = *ptr4;
					*ptr2 = (num2 << 1) | num;
					num = num2 >> 31;
					ptr4++;
					ptr2++;
				}
				if (num != 0)
				{
					*ptr2 = num;
					pResult.m_length = input.m_length + 1;
				}
				else
				{
					pResult.m_length = input.m_length;
				}
			}
		}
	}

	private unsafe static void BigInt_Multiply2(ref tBigInt pResult)
	{
		uint num = 0u;
		fixed (uint* ptr = pResult.m_blocks)
		{
			uint* ptr2 = ptr;
			for (uint* ptr3 = ptr2 + pResult.m_length; ptr2 != ptr3; ptr2++)
			{
				uint num2 = *ptr2;
				*ptr2 = (num2 << 1) | num;
				num = num2 >> 31;
			}
			if (num != 0)
			{
				*ptr2 = num;
				pResult.m_length++;
			}
		}
	}

	private unsafe static void BigInt_Multiply10(ref tBigInt pResult)
	{
		ulong num = 0uL;
		fixed (uint* ptr = pResult.m_blocks)
		{
			uint* ptr2 = ptr;
			for (uint* ptr3 = ptr2 + pResult.m_length; ptr2 != ptr3; ptr2++)
			{
				ulong num2 = (ulong)((long)(*ptr2) * 10L) + num;
				*ptr2 = (uint)(num2 & 0xFFFFFFFFu);
				num = num2 >> 32;
			}
			if (num != 0L)
			{
				*ptr2 = (uint)num;
				pResult.m_length++;
			}
		}
	}

	private unsafe static tBigInt g_PowerOf10_Big(int i)
	{
		tBigInt result = default(tBigInt);
		switch (i)
		{
		case 0:
			result.m_length = 1;
			result.m_blocks[0] = 100000000u;
			break;
		case 1:
			result.m_length = 2;
			result.m_blocks[0] = 1874919424u;
			result.m_blocks[1] = 2328306u;
			break;
		case 2:
			result.m_length = 4;
			result.m_blocks[0] = 0u;
			result.m_blocks[1] = 2242703233u;
			result.m_blocks[2] = 762134875u;
			result.m_blocks[3] = 1262u;
			break;
		case 3:
			result.m_length = 7;
			result.m_blocks[0] = 0u;
			result.m_blocks[1] = 0u;
			result.m_blocks[2] = 3211403009u;
			result.m_blocks[3] = 1849224548u;
			result.m_blocks[4] = 3668416493u;
			result.m_blocks[5] = 3913284084u;
			result.m_blocks[6] = 1593091u;
			break;
		case 4:
			result.m_length = 14;
			result.m_blocks[0] = 0u;
			result.m_blocks[1] = 0u;
			result.m_blocks[2] = 0u;
			result.m_blocks[3] = 0u;
			result.m_blocks[4] = 781532673u;
			result.m_blocks[5] = 64985353u;
			result.m_blocks[6] = 253049085u;
			result.m_blocks[7] = 594863151u;
			result.m_blocks[8] = 3553621484u;
			result.m_blocks[9] = 3288652808u;
			result.m_blocks[10] = 3167596762u;
			result.m_blocks[11] = 2788392729u;
			result.m_blocks[12] = 3911132675u;
			result.m_blocks[13] = 590u;
			break;
		default:
			result.m_length = 27;
			result.m_blocks[0] = 0u;
			result.m_blocks[1] = 0u;
			result.m_blocks[2] = 0u;
			result.m_blocks[3] = 0u;
			result.m_blocks[4] = 0u;
			result.m_blocks[5] = 0u;
			result.m_blocks[6] = 0u;
			result.m_blocks[7] = 0u;
			result.m_blocks[8] = 2553183233u;
			result.m_blocks[9] = 3201533787u;
			result.m_blocks[10] = 3638140786u;
			result.m_blocks[11] = 303378311u;
			result.m_blocks[12] = 1809731782u;
			result.m_blocks[13] = 3477761648u;
			result.m_blocks[14] = 3583367183u;
			result.m_blocks[15] = 649228654u;
			result.m_blocks[16] = 2915460784u;
			result.m_blocks[17] = 487929380u;
			result.m_blocks[18] = 1011012442u;
			result.m_blocks[19] = 1677677582u;
			result.m_blocks[20] = 3428152256u;
			result.m_blocks[21] = 1710878487u;
			result.m_blocks[22] = 1438394610u;
			result.m_blocks[23] = 2161952759u;
			result.m_blocks[24] = 4100910556u;
			result.m_blocks[25] = 1608314830u;
			result.m_blocks[26] = 349175u;
			break;
		}
		return result;
	}

	private static void BigInt_Pow10(out tBigInt pResult, uint exponent)
	{
		tBigInt lhs = default(tBigInt);
		tBigInt pResult2 = default(tBigInt);
		uint num = exponent & 7u;
		lhs.SetU32(g_PowerOf10_U32[num]);
		exponent >>= 3;
		int num2 = 0;
		while (exponent != 0)
		{
			if ((exponent & (true ? 1u : 0u)) != 0)
			{
				tBigInt rhs = g_PowerOf10_Big(num2);
				BigInt_Multiply(out pResult2, in lhs, in rhs);
				lhs = pResult2;
				pResult2 = lhs;
			}
			num2++;
			exponent >>= 1;
		}
		pResult = lhs;
	}

	private static void BigInt_MultiplyPow10(out tBigInt pResult, in tBigInt input, uint exponent)
	{
		tBigInt pResult2 = default(tBigInt);
		tBigInt pResult3 = default(tBigInt);
		uint num = exponent & 7u;
		if (num != 0)
		{
			BigInt_Multiply(out pResult2, in input, g_PowerOf10_U32[num]);
		}
		else
		{
			pResult2 = input;
		}
		exponent >>= 3;
		int num2 = 0;
		while (exponent != 0)
		{
			if ((exponent & (true ? 1u : 0u)) != 0)
			{
				tBigInt rhs = g_PowerOf10_Big(num2);
				BigInt_Multiply(out pResult3, in pResult2, in rhs);
				pResult2 = pResult3;
				pResult3 = pResult2;
			}
			num2++;
			exponent >>= 1;
		}
		pResult = pResult2;
	}

	private unsafe static void BigInt_Pow2(out tBigInt pResult, uint exponent)
	{
		int num = (int)exponent / 32;
		for (uint num2 = 0u; num2 <= num; num2++)
		{
			pResult.m_blocks[num2] = 0u;
		}
		pResult.m_length = num + 1;
		int num3 = (int)exponent % 32;
		ref uint reference = ref pResult.m_blocks[num];
		reference |= (uint)(1 << num3);
	}

	private unsafe static uint BigInt_DivideWithRemainder_MaxQuotient9(ref tBigInt pDividend, in tBigInt divisor)
	{
		int num = divisor.m_length;
		if (pDividend.m_length < divisor.m_length)
		{
			return 0u;
		}
		fixed (uint* ptr = divisor.m_blocks)
		{
			fixed (uint* ptr3 = pDividend.m_blocks)
			{
				uint* ptr2 = ptr;
				uint* ptr4 = ptr3;
				uint* ptr5 = ptr2 + num - 1;
				uint num2 = *(ptr4 + num - 1) / (*ptr5 + 1);
				if (num2 != 0)
				{
					ulong num3 = 0uL;
					ulong num4 = 0uL;
					do
					{
						ulong num5 = (ulong)((long)(*ptr2) * (long)num2) + num4;
						num4 = num5 >> 32;
						ulong num6 = *ptr4 - (num5 & 0xFFFFFFFFu) - num3;
						num3 = (num6 >> 32) & 1;
						*ptr4 = (uint)(num6 & 0xFFFFFFFFu);
						ptr2++;
						ptr4++;
					}
					while (ptr2 <= ptr5);
					while (num > 0 && pDividend.m_blocks[num - 1] == 0)
					{
						num--;
					}
					pDividend.m_length = num;
				}
				if (BigInt_Compare(in pDividend, in divisor) >= 0)
				{
					num2++;
					ptr2 = ptr;
					ptr4 = ptr3;
					ulong num7 = 0uL;
					do
					{
						ulong num8 = (ulong)((long)(*ptr4) - (long)(*ptr2)) - num7;
						num7 = (num8 >> 32) & 1;
						*ptr4 = (uint)(num8 & 0xFFFFFFFFu);
						ptr2++;
						ptr4++;
					}
					while (ptr2 <= ptr5);
					while (num > 0 && pDividend.m_blocks[num - 1] == 0)
					{
						num--;
					}
					pDividend.m_length = num;
				}
				return num2;
			}
		}
	}

	private unsafe static void BigInt_ShiftLeft(ref tBigInt pResult, uint shift)
	{
		int num = (int)shift / 32;
		int num2 = (int)shift % 32;
		int length = pResult.m_length;
		if (num2 == 0)
		{
			fixed (uint* ptr = pResult.m_blocks)
			{
				uint* ptr2 = ptr + length - 1;
				uint* ptr3 = ptr2 + num;
				while (ptr2 >= ptr)
				{
					*ptr3 = *ptr2;
					ptr2--;
					ptr3--;
				}
			}
			for (uint num3 = 0u; num3 < num; num3++)
			{
				pResult.m_blocks[num3] = 0u;
			}
			pResult.m_length += num;
			return;
		}
		int num4 = length - 1;
		int num5 = length + num;
		pResult.m_length = num5 + 1;
		int num6 = 32 - num2;
		uint num7 = 0u;
		uint num8 = pResult.m_blocks[num4];
		uint num9 = num8 >> num6;
		while (num4 > 0)
		{
			pResult.m_blocks[num5] = num7 | num9;
			num7 = num8 << num2;
			num4--;
			num5--;
			num8 = pResult.m_blocks[num4];
			num9 = num8 >> num6;
		}
		pResult.m_blocks[num5] = num7 | num9;
		pResult.m_blocks[num5 - 1] = num8 << num2;
		for (uint num10 = 0u; num10 < num; num10++)
		{
			pResult.m_blocks[num10] = 0u;
		}
		if (pResult.m_blocks[pResult.m_length - 1] == 0)
		{
			pResult.m_length--;
		}
	}

	private unsafe static uint Dragon4(ulong mantissa, int exponent, uint mantissaHighBitIdx, bool hasUnequalMargins, CutoffMode cutoffMode, uint cutoffNumber, byte* pOutBuffer, uint bufferSize, out int pOutExponent)
	{
		byte* ptr = pOutBuffer;
		if (mantissa == 0L)
		{
			*ptr = 48;
			pOutExponent = 0;
			return 1u;
		}
		tBigInt pResult = default(tBigInt);
		tBigInt pResult2 = default(tBigInt);
		tBigInt input = default(tBigInt);
		tBigInt pResult3 = default(tBigInt);
		tBigInt* ptr2;
		if (hasUnequalMargins)
		{
			if (exponent > 0)
			{
				pResult2.SetU64(4 * mantissa);
				BigInt_ShiftLeft(ref pResult2, (uint)exponent);
				pResult.SetU32(4u);
				BigInt_Pow2(out input, (uint)exponent);
				BigInt_Pow2(out pResult3, (uint)(exponent + 1));
			}
			else
			{
				pResult2.SetU64(4 * mantissa);
				BigInt_Pow2(out pResult, (uint)(-exponent + 2));
				input.SetU32(1u);
				pResult3.SetU32(2u);
			}
			ptr2 = &pResult3;
		}
		else
		{
			if (exponent > 0)
			{
				pResult2.SetU64(2 * mantissa);
				BigInt_ShiftLeft(ref pResult2, (uint)exponent);
				pResult.SetU32(2u);
				BigInt_Pow2(out input, (uint)exponent);
			}
			else
			{
				pResult2.SetU64(2 * mantissa);
				BigInt_Pow2(out pResult, (uint)(-exponent + 1));
				input.SetU32(1u);
			}
			ptr2 = &input;
		}
		int num = (int)Math.Ceiling((double)((int)mantissaHighBitIdx + exponent) * 0.3010299956639812 - 0.69);
		if (cutoffMode == CutoffMode.FractionLength && num <= (int)(0 - cutoffNumber))
		{
			num = (int)(0 - cutoffNumber + 1);
		}
		if (num > 0)
		{
			BigInt_MultiplyPow10(out var pResult4, in pResult, (uint)num);
			pResult = pResult4;
		}
		else if (num < 0)
		{
			BigInt_Pow10(out var pResult5, (uint)(-num));
			BigInt_Multiply(out var pResult6, in pResult2, in pResult5);
			pResult2 = pResult6;
			BigInt_Multiply(out pResult6, in input, in pResult5);
			input = pResult6;
			if (ptr2 != &input)
			{
				BigInt_Multiply2(out *ptr2, in input);
			}
		}
		if (BigInt_Compare(in pResult2, in pResult) >= 0)
		{
			num++;
		}
		else
		{
			BigInt_Multiply10(ref pResult2);
			BigInt_Multiply10(ref input);
			if (ptr2 != &input)
			{
				BigInt_Multiply2(out *ptr2, in input);
			}
		}
		int num2 = num - (int)bufferSize;
		switch (cutoffMode)
		{
		case CutoffMode.TotalLength:
		{
			int num4 = num - (int)cutoffNumber;
			if (num4 > num2)
			{
				num2 = num4;
			}
			break;
		}
		case CutoffMode.FractionLength:
		{
			int num3 = (int)(0 - cutoffNumber);
			if (num3 > num2)
			{
				num2 = num3;
			}
			break;
		}
		}
		pOutExponent = num - 1;
		uint block = pResult.GetBlock(pResult.GetLength() - 1);
		if (block < 8 || block > 429496729)
		{
			uint num5 = LogBase2(block);
			uint shift = (59 - num5) % 32;
			BigInt_ShiftLeft(ref pResult, shift);
			BigInt_ShiftLeft(ref pResult2, shift);
			BigInt_ShiftLeft(ref input, shift);
			if (ptr2 != &input)
			{
				BigInt_Multiply2(out *ptr2, in input);
			}
		}
		uint num6;
		bool flag;
		bool flag2;
		if (cutoffMode == CutoffMode.Unique)
		{
			while (true)
			{
				num--;
				num6 = BigInt_DivideWithRemainder_MaxQuotient9(ref pResult2, in pResult);
				BigInt_Add(out var pResult7, in pResult2, in *ptr2);
				flag = BigInt_Compare(in pResult2, in input) < 0;
				flag2 = BigInt_Compare(in pResult7, in pResult) > 0;
				if (flag || flag2 || num == num2)
				{
					break;
				}
				*ptr = (byte)(48 + num6);
				ptr++;
				BigInt_Multiply10(ref pResult2);
				BigInt_Multiply10(ref input);
				if (ptr2 != &input)
				{
					BigInt_Multiply2(out *ptr2, in input);
				}
			}
		}
		else
		{
			flag = false;
			flag2 = false;
			while (true)
			{
				num--;
				num6 = BigInt_DivideWithRemainder_MaxQuotient9(ref pResult2, in pResult);
				if (pResult2.IsZero() || num == num2)
				{
					break;
				}
				*ptr = (byte)(48 + num6);
				ptr++;
				BigInt_Multiply10(ref pResult2);
			}
		}
		bool flag3 = flag;
		if (flag == flag2)
		{
			BigInt_Multiply2(ref pResult2);
			int num7 = BigInt_Compare(in pResult2, in pResult);
			flag3 = num7 < 0;
			if (num7 == 0)
			{
				flag3 = (num6 & 1) == 0;
			}
		}
		if (flag3)
		{
			*ptr = (byte)(48 + num6);
			ptr++;
		}
		else if (num6 == 9)
		{
			while (true)
			{
				if (ptr == pOutBuffer)
				{
					*ptr = 49;
					ptr++;
					pOutExponent++;
					break;
				}
				ptr--;
				if (*ptr != 57)
				{
					byte* intPtr = ptr;
					(*intPtr)++;
					ptr++;
					break;
				}
			}
		}
		else
		{
			*ptr = (byte)(48 + num6 + 1);
			ptr++;
		}
		return (uint)(ptr - pOutBuffer);
	}

	private unsafe static int FormatPositional(byte* pOutBuffer, uint bufferSize, ulong mantissa, int exponent, uint mantissaHighBitIdx, bool hasUnequalMargins, int precision)
	{
		uint num = bufferSize - 1;
		int pOutExponent;
		uint num2 = ((precision >= 0) ? Dragon4(mantissa, exponent, mantissaHighBitIdx, hasUnequalMargins, CutoffMode.FractionLength, (uint)precision, pOutBuffer, num, out pOutExponent) : Dragon4(mantissa, exponent, mantissaHighBitIdx, hasUnequalMargins, CutoffMode.Unique, 0u, pOutBuffer, num, out pOutExponent));
		uint num3 = 0u;
		if (pOutExponent >= 0)
		{
			uint num4 = (uint)(pOutExponent + 1);
			if (num2 < num4)
			{
				if (num4 > num)
				{
					num4 = num;
				}
				for (; num2 < num4; num2++)
				{
					pOutBuffer[num2] = 48;
				}
			}
			else if (num2 > num4)
			{
				num3 = num2 - num4;
				uint num5 = num - num4 - 1;
				if (num3 > num5)
				{
					num3 = num5;
				}
				Unsafe.CopyBlock(pOutBuffer + num4 + 1, pOutBuffer + num4, num3);
				pOutBuffer[num4] = 46;
				num2 = num4 + 1 + num3;
			}
		}
		else
		{
			if (num > 2)
			{
				uint num6 = (uint)(-pOutExponent - 1);
				uint num7 = num - 2;
				if (num6 > num7)
				{
					num6 = num7;
				}
				uint num8 = 2 + num6;
				num3 = num2;
				uint num9 = num - num8;
				if (num3 > num9)
				{
					num3 = num9;
				}
				Unsafe.CopyBlock(pOutBuffer + num8, pOutBuffer, num3);
				for (uint num10 = 2u; num10 < num8; num10++)
				{
					pOutBuffer[num10] = 48;
				}
				num3 += num6;
				num2 = num3;
			}
			if (num > 1)
			{
				pOutBuffer[1] = 46;
				num2++;
			}
			if (num != 0)
			{
				*pOutBuffer = 48;
				num2++;
			}
		}
		if (precision > (int)num3 && num2 < num)
		{
			if (num3 == 0)
			{
				pOutBuffer[num2++] = 46;
			}
			uint num11 = (uint)(num2 + (precision - (int)num3));
			if (num11 > num)
			{
				num11 = num;
			}
			for (; num2 < num11; num2++)
			{
				pOutBuffer[num2] = 48;
			}
		}
		return (int)num2;
	}

	private unsafe static int FormatScientific(byte* pOutBuffer, uint bufferSize, ulong mantissa, int exponent, uint mantissaHighBitIdx, bool hasUnequalMargins, int precision)
	{
		int pOutExponent;
		uint num = ((precision >= 0) ? Dragon4(mantissa, exponent, mantissaHighBitIdx, hasUnequalMargins, CutoffMode.TotalLength, (uint)(precision + 1), pOutBuffer, bufferSize, out pOutExponent) : Dragon4(mantissa, exponent, mantissaHighBitIdx, hasUnequalMargins, CutoffMode.Unique, 0u, pOutBuffer, bufferSize, out pOutExponent));
		byte* ptr = pOutBuffer;
		if (bufferSize > 1)
		{
			ptr++;
			bufferSize--;
		}
		uint num2 = num - 1;
		if (num2 != 0 && bufferSize > 1)
		{
			uint num3 = bufferSize - 2;
			if (num2 > num3)
			{
				num2 = num3;
			}
			Unsafe.CopyBlock(ptr + 1, ptr, num2);
			*ptr = 46;
			ptr += 1 + num2;
			bufferSize -= 1 + num2;
		}
		if (precision > (int)num2 && bufferSize > 1)
		{
			if (num2 == 0)
			{
				*ptr = 46;
				ptr++;
				bufferSize--;
			}
			uint num4 = (uint)(precision - num2);
			if (num4 > bufferSize - 1)
			{
				num4 = bufferSize - 1;
			}
			for (byte* ptr2 = ptr + num4; ptr < ptr2; ptr++)
			{
				*ptr = 48;
			}
		}
		if (bufferSize > 1)
		{
			byte* ptr3 = stackalloc byte[5];
			*ptr3 = 101;
			if (pOutExponent >= 0)
			{
				ptr3[1] = 43;
			}
			else
			{
				ptr3[1] = 45;
				pOutExponent = -pOutExponent;
			}
			uint num5 = (uint)(pOutExponent / 100);
			uint num6 = (uint)((pOutExponent - num5 * 100) / 10);
			uint num7 = (uint)(pOutExponent - num5 * 100 - num6 * 10);
			ptr3[2] = (byte)(48 + num5);
			ptr3[3] = (byte)(48 + num6);
			ptr3[4] = (byte)(48 + num7);
			uint num8 = bufferSize - 1;
			uint num9 = ((5 < num8) ? 5u : num8);
			Unsafe.CopyBlock(ptr, ptr3, num9);
			ptr += num9;
			bufferSize -= num9;
		}
		return (int)(ptr - pOutBuffer);
	}

	private unsafe static void FormatInfinityNaN(byte* dest, ref int destIndex, int destLength, ulong mantissa, bool isNegative, FormatOptions formatOptions)
	{
		int length = ((mantissa == 0L) ? (8 + (isNegative ? 1 : 0)) : 3);
		int alignAndSize = formatOptions.AlignAndSize;
		if (AlignLeft(dest, ref destIndex, destLength, alignAndSize, length))
		{
			return;
		}
		if (mantissa == 0L)
		{
			if (isNegative)
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = 45;
			}
			for (int i = 0; i < 8; i++)
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = InfinityString[i];
			}
		}
		else
		{
			for (int j = 0; j < 3; j++)
			{
				if (destIndex >= destLength)
				{
					return;
				}
				dest[destIndex++] = NanString[j];
			}
		}
		AlignRight(dest, ref destIndex, destLength, alignAndSize, length);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static void ConvertFloatToString(byte* dest, ref int destIndex, int destLength, float value, FormatOptions formatOptions)
	{
		tFloatUnion32 tFloatUnion = default(tFloatUnion32);
		tFloatUnion.m_floatingPoint = value;
		uint exponent = tFloatUnion.GetExponent();
		uint mantissa = tFloatUnion.GetMantissa();
		uint num;
		int exponent2;
		uint mantissaHighBitIdx;
		bool hasUnequalMargins;
		switch (exponent)
		{
		case 255u:
			FormatInfinityNaN(dest, ref destIndex, destLength, mantissa, tFloatUnion.IsNegative(), formatOptions);
			return;
		default:
			num = (uint)(0x800000uL | (ulong)mantissa);
			exponent2 = (int)(exponent - 127 - 23);
			mantissaHighBitIdx = 23u;
			hasUnequalMargins = exponent != 1 && mantissa == 0;
			break;
		case 0u:
			num = mantissa;
			exponent2 = -149;
			mantissaHighBitIdx = LogBase2(num);
			hasUnequalMargins = false;
			break;
		}
		int num2 = ((formatOptions.Specifier == 0) ? (-1) : formatOptions.Specifier);
		int num3 = Math.Max(10, num2 + 1);
		byte* ptr = stackalloc byte[(int)(uint)num3];
		if (num2 < 0)
		{
			num2 = 7;
		}
		int pOutExponent;
		uint num4 = Dragon4(num, exponent2, mantissaHighBitIdx, hasUnequalMargins, CutoffMode.TotalLength, (uint)num2, ptr, (uint)(num3 - 1), out pOutExponent);
		ptr[num4] = 0;
		bool isNegative = tFloatUnion.IsNegative();
		if (tFloatUnion.m_integer == 2147483648u)
		{
			isNegative = false;
		}
		NumberBuffer number = new NumberBuffer(NumberBufferKind.Float, ptr, (int)num4, pOutExponent + 1, isNegative);
		FormatNumber(dest, ref destIndex, destLength, ref number, num2, formatOptions);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static void ConvertDoubleToString(byte* dest, ref int destIndex, int destLength, double value, FormatOptions formatOptions)
	{
		tFloatUnion64 tFloatUnion = default(tFloatUnion64);
		tFloatUnion.m_floatingPoint = value;
		uint exponent = tFloatUnion.GetExponent();
		ulong mantissa = tFloatUnion.GetMantissa();
		ulong num;
		int exponent2;
		uint mantissaHighBitIdx;
		bool hasUnequalMargins;
		switch (exponent)
		{
		case 2047u:
			FormatInfinityNaN(dest, ref destIndex, destLength, mantissa, tFloatUnion.IsNegative(), formatOptions);
			return;
		default:
			num = 0x10000000000000uL | mantissa;
			exponent2 = (int)(exponent - 1023 - 52);
			mantissaHighBitIdx = 52u;
			hasUnequalMargins = exponent != 1 && mantissa == 0;
			break;
		case 0u:
			num = mantissa;
			exponent2 = -1074;
			mantissaHighBitIdx = LogBase2((uint)num);
			hasUnequalMargins = false;
			break;
		}
		int num2 = ((formatOptions.Specifier == 0) ? (-1) : formatOptions.Specifier);
		int num3 = Math.Max(18, num2 + 1);
		byte* ptr = stackalloc byte[(int)(uint)num3];
		if (num2 < 0)
		{
			num2 = 15;
		}
		int pOutExponent;
		uint num4 = Dragon4(num, exponent2, mantissaHighBitIdx, hasUnequalMargins, CutoffMode.TotalLength, (uint)num2, ptr, (uint)(num3 - 1), out pOutExponent);
		ptr[num4] = 0;
		bool isNegative = tFloatUnion.IsNegative();
		if (tFloatUnion.m_integer == 9223372036854775808uL)
		{
			isNegative = false;
		}
		NumberBuffer number = new NumberBuffer(NumberBufferKind.Float, ptr, (int)num4, pOutExponent + 1, isNegative);
		FormatNumber(dest, ref destIndex, destLength, ref number, num2, formatOptions);
	}
}
