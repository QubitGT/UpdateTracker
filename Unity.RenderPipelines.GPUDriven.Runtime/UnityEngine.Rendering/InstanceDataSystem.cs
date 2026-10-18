using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal class InstanceDataSystem : IDisposable
{
	private static class InstanceTransformUpdateIDs
	{
		public static readonly int _TransformUpdateQueueCount = Shader.PropertyToID("_TransformUpdateQueueCount");

		public static readonly int _TransformUpdateOutputL2WVec4Offset = Shader.PropertyToID("_TransformUpdateOutputL2WVec4Offset");

		public static readonly int _TransformUpdateOutputW2LVec4Offset = Shader.PropertyToID("_TransformUpdateOutputW2LVec4Offset");

		public static readonly int _TransformUpdateOutputPrevL2WVec4Offset = Shader.PropertyToID("_TransformUpdateOutputPrevL2WVec4Offset");

		public static readonly int _TransformUpdateOutputPrevW2LVec4Offset = Shader.PropertyToID("_TransformUpdateOutputPrevW2LVec4Offset");

		public static readonly int _BoundingSphereOutputVec4Offset = Shader.PropertyToID("_BoundingSphereOutputVec4Offset");

		public static readonly int _TransformUpdateDataQueue = Shader.PropertyToID("_TransformUpdateDataQueue");

		public static readonly int _TransformUpdateIndexQueue = Shader.PropertyToID("_TransformUpdateIndexQueue");

		public static readonly int _BoundingSphereDataQueue = Shader.PropertyToID("_BoundingSphereDataQueue");

		public static readonly int _OutputTransformBuffer = Shader.PropertyToID("_OutputTransformBuffer");

		public static readonly int _ProbeUpdateQueueCount = Shader.PropertyToID("_ProbeUpdateQueueCount");

		public static readonly int _SHUpdateVec4Offset = Shader.PropertyToID("_SHUpdateVec4Offset");

		public static readonly int _ProbeUpdateDataQueue = Shader.PropertyToID("_ProbeUpdateDataQueue");

		public static readonly int _ProbeOcclusionUpdateDataQueue = Shader.PropertyToID("_ProbeOcclusionUpdateDataQueue");

		public static readonly int _ProbeUpdateIndexQueue = Shader.PropertyToID("_ProbeUpdateIndexQueue");

		public static readonly int _OutputProbeBuffer = Shader.PropertyToID("_OutputProbeBuffer");
	}

	private static class InstanceWindDataUpdateIDs
	{
		public static readonly int _WindDataQueueCount = Shader.PropertyToID("_WindDataQueueCount");

		public static readonly int _WindDataUpdateIndexQueue = Shader.PropertyToID("_WindDataUpdateIndexQueue");

		public static readonly int _WindDataBuffer = Shader.PropertyToID("_WindDataBuffer");

		public static readonly int _WindParamAddressArray = Shader.PropertyToID("_WindParamAddressArray");

		public static readonly int _WindHistoryParamAddressArray = Shader.PropertyToID("_WindHistoryParamAddressArray");
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct QueryRendererGroupInstancesCountJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 128;

		[ReadOnly]
		public CPUInstanceData instanceData;

		[ReadOnly]
		public CPUSharedInstanceData sharedInstanceData;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[ReadOnly]
		public NativeArray<int> rendererGroupIDs;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[WriteOnly]
		public NativeArray<int> instancesCount;

		public void Execute(int startIndex, int count)
		{
			for (int i = startIndex; i < startIndex + count; i++)
			{
				int key = rendererGroupIDs[i];
				if (rendererGroupInstanceMultiHash.TryGetFirstValue(key, out var item, out var _))
				{
					SharedInstanceHandle instance = instanceData.Get_SharedInstance(item);
					int value = sharedInstanceData.Get_RefCount(instance);
					instancesCount[i] = value;
				}
				else
				{
					instancesCount[i] = 0;
				}
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct ComputeInstancesOffsetAndResizeInstancesArrayJob : IJob
	{
		[ReadOnly]
		public NativeArray<int> instancesCount;

		[WriteOnly]
		public NativeArray<int> instancesOffset;

		public NativeList<InstanceHandle> instances;

		public void Execute()
		{
			int num = 0;
			for (int i = 0; i < instancesCount.Length; i++)
			{
				instancesOffset[i] = num;
				num += instancesCount[i];
			}
			instances.ResizeUninitialized(num);
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct QueryRendererGroupInstancesJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 128;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[ReadOnly]
		public NativeArray<int> rendererGroupIDs;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[WriteOnly]
		public NativeArray<InstanceHandle> instances;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 atomicNonFoundInstancesCount;

		public unsafe void Execute(int startIndex, int count)
		{
			int num = 0;
			for (int i = startIndex; i < startIndex + count; i++)
			{
				if (rendererGroupInstanceMultiHash.TryGetFirstValue(rendererGroupIDs[i], out var item, out var _))
				{
					instances[i] = item;
					continue;
				}
				num++;
				instances[i] = InstanceHandle.Invalid;
			}
			if (atomicNonFoundInstancesCount.Counter != null && num > 0)
			{
				atomicNonFoundInstancesCount.Add(num);
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct QueryRendererGroupInstancesMultiJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 128;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[ReadOnly]
		public NativeArray<int> rendererGroupIDs;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[ReadOnly]
		public NativeArray<int> instancesOffsets;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[ReadOnly]
		public NativeArray<int> instancesCounts;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[WriteOnly]
		public NativeArray<InstanceHandle> instances;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 atomicNonFoundSharedInstancesCount;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 atomicNonFoundInstancesCount;

		public unsafe void Execute(int startIndex, int count)
		{
			int num = 0;
			int num2 = 0;
			for (int i = startIndex; i < startIndex + count; i++)
			{
				int key = rendererGroupIDs[i];
				int num3 = instancesOffsets[i];
				int num4 = instancesCounts[i];
				InstanceHandle item;
				NativeParallelMultiHashMapIterator<int> it;
				bool flag = rendererGroupInstanceMultiHash.TryGetFirstValue(key, out item, out it);
				if (!flag)
				{
					num++;
				}
				for (int j = 0; j < num4; j++)
				{
					int index = num3 + j;
					if (flag)
					{
						instances[index] = item;
						flag = rendererGroupInstanceMultiHash.TryGetNextValue(out item, ref it);
					}
					else
					{
						num2++;
						instances[index] = InstanceHandle.Invalid;
					}
				}
			}
			if (atomicNonFoundSharedInstancesCount.Counter != null && num > 0)
			{
				atomicNonFoundSharedInstancesCount.Add(num);
			}
			if (atomicNonFoundInstancesCount.Counter != null && num2 > 0)
			{
				atomicNonFoundInstancesCount.Add(num2);
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct QuerySortedMeshInstancesJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 64;

		[ReadOnly]
		public CPUInstanceData instanceData;

		[ReadOnly]
		public CPUSharedInstanceData sharedInstanceData;

		[ReadOnly]
		public NativeArray<int> sortedMeshID;

		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeList<InstanceHandle> instances;

		public void Execute(int startIndex, int count)
		{
			ulong num = 0uL;
			for (int i = 0; i < count; i++)
			{
				int index = startIndex + i;
				_ = instanceData.instances[index];
				SharedInstanceHandle instance = instanceData.sharedInstances[index];
				int value = sharedInstanceData.Get_MeshID(instance);
				if (sortedMeshID.BinarySearch(value) >= 0)
				{
					num |= (ulong)(1L << i);
				}
			}
			int num2 = math.countbits(num);
			if (num2 > 0)
			{
				int num3 = AtomicAddLengthNoResize(in instances, num2);
				int num4 = math.tzcnt(num);
				while (num != 0L)
				{
					int index2 = startIndex + num4;
					instances[num3] = instanceData.instances[index2];
					num3++;
					num &= (ulong)(~(1L << num4));
					num4 = math.tzcnt(num);
				}
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct CalculateInterpolatedLightAndOcclusionProbesBatchJob : IJobParallelFor
	{
		public const int k_BatchSize = 1;

		public const int k_CalculatedProbesPerBatch = 8;

		[ReadOnly]
		public int probesCount;

		[ReadOnly]
		public LightProbesQuery lightProbesQuery;

		[NativeDisableParallelForRestriction]
		[ReadOnly]
		public NativeArray<Vector3> queryPostitions;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> compactTetrahedronCache;

		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<SphericalHarmonicsL2> probesSphericalHarmonics;

		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<Vector4> probesOcclusion;

		public void Execute(int index)
		{
			int num = index * 8;
			int length = math.min(probesCount, num + 8) - num;
			NativeArray<int> subArray = compactTetrahedronCache.GetSubArray(num, length);
			NativeArray<Vector3> subArray2 = queryPostitions.GetSubArray(num, length);
			NativeArray<SphericalHarmonicsL2> subArray3 = probesSphericalHarmonics.GetSubArray(num, length);
			NativeArray<Vector4> subArray4 = probesOcclusion.GetSubArray(num, length);
			lightProbesQuery.CalculateInterpolatedLightAndOcclusionProbes(subArray2, subArray, subArray3, subArray4);
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct ScatterTetrahedronCacheIndicesJob : IJobParallelFor
	{
		public const int k_BatchSize = 128;

		[ReadOnly]
		public NativeArray<InstanceHandle> probeInstances;

		[ReadOnly]
		public NativeArray<int> compactTetrahedronCache;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[NativeDisableParallelForRestriction]
		public CPUInstanceData instanceData;

		public void Execute(int index)
		{
			InstanceHandle instance = probeInstances[index];
			instanceData.Set_TetrahedronCacheIndex(instance, compactTetrahedronCache[index]);
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct TransformUpdateJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 64;

		[ReadOnly]
		public bool initialize;

		[ReadOnly]
		public bool enableBoundingSpheres;

		[ReadOnly]
		public NativeArray<InstanceHandle> instances;

		[ReadOnly]
		public NativeArray<Matrix4x4> localToWorldMatrices;

		[ReadOnly]
		public NativeArray<Matrix4x4> prevLocalToWorldMatrices;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 atomicTransformQueueCount;

		[NativeDisableParallelForRestriction]
		public CPUSharedInstanceData sharedInstanceData;

		[NativeDisableParallelForRestriction]
		public CPUInstanceData instanceData;

		[NativeDisableParallelForRestriction]
		public NativeArray<InstanceHandle> transformUpdateInstanceQueue;

		[NativeDisableParallelForRestriction]
		public NativeArray<TransformUpdatePacket> transformUpdateDataQueue;

		[NativeDisableParallelForRestriction]
		public NativeArray<float4> boundingSpheresDataQueue;

		public unsafe void Execute(int startIndex, int count)
		{
			ulong num = 0uL;
			for (int i = 0; i < count; i++)
			{
				InstanceHandle instance = instances[startIndex + i];
				if (!instance.valid)
				{
					continue;
				}
				if (!initialize)
				{
					int index = instanceData.InstanceToIndex(instance);
					int index2 = sharedInstanceData.InstanceToIndex(in instanceData, instance);
					TransformUpdateFlags transformUpdateFlags = sharedInstanceData.flags[index2].transformUpdateFlags;
					bool flag = instanceData.movedInCurrentFrameBits.Get(index);
					if ((transformUpdateFlags & TransformUpdateFlags.IsPartOfStaticBatch) != 0 || flag)
					{
						continue;
					}
				}
				num |= (ulong)(1L << i);
			}
			int num2 = math.countbits(num);
			if (num2 <= 0)
			{
				return;
			}
			int num3 = atomicTransformQueueCount.Add(num2);
			int num4 = math.tzcnt(num);
			while (num != 0L)
			{
				int index3 = startIndex + num4;
				InstanceHandle instanceHandle = instances[index3];
				int index4 = instanceData.InstanceToIndex(instanceHandle);
				int index5 = sharedInstanceData.InstanceToIndex(in instanceData, instanceHandle);
				bool flag2 = (sharedInstanceData.flags[index5].transformUpdateFlags & TransformUpdateFlags.IsPartOfStaticBatch) != 0;
				instanceData.movedInCurrentFrameBits.Set(index4, !flag2);
				transformUpdateInstanceQueue[num3] = instanceHandle;
				ref float4x4 reference = ref UnsafeUtility.ArrayElementAsRef<float4x4>(localToWorldMatrices.GetUnsafeReadOnlyPtr(), index3);
				ref AABB reference2 = ref UnsafeUtility.ArrayElementAsRef<AABB>(sharedInstanceData.localAABBs.GetUnsafePtr(), index5);
				AABB value = AABB.Transform(reference, reference2);
				instanceData.worldAABBs[index4] = value;
				if (initialize)
				{
					PackedMatrix packedMatrix = PackedMatrix.FromFloat4x4(in reference);
					Matrix4x4 m = prevLocalToWorldMatrices[index3];
					PackedMatrix packedMatrix2 = PackedMatrix.FromMatrix4x4(in m);
					transformUpdateDataQueue[num3 * 2] = new TransformUpdatePacket
					{
						localToWorld0 = packedMatrix.packed0,
						localToWorld1 = packedMatrix.packed1,
						localToWorld2 = packedMatrix.packed2
					};
					transformUpdateDataQueue[num3 * 2 + 1] = new TransformUpdatePacket
					{
						localToWorld0 = packedMatrix2.packed0,
						localToWorld1 = packedMatrix2.packed1,
						localToWorld2 = packedMatrix2.packed2
					};
				}
				else
				{
					Matrix4x4 m = reference;
					PackedMatrix packedMatrix3 = PackedMatrix.FromMatrix4x4(in m);
					transformUpdateDataQueue[num3] = new TransformUpdatePacket
					{
						localToWorld0 = packedMatrix3.packed0,
						localToWorld1 = packedMatrix3.packed1,
						localToWorld2 = packedMatrix3.packed2
					};
					float num5 = math.determinant((float3x3)reference);
					instanceData.localToWorldIsFlippedBits.Set(index4, num5 < 0f);
				}
				if (enableBoundingSpheres)
				{
					boundingSpheresDataQueue[num3] = new float4(value.center.x, value.center.y, value.center.z, math.distance(value.max, value.min) * 0.5f);
				}
				num3++;
				num &= (ulong)(~(1L << num4));
				num4 = math.tzcnt(num);
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct ProbesUpdateJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 64;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[ReadOnly]
		public NativeArray<InstanceHandle> instances;

		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public CPUInstanceData instanceData;

		[ReadOnly]
		public CPUSharedInstanceData sharedInstanceData;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 atomicProbesQueueCount;

		[NativeDisableParallelForRestriction]
		public NativeArray<InstanceHandle> probeInstanceQueue;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> compactTetrahedronCache;

		[NativeDisableParallelForRestriction]
		public NativeArray<Vector3> probeQueryPosition;

		public unsafe void Execute(int startIndex, int count)
		{
			ulong num = 0uL;
			for (int i = 0; i < count; i++)
			{
				InstanceHandle instance = instances[startIndex + i];
				if (instance.valid)
				{
					int index = sharedInstanceData.InstanceToIndex(in instanceData, instance);
					if ((sharedInstanceData.flags[index].transformUpdateFlags & TransformUpdateFlags.HasLightProbeCombined) != 0)
					{
						num |= (ulong)(1L << i);
					}
				}
			}
			int num2 = math.countbits(num);
			if (num2 > 0)
			{
				int num3 = atomicProbesQueueCount.Add(num2);
				int num4 = math.tzcnt(num);
				while (num != 0L)
				{
					InstanceHandle instanceHandle = instances[startIndex + num4];
					int index2 = instanceData.InstanceToIndex(instanceHandle);
					ref AABB reference = ref UnsafeUtility.ArrayElementAsRef<AABB>(instanceData.worldAABBs.GetUnsafePtr(), index2);
					probeInstanceQueue[num3] = instanceHandle;
					probeQueryPosition[num3] = reference.center;
					compactTetrahedronCache[num3] = instanceData.tetrahedronCacheIndices[index2];
					num3++;
					num &= (ulong)(~(1L << num4));
					num4 = math.tzcnt(num);
				}
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct MotionUpdateJob : IJobParallelFor
	{
		public const int k_BatchSize = 16;

		[ReadOnly]
		public int queueWriteBase;

		[NativeDisableParallelForRestriction]
		public CPUInstanceData instanceData;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 atomicUpdateQueueCount;

		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<InstanceHandle> transformUpdateInstanceQueue;

		public void Execute(int chunk_index)
		{
			int num = math.min(instanceData.instancesLength - 64 * chunk_index, 64);
			ulong num2 = ulong.MaxValue >> 64 - num;
			ulong num3 = instanceData.movedInCurrentFrameBits.GetChunk(chunk_index) & num2;
			ulong num4 = instanceData.movedInPreviousFrameBits.GetChunk(chunk_index) & num2;
			instanceData.movedInCurrentFrameBits.SetChunk(chunk_index, 0uL);
			instanceData.movedInPreviousFrameBits.SetChunk(chunk_index, num3);
			ulong num5 = num4 & ~num3;
			int num6 = math.countbits(num5);
			int num7 = queueWriteBase;
			if (num6 > 0)
			{
				num7 += atomicUpdateQueueCount.Add(num6);
			}
			for (int num8 = math.tzcnt(num5); num8 < 64; num8 = math.tzcnt(num5))
			{
				int index = 64 * chunk_index + num8;
				transformUpdateInstanceQueue[num7] = instanceData.IndexToInstance(index);
				num7++;
				num5 &= (ulong)(~(1L << num8));
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct UpdateRendererInstancesJob : IJobParallelFor
	{
		public const int k_BatchSize = 128;

		[ReadOnly]
		public bool implicitInstanceIndices;

		[ReadOnly]
		public GPUDrivenRendererGroupData rendererData;

		[ReadOnly]
		public NativeArray<InstanceHandle> instances;

		[ReadOnly]
		public NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataMap;

		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public CPUInstanceData instanceData;

		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public CPUSharedInstanceData sharedInstanceData;

		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public CPUPerCameraInstanceData perCameraInstanceData;

		public unsafe void Execute(int index)
		{
			int rendererGroupID = rendererData.rendererGroupID[index];
			int index2 = rendererData.meshIndex[index];
			GPUDrivenPackedRendererData gPUDrivenPackedRendererData = rendererData.packedRendererData[index];
			int key = rendererData.lodGroupID[index];
			int gameObjectLayer = rendererData.gameObjectLayer[index];
			int num = rendererData.lightmapIndex[index];
			AABB localAABB = rendererData.localBounds[index].ToAABB();
			int num2 = rendererData.materialsOffset[index];
			int num3 = rendererData.materialsCount[index];
			int meshID = rendererData.meshID[index2];
			GPUDrivenMeshLodInfo meshLodInfo = rendererData.meshLodInfo[index2];
			InstanceFlags instanceFlags = InstanceFlags.None;
			TransformUpdateFlags transformUpdateFlags = TransformUpdateFlags.None;
			int num4 = num & 0xFFFF;
			if (num4 >= 65534 && gPUDrivenPackedRendererData.lightProbeUsage == LightProbeUsage.BlendProbes)
			{
				transformUpdateFlags |= TransformUpdateFlags.HasLightProbeCombined;
			}
			if (gPUDrivenPackedRendererData.isPartOfStaticBatch)
			{
				transformUpdateFlags |= TransformUpdateFlags.IsPartOfStaticBatch;
			}
			switch (gPUDrivenPackedRendererData.shadowCastingMode)
			{
			case ShadowCastingMode.Off:
				instanceFlags |= InstanceFlags.IsShadowsOff;
				break;
			case ShadowCastingMode.ShadowsOnly:
				instanceFlags |= InstanceFlags.IsShadowsOnly;
				break;
			}
			if (meshLodInfo.lodSelectionActive)
			{
				instanceFlags |= InstanceFlags.HasMeshLod;
			}
			if (num4 != 65535)
			{
				instanceFlags |= InstanceFlags.AffectsLightmaps;
			}
			if (gPUDrivenPackedRendererData.smallMeshCulling)
			{
				instanceFlags |= InstanceFlags.SmallMeshCulling;
			}
			uint lodGroupAndMask = uint.MaxValue;
			if (lodGroupDataMap.TryGetValue(key, out var item) && gPUDrivenPackedRendererData.lodMask > 0)
			{
				lodGroupAndMask = (uint)((item.index << 8) | gPUDrivenPackedRendererData.lodMask);
			}
			int num5;
			int num6;
			if (implicitInstanceIndices)
			{
				num5 = 1;
				num6 = index;
			}
			else
			{
				num5 = rendererData.instancesCount[index];
				num6 = rendererData.instancesOffset[index];
			}
			if (num5 > 0)
			{
				InstanceHandle instance = instances[num6];
				SharedInstanceHandle instance2 = instanceData.Get_SharedInstance(instance);
				SmallIntegerArray materialIDs = new SmallIntegerArray(num3, Allocator.Persistent);
				for (int i = 0; i < num3; i++)
				{
					int index3 = rendererData.materialIndex[num2 + i];
					int value = rendererData.materialID[index3];
					materialIDs[i] = value;
				}
				sharedInstanceData.Set(instance2, rendererGroupID, in materialIDs, meshID, in localAABB, transformUpdateFlags, instanceFlags, lodGroupAndMask, meshLodInfo, gameObjectLayer, sharedInstanceData.Get_RefCount(instance2));
				for (int j = 0; j < num5; j++)
				{
					int index4 = num6 + j;
					ref Matrix4x4 reference = ref UnsafeUtility.ArrayElementAsRef<Matrix4x4>(rendererData.localToWorldMatrix.GetUnsafeReadOnlyPtr(), index4);
					AABB value2 = AABB.Transform(reference, localAABB);
					instance = instances[index4];
					bool value3 = math.determinant((float3x3)reference) < 0f;
					int num7 = instanceData.InstanceToIndex(instance);
					perCameraInstanceData.SetDefault(num7);
					instanceData.localToWorldIsFlippedBits.Set(num7, value3);
					instanceData.worldAABBs[num7] = value2;
					instanceData.tetrahedronCacheIndices[num7] = -1;
					instanceData.meshLodData[num7] = rendererData.meshLodData[index];
				}
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct CollectInstancesLODGroupsAndMasksJob : IJobParallelFor
	{
		public const int k_BatchSize = 128;

		[ReadOnly]
		public NativeArray<InstanceHandle> instances;

		[ReadOnly]
		public CPUInstanceData.ReadOnly instanceData;

		[ReadOnly]
		public CPUSharedInstanceData.ReadOnly sharedInstanceData;

		[WriteOnly]
		public NativeArray<uint> lodGroupAndMasks;

		public void Execute(int index)
		{
			InstanceHandle instance = instances[index];
			int index2 = sharedInstanceData.InstanceToIndex(in instanceData, instance);
			lodGroupAndMasks[index] = sharedInstanceData.lodGroupAndMasks[index2];
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct GetVisibleNonProcessedTreeInstancesJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 64;

		[ReadOnly]
		public CPUInstanceData instanceData;

		[ReadOnly]
		public CPUSharedInstanceData sharedInstanceData;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public ParallelBitArray compactedVisibilityMasks;

		[ReadOnly]
		public bool becomeVisible;

		[NativeDisableParallelForRestriction]
		public ParallelBitArray processedBits;

		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<int> rendererIDs;

		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<InstanceHandle> instances;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 atomicTreeInstancesCount;

		public void Execute(int startIndex, int count)
		{
			int chunk_index = startIndex / 64;
			ulong chunk = instanceData.visibleInPreviousFrameBits.GetChunk(chunk_index);
			ulong chunk2 = processedBits.GetChunk(chunk_index);
			ulong num = 0uL;
			for (int i = 0; i < count; i++)
			{
				int index = startIndex + i;
				InstanceHandle instanceHandle = instanceData.IndexToInstance(index);
				if (instanceHandle.type != InstanceType.SpeedTree || !compactedVisibilityMasks.Get(instanceHandle.index))
				{
					continue;
				}
				ulong num2 = (ulong)(1L << i);
				if ((chunk2 & num2) != 0)
				{
					continue;
				}
				bool flag = (chunk & num2) != 0;
				if (becomeVisible)
				{
					if (!flag)
					{
						num |= num2;
					}
				}
				else if (flag)
				{
					num |= num2;
				}
			}
			int num3 = math.countbits(num);
			if (num3 > 0)
			{
				processedBits.SetChunk(chunk_index, chunk2 | num);
				int num4 = atomicTreeInstancesCount.Add(num3);
				int num5 = math.tzcnt(num);
				while (num != 0L)
				{
					int index2 = startIndex + num5;
					InstanceHandle instanceHandle2 = instanceData.IndexToInstance(index2);
					SharedInstanceHandle instance = instanceData.Get_SharedInstance(instanceHandle2);
					int value = sharedInstanceData.Get_RendererGroupID(instance);
					rendererIDs[num4] = value;
					instances[num4] = instanceHandle2;
					num4++;
					num &= (ulong)(~(1L << num5));
					num5 = math.tzcnt(num);
				}
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct UpdateCompactedInstanceVisibilityJob : IJobParallelForBatch
	{
		public const int k_BatchSize = 64;

		[ReadOnly]
		public ParallelBitArray compactedVisibilityMasks;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[NativeDisableParallelForRestriction]
		public CPUInstanceData instanceData;

		public void Execute(int startIndex, int count)
		{
			ulong num = 0uL;
			for (int i = 0; i < count; i++)
			{
				int index = startIndex + i;
				InstanceHandle instanceHandle = instanceData.IndexToInstance(index);
				if (compactedVisibilityMasks.Get(instanceHandle.index))
				{
					num |= (ulong)(1L << i);
				}
			}
			instanceData.visibleInPreviousFrameBits.SetChunk(startIndex / 64, num);
		}
	}

	private InstanceAllocators m_InstanceAllocators;

	private CPUSharedInstanceData m_SharedInstanceData;

	private CPUInstanceData m_InstanceData;

	private CPUPerCameraInstanceData m_PerCameraInstanceData;

	private NativeParallelMultiHashMap<int, InstanceHandle> m_RendererGroupInstanceMultiHash;

	private ComputeShader m_TransformUpdateCS;

	private ComputeShader m_WindDataUpdateCS;

	private int m_TransformInitKernel;

	private int m_TransformUpdateKernel;

	private int m_MotionUpdateKernel;

	private int m_ProbeUpdateKernel;

	private int m_LODUpdateKernel;

	private int m_WindDataCopyHistoryKernel;

	private ComputeBuffer m_UpdateIndexQueueBuffer;

	private ComputeBuffer m_ProbeUpdateDataQueueBuffer;

	private ComputeBuffer m_ProbeOcclusionUpdateDataQueueBuffer;

	private ComputeBuffer m_TransformUpdateDataQueueBuffer;

	private ComputeBuffer m_BoundingSpheresUpdateDataQueueBuffer;

	private bool m_EnableBoundingSpheres;

	private readonly int[] m_ScratchWindParamAddressArray = new int[64];

	public bool hasBoundingSpheres => m_EnableBoundingSpheres;

	public CPUInstanceData.ReadOnly instanceData => m_InstanceData.AsReadOnly();

	public CPUPerCameraInstanceData perCameraInstanceData => m_PerCameraInstanceData;

	public int cameraCount => m_PerCameraInstanceData.cameraCount;

	public CPUSharedInstanceData.ReadOnly sharedInstanceData => m_SharedInstanceData.AsReadOnly();

	public NativeArray<InstanceHandle> aliveInstances => m_InstanceData.instances.GetSubArray(0, m_InstanceData.instancesLength);

	public InstanceDataSystem(int maxInstances, bool enableBoundingSpheres, GPUResidentDrawerResources resources)
	{
		m_InstanceAllocators = default(InstanceAllocators);
		m_SharedInstanceData = default(CPUSharedInstanceData);
		m_InstanceData = default(CPUInstanceData);
		m_PerCameraInstanceData = default(CPUPerCameraInstanceData);
		m_InstanceAllocators.Initialize();
		m_SharedInstanceData.Initialize(maxInstances);
		m_InstanceData.Initialize(maxInstances);
		m_PerCameraInstanceData.Initialize(maxInstances);
		m_RendererGroupInstanceMultiHash = new NativeParallelMultiHashMap<int, InstanceHandle>(maxInstances, Allocator.Persistent);
		m_TransformUpdateCS = resources.transformUpdaterKernels;
		m_WindDataUpdateCS = resources.windDataUpdaterKernels;
		m_TransformInitKernel = m_TransformUpdateCS.FindKernel("ScatterInitTransformMain");
		m_TransformUpdateKernel = m_TransformUpdateCS.FindKernel("ScatterUpdateTransformMain");
		m_MotionUpdateKernel = m_TransformUpdateCS.FindKernel("ScatterUpdateMotionMain");
		m_ProbeUpdateKernel = m_TransformUpdateCS.FindKernel("ScatterUpdateProbesMain");
		if (enableBoundingSpheres)
		{
			m_TransformUpdateCS.EnableKeyword("PROCESS_BOUNDING_SPHERES");
		}
		else
		{
			m_TransformUpdateCS.DisableKeyword("PROCESS_BOUNDING_SPHERES");
		}
		m_WindDataCopyHistoryKernel = m_WindDataUpdateCS.FindKernel("WindDataCopyHistoryMain");
		m_EnableBoundingSpheres = enableBoundingSpheres;
	}

	public void Dispose()
	{
		m_InstanceAllocators.Dispose();
		m_SharedInstanceData.Dispose();
		m_InstanceData.Dispose();
		m_PerCameraInstanceData.Dispose();
		m_RendererGroupInstanceMultiHash.Dispose();
		m_UpdateIndexQueueBuffer?.Dispose();
		m_ProbeUpdateDataQueueBuffer?.Dispose();
		m_ProbeOcclusionUpdateDataQueueBuffer?.Dispose();
		m_TransformUpdateDataQueueBuffer?.Dispose();
		m_BoundingSpheresUpdateDataQueueBuffer?.Dispose();
	}

	public int GetMaxInstancesOfType(InstanceType instanceType)
	{
		return m_InstanceAllocators.GetInstanceHandlesLength(instanceType);
	}

	public int GetAliveInstancesOfType(InstanceType instanceType)
	{
		return m_InstanceAllocators.GetInstancesLength(instanceType);
	}

	private void EnsureIndexQueueBufferCapacity(int capacity)
	{
		if (m_UpdateIndexQueueBuffer == null || m_UpdateIndexQueueBuffer.count < capacity)
		{
			m_UpdateIndexQueueBuffer?.Dispose();
			m_UpdateIndexQueueBuffer = new ComputeBuffer(capacity, 4, ComputeBufferType.Raw);
		}
	}

	private void EnsureProbeBuffersCapacity(int capacity)
	{
		EnsureIndexQueueBufferCapacity(capacity);
		if (m_ProbeUpdateDataQueueBuffer == null || m_ProbeUpdateDataQueueBuffer.count < capacity)
		{
			m_ProbeUpdateDataQueueBuffer?.Dispose();
			m_ProbeOcclusionUpdateDataQueueBuffer?.Dispose();
			m_ProbeUpdateDataQueueBuffer = new ComputeBuffer(capacity, Marshal.SizeOf<SHUpdatePacket>(), ComputeBufferType.Structured);
			m_ProbeOcclusionUpdateDataQueueBuffer = new ComputeBuffer(capacity, Marshal.SizeOf<Vector4>(), ComputeBufferType.Structured);
		}
	}

	private void EnsureTransformBuffersCapacity(int capacity)
	{
		EnsureIndexQueueBufferCapacity(capacity);
		int num = capacity * 2;
		if (m_TransformUpdateDataQueueBuffer == null || m_TransformUpdateDataQueueBuffer.count < num)
		{
			m_TransformUpdateDataQueueBuffer?.Dispose();
			m_BoundingSpheresUpdateDataQueueBuffer?.Dispose();
			m_TransformUpdateDataQueueBuffer = new ComputeBuffer(num, Marshal.SizeOf<TransformUpdatePacket>(), ComputeBufferType.Structured);
			if (m_EnableBoundingSpheres)
			{
				m_BoundingSpheresUpdateDataQueueBuffer = new ComputeBuffer(capacity, Marshal.SizeOf<float4>(), ComputeBufferType.Structured);
			}
		}
	}

	private JobHandle ScheduleInterpolateProbesAndUpdateTetrahedronCache(int queueCount, NativeArray<InstanceHandle> probeUpdateInstanceQueue, NativeArray<int> compactTetrahedronCache, NativeArray<Vector3> probeQueryPosition, NativeArray<SphericalHarmonicsL2> probeUpdateDataQueue, NativeArray<Vector4> probeOcclusionUpdateDataQueue)
	{
		LightProbesQuery lightProbesQuery = new LightProbesQuery(Allocator.TempJob);
		CalculateInterpolatedLightAndOcclusionProbesBatchJob calculateInterpolatedLightAndOcclusionProbesBatchJob = default(CalculateInterpolatedLightAndOcclusionProbesBatchJob);
		calculateInterpolatedLightAndOcclusionProbesBatchJob.lightProbesQuery = lightProbesQuery;
		calculateInterpolatedLightAndOcclusionProbesBatchJob.probesCount = queueCount;
		calculateInterpolatedLightAndOcclusionProbesBatchJob.queryPostitions = probeQueryPosition;
		calculateInterpolatedLightAndOcclusionProbesBatchJob.compactTetrahedronCache = compactTetrahedronCache;
		calculateInterpolatedLightAndOcclusionProbesBatchJob.probesSphericalHarmonics = probeUpdateDataQueue;
		calculateInterpolatedLightAndOcclusionProbesBatchJob.probesOcclusion = probeOcclusionUpdateDataQueue;
		CalculateInterpolatedLightAndOcclusionProbesBatchJob jobData = calculateInterpolatedLightAndOcclusionProbesBatchJob;
		int arrayLength = 1 + queueCount / 8;
		JobHandle jobHandle = IJobParallelForExtensions.Schedule(jobData, arrayLength, 1);
		lightProbesQuery.Dispose(jobHandle);
		ScatterTetrahedronCacheIndicesJob jobData2 = default(ScatterTetrahedronCacheIndicesJob);
		jobData2.compactTetrahedronCache = compactTetrahedronCache;
		jobData2.probeInstances = probeUpdateInstanceQueue;
		jobData2.instanceData = m_InstanceData;
		return IJobParallelForExtensions.Schedule(jobData2, queueCount, 128, jobHandle);
	}

	private void DispatchProbeUpdateCommand(int queueCount, NativeArray<InstanceHandle> probeInstanceQueue, NativeArray<SphericalHarmonicsL2> probeUpdateDataQueue, NativeArray<Vector4> probeOcclusionUpdateDataQueue, RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		EnsureProbeBuffersCapacity(queueCount);
		NativeArray<GPUInstanceIndex> nativeArray = new NativeArray<GPUInstanceIndex>(queueCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		outputBuffer.CPUInstanceArrayToGPUInstanceArray(probeInstanceQueue.GetSubArray(0, queueCount), nativeArray);
		m_UpdateIndexQueueBuffer.SetData(nativeArray, 0, 0, queueCount);
		m_ProbeUpdateDataQueueBuffer.SetData(probeUpdateDataQueue, 0, 0, queueCount);
		m_ProbeOcclusionUpdateDataQueueBuffer.SetData(probeOcclusionUpdateDataQueue, 0, 0, queueCount);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._ProbeUpdateQueueCount, queueCount);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._SHUpdateVec4Offset, renderersParameters.shCoefficients.uintOffset);
		m_TransformUpdateCS.SetBuffer(m_ProbeUpdateKernel, InstanceTransformUpdateIDs._ProbeUpdateIndexQueue, m_UpdateIndexQueueBuffer);
		m_TransformUpdateCS.SetBuffer(m_ProbeUpdateKernel, InstanceTransformUpdateIDs._ProbeUpdateDataQueue, m_ProbeUpdateDataQueueBuffer);
		m_TransformUpdateCS.SetBuffer(m_ProbeUpdateKernel, InstanceTransformUpdateIDs._ProbeOcclusionUpdateDataQueue, m_ProbeOcclusionUpdateDataQueueBuffer);
		m_TransformUpdateCS.SetBuffer(m_ProbeUpdateKernel, InstanceTransformUpdateIDs._OutputProbeBuffer, outputBuffer.gpuBuffer);
		m_TransformUpdateCS.Dispatch(m_ProbeUpdateKernel, (queueCount + 63) / 64, 1, 1);
		nativeArray.Dispose();
	}

	private void DispatchMotionUpdateCommand(int motionQueueCount, NativeArray<InstanceHandle> transformInstanceQueue, RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		EnsureTransformBuffersCapacity(motionQueueCount);
		NativeArray<GPUInstanceIndex> nativeArray = new NativeArray<GPUInstanceIndex>(motionQueueCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		outputBuffer.CPUInstanceArrayToGPUInstanceArray(transformInstanceQueue.GetSubArray(0, motionQueueCount), nativeArray);
		m_UpdateIndexQueueBuffer.SetData(nativeArray, 0, 0, motionQueueCount);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateQueueCount, motionQueueCount);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputL2WVec4Offset, renderersParameters.localToWorld.uintOffset);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputW2LVec4Offset, renderersParameters.worldToLocal.uintOffset);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputPrevL2WVec4Offset, renderersParameters.matrixPreviousM.uintOffset);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputPrevW2LVec4Offset, renderersParameters.matrixPreviousMI.uintOffset);
		m_TransformUpdateCS.SetBuffer(m_MotionUpdateKernel, InstanceTransformUpdateIDs._TransformUpdateIndexQueue, m_UpdateIndexQueueBuffer);
		m_TransformUpdateCS.SetBuffer(m_MotionUpdateKernel, InstanceTransformUpdateIDs._OutputTransformBuffer, outputBuffer.gpuBuffer);
		m_TransformUpdateCS.Dispatch(m_MotionUpdateKernel, (motionQueueCount + 63) / 64, 1, 1);
		nativeArray.Dispose();
	}

	private void DispatchTransformUpdateCommand(bool initialize, int transformQueueCount, NativeArray<InstanceHandle> transformInstanceQueue, NativeArray<TransformUpdatePacket> updateDataQueue, NativeArray<float4> boundingSphereUpdateDataQueue, RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		EnsureTransformBuffersCapacity(transformQueueCount);
		int count;
		int kernelIndex;
		if (initialize)
		{
			count = transformQueueCount * 2;
			kernelIndex = m_TransformInitKernel;
		}
		else
		{
			count = transformQueueCount;
			kernelIndex = m_TransformUpdateKernel;
		}
		NativeArray<GPUInstanceIndex> nativeArray = new NativeArray<GPUInstanceIndex>(transformQueueCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		outputBuffer.CPUInstanceArrayToGPUInstanceArray(transformInstanceQueue.GetSubArray(0, transformQueueCount), nativeArray);
		m_UpdateIndexQueueBuffer.SetData(nativeArray, 0, 0, transformQueueCount);
		m_TransformUpdateDataQueueBuffer.SetData(updateDataQueue, 0, 0, count);
		if (m_EnableBoundingSpheres)
		{
			m_BoundingSpheresUpdateDataQueueBuffer.SetData(boundingSphereUpdateDataQueue, 0, 0, transformQueueCount);
		}
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateQueueCount, transformQueueCount);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputL2WVec4Offset, renderersParameters.localToWorld.uintOffset);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputW2LVec4Offset, renderersParameters.worldToLocal.uintOffset);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputPrevL2WVec4Offset, renderersParameters.matrixPreviousM.uintOffset);
		m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._TransformUpdateOutputPrevW2LVec4Offset, renderersParameters.matrixPreviousMI.uintOffset);
		m_TransformUpdateCS.SetBuffer(kernelIndex, InstanceTransformUpdateIDs._TransformUpdateIndexQueue, m_UpdateIndexQueueBuffer);
		m_TransformUpdateCS.SetBuffer(kernelIndex, InstanceTransformUpdateIDs._TransformUpdateDataQueue, m_TransformUpdateDataQueueBuffer);
		if (m_EnableBoundingSpheres)
		{
			m_TransformUpdateCS.SetInt(InstanceTransformUpdateIDs._BoundingSphereOutputVec4Offset, renderersParameters.boundingSphere.uintOffset);
			m_TransformUpdateCS.SetBuffer(kernelIndex, InstanceTransformUpdateIDs._BoundingSphereDataQueue, m_BoundingSpheresUpdateDataQueueBuffer);
		}
		m_TransformUpdateCS.SetBuffer(kernelIndex, InstanceTransformUpdateIDs._OutputTransformBuffer, outputBuffer.gpuBuffer);
		m_TransformUpdateCS.Dispatch(kernelIndex, (transformQueueCount + 63) / 64, 1, 1);
		nativeArray.Dispose();
	}

	private void DispatchWindDataCopyHistoryCommand(NativeArray<GPUInstanceIndex> gpuInstanceIndices, RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		int windDataCopyHistoryKernel = m_WindDataCopyHistoryKernel;
		int length = gpuInstanceIndices.Length;
		EnsureIndexQueueBufferCapacity(length);
		m_UpdateIndexQueueBuffer.SetData(gpuInstanceIndices, 0, 0, length);
		m_WindDataUpdateCS.SetInt(InstanceWindDataUpdateIDs._WindDataQueueCount, length);
		for (int i = 0; i < 16; i++)
		{
			m_ScratchWindParamAddressArray[i * 4] = renderersParameters.windParams[i].gpuAddress;
		}
		m_WindDataUpdateCS.SetInts(InstanceWindDataUpdateIDs._WindParamAddressArray, m_ScratchWindParamAddressArray);
		for (int j = 0; j < 16; j++)
		{
			m_ScratchWindParamAddressArray[j * 4] = renderersParameters.windHistoryParams[j].gpuAddress;
		}
		m_WindDataUpdateCS.SetInts(InstanceWindDataUpdateIDs._WindHistoryParamAddressArray, m_ScratchWindParamAddressArray);
		m_WindDataUpdateCS.SetBuffer(windDataCopyHistoryKernel, InstanceWindDataUpdateIDs._WindDataUpdateIndexQueue, m_UpdateIndexQueueBuffer);
		m_WindDataUpdateCS.SetBuffer(windDataCopyHistoryKernel, InstanceWindDataUpdateIDs._WindDataBuffer, outputBuffer.gpuBuffer);
		m_WindDataUpdateCS.Dispatch(windDataCopyHistoryKernel, (length + 63) / 64, 1, 1);
	}

	private unsafe void UpdateInstanceMotionsData(in RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		NativeArray<InstanceHandle> nativeArray = new NativeArray<InstanceHandle>(m_InstanceData.instancesLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		int num = 0;
		MotionUpdateJob jobData = default(MotionUpdateJob);
		jobData.queueWriteBase = 0;
		jobData.instanceData = m_InstanceData;
		jobData.atomicUpdateQueueCount = new UnsafeAtomicCounter32(&num);
		jobData.transformUpdateInstanceQueue = nativeArray;
		IJobParallelForExtensions.Schedule(jobData, (m_InstanceData.instancesLength + 63) / 64, 16).Complete();
		if (num > 0)
		{
			DispatchMotionUpdateCommand(num, nativeArray, renderersParameters, outputBuffer);
		}
		nativeArray.Dispose();
	}

	private unsafe void UpdateInstanceTransformsData(bool initialize, NativeArray<InstanceHandle> instances, NativeArray<Matrix4x4> localToWorldMatrices, NativeArray<Matrix4x4> prevLocalToWorldMatrices, in RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		NativeArray<InstanceHandle> nativeArray = new NativeArray<InstanceHandle>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<TransformUpdatePacket> nativeArray2 = new NativeArray<TransformUpdatePacket>(initialize ? (instances.Length * 2) : instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<float4> nativeArray3 = new NativeArray<float4>(m_EnableBoundingSpheres ? instances.Length : 0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<InstanceHandle> nativeArray4 = new NativeArray<InstanceHandle>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> compactTetrahedronCache = new NativeArray<int>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector3> probeQueryPosition = new NativeArray<Vector3>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<SphericalHarmonicsL2> probeUpdateDataQueue = new NativeArray<SphericalHarmonicsL2>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector4> probeOcclusionUpdateDataQueue = new NativeArray<Vector4>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		int num = 0;
		int num2 = 0;
		TransformUpdateJob transformUpdateJob = default(TransformUpdateJob);
		transformUpdateJob.initialize = initialize;
		transformUpdateJob.enableBoundingSpheres = m_EnableBoundingSpheres;
		transformUpdateJob.instances = instances;
		transformUpdateJob.localToWorldMatrices = localToWorldMatrices;
		transformUpdateJob.prevLocalToWorldMatrices = prevLocalToWorldMatrices;
		transformUpdateJob.atomicTransformQueueCount = new UnsafeAtomicCounter32(&num);
		transformUpdateJob.sharedInstanceData = m_SharedInstanceData;
		transformUpdateJob.instanceData = m_InstanceData;
		transformUpdateJob.transformUpdateInstanceQueue = nativeArray;
		transformUpdateJob.transformUpdateDataQueue = nativeArray2;
		transformUpdateJob.boundingSpheresDataQueue = nativeArray3;
		TransformUpdateJob jobData = transformUpdateJob;
		ProbesUpdateJob probesUpdateJob = default(ProbesUpdateJob);
		probesUpdateJob.instances = instances;
		probesUpdateJob.instanceData = m_InstanceData;
		probesUpdateJob.sharedInstanceData = m_SharedInstanceData;
		probesUpdateJob.atomicProbesQueueCount = new UnsafeAtomicCounter32(&num2);
		probesUpdateJob.probeInstanceQueue = nativeArray4;
		probesUpdateJob.compactTetrahedronCache = compactTetrahedronCache;
		probesUpdateJob.probeQueryPosition = probeQueryPosition;
		probesUpdateJob.ScheduleBatch(dependsOn: jobData.ScheduleBatch(instances.Length, 64), arrayLength: instances.Length, indicesPerJobCount: 64).Complete();
		if (num2 > 0)
		{
			ScheduleInterpolateProbesAndUpdateTetrahedronCache(num2, nativeArray4, compactTetrahedronCache, probeQueryPosition, probeUpdateDataQueue, probeOcclusionUpdateDataQueue).Complete();
			DispatchProbeUpdateCommand(num2, nativeArray4, probeUpdateDataQueue, probeOcclusionUpdateDataQueue, renderersParameters, outputBuffer);
		}
		if (num > 0)
		{
			DispatchTransformUpdateCommand(initialize, num, nativeArray, nativeArray2, nativeArray3, renderersParameters, outputBuffer);
		}
		nativeArray.Dispose();
		nativeArray2.Dispose();
		nativeArray3.Dispose();
		nativeArray4.Dispose();
		compactTetrahedronCache.Dispose();
		probeQueryPosition.Dispose();
		probeUpdateDataQueue.Dispose();
		probeOcclusionUpdateDataQueue.Dispose();
	}

	private unsafe void UpdateInstanceProbesData(NativeArray<InstanceHandle> instances, in RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		NativeArray<InstanceHandle> nativeArray = new NativeArray<InstanceHandle>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> compactTetrahedronCache = new NativeArray<int>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector3> probeQueryPosition = new NativeArray<Vector3>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<SphericalHarmonicsL2> probeUpdateDataQueue = new NativeArray<SphericalHarmonicsL2>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector4> probeOcclusionUpdateDataQueue = new NativeArray<Vector4>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		int num = 0;
		ProbesUpdateJob jobData = default(ProbesUpdateJob);
		jobData.instances = instances;
		jobData.instanceData = m_InstanceData;
		jobData.sharedInstanceData = m_SharedInstanceData;
		jobData.atomicProbesQueueCount = new UnsafeAtomicCounter32(&num);
		jobData.probeInstanceQueue = nativeArray;
		jobData.compactTetrahedronCache = compactTetrahedronCache;
		jobData.probeQueryPosition = probeQueryPosition;
		jobData.ScheduleBatch(instances.Length, 64).Complete();
		if (num > 0)
		{
			ScheduleInterpolateProbesAndUpdateTetrahedronCache(num, nativeArray, compactTetrahedronCache, probeQueryPosition, probeUpdateDataQueue, probeOcclusionUpdateDataQueue).Complete();
			DispatchProbeUpdateCommand(num, nativeArray, probeUpdateDataQueue, probeOcclusionUpdateDataQueue, renderersParameters, outputBuffer);
		}
		nativeArray.Dispose();
		compactTetrahedronCache.Dispose();
		probeQueryPosition.Dispose();
		probeUpdateDataQueue.Dispose();
		probeOcclusionUpdateDataQueue.Dispose();
	}

	public void UpdateInstanceWindDataHistory(NativeArray<GPUInstanceIndex> gpuInstanceIndices, RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		if (gpuInstanceIndices.Length != 0)
		{
			DispatchWindDataCopyHistoryCommand(gpuInstanceIndices, renderersParameters, outputBuffer);
		}
	}

	public unsafe void ReallocateAndGetInstances(in GPUDrivenRendererGroupData rendererData, NativeArray<InstanceHandle> instances)
	{
		int instancesCount = 0;
		int num = 0;
		bool num2 = rendererData.instancesCount.Length == 0;
		if (num2)
		{
			new QueryRendererGroupInstancesJob
			{
				rendererGroupInstanceMultiHash = m_RendererGroupInstanceMultiHash,
				rendererGroupIDs = rendererData.rendererGroupID,
				instances = instances,
				atomicNonFoundInstancesCount = new UnsafeAtomicCounter32(&num)
			}.ScheduleBatch(rendererData.rendererGroupID.Length, 128).Complete();
			instancesCount = num;
		}
		else
		{
			new QueryRendererGroupInstancesMultiJob
			{
				rendererGroupInstanceMultiHash = m_RendererGroupInstanceMultiHash,
				rendererGroupIDs = rendererData.rendererGroupID,
				instancesOffsets = rendererData.instancesOffset,
				instancesCounts = rendererData.instancesCount,
				instances = instances,
				atomicNonFoundSharedInstancesCount = new UnsafeAtomicCounter32(&instancesCount),
				atomicNonFoundInstancesCount = new UnsafeAtomicCounter32(&num)
			}.ScheduleBatch(rendererData.rendererGroupID.Length, 128).Complete();
		}
		m_InstanceData.EnsureFreeInstances(num);
		m_PerCameraInstanceData.Grow(m_InstanceData.instancesCapacity);
		m_SharedInstanceData.EnsureFreeInstances(instancesCount);
		InstanceDataSystemBurst.ReallocateInstances(num2, in rendererData.rendererGroupID, in rendererData.packedRendererData, in rendererData.instancesOffset, in rendererData.instancesCount, ref m_InstanceAllocators, ref m_InstanceData, ref m_PerCameraInstanceData, ref m_SharedInstanceData, ref instances, ref m_RendererGroupInstanceMultiHash);
	}

	public void FreeRendererGroupInstances(NativeArray<int> rendererGroupsID)
	{
		NativeArray<int>.ReadOnly rendererGroupsID2 = rendererGroupsID.AsReadOnly();
		InstanceDataSystemBurst.FreeRendererGroupInstances(in rendererGroupsID2, ref m_InstanceAllocators, ref m_InstanceData, ref m_PerCameraInstanceData, ref m_SharedInstanceData, ref m_RendererGroupInstanceMultiHash);
	}

	public void FreeInstances(NativeArray<InstanceHandle> instances)
	{
		NativeArray<InstanceHandle>.ReadOnly instances2 = instances.AsReadOnly();
		InstanceDataSystemBurst.FreeInstances(in instances2, ref m_InstanceAllocators, ref m_InstanceData, ref m_PerCameraInstanceData, ref m_SharedInstanceData, ref m_RendererGroupInstanceMultiHash);
	}

	public JobHandle ScheduleUpdateInstanceDataJob(NativeArray<InstanceHandle> instances, in GPUDrivenRendererGroupData rendererData, NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataMap)
	{
		bool implicitInstanceIndices = rendererData.instancesCount.Length == 0;
		UpdateRendererInstancesJob jobData = default(UpdateRendererInstancesJob);
		jobData.implicitInstanceIndices = implicitInstanceIndices;
		jobData.instances = instances;
		jobData.rendererData = rendererData;
		jobData.lodGroupDataMap = lodGroupDataMap;
		jobData.instanceData = m_InstanceData;
		jobData.sharedInstanceData = m_SharedInstanceData;
		jobData.perCameraInstanceData = m_PerCameraInstanceData;
		return IJobParallelForExtensions.Schedule(jobData, rendererData.rendererGroupID.Length, 128);
	}

	public void UpdateAllInstanceProbes(in RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		NativeArray<InstanceHandle> subArray = m_InstanceData.instances.GetSubArray(0, m_InstanceData.instancesLength);
		if (subArray.Length != 0)
		{
			UpdateInstanceProbesData(subArray, in renderersParameters, outputBuffer);
		}
	}

	public void InitializeInstanceTransforms(NativeArray<InstanceHandle> instances, NativeArray<Matrix4x4> localToWorldMatrices, NativeArray<Matrix4x4> prevLocalToWorldMatrices, in RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		if (instances.Length != 0)
		{
			UpdateInstanceTransformsData(initialize: true, instances, localToWorldMatrices, prevLocalToWorldMatrices, in renderersParameters, outputBuffer);
		}
	}

	public void UpdateInstanceTransforms(NativeArray<InstanceHandle> instances, NativeArray<Matrix4x4> localToWorldMatrices, in RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		if (instances.Length != 0)
		{
			UpdateInstanceTransformsData(initialize: false, instances, localToWorldMatrices, localToWorldMatrices, in renderersParameters, outputBuffer);
		}
	}

	public void UpdateInstanceMotions(in RenderersParameters renderersParameters, GPUInstanceDataBuffer outputBuffer)
	{
		if (m_InstanceData.instancesLength != 0)
		{
			UpdateInstanceMotionsData(in renderersParameters, outputBuffer);
		}
	}

	public JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeArray<InstanceHandle> instances)
	{
		if (rendererGroupIDs.Length == 0)
		{
			return default(JobHandle);
		}
		QueryRendererGroupInstancesJob jobData = default(QueryRendererGroupInstancesJob);
		jobData.rendererGroupInstanceMultiHash = m_RendererGroupInstanceMultiHash;
		jobData.rendererGroupIDs = rendererGroupIDs;
		jobData.instances = instances;
		return jobData.ScheduleBatch(rendererGroupIDs.Length, 128);
	}

	public JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeList<InstanceHandle> instances)
	{
		if (rendererGroupIDs.Length == 0)
		{
			return default(JobHandle);
		}
		NativeArray<int> instancesOffset = new NativeArray<int>(rendererGroupIDs.Length, Allocator.TempJob);
		NativeArray<int> instancesCount = new NativeArray<int>(rendererGroupIDs.Length, Allocator.TempJob);
		JobHandle jobHandle = ScheduleQueryRendererGroupInstancesJob(rendererGroupIDs, instancesOffset, instancesCount, instances);
		instancesOffset.Dispose(jobHandle);
		instancesCount.Dispose(jobHandle);
		return jobHandle;
	}

	public JobHandle ScheduleQueryRendererGroupInstancesJob(NativeArray<int> rendererGroupIDs, NativeArray<int> instancesOffset, NativeArray<int> instancesCount, NativeList<InstanceHandle> instances)
	{
		if (rendererGroupIDs.Length == 0)
		{
			return default(JobHandle);
		}
		QueryRendererGroupInstancesCountJob jobData = default(QueryRendererGroupInstancesCountJob);
		jobData.instanceData = m_InstanceData;
		jobData.sharedInstanceData = m_SharedInstanceData;
		jobData.rendererGroupInstanceMultiHash = m_RendererGroupInstanceMultiHash;
		jobData.rendererGroupIDs = rendererGroupIDs;
		jobData.instancesCount = instancesCount;
		JobHandle dependsOn = jobData.ScheduleBatch(rendererGroupIDs.Length, 128);
		ComputeInstancesOffsetAndResizeInstancesArrayJob jobData2 = default(ComputeInstancesOffsetAndResizeInstancesArrayJob);
		jobData2.instancesCount = instancesCount;
		jobData2.instancesOffset = instancesOffset;
		jobData2.instances = instances;
		JobHandle dependsOn2 = jobData2.Schedule(dependsOn);
		QueryRendererGroupInstancesMultiJob jobData3 = default(QueryRendererGroupInstancesMultiJob);
		jobData3.rendererGroupInstanceMultiHash = m_RendererGroupInstanceMultiHash;
		jobData3.rendererGroupIDs = rendererGroupIDs;
		jobData3.instancesOffsets = instancesOffset;
		jobData3.instancesCounts = instancesCount;
		jobData3.instances = instances.AsDeferredJobArray();
		return jobData3.ScheduleBatch(rendererGroupIDs.Length, 128, dependsOn2);
	}

	public JobHandle ScheduleQuerySortedMeshInstancesJob(NativeArray<int> sortedMeshIDs, NativeList<InstanceHandle> instances)
	{
		if (sortedMeshIDs.Length == 0)
		{
			return default(JobHandle);
		}
		instances.Capacity = m_InstanceData.instancesLength;
		QuerySortedMeshInstancesJob jobData = default(QuerySortedMeshInstancesJob);
		jobData.instanceData = m_InstanceData;
		jobData.sharedInstanceData = m_SharedInstanceData;
		jobData.sortedMeshID = sortedMeshIDs;
		jobData.instances = instances;
		return jobData.ScheduleBatch(m_InstanceData.instancesLength, 64);
	}

	public JobHandle ScheduleCollectInstancesLODGroupAndMasksJob(NativeArray<InstanceHandle> instances, NativeArray<uint> lodGroupAndMasks)
	{
		CollectInstancesLODGroupsAndMasksJob jobData = default(CollectInstancesLODGroupsAndMasksJob);
		jobData.instanceData = instanceData;
		jobData.sharedInstanceData = sharedInstanceData;
		jobData.instances = instances;
		jobData.lodGroupAndMasks = lodGroupAndMasks;
		return IJobParallelForExtensions.Schedule(jobData, instances.Length, 128);
	}

	public bool InternalSanityCheckStates()
	{
		NativeParallelHashMap<SharedInstanceHandle, int> nativeParallelHashMap = new NativeParallelHashMap<SharedInstanceHandle, int>(64, Allocator.Temp);
		int num = 0;
		for (int i = 0; i < m_InstanceData.handlesLength; i++)
		{
			InstanceHandle instance = InstanceHandle.FromInt(i);
			if (m_InstanceData.IsValidInstance(instance))
			{
				SharedInstanceHandle key = m_InstanceData.Get_SharedInstance(instance);
				if (nativeParallelHashMap.TryGetValue(key, out var item))
				{
					nativeParallelHashMap[key] = item + 1;
				}
				else
				{
					nativeParallelHashMap.Add(key, 1);
				}
				num++;
			}
		}
		if (m_InstanceData.instancesLength != num)
		{
			return false;
		}
		int num2 = 0;
		for (int j = 0; j < m_SharedInstanceData.handlesLength; j++)
		{
			SharedInstanceHandle sharedInstanceHandle = default(SharedInstanceHandle);
			sharedInstanceHandle.index = j;
			SharedInstanceHandle sharedInstanceHandle2 = sharedInstanceHandle;
			if (m_SharedInstanceData.IsValidInstance(sharedInstanceHandle2))
			{
				int num3 = m_SharedInstanceData.Get_RefCount(sharedInstanceHandle2);
				if (nativeParallelHashMap[sharedInstanceHandle2] != num3)
				{
					return false;
				}
				num2++;
			}
		}
		if (m_SharedInstanceData.instancesLength != num2)
		{
			return false;
		}
		return true;
	}

	public unsafe void GetVisibleTreeInstances(in ParallelBitArray compactedVisibilityMasks, in ParallelBitArray processedBits, NativeList<int> visibeTreeRendererIDs, NativeList<InstanceHandle> visibeTreeInstances, bool becomeVisibleOnly, out int becomeVisibeTreeInstancesCount)
	{
		becomeVisibeTreeInstancesCount = 0;
		int aliveInstancesOfType = GetAliveInstancesOfType(InstanceType.SpeedTree);
		if (aliveInstancesOfType != 0)
		{
			visibeTreeRendererIDs.ResizeUninitialized(aliveInstancesOfType);
			visibeTreeInstances.ResizeUninitialized(aliveInstancesOfType);
			int num = 0;
			GetVisibleNonProcessedTreeInstancesJob jobData = default(GetVisibleNonProcessedTreeInstancesJob);
			jobData.becomeVisible = true;
			jobData.instanceData = m_InstanceData;
			jobData.sharedInstanceData = m_SharedInstanceData;
			jobData.compactedVisibilityMasks = compactedVisibilityMasks;
			jobData.processedBits = processedBits;
			jobData.rendererIDs = visibeTreeRendererIDs.AsArray();
			jobData.instances = visibeTreeInstances.AsArray();
			jobData.atomicTreeInstancesCount = new UnsafeAtomicCounter32(&num);
			jobData.ScheduleBatch(m_InstanceData.instancesLength, 64).Complete();
			becomeVisibeTreeInstancesCount = num;
			if (!becomeVisibleOnly)
			{
				jobData = default(GetVisibleNonProcessedTreeInstancesJob);
				jobData.becomeVisible = false;
				jobData.instanceData = m_InstanceData;
				jobData.sharedInstanceData = m_SharedInstanceData;
				jobData.compactedVisibilityMasks = compactedVisibilityMasks;
				jobData.processedBits = processedBits;
				jobData.rendererIDs = visibeTreeRendererIDs.AsArray();
				jobData.instances = visibeTreeInstances.AsArray();
				jobData.atomicTreeInstancesCount = new UnsafeAtomicCounter32(&num);
				jobData.ScheduleBatch(m_InstanceData.instancesLength, 64).Complete();
			}
			visibeTreeRendererIDs.ResizeUninitialized(num);
			visibeTreeInstances.ResizeUninitialized(num);
		}
	}

	public void UpdatePerFrameInstanceVisibility(in ParallelBitArray compactedVisibilityMasks)
	{
		UpdateCompactedInstanceVisibilityJob jobData = default(UpdateCompactedInstanceVisibilityJob);
		jobData.instanceData = m_InstanceData;
		jobData.compactedVisibilityMasks = compactedVisibilityMasks;
		jobData.ScheduleBatch(m_InstanceData.instancesLength, 64).Complete();
	}

	public void DeallocatePerCameraInstanceData(NativeArray<int> cameraIDs)
	{
		m_PerCameraInstanceData.DeallocateCameras(cameraIDs);
	}

	public void AllocatePerCameraInstanceData(NativeArray<int> cameraIDs)
	{
		m_PerCameraInstanceData.AllocateCameras(cameraIDs);
	}

	private unsafe static int AtomicAddLengthNoResize<T>(in NativeList<T> list, int count) where T : unmanaged
	{
		return Interlocked.Add(ref list.GetUnsafeList()->m_length, count) - count;
	}
}
