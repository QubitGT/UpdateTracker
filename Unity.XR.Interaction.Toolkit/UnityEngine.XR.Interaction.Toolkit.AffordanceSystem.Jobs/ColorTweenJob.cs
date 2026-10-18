using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

[BurstCompile]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public struct ColorTweenJob : ITweenJob<Color>, IJob
{
	public TweenJobData<Color> jobData { get; set; }

	public byte colorBlendMode { get; set; }

	public float colorBlendAmount { get; set; }

	public void Execute()
	{
		float t = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
		Color newValue = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, t);
		Color to = ProcessTargetAffordanceValue(jobData.initialValue, newValue);
		NativeArray<Color> outputData = jobData.outputData;
		outputData[0] = Lerp(jobData.tweenStartValue, to, jobData.tweenAmount);
	}

	private Color ProcessTargetAffordanceValue(Color initialValue, Color newValue)
	{
		Color result = newValue;
		switch (colorBlendMode)
		{
		case 1:
		{
			float num = colorBlendAmount;
			result = new Color(initialValue.r + newValue.r * num, initialValue.g + newValue.g * num, initialValue.b + newValue.b * num, initialValue.a + newValue.a * num);
			break;
		}
		case 2:
			result = Lerp(initialValue, newValue, colorBlendAmount);
			break;
		}
		return result;
	}

	public Color Lerp(Color from, Color to, float t)
	{
		if (IsNearlyEqual(from, to))
		{
			return to;
		}
		return (Vector4)math.lerp((Vector4)from, (Vector4)to, t);
	}

	public bool IsNearlyEqual(Color from, Color to)
	{
		return math.distancesq((Vector4)from, (Vector4)to) < 2.5000003E-07f;
	}
}
