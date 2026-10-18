using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct NativeRingQueueDisposeJob : IJob
{
	public NativeRingQueueDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
