namespace Fusion;

public interface INetworkObjectProvider
{
	NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result);

	void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context);

	NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid);

	void Shutdown(NetworkRunner networkRunner)
	{
	}

	void Initialize(NetworkRunner networkRunner)
	{
	}
}
