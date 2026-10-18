using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

[BurstCompile]
internal struct LightMinMaxZJob : IJobFor
{
	public Fixed2<float4x4> worldToViews;

	[ReadOnly]
	public NativeArray<VisibleLight> lights;

	public NativeArray<float2> minMaxZs;

	public void Execute(int index)
	{
		int index2 = index % lights.Length;
		VisibleLight visibleLight = lights[index2];
		float4x4 float4x = visibleLight.localToWorldMatrix;
		float3 xyz = float4x.c3.xyz;
		int index3 = index / lights.Length;
		float4x4 a = worldToViews[index3];
		float3 xyz2 = math.mul(a, math.float4(xyz, 1f)).xyz;
		xyz2.z *= -1f;
		float2 value = math.float2(xyz2.z - visibleLight.range, xyz2.z + visibleLight.range);
		if (visibleLight.lightType == LightType.Spot)
		{
			float num = math.radians(visibleLight.spotAngle) * 0.5f;
			float num2 = math.cos(num);
			float num3 = visibleLight.range * num2;
			float3 xyz3 = float4x.c2.xyz;
			float3 xyz4 = xyz + xyz3 * num3;
			float3 xyz5 = math.mul(a, math.float4(xyz4, 1f)).xyz;
			xyz5.z *= -1f;
			float x = MathF.PI / 2f - num;
			float num4 = visibleLight.range * num2 * math.sin(num) / math.sin(x);
			float3 @float = xyz5 - xyz2;
			float num5 = math.sqrt(1f - @float.z * @float.z / math.dot(@float, @float));
			if (0f - @float.z < num3 * num2)
			{
				value.x = math.min(xyz2.z, xyz5.z - num5 * num4);
			}
			if (@float.z < num3 * num2)
			{
				value.y = math.max(xyz2.z, xyz5.z + num5 * num4);
			}
		}
		else if (visibleLight.lightType != LightType.Point)
		{
			value.x = float.MaxValue;
			value.y = float.MinValue;
		}
		value.x = math.max(value.x, 0f);
		value.y = math.max(value.y, 0f);
		minMaxZs[index] = value;
	}
}
