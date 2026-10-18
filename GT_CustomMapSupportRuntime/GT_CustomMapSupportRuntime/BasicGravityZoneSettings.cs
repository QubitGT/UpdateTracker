using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class BasicGravityZoneSettings : MonoBehaviour
{
	public enum GravityZoneScaleFilter
	{
		Anyone,
		SmallOnly,
		NotSmall
	}

	public enum GravityZoneRule
	{
		Newest,
		Closest,
		Additive
	}

	[Header("Gravity Settings")]
	[Tooltip("negative number pulls, positive number pushes")]
	public float gravityStrength = -9.81f;

	[Tooltip("Filter which players are affected based on scale. Small = scale < 1")]
	public GravityZoneScaleFilter scaleFilter;

	[Tooltip("- Newest: Only in effect when this is the newest zone entered by the physics object. \n\n- Closest: if this gravity zone is the closest, then it will have effect. \n\n- Additive:  always in effect when a physics object is inside.")]
	public GravityZoneRule gravityRule;

	[Tooltip("The gravity zone with the highest authority will cause gravity zones with a lower authority level to be ignored. Gravity zones with the same authority level will follow the Gravity Rule setting.")]
	public int authorityLevel;

	[Header("Rotation Settings")]
	[Tooltip("If enabled, rotates the target away from gravity direction to be upside down")]
	public bool invertRotationDirection;

	public bool rotateTarget = true;

	public bool useRotationSpeedOverride;

	public float rotationSpeedOverride = 10f;
}
