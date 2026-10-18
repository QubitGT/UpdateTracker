using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/move-samples/")]
[Feature(Feature.BodyTracking)]
public class OVRCustomSkeleton : OVRSkeleton, ISerializationCallbackReceiver
{
	public enum RetargetingType
	{
		OculusSkeleton
	}

	[HideInInspector]
	[SerializeField]
	private List<Transform> _customBones_V2;

	[SerializeField]
	[HideInInspector]
	internal RetargetingType retargetingType;

	public List<Transform> CustomBones => _customBones_V2;

	protected override Transform GetBoneTransform(BoneId boneId)
	{
		return _customBones_V2[(int)boneId];
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		AllocateBones();
	}

	private void AllocateBones()
	{
		int num = 84;
		if (_customBones_V2.Count != num)
		{
			while (_customBones_V2.Count < num)
			{
				_customBones_V2.Add(null);
			}
		}
	}

	internal override void SetSkeletonType(SkeletonType skeletonType)
	{
		base.SetSkeletonType(skeletonType);
		if (_customBones_V2 == null)
		{
			_customBones_V2 = new List<Transform>();
		}
		AllocateBones();
	}
}
