using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Realtime;

public class LoadBalancingClient : IPhotonPeerListener
{
	private class EncryptionDataParameters
	{
		public const byte Mode = 0;

		public const byte Secret1 = 1;

		public const byte Secret2 = 2;
	}

	private class CallbackTargetChange
	{
		public readonly object Target;

		public readonly bool AddTarget;

		public CallbackTargetChange(object target, bool addTarget)
		{
			Target = target;
			AddTarget = addTarget;
		}
	}

	public AuthModeOption AuthMode;

	public EncryptionMode EncryptionMode;

	private object tokenCache;

	public string NameServerHost = "ns.photonengine.io";

	private static readonly Dictionary<ConnectionProtocol, int> ProtocolToNameServerPort = new Dictionary<ConnectionProtocol, int>
	{
		{
			ConnectionProtocol.Udp,
			5058
		},
		{
			ConnectionProtocol.Tcp,
			4533
		},
		{
			ConnectionProtocol.WebSocket,
			9093
		},
		{
			ConnectionProtocol.WebSocketSecure,
			19093
		}
	};

	public PhotonPortDefinition ServerPortOverrides;

	public string ProxyServerAddress;

	private ClientState state;

	public ConnectionCallbacksContainer ConnectionCallbackTargets;

	public MatchMakingCallbacksContainer MatchMakingCallbackTargets;

	internal InRoomCallbacksContainer InRoomCallbackTargets;

	internal LobbyCallbacksContainer LobbyCallbackTargets;

	internal WebRpcCallbacksContainer WebRpcCallbackTargets;

	internal ErrorInfoCallbacksContainer ErrorInfoCallbackTargets;

	public bool EnableLobbyStatistics;

	private readonly List<TypedLobbyInfo> lobbyStatistics = new List<TypedLobbyInfo>();

	private JoinType lastJoinType;

	private EnterRoomParams enterRoomParamsCache;

	private OperationResponse failedRoomEntryOperation;

	private const int FriendRequestListMax = 512;

	private string[] friendListRequested;

	public RegionHandler RegionHandler;

	private string bestRegionSummaryFromStorage;

	public string SummaryToCache;

	private bool connectToBestRegion = true;

	private readonly Queue<CallbackTargetChange> callbackTargetChanges = new Queue<CallbackTargetChange>();

	private readonly HashSet<object> callbackTargets = new HashSet<object>();

	public int NameServerPortInAppSettings;

	public LoadBalancingPeer LoadBalancingPeer { get; private set; }

	public SerializationProtocol SerializationProtocol
	{
		get
		{
			return LoadBalancingPeer.SerializationProtocolType;
		}
		set
		{
			LoadBalancingPeer.SerializationProtocolType = value;
		}
	}

	public string AppVersion { get; set; }

	public string AppId { get; set; }

	public ClientAppType ClientType { get; set; }

	public AuthenticationValues AuthValues { get; set; }

	public ConnectionProtocol? ExpectedProtocol { get; set; }

	private object TokenForInit
	{
		get
		{
			if (AuthMode == AuthModeOption.Auth)
			{
				return null;
			}
			if (AuthValues == null)
			{
				return null;
			}
			return AuthValues.Token;
		}
	}

	public bool IsUsingNameServer { get; set; }

	public string NameServerAddress => GetNameServerAddress();

	[Obsolete("Set port overrides in ServerPortOverrides. Not used anymore!")]
	public bool UseAlternativeUdpPorts { get; set; }

	public bool EnableProtocolFallback { get; set; }

	public string CurrentServerAddress => LoadBalancingPeer.ServerAddress;

	public string MasterServerAddress { get; set; }

	public string GameServerAddress { get; protected internal set; }

	public ServerConnection Server { get; private set; }

	public ClientState State
	{
		get
		{
			return state;
		}
		set
		{
			if (state != value)
			{
				ClientState arg = state;
				state = value;
				if (this.StateChanged != null)
				{
					this.StateChanged(arg, state);
				}
			}
		}
	}

	public bool IsConnected
	{
		get
		{
			if (LoadBalancingPeer != null && State != 0)
			{
				return State != ClientState.Disconnected;
			}
			return false;
		}
	}

	public bool IsConnectedAndReady
	{
		get
		{
			if (LoadBalancingPeer == null)
			{
				return false;
			}
			switch (State)
			{
			case ClientState.PeerCreated:
			case ClientState.Authenticating:
			case ClientState.DisconnectingFromMasterServer:
			case ClientState.ConnectingToGameServer:
			case ClientState.Joining:
			case ClientState.Leaving:
			case ClientState.DisconnectingFromGameServer:
			case ClientState.ConnectingToMasterServer:
			case ClientState.Disconnecting:
			case ClientState.Disconnected:
			case ClientState.ConnectingToNameServer:
			case ClientState.DisconnectingFromNameServer:
				return false;
			default:
				return true;
			}
		}
	}

	public DisconnectCause DisconnectedCause { get; protected set; }

	public bool InLobby => State == ClientState.JoinedLobby;

	public TypedLobby CurrentLobby { get; internal set; }

	public Player LocalPlayer { get; internal set; }

	public string NickName
	{
		get
		{
			return LocalPlayer.NickName;
		}
		set
		{
			if (LocalPlayer != null)
			{
				LocalPlayer.NickName = value;
			}
		}
	}

	public string UserId
	{
		get
		{
			if (AuthValues != null)
			{
				return AuthValues.UserId;
			}
			return null;
		}
		set
		{
			if (AuthValues == null)
			{
				AuthValues = new AuthenticationValues();
			}
			AuthValues.UserId = value;
		}
	}

	public Room CurrentRoom { get; set; }

	public bool InRoom
	{
		get
		{
			if (state == ClientState.Joined)
			{
				return CurrentRoom != null;
			}
			return false;
		}
	}

	public int PlayersOnMasterCount { get; internal set; }

	public int PlayersInRoomsCount { get; internal set; }

	public int RoomsCount { get; internal set; }

	public bool IsFetchingFriendList => friendListRequested != null;

	public string CloudRegion { get; private set; }

	public string CurrentCluster { get; private set; }

	public event Action<ClientState, ClientState> StateChanged;

	public event Action<EventData> EventReceived;

	public event Action<OperationResponse> OpResponseReceived;

	public LoadBalancingClient(ConnectionProtocol protocol = ConnectionProtocol.Udp)
	{
		ConnectionCallbackTargets = new ConnectionCallbacksContainer(this);
		MatchMakingCallbackTargets = new MatchMakingCallbacksContainer(this);
		InRoomCallbackTargets = new InRoomCallbacksContainer(this);
		LobbyCallbackTargets = new LobbyCallbacksContainer(this);
		WebRpcCallbackTargets = new WebRpcCallbacksContainer(this);
		ErrorInfoCallbackTargets = new ErrorInfoCallbacksContainer(this);
		LoadBalancingPeer = new LoadBalancingPeer(this, protocol);
		LoadBalancingPeer.OnDisconnectMessage += OnDisconnectMessageReceived;
		SerializationProtocol = SerializationProtocol.GpBinaryV18;
		LocalPlayer = CreatePlayer(string.Empty, -1, isLocal: true, null);
		CustomTypesUnity.Register();
		State = ClientState.PeerCreated;
	}

	public LoadBalancingClient(string masterAddress, string appId, string gameVersion, ConnectionProtocol protocol = ConnectionProtocol.Udp)
		: this(protocol)
	{
		MasterServerAddress = masterAddress;
		AppId = appId;
		AppVersion = gameVersion;
	}

	private string GetNameServerAddress()
	{
		int value = 0;
		ProtocolToNameServerPort.TryGetValue(LoadBalancingPeer.TransportProtocol, out value);
		if (NameServerPortInAppSettings != 0)
		{
			DebugReturn(DebugLevel.INFO, $"Using NameServerPortInAppSettings: {NameServerPortInAppSettings}");
			value = NameServerPortInAppSettings;
		}
		if (ServerPortOverrides.NameServerPort > 0)
		{
			value = ServerPortOverrides.NameServerPort;
		}
		switch (LoadBalancingPeer.TransportProtocol)
		{
		case ConnectionProtocol.Udp:
		case ConnectionProtocol.Tcp:
			return $"{NameServerHost}:{value}";
		case ConnectionProtocol.WebSocket:
			return $"ws://{NameServerHost}:{value}";
		case ConnectionProtocol.WebSocketSecure:
			return $"wss://{NameServerHost}:{value}";
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public virtual bool ConnectUsingSettings(AppSettings appSettings)
	{
		if (LoadBalancingPeer.PeerState != 0)
		{
			DebugReturn(DebugLevel.WARNING, "ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: " + LoadBalancingPeer.PeerState);
			return false;
		}
		if (appSettings == null)
		{
			DebugReturn(DebugLevel.ERROR, "ConnectUsingSettings failed. The appSettings can't be null.'");
			return false;
		}
		switch (ClientType)
		{
		case ClientAppType.Realtime:
			AppId = appSettings.AppIdRealtime;
			break;
		case ClientAppType.Voice:
			AppId = appSettings.AppIdVoice;
			break;
		case ClientAppType.Fusion:
			AppId = appSettings.AppIdFusion;
			break;
		}
		AppVersion = appSettings.AppVersion;
		IsUsingNameServer = appSettings.UseNameServer;
		CloudRegion = appSettings.FixedRegion;
		connectToBestRegion = string.IsNullOrEmpty(CloudRegion);
		EnableLobbyStatistics = appSettings.EnableLobbyStatistics;
		LoadBalancingPeer.DebugOut = appSettings.NetworkLogging;
		AuthMode = appSettings.AuthMode;
		if (appSettings.AuthMode == AuthModeOption.AuthOnceWss)
		{
			LoadBalancingPeer.TransportProtocol = ConnectionProtocol.WebSocketSecure;
			ExpectedProtocol = appSettings.Protocol;
		}
		else
		{
			LoadBalancingPeer.TransportProtocol = appSettings.Protocol;
			ExpectedProtocol = null;
		}
		EnableProtocolFallback = appSettings.EnableProtocolFallback;
		bestRegionSummaryFromStorage = appSettings.BestRegionSummaryFromStorage;
		DisconnectedCause = DisconnectCause.None;
		if (IsUsingNameServer)
		{
			Server = ServerConnection.NameServer;
			if (!appSettings.IsDefaultNameServer)
			{
				NameServerHost = appSettings.Server;
			}
			ProxyServerAddress = appSettings.ProxyServer;
			NameServerPortInAppSettings = appSettings.Port;
			if (!LoadBalancingPeer.Connect(NameServerAddress, ProxyServerAddress, AppId, TokenForInit))
			{
				return false;
			}
			State = ClientState.ConnectingToNameServer;
		}
		else
		{
			Server = ServerConnection.MasterServer;
			int num = (appSettings.IsDefaultPort ? 5055 : appSettings.Port);
			MasterServerAddress = $"{appSettings.Server}:{num}";
			if (!LoadBalancingPeer.Connect(MasterServerAddress, ProxyServerAddress, AppId, TokenForInit))
			{
				return false;
			}
			State = ClientState.ConnectingToMasterServer;
		}
		return true;
	}

	[Obsolete("Use ConnectToMasterServer() instead.")]
	public bool Connect()
	{
		return ConnectToMasterServer();
	}

	public virtual bool ConnectToMasterServer()
	{
		if (LoadBalancingPeer.PeerState != 0)
		{
			DebugReturn(DebugLevel.WARNING, "ConnectToMasterServer() failed. Can only connect while in state 'Disconnected'. Current state: " + LoadBalancingPeer.PeerState);
			return false;
		}
		if (AuthMode != 0 && TokenForInit == null)
		{
			DebugReturn(DebugLevel.ERROR, "Connect() failed. Can't connect to MasterServer with Token == null in AuthMode: " + AuthMode);
			return false;
		}
		if (LoadBalancingPeer.Connect(MasterServerAddress, ProxyServerAddress, AppId, TokenForInit))
		{
			DisconnectedCause = DisconnectCause.None;
			connectToBestRegion = false;
			State = ClientState.ConnectingToMasterServer;
			Server = ServerConnection.MasterServer;
			return true;
		}
		return false;
	}

	public bool ConnectToNameServer()
	{
		if (LoadBalancingPeer.PeerState != 0)
		{
			DebugReturn(DebugLevel.WARNING, "ConnectToNameServer() failed. Can only connect while in state 'Disconnected'. Current state: " + LoadBalancingPeer.PeerState);
			return false;
		}
		IsUsingNameServer = true;
		CloudRegion = null;
		if (AuthMode == AuthModeOption.AuthOnceWss)
		{
			if (!ExpectedProtocol.HasValue)
			{
				ExpectedProtocol = LoadBalancingPeer.TransportProtocol;
			}
			LoadBalancingPeer.TransportProtocol = ConnectionProtocol.WebSocketSecure;
		}
		if (LoadBalancingPeer.Connect(NameServerAddress, ProxyServerAddress, "NameServer", TokenForInit))
		{
			DisconnectedCause = DisconnectCause.None;
			connectToBestRegion = false;
			State = ClientState.ConnectingToNameServer;
			Server = ServerConnection.NameServer;
			return true;
		}
		return false;
	}

	public bool ConnectToRegionMaster(string region)
	{
		if (string.IsNullOrEmpty(region))
		{
			DebugReturn(DebugLevel.ERROR, "ConnectToRegionMaster() failed. The region can not be null or empty.");
			return false;
		}
		IsUsingNameServer = true;
		if (State == ClientState.Authenticating)
		{
			if ((int)LoadBalancingPeer.DebugOut >= 3)
			{
				DebugReturn(DebugLevel.INFO, "ConnectToRegionMaster() will skip calling authenticate, as the current state is 'Authenticating'. Just wait for the result.");
			}
			return true;
		}
		if (State == ClientState.ConnectedToNameServer)
		{
			CloudRegion = region;
			bool num = CallAuthenticate();
			if (num)
			{
				State = ClientState.Authenticating;
			}
			return num;
		}
		LoadBalancingPeer.Disconnect();
		if (!string.IsNullOrEmpty(region) && !region.Contains("/"))
		{
			region += "/*";
		}
		CloudRegion = region;
		if (AuthMode == AuthModeOption.AuthOnceWss)
		{
			if (!ExpectedProtocol.HasValue)
			{
				ExpectedProtocol = LoadBalancingPeer.TransportProtocol;
			}
			LoadBalancingPeer.TransportProtocol = ConnectionProtocol.WebSocketSecure;
		}
		connectToBestRegion = false;
		DisconnectedCause = DisconnectCause.None;
		if (!LoadBalancingPeer.Connect(NameServerAddress, ProxyServerAddress, "NameServer", null))
		{
			return false;
		}
		State = ClientState.ConnectingToNameServer;
		Server = ServerConnection.NameServer;
		return true;
	}

	[Conditional("UNITY_WEBGL")]
	private void CheckConnectSetupWebGl()
	{
	}

	private bool Connect(string serverAddress, string proxyServerAddress, ServerConnection serverType)
	{
		if (State == ClientState.Disconnecting)
		{
			DebugReturn(DebugLevel.ERROR, "Connect() failed. Can't connect while disconnecting (still). Current state: " + State);
			return false;
		}
		if (AuthMode != 0 && serverType != ServerConnection.NameServer && TokenForInit == null)
		{
			DebugReturn(DebugLevel.ERROR, "Connect() failed. Can't connect to " + serverType.ToString() + " with Token == null in AuthMode: " + AuthMode);
			return false;
		}
		bool flag = LoadBalancingPeer.Connect(serverAddress, proxyServerAddress, AppId, TokenForInit);
		if (flag)
		{
			DisconnectedCause = DisconnectCause.None;
			Server = serverType;
			switch (serverType)
			{
			case ServerConnection.NameServer:
				State = ClientState.ConnectingToNameServer;
				break;
			case ServerConnection.MasterServer:
				State = ClientState.ConnectingToMasterServer;
				break;
			case ServerConnection.GameServer:
				State = ClientState.ConnectingToGameServer;
				break;
			}
		}
		return flag;
	}

	public bool ReconnectToMaster()
	{
		if (LoadBalancingPeer.PeerState != 0)
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectToMaster() failed. Can only connect while in state 'Disconnected'. Current state: " + LoadBalancingPeer.PeerState);
			return false;
		}
		if (string.IsNullOrEmpty(MasterServerAddress))
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectToMaster() failed. MasterServerAddress is null or empty.");
			return false;
		}
		if (tokenCache == null)
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectToMaster() failed. It seems the client doesn't have any previous authentication token to re-connect.");
			return false;
		}
		if (AuthValues == null)
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectToMaster() with AuthValues == null is not correct!");
			AuthValues = new AuthenticationValues();
		}
		AuthValues.Token = tokenCache;
		return Connect(MasterServerAddress, ProxyServerAddress, ServerConnection.MasterServer);
	}

	public bool ReconnectAndRejoin()
	{
		if (LoadBalancingPeer.PeerState != 0)
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectAndRejoin() failed. Can only connect while in state 'Disconnected'. Current state: " + LoadBalancingPeer.PeerState);
			return false;
		}
		if (string.IsNullOrEmpty(GameServerAddress))
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectAndRejoin() failed. It seems the client wasn't connected to a game server before (no address).");
			return false;
		}
		if (enterRoomParamsCache == null)
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectAndRejoin() failed. It seems the client doesn't have any previous room to re-join.");
			return false;
		}
		if (tokenCache == null)
		{
			DebugReturn(DebugLevel.WARNING, "ReconnectAndRejoin() failed. It seems the client doesn't have any previous authentication token to re-connect.");
			return false;
		}
		if (AuthValues == null)
		{
			AuthValues = new AuthenticationValues();
		}
		AuthValues.Token = tokenCache;
		if (!string.IsNullOrEmpty(GameServerAddress) && enterRoomParamsCache != null)
		{
			lastJoinType = JoinType.JoinRoom;
			enterRoomParamsCache.JoinMode = JoinMode.RejoinOnly;
			return Connect(GameServerAddress, ProxyServerAddress, ServerConnection.GameServer);
		}
		return false;
	}

	public void Disconnect(DisconnectCause cause = DisconnectCause.DisconnectByClientLogic)
	{
		if (State == ClientState.Disconnecting || State == ClientState.PeerCreated)
		{
			DebugReturn(DebugLevel.INFO, "Disconnect() call gets skipped due to State " + State.ToString() + ". DisconnectedCause: " + DisconnectedCause.ToString() + " Parameter cause: " + cause);
		}
		else if (State != ClientState.Disconnected)
		{
			MatchMakingCallbackTargets.OnPreLeavingRoom();
			State = ClientState.Disconnecting;
			DisconnectedCause = cause;
			LoadBalancingPeer.Disconnect();
		}
	}

	private void DisconnectToReconnect()
	{
		switch (Server)
		{
		case ServerConnection.NameServer:
			State = ClientState.DisconnectingFromNameServer;
			break;
		case ServerConnection.MasterServer:
			State = ClientState.DisconnectingFromMasterServer;
			break;
		case ServerConnection.GameServer:
			State = ClientState.DisconnectingFromGameServer;
			break;
		}
		LoadBalancingPeer.Disconnect();
	}

	public void SimulateConnectionLoss(bool simulateTimeout)
	{
		DebugReturn(DebugLevel.WARNING, "SimulateConnectionLoss() set to: " + simulateTimeout);
		if (simulateTimeout)
		{
			LoadBalancingPeer.NetworkSimulationSettings.IncomingLossPercentage = 100;
			LoadBalancingPeer.NetworkSimulationSettings.OutgoingLossPercentage = 100;
		}
		LoadBalancingPeer.IsSimulationEnabled = simulateTimeout;
	}

	private bool CallAuthenticate()
	{
		if (IsUsingNameServer && Server != ServerConnection.NameServer && (AuthValues == null || AuthValues.Token == null))
		{
			DebugReturn(DebugLevel.ERROR, "Authenticate without Token is only allowed on Name Server. Connecting to: " + Server.ToString() + " on: " + CurrentServerAddress + ". State: " + State);
			return false;
		}
		if (AuthMode == AuthModeOption.Auth)
		{
			if (!CheckIfOpCanBeSent(230, Server, "Authenticate"))
			{
				return false;
			}
			return LoadBalancingPeer.OpAuthenticate(AppId, AppVersion, AuthValues, CloudRegion, EnableLobbyStatistics && Server == ServerConnection.MasterServer);
		}
		if (!CheckIfOpCanBeSent(231, Server, "AuthenticateOnce"))
		{
			return false;
		}
		ConnectionProtocol expectedProtocol = (ExpectedProtocol.HasValue ? ExpectedProtocol.Value : LoadBalancingPeer.TransportProtocol);
		return LoadBalancingPeer.OpAuthenticateOnce(AppId, AppVersion, AuthValues, CloudRegion, EncryptionMode, expectedProtocol);
	}

	public void Service()
	{
		if (LoadBalancingPeer != null)
		{
			LoadBalancingPeer.Service();
		}
	}

	private bool OpGetRegions()
	{
		if (!CheckIfOpCanBeSent(220, Server, "GetRegions"))
		{
			return false;
		}
		return LoadBalancingPeer.OpGetRegions(AppId);
	}

	public bool OpFindFriends(string[] friendsToFind, FindFriendsOptions options = null)
	{
		if (!CheckIfOpCanBeSent(222, Server, "FindFriends"))
		{
			return false;
		}
		if (IsFetchingFriendList)
		{
			DebugReturn(DebugLevel.WARNING, "OpFindFriends skipped: already fetching friends list.");
			return false;
		}
		if (friendsToFind == null || friendsToFind.Length == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpFindFriends skipped: friendsToFind array is null or empty.");
			return false;
		}
		if (friendsToFind.Length > 512)
		{
			DebugReturn(DebugLevel.ERROR, $"OpFindFriends skipped: friendsToFind array exceeds allowed length of {512}.");
			return false;
		}
		List<string> list = new List<string>(friendsToFind.Length);
		for (int i = 0; i < friendsToFind.Length; i++)
		{
			string text = friendsToFind[i];
			if (string.IsNullOrEmpty(text))
			{
				DebugReturn(DebugLevel.WARNING, $"friendsToFind array contains a null or empty UserId, element at position {i} skipped.");
			}
			else if (text.Equals(UserId))
			{
				DebugReturn(DebugLevel.WARNING, $"friendsToFind array contains local player's UserId \"{text}\", element at position {i} skipped.");
			}
			else if (list.Contains(text))
			{
				DebugReturn(DebugLevel.WARNING, $"friendsToFind array contains duplicate UserId \"{text}\", element at position {i} skipped.");
			}
			else
			{
				list.Add(text);
			}
		}
		if (list.Count == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpFindFriends skipped: friends list to find is empty.");
			return false;
		}
		string[] array = list.ToArray();
		bool flag = LoadBalancingPeer.OpFindFriends(array, options);
		friendListRequested = (flag ? array : null);
		return flag;
	}

	public bool OpJoinLobby(TypedLobby lobby)
	{
		if (!CheckIfOpCanBeSent(229, Server, "JoinLobby"))
		{
			return false;
		}
		if (lobby == null)
		{
			lobby = TypedLobby.Default;
		}
		bool num = LoadBalancingPeer.OpJoinLobby(lobby);
		if (num)
		{
			CurrentLobby = lobby;
			State = ClientState.JoiningLobby;
		}
		return num;
	}

	public bool OpLeaveLobby()
	{
		if (!CheckIfOpCanBeSent(228, Server, "LeaveLobby"))
		{
			return false;
		}
		return LoadBalancingPeer.OpLeaveLobby();
	}

	public bool OpJoinRandomRoom(OpJoinRandomRoomParams opJoinRandomRoomParams = null)
	{
		if (!CheckIfOpCanBeSent(225, Server, "JoinRandomGame"))
		{
			return false;
		}
		if (opJoinRandomRoomParams == null)
		{
			opJoinRandomRoomParams = new OpJoinRandomRoomParams();
		}
		enterRoomParamsCache = new EnterRoomParams();
		enterRoomParamsCache.Lobby = opJoinRandomRoomParams.TypedLobby;
		enterRoomParamsCache.ExpectedUsers = opJoinRandomRoomParams.ExpectedUsers;
		bool num = LoadBalancingPeer.OpJoinRandomRoom(opJoinRandomRoomParams);
		if (num)
		{
			lastJoinType = JoinType.JoinRandomRoom;
			State = ClientState.Joining;
		}
		return num;
	}

	public bool OpJoinRandomOrCreateRoom(OpJoinRandomRoomParams opJoinRandomRoomParams, EnterRoomParams createRoomParams)
	{
		if (!CheckIfOpCanBeSent(225, Server, "OpJoinRandomOrCreateRoom"))
		{
			return false;
		}
		if (opJoinRandomRoomParams == null)
		{
			opJoinRandomRoomParams = new OpJoinRandomRoomParams();
		}
		if (createRoomParams == null)
		{
			createRoomParams = new EnterRoomParams();
		}
		createRoomParams.JoinMode = JoinMode.CreateIfNotExists;
		enterRoomParamsCache = createRoomParams;
		enterRoomParamsCache.Lobby = opJoinRandomRoomParams.TypedLobby;
		enterRoomParamsCache.ExpectedUsers = opJoinRandomRoomParams.ExpectedUsers;
		bool num = LoadBalancingPeer.OpJoinRandomOrCreateRoom(opJoinRandomRoomParams, createRoomParams);
		if (num)
		{
			lastJoinType = JoinType.JoinRandomOrCreateRoom;
			State = ClientState.Joining;
		}
		return num;
	}

	public bool OpCreateRoom(EnterRoomParams enterRoomParams)
	{
		if (!CheckIfOpCanBeSent(227, Server, "CreateGame"))
		{
			return false;
		}
		if (!(enterRoomParams.OnGameServer = Server == ServerConnection.GameServer))
		{
			enterRoomParamsCache = enterRoomParams;
		}
		bool num = LoadBalancingPeer.OpCreateRoom(enterRoomParams);
		if (num)
		{
			lastJoinType = JoinType.CreateRoom;
			State = ClientState.Joining;
		}
		return num;
	}

	public bool OpJoinOrCreateRoom(EnterRoomParams enterRoomParams)
	{
		if (!CheckIfOpCanBeSent(226, Server, "JoinOrCreateRoom"))
		{
			return false;
		}
		bool flag = Server == ServerConnection.GameServer;
		enterRoomParams.JoinMode = JoinMode.CreateIfNotExists;
		enterRoomParams.OnGameServer = flag;
		if (!flag)
		{
			enterRoomParamsCache = enterRoomParams;
		}
		bool num = LoadBalancingPeer.OpJoinRoom(enterRoomParams);
		if (num)
		{
			lastJoinType = JoinType.JoinOrCreateRoom;
			State = ClientState.Joining;
		}
		return num;
	}

	public bool OpJoinRoom(EnterRoomParams enterRoomParams)
	{
		if (!CheckIfOpCanBeSent(226, Server, "JoinRoom"))
		{
			return false;
		}
		if (!(enterRoomParams.OnGameServer = Server == ServerConnection.GameServer))
		{
			enterRoomParamsCache = enterRoomParams;
		}
		bool num = LoadBalancingPeer.OpJoinRoom(enterRoomParams);
		if (num)
		{
			lastJoinType = ((enterRoomParams.JoinMode != JoinMode.CreateIfNotExists) ? JoinType.JoinRoom : JoinType.JoinOrCreateRoom);
			State = ClientState.Joining;
		}
		return num;
	}

	public bool OpRejoinRoom(string roomName)
	{
		if (!CheckIfOpCanBeSent(226, Server, "RejoinRoom"))
		{
			return false;
		}
		bool onGameServer = Server == ServerConnection.GameServer;
		EnterRoomParams enterRoomParams = (enterRoomParamsCache = new EnterRoomParams());
		enterRoomParams.RoomName = roomName;
		enterRoomParams.OnGameServer = onGameServer;
		enterRoomParams.JoinMode = JoinMode.RejoinOnly;
		bool num = LoadBalancingPeer.OpJoinRoom(enterRoomParams);
		if (num)
		{
			lastJoinType = JoinType.JoinRoom;
			State = ClientState.Joining;
		}
		return num;
	}

	public bool OpLeaveRoom(bool becomeInactive, bool sendAuthCookie = false)
	{
		if (!CheckIfOpCanBeSent(254, Server, "LeaveRoom"))
		{
			return false;
		}
		State = ClientState.Leaving;
		GameServerAddress = string.Empty;
		enterRoomParamsCache = null;
		return LoadBalancingPeer.OpLeaveRoom(becomeInactive, sendAuthCookie);
	}

	public bool OpGetGameList(TypedLobby typedLobby, string sqlLobbyFilter)
	{
		if (!CheckIfOpCanBeSent(217, Server, "GetGameList"))
		{
			return false;
		}
		if (string.IsNullOrEmpty(sqlLobbyFilter))
		{
			DebugReturn(DebugLevel.ERROR, "Operation GetGameList requires a filter.");
			return false;
		}
		if (typedLobby.Type != LobbyType.SqlLobby)
		{
			DebugReturn(DebugLevel.ERROR, "Operation GetGameList can only be used for lobbies of type SqlLobby.");
			return false;
		}
		return LoadBalancingPeer.OpGetGameList(typedLobby, sqlLobbyFilter);
	}

	public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
	{
		if (propertiesToSet == null || propertiesToSet.Count == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpSetCustomPropertiesOfActor() failed. propertiesToSet must not be null nor empty.");
			return false;
		}
		if (CurrentRoom == null)
		{
			if (expectedProperties == null && webFlags == null && LocalPlayer != null && LocalPlayer.ActorNumber == actorNr)
			{
				return LocalPlayer.SetCustomProperties(propertiesToSet);
			}
			if ((int)LoadBalancingPeer.DebugOut >= 1)
			{
				DebugReturn(DebugLevel.ERROR, "OpSetCustomPropertiesOfActor() failed. To use expectedProperties or webForward, you have to be in a room. State: " + State);
			}
			return false;
		}
		Hashtable hashtable = new Hashtable();
		hashtable.MergeStringKeys(propertiesToSet);
		if (hashtable.Count == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpSetCustomPropertiesOfActor() failed. Only string keys allowed for custom properties.");
			return false;
		}
		return OpSetPropertiesOfActor(actorNr, hashtable, expectedProperties, webFlags);
	}

	protected internal bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, Hashtable expectedProperties = null, WebFlags webFlags = null)
	{
		if (!CheckIfOpCanBeSent(252, Server, "SetProperties"))
		{
			return false;
		}
		if (actorProperties == null || actorProperties.Count == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpSetPropertiesOfActor() failed. actorProperties must not be null nor empty.");
			return false;
		}
		bool num = LoadBalancingPeer.OpSetPropertiesOfActor(actorNr, actorProperties, expectedProperties, webFlags);
		if (num && !CurrentRoom.BroadcastPropertiesChangeToAll && (expectedProperties == null || expectedProperties.Count == 0))
		{
			Player player = CurrentRoom.GetPlayer(actorNr);
			if (player != null)
			{
				player.InternalCacheProperties(actorProperties);
				InRoomCallbackTargets.OnPlayerPropertiesUpdate(player, actorProperties);
			}
		}
		return num;
	}

	public bool OpSetCustomPropertiesOfRoom(Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
	{
		if (propertiesToSet == null || propertiesToSet.Count == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpSetCustomPropertiesOfRoom() failed. propertiesToSet must not be null nor empty.");
			return false;
		}
		Hashtable hashtable = new Hashtable();
		hashtable.MergeStringKeys(propertiesToSet);
		if (hashtable.Count == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpSetCustomPropertiesOfRoom() failed. Only string keys are allowed for custom properties.");
			return false;
		}
		return OpSetPropertiesOfRoom(hashtable, expectedProperties, webFlags);
	}

	protected internal bool OpSetPropertyOfRoom(byte propCode, object value)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[propCode] = value;
		return OpSetPropertiesOfRoom(hashtable);
	}

	protected internal bool OpSetPropertiesOfRoom(Hashtable gameProperties, Hashtable expectedProperties = null, WebFlags webFlags = null)
	{
		if (!CheckIfOpCanBeSent(252, Server, "SetProperties"))
		{
			return false;
		}
		if (gameProperties == null || gameProperties.Count == 0)
		{
			DebugReturn(DebugLevel.ERROR, "OpSetPropertiesOfRoom() failed. gameProperties must not be null nor empty.");
			return false;
		}
		bool num = LoadBalancingPeer.OpSetPropertiesOfRoom(gameProperties, expectedProperties, webFlags);
		if (num && !CurrentRoom.BroadcastPropertiesChangeToAll && (expectedProperties == null || expectedProperties.Count == 0))
		{
			CurrentRoom.InternalCacheProperties(gameProperties);
			InRoomCallbackTargets.OnRoomPropertiesUpdate(gameProperties);
		}
		return num;
	}

	public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
	{
		if (!CheckIfOpCanBeSent(253, Server, "RaiseEvent"))
		{
			return false;
		}
		return LoadBalancingPeer.OpRaiseEvent(eventCode, customEventContent, raiseEventOptions, sendOptions);
	}

	public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
	{
		if (!CheckIfOpCanBeSent(248, Server, "ChangeGroups"))
		{
			return false;
		}
		return LoadBalancingPeer.OpChangeGroups(groupsToRemove, groupsToAdd);
	}

	private void ReadoutProperties(Hashtable gameProperties, Hashtable actorProperties, int targetActorNr)
	{
		if (CurrentRoom != null && gameProperties != null)
		{
			CurrentRoom.InternalCacheProperties(gameProperties);
			if (InRoom)
			{
				InRoomCallbackTargets.OnRoomPropertiesUpdate(gameProperties);
			}
		}
		if (actorProperties == null || actorProperties.Count <= 0)
		{
			return;
		}
		if (targetActorNr > 0)
		{
			Player player = CurrentRoom.GetPlayer(targetActorNr);
			if (player != null)
			{
				Hashtable hashtable = ReadoutPropertiesForActorNr(actorProperties, targetActorNr);
				player.InternalCacheProperties(hashtable);
				InRoomCallbackTargets.OnPlayerPropertiesUpdate(player, hashtable);
			}
			return;
		}
		foreach (object key in actorProperties.Keys)
		{
			int num = (int)key;
			if (num != 0)
			{
				Hashtable hashtable2 = (Hashtable)actorProperties[key];
				string actorName = (hashtable2[byte.MaxValue] as string) ?? string.Empty;
				Player player2 = CurrentRoom.GetPlayer(num);
				if (player2 == null)
				{
					player2 = CreatePlayer(actorName, num, isLocal: false, hashtable2);
					CurrentRoom.StorePlayer(player2);
				}
				player2.InternalCacheProperties(hashtable2);
			}
		}
	}

	private Hashtable ReadoutPropertiesForActorNr(Hashtable actorProperties, int actorNr)
	{
		if (actorProperties.ContainsKey(actorNr))
		{
			return (Hashtable)actorProperties[actorNr];
		}
		return actorProperties;
	}

	public void ChangeLocalID(int newID)
	{
		if (LocalPlayer == null)
		{
			DebugReturn(DebugLevel.WARNING, $"Local actor is null or not in mActors! mLocalActor: {LocalPlayer} mActors==null: {CurrentRoom.Players == null} newID: {newID}");
		}
		if (CurrentRoom == null)
		{
			LocalPlayer.ChangeLocalID(newID);
			LocalPlayer.RoomReference = null;
		}
		else
		{
			CurrentRoom.RemovePlayer(LocalPlayer);
			LocalPlayer.ChangeLocalID(newID);
			CurrentRoom.StorePlayer(LocalPlayer);
		}
	}

	private void GameEnteredOnGameServer(OperationResponse operationResponse)
	{
		CurrentRoom = CreateRoom(enterRoomParamsCache.RoomName, enterRoomParamsCache.RoomOptions);
		CurrentRoom.LoadBalancingClient = this;
		int newID = (int)operationResponse[254];
		ChangeLocalID(newID);
		if (operationResponse.Parameters.ContainsKey(252))
		{
			int[] actorsInGame = (int[])operationResponse.Parameters[252];
			UpdatedActorList(actorsInGame);
			if (LoadBalancingPeer.enterRoomParams != null)
			{
				if (LoadBalancingPeer.enterRoomParams.RoomOptions != null)
				{
					if (LoadBalancingPeer.enterRoomParams.RoomOptions.IsVisible)
					{
						DebugReturn(DebugLevel.INFO, $"Room option IsVisible is true");
					}
					else
					{
						DebugReturn(DebugLevel.INFO, $"Room option IsVisible is false");
					}
					if (LoadBalancingPeer.enterRoomParams.RoomOptions.IsOpen)
					{
						DebugReturn(DebugLevel.INFO, $"Room option IsOpen is true");
					}
					else
					{
						DebugReturn(DebugLevel.INFO, $"Room option IsOpen is false");
					}
					if (LoadBalancingPeer.enterRoomParams.RoomOptions.DeleteNullProperties)
					{
						DebugReturn(DebugLevel.INFO, $"Room option DeleteNullProperties is true");
					}
					else
					{
						DebugReturn(DebugLevel.INFO, $"Room option DeleteNullProperties is false");
					}
				}
				WebFlags webFlags = new WebFlags(1);
				Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
				dictionary.Add(244, (byte)195);
				dictionary.Add(234, webFlags.WebhookFlags);
				LoadBalancingPeer.SendOperation(253, dictionary, SendOptions.SendReliable);
			}
		}
		Hashtable actorProperties = (Hashtable)operationResponse[249];
		Hashtable gameProperties = (Hashtable)operationResponse[248];
		ReadoutProperties(gameProperties, actorProperties, 0);
		if (operationResponse.Parameters.TryGetValue(191, out var value))
		{
			CurrentRoom.InternalCacheRoomFlags((int)value);
		}
		State = ClientState.Joined;
		if (CurrentRoom.SuppressRoomEvents)
		{
			if (lastJoinType == JoinType.CreateRoom || (lastJoinType == JoinType.JoinOrCreateRoom && LocalPlayer.ActorNumber == 1))
			{
				MatchMakingCallbackTargets.OnCreatedRoom();
			}
			MatchMakingCallbackTargets.OnJoinedRoom();
		}
	}

	private void UpdatedActorList(int[] actorsInGame)
	{
		if (actorsInGame == null)
		{
			return;
		}
		foreach (int num in actorsInGame)
		{
			if (num != 0 && CurrentRoom.GetPlayer(num) == null)
			{
				CurrentRoom.StorePlayer(CreatePlayer(string.Empty, num, isLocal: false, null));
			}
		}
	}

	protected internal virtual Player CreatePlayer(string actorName, int actorNumber, bool isLocal, Hashtable actorProperties)
	{
		return new Player(actorName, actorNumber, isLocal, actorProperties);
	}

	protected internal virtual Room CreateRoom(string roomName, RoomOptions opt)
	{
		return new Room(roomName, opt);
	}

	private bool CheckIfOpAllowedOnServer(byte opCode, ServerConnection serverConnection)
	{
		switch (serverConnection)
		{
		case ServerConnection.MasterServer:
			switch (opCode)
			{
			case 217:
			case 218:
			case 219:
			case 221:
			case 222:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
				return true;
			}
			break;
		case ServerConnection.GameServer:
			switch (opCode)
			{
			case 218:
			case 219:
			case 226:
			case 227:
			case 230:
			case 231:
			case 248:
			case 251:
			case 252:
			case 253:
			case 254:
				return true;
			}
			break;
		case ServerConnection.NameServer:
			if (opCode == 218 || opCode == 220 || (uint)(opCode - 230) <= 1u)
			{
				return true;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("serverConnection", serverConnection, null);
		}
		return false;
	}

	private bool CheckIfOpCanBeSent(byte opCode, ServerConnection serverConnection, string opName)
	{
		if (LoadBalancingPeer == null)
		{
			DebugReturn(DebugLevel.ERROR, $"Operation {opName} ({opCode}) can't be sent because peer is null");
			return false;
		}
		if (!CheckIfOpAllowedOnServer(opCode, serverConnection))
		{
			if ((int)LoadBalancingPeer.DebugOut >= 1)
			{
				DebugReturn(DebugLevel.ERROR, $"Operation {opName} ({opCode}) not allowed on current server ({serverConnection})");
			}
			return false;
		}
		if (!CheckIfClientIsReadyToCallOperation(opCode))
		{
			DebugLevel debugLevel = DebugLevel.ERROR;
			if (opCode == 253 && (State == ClientState.Leaving || State == ClientState.Disconnecting || State == ClientState.DisconnectingFromGameServer))
			{
				debugLevel = DebugLevel.INFO;
			}
			if ((int)LoadBalancingPeer.DebugOut >= (int)debugLevel)
			{
				DebugReturn(debugLevel, $"Operation {opName} ({opCode}) not called because client is not connected or not ready yet, client state: {Enum.GetName(typeof(ClientState), State)}");
			}
			return false;
		}
		if (LoadBalancingPeer.PeerState != PeerStateValue.Connected)
		{
			DebugReturn(DebugLevel.ERROR, $"Operation {opName} ({opCode}) can't be sent because peer is not connected, peer state: {LoadBalancingPeer.PeerState}");
			return false;
		}
		return true;
	}

	private bool CheckIfClientIsReadyToCallOperation(byte opCode)
	{
		switch (opCode)
		{
		case 230:
		case 231:
			if (!IsConnectedAndReady && State != ClientState.ConnectingToNameServer && State != ClientState.ConnectingToMasterServer)
			{
				return State == ClientState.ConnectingToGameServer;
			}
			return true;
		case 248:
		case 251:
		case 252:
		case 253:
		case 254:
			return InRoom;
		case 226:
		case 227:
			if (State != ClientState.ConnectedToMasterServer && !InLobby)
			{
				return State == ClientState.ConnectedToGameServer;
			}
			return true;
		case 228:
			return InLobby;
		case 222:
			LoadBalancingPeer.enterRoomParams = new EnterRoomParams();
			goto case 217;
		case 217:
		case 221:
		case 225:
		case 229:
			if (State != ClientState.ConnectedToMasterServer)
			{
				return InLobby;
			}
			return true;
		case 220:
			return State == ClientState.ConnectedToNameServer;
		default:
			return IsConnected;
		}
	}

	public virtual void DebugReturn(DebugLevel level, string message)
	{
		if (LoadBalancingPeer.DebugOut == DebugLevel.ALL || (int)level <= (int)LoadBalancingPeer.DebugOut)
		{
			switch (level)
			{
			case DebugLevel.ERROR:
				UnityEngine.Debug.LogError(message);
				break;
			case DebugLevel.WARNING:
				UnityEngine.Debug.LogWarning(message);
				break;
			case DebugLevel.INFO:
				UnityEngine.Debug.Log(message);
				break;
			case DebugLevel.ALL:
				UnityEngine.Debug.Log(message);
				break;
			}
		}
	}

	private void CallbackRoomEnterFailed(OperationResponse operationResponse)
	{
		if (operationResponse.ReturnCode != 0)
		{
			if (operationResponse.OperationCode == 226)
			{
				MatchMakingCallbackTargets.OnJoinRoomFailed(operationResponse.ReturnCode, operationResponse.DebugMessage);
			}
			else if (operationResponse.OperationCode == 227)
			{
				MatchMakingCallbackTargets.OnCreateRoomFailed(operationResponse.ReturnCode, operationResponse.DebugMessage);
			}
			else if (operationResponse.OperationCode == 225)
			{
				MatchMakingCallbackTargets.OnJoinRandomFailed(operationResponse.ReturnCode, operationResponse.DebugMessage);
			}
		}
	}

	public virtual void OnOperationResponse(OperationResponse operationResponse)
	{
		if (operationResponse.Parameters.ContainsKey(221))
		{
			if (AuthValues == null)
			{
				AuthValues = new AuthenticationValues();
			}
			AuthValues.Token = operationResponse[221] as string;
			tokenCache = AuthValues.Token;
		}
		if (operationResponse.ReturnCode == 32743)
		{
			Disconnect(DisconnectCause.DisconnectByOperationLimit);
		}
		switch (operationResponse.OperationCode)
		{
		case 230:
		case 231:
		{
			if (operationResponse.ReturnCode != 0)
			{
				DebugReturn(DebugLevel.WARNING, operationResponse.ToStringFull() + " Server: " + Server.ToString() + " Address: " + LoadBalancingPeer.ServerAddress);
				switch (operationResponse.ReturnCode)
				{
				case short.MaxValue:
					DisconnectedCause = DisconnectCause.InvalidAuthentication;
					break;
				case 32755:
					DisconnectedCause = DisconnectCause.CustomAuthenticationFailed;
					ConnectionCallbackTargets.OnCustomAuthenticationFailed(operationResponse.DebugMessage);
					break;
				case 32756:
					DisconnectedCause = DisconnectCause.InvalidRegion;
					break;
				case 32757:
					DisconnectedCause = DisconnectCause.MaxCcuReached;
					break;
				case -3:
				case -2:
					DisconnectedCause = DisconnectCause.OperationNotAllowedInCurrentState;
					break;
				case 32753:
					DisconnectedCause = DisconnectCause.AuthenticationTicketExpired;
					break;
				}
				Disconnect(DisconnectedCause);
				break;
			}
			if (Server == ServerConnection.NameServer || Server == ServerConnection.MasterServer)
			{
				if (operationResponse.Parameters.ContainsKey(225))
				{
					string text3 = (string)operationResponse.Parameters[225];
					if (!string.IsNullOrEmpty(text3))
					{
						UserId = text3;
						LocalPlayer.UserId = text3;
						DebugReturn(DebugLevel.INFO, $"Received your UserID from server. Updating local value to: {UserId}");
					}
				}
				if (operationResponse.Parameters.ContainsKey(202))
				{
					NickName = (string)operationResponse.Parameters[202];
					DebugReturn(DebugLevel.INFO, $"Received your NickName from server. Updating local value to: {NickName}");
				}
				if (operationResponse.Parameters.ContainsKey(192))
				{
					SetupEncryption((Dictionary<byte, object>)operationResponse.Parameters[192]);
				}
			}
			if (Server == ServerConnection.NameServer)
			{
				string text4 = operationResponse[196] as string;
				if (!string.IsNullOrEmpty(text4))
				{
					CurrentCluster = text4;
				}
				MasterServerAddress = operationResponse[230] as string;
				if (ServerPortOverrides.MasterServerPort != 0)
				{
					MasterServerAddress = ReplacePortWithAlternative(MasterServerAddress, ServerPortOverrides.MasterServerPort);
				}
				if (AuthMode == AuthModeOption.AuthOnceWss && ExpectedProtocol.HasValue)
				{
					DebugReturn(DebugLevel.INFO, $"AuthOnceWss mode. Auth response switches TransportProtocol to ExpectedProtocol: {ExpectedProtocol}.");
					LoadBalancingPeer.TransportProtocol = ExpectedProtocol.Value;
					ExpectedProtocol = null;
				}
				DisconnectToReconnect();
			}
			else if (Server == ServerConnection.MasterServer)
			{
				State = ClientState.ConnectedToMasterServer;
				if (failedRoomEntryOperation == null)
				{
					ConnectionCallbackTargets.OnConnectedToMaster();
				}
				else
				{
					CallbackRoomEnterFailed(failedRoomEntryOperation);
					failedRoomEntryOperation = null;
				}
				if (AuthMode != 0)
				{
					LoadBalancingPeer.OpSettings(EnableLobbyStatistics);
				}
			}
			else if (Server == ServerConnection.GameServer)
			{
				State = ClientState.Joining;
				if (enterRoomParamsCache.JoinMode == JoinMode.RejoinOnly)
				{
					enterRoomParamsCache.PlayerProperties = null;
				}
				else
				{
					Hashtable hashtable2 = new Hashtable();
					hashtable2.Merge(LocalPlayer.CustomProperties);
					if (!string.IsNullOrEmpty(LocalPlayer.NickName))
					{
						hashtable2[byte.MaxValue] = LocalPlayer.NickName;
					}
					enterRoomParamsCache.PlayerProperties = hashtable2;
				}
				enterRoomParamsCache.OnGameServer = true;
				if (lastJoinType == JoinType.JoinRoom || lastJoinType == JoinType.JoinRandomRoom || lastJoinType == JoinType.JoinRandomOrCreateRoom || lastJoinType == JoinType.JoinOrCreateRoom)
				{
					LoadBalancingPeer.OpJoinRoom(enterRoomParamsCache);
				}
				else if (lastJoinType == JoinType.CreateRoom)
				{
					LoadBalancingPeer.OpCreateRoom(enterRoomParamsCache);
				}
				break;
			}
			Dictionary<string, object> dictionary = (Dictionary<string, object>)operationResponse[245];
			if (dictionary != null)
			{
				ConnectionCallbackTargets.OnCustomAuthenticationResponse(dictionary);
			}
			break;
		}
		case 220:
			if (operationResponse.ReturnCode == short.MaxValue)
			{
				DebugReturn(DebugLevel.ERROR, string.Format("GetRegions failed. AppId is unknown on the (cloud) server. " + operationResponse.DebugMessage));
				Disconnect(DisconnectCause.InvalidAuthentication);
				break;
			}
			if (operationResponse.ReturnCode != 0)
			{
				DebugReturn(DebugLevel.ERROR, "GetRegions failed. Can't provide regions list. ReturnCode: " + operationResponse.ReturnCode + ": " + operationResponse.DebugMessage);
				Disconnect(DisconnectCause.InvalidAuthentication);
				break;
			}
			if (RegionHandler == null)
			{
				RegionHandler = new RegionHandler(ServerPortOverrides.MasterServerPort);
			}
			if (RegionHandler.IsPinging)
			{
				DebugReturn(DebugLevel.WARNING, "Received an response for OpGetRegions while the RegionHandler is pinging regions already. Skipping this response in favor of completing the current region-pinging.");
				return;
			}
			RegionHandler.SetRegions(operationResponse);
			ConnectionCallbackTargets.OnRegionListReceived(RegionHandler);
			if (connectToBestRegion)
			{
				RegionHandler.PingMinimumOfRegions(OnRegionPingCompleted, bestRegionSummaryFromStorage);
			}
			break;
		case 225:
		case 226:
		case 227:
		{
			if (operationResponse.ReturnCode != 0)
			{
				if (Server == ServerConnection.GameServer)
				{
					failedRoomEntryOperation = operationResponse;
					DisconnectToReconnect();
				}
				else
				{
					State = (InLobby ? ClientState.JoinedLobby : ClientState.ConnectedToMasterServer);
					CallbackRoomEnterFailed(operationResponse);
				}
				break;
			}
			if (Server == ServerConnection.GameServer)
			{
				GameEnteredOnGameServer(operationResponse);
				break;
			}
			GameServerAddress = (string)operationResponse[230];
			if (ServerPortOverrides.GameServerPort != 0)
			{
				GameServerAddress = ReplacePortWithAlternative(GameServerAddress, ServerPortOverrides.GameServerPort);
			}
			string text2 = operationResponse[byte.MaxValue] as string;
			if (!string.IsNullOrEmpty(text2))
			{
				enterRoomParamsCache.RoomName = text2;
			}
			DisconnectToReconnect();
			break;
		}
		case 217:
		{
			if (operationResponse.ReturnCode != 0)
			{
				DebugReturn(DebugLevel.ERROR, "GetGameList failed: " + operationResponse.ToStringFull());
				break;
			}
			List<RoomInfo> list2 = new List<RoomInfo>();
			Hashtable hashtable = (Hashtable)operationResponse[222];
			foreach (string key in hashtable.Keys)
			{
				list2.Add(new RoomInfo(key, (Hashtable)hashtable[key]));
			}
			LobbyCallbackTargets.OnRoomListUpdate(list2);
			break;
		}
		case 229:
			State = ClientState.JoinedLobby;
			LobbyCallbackTargets.OnJoinedLobby();
			break;
		case 228:
			State = ClientState.ConnectedToMasterServer;
			LobbyCallbackTargets.OnLeftLobby();
			break;
		case 254:
			DisconnectToReconnect();
			break;
		case 223:
		{
			if (operationResponse.ReturnCode != 0)
			{
				DebugReturn(DebugLevel.ERROR, "OpFindFriends failed: " + operationResponse.ToStringFull());
				friendListRequested = null;
				break;
			}
			bool[] array = operationResponse[1] as bool[];
			string[] array2 = operationResponse[2] as string[];
			List<FriendInfo> list = new List<FriendInfo>(friendListRequested.Length);
			for (int i = 0; i < friendListRequested.Length; i++)
			{
				FriendInfo friendInfo = new FriendInfo();
				friendInfo.UserId = friendListRequested[i];
				friendInfo.Room = array2[i];
				friendInfo.IsOnline = array[i];
				list.Insert(i, friendInfo);
			}
			friendListRequested = null;
			MatchMakingCallbackTargets.OnFriendListUpdate(list);
			break;
		}
		case 219:
			WebRpcCallbackTargets.OnWebRpcResponse(operationResponse);
			break;
		}
		if (this.OpResponseReceived != null)
		{
			this.OpResponseReceived(operationResponse);
		}
	}

	public virtual void OnStatusChanged(StatusCode statusCode)
	{
		switch (statusCode)
		{
		case StatusCode.Connect:
			if (State == ClientState.ConnectingToNameServer)
			{
				if ((int)LoadBalancingPeer.DebugOut >= 5)
				{
					DebugReturn(DebugLevel.ALL, "Connected to nameserver.");
				}
				Server = ServerConnection.NameServer;
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
			}
			if (State == ClientState.ConnectingToGameServer)
			{
				if ((int)LoadBalancingPeer.DebugOut >= 5)
				{
					DebugReturn(DebugLevel.ALL, "Connected to gameserver.");
				}
				Server = ServerConnection.GameServer;
			}
			if (State == ClientState.ConnectingToMasterServer)
			{
				if ((int)LoadBalancingPeer.DebugOut >= 5)
				{
					DebugReturn(DebugLevel.ALL, "Connected to masterserver.");
				}
				Server = ServerConnection.MasterServer;
				ConnectionCallbackTargets.OnConnected();
			}
			if (LoadBalancingPeer.TransportProtocol != ConnectionProtocol.WebSocketSecure)
			{
				if (Server == ServerConnection.NameServer || AuthMode == AuthModeOption.Auth)
				{
					LoadBalancingPeer.EstablishEncryption();
				}
				break;
			}
			goto case StatusCode.EncryptionEstablished;
		case StatusCode.EncryptionEstablished:
			if (Server == ServerConnection.NameServer)
			{
				State = ClientState.ConnectedToNameServer;
				if (string.IsNullOrEmpty(CloudRegion))
				{
					OpGetRegions();
					break;
				}
			}
			else if (AuthMode == AuthModeOption.AuthOnce || AuthMode == AuthModeOption.AuthOnceWss)
			{
				break;
			}
			if (CallAuthenticate())
			{
				State = ClientState.Authenticating;
			}
			else
			{
				DebugReturn(DebugLevel.ERROR, "OpAuthenticate failed. Check log output and AuthValues. State: " + State);
			}
			break;
		case StatusCode.Disconnect:
		{
			friendListRequested = null;
			bool flag = CurrentRoom != null;
			CurrentRoom = null;
			ChangeLocalID(-1);
			if (Server == ServerConnection.GameServer && flag)
			{
				MatchMakingCallbackTargets.OnLeftRoom();
			}
			if (ExpectedProtocol.HasValue && LoadBalancingPeer.TransportProtocol != ExpectedProtocol)
			{
				DebugReturn(DebugLevel.INFO, $"On disconnect switches TransportProtocol to ExpectedProtocol: {ExpectedProtocol}.");
				LoadBalancingPeer.TransportProtocol = ExpectedProtocol.Value;
				ExpectedProtocol = null;
			}
			switch (State)
			{
			case ClientState.ConnectWithFallbackProtocol:
				EnableProtocolFallback = false;
				LoadBalancingPeer.TransportProtocol = ((LoadBalancingPeer.TransportProtocol != ConnectionProtocol.Tcp) ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp);
				NameServerPortInAppSettings = 0;
				ServerPortOverrides = default(PhotonPortDefinition);
				if (LoadBalancingPeer.Connect(NameServerAddress, ProxyServerAddress, AppId, TokenForInit))
				{
					State = ClientState.ConnectingToNameServer;
				}
				break;
			case ClientState.PeerCreated:
			case ClientState.Disconnecting:
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			case ClientState.DisconnectingFromGameServer:
			case ClientState.DisconnectingFromNameServer:
				ConnectToMasterServer();
				break;
			case ClientState.DisconnectingFromMasterServer:
				Connect(GameServerAddress, ProxyServerAddress, ServerConnection.GameServer);
				break;
			default:
			{
				string text = "";
				DebugReturn(DebugLevel.WARNING, "Got a unexpected Disconnect in LoadBalancingClient State: " + State.ToString() + ". Server: " + Server.ToString() + " Trace: " + text);
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			}
			case ClientState.Disconnected:
				break;
			}
			break;
		}
		case StatusCode.DisconnectByServerUserLimit:
			DebugReturn(DebugLevel.ERROR, "This connection was rejected due to the apps CCU limit.");
			DisconnectedCause = DisconnectCause.MaxCcuReached;
			State = ClientState.Disconnecting;
			break;
		case StatusCode.DnsExceptionOnConnect:
			DisconnectedCause = DisconnectCause.DnsExceptionOnConnect;
			State = ClientState.Disconnecting;
			break;
		case StatusCode.ServerAddressInvalid:
			DisconnectedCause = DisconnectCause.ServerAddressInvalid;
			State = ClientState.Disconnecting;
			break;
		case StatusCode.SecurityExceptionOnConnect:
		case StatusCode.ExceptionOnConnect:
		case StatusCode.EncryptionFailedToEstablish:
			DisconnectedCause = DisconnectCause.ExceptionOnConnect;
			if (EnableProtocolFallback && State == ClientState.ConnectingToNameServer)
			{
				State = ClientState.ConnectWithFallbackProtocol;
			}
			else
			{
				State = ClientState.Disconnecting;
			}
			break;
		case StatusCode.Exception:
		case StatusCode.SendError:
		case StatusCode.ExceptionOnReceive:
			DisconnectedCause = DisconnectCause.Exception;
			State = ClientState.Disconnecting;
			break;
		case StatusCode.DisconnectByServerTimeout:
			DisconnectedCause = DisconnectCause.ServerTimeout;
			State = ClientState.Disconnecting;
			break;
		case StatusCode.DisconnectByServerLogic:
			DisconnectedCause = DisconnectCause.DisconnectByServerLogic;
			State = ClientState.Disconnecting;
			break;
		case StatusCode.DisconnectByServerReasonUnknown:
			DisconnectedCause = DisconnectCause.DisconnectByServerReasonUnknown;
			State = ClientState.Disconnecting;
			break;
		case StatusCode.TimeoutDisconnect:
			DisconnectedCause = DisconnectCause.ClientTimeout;
			if (EnableProtocolFallback && State == ClientState.ConnectingToNameServer)
			{
				State = ClientState.ConnectWithFallbackProtocol;
			}
			else
			{
				State = ClientState.Disconnecting;
			}
			break;
		case (StatusCode)1027:
		case (StatusCode)1028:
		case (StatusCode)1029:
		case (StatusCode)1031:
		case (StatusCode)1032:
		case (StatusCode)1033:
		case (StatusCode)1034:
		case (StatusCode)1035:
		case (StatusCode)1036:
		case (StatusCode)1037:
		case (StatusCode)1038:
		case (StatusCode)1045:
		case (StatusCode)1046:
		case (StatusCode)1047:
			break;
		}
	}

	public virtual void OnEvent(EventData photonEvent)
	{
		int sender = photonEvent.Sender;
		Player player = ((CurrentRoom != null) ? CurrentRoom.GetPlayer(sender) : null);
		switch (photonEvent.Code)
		{
		case 229:
		case 230:
		{
			List<RoomInfo> list = new List<RoomInfo>();
			Hashtable hashtable4 = (Hashtable)photonEvent[222];
			foreach (string key in hashtable4.Keys)
			{
				list.Add(new RoomInfo(key, (Hashtable)hashtable4[key]));
			}
			LobbyCallbackTargets.OnRoomListUpdate(list);
			break;
		}
		case byte.MaxValue:
		{
			Hashtable hashtable = (Hashtable)photonEvent[249];
			if (player == null)
			{
				if (sender > 0)
				{
					player = CreatePlayer(string.Empty, sender, isLocal: false, hashtable);
					CurrentRoom.StorePlayer(player);
				}
			}
			else
			{
				player.InternalCacheProperties(hashtable);
				player.IsInactive = false;
				player.HasRejoined = sender != LocalPlayer.ActorNumber;
			}
			if (sender == LocalPlayer.ActorNumber)
			{
				int[] actorsInGame = (int[])photonEvent[252];
				UpdatedActorList(actorsInGame);
				player.HasRejoined = enterRoomParamsCache.JoinMode == JoinMode.RejoinOnly;
				if (lastJoinType == JoinType.CreateRoom || (lastJoinType == JoinType.JoinOrCreateRoom && LocalPlayer.ActorNumber == 1))
				{
					MatchMakingCallbackTargets.OnCreatedRoom();
				}
				MatchMakingCallbackTargets.OnJoinedRoom();
			}
			else
			{
				InRoomCallbackTargets.OnPlayerEnteredRoom(player);
			}
			break;
		}
		case 254:
			if (player != null)
			{
				bool flag2 = false;
				if (photonEvent.Parameters.ContainsKey(233))
				{
					flag2 = (bool)photonEvent.Parameters[233];
				}
				if (flag2)
				{
					player.IsInactive = true;
				}
				else
				{
					player.IsInactive = false;
					CurrentRoom.RemovePlayer(sender);
				}
			}
			if (photonEvent.Parameters.ContainsKey(203))
			{
				int num3 = (int)photonEvent[203];
				if (num3 != 0)
				{
					CurrentRoom.masterClientId = num3;
					InRoomCallbackTargets.OnMasterClientSwitched(CurrentRoom.GetPlayer(num3));
				}
			}
			InRoomCallbackTargets.OnPlayerLeftRoom(player);
			break;
		case 253:
		{
			int num = 0;
			if (photonEvent.Parameters.ContainsKey(253))
			{
				if (!(photonEvent[253] is int num2))
				{
					return;
				}
				num = num2;
			}
			Hashtable gameProperties = null;
			Hashtable actorProperties = null;
			if (num == 0)
			{
				if (!(photonEvent[251] is Hashtable hashtable2))
				{
					return;
				}
				gameProperties = hashtable2;
			}
			else
			{
				if (!(photonEvent[251] is Hashtable hashtable3))
				{
					return;
				}
				actorProperties = hashtable3;
			}
			ReadoutProperties(gameProperties, actorProperties, num);
			break;
		}
		case 226:
			PlayersInRoomsCount = (int)photonEvent[229];
			RoomsCount = (int)photonEvent[228];
			PlayersOnMasterCount = (int)photonEvent[227];
			break;
		case 224:
		{
			string[] array = photonEvent[213] as string[];
			int[] array2 = photonEvent[229] as int[];
			int[] array3 = photonEvent[228] as int[];
			ByteArraySlice byteArraySlice = photonEvent[212] as ByteArraySlice;
			bool flag = byteArraySlice != null;
			byte[] array4 = ((!flag) ? (photonEvent[212] as byte[]) : byteArraySlice.Buffer);
			lobbyStatistics.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				TypedLobbyInfo typedLobbyInfo = new TypedLobbyInfo();
				typedLobbyInfo.Name = array[i];
				typedLobbyInfo.Type = (LobbyType)array4[i];
				typedLobbyInfo.PlayerCount = array2[i];
				typedLobbyInfo.RoomCount = array3[i];
				lobbyStatistics.Add(typedLobbyInfo);
			}
			if (flag)
			{
				byteArraySlice.Release();
			}
			LobbyCallbackTargets.OnLobbyStatisticsUpdate(lobbyStatistics);
			break;
		}
		case 251:
			ErrorInfoCallbackTargets.OnErrorInfo(new ErrorInfo(photonEvent));
			break;
		case 223:
			if (AuthValues == null)
			{
				AuthValues = new AuthenticationValues();
			}
			AuthValues.Token = photonEvent[221] as string;
			tokenCache = AuthValues.Token;
			break;
		}
		UpdateCallbackTargets();
		if (this.EventReceived != null)
		{
			this.EventReceived(photonEvent);
		}
	}

	public virtual void OnMessage(object message)
	{
		DebugReturn(DebugLevel.ALL, $"got OnMessage {message}");
	}

	private void OnDisconnectMessageReceived(DisconnectMessage obj)
	{
		DebugReturn(DebugLevel.ERROR, $"Got DisconnectMessage. Code: {obj.Code} Msg: \"{obj.DebugMessage}\". Debug Info: {obj.Parameters.ToStringFull()}");
		Disconnect(DisconnectCause.DisconnectByDisconnectMessage);
	}

	private void OnRegionPingCompleted(RegionHandler regionHandler)
	{
		SummaryToCache = regionHandler.SummaryToCache;
		ConnectToRegionMaster(regionHandler.BestRegion.Code);
	}

	protected internal static string ReplacePortWithAlternative(string address, ushort replacementPort)
	{
		if (address.StartsWith("ws"))
		{
			return new UriBuilder(address)
			{
				Port = replacementPort
			}.ToString();
		}
		UriBuilder uriBuilder = new UriBuilder($"scheme://{address}");
		return $"{uriBuilder.Host}:{replacementPort}";
	}

	private void SetupEncryption(Dictionary<byte, object> encryptionData)
	{
		EncryptionMode encryptionMode = (EncryptionMode)(byte)encryptionData[0];
		switch (encryptionMode)
		{
		case EncryptionMode.PayloadEncryption:
		{
			byte[] secret = (byte[])encryptionData[1];
			LoadBalancingPeer.InitPayloadEncryption(secret);
			break;
		}
		case EncryptionMode.DatagramEncryption:
		case EncryptionMode.DatagramEncryptionRandomSequence:
		{
			byte[] encryptionSecret2 = (byte[])encryptionData[1];
			byte[] hmacSecret = (byte[])encryptionData[2];
			LoadBalancingPeer.InitDatagramEncryption(encryptionSecret2, hmacSecret, encryptionMode == EncryptionMode.DatagramEncryptionRandomSequence);
			break;
		}
		case EncryptionMode.DatagramEncryptionGCM:
		{
			byte[] encryptionSecret = (byte[])encryptionData[1];
			LoadBalancingPeer.InitDatagramEncryption(encryptionSecret, null, randomizedSequenceNumbers: true, chainingModeGCM: true);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool OpWebRpc(string uriPath, object parameters, bool sendAuthCookie = false)
	{
		if (string.IsNullOrEmpty(uriPath))
		{
			DebugReturn(DebugLevel.ERROR, "WebRPC method name must not be null nor empty.");
			return false;
		}
		if (!CheckIfOpCanBeSent(219, Server, "WebRpc"))
		{
			return false;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary.Add(209, uriPath);
		if (parameters != null)
		{
			dictionary.Add(208, parameters);
		}
		if (sendAuthCookie)
		{
			dictionary.Add(234, (byte)2);
		}
		return LoadBalancingPeer.SendOperation(219, dictionary, SendOptions.SendReliable);
	}

	public void AddCallbackTarget(object target)
	{
		callbackTargetChanges.Enqueue(new CallbackTargetChange(target, addTarget: true));
	}

	public void RemoveCallbackTarget(object target)
	{
		callbackTargetChanges.Enqueue(new CallbackTargetChange(target, addTarget: false));
	}

	protected internal void UpdateCallbackTargets()
	{
		while (callbackTargetChanges.Count > 0)
		{
			CallbackTargetChange callbackTargetChange = callbackTargetChanges.Dequeue();
			if (callbackTargetChange.AddTarget)
			{
				if (callbackTargets.Contains(callbackTargetChange.Target))
				{
					continue;
				}
				callbackTargets.Add(callbackTargetChange.Target);
			}
			else
			{
				if (!callbackTargets.Contains(callbackTargetChange.Target))
				{
					continue;
				}
				callbackTargets.Remove(callbackTargetChange.Target);
			}
			UpdateCallbackTarget(callbackTargetChange, InRoomCallbackTargets);
			UpdateCallbackTarget(callbackTargetChange, ConnectionCallbackTargets);
			UpdateCallbackTarget(callbackTargetChange, MatchMakingCallbackTargets);
			UpdateCallbackTarget(callbackTargetChange, LobbyCallbackTargets);
			UpdateCallbackTarget(callbackTargetChange, WebRpcCallbackTargets);
			UpdateCallbackTarget(callbackTargetChange, ErrorInfoCallbackTargets);
			if (callbackTargetChange.Target is IOnEventCallback onEventCallback)
			{
				if (callbackTargetChange.AddTarget)
				{
					EventReceived += onEventCallback.OnEvent;
				}
				else
				{
					EventReceived -= onEventCallback.OnEvent;
				}
			}
		}
	}

	private void UpdateCallbackTarget<T>(CallbackTargetChange change, List<T> container) where T : class
	{
		if (change.Target is T item)
		{
			if (change.AddTarget)
			{
				container.Add(item);
			}
			else
			{
				container.Remove(item);
			}
		}
	}
}
