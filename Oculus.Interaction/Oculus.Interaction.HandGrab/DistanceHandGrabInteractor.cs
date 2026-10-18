using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using Oculus.Interaction.Throw;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class DistanceHandGrabInteractor : PointerInteractor<DistanceHandGrabInteractor, DistanceHandGrabInteractable>, IHandGrabInteractor, IHandGrabState, IDistanceInteractor, IInteractorView
{
	[Tooltip("The hand to use.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[Tooltip("Detects when the hand grab selects or unselects.")]
	[SerializeField]
	private HandGrabAPI _handGrabApi;

	[Header("Grabbing")]
	[Tooltip("The grab types to support.")]
	[SerializeField]
	private GrabTypeFlags _supportedGrabTypes = GrabTypeFlags.Pinch;

	[Tooltip("The point on the hand used as the origin of the grab.")]
	[SerializeField]
	private Transform _grabOrigin;

	[Tooltip("Specifies an offset from the wrist that can be used to search for the best HandGrabInteractable available, act as a palm grab without a HandPose, and also act as an anchor for attaching the object.")]
	[SerializeField]
	[Optional]
	private Transform _gripPoint;

	[Tooltip("Specifies a moving point at the center of the tips of the currently pinching fingers. It's used to align interactables that don’t have a HandPose to the center of the pinch.")]
	[SerializeField]
	[Optional]
	private Transform _pinchPoint;

	[Tooltip("Determines how the object will move when thrown.")]
	[SerializeField]
	[Interface(typeof(IThrowVelocityCalculator), new Type[] { })]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	[Obsolete("Use Grabbable instead")]
	private UnityEngine.Object _velocityCalculator;

	[SerializeField]
	private DistantCandidateComputer<DistanceHandGrabInteractor, DistanceHandGrabInteractable> _distantCandidateComputer = new DistantCandidateComputer<DistanceHandGrabInteractor, DistanceHandGrabInteractable>();

	private bool _handGrabShouldSelect;

	private bool _handGrabShouldUnselect;

	private HandGrabResult _cachedResult = new HandGrabResult();

	private GrabTypeFlags _currentGrabType;

	public IHand Hand { get; private set; }

	[Obsolete("Use Grabbable instead")]
	public IThrowVelocityCalculator VelocityCalculator { get; set; }

	public IMovement Movement { get; set; }

	public bool MovementFinished { get; set; }

	public HandGrabTarget HandGrabTarget { get; } = new HandGrabTarget();


	public Transform WristPoint => _grabOrigin;

	public Transform PinchPoint => _pinchPoint;

	public Transform PalmPoint => _gripPoint;

	public HandGrabAPI HandGrabApi => _handGrabApi;

	public GrabTypeFlags SupportedGrabTypes => _supportedGrabTypes;

	public IHandGrabInteractable TargetInteractable => base.Interactable;

	public Pose Origin => _distantCandidateComputer.Origin;

	public Vector3 HitPoint { get; private set; }

	public IRelativeToRef DistanceInteractable => base.Interactable;

	public virtual bool IsGrabbing
	{
		get
		{
			if (base.HasSelectedInteractable)
			{
				if (Movement != null)
				{
					return Movement.Stopped;
				}
				return false;
			}
			return false;
		}
	}

	public float FingersStrength { get; private set; }

	public float WristStrength { get; private set; }

	public Pose WristToGrabPoseOffset { get; private set; }

	public HandFingerFlags GrabbingFingers()
	{
		return this.GrabbingFingers(base.SelectedInteractable);
	}

	protected virtual void Reset()
	{
		_hand = GetComponentInParent<IHand>() as MonoBehaviour;
		_handGrabApi = GetComponentInParent<HandGrabAPI>();
	}

	protected override void Awake()
	{
		base.Awake();
		Hand = _hand as IHand;
		VelocityCalculator = _velocityCalculator as IThrowVelocityCalculator;
		_nativeId = 4929598210385797474uL;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		_ = _velocityCalculator != null;
		this.EndStart(ref _started);
	}

	protected override void DoHoverUpdate()
	{
		base.DoHoverUpdate();
		_handGrabShouldSelect = false;
		if (!(base.Interactable == null))
		{
			UpdateTarget(base.Interactable);
			_currentGrabType = this.ComputeShouldSelect(base.Interactable);
			if (_currentGrabType != 0)
			{
				_handGrabShouldSelect = true;
			}
		}
	}

	protected override void InteractableSet(DistanceHandGrabInteractable interactable)
	{
		base.InteractableSet(interactable);
		UpdateTarget(base.Interactable);
	}

	protected override void InteractableUnset(DistanceHandGrabInteractable interactable)
	{
		base.InteractableUnset(interactable);
		SetGrabStrength(0f);
	}

	protected override void DoSelectUpdate()
	{
		_handGrabShouldUnselect = false;
		if (base.SelectedInteractable == null)
		{
			_handGrabShouldUnselect = true;
			return;
		}
		UpdateTargetSliding(base.SelectedInteractable);
		Pose handGrabPose = this.GetHandGrabPose();
		Movement.UpdateTarget(handGrabPose);
		Movement.Tick();
		GrabTypeFlags grabTypeFlags = this.ComputeShouldSelect(base.SelectedInteractable);
		GrabTypeFlags grabTypeFlags2 = this.ComputeShouldUnselect(base.SelectedInteractable);
		_currentGrabType |= grabTypeFlags;
		_currentGrabType &= ~grabTypeFlags2;
		if (grabTypeFlags2 != 0 && _currentGrabType == GrabTypeFlags.None)
		{
			_handGrabShouldUnselect = true;
		}
	}

	protected override void InteractableSelected(DistanceHandGrabInteractable interactable)
	{
		if (interactable != null)
		{
			WristToGrabPoseOffset = this.GetGrabOffset();
			Movement = this.GenerateMovement(interactable);
			SetGrabStrength(1f);
		}
		base.InteractableSelected(interactable);
	}

	protected override void InteractableUnselected(DistanceHandGrabInteractable interactable)
	{
		base.InteractableUnselected(interactable);
		Movement = null;
		_currentGrabType = GrabTypeFlags.None;
		ReleaseVelocityInformation releaseVelocityInformation = ((VelocityCalculator != null) ? VelocityCalculator.CalculateThrowVelocity(interactable.transform) : new ReleaseVelocityInformation(Vector3.zero, Vector3.zero, Vector3.zero));
		interactable.ApplyVelocities(releaseVelocityInformation.LinearVelocity, releaseVelocityInformation.AngularVelocity);
	}

	protected override void HandlePointerEventRaised(PointerEvent evt)
	{
		base.HandlePointerEventRaised(evt);
		if (!(base.SelectedInteractable == null) && base.SelectedInteractable.ResetGrabOnGrabsUpdated && evt.Identifier != base.Identifier && (evt.Type == PointerEventType.Select || evt.Type == PointerEventType.Unselect))
		{
			WristToGrabPoseOffset = this.GetGrabOffset();
			SetTarget(base.SelectedInteractable, _currentGrabType);
			Movement = this.GenerateMovement(base.SelectedInteractable);
			Pose targetGrabPose = this.GetTargetGrabPose();
			PointerEvent evt2 = new PointerEvent(base.Identifier, PointerEventType.Move, targetGrabPose, base.Data);
			base.SelectedInteractable.PointableElement.ProcessPointerEvent(evt2);
		}
	}

	protected override Pose ComputePointerPose()
	{
		if (Movement != null)
		{
			return Movement.Pose;
		}
		return this.GetHandGrabPose();
	}

	protected override bool ComputeShouldSelect()
	{
		return _handGrabShouldSelect;
	}

	protected override bool ComputeShouldUnselect()
	{
		return _handGrabShouldUnselect;
	}

	public override bool CanSelect(DistanceHandGrabInteractable interactable)
	{
		if (!base.CanSelect(interactable))
		{
			return false;
		}
		return this.CanInteractWith(interactable);
	}

	protected override DistanceHandGrabInteractable ComputeCandidate()
	{
		Vector3 bestHitPoint;
		DistanceHandGrabInteractable distanceHandGrabInteractable = _distantCandidateComputer.ComputeCandidate(Interactable<DistanceHandGrabInteractor, DistanceHandGrabInteractable>.Registry, this, out bestHitPoint);
		HitPoint = bestHitPoint;
		if (distanceHandGrabInteractable == null)
		{
			return null;
		}
		GrabTypeFlags grabTypes = SelectingGrabTypes(distanceHandGrabInteractable);
		if (this.GetPoseScore(distanceHandGrabInteractable, grabTypes, ref _cachedResult).IsValid())
		{
			return distanceHandGrabInteractable;
		}
		return null;
	}

	private GrabTypeFlags SelectingGrabTypes(IHandGrabInteractable interactable)
	{
		GrabTypeFlags handGrabTypes;
		if (base.State == InteractorState.Select || (handGrabTypes = this.ComputeShouldSelect(interactable)) == GrabTypeFlags.None)
		{
			HandGrabInteraction.ComputeHandGrabScore(this, interactable, out handGrabTypes);
		}
		if (handGrabTypes == GrabTypeFlags.None)
		{
			handGrabTypes = interactable.SupportedGrabTypes & SupportedGrabTypes;
		}
		return handGrabTypes;
	}

	private void UpdateTarget(IHandGrabInteractable interactable)
	{
		WristToGrabPoseOffset = this.GetGrabOffset();
		GrabTypeFlags selectingGrabTypes = SelectingGrabTypes(interactable);
		SetTarget(interactable, selectingGrabTypes);
		GrabTypeFlags handGrabTypes;
		float grabStrength = HandGrabInteraction.ComputeHandGrabScore(this, interactable, out handGrabTypes);
		SetGrabStrength(grabStrength);
	}

	private void UpdateTargetSliding(IHandGrabInteractable interactable)
	{
		if (!(interactable.Slippiness <= 0f) && HandGrabInteraction.ComputeHandGrabScore(this, interactable, out var handGrabTypes, includeSelecting: true) <= interactable.Slippiness)
		{
			SetTarget(interactable, handGrabTypes);
		}
	}

	private void SetTarget(IHandGrabInteractable interactable, GrabTypeFlags selectingGrabTypes)
	{
		this.CalculateBestGrab(interactable, selectingGrabTypes, out var activeGrabFlags, ref _cachedResult);
		HandGrabTarget.Set(interactable.RelativeTo, interactable.HandAlignment, activeGrabFlags, _cachedResult);
	}

	private void SetGrabStrength(float strength)
	{
		FingersStrength = strength;
		WristStrength = strength;
	}

	public void InjectAllDistanceHandGrabInteractor(HandGrabAPI handGrabApi, DistantCandidateComputer<DistanceHandGrabInteractor, DistanceHandGrabInteractable> distantCandidateComputer, Transform grabOrigin, IHand hand, GrabTypeFlags supportedGrabTypes)
	{
		InjectHandGrabApi(handGrabApi);
		InjectDistantCandidateComputer(distantCandidateComputer);
		InjectGrabOrigin(grabOrigin);
		InjectHand(hand);
		InjectSupportedGrabTypes(supportedGrabTypes);
	}

	public void InjectHandGrabApi(HandGrabAPI handGrabApi)
	{
		_handGrabApi = handGrabApi;
	}

	public void InjectDistantCandidateComputer(DistantCandidateComputer<DistanceHandGrabInteractor, DistanceHandGrabInteractable> distantCandidateComputer)
	{
		_distantCandidateComputer = distantCandidateComputer;
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectSupportedGrabTypes(GrabTypeFlags supportedGrabTypes)
	{
		_supportedGrabTypes = supportedGrabTypes;
	}

	public void InjectGrabOrigin(Transform grabOrigin)
	{
		_grabOrigin = grabOrigin;
	}

	public void InjectOptionalGripPoint(Transform gripPoint)
	{
		_gripPoint = gripPoint;
	}

	public void InjectOptionalPinchPoint(Transform pinchPoint)
	{
		_pinchPoint = pinchPoint;
	}

	[Obsolete("Use Grabbable instead")]
	public void InjectOptionalVelocityCalculator(IThrowVelocityCalculator velocityCalculator)
	{
		_velocityCalculator = velocityCalculator as UnityEngine.Object;
		VelocityCalculator = velocityCalculator;
	}
}
