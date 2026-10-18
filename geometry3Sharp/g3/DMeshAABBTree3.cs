using System;
using System.Collections.Generic;

namespace g3;

public class DMeshAABBTree3 : ISpatial
{
	public enum BuildStrategy
	{
		Default,
		TopDownMidpoint,
		BottomUpFromOneRings,
		TopDownMedian
	}

	public enum ClusterPolicy
	{
		Default,
		Fastest,
		FastVolumeMetric,
		MinimalVolume
	}

	public struct PointIntersection
	{
		public int t0;

		public int t1;

		public Vector3d point;
	}

	public struct SegmentIntersection
	{
		public int t0;

		public int t1;

		public Vector3d point0;

		public Vector3d point1;
	}

	public class IntersectionsQueryResult
	{
		public List<PointIntersection> Points;

		public List<SegmentIntersection> Segments;
	}

	public class TreeTraversal
	{
		public Func<AxisAlignedBox3f, int, bool> NextBoxF = (AxisAlignedBox3f box, int depth) => true;

		public Action<int> NextTriangleF = delegate
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

		public DVector<Vector3f> box_centers = new DVector<Vector3f>();

		public DVector<Vector3f> box_extents = new DVector<Vector3f>();

		public DVector<int> index_list = new DVector<int>();

		public int iBoxCur;

		public int iIndicesCur;
	}

	public delegate int ClusterFunctionType(int iStart, int iCount, ref int iBoxCur, ref int iIndicesCur);

	protected DMesh3 mesh;

	protected int mesh_timestamp;

	public Func<int, bool> TriangleFilterF;

	public int TopDownLeafMaxTriCount = 4;

	public int BottomUpClusterLookahead = 10;

	private Dictionary<int, List<int>> WindingCache;

	private int winding_cache_timestamp = -1;

	public double FWNBeta = 2.0;

	public int FWNApproxOrder = 2;

	private Dictionary<int, FWNInfo> FastWindingCache;

	private int fast_winding_cache_timestamp = -1;

	protected DVector<int> box_to_index;

	protected DVector<Vector3f> box_centers;

	protected DVector<Vector3f> box_extents;

	protected DVector<int> index_list;

	protected int triangles_end = -1;

	protected int root_index = -1;

	private const float box_eps = 5.9604645E-06f;

	public DMesh3 Mesh => mesh;

	public bool IsValid => mesh_timestamp == mesh.ShapeTimestamp;

	public bool SupportsNearestTriangle => true;

	public bool SupportsTriangleRayIntersection => true;

	public bool SupportsPointContainment => true;

	public AxisAlignedBox3d Bounds => get_box(root_index);

	public DMeshAABBTree3(DMesh3 m, bool autoBuild = false)
	{
		mesh = m;
		if (autoBuild)
		{
			Build();
		}
	}

	public void Build(BuildStrategy eStrategy = BuildStrategy.TopDownMidpoint, ClusterPolicy ePolicy = ClusterPolicy.Default)
	{
		switch (eStrategy)
		{
		case BuildStrategy.BottomUpFromOneRings:
			build_by_one_rings(ePolicy);
			break;
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
		mesh_timestamp = mesh.ShapeTimestamp;
	}

	public virtual int FindNearestTriangle(Vector3d p, double fMaxDist = double.MaxValue)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.FindNearestTriangle: mesh has been modified since tree construction");
		}
		double fNearestSqr = ((fMaxDist < double.MaxValue) ? (fMaxDist * fMaxDist) : double.MaxValue);
		int tID = -1;
		find_nearest_tri(root_index, p, ref fNearestSqr, ref tID);
		return tID;
	}

	public virtual int FindNearestTriangle(Vector3d p, out double fNearestDistSqr, double fMaxDist = double.MaxValue)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.FindNearestTriangle: mesh has been modified since tree construction");
		}
		fNearestDistSqr = ((fMaxDist < double.MaxValue) ? (fMaxDist * fMaxDist) : double.MaxValue);
		int tID = -1;
		find_nearest_tri(root_index, p, ref fNearestDistSqr, ref tID);
		return tID;
	}

	protected void find_nearest_tri(int iBox, Vector3d p, ref double fNearestSqr, ref int tID)
	{
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				if (TriangleFilterF == null || TriangleFilterF(num3))
				{
					double num4 = MeshQueries.TriDistanceSqr(mesh, num3, p);
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
			if (box_distance_sqr(num5, p) <= fNearestSqr)
			{
				find_nearest_tri(num5, p, ref fNearestSqr, ref tID);
			}
			return;
		}
		num5--;
		int iBox2 = index_list[num + 1] - 1;
		double num6 = box_distance_sqr(num5, p);
		double num7 = box_distance_sqr(iBox2, p);
		if (num6 < num7)
		{
			if (num6 < fNearestSqr)
			{
				find_nearest_tri(num5, p, ref fNearestSqr, ref tID);
				if (num7 < fNearestSqr)
				{
					find_nearest_tri(iBox2, p, ref fNearestSqr, ref tID);
				}
			}
		}
		else if (num7 < fNearestSqr)
		{
			find_nearest_tri(iBox2, p, ref fNearestSqr, ref tID);
			if (num6 < fNearestSqr)
			{
				find_nearest_tri(num5, p, ref fNearestSqr, ref tID);
			}
		}
	}

	public virtual int FindNearestVertex(Vector3d p, double fMaxDist = double.MaxValue)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.FindNearestVertex: mesh has been modified since tree construction");
		}
		double fNearestSqr = ((fMaxDist < double.MaxValue) ? (fMaxDist * fMaxDist) : double.MaxValue);
		int vid = -1;
		find_nearest_vtx(root_index, p, ref fNearestSqr, ref vid);
		return vid;
	}

	protected void find_nearest_vtx(int iBox, Vector3d p, ref double fNearestSqr, ref int vid)
	{
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				if (TriangleFilterF != null && !TriangleFilterF(num3))
				{
					continue;
				}
				Vector3i vector3i = mesh.GetTriangle(num3);
				for (int j = 0; j < 3; j++)
				{
					double num4 = mesh.GetVertex(vector3i[j]).DistanceSquared(ref p);
					if (num4 < fNearestSqr)
					{
						fNearestSqr = num4;
						vid = vector3i[j];
					}
				}
			}
			return;
		}
		int num5 = index_list[num];
		if (num5 < 0)
		{
			num5 = -num5 - 1;
			if (box_distance_sqr(num5, p) <= fNearestSqr)
			{
				find_nearest_vtx(num5, p, ref fNearestSqr, ref vid);
			}
			return;
		}
		num5--;
		int iBox2 = index_list[num + 1] - 1;
		double num6 = box_distance_sqr(num5, p);
		double num7 = box_distance_sqr(iBox2, p);
		if (num6 < num7)
		{
			if (num6 < fNearestSqr)
			{
				find_nearest_vtx(num5, p, ref fNearestSqr, ref vid);
				if (num7 < fNearestSqr)
				{
					find_nearest_vtx(iBox2, p, ref fNearestSqr, ref vid);
				}
			}
		}
		else if (num7 < fNearestSqr)
		{
			find_nearest_vtx(iBox2, p, ref fNearestSqr, ref vid);
			if (num6 < fNearestSqr)
			{
				find_nearest_vtx(num5, p, ref fNearestSqr, ref vid);
			}
		}
	}

	public virtual int FindNearestHitTriangle(Ray3d ray, double fMaxDist = double.MaxValue)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.FindNearestHitTriangle: mesh has been modified since tree construction");
		}
		if (!ray.Direction.IsNormalized)
		{
			throw new Exception("DMeshAABBTree3.FindNearestHitTriangle: ray direction is not normalized");
		}
		double fNearestT = ((fMaxDist < double.MaxValue) ? fMaxDist : 3.4028234663852886E+38);
		int tID = -1;
		find_hit_triangle(root_index, ref ray, ref fNearestT, ref tID);
		return tID;
	}

	protected void find_hit_triangle(int iBox, ref Ray3d ray, ref double fNearestT, ref int tID)
	{
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			Triangle3d triangle3d = default(Triangle3d);
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				if (TriangleFilterF == null || TriangleFilterF(num3))
				{
					mesh.GetTriVertices(num3, ref triangle3d.V0, ref triangle3d.V1, ref triangle3d.V2);
					if (IntrRay3Triangle3.Intersects(ref ray, ref triangle3d.V0, ref triangle3d.V1, ref triangle3d.V2, out var rayT) && rayT < fNearestT)
					{
						fNearestT = rayT;
						tID = num3;
					}
				}
			}
			return;
		}
		double num4 = 9.999999974752427E-07;
		int num5 = index_list[num];
		if (num5 < 0)
		{
			num5 = -num5 - 1;
			if (box_ray_intersect_t(num5, ray) <= fNearestT + num4)
			{
				find_hit_triangle(num5, ref ray, ref fNearestT, ref tID);
			}
			return;
		}
		num5--;
		int iBox2 = index_list[num + 1] - 1;
		double num6 = box_ray_intersect_t(num5, ray);
		double num7 = box_ray_intersect_t(iBox2, ray);
		if (num6 < num7)
		{
			if (num6 <= fNearestT + num4)
			{
				find_hit_triangle(num5, ref ray, ref fNearestT, ref tID);
				if (num7 <= fNearestT + num4)
				{
					find_hit_triangle(iBox2, ref ray, ref fNearestT, ref tID);
				}
			}
		}
		else if (num7 <= fNearestT + num4)
		{
			find_hit_triangle(iBox2, ref ray, ref fNearestT, ref tID);
			if (num6 <= fNearestT + num4)
			{
				find_hit_triangle(num5, ref ray, ref fNearestT, ref tID);
			}
		}
	}

	public virtual int FindAllHitTriangles(Ray3d ray, List<int> hitTriangles = null, double fMaxDist = double.MaxValue)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.FindNearestHitTriangle: mesh has been modified since tree construction");
		}
		if (!ray.Direction.IsNormalized)
		{
			throw new Exception("DMeshAABBTree3.FindNearestHitTriangle: ray direction is not normalized");
		}
		double fMaxDist2 = ((fMaxDist < double.MaxValue) ? fMaxDist : 3.4028234663852886E+38);
		return find_all_hit_triangles(root_index, hitTriangles, ref ray, fMaxDist2);
	}

	protected int find_all_hit_triangles(int iBox, List<int> hitTriangles, ref Ray3d ray, double fMaxDist)
	{
		int num = 0;
		int num2 = box_to_index[iBox];
		if (num2 < triangles_end)
		{
			Triangle3d triangle3d = default(Triangle3d);
			int num3 = index_list[num2];
			for (int i = 1; i <= num3; i++)
			{
				int num4 = index_list[num2 + i];
				if (TriangleFilterF == null || TriangleFilterF(num4))
				{
					mesh.GetTriVertices(num4, ref triangle3d.V0, ref triangle3d.V1, ref triangle3d.V2);
					if (IntrRay3Triangle3.Intersects(ref ray, ref triangle3d.V0, ref triangle3d.V1, ref triangle3d.V2, out var rayT) && rayT < fMaxDist)
					{
						hitTriangles?.Add(num4);
						num++;
					}
				}
			}
		}
		else
		{
			double num5 = 9.999999974752427E-07;
			int num6 = index_list[num2];
			if (num6 < 0)
			{
				num6 = -num6 - 1;
				if (box_ray_intersect_t(num6, ray) <= fMaxDist + num5)
				{
					num += find_all_hit_triangles(num6, hitTriangles, ref ray, fMaxDist);
				}
			}
			else
			{
				num6--;
				int iBox2 = index_list[num2 + 1] - 1;
				if (box_ray_intersect_t(num6, ray) <= fMaxDist + num5)
				{
					num += find_all_hit_triangles(num6, hitTriangles, ref ray, fMaxDist);
				}
				if (box_ray_intersect_t(iBox2, ray) <= fMaxDist + num5)
				{
					num += find_all_hit_triangles(iBox2, hitTriangles, ref ray, fMaxDist);
				}
			}
		}
		return num;
	}

	public virtual bool TestIntersection(IMesh testMesh, Func<Vector3d, Vector3d> TransformF = null, bool bBoundsCheck = true)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.TestIntersection: mesh has been modified since tree construction");
		}
		if (bBoundsCheck)
		{
			AxisAlignedBox3d testBox = MeshMeasurements.Bounds(testMesh, TransformF);
			if (!box_box_intersect(root_index, ref testBox))
			{
				return false;
			}
		}
		if (TransformF == null)
		{
			TransformF = (Vector3d x) => x;
		}
		Triangle3d triangle = default(Triangle3d);
		foreach (int item in testMesh.TriangleIndices())
		{
			Index3i triangle2 = testMesh.GetTriangle(item);
			triangle.V0 = TransformF(testMesh.GetVertex(triangle2.a));
			triangle.V1 = TransformF(testMesh.GetVertex(triangle2.b));
			triangle.V2 = TransformF(testMesh.GetVertex(triangle2.c));
			if (TestIntersection(triangle))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool TestIntersection(Triangle3d triangle)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.TestIntersection: mesh has been modified since tree construction");
		}
		AxisAlignedBox3d triBounds = BoundsUtil.Bounds(ref triangle);
		return find_any_intersection(root_index, ref triangle, ref triBounds) >= 0;
	}

	protected int find_any_intersection(int iBox, ref Triangle3d triangle, ref AxisAlignedBox3d triBounds)
	{
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			Triangle3d triangle2 = default(Triangle3d);
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				if (TriangleFilterF == null || TriangleFilterF(num3))
				{
					mesh.GetTriVertices(num3, ref triangle2.V0, ref triangle2.V1, ref triangle2.V2);
					if (IntrTriangle3Triangle3.Intersects(ref triangle, ref triangle2))
					{
						return num3;
					}
				}
			}
		}
		else
		{
			int num4 = index_list[num];
			if (num4 >= 0)
			{
				num4--;
				int iBox2 = index_list[num + 1] - 1;
				int num5 = -1;
				if (box_box_intersect(num4, ref triBounds))
				{
					num5 = find_any_intersection(num4, ref triangle, ref triBounds);
				}
				if (num5 == -1 && box_box_intersect(iBox2, ref triBounds))
				{
					num5 = find_any_intersection(iBox2, ref triangle, ref triBounds);
				}
				return num5;
			}
			num4 = -num4 - 1;
			if (box_box_intersect(num4, ref triBounds))
			{
				return find_any_intersection(num4, ref triangle, ref triBounds);
			}
		}
		return -1;
	}

	public virtual bool TestIntersection(DMeshAABBTree3 otherTree, Func<Vector3d, Vector3d> TransformF = null)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.TestIntersection: mesh has been modified since tree construction");
		}
		if (find_any_intersection(root_index, otherTree, TransformF, otherTree.root_index, 0))
		{
			return true;
		}
		return false;
	}

	protected bool find_any_intersection(int iBox, DMeshAABBTree3 otherTree, Func<Vector3d, Vector3d> TransformF, int oBox, int depth)
	{
		int num = box_to_index[iBox];
		int num2 = otherTree.box_to_index[oBox];
		if (num < triangles_end && num2 < otherTree.triangles_end)
		{
			Triangle3d triangle = default(Triangle3d);
			Triangle3d triangle2 = default(Triangle3d);
			int num3 = index_list[num];
			int num4 = otherTree.index_list[num2];
			for (int i = 1; i <= num4; i++)
			{
				int num5 = otherTree.index_list[num2 + i];
				if (otherTree.TriangleFilterF != null && !otherTree.TriangleFilterF(num5))
				{
					continue;
				}
				otherTree.mesh.GetTriVertices(num5, ref triangle2.V0, ref triangle2.V1, ref triangle2.V2);
				if (TransformF != null)
				{
					triangle2.V0 = TransformF(triangle2.V0);
					triangle2.V1 = TransformF(triangle2.V1);
					triangle2.V2 = TransformF(triangle2.V2);
				}
				for (int j = 1; j <= num3; j++)
				{
					int num6 = index_list[num + j];
					if (TriangleFilterF == null || TriangleFilterF(num6))
					{
						mesh.GetTriVertices(num6, ref triangle.V0, ref triangle.V1, ref triangle.V2);
						if (IntrTriangle3Triangle3.Intersects(ref triangle2, ref triangle))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		bool flag = num < triangles_end || depth % 2 == 0;
		if (flag && num2 < otherTree.triangles_end)
		{
			flag = false;
		}
		if (flag)
		{
			AxisAlignedBox3d box = get_boxd(iBox);
			int num7 = otherTree.index_list[num2];
			if (num7 >= 0)
			{
				num7--;
				int num8 = otherTree.index_list[num2 + 1] - 1;
				bool flag2 = false;
				if (otherTree.get_boxd(num7, TransformF).Intersects(box))
				{
					flag2 = find_any_intersection(iBox, otherTree, TransformF, num7, depth + 1);
				}
				if (!flag2 && otherTree.get_boxd(num8, TransformF).Intersects(box))
				{
					flag2 = find_any_intersection(iBox, otherTree, TransformF, num8, depth + 1);
				}
				return flag2;
			}
			num7 = -num7 - 1;
			if (otherTree.get_boxd(num7, TransformF).Intersects(box))
			{
				return find_any_intersection(iBox, otherTree, TransformF, oBox, depth + 1);
			}
		}
		else
		{
			AxisAlignedBox3d testBox = otherTree.get_boxd(oBox, TransformF);
			int num9 = index_list[num];
			if (num9 >= 0)
			{
				num9--;
				int iBox2 = index_list[num + 1] - 1;
				bool flag3 = false;
				if (box_box_intersect(num9, ref testBox))
				{
					flag3 = find_any_intersection(num9, otherTree, TransformF, oBox, depth + 1);
				}
				if (!flag3 && box_box_intersect(iBox2, ref testBox))
				{
					flag3 = find_any_intersection(iBox2, otherTree, TransformF, oBox, depth + 1);
				}
				return flag3;
			}
			num9 = -num9 - 1;
			if (box_box_intersect(num9, ref testBox))
			{
				return find_any_intersection(num9, otherTree, TransformF, oBox, depth + 1);
			}
		}
		return false;
	}

	public virtual IntersectionsQueryResult FindAllIntersections(DMeshAABBTree3 otherTree, Func<Vector3d, Vector3d> TransformF = null)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.FindIntersections: mesh has been modified since tree construction");
		}
		IntersectionsQueryResult intersectionsQueryResult = new IntersectionsQueryResult();
		intersectionsQueryResult.Points = new List<PointIntersection>();
		intersectionsQueryResult.Segments = new List<SegmentIntersection>();
		IntrTriangle3Triangle3 intr = new IntrTriangle3Triangle3(default(Triangle3d), default(Triangle3d));
		find_intersections(root_index, otherTree, TransformF, otherTree.root_index, 0, intr, intersectionsQueryResult);
		return intersectionsQueryResult;
	}

	protected void find_intersections(int iBox, DMeshAABBTree3 otherTree, Func<Vector3d, Vector3d> TransformF, int oBox, int depth, IntrTriangle3Triangle3 intr, IntersectionsQueryResult result)
	{
		int num = box_to_index[iBox];
		int num2 = otherTree.box_to_index[oBox];
		if (num < triangles_end && num2 < otherTree.triangles_end)
		{
			Triangle3d triangle = default(Triangle3d);
			Triangle3d triangle2 = default(Triangle3d);
			int num3 = index_list[num];
			int num4 = otherTree.index_list[num2];
			for (int i = 1; i <= num4; i++)
			{
				int num5 = otherTree.index_list[num2 + i];
				if (otherTree.TriangleFilterF != null && !otherTree.TriangleFilterF(num5))
				{
					continue;
				}
				otherTree.mesh.GetTriVertices(num5, ref triangle2.V0, ref triangle2.V1, ref triangle2.V2);
				if (TransformF != null)
				{
					triangle2.V0 = TransformF(triangle2.V0);
					triangle2.V1 = TransformF(triangle2.V1);
					triangle2.V2 = TransformF(triangle2.V2);
				}
				intr.Triangle0 = triangle2;
				for (int j = 1; j <= num3; j++)
				{
					int num6 = index_list[num + j];
					if (TriangleFilterF != null && !TriangleFilterF(num6))
					{
						continue;
					}
					mesh.GetTriVertices(num6, ref triangle.V0, ref triangle.V1, ref triangle.V2);
					intr.Triangle1 = triangle;
					if (!intr.Test() || !intr.Find())
					{
						continue;
					}
					if (intr.Quantity == 1)
					{
						result.Points.Add(new PointIntersection
						{
							t0 = num6,
							t1 = num5,
							point = intr.Points[0]
						});
						continue;
					}
					if (intr.Quantity == 2)
					{
						result.Segments.Add(new SegmentIntersection
						{
							t0 = num6,
							t1 = num5,
							point0 = intr.Points[0],
							point1 = intr.Points[1]
						});
						continue;
					}
					throw new Exception("DMeshAABBTree.find_intersections: found quantity " + intr.Quantity);
				}
			}
			return;
		}
		bool flag = num < triangles_end || depth % 2 == 0;
		if (flag && num2 < otherTree.triangles_end)
		{
			flag = false;
		}
		if (flag)
		{
			AxisAlignedBox3d box = get_boxd(iBox);
			int num7 = otherTree.index_list[num2];
			if (num7 < 0)
			{
				num7 = -num7 - 1;
				if (otherTree.get_boxd(num7, TransformF).Intersects(box))
				{
					find_intersections(iBox, otherTree, TransformF, num7, depth + 1, intr, result);
				}
				return;
			}
			num7--;
			if (otherTree.get_boxd(num7, TransformF).Intersects(box))
			{
				find_intersections(iBox, otherTree, TransformF, num7, depth + 1, intr, result);
			}
			int num8 = otherTree.index_list[num2 + 1] - 1;
			if (otherTree.get_boxd(num8, TransformF).Intersects(box))
			{
				find_intersections(iBox, otherTree, TransformF, num8, depth + 1, intr, result);
			}
			return;
		}
		AxisAlignedBox3d testBox = otherTree.get_boxd(oBox, TransformF);
		int num9 = index_list[num];
		if (num9 < 0)
		{
			num9 = -num9 - 1;
			if (box_box_intersect(num9, ref testBox))
			{
				find_intersections(num9, otherTree, TransformF, oBox, depth + 1, intr, result);
			}
			return;
		}
		num9--;
		if (box_box_intersect(num9, ref testBox))
		{
			find_intersections(num9, otherTree, TransformF, oBox, depth + 1, intr, result);
		}
		int iBox2 = index_list[num + 1] - 1;
		if (box_box_intersect(iBox2, ref testBox))
		{
			find_intersections(iBox2, otherTree, TransformF, oBox, depth + 1, intr, result);
		}
	}

	public virtual Index2i FindNearestTriangles(DMeshAABBTree3 otherTree, Func<Vector3d, Vector3d> TransformF, out double distance, double max_dist = double.MaxValue)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.TestIntersection: mesh has been modified since tree construction");
		}
		double nearest_sqr = double.MaxValue;
		if (max_dist < double.MaxValue)
		{
			nearest_sqr = max_dist * max_dist;
		}
		Index2i nearest_pair = Index2i.Max;
		find_nearest_triangles(root_index, otherTree, TransformF, otherTree.root_index, 0, ref nearest_sqr, ref nearest_pair);
		distance = ((nearest_sqr < double.MaxValue) ? Math.Sqrt(nearest_sqr) : double.MaxValue);
		return nearest_pair;
	}

	protected void find_nearest_triangles(int iBox, DMeshAABBTree3 otherTree, Func<Vector3d, Vector3d> TransformF, int oBox, int depth, ref double nearest_sqr, ref Index2i nearest_pair)
	{
		int num = box_to_index[iBox];
		int num2 = otherTree.box_to_index[oBox];
		if (num < triangles_end && num2 < otherTree.triangles_end)
		{
			Triangle3d triangle = default(Triangle3d);
			Triangle3d triangle2 = default(Triangle3d);
			int num3 = index_list[num];
			int num4 = otherTree.index_list[num2];
			DistTriangle3Triangle3 distTriangle3Triangle = new DistTriangle3Triangle3(default(Triangle3d), default(Triangle3d));
			for (int i = 1; i <= num4; i++)
			{
				int num5 = otherTree.index_list[num2 + i];
				if (otherTree.TriangleFilterF != null && !otherTree.TriangleFilterF(num5))
				{
					continue;
				}
				otherTree.mesh.GetTriVertices(num5, ref triangle2.V0, ref triangle2.V1, ref triangle2.V2);
				if (TransformF != null)
				{
					triangle2.V0 = TransformF(triangle2.V0);
					triangle2.V1 = TransformF(triangle2.V1);
					triangle2.V2 = TransformF(triangle2.V2);
				}
				distTriangle3Triangle.Triangle0 = triangle2;
				for (int j = 1; j <= num3; j++)
				{
					int num6 = index_list[num + j];
					if (TriangleFilterF == null || TriangleFilterF(num6))
					{
						mesh.GetTriVertices(num6, ref triangle.V0, ref triangle.V1, ref triangle.V2);
						distTriangle3Triangle.Triangle1 = triangle;
						double squared = distTriangle3Triangle.GetSquared();
						if (squared < nearest_sqr)
						{
							nearest_sqr = squared;
							nearest_pair = new Index2i(num6, num5);
						}
					}
				}
			}
			return;
		}
		bool flag = num < triangles_end || depth % 2 == 0;
		if (flag && num2 < otherTree.triangles_end)
		{
			flag = false;
		}
		if (flag)
		{
			AxisAlignedBox3d box = get_boxd(iBox);
			int num7 = otherTree.index_list[num2];
			if (num7 < 0)
			{
				num7 = -num7 - 1;
				if (otherTree.get_boxd(num7, TransformF).DistanceSquared(ref box) < nearest_sqr)
				{
					find_nearest_triangles(iBox, otherTree, TransformF, num7, depth + 1, ref nearest_sqr, ref nearest_pair);
				}
				return;
			}
			num7--;
			int num8 = otherTree.index_list[num2 + 1] - 1;
			AxisAlignedBox3d axisAlignedBox3d = otherTree.get_boxd(num7, TransformF);
			AxisAlignedBox3d axisAlignedBox3d2 = otherTree.get_boxd(num8, TransformF);
			double num9 = axisAlignedBox3d.DistanceSquared(ref box);
			double num10 = axisAlignedBox3d2.DistanceSquared(ref box);
			if (num10 < num9)
			{
				if (num10 < nearest_sqr)
				{
					find_nearest_triangles(iBox, otherTree, TransformF, num8, depth + 1, ref nearest_sqr, ref nearest_pair);
				}
				if (num9 < nearest_sqr)
				{
					find_nearest_triangles(iBox, otherTree, TransformF, num7, depth + 1, ref nearest_sqr, ref nearest_pair);
				}
			}
			else
			{
				if (num9 < nearest_sqr)
				{
					find_nearest_triangles(iBox, otherTree, TransformF, num7, depth + 1, ref nearest_sqr, ref nearest_pair);
				}
				if (num10 < nearest_sqr)
				{
					find_nearest_triangles(iBox, otherTree, TransformF, num8, depth + 1, ref nearest_sqr, ref nearest_pair);
				}
			}
			return;
		}
		AxisAlignedBox3d testBox = otherTree.get_boxd(oBox, TransformF);
		int num11 = index_list[num];
		if (num11 < 0)
		{
			num11 = -num11 - 1;
			if (box_box_distsqr(num11, ref testBox) < nearest_sqr)
			{
				find_nearest_triangles(num11, otherTree, TransformF, oBox, depth + 1, ref nearest_sqr, ref nearest_pair);
			}
			return;
		}
		num11--;
		int iBox2 = index_list[num + 1] - 1;
		double num12 = box_box_distsqr(num11, ref testBox);
		double num13 = box_box_distsqr(iBox2, ref testBox);
		if (num13 < num12)
		{
			if (num13 < nearest_sqr)
			{
				find_nearest_triangles(iBox2, otherTree, TransformF, oBox, depth + 1, ref nearest_sqr, ref nearest_pair);
			}
			if (num12 < nearest_sqr)
			{
				find_nearest_triangles(num11, otherTree, TransformF, oBox, depth + 1, ref nearest_sqr, ref nearest_pair);
			}
		}
		else
		{
			if (num12 < nearest_sqr)
			{
				find_nearest_triangles(num11, otherTree, TransformF, oBox, depth + 1, ref nearest_sqr, ref nearest_pair);
			}
			if (num13 < nearest_sqr)
			{
				find_nearest_triangles(iBox2, otherTree, TransformF, oBox, depth + 1, ref nearest_sqr, ref nearest_pair);
			}
		}
	}

	public virtual bool IsInside(Vector3d p)
	{
		Vector3d direction = new Vector3d(0.331960519038825, 0.462531727525156, 0.822111072077288);
		Ray3d ray = new Ray3d(p, direction);
		return FindAllHitTriangles(ray) % 2 != 0;
	}

	public virtual void DoTraversal(TreeTraversal traversal)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.DoTraversal: mesh has been modified since tree construction");
		}
		tree_traversal(root_index, 0, traversal);
	}

	protected virtual void tree_traversal(int iBox, int depth, TreeTraversal traversal)
	{
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				if (TriangleFilterF == null || TriangleFilterF(num3))
				{
					traversal.NextTriangleF(num3);
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

	public virtual double WindingNumber(Vector3d p)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.WindingNumber: mesh has been modified since tree construction");
		}
		if (WindingCache == null || winding_cache_timestamp != mesh.ShapeTimestamp)
		{
			build_winding_cache();
			winding_cache_timestamp = mesh.ShapeTimestamp;
		}
		return branch_winding_num(root_index, p) / (Math.PI * 4.0);
	}

	protected double branch_winding_num(int iBox, Vector3d p)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		double num = 0.0;
		int num2 = box_to_index[iBox];
		if (num2 < triangles_end)
		{
			int num3 = index_list[num2];
			for (int i = 1; i <= num3; i++)
			{
				int tID = index_list[num2 + i];
				mesh.GetTriVertices(tID, ref v, ref v2, ref v3);
				num += MathUtil.TriSolidAngle(v, v2, v3, ref p);
			}
		}
		else
		{
			int num4 = index_list[num2];
			if (num4 < 0)
			{
				num4 = -num4 - 1;
				num = ((box_contains(num4, p) || !WindingCache.ContainsKey(num4)) ? (num + branch_winding_num(num4, p)) : (num + evaluate_box_winding_cache(num4, p)));
			}
			else
			{
				num4--;
				int num5 = index_list[num2 + 1] - 1;
				num = ((box_contains(num4, p) || !WindingCache.ContainsKey(num4)) ? (num + branch_winding_num(num4, p)) : (num + evaluate_box_winding_cache(num4, p)));
				num = ((box_contains(num5, p) || !WindingCache.ContainsKey(num5)) ? (num + branch_winding_num(num5, p)) : (num + evaluate_box_winding_cache(num5, p)));
			}
		}
		return num;
	}

	protected void build_winding_cache()
	{
		int tri_count_thresh = 100;
		if (Mesh.TriangleCount > 250000)
		{
			tri_count_thresh = 500;
		}
		if (Mesh.TriangleCount > 1000000)
		{
			tri_count_thresh = 1000;
		}
		WindingCache = new Dictionary<int, List<int>>();
		build_winding_cache(root_index, 0, tri_count_thresh, out var _);
	}

	protected int build_winding_cache(int iBox, int depth, int tri_count_thresh, out HashSet<int> tri_hash)
	{
		tri_hash = null;
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			return index_list[num];
		}
		int num2 = index_list[num];
		if (num2 < 0)
		{
			num2 = -num2 - 1;
			return build_winding_cache(num2, depth + 1, tri_count_thresh, out tri_hash);
		}
		num2--;
		int iBox2 = index_list[num + 1] - 1;
		int num3 = build_winding_cache(num2, depth + 1, tri_count_thresh, out tri_hash);
		HashSet<int> tri_hash2;
		int num4 = build_winding_cache(iBox2, depth + 1, tri_count_thresh, out tri_hash2);
		bool flag = num3 + num4 > tri_count_thresh;
		if (depth == 0)
		{
			return num3 + num4;
		}
		if (tri_hash != null || tri_hash2 != null || flag)
		{
			if (tri_hash == null && tri_hash2 != null)
			{
				collect_triangles(num2, tri_hash2);
				tri_hash = tri_hash2;
			}
			else
			{
				if (tri_hash == null)
				{
					tri_hash = new HashSet<int>();
					collect_triangles(num2, tri_hash);
				}
				if (tri_hash2 == null)
				{
					collect_triangles(iBox2, tri_hash);
				}
				else
				{
					tri_hash.UnionWith(tri_hash2);
				}
			}
		}
		if (flag)
		{
			make_box_winding_cache(iBox, tri_hash);
		}
		return num3 + num4;
	}

	protected void make_box_winding_cache(int iBox, HashSet<int> triangles)
	{
		List<int> list = new List<int>();
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = Mesh.GetTriangle(triangle2);
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(triangle2);
			for (int i = 0; i < 3; i++)
			{
				if (triNeighbourTris[i] == -1 || !triangles.Contains(triNeighbourTris[i]))
				{
					list.Add(triangle[(i + 1) % 3]);
					list.Add(triangle[i]);
				}
			}
		}
		WindingCache[iBox] = list;
	}

	protected double evaluate_box_winding_cache(int iBox, Vector3d p)
	{
		List<int> list = WindingCache[iBox];
		int num = list.Count / 2;
		Vector3d c = box_centers[iBox];
		double num2 = 0.0;
		for (int i = 0; i < num; i++)
		{
			Vector3d vertex = Mesh.GetVertex(list[2 * i]);
			Vector3d vertex2 = Mesh.GetVertex(list[2 * i + 1]);
			num2 += MathUtil.TriSolidAngle(vertex, vertex2, c, ref p);
		}
		return 0.0 - num2;
	}

	protected void collect_triangles(int iBox, HashSet<int> triangles)
	{
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			int num2 = index_list[num];
			for (int i = 1; i <= num2; i++)
			{
				triangles.Add(index_list[num + i]);
			}
			return;
		}
		int num3 = index_list[num];
		if (num3 < 0)
		{
			collect_triangles(-num3 - 1, triangles);
			return;
		}
		collect_triangles(num3 - 1, triangles);
		collect_triangles(index_list[num + 1] - 1, triangles);
	}

	public virtual double FastWindingNumber(Vector3d p)
	{
		if (mesh_timestamp != mesh.ShapeTimestamp)
		{
			throw new Exception("DMeshAABBTree3.FastWindingNumber: mesh has been modified since tree construction");
		}
		if (FastWindingCache == null || fast_winding_cache_timestamp != mesh.ShapeTimestamp)
		{
			build_fast_winding_cache();
			fast_winding_cache_timestamp = mesh.ShapeTimestamp;
		}
		return branch_fast_winding_num(root_index, p);
	}

	protected double branch_fast_winding_num(int iBox, Vector3d p)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		double num = 0.0;
		int num2 = box_to_index[iBox];
		if (num2 < triangles_end)
		{
			int num3 = index_list[num2];
			for (int i = 1; i <= num3; i++)
			{
				int tID = index_list[num2 + i];
				mesh.GetTriVertices(tID, ref v, ref v2, ref v3);
				num += MathUtil.TriSolidAngle(v, v2, v3, ref p) / (Math.PI * 4.0);
			}
		}
		else
		{
			int num4 = index_list[num2];
			if (num4 < 0)
			{
				num4 = -num4 - 1;
				num = ((box_contains(num4, p) || !can_use_fast_winding_cache(num4, ref p)) ? (num + branch_fast_winding_num(num4, p)) : (num + evaluate_box_fast_winding_cache(num4, ref p)));
			}
			else
			{
				num4--;
				int iBox2 = index_list[num2 + 1] - 1;
				num = ((box_contains(num4, p) || !can_use_fast_winding_cache(num4, ref p)) ? (num + branch_fast_winding_num(num4, p)) : (num + evaluate_box_fast_winding_cache(num4, ref p)));
				num = ((box_contains(iBox2, p) || !can_use_fast_winding_cache(iBox2, ref p)) ? (num + branch_fast_winding_num(iBox2, p)) : (num + evaluate_box_fast_winding_cache(iBox2, ref p)));
			}
		}
		return num;
	}

	protected void build_fast_winding_cache()
	{
		int tri_count_thresh = 1;
		MeshTriInfoCache triCache = new MeshTriInfoCache(mesh);
		FastWindingCache = new Dictionary<int, FWNInfo>();
		build_fast_winding_cache(root_index, 0, tri_count_thresh, out var _, triCache);
	}

	protected int build_fast_winding_cache(int iBox, int depth, int tri_count_thresh, out HashSet<int> tri_hash, MeshTriInfoCache triCache)
	{
		tri_hash = null;
		int num = box_to_index[iBox];
		if (num < triangles_end)
		{
			return index_list[num];
		}
		int num2 = index_list[num];
		if (num2 < 0)
		{
			num2 = -num2 - 1;
			return build_fast_winding_cache(num2, depth + 1, tri_count_thresh, out tri_hash, triCache);
		}
		num2--;
		int iBox2 = index_list[num + 1] - 1;
		int num3 = build_fast_winding_cache(num2, depth + 1, tri_count_thresh, out tri_hash, triCache);
		HashSet<int> tri_hash2;
		int num4 = build_fast_winding_cache(iBox2, depth + 1, tri_count_thresh, out tri_hash2, triCache);
		bool flag = num3 + num4 > tri_count_thresh;
		if (depth == 0)
		{
			return num3 + num4;
		}
		if (tri_hash != null || tri_hash2 != null || flag)
		{
			if (tri_hash == null && tri_hash2 != null)
			{
				collect_triangles(num2, tri_hash2);
				tri_hash = tri_hash2;
			}
			else
			{
				if (tri_hash == null)
				{
					tri_hash = new HashSet<int>();
					collect_triangles(num2, tri_hash);
				}
				if (tri_hash2 == null)
				{
					collect_triangles(iBox2, tri_hash);
				}
				else
				{
					tri_hash.UnionWith(tri_hash2);
				}
			}
		}
		if (flag)
		{
			make_box_fast_winding_cache(iBox, tri_hash, triCache);
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

	protected void make_box_fast_winding_cache(int iBox, IEnumerable<int> triangles, MeshTriInfoCache triCache)
	{
		FWNInfo value = default(FWNInfo);
		FastTriWinding.ComputeCoeffs(Mesh, triangles, ref value.Center, ref value.R, ref value.Order1Vec, ref value.Order2Mat, triCache);
		FastWindingCache[iBox] = value;
	}

	protected double evaluate_box_fast_winding_cache(int iBox, ref Vector3d q)
	{
		FWNInfo fWNInfo = FastWindingCache[iBox];
		if (FWNApproxOrder == 2)
		{
			return FastTriWinding.EvaluateOrder2Approx(ref fWNInfo.Center, ref fWNInfo.Order1Vec, ref fWNInfo.Order2Mat, ref q);
		}
		return FastTriWinding.EvaluateOrder1Approx(ref fWNInfo.Center, ref fWNInfo.Order1Vec, ref q);
	}

	public double TotalVolume()
	{
		double volSum = 0.0;
		TreeTraversal traversal = new TreeTraversal
		{
			NextBoxF = delegate(AxisAlignedBox3f box, int depth)
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
			NextBoxF = delegate(AxisAlignedBox3f box, int depth)
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
		int[] array = new int[mesh.TriangleCount];
		Vector3d[] array2 = new Vector3d[mesh.TriangleCount];
		foreach (int item in mesh.TriangleIndices())
		{
			double lengthSquared = mesh.GetTriCentroid(item).LengthSquared;
			if (!double.IsNaN(lengthSquared) && !double.IsInfinity(lengthSquared))
			{
				array[num] = item;
				array2[num] = mesh.GetTriCentroid(item);
				num++;
			}
		}
		boxes_set boxes_set = new boxes_set();
		boxes_set boxes_set2 = new boxes_set();
		AxisAlignedBox3f box;
		int num2 = (bSorted ? split_tri_set_sorted(array, array2, 0, mesh.TriangleCount, 0, TopDownLeafMaxTriCount, boxes_set, boxes_set2, out box) : split_tri_set_midpoint(array, array2, 0, mesh.TriangleCount, 0, TopDownLeafMaxTriCount, boxes_set, boxes_set2, out box));
		box_to_index = boxes_set.box_to_index;
		box_centers = boxes_set.box_centers;
		box_extents = boxes_set.box_extents;
		index_list = boxes_set.index_list;
		triangles_end = boxes_set.iIndicesCur;
		int num3 = triangles_end;
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

	private int split_tri_set_sorted(int[] triangles, Vector3d[] centers, int iStart, int iCount, int depth, int minTriCount, boxes_set tris, boxes_set nodes, out AxisAlignedBox3f box)
	{
		box = AxisAlignedBox3f.Empty;
		int num = -1;
		if (iCount < minTriCount)
		{
			num = tris.iBoxCur++;
			tris.box_to_index.insert(tris.iIndicesCur, num);
			tris.index_list.insert(iCount, tris.iIndicesCur++);
			for (int i = 0; i < iCount; i++)
			{
				tris.index_list.insert(triangles[iStart + i], tris.iIndicesCur++);
				box.Contain(mesh.GetTriBounds(triangles[iStart + i]));
			}
			tris.box_centers.insert(box.Center, num);
			tris.box_extents.insert(box.Extents, num);
			return -(num + 1);
		}
		AxisComp comparer = new AxisComp
		{
			Axis = depth % 3
		};
		Array.Sort(centers, triangles, iStart, iCount, comparer);
		int num2 = iCount / 2;
		int iCount2 = num2;
		int iCount3 = iCount - num2;
		int value = split_tri_set_sorted(triangles, centers, iStart, iCount2, depth + 1, minTriCount, tris, nodes, out box);
		AxisAlignedBox3f box2;
		int value2 = split_tri_set_sorted(triangles, centers, iStart + num2, iCount3, depth + 1, minTriCount, tris, nodes, out box2);
		box.Contain(box2);
		num = nodes.iBoxCur++;
		nodes.box_to_index.insert(nodes.iIndicesCur, num);
		nodes.index_list.insert(value, nodes.iIndicesCur++);
		nodes.index_list.insert(value2, nodes.iIndicesCur++);
		nodes.box_centers.insert(box.Center, num);
		nodes.box_extents.insert(box.Extents, num);
		return num;
	}

	private int split_tri_set_midpoint(int[] triangles, Vector3d[] centers, int iStart, int iCount, int depth, int minTriCount, boxes_set tris, boxes_set nodes, out AxisAlignedBox3f box)
	{
		box = AxisAlignedBox3f.Empty;
		int num = -1;
		if (iCount < minTriCount)
		{
			num = tris.iBoxCur++;
			tris.box_to_index.insert(tris.iIndicesCur, num);
			tris.index_list.insert(iCount, tris.iIndicesCur++);
			for (int i = 0; i < iCount; i++)
			{
				tris.index_list.insert(triangles[iStart + i], tris.iIndicesCur++);
				box.Contain(mesh.GetTriBounds(triangles[iStart + i]));
			}
			tris.box_centers.insert(box.Center, num);
			tris.box_extents.insert(box.Extents, num);
			return -(num + 1);
		}
		int key = depth % 3;
		Interval1d empty = Interval1d.Empty;
		for (int j = 0; j < iCount; j++)
		{
			empty.Contain(centers[iStart + j][key]);
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
				for (; centers[iStart + k][key] <= center; k++)
				{
				}
				while (centers[iStart + num2][key] > center)
				{
					num2--;
				}
				if (k >= num2)
				{
					break;
				}
				Vector3d vector3d = centers[iStart + k];
				centers[iStart + k] = centers[iStart + num2];
				centers[iStart + num2] = vector3d;
				int num3 = triangles[iStart + k];
				triangles[iStart + k] = triangles[iStart + num2];
				triangles[iStart + num2] = num3;
			}
			num4 = k;
			iCount2 = iCount - num4;
		}
		else
		{
			num4 = iCount / 2;
			iCount2 = iCount - num4;
		}
		int value = split_tri_set_midpoint(triangles, centers, iStart, num4, depth + 1, minTriCount, tris, nodes, out box);
		AxisAlignedBox3f box2;
		int value2 = split_tri_set_midpoint(triangles, centers, iStart + num4, iCount2, depth + 1, minTriCount, tris, nodes, out box2);
		box.Contain(box2);
		num = nodes.iBoxCur++;
		nodes.box_to_index.insert(nodes.iIndicesCur, num);
		nodes.index_list.insert(value, nodes.iIndicesCur++);
		nodes.index_list.insert(value2, nodes.iIndicesCur++);
		nodes.box_centers.insert(box.Center, num);
		nodes.box_extents.insert(box.Extents, num);
		return num;
	}

	private void build_by_one_rings(ClusterPolicy ePolicy)
	{
		box_to_index = new DVector<int>();
		box_centers = new DVector<Vector3f>();
		box_extents = new DVector<Vector3f>();
		int iBoxCur = 0;
		index_list = new DVector<int>();
		int iIndicesCur = 0;
		byte[] array = new byte[mesh.MaxTriangleID];
		Array.Clear(array, 0, array.Length);
		int maxVtxEdgeCount = mesh.GetMaxVtxEdgeCount();
		int[] temp_tris = new int[2 * maxVtxEdgeCount];
		DVector<int> dVector = new DVector<int>();
		foreach (int item in mesh.VertexIndices())
		{
			if (add_one_ring_box(item, array, temp_tris, ref iBoxCur, ref iIndicesCur, dVector, 3) < 3)
			{
				dVector.Add(item);
			}
		}
		int length = dVector.Length;
		for (int i = 0; i < length; i++)
		{
			int vid = dVector[i];
			add_one_ring_box(vid, array, temp_tris, ref iBoxCur, ref iIndicesCur, null, 0);
		}
		triangles_end = iIndicesCur;
		ClusterFunctionType clusterFunctionType = cluster_boxes_nearsearch;
		switch (ePolicy)
		{
		case ClusterPolicy.Fastest:
			clusterFunctionType = cluster_boxes;
			break;
		case ClusterPolicy.MinimalVolume:
			clusterFunctionType = cluster_boxes_matrix;
			break;
		case ClusterPolicy.FastVolumeMetric:
			clusterFunctionType = cluster_boxes_nearsearch;
			break;
		}
		int num = iBoxCur;
		int num2 = clusterFunctionType(0, iBoxCur, ref iBoxCur, ref iIndicesCur);
		int iStart = num;
		int iCount = iBoxCur - num;
		while (num2 > 1)
		{
			num = iBoxCur;
			num2 = clusterFunctionType(iStart, iCount, ref iBoxCur, ref iIndicesCur);
			iStart = num;
			iCount = iBoxCur - num;
		}
		root_index = iBoxCur - 1;
	}

	private int add_one_ring_box(int vid, byte[] used_triangles, int[] temp_tris, ref int iBoxCur, ref int iIndicesCur, DVector<int> spill, int nSpillThresh)
	{
		int num = 0;
		foreach (int item in mesh.VtxTrianglesItr(vid))
		{
			if (used_triangles[item] == 0)
			{
				temp_tris[num++] = item;
			}
		}
		if (num == 0)
		{
			return 0;
		}
		if (num < nSpillThresh)
		{
			spill.Add(vid);
			return num;
		}
		AxisAlignedBox3f empty = AxisAlignedBox3f.Empty;
		int index = iBoxCur++;
		box_to_index.insert(iIndicesCur, index);
		index_list.insert(num, iIndicesCur++);
		for (int i = 0; i < num; i++)
		{
			index_list.insert(temp_tris[i], iIndicesCur++);
			used_triangles[temp_tris[i]]++;
			empty.Contain(mesh.GetTriBounds(temp_tris[i]));
		}
		box_centers.insert(empty.Center, index);
		box_extents.insert(empty.Extents, index);
		return num;
	}

	private int cluster_boxes(int iStart, int iCount, ref int iBoxCur, ref int iIndicesCur)
	{
		int[] array = new int[iCount];
		for (int i = 0; i < iCount; i++)
		{
			array[i] = iStart + i;
		}
		int nDim = 0;
		Array.Sort(array, delegate(int a, int b)
		{
			float num5 = box_centers[a][nDim] - box_extents[a][nDim];
			float num6 = box_centers[b][nDim] - box_extents[b][nDim];
			return (num5 != num6) ? ((!(num5 < num6)) ? 1 : (-1)) : 0;
		});
		int num = iCount / 2;
		int num2 = iCount - 2 * num;
		for (int j = 0; j < num; j++)
		{
			int num3 = array[2 * j];
			int num4 = array[2 * j + 1];
			get_combined_box(num3, num4, out var center, out var extent);
			int index = iBoxCur++;
			box_to_index.insert(iIndicesCur, index);
			index_list.insert(num3 + 1, iIndicesCur++);
			index_list.insert(num4 + 1, iIndicesCur++);
			box_centers.insert(center, index);
			box_extents.insert(extent, index);
		}
		if (num2 > 0)
		{
			if (num2 > 1)
			{
				Util.gBreakToDebugger();
			}
			int i2 = array[2 * num];
			duplicate_box(i2, ref iBoxCur, ref iIndicesCur);
		}
		return num + num2;
	}

	private int cluster_boxes_nearsearch(int iStart, int iCount, ref int iBoxCur, ref int iIndicesCur)
	{
		int[] array = new int[iCount];
		for (int i = 0; i < iCount; i++)
		{
			array[i] = iStart + i;
		}
		Func<int, int, double> func = combined_box_volume;
		int nDim = 0;
		Array.Sort(array, delegate(int a, int b)
		{
			float num10 = box_centers[a][nDim] - box_extents[a][nDim];
			float num11 = box_centers[b][nDim] - box_extents[b][nDim];
			return (num10 != num11) ? ((!(num10 < num11)) ? 1 : (-1)) : 0;
		});
		int num = iCount / 2;
		int num2 = iCount - 2 * num;
		int bottomUpClusterLookahead = BottomUpClusterLookahead;
		int[] array2 = new int[bottomUpClusterLookahead];
		double[] array3 = new double[bottomUpClusterLookahead];
		for (int j = 0; j < iCount - 1; j++)
		{
			int num3 = array[j];
			if (num3 < 0)
			{
				continue;
			}
			int num4 = Math.Min(bottomUpClusterLookahead, iCount - j - 1);
			for (int k = 0; k < num4; k++)
			{
				int num5 = array[array2[k] = j + k + 1];
				if (num5 < 0)
				{
					array3[k] = double.MaxValue;
				}
				else
				{
					array3[k] = func(num3, num5);
				}
			}
			Array.Sort(array3, array2, 0, num4);
			if (array3[0] != double.MaxValue)
			{
				int num6 = array2[0];
				int num7 = array[num6];
				if (num7 < 0)
				{
					Util.gBreakToDebugger();
				}
				get_combined_box(num3, num7, out var center, out var extent);
				int index = iBoxCur++;
				box_to_index.insert(iIndicesCur, index);
				index_list.insert(num3 + 1, iIndicesCur++);
				index_list.insert(num7 + 1, iIndicesCur++);
				box_centers.insert(center, index);
				box_extents.insert(extent, index);
				array[j] = -(array[j] + 1);
				array[num6] = -(array[num6] + 1);
			}
		}
		if (num2 > 0)
		{
			int num8 = -1;
			int num9 = 0;
			while (num8 < 0 && num9 < array.Length)
			{
				if (array[num9] >= 0)
				{
					num8 = array[num9];
				}
				num9++;
			}
			duplicate_box(num8, ref iBoxCur, ref iIndicesCur);
		}
		return num + num2;
	}

	private static double find_smallest_upper(double[,] m, ref int ii, ref int jj)
	{
		double num = double.MaxValue;
		int length = m.GetLength(0);
		int length2 = m.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = i + 1; j < length2; j++)
			{
				if (m[i, j] < num)
				{
					num = m[i, j];
					ii = i;
					jj = j;
				}
			}
		}
		return num;
	}

	private int cluster_boxes_matrix(int iStart, int iCount, ref int iBoxCur, ref int iIndicesCur)
	{
		int[] array = new int[iCount];
		for (int i = 0; i < iCount; i++)
		{
			array[i] = iStart + i;
		}
		Func<int, int, double> func = combined_box_volume;
		double[,] array2 = new double[iCount, iCount];
		for (int j = 0; j < iCount; j++)
		{
			for (int k = 0; k <= j; k++)
			{
				array2[j, k] = double.MaxValue;
			}
			for (int l = j + 1; l < iCount; l++)
			{
				array2[j, l] = func(array[j], array[l]);
			}
		}
		int num = iCount / 2;
		int num2 = iCount - 2 * num;
		for (int m = 0; m < num; m++)
		{
			int ii = 0;
			int jj = 0;
			bool flag = false;
			while (!flag)
			{
				find_smallest_upper(array2, ref ii, ref jj);
				if (array[ii] >= 0 && array[jj] >= 0)
				{
					flag = true;
				}
				array2[ii, jj] = double.MaxValue;
			}
			int num3 = array[ii];
			int num4 = array[jj];
			get_combined_box(num3, num4, out var center, out var extent);
			int index = iBoxCur++;
			box_to_index.insert(iIndicesCur, index);
			index_list.insert(num3 + 1, iIndicesCur++);
			index_list.insert(num4 + 1, iIndicesCur++);
			box_centers.insert(center, index);
			box_extents.insert(extent, index);
			array[ii] = -(array[ii] + 1);
			array[jj] = -(array[jj] + 1);
		}
		if (num2 > 0)
		{
			int num5 = -1;
			int num6 = 0;
			while (num5 < 0 && num6 < array.Length)
			{
				if (array[num6] >= 0)
				{
					num5 = array[num6];
				}
				num6++;
			}
			duplicate_box(num5, ref iBoxCur, ref iIndicesCur);
		}
		return num + num2;
	}

	private void duplicate_box(int i, ref int iBoxCur, ref int iIndicesCur)
	{
		int index = iBoxCur++;
		box_to_index.insert(iIndicesCur, index);
		index_list.insert(-(i + 1), iIndicesCur++);
		box_centers.insert(box_centers[i], index);
		box_extents.insert(box_extents[i], index);
	}

	private void get_combined_box(int b0, int b1, out Vector3f center, out Vector3f extent)
	{
		Vector3f vector3f = box_centers[b0];
		Vector3f vector3f2 = box_extents[b0];
		Vector3f vector3f3 = box_centers[b1];
		Vector3f vector3f4 = box_extents[b1];
		float num = Math.Min(vector3f.x - vector3f2.x, vector3f3.x - vector3f4.x);
		float num2 = Math.Max(vector3f.x + vector3f2.x, vector3f3.x + vector3f4.x);
		float num3 = Math.Min(vector3f.y - vector3f2.y, vector3f3.y - vector3f4.y);
		float num4 = Math.Max(vector3f.y + vector3f2.y, vector3f3.y + vector3f4.y);
		float num5 = Math.Min(vector3f.z - vector3f2.z, vector3f3.z - vector3f4.z);
		float num6 = Math.Max(vector3f.z + vector3f2.z, vector3f3.z + vector3f4.z);
		center = new Vector3f(0.5f * (num + num2), 0.5f * (num3 + num4), 0.5f * (num5 + num6));
		extent = new Vector3f(0.5f * (num2 - num), 0.5f * (num4 - num3), 0.5f * (num6 - num5));
	}

	private AxisAlignedBox3f get_box(int iBox)
	{
		Vector3f vCenter = box_centers[iBox];
		Vector3f vector3f = box_extents[iBox];
		return new AxisAlignedBox3f(ref vCenter, vector3f.x + 5.9604645E-06f, vector3f.y + 5.9604645E-06f, vector3f.z + 5.9604645E-06f);
	}

	private AxisAlignedBox3d get_boxd(int iBox)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3f vector3f = box_extents[iBox];
		return new AxisAlignedBox3d(ref vCenter, vector3f.x + 5.9604645E-06f, vector3f.y + 5.9604645E-06f, vector3f.z + 5.9604645E-06f);
	}

	private AxisAlignedBox3d get_boxd(int iBox, Func<Vector3d, Vector3d> TransformF)
	{
		if (TransformF != null)
		{
			AxisAlignedBox3d boxIn = get_boxd(iBox);
			return BoundsUtil.Bounds(ref boxIn, TransformF);
		}
		return get_boxd(iBox);
	}

	private double box_ray_intersect_t(int iBox, Ray3d ray)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3f vector3f = box_extents[iBox];
		AxisAlignedBox3d box = new AxisAlignedBox3d(ref vCenter, vector3f.x + 5.9604645E-06f, vector3f.y + 5.9604645E-06f, vector3f.z + 5.9604645E-06f);
		double RayParam = double.MaxValue;
		if (IntrRay3AxisAlignedBox3.FindRayIntersectT(ref ray, ref box, out RayParam))
		{
			return RayParam;
		}
		return double.MaxValue;
	}

	private bool box_box_intersect(int iBox, ref AxisAlignedBox3d testBox)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3f vector3f = box_extents[iBox];
		return new AxisAlignedBox3d(ref vCenter, vector3f.x + 5.9604645E-06f, vector3f.y + 5.9604645E-06f, vector3f.z + 5.9604645E-06f).Intersects(testBox);
	}

	private double box_box_distsqr(int iBox, ref AxisAlignedBox3d testBox)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3f vector3f = box_extents[iBox];
		return new AxisAlignedBox3d(ref vCenter, vector3f.x + 5.9604645E-06f, vector3f.y + 5.9604645E-06f, vector3f.z + 5.9604645E-06f).DistanceSquared(ref testBox);
	}

	private double box_distance_sqr(int iBox, Vector3d p)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3f vector3f = box_extents[iBox];
		return new AxisAlignedBox3d(ref vCenter, vector3f.x + 5.9604645E-06f, vector3f.y + 5.9604645E-06f, vector3f.z + 5.9604645E-06f).DistanceSquared(p);
	}

	protected bool box_contains(int iBox, Vector3d p)
	{
		Vector3d vCenter = box_centers[iBox];
		Vector3f vector3f = box_extents[iBox];
		return new AxisAlignedBox3d(ref vCenter, vector3f.x + 5.9604645E-06f, vector3f.y + 5.9604645E-06f, vector3f.z + 5.9604645E-06f).Contains(p);
	}

	private double combined_box_volume(int b0, int b1)
	{
		Vector3f vector3f = box_centers[b0];
		Vector3f vector3f2 = box_extents[b0];
		Vector3f vector3f3 = box_centers[b1];
		Vector3f vector3f4 = box_extents[b1];
		float num = Math.Min(vector3f.x - vector3f2.x, vector3f3.x - vector3f4.x);
		float num2 = Math.Max(vector3f.x + vector3f2.x, vector3f3.x + vector3f4.x);
		float num3 = Math.Min(vector3f.y - vector3f2.y, vector3f3.y - vector3f4.y);
		float num4 = Math.Max(vector3f.y + vector3f2.y, vector3f3.y + vector3f4.y);
		float num5 = Math.Min(vector3f.z - vector3f2.z, vector3f3.z - vector3f4.z);
		float num6 = Math.Max(vector3f.z + vector3f2.z, vector3f3.z + vector3f4.z);
		return (num2 - num) * (num4 - num3) * (num6 - num5);
	}

	private double combined_box_length(int b0, int b1)
	{
		Vector3f vector3f = box_centers[b0];
		Vector3f vector3f2 = box_extents[b0];
		Vector3f vector3f3 = box_centers[b1];
		Vector3f vector3f4 = box_extents[b1];
		float num = Math.Min(vector3f.x - vector3f2.x, vector3f3.x - vector3f4.x);
		float num2 = Math.Max(vector3f.x + vector3f2.x, vector3f3.x + vector3f4.x);
		float num3 = Math.Min(vector3f.y - vector3f2.y, vector3f3.y - vector3f4.y);
		float num4 = Math.Max(vector3f.y + vector3f2.y, vector3f3.y + vector3f4.y);
		float num5 = Math.Min(vector3f.z - vector3f2.z, vector3f3.z - vector3f4.z);
		float num6 = Math.Max(vector3f.z + vector3f2.z, vector3f3.z + vector3f4.z);
		return (num2 - num) * (num2 - num) + (num4 - num3) * (num4 - num3) + (num6 - num5) * (num6 - num5);
	}

	public void TestCoverage()
	{
		int[] array = new int[mesh.MaxTriangleID];
		Array.Clear(array, 0, array.Length);
		int[] array2 = new int[box_to_index.Length];
		Array.Clear(array2, 0, array2.Length);
		test_coverage(array, array2, root_index);
		foreach (int item in mesh.TriangleIndices())
		{
			if (array[item] != 1)
			{
				Util.gBreakToDebugger();
			}
		}
	}

	private void test_coverage(int[] tri_counts, int[] parent_indices, int iBox)
	{
		int num = box_to_index[iBox];
		debug_check_child_tris_in_box(iBox);
		if (num < triangles_end)
		{
			int num2 = index_list[num];
			AxisAlignedBox3f axisAlignedBox3f = get_box(iBox);
			for (int i = 1; i <= num2; i++)
			{
				int num3 = index_list[num + i];
				tri_counts[num3]++;
				Index3i triangle = mesh.GetTriangle(num3);
				for (int j = 0; j < 3; j++)
				{
					Vector3f v = (Vector3f)mesh.GetVertex(triangle[j]);
					if (!axisAlignedBox3f.Contains(v))
					{
						Util.gBreakToDebugger();
					}
				}
			}
		}
		else
		{
			int num4 = index_list[num];
			if (num4 < 0)
			{
				num4 = -num4 - 1;
				parent_indices[num4] = iBox;
				test_coverage(tri_counts, parent_indices, num4);
				return;
			}
			num4--;
			parent_indices[num4] = iBox;
			test_coverage(tri_counts, parent_indices, num4);
			int num5 = index_list[num + 1];
			num5--;
			parent_indices[num5] = iBox;
			test_coverage(tri_counts, parent_indices, num5);
		}
	}

	private void debug_check_child_tri_distances(int iBox, Vector3d p)
	{
		double fBoxDistSqr = box_distance_sqr(iBox, p);
		TreeTraversal traversal = new TreeTraversal
		{
			NextTriangleF = delegate(int tID)
			{
				double num = MeshQueries.TriDistanceSqr(mesh, tID, p);
				if (num < fBoxDistSqr && Math.Abs(num - fBoxDistSqr) > 1E-06)
				{
					Util.gBreakToDebugger();
				}
			}
		};
		tree_traversal(iBox, 0, traversal);
	}

	private void debug_check_child_tris_in_box(int iBox)
	{
		AxisAlignedBox3f box = get_box(iBox);
		TreeTraversal traversal = new TreeTraversal
		{
			NextTriangleF = delegate(int tID)
			{
				Index3i triangle = mesh.GetTriangle(tID);
				for (int i = 0; i < 3; i++)
				{
					Vector3f v = (Vector3f)mesh.GetVertex(triangle[i]);
					if (!box.Contains(v))
					{
						Util.gBreakToDebugger();
					}
				}
			}
		};
		tree_traversal(iBox, 0, traversal);
	}
}
