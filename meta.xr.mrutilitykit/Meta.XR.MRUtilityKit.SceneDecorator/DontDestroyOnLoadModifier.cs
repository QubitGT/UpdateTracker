using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class DontDestroyOnLoadModifier : Modifier
{
	public override void ApplyModifier(GameObject decorationGO, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration, Candidate candidate)
	{
		Object.DontDestroyOnLoad(decorationGO);
	}
}
