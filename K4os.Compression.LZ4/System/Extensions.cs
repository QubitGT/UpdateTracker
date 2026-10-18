using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

internal static class Extensions
{
	internal static T Required<T>([NotNull] this T? value, [CallerArgumentExpression("value")] string name = null)
	{
		if (value != null)
		{
			return value;
		}
		return ThrowArgumentNullException<T>(name);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public static void AssertTrue(this bool value, [CallerArgumentExpression("value")] string? name = null)
	{
		if (!value)
		{
			ThrowAssertionFailed(name);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowAssertionFailed(string? name)
	{
		throw new ArgumentException((name ?? "<unknown>") + " assertion failed");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static T ThrowArgumentNullException<T>(string name)
	{
		throw new ArgumentNullException(name);
	}

	internal static void Validate<T>(this T[]? buffer, int offset, int length, bool allowNullIfEmpty = false)
	{
		if (!allowNullIfEmpty || buffer != null || offset != 0 || length != 0)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", "cannot be null");
			}
			if (offset < 0 || length < 0 || offset + length > buffer.Length)
			{
				throw new ArgumentException($"invalid offset/length combination: {offset}/{length}");
			}
		}
	}
}
