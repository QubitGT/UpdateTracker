using System;
using System.Collections.Generic;
using Meta.XR.Util;

namespace Oculus.Interaction.Body.Input;

[Feature(Feature.Interaction)]
public class OVRSkeletonMapping : BodySkeletonMapping<OVRPlugin.BoneId>, ISkeletonMapping
{
	private static readonly Dictionary<BodyJointId, JointInfo> _upperBodyJoints = new Dictionary<BodyJointId, JointInfo>
	{
		[BodyJointId.Body_Start] = new JointInfo(OVRPlugin.BoneId.Hand_Start, OVRPlugin.BoneId.Hand_Start),
		[BodyJointId.Body_Hips] = new JointInfo(OVRPlugin.BoneId.Hand_ForearmStub, OVRPlugin.BoneId.Hand_Start),
		[BodyJointId.Body_SpineLower] = new JointInfo(OVRPlugin.BoneId.Hand_Thumb0, OVRPlugin.BoneId.Hand_ForearmStub),
		[BodyJointId.Body_SpineMiddle] = new JointInfo(OVRPlugin.BoneId.Hand_Thumb1, OVRPlugin.BoneId.Hand_Thumb0),
		[BodyJointId.Body_SpineUpper] = new JointInfo(OVRPlugin.BoneId.Hand_Thumb2, OVRPlugin.BoneId.Hand_Thumb1),
		[BodyJointId.Body_Chest] = new JointInfo(OVRPlugin.BoneId.Hand_Thumb3, OVRPlugin.BoneId.Hand_Thumb2),
		[BodyJointId.Body_Neck] = new JointInfo(OVRPlugin.BoneId.Hand_Index1, OVRPlugin.BoneId.Hand_Thumb3),
		[BodyJointId.Body_Head] = new JointInfo(OVRPlugin.BoneId.Hand_Index2, OVRPlugin.BoneId.Hand_Index1),
		[BodyJointId.Body_LeftShoulder] = new JointInfo(OVRPlugin.BoneId.Hand_Index3, OVRPlugin.BoneId.Hand_Thumb3),
		[BodyJointId.Body_LeftScapula] = new JointInfo(OVRPlugin.BoneId.Hand_Middle1, OVRPlugin.BoneId.Hand_Index3),
		[BodyJointId.Body_LeftArmUpper] = new JointInfo(OVRPlugin.BoneId.Hand_Middle2, OVRPlugin.BoneId.Hand_Middle1),
		[BodyJointId.Body_LeftArmLower] = new JointInfo(OVRPlugin.BoneId.Hand_Middle3, OVRPlugin.BoneId.Hand_Middle2),
		[BodyJointId.Body_LeftHandWristTwist] = new JointInfo(OVRPlugin.BoneId.Hand_Ring1, OVRPlugin.BoneId.Hand_Middle3),
		[BodyJointId.Body_RightShoulder] = new JointInfo(OVRPlugin.BoneId.Hand_Ring2, OVRPlugin.BoneId.Hand_Thumb3),
		[BodyJointId.Body_RightScapula] = new JointInfo(OVRPlugin.BoneId.Hand_Ring3, OVRPlugin.BoneId.Hand_Ring2),
		[BodyJointId.Body_RightArmUpper] = new JointInfo(OVRPlugin.BoneId.Hand_Pinky0, OVRPlugin.BoneId.Hand_Ring3),
		[BodyJointId.Body_RightArmLower] = new JointInfo(OVRPlugin.BoneId.Hand_Pinky1, OVRPlugin.BoneId.Hand_Pinky0),
		[BodyJointId.Body_RightHandWristTwist] = new JointInfo(OVRPlugin.BoneId.Hand_Pinky2, OVRPlugin.BoneId.Hand_Pinky1),
		[BodyJointId.Body_LeftHandPalm] = new JointInfo(OVRPlugin.BoneId.Hand_Pinky3, OVRPlugin.BoneId.Hand_MaxSkinnable),
		[BodyJointId.Body_LeftHandWrist] = new JointInfo(OVRPlugin.BoneId.Hand_MaxSkinnable, OVRPlugin.BoneId.Hand_Middle3),
		[BodyJointId.Body_LeftHandThumbMetacarpal] = new JointInfo(OVRPlugin.BoneId.Hand_IndexTip, OVRPlugin.BoneId.Hand_MaxSkinnable),
		[BodyJointId.Body_LeftHandThumbProximal] = new JointInfo(OVRPlugin.BoneId.Hand_MiddleTip, OVRPlugin.BoneId.Hand_IndexTip),
		[BodyJointId.Body_LeftHandThumbDistal] = new JointInfo(OVRPlugin.BoneId.Hand_RingTip, OVRPlugin.BoneId.Hand_MiddleTip),
		[BodyJointId.Body_LeftHandThumbTip] = new JointInfo(OVRPlugin.BoneId.Hand_PinkyTip, OVRPlugin.BoneId.Hand_RingTip),
		[BodyJointId.Body_LeftHandIndexMetacarpal] = new JointInfo(OVRPlugin.BoneId.Hand_End, OVRPlugin.BoneId.Hand_MaxSkinnable),
		[BodyJointId.Body_LeftHandIndexProximal] = new JointInfo(OVRPlugin.BoneId.XRHand_LittleTip, OVRPlugin.BoneId.Hand_End),
		[BodyJointId.Body_LeftHandIndexIntermediate] = new JointInfo(OVRPlugin.BoneId.XRHand_Max, OVRPlugin.BoneId.XRHand_LittleTip),
		[BodyJointId.Body_LeftHandIndexDistal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandIndexDistal, OVRPlugin.BoneId.XRHand_Max),
		[BodyJointId.Body_LeftHandIndexTip] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandIndexTip, OVRPlugin.BoneId.Body_LeftHandIndexDistal),
		[BodyJointId.Body_LeftHandMiddleMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandMiddleMetacarpal, OVRPlugin.BoneId.Hand_MaxSkinnable),
		[BodyJointId.Body_LeftHandMiddleProximal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandMiddleProximal, OVRPlugin.BoneId.Body_LeftHandMiddleMetacarpal),
		[BodyJointId.Body_LeftHandMiddleIntermediate] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandMiddleIntermediate, OVRPlugin.BoneId.Body_LeftHandMiddleProximal),
		[BodyJointId.Body_LeftHandMiddleDistal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandMiddleDistal, OVRPlugin.BoneId.Body_LeftHandMiddleIntermediate),
		[BodyJointId.Body_LeftHandMiddleTip] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandMiddleTip, OVRPlugin.BoneId.Body_LeftHandMiddleDistal),
		[BodyJointId.Body_LeftHandRingMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandRingMetacarpal, OVRPlugin.BoneId.Hand_MaxSkinnable),
		[BodyJointId.Body_LeftHandRingProximal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandRingProximal, OVRPlugin.BoneId.Body_LeftHandRingMetacarpal),
		[BodyJointId.Body_LeftHandRingIntermediate] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandRingIntermediate, OVRPlugin.BoneId.Body_LeftHandRingProximal),
		[BodyJointId.Body_LeftHandRingDistal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandRingDistal, OVRPlugin.BoneId.Body_LeftHandRingIntermediate),
		[BodyJointId.Body_LeftHandRingTip] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandRingTip, OVRPlugin.BoneId.Body_LeftHandRingDistal),
		[BodyJointId.Body_LeftHandLittleMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandLittleMetacarpal, OVRPlugin.BoneId.Hand_MaxSkinnable),
		[BodyJointId.Body_LeftHandLittleProximal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandLittleProximal, OVRPlugin.BoneId.Body_LeftHandLittleMetacarpal),
		[BodyJointId.Body_LeftHandLittleIntermediate] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandLittleIntermediate, OVRPlugin.BoneId.Body_LeftHandLittleProximal),
		[BodyJointId.Body_LeftHandLittleDistal] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandLittleDistal, OVRPlugin.BoneId.Body_LeftHandLittleIntermediate),
		[BodyJointId.Body_LeftHandLittleTip] = new JointInfo(OVRPlugin.BoneId.Body_LeftHandLittleTip, OVRPlugin.BoneId.Body_LeftHandLittleDistal),
		[BodyJointId.Body_RightHandPalm] = new JointInfo(OVRPlugin.BoneId.Body_RightHandPalm, OVRPlugin.BoneId.Body_RightHandWrist),
		[BodyJointId.Body_RightHandWrist] = new JointInfo(OVRPlugin.BoneId.Body_RightHandWrist, OVRPlugin.BoneId.Hand_Pinky1),
		[BodyJointId.Body_RightHandThumbMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandThumbMetacarpal, OVRPlugin.BoneId.Body_RightHandWrist),
		[BodyJointId.Body_RightHandThumbProximal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandThumbProximal, OVRPlugin.BoneId.Body_RightHandThumbMetacarpal),
		[BodyJointId.Body_RightHandThumbDistal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandThumbDistal, OVRPlugin.BoneId.Body_RightHandThumbProximal),
		[BodyJointId.Body_RightHandThumbTip] = new JointInfo(OVRPlugin.BoneId.Body_RightHandThumbTip, OVRPlugin.BoneId.Body_RightHandThumbDistal),
		[BodyJointId.Body_RightHandIndexMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandIndexMetacarpal, OVRPlugin.BoneId.Body_RightHandWrist),
		[BodyJointId.Body_RightHandIndexProximal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandIndexProximal, OVRPlugin.BoneId.Body_RightHandIndexMetacarpal),
		[BodyJointId.Body_RightHandIndexIntermediate] = new JointInfo(OVRPlugin.BoneId.Body_RightHandIndexIntermediate, OVRPlugin.BoneId.Body_RightHandIndexProximal),
		[BodyJointId.Body_RightHandIndexDistal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandIndexDistal, OVRPlugin.BoneId.Body_RightHandIndexIntermediate),
		[BodyJointId.Body_RightHandIndexTip] = new JointInfo(OVRPlugin.BoneId.Body_RightHandIndexTip, OVRPlugin.BoneId.Body_RightHandIndexDistal),
		[BodyJointId.Body_RightHandMiddleMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandMiddleMetacarpal, OVRPlugin.BoneId.Body_RightHandWrist),
		[BodyJointId.Body_RightHandMiddleProximal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandMiddleProximal, OVRPlugin.BoneId.Body_RightHandMiddleMetacarpal),
		[BodyJointId.Body_RightHandMiddleIntermediate] = new JointInfo(OVRPlugin.BoneId.Body_RightHandMiddleIntermediate, OVRPlugin.BoneId.Body_RightHandMiddleProximal),
		[BodyJointId.Body_RightHandMiddleDistal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandMiddleDistal, OVRPlugin.BoneId.Body_RightHandMiddleIntermediate),
		[BodyJointId.Body_RightHandMiddleTip] = new JointInfo(OVRPlugin.BoneId.Body_RightHandMiddleTip, OVRPlugin.BoneId.Body_RightHandMiddleDistal),
		[BodyJointId.Body_RightHandRingMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandRingMetacarpal, OVRPlugin.BoneId.Body_RightHandWrist),
		[BodyJointId.Body_RightHandRingProximal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandRingProximal, OVRPlugin.BoneId.Body_RightHandRingMetacarpal),
		[BodyJointId.Body_RightHandRingIntermediate] = new JointInfo(OVRPlugin.BoneId.Body_RightHandRingIntermediate, OVRPlugin.BoneId.Body_RightHandRingProximal),
		[BodyJointId.Body_RightHandRingDistal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandRingDistal, OVRPlugin.BoneId.Body_RightHandRingIntermediate),
		[BodyJointId.Body_RightHandRingTip] = new JointInfo(OVRPlugin.BoneId.Body_RightHandRingTip, OVRPlugin.BoneId.Body_RightHandRingDistal),
		[BodyJointId.Body_RightHandLittleMetacarpal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandLittleMetacarpal, OVRPlugin.BoneId.Body_RightHandWrist),
		[BodyJointId.Body_RightHandLittleProximal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandLittleProximal, OVRPlugin.BoneId.Body_RightHandLittleMetacarpal),
		[BodyJointId.Body_RightHandLittleIntermediate] = new JointInfo(OVRPlugin.BoneId.Body_RightHandLittleIntermediate, OVRPlugin.BoneId.Body_RightHandLittleProximal),
		[BodyJointId.Body_RightHandLittleDistal] = new JointInfo(OVRPlugin.BoneId.Body_RightHandLittleDistal, OVRPlugin.BoneId.Body_RightHandLittleIntermediate),
		[BodyJointId.Body_RightHandLittleTip] = new JointInfo(OVRPlugin.BoneId.Body_RightHandLittleTip, OVRPlugin.BoneId.Body_RightHandLittleDistal)
	};

	private static readonly Dictionary<BodyJointId, JointInfo> _lowerBodyJoints = new Dictionary<BodyJointId, JointInfo>
	{
		[BodyJointId.Body_LeftLegUpper] = new JointInfo(OVRPlugin.BoneId.Body_End, OVRPlugin.BoneId.Hand_ForearmStub),
		[BodyJointId.Body_LeftLegLower] = new JointInfo(OVRPlugin.BoneId.FullBody_LeftLowerLeg, OVRPlugin.BoneId.Body_End),
		[BodyJointId.Body_LeftFootAnkleTwist] = new JointInfo(OVRPlugin.BoneId.FullBody_LeftFootAnkleTwist, OVRPlugin.BoneId.FullBody_LeftLowerLeg),
		[BodyJointId.Body_LeftFootAnkle] = new JointInfo(OVRPlugin.BoneId.FullBody_LeftFootAnkle, OVRPlugin.BoneId.FullBody_LeftFootAnkleTwist),
		[BodyJointId.Body_LeftFootSubtalar] = new JointInfo(OVRPlugin.BoneId.FullBody_LeftFootSubtalar, OVRPlugin.BoneId.FullBody_LeftFootAnkle),
		[BodyJointId.Body_LeftFootTransverse] = new JointInfo(OVRPlugin.BoneId.FullBody_LeftFootTransverse, OVRPlugin.BoneId.FullBody_LeftFootSubtalar),
		[BodyJointId.Body_LeftFootBall] = new JointInfo(OVRPlugin.BoneId.FullBody_LeftFootBall, OVRPlugin.BoneId.FullBody_LeftFootTransverse),
		[BodyJointId.Body_RightLegUpper] = new JointInfo(OVRPlugin.BoneId.FullBody_RightUpperLeg, OVRPlugin.BoneId.Hand_ForearmStub),
		[BodyJointId.Body_RightLegLower] = new JointInfo(OVRPlugin.BoneId.FullBody_RightLowerLeg, OVRPlugin.BoneId.FullBody_RightUpperLeg),
		[BodyJointId.Body_RightFootAnkleTwist] = new JointInfo(OVRPlugin.BoneId.FullBody_RightFootAnkleTwist, OVRPlugin.BoneId.FullBody_RightLowerLeg),
		[BodyJointId.Body_RightFootAnkle] = new JointInfo(OVRPlugin.BoneId.FullBody_RightFootAnkle, OVRPlugin.BoneId.FullBody_RightFootAnkleTwist),
		[BodyJointId.Body_RightFootSubtalar] = new JointInfo(OVRPlugin.BoneId.FullBody_RightFootSubtalar, OVRPlugin.BoneId.FullBody_RightFootAnkle),
		[BodyJointId.Body_RightFootTransverse] = new JointInfo(OVRPlugin.BoneId.FullBody_RightFootTransverse, OVRPlugin.BoneId.FullBody_RightFootSubtalar),
		[BodyJointId.Body_RightFootBall] = new JointInfo(OVRPlugin.BoneId.FullBody_RightFootBall, OVRPlugin.BoneId.FullBody_RightFootTransverse)
	};

	[Obsolete("Use the parameterized constructor instead", true)]
	public OVRSkeletonMapping()
		: base(OVRPlugin.BoneId.Hand_Start, (IReadOnlyDictionary<BodyJointId, JointInfo>)_upperBodyJoints)
	{
	}

	public OVRSkeletonMapping(OVRPlugin.BodyJointSet skeletonType)
		: base(GetRoot(), GetJointMapping(skeletonType))
	{
	}

	private static IReadOnlyDictionary<BodyJointId, JointInfo> GetJointMapping(OVRPlugin.BodyJointSet jointSet)
	{
		Dictionary<BodyJointId, JointInfo> dictionary = new Dictionary<BodyJointId, JointInfo>();
		foreach (KeyValuePair<BodyJointId, JointInfo> upperBodyJoint in _upperBodyJoints)
		{
			dictionary.Add(upperBodyJoint.Key, upperBodyJoint.Value);
		}
		if (jointSet == OVRPlugin.BodyJointSet.FullBody)
		{
			foreach (KeyValuePair<BodyJointId, JointInfo> lowerBodyJoint in _lowerBodyJoints)
			{
				dictionary.Add(lowerBodyJoint.Key, lowerBodyJoint.Value);
			}
		}
		return dictionary;
	}

	private static OVRPlugin.BoneId GetRoot()
	{
		return OVRPlugin.BoneId.Hand_Start;
	}
}
