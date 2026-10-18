using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[DisallowMultipleComponent]
public class SurfaceOverrideSettings : MonoBehaviour
{
	public SurfaceSoundOverride soundOverride;

	public float extraVelMultiplier = 1f;

	public float extraVelMaxMultiplier = 1f;

	[Tooltip("-1.0 represents the default value, valid values are between 0.0 and 1.0")]
	public float slidePercentage = -1f;

	[Tooltip("If TRUE, players won't be pushed away when tapping on the object")]
	public bool disablePushBackEffect;
}
