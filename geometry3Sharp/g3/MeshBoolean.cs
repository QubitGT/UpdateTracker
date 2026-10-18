using System;
using System.Collections.Generic;

namespace g3;

public class MeshBoolean
{
	public DMesh3 Target;

	public DMesh3 Tool;

	public double VertexSnapTol = 1E-05;

	public DMesh3 Result;

	private MeshMeshCut cutTargetOp;

	private MeshMeshCut cutToolOp;

	private DMesh3 cutTargetMesh;

	private DMesh3 cutToolMesh;

	public bool Compute()
	{
		cutTargetOp = new MeshMeshCut
		{
			Target = new DMesh3(Target),
			CutMesh = Tool,
			VertexSnapTol = VertexSnapTol
		};
		cutTargetOp.Compute();
		cutTargetOp.RemoveContained();
		cutTargetMesh = cutTargetOp.Target;
		cutToolOp = new MeshMeshCut
		{
			Target = new DMesh3(Tool),
			CutMesh = Target,
			VertexSnapTol = VertexSnapTol
		};
		cutToolOp.Compute();
		cutToolOp.RemoveContained();
		cutToolMesh = cutToolOp.Target;
		resolve_vtx_pairs();
		Result = cutToolMesh;
		MeshEditor.Append(Result, cutTargetMesh);
		return true;
	}

	private void resolve_vtx_pairs()
	{
		HashSet<int> hashSet = new HashSet<int>(MeshIterators.BoundaryVertices(cutTargetMesh));
		HashSet<int> hashSet2 = new HashSet<int>(MeshIterators.BoundaryVertices(cutToolMesh));
		split_missing(cutTargetOp, cutToolOp, cutTargetMesh, cutToolMesh, hashSet, hashSet2);
		split_missing(cutToolOp, cutTargetOp, cutToolMesh, cutTargetMesh, hashSet2, hashSet);
	}

	private void split_missing(MeshMeshCut fromOp, MeshMeshCut toOp, DMesh3 fromMesh, DMesh3 toMesh, HashSet<int> fromVerts, HashSet<int> toVerts)
	{
		List<int> list = new List<int>();
		foreach (int fromVert in fromVerts)
		{
			Vector3d vertex = fromMesh.GetVertex(fromVert);
			if (find_nearest_vertex(toMesh, vertex, toVerts) == -1)
			{
				list.Add(fromVert);
			}
		}
		foreach (int item in list)
		{
			Vector3d vertex2 = fromMesh.GetVertex(item);
			int num = find_nearest_edge(toMesh, vertex2, toVerts);
			if (num == -1)
			{
				Console.WriteLine("could not find edge to split?");
				continue;
			}
			if (toMesh.SplitEdge(num, out var split) != 0)
			{
				Console.WriteLine("edge split failed");
				continue;
			}
			toMesh.SetVertex(split.vNew, vertex2);
			toVerts.Add(split.vNew);
		}
	}

	private int find_nearest_vertex(DMesh3 mesh, Vector3d v, HashSet<int> vertices)
	{
		int result = -1;
		double num = VertexSnapTol * VertexSnapTol;
		foreach (int vertex in vertices)
		{
			double num2 = mesh.GetVertex(vertex).DistanceSquared(ref v);
			if (num2 < num)
			{
				result = vertex;
				num = num2;
			}
		}
		return result;
	}

	private int find_nearest_edge(DMesh3 mesh, Vector3d v, HashSet<int> vertices)
	{
		int result = -1;
		double num = VertexSnapTol * VertexSnapTol;
		foreach (int item in mesh.BoundaryEdgeIndices())
		{
			Index2i edgeV = mesh.GetEdgeV(item);
			if (vertices.Contains(edgeV.a) && vertices.Contains(edgeV.b))
			{
				double num2 = new Segment3d(mesh.GetVertex(edgeV.a), mesh.GetVertex(edgeV.b)).DistanceSquared(v);
				if (num2 < num)
				{
					result = item;
					num = num2;
				}
			}
		}
		return result;
	}
}
