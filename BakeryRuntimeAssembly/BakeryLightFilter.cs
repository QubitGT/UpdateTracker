using UnityEngine;

[HelpURL("https://geom.io/bakery/wiki/index.php?title=Manual#Bakery_Light_Filter")]
[DisallowMultipleComponent]
public class BakeryLightFilter : MonoBehaviour
{
	public Texture2D texture;

	[HideInInspector]
	public int lmid;
}
