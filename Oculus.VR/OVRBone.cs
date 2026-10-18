using System;
using UnityEngine;

public class OVRBone : IDisposable
{
	public OVRSkeleton.BoneId Id { get; set; }

	public short ParentBoneIndex { get; set; }

	public Transform Transform { get; set; }

	public OVRBone()
	{
	}

	public OVRBone(OVRSkeleton.BoneId id, short parentBoneIndex, Transform trans)
	{
		Id = id;
		ParentBoneIndex = parentBoneIndex;
		Transform = trans;
	}

	public void Dispose()
	{
		if (Transform != null)
		{
			UnityEngine.Object.Destroy(Transform.gameObject);
			Transform = null;
		}
	}
}
