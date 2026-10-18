using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal struct ReceiverSphereCuller
{
	internal struct SplitInfo
	{
		public float4 receiverSphereLightSpace;

		public float cascadeBlendCullingFactor;
	}

	public NativeList<SplitInfo> splitInfos;

	public float3x3 worldToLightSpaceRotation;

	internal static ReceiverSphereCuller CreateEmptyForTesting(Allocator allocator)
	{
		ReceiverSphereCuller result = default(ReceiverSphereCuller);
		result.splitInfos = new NativeList<SplitInfo>(0, allocator);
		result.worldToLightSpaceRotation = float3x3.identity;
		return result;
	}

	internal void Dispose(JobHandle job)
	{
		splitInfos.Dispose(job);
	}

	internal bool UseReceiverPlanes()
	{
		return splitInfos.Length == 0;
	}

	internal static ReceiverSphereCuller Create(in BatchCullingContext cc, Allocator allocator)
	{
		int num = cc.cullingSplits.Length;
		bool flag = num > 1;
		for (int i = 0; i < num; i++)
		{
			if (!(cc.cullingSplits[i].sphereRadius > 0f))
			{
				flag = false;
			}
		}
		if (!flag)
		{
			num = 0;
		}
		float3x3 v = (float3x3)cc.localToWorldMatrix;
		ReceiverSphereCuller receiverSphereCuller = default(ReceiverSphereCuller);
		receiverSphereCuller.splitInfos = new NativeList<SplitInfo>(num, allocator);
		receiverSphereCuller.worldToLightSpaceRotation = math.transpose(v);
		ReceiverSphereCuller result = receiverSphereCuller;
		result.splitInfos.ResizeUninitialized(num);
		for (int j = 0; j < num; j++)
		{
			CullingSplit cullingSplit = cc.cullingSplits[j];
			float4 receiverSphereLightSpace = new float4(math.mul(result.worldToLightSpaceRotation, cullingSplit.sphereCenter), cullingSplit.sphereRadius);
			result.splitInfos[j] = new SplitInfo
			{
				receiverSphereLightSpace = receiverSphereLightSpace,
				cascadeBlendCullingFactor = cullingSplit.cascadeBlendCullingFactor
			};
		}
		return result;
	}

	internal static float DistanceUntilCylinderFullyCrossesPlane(float3 cylinderCenter, float3 cylinderDirection, float cylinderRadius, Plane plane)
	{
		float y = 0.001f;
		float num = math.max(math.abs(math.dot(plane.normal, cylinderDirection)), y);
		float num2 = (math.dot(plane.normal, cylinderCenter) + plane.distance) / num;
		float num3 = math.sqrt(math.max(1f - num * num, 0f));
		float num4 = cylinderRadius * num3 / num;
		return num2 + num4;
	}

	internal static uint ComputeSplitVisibilityMask(NativeArray<Plane> lightFacingFrustumPlanes, NativeArray<SplitInfo> splitInfos, float3x3 worldToLightSpaceRotation, in AABB bounds)
	{
		float3 center = bounds.center;
		float3 @float = math.mul(worldToLightSpaceRotation, bounds.center);
		float num = math.length(bounds.extents);
		float3 c = math.transpose(worldToLightSpaceRotation).c2;
		float x = float.PositiveInfinity;
		for (int i = 0; i < lightFacingFrustumPlanes.Length; i++)
		{
			x = math.min(x, DistanceUntilCylinderFullyCrossesPlane(center, c, num, lightFacingFrustumPlanes[i]));
		}
		x = math.max(x, 0f);
		uint num2 = 0u;
		int length = splitInfos.Length;
		for (int j = 0; j < length; j++)
		{
			SplitInfo splitInfo = splitInfos[j];
			float3 xyz = splitInfo.receiverSphereLightSpace.xyz;
			float w = splitInfo.receiverSphereLightSpace.w;
			float3 float2 = @float - xyz;
			float num3 = math.lengthsq(num + w) - math.lengthsq(float2.xy);
			if (!(num3 < 0f) && (!(float2.z > 0f) || !(math.lengthsq(float2.z) > num3)))
			{
				num2 |= (uint)(1 << j);
				float num4 = w * splitInfo.cascadeBlendCullingFactor;
				float3 x2 = float2 + new float3(0f, 0f, x);
				float num5 = num4 - num;
				float num6 = math.max(math.lengthsq(float2), math.lengthsq(x2));
				if (num5 > 0f && num6 < math.lengthsq(num5))
				{
					break;
				}
			}
		}
		return num2;
	}
}
