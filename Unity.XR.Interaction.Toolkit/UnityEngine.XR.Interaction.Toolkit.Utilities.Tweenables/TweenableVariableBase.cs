using System;
using System.Collections;
using Unity.XR.CoreUtils.Bindings.Variables;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class TweenableVariableBase<T> : BindableVariable<T> where T : IEquatable<T>
{
	protected const float k_NearlyOne = 0.99999f;

	private AnimationCurve m_AnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private T m_Target;

	public AnimationCurve animationCurve
	{
		get
		{
			return m_AnimationCurve;
		}
		set
		{
			m_AnimationCurve = value;
			OnAnimationCurveChanged(value);
		}
	}

	public T target
	{
		get
		{
			return m_Target;
		}
		set
		{
			if (!m_Target.Equals(value))
			{
				m_Target = value;
				OnTargetChanged(m_Target);
			}
		}
	}

	public T initialValue { get; set; }

	public void HandleTween(float tweenTarget)
	{
		if (!ValueEquals(target))
		{
			PreprocessTween();
			ExecuteTween(base.Value, target, tweenTarget);
		}
	}

	protected abstract void ExecuteTween(T startValue, T targetValue, float tweenAmount, bool useCurve = false);

	public IEnumerator StartAutoTween(float deltaTimeMultiplier)
	{
		while (true)
		{
			HandleTween(Time.deltaTime * deltaTimeMultiplier);
			yield return null;
		}
	}

	public IEnumerator PlaySequence(T start, T finish, float duration, Action onComplete = null)
	{
		for (float timeElapsed = 0f; timeElapsed < duration; timeElapsed += Time.deltaTime)
		{
			PreprocessTween();
			float tweenAmount = Mathf.Clamp01(timeElapsed / duration);
			ExecuteTween(start, finish, tweenAmount, useCurve: true);
			yield return null;
		}
		PreprocessTween();
		ExecuteTween(start, finish, 1f);
		onComplete?.Invoke();
	}

	protected virtual void OnAnimationCurveChanged(AnimationCurve value)
	{
	}

	protected virtual void OnTargetChanged(T newTarget)
	{
	}

	protected virtual void PreprocessTween()
	{
	}

	protected TweenableVariableBase()
		: base(default(T), checkEquality: true, (Func<T, T, bool>)null, startInitialized: false)
	{
	}
}
