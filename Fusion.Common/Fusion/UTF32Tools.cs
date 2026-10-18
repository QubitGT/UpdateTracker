#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Fusion;

public static class UTF32Tools
{
	public struct CharEnumerator : IEnumerator<char>, IEnumerator, IDisposable
	{
		private int _index;

		private int _length;

		private char _pendingLowSurrogate;

		private unsafe uint* _ptr;

		public char Current { get; private set; }

		object IEnumerator.Current => Current;

		internal unsafe CharEnumerator(uint* utf32, int length)
		{
			_index = 0;
			Current = (_pendingLowSurrogate = '\0');
			_ptr = utf32;
			_length = length;
		}

		public void Dispose()
		{
		}

		public unsafe bool MoveNext()
		{
			if (_pendingLowSurrogate != 0)
			{
				Current = _pendingLowSurrogate;
				_pendingLowSurrogate = '\0';
				return true;
			}
			if (_index >= _length)
			{
				return false;
			}
			(Current, _pendingLowSurrogate) = ToUTF16(_ptr[_index++]);
			return true;
		}

		public void Reset()
		{
			_index = 0;
		}
	}

	public readonly struct ConversionResult
	{
		public readonly int CharacterCount;

		public readonly int CodePointCount;

		public ConversionResult(int words, int characters)
		{
			CodePointCount = words;
			CharacterCount = characters;
		}
	}

	public unsafe static ConversionResult Convert(string str, uint* dst, int dstCapacity)
	{
		if (string.IsNullOrEmpty(str))
		{
			return default(ConversionResult);
		}
		fixed (char* str2 = str)
		{
			return Convert(str2, str.Length, dst, dstCapacity);
		}
	}

	public unsafe static ConversionResult Convert(char* str, int strLength, uint* dst, int dstCapacity)
	{
		int num = 0;
		int num2 = 0;
		while (num < dstCapacity && num2 < strLength)
		{
			char c = str[num2];
			if ((uint)(c - 55296) >= 2048u)
			{
				dst[num] = c;
			}
			else
			{
				if (!char.IsHighSurrogate(c) || num2 >= strLength - 1 || !char.IsLowSurrogate(str[num2 + 1]))
				{
					Assert.AlwaysFail($"Failed to convert character {c}");
					break;
				}
				char c2 = c;
				char c3 = str[++num2];
				dst[num] = (uint)((c2 - 55296) * 1024 + (c3 - 56320) + 65536);
			}
			num++;
			num2++;
		}
		return new ConversionResult(num, num2);
	}

	internal unsafe static int CompareOrdinal(uint* strA, int aLength, uint* strB, int bLength, bool ignoreCase)
	{
		if (strA == null && aLength > 0)
		{
			throw new ArgumentNullException("strA");
		}
		if (strB == null && bLength > 0)
		{
			throw new ArgumentNullException("strB");
		}
		int num = Math.Min(aLength, bLength);
		if (!ignoreCase)
		{
			for (int i = 0; i < num; i++)
			{
				int num2 = (int)(strA[i] - strB[i]);
				if (num2 != 0)
				{
					return num2;
				}
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				if (!IsValidCodePoint(strA[j]))
				{
					Assert.AlwaysFail($"Failed to convert character {strA[j]}");
					continue;
				}
				if (!IsValidCodePoint(strB[j]))
				{
					Assert.AlwaysFail($"Failed to convert character {strB[j]}");
					continue;
				}
				int num3 = (int)(strA[j] - strB[j]);
				if (num3 == 0)
				{
					continue;
				}
				uint num4 = ToLowerInvariant(strA[j]);
				if (num4 != strB[j])
				{
					uint num5 = ToLowerInvariant(strB[j]);
					if (num4 != num5)
					{
						return num3;
					}
				}
			}
		}
		return aLength - bLength;
	}

	internal unsafe static int CompareOrdinal(string strA, uint* strB, int bLength, bool ignoreCase = false)
	{
		if (strA == null)
		{
			throw new ArgumentNullException("strA");
		}
		int length = strA.Length;
		fixed (char* ptr = strA)
		{
			int num = 0;
			int num2 = 0;
			while (num < bLength && num2 < length)
			{
				char c = ptr[num2];
				if ((uint)(c - 55296) >= 2048u)
				{
					int num3 = (int)(c - strB[num]);
					if (num3 != 0)
					{
						if (!ignoreCase)
						{
							return num3;
						}
						(char, char) tuple = ToUTF16(strB[num]);
						var (c2, _) = tuple;
						if (tuple.Item2 != 0)
						{
							return num3;
						}
						num3 = char.ToLowerInvariant(c) - char.ToLowerInvariant(c2);
						if (num3 != 0)
						{
							return num3;
						}
					}
				}
				else
				{
					if (!char.IsHighSurrogate(c) || num2 >= length - 1 || !char.IsLowSurrogate(ptr[num2 + 1]))
					{
						Assert.AlwaysFail($"Failed to convert character {c}");
						break;
					}
					char charOrHighSurrogate = c;
					char lowSurrogate = ptr[++num2];
					uint num4 = ToUTF32(charOrHighSurrogate, lowSurrogate);
					int result = (int)(num4 - strB[num]);
					if (num4 != strB[num])
					{
						if (!ignoreCase)
						{
							return result;
						}
						uint num5 = ToLowerInvariant(num4);
						if (num5 != strB[num])
						{
							uint num6 = ToLowerInvariant(strB[num]);
							if (num5 != num6)
							{
								return result;
							}
						}
					}
				}
				num++;
				num2++;
			}
			return length - num2 - (bLength - num);
		}
	}

	internal unsafe static bool EndsWithOrdinal(uint* strA, int aLength, uint* bStr, int bLength, bool ignoreCase = false)
	{
		if (bLength > aLength)
		{
			return false;
		}
		return CompareOrdinal(strA + (aLength - bLength), bLength, bStr, bLength, ignoreCase) == 0;
	}

	internal unsafe static bool EndsWithOrdinal(uint* strA, int aLength, string strB, bool ignoreCase = false)
	{
		if (strB == null)
		{
			throw new ArgumentNullException("strB");
		}
		int byteCount = Encoding.UTF32.GetByteCount(strB);
		Assert.Check(byteCount % 4 == 0);
		int num = byteCount / 4;
		if (aLength < num)
		{
			return false;
		}
		return CompareOrdinal(strB, strA + (aLength - num), num, ignoreCase) == 0;
	}

	internal unsafe static int GetHashDeterministic(uint* str, int length)
	{
		int a = 352654597;
		int b = a;
		for (int i = 0; i < length; i++)
		{
			(char, char) tuple = ToUTF16(str[i]);
			char item = tuple.Item1;
			char item2 = tuple.Item2;
			a = ((a << 5) + a) ^ item;
			Swap(ref a, ref b);
			if (item2 != 0)
			{
				a = ((a << 5) + a) ^ item2;
				Swap(ref a, ref b);
			}
		}
		return a + b * 1566083941;
	}

	internal unsafe static bool StartsWithOrdinal(uint* strA, int aLength, uint* strB, int bLength, bool ignoreCase = false)
	{
		if (bLength > aLength)
		{
			return false;
		}
		return CompareOrdinal(strA, bLength, strB, bLength, ignoreCase) == 0;
	}

	internal unsafe static bool StartsWithOrdinal(uint* strA, int aLength, string strB, bool ignoreCase = false)
	{
		if (strB == null)
		{
			throw new ArgumentNullException("strB");
		}
		int byteCount = Encoding.UTF32.GetByteCount(strB);
		Assert.Check(byteCount % 4 == 0);
		int num = byteCount / 4;
		if (aLength < num)
		{
			return false;
		}
		return CompareOrdinal(strB, strA, num, ignoreCase) == 0;
	}

	internal unsafe static int IndexOf(uint* str, int length, string pattern)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (pattern == null)
		{
			throw new ArgumentNullException("pattern");
		}
		if (length == 0)
		{
			return -1;
		}
		int length2 = GetLength(pattern);
		if (length2 > length)
		{
			return -1;
		}
		fixed (char* ptr = pattern)
		{
			char* end = ptr + pattern.Length;
			for (int i = 0; i + length2 <= length; i++)
			{
				char* pstr = ptr;
				int j;
				for (j = 0; j < length2; j++)
				{
					uint num = ReadNextCodePoint(ref pstr, end);
					if (str[i + j] != num)
					{
						break;
					}
				}
				if (j == length2)
				{
					return i;
				}
			}
		}
		return -1;
	}

	internal unsafe static int IndexOf(uint* str, int length, uint* pattern, int patternLength)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (patternLength < 0)
		{
			throw new ArgumentOutOfRangeException("patternLength");
		}
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (pattern == null)
		{
			throw new ArgumentNullException("pattern");
		}
		if (length == 0 || patternLength > length)
		{
			return -1;
		}
		for (int i = 0; i + patternLength <= length; i++)
		{
			int j;
			for (j = 0; j < patternLength && str[i + j] == pattern[j]; j++)
			{
			}
			if (j == patternLength)
			{
				return i;
			}
		}
		return -1;
	}

	internal unsafe static void ToLowerInvariant(uint* src, uint* dst, int length)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (length < 0)
		{
			throw new ArgumentNullException("length");
		}
		for (int i = 0; i < length; i++)
		{
			dst[i] = ToLowerInvariant(src[i]);
		}
	}

	internal unsafe static void ToUpperInvariant(uint* src, uint* dst, int length)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (length < 0)
		{
			throw new ArgumentNullException("length");
		}
		for (int i = 0; i < length; i++)
		{
			dst[i] = ToUpperInvariant(src[i]);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static char GetHighSurrogate(uint scalar)
	{
		return (char)((scalar - 65536) / 1024 + 55296);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetLength(string str)
	{
		int byteCount = Encoding.UTF32.GetByteCount(str);
		Assert.Check(byteCount % 4 == 0);
		return byteCount / 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static char GetLowSurrogate(uint scalar)
	{
		return (char)((scalar - 65536) % 1024 + 56320);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsValidCodePoint(uint scalar)
	{
		return ((scalar - 1114112) ^ 0xD800) >= 4293855232u;
	}

	private unsafe static uint ReadNextCodePoint(ref char* pstr, char* end)
	{
		char c = *(pstr++);
		if (char.IsHighSurrogate(c))
		{
			Assert.Always(pstr < end, "Surrogate found at the end of the string");
			char c2 = *(pstr++);
			Assert.Check(char.IsLowSurrogate(c2));
			return (uint)((c - 55296) * 1024 + (c2 - 56320) + 65536);
		}
		Assert.Check(!char.IsLowSurrogate(c));
		return c;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Swap(ref int a, ref int b)
	{
		int num = a;
		a = b;
		b = num;
	}

	private unsafe static uint ToLowerInvariant(uint value)
	{
		var (c, c2) = ToUTF16(value);
		if (c2 == '\0')
		{
			return char.ToLowerInvariant(c);
		}
		char* ptr = stackalloc char[2];
		*ptr = c;
		ptr[1] = c2;
		string text = new string(ptr, 0, 2);
		string text2 = text.ToLowerInvariant();
		Assert.Check(text2.Length == 2);
		return ToUTF32(text2[0], text2[1]);
	}

	private unsafe static uint ToUpperInvariant(uint value)
	{
		var (c, c2) = ToUTF16(value);
		if (c2 == '\0')
		{
			return char.ToUpperInvariant(c);
		}
		char* ptr = stackalloc char[2];
		*ptr = c;
		ptr[1] = c2;
		string text = new string(ptr, 0, 2);
		string text2 = text.ToUpperInvariant();
		Assert.Check(text2.Length == 2);
		return ToUTF32(text2[0], text2[1]);
	}

	private static (char, char) ToUTF16(uint scalar)
	{
		if (scalar >= 65536)
		{
			return (GetHighSurrogate(scalar), GetLowSurrogate(scalar));
		}
		return ((char)scalar, '\0');
	}

	private static uint ToUTF32(char charOrHighSurrogate, char lowSurrogate = '\0')
	{
		if (char.IsHighSurrogate(charOrHighSurrogate))
		{
			Assert.Check(char.IsLowSurrogate(lowSurrogate));
			return (uint)((charOrHighSurrogate - 55296) * 1024 + (lowSurrogate - 56320) + 65536);
		}
		Assert.Check(lowSurrogate == '\0');
		return charOrHighSurrogate;
	}
}
