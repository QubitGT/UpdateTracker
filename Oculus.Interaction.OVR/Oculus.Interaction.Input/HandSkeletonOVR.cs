using System;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class HandSkeletonOVR : MonoBehaviour, IHandSkeletonProvider
{
	private readonly HandSkeleton[] _skeletons = new HandSkeleton[2]
	{
		new HandSkeleton(),
		new HandSkeleton()
	};

	public HandSkeleton this[Handedness handedness] => _skeletons[(int)handedness];

	protected void Awake()
	{
		ApplyToSkeleton(in OVRSkeletonData.LeftSkeleton, _skeletons[0]);
		ApplyToSkeleton(in OVRSkeletonData.RightSkeleton, _skeletons[1]);
	}

	public static HandSkeleton CreateSkeletonData(Handedness handedness)
	{
		HandSkeleton handSkeleton = new HandSkeleton();
		if (handedness == Handedness.Left)
		{
			ApplyToSkeleton(in OVRSkeletonData.LeftSkeleton, handSkeleton);
		}
		else
		{
			ApplyToSkeleton(in OVRSkeletonData.RightSkeleton, handSkeleton);
		}
		return handSkeleton;
	}

	private static void ApplyToSkeleton(in OVRPlugin.Skeleton2 ovrSkeleton, HandSkeleton handSkeleton)
	{
		int num = handSkeleton.joints.Length;
		for (int i = 0; i < num; i++)
		{
			ref OVRPlugin.Posef pose = ref ovrSkeleton.Bones[i].Pose;
			handSkeleton.joints[i] = new HandSkeletonJoint
			{
				pose = new Pose
				{
					position = pose.Position.FromFlippedZVector3f(),
					rotation = pose.Orientation.FromFlippedZQuatf()
				},
				parent = ovrSkeleton.Bones[i].ParentBoneIndex
			};
		}
	}

	internal static float GetBoneRadius(in OVRPlugin.Skeleton2 ovrSkeleton, int boneIndex)
	{
		if (boneIndex == 6)
		{
			boneIndex = 7;
		}
		else if (boneIndex == 11)
		{
			boneIndex = 12;
		}
		else if (boneIndex == 16)
		{
			boneIndex = 17;
		}
		else if (boneIndex == 21)
		{
			boneIndex = 22;
		}
		int num = Array.FindIndex(ovrSkeleton.BoneCapsules, (OVRPlugin.BoneCapsule c) => c.BoneIndex == boneIndex);
		if (num >= 0)
		{
			return ovrSkeleton.BoneCapsules[num].Radius;
		}
		return 0f;
	}
}
