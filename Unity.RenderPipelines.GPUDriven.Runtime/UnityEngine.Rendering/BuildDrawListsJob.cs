using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct BuildDrawListsJob : IJobParallelFor
{
	public const int k_BatchSize = 128;

	public const int k_IntsPerCacheLine = 16;

	[ReadOnly]
	public NativeParallelHashMap<DrawKey, int> batchHash;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[ReadOnly]
	public NativeList<DrawInstance> drawInstances;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[ReadOnly]
	public NativeList<DrawBatch> drawBatches;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[WriteOnly]
	public NativeArray<int> internalDrawIndex;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[WriteOnly]
	public NativeArray<int> drawInstanceIndices;

	private unsafe static int IncrementCounter(int* counter)
	{
		return Interlocked.Increment(ref UnsafeUtility.AsRef<int>(counter)) - 1;
	}

	public unsafe void Execute(int index)
	{
		ref DrawInstance reference = ref drawInstances.ElementAt(index);
		int num = batchHash[reference.key];
		ref DrawBatch reference2 = ref drawBatches.ElementAt(num);
		int num2 = IncrementCounter((int*)((byte*)internalDrawIndex.GetUnsafePtr() + (nint)(num * 16) * (nint)4));
		int index2 = reference2.instanceOffset + num2;
		drawInstanceIndices[index2] = reference.instanceIndex;
	}
}
