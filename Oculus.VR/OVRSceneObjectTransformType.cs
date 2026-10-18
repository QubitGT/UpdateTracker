using System;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#further-scene-model-unity-components")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
public class OVRSceneObjectTransformType : MonoBehaviour
{
	[Serializable]
	public enum Transformation
	{
		Volume,
		Plane,
		None
	}

	[Tooltip("Choose the type of scene anchor (volume/plane) that may modify this transform.")]
	public Transformation TransformType;
}
