using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4;

public static class LZ4Pickler
{
	private const int MAX_STACKALLOC = 1024;

	private const byte VersionMask = 7;

	public static byte[] Pickle(byte[] source, LZ4Level level = LZ4Level.L00_FAST)
	{
		return Pickle(source.AsSpan(), level);
	}

	public static byte[] Pickle(byte[] source, int sourceIndex, int sourceLength, LZ4Level level = LZ4Level.L00_FAST)
	{
		return Pickle(source.AsSpan(sourceIndex, sourceLength), level);
	}

	public unsafe static byte[] Pickle(byte* source, int length, LZ4Level level = LZ4Level.L00_FAST)
	{
		return Pickle(new Span<byte>(source, length), level);
	}

	public static byte[] Pickle(ReadOnlySpan<byte> source, LZ4Level level = LZ4Level.L00_FAST)
	{
		int length = source.Length;
		if (length == 0)
		{
			return Mem.Empty;
		}
		if (length <= 1024)
		{
			Span<byte> buffer = stackalloc byte[1024];
			return PickleWithBuffer(source, level, buffer);
		}
		PinnedMemory.Alloc(out var memory, length, zero: false);
		try
		{
			return PickleWithBuffer(source, level, memory.Span);
		}
		finally
		{
			memory.Free();
		}
	}

	private static byte[] PickleWithBuffer(ReadOnlySpan<byte> source, LZ4Level level, Span<byte> buffer)
	{
		int length = source.Length;
		int num = LZ4Codec.Encode(source, buffer, level);
		if (num <= 0 || num >= length)
		{
			byte[] array = new byte[GetUncompressedHeaderSize(0, length) + length];
			Span<byte> target = array.AsSpan();
			int start = EncodeUncompressedHeader(target, 0, length);
			source.CopyTo(target.Slice(start));
			return array;
		}
		int compressedHeaderSize = GetCompressedHeaderSize(0, length, num);
		byte[] array2 = new byte[compressedHeaderSize + num];
		Span<byte> target2 = array2.AsSpan();
		int start2 = EncodeCompressedHeader(target2, 0, compressedHeaderSize, length, num);
		buffer.Slice(0, num).CopyTo(target2.Slice(start2));
		return array2;
	}

	public static void Pickle<TBufferWriter>(ReadOnlySpan<byte> source, TBufferWriter writer, LZ4Level level = LZ4Level.L00_FAST) where TBufferWriter : IBufferWriter<byte>
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int length = source.Length;
		if (length != 0)
		{
			int pessimisticHeaderSize = GetPessimisticHeaderSize(0, length);
			Span<byte> span = writer.GetSpan(pessimisticHeaderSize + length);
			int num = LZ4Codec.Encode(source, span.Slice(pessimisticHeaderSize, length), level);
			if (num <= 0 || num >= length)
			{
				int num2 = EncodeUncompressedHeader(span, 0, length);
				source.CopyTo(span.Slice(num2));
				writer.Advance(num2 + length);
			}
			else
			{
				int num3 = EncodeCompressedHeader(span, 0, pessimisticHeaderSize, length, num);
				writer.Advance(num3 + num);
			}
		}
	}

	public static void Pickle(ReadOnlySpan<byte> source, IBufferWriter<byte> writer, LZ4Level level = LZ4Level.L00_FAST)
	{
		LZ4Pickler.Pickle<IBufferWriter<byte>>(source, writer, level);
	}

	private static int GetPessimisticHeaderSize(int version, int sourceLength)
	{
		if (version == 0)
		{
			return 1 + EffectiveSizeOf(sourceLength);
		}
		throw UnexpectedVersion(version);
	}

	private static int GetUncompressedHeaderSize(int version, int sourceLength)
	{
		if (version == 0)
		{
			return 1;
		}
		throw UnexpectedVersion(version);
	}

	private static int GetCompressedHeaderSize(int version, int sourceLength, int encodedLength)
	{
		if (version == 0)
		{
			return 1 + EffectiveSizeOf(sourceLength - encodedLength);
		}
		throw UnexpectedVersion(version);
	}

	private static int EncodeUncompressedHeader(Span<byte> target, int version, int sourceLength)
	{
		if (version == 0)
		{
			return EncodeUncompressedHeaderV0(target);
		}
		throw UnexpectedVersion(version);
	}

	private static int EncodeUncompressedHeaderV0(Span<byte> target)
	{
		target[0] = 0;
		return 1;
	}

	private static int EncodeCompressedHeader(Span<byte> target, int version, int headerSize, int sourceLength, int encodedLength)
	{
		if (version == 0)
		{
			return EncodeCompressedHeaderV0(target, headerSize, sourceLength, encodedLength);
		}
		throw UnexpectedVersion(version);
	}

	private static int EncodeCompressedHeaderV0(Span<byte> target, int headerSize, int sourceLength, int encodedLength)
	{
		int value = sourceLength - encodedLength;
		int num = headerSize - 1;
		target[0] = EncodeHeaderByteV0(num);
		PokeN(target.Slice(1), value, num);
		return 1 + num;
	}

	private unsafe static void PokeN(Span<byte> target, int value, int size)
	{
		if (size < 0 || size > 4 || target.Length < size)
		{
			throw new ArgumentException($"Unexpected size: {size}");
		}
		Unsafe.CopyBlockUnaligned(ref target[0], ref *(byte*)(&value), (uint)size);
	}

	private static byte EncodeHeaderByteV0(int sizeOfDiff)
	{
		return (byte)(0u | (uint)((EncodeSizeOf(sizeOfDiff) & 3) << 6));
	}

	private static int EffectiveSizeOf(int value)
	{
		if (value > 255)
		{
			if (value <= 65535)
			{
				return 2;
			}
		}
		else if (value >= 0)
		{
			return 1;
		}
		return 4;
	}

	private static int EncodeSizeOf(int size)
	{
		if (size == 4)
		{
			return 3;
		}
		return size;
	}

	private static Exception UnexpectedVersion(int version)
	{
		return new ArgumentException($"Unexpected pickle version: {version}");
	}

	public static byte[] Unpickle(byte[] source)
	{
		return Unpickle(source.AsSpan());
	}

	public static byte[] Unpickle(byte[] source, int index, int count)
	{
		return Unpickle(source.AsSpan(index, count));
	}

	public unsafe static byte[] Unpickle(byte* source, int count)
	{
		return Unpickle(new Span<byte>(source, count));
	}

	public static byte[] Unpickle(ReadOnlySpan<byte> source)
	{
		if (source.Length == 0)
		{
			return Mem.Empty;
		}
		PickleHeader header = DecodeHeader(source);
		int num = UnpickledSize(in header);
		if (num == 0)
		{
			return Mem.Empty;
		}
		byte[] array = new byte[num];
		UnpickleCore(in header, source, array);
		return array;
	}

	public static void Unpickle<TBufferWriter>(ReadOnlySpan<byte> source, TBufferWriter writer) where TBufferWriter : IBufferWriter<byte>
	{
		writer.Required("writer");
		if (source.Length != 0)
		{
			PickleHeader header = DecodeHeader(source);
			int num = UnpickledSize(in header);
			Span<byte> target = writer.GetSpan(num).Slice(0, num);
			UnpickleCore(in header, source, target);
			writer.Advance(num);
		}
	}

	public static void Unpickle(ReadOnlySpan<byte> source, IBufferWriter<byte> writer)
	{
		LZ4Pickler.Unpickle<IBufferWriter<byte>>(source, writer);
	}

	public static int UnpickledSize(ReadOnlySpan<byte> source)
	{
		PickleHeader header = DecodeHeader(source);
		return UnpickledSize(in header);
	}

	private static int UnpickledSize(in PickleHeader header)
	{
		return header.ResultLength;
	}

	public static void Unpickle(ReadOnlySpan<byte> source, Span<byte> output)
	{
		if (source.Length != 0)
		{
			PickleHeader header = DecodeHeader(source);
			UnpickleCore(in header, source, output);
		}
	}

	private static void UnpickleCore(in PickleHeader header, ReadOnlySpan<byte> source, Span<byte> target)
	{
		ReadOnlySpan<byte> source2 = source.Slice(header.DataOffset);
		int num = UnpickledSize(in header);
		int length = target.Length;
		if (length != num)
		{
			throw CorruptedPickle($"Output buffer size ({length}) does not match expected value ({num})");
		}
		if (!header.IsCompressed)
		{
			source2.CopyTo(target);
			return;
		}
		int num2 = LZ4Codec.Decode(source2, target);
		if (num2 == num)
		{
			return;
		}
		throw CorruptedPickle($"Expected to decode {num} bytes but {num2} has been decoded");
	}

	private static PickleHeader DecodeHeader(ReadOnlySpan<byte> source)
	{
		int num = source[0] & 7;
		if (num == 0)
		{
			return DecodeHeaderV0(source);
		}
		throw CorruptedPickle($"Version {num} is not recognized");
	}

	private static PickleHeader DecodeHeaderV0(ReadOnlySpan<byte> source)
	{
		int num = (source[0] >> 6) & 3;
		int num2 = ((num != 3) ? num : 4);
		int num3 = num2;
		ushort num4 = (ushort)(1 + num3);
		int num5 = source.Length - num4;
		if (num5 < 0)
		{
			throw CorruptedPickle($"Unexpected data length: {num5}");
		}
		int num6 = ((num3 != 0) ? PeekN(source.Slice(1), num3) : 0);
		int resultLength = num5 + num6;
		return new PickleHeader(num4, resultLength, num6 != 0);
	}

	private unsafe static int PeekN(ReadOnlySpan<byte> bytes, int size)
	{
		int result = 0;
		if (size < 0 || size > 4 || size > bytes.Length)
		{
			throw CorruptedPickle($"Unexpected field size: {size}");
		}
		fixed (byte* source = bytes)
		{
			Unsafe.CopyBlockUnaligned(&result, source, (uint)size);
		}
		return result;
	}

	private static Exception CorruptedPickle(string message)
	{
		return new InvalidDataException("Pickle is corrupted: " + message);
	}
}
