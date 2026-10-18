using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime;

public class Room : RoomInfo
{
	private bool isOffline;

	private Dictionary<int, Player> players = new Dictionary<int, Player>();

	public LoadBalancingClient LoadBalancingClient { get; set; }

	public new string Name
	{
		get
		{
			return name;
		}
		internal set
		{
			name = value;
		}
	}

	public bool IsOffline
	{
		get
		{
			return isOffline;
		}
		private set
		{
			isOffline = value;
		}
	}

	public new bool IsOpen
	{
		get
		{
			return isOpen;
		}
		set
		{
			if (value != isOpen && !isOffline)
			{
				LoadBalancingClient.OpSetPropertiesOfRoom(new Hashtable { { 253, value } });
			}
			isOpen = value;
		}
	}

	public new bool IsVisible
	{
		get
		{
			return isVisible;
		}
		set
		{
			if (value != isVisible && !isOffline)
			{
				LoadBalancingClient.OpSetPropertiesOfRoom(new Hashtable { { 254, value } });
			}
			isVisible = value;
		}
	}

	public new byte MaxPlayers
	{
		get
		{
			return maxPlayers;
		}
		set
		{
			if (value != maxPlayers && !isOffline)
			{
				LoadBalancingClient.OpSetPropertiesOfRoom(new Hashtable { 
				{
					byte.MaxValue,
					value
				} });
			}
			maxPlayers = value;
		}
	}

	public new byte PlayerCount
	{
		get
		{
			if (Players == null)
			{
				return 0;
			}
			return (byte)Players.Count;
		}
	}

	public Dictionary<int, Player> Players
	{
		get
		{
			return players;
		}
		private set
		{
			players = value;
		}
	}

	public string[] ExpectedUsers => expectedUsers;

	public int PlayerTtl
	{
		get
		{
			return playerTtl;
		}
		set
		{
			if (value != playerTtl && !isOffline)
			{
				LoadBalancingClient.OpSetPropertyOfRoom(246, value);
			}
			playerTtl = value;
		}
	}

	public int EmptyRoomTtl
	{
		get
		{
			return emptyRoomTtl;
		}
		set
		{
			if (value != emptyRoomTtl && !isOffline)
			{
				LoadBalancingClient.OpSetPropertyOfRoom(245, value);
			}
			emptyRoomTtl = value;
		}
	}

	public int MasterClientId => masterClientId;

	public string[] PropertiesListedInLobby
	{
		get
		{
			return propertiesListedInLobby;
		}
		private set
		{
			propertiesListedInLobby = value;
		}
	}

	public bool AutoCleanUp => autoCleanUp;

	public bool BroadcastPropertiesChangeToAll { get; private set; }

	public bool SuppressRoomEvents { get; private set; }

	public bool SuppressPlayerInfo { get; private set; }

	public bool PublishUserId { get; private set; }

	public bool DeleteNullProperties { get; private set; }

	public Room(string roomName, RoomOptions options, bool isOffline = false)
		: base(roomName, options?.CustomRoomProperties)
	{
		if (options != null)
		{
			isVisible = options.IsVisible;
			isOpen = options.IsOpen;
			maxPlayers = options.MaxPlayers;
			propertiesListedInLobby = options.CustomRoomPropertiesForLobby;
		}
		this.isOffline = isOffline;
	}

	internal void InternalCacheRoomFlags(int roomFlags)
	{
		BroadcastPropertiesChangeToAll = (roomFlags & 0x20) != 0;
		SuppressRoomEvents = (roomFlags & 4) != 0;
		SuppressPlayerInfo = (roomFlags & 0x40) != 0;
		PublishUserId = (roomFlags & 8) != 0;
		DeleteNullProperties = (roomFlags & 0x10) != 0;
		autoCleanUp = (roomFlags & 2) != 0;
	}

	protected internal override void InternalCacheProperties(Hashtable propertiesToCache)
	{
		int num = masterClientId;
		base.InternalCacheProperties(propertiesToCache);
		if (num != 0 && masterClientId != num)
		{
			LoadBalancingClient.InRoomCallbackTargets.OnMasterClientSwitched(GetPlayer(masterClientId));
		}
	}

	public virtual bool SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
	{
		if (propertiesToSet == null || propertiesToSet.Count == 0)
		{
			return false;
		}
		Hashtable hashtable = propertiesToSet.StripToStringKeys();
		if (isOffline)
		{
			if (hashtable.Count == 0)
			{
				return false;
			}
			base.CustomProperties.Merge(hashtable);
			base.CustomProperties.StripKeysWithNullValues();
			LoadBalancingClient.InRoomCallbackTargets.OnRoomPropertiesUpdate(propertiesToSet);
			return true;
		}
		return LoadBalancingClient.OpSetPropertiesOfRoom(hashtable, expectedProperties, webFlags);
	}

	public bool SetPropertiesListedInLobby(string[] lobbyProps)
	{
		if (isOffline)
		{
			return false;
		}
		Hashtable hashtable = new Hashtable();
		hashtable[250] = lobbyProps;
		return LoadBalancingClient.OpSetPropertiesOfRoom(hashtable);
	}

	protected internal virtual void RemovePlayer(Player player)
	{
		Players.Remove(player.ActorNumber);
		player.RoomReference = null;
	}

	protected internal virtual void RemovePlayer(int id)
	{
		RemovePlayer(GetPlayer(id));
	}

	public bool SetMasterClient(Player masterClientPlayer)
	{
		if (isOffline)
		{
			return false;
		}
		Hashtable gameProperties = new Hashtable { { 248, masterClientPlayer.ActorNumber } };
		Hashtable expectedProperties = new Hashtable { { 248, MasterClientId } };
		return LoadBalancingClient.OpSetPropertiesOfRoom(gameProperties, expectedProperties);
	}

	public virtual bool AddPlayer(Player player)
	{
		if (!Players.ContainsKey(player.ActorNumber))
		{
			StorePlayer(player);
			return true;
		}
		return false;
	}

	public virtual Player StorePlayer(Player player)
	{
		Players[player.ActorNumber] = player;
		player.RoomReference = this;
		return player;
	}

	public virtual Player GetPlayer(int id, bool findMaster = false)
	{
		int key = ((findMaster && id == 0) ? MasterClientId : id);
		Player value = null;
		Players.TryGetValue(key, out value);
		return value;
	}

	public bool ClearExpectedUsers()
	{
		if (ExpectedUsers == null || ExpectedUsers.Length == 0)
		{
			return false;
		}
		return SetExpectedUsers(new string[0], ExpectedUsers);
	}

	public bool SetExpectedUsers(string[] newExpectedUsers)
	{
		if (newExpectedUsers == null || newExpectedUsers.Length == 0)
		{
			LoadBalancingClient.DebugReturn(DebugLevel.ERROR, "newExpectedUsers array is null or empty, call Room.ClearExpectedUsers() instead if this is what you want.");
			return false;
		}
		return SetExpectedUsers(newExpectedUsers, ExpectedUsers);
	}

	private bool SetExpectedUsers(string[] newExpectedUsers, string[] oldExpectedUsers)
	{
		if (isOffline)
		{
			return false;
		}
		Hashtable hashtable = new Hashtable(1);
		hashtable.Add(247, newExpectedUsers);
		Hashtable hashtable2 = null;
		if (oldExpectedUsers != null)
		{
			hashtable2 = new Hashtable(1);
			hashtable2.Add(247, oldExpectedUsers);
		}
		return LoadBalancingClient.OpSetPropertiesOfRoom(hashtable, hashtable2);
	}

	public override string ToString()
	{
		return string.Format("Room: '{0}' {1},{2} {4}/{3} players.", name, isVisible ? "visible" : "hidden", isOpen ? "open" : "closed", maxPlayers, PlayerCount);
	}

	public new string ToStringFull()
	{
		return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", name, isVisible ? "visible" : "hidden", isOpen ? "open" : "closed", maxPlayers, PlayerCount, base.CustomProperties.ToStringFull());
	}
}
