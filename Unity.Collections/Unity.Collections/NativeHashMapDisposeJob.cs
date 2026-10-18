using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct NativeHashMapDisposeJob : IJob
{
	internal NativeHashMapDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
