using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class MeshMeshCut
{
	private class SegmentVtx
	{
		public Vector3d v;

		public int type = -1;

		public int initial_type = -1;

		public int vtx_id = -1;

		public int elem_id = -1;
	}

	private class IntersectSegment
	{
		public int base_tid;

		public SegmentVtx v0;

		public SegmentVtx v1;

		public SegmentVtx this[int key]
		{
			get
			{
				if (key != 0)
				{
					return v1;
				}
				return v0;
			}
			set
			{
				if (key == 0)
				{
					v0 = value;
				}
				else
				{
					v1 = value;
				}
			}
		}
	}

	public DMesh3 Target;

	public DMesh3 CutMesh;

	private PointHashGrid3d<int> PointHash;

	public double VertexSnapTol = 1E-05;

	public List<int> CutVertices;

	private List<SegmentVtx> SegVertices;

	private Dictionary<int, SegmentVtx> VIDToSegVtxMap;

	private Dictionary<int, List<SegmentVtx>> FaceVertices;

	private Dictionary<int, List<SegmentVtx>> EdgeVertices;

	private IntersectSegment[] Segments;

	private Vector3d[] BaseFaceCentroids;

	private Vector3d[] BaseFaceNormals;

	private Dictionary<int, HashSet<int>> SubFaces;

	private Dictionary<int, int> ParentFaces;

	private HashSet<int> SegmentInsertVertices;

	public void Compute()
	{
		double cellSize = Target.CachedBounds.MaxDim / 64.0;
		PointHash = new PointHashGrid3d<int>(cellSize, -1);
		foreach (int item in Target.VertexIndices())
		{
			Vector3d vertex = Target.GetVertex(item);
			int num = find_existing_vertex(vertex);
			if (num != -1)
			{
				Console.WriteLine("VERTEX {0} IS DUPLICATE OF {1}!", item, num);
			}
			PointHash.InsertPointUnsafe(item, vertex);
		}
		initialize();
		find_segments();
		insert_face_vertices();
		insert_edge_vertices();
		connect_edges();
		foreach (SegmentVtx segVertex in SegVertices)
		{
			SegmentInsertVertices.Add(segVertex.vtx_id);
		}
	}

	public void RemoveContained()
	{
		DMeshAABBTree3 spatial = new DMeshAABBTree3(CutMesh, autoBuild: true);
		spatial.WindingNumber(Vector3d.Zero);
		SafeListBuilder<int> removeT = new SafeListBuilder<int>();
		gParallel.ForEach(Target.TriangleIndices(), delegate(int tid)
		{
			Vector3d triCentroid = Target.GetTriCentroid(tid);
			if (spatial.WindingNumber(triCentroid) > 0.9)
			{
				removeT.SafeAdd(tid);
			}
		});
		MeshEditor.RemoveTriangles(Target, removeT.Result);
		CutVertices = new List<int>();
		foreach (int segmentInsertVertex in SegmentInsertVertices)
		{
			if (Target.IsVertex(segmentInsertVertex))
			{
				CutVertices.Add(segmentInsertVertex);
			}
		}
	}

	public void AppendSegments(double r)
	{
		IntersectSegment[] segments = Segments;
		foreach (IntersectSegment intersectSegment in segments)
		{
			Segment3d seg = new Segment3d(intersectSegment.v0.v, intersectSegment.v1.v);
			if (Target.FindEdge(intersectSegment.v0.vtx_id, intersectSegment.v1.vtx_id) == -1)
			{
				MeshEditor.AppendLine(Target, seg, (float)r);
			}
		}
	}

	public void ColorFaces()
	{
		int num = 1;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int key in SubFaces.Keys)
		{
			dictionary[key] = num++;
		}
		Target.EnableTriangleGroups();
		foreach (int item in Target.TriangleIndices())
		{
			if (ParentFaces.ContainsKey(item))
			{
				Target.SetTriangleGroup(item, dictionary[ParentFaces[item]]);
			}
			else if (SubFaces.ContainsKey(item))
			{
				Target.SetTriangleGroup(item, dictionary[item]);
			}
		}
	}

	private void initialize()
	{
		BaseFaceCentroids = new Vector3d[Target.MaxTriangleID];
		BaseFaceNormals = new Vector3d[Target.MaxTriangleID];
		double fArea = 0.0;
		foreach (int item in Target.TriangleIndices())
		{
			Target.GetTriInfo(item, out BaseFaceNormals[item], out fArea, out BaseFaceCentroids[item]);
		}
		SegVertices = new List<SegmentVtx>();
		EdgeVertices = new Dictionary<int, List<SegmentVtx>>();
		FaceVertices = new Dictionary<int, List<SegmentVtx>>();
		SubFaces = new Dictionary<int, HashSet<int>>();
		ParentFaces = new Dictionary<int, int>();
		SegmentInsertVertices = new HashSet<int>();
		VIDToSegVtxMap = new Dictionary<int, SegmentVtx>();
	}

	private void find_segments()
	{
		Dictionary<Vector3d, SegmentVtx> dictionary = new Dictionary<Vector3d, SegmentVtx>();
		DMeshAABBTree3 dMeshAABBTree = new DMeshAABBTree3(Target, autoBuild: true);
		DMeshAABBTree3 otherTree = new DMeshAABBTree3(CutMesh, autoBuild: true);
		DMeshAABBTree3.IntersectionsQueryResult intersectionsQueryResult = dMeshAABBTree.FindAllIntersections(otherTree);
		Segments = new IntersectSegment[intersectionsQueryResult.Segments.Count];
		for (int i = 0; i < Segments.Length; i++)
		{
			DMeshAABBTree3.SegmentIntersection segmentIntersection = intersectionsQueryResult.Segments[i];
			Vector3dTuple2 vector3dTuple = new Vector3dTuple2(segmentIntersection.point0, segmentIntersection.point1);
			IntersectSegment intersectSegment = new IntersectSegment
			{
				base_tid = segmentIntersection.t0
			};
			Segments[i] = intersectSegment;
			for (int j = 0; j < 2; j++)
			{
				Vector3d v = vector3dTuple[j];
				if (dictionary.TryGetValue(v, out var value))
				{
					intersectSegment[j] = value;
					continue;
				}
				value = new SegmentVtx
				{
					v = v
				};
				SegVertices.Add(value);
				dictionary[v] = value;
				intersectSegment[j] = value;
				int num = find_existing_vertex(segmentIntersection.point0);
				if (num >= 0)
				{
					value.initial_type = (value.type = 0);
					value.elem_id = num;
					value.vtx_id = num;
					VIDToSegVtxMap[value.vtx_id] = value;
					continue;
				}
				Triangle3d tri = default(Triangle3d);
				Target.GetTriVertices(segmentIntersection.t0, ref tri.V0, ref tri.V1, ref tri.V2);
				Index3i triangle = Target.GetTriangle(segmentIntersection.t0);
				int num2 = on_edge(ref tri, ref v);
				if (num2 >= 0)
				{
					value.initial_type = (value.type = 1);
					value.elem_id = Target.FindEdge(triangle[num2], triangle[(num2 + 1) % 3]);
					add_edge_vtx(value.elem_id, value);
				}
				else
				{
					value.initial_type = (value.type = 2);
					value.elem_id = segmentIntersection.t0;
					add_face_vtx(value.elem_id, value);
				}
			}
		}
	}

	private void insert_face_vertices()
	{
		while (FaceVertices.Count > 0)
		{
			KeyValuePair<int, List<SegmentVtx>> keyValuePair = FaceVertices.First();
			int key = keyValuePair.Key;
			List<SegmentVtx> value = keyValuePair.Value;
			SegmentVtx segmentVtx = value[value.Count - 1];
			value.RemoveAt(value.Count - 1);
			if (Target.PokeTriangle(key, out var result) != 0)
			{
				throw new Exception("shit");
			}
			int new_vid = result.new_vid;
			Target.SetVertex(new_vid, segmentVtx.v);
			segmentVtx.vtx_id = new_vid;
			VIDToSegVtxMap[segmentVtx.vtx_id] = segmentVtx;
			PointHash.InsertPoint(segmentVtx.vtx_id, segmentVtx.v);
			FaceVertices.Remove(key);
			Index3i new_edges = result.new_edges;
			Index3i pokeTris = new Index3i(key, result.new_t1, result.new_t2);
			foreach (SegmentVtx item in value)
			{
				update_from_poke(item, new_edges, pokeTris);
				if (item.type == 1)
				{
					add_edge_vtx(item.elem_id, item);
				}
				else if (item.type == 2)
				{
					add_face_vtx(item.elem_id, item);
				}
			}
			add_poke_subfaces(key, ref result);
		}
	}

	private void update_from_poke(SegmentVtx sv, Index3i pokeEdges, Index3i pokeTris)
	{
		int num = find_existing_vertex(sv.v);
		if (num >= 0)
		{
			sv.type = 0;
			sv.elem_id = num;
			sv.vtx_id = num;
			VIDToSegVtxMap[sv.vtx_id] = sv;
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			if (is_on_edge(pokeEdges[i], sv.v))
			{
				sv.type = 1;
				sv.elem_id = pokeEdges[i];
				return;
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (is_in_triangle(pokeTris[j], sv.v))
			{
				sv.type = 2;
				sv.elem_id = pokeTris[j];
				return;
			}
		}
		Console.WriteLine("unsorted vertex!");
		sv.elem_id = pokeTris.a;
	}

	private void insert_edge_vertices()
	{
		while (EdgeVertices.Count > 0)
		{
			KeyValuePair<int, List<SegmentVtx>> keyValuePair = EdgeVertices.First();
			int key = keyValuePair.Key;
			List<SegmentVtx> value = keyValuePair.Value;
			SegmentVtx segmentVtx = value[value.Count - 1];
			value.RemoveAt(value.Count - 1);
			Index2i edgeT = Target.GetEdgeT(key);
			if (Target.SplitEdge(key, out var split) != 0)
			{
				throw new Exception("insert_edge_vertices: split failed!");
			}
			int vNew = split.vNew;
			Index2i splitEdges = new Index2i(key, split.eNewBN);
			Target.SetVertex(vNew, segmentVtx.v);
			segmentVtx.vtx_id = vNew;
			VIDToSegVtxMap[segmentVtx.vtx_id] = segmentVtx;
			PointHash.InsertPoint(segmentVtx.vtx_id, segmentVtx.v);
			EdgeVertices.Remove(key);
			foreach (SegmentVtx item in value)
			{
				update_from_split(item, splitEdges);
				if (item.type == 1)
				{
					add_edge_vtx(item.elem_id, item);
				}
			}
			add_split_subfaces(edgeT, ref split);
		}
	}

	private void update_from_split(SegmentVtx sv, Index2i splitEdges)
	{
		int num = find_existing_vertex(sv.v);
		if (num >= 0)
		{
			sv.type = 0;
			sv.elem_id = num;
			sv.vtx_id = num;
			VIDToSegVtxMap[sv.vtx_id] = sv;
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (is_on_edge(splitEdges[i], sv.v))
			{
				sv.type = 1;
				sv.elem_id = splitEdges[i];
				return;
			}
		}
		throw new Exception("update_from_split: unsortable vertex?");
	}

	private void connect_edges()
	{
		int num = Segments.Length;
		for (int i = 0; i < num; i++)
		{
			IntersectSegment intersectSegment = Segments[i];
			if (intersectSegment.v0 == intersectSegment.v1 || intersectSegment.v0.vtx_id == intersectSegment.v1.vtx_id)
			{
				continue;
			}
			int vtx_id = intersectSegment.v0.vtx_id;
			int vtx_id2 = intersectSegment.v1.vtx_id;
			if (vtx_id == -1 || vtx_id2 == -1)
			{
				throw new Exception("segment vertex is not defined?");
			}
			if (Target.FindEdge(vtx_id, vtx_id2) == -1)
			{
				try
				{
					insert_segment(intersectSegment);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void insert_segment(IntersectSegment seg)
	{
		List<int> regionTris = get_all_baseface_tris(seg.base_tid);
		RegionOperator regionOperator = new RegionOperator(Target, regionTris);
		Vector3d n = BaseFaceNormals[seg.base_tid];
		Vector3d c = BaseFaceCentroids[seg.base_tid];
		Vector3d.MakePerpVectors(ref n, out var e0, out var e1);
		DMesh3 subMesh = regionOperator.Region.SubMesh;
		MeshTransforms.PerVertexTransform(subMesh, delegate(Vector3d v)
		{
			v -= c;
			return new Vector3d(v.Dot(e0), v.Dot(e1), 0.0);
		});
		Vector3d v2 = seg.v0.v;
		Vector3d v3 = seg.v1.v;
		v2 -= c;
		v3 -= c;
		Vector2d v4 = new Vector2d(v2.Dot(e0), v2.Dot(e1));
		Vector2d v5 = new Vector2d(v3.Dot(e0), v3.Dot(e1));
		PolyLine2d polyLine2d = new PolyLine2d();
		polyLine2d.AppendVertex(v4);
		polyLine2d.AppendVertex(v5);
		MeshInsertUVPolyCurve meshInsertUVPolyCurve = new MeshInsertUVPolyCurve(subMesh, polyLine2d);
		meshInsertUVPolyCurve.Apply();
		MeshVertexSelection meshVertexSelection = new MeshVertexSelection(subMesh);
		meshVertexSelection.SelectEdgeVertices(meshInsertUVPolyCurve.OnCutEdges);
		MeshTransforms.PerVertexTransform(subMesh, (Vector3d v) => c + v.x * e0 + v.y * e1);
		regionOperator.BackPropropagate();
		foreach (int item in meshVertexSelection)
		{
			SegmentInsertVertices.Add(regionOperator.ReinsertSubToBaseMapV[item]);
		}
		add_regionop_subfaces(seg.base_tid, regionOperator);
	}

	private void add_edge_vtx(int eid, SegmentVtx vtx)
	{
		if (EdgeVertices.TryGetValue(eid, out var value))
		{
			value.Add(vtx);
			return;
		}
		value = new List<SegmentVtx> { vtx };
		EdgeVertices[eid] = value;
	}

	private void add_face_vtx(int tid, SegmentVtx vtx)
	{
		if (FaceVertices.TryGetValue(tid, out var value))
		{
			value.Add(vtx);
			return;
		}
		value = new List<SegmentVtx> { vtx };
		FaceVertices[tid] = value;
	}

	private void add_poke_subfaces(int tid, ref DMesh3.PokeTriangleInfo pokeInfo)
	{
		int num = get_parent(tid);
		HashSet<int> subfaces = get_subfaces(num);
		if (tid != num)
		{
			add_subface(subfaces, num, tid);
		}
		add_subface(subfaces, num, pokeInfo.new_t1);
		add_subface(subfaces, num, pokeInfo.new_t2);
	}

	private void add_split_subfaces(Index2i origTris, ref DMesh3.EdgeSplitInfo splitInfo)
	{
		int num = get_parent(origTris.a);
		HashSet<int> subfaces = get_subfaces(num);
		if (origTris.a != num)
		{
			add_subface(subfaces, num, origTris.a);
		}
		add_subface(subfaces, num, splitInfo.eNewT2);
		if (origTris.b != -1)
		{
			int num2 = get_parent(origTris.b);
			HashSet<int> subfaces2 = get_subfaces(num2);
			if (origTris.b != num2)
			{
				add_subface(subfaces2, num2, origTris.b);
			}
			add_subface(subfaces2, num2, splitInfo.eNewT3);
		}
	}

	private void add_regionop_subfaces(int parent, RegionOperator op)
	{
		HashSet<int> subfaces = get_subfaces(parent);
		int[] currentBaseTriangles = op.CurrentBaseTriangles;
		foreach (int num in currentBaseTriangles)
		{
			if (num != parent)
			{
				add_subface(subfaces, parent, num);
			}
		}
	}

	private int get_parent(int tid)
	{
		if (!ParentFaces.TryGetValue(tid, out var value))
		{
			return tid;
		}
		return value;
	}

	private HashSet<int> get_subfaces(int parent)
	{
		if (!SubFaces.TryGetValue(parent, out var value))
		{
			value = new HashSet<int>();
			SubFaces[parent] = value;
		}
		return value;
	}

	private void add_subface(HashSet<int> subfaces, int parent, int tid)
	{
		subfaces.Add(tid);
		ParentFaces[tid] = parent;
	}

	private List<int> get_all_baseface_tris(int base_tid)
	{
		return new List<int>(get_subfaces(base_tid)) { base_tid };
	}

	private bool is_inserted_free_edge(int eid)
	{
		Index2i edgeT = Target.GetEdgeT(eid);
		if (get_parent(edgeT.a) != get_parent(edgeT.b))
		{
			return false;
		}
		throw new Exception("not done yet!");
	}

	protected int on_edge(ref Triangle3d tri, ref Vector3d v)
	{
		if (new Segment3d(tri.V0, tri.V1).DistanceSquared(v) < VertexSnapTol * VertexSnapTol)
		{
			return 0;
		}
		if (new Segment3d(tri.V1, tri.V2).DistanceSquared(v) < VertexSnapTol * VertexSnapTol)
		{
			return 1;
		}
		if (new Segment3d(tri.V2, tri.V0).DistanceSquared(v) < VertexSnapTol * VertexSnapTol)
		{
			return 2;
		}
		return -1;
	}

	protected int on_edge_eid(int tid, Vector3d v)
	{
		Index3i triangle = Target.GetTriangle(tid);
		Triangle3d tri = default(Triangle3d);
		Target.GetTriVertices(tid, ref tri.V0, ref tri.V1, ref tri.V2);
		int num = on_edge(ref tri, ref v);
		if (num < 0)
		{
			return -1;
		}
		return Target.FindEdge(triangle[num], triangle[(num + 1) % 3]);
	}

	protected bool is_on_edge(int eid, Vector3d v)
	{
		Index2i edgeV = Target.GetEdgeV(eid);
		return new Segment3d(Target.GetVertex(edgeV.a), Target.GetVertex(edgeV.b)).DistanceSquared(v) < VertexSnapTol * VertexSnapTol;
	}

	protected bool is_in_triangle(int tid, Vector3d v)
	{
		Triangle3d triangle3d = default(Triangle3d);
		Target.GetTriVertices(tid, ref triangle3d.V0, ref triangle3d.V1, ref triangle3d.V2);
		Vector3d vector3d = triangle3d.BarycentricCoords(v);
		if (vector3d.x >= 0.0 && vector3d.y >= 0.0 && vector3d.z >= 0.0 && vector3d.x < 1.0 && vector3d.y <= 1.0)
		{
			return vector3d.z <= 1.0;
		}
		return false;
	}

	protected int find_existing_vertex(Vector3d pt)
	{
		return find_nearest_vertex(pt, VertexSnapTol);
	}

	protected int find_nearest_vertex(Vector3d pt, double searchRadius, int ignore_vid = -1)
	{
		KeyValuePair<int, double> keyValuePair = ((ignore_vid == -1) ? PointHash.FindNearestInRadius(pt, searchRadius, (int b) => pt.DistanceSquared(Target.GetVertex(b))) : PointHash.FindNearestInRadius(pt, searchRadius, (int b) => pt.DistanceSquared(Target.GetVertex(b)), (int vid) => vid == ignore_vid));
		if (keyValuePair.Key == PointHash.InvalidValue)
		{
			return -1;
		}
		return keyValuePair.Key;
	}
}
