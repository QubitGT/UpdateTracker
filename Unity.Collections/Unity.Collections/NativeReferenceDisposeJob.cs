using Unity.Burst;
using Unity.Jobs;

namespace Unity.Collections;

[BurstCompile]
internal struct NativeReferenceDisposeJob : IJob
{
	internal NativeReferenceDispose Data;

	public void Execute()
	{
		Data.Dispose();
	}
}
