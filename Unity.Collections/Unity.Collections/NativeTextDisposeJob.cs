using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct NativeTextDisposeJob : IJob
{
	public NativeTextDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
