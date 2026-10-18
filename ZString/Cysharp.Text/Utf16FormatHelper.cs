using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cysharp.Text;

internal static class Utf16FormatHelper
{
	private const char sp = ' ';

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FormatTo<TBufferWriter, T>(ref TBufferWriter sb, T arg, int width, ReadOnlySpan<char> format, string argName) where TBufferWriter : IBufferWriter<char>
	{
		if (width <= 0)
		{
			Span<char> span = sb.GetSpan();
			if (!Utf16ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, span, out var charsWritten, format))
			{
				sb.Advance(0);
				span = sb.GetSpan(Math.Max(span.Length + 1, charsWritten));
				if (!Utf16ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, span, out charsWritten, format))
				{
					ExceptionUtil.ThrowArgumentException(argName);
				}
			}
			sb.Advance(charsWritten);
			width *= -1;
			int num = width - charsWritten;
			if (width > 0 && num > 0)
			{
				sb.GetSpan(num).Fill(' ');
				sb.Advance(num);
			}
		}
		else
		{
			FormatToRightJustify(ref sb, arg, width, format, argName);
		}
	}

	private static void FormatToRightJustify<TBufferWriter, T>(ref TBufferWriter sb, T arg, int width, ReadOnlySpan<char> format, string argName) where TBufferWriter : IBufferWriter<char>
	{
		if (typeof(T) == typeof(string))
		{
			string text = Unsafe.As<string>(arg);
			int num = width - text.Length;
			if (num > 0)
			{
				sb.GetSpan(num).Fill(' ');
				sb.Advance(num);
			}
			Span<char> span = sb.GetSpan(text.Length);
			text.AsSpan().CopyTo(span);
			sb.Advance(text.Length);
			return;
		}
		Span<char> destination = stackalloc char[typeof(T).IsValueType ? (Unsafe.SizeOf<T>() * 8) : 1024];
		if (!Utf16ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, destination, out var charsWritten, format))
		{
			destination = stackalloc char[destination.Length * 2];
			if (!Utf16ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, destination, out charsWritten, format))
			{
				ExceptionUtil.ThrowArgumentException(argName);
			}
		}
		int num2 = width - charsWritten;
		if (num2 > 0)
		{
			sb.GetSpan(num2).Fill(' ');
			sb.Advance(num2);
		}
		Span<char> span2 = sb.GetSpan(charsWritten);
		destination.CopyTo(span2);
		sb.Advance(charsWritten);
	}
}
