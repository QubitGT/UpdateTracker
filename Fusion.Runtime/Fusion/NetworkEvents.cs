using System;
using System.Collections.Generic;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion;

[AddComponentMenu("Fusion/Network Events")]
public class NetworkEvents : Behaviour, INetworkRunnerCallbacks, IPublicFacingInterface
{
	[Serializable]
	public class InputEvent : UnityEvent<NetworkRunner, NetworkInput>
	{
	}

	[Serializable]
	public class InputPlayerEvent : UnityEvent<NetworkRunner, PlayerRef, NetworkInput>
	{
	}

	[Serializable]
	public class ConnectRequestEvent : UnityEvent<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest, byte[]>
	{
	}

	[Serializable]
	public class ConnectFailedEvent : UnityEvent<NetworkRunner, NetAddress, NetConnectFailedReason>
	{
	}

	[Serializable]
	public class DisconnectFromServerEvent : UnityEvent<NetworkRunner, NetDisconnectReason>
	{
	}

	[Serializable]
	public class ShutdownEvent : UnityEvent<NetworkRunner, ShutdownReason>
	{
	}

	[Serializable]
	public class PlayerEvent : UnityEvent<NetworkRunner, PlayerRef>
	{
	}

	[Serializable]
	public class RunnerEvent : UnityEvent<NetworkRunner>
	{
	}

	[Serializable]
	public class SimulationMessageEvent : UnityEvent<NetworkRunner, SimulationMessagePtr>
	{
	}

	[Serializable]
	public class SessionListUpdateEvent : UnityEvent<NetworkRunner, List<SessionInfo>>
	{
	}

	[Serializable]
	public class CustomAuthenticationResponse : UnityEvent<NetworkRunner, Dictionary<string, object>>
	{
	}

	[Serializable]
	public class HostMigrationEvent : UnityEvent<NetworkRunner, HostMigrationToken>
	{
	}

	[Serializable]
	public class ReliableDataEvent : UnityEvent<NetworkRunner, PlayerRef, ReliableKey, ArraySegment<byte>>
	{
	}

	[Serializable]
	public class ReliableProgressEvent : UnityEvent<NetworkRunner, PlayerRef, ReliableKey, float>
	{
	}

	[Serializable]
	public class ObjectEvent : UnityEvent<NetworkRunner, NetworkObject>
	{
	}

	[Serializable]
	public class ObjectPlayerEvent : UnityEvent<NetworkRunner, NetworkObject, PlayerRef>
	{
	}

	public InputEvent OnInput;

	public InputPlayerEvent OnInputMissing;

	public RunnerEvent OnConnectedToServer;

	public DisconnectFromServerEvent OnDisconnectedFromServer;

	public ConnectRequestEvent OnConnectRequest;

	public ConnectFailedEvent OnConnectFailed;

	public PlayerEvent PlayerJoined;

	public PlayerEvent PlayerLeft;

	public SimulationMessageEvent OnSimulationMessage;

	public ShutdownEvent OnShutdown;

	public SessionListUpdateEvent OnSessionListUpdate;

	public CustomAuthenticationResponse OnCustomAuthenticationResponse;

	public HostMigrationEvent OnHostMigration;

	public RunnerEvent OnSceneLoadDone;

	public RunnerEvent OnSceneLoadStart;

	public ReliableDataEvent OnReliableData;

	public ReliableProgressEvent OnReliableProgress;

	public ObjectPlayerEvent OnObjectEnterAOI;

	public ObjectPlayerEvent OnObjectExitAOI;

	void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		OnObjectExitAOI.Invoke(runner, obj, player);
	}

	void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		OnObjectEnterAOI.Invoke(runner, obj, player);
	}

	void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		PlayerJoined?.Invoke(runner, player);
	}

	void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		PlayerLeft?.Invoke(runner, player);
	}

	void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
	{
		OnInput?.Invoke(runner, input);
	}

	void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
		OnInputMissing?.Invoke(runner, player, input);
	}

	void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
		OnShutdown?.Invoke(runner, shutdownReason);
	}

	void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
	{
		OnConnectedToServer?.Invoke(runner);
	}

	void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
		OnDisconnectedFromServer?.Invoke(runner, reason);
	}

	void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
		OnConnectRequest?.Invoke(runner, request, token);
	}

	void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
		OnConnectFailed?.Invoke(runner, remoteAddress, reason);
	}

	void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
		OnSimulationMessage?.Invoke(runner, message);
	}

	void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
		OnSessionListUpdate?.Invoke(runner, sessionList);
	}

	void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
		OnReliableData?.Invoke(runner, player, key, data);
	}

	void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
		OnReliableProgress?.Invoke(runner, player, key, progress);
	}

	void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
	{
		OnSceneLoadDone?.Invoke(runner);
	}

	void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
	{
		OnSceneLoadStart?.Invoke(runner);
	}

	void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
		OnCustomAuthenticationResponse?.Invoke(runner, data);
	}

	void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
		OnHostMigration?.Invoke(runner, hostMigrationToken);
	}
}
