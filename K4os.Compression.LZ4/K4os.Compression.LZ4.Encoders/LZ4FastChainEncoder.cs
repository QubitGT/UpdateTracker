using K4os.Compression.LZ4.Engine;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Encoders;

public class LZ4FastChainEncoder : LZ4EncoderBase
{
	private PinnedMemory _contextPin;

	private unsafe LL.LZ4_stream_t* Context => _contextPin.Reference<LL.LZ4_stream_t>();

	public LZ4FastChainEncoder(int blockSize, int extraBlocks = 0)
		: base(chaining: true, blockSize, extraBlocks)
	{
		PinnedMemory.Alloc<LL.LZ4_stream_t>(out _contextPin);
	}

	protected override void ReleaseUnmanaged()
	{
		base.ReleaseUnmanaged();
		_contextPin.Free();
	}

	protected unsafe override int EncodeBlock(byte* source, int sourceLength, byte* target, int targetLength)
	{
		return LLxx.LZ4_compress_fast_continue(Context, source, target, sourceLength, targetLength, 1);
	}

	protected unsafe override int CopyDict(byte* target, int length)
	{
		return LL.LZ4_saveDict(Context, target, length);
	}
}
