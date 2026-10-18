using UnityEngine;

namespace Unity.Cinemachine;

public class FoldoutWithEnabledButtonAttribute : PropertyAttribute
{
	public string EnabledPropertyName;

	public FoldoutWithEnabledButtonAttribute(string enabledProperty = "Enabled")
	{
		EnabledPropertyName = enabledProperty;
	}
}
