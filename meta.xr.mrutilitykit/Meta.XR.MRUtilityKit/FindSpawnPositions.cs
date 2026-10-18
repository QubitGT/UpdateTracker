using Meta.XR.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.XR.MRUtilityKit;

[Feature(Feature.Scene)]
public class FindSpawnPositions : MonoBehaviour
{
	public enum SpawnLocation
	{
		Floating,
		AnySurface,
		VerticalSurfaces,
		OnTopOfSurfaces,
		HangingDown
	}

	[Tooltip("When the scene data is loaded, this controls what room(s) the prefabs will spawn in.")]
	public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

	[SerializeField]
	[Tooltip("Prefab to be placed into the scene, or object in the scene to be moved around.")]
	public GameObject SpawnObject;

	[SerializeField]
	[Tooltip("Number of SpawnObject(s) to place into the scene per room, only applies to Prefabs.")]
	public int SpawnAmount = 8;

	[SerializeField]
	[Tooltip("Maximum number of times to attempt spawning/moving an object before giving up.")]
	public int MaxIterations = 1000;

	[FormerlySerializedAs("selectedSnapOption")]
	[SerializeField]
	[Tooltip("Attach content to scene surfaces.")]
	public SpawnLocation SpawnLocations;

	[SerializeField]
	[Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
	public MRUKAnchor.SceneLabels Labels = (MRUKAnchor.SceneLabels)(-1);

	[SerializeField]
	[Tooltip("If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.")]
	public bool CheckOverlaps = true;

	[SerializeField]
	[Tooltip("Required free space for the object (Set negative to auto-detect using GetPrefabBounds)")]
	public float OverrideBounds = -1f;

	[FormerlySerializedAs("layerMask")]
	[SerializeField]
	[Tooltip("Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.")]
	public LayerMask LayerMask = -1;

	[SerializeField]
	[Tooltip("The clearance distance required in front of the surface in order for it to be considered a valid spawn position")]
	public float SurfaceClearanceDistance = 0.1f;

	private void Start()
	{
		OVRTelemetry.Start(651888440, 0, -1L).Send();
		if (!MRUK.Instance || SpawnOnStart == MRUK.RoomFilter.None)
		{
			return;
		}
		MRUK.Instance.RegisterSceneLoadedCallback(delegate
		{
			switch (SpawnOnStart)
			{
			case MRUK.RoomFilter.AllRooms:
				StartSpawn();
				break;
			case MRUK.RoomFilter.CurrentRoomOnly:
				StartSpawn(MRUK.Instance.GetCurrentRoom());
				break;
			}
		});
	}

	public void StartSpawn()
	{
		foreach (MRUKRoom room in MRUK.Instance.Rooms)
		{
			StartSpawn(room);
		}
	}

	public void StartSpawn(MRUKRoom room)
	{
		Bounds? prefabBounds = Utilities.GetPrefabBounds(SpawnObject);
		float num = 0f;
		float num2 = (prefabBounds.HasValue ? (0f - prefabBounds.GetValueOrDefault().min.y) : 0f);
		float num3 = prefabBounds?.center.y ?? 0f;
		Bounds bounds = default(Bounds);
		if (prefabBounds.HasValue)
		{
			num = Mathf.Min(0f - prefabBounds.Value.min.x, 0f - prefabBounds.Value.min.z, prefabBounds.Value.max.x, prefabBounds.Value.max.z);
			if (num < 0f)
			{
				num = 0f;
			}
			Vector3 min = prefabBounds.Value.min;
			Vector3 max = prefabBounds.Value.max;
			min.y += 0.01f;
			if (max.y < min.y)
			{
				max.y = min.y;
			}
			bounds.SetMinMax(min, max);
			if (OverrideBounds > 0f)
			{
				Vector3 center = new Vector3(0f, 0.01f, 0f);
				Vector3 size = new Vector3(OverrideBounds * 2f, 0.02f, OverrideBounds * 2f);
				bounds = new Bounds(center, size);
			}
		}
		for (int i = 0; i < SpawnAmount; i++)
		{
			bool flag = false;
			for (int j = 0; j < MaxIterations; j++)
			{
				Vector3 vector = Vector3.zero;
				Vector3 toDirection = Vector3.zero;
				if (SpawnLocations == SpawnLocation.Floating)
				{
					Vector3? vector2 = room.GenerateRandomPositionInRoom(num, avoidVolumes: true);
					if (!vector2.HasValue)
					{
						break;
					}
					vector = vector2.Value;
				}
				else
				{
					MRUK.SurfaceType surfaceType = (MRUK.SurfaceType)0;
					switch (SpawnLocations)
					{
					case SpawnLocation.AnySurface:
						surfaceType |= MRUK.SurfaceType.FACING_UP;
						surfaceType |= MRUK.SurfaceType.VERTICAL;
						surfaceType |= MRUK.SurfaceType.FACING_DOWN;
						break;
					case SpawnLocation.VerticalSurfaces:
						surfaceType |= MRUK.SurfaceType.VERTICAL;
						break;
					case SpawnLocation.OnTopOfSurfaces:
						surfaceType |= MRUK.SurfaceType.FACING_UP;
						break;
					case SpawnLocation.HangingDown:
						surfaceType |= MRUK.SurfaceType.FACING_DOWN;
						break;
					}
					if (room.GenerateRandomPositionOnSurface(surfaceType, num, new LabelFilter(Labels), out var position, out var normal))
					{
						vector = position + normal * num2;
						toDirection = normal;
						Vector3 vector3 = vector + normal * num3;
						if (!room.IsPositionInRoom(vector3) || room.IsPositionInSceneVolume(vector3) || room.Raycast(new Ray(position, normal), SurfaceClearanceDistance, out var _))
						{
							continue;
						}
					}
				}
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, toDirection);
				if (!CheckOverlaps || !prefabBounds.HasValue || !Physics.CheckBox(vector + quaternion * bounds.center, bounds.extents, quaternion, LayerMask, QueryTriggerInteraction.Ignore))
				{
					flag = true;
					if (SpawnObject.gameObject.scene.path == null)
					{
						Object.Instantiate(SpawnObject, vector, quaternion, base.transform);
						break;
					}
					SpawnObject.transform.position = vector;
					SpawnObject.transform.rotation = quaternion;
					return;
				}
			}
			if (!flag)
			{
				Debug.LogWarning($"Failed to find valid spawn position after {MaxIterations} iterations. Only spawned {i} prefabs instead of {SpawnAmount}.");
				break;
			}
		}
	}
}
