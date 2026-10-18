using System;

namespace g3;

public class BlockSupportGenerator
{
	public DMesh3 Mesh;

	public double CellSize;

	public double OverhangAngleDeg = 30.0;

	public float ForceMinY = float.MaxValue;

	public bool SubtractMesh;

	public double SubtractMeshOffset = 0.05;

	public Func<bool> CancelF = () => false;

	public bool DebugPrint;

	private AxisAlignedBox3d grid_bounds;

	private Vector3f grid_origin;

	private DenseGrid3f volume_grid;

	private MeshSignedDistanceGrid sdf;

	public DMesh3 SupportMesh;

	private const float SUPPORT_TIP_TOP = -1f;

	public Vector3i Dimensions => new Vector3i(volume_grid.ni, volume_grid.nj, volume_grid.nk);

	public DenseGrid3f Grid => volume_grid;

	public Vector3f GridOrigin => grid_origin;

	public float this[int i, int j, int k] => volume_grid[i, j, k];

	public BlockSupportGenerator(DMesh3 mesh, double cellSize)
	{
		Mesh = mesh;
		CellSize = cellSize;
	}

	public BlockSupportGenerator(DMesh3 mesh, int grid_resolution)
	{
		Mesh = mesh;
		double num = Math.Max(Mesh.CachedBounds.Width, Mesh.CachedBounds.Height);
		CellSize = num / (double)grid_resolution;
	}

	public void Generate()
	{
		grid_bounds = Mesh.CachedBounds;
		if (ForceMinY != float.MaxValue)
		{
			grid_bounds.Min.y = ForceMinY;
		}
		float num = 2f * (float)CellSize;
		Vector3f vector3f = new Vector3f(num, 0f, num);
		grid_origin = (Vector3f)grid_bounds.Min - vector3f;
		grid_origin.y += (float)CellSize * 0.5f;
		Vector3f vector3f2 = (Vector3f)grid_bounds.Max + vector3f;
		int ni = (int)((vector3f2.x - grid_origin.x) / (float)CellSize) + 1;
		int nj = (int)((vector3f2.y - grid_origin.y) / (float)CellSize) + 1;
		int nk = (int)((vector3f2.z - grid_origin.z) / (float)CellSize) + 1;
		volume_grid = new DenseGrid3f();
		generate_support(grid_origin, (float)CellSize, ni, nj, nk, volume_grid);
	}

	public Vector3f CellCenter(int i, int j, int k)
	{
		return new Vector3f((double)(float)i * CellSize + (double)grid_origin.x, (double)(float)j * CellSize + (double)grid_origin.y, (double)(float)k * CellSize + (double)grid_origin.z);
	}

	private void generate_support(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f supportGrid)
	{
		supportGrid.resize(ni, nj, nk);
		supportGrid.assign(1f);
		bool flag = false;
		int num = 1;
		if (SubtractMesh && SubtractMeshOffset > 0.0)
		{
			int val = (int)(SubtractMeshOffset / CellSize) + 1;
			num = Math.Max(num, val);
		}
		sdf = new MeshSignedDistanceGrid(Mesh, CellSize)
		{
			ComputeSigns = true,
			ExactBandWidth = num
		};
		sdf.CancelF = CancelF;
		sdf.Compute();
		if (CancelF())
		{
			return;
		}
		DenseGridTrilinearImplicit distanceField = new DenseGridTrilinearImplicit(sdf.Grid, sdf.GridOrigin, sdf.CellSize);
		double num2 = Math.Cos(MathUtil.Clamp(OverhangAngleDeg, 0.01, 89.99) * (Math.PI / 180.0));
		double num3 = dx;
		double num4 = origin[0];
		double num5 = origin[1];
		double num6 = origin[2];
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
			if (MathUtil.Normal(ref v, ref v2, ref v3).Dot(-Vector3d.AxisY) < num2)
			{
				continue;
			}
			double a = (v[0] - num4) / num3;
			double a2 = (v[1] - num5) / num3;
			double a3 = (v[2] - num6) / num3;
			double b = (v2[0] - num4) / num3;
			double b2 = (v2[1] - num5) / num3;
			double b3 = (v2[2] - num6) / num3;
			double c = (v3[0] - num4) / num3;
			double c2 = (v3[1] - num5) / num3;
			double c3 = (v3[2] - num6) / num3;
			int num7 = 0;
			int num8 = MathUtil.Clamp((int)MathUtil.Min(a, b, c) - num7, 0, ni - 1);
			int num9 = MathUtil.Clamp((int)MathUtil.Max(a, b, c) + num7 + 1, 0, ni - 1);
			int num10 = MathUtil.Clamp((int)MathUtil.Min(a2, b2, c2) - num7, 0, nj - 1);
			int num11 = MathUtil.Clamp((int)MathUtil.Max(a2, b2, c2) + num7 + 1, 0, nj - 1);
			int num12 = MathUtil.Clamp((int)MathUtil.Min(a3, b3, c3) - num7, 0, nk - 1);
			int num13 = MathUtil.Clamp((int)MathUtil.Max(a3, b3, c3) + num7 + 1, 0, nk - 1);
			for (int i = num12; i <= num13; i++)
			{
				for (int j = num10; j <= num11; j++)
				{
					for (int k = num8; k <= num9; k++)
					{
						Vector3d x = new Vector3d((float)k * dx + origin[0], (float)j * dx + origin[1], (float)i * dx + origin[2]);
						float num14 = (float)MeshSignedDistanceGrid.point_triangle_distance(ref x, ref v, ref v2, ref v3);
						if (flag)
						{
							int num15 = ((i % 2 == 0) ? 1 : 0);
							if (k % 2 == num15)
							{
								continue;
							}
						}
						if (num14 < dx / 2f)
						{
							supportGrid[k, j, i] = -1f;
						}
					}
				}
			}
		}
		if (!CancelF())
		{
			fill_vertical_spans(supportGrid, distanceField);
			generate_mesh(supportGrid, distanceField);
		}
	}

	private Vector3d get_cell_center(Vector3i ijk)
	{
		return new Vector3d((double)ijk.x * CellSize, (double)ijk.y * CellSize, (double)ijk.z * CellSize) + GridOrigin;
	}

	private Vector3d get_cell_center(int i, int j, int k)
	{
		return new Vector3d((double)i * CellSize, (double)j * CellSize, (double)k * CellSize) + GridOrigin;
	}

	private void fill_vertical_spans(DenseGrid3f supportGrid, DenseGridTrilinearImplicit distanceField)
	{
		int ni = supportGrid.ni;
		int nj = supportGrid.nj;
		int nk = supportGrid.nk;
		_ = CellSize;
		_ = GridOrigin;
		for (int i = 0; i < nk; i++)
		{
			for (int j = 0; j < ni; j++)
			{
				bool flag = false;
				for (int num = nj - 1; num >= 0; num--)
				{
					if (supportGrid[j, num, i] >= 0f)
					{
						Vector3d pt = get_cell_center(j, num, i);
						if (flag)
						{
							if (distanceField.Value(ref pt) < 0.0)
							{
								supportGrid[j, num, i] = -3f;
								flag = false;
							}
							else
							{
								supportGrid[j, num, i] = -1f;
							}
						}
					}
					else
					{
						flag = true;
					}
				}
			}
		}
	}

	private void generate_mesh(DenseGrid3f supportGrid, DenseGridTrilinearImplicit distanceField)
	{
		DenseGridTrilinearImplicit denseGridTrilinearImplicit = new DenseGridTrilinearImplicit(supportGrid, GridOrigin, CellSize);
		BoundedImplicitFunction3d a = denseGridTrilinearImplicit;
		if (SubtractMesh)
		{
			BoundedImplicitFunction3d b = distanceField;
			if (SubtractMeshOffset > 0.0)
			{
				b = new ImplicitOffset3d
				{
					A = distanceField,
					Offset = SubtractMeshOffset
				};
			}
			a = new ImplicitDifference3d
			{
				A = denseGridTrilinearImplicit,
				B = b
			};
		}
		ImplicitHalfSpace3d b2 = new ImplicitHalfSpace3d
		{
			Origin = Vector3d.Zero,
			Normal = Vector3d.AxisY
		};
		ImplicitDifference3d @implicit = new ImplicitDifference3d
		{
			A = a,
			B = b2
		};
		MarchingCubes marchingCubes = new MarchingCubes
		{
			Implicit = @implicit,
			Bounds = grid_bounds,
			CubeSize = CellSize
		};
		marchingCubes.Bounds.Min.y = -2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Min.x -= 2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Min.z -= 2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Max.x += 2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Max.z += 2.0 * marchingCubes.CubeSize;
		marchingCubes.Generate();
		SupportMesh = marchingCubes.Mesh;
	}
}
