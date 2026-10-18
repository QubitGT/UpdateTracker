using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct FindNonRegisteredMeshesJob : IJobParallelForBatch
{
	public const int k_BatchSize = 128;

	[ReadOnly]
	public NativeArray<int> instanceIDs;

	[ReadOnly]
	public NativeParallelHashMap<int, BatchMeshID> hashMap;

	[WriteOnly]
	public NativeList<int>.ParallelWriter outInstancesWriter;

	public unsafe void Execute(int startIndex, int count)
	{
		int* ptr = stackalloc int[128];
		UnsafeList<int> unsafeList = new UnsafeList<int>(ptr, 128);
		unsafeList.Length = 0;
		for (int i = startIndex; i < startIndex + count; i++)
		{
			int num = instanceIDs[i];
			if (!hashMap.ContainsKey(num))
			{
				unsafeList.AddNoResize(num);
			}
		}
		outInstancesWriter.AddRangeNoResize(ptr, unsafeList.Length);
	}
}
