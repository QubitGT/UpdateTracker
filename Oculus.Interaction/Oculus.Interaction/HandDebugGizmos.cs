using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandDebugGizmos : SkeletonDebugGizmos, IHandVisual
{
	public enum CoordSpace
	{
		World,
		Local
	}

	[Tooltip("The IHand that will drive the visuals.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[Tooltip("The coordinate space in which to draw the skeleton. World space draws the skeleton at the world Body location. Local draws the skeleton relative to this transform's position, and can be placed, scaled, or mirrored as desired.")]
	[SerializeField]
	private CoordSpace _space;

	private bool _isVisible;

	protected bool _started;

	public IHand Hand { get; private set; }

	public CoordSpace Space
	{
		get
		{
			return _space;
		}
		set
		{
			_space = value;
		}
	}

	public bool ForceOffVisibility { get; set; }

	public bool IsVisible => _isVisible;

	public event Action WhenHandVisualUpdated = delegate
	{
	};

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

	public Pose GetJointPose(HandJointId jointId, Space space)
	{
		switch (space)
		{
		case UnityEngine.Space.Self:
		{
			if (Hand.GetJointPoseLocal(jointId, out var pose2))
			{
				return pose2;
			}
			break;
		}
		case UnityEngine.Space.World:
		{
			if (Hand.GetJointPose(jointId, out var pose))
			{
				return pose;
			}
			break;
		}
		}
		return default(Pose);
	}

	private void HandleHandUpdated()
	{
		_isVisible = Hand.IsTrackedDataValid && !ForceOffVisibility;
		if (_isVisible)
		{
			for (HandJointId handJointId = HandJointId.HandStart; handJointId < HandJointId.HandEnd; handJointId++)
			{
				Draw((int)handJointId, base.Visibility);
			}
		}
		this.WhenHandVisualUpdated();
	}

	protected override bool TryGetParentJointId(int jointId, out int parent)
	{
		if (jointId >= HandJointUtils.JointParentList.Length)
		{
			parent = -1;
			return false;
		}
		parent = (int)HandJointUtils.JointParentList[jointId];
		return parent > -1;
	}

	protected override bool TryGetJointPose(int jointId, out Pose pose)
	{
		CoordSpace space = _space;
		bool result;
		if (space == CoordSpace.World || space != CoordSpace.Local)
		{
			result = Hand.GetJointPose((HandJointId)jointId, out pose);
		}
		else
		{
			result = Hand.GetJointPoseFromWrist((HandJointId)jointId, out pose);
			pose.position = base.transform.TransformPoint(pose.position);
			pose.rotation = base.transform.rotation * pose.rotation;
		}
		return result;
	}

	public void InjectAllHandDebugGizmos(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
