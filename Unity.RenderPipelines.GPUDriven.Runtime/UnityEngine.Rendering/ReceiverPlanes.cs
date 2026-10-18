using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal struct ReceiverPlanes
{
	public NativeList<Plane> planes;

	public int lightFacingPlaneCount;

	private static bool IsSignBitSet(float x)
	{
		return math.asuint(x) >> 31 != 0;
	}

	internal NativeArray<Plane> LightFacingFrustumPlaneSubArray()
	{
		return planes.AsArray().GetSubArray(0, lightFacingPlaneCount);
	}

	internal NativeArray<Plane> SilhouettePlaneSubArray()
	{
		return planes.AsArray().GetSubArray(lightFacingPlaneCount, planes.Length - lightFacingPlaneCount);
	}

	internal static ReceiverPlanes CreateEmptyForTesting(Allocator allocator)
	{
		ReceiverPlanes result = default(ReceiverPlanes);
		result.planes = new NativeList<Plane>(allocator);
		result.lightFacingPlaneCount = 0;
		return result;
	}

	internal void Dispose(JobHandle job)
	{
		planes.Dispose(job);
	}

	internal static ReceiverPlanes Create(in BatchCullingContext cc, Allocator allocator)
	{
		ReceiverPlanes receiverPlanes = default(ReceiverPlanes);
		receiverPlanes.planes = new NativeList<Plane>(allocator);
		receiverPlanes.lightFacingPlaneCount = 0;
		ReceiverPlanes result = receiverPlanes;
		if (cc.viewType == BatchCullingViewType.Light && cc.receiverPlaneCount != 0)
		{
			bool flag = false;
			if (cc.cullingSplits.Length > 0)
			{
				Matrix4x4 cullingMatrix = cc.cullingSplits[0].cullingMatrix;
				flag = cullingMatrix[15] == 1f && cullingMatrix[11] == 0f && cullingMatrix[7] == 0f && cullingMatrix[3] == 0f;
			}
			if (flag)
			{
				Vector3 vector = -cc.localToWorldMatrix.GetColumn(2);
				int num = 0;
				for (int i = 0; i < cc.receiverPlaneCount; i++)
				{
					Plane value = cc.cullingPlanes[cc.receiverPlaneOffset + i];
					if (IsSignBitSet(Vector3.Dot(value.normal, vector)))
					{
						num |= 1 << i;
					}
					else
					{
						result.planes.Add(in value);
					}
				}
				result.lightFacingPlaneCount = result.planes.Length;
				if (cc.receiverPlaneCount == 6)
				{
					for (int j = 0; j < cc.receiverPlaneCount; j++)
					{
						for (int k = j + 1; k < cc.receiverPlaneCount; k++)
						{
							if (j / 2 != k / 2 && ((uint)((num >> j) ^ (num >> k)) & (true ? 1u : 0u)) != 0)
							{
								int num4;
								int num5;
								if (((uint)(num >> j) & (true ? 1u : 0u)) != 0)
								{
									int num2 = k;
									int num3 = j;
									num4 = num2;
									num5 = num3;
								}
								else
								{
									int num6 = j;
									int num3 = k;
									num4 = num6;
									num5 = num3;
								}
								Plane plane = cc.cullingPlanes[cc.receiverPlaneOffset + num4];
								Plane plane2 = cc.cullingPlanes[cc.receiverPlaneOffset + num5];
								float4 a = new float4(plane.normal, plane.distance);
								float4 b = new float4(plane2.normal, plane2.distance);
								float4 x = Line.PlaneContainingLineWithNormalPerpendicularToVector(Line.LineOfPlaneIntersectingPlane(a, b), vector);
								x /= math.length(x.xyz);
								if (!math.any(math.isnan(x)))
								{
									ref NativeList<Plane> reference = ref result.planes;
									Plane value2 = new Plane(x.xyz, x.w);
									reference.Add(in value2);
								}
							}
						}
					}
				}
			}
			else
			{
				Vector3 position = cc.localToWorldMatrix.GetPosition();
				int num7 = 0;
				for (int l = 0; l < cc.receiverPlaneCount; l++)
				{
					Plane value3 = cc.cullingPlanes[cc.receiverPlaneOffset + l];
					if (IsSignBitSet(value3.GetDistanceToPoint(position)))
					{
						num7 |= 1 << l;
					}
					else
					{
						result.planes.Add(in value3);
					}
				}
				result.lightFacingPlaneCount = result.planes.Length;
				if (cc.receiverPlaneCount == 6)
				{
					for (int m = 0; m < cc.receiverPlaneCount; m++)
					{
						for (int n = m + 1; n < cc.receiverPlaneCount; n++)
						{
							if (m / 2 != n / 2 && ((uint)((num7 >> m) ^ (num7 >> n)) & (true ? 1u : 0u)) != 0)
							{
								int num9;
								int num10;
								if (((uint)(num7 >> m) & (true ? 1u : 0u)) != 0)
								{
									int num8 = n;
									int num3 = m;
									num9 = num8;
									num10 = num3;
								}
								else
								{
									int num11 = m;
									int num3 = n;
									num9 = num11;
									num10 = num3;
								}
								Plane plane3 = cc.cullingPlanes[cc.receiverPlaneOffset + num9];
								Plane plane4 = cc.cullingPlanes[cc.receiverPlaneOffset + num10];
								float4 a2 = new float4(plane3.normal, plane3.distance);
								float4 b2 = new float4(plane4.normal, plane4.distance);
								float4 x2 = Line.PlaneContainingLineAndPoint(Line.LineOfPlaneIntersectingPlane(a2, b2), position);
								x2 /= math.length(x2.xyz);
								if (!math.any(math.isnan(x2)))
								{
									ref NativeList<Plane> reference2 = ref result.planes;
									Plane value2 = new Plane(x2.xyz, x2.w);
									reference2.Add(in value2);
								}
							}
						}
					}
				}
			}
		}
		return result;
	}
}
