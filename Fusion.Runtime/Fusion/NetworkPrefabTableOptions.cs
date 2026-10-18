using System;

namespace Fusion;

[Serializable]
public struct NetworkPrefabTableOptions
{
	public bool UnloadPrefabOnReleasingLastInstance;

	public bool UnloadUnusedPrefabsOnShutdown;

	public static NetworkPrefabTableOptions Default = new NetworkPrefabTableOptions
	{
		UnloadPrefabOnReleasingLastInstance = false,
		UnloadUnusedPrefabsOnShutdown = true
	};
}
