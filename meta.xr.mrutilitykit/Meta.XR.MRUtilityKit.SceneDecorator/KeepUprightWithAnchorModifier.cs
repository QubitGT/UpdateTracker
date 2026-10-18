using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class KeepUprightWithAnchorModifier : Modifier
{
	[SerializeField]
	public Vector3 uprightAxis = new Vector3(0f, 1f, 0f);

	public override void ApplyModifier(GameObject decorationGO, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration, Candidate candidate)
	{
		Quaternion rotation = decorationGO.transform.rotation;
		Vector3 fromDirection = rotation * uprightAxis;
		rotation *= Quaternion.FromToRotation(fromDirection, sceneAnchor.transform.up);
		decorationGO.transform.rotation = rotation;
	}
}
