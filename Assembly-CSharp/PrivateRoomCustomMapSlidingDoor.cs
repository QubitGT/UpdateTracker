using UnityEngine;

[RequireComponent(typeof(CustomMapSlidingDoor))]
[AddComponentMenu("GorillaTag/Virtual Stump/Private Room Custom Map Sliding Door")]
public class PrivateRoomCustomMapSlidingDoor : MonoBehaviour
{
	private CustomMapSlidingDoor door;

	private void Awake()
	{
		door = GetComponent<CustomMapSlidingDoor>();
	}

	private void Update()
	{
		if (!(door == null))
		{
			if (ShouldBeOpen())
			{
				door.OpenDoor();
			}
			else if (MapBrowseRoomVolume.IsLocalPlayerInside)
			{
				door.CloseDoor();
			}
		}
	}

	private static bool ShouldBeOpen()
	{
		if (!CustomMapLoader.KeepTunnelDoorOpen && !CustomMapLoader.IsAwaitingPlayerEntry())
		{
			return CustomMapLoader.IsMapLoaded();
		}
		return true;
	}
}
