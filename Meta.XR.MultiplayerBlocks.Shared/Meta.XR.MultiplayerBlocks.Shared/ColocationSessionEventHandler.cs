using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.XR.BuildingBlocks;
using Meta.XR.MRUtilityKit;
using Meta.XR.MultiplayerBlocks.Colocation;
using UnityEngine;
using UnityEngine.Android;

namespace Meta.XR.MultiplayerBlocks.Shared;

public class ColocationSessionEventHandler : MonoBehaviour
{
	internal enum Basis
	{
		SharedSpatialAnchor,
		RoomAnchors
	}

	[Serializable]
	private struct SpaceSharingInfo
	{
		internal Guid RoomId;

		internal Pose FloorAnchor;
	}

	[Tooltip("The basis alignment/common reference approach for colocation")]
	[SerializeField]
	internal Basis basis;

	[SerializeField]
	private GameObject AnchorPrefab;

	private ColocationController _colocationController;

	private SharedAnchorManager _sharedAnchorManager;

	private AlignCameraToAnchor _alignCameraToAnchor;

	private OVRCameraRig _cameraRig;

	private void Awake()
	{
		_colocationController = UnityEngine.Object.FindObjectOfType<ColocationController>();
		_cameraRig = UnityEngine.Object.FindObjectOfType<OVRCameraRig>();
		if (basis == Basis.RoomAnchors)
		{
			LocalMatchmaking.BeforeStartHost = SpaceSharingBeforeHostStart;
		}
	}

	private void Start()
	{
		switch (basis)
		{
		case Basis.SharedSpatialAnchor:
		{
			SharedSpatialAnchorCore sharedSpatialAnchorCore = UnityEngine.Object.FindObjectOfType<SharedSpatialAnchorCore>();
			if (sharedSpatialAnchorCore == null)
			{
				throw new InvalidOperationException("SharedSpatialAnchorCore component is missing from the scene, add this component to allow anchor sharing.");
			}
			_sharedAnchorManager = new SharedAnchorManager(sharedSpatialAnchorCore);
			if (_colocationController.DebuggingOptions.visualizeAlignmentAnchor)
			{
				_sharedAnchorManager.AnchorPrefab = AnchorPrefab;
			}
			LocalMatchmaking.OnSessionCreateSucceeded.AddListener(OnSessionCreatedWithSpatialAnchor);
			LocalMatchmaking.OnSessionDiscoverSucceeded.AddListener(OnSessionDiscoveredWithSpatialAnchor);
			break;
		}
		case Basis.RoomAnchors:
			LocalMatchmaking.OnSessionCreateSucceeded.AddListener(OnSessionCreatedWithSpaceSharing);
			LocalMatchmaking.OnSessionDiscoverSucceeded.AddListener(OnSessionDiscoveredWithSpaceSharing);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		LocalMatchmaking.OnSessionCreateFailed.AddListener(Debug.LogError);
	}

	private async void OnSessionCreatedWithSpatialAnchor(Guid groupUuid)
	{
		await _sharedAnchorManager.CreateAlignmentAnchor();
		if (await _sharedAnchorManager.ShareAnchorsWithGroup(groupUuid))
		{
			_colocationController.ColocationReadyCallbacks?.Invoke();
			Meta.XR.MultiplayerBlocks.Colocation.Logger.Log("Host has created and shared the alignment anchor, and is ready for colocation", LogLevel.Info);
		}
	}

	private async void OnSessionDiscoveredWithSpatialAnchor(Guid groupUuid)
	{
		IReadOnlyList<OVRSpatialAnchor> readOnlyList = await _sharedAnchorManager.RetrieveAnchorsFromGroup(groupUuid);
		if (readOnlyList.Count != 0)
		{
			AlignCameraToAnchor alignCameraToAnchor = _cameraRig.gameObject.AddComponent<AlignCameraToAnchor>();
			alignCameraToAnchor.CameraAlignmentAnchor = readOnlyList[0];
			alignCameraToAnchor.RealignToAnchor();
			_colocationController.ColocationReadyCallbacks?.Invoke();
			Meta.XR.MultiplayerBlocks.Colocation.Logger.Log("Guest has retrieved and aligned with the alignment anchor, and is ready for colocation", LogLevel.Info);
		}
	}

	private async Task<bool> SpaceSharingBeforeHostStart()
	{
		if (!(await RequestScenePermissionIfNeeded()))
		{
			return false;
		}
		return await LoadScene();
	}

	private async Task<bool> RequestScenePermissionIfNeeded()
	{
		if (!OVRPermissionsRequester.IsPermissionGranted(OVRPermissionsRequester.Permission.Scene))
		{
			TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();
			PermissionCallbacks permissionCallbacks = new PermissionCallbacks();
			permissionCallbacks.PermissionDenied += delegate
			{
				Meta.XR.MultiplayerBlocks.Colocation.Logger.Log("Host failed to load scene from device as permission denied by user", LogLevel.Error);
				taskCompletion.SetResult(result: false);
			};
			permissionCallbacks.PermissionGranted += delegate
			{
				taskCompletion.SetResult(result: true);
			};
			Permission.RequestUserPermissions(new string[1] { OVRPermissionsRequester.GetPermissionId(OVRPermissionsRequester.Permission.Scene) }, permissionCallbacks);
			await taskCompletion.Task;
			if (!taskCompletion.Task.Result)
			{
				return false;
			}
		}
		return true;
	}

	private async Task<bool> LoadScene()
	{
		MRUK.Instance.RegisterSceneLoadedCallback(delegate
		{
			MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
			Transform transform = currentRoom.FloorAnchor.transform;
			SpaceSharingInfo obj = default(SpaceSharingInfo);
			obj.RoomId = currentRoom.Anchor.Uuid;
			obj.FloorAnchor = new Pose(transform.position, transform.rotation);
			LocalMatchmaking.ExtraData = SerializationUtils.SerializeToString(obj);
		});
		if (!MRUK.Instance.IsInitialized)
		{
			MRUK.LoadDeviceResult loadDeviceResult = await MRUK.Instance.LoadSceneFromDevice();
			if (loadDeviceResult != 0)
			{
				Meta.XR.MultiplayerBlocks.Colocation.Logger.Log($"Host failed to load scene from device: {loadDeviceResult}", LogLevel.Error);
				return false;
			}
		}
		return true;
	}

	private async void OnSessionCreatedWithSpaceSharing(Guid groupUuid)
	{
		MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
		OVRResult<OVRAnchor.ShareResult> oVRResult = await MRUK.Instance.ShareRoomsAsync(new MRUKRoom[1] { currentRoom }, groupUuid);
		if (!oVRResult.Success)
		{
			Meta.XR.MultiplayerBlocks.Colocation.Logger.Log($"Host failed to share rooms with group {groupUuid} : {oVRResult.Status}", LogLevel.Error);
			return;
		}
		if (_colocationController.DebuggingOptions.visualizeAlignmentAnchor)
		{
			UnityEngine.Object.Instantiate(AnchorPrefab, currentRoom.FloorAnchor.transform);
		}
		Meta.XR.MultiplayerBlocks.Colocation.Logger.Log($"Host successfully shared rooms with group {groupUuid}", LogLevel.Info);
	}

	private async void OnSessionDiscoveredWithSpaceSharing(Guid groupUuid)
	{
		if (string.IsNullOrEmpty(LocalMatchmaking.ExtraData))
		{
			Meta.XR.MultiplayerBlocks.Colocation.Logger.Log("Guest failed to load the data for space sharing from group sharing", LogLevel.Error);
			return;
		}
		SpaceSharingInfo spaceSharingInfo;
		try
		{
			spaceSharingInfo = SerializationUtils.DeserializeFromString<SpaceSharingInfo>(LocalMatchmaking.ExtraData);
		}
		catch (Exception arg)
		{
			Meta.XR.MultiplayerBlocks.Colocation.Logger.Log($"Guest failed to parse the data for space sharing from group : {LocalMatchmaking.ExtraData}, {arg}", LogLevel.Error);
			return;
		}
		MRUK.LoadDeviceResult loadDeviceResult = await MRUK.Instance.LoadSceneFromSharedRooms(new Guid[1] { spaceSharingInfo.RoomId }, groupUuid, (spaceSharingInfo.RoomId, spaceSharingInfo.FloorAnchor));
		if (loadDeviceResult != 0)
		{
			Meta.XR.MultiplayerBlocks.Colocation.Logger.Log($"Failed to load scene from shared room: {loadDeviceResult}", LogLevel.Error);
			return;
		}
		Meta.XR.MultiplayerBlocks.Colocation.Logger.Log("Guest has successfully loaded the shared room and is ready for colocation", LogLevel.Info);
		if (_colocationController.DebuggingOptions.visualizeAlignmentAnchor)
		{
			UnityEngine.Object.Instantiate(AnchorPrefab, MRUK.Instance.GetCurrentRoom().FloorAnchor.transform);
		}
	}

	private void OnDestroy()
	{
		switch (basis)
		{
		case Basis.SharedSpatialAnchor:
			LocalMatchmaking.OnSessionCreateSucceeded.RemoveListener(OnSessionCreatedWithSpatialAnchor);
			LocalMatchmaking.OnSessionDiscoverSucceeded.RemoveListener(OnSessionDiscoveredWithSpatialAnchor);
			break;
		case Basis.RoomAnchors:
			LocalMatchmaking.OnSessionCreateSucceeded.RemoveListener(OnSessionCreatedWithSpaceSharing);
			LocalMatchmaking.OnSessionDiscoverSucceeded.RemoveListener(OnSessionDiscoveredWithSpaceSharing);
			LocalMatchmaking.BeforeStartHost = null;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
