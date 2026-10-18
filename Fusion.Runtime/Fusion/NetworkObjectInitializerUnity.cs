namespace Fusion;

public class NetworkObjectInitializerUnity : INetworkObjectInitializer
{
	public void InitializeNetworkState(NetworkObject networkObject)
	{
		NetworkBehaviour[] networkedBehaviours = networkObject.NetworkedBehaviours;
		foreach (NetworkBehaviour networkBehaviour in networkedBehaviours)
		{
			networkBehaviour.CopyBackingFieldsToState(firstTime: true);
		}
	}
}
