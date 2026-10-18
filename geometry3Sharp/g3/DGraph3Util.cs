using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public static class DGraph3Util
{
	public struct Curves
	{
		public List<DCurve3> Loops;

		public List<DCurve3> Paths;

		public HashSet<int> BoundaryV;

		public HashSet<int> JunctionV;

		public List<List<int>> LoopEdges;

		public List<List<int>> PathEdges;
	}

	public static Curves ExtractCurves(DGraph3 graph, bool bWantLoopIndices = false, Func<int, bool> CurveOrientationF = null)
	{
		Curves result = default(Curves);
		result.Loops = new List<DCurve3>();
		result.Paths = new List<DCurve3>();
		if (bWantLoopIndices)
		{
			result.LoopEdges = new List<List<int>>();
			result.PathEdges = new List<List<int>>();
		}
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		HashSet<int> hashSet3 = new HashSet<int>();
		foreach (int item in graph.VertexIndices())
		{
			if (graph.IsBoundaryVertex(item))
			{
				hashSet2.Add(item);
			}
			if (graph.IsJunctionVertex(item))
			{
				hashSet3.Add(item);
			}
		}
		foreach (int item2 in hashSet2)
		{
			int num = item2;
			int num2 = graph.GetVtxEdges(num)[0];
			if (hashSet.Contains(num2))
			{
				continue;
			}
			bool flag = CurveOrientationF?.Invoke(num2) ?? false;
			DCurve3 dCurve = new DCurve3
			{
				Closed = false
			};
			List<int> list = (bWantLoopIndices ? new List<int>() : null);
			dCurve.AppendVertex(graph.GetVertex(num));
			list?.Add(num2);
			while (true)
			{
				hashSet.Add(num2);
				Index2i index2i = NextEdgeAndVtx(num2, num, graph);
				num2 = index2i.a;
				num = index2i.b;
				dCurve.AppendVertex(graph.GetVertex(num));
				if (hashSet2.Contains(num) || hashSet3.Contains(num))
				{
					break;
				}
				list?.Add(num2);
			}
			if (flag)
			{
				dCurve.Reverse();
			}
			result.Paths.Add(dCurve);
			if (list != null)
			{
				if (flag)
				{
					list.Reverse();
				}
				result.PathEdges.Add(list);
			}
		}
		result.BoundaryV = hashSet2;
		foreach (int item3 in hashSet3)
		{
			foreach (int item4 in graph.VtxEdgesItr(item3))
			{
				if (hashSet.Contains(item4))
				{
					continue;
				}
				int num3 = item3;
				int num4 = item4;
				bool flag2 = CurveOrientationF?.Invoke(num4) ?? false;
				DCurve3 dCurve2 = new DCurve3
				{
					Closed = false
				};
				List<int> list2 = (bWantLoopIndices ? new List<int>() : null);
				dCurve2.AppendVertex(graph.GetVertex(num3));
				list2?.Add(num4);
				while (true)
				{
					hashSet.Add(num4);
					Index2i index2i2 = NextEdgeAndVtx(num4, num3, graph);
					num4 = index2i2.a;
					num3 = index2i2.b;
					dCurve2.AppendVertex(graph.GetVertex(num3));
					if (num4 == int.MaxValue || hashSet3.Contains(num3))
					{
						break;
					}
					list2?.Add(num4);
				}
				if (num3 == item3)
				{
					dCurve2.RemoveVertex(dCurve2.VertexCount - 1);
					dCurve2.Closed = true;
					if (flag2)
					{
						dCurve2.Reverse();
					}
					result.Loops.Add(dCurve2);
					if (list2 != null)
					{
						if (flag2)
						{
							list2.Reverse();
						}
						result.LoopEdges.Add(list2);
					}
					if (num4 != int.MaxValue)
					{
						hashSet.Add(num4);
					}
					continue;
				}
				if (flag2)
				{
					dCurve2.Reverse();
				}
				result.Paths.Add(dCurve2);
				if (list2 != null)
				{
					if (flag2)
					{
						list2.Reverse();
					}
					result.PathEdges.Add(list2);
				}
			}
		}
		result.JunctionV = hashSet3;
		foreach (int item5 in graph.EdgeIndices())
		{
			if (hashSet.Contains(item5))
			{
				continue;
			}
			int num5 = item5;
			int num6 = graph.GetEdgeV(num5).a;
			bool flag3 = CurveOrientationF?.Invoke(num5) ?? false;
			DCurve3 dCurve3 = new DCurve3
			{
				Closed = true
			};
			List<int> list3 = (bWantLoopIndices ? new List<int>() : null);
			dCurve3.AppendVertex(graph.GetVertex(num6));
			list3?.Add(num5);
			do
			{
				hashSet.Add(num5);
				Index2i index2i3 = NextEdgeAndVtx(num5, num6, graph);
				num5 = index2i3.a;
				num6 = index2i3.b;
				dCurve3.AppendVertex(graph.GetVertex(num6));
				list3?.Add(num5);
				if (num5 == int.MaxValue || hashSet3.Contains(num6))
				{
					throw new Exception("how did this happen??");
				}
			}
			while (!hashSet.Contains(num5));
			dCurve3.RemoveVertex(dCurve3.VertexCount - 1);
			if (flag3)
			{
				dCurve3.Reverse();
			}
			result.Loops.Add(dCurve3);
			if (list3 != null)
			{
				list3.RemoveAt(list3.Count - 1);
				if (flag3)
				{
					list3.Reverse();
				}
				result.LoopEdges.Add(list3);
			}
		}
		return result;
	}

	public static void DisconnectJunction(DGraph3 graph, int vid, double shrinkFactor = 1.0)
	{
		Vector3d vertex = graph.GetVertex(vid);
		int[] array = graph.VtxVerticesItr(vid).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			int eID = graph.FindEdge(vid, array[i]);
			graph.RemoveEdge(eID, bRemoveIsolatedVertices: true);
			if (graph.IsVertex(array[i]))
			{
				Vector3d v = Vector3d.Lerp(graph.GetVertex(array[i]), vertex, shrinkFactor);
				int v2 = graph.AppendVertex(v);
				graph.AppendEdge(array[i], v2);
			}
		}
	}

	public static Index2i NextEdgeAndVtx(int eid, int prev_vid, DGraph3 graph)
	{
		Index2i edgeV = graph.GetEdgeV(eid);
		if (edgeV.a == -1)
		{
			return Index2i.Max;
		}
		int num = ((edgeV.a == prev_vid) ? edgeV.b : edgeV.a);
		if (graph.GetVtxEdgeCount(num) != 2)
		{
			return new Index2i(int.MaxValue, num);
		}
		foreach (int item in graph.VtxEdgesItr(num))
		{
			if (item != eid)
			{
				return new Index2i(item, num);
			}
		}
		return Index2i.Max;
	}

	public static List<int> WalkToNextNonRegularVtx(DGraph3 graph, int fromVtx, int eid)
	{
		List<int> list = new List<int>();
		list.Add(fromVtx);
		int prev_vid = fromVtx;
		int eid2 = eid;
		bool flag = true;
		while (flag)
		{
			Index2i index2i = NextEdgeAndVtx(eid2, prev_vid, graph);
			int a = index2i.a;
			int b = index2i.b;
			if (a == int.MaxValue)
			{
				if (graph.IsRegularVertex(b))
				{
					throw new Exception("WalkToNextNonRegularVtx: have no next edge but vtx is regular - how?");
				}
				list.Add(b);
				flag = false;
			}
			else
			{
				list.Add(b);
				prev_vid = b;
				eid2 = a;
			}
		}
		return list;
	}

	public static void ErodeOpenSpurs(DGraph3 graph)
	{
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		HashSet<int> hashSet3 = new HashSet<int>();
		foreach (int item in graph.VertexIndices())
		{
			if (graph.IsBoundaryVertex(item))
			{
				hashSet2.Add(item);
			}
			if (graph.IsJunctionVertex(item))
			{
				hashSet3.Add(item);
			}
		}
		foreach (int item2 in hashSet2)
		{
			if (!graph.IsVertex(item2))
			{
				continue;
			}
			int num = item2;
			int num2 = graph.GetVtxEdges(num)[0];
			if (hashSet.Contains(num2))
			{
				continue;
			}
			List<int> list = new List<int>();
			list?.Add(num2);
			while (true)
			{
				hashSet.Add(num2);
				Index2i index2i = NextEdgeAndVtx(num2, num, graph);
				num2 = index2i.a;
				num = index2i.b;
				if (hashSet2.Contains(num) || hashSet3.Contains(num))
				{
					break;
				}
				list?.Add(num2);
			}
			foreach (int item3 in list)
			{
				graph.RemoveEdge(item3, bRemoveIsolatedVertices: true);
			}
		}
	}
}
