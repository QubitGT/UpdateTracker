using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class BaseSynchronousAffordanceStateReceiver<T> : BaseAffordanceStateReceiver<T>, ISynchronousAffordanceStateReceiver, IAffordanceStateReceiver where T : struct, IEquatable<T>
{
	public virtual void HandleTween(float tweenTarget)
	{
		CaptureInitialValue();
		AffordanceStateData value = base.currentAffordanceStateData.Value;
		AffordanceThemeData<T> affordanceThemeDataForIndex = base.affordanceTheme.GetAffordanceThemeDataForIndex(value.stateIndex);
		if (affordanceThemeDataForIndex == null)
		{
			string nameForIndex = AffordanceStateShortcuts.GetNameForIndex(value.stateIndex);
			XRLoggingUtils.LogError($"Missing theme data for affordance state index {value.stateIndex} \"{nameForIndex}\" with {this}.", this);
			return;
		}
		float interpolationAmount = base.affordanceTheme.animationCurve.Evaluate(value.stateTransitionAmountFloat);
		T newValue = ((base.replaceIdleStateValueWithInitialValue && value.stateIndex == 1) ? base.initialValue : Interpolate(affordanceThemeDataForIndex.animationStateStartValue, affordanceThemeDataForIndex.animationStateEndValue, interpolationAmount));
		T targetValue = ProcessTargetAffordanceValue(newValue);
		T newValue2 = Interpolate(base.currentAffordanceValue.Value, targetValue, tweenTarget);
		ConsumeAffordance(newValue2);
	}

	protected abstract T Interpolate(T startValue, T targetValue, float interpolationAmount);
}
