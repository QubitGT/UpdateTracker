using UnityEngine;

[HelpURL("https://geom.io/bakery/wiki/index.php?title=Manual#Bakery_Direct_Light")]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class BakeryDirectLight : MonoBehaviour
{
	public Color color = Color.white;

	public float intensity = 1f;

	public float shadowSpread = 0.01f;

	public int samples = 16;

	public int bitmask = 1;

	public bool bakeToIndirect;

	public bool shadowmask;

	public bool shadowmaskDenoise;

	public float indirectIntensity = 1f;

	public Texture2D cloudShadow;

	public float cloudShadowTilingX = 0.01f;

	public float cloudShadowTilingY = 0.01f;

	public float cloudShadowOffsetX;

	public float cloudShadowOffsetY;

	public bool supersample;

	public int UID;

	public static int lightsChanged;

	private static GameObject objShownError;
}
