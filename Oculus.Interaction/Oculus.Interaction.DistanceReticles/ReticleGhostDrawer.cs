using System;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction.DistanceReticles;

public class ReticleGhostDrawer : InteractorReticle<ReticleDataGhost>
{
	[Tooltip("The hand grab interactor to use for pose data.")]
	[FormerlySerializedAs("_handGrabber")]
	[SerializeField]
	[Interface(typeof(IHandGrabInteractor), new Type[] { typeof(IInteractorView) })]
	private UnityEngine.Object _handGrabInteractor;

	[Tooltip("Provides pose data for the ghost hand.")]
	[FormerlySerializedAs("_modifier")]
	[SerializeField]
	private SyntheticHand _syntheticHand;

	[Tooltip("Determines the visuals of the hand.")]
	[SerializeField]
	[Interface(typeof(IHandVisual), new Type[] { })]
	[FormerlySerializedAs("_visualHand")]
	private UnityEngine.Object _handVisual;

	private IHandVisual HandVisual;

	private bool _areFingersFree = true;

	private bool _isWristFree = true;

	private ITrackingToWorldTransformer Transformer;

	private IHandGrabInteractor HandGrabInteractor { get; set; }

	protected override IInteractorView Interactor { get; set; }

	protected override Component InteractableComponent => HandGrabInteractor.TargetInteractable as Component;

	protected virtual void Awake()
	{
		HandVisual = _handVisual as IHandVisual;
		HandGrabInteractor = _handGrabInteractor as IHandGrabInteractor;
		Interactor = _handGrabInteractor as IInteractorView;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		Transformer = _syntheticHand.GetData().Config.TrackingToWorldTransformer;
		Hide();
		this.EndStart(ref _started);
	}

	private void UpdateHandPose(IHandGrabState snapper)
	{
		HandGrabTarget handGrabTarget = snapper.HandGrabTarget;
		if (handGrabTarget == null)
		{
			FreeFingers();
			FreeWrist();
			return;
		}
		if (handGrabTarget.HandPose != null)
		{
			UpdateFingers(handGrabTarget.HandPose, snapper.GrabbingFingers());
			_areFingersFree = false;
		}
		else
		{
			FreeFingers();
		}
		Pose worldPose = snapper.GetVisualWristPose();
		Pose wristPose = ((Transformer != null) ? Transformer.ToTrackingPose(in worldPose) : worldPose);
		_syntheticHand.LockWristPose(wristPose);
		_isWristFree = false;
	}

	private void UpdateFingers(HandPose handPose, HandFingerFlags grabbingFingers)
	{
		Quaternion[] jointRotations = handPose.JointRotations;
		_syntheticHand.OverrideAllJoints(in jointRotations, 1f);
		for (int i = 0; i < 5; i++)
		{
			int num = 1 << i;
			JointFreedom freedomLevel = handPose.FingersFreedom[i];
			if (freedomLevel == JointFreedom.Constrained && ((uint)grabbingFingers & (uint)num) != 0)
			{
				freedomLevel = JointFreedom.Locked;
			}
			SyntheticHand syntheticHand = _syntheticHand;
			HandFinger finger = (HandFinger)i;
			syntheticHand.SetFingerFreedom(in finger, in freedomLevel);
		}
	}

	private bool FreeFingers()
	{
		if (!_areFingersFree)
		{
			_syntheticHand.FreeAllJoints();
			_areFingersFree = true;
			return true;
		}
		return false;
	}

	private bool FreeWrist()
	{
		if (!_isWristFree)
		{
			_syntheticHand.FreeWrist();
			_isWristFree = true;
			return true;
		}
		return false;
	}

	protected override void Align(ReticleDataGhost data)
	{
		UpdateHandPose(HandGrabInteractor);
		_syntheticHand.MarkInputDataRequiresUpdate();
	}

	protected override void Draw(ReticleDataGhost data)
	{
		HandVisual.ForceOffVisibility = false;
	}

	protected override void Hide()
	{
		HandVisual.ForceOffVisibility = true;
		_syntheticHand.MarkInputDataRequiresUpdate();
	}

	public void InjectAllReticleGhostDrawer(IHandGrabInteractor handGrabInteractor, SyntheticHand syntheticHand, IHandVisual visualHand)
	{
		InjectHandGrabInteractor(handGrabInteractor);
		InjectSyntheticHand(syntheticHand);
		InjectVisualHand(visualHand);
	}

	public void InjectHandGrabInteractor(IHandGrabInteractor handGrabInteractor)
	{
		_handGrabInteractor = handGrabInteractor as UnityEngine.Object;
		HandGrabInteractor = handGrabInteractor;
		Interactor = handGrabInteractor as IInteractorView;
	}

	public void InjectSyntheticHand(SyntheticHand syntheticHand)
	{
		_syntheticHand = syntheticHand;
	}

	public void InjectVisualHand(IHandVisual visualHand)
	{
		_handVisual = visualHand as UnityEngine.Object;
		HandVisual = visualHand;
	}
}
