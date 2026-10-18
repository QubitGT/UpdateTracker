using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class MeshBoundaryLoops : IEnumerable<EdgeLoop>, IEnumerable
{
	public enum SpanBehaviors
	{
		Ignore,
		ThrowException,
		Compute
	}

	public enum FailureBehaviors
	{
		ThrowException,
		ConvertToOpenSpan
	}

	private struct Subloops
	{
		public List<EdgeLoop> Loops;

		public List<EdgeSpan> Spans;
	}

	public DMesh3 Mesh;

	public List<EdgeLoop> Loops;

	public List<EdgeSpan> Spans;

	public bool SawOpenSpans;

	public bool FellBackToSpansOnFailure;

	public SpanBehaviors SpanBehavior = SpanBehaviors.Compute;

	public FailureBehaviors FailureBehavior = FailureBehaviors.ConvertToOpenSpan;

	public Func<int, bool> EdgeFilterF;

	public List<int> FailureBowties;

	public int Count => Loops.Count;

	public int SpanCount => Spans.Count;

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

	public MeshBoundaryLoops(DMesh3 mesh, bool bAutoCompute = true)
	{
		Mesh = mesh;
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

	public Index2i FindVertexIndex(int vID)
	{
		int count = Loops.Count;
		for (int i = 0; i < count; i++)
		{
			int num = Loops[i].FindVertexIndex(vID);
			if (num >= 0)
			{
				return new Index2i(i, num);
			}
		}
		return Index2i.Max;
	}

	public int FindLoopContainingVertex(int vid)
	{
		int count = Loops.Count;
		for (int i = 0; i < count; i++)
		{
			if (Loops[i].Vertices.Contains(vid))
			{
				return i;
			}
		}
		return -1;
	}

	public int FindLoopContainingEdge(int eid)
	{
		int count = Loops.Count;
		for (int i = 0; i < count; i++)
		{
			if (Loops[i].Edges.Contains(eid))
			{
				return i;
			}
		}
		return -1;
	}

	public bool Compute()
	{
		Loops = new List<EdgeLoop>();
		Spans = new List<EdgeSpan>();
		if (Mesh.CachedIsClosed)
		{
			return true;
		}
		int maxEdgeID = Mesh.MaxEdgeID;
		BitArray bitArray = new BitArray(maxEdgeID);
		bitArray.SetAll(value: false);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		int[] array = new int[16];
		for (int i = 0; i < maxEdgeID; i++)
		{
			if (!Mesh.IsEdge(i) || bitArray[i] || !Mesh.IsBoundaryEdge(i))
			{
				continue;
			}
			if (EdgeFilterF != null && !EdgeFilterF(i))
			{
				bitArray[i] = true;
				continue;
			}
			int num = i;
			bitArray[num] = true;
			list.Add(num);
			int num2 = i;
			bool flag = false;
			bool flag2 = false;
			while (!flag)
			{
				Index2i orientedBoundaryEdgeV = Mesh.GetOrientedBoundaryEdgeV(num2);
				int a = orientedBoundaryEdgeV.a;
				int num3 = orientedBoundaryEdgeV.b;
				if (flag2)
				{
					a = orientedBoundaryEdgeV.b;
					num3 = orientedBoundaryEdgeV.a;
				}
				else
				{
					list2.Add(a);
				}
				int e = -1;
				int e2 = 1;
				int num4 = Mesh.VtxBoundaryEdges(num3, ref e, ref e2);
				if (EdgeFilterF != null)
				{
					if (num4 > 2)
					{
						if (num4 >= array.Length)
						{
							array = new int[num4];
						}
						int max_i = Mesh.VtxAllBoundaryEdges(num3, array);
						max_i = BufferUtil.CountValid(array, EdgeFilterF, max_i);
					}
					else
					{
						if (!EdgeFilterF(e))
						{
							num4--;
						}
						if (!EdgeFilterF(e2))
						{
							num4--;
						}
					}
				}
				if (num4 < 2)
				{
					if (SpanBehavior == SpanBehaviors.ThrowException)
					{
						throw new MeshBoundaryLoopsException("MeshBoundaryLoops.Compute: found open span at vertex " + num3)
						{
							UnclosedLoop = true
						};
					}
					if (flag2)
					{
						flag = true;
						continue;
					}
					flag2 = true;
					num2 = list[0];
					list.Reverse();
					continue;
				}
				int num5 = -1;
				if (num4 > 2)
				{
					if (num3 == list2[0])
					{
						num5 = -2;
					}
					else
					{
						if (num4 >= array.Length)
						{
							array = new int[2 * num4];
						}
						int num6 = Mesh.VtxAllBoundaryEdges(num3, array);
						if (EdgeFilterF != null)
						{
							num6 = BufferUtil.FilterInPlace(array, EdgeFilterF, num6);
						}
						num5 = find_left_turn_edge(num2, num3, array, num6, bitArray);
						if (num5 == -1)
						{
							if (FailureBehavior == FailureBehaviors.ThrowException || SpanBehavior == SpanBehaviors.ThrowException)
							{
								throw new MeshBoundaryLoopsException("MeshBoundaryLoops.Compute: cannot find valid outgoing edge at bowtie vertex " + num3)
								{
									BowtieFailure = true
								};
							}
							if (flag2)
							{
								flag = true;
								continue;
							}
							flag2 = true;
							flag = true;
							continue;
						}
					}
					if (!list3.Contains(num3))
					{
						list3.Add(num3);
					}
				}
				else
				{
					num5 = ((e == num2) ? e2 : e);
				}
				if (num5 == -2)
				{
					flag = true;
				}
				else if (num5 == num)
				{
					flag = true;
				}
				else if (bitArray[num5])
				{
					if (FailureBehavior == FailureBehaviors.ThrowException || SpanBehavior == SpanBehaviors.ThrowException)
					{
						throw new MeshBoundaryLoopsException("MeshBoundaryLoops.Compute: encountered repeated edge " + num5)
						{
							RepeatedEdge = true
						};
					}
					flag2 = true;
					flag = true;
				}
				else
				{
					list.Add(num5);
					bitArray[num5] = true;
					num2 = num5;
				}
			}
			if (flag2)
			{
				SawOpenSpans = true;
				if (SpanBehavior == SpanBehaviors.Compute)
				{
					list.Reverse();
					EdgeSpan item = EdgeSpan.FromEdges(Mesh, list);
					Spans.Add(item);
				}
			}
			else if (list3.Count > 0)
			{
				Subloops subloops = extract_subloops(list2, list, list3);
				foreach (EdgeLoop loop in subloops.Loops)
				{
					Loops.Add(loop);
				}
				if (subloops.Spans.Count > 0)
				{
					FellBackToSpansOnFailure = true;
					foreach (EdgeSpan span in subloops.Spans)
					{
						Spans.Add(span);
					}
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

	private int find_left_turn_edge(int incoming_e, int bowtie_v, int[] bdry_edges, int bdry_edges_count, BitArray used_edges)
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
			Index2i orientedBoundaryEdgeV = Mesh.GetOrientedBoundaryEdgeV(num2);
			if (orientedBoundaryEdgeV.a == bowtie_v)
			{
				Vector3d vector3d3 = Mesh.GetVertex(orientedBoundaryEdgeV.b) - Mesh.GetVertex(bowtie_v);
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

	private Subloops extract_subloops(List<int> loopV, List<int> loopE, List<int> bowties)
	{
		Subloops result = default(Subloops);
		result.Loops = new List<EdgeLoop>();
		result.Spans = new List<EdgeSpan>();
		List<int> list = new List<int>();
		foreach (int bowty in bowties)
		{
			if (count_in_list(loopV, bowty) > 1)
			{
				list.Add(bowty);
			}
		}
		if (list.Count == 0)
		{
			result.Loops.Add(new EdgeLoop(Mesh)
			{
				Vertices = loopV.ToArray(),
				Edges = loopE.ToArray(),
				BowtieVertices = bowties.ToArray()
			});
			return result;
		}
		while (list.Count > 0)
		{
			int i = 0;
			int num = 0;
			int start_i = -1;
			int end_i = -1;
			int num2 = -1;
			int num3 = int.MaxValue;
			for (; i < list.Count; i++)
			{
				num = list[i];
				if (is_simple_bowtie_loop(loopV, list, num, out start_i, out end_i))
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
				if (FailureBehavior == FailureBehaviors.ThrowException)
				{
					FailureBowties = list;
					throw new MeshBoundaryLoopsException("MeshBoundaryLoops.Compute: Cannot find a valid simple loop");
				}
				EdgeSpan edgeSpan = new EdgeSpan(Mesh);
				List<int> list2 = new List<int>();
				for (int j = 0; j < loopV.Count; j++)
				{
					if (loopV[j] != -1)
					{
						list2.Add(loopV[j]);
					}
				}
				edgeSpan.Vertices = list2.ToArray();
				edgeSpan.Edges = EdgeSpan.VerticesToEdges(Mesh, edgeSpan.Vertices);
				edgeSpan.BowtieVertices = bowties.ToArray();
				result.Spans.Add(edgeSpan);
				return result;
			}
			if (num != num2)
			{
				num = num2;
				is_simple_bowtie_loop(loopV, list, num, out start_i, out end_i);
			}
			EdgeLoop edgeLoop = new EdgeLoop(Mesh);
			edgeLoop.Vertices = extract_span(loopV, start_i, end_i, bMarkInvalid: true);
			edgeLoop.Edges = EdgeLoop.VertexLoopToEdgeLoop(Mesh, edgeLoop.Vertices);
			edgeLoop.BowtieVertices = bowties.ToArray();
			result.Loops.Add(edgeLoop);
			if (count_in_list(loopV, num) < 2)
			{
				list.Remove(num);
			}
		}
		int num5 = 0;
		for (int k = 0; k < loopV.Count; k++)
		{
			if (loopV[k] != -1)
			{
				num5++;
			}
		}
		if (num5 > 0)
		{
			EdgeLoop edgeLoop2 = new EdgeLoop(Mesh);
			edgeLoop2.Vertices = new int[num5];
			int num6 = 0;
			for (int l = 0; l < loopV.Count; l++)
			{
				if (loopV[l] != -1)
				{
					edgeLoop2.Vertices[num6++] = loopV[l];
				}
			}
			edgeLoop2.Edges = EdgeLoop.VertexLoopToEdgeLoop(Mesh, edgeLoop2.Vertices);
			edgeLoop2.BowtieVertices = bowties.ToArray();
			result.Loops.Add(edgeLoop2);
		}
		return result;
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
