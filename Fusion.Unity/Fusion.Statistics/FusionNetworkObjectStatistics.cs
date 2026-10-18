using UnityEngine;

namespace Fusion.Statistics;

[RequireComponent(typeof(NetworkObject))]
[DisallowMultipleComponent]
[AddComponentMenu("Fusion/Statistics/Network Object Statistics")]
public class FusionNetworkObjectStatistics : MonoBehaviour
{
	[HideInInspector]
	public NetworkObject NetworkObject;

	private void ToggleMonitoring(bool value)
	{
		NetworkObject = GetComponent<NetworkObject>();
		if (!NetworkObject.Runner || !NetworkObject.Runner.IsRunning || !NetworkObject.Runner.TryGetComponent<FusionStatistics>(out var component) || !component.MonitorNetworkObject(NetworkObject, this, value))
		{
			Object.Destroy(this);
		}
	}

	private void OnEnable()
	{
		ToggleMonitoring(value: true);
	}

	private void OnDisable()
	{
		ToggleMonitoring(value: false);
	}
}
