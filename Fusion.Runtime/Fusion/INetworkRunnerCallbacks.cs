using System;
using System.Collections.Generic;
using Fusion.Sockets;

namespace Fusion;

public interface INetworkRunnerCallbacks : IPublicFacingInterface
{
	void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player);

	void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player);

	void OnPlayerJoined(NetworkRunner runner, PlayerRef player);

	void OnPlayerLeft(NetworkRunner runner, PlayerRef player);

	void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason);

	void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason);

	void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token);

	void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason);

	void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message);

	void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data);

	void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress);

	void OnInput(NetworkRunner runner, NetworkInput input);

	void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input);

	void OnConnectedToServer(NetworkRunner runner);

	void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList);

	void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data);

	void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken);

	void OnSceneLoadDone(NetworkRunner runner);

	void OnSceneLoadStart(NetworkRunner runner);
}
