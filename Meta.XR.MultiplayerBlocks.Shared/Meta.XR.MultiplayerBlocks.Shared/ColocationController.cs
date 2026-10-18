using Meta.XR.MultiplayerBlocks.Colocation;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MultiplayerBlocks.Shared;

public class ColocationController : MonoBehaviour
{
	[SerializeField]
	public UnityEvent ColocationReadyCallbacks;

	[SerializeField]
	internal ColocationDebuggingOptions DebuggingOptions;

	public void Awake()
	{
		if (DebuggingOptions.enableVerboseLogging)
		{
			Meta.XR.MultiplayerBlocks.Colocation.Logger.SetAllLogsVisibility(value: true);
			return;
		}
		Meta.XR.MultiplayerBlocks.Colocation.Logger.SetAllLogsVisibility(value: false);
		Meta.XR.MultiplayerBlocks.Colocation.Logger.SetLogLevelVisibility(LogLevel.Error, value: true);
		Meta.XR.MultiplayerBlocks.Colocation.Logger.SetLogLevelVisibility(LogLevel.SharedSpatialAnchorsError, value: true);
	}
}
