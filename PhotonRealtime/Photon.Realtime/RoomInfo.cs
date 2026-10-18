using ExitGames.Client.Photon;

namespace Photon.Realtime;

public class RoomInfo
{
	public bool RemovedFromList;

	private Hashtable customProperties = new Hashtable();

	protected byte maxPlayers;

	protected int emptyRoomTtl;

	protected int playerTtl;

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

	public byte MaxPlayers => maxPlayers;

	public bool IsOpen => isOpen;

	public bool IsVisible => isVisible;

	protected internal RoomInfo(string roomName, Hashtable roomProperties)
	{
		InternalCacheProperties(roomProperties);
		name = roomName;
	}

	public override bool Equals(object other)
	{
		if (other is RoomInfo roomInfo)
		{
			return Name.Equals(roomInfo.name);
		}
		return false;
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
		if (propertiesToCache.ContainsKey(251) && propertiesToCache[251] is bool removedFromList)
		{
			RemovedFromList = removedFromList;
			if (RemovedFromList)
			{
				return;
			}
		}
		if (propertiesToCache.ContainsKey(byte.MaxValue) && propertiesToCache[byte.MaxValue] is byte b)
		{
			maxPlayers = b;
		}
		if (propertiesToCache.ContainsKey(253) && propertiesToCache[253] is bool flag)
		{
			isOpen = flag;
		}
		if (propertiesToCache.ContainsKey(254) && propertiesToCache[254] is bool flag2)
		{
			isVisible = flag2;
		}
		if (propertiesToCache.ContainsKey(252) && (propertiesToCache[252] is int || propertiesToCache[252] is byte))
		{
			PlayerCount = (byte)propertiesToCache[252];
		}
		if (propertiesToCache.ContainsKey(249) && propertiesToCache[249] is bool flag3)
		{
			autoCleanUp = flag3;
		}
		if (propertiesToCache.ContainsKey(248) && propertiesToCache[248] is int num)
		{
			masterClientId = num;
		}
		if (propertiesToCache.ContainsKey(250))
		{
			propertiesListedInLobby = propertiesToCache[250] as string[];
		}
		if (propertiesToCache.ContainsKey(247))
		{
			expectedUsers = propertiesToCache[247] as string[];
		}
		if (propertiesToCache.ContainsKey(245) && propertiesToCache[245] is int num2)
		{
			emptyRoomTtl = num2;
		}
		if (propertiesToCache.ContainsKey(246) && propertiesToCache[246] is int num3)
		{
			playerTtl = num3;
		}
		customProperties.MergeStringKeys(propertiesToCache);
		customProperties.StripKeysWithNullValues();
	}
}
