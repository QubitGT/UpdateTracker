using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using Oculus.Interaction.Throw;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class HandGrabInteractor : PointerInteractor<HandGrabInteractor, HandGrabInteractable>, IHandGrabInteractor, IHandGrabState, IRigidbodyRef
{
	[Tooltip("The IHand that should be able to grab.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[Tooltip("The hand's Rigidbody, which detects interactables.")]
	[SerializeField]
	private Rigidbody _rigidbody;

	[Tooltip("Detects when the hand grab selects or unselects.")]
	[SerializeField]
	private HandGrabAPI _handGrabApi;

	[Tooltip("The grab types that the hand supports.")]
	[SerializeField]
	private GrabTypeFlags _supportedGrabTypes = GrabTypeFlags.All;

	[SerializeField]
	[Tooltip("When enabled, nearby interactables can become candidates even if thefinger strength is 0")]
	private bool _hoverOnZeroStrength;

	[Tooltip("The origin of the grab.")]
	[SerializeField]
	private Transform _grabOrigin;

	[Tooltip("Specifies an offset from the wrist that can be used to search for the best HandGrabInteractable available, act as a palm grab without a HandPose, and also act as an anchor for attaching the object.")]
	[SerializeField]
	[Optional]
	private Transform _gripPoint;

	[Tooltip("Collider used to detect a palm grab.")]
	[SerializeField]
	[Optional]
	private Collider _gripCollider;

	[Tooltip("Specifies a moving point at the center of the tips of the currently pinching fingers. It's used to align interactables that don’t have a HandPose to the center of the pinch.")]
	[SerializeField]
	[Optional]
	private Transform _pinchPoint;

	[Tooltip("Collider used to detect a pinch grab.")]
	[SerializeField]
	[Optional]
	private Collider _pinchCollider;

	[Tooltip("Determines how the object will move when thrown.")]
	[SerializeField]
	[Interface(typeof(IThrowVelocityCalculator), new Type[] { })]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	[Obsolete("Use Grabbable instead")]
	private UnityEngine.Object _velocityCalculator;

	private bool _handGrabShouldSelect;

	private bool _handGrabShouldUnselect;

	private HandGrabResult _cachedResult = new HandGrabResult();

	private HandGrabInteractable _selectedInteractableOverride;

	private GrabTypeFlags _currentGrabType;

	public IHand Hand { get; private set; }

	public bool HoverOnZeroStrength
	{
		get
		{
			return _hoverOnZeroStrength;
		}
		set
		{
			_hoverOnZeroStrength = value;
		}
	}

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

	public Rigidbody Rigidbody => _rigidbody;

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
		_nativeId = 5208257256663643250uL;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		Collider[] componentsInChildren = Rigidbody.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_ = componentsInChildren[i];
		}
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

	protected override void InteractableSet(HandGrabInteractable interactable)
	{
		base.InteractableSet(interactable);
		UpdateTarget(base.Interactable);
	}

	protected override void InteractableUnset(HandGrabInteractable interactable)
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

	protected override void InteractableSelected(HandGrabInteractable interactable)
	{
		if (interactable != null)
		{
			WristToGrabPoseOffset = this.GetGrabOffset();
			Movement = this.GenerateMovement(interactable);
			SetGrabStrength(1f);
		}
		base.InteractableSelected(interactable);
	}

	protected override void InteractableUnselected(HandGrabInteractable interactable)
	{
		base.InteractableUnselected(interactable);
		Movement = null;
		_currentGrabType = GrabTypeFlags.None;
		if (VelocityCalculator != null)
		{
			ReleaseVelocityInformation releaseVelocityInformation = VelocityCalculator.CalculateThrowVelocity(interactable.transform);
			interactable.ApplyVelocities(releaseVelocityInformation.LinearVelocity, releaseVelocityInformation.AngularVelocity);
		}
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
		if (!_handGrabShouldUnselect)
		{
			if (_selectedInteractableOverride != null)
			{
				return _selectedInteractableOverride != base.SelectedInteractable;
			}
			return false;
		}
		return true;
	}

	public override bool CanSelect(HandGrabInteractable interactable)
	{
		if (!base.CanSelect(interactable))
		{
			return false;
		}
		return this.CanInteractWith(interactable);
	}

	protected override HandGrabInteractable ComputeCandidate()
	{
		InteractableRegistry<HandGrabInteractor, HandGrabInteractable>.InteractableSet interactableSet = Interactable<HandGrabInteractor, HandGrabInteractable>.Registry.List(this);
		float num = float.NegativeInfinity;
		GrabPoseScore referenceScore = GrabPoseScore.Max;
		HandGrabInteractable result = null;
		foreach (HandGrabInteractable item in interactableSet)
		{
			float fingerScore;
			GrabTypeFlags grabTypeFlags = SelectingGrabTypes(item, num, out fingerScore);
			if (grabTypeFlags != 0)
			{
				GrabPoseScore poseScore = this.GetPoseScore(item, grabTypeFlags, ref _cachedResult);
				if (fingerScore > num || poseScore.IsBetterThan(referenceScore))
				{
					num = fingerScore;
					referenceScore = poseScore;
					result = item;
				}
			}
		}
		return result;
	}

	private GrabTypeFlags SelectingGrabTypes(HandGrabInteractable interactable, float minFingerScoreRequired, out float fingerScore)
	{
		fingerScore = 1f;
		GrabTypeFlags handGrabTypes;
		if (base.State == InteractorState.Select || (handGrabTypes = this.ComputeShouldSelect(interactable)) == GrabTypeFlags.None)
		{
			fingerScore = HandGrabInteraction.ComputeHandGrabScore(this, interactable, out handGrabTypes);
		}
		if (fingerScore < minFingerScoreRequired)
		{
			return GrabTypeFlags.None;
		}
		if (handGrabTypes == GrabTypeFlags.None)
		{
			if (!_hoverOnZeroStrength)
			{
				return GrabTypeFlags.None;
			}
			handGrabTypes = interactable.SupportedGrabTypes & SupportedGrabTypes;
		}
		if (_gripCollider != null && (handGrabTypes & GrabTypeFlags.Palm) != 0 && !OverlapsVolume(interactable, _gripCollider))
		{
			handGrabTypes &= ~GrabTypeFlags.Palm;
		}
		if (_pinchCollider != null && (handGrabTypes & GrabTypeFlags.Pinch) != 0 && !OverlapsVolume(interactable, _pinchCollider))
		{
			handGrabTypes &= ~GrabTypeFlags.Pinch;
		}
		return handGrabTypes;
	}

	public void ForceSelect(HandGrabInteractable interactable, bool allowManualRelease = false)
	{
		_selectedInteractableOverride = interactable;
		SetComputeCandidateOverride(() => interactable);
		SetComputeShouldSelectOverride(() => (object)interactable == base.Interactable);
		if (!allowManualRelease)
		{
			SetComputeShouldUnselectOverride(() => (object)interactable != base.SelectedInteractable, clearOverrideOnUnselect: false);
		}
	}

	public void ForceRelease()
	{
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

	public override void SetComputeCandidateOverride(Func<HandGrabInteractable> computeCandidate, bool shouldClearOverrideOnSelect = true)
	{
		base.SetComputeCandidateOverride(() => computeCandidate(), shouldClearOverrideOnSelect);
	}

	public override void Unselect()
	{
		if (base.State == InteractorState.Select && _selectedInteractableOverride != null && (base.SelectedInteractable == _selectedInteractableOverride || base.SelectedInteractable == null))
		{
			_selectedInteractableOverride = null;
			ClearComputeShouldUnselectOverride();
		}
		base.Unselect();
	}

	private bool OverlapsVolume(HandGrabInteractable interactable, Collider volume)
	{
		Collider[] colliders = interactable.Colliders;
		foreach (Collider collider in colliders)
		{
			if (collider.enabled && Physics.ComputePenetration(volume, volume.transform.position, volume.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var _, out var _))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateTarget(HandGrabInteractable interactable)
	{
		WristToGrabPoseOffset = this.GetGrabOffset();
		float fingerScore;
		GrabTypeFlags selectingGrabTypes = SelectingGrabTypes(interactable, float.NegativeInfinity, out fingerScore);
		SetTarget(interactable, selectingGrabTypes);
		SetGrabStrength(fingerScore);
	}

	private void UpdateTargetSliding(HandGrabInteractable interactable)
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

	public void InjectAllHandGrabInteractor(HandGrabAPI handGrabApi, Transform grabOrigin, IHand hand, Rigidbody rigidbody, GrabTypeFlags supportedGrabTypes)
	{
		InjectHandGrabApi(handGrabApi);
		InjectGrabOrigin(grabOrigin);
		InjectHand(hand);
		InjectRigidbody(rigidbody);
		InjectSupportedGrabTypes(supportedGrabTypes);
	}

	public void InjectHandGrabApi(HandGrabAPI handGrabAPI)
	{
		_handGrabApi = handGrabAPI;
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectRigidbody(Rigidbody rigidbody)
	{
		_rigidbody = rigidbody;
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

	public void InjectOptionalGripCollider(Collider gripCollider)
	{
		_gripCollider = gripCollider;
	}

	public void InjectOptionalPinchPoint(Transform pinchPoint)
	{
		_pinchPoint = pinchPoint;
	}

	public void InjectOptionalPinchCollider(Collider pinchCollider)
	{
		_pinchCollider = pinchCollider;
	}

	[Obsolete("Use Grabbable instead")]
	public void InjectOptionalVelocityCalculator(IThrowVelocityCalculator velocityCalculator)
	{
		_velocityCalculator = velocityCalculator as UnityEngine.Object;
		VelocityCalculator = velocityCalculator;
	}
}
