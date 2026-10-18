using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

[BurstCompile]
internal struct CullingJob : IJobParallelFor
{
	private enum CrossFadeType
	{
		kDisabled,
		kCrossFadeOut,
		kCrossFadeIn,
		kVisible
	}

	public const int k_BatchSize = 32;

	public const uint k_MeshLodCrossfadeActive = 64u;

	public const uint k_MeshLodCrossfadeSignBit = 128u;

	public const uint k_MeshLodCrossfadeBits = 192u;

	public const uint k_LODFadeOff = 255u;

	public const uint k_LODFadeZeroPacked = 127u;

	public const uint k_LODFadeIsSpeedTree = 256u;

	private const uint k_InvalidCrossFadeAndLevel = uint.MaxValue;

	private const uint k_VisibilityMaskNotVisible = 0u;

	private const float k_SmallMeshTransitionWidth = 0.1f;

	[ReadOnly]
	public BinningConfig binningConfig;

	[ReadOnly]
	public BatchCullingViewType viewType;

	[ReadOnly]
	public float3 cameraPosition;

	[ReadOnly]
	public float sqrMeshLodSelectionConstant;

	[ReadOnly]
	public float sqrScreenRelativeMetric;

	[ReadOnly]
	public float minScreenRelativeHeight;

	[ReadOnly]
	public bool isOrtho;

	[ReadOnly]
	public bool cullLightmappedShadowCasters;

	[ReadOnly]
	public int maxLOD;

	[ReadOnly]
	public uint cullingLayerMask;

	[ReadOnly]
	public ulong sceneCullingMask;

	[ReadOnly]
	public bool animateCrossFades;

	[ReadOnly]
	public NativeArray<FrustumPlaneCuller.PlanePacket4> frustumPlanePackets;

	[ReadOnly]
	public NativeArray<FrustumPlaneCuller.SplitInfo> frustumSplitInfos;

	[ReadOnly]
	public NativeArray<Plane> lightFacingFrustumPlanes;

	[ReadOnly]
	public NativeArray<ReceiverSphereCuller.SplitInfo> receiverSplitInfos;

	public float3x3 worldToLightSpaceRotation;

	[ReadOnly]
	public CPUInstanceData.ReadOnly instanceData;

	[ReadOnly]
	public CPUSharedInstanceData.ReadOnly sharedInstanceData;

	[NativeDisableContainerSafetyRestriction]
	[NoAlias]
	[ReadOnly]
	public NativeList<LODGroupCullingData> lodGroupCullingData;

	[NativeDisableUnsafePtrRestriction]
	[ReadOnly]
	public IntPtr occlusionBuffer;

	[NativeDisableContainerSafetyRestriction]
	public CPUPerCameraInstanceData.PerCameraInstanceDataArrays cameraInstanceData;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<byte> rendererVisibilityMasks;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<byte> rendererMeshLodSettings;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<byte> rendererCrossFadeValues;

	private static uint PackFloatToUint8(float percent)
	{
		uint valueToClamp = (uint)((1f + percent) * 127f + 0.5f);
		if (percent < 0f)
		{
			return math.clamp(valueToClamp, 0u, 126u);
		}
		return math.clamp(valueToClamp, 128u, 254u);
	}

	private unsafe uint CalculateLODVisibility(int instanceIndex, int sharedInstanceIndex, InstanceFlags instanceFlags)
	{
		uint num = sharedInstanceData.lodGroupAndMasks[sharedInstanceIndex];
		if (num == uint.MaxValue)
		{
			if (viewType >= BatchCullingViewType.SelectionOutline || (instanceFlags & InstanceFlags.SmallMeshCulling) == 0 || minScreenRelativeHeight == 0f)
			{
				return 255u;
			}
			ref readonly AABB reference = ref instanceData.worldAABBs.UnsafeElementAt(instanceIndex);
			float num2 = math.sqrt(isOrtho ? sqrScreenRelativeMetric : LODRenderingUtils.CalculateSqrPerspectiveDistance(reference.center, cameraPosition, sqrScreenRelativeMetric));
			float3 @float = reference.extents * 2f;
			float size = math.max(math.max(@float.x, @float.y), @float.z);
			float num3 = LODRenderingUtils.CalculateLODDistance(minScreenRelativeHeight, size);
			if (num3 < num2)
			{
				return 127u;
			}
			float relativeScreenHeight = minScreenRelativeHeight + 0.1f * minScreenRelativeHeight;
			float num4 = Mathf.Max(0f, num3 - LODRenderingUtils.CalculateLODDistance(relativeScreenHeight, size));
			float num5 = (num3 - num2) / num4;
			if (!(num5 > 1f))
			{
				return PackFloatToUint8(num5);
			}
			return 255u;
		}
		uint index = num >> 8;
		uint num6 = num & 0xFFu;
		ref LODGroupCullingData reference2 = ref lodGroupCullingData.ElementAt((int)index);
		if (reference2.forceLODMask != 0)
		{
			if ((reference2.forceLODMask & num6) == 0)
			{
				return 127u;
			}
			return 255u;
		}
		float num7 = (isOrtho ? sqrScreenRelativeMetric : LODRenderingUtils.CalculateSqrPerspectiveDistance(reference2.worldSpaceReferencePoint, cameraPosition, sqrScreenRelativeMetric));
		uint num8 = (uint)(-1 << maxLOD);
		num6 &= num8;
		int num9 = math.max(math.tzcnt(num6) - 1, maxLOD);
		num6 >>= num9;
		while (num6 != 0)
		{
			float num10 = ((num9 == maxLOD) ? 0f : reference2.sqrDistances[num9 - 1]);
			float num11 = reference2.sqrDistances[num9];
			if (num7 < num10)
			{
				break;
			}
			if (num7 > num11)
			{
				num9++;
				num6 >>= 1;
				continue;
			}
			CrossFadeType crossFadeType = (CrossFadeType)((int)num6 & 3);
			switch (crossFadeType)
			{
			case CrossFadeType.kDisabled:
				return 127u;
			case CrossFadeType.kVisible:
				return 255u;
			}
			float num12 = math.sqrt(num7);
			float num13 = math.sqrt(num11);
			if (reference2.percentageFlags[num9])
			{
				switch (crossFadeType)
				{
				case CrossFadeType.kCrossFadeIn:
					return 128u;
				case CrossFadeType.kCrossFadeOut:
				{
					float num14 = ((num9 > 0) ? math.sqrt(reference2.sqrDistances[num9 - 1]) : reference2.worldSpaceSize);
					return PackFloatToUint8(math.max(num12 - num14, 0f) / (num13 - num14)) | 0x100u;
				}
				}
				continue;
			}
			float num15 = reference2.transitionDistances[num9];
			float num16 = num13 - num12;
			if (num16 < num15)
			{
				float num17 = num16 / num15;
				if (crossFadeType == CrossFadeType.kCrossFadeIn)
				{
					num17 = 0f - num17;
				}
				return PackFloatToUint8(num17);
			}
			if (crossFadeType != CrossFadeType.kCrossFadeOut)
			{
				return 127u;
			}
			return 255u;
		}
		return 127u;
	}

	private uint CalculateVisibilityMask(int instanceIndex, int sharedInstanceIndex, InstanceFlags instanceFlags)
	{
		if (cullingLayerMask == 0)
		{
			return 0u;
		}
		if ((cullingLayerMask & (1 << sharedInstanceData.gameObjectLayers[sharedInstanceIndex])) == 0L)
		{
			return 0u;
		}
		if (cullLightmappedShadowCasters && (instanceFlags & InstanceFlags.AffectsLightmaps) != 0)
		{
			return 0u;
		}
		if (viewType == BatchCullingViewType.Camera && (instanceFlags & InstanceFlags.IsShadowsOnly) != 0)
		{
			return 0u;
		}
		if (viewType == BatchCullingViewType.Light && (instanceFlags & InstanceFlags.IsShadowsOff) != 0)
		{
			return 0u;
		}
		ref readonly AABB reference = ref instanceData.worldAABBs.UnsafeElementAt(instanceIndex);
		uint num = FrustumPlaneCuller.ComputeSplitVisibilityMask(frustumPlanePackets, frustumSplitInfos, in reference);
		if (num != 0 && receiverSplitInfos.Length > 0)
		{
			num &= ReceiverSphereCuller.ComputeSplitVisibilityMask(lightFacingFrustumPlanes, receiverSplitInfos, worldToLightSpaceRotation, in reference);
		}
		if (num != 0 && occlusionBuffer != IntPtr.Zero)
		{
			num = (BatchRendererGroup.OcclusionTestAABB(occlusionBuffer, reference.ToBounds()) ? num : 0u);
		}
		return num;
	}

	private uint ComputeMeshLODLevel(int instanceIndex, int sharedInstanceIndex)
	{
		ref readonly GPUDrivenRendererMeshLodData reference = ref instanceData.meshLodData.UnsafeElementAt(instanceIndex);
		GPUDrivenMeshLodInfo gPUDrivenMeshLodInfo = sharedInstanceData.meshLodInfos[sharedInstanceIndex];
		if (reference.forceLod >= 0)
		{
			return (uint)math.clamp(reference.forceLod, 0, gPUDrivenMeshLodInfo.levelCount - 1);
		}
		ref readonly AABB reference2 = ref instanceData.worldAABBs.UnsafeElementAt(instanceIndex);
		float num = math.max(math.lengthsq(reference2.extents), 1E-05f) * 4f;
		return (uint)math.floor(math.clamp(math.max(math.log2(Math.Sqrt((isOrtho ? sqrMeshLodSelectionConstant : LODRenderingUtils.CalculateSqrPerspectiveDistance(reference2.center, cameraPosition, sqrMeshLodSelectionConstant)) / num)) * (double)gPUDrivenMeshLodInfo.lodSlope + (double)gPUDrivenMeshLodInfo.lodBias, 0.0) + (double)reference.lodSelectionBias, 0.0, gPUDrivenMeshLodInfo.levelCount - 1));
	}

	private uint ComputeMeshLODCrossfade(int instanceIndex, ref uint meshLodLevel)
	{
		byte b = cameraInstanceData.meshLods[instanceIndex];
		if (b == byte.MaxValue)
		{
			cameraInstanceData.meshLods[instanceIndex] = (byte)meshLodLevel;
			return 255u;
		}
		byte b2 = cameraInstanceData.crossFades[instanceIndex];
		if (b2 == byte.MaxValue)
		{
			if (b == meshLodLevel)
			{
				return 255u;
			}
			cameraInstanceData.meshLods[instanceIndex] = (byte)meshLodLevel;
			cameraInstanceData.crossFades[instanceIndex] = (byte)((meshLodLevel >= b) ? 1u : 128u);
			meshLodLevel = b;
			return 255u;
		}
		if ((long)(b2 - 1) % 127L == 0L)
		{
			meshLodLevel = b;
			return 255u;
		}
		meshLodLevel = b | (((uint)b2 > 127u) ? 192u : 64u);
		return b2;
	}

	private void EnforcePreviousFrameMeshLOD(int instanceIndex, ref uint meshLodLevel)
	{
		ref byte reference = ref cameraInstanceData.meshLods.ElementAt(instanceIndex);
		if (reference != byte.MaxValue)
		{
			meshLodLevel = reference;
		}
	}

	public void Execute(int instanceIndex)
	{
		InstanceHandle instance = instanceData.instances[instanceIndex];
		int num = sharedInstanceData.InstanceToIndex(in instanceData, instance);
		InstanceFlags instanceFlags = sharedInstanceData.flags[num].instanceFlags;
		uint num2 = CalculateVisibilityMask(instanceIndex, num, instanceFlags);
		if (num2 == 0)
		{
			rendererVisibilityMasks[instance.index] = 0;
			return;
		}
		uint num3 = CalculateLODVisibility(instanceIndex, num, instanceFlags);
		if (num3 == 127)
		{
			rendererVisibilityMasks[instance.index] = 0;
			return;
		}
		if (binningConfig.supportsMotionCheck)
		{
			bool flag = instanceData.movedInPreviousFrameBits.Get(instanceIndex);
			num2 = (num2 << 1) | (flag ? 1u : 0u);
		}
		uint meshLodLevel = 0u;
		bool flag2 = (instanceFlags & InstanceFlags.HasMeshLod) != 0;
		if (flag2)
		{
			meshLodLevel = ComputeMeshLODLevel(instanceIndex, num);
		}
		if (binningConfig.supportsCrossFade)
		{
			if (flag2 && animateCrossFades)
			{
				if (num3 == 255)
				{
					num3 = ComputeMeshLODCrossfade(instanceIndex, ref meshLodLevel);
				}
				else
				{
					EnforcePreviousFrameMeshLOD(instanceIndex, ref meshLodLevel);
				}
			}
			num2 = (num2 << 1) | ((num3 < 255) ? 1u : 0u);
		}
		rendererVisibilityMasks[instance.index] = (byte)num2;
		rendererMeshLodSettings[instance.index] = (byte)meshLodLevel;
		rendererCrossFadeValues[instance.index] = (byte)(num3 & 0xFFu);
	}
}
