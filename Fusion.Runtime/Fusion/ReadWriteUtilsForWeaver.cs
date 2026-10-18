#define DEBUG
using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine.Scripting;

namespace Fusion;

public static class ReadWriteUtilsForWeaver
{
	private const float ACCURACY = 1024f;

	private const int STRING_LENGTH_INDEX = 0;

	private const int STRING_HASHCODE_INDEX = 1;

	private const int STRING_DATA_INDEX = 2;

	private const int STRING_NOHASHCODE_DATA_INDEX = 1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Preserve]
	public unsafe static bool ReadBoolean(int* data)
	{
		return (*data != 0) ? true : false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Preserve]
	public unsafe static void WriteBoolean(int* data, bool value)
	{
		*data = (value ? 1 : 0);
	}

	[Preserve]
	public unsafe static int GetByteArrayHashCode(byte* ptr, int length)
	{
		return HashCodeUtilities.GetArrayHashCode(ptr, length);
	}

	[Preserve]
	public unsafe static int WriteStringUtf8NoHash(void* destination, string str)
	{
		return Native.WriteLengthPrefixedUTF8(destination, str);
	}

	[Preserve]
	public unsafe static int ReadStringUtf8NoHash(void* source, out string result)
	{
		return Native.ReadLengthPrefixedUTF8(source, out result);
	}

	[Preserve]
	public static int GetByteCountUtf8NoHash(string value)
	{
		return Native.GetLengthPrefixedUTF8ByteCount(value);
	}

	[Preserve]
	public static int GetStringHashCode(string value, int maxLength)
	{
		int len = Math.Min(value.Length, maxLength);
		return value.GetHashDeterministicInternal(len, 352654597);
	}

	[Preserve]
	public unsafe static int WriteStringUtf32NoHash(int* ptr, int maxLength, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			*ptr = 0;
			return 4;
		}
		UTF32Tools.ConversionResult conversionResult = UTF32Tools.Convert(value, (uint*)(ptr + 1), maxLength);
		*ptr = conversionResult.CodePointCount;
		return (conversionResult.CodePointCount + 1) * 4;
	}

	[Preserve]
	public unsafe static int ReadStringUtf32NoHash(int* ptr, int maxLength, out string result)
	{
		int num = Math.Min(*ptr, maxLength);
		int* value = ptr + 1;
		if (num == 0)
		{
			result = "";
		}
		else
		{
			result = new string((sbyte*)value, 0, num * 4, Encoding.UTF32);
		}
		return (num + 1) * 4;
	}

	[Preserve]
	public unsafe static int WriteStringUtf32WithHash(int* ptr, int maxLength, string value, ref string cache)
	{
		if (string.IsNullOrEmpty(value))
		{
			*ptr = 0;
			ptr[1] = 0;
			return 8;
		}
		UTF32Tools.ConversionResult conversionResult = UTF32Tools.Convert(value, (uint*)(ptr + 2), maxLength);
		*ptr = conversionResult.CodePointCount;
		Assert.Check(conversionResult.CharacterCount <= value.Length);
		if (conversionResult.CharacterCount < value.Length)
		{
			cache = value.Substring(0, conversionResult.CharacterCount);
		}
		else
		{
			cache = value;
		}
		ptr[1] = cache.GetHashDeterministic();
		return (conversionResult.CodePointCount + 2) * 4;
	}

	[Preserve]
	public unsafe static int ReadStringUtf32WithHash(int* ptr, int maxLength, ref string cache)
	{
		int num = Math.Min(*ptr, maxLength);
		int num2 = ptr[1];
		int* ptr2 = ptr + 2;
		if (num == 0)
		{
			cache = "";
		}
		else
		{
			if (cache != null && num >= cache.Length / 2 && num <= cache.Length && num2 == cache.GetHashCode() && UTF32Tools.CompareOrdinal(cache, (uint*)ptr2, num) == 0)
			{
				return (2 + num) * 4;
			}
			cache = new string((sbyte*)ptr2, 0, num * 4, Encoding.UTF32);
		}
		return (2 + num) * 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Preserve]
	public static int GetWordCountString(int capacity, bool withCaching)
	{
		if (withCaching)
		{
			return 2 + capacity;
		}
		return 1 + capacity;
	}

	[Preserve]
	public static int VerifyRawNetworkUnwrap<T>(int actual, int maxBytes)
	{
		if (actual > maxBytes)
		{
			throw new InvalidOperationException($"Overflow when unwrapping {typeof(T).FullName}: expected max {maxBytes}, got {actual}");
		}
		return actual;
	}

	[Preserve]
	public static int VerifyRawNetworkWrap<T>(int actual, int maxBytes)
	{
		if (actual > maxBytes)
		{
			throw new InvalidOperationException($"Overflow when wrapping {typeof(T).FullName}: expected max {maxBytes}, got {actual}");
		}
		return actual;
	}
}
