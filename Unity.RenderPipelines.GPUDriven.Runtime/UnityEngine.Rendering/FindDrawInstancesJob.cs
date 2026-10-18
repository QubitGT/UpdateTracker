using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct FindDrawInstancesJob : IJobParallelForBatch
{
	public const int k_BatchSize = 128;

	[ReadOnly]
	public NativeArray<InstanceHandle> instancesSorted;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[ReadOnly]
	public NativeList<DrawInstance> drawInstances;

	[WriteOnly]
	public NativeList<int>.ParallelWriter outDrawInstanceIndicesWriter;

	public unsafe void Execute(int startIndex, int count)
	{
		int* ptr = stackalloc int[128];
		int count2 = 0;
		for (int i = startIndex; i < startIndex + count; i++)
		{
			ref DrawInstance reference = ref drawInstances.ElementAt(i);
			if (instancesSorted.BinarySearch(InstanceHandle.FromInt(reference.instanceIndex)) >= 0)
			{
				ptr[count2++] = i;
			}
		}
		outDrawInstanceIndicesWriter.AddRangeNoResize(ptr, count2);
	}
}
