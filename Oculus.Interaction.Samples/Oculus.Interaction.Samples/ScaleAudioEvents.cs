using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Samples;

public class ScaleAudioEvents : MonoBehaviour
{
	private enum Direction
	{
		None,
		ScaleUp,
		ScaleDown
	}

	[SerializeField]
	[Interface(typeof(IInteractableView), new Type[] { })]
	private UnityEngine.Object _interactableView;

	[Tooltip("Transform to track scale of. If not provided, transform of this component is used.")]
	[SerializeField]
	[Optional]
	private Transform _trackedTransform;

	[Tooltip("The increase in scale magnitude that will fire the step event")]
	[SerializeField]
	private float _stepSize = 0.4f;

	[Tooltip("Events will not be fired more frequently than this many times per second")]
	[SerializeField]
	private int _maxEventFreq = 20;

	[SerializeField]
	private UnityEvent _whenScalingStarted = new UnityEvent();

	[SerializeField]
	private UnityEvent _whenScalingEnded = new UnityEvent();

	[SerializeField]
	private UnityEvent _whenScaledUp = new UnityEvent();

	[SerializeField]
	private UnityEvent _whenScaledDown = new UnityEvent();

	private IInteractableView InteractableView;

	private bool _isScaling;

	private Vector3 _lastStep;

	private float _lastEventTime;

	private Direction _direction;

	protected bool _started;

	public UnityEvent WhenScalingStarted => _whenScalingStarted;

	public UnityEvent WhenScalingEnded => _whenScalingEnded;

	public UnityEvent WhenScaledUp => _whenScaledUp;

	public UnityEvent WhenScaledDown => _whenScaledDown;

	private Transform TrackedTransform
	{
		get
		{
			if (!(_trackedTransform == null))
			{
				return _trackedTransform;
			}
			return base.transform;
		}
	}

	private void ScalingStarted()
	{
		_lastStep = TrackedTransform.localScale;
		_whenScalingStarted.Invoke();
	}

	private void ScalingEnded()
	{
		_whenScalingEnded.Invoke();
	}

	private float GetTotalDelta(out Direction direction)
	{
		float magnitude = _lastStep.magnitude;
		float magnitude2 = TrackedTransform.localScale.magnitude;
		if (magnitude2 == magnitude)
		{
			direction = Direction.None;
		}
		else
		{
			direction = ((magnitude2 > magnitude) ? Direction.ScaleUp : Direction.ScaleDown);
		}
		if (direction != Direction.ScaleUp)
		{
			return magnitude - magnitude2;
		}
		return magnitude2 - magnitude;
	}

	private void UpdateScaling()
	{
		if (_stepSize <= 0f || _maxEventFreq <= 0)
		{
			return;
		}
		float stepSize = _stepSize;
		if (!(GetTotalDelta(out _direction) > stepSize))
		{
			return;
		}
		_lastStep = TrackedTransform.localScale;
		if (Time.time - _lastEventTime >= 1f / (float)_maxEventFreq)
		{
			_lastEventTime = Time.time;
			if (_direction == Direction.ScaleUp)
			{
				_whenScaledUp.Invoke();
			}
			else
			{
				_whenScaledDown.Invoke();
			}
		}
	}

	protected virtual void Awake()
	{
		InteractableView = _interactableView as IInteractableView;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void Update()
	{
		bool isScaling = _isScaling;
		_isScaling = InteractableView.State == InteractableState.Select;
		if (!_isScaling)
		{
			if (isScaling)
			{
				ScalingEnded();
			}
			return;
		}
		if (!isScaling)
		{
			ScalingStarted();
		}
		UpdateScaling();
	}
}
