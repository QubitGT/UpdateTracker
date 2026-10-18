using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class SpaceMapGPUMask : Mask
{
	private SpaceMapGPU spaceMap;

	public override float SampleMask(Candidate candidate)
	{
		if (spaceMap == null)
		{
			spaceMap = Object.FindAnyObjectByType<SpaceMapGPU>();
			if (spaceMap == null)
			{
				Debug.LogWarning("SpaceMapGPU cannot be found, does it exist in the Scene?");
				return 0f;
			}
		}
		return spaceMap.GetColorAtPosition(candidate.hit.point).r;
	}

	public override bool Check(Candidate c)
	{
		return true;
	}
}
