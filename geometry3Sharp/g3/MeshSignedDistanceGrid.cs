using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class MeshSignedDistanceGrid
{
	public enum ComputeModes
	{
		FullGrid,
		NarrowBandOnly,
		NarrowBand_SpatialFloodFill
	}

	public enum InsideModes
	{
		CrossingCount,
		ParityCount
	}

	public DMesh3 Mesh;

	public DMeshAABBTree3 Spatial;

	public float CellSize;

	public int ExactBandWidth = 1;

	public Vector3d ExpandBounds = Vector3d.Zero;

	public bool UseParallel = true;

	public ComputeModes ComputeMode = ComputeModes.NarrowBandOnly;

	public double NarrowBandMaxDistance;

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

	public MeshSignedDistanceGrid(DMesh3 mesh, double cellSize, DMeshAABBTree3 spatial = null)
	{
		Mesh = mesh;
		CellSize = (float)cellSize;
		Spatial = spatial;
	}

	public void Compute()
	{
		AxisAlignedBox3d cachedBounds = Mesh.CachedBounds;
		float num = (float)(2 * ExactBandWidth) * CellSize;
		if (ComputeMode == ComputeModes.NarrowBand_SpatialFloodFill)
		{
			num = (float)Math.Max(num, 2.0 * NarrowBandMaxDistance);
		}
		grid_origin = (Vector3f)cachedBounds.Min - num * Vector3f.One - (Vector3f)ExpandBounds;
		Vector3f vector3f = (Vector3f)cachedBounds.Max + num * Vector3f.One + (Vector3f)ExpandBounds;
		int ni = (int)((vector3f.x - grid_origin.x) / CellSize) + 1;
		int nj = (int)((vector3f.y - grid_origin.y) / CellSize) + 1;
		int nk = (int)((vector3f.z - grid_origin.z) / CellSize) + 1;
		grid = new DenseGrid3f();
		if (ComputeMode == ComputeModes.NarrowBand_SpatialFloodFill)
		{
			if (Spatial == null || NarrowBandMaxDistance == 0.0 || !UseParallel)
			{
				throw new Exception("MeshSignedDistanceGrid.Compute: must set Spatial data structure and band max distance, and UseParallel=true");
			}
			make_level_set3_parallel_floodfill(grid_origin, CellSize, ni, nj, nk, grid, ExactBandWidth);
		}
		else if (UseParallel)
		{
			if (Spatial != null)
			{
				make_level_set3_parallel_spatial(grid_origin, CellSize, ni, nj, nk, grid, ExactBandWidth);
			}
			else
			{
				make_level_set3_parallel(grid_origin, CellSize, ni, nj, nk, grid, ExactBandWidth);
			}
		}
		else
		{
			make_level_set3(grid_origin, CellSize, ni, nj, nk, grid, ExactBandWidth);
		}
	}

	public Vector3f CellCenter(int i, int j, int k)
	{
		return cell_center(new Vector3i(i, j, k));
	}

	private Vector3f cell_center(Vector3i ijk)
	{
		return new Vector3f((float)ijk.x * CellSize + grid_origin[0], (float)ijk.y * CellSize + grid_origin[1], (float)ijk.z * CellSize + grid_origin[2]);
	}

	private float upper_bound(DenseGrid3f grid)
	{
		return (float)(grid.ni + grid.nj + grid.nk) * CellSize;
	}

	private float cell_tri_dist(Vector3i idx, int tid)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		Vector3d x = cell_center(idx);
		Mesh.GetTriVertices(tid, ref v, ref v2, ref v3);
		return (float)point_triangle_distance(ref x, ref v, ref v2, ref v3);
	}

	private void make_level_set3(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f distances, int exact_band)
	{
		distances.resize(ni, nj, nk);
		distances.assign(upper_bound(distances));
		DenseGrid3i denseGrid3i = new DenseGrid3i(ni, nj, nk, -1);
		DenseGrid3i denseGrid3i2 = new DenseGrid3i(ni, nj, nk, 0);
		if (DebugPrint)
		{
			Console.WriteLine("start");
		}
		double num = dx;
		double num2 = origin[0];
		double num3 = origin[1];
		double num4 = origin[2];
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		foreach (int item in Mesh.TriangleIndices())
		{
			if (item % 100 == 0 && CancelF())
			{
				break;
			}
			Mesh.GetTriVertices(item, ref v, ref v2, ref v3);
			double a = (v[0] - num2) / num;
			double a2 = (v[1] - num3) / num;
			double a3 = (v[2] - num4) / num;
			double b = (v2[0] - num2) / num;
			double b2 = (v2[1] - num3) / num;
			double b3 = (v2[2] - num4) / num;
			double c = (v3[0] - num2) / num;
			double c2 = (v3[1] - num3) / num;
			double c3 = (v3[2] - num4) / num;
			int num5 = MathUtil.Clamp((int)MathUtil.Min(a, b, c) - exact_band, 0, ni - 1);
			int num6 = MathUtil.Clamp((int)MathUtil.Max(a, b, c) + exact_band + 1, 0, ni - 1);
			int num7 = MathUtil.Clamp((int)MathUtil.Min(a2, b2, c2) - exact_band, 0, nj - 1);
			int num8 = MathUtil.Clamp((int)MathUtil.Max(a2, b2, c2) + exact_band + 1, 0, nj - 1);
			int num9 = MathUtil.Clamp((int)MathUtil.Min(a3, b3, c3) - exact_band, 0, nk - 1);
			int num10 = MathUtil.Clamp((int)MathUtil.Max(a3, b3, c3) + exact_band + 1, 0, nk - 1);
			for (int i = num9; i <= num10; i++)
			{
				for (int j = num7; j <= num8; j++)
				{
					for (int k = num5; k <= num6; k++)
					{
						Vector3d x = new Vector3d((float)k * dx + origin[0], (float)j * dx + origin[1], (float)i * dx + origin[2]);
						float num11 = (float)point_triangle_distance(ref x, ref v, ref v2, ref v3);
						if (num11 < distances[k, j, i])
						{
							distances[k, j, i] = num11;
							denseGrid3i[k, j, i] = item;
						}
					}
				}
			}
		}
		if (CancelF())
		{
			return;
		}
		if (ComputeSigns)
		{
			if (DebugPrint)
			{
				Console.WriteLine("done narrow-band");
			}
			compute_intersections(origin, dx, ni, nj, nk, denseGrid3i2);
			if (CancelF())
			{
				return;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done intersections");
			}
			if (ComputeMode == ComputeModes.FullGrid)
			{
				for (int l = 0; l < 2; l++)
				{
					sweep_pass(origin, dx, distances, denseGrid3i);
					if (CancelF())
					{
						return;
					}
				}
				if (DebugPrint)
				{
					Console.WriteLine("done sweeping");
				}
			}
			else if (DebugPrint)
			{
				Console.WriteLine("skipped sweeping");
			}
			compute_signs(ni, nj, nk, distances, denseGrid3i2);
			if (CancelF())
			{
				return;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done signs");
			}
			if (WantIntersectionsGrid)
			{
				intersections_grid = denseGrid3i2;
			}
		}
		if (WantClosestTriGrid)
		{
			closest_tri_grid = denseGrid3i;
		}
	}

	private void make_level_set3_parallel(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f distances, int exact_band)
	{
		distances.resize(ni, nj, nk);
		distances.assign(upper_bound(grid));
		DenseGrid3i closest_tri = new DenseGrid3i(ni, nj, nk, -1);
		DenseGrid3i denseGrid3i = new DenseGrid3i(ni, nj, nk, 0);
		if (DebugPrint)
		{
			Console.WriteLine("start");
		}
		double ox = origin[0];
		double oy = origin[1];
		double oz = origin[2];
		double invdx = 1.0 / (double)dx;
		int wi = ni / 2;
		int wj = nj / 2;
		int wk = nk / 2;
		SpinLock[] grid_locks = new SpinLock[8];
		bool abort = false;
		gParallel.ForEach(Mesh.TriangleIndices(), delegate(int tid)
		{
			if (tid % 100 == 0)
			{
				abort = CancelF();
			}
			if (!abort)
			{
				Vector3d v = Vector3d.Zero;
				Vector3d v2 = Vector3d.Zero;
				Vector3d v3 = Vector3d.Zero;
				Mesh.GetTriVertices(tid, ref v, ref v2, ref v3);
				double a = (v[0] - ox) * invdx;
				double a2 = (v[1] - oy) * invdx;
				double a3 = (v[2] - oz) * invdx;
				double b = (v2[0] - ox) * invdx;
				double b2 = (v2[1] - oy) * invdx;
				double b3 = (v2[2] - oz) * invdx;
				double c = (v3[0] - ox) * invdx;
				double c2 = (v3[1] - oy) * invdx;
				double c3 = (v3[2] - oz) * invdx;
				int num = MathUtil.Clamp((int)MathUtil.Min(a, b, c) - exact_band, 0, ni - 1);
				int num2 = MathUtil.Clamp((int)MathUtil.Max(a, b, c) + exact_band + 1, 0, ni - 1);
				int num3 = MathUtil.Clamp((int)MathUtil.Min(a2, b2, c2) - exact_band, 0, nj - 1);
				int num4 = MathUtil.Clamp((int)MathUtil.Max(a2, b2, c2) + exact_band + 1, 0, nj - 1);
				int num5 = MathUtil.Clamp((int)MathUtil.Min(a3, b3, c3) - exact_band, 0, nk - 1);
				int num6 = MathUtil.Clamp((int)MathUtil.Max(a3, b3, c3) + exact_band + 1, 0, nk - 1);
				for (int j = num5; j <= num6; j++)
				{
					for (int k = num3; k <= num4; k++)
					{
						int num7 = ((k >= wj) ? 1 : 0) | ((j >= wk) ? 2 : 0);
						for (int l = num; l <= num2; l++)
						{
							Vector3d x = new Vector3d((float)l * dx + origin[0], (float)k * dx + origin[1], (float)j * dx + origin[2]);
							float num8 = (float)point_triangle_distance(ref x, ref v, ref v2, ref v3);
							if (num8 < distances[l, k, j])
							{
								int num9 = num7 | ((l >= wi) ? 4 : 0);
								bool lockTaken = false;
								grid_locks[num9].Enter(ref lockTaken);
								if (num8 < distances[l, k, j])
								{
									distances[l, k, j] = num8;
									closest_tri[l, k, j] = tid;
								}
								grid_locks[num9].Exit();
							}
						}
					}
				}
			}
		});
		if (DebugPrint)
		{
			Console.WriteLine("done narrow-band");
		}
		if (CancelF())
		{
			return;
		}
		if (ComputeSigns)
		{
			compute_intersections(origin, dx, ni, nj, nk, denseGrid3i);
			if (CancelF())
			{
				return;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done intersections");
			}
			if (ComputeMode == ComputeModes.FullGrid)
			{
				for (int i = 0; i < 2; i++)
				{
					sweep_pass(origin, dx, distances, closest_tri);
					if (CancelF())
					{
						return;
					}
				}
				if (DebugPrint)
				{
					Console.WriteLine("done sweeping");
				}
			}
			else if (DebugPrint)
			{
				Console.WriteLine("skipped sweeping");
			}
			if (DebugPrint)
			{
				Console.WriteLine("done sweeping");
			}
			compute_signs(ni, nj, nk, distances, denseGrid3i);
			if (CancelF())
			{
				return;
			}
			if (WantIntersectionsGrid)
			{
				intersections_grid = denseGrid3i;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done signs");
			}
		}
		if (WantClosestTriGrid)
		{
			closest_tri_grid = closest_tri;
		}
	}

	private void make_level_set3_parallel_spatial(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f distances, int exact_band)
	{
		distances.resize(ni, nj, nk);
		float upper_bound = this.upper_bound(distances);
		distances.assign(upper_bound);
		DenseGrid3i closest_tri = new DenseGrid3i(ni, nj, nk, -1);
		DenseGrid3i denseGrid3i = new DenseGrid3i(ni, nj, nk, 0);
		if (DebugPrint)
		{
			Console.WriteLine("start");
		}
		double ox = origin[0];
		double oy = origin[1];
		double oz = origin[2];
		double invdx = 1.0 / (double)dx;
		bool abort = false;
		gParallel.ForEach(Mesh.TriangleIndices(), delegate(int tid)
		{
			if (tid % 100 == 0)
			{
				abort = CancelF();
			}
			if (!abort)
			{
				Vector3d v = Vector3d.Zero;
				Vector3d v2 = Vector3d.Zero;
				Vector3d v3 = Vector3d.Zero;
				Mesh.GetTriVertices(tid, ref v, ref v2, ref v3);
				double a = (v[0] - ox) * invdx;
				double a2 = (v[1] - oy) * invdx;
				double a3 = (v[2] - oz) * invdx;
				double b = (v2[0] - ox) * invdx;
				double b2 = (v2[1] - oy) * invdx;
				double b3 = (v2[2] - oz) * invdx;
				double c = (v3[0] - ox) * invdx;
				double c2 = (v3[1] - oy) * invdx;
				double c3 = (v3[2] - oz) * invdx;
				int num2 = MathUtil.Clamp((int)MathUtil.Min(a, b, c) - exact_band, 0, ni - 1);
				int num3 = MathUtil.Clamp((int)MathUtil.Max(a, b, c) + exact_band + 1, 0, ni - 1);
				int num4 = MathUtil.Clamp((int)MathUtil.Min(a2, b2, c2) - exact_band, 0, nj - 1);
				int num5 = MathUtil.Clamp((int)MathUtil.Max(a2, b2, c2) + exact_band + 1, 0, nj - 1);
				int num6 = MathUtil.Clamp((int)MathUtil.Min(a3, b3, c3) - exact_band, 0, nk - 1);
				int num7 = MathUtil.Clamp((int)MathUtil.Max(a3, b3, c3) + exact_band + 1, 0, nk - 1);
				for (int j = num6; j <= num7; j++)
				{
					for (int k = num4; k <= num5; k++)
					{
						for (int l = num2; l <= num3; l++)
						{
							distances[l, k, j] = 1f;
						}
					}
				}
			}
		});
		if (DebugPrint)
		{
			Console.WriteLine("done narrow-band tagging");
		}
		double max_dist = (double)exact_band * ((double)dx * 1.4142135623730951);
		gParallel.ForEach(grid.Indices(), delegate(Vector3i idx)
		{
			if (distances[idx] == 1f)
			{
				int x = idx.x;
				int y = idx.y;
				int z = idx.z;
				Vector3d point = new Vector3d((float)x * dx + origin[0], (float)y * dx + origin[1], (float)z * dx + origin[2]);
				int num = Spatial.FindNearestTriangle(point, max_dist);
				if (num == -1)
				{
					distances[idx] = upper_bound;
				}
				else
				{
					Triangle3d triangle = default(Triangle3d);
					Mesh.GetTriVertices(num, ref triangle.V0, ref triangle.V1, ref triangle.V2);
					Vector3d closestPoint = default(Vector3d);
					Vector3d baryCoords = default(Vector3d);
					double d = DistPoint3Triangle3.DistanceSqr(ref point, ref triangle, out closestPoint, out baryCoords);
					distances[idx] = (float)Math.Sqrt(d);
					closest_tri[idx] = num;
				}
			}
		});
		if (DebugPrint)
		{
			Console.WriteLine("done distances");
		}
		if (CancelF())
		{
			return;
		}
		if (ComputeSigns)
		{
			if (DebugPrint)
			{
				Console.WriteLine("done narrow-band");
			}
			compute_intersections(origin, dx, ni, nj, nk, denseGrid3i);
			if (CancelF())
			{
				return;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done intersections");
			}
			if (ComputeMode == ComputeModes.FullGrid)
			{
				for (int i = 0; i < 2; i++)
				{
					sweep_pass(origin, dx, distances, closest_tri);
					if (CancelF())
					{
						return;
					}
				}
				if (DebugPrint)
				{
					Console.WriteLine("done sweeping");
				}
			}
			else if (DebugPrint)
			{
				Console.WriteLine("skipped sweeping");
			}
			if (DebugPrint)
			{
				Console.WriteLine("done sweeping");
			}
			compute_signs(ni, nj, nk, distances, denseGrid3i);
			if (CancelF())
			{
				return;
			}
			if (WantIntersectionsGrid)
			{
				intersections_grid = denseGrid3i;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done signs");
			}
		}
		if (WantClosestTriGrid)
		{
			closest_tri_grid = closest_tri;
		}
	}

	private void make_level_set3_parallel_floodfill(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f distances, int exact_band)
	{
		distances.resize(ni, nj, nk);
		float upper_bound = this.upper_bound(distances);
		distances.assign(upper_bound);
		DenseGrid3i closest_tri = new DenseGrid3i(ni, nj, nk, -1);
		DenseGrid3i denseGrid3i = new DenseGrid3i(ni, nj, nk, 0);
		if (DebugPrint)
		{
			Console.WriteLine("start");
		}
		double ox = origin[0];
		double oy = origin[1];
		double oz = origin[2];
		double invdx = 1.0 / (double)dx;
		SpinLock grid_lock = default(SpinLock);
		List<int> Q = new List<int>();
		bool[] done = new bool[distances.size];
		bool abort = false;
		gParallel.ForEach(Mesh.VertexIndices(), delegate(int vid)
		{
			if (vid % 100 == 0)
			{
				abort = CancelF();
			}
			if (!abort)
			{
				Vector3d vertex = Mesh.GetVertex(vid);
				double num4 = (vertex.x - ox) * invdx;
				double num5 = (vertex.y - oy) * invdx;
				double num6 = (vertex.z - oz) * invdx;
				Vector3i ijk2 = new Vector3i(MathUtil.Clamp((int)num4, 0, ni - 1), MathUtil.Clamp((int)num5, 0, nj - 1), MathUtil.Clamp((int)num6, 0, nk - 1));
				if (!(distances[ijk2] < upper_bound))
				{
					bool lockTaken2 = false;
					grid_lock.Enter(ref lockTaken2);
					Vector3d point2 = cell_center(ijk2);
					int num7 = Spatial.FindNearestTriangle(point2);
					Triangle3d triangle2 = default(Triangle3d);
					Mesh.GetTriVertices(num7, ref triangle2.V0, ref triangle2.V1, ref triangle2.V2);
					Vector3d closestPoint2 = default(Vector3d);
					Vector3d baryCoords2 = default(Vector3d);
					double d = DistPoint3Triangle3.DistanceSqr(ref point2, ref triangle2, out closestPoint2, out baryCoords2);
					distances[ijk2] = (float)Math.Sqrt(d);
					closest_tri[ijk2] = num7;
					int num8 = distances.to_linear(ref ijk2);
					Q.Add(num8);
					done[num8] = true;
					grid_lock.Exit();
				}
			}
		});
		if (DebugPrint)
		{
			Console.WriteLine("done vertices");
		}
		if (CancelF())
		{
			return;
		}
		List<int> next_Q = new List<int>();
		AxisAlignedBox3i bounds = distances.BoundsInclusive;
		double max_dist = NarrowBandMaxDistance;
		double max_query_dist = max_dist + (double)(2f * dx) * 1.4142135623730951;
		for (int count = Q.Count; count > 0; count = Q.Count)
		{
			next_Q.Clear();
			gParallel.ForEach(Q, delegate(int cur_linear_index)
			{
				Vector3i vector3i = distances.to_index(cur_linear_index);
				Vector3i[] gridOffsets = gIndices.GridOffsets26;
				foreach (Vector3i vector3i2 in gridOffsets)
				{
					Vector3i ijk = vector3i + vector3i2;
					if (bounds.Contains(ijk))
					{
						int num = distances.to_linear(ref ijk);
						if (!done[num])
						{
							Vector3d point = cell_center(ijk);
							int num2 = Spatial.FindNearestTriangle(point, max_query_dist);
							if (num2 == -1)
							{
								done[num] = true;
							}
							else
							{
								Triangle3d triangle = default(Triangle3d);
								Mesh.GetTriVertices(num2, ref triangle.V0, ref triangle.V1, ref triangle.V2);
								Vector3d closestPoint = default(Vector3d);
								Vector3d baryCoords = default(Vector3d);
								double num3 = Math.Sqrt(DistPoint3Triangle3.DistanceSqr(ref point, ref triangle, out closestPoint, out baryCoords));
								bool lockTaken = false;
								grid_lock.Enter(ref lockTaken);
								if (!done[num])
								{
									distances[num] = (float)num3;
									closest_tri[num] = num2;
									done[num] = true;
									if (num3 < max_dist)
									{
										next_Q.Add(num);
									}
								}
								grid_lock.Exit();
							}
						}
					}
				}
			});
			List<int> list = Q;
			Q = next_Q;
			next_Q = list;
		}
		if (DebugPrint)
		{
			Console.WriteLine("done floodfill");
		}
		if (CancelF())
		{
			return;
		}
		if (ComputeSigns)
		{
			if (DebugPrint)
			{
				Console.WriteLine("done narrow-band");
			}
			compute_intersections(origin, dx, ni, nj, nk, denseGrid3i);
			if (CancelF())
			{
				return;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done intersections");
			}
			if (ComputeMode == ComputeModes.FullGrid)
			{
				for (int i = 0; i < 2; i++)
				{
					sweep_pass(origin, dx, distances, closest_tri);
					if (CancelF())
					{
						return;
					}
				}
				if (DebugPrint)
				{
					Console.WriteLine("done sweeping");
				}
			}
			else if (DebugPrint)
			{
				Console.WriteLine("skipped sweeping");
			}
			if (DebugPrint)
			{
				Console.WriteLine("done sweeping");
			}
			compute_signs(ni, nj, nk, distances, denseGrid3i);
			if (CancelF())
			{
				return;
			}
			if (WantIntersectionsGrid)
			{
				intersections_grid = denseGrid3i;
			}
			if (DebugPrint)
			{
				Console.WriteLine("done signs");
			}
		}
		if (WantClosestTriGrid)
		{
			closest_tri_grid = closest_tri;
		}
	}

	private void sweep_pass(Vector3f origin, float dx, DenseGrid3f distances, DenseGrid3i closest_tri)
	{
		sweep(distances, closest_tri, origin, dx, 1, 1, 1);
		if (CancelF())
		{
			return;
		}
		sweep(distances, closest_tri, origin, dx, -1, -1, -1);
		if (CancelF())
		{
			return;
		}
		sweep(distances, closest_tri, origin, dx, 1, 1, -1);
		if (CancelF())
		{
			return;
		}
		sweep(distances, closest_tri, origin, dx, -1, -1, 1);
		if (CancelF())
		{
			return;
		}
		sweep(distances, closest_tri, origin, dx, 1, -1, 1);
		if (CancelF())
		{
			return;
		}
		sweep(distances, closest_tri, origin, dx, -1, 1, -1);
		if (!CancelF())
		{
			sweep(distances, closest_tri, origin, dx, 1, -1, -1);
			if (!CancelF())
			{
				sweep(distances, closest_tri, origin, dx, -1, 1, 1);
			}
		}
	}

	private void sweep(DenseGrid3f phi, DenseGrid3i closest_tri, Vector3f origin, float dx, int di, int dj, int dk)
	{
		int num;
		int num2;
		if (di > 0)
		{
			num = 1;
			num2 = phi.ni;
		}
		else
		{
			num = phi.ni - 2;
			num2 = -1;
		}
		int num3;
		int num4;
		if (dj > 0)
		{
			num3 = 1;
			num4 = phi.nj;
		}
		else
		{
			num3 = phi.nj - 2;
			num4 = -1;
		}
		int num5;
		int num6;
		if (dk > 0)
		{
			num5 = 1;
			num6 = phi.nk;
		}
		else
		{
			num5 = phi.nk - 2;
			num6 = -1;
		}
		for (int i = num5; i != num6; i += dk)
		{
			if (CancelF())
			{
				break;
			}
			for (int j = num3; j != num4; j += dj)
			{
				for (int k = num; k != num2; k += di)
				{
					Vector3d gx = new Vector3d((float)k * dx + origin[0], (float)j * dx + origin[1], (float)i * dx + origin[2]);
					check_neighbour(phi, closest_tri, ref gx, k, j, i, k - di, j, i);
					check_neighbour(phi, closest_tri, ref gx, k, j, i, k, j - dj, i);
					check_neighbour(phi, closest_tri, ref gx, k, j, i, k - di, j - dj, i);
					check_neighbour(phi, closest_tri, ref gx, k, j, i, k, j, i - dk);
					check_neighbour(phi, closest_tri, ref gx, k, j, i, k - di, j, i - dk);
					check_neighbour(phi, closest_tri, ref gx, k, j, i, k, j - dj, i - dk);
					check_neighbour(phi, closest_tri, ref gx, k, j, i, k - di, j - dj, i - dk);
				}
			}
		}
	}

	private void check_neighbour(DenseGrid3f phi, DenseGrid3i closest_tri, ref Vector3d gx, int i0, int j0, int k0, int i1, int j1, int k1)
	{
		if (closest_tri[i1, j1, k1] >= 0)
		{
			Vector3d v = Vector3f.Zero;
			Vector3d v2 = Vector3f.Zero;
			Vector3d v3 = Vector3f.Zero;
			Mesh.GetTriVertices(closest_tri[i1, j1, k1], ref v, ref v2, ref v3);
			float num = (float)point_triangle_distance(ref gx, ref v, ref v2, ref v3);
			if (num < phi[i0, j0, k0])
			{
				phi[i0, j0, k0] = num;
				closest_tri[i0, j0, k0] = closest_tri[i1, j1, k1];
			}
		}
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

	public static float point_segment_distance(ref Vector3f x0, ref Vector3f x1, ref Vector3f x2)
	{
		Vector3f vector3f = x2 - x1;
		float lengthSquared = vector3f.LengthSquared;
		float num = vector3f.Dot(x2 - x0) / lengthSquared;
		if (num < 0f)
		{
			num = 0f;
		}
		else if (num > 1f)
		{
			num = 1f;
		}
		return x0.Distance(num * x1 + (1f - num) * x2);
	}

	public static double point_segment_distance(ref Vector3d x0, ref Vector3d x1, ref Vector3d x2)
	{
		Vector3d vector3d = x2 - x1;
		double lengthSquared = vector3d.LengthSquared;
		double num = vector3d.Dot(x2 - x0) / lengthSquared;
		if (num < 0.0)
		{
			num = 0.0;
		}
		else if (num > 1.0)
		{
			num = 1.0;
		}
		return x0.Distance(num * x1 + (1.0 - num) * x2);
	}

	public static float point_triangle_distance(ref Vector3f x0, ref Vector3f x1, ref Vector3f x2, ref Vector3f x3)
	{
		Vector3f vector3f = x1 - x3;
		Vector3f v = x2 - x3;
		Vector3f v2 = x0 - x3;
		float lengthSquared = vector3f.LengthSquared;
		float lengthSquared2 = v.LengthSquared;
		float num = vector3f.Dot(v);
		float num2 = 1f / Math.Max(lengthSquared * lengthSquared2 - num * num, 1E-30f);
		float num3 = vector3f.Dot(v2);
		float num4 = v.Dot(v2);
		float num5 = num2 * (lengthSquared2 * num3 - num * num4);
		float num6 = num2 * (lengthSquared * num4 - num * num3);
		float num7 = 1f - num5 - num6;
		if (num5 >= 0f && num6 >= 0f && num7 >= 0f)
		{
			return x0.Distance(num5 * x1 + num6 * x2 + num7 * x3);
		}
		if (num5 > 0f)
		{
			return Math.Min(point_segment_distance(ref x0, ref x1, ref x2), point_segment_distance(ref x0, ref x1, ref x3));
		}
		if (num6 > 0f)
		{
			return Math.Min(point_segment_distance(ref x0, ref x1, ref x2), point_segment_distance(ref x0, ref x2, ref x3));
		}
		return Math.Min(point_segment_distance(ref x0, ref x1, ref x3), point_segment_distance(ref x0, ref x2, ref x3));
	}

	public static double point_triangle_distance(ref Vector3d x0, ref Vector3d x1, ref Vector3d x2, ref Vector3d x3)
	{
		Vector3d vector3d = x1 - x3;
		Vector3d v = x2 - x3;
		Vector3d v2 = x0 - x3;
		double lengthSquared = vector3d.LengthSquared;
		double lengthSquared2 = v.LengthSquared;
		double num = vector3d.Dot(ref v);
		double num2 = 1.0 / Math.Max(lengthSquared * lengthSquared2 - num * num, 1E-30);
		double num3 = vector3d.Dot(ref v2);
		double num4 = v.Dot(ref v2);
		double num5 = num2 * (lengthSquared2 * num3 - num * num4);
		double num6 = num2 * (lengthSquared * num4 - num * num3);
		double num7 = 1.0 - num5 - num6;
		if (num5 >= 0.0 && num6 >= 0.0 && num7 >= 0.0)
		{
			return x0.Distance(num5 * x1 + num6 * x2 + num7 * x3);
		}
		if (num5 > 0.0)
		{
			return Math.Min(point_segment_distance(ref x0, ref x1, ref x2), point_segment_distance(ref x0, ref x1, ref x3));
		}
		if (num6 > 0.0)
		{
			return Math.Min(point_segment_distance(ref x0, ref x1, ref x2), point_segment_distance(ref x0, ref x2, ref x3));
		}
		return Math.Min(point_segment_distance(ref x0, ref x1, ref x3), point_segment_distance(ref x0, ref x2, ref x3));
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
