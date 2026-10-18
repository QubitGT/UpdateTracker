using System;
using System.Collections.Generic;
using Meta.XR.Util;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_scene_navigation")]
[Feature(Feature.Scene)]
public class SceneNavigation : MonoBehaviour
{
	[Tooltip("When the scene data is loaded, this controls what room(s) will be used when baking the NavMesh.")]
	public MRUK.RoomFilter BuildOnSceneLoaded = MRUK.RoomFilter.CurrentRoomOnly;

	[Tooltip("If enabled, updates on scene elements such as rooms and anchors will be handled by this class")]
	internal bool TrackUpdates = true;

	[Tooltip("Used for specifying the type of geometry to collect when building a NavMesh")]
	public NavMeshCollectGeometry CollectGeometry = NavMeshCollectGeometry.PhysicsColliders;

	[Tooltip("Used for specifying the type of objects to include when building a NavMesh")]
	public CollectObjects CollectObjects = CollectObjects.Children;

	[Tooltip("The minimum distance to the walls where the navigation mesh can exist.")]
	public float AgentRadius = 0.2f;

	[Tooltip("How much vertical clearance space must exist.")]
	public float AgentHeight = 0.5f;

	[Tooltip("The height of discontinuities in the level the agent can climb over (i.e. steps and stairs).")]
	public float AgentClimb = 0.04f;

	[Tooltip("Maximum slope the agent can walk up.")]
	public float AgentMaxSlope = 5.5f;

	[Tooltip("The agents that will be assigned to the NavMesh generated with the scene data.")]
	public List<NavMeshAgent> Agents;

	[FormerlySerializedAs("SceneObjectsToInclude")]
	[Tooltip("The scene objects that will contribute to the creation of the NavMesh.")]
	public MRUKAnchor.SceneLabels NavigableSurfaces;

	[Tooltip("The scene objects that will carve a hole in the NavMesh.")]
	public MRUKAnchor.SceneLabels SceneObstacles;

	[Tooltip("A bitmask representing the layers to consider when selecting what that will be used for baking.")]
	public LayerMask Layers;

	[Tooltip("The agent's used that is going to be used to build the NavMesh")]
	public int AgentIndex;

	[Tooltip("Determines whether scene data should be used for NavMesh generation.")]
	public bool UseSceneData = true;

	[Tooltip("Determines whether a custom NavMeshAgent configuration should be used. If true, a new agent will be created when building the NavMesh.")]
	public bool CustomAgent = true;

	[Tooltip("Allows overriding the default voxel size used in NavMesh generation. Enable this to specify a custom voxel size.")]
	public bool OverrideVoxelSize;

	[Tooltip("The NavMesh voxel size in world length units. Should be 4-6 voxels per character diameter.")]
	public float VoxelSize;

	[Tooltip("Allows overriding the default tile size used in NavMesh generation. Enable this to specify a custom tile size.")]
	public bool OverrideTileSize;

	[Tooltip("Specifies the tile size for the NavMesh if OverrideTileSize is enabled. Represents the width and height of the square tiles in world units.")]
	public int TileSize = 256;

	[Tooltip("Enables the generation of off-mesh links in the NavMesh, allowing agents to navigate between disconnected mesh regions, such as jumping or climbing.")]
	public bool GenerateLinks;

	private EffectMesh _effectMesh;

	private readonly List<NavMeshBuildSource> _sources = new List<NavMeshBuildSource>();

	private readonly List<Mesh> _connectionMeshes = new List<Mesh>();

	private const float _minimumNavMeshSurfaceArea = 0f;

	private NavMeshSurface _navMeshSurface;

	private const string _obstaclePrefix = "_obstacles";

	private Transform _obstaclesRoot;

	private Transform _surfacesRoot;

	private MRUKAnchor.SceneLabels _cachedNavigableSceneLabels;

	[field: SerializeField]
	[field: Space(10f)]
	public UnityEvent OnNavMeshInitialized { get; private set; } = new UnityEvent();


	public Dictionary<MRUKAnchor, GameObject> Obstacles { get; private set; } = new Dictionary<MRUKAnchor, GameObject>();


	[Obsolete("Navigable surfaces are now handled as NavMeshBuildSource hence this container is not going to be populated.Access the anchors used as navigable surfaces directly.")]
	public Dictionary<MRUKAnchor, GameObject> Surfaces { get; private set; } = new Dictionary<MRUKAnchor, GameObject>();


	private Transform ObstacleRoot
	{
		get
		{
			if (_obstaclesRoot == null)
			{
				_obstaclesRoot = new GameObject("_obstacles").transform;
			}
			return _obstaclesRoot;
		}
	}

	private void Awake()
	{
		_navMeshSurface = base.gameObject.GetComponent<NavMeshSurface>();
		_cachedNavigableSceneLabels = NavigableSurfaces;
	}

	private void Start()
	{
		OVRTelemetry.Start(651889094, 0, -1L).Send();
		if ((object)MRUK.Instance != null)
		{
			MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoadedEvent);
			if (TrackUpdates)
			{
				MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveCreatedRoom);
				MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRemovedRoom);
				MRUK.Instance.RoomUpdatedEvent.AddListener(ReceiveUpdatedRoom);
			}
		}
	}

	private void OnSceneLoadedEvent()
	{
		switch (BuildOnSceneLoaded)
		{
		case MRUK.RoomFilter.CurrentRoomOnly:
			BuildSceneNavMeshForRoom(MRUK.Instance.GetCurrentRoom());
			break;
		case MRUK.RoomFilter.AllRooms:
			BuildSceneNavMesh();
			break;
		case MRUK.RoomFilter.None:
			break;
		}
	}

	private void ReceiveCreatedRoom(MRUKRoom room)
	{
		if (TrackUpdates)
		{
			BuildSceneNavMeshForRoom(room);
		}
	}

	private void ReceiveUpdatedRoom(MRUKRoom room)
	{
		if (TrackUpdates)
		{
			RemoveNavMeshData();
			BuildSceneNavMeshForRoom(room);
		}
	}

	private void ReceiveRemovedRoom(MRUKRoom room)
	{
		if (TrackUpdates)
		{
			RemoveNavMeshData();
		}
	}

	public void ToggleGlobalMeshNavigation(bool useGlobalMesh, int agentTypeID = -1)
	{
		if (useGlobalMesh && MRUK.Instance.GetCurrentRoom().GlobalMeshAnchor == null)
		{
			Debug.LogWarning("[MRUK] No Global Mesh anchor was found in the scene.");
			return;
		}
		if (useGlobalMesh)
		{
			_cachedNavigableSceneLabels = NavigableSurfaces;
			NavigableSurfaces = MRUKAnchor.SceneLabels.GLOBAL_MESH;
		}
		else
		{
			NavigableSurfaces = _cachedNavigableSceneLabels;
		}
		BuildSceneNavMesh();
	}

	public void BuildSceneNavMesh()
	{
		BuildSceneNavMeshForRoom();
	}

	public void BuildSceneNavMeshForRoom(MRUKRoom room = null)
	{
		if (!MRUK.Instance)
		{
			throw new NullReferenceException("MRUK instance is not initialized.");
		}
		List<MRUKRoom> list = ((room != null) ? new List<MRUKRoom> { room } : MRUK.Instance.Rooms);
		if (list.Count == 0)
		{
			throw new InvalidOperationException("No rooms available for NavMesh building.");
		}
		CreateNavMeshSurface();
		RemoveNavMeshData();
		Bounds bounds = ResizeNavMeshFromRoomBounds(ref _navMeshSurface, list);
		_sources.Clear();
		NavMeshBuildSettings navMeshBuildSettings = ((!CustomAgent) ? NavMesh.GetSettingsByIndex(AgentIndex) : CreateNavMeshBuildSettings(AgentRadius, AgentHeight, AgentMaxSlope, AgentClimb));
		_navMeshSurface.agentTypeID = navMeshBuildSettings.agentTypeID;
		ValidateBuildSettings(navMeshBuildSettings, bounds);
		if (UseSceneData)
		{
			CreateObstacles(list);
			CollectSceneSources(list, _sources);
		}
		else
		{
			NavMeshBuilder.CollectSources(bounds, _navMeshSurface.layerMask, _navMeshSurface.useGeometry, 0, GenerateLinks, new List<NavMeshBuildMarkup>(), includeOnlyMarkedObjects: false, _sources);
		}
		NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(navMeshBuildSettings, _sources, bounds, Vector3.zero, Quaternion.identity);
		_navMeshSurface.navMeshData = navMeshData;
		_navMeshSurface.AddData();
		InitializeNavMesh(navMeshBuildSettings.agentTypeID);
	}

	private void CollectSceneSources(List<MRUKRoom> rooms, ICollection<NavMeshBuildSource> sources)
	{
		NavMeshBuildSource item = default(NavMeshBuildSource);
		foreach (MRUKRoom room in rooms)
		{
			foreach (MRUKAnchor anchor in room.Anchors)
			{
				if ((bool)anchor && anchor.HasAnyLabel(NavigableSurfaces))
				{
					item.transform = anchor.transform.localToWorldMatrix;
					item.sourceObject = Utilities.SetupAnchorMeshGeometry(anchor, useFunctionalSurfaces: true);
					item.shape = NavMeshBuildSourceShape.Mesh;
					sources.Add(item);
				}
			}
		}
	}

	public NavMeshBuildSettings CreateNavMeshBuildSettings(float agentRadius, float agentHeight, float agentMaxSlope, float agentClimb)
	{
		NavMeshBuildSettings result = NavMesh.CreateSettings();
		result.agentRadius = agentRadius;
		result.agentHeight = agentHeight;
		result.agentSlope = agentMaxSlope;
		result.agentClimb = agentClimb;
		result.overrideVoxelSize = OverrideVoxelSize;
		if (OverrideVoxelSize)
		{
			result.voxelSize = VoxelSize;
		}
		result.overrideTileSize = OverrideTileSize;
		if (OverrideTileSize)
		{
			result.tileSize = TileSize;
		}
		return result;
	}

	public void CreateNavMeshSurface()
	{
		_navMeshSurface = GetComponent<NavMeshSurface>();
		if (!_navMeshSurface)
		{
			_navMeshSurface = base.gameObject.AddComponent<NavMeshSurface>();
		}
		_navMeshSurface.minRegionArea = 0.01f;
		_navMeshSurface.voxelSize = VoxelSize;
		if (!UseSceneData)
		{
			_navMeshSurface.collectObjects = CollectObjects;
			_navMeshSurface.useGeometry = CollectGeometry;
			_navMeshSurface.hideFlags = HideFlags.None;
		}
		else
		{
			_navMeshSurface.collectObjects = CollectObjects.Children;
			_navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
			_navMeshSurface.hideFlags = HideFlags.NotEditable;
		}
		_navMeshSurface.layerMask = Layers;
	}

	public void RemoveNavMeshData()
	{
		if ((bool)_navMeshSurface)
		{
			_navMeshSurface.navMeshData = null;
			_navMeshSurface.RemoveData();
			if (Obstacles != null)
			{
				ClearObstacles();
			}
		}
	}

	public Bounds ResizeNavMeshFromRoomBounds(ref NavMeshSurface surface, MRUKRoom room = null)
	{
		List<MRUKRoom> rooms = ((room != null) ? new List<MRUKRoom> { room } : new List<MRUKRoom> { MRUK.Instance.GetCurrentRoom() });
		return ResizeNavMeshFromRoomBounds(ref surface, rooms);
	}

	public Bounds ResizeNavMeshFromRoomBounds(ref NavMeshSurface surface, List<MRUKRoom> rooms)
	{
		if (rooms.Count == 0)
		{
			throw new InvalidOperationException("No rooms available to resize the NavMeshSurface.");
		}
		Bounds roomBounds = rooms[0].GetRoomBounds();
		for (int i = 1; i < rooms.Count; i++)
		{
			roomBounds.Encapsulate(rooms[i].GetRoomBounds());
		}
		Vector3 center = new Vector3(roomBounds.center.x, roomBounds.center.y, roomBounds.center.z);
		surface.center = center;
		Vector3 vector = new Vector3(roomBounds.size.x, roomBounds.size.y, roomBounds.size.z);
		surface.size = vector * 1.1f;
		return new Bounds(surface.center, surface.size);
	}

	private void InitializeNavMesh(int agentTypeID)
	{
		if (_navMeshSurface.navMeshData.sourceBounds.extents.x * _navMeshSurface.navMeshData.sourceBounds.extents.z > 0f)
		{
			if (Agents != null)
			{
				foreach (NavMeshAgent agent in Agents)
				{
					agent.agentTypeID = agentTypeID;
				}
			}
			OnNavMeshInitialized?.Invoke();
		}
		else
		{
			Debug.LogWarning("Failed to generate a nav mesh, this may be because the room is too small or the AgentType settings are to strict");
		}
	}

	public void CreateObstacles(MRUKRoom room = null)
	{
		List<MRUKRoom> rooms = ((room == null) ? new List<MRUKRoom> { room } : new List<MRUKRoom> { MRUK.Instance.GetCurrentRoom() });
		CreateObstacles(rooms);
	}

	public void CreateObstacles(List<MRUKRoom> rooms)
	{
		ObstacleRoot.transform.SetParent(base.transform);
		foreach (MRUKRoom room in rooms)
		{
			foreach (MRUKAnchor anchor in room.Anchors)
			{
				CreateObstacle(anchor);
			}
		}
	}

	public void CreateObstacle(MRUKAnchor anchor, bool shouldCarve = true, bool carveOnlyStationary = false, float carvingTimeToStationary = 0.2f, float carvingMoveThreshold = 0.2f)
	{
		if (!anchor || !anchor.HasAnyLabel(SceneObstacles))
		{
			return;
		}
		if (Obstacles.ContainsKey(anchor))
		{
			Debug.LogWarning("Anchor already associated with an obstacle from this SceneNavigation");
			return;
		}
		Vector3 obstacleSize;
		Vector3 obstacleCenter;
		if (anchor.VolumeBounds.HasValue)
		{
			obstacleSize = anchor.VolumeBounds.Value.size;
			obstacleCenter = anchor.VolumeBounds.Value.center;
		}
		else
		{
			if (!anchor.PlaneRect.HasValue)
			{
				return;
			}
			obstacleSize = anchor.PlaneRect.Value.size;
			obstacleCenter = anchor.PlaneRect.Value.center;
		}
		InstantiateObstacle(anchor, shouldCarve, carveOnlyStationary, carvingTimeToStationary, carvingMoveThreshold, obstacleSize, obstacleCenter);
	}

	private void InstantiateObstacle(MRUKAnchor anchor, bool shouldCarve, bool carveOnlyStationary, float carvingTimeToStationary, float carvingMoveThreshold, Vector3 obstacleSize, Vector3 obstacleCenter)
	{
		GameObject gameObject = new GameObject("_obstacles_" + anchor.name);
		gameObject.transform.SetParent(_obstaclesRoot.transform);
		NavMeshObstacle navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
		navMeshObstacle.carving = shouldCarve;
		navMeshObstacle.carveOnlyStationary = carveOnlyStationary;
		navMeshObstacle.carvingTimeToStationary = carvingTimeToStationary;
		navMeshObstacle.carvingMoveThreshold = carvingMoveThreshold;
		navMeshObstacle.shape = NavMeshObstacleShape.Box;
		navMeshObstacle.transform.position = anchor.transform.position;
		navMeshObstacle.transform.rotation = anchor.transform.rotation;
		navMeshObstacle.size = obstacleSize;
		navMeshObstacle.center = obstacleCenter;
		Obstacles.Add(anchor, gameObject);
	}

	private List<Mesh> CreateRoomBridges(List<(MRUKAnchor, MRUKAnchor)> connections)
	{
		_connectionMeshes.Clear();
		Vector3[] array = new Vector3[4];
		int[] triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		for (int i = 0; i < connections.Count; i++)
		{
			MRUKAnchor item = connections[i].Item1;
			MRUKAnchor item2 = connections[i].Item2;
			if (item.PlaneRect.HasValue && item2.PlaneRect.HasValue)
			{
				List<Vector2> planeBoundary2D = item.PlaneBoundary2D;
				List<Vector2> planeBoundary2D2 = item2.PlaneBoundary2D;
				Mesh mesh = new Mesh();
				array[0] = item.transform.TransformPoint(new Vector3(planeBoundary2D[0].x, planeBoundary2D[0].y, 0f));
				array[1] = item2.transform.TransformPoint(new Vector3(planeBoundary2D2[1].x, planeBoundary2D2[1].y, 0f));
				array[2] = item.transform.TransformPoint(new Vector3(planeBoundary2D[1].x, planeBoundary2D[1].y, 0f));
				array[3] = item2.transform.TransformPoint(new Vector3(planeBoundary2D2[0].x, planeBoundary2D2[0].y, 0f));
				mesh.vertices = array;
				mesh.triangles = triangles;
				mesh.RecalculateNormals();
				_connectionMeshes.Add(mesh);
			}
		}
		return _connectionMeshes;
	}

	[Obsolete("Navigable surfaces are now handled as NavMeshBuildSource, and are automatically created when buildingthe NavMesh using the scene data. Use EffectMesh to spawn colliders in the place of anchors.", true)]
	public void CreateNavigableSurfaces(MRUKRoom room = null)
	{
		List<MRUKRoom> list = new List<MRUKRoom>();
		if ((bool)room)
		{
			list.Add(room);
		}
		else
		{
			list = MRUK.Instance.Rooms;
		}
		if (_surfacesRoot == null)
		{
			_surfacesRoot = new GameObject("_surface").transform;
		}
		_surfacesRoot.transform.SetParent(base.transform);
		foreach (MRUKRoom item in list)
		{
			foreach (MRUKAnchor anchor in item.Anchors)
			{
				CreateNavigableSurface(anchor);
			}
		}
		_navMeshSurface.collectObjects = CollectObjects.Children;
	}

	private void CreateNavigableSurface(MRUKAnchor anchor)
	{
		if (!anchor || !anchor.HasAnyLabel(NavigableSurfaces))
		{
			return;
		}
		GameObject gameObject = new GameObject("_surface_" + anchor.name);
		gameObject.transform.SetParent(_surfacesRoot.transform);
		gameObject.gameObject.layer = GetFirstLayerFromLayerMask(Layers);
		if (!anchor || !anchor.HasAnyLabel(NavigableSurfaces))
		{
			return;
		}
		if (Surfaces.ContainsKey(anchor))
		{
			Debug.LogWarning("Anchor already associated with an obstacle from this SceneNavigation");
			return;
		}
		Vector3 size;
		Vector3 center;
		if (anchor.VolumeBounds.HasValue)
		{
			size = anchor.VolumeBounds.Value.size;
			center = anchor.VolumeBounds.Value.center;
		}
		else
		{
			if (!anchor.PlaneRect.HasValue)
			{
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = anchor.Mesh;
				meshCollider.transform.position = anchor.transform.position;
				meshCollider.transform.rotation = anchor.transform.rotation;
				Surfaces.Add(anchor, gameObject);
				return;
			}
			size = anchor.PlaneRect.Value.size;
			center = anchor.PlaneRect.Value.center;
		}
		BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.transform.position = anchor.transform.position;
		boxCollider.transform.rotation = anchor.transform.rotation;
		boxCollider.size = size;
		boxCollider.center = center;
		Surfaces.Add(anchor, gameObject);
	}

	public void ClearObstacles(MRUKRoom room = null)
	{
		List<MRUKAnchor> list = new List<MRUKAnchor>();
		foreach (KeyValuePair<MRUKAnchor, GameObject> obstacle in Obstacles)
		{
			if (!(room != null) || !(obstacle.Key.Room != room))
			{
				UnityEngine.Object.DestroyImmediate(obstacle.Value);
				list.Add(obstacle.Key);
			}
		}
		foreach (MRUKAnchor item in list)
		{
			Obstacles.Remove(item);
		}
	}

	public void ClearObstacle(MRUKAnchor anchor)
	{
		if (Obstacles.TryGetValue(anchor, out var value))
		{
			UnityEngine.Object.DestroyImmediate(value);
			Obstacles.Remove(anchor);
		}
	}

	private void ClearSurfaces(MRUKRoom room = null)
	{
		List<MRUKAnchor> list = new List<MRUKAnchor>();
		foreach (KeyValuePair<MRUKAnchor, GameObject> surface in Surfaces)
		{
			if (!(room != null) || !(surface.Key.Room != room))
			{
				UnityEngine.Object.DestroyImmediate(surface.Value);
				list.Add(surface.Key);
			}
		}
		foreach (MRUKAnchor item in list)
		{
			Surfaces.Remove(item);
		}
	}

	[Obsolete("Navigable surfaces are now handled as NavMeshBuildSource hence their destruction is handled internally.")]
	public void ClearSurface(MRUKAnchor anchor)
	{
		if (Surfaces.ContainsKey(anchor))
		{
			UnityEngine.Object.DestroyImmediate(Surfaces[anchor]);
			Surfaces.Remove(anchor);
		}
	}

	public static int GetFirstLayerFromLayerMask(LayerMask layerMask)
	{
		int result = 0;
		for (int i = 0; i < 32; i++)
		{
			if (((1 << i) & (int)layerMask) != 0)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public static bool ValidateBuildSettings(NavMeshBuildSettings navMeshBuildSettings, Bounds navMeshBounds)
	{
		string[] array = navMeshBuildSettings.ValidationReport(navMeshBounds);
		if (array.Length == 0)
		{
			return true;
		}
		string text = "Some NavMeshBuildSettings constraints were violated:\n";
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			text = text + "- " + text2 + "\n";
		}
		Debug.LogWarning(text);
		return false;
	}

	private void OnDestroy()
	{
		OnNavMeshInitialized.RemoveAllListeners();
		if ((bool)MRUK.Instance)
		{
			MRUK.Instance.RoomCreatedEvent.RemoveListener(ReceiveCreatedRoom);
			MRUK.Instance.RoomRemovedEvent.RemoveListener(ReceiveRemovedRoom);
			MRUK.Instance.RoomUpdatedEvent.RemoveListener(ReceiveUpdatedRoom);
			MRUK.Instance.SceneLoadedEvent.RemoveListener(OnSceneLoadedEvent);
		}
	}
}
