using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class Player
{
	private int actorNumber = -1;

	public readonly bool IsLocal;

	private string nickName = string.Empty;

	public object TagObject;

	protected internal Room RoomReference { get; set; }

	public int ActorNumber => actorNumber;

	public bool HasRejoined { get; internal set; }

	public string NickName
	{
		get
		{
			return nickName;
		}
		set
		{
			if (string.IsNullOrEmpty(nickName) || !nickName.Equals(value))
			{
				nickName = value;
				if (IsLocal)
				{
					SetPlayerNameProperty();
				}
			}
		}
	}

	public string UserId { get; internal set; }

	public bool IsMasterClient
	{
		get
		{
			if (RoomReference == null)
			{
				return false;
			}
			return ActorNumber == RoomReference.MasterClientId;
		}
	}

	public bool IsInactive { get; protected internal set; }

	public Hashtable CustomProperties { get; set; }

	protected internal Player(string nickName, int actorNumber, bool isLocal)
		: this(nickName, actorNumber, isLocal, null)
	{
	}

	protected internal Player(string nickName, int actorNumber, bool isLocal, Hashtable playerProperties)
	{
		IsLocal = isLocal;
		this.actorNumber = actorNumber;
		NickName = nickName;
		CustomProperties = new Hashtable();
		InternalCacheProperties(playerProperties);
	}

	public Player Get(int id)
	{
		if (RoomReference == null)
		{
			return null;
		}
		return RoomReference.GetPlayer(id);
	}

	public Player GetNext()
	{
		return GetNextFor(ActorNumber);
	}

	public Player GetNextFor(Player currentPlayer)
	{
		if (currentPlayer == null)
		{
			return null;
		}
		return GetNextFor(currentPlayer.ActorNumber);
	}

	public Player GetNextFor(int currentPlayerId)
	{
		if (RoomReference == null || RoomReference.Players == null || RoomReference.Players.Count < 2)
		{
			return null;
		}
		Dictionary<int, Player> players = RoomReference.Players;
		int num = int.MaxValue;
		int num2 = currentPlayerId;
		foreach (int key in players.Keys)
		{
			if (key < num2)
			{
				num2 = key;
			}
			else if (key > currentPlayerId && key < num)
			{
				num = key;
			}
		}
		return (num != int.MaxValue) ? players[num] : players[num2];
	}

	protected internal virtual void InternalCacheProperties(Hashtable properties)
	{
		if (properties != null && properties.Count != 0 && !CustomProperties.Equals(properties))
		{
			if (!IsLocal && properties.ContainsKey(byte.MaxValue))
			{
				string text = (string)properties[byte.MaxValue];
				NickName = text;
			}
			if (properties.ContainsKey(253))
			{
				UserId = (string)properties[253];
			}
			if (properties.ContainsKey(254))
			{
				IsInactive = (bool)properties[254];
			}
			CustomProperties.MergeStringKeys(properties);
			CustomProperties.StripKeysWithNullValues();
		}
	}

	public override string ToString()
	{
		return $"#{ActorNumber:00} '{NickName}'";
	}

	public string ToStringFull()
	{
		return string.Format("#{0:00} '{1}'{2} {3}", ActorNumber, NickName, IsInactive ? " (inactive)" : "", CustomProperties.ToStringFull());
	}

	public override bool Equals(object p)
	{
		return p is Player player && GetHashCode() == player.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ActorNumber;
	}

	protected internal void ChangeLocalID(int newID)
	{
		if (IsLocal)
		{
			actorNumber = newID;
		}
	}

	public bool SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedValues = null, WebFlags webFlags = null)
	{
		if (propertiesToSet == null || propertiesToSet.Count == 0)
		{
			return false;
		}
		Hashtable hashtable = propertiesToSet.StripToStringKeys();
		if (RoomReference != null)
		{
			if (RoomReference.IsOffline)
			{
				if (hashtable.Count == 0)
				{
					return false;
				}
				CustomProperties.Merge(hashtable);
				CustomProperties.StripKeysWithNullValues();
				RoomReference.LoadBalancingClient.InRoomCallbackTargets.OnPlayerPropertiesUpdate(this, hashtable);
				return true;
			}
			Hashtable expectedProperties = expectedValues.StripToStringKeys();
			return RoomReference.LoadBalancingClient.OpSetPropertiesOfActor(actorNumber, hashtable, expectedProperties, webFlags);
		}
		if (IsLocal)
		{
			if (hashtable.Count == 0)
			{
				return false;
			}
			if (expectedValues == null && webFlags == null)
			{
				CustomProperties.Merge(hashtable);
				CustomProperties.StripKeysWithNullValues();
				return true;
			}
		}
		return false;
	}

	internal bool UpdateNickNameOnJoined()
	{
		if (RoomReference == null || RoomReference.CustomProperties == null || !IsLocal)
		{
			return false;
		}
		string b = (RoomReference.CustomProperties.ContainsKey(byte.MaxValue) ? (RoomReference.CustomProperties[byte.MaxValue] as string) : string.Empty);
		if (!string.Equals(NickName, b))
		{
			return SetPlayerNameProperty();
		}
		return true;
	}

	private bool SetPlayerNameProperty()
	{
		if (RoomReference != null && !RoomReference.IsOffline)
		{
			Hashtable hashtable = new Hashtable();
			hashtable[byte.MaxValue] = nickName;
			return RoomReference.LoadBalancingClient.OpSetPropertiesOfActor(ActorNumber, hashtable);
		}
		return false;
	}
}
