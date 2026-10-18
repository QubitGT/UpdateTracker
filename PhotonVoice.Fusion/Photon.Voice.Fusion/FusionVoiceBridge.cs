using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;

namespace Photon.Voice.Fusion;

[RequireComponent(typeof(NetworkRunner))]
[RequireComponent(typeof(VoiceConnection))]
public class FusionVoiceBridge : VoiceComponent, INetworkRunnerCallbacks, IPublicFacingInterface
{
	private NetworkRunner networkRunner;

	private VoiceConnection voiceConnection;

	private EnterRoomParams voiceRoomParams = new EnterRoomParams
	{
		RoomOptions = new RoomOptions
		{
			IsVisible = false
		}
	};

	private const byte FusionNetworkIdTypeCode = 0;

	private static byte[] memCompressedUInt64 = new byte[10];

	[field: SerializeField]
	public bool UseFusionAppSettings { get; set; } = true;


	[field: SerializeField]
	public bool UseFusionAuthValues { get; set; } = true;


	protected override void Awake()
	{
		base.Awake();
		VoiceRegisterCustomTypes();
		networkRunner = GetComponent<NetworkRunner>();
		voiceConnection = GetComponent<VoiceConnection>();
		voiceConnection.SpeakerFactory = FusionSpeakerFactory;
	}

	private void OnEnable()
	{
		voiceConnection.Client.StateChanged += OnVoiceClientStateChanged;
		if (networkRunner.IsPlayer && networkRunner.IsConnectedToServer)
		{
			VoiceConnectOrJoinRoom();
		}
	}

	private void OnDisable()
	{
		voiceConnection.Client.StateChanged -= OnVoiceClientStateChanged;
	}

	private void OnVoiceClientStateChanged(ClientState previous, ClientState current)
	{
		VoiceConnectOrJoinRoom(current);
	}

	private Speaker FusionSpeakerFactory(int playerId, byte voiceId, object userData)
	{
		if (!(userData is NetworkId networkId))
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("UserData ({0}) is not of type NetworkId. Remote voice {1}/{2} not linked. Do you have a Recorder not used with a VoiceNetworkObject? is this expected?", (userData == null) ? "null" : userData.ToString(), playerId, voiceId);
			}
			return null;
		}
		if (!networkId.IsValid)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("NetworkId is not valid ({0}). Remote voice {1}/{2} not linked.", networkId, playerId, voiceId);
			}
			return null;
		}
		VoiceNetworkObject voiceNetworkObject = networkRunner.TryGetNetworkedBehaviourFromNetworkedObjectRef<VoiceNetworkObject>(networkId);
		if ((object)voiceNetworkObject == null || !voiceNetworkObject)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("No voiceNetworkObject found with ID {0}. Remote voice {1}/{2} not linked.", networkId, playerId, voiceId);
			}
			return null;
		}
		if (!voiceNetworkObject.IgnoreGlobalLogLevel)
		{
			voiceNetworkObject.LogLevel = base.LogLevel;
		}
		if (!voiceNetworkObject.IsSpeaker)
		{
			voiceNetworkObject.SetupSpeakerInUse();
		}
		return voiceNetworkObject.SpeakerInUse;
	}

	private string VoiceGetMirroringRoomName()
	{
		return $"{networkRunner.SessionInfo.Name}_voice";
	}

	private void VoiceConnectOrJoinRoom()
	{
		VoiceConnectOrJoinRoom(voiceConnection.ClientState);
	}

	private void VoiceConnectOrJoinRoom(ClientState state)
	{
		if (ConnectionHandler.AppQuits)
		{
			return;
		}
		switch (state)
		{
		case ClientState.PeerCreated:
		case ClientState.Disconnected:
			if (!VoiceConnectAndFollowFusion() && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Connecting to server failed.");
			}
			break;
		case ClientState.ConnectedToMasterServer:
			if (!VoiceJoinMirroringRoom() && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Joining a voice room failed.");
			}
			break;
		case ClientState.Joined:
		{
			string text = VoiceGetMirroringRoomName();
			string text2 = voiceConnection.Client.CurrentRoom.Name;
			if (!text2.Equals(text))
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Voice room mismatch: Expected:\"{0}\" Current:\"{1}\", leaving the second to join the first.", text, text2);
				}
				if (!voiceConnection.Client.OpLeaveRoom(becomeInactive: false) && base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Leaving the current voice room failed.");
				}
			}
			break;
		}
		}
	}

	private bool VoiceConnectAndFollowFusion()
	{
		Photon.Realtime.AppSettings appSettings = new Photon.Realtime.AppSettings();
		if (UseFusionAppSettings)
		{
			appSettings.AppIdVoice = PhotonAppSettings.Global.AppSettings.AppIdVoice;
			appSettings.AppVersion = PhotonAppSettings.Global.AppSettings.AppVersion;
			appSettings.FixedRegion = PhotonAppSettings.Global.AppSettings.FixedRegion;
			appSettings.UseNameServer = PhotonAppSettings.Global.AppSettings.UseNameServer;
			appSettings.Server = PhotonAppSettings.Global.AppSettings.Server;
			appSettings.Port = PhotonAppSettings.Global.AppSettings.Port;
			appSettings.ProxyServer = PhotonAppSettings.Global.AppSettings.ProxyServer;
			appSettings.BestRegionSummaryFromStorage = PhotonAppSettings.Global.AppSettings.BestRegionSummaryFromStorage;
			appSettings.EnableLobbyStatistics = false;
			appSettings.EnableProtocolFallback = PhotonAppSettings.Global.AppSettings.EnableProtocolFallback;
			appSettings.Protocol = PhotonAppSettings.Global.AppSettings.Protocol;
			appSettings.AuthMode = (Photon.Realtime.AuthModeOption)PhotonAppSettings.Global.AppSettings.AuthMode;
			appSettings.NetworkLogging = PhotonAppSettings.Global.AppSettings.NetworkLogging;
		}
		else
		{
			voiceConnection.Settings.CopyTo(appSettings);
		}
		string region = networkRunner.SessionInfo.Region;
		if (string.IsNullOrEmpty(region))
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Unexpected: fusion region is empty.");
			}
			if (!string.IsNullOrEmpty(appSettings.FixedRegion))
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Unexpected: fusion region is empty while voice region is set to \"{0}\". Setting it to null now.", appSettings.FixedRegion);
				}
				appSettings.FixedRegion = null;
			}
		}
		else if (!string.Equals(appSettings.FixedRegion, region, StringComparison.OrdinalIgnoreCase))
		{
			if (base.Logger.IsInfoEnabled)
			{
				if (string.IsNullOrEmpty(appSettings.FixedRegion))
				{
					base.Logger.LogInfo("Setting voice region to \"{0}\" to match fusion region.", region);
				}
				else
				{
					base.Logger.LogInfo("Switching voice region to \"{0}\" from \"{1}\" to match fusion region.", region, appSettings.FixedRegion);
				}
			}
			appSettings.FixedRegion = region;
		}
		if (UseFusionAuthValues && networkRunner.AuthenticationValues != null)
		{
			voiceConnection.Client.AuthValues = new Photon.Realtime.AuthenticationValues(networkRunner.AuthenticationValues.UserId)
			{
				AuthGetParameters = networkRunner.AuthenticationValues.AuthGetParameters,
				AuthType = (Photon.Realtime.CustomAuthenticationType)networkRunner.AuthenticationValues.AuthType
			};
			if (networkRunner.AuthenticationValues.AuthPostData != null)
			{
				if (networkRunner.AuthenticationValues.AuthPostData is byte[] authPostData)
				{
					voiceConnection.Client.AuthValues.SetAuthPostData(authPostData);
				}
				else if (networkRunner.AuthenticationValues.AuthPostData is string authPostData2)
				{
					voiceConnection.Client.AuthValues.SetAuthPostData(authPostData2);
				}
				else if (networkRunner.AuthenticationValues.AuthPostData is Dictionary<string, object> authPostData3)
				{
					voiceConnection.Client.AuthValues.SetAuthPostData(authPostData3);
				}
			}
		}
		return voiceConnection.ConnectUsingSettings(appSettings);
	}

	private void VoiceDisconnect()
	{
		voiceConnection.Client.Disconnect();
	}

	private bool VoiceJoinRoom(string voiceRoomName)
	{
		if (string.IsNullOrEmpty(voiceRoomName))
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Voice room name is null or empty.");
			}
			return false;
		}
		voiceRoomParams.RoomName = voiceRoomName;
		return voiceConnection.Client.OpJoinOrCreateRoom(voiceRoomParams);
	}

	private bool VoiceJoinMirroringRoom()
	{
		return VoiceJoinRoom(VoiceGetMirroringRoomName());
	}

	private static void VoiceRegisterCustomTypes()
	{
		PhotonPeer.RegisterType(typeof(NetworkId), 0, SerializeFusionNetworkId, DeserializeFusionNetworkId);
	}

	private static object DeserializeFusionNetworkId(StreamBuffer instream, short length)
	{
		NetworkId networkId = default(NetworkId);
		lock (memCompressedUInt64)
		{
			ulong num = ReadCompressedUInt64(instream);
			networkId.Raw = (uint)num;
		}
		return networkId;
	}

	private static ulong ReadCompressedUInt64(StreamBuffer stream)
	{
		ulong num = 0uL;
		int num2 = 0;
		byte[] buffer = stream.GetBuffer();
		int num3 = stream.Position;
		while (num2 != 70)
		{
			if (num3 >= buffer.Length)
			{
				throw new EndOfStreamException("Failed to read full ulong.");
			}
			byte b = buffer[num3];
			num3++;
			num |= (ulong)((long)(b & 0x7F) << num2);
			num2 += 7;
			if ((b & 0x80) == 0)
			{
				break;
			}
		}
		stream.Position = num3;
		return num;
	}

	private static int WriteCompressedUInt64(StreamBuffer stream, ulong value)
	{
		int num = 0;
		lock (memCompressedUInt64)
		{
			memCompressedUInt64[num] = (byte)(value & 0x7F);
			for (value >>= 7; value != 0; value >>= 7)
			{
				memCompressedUInt64[num] |= 128;
				memCompressedUInt64[++num] = (byte)(value & 0x7F);
			}
			num++;
			stream.Write(memCompressedUInt64, 0, num);
			return num;
		}
	}

	private static short SerializeFusionNetworkId(StreamBuffer outstream, object customobject)
	{
		return (short)WriteCompressedUInt64(outstream, ((NetworkId)customobject).Raw);
	}

	void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnPlayerJoined {0}", player);
		}
		if (runner.LocalPlayer == player)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Local player joined, calling VoiceConnectOrJoinRoom");
			}
			VoiceConnectOrJoinRoom();
		}
	}

	void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnPlayerLeft {0}", player);
		}
		if (runner.LocalPlayer == player)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Local player left, calling VoiceDisconnect");
			}
			VoiceDisconnect();
		}
	}

	void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
	{
	}

	void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
	}

	void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
	}

	void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
	{
		VoiceConnectOrJoinRoom();
	}

	void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
	}

	void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
	}

	void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
	}

	void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
	}

	void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
	}

	void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
	}

	void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
	{
	}

	void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
	{
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnDisconnectedFromServer, calling VoiceDisconnect");
		}
		VoiceDisconnect();
	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
	}
}
