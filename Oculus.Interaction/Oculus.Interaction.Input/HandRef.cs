using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class HandRef : MonoBehaviour, IHand, IActiveState
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	public IHand Hand { get; private set; }

	public Handedness Handedness => Hand.Handedness;

	public bool IsConnected => Hand.IsConnected;

	public bool IsHighConfidence => Hand.IsHighConfidence;

	public bool IsDominantHand => Hand.IsDominantHand;

	public float Scale => Hand.Scale;

	public bool IsPointerPoseValid => Hand.IsPointerPoseValid;

	public bool IsTrackedDataValid => Hand.IsTrackedDataValid;

	public int CurrentDataVersion => Hand.CurrentDataVersion;

	public bool Active => IsConnected;

	public event Action WhenHandUpdated
	{
		add
		{
			Hand.WhenHandUpdated += value;
		}
		remove
		{
			Hand.WhenHandUpdated -= value;
		}
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
	}

	public bool GetFingerIsPinching(HandFinger finger)
	{
		return Hand.GetFingerIsPinching(finger);
	}

	public bool GetIndexFingerIsPinching()
	{
		return Hand.GetIndexFingerIsPinching();
	}

	public bool GetPointerPose(out Pose pose)
	{
		return Hand.GetPointerPose(out pose);
	}

	public bool GetJointPose(HandJointId handJointId, out Pose pose)
	{
		return Hand.GetJointPose(handJointId, out pose);
	}

	public bool GetJointPoseLocal(HandJointId handJointId, out Pose pose)
	{
		return Hand.GetJointPoseLocal(handJointId, out pose);
	}

	public bool GetJointPosesLocal(out ReadOnlyHandJointPoses jointPosesLocal)
	{
		return Hand.GetJointPosesLocal(out jointPosesLocal);
	}

	public bool GetJointPoseFromWrist(HandJointId handJointId, out Pose pose)
	{
		return Hand.GetJointPoseFromWrist(handJointId, out pose);
	}

	public bool GetJointPosesFromWrist(out ReadOnlyHandJointPoses jointPosesFromWrist)
	{
		return Hand.GetJointPosesFromWrist(out jointPosesFromWrist);
	}

	public bool GetPalmPoseLocal(out Pose pose)
	{
		return Hand.GetPalmPoseLocal(out pose);
	}

	public bool GetFingerIsHighConfidence(HandFinger finger)
	{
		return Hand.GetFingerIsHighConfidence(finger);
	}

	public float GetFingerPinchStrength(HandFinger finger)
	{
		return Hand.GetFingerPinchStrength(finger);
	}

	public bool GetRootPose(out Pose pose)
	{
		return Hand.GetRootPose(out pose);
	}

	public void InjectAllHandRef(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
