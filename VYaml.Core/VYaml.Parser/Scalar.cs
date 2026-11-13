using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using VYaml.Internal;

namespace VYaml.Parser;

internal class Scalar : ITokenContent
{
	private const int MinimumGrow = 4;

	private const int GrowFactor = 200;

	public static readonly Scalar Null = new Scalar(0);

	private byte[] buffer;

	public int Length { get; private set; }

	public Scalar(int capacity)
	{
		buffer = new byte[capacity];
	}

	public Scalar(ReadOnlySpan<byte> content)
	{
		buffer = new byte[content.Length];
		Write(content);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<byte> AsSpan()
	{
		return MemoryExtensions.AsSpan(buffer, 0, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<byte> AsSpan(int start, int length)
	{
		return MemoryExtensions.AsSpan(buffer, start, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<byte> AsUtf8()
	{
		return MemoryExtensions.AsSpan(buffer, 0, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write(byte code)
	{
		if (Length == buffer.Length)
		{
			Grow();
		}
		buffer[Length++] = code;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write(LineBreakState lineBreak)
	{
		switch (lineBreak)
		{
		case LineBreakState.Lf:
			Write(10);
			break;
		case LineBreakState.CrLf:
			Write(13);
			Write(10);
			break;
		case LineBreakState.Cr:
			Write(13);
			break;
		default:
			throw new ArgumentOutOfRangeException("lineBreak", lineBreak, null);
		case LineBreakState.None:
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write(ReadOnlySpan<byte> codes)
	{
		Grow(Length + codes.Length);
		codes.CopyTo(MemoryExtensions.AsSpan(buffer, Length, codes.Length));
		Length += codes.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteUnicodeCodepoint(int codepoint)
	{
		Span<char> span = stackalloc char[1] { (char)codepoint };
		Span<byte> span2 = stackalloc byte[StringEncoding.Utf8.GetByteCount(span)];
		StringEncoding.Utf8.GetBytes(span, span2);
		Write(span2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		Length = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return StringEncoding.Utf8.GetString(AsSpan());
	}

	public bool IsNull()
	{
		Span<byte> span = AsSpan();
		switch (span.Length)
		{
		case 1:
			if (span[0] != 126)
			{
				break;
			}
			goto case 0;
		case 4:
			if (!span.SequenceEqual(YamlCodes.Null0) && !span.SequenceEqual(YamlCodes.Null1) && !span.SequenceEqual(YamlCodes.Null2))
			{
				break;
			}
			goto case 0;
		case 0:
			return true;
		}
		return false;
	}

	public bool TryGetBool(out bool value)
	{
		Span<byte> span = AsSpan();
		switch (span.Length)
		{
		case 4:
			if (span.SequenceEqual(YamlCodes.True0) || span.SequenceEqual(YamlCodes.True1) || span.SequenceEqual(YamlCodes.True2))
			{
				value = true;
				return true;
			}
			break;
		case 5:
			if (span.SequenceEqual(YamlCodes.False0) || span.SequenceEqual(YamlCodes.False1) || span.SequenceEqual(YamlCodes.False2))
			{
				value = false;
				return true;
			}
			break;
		}
		value = false;
		return false;
	}

	public bool TryGetInt32(out int value)
	{
		Span<byte> span = AsSpan();
		if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span, out value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			return true;
		}
		if (TryDetectHex(span, out var slice))
		{
			if (Utf8Parser.TryParse(slice, out value, out bytesConsumed, 'x'))
			{
				return bytesConsumed == slice.Length;
			}
			return false;
		}
		if (TryDetectHexNegative(span, out slice) && Utf8Parser.TryParse(slice, out value, out bytesConsumed, 'x') && bytesConsumed == slice.Length)
		{
			value *= -1;
			return true;
		}
		if (TryParseOctal(span, out var value2) && value2 <= int.MaxValue)
		{
			value = (int)value2;
			return true;
		}
		return false;
	}

	public bool TryGetInt64(out long value)
	{
		Span<byte> span = AsSpan();
		if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span, out value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			return true;
		}
		Span<byte> span2;
		if (span.Length > YamlCodes.HexPrefix.Length && span.StartsWith(YamlCodes.HexPrefix))
		{
			span2 = span;
			int num = YamlCodes.HexPrefix.Length;
			Span<byte> span3 = span2.Slice(num, span2.Length - num);
			if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span3, out value, out int bytesConsumed2, 'x'))
			{
				return bytesConsumed2 == span3.Length;
			}
			return false;
		}
		if (span.Length > YamlCodes.HexPrefixNegative.Length && span.StartsWith(YamlCodes.HexPrefixNegative))
		{
			span2 = span;
			int num = YamlCodes.HexPrefixNegative.Length;
			Span<byte> span4 = span2.Slice(num, span2.Length - num);
			if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span4, out value, out int bytesConsumed3, 'x') && bytesConsumed3 == span4.Length)
			{
				value = -value;
				return true;
			}
		}
		if (TryParseOctal(span, out var value2) && value2 <= long.MaxValue)
		{
			value = (long)value2;
			return true;
		}
		return false;
	}

	public bool TryGetUInt32(out uint value)
	{
		Span<byte> span = AsSpan();
		if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span, out value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			return true;
		}
		if (TryDetectHex(span, out var slice))
		{
			if (Utf8Parser.TryParse(slice, out value, out bytesConsumed, 'x'))
			{
				return bytesConsumed == slice.Length;
			}
			return false;
		}
		if (TryParseOctal(span, out var value2) && value2 <= uint.MaxValue)
		{
			value = (uint)value2;
			return true;
		}
		return false;
	}

	public bool TryGetUInt64(out ulong value)
	{
		Span<byte> span = AsSpan();
		if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span, out value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			return true;
		}
		if (TryDetectHex(span, out var slice))
		{
			if (Utf8Parser.TryParse(slice, out value, out bytesConsumed, 'x'))
			{
				return bytesConsumed == slice.Length;
			}
			return false;
		}
		if (TryParseOctal(span, out value))
		{
			return true;
		}
		return false;
	}

	public bool TryGetFloat(out float value)
	{
		Span<byte> span = AsSpan();
		if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span, out value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			return true;
		}
		switch (span.Length)
		{
		case 4:
			if (span.SequenceEqual(YamlCodes.Inf0) || span.SequenceEqual(YamlCodes.Inf1) || span.SequenceEqual(YamlCodes.Inf2))
			{
				value = float.PositiveInfinity;
				return true;
			}
			if (span.SequenceEqual(YamlCodes.Nan0) || span.SequenceEqual(YamlCodes.Nan1) || span.SequenceEqual(YamlCodes.Nan2))
			{
				value = float.NaN;
				return true;
			}
			break;
		case 5:
			if (span.SequenceEqual(YamlCodes.Inf3) || span.SequenceEqual(YamlCodes.Inf4) || span.SequenceEqual(YamlCodes.Inf5))
			{
				value = float.PositiveInfinity;
				return true;
			}
			if (span.SequenceEqual(YamlCodes.NegInf0) || span.SequenceEqual(YamlCodes.NegInf1) || span.SequenceEqual(YamlCodes.NegInf2))
			{
				value = float.NegativeInfinity;
				return true;
			}
			break;
		}
		return false;
	}

	public bool TryGetDouble(out double value)
	{
		Span<byte> span = AsSpan();
		if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span, out value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			return true;
		}
		switch (span.Length)
		{
		case 4:
			if (span.SequenceEqual(YamlCodes.Inf0) || span.SequenceEqual(YamlCodes.Inf1) || span.SequenceEqual(YamlCodes.Inf2))
			{
				value = double.PositiveInfinity;
				return true;
			}
			if (span.SequenceEqual(YamlCodes.Nan0) || span.SequenceEqual(YamlCodes.Nan1) || span.SequenceEqual(YamlCodes.Nan2))
			{
				value = double.NaN;
				return true;
			}
			break;
		case 5:
			if (span.SequenceEqual(YamlCodes.Inf3) || span.SequenceEqual(YamlCodes.Inf4) || span.SequenceEqual(YamlCodes.Inf5))
			{
				value = double.PositiveInfinity;
				return true;
			}
			if (span.SequenceEqual(YamlCodes.NegInf0) || span.SequenceEqual(YamlCodes.NegInf1) || span.SequenceEqual(YamlCodes.NegInf2))
			{
				value = double.NegativeInfinity;
				return true;
			}
			break;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool SequenceEqual(Scalar other)
	{
		return AsSpan().SequenceEqual(other.AsSpan());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool SequenceEqual(ReadOnlySpan<byte> span)
	{
		return AsSpan().SequenceEqual(span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Grow(int sizeHint)
	{
		if (sizeHint > buffer.Length)
		{
			int num;
			for (num = buffer.Length * 200 / 100; num < sizeHint; num = num * 200 / 100)
			{
			}
			SetCapacity(num);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TryDetectHex(ReadOnlySpan<byte> span, out ReadOnlySpan<byte> slice)
	{
		if (span.Length > YamlCodes.HexPrefix.Length && span.StartsWith(YamlCodes.HexPrefix))
		{
			ReadOnlySpan<byte> readOnlySpan = span;
			int num = YamlCodes.HexPrefix.Length;
			slice = readOnlySpan.Slice(num, readOnlySpan.Length - num);
			return true;
		}
		slice = default(ReadOnlySpan<byte>);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TryDetectHexNegative(ReadOnlySpan<byte> span, out ReadOnlySpan<byte> slice)
	{
		if (span.Length > YamlCodes.HexPrefixNegative.Length && span.StartsWith(YamlCodes.HexPrefixNegative))
		{
			ReadOnlySpan<byte> readOnlySpan = span;
			int num = YamlCodes.HexPrefixNegative.Length;
			slice = readOnlySpan.Slice(num, readOnlySpan.Length - num);
			return true;
		}
		slice = default(ReadOnlySpan<byte>);
		return false;
	}

	private static bool TryParseOctal(ReadOnlySpan<byte> span, out ulong value)
	{
		if (span.Length <= YamlCodes.OctalPrefix.Length || !span.StartsWith(YamlCodes.OctalPrefix))
		{
			value = 0uL;
			return false;
		}
		int i;
		for (i = YamlCodes.OctalPrefix.Length; i < span.Length && span[i] == 48; i++)
		{
		}
		if (i >= span.Length)
		{
			value = 0uL;
			return i == span.Length;
		}
		ReadOnlySpan<byte> readOnlySpan = span;
		int num = i;
		ReadOnlySpan<byte> readOnlySpan2 = readOnlySpan.Slice(num, readOnlySpan.Length - num);
		int num2 = readOnlySpan2[0] - 48;
		if (num2 < 0 || num2 > 7 || (num2 > 1 && readOnlySpan2.Length == 22) || readOnlySpan2.Length > 22)
		{
			value = 0uL;
			return false;
		}
		value = (ulong)num2;
		for (int j = 1; j < readOnlySpan2.Length; j++)
		{
			num2 = readOnlySpan2[j] - 48;
			if (num2 < 0 || num2 > 7)
			{
				value = 0uL;
				return false;
			}
			value = (value << 3) + (uint)num2;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Grow()
	{
		int num = buffer.Length * 200 / 100;
		if (num < buffer.Length + 4)
		{
			num = buffer.Length + 4;
		}
		SetCapacity(num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetCapacity(int newCapacity)
	{
		if (buffer.Length < newCapacity)
		{
			byte[] destinationArray = ArrayPool<byte>.Shared.Rent(newCapacity);
			Array.Copy(buffer, 0, destinationArray, 0, Length);
			ArrayPool<byte>.Shared.Return(buffer);
			buffer = destinationArray;
		}
	}
}
