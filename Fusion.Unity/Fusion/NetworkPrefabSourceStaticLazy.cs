using System;

namespace Fusion;

[Serializable]
public class NetworkPrefabSourceStaticLazy : NetworkAssetSourceStaticLazy<NetworkObject>, INetworkPrefabSource, INetworkAssetSource<NetworkObject>
{
	public NetworkObjectGuid AssetGuid;

	NetworkObjectGuid INetworkPrefabSource.AssetGuid => AssetGuid;
}
