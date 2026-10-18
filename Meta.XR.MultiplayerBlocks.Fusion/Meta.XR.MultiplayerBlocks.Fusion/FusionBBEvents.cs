using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Fusion;

public class FusionBBEvents : MonoBehaviour, INetworkRunnerCallbacks, IPublicFacingInterface
{
	public static event Action<NetworkRunner> OnConnectedToServer;

	public static event Action<NetworkRunner, PlayerRef> OnPlayerJoined;

	public static event Action<NetworkRunner, NetworkInput> OnInput;

	public static event Action<NetworkRunner, NetAddress, NetConnectFailedReason> OnConnectFailed;

	public static event Action<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest, byte[]> OnConnectRequest;

	public static event Action<NetworkRunner, Dictionary<string, object>> OnCustomAuthenticationResponse;

	public static event Action<NetworkRunner, HostMigrationToken> OnHostMigration;

	public static event Action<NetworkRunner, PlayerRef, NetworkInput> OnInputMissing;

	public static event Action<NetworkRunner, PlayerRef> OnPlayerLeft;

	public static event Action<NetworkRunner> OnSceneLoadDone;

	public static event Action<NetworkRunner> OnSceneLoadStart;

	public static event Action<NetworkRunner, List<SessionInfo>> OnSessionListUpdated;

	public static event Action<NetworkRunner, ShutdownReason> OnShutdown;

	public static event Action<NetworkRunner, SimulationMessagePtr> OnUserSimulationMessage;

	public static event Action<NetworkRunner, NetworkObject, PlayerRef> OnObjectExitAOI;

	public static event Action<NetworkRunner, NetworkObject, PlayerRef> OnObjectEnterAOI;

	public static event Action<NetworkRunner, NetDisconnectReason> OnDisconnectedFromServer;

	public static event Action<NetworkRunner, PlayerRef, ReliableKey, ArraySegment<byte>> OnReliableDataReceived;

	public static event Action<NetworkRunner, PlayerRef, ReliableKey, float> OnReliableDataProgress;

	void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
	{
		FusionBBEvents.OnConnectedToServer?.Invoke(runner);
	}

	void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		FusionBBEvents.OnPlayerJoined?.Invoke(runner, player);
	}

	void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
	{
		FusionBBEvents.OnInput?.Invoke(runner, input);
	}

	void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
		Debug.LogWarning("OnConnectFailed");
		FusionBBEvents.OnConnectFailed?.Invoke(runner, remoteAddress, reason);
	}

	void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
		Debug.LogWarning("OnConnectRequest");
		FusionBBEvents.OnConnectRequest?.Invoke(runner, request, token);
	}

	void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
		FusionBBEvents.OnCustomAuthenticationResponse?.Invoke(runner, data);
	}

	void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
		FusionBBEvents.OnHostMigration?.Invoke(runner, hostMigrationToken);
	}

	void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
		FusionBBEvents.OnInputMissing?.Invoke(runner, player, input);
	}

	void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		FusionBBEvents.OnPlayerLeft?.Invoke(runner, player);
	}

	void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
	{
		FusionBBEvents.OnSceneLoadDone?.Invoke(runner);
	}

	void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
	{
		FusionBBEvents.OnSceneLoadStart?.Invoke(runner);
	}

	void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
		FusionBBEvents.OnSessionListUpdated?.Invoke(runner, sessionList);
	}

	void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
		FusionBBEvents.OnShutdown?.Invoke(runner, shutdownReason);
	}

	void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
		FusionBBEvents.OnUserSimulationMessage?.Invoke(runner, message);
	}

	void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		FusionBBEvents.OnObjectExitAOI?.Invoke(runner, obj, player);
	}

	void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		FusionBBEvents.OnObjectEnterAOI?.Invoke(runner, obj, player);
	}

	void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
		FusionBBEvents.OnDisconnectedFromServer?.Invoke(runner, reason);
	}

	void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
		FusionBBEvents.OnReliableDataReceived?.Invoke(runner, player, key, data);
	}

	void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
		FusionBBEvents.OnReliableDataProgress?.Invoke(runner, player, key, progress);
	}
}
