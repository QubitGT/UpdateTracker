using System;
using Oculus.Interaction.Throw;
using UnityEngine;

namespace Oculus.Interaction;

public class DistanceGrabInteractor : PointerInteractor<DistanceGrabInteractor, DistanceGrabInteractable>, IDistanceInteractor, IInteractorView
{
	[Tooltip("The selection mechanism to trigger the grab.")]
	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	private UnityEngine.Object _selector;

	[Tooltip("The center of the grab.")]
	[SerializeField]
	[Optional]
	private Transform _grabCenter;

	[Tooltip("The location where the interactable will move when selected.")]
	[SerializeField]
	[Optional]
	private Transform _grabTarget;

	[Tooltip("Determines how the object will move when thrown.")]
	[SerializeField]
	[Interface(typeof(IThrowVelocityCalculator), new Type[] { })]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	[Obsolete("Use Grabbable instead")]
	private UnityEngine.Object _velocityCalculator;

	[SerializeField]
	private DistantCandidateComputer<DistanceGrabInteractor, DistanceGrabInteractable> _distantCandidateComputer = new DistantCandidateComputer<DistanceGrabInteractor, DistanceGrabInteractable>();

	private IMovement _movement;

	[Obsolete("Use Grabbable instead")]
	public IThrowVelocityCalculator VelocityCalculator { get; set; }

	public Pose Origin => _distantCandidateComputer.Origin;

	public Vector3 HitPoint { get; private set; }

	public IRelativeToRef DistanceInteractable => base.Interactable;

	protected override void Awake()
	{
		base.Awake();
		base.Selector = _selector as ISelector;
		VelocityCalculator = _velocityCalculator as IThrowVelocityCalculator;
		_nativeId = 4929598210385797474uL;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		if (_grabCenter == null)
		{
			_grabCenter = base.transform;
		}
		if (_grabTarget == null)
		{
			_grabTarget = _grabCenter;
		}
		_ = _velocityCalculator != null;
		this.EndStart(ref _started);
	}

	protected override void DoPreprocess()
	{
		base.transform.position = _grabCenter.position;
		base.transform.rotation = _grabCenter.rotation;
	}

	protected override DistanceGrabInteractable ComputeCandidate()
	{
		Vector3 bestHitPoint;
		DistanceGrabInteractable result = _distantCandidateComputer.ComputeCandidate(Interactable<DistanceGrabInteractor, DistanceGrabInteractable>.Registry, this, out bestHitPoint);
		HitPoint = bestHitPoint;
		return result;
	}

	protected override void InteractableSelected(DistanceGrabInteractable interactable)
	{
		Pose to = _grabTarget.GetPose();
		_movement = interactable.GenerateMovement(in to);
		base.InteractableSelected(interactable);
		interactable.WhenPointerEventRaised += HandleOtherPointerEventRaised;
	}

	protected override void InteractableUnselected(DistanceGrabInteractable interactable)
	{
		interactable.WhenPointerEventRaised -= HandleOtherPointerEventRaised;
		base.InteractableUnselected(interactable);
		_movement = null;
		ReleaseVelocityInformation releaseVelocityInformation = ((VelocityCalculator != null) ? VelocityCalculator.CalculateThrowVelocity(interactable.transform) : new ReleaseVelocityInformation(Vector3.zero, Vector3.zero, Vector3.zero));
		interactable.ApplyVelocities(releaseVelocityInformation.LinearVelocity, releaseVelocityInformation.AngularVelocity);
	}

	private void HandleOtherPointerEventRaised(PointerEvent evt)
	{
		if (base.SelectedInteractable == null)
		{
			return;
		}
		if (evt.Type == PointerEventType.Select || evt.Type == PointerEventType.Unselect)
		{
			Pose to = _grabTarget.GetPose();
			if (base.SelectedInteractable.ResetGrabOnGrabsUpdated)
			{
				_movement = base.SelectedInteractable.GenerateMovement(in to);
				base.SelectedInteractable.PointableElement.ProcessPointerEvent(new PointerEvent(base.Identifier, PointerEventType.Move, _movement.Pose, base.Data));
			}
		}
		if (evt.Identifier == base.Identifier && evt.Type == PointerEventType.Cancel)
		{
			base.SelectedInteractable.WhenPointerEventRaised -= HandleOtherPointerEventRaised;
		}
	}

	protected override Pose ComputePointerPose()
	{
		if (_movement != null)
		{
			return _movement.Pose;
		}
		return _grabTarget.GetPose();
	}

	protected override void DoSelectUpdate()
	{
		if (!(_selectedInteractable == null))
		{
			_movement.UpdateTarget(_grabTarget.GetPose());
			_movement.Tick();
		}
	}

	public void InjectAllDistanceGrabInteractor(ISelector selector, DistantCandidateComputer<DistanceGrabInteractor, DistanceGrabInteractable> distantCandidateComputer)
	{
		InjectSelector(selector);
		InjectDistantCandidateComputer(distantCandidateComputer);
	}

	public void InjectSelector(ISelector selector)
	{
		_selector = selector as UnityEngine.Object;
		base.Selector = selector;
	}

	public void InjectDistantCandidateComputer(DistantCandidateComputer<DistanceGrabInteractor, DistanceGrabInteractable> distantCandidateComputer)
	{
		_distantCandidateComputer = distantCandidateComputer;
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
