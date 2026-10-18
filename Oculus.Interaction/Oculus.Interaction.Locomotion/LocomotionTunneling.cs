using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotionTunneling : MonoBehaviour, ITimeConsumer, IDeltaTimeConsumer
{
	[SerializeField]
	[Interface(typeof(ILocomotionEventHandler), new Type[] { })]
	private UnityEngine.Object _locomotor;

	[SerializeField]
	private TunnelingEffect _tunneling;

	[SerializeField]
	private AnimationCurve _rotationStrength;

	[SerializeField]
	private AnimationCurve _accelerationStrength;

	[SerializeField]
	private AnimationCurve _movementStrength;

	[SerializeField]
	private float _fadeOutTime = 0.2f;

	[SerializeField]
	private float _fadeOutWait = 0.2f;

	private Func<float> _deltaTimeProvider = () => Time.deltaTime;

	private Func<float> _timeProvider = () => Time.time;

	private bool _started;

	private Vector3 _lastVelocity = Vector3.zero;

	private float _fadeOutStart;

	private ILocomotionEventHandler Locomotor { get; set; }

	public AnimationCurve RotationStrength
	{
		get
		{
			return _rotationStrength;
		}
		set
		{
			_rotationStrength = value;
		}
	}

	public AnimationCurve AccelerationStrength
	{
		get
		{
			return _accelerationStrength;
		}
		set
		{
			_accelerationStrength = value;
		}
	}

	public AnimationCurve MovementStrength
	{
		get
		{
			return _movementStrength;
		}
		set
		{
			_movementStrength = value;
		}
	}

	public float FadeOutTime
	{
		get
		{
			return _fadeOutTime;
		}
		set
		{
			_fadeOutTime = value;
		}
	}

	public float FadeOutWait
	{
		get
		{
			return _fadeOutWait;
		}
		set
		{
			_fadeOutWait = value;
		}
	}

	public void SetDeltaTimeProvider(Func<float> deltaTimeProvider)
	{
		_deltaTimeProvider = deltaTimeProvider;
	}

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		Locomotor = _locomotor as ILocomotionEventHandler;
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
			_tunneling.UseAimingTarget = false;
			_tunneling.UserFOV = 360f;
			_lastVelocity = Vector3.zero;
			Locomotor.WhenLocomotionEventHandled += HandleLocomotionEventHandled;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_tunneling.enabled = false;
			Locomotor.WhenLocomotionEventHandled -= HandleLocomotionEventHandled;
		}
	}

	private void HandleLocomotionEventHandled(LocomotionEvent locomotionEvent, Pose pose)
	{
		if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Velocity)
		{
			locomotionEvent.Pose.rotation.ToAngleAxis(out var angle, out var _);
			float fOV = _rotationStrength.Evaluate(Mathf.Abs(angle));
			SetFOV(fOV);
		}
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Velocity)
		{
			Vector3 position = pose.position;
			float time = (position - _lastVelocity).magnitude / _deltaTimeProvider();
			_lastVelocity = position;
			float b = _movementStrength.Evaluate(position.magnitude);
			float a = _accelerationStrength.Evaluate(time);
			SetFOV(Mathf.Min(a, b));
		}
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative || locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute || locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel)
		{
			float fOV2 = _movementStrength.Evaluate(pose.position.magnitude);
			SetFOV(fOV2);
		}
	}

	private void SetFOV(float fov)
	{
		if (!Mathf.Approximately(fov, 0f))
		{
			_tunneling.enabled = true;
			_tunneling.UserFOV = Mathf.Min(_tunneling.UserFOV, fov);
			_tunneling.AlphaStrength = 1f;
			_fadeOutStart = _timeProvider();
		}
	}

	protected virtual void LateUpdate()
	{
		float num = _timeProvider() - _fadeOutStart;
		if (!(num < _fadeOutWait))
		{
			float num2 = num - _fadeOutWait;
			float num3 = Mathf.Lerp(1f, 0f, num2 / _fadeOutTime);
			_tunneling.AlphaStrength = num3;
			if (num3 <= 0f)
			{
				_tunneling.enabled = false;
				_tunneling.UserFOV = 360f;
			}
		}
	}
}
