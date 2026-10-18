using System;

namespace Fusion;

internal abstract class NetworkBufferSerializer
{
	protected const int DATA_BLOCK_SIZE = 6;

	protected const int OFFSET_BLOCK_SIZE = 4;

	public abstract int Read(Simulation.RecvContext rc, NetworkObjectMeta meta, NetworkBufferSerializerInfo info, Span<int> ptr, int word);

	public abstract int Write(Simulation.SendContext sc, NetworkObjectMeta meta, NetworkBufferSerializerInfo info, Span<int> ptr, int word, int prev);

	public abstract int Skip(Simulation.RecvContext rc, int word);
}
