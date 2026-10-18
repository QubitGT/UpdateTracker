using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct RegisterNewMaterialsJob : IJobParallelFor
{
	public const int k_BatchSize = 128;

	[ReadOnly]
	public NativeArray<int> instanceIDs;

	[ReadOnly]
	public NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas;

	[ReadOnly]
	public NativeArray<BatchMaterialID> batchIDs;

	[WriteOnly]
	public NativeParallelHashMap<int, BatchMaterialID>.ParallelWriter batchMaterialHashMap;

	[WriteOnly]
	public NativeParallelHashMap<int, GPUDrivenPackedMaterialData>.ParallelWriter packedMaterialHashMap;

	public void Execute(int index)
	{
		int key = instanceIDs[index];
		batchMaterialHashMap.TryAdd(key, batchIDs[index]);
		packedMaterialHashMap.TryAdd(key, packedMaterialDatas[index]);
	}
}
