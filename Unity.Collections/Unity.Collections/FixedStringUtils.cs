using System.Runtime.InteropServices;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
internal static class FixedStringUtils
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct UintFloatUnion
	{
		[FieldOffset(0)]
		public uint uintValue;

		[FieldOffset(0)]
		public float floatValue;
	}

	internal static ParseError Base10ToBase2(ref float output, ulong mantissa10, int exponent10)
	{
		if (mantissa10 == 0L)
		{
			output = 0f;
			return ParseError.None;
		}
		if (exponent10 == 0)
		{
			output = mantissa10;
			return ParseError.None;
		}
		int num = exponent10;
		ulong num2 = mantissa10;
		while (exponent10 > 0)
		{
			while ((num2 & 0xE000000000000000uL) != 0L)
			{
				num2 >>= 1;
				num++;
			}
			num2 *= 5;
			exponent10--;
		}
		while (exponent10 < 0)
		{
			while ((num2 & 0x8000000000000000uL) == 0L)
			{
				num2 <<= 1;
				num--;
			}
			num2 /= 5;
			exponent10++;
		}
		UintFloatUnion uintFloatUnion = default(UintFloatUnion);
		uintFloatUnion.floatValue = num2;
		int num3 = (int)(((uintFloatUnion.uintValue >> 23) & 0xFF) - 127);
		num3 += num;
		if (num3 > 128)
		{
			return ParseError.Overflow;
		}
		if (num3 < -127)
		{
			return ParseError.Underflow;
		}
		uintFloatUnion.uintValue = (uintFloatUnion.uintValue & 0x807FFFFFu) | (uint)(num3 + 127 << 23);
		output = uintFloatUnion.floatValue;
		return ParseError.None;
	}

	internal static void Base2ToBase10(ref ulong mantissa10, ref int exponent10, float input)
	{
		UintFloatUnion uintFloatUnion = default(UintFloatUnion);
		uintFloatUnion.floatValue = input;
		if (uintFloatUnion.uintValue == 0)
		{
			mantissa10 = 0uL;
			exponent10 = 0;
			return;
		}
		uint num = (uintFloatUnion.uintValue & 0x7FFFFFu) | 0x800000u;
		int i = (int)((uintFloatUnion.uintValue >> 23) - 127 - 23);
		mantissa10 = num;
		exponent10 = i;
		if (i > 0)
		{
			while (i > 0)
			{
				while (mantissa10 <= 1844674407370955161L)
				{
					mantissa10 *= 10uL;
					exponent10--;
				}
				mantissa10 /= 5uL;
				i--;
			}
		}
		if (i < 0)
		{
			for (; i < 0; i++)
			{
				while (mantissa10 > 3689348814741910323L)
				{
					mantissa10 /= 10uL;
					exponent10++;
				}
				mantissa10 *= 5uL;
			}
		}
		while (mantissa10 > 9999999 || mantissa10 % 10 == 0L)
		{
			mantissa10 = (mantissa10 + (uint)((mantissa10 < 100000000) ? 5 : 0)) / 10;
			exponent10++;
		}
	}
}
