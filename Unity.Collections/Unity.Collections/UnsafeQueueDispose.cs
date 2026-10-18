using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
internal struct UnsafeQueueDispose
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeQueueData* m_Buffer;

	internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

	public unsafe void Dispose()
	{
		UnsafeQueueData.DeallocateQueue(m_Buffer, m_AllocatorLabel);
	}
}
