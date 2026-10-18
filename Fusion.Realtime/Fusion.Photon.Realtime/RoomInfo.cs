using System;
using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class RoomInfo
{
	public bool RemovedFromList;

	private Hashtable customProperties = new Hashtable();

	protected int maxPlayers = 0;

	protected int emptyRoomTtl = 0;

	protected int playerTtl = 0;

	protected string[] expectedUsers;

	protected bool isOpen = true;

	protected bool isVisible = true;

	protected bool autoCleanUp = true;

	protected string name;

	public int masterClientId;

	protected string[] propertiesListedInLobby;

	public Hashtable CustomProperties => customProperties;

	public string Name => name;

	public int PlayerCount { get; private set; }

	public int MaxPlayers => maxPlayers;

	public bool IsOpen => isOpen;

	public bool IsVisible => isVisible;

	protected internal RoomInfo(string roomName, Hashtable roomProperties)
	{
		InternalCacheProperties(roomProperties);
		name = roomName;
	}

	public override bool Equals(object other)
	{
		return other is RoomInfo roomInfo && Name.Equals(roomInfo.name);
	}

	public override int GetHashCode()
	{
		return name.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("Room: '{0}' {1},{2} {4}/{3} players.", name, isVisible ? "visible" : "hidden", isOpen ? "open" : "closed", maxPlayers, PlayerCount);
	}

	public string ToStringFull()
	{
		return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", name, isVisible ? "visible" : "hidden", isOpen ? "open" : "closed", maxPlayers, PlayerCount, customProperties.ToStringFull());
	}

	protected internal virtual void InternalCacheProperties(Hashtable propertiesToCache)
	{
		if (propertiesToCache == null || propertiesToCache.Count == 0 || customProperties.Equals(propertiesToCache))
		{
			return;
		}
		if (propertiesToCache.ContainsKey(251))
		{
			RemovedFromList = (bool)propertiesToCache[251];
			if (RemovedFromList)
			{
				return;
			}
		}
		if (propertiesToCache.ContainsKey(243))
		{
			maxPlayers = Convert.ToInt32(propertiesToCache[243]);
		}
		else if (propertiesToCache.ContainsKey(byte.MaxValue))
		{
			maxPlayers = Convert.ToInt32(propertiesToCache[byte.MaxValue]);
		}
		if (propertiesToCache.ContainsKey(253))
		{
			isOpen = (bool)propertiesToCache[253];
		}
		if (propertiesToCache.ContainsKey(254))
		{
			isVisible = (bool)propertiesToCache[254];
		}
		if (propertiesToCache.ContainsKey(252))
		{
			PlayerCount = Convert.ToInt32(propertiesToCache[252]);
		}
		if (propertiesToCache.ContainsKey(249))
		{
			autoCleanUp = (bool)propertiesToCache[249];
		}
		if (propertiesToCache.ContainsKey(248))
		{
			masterClientId = (int)propertiesToCache[248];
		}
		if (propertiesToCache.ContainsKey(250))
		{
			propertiesListedInLobby = propertiesToCache[250] as string[];
		}
		if (propertiesToCache.ContainsKey(247))
		{
			expectedUsers = (string[])propertiesToCache[247];
		}
		if (propertiesToCache.ContainsKey(245))
		{
			emptyRoomTtl = (int)propertiesToCache[245];
		}
		if (propertiesToCache.ContainsKey(246))
		{
			playerTtl = (int)propertiesToCache[246];
		}
		customProperties.MergeStringKeys(propertiesToCache);
		customProperties.StripKeysWithNullValues();
	}
}
