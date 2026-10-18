using System;

namespace Fusion;

[Serializable]
public class NetworkPrefabSourceAddressable : NetworkAssetSourceAddressable<NetworkObject>, INetworkPrefabSource, INetworkAssetSource<NetworkObject>
{
	public NetworkObjectGuid AssetGuid;

	NetworkObjectGuid INetworkPrefabSource.AssetGuid => AssetGuid;
}
