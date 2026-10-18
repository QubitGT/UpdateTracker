using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

[Feature(Feature.Anchors)]
public class SharedSpatialAnchorErrorHandler : MonoBehaviour
{
	[Tooltip("Disables the message alerts in headset.")]
	public bool DisableRuntimeGUIAlerts;

	[SerializeField]
	private GameObject AlertViewHUDPrefab;

	private string cloudPermissionMsg = "Your headset uses on-device point cloud data to determine its position within your room. To expand your headset’s capabilities and enable features like local multiplayer, you’ll need to share point cloud data with Meta. You can turn off point cloud sharing anytime in Settings.\n\nSettings > Privacy > Device Permissions > Turn on \"Share Point Cloud Data\"";

	private void Awake()
	{
		if ((bool)AlertViewHUDPrefab)
		{
			Object.Instantiate(AlertViewHUDPrefab);
		}
	}

	public void OnAnchorCreate(OVRSpatialAnchor _, OVRSpatialAnchor.OperationResult result)
	{
		switch (result)
		{
		case OVRSpatialAnchor.OperationResult.Failure_SpaceCloudStorageDisabled:
			LogWarning(cloudPermissionMsg);
			break;
		default:
			LogWarning("Failed to create the spatial anchor.");
			break;
		case OVRSpatialAnchor.OperationResult.Success:
			break;
		}
	}

	public void OnAnchorShare(List<OVRSpatialAnchor> _, OVRSpatialAnchor.OperationResult result)
	{
		switch (result)
		{
		case OVRSpatialAnchor.OperationResult.Failure_SpaceCloudStorageDisabled:
			LogWarning(cloudPermissionMsg);
			break;
		default:
			LogWarning("Failed to share the spatial anchor.");
			break;
		case OVRSpatialAnchor.OperationResult.Success:
			break;
		}
	}

	public void OnSharedSpatialAnchorLoad(List<OVRSpatialAnchor> loadedAnchors, OVRSpatialAnchor.OperationResult result)
	{
		if (result == OVRSpatialAnchor.OperationResult.Failure_SpaceCloudStorageDisabled)
		{
			LogWarning(cloudPermissionMsg);
		}
		else if (loadedAnchors == null || loadedAnchors.Count == 0)
		{
			LogWarning("Failed to load the spatial anchor(s).");
		}
	}

	public void OnAnchorEraseAll(OVRSpatialAnchor.OperationResult result)
	{
		if (result == OVRSpatialAnchor.OperationResult.Failure)
		{
			LogWarning("Failed to erase the spatial anchor(s).");
		}
	}

	public void OnAnchorErase(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
	{
		if (result == OVRSpatialAnchor.OperationResult.Failure)
		{
			LogWarning($"Failed to erase the spatial anchor with uuid: {anchor}");
		}
	}

	private void LogWarning(string msg)
	{
		if (!DisableRuntimeGUIAlerts)
		{
			AlertViewHUD.PostMessage(msg, AlertViewHUD.MessageType.Error);
		}
		Debug.LogWarning("[SharedSpatialAnchorErrorHandler] " + msg);
	}
}
