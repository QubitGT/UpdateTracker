using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.XR.InputActions;

[Serializable]
public class InputActionDefinition
{
	[Tooltip("The name of this action. This is used in functions like OVRPlugin.GetActionStateBoolean to identify this specific action.")]
	public string ActionName;

	[Tooltip("The type of this action. Does it return a bool, pose, vector2, float or trigger a vibration?")]
	public OVRPlugin.ActionTypes Type;

	[Tooltip("Paths: the path from where this action will get its data. This is based on the OpenXR specification for the device.")]
	[FormerlySerializedAs("Path")]
	public string[] Paths;
}
