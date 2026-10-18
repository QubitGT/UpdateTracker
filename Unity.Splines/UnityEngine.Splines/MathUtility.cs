using Unity.Mathematics;

namespace UnityEngine.Splines;

internal static class MathUtility
{
	public static float3 MultiplyVector(float4x4 matrix, float3 vector)
	{
		float3 result = default(float3);
		result.x = matrix.c0.x * vector.x + matrix.c1.x * vector.y + matrix.c2.x * vector.z;
		result.y = matrix.c0.y * vector.x + matrix.c1.y * vector.y + matrix.c2.y * vector.z;
		result.z = matrix.c0.z * vector.x + matrix.c1.z * vector.y + matrix.c2.z * vector.z;
		return result;
	}

	public static bool All(float4x4 matrixA, float4x4 matrixB)
	{
		bool4x4 bool4x = matrixA == matrixB;
		if (math.all(bool4x.c0) && math.all(bool4x.c1) && math.all(bool4x.c2))
		{
			return math.all(bool4x.c3);
		}
		return false;
	}
}
