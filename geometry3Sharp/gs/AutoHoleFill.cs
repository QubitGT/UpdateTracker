using System.Collections.Generic;
using g3;

namespace gs;

public class AutoHoleFill
{
	private enum UseFillType
	{
		PlanarFill,
		MinimalFill,
		PlanarSpansFill,
		SmoothFill
	}

	public DMesh3 Mesh;

	public double TargetEdgeLength = 2.5;

	public EdgeLoop FillLoop;

	public int[] FillTriangles;

	public AutoHoleFill(DMesh3 mesh, EdgeLoop fillLoop)
	{
		Mesh = mesh;
		FillLoop = fillLoop;
	}

	public bool Apply()
	{
		UseFillType useFillType = classify_hole();
		bool flag = false;
		bool flag2 = false;
		flag = ((useFillType == UseFillType.PlanarFill && !flag2) ? fill_planar() : (useFillType switch
		{
			UseFillType.MinimalFill => fill_minimal(), 
			UseFillType.PlanarSpansFill => fill_planar_spans(), 
			_ => fill_smooth(), 
		}));
		if (!flag && useFillType != UseFillType.SmoothFill)
		{
			flag = fill_smooth();
		}
		return flag;
	}

	private UseFillType classify_hole()
	{
		return UseFillType.MinimalFill;
	}

	private bool fill_smooth()
	{
		return new SmoothedHoleFill(Mesh, FillLoop)
		{
			TargetEdgeLength = TargetEdgeLength,
			SmoothAlpha = 1.0,
			ConstrainToHoleInterior = true
		}.Apply();
	}

	private bool fill_planar()
	{
		Vector3d zero = Vector3d.Zero;
		Vector3d zero2 = Vector3d.Zero;
		int edgeCount = FillLoop.EdgeCount;
		for (int i = 0; i < edgeCount; i++)
		{
			int eID = FillLoop.Edges[i];
			zero += Mesh.GetTriNormal(Mesh.GetEdgeT(eID).a);
			zero2 += Mesh.GetEdgePoint(eID, 0.5);
		}
		zero.Normalize();
		zero2 /= (double)edgeCount;
		PlanarHoleFiller planarHoleFiller = new PlanarHoleFiller(Mesh);
		planarHoleFiller.FillTargetEdgeLen = TargetEdgeLength;
		planarHoleFiller.AddFillLoop(FillLoop);
		planarHoleFiller.SetPlane(zero2, zero);
		return planarHoleFiller.Fill();
	}

	private bool fill_minimal()
	{
		return new MinimalHoleFill(Mesh, FillLoop).Apply();
	}

	private bool fill_planar_spans()
	{
		foreach (KeyValuePair<Vector3d, List<EdgeSpan>> item in find_coplanar_span_sets(Mesh, FillLoop))
		{
			Vector3d key = item.Key;
			List<EdgeSpan> value = item.Value;
			Vector3d vertex = value[0].GetVertex(0);
			if (value.Count > 1)
			{
				foreach (List<EdgeSpan> item2 in sort_planar_spans(value, key))
				{
					if (item2.Count == 1)
					{
						PlanarSpansFiller planarSpansFiller = new PlanarSpansFiller(Mesh, item2);
						planarSpansFiller.FillTargetEdgeLen = TargetEdgeLength;
						planarSpansFiller.SetPlane(vertex, key);
						planarSpansFiller.Fill();
					}
				}
			}
			else
			{
				PlanarSpansFiller planarSpansFiller2 = new PlanarSpansFiller(Mesh, value);
				planarSpansFiller2.FillTargetEdgeLen = TargetEdgeLength;
				planarSpansFiller2.SetPlane(vertex, key);
				planarSpansFiller2.Fill();
			}
		}
		return true;
	}

	private List<List<EdgeSpan>> sort_planar_spans(List<EdgeSpan> allspans, Vector3d normal)
	{
		List<List<EdgeSpan>> list = new List<List<EdgeSpan>>();
		Frame3f polyFrame = new Frame3f(Vector3d.Zero, normal);
		int count = allspans.Count;
		List<PolyLine2d> list2 = new List<PolyLine2d>();
		foreach (EdgeSpan allspan in allspans)
		{
			list2.Add(to_polyline(allspan, polyFrame));
		}
		bool[] array = new bool[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = false;
		}
		bool[] array2 = new bool[count];
		for (int j = 0; j < count; j++)
		{
			if (array2[j])
			{
				continue;
			}
			bool flag = array[j];
			AxisAlignedBox2d bounds = list2[j].Bounds;
			array2[j] = true;
			List<int> list3 = new List<int> { j };
			for (int k = j + 1; k < count; k++)
			{
				if (!array2[k])
				{
					AxisAlignedBox2d bounds2 = list2[k].Bounds;
					if (bounds.Intersects(bounds2))
					{
						array2[k] = true;
						flag = flag || array[k];
						bounds.Contain(bounds2);
						list3.Add(k);
					}
				}
			}
			if (flag)
			{
				continue;
			}
			List<EdgeSpan> list4 = new List<EdgeSpan>();
			foreach (int item in list3)
			{
				list4.Add(allspans[item]);
			}
			list.Add(list4);
		}
		return list;
	}

	private PolyLine2d to_polyline(EdgeSpan span, Frame3f polyFrame)
	{
		int vertexCount = span.VertexCount;
		PolyLine2d polyLine2d = new PolyLine2d();
		for (int i = 0; i < vertexCount; i++)
		{
			polyLine2d.AppendVertex(polyFrame.ToPlaneUV((Vector3f)span.GetVertex(i), 2));
		}
		return polyLine2d;
	}

	private Polygon2d to_polygon(EdgeSpan span, Frame3f polyFrame)
	{
		int vertexCount = span.VertexCount;
		Polygon2d polygon2d = new Polygon2d();
		for (int i = 0; i < vertexCount; i++)
		{
			polygon2d.AppendVertex(polyFrame.ToPlaneUV((Vector3f)span.GetVertex(i), 2));
		}
		return polygon2d;
	}

	private bool self_intersects(PolyLine2d poly)
	{
		Segment2d seg = new Segment2d(poly.Start, poly.End);
		int num = poly.VertexCount - 2;
		for (int i = 1; i < num; i++)
		{
			if (poly.Segment(i).Intersects(ref seg))
			{
				return true;
			}
		}
		return false;
	}

	private Dictionary<Vector3d, List<EdgeSpan>> find_coplanar_span_sets(DMesh3 mesh, EdgeLoop loop)
	{
		double num = 0.999;
		Dictionary<Vector3d, List<EdgeSpan>> dictionary = new Dictionary<Vector3d, List<EdgeSpan>>();
		int num2 = loop.Vertices.Length;
		int num3 = loop.Edges.Length;
		Vector3d[] array = new Vector3d[num3];
		for (int i = 0; i < num3; i++)
		{
			array[i] = mesh.GetTriNormal(mesh.GetEdgeT(loop.Edges[i]).a);
		}
		bool[] array2 = new bool[num2];
		int num4 = 0;
		for (int j = 0; j < num2; j++)
		{
			int num5 = ((j == 0) ? (num2 - 1) : (j - 1));
			if (array[j].Dot(ref array[num5]) > num)
			{
				array2[j] = true;
				num4++;
			}
		}
		if (num4 < 2)
		{
			return null;
		}
		int k;
		for (k = 0; array2[k]; k++)
		{
		}
		int num6 = k;
		int num7 = k + 1;
		while (num7 != k)
		{
			if (!array2[num7])
			{
				num6 = num7;
				num7 = (num7 + 1) % num2;
				continue;
			}
			List<int> list = new List<int> { loop.Edges[num6] };
			int num8 = num7;
			while (array2[num7])
			{
				list.Add(loop.Edges[num7]);
				num7 = (num7 + 1) % num2;
			}
			if (list.Count <= 1)
			{
				continue;
			}
			Vector3d v = array[num8];
			EdgeSpan edgeSpan = EdgeSpan.FromEdges(mesh, list);
			edgeSpan.CheckValidity();
			foreach (KeyValuePair<Vector3d, List<EdgeSpan>> item in dictionary)
			{
				if (item.Key.Dot(ref v) > num)
				{
					v = item.Key;
					break;
				}
			}
			if (!dictionary.TryGetValue(v, out var value))
			{
				dictionary[v] = new List<EdgeSpan> { edgeSpan };
			}
			else
			{
				value.Add(edgeSpan);
			}
		}
		return dictionary;
	}
}
