using GorillaLocomotion;
using UnityEngine;

[AddComponentMenu("GorillaTag/Virtual Stump/Map Browse Room Volume")]
public class MapBrowseRoomVolume : MonoBehaviour
{
	public static bool IsLocalPlayerInside { get; private set; }

	public void OnTriggerEnter(Collider other)
	{
		if (!(other != GTPlayer.Instance.headCollider))
		{
			IsLocalPlayerInside = true;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (!(other != GTPlayer.Instance.headCollider))
		{
			IsLocalPlayerInside = false;
			CustomMapsTerminal.ReleaseLocalTerminalControl();
		}
	}

	private void OnDisable()
	{
		IsLocalPlayerInside = false;
	}
}
