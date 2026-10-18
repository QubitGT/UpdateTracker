using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[BurstCompile]
public struct GetPosition : IJobParallelFor
{
	[ReadOnly]
	public NativeSpline Spline;

	[WriteOnly]
	public NativeArray<float3> Positions;

	public void Execute(int index)
	{
		Positions[index] = Spline.EvaluatePosition((float)index / ((float)Positions.Length - 1f));
	}
}
