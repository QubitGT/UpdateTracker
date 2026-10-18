using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
internal struct UnsafeDisposeJob : IJob
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe void* Ptr;

	public AllocatorManager.AllocatorHandle Allocator;

	public unsafe void Execute()
	{
		AllocatorManager.Free(Allocator, Ptr);
	}
}
