using System;

namespace UnityEngine.Rendering;

internal static class LODRenderingUtils
{
	public static float CalculateFOVHalfAngle(float fieldOfView)
	{
		return Mathf.Tan(MathF.PI / 180f * fieldOfView * 0.5f);
	}

	public static float CalculateScreenRelativeMetricNoBias(LODParameters lodParams)
	{
		if (lodParams.isOrthographic)
		{
			return 2f * lodParams.orthoSize;
		}
		float num = CalculateFOVHalfAngle(lodParams.fieldOfView);
		return 2f * num;
	}

	public static float CalculateMeshLodConstant(LODParameters lodParams, float screenRelativeMetric, float meshLodThreshold)
	{
		return meshLodThreshold * screenRelativeMetric / (float)lodParams.cameraPixelHeight;
	}

	public static float CalculatePerspectiveDistance(Vector3 objPosition, Vector3 camPosition, float sqrScreenRelativeMetric)
	{
		return Mathf.Sqrt(CalculateSqrPerspectiveDistance(objPosition, camPosition, sqrScreenRelativeMetric));
	}

	public static float CalculateSqrPerspectiveDistance(Vector3 objPosition, Vector3 camPosition, float sqrScreenRelativeMetric)
	{
		return (objPosition - camPosition).sqrMagnitude * sqrScreenRelativeMetric;
	}

	public static Vector3 GetWorldReferencePoint(this LODGroup lodGroup)
	{
		return lodGroup.transform.TransformPoint(lodGroup.localReferencePoint);
	}

	public static float GetWorldSpaceScale(this LODGroup lodGroup)
	{
		Vector3 lossyScale = lodGroup.transform.lossyScale;
		return Mathf.Max(Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y)), Mathf.Abs(lossyScale.z));
	}

	public static float GetWorldSpaceSize(this LODGroup lodGroup)
	{
		return lodGroup.GetWorldSpaceScale() * lodGroup.size;
	}

	public static float CalculateLODDistance(float relativeScreenHeight, float size)
	{
		return size / relativeScreenHeight;
	}
}
