using System;
using System.Collections.Generic;
using UnityEngine;

public interface OVRHumanBodyBonesMappingsInterface
{
	Dictionary<HumanBodyBones, Tuple<HumanBodyBones, HumanBodyBones>> GetBoneToJointPair { get; }

	Dictionary<HumanBodyBones, OVRUnityHumanoidSkeletonRetargeter.OVRHumanBodyBonesMappings.BodySection> GetBoneToBodySection { get; }

	Dictionary<OVRSkeleton.BoneId, HumanBodyBones> GetFullBodyBoneIdToHumanBodyBone { get; }

	Dictionary<OVRSkeleton.BoneId, HumanBodyBones> GetBoneIdToHumanBodyBone { get; }

	Dictionary<OVRSkeleton.BoneId, Tuple<OVRSkeleton.BoneId, OVRSkeleton.BoneId>> GetFullBodyBoneIdToJointPair { get; }

	Dictionary<OVRSkeleton.BoneId, Tuple<OVRSkeleton.BoneId, OVRSkeleton.BoneId>> GetBoneIdToJointPair { get; }
}
