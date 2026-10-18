using Unity.Burst;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

[BurstCompile]
public static class BurstGazeUtility
{
	public static bool IsOutsideGaze(in float3 gazePosition, in float3 gazeDirection, in float3 targetPosition, float angleThreshold)
	{
		float3 targetDirection = math.normalize(targetPosition - gazePosition);
		return !IsAlignedToGazeForward(in gazeDirection, in targetDirection, angleThreshold);
	}

	public static bool IsAlignedToGazeForward(in float3 gazeDirection, in float3 targetDirection, float angleThreshold)
	{
		float num = math.cos(math.radians(angleThreshold));
		return math.dot(targetDirection, gazeDirection) > num;
	}

	public static bool IsOutsideDistanceRange(in float3 gazePosition, in float3 targetPosition, float distanceThreshold)
	{
		return math.length(targetPosition - gazePosition) > distanceThreshold;
	}
}
