using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct NativeQueueDisposeJob : IJob
{
	public NativeQueueDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
