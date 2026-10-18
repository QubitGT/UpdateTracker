using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
[GenerateTestsForBurstCompatibility]
internal struct NativeListDisposeJob : IJob
{
	internal NativeListDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
