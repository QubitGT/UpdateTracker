using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct PrefixSumDrawsAndInstances : IJob
{
	[ReadOnly]
	public NativeList<DrawRange> drawRanges;

	[ReadOnly]
	public NativeArray<int> drawBatchIndices;

	[ReadOnly]
	public NativeArray<int> batchBinAllocOffsets;

	[ReadOnly]
	public NativeArray<int> batchBinCounts;

	[ReadOnly]
	public NativeArray<int> binVisibleInstanceCounts;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[WriteOnly]
	public NativeArray<int> batchDrawCommandOffsets;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[WriteOnly]
	public NativeArray<int> binVisibleInstanceOffsets;

	[NativeDisableUnsafePtrRestriction]
	public NativeArray<BatchCullingOutputDrawCommands> cullingOutput;

	[ReadOnly]
	public IndirectBufferLimits indirectBufferLimits;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	public NativeArray<IndirectBufferAllocInfo> indirectBufferAllocInfo;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	public NativeArray<int> indirectAllocationCounters;

	public unsafe void Execute()
	{
		BatchCullingOutputDrawCommands value = cullingOutput[0];
		bool flag = indirectBufferLimits.maxInstanceCount > 0;
		int num2;
		int num3;
		int num4;
		while (true)
		{
			int num = 0;
			num2 = 0;
			num3 = 0;
			num4 = 0;
			int num5 = 0;
			for (int i = 0; i < drawRanges.Length; i++)
			{
				DrawRange drawRange = drawRanges[i];
				bool flag2 = flag && drawRange.key.supportsIndirect;
				int num6 = 0;
				int drawCommandsBegin = (flag2 ? num4 : num2);
				for (int j = 0; j < drawRange.drawCount; j++)
				{
					int index = drawBatchIndices[drawRange.drawOffset + j];
					int num7 = batchBinAllocOffsets[index];
					int num8 = batchBinCounts[index];
					if (flag2)
					{
						batchDrawCommandOffsets[index] = num4;
						num4 += num8;
					}
					else
					{
						batchDrawCommandOffsets[index] = num2;
						num2 += num8;
					}
					num6 += num8;
					for (int k = 0; k < num8; k++)
					{
						int index2 = num7 + k;
						if (flag2)
						{
							binVisibleInstanceOffsets[index2] = num5;
							num5 += binVisibleInstanceCounts[index2];
						}
						else
						{
							binVisibleInstanceOffsets[index2] = num3;
							num3 += binVisibleInstanceCounts[index2];
						}
					}
				}
				if (num6 != 0)
				{
					RangeKey key = drawRange.key;
					value.drawRanges[num] = new BatchDrawRange
					{
						drawCommandsBegin = (uint)drawCommandsBegin,
						drawCommandsCount = (uint)num6,
						drawCommandsType = (flag2 ? BatchDrawCommandType.Indirect : BatchDrawCommandType.Direct),
						filterSettings = new BatchFilterSettings
						{
							renderingLayerMask = key.renderingLayerMask,
							rendererPriority = key.rendererPriority,
							layer = key.layer,
							batchLayer = (byte)(flag2 ? 28 : 29),
							motionMode = key.motionMode,
							shadowCastingMode = key.shadowCastingMode,
							receiveShadows = true,
							staticShadowCaster = key.staticShadowCaster,
							allDepthSorted = false
						}
					};
					num++;
				}
			}
			value.drawRangeCount = num;
			bool flag3 = true;
			if (flag)
			{
				int* unsafePtr = (int*)indirectAllocationCounters.GetUnsafePtr();
				IndirectBufferAllocInfo value2 = default(IndirectBufferAllocInfo);
				value2.drawCount = num4;
				value2.instanceCount = num5;
				int num9 = value2.drawCount + 1;
				int num10 = Interlocked.Add(ref UnsafeUtility.AsRef<int>(unsafePtr + 1), num9);
				value2.drawAllocIndex = num10 - num9;
				int num11 = Interlocked.Add(ref UnsafeUtility.AsRef<int>(unsafePtr), value2.instanceCount);
				value2.instanceAllocIndex = num11 - value2.instanceCount;
				if (!value2.IsWithinLimits(in indirectBufferLimits))
				{
					value2 = default(IndirectBufferAllocInfo);
					flag3 = false;
				}
				indirectBufferAllocInfo[0] = value2;
			}
			if (flag3)
			{
				break;
			}
			flag = false;
		}
		if (num2 != 0)
		{
			value.drawCommandCount = num2;
			value.drawCommands = MemoryUtilities.Malloc<BatchDrawCommand>(num2, Allocator.TempJob);
			value.visibleInstanceCount = num3;
			value.visibleInstances = MemoryUtilities.Malloc<int>(num3, Allocator.TempJob);
		}
		if (num4 != 0)
		{
			value.indirectDrawCommandCount = num4;
			value.indirectDrawCommands = MemoryUtilities.Malloc<BatchDrawCommandIndirect>(num4, Allocator.TempJob);
		}
		int num12 = num2 + num4;
		value.instanceSortingPositions = MemoryUtilities.Malloc<float>(3 * num12, Allocator.TempJob);
		cullingOutput[0] = value;
	}
}
