using GorillaTagScripts.VirtualStumpCustomMaps;
using UnityEngine;

[AddComponentMenu("GorillaTag/Virtual Stump/Enter V Stump Proxy")]
public class EnterVStumpProxy : MonoBehaviour
{
	public void EnterVStump()
	{
		CustomMapManager.EnterVStump();
	}
}
