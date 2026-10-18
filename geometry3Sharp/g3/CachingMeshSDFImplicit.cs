using System;

namespace g3;

public class CachingMeshSDFImplicit : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public CachingMeshSDF SDF;

	public double CellSize;

	public Vector3d GridOrigin;

	public double Outside = Math.Sqrt(Math.Sqrt(double.MaxValue));

	public CachingMeshSDFImplicit(CachingMeshSDF sdf)
	{
		SDF = sdf;
		GridOrigin = sdf.GridOrigin;
		CellSize = sdf.CellSize;
	}

	public AxisAlignedBox3d Bounds()
	{
		return new AxisAlignedBox3d(GridOrigin.x, GridOrigin.y, GridOrigin.z, GridOrigin.x + CellSize * (double)SDF.Grid.ni, GridOrigin.y + CellSize * (double)SDF.Grid.nj, GridOrigin.z + CellSize * (double)SDF.Grid.nk);
	}

	public double Value(ref Vector3d pt)
	{
		Vector3d vector3d = new Vector3d((pt.x - GridOrigin.x) / CellSize, (pt.y - GridOrigin.y) / CellSize, (pt.z - GridOrigin.z) / CellSize);
		int num = (int)vector3d.x;
		int num2 = (int)vector3d.y;
		int num3 = num2 + 1;
		int num4 = (int)vector3d.z;
		int num5 = num4 + 1;
		if (num < 0 || num + 1 >= SDF.Grid.ni || num2 < 0 || num3 >= SDF.Grid.nj || num4 < 0 || num5 >= SDF.Grid.nk)
		{
			return Outside;
		}
		double num6 = vector3d.x - (double)num;
		double num7 = vector3d.y - (double)num2;
		double num8 = vector3d.z - (double)num4;
		double num9 = 1.0 - num6;
		get_value_pair(num, num2, num4, out var a, out var b);
		double num10 = (1.0 - num7) * (1.0 - num8);
		double num11 = (num9 * a + num6 * b) * num10;
		get_value_pair(num, num2, num5, out a, out b);
		num10 = (1.0 - num7) * num8;
		double num12 = num11 + (num9 * a + num6 * b) * num10;
		get_value_pair(num, num3, num4, out a, out b);
		num10 = num7 * (1.0 - num8);
		double num13 = num12 + (num9 * a + num6 * b) * num10;
		get_value_pair(num, num3, num5, out a, out b);
		num10 = num7 * num8;
		return num13 + (num9 * a + num6 * b) * num10;
	}

	private void get_value_pair(int i, int j, int k, out double a, out double b)
	{
		a = SDF.GetValue(new Vector3i(i, j, k));
		b = SDF.GetValue(new Vector3i(i + 1, j, k));
	}

	public Vector3d Gradient(ref Vector3d pt)
	{
		Vector3d vector3d = new Vector3d((pt.x - GridOrigin.x) / CellSize, (pt.y - GridOrigin.y) / CellSize, (pt.z - GridOrigin.z) / CellSize);
		if (vector3d.x < 0.0 || vector3d.x >= (double)(SDF.Grid.ni - 1) || vector3d.y < 0.0 || vector3d.y >= (double)(SDF.Grid.nj - 1) || vector3d.z < 0.0 || vector3d.z >= (double)(SDF.Grid.nk - 1))
		{
			return Vector3d.Zero;
		}
		int num = (int)vector3d.x;
		int num2 = (int)vector3d.y;
		int j = num2 + 1;
		int num3 = (int)vector3d.z;
		int k = num3 + 1;
		double num4 = vector3d.x - (double)num;
		double num5 = vector3d.y - (double)num2;
		double num6 = vector3d.z - (double)num3;
		get_value_pair(num, num2, num3, out var a, out var b);
		get_value_pair(num, j, num3, out var a2, out var b2);
		get_value_pair(num, num2, k, out var a3, out var b3);
		get_value_pair(num, j, k, out var a4, out var b4);
		double x = (0.0 - a) * (1.0 - num5) * (1.0 - num6) + (0.0 - a3) * (1.0 - num5) * num6 + (0.0 - a2) * num5 * (1.0 - num6) + (0.0 - a4) * num5 * num6 + b * (1.0 - num5) * (1.0 - num6) + b3 * (1.0 - num5) * num6 + b2 * num5 * (1.0 - num6) + b4 * num5 * num6;
		double y = (0.0 - a) * (1.0 - num4) * (1.0 - num6) + (0.0 - a3) * (1.0 - num4) * num6 + a2 * (1.0 - num4) * (1.0 - num6) + a4 * (1.0 - num4) * num6 + (0.0 - b) * num4 * (1.0 - num6) + (0.0 - b3) * num4 * num6 + b2 * num4 * (1.0 - num6) + b4 * num4 * num6;
		double z = (0.0 - a) * (1.0 - num4) * (1.0 - num5) + a3 * (1.0 - num4) * (1.0 - num5) + (0.0 - a2) * (1.0 - num4) * num5 + a4 * (1.0 - num4) * num5 + (0.0 - b) * num4 * (1.0 - num5) + b3 * num4 * (1.0 - num5) + (0.0 - b2) * num4 * num5 + b4 * num4 * num5;
		return new Vector3d(x, y, z);
	}
}
