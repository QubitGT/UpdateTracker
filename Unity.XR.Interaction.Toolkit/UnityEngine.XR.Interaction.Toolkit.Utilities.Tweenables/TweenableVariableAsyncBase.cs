using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class TweenableVariableAsyncBase<T> : TweenableVariableBase<T>, IDisposable where T : struct, IEquatable<T>
{
	private bool m_OutputInitialized;

	private NativeArray<T> m_JobOutputStore;

	private bool m_CurveDirty = true;

	private NativeCurve m_NativeCurve;

	private bool m_HasJobPending;

	private JobHandle m_LastJobHandle;

	public new T Value
	{
		get
		{
			return base.Value;
		}
		set
		{
			if (m_HasJobPending && m_OutputInitialized)
			{
				CompleteJob();
				m_JobOutputStore[0] = value;
			}
			base.Value = value;
		}
	}

	public void Dispose()
	{
		if (m_OutputInitialized)
		{
			UpdateStateFromCompletedJob();
			m_JobOutputStore.Dispose();
			m_OutputInitialized = false;
		}
		if (m_NativeCurve.isCreated)
		{
			m_NativeCurve.Dispose();
			m_CurveDirty = true;
		}
	}

	private NativeCurve GetNativeCurve()
	{
		RefreshCurve();
		return m_NativeCurve;
	}

	private void RefreshCurve()
	{
		if (m_CurveDirty || !m_NativeCurve.isCreated)
		{
			m_NativeCurve.Update(base.animationCurve, 1024);
			m_CurveDirty = false;
		}
	}

	protected override void PreprocessTween()
	{
		base.PreprocessTween();
		UpdateStateFromCompletedJob();
	}

	protected override void ExecuteTween(T startValue, T targetValue, float tweenAmount, bool useCurve = false)
	{
		if (tweenAmount > 0.99999f)
		{
			Value = targetValue;
			return;
		}
		T stateOriginValue = (useCurve ? startValue : targetValue);
		float tweenAmount2 = (useCurve ? 1f : tweenAmount);
		byte stateTransitionIncrement = (useCurve ? ((byte)math.ceil(tweenAmount * 255f)) : byte.MaxValue);
		TweenJobData<T> tweenJobData = default(TweenJobData<T>);
		tweenJobData.initialValue = base.initialValue;
		tweenJobData.stateOriginValue = stateOriginValue;
		tweenJobData.stateTargetValue = targetValue;
		tweenJobData.stateTransitionIncrement = stateTransitionIncrement;
		tweenJobData.nativeCurve = GetNativeCurve();
		tweenJobData.tweenStartValue = startValue;
		tweenJobData.tweenAmount = tweenAmount2;
		tweenJobData.outputData = GetJobOutputStore();
		TweenJobData<T> jobData = tweenJobData;
		m_LastJobHandle = ScheduleTweenJob(ref jobData);
		m_HasJobPending = true;
	}

	private void UpdateStateFromCompletedJob()
	{
		if (CompleteJob())
		{
			Value = GetJobOutputStore()[0];
		}
	}

	protected abstract JobHandle ScheduleTweenJob(ref TweenJobData<T> jobData);

	private NativeArray<T> GetJobOutputStore()
	{
		if (!m_OutputInitialized)
		{
			m_JobOutputStore = new NativeArray<T>(1, Allocator.Persistent);
			m_OutputInitialized = true;
			DisposableManagerSingleton.RegisterDisposable(this);
		}
		return m_JobOutputStore;
	}

	protected override void OnAnimationCurveChanged(AnimationCurve value)
	{
		base.OnAnimationCurveChanged(value);
		m_CurveDirty = true;
	}

	private bool CompleteJob()
	{
		if (!m_OutputInitialized || !m_HasJobPending)
		{
			return false;
		}
		m_LastJobHandle.Complete();
		m_LastJobHandle = default(JobHandle);
		m_HasJobPending = false;
		return true;
	}
}
