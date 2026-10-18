using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class HandGrabStateVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHandGrabState), new Type[] { })]
	private UnityEngine.Object _handGrabState;

	private IHandGrabState HandGrabState;

	[SerializeField]
	private SyntheticHand _syntheticHand;

	private bool _areFingersFree = true;

	private bool _isWristFree = true;

	private bool _wasCompletelyFree = true;

	protected bool _started;

	protected virtual void Awake()
	{
		HandGrabState = _handGrabState as IHandGrabState;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	private void LateUpdate()
	{
		ConstrainingForce(HandGrabState, out var fingersConstraint, out var wristConstraint);
		UpdateHandPose(HandGrabState, fingersConstraint, wristConstraint);
		bool flag = _areFingersFree && _isWristFree;
		if (!flag || (flag && !_wasCompletelyFree))
		{
			_syntheticHand.MarkInputDataRequiresUpdate();
		}
		_wasCompletelyFree = flag;
	}

	private void ConstrainingForce(IHandGrabState grabSource, out float fingersConstraint, out float wristConstraint)
	{
		HandGrabTarget handGrabTarget = grabSource.HandGrabTarget;
		fingersConstraint = (wristConstraint = 0f);
		if (handGrabTarget != null)
		{
			if (grabSource.IsGrabbing && handGrabTarget.HandAlignment != 0)
			{
				fingersConstraint = grabSource.FingersStrength;
				wristConstraint = grabSource.WristStrength;
			}
			else if (handGrabTarget.HandAlignment == HandAlignType.AttractOnHover)
			{
				fingersConstraint = grabSource.FingersStrength;
				wristConstraint = grabSource.WristStrength;
			}
			else if (handGrabTarget.HandAlignment == HandAlignType.AlignFingersOnHover)
			{
				fingersConstraint = grabSource.FingersStrength;
			}
		}
	}

	private void UpdateHandPose(IHandGrabState grabSource, float fingersConstraint, float wristConstraint)
	{
		HandGrabTarget handGrabTarget = grabSource.HandGrabTarget;
		if (handGrabTarget == null)
		{
			FreeFingers();
			FreeWrist();
			return;
		}
		if (fingersConstraint > 0f && handGrabTarget.HandPose != null)
		{
			UpdateFingers(handGrabTarget.HandPose, grabSource.GrabbingFingers(), fingersConstraint);
			_areFingersFree = false;
		}
		else
		{
			FreeFingers();
		}
		if (wristConstraint > 0f)
		{
			Pose visualWristPose = grabSource.GetVisualWristPose();
			_syntheticHand.LockWristPose(visualWristPose, wristConstraint, SyntheticHand.WristLockMode.Full, worldPose: true);
			_isWristFree = false;
		}
		else
		{
			FreeWrist();
		}
	}

	private void UpdateFingers(HandPose handPose, HandFingerFlags grabbingFingers, float strength)
	{
		Quaternion[] jointRotations = handPose.JointRotations;
		_syntheticHand.OverrideAllJoints(in jointRotations, strength);
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

	public void InjectAllHandGrabInteractorVisual(IHandGrabState handGrabState, SyntheticHand syntheticHand)
	{
		InjectHandGrabState(handGrabState);
		InjectSyntheticHand(syntheticHand);
	}

	public void InjectHandGrabState(IHandGrabState handGrabState)
	{
		HandGrabState = handGrabState;
		_handGrabState = handGrabState as UnityEngine.Object;
	}

	public void InjectSyntheticHand(SyntheticHand syntheticHand)
	{
		_syntheticHand = syntheticHand;
	}
}
