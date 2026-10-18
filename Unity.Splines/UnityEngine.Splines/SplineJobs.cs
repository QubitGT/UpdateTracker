using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public static class SplineJobs
{
	public static void EvaluatePosition<T>(T spline, NativeArray<float3> positions) where T : ISpline
	{
		using NativeSpline spline2 = new NativeSpline(spline, Allocator.TempJob);
		EvaluatePosition(spline2, positions);
	}

	public static void EvaluatePosition(NativeSpline spline, NativeArray<float3> positions)
	{
		GetPosition jobData = default(GetPosition);
		jobData.Spline = spline;
		jobData.Positions = positions;
		jobData.Schedule(positions.Length, 1).Complete();
	}

	public static void EvaluatePositionTangentNormal<T>(T spline, NativeArray<float3> positions, NativeArray<float3> tangents, NativeArray<float3> normals) where T : ISpline
	{
		using NativeSpline spline2 = new NativeSpline(spline, Allocator.TempJob);
		EvaluatePositionTangentNormal(spline2, positions, tangents, normals);
	}

	public static void EvaluatePositionTangentNormal(NativeSpline spline, NativeArray<float3> positions, NativeArray<float3> tangents, NativeArray<float3> normals)
	{
		GetPositionTangentNormal jobData = default(GetPositionTangentNormal);
		jobData.Spline = spline;
		jobData.Positions = positions;
		jobData.Tangents = tangents;
		jobData.Normals = normals;
		jobData.Schedule(positions.Length, 1).Complete();
	}
}
