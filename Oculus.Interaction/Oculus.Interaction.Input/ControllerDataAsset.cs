using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Serializable]
public class ControllerDataAsset : ICopyFrom<ControllerDataAsset>
{
	public bool IsDataValid;

	public bool IsConnected;

	public bool IsTracked;

	public ControllerInput Input;

	public Pose RootPose;

	public PoseOrigin RootPoseOrigin;

	public Pose PointerPose;

	public PoseOrigin PointerPoseOrigin;

	public bool IsDominantHand;

	public ControllerDataSourceConfig Config;

	public void CopyFrom(ControllerDataAsset source)
	{
		IsDataValid = source.IsDataValid;
		IsConnected = source.IsConnected;
		IsTracked = source.IsTracked;
		IsDominantHand = source.IsDominantHand;
		Config = source.Config;
		CopyPosesAndStateFrom(source);
	}

	public void CopyPosesAndStateFrom(ControllerDataAsset source)
	{
		Input = source.Input;
		RootPose = source.RootPose;
		RootPoseOrigin = source.RootPoseOrigin;
		PointerPose = source.PointerPose;
		PointerPoseOrigin = source.PointerPoseOrigin;
	}
}
