using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct DrawCommandOutputPerBatch : IJobParallelFor
{
	[ReadOnly]
	public BinningConfig binningConfig;

	[ReadOnly]
	public NativeParallelHashMap<uint, BatchID> batchIDs;

	[ReadOnly]
	public GPUInstanceDataBuffer.ReadOnly instanceDataBuffer;

	[ReadOnly]
	public NativeList<DrawBatch> drawBatches;

	[ReadOnly]
	public NativeArray<int> drawInstanceIndices;

	[ReadOnly]
	public CPUInstanceData.ReadOnly instanceData;

	[ReadOnly]
	public NativeArray<byte> rendererVisibilityMasks;

	[ReadOnly]
	public NativeArray<byte> rendererMeshLodSettings;

	[ReadOnly]
	public NativeArray<byte> rendererCrossFadeValues;

	[ReadOnly]
	[DeallocateOnJobCompletion]
	public NativeArray<int> batchBinAllocOffsets;

	[ReadOnly]
	[DeallocateOnJobCompletion]
	public NativeArray<int> batchBinCounts;

	[ReadOnly]
	[DeallocateOnJobCompletion]
	public NativeArray<int> batchDrawCommandOffsets;

	[ReadOnly]
	[DeallocateOnJobCompletion]
	public NativeArray<short> binConfigIndices;

	[ReadOnly]
	[DeallocateOnJobCompletion]
	public NativeArray<int> binVisibleInstanceOffsets;

	[ReadOnly]
	[DeallocateOnJobCompletion]
	public NativeArray<int> binVisibleInstanceCounts;

	[ReadOnly]
	public NativeArray<BatchCullingOutputDrawCommands> cullingOutput;

	[ReadOnly]
	public IndirectBufferLimits indirectBufferLimits;

	[ReadOnly]
	public GraphicsBufferHandle visibleInstancesBufferHandle;

	[ReadOnly]
	public GraphicsBufferHandle indirectArgsBufferHandle;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	public NativeArray<IndirectBufferAllocInfo> indirectBufferAllocInfo;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	public NativeArray<IndirectDrawInfo> indirectDrawInfoGlobalArray;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	public NativeArray<IndirectInstanceInfo> indirectInstanceInfoGlobalArray;

	private int EncodeGPUInstanceIndexAndCrossFade(int rendererIndex, bool negateCrossFade)
	{
		GPUInstanceIndex gPUInstanceIndex = instanceDataBuffer.CPUInstanceToGPUInstance(InstanceHandle.FromInt(rendererIndex));
		int num = rendererCrossFadeValues[rendererIndex];
		if ((long)num == 255)
		{
			return gPUInstanceIndex.index;
		}
		num -= 127;
		if (negateCrossFade)
		{
			num = -num;
		}
		gPUInstanceIndex.index |= num << 24;
		return gPUInstanceIndex.index;
	}

	private bool IsInstanceFlipped(int rendererIndex)
	{
		InstanceHandle instance = InstanceHandle.FromInt(rendererIndex);
		int index = instanceData.InstanceToIndex(instance);
		return instanceData.localToWorldIsFlippedBits.Get(index);
	}

	private bool IsMeshLodVisible(int batchLodLevel, int rendererIndex, bool supportsCrossFade, ref bool negateCrossfade)
	{
		if (batchLodLevel < 0)
		{
			return true;
		}
		byte b = rendererMeshLodSettings[rendererIndex];
		uint num = b & 0xFFFFFF3Fu;
		if (batchLodLevel == num)
		{
			return true;
		}
		if (!supportsCrossFade)
		{
			return false;
		}
		uint num2 = b & 0xC0u;
		if (num2 == 0)
		{
			return false;
		}
		int num3 = (int)(num2 - 128) >> 6;
		negateCrossfade = true;
		return batchLodLevel == num + num3;
	}

	public unsafe void Execute(int batchIndex)
	{
		DrawBatch drawBatch = drawBatches[batchIndex];
		int num = batchBinCounts[batchIndex];
		if (num == 0)
		{
			return;
		}
		BatchCullingOutputDrawCommands batchCullingOutputDrawCommands = cullingOutput[0];
		IndirectBufferAllocInfo indirectBufferAllocInfo = default(IndirectBufferAllocInfo);
		if (indirectBufferLimits.maxDrawCount > 0)
		{
			indirectBufferAllocInfo = this.indirectBufferAllocInfo[0];
		}
		bool flag = !indirectBufferAllocInfo.IsEmpty() && drawBatch.key.range.supportsIndirect;
		int visibilityConfigCount = binningConfig.visibilityConfigCount;
		int* ptr = stackalloc int[visibilityConfigCount];
		for (int i = 0; i < visibilityConfigCount; i++)
		{
			ptr[i] = 0;
		}
		int* ptr2 = stackalloc int[visibilityConfigCount];
		int num2 = batchBinAllocOffsets[batchIndex];
		int num3 = batchDrawCommandOffsets[batchIndex];
		int num4 = 0;
		bool flag2 = drawBatch.key.range.motionMode == MotionVectorGenerationMode.Object || drawBatch.key.range.motionMode == MotionVectorGenerationMode.ForceNoMotion;
		for (int j = 0; j < num; j++)
		{
			int index = num2 + j;
			int num5 = binVisibleInstanceOffsets[index];
			int num6 = binVisibleInstanceCounts[index];
			num4 = num5;
			short num7 = binConfigIndices[index];
			ptr[num7] = num5;
			int num8 = (ptr2[num7] = num3 + j);
			BatchDrawCommandFlags batchDrawCommandFlags = drawBatch.key.flags;
			if (((uint)num7 & (true ? 1u : 0u)) != 0)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.FlipWinding;
			}
			int num9 = num7 >> 1;
			if (binningConfig.supportsCrossFade)
			{
				batchDrawCommandFlags = ((((uint)num9 & (true ? 1u : 0u)) != 0) ? (batchDrawCommandFlags | BatchDrawCommandFlags.LODCrossFadeKeyword) : (batchDrawCommandFlags & ~BatchDrawCommandFlags.LODCrossFadeKeyword));
				num9 >>= 1;
			}
			else
			{
				batchDrawCommandFlags &= ~BatchDrawCommandFlags.LODCrossFadeKeyword;
			}
			if (binningConfig.supportsMotionCheck)
			{
				if ((num9 & 1) != 0 && flag2)
				{
					batchDrawCommandFlags |= BatchDrawCommandFlags.HasMotion;
				}
				num9 >>= 1;
			}
			int sortingPosition = 0;
			if ((batchDrawCommandFlags & BatchDrawCommandFlags.HasSortingPosition) != 0)
			{
				int num10 = num8;
				if (flag)
				{
					num10 += batchCullingOutputDrawCommands.drawCommandCount;
				}
				sortingPosition = 3 * num10;
			}
			if (flag)
			{
				int num11 = indirectBufferAllocInfo.instanceAllocIndex + num5;
				int num12 = indirectBufferAllocInfo.drawAllocIndex + num8;
				indirectDrawInfoGlobalArray[num12] = new IndirectDrawInfo
				{
					indexCount = drawBatch.procInfo.indexCount,
					firstIndex = drawBatch.procInfo.firstIndex,
					baseVertex = drawBatch.procInfo.baseVertex,
					firstInstanceGlobalIndex = (uint)num11,
					maxInstanceCountAndTopology = ((uint)(num6 << 3) | (uint)drawBatch.procInfo.topology)
				};
				batchCullingOutputDrawCommands.indirectDrawCommands[num8] = new BatchDrawCommandIndirect
				{
					flags = batchDrawCommandFlags,
					visibleOffset = (uint)num11,
					batchID = batchIDs[drawBatch.key.overridenComponents],
					materialID = drawBatch.key.materialID,
					splitVisibilityMask = (ushort)num9,
					lightmapIndex = (ushort)drawBatch.key.lightmapIndex,
					sortingPosition = sortingPosition,
					meshID = drawBatch.key.meshID,
					topology = drawBatch.procInfo.topology,
					visibleInstancesBufferHandle = visibleInstancesBufferHandle,
					indirectArgsBufferHandle = indirectArgsBufferHandle,
					indirectArgsBufferOffset = (uint)(num12 * 20)
				};
			}
			else
			{
				batchCullingOutputDrawCommands.drawCommands[num8] = new BatchDrawCommand
				{
					flags = batchDrawCommandFlags,
					visibleOffset = (uint)num5,
					visibleCount = (uint)num6,
					batchID = batchIDs[drawBatch.key.overridenComponents],
					materialID = drawBatch.key.materialID,
					splitVisibilityMask = (ushort)num9,
					lightmapIndex = (ushort)drawBatch.key.lightmapIndex,
					sortingPosition = sortingPosition,
					meshID = drawBatch.key.meshID,
					submeshIndex = (ushort)drawBatch.key.submeshIndex,
					activeMeshLod = (ushort)drawBatch.key.activeMeshLod
				};
			}
		}
		int instanceOffset = drawBatch.instanceOffset;
		int instanceCount = drawBatch.instanceCount;
		bool supportsCrossFade = (drawBatch.key.flags & BatchDrawCommandFlags.LODCrossFadeKeyword) != 0;
		int num13 = 0;
		if (num > 1)
		{
			for (int k = 0; k < instanceCount; k++)
			{
				int num14 = drawInstanceIndices[instanceOffset + k];
				bool flag3 = IsInstanceFlipped(num14);
				int num15 = rendererVisibilityMasks[num14];
				if (num15 == 0)
				{
					continue;
				}
				bool negateCrossfade = false;
				if (!IsMeshLodVisible(drawBatch.key.activeMeshLod, num14, supportsCrossFade, ref negateCrossfade))
				{
					continue;
				}
				num13 = num14;
				int num16 = (num15 << 1) | (flag3 ? 1 : 0);
				int num17 = ptr[num16];
				ptr[num16]++;
				int num18 = EncodeGPUInstanceIndexAndCrossFade(num14, negateCrossfade);
				if (flag)
				{
					if (binningConfig.supportsCrossFade)
					{
						num15 >>= 1;
					}
					if (binningConfig.supportsMotionCheck)
					{
						num15 >>= 1;
					}
					indirectInstanceInfoGlobalArray[indirectBufferAllocInfo.instanceAllocIndex + num17] = new IndirectInstanceInfo
					{
						drawOffsetAndSplitMask = ((ptr2[num16] << 8) | num15),
						instanceIndexAndCrossFade = num18
					};
				}
				else
				{
					batchCullingOutputDrawCommands.visibleInstances[num17] = num18;
				}
			}
		}
		else
		{
			int num19 = num4;
			for (int l = 0; l < instanceCount; l++)
			{
				int num20 = drawInstanceIndices[instanceOffset + l];
				int num21 = rendererVisibilityMasks[num20];
				if (num21 == 0)
				{
					continue;
				}
				bool negateCrossfade2 = false;
				if (!IsMeshLodVisible(drawBatch.key.activeMeshLod, num20, supportsCrossFade, ref negateCrossfade2))
				{
					continue;
				}
				num13 = num20;
				int num22 = EncodeGPUInstanceIndexAndCrossFade(num20, negateCrossfade2);
				if (flag)
				{
					if (binningConfig.supportsCrossFade)
					{
						num21 >>= 1;
					}
					if (binningConfig.supportsMotionCheck)
					{
						num21 >>= 1;
					}
					indirectInstanceInfoGlobalArray[indirectBufferAllocInfo.instanceAllocIndex + num19] = new IndirectInstanceInfo
					{
						drawOffsetAndSplitMask = ((num3 << 8) | num21),
						instanceIndexAndCrossFade = num22
					};
				}
				else
				{
					batchCullingOutputDrawCommands.visibleInstances[num19] = num22;
				}
				num19++;
			}
		}
		if ((drawBatch.key.flags & BatchDrawCommandFlags.HasSortingPosition) != 0)
		{
			InstanceHandle instance = InstanceHandle.FromInt(num13 & 0xFFFFFF);
			int index2 = instanceData.InstanceToIndex(instance);
			float3 center = instanceData.worldAABBs.UnsafeElementAt(index2).center;
			int num23 = num3;
			if (flag)
			{
				num23 += batchCullingOutputDrawCommands.drawCommandCount;
			}
			int num24 = 3 * num23;
			batchCullingOutputDrawCommands.instanceSortingPositions[num24] = center.x;
			batchCullingOutputDrawCommands.instanceSortingPositions[num24 + 1] = center.y;
			batchCullingOutputDrawCommands.instanceSortingPositions[num24 + 2] = center.z;
		}
	}
}
