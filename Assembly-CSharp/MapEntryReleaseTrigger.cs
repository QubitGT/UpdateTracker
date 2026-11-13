using GorillaLocomotion;
using GorillaTagScripts.VirtualStumpCustomMaps;
using UnityEngine;

[AddComponentMenu("GorillaTag/Virtual Stump/Map Entry Release Trigger")]
public class MapEntryReleaseTrigger : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		if (!(other != GTPlayer.Instance.headCollider) && !CustomMapDevWatchMode.IsWatching)
		{
			CustomMapsTerminal.ReleaseLocalTerminalControl();
		}
	}
}
