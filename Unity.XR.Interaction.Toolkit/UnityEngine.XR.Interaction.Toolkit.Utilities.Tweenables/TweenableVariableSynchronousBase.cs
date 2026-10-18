using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class TweenableVariableSynchronousBase<T> : TweenableVariableBase<T> where T : IEquatable<T>
{
	protected override void ExecuteTween(T startValue, T targetValue, float tweenAmount, bool useCurve = false)
	{
		if (tweenAmount > 0.99999f || IsNearlyEqual(startValue, targetValue))
		{
			base.Value = targetValue;
			return;
		}
		float t = (useCurve ? base.animationCurve.Evaluate(tweenAmount) : tweenAmount);
		base.Value = Lerp(startValue, targetValue, t);
	}

	protected abstract T Lerp(T from, T to, float t);

	protected abstract bool IsNearlyEqual(T startValue, T targetValue);
}
