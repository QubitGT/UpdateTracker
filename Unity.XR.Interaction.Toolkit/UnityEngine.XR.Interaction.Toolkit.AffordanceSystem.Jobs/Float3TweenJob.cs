using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

[BurstCompile]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public struct Float3TweenJob : ITweenJob<float3>, IJob
{
	public TweenJobData<float3> jobData { get; set; }

	public void Execute()
	{
		float t = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
		float3 to = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, t);
		NativeArray<float3> outputData = jobData.outputData;
		outputData[0] = Lerp(jobData.tweenStartValue, to, jobData.tweenAmount);
	}

	public float3 Lerp(float3 from, float3 to, float t)
	{
		if (IsNearlyEqual(from, to))
		{
			return to;
		}
		return math.lerp(from, to, t);
	}

	public bool IsNearlyEqual(float3 from, float3 to)
	{
		return math.distancesq(from, to) < 2.5000003E-07f;
	}
}
