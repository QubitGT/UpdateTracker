using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class BaseAsyncAffordanceStateReceiver<T> : BaseAffordanceStateReceiver<T>, IAsyncAffordanceStateReceiver, IAffordanceStateReceiver where T : struct, IEquatable<T>
{
	private NativeArray<T> m_JobOutputStore;

	private NativeCurve m_NativeCurve;

	private JobHandle m_LastJobHandle;

	private bool m_OutputInitialized;

	protected virtual void OnDestroy()
	{
		m_LastJobHandle.Complete();
		if (m_JobOutputStore.IsCreated)
		{
			m_JobOutputStore.Dispose();
		}
		if (m_NativeCurve.isCreated)
		{
			m_NativeCurve.Dispose();
		}
	}

	public JobHandle HandleTween(float tweenTarget)
	{
		CaptureInitialValue();
		AffordanceStateData value = base.currentAffordanceStateData.Value;
		AffordanceThemeData<T> affordanceThemeDataForIndex = base.affordanceTheme.GetAffordanceThemeDataForIndex(value.stateIndex);
		if (affordanceThemeDataForIndex == null)
		{
			string nameForIndex = AffordanceStateShortcuts.GetNameForIndex(value.stateIndex);
			XRLoggingUtils.LogError($"Missing theme data for affordance state index {value.stateIndex} \"{nameForIndex}\" with {this}.", this);
			return default(JobHandle);
		}
		T animationStateStartValue = affordanceThemeDataForIndex.animationStateStartValue;
		T animationStateEndValue = affordanceThemeDataForIndex.animationStateEndValue;
		if (base.replaceIdleStateValueWithInitialValue && value.stateIndex == 1)
		{
			animationStateStartValue = base.initialValue;
			animationStateEndValue = base.initialValue;
		}
		TweenJobData<T> tweenJobData = default(TweenJobData<T>);
		tweenJobData.initialValue = base.initialValue;
		tweenJobData.stateOriginValue = ProcessTargetAffordanceValue(animationStateStartValue);
		tweenJobData.stateTargetValue = ProcessTargetAffordanceValue(animationStateEndValue);
		tweenJobData.stateTransitionIncrement = value.stateTransitionIncrement;
		tweenJobData.nativeCurve = m_NativeCurve;
		tweenJobData.tweenStartValue = base.currentAffordanceValue.Value;
		tweenJobData.tweenAmount = tweenTarget;
		tweenJobData.outputData = GetJobOutputStore();
		TweenJobData<T> jobData = tweenJobData;
		m_LastJobHandle = ScheduleTweenJob(ref jobData);
		return m_LastJobHandle;
	}

	public void UpdateStateFromCompletedJob()
	{
		if (m_OutputInitialized)
		{
			ConsumeAffordance(GetJobOutputStore()[0]);
		}
	}

	protected abstract JobHandle ScheduleTweenJob(ref TweenJobData<T> jobData);

	protected override void OnAffordanceThemeChanged(BaseAffordanceTheme<T> newValue)
	{
		base.OnAffordanceThemeChanged(newValue);
		m_NativeCurve.Update(newValue.animationCurve, 1024);
	}

	private NativeArray<T> GetJobOutputStore()
	{
		if (!m_OutputInitialized && base.enabled)
		{
			m_JobOutputStore = new NativeArray<T>(1, Allocator.Persistent);
			m_OutputInitialized = true;
		}
		return m_JobOutputStore;
	}
}
