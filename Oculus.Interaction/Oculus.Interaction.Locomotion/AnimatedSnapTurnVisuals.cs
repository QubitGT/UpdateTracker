using System;
using System.Collections;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class AnimatedSnapTurnVisuals : MonoBehaviour, ITimeConsumer
{
	[SerializeField]
	private TurnArrowVisuals _visuals;

	[SerializeField]
	[Interface(typeof(ILocomotionEventBroadcaster), new Type[] { })]
	private UnityEngine.Object _locomotionEventBroadcaster;

	[SerializeField]
	private AnimationCurve _animation;

	[SerializeField]
	private float _highlightOffset = 0.8f;

	private Func<float> _timeProvider = () => Time.time;

	private float _progressValue;

	private Coroutine _animationRoutine;

	protected bool _started;

	private ILocomotionEventBroadcaster LocomotionEventBroadcaster { get; set; }

	public AnimationCurve Animation
	{
		get
		{
			return _animation;
		}
		set
		{
			_animation = value;
		}
	}

	public float HighlightOffset
	{
		get
		{
			return _highlightOffset;
		}
		set
		{
			_highlightOffset = value;
		}
	}

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		LocomotionEventBroadcaster = _locomotionEventBroadcaster as ILocomotionEventBroadcaster;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			LocomotionEventBroadcaster.WhenLocomotionPerformed += HandleLocomotionPerformed;
			_visuals.Progress = 0f;
			_visuals.Value = 0f;
			_visuals.HighLight = false;
			_visuals.UpdateVisual();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			LocomotionEventBroadcaster.WhenLocomotionPerformed -= HandleLocomotionPerformed;
		}
	}

	private void HandleLocomotionPerformed(LocomotionEvent ev)
	{
		if (ev.Rotation == LocomotionEvent.RotationType.Relative)
		{
			StopAnimation();
			float direction = ((Mathf.Repeat(ev.Pose.rotation.eulerAngles.y, 360f) < 180f) ? 1f : (-1f));
			_animationRoutine = StartCoroutine(AnimationRoutine(direction));
		}
	}

	private void StopAnimation()
	{
		if (_animationRoutine != null)
		{
			StopCoroutine(_animationRoutine);
			_animationRoutine = null;
		}
	}

	private IEnumerator AnimationRoutine(float direction)
	{
		float totalTime = _animation.keys[_animation.keys.Length - 1].time;
		float startTime = _timeProvider();
		float ellapsedTime = 0f;
		_visuals.Progress = 0f;
		_visuals.Value = direction;
		_visuals.HighLight = false;
		_visuals.UpdateVisual();
		while (ellapsedTime < totalTime)
		{
			_visuals.Progress = _animation.Evaluate(ellapsedTime);
			_visuals.HighLight = _progressValue > 0.8f;
			ellapsedTime = _timeProvider() - startTime;
			_visuals.UpdateVisual();
			yield return null;
		}
		_visuals.Progress = 0f;
		_visuals.Value = 0f;
		_visuals.HighLight = false;
		_visuals.UpdateVisual();
	}

	public void InjectAllAnimatedSnapTurnVisuals(TurnArrowVisuals visuals, ILocomotionEventBroadcaster locomotionEventBroadcaster)
	{
		InjectVisuals(visuals);
		InjectLocomotionEventBroadcaster(locomotionEventBroadcaster);
	}

	public void InjectVisuals(TurnArrowVisuals visuals)
	{
		_visuals = visuals;
	}

	public void InjectLocomotionEventBroadcaster(ILocomotionEventBroadcaster locomotionEventBroadcaster)
	{
		LocomotionEventBroadcaster = locomotionEventBroadcaster;
		_locomotionEventBroadcaster = locomotionEventBroadcaster as UnityEngine.Object;
	}
}
