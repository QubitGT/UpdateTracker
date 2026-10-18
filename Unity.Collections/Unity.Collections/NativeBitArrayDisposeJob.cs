using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct NativeBitArrayDisposeJob : IJob
{
	public NativeBitArrayDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
