using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cysharp.Text;

internal static class Utf8FormatHelper
{
	private const byte sp = 32;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FormatTo<TBufferWriter, T>(ref TBufferWriter sb, T arg, int width, StandardFormat format, string argName) where TBufferWriter : IBufferWriter<byte>
	{
		if (width <= 0)
		{
			Span<byte> span = sb.GetSpan();
			if (!Utf8ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, span, out var written, format))
			{
				sb.Advance(0);
				span = sb.GetSpan(Math.Max(span.Length + 1, written));
				if (!Utf8ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, span, out written, format))
				{
					ExceptionUtil.ThrowArgumentException(argName);
				}
			}
			sb.Advance(written);
			width *= -1;
			int num = width - written;
			if (width > 0 && num > 0)
			{
				sb.GetSpan(num).Fill(32);
				sb.Advance(num);
			}
		}
		else
		{
			FormatToRightJustify(ref sb, arg, width, format, argName);
		}
	}

	private static void FormatToRightJustify<TBufferWriter, T>(ref TBufferWriter sb, T arg, int width, StandardFormat format, string argName) where TBufferWriter : IBufferWriter<byte>
	{
		if (typeof(T) == typeof(string))
		{
			string text = Unsafe.As<string>(arg);
			int num = width - text.Length;
			if (num > 0)
			{
				sb.GetSpan(num).Fill(32);
				sb.Advance(num);
			}
			ZString.AppendChars(ref sb, text.AsSpan());
			return;
		}
		Span<byte> destination = stackalloc byte[typeof(T).IsValueType ? (Unsafe.SizeOf<T>() * 8) : 1024];
		if (!Utf8ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, destination, out var written, format))
		{
			destination = stackalloc byte[destination.Length * 2];
			if (!Utf8ValueStringBuilder.FormatterCache<T>.TryFormatDelegate(arg, destination, out written, format))
			{
				ExceptionUtil.ThrowArgumentException(argName);
			}
		}
		int num2 = width - written;
		if (num2 > 0)
		{
			sb.GetSpan(num2).Fill(32);
			sb.Advance(num2);
		}
		Span<byte> span = sb.GetSpan(written);
		destination.CopyTo(span);
		sb.Advance(written);
	}
}
