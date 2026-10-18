using Unity.Mathematics;

namespace UnityEngine.Splines;

public static class SplineMath
{
	public static float RayLineParameter(float3 ro, float3 rd, float3 lineOrigin, float3 lineDir)
	{
		float3 x = ro - lineOrigin;
		float3 y = math.cross(rd, math.cross(rd, lineDir));
		return math.dot(x, y) / math.dot(lineDir, y);
	}

	public static float3 RayLineDistance(float3 ro, float3 rd, float3 a, float3 b)
	{
		(float3, float3) tuple = RayLineNearestPoint(ro, rd, a, b);
		return tuple.Item2 - tuple.Item1;
	}

	public static (float3 rayPoint, float3 linePoint) RayLineNearestPoint(float3 ro, float3 rd, float3 a, float3 b)
	{
		float rayParam;
		float lineParam;
		return RayLineNearestPoint(ro, rd, a, b, out rayParam, out lineParam);
	}

	public static (float3 rayPoint, float3 linePoint) RayLineNearestPoint(float3 ro, float3 rd, float3 a, float3 b, out float rayParam, out float lineParam)
	{
		float3 @float = b - a;
		lineParam = RayLineParameter(ro, rd, a, @float);
		float3 float2 = a + @float * math.saturate(lineParam);
		rayParam = math.dot(rd, float2 - ro);
		return (rayPoint: ro + rd * rayParam, linePoint: float2);
	}

	public static float3 PointLineNearestPoint(float3 p, float3 a, float3 b, out float lineParam)
	{
		float3 @float = b - a;
		float num = math.length(@float);
		float3 float2 = math.select(0f, @float * (1f / num), num > 1.1754944E-38f);
		lineParam = math.dot(float2, p - a);
		return a + float2 * math.clamp(lineParam, 0f, num);
	}

	public static float DistancePointLine(float3 p, float3 a, float3 b)
	{
		float lineParam;
		return math.length(PointLineNearestPoint(p, a, b, out lineParam) - p);
	}

	internal static float GetUnitCircleTangentLength()
	{
		return 4f * (math.sqrt(2f) - 1f) / 3f;
	}
}
