using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_destructible_global_mesh_spawner")]
public class DestructibleGlobalMeshSpawner : MonoBehaviour
{
	[SerializeField]
	public MRUK.RoomFilter CreateOnRoomLoaded = MRUK.RoomFilter.CurrentRoomOnly;

	public UnityEvent<DestructibleMeshComponent> OnDestructibleMeshCreated;

	public Func<DestructibleMeshComponent.MeshSegmentationResult, DestructibleMeshComponent.MeshSegmentationResult> OnSegmentationCompleted;

	[SerializeField]
	private bool _reserveSpace;

	[SerializeField]
	private Vector3 _reservedMin;

	[SerializeField]
	private Vector3 _reservedMax;

	[SerializeField]
	private Material _globalMeshMaterial;

	[SerializeField]
	private float _pointsPerUnitX = 1f;

	[SerializeField]
	private float _pointsPerUnitY = 1f;

	[SerializeField]
	private int _maxPointsCount = 256;

	[SerializeField]
	private float _reservedTop;

	[SerializeField]
	private float _reservedBottom;

	private readonly Dictionary<MRUKRoom, DestructibleGlobalMesh> _spawnedDestructibleMeshes = new Dictionary<MRUKRoom, DestructibleGlobalMesh>();

	private const string _destructibleGlobalMeshObjectName = "DestructibleGlobalMesh";

	private static List<Vector3> _points = new List<Vector3>();

	public bool ReserveSpace
	{
		get
		{
			return _reserveSpace;
		}
		set
		{
			_reserveSpace = value;
		}
	}

	public float PointsPerUnitX
	{
		get
		{
			return _pointsPerUnitX;
		}
		set
		{
			_pointsPerUnitX = value;
		}
	}

	public float PointsPerUnitY
	{
		get
		{
			return _pointsPerUnitY;
		}
		set
		{
			_pointsPerUnitY = value;
		}
	}

	public int MaxPointsCount
	{
		get
		{
			return _maxPointsCount;
		}
		set
		{
			_maxPointsCount = value;
		}
	}

	public Material GlobalMeshMaterial
	{
		get
		{
			return _globalMeshMaterial;
		}
		set
		{
			_globalMeshMaterial = value;
		}
	}

	public float ReservedTop
	{
		get
		{
			return _reservedTop;
		}
		set
		{
			_reservedTop = value;
		}
	}

	public float ReservedBottom
	{
		get
		{
			return _reservedBottom;
		}
		set
		{
			_reservedBottom = value;
		}
	}

	private void Start()
	{
		OVRTelemetry.Start(651898938, 0, -1L).Send();
		MRUK.Instance.RegisterSceneLoadedCallback(delegate
		{
			if (CreateOnRoomLoaded != 0)
			{
				switch (CreateOnRoomLoaded)
				{
				case MRUK.RoomFilter.CurrentRoomOnly:
				{
					MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
					if (!_spawnedDestructibleMeshes.ContainsKey(currentRoom))
					{
						AddDestructibleGlobalMesh(MRUK.Instance.GetCurrentRoom());
					}
					break;
				}
				case MRUK.RoomFilter.AllRooms:
					AddDestructibleGlobalMesh();
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case MRUK.RoomFilter.None:
					break;
				}
			}
		});
		MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveCreatedRoom);
		MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRemovedRoom);
	}

	private void AddDestructibleGlobalMesh()
	{
		foreach (MRUKRoom room in MRUK.Instance.Rooms)
		{
			if (!room.GlobalMeshAnchor)
			{
				Debug.LogWarning("Can not find a global mesh anchor, skipping the destructible mesh creation for this room");
			}
			else if (!_spawnedDestructibleMeshes.ContainsKey(room))
			{
				AddDestructibleGlobalMesh(room);
			}
		}
	}

	public DestructibleGlobalMesh AddDestructibleGlobalMesh(MRUKRoom room)
	{
		if (_spawnedDestructibleMeshes.ContainsKey(room))
		{
			throw new Exception("Cannot add a destructible mesh to this room as it already contains one.");
		}
		if (!room.GlobalMeshAnchor)
		{
			throw new Exception("A destructible mesh can not be created for this room as it does not contain a global mesh anchor.");
		}
		GameObject obj = new GameObject("DestructibleGlobalMesh");
		obj.transform.SetParent(room.GlobalMeshAnchor.transform, worldPositionStays: false);
		DestructibleMeshComponent destructibleMeshComponent = obj.AddComponent<DestructibleMeshComponent>();
		destructibleMeshComponent.GlobalMeshMaterial = _globalMeshMaterial;
		if (!_reserveSpace)
		{
			float reservedBottom = (ReservedTop = -1f);
			ReservedBottom = reservedBottom;
		}
		destructibleMeshComponent.ReservedBottom = ReservedBottom;
		destructibleMeshComponent.ReservedTop = ReservedTop;
		destructibleMeshComponent.OnDestructibleMeshCreated = OnDestructibleMeshCreated;
		destructibleMeshComponent.OnSegmentationCompleted = OnSegmentationCompleted;
		DestructibleGlobalMesh destructibleGlobalMesh = default(DestructibleGlobalMesh);
		destructibleGlobalMesh.MaxPointsCount = _maxPointsCount;
		destructibleGlobalMesh.PointsPerUnitX = _pointsPerUnitX;
		destructibleGlobalMesh.PointsPerUnitY = _pointsPerUnitY;
		destructibleGlobalMesh.DestructibleMeshComponent = destructibleMeshComponent;
		DestructibleGlobalMesh destructibleGlobalMesh2 = destructibleGlobalMesh;
		CreateDestructibleGlobalMesh(destructibleGlobalMesh2, room);
		_spawnedDestructibleMeshes.Add(room, destructibleGlobalMesh2);
		return destructibleGlobalMesh2;
	}

	private static void CreateDestructibleGlobalMesh(DestructibleGlobalMesh destructibleGlobalMesh, MRUKRoom room)
	{
		if (!room)
		{
			throw new Exception("Could not find a room for the destructible mesh");
		}
		if (!room.GlobalMeshAnchor || !room.GlobalMeshAnchor.Mesh)
		{
			throw new Exception("Could not load the mesh associated with the global mesh anchor of the room");
		}
		Vector3[] vertices = room.GlobalMeshAnchor.Mesh.vertices;
		int[] triangles = room.GlobalMeshAnchor.Mesh.triangles;
		Vector3[] array = ComputeRoomBoxGrid(room, destructibleGlobalMesh.MaxPointsCount, destructibleGlobalMesh.PointsPerUnitX, destructibleGlobalMesh.PointsPerUnitY);
		uint[] meshIndices = Array.ConvertAll(triangles, Convert.ToUInt32);
		Vector3[] array2 = new Vector3[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = room.transform.InverseTransformPoint(array[i]);
		}
		destructibleGlobalMesh.DestructibleMeshComponent.SegmentMesh(vertices, meshIndices, array2);
	}

	public bool TryGetDestructibleMeshForRoom(MRUKRoom room, out DestructibleGlobalMesh destructibleGlobalMesh)
	{
		destructibleGlobalMesh = _spawnedDestructibleMeshes.GetValueOrDefault(room);
		return destructibleGlobalMesh != default(DestructibleGlobalMesh);
	}

	public void RemoveDestructibleGlobalMesh(MRUKRoom room = null)
	{
		if (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
		{
			throw new Exception("Can not remove a destructible global mesh when MRUK instance has not been initialized.");
		}
		if (room == null)
		{
			room = MRUK.Instance.GetCurrentRoom();
		}
		if (TryGetDestructibleMeshForRoom(room, out var destructibleGlobalMesh))
		{
			UnityEngine.Object.Destroy(destructibleGlobalMesh.DestructibleMeshComponent.gameObject);
			_spawnedDestructibleMeshes.Remove(room);
		}
	}

	private void ReceiveCreatedRoom(MRUKRoom room)
	{
		if ((CreateOnRoomLoaded != MRUK.RoomFilter.CurrentRoomOnly || _spawnedDestructibleMeshes.Count <= 0) && CreateOnRoomLoaded == MRUK.RoomFilter.AllRooms)
		{
			AddDestructibleGlobalMesh();
		}
	}

	private void ReceiveRemovedRoom(MRUKRoom room)
	{
		if (room == null)
		{
			throw new Exception("Received a Room Removed event but the room is null.");
		}
		RemoveDestructibleGlobalMesh(room);
	}

	private static Vector3[] ComputeRoomBoxGrid(MRUKRoom room, int maxPointsCount, float pointsPerUnitX = 1f, float pointPerUnitY = 1f)
	{
		_points.Clear();
		foreach (MRUKAnchor wallAnchor in room.WallAnchors)
		{
			GeneratePoints(_points, wallAnchor.transform.position, wallAnchor.transform.rotation, wallAnchor.PlaneRect, pointsPerUnitX, pointPerUnitY);
		}
		float num = room.CeilingAnchor.transform.position.y - room.FloorAnchor.transform.position.y;
		float num2 = Mathf.Max(Mathf.Ceil(pointPerUnitY * num), 1f);
		float num3 = num / num2;
		for (int i = 0; (float)i < num2; i++)
		{
			GeneratePoints(position: new Vector3(room.CeilingAnchor.transform.position.x, room.CeilingAnchor.transform.position.y - num3 * (float)i, room.CeilingAnchor.transform.position.z), points: _points, rotation: room.CeilingAnchor.transform.rotation, planeBounds: room.CeilingAnchor.PlaneRect, pointsPerUnitX: pointsPerUnitX, pointsPerUnitY: pointPerUnitY);
		}
		GeneratePoints(_points, room.CeilingAnchor.transform.position, room.CeilingAnchor.transform.rotation, room.CeilingAnchor.PlaneRect, pointsPerUnitX, pointPerUnitY);
		GeneratePoints(_points, room.FloorAnchor.transform.position, room.FloorAnchor.transform.rotation, room.FloorAnchor.PlaneRect, pointsPerUnitX, pointPerUnitY);
		if (_points.Count > maxPointsCount)
		{
			Shuffle(_points);
			_points.RemoveRange(maxPointsCount, _points.Count - maxPointsCount);
		}
		return _points.ToArray();
	}

	private static void GeneratePoints(List<Vector3> points, Vector3 position, Quaternion rotation, Rect? planeBounds, float pointsPerUnitX, float pointsPerUnitY)
	{
		if (!planeBounds.HasValue)
		{
			throw new Exception("Failed to generate points as the given plane has no bounds.");
		}
		Vector3 vector = new Vector3(planeBounds.Value.size.x, planeBounds.Value.size.y, 0f);
		Vector3 vector2 = position - rotation * new Vector3(vector.x * 0.5f, vector.y * 0.5f);
		float num = Mathf.Max(Mathf.Ceil(pointsPerUnitX * vector.x), 1f);
		float num2 = Mathf.Max(Mathf.Ceil(pointsPerUnitY * vector.y), 1f);
		Vector2 vector3 = new Vector2(vector.x / (num + 1f), vector.y / (num2 + 1f));
		for (int i = 0; (float)i < num2; i++)
		{
			for (int j = 0; (float)j < num; j++)
			{
				float x = (float)(j + 1) * vector3.x;
				float y = (float)(i + 1) * vector3.y;
				Vector3 item = vector2 + rotation * new Vector3(x, y);
				points.Add(item);
			}
		}
	}

	private static void Shuffle<T>(List<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int num2 = UnityEngine.Random.Range(0, num + 1);
			int index = num2;
			int index2 = num;
			T val = list[num];
			T val2 = list[num2];
			T val4 = (list[index] = val);
			val4 = (list[index2] = val2);
		}
	}
}
