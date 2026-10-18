using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct FindNonRegisteredMaterialsJob : IJobParallelForBatch
{
	public const int k_BatchSize = 128;

	[ReadOnly]
	public NativeArray<int> instanceIDs;

	[ReadOnly]
	public NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas;

	[ReadOnly]
	public NativeParallelHashMap<int, BatchMaterialID> hashMap;

	[WriteOnly]
	public NativeList<int>.ParallelWriter outInstancesWriter;

	[WriteOnly]
	public NativeList<GPUDrivenPackedMaterialData>.ParallelWriter outPackedMaterialDatasWriter;

	public unsafe void Execute(int startIndex, int count)
	{
		int* ptr = stackalloc int[128];
		UnsafeList<int> unsafeList = new UnsafeList<int>(ptr, 128);
		GPUDrivenPackedMaterialData* ptr2 = stackalloc GPUDrivenPackedMaterialData[128];
		UnsafeList<GPUDrivenPackedMaterialData> unsafeList2 = new UnsafeList<GPUDrivenPackedMaterialData>(ptr2, 128);
		unsafeList.Length = 0;
		unsafeList2.Length = 0;
		for (int i = startIndex; i < startIndex + count; i++)
		{
			int num = instanceIDs[i];
			if (!hashMap.ContainsKey(num))
			{
				unsafeList.AddNoResize(num);
				unsafeList2.AddNoResize(packedMaterialDatas[i]);
			}
		}
		outInstancesWriter.AddRangeNoResize(ptr, unsafeList.Length);
		outPackedMaterialDatasWriter.AddRangeNoResize(ptr2, unsafeList2.Length);
	}
}
