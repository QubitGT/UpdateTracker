using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Sockets;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Voice.Fusion;

[AddComponentMenu("Photon Voice/Fusion/Fusion Voice Client")]
[RequireComponent(typeof(NetworkRunner))]
public class FusionVoiceClient : MonoBehaviour, INetworkRunnerCallbacks, IPublicFacingInterface
{
	private NetworkRunner networkRunner;

	private EnterRoomParams voiceRoomParams = new EnterRoomParams
	{
		RoomOptions = new RoomOptions
		{
			IsVisible = false
		}
	};

	private bool voiceFollowClientStarted;

	[SerializeField]
	public bool UseFusionAppSettings = true;

	[SerializeField]
	public bool UseFusionAuthValues = true;

	private string fusionOfflineVoiceRoomName;

	private const byte FusionNetworkIdTypeCode = 0;

	private static byte[] memCompressedUInt64 = new byte[10];

	private string FusionOfflineVoiceRoomName
	{
		get
		{
			if (fusionOfflineVoiceRoomName == null)
			{
				fusionOfflineVoiceRoomName = $"fusion_offline_{Guid.NewGuid()}_voice";
			}
			return fusionOfflineVoiceRoomName;
		}
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
	}

	void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
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
	}

	void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
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

	void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, ArraySegment<byte> data)
	{
	}

	void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, float progress)
	{
	}
}
