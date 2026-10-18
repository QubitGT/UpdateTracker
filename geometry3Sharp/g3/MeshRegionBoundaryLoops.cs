using System.Collections;
using System.Collections.Generic;

namespace g3;

public class MeshRegionBoundaryLoops : IEnumerable<EdgeLoop>, IEnumerable
{
	public DMesh3 Mesh;

	public List<EdgeLoop> Loops;

	private IndexFlagSet triangles;

	private IndexFlagSet edges;

	public int Count => Loops.Count;

	public EdgeLoop this[int index] => Loops[index];

	public int MaxVerticesLoopIndex
	{
		get
		{
			int num = 0;
			for (int i = 1; i < Loops.Count; i++)
			{
				if (Loops[i].Vertices.Length > Loops[num].Vertices.Length)
				{
					num = i;
				}
			}
			return num;
		}
	}

	public MeshRegionBoundaryLoops(DMesh3 mesh, int[] RegionTris, bool bAutoCompute = true)
	{
		Mesh = mesh;
		triangles = new IndexFlagSet(mesh.MaxTriangleID, RegionTris.Length);
		for (int i = 0; i < RegionTris.Length; i++)
		{
			triangles[RegionTris[i]] = true;
		}
		edges = new IndexFlagSet(mesh.MaxEdgeID, RegionTris.Length);
		foreach (int tID in RegionTris)
		{
			Index3i triEdges = Mesh.GetTriEdges(tID);
			for (int k = 0; k < 3; k++)
			{
				int num = triEdges[k];
				if (!edges.Contains(num))
				{
					Index2i edgeT = mesh.GetEdgeT(num);
					if (edgeT.b == -1 || triangles[edgeT.a] != triangles[edgeT.b])
					{
						edges.Add(num);
					}
				}
			}
		}
		if (bAutoCompute)
		{
			Compute();
		}
	}

	public IEnumerator<EdgeLoop> GetEnumerator()
	{
		return Loops.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Loops.GetEnumerator();
	}

	private bool edge_is_boundary(int eid)
	{
		return edges.Contains(eid);
	}

	private bool edge_is_boundary(int eid, ref int tid_in, ref int tid_out)
	{
		if (!edges.Contains(eid))
		{
			return false;
		}
		tid_in = (tid_out = -1);
		Index2i edgeT = Mesh.GetEdgeT(eid);
		if (edgeT.b == -1)
		{
			tid_in = edgeT.a;
			tid_out = edgeT.b;
			return true;
		}
		bool flag = triangles[edgeT.a];
		bool flag2 = triangles[edgeT.b];
		if (flag != flag2)
		{
			tid_in = (flag ? edgeT.a : edgeT.b);
			tid_out = (flag ? edgeT.b : edgeT.a);
			return true;
		}
		return false;
	}

	private Index2i get_oriented_edgev(int eID, int tid_in, int tid_out)
	{
		Index2i edgeV = Mesh.GetEdgeV(eID);
		int a = edgeV.a;
		int b = edgeV.b;
		Index3i tri_verts = Mesh.GetTriangle(tid_in);
		int num = IndexUtil.find_edge_index_in_tri(a, b, ref tri_verts);
		return new Index2i(tri_verts[num], tri_verts[(num + 1) % 3]);
	}

	public int vertex_boundary_edges(int vID, ref int e0, ref int e1)
	{
		int num = 0;
		foreach (int item in Mesh.VtxEdgesItr(vID))
		{
			if (edge_is_boundary(item))
			{
				switch (num)
				{
				case 0:
					e0 = item;
					break;
				case 1:
					e1 = item;
					break;
				}
				num++;
			}
		}
		return num;
	}

	public int all_vertex_boundary_edges(int vID, int[] e)
	{
		int result = 0;
		foreach (int item in Mesh.VtxEdgesItr(vID))
		{
			if (edge_is_boundary(item))
			{
				e[result++] = item;
			}
		}
		return result;
	}

	public bool Compute()
	{
		Loops = new List<EdgeLoop>();
		IndexFlagSet indexFlagSet = new IndexFlagSet(Mesh.MaxEdgeID, edges.Count);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		int[] array = new int[16];
		foreach (int edge in edges)
		{
			if (indexFlagSet[edge] || !edge_is_boundary(edge))
			{
				continue;
			}
			int num = edge;
			indexFlagSet[num] = true;
			list.Add(num);
			int num2 = edge;
			bool flag = false;
			while (!flag)
			{
				int tid_in = -1;
				int tid_out = -1;
				edge_is_boundary(num2, ref tid_in, ref tid_out);
				Index2i index2i = get_oriented_edgev(num2, tid_in, tid_out);
				int a = index2i.a;
				int b = index2i.b;
				list2.Add(a);
				int e = -1;
				int e2 = 1;
				int num3 = vertex_boundary_edges(b, ref e, ref e2);
				if (num3 < 2)
				{
					throw new MeshBoundaryLoopsException("MeshRegionBoundaryLoops.Compute: found broken neighbourhood at vertex " + b)
					{
						UnclosedLoop = true
					};
				}
				int num4 = -1;
				if (num3 > 2)
				{
					if (b == list2[0])
					{
						num4 = -2;
					}
					else
					{
						if (num3 >= array.Length)
						{
							array = new int[num3];
						}
						int bdry_edges_count = all_vertex_boundary_edges(b, array);
						num4 = find_left_turn_edge(num2, b, array, bdry_edges_count, indexFlagSet);
						if (num4 == -1)
						{
							throw new MeshBoundaryLoopsException("MeshRegionBoundaryLoops.Compute: cannot find valid outgoing edge at bowtie vertex " + b)
							{
								BowtieFailure = true
							};
						}
					}
					if (!list3.Contains(b))
					{
						list3.Add(b);
					}
				}
				else
				{
					num4 = ((e == num2) ? e2 : e);
				}
				if (num4 == -2)
				{
					flag = true;
					continue;
				}
				if (num4 == num)
				{
					flag = true;
					continue;
				}
				list.Add(num4);
				num2 = num4;
				indexFlagSet[num2] = true;
			}
			if (list3.Count > 0)
			{
				List<EdgeLoop> list4 = extract_subloops(list2, list, list3);
				for (int i = 0; i < list4.Count; i++)
				{
					Loops.Add(list4[i]);
				}
			}
			else
			{
				EdgeLoop edgeLoop = new EdgeLoop(Mesh);
				edgeLoop.Vertices = list2.ToArray();
				edgeLoop.Edges = list.ToArray();
				Loops.Add(edgeLoop);
			}
			list.Clear();
			list2.Clear();
			list3.Clear();
		}
		return true;
	}

	private Vector3d get_vtx_normal(int vid)
	{
		Vector3d zero = Vector3d.Zero;
		foreach (int item in Mesh.VtxTrianglesItr(vid))
		{
			zero += Mesh.GetTriNormal(item);
		}
		zero.Normalize();
		return zero;
	}

	private int find_left_turn_edge(int incoming_e, int bowtie_v, int[] bdry_edges, int bdry_edges_count, IndexFlagSet used_edges)
	{
		Vector3d vector3d = get_vtx_normal(bowtie_v);
		int vID = Mesh.edge_other_v(incoming_e, bowtie_v);
		Vector3d vector3d2 = Mesh.GetVertex(bowtie_v) - Mesh.GetVertex(vID);
		int result = -1;
		double num = double.MaxValue;
		for (int i = 0; i < bdry_edges_count; i++)
		{
			int num2 = bdry_edges[i];
			if (used_edges[num2])
			{
				continue;
			}
			int tid_in = -1;
			int tid_out = -1;
			edge_is_boundary(num2, ref tid_in, ref tid_out);
			Index2i index2i = get_oriented_edgev(num2, tid_in, tid_out);
			if (index2i.a == bowtie_v)
			{
				Vector3d vector3d3 = Mesh.GetVertex(index2i.b) - Mesh.GetVertex(bowtie_v);
				float num3 = MathUtil.PlaneAngleSignedD((Vector3f)vector3d2, (Vector3f)vector3d3, (Vector3f)vector3d);
				if (num == double.MaxValue || (double)num3 < num)
				{
					num = num3;
					result = num2;
				}
			}
		}
		return result;
	}

	private List<EdgeLoop> extract_subloops(List<int> loopV, List<int> loopE, List<int> bowties)
	{
		List<EdgeLoop> list = new List<EdgeLoop>();
		List<int> list2 = new List<int>();
		foreach (int bowty in bowties)
		{
			if (count_in_list(loopV, bowty) > 1)
			{
				list2.Add(bowty);
			}
		}
		if (list2.Count == 0)
		{
			list.Add(new EdgeLoop(Mesh)
			{
				Vertices = loopV.ToArray(),
				Edges = loopE.ToArray(),
				BowtieVertices = bowties.ToArray()
			});
			return list;
		}
		while (list2.Count > 0)
		{
			int i = 0;
			int num = 0;
			int start_i = -1;
			int end_i = -1;
			int num2 = -1;
			int num3 = int.MaxValue;
			for (; i < list2.Count; i++)
			{
				num = list2[i];
				if (is_simple_bowtie_loop(loopV, list2, num, out start_i, out end_i))
				{
					int num4 = count_span(loopV, start_i, end_i);
					if (num4 < num3)
					{
						num2 = num;
						num3 = num4;
					}
				}
			}
			if (num2 == -1)
			{
				throw new MeshBoundaryLoopsException("MeshRegionBoundaryLoops.Compute: Cannot find a valid simple loop");
			}
			if (num != num2)
			{
				num = num2;
				is_simple_bowtie_loop(loopV, list2, num, out start_i, out end_i);
			}
			EdgeLoop edgeLoop = new EdgeLoop(Mesh);
			edgeLoop.Vertices = extract_span(loopV, start_i, end_i, bMarkInvalid: true);
			edgeLoop.Edges = EdgeLoop.VertexLoopToEdgeLoop(Mesh, edgeLoop.Vertices);
			edgeLoop.BowtieVertices = bowties.ToArray();
			list.Add(edgeLoop);
			if (count_in_list(loopV, num) < 2)
			{
				list2.Remove(num);
			}
		}
		int num5 = 0;
		for (int j = 0; j < loopV.Count; j++)
		{
			if (loopV[j] != -1)
			{
				num5++;
			}
		}
		if (num5 > 0)
		{
			EdgeLoop edgeLoop2 = new EdgeLoop(Mesh);
			edgeLoop2.Vertices = new int[num5];
			int num6 = 0;
			for (int k = 0; k < loopV.Count; k++)
			{
				if (loopV[k] != -1)
				{
					edgeLoop2.Vertices[num6++] = loopV[k];
				}
			}
			edgeLoop2.Edges = EdgeLoop.VertexLoopToEdgeLoop(Mesh, edgeLoop2.Vertices);
			edgeLoop2.BowtieVertices = bowties.ToArray();
			list.Add(edgeLoop2);
		}
		return list;
	}

	private bool is_simple_bowtie_loop(List<int> loopV, List<int> bowties, int bowtieV, out int start_i, out int end_i)
	{
		start_i = find_index(loopV, 0, bowtieV);
		end_i = find_index(loopV, start_i + 1, bowtieV);
		if (is_simple_path(loopV, bowties, bowtieV, start_i, end_i))
		{
			return true;
		}
		if (is_simple_path(loopV, bowties, bowtieV, end_i, start_i))
		{
			int num = start_i;
			start_i = end_i;
			end_i = num;
			return true;
		}
		return false;
	}

	private bool is_simple_path(List<int> loopV, List<int> bowties, int bowtieV, int i1, int i2)
	{
		int count = loopV.Count;
		for (int num = i1; num != i2; num = (num + 1) % count)
		{
			int num2 = loopV[num];
			if (num2 != -1 && num2 != bowtieV && bowties.Contains(num2))
			{
				return false;
			}
		}
		return true;
	}

	private int[] extract_span(List<int> loop, int i0, int i1, bool bMarkInvalid)
	{
		int[] array = new int[count_span(loop, i0, i1)];
		int num = 0;
		int count = loop.Count;
		for (int num2 = i0; num2 != i1; num2 = (num2 + 1) % count)
		{
			if (loop[num2] != -1)
			{
				array[num++] = loop[num2];
				if (bMarkInvalid)
				{
					loop[num2] = -1;
				}
			}
		}
		return array;
	}

	private int count_span(List<int> l, int i0, int i1)
	{
		int num = 0;
		int count = l.Count;
		for (int num2 = i0; num2 != i1; num2 = (num2 + 1) % count)
		{
			if (l[num2] != -1)
			{
				num++;
			}
		}
		return num;
	}

	private int find_index(List<int> loop, int start, int item)
	{
		for (int i = start; i < loop.Count; i++)
		{
			if (loop[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	private int count_in_list(List<int> loop, int item)
	{
		int num = 0;
		for (int i = 0; i < loop.Count; i++)
		{
			if (loop[i] == item)
			{
				num++;
			}
		}
		return num;
	}
}
