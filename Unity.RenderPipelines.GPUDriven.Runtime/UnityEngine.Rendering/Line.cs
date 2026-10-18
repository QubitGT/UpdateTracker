using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal struct Line
{
	public float3 m;

	public float3 t;

	internal static Line LineOfPlaneIntersectingPlane(float4 a, float4 b)
	{
		Line result = default(Line);
		result.m = a.w * b.xyz - b.w * a.xyz;
		result.t = math.cross(a.xyz, b.xyz);
		return result;
	}

	internal static float4 PlaneContainingLineAndPoint(Line a, float3 b)
	{
		return new float4(a.m + math.cross(a.t, b), 0f - math.dot(a.m, b));
	}

	internal static float4 PlaneContainingLineWithNormalPerpendicularToVector(Line a, float3 b)
	{
		return new float4(math.cross(a.t, b), 0f - math.dot(a.m, b));
	}
}
