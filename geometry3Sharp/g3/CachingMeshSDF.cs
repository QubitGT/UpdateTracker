using System;

namespace g3;

public class CachingMeshSDF
{
	public enum InsideModes
	{
		CrossingCount,
		ParityCount
	}

	public DMesh3 Mesh;

	public DMeshAABBTree3 Spatial;

	public float CellSize;

	public Vector3d ExpandBounds = Vector3d.Zero;

	public float MaxOffsetDistance;

	public bool UseParallel = true;

	public bool ComputeSigns = true;

	public InsideModes InsideMode = InsideModes.ParityCount;

	public bool WantClosestTriGrid;

	public bool WantIntersectionsGrid;

	public Func<bool> CancelF = () => false;

	public bool DebugPrint;

	private Vector3f grid_origin;

	private DenseGrid3f grid;

	private DenseGrid3i closest_tri_grid;

	private DenseGrid3i intersections_grid;

	private float UpperBoundDistance;

	private double MaxDistQueryDist;

	public Vector3i Dimensions => new Vector3i(grid.ni, grid.nj, grid.nk);

	public DenseGrid3f Grid => grid;

	public Vector3f GridOrigin => grid_origin;

	public DenseGrid3i ClosestTriGrid
	{
		get
		{
			if (!WantClosestTriGrid)
			{
				throw new Exception("Set WantClosestTriGrid=true to return this value");
			}
			return closest_tri_grid;
		}
	}

	public DenseGrid3i IntersectionsGrid
	{
		get
		{
			if (!WantIntersectionsGrid)
			{
				throw new Exception("Set WantIntersectionsGrid=true to return this value");
			}
			return intersections_grid;
		}
	}

	public float this[int i, int j, int k] => grid[i, j, k];

	public float this[Vector3i idx] => grid[idx.x, idx.y, idx.z];

	public CachingMeshSDF(DMesh3 mesh, double cellSize, DMeshAABBTree3 spatial)
	{
		Mesh = mesh;
		CellSize = (float)cellSize;
		Spatial = spatial;
	}

	public void Initialize()
	{
		AxisAlignedBox3d cachedBounds = Mesh.CachedBounds;
		float num = Math.Max(4f * CellSize, 2f * MaxOffsetDistance + 2f * CellSize);
		grid_origin = (Vector3f)cachedBounds.Min - num * Vector3f.One - (Vector3f)ExpandBounds;
		Vector3f vector3f = (Vector3f)cachedBounds.Max + num * Vector3f.One + (Vector3f)ExpandBounds;
		int num2 = (int)((vector3f.x - grid_origin.x) / CellSize) + 1;
		int num3 = (int)((vector3f.y - grid_origin.y) / CellSize) + 1;
		int num4 = (int)((vector3f.z - grid_origin.z) / CellSize) + 1;
		UpperBoundDistance = (float)(num2 + num3 + num4) * CellSize;
		grid = new DenseGrid3f(num2, num3, num4, UpperBoundDistance);
		MaxDistQueryDist = (double)MaxOffsetDistance + (double)(2f * CellSize) * 1.4142135623730951;
		if (WantClosestTriGrid)
		{
			closest_tri_grid = new DenseGrid3i(num2, num3, num4, -1);
		}
		DenseGrid3i denseGrid3i = new DenseGrid3i(num2, num3, num4, 0);
		if (!ComputeSigns)
		{
			return;
		}
		compute_intersections(grid_origin, CellSize, num2, num3, num4, denseGrid3i);
		if (!CancelF())
		{
			compute_signs(num2, num3, num4, grid, denseGrid3i);
			if (!CancelF() && WantIntersectionsGrid)
			{
				intersections_grid = denseGrid3i;
			}
		}
	}

	public float GetValue(Vector3i idx)
	{
		float num = grid[idx];
		if (num == UpperBoundDistance || num == 0f - UpperBoundDistance)
		{
			Vector3d p = cell_center(idx);
			float num2 = Math.Sign(num);
			double fNearestDistSqr;
			int num3 = Spatial.FindNearestTriangle(p, out fNearestDistSqr, MaxDistQueryDist);
			num = ((num3 != -1) ? (num2 * (float)Math.Sqrt(fNearestDistSqr)) : (num + 0.0001f));
			grid[idx] = num;
			if (closest_tri_grid != null)
			{
				closest_tri_grid[idx] = num3;
			}
		}
		return num;
	}

	public Vector3f CellCenter(int i, int j, int k)
	{
		return cell_center(new Vector3i(i, j, k));
	}

	private Vector3f cell_center(Vector3i ijk)
	{
		return new Vector3f((float)ijk.x * CellSize + grid_origin[0], (float)ijk.y * CellSize + grid_origin[1], (float)ijk.z * CellSize + grid_origin[2]);
	}

	private void compute_intersections(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3i intersection_count)
	{
		double ox = origin[0];
		double oy = origin[1];
		double oz = origin[2];
		double invdx = 1.0 / (double)dx;
		bool cancelled = false;
		Action<int> action = delegate(int tid)
		{
			if (tid % 100 == 0 && CancelF())
			{
				cancelled = true;
			}
			if (!cancelled)
			{
				Vector3d v = Vector3d.Zero;
				Vector3d v2 = Vector3d.Zero;
				Vector3d v3 = Vector3d.Zero;
				Mesh.GetTriVertices(tid, ref v, ref v2, ref v3);
				bool decrement = false;
				if (InsideMode == InsideModes.ParityCount)
				{
					decrement = MathUtil.FastNormalDirection(ref v, ref v2, ref v3).x > 0.0;
				}
				double num = (v[0] - ox) * invdx;
				double num2 = (v[1] - oy) * invdx;
				double num3 = (v[2] - oz) * invdx;
				double num4 = (v2[0] - ox) * invdx;
				double num5 = (v2[1] - oy) * invdx;
				double num6 = (v2[2] - oz) * invdx;
				double num7 = (v3[0] - ox) * invdx;
				double num8 = (v3[1] - oy) * invdx;
				double num9 = (v3[2] - oz) * invdx;
				int num10 = MathUtil.Clamp((int)Math.Ceiling(MathUtil.Min(num2, num5, num8)), 0, nj - 1);
				int num11 = MathUtil.Clamp((int)Math.Floor(MathUtil.Max(num2, num5, num8)), 0, nj - 1);
				int num12 = MathUtil.Clamp((int)Math.Ceiling(MathUtil.Min(num3, num6, num9)), 0, nk - 1);
				int num13 = MathUtil.Clamp((int)Math.Floor(MathUtil.Max(num3, num6, num9)), 0, nk - 1);
				for (int i = num12; i <= num13; i++)
				{
					for (int j = num10; j <= num11; j++)
					{
						if (point_in_triangle_2d(j, i, num2, num3, num5, num6, num8, num9, out var a, out var b, out var c))
						{
							int num14 = (int)Math.Ceiling(a * num + b * num4 + c * num7);
							if (num14 < 0)
							{
								intersection_count.atomic_incdec(0, j, i, decrement);
							}
							else if (num14 < ni)
							{
								intersection_count.atomic_incdec(num14, j, i, decrement);
							}
						}
					}
				}
			}
		};
		if (UseParallel)
		{
			gParallel.ForEach(Mesh.TriangleIndices(), action);
			return;
		}
		foreach (int item in Mesh.TriangleIndices())
		{
			action(item);
		}
	}

	private void compute_signs(int ni, int nj, int nk, DenseGrid3f distances, DenseGrid3i intersection_counts)
	{
		Func<int, bool> isInsideF = (int count) => count % 2 == 1;
		if (InsideMode == InsideModes.ParityCount)
		{
			isInsideF = (int count) => count > 0;
		}
		if (UseParallel)
		{
			gParallel.ForEach(new AxisAlignedBox2i(0, 0, nj, nk).IndicesExclusive(), delegate(Vector2i vi)
			{
				if (!CancelF())
				{
					int x = vi.x;
					int y = vi.y;
					int num2 = 0;
					for (int l = 0; l < ni; l++)
					{
						num2 += intersection_counts[l, x, y];
						if (isInsideF(num2))
						{
							distances[l, x, y] = 0f - distances[l, x, y];
						}
					}
				}
			});
			return;
		}
		for (int i = 0; i < nk; i++)
		{
			if (CancelF())
			{
				break;
			}
			for (int j = 0; j < nj; j++)
			{
				int num = 0;
				for (int k = 0; k < ni; k++)
				{
					num += intersection_counts[k, j, i];
					if (isInsideF(num))
					{
						distances[k, j, i] = 0f - distances[k, j, i];
					}
				}
			}
		}
	}

	public static int orientation(double x1, double y1, double x2, double y2, out double twice_signed_area)
	{
		twice_signed_area = y1 * x2 - x1 * y2;
		if (twice_signed_area > 0.0)
		{
			return 1;
		}
		if (twice_signed_area < 0.0)
		{
			return -1;
		}
		if (y2 > y1)
		{
			return 1;
		}
		if (y2 < y1)
		{
			return -1;
		}
		if (x1 > x2)
		{
			return 1;
		}
		if (x1 < x2)
		{
			return -1;
		}
		return 0;
	}

	public static bool point_in_triangle_2d(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, out double a, out double b, out double c)
	{
		a = (b = (c = 0.0));
		x1 -= x0;
		x2 -= x0;
		x3 -= x0;
		y1 -= y0;
		y2 -= y0;
		y3 -= y0;
		int num = orientation(x2, y2, x3, y3, out a);
		if (num == 0)
		{
			return false;
		}
		if (orientation(x3, y3, x1, y1, out b) != num)
		{
			return false;
		}
		if (orientation(x1, y1, x2, y2, out c) != num)
		{
			return false;
		}
		double num2 = a + b + c;
		if (num2 == 0.0)
		{
			throw new Exception("MakeNarrowBandLevelSet.point_in_triangle_2d: badness!");
		}
		a /= num2;
		b /= num2;
		c /= num2;
		return true;
	}
}
