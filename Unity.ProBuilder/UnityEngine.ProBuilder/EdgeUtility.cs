using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

internal static class EdgeUtility
{
	public static IEnumerable<Edge> GetSharedVertexHandleEdges(this ProBuilderMesh mesh, IEnumerable<Edge> edges)
	{
		return edges.Select((Edge x) => mesh.GetSharedVertexHandleEdge(x));
	}

	public static Edge GetSharedVertexHandleEdge(this ProBuilderMesh mesh, Edge edge)
	{
		return new Edge(mesh.sharedVertexLookup[edge.a], mesh.sharedVertexLookup[edge.b]);
	}

	internal static Edge GetEdgeWithSharedVertexHandles(this ProBuilderMesh mesh, Edge edge)
	{
		return new Edge(mesh.sharedVerticesInternal[edge.a][0], mesh.sharedVerticesInternal[edge.b][0]);
	}

	public static bool ValidateEdge(ProBuilderMesh mesh, Edge edge, out SimpleTuple<Face, Edge> validEdge)
	{
		Face[] facesInternal = mesh.facesInternal;
		SharedVertex[] sharedVerticesInternal = mesh.sharedVerticesInternal;
		Edge sharedVertexHandleEdge = mesh.GetSharedVertexHandleEdge(edge);
		for (int i = 0; i < facesInternal.Length; i++)
		{
			int index_a = -1;
			int index_a2 = -1;
			int index_b = -1;
			int index_b2 = -1;
			if (facesInternal[i].distinctIndexesInternal.ContainsMatch(sharedVerticesInternal[sharedVertexHandleEdge.a].arrayInternal, out index_a, out index_b) && facesInternal[i].distinctIndexesInternal.ContainsMatch(sharedVerticesInternal[sharedVertexHandleEdge.b].arrayInternal, out index_a2, out index_b2))
			{
				int a = facesInternal[i].distinctIndexesInternal[index_a];
				int b = facesInternal[i].distinctIndexesInternal[index_a2];
				validEdge = new SimpleTuple<Face, Edge>(facesInternal[i], new Edge(a, b));
				return true;
			}
		}
		validEdge = default(SimpleTuple<Face, Edge>);
		return false;
	}

	internal static bool Contains(this Edge[] edges, Edge edge)
	{
		for (int i = 0; i < edges.Length; i++)
		{
			if (edges[i].Equals(edge))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool Contains(this Edge[] edges, int x, int y)
	{
		for (int i = 0; i < edges.Length; i++)
		{
			if ((x == edges[i].a && y == edges[i].b) || (x == edges[i].b && y == edges[i].a))
			{
				return true;
			}
		}
		return false;
	}

	internal static int IndexOf(this ProBuilderMesh mesh, IList<Edge> edges, Edge edge)
	{
		for (int i = 0; i < edges.Count; i++)
		{
			if (edges[i].Equals(edge, mesh.sharedVertexLookup))
			{
				return i;
			}
		}
		return -1;
	}

	internal static int[] AllTriangles(this Edge[] edges)
	{
		int[] array = new int[edges.Length * 2];
		int num = 0;
		for (int i = 0; i < edges.Length; i++)
		{
			array[num++] = edges[i].a;
			array[num++] = edges[i].b;
		}
		return array;
	}

	internal static Face GetFace(this ProBuilderMesh mesh, Edge edge)
	{
		Face result = null;
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			Edge[] edgesInternal = face.edgesInternal;
			int j = 0;
			for (int num = edgesInternal.Length; j < num; j++)
			{
				if (edge.Equals(edgesInternal[j]))
				{
					return face;
				}
				if (edgesInternal.Contains(edgesInternal[j]))
				{
					result = face;
				}
			}
		}
		return result;
	}
}
