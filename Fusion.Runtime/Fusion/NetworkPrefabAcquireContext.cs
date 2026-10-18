using System;

namespace Fusion;

public readonly struct NetworkPrefabAcquireContext
{
	public readonly NetworkPrefabId PrefabId;

	public readonly NetworkObjectMeta Meta;

	public readonly bool IsSynchronous;

	public readonly bool DontDestroyOnLoad;

	public bool HasHeader => Meta != null;

	public Span<int> Data => (Meta != null) ? Meta.Data : default(Span<int>);

	public NetworkPrefabAcquireContext(NetworkPrefabId prefabId, NetworkObjectMeta meta = null, bool isSynchronous = true, bool dontDestroyOnLoad = false)
	{
		PrefabId = prefabId;
		Meta = meta;
		IsSynchronous = isSynchronous;
		DontDestroyOnLoad = dontDestroyOnLoad;
	}
}
