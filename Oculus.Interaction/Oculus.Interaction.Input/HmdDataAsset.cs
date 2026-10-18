using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Serializable]
public class HmdDataAsset : ICopyFrom<HmdDataAsset>
{
	public Pose Root;

	public bool IsTracked;

	public int FrameId;

	public HmdDataSourceConfig Config;

	public void CopyFrom(HmdDataAsset source)
	{
		Root = source.Root;
		IsTracked = source.IsTracked;
		FrameId = source.FrameId;
		Config = source.Config;
	}
}
