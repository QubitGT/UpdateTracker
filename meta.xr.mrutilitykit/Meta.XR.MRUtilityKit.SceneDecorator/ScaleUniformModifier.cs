using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class ScaleUniformModifier : Modifier
{
	[SerializeField]
	public Mask mask;

	[SerializeField]
	public float limitMin;

	[SerializeField]
	public float limitMax;

	[SerializeField]
	public float scale = 1f;

	[SerializeField]
	public float offset;

	public override void ApplyModifier(GameObject decorationGO, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration, Candidate candidate)
	{
		decorationGO.transform.localScale *= mask.SampleMask(candidate, limitMin, limitMax, scale, offset);
	}
}
