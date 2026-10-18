using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Unity.AI.Navigation;

[ExecuteAlways]
[DefaultExecutionOrder(-102)]
[AddComponentMenu("Navigation/NavMesh Surface", 30)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshSurface.html")]
public class NavMeshSurface : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private byte m_SerializedVersion;

	[SerializeField]
	private int m_AgentTypeID;

	[SerializeField]
	private CollectObjects m_CollectObjects;

	[SerializeField]
	private Vector3 m_Size = new Vector3(10f, 10f, 10f);

	[SerializeField]
	private Vector3 m_Center = new Vector3(0f, 2f, 0f);

	[SerializeField]
	private LayerMask m_LayerMask = -1;

	[SerializeField]
	private NavMeshCollectGeometry m_UseGeometry;

	[SerializeField]
	private int m_DefaultArea;

	[SerializeField]
	private bool m_GenerateLinks;

	[SerializeField]
	private bool m_IgnoreNavMeshAgent = true;

	[SerializeField]
	private bool m_IgnoreNavMeshObstacle = true;

	[SerializeField]
	private bool m_OverrideTileSize;

	[SerializeField]
	private int m_TileSize = 256;

	[SerializeField]
	private bool m_OverrideVoxelSize;

	[SerializeField]
	private float m_VoxelSize;

	[SerializeField]
	private float m_MinRegionArea = 2f;

	[FormerlySerializedAs("m_BakedNavMeshData")]
	[SerializeField]
	private NavMeshData m_NavMeshData;

	[SerializeField]
	private bool m_BuildHeightMesh;

	private NavMeshDataInstance m_NavMeshDataInstance;

	private Vector3 m_LastPosition = Vector3.zero;

	private Quaternion m_LastRotation = Quaternion.identity;

	private static readonly List<NavMeshSurface> s_NavMeshSurfaces = new List<NavMeshSurface>();

	public int agentTypeID
	{
		get
		{
			return m_AgentTypeID;
		}
		set
		{
			m_AgentTypeID = value;
		}
	}

	public CollectObjects collectObjects
	{
		get
		{
			return m_CollectObjects;
		}
		set
		{
			m_CollectObjects = value;
		}
	}

	public Vector3 size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size = value;
		}
	}

	public Vector3 center
	{
		get
		{
			return m_Center;
		}
		set
		{
			m_Center = value;
		}
	}

	public LayerMask layerMask
	{
		get
		{
			return m_LayerMask;
		}
		set
		{
			m_LayerMask = value;
		}
	}

	public NavMeshCollectGeometry useGeometry
	{
		get
		{
			return m_UseGeometry;
		}
		set
		{
			m_UseGeometry = value;
		}
	}

	public int defaultArea
	{
		get
		{
			return m_DefaultArea;
		}
		set
		{
			m_DefaultArea = value;
		}
	}

	public bool ignoreNavMeshAgent
	{
		get
		{
			return m_IgnoreNavMeshAgent;
		}
		set
		{
			m_IgnoreNavMeshAgent = value;
		}
	}

	public bool ignoreNavMeshObstacle
	{
		get
		{
			return m_IgnoreNavMeshObstacle;
		}
		set
		{
			m_IgnoreNavMeshObstacle = value;
		}
	}

	public bool overrideTileSize
	{
		get
		{
			return m_OverrideTileSize;
		}
		set
		{
			m_OverrideTileSize = value;
		}
	}

	public int tileSize
	{
		get
		{
			return m_TileSize;
		}
		set
		{
			m_TileSize = value;
		}
	}

	public bool overrideVoxelSize
	{
		get
		{
			return m_OverrideVoxelSize;
		}
		set
		{
			m_OverrideVoxelSize = value;
		}
	}

	public float voxelSize
	{
		get
		{
			return m_VoxelSize;
		}
		set
		{
			m_VoxelSize = value;
		}
	}

	public float minRegionArea
	{
		get
		{
			return m_MinRegionArea;
		}
		set
		{
			m_MinRegionArea = value;
		}
	}

	public bool buildHeightMesh
	{
		get
		{
			return m_BuildHeightMesh;
		}
		set
		{
			m_BuildHeightMesh = value;
		}
	}

	public NavMeshData navMeshData
	{
		get
		{
			return m_NavMeshData;
		}
		set
		{
			m_NavMeshData = value;
		}
	}

	internal NavMeshDataInstance navMeshDataInstance => m_NavMeshDataInstance;

	public static List<NavMeshSurface> activeSurfaces => s_NavMeshSurfaces;

	private Bounds GetInflatedBounds()
	{
		NavMeshBuildSettings settingsByID = NavMesh.GetSettingsByID(m_AgentTypeID);
		float num = ((settingsByID.agentTypeID != -1) ? settingsByID.agentRadius : 0f);
		Bounds result = new Bounds(center, size);
		result.Expand(new Vector3(num, 0f, num));
		return result;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ClearNavMeshSurfaces()
	{
		s_NavMeshSurfaces.Clear();
	}

	private void OnEnable()
	{
		Register(this);
		AddData();
	}

	private void OnDisable()
	{
		RemoveData();
		Unregister(this);
	}

	public void AddData()
	{
		if (!m_NavMeshDataInstance.valid)
		{
			if (m_NavMeshData != null)
			{
				m_NavMeshDataInstance = NavMesh.AddNavMeshData(m_NavMeshData, base.transform.position, base.transform.rotation);
				m_NavMeshDataInstance.owner = this;
			}
			m_LastPosition = base.transform.position;
			m_LastRotation = base.transform.rotation;
		}
	}

	public void RemoveData()
	{
		m_NavMeshDataInstance.Remove();
		m_NavMeshDataInstance = default(NavMeshDataInstance);
	}

	public NavMeshBuildSettings GetBuildSettings()
	{
		NavMeshBuildSettings settingsByID = NavMesh.GetSettingsByID(m_AgentTypeID);
		if (settingsByID.agentTypeID == -1)
		{
			Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
			settingsByID.agentTypeID = m_AgentTypeID;
		}
		if (overrideTileSize)
		{
			settingsByID.overrideTileSize = true;
			settingsByID.tileSize = tileSize;
		}
		if (overrideVoxelSize)
		{
			settingsByID.overrideVoxelSize = true;
			settingsByID.voxelSize = voxelSize;
		}
		settingsByID.minRegionArea = minRegionArea;
		settingsByID.buildHeightMesh = buildHeightMesh;
		return settingsByID;
	}

	public void BuildNavMesh()
	{
		List<NavMeshBuildSource> sources = CollectSources();
		Bounds localBounds = new Bounds(m_Center, Abs(m_Size));
		if (m_CollectObjects != CollectObjects.Volume)
		{
			localBounds = CalculateWorldBounds(sources);
		}
		NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(), sources, localBounds, base.transform.position, base.transform.rotation);
		if (navMeshData != null)
		{
			navMeshData.name = base.gameObject.name;
			RemoveData();
			m_NavMeshData = navMeshData;
			if (base.isActiveAndEnabled)
			{
				AddData();
			}
		}
	}

	public AsyncOperation UpdateNavMesh(NavMeshData data)
	{
		List<NavMeshBuildSource> sources = CollectSources();
		Bounds localBounds = new Bounds(m_Center, Abs(m_Size));
		if (m_CollectObjects != CollectObjects.Volume)
		{
			localBounds = CalculateWorldBounds(sources);
		}
		return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, localBounds);
	}

	private static void Register(NavMeshSurface surface)
	{
		if (s_NavMeshSurfaces.Count == 0)
		{
			NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Combine(NavMesh.onPreUpdate, new NavMesh.OnNavMeshPreUpdate(UpdateActive));
		}
		if (!s_NavMeshSurfaces.Contains(surface))
		{
			s_NavMeshSurfaces.Add(surface);
		}
	}

	private static void Unregister(NavMeshSurface surface)
	{
		s_NavMeshSurfaces.Remove(surface);
		if (s_NavMeshSurfaces.Count == 0)
		{
			NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Remove(NavMesh.onPreUpdate, new NavMesh.OnNavMeshPreUpdate(UpdateActive));
		}
	}

	private static void UpdateActive()
	{
		for (int i = 0; i < s_NavMeshSurfaces.Count; i++)
		{
			s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
		}
	}

	private void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
	{
		List<NavMeshModifierVolume> list;
		if (m_CollectObjects == CollectObjects.Children)
		{
			list = new List<NavMeshModifierVolume>(GetComponentsInChildren<NavMeshModifierVolume>());
			list.RemoveAll((NavMeshModifierVolume x) => !x.isActiveAndEnabled);
		}
		else
		{
			list = NavMeshModifierVolume.activeModifiers;
		}
		foreach (NavMeshModifierVolume item2 in list)
		{
			if (((int)m_LayerMask & (1 << item2.gameObject.layer)) != 0 && item2.AffectsAgentType(m_AgentTypeID))
			{
				Vector3 pos = item2.transform.TransformPoint(item2.center);
				Vector3 lossyScale = item2.transform.lossyScale;
				Vector3 vector = new Vector3(item2.size.x * Mathf.Abs(lossyScale.x), item2.size.y * Mathf.Abs(lossyScale.y), item2.size.z * Mathf.Abs(lossyScale.z));
				NavMeshBuildSource item = default(NavMeshBuildSource);
				item.shape = NavMeshBuildSourceShape.ModifierBox;
				item.transform = Matrix4x4.TRS(pos, item2.transform.rotation, Vector3.one);
				item.size = vector;
				item.area = item2.area;
				sources.Add(item);
			}
		}
	}

	private List<NavMeshBuildSource> CollectSources()
	{
		List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
		List<NavMeshBuildMarkup> list = new List<NavMeshBuildMarkup>();
		List<NavMeshModifier> list2;
		if (m_CollectObjects == CollectObjects.Children)
		{
			list2 = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
			list2.RemoveAll((NavMeshModifier x) => !x.isActiveAndEnabled);
		}
		else
		{
			list2 = NavMeshModifier.activeModifiers;
		}
		foreach (NavMeshModifier item2 in list2)
		{
			if (((int)m_LayerMask & (1 << item2.gameObject.layer)) != 0 && item2.AffectsAgentType(m_AgentTypeID))
			{
				NavMeshBuildMarkup item = default(NavMeshBuildMarkup);
				item.root = item2.transform;
				item.overrideArea = item2.overrideArea;
				item.area = item2.area;
				item.ignoreFromBuild = item2.ignoreFromBuild;
				item.applyToChildren = item2.applyToChildren;
				item.overrideGenerateLinks = item2.overrideGenerateLinks;
				item.generateLinks = item2.generateLinks;
				list.Add(item);
			}
		}
		switch (m_CollectObjects)
		{
		default:
			CollectSourcesInHierarchy(null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, list, includeOnlyMarkedObjects: false, sources);
			break;
		case CollectObjects.Children:
			CollectSourcesInHierarchy(base.transform, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, list, includeOnlyMarkedObjects: false, sources);
			break;
		case CollectObjects.Volume:
		{
			Bounds worldBounds = GetWorldBounds(Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one), GetInflatedBounds());
			CollectSourcesInVolume(worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, list, includeOnlyMarkedObjects: false, sources);
			break;
		}
		case CollectObjects.MarkedWithModifier:
			CollectSourcesInHierarchy(null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, list, includeOnlyMarkedObjects: true, sources);
			break;
		}
		if (m_IgnoreNavMeshAgent)
		{
			sources.RemoveAll((NavMeshBuildSource x) => x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null);
		}
		if (m_IgnoreNavMeshObstacle)
		{
			sources.RemoveAll((NavMeshBuildSource x) => x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null);
		}
		AppendModifierVolumes(ref sources);
		return sources;
	}

	private static Vector3 Abs(Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	private static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
	{
		Vector3 vector = Abs(mat.MultiplyVector(Vector3.right));
		Vector3 vector2 = Abs(mat.MultiplyVector(Vector3.up));
		Vector3 vector3 = Abs(mat.MultiplyVector(Vector3.forward));
		Vector3 vector4 = mat.MultiplyPoint(bounds.center);
		Vector3 vector5 = vector * bounds.size.x + vector2 * bounds.size.y + vector3 * bounds.size.z;
		return new Bounds(vector4, vector5);
	}

	private Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
	{
		Matrix4x4 inverse = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one).inverse;
		Bounds result = default(Bounds);
		foreach (NavMeshBuildSource source in sources)
		{
			switch (source.shape)
			{
			case NavMeshBuildSourceShape.Mesh:
			{
				Mesh mesh = source.sourceObject as Mesh;
				result.Encapsulate(GetWorldBounds(inverse * source.transform, mesh.bounds));
				break;
			}
			case NavMeshBuildSourceShape.Terrain:
			{
				TerrainData terrainData = source.sourceObject as TerrainData;
				result.Encapsulate(GetWorldBounds(inverse * source.transform, new Bounds(0.5f * terrainData.size, terrainData.size)));
				break;
			}
			case NavMeshBuildSourceShape.Box:
			case NavMeshBuildSourceShape.Sphere:
			case NavMeshBuildSourceShape.Capsule:
			case NavMeshBuildSourceShape.ModifierBox:
				result.Encapsulate(GetWorldBounds(inverse * source.transform, new Bounds(Vector3.zero, source.size)));
				break;
			}
		}
		result.Expand(0.1f);
		return result;
	}

	private bool HasTransformChanged()
	{
		if (m_LastPosition != base.transform.position)
		{
			return true;
		}
		if (m_LastRotation != base.transform.rotation)
		{
			return true;
		}
		return false;
	}

	private void UpdateDataIfTransformChanged()
	{
		if (HasTransformChanged())
		{
			RemoveData();
			AddData();
		}
	}

	private void CollectSourcesInVolume(Bounds includedWorldBounds, int includedLayerMask, NavMeshCollectGeometry geometry, int areaByDefault, bool generateLinksByDefault, List<NavMeshBuildMarkup> markups, bool includeOnlyMarkedObjects, List<NavMeshBuildSource> results)
	{
		NavMeshBuilder.CollectSources(includedWorldBounds, includedLayerMask, geometry, areaByDefault, generateLinksByDefault, markups, includeOnlyMarkedObjects, results);
	}

	private void CollectSourcesInHierarchy(Transform root, int includedLayerMask, NavMeshCollectGeometry geometry, int areaByDefault, bool generateLinksByDefault, List<NavMeshBuildMarkup> markups, bool includeOnlyMarkedObjects, List<NavMeshBuildSource> results)
	{
		NavMeshBuilder.CollectSources(root, includedLayerMask, geometry, areaByDefault, generateLinksByDefault, markups, includeOnlyMarkedObjects, results);
	}
}
