using System;
using System.Collections.Generic;

namespace Oculus.Interaction.Input.Compatibility.OVR;

public class HandJointUtils
{
	public static List<HandJointId[]> FingerToJointList = new List<HandJointId[]>
	{
		new HandJointId[5]
		{
			HandJointId.HandThumb0,
			HandJointId.HandThumb1,
			HandJointId.HandThumb2,
			HandJointId.HandThumb3,
			HandJointId.HandMaxSkinnable
		},
		new HandJointId[4]
		{
			HandJointId.HandIndex1,
			HandJointId.HandIndex2,
			HandJointId.HandIndex3,
			HandJointId.HandIndexTip
		},
		new HandJointId[4]
		{
			HandJointId.HandMiddle1,
			HandJointId.HandMiddle2,
			HandJointId.HandMiddle3,
			HandJointId.HandMiddleTip
		},
		new HandJointId[4]
		{
			HandJointId.HandRing1,
			HandJointId.HandRing2,
			HandJointId.HandRing3,
			HandJointId.HandRingTip
		},
		new HandJointId[5]
		{
			HandJointId.HandPinky0,
			HandJointId.HandPinky1,
			HandJointId.HandPinky2,
			HandJointId.HandPinky3,
			HandJointId.HandPinkyTip
		}
	};

	public static HandFinger[] JointToFingerList = new HandFinger[24]
	{
		HandFinger.Invalid,
		HandFinger.Invalid,
		HandFinger.Thumb,
		HandFinger.Thumb,
		HandFinger.Thumb,
		HandFinger.Thumb,
		HandFinger.Index,
		HandFinger.Index,
		HandFinger.Index,
		HandFinger.Middle,
		HandFinger.Middle,
		HandFinger.Middle,
		HandFinger.Ring,
		HandFinger.Ring,
		HandFinger.Ring,
		HandFinger.Pinky,
		HandFinger.Pinky,
		HandFinger.Pinky,
		HandFinger.Pinky,
		HandFinger.Thumb,
		HandFinger.Index,
		HandFinger.Middle,
		HandFinger.Ring,
		HandFinger.Pinky
	};

	public static HandJointId[] JointParentList = new HandJointId[24]
	{
		HandJointId.Invalid,
		HandJointId.HandStart,
		HandJointId.HandStart,
		HandJointId.HandThumb0,
		HandJointId.HandThumb1,
		HandJointId.HandThumb2,
		HandJointId.HandStart,
		HandJointId.HandIndex1,
		HandJointId.HandIndex2,
		HandJointId.HandStart,
		HandJointId.HandMiddle1,
		HandJointId.HandMiddle2,
		HandJointId.HandStart,
		HandJointId.HandRing1,
		HandJointId.HandRing2,
		HandJointId.HandStart,
		HandJointId.HandPinky0,
		HandJointId.HandPinky1,
		HandJointId.HandPinky2,
		HandJointId.HandThumb3,
		HandJointId.HandIndex3,
		HandJointId.HandMiddle3,
		HandJointId.HandRing3,
		HandJointId.HandPinky3
	};

	public static HandJointId[][] JointChildrenList = new HandJointId[24][]
	{
		new HandJointId[6]
		{
			HandJointId.HandForearmStub,
			HandJointId.HandThumb0,
			HandJointId.HandIndex1,
			HandJointId.HandMiddle1,
			HandJointId.HandRing1,
			HandJointId.HandPinky0
		},
		new HandJointId[0],
		new HandJointId[1] { HandJointId.HandThumb1 },
		new HandJointId[1] { HandJointId.HandThumb2 },
		new HandJointId[1] { HandJointId.HandThumb3 },
		new HandJointId[1] { HandJointId.HandMaxSkinnable },
		new HandJointId[1] { HandJointId.HandIndex2 },
		new HandJointId[1] { HandJointId.HandIndex3 },
		new HandJointId[1] { HandJointId.HandIndexTip },
		new HandJointId[1] { HandJointId.HandMiddle2 },
		new HandJointId[1] { HandJointId.HandMiddle3 },
		new HandJointId[1] { HandJointId.HandMiddleTip },
		new HandJointId[1] { HandJointId.HandRing2 },
		new HandJointId[1] { HandJointId.HandRing3 },
		new HandJointId[1] { HandJointId.HandRingTip },
		new HandJointId[1] { HandJointId.HandPinky1 },
		new HandJointId[1] { HandJointId.HandPinky2 },
		new HandJointId[1] { HandJointId.HandPinky3 },
		new HandJointId[1] { HandJointId.HandPinkyTip },
		new HandJointId[0],
		new HandJointId[0],
		new HandJointId[0],
		new HandJointId[0],
		new HandJointId[0]
	};

	[Obsolete("Use JointToFingerListinstead.")]
	public static List<HandJointId> JointIds = new List<HandJointId>
	{
		HandJointId.HandIndex1,
		HandJointId.HandIndex2,
		HandJointId.HandIndex3,
		HandJointId.HandMiddle1,
		HandJointId.HandMiddle2,
		HandJointId.HandMiddle3,
		HandJointId.HandRing1,
		HandJointId.HandRing2,
		HandJointId.HandRing3,
		HandJointId.HandPinky0,
		HandJointId.HandPinky1,
		HandJointId.HandPinky2,
		HandJointId.HandPinky3,
		HandJointId.HandThumb0,
		HandJointId.HandThumb1,
		HandJointId.HandThumb2,
		HandJointId.HandThumb3
	};

	private static readonly HandJointId[] _handFingerProximals = new HandJointId[5]
	{
		HandJointId.HandThumb2,
		HandJointId.HandIndex1,
		HandJointId.HandMiddle1,
		HandJointId.HandRing1,
		HandJointId.HandPinky1
	};

	public static HandJointId GetHandFingerTip(HandFinger finger)
	{
		return (HandJointId)(19 + finger);
	}

	public static bool IsFingerTip(HandJointId joint)
	{
		if (joint != HandJointId.HandMaxSkinnable && joint != HandJointId.HandIndexTip && joint != HandJointId.HandMiddleTip && joint != HandJointId.HandRingTip)
		{
			return joint == HandJointId.HandPinkyTip;
		}
		return true;
	}

	public static HandJointId GetHandFingerProximal(HandFinger finger)
	{
		return _handFingerProximals[(int)finger];
	}
}
