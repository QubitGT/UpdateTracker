using K4os.Compression.LZ4.Engine;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Encoders;

public class LZ4HighChainEncoder : LZ4EncoderBase
{
	private PinnedMemory _contextPin;

	private unsafe LL.LZ4_streamHC_t* Context => _contextPin.Reference<LL.LZ4_streamHC_t>();

	public unsafe LZ4HighChainEncoder(LZ4Level level, int blockSize, int extraBlocks = 0)
		: base(chaining: true, blockSize, extraBlocks)
	{
		if (level < LZ4Level.L03_HC)
		{
			level = LZ4Level.L03_HC;
		}
		if (level > LZ4Level.L12_MAX)
		{
			level = LZ4Level.L12_MAX;
		}
		PinnedMemory.Alloc<LL.LZ4_streamHC_t>(out _contextPin, zero: false);
		LL.LZ4_initStreamHC(Context);
		LL.LZ4_resetStreamHC_fast(Context, (int)level);
	}

	protected override void ReleaseUnmanaged()
	{
		base.ReleaseUnmanaged();
		_contextPin.Free();
	}

	protected unsafe override int EncodeBlock(byte* source, int sourceLength, byte* target, int targetLength)
	{
		return LLxx.LZ4_compress_HC_continue(Context, source, target, sourceLength, targetLength);
	}

	protected unsafe override int CopyDict(byte* target, int length)
	{
		return LL.LZ4_saveDictHC(Context, target, length);
	}
}
