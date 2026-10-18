using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class MeshTopology
{
	public DMesh3 Mesh;

	private double crease_angle = 30.0;

	public HashSet<int> BoundaryEdges;

	public HashSet<int> CreaseEdges;

	public HashSet<int> AllEdges;

	public HashSet<int> AllVertices;

	public HashSet<int> JunctionVertices;

	public EdgeLoop[] Loops;

	public EdgeSpan[] Spans;

	private int topo_timestamp = -1;

	public bool IgnoreTimestamp;

	public double CreaseAngle
	{
		get
		{
			return crease_angle;
		}
		set
		{
			crease_angle = value;
			invalidate_topology();
		}
	}

	public MeshTopology(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public void Compute()
	{
		validate_topology();
	}

	public void AddRemeshConstraints(MeshConstraints constraints)
	{
		validate_topology();
		int num = 10;
		EdgeSpan[] spans = Spans;
		foreach (EdgeSpan edgeSpan in spans)
		{
			DCurveProjectionTarget target = new DCurveProjectionTarget(edgeSpan.ToCurve());
			MeshConstraintUtil.ConstrainVtxSpanTo(constraints, Mesh, edgeSpan.Vertices, target, num++);
		}
		EdgeLoop[] loops = Loops;
		foreach (EdgeLoop edgeLoop in loops)
		{
			DCurveProjectionTarget target2 = new DCurveProjectionTarget(edgeLoop.ToCurve());
			MeshConstraintUtil.ConstrainVtxLoopTo(constraints, Mesh, edgeLoop.Vertices, target2, num++);
		}
		VertexConstraint pinned = VertexConstraint.Pinned;
		pinned.FixedSetID = -1;
		foreach (int junctionVertex in JunctionVertices)
		{
			if (constraints.HasVertexConstraint(junctionVertex))
			{
				VertexConstraint vertexConstraint = constraints.GetVertexConstraint(junctionVertex);
				vertexConstraint.Target = null;
				vertexConstraint.Fixed = true;
				vertexConstraint.FixedSetID = -1;
				constraints.SetOrUpdateVertexConstraint(junctionVertex, vertexConstraint);
			}
			else
			{
				constraints.SetOrUpdateVertexConstraint(junctionVertex, pinned);
			}
		}
	}

	private void invalidate_topology()
	{
		topo_timestamp = -1;
	}

	private void validate_topology()
	{
		if ((!IgnoreTimestamp || AllEdges == null) && Mesh.ShapeTimestamp != topo_timestamp)
		{
			find_crease_edges(CreaseAngle);
			extract_topology();
			topo_timestamp = Mesh.ShapeTimestamp;
		}
	}

	private void find_crease_edges(double angle_tol)
	{
		CreaseEdges = new HashSet<int>();
		BoundaryEdges = new HashSet<int>();
		double num = Math.Cos(angle_tol * (Math.PI / 180.0));
		foreach (int item in Mesh.EdgeIndices())
		{
			Index2i edgeT = Mesh.GetEdgeT(item);
			if (edgeT.b == -1)
			{
				BoundaryEdges.Add(item);
				continue;
			}
			Vector3d triNormal = Mesh.GetTriNormal(edgeT.a);
			Vector3d triNormal2 = Mesh.GetTriNormal(edgeT.b);
			if (Math.Abs(triNormal.Dot(triNormal2)) < num)
			{
				CreaseEdges.Add(item);
			}
		}
		AllEdges = new HashSet<int>(CreaseEdges);
		foreach (int boundaryEdge in BoundaryEdges)
		{
			AllEdges.Add(boundaryEdge);
		}
		AllVertices = new HashSet<int>();
		IndexUtil.EdgesToVertices(Mesh, AllEdges, AllVertices);
	}

	private void extract_topology()
	{
		DGraph3 dGraph = new DGraph3();
		int[] array = new int[Mesh.MaxVertexID];
		int[] array2 = new int[AllVertices.Count];
		foreach (int allVertex in AllVertices)
		{
			array2[array[allVertex] = dGraph.AppendVertex(Mesh.GetVertex(allVertex))] = allVertex;
		}
		int[] array3 = new int[Mesh.MaxEdgeID];
		foreach (int allEdge in AllEdges)
		{
			Index2i edgeV = Mesh.GetEdgeV(allEdge);
			int v = array[edgeV.a];
			int v2 = array[edgeV.b];
			int num = dGraph.AppendEdge(v, v2, allEdge);
			array3[allEdge] = num;
		}
		DGraph3Util.Curves curves = DGraph3Util.ExtractCurves(dGraph, bWantLoopIndices: true);
		int count = curves.PathEdges.Count;
		Spans = new EdgeSpan[count];
		for (int i = 0; i < count; i++)
		{
			List<int> list = curves.PathEdges[i];
			for (int j = 0; j < list.Count; j++)
			{
				list[j] = dGraph.GetEdgeGroup(list[j]);
			}
			Spans[i] = EdgeSpan.FromEdges(Mesh, list);
		}
		int count2 = curves.LoopEdges.Count;
		Loops = new EdgeLoop[count2];
		for (int k = 0; k < count2; k++)
		{
			List<int> list2 = curves.LoopEdges[k];
			for (int l = 0; l < list2.Count; l++)
			{
				list2[l] = dGraph.GetEdgeGroup(list2[l]);
			}
			Loops[k] = EdgeLoop.FromEdges(Mesh, list2);
		}
		JunctionVertices = new HashSet<int>();
		foreach (int item in curves.JunctionV)
		{
			JunctionVertices.Add(array2[item]);
		}
	}

	public DMesh3 MakeElementsMesh(Polygon2d spanProfile, Polygon2d loopProfile)
	{
		DMesh3 dMesh = new DMesh3();
		validate_topology();
		EdgeSpan[] spans = Spans;
		for (int i = 0; i < spans.Length; i++)
		{
			TubeGenerator tubeGenerator = new TubeGenerator(spans[i].ToCurve(Mesh), spanProfile);
			MeshEditor.Append(dMesh, tubeGenerator.Generate().MakeDMesh());
		}
		EdgeLoop[] loops = Loops;
		for (int i = 0; i < loops.Length; i++)
		{
			TubeGenerator tubeGenerator2 = new TubeGenerator(loops[i].ToCurve(Mesh), loopProfile);
			MeshEditor.Append(dMesh, tubeGenerator2.Generate().MakeDMesh());
		}
		return dMesh;
	}
}
