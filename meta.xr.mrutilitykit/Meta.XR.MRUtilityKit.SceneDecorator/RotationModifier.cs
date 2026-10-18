using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class RotationModifier : Modifier
{
	[SerializeField]
	public Mask mask;

	[SerializeField]
	public float limitMin = float.NegativeInfinity;

	[SerializeField]
	public float limitMax = float.PositiveInfinity;

	[SerializeField]
	public float scale = 1f;

	[SerializeField]
	public float offset;

	[SerializeField]
	public Vector3 rotationAxis = new Vector3(0f, 1f, 0f);

	[SerializeField]
	public bool localSpace;

	public override void ApplyModifier(GameObject decorationGO, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration, Candidate candidate)
	{
		Vector3 axis = (localSpace ? decorationGO.transform.rotation : Quaternion.identity) * rotationAxis;
		decorationGO.transform.rotation *= Quaternion.AngleAxis(mask.SampleMask(candidate, limitMin, limitMax, scale, offset), axis);
	}
}
