using System;
using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Meta.XR.MRUtilityKit;

[Feature(Feature.Scene)]
[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_effect_mesh")]
public class EffectMesh : MonoBehaviour
{
	public enum WallTextureCoordinateModeU
	{
		METRIC,
		METRIC_SEAMLESS,
		MAINTAIN_ASPECT_RATIO,
		MAINTAIN_ASPECT_RATIO_SEAMLESS,
		STRETCH,
		STRETCH_SECTION
	}

	public enum WallTextureCoordinateModeV
	{
		METRIC,
		MAINTAIN_ASPECT_RATIO,
		STRETCH
	}

	public enum AnchorTextureCoordinateMode
	{
		METRIC,
		STRETCH
	}

	[Serializable]
	public class TextureCoordinateModes
	{
		[FormerlySerializedAs("U")]
		public WallTextureCoordinateModeU WallU;

		[FormerlySerializedAs("V")]
		public WallTextureCoordinateModeV WallV;

		public AnchorTextureCoordinateMode AnchorUV;
	}

	public class EffectMeshObject
	{
		public GameObject effectMeshGO;

		public Mesh mesh;

		public Collider collider;
	}

	[Tooltip("When the scene data is loaded, this controls what room(s) the effect mesh is applied to.")]
	public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

	[Tooltip("If enabled, updates on scene elements such as rooms and anchors will be handled by this class")]
	internal bool TrackUpdates = true;

	[Tooltip("The material applied to the generated mesh. If you'd like a multi-material room, you can use another EffectMesh object with a different Mesh Material.")]
	[FormerlySerializedAs("_MeshMaterial")]
	public Material MeshMaterial;

	[NonSerialized]
	[Obsolete("BorderSize functionality has been removed.")]
	[FormerlySerializedAs("_borderSize")]
	public float BorderSize;

	[Tooltip("Generate a BoxCollider for each mesh component.")]
	[FormerlySerializedAs("addColliders")]
	public bool Colliders;

	[Tooltip("Cut holes in the mesh for door frames and/or window frames. NOTE: This does not apply if border size is non-zero.")]
	public MRUKAnchor.SceneLabels CutHoles;

	[Tooltip("Whether the effect mesh objects will cast a shadow.")]
	[SerializeField]
	private bool castShadows = true;

	[Tooltip("Hide the effect mesh.")]
	[SerializeField]
	private bool hideMesh;

	private MRUK.SceneTrackingSettings SceneTrackingSettings;

	[HideInInspector]
	public int Layer;

	[Tooltip("Can not exceed 8.")]
	public TextureCoordinateModes[] textureCoordinateModes = new TextureCoordinateModes[1]
	{
		new TextureCoordinateModes()
	};

	[Tooltip("Specifies the scene labels that determine which anchors representations are created by the effect mesh.")]
	[FormerlySerializedAs("_include")]
	public MRUKAnchor.SceneLabels Labels;

	private static readonly string Suffix = "_EffectMesh";

	private Dictionary<MRUKAnchor, EffectMeshObject> effectMeshObjects = new Dictionary<MRUKAnchor, EffectMeshObject>();

	public bool CastShadow
	{
		get
		{
			return castShadows;
		}
		set
		{
			ToggleShadowCasting(value);
			castShadows = value;
		}
	}

	public bool HideMesh
	{
		get
		{
			return hideMesh;
		}
		set
		{
			ToggleEffectMeshVisibility(!value);
			hideMesh = value;
		}
	}

	[Obsolete("This property is deprecated. Please use 'ToggleEffectMeshColliders' instead.")]
	public bool ToggleColliders
	{
		get
		{
			return Colliders;
		}
		set
		{
			ToggleEffectMeshColliders(!value);
			Colliders = value;
		}
	}

	public IReadOnlyDictionary<MRUKAnchor, EffectMeshObject> EffectMeshObjects => effectMeshObjects;

	private void Start()
	{
		OVRTelemetry.Start(651897605, 0, -1L).Send();
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
					CreateMesh(MRUK.Instance.GetCurrentRoom());
					break;
				case MRUK.RoomFilter.AllRooms:
					CreateMesh();
					break;
				}
			}
		});
		if (TrackUpdates)
		{
			MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveCreatedRoom);
			MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRemovedRoom);
		}
	}

	private void ReceiveRemovedRoom(MRUKRoom room)
	{
		DestroyMesh(room);
		UnregisterAnchorUpdates(room);
	}

	private void UnregisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.RemoveListener(ReceiveAnchorCreatedEvent);
		room.AnchorRemovedEvent.RemoveListener(ReceiveAnchorRemovedCallback);
		room.AnchorUpdatedEvent.RemoveListener(ReceiveAnchorUpdatedCallback);
	}

	private void RegisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.AddListener(ReceiveAnchorCreatedEvent);
		room.AnchorRemovedEvent.AddListener(ReceiveAnchorRemovedCallback);
		room.AnchorUpdatedEvent.AddListener(ReceiveAnchorUpdatedCallback);
	}

	private void ReceiveAnchorUpdatedCallback(MRUKAnchor anchor)
	{
		if (!SceneTrackingSettings.UnTrackedRooms.Contains(anchor.Room) && !SceneTrackingSettings.UnTrackedAnchors.Contains(anchor) && TrackUpdates && anchor.HasAnyLabel(Labels))
		{
			DestroyMesh(anchor);
			CreateEffectMesh(anchor);
		}
	}

	private void ReceiveAnchorRemovedCallback(MRUKAnchor anchor)
	{
		DestroyMesh(anchor);
	}

	private void ReceiveAnchorCreatedEvent(MRUKAnchor anchor)
	{
		if (!SceneTrackingSettings.UnTrackedRooms.Contains(anchor.Room) && TrackUpdates && anchor.HasAnyLabel(Labels))
		{
			CreateEffectMesh(anchor);
		}
	}

	private void ReceiveCreatedRoom(MRUKRoom room)
	{
		if (TrackUpdates && SpawnOnStart == MRUK.RoomFilter.AllRooms)
		{
			CreateMesh(room);
		}
	}

	public void CreateMesh()
	{
		foreach (MRUKRoom room in MRUK.Instance.Rooms)
		{
			CreateMesh(room);
		}
	}

	public void DestroyMesh(LabelFilter label = default(LabelFilter))
	{
		List<MRUKAnchor> list = new List<MRUKAnchor>();
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			bool flag = label.PassesFilter(effectMeshObject.Key.Label);
			if ((bool)effectMeshObject.Value.effectMeshGO && flag)
			{
				UnityEngine.Object.DestroyImmediate(effectMeshObject.Value.effectMeshGO);
				list.Add(effectMeshObject.Key);
			}
		}
		foreach (MRUKAnchor item in list)
		{
			effectMeshObjects.Remove(item);
			SceneTrackingSettings.UnTrackedAnchors.Add(item);
		}
	}

	public void DestroyMesh(MRUKRoom room)
	{
		foreach (MRUKAnchor anchor in room.Anchors)
		{
			DestroyMesh(anchor);
		}
		SceneTrackingSettings.UnTrackedRooms.Add(room);
	}

	public void DestroyMesh(MRUKAnchor anchor)
	{
		if (effectMeshObjects.TryGetValue(anchor, out var value) && (bool)value.effectMeshGO)
		{
			UnityEngine.Object.DestroyImmediate(value.effectMeshGO);
			effectMeshObjects.Remove(anchor);
			SceneTrackingSettings.UnTrackedAnchors.Add(anchor);
		}
	}

	public void AddColliders(LabelFilter label = default(LabelFilter))
	{
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			bool flag = label.PassesFilter(effectMeshObject.Key.Label);
			if ((bool)effectMeshObject.Key && !effectMeshObject.Value.collider && flag)
			{
				effectMeshObject.Value.collider = AddCollider(effectMeshObject.Key, effectMeshObject.Value);
			}
		}
	}

	public void DestroyColliders(LabelFilter label = default(LabelFilter))
	{
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			bool flag = label.PassesFilter(effectMeshObject.Key.Label);
			if ((bool)effectMeshObject.Value.collider && flag)
			{
				UnityEngine.Object.DestroyImmediate(effectMeshObject.Value.collider);
			}
		}
	}

	public void ToggleShadowCasting(bool shouldCast, LabelFilter label = default(LabelFilter))
	{
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			bool flag = label.PassesFilter(effectMeshObject.Key.Label);
			if ((bool)effectMeshObject.Value.effectMeshGO && flag)
			{
				ShadowCastingMode shadowCastingMode = (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
				effectMeshObject.Value.effectMeshGO.GetComponent<MeshRenderer>().shadowCastingMode = shadowCastingMode;
			}
		}
	}

	public void ToggleEffectMeshVisibility(bool shouldShow, LabelFilter label = default(LabelFilter), Material materialOverride = null)
	{
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			bool flag = label.PassesFilter(effectMeshObject.Key.Label);
			if ((bool)effectMeshObject.Value.effectMeshGO && flag)
			{
				effectMeshObject.Value.effectMeshGO.GetComponent<MeshRenderer>().enabled = shouldShow;
				if ((bool)materialOverride)
				{
					effectMeshObject.Value.effectMeshGO.GetComponent<MeshRenderer>().material = materialOverride;
				}
			}
		}
	}

	public void ToggleEffectMeshColliders(bool doEnable, LabelFilter label = default(LabelFilter))
	{
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			if (label.PassesFilter(effectMeshObject.Key.Label))
			{
				if (!effectMeshObject.Value.collider)
				{
					AddCollider(effectMeshObject.Key, effectMeshObject.Value);
				}
				effectMeshObject.Value.collider.enabled = doEnable;
			}
		}
	}

	public void OverrideEffectMaterial(Material newMaterial, LabelFilter label = default(LabelFilter))
	{
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			bool flag = label.PassesFilter(effectMeshObject.Key.Label);
			if ((bool)effectMeshObject.Value.effectMeshGO && flag)
			{
				effectMeshObject.Value.effectMeshGO.GetComponent<MeshRenderer>().material = newMaterial;
			}
		}
	}

	private static void OrderWalls(List<MRUKAnchor> walls)
	{
		int count = walls.Count;
		if (count <= 1)
		{
			return;
		}
		List<MRUKAnchor> list;
		using (new OVRObjectPool.ListScope<MRUKAnchor>(out list))
		{
			int index = count - 1;
			MRUKAnchor mRUKAnchor = walls[index];
			list.Add(mRUKAnchor);
			walls.RemoveAt(index);
			while (walls.Count > 0)
			{
				float num = float.MaxValue;
				int index2 = -1;
				Vector3 a = mRUKAnchor.transform.position + mRUKAnchor.transform.right * mRUKAnchor.PlaneRect.Value.min.x;
				for (int i = 0; i < walls.Count; i++)
				{
					MRUKAnchor mRUKAnchor2 = walls[i];
					Vector3 b = mRUKAnchor2.transform.position + mRUKAnchor2.transform.right * mRUKAnchor2.PlaneRect.Value.max.x;
					float num2 = Vector3.Distance(a, b);
					if (num2 < num)
					{
						num = num2;
						index2 = i;
					}
				}
				mRUKAnchor = walls[index2];
				list.Add(mRUKAnchor);
				walls.RemoveAt(index2);
			}
			walls.AddRange(list);
		}
	}

	public void CreateMesh(MRUKRoom room)
	{
		CreateMesh(room, null);
	}

	private void CreateMesh(MRUKRoom room, List<MRUKRoom> connectedRooms)
	{
		Span<MRUKAnchor.SceneLabels> span = stackalloc MRUKAnchor.SceneLabels[1] { MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.INVISIBLE_WALL_FACE };
		MRUKAnchor.SceneLabels sceneLabels = (MRUKAnchor.SceneLabels)0;
		Span<MRUKAnchor.SceneLabels> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			MRUKAnchor.SceneLabels sceneLabels2 = span2[i];
			sceneLabels |= sceneLabels2;
		}
		foreach (MRUKAnchor anchor in room.Anchors)
		{
			if (anchor.HasAnyLabel(Labels) && !anchor.HasAnyLabel(sceneLabels))
			{
				if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.GLOBAL_MESH))
				{
					CreateGlobalMeshObject(anchor);
				}
				else
				{
					CreateEffectMesh(anchor);
				}
			}
		}
		float num = 0f;
		foreach (MRUKAnchor anchor2 in room.Anchors)
		{
			if (anchor2.HasAnyLabel(sceneLabels))
			{
				num += anchor2.PlaneRect.Value.size.x;
			}
		}
		List<MRUKAnchor> list;
		using (new OVRObjectPool.ListScope<MRUKAnchor>(out list))
		{
			float uSpacing = 0f;
			span2 = span;
			for (int i = 0; i < span2.Length; i++)
			{
				MRUKAnchor.SceneLabels sceneLabels3 = span2[i];
				if (!IncludesLabel(sceneLabels3))
				{
					continue;
				}
				list.Clear();
				foreach (MRUKAnchor anchor3 in room.Anchors)
				{
					if (anchor3.HasAnyLabel(sceneLabels3))
					{
						list.Add(anchor3);
					}
				}
				OrderWalls(list);
				foreach (MRUKAnchor item in list)
				{
					if (IncludesLabel(item.Label))
					{
						CreateEffectMeshWall(item, num, ref uSpacing, connectedRooms);
					}
				}
			}
		}
		RegisterAnchorUpdates(room);
		if (!TrackUpdates)
		{
			SceneTrackingSettings.UnTrackedRooms.Add(room);
		}
	}

	private bool IncludesLabel(MRUKAnchor.SceneLabels label)
	{
		return (Labels & label) != 0;
	}

	public EffectMeshObject CreateEffectMesh(MRUKAnchor anchorInfo)
	{
		if (effectMeshObjects.ContainsKey(anchorInfo))
		{
			return null;
		}
		EffectMeshObject effectMeshObject = new EffectMeshObject();
		Mesh mesh = Utilities.SetupAnchorMeshGeometry(anchorInfo, useFunctionalSurfaces: false, textureCoordinateModes);
		GameObject gameObject = new GameObject(anchorInfo.name + Suffix);
		gameObject.transform.SetParent(anchorInfo.transform, worldPositionStays: false);
		gameObject.layer = Layer;
		effectMeshObject.effectMeshGO = gameObject;
		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		if (MeshMaterial != null)
		{
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.material = MeshMaterial;
			meshRenderer.shadowCastingMode = (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			meshRenderer.enabled = !hideMesh;
		}
		mesh.name = anchorInfo.name;
		effectMeshObject.mesh = mesh;
		if (Colliders)
		{
			effectMeshObject.collider = AddCollider(anchorInfo, effectMeshObject);
		}
		effectMeshObjects.Add(anchorInfo, effectMeshObject);
		return effectMeshObject;
	}

	private Collider AddCollider(MRUKAnchor anchorInfo, EffectMeshObject effectMeshObject)
	{
		if (anchorInfo.VolumeBounds.HasValue)
		{
			BoxCollider boxCollider = effectMeshObject.effectMeshGO.AddComponent<BoxCollider>();
			boxCollider.size = anchorInfo.VolumeBounds.Value.size;
			boxCollider.center = anchorInfo.VolumeBounds.Value.center;
			return boxCollider;
		}
		MeshCollider meshCollider = effectMeshObject.effectMeshGO.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = effectMeshObject.mesh;
		meshCollider.convex = false;
		return meshCollider;
	}

	private float GetSeamlessFactor(float totalWallLength, float stepSize)
	{
		float b = Mathf.Round(totalWallLength / stepSize);
		b = Mathf.Max(1f, b);
		return totalWallLength / b;
	}

	private void CreateEffectMeshWall(MRUKAnchor anchorInfo, float totalWallLength, ref float uSpacing, List<MRUKRoom> connectedRooms)
	{
		if (effectMeshObjects.ContainsKey(anchorInfo))
		{
			return;
		}
		EffectMeshObject effectMeshObject = new EffectMeshObject();
		GameObject gameObject = new GameObject(anchorInfo.name + Suffix);
		gameObject.layer = Layer;
		gameObject.transform.SetParent(anchorInfo.transform, worldPositionStays: false);
		effectMeshObject.effectMeshGO = gameObject;
		Mesh mesh = new Mesh();
		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		if (MeshMaterial != null)
		{
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.material = MeshMaterial;
			meshRenderer.shadowCastingMode = (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			meshRenderer.enabled = !hideMesh;
		}
		List<List<Vector2>> list = null;
		Rect value = anchorInfo.PlaneRect.Value;
		foreach (MRUKAnchor childAnchor in anchorInfo.ChildAnchors)
		{
			if (childAnchor.PlaneRect.HasValue && (childAnchor.Label & CutHoles) != 0)
			{
				Vector2 vector = anchorInfo.transform.InverseTransformPoint(childAnchor.transform.position);
				Rect value2 = childAnchor.PlaneRect.Value;
				value2.position += new Vector2(vector.x, vector.y);
				List<Vector2> list2 = new List<Vector2>(childAnchor.PlaneBoundary2D.Count);
				for (int num = childAnchor.PlaneBoundary2D.Count - 1; num >= 0; num--)
				{
					list2.Add(childAnchor.PlaneBoundary2D[num] + vector);
				}
				if (list == null)
				{
					list = new List<List<Vector2>>();
				}
				list.Add(list2);
			}
		}
		Triangulator.TriangulatePoints(anchorInfo.PlaneBoundary2D, list, out var outVertices, out var outIndices);
		int num2 = outVertices.Length;
		int num3 = Math.Min(8, textureCoordinateModes.Length);
		Vector3[] array = new Vector3[num2];
		for (int i = 0; i < outVertices.Length; i++)
		{
			array[i] = outVertices[i];
		}
		Vector2[][] array2 = new Vector2[num3][];
		for (int j = 0; j < num3; j++)
		{
			array2[j] = new Vector2[num2];
		}
		Color32[] array3 = new Color32[num2];
		Vector3[] array4 = new Vector3[num2];
		Vector4[] array5 = new Vector4[num2];
		int num4 = 0;
		float seamlessFactor = GetSeamlessFactor(totalWallLength, 1f);
		float width = value.width;
		Vector3 forward = Vector3.forward;
		Vector4 vector2 = new Vector4(1f, 0f, 0f, 1f);
		for (int k = 0; k < outVertices.Length; k++)
		{
			Vector3 vector3 = array[k];
			float num5 = vector3.x - value.xMin;
			float num6 = vector3.y - value.yMin;
			for (int l = 0; l < num3; l++)
			{
				float num7 = uSpacing;
				float num8;
				if (textureCoordinateModes[l].WallV != 0)
				{
					_ = 2;
					num8 = value.height;
				}
				else
				{
					num8 = 1f;
				}
				float num9;
				switch (textureCoordinateModes[l].WallU)
				{
				default:
					num9 = totalWallLength;
					break;
				case WallTextureCoordinateModeU.METRIC:
					num9 = 1f;
					break;
				case WallTextureCoordinateModeU.METRIC_SEAMLESS:
					num9 = seamlessFactor;
					break;
				case WallTextureCoordinateModeU.MAINTAIN_ASPECT_RATIO:
					num9 = num8;
					break;
				case WallTextureCoordinateModeU.MAINTAIN_ASPECT_RATIO_SEAMLESS:
					num9 = GetSeamlessFactor(totalWallLength, num8);
					break;
				case WallTextureCoordinateModeU.STRETCH_SECTION:
					num9 = width;
					num7 = 0f;
					break;
				}
				if (textureCoordinateModes[l].WallV == WallTextureCoordinateModeV.MAINTAIN_ASPECT_RATIO)
				{
					num8 = num9;
				}
				array2[l][num4] = new Vector2((num7 + width - num5) / num9, num6 / num8);
			}
			array[num4] = new Vector3(vector3.x, vector3.y, 0f);
			array3[num4] = Color.white;
			array4[num4] = forward;
			array5[num4] = vector2;
			num4++;
		}
		uSpacing += width;
		int[] triangles = outIndices;
		mesh.Clear();
		mesh.name = anchorInfo.name;
		mesh.vertices = array;
		for (int m = 0; m < num3; m++)
		{
			switch (m)
			{
			case 0:
				mesh.uv = array2[m];
				break;
			case 1:
				mesh.uv2 = array2[m];
				break;
			case 2:
				mesh.uv3 = array2[m];
				break;
			case 3:
				mesh.uv4 = array2[m];
				break;
			case 4:
				mesh.uv5 = array2[m];
				break;
			case 5:
				mesh.uv6 = array2[m];
				break;
			case 6:
				mesh.uv7 = array2[m];
				break;
			case 7:
				mesh.uv8 = array2[m];
				break;
			}
		}
		mesh.colors32 = array3;
		mesh.triangles = triangles;
		mesh.normals = array4;
		mesh.tangents = array5;
		effectMeshObject.mesh = mesh;
		if (Colliders)
		{
			effectMeshObject.collider = AddCollider(anchorInfo, effectMeshObject);
		}
		effectMeshObjects.Add(anchorInfo, effectMeshObject);
	}

	private void CreateGlobalMeshObject(MRUKAnchor globalMeshAnchor)
	{
		if (!globalMeshAnchor)
		{
			Debug.LogWarning("No global mesh was found in the current room");
		}
		else if (!effectMeshObjects.ContainsKey(globalMeshAnchor))
		{
			EffectMeshObject effectMeshObject = new EffectMeshObject();
			GameObject gameObject = new GameObject(globalMeshAnchor.name + Suffix, typeof(MeshFilter), typeof(MeshRenderer));
			gameObject.layer = Layer;
			gameObject.transform.SetParent(globalMeshAnchor.transform, worldPositionStays: false);
			effectMeshObject.effectMeshGO = gameObject;
			globalMeshAnchor.Mesh.RecalculateNormals();
			Mesh mesh = globalMeshAnchor.Mesh;
			gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
			if (Colliders)
			{
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = mesh;
				effectMeshObject.collider = meshCollider;
			}
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			if (MeshMaterial != null)
			{
				component.material = MeshMaterial;
			}
			component.enabled = !hideMesh;
			component.shadowCastingMode = (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			effectMeshObject.mesh = mesh;
			effectMeshObjects.Add(globalMeshAnchor, effectMeshObject);
		}
	}

	public void SetEffectObjectsParent(Transform newParent)
	{
		foreach (KeyValuePair<MRUKAnchor, EffectMeshObject> effectMeshObject in effectMeshObjects)
		{
			effectMeshObject.Value.effectMeshGO.transform.SetParent(newParent);
		}
	}
}
