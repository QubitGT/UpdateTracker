using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.Input;

public class BodyDataAsset : ICopyFrom<BodyDataAsset>
{
	public ISkeletonMapping SkeletonMapping { get; set; }

	public Pose Root { get; set; } = Pose.identity;


	public float RootScale { get; set; } = 1f;


	public bool IsDataValid { get; set; }

	public bool IsDataHighConfidence { get; set; }

	public Pose[] JointPoses { get; set; } = new Pose[84];


	public int SkeletonChangedCount { get; set; }

	public void CopyFrom(BodyDataAsset source)
	{
		SkeletonMapping = source.SkeletonMapping;
		Root = source.Root;
		RootScale = source.RootScale;
		IsDataValid = source.IsDataValid;
		IsDataHighConfidence = source.IsDataHighConfidence;
		SkeletonChangedCount = source.SkeletonChangedCount;
		for (int i = 0; i < source.JointPoses.Length; i++)
		{
			JointPoses[i] = source.JointPoses[i];
		}
	}
}
