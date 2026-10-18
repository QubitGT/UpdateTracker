using Oculus.Interaction.Input;
using Oculus.Interaction.Input.Compatibility.OVR;
using UnityEngine;

namespace Oculus.Interaction;

public static class HandTranslationUtils
{
	public const string UpgradeRequiredMessage = "Some fields do not contain the expected values of converting to OpenXR from the previous serialized data. Convert the values?";

	public const string UpgradeRequiredButton = "Convert";

	private static readonly HandMirroring.HandSpace _openXRLeft = new HandMirroring.HandSpace(Oculus.Interaction.Input.Constants.LeftDistal, Oculus.Interaction.Input.Constants.LeftDorsal, Oculus.Interaction.Input.Constants.LeftThumbSide);

	private static readonly HandMirroring.HandSpace _openXRRight = new HandMirroring.HandSpace(Oculus.Interaction.Input.Constants.RightDistal, Oculus.Interaction.Input.Constants.RightDorsal, Oculus.Interaction.Input.Constants.RightThumbSide);

	private static readonly HandMirroring.HandSpace _ovrLeft = new HandMirroring.HandSpace(Oculus.Interaction.Input.Compatibility.OVR.Constants.LeftDistal, Oculus.Interaction.Input.Compatibility.OVR.Constants.LeftDorsal, Oculus.Interaction.Input.Compatibility.OVR.Constants.LeftThumbSide);

	private static readonly HandMirroring.HandSpace _ovrRight = new HandMirroring.HandSpace(Oculus.Interaction.Input.Compatibility.OVR.Constants.RightDistal, Oculus.Interaction.Input.Compatibility.OVR.Constants.RightDorsal, Oculus.Interaction.Input.Compatibility.OVR.Constants.RightThumbSide);

	public static readonly HandMirroring.HandsSpace openXRHands = new HandMirroring.HandsSpace(_openXRLeft, _openXRRight);

	public static readonly HandMirroring.HandsSpace ovrHands = new HandMirroring.HandsSpace(_ovrLeft, _ovrRight);

	public static int[] HAND_JOINT_IDS_OpenXRtoOVR = new int[19]
	{
		1, 2, 3, -1, 4, 5, 6, -1, 7, 8,
		9, -1, 10, 11, 12, 13, 14, 15, 16
	};

	public static GUIStyle FixButtonStyle => new GUIStyle(GUI.skin.button)
	{
		stretchWidth = true,
		stretchHeight = true,
		fixedWidth = 60f
	};

	public static int OpenXRHandJointToOVR(int openXRJointId)
	{
		return (Oculus.Interaction.Input.HandJointId)openXRJointId switch
		{
			Oculus.Interaction.Input.HandJointId.HandStart => -1, 
			Oculus.Interaction.Input.HandJointId.HandWristRoot => 0, 
			Oculus.Interaction.Input.HandJointId.HandThumb1 => 3, 
			Oculus.Interaction.Input.HandJointId.HandThumb2 => 4, 
			Oculus.Interaction.Input.HandJointId.HandThumb3 => 5, 
			Oculus.Interaction.Input.HandJointId.HandThumbTip => 19, 
			Oculus.Interaction.Input.HandJointId.HandIndex0 => -1, 
			Oculus.Interaction.Input.HandJointId.HandIndex1 => 6, 
			Oculus.Interaction.Input.HandJointId.HandIndex2 => 7, 
			Oculus.Interaction.Input.HandJointId.HandIndex3 => 8, 
			Oculus.Interaction.Input.HandJointId.HandIndexTip => 20, 
			Oculus.Interaction.Input.HandJointId.HandMiddle0 => -1, 
			Oculus.Interaction.Input.HandJointId.HandMiddle1 => 9, 
			Oculus.Interaction.Input.HandJointId.HandMiddle2 => 10, 
			Oculus.Interaction.Input.HandJointId.HandMiddle3 => 11, 
			Oculus.Interaction.Input.HandJointId.HandMiddleTip => 21, 
			Oculus.Interaction.Input.HandJointId.HandRing0 => -1, 
			Oculus.Interaction.Input.HandJointId.HandRing1 => 12, 
			Oculus.Interaction.Input.HandJointId.HandRing2 => 13, 
			Oculus.Interaction.Input.HandJointId.HandRing3 => 14, 
			Oculus.Interaction.Input.HandJointId.HandRingTip => 22, 
			Oculus.Interaction.Input.HandJointId.HandPinky0 => 15, 
			Oculus.Interaction.Input.HandJointId.HandPinky1 => 16, 
			Oculus.Interaction.Input.HandJointId.HandPinky2 => 17, 
			Oculus.Interaction.Input.HandJointId.HandPinky3 => 18, 
			Oculus.Interaction.Input.HandJointId.HandPinkyTip => 23, 
			_ => -1, 
		};
	}

	public static bool OVRHandRotationsToOpenXRPoses(Quaternion[] ovrJointRotations, Oculus.Interaction.Input.Handedness handedness, ref Pose[] targetPoses)
	{
		if (ovrJointRotations.Length < 24 || targetPoses.Length < 26)
		{
			return false;
		}
		Oculus.Interaction.Input.Compatibility.OVR.HandSkeleton handSkeleton = ((handedness == Oculus.Interaction.Input.Handedness.Left) ? Oculus.Interaction.Input.Compatibility.OVR.HandSkeleton.DefaultLeftSkeleton : Oculus.Interaction.Input.Compatibility.OVR.HandSkeleton.DefaultRightSkeleton);
		Oculus.Interaction.Input.HandSkeleton handSkeleton2 = ((handedness == Oculus.Interaction.Input.Handedness.Left) ? Oculus.Interaction.Input.HandSkeleton.DefaultLeftSkeleton : Oculus.Interaction.Input.HandSkeleton.DefaultRightSkeleton);
		targetPoses[1] = handSkeleton2.Joints[1].pose;
		for (int i = 0; i < 26; i++)
		{
			Pose b = handSkeleton2[i].pose;
			int num = (int)Oculus.Interaction.Input.HandJointUtils.JointParentList[i];
			if (num < 0)
			{
				continue;
			}
			Oculus.Interaction.Input.Compatibility.OVR.HandJointId handJointId = (Oculus.Interaction.Input.Compatibility.OVR.HandJointId)OpenXRHandJointToOVR(i);
			if (Oculus.Interaction.Input.HandJointUtils.JointToFingerList[i] == Oculus.Interaction.Input.HandFinger.Thumb)
			{
				Pose a = Pose.identity;
				for (int num2 = (int)handJointId; num2 >= 0; num2 = (int)Oculus.Interaction.Input.Compatibility.OVR.HandJointUtils.JointParentList[num2])
				{
					Pose b2 = handSkeleton.Joints[num2].pose;
					b2.rotation = ovrJointRotations[num2];
					a.Postmultiply(in b2);
				}
				a.rotation = a.rotation.normalized;
				Pose[] obj = targetPoses;
				int num3 = i;
				HandMirroring.HandSpace fromHand = ovrHands[handedness];
				HandMirroring.HandSpace toHand = openXRHands[handedness];
				obj[num3] = HandMirroring.TransformPose(in a, in fromHand, in toHand);
			}
			else
			{
				if (handJointId != Oculus.Interaction.Input.Compatibility.OVR.HandJointId.Invalid && handJointId < Oculus.Interaction.Input.Compatibility.OVR.HandJointId.HandMaxSkinnable)
				{
					ref Quaternion rotation = ref ovrJointRotations[(int)handJointId];
					HandMirroring.HandSpace fromHand = ovrHands[handedness];
					HandMirroring.HandSpace toHand = openXRHands[handedness];
					b.rotation = HandMirroring.TransformRotation(in rotation, in fromHand, in toHand);
				}
				PoseUtils.Multiply(in targetPoses[num], in b, ref targetPoses[i]);
			}
		}
		return true;
	}

	public static Oculus.Interaction.Input.HandJointId OVRHandJointToOpenXR(int ovrJointId)
	{
		return ovrJointId switch
		{
			0 => Oculus.Interaction.Input.HandJointId.HandWristRoot, 
			1 => Oculus.Interaction.Input.HandJointId.Invalid, 
			2 => Oculus.Interaction.Input.HandJointId.Invalid, 
			3 => Oculus.Interaction.Input.HandJointId.HandThumb1, 
			4 => Oculus.Interaction.Input.HandJointId.HandThumb2, 
			5 => Oculus.Interaction.Input.HandJointId.HandThumb3, 
			6 => Oculus.Interaction.Input.HandJointId.HandIndex1, 
			7 => Oculus.Interaction.Input.HandJointId.HandIndex2, 
			8 => Oculus.Interaction.Input.HandJointId.HandIndex3, 
			9 => Oculus.Interaction.Input.HandJointId.HandMiddle1, 
			10 => Oculus.Interaction.Input.HandJointId.HandMiddle2, 
			11 => Oculus.Interaction.Input.HandJointId.HandMiddle3, 
			12 => Oculus.Interaction.Input.HandJointId.HandRing1, 
			13 => Oculus.Interaction.Input.HandJointId.HandRing2, 
			14 => Oculus.Interaction.Input.HandJointId.HandRing3, 
			15 => Oculus.Interaction.Input.HandJointId.HandPinky0, 
			16 => Oculus.Interaction.Input.HandJointId.HandPinky1, 
			17 => Oculus.Interaction.Input.HandJointId.HandPinky2, 
			18 => Oculus.Interaction.Input.HandJointId.HandPinky3, 
			19 => Oculus.Interaction.Input.HandJointId.HandThumbTip, 
			20 => Oculus.Interaction.Input.HandJointId.HandIndexTip, 
			21 => Oculus.Interaction.Input.HandJointId.HandMiddleTip, 
			22 => Oculus.Interaction.Input.HandJointId.HandRingTip, 
			23 => Oculus.Interaction.Input.HandJointId.HandPinkyTip, 
			_ => Oculus.Interaction.Input.HandJointId.Invalid, 
		};
	}

	public static Vector3 TransformOVRToOpenXRPosition(Vector3 position, Oculus.Interaction.Input.Handedness handedness)
	{
		HandMirroring.HandSpace fromHand = ovrHands[handedness];
		HandMirroring.HandSpace toHand = openXRHands[handedness];
		return HandMirroring.TransformPosition(in position, in fromHand, in toHand);
	}

	public static Quaternion TransformOVRToOpenXRRotation(Quaternion rotation, Oculus.Interaction.Input.Handedness handedness)
	{
		HandMirroring.HandSpace fromHand = ovrHands[handedness];
		HandMirroring.HandSpace toHand = openXRHands[handedness];
		return HandMirroring.TransformRotation(in rotation, in fromHand, in toHand);
	}
}
