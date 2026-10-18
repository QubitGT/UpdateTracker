using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines.ExtrusionShapes;

namespace UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("Splines/Spline Extrude")]
[ExecuteAlways]
public class SplineExtrude : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The Spline to extrude.")]
	private SplineContainer m_Container;

	[SerializeField]
	[Tooltip("The mesh that should be extruded. If none, a temporary mesh will be created.")]
	private Mesh m_TargetMesh;

	[SerializeField]
	[Tooltip("Enable to regenerate the extruded mesh when the target Spline is modified. Disable this option if the Spline will not be modified at runtime.")]
	private bool m_RebuildOnSplineChange = true;

	[SerializeField]
	[Tooltip("The maximum number of times per-second that the mesh will be rebuilt.")]
	private int m_RebuildFrequency = 30;

	[SerializeField]
	[Tooltip("Automatically update any Mesh, Box, or Sphere collider components when the mesh is extruded.")]
	private bool m_UpdateColliders = true;

	[SerializeField]
	[Tooltip("The number of sides that comprise the radius of the mesh.")]
	private int m_Sides = 8;

	[SerializeField]
	[Tooltip("The number of edge loops that comprise the length of one unit of the mesh. The total number of sections is equal to \"Spline.GetLength() * segmentsPerUnit\".")]
	private float m_SegmentsPerUnit = 4f;

	[SerializeField]
	[Tooltip("Indicates if the start and end of the mesh are filled. When the target Spline is closed or when the profile of the shape to extrude is concave, this setting is ignored.")]
	private bool m_Capped = true;

	[SerializeField]
	[Tooltip("The radius of the extruded mesh.")]
	private float m_Radius = 0.25f;

	[SerializeField]
	[Tooltip("The section of the Spline to extrude.")]
	private Vector2 m_Range = new Vector2(0f, 1f);

	[SerializeField]
	[Tooltip("Set true to reverse the winding order of vertices so that the face normals are inverted.")]
	private bool m_FlipNormals;

	private Mesh m_Mesh;

	private float m_NextScheduledRebuild;

	private float m_AutosmoothAngle = 180f;

	private bool m_RebuildRequested;

	private bool m_CanCapEnds;

	[SerializeReference]
	private IExtrudeShape m_Shape;

	internal static readonly string k_EmptyContainerError = "Spline Extrude does not have a valid SplineContainer set.";

	internal bool CanCapEnds => m_CanCapEnds;

	internal IExtrudeShape Shape
	{
		get
		{
			return m_Shape;
		}
		set
		{
			m_Shape = value;
		}
	}

	[Obsolete("Use Container instead.", false)]
	public SplineContainer container => Container;

	public SplineContainer Container
	{
		get
		{
			return m_Container;
		}
		set
		{
			m_Container = value;
		}
	}

	[Obsolete("Use RebuildOnSplineChange instead.", false)]
	public bool rebuildOnSplineChange => RebuildOnSplineChange;

	public bool RebuildOnSplineChange
	{
		get
		{
			return m_RebuildOnSplineChange;
		}
		set
		{
			m_RebuildOnSplineChange = value;
			if (!value)
			{
				m_RebuildRequested = value;
			}
		}
	}

	[Obsolete("Use RebuildFrequency instead.", false)]
	public int rebuildFrequency => RebuildFrequency;

	public int RebuildFrequency
	{
		get
		{
			return m_RebuildFrequency;
		}
		set
		{
			m_RebuildFrequency = Mathf.Max(value, 1);
		}
	}

	[Obsolete("Use Sides instead.", false)]
	public int sides => Sides;

	public int Sides
	{
		get
		{
			return m_Sides;
		}
		set
		{
			m_Sides = Mathf.Max(value, 3);
			if (m_Shape == null)
			{
				Circle circle = new Circle();
				circle.SideCount = m_Sides;
				m_Shape = circle;
			}
		}
	}

	[Obsolete("Use SegmentsPerUnit instead.", false)]
	public float segmentsPerUnit => SegmentsPerUnit;

	public float SegmentsPerUnit
	{
		get
		{
			return m_SegmentsPerUnit;
		}
		set
		{
			m_SegmentsPerUnit = Mathf.Max(value, 0.0001f);
		}
	}

	[Obsolete("Use Capped instead.", false)]
	public bool capped => Capped;

	public bool Capped
	{
		get
		{
			return m_Capped;
		}
		set
		{
			m_Capped = value;
		}
	}

	[Obsolete("Use Radius instead.", false)]
	public float radius => Radius;

	public float Radius
	{
		get
		{
			return m_Radius;
		}
		set
		{
			m_Radius = Mathf.Max(value, 1E-05f);
		}
	}

	[Obsolete("Use Range instead.", false)]
	public Vector2 range => Range;

	public Vector2 Range
	{
		get
		{
			return m_Range;
		}
		set
		{
			m_Range = new Vector2(Mathf.Min(value.x, value.y), Mathf.Max(value.x, value.y));
		}
	}

	public bool FlipNormals
	{
		get
		{
			return m_FlipNormals;
		}
		set
		{
			m_FlipNormals = value;
		}
	}

	public Mesh targetMesh
	{
		get
		{
			return m_TargetMesh;
		}
		set
		{
			if (!(m_TargetMesh == value))
			{
				CleanupMesh();
				m_TargetMesh = value;
				EnsureMeshExists();
				Rebuild();
			}
		}
	}

	[Obsolete("Use Spline instead.", false)]
	public Spline spline => Spline;

	public Spline Spline => m_Container?.Spline;

	public IReadOnlyList<Spline> Splines => m_Container?.Splines;

	internal void Reset()
	{
		TryGetComponent<SplineContainer>(out m_Container);
		if (TryGetComponent<MeshRenderer>(out var component) && component.sharedMaterial == null)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Material sharedMaterial = obj.GetComponent<MeshRenderer>().sharedMaterial;
			Object.DestroyImmediate(obj);
			component.sharedMaterial = sharedMaterial;
		}
		Rebuild();
	}

	private bool IsNullOrEmptyContainer()
	{
		if (!(m_Container == null) && m_Container.Spline != null)
		{
			return m_Container.Splines.Count == 0;
		}
		return true;
	}

	internal void SetSplineContainerOnGO()
	{
		SplineContainer component = base.gameObject.GetComponent<SplineContainer>();
		if (component != null && component != m_Container)
		{
			m_Container = component;
		}
	}

	private void OnEnable()
	{
		EnsureMeshExists();
		Spline.Changed += OnSplineChanged;
		if (!IsNullOrEmptyContainer())
		{
			Rebuild();
		}
	}

	private void OnDisable()
	{
		Spline.Changed -= OnSplineChanged;
		CleanupMesh();
	}

	private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
	{
		if (m_RebuildOnSplineChange)
		{
			bool flag = m_Container != null && Splines.Contains(spline);
			bool flag2 = m_Shape is SplineShape { Spline: not null } splineShape && splineShape.Spline.Equals(spline);
			m_RebuildRequested |= flag || flag2;
		}
	}

	private void Update()
	{
		if (m_RebuildRequested && Time.time >= m_NextScheduledRebuild)
		{
			Rebuild();
		}
	}

	private void EnsureMeshExists()
	{
		if (m_Mesh == null)
		{
			if (targetMesh != null)
			{
				m_Mesh = targetMesh;
			}
			else
			{
				m_Mesh = new Mesh
				{
					name = "<Spline Extruded Mesh>"
				};
				m_Mesh.hideFlags = HideFlags.HideAndDontSave;
			}
		}
		if (TryGetComponent<MeshFilter>(out var component))
		{
			component.hideFlags = HideFlags.NotEditable;
			component.sharedMesh = m_Mesh;
		}
	}

	private void CleanupMesh()
	{
		if (TryGetComponent<MeshFilter>(out var component))
		{
			component.hideFlags = HideFlags.None;
			component.sharedMesh = null;
		}
		if (m_Mesh != m_TargetMesh)
		{
			Object.DestroyImmediate(m_Mesh);
		}
		m_Mesh = null;
	}

	public void Rebuild()
	{
		if (m_Shape == null)
		{
			Circle circle = new Circle();
			circle.SideCount = m_Sides;
			m_Shape = circle;
		}
		if (m_Mesh == null)
		{
			return;
		}
		if (IsNullOrEmptyContainer())
		{
			if (Application.isPlaying)
			{
				Debug.LogError(k_EmptyContainerError, this);
			}
			return;
		}
		m_Mesh.Clear();
		if (m_Range.x == m_Range.y)
		{
			return;
		}
		ExtrudeSettings<IExtrudeShape> extrudeSettings = new ExtrudeSettings<IExtrudeShape>(m_Shape);
		extrudeSettings.Radius = m_Radius;
		extrudeSettings.CapEnds = m_Capped;
		extrudeSettings.Range = m_Range;
		extrudeSettings.FlipNormals = m_FlipNormals;
		ExtrudeSettings<IExtrudeShape> settings = extrudeSettings;
		SplineMesh.Extrude(m_Container.Splines, m_Mesh, settings, SegmentsPerUnit);
		m_CanCapEnds = SplineMesh.s_IsConvex && !Spline.Closed;
		AutosmoothNormals();
		m_NextScheduledRebuild = Time.time + 1f / (float)m_RebuildFrequency;
		if (m_UpdateColliders)
		{
			if (TryGetComponent<MeshCollider>(out var component))
			{
				component.sharedMesh = m_Mesh;
			}
			if (TryGetComponent<BoxCollider>(out var component2))
			{
				component2.center = m_Mesh.bounds.center;
				component2.size = m_Mesh.bounds.size;
			}
			if (TryGetComponent<SphereCollider>(out var component3))
			{
				component3.center = m_Mesh.bounds.center;
				Vector3 extents = m_Mesh.bounds.extents;
				component3.radius = Mathf.Max(extents.x, extents.y, extents.z);
			}
		}
		m_RebuildRequested = false;
	}

	private void AutosmoothNormals()
	{
		Vector3[] vertices = m_Mesh.vertices;
		int[] triangles = m_Mesh.triangles;
		Vector3[] array = new Vector3[vertices.Length];
		Dictionary<int, Vector3> dictionary = new Dictionary<int, Vector3>();
		Dictionary<int, List<int>> dictionary2 = new Dictionary<int, List<int>>();
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Vector3 vector = vertices[triangles[i]];
			Vector3 vector2 = vertices[triangles[i + 1]];
			Vector3 vector3 = vertices[triangles[i + 2]];
			Vector3 normalized = Vector3.Cross(vector2 - vector, vector3 - vector).normalized;
			int num = i / 3;
			dictionary[num] = normalized;
			for (int j = 0; j < 3; j++)
			{
				int key = triangles[i + j];
				if (!dictionary2.ContainsKey(key))
				{
					dictionary2[key] = new List<int>();
				}
				dictionary2[key].Add(num);
			}
		}
		foreach (KeyValuePair<int, List<int>> item in dictionary2)
		{
			int key2 = item.Key;
			List<int> value = item.Value;
			Vector3 zero = Vector3.zero;
			foreach (int item2 in value)
			{
				Vector3 vector4 = dictionary[item2];
				bool flag = true;
				foreach (int item3 in value)
				{
					if (item2 != item3)
					{
						Vector3 to = dictionary[item3];
						if (Vector3.Angle(vector4, to) > m_AutosmoothAngle)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					zero += vector4;
					continue;
				}
				array[key2] = vector4;
				break;
			}
			if (array[key2] == Vector3.zero)
			{
				array[key2] = zero.normalized;
			}
		}
		m_Mesh.normals = array;
	}

	internal Mesh CreateMeshAsset()
	{
		return new Mesh
		{
			name = base.name
		};
	}
}
