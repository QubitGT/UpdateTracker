namespace Unity.Collections.LowLevel.Unsafe;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
internal struct UnsafeParallelHashMapDataDispose
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeParallelHashMapData* m_Buffer;

	internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

	public unsafe void Dispose()
	{
		UnsafeParallelHashMapData.DeallocateHashMap(m_Buffer, m_AllocatorLabel);
	}
}
