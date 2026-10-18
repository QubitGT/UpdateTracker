using System;
using Oculus.Interaction.Grab;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class HandGrabTarget
{
	[Obsolete]
	public enum GrabAnchor
	{
		None,
		Wrist,
		Pinch,
		Palm
	}

	private Transform _relativeTo;

	private HandGrabResult _handGrabResult = new HandGrabResult();

	public HandPose HandPose
	{
		get
		{
			if (!_handGrabResult.HasHandPose)
			{
				return null;
			}
			return _handGrabResult.HandPose;
		}
	}

	public HandAlignType HandAlignment { get; private set; }

	public GrabTypeFlags Anchor { get; private set; }

	public Pose GetWorldPoseDisplaced(in Pose offset)
	{
		Pose offset2 = PoseUtils.Multiply(in _handGrabResult.RelativePose, in offset);
		return _relativeTo.GlobalPose(in offset2);
	}

	[Obsolete("Use Set with GrabTypeFlags instead")]
	public void Set(Transform relativeTo, HandAlignType handAlignment, GrabAnchor anchor, HandGrabResult handGrabResult)
	{
		HandAlignment = handAlignment;
		_relativeTo = relativeTo;
		_handGrabResult.CopyFrom(handGrabResult);
		switch (anchor)
		{
		case GrabAnchor.Pinch:
			Anchor = GrabTypeFlags.Pinch;
			break;
		case GrabAnchor.Palm:
			Anchor = GrabTypeFlags.Palm;
			break;
		default:
			Anchor = GrabTypeFlags.None;
			break;
		}
	}

	public void Set(Transform relativeTo, HandAlignType handAlignment, GrabTypeFlags anchor, HandGrabResult handGrabResult)
	{
		Anchor = anchor;
		HandAlignment = handAlignment;
		_relativeTo = relativeTo;
		_handGrabResult.CopyFrom(handGrabResult);
	}
}
