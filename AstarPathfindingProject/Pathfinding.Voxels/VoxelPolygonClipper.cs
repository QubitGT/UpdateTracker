using UnityEngine;

namespace Pathfinding.Voxels;

internal struct VoxelPolygonClipper
{
	public float[] x;

	public float[] y;

	public float[] z;

	public int n;

	public Vector3 this[int i]
	{
		set
		{
			x[i] = value.x;
			y[i] = value.y;
			z[i] = value.z;
		}
	}

	public VoxelPolygonClipper(int capacity)
	{
		x = new float[capacity];
		y = new float[capacity];
		z = new float[capacity];
		n = 0;
	}

	public void ClipPolygonAlongX(ref VoxelPolygonClipper result, float multi, float offset)
	{
		int num = 0;
		float num2 = multi * x[n - 1] + offset;
		int i = 0;
		int num3 = n - 1;
		for (; i < n; i++)
		{
			float num4 = multi * x[i] + offset;
			bool num5 = num2 >= 0f;
			bool flag = num4 >= 0f;
			if (num5 != flag)
			{
				float num6 = num2 / (num2 - num4);
				result.x[num] = x[num3] + (x[i] - x[num3]) * num6;
				result.y[num] = y[num3] + (y[i] - y[num3]) * num6;
				result.z[num] = z[num3] + (z[i] - z[num3]) * num6;
				num++;
			}
			if (flag)
			{
				result.x[num] = x[i];
				result.y[num] = y[i];
				result.z[num] = z[i];
				num++;
			}
			num2 = num4;
			num3 = i;
		}
		result.n = num;
	}

	public void ClipPolygonAlongZWithYZ(ref VoxelPolygonClipper result, float multi, float offset)
	{
		int num = 0;
		float num2 = multi * z[n - 1] + offset;
		int i = 0;
		int num3 = n - 1;
		for (; i < n; i++)
		{
			float num4 = multi * z[i] + offset;
			bool num5 = num2 >= 0f;
			bool flag = num4 >= 0f;
			if (num5 != flag)
			{
				float num6 = num2 / (num2 - num4);
				result.y[num] = y[num3] + (y[i] - y[num3]) * num6;
				result.z[num] = z[num3] + (z[i] - z[num3]) * num6;
				num++;
			}
			if (flag)
			{
				result.y[num] = y[i];
				result.z[num] = z[i];
				num++;
			}
			num2 = num4;
			num3 = i;
		}
		result.n = num;
	}

	public void ClipPolygonAlongZWithY(ref VoxelPolygonClipper result, float multi, float offset)
	{
		int num = 0;
		float num2 = multi * z[n - 1] + offset;
		int i = 0;
		int num3 = n - 1;
		for (; i < n; i++)
		{
			float num4 = multi * z[i] + offset;
			bool num5 = num2 >= 0f;
			bool flag = num4 >= 0f;
			if (num5 != flag)
			{
				float num6 = num2 / (num2 - num4);
				result.y[num] = y[num3] + (y[i] - y[num3]) * num6;
				num++;
			}
			if (flag)
			{
				result.y[num] = y[i];
				num++;
			}
			num2 = num4;
			num3 = i;
		}
		result.n = num;
	}
}
