using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

[BurstCompile]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public struct FloatTweenJob : ITweenJob<float>, IJob
{
	public TweenJobData<float> jobData { get; set; }

	public void Execute()
	{
		float t = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
		float to = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, t);
		NativeArray<float> outputData = jobData.outputData;
		outputData[0] = Lerp(jobData.tweenStartValue, to, jobData.tweenAmount);
	}

	public float Lerp(float from, float to, float t)
	{
		if (IsNearlyEqual(from, to))
		{
			return to;
		}
		return math.lerp(from, to, t);
	}

	public bool IsNearlyEqual(float from, float to)
	{
		return math.distancesq(from, to) < 2.5000003E-07f;
	}
}
