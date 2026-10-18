#define SUPPORTED_UNITY
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class LoadBalancingPeer : PhotonPeer
{
	private readonly Pool<ParameterDictionary> paramDictionaryPool = new Pool<ParameterDictionary>(() => new ParameterDictionary(), delegate(ParameterDictionary x)
	{
		x.Clear();
	}, 1);

	[Obsolete("Use RegionHandler.PingImplementation directly.")]
	protected internal static Type PingImplementation
	{
		get
		{
			return RegionHandler.PingImplementation;
		}
		set
		{
			RegionHandler.PingImplementation = value;
		}
	}

	public LoadBalancingPeer(ConnectionProtocol protocolType)
		: base(protocolType)
	{
		ConfigUnitySockets();
	}

	public LoadBalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
		: this(protocolType)
	{
		base.Listener = listener;
	}

	[Conditional("SUPPORTED_UNITY")]
	private void ConfigUnitySockets()
	{
		Type type = null;
		if ((RuntimeUnityFlagsSetup.IsUNITY_XBOXONE || RuntimeUnityFlagsSetup.IsUNITY_GAMECORE) && !RuntimeUnityFlagsSetup.IsUNITY_EDITOR)
		{
			type = Type.GetType("ExitGames.Client.Photon.SocketNativeSource, Assembly-CSharp", throwOnError: false);
			if (type == null)
			{
				type = Type.GetType("ExitGames.Client.Photon.SocketNativeSource, Assembly-CSharp-firstpass", throwOnError: false);
			}
			if (type == null)
			{
				type = Type.GetType("ExitGames.Client.Photon.SocketNativeSource, PhotonRealtime", throwOnError: false);
			}
			if (type != null)
			{
				SocketImplementationConfig[ConnectionProtocol.Udp] = type;
			}
		}
		else
		{
			type = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, PhotonWebSocket", throwOnError: false);
			if (type == null)
			{
				type = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp-firstpass", throwOnError: false);
			}
			if (type == null)
			{
				type = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp", throwOnError: false);
			}
			if (RuntimeUnityFlagsSetup.IsUNITY_WEBGL && type == null && (int)DebugOut >= 2)
			{
				base.Listener.DebugReturn(DebugLevel.WARNING, "SocketWebTcp type not found in the usual Assemblies. This is required as wrapper for the browser WebSocket API. Make sure to make the PhotonLibs\\WebSocket code available.");
			}
		}
		if (type != null)
		{
			SocketImplementationConfig[ConnectionProtocol.WebSocket] = type;
			SocketImplementationConfig[ConnectionProtocol.WebSocketSecure] = type;
		}
	}

	public virtual bool OpGetRegions(string appId)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>(1);
		dictionary[224] = appId;
		return SendOperation(220, dictionary, new SendOptions
		{
			Reliability = true,
			Encrypt = true
		});
	}

	public virtual bool OpJoinLobby(TypedLobby lobby = null)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpJoinLobby()");
		}
		Dictionary<byte, object> dictionary = null;
		if (lobby != null && !lobby.IsDefault)
		{
			dictionary = new Dictionary<byte, object>();
			dictionary[213] = lobby.Name;
			dictionary[212] = (byte)lobby.Type;
		}
		return SendOperation(229, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpLeaveLobby()
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpLeaveLobby()");
		}
		return SendOperation((byte)228, (Dictionary<byte, object>)null, SendOptions.SendReliable);
	}

	private void RoomOptionsToOpParameters(Dictionary<byte, object> op, RoomOptions roomOptions, bool usePropertiesKey = false)
	{
		if (roomOptions == null)
		{
			roomOptions = new RoomOptions();
		}
		Hashtable hashtable = new Hashtable();
		hashtable[253] = roomOptions.IsOpen;
		hashtable[254] = roomOptions.IsVisible;
		hashtable[250] = ((roomOptions.CustomRoomPropertiesForLobby == null) ? new string[0] : roomOptions.CustomRoomPropertiesForLobby);
		hashtable.MergeStringKeys(roomOptions.CustomRoomProperties);
		if (roomOptions.MaxPlayers > 0)
		{
			byte b = (byte)((roomOptions.MaxPlayers <= 255) ? ((byte)roomOptions.MaxPlayers) : 0);
			hashtable[byte.MaxValue] = b;
			hashtable[243] = roomOptions.MaxPlayers;
		}
		if (!usePropertiesKey)
		{
			op[248] = hashtable;
		}
		else
		{
			op[251] = hashtable;
		}
		int num = 0;
		if (roomOptions.CleanupCacheOnLeave)
		{
			op[241] = true;
			num |= 2;
		}
		else
		{
			op[241] = false;
			hashtable[249] = false;
		}
		num |= 1;
		op[232] = true;
		if (roomOptions.PlayerTtl > 0 || roomOptions.PlayerTtl == -1)
		{
			op[235] = roomOptions.PlayerTtl;
		}
		if (roomOptions.EmptyRoomTtl > 0)
		{
			op[236] = roomOptions.EmptyRoomTtl;
		}
		if (roomOptions.SuppressRoomEvents)
		{
			num |= 4;
			op[237] = true;
		}
		if (roomOptions.SuppressPlayerInfo)
		{
			num |= 0x40;
		}
		if (roomOptions.Plugins != null)
		{
			op[204] = roomOptions.Plugins;
		}
		if (roomOptions.PublishUserId)
		{
			num |= 8;
			op[239] = true;
		}
		if (roomOptions.DeleteNullProperties)
		{
			num |= 0x10;
		}
		if (roomOptions.BroadcastPropsChangeToAll)
		{
			num |= 0x20;
		}
		op[191] = num;
	}

	public virtual bool OpCreateRoom(EnterRoomParams opParams)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpCreateRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		SendOptions sendOptions2 = sendOptions;
		if (!string.IsNullOrEmpty(opParams.RoomName))
		{
			dictionary[byte.MaxValue] = opParams.RoomName;
		}
		if (opParams.Lobby != null && !opParams.Lobby.IsDefault)
		{
			dictionary[213] = opParams.Lobby.Name;
			dictionary[212] = (byte)opParams.Lobby.Type;
		}
		if (opParams.ExpectedUsers != null && opParams.ExpectedUsers.Length != 0)
		{
			dictionary[238] = opParams.ExpectedUsers;
			sendOptions2.Encrypt = true;
		}
		if (opParams.Ticket != null)
		{
			dictionary[190] = opParams.Ticket;
		}
		if (opParams.OnGameServer)
		{
			if (opParams.PlayerProperties != null && opParams.PlayerProperties.Count > 0)
			{
				dictionary[249] = opParams.PlayerProperties;
			}
			dictionary[250] = true;
			RoomOptionsToOpParameters(dictionary, opParams.RoomOptions);
		}
		return SendOperation(227, dictionary, sendOptions2);
	}

	public virtual bool OpJoinRoom(EnterRoomParams opParams)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		SendOptions sendOptions2 = sendOptions;
		if (!string.IsNullOrEmpty(opParams.RoomName))
		{
			dictionary[byte.MaxValue] = opParams.RoomName;
		}
		if (opParams.JoinMode == JoinMode.CreateIfNotExists)
		{
			dictionary[215] = (byte)1;
			if (opParams.Lobby != null && !opParams.Lobby.IsDefault)
			{
				dictionary[213] = opParams.Lobby.Name;
				dictionary[212] = (byte)opParams.Lobby.Type;
			}
		}
		else if (opParams.JoinMode == JoinMode.RejoinOnly)
		{
			dictionary[215] = (byte)3;
		}
		if (opParams.ExpectedUsers != null && opParams.ExpectedUsers.Length != 0)
		{
			dictionary[238] = opParams.ExpectedUsers;
			sendOptions2.Encrypt = true;
		}
		if (opParams.Ticket != null)
		{
			dictionary[190] = opParams.Ticket;
		}
		if (opParams.OnGameServer)
		{
			if (opParams.PlayerProperties != null && opParams.PlayerProperties.Count > 0)
			{
				dictionary[249] = opParams.PlayerProperties;
			}
			dictionary[250] = true;
			RoomOptionsToOpParameters(dictionary, opParams.RoomOptions);
		}
		return SendOperation(226, dictionary, sendOptions2);
	}

	public virtual bool OpJoinRandomRoom(OpJoinRandomRoomParams opJoinRandomRoomParams)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomRoom()");
		}
		Hashtable hashtable = new Hashtable();
		hashtable.MergeStringKeys(opJoinRandomRoomParams.ExpectedCustomRoomProperties);
		if (opJoinRandomRoomParams.ExpectedMaxPlayers > 0)
		{
			byte b = (byte)((opJoinRandomRoomParams.ExpectedMaxPlayers <= 255) ? ((byte)opJoinRandomRoomParams.ExpectedMaxPlayers) : 0);
			hashtable[byte.MaxValue] = b;
			if (opJoinRandomRoomParams.ExpectedMaxPlayers > 255)
			{
				hashtable[243] = opJoinRandomRoomParams.ExpectedMaxPlayers;
			}
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		SendOptions sendOptions2 = sendOptions;
		if (hashtable.Count > 0)
		{
			dictionary[248] = hashtable;
		}
		if (opJoinRandomRoomParams.MatchingType != 0)
		{
			dictionary[223] = (byte)opJoinRandomRoomParams.MatchingType;
		}
		if (opJoinRandomRoomParams.TypedLobby != null && !opJoinRandomRoomParams.TypedLobby.IsDefault)
		{
			dictionary[213] = opJoinRandomRoomParams.TypedLobby.Name;
			dictionary[212] = (byte)opJoinRandomRoomParams.TypedLobby.Type;
		}
		if (!string.IsNullOrEmpty(opJoinRandomRoomParams.SqlLobbyFilter))
		{
			dictionary[245] = opJoinRandomRoomParams.SqlLobbyFilter;
		}
		if (opJoinRandomRoomParams.ExpectedUsers != null && opJoinRandomRoomParams.ExpectedUsers.Length != 0)
		{
			dictionary[238] = opJoinRandomRoomParams.ExpectedUsers;
			sendOptions2.Encrypt = true;
		}
		if (opJoinRandomRoomParams.Ticket != null)
		{
			dictionary[190] = opJoinRandomRoomParams.Ticket;
		}
		dictionary[188] = true;
		return SendOperation(225, dictionary, sendOptions2);
	}

	public virtual bool OpJoinRandomOrCreateRoom(OpJoinRandomRoomParams opJoinRandomRoomParams, EnterRoomParams createRoomParams)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomOrCreateRoom()");
		}
		Hashtable hashtable = new Hashtable();
		hashtable.MergeStringKeys(opJoinRandomRoomParams.ExpectedCustomRoomProperties);
		if (opJoinRandomRoomParams.ExpectedMaxPlayers > 0)
		{
			byte b = (byte)((opJoinRandomRoomParams.ExpectedMaxPlayers <= 255) ? ((byte)opJoinRandomRoomParams.ExpectedMaxPlayers) : 0);
			hashtable[byte.MaxValue] = b;
			if (opJoinRandomRoomParams.ExpectedMaxPlayers > 255)
			{
				hashtable[243] = opJoinRandomRoomParams.ExpectedMaxPlayers;
			}
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		SendOptions sendOptions2 = sendOptions;
		if (hashtable.Count > 0)
		{
			dictionary[248] = hashtable;
		}
		if (opJoinRandomRoomParams.MatchingType != 0)
		{
			dictionary[223] = (byte)opJoinRandomRoomParams.MatchingType;
		}
		if (opJoinRandomRoomParams.TypedLobby != null && !opJoinRandomRoomParams.TypedLobby.IsDefault)
		{
			dictionary[213] = opJoinRandomRoomParams.TypedLobby.Name;
			dictionary[212] = (byte)opJoinRandomRoomParams.TypedLobby.Type;
		}
		if (!string.IsNullOrEmpty(opJoinRandomRoomParams.SqlLobbyFilter))
		{
			dictionary[245] = opJoinRandomRoomParams.SqlLobbyFilter;
		}
		if (opJoinRandomRoomParams.ExpectedUsers != null && opJoinRandomRoomParams.ExpectedUsers.Length != 0)
		{
			dictionary[238] = opJoinRandomRoomParams.ExpectedUsers;
			sendOptions2.Encrypt = true;
		}
		if (opJoinRandomRoomParams.Ticket != null)
		{
			dictionary[190] = opJoinRandomRoomParams.Ticket;
		}
		dictionary[215] = (byte)1;
		dictionary[188] = true;
		if (createRoomParams != null && !string.IsNullOrEmpty(createRoomParams.RoomName))
		{
			dictionary[byte.MaxValue] = createRoomParams.RoomName;
		}
		return SendOperation(225, dictionary, sendOptions2);
	}

	public virtual bool OpLeaveRoom(bool becomeInactive, bool sendAuthCookie = false)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (becomeInactive)
		{
			dictionary[233] = true;
		}
		if (sendAuthCookie)
		{
			dictionary[234] = (byte)2;
		}
		return SendOperation(254, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpGetGameList(TypedLobby lobby, string queryData)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList()");
		}
		if (lobby == null)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. Lobby cannot be null.");
			}
			return false;
		}
		if (lobby.Type != LobbyType.SqlLobby)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. LobbyType must be SqlLobby.");
			}
			return false;
		}
		if (lobby.IsDefault)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. LobbyName must be not null and not empty.");
			}
			return false;
		}
		if (string.IsNullOrEmpty(queryData))
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. queryData must be not null and not empty.");
			}
			return false;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[213] = lobby.Name;
		dictionary[212] = (byte)lobby.Type;
		dictionary[245] = queryData;
		return SendOperation(217, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpFindFriends(string[] friendsToFind, FindFriendsOptions options = null)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (friendsToFind != null && friendsToFind.Length != 0)
		{
			dictionary[1] = friendsToFind;
		}
		if (options != null)
		{
			dictionary[2] = options.ToIntFlags();
		}
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		sendOptions.Encrypt = true;
		SendOptions sendOptions2 = sendOptions;
		return SendOperation(222, dictionary, sendOptions2);
	}

	public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties)
	{
		return OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys());
	}

	protected internal bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, Hashtable expectedProperties = null, WebFlags webflags = null)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor()");
		}
		if (actorNr <= 0 || actorProperties == null || actorProperties.Count == 0)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor not sent. ActorNr must be > 0 and actorProperties must be not null nor empty.");
			}
			return false;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary.Add(251, actorProperties);
		dictionary.Add(254, actorNr);
		dictionary.Add(250, true);
		if (expectedProperties != null && expectedProperties.Count != 0)
		{
			dictionary.Add(231, expectedProperties);
		}
		if (webflags != null && webflags.HttpForward)
		{
			dictionary[234] = webflags.WebhookFlags;
		}
		return SendOperation(252, dictionary, SendOptions.SendReliable);
	}

	protected bool OpSetPropertyOfRoom(byte propCode, object value)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[propCode] = value;
		return OpSetPropertiesOfRoom(hashtable);
	}

	public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties)
	{
		return OpSetPropertiesOfRoom(gameProperties.StripToStringKeys());
	}

	protected internal bool OpSetPropertiesOfRoom(Hashtable gameProperties, Hashtable expectedProperties = null, WebFlags webflags = null)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom()");
		}
		if (gameProperties == null || gameProperties.Count == 0)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom not sent. gameProperties must be not null nor empty.");
			}
			return false;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary.Add(251, gameProperties);
		dictionary.Add(250, true);
		if (expectedProperties != null && expectedProperties.Count != 0)
		{
			dictionary.Add(231, expectedProperties);
		}
		if (webflags != null && webflags.HttpForward)
		{
			dictionary[234] = webflags.WebhookFlags;
		}
		return SendOperation(252, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpAuthenticate(string appId, string appVersion, AuthenticationValues authValues, string regionCode, bool getLobbyStatistics)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (getLobbyStatistics)
		{
			dictionary[211] = true;
		}
		if (authValues != null && authValues.Token != null)
		{
			dictionary[221] = authValues.Token;
			return SendOperation(230, dictionary, SendOptions.SendReliable);
		}
		dictionary[220] = appVersion;
		dictionary[224] = appId;
		if (!string.IsNullOrEmpty(regionCode))
		{
			dictionary[210] = regionCode;
		}
		if (authValues != null)
		{
			if (!string.IsNullOrEmpty(authValues.UserId))
			{
				dictionary[225] = authValues.UserId;
			}
			if (authValues.AuthType != CustomAuthenticationType.None)
			{
				dictionary[217] = (byte)authValues.AuthType;
				if (!string.IsNullOrEmpty(authValues.AuthGetParameters))
				{
					dictionary[216] = authValues.AuthGetParameters;
				}
				if (authValues.AuthPostData != null)
				{
					dictionary[214] = authValues.AuthPostData;
				}
			}
		}
		return SendOperation(230, dictionary, new SendOptions
		{
			Reliability = true,
			Encrypt = true
		});
	}

	public virtual bool OpAuthenticateOnce(string appId, string appVersion, AuthenticationValues authValues, string regionCode, EncryptionMode encryptionMode, ConnectionProtocol expectedProtocol)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticateOnce(): authValues = " + authValues?.ToString() + ", region = " + regionCode + ", encryption = " + encryptionMode);
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (authValues != null && authValues.Token != null)
		{
			dictionary[221] = authValues.Token;
			return SendOperation(231, dictionary, SendOptions.SendReliable);
		}
		if (encryptionMode == EncryptionMode.DatagramEncryptionGCM && expectedProtocol != 0)
		{
			throw new NotSupportedException("Expected protocol set to UDP, due to encryption mode DatagramEncryptionGCM.");
		}
		dictionary[195] = (byte)expectedProtocol;
		dictionary[193] = (byte)encryptionMode;
		dictionary[220] = appVersion;
		dictionary[224] = appId;
		if (!string.IsNullOrEmpty(regionCode))
		{
			dictionary[210] = regionCode;
		}
		if (authValues != null)
		{
			if (!string.IsNullOrEmpty(authValues.UserId))
			{
				dictionary[225] = authValues.UserId;
			}
			if (authValues.AuthType != CustomAuthenticationType.None)
			{
				dictionary[217] = (byte)authValues.AuthType;
				if (authValues.Token != null)
				{
					dictionary[221] = authValues.Token;
				}
				else
				{
					if (!string.IsNullOrEmpty(authValues.AuthGetParameters))
					{
						dictionary[216] = authValues.AuthGetParameters;
					}
					if (authValues.AuthPostData != null)
					{
						dictionary[214] = authValues.AuthPostData;
					}
				}
			}
		}
		return SendOperation(231, dictionary, new SendOptions
		{
			Reliability = true,
			Encrypt = true
		});
	}

	public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
	{
		if ((int)DebugOut >= 5)
		{
			base.Listener.DebugReturn(DebugLevel.ALL, "OpChangeGroups()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (groupsToRemove != null)
		{
			dictionary[239] = groupsToRemove;
		}
		if (groupsToAdd != null)
		{
			dictionary[238] = groupsToAdd;
		}
		return SendOperation(248, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
	{
		ParameterDictionary parameterDictionary = paramDictionaryPool.Acquire();
		try
		{
			if (raiseEventOptions != null)
			{
				if (raiseEventOptions.CachingOption != 0)
				{
					parameterDictionary.Add(247, (byte)raiseEventOptions.CachingOption);
				}
				switch (raiseEventOptions.CachingOption)
				{
				case EventCaching.SliceSetIndex:
				case EventCaching.SlicePurgeIndex:
				case EventCaching.SlicePurgeUpToIndex:
					return SendOperation(253, parameterDictionary, sendOptions);
				case EventCaching.RemoveFromRoomCacheForActorsLeft:
				case EventCaching.SliceIncreaseIndex:
					return SendOperation(253, parameterDictionary, sendOptions);
				case EventCaching.RemoveFromRoomCache:
					if (raiseEventOptions.TargetActors != null)
					{
						parameterDictionary.Add(252, raiseEventOptions.TargetActors);
					}
					break;
				default:
					if (raiseEventOptions.TargetActors != null)
					{
						parameterDictionary.Add(252, raiseEventOptions.TargetActors);
					}
					else if (raiseEventOptions.InterestGroup != 0)
					{
						parameterDictionary.Add(240, raiseEventOptions.InterestGroup);
					}
					else if (raiseEventOptions.Receivers != 0)
					{
						parameterDictionary.Add(246, (byte)raiseEventOptions.Receivers);
					}
					if (raiseEventOptions.Flags.HttpForward)
					{
						parameterDictionary.Add(234, raiseEventOptions.Flags.WebhookFlags);
					}
					break;
				}
			}
			parameterDictionary.Add(244, eventCode);
			if (customEventContent != null)
			{
				parameterDictionary.Add(245, customEventContent);
			}
			return SendOperation(253, parameterDictionary, sendOptions);
		}
		finally
		{
			paramDictionaryPool.Release(parameterDictionary);
		}
	}

	public virtual bool OpSettings(bool receiveLobbyStats)
	{
		if ((int)DebugOut >= 5)
		{
			base.Listener.DebugReturn(DebugLevel.ALL, "OpSettings()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (receiveLobbyStats)
		{
			dictionary[0] = receiveLobbyStats;
		}
		if (dictionary.Count == 0)
		{
			return true;
		}
		return SendOperation(218, dictionary, SendOptions.SendReliable);
	}
}
