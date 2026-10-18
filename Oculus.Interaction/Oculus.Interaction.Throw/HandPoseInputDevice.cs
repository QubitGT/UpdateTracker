using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Throw;

public class HandPoseInputDevice : MonoBehaviour, IPoseInputDevice
{
	private class HandJointPoseMetaData
	{
		public readonly HandFinger Finger;

		public readonly HandJointId JointId;

		public readonly List<Vector3> Velocities;

		private Vector3? _previousPosition;

		private int _lastWritePos;

		private int _bufferLength;

		public HandJointPoseMetaData(HandFinger finger, HandJointId joint, int bufferLength)
		{
			Finger = finger;
			JointId = joint;
			Velocities = new List<Vector3>();
			_previousPosition = null;
			_lastWritePos = -1;
			_bufferLength = bufferLength;
		}

		public void BufferNewValue(Pose newPose, float delta)
		{
			Vector3 position = newPose.position;
			Vector3 vector = Vector3.zero;
			if (delta > Mathf.Epsilon && _previousPosition.HasValue)
			{
				vector = (position - _previousPosition.Value) / delta;
			}
			int num = ((_lastWritePos >= 0) ? ((_lastWritePos + 1) % _bufferLength) : 0);
			if (Velocities.Count <= num)
			{
				Velocities.Add(vector);
			}
			else
			{
				Velocities[num] = vector;
			}
			_previousPosition = position;
			_lastWritePos = num;
		}

		public Vector3 GetAverageVelocityVector()
		{
			int count = Velocities.Count;
			if (count == 0)
			{
				return Vector3.zero;
			}
			Vector3 zero = Vector3.zero;
			foreach (Vector3 velocity in Velocities)
			{
				zero += velocity;
			}
			return zero / count;
		}

		public void ResetSpeedsBuffer()
		{
			Velocities.Clear();
			_lastWritePos = -1;
			_previousPosition = null;
		}
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private float _bufferLengthSeconds = 0.1f;

	[SerializeField]
	private float _sampleFrequency = 90f;

	private int _bufferSize = -1;

	private HandJointPoseMetaData[] _jointPoseInfoArray;

	public IHand Hand { get; private set; }

	public float BufferLengthSeconds
	{
		get
		{
			return _bufferLengthSeconds;
		}
		set
		{
			_bufferLengthSeconds = value;
		}
	}

	public float SampleFrequency
	{
		get
		{
			return _sampleFrequency;
		}
		set
		{
			_sampleFrequency = value;
		}
	}

	public bool IsInputValid => Hand.IsTrackedDataValid;

	public bool IsHighConfidence => Hand.IsHighConfidence;

	public bool GetRootPose(out Pose pose)
	{
		pose = Pose.identity;
		if (!IsInputValid)
		{
			return false;
		}
		if (!Hand.GetJointPose(HandJointId.HandWristRoot, out pose))
		{
			return false;
		}
		if (!Hand.GetPalmPoseLocal(out var pose2))
		{
			return false;
		}
		pose2.Postmultiply(in pose);
		pose = pose2;
		return true;
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		_bufferSize = Mathf.CeilToInt(_bufferLengthSeconds * _sampleFrequency);
	}

	protected virtual void LateUpdate()
	{
		BufferFingerVelocities();
	}

	private void BufferFingerVelocities()
	{
		if (IsInputValid)
		{
			AllocateFingerBonesArrayIfNecessary();
			BufferFingerBoneVelocities();
		}
	}

	private void AllocateFingerBonesArrayIfNecessary()
	{
		if (_jointPoseInfoArray == null)
		{
			_jointPoseInfoArray = new HandJointPoseMetaData[5]
			{
				new HandJointPoseMetaData(HandFinger.Thumb, HandJointId.HandThumb3, _bufferSize),
				new HandJointPoseMetaData(HandFinger.Index, HandJointId.HandIndex3, _bufferSize),
				new HandJointPoseMetaData(HandFinger.Middle, HandJointId.HandMiddle3, _bufferSize),
				new HandJointPoseMetaData(HandFinger.Ring, HandJointId.HandRing3, _bufferSize),
				new HandJointPoseMetaData(HandFinger.Pinky, HandJointId.HandPinky3, _bufferSize)
			};
		}
	}

	private bool GetFingerIsHighConfidence(HandFinger handFinger)
	{
		if (Hand.IsTrackedDataValid)
		{
			return Hand.GetFingerIsHighConfidence(handFinger);
		}
		return false;
	}

	private bool GetJointPose(HandJointId handJointId, out Pose pose)
	{
		pose = Pose.identity;
		if (!Hand.IsTrackedDataValid)
		{
			return false;
		}
		if (!Hand.GetJointPose(handJointId, out pose))
		{
			return false;
		}
		return true;
	}

	private void BufferFingerBoneVelocities()
	{
		float deltaTime = Time.deltaTime;
		HandJointPoseMetaData[] jointPoseInfoArray = _jointPoseInfoArray;
		foreach (HandJointPoseMetaData handJointPoseMetaData in jointPoseInfoArray)
		{
			if (GetFingerIsHighConfidence(handJointPoseMetaData.Finger) && GetJointPose(handJointPoseMetaData.JointId, out var pose))
			{
				handJointPoseMetaData.BufferNewValue(pose, deltaTime);
			}
		}
	}

	public (Vector3, Vector3) GetExternalVelocities()
	{
		if (_jointPoseInfoArray == null || _jointPoseInfoArray.Length == 0)
		{
			return (Vector3.zero, Vector3.zero);
		}
		Vector3 zero = Vector3.zero;
		HandJointPoseMetaData[] jointPoseInfoArray = _jointPoseInfoArray;
		foreach (HandJointPoseMetaData handJointPoseMetaData in jointPoseInfoArray)
		{
			zero += handJointPoseMetaData.GetAverageVelocityVector();
		}
		zero /= (float)_jointPoseInfoArray.Length;
		jointPoseInfoArray = _jointPoseInfoArray;
		for (int i = 0; i < jointPoseInfoArray.Length; i++)
		{
			jointPoseInfoArray[i].ResetSpeedsBuffer();
		}
		return (zero, Vector3.zero);
	}

	public void InjectAllHandPoseInputDevice(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
