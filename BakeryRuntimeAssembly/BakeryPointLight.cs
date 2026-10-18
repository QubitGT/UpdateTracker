using UnityEngine;

[HelpURL("https://geom.io/bakery/wiki/index.php?title=Manual#Bakery_Point_Light")]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class BakeryPointLight : MonoBehaviour
{
	public enum ftLightProjectionMode
	{
		Omni,
		Cookie,
		Cubemap,
		IES,
		Cone
	}

	public enum Direction
	{
		NegativeY,
		PositiveZ
	}

	public int UID;

	public Color color = Color.white;

	public float intensity = 1f;

	public float shadowSpread = 0.05f;

	public float cutoff = 10f;

	public bool realisticFalloff;

	public bool legacySampling = true;

	public int samples = 8;

	public ftLightProjectionMode projMode;

	public Texture2D cookie;

	public float angle = 30f;

	public float innerAngle;

	public Cubemap cubemap;

	public Object iesFile;

	public int bitmask = 1;

	public bool bakeToIndirect;

	public bool shadowmask;

	public bool shadowmaskFalloff;

	public float indirectIntensity = 1f;

	public float falloffMinRadius = 1f;

	public int shadowmaskGroupID;

	public bool correctCookieDistortion;

	public Direction directionMode;

	public int maskChannel;

	private const float GIZMO_MAXSIZE = 0.1f;

	private const float GIZMO_SCALE = 0.01f;

	public static int lightsChanged;

	private static GameObject objShownError;
}
