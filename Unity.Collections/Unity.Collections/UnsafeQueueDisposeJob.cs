using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct UnsafeQueueDisposeJob : IJob
{
	public UnsafeQueueDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
