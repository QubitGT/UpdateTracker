using System;
using System.Collections.Generic;

namespace g3;

public class IndexUtil
{
	public static bool same_pair_unordered(int a0, int a1, int b0, int b1)
	{
		if (a0 != b0)
		{
			if (a0 == b1)
			{
				return a1 == b0;
			}
			return false;
		}
		return a1 == b1;
	}

	public static int find_shared_edge_v(ref Index2i ev0, ref Index2i ev1)
	{
		if (ev0.a == ev1.a)
		{
			return ev0.a;
		}
		if (ev0.a == ev1.b)
		{
			return ev0.a;
		}
		if (ev0.b == ev1.a)
		{
			return ev0.b;
		}
		if (ev0.b == ev1.b)
		{
			return ev0.b;
		}
		return -1;
	}

	public static int find_edge_other_v(ref Index2i ev, int v)
	{
		if (ev.a == v)
		{
			return ev.b;
		}
		if (ev.b == v)
		{
			return ev.a;
		}
		return -1;
	}

	public static int find_edge_other_v(Index2i ev, int v)
	{
		if (ev.a == v)
		{
			return ev.b;
		}
		if (ev.b == v)
		{
			return ev.a;
		}
		return -1;
	}

	public static int find_tri_index(int a, int[] tri_verts)
	{
		if (tri_verts[0] == a)
		{
			return 0;
		}
		if (tri_verts[1] == a)
		{
			return 1;
		}
		if (tri_verts[2] == a)
		{
			return 2;
		}
		return -1;
	}

	public static int find_tri_index(int a, Index3i tri_verts)
	{
		if (tri_verts.a == a)
		{
			return 0;
		}
		if (tri_verts.b == a)
		{
			return 1;
		}
		if (tri_verts.c == a)
		{
			return 2;
		}
		return -1;
	}

	public static int find_tri_index(int a, ref Index3i tri_verts)
	{
		if (tri_verts.a == a)
		{
			return 0;
		}
		if (tri_verts.b == a)
		{
			return 1;
		}
		if (tri_verts.c == a)
		{
			return 2;
		}
		return -1;
	}

	public static int find_edge_index_in_tri(int a, int b, int[] tri_verts)
	{
		if (same_pair_unordered(a, b, tri_verts[0], tri_verts[1]))
		{
			return 0;
		}
		if (same_pair_unordered(a, b, tri_verts[1], tri_verts[2]))
		{
			return 1;
		}
		if (same_pair_unordered(a, b, tri_verts[2], tri_verts[0]))
		{
			return 2;
		}
		return -1;
	}

	public static int find_edge_index_in_tri(int a, int b, ref Index3i tri_verts)
	{
		if (same_pair_unordered(a, b, tri_verts.a, tri_verts.b))
		{
			return 0;
		}
		if (same_pair_unordered(a, b, tri_verts.b, tri_verts.c))
		{
			return 1;
		}
		if (same_pair_unordered(a, b, tri_verts.c, tri_verts.a))
		{
			return 2;
		}
		return -1;
	}

	public static int find_tri_ordered_edge(int a, int b, int[] tri_verts)
	{
		if (tri_verts[0] == a && tri_verts[1] == b)
		{
			return 0;
		}
		if (tri_verts[1] == a && tri_verts[2] == b)
		{
			return 1;
		}
		if (tri_verts[2] == a && tri_verts[0] == b)
		{
			return 2;
		}
		return -1;
	}

	public static int find_tri_ordered_edge(int a, int b, ref Index3i tri_verts)
	{
		if (tri_verts.a == a && tri_verts.b == b)
		{
			return 0;
		}
		if (tri_verts.b == a && tri_verts.c == b)
		{
			return 1;
		}
		if (tri_verts.c == a && tri_verts.a == b)
		{
			return 2;
		}
		return -1;
	}

	public static int find_tri_ordered_edge(int a, int b, Index3i tri_verts)
	{
		return find_tri_ordered_edge(a, b, ref tri_verts);
	}

	public static int find_tri_other_vtx(int a, int b, int[] tri_verts)
	{
		for (int i = 0; i < 3; i++)
		{
			if (same_pair_unordered(a, b, tri_verts[i], tri_verts[(i + 1) % 3]))
			{
				return tri_verts[(i + 2) % 3];
			}
		}
		return -1;
	}

	public static int find_tri_other_vtx(int a, int b, Index3i tri_verts)
	{
		for (int i = 0; i < 3; i++)
		{
			if (same_pair_unordered(a, b, tri_verts[i], tri_verts[(i + 1) % 3]))
			{
				return tri_verts[(i + 2) % 3];
			}
		}
		return -1;
	}

	public static int find_tri_other_vtx(int a, int b, DVector<int> tri_array, int ti)
	{
		int num = 3 * ti;
		for (int i = 0; i < 3; i++)
		{
			if (same_pair_unordered(a, b, tri_array[num + i], tri_array[num + (i + 1) % 3]))
			{
				return tri_array[num + (i + 2) % 3];
			}
		}
		return -1;
	}

	public static Index2i find_tri_other_verts(int a, ref Index3i tri_verts)
	{
		if (tri_verts.a == a)
		{
			return new Index2i(tri_verts.b, tri_verts.c);
		}
		if (tri_verts.b == a)
		{
			return new Index2i(tri_verts.c, tri_verts.a);
		}
		if (tri_verts.c == a)
		{
			return new Index2i(tri_verts.a, tri_verts.b);
		}
		return Index2i.Max;
	}

	public static int find_tri_other_index(int a, int b, int[] tri_verts)
	{
		for (int i = 0; i < 3; i++)
		{
			if (same_pair_unordered(a, b, tri_verts[i], tri_verts[(i + 1) % 3]))
			{
				return (i + 2) % 3;
			}
		}
		return -1;
	}

	public static bool orient_tri_edge(ref int a, ref int b, ref Index3i tri_verts)
	{
		if (a == tri_verts.a)
		{
			if (tri_verts.c == b)
			{
				int num = a;
				a = b;
				b = num;
				return true;
			}
		}
		else if (a == tri_verts.b)
		{
			if (tri_verts.a == b)
			{
				int num2 = a;
				a = b;
				b = num2;
				return true;
			}
		}
		else if (a == tri_verts.c && tri_verts.b == b)
		{
			int num3 = a;
			a = b;
			b = num3;
			return true;
		}
		return false;
	}

	public static bool orient_tri_edge(ref int a, ref int b, Index3i tri_verts)
	{
		return orient_tri_edge(ref a, ref b, ref tri_verts);
	}

	public static int orient_tri_edge_and_find_other_vtx(ref int a, ref int b, int[] tri_verts)
	{
		for (int i = 0; i < 3; i++)
		{
			if (same_pair_unordered(a, b, tri_verts[i], tri_verts[(i + 1) % 3]))
			{
				a = tri_verts[i];
				b = tri_verts[(i + 1) % 3];
				return tri_verts[(i + 2) % 3];
			}
		}
		return -1;
	}

	public static int orient_tri_edge_and_find_other_vtx(ref int a, ref int b, Index3i tri_verts)
	{
		for (int i = 0; i < 3; i++)
		{
			if (same_pair_unordered(a, b, tri_verts[i], tri_verts[(i + 1) % 3]))
			{
				a = tri_verts[i];
				b = tri_verts[(i + 1) % 3];
				return tri_verts[(i + 2) % 3];
			}
		}
		return -1;
	}

	public static bool is_ordered(int a, int b, ref Index3i tri_verts)
	{
		if ((tri_verts.a != a || tri_verts.b != b) && (tri_verts.b != a || tri_verts.c != b))
		{
			if (tri_verts.c == a)
			{
				return tri_verts.a == b;
			}
			return false;
		}
		return true;
	}

	public static bool is_same_triangle(int a, int b, int c, ref Index3i tri)
	{
		if (tri.a == a)
		{
			return same_pair_unordered(tri.b, tri.c, b, c);
		}
		if (tri.b == a)
		{
			return same_pair_unordered(tri.a, tri.c, b, c);
		}
		if (tri.c == a)
		{
			return same_pair_unordered(tri.a, tri.b, b, c);
		}
		return false;
	}

	public static void cycle_indices_minfirst(ref Index3i tri)
	{
		if (tri.b < tri.a && tri.b < tri.c)
		{
			int a = tri.a;
			int b = tri.b;
			int c = tri.c;
			tri.a = b;
			tri.b = c;
			tri.c = a;
		}
		else if (tri.c < tri.a && tri.c < tri.b)
		{
			int a2 = tri.a;
			int b2 = tri.b;
			int c2 = tri.c;
			tri.a = c2;
			tri.b = a2;
			tri.c = b2;
		}
	}

	public static void sort_indices(ref Index3i tri)
	{
		if (tri.a < tri.b && tri.a < tri.c)
		{
			if (tri.b > tri.c)
			{
				int b = tri.b;
				tri.b = tri.c;
				tri.c = b;
			}
		}
		else if (tri.b < tri.a && tri.b < tri.c)
		{
			if (tri.a < tri.c)
			{
				int b2 = tri.b;
				tri.b = tri.a;
				tri.a = b2;
				return;
			}
			int a = tri.a;
			int b3 = tri.b;
			int c = tri.c;
			tri.a = b3;
			tri.b = c;
			tri.c = a;
		}
		else if (tri.c < tri.a && tri.c < tri.b)
		{
			if (tri.b < tri.a)
			{
				int c2 = tri.c;
				tri.c = tri.a;
				tri.a = c2;
				return;
			}
			int a2 = tri.a;
			int b4 = tri.b;
			int c3 = tri.c;
			tri.a = c3;
			tri.b = a2;
			tri.c = b4;
		}
	}

	public static Vector3i ToGrid3Index(int idx, int nx, int ny)
	{
		int x = idx % nx;
		int y = idx / nx % ny;
		int z = idx / (nx * ny);
		return new Vector3i(x, y, z);
	}

	public static int ToGrid3Linear(int i, int j, int k, int nx, int ny)
	{
		return i + nx * (j + ny * k);
	}

	public static int ToGrid3Linear(Vector3i ijk, int nx, int ny)
	{
		return ijk.x + nx * (ijk.y + ny * ijk.z);
	}

	public static int ToGrid3Linear(ref Vector3i ijk, int nx, int ny)
	{
		return ijk.x + nx * (ijk.y + ny * ijk.z);
	}

	public static int[] FilterValid(int[] indices, Func<int, bool> FilterF, bool bForceCopy = false)
	{
		int num = 0;
		for (int i = 0; i < indices.Length; i++)
		{
			if (FilterF(indices[i]))
			{
				num++;
			}
		}
		if (num == indices.Length && !bForceCopy)
		{
			return indices;
		}
		int[] array = new int[num];
		int num2 = 0;
		for (int j = 0; j < indices.Length; j++)
		{
			if (FilterF(indices[j]))
			{
				array[num2++] = indices[j];
			}
		}
		return array;
	}

	public static bool IndicesCheck(int[] indices, Func<int, bool> CheckF)
	{
		for (int i = 0; i < indices.Length; i++)
		{
			if (!CheckF(indices[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static void Apply(List<int> indices, IIndexMap map)
	{
		int count = indices.Count;
		for (int i = 0; i < count; i++)
		{
			indices[i] = map[indices[i]];
		}
	}

	public static void Apply(int[] indices, IIndexMap map)
	{
		int num = indices.Length;
		for (int i = 0; i < num; i++)
		{
			indices[i] = map[indices[i]];
		}
	}

	public static void Apply(int[] indices, IList<int> map)
	{
		int num = indices.Length;
		for (int i = 0; i < num; i++)
		{
			indices[i] = map[indices[i]];
		}
	}

	public static void TrianglesToVertices(DMesh3 mesh, IEnumerable<int> triangles, HashSet<int> vertices)
	{
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = mesh.GetTriangle(triangle2);
			vertices.Add(triangle.a);
			vertices.Add(triangle.b);
			vertices.Add(triangle.c);
		}
	}

	public static void TrianglesToVertices(DMesh3 mesh, HashSet<int> triangles, HashSet<int> vertices)
	{
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = mesh.GetTriangle(triangle2);
			vertices.Add(triangle.a);
			vertices.Add(triangle.b);
			vertices.Add(triangle.c);
		}
	}

	public static void TrianglesToEdges(DMesh3 mesh, IEnumerable<int> triangles, HashSet<int> edges)
	{
		foreach (int triangle in triangles)
		{
			Index3i triEdges = mesh.GetTriEdges(triangle);
			edges.Add(triEdges.a);
			edges.Add(triEdges.b);
			edges.Add(triEdges.c);
		}
	}

	public static void TrianglesToEdges(DMesh3 mesh, HashSet<int> triangles, HashSet<int> edges)
	{
		foreach (int triangle in triangles)
		{
			Index3i triEdges = mesh.GetTriEdges(triangle);
			edges.Add(triEdges.a);
			edges.Add(triEdges.b);
			edges.Add(triEdges.c);
		}
	}

	public static void EdgesToVertices(DMesh3 mesh, IEnumerable<int> edges, HashSet<int> vertices)
	{
		foreach (int edge in edges)
		{
			Index2i edgeV = mesh.GetEdgeV(edge);
			vertices.Add(edgeV.a);
			vertices.Add(edgeV.b);
		}
	}

	public static void EdgesToVertices(DMesh3 mesh, HashSet<int> edges, HashSet<int> vertices)
	{
		foreach (int edge in edges)
		{
			Index2i edgeV = mesh.GetEdgeV(edge);
			vertices.Add(edgeV.a);
			vertices.Add(edgeV.b);
		}
	}
}
