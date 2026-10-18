using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[AddComponentMenu("//ProBuilder MeshFilter")]
[RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
[ExcludeFromPreset]
[ExcludeFromObjectFactory]
public sealed class ProBuilderMesh : MonoBehaviour
{
	[Flags]
	private enum CacheValidState : byte
	{
		SharedVertex = 1,
		SharedTexture = 2
	}

	internal struct NonVersionedEditScope : IDisposable
	{
		private readonly ProBuilderMesh m_Mesh;

		private readonly ushort m_VersionIndex;

		public NonVersionedEditScope(ProBuilderMesh mesh)
		{
			m_Mesh = mesh;
			m_VersionIndex = mesh.versionIndex;
		}

		public void Dispose()
		{
			m_Mesh.m_VersionIndex = m_VersionIndex;
			m_Mesh.m_InstanceVersionIndex = m_VersionIndex;
		}
	}

	internal const HideFlags k_MeshFilterHideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;

	private const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/EditableMesh/EditableMesh.png";

	private const int k_UVChannelCount = 4;

	internal const int k_MeshFormatVersion = 2;

	internal const int k_MeshFormatVersionSubmeshMaterialRefactor = 1;

	internal const int k_MeshFormatVersionAutoUVScaleOffset = 2;

	public const uint maxVertexCount = 65535u;

	[SerializeField]
	private int m_MeshFormatVersion;

	[SerializeField]
	[FormerlySerializedAs("_quads")]
	private Face[] m_Faces;

	[SerializeField]
	[FormerlySerializedAs("_sharedIndices")]
	[FormerlySerializedAs("m_SharedVertexes")]
	private SharedVertex[] m_SharedVertices;

	[NonSerialized]
	private CacheValidState m_CacheValid;

	[NonSerialized]
	private Dictionary<int, int> m_SharedVertexLookup;

	[SerializeField]
	[FormerlySerializedAs("_sharedIndicesUV")]
	private SharedVertex[] m_SharedTextures;

	[NonSerialized]
	private Dictionary<int, int> m_SharedTextureLookup;

	[SerializeField]
	[FormerlySerializedAs("_vertices")]
	private Vector3[] m_Positions;

	[SerializeField]
	[FormerlySerializedAs("_uv")]
	private Vector2[] m_Textures0;

	[SerializeField]
	[FormerlySerializedAs("_uv3")]
	private List<Vector4> m_Textures2;

	[SerializeField]
	[FormerlySerializedAs("_uv4")]
	private List<Vector4> m_Textures3;

	[SerializeField]
	[FormerlySerializedAs("_tangents")]
	private Vector4[] m_Tangents;

	[NonSerialized]
	private Vector3[] m_Normals;

	[SerializeField]
	[FormerlySerializedAs("_colors")]
	private Color[] m_Colors;

	[FormerlySerializedAs("unwrapParameters")]
	[SerializeField]
	private UnwrapParameters m_UnwrapParameters;

	[FormerlySerializedAs("dontDestroyMeshOnDelete")]
	[SerializeField]
	private bool m_PreserveMeshAssetOnDestroy;

	[SerializeField]
	internal string assetGuid;

	[SerializeField]
	private Mesh m_Mesh;

	[NonSerialized]
	private MeshRenderer m_MeshRenderer;

	[NonSerialized]
	private MeshFilter m_MeshFilter;

	internal const ushort k_UnitializedVersionIndex = 0;

	[SerializeField]
	private ushort m_VersionIndex;

	[NonSerialized]
	private ushort m_InstanceVersionIndex;

	private static HashSet<int> s_CachedHashSet = new HashSet<int>();

	[SerializeField]
	private bool m_IsSelectable = true;

	[SerializeField]
	private int[] m_SelectedFaces = new int[0];

	[SerializeField]
	private Edge[] m_SelectedEdges = new Edge[0];

	[SerializeField]
	private int[] m_SelectedVertices = new int[0];

	private bool m_SelectedCacheDirty;

	private int m_SelectedSharedVerticesCount;

	private int m_SelectedCoincidentVertexCount;

	private HashSet<int> m_SelectedSharedVertices = new HashSet<int>();

	private List<int> m_SelectedCoincidentVertices = new List<int>();

	public bool userCollisions { get; set; }

	public UnwrapParameters unwrapParameters
	{
		get
		{
			return m_UnwrapParameters;
		}
		set
		{
			m_UnwrapParameters = value;
		}
	}

	internal MeshRenderer renderer
	{
		get
		{
			if (!base.gameObject.TryGetComponent<MeshRenderer>(out m_MeshRenderer))
			{
				return null;
			}
			return m_MeshRenderer;
		}
	}

	internal MeshFilter filter
	{
		get
		{
			if (m_MeshFilter == null && !base.gameObject.TryGetComponent<MeshFilter>(out m_MeshFilter))
			{
				return null;
			}
			return m_MeshFilter;
		}
	}

	internal ushort versionIndex => m_VersionIndex;

	internal ushort nonSerializedVersionIndex => m_InstanceVersionIndex;

	public bool preserveMeshAssetOnDestroy
	{
		get
		{
			return m_PreserveMeshAssetOnDestroy;
		}
		set
		{
			m_PreserveMeshAssetOnDestroy = value;
		}
	}

	internal Face[] facesInternal
	{
		get
		{
			return m_Faces;
		}
		set
		{
			m_Faces = value;
		}
	}

	public IList<Face> faces
	{
		get
		{
			return new ReadOnlyCollection<Face>(m_Faces);
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_Faces = value.ToArray();
		}
	}

	internal SharedVertex[] sharedVerticesInternal
	{
		get
		{
			return m_SharedVertices;
		}
		set
		{
			m_SharedVertices = value;
			InvalidateSharedVertexLookup();
		}
	}

	public IList<SharedVertex> sharedVertices
	{
		get
		{
			return new ReadOnlyCollection<SharedVertex>(m_SharedVertices);
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			int count = value.Count;
			m_SharedVertices = new SharedVertex[count];
			for (int i = 0; i < count; i++)
			{
				m_SharedVertices[i] = new SharedVertex(value[i]);
			}
			InvalidateSharedVertexLookup();
		}
	}

	internal Dictionary<int, int> sharedVertexLookup
	{
		get
		{
			if ((m_CacheValid & CacheValidState.SharedVertex) != CacheValidState.SharedVertex)
			{
				if (m_SharedVertexLookup == null)
				{
					m_SharedVertexLookup = new Dictionary<int, int>();
				}
				SharedVertex.GetSharedVertexLookup(m_SharedVertices, m_SharedVertexLookup);
				m_CacheValid |= CacheValidState.SharedVertex;
			}
			return m_SharedVertexLookup;
		}
	}

	internal SharedVertex[] sharedTextures
	{
		get
		{
			return m_SharedTextures;
		}
		set
		{
			m_SharedTextures = value;
			InvalidateSharedTextureLookup();
		}
	}

	internal Dictionary<int, int> sharedTextureLookup
	{
		get
		{
			if ((m_CacheValid & CacheValidState.SharedTexture) != CacheValidState.SharedTexture)
			{
				m_CacheValid |= CacheValidState.SharedTexture;
				if (m_SharedTextureLookup == null)
				{
					m_SharedTextureLookup = new Dictionary<int, int>();
				}
				SharedVertex.GetSharedVertexLookup(m_SharedTextures, m_SharedTextureLookup);
			}
			return m_SharedTextureLookup;
		}
	}

	internal Vector3[] positionsInternal
	{
		get
		{
			return m_Positions;
		}
		set
		{
			m_Positions = value;
		}
	}

	public IList<Vector3> positions
	{
		get
		{
			return new ReadOnlyCollection<Vector3>(m_Positions);
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_Positions = value.ToArray();
		}
	}

	public IList<Vector3> normals
	{
		get
		{
			if (m_Normals == null)
			{
				return null;
			}
			return new ReadOnlyCollection<Vector3>(m_Normals);
		}
	}

	internal Vector3[] normalsInternal
	{
		get
		{
			return m_Normals;
		}
		set
		{
			m_Normals = value;
		}
	}

	internal Color[] colorsInternal
	{
		get
		{
			return m_Colors;
		}
		set
		{
			m_Colors = value;
		}
	}

	public IList<Color> colors
	{
		get
		{
			if (m_Colors == null)
			{
				return null;
			}
			return new ReadOnlyCollection<Color>(m_Colors);
		}
		set
		{
			if (value == null || value.Count == 0)
			{
				m_Colors = null;
				return;
			}
			if (value.Count != vertexCount)
			{
				throw new ArgumentOutOfRangeException("value", "Array length must match vertex count.");
			}
			m_Colors = value.ToArray();
		}
	}

	public IList<Vector4> tangents
	{
		get
		{
			if (m_Tangents != null && m_Tangents.Length == vertexCount)
			{
				return new ReadOnlyCollection<Vector4>(m_Tangents);
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_Tangents = null;
				return;
			}
			if (value.Count != vertexCount)
			{
				throw new ArgumentOutOfRangeException("value", "Tangent array length must match vertex count");
			}
			m_Tangents = value.ToArray();
		}
	}

	internal Vector4[] tangentsInternal
	{
		get
		{
			return m_Tangents;
		}
		set
		{
			m_Tangents = value;
		}
	}

	internal Vector2[] texturesInternal
	{
		get
		{
			return m_Textures0;
		}
		set
		{
			m_Textures0 = value;
		}
	}

	internal List<Vector4> textures2Internal
	{
		get
		{
			return m_Textures2;
		}
		set
		{
			m_Textures2 = value;
		}
	}

	internal List<Vector4> textures3Internal
	{
		get
		{
			return m_Textures3;
		}
		set
		{
			m_Textures3 = value;
		}
	}

	public IList<Vector2> textures
	{
		get
		{
			if (m_Textures0 == null)
			{
				return null;
			}
			return new ReadOnlyCollection<Vector2>(m_Textures0);
		}
		set
		{
			if (value == null)
			{
				m_Textures0 = null;
				return;
			}
			if (value.Count != vertexCount)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			m_Textures0 = value.ToArray();
		}
	}

	public int faceCount
	{
		get
		{
			if (m_Faces != null)
			{
				return m_Faces.Length;
			}
			return 0;
		}
	}

	public int vertexCount
	{
		get
		{
			if (m_Positions != null)
			{
				return m_Positions.Length;
			}
			return 0;
		}
	}

	public int edgeCount
	{
		get
		{
			int num = 0;
			int i = 0;
			for (int num2 = faceCount; i < num2; i++)
			{
				num += facesInternal[i].edgesInternal.Length;
			}
			return num;
		}
	}

	public int indexCount
	{
		get
		{
			if (m_Faces != null)
			{
				return m_Faces.Sum((Face x) => x.indexesInternal.Length);
			}
			return 0;
		}
	}

	public int triangleCount
	{
		get
		{
			if (m_Faces != null)
			{
				return m_Faces.Sum((Face x) => x.indexesInternal.Length) / 3;
			}
			return 0;
		}
	}

	internal Mesh mesh
	{
		get
		{
			if (m_Mesh == null && filter != null)
			{
				m_Mesh = filter.sharedMesh;
			}
			return m_Mesh;
		}
		set
		{
			m_Mesh = value;
		}
	}

	[Obsolete("InstanceID is not used to track mesh references as of 2023/04/12")]
	internal int id => base.gameObject.GetInstanceID();

	public MeshSyncState meshSyncState
	{
		get
		{
			if (mesh == null)
			{
				return MeshSyncState.Null;
			}
			if (m_VersionIndex != m_InstanceVersionIndex && m_InstanceVersionIndex != 0)
			{
				return MeshSyncState.NeedsRebuild;
			}
			if (mesh.uv2 != null)
			{
				return MeshSyncState.InSync;
			}
			return MeshSyncState.Lightmap;
		}
	}

	internal int meshFormatVersion => m_MeshFormatVersion;

	public bool selectable
	{
		get
		{
			return m_IsSelectable;
		}
		set
		{
			m_IsSelectable = value;
		}
	}

	public int selectedFaceCount => m_SelectedFaces.Length;

	public int selectedVertexCount => m_SelectedVertices.Length;

	public int selectedEdgeCount => m_SelectedEdges.Length;

	internal int selectedSharedVerticesCount
	{
		get
		{
			CacheSelection();
			return m_SelectedSharedVerticesCount;
		}
	}

	internal int selectedCoincidentVertexCount
	{
		get
		{
			CacheSelection();
			return m_SelectedCoincidentVertexCount;
		}
	}

	internal IEnumerable<int> selectedSharedVertices
	{
		get
		{
			CacheSelection();
			return m_SelectedSharedVertices;
		}
	}

	internal IEnumerable<int> selectedCoincidentVertices
	{
		get
		{
			CacheSelection();
			return m_SelectedCoincidentVertices;
		}
	}

	public ReadOnlyCollection<int> selectedFaceIndexes => new ReadOnlyCollection<int>(m_SelectedFaces);

	public ReadOnlyCollection<int> selectedVertices => new ReadOnlyCollection<int>(m_SelectedVertices);

	public ReadOnlyCollection<Edge> selectedEdges => new ReadOnlyCollection<Edge>(m_SelectedEdges);

	internal Face[] selectedFacesInternal
	{
		get
		{
			return GetSelectedFaces();
		}
		set
		{
			m_SelectedFaces = value.Select((Face x) => Array.IndexOf(m_Faces, x)).ToArray();
		}
	}

	internal int[] selectedFaceIndicesInternal
	{
		get
		{
			return m_SelectedFaces;
		}
		set
		{
			m_SelectedFaces = value;
		}
	}

	internal Edge[] selectedEdgesInternal
	{
		get
		{
			return m_SelectedEdges;
		}
		set
		{
			m_SelectedEdges = value;
		}
	}

	internal int[] selectedIndexesInternal
	{
		get
		{
			return m_SelectedVertices;
		}
		set
		{
			m_SelectedVertices = value;
		}
	}

	public static event Action<ProBuilderMesh> meshWillBeDestroyed;

	internal static event Action<ProBuilderMesh> meshWasInitialized;

	internal static event Action<ProBuilderMesh> componentWillBeDestroyed;

	internal static event Action<ProBuilderMesh> componentHasBeenReset;

	public static event Action<ProBuilderMesh> elementSelectionChanged;

	public bool HasArrays(MeshArrays channels)
	{
		bool flag = false;
		int num = vertexCount;
		flag |= (channels & MeshArrays.Position) == MeshArrays.Position && m_Positions == null;
		flag |= (channels & MeshArrays.Normal) == MeshArrays.Normal && (m_Normals == null || m_Normals.Length != num);
		flag |= (channels & MeshArrays.Texture0) == MeshArrays.Texture0 && (m_Textures0 == null || m_Textures0.Length != num);
		flag |= (channels & MeshArrays.Texture2) == MeshArrays.Texture2 && (m_Textures2 == null || m_Textures2.Count != num);
		flag |= (channels & MeshArrays.Texture3) == MeshArrays.Texture3 && (m_Textures3 == null || m_Textures3.Count != num);
		flag |= (channels & MeshArrays.Color) == MeshArrays.Color && (m_Colors == null || m_Colors.Length != num);
		flag |= (channels & MeshArrays.Tangent) == MeshArrays.Tangent && (m_Tangents == null || m_Tangents.Length != num);
		if ((channels & MeshArrays.Texture1) == MeshArrays.Texture1 && mesh != null)
		{
			flag |= !mesh.HasVertexAttribute(VertexAttribute.TexCoord1);
		}
		return !flag;
	}

	internal void InvalidateSharedVertexLookup()
	{
		if (m_SharedVertexLookup == null)
		{
			m_SharedVertexLookup = new Dictionary<int, int>();
		}
		m_SharedVertexLookup.Clear();
		m_CacheValid &= ~CacheValidState.SharedVertex;
	}

	internal void InvalidateSharedTextureLookup()
	{
		if (m_SharedTextureLookup == null)
		{
			m_SharedTextureLookup = new Dictionary<int, int>();
		}
		m_SharedTextureLookup.Clear();
		m_CacheValid &= ~CacheValidState.SharedTexture;
	}

	internal void InvalidateFaces()
	{
		if (m_Faces == null)
		{
			m_Faces = new Face[0];
			return;
		}
		foreach (Face face in faces)
		{
			face.InvalidateCache();
		}
	}

	internal void InvalidateCaches()
	{
		InvalidateSharedVertexLookup();
		InvalidateSharedTextureLookup();
		InvalidateFaces();
		m_SelectedCacheDirty = true;
	}

	internal void SetSharedVertices(IEnumerable<KeyValuePair<int, int>> indexes)
	{
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		m_SharedVertices = SharedVertex.ToSharedVertices(indexes);
		InvalidateSharedVertexLookup();
	}

	internal void SetSharedTextures(IEnumerable<KeyValuePair<int, int>> indexes)
	{
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		m_SharedTextures = SharedVertex.ToSharedVertices(indexes);
		InvalidateSharedTextureLookup();
	}

	public Vertex[] GetVertices(IList<int> indexes = null)
	{
		int num = vertexCount;
		int num2 = indexes?.Count ?? vertexCount;
		Vertex[] array = new Vertex[num2];
		Vector3[] array2 = positionsInternal;
		Color[] array3 = colorsInternal;
		Vector2[] array4 = texturesInternal;
		Vector4[] array5 = GetTangents();
		Vector3[] array6 = GetNormals();
		Vector2[] array7 = ((mesh != null) ? mesh.uv2 : null);
		List<Vector4> list = new List<Vector4>();
		List<Vector4> list2 = new List<Vector4>();
		GetUVs(2, list);
		GetUVs(3, list2);
		bool flag = array2 != null && array2.Length == num;
		bool flag2 = array3 != null && array3.Length == num;
		bool flag3 = array6 != null && array6.Length == num;
		bool flag4 = array5 != null && array5.Length == num;
		bool flag5 = array4 != null && array4.Length == num;
		bool flag6 = array7 != null && array7.Length == num;
		bool flag7 = list.Count == num;
		bool flag8 = list2.Count == num;
		for (int i = 0; i < num2; i++)
		{
			array[i] = new Vertex();
			int num3 = indexes?[i] ?? i;
			if (flag)
			{
				array[i].position = array2[num3];
			}
			if (flag2)
			{
				array[i].color = array3[num3];
			}
			if (flag3)
			{
				array[i].normal = array6[num3];
			}
			if (flag4)
			{
				array[i].tangent = array5[num3];
			}
			if (flag5)
			{
				array[i].uv0 = array4[num3];
			}
			if (flag6)
			{
				array[i].uv2 = array7[num3];
			}
			if (flag7)
			{
				array[i].uv3 = list[num3];
			}
			if (flag8)
			{
				array[i].uv4 = list2[num3];
			}
		}
		return array;
	}

	internal void GetVerticesInList(IList<Vertex> vertices)
	{
		int num = vertexCount;
		vertices.Clear();
		Vector3[] array = positionsInternal;
		Color[] array2 = colorsInternal;
		Vector2[] array3 = texturesInternal;
		Vector4[] array4 = GetTangents();
		Vector3[] array5 = GetNormals();
		Vector2[] array6 = ((mesh != null) ? mesh.uv2 : null);
		List<Vector4> list = new List<Vector4>();
		List<Vector4> list2 = new List<Vector4>();
		GetUVs(2, list);
		GetUVs(3, list2);
		bool flag = array != null && array.Length == num;
		bool flag2 = array2 != null && array2.Length == num;
		bool flag3 = array5 != null && array5.Length == num;
		bool flag4 = array4 != null && array4.Length == num;
		bool flag5 = array3 != null && array3.Length == num;
		bool flag6 = array6 != null && array6.Length == num;
		bool flag7 = list.Count == num;
		bool flag8 = list2.Count == num;
		for (int i = 0; i < num; i++)
		{
			vertices.Add(new Vertex());
			if (flag)
			{
				vertices[i].position = array[i];
			}
			if (flag2)
			{
				vertices[i].color = array2[i];
			}
			if (flag3)
			{
				vertices[i].normal = array5[i];
			}
			if (flag4)
			{
				vertices[i].tangent = array4[i];
			}
			if (flag5)
			{
				vertices[i].uv0 = array3[i];
			}
			if (flag6)
			{
				vertices[i].uv2 = array6[i];
			}
			if (flag7)
			{
				vertices[i].uv3 = list[i];
			}
			if (flag8)
			{
				vertices[i].uv4 = list2[i];
			}
		}
	}

	public void SetVertices(IList<Vertex> vertices, bool applyMesh = false)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		Vertex vertex = vertices.FirstOrDefault();
		if (vertex == null || !vertex.HasArrays(MeshArrays.Position))
		{
			Clear();
			return;
		}
		Vertex.GetArrays(vertices, out var position, out var color, out var uv, out var normal, out var tangent, out var uv2, out var uv3, out var uv4);
		m_Positions = position;
		m_Colors = color;
		m_Normals = normal;
		m_Tangents = tangent;
		m_Textures0 = uv;
		m_Textures2 = uv3;
		m_Textures3 = uv4;
		if (applyMesh)
		{
			Mesh mesh = this.mesh;
			if (vertex.HasArrays(MeshArrays.Position))
			{
				mesh.vertices = position;
			}
			if (vertex.HasArrays(MeshArrays.Color))
			{
				mesh.colors = color;
			}
			if (vertex.HasArrays(MeshArrays.Texture0))
			{
				mesh.uv = uv;
			}
			if (vertex.HasArrays(MeshArrays.Normal))
			{
				mesh.normals = normal;
			}
			if (vertex.HasArrays(MeshArrays.Tangent))
			{
				mesh.tangents = tangent;
			}
			if (vertex.HasArrays(MeshArrays.Texture1))
			{
				mesh.uv2 = uv2;
			}
			if (vertex.HasArrays(MeshArrays.Texture2))
			{
				mesh.SetUVs(2, uv3);
			}
			if (vertex.HasArrays(MeshArrays.Texture3))
			{
				mesh.SetUVs(3, uv4);
			}
			IncrementVersionIndex();
		}
	}

	public Vector3[] GetNormals()
	{
		if (!HasArrays(MeshArrays.Normal))
		{
			Normals.CalculateNormals(this);
		}
		return normals.ToArray();
	}

	public Color[] GetColors()
	{
		if (HasArrays(MeshArrays.Color))
		{
			return colors.ToArray();
		}
		return ArrayUtility.Fill(Color.white, vertexCount);
	}

	public Vector4[] GetTangents()
	{
		if (!HasArrays(MeshArrays.Tangent))
		{
			Normals.CalculateTangents(this);
		}
		return tangents.ToArray();
	}

	public void GetUVs(int channel, List<Vector4> uvs)
	{
		if (uvs == null)
		{
			throw new ArgumentNullException("uvs");
		}
		if (channel < 0 || channel > 3)
		{
			throw new ArgumentOutOfRangeException("channel");
		}
		uvs.Clear();
		switch (channel)
		{
		case 0:
		{
			for (int j = 0; j < vertexCount; j++)
			{
				uvs.Add(m_Textures0[j]);
			}
			break;
		}
		case 1:
			if (mesh != null && mesh.uv2 != null)
			{
				Vector2[] uv = mesh.uv2;
				for (int i = 0; i < uv.Length; i++)
				{
					uvs.Add(uv[i]);
				}
			}
			break;
		case 2:
			if (m_Textures2 != null)
			{
				uvs.AddRange(m_Textures2);
			}
			break;
		case 3:
			if (m_Textures3 != null)
			{
				uvs.AddRange(m_Textures3);
			}
			break;
		}
	}

	internal ReadOnlyCollection<Vector2> GetUVs(int channel)
	{
		switch (channel)
		{
		case 0:
			return new ReadOnlyCollection<Vector2>(m_Textures0);
		case 1:
			return new ReadOnlyCollection<Vector2>(mesh.uv2);
		case 2:
			if (m_Textures2 != null)
			{
				return new ReadOnlyCollection<Vector2>(m_Textures2.Cast<Vector2>().ToList());
			}
			return null;
		case 3:
			if (m_Textures3 != null)
			{
				return new ReadOnlyCollection<Vector2>(m_Textures3.Cast<Vector2>().ToList());
			}
			return null;
		default:
			return null;
		}
	}

	public void SetUVs(int channel, List<Vector4> uvs)
	{
		switch (channel)
		{
		case 0:
			m_Textures0 = uvs?.Select((Func<Vector4, Vector2>)((Vector4 x) => x)).ToArray();
			break;
		case 1:
			mesh.uv2 = uvs?.Select((Func<Vector4, Vector2>)((Vector4 x) => x)).ToArray();
			break;
		case 2:
			m_Textures2 = ((uvs != null) ? new List<Vector4>(uvs) : null);
			break;
		case 3:
			m_Textures3 = ((uvs != null) ? new List<Vector4>(uvs) : null);
			break;
		}
	}

	private void Awake()
	{
		EnsureMeshFilterIsAssigned();
		EnsureMeshColliderIsAssigned();
		ClearSelection();
		if (vertexCount > 0 && faceCount > 0 && meshSyncState == MeshSyncState.Null)
		{
			using (new NonVersionedEditScope(this))
			{
				Rebuild();
				ProBuilderMesh.meshWasInitialized?.Invoke(this);
			}
		}
	}

	private void Reset()
	{
		if (meshSyncState != 0)
		{
			Rebuild();
			if (ProBuilderMesh.componentHasBeenReset != null)
			{
				ProBuilderMesh.componentHasBeenReset(this);
			}
		}
	}

	private void OnDestroy()
	{
		if (m_MeshFilter != null || TryGetComponent<MeshFilter>(out m_MeshFilter))
		{
			m_MeshFilter.hideFlags = HideFlags.None;
		}
		if (ProBuilderMesh.componentWillBeDestroyed != null)
		{
			ProBuilderMesh.componentWillBeDestroyed(this);
		}
		if (!preserveMeshAssetOnDestroy && Application.isEditor && !Application.isPlaying && Time.frameCount > 0)
		{
			DestroyUnityMesh();
		}
	}

	internal void DestroyUnityMesh()
	{
		if (ProBuilderMesh.meshWillBeDestroyed != null)
		{
			ProBuilderMesh.meshWillBeDestroyed(this);
		}
		else
		{
			Object.DestroyImmediate(base.gameObject.GetComponent<MeshFilter>().sharedMesh, allowDestroyingAssets: true);
		}
	}

	private void IncrementVersionIndex()
	{
		if (++m_VersionIndex == 0)
		{
			m_VersionIndex = 1;
		}
		m_InstanceVersionIndex = m_VersionIndex;
	}

	public void Clear()
	{
		m_Faces = new Face[0];
		m_Positions = new Vector3[0];
		m_Textures0 = new Vector2[0];
		m_Textures2 = null;
		m_Textures3 = null;
		m_Tangents = null;
		m_SharedVertices = new SharedVertex[0];
		m_SharedTextures = new SharedVertex[0];
		InvalidateSharedVertexLookup();
		InvalidateSharedTextureLookup();
		m_Colors = null;
		m_MeshFormatVersion = 2;
		IncrementVersionIndex();
		ClearSelection();
	}

	internal void EnsureMeshFilterIsAssigned()
	{
		if (filter == null)
		{
			m_MeshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		if (!renderer.isPartOfStaticBatch && filter.sharedMesh != m_Mesh)
		{
			filter.sharedMesh = m_Mesh;
		}
	}

	internal static ProBuilderMesh CreateInstanceWithPoints(Vector3[] positions)
	{
		if (positions.Length % 4 != 0)
		{
			Log.Warning("Invalid Geometry. Make sure vertices in are pairs of 4 (faces).");
			return null;
		}
		ProBuilderMesh proBuilderMesh = new GameObject
		{
			name = "ProBuilder Mesh"
		}.AddComponent<ProBuilderMesh>();
		proBuilderMesh.m_MeshFormatVersion = 2;
		proBuilderMesh.GeometryWithPoints(positions);
		return proBuilderMesh;
	}

	public static ProBuilderMesh Create()
	{
		ProBuilderMesh proBuilderMesh = new GameObject().AddComponent<ProBuilderMesh>();
		proBuilderMesh.m_MeshFormatVersion = 2;
		proBuilderMesh.Clear();
		return proBuilderMesh;
	}

	public static ProBuilderMesh Create(IEnumerable<Vector3> positions, IEnumerable<Face> faces)
	{
		GameObject obj = new GameObject();
		ProBuilderMesh proBuilderMesh = obj.AddComponent<ProBuilderMesh>();
		obj.name = "ProBuilder Mesh";
		proBuilderMesh.m_MeshFormatVersion = 2;
		proBuilderMesh.RebuildWithPositionsAndFaces(positions, faces);
		return proBuilderMesh;
	}

	public static ProBuilderMesh Create(IList<Vertex> vertices, IList<Face> faces, IList<SharedVertex> sharedVertices = null, IList<SharedVertex> sharedTextures = null, IList<Material> materials = null)
	{
		ProBuilderMesh proBuilderMesh = new GameObject
		{
			name = "ProBuilder Mesh"
		}.AddComponent<ProBuilderMesh>();
		if (materials != null)
		{
			proBuilderMesh.renderer.sharedMaterials = materials.ToArray();
		}
		proBuilderMesh.m_MeshFormatVersion = 2;
		proBuilderMesh.SetVertices(vertices);
		proBuilderMesh.faces = faces;
		proBuilderMesh.sharedVertices = sharedVertices;
		proBuilderMesh.sharedTextures = sharedTextures?.ToArray();
		proBuilderMesh.ToMesh();
		proBuilderMesh.Refresh();
		return proBuilderMesh;
	}

	internal void GeometryWithPoints(Vector3[] points)
	{
		Face[] array = new Face[points.Length / 4];
		for (int i = 0; i < points.Length; i += 4)
		{
			array[i / 4] = new Face(new int[6]
			{
				i,
				i + 1,
				i + 2,
				i + 1,
				i + 3,
				i + 2
			}, 0, AutoUnwrapSettings.tile, 0, -1, -1, manualUVs: false);
		}
		Clear();
		positions = points;
		m_Faces = array;
		m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(points);
		InvalidateCaches();
		ToMesh();
		Refresh();
	}

	public void RebuildWithPositionsAndFaces(IEnumerable<Vector3> vertices, IEnumerable<Face> faces)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		Clear();
		m_Positions = vertices.ToArray();
		m_Faces = faces.ToArray();
		m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(m_Positions);
		InvalidateSharedVertexLookup();
		InvalidateSharedTextureLookup();
		ToMesh();
		Refresh();
	}

	internal void Rebuild()
	{
		ToMesh();
		Refresh();
	}

	public void ToMesh(MeshTopology preferredTopology = MeshTopology.Triangles)
	{
		bool usedInParticleSystem = false;
		if (mesh == null)
		{
			mesh = new Mesh
			{
				name = $"pb_Mesh{GetInstanceID()}"
			};
		}
		else if (mesh.vertexCount != vertexCount)
		{
			usedInParticleSystem = MeshUtility.IsUsedInParticleSystem(this);
			mesh.Clear();
		}
		mesh.indexFormat = ((vertexCount > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		mesh.vertices = m_Positions;
		mesh.uv2 = null;
		if (m_MeshFormatVersion < 2)
		{
			if (m_MeshFormatVersion < 1)
			{
				Submesh.MapFaceMaterialsToSubmeshIndex(this);
			}
			if (m_MeshFormatVersion < 2)
			{
				UvUnwrapping.UpgradeAutoUVScaleOffset(this);
			}
			m_MeshFormatVersion = 2;
		}
		m_MeshFormatVersion = 2;
		int materialCount = MaterialUtility.GetMaterialCount(renderer);
		Submesh[] submeshes = Submesh.GetSubmeshes(facesInternal, materialCount, preferredTopology);
		mesh.subMeshCount = submeshes.Length;
		if (mesh.subMeshCount == 0)
		{
			FinalizeToMesh(usedInParticleSystem);
			return;
		}
		int num = 0;
		bool flag = false;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			if (submeshes[i].m_Indexes.Length == 0)
			{
				if (!flag)
				{
					MaterialUtility.s_MaterialArray.Clear();
					renderer.GetSharedMaterials(MaterialUtility.s_MaterialArray);
					flag = true;
				}
				submeshes[i].submeshIndex = -1;
				MaterialUtility.s_MaterialArray.RemoveAt(num);
				Face[] array = facesInternal;
				foreach (Face face in array)
				{
					if (num < face.submeshIndex)
					{
						face.submeshIndex--;
					}
				}
				continue;
			}
			submeshes[i].submeshIndex = num;
			int num2 = 0;
			int[] indexes = submeshes[i].m_Indexes;
			if (submeshes[i].m_Topology == MeshTopology.Triangles && indexes.Length % 3 == 0)
			{
				for (int k = 0; k < indexes.Length; k += 3)
				{
					if (k + 2 < indexes.Length && indexes[k] < positions.Count && indexes[k + 1] < positions.Count && indexes[k + 2] < positions.Count)
					{
						Vector3 lhs = positions[indexes[k + 1]] - positions[indexes[k]];
						Vector3 rhs = positions[indexes[k + 2]] - positions[indexes[k]];
						if (Vector3.Cross(lhs, rhs).sqrMagnitude < Mathf.Epsilon)
						{
							num2 += 3;
							continue;
						}
						indexes[k - num2] = indexes[k];
						indexes[k - num2 + 1] = indexes[k + 1];
						indexes[k - num2 + 2] = indexes[k + 2];
					}
				}
			}
			int[] array2;
			if (num2 > 0)
			{
				array2 = new int[indexes.Length - num2];
				Array.Copy(indexes, 0, array2, 0, array2.Length);
			}
			else
			{
				array2 = submeshes[i].m_Indexes;
			}
			mesh.SetIndices(array2, submeshes[i].m_Topology, submeshes[i].submeshIndex, calculateBounds: false);
			num++;
		}
		if (mesh.subMeshCount < materialCount)
		{
			int num3 = materialCount - mesh.subMeshCount;
			int index = MaterialUtility.s_MaterialArray.Count - num3;
			MaterialUtility.s_MaterialArray.RemoveRange(index, num3);
			flag = true;
		}
		if (flag)
		{
			renderer.sharedMaterials = MaterialUtility.s_MaterialArray.ToArray();
		}
		FinalizeToMesh(usedInParticleSystem);
	}

	private void FinalizeToMesh(bool usedInParticleSystem)
	{
		EnsureMeshFilterIsAssigned();
		if (usedInParticleSystem)
		{
			MeshUtility.RestoreParticleSystem(this);
		}
		IncrementVersionIndex();
	}

	public void MakeUnique()
	{
		mesh = ((mesh != null) ? Object.Instantiate(mesh) : new Mesh
		{
			name = $"pb_Mesh{GetInstanceID()}"
		});
		if (meshSyncState == MeshSyncState.InSync)
		{
			filter.mesh = mesh;
			return;
		}
		ToMesh();
		Refresh();
	}

	public void CopyFrom(ProBuilderMesh other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		Clear();
		positions = other.positions;
		sharedVertices = other.sharedVerticesInternal;
		SetSharedTextures(other.sharedTextureLookup);
		facesInternal = other.faces.Select((Face x) => new Face(x)).ToArray();
		List<Vector4> uvs = new List<Vector4>();
		for (int i = 0; i < 4; i++)
		{
			other.GetUVs(i, uvs);
			SetUVs(i, uvs);
		}
		tangents = other.tangents;
		colors = other.colors;
		userCollisions = other.userCollisions;
		selectable = other.selectable;
		unwrapParameters = new UnwrapParameters(other.unwrapParameters);
	}

	public void Refresh(RefreshMask mask = RefreshMask.All)
	{
		if ((mask & RefreshMask.UV) > (RefreshMask)0)
		{
			RefreshUV(facesInternal);
		}
		if ((mask & RefreshMask.Colors) > (RefreshMask)0)
		{
			RefreshColors();
		}
		if ((mask & RefreshMask.Normals) > (RefreshMask)0)
		{
			RefreshNormals();
		}
		if ((mask & RefreshMask.Tangents) > (RefreshMask)0)
		{
			RefreshTangents();
		}
		if ((mask & RefreshMask.Collisions) > (RefreshMask)0)
		{
			EnsureMeshColliderIsAssigned();
		}
		if ((mask & RefreshMask.Bounds) > (RefreshMask)0 && mesh != null)
		{
			mesh.RecalculateBounds();
		}
		IncrementVersionIndex();
	}

	internal void EnsureMeshColliderIsAssigned()
	{
		if (base.gameObject.TryGetComponent<MeshCollider>(out var component))
		{
			component.sharedMesh = ((mesh != null && mesh.vertexCount > 0) ? mesh : null);
		}
	}

	internal int GetUnusedTextureGroup(int i = 1)
	{
		while (Array.Exists(facesInternal, (Face element) => element.textureGroup == i))
		{
			i++;
		}
		return i;
	}

	private static bool IsValidTextureGroup(int group)
	{
		return group > 0;
	}

	internal int UnusedElementGroup(int i = 1)
	{
		while (Array.Exists(facesInternal, (Face element) => element.elementGroup == i))
		{
			i++;
		}
		return i;
	}

	public void RefreshUV(IEnumerable<Face> facesToRefresh)
	{
		if (!HasArrays(MeshArrays.Texture0))
		{
			m_Textures0 = new Vector2[vertexCount];
			Face[] array = facesInternal;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].manualUV = false;
			}
			facesToRefresh = facesInternal;
		}
		s_CachedHashSet.Clear();
		foreach (Face item in facesToRefresh)
		{
			if (item.manualUV)
			{
				continue;
			}
			int[] indexesInternal = item.indexesInternal;
			if (indexesInternal == null || indexesInternal.Length >= 3)
			{
				int textureGroup = item.textureGroup;
				if (!IsValidTextureGroup(textureGroup))
				{
					UvUnwrapping.Unwrap(this, item);
				}
				else if (s_CachedHashSet.Add(textureGroup))
				{
					UvUnwrapping.ProjectTextureGroup(this, textureGroup, item.uv);
				}
			}
		}
		mesh.uv = m_Textures0;
		if (HasArrays(MeshArrays.Texture2))
		{
			mesh.SetUVs(2, m_Textures2);
		}
		if (HasArrays(MeshArrays.Texture3))
		{
			mesh.SetUVs(3, m_Textures3);
		}
		IncrementVersionIndex();
	}

	internal void SetGroupUV(AutoUnwrapSettings settings, int group)
	{
		if (!IsValidTextureGroup(group))
		{
			return;
		}
		Face[] array = facesInternal;
		foreach (Face face in array)
		{
			if (face.textureGroup == group)
			{
				face.uv = settings;
			}
		}
	}

	private void RefreshColors()
	{
		filter.sharedMesh.colors = m_Colors;
	}

	public void SetFaceColor(Face face, Color color)
	{
		if (face == null)
		{
			throw new ArgumentNullException("face");
		}
		if (!HasArrays(MeshArrays.Color))
		{
			m_Colors = ArrayUtility.Fill(Color.white, vertexCount);
		}
		foreach (int distinctIndex in face.distinctIndexes)
		{
			m_Colors[distinctIndex] = color;
		}
	}

	public void SetMaterial(IEnumerable<Face> faces, Material material)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		int num = sharedMaterials.Length;
		int num2 = -1;
		for (int i = 0; i < num; i++)
		{
			if (num2 >= 0)
			{
				break;
			}
			if (sharedMaterials[i] == material)
			{
				num2 = i;
			}
		}
		if (num2 < 0)
		{
			bool[] array = new bool[num];
			Face[] array2 = m_Faces;
			foreach (Face face in array2)
			{
				array[Math.Clamp(face.submeshIndex, 0, num - 1)] = true;
			}
			num2 = Array.IndexOf(array, value: false);
			if (num2 > -1)
			{
				sharedMaterials[num2] = material;
				renderer.sharedMaterials = sharedMaterials;
			}
			else
			{
				num2 = sharedMaterials.Length;
				Material[] array3 = new Material[num2 + 1];
				Array.Copy(sharedMaterials, array3, num2);
				array3[num2] = material;
				renderer.sharedMaterials = array3;
			}
		}
		foreach (Face face2 in faces)
		{
			face2.submeshIndex = num2;
		}
		IncrementVersionIndex();
	}

	private void RefreshNormals()
	{
		Normals.CalculateNormals(this);
		mesh.normals = m_Normals;
	}

	private void RefreshTangents()
	{
		Normals.CalculateTangents(this);
		mesh.tangents = m_Tangents;
	}

	internal int GetSharedVertexHandle(int vertex)
	{
		if (m_SharedVertexLookup.TryGetValue(vertex, out var value))
		{
			return value;
		}
		for (int i = 0; i < m_SharedVertices.Length; i++)
		{
			int j = 0;
			for (int count = m_SharedVertices[i].Count; j < count; j++)
			{
				if (m_SharedVertices[i][j] == vertex)
				{
					return i;
				}
			}
		}
		throw new ArgumentOutOfRangeException("vertex");
	}

	internal HashSet<int> GetSharedVertexHandles(IEnumerable<int> vertices)
	{
		Dictionary<int, int> dictionary = sharedVertexLookup;
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int vertex in vertices)
		{
			hashSet.Add(dictionary[vertex]);
		}
		return hashSet;
	}

	public List<int> GetCoincidentVertices(IEnumerable<int> vertices)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		List<int> list = new List<int>();
		GetCoincidentVertices(vertices, list);
		return list;
	}

	public void GetCoincidentVertices(IEnumerable<Face> faces, List<int> coincident)
	{
		if (faces == null)
		{
			throw new ArgumentNullException("faces");
		}
		if (coincident == null)
		{
			throw new ArgumentNullException("coincident");
		}
		coincident.Clear();
		s_CachedHashSet.Clear();
		Dictionary<int, int> dictionary = sharedVertexLookup;
		foreach (Face face in faces)
		{
			int[] distinctIndexesInternal = face.distinctIndexesInternal;
			foreach (int key in distinctIndexesInternal)
			{
				int num = dictionary[key];
				if (s_CachedHashSet.Add(num))
				{
					SharedVertex sharedVertex = m_SharedVertices[num];
					int j = 0;
					for (int count = sharedVertex.Count; j < count; j++)
					{
						coincident.Add(sharedVertex[j]);
					}
				}
			}
		}
	}

	public void GetCoincidentVertices(IEnumerable<Edge> edges, List<int> coincident)
	{
		if (faces == null)
		{
			throw new ArgumentNullException("edges");
		}
		if (coincident == null)
		{
			throw new ArgumentNullException("coincident");
		}
		coincident.Clear();
		s_CachedHashSet.Clear();
		Dictionary<int, int> dictionary = sharedVertexLookup;
		foreach (Edge edge in edges)
		{
			int num = dictionary[edge.a];
			if (s_CachedHashSet.Add(num))
			{
				SharedVertex sharedVertex = m_SharedVertices[num];
				int i = 0;
				for (int count = sharedVertex.Count; i < count; i++)
				{
					coincident.Add(sharedVertex[i]);
				}
			}
			num = dictionary[edge.b];
			if (s_CachedHashSet.Add(num))
			{
				SharedVertex sharedVertex2 = m_SharedVertices[num];
				int j = 0;
				for (int count2 = sharedVertex2.Count; j < count2; j++)
				{
					coincident.Add(sharedVertex2[j]);
				}
			}
		}
	}

	public void GetCoincidentVertices(IEnumerable<int> vertices, List<int> coincident)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		if (coincident == null)
		{
			throw new ArgumentNullException("coincident");
		}
		coincident.Clear();
		s_CachedHashSet.Clear();
		Dictionary<int, int> dictionary = sharedVertexLookup;
		foreach (int vertex in vertices)
		{
			int num = dictionary[vertex];
			if (s_CachedHashSet.Add(num))
			{
				SharedVertex sharedVertex = m_SharedVertices[num];
				int i = 0;
				for (int count = sharedVertex.Count; i < count; i++)
				{
					coincident.Add(sharedVertex[i]);
				}
			}
		}
	}

	public void GetCoincidentVertices(int vertex, List<int> coincident)
	{
		if (coincident == null)
		{
			throw new ArgumentNullException("coincident");
		}
		if (!sharedVertexLookup.TryGetValue(vertex, out var value))
		{
			throw new ArgumentOutOfRangeException("vertex");
		}
		SharedVertex sharedVertex = m_SharedVertices[value];
		int i = 0;
		for (int count = sharedVertex.Count; i < count; i++)
		{
			coincident.Add(sharedVertex[i]);
		}
	}

	public void SetVerticesCoincident(IEnumerable<int> vertices)
	{
		Dictionary<int, int> lookup = sharedVertexLookup;
		List<int> list = new List<int>();
		GetCoincidentVertices(vertices, list);
		SharedVertex.SetCoincident(ref lookup, list);
		SetSharedVertices(lookup);
	}

	internal void SetTexturesCoincident(IEnumerable<int> vertices)
	{
		Dictionary<int, int> lookup = sharedTextureLookup;
		SharedVertex.SetCoincident(ref lookup, vertices);
		SetSharedTextures(lookup);
	}

	internal void AddToSharedVertex(int sharedVertexHandle, int vertex)
	{
		if (sharedVertexHandle < 0 || sharedVertexHandle >= m_SharedVertices.Length)
		{
			throw new ArgumentOutOfRangeException("sharedVertexHandle");
		}
		m_SharedVertices[sharedVertexHandle].Add(vertex);
		InvalidateSharedVertexLookup();
	}

	internal void AddSharedVertex(SharedVertex vertex)
	{
		if (vertex == null)
		{
			throw new ArgumentNullException("vertex");
		}
		m_SharedVertices = m_SharedVertices.Add(vertex);
		InvalidateSharedVertexLookup();
	}

	private void CacheSelection()
	{
		if (!m_SelectedCacheDirty)
		{
			return;
		}
		m_SelectedCacheDirty = false;
		m_SelectedSharedVertices.Clear();
		m_SelectedCoincidentVertices.Clear();
		Dictionary<int, int> dictionary = sharedVertexLookup;
		m_SelectedSharedVerticesCount = 0;
		m_SelectedCoincidentVertexCount = 0;
		try
		{
			int[] array = m_SelectedVertices;
			foreach (int key in array)
			{
				if (m_SelectedSharedVertices.Add(dictionary[key]))
				{
					SharedVertex sharedVertex = sharedVerticesInternal[dictionary[key]];
					m_SelectedSharedVerticesCount++;
					m_SelectedCoincidentVertexCount += sharedVertex.Count;
					m_SelectedCoincidentVertices.AddRange(sharedVertex);
				}
			}
		}
		catch
		{
			ClearSelection();
		}
	}

	public Face[] GetSelectedFaces()
	{
		int num = m_SelectedFaces.Length;
		Face[] array = new Face[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = m_Faces[m_SelectedFaces[i]];
		}
		return array;
	}

	internal Face GetActiveFace()
	{
		if (selectedFaceCount < 1)
		{
			return null;
		}
		return m_Faces[selectedFaceIndicesInternal[selectedFaceCount - 1]];
	}

	internal Edge GetActiveEdge()
	{
		if (selectedEdgeCount < 1)
		{
			return Edge.Empty;
		}
		return m_SelectedEdges[selectedEdgeCount - 1];
	}

	internal int GetActiveVertex()
	{
		if (selectedVertexCount < 1)
		{
			return -1;
		}
		return m_SelectedVertices[selectedVertexCount - 1];
	}

	internal void AddToFaceSelection(int index)
	{
		if (index > -1)
		{
			SetSelectedFaces(m_SelectedFaces.Add(index));
		}
	}

	public void SetSelectedFaces(IEnumerable<Face> selected)
	{
		SetSelectedFaces(selected?.Select((Face x) => Array.IndexOf(facesInternal, x)));
	}

	internal void SetSelectedFaces(IEnumerable<int> selected)
	{
		if (selected == null)
		{
			ClearSelection();
		}
		else
		{
			m_SelectedFaces = selected.ToArray();
			m_SelectedVertices = m_SelectedFaces.SelectMany((int x) => facesInternal[x].distinctIndexesInternal).ToArray();
			m_SelectedEdges = m_SelectedFaces.SelectMany((int x) => facesInternal[x].edges).ToArray();
		}
		m_SelectedCacheDirty = true;
		if (ProBuilderMesh.elementSelectionChanged != null)
		{
			ProBuilderMesh.elementSelectionChanged(this);
		}
	}

	public void SetSelectedEdges(IEnumerable<Edge> edges)
	{
		if (edges == null)
		{
			ClearSelection();
		}
		else
		{
			m_SelectedFaces = new int[0];
			m_SelectedEdges = edges.ToArray();
			m_SelectedVertices = m_SelectedEdges.AllTriangles();
		}
		m_SelectedCacheDirty = true;
		if (ProBuilderMesh.elementSelectionChanged != null)
		{
			ProBuilderMesh.elementSelectionChanged(this);
		}
	}

	public void SetSelectedVertices(IEnumerable<int> vertices)
	{
		m_SelectedFaces = new int[0];
		m_SelectedEdges = new Edge[0];
		m_SelectedVertices = ((vertices != null) ? vertices.Distinct().ToArray() : new int[0]);
		m_SelectedCacheDirty = true;
		if (ProBuilderMesh.elementSelectionChanged != null)
		{
			ProBuilderMesh.elementSelectionChanged(this);
		}
	}

	internal void RemoveFromFaceSelectionAtIndex(int index)
	{
		SetSelectedFaces(m_SelectedFaces.RemoveAt(index));
	}

	public void ClearSelection()
	{
		m_SelectedFaces = new int[0];
		m_SelectedEdges = new Edge[0];
		m_SelectedVertices = new int[0];
		m_SelectedCacheDirty = true;
	}
}
