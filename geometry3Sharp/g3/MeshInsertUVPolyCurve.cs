using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class MeshInsertUVPolyCurve
{
	public DMesh3 Mesh;

	public PolyLine2d Curve;

	public bool IsLoop;

	public Func<int, Vector2d> PointF;

	public Action<int, Vector2d> SetPointF;

	public bool EnableCutSpansAndLoops = true;

	public bool UseTriSpatial = true;

	public double SpatialEpsilon = 1E-08;

	public int[] CurveVertices;

	public HashSet<int> OnCutEdges;

	public List<EdgeSpan> Spans;

	public List<EdgeLoop> Loops;

	private TriangleBinsGrid2d triSpatial;

	public MeshInsertUVPolyCurve(DMesh3 mesh, PolyLine2d curve, bool isLoop = false)
	{
		Mesh = mesh;
		Curve = curve;
		IsLoop = isLoop;
		PointF = (int vid) => Mesh.GetVertex(vid).xy;
		SetPointF = delegate(int vid, Vector2d pos)
		{
			Mesh.SetVertex(vid, new Vector3d(pos.x, pos.y, 0.0));
		};
	}

	public MeshInsertUVPolyCurve(DMesh3 mesh, Polygon2d loop)
	{
		Mesh = mesh;
		Curve = new PolyLine2d(loop.Vertices);
		IsLoop = true;
		PointF = (int vid) => Mesh.GetVertex(vid).xy;
		SetPointF = delegate(int vid, Vector2d pos)
		{
			Mesh.SetVertex(vid, new Vector3d(pos.x, pos.y, 0.0));
		};
	}

	public MeshInsertUVPolyCurve(DMesh3 mesh, PolyLine2d path)
	{
		Mesh = mesh;
		Curve = new PolyLine2d(path.Vertices);
		IsLoop = false;
		PointF = (int vid) => Mesh.GetVertex(vid).xy;
		SetPointF = delegate(int vid, Vector2d pos)
		{
			Mesh.SetVertex(vid, new Vector3d(pos.x, pos.y, 0.0));
		};
	}

	public virtual ValidationStatus Validate(double fDegenerateTol = 9.999999974752427E-07)
	{
		double num = fDegenerateTol * fDegenerateTol;
		int num2 = (IsLoop ? (Curve.VertexCount - 1) : Curve.VertexCount);
		for (int i = 0; i < num2; i++)
		{
			Vector2d vector2d = Curve[i];
			Vector2d v = Curve[(i + 1) % Curve.VertexCount];
			if (vector2d.DistanceSquared(v) < num)
			{
				return ValidationStatus.NearDenegerateInputGeometry;
			}
		}
		foreach (int item in Mesh.EdgeIndices())
		{
			Index2i edgeV = Mesh.GetEdgeV(item);
			if (PointF(edgeV.a).DistanceSquared(PointF(edgeV.b)) < num)
			{
				return ValidationStatus.NearDegenerateMeshEdges;
			}
		}
		return ValidationStatus.Ok;
	}

	private void spatial_add_triangle(int tid)
	{
		if (triSpatial != null)
		{
			Index3i triangle = Mesh.GetTriangle(tid);
			Vector2d a = PointF(triangle.a);
			Vector2d b = PointF(triangle.b);
			Vector2d c = PointF(triangle.c);
			triSpatial.InsertTriangleUnsafe(tid, ref a, ref b, ref c);
		}
	}

	private void spatial_add_triangles(int t0, int t1)
	{
		if (triSpatial != null)
		{
			spatial_add_triangle(t0);
			if (t1 != -1)
			{
				spatial_add_triangle(t1);
			}
		}
	}

	private void spatial_remove_triangle(int tid)
	{
		if (triSpatial != null)
		{
			Index3i triangle = Mesh.GetTriangle(tid);
			Vector2d a = PointF(triangle.a);
			Vector2d b = PointF(triangle.b);
			Vector2d c = PointF(triangle.c);
			triSpatial.RemoveTriangleUnsafe(tid, ref a, ref b, ref c);
		}
	}

	private void spatial_remove_triangles(int t0, int t1)
	{
		if (triSpatial != null)
		{
			spatial_remove_triangle(t0);
			if (t1 != -1)
			{
				spatial_remove_triangle(t1);
			}
		}
	}

	private void insert_corners(HashSet<int> MeshVertsOnCurve)
	{
		PrimalQuery2d query = new PrimalQuery2d(PointF);
		if (UseTriSpatial)
		{
			int num = Mesh.TriangleCount + Curve.VertexCount;
			int numCells = 32;
			if (num < 25)
			{
				numCells = 8;
			}
			else if (num < 100)
			{
				numCells = 16;
			}
			AxisAlignedBox3d cachedBounds = Mesh.CachedBounds;
			AxisAlignedBox2d bounds = new AxisAlignedBox2d(cachedBounds.Min.xy, cachedBounds.Max.xy);
			triSpatial = new TriangleBinsGrid2d(bounds, numCells);
			foreach (int item in Mesh.TriangleIndices())
			{
				spatial_add_triangle(item);
			}
		}
		Func<int, Vector2d, bool> containsF = delegate(int tid, Vector2d pos)
		{
			Index3i triangle3 = Mesh.GetTriangle(tid);
			int num5 = query.ToTriangleUnsigned(pos, triangle3.a, triangle3.b, triangle3.c);
			return num5 == -1 || num5 == 0;
		};
		CurveVertices = new int[Curve.VertexCount];
		for (int i = 0; i < Curve.VertexCount; i++)
		{
			Vector2d vector2d = Curve[i];
			bool flag = false;
			int num2 = -1;
			if (triSpatial != null)
			{
				num2 = triSpatial.FindContainingTriangle(vector2d, containsF);
			}
			else
			{
				foreach (int item2 in Mesh.TriangleIndices())
				{
					Index3i triangle = Mesh.GetTriangle(item2);
					int num3 = query.ToTriangleUnsigned(vector2d, triangle.a, triangle.b, triangle.c);
					if (num3 == -1 || num3 == 0)
					{
						num2 = item2;
						break;
					}
				}
			}
			if (num2 != -1)
			{
				Index3i triangle2 = Mesh.GetTriangle(num2);
				Vector3d bary_coords = MathUtil.BarycentricCoords(vector2d, PointF(triangle2.a), PointF(triangle2.b), PointF(triangle2.c));
				bool is_existing_v;
				int num4 = insert_corner_from_bary(i, num2, bary_coords, 0.01, 100.0 * SpatialEpsilon, out is_existing_v);
				if (num4 > 0)
				{
					CurveVertices[i] = num4;
					if (is_existing_v)
					{
						MeshVertsOnCurve.Add(num4);
					}
					flag = true;
				}
			}
			if (!flag)
			{
				foreach (int item3 in Mesh.VertexIndices())
				{
					Vector2d v = PointF(item3);
					if (vector2d.Distance(v) < SpatialEpsilon)
					{
						CurveVertices[i] = item3;
						MeshVertsOnCurve.Add(item3);
						flag = true;
					}
				}
			}
			if (!flag)
			{
				throw new Exception("MeshInsertUVPolyCurve.insert_corners: curve vertex " + i + " is not inside or on any mesh triangle!");
			}
		}
	}

	private int insert_corner_from_bary(int iCorner, int tid, Vector3d bary_coords, double bary_tol, double spatial_tol, out bool is_existing_v)
	{
		is_existing_v = false;
		Vector2d vector2d = Curve[iCorner];
		Index3i triangle = Mesh.GetTriangle(tid);
		int num = -1;
		if (bary_coords.x > 1.0 - bary_tol)
		{
			num = triangle.a;
		}
		else if (bary_coords.y > 1.0 - bary_tol)
		{
			num = triangle.b;
		}
		else if (bary_coords.z > 1.0 - bary_tol)
		{
			num = triangle.c;
		}
		if (num != -1 && PointF(num).Distance(vector2d) < spatial_tol)
		{
			is_existing_v = true;
			return num;
		}
		int num2 = -1;
		if (bary_coords.x < bary_tol)
		{
			num2 = 1;
		}
		else if (bary_coords.y < bary_tol)
		{
			num2 = 2;
		}
		else if (bary_coords.z < bary_tol)
		{
			num2 = 0;
		}
		if (num2 >= 0)
		{
			int triEdge = Mesh.GetTriEdge(tid, num2);
			Index2i edgeV = Mesh.GetEdgeV(triEdge);
			if (new Segment2d(PointF(edgeV.a), PointF(edgeV.b)).DistanceSquared(vector2d) < spatial_tol * spatial_tol)
			{
				Index2i edgeT = Mesh.GetEdgeT(triEdge);
				spatial_remove_triangles(edgeT.a, edgeT.b);
				DMesh3.EdgeSplitInfo split;
				MeshResult meshResult = Mesh.SplitEdge(triEdge, out split);
				if (meshResult != 0)
				{
					throw new Exception("MeshInsertUVPolyCurve.insert_corner_from_bary: edge split failed in case sum==2 - " + meshResult);
				}
				SetPointF(split.vNew, vector2d);
				spatial_add_triangles(edgeT.a, edgeT.b);
				spatial_add_triangles(split.eNewT2, split.eNewT3);
				return split.vNew;
			}
		}
		spatial_remove_triangle(tid);
		DMesh3.PokeTriangleInfo result;
		MeshResult meshResult2 = Mesh.PokeTriangle(tid, bary_coords, out result);
		if (meshResult2 != 0)
		{
			throw new Exception("MeshInsertUVPolyCurve.insert_corner_from_bary: face poke failed - " + meshResult2);
		}
		SetPointF(result.new_vid, vector2d);
		spatial_add_triangle(tid);
		spatial_add_triangle(result.new_t1);
		spatial_add_triangle(result.new_t2);
		return result.new_vid;
	}

	public virtual bool Apply()
	{
		HashSet<int> hashSet = new HashSet<int>();
		insert_corners(hashSet);
		HashSet<int> hashSet2 = new HashSet<int>();
		HashSet<int> hashSet3 = new HashSet<int>();
		OnCutEdges = new HashSet<int>();
		HashSet<int> hashSet4 = new HashSet<int>();
		HashSet<int> hashSet5 = new HashSet<int>();
		sbyte[] signs = new sbyte[2 * Mesh.MaxVertexID + 2 * Curve.VertexCount];
		HashSet<int> hashSet6 = new HashSet<int>();
		HashSet<int> hashSet7 = new HashSet<int>();
		HashSet<int> hashSet8 = new HashSet<int>();
		int num = (IsLoop ? Curve.VertexCount : (Curve.VertexCount - 1));
		for (int i = 0; i < num; i++)
		{
			int num2 = i;
			int num3 = (i + 1) % Curve.VertexCount;
			Segment2d seg = new Segment2d(Curve[num2], Curve[num3]);
			int i0_vid = CurveVertices[num2];
			int i1_vid = CurveVertices[num3];
			int num4 = Mesh.FindEdge(i0_vid, i1_vid);
			if (num4 != -1)
			{
				add_cut_edge(num4);
				continue;
			}
			if (triSpatial != null)
			{
				hashSet6.Clear();
				hashSet7.Clear();
				hashSet8.Clear();
				AxisAlignedBox2d range = new AxisAlignedBox2d(seg.P0);
				range.Contain(seg.P1);
				range.Expand(9.999999747378752E-06);
				triSpatial.FindTrianglesInRange(range, hashSet6);
				IndexUtil.TrianglesToVertices(Mesh, hashSet6, hashSet7);
				IndexUtil.TrianglesToEdges(Mesh, hashSet6, hashSet8);
			}
			int maxVertexID = Mesh.MaxVertexID;
			IEnumerable<int> source = Interval1i.Range(maxVertexID);
			if (triSpatial != null)
			{
				source = hashSet7;
			}
			if (signs.Length < maxVertexID)
			{
				signs = new sbyte[2 * maxVertexID];
			}
			gParallel.ForEach(source, delegate(int vid)
			{
				if (Mesh.IsVertex(vid))
				{
					if (vid == i0_vid || vid == i1_vid)
					{
						signs[vid] = 0;
					}
					else
					{
						Vector2d test = PointF(vid);
						signs[vid] = (sbyte)seg.WhichSide(test, SpatialEpsilon);
					}
				}
				else
				{
					signs[vid] = sbyte.MaxValue;
				}
			});
			int maxEdgeID = Mesh.MaxEdgeID;
			hashSet4.Clear();
			hashSet5.Clear();
			hashSet5.Add(i0_vid);
			hashSet5.Add(i1_vid);
			IEnumerable<int> enumerable = Interval1i.Range(maxEdgeID);
			if (triSpatial != null)
			{
				enumerable = hashSet8;
			}
			foreach (int item2 in enumerable)
			{
				if (!Mesh.IsEdge(item2) || item2 >= maxEdgeID || hashSet4.Contains(item2) || Mesh.IsBoundaryEdge(item2))
				{
					continue;
				}
				Index2i edgeV = Mesh.GetEdgeV(item2);
				int num5 = signs[edgeV.a];
				int num6 = signs[edgeV.b];
				bool flag = false;
				if (num5 == 0)
				{
					flag = hashSet.Contains(edgeV.a) || Math.Abs(seg.Project(PointF(edgeV.a))) < seg.Extent + SpatialEpsilon;
				}
				bool flag2 = false;
				if (num6 == 0)
				{
					flag2 = hashSet.Contains(edgeV.b) || Math.Abs(seg.Project(PointF(edgeV.b))) < seg.Extent + SpatialEpsilon;
				}
				if (flag || flag2)
				{
					if (flag && flag2)
					{
						hashSet2.Add(item2);
						add_cut_edge(item2);
						hashSet5.Add(edgeV.a);
						hashSet5.Add(edgeV.b);
					}
					else
					{
						int item = (flag ? edgeV.a : edgeV.b);
						hashSet3.Add(item);
						hashSet5.Add(item);
					}
				}
				else
				{
					if (num5 * num6 > 0)
					{
						continue;
					}
					Vector2d vector2d = PointF(edgeV.a);
					Vector2d vector2d2 = PointF(edgeV.b);
					Segment2d seg2 = new Segment2d(vector2d, vector2d2);
					IntrSegment2Segment2 intrSegment2Segment = new IntrSegment2Segment2(seg, seg2);
					intrSegment2Segment.Compute();
					if (intrSegment2Segment.Type == IntersectionType.Segment)
					{
						hashSet2.Add(item2);
						hashSet5.Add(edgeV.a);
						hashSet5.Add(edgeV.b);
						add_cut_edge(item2);
					}
					else
					{
						if (intrSegment2Segment.Type != IntersectionType.Point)
						{
							continue;
						}
						Vector2d point = intrSegment2Segment.Point0;
						double split_t = Math.Sqrt(point.DistanceSquared(vector2d) / vector2d.DistanceSquared(vector2d2));
						if (!(Math.Abs(seg2.Project(point)) < seg2.Extent - SpatialEpsilon))
						{
							continue;
						}
						Index2i edgeT = Mesh.GetEdgeT(item2);
						spatial_remove_triangles(edgeT.a, edgeT.b);
						DMesh3.EdgeSplitInfo split;
						MeshResult meshResult = Mesh.SplitEdge(item2, out split, split_t);
						if (meshResult != 0)
						{
							throw new Exception("MeshInsertUVSegment.Apply: SplitEdge failed - " + meshResult);
						}
						SetPointF(split.vNew, point);
						hashSet5.Add(split.vNew);
						hashSet4.Add(split.eNewBN);
						hashSet4.Add(split.eNewCN);
						spatial_add_triangles(edgeT.a, edgeT.b);
						spatial_add_triangles(split.eNewT2, split.eNewT3);
						Index2i edgeV2 = Mesh.GetEdgeV(split.eNewCN);
						if (hashSet5.Contains(edgeV2.a) && hashSet5.Contains(edgeV2.b))
						{
							add_cut_edge(split.eNewCN);
						}
						if (split.eNewDN != -1)
						{
							hashSet4.Add(split.eNewDN);
							Index2i edgeV3 = Mesh.GetEdgeV(split.eNewDN);
							if (hashSet5.Contains(edgeV3.a) && hashSet5.Contains(edgeV3.b))
							{
								add_cut_edge(split.eNewDN);
							}
						}
					}
				}
			}
		}
		if (EnableCutSpansAndLoops)
		{
			find_cut_paths(OnCutEdges);
		}
		return true;
	}

	private void add_cut_edge(int eid)
	{
		OnCutEdges.Add(eid);
	}

	public void Simplify()
	{
		for (int i = 0; i < Loops.Count; i++)
		{
			EdgeLoop value = simplify(Loops[i]);
			Loops[i] = value;
		}
	}

	private EdgeLoop simplify(EdgeLoop loop)
	{
		HashSet<int> hashSet = new HashSet<int>(CurveVertices);
		List<int> list = new List<int>();
		for (int i = 0; i < loop.EdgeCount; i++)
		{
			int num = loop.Edges[i];
			Index2i edgeV = Mesh.GetEdgeV(num);
			if (hashSet.Contains(edgeV.a) && hashSet.Contains(edgeV.b))
			{
				list.Add(num);
				continue;
			}
			int num2 = edgeV.a;
			int num3 = edgeV.b;
			Vector3d zero = Vector3d.Zero;
			if (!hashSet.Contains(edgeV.b))
			{
				zero = ((!hashSet.Contains(edgeV.a)) ? (0.5 * (Mesh.GetVertex(edgeV.a) + Mesh.GetVertex(edgeV.b))) : Mesh.GetVertex(edgeV.a));
			}
			else
			{
				num2 = edgeV.b;
				num3 = edgeV.a;
				zero = Mesh.GetVertex(edgeV.b);
			}
			if (MeshUtil.CheckIfCollapseCreatesFlip(Mesh, num, zero))
			{
				list.Add(num);
				continue;
			}
			Index4i edge = Mesh.GetEdge(num);
			int vB = IndexUtil.find_tri_other_vtx(num2, num3, Mesh.GetTriangle(edge.c));
			int vB2 = IndexUtil.find_tri_other_vtx(num2, num3, Mesh.GetTriangle(edge.d));
			int item = Mesh.FindEdge(num3, vB);
			int item2 = Mesh.FindEdge(num3, vB2);
			DMesh3.EdgeCollapseInfo collapse;
			if (OnCutEdges.Contains(item) || OnCutEdges.Contains(item2))
			{
				list.Add(num);
			}
			else if (Mesh.CollapseEdge(num2, num3, out collapse) == MeshResult.Ok)
			{
				Mesh.SetVertex(collapse.vKept, zero);
				OnCutEdges.Remove(collapse.eCollapsed);
			}
			else
			{
				list.Add(num);
			}
		}
		return EdgeLoop.FromEdges(Mesh, list);
	}

	private void find_cut_paths(HashSet<int> CutEdges)
	{
		Spans = new List<EdgeSpan>();
		Loops = new List<EdgeLoop>();
		HashSet<int> hashSet = new HashSet<int>(CutEdges);
		while (hashSet.Count > 0)
		{
			int num = hashSet.First();
			hashSet.Remove(num);
			Index2i edgeV = Mesh.GetEdgeV(num);
			bool bClosedLoop;
			List<int> list = walk_edge_span_forward(Mesh, num, edgeV.a, hashSet, out bClosedLoop);
			if (!bClosedLoop)
			{
				List<int> list2 = walk_edge_span_forward(Mesh, num, edgeV.b, hashSet, out bClosedLoop);
				if (bClosedLoop)
				{
					throw new Exception("find_cut_paths: how did this possibly happen?!?");
				}
				if (list2.Count > 1)
				{
					list2.Reverse();
					list2.RemoveAt(list2.Count - 1);
					list2.AddRange(list);
					Index2i ev = Mesh.GetEdgeV(list2[0]);
					Index2i ev2 = Mesh.GetEdgeV(list2[list2.Count - 1]);
					bClosedLoop = list2.Count > 2 && IndexUtil.find_shared_edge_v(ref ev, ref ev2) != -1;
					list = list2;
				}
			}
			if (bClosedLoop)
			{
				EdgeLoop item = EdgeLoop.FromEdges(Mesh, list);
				Loops.Add(item);
			}
			else
			{
				EdgeSpan item2 = EdgeSpan.FromEdges(Mesh, list);
				Spans.Add(item2);
			}
		}
	}

	private static List<int> walk_edge_span_forward(DMesh3 mesh, int start_edge, int start_pivot_v, HashSet<int> EdgeSet, out bool bClosedLoop)
	{
		bClosedLoop = false;
		List<int> list = new List<int>();
		list.Add(start_edge);
		int num = start_pivot_v;
		int num2 = IndexUtil.find_edge_other_v(mesh.GetEdgeV(start_edge), start_pivot_v);
		bool flag = false;
		while (!flag)
		{
			int num3 = -1;
			foreach (int item in mesh.VtxEdgesItr(num))
			{
				if (EdgeSet.Contains(item))
				{
					num3 = item;
					break;
				}
			}
			if (num3 == -1)
			{
				flag = true;
				break;
			}
			Index2i edgeV = mesh.GetEdgeV(num3);
			if (edgeV.a == num)
			{
				num = edgeV.b;
			}
			else
			{
				if (edgeV.b != num)
				{
					throw new Exception("walk_edge_span_forward: found valid next edge but not connected to previous vertex??");
				}
				num = edgeV.a;
			}
			list.Add(num3);
			EdgeSet.Remove(num3);
			if (num == num2)
			{
				flag = true;
				bClosedLoop = true;
			}
		}
		return list;
	}
}
