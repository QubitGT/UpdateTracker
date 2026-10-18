using System.Runtime.InteropServices;

namespace System;

internal struct GuidEx
{
	private int _a;

	private short _b;

	private short _c;

	private byte _d;

	private byte _e;

	private byte _f;

	private byte _g;

	private byte _h;

	private byte _i;

	private byte _j;

	private byte _k;

	private unsafe static int HexsToChars(char* guidChars, int a, int b)
	{
		*guidChars = HexConverter.ToCharLower(a >> 4);
		guidChars[1] = HexConverter.ToCharLower(a);
		guidChars[2] = HexConverter.ToCharLower(b >> 4);
		guidChars[3] = HexConverter.ToCharLower(b);
		return 4;
	}

	private unsafe static int HexsToCharsHexOutput(char* guidChars, int a, int b)
	{
		*guidChars = '0';
		guidChars[1] = 'x';
		guidChars[2] = HexConverter.ToCharLower(a >> 4);
		guidChars[3] = HexConverter.ToCharLower(a);
		guidChars[4] = ',';
		guidChars[5] = '0';
		guidChars[6] = 'x';
		guidChars[7] = HexConverter.ToCharLower(b >> 4);
		guidChars[8] = HexConverter.ToCharLower(b);
		return 9;
	}

	public unsafe bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default(ReadOnlySpan<char>))
	{
		if (format.Length == 0)
		{
			format = "D".AsSpan();
		}
		if (format.Length != 1)
		{
			throw new FormatException("InvalidGuidFormatSpecification");
		}
		bool flag = true;
		bool flag2 = false;
		int num = 0;
		int num2;
		switch (format[0])
		{
		case 'D':
		case 'd':
			num2 = 36;
			break;
		case 'N':
		case 'n':
			flag = false;
			num2 = 32;
			break;
		case 'B':
		case 'b':
			num = 8192123;
			num2 = 38;
			break;
		case 'P':
		case 'p':
			num = 2687016;
			num2 = 38;
			break;
		case 'X':
		case 'x':
			num = 8192123;
			flag = false;
			flag2 = true;
			num2 = 68;
			break;
		default:
			throw new FormatException("InvalidGuidFormatSpecification");
		}
		if (destination.Length < num2)
		{
			charsWritten = 0;
			return false;
		}
		fixed (char* ptr = &MemoryMarshal.GetReference(destination))
		{
			char* ptr2 = ptr;
			if (num != 0)
			{
				*(ptr2++) = (char)num;
			}
			if (flag2)
			{
				*(ptr2++) = '0';
				*(ptr2++) = 'x';
				ptr2 += HexsToChars(ptr2, _a >> 24, _a >> 16);
				ptr2 += HexsToChars(ptr2, _a >> 8, _a);
				*(ptr2++) = ',';
				*(ptr2++) = '0';
				*(ptr2++) = 'x';
				ptr2 += HexsToChars(ptr2, _b >> 8, _b);
				*(ptr2++) = ',';
				*(ptr2++) = '0';
				*(ptr2++) = 'x';
				ptr2 += HexsToChars(ptr2, _c >> 8, _c);
				*(ptr2++) = ',';
				*(ptr2++) = '{';
				ptr2 += HexsToCharsHexOutput(ptr2, _d, _e);
				*(ptr2++) = ',';
				ptr2 += HexsToCharsHexOutput(ptr2, _f, _g);
				*(ptr2++) = ',';
				ptr2 += HexsToCharsHexOutput(ptr2, _h, _i);
				*(ptr2++) = ',';
				ptr2 += HexsToCharsHexOutput(ptr2, _j, _k);
				*(ptr2++) = '}';
			}
			else
			{
				ptr2 += HexsToChars(ptr2, _a >> 24, _a >> 16);
				ptr2 += HexsToChars(ptr2, _a >> 8, _a);
				if (flag)
				{
					*(ptr2++) = '-';
				}
				ptr2 += HexsToChars(ptr2, _b >> 8, _b);
				if (flag)
				{
					*(ptr2++) = '-';
				}
				ptr2 += HexsToChars(ptr2, _c >> 8, _c);
				if (flag)
				{
					*(ptr2++) = '-';
				}
				ptr2 += HexsToChars(ptr2, _d, _e);
				if (flag)
				{
					*(ptr2++) = '-';
				}
				ptr2 += HexsToChars(ptr2, _f, _g);
				ptr2 += HexsToChars(ptr2, _h, _i);
				ptr2 += HexsToChars(ptr2, _j, _k);
			}
			if (num != 0)
			{
				*(ptr2++) = (char)(num >> 16);
			}
		}
		charsWritten = num2;
		return true;
	}
}
