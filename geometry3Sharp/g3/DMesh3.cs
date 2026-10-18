using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace g3;

public class DMesh3 : IDeformableMesh, IMesh, IPointSet
{
	public struct CompactInfo
	{
		public IIndexMap MapV;
	}

	public struct EdgeSplitInfo
	{
		public bool bIsBoundary;

		public int vNew;

		public int eNewBN;

		public int eNewCN;

		public int eNewDN;

		public int eNewT2;

		public int eNewT3;
	}

	public struct EdgeFlipInfo
	{
		public int eID;

		public int v0;

		public int v1;

		public int ov0;

		public int ov1;

		public int t0;

		public int t1;
	}

	public struct EdgeCollapseInfo
	{
		public int vKept;

		public int vRemoved;

		public bool bIsBoundary;

		public int eCollapsed;

		public int tRemoved0;

		public int tRemoved1;

		public int eRemoved0;

		public int eRemoved1;

		public int eKept0;

		public int eKept1;
	}

	public struct MergeEdgesInfo
	{
		public int eKept;

		public int eRemoved;

		public Vector2i vKept;

		public Vector2i vRemoved;

		public Vector2i eRemovedExtra;

		public Vector2i eKeptExtra;
	}

	public struct PokeTriangleInfo
	{
		public int new_vid;

		public int new_t1;

		public int new_t2;

		public Index3i new_edges;
	}

	public const int InvalidID = -1;

	public const int NonManifoldID = -2;

	public static readonly Vector3d InvalidVertex = new Vector3d(double.MaxValue, 0.0, 0.0);

	public static readonly Index3i InvalidTriangle = new Index3i(-1, -1, -1);

	public static readonly Index2i InvalidEdge = new Index2i(-1, -1);

	private RefCountVector vertices_refcount;

	private DVector<double> vertices;

	private DVector<float> normals;

	private DVector<float> colors;

	private DVector<float> uv;

	private SmallListSet vertex_edges;

	private RefCountVector triangles_refcount;

	private DVector<int> triangles;

	private DVector<int> triangle_edges;

	private DVector<int> triangle_groups;

	private RefCountVector edges_refcount;

	private DVector<int> edges;

	private int timestamp;

	private int shape_timestamp;

	private int max_group_id;

	public bool Clockwise;

	private Dictionary<string, object> Metadata;

	private AxisAlignedBox3d cached_bounds;

	private int cached_bounds_timestamp = -1;

	private bool cached_is_closed;

	private int cached_is_closed_timestamp = -1;

	public int Timestamp => timestamp;

	public int ShapeTimestamp => shape_timestamp;

	public int VertexCount => vertices_refcount.count;

	public int TriangleCount => triangles_refcount.count;

	public int EdgeCount => edges_refcount.count;

	public int MaxVertexID => vertices_refcount.max_index;

	public int MaxTriangleID => triangles_refcount.max_index;

	public int MaxEdgeID => edges_refcount.max_index;

	public int MaxGroupID => max_group_id;

	public bool HasVertexColors => colors != null;

	public bool HasVertexNormals => normals != null;

	public bool HasVertexUVs => uv != null;

	public bool HasTriangleGroups => triangle_groups != null;

	public MeshComponents Components
	{
		get
		{
			MeshComponents meshComponents = MeshComponents.None;
			if (normals != null)
			{
				meshComponents |= MeshComponents.VertexNormals;
			}
			if (colors != null)
			{
				meshComponents |= MeshComponents.VertexColors;
			}
			if (uv != null)
			{
				meshComponents |= MeshComponents.VertexUVs;
			}
			if (triangle_groups != null)
			{
				meshComponents |= MeshComponents.FaceGroups;
			}
			return meshComponents;
		}
	}

	public AxisAlignedBox3d CachedBounds
	{
		get
		{
			if (cached_bounds_timestamp != Timestamp)
			{
				cached_bounds = GetBounds();
				cached_bounds_timestamp = Timestamp;
			}
			return cached_bounds;
		}
	}

	public bool CachedIsClosed
	{
		get
		{
			if (cached_is_closed_timestamp != Timestamp)
			{
				cached_is_closed = IsClosed();
				cached_is_closed_timestamp = Timestamp;
			}
			return cached_is_closed;
		}
	}

	public bool IsCompact
	{
		get
		{
			if (vertices_refcount.is_dense && edges_refcount.is_dense)
			{
				return triangles_refcount.is_dense;
			}
			return false;
		}
	}

	public bool IsCompactV => vertices_refcount.is_dense;

	public bool IsCompactT => triangles_refcount.is_dense;

	public double CompactMetric => ((double)VertexCount / (double)MaxVertexID + (double)TriangleCount / (double)MaxTriangleID) * 0.5;

	public bool HasMetadata
	{
		get
		{
			if (Metadata != null)
			{
				return Metadata.Keys.Count > 0;
			}
			return false;
		}
	}

	public DVector<double> VerticesBuffer
	{
		get
		{
			return vertices;
		}
		set
		{
			vertices = value;
		}
	}

	public RefCountVector VerticesRefCounts
	{
		get
		{
			return vertices_refcount;
		}
		set
		{
			vertices_refcount = value;
		}
	}

	public DVector<float> NormalsBuffer
	{
		get
		{
			return normals;
		}
		set
		{
			normals = value;
		}
	}

	public DVector<float> ColorsBuffer
	{
		get
		{
			return colors;
		}
		set
		{
			colors = value;
		}
	}

	public DVector<float> UVBuffer
	{
		get
		{
			return uv;
		}
		set
		{
			uv = value;
		}
	}

	public DVector<int> TrianglesBuffer
	{
		get
		{
			return triangles;
		}
		set
		{
			triangles = value;
		}
	}

	public RefCountVector TrianglesRefCounts
	{
		get
		{
			return triangles_refcount;
		}
		set
		{
			triangles_refcount = value;
		}
	}

	public DVector<int> GroupsBuffer
	{
		get
		{
			return triangle_groups;
		}
		set
		{
			triangle_groups = value;
		}
	}

	public DVector<int> EdgesBuffer
	{
		get
		{
			return edges;
		}
		set
		{
			edges = value;
		}
	}

	public RefCountVector EdgesRefCounts
	{
		get
		{
			return edges_refcount;
		}
		set
		{
			edges_refcount = value;
		}
	}

	public SmallListSet VertexEdges
	{
		get
		{
			return vertex_edges;
		}
		set
		{
			vertex_edges = value;
		}
	}

	public DMesh3(bool bWantNormals = true, bool bWantColors = false, bool bWantUVs = false, bool bWantTriGroups = false)
	{
		vertices = new DVector<double>();
		if (bWantNormals)
		{
			normals = new DVector<float>();
		}
		if (bWantColors)
		{
			colors = new DVector<float>();
		}
		if (bWantUVs)
		{
			uv = new DVector<float>();
		}
		vertex_edges = new SmallListSet();
		vertices_refcount = new RefCountVector();
		triangles = new DVector<int>();
		triangle_edges = new DVector<int>();
		triangles_refcount = new RefCountVector();
		if (bWantTriGroups)
		{
			triangle_groups = new DVector<int>();
		}
		max_group_id = 0;
		edges = new DVector<int>();
		edges_refcount = new RefCountVector();
	}

	public DMesh3(MeshComponents flags)
		: this((flags & MeshComponents.VertexNormals) != 0, (flags & MeshComponents.VertexColors) != 0, (flags & MeshComponents.VertexUVs) != 0, (flags & MeshComponents.FaceGroups) != 0)
	{
	}

	public DMesh3(DMesh3 copy, bool bCompact = false, bool bWantNormals = true, bool bWantColors = true, bool bWantUVs = true)
	{
		if (bCompact)
		{
			CompactCopy(copy, bWantNormals, bWantColors, bWantUVs);
		}
		else
		{
			Copy(copy, bWantNormals, bWantColors, bWantUVs);
		}
	}

	public DMesh3(DMesh3 copy, bool bCompact, MeshComponents flags)
		: this(copy, bCompact, (flags & MeshComponents.VertexNormals) != 0, (flags & MeshComponents.VertexColors) != 0, (flags & MeshComponents.VertexUVs) != 0)
	{
	}

	public DMesh3(IMesh copy, MeshHints hints, bool bWantNormals = true, bool bWantColors = true, bool bWantUVs = true)
	{
		Copy(copy, hints, bWantNormals, bWantColors, bWantUVs);
	}

	public DMesh3(IMesh copy, MeshHints hints, MeshComponents flags)
		: this(copy, hints, (flags & MeshComponents.VertexNormals) != 0, (flags & MeshComponents.VertexColors) != 0, (flags & MeshComponents.VertexUVs) != 0)
	{
	}

	public CompactInfo CompactCopy(DMesh3 copy, bool bNormals = true, bool bColors = true, bool bUVs = true)
	{
		CompactInfo result;
		if (copy.IsCompact)
		{
			Copy(copy, bNormals, bColors, bUVs);
			result = default(CompactInfo);
			result.MapV = new IdentityIndexMap();
			return result;
		}
		vertices = new DVector<double>();
		vertex_edges = new SmallListSet();
		vertices_refcount = new RefCountVector();
		triangles = new DVector<int>();
		triangle_edges = new DVector<int>();
		triangles_refcount = new RefCountVector();
		edges = new DVector<int>();
		edges_refcount = new RefCountVector();
		max_group_id = 0;
		normals = ((bNormals && copy.normals != null) ? new DVector<float>() : null);
		colors = ((bColors && copy.colors != null) ? new DVector<float>() : null);
		uv = ((bUVs && copy.uv != null) ? new DVector<float>() : null);
		triangle_groups = ((copy.triangle_groups != null) ? new DVector<int>() : null);
		NewVertexInfo vinfo = default(NewVertexInfo);
		int[] array = new int[copy.MaxVertexID];
		foreach (int item in copy.vertices_refcount)
		{
			copy.GetVertex(item, ref vinfo, bNormals, bColors, bUVs);
			array[item] = AppendVertex(vinfo);
		}
		foreach (int item2 in copy.triangles_refcount)
		{
			Index3i triangle = copy.GetTriangle(item2);
			triangle.a = array[triangle.a];
			triangle.b = array[triangle.b];
			triangle.c = array[triangle.c];
			int num2 = (copy.HasTriangleGroups ? copy.GetTriangleGroup(item2) : (-1));
			AppendTriangle(triangle, num2);
			max_group_id = Math.Max(max_group_id, num2 + 1);
		}
		if (copy.Metadata != null)
		{
			Metadata = copy.Metadata;
		}
		result = default(CompactInfo);
		result.MapV = new IndexMap(array, MaxVertexID);
		return result;
	}

	public void Copy(DMesh3 copy, bool bNormals = true, bool bColors = true, bool bUVs = true)
	{
		vertices = new DVector<double>(copy.vertices);
		normals = ((bNormals && copy.normals != null) ? new DVector<float>(copy.normals) : null);
		colors = ((bColors && copy.colors != null) ? new DVector<float>(copy.colors) : null);
		uv = ((bUVs && copy.uv != null) ? new DVector<float>(copy.uv) : null);
		vertices_refcount = new RefCountVector(copy.vertices_refcount);
		vertex_edges = new SmallListSet(copy.vertex_edges);
		triangles = new DVector<int>(copy.triangles);
		triangle_edges = new DVector<int>(copy.triangle_edges);
		triangles_refcount = new RefCountVector(copy.triangles_refcount);
		if (copy.triangle_groups != null)
		{
			triangle_groups = new DVector<int>(copy.triangle_groups);
		}
		if (copy.Metadata != null)
		{
			Metadata = copy.Metadata;
		}
		max_group_id = copy.max_group_id;
		edges = new DVector<int>(copy.edges);
		edges_refcount = new RefCountVector(copy.edges_refcount);
	}

	public CompactInfo Copy(IMesh copy, MeshHints hints, bool bNormals = true, bool bColors = true, bool bUVs = true)
	{
		vertices = new DVector<double>();
		vertex_edges = new SmallListSet();
		vertices_refcount = new RefCountVector();
		triangles = new DVector<int>();
		triangle_edges = new DVector<int>();
		triangles_refcount = new RefCountVector();
		edges = new DVector<int>();
		edges_refcount = new RefCountVector();
		max_group_id = 0;
		normals = ((bNormals && copy.HasVertexNormals) ? new DVector<float>() : null);
		colors = ((bColors && copy.HasVertexColors) ? new DVector<float>() : null);
		uv = ((bUVs && copy.HasVertexUVs) ? new DVector<float>() : null);
		triangle_groups = (copy.HasTriangleGroups ? new DVector<int>() : null);
		NewVertexInfo newVertexInfo = default(NewVertexInfo);
		int[] array = new int[copy.MaxVertexID];
		foreach (int item in copy.VertexIndices())
		{
			newVertexInfo = copy.GetVertexAll(item);
			array[item] = AppendVertex(newVertexInfo);
		}
		foreach (int item2 in copy.TriangleIndices())
		{
			Index3i triangle = copy.GetTriangle(item2);
			triangle.a = array[triangle.a];
			triangle.b = array[triangle.b];
			triangle.c = array[triangle.c];
			int num = (copy.HasTriangleGroups ? copy.GetTriangleGroup(item2) : (-1));
			AppendTriangle(triangle, num);
			max_group_id = Math.Max(max_group_id, num + 1);
		}
		CompactInfo result = default(CompactInfo);
		result.MapV = new IndexMap(array, MaxVertexID);
		return result;
	}

	private void updateTimeStamp(bool bShapeChange)
	{
		timestamp++;
		if (bShapeChange)
		{
			shape_timestamp++;
		}
	}

	public bool IsVertex(int vID)
	{
		return vertices_refcount.isValid(vID);
	}

	public bool IsTriangle(int tID)
	{
		return triangles_refcount.isValid(tID);
	}

	public bool IsEdge(int eID)
	{
		return edges_refcount.isValid(eID);
	}

	public Vector3d GetVertex(int vID)
	{
		int num = 3 * vID;
		return new Vector3d(vertices[num], vertices[num + 1], vertices[num + 2]);
	}

	public Vector3f GetVertexf(int vID)
	{
		int num = 3 * vID;
		return new Vector3f((float)vertices[num], (float)vertices[num + 1], (float)vertices[num + 2]);
	}

	public void SetVertex(int vID, Vector3d vNewPos)
	{
		int num = 3 * vID;
		vertices[num] = vNewPos.x;
		vertices[num + 1] = vNewPos.y;
		vertices[num + 2] = vNewPos.z;
		updateTimeStamp(bShapeChange: true);
	}

	public Vector3f GetVertexNormal(int vID)
	{
		if (normals == null)
		{
			return Vector3f.AxisY;
		}
		int num = 3 * vID;
		return new Vector3f(normals[num], normals[num + 1], normals[num + 2]);
	}

	public void SetVertexNormal(int vID, Vector3f vNewNormal)
	{
		if (HasVertexNormals)
		{
			int num = 3 * vID;
			normals[num] = vNewNormal.x;
			normals[num + 1] = vNewNormal.y;
			normals[num + 2] = vNewNormal.z;
			updateTimeStamp(bShapeChange: false);
		}
	}

	public Vector3f GetVertexColor(int vID)
	{
		if (colors == null)
		{
			return Vector3f.One;
		}
		int num = 3 * vID;
		return new Vector3f(colors[num], colors[num + 1], colors[num + 2]);
	}

	public void SetVertexColor(int vID, Vector3f vNewColor)
	{
		if (HasVertexColors)
		{
			int num = 3 * vID;
			colors[num] = vNewColor.x;
			colors[num + 1] = vNewColor.y;
			colors[num + 2] = vNewColor.z;
			updateTimeStamp(bShapeChange: false);
		}
	}

	public Vector2f GetVertexUV(int vID)
	{
		if (uv == null)
		{
			return Vector2f.Zero;
		}
		int num = 2 * vID;
		return new Vector2f(uv[num], uv[num + 1]);
	}

	public void SetVertexUV(int vID, Vector2f vNewUV)
	{
		if (HasVertexUVs)
		{
			int num = 2 * vID;
			uv[num] = vNewUV.x;
			uv[num + 1] = vNewUV.y;
			updateTimeStamp(bShapeChange: false);
		}
	}

	public bool GetVertex(int vID, ref NewVertexInfo vinfo, bool bWantNormals, bool bWantColors, bool bWantUVs)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return false;
		}
		vinfo.v.Set(vertices[3 * vID], vertices[3 * vID + 1], vertices[3 * vID + 2]);
		vinfo.bHaveN = (vinfo.bHaveUV = (vinfo.bHaveC = false));
		if (HasVertexNormals && bWantNormals)
		{
			vinfo.bHaveN = true;
			vinfo.n.Set(normals[3 * vID], normals[3 * vID + 1], normals[3 * vID + 2]);
		}
		if (HasVertexColors && bWantColors)
		{
			vinfo.bHaveC = true;
			vinfo.c.Set(colors[3 * vID], colors[3 * vID + 1], colors[3 * vID + 2]);
		}
		if (HasVertexUVs && bWantUVs)
		{
			vinfo.bHaveUV = true;
			vinfo.uv.Set(uv[2 * vID], uv[2 * vID + 1]);
		}
		return true;
	}

	[Obsolete("GetVtxEdges will be removed in future, use VtxEdgesItr instead")]
	public ReadOnlyCollection<int> GetVtxEdges(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return null;
		}
		return vertex_edges_list(vID).AsReadOnly();
	}

	public int GetVtxEdgeCount(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return -1;
		}
		return vertex_edges.Count(vID);
	}

	[Obsolete("GetVtxEdgeValence will be removed in future, use GetVtxEdgeCount instead")]
	public int GetVtxEdgeValence(int vID)
	{
		return vertex_edges.Count(vID);
	}

	public int GetMaxVtxEdgeCount()
	{
		int num = 0;
		foreach (int item in vertices_refcount)
		{
			num = Math.Max(num, vertex_edges.Count(item));
		}
		return num;
	}

	public NewVertexInfo GetVertexAll(int i)
	{
		NewVertexInfo result = default(NewVertexInfo);
		result.v = GetVertex(i);
		if (HasVertexNormals)
		{
			result.bHaveN = true;
			result.n = GetVertexNormal(i);
		}
		else
		{
			result.bHaveN = false;
		}
		if (HasVertexColors)
		{
			result.bHaveC = true;
			result.c = GetVertexColor(i);
		}
		else
		{
			result.bHaveC = false;
		}
		if (HasVertexUVs)
		{
			result.bHaveUV = true;
			result.uv = GetVertexUV(i);
		}
		else
		{
			result.bHaveUV = false;
		}
		return result;
	}

	public Frame3f GetVertexFrame(int vID, bool bFrameNormalY = false)
	{
		int num = 3 * vID;
		Vector3d vector3d = new Vector3d(vertices[num], vertices[num + 1], vertices[num + 2]);
		Vector3d vector3d2 = new Vector3d(normals[num], normals[num + 1], normals[num + 2]);
		int eID = vertex_edges.First(vID);
		int num2 = 3 * edge_other_v(eID, vID);
		Vector3d v = new Vector3d(vertices[num2], vertices[num2 + 1], vertices[num2 + 2]) - vector3d;
		v.Normalize();
		Vector3d vector3d3 = vector3d2.Cross(v);
		v = vector3d3.Cross(vector3d2);
		if (bFrameNormalY)
		{
			return new Frame3f((Vector3f)vector3d, (Vector3f)v, (Vector3f)vector3d2, (Vector3f)(-vector3d3));
		}
		return new Frame3f((Vector3f)vector3d, (Vector3f)v, (Vector3f)vector3d3, (Vector3f)vector3d2);
	}

	public Index3i GetTriangle(int tID)
	{
		int num = 3 * tID;
		return new Index3i(triangles[num], triangles[num + 1], triangles[num + 2]);
	}

	public Index3i GetTriEdges(int tID)
	{
		int num = 3 * tID;
		return new Index3i(triangle_edges[num], triangle_edges[num + 1], triangle_edges[num + 2]);
	}

	public int GetTriEdge(int tid, int j)
	{
		return triangle_edges[3 * tid + j];
	}

	public Index3i GetTriNeighbourTris(int tID)
	{
		if (triangles_refcount.isValid(tID))
		{
			int num = 3 * tID;
			Index3i zero = Index3i.Zero;
			for (int i = 0; i < 3; i++)
			{
				int num2 = 4 * triangle_edges[num + i];
				zero[i] = ((edges[num2 + 2] == tID) ? edges[num2 + 3] : edges[num2 + 2]);
			}
			return zero;
		}
		return InvalidTriangle;
	}

	public IEnumerable<int> TriTrianglesItr(int tID)
	{
		if (!triangles_refcount.isValid(tID))
		{
			yield break;
		}
		int tei = 3 * tID;
		int i = 0;
		while (i < 3)
		{
			int num = 4 * triangle_edges[tei + i];
			int num2 = ((edges[num + 2] == tID) ? edges[num + 3] : edges[num + 2]);
			if (num2 != -1)
			{
				yield return num2;
			}
			int num3 = i + 1;
			i = num3;
		}
	}

	public int GetTriangleGroup(int tID)
	{
		if (triangle_groups != null)
		{
			if (!triangles_refcount.isValid(tID))
			{
				return 0;
			}
			return triangle_groups[tID];
		}
		return -1;
	}

	public void SetTriangleGroup(int tid, int group_id)
	{
		if (triangle_groups != null)
		{
			triangle_groups[tid] = group_id;
			max_group_id = Math.Max(max_group_id, group_id + 1);
			updateTimeStamp(bShapeChange: false);
		}
	}

	public int AllocateTriangleGroup()
	{
		return ++max_group_id;
	}

	public void GetTriVertices(int tID, ref Vector3d v0, ref Vector3d v1, ref Vector3d v2)
	{
		int num = 3 * triangles[3 * tID];
		v0.x = vertices[num];
		v0.y = vertices[num + 1];
		v0.z = vertices[num + 2];
		int num2 = 3 * triangles[3 * tID + 1];
		v1.x = vertices[num2];
		v1.y = vertices[num2 + 1];
		v1.z = vertices[num2 + 2];
		int num3 = 3 * triangles[3 * tID + 2];
		v2.x = vertices[num3];
		v2.y = vertices[num3 + 1];
		v2.z = vertices[num3 + 2];
	}

	public Vector3d GetTriVertex(int tid, int j)
	{
		int num = triangles[3 * tid + j];
		return new Vector3d(vertices[3 * num], vertices[3 * num + 1], vertices[3 * num + 2]);
	}

	public Vector3d GetTriBaryPoint(int tID, double bary0, double bary1, double bary2)
	{
		int num = 3 * triangles[3 * tID];
		int num2 = 3 * triangles[3 * tID + 1];
		int num3 = 3 * triangles[3 * tID + 2];
		return new Vector3d(bary0 * vertices[num] + bary1 * vertices[num2] + bary2 * vertices[num3], bary0 * vertices[num + 1] + bary1 * vertices[num2 + 1] + bary2 * vertices[num3 + 1], bary0 * vertices[num + 2] + bary1 * vertices[num2 + 2] + bary2 * vertices[num3 + 2]);
	}

	public Vector3d GetTriNormal(int tID)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		GetTriVertices(tID, ref v, ref v2, ref v3);
		return MathUtil.Normal(ref v, ref v2, ref v3);
	}

	public double GetTriArea(int tID)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		GetTriVertices(tID, ref v, ref v2, ref v3);
		return MathUtil.Area(ref v, ref v2, ref v3);
	}

	public void GetTriInfo(int tID, out Vector3d normal, out double fArea, out Vector3d vCentroid)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		GetTriVertices(tID, ref v, ref v2, ref v3);
		vCentroid = 1.0 / 3.0 * (v + v2 + v3);
		normal = MathUtil.FastNormalArea(ref v, ref v2, ref v3, out fArea);
	}

	public Vector3d GetTriBaryNormal(int tID, double bary0, double bary1, double bary2)
	{
		int num = 3 * triangles[3 * tID];
		int num2 = 3 * triangles[3 * tID + 1];
		int num3 = 3 * triangles[3 * tID + 2];
		Vector3d result = new Vector3d(bary0 * (double)normals[num] + bary1 * (double)normals[num2] + bary2 * (double)normals[num3], bary0 * (double)normals[num + 1] + bary1 * (double)normals[num2 + 1] + bary2 * (double)normals[num3 + 1], bary0 * (double)normals[num + 2] + bary1 * (double)normals[num2 + 2] + bary2 * (double)normals[num3 + 2]);
		result.Normalize();
		return result;
	}

	public Vector3d GetTriCentroid(int tID)
	{
		int num = 3 * triangles[3 * tID];
		int num2 = 3 * triangles[3 * tID + 1];
		int num3 = 3 * triangles[3 * tID + 2];
		double num4 = 1.0 / 3.0;
		return new Vector3d((vertices[num] + vertices[num2] + vertices[num3]) * num4, (vertices[num + 1] + vertices[num2 + 1] + vertices[num3 + 1]) * num4, (vertices[num + 2] + vertices[num2 + 2] + vertices[num3 + 2]) * num4);
	}

	public void GetTriBaryPoint(int tID, double bary0, double bary1, double bary2, out NewVertexInfo vinfo)
	{
		vinfo = default(NewVertexInfo);
		int num = 3 * triangles[3 * tID];
		int num2 = 3 * triangles[3 * tID + 1];
		int num3 = 3 * triangles[3 * tID + 2];
		vinfo.v = new Vector3d(bary0 * vertices[num] + bary1 * vertices[num2] + bary2 * vertices[num3], bary0 * vertices[num + 1] + bary1 * vertices[num2 + 1] + bary2 * vertices[num3 + 1], bary0 * vertices[num + 2] + bary1 * vertices[num2 + 2] + bary2 * vertices[num3 + 2]);
		vinfo.bHaveN = HasVertexNormals;
		if (vinfo.bHaveN)
		{
			vinfo.n = new Vector3f(bary0 * (double)normals[num] + bary1 * (double)normals[num2] + bary2 * (double)normals[num3], bary0 * (double)normals[num + 1] + bary1 * (double)normals[num2 + 1] + bary2 * (double)normals[num3 + 1], bary0 * (double)normals[num + 2] + bary1 * (double)normals[num2 + 2] + bary2 * (double)normals[num3 + 2]);
			vinfo.n.Normalize();
		}
		vinfo.bHaveC = HasVertexColors;
		if (vinfo.bHaveC)
		{
			vinfo.c = new Vector3f(bary0 * (double)colors[num] + bary1 * (double)colors[num2] + bary2 * (double)colors[num3], bary0 * (double)colors[num + 1] + bary1 * (double)colors[num2 + 1] + bary2 * (double)colors[num3 + 1], bary0 * (double)colors[num + 2] + bary1 * (double)colors[num2 + 2] + bary2 * (double)colors[num3 + 2]);
		}
		vinfo.bHaveUV = HasVertexUVs;
		if (vinfo.bHaveUV)
		{
			num = 2 * triangles[3 * tID];
			num2 = 2 * triangles[3 * tID + 1];
			num3 = 2 * triangles[3 * tID + 2];
			vinfo.uv = new Vector2f(bary0 * (double)uv[num] + bary1 * (double)uv[num2] + bary2 * (double)uv[num3], bary0 * (double)uv[num + 1] + bary1 * (double)uv[num2 + 1] + bary2 * (double)uv[num3 + 1]);
		}
	}

	public AxisAlignedBox3d GetTriBounds(int tID)
	{
		int num = 3 * triangles[3 * tID];
		double num2 = vertices[num];
		double num3 = vertices[num + 1];
		double num4 = vertices[num + 2];
		double num5 = num2;
		double num6 = num2;
		double num7 = num3;
		double num8 = num3;
		double num9 = num4;
		double num10 = num4;
		for (int i = 1; i < 3; i++)
		{
			num = 3 * triangles[3 * tID + i];
			num2 = vertices[num];
			num3 = vertices[num + 1];
			num4 = vertices[num + 2];
			if (num2 < num5)
			{
				num5 = num2;
			}
			else if (num2 > num6)
			{
				num6 = num2;
			}
			if (num3 < num7)
			{
				num7 = num3;
			}
			else if (num3 > num8)
			{
				num8 = num3;
			}
			if (num4 < num9)
			{
				num9 = num4;
			}
			else if (num4 > num10)
			{
				num10 = num4;
			}
		}
		return new AxisAlignedBox3d(num5, num7, num9, num6, num8, num10);
	}

	public Frame3f GetTriFrame(int tID, int nEdge = 0)
	{
		int num = 3 * tID;
		int num2 = 3 * triangles[num + nEdge % 3];
		int num3 = 3 * triangles[num + (nEdge + 1) % 3];
		int num4 = 3 * triangles[num + (nEdge + 2) % 3];
		Vector3d vector3d = new Vector3d(vertices[num2], vertices[num2 + 1], vertices[num2 + 2]);
		Vector3d vector3d2 = new Vector3d(vertices[num3], vertices[num3 + 1], vertices[num3 + 2]);
		Vector3d vector3d3 = new Vector3d(vertices[num4], vertices[num4 + 1], vertices[num4 + 2]);
		Vector3d vector3d4 = vector3d2 - vector3d;
		vector3d4.Normalize();
		Vector3d v = vector3d3 - vector3d2;
		v.Normalize();
		Vector3d vector3d5 = vector3d4.Cross(v);
		vector3d5.Normalize();
		Vector3d vector3d6 = vector3d5.Cross(vector3d4);
		return new Frame3f((Vector3f)(vector3d + vector3d2 + vector3d3) / 3f, (Vector3f)vector3d4, (Vector3f)vector3d6, (Vector3f)vector3d5);
	}

	public double GetTriSolidAngle(int tID, ref Vector3d p)
	{
		int num = 3 * tID;
		int num2 = 3 * triangles[num];
		Vector3d v = new Vector3d(vertices[num2] - p.x, vertices[num2 + 1] - p.y, vertices[num2 + 2] - p.z);
		int num3 = 3 * triangles[num + 1];
		Vector3d v2 = new Vector3d(vertices[num3] - p.x, vertices[num3 + 1] - p.y, vertices[num3 + 2] - p.z);
		int num4 = 3 * triangles[num + 2];
		Vector3d v3 = new Vector3d(vertices[num4] - p.x, vertices[num4 + 1] - p.y, vertices[num4 + 2] - p.z);
		double length = v.Length;
		double length2 = v2.Length;
		double length3 = v3.Length;
		double x = length * length2 * length3 + v.Dot(ref v2) * length3 + v2.Dot(ref v3) * length + v3.Dot(ref v) * length2;
		double y = v.x * (v2.y * v3.z - v3.y * v2.z) - v.y * (v2.x * v3.z - v3.x * v2.z) + v.z * (v2.x * v3.y - v3.x * v2.y);
		return 2.0 * Math.Atan2(y, x);
	}

	public double GetTriInternalAngleR(int tID, int i)
	{
		int num = 3 * tID;
		int num2 = 3 * triangles[num];
		Vector3d vector3d = new Vector3d(vertices[num2], vertices[num2 + 1], vertices[num2 + 2]);
		int num3 = 3 * triangles[num + 1];
		Vector3d vector3d2 = new Vector3d(vertices[num3], vertices[num3 + 1], vertices[num3 + 2]);
		int num4 = 3 * triangles[num + 2];
		Vector3d vector3d3 = new Vector3d(vertices[num4], vertices[num4 + 1], vertices[num4 + 2]);
		return i switch
		{
			0 => (vector3d2 - vector3d).Normalized.AngleR((vector3d3 - vector3d).Normalized), 
			1 => (vector3d - vector3d2).Normalized.AngleR((vector3d3 - vector3d2).Normalized), 
			_ => (vector3d - vector3d3).Normalized.AngleR((vector3d2 - vector3d3).Normalized), 
		};
	}

	public Index2i GetEdgeV(int eID)
	{
		int num = 4 * eID;
		return new Index2i(edges[num], edges[num + 1]);
	}

	public bool GetEdgeV(int eID, ref Vector3d a, ref Vector3d b)
	{
		int num = 3 * edges[4 * eID];
		a.x = vertices[num];
		a.y = vertices[num + 1];
		a.z = vertices[num + 2];
		int num2 = 3 * edges[4 * eID + 1];
		b.x = vertices[num2];
		b.y = vertices[num2 + 1];
		b.z = vertices[num2 + 2];
		return true;
	}

	public Index2i GetEdgeT(int eID)
	{
		int num = 4 * eID;
		return new Index2i(edges[num + 2], edges[num + 3]);
	}

	public Index4i GetEdge(int eID)
	{
		int num = 4 * eID;
		return new Index4i(edges[num], edges[num + 1], edges[num + 2], edges[num + 3]);
	}

	public bool GetEdge(int eID, ref int a, ref int b, ref int t0, ref int t1)
	{
		int num = eID * 4;
		a = edges[num];
		b = edges[num + 1];
		t0 = edges[num + 2];
		t1 = edges[num + 3];
		return true;
	}

	public Index2i GetOrientedBoundaryEdgeV(int eID)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 4 * eID;
			if (edges[num + 3] == -1)
			{
				int a = edges[num];
				int b = edges[num + 1];
				int num2 = 3 * edges[num + 2];
				Index3i tri_verts = new Index3i(triangles[num2], triangles[num2 + 1], triangles[num2 + 2]);
				int num3 = IndexUtil.find_edge_index_in_tri(a, b, ref tri_verts);
				return new Index2i(tri_verts[num3], tri_verts[(num3 + 1) % 3]);
			}
		}
		return InvalidEdge;
	}

	public Vector3d GetEdgeNormal(int eID)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 4 * eID;
			Vector3d triNormal = GetTriNormal(edges[num + 2]);
			if (edges[num + 3] != -1)
			{
				triNormal += GetTriNormal(edges[num + 3]);
				triNormal.Normalize();
			}
			return triNormal;
		}
		return Vector3d.Zero;
	}

	public Vector3d GetEdgePoint(int eID, double t)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 4 * eID;
			int num2 = 3 * edges[num];
			int num3 = 3 * edges[num + 1];
			double num4 = 1.0 - t;
			return new Vector3d(num4 * vertices[num2] + t * vertices[num3], num4 * vertices[num2 + 1] + t * vertices[num3 + 1], num4 * vertices[num2 + 2] + t * vertices[num3 + 2]);
		}
		return Vector3d.Zero;
	}

	public int AppendVertex(Vector3d v)
	{
		return AppendVertex(new NewVertexInfo
		{
			v = v,
			bHaveC = false,
			bHaveUV = false,
			bHaveN = false
		});
	}

	public int AppendVertex(ref NewVertexInfo info)
	{
		int num = vertices_refcount.allocate();
		int num2 = 3 * num;
		vertices.insert(info.v[2], num2 + 2);
		vertices.insert(info.v[1], num2 + 1);
		vertices.insert(info.v[0], num2);
		if (normals != null)
		{
			Vector3f vector3f = (info.bHaveN ? info.n : Vector3f.AxisY);
			normals.insert(vector3f[2], num2 + 2);
			normals.insert(vector3f[1], num2 + 1);
			normals.insert(vector3f[0], num2);
		}
		if (colors != null)
		{
			Vector3f vector3f2 = (info.bHaveC ? info.c : Vector3f.One);
			colors.insert(vector3f2[2], num2 + 2);
			colors.insert(vector3f2[1], num2 + 1);
			colors.insert(vector3f2[0], num2);
		}
		if (uv != null)
		{
			Vector2f vector2f = (info.bHaveUV ? info.uv : Vector2f.Zero);
			int num3 = 2 * num;
			uv.insert(vector2f[1], num3 + 1);
			uv.insert(vector2f[0], num3);
		}
		allocate_edges_list(num);
		updateTimeStamp(bShapeChange: true);
		return num;
	}

	public int AppendVertex(NewVertexInfo info)
	{
		return AppendVertex(ref info);
	}

	public int AppendVertex(DMesh3 from, int fromVID)
	{
		int num = 3 * fromVID;
		int num2 = vertices_refcount.allocate();
		int num3 = 3 * num2;
		vertices.insert(from.vertices[num + 2], num3 + 2);
		vertices.insert(from.vertices[num + 1], num3 + 1);
		vertices.insert(from.vertices[num], num3);
		if (normals != null)
		{
			if (from.normals != null)
			{
				normals.insert(from.normals[num + 2], num3 + 2);
				normals.insert(from.normals[num + 1], num3 + 1);
				normals.insert(from.normals[num], num3);
			}
			else
			{
				normals.insert(0f, num3 + 2);
				normals.insert(1f, num3 + 1);
				normals.insert(0f, num3);
			}
		}
		if (colors != null)
		{
			if (from.colors != null)
			{
				colors.insert(from.colors[num + 2], num3 + 2);
				colors.insert(from.colors[num + 1], num3 + 1);
				colors.insert(from.colors[num], num3);
			}
			else
			{
				colors.insert(1f, num3 + 2);
				colors.insert(1f, num3 + 1);
				colors.insert(1f, num3);
			}
		}
		if (uv != null)
		{
			int num4 = 2 * num2;
			if (from.uv != null)
			{
				int num5 = 2 * fromVID;
				uv.insert(from.uv[num5 + 1], num4 + 1);
				uv.insert(from.uv[num5], num4);
			}
			else
			{
				uv.insert(0f, num4 + 1);
				uv.insert(0f, num4);
			}
		}
		allocate_edges_list(num2);
		updateTimeStamp(bShapeChange: true);
		return num2;
	}

	public MeshResult InsertVertex(int vid, ref NewVertexInfo info, bool bUnsafe = false)
	{
		if (vertices_refcount.isValid(vid))
		{
			return MeshResult.Failed_VertexAlreadyExists;
		}
		if (!(bUnsafe ? vertices_refcount.allocate_at_unsafe(vid) : vertices_refcount.allocate_at(vid)))
		{
			return MeshResult.Failed_CannotAllocateVertex;
		}
		int num = 3 * vid;
		vertices.insert(info.v[2], num + 2);
		vertices.insert(info.v[1], num + 1);
		vertices.insert(info.v[0], num);
		if (normals != null)
		{
			Vector3f vector3f = (info.bHaveN ? info.n : Vector3f.AxisY);
			normals.insert(vector3f[2], num + 2);
			normals.insert(vector3f[1], num + 1);
			normals.insert(vector3f[0], num);
		}
		if (colors != null)
		{
			Vector3f vector3f2 = (info.bHaveC ? info.c : Vector3f.One);
			colors.insert(vector3f2[2], num + 2);
			colors.insert(vector3f2[1], num + 1);
			colors.insert(vector3f2[0], num);
		}
		if (uv != null)
		{
			Vector2f vector2f = (info.bHaveUV ? info.uv : Vector2f.Zero);
			int num2 = 2 * vid;
			uv.insert(vector2f[1], num2 + 1);
			uv.insert(vector2f[0], num2);
		}
		allocate_edges_list(vid);
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public MeshResult InsertVertex(int vid, NewVertexInfo info)
	{
		return InsertVertex(vid, ref info);
	}

	public virtual void BeginUnsafeVerticesInsert()
	{
	}

	public virtual void EndUnsafeVerticesInsert()
	{
		vertices_refcount.rebuild_free_list();
	}

	public int AppendTriangle(int v0, int v1, int v2, int gid = -1)
	{
		return AppendTriangle(new Index3i(v0, v1, v2), gid);
	}

	public int AppendTriangle(Index3i tv, int gid = -1)
	{
		if (!IsVertex(tv[0]) || !IsVertex(tv[1]) || !IsVertex(tv[2]))
		{
			return -1;
		}
		if (tv[0] == tv[1] || tv[0] == tv[2] || tv[1] == tv[2])
		{
			return -1;
		}
		int num = find_edge(tv[0], tv[1]);
		int num2 = find_edge(tv[1], tv[2]);
		int num3 = find_edge(tv[2], tv[0]);
		if ((num != -1 && !IsBoundaryEdge(num)) || (num2 != -1 && !IsBoundaryEdge(num2)) || (num3 != -1 && !IsBoundaryEdge(num3)))
		{
			return -2;
		}
		int num4 = triangles_refcount.allocate();
		int num5 = 3 * num4;
		triangles.insert(tv[2], num5 + 2);
		triangles.insert(tv[1], num5 + 1);
		triangles.insert(tv[0], num5);
		if (triangle_groups != null)
		{
			triangle_groups.insert(gid, num4);
			max_group_id = Math.Max(max_group_id, gid + 1);
		}
		vertices_refcount.increment(tv[0], 1);
		vertices_refcount.increment(tv[1], 1);
		vertices_refcount.increment(tv[2], 1);
		add_tri_edge(num4, tv[0], tv[1], 0, num);
		add_tri_edge(num4, tv[1], tv[2], 1, num2);
		add_tri_edge(num4, tv[2], tv[0], 2, num3);
		updateTimeStamp(bShapeChange: true);
		return num4;
	}

	private void add_tri_edge(int tid, int v0, int v1, int j, int eid)
	{
		if (eid != -1)
		{
			edges[4 * eid + 3] = tid;
			triangle_edges.insert(eid, 3 * tid + j);
		}
		else
		{
			triangle_edges.insert(add_edge(v0, v1, tid), 3 * tid + j);
		}
	}

	public MeshResult InsertTriangle(int tid, Index3i tv, int gid = -1, bool bUnsafe = false)
	{
		if (triangles_refcount.isValid(tid))
		{
			return MeshResult.Failed_TriangleAlreadyExists;
		}
		if (!IsVertex(tv[0]) || !IsVertex(tv[1]) || !IsVertex(tv[2]))
		{
			return MeshResult.Failed_NotAVertex;
		}
		if (tv[0] == tv[1] || tv[0] == tv[2] || tv[1] == tv[2])
		{
			return MeshResult.Failed_InvalidNeighbourhood;
		}
		int num = find_edge(tv[0], tv[1]);
		int num2 = find_edge(tv[1], tv[2]);
		int num3 = find_edge(tv[2], tv[0]);
		if ((num != -1 && !IsBoundaryEdge(num)) || (num2 != -1 && !IsBoundaryEdge(num2)) || (num3 != -1 && !IsBoundaryEdge(num3)))
		{
			return MeshResult.Failed_WouldCreateNonmanifoldEdge;
		}
		if (!(bUnsafe ? triangles_refcount.allocate_at_unsafe(tid) : triangles_refcount.allocate_at(tid)))
		{
			return MeshResult.Failed_CannotAllocateTriangle;
		}
		int num4 = 3 * tid;
		triangles.insert(tv[2], num4 + 2);
		triangles.insert(tv[1], num4 + 1);
		triangles.insert(tv[0], num4);
		if (triangle_groups != null)
		{
			triangle_groups.insert(gid, tid);
			max_group_id = Math.Max(max_group_id, gid + 1);
		}
		vertices_refcount.increment(tv[0], 1);
		vertices_refcount.increment(tv[1], 1);
		vertices_refcount.increment(tv[2], 1);
		add_tri_edge(tid, tv[0], tv[1], 0, num);
		add_tri_edge(tid, tv[1], tv[2], 1, num2);
		add_tri_edge(tid, tv[2], tv[0], 2, num3);
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public virtual void BeginUnsafeTrianglesInsert()
	{
	}

	public virtual void EndUnsafeTrianglesInsert()
	{
		triangles_refcount.rebuild_free_list();
	}

	public void EnableVertexNormals(Vector3f initial_normal)
	{
		if (!HasVertexNormals)
		{
			normals = new DVector<float>();
			int maxVertexID = MaxVertexID;
			normals.resize(3 * maxVertexID);
			for (int i = 0; i < maxVertexID; i++)
			{
				int num = 3 * i;
				normals[num] = initial_normal.x;
				normals[num + 1] = initial_normal.y;
				normals[num + 2] = initial_normal.z;
			}
		}
	}

	public void DiscardVertexNormals()
	{
		normals = null;
	}

	public void EnableVertexColors(Vector3f initial_color)
	{
		if (!HasVertexColors)
		{
			colors = new DVector<float>();
			int maxVertexID = MaxVertexID;
			colors.resize(3 * maxVertexID);
			for (int i = 0; i < maxVertexID; i++)
			{
				int num = 3 * i;
				colors[num] = initial_color.x;
				colors[num + 1] = initial_color.y;
				colors[num + 2] = initial_color.z;
			}
		}
	}

	public void DiscardVertexColors()
	{
		colors = null;
	}

	public void EnableVertexUVs(Vector2f initial_uv)
	{
		if (!HasVertexUVs)
		{
			uv = new DVector<float>();
			int maxVertexID = MaxVertexID;
			uv.resize(2 * maxVertexID);
			for (int i = 0; i < maxVertexID; i++)
			{
				int num = 2 * i;
				uv[num] = initial_uv.x;
				uv[num + 1] = initial_uv.y;
			}
		}
	}

	public void DiscardVertexUVs()
	{
		uv = null;
	}

	public void EnableTriangleGroups(int initial_group = 0)
	{
		if (!HasTriangleGroups)
		{
			triangle_groups = new DVector<int>();
			int maxTriangleID = MaxTriangleID;
			triangle_groups.resize(maxTriangleID);
			for (int i = 0; i < maxTriangleID; i++)
			{
				triangle_groups[i] = initial_group;
			}
			max_group_id = 0;
		}
	}

	public void DiscardTriangleGroups()
	{
		triangle_groups = null;
		max_group_id = 0;
	}

	public IEnumerable<int> VertexIndices()
	{
		foreach (int item in vertices_refcount)
		{
			yield return item;
		}
	}

	public IEnumerable<int> TriangleIndices()
	{
		foreach (int item in triangles_refcount)
		{
			yield return item;
		}
	}

	public IEnumerable<int> EdgeIndices()
	{
		foreach (int item in edges_refcount)
		{
			yield return item;
		}
	}

	public IEnumerable<int> BoundaryEdgeIndices()
	{
		foreach (int item in edges_refcount)
		{
			if (edges[4 * item + 3] == -1)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<Vector3d> Vertices()
	{
		foreach (int item in vertices_refcount)
		{
			int num2 = 3 * item;
			yield return new Vector3d(vertices[num2], vertices[num2 + 1], vertices[num2 + 2]);
		}
	}

	public IEnumerable<Index3i> Triangles()
	{
		foreach (int item in triangles_refcount)
		{
			int num2 = 3 * item;
			yield return new Index3i(triangles[num2], triangles[num2 + 1], triangles[num2 + 2]);
		}
	}

	public IEnumerable<Index4i> Edges()
	{
		foreach (int item in edges_refcount)
		{
			int num2 = 4 * item;
			yield return new Index4i(edges[num2], edges[num2 + 1], edges[num2 + 2], edges[num2 + 3]);
		}
	}

	public int FindEdge(int vA, int vB)
	{
		return find_edge(vA, vB);
	}

	public int FindEdgeFromTri(int vA, int vB, int tID)
	{
		return find_edge_from_tri(vA, vB, tID);
	}

	public Index2i GetEdgeOpposingV(int eID)
	{
		int num = 4 * eID;
		int a = edges[num];
		int b = edges[num + 1];
		int ti = edges[num + 2];
		int num2 = edges[num + 3];
		int ii = IndexUtil.find_tri_other_vtx(a, b, triangles, ti);
		if (num2 != -1)
		{
			int jj = IndexUtil.find_tri_other_vtx(a, b, triangles, num2);
			return new Index2i(ii, jj);
		}
		return new Index2i(ii, -1);
	}

	public int FindTriangle(int a, int b, int c)
	{
		int num = find_edge(a, b);
		if (num == -1)
		{
			return -1;
		}
		int num2 = 4 * num;
		int num3 = 3 * edges[num2 + 2];
		if (triangles[num3] == c || triangles[num3 + 1] == c || triangles[num3 + 2] == c)
		{
			return edges[num2 + 2];
		}
		if (edges[num2 + 3] != -1)
		{
			num3 = 3 * edges[num2 + 3];
			if (triangles[num3] == c || triangles[num3 + 1] == c || triangles[num3 + 2] == c)
			{
				return edges[num2 + 3];
			}
		}
		return -1;
	}

	public IEnumerable<int> VtxVerticesItr(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			yield break;
		}
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			yield return edge_other_v(item, vID);
		}
	}

	public IEnumerable<int> VtxEdgesItr(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			return vertex_edges.ValueItr(vID);
		}
		return Enumerable.Empty<int>();
	}

	public int VtxBoundaryEdges(int vID, ref int e0, ref int e1)
	{
		if (vertices_refcount.isValid(vID))
		{
			int num = 0;
			{
				foreach (int item in vertex_edges.ValueItr(vID))
				{
					int num2 = 4 * item;
					if (edges[num2 + 3] == -1)
					{
						switch (num)
						{
						case 0:
							e0 = item;
							break;
						case 1:
							e1 = item;
							break;
						}
						num++;
					}
				}
				return num;
			}
		}
		return -1;
	}

	public int VtxAllBoundaryEdges(int vID, int[] e)
	{
		if (vertices_refcount.isValid(vID))
		{
			int result = 0;
			{
				foreach (int item in vertex_edges.ValueItr(vID))
				{
					int num = 4 * item;
					if (edges[num + 3] == -1)
					{
						e[result++] = item;
					}
				}
				return result;
			}
		}
		return -1;
	}

	public MeshResult GetVtxTriangles(int vID, List<int> vTriangles, bool bUseOrientation)
	{
		if (!IsVertex(vID))
		{
			return MeshResult.Failed_NotAVertex;
		}
		if (bUseOrientation)
		{
			foreach (int item2 in vertex_edges.ValueItr(vID))
			{
				int vB = edge_other_v(item2, vID);
				int num = 4 * item2;
				int num2 = edges[num + 2];
				if (tri_has_sequential_v(num2, vID, vB))
				{
					vTriangles.Add(num2);
				}
				int num3 = edges[num + 3];
				if (num3 != -1 && tri_has_sequential_v(num3, vID, vB))
				{
					vTriangles.Add(num3);
				}
			}
		}
		else
		{
			foreach (int item3 in vertex_edges.ValueItr(vID))
			{
				int num4 = 4 * item3;
				int item = edges[num4 + 2];
				if (!vTriangles.Contains(item))
				{
					vTriangles.Add(item);
				}
				int num5 = edges[num4 + 3];
				if (num5 != -1 && !vTriangles.Contains(num5))
				{
					vTriangles.Add(num5);
				}
			}
		}
		return MeshResult.Ok;
	}

	public int GetVtxTriangleCount(int vID, bool bBruteForce = false)
	{
		if (bBruteForce)
		{
			List<int> list = new List<int>();
			if (GetVtxTriangles(vID, list, bUseOrientation: false) != 0)
			{
				return -1;
			}
			return list.Count;
		}
		if (!IsVertex(vID))
		{
			return -1;
		}
		int num = 0;
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			int vB = edge_other_v(item, vID);
			int num2 = 4 * item;
			int tID = edges[num2 + 2];
			if (tri_has_sequential_v(tID, vID, vB))
			{
				num++;
			}
			int num3 = edges[num2 + 3];
			if (num3 != -1 && tri_has_sequential_v(num3, vID, vB))
			{
				num++;
			}
		}
		return num;
	}

	public IEnumerable<int> VtxTrianglesItr(int vID)
	{
		if (!IsVertex(vID))
		{
			yield break;
		}
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			int vOther = edge_other_v(item, vID);
			int i = 4 * item;
			int num = edges[i + 2];
			if (tri_has_sequential_v(num, vID, vOther))
			{
				yield return num;
			}
			int num2 = edges[i + 3];
			if (num2 != -1 && tri_has_sequential_v(num2, vID, vOther))
			{
				yield return num2;
			}
		}
	}

	public void GetVtxNbrhood(int eID, int vID, ref int vOther, ref int oppV1, ref int oppV2, ref int t1, ref int t2)
	{
		int num = 4 * eID;
		vOther = ((edges[num] == vID) ? edges[num + 1] : edges[num]);
		t1 = edges[num + 2];
		oppV1 = IndexUtil.find_tri_other_vtx(vID, vOther, triangles, t1);
		t2 = edges[num + 3];
		if (t2 != -1)
		{
			oppV2 = IndexUtil.find_tri_other_vtx(vID, vOther, triangles, t2);
		}
		else
		{
			t2 = -1;
		}
	}

	public void VtxOneRingCentroid(int vID, ref Vector3d centroid)
	{
		centroid = Vector3d.Zero;
		if (!vertices_refcount.isValid(vID))
		{
			return;
		}
		int num = 0;
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			int num2 = 3 * edge_other_v(item, vID);
			centroid.x += vertices[num2];
			centroid.y += vertices[num2 + 1];
			centroid.z += vertices[num2 + 2];
			num++;
		}
		if (num > 0)
		{
			double num3 = 1.0 / (double)num;
			centroid.x *= num3;
			centroid.y *= num3;
			centroid.z *= num3;
		}
	}

	public bool tri_has_v(int tID, int vID)
	{
		int num = 3 * tID;
		if (triangles[num] != vID && triangles[num + 1] != vID)
		{
			return triangles[num + 2] == vID;
		}
		return true;
	}

	public bool tri_is_boundary(int tID)
	{
		int num = 3 * tID;
		if (!IsBoundaryEdge(triangle_edges[num]) && !IsBoundaryEdge(triangle_edges[num + 1]))
		{
			return IsBoundaryEdge(triangle_edges[num + 2]);
		}
		return true;
	}

	public bool tri_has_neighbour_t(int tCheck, int tNbr)
	{
		int num = 3 * tCheck;
		if (!edge_has_t(triangle_edges[num], tNbr) && !edge_has_t(triangle_edges[num + 1], tNbr))
		{
			return edge_has_t(triangle_edges[num + 2], tNbr);
		}
		return true;
	}

	public bool tri_has_sequential_v(int tID, int vA, int vB)
	{
		int num = 3 * tID;
		int num2 = triangles[num];
		int num3 = triangles[num + 1];
		int num4 = triangles[num + 2];
		if (num2 == vA && num3 == vB)
		{
			return true;
		}
		if (num3 == vA && num4 == vB)
		{
			return true;
		}
		if (num4 == vA && num2 == vB)
		{
			return true;
		}
		return false;
	}

	public int find_tri_neighbour_edge(int tID, int vA, int vB)
	{
		int num = 3 * tID;
		int num2 = triangles[num];
		int num3 = triangles[num + 1];
		if (IndexUtil.same_pair_unordered(num2, num3, vA, vB))
		{
			return triangle_edges[3 * tID];
		}
		int num4 = triangles[num + 2];
		if (IndexUtil.same_pair_unordered(num3, num4, vA, vB))
		{
			return triangle_edges[3 * tID + 1];
		}
		if (IndexUtil.same_pair_unordered(num4, num2, vA, vB))
		{
			return triangle_edges[3 * tID + 2];
		}
		return -1;
	}

	public int find_tri_neighbour_index(int tID, int vA, int vB)
	{
		int num = 3 * tID;
		int num2 = triangles[num];
		int num3 = triangles[num + 1];
		if (IndexUtil.same_pair_unordered(num2, num3, vA, vB))
		{
			return 0;
		}
		int num4 = triangles[num + 2];
		if (IndexUtil.same_pair_unordered(num3, num4, vA, vB))
		{
			return 1;
		}
		if (IndexUtil.same_pair_unordered(num4, num2, vA, vB))
		{
			return 2;
		}
		return -1;
	}

	public bool IsBoundaryEdge(int eid)
	{
		return edges[4 * eid + 3] == -1;
	}

	[Obsolete("edge_is_boundary will be removed in future, use IsBoundaryEdge instead")]
	public bool edge_is_boundary(int eid)
	{
		return edges[4 * eid + 3] == -1;
	}

	public bool edge_has_v(int eid, int vid)
	{
		int num = 4 * eid;
		if (edges[num] != vid)
		{
			return edges[num + 1] == vid;
		}
		return true;
	}

	public bool edge_has_t(int eid, int tid)
	{
		int num = 4 * eid;
		if (edges[num + 2] != tid)
		{
			return edges[num + 3] == tid;
		}
		return true;
	}

	public int edge_other_v(int eID, int vID)
	{
		int num = 4 * eID;
		int num2 = edges[num];
		int num3 = edges[num + 1];
		if (num2 != vID)
		{
			if (num3 != vID)
			{
				return -1;
			}
			return num2;
		}
		return num3;
	}

	public int edge_other_t(int eID, int tid)
	{
		int num = 4 * eID;
		int num2 = edges[num + 2];
		int num3 = edges[num + 3];
		if (num2 != tid)
		{
			if (num3 != tid)
			{
				return -1;
			}
			return num2;
		}
		return num3;
	}

	[Obsolete("vertex_is_boundary will be removed in future, use IsBoundaryVertex instead")]
	public bool vertex_is_boundary(int vID)
	{
		return IsBoundaryVertex(vID);
	}

	public bool IsBoundaryVertex(int vID)
	{
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			if (edges[4 * item + 3] == -1)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBoundaryTriangle(int tID)
	{
		int num = 3 * tID;
		if (!IsBoundaryEdge(triangle_edges[num]) && !IsBoundaryEdge(triangle_edges[num + 1]))
		{
			return IsBoundaryEdge(triangle_edges[num + 2]);
		}
		return true;
	}

	private int find_edge(int vA, int vB)
	{
		int num = Math.Max(vA, vB);
		int list_index = Math.Min(vA, vB);
		foreach (int item in vertex_edges.ValueItr(list_index))
		{
			if (edges[4 * item + 1] == num)
			{
				return item;
			}
		}
		return -1;
	}

	private int find_edge_from_tri(int vA, int vB, int tID)
	{
		int num = 3 * tID;
		int num2 = triangles[num];
		int num3 = triangles[num + 1];
		if (IndexUtil.same_pair_unordered(vA, vB, num2, num3))
		{
			return triangle_edges[num];
		}
		int num4 = triangles[num + 2];
		if (IndexUtil.same_pair_unordered(vA, vB, num3, num4))
		{
			return triangle_edges[num + 1];
		}
		if (IndexUtil.same_pair_unordered(vA, vB, num4, num2))
		{
			return triangle_edges[num + 2];
		}
		return -1;
	}

	public bool IsGroupBoundaryEdge(int eID)
	{
		if (!IsEdge(eID))
		{
			throw new Exception("DMesh3.IsGroupBoundaryEdge: " + eID + " is not a valid edge");
		}
		if (triangle_groups == null)
		{
			throw new Exception("DMesh3.IsGroupBoundaryEdge: no triangle groups!");
		}
		int num = edges[4 * eID + 3];
		if (num == -1)
		{
			return false;
		}
		int num2 = triangle_groups[num];
		int i = edges[4 * eID + 2];
		int num3 = triangle_groups[i];
		return num2 != num3;
	}

	public bool IsGroupBoundaryVertex(int vID)
	{
		if (!IsVertex(vID))
		{
			throw new Exception("DMesh3.IsGroupBoundaryVertex: " + vID + " is not a valid vertex");
		}
		if (triangle_groups == null)
		{
			throw new Exception("DMesh3.IsGroupBoundaryVertex: no triangle groups!");
		}
		int num = int.MinValue;
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			int i = edges[4 * item + 2];
			int num2 = triangle_groups[i];
			if (num != num2)
			{
				if (num != int.MinValue)
				{
					return true;
				}
				num = num2;
			}
			int num3 = edges[4 * item + 3];
			if (num3 != -1)
			{
				int num4 = triangle_groups[num3];
				if (num != num4)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsGroupJunctionVertex(int vID)
	{
		if (!IsVertex(vID))
		{
			throw new Exception("DMesh3.IsGroupJunctionVertex: " + vID + " is not a valid vertex");
		}
		if (triangle_groups == null)
		{
			throw new Exception("DMesh3.IsGroupJunctionVertex: no triangle groups!");
		}
		Index2i max = Index2i.Max;
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			Index2i index2i = new Index2i(edges[4 * item + 2], edges[4 * item + 3]);
			for (int i = 0; i < 2; i++)
			{
				if (index2i[i] == -1)
				{
					continue;
				}
				int num = triangle_groups[index2i[i]];
				if (num != max.a && num != max.b)
				{
					if (max.a != Index2i.Max.a && max.b != Index2i.Max.b)
					{
						return true;
					}
					if (max.a == Index2i.Max.a)
					{
						max.a = num;
					}
					else
					{
						max.b = num;
					}
				}
			}
		}
		return false;
	}

	public bool GetVertexGroups(int vID, out Index4i groups)
	{
		groups = Index4i.Max;
		int num = 0;
		if (!IsVertex(vID))
		{
			throw new Exception("DMesh3.GetVertexGroups: " + vID + " is not a valid vertex");
		}
		if (triangle_groups == null)
		{
			throw new Exception("DMesh3.GetVertexGroups: no triangle groups!");
		}
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			int i = edges[4 * item + 2];
			int num2 = triangle_groups[i];
			if (!groups.Contains(num2))
			{
				groups[num++] = num2;
			}
			if (num == 4)
			{
				return false;
			}
			int num3 = edges[4 * item + 3];
			if (num3 != -1)
			{
				int num4 = triangle_groups[num3];
				if (!groups.Contains(num4))
				{
					groups[num++] = num4;
				}
				if (num == 4)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool GetAllVertexGroups(int vID, ref List<int> groups)
	{
		if (!IsVertex(vID))
		{
			throw new Exception("DMesh3.GetAllVertexGroups: " + vID + " is not a valid vertex");
		}
		if (triangle_groups == null)
		{
			throw new Exception("DMesh3.GetAllVertexGroups: no triangle groups!");
		}
		foreach (int item3 in vertex_edges.ValueItr(vID))
		{
			int i = edges[4 * item3 + 2];
			int item = triangle_groups[i];
			if (!groups.Contains(item))
			{
				groups.Add(item);
			}
			int num = edges[4 * item3 + 3];
			if (num != -1)
			{
				int item2 = triangle_groups[num];
				if (!groups.Contains(item2))
				{
					groups.Add(item2);
				}
			}
		}
		return true;
	}

	public List<int> GetAllVertexGroups(int vID)
	{
		List<int> groups = new List<int>();
		GetAllVertexGroups(vID, ref groups);
		return groups;
	}

	public bool IsBowtieVertex(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			int num = vertex_edges.Count(vID);
			if (num == 0)
			{
				return false;
			}
			int num2 = -1;
			bool flag = false;
			foreach (int item in vertex_edges.ValueItr(vID))
			{
				if (edges[4 * item + 3] == -1)
				{
					flag = true;
					num2 = item;
					break;
				}
			}
			if (num2 == -1)
			{
				num2 = vertex_edges.First(vID);
			}
			int num3 = edges[4 * num2 + 2];
			int num4 = num2;
			int num5 = 1;
			while (true)
			{
				int num6 = 3 * num3;
				Index3i tri_verts = new Index3i(triangles[num6], triangles[num6 + 1], triangles[num6 + 2]);
				Index3i index3i = new Index3i(triangle_edges[num6], triangle_edges[num6 + 1], triangle_edges[num6 + 2]);
				int num7 = IndexUtil.find_tri_index(vID, ref tri_verts);
				int num8 = index3i[num7];
				int num9 = index3i[(num7 + 2) % 3];
				int num10 = ((num8 == num4) ? num9 : num8);
				if (num10 == num2)
				{
					break;
				}
				Index2i edgeT = GetEdgeT(num10);
				int num11 = ((edgeT.a == num3) ? edgeT.b : edgeT.a);
				if (num11 == -1)
				{
					break;
				}
				num4 = num10;
				num3 = num11;
				num5++;
			}
			return (flag ? (num - 1) : num) != num5;
		}
		throw new Exception("DMesh3.IsBowtieVertex: " + vID + " is not a valid vertex");
	}

	public AxisAlignedBox3d GetBounds()
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		IEnumerator enumerator = vertices_refcount.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				int num4 = (int)enumerator.Current;
				num = vertices[3 * num4];
				num2 = vertices[3 * num4 + 1];
				num3 = vertices[3 * num4 + 2];
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		double num5 = num;
		double num6 = num;
		double num7 = num2;
		double num8 = num2;
		double num9 = num3;
		double num10 = num3;
		foreach (int item in vertices_refcount)
		{
			num = vertices[3 * item];
			num2 = vertices[3 * item + 1];
			num3 = vertices[3 * item + 2];
			if (num < num5)
			{
				num5 = num;
			}
			else if (num > num6)
			{
				num6 = num;
			}
			if (num2 < num7)
			{
				num7 = num2;
			}
			else if (num2 > num8)
			{
				num8 = num2;
			}
			if (num3 < num9)
			{
				num9 = num3;
			}
			else if (num3 > num10)
			{
				num10 = num3;
			}
		}
		return new AxisAlignedBox3d(num5, num7, num9, num6, num8, num10);
	}

	public bool IsClosed()
	{
		if (TriangleCount == 0)
		{
			return false;
		}
		if (MaxEdgeID / EdgeCount > 5)
		{
			foreach (int item in edges_refcount)
			{
				if (IsBoundaryEdge(item))
				{
					return false;
				}
			}
		}
		else
		{
			int maxEdgeID = MaxEdgeID;
			for (int i = 0; i < maxEdgeID; i++)
			{
				if (edges_refcount.isValid(i) && IsBoundaryEdge(i))
				{
					return false;
				}
			}
		}
		return true;
	}

	public double WindingNumber(Vector3d v)
	{
		double num = 0.0;
		foreach (int item in triangles_refcount)
		{
			num += GetTriSolidAngle(item, ref v);
		}
		return num / (Math.PI * 4.0);
	}

	public void AttachMetadata(string key, object o)
	{
		if (Metadata == null)
		{
			Metadata = new Dictionary<string, object>();
		}
		Metadata.Add(key, o);
	}

	public object FindMetadata(string key)
	{
		if (Metadata == null)
		{
			return null;
		}
		object value = null;
		if (!Metadata.TryGetValue(key, out value))
		{
			return null;
		}
		return value;
	}

	public bool RemoveMetadata(string key)
	{
		if (Metadata == null)
		{
			return false;
		}
		return Metadata.Remove(key);
	}

	public void ClearMetadata()
	{
		if (Metadata != null)
		{
			Metadata.Clear();
			Metadata = null;
		}
	}

	public void RebuildFromEdgeRefcounts()
	{
		int num = vertices.Length / 3;
		int num2 = triangles.Length / 3;
		triangle_edges.resize(triangles.Length);
		triangles_refcount.RawRefCounts.resize(num2);
		vertex_edges.Resize(num);
		vertices_refcount.RawRefCounts.resize(num);
		int num3 = edges.Length / 4;
		for (int i = 0; i < num3; i++)
		{
			if (edges_refcount.isValid(i))
			{
				int num4 = edges[4 * i];
				int num5 = edges[4 * i + 1];
				int num6 = edges[4 * i + 2];
				int num7 = edges[4 * i + 3];
				if (!vertices_refcount.isValidUnsafe(num4))
				{
					allocate_edges_list(num4);
					vertices_refcount.set_Unsafe(num4, 1);
				}
				if (!vertices_refcount.isValidUnsafe(num5))
				{
					allocate_edges_list(num5);
					vertices_refcount.set_Unsafe(num5, 1);
				}
				triangles_refcount.set_Unsafe(num6, 1);
				Index3i tri_verts = GetTriangle(num6);
				int num8 = IndexUtil.find_edge_index_in_tri(num4, num5, ref tri_verts);
				triangle_edges[3 * num6 + num8] = i;
				if (num7 != -1)
				{
					triangles_refcount.set_Unsafe(num7, 1);
					Index3i tri_verts2 = GetTriangle(num7);
					int num9 = IndexUtil.find_edge_index_in_tri(num4, num5, ref tri_verts2);
					triangle_edges[3 * num7 + num9] = i;
				}
				vertex_edges.Insert(num4, i);
				vertex_edges.Insert(num5, i);
			}
		}
		bool hasTriangleGroups = HasTriangleGroups;
		max_group_id = 0;
		for (int j = 0; j < num2; j++)
		{
			if (triangles_refcount.isValid(j))
			{
				int index = triangles[3 * j];
				int index2 = triangles[3 * j + 1];
				int index3 = triangles[3 * j + 2];
				vertices_refcount.increment(index, 1);
				vertices_refcount.increment(index2, 1);
				vertices_refcount.increment(index3, 1);
				if (hasTriangleGroups)
				{
					max_group_id = Math.Max(max_group_id, triangle_groups[j]);
				}
			}
		}
		max_group_id++;
		vertices_refcount.rebuild_free_list();
		triangles_refcount.rebuild_free_list();
		edges_refcount.rebuild_free_list();
		updateTimeStamp(bShapeChange: true);
	}

	public CompactInfo CompactInPlace(bool bComputeCompactInfo = false)
	{
		IndexMap indexMap = (bComputeCompactInfo ? new IndexMap(MaxVertexID, VertexCount) : null);
		CompactInfo result = default(CompactInfo);
		result.MapV = indexMap;
		int num = MaxVertexID - 1;
		int i = 0;
		while (!vertices_refcount.isValidUnsafe(num))
		{
			num--;
		}
		for (; vertices_refcount.isValidUnsafe(i); i++)
		{
		}
		DVector<short> rawRefCounts = vertices_refcount.RawRefCounts;
		while (i < num)
		{
			int num2 = i * 3;
			int num3 = num * 3;
			vertices[num2] = vertices[num3];
			vertices[num2 + 1] = vertices[num3 + 1];
			vertices[num2 + 2] = vertices[num3 + 2];
			if (normals != null)
			{
				normals[num2] = normals[num3];
				normals[num2 + 1] = normals[num3 + 1];
				normals[num2 + 2] = normals[num3 + 2];
			}
			if (colors != null)
			{
				colors[num2] = colors[num3];
				colors[num2 + 1] = colors[num3 + 1];
				colors[num2 + 2] = colors[num3 + 2];
			}
			if (uv != null)
			{
				int num4 = i * 2;
				int num5 = num * 2;
				uv[num4] = uv[num5];
				uv[num4 + 1] = uv[num5 + 1];
			}
			foreach (int item in vertex_edges.ValueItr(num))
			{
				replace_edge_vertex(item, num, i);
				int tID = edges[4 * item + 2];
				replace_tri_vertex(tID, num, i);
				int num6 = edges[4 * item + 3];
				if (num6 != -1)
				{
					replace_tri_vertex(num6, num, i);
				}
			}
			rawRefCounts[i] = rawRefCounts[num];
			rawRefCounts[num] = RefCountVector.invalid;
			vertex_edges.Move(num, i);
			if (indexMap != null)
			{
				indexMap[num] = i;
			}
			num--;
			i++;
			while (!vertices_refcount.isValidUnsafe(num))
			{
				num--;
			}
			for (; vertices_refcount.isValidUnsafe(i) && i < num; i++)
			{
			}
		}
		vertices_refcount.trim(VertexCount);
		vertices.resize(VertexCount * 3);
		if (normals != null)
		{
			normals.resize(VertexCount * 3);
		}
		if (colors != null)
		{
			colors.resize(VertexCount * 3);
		}
		if (uv != null)
		{
			uv.resize(VertexCount * 2);
		}
		int num7 = MaxTriangleID - 1;
		int j = 0;
		while (!triangles_refcount.isValidUnsafe(num7))
		{
			num7--;
		}
		for (; triangles_refcount.isValidUnsafe(j); j++)
		{
		}
		DVector<short> rawRefCounts2 = triangles_refcount.RawRefCounts;
		while (j < num7)
		{
			int num8 = j * 3;
			int num9 = num7 * 3;
			for (int k = 0; k < 3; k++)
			{
				triangles[num8 + k] = triangles[num9 + k];
				triangle_edges[num8 + k] = triangle_edges[num9 + k];
			}
			if (triangle_groups != null)
			{
				triangle_groups[j] = triangle_groups[num7];
			}
			for (int l = 0; l < 3; l++)
			{
				int eID = triangle_edges[num8 + l];
				replace_edge_triangle(eID, num7, j);
			}
			rawRefCounts2[j] = rawRefCounts2[num7];
			rawRefCounts2[num7] = RefCountVector.invalid;
			num7--;
			j++;
			while (!triangles_refcount.isValidUnsafe(num7))
			{
				num7--;
			}
			for (; triangles_refcount.isValidUnsafe(j) && j < num7; j++)
			{
			}
		}
		triangles_refcount.trim(TriangleCount);
		triangles.resize(TriangleCount * 3);
		triangle_edges.resize(TriangleCount * 3);
		if (triangle_groups != null)
		{
			triangle_groups.resize(TriangleCount);
		}
		int iLastE = MaxEdgeID - 1;
		int m = 0;
		for (; !edges_refcount.isValidUnsafe(iLastE); iLastE--)
		{
		}
		for (; edges_refcount.isValidUnsafe(m); m++)
		{
		}
		DVector<short> rawRefCounts3 = edges_refcount.RawRefCounts;
		while (m < iLastE)
		{
			int num10 = m * 4;
			int num11 = iLastE * 4;
			for (int n = 0; n < 4; n++)
			{
				edges[num10 + n] = edges[num11 + n];
			}
			int list_index = edges[num10];
			int list_index2 = edges[num10 + 1];
			vertex_edges.Replace(list_index, (int eid) => eid == iLastE, m);
			vertex_edges.Replace(list_index2, (int eid) => eid == iLastE, m);
			replace_triangle_edge(edges[num10 + 2], iLastE, m);
			if (edges[num10 + 3] != -1)
			{
				replace_triangle_edge(edges[num10 + 3], iLastE, m);
			}
			rawRefCounts3[m] = rawRefCounts3[iLastE];
			rawRefCounts3[iLastE] = RefCountVector.invalid;
			iLastE--;
			m++;
			for (; !edges_refcount.isValidUnsafe(iLastE); iLastE--)
			{
			}
			for (; edges_refcount.isValidUnsafe(m) && m < iLastE; m++)
			{
			}
		}
		edges_refcount.trim(EdgeCount);
		edges.resize(EdgeCount * 4);
		return result;
	}

	public static explicit operator Mesh(DMesh3 mesh)
	{
		if (!mesh.Clockwise)
		{
			mesh.ReverseOrientation();
		}
		Mesh mesh2 = new Mesh();
		mesh2.MarkDynamic();
		if (mesh.VertexCount > 64000 || mesh.TriangleCount > 64000)
		{
			mesh2.indexFormat = IndexFormat.UInt32;
		}
		Vector3[] array = new Vector3[mesh.VertexCount];
		Color[] array2 = new Color[mesh.VertexCount];
		Vector2[] array3 = new Vector2[mesh.VertexCount];
		Vector3[] array4 = new Vector3[mesh.VertexCount];
		for (int i = 0; i < mesh.VertexCount; i++)
		{
			if (mesh.IsVertex(i))
			{
				NewVertexInfo vertexAll = mesh.GetVertexAll(i);
				array[i] = (Vector3)vertexAll.v;
				if (vertexAll.bHaveC)
				{
					array2[i] = vertexAll.c;
				}
				if (vertexAll.bHaveUV)
				{
					array3[i] = vertexAll.uv;
				}
				if (vertexAll.bHaveN)
				{
					array4[i] = vertexAll.n;
				}
			}
		}
		mesh2.vertices = array;
		if (mesh.HasVertexColors)
		{
			mesh2.SetColors(array2);
		}
		if (mesh.HasVertexUVs)
		{
			mesh2.SetUVs(0, array3);
		}
		int[] array5 = new int[mesh.TriangleCount * 3];
		int num = 0;
		foreach (Index3i item in mesh.Triangles())
		{
			array5[num * 3] = item.a;
			array5[num * 3 + 1] = item.b;
			array5[num * 3 + 2] = item.c;
			num++;
		}
		mesh2.triangles = array5;
		if (mesh.HasVertexNormals)
		{
			mesh2.SetNormals(array4);
		}
		else
		{
			mesh2.RecalculateNormals();
		}
		mesh2.RecalculateBounds();
		mesh2.RecalculateTangents();
		return mesh2;
	}

	public static explicit operator DMesh3(Mesh mesh)
	{
		DMesh3 dMesh = new DMesh3();
		dMesh.Clockwise = true;
		Vector3[] array = mesh.vertices;
		foreach (Vector3 vector in array)
		{
			dMesh.AppendVertex(vector);
		}
		int[] array2 = mesh.triangles;
		for (int j = 0; j < array2.Length; j += 3)
		{
			dMesh.AppendTriangle(array2[j], array2[j + 1], array2[j + 2]);
		}
		dMesh.ReverseOrientation();
		return dMesh;
	}

	public void CalculateUVs()
	{
		EnableVertexUVs(Vector2f.Zero);
		OrthogonalPlaneFit3 orthogonalPlaneFit = new OrthogonalPlaneFit3(Vertices());
		Frame3f frame3f = new Frame3f(orthogonalPlaneFit.Origin, orthogonalPlaneFit.Normal);
		AxisAlignedBox3d cachedBounds = CachedBounds;
		AxisAlignedBox2d axisAlignedBox2d = default(AxisAlignedBox2d);
		for (int i = 0; i < 8; i++)
		{
			axisAlignedBox2d.Contain(frame3f.ToPlaneUV((Vector3f)cachedBounds.Corner(i), 3));
		}
		Vector2f vector2f = (Vector2f)axisAlignedBox2d.Min;
		float num = (float)axisAlignedBox2d.Width;
		float num2 = (float)axisAlignedBox2d.Height;
		for (int j = 0; j < VertexCount; j++)
		{
			Vector2f vNewUV = frame3f.ToPlaneUV((Vector3f)GetVertex(j), 3);
			vNewUV.x = (vNewUV.x - vector2f.x) / num;
			vNewUV.y = (vNewUV.y - vector2f.y) / num2;
			SetVertexUV(j, vNewUV);
		}
	}

	public Task<int> CalculateUVsAsync()
	{
		TaskCompletionSource<int> tcs1 = new TaskCompletionSource<int>();
		Task<int> task = tcs1.Task;
		task.ConfigureAwait(continueOnCapturedContext: false);
		Task.Factory.StartNew(delegate
		{
			CalculateUVs();
			tcs1.SetResult(1);
		});
		return task;
	}

	public int[] Colorisation()
	{
		int[] array = new int[VertexCount];
		int[] array2 = new int[6] { 1, 2, 3, 4, 5, 6 };
		int[] array3 = new int[6] { 3, 4, 5, 0, 1, 2 };
		foreach (Index3i item in Triangles())
		{
			int[] array4 = new int[6];
			int[] array5 = item.array;
			int[] array6 = array5;
			foreach (int num in array6)
			{
				int num2 = array[num];
				if (num2 == 0)
				{
					continue;
				}
				for (int j = 0; j < 6; j++)
				{
					if (num2 == array2[j])
					{
						array4[j]++;
						break;
					}
				}
			}
			if (array4.Max() > 1)
			{
				for (int k = 0; k < 6; k++)
				{
					if (array4[k] <= 1)
					{
						continue;
					}
					array6 = array5;
					foreach (int num3 in array6)
					{
						if (array[num3] == array2[k])
						{
							array[num3] = array2[array3[k]];
							break;
						}
					}
					array4[k]--;
					array4[array3[k]]++;
				}
			}
			while (array4.Sum() < 3)
			{
				array6 = array5;
				foreach (int num4 in array6)
				{
					if (array[num4] != 0)
					{
						continue;
					}
					for (int l = 0; l < 6; l++)
					{
						if (array4[l] == 0 && array4[array3[l]] <= 0)
						{
							array[num4] = array2[l];
							array4[l]++;
							break;
						}
					}
				}
			}
		}
		return array;
	}

	public void Colorisation(out Vector2[] uv)
	{
		int[] array = Colorisation();
		uv = new Vector2[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			switch (array[i])
			{
			case 1:
			case 4:
				uv[i] = new Vector2(1f, 0f);
				break;
			case 2:
			case 5:
				uv[i] = new Vector2(2f, 0f);
				break;
			case 3:
			case 6:
				uv[i] = new Vector2(3f, 0f);
				break;
			default:
				throw new Exception("Invalid Color in colorisation");
			}
		}
	}

	[Conditional("DEBUG")]
	public void debug_check_is_vertex(int v)
	{
		if (!IsVertex(v))
		{
			throw new Exception("DMesh3.debug_is_vertex - not a vertex!");
		}
	}

	[Conditional("DEBUG")]
	public void debug_check_is_triangle(int t)
	{
		if (!IsTriangle(t))
		{
			throw new Exception("DMesh3.debug_is_triangle - not a triangle!");
		}
	}

	[Conditional("DEBUG")]
	public void debug_check_is_edge(int e)
	{
		if (!IsEdge(e))
		{
			throw new Exception("DMesh3.debug_is_edge - not an edge!");
		}
	}

	public void debug_print_vertex(int v)
	{
		Console.WriteLine("Vertex " + v);
		List<int> list = new List<int>();
		GetVtxTriangles(v, list, bUseOrientation: false);
		Console.WriteLine($"  Tris {list.Count}  Edges {GetVtxEdgeCount(v)}  refcount {vertices_refcount.refCount(v)}");
		foreach (int item in list)
		{
			Index3i triangle = GetTriangle(item);
			Index3i triEdges = GetTriEdges(item);
			Console.WriteLine(string.Format("  t{6} {0} {1} {2}   te {3} {4} {5}", triangle[0], triangle[1], triangle[2], triEdges[0], triEdges[1], triEdges[2], item));
		}
		foreach (int item2 in VtxEdgesItr(v))
		{
			Index2i edgeV = GetEdgeV(item2);
			Index2i edgeT = GetEdgeT(item2);
			Console.WriteLine(string.Format("  e{4} {0} {1} / {2} {3}", edgeV[0], edgeV[1], edgeT[0], edgeT[1], item2));
		}
	}

	public void debug_print_mesh()
	{
		for (int i = 0; i < vertices_refcount.max_index; i++)
		{
			if (!vertices_refcount.isValid(i))
			{
				Console.WriteLine($"v{i} : invalid");
			}
			else
			{
				debug_print_vertex(i);
			}
		}
	}

	public string MeshInfoString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Vertices  count {0} max {1} {2}", VertexCount, MaxVertexID, vertices_refcount.UsageStats);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Triangles count {0} max {1} {2}", TriangleCount, MaxTriangleID, triangles_refcount.UsageStats);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Edges     count {0} max {1} {2}", EdgeCount, MaxEdgeID, edges_refcount.UsageStats);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Normals {0}  Colors {1}  UVs {2}  Groups {3}", HasVertexNormals, HasVertexColors, HasVertexUVs, HasTriangleGroups);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Closed {0} Compact {1} timestamp {2} shape_timestamp {3}  MaxGroupID {4}", CachedIsClosed, IsCompact, timestamp, shape_timestamp, max_group_id);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("VertexEdges " + vertex_edges.MemoryUsage);
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	public bool IsSameMesh(DMesh3 m2, bool bCheckConnectivity, bool bCheckEdgeIDs = false, bool bCheckNormals = false, bool bCheckColors = false, bool bCheckUVs = false, bool bCheckGroups = false, float Epsilon = 1.1920929E-07f)
	{
		if (VertexCount != m2.VertexCount)
		{
			return false;
		}
		if (TriangleCount != m2.TriangleCount)
		{
			return false;
		}
		foreach (int item in VertexIndices())
		{
			if (!m2.IsVertex(item) || !GetVertex(item).EpsilonEqual(m2.GetVertex(item), Epsilon))
			{
				return false;
			}
		}
		foreach (int item2 in TriangleIndices())
		{
			if (!m2.IsTriangle(item2) || !GetTriangle(item2).Equals(m2.GetTriangle(item2)))
			{
				return false;
			}
		}
		if (bCheckConnectivity)
		{
			foreach (int item3 in EdgeIndices())
			{
				Index4i edge = GetEdge(item3);
				int num = m2.FindEdge(edge.a, edge.b);
				if (num == -1)
				{
					return false;
				}
				Index4i edge2 = m2.GetEdge(num);
				if (Math.Min(edge.c, edge.d) != Math.Min(edge2.c, edge2.d) || Math.Max(edge.c, edge.d) != Math.Max(edge2.c, edge2.d))
				{
					return false;
				}
			}
		}
		if (bCheckEdgeIDs)
		{
			if (EdgeCount != m2.EdgeCount)
			{
				return false;
			}
			foreach (int item4 in EdgeIndices())
			{
				if (!m2.IsEdge(item4) || !GetEdge(item4).Equals(m2.GetEdge(item4)))
				{
					return false;
				}
			}
		}
		if (bCheckNormals)
		{
			if (HasVertexNormals != m2.HasVertexNormals)
			{
				return false;
			}
			if (HasVertexNormals)
			{
				foreach (int item5 in VertexIndices())
				{
					if (!GetVertexNormal(item5).EpsilonEqual(m2.GetVertexNormal(item5), Epsilon))
					{
						return false;
					}
				}
			}
		}
		if (bCheckColors)
		{
			if (HasVertexColors != m2.HasVertexColors)
			{
				return false;
			}
			if (HasVertexColors)
			{
				foreach (int item6 in VertexIndices())
				{
					if (!GetVertexColor(item6).EpsilonEqual(m2.GetVertexColor(item6), Epsilon))
					{
						return false;
					}
				}
			}
		}
		if (bCheckUVs)
		{
			if (HasVertexUVs != m2.HasVertexUVs)
			{
				return false;
			}
			if (HasVertexUVs)
			{
				foreach (int item7 in VertexIndices())
				{
					if (!GetVertexUV(item7).EpsilonEqual(m2.GetVertexUV(item7), Epsilon))
					{
						return false;
					}
				}
			}
		}
		if (bCheckGroups)
		{
			if (HasTriangleGroups != m2.HasTriangleGroups)
			{
				return false;
			}
			if (HasTriangleGroups)
			{
				foreach (int item8 in TriangleIndices())
				{
					if (GetTriangleGroup(item8) != m2.GetTriangleGroup(item8))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool CheckValidity(bool bAllowNonManifoldVertices = false, FailMode eFailMode = FailMode.Throw)
	{
		int[] array = new int[MaxVertexID];
		bool is_ok = true;
		Action<bool> action = delegate(bool b)
		{
			is_ok &= b;
		};
		switch (eFailMode)
		{
		case FailMode.DebugAssert:
			action = delegate(bool b)
			{
				is_ok &= b;
			};
			break;
		case FailMode.gDevAssert:
			action = delegate(bool b)
			{
				is_ok &= b;
			};
			break;
		case FailMode.Throw:
			action = delegate(bool b)
			{
				if (!b)
				{
					throw new Exception("DMesh3.CheckValidity: check failed");
				}
			};
			break;
		}
		if (normals != null)
		{
			action(normals.size == vertices.size);
		}
		if (colors != null)
		{
			action(colors.size == vertices.size);
		}
		if (uv != null)
		{
			action(uv.size / 2 == vertices.size / 3);
		}
		if (triangle_groups != null)
		{
			action(triangle_groups.size == triangles.size / 3);
		}
		foreach (int item in TriangleIndices())
		{
			action(IsTriangle(item));
			action(triangles_refcount.refCount(item) == 1);
			Index3i triangle = GetTriangle(item);
			for (int i = 0; i < 3; i++)
			{
				action(IsVertex(triangle[i]));
				array[triangle[i]]++;
			}
			Index3i index3i = default(Index3i);
			for (int j = 0; j < 3; j++)
			{
				int vA = triangle[j];
				int vB = triangle[(j + 1) % 3];
				index3i[j] = FindEdge(vA, vB);
				action(index3i[j] != -1);
				action(edge_has_t(index3i[j], item));
				action(index3i[j] == FindEdgeFromTri(vA, vB, item));
			}
			action(index3i[0] != index3i[1] && index3i[0] != index3i[2] && index3i[1] != index3i[2]);
			Index3i triEdges = GetTriEdges(item);
			for (int k = 0; k < 3; k++)
			{
				int eID = triEdges[k];
				action(IsEdge(eID));
				int num = edge_other_t(eID, item);
				if (num == -1)
				{
					action(tri_is_boundary(item));
					continue;
				}
				action(tri_has_neighbour_t(num, item));
				int num2 = triangle[k];
				int num3 = triangle[(k + 1) % 3];
				Index2i edgeV = GetEdgeV(triEdges[k]);
				action(IndexUtil.same_pair_unordered(num2, num3, edgeV[0], edgeV[1]));
				int num4 = IndexUtil.find_tri_ordered_edge(num3, num2, GetTriangle(num).array);
				action(num4 != -1);
			}
		}
		foreach (int item2 in EdgeIndices())
		{
			action(IsEdge(item2));
			action(edges_refcount.refCount(item2) == 1);
			Index2i edgeV2 = GetEdgeV(item2);
			Index2i edgeT = GetEdgeT(item2);
			action(IsVertex(edgeV2[0]));
			action(IsVertex(edgeV2[1]));
			action(edgeT[0] != -1);
			action(edgeV2[0] < edgeV2[1]);
			action(IsTriangle(edgeT[0]));
			if (edgeT[1] != -1)
			{
				action(IsTriangle(edgeT[1]));
			}
		}
		if (vertices_refcount.is_dense)
		{
			for (int l = 0; l < vertices.Length / 3; l++)
			{
				action(vertices_refcount.isValid(l));
			}
		}
		foreach (int item3 in VertexIndices())
		{
			action(IsVertex(item3));
			Vector3d vertex = GetVertex(item3);
			action(!double.IsNaN(vertex.LengthSquared));
			action(!double.IsInfinity(vertex.LengthSquared));
			foreach (int item4 in vertex_edges.ValueItr(item3))
			{
				action(IsEdge(item4));
				action(edge_has_v(item4, item3));
				int num5 = edge_other_v(item4, item3);
				int num6 = find_edge(item3, num5);
				action(num6 != -1);
				action(num6 == item4);
				num6 = find_edge(num5, item3);
				action(num6 != -1);
				action(num6 == item4);
			}
			foreach (int item5 in VtxVerticesItr(item3))
			{
				action(IsVertex(item5));
				int eID2 = find_edge(item3, item5);
				action(IsEdge(eID2));
			}
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			GetVtxTriangles(item3, list, bUseOrientation: false);
			GetVtxTriangles(item3, list2, bUseOrientation: true);
			action(list.Count == list2.Count);
			if (bAllowNonManifoldVertices)
			{
				action(list.Count <= GetVtxEdgeCount(item3));
			}
			else
			{
				action(list.Count == GetVtxEdgeCount(item3) || list.Count == GetVtxEdgeCount(item3) - 1);
			}
			action(vertices_refcount.refCount(item3) == list.Count + 1);
			action(array[item3] == list.Count);
			foreach (int item6 in list)
			{
				action(tri_has_v(item6, item3));
			}
			List<int> list3 = new List<int>(list);
			foreach (int item7 in vertex_edges.ValueItr(item3))
			{
				Index2i edgeT2 = GetEdgeT(item7);
				action(list.Contains(edgeT2[0]));
				if (edgeT2[1] != -1)
				{
					action(list.Contains(edgeT2[1]));
				}
				list3.Remove(edgeT2[0]);
				if (edgeT2[1] != -1)
				{
					list3.Remove(edgeT2[1]);
				}
			}
			action(list3.Count == 0);
		}
		return is_ok;
	}

	public MeshResult ReverseTriOrientation(int tID)
	{
		if (!IsTriangle(tID))
		{
			return MeshResult.Failed_NotATriangle;
		}
		internal_reverse_tri_orientation(tID);
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	private void internal_reverse_tri_orientation(int tID)
	{
		Index3i triangle = GetTriangle(tID);
		set_triangle(tID, triangle[1], triangle[0], triangle[2]);
		Index3i triEdges = GetTriEdges(tID);
		set_triangle_edges(tID, triEdges[0], triEdges[2], triEdges[1]);
	}

	public void ReverseOrientation(bool bFlipNormals = true)
	{
		foreach (int item in TriangleIndices())
		{
			internal_reverse_tri_orientation(item);
		}
		if (bFlipNormals && HasVertexNormals)
		{
			foreach (int item2 in VertexIndices())
			{
				int num = 3 * item2;
				normals[num] = 0f - normals[num];
				normals[num + 1] = 0f - normals[num + 1];
				normals[num + 2] = 0f - normals[num + 2];
			}
		}
		updateTimeStamp(bShapeChange: true);
		Clockwise = !Clockwise;
	}

	public MeshResult RemoveVertex(int vID, bool bRemoveAllTriangles = true, bool bPreserveManifold = false)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return MeshResult.Failed_NotAVertex;
		}
		if (bRemoveAllTriangles)
		{
			if (bPreserveManifold)
			{
				foreach (int item in VtxTrianglesItr(vID))
				{
					Index3i tri_verts = GetTriangle(item);
					int num = IndexUtil.find_tri_index(vID, ref tri_verts);
					int num2 = tri_verts[(num + 1) % 3];
					int num3 = tri_verts[(num + 2) % 3];
					int eid = find_edge(num2, num3);
					if (!IsBoundaryEdge(eid) && (IsBoundaryVertex(num2) || IsBoundaryVertex(num3)))
					{
						return MeshResult.Failed_WouldCreateBowtie;
					}
				}
			}
			List<int> list = new List<int>();
			GetVtxTriangles(vID, list, bUseOrientation: true);
			foreach (int item2 in list)
			{
				MeshResult meshResult = RemoveTriangle(item2, bRemoveIsolatedVertices: false, bPreserveManifold);
				if (meshResult != 0)
				{
					return meshResult;
				}
			}
		}
		if (vertices_refcount.refCount(vID) != 1)
		{
			throw new NotImplementedException("DMesh3.RemoveVertex: vertex is still referenced");
		}
		vertices_refcount.decrement(vID, 1);
		vertex_edges.Clear(vID);
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public MeshResult RemoveTriangle(int tID, bool bRemoveIsolatedVertices = true, bool bPreserveManifold = false)
	{
		if (!triangles_refcount.isValid(tID))
		{
			return MeshResult.Failed_NotATriangle;
		}
		Index3i triangle = GetTriangle(tID);
		Index3i triEdges = GetTriEdges(tID);
		if (bPreserveManifold)
		{
			for (int i = 0; i < 3; i++)
			{
				if (IsBoundaryVertex(triangle[i]) && !IsBoundaryEdge(triEdges[i]) && !IsBoundaryEdge(triEdges[(i + 2) % 3]))
				{
					return MeshResult.Failed_WouldCreateBowtie;
				}
			}
		}
		for (int j = 0; j < 3; j++)
		{
			int num = triEdges[j];
			replace_edge_triangle(num, tID, -1);
			if (edges[4 * num + 2] == -1)
			{
				int list_index = edges[4 * num];
				vertex_edges.Remove(list_index, num);
				int list_index2 = edges[4 * num + 1];
				vertex_edges.Remove(list_index2, num);
				edges_refcount.decrement(num, 1);
			}
		}
		triangles_refcount.decrement(tID, 1);
		for (int k = 0; k < 3; k++)
		{
			int num2 = triangle[k];
			vertices_refcount.decrement(num2, 1);
			if (bRemoveIsolatedVertices && vertices_refcount.refCount(num2) == 1)
			{
				vertices_refcount.decrement(num2, 1);
				vertex_edges.Clear(num2);
			}
		}
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public virtual MeshResult SetTriangle(int tID, Index3i newv, bool bRemoveIsolatedVertices = true)
	{
		Index3i triangle = GetTriangle(tID);
		Index3i triEdges = GetTriEdges(tID);
		if (triangle.a == newv.a && triangle.b == newv.b)
		{
			triEdges.a = -1;
		}
		if (triangle.b == newv.b && triangle.c == newv.c)
		{
			triEdges.b = -1;
		}
		if (triangle.c == newv.c && triangle.a == newv.a)
		{
			triEdges.c = -1;
		}
		if (!triangles_refcount.isValid(tID))
		{
			return MeshResult.Failed_NotATriangle;
		}
		if (!IsVertex(newv[0]) || !IsVertex(newv[1]) || !IsVertex(newv[2]))
		{
			return MeshResult.Failed_NotAVertex;
		}
		if (newv[0] == newv[1] || newv[0] == newv[2] || newv[1] == newv[2])
		{
			return MeshResult.Failed_BrokenTopology;
		}
		int num = find_edge(newv[0], newv[1]);
		int num2 = find_edge(newv[1], newv[2]);
		int num3 = find_edge(newv[2], newv[0]);
		if ((triEdges.a != -1 && num != -1 && !IsBoundaryEdge(num)) || (triEdges.b != -1 && num2 != -1 && !IsBoundaryEdge(num2)) || (triEdges.c != -1 && num3 != -1 && !IsBoundaryEdge(num3)))
		{
			return MeshResult.Failed_BrokenTopology;
		}
		for (int i = 0; i < 3; i++)
		{
			int num4 = triEdges[i];
			if (num4 != -1)
			{
				replace_edge_triangle(num4, tID, -1);
				if (edges[4 * num4 + 2] == -1)
				{
					int list_index = edges[4 * num4];
					vertex_edges.Remove(list_index, num4);
					int list_index2 = edges[4 * num4 + 1];
					vertex_edges.Remove(list_index2, num4);
					edges_refcount.decrement(num4, 1);
				}
			}
		}
		for (int j = 0; j < 3; j++)
		{
			int num5 = triangle[j];
			if (num5 != newv[j])
			{
				vertices_refcount.decrement(num5, 1);
				if (bRemoveIsolatedVertices && vertices_refcount.refCount(num5) == 1)
				{
					vertices_refcount.decrement(num5, 1);
					vertex_edges.Clear(num5);
				}
			}
		}
		int num6 = 3 * tID;
		for (int k = 0; k < 3; k++)
		{
			if (newv[k] != triangle[k])
			{
				triangles[num6 + k] = newv[k];
				vertices_refcount.increment(newv[k], 1);
			}
		}
		if (triEdges.a != -1)
		{
			add_tri_edge(tID, newv[0], newv[1], 0, num);
		}
		if (triEdges.b != -1)
		{
			add_tri_edge(tID, newv[1], newv[2], 1, num2);
		}
		if (triEdges.c != -1)
		{
			add_tri_edge(tID, newv[2], newv[0], 2, num3);
		}
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public MeshResult SplitEdge(int vA, int vB, out EdgeSplitInfo split)
	{
		int num = find_edge(vA, vB);
		if (num == -1)
		{
			split = default(EdgeSplitInfo);
			return MeshResult.Failed_NotAnEdge;
		}
		return SplitEdge(num, out split);
	}

	public MeshResult SplitEdge(int eab, out EdgeSplitInfo split, double split_t = 0.5)
	{
		split = default(EdgeSplitInfo);
		if (!IsEdge(eab))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		int num = 4 * eab;
		int a = edges[num];
		int b = edges[num + 1];
		int num2 = edges[num + 2];
		if (num2 == -1)
		{
			return MeshResult.Failed_BrokenTopology;
		}
		int[] array = GetTriangle(num2).array;
		int num3 = IndexUtil.orient_tri_edge_and_find_other_vtx(ref a, ref b, array);
		if (vertices_refcount.rawRefCount(num3) > 32764)
		{
			return MeshResult.Failed_HitValenceLimit;
		}
		if (a != edges[num])
		{
			split_t = 1.0 - split_t;
		}
		if (IsBoundaryEdge(eab))
		{
			Vector3d v = Vector3d.Lerp(GetVertex(a), GetVertex(b), split_t);
			int num4 = AppendVertex(v);
			if (HasVertexNormals)
			{
				SetVertexNormal(num4, Vector3f.Lerp(GetVertexNormal(a), GetVertexNormal(b), (float)split_t).Normalized);
			}
			if (HasVertexColors)
			{
				SetVertexColor(num4, Colorf.Lerp(GetVertexColor(a), GetVertexColor(b), (float)split_t));
			}
			if (HasVertexUVs)
			{
				SetVertexUV(num4, Vector2f.Lerp(GetVertexUV(a), GetVertexUV(b), (float)split_t));
			}
			int num5 = GetTriEdges(num2)[IndexUtil.find_edge_index_in_tri(b, num3, array)];
			replace_tri_vertex(num2, b, num4);
			int num6 = add_triangle_only(num4, b, num3, -1, -1, -1);
			if (triangle_groups != null)
			{
				triangle_groups.insert(triangle_groups[num2], num6);
			}
			replace_edge_triangle(num5, num2, num6);
			replace_edge_vertex(eab, b, num4);
			vertex_edges.Remove(b, eab);
			vertex_edges.Insert(num4, eab);
			int num7 = add_edge(num4, b, num6);
			int num8 = add_edge(num4, num3, num2, num6);
			replace_triangle_edge(num2, num5, num8);
			set_triangle_edges(num6, num7, num5, num8);
			vertices_refcount.increment(num3, 1);
			vertices_refcount.increment(num4, 2);
			split.bIsBoundary = true;
			split.vNew = num4;
			split.eNewBN = num7;
			split.eNewCN = num8;
			split.eNewDN = -1;
			split.eNewT2 = num6;
			split.eNewT3 = -1;
			updateTimeStamp(bShapeChange: true);
			return MeshResult.Ok;
		}
		int num9 = edges[num + 3];
		int[] array2 = GetTriangle(num9).array;
		int num10 = IndexUtil.find_tri_other_vtx(a, b, array2);
		if (vertices_refcount.rawRefCount(num10) > 32764)
		{
			return MeshResult.Failed_HitValenceLimit;
		}
		Vector3d v2 = Vector3d.Lerp(GetVertex(a), GetVertex(b), split_t);
		int num11 = AppendVertex(v2);
		if (HasVertexNormals)
		{
			SetVertexNormal(num11, Vector3f.Lerp(GetVertexNormal(a), GetVertexNormal(b), (float)split_t).Normalized);
		}
		if (HasVertexColors)
		{
			SetVertexColor(num11, Colorf.Lerp(GetVertexColor(a), GetVertexColor(b), (float)split_t));
		}
		if (HasVertexUVs)
		{
			SetVertexUV(num11, Vector2f.Lerp(GetVertexUV(a), GetVertexUV(b), (float)split_t));
		}
		int num12 = GetTriEdges(num2)[IndexUtil.find_edge_index_in_tri(b, num3, array)];
		int num13 = GetTriEdges(num9)[IndexUtil.find_edge_index_in_tri(num10, b, array2)];
		replace_tri_vertex(num2, b, num11);
		replace_tri_vertex(num9, b, num11);
		int num14 = add_triangle_only(num11, b, num3, -1, -1, -1);
		int num15 = add_triangle_only(num11, num10, b, -1, -1, -1);
		if (triangle_groups != null)
		{
			triangle_groups.insert(triangle_groups[num2], num14);
			triangle_groups.insert(triangle_groups[num9], num15);
		}
		replace_edge_triangle(num12, num2, num14);
		replace_edge_triangle(num13, num9, num15);
		replace_edge_vertex(eab, b, num11);
		vertex_edges.Remove(b, eab);
		vertex_edges.Insert(num11, eab);
		int num16 = add_edge(num11, b, num14, num15);
		int num17 = add_edge(num11, num3, num2, num14);
		int num18 = add_edge(num10, num11, num9, num15);
		replace_triangle_edge(num2, num12, num17);
		replace_triangle_edge(num9, num13, num18);
		set_triangle_edges(num14, num16, num12, num17);
		set_triangle_edges(num15, num18, num13, num16);
		vertices_refcount.increment(num3, 1);
		vertices_refcount.increment(num10, 1);
		vertices_refcount.increment(num11, 4);
		split.bIsBoundary = false;
		split.vNew = num11;
		split.eNewBN = num16;
		split.eNewCN = num17;
		split.eNewDN = num18;
		split.eNewT2 = num14;
		split.eNewT3 = num15;
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public MeshResult FlipEdge(int vA, int vB, out EdgeFlipInfo flip)
	{
		int num = find_edge(vA, vB);
		if (num == -1)
		{
			flip = default(EdgeFlipInfo);
			return MeshResult.Failed_NotAnEdge;
		}
		return FlipEdge(num, out flip);
	}

	public MeshResult FlipEdge(int eab, out EdgeFlipInfo flip)
	{
		flip = default(EdgeFlipInfo);
		if (!IsEdge(eab))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		if (IsBoundaryEdge(eab))
		{
			return MeshResult.Failed_IsBoundaryEdge;
		}
		int num = 4 * eab;
		int a = edges[num];
		int b = edges[num + 1];
		int num2 = edges[num + 2];
		int num3 = edges[num + 3];
		int[] array = GetTriangle(num2).array;
		int[] array2 = GetTriangle(num3).array;
		int num4 = IndexUtil.orient_tri_edge_and_find_other_vtx(ref a, ref b, array);
		int num5 = IndexUtil.find_tri_other_vtx(a, b, array2);
		if (num4 == -1 || num5 == -1)
		{
			return MeshResult.Failed_BrokenTopology;
		}
		if (find_edge(num4, num5) != -1)
		{
			return MeshResult.Failed_FlippedEdgeExists;
		}
		int e = find_tri_neighbour_edge(num2, b, num4);
		int num6 = find_tri_neighbour_edge(num2, num4, a);
		int e2 = find_tri_neighbour_edge(num3, a, num5);
		int num7 = find_tri_neighbour_edge(num3, num5, b);
		set_triangle(num2, num4, num5, b);
		set_triangle(num3, num5, num4, a);
		set_edge_vertices(eab, num4, num5);
		set_edge_triangles(eab, num2, num3);
		if (replace_edge_triangle(num6, num2, num3) == -1)
		{
			throw new ArgumentException("DMesh3.FlipEdge: first replace_edge_triangle failed");
		}
		if (replace_edge_triangle(num7, num3, num2) == -1)
		{
			throw new ArgumentException("DMesh3.FlipEdge: second replace_edge_triangle failed");
		}
		set_triangle_edges(num2, eab, num7, e);
		set_triangle_edges(num3, eab, num6, e2);
		if (!vertex_edges.Remove(a, eab))
		{
			throw new ArgumentException("DMesh3.FlipEdge: first edge list remove failed");
		}
		if (!vertex_edges.Remove(b, eab))
		{
			throw new ArgumentException("DMesh3.FlipEdge: second edge list remove failed");
		}
		vertices_refcount.decrement(a, 1);
		vertices_refcount.decrement(b, 1);
		if (!IsVertex(a) || !IsVertex(b))
		{
			throw new ArgumentException("DMesh3.FlipEdge: either a or b is not a vertex?");
		}
		vertex_edges.Insert(num4, eab);
		vertex_edges.Insert(num5, eab);
		vertices_refcount.increment(num4, 1);
		vertices_refcount.increment(num5, 1);
		flip.eID = eab;
		flip.v0 = a;
		flip.v1 = b;
		flip.ov0 = num4;
		flip.ov1 = num5;
		flip.t0 = num2;
		flip.t1 = num3;
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	private void debug_fail(string s)
	{
	}

	private void check_tri(int t)
	{
		Index3i triangle = GetTriangle(t);
		if (triangle[0] != triangle[1] && triangle[0] != triangle[2])
		{
			_ = triangle[1];
			_ = triangle[2];
		}
	}

	private void check_edge(int e)
	{
		_ = GetEdgeT(e)[0];
		_ = -1;
	}

	public MeshResult CollapseEdge(int vKeep, int vRemove, out EdgeCollapseInfo collapse)
	{
		collapse = default(EdgeCollapseInfo);
		if (!IsVertex(vKeep) || !IsVertex(vRemove))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		int num = find_edge(vRemove, vKeep);
		if (num == -1)
		{
			return MeshResult.Failed_NotAnEdge;
		}
		int num2 = edges[4 * num + 2];
		if (num2 == -1)
		{
			return MeshResult.Failed_BrokenTopology;
		}
		Index3i triangle = GetTriangle(num2);
		int num3 = IndexUtil.find_tri_other_vtx(vRemove, vKeep, triangle);
		bool flag = false;
		int num4 = -1;
		int num5 = edges[4 * num + 3];
		if (num5 != -1)
		{
			Index3i triangle2 = GetTriangle(num5);
			num4 = IndexUtil.find_tri_other_vtx(vRemove, vKeep, triangle2);
			if (num3 == num4)
			{
				return MeshResult.Failed_FoundDuplicateTriangle;
			}
		}
		else
		{
			flag = true;
		}
		int num6 = vertex_edges.Count(vRemove);
		int num7 = -1;
		int num8 = -1;
		int num9 = -1;
		int num10 = -1;
		foreach (int item in vertex_edges.ValueItr(vRemove))
		{
			int num11 = edge_other_v(item, vRemove);
			if (num11 == num3)
			{
				num7 = item;
			}
			else if (num11 == num4)
			{
				num8 = item;
			}
			else
			{
				if (num11 == vKeep)
				{
					continue;
				}
				foreach (int item2 in vertex_edges.ValueItr(vKeep))
				{
					if (edge_other_v(item2, vKeep) == num11)
					{
						return MeshResult.Failed_InvalidNeighbourhood;
					}
				}
			}
		}
		if (num6 == 3 && !flag)
		{
			int num12 = find_edge(num4, num3);
			int num13 = 4 * num12;
			if (num12 != -1 && edges[num13 + 3] != -1)
			{
				int tID = edges[num13 + 2];
				int tID2 = edges[num13 + 3];
				if ((tri_has_v(tID, vRemove) && tri_has_v(tID2, vKeep)) || (tri_has_v(tID, vKeep) && tri_has_v(tID2, vRemove)))
				{
					return MeshResult.Failed_CollapseTetrahedron;
				}
			}
		}
		else if (flag && IsBoundaryEdge(num7))
		{
			num9 = find_edge_from_tri(vKeep, num3, num2);
			if (IsBoundaryEdge(num9))
			{
				return MeshResult.Failed_CollapseTriangle;
			}
		}
		if (!flag && IsBoundaryVertex(vRemove) && IsBoundaryVertex(vKeep))
		{
			return MeshResult.Failed_InvalidNeighbourhood;
		}
		int num14 = -1;
		int num15 = -1;
		foreach (int item3 in vertex_edges.ValueItr(vRemove))
		{
			int num16 = edge_other_v(item3, vRemove);
			if (num16 == vKeep)
			{
				if (!vertex_edges.Remove(vKeep, item3))
				{
					debug_fail("remove case o == b");
				}
			}
			else if (num16 == num3)
			{
				if (!vertex_edges.Remove(num3, item3))
				{
					debug_fail("remove case o == c");
				}
				num15 = edge_other_t(item3, num2);
			}
			else if (num16 == num4)
			{
				if (!vertex_edges.Remove(num4, item3))
				{
					debug_fail("remove case o == c, step 1");
				}
				num14 = edge_other_t(item3, num5);
			}
			else
			{
				if (replace_edge_vertex(item3, vRemove, vKeep) == -1)
				{
					debug_fail("remove case else");
				}
				vertex_edges.Insert(vKeep, item3);
			}
			for (int i = 0; i < 2; i++)
			{
				int num17 = edges[4 * item3 + 2 + i];
				if (num17 != -1 && num17 != num2 && num17 != num5 && tri_has_v(num17, vRemove))
				{
					if (replace_tri_vertex(num17, vRemove, vKeep) == -1)
					{
						debug_fail("remove last check");
					}
					vertices_refcount.increment(vKeep, 1);
					vertices_refcount.decrement(vRemove, 1);
				}
			}
		}
		if (!flag)
		{
			vertex_edges.Clear(vRemove);
			vertices_refcount.decrement(vRemove, 3);
			triangles_refcount.decrement(num2, 1);
			triangles_refcount.decrement(num5, 1);
			vertices_refcount.decrement(num3, 1);
			vertices_refcount.decrement(num4, 1);
			vertices_refcount.decrement(vKeep, 2);
			edges_refcount.decrement(num8, 1);
			edges_refcount.decrement(num, 1);
			edges_refcount.decrement(num7, 1);
			num10 = find_edge_from_tri(vKeep, num4, num5);
			if (num9 == -1)
			{
				num9 = find_edge_from_tri(vKeep, num3, num2);
			}
			if (replace_edge_triangle(num10, num5, num14) == -1)
			{
				debug_fail("isboundary=false branch, ebd replace triangle");
			}
			if (replace_edge_triangle(num9, num2, num15) == -1)
			{
				debug_fail("isboundary=false branch, ebc replace triangle");
			}
			if (num14 != -1 && replace_triangle_edge(num14, num8, num10) == -1)
			{
				debug_fail("isboundary=false branch, ebd replace triangle");
			}
			if (num15 != -1 && replace_triangle_edge(num15, num7, num9) == -1)
			{
				debug_fail("isboundary=false branch, ebd replace triangle");
			}
		}
		else
		{
			vertex_edges.Clear(vRemove);
			vertices_refcount.decrement(vRemove, 2);
			triangles_refcount.decrement(num2, 1);
			vertices_refcount.decrement(num3, 1);
			vertices_refcount.decrement(vKeep, 1);
			edges_refcount.decrement(num, 1);
			edges_refcount.decrement(num7, 1);
			num9 = find_edge_from_tri(vKeep, num3, num2);
			if (replace_edge_triangle(num9, num2, num15) == -1)
			{
				debug_fail("isboundary=false branch, ebc replace triangle");
			}
			if (num15 != -1 && replace_triangle_edge(num15, num7, num9) == -1)
			{
				debug_fail("isboundary=true branch, ebd replace triangle");
			}
		}
		collapse.vKept = vKeep;
		collapse.vRemoved = vRemove;
		collapse.bIsBoundary = flag;
		collapse.eCollapsed = num;
		collapse.tRemoved0 = num2;
		collapse.tRemoved1 = num5;
		collapse.eRemoved0 = num7;
		collapse.eRemoved1 = num8;
		collapse.eKept0 = num9;
		collapse.eKept1 = num10;
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public MeshResult MergeEdges(int eKeep, int eDiscard, out MergeEdgesInfo merge_info)
	{
		merge_info = default(MergeEdgesInfo);
		if (!IsEdge(eKeep) || !IsEdge(eDiscard))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		Index4i edge = GetEdge(eKeep);
		Index4i edge2 = GetEdge(eDiscard);
		if (edge.d != -1 || edge2.d != -1)
		{
			return MeshResult.Failed_NotABoundaryEdge;
		}
		int a = edge.a;
		int b = edge.b;
		int c = edge.c;
		int a2 = edge2.a;
		int b2 = edge2.b;
		int c2 = edge2.c;
		IndexUtil.orient_tri_edge(ref a, ref b, GetTriangle(c));
		IndexUtil.orient_tri_edge(ref a2, ref b2, GetTriangle(c2));
		int num = a2;
		a2 = b2;
		b2 = num;
		Vector3d vertex = GetVertex(a);
		Vector3d vertex2 = GetVertex(b);
		Vector3d vertex3 = GetVertex(a2);
		Vector3d vertex4 = GetVertex(b2);
		if (vertex.DistanceSquared(vertex3) + vertex2.DistanceSquared(vertex4) > vertex.DistanceSquared(vertex4) + vertex2.DistanceSquared(vertex3))
		{
			return MeshResult.Failed_SameOrientation;
		}
		merge_info.eKept = eKeep;
		merge_info.eRemoved = eDiscard;
		if (a != a2 && find_edge(a, a2) != -1)
		{
			return MeshResult.Failed_InvalidNeighbourhood;
		}
		if (b != b2 && find_edge(b, b2) != -1)
		{
			return MeshResult.Failed_InvalidNeighbourhood;
		}
		if (a != a2)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = ((b == b2) ? b : (-1));
			foreach (int item in VtxVerticesItr(a2))
			{
				if (item != num4 && (num2 = find_edge(a, item)) != -1)
				{
					num3 = find_edge(a2, item);
					if (!IsBoundaryEdge(num2) || !IsBoundaryEdge(num3))
					{
						return MeshResult.Failed_InvalidNeighbourhood;
					}
				}
			}
		}
		if (b != b2)
		{
			int num5 = 0;
			int num6 = 0;
			int num7 = ((a == a2) ? a : (-1));
			foreach (int item2 in VtxVerticesItr(b2))
			{
				if (item2 != num7 && (num5 = find_edge(b, item2)) != -1)
				{
					num6 = find_edge(b2, item2);
					if (!IsBoundaryEdge(num5) || !IsBoundaryEdge(num6))
					{
						return MeshResult.Failed_InvalidNeighbourhood;
					}
				}
			}
		}
		if (a != a2)
		{
			foreach (int item3 in vertex_edges.ValueItr(a2))
			{
				if (item3 != eDiscard)
				{
					replace_edge_vertex(item3, a2, a);
					short num8 = 0;
					if (replace_tri_vertex(edges[4 * item3 + 2], a2, a) >= 0)
					{
						num8++;
					}
					if (edges[4 * item3 + 3] != -1 && replace_tri_vertex(edges[4 * item3 + 3], a2, a) >= 0)
					{
						num8++;
					}
					vertex_edges.Insert(a, item3);
					if (num8 > 0)
					{
						vertices_refcount.increment(a, num8);
						vertices_refcount.decrement(a2, num8);
					}
				}
			}
			vertex_edges.Clear(a2);
			vertices_refcount.decrement(a2, 1);
			merge_info.vRemoved[0] = a2;
		}
		else
		{
			vertex_edges.Remove(a, eDiscard);
			merge_info.vRemoved[0] = -1;
		}
		merge_info.vKept[0] = a;
		if (b2 != b)
		{
			foreach (int item4 in vertex_edges.ValueItr(b2))
			{
				if (item4 != eDiscard)
				{
					replace_edge_vertex(item4, b2, b);
					short num9 = 0;
					if (replace_tri_vertex(edges[4 * item4 + 2], b2, b) >= 0)
					{
						num9++;
					}
					if (edges[4 * item4 + 3] != -1 && replace_tri_vertex(edges[4 * item4 + 3], b2, b) >= 0)
					{
						num9++;
					}
					vertex_edges.Insert(b, item4);
					if (num9 > 0)
					{
						vertices_refcount.increment(b, num9);
						vertices_refcount.decrement(b2, num9);
					}
				}
			}
			vertex_edges.Clear(b2);
			vertices_refcount.decrement(b2, 1);
			merge_info.vRemoved[1] = b2;
		}
		else
		{
			vertex_edges.Remove(b, eDiscard);
			merge_info.vRemoved[1] = -1;
		}
		merge_info.vKept[1] = b;
		replace_triangle_edge(c2, eDiscard, eKeep);
		edges_refcount.decrement(eDiscard, 1);
		set_edge_triangles(eKeep, c, c2);
		merge_info.eRemovedExtra = new Vector2i(-1, -1);
		merge_info.eKeptExtra = merge_info.eRemovedExtra;
		for (int i = 0; i < 2; i++)
		{
			int num10 = a;
			int num11 = a2;
			if (i == 1)
			{
				num10 = b;
				num11 = b2;
			}
			if (num10 == num11)
			{
				continue;
			}
			List<int> list = vertex_edges_list(num10);
			int count = list.Count;
			bool flag = false;
			for (int j = 0; j < count; j++)
			{
				if (flag)
				{
					break;
				}
				int num12 = list[j];
				if (!IsBoundaryEdge(num12))
				{
					continue;
				}
				int num13 = edge_other_v(num12, num10);
				for (int k = j + 1; k < count; k++)
				{
					int num14 = list[k];
					int num15 = edge_other_v(num14, num10);
					if (num13 == num15 && IsBoundaryEdge(num14))
					{
						int t = edges[4 * num12 + 2];
						int num16 = edges[4 * num14 + 2];
						replace_triangle_edge(num16, num14, num12);
						set_edge_triangles(num12, t, num16);
						vertex_edges.Remove(num10, num14);
						vertex_edges.Remove(num13, num14);
						edges_refcount.decrement(num14, 1);
						merge_info.eRemovedExtra[i] = num14;
						merge_info.eKeptExtra[i] = num12;
						flag = true;
						break;
					}
				}
			}
		}
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public virtual MeshResult PokeTriangle(int tid, out PokeTriangleInfo result)
	{
		return PokeTriangle(tid, Vector3d.One / 3.0, out result);
	}

	public virtual MeshResult PokeTriangle(int tid, Vector3d baryCoordinates, out PokeTriangleInfo result)
	{
		result = default(PokeTriangleInfo);
		if (!IsTriangle(tid))
		{
			return MeshResult.Failed_NotATriangle;
		}
		Index3i triangle = GetTriangle(tid);
		Index3i triEdges = GetTriEdges(tid);
		GetTriBaryPoint(tid, baryCoordinates.x, baryCoordinates.y, baryCoordinates.z, out var vinfo);
		int num = AppendVertex(vinfo);
		int num2 = add_edge(triangle.a, num, -1);
		int num3 = add_edge(triangle.b, num, -1);
		int num4 = add_edge(triangle.c, num, -1);
		vertices_refcount.increment(triangle.a, 1);
		vertices_refcount.increment(triangle.b, 1);
		vertices_refcount.increment(triangle.c, 1);
		vertices_refcount.increment(num, 3);
		set_triangle(tid, triangle.a, triangle.b, num);
		set_triangle_edges(tid, triEdges.a, num3, num2);
		int num5 = add_triangle_only(triangle.b, triangle.c, num, triEdges.b, num4, num3);
		int num6 = add_triangle_only(triangle.c, triangle.a, num, triEdges.c, num2, num4);
		replace_edge_triangle(triEdges.b, tid, num5);
		replace_edge_triangle(triEdges.c, tid, num6);
		set_edge_triangles(num2, tid, num6);
		set_edge_triangles(num3, tid, num5);
		set_edge_triangles(num4, num5, num6);
		if (HasTriangleGroups)
		{
			int value = triangle_groups[tid];
			triangle_groups.insert(value, num5);
			triangle_groups.insert(value, num6);
		}
		result.new_vid = num;
		result.new_t1 = num5;
		result.new_t2 = num6;
		result.new_edges = new Index3i(num2, num3, num4);
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	private void set_triangle(int tid, int v0, int v1, int v2)
	{
		int num = 3 * tid;
		triangles[num] = v0;
		triangles[num + 1] = v1;
		triangles[num + 2] = v2;
	}

	private void set_triangle_edges(int tid, int e0, int e1, int e2)
	{
		int num = 3 * tid;
		triangle_edges[num] = e0;
		triangle_edges[num + 1] = e1;
		triangle_edges[num + 2] = e2;
	}

	private int add_edge(int vA, int vB, int tA, int tB = -1)
	{
		if (vB < vA)
		{
			int num = vB;
			vB = vA;
			vA = num;
		}
		int num2 = edges_refcount.allocate();
		int num3 = 4 * num2;
		edges.insert(vA, num3);
		edges.insert(vB, num3 + 1);
		edges.insert(tA, num3 + 2);
		edges.insert(tB, num3 + 3);
		vertex_edges.Insert(vA, num2);
		vertex_edges.Insert(vB, num2);
		return num2;
	}

	private int replace_tri_vertex(int tID, int vOld, int vNew)
	{
		int num = 3 * tID;
		if (triangles[num] == vOld)
		{
			triangles[num] = vNew;
			return 0;
		}
		if (triangles[num + 1] == vOld)
		{
			triangles[num + 1] = vNew;
			return 1;
		}
		if (triangles[num + 2] == vOld)
		{
			triangles[num + 2] = vNew;
			return 2;
		}
		return -1;
	}

	private int add_triangle_only(int a, int b, int c, int e0, int e1, int e2)
	{
		int num = triangles_refcount.allocate();
		int num2 = 3 * num;
		triangles.insert(c, num2 + 2);
		triangles.insert(b, num2 + 1);
		triangles.insert(a, num2);
		triangle_edges.insert(e2, num2 + 2);
		triangle_edges.insert(e1, num2 + 1);
		triangle_edges.insert(e0, num2);
		return num;
	}

	private void allocate_edges_list(int vid)
	{
		if (vid < vertex_edges.Size)
		{
			vertex_edges.Clear(vid);
		}
		vertex_edges.AllocateAt(vid);
	}

	private List<int> vertex_edges_list(int vid)
	{
		return new List<int>(vertex_edges.ValueItr(vid));
	}

	private List<int> vertex_vertices_list(int vid)
	{
		List<int> list = new List<int>();
		foreach (int item in vertex_edges.ValueItr(vid))
		{
			list.Add(edge_other_v(item, vid));
		}
		return list;
	}

	private void set_edge_vertices(int eID, int a, int b)
	{
		int num = 4 * eID;
		edges[num] = Math.Min(a, b);
		edges[num + 1] = Math.Max(a, b);
	}

	private void set_edge_triangles(int eID, int t0, int t1)
	{
		int num = 4 * eID;
		edges[num + 2] = t0;
		edges[num + 3] = t1;
	}

	private int replace_edge_vertex(int eID, int vOld, int vNew)
	{
		int num = 4 * eID;
		int num2 = edges[num];
		int num3 = edges[num + 1];
		if (num2 == vOld)
		{
			edges[num] = Math.Min(num3, vNew);
			edges[num + 1] = Math.Max(num3, vNew);
			return 0;
		}
		if (num3 == vOld)
		{
			edges[num] = Math.Min(num2, vNew);
			edges[num + 1] = Math.Max(num2, vNew);
			return 1;
		}
		return -1;
	}

	private int replace_edge_triangle(int eID, int tOld, int tNew)
	{
		int num = 4 * eID;
		int num2 = edges[num + 2];
		int num3 = edges[num + 3];
		if (num2 == tOld)
		{
			if (tNew == -1)
			{
				edges[num + 2] = num3;
				edges[num + 3] = -1;
			}
			else
			{
				edges[num + 2] = tNew;
			}
			return 0;
		}
		if (num3 == tOld)
		{
			edges[num + 3] = tNew;
			return 1;
		}
		return -1;
	}

	private int replace_triangle_edge(int tID, int eOld, int eNew)
	{
		int num = 3 * tID;
		if (triangle_edges[num] == eOld)
		{
			triangle_edges[num] = eNew;
			return 0;
		}
		if (triangle_edges[num + 1] == eOld)
		{
			triangle_edges[num + 1] = eNew;
			return 1;
		}
		if (triangle_edges[num + 2] == eOld)
		{
			triangle_edges[num + 2] = eNew;
			return 2;
		}
		return -1;
	}
}
