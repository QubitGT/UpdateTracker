using System;
using System.Collections.Generic;
using System.Threading;
using g3;

namespace gs;

public class MeshWindingNumberGrid
{
	public enum ComputeModes
	{
		FullGrid,
		NarrowBand
	}

	public DMesh3 Mesh;

	public DMeshAABBTree3 MeshSpatial;

	public double CellSize;

	public int BufferCells = 1;

	public ComputeModes ComputeMode = ComputeModes.NarrowBand;

	public float WindingIsoValue = 0.5f;

	public bool WantMeshSDFGrid = true;

	public Func<bool> CancelF = () => false;

	public bool DebugPrint;

	private Vector3f grid_origin;

	private DenseGrid3f winding_grid;

	private MeshSignedDistanceGrid mesh_sdf;

	public Vector3i Dimensions => new Vector3i(winding_grid.ni, winding_grid.nj, winding_grid.nk);

	public DenseGrid3f Grid => winding_grid;

	public Vector3f GridOrigin => grid_origin;

	public MeshSignedDistanceGrid SDFGrid => mesh_sdf;

	public float this[int i, int j, int k] => winding_grid[i, j, k];

	public MeshWindingNumberGrid(DMesh3 mesh, DMeshAABBTree3 spatial, double cellSize)
	{
		Mesh = mesh;
		MeshSpatial = spatial;
		CellSize = cellSize;
	}

	public void Compute()
	{
		AxisAlignedBox3d cachedBounds = Mesh.CachedBounds;
		float num = (float)(2 * BufferCells) * (float)CellSize;
		grid_origin = (Vector3f)cachedBounds.Min - num * Vector3f.One;
		Vector3f vector3f = (Vector3f)cachedBounds.Max + num * Vector3f.One;
		int ni = (int)((vector3f.x - grid_origin.x) / (float)CellSize) + 1;
		int nj = (int)((vector3f.y - grid_origin.y) / (float)CellSize) + 1;
		int nk = (int)((vector3f.z - grid_origin.z) / (float)CellSize) + 1;
		winding_grid = new DenseGrid3f();
		if (ComputeMode == ComputeModes.FullGrid)
		{
			make_grid_dense(grid_origin, (float)CellSize, ni, nj, nk, winding_grid);
		}
		else
		{
			make_grid(grid_origin, (float)CellSize, ni, nj, nk, winding_grid);
		}
	}

	public Vector3f CellCenter(int i, int j, int k)
	{
		return new Vector3f((double)(float)i * CellSize + (double)grid_origin.x, (double)(float)j * CellSize + (double)grid_origin.y, (double)(float)k * CellSize + (double)grid_origin.z);
	}

	private void make_grid(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f winding)
	{
		winding.resize(ni, nj, nk);
		winding.assign(float.MaxValue);
		MeshSpatial.WindingNumber(Vector3d.Zero);
		if (DebugPrint)
		{
			Console.WriteLine("start");
		}
		MeshSignedDistanceGrid meshSignedDistanceGrid = new MeshSignedDistanceGrid(Mesh, CellSize)
		{
			ComputeSigns = false
		};
		meshSignedDistanceGrid.CancelF = CancelF;
		meshSignedDistanceGrid.Compute();
		if (CancelF())
		{
			return;
		}
		DenseGrid3f distances = meshSignedDistanceGrid.Grid;
		if (WantMeshSDFGrid)
		{
			mesh_sdf = meshSignedDistanceGrid;
		}
		if (DebugPrint)
		{
			Console.WriteLine("done initial sdf");
		}
		_ = origin[0];
		_ = origin[1];
		_ = origin[2];
		gParallel.ForEach(gIndices.Grid3IndicesYZ(nj, nk), delegate(Vector3i jk)
		{
			if (!CancelF())
			{
				for (int j = 0; j < ni; j++)
				{
					Vector3i ijk2 = new Vector3i(j, jk.y, jk.z);
					if ((double)distances[ijk2] < CellSize)
					{
						Vector3d p2 = new Vector3d((float)ijk2.x * dx + origin[0], (float)ijk2.y * dx + origin[1], (float)ijk2.z * dx + origin[2]);
						winding[ijk2] = (float)MeshSpatial.WindingNumber(p2);
					}
				}
			}
		});
		if (CancelF())
		{
			return;
		}
		if (DebugPrint)
		{
			Console.WriteLine("done narrow-band");
		}
		AxisAlignedBox3i bounds = winding.Bounds;
		bounds.Max -= Vector3i.One;
		Bitmap3 bits = new Bitmap3(new Vector3i(ni, nj, nk));
		List<Vector3i> list = new List<Vector3i>();
		foreach (Vector3i item in winding.Indices())
		{
			if (winding[item] != float.MaxValue)
			{
				list.Add(item);
				bits[item] = true;
			}
		}
		if (CancelF())
		{
			return;
		}
		HashSet<Vector3i> queue = new HashSet<Vector3i>();
		SpinLock queue_lock = default(SpinLock);
		while (true)
		{
			if (CancelF())
			{
				return;
			}
			bool abort = false;
			int iter_count = 0;
			gParallel.ForEach(list, delegate(Vector3i ijk)
			{
				Interlocked.Increment(ref iter_count);
				if (iter_count % 100 == 0)
				{
					abort = CancelF();
				}
				if (!abort)
				{
					float num2 = winding[ijk];
					for (int i = 0; i < 26; i++)
					{
						Vector3i vector3i = ijk + gIndices.GridOffsets26[i];
						if (bounds.Contains(vector3i))
						{
							float num3 = winding[vector3i];
							if (num3 == float.MaxValue)
							{
								Vector3d p = new Vector3d((float)vector3i.x * dx + origin[0], (float)vector3i.y * dx + origin[1], (float)vector3i.z * dx + origin[2]);
								num3 = (float)MeshSpatial.WindingNumber(p);
								winding[vector3i] = num3;
							}
							if (!bits[vector3i] && ((num2 < WindingIsoValue && num3 > WindingIsoValue) || (num2 > WindingIsoValue && num3 < WindingIsoValue)))
							{
								bool lockTaken = false;
								queue_lock.Enter(ref lockTaken);
								queue.Add(vector3i);
								queue_lock.Exit();
							}
						}
					}
				}
			});
			if (DebugPrint)
			{
				Console.WriteLine("front has {0} voxels", queue.Count);
			}
			if (queue.Count == 0)
			{
				break;
			}
			foreach (Vector3i item2 in queue)
			{
				bits[item2] = true;
			}
			list.Clear();
			list.AddRange(queue);
			queue.Clear();
		}
		if (DebugPrint)
		{
			Console.WriteLine("done front-prop");
		}
		if (DebugPrint)
		{
			int num = 0;
			foreach (Vector3i item3 in winding.Indices())
			{
				if (winding[item3] != float.MaxValue)
				{
					num++;
				}
			}
			Console.WriteLine("filled: {0} / {1}  -  {2}%", num, ni * nj * nk, (double)num / (double)(ni * nj * nk) * 100.0);
		}
		if (!CancelF())
		{
			fill_spans(ni, nj, nk, winding);
			if (DebugPrint)
			{
				Console.WriteLine("done sweep");
			}
		}
	}

	private void make_grid_dense(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f winding)
	{
		winding.resize(ni, nj, nk);
		MeshSpatial.WindingNumber(Vector3d.Zero);
		bool abort = false;
		int count = 0;
		gParallel.ForEach(winding.Indices(), delegate(Vector3i ijk)
		{
			Interlocked.Increment(ref count);
			if (count % 100 == 0)
			{
				abort = CancelF();
			}
			if (!abort)
			{
				Vector3d p = new Vector3d((float)ijk.x * dx + origin[0], (float)ijk.y * dx + origin[1], (float)ijk.z * dx + origin[2]);
				winding[ijk] = (float)MeshSpatial.WindingNumber(p);
			}
		});
	}

	private void fill_spans(int ni, int nj, int nk, DenseGrid3f winding)
	{
		gParallel.ForEach(gIndices.Grid3IndicesYZ(nj, nk), delegate(Vector3i idx)
		{
			int y = idx.y;
			int z = idx.z;
			float num = winding[0, y, z];
			if (num == float.MaxValue)
			{
				num = 0f;
			}
			for (int i = 0; i < ni; i++)
			{
				if (winding[i, y, z] == float.MaxValue)
				{
					winding[i, y, z] = num;
				}
				else
				{
					num = winding[i, y, z];
					if (num < WindingIsoValue)
					{
						num = 0f;
					}
				}
			}
		});
	}
}
