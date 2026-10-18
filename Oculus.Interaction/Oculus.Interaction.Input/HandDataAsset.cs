using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Serializable]
public class HandDataAsset : ICopyFrom<HandDataAsset>
{
	public bool IsDataValid;

	public bool IsConnected;

	public bool IsTracked;

	public Pose Root;

	public PoseOrigin RootPoseOrigin;

	public Pose[] JointPoses = new Pose[26];

	public float[] JointRadii = new float[26];

	[Obsolete("Deprecated. Use JointPoses instead.")]
	public Quaternion[] Joints = new Quaternion[26];

	public bool IsHighConfidence;

	public bool[] IsFingerPinching = new bool[5];

	public bool[] IsFingerHighConfidence = new bool[5];

	public float[] FingerPinchStrength = new float[5];

	public float HandScale;

	public Pose PointerPose;

	public PoseOrigin PointerPoseOrigin;

	public bool IsDominantHand;

	public HandDataSourceConfig Config = new HandDataSourceConfig();

	public bool IsDataValidAndConnected
	{
		get
		{
			if (IsDataValid)
			{
				return IsConnected;
			}
			return false;
		}
	}

	public void CopyFrom(HandDataAsset source)
	{
		IsDataValid = source.IsDataValid;
		IsConnected = source.IsConnected;
		IsTracked = source.IsTracked;
		IsHighConfidence = source.IsHighConfidence;
		IsDominantHand = source.IsDominantHand;
		Config = source.Config;
		CopyPosesFrom(source);
	}

	public void CopyPosesFrom(HandDataAsset source)
	{
		Root = source.Root;
		RootPoseOrigin = source.RootPoseOrigin;
		Array.Copy(source.JointPoses, JointPoses, 26);
		Array.Copy(source.JointRadii, JointRadii, source.JointRadii.Length);
		Array.Copy(source.Joints, Joints, 26);
		Array.Copy(source.IsFingerPinching, IsFingerPinching, IsFingerPinching.Length);
		Array.Copy(source.IsFingerHighConfidence, IsFingerHighConfidence, IsFingerHighConfidence.Length);
		Array.Copy(source.FingerPinchStrength, FingerPinchStrength, FingerPinchStrength.Length);
		HandScale = source.HandScale;
		PointerPose = source.PointerPose;
		PointerPoseOrigin = source.PointerPoseOrigin;
		Config = source.Config;
	}
}
