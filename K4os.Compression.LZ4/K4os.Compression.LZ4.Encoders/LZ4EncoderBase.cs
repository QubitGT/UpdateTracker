using System;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Encoders;

public abstract class LZ4EncoderBase : UnmanagedResources, ILZ4Encoder, IDisposable
{
	private PinnedMemory _inputBufferPin;

	private readonly int _inputLength;

	private readonly int _blockSize;

	private int _inputIndex;

	private int _inputPointer;

	private unsafe byte* InputBuffer => _inputBufferPin.Pointer;

	public int BlockSize => _blockSize;

	public int BytesReady => _inputPointer - _inputIndex;

	protected LZ4EncoderBase(bool chaining, int blockSize, int extraBlocks)
	{
		blockSize = Mem.RoundUp(Math.Max(blockSize, 1024), 1024);
		extraBlocks = Math.Max(extraBlocks, 0);
		int num = (chaining ? 65536 : 0);
		_blockSize = blockSize;
		_inputLength = num + (1 + extraBlocks) * blockSize + 32;
		_inputIndex = (_inputPointer = 0);
		PinnedMemory.Alloc(out _inputBufferPin, _inputLength + 8, zero: false);
	}

	public unsafe int Topup(byte* source, int length)
	{
		ThrowIfDisposed();
		if (length == 0)
		{
			return 0;
		}
		int num = _inputIndex + _blockSize - _inputPointer;
		if (num <= 0)
		{
			return 0;
		}
		int num2 = Math.Min(num, length);
		Mem.Move(InputBuffer + _inputPointer, source, num2);
		_inputPointer += num2;
		return num2;
	}

	public unsafe int Encode(byte* target, int length, bool allowCopy)
	{
		ThrowIfDisposed();
		int num = _inputPointer - _inputIndex;
		if (num <= 0)
		{
			return 0;
		}
		int num2 = EncodeBlock(InputBuffer + _inputIndex, num, target, length);
		if (num2 <= 0)
		{
			throw new InvalidOperationException("Failed to encode chunk. Target buffer too small.");
		}
		if (allowCopy && num2 >= num)
		{
			Mem.Move(target, InputBuffer + _inputIndex, num);
			num2 = -num;
		}
		Commit();
		return num2;
	}

	private unsafe void Commit()
	{
		_inputIndex = _inputPointer;
		if (_inputIndex + _blockSize > _inputLength)
		{
			_inputIndex = (_inputPointer = CopyDict(InputBuffer, _inputPointer));
		}
	}

	protected unsafe abstract int EncodeBlock(byte* source, int sourceLength, byte* target, int targetLength);

	protected unsafe abstract int CopyDict(byte* target, int dictionaryLength);

	protected override void ReleaseUnmanaged()
	{
		base.ReleaseUnmanaged();
		_inputBufferPin.Free();
	}
}
