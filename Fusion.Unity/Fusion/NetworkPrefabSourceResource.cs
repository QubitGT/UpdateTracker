using System;

namespace Fusion;

[Serializable]
public class NetworkPrefabSourceResource : NetworkAssetSourceResource<NetworkObject>, INetworkPrefabSource, INetworkAssetSource<NetworkObject>
{
	public NetworkObjectGuid AssetGuid;

	NetworkObjectGuid INetworkPrefabSource.AssetGuid => AssetGuid;
}
