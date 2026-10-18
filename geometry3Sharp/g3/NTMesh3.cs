using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace g3;

public class NTMesh3 : IDeformableMesh, IMesh, IPointSet
{
	public struct EdgeSplitInfo
	{
		public bool bIsBoundary;

		public int vNew;

		public int eNewBN;

		public List<int> NewEdges;
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

	private SmallListSet vertex_edges;

	private RefCountVector triangles_refcount;

	private DVector<int> triangles;

	private DVector<int> triangle_edges;

	private DVector<int> triangle_groups;

	private RefCountVector edges_refcount;

	private DVector<int> edges;

	private SmallListSet edge_triangles;

	private int timestamp;

	private int shape_timestamp;

	private int max_group_id;

	private AxisAlignedBox3d cached_bounds;

	private int cached_bounds_timestamp = -1;

	private bool cached_is_closed;

	private int cached_is_closed_timstamp = -1;

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

	public bool HasVertexUVs => false;

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
			if (cached_is_closed_timstamp != Timestamp)
			{
				cached_is_closed = IsClosed();
				cached_is_closed_timstamp = Timestamp;
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

	public NTMesh3(bool bWantNormals = true, bool bWantColors = false, bool bWantTriGroups = false)
	{
		allocate(bWantNormals, bWantColors, bWantTriGroups);
	}

	public NTMesh3(MeshComponents flags)
		: this((flags & MeshComponents.VertexNormals) != 0, (flags & MeshComponents.VertexColors) != 0, (flags & MeshComponents.FaceGroups) != 0)
	{
	}

	private void allocate(bool bWantNormals, bool bWantColors, bool bWantTriGroups)
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
		edge_triangles = new SmallListSet();
	}

	public NTMesh3(NTMesh3 copy)
	{
		Copy(copy);
	}

	public void Copy(NTMesh3 copy, bool bNormals = true, bool bColors = true)
	{
		vertices = new DVector<double>(copy.vertices);
		normals = ((bNormals && copy.normals != null) ? new DVector<float>(copy.normals) : null);
		colors = ((bColors && copy.colors != null) ? new DVector<float>(copy.colors) : null);
		vertices_refcount = new RefCountVector(copy.vertices_refcount);
		vertex_edges = new SmallListSet(copy.vertex_edges);
		triangles = new DVector<int>(copy.triangles);
		triangle_edges = new DVector<int>(copy.triangle_edges);
		triangles_refcount = new RefCountVector(copy.triangles_refcount);
		if (copy.triangle_groups != null)
		{
			triangle_groups = new DVector<int>(copy.triangle_groups);
		}
		max_group_id = copy.max_group_id;
		edges = new DVector<int>(copy.edges);
		edges_refcount = new RefCountVector(copy.edges_refcount);
		edge_triangles = new SmallListSet(copy.edge_triangles);
	}

	public NTMesh3(DMesh3 copy)
	{
		allocate(copy.HasVertexNormals, copy.HasVertexColors, copy.HasTriangleGroups);
		int[] array = new int[copy.MaxVertexID];
		foreach (int item in copy.VertexIndices())
		{
			array[item] = AppendVertex(copy.GetVertex(item));
		}
		foreach (Index3i item2 in copy.Triangles())
		{
			AppendTriangle(array[item2.a], array[item2.b], array[item2.c]);
		}
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

	public Vector2f GetVertexUV(int i)
	{
		return Vector2f.Zero;
	}

	public void SetVertexUV(int vID, Vector2f UV)
	{
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

	public bool GetVertex(int vID, ref NewVertexInfo vinfo, bool bWantNormals, bool bWantColors, bool bWantUVs)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return false;
		}
		vinfo.v.Set(vertices[3 * vID], vertices[3 * vID + 1], vertices[3 * vID + 2]);
		vinfo.bHaveN = (vinfo.bHaveUV = (vinfo.bHaveC = false));
		if (HasVertexColors && bWantNormals)
		{
			vinfo.bHaveN = true;
			vinfo.n.Set(normals[3 * vID], normals[3 * vID + 1], normals[3 * vID + 2]);
		}
		if (HasVertexColors && bWantColors)
		{
			vinfo.bHaveC = true;
			vinfo.c.Set(colors[3 * vID], colors[3 * vID + 1], colors[3 * vID + 2]);
		}
		return true;
	}

	public int GetVtxEdgeCount(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return -1;
		}
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
		result.bHaveUV = false;
		return result;
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
			int list_index = triangle_edges[tei + i];
			foreach (int item in edge_triangles.ValueItr(list_index))
			{
				if (item != tID)
				{
					yield return item;
				}
			}
			int num = i + 1;
			i = num;
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
		return max_group_id++;
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
		int num2 = triangles[num + nEdge % 3];
		int num3 = triangles[num + (nEdge + 1) % 3];
		int num4 = triangles[num + (nEdge + 2) % 3];
		Vector3d v = new Vector3d(vertices[3 * num2], vertices[3 * num2 + 1], vertices[3 * num2 + 2]);
		Vector3d v2 = new Vector3d(vertices[3 * num3], vertices[3 * num3 + 1], vertices[3 * num3 + 2]);
		Vector3d v3 = new Vector3d(vertices[3 * num4], vertices[3 * num4 + 1], vertices[3 * num4 + 2]);
		Vector3f x = (Vector3f)(v2 - v).Normalized;
		Vector3f vector3f = (Vector3f)MathUtil.Normal(ref v, ref v2, ref v3);
		Vector3f y = x.Cross(vector3f);
		return new Frame3f((Vector3f)(v + v2 + v3) / 3f, x, y, vector3f);
	}

	public Index2i GetEdgeV(int eID)
	{
		int num = 2 * eID;
		return new Index2i(edges[num], edges[num + 1]);
	}

	public bool GetEdgeV(int eID, ref Vector3d a, ref Vector3d b)
	{
		int num = 3 * edges[2 * eID];
		a.x = vertices[num];
		a.y = vertices[num + 1];
		a.z = vertices[num + 2];
		int num2 = 3 * edges[2 * eID + 1];
		b.x = vertices[num2];
		b.y = vertices[num2 + 1];
		b.z = vertices[num2 + 2];
		return true;
	}

	public IEnumerable<int> EdgeTrianglesItr(int eID)
	{
		return edge_triangles.ValueItr(eID);
	}

	public int EdgeTrianglesCount(int eID)
	{
		return edge_triangles.Count(eID);
	}

	public Index2i GetOrientedBoundaryEdgeV(int eID)
	{
		if (edges_refcount.isValid(eID) && edge_is_boundary(eID))
		{
			int num = 2 * eID;
			int a = edges[num];
			int b = edges[num + 1];
			int num2 = edge_triangles.First(eID);
			Index3i tri_verts = new Index3i(triangles[num2], triangles[num2 + 1], triangles[num2 + 2]);
			int num3 = IndexUtil.find_edge_index_in_tri(a, b, ref tri_verts);
			return new Index2i(tri_verts[num3], tri_verts[(num3 + 1) % 3]);
		}
		return InvalidEdge;
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

	public int AppendVertex(NewVertexInfo info)
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
		allocate_vertex_edges_list(num);
		updateTimeStamp(bShapeChange: true);
		return num;
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
		int eid = find_edge(tv[0], tv[1]);
		int eid2 = find_edge(tv[1], tv[2]);
		int eid3 = find_edge(tv[2], tv[0]);
		int num = triangles_refcount.allocate();
		int num2 = 3 * num;
		triangles.insert(tv[2], num2 + 2);
		triangles.insert(tv[1], num2 + 1);
		triangles.insert(tv[0], num2);
		if (triangle_groups != null)
		{
			triangle_groups.insert(gid, num);
			max_group_id = Math.Max(max_group_id, gid + 1);
		}
		vertices_refcount.increment(tv[0], 1);
		vertices_refcount.increment(tv[1], 1);
		vertices_refcount.increment(tv[2], 1);
		add_tri_edge(num, tv[0], tv[1], 0, eid);
		add_tri_edge(num, tv[1], tv[2], 1, eid2);
		add_tri_edge(num, tv[2], tv[0], 2, eid3);
		updateTimeStamp(bShapeChange: true);
		return num;
	}

	private void add_tri_edge(int tid, int v0, int v1, int j, int eid)
	{
		if (eid != -1)
		{
			edge_triangles.Insert(eid, tid);
			triangle_edges.insert(eid, 3 * tid + j);
		}
		else
		{
			eid = add_edge(v0, v1, tid);
			triangle_edges.insert(eid, 3 * tid + j);
		}
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
			if (edge_triangles.Count(item) == 1)
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

	public int FindEdge(int vA, int vB)
	{
		return find_edge(vA, vB);
	}

	public int FindEdgeFromTri(int vA, int vB, int t)
	{
		return find_edge_from_tri(vA, vB, t);
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

	public int VtxBoundaryEdges(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			int num = 0;
			{
				foreach (int item in vertex_edges.ValueItr(vID))
				{
					if (edge_triangles.Count(item) == 1)
					{
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
					if (edge_triangles.Count(item) == 1)
					{
						e[result++] = item;
					}
				}
				return result;
			}
		}
		return -1;
	}

	public MeshResult GetVtxTriangles(int vID, List<int> vTriangles)
	{
		if (!IsVertex(vID))
		{
			return MeshResult.Failed_NotAVertex;
		}
		vTriangles.Clear();
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			foreach (int item2 in edge_triangles.ValueItr(item))
			{
				if (!vTriangles.Contains(item2))
				{
					vTriangles.Add(item2);
				}
			}
		}
		return MeshResult.Ok;
	}

	public int GetVtxTriangleCount(int vID, bool bBruteForce = false)
	{
		List<int> list = new List<int>();
		if (GetVtxTriangles(vID, list) != 0)
		{
			return -1;
		}
		return list.Count;
	}

	public IEnumerable<int> VtxTrianglesItr(int vID)
	{
		if (!IsVertex(vID))
		{
			yield break;
		}
		List<int> list = new List<int>();
		GetVtxTriangles(vID, list);
		foreach (int item in list)
		{
			yield return item;
		}
	}

	protected bool tri_has_v(int tID, int vID)
	{
		int num = 3 * tID;
		if (triangles[num] != vID && triangles[num + 1] != vID)
		{
			return triangles[num + 2] == vID;
		}
		return true;
	}

	protected bool tri_is_boundary(int tID)
	{
		int num = 3 * tID;
		if (!edge_is_boundary(triangle_edges[num]) && !edge_is_boundary(triangle_edges[num + 1]))
		{
			return edge_is_boundary(triangle_edges[num + 2]);
		}
		return true;
	}

	protected bool tri_has_neighbour_t(int tCheck, int tNbr)
	{
		int num = 3 * tCheck;
		if (!edge_has_t(triangle_edges[num], tNbr) && !edge_has_t(triangle_edges[num + 1], tNbr))
		{
			return edge_has_t(triangle_edges[num + 2], tNbr);
		}
		return true;
	}

	protected bool tri_has_sequential_v(int tID, int vA, int vB)
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

	protected int find_tri_neighbour_edge(int tID, int vA, int vB)
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

	protected int find_tri_neighbour_index(int tID, int vA, int vB)
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

	public bool IsNonManifoldEdge(int eid)
	{
		return edge_triangles.Count(eid) > 2;
	}

	public bool IsBoundaryEdge(int eid)
	{
		return edge_triangles.Count(eid) == 1;
	}

	protected bool edge_is_boundary(int eid)
	{
		return edge_triangles.Count(eid) == 1;
	}

	protected bool edge_has_v(int eid, int vid)
	{
		int num = 2 * eid;
		if (edges[num] != vid)
		{
			return edges[num + 1] == vid;
		}
		return true;
	}

	protected bool edge_has_t(int eid, int tid)
	{
		return edge_triangles.Contains(eid, tid);
	}

	protected int edge_other_v(int eID, int vID)
	{
		int num = 2 * eID;
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

	public bool vertex_is_boundary(int vID)
	{
		return IsBoundaryVertex(vID);
	}

	public bool IsBoundaryVertex(int vID)
	{
		foreach (int item in vertex_edges.ValueItr(vID))
		{
			if (edge_triangles.Count(item) == 1)
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
			if (edges[2 * item + 1] == num)
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

	public bool IsBowtieVertex(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			int vtxTriangleCount = GetVtxTriangleCount(vID);
			int vtxEdgeCount = GetVtxEdgeCount(vID);
			if (vtxTriangleCount != vtxEdgeCount && vtxTriangleCount != vtxEdgeCount - 1)
			{
				return true;
			}
			return false;
		}
		throw new Exception("NTMesh3.IsBowtieVertex: " + vID + " is not a valid vertex");
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
				if (edge_is_boundary(item))
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
				if (edges_refcount.isValid(i) && edge_is_boundary(i))
				{
					return false;
				}
			}
		}
		return true;
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
		allocate_edge_triangles_list(num2);
		int num3 = 2 * num2;
		edges.insert(vA, num3);
		edges.insert(vB, num3 + 1);
		if (tA != -1)
		{
			edge_triangles.Insert(num2, tA);
		}
		if (tB != -1)
		{
			edge_triangles.Insert(num2, tB);
		}
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

	private void allocate_vertex_edges_list(int vid)
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

	private void allocate_edge_triangles_list(int eid)
	{
		if (eid < edge_triangles.Size)
		{
			edge_triangles.Clear(eid);
		}
		edge_triangles.AllocateAt(eid);
	}

	private void set_edge_vertices(int eID, int a, int b)
	{
		int num = 2 * eID;
		edges[num] = Math.Min(a, b);
		edges[num + 1] = Math.Max(a, b);
	}

	private int replace_edge_vertex(int eID, int vOld, int vNew)
	{
		int num = 2 * eID;
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

	private bool replace_edge_triangle(int eID, int tOld, int tNew)
	{
		bool result = edge_triangles.Remove(eID, tOld);
		edge_triangles.Insert(eID, tNew);
		return result;
	}

	private void add_edge_triangle(int eID, int tID)
	{
		edge_triangles.Insert(eID, tID);
	}

	private bool remove_edge_triangle(int eID, int tID)
	{
		return edge_triangles.Remove(eID, tID);
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

	public MeshResult RemoveTriangle(int tID, bool bRemoveIsolatedVertices = true)
	{
		if (!triangles_refcount.isValid(tID))
		{
			return MeshResult.Failed_NotATriangle;
		}
		Index3i triangle = GetTriangle(tID);
		Index3i triEdges = GetTriEdges(tID);
		for (int i = 0; i < 3; i++)
		{
			int num = triEdges[i];
			remove_edge_triangle(num, tID);
			if (edge_triangles.Count(num) == 0)
			{
				int list_index = edges[2 * num];
				vertex_edges.Remove(list_index, num);
				int list_index2 = edges[2 * num + 1];
				vertex_edges.Remove(list_index2, num);
				edges_refcount.decrement(num, 1);
			}
		}
		triangles_refcount.decrement(tID, 1);
		for (int j = 0; j < 3; j++)
		{
			int num2 = triangle[j];
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

	public MeshResult SplitEdge(int eab, out EdgeSplitInfo split)
	{
		split = default(EdgeSplitInfo);
		if (!IsEdge(eab))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		int num = 2 * eab;
		int num2 = edges[num];
		int num3 = edges[num + 1];
		List<int> list = new List<int>(edge_triangles.ValueItr(eab));
		if (list.Count < 1)
		{
			return MeshResult.Failed_BrokenTopology;
		}
		Vector3d v = 0.5 * (GetVertex(num2) + GetVertex(num3));
		int num4 = AppendVertex(v);
		if (HasVertexNormals)
		{
			SetVertexNormal(num4, (GetVertexNormal(num2) + GetVertexNormal(num3)).Normalized);
		}
		if (HasVertexColors)
		{
			SetVertexColor(num4, 0.5f * (GetVertexColor(num2) + GetVertexColor(num3)));
		}
		replace_edge_vertex(eab, num3, num4);
		vertex_edges.Remove(num3, eab);
		vertex_edges.Insert(num4, eab);
		int num5 = add_edge(num4, num3, -1);
		vertices_refcount.increment(num4, (short)list.Count);
		split.NewEdges = new List<int>();
		split.eNewBN = num5;
		foreach (int item in list)
		{
			Index3i tri_verts = GetTriangle(item);
			Index3i triEdges = GetTriEdges(item);
			int num6 = IndexUtil.find_tri_other_vtx(num2, num3, tri_verts);
			replace_tri_vertex(item, num3, num4);
			int num7 = triEdges[IndexUtil.find_edge_index_in_tri(num3, num6, ref tri_verts)];
			bool num8 = IndexUtil.is_ordered(num2, num3, ref tri_verts);
			int num9 = (num8 ? add_triangle_only(num4, num3, num6, -1, -1, -1) : add_triangle_only(num3, num4, num6, -1, -1, -1));
			if (triangle_groups != null)
			{
				triangle_groups.insert(triangle_groups[item], num9);
			}
			replace_edge_triangle(num7, item, num9);
			add_edge_triangle(num5, num9);
			int num10 = add_edge(num6, num4, item, num9);
			split.NewEdges.Add(num10);
			replace_triangle_edge(item, num7, num10);
			if (num8)
			{
				set_triangle_edges(num9, num5, num7, num10);
			}
			else
			{
				set_triangle_edges(num9, num5, num10, num7);
			}
			vertices_refcount.increment(num6, 1);
			vertices_refcount.increment(num4, 1);
		}
		split.bIsBoundary = list.Count == 1;
		split.vNew = num4;
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
		Vector3d v = (GetVertex(triangle.a) + GetVertex(triangle.b) + GetVertex(triangle.c)) / 3.0;
		int num = AppendVertex(v);
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
		add_edge_triangle(num2, tid);
		add_edge_triangle(num2, num6);
		add_edge_triangle(num3, tid);
		add_edge_triangle(num3, num5);
		add_edge_triangle(num4, num5);
		add_edge_triangle(num4, num6);
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

	public DMesh3 Deconstruct()
	{
		DMesh3 dMesh = new DMesh3();
		foreach (Index3i item in Triangles())
		{
			dMesh.AppendTriangle(dMesh.AppendVertex(GetVertex(item.a)), dMesh.AppendVertex(GetVertex(item.b)), dMesh.AppendVertex(GetVertex(item.c)));
		}
		return dMesh;
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

	public bool CheckValidity(FailMode eFailMode = FailMode.Throw)
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
				int num = triEdges[k];
				action(IsEdge(num));
				if (edge_is_boundary(num))
				{
					action(tri_is_boundary(item));
					continue;
				}
				bool obj = false;
				foreach (int item2 in EdgeTrianglesItr(num))
				{
					if (item2 != item)
					{
						action(tri_has_neighbour_t(item2, item));
					}
					else
					{
						obj = true;
					}
				}
				action(obj);
				int a = triangle[k];
				int a2 = triangle[(k + 1) % 3];
				Index2i edgeV = GetEdgeV(triEdges[k]);
				action(IndexUtil.same_pair_unordered(a, a2, edgeV[0], edgeV[1]));
			}
		}
		foreach (int item3 in EdgeIndices())
		{
			action(IsEdge(item3));
			action(edges_refcount.refCount(item3) == 1);
			Index2i edgeV2 = GetEdgeV(item3);
			action(IsVertex(edgeV2[0]));
			action(IsVertex(edgeV2[1]));
			action(edgeV2[0] < edgeV2[1]);
			foreach (int item4 in EdgeTrianglesItr(item3))
			{
				action(IsTriangle(item4));
			}
		}
		if (vertices_refcount.is_dense)
		{
			for (int l = 0; l < vertices.Length / 3; l++)
			{
				action(vertices_refcount.isValid(l));
			}
		}
		foreach (int item5 in VertexIndices())
		{
			action(IsVertex(item5));
			Vector3d vertex = GetVertex(item5);
			action(!double.IsNaN(vertex.LengthSquared));
			action(!double.IsInfinity(vertex.LengthSquared));
			foreach (int item6 in vertex_edges.ValueItr(item5))
			{
				action(IsEdge(item6));
				action(edge_has_v(item6, item5));
				int num2 = edge_other_v(item6, item5);
				int num3 = find_edge(item5, num2);
				action(num3 != -1);
				action(num3 == item6);
				num3 = find_edge(num2, item5);
				action(num3 != -1);
				action(num3 == item6);
			}
			foreach (int item7 in VtxVerticesItr(item5))
			{
				action(IsVertex(item7));
				int eID = find_edge(item5, item7);
				action(IsEdge(eID));
			}
			List<int> list = new List<int>();
			GetVtxTriangles(item5, list);
			action(vertices_refcount.refCount(item5) == list.Count + 1);
			action(array[item5] == list.Count);
			foreach (int item8 in list)
			{
				action(tri_has_v(item8, item5));
			}
			List<int> list2 = new List<int>(list);
			foreach (int item9 in vertex_edges.ValueItr(item5))
			{
				foreach (int item10 in EdgeTrianglesItr(item9))
				{
					action(list.Contains(item10));
					list2.Remove(item10);
				}
			}
			action(list2.Count == 0);
		}
		return is_ok;
	}
}
