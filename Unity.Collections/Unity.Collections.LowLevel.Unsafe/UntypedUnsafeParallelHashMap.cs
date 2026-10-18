namespace Unity.Collections.LowLevel.Unsafe;

public struct UntypedUnsafeParallelHashMap
{
	[NativeDisableUnsafePtrRestriction]
	private unsafe UnsafeParallelHashMapData* m_Buffer;

	private AllocatorManager.AllocatorHandle m_AllocatorLabel;
}
