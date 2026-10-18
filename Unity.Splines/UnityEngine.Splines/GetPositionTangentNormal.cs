using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public struct GetPositionTangentNormal : IJobParallelFor
{
	[ReadOnly]
	public NativeSpline Spline;

	[WriteOnly]
	public NativeArray<float3> Positions;

	[WriteOnly]
	public NativeArray<float3> Tangents;

	[WriteOnly]
	public NativeArray<float3> Normals;

	public void Execute(int index)
	{
		Spline.Evaluate((float)index / ((float)Positions.Length - 1f), out var position, out var tangent, out var upVector);
		Positions[index] = position;
		Tangents[index] = tangent;
		Normals[index] = upVector;
	}
}
