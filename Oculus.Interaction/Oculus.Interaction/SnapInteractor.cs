using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class SnapInteractor : Interactor<SnapInteractor, SnapInteractable>, IRigidbodyRef
{
	[Tooltip("The object's Grabbable component.")]
	[SerializeField]
	private PointableElement _pointableElement;

	[Tooltip("The object's RigidBody component.")]
	[SerializeField]
	private Rigidbody _rigidbody;

	[Tooltip("Used to determine which object should snap to your hand when there are multiple to choose from. Objects with a lower threshold have a higher priority.")]
	[SerializeField]
	private float _distanceThreshold = 0.01f;

	[SerializeField]
	[Optional]
	[FormerlySerializedAs("_snapPoint")]
	[FormerlySerializedAs("_dropPoint")]
	private Transform _snapPoseTransform;

	[Tooltip("The default Interactable to snap to until you interact with the object.")]
	[SerializeField]
	[Optional]
	private SnapInteractable _defaultInteractable;

	[SerializeField]
	[Optional]
	[Tooltip("Interactable to automatically snap to when the associated Pointable is not being pointed at for Time-Out seconds")]
	private SnapInteractable _timeOutInteractable;

	[SerializeField]
	[Optional]
	[Tooltip("When the associated Pointable is not being pointed at for Time-Out seconds the SnapInteractor will snap to the TimeOutInteractable, unless it is null.")]
	private float _timeOut;

	private float _idleStarted = -1f;

	private IMovement _movement;

	private bool _shouldSelect;

	private bool _shouldUnselect;

	public IPointableElement PointableElement => _pointableElement;

	public Rigidbody Rigidbody => _rigidbody;

	public Pose SnapPose => _snapPoseTransform.GetPose();

	public float DistanceThreshold
	{
		get
		{
			return _distanceThreshold;
		}
		set
		{
			_distanceThreshold = value;
		}
	}

	private void Reset()
	{
		_rigidbody = GetComponentInParent<Rigidbody>();
		_pointableElement = GetComponentInParent<PointableElement>();
	}

	protected override void Awake()
	{
		base.Awake();
		_nativeId = 6011849687482789746uL;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		if (_snapPoseTransform == null)
		{
			_snapPoseTransform = base.transform;
		}
		this.EndStart(ref _started);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!_started)
		{
			return;
		}
		_pointableElement.WhenPointerEventRaised += HandlePointerEventRaised;
		if (_defaultInteractable != null)
		{
			SetComputeCandidateOverride(() => _defaultInteractable);
			SetComputeShouldSelectOverride(() => true);
		}
	}

	protected override void OnDisable()
	{
		if (_started)
		{
			_pointableElement.WhenPointerEventRaised -= HandlePointerEventRaised;
		}
		base.OnDisable();
	}

	protected override bool ComputeShouldSelect()
	{
		return _shouldSelect;
	}

	protected override bool ComputeShouldUnselect()
	{
		return _shouldUnselect;
	}

	protected override void DoHoverUpdate()
	{
		base.DoHoverUpdate();
		_shouldUnselect = false;
		if (!(base.Interactable == null))
		{
			GeneratePointerEvent(PointerEventType.Move);
			base.Interactable.InteractorHoverUpdated(this);
		}
	}

	protected override void DoSelectUpdate()
	{
		base.DoSelectUpdate();
		Pose result;
		if (_movement == null || base.Interactable == null)
		{
			_shouldUnselect = true;
		}
		else if (base.Interactable.PoseForInteractor(this, out result))
		{
			_movement.UpdateTarget(result);
			_movement.Tick();
			GeneratePointerEvent(PointerEventType.Move);
		}
		else
		{
			_shouldUnselect = true;
		}
	}

	protected override void InteractableSet(SnapInteractable interactable)
	{
		base.InteractableSet(interactable);
		if (interactable != null)
		{
			GeneratePointerEvent(PointerEventType.Hover);
		}
	}

	protected override void InteractableUnset(SnapInteractable interactable)
	{
		if (interactable != null)
		{
			GeneratePointerEvent(PointerEventType.Unhover);
		}
		base.InteractableUnset(interactable);
	}

	protected override void InteractableSelected(SnapInteractable interactable)
	{
		base.InteractableSelected(interactable);
		_shouldSelect = false;
		if (interactable != null)
		{
			Pose from = _snapPoseTransform.GetPose();
			_movement = interactable.GenerateMovement(in from, this);
			if (_movement != null)
			{
				GeneratePointerEvent(PointerEventType.Select);
			}
		}
	}

	protected override void InteractableUnselected(SnapInteractable interactable)
	{
		_movement?.StopAndSetPose(_movement.Pose);
		if (interactable != null)
		{
			GeneratePointerEvent(PointerEventType.Unselect);
		}
		base.InteractableUnselected(interactable);
		_movement = null;
	}

	protected virtual void HandlePointerEventRaised(PointerEvent evt)
	{
		if (_pointableElement.SelectingPointsCount == 0 && evt.Identifier != base.Identifier && evt.Type == PointerEventType.Unselect && base.Interactable != null)
		{
			_shouldSelect = true;
		}
		if (evt.Identifier == base.Identifier && evt.Type == PointerEventType.Cancel && base.Interactable != null)
		{
			base.Interactable.RemoveInteractorByIdentifier(base.Identifier);
		}
	}

	private void GeneratePointerEvent(PointerEventType pointerEventType)
	{
		Pose pose = ComputePointerPose();
		_pointableElement.ProcessPointerEvent(new PointerEvent(base.Identifier, pointerEventType, pose, base.Data));
	}

	protected override void DoPreprocess()
	{
		if (_pointableElement.Points.Count == 0)
		{
			if (_idleStarted < 0f)
			{
				_idleStarted = Time.time;
			}
		}
		else
		{
			_idleStarted = -1f;
		}
	}

	protected Pose ComputePointerPose()
	{
		if (_movement != null)
		{
			return _movement.Pose;
		}
		return SnapPose;
	}

	private bool TimedOut()
	{
		if (_timeOutInteractable != null && _timeOut >= 0f && _idleStarted >= 0f)
		{
			return Time.time - _idleStarted > _timeOut;
		}
		return false;
	}

	protected override SnapInteractable ComputeCandidate()
	{
		if (TimedOut())
		{
			_shouldSelect = true;
			return _timeOutInteractable;
		}
		if (_pointableElement.SelectingPointsCount == 0)
		{
			if (!_shouldSelect)
			{
				return null;
			}
			return base.Interactable;
		}
		float num = _distanceThreshold * _distanceThreshold;
		SnapInteractable result = null;
		float num2 = float.MaxValue;
		float num3 = float.MaxValue;
		foreach (SnapInteractable item in Interactable<SnapInteractor, SnapInteractable>.Registry.List(this))
		{
			if (!item.PoseForInteractor(this, out var result2))
			{
				continue;
			}
			float sqrMagnitude = (result2.position - _snapPoseTransform.position).sqrMagnitude;
			if (!(sqrMagnitude > num2))
			{
				float num4 = Quaternion.Angle(result2.rotation, _snapPoseTransform.rotation);
				if (!(Mathf.Abs(sqrMagnitude - num2) < num) || !(num4 >= num3))
				{
					num2 = sqrMagnitude;
					num3 = num4;
					result = item;
				}
			}
		}
		return result;
	}

	public void InjectAllSnapInteractor(PointableElement pointableElement, Rigidbody rigidbody)
	{
		InjectPointableElement(pointableElement);
		InjectRigidbody(rigidbody);
	}

	public void InjectPointableElement(PointableElement pointableElement)
	{
		_pointableElement = pointableElement;
	}

	public void InjectRigidbody(Rigidbody rigidbody)
	{
		_rigidbody = rigidbody;
	}

	public void InjectOptionalSnapPoseTransform(Transform snapPoint)
	{
		_snapPoseTransform = snapPoint;
	}

	public void InjectOptionalTimeOutInteractable(SnapInteractable interactable)
	{
		_timeOutInteractable = interactable;
	}

	public void InjectOptionaTimeOut(float timeOut)
	{
		_timeOut = timeOut;
	}
}
