using System;

namespace Fusion;

[Serializable]
public class NetworkPrefabSourceStatic : NetworkAssetSourceStatic<NetworkObject>, INetworkPrefabSource, INetworkAssetSource<NetworkObject>
{
	public NetworkObjectGuid AssetGuid;

	NetworkObjectGuid INetworkPrefabSource.AssetGuid => AssetGuid;
}
