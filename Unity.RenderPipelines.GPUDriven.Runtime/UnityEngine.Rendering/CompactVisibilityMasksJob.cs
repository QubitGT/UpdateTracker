using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct CompactVisibilityMasksJob : IJobParallelForBatch
{
	public const int k_BatchSize = 64;

	[ReadOnly]
	public NativeArray<byte> rendererVisibilityMasks;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	public ParallelBitArray compactedVisibilityMasks;

	public void Execute(int startIndex, int count)
	{
		ulong num = 0uL;
		for (int i = 0; i < count; i++)
		{
			if (rendererVisibilityMasks[startIndex + i] != 0)
			{
				num |= (ulong)(1L << i);
			}
		}
		int chunk_index = startIndex / 64;
		compactedVisibilityMasks.InterlockedOrChunk(chunk_index, num);
	}
}
