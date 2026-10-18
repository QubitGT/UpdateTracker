using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
internal struct UnsafeParallelHashMapDisposeJob : IJob
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeParallelHashMapData* Data;

	public AllocatorManager.AllocatorHandle Allocator;

	public unsafe void Execute()
	{
		UnsafeParallelHashMapData.DeallocateHashMap(Data, Allocator);
	}
}
