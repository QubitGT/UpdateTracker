using System;
using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_anchor_prefab_spawner")]
[Feature(Feature.Scene)]
public class AnchorPrefabSpawner : MonoBehaviour, ICustomAnchorPrefabSpawner
{
	public enum ScalingMode
	{
		Stretch,
		UniformScaling,
		UniformXZScale,
		NoScaling,
		Custom
	}

	public enum AlignMode
	{
		Automatic,
		Bottom,
		Center,
		NoAlignment,
		Custom
	}

	public enum SelectionMode
	{
		Random,
		ClosestSize,
		Custom
	}

	[Serializable]
	public struct AnchorPrefabGroup : IEquatable<AnchorPrefabGroup>
	{
		[FormerlySerializedAs("_include")]
		[SerializeField]
		[Tooltip("Anchors to include.")]
		public MRUKAnchor.SceneLabels Labels;

		[SerializeField]
		[Tooltip("Prefab(s) to spawn (randomly chosen from list.)")]
		public List<GameObject> Prefabs;

		[SerializeField]
		[Tooltip("The logic that determines what prefab to chose when spawning the relative labels' game objects")]
		public SelectionMode PrefabSelection;

		[SerializeField]
		[Tooltip("When enabled, the prefab will be rotated to try and match the aspect ratio of the volume as closely as possible. This is most useful for long and thin volumes, keep this disabled for objects with an aspect ratio close to 1:1. Only applies to volumes.")]
		public bool MatchAspectRatio;

		[SerializeField]
		[Tooltip("When calculate facing direction is enabled the prefab will be rotated to face away from the closest wall. If match aspect ratio is also enabled then that will take precedence and it will be constrained to a choice between 2 directions only.Only applies to volumes.")]
		public bool CalculateFacingDirection;

		[SerializeField]
		[Tooltip("Set what scaling mode to apply to the prefab. By default the prefab will be stretched to fit the size of the plane/volume. But in some cases this may not be desirable and can be customized here.")]
		public ScalingMode Scaling;

		[SerializeField]
		[Tooltip("Spawn new object at the center, top or bottom of the anchor.")]
		public AlignMode Alignment;

		[SerializeField]
		[Tooltip("Don't analyze prefab, just assume a default scale of 1.")]
		public bool IgnorePrefabSize;

		public bool Equals(AnchorPrefabGroup other)
		{
			if (Labels == other.Labels && object.Equals(Prefabs, other.Prefabs) && PrefabSelection == other.PrefabSelection && MatchAspectRatio == other.MatchAspectRatio && CalculateFacingDirection == other.CalculateFacingDirection && Scaling == other.Scaling && Alignment == other.Alignment)
			{
				return IgnorePrefabSize == other.IgnorePrefabSize;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is AnchorPrefabGroup other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)Labels, Prefabs, (int)PrefabSelection, MatchAspectRatio, CalculateFacingDirection, (int)Scaling, (int)Alignment, IgnorePrefabSize);
		}

		public static bool operator ==(AnchorPrefabGroup left, AnchorPrefabGroup right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AnchorPrefabGroup left, AnchorPrefabGroup right)
		{
			return !left.Equals(right);
		}
	}

	[Tooltip("When the scene data is loaded, this controls what room(s) the prefabs will spawn in.")]
	public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

	[Tooltip("If enabled, updates on scene elements such as rooms and anchors will be handled by this class")]
	internal bool TrackUpdates = true;

	[Tooltip("Specify a seed value for consistent prefab selection (0 = Random).")]
	public int SeedValue;

	[NonSerialized]
	[Obsolete("Event onPrefabSpawned will be deprecated in a future version")]
	public UnityEvent onPrefabSpawned = new UnityEvent();

	public List<AnchorPrefabGroup> PrefabsToSpawn;

	protected System.Random _random;

	private MRUK.SceneTrackingSettings SceneTrackingSettings;

	private static readonly string Suffix = "(PrefabSpawner Clone)";

	private Func<Vector3, Vector3> _customPrefabScalingVolume;

	private Func<Bounds, Bounds?, (Vector3, Vector3)> _customPrefabAlignmentVolume;

	private Func<Vector2, Vector2> _customPrefabScalingPlaneRect;

	private Func<Rect, Bounds?, (Vector3, Vector2)> _customPrefabAlignmentPlaneRect;

	private Func<MRUKAnchor, List<GameObject>, GameObject> _customPrefabSelection;

	public Dictionary<MRUKAnchor, GameObject> AnchorPrefabSpawnerObjects { get; } = new Dictionary<MRUKAnchor, GameObject>();


	[Obsolete("Use AnchorPrefabSpawnerObjects property instead. This property is inefficient because it will generate a new list each time it is accessed")]
	public List<GameObject> SpawnedPrefabs => new List<GameObject>(AnchorPrefabSpawnerObjects.Values);

	protected virtual void Start()
	{
		OVRTelemetry.Start(651902681, 0, -1L).Send();
		if ((object)MRUK.Instance == null)
		{
			return;
		}
		SceneTrackingSettings.UnTrackedRooms = new HashSet<MRUKRoom>();
		SceneTrackingSettings.UnTrackedAnchors = new HashSet<MRUKAnchor>();
		MRUK.Instance.RegisterSceneLoadedCallback(delegate
		{
			if (SpawnOnStart != 0)
			{
				switch (SpawnOnStart)
				{
				case MRUK.RoomFilter.CurrentRoomOnly:
					SpawnPrefabs(MRUK.Instance.GetCurrentRoom());
					break;
				case MRUK.RoomFilter.AllRooms:
					SpawnPrefabs();
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case MRUK.RoomFilter.None:
					break;
				}
			}
		});
		_ = TrackUpdates;
	}

	protected virtual void OnEnable()
	{
		if ((bool)MRUK.Instance)
		{
			MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveCreatedRoom);
			MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRemovedRoom);
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)MRUK.Instance)
		{
			MRUK.Instance.RoomCreatedEvent.RemoveListener(ReceiveCreatedRoom);
			MRUK.Instance.RoomRemovedEvent.RemoveListener(ReceiveRemovedRoom);
		}
	}

	protected virtual void ReceiveRemovedRoom(MRUKRoom room)
	{
		ClearPrefabs(room);
		UnRegisterAnchorUpdates(room);
	}

	protected virtual void UnRegisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.RemoveListener(ReceiveAnchorCreatedEvent);
		room.AnchorRemovedEvent.RemoveListener(ReceiveAnchorRemovedCallback);
		room.AnchorUpdatedEvent.RemoveListener(ReceiveAnchorUpdatedCallback);
	}

	protected virtual void RegisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.AddListener(ReceiveAnchorCreatedEvent);
		room.AnchorRemovedEvent.AddListener(ReceiveAnchorRemovedCallback);
		room.AnchorUpdatedEvent.AddListener(ReceiveAnchorUpdatedCallback);
	}

	protected virtual void ReceiveAnchorUpdatedCallback(MRUKAnchor anchorInfo)
	{
		if (!SceneTrackingSettings.UnTrackedRooms.Contains(anchorInfo.Room) && !SceneTrackingSettings.UnTrackedAnchors.Contains(anchorInfo) && TrackUpdates)
		{
			ClearPrefabs();
			SpawnPrefabs(anchorInfo);
		}
	}

	protected virtual void ReceiveAnchorRemovedCallback(MRUKAnchor anchorInfo)
	{
		ClearPrefabs();
	}

	protected virtual void ReceiveAnchorCreatedEvent(MRUKAnchor anchorInfo)
	{
		if (!SceneTrackingSettings.UnTrackedRooms.Contains(anchorInfo.Room) && TrackUpdates)
		{
			SpawnPrefabs();
		}
	}

	protected virtual void ReceiveCreatedRoom(MRUKRoom room)
	{
		if (TrackUpdates && SpawnOnStart == MRUK.RoomFilter.AllRooms)
		{
			SpawnPrefabs(room);
			RegisterAnchorUpdates(room);
		}
	}

	protected virtual void ClearPrefabs(MRUKRoom room)
	{
		List<MRUKAnchor> list = new List<MRUKAnchor>();
		foreach (KeyValuePair<MRUKAnchor, GameObject> anchorPrefabSpawnerObject in AnchorPrefabSpawnerObjects)
		{
			if (!(anchorPrefabSpawnerObject.Key.Room != room))
			{
				ClearPrefab(anchorPrefabSpawnerObject.Value);
				list.Add(anchorPrefabSpawnerObject.Key);
			}
		}
		foreach (MRUKAnchor item in list)
		{
			AnchorPrefabSpawnerObjects.Remove(item);
		}
		SceneTrackingSettings.UnTrackedRooms.Add(room);
	}

	protected virtual void ClearPrefab(GameObject go)
	{
		UnityEngine.Object.Destroy(go);
	}

	protected virtual void ClearPrefab(MRUKAnchor anchorInfo)
	{
		if (AnchorPrefabSpawnerObjects.ContainsKey(anchorInfo))
		{
			ClearPrefab(AnchorPrefabSpawnerObjects[anchorInfo]);
			AnchorPrefabSpawnerObjects.Remove(anchorInfo);
			SceneTrackingSettings.UnTrackedAnchors.Add(anchorInfo);
		}
	}

	protected virtual void ClearPrefabs()
	{
		foreach (KeyValuePair<MRUKAnchor, GameObject> anchorPrefabSpawnerObject in AnchorPrefabSpawnerObjects)
		{
			ClearPrefab(anchorPrefabSpawnerObject.Value);
		}
		AnchorPrefabSpawnerObjects.Clear();
	}

	protected virtual void SpawnPrefabs(bool clearPrefabs = true)
	{
		if (clearPrefabs)
		{
			ClearPrefabs();
		}
		foreach (MRUKRoom room in MRUK.Instance.Rooms)
		{
			SpawnPrefabsInternal(room);
		}
		onPrefabSpawned?.Invoke();
	}

	protected virtual void SpawnPrefabs(MRUKRoom room, bool clearPrefabs = true)
	{
		if (clearPrefabs)
		{
			ClearPrefabs();
		}
		SpawnPrefabsInternal(room);
		onPrefabSpawned?.Invoke();
	}

	private void SpawnPrefabsInternal(MRUKRoom room)
	{
		InitializeRandom(ref SeedValue);
		foreach (MRUKAnchor anchor in room.Anchors)
		{
			SpawnPrefab(anchor);
		}
	}

	protected virtual void SpawnPrefab(MRUKAnchor anchorInfo)
	{
		AnchorPrefabGroup prefabGroup;
		GameObject gameObject = LabelToPrefab(anchorInfo.Label, anchorInfo, out prefabGroup);
		if (gameObject == null)
		{
			return;
		}
		if (AnchorPrefabSpawnerObjects.ContainsKey(anchorInfo))
		{
			Debug.LogWarning("Anchor already associated with a gameobject spawned from this AnchorPrefabSpawner");
			return;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, anchorInfo.transform);
		gameObject2.name = gameObject.name + Suffix;
		gameObject2.name = gameObject.name + Suffix;
		gameObject2.transform.parent = anchorInfo.transform;
		Bounds? bounds = (prefabGroup.IgnorePrefabSize ? null : Utilities.GetPrefabBounds(gameObject));
		if (!bounds.HasValue)
		{
			bounds = gameObject2.GetComponentInChildren<GridSliceResizer>(includeInactive: true)?.OriginalMesh.bounds;
		}
		Vector3 prefabSize = bounds?.size ?? Vector3.one;
		if (anchorInfo.VolumeBounds.HasValue)
		{
			int cardinalAxisIndex = 0;
			if (prefabGroup.CalculateFacingDirection && !prefabGroup.MatchAspectRatio)
			{
				anchorInfo.Room.GetDirectionAwayFromClosestWall(anchorInfo, out cardinalAxisIndex);
			}
			Bounds volumeBounds = AnchorPrefabSpawnerUtilities.RotateVolumeBounds(anchorInfo.VolumeBounds.Value, cardinalAxisIndex);
			Vector3 size = volumeBounds.size;
			Vector3 localScale = new Vector3(size.x / prefabSize.x, size.z / prefabSize.y, size.y / prefabSize.z);
			if (prefabGroup.MatchAspectRatio)
			{
				AnchorPrefabSpawnerUtilities.MatchAspectRatio(anchorInfo, prefabGroup.CalculateFacingDirection, prefabSize, size, ref cardinalAxisIndex, ref volumeBounds, ref localScale);
			}
			localScale = ((prefabGroup.Scaling == ScalingMode.Custom) ? CustomPrefabScaling(localScale) : AnchorPrefabSpawnerUtilities.ScalePrefab(localScale, prefabGroup.Scaling));
			Vector3 vector = ((prefabGroup.Alignment == AlignMode.Custom) ? CustomPrefabAlignment(volumeBounds, bounds) : AnchorPrefabSpawnerUtilities.AlignPrefabPivot(volumeBounds, bounds, localScale, prefabGroup.Alignment));
			gameObject2.transform.localPosition = Quaternion.AngleAxis(cardinalAxisIndex * 90, Vector3.forward) * vector;
			gameObject2.transform.localRotation = Quaternion.Euler((cardinalAxisIndex - 1) * 90, -90f, -90f);
			gameObject2.transform.localScale = localScale;
		}
		else if (anchorInfo.PlaneRect.HasValue)
		{
			Vector2 size2 = anchorInfo.PlaneRect.Value.size;
			Vector2 localScale2 = new Vector2(size2.x / prefabSize.x, size2.y / prefabSize.y);
			gameObject2.transform.localScale = ((prefabGroup.Scaling == ScalingMode.Custom) ? ((Vector3)CustomPrefabScaling(localScale2)) : AnchorPrefabSpawnerUtilities.ScalePrefab(localScale2, prefabGroup.Scaling));
			gameObject2.transform.localPosition = ((prefabGroup.Alignment == AlignMode.Custom) ? CustomPrefabAlignment(anchorInfo.PlaneRect.Value, bounds) : AnchorPrefabSpawnerUtilities.AlignPrefabPivot(anchorInfo.PlaneRect.Value, bounds, localScale2, prefabGroup.Alignment));
		}
		AnchorPrefabSpawnerObjects.Add(anchorInfo, gameObject2);
	}

	private GameObject LabelToPrefab(MRUKAnchor.SceneLabels labels, MRUKAnchor anchor, out AnchorPrefabGroup prefabGroup)
	{
		foreach (AnchorPrefabGroup item in PrefabsToSpawn)
		{
			if ((item.Labels & labels) != 0 && ((item.Prefabs != null && item.Prefabs.Count != 0) || item.PrefabSelection == SelectionMode.Custom))
			{
				GameObject gameObject = null;
				gameObject = ((item.PrefabSelection != SelectionMode.Custom) ? AnchorPrefabSpawnerUtilities.SelectPrefab(anchor, item.PrefabSelection, item.Prefabs, _random) : CustomPrefabSelection(anchor, item.Prefabs));
				prefabGroup = item;
				return gameObject;
			}
		}
		prefabGroup = default(AnchorPrefabGroup);
		return null;
	}

	public void InitializeRandom(ref int seed)
	{
		if (seed == 0)
		{
			seed = Environment.TickCount;
		}
		_random = new System.Random(seed);
	}

	public virtual GameObject CustomPrefabSelection(MRUKAnchor anchor, List<GameObject> prefabs)
	{
		throw new Exception("A custom prefab selection method was selected but no implementation was provided. Extend this class and override the `CustomPrefabSelection` method with your custom logic.");
	}

	public virtual Vector3 CustomPrefabScaling(Vector3 localScale)
	{
		throw new NotImplementedException("A custom scaling method for an anchor's volume is selected but no implementation was provided. Extend this class and override the `CustomPrefabVolumeScaling` method with your custom logic.");
	}

	public virtual Vector2 CustomPrefabScaling(Vector2 localScale)
	{
		throw new NotImplementedException("A custom scaling method was selected but no implementation was provided. Extend this class and override the `CustomPrefabPlaneRectScaling` method with your custom logic.");
	}

	public virtual Vector3 CustomPrefabAlignment(Bounds anchorVolumeBounds, Bounds? prefabBounds)
	{
		throw new NotImplementedException("A custom volume alignment method was selected but no implementation was provided.Extend this class and override the `CustomPrefabAlignment` method with your custom logic.");
	}

	public virtual Vector3 CustomPrefabAlignment(Rect anchorPlaneRect, Bounds? prefabBounds)
	{
		throw new NotImplementedException("A custom prefab selection method was selected but no implementation was provided. Extend this class and override the `CustomPrefabAlignment` method with your custom logic.");
	}

	private void OnDestroy()
	{
		onPrefabSpawned.RemoveAllListeners();
	}
}
