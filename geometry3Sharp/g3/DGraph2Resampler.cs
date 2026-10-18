using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace g3;

public class DGraph2Resampler
{
	public DGraph2 Graph;

	public Func<int, bool> FixedEdgeFilterF = (int eid) => false;

	public DGraph2Resampler(DGraph2 graph)
	{
		Graph = graph;
	}

	public void SplitToMaxEdgeLength(double fMaxLen)
	{
		List<int> list = new List<int>();
		int maxEdgeID = Graph.MaxEdgeID;
		for (int i = 0; i < maxEdgeID; i++)
		{
			if (Graph.IsEdge(i) && !FixedEdgeFilterF(i))
			{
				Index2i edgeV = Graph.GetEdgeV(i);
				double num = Graph.GetVertex(edgeV.a).Distance(Graph.GetVertex(edgeV.b));
				if (num > fMaxLen && Graph.SplitEdge(i, out var split) == MeshResult.Ok && num > 2.0 * fMaxLen)
				{
					list.Add(i);
					list.Add(split.eNewBN);
				}
			}
		}
		while (list.Count > 0)
		{
			int num2 = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			if (Graph.IsEdge(num2))
			{
				Index2i edgeV2 = Graph.GetEdgeV(num2);
				double num3 = Graph.GetVertex(edgeV2.a).Distance(Graph.GetVertex(edgeV2.b));
				if (num3 > fMaxLen && Graph.SplitEdge(num2, out var split2) == MeshResult.Ok && num3 > 2.0 * fMaxLen)
				{
					list.Add(num2);
					list.Add(split2.eNewBN);
				}
			}
		}
	}

	public void CollapseFlatVertices(double fMaxDeviationDeg = 5.0)
	{
		bool flag = false;
		int num = 200;
		int num2 = 0;
		while (!flag && num2++ < num)
		{
			flag = true;
			int maxVertexID = Graph.MaxVertexID;
			int num3 = 0;
			do
			{
				int num4 = num3;
				num3 = (num3 + 31337) % maxVertexID;
				if (!Graph.IsVertex(num4) || Graph.GetVtxEdgeCount(num4) != 2 || Math.Abs(Graph.OpeningAngle(num4)) < 180.0 - fMaxDeviationDeg)
				{
					continue;
				}
				ReadOnlyCollection<int> vtxEdges = Graph.GetVtxEdges(num4);
				int num5 = vtxEdges.First();
				int arg = vtxEdges.Last();
				if (!FixedEdgeFilterF(num5) && !FixedEdgeFilterF(arg))
				{
					Index2i edgeV = Graph.GetEdgeV(num5);
					int vKeep = ((edgeV.a == num4) ? edgeV.b : edgeV.a);
					if (Graph.CollapseEdge(vKeep, num4, out var _) != 0)
					{
						throw new Exception("DGraph2Resampler.CollapseFlatVertices: failed!");
					}
					flag = false;
				}
			}
			while (num3 != 0);
		}
	}

	public void CollapseDegenerateEdges(double fDegenLenThresh = 1.1920928955078125E-07)
	{
		bool flag = false;
		int num = 100;
		int num2 = 0;
		while (!flag && num2++ < num)
		{
			flag = true;
			int maxEdgeID = Graph.MaxEdgeID;
			for (int i = 0; i < maxEdgeID; i++)
			{
				if (!Graph.IsEdge(i) || FixedEdgeFilterF(i))
				{
					continue;
				}
				Index2i edgeV = Graph.GetEdgeV(i);
				Vector2d vertex = Graph.GetVertex(edgeV.a);
				Vector2d vertex2 = Graph.GetVertex(edgeV.b);
				if (vertex.Distance(vertex2) < fDegenLenThresh)
				{
					int a = edgeV.a;
					int b = edgeV.b;
					if (Graph.CollapseEdge(a, b, out var _) == MeshResult.Ok)
					{
						flag = false;
					}
				}
			}
		}
	}

	public void CollapseToMinEdgeLength(double fMinLen)
	{
		double num = 140.0;
		double num2 = fMinLen * fMinLen;
		bool flag = false;
		int num3 = 100;
		int num4 = 0;
		while (!flag && num4++ < num3)
		{
			flag = true;
			int maxEdgeID = Graph.MaxEdgeID;
			int num5 = 0;
			do
			{
				int num6 = num5;
				num5 = (num5 + 31337) % maxEdgeID;
				if (!Graph.IsEdge(num6) || FixedEdgeFilterF(num6))
				{
					continue;
				}
				Index2i edgeV = Graph.GetEdgeV(num6);
				Vector2d vertex = Graph.GetVertex(edgeV.a);
				Vector2d vertex2 = Graph.GetVertex(edgeV.b);
				if (!(vertex.DistanceSquared(vertex2) < num2))
				{
					continue;
				}
				int num7 = -1;
				int vtxEdgeCount = Graph.GetVtxEdgeCount(edgeV.a);
				int vtxEdgeCount2 = Graph.GetVtxEdgeCount(edgeV.b);
				if (vtxEdgeCount != 2 && vtxEdgeCount2 != 2)
				{
					continue;
				}
				if (vtxEdgeCount != 2)
				{
					num7 = 0;
				}
				else if (vtxEdgeCount2 != 2)
				{
					num7 = 1;
				}
				if (num7 == -1)
				{
					double num8 = Math.Abs(Graph.OpeningAngle(edgeV.a));
					double num9 = Math.Abs(Graph.OpeningAngle(edgeV.b));
					if (num8 < num && num9 < num)
					{
						continue;
					}
					if (num8 < num)
					{
						num7 = 0;
					}
					else if (num9 < num)
					{
						num7 = 1;
					}
				}
				Vector2d vNewPos = num7 switch
				{
					0 => vertex, 
					-1 => 0.5 * (vertex + vertex2), 
					_ => vertex2, 
				};
				int vKeep = edgeV.a;
				int vRemove = edgeV.b;
				if (num7 == 1)
				{
					vRemove = edgeV.a;
					vKeep = edgeV.b;
				}
				if (Graph.CollapseEdge(vKeep, vRemove, out var collapse) == MeshResult.Ok)
				{
					Graph.SetVertex(collapse.vKept, vNewPos);
					flag = false;
				}
			}
			while (num5 != 0);
		}
	}
}
