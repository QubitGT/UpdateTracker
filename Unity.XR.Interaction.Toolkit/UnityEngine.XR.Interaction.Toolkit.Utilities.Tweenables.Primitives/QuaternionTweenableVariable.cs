using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class QuaternionTweenableVariable : TweenableVariableSynchronousBase<Quaternion>
{
	public float angleEqualityThreshold { get; set; } = 0.01f;


	protected override Quaternion Lerp(Quaternion from, Quaternion to, float t)
	{
		return Quaternion.Slerp(from, to, t);
	}

	protected override bool IsNearlyEqual(Quaternion startValue, Quaternion targetValue)
	{
		return Quaternion.Angle(startValue, targetValue) < angleEqualityThreshold;
	}
}
