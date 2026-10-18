using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Engine;

public static class Pubternal
{
	public class FastContext : UnmanagedResources
	{
		internal unsafe LL.LZ4_stream_t* Context { get; }

		public unsafe FastContext()
		{
			Context = (LL.LZ4_stream_t*)Mem.AllocZero(sizeof(LL.LZ4_stream_t));
		}

		protected unsafe override void ReleaseUnmanaged()
		{
			Mem.Free(Context);
		}
	}

	public unsafe static int CompressFast(FastContext context, byte* source, byte* target, int sourceLength, int targetLength, int acceleration)
	{
		return LLxx.LZ4_compress_fast_continue(context.Context, source, target, sourceLength, targetLength, acceleration);
	}
}
