using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class GraphCells2d
{
	public DGraph2 Graph;

	public List<int[]> CellLoops;

	public GraphCells2d(DGraph2 graph)
	{
		Graph = graph;
	}

	public void FindCells()
	{
		Index2i[][] array = new Index2i[Graph.MaxVertexID][];
		HashSet<Index2i> hashSet = new HashSet<Index2i>();
		foreach (int item2 in Graph.VertexIndices())
		{
			int[] array2 = Graph.SortedVtxEdges(item2);
			array[item2] = new Index2i[array2.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array[item2][i] = new Index2i(array2[i], array2[(i + 1) % array2.Length]);
				hashSet.Add(new Index2i(item2, i));
			}
		}
		CellLoops = new List<int[]>();
		List<int> list = new List<int>();
		while (hashSet.Count > 0)
		{
			Index2i item = hashSet.First();
			hashSet.Remove(item);
			int a = item.a;
			int b = item.b;
			_ = array[a][b].a;
			int b2 = array[a][b].b;
			list.Clear();
			list.Add(a);
			int num = a;
			int num2 = b2;
			bool flag = false;
			while (!flag)
			{
				Index2i edgeV = Graph.GetEdgeV(num2);
				int num3 = ((edgeV.a == num) ? edgeV.b : edgeV.a);
				if (num3 == a)
				{
					flag = true;
					continue;
				}
				Index2i[] array3 = array[num3];
				int num4 = -1;
				for (int j = 0; j < array3.Length; j++)
				{
					if (array3[j].a == num2)
					{
						num4 = j;
						break;
					}
				}
				if (num4 == -1)
				{
					throw new Exception("could not find next wedge?");
				}
				hashSet.Remove(new Index2i(num3, num4));
				list.Add(num3);
				num = num3;
				num2 = array3[num4].b;
			}
			CellLoops.Add(list.ToArray());
		}
	}

	public List<Polygon2d> CellsToPolygons(Func<Polygon2d, bool> FilterF = null)
	{
		List<Polygon2d> list = new List<Polygon2d>();
		for (int i = 0; i < CellLoops.Count; i++)
		{
			int[] array = CellLoops[i];
			Polygon2d polygon2d = new Polygon2d();
			for (int j = 0; j < array.Length; j++)
			{
				polygon2d.AppendVertex(Graph.GetVertex(array[j]));
			}
			if (FilterF == null || FilterF(polygon2d))
			{
				list.Add(polygon2d);
			}
		}
		return list;
	}

	public List<Polygon2d> ContainedCells(GeneralPolygon2d container)
	{
		Func<Polygon2d, bool> filterF = delegate(Polygon2d poly)
		{
			bool isClockwise = poly.IsClockwise;
			for (int i = 0; i < poly.VertexCount; i++)
			{
				Segment2d segment2d = poly.Segment(i);
				Vector2d vector2d = segment2d.Center + 1.1920928955078125E-07 * segment2d.Direction.Perp;
				if (poly.Contains(vector2d) == isClockwise)
				{
					return container.Contains(vector2d);
				}
			}
			return false;
		};
		return CellsToPolygons(filterF);
	}
}
