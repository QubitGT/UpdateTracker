using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct UpdatePackedMaterialDataCacheJob : IJob
{
	[ReadOnly]
	public NativeArray<int>.ReadOnly materialIDs;

	[ReadOnly]
	public NativeArray<GPUDrivenPackedMaterialData>.ReadOnly packedMaterialDatas;

	public NativeParallelHashMap<int, GPUDrivenPackedMaterialData> packedMaterialHash;

	private void ProcessMaterial(int i)
	{
		int num = materialIDs[i];
		GPUDrivenPackedMaterialData value = packedMaterialDatas[i];
		if (num != 0)
		{
			packedMaterialHash[num] = value;
		}
	}

	public void Execute()
	{
		for (int i = 0; i < materialIDs.Length; i++)
		{
			ProcessMaterial(i);
		}
	}
}
