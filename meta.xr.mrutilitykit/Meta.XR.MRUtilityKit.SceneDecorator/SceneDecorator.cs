using System;
using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class SceneDecorator : MonoBehaviour
{
	public interface IDistribution
	{
		void Distribute(SceneDecorator sceneDecorator, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration);
	}

	public static readonly float PI = 3.14159f;

	[SerializeField]
	public List<SceneDecoration> sceneDecorations;

	[SerializeField]
	public Collider[] customColliders;

	[SerializeField]
	public string[] customTargetTags;

	[SerializeField]
	public int recursionLimit = 3;

	private int _recursionDepth;

	private SceneDecorator _parent;

	[Tooltip("When the scene data is loaded, this controls what room(s) the decorator will add decorations.")]
	public MRUK.RoomFilter DecorateOnStart = MRUK.RoomFilter.AllRooms;

	[Tooltip("If enabled, updates on scene elements such as rooms and anchors will be handled by this class")]
	internal bool TrackUpdates = true;

	private PoolManagerComponent _poolManagerComponent;

	private PoolManagerSingleton _poolManagerSingleton;

	private Dictionary<GameObject, MRUKAnchor> _spawnedDecorations = new Dictionary<GameObject, MRUKAnchor>();

	private void Start()
	{
		_poolManagerSingleton = base.gameObject.AddComponent<PoolManagerSingleton>();
		_poolManagerComponent = base.gameObject.AddComponent<PoolManagerComponent>();
		InitPools();
		OVRTelemetry.Start(651888752, 0, -1L).Send();
		if ((object)MRUK.Instance == null)
		{
			return;
		}
		MRUK.Instance.SceneLoadedEvent.AddListener(delegate
		{
			if (DecorateOnStart != 0)
			{
				switch (DecorateOnStart)
				{
				case MRUK.RoomFilter.CurrentRoomOnly:
					DecorateScene(MRUK.Instance.GetCurrentRoom(), _recursionDepth);
					break;
				case MRUK.RoomFilter.AllRooms:
					DecorateScene(MRUK.Instance.Rooms, _recursionDepth);
					break;
				}
			}
		});
		if (MRUK.Instance.IsInitialized)
		{
			switch (DecorateOnStart)
			{
			case MRUK.RoomFilter.CurrentRoomOnly:
				DecorateScene(MRUK.Instance.GetCurrentRoom(), _recursionDepth);
				break;
			case MRUK.RoomFilter.AllRooms:
				DecorateScene(MRUK.Instance.Rooms, _recursionDepth);
				break;
			}
		}
		if (TrackUpdates)
		{
			MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveRoomCreated);
			MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRoomRemoved);
		}
	}

	private void OnDestroy()
	{
		if (!(MRUK.Instance == null))
		{
			MRUK.Instance.RoomCreatedEvent.RemoveListener(ReceiveRoomCreated);
			MRUK.Instance.RoomRemovedEvent.RemoveListener(ReceiveRoomRemoved);
		}
	}

	private void InitPools()
	{
		List<PoolManagerComponent.PoolDesc> list = new List<PoolManagerComponent.PoolDesc>();
		foreach (SceneDecoration sceneDecoration in sceneDecorations)
		{
			GameObject[] decorationPrefabs = sceneDecoration.decorationPrefabs;
			foreach (GameObject primitive in decorationPrefabs)
			{
				list.Add(new PoolManagerComponent.PoolDesc
				{
					poolType = PoolManagerComponent.PoolDesc.PoolType.FIXED,
					size = sceneDecoration.Poolsize,
					primitive = primitive,
					callbackProviderOverride = null
				});
			}
		}
		_poolManagerComponent.defaultPools = list.ToArray();
		_poolManagerComponent.InitDefaultPools();
	}

	private void OnEnable()
	{
		if ((bool)MRUK.Instance)
		{
			MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveRoomCreated);
			MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRoomRemoved);
		}
	}

	private void OnDisable()
	{
		if ((bool)MRUK.Instance)
		{
			MRUK.Instance.RoomCreatedEvent.RemoveListener(ReceiveRoomCreated);
			MRUK.Instance.RoomRemovedEvent.RemoveListener(ReceiveRoomRemoved);
		}
	}

	private void ReceiveRoomRemoved(MRUKRoom room)
	{
		ClearDecorations(room);
		UnRegisterAnchorUpdates(room);
	}

	private void ReceiveRoomCreated(MRUKRoom room)
	{
		if (TrackUpdates && DecorateOnStart == MRUK.RoomFilter.AllRooms)
		{
			DecorateScene(room);
			RegisterAnchorUpdates(room);
		}
	}

	private void UnRegisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.RemoveListener(ReceiveAnchorCreated);
		room.AnchorRemovedEvent.RemoveListener(ReceiveAnchorRemoved);
		room.AnchorUpdatedEvent.RemoveListener(ReceiveAnchorUpdated);
	}

	private void ReceiveAnchorUpdated(MRUKAnchor anchor)
	{
		if (TrackUpdates)
		{
			ClearDecorations(anchor);
			Decorate(anchor);
		}
	}

	private void ReceiveAnchorRemoved(MRUKAnchor anchor)
	{
		ClearDecorations(anchor);
	}

	private void ReceiveAnchorCreated(MRUKAnchor anchor)
	{
		Decorate(anchor);
	}

	private void RegisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.AddListener(ReceiveAnchorCreated);
		room.AnchorRemovedEvent.AddListener(ReceiveAnchorRemoved);
		room.AnchorUpdatedEvent.AddListener(ReceiveAnchorUpdated);
	}

	private void ClearDecorations(MRUKAnchor anchor)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<GameObject, MRUKAnchor> spawnedDecoration in _spawnedDecorations)
		{
			if (!(spawnedDecoration.Value != anchor))
			{
				list.Add(spawnedDecoration.Key);
			}
		}
		foreach (GameObject item in list)
		{
			_poolManagerSingleton.Release(item);
		}
	}

	private void ClearDecorations(MRUKRoom room)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<GameObject, MRUKAnchor> spawnedDecoration in _spawnedDecorations)
		{
			if (!(spawnedDecoration.Value.Room != room))
			{
				list.Add(spawnedDecoration.Key);
			}
		}
		foreach (GameObject item in list)
		{
			_poolManagerSingleton.Release(item);
		}
	}

	public void ClearDecorations()
	{
		foreach (KeyValuePair<GameObject, MRUKAnchor> spawnedDecoration in _spawnedDecorations)
		{
			_poolManagerSingleton.Release(spawnedDecoration.Key);
		}
	}

	private void DecorateScene(MRUKRoom room)
	{
		DecorateScene(room, 0);
	}

	public void DecorateScene()
	{
		foreach (MRUKRoom room in MRUK.Instance.Rooms)
		{
			DecorateScene(room, 0);
		}
	}

	private void DecorateScene(List<MRUKRoom> rooms, int recursionDepth)
	{
		foreach (MRUKRoom room in rooms)
		{
			DecorateScene(room, recursionDepth);
		}
	}

	private void DecorateScene(MRUKRoom room, int recursionDepth)
	{
		if (recursionDepth >= recursionLimit)
		{
			return;
		}
		foreach (SceneDecoration sceneDecoration in sceneDecorations)
		{
			Decorate(room, sceneDecoration);
		}
	}

	private void Decorate(MRUKAnchor anchor)
	{
		foreach (SceneDecoration sceneDecoration in sceneDecorations)
		{
			Decorate(anchor, sceneDecoration);
		}
	}

	private void Decorate(MRUKAnchor anchor, SceneDecoration sceneDecoration)
	{
		MRUKAnchor.SceneLabels executeSceneLabels = sceneDecoration.executeSceneLabels;
		if (anchor.Label == executeSceneLabels)
		{
			Distribute(anchor, sceneDecoration);
		}
	}

	private void Decorate(MRUKRoom room, SceneDecoration sceneDecoration)
	{
		MRUKAnchor.SceneLabels executeSceneLabels = sceneDecoration.executeSceneLabels;
		foreach (MRUKAnchor.SceneLabels value in Enum.GetValues(typeof(MRUKAnchor.SceneLabels)))
		{
			if ((executeSceneLabels & value) == 0)
			{
				continue;
			}
			List<MRUKAnchor> anchorsWithLabel = GetAnchorsWithLabel(room, value);
			if (anchorsWithLabel == null)
			{
				continue;
			}
			foreach (MRUKAnchor item in anchorsWithLabel)
			{
				Distribute(item, sceneDecoration);
			}
		}
	}

	private void Distribute(MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration)
	{
		if (sceneDecoration.decorationPrefabs.Length == 0)
		{
			Debug.LogWarning("No decoration prefab added to " + sceneDecoration.name);
			return;
		}
		if (_poolManagerComponent.defaultPools.Length == 0)
		{
			InitPools();
		}
		switch (sceneDecoration.distributionType)
		{
		case DistributionType.GRID:
			sceneDecoration.gridDistribution.Distribute(this, sceneAnchor, sceneDecoration);
			break;
		case DistributionType.SIMPLEX:
			sceneDecoration.simplexDistribution.Distribute(this, sceneAnchor, sceneDecoration);
			break;
		case DistributionType.STAGGERED_CONCENTRIC:
			sceneDecoration.staggeredConcentricDistribution.Distribute(this, sceneAnchor, sceneDecoration);
			break;
		default:
			sceneDecoration.randomDistribution.Distribute(this, sceneAnchor, sceneDecoration);
			break;
		}
	}

	private static void TestCollider(Collider c, Vector3 worldPos, Vector3 rayDir, SceneDecoration sceneDecoration, ref RaycastHit closestHit)
	{
		rayDir = (sceneDecoration.selectBehind ? (-rayDir) : rayDir);
		if (c.Raycast(new Ray(worldPos, rayDir), out var hitInfo, float.PositiveInfinity) && hitInfo.distance < Mathf.Abs(closestHit.distance))
		{
			closestHit = hitInfo;
			closestHit.distance = (sceneDecoration.selectBehind ? (0f - closestHit.distance) : closestHit.distance);
			if (sceneDecoration.DrawDebugRaysAndImpactPoints)
			{
				Debug.DrawLine(worldPos, closestHit.point, Color.magenta, 3600f);
				Utilities.DrawWireSphere(worldPos, 0.05f, Color.cyan, 3600f);
				Utilities.DrawWireSphere(closestHit.point, 0.05f, Color.blue, 3600f);
			}
		}
	}

	private static void TestPhysicsLayers(Vector3 worldPos, Vector3 rayDir, SceneDecoration sceneDecoration, ref RaycastHit closestHit)
	{
		rayDir = (sceneDecoration.selectBehind ? (-rayDir) : rayDir);
		if (sceneDecoration.DrawDebugRaysAndImpactPoints)
		{
			Utilities.DrawWireSphere(worldPos, 0.05f, Color.cyan, 3600f);
		}
		if (Physics.Raycast(new Ray(worldPos, rayDir), out var hitInfo, float.PositiveInfinity, sceneDecoration.targetPhysicsLayers) && hitInfo.distance < Mathf.Abs(closestHit.distance))
		{
			closestHit = hitInfo;
			closestHit.distance = (sceneDecoration.selectBehind ? (0f - closestHit.distance) : closestHit.distance);
			if (sceneDecoration.DrawDebugRaysAndImpactPoints)
			{
				Debug.DrawLine(worldPos, closestHit.point, Color.red, 3600f);
				Utilities.DrawWireSphere(closestHit.point, 0.05f, Color.blue, 3600f);
			}
		}
	}

	private bool TestConstraints(SceneDecoration sceneDecoration, Candidate c)
	{
		Constraint[] constraints = sceneDecoration.constraints;
		for (int i = 0; i < constraints.Length; i++)
		{
			Constraint constraint = constraints[i];
			if (!constraint.enabled)
			{
				continue;
			}
			ConstraintModeCheck modeCheck = constraint.modeCheck;
			float num = constraint.mask.SampleMask(c);
			bool flag = constraint.mask.Check(c);
			switch (modeCheck)
			{
			case ConstraintModeCheck.Value:
				if ((num < constraint.min) | (num > constraint.max))
				{
					return false;
				}
				break;
			case ConstraintModeCheck.Bool:
				if (!flag)
				{
					return false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		return true;
	}

	private void ApplyModifiers(GameObject decorationGO, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration, Candidate candidate)
	{
		Modifier[] modifiers = sceneDecoration.modifiers;
		foreach (Modifier modifier in modifiers)
		{
			if (modifier.enabled)
			{
				modifier.ApplyModifier(decorationGO, sceneAnchor, sceneDecoration, candidate);
			}
		}
	}

	public void GenerateOn(Vector2 localPos, Vector2 localPosNormalized, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration)
	{
		Vector3 zero = Vector3.zero;
		if (sceneDecoration.placement == Placement.SPHERICAL)
		{
			localPos *= new Vector2(2f * PI, PI);
			float num = Mathf.Sin(localPos.x);
			zero = new Vector3(num * Mathf.Cos(localPos.y), num * Mathf.Sin(localPos.y), Mathf.Cos(localPos.x));
		}
		else
		{
			zero = new Vector3(localPos.x, localPos.y, 0f) + sceneDecoration.rayOffset;
		}
		GenerateAt(sceneAnchor.transform.TransformPoint(zero), localPos, localPosNormalized, sceneAnchor, sceneDecoration);
	}

	private void GenerateAt(Vector3 worldPos, Vector2 localPos, Vector2 localPosNormalized, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration)
	{
		Vector3 vector = sceneDecoration.placementDirection;
		switch (sceneDecoration.placement)
		{
		case Placement.LOCAL_PLANAR:
			vector = sceneAnchor.transform.rotation * vector;
			break;
		case Placement.SPHERICAL:
			vector = (worldPos - sceneAnchor.transform.position).normalized;
			break;
		}
		RaycastHit raycastHit = default(RaycastHit);
		raycastHit.distance = float.PositiveInfinity;
		RaycastHit hitInfo = raycastHit;
		Target targets = sceneDecoration.targets;
		foreach (Target value in Enum.GetValues(typeof(Target)))
		{
			if ((targets & value) == 0)
			{
				continue;
			}
			switch (value)
			{
			case Target.PHYSICS_LAYERS:
				TestPhysicsLayers(worldPos, vector, sceneDecoration, ref hitInfo);
				continue;
			case Target.CUSTOM_COLLIDERS:
			{
				Collider[] array2 = customColliders;
				for (int i = 0; i < array2.Length; i++)
				{
					TestCollider(array2[i], worldPos, vector, sceneDecoration, ref hitInfo);
				}
				continue;
			}
			case Target.CUSTOM_TAGS:
			{
				string[] array = customTargetTags;
				foreach (string text in array)
				{
					if (sceneAnchor.gameObject.CompareTag(text))
					{
						MeshCollider componentInChildren = sceneAnchor.gameObject.GetComponentInChildren<MeshCollider>();
						if (!(componentInChildren == null))
						{
							TestCollider(componentInChildren, worldPos, vector, sceneDecoration, ref hitInfo);
						}
					}
				}
				continue;
			}
			case Target.SCENE_ANCHORS:
			{
				Ray ray = new Ray(worldPos, vector);
				sceneAnchor.Raycast(ray, float.PositiveInfinity, out hitInfo);
				continue;
			}
			}
			if (sceneAnchor.Room.GlobalMeshAnchor != null)
			{
				MeshCollider componentInChildren2 = sceneAnchor.Room.GlobalMeshAnchor.gameObject.GetComponentInChildren<MeshCollider>();
				if (!(componentInChildren2 == null))
				{
					TestCollider(componentInChildren2, worldPos, vector, sceneDecoration, ref hitInfo);
				}
			}
		}
		if (float.IsPositiveInfinity(hitInfo.distance))
		{
			return;
		}
		Vector3 closestPosition;
		float closestSurfacePosition = sceneAnchor.GetClosestSurfacePosition(hitInfo.point, out closestPosition);
		GameObject gameObject = sceneDecoration.decorationPrefabs[UnityEngine.Random.Range(0, sceneDecoration.decorationPrefabs.Length)];
		Candidate candidate = default(Candidate);
		candidate.decorationPrefab = gameObject;
		candidate.localPos = localPos;
		candidate.localPosNormalized = localPosNormalized;
		candidate.hit = hitInfo;
		candidate.anchorDist = closestSurfacePosition;
		candidate.anchorCompDists = closestPosition;
		candidate.slope = 57.29578f * Mathf.Acos(Vector3.Dot(hitInfo.normal, -vector));
		Candidate candidate2 = candidate;
		if (!TestConstraints(sceneDecoration, candidate2))
		{
			return;
		}
		Transform transform = sceneDecoration.spawnHierarchy switch
		{
			SpawnHierarchy.SCENE_DECORATOR_CHILD => base.gameObject.transform, 
			SpawnHierarchy.ANCHOR_CHILD => sceneAnchor.transform, 
			SpawnHierarchy.TARGET_CHILD => hitInfo.transform, 
			SpawnHierarchy.TARGET_COLLIDER_CHILD => (hitInfo.collider == null) ? null : hitInfo.collider.transform, 
			_ => null, 
		};
		gameObject = _poolManagerSingleton.Create(gameObject, hitInfo.point, Quaternion.identity, sceneAnchor, transform);
		if (!(gameObject == null))
		{
			_spawnedDecorations[gameObject] = sceneAnchor;
			if (sceneDecoration.lifetime > 0f)
			{
				UnityEngine.Object.Destroy(gameObject, sceneDecoration.lifetime);
			}
			if (gameObject.TryGetComponent<SceneDecorator>(out var component))
			{
				component._parent = this;
				component._recursionDepth = _recursionDepth + 1;
			}
			if ((transform != null) & sceneDecoration.discardParentScaling)
			{
				Vector3 localScale = gameObject.transform.localScale;
				Vector3 lossyScale = transform.lossyScale;
				localScale.x *= 1f / lossyScale.x;
				localScale.y *= 1f / lossyScale.y;
				localScale.z *= 1f / lossyScale.z;
				gameObject.transform.localScale = localScale;
			}
			ApplyModifiers(gameObject, sceneAnchor, sceneDecoration, candidate2);
		}
	}

	private List<MRUKAnchor> GetAnchorsWithLabel(MRUKRoom room, MRUKAnchor.SceneLabels label)
	{
		List<MRUKAnchor> list = new List<MRUKAnchor>();
		foreach (MRUKAnchor anchor in room.Anchors)
		{
			if (anchor.Label == label)
			{
				list.Add(anchor);
			}
		}
		return list;
	}
}
