using System;
using System.Collections.Generic;
using Fusion.Sockets;

namespace Fusion;

public class NetworkDelegates : INetworkRunnerCallbacks, IPublicFacingInterface
{
	public Action<NetworkRunner, PlayerRef> OnPlayerJoined;

	public Action<NetworkRunner, PlayerRef> OnPlayerLeft;

	public Action<NetworkRunner, NetworkInput> OnInput;

	public Action<NetworkRunner, PlayerRef, NetworkInput> OnInputMissing;

	public Action<NetworkRunner, ShutdownReason> OnShutdown;

	public Action<NetworkRunner, NetDisconnectReason> OnDisconnectedFromServer;

	public Action<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest, byte[]> OnConnectRequest;

	public Action<NetworkRunner, NetAddress, NetConnectFailedReason> OnConnectFailed;

	public Action<NetworkRunner, SimulationMessagePtr> OnUserSimulationMessage;

	public Action<NetworkRunner, PlayerRef, ReliableKey, ArraySegment<byte>> OnReliableDataReceived;

	public Action<NetworkRunner, PlayerRef, ReliableKey, float> OnReliableDataProgress;

	public Action<NetworkRunner, NetworkObject, PlayerRef> OnObjectExitAOI;

	public Action<NetworkRunner, NetworkObject, PlayerRef> OnObjectEnterAOI;

	public Action<NetworkRunner> OnConnectedToServer;

	public Action<NetworkRunner> OnSceneLoadDone;

	public Action<NetworkRunner> OnSceneLoadStart;

	public Action<NetworkRunner, List<SessionInfo>> OnSessionListUpdated;

	public Action<NetworkRunner, Dictionary<string, object>> OnCustomAuthenticationResponse;

	public Action<NetworkRunner, HostMigrationToken> OnHostMigration;

	void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		OnObjectExitAOI?.Invoke(runner, obj, player);
	}

	void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		OnObjectEnterAOI?.Invoke(runner, obj, player);
	}

	void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		OnPlayerJoined?.Invoke(runner, player);
	}

	void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		OnPlayerLeft?.Invoke(runner, player);
	}

	void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
		OnDisconnectedFromServer?.Invoke(runner, reason);
	}

	void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
		OnShutdown?.Invoke(runner, shutdownReason);
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
		OnUserSimulationMessage?.Invoke(runner, message);
	}

	void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
		OnReliableDataReceived?.Invoke(runner, player, key, data);
	}

	void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
		OnReliableDataProgress?.Invoke(runner, player, key, progress);
	}

	void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
	{
		OnInput?.Invoke(runner, input);
	}

	void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
		OnInputMissing?.Invoke(runner, player, input);
	}

	void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
	{
		OnConnectedToServer?.Invoke(runner);
	}

	void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
		OnSessionListUpdated?.Invoke(runner, sessionList);
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
