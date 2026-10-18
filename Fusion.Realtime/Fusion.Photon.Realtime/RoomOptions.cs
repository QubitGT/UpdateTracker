using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class RoomOptions
{
	private bool isVisible = true;

	private bool isOpen = true;

	public int MaxPlayers;

	public int PlayerTtl;

	public int EmptyRoomTtl;

	private bool cleanupCacheOnLeave = true;

	public Hashtable CustomRoomProperties;

	public string[] CustomRoomPropertiesForLobby = new string[0];

	public string[] Plugins;

	private bool broadcastPropsChangeToAll = true;

	public bool IsVisible
	{
		get
		{
			return isVisible;
		}
		set
		{
			isVisible = value;
		}
	}

	public bool IsOpen
	{
		get
		{
			return isOpen;
		}
		set
		{
			isOpen = value;
		}
	}

	public bool CleanupCacheOnLeave
	{
		get
		{
			return cleanupCacheOnLeave;
		}
		set
		{
			cleanupCacheOnLeave = value;
		}
	}

	public bool SuppressRoomEvents { get; set; }

	public bool SuppressPlayerInfo { get; set; }

	public bool PublishUserId { get; set; }

	public bool DeleteNullProperties { get; set; }

	public bool BroadcastPropsChangeToAll
	{
		get
		{
			return broadcastPropsChangeToAll;
		}
		set
		{
			broadcastPropsChangeToAll = value;
		}
	}
}
