using System;
using System.Collections.Generic;
using System.Linq;
using g3;

namespace gs;

public class MinimalHoleFill
{
	public DMesh3 Mesh;

	public EdgeLoop FillLoop;

	public bool IgnoreBoundaryTriangles;

	public bool OptimizeDevelopability = true;

	public bool OptimizeTriangles = true;

	public double DevelopabilityTolerance = 0.0001;

	public int[] FillVertices;

	public int[] FillTriangles;

	private RegionOperator regionop;

	private DMesh3 fillmesh;

	private HashSet<int> boundaryv;

	private Dictionary<int, double> exterior_angle_sums;

	private double[] curvatures;

	public MinimalHoleFill(DMesh3 mesh, EdgeLoop fillLoop)
	{
		Mesh = mesh;
		FillLoop = fillLoop;
	}

	public bool Apply()
	{
		SimpleHoleFiller simpleHoleFiller = new SimpleHoleFiller(Mesh, FillLoop);
		int group_id = Mesh.AllocateTriangleGroup();
		if (!simpleHoleFiller.Fill(group_id))
		{
			return false;
		}
		if (FillLoop.Vertices.Length <= 3)
		{
			FillTriangles = simpleHoleFiller.NewTriangles;
			FillVertices = new int[0];
			return true;
		}
		HashSet<int> hashSet = new HashSet<int>(simpleHoleFiller.NewTriangles);
		regionop = new RegionOperator(Mesh, simpleHoleFiller.NewTriangles, delegate(DSubmesh3 submesh)
		{
			submesh.ComputeTriMaps = true;
		});
		fillmesh = regionop.Region.SubMesh;
		boundaryv = new HashSet<int>(MeshIterators.BoundaryEdgeVertices(fillmesh));
		exterior_angle_sums = new Dictionary<int, double>();
		if (!IgnoreBoundaryTriangles)
		{
			foreach (int item in boundaryv)
			{
				double num = 0.0;
				int num2 = regionop.Region.MapVertexToBaseMesh(item);
				foreach (int item2 in regionop.BaseMesh.VtxTrianglesItr(num2))
				{
					if (!hashSet.Contains(item2))
					{
						Index3i tri_verts = regionop.BaseMesh.GetTriangle(item2);
						int i = IndexUtil.find_tri_index(num2, ref tri_verts);
						num += regionop.BaseMesh.GetTriInternalAngleR(item2, i);
					}
				}
				exterior_angle_sums[item] = num;
			}
		}
		MeshQueries.EdgeLengthStatsFromEdges(Mesh, FillLoop.Edges, out var _, out var _, out var avgEdgeLen);
		MeshQueries.EdgeLengthStats(fillmesh, out var _, out var maxEdgeLen2, out var _);
		double num3 = avgEdgeLen;
		if (maxEdgeLen2 / num3 > 10.0)
		{
			num3 = maxEdgeLen2 / 10.0;
		}
		RemesherPro obj = new RemesherPro(fillmesh)
		{
			SmoothSpeedT = 1.0
		};
		MeshConstraintUtil.FixAllBoundaryEdges(obj);
		obj.SetTargetEdgeLength(num3);
		obj.FastestRemesh();
		int num4 = 0;
		int num5 = 0;
		while (num5++ < 20 && num4 < 2)
		{
			int maxEdgeID = fillmesh.MaxEdgeID;
			int num6 = 0;
			for (int j = 0; j < maxEdgeID; j++)
			{
				if (!fillmesh.IsEdge(j) || fillmesh.IsBoundaryEdge(j))
				{
					continue;
				}
				Index2i edgeV = fillmesh.GetEdgeV(j);
				bool flag = boundaryv.Contains(edgeV.a);
				bool flag2 = boundaryv.Contains(edgeV.b);
				if (!(flag && flag2))
				{
					int num7 = (flag ? edgeV.a : edgeV.b);
					int vRemove = ((num7 == edgeV.a) ? edgeV.b : edgeV.a);
					Vector3d vertex = fillmesh.GetVertex(num7);
					if (!MeshUtil.CheckIfCollapseCreatesFlip(fillmesh, j, vertex) && fillmesh.CollapseEdge(num7, vRemove, out var _) == MeshResult.Ok)
					{
						num6++;
					}
				}
			}
			num4 = ((num6 == 0) ? (num4 + 1) : 0);
			maxEdgeID = fillmesh.MaxEdgeID;
			for (int k = 0; k < maxEdgeID; k++)
			{
				if (!fillmesh.IsEdge(k) || fillmesh.IsBoundaryEdge(k))
				{
					continue;
				}
				bool flag3 = false;
				Index2i edgeV2 = fillmesh.GetEdgeV(k);
				MeshUtil.GetEdgeFlipNormals(fillmesh, k, out var n, out var n2, out var on, out var on2);
				double num8 = n.Dot(n2);
				double num9 = on.Dot(on2);
				if (n.Dot(n2) < 0.1 || num9 > num8 + 1.1920928955078125E-07)
				{
					flag3 = true;
				}
				if (!flag3)
				{
					Index2i edgeOpposingV = fillmesh.GetEdgeOpposingV(k);
					double num10 = fillmesh.GetVertex(edgeV2.a).Distance(fillmesh.GetVertex(edgeV2.b));
					if (fillmesh.GetVertex(edgeOpposingV.a).Distance(fillmesh.GetVertex(edgeOpposingV.b)) < num10 && !MeshUtil.CheckIfEdgeFlipCreatesFlip(fillmesh, k))
					{
						flag3 = true;
					}
				}
				if (flag3)
				{
					fillmesh.FlipEdge(k, out var _);
				}
			}
		}
		remove_remaining_interior_verts();
		bool flag4 = true;
		bool optimizeDevelopability = OptimizeDevelopability;
		bool flag5 = OptimizeDevelopability && OptimizeTriangles;
		HashSet<int> hashSet2 = new HashSet<int>(fillmesh.EdgeIndices());
		HashSet<int> hashSet3 = new HashSet<int>();
		int num11 = 0;
		int num12 = 0;
		while (num11++ < 40 && num12 < 2 && hashSet2.Count() > 0 && flag4)
		{
			num12++;
			foreach (int item3 in hashSet2)
			{
				if (!fillmesh.IsBoundaryEdge(item3))
				{
					bool flag6 = false;
					fillmesh.GetEdgeV(item3);
					MeshUtil.GetEdgeFlipNormals(fillmesh, item3, out var n3, out var n4, out var on3, out var on4);
					double num13 = n3.Dot(n4);
					double num14 = on3.Dot(on4);
					if (num11 < 20 && num13 < 0.1)
					{
						flag6 = true;
					}
					if (num14 > num13 + 1.1920928955078125E-07)
					{
						flag6 = true;
					}
					if (flag6 && fillmesh.FlipEdge(item3, out var _) == MeshResult.Ok)
					{
						num12 = 0;
						add_all_edges(item3, hashSet3);
					}
				}
			}
			HashSet<int> hashSet4 = hashSet2;
			hashSet2 = hashSet3;
			hashSet3 = hashSet4;
			hashSet3.Clear();
		}
		int num15 = 0;
		if (optimizeDevelopability)
		{
			curvatures = new double[fillmesh.MaxVertexID];
			foreach (int item4 in fillmesh.VertexIndices())
			{
				update_curvature(item4);
			}
			hashSet2 = new HashSet<int>(fillmesh.EdgeIndices());
			hashSet3 = new HashSet<int>();
			while (num15++ < 40 && hashSet2.Count() > 0 && optimizeDevelopability)
			{
				foreach (int item5 in hashSet2)
				{
					if (fillmesh.IsBoundaryEdge(item5))
					{
						continue;
					}
					Index2i edgeV3 = fillmesh.GetEdgeV(item5);
					Index2i edgeOpposingV2 = fillmesh.GetEdgeOpposingV(item5);
					if (fillmesh.FindEdge(edgeOpposingV2.a, edgeOpposingV2.b) != -1)
					{
						continue;
					}
					double num16 = curvature_metric_cached(edgeV3.a, edgeV3.b, edgeOpposingV2.a, edgeOpposingV2.b);
					if (!(num16 < 9.999999974752427E-07) && fillmesh.FlipEdge(item5, out var flip3) == MeshResult.Ok)
					{
						if (!(curvature_metric_eval(edgeV3.a, edgeV3.b, edgeOpposingV2.a, edgeOpposingV2.b) < num16 - 9.999999974752427E-07))
						{
							fillmesh.FlipEdge(item5, out flip3);
							continue;
						}
						update_curvature(edgeV3.a);
						update_curvature(edgeV3.b);
						update_curvature(edgeOpposingV2.a);
						update_curvature(edgeOpposingV2.b);
						add_all_edges(item5, hashSet3);
					}
				}
				HashSet<int> hashSet5 = hashSet2;
				hashSet2 = hashSet3;
				hashSet3 = hashSet5;
				hashSet3.Clear();
			}
		}
		if (flag5)
		{
			hashSet2 = new HashSet<int>(fillmesh.EdgeIndices());
			hashSet3 = new HashSet<int>();
			int num17 = 0;
			while (hashSet2.Count() > 0 && num17 < 20)
			{
				num17++;
				foreach (int item6 in hashSet2)
				{
					if (fillmesh.IsBoundaryEdge(item6))
					{
						continue;
					}
					Index2i edgeV4 = fillmesh.GetEdgeV(item6);
					Index2i edgeOpposingV3 = fillmesh.GetEdgeOpposingV(item6);
					if (fillmesh.FindEdge(edgeOpposingV3.a, edgeOpposingV3.b) != -1)
					{
						continue;
					}
					double num18 = curvature_metric_cached(edgeV4.a, edgeV4.b, edgeOpposingV3.a, edgeOpposingV3.b);
					if (!(aspect_metric(item6) > 1.0) && fillmesh.FlipEdge(item6, out var flip4) == MeshResult.Ok)
					{
						double num19 = curvature_metric_eval(edgeV4.a, edgeV4.b, edgeOpposingV3.a, edgeOpposingV3.b);
						if (!(Math.Abs(num18 - num19) < DevelopabilityTolerance))
						{
							fillmesh.FlipEdge(item6, out flip4);
							continue;
						}
						update_curvature(edgeV4.a);
						update_curvature(edgeV4.b);
						update_curvature(edgeOpposingV3.a);
						update_curvature(edgeOpposingV3.b);
						add_all_edges(item6, hashSet3);
					}
				}
				HashSet<int> hashSet6 = hashSet2;
				hashSet2 = hashSet3;
				hashSet3 = hashSet6;
				hashSet3.Clear();
			}
		}
		regionop.BackPropropagate();
		FillTriangles = regionop.CurrentBaseTriangles;
		FillVertices = regionop.CurrentBaseInteriorVertices().ToArray();
		return true;
	}

	private void remove_remaining_interior_verts()
	{
		HashSet<int> hashSet = new HashSet<int>(MeshIterators.InteriorVertices(fillmesh));
		int num = 0;
		while (hashSet.Count > 0 && hashSet.Count != num)
		{
			num = hashSet.Count;
			int[] array = hashSet.ToArray();
			foreach (int num2 in array)
			{
				foreach (int item in fillmesh.VtxEdgesItr(num2))
				{
					Index2i edgeV = fillmesh.GetEdgeV(item);
					int vKeep = ((edgeV.a == num2) ? edgeV.b : edgeV.a);
					if (fillmesh.CollapseEdge(vKeep, num2, out var _) == MeshResult.Ok)
					{
						break;
					}
				}
				if (!fillmesh.IsVertex(num2))
				{
					hashSet.Remove(num2);
				}
			}
		}
		if (hashSet.Count > 0)
		{
			Util.gBreakToDebugger();
		}
	}

	private void add_all_edges(int ei, HashSet<int> edge_set)
	{
		Index2i edgeT = fillmesh.GetEdgeT(ei);
		Index3i triEdges = fillmesh.GetTriEdges(edgeT.a);
		edge_set.Add(triEdges.a);
		edge_set.Add(triEdges.b);
		edge_set.Add(triEdges.c);
		triEdges = fillmesh.GetTriEdges(edgeT.b);
		edge_set.Add(triEdges.a);
		edge_set.Add(triEdges.b);
		edge_set.Add(triEdges.c);
	}

	private double area_metric(int eid)
	{
		MeshUtil.GetEdgeFlipTris(fillmesh, eid, out var orig_t, out var orig_t2, out var flip_t, out var flip_t2);
		double num = get_tri_area(fillmesh, ref orig_t);
		double num2 = get_tri_area(fillmesh, ref orig_t2);
		double num3 = get_tri_area(fillmesh, ref flip_t);
		double num4 = get_tri_area(fillmesh, ref flip_t2);
		double num5 = (num + num2) * 0.5;
		double num6 = (num3 + num4) * 0.5;
		double num7 = Math.Abs(num - num5) + Math.Abs(num2 - num5);
		return (Math.Abs(num3 - num6) + Math.Abs(num4 - num6)) / num7;
	}

	private double aspect_metric(int eid)
	{
		MeshUtil.GetEdgeFlipTris(fillmesh, eid, out var orig_t, out var orig_t2, out var flip_t, out var flip_t2);
		double num = get_tri_aspect(fillmesh, ref orig_t);
		double num2 = get_tri_aspect(fillmesh, ref orig_t2);
		double num3 = get_tri_aspect(fillmesh, ref flip_t);
		double num4 = get_tri_aspect(fillmesh, ref flip_t2);
		double num5 = Math.Abs(num - 1.0) + Math.Abs(num2 - 1.0);
		return (Math.Abs(num3 - 1.0) + Math.Abs(num4 - 1.0)) / num5;
	}

	private void update_curvature(int vid)
	{
		double value = 0.0;
		exterior_angle_sums.TryGetValue(vid, out value);
		foreach (int item in fillmesh.VtxTrianglesItr(vid))
		{
			Index3i tri_verts = fillmesh.GetTriangle(item);
			int i = IndexUtil.find_tri_index(vid, ref tri_verts);
			value += fillmesh.GetTriInternalAngleR(item, i);
		}
		curvatures[vid] = value - Math.PI * 2.0;
	}

	private double curvature_metric_cached(int a, int b, int c, int d)
	{
		double value = curvatures[a];
		double value2 = curvatures[b];
		double value3 = curvatures[c];
		double value4 = curvatures[d];
		return Math.Abs(value) + Math.Abs(value2) + Math.Abs(value3) + Math.Abs(value4);
	}

	private double curvature_metric_eval(int a, int b, int c, int d)
	{
		double value = compute_gauss_curvature(a);
		double value2 = compute_gauss_curvature(b);
		double value3 = compute_gauss_curvature(c);
		double value4 = compute_gauss_curvature(d);
		return Math.Abs(value) + Math.Abs(value2) + Math.Abs(value3) + Math.Abs(value4);
	}

	private double compute_gauss_curvature(int vid)
	{
		double value = 0.0;
		exterior_angle_sums.TryGetValue(vid, out value);
		foreach (int item in fillmesh.VtxTrianglesItr(vid))
		{
			Index3i tri_verts = fillmesh.GetTriangle(item);
			int i = IndexUtil.find_tri_index(vid, ref tri_verts);
			value += fillmesh.GetTriInternalAngleR(item, i);
		}
		return value - Math.PI * 2.0;
	}

	private Vector3d get_tri_normal(DMesh3 mesh, Index3i tri)
	{
		return MathUtil.Normal(mesh.GetVertex(tri.a), mesh.GetVertex(tri.b), mesh.GetVertex(tri.c));
	}

	private double get_tri_area(DMesh3 mesh, ref Index3i tri)
	{
		return MathUtil.Area(mesh.GetVertex(tri.a), mesh.GetVertex(tri.b), mesh.GetVertex(tri.c));
	}

	private double get_tri_aspect(DMesh3 mesh, ref Index3i tri)
	{
		return MathUtil.AspectRatio(mesh.GetVertex(tri.a), mesh.GetVertex(tri.b), mesh.GetVertex(tri.c));
	}
}
