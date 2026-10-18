using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public static class DGraph2Util
{
	public class Curves
	{
		public List<Polygon2d> Loops;

		public List<PolyLine2d> Paths;
	}

	public static Curves ExtractCurves(DGraph2 graph)
	{
		Curves curves = new Curves();
		curves.Loops = new List<Polygon2d>();
		curves.Paths = new List<PolyLine2d>();
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
			if (!hashSet.Contains(num2))
			{
				PolyLine2d polyLine2d = new PolyLine2d();
				polyLine2d.AppendVertex(graph.GetVertex(num));
				do
				{
					hashSet.Add(num2);
					Index2i index2i = NextEdgeAndVtx(num2, num, graph);
					num2 = index2i.a;
					num = index2i.b;
					polyLine2d.AppendVertex(graph.GetVertex(num));
				}
				while (!hashSet2.Contains(num) && !hashSet3.Contains(num));
				curves.Paths.Add(polyLine2d);
			}
		}
		hashSet2.Clear();
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
				PolyLine2d polyLine2d2 = new PolyLine2d();
				polyLine2d2.AppendVertex(graph.GetVertex(num3));
				bool flag = false;
				do
				{
					hashSet.Add(num4);
					Index2i index2i2 = NextEdgeAndVtx(num4, num3, graph);
					num4 = index2i2.a;
					num3 = index2i2.b;
					if (num3 == item3)
					{
						flag = true;
						break;
					}
					polyLine2d2.AppendVertex(graph.GetVertex(num3));
				}
				while (num4 != int.MaxValue && !hashSet3.Contains(num3));
				if (flag)
				{
					curves.Loops.Add(new Polygon2d(polyLine2d2.Vertices));
				}
				else
				{
					curves.Paths.Add(polyLine2d2);
				}
			}
		}
		foreach (int item5 in graph.EdgeIndices())
		{
			if (hashSet.Contains(item5))
			{
				continue;
			}
			int num5 = item5;
			int num6 = graph.GetEdgeV(num5).a;
			Polygon2d polygon2d = new Polygon2d();
			polygon2d.AppendVertex(graph.GetVertex(num6));
			do
			{
				hashSet.Add(num5);
				Index2i index2i3 = NextEdgeAndVtx(num5, num6, graph);
				num5 = index2i3.a;
				num6 = index2i3.b;
				polygon2d.AppendVertex(graph.GetVertex(num6));
				if (num5 == int.MaxValue || hashSet3.Contains(num6))
				{
					throw new Exception("how did this happen??");
				}
			}
			while (!hashSet.Contains(num5));
			polygon2d.RemoveVertex(polygon2d.VertexCount - 1);
			curves.Loops.Add(polygon2d);
		}
		return curves;
	}

	public static void ChainOpenPaths(Curves c, double epsilon = 2.220446049250313E-16)
	{
		List<PolyLine2d> list = new List<PolyLine2d>(c.Paths);
		c.Paths = new List<PolyLine2d>();
		List<PolyLine2d> list2 = new List<PolyLine2d>();
		List<PolyLine2d> list3 = new List<PolyLine2d>();
		bool flag = true;
		while (flag && list.Count > 0)
		{
			flag = false;
			foreach (PolyLine2d item in list)
			{
				List<PolyLine2d> list4 = find_connected_start(item, list, epsilon);
				List<PolyLine2d> list5 = find_connected_end(item, list, epsilon);
				if (list4.Count == 0 || list5.Count == 0)
				{
					list2.Add(item);
					flag = true;
				}
				else
				{
					list3.Add(item);
				}
			}
			list.Clear();
			list.AddRange(list3);
			list3.Clear();
		}
		flag = true;
		while (flag && list.Count > 0)
		{
			flag = false;
			while (true)
			{
				IL_00b8:
				foreach (PolyLine2d item2 in list)
				{
					List<PolyLine2d> list6 = find_connected_start(item2, list, epsilon);
					List<PolyLine2d> list7 = find_connected_end(item2, list, 2.0 * epsilon);
					if (list6.Count == 1 && list7.Count == 1 && list6[0] == list7[0])
					{
						c.Loops.Add(to_loop(item2, list6[0], epsilon));
						list.Remove(item2);
						list.Remove(list6[0]);
						list3.Remove(list6[0]);
						flag = true;
					}
					else if (list6.Count == 1 && list7.Count < 2)
					{
						list3.Add(merge_paths(list6[0], item2, 2.0 * epsilon));
						list.Remove(item2);
						list.Remove(list6[0]);
						list3.Remove(list6[0]);
						flag = true;
					}
					else
					{
						if (list7.Count != 1 || list6.Count >= 2)
						{
							list3.Add(item2);
							continue;
						}
						list3.Add(merge_paths(item2, list7[0], 2.0 * epsilon));
						list.Remove(item2);
						list.Remove(list7[0]);
						list3.Remove(list7[0]);
						flag = true;
					}
					goto IL_00b8;
				}
				break;
			}
			list.Clear();
			list.AddRange(list3);
			list3.Clear();
		}
		c.Paths.AddRange(list);
		c.Paths.AddRange(list2);
	}

	private static List<PolyLine2d> find_connected_start(PolyLine2d pTest, List<PolyLine2d> potential, double eps = 2.220446049250313E-16)
	{
		List<PolyLine2d> list = new List<PolyLine2d>();
		foreach (PolyLine2d item in potential)
		{
			if (pTest != item && (pTest.Start.Distance(item.Start) < eps || pTest.Start.Distance(item.End) < eps))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static List<PolyLine2d> find_connected_end(PolyLine2d pTest, List<PolyLine2d> potential, double eps = 2.220446049250313E-16)
	{
		List<PolyLine2d> list = new List<PolyLine2d>();
		foreach (PolyLine2d item in potential)
		{
			if (pTest != item && (pTest.End.Distance(item.Start) < eps || pTest.End.Distance(item.End) < eps))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static Polygon2d to_loop(PolyLine2d p1, PolyLine2d p2, double eps = 2.220446049250313E-16)
	{
		Polygon2d polygon2d = new Polygon2d(p1.Vertices);
		if (p1.End.Distance(p2.Start) > eps)
		{
			p2.Reverse();
		}
		polygon2d.AppendVertices(p2);
		return polygon2d;
	}

	private static PolyLine2d merge_paths(PolyLine2d p1, PolyLine2d p2, double eps = 2.220446049250313E-16)
	{
		PolyLine2d polyLine2d;
		if (p1.End.Distance(p2.Start) < eps)
		{
			polyLine2d = new PolyLine2d(p1);
			polyLine2d.AppendVertices(p2);
		}
		else if (p1.End.Distance(p2.End) < eps)
		{
			polyLine2d = new PolyLine2d(p1);
			p2.Reverse();
			polyLine2d.AppendVertices(p2);
		}
		else if (p1.Start.Distance(p2.Start) < eps)
		{
			p2.Reverse();
			polyLine2d = new PolyLine2d(p2);
			polyLine2d.AppendVertices(p1);
		}
		else
		{
			if (!(p1.Start.Distance(p2.End) < eps))
			{
				throw new Exception("shit");
			}
			polyLine2d = new PolyLine2d(p2);
			polyLine2d.AppendVertices(p1);
		}
		return polyLine2d;
	}

	public static int DisconnectJunctions(DGraph2 graph)
	{
		List<int> list = new List<int>();
		foreach (int item in graph.VertexIndices())
		{
			if (graph.IsJunctionVertex(item))
			{
				list.Add(item);
			}
		}
		foreach (int item2 in list)
		{
			Vector2d vertex = graph.GetVertex(item2);
			int[] array = graph.VtxVerticesItr(item2).ToArray();
			Index2i index2i = Index2i.Max;
			double num = 0.0;
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = i + 1; j < array.Length; j++)
				{
					double value = Vector2d.AngleD((graph.GetVertex(array[i]) - vertex).Normalized, (graph.GetVertex(array[j]) - vertex).Normalized);
					value = Math.Abs(value);
					if (value > num)
					{
						num = value;
						index2i = new Index2i(array[i], array[j]);
					}
				}
			}
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] != index2i.a && array[k] != index2i.b)
				{
					int eID = graph.FindEdge(item2, array[k]);
					graph.RemoveEdge(eID, bRemoveIsolatedVertices: true);
					if (graph.IsVertex(array[k]))
					{
						Vector2d v = Vector2d.Lerp(graph.GetVertex(array[k]), vertex, 0.99);
						int v2 = graph.AppendVertex(v);
						graph.AppendEdge(array[k], v2);
					}
				}
			}
		}
		return list.Count;
	}

	public static void DisconnectJunction(DGraph2 graph, int vid, double shrinkFactor = 1.0)
	{
		Vector2d vertex = graph.GetVertex(vid);
		int[] array = graph.VtxVerticesItr(vid).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			int eID = graph.FindEdge(vid, array[i]);
			graph.RemoveEdge(eID, bRemoveIsolatedVertices: true);
			if (graph.IsVertex(array[i]))
			{
				Vector2d v = Vector2d.Lerp(graph.GetVertex(array[i]), vertex, shrinkFactor);
				int v2 = graph.AppendVertex(v);
				graph.AppendEdge(array[i], v2);
			}
		}
	}

	public static Vector2d VertexLaplacian(DGraph2 graph, int vid, out bool isValid)
	{
		Vector2d vertex = graph.GetVertex(vid);
		Vector2d zero = Vector2d.Zero;
		int num = 0;
		foreach (int item in graph.VtxVerticesItr(vid))
		{
			zero += graph.GetVertex(item);
			num++;
		}
		if (num == 2)
		{
			zero /= (double)num;
			isValid = true;
			return zero - vertex;
		}
		isValid = false;
		return vertex;
	}

	public static bool FindRayIntersection(Vector2d o, Vector2d d, out int hit_eid, out double hit_ray_t, DGraph2 graph)
	{
		Line2d line = new Line2d(o, d);
		Vector2d a = Vector2d.Zero;
		Vector2d b = Vector2d.Zero;
		int num = -1;
		double num2 = double.MaxValue;
		IntrLine2Segment2 intrLine2Segment = new IntrLine2Segment2(line, new Segment2d(a, b));
		foreach (int item in graph.VertexIndices())
		{
			graph.GetEdgeV(item, ref a, ref b);
			intrLine2Segment.Segment = new Segment2d(a, b);
			if (intrLine2Segment.Find() && intrLine2Segment.IsSimpleIntersection && intrLine2Segment.Parameter > 0.0 && intrLine2Segment.Parameter < num2)
			{
				num = item;
				num2 = intrLine2Segment.Parameter;
			}
		}
		hit_eid = num;
		hit_ray_t = num2;
		return hit_ray_t < double.MaxValue;
	}

	public static Index2i NextEdgeAndVtx(int eid, int prev_vid, DGraph2 graph)
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

	public static List<int> WalkToNextNonRegularVtx(DGraph2 graph, int fromVtx, int eid)
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

	public static double PathLength(DGraph2 graph, IList<int> pathVertices)
	{
		double num = 0.0;
		int count = pathVertices.Count;
		Vector2d vector2d = graph.GetVertex(pathVertices[0]);
		Vector2d zero = Vector2d.Zero;
		for (int i = 1; i < count; i++)
		{
			zero = graph.GetVertex(pathVertices[i]);
			num += vector2d.Distance(zero);
			vector2d = zero;
		}
		return num;
	}
}
