using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class DominantHandRef : MonoBehaviour, IHand, IActiveState
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _leftHand;

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _rightHand;

	[SerializeField]
	[Tooltip("If true, the HandRef will point to the Dominant hand. If false it will point to the Non Dominant Hand")]
	private bool _selectDominant = true;

	private Action _whenHandUpdated = delegate
	{
	};

	protected bool _started;

	public IHand LeftHand { get; private set; }

	public IHand RightHand { get; private set; }

	public bool SelectDominant
	{
		get
		{
			return _selectDominant;
		}
		set
		{
			_selectDominant = value;
		}
	}

	public IHand Hand
	{
		get
		{
			if (LeftHand.IsDominantHand != _selectDominant)
			{
				return RightHand;
			}
			return LeftHand;
		}
	}

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
			_whenHandUpdated = (Action)Delegate.Combine(_whenHandUpdated, value);
		}
		remove
		{
			_whenHandUpdated = (Action)Delegate.Remove(_whenHandUpdated, value);
		}
	}

	protected virtual void Awake()
	{
		LeftHand = _leftHand as IHand;
		RightHand = _rightHand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			LeftHand.WhenHandUpdated += HandleLeftHandUpdated;
			RightHand.WhenHandUpdated += HandleRightHandUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			LeftHand.WhenHandUpdated -= HandleLeftHandUpdated;
			RightHand.WhenHandUpdated -= HandleRightHandUpdated;
		}
	}

	private void HandleLeftHandUpdated()
	{
		if (LeftHand.IsDominantHand == _selectDominant)
		{
			_whenHandUpdated();
		}
	}

	private void HandleRightHandUpdated()
	{
		if (RightHand.IsDominantHand == _selectDominant)
		{
			_whenHandUpdated();
		}
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

	public void InjectAllDominantHandRef(IHand leftHand, IHand rightHand)
	{
		InjectLeftHand(leftHand);
		InjectRightHand(rightHand);
	}

	public void InjectLeftHand(IHand leftHand)
	{
		_leftHand = leftHand as UnityEngine.Object;
		LeftHand = leftHand;
	}

	public void InjectRightHand(IHand rightHand)
	{
		_rightHand = rightHand as UnityEngine.Object;
		RightHand = rightHand;
	}
}
