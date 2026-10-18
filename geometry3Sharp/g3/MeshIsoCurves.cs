using System;
using System.Collections.Generic;

namespace g3;

public class MeshIsoCurves
{
	public enum RootfindingModes
	{
		SingleLerp,
		LerpSteps,
		Bisection
	}

	public enum TriangleCase
	{
		EdgeEdge = 1,
		EdgeVertex,
		OnEdge
	}

	public struct GraphEdgeInfo
	{
		public TriangleCase caseType;

		public int mesh_tri;

		public Index2i mesh_edges;

		public Index2i order;
	}

	public DMesh3 Mesh;

	public Func<Vector3d, double> ValueF;

	public Func<int, double> VertexValueF;

	public bool PrecomputeVertexValues;

	public RootfindingModes RootMode;

	public int RootModeSteps = 5;

	public DGraph3 Graph;

	public bool WantGraphEdgeInfo;

	public DVector<GraphEdgeInfo> GraphEdges;

	private Dictionary<int, Vector3d> EdgeLocations = new Dictionary<int, Vector3d>();

	private Dictionary<Vector3d, int> Vertices;

	public MeshIsoCurves(DMesh3 mesh, Func<Vector3d, double> valueF)
	{
		Mesh = mesh;
		ValueF = valueF;
	}

	public void Compute()
	{
		compute_full(Mesh.TriangleIndices(), bIsFullMeshHint: true);
	}

	public void Compute(IEnumerable<int> Triangles)
	{
		compute_full(Triangles);
	}

	protected void compute_full(IEnumerable<int> Triangles, bool bIsFullMeshHint = false)
	{
		Graph = new DGraph3();
		if (WantGraphEdgeInfo)
		{
			GraphEdges = new DVector<GraphEdgeInfo>();
		}
		Vertices = new Dictionary<Vector3d, int>();
		double[] vertex_values = null;
		if (PrecomputeVertexValues)
		{
			vertex_values = new double[Mesh.MaxVertexID];
			IEnumerable<int> source = Mesh.VertexIndices();
			if (!bIsFullMeshHint)
			{
				MeshVertexSelection meshVertexSelection = new MeshVertexSelection(Mesh);
				meshVertexSelection.SelectTriangleVertices(Triangles);
				source = meshVertexSelection;
			}
			gParallel.ForEach(source, delegate(int vid)
			{
				vertex_values[vid] = ValueF(Mesh.GetVertex(vid));
			});
			VertexValueF = (int vid) => vertex_values[vid];
		}
		foreach (int Triangle in Triangles)
		{
			Vector3dTuple3 vector3dTuple = default(Vector3dTuple3);
			Mesh.GetTriVertices(Triangle, ref vector3dTuple.V0, ref vector3dTuple.V1, ref vector3dTuple.V2);
			Index3i triangle = Mesh.GetTriangle(Triangle);
			Vector3d vector3d = ((VertexValueF != null) ? new Vector3d(VertexValueF(triangle.a), VertexValueF(triangle.b), VertexValueF(triangle.c)) : new Vector3d(ValueF(vector3dTuple.V0), ValueF(vector3dTuple.V1), ValueF(vector3dTuple.V2)));
			if ((vector3d.x < 0.0 && vector3d.y < 0.0 && vector3d.z < 0.0) || (vector3d.x > 0.0 && vector3d.y > 0.0 && vector3d.z > 0.0))
			{
				continue;
			}
			Index3i triEdges = Mesh.GetTriEdges(Triangle);
			if (vector3d.x * vector3d.y * vector3d.z == 0.0)
			{
				int num = ((vector3d.x != 0.0) ? ((vector3d.y == 0.0) ? 1 : 2) : 0);
				int num2 = (num + 1) % 3;
				int num3 = (num + 2) % 3;
				if (vector3d[num2] * vector3d[num3] > 0.0)
				{
					continue;
				}
				if (vector3d[num2] == 0.0 || vector3d[num3] == 0.0)
				{
					int num4 = ((vector3d[num2] == 0.0) ? num2 : num3);
					if ((num + 1) % 3 != num4)
					{
						int num5 = num;
						num = num4;
						num4 = num5;
					}
					int num6 = add_or_append_vertex(Mesh.GetVertex(triangle[num]));
					int num7 = add_or_append_vertex(Mesh.GetVertex(triangle[num4]));
					int num8 = Graph.AppendEdge(num6, num7, 3);
					if (num8 >= 0 && WantGraphEdgeInfo)
					{
						add_on_edge(num8, Triangle, triEdges[num], new Index2i(num6, num7));
					}
					continue;
				}
				int num9 = add_or_append_vertex(Mesh.GetVertex(triangle[num]));
				int num10 = num2;
				int num11 = num3;
				if (triangle[num11] < triangle[num10])
				{
					int num12 = num10;
					num10 = num11;
					num11 = num12;
				}
				Vector3d vector3d2 = find_crossing(vector3dTuple[num10], vector3dTuple[num11], vector3d[num10], vector3d[num11]);
				int num13 = add_or_append_vertex(vector3d2);
				add_edge_pos(triangle[num10], triangle[num11], vector3d2);
				if (num9 != num13)
				{
					int num14 = Graph.AppendEdge(num9, num13, 2);
					if (num14 >= 0 && WantGraphEdgeInfo)
					{
						add_edge_vert(num14, Triangle, triEdges[(num + 1) % 3], triangle[num], new Index2i(num9, num13));
					}
				}
				continue;
			}
			Index3i min = Index3i.Min;
			int num15 = 0;
			for (int i = 0; i < 3; i++)
			{
				int num16 = i;
				int num17 = (i + 1) % 3;
				if (vector3d[num16] < 0.0)
				{
					num15++;
				}
				if (!(vector3d[num16] * vector3d[num17] > 0.0))
				{
					if (triangle[num17] < triangle[num16])
					{
						int num18 = num16;
						num16 = num17;
						num17 = num18;
					}
					Vector3d vector3d3 = find_crossing(vector3dTuple[num16], vector3dTuple[num17], vector3d[num16], vector3d[num17]);
					min[i] = add_or_append_vertex(vector3d3);
					add_edge_pos(triangle[num16], triangle[num17], vector3d3);
				}
			}
			int num19 = ((min.a == int.MinValue) ? 1 : 0);
			int num20 = ((min.c == int.MinValue) ? 1 : 2);
			if (num19 == 0 && num20 == 2)
			{
				num19 = 2;
				num20 = 0;
			}
			if (num15 == 1)
			{
				int num21 = num19;
				num19 = num20;
				num20 = num21;
			}
			int num22 = min[num19];
			int num23 = min[num20];
			if (num22 != num23)
			{
				int num24 = Graph.AppendEdge(num22, num23, 1);
				if (num24 >= 0 && WantGraphEdgeInfo)
				{
					add_edge_edge(num24, Triangle, new Index2i(triEdges[num19], triEdges[num20]), new Index2i(num22, num23));
				}
			}
		}
		Vertices = null;
	}

	private int add_or_append_vertex(Vector3d pos)
	{
		if (!Vertices.TryGetValue(pos, out var value))
		{
			value = Graph.AppendVertex(pos);
			Vertices.Add(pos, value);
		}
		return value;
	}

	private void add_edge_edge(int graph_eid, int mesh_tri, Index2i mesh_edges, Index2i order)
	{
		GraphEdgeInfo graphEdgeInfo = default(GraphEdgeInfo);
		graphEdgeInfo.caseType = TriangleCase.EdgeEdge;
		graphEdgeInfo.mesh_edges = mesh_edges;
		graphEdgeInfo.mesh_tri = mesh_tri;
		graphEdgeInfo.order = order;
		GraphEdgeInfo value = graphEdgeInfo;
		GraphEdges.insertAt(value, graph_eid);
	}

	private void add_edge_vert(int graph_eid, int mesh_tri, int mesh_edge, int mesh_vert, Index2i order)
	{
		GraphEdgeInfo graphEdgeInfo = default(GraphEdgeInfo);
		graphEdgeInfo.caseType = TriangleCase.EdgeVertex;
		graphEdgeInfo.mesh_edges = new Index2i(mesh_edge, mesh_vert);
		graphEdgeInfo.mesh_tri = mesh_tri;
		graphEdgeInfo.order = order;
		GraphEdgeInfo value = graphEdgeInfo;
		GraphEdges.insertAt(value, graph_eid);
	}

	private void add_on_edge(int graph_eid, int mesh_tri, int mesh_edge, Index2i order)
	{
		GraphEdgeInfo graphEdgeInfo = default(GraphEdgeInfo);
		graphEdgeInfo.caseType = TriangleCase.OnEdge;
		graphEdgeInfo.mesh_edges = new Index2i(mesh_edge, -1);
		graphEdgeInfo.mesh_tri = mesh_tri;
		graphEdgeInfo.order = order;
		GraphEdgeInfo value = graphEdgeInfo;
		GraphEdges.insertAt(value, graph_eid);
	}

	private Vector3d find_crossing(Vector3d a, Vector3d b, double fA, double fB)
	{
		if (fB < fA)
		{
			Vector3d vector3d = a;
			a = b;
			b = vector3d;
			double num = fA;
			fA = fB;
			fB = num;
		}
		if (RootMode == RootfindingModes.Bisection)
		{
			for (int i = 0; i < RootModeSteps; i++)
			{
				Vector3d vector3d2 = Vector3d.Lerp(a, b, 0.5);
				double num2 = ValueF(vector3d2);
				if (num2 < 0.0)
				{
					fA = num2;
					a = vector3d2;
				}
				else
				{
					fB = num2;
					b = vector3d2;
				}
			}
			return Vector3d.Lerp(a, b, 0.5);
		}
		if (Math.Abs(fB - fA) < 1E-08)
		{
			return a;
		}
		double num3 = 0.0;
		if (RootMode == RootfindingModes.LerpSteps)
		{
			for (int j = 0; j < RootModeSteps; j++)
			{
				num3 = MathUtil.Clamp((0.0 - fA) / (fB - fA), 0.0, 1.0);
				Vector3d vector3d3 = (1.0 - num3) * a + num3 * b;
				double num4 = ValueF(vector3d3);
				if (num4 < 0.0)
				{
					fA = num4;
					a = vector3d3;
				}
				else
				{
					fB = num4;
					b = vector3d3;
				}
			}
		}
		num3 = MathUtil.Clamp((0.0 - fA) / (fB - fA), 0.0, 1.0);
		return (1.0 - num3) * a + num3 * b;
	}

	private void add_edge_pos(int a, int b, Vector3d crossing_pos)
	{
		int num = Mesh.FindEdge(a, b);
		if (num == -1)
		{
			throw new Exception("MeshIsoCurves.add_edge_split: invalid edge?");
		}
		if (!EdgeLocations.ContainsKey(num))
		{
			EdgeLocations[num] = crossing_pos;
		}
	}

	public void SplitAtIsoCrossings(double min_len = 0.0)
	{
		foreach (KeyValuePair<int, Vector3d> edgeLocation in EdgeLocations)
		{
			int key = edgeLocation.Key;
			Vector3d value = edgeLocation.Value;
			if (!Mesh.IsEdge(key))
			{
				continue;
			}
			Index2i edgeV = Mesh.GetEdgeV(key);
			Vector3d vertex = Mesh.GetVertex(edgeV.a);
			Vector3d vertex2 = Mesh.GetVertex(edgeV.b);
			if (!(vertex.Distance(vertex2) < min_len))
			{
				Vector3d v = (vertex + vertex2) * 0.5;
				if (!(vertex.Distance(v) < min_len) && !(vertex2.Distance(v) < min_len) && Mesh.SplitEdge(key, out var split) == MeshResult.Ok)
				{
					Mesh.SetVertex(split.vNew, value);
				}
			}
		}
	}

	public bool ShouldReverseGraphEdge(int graph_eid)
	{
		if (GraphEdges == null)
		{
			throw new Exception("MeshIsoCurves.OrientEdge: must track edge graph info to orient edge");
		}
		Index2i edgeV = Graph.GetEdgeV(graph_eid);
		GraphEdgeInfo graphEdgeInfo = GraphEdges[graph_eid];
		if (edgeV.b == graphEdgeInfo.order.a && edgeV.a == graphEdgeInfo.order.b)
		{
			return true;
		}
		return false;
	}
}
