using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class ZoneShaderTriggerSettings : MonoBehaviour
{
	public enum ActivationType
	{
		ActivateSpecificSettings,
		ActivateCustomMapDefaults
	}

	public ActivationType activationType;

	[Tooltip("If this is TRUE, these ZoneShaderSettings will be activated when this GameObject is activated")]
	public bool activateOnEnable;

	public GameObject? zoneShaderSettingsObject;
}
