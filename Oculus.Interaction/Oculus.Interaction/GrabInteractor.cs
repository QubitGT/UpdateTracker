using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.Throw;
using UnityEngine;

namespace Oculus.Interaction;

public class GrabInteractor : PointerInteractor<GrabInteractor, GrabInteractable>, IRigidbodyRef
{
	[Tooltip("The selection mechanism that broadcasts select and release events. For example, a ControllerSelector.")]
	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	private UnityEngine.Object _selector;

	[Tooltip("The hand or controller's Rigidbody, which detects interactables.")]
	[SerializeField]
	private Rigidbody _rigidbody;

	[Tooltip("The center of the grab.")]
	[SerializeField]
	[Optional]
	private Transform _grabCenter;

	[Tooltip("The location where the interactable will move when selected.")]
	[SerializeField]
	[Optional]
	private Transform _grabTarget;

	private Collider[] _colliders;

	private Tween _tween;

	private bool _outsideReleaseDist;

	[Tooltip("Determines how the object will move when thrown.")]
	[SerializeField]
	[Interface(typeof(IThrowVelocityCalculator), new Type[] { })]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	[Obsolete("Use Grabbable instead")]
	private UnityEngine.Object _velocityCalculator;

	private GrabInteractable _selectedInteractableOverride;

	private bool _isSelectionOverriden;

	public Rigidbody Rigidbody => _rigidbody;

	[Obsolete("Use Grabbable instead")]
	public IThrowVelocityCalculator VelocityCalculator { get; set; }

	protected override void Awake()
	{
		base.Awake();
		base.Selector = _selector as ISelector;
		VelocityCalculator = _velocityCalculator as IThrowVelocityCalculator;
		_nativeId = 5148284398804954994uL;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		_colliders = Rigidbody.GetComponentsInChildren<Collider>();
		Collider[] colliders = _colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			_ = colliders[i];
		}
		if (_grabCenter == null)
		{
			_grabCenter = base.transform;
		}
		if (_grabTarget == null)
		{
			_grabTarget = _grabCenter;
		}
		_ = _velocityCalculator != null;
		_tween = new Tween(Pose.identity);
		this.EndStart(ref _started);
	}

	protected override void DoPreprocess()
	{
		base.transform.position = _grabCenter.position;
		base.transform.rotation = _grabCenter.rotation;
	}

	protected override GrabInteractable ComputeCandidate()
	{
		Vector3 position = Rigidbody.transform.position;
		GrabInteractable result = null;
		GrabPoseScore referenceScore = GrabPoseScore.Max;
		foreach (GrabInteractable item in Interactable<GrabInteractor, GrabInteractable>.Registry.List(this))
		{
			_ = item.Colliders;
			Vector3 hitPoint;
			GrabPoseScore grabPoseScore = GrabPoseHelper.CollidersScore(position, item.Colliders, out hitPoint);
			if (grabPoseScore.IsBetterThan(referenceScore))
			{
				referenceScore = grabPoseScore;
				result = item;
			}
		}
		return result;
	}

	public void ForceSelect(GrabInteractable interactable)
	{
		_isSelectionOverriden = true;
		_selectedInteractableOverride = interactable;
		SetComputeCandidateOverride(() => interactable);
		SetComputeShouldSelectOverride(() => (object)interactable == base.Interactable);
		SetComputeShouldUnselectOverride(() => (object)interactable != base.SelectedInteractable, clearOverrideOnUnselect: false);
	}

	public void ForceRelease()
	{
		_isSelectionOverriden = false;
		_selectedInteractableOverride = null;
		ClearComputeCandidateOverride();
		ClearComputeShouldSelectOverride();
		if (base.State == InteractorState.Select)
		{
			SetComputeShouldUnselectOverride(() => true);
		}
		else
		{
			ClearComputeShouldUnselectOverride();
		}
	}

	public override void Unselect()
	{
		if (base.State == InteractorState.Select && _isSelectionOverriden && (base.SelectedInteractable == _selectedInteractableOverride || base.SelectedInteractable == null))
		{
			_isSelectionOverriden = false;
			_selectedInteractableOverride = null;
			ClearComputeShouldUnselectOverride();
		}
		base.Unselect();
	}

	protected override void InteractableSelected(GrabInteractable interactable)
	{
		Pose pose = _grabTarget.GetPose();
		Pose grabSourceForTarget = _interactable.GetGrabSourceForTarget(pose);
		_tween.StopAndSetPose(grabSourceForTarget);
		base.InteractableSelected(interactable);
		_tween.MoveTo(pose);
	}

	protected override void InteractableUnselected(GrabInteractable interactable)
	{
		base.InteractableUnselected(interactable);
		ReleaseVelocityInformation releaseVelocityInformation = ((VelocityCalculator != null) ? VelocityCalculator.CalculateThrowVelocity(interactable.transform) : new ReleaseVelocityInformation(Vector3.zero, Vector3.zero, Vector3.zero));
		interactable.ApplyVelocities(releaseVelocityInformation.LinearVelocity, releaseVelocityInformation.AngularVelocity);
	}

	protected override void HandlePointerEventRaised(PointerEvent evt)
	{
		base.HandlePointerEventRaised(evt);
		if (!(base.SelectedInteractable == null) && (evt.Type == PointerEventType.Select || evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel))
		{
			Pose pose = _grabTarget.GetPose();
			if (base.SelectedInteractable.ResetGrabOnGrabsUpdated)
			{
				Pose grabSourceForTarget = _interactable.GetGrabSourceForTarget(pose);
				_tween.StopAndSetPose(grabSourceForTarget);
				base.SelectedInteractable.PointableElement.ProcessPointerEvent(new PointerEvent(base.Identifier, PointerEventType.Move, _tween.Pose, base.Data));
				_tween.MoveTo(pose);
			}
			else
			{
				_tween.StopAndSetPose(pose);
				base.SelectedInteractable.PointableElement.ProcessPointerEvent(new PointerEvent(base.Identifier, PointerEventType.Move, pose, base.Data));
				_tween.MoveTo(pose);
			}
		}
	}

	protected override Pose ComputePointerPose()
	{
		if (base.SelectedInteractable != null)
		{
			return _tween.Pose;
		}
		return _grabTarget.GetPose();
	}

	protected override void DoSelectUpdate()
	{
		GrabInteractable selectedInteractable = _selectedInteractable;
		if (selectedInteractable == null)
		{
			return;
		}
		_tween.UpdateTarget(_grabTarget.GetPose());
		_tween.Tick();
		_outsideReleaseDist = false;
		if (selectedInteractable.ReleaseDistance > 0f)
		{
			float num = float.MaxValue;
			Collider[] colliders = selectedInteractable.Colliders;
			for (int i = 0; i < colliders.Length; i++)
			{
				float sqrMagnitude = (colliders[i].bounds.center - Rigidbody.transform.position).sqrMagnitude;
				num = Mathf.Min(num, sqrMagnitude);
			}
			float num2 = selectedInteractable.ReleaseDistance * selectedInteractable.ReleaseDistance;
			if (num > num2)
			{
				_outsideReleaseDist = true;
			}
		}
	}

	protected override bool ComputeShouldUnselect()
	{
		if (!_outsideReleaseDist)
		{
			return base.ComputeShouldUnselect();
		}
		return true;
	}

	public void InjectAllGrabInteractor(ISelector selector, Rigidbody rigidbody)
	{
		InjectSelector(selector);
		InjectRigidbody(rigidbody);
	}

	public void InjectSelector(ISelector selector)
	{
		_selector = selector as UnityEngine.Object;
		base.Selector = selector;
	}

	public void InjectRigidbody(Rigidbody rigidbody)
	{
		_rigidbody = rigidbody;
	}

	public void InjectOptionalGrabCenter(Transform grabCenter)
	{
		_grabCenter = grabCenter;
	}

	public void InjectOptionalGrabTarget(Transform grabTarget)
	{
		_grabTarget = grabTarget;
	}

	[Obsolete("Use Grabbable instead")]
	public void InjectOptionalVelocityCalculator(IThrowVelocityCalculator velocityCalculator)
	{
		_velocityCalculator = velocityCalculator as UnityEngine.Object;
		VelocityCalculator = velocityCalculator;
	}
}
