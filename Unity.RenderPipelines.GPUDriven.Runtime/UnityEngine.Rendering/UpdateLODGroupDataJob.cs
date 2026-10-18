using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct UpdateLODGroupDataJob : IJobParallelFor
{
	public const int k_BatchSize = 256;

	[ReadOnly]
	public NativeArray<GPUInstanceIndex> lodGroupInstances;

	[ReadOnly]
	public GPUDrivenLODGroupData inputData;

	[ReadOnly]
	public bool supportDitheringCrossFade;

	public NativeArray<LODGroupData> lodGroupsData;

	public NativeArray<LODGroupCullingData> lodGroupsCullingData;

	[NativeDisableUnsafePtrRestriction]
	public UnsafeAtomicCounter32 rendererCount;

	public unsafe void Execute(int index)
	{
		GPUInstanceIndex gPUInstanceIndex = lodGroupInstances[index];
		LODFadeMode num = inputData.fadeMode[index];
		int num2 = inputData.lodOffset[index];
		int num3 = inputData.lodCount[index];
		short num4 = inputData.renderersCount[index];
		Vector3 vector = inputData.worldSpaceReferencePoint[index];
		float num5 = inputData.worldSpaceSize[index];
		bool flag = inputData.lastLODIsBillboard[index];
		byte forceLODMask = inputData.forceLODMask[index];
		bool flag2 = num != 0 && supportDitheringCrossFade;
		bool flag3 = num == LODFadeMode.SpeedTree;
		LODGroupData* ptr = (LODGroupData*)((byte*)lodGroupsData.GetUnsafePtr() + (nint)gPUInstanceIndex.index * (nint)sizeof(LODGroupData));
		LODGroupCullingData* ptr2 = (LODGroupCullingData*)((byte*)lodGroupsCullingData.GetUnsafePtr() + (nint)gPUInstanceIndex.index * (nint)sizeof(LODGroupCullingData));
		ptr->valid = true;
		ptr->lodCount = num3;
		ptr->rendererCount = (flag2 ? num4 : 0);
		ptr2->worldSpaceSize = num5;
		ptr2->worldSpaceReferencePoint = vector;
		ptr2->forceLODMask = forceLODMask;
		ptr2->lodCount = num3;
		rendererCount.Add(ptr->rendererCount);
		int num6 = 0;
		if (flag3)
		{
			int index2 = num2 + (num3 - 1);
			bool flag4 = num3 > 0 && inputData.lodRenderersCount[index2] == 1 && flag;
			num6 = ((num3 != 0) ? ((!flag4) ? (num3 - 1) : (Math.Max(num3, 2) - 2)) : 0);
		}
		for (int i = 0; i < num3; i++)
		{
			int num7 = num2 + i;
			float num8 = inputData.lodScreenRelativeTransitionHeight[num7];
			float num9 = LODRenderingUtils.CalculateLODDistance(num8, num5);
			ptr->screenRelativeTransitionHeights[i] = num8;
			ptr->fadeTransitionWidth[i] = 0f;
			ptr2->sqrDistances[i] = num9 * num9;
			ptr2->percentageFlags[i] = false;
			ptr2->transitionDistances[i] = 0f;
			if (flag3 && i < num6)
			{
				ptr2->percentageFlags[i] = true;
			}
			else if (flag2 && i >= num6)
			{
				float num10 = inputData.lodFadeTransitionWidth[num7];
				float num11 = ((i != 0) ? inputData.lodScreenRelativeTransitionHeight[num7 - 1] : 1f);
				float relativeScreenHeight = num8 + num10 * (num11 - num8);
				float b = num9 - LODRenderingUtils.CalculateLODDistance(relativeScreenHeight, num5);
				b = Mathf.Max(0f, b);
				ptr->fadeTransitionWidth[i] = num10;
				ptr2->transitionDistances[i] = b;
			}
		}
	}
}
