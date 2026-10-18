using Meta.XR.BuildingBlocks;
using Meta.XR.MRUtilityKit;
using Meta.XR.MultiplayerBlocks.Colocation;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Shared;

internal static class NetworkBootstrapperUtils
{
	public static void SetEntitlementIds(PlatformInfo info, ref NetworkBootstrapperParams param)
	{
		param.myOculusId = info.OculusUser.ID;
		param.myPlayerId = (ulong)SystemInfo.deviceUniqueIdentifier.GetHashCode();
	}

	public static void SetUpAndStartAutomaticColocation(ref NetworkBootstrapperParams param, GameObject anchorPrefab, INetworkData networkData, INetworkMessenger networkMessenger)
	{
		networkMessenger.RegisterLocalPlayer(param.myPlayerId);
		SharedSpatialAnchorCore sharedSpatialAnchorCore = Object.FindObjectOfType<SharedSpatialAnchorCore>();
		if (sharedSpatialAnchorCore == null)
		{
			Debug.LogWarning("SharedSpatialAnchorCore component is missing from the scene, add this component to allow anchor sharing.");
			return;
		}
		param.sharedAnchorManager = new SharedAnchorManager(sharedSpatialAnchorCore);
		if (param.colocationController != null && param.colocationController.DebuggingOptions.visualizeAlignmentAnchor)
		{
			param.sharedAnchorManager.AnchorPrefab = anchorPrefab ?? new GameObject();
		}
		NetworkAdapter.SetConfig(networkData, networkMessenger);
		if (MRUK.Instance != null)
		{
			MRUK.Instance.EnableWorldLock = false;
		}
		param.colocationLauncher = new AutomaticColocationLauncher();
		param.colocationLauncher.Init(NetworkAdapter.NetworkData, NetworkAdapter.NetworkMessenger, param.sharedAnchorManager, param.ovrCameraRig.gameObject, param.myPlayerId, param.myOculusId);
		param.setupColocationReadyEvents();
		param.colocationLauncher.ColocationFailed += OnColocationFailed;
		param.colocationLauncher.ColocateAutomatically();
	}

	private static void OnColocationFailed(ColocationFailedReason e)
	{
		Meta.XR.MultiplayerBlocks.Colocation.Logger.Log($"Colocation failed - {e}", LogLevel.Error);
	}
}
