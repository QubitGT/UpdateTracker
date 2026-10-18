using System;
using K4os.Compression.LZ4.Engine;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Encoders;

public class LZ4ChainDecoder : UnmanagedResources, ILZ4Decoder, IDisposable
{
	private PinnedMemory _outputBufferPin;

	private PinnedMemory _contextPin;

	private readonly int _blockSize;

	private readonly int _outputLength;

	private int _outputIndex;

	private unsafe byte* OutputBuffer => _outputBufferPin.Pointer;

	private unsafe LL.LZ4_streamDecode_t* Context => _contextPin.Reference<LL.LZ4_streamDecode_t>();

	public int BlockSize => _blockSize;

	public int BytesReady => _outputIndex;

	public LZ4ChainDecoder(int blockSize, int extraBlocks)
	{
		blockSize = Mem.RoundUp(Math.Max(blockSize, 1024), 1024);
		extraBlocks = Math.Max(extraBlocks, 0);
		_blockSize = blockSize;
		_outputLength = 65536 + (1 + extraBlocks) * _blockSize + 32;
		_outputIndex = 0;
		PinnedMemory.Alloc<LL.LZ4_streamDecode_t>(out _contextPin);
		PinnedMemory.Alloc(out _outputBufferPin, _outputLength + 8, zero: false);
	}

	public unsafe int Decode(byte* source, int length, int blockSize)
	{
		if (blockSize <= 0)
		{
			blockSize = _blockSize;
		}
		Prepare(blockSize);
		int num = DecodeBlock(source, length, OutputBuffer + _outputIndex, blockSize);
		if (num < 0)
		{
			throw new InvalidOperationException();
		}
		_outputIndex += num;
		return num;
	}

	public unsafe int Inject(byte* source, int length)
	{
		if (length <= 0)
		{
			return 0;
		}
		if (length > Math.Max(_blockSize, 65536))
		{
			throw new InvalidOperationException();
		}
		byte* outputBuffer = OutputBuffer;
		if (_outputIndex + length < _outputLength)
		{
			Mem.Move(outputBuffer + _outputIndex, source, length);
			_outputIndex = ApplyDict(_outputIndex + length);
		}
		else if (length >= 65536)
		{
			Mem.Move(outputBuffer, source, length);
			_outputIndex = ApplyDict(length);
		}
		else
		{
			int num = Math.Min(65536 - length, _outputIndex);
			Mem.Move(outputBuffer, outputBuffer + _outputIndex - num, num);
			Mem.Move(outputBuffer + num, source, length);
			_outputIndex = ApplyDict(num + length);
		}
		return length;
	}

	public unsafe void Drain(byte* target, int offset, int length)
	{
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

	private void Prepare(int blockSize)
	{
		if (_outputIndex + blockSize > _outputLength)
		{
			_outputIndex = CopyDict(_outputIndex);
		}
	}

	private unsafe int CopyDict(int index)
	{
		int num = Math.Max(index - 65536, 0);
		int num2 = index - num;
		Mem.Move(OutputBuffer, OutputBuffer + num, num2);
		LL.LZ4_setStreamDecode(Context, OutputBuffer, num2);
		return num2;
	}

	private unsafe int ApplyDict(int index)
	{
		int num = Math.Max(index - 65536, 0);
		int dictSize = index - num;
		LL.LZ4_setStreamDecode(Context, OutputBuffer + num, dictSize);
		return index;
	}

	private unsafe int DecodeBlock(byte* source, int sourceLength, byte* target, int targetLength)
	{
		return LLxx.LZ4_decompress_safe_continue(Context, source, target, sourceLength, targetLength);
	}

	protected override void ReleaseUnmanaged()
	{
		base.ReleaseUnmanaged();
		_contextPin.Free();
		_outputBufferPin.Free();
	}
}
