using System;
using System.Collections.Generic;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion;

[ScriptHelp(BackColor = ScriptHeaderBackColor.Sand)]
[DisallowMultipleComponent]
public class RunnerEnableVisibility : Behaviour, INetworkRunnerCallbacks, IPublicFacingInterface
{
	private void Awake()
	{
		NetworkRunner componentInParent = GetComponentInParent<NetworkRunner>();
		if ((bool)componentInParent)
		{
			componentInParent.EnableVisibilityExtension();
			componentInParent.ObjectAcquired -= RunnerOnObjectAcquired;
			componentInParent.ObjectAcquired += RunnerOnObjectAcquired;
		}
	}

	private void OnDestroy()
	{
		if (TryGetComponent<NetworkRunner>(out var component))
		{
			component.DisableVisibilityExtension();
			component.RemoveCallbacks(this);
			component.ObjectAcquired -= RunnerOnObjectAcquired;
		}
	}

	private void RunnerOnObjectAcquired(NetworkRunner runner, NetworkObject obj)
	{
		if (runner.IsRunning)
		{
			if (runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single)
			{
				UnityEngine.Object.Destroy(this);
			}
			else
			{
				runner.AddVisibilityNodes(obj.gameObject);
			}
		}
	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
	}

	void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
	{
		if (!runner.IsRunning)
		{
			return;
		}
		if (runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Scene simulationUnityScene = runner.SimulationUnityScene;
		if (simulationUnityScene.IsValid())
		{
			GameObject[] rootGameObjects = simulationUnityScene.GetRootGameObjects();
			foreach (GameObject go in rootGameObjects)
			{
				runner.AddVisibilityNodes(go);
			}
		}
	}

	void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
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

	void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
	}

	void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
	{
	}
}
