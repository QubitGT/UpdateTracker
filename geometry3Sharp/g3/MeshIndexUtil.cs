using System.Collections.Generic;

namespace g3;

public static class MeshIndexUtil
{
	public static List<int> MapEdgesViaVertexMap(IIndexMap AtoBV, DMesh3 MeshA, DMesh3 MeshB, List<int> edges)
	{
		int count = edges.Count;
		List<int> list = new List<int>(count);
		for (int i = 0; i < count; i++)
		{
			int eID = edges[i];
			Index2i edgeV = MeshA.GetEdgeV(eID);
			int vA = AtoBV[edgeV.a];
			int vB = AtoBV[edgeV.b];
			int item = MeshB.FindEdge(vA, vB);
			list.Add(item);
		}
		return list;
	}

	public static EdgeLoop MapLoopViaVertexMap(IIndexMap AtoBV, DMesh3 MeshA, DMesh3 MeshB, EdgeLoop loopIn)
	{
		int vertexCount = loopIn.VertexCount;
		int edgeCount = loopIn.EdgeCount;
		int[] array = new int[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			array[i] = AtoBV[loopIn.Vertices[i]];
		}
		int[] array2 = new int[edgeCount];
		for (int j = 0; j < edgeCount; j++)
		{
			int eID = loopIn.Edges[j];
			Index2i edgeV = MeshA.GetEdgeV(eID);
			int vA = AtoBV[edgeV.a];
			int vB = AtoBV[edgeV.b];
			array2[j] = MeshB.FindEdge(vA, vB);
		}
		return new EdgeLoop(MeshB, array, array2, bCopyArrays: false);
	}
}
