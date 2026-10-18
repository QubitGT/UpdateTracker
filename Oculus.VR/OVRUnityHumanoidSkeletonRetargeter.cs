using System;
using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

[Feature(Feature.BodyTracking)]
public class OVRUnityHumanoidSkeletonRetargeter : OVRSkeleton
{
	public class OVRHumanBodyBonesMappings : OVRHumanBodyBonesMappingsInterface
	{
		public enum BodySection
		{
			LeftLeg,
			LeftFoot,
			RightLeg,
			RightFoot,
			LeftArm,
			LeftHand,
			RightArm,
			RightHand,
			Hips,
			Back,
			Neck,
			Head
		}

		public enum FullBodyTrackingBoneId
		{
			FullBody_Start = 0,
			FullBody_Root = 0,
			FullBody_Hips = 1,
			FullBody_SpineLower = 2,
			FullBody_SpineMiddle = 3,
			FullBody_SpineUpper = 4,
			FullBody_Chest = 5,
			FullBody_Neck = 6,
			FullBody_Head = 7,
			FullBody_LeftShoulder = 8,
			FullBody_LeftScapula = 9,
			FullBody_LeftArmUpper = 10,
			FullBody_LeftArmLower = 11,
			FullBody_LeftHandWristTwist = 12,
			FullBody_RightShoulder = 13,
			FullBody_RightScapula = 14,
			FullBody_RightArmUpper = 15,
			FullBody_RightArmLower = 16,
			FullBody_RightHandWristTwist = 17,
			FullBody_LeftHandPalm = 18,
			FullBody_LeftHandWrist = 19,
			FullBody_LeftHandThumbMetacarpal = 20,
			FullBody_LeftHandThumbProximal = 21,
			FullBody_LeftHandThumbDistal = 22,
			FullBody_LeftHandThumbTip = 23,
			FullBody_LeftHandIndexMetacarpal = 24,
			FullBody_LeftHandIndexProximal = 25,
			FullBody_LeftHandIndexIntermediate = 26,
			FullBody_LeftHandIndexDistal = 27,
			FullBody_LeftHandIndexTip = 28,
			FullBody_LeftHandMiddleMetacarpal = 29,
			FullBody_LeftHandMiddleProximal = 30,
			FullBody_LeftHandMiddleIntermediate = 31,
			FullBody_LeftHandMiddleDistal = 32,
			FullBody_LeftHandMiddleTip = 33,
			FullBody_LeftHandRingMetacarpal = 34,
			FullBody_LeftHandRingProximal = 35,
			FullBody_LeftHandRingIntermediate = 36,
			FullBody_LeftHandRingDistal = 37,
			FullBody_LeftHandRingTip = 38,
			FullBody_LeftHandLittleMetacarpal = 39,
			FullBody_LeftHandLittleProximal = 40,
			FullBody_LeftHandLittleIntermediate = 41,
			FullBody_LeftHandLittleDistal = 42,
			FullBody_LeftHandLittleTip = 43,
			FullBody_RightHandPalm = 44,
			FullBody_RightHandWrist = 45,
			FullBody_RightHandThumbMetacarpal = 46,
			FullBody_RightHandThumbProximal = 47,
			FullBody_RightHandThumbDistal = 48,
			FullBody_RightHandThumbTip = 49,
			FullBody_RightHandIndexMetacarpal = 50,
			FullBody_RightHandIndexProximal = 51,
			FullBody_RightHandIndexIntermediate = 52,
			FullBody_RightHandIndexDistal = 53,
			FullBody_RightHandIndexTip = 54,
			FullBody_RightHandMiddleMetacarpal = 55,
			FullBody_RightHandMiddleProximal = 56,
			FullBody_RightHandMiddleIntermediate = 57,
			FullBody_RightHandMiddleDistal = 58,
			FullBody_RightHandMiddleTip = 59,
			FullBody_RightHandRingMetacarpal = 60,
			FullBody_RightHandRingProximal = 61,
			FullBody_RightHandRingIntermediate = 62,
			FullBody_RightHandRingDistal = 63,
			FullBody_RightHandRingTip = 64,
			FullBody_RightHandLittleMetacarpal = 65,
			FullBody_RightHandLittleProximal = 66,
			FullBody_RightHandLittleIntermediate = 67,
			FullBody_RightHandLittleDistal = 68,
			FullBody_RightHandLittleTip = 69,
			FullBody_LeftUpperLeg = 70,
			FullBody_LeftLowerLeg = 71,
			FullBody_LeftFootAnkleTwist = 72,
			FullBody_LeftFootAnkle = 73,
			FullBody_LeftFootSubtalar = 74,
			FullBody_LeftFootTransverse = 75,
			FullBody_LeftFootBall = 76,
			FullBody_RightUpperLeg = 77,
			FullBody_RightLowerLeg = 78,
			FullBody_RightFootAnkleTwist = 79,
			FullBody_RightFootAnkle = 80,
			FullBody_RightFootSubtalar = 81,
			FullBody_RightFootTransverse = 82,
			FullBody_RightFootBall = 83,
			FullBody_End = 84,
			NoOverride = 85,
			Remove = 86
		}

		public enum BodyTrackingBoneId
		{
			Body_Start = 0,
			Body_Root = 0,
			Body_Hips = 1,
			Body_SpineLower = 2,
			Body_SpineMiddle = 3,
			Body_SpineUpper = 4,
			Body_Chest = 5,
			Body_Neck = 6,
			Body_Head = 7,
			Body_LeftShoulder = 8,
			Body_LeftScapula = 9,
			Body_LeftArmUpper = 10,
			Body_LeftArmLower = 11,
			Body_LeftHandWristTwist = 12,
			Body_RightShoulder = 13,
			Body_RightScapula = 14,
			Body_RightArmUpper = 15,
			Body_RightArmLower = 16,
			Body_RightHandWristTwist = 17,
			Body_LeftHandPalm = 18,
			Body_LeftHandWrist = 19,
			Body_LeftHandThumbMetacarpal = 20,
			Body_LeftHandThumbProximal = 21,
			Body_LeftHandThumbDistal = 22,
			Body_LeftHandThumbTip = 23,
			Body_LeftHandIndexMetacarpal = 24,
			Body_LeftHandIndexProximal = 25,
			Body_LeftHandIndexIntermediate = 26,
			Body_LeftHandIndexDistal = 27,
			Body_LeftHandIndexTip = 28,
			Body_LeftHandMiddleMetacarpal = 29,
			Body_LeftHandMiddleProximal = 30,
			Body_LeftHandMiddleIntermediate = 31,
			Body_LeftHandMiddleDistal = 32,
			Body_LeftHandMiddleTip = 33,
			Body_LeftHandRingMetacarpal = 34,
			Body_LeftHandRingProximal = 35,
			Body_LeftHandRingIntermediate = 36,
			Body_LeftHandRingDistal = 37,
			Body_LeftHandRingTip = 38,
			Body_LeftHandLittleMetacarpal = 39,
			Body_LeftHandLittleProximal = 40,
			Body_LeftHandLittleIntermediate = 41,
			Body_LeftHandLittleDistal = 42,
			Body_LeftHandLittleTip = 43,
			Body_RightHandPalm = 44,
			Body_RightHandWrist = 45,
			Body_RightHandThumbMetacarpal = 46,
			Body_RightHandThumbProximal = 47,
			Body_RightHandThumbDistal = 48,
			Body_RightHandThumbTip = 49,
			Body_RightHandIndexMetacarpal = 50,
			Body_RightHandIndexProximal = 51,
			Body_RightHandIndexIntermediate = 52,
			Body_RightHandIndexDistal = 53,
			Body_RightHandIndexTip = 54,
			Body_RightHandMiddleMetacarpal = 55,
			Body_RightHandMiddleProximal = 56,
			Body_RightHandMiddleIntermediate = 57,
			Body_RightHandMiddleDistal = 58,
			Body_RightHandMiddleTip = 59,
			Body_RightHandRingMetacarpal = 60,
			Body_RightHandRingProximal = 61,
			Body_RightHandRingIntermediate = 62,
			Body_RightHandRingDistal = 63,
			Body_RightHandRingTip = 64,
			Body_RightHandLittleMetacarpal = 65,
			Body_RightHandLittleProximal = 66,
			Body_RightHandLittleIntermediate = 67,
			Body_RightHandLittleDistal = 68,
			Body_RightHandLittleTip = 69,
			Body_End = 70,
			NoOverride = 71,
			Remove = 72
		}

		public static readonly Dictionary<HumanBodyBones, Tuple<HumanBodyBones, HumanBodyBones>> BoneToJointPair = new Dictionary<HumanBodyBones, Tuple<HumanBodyBones, HumanBodyBones>>
		{
			{
				HumanBodyBones.Neck,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Neck, HumanBodyBones.Head)
			},
			{
				HumanBodyBones.Head,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Neck, HumanBodyBones.Head)
			},
			{
				HumanBodyBones.LeftEye,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Head, HumanBodyBones.LeftEye)
			},
			{
				HumanBodyBones.RightEye,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Head, HumanBodyBones.RightEye)
			},
			{
				HumanBodyBones.Jaw,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Head, HumanBodyBones.Jaw)
			},
			{
				HumanBodyBones.Hips,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Hips, HumanBodyBones.Spine)
			},
			{
				HumanBodyBones.Spine,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Spine, HumanBodyBones.Chest)
			},
			{
				HumanBodyBones.Chest,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.Chest, HumanBodyBones.UpperChest)
			},
			{
				HumanBodyBones.UpperChest,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.UpperChest, HumanBodyBones.Neck)
			},
			{
				HumanBodyBones.LeftShoulder,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm)
			},
			{
				HumanBodyBones.LeftUpperArm,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm)
			},
			{
				HumanBodyBones.LeftLowerArm,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand)
			},
			{
				HumanBodyBones.LeftHand,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal)
			},
			{
				HumanBodyBones.RightShoulder,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm)
			},
			{
				HumanBodyBones.RightUpperArm,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm)
			},
			{
				HumanBodyBones.RightLowerArm,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand)
			},
			{
				HumanBodyBones.RightHand,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal)
			},
			{
				HumanBodyBones.RightUpperLeg,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg)
			},
			{
				HumanBodyBones.RightLowerLeg,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot)
			},
			{
				HumanBodyBones.RightFoot,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightFoot, HumanBodyBones.RightToes)
			},
			{
				HumanBodyBones.RightToes,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightFoot, HumanBodyBones.RightToes)
			},
			{
				HumanBodyBones.LeftUpperLeg,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg)
			},
			{
				HumanBodyBones.LeftLowerLeg,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot)
			},
			{
				HumanBodyBones.LeftFoot,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes)
			},
			{
				HumanBodyBones.LeftToes,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes)
			},
			{
				HumanBodyBones.LeftThumbProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate)
			},
			{
				HumanBodyBones.LeftThumbIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal)
			},
			{
				HumanBodyBones.LeftThumbDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftThumbDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.LeftIndexProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate)
			},
			{
				HumanBodyBones.LeftIndexIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal)
			},
			{
				HumanBodyBones.LeftIndexDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftIndexDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.LeftMiddleProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate)
			},
			{
				HumanBodyBones.LeftMiddleIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal)
			},
			{
				HumanBodyBones.LeftMiddleDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftMiddleDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.LeftRingProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate)
			},
			{
				HumanBodyBones.LeftRingIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal)
			},
			{
				HumanBodyBones.LeftRingDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftRingDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.LeftLittleProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate)
			},
			{
				HumanBodyBones.LeftLittleIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal)
			},
			{
				HumanBodyBones.LeftLittleDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.LeftLittleDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.RightThumbProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate)
			},
			{
				HumanBodyBones.RightThumbIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal)
			},
			{
				HumanBodyBones.RightThumbDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightThumbDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.RightIndexProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate)
			},
			{
				HumanBodyBones.RightIndexIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal)
			},
			{
				HumanBodyBones.RightIndexDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightIndexDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.RightMiddleProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate)
			},
			{
				HumanBodyBones.RightMiddleIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal)
			},
			{
				HumanBodyBones.RightMiddleDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightMiddleDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.RightRingProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate)
			},
			{
				HumanBodyBones.RightRingIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal)
			},
			{
				HumanBodyBones.RightRingDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightRingDistal, HumanBodyBones.LastBone)
			},
			{
				HumanBodyBones.RightLittleProximal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate)
			},
			{
				HumanBodyBones.RightLittleIntermediate,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal)
			},
			{
				HumanBodyBones.RightLittleDistal,
				new Tuple<HumanBodyBones, HumanBodyBones>(HumanBodyBones.RightLittleDistal, HumanBodyBones.LastBone)
			}
		};

		public static readonly Dictionary<HumanBodyBones, BodySection> BoneToBodySection = new Dictionary<HumanBodyBones, BodySection>
		{
			{
				HumanBodyBones.Neck,
				BodySection.Neck
			},
			{
				HumanBodyBones.Head,
				BodySection.Head
			},
			{
				HumanBodyBones.LeftEye,
				BodySection.Head
			},
			{
				HumanBodyBones.RightEye,
				BodySection.Head
			},
			{
				HumanBodyBones.Jaw,
				BodySection.Head
			},
			{
				HumanBodyBones.Hips,
				BodySection.Hips
			},
			{
				HumanBodyBones.Spine,
				BodySection.Back
			},
			{
				HumanBodyBones.Chest,
				BodySection.Back
			},
			{
				HumanBodyBones.UpperChest,
				BodySection.Back
			},
			{
				HumanBodyBones.RightShoulder,
				BodySection.RightArm
			},
			{
				HumanBodyBones.RightUpperArm,
				BodySection.RightArm
			},
			{
				HumanBodyBones.RightLowerArm,
				BodySection.RightArm
			},
			{
				HumanBodyBones.RightHand,
				BodySection.RightArm
			},
			{
				HumanBodyBones.LeftShoulder,
				BodySection.LeftArm
			},
			{
				HumanBodyBones.LeftUpperArm,
				BodySection.LeftArm
			},
			{
				HumanBodyBones.LeftLowerArm,
				BodySection.LeftArm
			},
			{
				HumanBodyBones.LeftHand,
				BodySection.LeftArm
			},
			{
				HumanBodyBones.LeftUpperLeg,
				BodySection.LeftLeg
			},
			{
				HumanBodyBones.LeftLowerLeg,
				BodySection.LeftLeg
			},
			{
				HumanBodyBones.LeftFoot,
				BodySection.LeftFoot
			},
			{
				HumanBodyBones.LeftToes,
				BodySection.LeftFoot
			},
			{
				HumanBodyBones.RightUpperLeg,
				BodySection.RightLeg
			},
			{
				HumanBodyBones.RightLowerLeg,
				BodySection.RightLeg
			},
			{
				HumanBodyBones.RightFoot,
				BodySection.RightFoot
			},
			{
				HumanBodyBones.RightToes,
				BodySection.RightFoot
			},
			{
				HumanBodyBones.LeftThumbProximal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftThumbIntermediate,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftThumbDistal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftIndexProximal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftIndexIntermediate,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftIndexDistal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftMiddleProximal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftMiddleIntermediate,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftMiddleDistal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftRingProximal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftRingIntermediate,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftRingDistal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftLittleProximal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftLittleIntermediate,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.LeftLittleDistal,
				BodySection.LeftHand
			},
			{
				HumanBodyBones.RightThumbProximal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightThumbIntermediate,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightThumbDistal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightIndexProximal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightIndexIntermediate,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightIndexDistal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightMiddleProximal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightMiddleIntermediate,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightMiddleDistal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightRingProximal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightRingIntermediate,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightRingDistal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightLittleProximal,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightLittleIntermediate,
				BodySection.RightHand
			},
			{
				HumanBodyBones.RightLittleDistal,
				BodySection.RightHand
			}
		};

		public static readonly Dictionary<BoneId, HumanBodyBones> FullBodyBoneIdToHumanBodyBone = new Dictionary<BoneId, HumanBodyBones>
		{
			{
				BoneId.Hand_ForearmStub,
				HumanBodyBones.Hips
			},
			{
				BoneId.Hand_Thumb0,
				HumanBodyBones.Spine
			},
			{
				BoneId.Hand_Thumb2,
				HumanBodyBones.Chest
			},
			{
				BoneId.Hand_Thumb3,
				HumanBodyBones.UpperChest
			},
			{
				BoneId.Hand_Index1,
				HumanBodyBones.Neck
			},
			{
				BoneId.Hand_Index2,
				HumanBodyBones.Head
			},
			{
				BoneId.Hand_Index3,
				HumanBodyBones.LeftShoulder
			},
			{
				BoneId.Hand_Middle2,
				HumanBodyBones.LeftUpperArm
			},
			{
				BoneId.Hand_Middle3,
				HumanBodyBones.LeftLowerArm
			},
			{
				BoneId.Hand_MaxSkinnable,
				HumanBodyBones.LeftHand
			},
			{
				BoneId.Hand_Ring2,
				HumanBodyBones.RightShoulder
			},
			{
				BoneId.Hand_Pinky0,
				HumanBodyBones.RightUpperArm
			},
			{
				BoneId.Hand_Pinky1,
				HumanBodyBones.RightLowerArm
			},
			{
				BoneId.Body_RightHandWrist,
				HumanBodyBones.RightHand
			},
			{
				BoneId.Hand_IndexTip,
				HumanBodyBones.LeftThumbProximal
			},
			{
				BoneId.Hand_MiddleTip,
				HumanBodyBones.LeftThumbIntermediate
			},
			{
				BoneId.Hand_RingTip,
				HumanBodyBones.LeftThumbDistal
			},
			{
				BoneId.XRHand_LittleTip,
				HumanBodyBones.LeftIndexProximal
			},
			{
				BoneId.XRHand_Max,
				HumanBodyBones.LeftIndexIntermediate
			},
			{
				BoneId.Body_LeftHandIndexDistal,
				HumanBodyBones.LeftIndexDistal
			},
			{
				BoneId.Body_LeftHandMiddleProximal,
				HumanBodyBones.LeftMiddleProximal
			},
			{
				BoneId.Body_LeftHandMiddleIntermediate,
				HumanBodyBones.LeftMiddleIntermediate
			},
			{
				BoneId.Body_LeftHandMiddleDistal,
				HumanBodyBones.LeftMiddleDistal
			},
			{
				BoneId.Body_LeftHandRingProximal,
				HumanBodyBones.LeftRingProximal
			},
			{
				BoneId.Body_LeftHandRingIntermediate,
				HumanBodyBones.LeftRingIntermediate
			},
			{
				BoneId.Body_LeftHandRingDistal,
				HumanBodyBones.LeftRingDistal
			},
			{
				BoneId.Body_LeftHandLittleProximal,
				HumanBodyBones.LeftLittleProximal
			},
			{
				BoneId.Body_LeftHandLittleIntermediate,
				HumanBodyBones.LeftLittleIntermediate
			},
			{
				BoneId.Body_LeftHandLittleDistal,
				HumanBodyBones.LeftLittleDistal
			},
			{
				BoneId.Body_RightHandThumbMetacarpal,
				HumanBodyBones.RightThumbProximal
			},
			{
				BoneId.Body_RightHandThumbProximal,
				HumanBodyBones.RightThumbIntermediate
			},
			{
				BoneId.Body_RightHandThumbDistal,
				HumanBodyBones.RightThumbDistal
			},
			{
				BoneId.Body_RightHandIndexProximal,
				HumanBodyBones.RightIndexProximal
			},
			{
				BoneId.Body_RightHandIndexIntermediate,
				HumanBodyBones.RightIndexIntermediate
			},
			{
				BoneId.Body_RightHandIndexDistal,
				HumanBodyBones.RightIndexDistal
			},
			{
				BoneId.Body_RightHandMiddleProximal,
				HumanBodyBones.RightMiddleProximal
			},
			{
				BoneId.Body_RightHandMiddleIntermediate,
				HumanBodyBones.RightMiddleIntermediate
			},
			{
				BoneId.Body_RightHandMiddleDistal,
				HumanBodyBones.RightMiddleDistal
			},
			{
				BoneId.Body_RightHandRingProximal,
				HumanBodyBones.RightRingProximal
			},
			{
				BoneId.Body_RightHandRingIntermediate,
				HumanBodyBones.RightRingIntermediate
			},
			{
				BoneId.Body_RightHandRingDistal,
				HumanBodyBones.RightRingDistal
			},
			{
				BoneId.Body_RightHandLittleProximal,
				HumanBodyBones.RightLittleProximal
			},
			{
				BoneId.Body_RightHandLittleIntermediate,
				HumanBodyBones.RightLittleIntermediate
			},
			{
				BoneId.Body_RightHandLittleDistal,
				HumanBodyBones.RightLittleDistal
			},
			{
				BoneId.Body_End,
				HumanBodyBones.LeftUpperLeg
			},
			{
				BoneId.FullBody_LeftLowerLeg,
				HumanBodyBones.LeftLowerLeg
			},
			{
				BoneId.FullBody_LeftFootAnkle,
				HumanBodyBones.LeftFoot
			},
			{
				BoneId.FullBody_LeftFootBall,
				HumanBodyBones.LeftToes
			},
			{
				BoneId.FullBody_RightUpperLeg,
				HumanBodyBones.RightUpperLeg
			},
			{
				BoneId.FullBody_RightLowerLeg,
				HumanBodyBones.RightLowerLeg
			},
			{
				BoneId.FullBody_RightFootAnkle,
				HumanBodyBones.RightFoot
			},
			{
				BoneId.FullBody_RightFootBall,
				HumanBodyBones.RightToes
			}
		};

		public static readonly Dictionary<BoneId, HumanBodyBones> BoneIdToHumanBodyBone = new Dictionary<BoneId, HumanBodyBones>
		{
			{
				BoneId.Hand_ForearmStub,
				HumanBodyBones.Hips
			},
			{
				BoneId.Hand_Thumb0,
				HumanBodyBones.Spine
			},
			{
				BoneId.Hand_Thumb2,
				HumanBodyBones.Chest
			},
			{
				BoneId.Hand_Thumb3,
				HumanBodyBones.UpperChest
			},
			{
				BoneId.Hand_Index1,
				HumanBodyBones.Neck
			},
			{
				BoneId.Hand_Index2,
				HumanBodyBones.Head
			},
			{
				BoneId.Hand_Index3,
				HumanBodyBones.LeftShoulder
			},
			{
				BoneId.Hand_Middle2,
				HumanBodyBones.LeftUpperArm
			},
			{
				BoneId.Hand_Middle3,
				HumanBodyBones.LeftLowerArm
			},
			{
				BoneId.Hand_MaxSkinnable,
				HumanBodyBones.LeftHand
			},
			{
				BoneId.Hand_Ring2,
				HumanBodyBones.RightShoulder
			},
			{
				BoneId.Hand_Pinky0,
				HumanBodyBones.RightUpperArm
			},
			{
				BoneId.Hand_Pinky1,
				HumanBodyBones.RightLowerArm
			},
			{
				BoneId.Body_RightHandWrist,
				HumanBodyBones.RightHand
			},
			{
				BoneId.Hand_IndexTip,
				HumanBodyBones.LeftThumbProximal
			},
			{
				BoneId.Hand_MiddleTip,
				HumanBodyBones.LeftThumbIntermediate
			},
			{
				BoneId.Hand_RingTip,
				HumanBodyBones.LeftThumbDistal
			},
			{
				BoneId.XRHand_LittleTip,
				HumanBodyBones.LeftIndexProximal
			},
			{
				BoneId.XRHand_Max,
				HumanBodyBones.LeftIndexIntermediate
			},
			{
				BoneId.Body_LeftHandIndexDistal,
				HumanBodyBones.LeftIndexDistal
			},
			{
				BoneId.Body_LeftHandMiddleProximal,
				HumanBodyBones.LeftMiddleProximal
			},
			{
				BoneId.Body_LeftHandMiddleIntermediate,
				HumanBodyBones.LeftMiddleIntermediate
			},
			{
				BoneId.Body_LeftHandMiddleDistal,
				HumanBodyBones.LeftMiddleDistal
			},
			{
				BoneId.Body_LeftHandRingProximal,
				HumanBodyBones.LeftRingProximal
			},
			{
				BoneId.Body_LeftHandRingIntermediate,
				HumanBodyBones.LeftRingIntermediate
			},
			{
				BoneId.Body_LeftHandRingDistal,
				HumanBodyBones.LeftRingDistal
			},
			{
				BoneId.Body_LeftHandLittleProximal,
				HumanBodyBones.LeftLittleProximal
			},
			{
				BoneId.Body_LeftHandLittleIntermediate,
				HumanBodyBones.LeftLittleIntermediate
			},
			{
				BoneId.Body_LeftHandLittleDistal,
				HumanBodyBones.LeftLittleDistal
			},
			{
				BoneId.Body_RightHandThumbMetacarpal,
				HumanBodyBones.RightThumbProximal
			},
			{
				BoneId.Body_RightHandThumbProximal,
				HumanBodyBones.RightThumbIntermediate
			},
			{
				BoneId.Body_RightHandThumbDistal,
				HumanBodyBones.RightThumbDistal
			},
			{
				BoneId.Body_RightHandIndexProximal,
				HumanBodyBones.RightIndexProximal
			},
			{
				BoneId.Body_RightHandIndexIntermediate,
				HumanBodyBones.RightIndexIntermediate
			},
			{
				BoneId.Body_RightHandIndexDistal,
				HumanBodyBones.RightIndexDistal
			},
			{
				BoneId.Body_RightHandMiddleProximal,
				HumanBodyBones.RightMiddleProximal
			},
			{
				BoneId.Body_RightHandMiddleIntermediate,
				HumanBodyBones.RightMiddleIntermediate
			},
			{
				BoneId.Body_RightHandMiddleDistal,
				HumanBodyBones.RightMiddleDistal
			},
			{
				BoneId.Body_RightHandRingProximal,
				HumanBodyBones.RightRingProximal
			},
			{
				BoneId.Body_RightHandRingIntermediate,
				HumanBodyBones.RightRingIntermediate
			},
			{
				BoneId.Body_RightHandRingDistal,
				HumanBodyBones.RightRingDistal
			},
			{
				BoneId.Body_RightHandLittleProximal,
				HumanBodyBones.RightLittleProximal
			},
			{
				BoneId.Body_RightHandLittleIntermediate,
				HumanBodyBones.RightLittleIntermediate
			},
			{
				BoneId.Body_RightHandLittleDistal,
				HumanBodyBones.RightLittleDistal
			}
		};

		public static readonly Dictionary<BoneId, Tuple<BoneId, BoneId>> FullBoneIdToJointPair = new Dictionary<BoneId, Tuple<BoneId, BoneId>>
		{
			{
				BoneId.Hand_Index1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Index1, BoneId.Hand_Index2)
			},
			{
				BoneId.Hand_Index2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Index2, BoneId.Invalid)
			},
			{
				BoneId.Hand_Start,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Start, BoneId.Hand_ForearmStub)
			},
			{
				BoneId.Hand_ForearmStub,
				new Tuple<BoneId, BoneId>(BoneId.Hand_ForearmStub, BoneId.Hand_Thumb0)
			},
			{
				BoneId.Hand_Thumb0,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb0, BoneId.Hand_Thumb1)
			},
			{
				BoneId.Hand_Thumb1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb1, BoneId.Hand_Thumb2)
			},
			{
				BoneId.Hand_Thumb2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb2, BoneId.Hand_Thumb3)
			},
			{
				BoneId.Hand_Thumb3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb3, BoneId.Hand_Index1)
			},
			{
				BoneId.Hand_Index3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Index3, BoneId.Hand_Middle2)
			},
			{
				BoneId.Hand_Middle1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Middle1, BoneId.Hand_Middle2)
			},
			{
				BoneId.Hand_Middle2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Middle2, BoneId.Hand_Middle3)
			},
			{
				BoneId.Hand_Middle3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Middle3, BoneId.Hand_MaxSkinnable)
			},
			{
				BoneId.Hand_MaxSkinnable,
				new Tuple<BoneId, BoneId>(BoneId.Hand_MaxSkinnable, BoneId.Body_LeftHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_Pinky3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky3, BoneId.Body_LeftHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_Ring1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Ring1, BoneId.Body_LeftHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_IndexTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_IndexTip, BoneId.Hand_MiddleTip)
			},
			{
				BoneId.Hand_MiddleTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_MiddleTip, BoneId.Hand_RingTip)
			},
			{
				BoneId.Hand_RingTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_RingTip, BoneId.Hand_PinkyTip)
			},
			{
				BoneId.Hand_PinkyTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_RingTip, BoneId.Hand_PinkyTip)
			},
			{
				BoneId.Hand_End,
				new Tuple<BoneId, BoneId>(BoneId.Hand_End, BoneId.XRHand_LittleTip)
			},
			{
				BoneId.XRHand_LittleTip,
				new Tuple<BoneId, BoneId>(BoneId.XRHand_LittleTip, BoneId.XRHand_Max)
			},
			{
				BoneId.XRHand_Max,
				new Tuple<BoneId, BoneId>(BoneId.XRHand_Max, BoneId.Body_LeftHandIndexDistal)
			},
			{
				BoneId.Body_LeftHandIndexDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandIndexDistal, BoneId.Body_LeftHandIndexTip)
			},
			{
				BoneId.Body_LeftHandIndexTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandIndexDistal, BoneId.Body_LeftHandIndexTip)
			},
			{
				BoneId.Body_LeftHandMiddleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleMetacarpal, BoneId.Body_LeftHandMiddleProximal)
			},
			{
				BoneId.Body_LeftHandMiddleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleProximal, BoneId.Body_LeftHandMiddleIntermediate)
			},
			{
				BoneId.Body_LeftHandMiddleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleIntermediate, BoneId.Body_LeftHandMiddleDistal)
			},
			{
				BoneId.Body_LeftHandMiddleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleDistal, BoneId.Body_LeftHandMiddleTip)
			},
			{
				BoneId.Body_LeftHandMiddleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleDistal, BoneId.Body_LeftHandMiddleTip)
			},
			{
				BoneId.Body_LeftHandRingMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingMetacarpal, BoneId.Body_LeftHandRingProximal)
			},
			{
				BoneId.Body_LeftHandRingProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingProximal, BoneId.Body_LeftHandRingIntermediate)
			},
			{
				BoneId.Body_LeftHandRingIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingIntermediate, BoneId.Body_LeftHandRingDistal)
			},
			{
				BoneId.Body_LeftHandRingDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingDistal, BoneId.Body_LeftHandRingTip)
			},
			{
				BoneId.Body_LeftHandRingTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingDistal, BoneId.Body_LeftHandRingTip)
			},
			{
				BoneId.Body_LeftHandLittleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleMetacarpal, BoneId.Body_LeftHandLittleProximal)
			},
			{
				BoneId.Body_LeftHandLittleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleProximal, BoneId.Body_LeftHandLittleIntermediate)
			},
			{
				BoneId.Body_LeftHandLittleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleIntermediate, BoneId.Body_LeftHandLittleDistal)
			},
			{
				BoneId.Body_LeftHandLittleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleDistal, BoneId.Body_LeftHandLittleTip)
			},
			{
				BoneId.Body_LeftHandLittleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleDistal, BoneId.Body_LeftHandLittleTip)
			},
			{
				BoneId.Hand_Ring2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Ring2, BoneId.Hand_Pinky0)
			},
			{
				BoneId.Hand_Ring3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Ring3, BoneId.Hand_Pinky0)
			},
			{
				BoneId.Hand_Pinky0,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky0, BoneId.Hand_Pinky1)
			},
			{
				BoneId.Hand_Pinky1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky1, BoneId.Body_RightHandWrist)
			},
			{
				BoneId.Body_RightHandWrist,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandWrist, BoneId.Body_RightHandMiddleMetacarpal)
			},
			{
				BoneId.Body_RightHandPalm,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandPalm, BoneId.Body_RightHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_Pinky2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky2, BoneId.Body_RightHandMiddleMetacarpal)
			},
			{
				BoneId.Body_RightHandThumbMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbMetacarpal, BoneId.Body_RightHandThumbProximal)
			},
			{
				BoneId.Body_RightHandThumbProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbProximal, BoneId.Body_RightHandThumbDistal)
			},
			{
				BoneId.Body_RightHandThumbDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbDistal, BoneId.Body_RightHandThumbTip)
			},
			{
				BoneId.Body_RightHandThumbTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbDistal, BoneId.Body_RightHandThumbTip)
			},
			{
				BoneId.Body_RightHandIndexMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexMetacarpal, BoneId.Body_RightHandIndexProximal)
			},
			{
				BoneId.Body_RightHandIndexProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexProximal, BoneId.Body_RightHandIndexIntermediate)
			},
			{
				BoneId.Body_RightHandIndexIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexIntermediate, BoneId.Body_RightHandIndexDistal)
			},
			{
				BoneId.Body_RightHandIndexDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexDistal, BoneId.Body_RightHandIndexTip)
			},
			{
				BoneId.Body_RightHandIndexTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexDistal, BoneId.Body_RightHandIndexTip)
			},
			{
				BoneId.Body_RightHandMiddleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleMetacarpal, BoneId.Body_RightHandMiddleProximal)
			},
			{
				BoneId.Body_RightHandMiddleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleProximal, BoneId.Body_RightHandMiddleIntermediate)
			},
			{
				BoneId.Body_RightHandMiddleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleIntermediate, BoneId.Body_RightHandMiddleDistal)
			},
			{
				BoneId.Body_RightHandMiddleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleDistal, BoneId.Body_RightHandMiddleTip)
			},
			{
				BoneId.Body_RightHandMiddleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleDistal, BoneId.Body_RightHandMiddleTip)
			},
			{
				BoneId.Body_RightHandRingMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingMetacarpal, BoneId.Body_RightHandRingProximal)
			},
			{
				BoneId.Body_RightHandRingProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingProximal, BoneId.Body_RightHandRingIntermediate)
			},
			{
				BoneId.Body_RightHandRingIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingIntermediate, BoneId.Body_RightHandRingDistal)
			},
			{
				BoneId.Body_RightHandRingDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingDistal, BoneId.Body_RightHandRingTip)
			},
			{
				BoneId.Body_RightHandRingTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingDistal, BoneId.Body_RightHandRingTip)
			},
			{
				BoneId.Body_RightHandLittleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleMetacarpal, BoneId.Body_RightHandLittleProximal)
			},
			{
				BoneId.Body_RightHandLittleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleProximal, BoneId.Body_RightHandLittleIntermediate)
			},
			{
				BoneId.Body_RightHandLittleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleIntermediate, BoneId.Body_RightHandLittleDistal)
			},
			{
				BoneId.Body_RightHandLittleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleDistal, BoneId.Body_RightHandLittleTip)
			},
			{
				BoneId.Body_RightHandLittleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleDistal, BoneId.Body_RightHandLittleTip)
			},
			{
				BoneId.Body_End,
				new Tuple<BoneId, BoneId>(BoneId.Body_End, BoneId.FullBody_LeftLowerLeg)
			},
			{
				BoneId.FullBody_LeftLowerLeg,
				new Tuple<BoneId, BoneId>(BoneId.FullBody_LeftLowerLeg, BoneId.FullBody_LeftFootAnkle)
			},
			{
				BoneId.FullBody_LeftFootAnkle,
				new Tuple<BoneId, BoneId>(BoneId.FullBody_LeftFootAnkle, BoneId.FullBody_LeftFootBall)
			},
			{
				BoneId.FullBody_LeftFootBall,
				new Tuple<BoneId, BoneId>(BoneId.FullBody_LeftFootAnkle, BoneId.FullBody_LeftFootBall)
			},
			{
				BoneId.FullBody_RightUpperLeg,
				new Tuple<BoneId, BoneId>(BoneId.FullBody_RightUpperLeg, BoneId.FullBody_RightLowerLeg)
			},
			{
				BoneId.FullBody_RightLowerLeg,
				new Tuple<BoneId, BoneId>(BoneId.FullBody_RightLowerLeg, BoneId.FullBody_RightFootAnkle)
			},
			{
				BoneId.FullBody_RightFootAnkle,
				new Tuple<BoneId, BoneId>(BoneId.FullBody_RightFootAnkle, BoneId.FullBody_RightFootBall)
			},
			{
				BoneId.FullBody_RightFootBall,
				new Tuple<BoneId, BoneId>(BoneId.FullBody_RightFootAnkle, BoneId.FullBody_RightFootBall)
			}
		};

		public static readonly Dictionary<BoneId, Tuple<BoneId, BoneId>> BoneIdToJointPair = new Dictionary<BoneId, Tuple<BoneId, BoneId>>
		{
			{
				BoneId.Hand_Index1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Index1, BoneId.Hand_Index2)
			},
			{
				BoneId.Hand_Index2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Index2, BoneId.Invalid)
			},
			{
				BoneId.Hand_Start,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Start, BoneId.Hand_ForearmStub)
			},
			{
				BoneId.Hand_ForearmStub,
				new Tuple<BoneId, BoneId>(BoneId.Hand_ForearmStub, BoneId.Hand_Thumb0)
			},
			{
				BoneId.Hand_Thumb0,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb0, BoneId.Hand_Thumb1)
			},
			{
				BoneId.Hand_Thumb1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb1, BoneId.Hand_Thumb2)
			},
			{
				BoneId.Hand_Thumb2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb2, BoneId.Hand_Thumb3)
			},
			{
				BoneId.Hand_Thumb3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Thumb3, BoneId.Hand_Index1)
			},
			{
				BoneId.Hand_Index3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Index3, BoneId.Hand_Middle2)
			},
			{
				BoneId.Hand_Middle1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Middle1, BoneId.Hand_Middle2)
			},
			{
				BoneId.Hand_Middle2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Middle2, BoneId.Hand_Middle3)
			},
			{
				BoneId.Hand_Middle3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Middle3, BoneId.Hand_MaxSkinnable)
			},
			{
				BoneId.Hand_MaxSkinnable,
				new Tuple<BoneId, BoneId>(BoneId.Hand_MaxSkinnable, BoneId.Body_LeftHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_Pinky3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky3, BoneId.Body_LeftHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_Ring1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Ring1, BoneId.Body_LeftHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_IndexTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_IndexTip, BoneId.Hand_MiddleTip)
			},
			{
				BoneId.Hand_MiddleTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_MiddleTip, BoneId.Hand_RingTip)
			},
			{
				BoneId.Hand_RingTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_RingTip, BoneId.Hand_PinkyTip)
			},
			{
				BoneId.Hand_PinkyTip,
				new Tuple<BoneId, BoneId>(BoneId.Hand_RingTip, BoneId.Hand_PinkyTip)
			},
			{
				BoneId.Hand_End,
				new Tuple<BoneId, BoneId>(BoneId.Hand_End, BoneId.XRHand_LittleTip)
			},
			{
				BoneId.XRHand_LittleTip,
				new Tuple<BoneId, BoneId>(BoneId.XRHand_LittleTip, BoneId.XRHand_Max)
			},
			{
				BoneId.XRHand_Max,
				new Tuple<BoneId, BoneId>(BoneId.XRHand_Max, BoneId.Body_LeftHandIndexDistal)
			},
			{
				BoneId.Body_LeftHandIndexDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandIndexDistal, BoneId.Body_LeftHandIndexTip)
			},
			{
				BoneId.Body_LeftHandIndexTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandIndexDistal, BoneId.Body_LeftHandIndexTip)
			},
			{
				BoneId.Body_LeftHandMiddleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleMetacarpal, BoneId.Body_LeftHandMiddleProximal)
			},
			{
				BoneId.Body_LeftHandMiddleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleProximal, BoneId.Body_LeftHandMiddleIntermediate)
			},
			{
				BoneId.Body_LeftHandMiddleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleIntermediate, BoneId.Body_LeftHandMiddleDistal)
			},
			{
				BoneId.Body_LeftHandMiddleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleDistal, BoneId.Body_LeftHandMiddleTip)
			},
			{
				BoneId.Body_LeftHandMiddleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandMiddleDistal, BoneId.Body_LeftHandMiddleTip)
			},
			{
				BoneId.Body_LeftHandRingMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingMetacarpal, BoneId.Body_LeftHandRingProximal)
			},
			{
				BoneId.Body_LeftHandRingProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingProximal, BoneId.Body_LeftHandRingIntermediate)
			},
			{
				BoneId.Body_LeftHandRingIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingIntermediate, BoneId.Body_LeftHandRingDistal)
			},
			{
				BoneId.Body_LeftHandRingDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingDistal, BoneId.Body_LeftHandRingTip)
			},
			{
				BoneId.Body_LeftHandRingTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandRingDistal, BoneId.Body_LeftHandRingTip)
			},
			{
				BoneId.Body_LeftHandLittleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleMetacarpal, BoneId.Body_LeftHandLittleProximal)
			},
			{
				BoneId.Body_LeftHandLittleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleProximal, BoneId.Body_LeftHandLittleIntermediate)
			},
			{
				BoneId.Body_LeftHandLittleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleIntermediate, BoneId.Body_LeftHandLittleDistal)
			},
			{
				BoneId.Body_LeftHandLittleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleDistal, BoneId.Body_LeftHandLittleTip)
			},
			{
				BoneId.Body_LeftHandLittleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_LeftHandLittleDistal, BoneId.Body_LeftHandLittleTip)
			},
			{
				BoneId.Hand_Ring2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Ring2, BoneId.Hand_Pinky0)
			},
			{
				BoneId.Hand_Ring3,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Ring3, BoneId.Hand_Pinky0)
			},
			{
				BoneId.Hand_Pinky0,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky0, BoneId.Hand_Pinky1)
			},
			{
				BoneId.Hand_Pinky1,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky1, BoneId.Body_RightHandWrist)
			},
			{
				BoneId.Body_RightHandWrist,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandWrist, BoneId.Body_RightHandMiddleMetacarpal)
			},
			{
				BoneId.Body_RightHandPalm,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandPalm, BoneId.Body_RightHandMiddleMetacarpal)
			},
			{
				BoneId.Hand_Pinky2,
				new Tuple<BoneId, BoneId>(BoneId.Hand_Pinky2, BoneId.Body_RightHandMiddleMetacarpal)
			},
			{
				BoneId.Body_RightHandThumbMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbMetacarpal, BoneId.Body_RightHandThumbProximal)
			},
			{
				BoneId.Body_RightHandThumbProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbProximal, BoneId.Body_RightHandThumbDistal)
			},
			{
				BoneId.Body_RightHandThumbDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbDistal, BoneId.Body_RightHandThumbTip)
			},
			{
				BoneId.Body_RightHandThumbTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandThumbDistal, BoneId.Body_RightHandThumbTip)
			},
			{
				BoneId.Body_RightHandIndexMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexMetacarpal, BoneId.Body_RightHandIndexProximal)
			},
			{
				BoneId.Body_RightHandIndexProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexProximal, BoneId.Body_RightHandIndexIntermediate)
			},
			{
				BoneId.Body_RightHandIndexIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexIntermediate, BoneId.Body_RightHandIndexDistal)
			},
			{
				BoneId.Body_RightHandIndexDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexDistal, BoneId.Body_RightHandIndexTip)
			},
			{
				BoneId.Body_RightHandIndexTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandIndexDistal, BoneId.Body_RightHandIndexTip)
			},
			{
				BoneId.Body_RightHandMiddleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleMetacarpal, BoneId.Body_RightHandMiddleProximal)
			},
			{
				BoneId.Body_RightHandMiddleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleProximal, BoneId.Body_RightHandMiddleIntermediate)
			},
			{
				BoneId.Body_RightHandMiddleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleIntermediate, BoneId.Body_RightHandMiddleDistal)
			},
			{
				BoneId.Body_RightHandMiddleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleDistal, BoneId.Body_RightHandMiddleTip)
			},
			{
				BoneId.Body_RightHandMiddleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandMiddleDistal, BoneId.Body_RightHandMiddleTip)
			},
			{
				BoneId.Body_RightHandRingMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingMetacarpal, BoneId.Body_RightHandRingProximal)
			},
			{
				BoneId.Body_RightHandRingProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingProximal, BoneId.Body_RightHandRingIntermediate)
			},
			{
				BoneId.Body_RightHandRingIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingIntermediate, BoneId.Body_RightHandRingDistal)
			},
			{
				BoneId.Body_RightHandRingDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingDistal, BoneId.Body_RightHandRingTip)
			},
			{
				BoneId.Body_RightHandRingTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandRingDistal, BoneId.Body_RightHandRingTip)
			},
			{
				BoneId.Body_RightHandLittleMetacarpal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleMetacarpal, BoneId.Body_RightHandLittleProximal)
			},
			{
				BoneId.Body_RightHandLittleProximal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleProximal, BoneId.Body_RightHandLittleIntermediate)
			},
			{
				BoneId.Body_RightHandLittleIntermediate,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleIntermediate, BoneId.Body_RightHandLittleDistal)
			},
			{
				BoneId.Body_RightHandLittleDistal,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleDistal, BoneId.Body_RightHandLittleTip)
			},
			{
				BoneId.Body_RightHandLittleTip,
				new Tuple<BoneId, BoneId>(BoneId.Body_RightHandLittleDistal, BoneId.Body_RightHandLittleTip)
			}
		};

		public Dictionary<HumanBodyBones, Tuple<HumanBodyBones, HumanBodyBones>> GetBoneToJointPair => BoneToJointPair;

		public Dictionary<HumanBodyBones, BodySection> GetBoneToBodySection => BoneToBodySection;

		public Dictionary<BoneId, HumanBodyBones> GetFullBodyBoneIdToHumanBodyBone => FullBodyBoneIdToHumanBodyBone;

		public Dictionary<BoneId, HumanBodyBones> GetBoneIdToHumanBodyBone => BoneIdToHumanBodyBone;

		public Dictionary<BoneId, Tuple<BoneId, BoneId>> GetFullBodyBoneIdToJointPair => FullBoneIdToJointPair;

		public Dictionary<BoneId, Tuple<BoneId, BoneId>> GetBoneIdToJointPair => BoneIdToJointPair;
	}

	protected class OVRSkeletonMetadata
	{
		public class BoneData
		{
			public Transform OriginalJoint;

			public Vector3 FromPosition;

			public Vector3 ToPosition;

			public Transform JointPairStart;

			public Transform JointPairEnd;

			public Quaternion JointPairOrientation;

			public Quaternion? CorrectionQuaternion;

			public Transform ParentTransform;

			public bool DegenerateJoint;

			public BoneData()
			{
			}

			public BoneData(BoneData otherBoneData)
			{
				OriginalJoint = otherBoneData.OriginalJoint;
				FromPosition = otherBoneData.FromPosition;
				ToPosition = otherBoneData.ToPosition;
				JointPairStart = otherBoneData.JointPairStart;
				JointPairEnd = otherBoneData.JointPairEnd;
				JointPairOrientation = otherBoneData.JointPairOrientation;
				CorrectionQuaternion = otherBoneData.CorrectionQuaternion;
				ParentTransform = otherBoneData.ParentTransform;
				DegenerateJoint = otherBoneData.DegenerateJoint;
			}
		}

		private readonly HumanBodyBones[] _boneEnumValues = (HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones));

		public Dictionary<HumanBodyBones, BoneData> BodyToBoneData { get; } = new Dictionary<HumanBodyBones, BoneData>();


		public OVRSkeletonMetadata(OVRSkeletonMetadata otherSkeletonMetaData)
		{
			BodyToBoneData = new Dictionary<HumanBodyBones, BoneData>();
			foreach (HumanBodyBones key in otherSkeletonMetaData.BodyToBoneData.Keys)
			{
				BoneData otherBoneData = otherSkeletonMetaData.BodyToBoneData[key];
				BodyToBoneData[key] = new BoneData(otherBoneData);
			}
		}

		public OVRSkeletonMetadata(Animator animator, OVRHumanBodyBonesMappingsInterface bodyBonesMappingInterface = null)
		{
			BuildBoneData(animator, bodyBonesMappingInterface);
		}

		public OVRSkeletonMetadata(OVRSkeleton skeleton, bool useBindPose, Dictionary<BoneId, HumanBodyBones> customBoneIdToHumanBodyBone, OVRHumanBodyBonesMappingsInterface bodyBonesMappingInterface)
		{
			BuildBoneDataSkeleton(skeleton, useBindPose, customBoneIdToHumanBodyBone, bodyBonesMappingInterface);
		}

		public OVRSkeletonMetadata(OVRSkeleton skeleton, bool useBindPose, Dictionary<BoneId, HumanBodyBones> customBoneIdToHumanBodyBone, bool useFullBody, OVRHumanBodyBonesMappingsInterface bodyBonesMappingInterface)
		{
			if (useFullBody)
			{
				BuildBoneDataSkeletonFullBody(skeleton, useBindPose, customBoneIdToHumanBodyBone, bodyBonesMappingInterface);
			}
			else
			{
				BuildBoneDataSkeleton(skeleton, useBindPose, customBoneIdToHumanBodyBone, bodyBonesMappingInterface);
			}
		}

		public void BuildBoneDataSkeleton(OVRSkeleton skeleton, bool useBindPose, Dictionary<BoneId, HumanBodyBones> customBoneIdToHumanBodyBone, OVRHumanBodyBonesMappingsInterface bodyBonesMappingInterface)
		{
			AssembleSkeleton(skeleton, useBindPose, customBoneIdToHumanBodyBone, bodyBonesMappingInterface);
		}

		public void BuildBoneDataSkeletonFullBody(OVRSkeleton skeleton, bool useBindPose, Dictionary<BoneId, HumanBodyBones> customBoneIdToHumanBodyBone, OVRHumanBodyBonesMappingsInterface bodyBonesMappingInterface)
		{
			AssembleSkeleton(skeleton, useBindPose, customBoneIdToHumanBodyBone, bodyBonesMappingInterface, useFullBody: true);
		}

		private void AssembleSkeleton(OVRSkeleton skeleton, bool useBindPose, Dictionary<BoneId, HumanBodyBones> customBoneIdToHumanBodyBone, OVRHumanBodyBonesMappingsInterface bodyBonesMappingInterface, bool useFullBody = false)
		{
			if (BodyToBoneData.Count != 0)
			{
				BodyToBoneData.Clear();
			}
			IList<OVRBone> list = (useBindPose ? skeleton.BindPoses : skeleton.Bones);
			for (int i = 0; i < list.Count; i++)
			{
				OVRBone oVRBone = list[i];
				if (!customBoneIdToHumanBodyBone.ContainsKey(oVRBone.Id))
				{
					continue;
				}
				HumanBodyBones key = customBoneIdToHumanBodyBone[oVRBone.Id];
				BoneData boneData = new BoneData();
				boneData.OriginalJoint = oVRBone.Transform;
				if (useFullBody)
				{
					if (!bodyBonesMappingInterface.GetFullBodyBoneIdToJointPair.ContainsKey(oVRBone.Id))
					{
						Debug.LogError($"Can't find {oVRBone.Id} in bone Id to joint pair map!");
						continue;
					}
				}
				else if (!bodyBonesMappingInterface.GetBoneIdToJointPair.ContainsKey(oVRBone.Id))
				{
					Debug.LogError($"Can't find {oVRBone.Id} in bone Id to joint pair map!");
					continue;
				}
				Tuple<BoneId, BoneId> obj = (useFullBody ? bodyBonesMappingInterface.GetFullBodyBoneIdToJointPair[oVRBone.Id] : bodyBonesMappingInterface.GetBoneIdToJointPair[oVRBone.Id]);
				BoneId item = obj.Item1;
				BoneId item2 = obj.Item2;
				boneData.JointPairStart = ((item == oVRBone.Id) ? oVRBone.Transform : FindBoneWithBoneId(list, item).Transform);
				boneData.JointPairEnd = ((item2 != BoneId.Invalid) ? FindBoneWithBoneId(list, item2).Transform : boneData.JointPairStart);
				boneData.ParentTransform = list[oVRBone.ParentBoneIndex].Transform;
				if (boneData.JointPairStart == null)
				{
					Debug.LogWarning($"{oVRBone.Id} has invalid start joint.");
				}
				if (boneData.JointPairEnd == null)
				{
					Debug.LogWarning($"{oVRBone.Id} has invalid end joint.");
				}
				BodyToBoneData.Add(key, boneData);
			}
		}

		private static OVRBone FindBoneWithBoneId(IList<OVRBone> bones, BoneId boneId)
		{
			for (int i = 0; i < bones.Count; i++)
			{
				if (bones[i].Id == boneId)
				{
					return bones[i];
				}
			}
			return null;
		}

		private void BuildBoneData(Animator animator, OVRHumanBodyBonesMappingsInterface bodyBonesMappingInterface)
		{
			if (BodyToBoneData.Count != 0)
			{
				BodyToBoneData.Clear();
			}
			HumanBodyBones[] boneEnumValues = _boneEnumValues;
			foreach (HumanBodyBones humanBodyBones in boneEnumValues)
			{
				if (humanBodyBones != HumanBodyBones.LastBone)
				{
					if (animator.avatar == null)
					{
						Debug.LogWarning($"{animator} has no avatar.");
					}
					if (animator.avatar != null && !animator.avatar.isHuman)
					{
						Debug.LogWarning($"{animator} does not have have a " + "valid human description!");
					}
					Transform boneTransform = animator.GetBoneTransform(humanBodyBones);
					if (!(boneTransform == null))
					{
						BoneData boneData = new BoneData();
						boneData.OriginalJoint = boneTransform;
						BodyToBoneData.Add(humanBodyBones, boneData);
					}
				}
			}
			foreach (HumanBodyBones key in BodyToBoneData.Keys)
			{
				BoneData boneData2 = BodyToBoneData[key];
				Tuple<HumanBodyBones, HumanBodyBones> tuple = bodyBonesMappingInterface.GetBoneToJointPair[key];
				boneData2.JointPairStart = ((tuple.Item1 != HumanBodyBones.LastBone) ? animator.GetBoneTransform(tuple.Item1) : boneData2.OriginalJoint);
				boneData2.JointPairEnd = ((tuple.Item2 != HumanBodyBones.LastBone) ? animator.GetBoneTransform(tuple.Item2) : FindFirstChild(boneData2.OriginalJoint, boneData2.OriginalJoint));
				boneData2.ParentTransform = boneData2.OriginalJoint.parent;
				if (boneData2.JointPairStart == null)
				{
					Debug.LogWarning($"{key} has invalid start joint, setting to {boneData2.OriginalJoint}.");
					boneData2.JointPairStart = boneData2.OriginalJoint;
				}
				if (boneData2.JointPairEnd == null)
				{
					Debug.LogWarning($"{key} has invalid end joint.");
				}
			}
		}

		public void BuildCoordinateAxesForAllBones()
		{
			foreach (HumanBodyBones key in BodyToBoneData.Keys)
			{
				BoneData boneData = BodyToBoneData[key];
				Vector3 position = boneData.JointPairStart.position;
				Vector3 vector;
				if (boneData.JointPairEnd == null || boneData.JointPairEnd == boneData.JointPairStart || (boneData.JointPairEnd.position - boneData.JointPairStart.position).magnitude < Mathf.Epsilon)
				{
					Transform parentTransform = boneData.ParentTransform;
					Transform jointPairStart = boneData.JointPairStart;
					position = parentTransform.position;
					vector = jointPairStart.position;
					boneData.DegenerateJoint = true;
				}
				else
				{
					vector = boneData.JointPairEnd.position;
					boneData.DegenerateJoint = false;
				}
				if (key == HumanBodyBones.LeftHand || key == HumanBodyBones.RightHand)
				{
					vector = FixJointPairEndPositionHand(vector, key);
					HumanBodyBones humanBodyBones = ((key == HumanBodyBones.LeftHand) ? HumanBodyBones.LeftThumbIntermediate : HumanBodyBones.RightThumbIntermediate);
					if (!BodyToBoneData.ContainsKey(humanBodyBones))
					{
						Debug.LogWarning($"Character is missing bone corresponding to {humanBodyBones}," + " used for creating right vector. Using backup approach.");
						boneData.JointPairOrientation = CreateQuaternionForBoneData(position, vector);
					}
					else
					{
						Vector3 rightVector = BodyToBoneData[humanBodyBones].OriginalJoint.position - position;
						boneData.JointPairOrientation = CreateQuaternionForBoneDataWithRightVec(position, vector, rightVector);
					}
				}
				else
				{
					boneData.JointPairOrientation = CreateQuaternionForBoneData(position, vector);
				}
				boneData.ToPosition = (boneData.FromPosition = boneData.OriginalJoint.position) + (vector - position);
			}
		}

		private Vector3 FixJointPairEndPositionHand(Vector3 jointPairEndPosition, HumanBodyBones humanBodyBone)
		{
			Vector3 result = jointPairEndPosition;
			if (humanBodyBone == HumanBodyBones.LeftHand && BodyToBoneData.ContainsKey(HumanBodyBones.LeftThumbProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.LeftIndexProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.LeftMiddleProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.LeftRingProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.LeftLittleProximal))
			{
				Vector3 position = BodyToBoneData[HumanBodyBones.LeftThumbProximal].OriginalJoint.position;
				Vector3 position2 = BodyToBoneData[HumanBodyBones.LeftIndexProximal].OriginalJoint.position;
				Vector3 position3 = BodyToBoneData[HumanBodyBones.LeftMiddleProximal].OriginalJoint.position;
				Vector3 position4 = BodyToBoneData[HumanBodyBones.LeftRingProximal].OriginalJoint.position;
				Vector3 position5 = BodyToBoneData[HumanBodyBones.LeftLittleProximal].OriginalJoint.position;
				result = (position + position2 + position3 + position4 + position5) / 5f;
			}
			if (humanBodyBone == HumanBodyBones.RightHand && BodyToBoneData.ContainsKey(HumanBodyBones.RightThumbProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.RightIndexProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.RightMiddleProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.RightRingProximal) && BodyToBoneData.ContainsKey(HumanBodyBones.RightLittleProximal))
			{
				Vector3 position6 = BodyToBoneData[HumanBodyBones.RightThumbProximal].OriginalJoint.position;
				Vector3 position7 = BodyToBoneData[HumanBodyBones.RightIndexProximal].OriginalJoint.position;
				Vector3 position8 = BodyToBoneData[HumanBodyBones.RightMiddleProximal].OriginalJoint.position;
				Vector3 position9 = BodyToBoneData[HumanBodyBones.RightRingProximal].OriginalJoint.position;
				Vector3 position10 = BodyToBoneData[HumanBodyBones.RightLittleProximal].OriginalJoint.position;
				result = (position6 + position7 + position8 + position9 + position10) / 5f;
			}
			return result;
		}

		private static Transform FindFirstChild(Transform startTransform, Transform currTransform)
		{
			if (startTransform != currTransform)
			{
				return currTransform;
			}
			if (currTransform.childCount == 0)
			{
				return null;
			}
			Transform result = null;
			for (int i = 0; i < currTransform.childCount; i++)
			{
				Transform transform = FindFirstChild(startTransform, currTransform.GetChild(i));
				if (transform != null)
				{
					result = transform;
					break;
				}
			}
			return result;
		}

		private static Quaternion CreateQuaternionForBoneDataWithRightVec(Vector3 fromPosition, Vector3 toPosition, Vector3 rightVector)
		{
			Vector3 vector = (toPosition - fromPosition).normalized;
			if (vector.sqrMagnitude < Mathf.Epsilon)
			{
				vector = Vector3.forward;
			}
			Vector3 upwards = Vector3.Cross(vector, rightVector);
			return Quaternion.LookRotation(vector, upwards);
		}

		private static Quaternion CreateQuaternionForBoneData(Vector3 fromPosition, Vector3 toPosition)
		{
			Vector3 forward = (toPosition - fromPosition).normalized;
			if (forward.sqrMagnitude < Mathf.Epsilon)
			{
				forward = Vector3.forward;
			}
			return Quaternion.LookRotation(forward);
		}
	}

	[Serializable]
	public class JointAdjustment
	{
		public HumanBodyBones Joint;

		public Vector3 PositionChange = Vector3.zero;

		public Quaternion RotationChange = Quaternion.identity;

		public Quaternion[] RotationTweaks;

		public bool DisableRotationTransform;

		public bool DisablePositionTransform;

		public OVRHumanBodyBonesMappings.FullBodyTrackingBoneId FullBodyBoneIdOverrideValue = OVRHumanBodyBonesMappings.FullBodyTrackingBoneId.NoOverride;

		public OVRHumanBodyBonesMappings.BodyTrackingBoneId BoneIdOverrideValue = OVRHumanBodyBonesMappings.BodyTrackingBoneId.NoOverride;

		public Quaternion PrecomputedRotationTweaks { get; private set; }

		public void PrecomputeRotationTweaks()
		{
			PrecomputedRotationTweaks = Quaternion.identity;
			if (RotationTweaks == null || RotationTweaks.Length == 0)
			{
				return;
			}
			Quaternion[] rotationTweaks = RotationTweaks;
			for (int i = 0; i < rotationTweaks.Length; i++)
			{
				Quaternion quaternion = rotationTweaks[i];
				if (!(quaternion.w < Mathf.Epsilon) || !(quaternion.x < Mathf.Epsilon) || !(quaternion.y < Mathf.Epsilon) || !(quaternion.z < Mathf.Epsilon))
				{
					PrecomputedRotationTweaks *= quaternion;
				}
			}
		}
	}

	public enum UpdateType
	{
		FixedUpdateOnly,
		UpdateOnly,
		FixedUpdateAndUpdate
	}

	private OVRSkeletonMetadata _sourceSkeletonData;

	private OVRSkeletonMetadata _sourceSkeletonTPoseData;

	private OVRSkeletonMetadata _targetSkeletonData;

	private Animator _animatorTargetSkeleton;

	private Dictionary<BoneId, HumanBodyBones> _customBoneIdToHumanBodyBone = new Dictionary<BoneId, HumanBodyBones>();

	private readonly Dictionary<HumanBodyBones, Quaternion> _targetTPoseRotations = new Dictionary<HumanBodyBones, Quaternion>();

	private Dictionary<HumanBodyBones, Transform> _targetTPoseTransformDup = new Dictionary<HumanBodyBones, Transform>();

	private int _lastSkelChangeCount = -1;

	private Vector3 _lastTrackedScale;

	[SerializeField]
	protected JointAdjustment[] _adjustments = new JointAdjustment[1]
	{
		new JointAdjustment
		{
			Joint = HumanBodyBones.Hips,
			RotationChange = Quaternion.Euler(60f, 0f, 0f)
		}
	};

	[SerializeField]
	protected OVRHumanBodyBonesMappings.BodySection[] _fullBodySectionsToAlign = new OVRHumanBodyBonesMappings.BodySection[12]
	{
		OVRHumanBodyBonesMappings.BodySection.LeftArm,
		OVRHumanBodyBonesMappings.BodySection.RightArm,
		OVRHumanBodyBonesMappings.BodySection.LeftHand,
		OVRHumanBodyBonesMappings.BodySection.RightHand,
		OVRHumanBodyBonesMappings.BodySection.Hips,
		OVRHumanBodyBonesMappings.BodySection.Back,
		OVRHumanBodyBonesMappings.BodySection.Neck,
		OVRHumanBodyBonesMappings.BodySection.Head,
		OVRHumanBodyBonesMappings.BodySection.LeftLeg,
		OVRHumanBodyBonesMappings.BodySection.LeftFoot,
		OVRHumanBodyBonesMappings.BodySection.RightLeg,
		OVRHumanBodyBonesMappings.BodySection.RightFoot
	};

	[SerializeField]
	protected OVRHumanBodyBonesMappings.BodySection[] _bodySectionsToAlign = new OVRHumanBodyBonesMappings.BodySection[8]
	{
		OVRHumanBodyBonesMappings.BodySection.LeftArm,
		OVRHumanBodyBonesMappings.BodySection.RightArm,
		OVRHumanBodyBonesMappings.BodySection.LeftHand,
		OVRHumanBodyBonesMappings.BodySection.RightHand,
		OVRHumanBodyBonesMappings.BodySection.Hips,
		OVRHumanBodyBonesMappings.BodySection.Back,
		OVRHumanBodyBonesMappings.BodySection.Neck,
		OVRHumanBodyBonesMappings.BodySection.Head
	};

	[SerializeField]
	protected OVRHumanBodyBonesMappings.BodySection[] _fullBodySectionToPosition = new OVRHumanBodyBonesMappings.BodySection[11]
	{
		OVRHumanBodyBonesMappings.BodySection.LeftArm,
		OVRHumanBodyBonesMappings.BodySection.RightArm,
		OVRHumanBodyBonesMappings.BodySection.LeftHand,
		OVRHumanBodyBonesMappings.BodySection.RightHand,
		OVRHumanBodyBonesMappings.BodySection.Hips,
		OVRHumanBodyBonesMappings.BodySection.Neck,
		OVRHumanBodyBonesMappings.BodySection.Head,
		OVRHumanBodyBonesMappings.BodySection.LeftLeg,
		OVRHumanBodyBonesMappings.BodySection.LeftFoot,
		OVRHumanBodyBonesMappings.BodySection.RightLeg,
		OVRHumanBodyBonesMappings.BodySection.RightFoot
	};

	[SerializeField]
	protected OVRHumanBodyBonesMappings.BodySection[] _bodySectionToPosition = new OVRHumanBodyBonesMappings.BodySection[7]
	{
		OVRHumanBodyBonesMappings.BodySection.LeftArm,
		OVRHumanBodyBonesMappings.BodySection.RightArm,
		OVRHumanBodyBonesMappings.BodySection.LeftHand,
		OVRHumanBodyBonesMappings.BodySection.RightHand,
		OVRHumanBodyBonesMappings.BodySection.Hips,
		OVRHumanBodyBonesMappings.BodySection.Neck,
		OVRHumanBodyBonesMappings.BodySection.Head
	};

	[SerializeField]
	[Tooltip("Controls if we run retargeting from FixedUpdate, Update, or both.")]
	protected UpdateType _updateType = UpdateType.UpdateOnly;

	private OVRHumanBodyBonesMappingsInterface _bodyBonesMappingInterface = new OVRHumanBodyBonesMappings();

	protected OVRSkeletonMetadata SourceSkeletonData => _sourceSkeletonData;

	protected OVRSkeletonMetadata SourceSkeletonTPoseData => _sourceSkeletonTPoseData;

	protected OVRSkeletonMetadata TargetSkeletonData => _targetSkeletonData;

	protected Animator AnimatorTargetSkeleton => _animatorTargetSkeleton;

	protected Dictionary<BoneId, HumanBodyBones> CustomBoneIdToHumanBodyBone => _customBoneIdToHumanBodyBone;

	protected Dictionary<HumanBodyBones, Quaternion> TargetTPoseRotations => _targetTPoseRotations;

	protected JointAdjustment[] Adjustments => _adjustments;

	protected OVRHumanBodyBonesMappings.BodySection[] FullBodySectionsToAlign => _fullBodySectionsToAlign;

	protected OVRHumanBodyBonesMappings.BodySection[] BodySectionsToAlign => _bodySectionsToAlign;

	protected OVRHumanBodyBonesMappings.BodySection[] FullBodySectionToPosition => _fullBodySectionToPosition;

	protected OVRHumanBodyBonesMappings.BodySection[] BodySectionToPosition => _bodySectionToPosition;

	public OVRHumanBodyBonesMappingsInterface BodyBoneMappingsInterface
	{
		get
		{
			return _bodyBonesMappingInterface;
		}
		set
		{
			_bodyBonesMappingInterface = value;
		}
	}

	public OVRUnityHumanoidSkeletonRetargeter()
	{
		_skeletonType = SkeletonType.Body;
	}

	protected override void Start()
	{
		base.Start();
		_lastTrackedScale = base.transform.lossyScale;
		ValidateGameObjectForUnityHumanoidRetargeting(base.gameObject);
		_animatorTargetSkeleton = base.gameObject.GetComponent<Animator>();
		CreateCustomBoneIdToHumanBodyBoneMapping();
		StoreTTargetPoseRotations();
		_targetSkeletonData = new OVRSkeletonMetadata(_animatorTargetSkeleton, _bodyBonesMappingInterface);
		_targetSkeletonData.BuildCoordinateAxesForAllBones();
		PrecomputeAllRotationTweaks();
	}

	private void PrecomputeAllRotationTweaks()
	{
		if (_adjustments != null && _adjustments.Length != 0)
		{
			JointAdjustment[] adjustments = _adjustments;
			for (int i = 0; i < adjustments.Length; i++)
			{
				adjustments[i].PrecomputeRotationTweaks();
			}
		}
	}

	protected virtual void OnValidate()
	{
		PrecomputeAllRotationTweaks();
	}

	internal static void ValidateGameObjectForUnityHumanoidRetargeting(GameObject go)
	{
		if (go.GetComponent<Animator>() == null)
		{
			throw new InvalidOperationException("Retargeting to Unity Humanoid requires an Animator component with a humanoid avatar on T-Pose");
		}
	}

	private void StoreTTargetPoseRotations()
	{
		for (HumanBodyBones humanBodyBones = HumanBodyBones.Hips; humanBodyBones < HumanBodyBones.LastBone; humanBodyBones++)
		{
			Transform boneTransform = _animatorTargetSkeleton.GetBoneTransform(humanBodyBones);
			_targetTPoseRotations[humanBodyBones] = (boneTransform ? boneTransform.rotation : Quaternion.identity);
		}
		Transform obj = CreateDuplicateTransformHierarchy(_animatorTargetSkeleton.GetBoneTransform(HumanBodyBones.Hips));
		obj.name = base.name + "-tPose";
		obj.SetParent(base.transform, worldPositionStays: false);
	}

	private Transform CreateDuplicateTransformHierarchy(Transform transformFromOriginalHierarchy)
	{
		Transform transform = new GameObject(transformFromOriginalHierarchy.name + "-tPose").transform;
		transform.localPosition = transformFromOriginalHierarchy.localPosition;
		transform.localRotation = transformFromOriginalHierarchy.localRotation;
		transform.localScale = transformFromOriginalHierarchy.localScale;
		HumanBodyBones humanBodyBones = FindHumanBodyBoneFromTransform(transformFromOriginalHierarchy);
		if (humanBodyBones != HumanBodyBones.LastBone)
		{
			_targetTPoseTransformDup[humanBodyBones] = transform;
		}
		foreach (Transform item in transformFromOriginalHierarchy)
		{
			CreateDuplicateTransformHierarchy(item).SetParent(transform, worldPositionStays: false);
		}
		return transform;
	}

	private HumanBodyBones FindHumanBodyBoneFromTransform(Transform candidateTransform)
	{
		for (HumanBodyBones humanBodyBones = HumanBodyBones.Hips; humanBodyBones < HumanBodyBones.LastBone; humanBodyBones++)
		{
			if (_animatorTargetSkeleton.GetBoneTransform(humanBodyBones) == candidateTransform)
			{
				return humanBodyBones;
			}
		}
		return HumanBodyBones.LastBone;
	}

	private void AlignHierarchies(Transform transformToAlign, Transform referenceTransform)
	{
		transformToAlign.localRotation = referenceTransform.localRotation;
		transformToAlign.localPosition = referenceTransform.localPosition;
		transformToAlign.localScale = referenceTransform.localScale;
		for (int i = 0; i < referenceTransform.childCount; i++)
		{
			AlignHierarchies(transformToAlign.GetChild(i), referenceTransform.GetChild(i));
		}
	}

	private void CreateCustomBoneIdToHumanBodyBoneMapping()
	{
		CopyBoneIdToHumanBodyBoneMapping();
		AdjustCustomBoneIdToHumanBodyBoneMapping();
	}

	private void CopyBoneIdToHumanBodyBoneMapping()
	{
		_customBoneIdToHumanBodyBone.Clear();
		if (_skeletonType == SkeletonType.FullBody)
		{
			foreach (KeyValuePair<BoneId, HumanBodyBones> item in _bodyBonesMappingInterface.GetFullBodyBoneIdToHumanBodyBone)
			{
				_customBoneIdToHumanBodyBone.Add(item.Key, item.Value);
			}
			return;
		}
		foreach (KeyValuePair<BoneId, HumanBodyBones> item2 in _bodyBonesMappingInterface.GetBoneIdToHumanBodyBone)
		{
			_customBoneIdToHumanBodyBone.Add(item2.Key, item2.Value);
		}
	}

	private void AdjustCustomBoneIdToHumanBodyBoneMapping()
	{
		JointAdjustment[] adjustments = _adjustments;
		foreach (JointAdjustment jointAdjustment in adjustments)
		{
			bool flag = _skeletonType == SkeletonType.FullBody;
			if ((!flag || jointAdjustment.FullBodyBoneIdOverrideValue != OVRHumanBodyBonesMappings.FullBodyTrackingBoneId.NoOverride) && jointAdjustment.BoneIdOverrideValue != OVRHumanBodyBonesMappings.BodyTrackingBoneId.NoOverride)
			{
				if ((flag && jointAdjustment.FullBodyBoneIdOverrideValue == OVRHumanBodyBonesMappings.FullBodyTrackingBoneId.Remove) || jointAdjustment.BoneIdOverrideValue == OVRHumanBodyBonesMappings.BodyTrackingBoneId.Remove)
				{
					RemoveMappingCorrespondingToHumanBodyBone(jointAdjustment.Joint);
				}
				else if (flag)
				{
					_customBoneIdToHumanBodyBone[(BoneId)jointAdjustment.FullBodyBoneIdOverrideValue] = jointAdjustment.Joint;
				}
				else
				{
					_customBoneIdToHumanBodyBone[(BoneId)jointAdjustment.BoneIdOverrideValue] = jointAdjustment.Joint;
				}
			}
		}
	}

	private void RemoveMappingCorrespondingToHumanBodyBone(HumanBodyBones boneId)
	{
		foreach (BoneId key in _customBoneIdToHumanBodyBone.Keys)
		{
			if (_customBoneIdToHumanBodyBone[key] == boneId)
			{
				_customBoneIdToHumanBodyBone.Remove(key);
				break;
			}
		}
	}

	protected override void Update()
	{
		if (ShouldRunUpdateThisFrame())
		{
			UpdateSkeleton();
			RecomputeSkeletalOffsetsIfNecessary();
			AlignTargetWithSource();
		}
	}

	protected bool ShouldRunUpdateThisFrame()
	{
		bool inFixedTimeStep = Time.inFixedTimeStep;
		return _updateType switch
		{
			UpdateType.FixedUpdateOnly => inFixedTimeStep, 
			UpdateType.UpdateOnly => !inFixedTimeStep, 
			_ => true, 
		};
	}

	protected void RecomputeSkeletalOffsetsIfNecessary()
	{
		if (OffsetComputationNeededThisFrame())
		{
			ComputeOffsetsUsingSkeletonComponent();
		}
	}

	protected bool OffsetComputationNeededThisFrame()
	{
		if (!base.IsInitialized || base.BindPoses == null || base.BindPoses.Count == 0)
		{
			return false;
		}
		bool num = _lastSkelChangeCount != base.SkeletonChangedCount;
		bool flag = (base.transform.lossyScale - _lastTrackedScale).sqrMagnitude > Mathf.Epsilon;
		return num || flag;
	}

	protected void ComputeOffsetsUsingSkeletonComponent()
	{
		if (!base.IsInitialized || base.BindPoses == null || base.BindPoses.Count == 0)
		{
			return;
		}
		if (_sourceSkeletonData == null)
		{
			_sourceSkeletonData = new OVRSkeletonMetadata(this, useBindPose: false, _customBoneIdToHumanBodyBone, _skeletonType == SkeletonType.FullBody, _bodyBonesMappingInterface);
		}
		else if (_skeletonType == SkeletonType.FullBody)
		{
			_sourceSkeletonData.BuildBoneDataSkeletonFullBody(this, useBindPose: false, _customBoneIdToHumanBodyBone, _bodyBonesMappingInterface);
		}
		else
		{
			_sourceSkeletonData.BuildBoneDataSkeleton(this, useBindPose: false, _customBoneIdToHumanBodyBone, _bodyBonesMappingInterface);
		}
		_sourceSkeletonData.BuildCoordinateAxesForAllBones();
		if (_sourceSkeletonTPoseData == null)
		{
			_sourceSkeletonTPoseData = new OVRSkeletonMetadata(this, useBindPose: true, _customBoneIdToHumanBodyBone, _skeletonType == SkeletonType.FullBody, _bodyBonesMappingInterface);
		}
		else if (_skeletonType == SkeletonType.FullBody)
		{
			_sourceSkeletonTPoseData.BuildBoneDataSkeletonFullBody(this, useBindPose: true, _customBoneIdToHumanBodyBone, _bodyBonesMappingInterface);
		}
		else
		{
			_sourceSkeletonTPoseData.BuildBoneDataSkeleton(this, useBindPose: true, _customBoneIdToHumanBodyBone, _bodyBonesMappingInterface);
		}
		_sourceSkeletonTPoseData.BuildCoordinateAxesForAllBones();
		AlignHierarchies(_animatorTargetSkeleton.GetBoneTransform(HumanBodyBones.Hips), _targetTPoseTransformDup[HumanBodyBones.Hips]);
		_targetSkeletonData.BuildCoordinateAxesForAllBones();
		for (int i = 0; i < base.BindPoses.Count; i++)
		{
			if (_customBoneIdToHumanBodyBone.TryGetValue(base.BindPoses[i].Id, out var value) && _targetSkeletonData.BodyToBoneData.TryGetValue(value, out var value2) && IsBodySectionInArray(_bodyBonesMappingInterface.GetBoneToBodySection[value], (_skeletonType == SkeletonType.FullBody) ? _fullBodySectionsToAlign : _bodySectionsToAlign) && _sourceSkeletonTPoseData.BodyToBoneData.TryGetValue(value, out var value3) && _sourceSkeletonData.BodyToBoneData.TryGetValue(value, out var value4))
			{
				if (value3.DegenerateJoint || value4.DegenerateJoint)
				{
					value2.CorrectionQuaternion = null;
					continue;
				}
				Vector3 toDirection = value3.JointPairOrientation * Vector3.forward;
				Quaternion quaternion = Quaternion.FromToRotation(value2.JointPairOrientation * Vector3.forward, toDirection);
				Quaternion quaternion2 = Quaternion.Inverse(base.BindPoses[i].Transform.rotation);
				value2.CorrectionQuaternion = quaternion2 * quaternion * _animatorTargetSkeleton.GetBoneTransform(value).rotation;
			}
		}
		_lastSkelChangeCount = base.SkeletonChangedCount;
		_lastTrackedScale = base.transform.lossyScale;
	}

	protected static bool IsBodySectionInArray(OVRHumanBodyBonesMappings.BodySection bodySectionToCheck, OVRHumanBodyBonesMappings.BodySection[] sectionArrayToCheck)
	{
		for (int i = 0; i < sectionArrayToCheck.Length; i++)
		{
			if (sectionArrayToCheck[i] == bodySectionToCheck)
			{
				return true;
			}
		}
		return false;
	}

	private void AlignTargetWithSource()
	{
		if (!base.IsInitialized || base.Bones == null || base.Bones.Count == 0)
		{
			return;
		}
		for (int i = 0; i < base.Bones.Count; i++)
		{
			if (!_customBoneIdToHumanBodyBone.TryGetValue(base.Bones[i].Id, out var value) || !_targetSkeletonData.BodyToBoneData.TryGetValue(value, out var value2) || !value2.CorrectionQuaternion.HasValue)
			{
				continue;
			}
			Transform originalJoint = value2.OriginalJoint;
			Quaternion value3 = value2.CorrectionQuaternion.Value;
			JointAdjustment jointAdjustment = FindAdjustment(value);
			bool flag = IsBodySectionInArray(_bodyBonesMappingInterface.GetBoneToBodySection[value], (_skeletonType == SkeletonType.FullBody) ? _fullBodySectionToPosition : _bodySectionToPosition);
			if (jointAdjustment == null)
			{
				originalJoint.rotation = base.Bones[i].Transform.rotation * value3;
				if (flag)
				{
					originalJoint.position = base.Bones[i].Transform.position;
				}
				continue;
			}
			if (!jointAdjustment.DisableRotationTransform)
			{
				originalJoint.rotation = base.Bones[i].Transform.rotation * value3;
			}
			originalJoint.rotation *= jointAdjustment.RotationChange;
			originalJoint.rotation *= jointAdjustment.PrecomputedRotationTweaks;
			if (!jointAdjustment.DisablePositionTransform && flag)
			{
				originalJoint.position = base.Bones[i].Transform.position;
			}
			originalJoint.position += jointAdjustment.PositionChange;
		}
	}

	protected JointAdjustment FindAdjustment(HumanBodyBones boneId)
	{
		JointAdjustment[] adjustments = _adjustments;
		foreach (JointAdjustment jointAdjustment in adjustments)
		{
			if (jointAdjustment.Joint == boneId)
			{
				return jointAdjustment;
			}
		}
		return null;
	}
}
