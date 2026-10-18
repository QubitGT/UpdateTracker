using System;
using System.Collections.Generic;

namespace g3;

public class GraphSplitter2d
{
	private struct edge_hit
	{
		public int hit_eid;

		public Index2i vtx_signs;

		public int hit_vid;

		public Vector2d hit_pos;

		public double line_t;
	}

	public DGraph2 Graph;

	public double OnVertexTol = 1.1920928955078125E-07;

	public int InsertedEdgesID = 1;

	public Func<Vector2d, bool> InsideTestF;

	private DVector<int> EdgeSigns = new DVector<int>();

	private List<edge_hit> hits = new List<edge_hit>();

	public GraphSplitter2d(DGraph2 graph)
	{
		Graph = graph;
	}

	public void InsertLine(Line2d line, int insert_edges_id = -1)
	{
		if (insert_edges_id == -1)
		{
			insert_edges_id = InsertedEdgesID;
		}
		do_split(line, insert_edges: true, insert_edges_id);
	}

	protected virtual void do_split(Line2d line, bool insert_edges, int insert_gid)
	{
		if (EdgeSigns.Length < Graph.MaxVertexID)
		{
			EdgeSigns.resize(Graph.MaxVertexID);
		}
		foreach (int item2 in Graph.VertexIndices())
		{
			EdgeSigns[item2] = line.WhichSide(Graph.GetVertex(item2), OnVertexTol);
		}
		hits.Clear();
		foreach (int item3 in Graph.EdgeIndices())
		{
			Index2i edgeV = Graph.GetEdgeV(item3);
			Index2i vtx_signs = new Index2i(EdgeSigns[edgeV.a], EdgeSigns[edgeV.b]);
			if (vtx_signs.a * vtx_signs.b > 0)
			{
				continue;
			}
			edge_hit edge_hit = default(edge_hit);
			edge_hit.hit_eid = item3;
			edge_hit.vtx_signs = vtx_signs;
			edge_hit.hit_vid = -1;
			edge_hit item = edge_hit;
			Vector2d vertex = Graph.GetVertex(edgeV.a);
			Vector2d vertex2 = Graph.GetVertex(edgeV.b);
			if (vtx_signs.a == vtx_signs.b)
			{
				if (vertex.DistanceSquared(vertex2) > 2.220446049250313E-16)
				{
					item.hit_vid = edgeV.a;
					item.line_t = line.Project(vertex);
					hits.Add(item);
					item.hit_vid = edgeV.b;
					item.line_t = line.Project(vertex2);
					hits.Add(item);
				}
				else
				{
					vtx_signs.b = 1;
				}
			}
			if (vtx_signs.a == 0)
			{
				item.hit_pos = vertex;
				item.hit_vid = edgeV.a;
				item.line_t = line.Project(vertex);
			}
			else if (vtx_signs.b == 0)
			{
				item.hit_pos = vertex2;
				item.hit_vid = edgeV.b;
				item.line_t = line.Project(vertex2);
			}
			else
			{
				IntrLine2Segment2 intrLine2Segment = new IntrLine2Segment2(line, new Segment2d(vertex, vertex2));
				if (!intrLine2Segment.Find())
				{
					throw new Exception("GraphSplitter2d.Split: signs are different but ray did not it?");
				}
				if (!intrLine2Segment.IsSimpleIntersection)
				{
					throw new Exception("GraphSplitter2d.Split: got parallel edge case!");
				}
				item.hit_pos = intrLine2Segment.Point;
				item.line_t = intrLine2Segment.Parameter;
			}
			hits.Add(item);
		}
		hits.Sort((edge_hit hit0, edge_hit hit1) => hit0.line_t.CompareTo(hit1.line_t));
		int count = hits.Count;
		for (int i = 0; i < count - 1; i++)
		{
			int index = i + 1;
			if (hits[i].line_t == hits[index].line_t || hits[i].hit_eid == hits[index].hit_eid)
			{
				continue;
			}
			int num = hits[i].hit_vid;
			int num2 = hits[index].hit_vid;
			if ((num == num2 && num >= 0) || (num >= 0 && num2 >= 0 && Graph.FindEdge(num, num2) >= 0))
			{
				continue;
			}
			if (num == -1)
			{
				if (Graph.SplitEdge(hits[i].hit_eid, out var split) != 0)
				{
					throw new Exception("GraphSplitter2d.Split: first edge split failed!");
				}
				num = split.vNew;
				Graph.SetVertex(num, hits[i].hit_pos);
				edge_hit value = hits[i];
				value.hit_vid = num;
				hits[i] = value;
			}
			if (num2 == -1)
			{
				if (Graph.SplitEdge(hits[index].hit_eid, out var split2) != 0)
				{
					throw new Exception("GraphSplitter2d.Split: second edge split failed!");
				}
				num2 = split2.vNew;
				Graph.SetVertex(num2, hits[index].hit_pos);
				edge_hit value2 = hits[index];
				value2.hit_vid = num2;
				hits[index] = value2;
			}
			if (InsideTestF != null)
			{
				Vector2d arg = 0.5 * (Graph.GetVertex(num) + Graph.GetVertex(num2));
				if (!InsideTestF(arg))
				{
					continue;
				}
			}
			if (insert_edges)
			{
				Graph.AppendEdge(num, num2, insert_gid);
			}
		}
	}
}
