using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandJointsPose : MonoBehaviour
{
	[Serializable]
	public struct WeightedJoint
	{
		public HandJointId handJointId;

		public float weight;
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	[InspectorName("Weighted Joints")]
	private List<WeightedJoint> _weightedJoints;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _localPositionOffset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotationOffset = Quaternion.identity;

	[SerializeField]
	[InspectorName("Weighted Joints")]
	private List<WeightedJoint> _joints;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _posOffset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotOffset = Quaternion.identity;

	[SerializeField]
	[Tooltip("When the attached hand's handedness is set to Left, this property will mirror the offsets. This allows for offset values to be set in Right hand coordinates for both Left and Right hands.")]
	private bool _mirrorOffsetsForLeftHand = true;

	private Pose _cachedPose = Pose.identity;

	protected bool _started;

	public IHand Hand { get; private set; }

	public bool MirrorOffsetsForLeftHand
	{
		get
		{
			return _mirrorOffsetsForLeftHand;
		}
		set
		{
			_mirrorOffsetsForLeftHand = value;
		}
	}

	public List<WeightedJoint> WeightedJoints
	{
		get
		{
			return _joints;
		}
		set
		{
			_joints = value;
		}
	}

	public Vector3 LocalPositionOffset
	{
		get
		{
			return _posOffset;
		}
		set
		{
			_posOffset = value;
		}
	}

	public Quaternion RotationOffset
	{
		get
		{
			return _rotOffset;
		}
		set
		{
			_rotOffset = value;
		}
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
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
			Hand.WhenHandUpdated += HandleHandUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandUpdated;
		}
	}

	private void HandleHandUpdated()
	{
		Pose pose = Pose.identity;
		float num = 0f;
		foreach (WeightedJoint weightedJoint in WeightedJoints)
		{
			if (!Hand.GetJointPose(weightedJoint.handJointId, out var pose2))
			{
				return;
			}
			float t = weightedJoint.weight / (num + weightedJoint.weight);
			num += weightedJoint.weight;
			pose.Lerp(in pose2, t);
		}
		GetOffset(ref _cachedPose, Hand.Handedness, Hand.Scale);
		_cachedPose.Postmultiply(in pose);
		base.transform.SetPose(in pose);
	}

	private void GetOffset(ref Pose pose, Handedness handedness, float scale)
	{
		if (_mirrorOffsetsForLeftHand && handedness == Handedness.Left)
		{
			Vector3 position = LocalPositionOffset * scale;
			pose.position = HandMirroring.Mirror(in position);
			Quaternion rotation = RotationOffset;
			pose.rotation = HandMirroring.Mirror(in rotation);
		}
		else
		{
			pose.position = LocalPositionOffset * scale;
			pose.rotation = RotationOffset;
		}
	}

	public void InjectAllHandJoint(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
