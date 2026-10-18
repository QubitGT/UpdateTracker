using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.InputActions;

[Serializable]
public class UserInputActionSet
{
	[InlineLink("https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#semantic-path-interaction-profiles")]
	[Tooltip("The interaction profile of the device these actions should be applied to.")]
	public string InteractionProfile;

	[Tooltip("A list of the different Input Actions that this device supports.")]
	public List<InputActionDefinition> InputActionDefinitions = new List<InputActionDefinition>();

	public override string ToString()
	{
		return JsonUtility.ToJson(this);
	}
}
