using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Meta.XR.ImmersiveDebugger;
using Meta.XR.Util;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_m_r_u_k")]
[Feature(Feature.Scene)]
public class MRUK : MonoBehaviour
{
	public enum PositioningMethod
	{
		DEFAULT,
		CENTER,
		EDGE
	}

	public enum SceneDataSource
	{
		Device,
		Prefab,
		DeviceWithPrefabFallback,
		Json,
		DeviceWithJsonFallback
	}

	public enum RoomFilter
	{
		None,
		CurrentRoomOnly,
		AllRooms
	}

	public enum LoadDeviceResult
	{
		Success = 0,
		NoScenePermission = 1,
		NoRoomsFound = 2,
		DiscoveryOngoing = 3,
		Failure = -1000,
		StorageAtCapacity = -9001,
		NotInitialized = -1002,
		FailureDataIsInvalid = -1008,
		FailureInsufficientResources = -9000,
		FailureInsufficientView = -9002,
		FailurePermissionInsufficient = -9003,
		FailureRateLimited = -9004,
		FailureTooDark = -9005,
		FailureTooBright = -9006
	}

	internal struct SceneTrackingSettings
	{
		internal HashSet<MRUKRoom> UnTrackedRooms;

		internal HashSet<MRUKAnchor> UnTrackedAnchors;
	}

	[Flags]
	public enum SurfaceType
	{
		FACING_UP = 1,
		FACING_DOWN = 2,
		VERTICAL = 4
	}

	[Flags]
	private enum AnchorRepresentation
	{
		PLANE = 1,
		VOLUME = 2
	}

	[Serializable]
	public class MRUKSettings
	{
		[Header("Data Source settings")]
		[SerializeField]
		[Tooltip("Where to load the data from.")]
		public SceneDataSource DataSource;

		[SerializeField]
		[Tooltip("The index (0-based) into the RoomPrefabs or SceneJsons array; -1 is random.")]
		public int RoomIndex = -1;

		[SerializeField]
		[Tooltip("The list of prefab rooms to use.")]
		public GameObject[] RoomPrefabs;

		[SerializeField]
		[Tooltip("The list of JSON text files with scene data to use. Uses RoomIndex")]
		public TextAsset[] SceneJsons;

		[Space]
		[Header("Startup settings")]
		[SerializeField]
		[Tooltip("Trigger a scene load on startup. If set to false, you can call LoadSceneFromDevice(), LoadSceneFromPrefab() or LoadSceneFromJsonString() manually.")]
		public bool LoadSceneOnStartup = true;

		[Space]
		[Header("Other settings")]
		[SerializeField]
		[Tooltip("The width of a seat. Used to calculate seat positions with the COUCH label.")]
		public float SeatWidth = 0.6f;

		[SerializeField]
		[HideInInspector]
		[Obsolete]
		internal string SceneJson;

		[field: SerializeField]
		[field: Tooltip("Settings related to trackables that are detectable in the environment at runtime.")]
		public OVRAnchor.TrackerConfiguration TrackerConfiguration { get; set; }

		[field: SerializeField]
		[field: Tooltip("Invoked after a newly detected anchor has been localized.")]
		public UnityEvent<MRUKTrackable> TrackableAdded { get; private set; } = new UnityEvent<MRUKTrackable>();


		[field: SerializeField]
		[field: Tooltip("The event is invoked when an anchor is removed.")]
		public UnityEvent<MRUKTrackable> TrackableRemoved { get; private set; } = new UnityEvent<MRUKTrackable>();

	}

	private struct SharedRoomsData
	{
		internal IEnumerable<Guid> roomUuids;

		internal Guid groupUuid;

		internal (Guid alignmentRoomUuid, Pose floorWorldPoseOnHost)? alignmentData;
	}

	private enum TrackableState
	{
		PendingLocalization,
		InstanceDestroyed,
		Instantiated,
		LocalizationFailed
	}

	public bool EnableWorldLock = true;

	[HideInInspector]
	public Matrix4x4 TrackingSpaceOffset = Matrix4x4.identity;

	private bool _worldLockActive;

	private bool _worldLockWasEnabled;

	private bool _loadSceneCalled;

	private Pose? _prevTrackingSpacePose;

	private readonly List<OVRSemanticLabels.Classification> _classificationsBuffer = new List<OVRSemanticLabels.Classification>(1);

	[Tooltip("Contains all the information regarding data loading.")]
	public MRUKSettings SceneSettings;

	private MRUKRoom _cachedCurrentRoom;

	private int _cachedCurrentRoomFrame;

	[SerializeField]
	internal GameObject _immersiveSceneDebuggerPrefab;

	private OVRTask<LoadDeviceResult>? _loadSceneTask;

	private ulong _currentAppSpace;

	private bool _openXrInitialised;

	private readonly OVRAnchor.Tracker _tracker = new OVRAnchor.Tracker();

	private Coroutine _trackerCoroutine;

	private readonly Dictionary<OVRAnchor, TrackableState> _trackableStates = new Dictionary<OVRAnchor, TrackableState>();

	private readonly Dictionary<OVRAnchor, Transform> _trackableTransforms = new Dictionary<OVRAnchor, Transform>();

	private static readonly TimeSpan TimeBetweenFetchTrackables = TimeSpan.FromSeconds(0.5);

	public bool IsInitialized { get; private set; }

	[field: SerializeField]
	[field: FormerlySerializedAs("SceneLoadedEvent")]
	public UnityEvent SceneLoadedEvent { get; private set; } = new UnityEvent();


	[field: SerializeField]
	[field: FormerlySerializedAs("RoomCreatedEvent")]
	public UnityEvent<MRUKRoom> RoomCreatedEvent { get; private set; } = new UnityEvent<MRUKRoom>();


	[field: SerializeField]
	[field: FormerlySerializedAs("RoomUpdatedEvent")]
	public UnityEvent<MRUKRoom> RoomUpdatedEvent { get; private set; } = new UnityEvent<MRUKRoom>();


	[field: SerializeField]
	[field: FormerlySerializedAs("RoomRemovedEvent")]
	public UnityEvent<MRUKRoom> RoomRemovedEvent { get; private set; } = new UnityEvent<MRUKRoom>();


	public bool IsWorldLockActive
	{
		get
		{
			if (EnableWorldLock)
			{
				return _worldLockActive;
			}
			return false;
		}
	}

	internal OVRCameraRig _cameraRig { get; private set; }

	public List<MRUKRoom> Rooms { get; } = new List<MRUKRoom>();


	public static MRUK Instance { get; private set; }

	private static bool IsOpenXRAvailable => OVRPlugin.initialized;

	public OVRAnchor.TrackerConfiguration TrackerConfiguration => _tracker?.Configuration ?? default(OVRAnchor.TrackerConfiguration);

	private void InitializeScene()
	{
		try
		{
			SceneLoadedEvent.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		IsInitialized = true;
	}

	public void RegisterSceneLoadedCallback(UnityAction callback)
	{
		SceneLoadedEvent.AddListener(callback);
		if (IsInitialized)
		{
			callback();
		}
	}

	[Obsolete("Use UnityEvent RoomCreatedEvent directly instead")]
	public void RegisterRoomCreatedCallback(UnityAction<MRUKRoom> callback)
	{
		RoomCreatedEvent.AddListener(callback);
	}

	[Obsolete("Use UnityEvent RoomUpdatedEvent directly instead")]
	public void RegisterRoomUpdatedCallback(UnityAction<MRUKRoom> callback)
	{
		RoomUpdatedEvent.AddListener(callback);
	}

	[Obsolete("Use UnityEvent RoomRemovedEvent directly instead")]
	public void RegisterRoomRemovedCallback(UnityAction<MRUKRoom> callback)
	{
		RoomRemovedEvent.AddListener(callback);
	}

	[Obsolete("Use Rooms property instead")]
	public List<MRUKRoom> GetRooms()
	{
		return Rooms;
	}

	[Obsolete("Use GetCurrentRoom().Anchors instead")]
	public List<MRUKAnchor> GetAnchors()
	{
		return GetCurrentRoom().Anchors;
	}

	public MRUKRoom GetCurrentRoom()
	{
		if (_cachedCurrentRoomFrame != Time.frameCount)
		{
			Vector3? vector = _cameraRig?.centerEyeAnchor.position;
			if (vector.HasValue)
			{
				Vector3 valueOrDefault = vector.GetValueOrDefault();
				MRUKRoom mRUKRoom = null;
				foreach (MRUKRoom room in Rooms)
				{
					if (room.IsPositionInRoom(valueOrDefault, testVerticalBounds: false))
					{
						mRUKRoom = room;
						if (room.IsLocal)
						{
							break;
						}
					}
				}
				if (mRUKRoom != null)
				{
					_cachedCurrentRoom = mRUKRoom;
					_cachedCurrentRoomFrame = Time.frameCount;
					return mRUKRoom;
				}
			}
		}
		if (_cachedCurrentRoom != null)
		{
			return _cachedCurrentRoom;
		}
		if (Rooms.Count > 0)
		{
			return Rooms[0];
		}
		return null;
	}

	public static async Task<bool> HasSceneModel()
	{
		List<OVRAnchor> rooms = new List<OVRAnchor>();
		return (await OVRAnchor.FetchAnchorsAsync(rooms, new OVRAnchor.FetchOptions
		{
			SingleComponentType = typeof(OVRRoomLayout)
		})).Success && rooms.Count > 0;
	}

	private unsafe void Awake()
	{
		_cameraRig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
		MRUKNative.LoadMRUKSharedLibrary();
		MRUKNativeFuncs.SetLogPrinter(OnSharedLibLog);
		InitializeAnchorStore();
		if (SceneSettings != null && SceneSettings.LoadSceneOnStartup)
		{
			LoadScene(SceneSettings.DataSource);
		}
		if (RuntimeSettings.Instance.ImmersiveDebuggerEnabled && _immersiveSceneDebuggerPrefab != null && !(ImmersiveSceneDebugger.Instance != null))
		{
			UnityEngine.Object.Instantiate(_immersiveSceneDebuggerPrefab);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			DestroyAnchorStore();
			MRUKNative.FreeMRUKSharedLibrary();
			Instance = null;
			RoomCreatedEvent.RemoveAllListeners();
			RoomRemovedEvent.RemoveAllListeners();
			RoomUpdatedEvent.RemoveAllListeners();
			SceneLoadedEvent.RemoveAllListeners();
		}
	}

	private void Start()
	{
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.LogPrinter))]
	private unsafe static void OnSharedLibLog(MRUKNativeFuncs.MrukLogLevel logLevel, char* message, uint length)
	{
		try
		{
			LogType logType = LogType.Log;
			switch (logLevel)
			{
			case MRUKNativeFuncs.MrukLogLevel.Debug:
			case MRUKNativeFuncs.MrukLogLevel.Info:
				logType = LogType.Log;
				break;
			case MRUKNativeFuncs.MrukLogLevel.Warn:
				logType = LogType.Warning;
				break;
			case MRUKNativeFuncs.MrukLogLevel.Error:
				logType = LogType.Error;
				break;
			}
			Debug.LogFormat(logType, LogOption.None, null, "MRUK Shared: {0}", Marshal.PtrToStringUTF8((IntPtr)message, (int)length));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.TrackingSpacePoseGetter))]
	private static Pose GetTrackingSpacePose()
	{
		Transform trackingSpace = GetTrackingSpace();
		Pose lhs = FlipZRotateY180((trackingSpace != null) ? new Pose(trackingSpace.position, trackingSpace.rotation) : Pose.identity);
		return FlipZRotateY180(Pose.identity).GetTransformedBy(lhs);
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.TrackingSpacePoseSetter))]
	private static void SetTrackingSpacePose(Pose openXrPose)
	{
		Pose pose = FlipZRotateY180(FlipZRotateY180(Pose.identity).GetTransformedBy(openXrPose));
		GetTrackingSpace()?.SetPositionAndRotation(pose.position, pose.rotation);
	}

	private static Transform GetTrackingSpace()
	{
		if (Instance != null && Instance._cameraRig != null)
		{
			return Instance._cameraRig.trackingSpace;
		}
		Debug.LogError("OVRCameraRig is not present, but MRUK requires it. Please add OVRCameraRig to your scene via 'Meta / Tools / Building Blocks / Camera Rig'.");
		return null;
	}

	private void Update()
	{
		if (SceneSettings.LoadSceneOnStartup)
		{
			_ = _loadSceneCalled;
		}
		UpdateAnchorStore();
		bool worldLockActive = false;
		if ((bool)_cameraRig)
		{
			if (EnableWorldLock)
			{
				MRUKRoom currentRoom = GetCurrentRoom();
				if ((bool)currentRoom)
				{
					Pose offset = Pose.identity;
					if (MRUKNativeFuncs.AnchorStoreGetWorldLockOffset(currentRoom.Anchor.Uuid, ref offset))
					{
						Pose? prevTrackingSpacePose = _prevTrackingSpacePose;
						if (prevTrackingSpacePose.HasValue)
						{
							Pose valueOrDefault = prevTrackingSpacePose.GetValueOrDefault();
							if (_cameraRig.trackingSpace.position != valueOrDefault.position || _cameraRig.trackingSpace.rotation != valueOrDefault.rotation)
							{
								Debug.LogWarning("MRUK EnableWorldLock is enabled and is controlling the tracking space position.\n" + $"Tracking position was set to {_cameraRig.trackingSpace.position} and rotation to {_cameraRig.trackingSpace.rotation}, this is being overridden by MRUK.\n" + "Use 'TrackingSpaceOffset' instead to translate or rotate the TrackingSpace.");
							}
						}
						offset = FlipZ(offset);
						Pose lhs = (((object)currentRoom.FloorAnchor == null || !currentRoom.FloorAnchor.HasValidHandle) ? currentRoom.DeltaPose : currentRoom.FloorAnchor.DeltaPose);
						lhs = offset.GetTransformedBy(lhs);
						Vector3 position = TrackingSpaceOffset.MultiplyPoint3x4(lhs.position);
						Quaternion rotation = TrackingSpaceOffset.rotation * lhs.rotation;
						_cameraRig.trackingSpace.SetPositionAndRotation(position, rotation);
						_prevTrackingSpacePose = new Pose(position, rotation);
						worldLockActive = true;
					}
				}
			}
			else if (_worldLockWasEnabled)
			{
				_cameraRig.trackingSpace.localPosition = Vector3.zero;
				_cameraRig.trackingSpace.localRotation = Quaternion.identity;
				_prevTrackingSpacePose = null;
			}
			_worldLockWasEnabled = EnableWorldLock;
		}
		_worldLockActive = worldLockActive;
		UpdateTrackables();
	}

	internal async Task LoadScene(SceneDataSource dataSource)
	{
		_loadSceneCalled = true;
		try
		{
			if (dataSource == SceneDataSource.Device || dataSource == SceneDataSource.DeviceWithPrefabFallback || dataSource == SceneDataSource.DeviceWithJsonFallback)
			{
				await LoadSceneFromDevice();
			}
			if (dataSource == SceneDataSource.Prefab || (dataSource == SceneDataSource.DeviceWithPrefabFallback && Rooms.Count == 0))
			{
				if (SceneSettings.RoomPrefabs.Length == 0)
				{
					Debug.LogWarning("Failed to load room from prefab because prefabs list is empty");
					return;
				}
				int roomIndex = GetRoomIndex();
				Debug.Log($"Loading prefab room {roomIndex}");
				GameObject scenePrefab = SceneSettings.RoomPrefabs[roomIndex];
				await LoadSceneFromPrefab(scenePrefab);
			}
			switch (dataSource)
			{
			case SceneDataSource.DeviceWithJsonFallback:
				if (Rooms.Count != 0)
				{
					break;
				}
				goto case SceneDataSource.Json;
			case SceneDataSource.Json:
				if (SceneSettings.SceneJsons.Length != 0)
				{
					int roomIndex2 = GetRoomIndex(fromPrefabs: false);
					Debug.Log($"Loading SceneJson {roomIndex2}");
					TextAsset textAsset = SceneSettings.SceneJsons[roomIndex2];
					await LoadSceneFromJsonString(textAsset.text);
				}
				else if (SceneSettings.SceneJson != "")
				{
					await LoadSceneFromJsonString(SceneSettings.SceneJson);
				}
				else
				{
					Debug.LogWarning("The list of SceneJsons is empty");
				}
				break;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			throw;
		}
	}

	private int GetRoomIndex(bool fromPrefabs = true)
	{
		int num = SceneSettings.RoomIndex;
		if (num == -1)
		{
			num = UnityEngine.Random.Range(0, fromPrefabs ? SceneSettings.RoomPrefabs.Length : SceneSettings.SceneJsons.Length);
		}
		return num;
	}

	internal void OnRoomDestroyed(MRUKRoom room)
	{
		Rooms.Remove(room);
		if (_cachedCurrentRoom == room)
		{
			_cachedCurrentRoom = null;
		}
	}

	public void ClearScene()
	{
		ClearSceneSharedLib();
	}

	public async Task<LoadDeviceResult> LoadSceneFromSharedRooms(IEnumerable<Guid> roomUuids, Guid groupUuid, (Guid alignmentRoomUuid, Pose floorWorldPoseOnHost)? alignmentData, bool removeMissingRooms = true)
	{
		if (groupUuid == Guid.Empty)
		{
			throw new ArgumentException("groupUuid");
		}
		if (alignmentData?.alignmentRoomUuid == Guid.Empty)
		{
			throw new ArgumentException("alignmentRoomUuid");
		}
		return await LoadSceneFromDeviceInternal(requestSceneCaptureIfNoDataFound: false, removeMissingRooms, new SharedRoomsData
		{
			roomUuids = roomUuids,
			groupUuid = groupUuid,
			alignmentData = alignmentData
		});
	}

	public Task<LoadDeviceResult> LoadSceneFromSharedRooms(Guid groupUuid, (Guid alignmentRoomUuid, Pose floorWorldPoseOnHost)? alignmentData, bool removeMissingRooms = true)
	{
		return LoadSceneFromSharedRooms(null, groupUuid, alignmentData, removeMissingRooms);
	}

	public async OVRTask<OVRResult<OVRAnchor.ShareResult>> ShareRoomsAsync(IEnumerable<MRUKRoom> rooms, Guid groupUuid)
	{
		if (rooms == null)
		{
			throw new ArgumentNullException("rooms");
		}
		if (groupUuid == Guid.Empty)
		{
			throw new ArgumentException("groupUuid");
		}
		List<OVRAnchor> roomAnchors;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out roomAnchors))
		{
			List<OVRTask<bool>> list = new List<OVRTask<bool>>();
			foreach (MRUKRoom room in rooms)
			{
				if (!room.IsLocal)
				{
					Debug.LogError("Sharing JSON or Prefab rooms is not supported. Only rooms loaded from device (MRUKRoom.IsLocal == true) can be shared.");
					return OVRResult<OVRAnchor.ShareResult>.FromFailure(OVRAnchor.ShareResult.FailureOperationFailed);
				}
				if (room.Anchor.TryGetComponent<OVRSharable>(out var component))
				{
					list.Add(component.SetEnabledAsync(enabled: true));
				}
				roomAnchors.Add(room.Anchor);
			}
			await OVRTask.WhenAll(list);
			return await OVRAnchor.ShareAsync(roomAnchors, groupUuid);
		}
	}

	public async Task<LoadDeviceResult> LoadSceneFromDevice(bool requestSceneCaptureIfNoDataFound = true, bool removeMissingRooms = true)
	{
		return await LoadSceneFromDeviceInternal(requestSceneCaptureIfNoDataFound, removeMissingRooms);
	}

	private async Task<LoadDeviceResult> LoadSceneFromDeviceInternal(bool requestSceneCaptureIfNoDataFound, bool removeMissingRooms, SharedRoomsData? sharedRoomsData = null)
	{
		LoadDeviceResult loadDeviceResult = await LoadSceneFromDeviceSharedLib(requestSceneCaptureIfNoDataFound, removeMissingRooms, sharedRoomsData);
		OVRTelemetry.Start(651892966, 0, -1L).AddAnnotation("NumRooms", Rooms.Count.ToString()).SetResult((loadDeviceResult == LoadDeviceResult.Success) ? OVRPlugin.Qpl.ResultType.Success : OVRPlugin.Qpl.ResultType.Fail)
			.Send();
		return loadDeviceResult;
	}

	private void FindAllObjects(GameObject roomPrefab, out List<GameObject> walls, out List<GameObject> volumes, out List<GameObject> planes)
	{
		walls = new List<GameObject>();
		volumes = new List<GameObject>();
		planes = new List<GameObject>();
		FindObjects(MRUKAnchor.SceneLabels.WALL_FACE.ToString(), roomPrefab.transform, ref walls);
		FindObjects(MRUKAnchor.SceneLabels.INVISIBLE_WALL_FACE.ToString(), roomPrefab.transform, ref walls);
		FindObjects(MRUKAnchor.SceneLabels.OTHER.ToString(), roomPrefab.transform, ref volumes);
		FindObjects(MRUKAnchor.SceneLabels.TABLE.ToString(), roomPrefab.transform, ref volumes);
		FindObjects(MRUKAnchor.SceneLabels.COUCH.ToString(), roomPrefab.transform, ref volumes);
		FindObjects(MRUKAnchor.SceneLabels.WINDOW_FRAME.ToString(), roomPrefab.transform, ref planes);
		FindObjects(MRUKAnchor.SceneLabels.DOOR_FRAME.ToString(), roomPrefab.transform, ref planes);
		FindObjects(MRUKAnchor.SceneLabels.WALL_ART.ToString(), roomPrefab.transform, ref planes);
		FindObjects(MRUKAnchor.SceneLabels.PLANT.ToString(), roomPrefab.transform, ref volumes);
		FindObjects(MRUKAnchor.SceneLabels.SCREEN.ToString(), roomPrefab.transform, ref volumes);
		FindObjects(MRUKAnchor.SceneLabels.BED.ToString(), roomPrefab.transform, ref volumes);
		FindObjects(MRUKAnchor.SceneLabels.LAMP.ToString(), roomPrefab.transform, ref volumes);
		FindObjects(MRUKAnchor.SceneLabels.STORAGE.ToString(), roomPrefab.transform, ref volumes);
	}

	public async Task<LoadDeviceResult> LoadSceneFromPrefab(GameObject scenePrefab, bool clearSceneFirst = true)
	{
		if (clearSceneFirst)
		{
			ClearScene();
		}
		LoadDeviceResult loadDeviceResult = await LoadSceneFromPrefabSharedLib(scenePrefab);
		OVRTelemetry.Start(651889651, 0, -1L).AddAnnotation("SceneName", scenePrefab.name).AddAnnotation("NumRooms", Rooms.Count.ToString())
			.SetResult((loadDeviceResult == LoadDeviceResult.Success) ? OVRPlugin.Qpl.ResultType.Success : OVRPlugin.Qpl.ResultType.Fail)
			.Send();
		return loadDeviceResult;
	}

	[Obsolete("Coordinate system is now obsolete, use the overload that doesn't take this parameter")]
	public string SaveSceneToJsonString(SerializationHelpers.CoordinateSystem coordinateSystem = SerializationHelpers.CoordinateSystem.Unity, bool includeGlobalMesh = true, List<MRUKRoom> rooms = null)
	{
		return SaveSceneToJsonString(includeGlobalMesh, rooms);
	}

	public string SaveSceneToJsonString(bool includeGlobalMesh = true, List<MRUKRoom> rooms = null)
	{
		return SaveSceneToJsonSharedLib(includeGlobalMesh, rooms);
	}

	public async Task<LoadDeviceResult> LoadSceneFromJsonString(string jsonString, bool removeMissingRooms = true)
	{
		LoadDeviceResult loadDeviceResult = await LoadSceneFromJsonSharedLib(jsonString, removeMissingRooms);
		OVRTelemetry.Start(651895197, 0, -1L).AddAnnotation("NumRooms", Rooms.Count.ToString()).SetResult((loadDeviceResult == LoadDeviceResult.Success) ? OVRPlugin.Qpl.ResultType.Success : OVRPlugin.Qpl.ResultType.Fail)
			.Send();
		return loadDeviceResult;
	}

	private void FindObjects(string objName, Transform rootTransform, ref List<GameObject> objList)
	{
		if (rootTransform.name.Equals(objName))
		{
			objList.Add(rootTransform.gameObject);
		}
		foreach (Transform item in rootTransform)
		{
			FindObjects(objName, item, ref objList);
		}
	}

	private void InitializeAnchorStore()
	{
		if (IsOpenXRAvailable)
		{
			ulong nativeOpenXRInstance = OVRPlugin.GetNativeOpenXRInstance();
			ulong nativeOpenXRSession = OVRPlugin.GetNativeOpenXRSession();
			IntPtr openXRInstanceProcAddrFunc = OVRPlugin.GetOpenXRInstanceProcAddrFunc();
			_currentAppSpace = OVRPlugin.GetAppSpace();
			if (MRUKNativeFuncs.AnchorStoreCreate(nativeOpenXRInstance, nativeOpenXRSession, openXRInstanceProcAddrFunc, _currentAppSpace, null, 0u) != 0)
			{
				Debug.LogError("Failed to create anchor store");
			}
			else
			{
				_openXrInitialised = true;
			}
			if (OVRPlugin.RegisterOpenXREventHandler(OnOpenXrEvent) != 0)
			{
				Debug.LogError("Failed to register OpenXR event handler");
			}
		}
		else
		{
			MRUKNativeFuncs.AnchorStoreCreateWithoutOpenXr();
		}
		MRUKNativeFuncs.MrukEventListener listener = default(MRUKNativeFuncs.MrukEventListener);
		listener.userContext = IntPtr.Zero;
		listener.onPreRoomAnchorAdded = OnPreRoomAnchorAdded;
		listener.onRoomAnchorAdded = OnRoomAnchorAdded;
		listener.onRoomAnchorUpdated = OnRoomAnchorUpdated;
		listener.onRoomAnchorRemoved = OnRoomAnchorRemoved;
		listener.onSceneAnchorAdded = OnSceneAnchorAdded;
		listener.onSceneAnchorUpdated = OnSceneAnchorUpdated;
		listener.onSceneAnchorRemoved = OnSceneAnchorRemoved;
		listener.onDiscoveryFinished = OnDiscoveryFinished;
		listener.onEnvironmentRaycasterCreated = OnEnvironmentRaycasterCreated;
		MRUKNativeFuncs.AnchorStoreRegisterEventListener(listener);
		MRUKNativeFuncs.SetTrackingSpacePoseGetter(GetTrackingSpacePose);
		MRUKNativeFuncs.SetTrackingSpacePoseSetter(SetTrackingSpacePose);
	}

	private void DestroyAnchorStore()
	{
		if (IsOpenXRAvailable)
		{
			OVRPlugin.UnregisterOpenXREventHandler(OnOpenXrEvent);
		}
		MRUKNativeFuncs.AnchorStoreDestroy();
	}

	private void UpdateAnchorStore()
	{
		if (IsOpenXRAvailable)
		{
			ulong appSpace = OVRPlugin.GetAppSpace();
			if (appSpace != _currentAppSpace)
			{
				MRUKNativeFuncs.AnchorStoreSetBaseSpace(appSpace);
				_currentAppSpace = appSpace;
			}
		}
		else if (_openXrInitialised)
		{
			MRUKNativeFuncs.AnchorStoreShutdownOpenXr();
			_openXrInitialised = false;
		}
		ulong nextPredictedDisplayTime = (OVRPlugin.initialized ? ((ulong)(OVRPlugin.GetPredictedDisplayTime() * 1000000000.0)) : 0);
		MRUKNativeFuncs.AnchorStoreTick(nextPredictedDisplayTime);
	}

	private unsafe async Task<LoadDeviceResult> LoadSceneFromDeviceSharedLib(bool requestSceneCaptureIfNoDataFound, bool removeMissingRooms, SharedRoomsData? sharedRoomsData = null)
	{
		if (_loadSceneTask.HasValue)
		{
			return LoadDeviceResult.DiscoveryOngoing;
		}
		if (!IsOpenXRAvailable)
		{
			return LoadDeviceResult.NotInitialized;
		}
		MRUKNativeFuncs.MrukSceneModel sceneModel = MRUKNativeFuncs.MrukSceneModel.V1;
		MRUKNativeFuncs.MrukResult mrukResult;
		if (sharedRoomsData.HasValue)
		{
			SharedRoomsData valueOrDefault = sharedRoomsData.GetValueOrDefault();
			using OVRNativeList<Guid> oVRNativeList = valueOrDefault.roomUuids?.ToNativeList(Allocator.Temp) ?? default(OVRNativeList<Guid>);
			MRUKNativeFuncs.MrukSharedRoomsData mrukSharedRoomsData = default(MRUKNativeFuncs.MrukSharedRoomsData);
			mrukSharedRoomsData.groupUuid = valueOrDefault.groupUuid;
			mrukSharedRoomsData.roomUuids = oVRNativeList.Data;
			mrukSharedRoomsData.numRoomUuids = (uint)oVRNativeList.Count;
			MRUKNativeFuncs.MrukSharedRoomsData sharedRoomsData2 = mrukSharedRoomsData;
			if (valueOrDefault.alignmentData.HasValue)
			{
				sharedRoomsData2.alignmentRoomUuid = valueOrDefault.alignmentData.Value.alignmentRoomUuid;
				sharedRoomsData2.roomWorldPoseOnHost = FlipZRotateY180(valueOrDefault.alignmentData.Value.floorWorldPoseOnHost);
			}
			mrukResult = MRUKNativeFuncs.AnchorStoreStartQueryByLocalGroup(sharedRoomsData2, removeMissingRooms, sceneModel);
		}
		else
		{
			mrukResult = MRUKNativeFuncs.AnchorStoreStartDiscovery(removeMissingRooms, sceneModel);
		}
		if (mrukResult != 0)
		{
			return ConvertResult(mrukResult);
		}
		LoadDeviceResult result = await WaitForDiscoveryFinished();
		if (result == LoadDeviceResult.NoRoomsFound)
		{
			bool flag = requestSceneCaptureIfNoDataFound;
			if (flag)
			{
				flag = await OVRScene.RequestSpaceSetup();
			}
			if (flag)
			{
				return await LoadSceneFromDeviceSharedLib(requestSceneCaptureIfNoDataFound: false, removeMissingRooms);
			}
		}
		if (result == LoadDeviceResult.Success)
		{
			InitializeScene();
		}
		return result;
	}

	private async Task<LoadDeviceResult> LoadSceneFromJsonSharedLib(string jsonString, bool removeMissingRooms = true)
	{
		if (_loadSceneTask.HasValue)
		{
			return LoadDeviceResult.DiscoveryOngoing;
		}
		MRUKNativeFuncs.MrukResult mrukResult = MRUKNativeFuncs.AnchorStoreLoadSceneFromJson(jsonString, removeMissingRooms, MRUKNativeFuncs.MrukSceneModel.V2FallbackV1);
		if (mrukResult != 0)
		{
			return ConvertResult(mrukResult);
		}
		LoadDeviceResult num = await WaitForDiscoveryFinished();
		if (num == LoadDeviceResult.Success)
		{
			InitializeScene();
		}
		return num;
	}

	private unsafe string SaveSceneToJsonSharedLib(bool includeGlobalMesh, List<MRUKRoom> rooms)
	{
		Guid[] array = null;
		if (rooms != null)
		{
			array = new Guid[rooms.Count];
			for (int i = 0; i < rooms.Count; i++)
			{
				array[i] = rooms[i].Anchor.Uuid;
			}
		}
		char* ptr = MRUKNativeFuncs.AnchorStoreSaveSceneToJson(includeGlobalMesh, array, (array != null) ? ((uint)array.Length) : 0u);
		string result = Marshal.PtrToStringUTF8((IntPtr)ptr);
		MRUKNativeFuncs.AnchorStoreFreeJson(ptr);
		return result;
	}

	private unsafe async Task<LoadDeviceResult> LoadSceneFromPrefabSharedLib(GameObject scenePrefab)
	{
		if (_loadSceneTask.HasValue)
		{
			return LoadDeviceResult.DiscoveryOngoing;
		}
		List<GameObject> list = new List<GameObject>();
		if (scenePrefab.transform.childCount > 0 && scenePrefab.transform.GetChild(0).childCount > 0)
		{
			foreach (Transform item8 in scenePrefab.transform)
			{
				list.Add(item8.gameObject);
			}
		}
		else
		{
			list.Add(scenePrefab);
		}
		List<MRUKNativeFuncs.MrukRoomAnchor> list2 = new List<MRUKNativeFuncs.MrukRoomAnchor>();
		List<MRUKNativeFuncs.MrukSceneAnchor> list3 = new List<MRUKNativeFuncs.MrukSceneAnchor>();
		List<GCHandle> list4 = new List<GCHandle>();
		MRUKNativeFuncs.MrukResult mrukResult;
		try
		{
			foreach (GameObject item9 in list)
			{
				FindAllObjects(item9, out var walls, out var volumes, out var planes);
				MRUKNativeFuncs.MrukRoomAnchor mrukRoomAnchor = default(MRUKNativeFuncs.MrukRoomAnchor);
				mrukRoomAnchor.uuid = Guid.NewGuid();
				mrukRoomAnchor.pose = Pose.identity;
				MRUKNativeFuncs.MrukRoomAnchor item = mrukRoomAnchor;
				List<MRUKNativeFuncs.MrukSceneAnchor> list5 = new List<MRUKNativeFuncs.MrukSceneAnchor>();
				List<MRUKNativeFuncs.MrukSceneAnchor> list6 = new List<MRUKNativeFuncs.MrukSceneAnchor>();
				List<Vector3> list7 = new List<Vector3>();
				float num = 0f;
				for (int i = 0; i < walls.Count; i++)
				{
					if (i == 0)
					{
						num = walls[i].transform.localScale.y;
					}
					MRUKNativeFuncs.MrukSceneAnchor item2 = CreateMrukSceneAnchor(walls[i].name, list4, walls[i].transform.position, walls[i].transform.rotation, walls[i].transform.localScale, AnchorRepresentation.PLANE);
					item2.roomUuid = item.uuid;
					item2.pose.rotation *= Quaternion.AngleAxis(180f, Vector3.up);
					list6.Add(item2);
				}
				int thisID = 0;
				for (int j = 0; j < list6.Count; j++)
				{
					MRUKNativeFuncs.MrukSceneAnchor adjacentMrukSceneWall = GetAdjacentMrukSceneWall(ref thisID, list6);
					list5.Add(adjacentMrukSceneWall);
					MRUKNativeFuncs.MrukPlane plane = adjacentMrukSceneWall.plane;
					Vector3 item3 = adjacentMrukSceneWall.pose.position + adjacentMrukSceneWall.pose.rotation * new Vector3(plane.x + plane.width, plane.y, 0f);
					list7.Add(item3);
				}
				for (int k = 0; k < list5.Count; k++)
				{
					MRUKNativeFuncs.MrukPlane plane2 = list5[k].plane;
					Vector3 vector = list7[k];
					int index = ((k != list5.Count - 1) ? (k + 1) : 0);
					Vector3 vector2 = list7[index];
					Vector3 lhs = vector - vector2;
					lhs.y = 0f;
					float magnitude = lhs.magnitude;
					lhs /= magnitude;
					Vector3 up = Vector3.up;
					Vector3 forward = Vector3.Cross(lhs, up);
					Vector3 position = (vector + vector2) * 0.5f + Vector3.up * (plane2.height * 0.5f);
					Quaternion rotation = Quaternion.LookRotation(forward, up);
					MRUKNativeFuncs.MrukPlane mrukPlane = default(MRUKNativeFuncs.MrukPlane);
					mrukPlane.x = -0.5f * magnitude;
					mrukPlane.y = plane2.y;
					mrukPlane.width = magnitude;
					mrukPlane.height = plane2.height;
					MRUKNativeFuncs.MrukPlane plane3 = mrukPlane;
					MRUKNativeFuncs.MrukSceneAnchor mrukSceneAnchor = list5[k];
					mrukSceneAnchor.pose.position = position;
					mrukSceneAnchor.pose.rotation = rotation;
					mrukSceneAnchor.plane = plane3;
					*mrukSceneAnchor.planeBoundary = new Vector2(plane3.x, plane3.y);
					mrukSceneAnchor.planeBoundary[1] = new Vector2(plane3.x + plane3.width, plane3.y);
					mrukSceneAnchor.planeBoundary[2] = new Vector2(plane3.x + plane3.width, plane3.y + plane3.height);
					mrukSceneAnchor.planeBoundary[3] = new Vector2(plane3.x, plane3.y + plane3.height);
					list5[k] = mrukSceneAnchor;
					list3.Add(mrukSceneAnchor);
				}
				list3.Reverse();
				foreach (GameObject item10 in volumes)
				{
					Vector3 objScale = new Vector3(item10.transform.localScale.x, item10.transform.localScale.z, item10.transform.localScale.y);
					AnchorRepresentation anchorRepresentation = AnchorRepresentation.VOLUME;
					if (item10.transform.name == MRUKAnchor.SceneLabels.TABLE.ToString() || item10.transform.name == MRUKAnchor.SceneLabels.COUCH.ToString() || item10.transform.name == MRUKAnchor.SceneLabels.BED.ToString() || item10.transform.name == MRUKAnchor.SceneLabels.STORAGE.ToString())
					{
						anchorRepresentation |= AnchorRepresentation.PLANE;
					}
					MRUKNativeFuncs.MrukSceneAnchor item4 = CreateMrukSceneAnchor(item10.name, list4, item10.transform.position, item10.transform.rotation, objScale, anchorRepresentation);
					item4.roomUuid = item.uuid;
					if (item4.semanticLabel != MRUKNativeFuncs.MrukLabel.Couch)
					{
						item4.pose.position += objScale.z * 0.5f * Vector3.up;
					}
					item4.pose.rotation *= Quaternion.AngleAxis(-90f, Vector3.right);
					list3.Add(item4);
				}
				foreach (GameObject item11 in planes)
				{
					MRUKNativeFuncs.MrukSceneAnchor item5 = CreateMrukSceneAnchor(item11.name, list4, item11.transform.position, item11.transform.rotation, item11.transform.localScale, AnchorRepresentation.PLANE);
					item5.roomUuid = item.uuid;
					item5.pose.rotation *= Quaternion.AngleAxis(180f, Vector3.up);
					list3.Add(item5);
				}
				MRUKNativeFuncs.MrukSceneAnchor mrukSceneAnchor2 = default(MRUKNativeFuncs.MrukSceneAnchor);
				float num2 = 0f;
				foreach (MRUKNativeFuncs.MrukSceneAnchor item12 in list5)
				{
					float width = item12.plane.width;
					if (width > num2)
					{
						num2 = width;
						mrukSceneAnchor2 = item12;
					}
				}
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				Quaternion quaternion = Quaternion.Inverse(mrukSceneAnchor2.pose.rotation);
				for (int l = 0; l < list7.Count; l++)
				{
					Vector3 vector3 = quaternion * (list7[l] - mrukSceneAnchor2.pose.position);
					num3 = ((l == 0) ? vector3.z : Mathf.Min(num3, vector3.z));
					num4 = ((l == 0) ? vector3.z : Mathf.Max(num4, vector3.z));
					num5 = ((l == 0) ? vector3.x : Mathf.Min(num5, vector3.x));
					num6 = ((l == 0) ? vector3.x : Mathf.Max(num6, vector3.x));
				}
				Vector3 vector4 = new Vector3((num5 + num6) * 0.5f, 0f, (num3 + num4) * 0.5f);
				Vector3 vector5 = mrukSceneAnchor2.pose.position + mrukSceneAnchor2.pose.rotation * vector4;
				vector5 -= Vector3.up * num * 0.5f;
				Vector3 objScale2 = new Vector3(num4 - num3, num6 - num5, 1f);
				for (int m = 0; m < 2; m++)
				{
					string semanticLabel = ((m == 0) ? "FLOOR" : "CEILING");
					Vector3 position2 = vector5 + Vector3.up * num * m;
					float num7 = ((m == 0) ? 1 : (-1));
					Quaternion rotation2 = Quaternion.LookRotation(mrukSceneAnchor2.pose.up * num7, mrukSceneAnchor2.pose.right);
					MRUKNativeFuncs.MrukSceneAnchor item6 = CreateMrukSceneAnchor(semanticLabel, list4, position2, rotation2, objScale2, AnchorRepresentation.PLANE);
					item6.roomUuid = item.uuid;
					Quaternion quaternion2 = Quaternion.Inverse(item6.pose.rotation);
					Vector2[] array = new Vector2[list7.Count];
					int num8 = 0;
					foreach (Vector3 item13 in list7)
					{
						Vector3 vector6 = quaternion2 * (item13 - item6.pose.position);
						array[num8++] = new Vector2(vector6.x, vector6.y);
					}
					if (m == 1)
					{
						Array.Reverse(array);
					}
					GCHandle item7 = GCHandle.Alloc(array, GCHandleType.Pinned);
					list4.Add(item7);
					item6.planeBoundary = (Vector2*)(void*)item7.AddrOfPinnedObject();
					item6.planeBoundaryCount = (uint)array.Length;
					item6.hasPlane = true;
					list3.Add(item6);
				}
				list2.Add(item);
			}
			for (int n = 0; n < list3.Count; n++)
			{
				MRUKNativeFuncs.MrukSceneAnchor value = list3[n];
				value.pose = FlipZRotateY180(value.pose);
				value.volume = ConvertVolume(value.volume);
				value.plane = ConvertPlane(value.plane);
				int num9 = 0;
				int num10 = (int)(value.planeBoundaryCount - 1);
				while (num9 < num10)
				{
					Vector2 vector7 = value.planeBoundary[num9];
					Vector2 vector8 = value.planeBoundary[num10];
					value.planeBoundary[num9] = FlipX(vector8);
					value.planeBoundary[num10] = FlipX(vector7);
					num9++;
					num10--;
				}
				if (num9 == num10)
				{
					value.planeBoundary[num9] = FlipX(value.planeBoundary[num9]);
				}
				list3[n] = value;
			}
			MRUKNativeFuncs.MrukRoomAnchor[] array3;
			try
			{
				MRUKNativeFuncs.MrukRoomAnchor[] array2 = list2.ToArray();
				array3 = array2;
				MRUKNativeFuncs.MrukRoomAnchor* roomAnchors = (MRUKNativeFuncs.MrukRoomAnchor*)((array2 != null && array3.Length != 0) ? Unsafe.AsPointer(ref array3[0]) : null);
				MRUKNativeFuncs.MrukSceneAnchor[] array5;
				try
				{
					MRUKNativeFuncs.MrukSceneAnchor[] array4 = list3.ToArray();
					array5 = array4;
					MRUKNativeFuncs.MrukSceneAnchor* sceneAnchors = (MRUKNativeFuncs.MrukSceneAnchor*)((array4 != null && array5.Length != 0) ? Unsafe.AsPointer(ref array5[0]) : null);
					mrukResult = MRUKNativeFuncs.AnchorStoreLoadSceneFromPrefab(roomAnchors, (uint)list2.Count, sceneAnchors, (uint)list3.Count);
				}
				finally
				{
					array5 = null;
				}
			}
			finally
			{
				array3 = null;
			}
		}
		finally
		{
			foreach (GCHandle item14 in list4)
			{
				item14.Free();
			}
		}
		if (mrukResult != 0)
		{
			return ConvertResult(mrukResult);
		}
		LoadDeviceResult num11 = await WaitForDiscoveryFinished();
		if (num11 == LoadDeviceResult.Success)
		{
			InitializeScene();
		}
		return num11;
	}

	private MRUKNativeFuncs.MrukSceneAnchor GetAdjacentMrukSceneWall(ref int thisID, List<MRUKNativeFuncs.MrukSceneAnchor> randomWalls)
	{
		Vector3 vector = new Vector2(randomWalls[thisID].plane.width, randomWalls[thisID].plane.height) * 0.5f;
		Vector3 b = randomWalls[thisID].pose.position - randomWalls[thisID].pose.up * vector.y - randomWalls[thisID].pose.right * vector.x;
		float num = float.PositiveInfinity;
		int num2 = 0;
		for (int i = 0; i < randomWalls.Count; i++)
		{
			if (i != thisID)
			{
				Vector2 vector2 = new Vector2(randomWalls[i].plane.width * 0.5f, randomWalls[i].plane.height * 0.5f);
				float num3 = Vector3.Distance(randomWalls[i].pose.position - randomWalls[i].pose.up * vector2.y + randomWalls[i].pose.right * vector2.x, b);
				if (num3 < num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		thisID = num2;
		return randomWalls[thisID];
	}

	private unsafe MRUKNativeFuncs.MrukSceneAnchor CreateMrukSceneAnchor(string semanticLabel, List<GCHandle> handles, Vector3 position, Quaternion rotation, Vector3 objScale, AnchorRepresentation representation)
	{
		MRUKNativeFuncs.MrukSceneAnchor mrukSceneAnchor = default(MRUKNativeFuncs.MrukSceneAnchor);
		mrukSceneAnchor.semanticLabel = (MRUKNativeFuncs.MrukLabel)(1 << (int)OVRSemanticLabels.FromApiLabel(semanticLabel));
		MRUKNativeFuncs.MrukSceneAnchor result = mrukSceneAnchor;
		result.pose.position = position;
		result.pose.rotation = rotation;
		if ((representation & AnchorRepresentation.PLANE) != 0)
		{
			MRUKNativeFuncs.MrukPlane plane = default(MRUKNativeFuncs.MrukPlane);
			plane.x = -0.5f * objScale.x;
			plane.y = -0.5f * objScale.y;
			plane.width = objScale.x;
			plane.height = objScale.y;
			MRUKNativeFuncs.MrukPlane mrukPlane = (result.plane = plane);
			Vector2[] array = new Vector2[4]
			{
				new Vector2(mrukPlane.x, mrukPlane.y),
				new Vector2(mrukPlane.x + mrukPlane.width, mrukPlane.y),
				new Vector2(mrukPlane.x + mrukPlane.width, mrukPlane.y + mrukPlane.height),
				new Vector2(mrukPlane.x, mrukPlane.y + mrukPlane.height)
			};
			GCHandle item = GCHandle.Alloc(array, GCHandleType.Pinned);
			handles.Add(item);
			result.planeBoundary = (Vector2*)(void*)item.AddrOfPinnedObject();
			result.planeBoundaryCount = (uint)array.Length;
			result.hasPlane = true;
		}
		if ((representation & AnchorRepresentation.VOLUME) != 0)
		{
			Vector3 vector = new Vector3(0f, 0f, (0f - objScale.z) * 0.5f);
			if (result.semanticLabel == MRUKNativeFuncs.MrukLabel.Couch)
			{
				vector = Vector3.zero;
			}
			result.volume = new MRUKNativeFuncs.MrukVolume
			{
				min = vector - 0.5f * objScale,
				max = vector + 0.5f * objScale
			};
			result.hasVolume = true;
		}
		result.uuid = Guid.NewGuid();
		return result;
	}

	private void ClearSceneSharedLib()
	{
		MRUKNativeFuncs.AnchorStoreClearRooms();
	}

	private static Vector2 FlipX(Vector2 vector)
	{
		return new Vector2(0f - vector.x, vector.y);
	}

	private static Vector3 FlipX(Vector3 vector)
	{
		return new Vector3(0f - vector.x, vector.y, vector.z);
	}

	private static Vector3 FlipZ(Vector3 vector)
	{
		return new Vector3(vector.x, vector.y, 0f - vector.z);
	}

	private static Quaternion FlipZ(Quaternion quaternion)
	{
		return new Quaternion(0f - quaternion.x, 0f - quaternion.y, quaternion.z, quaternion.w);
	}

	private static Quaternion FlipZRotateY180(Quaternion rotation)
	{
		return new Quaternion(0f - rotation.z, rotation.w, 0f - rotation.x, rotation.y);
	}

	private static Pose FlipZ(Pose pose)
	{
		return new Pose(FlipZ(pose.position), FlipZ(pose.rotation));
	}

	internal static Pose FlipZRotateY180(Pose pose)
	{
		return new Pose(FlipZ(pose.position), FlipZRotateY180(pose.rotation));
	}

	private static MRUKNativeFuncs.MrukVolume ConvertVolume(MRUKNativeFuncs.MrukVolume volume)
	{
		Vector3 min = volume.min;
		Vector3 max = volume.max;
		MRUKNativeFuncs.MrukVolume result = default(MRUKNativeFuncs.MrukVolume);
		result.min = new Vector3(0f - max.x, min.y, min.z);
		result.max = new Vector3(0f - min.x, max.y, max.z);
		return result;
	}

	private static MRUKNativeFuncs.MrukPlane ConvertPlane(MRUKNativeFuncs.MrukPlane plane)
	{
		MRUKNativeFuncs.MrukPlane result = default(MRUKNativeFuncs.MrukPlane);
		result.x = 0f - (plane.x + plane.width);
		result.y = plane.y;
		result.width = plane.width;
		result.height = plane.height;
		return result;
	}

	private MRUKRoom FindRoomByUuid(Guid uuid)
	{
		foreach (MRUKRoom room in Rooms)
		{
			if (room.Anchor.Uuid == uuid)
			{
				return room;
			}
		}
		return null;
	}

	private unsafe static void UpdateAnchorProperties(MRUKAnchor anchor, ref MRUKNativeFuncs.MrukSceneAnchor sceneAnchor)
	{
		MRUKAnchor.SceneLabels sceneLabels = ConvertLabel(sceneAnchor.semanticLabel);
		string text = ((sceneLabels != 0) ? sceneLabels.ToString() : "UNDEFINED_ANCHOR");
		anchor.gameObject.name = text;
		Pose pose2 = (anchor.InitialPose = FlipZRotateY180(sceneAnchor.pose));
		anchor.gameObject.transform.SetPositionAndRotation(pose2.position, pose2.rotation);
		anchor.Label = sceneLabels;
		anchor.Anchor = new OVRAnchor(sceneAnchor.space, sceneAnchor.uuid);
		if (sceneAnchor.hasPlane)
		{
			anchor.PlaneBoundary2D = new List<Vector2>((int)sceneAnchor.planeBoundaryCount);
			for (int i = 0; i < sceneAnchor.planeBoundaryCount; i++)
			{
				Vector2 vector = sceneAnchor.planeBoundary[sceneAnchor.planeBoundaryCount - i - 1];
				anchor.PlaneBoundary2D.Add(FlipX(vector));
			}
			MRUKNativeFuncs.MrukPlane mrukPlane = ConvertPlane(sceneAnchor.plane);
			anchor.PlaneRect = new Rect(mrukPlane.x, mrukPlane.y, mrukPlane.width, mrukPlane.height);
		}
		else
		{
			anchor.PlaneBoundary2D = null;
			anchor.PlaneRect = null;
		}
		if (sceneAnchor.hasVolume)
		{
			MRUKNativeFuncs.MrukVolume mrukVolume = ConvertVolume(sceneAnchor.volume);
			anchor.VolumeBounds = new Bounds((mrukVolume.min + mrukVolume.max) / 2f, mrukVolume.max - mrukVolume.min);
		}
		else
		{
			anchor.VolumeBounds = null;
		}
		if (sceneAnchor.globalMeshPositionsCount != 0 && sceneAnchor.globalMeshIndicesCount != 0)
		{
			Vector3[] array = new Vector3[sceneAnchor.globalMeshPositionsCount];
			for (int j = 0; j < sceneAnchor.globalMeshPositionsCount; j++)
			{
				array[j] = FlipX(sceneAnchor.globalMeshPositions[j]);
			}
			int[] array2 = new int[sceneAnchor.globalMeshIndicesCount];
			for (int k = 0; k < sceneAnchor.globalMeshIndicesCount; k += 3)
			{
				array2[k] = (int)sceneAnchor.globalMeshIndices[k];
				array2[k + 1] = (int)sceneAnchor.globalMeshIndices[k + 2];
				array2[k + 2] = (int)sceneAnchor.globalMeshIndices[k + 1];
			}
			Mesh mesh = new Mesh
			{
				indexFormat = ((sceneAnchor.globalMeshIndicesCount > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16),
				vertices = array,
				triangles = array2
			};
			anchor.Mesh = mesh;
		}
		else
		{
			anchor.Mesh = null;
		}
	}

	[MonoPInvokeCallback(typeof(OVRPlugin.OpenXREventDelegateType))]
	private static void OnOpenXrEvent(IntPtr data, IntPtr context)
	{
		try
		{
			MRUKNativeFuncs.AnchorStoreOnOpenXrEvent(data);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnPreRoomAnchorAdded))]
	private static void OnPreRoomAnchorAdded(ref MRUKNativeFuncs.MrukRoomAnchor roomAnchor, IntPtr userContext)
	{
		try
		{
			Guid uuid = roomAnchor.uuid;
			MRUKRoom mRUKRoom = new GameObject($"Room - {uuid}").AddComponent<MRUKRoom>();
			mRUKRoom.Anchor = new OVRAnchor(roomAnchor.space, uuid);
			if (roomAnchor.pose != Pose.identity)
			{
				Pose pose2 = (mRUKRoom.InitialPose = FlipZRotateY180(roomAnchor.pose));
				mRUKRoom.transform.SetPositionAndRotation(pose2.position, pose2.rotation);
			}
			Instance.Rooms.Add(mRUKRoom);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnRoomAnchorAdded))]
	private static void OnRoomAnchorAdded(ref MRUKNativeFuncs.MrukRoomAnchor roomAnchor, IntPtr userContext)
	{
		try
		{
			MRUKRoom mRUKRoom = Instance.FindRoomByUuid(roomAnchor.uuid);
			mRUKRoom.ComputeRoomInfo();
			Instance.RoomCreatedEvent.Invoke(mRUKRoom);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnRoomAnchorUpdated))]
	private static void OnRoomAnchorUpdated(ref MRUKNativeFuncs.MrukRoomAnchor roomAnchor, ref Guid oldRoomAnchorUuid, bool significantChange, IntPtr userContext)
	{
		try
		{
			MRUKRoom mRUKRoom = Instance.FindRoomByUuid(oldRoomAnchorUuid);
			mRUKRoom.Anchor = new OVRAnchor(roomAnchor.space, roomAnchor.uuid);
			if (significantChange)
			{
				mRUKRoom.ComputeRoomInfo();
				Instance.RoomUpdatedEvent.Invoke(mRUKRoom);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnRoomAnchorRemoved))]
	private static void OnRoomAnchorRemoved(ref MRUKNativeFuncs.MrukRoomAnchor roomAnchor, IntPtr userContext)
	{
		try
		{
			MRUKRoom mRUKRoom = Instance.FindRoomByUuid(roomAnchor.uuid);
			Instance.RoomRemovedEvent.Invoke(mRUKRoom);
			Instance.Rooms.Remove(mRUKRoom);
			Utilities.DestroyGameObjectAndChildren(mRUKRoom.gameObject);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnSceneAnchorAdded))]
	private static void OnSceneAnchorAdded(ref MRUKNativeFuncs.MrukSceneAnchor sceneAnchor, IntPtr userContext)
	{
		try
		{
			MRUKRoom mRUKRoom = Instance.FindRoomByUuid(sceneAnchor.roomUuid);
			GameObject obj = new GameObject();
			MRUKAnchor mRUKAnchor = obj.AddComponent<MRUKAnchor>();
			mRUKAnchor.Room = mRUKRoom;
			obj.transform.SetParent(mRUKRoom.transform);
			UpdateAnchorProperties(mRUKAnchor, ref sceneAnchor);
			mRUKRoom.Anchors.Add(mRUKAnchor);
			if ((mRUKAnchor.Label & (MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.INVISIBLE_WALL_FACE)) != 0)
			{
				mRUKRoom.WallAnchors.Add(mRUKAnchor);
			}
			if ((mRUKAnchor.Label & MRUKAnchor.SceneLabels.CEILING) != 0)
			{
				mRUKRoom.CeilingAnchor = mRUKAnchor;
			}
			if ((mRUKAnchor.Label & MRUKAnchor.SceneLabels.FLOOR) != 0)
			{
				mRUKRoom.FloorAnchor = mRUKAnchor;
			}
			mRUKRoom.AnchorCreatedEvent.Invoke(mRUKAnchor);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnSceneAnchorUpdated))]
	private static void OnSceneAnchorUpdated(ref MRUKNativeFuncs.MrukSceneAnchor sceneAnchor, bool significantChange, IntPtr userContext)
	{
		try
		{
			MRUKRoom mRUKRoom = Instance.FindRoomByUuid(sceneAnchor.roomUuid);
			MRUKAnchor mRUKAnchor = mRUKRoom.FindAnchorByUuid(sceneAnchor.uuid);
			UpdateAnchorProperties(mRUKAnchor, ref sceneAnchor);
			if (significantChange)
			{
				mRUKRoom.AnchorUpdatedEvent.Invoke(mRUKAnchor);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnSceneAnchorRemoved))]
	private static void OnSceneAnchorRemoved(ref MRUKNativeFuncs.MrukSceneAnchor sceneAnchor, IntPtr userContext)
	{
		try
		{
			MRUKRoom mRUKRoom = Instance.FindRoomByUuid(sceneAnchor.roomUuid);
			MRUKAnchor mRUKAnchor = mRUKRoom.FindAnchorByUuid(sceneAnchor.uuid);
			mRUKRoom.AnchorRemovedEvent.Invoke(mRUKAnchor);
			mRUKRoom.RemoveAndDestroyAnchor(mRUKAnchor);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnDiscoveryFinished))]
	private static void OnDiscoveryFinished(MRUKNativeFuncs.MrukResult result, IntPtr userContext)
	{
		try
		{
			Instance._loadSceneTask?.SetResult(ConvertResult(result));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[MonoPInvokeCallback(typeof(MRUKNativeFuncs.MrukOnEnvironmentRaycasterCreated))]
	private static void OnEnvironmentRaycasterCreated(MRUKNativeFuncs.MrukResult result, IntPtr userContext)
	{
	}

	private async Task<LoadDeviceResult> WaitForDiscoveryFinished()
	{
		_loadSceneTask = OVRTask.Create<LoadDeviceResult>(Guid.NewGuid());
		LoadDeviceResult result = await _loadSceneTask.Value;
		_loadSceneTask = null;
		return result;
	}

	private static LoadDeviceResult ConvertResult(MRUKNativeFuncs.MrukResult result)
	{
		return result switch
		{
			MRUKNativeFuncs.MrukResult.Success => LoadDeviceResult.Success, 
			MRUKNativeFuncs.MrukResult.ErrorDiscoveryOngoing => LoadDeviceResult.DiscoveryOngoing, 
			MRUKNativeFuncs.MrukResult.ErrorInvalidJson => LoadDeviceResult.FailureDataIsInvalid, 
			MRUKNativeFuncs.MrukResult.ErrorNoRoomsFound => LoadDeviceResult.NoRoomsFound, 
			MRUKNativeFuncs.MrukResult.ErrorInsufficientResources => LoadDeviceResult.FailureInsufficientResources, 
			MRUKNativeFuncs.MrukResult.ErrorStorageAtCapacity => LoadDeviceResult.StorageAtCapacity, 
			MRUKNativeFuncs.MrukResult.ErrorInsufficientView => LoadDeviceResult.FailureInsufficientView, 
			MRUKNativeFuncs.MrukResult.ErrorPermissionInsufficient => LoadDeviceResult.FailurePermissionInsufficient, 
			MRUKNativeFuncs.MrukResult.ErrorRateLimited => LoadDeviceResult.FailureRateLimited, 
			MRUKNativeFuncs.MrukResult.ErrorTooDark => LoadDeviceResult.FailureTooDark, 
			MRUKNativeFuncs.MrukResult.ErrorTooBright => LoadDeviceResult.FailureTooBright, 
			_ => LoadDeviceResult.Failure, 
		};
	}

	private static MRUKAnchor.SceneLabels ConvertLabel(MRUKNativeFuncs.MrukLabel label)
	{
		return (MRUKAnchor.SceneLabels)label;
	}

	public void GetTrackables(List<MRUKTrackable> trackables)
	{
		if (trackables == null)
		{
			throw new ArgumentNullException("trackables");
		}
		trackables.Clear();
		foreach (Transform value in _trackableTransforms.Values)
		{
			if ((bool)value && value.TryGetComponent<MRUKTrackable>(out var component))
			{
				trackables.Add(component);
			}
		}
	}

	private void OnEnable()
	{
		_trackerCoroutine = StartCoroutine(TrackerCoroutine());
	}

	private void UpdateTrackables()
	{
		List<OVRAnchor> list;
		OVRAnchor key;
		Transform value;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out list))
		{
			foreach (KeyValuePair<OVRAnchor, Transform> trackableTransform in _trackableTransforms)
			{
				trackableTransform.Deconstruct(out key, out value);
				OVRAnchor item = key;
				if (value == null)
				{
					list.Add(item);
				}
			}
			foreach (OVRAnchor item2 in list)
			{
				_trackableTransforms.Remove(item2);
				_trackableStates[item2] = TrackableState.InstanceDestroyed;
				item2.Dispose();
			}
		}
		List<OVRLocatable.TrackingSpacePose> list2;
		using (new OVRObjectPool.ListScope<OVRLocatable.TrackingSpacePose>(out list2))
		{
			OVRLocatable.UpdateSceneAnchorTransforms(_trackableTransforms, _cameraRig ? _cameraRig.trackingSpace : null, list2);
			using List<OVRLocatable.TrackingSpacePose>.Enumerator enumerator3 = list2.GetEnumerator();
			foreach (KeyValuePair<OVRAnchor, Transform> trackableTransform2 in _trackableTransforms)
			{
				trackableTransform2.Deconstruct(out key, out value);
				Transform obj = value;
				enumerator3.MoveNext();
				OVRLocatable.TrackingSpacePose current2 = enumerator3.Current;
				if (obj.TryGetComponent<MRUKTrackable>(out var component))
				{
					component.IsTracked = current2.IsPositionTracked && current2.IsRotationTracked;
				}
			}
		}
	}

	private void OnDisable()
	{
		if (_trackerCoroutine != null)
		{
			StopCoroutine(_trackerCoroutine);
			_trackerCoroutine = null;
		}
		_tracker.Dispose();
	}

	private async void ConfigureTrackerAndLogResult(OVRAnchor.TrackerConfiguration config)
	{
		OVRResult<OVRAnchor.ConfigureTrackerResult> oVRResult = await _tracker.ConfigureAsync(config);
		if ((bool)this && base.enabled)
		{
			if (oVRResult.Success)
			{
				Debug.Log($"Configured anchor trackers: {_tracker.Configuration}");
			}
			else
			{
				Debug.LogWarning($"Unable to fully satisfy requested tracker configuration. Requested={config}, Actual={_tracker.Configuration}");
			}
		}
	}

	private IEnumerator TrackerCoroutine()
	{
		List<OVRAnchor> anchors = new List<OVRAnchor>();
		HashSet<OVRAnchor> removed = new HashSet<OVRAnchor>();
		OVRAnchor.TrackerConfiguration lastConfig = default(OVRAnchor.TrackerConfiguration);
		bool hasScenePermission = Permission.HasUserAuthorizedPermission("com.oculus.permission.USE_SCENE");
		while (base.enabled)
		{
			double nextFetchTime = (double)Time.realtimeSinceStartup + TimeBetweenFetchTrackables.TotalSeconds;
			int startFrame = Time.frameCount;
			bool flag = false;
			if (!hasScenePermission && Permission.HasUserAuthorizedPermission("com.oculus.permission.USE_SCENE"))
			{
				flag = true;
				hasScenePermission = true;
			}
			if (lastConfig != SceneSettings.TrackerConfiguration || (flag && _tracker.Configuration != SceneSettings.TrackerConfiguration))
			{
				ConfigureTrackerAndLogResult(SceneSettings.TrackerConfiguration);
				lastConfig = SceneSettings.TrackerConfiguration;
			}
			OVRTask<OVRResult<List<OVRAnchor>, OVRAnchor.FetchResult>> task = _tracker.FetchTrackablesAsync(anchors);
			while (!task.IsCompleted)
			{
				yield return null;
				if (!base.enabled)
				{
					task.Dispose();
					yield break;
				}
			}
			if (task.GetResult().Success)
			{
				removed.Clear();
				foreach (OVRAnchor key in _trackableStates.Keys)
				{
					removed.Add(key);
				}
				foreach (OVRAnchor item in anchors)
				{
					Transform value;
					MRUKTrackable component2;
					if (_trackableStates.TryAdd(item, TrackableState.PendingLocalization))
					{
						if (item.TryGetComponent<OVRLocatable>(out var component))
						{
							LocalizeTrackable(item, component);
						}
					}
					else if (_trackableTransforms.TryGetValue(item, out value) && (bool)value && value.TryGetComponent<MRUKTrackable>(out component2) && (bool)component2)
					{
						try
						{
							component2.OnFetch();
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
					removed.Remove(item);
				}
				foreach (OVRAnchor item2 in removed)
				{
					_trackableStates.Remove(item2);
				}
				List<MRUKTrackable> list;
				using (new OVRObjectPool.ListScope<MRUKTrackable>(out list))
				{
					foreach (OVRAnchor item3 in removed)
					{
						if (_trackableTransforms.Remove(item3, out var value2) && (bool)value2 && value2.TryGetComponent<MRUKTrackable>(out var component3))
						{
							list.Add(component3);
						}
					}
					foreach (MRUKTrackable item4 in list)
					{
						try
						{
							item4.IsTracked = false;
							SceneSettings.TrackableRemoved.Invoke(item4);
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
						}
					}
				}
			}
			while (base.enabled && (startFrame == Time.frameCount || (double)Time.realtimeSinceStartup < nextFetchTime))
			{
				yield return null;
			}
		}
	}

	private async void LocalizeTrackable(OVRAnchor anchor, OVRLocatable locatable)
	{
		if (await locatable.SetEnabledAsync(enabled: true))
		{
			while ((bool)this)
			{
				if (!_trackableStates.TryGetValue(anchor, out var value) || value != 0)
				{
					return;
				}
				if (!base.enabled)
				{
					_trackableStates.Remove(anchor);
				}
				if (locatable.TryGetSceneAnchorPose(out var pose) && pose.Position.HasValue && pose.Rotation.HasValue)
				{
					GameObject gameObject = new GameObject($"Trackable({anchor.GetTrackableType()}) {anchor}");
					gameObject.transform.SetParent(_cameraRig.trackingSpace, worldPositionStays: false);
					gameObject.transform.SetLocalPositionAndRotation(pose.Position.Value, pose.Rotation.Value);
					_trackableTransforms[anchor] = gameObject.transform;
					MRUKTrackable mRUKTrackable = gameObject.AddComponent<MRUKTrackable>();
					mRUKTrackable.OnInstantiate(anchor);
					_trackableStates[anchor] = TrackableState.Instantiated;
					try
					{
						SceneSettings.TrackableAdded.Invoke(mRUKTrackable);
						return;
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						return;
					}
				}
				await Task.Yield();
			}
		}
		else
		{
			if ((bool)this)
			{
				_trackableStates[anchor] = TrackableState.LocalizationFailed;
			}
			Debug.LogError($"Unable to localize anchor {anchor}. Will not create a GameObject to represent it.");
		}
		anchor.Dispose();
	}
}
