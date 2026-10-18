namespace Unity.Collections.LowLevel.Unsafe;

internal struct UntypedUnsafeList
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe readonly void* Ptr;

	internal readonly int m_length;

	internal readonly int m_capacity;

	internal readonly AllocatorManager.AllocatorHandle Allocator;

	internal readonly int padding;
}
