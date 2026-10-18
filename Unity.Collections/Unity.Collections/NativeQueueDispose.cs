using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
internal struct NativeQueueDispose
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeQueue<int>* m_QueueData;

	public unsafe void Dispose()
	{
		UnsafeQueue<int>.Free(m_QueueData);
	}
}
