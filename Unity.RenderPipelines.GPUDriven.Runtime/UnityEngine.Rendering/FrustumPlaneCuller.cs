using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal struct FrustumPlaneCuller
{
	internal struct PlanePacket4
	{
		public float4 nx;

		public float4 ny;

		public float4 nz;

		public float4 d;

		public float4 nxAbs;

		public float4 nyAbs;

		public float4 nzAbs;

		public PlanePacket4(NativeArray<Plane> planes, int offset, int limit)
		{
			Plane plane = planes[Mathf.Min(offset, limit)];
			Plane plane2 = planes[Mathf.Min(offset + 1, limit)];
			Plane plane3 = planes[Mathf.Min(offset + 2, limit)];
			Plane plane4 = planes[Mathf.Min(offset + 3, limit)];
			nx = new float4(plane.normal.x, plane2.normal.x, plane3.normal.x, plane4.normal.x);
			ny = new float4(plane.normal.y, plane2.normal.y, plane3.normal.y, plane4.normal.y);
			nz = new float4(plane.normal.z, plane2.normal.z, plane3.normal.z, plane4.normal.z);
			d = new float4(plane.distance, plane2.distance, plane3.distance, plane4.distance);
			nxAbs = math.abs(nx);
			nyAbs = math.abs(ny);
			nzAbs = math.abs(nz);
		}
	}

	internal struct SplitInfo
	{
		public int packetCount;
	}

	public NativeList<PlanePacket4> planePackets;

	public NativeList<SplitInfo> splitInfos;

	internal void Dispose(JobHandle job)
	{
		planePackets.Dispose(job);
		splitInfos.Dispose(job);
	}

	internal static FrustumPlaneCuller Create(in BatchCullingContext cc, NativeArray<Plane> receiverPlanes, in ReceiverSphereCuller receiverSphereCuller, Allocator allocator)
	{
		int length = cc.cullingSplits.Length;
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			int num2 = receiverPlanes.Length + cc.cullingSplits[i].cullingPlaneCount;
			num += (num2 + 3) / 4;
		}
		FrustumPlaneCuller frustumPlaneCuller = default(FrustumPlaneCuller);
		frustumPlaneCuller.planePackets = new NativeList<PlanePacket4>(num, allocator);
		frustumPlaneCuller.splitInfos = new NativeList<SplitInfo>(length, allocator);
		FrustumPlaneCuller result = frustumPlaneCuller;
		result.planePackets.ResizeUninitialized(num);
		result.splitInfos.ResizeUninitialized(length);
		NativeList<Plane> nativeList = new NativeList<Plane>(Allocator.Temp);
		int num3 = 0;
		for (int j = 0; j < length; j++)
		{
			CullingSplit cullingSplit = cc.cullingSplits[j];
			nativeList.Clear();
			for (int k = 0; k < cullingSplit.cullingPlaneCount; k++)
			{
				Plane value = cc.cullingPlanes[cullingSplit.cullingPlaneOffset + k];
				nativeList.Add(in value);
			}
			if (receiverSphereCuller.UseReceiverPlanes())
			{
				nativeList.AddRange(receiverPlanes);
			}
			int num4 = (nativeList.Length + 3) / 4;
			result.splitInfos[j] = new SplitInfo
			{
				packetCount = num4
			};
			for (int l = 0; l < num4; l++)
			{
				result.planePackets[num3 + l] = new PlanePacket4(nativeList.AsArray(), 4 * l, nativeList.Length - 1);
			}
			num3 += num4;
		}
		nativeList.Dispose();
		return result;
	}

	internal static uint ComputeSplitVisibilityMask(NativeArray<PlanePacket4> planePackets, NativeArray<SplitInfo> splitInfos, in AABB bounds)
	{
		float4 xxxx = bounds.center.xxxx;
		float4 yyyy = bounds.center.yyyy;
		float4 zzzz = bounds.center.zzzz;
		float4 xxxx2 = bounds.extents.xxxx;
		float4 yyyy2 = bounds.extents.yyyy;
		float4 zzzz2 = bounds.extents.zzzz;
		uint num = 0u;
		int num2 = 0;
		int length = splitInfos.Length;
		for (int i = 0; i < length; i++)
		{
			SplitInfo splitInfo = splitInfos[i];
			bool4 x = new bool4(v: false);
			for (int j = 0; j < splitInfo.packetCount; j++)
			{
				PlanePacket4 planePacket = planePackets[num2 + j];
				float4 @float = planePacket.nx * xxxx + planePacket.ny * yyyy + planePacket.nz * zzzz + planePacket.d;
				float4 float2 = planePacket.nxAbs * xxxx2 + planePacket.nyAbs * yyyy2 + planePacket.nzAbs * zzzz2;
				x |= @float + float2 < float4.zero;
			}
			if (!math.any(x))
			{
				num |= (uint)(1 << i);
			}
			num2 += splitInfo.packetCount;
		}
		return num;
	}
}
