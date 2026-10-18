using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace VYaml.Internal;

internal class ReusableByteSequenceSegment : ReadOnlySequenceSegment<byte>
{
	private bool returnToPool;

	public ReusableByteSequenceSegment()
	{
		returnToPool = false;
	}

	public void SetBuffer(ReadOnlyMemory<byte> buffer, bool returnToPool)
	{
		base.Memory = buffer;
		this.returnToPool = returnToPool;
	}

	public void Reset()
	{
		if (returnToPool && MemoryMarshal.TryGetArray(base.Memory, out var segment) && segment.Array != null)
		{
			ArrayPool<byte>.Shared.Return(segment.Array);
		}
		base.Memory = default(ReadOnlyMemory<byte>);
		base.RunningIndex = 0L;
		base.Next = null;
	}

	public void SetRunningIndexAndNext(long runningIndex, ReusableByteSequenceSegment? nextSegment)
	{
		base.RunningIndex = runningIndex;
		base.Next = nextSegment;
	}
}
