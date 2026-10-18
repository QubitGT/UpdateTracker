using System;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Encoders;

public class LZ4BlockDecoder : UnmanagedResources, ILZ4Decoder, IDisposable
{
	private PinnedMemory _outputBufferPin;

	private readonly int _outputLength;

	private int _outputIndex;

	private readonly int _blockSize;

	private unsafe byte* OutputBuffer => _outputBufferPin.Pointer;

	public int BlockSize => _blockSize;

	public int BytesReady => _outputIndex;

	public LZ4BlockDecoder(int blockSize)
	{
		blockSize = Mem.RoundUp(Math.Max(blockSize, 1024), 1024);
		_blockSize = blockSize;
		_outputLength = _blockSize + 8;
		_outputIndex = 0;
		PinnedMemory.Alloc(out _outputBufferPin, _outputLength + 8, zero: false);
	}

	public unsafe int Decode(byte* source, int length, int blockSize = 0)
	{
		ThrowIfDisposed();
		if (blockSize <= 0)
		{
			blockSize = _blockSize;
		}
		if (blockSize > _blockSize)
		{
			throw new InvalidOperationException();
		}
		int num = LZ4Codec.Decode(source, length, OutputBuffer, _outputLength);
		if (num < 0)
		{
			throw new InvalidOperationException();
		}
		_outputIndex = num;
		return _outputIndex;
	}

	public unsafe int Inject(byte* source, int length)
	{
		ThrowIfDisposed();
		if (length <= 0)
		{
			return _outputIndex = 0;
		}
		if (length > _outputLength)
		{
			throw new InvalidOperationException();
		}
		Mem.Move(OutputBuffer, source, length);
		_outputIndex = length;
		return length;
	}

	public unsafe void Drain(byte* target, int offset, int length)
	{
		ThrowIfDisposed();
		offset = _outputIndex + offset;
		if (offset < 0 || length < 0 || offset + length > _outputIndex)
		{
			throw new InvalidOperationException();
		}
		Mem.Move(target, OutputBuffer + offset, length);
	}

	public unsafe byte* Peek(int offset)
	{
		ThrowIfDisposed();
		offset = _outputIndex + offset;
		if (offset < 0 || offset > _outputIndex)
		{
			throw new InvalidOperationException();
		}
		return OutputBuffer + offset;
	}

	protected override void ReleaseUnmanaged()
	{
		base.ReleaseUnmanaged();
		_outputBufferPin.Free();
	}
}
