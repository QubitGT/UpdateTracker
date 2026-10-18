using System;
using System.Collections.Generic;

namespace g3;

public class PointAABBTree3
{
	public enum BuildStrategy
	{
		Default,
		TopDownMidpoint,
		TopDownMedian
	}

	public class TreeTraversal
	{
		public Func<AxisAlignedBox3d, int, bool> NextBoxF = (AxisAlignedBox3d box, int depth) => true;

		public Action<int> NextPointF = delegate
		{
		};
	}

	private struct FWNInfo
	{
		public Vector3d Center;

		public double R;

		public Vector3d Order1Vec;

		public Matrix3d Order2Mat;
	}

	private class AxisComp : IComparer<Vector3d>
	{
		public int Axis;

		public int Compare(Vector3d a, Vector3d b)
		{
			return a[Axis].CompareTo(b[Axis]);
		}
	}

	private class boxes_set
	{
		public DVector<int> box_to_index = new DVector<int>();

		public DVector<Vector3d> box_centers = new DVector<Vector3d>();

		public DVector<Vector3d> box_extents = new DVector<Vector3d>();

		public DVector<int> index_list = new DVector<int>();

		public int iBoxCur;

		public int iIndicesCur;
	}

	private IPointSet points;

	private int points_timestamp;

	public Func<int, bool> PointFilterF;

	public int LeafMaxPointCount = 32;

	public double FWNBeta = 2.0;

	public int FWNApproxOrder = 2;

	public Func<int, double> FWNAreaEstimateF = (int vid) => 1.0;

	private Dictionary<int, FWNInfo> FastWindingCache;

	private double[] FastWindingAreaCache;

	private int fast_winding_cache_timestamp = -1;

	private DVector<int> box_to_index;

	private DVector<Vector3d> box_centers;

	private DVector<Vector3d> box_extents;

	private DVector<int> index_list;

	private int points_end = -1;

	private int root_index = -1;

	private const double box_eps = 1.1102230246251565E-14;

	public IPointSet Points => points;

	public AxisAlignedBox3d Bounds => get_box(root_index);

	public PointAABBTree3(IPointSet pointsIn, bool autoBuild = true)
	{
		points = pointsIn;
		if (autoBuild)
		{
			Build();
		}
	}

	public void Build(BuildStrategy eStrategy = BuildStrategy.TopDownMidpoint)
	{
		switch (eStrategy)
		{
		case BuildStrategy.TopDownMedian:
			build_top_down(bSorted: true);
			break;
		case BuildStrategy.TopDownMidpoint:
			build_top_down(bSorted: false);
			break;
		case BuildStrategy.Default:
			build_top_down(bSorted: false);
			break;
		}
		points_timestamp = points.Timestamp;
	}

	public virtual int FindNearestPoint(Vector3d p, double fMaxDist = double.MaxValue)
	{
		if (points_timestamp != points.Timestamp)
		{
			throw new Exception("PointAABBTree3.FindNearestPoint: mesh has been modified since tree construction");
		}
		double fNearestSqr = ((fMaxDist < double.MaxValue) ? (fMaxDist * fMaxDist) : double.MaxValue);
		int tID = -1;
		find_nearest_point(root_index, p, ref fNearestSqr, ref tID);
		return tID;
	}

	protected void find_nearest_point(int iBox, Vector3d p, ref double fNearestSqr, ref int tID)
	{
		int num = box_to_index[iBox];
		if (num < points_end)
		{
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				if (PointFilterF == null || PointFilterF(num3))
				{
					double num4 = points.GetVertex(num3).DistanceSquared(p);
					if (num4 < fNearestSqr)
					{
						fNearestSqr = num4;
						tID = num3;
					}
				}
			}
			return;
		}
		int num5 = index_list[num];
		if (num5 < 0)
		{
			num5 = -num5 - 1;
			if (box_distance_sqr(num5, ref p) <= fNearestSqr)
			{
				find_nearest_point(num5, p, ref fNearestSqr, ref tID);
			}
			return;
		}
		num5--;
		int iBox2 = index_list[num + 1] - 1;
		double num6 = box_distance_sqr(num5, ref p);
		double num7 = box_distance_sqr(iBox2, ref p);
		if (num6 < num7)
		{
			if (num6 < fNearestSqr)
			{
				find_nearest_point(num5, p, ref fNearestSqr, ref tID);
				if (num7 < fNearestSqr)
				{
					find_nearest_point(iBox2, p, ref fNearestSqr, ref tID);
				}
			}
		}
		else if (num7 < fNearestSqr)
		{
			find_nearest_point(iBox2, p, ref fNearestSqr, ref tID);
			if (num6 < fNearestSqr)
			{
				find_nearest_point(num5, p, ref fNearestSqr, ref tID);
			}
		}
	}

	public virtual void DoTraversal(TreeTraversal traversal)
	{
		if (points_timestamp != points.Timestamp)
		{
			throw new Exception("PointAABBTree3.FindNearestPoint: mesh has been modified since tree construction");
		}
		tree_traversal(root_index, 0, traversal);
	}

	protected virtual void tree_traversal(int iBox, int depth, TreeTraversal traversal)
	{
		int num = box_to_index[iBox];
		if (num < points_end)
		{
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				if (PointFilterF == null || PointFilterF(num3))
				{
					traversal.NextPointF(num3);
				}
			}
			return;
		}
		int num4 = index_list[num];
		if (num4 < 0)
		{
			num4 = -num4 - 1;
			if (traversal.NextBoxF(get_box(num4), depth + 1))
			{
				tree_traversal(num4, depth + 1, traversal);
			}
			return;
		}
		num4--;
		if (traversal.NextBoxF(get_box(num4), depth + 1))
		{
			tree_traversal(num4, depth + 1, traversal);
		}
		int iBox2 = index_list[num + 1] - 1;
		if (traversal.NextBoxF(get_box(iBox2), depth + 1))
		{
			tree_traversal(iBox2, depth + 1, traversal);
		}
	}

	public virtual double FastWindingNumber(Vector3d p)
	{
		if (points_timestamp != points.Timestamp)
		{
			throw new Exception("PointAABBTree3.FindNearestPoint: mesh has been modified since tree construction");
		}
		if (FastWindingCache == null || fast_winding_cache_timestamp != points.Timestamp)
		{
			build_fast_winding_cache();
			fast_winding_cache_timestamp = points.Timestamp;
		}
		return branch_fast_winding_num(root_index, p);
	}

	protected double branch_fast_winding_num(int iBox, Vector3d p)
	{
		double num = 0.0;
		int num2 = box_to_index[iBox];
		if (num2 < points_end)
		{
			int num3 = index_list[num2];
			for (int i = 1; i <= num3; i++)
			{
				int num4 = index_list[num2 + i];
				Vector3d x = Points.GetVertex(num4);
				Vector3d xn = Points.GetVertexNormal(num4);
				double xA = FastWindingAreaCache[num4];
				num += FastPointWinding.ExactEval(ref x, ref xn, xA, ref p);
			}
		}
		else
		{
			int num5 = index_list[num2];
			if (num5 < 0)
			{
				num5 = -num5 - 1;
				num = ((box_contains(num5, p) || !can_use_fast_winding_cache(num5, ref p)) ? (num + branch_fast_winding_num(num5, p)) : (num + evaluate_box_fast_winding_cache(num5, ref p)));
			}
			else
			{
				num5--;
				int iBox2 = index_list[num2 + 1] - 1;
				num = ((box_contains(num5, p) || !can_use_fast_winding_cache(num5, ref p)) ? (num + branch_fast_winding_num(num5, p)) : (num + evaluate_box_fast_winding_cache(num5, ref p)));
				num = ((box_contains(iBox2, p) || !can_use_fast_winding_cache(iBox2, ref p)) ? (num + branch_fast_winding_num(iBox2, p)) : (num + evaluate_box_fast_winding_cache(iBox2, ref p)));
			}
		}
		return num;
	}

	protected void build_fast_winding_cache()
	{
		int pt_count_thresh = 1;
		FastWindingAreaCache = new double[Points.MaxVertexID];
		foreach (int item in Points.VertexIndices())
		{
			FastWindingAreaCache[item] = FWNAreaEstimateF(item);
		}
		FastWindingCache = new Dictionary<int, FWNInfo>();
		build_fast_winding_cache(root_index, 0, pt_count_thresh, out var _);
	}

	protected int build_fast_winding_cache(int iBox, int depth, int pt_count_thresh, out HashSet<int> pts_hash)
	{
		pts_hash = null;
		int num = box_to_index[iBox];
		if (num < points_end)
		{
			return index_list[num];
		}
		int num2 = index_list[num];
		if (num2 < 0)
		{
			num2 = -num2 - 1;
			return build_fast_winding_cache(num2, depth + 1, pt_count_thresh, out pts_hash);
		}
		num2--;
		int iBox2 = index_list[num + 1] - 1;
		int num3 = build_fast_winding_cache(num2, depth + 1, pt_count_thresh, out pts_hash);
		HashSet<int> pts_hash2;
		int num4 = build_fast_winding_cache(iBox2, depth + 1, pt_count_thresh, out pts_hash2);
		bool flag = num3 + num4 > pt_count_thresh;
		if (depth == 0)
		{
			return num3 + num4;
		}
		if (pts_hash != null || pts_hash2 != null || flag)
		{
			if (pts_hash == null && pts_hash2 != null)
			{
				collect_points(num2, pts_hash2);
				pts_hash = pts_hash2;
			}
			else
			{
				if (pts_hash == null)
				{
					pts_hash = new HashSet<int>();
					collect_points(num2, pts_hash);
				}
				if (pts_hash2 == null)
				{
					collect_points(iBox2, pts_hash);
				}
				else
				{
					pts_hash.UnionWith(pts_hash2);
				}
			}
		}
		if (flag)
		{
			make_box_fast_winding_cache(iBox, pts_hash);
		}
		return num3 + num4;
	}

	protected bool can_use_fast_winding_cache(int iBox, ref Vector3d q)
	{
		if (!FastWindingCache.TryGetValue(iBox, out var value))
		{
			return false;
		}
		if (value.Center.Distance(ref q) > FWNBeta * value.R)
		{
			return true;
		}
		return false;
	}

	protected void make_box_fast_winding_cache(int iBox, IEnumerable<int> pointIndices)
	{
		FWNInfo value = default(FWNInfo);
		FastPointWinding.ComputeCoeffs(points, pointIndices, FastWindingAreaCache, ref value.Center, ref value.R, ref value.Order1Vec, ref value.Order2Mat);
		FastWindingCache[iBox] = value;
	}

	protected double evaluate_box_fast_winding_cache(int iBox, ref Vector3d q)
	{
		FWNInfo fWNInfo = FastWindingCache[iBox];
		if (FWNApproxOrder == 2)
		{
			return FastPointWinding.EvaluateOrder2Approx(ref fWNInfo.Center, ref fWNInfo.Order1Vec, ref fWNInfo.Order2Mat, ref q);
		}
		return FastPointWinding.EvaluateOrder1Approx(ref fWNInfo.Center, ref fWNInfo.Order1Vec, ref q);
	}

	protected void collect_points(int iBox, HashSet<int> points)
	{
		int num = box_to_index[iBox];
		if (num < points_end)
		{
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				points.Add(index_list[num + i]);
			}
			return;
		}
		int num3 = index_list[num];
		if (num3 < 0)
		{
			collect_points(-num3 - 1, points);
			return;
		}
		collect_points(num3 - 1, points);
		collect_points(index_list[num + 1] - 1, points);
	}

	public double TotalVolume()
	{
		double volSum = 0.0;
		TreeTraversal traversal = new TreeTraversal
		{
			NextBoxF = delegate(AxisAlignedBox3d box, int depth)
			{
				volSum += box.Volume;
				return true;
			}
		};
		DoTraversal(traversal);
		return volSum;
	}

	public double TotalExtentSum()
	{
		double extSum = 0.0;
		TreeTraversal traversal = new TreeTraversal
		{
			NextBoxF = delegate(AxisAlignedBox3d box, int depth)
			{
				extSum += box.Extents.LengthL1;
				return true;
			}
		};
		DoTraversal(traversal);
		return extSum;
	}

	private void build_top_down(bool bSorted)
	{
		int num = 0;
		int vertexCount = points.VertexCount;
		int[] array = new int[vertexCount];
		Vector3d[] array2 = new Vector3d[vertexCount];
		foreach (int item in points.VertexIndices())
		{
			Vector3d vertex = points.GetVertex(item);
			double lengthSquared = vertex.LengthSquared;
			if (!double.IsNaN(lengthSquared) && !double.IsInfinity(lengthSquared))
			{
				array[num] = item;
				array2[num] = vertex;
				num++;
			}
		}
		boxes_set boxes_set = new boxes_set();
		boxes_set boxes_set2 = new boxes_set();
		AxisAlignedBox3d box;
		int num2 = (bSorted ? split_point_set_sorted(array, array2, 0, vertexCount, 0, LeafMaxPointCount, boxes_set, boxes_set2, out box) : split_point_set_midpoint(array, array2, 0, vertexCount, 0, LeafMaxPointCount, boxes_set, boxes_set2, out box));
		box_to_index = boxes_set.box_to_index;
		box_centers = boxes_set.box_centers;
		box_extents = boxes_set.box_extents;
		index_list = boxes_set.index_list;
		points_end = boxes_set.iIndicesCur;
		int num3 = points_end;
		int iBoxCur = boxes_set.iBoxCur;
		for (num = 0; num < boxes_set2.iBoxCur; num++)
		{
			box_centers.insert(boxes_set2.box_centers[num], iBoxCur + num);
			box_extents.insert(boxes_set2.box_extents[num], iBoxCur + num);
			box_to_index.insert(num3 + boxes_set2.box_to_index[num], iBoxCur + num);
		}
		for (num = 0; num < boxes_set2.iIndicesCur; num++)
		{
			int num4 = boxes_set2.index_list[num];
			num4 = ((num4 >= 0) ? (num4 + iBoxCur) : (-num4 - 1));
			num4++;
			index_list.insert(num4, num3 + num);
		}
		root_index = num2 + iBoxCur;
	}

	private int split_point_set_sorted(int[] pt_indices, Vector3d[] positions, int iStart, int iCount, int depth, int minIndexCount, boxes_set leafs, boxes_set nodes, out AxisAlignedBox3d box)
	{
		box = AxisAlignedBox3d.Empty;
		int num = -1;
		if (iCount < minIndexCount)
		{
			num = leafs.iBoxCur++;
			leafs.box_to_index.insert(leafs.iIndicesCur, num);
			leafs.index_list.insert(iCount, leafs.iIndicesCur++);
			for (int i = 0; i < iCount; i++)
			{
				leafs.index_list.insert(pt_indices[iStart + i], leafs.iIndicesCur++);
				box.Contain(points.GetVertex(pt_indices[iStart + i]));
			}
			leafs.box_centers.insert(box.Center, num);
			leafs.box_extents.insert(box.Extents, num);
			return -(num + 1);
		}
		AxisComp comparer = new AxisComp
		{
			Axis = depth % 3
		};
		Array.Sort(positions, pt_indices, iStart, iCount, comparer);
		int num2 = iCount / 2;
		int iCount2 = num2;
		int iCount3 = iCount - num2;
		int value = split_point_set_sorted(pt_indices, positions, iStart, iCount2, depth + 1, minIndexCount, leafs, nodes, out box);
		AxisAlignedBox3d box2;
		int value2 = split_point_set_sorted(pt_indices, positions, iStart + num2, iCount3, depth + 1, minIndexCount, leafs, nodes, out box2);
		box.Contain(box2);
		num = nodes.iBoxCur++;
		nodes.box_to_index.insert(nodes.iIndicesCur, num);
		nodes.index_list.insert(value, nodes.iIndicesCur++);
		nodes.index_list.insert(value2, nodes.iIndicesCur++);
		nodes.box_centers.insert(box.Center, num);
		nodes.box_extents.insert(box.Extents, num);
		return num;
	}

	private int split_point_set_midpoint(int[] pt_indices, Vector3d[] positions, int iStart, int iCount, int depth, int minIndexCount, boxes_set leafs, boxes_set nodes, out AxisAlignedBox3d box)
	{
		box = AxisAlignedBox3d.Empty;
		int num = -1;
		if (iCount < minIndexCount)
		{
			num = leafs.iBoxCur++;
			leafs.box_to_index.insert(leafs.iIndicesCur, num);
			leafs.index_list.insert(iCount, leafs.iIndicesCur++);
			for (int i = 0; i < iCount; i++)
			{
				leafs.index_list.insert(pt_indices[iStart + i], leafs.iIndicesCur++);
				box.Contain(points.GetVertex(pt_indices[iStart + i]));
			}
			leafs.box_centers.insert(box.Center, num);
			leafs.box_extents.insert(box.Extents, num);
			return -(num + 1);
		}
		int key = depth % 3;
		Interval1d empty = Interval1d.Empty;
		for (int j = 0; j < iCount; j++)
		{
			empty.Contain(positions[iStart + j][key]);
		}
		double center = empty.Center;
		int num4;
		int iCount2;
		if (Math.Abs(empty.a - empty.b) > 1E-08)
		{
			int k = 0;
			int num2 = iCount - 1;
			while (k < num2)
			{
				for (; positions[iStart + k][key] <= center; k++)
				{
				}
				while (positions[iStart + num2][key] > center)
				{
					num2--;
				}
				if (k >= num2)
				{
					break;
				}
				Vector3d vector3d = positions[iStart + k];
				positions[iStart + k] = positions[iStart + num2];
				positions[iStart + num2] = vector3d;
				int num3 = pt_indices[iStart + k];
				pt_indices[iStart + k] = pt_indices[iStart + num2];
				pt_indices[iStart + num2] = num3;
			}
			num4 = k;
			iCount2 = iCount - num4;
		}
		else
		{
			num4 = iCount / 2;
			iCount2 = iCount - num4;
		}
		int value = split_point_set_midpoint(pt_indices, positions, iStart, num4, depth + 1, minIndexCount, leafs, nodes, out box);
		AxisAlignedBox3d box2;
		int value2 = split_point_set_midpoint(pt_indices, positions, iStart + num4, iCount2, depth + 1, minIndexCount, leafs, nodes, out box2);
		box.Contain(box2);
		num = nodes.iBoxCur++;
		nodes.box_to_index.insert(nodes.iIndicesCur, num);
		nodes.index_list.insert(value, nodes.iIndicesCur++);
		nodes.index_list.insert(value2, nodes.iIndicesCur++);
		nodes.box_centers.insert(box.Center, num);
		nodes.box_extents.insert(box.Extents, num);
		return num;
	}

	private AxisAlignedBox3d get_box(int iBox)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3d vector3d = box_extents[iBox];
		return new AxisAlignedBox3d(ref vCenter, vector3d.x, vector3d.y, vector3d.z);
	}

	private double box_distance_sqr(int iBox, ref Vector3d p)
	{
		Vector3d vector3d = box_centers[iBox];
		Vector3d vector3d2 = box_extents[iBox];
		double num = Math.Abs(p.x - vector3d.x);
		num = ((num < vector3d2.x) ? 0.0 : (num - vector3d2.x));
		double num2 = Math.Abs(p.y - vector3d.y);
		num2 = ((num2 < vector3d2.y) ? 0.0 : (num2 - vector3d2.y));
		double num3 = Math.Abs(p.z - vector3d.z);
		num3 = ((num3 < vector3d2.z) ? 0.0 : (num3 - vector3d2.z));
		return num * num + num2 * num2 + num3 * num3;
	}

	protected bool box_contains(int iBox, Vector3d p)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3d vector3d = box_extents[iBox];
		return new AxisAlignedBox3d(ref vCenter, vector3d.x + 1.1102230246251565E-14, vector3d.y + 1.1102230246251565E-14, vector3d.z + 1.1102230246251565E-14).Contains(p);
	}

	public void TestCoverage()
	{
		int[] array = new int[points.MaxVertexID];
		Array.Clear(array, 0, array.Length);
		int[] array2 = new int[box_to_index.Length];
		Array.Clear(array2, 0, array2.Length);
		test_coverage(array, array2, root_index);
		foreach (int item in points.VertexIndices())
		{
			if (array[item] != 1)
			{
				Util.gBreakToDebugger();
			}
		}
	}

	private void test_coverage(int[] point_counts, int[] parent_indices, int iBox)
	{
		int num = box_to_index[iBox];
		debug_check_child_points_in_box(iBox);
		if (num < points_end)
		{
			int num2 = index_list[num];
			AxisAlignedBox3d axisAlignedBox3d = get_box(iBox);
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				point_counts[num3]++;
				Vector3d vertex = points.GetVertex(num3);
				if (!axisAlignedBox3d.Contains(vertex))
				{
					Util.gBreakToDebugger();
				}
			}
			return;
		}
		int num4 = index_list[num];
		if (num4 < 0)
		{
			num4 = -num4 - 1;
			parent_indices[num4] = iBox;
			test_coverage(point_counts, parent_indices, num4);
			return;
		}
		num4--;
		parent_indices[num4] = iBox;
		test_coverage(point_counts, parent_indices, num4);
		int num5 = index_list[num + 1];
		num5--;
		parent_indices[num5] = iBox;
		test_coverage(point_counts, parent_indices, num5);
	}

	private void debug_check_child_point_distances(int iBox, Vector3d p)
	{
		double fBoxDistSqr = box_distance_sqr(iBox, ref p);
		TreeTraversal traversal = new TreeTraversal
		{
			NextPointF = delegate(int vID)
			{
				Vector3d vertex = points.GetVertex(vID);
				double num = p.DistanceSquared(vertex);
				if (num < fBoxDistSqr && Math.Abs(num - fBoxDistSqr) > 1E-06)
				{
					Util.gBreakToDebugger();
				}
			}
		};
		tree_traversal(iBox, 0, traversal);
	}

	private void debug_check_child_points_in_box(int iBox)
	{
		AxisAlignedBox3d box = get_box(iBox);
		TreeTraversal traversal = new TreeTraversal
		{
			NextPointF = delegate(int vID)
			{
				Vector3d vertex = points.GetVertex(vID);
				if (!box.Contains(vertex))
				{
					Util.gBreakToDebugger();
				}
			}
		};
		tree_traversal(iBox, 0, traversal);
	}
}
