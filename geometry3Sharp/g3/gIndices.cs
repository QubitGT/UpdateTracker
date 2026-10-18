using System.Collections.Generic;

namespace g3;

public static class gIndices
{
	public static readonly Vector2i[] GridOffsets4 = new Vector2i[4]
	{
		new Vector2i(-1, 0),
		new Vector2i(1, 0),
		new Vector2i(0, -1),
		new Vector2i(0, 1)
	};

	public static readonly Vector2i[] GridOffsets8 = new Vector2i[8]
	{
		new Vector2i(-1, 0),
		new Vector2i(1, 0),
		new Vector2i(0, -1),
		new Vector2i(0, 1),
		new Vector2i(-1, 1),
		new Vector2i(1, 1),
		new Vector2i(-1, -1),
		new Vector2i(1, -1)
	};

	public static readonly int[,] BoxFaces = new int[6, 4]
	{
		{ 1, 0, 3, 2 },
		{ 4, 5, 6, 7 },
		{ 0, 4, 7, 3 },
		{ 5, 1, 2, 6 },
		{ 0, 1, 5, 4 },
		{ 7, 6, 2, 3 }
	};

	public static readonly int[] BoxFaceNormals = new int[6] { -3, 3, -1, 1, -2, 2 };

	public static readonly Vector3i[] GridOffsets6 = new Vector3i[6]
	{
		new Vector3i(0, 0, -1),
		new Vector3i(0, 0, 1),
		new Vector3i(-1, 0, 0),
		new Vector3i(1, 0, 0),
		new Vector3i(0, -1, 0),
		new Vector3i(0, 1, 0)
	};

	public static readonly Vector3i[] GridOffsets26 = new Vector3i[26]
	{
		new Vector3i(0, 0, -1),
		new Vector3i(0, 0, 1),
		new Vector3i(-1, 0, 0),
		new Vector3i(1, 0, 0),
		new Vector3i(0, -1, 0),
		new Vector3i(0, 1, 0),
		new Vector3i(1, 1, 0),
		new Vector3i(-1, 1, 0),
		new Vector3i(0, 1, 1),
		new Vector3i(0, 1, -1),
		new Vector3i(1, 0, 1),
		new Vector3i(-1, 0, 1),
		new Vector3i(1, 0, -1),
		new Vector3i(-1, 0, -1),
		new Vector3i(1, -1, 0),
		new Vector3i(-1, -1, 0),
		new Vector3i(0, -1, 1),
		new Vector3i(0, -1, -1),
		new Vector3i(1, 1, 1),
		new Vector3i(-1, 1, 1),
		new Vector3i(1, 1, -1),
		new Vector3i(-1, 1, -1),
		new Vector3i(1, -1, 1),
		new Vector3i(-1, -1, 1),
		new Vector3i(1, -1, -1),
		new Vector3i(-1, -1, -1)
	};

	public static IEnumerable<Vector3i> Grid3Indices(int nx, int ny, int nz)
	{
		int z = 0;
		while (z < nz)
		{
			int num;
			for (int y = 0; y < ny; y = num)
			{
				for (int x = 0; x < nx; x = num)
				{
					yield return new Vector3i(x, y, z);
					num = x + 1;
				}
				num = y + 1;
			}
			num = z + 1;
			z = num;
		}
	}

	public static IEnumerable<Vector3i> Grid3IndicesYZ(int ny, int nz)
	{
		int z = 0;
		while (z < nz)
		{
			int num;
			for (int y = 0; y < ny; y = num)
			{
				yield return new Vector3i(0, y, z);
				num = y + 1;
			}
			num = z + 1;
			z = num;
		}
	}

	public static IEnumerable<Vector3i> Grid3IndicesXZ(int nx, int nz)
	{
		int z = 0;
		while (z < nz)
		{
			int num;
			for (int x = 0; x < nx; x = num)
			{
				yield return new Vector3i(x, 0, z);
				num = x + 1;
			}
			num = z + 1;
			z = num;
		}
	}

	public static IEnumerable<Vector3i> Grid3IndicesXY(int nx, int ny)
	{
		int y = 0;
		while (y < ny)
		{
			int num;
			for (int x = 0; x < nx; x = num)
			{
				yield return new Vector3i(x, y, 0);
				num = x + 1;
			}
			num = y + 1;
			y = num;
		}
	}
}
