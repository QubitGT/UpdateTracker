using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Samples;

public class RotationAudioEvents : MonoBehaviour
{
	private enum Direction
	{
		None,
		Opening,
		Closing
	}

	[SerializeField]
	[Interface(typeof(IInteractableView), new Type[] { })]
	private UnityEngine.Object _interactableView;

	[Tooltip("Transform to track rotation of. If not provided, transform of this component is used.")]
	[SerializeField]
	[Optional]
	private Transform _trackedTransform;

	[SerializeField]
	private Transform _relativeTo;

	[Tooltip("The angle delta at which the threshold crossed event will be fired.")]
	[SerializeField]
	private float _thresholdDeg = 20f;

	[Tooltip("Maximum rotation arc within which the crossed event will be triggered.")]
	[SerializeField]
	[Range(1f, 150f)]
	private float _maxRangeDeg = 150f;

	[SerializeField]
	private UnityEvent _whenRotationStarted = new UnityEvent();

	[SerializeField]
	private UnityEvent _whenRotationEnded = new UnityEvent();

	[SerializeField]
	private UnityEvent _whenRotatedOpen = new UnityEvent();

	[SerializeField]
	private UnityEvent _whenRotatedClosed = new UnityEvent();

	private IInteractableView InteractableView;

	private float _baseDelta;

	private bool _isRotating;

	private Direction _lastCrossedDirection;

	protected bool _started;

	public UnityEvent WhenRotationStarted => _whenRotationStarted;

	public UnityEvent WhenRotationEnded => _whenRotationEnded;

	public UnityEvent WhenRotatedOpen => _whenRotatedOpen;

	public UnityEvent WhenRotatedClosed => _whenRotatedClosed;

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

	private void RotationStarted()
	{
		_baseDelta = GetTotalDelta();
		_lastCrossedDirection = Direction.None;
		_whenRotationStarted.Invoke();
	}

	private void RotationEnded()
	{
		_whenRotationEnded.Invoke();
	}

	private Quaternion GetCurrentRotation()
	{
		return Quaternion.Inverse(_relativeTo.rotation) * TrackedTransform.rotation;
	}

	private float GetTotalDelta()
	{
		return Quaternion.Angle(_relativeTo.rotation, GetCurrentRotation());
	}

	private void UpdateRotation()
	{
		float totalDelta = GetTotalDelta();
		if (totalDelta > _maxRangeDeg)
		{
			return;
		}
		if (Mathf.Abs(totalDelta - _baseDelta) > _thresholdDeg)
		{
			Direction direction = ((totalDelta - _baseDelta > 0f) ? Direction.Opening : Direction.Closing);
			if (direction != _lastCrossedDirection)
			{
				_lastCrossedDirection = direction;
				if (direction == Direction.Opening)
				{
					_whenRotatedOpen.Invoke();
				}
				else
				{
					_whenRotatedClosed.Invoke();
				}
			}
		}
		if (_lastCrossedDirection == Direction.Opening)
		{
			_baseDelta = Mathf.Max(_baseDelta, totalDelta);
		}
		else if (_lastCrossedDirection == Direction.Closing)
		{
			_baseDelta = Mathf.Min(_baseDelta, totalDelta);
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
		bool isRotating = _isRotating;
		_isRotating = InteractableView.State == InteractableState.Select;
		if (!_isRotating)
		{
			if (isRotating)
			{
				RotationEnded();
			}
			return;
		}
		if (!isRotating)
		{
			RotationStarted();
		}
		UpdateRotation();
	}
}
