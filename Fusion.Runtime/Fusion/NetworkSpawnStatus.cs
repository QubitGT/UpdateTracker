namespace Fusion;

public enum NetworkSpawnStatus
{
	Queued,
	Spawned,
	FailedToLoadPrefabSynchronously,
	FailedToCreateInstance,
	FailedClientCantSpawn,
	FailedLocalPlayerNotYetSet
}
