using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
internal struct UnsafeParallelHashMapDataDisposeJob : IJob
{
	internal UnsafeParallelHashMapDataDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
