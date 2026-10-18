using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Meta.XR.Util;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Serialization;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRSceneManager : MonoBehaviour
{
	[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
	public static class Classification
	{
		public const string Floor = "FLOOR";

		public const string Ceiling = "CEILING";

		public const string WallFace = "WALL_FACE";

		[Obsolete("Deprecated. Use Table classification instead.")]
		public const string Desk = "DESK";

		public const string Couch = "COUCH";

		public const string DoorFrame = "DOOR_FRAME";

		public const string WindowFrame = "WINDOW_FRAME";

		public const string Other = "OTHER";

		public const string Storage = "STORAGE";

		public const string Bed = "BED";

		public const string Screen = "SCREEN";

		public const string Lamp = "LAMP";

		public const string Plant = "PLANT";

		public const string Table = "TABLE";

		public const string WallArt = "WALL_ART";

		public const string InvisibleWallFace = "INVISIBLE_WALL_FACE";

		public const string GlobalMesh = "GLOBAL_MESH";

		public static IReadOnlyList<string> List { get; } = new string[17]
		{
			"FLOOR", "CEILING", "WALL_FACE", "DESK", "COUCH", "DOOR_FRAME", "WINDOW_FRAME", "OTHER", "STORAGE", "BED",
			"SCREEN", "LAMP", "PLANT", "TABLE", "WALL_ART", "INVISIBLE_WALL_FACE", "GLOBAL_MESH"
		};


		public static HashSet<string> Set { get; } = new HashSet<string>(List);

	}

	[Obsolete("RoomLayoutInformation is obsoleted. For each room's layout information (floor, ceiling, walls) see OVRSceneRoom.", false)]
	public class RoomLayoutInformation
	{
		public OVRScenePlane Floor;

		public OVRScenePlane Ceiling;

		public List<OVRScenePlane> Walls = new List<OVRScenePlane>();
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct LogForwarder
	{
		public void Log(string context, string message, GameObject gameObject = null)
		{
			UnityEngine.Debug.Log("[" + context + "] " + message, gameObject);
		}

		public void LogWarning(string context, string message, GameObject gameObject = null)
		{
			UnityEngine.Debug.LogWarning("[" + context + "] " + message, gameObject);
		}

		public void LogError(string context, string message, GameObject gameObject = null)
		{
			UnityEngine.Debug.LogError("[" + context + "] " + message, gameObject);
		}
	}

	internal static class Development
	{
		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void Log(string context, string message, GameObject gameObject = null)
		{
			UnityEngine.Debug.Log("[" + context + "] " + message, gameObject);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void LogWarning(string context, string message, GameObject gameObject = null)
		{
			UnityEngine.Debug.LogWarning("[" + context + "] " + message, gameObject);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void LogError(string context, string message, GameObject gameObject = null)
		{
			UnityEngine.Debug.LogError("[" + context + "] " + message, gameObject);
		}
	}

	public enum LoadSceneModelResult
	{
		Success = 0,
		NoSceneModelToLoad = 1,
		FailureScenePermissionNotGranted = -1,
		FailureUnexpectedError = -2
	}

	internal struct Metrics
	{
		public int TotalRoomCount;

		public int CandidateRoomCount;

		public int Loaded;

		public int Failed;

		public int SkippedUserNotInRoom;

		public int SkippedAlreadyInstantiated;

		public static Metrics operator +(Metrics lhs, Metrics rhs)
		{
			Metrics result = default(Metrics);
			result.TotalRoomCount = lhs.TotalRoomCount + rhs.TotalRoomCount;
			result.CandidateRoomCount = lhs.CandidateRoomCount + rhs.CandidateRoomCount;
			result.Loaded = lhs.Loaded + rhs.Loaded;
			result.Failed = lhs.Failed + rhs.Failed;
			result.SkippedUserNotInRoom = lhs.SkippedUserNotInRoom + rhs.SkippedUserNotInRoom;
			result.SkippedAlreadyInstantiated = lhs.SkippedAlreadyInstantiated + rhs.SkippedAlreadyInstantiated;
			return result;
		}
	}

	internal struct RoomLayoutUuids
	{
		public Guid Floor;

		public Guid Ceiling;

		public Guid[] Walls;
	}

	internal const string DeprecationMessage = "OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)";

	[FormerlySerializedAs("planePrefab")]
	[Tooltip("A prefab that will be used to instantiate any Plane found when querying the Scene model. If the anchor contains both Volume and Plane elements, Volume will be used instead.")]
	public OVRSceneAnchor PlanePrefab;

	[FormerlySerializedAs("volumePrefab")]
	[Tooltip("A prefab that will be used to instantiate any Volume found when querying the Scene model. This anchor may also contain Plane elements.")]
	public OVRSceneAnchor VolumePrefab;

	[FormerlySerializedAs("prefabOverrides")]
	[Tooltip("Overrides the instantiation of the generic Plane/Volume prefabs with specialized ones.")]
	public List<OVRScenePrefabOverride> PrefabOverrides = new List<OVRScenePrefabOverride>();

	[Tooltip("Scene manager will only present the room(s) the user is currently in.")]
	public bool ActiveRoomsOnly = true;

	[FormerlySerializedAs("verboseLogging")]
	[Tooltip("When enabled, verbose debug logs will be emitted.")]
	public bool VerboseLogging;

	[Tooltip("The maximum number of scene anchors that will be updated each frame.")]
	public int MaxSceneAnchorUpdatesPerFrame = 3;

	[SerializeField]
	[Tooltip("(Optional) The parent transform for each new scene anchor. Changing this value does not affect existing scene anchors. May be null.")]
	internal Transform _initialAnchorParent;

	public Action SceneModelLoadedSuccessfully;

	public Action NoSceneModelToLoad;

	public Action SceneCaptureReturnedWithoutError;

	public Action UnexpectedErrorWithSceneCapture;

	public Action NewSceneModelAvailable;

	[Obsolete("RoomLayout is obsoleted. For each room's layout information (floor, ceiling, walls) see OVRSceneRoom.", false)]
	public RoomLayoutInformation RoomLayout;

	private ulong _sceneCaptureRequestId = ulong.MaxValue;

	private OVRCameraRig _cameraRig;

	private int _sceneAnchorUpdateIndex;

	private bool _hasLoadBeenRequested;

	public Transform InitialAnchorParent
	{
		get
		{
			return _initialAnchorParent;
		}
		set
		{
			_initialAnchorParent = value;
		}
	}

	internal LogForwarder? Verbose
	{
		get
		{
			if (!VerboseLogging)
			{
				return null;
			}
			return default(LogForwarder);
		}
	}

	public event Action LoadSceneModelFailedPermissionNotGranted;

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private static void Log(string message, GameObject gameObject = null)
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private static void LogWarning(string message, GameObject gameObject = null)
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private static void LogError(string message, GameObject gameObject = null)
	{
	}

	private void Awake()
	{
		if (UnityEngine.Object.FindObjectsByType<OVRSceneManager>(FindObjectsSortMode.None).Length > 1)
		{
			default(LogForwarder).LogError("OVRSceneManager", "Found multiple OVRSceneManagers. Destroying '" + base.name + "'.");
			base.enabled = false;
			UnityEngine.Object.DestroyImmediate(this);
		}
	}

	private void Start()
	{
		OVRTelemetry.Start(163061745, 0, -1L).AddAnnotation("basic_prefabs", (PlanePrefab != null || VolumePrefab != null) ? "true" : "false").AddAnnotation("prefab_overrides", (PrefabOverrides.Count > 0) ? "true" : "false")
			.AddAnnotation("active_rooms_only", ActiveRoomsOnly ? "true" : "false")
			.Send();
	}

	private static void LogResult(OVRAnchor.FetchResult value)
	{
		((OVRPlugin.Result)value).IsSuccess();
	}

	internal static async OVRTask<bool> FetchAnchorsAsync<T>(List<OVRAnchor> anchors, Action<List<OVRAnchor>, int> incrementalResultsCallback = null) where T : struct, IOVRAnchorComponent<T>
	{
		OVRResult<List<OVRAnchor>, OVRAnchor.FetchResult> oVRResult = await OVRAnchor.FetchAnchorsAsync(anchors, new OVRAnchor.FetchOptions
		{
			SingleComponentType = typeof(T)
		}, incrementalResultsCallback);
		LogResult(oVRResult.Status);
		return oVRResult.Success;
	}

	internal static async OVRTask<bool> FetchAnchorsAsync(IEnumerable<Guid> uuids, List<OVRAnchor> anchors)
	{
		OVRResult<List<OVRAnchor>, OVRAnchor.FetchResult> oVRResult = await OVRAnchor.FetchAnchorsAsync(anchors, new OVRAnchor.FetchOptions
		{
			Uuids = uuids
		});
		LogResult(oVRResult.Status);
		return oVRResult.Success;
	}

	internal async void OnApplicationPause(bool isPaused)
	{
		if (isPaused || !_hasLoadBeenRequested)
		{
			return;
		}
		List<OVRAnchor> anchors;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out anchors))
		{
			if (!(await FetchAnchorsAsync<OVRRoomLayout>(anchors)))
			{
				Verbose?.Log("OVRSceneManager", "Failed to retrieve scene model information on resume.");
				return;
			}
			foreach (OVRAnchor item in anchors)
			{
				if (!OVRSceneAnchor.SceneAnchors.ContainsKey(item.Uuid))
				{
					Verbose?.Log("OVRSceneManager", "Scene model changed. Invoking NewSceneModelAvailable event.");
					NewSceneModelAvailable?.Invoke();
					break;
				}
			}
		}
		QueryForExistingAnchorsTransform();
	}

	private async void QueryForExistingAnchorsTransform()
	{
		List<OVRAnchor> list;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out list))
		{
			List<Guid> list2;
			using (new OVRObjectPool.ListScope<Guid>(out list2))
			{
				foreach (OVRSceneAnchor sceneAnchors in OVRSceneAnchor.SceneAnchorsList)
				{
					if (sceneAnchors.Space.Valid && sceneAnchors.IsTracked)
					{
						list2.Add(sceneAnchors.Uuid);
					}
				}
				if (list2.Count > 0)
				{
					await FetchAnchorsAsync(list2, list);
				}
				UpdateAllSceneAnchors();
			}
		}
	}

	public bool LoadSceneModel()
	{
		_hasLoadBeenRequested = true;
		DestroyExistingAnchors();
		OVRTask<LoadSceneModelResult> task2 = LoadSceneModelAsync();
		if (!task2.IsCompleted)
		{
			AwaitTask(task2);
			return true;
		}
		return InterpretResult(task2.GetResult());
		async void AwaitTask(OVRTask<LoadSceneModelResult> task)
		{
			InterpretResult(await task);
		}
		bool InterpretResult(LoadSceneModelResult result)
		{
			switch (result)
			{
			case LoadSceneModelResult.Success:
				SceneModelLoadedSuccessfully?.Invoke();
				return true;
			case LoadSceneModelResult.FailureScenePermissionNotGranted:
				this.LoadSceneModelFailedPermissionNotGranted?.Invoke();
				return true;
			case LoadSceneModelResult.NoSceneModelToLoad:
				NoSceneModelToLoad?.Invoke();
				return true;
			default:
				return false;
			}
		}
	}

	private async OVRTask<Metrics> ProcessBatch(List<OVRAnchor> rooms, int startingIndex)
	{
		Metrics metrics = new Metrics
		{
			TotalRoomCount = rooms.Count - startingIndex
		};
		List<OVRAnchor> candidateRooms;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out candidateRooms))
		{
			Dictionary<OVRAnchor, RoomLayoutUuids> layoutUuids;
			using (new OVRObjectPool.DictionaryScope<OVRAnchor, RoomLayoutUuids>(out layoutUuids))
			{
				for (int i = startingIndex; i < rooms.Count; i++)
				{
					OVRAnchor oVRAnchor = rooms[i];
					RoomLayoutUuids value = default(RoomLayoutUuids);
					if (oVRAnchor.GetComponent<OVRRoomLayout>().TryGetRoomLayout(out value.Ceiling, out value.Floor, out value.Walls))
					{
						layoutUuids.Add(oVRAnchor, value);
						candidateRooms.Add(oVRAnchor);
					}
					else
					{
						metrics.Failed++;
					}
				}
				metrics.CandidateRoomCount = candidateRooms.Count;
				if (candidateRooms.Count == 0)
				{
					return metrics;
				}
				if (ActiveRoomsOnly)
				{
					LoadSceneModelResult loadSceneModelResult;
					(loadSceneModelResult, metrics.SkippedUserNotInRoom) = await FilterByActiveRoom(candidateRooms, layoutUuids);
					if (loadSceneModelResult < LoadSceneModelResult.Success)
					{
						metrics.Failed += metrics.CandidateRoomCount;
						return metrics;
					}
					_ = metrics.SkippedUserNotInRoom;
					_ = 0;
					if (candidateRooms.Count == 0)
					{
						return metrics;
					}
				}
				List<bool> taskResults;
				using (new OVRObjectPool.ListScope<bool>(out taskResults))
				{
					List<OVRTask<bool>> list;
					using (new OVRObjectPool.ListScope<OVRTask<bool>>(out list))
					{
						foreach (OVRAnchor item in candidateRooms)
						{
							if (OVRSceneAnchor.SceneAnchors.TryGetValue(item.Uuid, out var value2))
							{
								value2.IsTracked = true;
								metrics.SkippedAlreadyInstantiated++;
								continue;
							}
							RoomLayoutUuids roomLayoutUuids = layoutUuids[item];
							GameObject obj = new GameObject($"Room {item.Uuid}");
							obj.transform.parent = _initialAnchorParent;
							value2 = obj.AddComponent<OVRSceneAnchor>();
							value2.Initialize(item);
							OVRSceneRoom oVRSceneRoom = obj.AddComponent<OVRSceneRoom>();
							list.Add(oVRSceneRoom.LoadRoom(roomLayoutUuids.Floor, roomLayoutUuids.Ceiling, roomLayoutUuids.Walls));
						}
						await OVRTask.WhenAll(list, taskResults);
						foreach (bool item2 in taskResults)
						{
							if (item2)
							{
								metrics.Loaded++;
							}
							else
							{
								metrics.Failed++;
							}
						}
					}
				}
			}
		}
		return metrics;
	}

	private async OVRTask<LoadSceneModelResult> LoadSceneModelAsync()
	{
		if (!Permission.HasUserAuthorizedPermission("com.oculus.permission.USE_SCENE"))
		{
			return LoadSceneModelResult.FailureScenePermissionNotGranted;
		}
		List<Metrics> taskResults;
		using (new OVRObjectPool.ListScope<Metrics>(out taskResults))
		{
			List<OVRTask<Metrics>> tasks;
			using (new OVRObjectPool.ListScope<OVRTask<Metrics>>(out tasks))
			{
				List<OVRAnchor> list;
				using (new OVRObjectPool.ListScope<OVRAnchor>(out list))
				{
					bool result = await FetchAnchorsAsync<OVRRoomLayout>(list, delegate(List<OVRAnchor> rooms, int startingIndex)
					{
						tasks.Add(ProcessBatch(rooms, startingIndex));
					});
					await OVRTask.WhenAll(tasks, taskResults);
					if (!result)
					{
						return LoadSceneModelResult.FailureUnexpectedError;
					}
					Metrics metrics = default(Metrics);
					foreach (Metrics item in taskResults)
					{
						metrics += item;
					}
					if (metrics.Loaded > 0)
					{
						RoomLayout = GetRoomLayoutInformation();
						return LoadSceneModelResult.Success;
					}
					if (metrics.SkippedAlreadyInstantiated > 0)
					{
						return LoadSceneModelResult.Success;
					}
					if (metrics.SkippedUserNotInRoom > 0)
					{
						return LoadSceneModelResult.NoSceneModelToLoad;
					}
					if (metrics.Failed > 0)
					{
						return LoadSceneModelResult.FailureUnexpectedError;
					}
					return LoadSceneModelResult.NoSceneModelToLoad;
				}
			}
		}
	}

	private static async OVRTask<(LoadSceneModelResult, int)> FilterByActiveRoom(List<OVRAnchor> rooms, Dictionary<OVRAnchor, RoomLayoutUuids> layouts)
	{
		rooms.Clear();
		int skipped = 0;
		Vector3 userPosition = OVRPlugin.GetNodePose(OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render).Position.FromVector3f();
		List<OVRAnchor> floorAndCeilingAnchors;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out floorAndCeilingAnchors))
		{
			List<Guid> list;
			using (new OVRObjectPool.ListScope<Guid>(out list))
			{
				foreach (RoomLayoutUuids value3 in layouts.Values)
				{
					list.Add(value3.Ceiling);
					list.Add(value3.Floor);
				}
				if (!(await FetchAnchorsAsync(list, floorAndCeilingAnchors)))
				{
					return (LoadSceneModelResult.FailureUnexpectedError, 0);
				}
				List<bool> list2;
				using (new OVRObjectPool.ListScope<bool>(out list2))
				{
					List<OVRTask<bool>> list3;
					using (new OVRObjectPool.ListScope<OVRTask<bool>>(out list3))
					{
						foreach (OVRAnchor item2 in floorAndCeilingAnchors)
						{
							if (item2.TryGetComponent<OVRLocatable>(out var component))
							{
								list3.Add(component.SetEnabledAsync(enabled: true));
							}
						}
						await OVRTask.WhenAll(list3, list2);
					}
				}
				Dictionary<Guid, OVRAnchor> dictionary;
				using (new OVRObjectPool.DictionaryScope<Guid, OVRAnchor>(out dictionary))
				{
					foreach (OVRAnchor item3 in floorAndCeilingAnchors)
					{
						dictionary.Add(item3.Uuid, item3);
					}
					foreach (var (item, roomLayoutUuids2) in layouts)
					{
						if (dictionary.TryGetValue(roomLayoutUuids2.Floor, out var value) && dictionary.TryGetValue(roomLayoutUuids2.Ceiling, out var value2) && IsUserInRoom(userPosition, value, value2))
						{
							rooms.Add(item);
						}
						else
						{
							skipped++;
						}
					}
				}
			}
		}
		return (LoadSceneModelResult.Success, skipped);
	}

	private static bool IsUserInRoom(Vector3 userPosition, OVRAnchor floor, OVRAnchor ceiling)
	{
		if (!OVRPlugin.TryLocateSpace(floor.Handle, OVRPlugin.GetTrackingOriginType(), out var pose))
		{
			return false;
		}
		if (!OVRPlugin.TryLocateSpace(ceiling.Handle, OVRPlugin.GetTrackingOriginType(), out var pose2))
		{
			return false;
		}
		if (!OVRPlugin.GetSpaceBoundary2DCount(floor.Handle, out var count))
		{
			return false;
		}
		using NativeArray<Vector2> nativeArray = new NativeArray<Vector2>(count, Allocator.Temp);
		if (!OVRPlugin.GetSpaceBoundary2D(floor.Handle, nativeArray))
		{
			return false;
		}
		if (userPosition.y < pose.Position.y)
		{
			return false;
		}
		if (userPosition.y > pose2.Position.y)
		{
			return false;
		}
		Vector3 vector = userPosition - pose.Position.FromVector3f();
		Vector3 vector2 = Quaternion.Inverse(pose.Orientation.FromQuatf()) * vector;
		return PointInPolygon2D(nativeArray, vector2);
	}

	private void DestroyExistingAnchors()
	{
		List<OVRSceneAnchor> list;
		using (new OVRObjectPool.ListScope<OVRSceneAnchor>(out list))
		{
			OVRSceneAnchor.GetSceneAnchors(list);
			foreach (OVRSceneAnchor item in list)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		RoomLayout = null;
	}

	public bool RequestSceneCapture()
	{
		bool num = OVRPlugin.RequestSceneCapture(out _sceneCaptureRequestId);
		if (!num)
		{
			Action unexpectedErrorWithSceneCapture = UnexpectedErrorWithSceneCapture;
			if (unexpectedErrorWithSceneCapture == null)
			{
				return num;
			}
			unexpectedErrorWithSceneCapture();
		}
		return num;
	}

	public OVRTask<bool> DoesRoomSetupExist(IEnumerable<string> requestedAnchorClassifications)
	{
		OVRTask<bool> task = OVRTask.FromGuid<bool>(Guid.NewGuid());
		CheckIfClassificationsAreValid(requestedAnchorClassifications);
		List<OVRAnchor> list;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out list))
		{
			FetchAnchorsAsync<OVRRoomLayout>(list).ContinueWith(delegate(bool result, List<OVRAnchor> anchors)
			{
				CheckClassificationsInRooms(result, anchors, requestedAnchorClassifications, task);
			}, list);
		}
		return task;
	}

	private static void CheckIfClassificationsAreValid(IEnumerable<string> requestedAnchorClassifications)
	{
		if (requestedAnchorClassifications == null)
		{
			throw new ArgumentNullException("requestedAnchorClassifications");
		}
		foreach (string requestedAnchorClassification in requestedAnchorClassifications)
		{
			if (!Classification.Set.Contains(requestedAnchorClassification))
			{
				throw new ArgumentException("requestedAnchorClassifications contains invalid anchor Classification " + requestedAnchorClassification + ".");
			}
		}
	}

	private static void GetUuidsToQuery(OVRAnchor anchor, HashSet<Guid> uuidsToQuery)
	{
		if (anchor.TryGetComponent<OVRAnchorContainer>(out var component))
		{
			Guid[] uuids = component.Uuids;
			foreach (Guid item in uuids)
			{
				uuidsToQuery.Add(item);
			}
		}
	}

	private static void CheckClassificationsInRooms(bool success, List<OVRAnchor> rooms, IEnumerable<string> requestedAnchorClassifications, OVRTask<bool> task)
	{
		if (!success)
		{
			return;
		}
		HashSet<Guid> set;
		using (new OVRObjectPool.HashSetScope<Guid>(out set))
		{
			List<Guid> list;
			using (new OVRObjectPool.ListScope<Guid>(out list))
			{
				for (int i = 0; i < rooms.Count; i++)
				{
					GetUuidsToQuery(rooms[i], set);
					list.AddRange(set);
					set.Clear();
				}
				List<OVRAnchor> roomAnchors;
				using (new OVRObjectPool.ListScope<OVRAnchor>(out roomAnchors))
				{
					FetchAnchorsAsync(list, roomAnchors).ContinueWith(delegate(bool result)
					{
						CheckIfAnchorsContainClassifications(result, roomAnchors, requestedAnchorClassifications, task);
					});
				}
			}
		}
	}

	private static void CheckIfAnchorsContainClassifications(bool success, List<OVRAnchor> roomAnchors, IEnumerable<string> requestedAnchorClassifications, OVRTask<bool> task)
	{
		if (!success)
		{
			return;
		}
		List<string> list;
		using (new OVRObjectPool.ListScope<string>(out list))
		{
			CollectLabelsFromAnchors(roomAnchors, list);
			foreach (string requestedAnchorClassification in requestedAnchorClassifications)
			{
				int num = list.IndexOf(requestedAnchorClassification);
				if (num >= 0)
				{
					list.RemoveAt(num);
					continue;
				}
				task.SetResult(result: false);
				return;
			}
		}
		task.SetResult(result: true);
	}

	private static void CollectLabelsFromAnchors(List<OVRAnchor> anchors, List<string> labels)
	{
		for (int i = 0; i < anchors.Count; i++)
		{
			if (anchors[i].TryGetComponent<OVRSemanticLabels>(out var component))
			{
				labels.AddRange(component.Labels.Split(','));
			}
		}
	}

	private static void OnTrackingSpaceChanged(Transform trackingSpace)
	{
		UpdateAllSceneAnchors();
	}

	private void Update()
	{
		UpdateSomeSceneAnchors();
	}

	private static void UpdateAllSceneAnchors()
	{
		foreach (OVRSceneAnchor value in OVRSceneAnchor.SceneAnchors.Values)
		{
			value.TryUpdateTransform(useCache: true);
			if (value.TryGetComponent<OVRScenePlane>(out var component))
			{
				component.UpdateTransform();
				component.RequestBoundary();
			}
			if (value.TryGetComponent<OVRSceneVolume>(out var component2))
			{
				component2.UpdateTransform();
			}
		}
	}

	private void UpdateSomeSceneAnchors()
	{
		for (int i = 0; i < Math.Min(OVRSceneAnchor.SceneAnchorsList.Count, MaxSceneAnchorUpdatesPerFrame); i++)
		{
			_sceneAnchorUpdateIndex %= OVRSceneAnchor.SceneAnchorsList.Count;
			OVRSceneAnchor.SceneAnchorsList[_sceneAnchorUpdateIndex++].TryUpdateTransform(useCache: false);
		}
	}

	private RoomLayoutInformation GetRoomLayoutInformation()
	{
		RoomLayoutInformation roomLayoutInformation = new RoomLayoutInformation();
		if (OVRSceneRoom.SceneRoomsList.Count > 0)
		{
			roomLayoutInformation.Floor = OVRSceneRoom.SceneRoomsList[0].Floor;
			roomLayoutInformation.Ceiling = OVRSceneRoom.SceneRoomsList[0].Ceiling;
			roomLayoutInformation.Walls.Clear();
			roomLayoutInformation.Walls.AddRange(OVRSceneRoom.SceneRoomsList[0].Walls);
		}
		return roomLayoutInformation;
	}

	private void OnEnable()
	{
		OVRManager.SceneCaptureComplete += OVRManager_SceneCaptureComplete;
		if (OVRManager.display != null)
		{
			OVRManager.display.RecenteredPose += UpdateAllSceneAnchors;
		}
		if (!_cameraRig)
		{
			_cameraRig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
		}
		if ((bool)_cameraRig)
		{
			_cameraRig.TrackingSpaceChanged += OnTrackingSpaceChanged;
		}
	}

	private void OnDisable()
	{
		OVRManager.SceneCaptureComplete -= OVRManager_SceneCaptureComplete;
		if (OVRManager.display != null)
		{
			OVRManager.display.RecenteredPose -= UpdateAllSceneAnchors;
		}
		if ((bool)_cameraRig)
		{
			_cameraRig.TrackingSpaceChanged -= OnTrackingSpaceChanged;
		}
	}

	internal static bool PointInPolygon2D(NativeArray<Vector2> boundaryVertices, Vector2 target)
	{
		if (boundaryVertices.Length < 3)
		{
			return false;
		}
		int num = 0;
		float x = target.x;
		float y = target.y;
		for (int i = 0; i < boundaryVertices.Length; i++)
		{
			float x2 = boundaryVertices[i].x;
			float y2 = boundaryVertices[i].y;
			float x3 = boundaryVertices[(i + 1) % boundaryVertices.Length].x;
			float y3 = boundaryVertices[(i + 1) % boundaryVertices.Length].y;
			if (y < y2 != y < y3 && x < x2 + (y - y2) / (y3 - y2) * (x3 - x2))
			{
				num += ((y2 < y3) ? 1 : (-1));
			}
		}
		return num != 0;
	}

	private void OVRManager_SceneCaptureComplete(ulong requestId, bool result)
	{
		if (requestId != _sceneCaptureRequestId)
		{
			Verbose?.LogWarning("OVRSceneManager", $"Scene Room Setup with requestId: [{requestId}] was ignored, as it was not issued by this Scene Load request.");
		}
		else if (result)
		{
			SceneCaptureReturnedWithoutError?.Invoke();
		}
		else
		{
			UnexpectedErrorWithSceneCapture?.Invoke();
		}
	}

	internal OVRSceneAnchor InstantiateSceneAnchor(OVRAnchor anchor, OVRSceneAnchor prefab)
	{
		OVRSpace oVRSpace = anchor.Handle;
		_ = anchor.Uuid;
		string labels;
		string[] array = (OVRPlugin.GetSpaceSemanticLabels(oVRSpace, out labels) ? labels.Split(',') : Array.Empty<string>());
		if (PrefabOverrides.Count > 0)
		{
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				foreach (OVRScenePrefabOverride prefabOverride in PrefabOverrides)
				{
					if (prefabOverride.ClassificationLabel == text)
					{
						prefab = prefabOverride.Prefab;
						break;
					}
				}
			}
		}
		if (prefab == null)
		{
			Verbose?.Log("OVRSceneManager", $"No prefab was provided for space: [{oVRSpace}]" + ((array.Length != 0) ? (" with semantic label " + array[0]) : ""));
			return null;
		}
		OVRSceneAnchor oVRSceneAnchor = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, _initialAnchorParent);
		oVRSceneAnchor.gameObject.SetActive(value: true);
		oVRSceneAnchor.Initialize(anchor);
		return oVRSceneAnchor;
	}
}
