using System.Collections.Concurrent;

namespace VYaml.Internal;

internal static class ReusableByteSequenceBuilderPool
{
	private static readonly ConcurrentQueue<ReusableByteSequenceBuilder> queue = new ConcurrentQueue<ReusableByteSequenceBuilder>();

	public static ReusableByteSequenceBuilder Rent()
	{
		if (queue.TryDequeue(out ReusableByteSequenceBuilder result))
		{
			return result;
		}
		return new ReusableByteSequenceBuilder();
	}

	public static void Return(ReusableByteSequenceBuilder builder)
	{
		builder.Reset();
		queue.Enqueue(builder);
	}
}
