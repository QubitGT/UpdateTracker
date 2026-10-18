using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(Light))]
public class UberShaderDynamicLight : MonoBehaviour
{
	public Light? dynamicLight;

	private void Awake()
	{
		if (dynamicLight == null)
		{
			dynamicLight = GetComponent<Light>();
		}
	}
}
