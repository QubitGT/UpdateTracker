using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Cysharp.Text;

internal static class EnumUtil<T>
{
	private const string InvalidName = "$";

	private static readonly Dictionary<T, string> names;

	private static readonly Dictionary<T, byte[]> utf8names;

	static EnumUtil()
	{
		string[] array = Enum.GetNames(typeof(T));
		Array values = Enum.GetValues(typeof(T));
		names = new Dictionary<T, string>(array.Length);
		utf8names = new Dictionary<T, byte[]>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			if (names.ContainsKey((T)values.GetValue(i)))
			{
				names[(T)values.GetValue(i)] = "$";
				utf8names[(T)values.GetValue(i)] = Array.Empty<byte>();
			}
			else
			{
				names.Add((T)values.GetValue(i), array[i]);
				utf8names.Add((T)values.GetValue(i), Encoding.UTF8.GetBytes(array[i]));
			}
		}
	}

	public static bool TryFormatUtf16(T value, Span<char> dest, out int written, ReadOnlySpan<char> _)
	{
		if (!names.TryGetValue(value, out string value2) || value2 == "$")
		{
			value2 = value.ToString();
		}
		written = value2.Length;
		return MemoryExtensions.AsSpan(value2).TryCopyTo(dest);
	}

	public static bool TryFormatUtf8(T value, Span<byte> dest, out int written, StandardFormat _)
	{
		if (!utf8names.TryGetValue(value, out byte[] value2) || value2.Length == 0)
		{
			value2 = Encoding.UTF8.GetBytes(value.ToString());
		}
		written = value2.Length;
		return MemoryExtensions.AsSpan(value2).TryCopyTo(dest);
	}
}
