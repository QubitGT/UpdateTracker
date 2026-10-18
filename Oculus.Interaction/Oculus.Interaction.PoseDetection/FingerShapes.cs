using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class FingerShapes
{
	private static readonly HandJointId[][] CURL_LINE_JOINTS = new HandJointId[5][]
	{
		new HandJointId[3]
		{
			HandJointId.HandThumb2,
			HandJointId.HandThumb3,
			HandJointId.HandThumbTip
		},
		new HandJointId[3]
		{
			HandJointId.HandIndex2,
			HandJointId.HandIndex3,
			HandJointId.HandIndexTip
		},
		new HandJointId[3]
		{
			HandJointId.HandMiddle2,
			HandJointId.HandMiddle3,
			HandJointId.HandMiddleTip
		},
		new HandJointId[3]
		{
			HandJointId.HandRing2,
			HandJointId.HandRing3,
			HandJointId.HandRingTip
		},
		new HandJointId[3]
		{
			HandJointId.HandPinky2,
			HandJointId.HandPinky3,
			HandJointId.HandPinkyTip
		}
	};

	private static readonly HandJointId[][] FLEXION_LINE_JOINTS = new HandJointId[5][]
	{
		new HandJointId[3]
		{
			HandJointId.HandThumb1,
			HandJointId.HandThumb2,
			HandJointId.HandThumb3
		},
		new HandJointId[3]
		{
			HandJointId.HandIndex1,
			HandJointId.HandIndex2,
			HandJointId.HandIndex3
		},
		new HandJointId[3]
		{
			HandJointId.HandMiddle1,
			HandJointId.HandMiddle2,
			HandJointId.HandMiddle3
		},
		new HandJointId[3]
		{
			HandJointId.HandRing1,
			HandJointId.HandRing2,
			HandJointId.HandRing3
		},
		new HandJointId[3]
		{
			HandJointId.HandPinky1,
			HandJointId.HandPinky2,
			HandJointId.HandPinky3
		}
	};

	private static readonly HandJointId[][] ABDUCTION_LINE_JOINTS = new HandJointId[5][]
	{
		new HandJointId[4]
		{
			HandJointId.HandThumbTip,
			HandJointId.HandThumb1,
			HandJointId.HandIndex1,
			HandJointId.HandIndexTip
		},
		new HandJointId[4]
		{
			HandJointId.HandIndexTip,
			HandJointId.HandIndex1,
			HandJointId.HandMiddle1,
			HandJointId.HandMiddleTip
		},
		new HandJointId[4]
		{
			HandJointId.HandMiddleTip,
			HandJointId.HandMiddle1,
			HandJointId.HandRing1,
			HandJointId.HandRingTip
		},
		new HandJointId[4]
		{
			HandJointId.HandRingTip,
			HandJointId.HandRing1,
			HandJointId.HandPinky1,
			HandJointId.HandPinkyTip
		},
		Array.Empty<HandJointId>()
	};

	private static readonly HandJointId[][] OPPOSITION_LINE_JOINTS = new HandJointId[5][]
	{
		Array.Empty<HandJointId>(),
		new HandJointId[2]
		{
			HandJointId.HandThumbTip,
			HandJointId.HandIndexTip
		},
		new HandJointId[2]
		{
			HandJointId.HandThumbTip,
			HandJointId.HandMiddleTip
		},
		new HandJointId[2]
		{
			HandJointId.HandThumbTip,
			HandJointId.HandRingTip
		},
		new HandJointId[2]
		{
			HandJointId.HandThumbTip,
			HandJointId.HandPinkyTip
		}
	};

	private static readonly HandJointId[][] CURL_ANGLE_JOINTS = new HandJointId[5][]
	{
		new HandJointId[4]
		{
			HandJointId.HandThumb1,
			HandJointId.HandThumb2,
			HandJointId.HandThumb3,
			HandJointId.HandThumbTip
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
		new HandJointId[4]
		{
			HandJointId.HandPinky1,
			HandJointId.HandPinky2,
			HandJointId.HandPinky3,
			HandJointId.HandPinkyTip
		}
	};

	public virtual float GetValue(HandFinger finger, FingerFeature feature, IHand hand)
	{
		return feature switch
		{
			FingerFeature.Curl => GetCurlValue(finger, hand), 
			FingerFeature.Flexion => GetFlexionValue(finger, hand), 
			FingerFeature.Abduction => GetAbductionValue(finger, hand), 
			FingerFeature.Opposition => GetOppositionValue(finger, hand), 
			_ => 0f, 
		};
	}

	private static float PosesCurlValue(Pose p0, Pose p1, Pose p2)
	{
		Vector3 from = p0.position - p1.position;
		Vector3 to = p2.position - p1.position;
		Vector3 axis = p1.rotation * Constants.LeftThumbSide;
		float num = Vector3.SignedAngle(from, to, axis);
		if (num < 0f)
		{
			num += 360f;
		}
		return num;
	}

	public static float PosesListCurlValue(Pose[] poses)
	{
		float num = 0f;
		for (int i = 0; i < poses.Length - 2; i++)
		{
			num += PosesCurlValue(poses[i], poses[i + 1], poses[i + 2]);
		}
		return num;
	}

	protected float JointsCurlValue(HandJointId[] joints, IHand hand)
	{
		if (!hand.GetJointPosesFromWrist(out var jointPosesFromWrist))
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < joints.Length - 2; i++)
		{
			num += PosesCurlValue(jointPosesFromWrist[(int)joints[i]], jointPosesFromWrist[(int)joints[i + 1]], jointPosesFromWrist[(int)joints[i + 2]]);
		}
		return num;
	}

	public float GetCurlValue(HandFinger finger, IHand hand)
	{
		HandJointId[] array = CURL_ANGLE_JOINTS[(int)finger];
		return JointsCurlValue(array, hand) / (float)(array.Length - 2);
	}

	public float GetFlexionValue(HandFinger finger, IHand hand)
	{
		if (!hand.GetJointPosesFromWrist(out var jointPosesFromWrist))
		{
			return 0f;
		}
		HandJointId handFingerProximal = HandJointUtils.GetHandFingerProximal(finger);
		Vector3 rightDorsal = Constants.RightDorsal;
		Vector3 to = Vector3.ProjectOnPlane(jointPosesFromWrist[handFingerProximal].rotation * Constants.RightDorsal, Constants.RightThumbSide);
		return 180f + Vector3.SignedAngle(rightDorsal, to, Constants.RightPinkySide);
	}

	public float GetAbductionValue(HandFinger finger, IHand hand)
	{
		if (finger == HandFinger.Pinky || !hand.GetJointPosesFromWrist(out var jointPosesFromWrist))
		{
			return 0f;
		}
		HandFinger finger2 = finger + 1;
		Vector3 position = jointPosesFromWrist[HandJointUtils.GetHandFingerProximal(finger)].position;
		Vector3 vector = Vector3.Lerp(position, jointPosesFromWrist[HandJointUtils.GetHandFingerProximal(finger2)].position, 0.5f);
		Vector3 vector2 = ((finger != 0) ? (jointPosesFromWrist[HandJointUtils.GetHandFingerTip(finger)].position - vector) : (jointPosesFromWrist[HandJointUtils.GetHandFingerTip(finger)].position - position));
		Vector3 vector3 = jointPosesFromWrist[HandJointUtils.GetHandFingerTip(finger2)].position - vector;
		Vector3 axis = Vector3.Cross(vector2, vector3);
		return Vector3.SignedAngle(vector2, vector3, axis);
	}

	public float GetOppositionValue(HandFinger finger, IHand hand)
	{
		if (finger == HandFinger.Thumb || !hand.GetJointPosesFromWrist(out var jointPosesFromWrist))
		{
			return 0f;
		}
		Vector3 position = jointPosesFromWrist[HandJointUtils.GetHandFingerTip(finger)].position;
		Vector3 position2 = jointPosesFromWrist[HandJointId.HandThumbTip].position;
		return Vector3.Magnitude(position - position2);
	}

	public virtual IReadOnlyList<HandJointId> GetJointsAffected(HandFinger finger, FingerFeature feature)
	{
		return feature switch
		{
			FingerFeature.Curl => CURL_LINE_JOINTS[(int)finger], 
			FingerFeature.Flexion => FLEXION_LINE_JOINTS[(int)finger], 
			FingerFeature.Abduction => ABDUCTION_LINE_JOINTS[(int)finger], 
			FingerFeature.Opposition => OPPOSITION_LINE_JOINTS[(int)finger], 
			_ => null, 
		};
	}
}
