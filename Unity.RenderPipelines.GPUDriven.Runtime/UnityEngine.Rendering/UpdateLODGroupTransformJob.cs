using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct UpdateLODGroupTransformJob : IJobParallelFor
{
	public const int k_BatchSize = 256;

	[ReadOnly]
	public NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash;

	[ReadOnly]
	public NativeArray<int> lodGroupIDs;

	[ReadOnly]
	public NativeArray<Vector3> worldSpaceReferencePoints;

	[ReadOnly]
	public NativeArray<float> worldSpaceSizes;

	[ReadOnly]
	public bool requiresGPUUpload;

	[ReadOnly]
	public bool supportDitheringCrossFade;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[ReadOnly]
	public NativeList<LODGroupData> lodGroupData;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[WriteOnly]
	public NativeList<LODGroupCullingData> lodGroupCullingData;

	[NativeDisableUnsafePtrRestriction]
	public UnsafeAtomicCounter32 atomicUpdateCount;

	public unsafe void Execute(int index)
	{
		int key = lodGroupIDs[index];
		if (!lodGroupDataHash.TryGetValue(key, out var item))
		{
			return;
		}
		float num = worldSpaceSizes[index];
		LODGroupData* ptr = lodGroupData.GetUnsafePtr() + item.index;
		LODGroupCullingData* ptr2 = lodGroupCullingData.GetUnsafePtr() + item.index;
		ptr2->worldSpaceSize = num;
		ptr2->worldSpaceReferencePoint = worldSpaceReferencePoints[index];
		for (int i = 0; i < ptr->lodCount; i++)
		{
			float num2 = ptr->screenRelativeTransitionHeights[i];
			float num3 = LODRenderingUtils.CalculateLODDistance(num2, num);
			ptr2->sqrDistances[i] = num3 * num3;
			if (supportDitheringCrossFade && !ptr2->percentageFlags[i])
			{
				float num4 = ((i != 0) ? ptr->screenRelativeTransitionHeights[i - 1] : 1f);
				float relativeScreenHeight = num2 + ptr->fadeTransitionWidth[i] * (num4 - num2);
				float b = num3 - LODRenderingUtils.CalculateLODDistance(relativeScreenHeight, num);
				b = Mathf.Max(0f, b);
				ptr2->transitionDistances[i] = b;
			}
			else
			{
				ptr2->transitionDistances[i] = 0f;
			}
		}
	}
}
