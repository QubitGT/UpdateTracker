using System;
using UnityEngine;

namespace Oculus.Interaction;

[Serializable]
public class ProgressCurve : ITimeConsumer
{
	[SerializeField]
	private AnimationCurve _animationCurve;

	[SerializeField]
	private float _animationLength;

	private Func<float> _timeProvider = () => Time.time;

	private float _animationStartTime;

	public AnimationCurve AnimationCurve
	{
		get
		{
			return _animationCurve;
		}
		set
		{
			_animationCurve = value;
		}
	}

	public float AnimationLength
	{
		get
		{
			return _animationLength;
		}
		set
		{
			_animationLength = value;
		}
	}

	[Obsolete("Use SetTimeProvider()")]
	public Func<float> TimeProvider
	{
		get
		{
			return _timeProvider;
		}
		set
		{
			_timeProvider = value;
		}
	}

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	public ProgressCurve()
	{
		_animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		_animationLength = 1f;
	}

	public ProgressCurve(AnimationCurve animationCurve, float animationLength)
	{
		_animationCurve = animationCurve;
		_animationLength = animationLength;
	}

	public ProgressCurve(ProgressCurve other)
	{
		Copy(other);
	}

	public void Copy(ProgressCurve other)
	{
		_animationCurve = other._animationCurve;
		_animationLength = other._animationLength;
		_animationStartTime = other._animationStartTime;
		_timeProvider = other._timeProvider;
	}

	public void Start()
	{
		_animationStartTime = _timeProvider();
	}

	public float Progress()
	{
		if (_animationLength <= 0f)
		{
			return _animationCurve.Evaluate(1f);
		}
		float time = Mathf.Clamp01(ProgressTime() / _animationLength);
		return _animationCurve.Evaluate(time);
	}

	public float ProgressIn(float time)
	{
		if (_animationLength <= 0f)
		{
			return _animationCurve.Evaluate(1f);
		}
		float time2 = Mathf.Clamp01((ProgressTime() + time) / _animationLength);
		return _animationCurve.Evaluate(time2);
	}

	public float ProgressTime()
	{
		return Mathf.Clamp(_timeProvider() - _animationStartTime, 0f, _animationLength);
	}

	public void End()
	{
		_animationStartTime = _timeProvider() - _animationLength;
	}
}
