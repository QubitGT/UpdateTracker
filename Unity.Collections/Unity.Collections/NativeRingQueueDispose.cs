using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
internal struct NativeRingQueueDispose
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeRingQueue<int>* m_QueueData;

	public unsafe void Dispose()
	{
		UnsafeRingQueue<int>.Free(m_QueueData);
	}
}
