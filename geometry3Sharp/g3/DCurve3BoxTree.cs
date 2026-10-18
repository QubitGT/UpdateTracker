using System;
using System.Collections.Generic;

namespace g3;

public class DCurve3BoxTree
{
	public DCurve3 Curve;

	private Box3d[] boxes;

	private int layers;

	private List<int> layer_counts;

	public DCurve3BoxTree(DCurve3 curve)
	{
		Curve = curve;
		build_sequential(curve);
	}

	public double DistanceSquared(Vector3d pt)
	{
		int iNearSeg;
		double fNearSegT;
		return SquaredDistance(pt, out iNearSeg, out fNearSegT);
	}

	public double Distance(Vector3d pt)
	{
		int iNearSeg;
		double fNearSegT;
		return Math.Sqrt(SquaredDistance(pt, out iNearSeg, out fNearSegT));
	}

	public Vector3d NearestPoint(Vector3d pt)
	{
		SquaredDistance(pt, out var iNearSeg, out var fNearSegT);
		return Curve.PointAt(iNearSeg, fNearSegT);
	}

	public double SquaredDistance(Vector3d pt, out int iNearSeg, out double fNearSegT, double max_dist = double.MaxValue)
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

	private void find_min_distance(ref Vector3d pt, ref double min_dist, ref int min_dist_seg, ref double min_dist_segt, int bi, int iLayerStart, int iLayer)
	{
		if (iLayer == 0)
		{
			int num = 2 * bi;
			double num2 = Curve.GetSegment(num).DistanceSquared(pt, out var t);
			if (num2 <= min_dist)
			{
				min_dist = num2;
				min_dist_seg = num;
				min_dist_segt = t;
			}
			if (num + 1 < Curve.SegmentCount)
			{
				num2 = Curve.GetSegment(num + 1).DistanceSquared(pt, out t);
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

	public double SquaredDistance(Ray3d ray, out int iNearSeg, out double fNearSegT, out double fRayT, double max_dist = double.MaxValue)
	{
		int iLayerStart = boxes.Length - 1;
		int iLayer = layers - 1;
		double min_dist = max_dist;
		iNearSeg = -1;
		fNearSegT = 0.0;
		fRayT = double.MaxValue;
		find_min_distance(ref ray, ref min_dist, ref iNearSeg, ref fNearSegT, ref fRayT, 0, iLayerStart, iLayer);
		if (iNearSeg == -1)
		{
			return double.MaxValue;
		}
		return min_dist;
	}

	private void find_min_distance(ref Ray3d ray, ref double min_dist, ref int min_dist_seg, ref double min_dist_segt, ref double min_dist_rayt, int bi, int iLayerStart, int iLayer)
	{
		if (iLayer == 0)
		{
			int num = 2 * bi;
			Segment3d segment = Curve.GetSegment(num);
			double num2 = Math.Sqrt(DistRay3Segment3.SquaredDistance(ref ray, ref segment, out var rayT, out var segT));
			if (num2 <= min_dist)
			{
				min_dist = num2;
				min_dist_seg = num;
				min_dist_segt = segT;
				min_dist_rayt = rayT;
			}
			if (num + 1 < Curve.SegmentCount)
			{
				Segment3d segment2 = Curve.GetSegment(num + 1);
				num2 = Math.Sqrt(DistRay3Segment3.SquaredDistance(ref ray, ref segment2, out rayT, out segT));
				if (num2 <= min_dist)
				{
					min_dist = num2;
					min_dist_seg = num + 1;
					min_dist_segt = segT;
					min_dist_rayt = rayT;
				}
			}
			return;
		}
		int num3 = iLayer - 1;
		int num4 = layer_counts[num3];
		int num5 = iLayerStart - num4;
		int num6 = num5 + 2 * bi;
		if (IntrRay3Box3.Intersects(ref ray, ref boxes[num6], min_dist))
		{
			find_min_distance(ref ray, ref min_dist, ref min_dist_seg, ref min_dist_segt, ref min_dist_rayt, 2 * bi, num5, num3);
		}
		if (2 * bi + 1 < num4)
		{
			int num7 = num6 + 1;
			if (IntrRay3Box3.Intersects(ref ray, ref boxes[num7], min_dist))
			{
				find_min_distance(ref ray, ref min_dist, ref min_dist_seg, ref min_dist_segt, ref min_dist_rayt, 2 * bi + 1, num5, num3);
			}
		}
	}

	public bool FindClosestRayIntersction(Ray3d ray, double radius, out int hitSegment, out double fRayT)
	{
		int iLayerStart = boxes.Length - 1;
		int iLayer = layers - 1;
		hitSegment = -1;
		fRayT = double.MaxValue;
		find_closest_ray_intersction(ref ray, radius, ref hitSegment, ref fRayT, 0, iLayerStart, iLayer);
		return hitSegment != -1;
	}

	private void find_closest_ray_intersction(ref Ray3d ray, double radius, ref int nearestSegment, ref double nearest_ray_t, int bi, int iLayerStart, int iLayer)
	{
		if (iLayer == 0)
		{
			int num = 2 * bi;
			Segment3d segment = Curve.GetSegment(num);
			if (DistRay3Segment3.SquaredDistance(ref ray, ref segment, out var rayT, out var segT) <= radius * radius && rayT < nearest_ray_t)
			{
				nearestSegment = num;
				nearest_ray_t = rayT;
			}
			if (num + 1 < Curve.SegmentCount)
			{
				Segment3d segment2 = Curve.GetSegment(num + 1);
				if (DistRay3Segment3.SquaredDistance(ref ray, ref segment2, out rayT, out segT) <= radius * radius && rayT < nearest_ray_t)
				{
					nearestSegment = num + 1;
					nearest_ray_t = rayT;
				}
			}
			return;
		}
		int num2 = iLayer - 1;
		int num3 = layer_counts[num2];
		int num4 = iLayerStart - num3;
		int num5 = num4 + 2 * bi;
		if (IntrRay3Box3.Intersects(ref ray, ref boxes[num5], radius))
		{
			find_closest_ray_intersction(ref ray, radius, ref nearestSegment, ref nearest_ray_t, 2 * bi, num4, num2);
		}
		if (2 * bi + 1 < num3)
		{
			int num6 = num5 + 1;
			if (IntrRay3Box3.Intersects(ref ray, ref boxes[num6], radius))
			{
				find_closest_ray_intersction(ref ray, radius, ref nearestSegment, ref nearest_ray_t, 2 * bi + 1, num4, num2);
			}
		}
	}

	private void build_sequential(DCurve3 curve)
	{
		int vertexCount = curve.VertexCount;
		int num = (curve.Closed ? vertexCount : (vertexCount - 1));
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
		if (layers == 0)
		{
			layers = 1;
			num2 = 1;
			layer_counts = new List<int> { 1 };
		}
		boxes = new Box3d[num2];
		num3 = 0;
		int num5 = (curve.Closed ? vertexCount : (vertexCount - 1));
		for (int i = 0; i < num5; i += 2)
		{
			Vector3d vector3d = curve[(i + 1) % vertexCount];
			Segment3d seg = new Segment3d(curve[i], vector3d);
			Box3d box = new Box3d(seg);
			if (i < vertexCount - 1)
			{
				Segment3d seg2 = new Segment3d(vector3d, curve[(i + 2) % vertexCount]);
				Box3d box2 = new Box3d(seg2);
				box = Box3d.Merge(ref box, ref box2);
			}
			boxes[num3++] = box;
		}
		num = num3;
		if (num == 1)
		{
			return;
		}
		int num6 = 0;
		bool flag = false;
		while (!flag)
		{
			int num7 = num3;
			for (int j = 0; j < num; j += 2)
			{
				Box3d box3d = Box3d.Merge(ref boxes[num6 + j], ref boxes[num6 + j + 1]);
				boxes[num3++] = box3d;
			}
			num = num / 2 + ((num % 2 != 0) ? 1 : 0);
			num6 = num7;
			if (num == 1)
			{
				flag = true;
			}
		}
	}
}
