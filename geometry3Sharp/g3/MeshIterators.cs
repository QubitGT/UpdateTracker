using System;
using System.Collections.Generic;

namespace g3;

public static class MeshIterators
{
	public static IEnumerable<int> FilteredVertices(DMesh3 mesh, Func<DMesh3, int, bool> FilterF)
	{
		int N = mesh.MaxVertexID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsVertex(i) && FilterF(mesh, i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> FilteredEdges(DMesh3 mesh, Func<DMesh3, int, bool> FilterF)
	{
		int N = mesh.MaxEdgeID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsEdge(i) && FilterF(mesh, i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> FilteredTriangles(DMesh3 mesh, Func<DMesh3, int, bool> FilterF)
	{
		int N = mesh.MaxTriangleID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsTriangle(i) && FilterF(mesh, i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> BoundaryVertices(DMesh3 mesh)
	{
		int N = mesh.MaxVertexID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsVertex(i) && mesh.IsBoundaryVertex(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> BoundaryEdgeVertices(DMesh3 mesh)
	{
		int N = mesh.MaxEdgeID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsEdge(i) && mesh.IsBoundaryEdge(i))
			{
				Index2i ev = mesh.GetEdgeV(i);
				yield return ev.a;
				yield return ev.b;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> InteriorVertices(DMesh3 mesh)
	{
		int N = mesh.MaxVertexID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsVertex(i) && !mesh.IsBoundaryVertex(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> GroupBoundaryVertices(DMesh3 mesh)
	{
		int N = mesh.MaxVertexID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsVertex(i) && mesh.IsGroupBoundaryVertex(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> GroupJunctionVertices(DMesh3 mesh)
	{
		int N = mesh.MaxVertexID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsVertex(i) && mesh.IsGroupJunctionVertex(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> BoundaryEdges(DMesh3 mesh)
	{
		int N = mesh.MaxEdgeID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsEdge(i) && mesh.IsBoundaryEdge(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> InteriorEdges(DMesh3 mesh)
	{
		int N = mesh.MaxEdgeID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsEdge(i) && !mesh.IsBoundaryEdge(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> GroupBoundaryEdges(DMesh3 mesh)
	{
		int N = mesh.MaxEdgeID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsEdge(i) && mesh.IsGroupBoundaryEdge(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<int> BowtieVertices(DMesh3 mesh)
	{
		int N = mesh.MaxVertexID;
		int i = 0;
		while (i < N)
		{
			if (mesh.IsVertex(i) && mesh.IsBowtieVertex(i))
			{
				yield return i;
			}
			int num = i + 1;
			i = num;
		}
	}
}
