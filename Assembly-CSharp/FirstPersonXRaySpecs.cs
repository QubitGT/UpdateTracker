using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/First Person X Ray Specs")]
public class FirstPersonXRaySpecs : MonoBehaviour
{
	private void OnEnable()
	{
		GorillaBodyRenderer.SetAllSkeletons(allSkeletons: true);
	}

	private void OnDisable()
	{
		GorillaBodyRenderer.SetAllSkeletons(allSkeletons: false);
	}
}
