using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Fusion;

public static class BinUtils
{
	private static readonly string[] _byteHexValue = new string[256]
	{
		"00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
		"0A", "0B", "0C", "0D", "0E", "0F", "10", "11", "12", "13",
		"14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D",
		"1E", "1F", "20", "21", "22", "23", "24", "25", "26", "27",
		"28", "29", "2A", "2B", "2C", "2D", "2E", "2F", "30", "31",
		"32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B",
		"3C", "3D", "3E", "3F", "40", "41", "42", "43", "44", "45",
		"46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
		"50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
		"5A", "5B", "5C", "5D", "5E", "5F", "60", "61", "62", "63",
		"64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D",
		"6E", "6F", "70", "71", "72", "73", "74", "75", "76", "77",
		"78", "79", "7A", "7B", "7C", "7D", "7E", "7F", "80", "81",
		"82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B",
		"8C", "8D", "8E", "8F", "90", "91", "92", "93", "94", "95",
		"96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
		"A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9",
		"AA", "AB", "AC", "AD", "AE", "AF", "B0", "B1", "B2", "B3",
		"B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD",
		"BE", "BF", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7",
		"C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF", "D0", "D1",
		"D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB",
		"DC", "DD", "DE", "DF", "E0", "E1", "E2", "E3", "E4", "E5",
		"E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
		"F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9",
		"FA", "FB", "FC", "FD", "FE", "FF"
	};

	public static string ByteToHex(byte value)
	{
		return _byteHexValue[value];
	}

	public unsafe static string BytesToHex(byte* buffer, int length, int columns = 16, string rowSeparator = "\n", string columnSeparator = " ")
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (num < length)
		{
			stringBuilder.Append(_byteHexValue[buffer[num++]]);
			if (num == length)
			{
				break;
			}
			if (num % columns == 0)
			{
				stringBuilder.Append(rowSeparator);
			}
			else
			{
				stringBuilder.Append(columnSeparator);
			}
		}
		return stringBuilder.ToString();
	}

	public unsafe static string WordsToHex(int* buffer, int length, int columns = 4, string rowSeparator = "\n", string columnSeparator = " ")
	{
		return WordsToHex(new ReadOnlySpan<uint>(buffer, length), columns, rowSeparator, columnSeparator);
	}

	public unsafe static string WordsToHex(uint* buffer, int length, int columns = 4, string rowSeparator = "\n", string columnSeparator = " ")
	{
		return WordsToHex(new ReadOnlySpan<uint>(buffer, length), columns, rowSeparator, columnSeparator);
	}

	public static string WordsToHex(ReadOnlySpan<int> buffer, int columns = 4, string rowSeparator = "\n", string columnSeparator = " ")
	{
		return WordsToHex(MemoryMarshal.Cast<int, uint>(buffer), columns, rowSeparator, columnSeparator);
	}

	public static string WordsToHex(ReadOnlySpan<uint> buffer, int columns = 4, string rowSeparator = "\n", string columnSeparator = " ")
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (num < buffer.Length)
		{
			stringBuilder.Append(_byteHexValue[0xFF & (buffer[num] >> 24)]);
			stringBuilder.Append(_byteHexValue[0xFF & (buffer[num] >> 16)]);
			stringBuilder.Append(_byteHexValue[0xFF & (buffer[num] >> 8)]);
			stringBuilder.Append(_byteHexValue[0xFF & buffer[num]]);
			if (++num % columns == 0)
			{
				stringBuilder.Append(rowSeparator);
			}
			else
			{
				stringBuilder.Append(columnSeparator);
			}
		}
		return stringBuilder.ToString();
	}

	private static bool TryHexToByte(char c, out byte result)
	{
		if (c >= '0' && c <= '9')
		{
			result = (byte)(c - 48);
			return true;
		}
		if (c >= 'a' && c <= 'f')
		{
			result = (byte)(10 + c - 97);
			return true;
		}
		if (c >= 'A' && c <= 'F')
		{
			result = (byte)(10 + c - 65);
			return true;
		}
		result = 0;
		return false;
	}

	public unsafe static int HexToBytes(string str, byte* buffer, int length)
	{
		int i = 0;
		for (int num = 0; i < str.Length && num < length; i++)
		{
			if (TryHexToByte(str[i], out var result))
			{
				i++;
				if (i == str.Length)
				{
					buffer[num++] = result;
					break;
				}
				if (TryHexToByte(str[i], out var result2))
				{
					buffer[num++] = (byte)(16 * result + result2);
					continue;
				}
				buffer[num++] = result;
			}
			if (!char.IsWhiteSpace(str, i))
			{
				break;
			}
		}
		return i;
	}

	public unsafe static (int, int) HexToInts(string str, int* buffer, int length)
	{
		int i = 0;
		int j;
		for (j = 0; j < length; j++)
		{
			if (i >= str.Length)
			{
				break;
			}
			int num = 0;
			for (int k = 0; k < 8; k++)
			{
				if (i >= str.Length)
				{
					break;
				}
				char c = str[i++];
				if (TryHexToByte(c, out var result))
				{
					num = (num << 4) | result;
					continue;
				}
				if (!char.IsWhiteSpace(c))
				{
					return (i, j);
				}
				for (; i < str.Length && char.IsWhiteSpace(str[i]); i++)
				{
				}
				break;
			}
			buffer[j] = num;
		}
		return (i, j);
	}

	public unsafe static string BytesToHex(byte[] buffer, int columns = 16)
	{
		if (buffer == null)
		{
			return "<null>";
		}
		if (buffer.Length == 0)
		{
			return "<empty>";
		}
		fixed (byte* buffer2 = buffer)
		{
			return BytesToHex(buffer2, buffer.Length, columns);
		}
	}

	public unsafe static string BytesToHex(ReadOnlySpan<byte> buffer, int columns = 16)
	{
		if (buffer.Length == 0)
		{
			return "<empty>";
		}
		fixed (byte* buffer2 = buffer)
		{
			return BytesToHex(buffer2, buffer.Length, columns);
		}
	}

	internal static void RepeatingCopyTo(this ReadOnlySpan<byte> src, Span<byte> dst)
	{
		if (!src.IsEmpty)
		{
			while (dst.Length >= src.Length)
			{
				src.CopyTo(dst);
				int length = src.Length;
				dst = dst.Slice(length, dst.Length - length);
			}
			if (dst.Length > 0)
			{
				src.Slice(0, dst.Length).CopyTo(dst);
			}
		}
	}

	internal static bool RepeatingSequenceEqualTo(this ReadOnlySpan<byte> span, ReadOnlySpan<byte> other)
	{
		while (span.Length >= other.Length)
		{
			if (!span.Slice(0, other.Length).SequenceEqual(other))
			{
				return false;
			}
			int length = other.Length;
			span = span.Slice(length, span.Length - length);
		}
		if (span.Length > 0 && !span.SequenceEqual(other.Slice(0, span.Length)))
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T Read<T>(this Span<byte> source) where T : unmanaged
	{
		Assert.Always(source.Length >= sizeof(T), source.Length, sizeof(T));
		return Unsafe.As<byte, T>(ref source[0]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T Read<T>(this Span<int> source) where T : unmanaged
	{
		Assert.Always(source.Length * 4 >= sizeof(T), source.Length, sizeof(T));
		return Unsafe.As<int, T>(ref source[0]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T AsRef<T>(this Span<byte> source) where T : unmanaged
	{
		Assert.Always(source.Length >= sizeof(T), source.Length, sizeof(T));
		return ref Unsafe.As<byte, T>(ref source[0]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T AsRef<T>(this Span<int> source) where T : unmanaged
	{
		Assert.Always(source.Length * 4 >= sizeof(T), source.Length, sizeof(T));
		return ref Unsafe.As<int, T>(ref source[0]);
	}

	public unsafe static T* AsPointer<T>(this Span<byte> source) where T : unmanaged
	{
		Assert.Always(source.Length >= sizeof(T), source.Length, sizeof(T));
		return (T*)Unsafe.AsPointer(ref source[0]);
	}

	public unsafe static T* AsPointer<T>(this Span<int> source) where T : unmanaged
	{
		Assert.Always(source.Length * 4 >= sizeof(T), source.Length, sizeof(T));
		return (T*)Unsafe.AsPointer(ref source[0]);
	}
}
