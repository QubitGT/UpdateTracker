using System;
using System.Collections.Generic;
using System.Threading;
using g3;

namespace gs;

public class MarchingCubesPro
{
	public enum RootfindingModes
	{
		SingleLerp,
		LerpSteps,
		Bisection
	}

	private class GridCell
	{
		public Vector3i[] i;

		public double[] f;

		public GridCell()
		{
			i = new Vector3i[8];
			f = new double[8];
		}
	}

	public ImplicitFunction3d Implicit;

	public double IsoValue;

	public AxisAlignedBox3d Bounds;

	public double CubeSize = 0.1;

	public bool ParallelCompute = true;

	public RootfindingModes RootMode;

	public int RootModeSteps = 5;

	public Func<bool> CancelF = () => false;

	public Vector3i CellDimensions;

	public DMesh3 Mesh;

	private AxisAlignedBox3i GridBounds;

	private AxisAlignedBox3i LastGridBounds;

	private const int EDGE_X = 268435456;

	private const int EDGE_Y = 536870912;

	private const int EDGE_Z = 1073741824;

	private Dictionary<long, int> edge_vertices = new Dictionary<long, int>();

	private SpinLock edge_vertices_lock;

	private Dictionary<long, double> corner_values = new Dictionary<long, double>();

	private SpinLock corner_values_lock;

	private DenseGrid3f corner_values_grid;

	private bool parallel_mesh_access;

	private SpinLock mesh_lock;

	private DenseGrid3i done_cells;

	private SpinLock done_cells_lock;

	private static readonly int[,] edge_indices = new int[12, 2]
	{
		{ 0, 1 },
		{ 1, 2 },
		{ 2, 3 },
		{ 3, 0 },
		{ 4, 5 },
		{ 5, 6 },
		{ 6, 7 },
		{ 7, 4 },
		{ 0, 4 },
		{ 1, 5 },
		{ 2, 6 },
		{ 3, 7 }
	};

	private static readonly int[] edgeTable = new int[256]
	{
		0, 265, 515, 778, 1030, 1295, 1541, 1804, 2060, 2309,
		2575, 2822, 3082, 3331, 3593, 3840, 400, 153, 915, 666,
		1430, 1183, 1941, 1692, 2460, 2197, 2975, 2710, 3482, 3219,
		3993, 3728, 560, 825, 51, 314, 1590, 1855, 1077, 1340,
		2620, 2869, 2111, 2358, 3642, 3891, 3129, 3376, 928, 681,
		419, 170, 1958, 1711, 1445, 1196, 2988, 2725, 2479, 2214,
		4010, 3747, 3497, 3232, 1120, 1385, 1635, 1898, 102, 367,
		613, 876, 3180, 3429, 3695, 3942, 2154, 2403, 2665, 2912,
		1520, 1273, 2035, 1786, 502, 255, 1013, 764, 3580, 3317,
		4095, 3830, 2554, 2291, 3065, 2800, 1616, 1881, 1107, 1370,
		598, 863, 85, 348, 3676, 3925, 3167, 3414, 2650, 2899,
		2137, 2384, 1984, 1737, 1475, 1226, 966, 719, 453, 204,
		4044, 3781, 3535, 3270, 3018, 2755, 2505, 2240, 2240, 2505,
		2755, 3018, 3270, 3535, 3781, 4044, 204, 453, 719, 966,
		1226, 1475, 1737, 1984, 2384, 2137, 2899, 2650, 3414, 3167,
		3925, 3676, 348, 85, 863, 598, 1370, 1107, 1881, 1616,
		2800, 3065, 2291, 2554, 3830, 4095, 3317, 3580, 764, 1013,
		255, 502, 1786, 2035, 1273, 1520, 2912, 2665, 2403, 2154,
		3942, 3695, 3429, 3180, 876, 613, 367, 102, 1898, 1635,
		1385, 1120, 3232, 3497, 3747, 4010, 2214, 2479, 2725, 2988,
		1196, 1445, 1711, 1958, 170, 419, 681, 928, 3376, 3129,
		3891, 3642, 2358, 2111, 2869, 2620, 1340, 1077, 1855, 1590,
		314, 51, 825, 560, 3728, 3993, 3219, 3482, 2710, 2975,
		2197, 2460, 1692, 1941, 1183, 1430, 666, 915, 153, 400,
		3840, 3593, 3331, 3082, 2822, 2575, 2309, 2060, 1804, 1541,
		1295, 1030, 778, 515, 265, 0
	};

	private static readonly int[,] triTable = new int[256, 16]
	{
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 8, 3, 9, 8, 1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 1, 2, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 2, 10, 0, 2, 9, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 8, 3, 2, 10, 8, 10, 9, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 11, 2, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 11, 2, 8, 11, 0, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 9, 0, 2, 3, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 11, 2, 1, 9, 11, 9, 8, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 10, 1, 11, 10, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 10, 1, 0, 8, 10, 8, 11, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 9, 0, 3, 11, 9, 11, 10, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 8, 10, 10, 8, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 7, 8, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 3, 0, 7, 3, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, 8, 4, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 1, 9, 4, 7, 1, 7, 3, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 8, 4, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 4, 7, 3, 0, 4, 1, 2, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 2, 10, 9, 0, 2, 8, 4, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 10, 9, 2, 9, 7, 2, 7, 3, 7,
			9, 4, -1, -1, -1, -1
		},
		{
			8, 4, 7, 3, 11, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 4, 7, 11, 2, 4, 2, 0, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 0, 1, 8, 4, 7, 2, 3, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 7, 11, 9, 4, 11, 9, 11, 2, 9,
			2, 1, -1, -1, -1, -1
		},
		{
			3, 10, 1, 3, 11, 10, 7, 8, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 11, 10, 1, 4, 11, 1, 0, 4, 7,
			11, 4, -1, -1, -1, -1
		},
		{
			4, 7, 8, 9, 0, 11, 9, 11, 10, 11,
			0, 3, -1, -1, -1, -1
		},
		{
			4, 7, 11, 4, 11, 9, 9, 11, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 4, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 4, 0, 8, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 5, 4, 1, 5, 0, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 5, 4, 8, 3, 5, 3, 1, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 9, 5, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 8, 1, 2, 10, 4, 9, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 2, 10, 5, 4, 2, 4, 0, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 10, 5, 3, 2, 5, 3, 5, 4, 3,
			4, 8, -1, -1, -1, -1
		},
		{
			9, 5, 4, 2, 3, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 11, 2, 0, 8, 11, 4, 9, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 5, 4, 0, 1, 5, 2, 3, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 1, 5, 2, 5, 8, 2, 8, 11, 4,
			8, 5, -1, -1, -1, -1
		},
		{
			10, 3, 11, 10, 1, 3, 9, 5, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 5, 0, 8, 1, 8, 10, 1, 8,
			11, 10, -1, -1, -1, -1
		},
		{
			5, 4, 0, 5, 0, 11, 5, 11, 10, 11,
			0, 3, -1, -1, -1, -1
		},
		{
			5, 4, 8, 5, 8, 10, 10, 8, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 7, 8, 5, 7, 9, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 3, 0, 9, 5, 3, 5, 7, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 7, 8, 0, 1, 7, 1, 5, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 5, 3, 3, 5, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 7, 8, 9, 5, 7, 10, 1, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 1, 2, 9, 5, 0, 5, 3, 0, 5,
			7, 3, -1, -1, -1, -1
		},
		{
			8, 0, 2, 8, 2, 5, 8, 5, 7, 10,
			5, 2, -1, -1, -1, -1
		},
		{
			2, 10, 5, 2, 5, 3, 3, 5, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 9, 5, 7, 8, 9, 3, 11, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 7, 9, 7, 2, 9, 2, 0, 2,
			7, 11, -1, -1, -1, -1
		},
		{
			2, 3, 11, 0, 1, 8, 1, 7, 8, 1,
			5, 7, -1, -1, -1, -1
		},
		{
			11, 2, 1, 11, 1, 7, 7, 1, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 8, 8, 5, 7, 10, 1, 3, 10,
			3, 11, -1, -1, -1, -1
		},
		{
			5, 7, 0, 5, 0, 9, 7, 11, 0, 1,
			0, 10, 11, 10, 0, -1
		},
		{
			11, 10, 0, 11, 0, 3, 10, 5, 0, 8,
			0, 7, 5, 7, 0, -1
		},
		{
			11, 10, 5, 7, 11, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 6, 5, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 5, 10, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 0, 1, 5, 10, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 8, 3, 1, 9, 8, 5, 10, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 6, 5, 2, 6, 1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 6, 5, 1, 2, 6, 3, 0, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 6, 5, 9, 0, 6, 0, 2, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 9, 8, 5, 8, 2, 5, 2, 6, 3,
			2, 8, -1, -1, -1, -1
		},
		{
			2, 3, 11, 10, 6, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 0, 8, 11, 2, 0, 10, 6, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, 2, 3, 11, 5, 10, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 10, 6, 1, 9, 2, 9, 11, 2, 9,
			8, 11, -1, -1, -1, -1
		},
		{
			6, 3, 11, 6, 5, 3, 5, 1, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 11, 0, 11, 5, 0, 5, 1, 5,
			11, 6, -1, -1, -1, -1
		},
		{
			3, 11, 6, 0, 3, 6, 0, 6, 5, 0,
			5, 9, -1, -1, -1, -1
		},
		{
			6, 5, 9, 6, 9, 11, 11, 9, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 10, 6, 4, 7, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 3, 0, 4, 7, 3, 6, 5, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 9, 0, 5, 10, 6, 8, 4, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 6, 5, 1, 9, 7, 1, 7, 3, 7,
			9, 4, -1, -1, -1, -1
		},
		{
			6, 1, 2, 6, 5, 1, 4, 7, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 5, 5, 2, 6, 3, 0, 4, 3,
			4, 7, -1, -1, -1, -1
		},
		{
			8, 4, 7, 9, 0, 5, 0, 6, 5, 0,
			2, 6, -1, -1, -1, -1
		},
		{
			7, 3, 9, 7, 9, 4, 3, 2, 9, 5,
			9, 6, 2, 6, 9, -1
		},
		{
			3, 11, 2, 7, 8, 4, 10, 6, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 10, 6, 4, 7, 2, 4, 2, 0, 2,
			7, 11, -1, -1, -1, -1
		},
		{
			0, 1, 9, 4, 7, 8, 2, 3, 11, 5,
			10, 6, -1, -1, -1, -1
		},
		{
			9, 2, 1, 9, 11, 2, 9, 4, 11, 7,
			11, 4, 5, 10, 6, -1
		},
		{
			8, 4, 7, 3, 11, 5, 3, 5, 1, 5,
			11, 6, -1, -1, -1, -1
		},
		{
			5, 1, 11, 5, 11, 6, 1, 0, 11, 7,
			11, 4, 0, 4, 11, -1
		},
		{
			0, 5, 9, 0, 6, 5, 0, 3, 6, 11,
			6, 3, 8, 4, 7, -1
		},
		{
			6, 5, 9, 6, 9, 11, 4, 7, 9, 7,
			11, 9, -1, -1, -1, -1
		},
		{
			10, 4, 9, 6, 4, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 10, 6, 4, 9, 10, 0, 8, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 0, 1, 10, 6, 0, 6, 4, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 3, 1, 8, 1, 6, 8, 6, 4, 6,
			1, 10, -1, -1, -1, -1
		},
		{
			1, 4, 9, 1, 2, 4, 2, 6, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 8, 1, 2, 9, 2, 4, 9, 2,
			6, 4, -1, -1, -1, -1
		},
		{
			0, 2, 4, 4, 2, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 3, 2, 8, 2, 4, 4, 2, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 4, 9, 10, 6, 4, 11, 2, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 2, 2, 8, 11, 4, 9, 10, 4,
			10, 6, -1, -1, -1, -1
		},
		{
			3, 11, 2, 0, 1, 6, 0, 6, 4, 6,
			1, 10, -1, -1, -1, -1
		},
		{
			6, 4, 1, 6, 1, 10, 4, 8, 1, 2,
			1, 11, 8, 11, 1, -1
		},
		{
			9, 6, 4, 9, 3, 6, 9, 1, 3, 11,
			6, 3, -1, -1, -1, -1
		},
		{
			8, 11, 1, 8, 1, 0, 11, 6, 1, 9,
			1, 4, 6, 4, 1, -1
		},
		{
			3, 11, 6, 3, 6, 0, 0, 6, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 4, 8, 11, 6, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 10, 6, 7, 8, 10, 8, 9, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 7, 3, 0, 10, 7, 0, 9, 10, 6,
			7, 10, -1, -1, -1, -1
		},
		{
			10, 6, 7, 1, 10, 7, 1, 7, 8, 1,
			8, 0, -1, -1, -1, -1
		},
		{
			10, 6, 7, 10, 7, 1, 1, 7, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 6, 1, 6, 8, 1, 8, 9, 8,
			6, 7, -1, -1, -1, -1
		},
		{
			2, 6, 9, 2, 9, 1, 6, 7, 9, 0,
			9, 3, 7, 3, 9, -1
		},
		{
			7, 8, 0, 7, 0, 6, 6, 0, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 3, 2, 6, 7, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 3, 11, 10, 6, 8, 10, 8, 9, 8,
			6, 7, -1, -1, -1, -1
		},
		{
			2, 0, 7, 2, 7, 11, 0, 9, 7, 6,
			7, 10, 9, 10, 7, -1
		},
		{
			1, 8, 0, 1, 7, 8, 1, 10, 7, 6,
			7, 10, 2, 3, 11, -1
		},
		{
			11, 2, 1, 11, 1, 7, 10, 6, 1, 6,
			7, 1, -1, -1, -1, -1
		},
		{
			8, 9, 6, 8, 6, 7, 9, 1, 6, 11,
			6, 3, 1, 3, 6, -1
		},
		{
			0, 9, 1, 11, 6, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 8, 0, 7, 0, 6, 3, 11, 0, 11,
			6, 0, -1, -1, -1, -1
		},
		{
			7, 11, 6, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 6, 11, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 8, 11, 7, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, 11, 7, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 1, 9, 8, 3, 1, 11, 7, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 1, 2, 6, 11, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 3, 0, 8, 6, 11, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 9, 0, 2, 10, 9, 6, 11, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 11, 7, 2, 10, 3, 10, 8, 3, 10,
			9, 8, -1, -1, -1, -1
		},
		{
			7, 2, 3, 6, 2, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 0, 8, 7, 6, 0, 6, 2, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 7, 6, 2, 3, 7, 0, 1, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 6, 2, 1, 8, 6, 1, 9, 8, 8,
			7, 6, -1, -1, -1, -1
		},
		{
			10, 7, 6, 10, 1, 7, 1, 3, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 7, 6, 1, 7, 10, 1, 8, 7, 1,
			0, 8, -1, -1, -1, -1
		},
		{
			0, 3, 7, 0, 7, 10, 0, 10, 9, 6,
			10, 7, -1, -1, -1, -1
		},
		{
			7, 6, 10, 7, 10, 8, 8, 10, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 8, 4, 11, 8, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 6, 11, 3, 0, 6, 0, 4, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 6, 11, 8, 4, 6, 9, 0, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 4, 6, 9, 6, 3, 9, 3, 1, 11,
			3, 6, -1, -1, -1, -1
		},
		{
			6, 8, 4, 6, 11, 8, 2, 10, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 3, 0, 11, 0, 6, 11, 0,
			4, 6, -1, -1, -1, -1
		},
		{
			4, 11, 8, 4, 6, 11, 0, 2, 9, 2,
			10, 9, -1, -1, -1, -1
		},
		{
			10, 9, 3, 10, 3, 2, 9, 4, 3, 11,
			3, 6, 4, 6, 3, -1
		},
		{
			8, 2, 3, 8, 4, 2, 4, 6, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 4, 2, 4, 6, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 9, 0, 2, 3, 4, 2, 4, 6, 4,
			3, 8, -1, -1, -1, -1
		},
		{
			1, 9, 4, 1, 4, 2, 2, 4, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 1, 3, 8, 6, 1, 8, 4, 6, 6,
			10, 1, -1, -1, -1, -1
		},
		{
			10, 1, 0, 10, 0, 6, 6, 0, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 6, 3, 4, 3, 8, 6, 10, 3, 0,
			3, 9, 10, 9, 3, -1
		},
		{
			10, 9, 4, 6, 10, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 5, 7, 6, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 4, 9, 5, 11, 7, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 0, 1, 5, 4, 0, 7, 6, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 7, 6, 8, 3, 4, 3, 5, 4, 3,
			1, 5, -1, -1, -1, -1
		},
		{
			9, 5, 4, 10, 1, 2, 7, 6, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 11, 7, 1, 2, 10, 0, 8, 3, 4,
			9, 5, -1, -1, -1, -1
		},
		{
			7, 6, 11, 5, 4, 10, 4, 2, 10, 4,
			0, 2, -1, -1, -1, -1
		},
		{
			3, 4, 8, 3, 5, 4, 3, 2, 5, 10,
			5, 2, 11, 7, 6, -1
		},
		{
			7, 2, 3, 7, 6, 2, 5, 4, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 4, 0, 8, 6, 0, 6, 2, 6,
			8, 7, -1, -1, -1, -1
		},
		{
			3, 6, 2, 3, 7, 6, 1, 5, 0, 5,
			4, 0, -1, -1, -1, -1
		},
		{
			6, 2, 8, 6, 8, 7, 2, 1, 8, 4,
			8, 5, 1, 5, 8, -1
		},
		{
			9, 5, 4, 10, 1, 6, 1, 7, 6, 1,
			3, 7, -1, -1, -1, -1
		},
		{
			1, 6, 10, 1, 7, 6, 1, 0, 7, 8,
			7, 0, 9, 5, 4, -1
		},
		{
			4, 0, 10, 4, 10, 5, 0, 3, 10, 6,
			10, 7, 3, 7, 10, -1
		},
		{
			7, 6, 10, 7, 10, 8, 5, 4, 10, 4,
			8, 10, -1, -1, -1, -1
		},
		{
			6, 9, 5, 6, 11, 9, 11, 8, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 6, 11, 0, 6, 3, 0, 5, 6, 0,
			9, 5, -1, -1, -1, -1
		},
		{
			0, 11, 8, 0, 5, 11, 0, 1, 5, 5,
			6, 11, -1, -1, -1, -1
		},
		{
			6, 11, 3, 6, 3, 5, 5, 3, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 9, 5, 11, 9, 11, 8, 11,
			5, 6, -1, -1, -1, -1
		},
		{
			0, 11, 3, 0, 6, 11, 0, 9, 6, 5,
			6, 9, 1, 2, 10, -1
		},
		{
			11, 8, 5, 11, 5, 6, 8, 0, 5, 10,
			5, 2, 0, 2, 5, -1
		},
		{
			6, 11, 3, 6, 3, 5, 2, 10, 3, 10,
			5, 3, -1, -1, -1, -1
		},
		{
			5, 8, 9, 5, 2, 8, 5, 6, 2, 3,
			8, 2, -1, -1, -1, -1
		},
		{
			9, 5, 6, 9, 6, 0, 0, 6, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 5, 8, 1, 8, 0, 5, 6, 8, 3,
			8, 2, 6, 2, 8, -1
		},
		{
			1, 5, 6, 2, 1, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 3, 6, 1, 6, 10, 3, 8, 6, 5,
			6, 9, 8, 9, 6, -1
		},
		{
			10, 1, 0, 10, 0, 6, 9, 5, 0, 5,
			6, 0, -1, -1, -1, -1
		},
		{
			0, 3, 8, 5, 6, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 5, 6, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 5, 10, 7, 5, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 5, 10, 11, 7, 5, 8, 3, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 11, 7, 5, 10, 11, 1, 9, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 7, 5, 10, 11, 7, 9, 8, 1, 8,
			3, 1, -1, -1, -1, -1
		},
		{
			11, 1, 2, 11, 7, 1, 7, 5, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 1, 2, 7, 1, 7, 5, 7,
			2, 11, -1, -1, -1, -1
		},
		{
			9, 7, 5, 9, 2, 7, 9, 0, 2, 2,
			11, 7, -1, -1, -1, -1
		},
		{
			7, 5, 2, 7, 2, 11, 5, 9, 2, 3,
			2, 8, 9, 8, 2, -1
		},
		{
			2, 5, 10, 2, 3, 5, 3, 7, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 2, 0, 8, 5, 2, 8, 7, 5, 10,
			2, 5, -1, -1, -1, -1
		},
		{
			9, 0, 1, 5, 10, 3, 5, 3, 7, 3,
			10, 2, -1, -1, -1, -1
		},
		{
			9, 8, 2, 9, 2, 1, 8, 7, 2, 10,
			2, 5, 7, 5, 2, -1
		},
		{
			1, 3, 5, 3, 7, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 7, 0, 7, 1, 1, 7, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 0, 3, 9, 3, 5, 5, 3, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 8, 7, 5, 9, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 8, 4, 5, 10, 8, 10, 11, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 0, 4, 5, 11, 0, 5, 10, 11, 11,
			3, 0, -1, -1, -1, -1
		},
		{
			0, 1, 9, 8, 4, 10, 8, 10, 11, 10,
			4, 5, -1, -1, -1, -1
		},
		{
			10, 11, 4, 10, 4, 5, 11, 3, 4, 9,
			4, 1, 3, 1, 4, -1
		},
		{
			2, 5, 1, 2, 8, 5, 2, 11, 8, 4,
			5, 8, -1, -1, -1, -1
		},
		{
			0, 4, 11, 0, 11, 3, 4, 5, 11, 2,
			11, 1, 5, 1, 11, -1
		},
		{
			0, 2, 5, 0, 5, 9, 2, 11, 5, 4,
			5, 8, 11, 8, 5, -1
		},
		{
			9, 4, 5, 2, 11, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 5, 10, 3, 5, 2, 3, 4, 5, 3,
			8, 4, -1, -1, -1, -1
		},
		{
			5, 10, 2, 5, 2, 4, 4, 2, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 10, 2, 3, 5, 10, 3, 8, 5, 4,
			5, 8, 0, 1, 9, -1
		},
		{
			5, 10, 2, 5, 2, 4, 1, 9, 2, 9,
			4, 2, -1, -1, -1, -1
		},
		{
			8, 4, 5, 8, 5, 3, 3, 5, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 4, 5, 1, 0, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 4, 5, 8, 5, 3, 9, 0, 5, 0,
			3, 5, -1, -1, -1, -1
		},
		{
			9, 4, 5, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 11, 7, 4, 9, 11, 9, 10, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 4, 9, 7, 9, 11, 7, 9,
			10, 11, -1, -1, -1, -1
		},
		{
			1, 10, 11, 1, 11, 4, 1, 4, 0, 7,
			4, 11, -1, -1, -1, -1
		},
		{
			3, 1, 4, 3, 4, 8, 1, 10, 4, 7,
			4, 11, 10, 11, 4, -1
		},
		{
			4, 11, 7, 9, 11, 4, 9, 2, 11, 9,
			1, 2, -1, -1, -1, -1
		},
		{
			9, 7, 4, 9, 11, 7, 9, 1, 11, 2,
			11, 1, 0, 8, 3, -1
		},
		{
			11, 7, 4, 11, 4, 2, 2, 4, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 7, 4, 11, 4, 2, 8, 3, 4, 3,
			2, 4, -1, -1, -1, -1
		},
		{
			2, 9, 10, 2, 7, 9, 2, 3, 7, 7,
			4, 9, -1, -1, -1, -1
		},
		{
			9, 10, 7, 9, 7, 4, 10, 2, 7, 8,
			7, 0, 2, 0, 7, -1
		},
		{
			3, 7, 10, 3, 10, 2, 7, 4, 10, 1,
			10, 0, 4, 0, 10, -1
		},
		{
			1, 10, 2, 8, 7, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 1, 4, 1, 7, 7, 1, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 1, 4, 1, 7, 0, 8, 1, 8,
			7, 1, -1, -1, -1, -1
		},
		{
			4, 0, 3, 7, 4, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 8, 7, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 10, 8, 10, 11, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 9, 3, 9, 11, 11, 9, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 10, 0, 10, 8, 8, 10, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 1, 10, 11, 3, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 11, 1, 11, 9, 9, 11, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 9, 3, 9, 11, 1, 2, 9, 2,
			11, 9, -1, -1, -1, -1
		},
		{
			0, 2, 11, 8, 0, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 2, 11, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 3, 8, 2, 8, 10, 10, 8, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 10, 2, 0, 9, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 3, 8, 2, 8, 10, 0, 1, 8, 1,
			10, 8, -1, -1, -1, -1
		},
		{
			1, 10, 2, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 3, 8, 9, 1, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 9, 1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 3, 8, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		}
	};

	public MarchingCubesPro()
	{
		Implicit = new ImplicitSphere3d();
		Bounds = new AxisAlignedBox3d(Vector3d.Zero, 8.0);
		CubeSize = 0.25;
	}

	public void Generate()
	{
		Mesh = new DMesh3();
		int num = (int)(Bounds.Width / CubeSize) + 1;
		int num2 = (int)(Bounds.Height / CubeSize) + 1;
		int num3 = (int)(Bounds.Depth / CubeSize) + 1;
		CellDimensions = new Vector3i(num, num2, num3);
		GridBounds = new AxisAlignedBox3i(Vector3i.Zero, CellDimensions);
		corner_values_grid = new DenseGrid3f(num + 1, num2 + 1, num3 + 1, float.MaxValue);
		edge_vertices = new Dictionary<long, int>();
		corner_values = new Dictionary<long, double>();
		if (ParallelCompute)
		{
			generate_parallel();
		}
		else
		{
			generate_basic();
		}
	}

	public void GenerateContinuation(IEnumerable<Vector3d> seeds)
	{
		Mesh = new DMesh3();
		int num = (int)(Bounds.Width / CubeSize) + 1;
		int num2 = (int)(Bounds.Height / CubeSize) + 1;
		int num3 = (int)(Bounds.Depth / CubeSize) + 1;
		CellDimensions = new Vector3i(num, num2, num3);
		GridBounds = new AxisAlignedBox3i(Vector3i.Zero, CellDimensions);
		if (LastGridBounds != GridBounds)
		{
			corner_values_grid = new DenseGrid3f(num + 1, num2 + 1, num3 + 1, float.MaxValue);
			edge_vertices = new Dictionary<long, int>();
			corner_values = new Dictionary<long, double>();
			if (ParallelCompute)
			{
				done_cells = new DenseGrid3i(CellDimensions.x, CellDimensions.y, CellDimensions.z, 0);
			}
		}
		else
		{
			edge_vertices.Clear();
			corner_values.Clear();
			corner_values_grid.assign(float.MaxValue);
			if (ParallelCompute)
			{
				done_cells.assign(0);
			}
		}
		if (ParallelCompute)
		{
			generate_continuation_parallel(seeds);
		}
		else
		{
			generate_continuation(seeds);
		}
		LastGridBounds = GridBounds;
	}

	private void corner_pos(ref Vector3i ijk, ref Vector3d p)
	{
		p.x = Bounds.Min.x + CubeSize * (double)ijk.x;
		p.y = Bounds.Min.y + CubeSize * (double)ijk.y;
		p.z = Bounds.Min.z + CubeSize * (double)ijk.z;
	}

	private Vector3d corner_pos(ref Vector3i ijk)
	{
		return new Vector3d(Bounds.Min.x + CubeSize * (double)ijk.x, Bounds.Min.y + CubeSize * (double)ijk.y, Bounds.Min.z + CubeSize * (double)ijk.z);
	}

	private Vector3i cell_index(Vector3d pos)
	{
		return new Vector3i((int)((pos.x - Bounds.Min.x) / CubeSize), (int)((pos.y - Bounds.Min.y) / CubeSize), (int)((pos.z - Bounds.Min.z) / CubeSize));
	}

	private long corner_hash(ref Vector3i idx)
	{
		return ((long)idx.x & 0xFFFFL) | (long)(((ulong)idx.y & 0xFFFFuL) << 16) | (long)(((ulong)idx.z & 0xFFFFuL) << 32);
	}

	private long corner_hash(int x, int y, int z)
	{
		return ((long)x & 0xFFFFL) | (long)(((ulong)y & 0xFFFFuL) << 16) | (long)(((ulong)z & 0xFFFFuL) << 32);
	}

	private long edge_hash(ref Vector3i idx1, ref Vector3i idx2)
	{
		if (idx1.x != idx2.x)
		{
			int x = Math.Min(idx1.x, idx2.x);
			return corner_hash(x, idx1.y, idx1.z) | 0x10000000;
		}
		if (idx1.y != idx2.y)
		{
			int y = Math.Min(idx1.y, idx2.y);
			return corner_hash(idx1.x, y, idx1.z) | 0x20000000;
		}
		int z = Math.Min(idx1.z, idx2.z);
		return corner_hash(idx1.x, idx1.y, z) | 0x40000000;
	}

	private int edge_vertex_id(ref Vector3i idx1, ref Vector3i idx2, double f1, double f2)
	{
		long key = edge_hash(ref idx1, ref idx2);
		int value = -1;
		bool lockTaken = false;
		edge_vertices_lock.Enter(ref lockTaken);
		bool num = edge_vertices.TryGetValue(key, out value);
		edge_vertices_lock.Exit();
		if (num)
		{
			return value;
		}
		Vector3d p = Vector3d.Zero;
		Vector3d p2 = Vector3d.Zero;
		corner_pos(ref idx1, ref p);
		corner_pos(ref idx2, ref p2);
		Vector3d pIso = Vector3d.Zero;
		find_iso(ref p, ref p2, f1, f2, ref pIso);
		lockTaken = false;
		edge_vertices_lock.Enter(ref lockTaken);
		if (!edge_vertices.TryGetValue(key, out value))
		{
			value = append_vertex(pIso);
			edge_vertices[key] = value;
		}
		edge_vertices_lock.Exit();
		return value;
	}

	private double corner_value(ref Vector3i idx)
	{
		long key = corner_hash(ref idx);
		double value = 0.0;
		if (!corner_values.TryGetValue(key, out value))
		{
			Vector3d pt = corner_pos(ref idx);
			value = Implicit.Value(ref pt);
			corner_values[key] = value;
		}
		return value;
	}

	private void initialize_cell_values(GridCell cell, bool shift)
	{
		bool lockTaken = false;
		corner_values_lock.Enter(ref lockTaken);
		if (shift)
		{
			cell.f[1] = corner_value(ref cell.i[1]);
			cell.f[2] = corner_value(ref cell.i[2]);
			cell.f[5] = corner_value(ref cell.i[5]);
			cell.f[6] = corner_value(ref cell.i[6]);
		}
		else
		{
			for (int i = 0; i < 8; i++)
			{
				cell.f[i] = corner_value(ref cell.i[i]);
			}
		}
		corner_values_lock.Exit();
	}

	private double corner_value_grid(ref Vector3i idx)
	{
		double num = corner_values_grid[idx];
		if (num != 3.4028234663852886E+38)
		{
			return num;
		}
		Vector3d pt = corner_pos(ref idx);
		num = Implicit.Value(ref pt);
		corner_values_grid[idx] = (float)num;
		return num;
	}

	private void initialize_cell_values_grid(GridCell cell, bool shift)
	{
		if (shift)
		{
			cell.f[1] = corner_value_grid(ref cell.i[1]);
			cell.f[2] = corner_value_grid(ref cell.i[2]);
			cell.f[5] = corner_value_grid(ref cell.i[5]);
			cell.f[6] = corner_value_grid(ref cell.i[6]);
		}
		else
		{
			for (int i = 0; i < 8; i++)
			{
				cell.f[i] = corner_value_grid(ref cell.i[i]);
			}
		}
	}

	private double corner_value_nohash(ref Vector3i idx)
	{
		Vector3d pt = corner_pos(ref idx);
		return Implicit.Value(ref pt);
	}

	private void initialize_cell_values_nohash(GridCell cell, bool shift)
	{
		if (shift)
		{
			cell.f[1] = corner_value_nohash(ref cell.i[1]);
			cell.f[2] = corner_value_nohash(ref cell.i[2]);
			cell.f[5] = corner_value_nohash(ref cell.i[5]);
			cell.f[6] = corner_value_nohash(ref cell.i[6]);
		}
		else
		{
			for (int i = 0; i < 8; i++)
			{
				cell.f[i] = corner_value_nohash(ref cell.i[i]);
			}
		}
	}

	private void initialize_cell(GridCell cell, ref Vector3i idx)
	{
		cell.i[0] = new Vector3i(idx.x, idx.y, idx.z);
		cell.i[1] = new Vector3i(idx.x + 1, idx.y, idx.z);
		cell.i[2] = new Vector3i(idx.x + 1, idx.y, idx.z + 1);
		cell.i[3] = new Vector3i(idx.x, idx.y, idx.z + 1);
		cell.i[4] = new Vector3i(idx.x, idx.y + 1, idx.z);
		cell.i[5] = new Vector3i(idx.x + 1, idx.y + 1, idx.z);
		cell.i[6] = new Vector3i(idx.x + 1, idx.y + 1, idx.z + 1);
		cell.i[7] = new Vector3i(idx.x, idx.y + 1, idx.z + 1);
		initialize_cell_values_grid(cell, shift: false);
	}

	private void shift_cell_x(GridCell cell, int xi)
	{
		cell.f[0] = cell.f[1];
		cell.f[3] = cell.f[2];
		cell.f[4] = cell.f[5];
		cell.f[7] = cell.f[6];
		cell.i[0].x = xi;
		cell.i[1].x = xi + 1;
		cell.i[2].x = xi + 1;
		cell.i[3].x = xi;
		cell.i[4].x = xi;
		cell.i[5].x = xi + 1;
		cell.i[6].x = xi + 1;
		cell.i[7].x = xi;
		initialize_cell_values_grid(cell, shift: true);
	}

	private void generate_parallel()
	{
		mesh_lock = default(SpinLock);
		parallel_mesh_access = true;
		gParallel.ForEach(Interval1i.Range(CellDimensions.z), delegate(int zi)
		{
			GridCell cell = new GridCell();
			int[] vertIndexList = new int[12];
			for (int i = 0; i < CellDimensions.y; i++)
			{
				if (CancelF())
				{
					break;
				}
				Vector3i idx = new Vector3i(0, i, zi);
				initialize_cell(cell, ref idx);
				polygonize_cell(cell, vertIndexList);
				for (int j = 1; j < CellDimensions.x; j++)
				{
					shift_cell_x(cell, j);
					polygonize_cell(cell, vertIndexList);
				}
			}
		});
		parallel_mesh_access = false;
	}

	private void generate_basic()
	{
		GridCell cell = new GridCell();
		int[] vertIndexList = new int[12];
		for (int i = 0; i < CellDimensions.z; i++)
		{
			for (int j = 0; j < CellDimensions.y; j++)
			{
				if (CancelF())
				{
					return;
				}
				Vector3i idx = new Vector3i(0, j, i);
				initialize_cell(cell, ref idx);
				polygonize_cell(cell, vertIndexList);
				for (int k = 1; k < CellDimensions.x; k++)
				{
					shift_cell_x(cell, k);
					polygonize_cell(cell, vertIndexList);
				}
			}
		}
	}

	private void generate_continuation(IEnumerable<Vector3d> seeds)
	{
		GridCell cell = new GridCell();
		int[] vertIndexList = new int[12];
		done_cells = new DenseGrid3i(CellDimensions.x, CellDimensions.y, CellDimensions.z, 0);
		List<Vector3i> list = new List<Vector3i>();
		foreach (Vector3d seed in seeds)
		{
			Vector3i vector3i = cell_index(seed);
			if (done_cells[vector3i] == 1)
			{
				continue;
			}
			list.Add(vector3i);
			done_cells[vector3i] = 1;
			while (list.Count > 0)
			{
				Vector3i idx = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				if (CancelF())
				{
					return;
				}
				initialize_cell(cell, ref idx);
				if (!polygonize_cell(cell, vertIndexList))
				{
					continue;
				}
				Vector3i[] gridOffsets = gIndices.GridOffsets6;
				foreach (Vector3i vector3i2 in gridOffsets)
				{
					Vector3i vector3i3 = idx + vector3i2;
					if (GridBounds.Contains(vector3i3) && done_cells[vector3i3] == 0)
					{
						list.Add(vector3i3);
						done_cells[vector3i3] = 1;
					}
				}
			}
		}
	}

	private void generate_continuation_parallel(IEnumerable<Vector3d> seeds)
	{
		mesh_lock = default(SpinLock);
		parallel_mesh_access = true;
		gParallel.ForEach(seeds, delegate(Vector3d seed)
		{
			Vector3i idx = cell_index(seed);
			if (set_cell_if_not_done(ref idx))
			{
				GridCell cell = new GridCell();
				int[] vertIndexList = new int[12];
				List<Vector3i> list = new List<Vector3i> { idx };
				while (list.Count > 0)
				{
					Vector3i idx2 = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
					if (CancelF())
					{
						break;
					}
					initialize_cell(cell, ref idx2);
					if (polygonize_cell(cell, vertIndexList))
					{
						Vector3i[] gridOffsets = gIndices.GridOffsets6;
						foreach (Vector3i vector3i in gridOffsets)
						{
							Vector3i idx3 = idx2 + vector3i;
							if (GridBounds.Contains(idx3) && set_cell_if_not_done(ref idx3))
							{
								list.Add(idx3);
							}
						}
					}
				}
			}
		});
		parallel_mesh_access = false;
	}

	private bool set_cell_if_not_done(ref Vector3i idx)
	{
		bool result = false;
		bool lockTaken = false;
		done_cells_lock.Enter(ref lockTaken);
		if (done_cells[idx] == 0)
		{
			done_cells[idx] = 1;
			result = true;
		}
		done_cells_lock.Exit();
		return result;
	}

	private bool polygonize_cell(GridCell cell, int[] vertIndexList)
	{
		int num = 0;
		int num2 = 1;
		for (int i = 0; i < 8; i++)
		{
			if (cell.f[i] < IsoValue)
			{
				num |= num2;
			}
			num2 <<= 1;
		}
		if (edgeTable[num] == 0)
		{
			return false;
		}
		num2 = 1;
		_ = Vector3d.Zero;
		_ = Vector3d.Zero;
		for (int j = 0; j <= 11; j++)
		{
			if ((edgeTable[num] & num2) != 0)
			{
				int num3 = edge_indices[j, 0];
				int num4 = edge_indices[j, 1];
				vertIndexList[j] = edge_vertex_id(ref cell.i[num3], ref cell.i[num4], cell.f[num3], cell.f[num4]);
			}
			num2 <<= 1;
		}
		int num5 = 0;
		for (int k = 0; triTable[num, k] != -1; k += 3)
		{
			int num6 = triTable[num, k];
			int num7 = triTable[num, k + 1];
			int num8 = triTable[num, k + 2];
			int num9 = vertIndexList[num6];
			int num10 = vertIndexList[num7];
			int num11 = vertIndexList[num8];
			if (num9 != num10 && num9 != num11 && num10 != num11)
			{
				append_triangle(num9, num10, num11);
				num5++;
			}
		}
		return num5 > 0;
	}

	private int append_vertex(Vector3d v)
	{
		bool lockTaken = false;
		if (parallel_mesh_access)
		{
			mesh_lock.Enter(ref lockTaken);
		}
		int result = Mesh.AppendVertex(v);
		if (lockTaken)
		{
			mesh_lock.Exit();
		}
		return result;
	}

	private int append_triangle(int a, int b, int c)
	{
		bool lockTaken = false;
		if (parallel_mesh_access)
		{
			mesh_lock.Enter(ref lockTaken);
		}
		int result = Mesh.AppendTriangle(a, b, c);
		if (lockTaken)
		{
			mesh_lock.Exit();
		}
		return result;
	}

	private void find_iso(ref Vector3d p1, ref Vector3d p2, double valp1, double valp2, ref Vector3d pIso)
	{
		if (Math.Abs(valp1 - valp2) < 1E-05)
		{
			pIso = (p1 + p2) * 0.5;
			return;
		}
		if (Math.Abs(IsoValue - valp1) < 1E-05)
		{
			pIso = 0.999999 * p1 + 1.0000000000287557E-06 * p2;
			return;
		}
		if (Math.Abs(IsoValue - valp2) < 1E-05)
		{
			pIso = 0.999999 * p2 + 1.0000000000287557E-06 * p1;
			return;
		}
		Vector3d a = p1;
		Vector3d b = p2;
		double num = valp1;
		double num2 = valp2;
		if (valp2 < valp1)
		{
			a = p2;
			b = p1;
			num2 = valp1;
			num = valp2;
		}
		if (RootMode == RootfindingModes.Bisection)
		{
			for (int i = 0; i < RootModeSteps; i++)
			{
				pIso.x = (a.x + b.x) * 0.5;
				pIso.y = (a.y + b.y) * 0.5;
				pIso.z = (a.z + b.z) * 0.5;
				double num3 = Implicit.Value(ref pIso);
				if (num3 < IsoValue)
				{
					a = pIso;
					num = num3;
				}
				else
				{
					b = pIso;
					num2 = num3;
				}
			}
			pIso = Vector3d.Lerp(a, b, 0.5);
			return;
		}
		double num4 = 0.0;
		if (RootMode == RootfindingModes.LerpSteps)
		{
			for (int j = 0; j < RootModeSteps; j++)
			{
				num4 = (IsoValue - num) / (num2 - num);
				pIso.x = a.x + num4 * (b.x - a.x);
				pIso.y = a.y + num4 * (b.y - a.y);
				pIso.z = a.z + num4 * (b.z - a.z);
				double num5 = Implicit.Value(ref pIso);
				if (num5 < IsoValue)
				{
					a = pIso;
					num = num5;
				}
				else
				{
					b = pIso;
					num2 = num5;
				}
			}
		}
		num4 = (IsoValue - num) / (num2 - num);
		pIso.x = a.x + num4 * (b.x - a.x);
		pIso.y = a.y + num4 * (b.y - a.y);
		pIso.z = a.z + num4 * (b.z - a.z);
	}
}
