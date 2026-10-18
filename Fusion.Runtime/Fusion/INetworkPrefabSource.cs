namespace Fusion;

public interface INetworkPrefabSource : INetworkAssetSource<NetworkObject>
{
	NetworkObjectGuid AssetGuid { get; }
}
