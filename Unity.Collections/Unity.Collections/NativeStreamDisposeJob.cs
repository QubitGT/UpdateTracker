using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct NativeStreamDisposeJob : IJob
{
	public NativeStreamDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
