using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

[BurstCompile]
internal struct ReflectionProbeMinMaxZJob : IJobFor
{
	public Fixed2<float4x4> worldToViews;

	[ReadOnly]
	public NativeArray<VisibleReflectionProbe> reflectionProbes;

	public NativeArray<float2> minMaxZs;

	public void Execute(int index)
	{
		float2 value = math.float2(float.MaxValue, float.MinValue);
		int index2 = index % reflectionProbes.Length;
		VisibleReflectionProbe visibleReflectionProbe = reflectionProbes[index2];
		int index3 = index / reflectionProbes.Length;
		float4x4 a = worldToViews[index3];
		float3 @float = visibleReflectionProbe.bounds.center;
		float3 float2 = visibleReflectionProbe.bounds.extents;
		for (int i = 0; i < 8; i++)
		{
			int num = ((i << 1) & 2) - 1;
			int num2 = (i & 2) - 1;
			int num3 = ((i >> 1) & 2) - 1;
			float4 float3 = math.mul(a, math.float4(@float + float2 * math.float3(num, num2, num3), 1f));
			float3.z *= -1f;
			value.x = math.min(value.x, float3.z);
			value.y = math.max(value.y, float3.z);
		}
		minMaxZs[index] = value;
	}
}
