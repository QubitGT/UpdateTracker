using UnityEngine;

[HelpURL("https://geom.io/bakery/wiki/index.php?title=Manual#Bakery_Light_Mesh")]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class BakeryLightMesh : MonoBehaviour
{
	public int UID;

	public Color color = Color.white;

	public float intensity = 1f;

	public Texture2D texture;

	public float cutoff = 100f;

	public int samples = 256;

	public int samples2 = 16;

	public int samples2_previous = 16;

	public int bitmask = 1;

	public bool selfShadow = true;

	public bool bakeToIndirect = true;

	public bool shadowmask;

	public float indirectIntensity = 1f;

	public bool shadowmaskFalloff;

	public int maskChannel;

	public int lmid = -2;

	public static int lightsChanged;

	private static GameObject objShownError;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
		if (component != null)
		{
			Gizmos.DrawWireSphere(component.bounds.center, cutoff);
		}
	}
}
