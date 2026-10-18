using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class HandJointUtils
{
	public static List<HandJointId[]> FingerToJointList = new List<HandJointId[]>
	{
		new HandJointId[4]
		{
			HandJointId.HandThumb1,
			HandJointId.HandThumb2,
			HandJointId.HandThumb3,
			HandJointId.HandThumbTip
		},
		new HandJointId[5]
		{
			HandJointId.HandIndex0,
			HandJointId.HandIndex1,
			HandJointId.HandIndex2,
			HandJointId.HandIndex3,
			HandJointId.HandIndexTip
		},
		new HandJointId[5]
		{
			HandJointId.HandMiddle0,
			HandJointId.HandMiddle1,
			HandJointId.HandMiddle2,
			HandJointId.HandMiddle3,
			HandJointId.HandMiddleTip
		},
		new HandJointId[5]
		{
			HandJointId.HandRing0,
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

	public static HandFinger[] JointToFingerList = new HandFinger[26]
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
		HandFinger.Index,
		HandFinger.Index,
		HandFinger.Middle,
		HandFinger.Middle,
		HandFinger.Middle,
		HandFinger.Middle,
		HandFinger.Middle,
		HandFinger.Ring,
		HandFinger.Ring,
		HandFinger.Ring,
		HandFinger.Ring,
		HandFinger.Ring,
		HandFinger.Pinky,
		HandFinger.Pinky,
		HandFinger.Pinky,
		HandFinger.Pinky,
		HandFinger.Pinky
	};

	public static HandJointId[] JointParentList = new HandJointId[26]
	{
		HandJointId.HandWristRoot,
		HandJointId.Invalid,
		HandJointId.HandWristRoot,
		HandJointId.HandThumb1,
		HandJointId.HandThumb2,
		HandJointId.HandThumb3,
		HandJointId.HandWristRoot,
		HandJointId.HandIndex0,
		HandJointId.HandIndex1,
		HandJointId.HandIndex2,
		HandJointId.HandIndex3,
		HandJointId.HandWristRoot,
		HandJointId.HandMiddle0,
		HandJointId.HandMiddle1,
		HandJointId.HandMiddle2,
		HandJointId.HandMiddle3,
		HandJointId.HandWristRoot,
		HandJointId.HandRing0,
		HandJointId.HandRing1,
		HandJointId.HandRing2,
		HandJointId.HandRing3,
		HandJointId.HandWristRoot,
		HandJointId.HandPinky0,
		HandJointId.HandPinky1,
		HandJointId.HandPinky2,
		HandJointId.HandPinky3
	};

	public static HandJointId[][] JointChildrenList = new HandJointId[26][]
	{
		new HandJointId[0],
		new HandJointId[6]
		{
			HandJointId.HandStart,
			HandJointId.HandThumb1,
			HandJointId.HandIndex0,
			HandJointId.HandMiddle0,
			HandJointId.HandRing0,
			HandJointId.HandPinky0
		},
		new HandJointId[1] { HandJointId.HandThumb2 },
		new HandJointId[1] { HandJointId.HandThumb3 },
		new HandJointId[1] { HandJointId.HandThumbTip },
		new HandJointId[0],
		new HandJointId[1] { HandJointId.HandIndex1 },
		new HandJointId[1] { HandJointId.HandIndex2 },
		new HandJointId[1] { HandJointId.HandIndex3 },
		new HandJointId[1] { HandJointId.HandIndexTip },
		new HandJointId[0],
		new HandJointId[1] { HandJointId.HandMiddle1 },
		new HandJointId[1] { HandJointId.HandMiddle2 },
		new HandJointId[1] { HandJointId.HandMiddle3 },
		new HandJointId[1] { HandJointId.HandMiddleTip },
		new HandJointId[0],
		new HandJointId[1] { HandJointId.HandRing1 },
		new HandJointId[1] { HandJointId.HandRing2 },
		new HandJointId[1] { HandJointId.HandRing3 },
		new HandJointId[1] { HandJointId.HandRingTip },
		new HandJointId[0],
		new HandJointId[1] { HandJointId.HandPinky1 },
		new HandJointId[1] { HandJointId.HandPinky2 },
		new HandJointId[1] { HandJointId.HandPinky3 },
		new HandJointId[1] { HandJointId.HandPinkyTip },
		new HandJointId[0]
	};

	[Obsolete("Use JointToFingerListinstead.")]
	public static List<HandJointId> JointIds = new List<HandJointId>
	{
		HandJointId.HandIndex0,
		HandJointId.HandIndex1,
		HandJointId.HandIndex2,
		HandJointId.HandIndex3,
		HandJointId.HandMiddle0,
		HandJointId.HandMiddle1,
		HandJointId.HandMiddle2,
		HandJointId.HandMiddle3,
		HandJointId.HandRing0,
		HandJointId.HandRing1,
		HandJointId.HandRing2,
		HandJointId.HandRing3,
		HandJointId.HandPinky0,
		HandJointId.HandPinky1,
		HandJointId.HandPinky2,
		HandJointId.HandPinky3,
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
		return finger switch
		{
			HandFinger.Thumb => HandJointId.HandThumbTip, 
			HandFinger.Index => HandJointId.HandIndexTip, 
			HandFinger.Middle => HandJointId.HandMiddleTip, 
			HandFinger.Ring => HandJointId.HandRingTip, 
			HandFinger.Pinky => HandJointId.HandPinkyTip, 
			_ => HandJointId.Invalid, 
		};
	}

	public static bool IsFingerTip(HandJointId joint)
	{
		if (joint != HandJointId.HandThumbTip && joint != HandJointId.HandIndexTip && joint != HandJointId.HandMiddleTip && joint != HandJointId.HandRingTip)
		{
			return joint == HandJointId.HandPinkyTip;
		}
		return true;
	}

	public static HandJointId GetHandFingerProximal(HandFinger finger)
	{
		return _handFingerProximals[(int)finger];
	}

	public static bool WristJointPosesToLocalRotations(Pose[] jointPoses, ref Quaternion[] joints)
	{
		if (jointPoses.Length < 26 || joints.Length < 26)
		{
			return false;
		}
		for (int i = 0; i < 26; i++)
		{
			int num = (int)JointParentList[i];
			joints[i] = ((num < 0) ? Quaternion.identity : (Quaternion.Inverse(jointPoses[num].rotation) * jointPoses[i].rotation));
		}
		return true;
	}
}
