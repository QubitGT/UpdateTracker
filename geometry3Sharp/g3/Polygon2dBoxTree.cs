using System;
using System.Collections.Generic;

namespace g3;

public class Polygon2dBoxTree
{
	public Polygon2d Polygon;

	private Box2d[] boxes;

	private int layers;

	private List<int> layer_counts;

	public Polygon2dBoxTree(Polygon2d poly)
	{
		Polygon = poly;
		build_sequential(poly);
	}

	public double DistanceSquared(Vector2d pt)
	{
		int iNearSeg;
		double fNearSegT;
		return SquaredDistance(pt, out iNearSeg, out fNearSegT);
	}

	public double Distance(Vector2d pt)
	{
		int iNearSeg;
		double fNearSegT;
		return Math.Sqrt(SquaredDistance(pt, out iNearSeg, out fNearSegT));
	}

	public Vector2d NearestPoint(Vector2d pt)
	{
		SquaredDistance(pt, out var iNearSeg, out var fNearSegT);
		return Polygon.PointAt(iNearSeg, fNearSegT);
	}

	public double SquaredDistance(Vector2d pt, out int iNearSeg, out double fNearSegT, double max_dist = double.MaxValue)
	{
		int iLayerStart = boxes.Length - 1;
		int iLayer = layers - 1;
		double min_dist = max_dist;
		iNearSeg = -1;
		fNearSegT = 0.0;
		find_min_distance(ref pt, ref min_dist, ref iNearSeg, ref fNearSegT, 0, iLayerStart, iLayer);
		if (iNearSeg == -1)
		{
			return double.MaxValue;
		}
		return min_dist;
	}

	private void find_min_distance(ref Vector2d pt, ref double min_dist, ref int min_dist_seg, ref double min_dist_segt, int bi, int iLayerStart, int iLayer)
	{
		if (iLayer == 0)
		{
			int num = 2 * bi;
			double num2 = Polygon.Segment(num).DistanceSquared(pt, out var t);
			if (num2 <= min_dist)
			{
				min_dist = num2;
				min_dist_seg = num;
				min_dist_segt = t;
			}
			if (num + 1 < Polygon.VertexCount)
			{
				num2 = Polygon.Segment(num + 1).DistanceSquared(pt, out t);
				if (num2 <= min_dist)
				{
					min_dist = num2;
					min_dist_seg = num + 1;
					min_dist_segt = t;
				}
			}
			return;
		}
		int num3 = iLayer - 1;
		int num4 = layer_counts[num3];
		int num5 = iLayerStart - num4;
		int num6 = num5 + 2 * bi;
		if (boxes[num6].DistanceSquared(pt) <= min_dist)
		{
			find_min_distance(ref pt, ref min_dist, ref min_dist_seg, ref min_dist_segt, 2 * bi, num5, num3);
		}
		if (2 * bi + 1 < num4)
		{
			int num7 = num6 + 1;
			if (boxes[num7].DistanceSquared(pt) <= min_dist)
			{
				find_min_distance(ref pt, ref min_dist, ref min_dist_seg, ref min_dist_segt, 2 * bi + 1, num5, num3);
			}
		}
	}

	private void build_sequential(Polygon2d poly)
	{
		int vertexCount = poly.VertexCount;
		int num = vertexCount;
		int num2 = 0;
		layers = 0;
		layer_counts = new List<int>();
		int num3 = 0;
		while (num > 1)
		{
			int num4 = num / 2 + ((num % 2 != 0) ? 1 : 0);
			num2 += num4;
			num = num4;
			layer_counts.Add(num4);
			num3 += num4;
			layers++;
		}
		boxes = new Box2d[num2];
		num3 = 0;
		for (int i = 0; i < vertexCount; i += 2)
		{
			Vector2d vector2d = poly[(i + 1) % vertexCount];
			Segment2d seg = new Segment2d(poly[i], vector2d);
			Box2d box = new Box2d(seg);
			if (i < vertexCount - 1)
			{
				Segment2d seg2 = new Segment2d(vector2d, poly[(i + 2) % vertexCount]);
				Box2d box2 = new Box2d(seg2);
				box = Box2d.Merge(ref box, ref box2);
			}
			boxes[num3++] = box;
		}
		num = num3;
		int num5 = 0;
		bool flag = false;
		while (!flag)
		{
			int num6 = num3;
			for (int j = 0; j < num; j += 2)
			{
				Box2d box2d = Box2d.Merge(ref boxes[num5 + j], ref boxes[num5 + j + 1]);
				boxes[num3++] = box2d;
			}
			num = num / 2 + ((num % 2 != 0) ? 1 : 0);
			num5 = num6;
			if (num == 1)
			{
				flag = true;
			}
		}
	}
}
