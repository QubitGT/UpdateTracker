using System;
using VYaml.Annotations;

namespace VYaml.Internal;

internal static class KeyNameMutator
{
	public static string Mutate(string s, NamingConvention namingConvention)
	{
		return namingConvention switch
		{
			NamingConvention.LowerCamelCase => ToLowerCamelCase(s), 
			NamingConvention.UpperCamelCase => s, 
			NamingConvention.SnakeCase => ToSnakeCase(s), 
			NamingConvention.KebabCase => ToSnakeCase(s, '-'), 
			_ => throw new ArgumentOutOfRangeException("namingConvention", namingConvention, null), 
		};
	}

	public static string ToLowerCamelCase(string s)
	{
		ReadOnlySpan<char> readOnlySpan = MemoryExtensions.AsSpan(s);
		if (readOnlySpan.Length <= 0 || (readOnlySpan.Length <= 1 && char.IsLower(readOnlySpan[0])))
		{
			return s;
		}
		Span<char> span = stackalloc char[readOnlySpan.Length];
		span[0] = char.ToLowerInvariant(readOnlySpan[0]);
		ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
		ReadOnlySpan<char> readOnlySpan3 = readOnlySpan2.Slice(1, readOnlySpan2.Length - 1);
		Span<char> span2 = span;
		readOnlySpan3.CopyTo(span2.Slice(1, span2.Length - 1));
		return span.ToString();
	}

	public static string ToSnakeCase(string s, char separator = '_')
	{
		ReadOnlySpan<char> readOnlySpan = MemoryExtensions.AsSpan(s);
		if (readOnlySpan.Length <= 0)
		{
			return s;
		}
		Span<char> span = stackalloc char[readOnlySpan.Length * 2];
		int num = 0;
		ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			char c = readOnlySpan2[i];
			if (char.IsUpper(c))
			{
				if (num == 0 || char.IsUpper(readOnlySpan[num - 1]))
				{
					span[num++] = char.ToLowerInvariant(c);
					continue;
				}
				span[num++] = separator;
				if (span.Length <= num)
				{
					span = new char[span.Length * 2];
				}
				span[num++] = char.ToLowerInvariant(c);
			}
			else
			{
				span[num++] = c;
			}
		}
		Span<char> span2 = span;
		return span2.Slice(0, num).ToString();
	}
}
