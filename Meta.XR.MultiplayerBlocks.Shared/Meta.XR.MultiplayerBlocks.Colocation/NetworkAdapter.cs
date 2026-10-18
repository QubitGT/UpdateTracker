namespace Meta.XR.MultiplayerBlocks.Colocation;

internal static class NetworkAdapter
{
	public static INetworkData NetworkData { get; private set; }

	public static INetworkMessenger NetworkMessenger { get; private set; }

	public static void SetConfig(INetworkData networkData, INetworkMessenger networkMessenger)
	{
		NetworkData = networkData;
		NetworkMessenger = networkMessenger;
	}
}
