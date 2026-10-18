namespace Pathfinding.Voxels;

internal struct Int3PolygonClipper
{
	private float[] clipPolygonCache;

	private int[] clipPolygonIntCache;

	public void Init()
	{
		if (clipPolygonCache == null)
		{
			clipPolygonCache = new float[21];
			clipPolygonIntCache = new int[21];
		}
	}

	public int ClipPolygon(Int3[] vIn, int n, Int3[] vOut, int multi, int offset, int axis)
	{
		Init();
		int[] array = clipPolygonIntCache;
		for (int i = 0; i < n; i++)
		{
			array[i] = multi * vIn[i][axis] + offset;
		}
		int num = 0;
		int j = 0;
		int num2 = n - 1;
		for (; j < n; j++)
		{
			bool num3 = array[num2] >= 0;
			bool flag = array[j] >= 0;
			if (num3 != flag)
			{
				double num4 = (double)array[num2] / (double)(array[num2] - array[j]);
				vOut[num] = vIn[num2] + (vIn[j] - vIn[num2]) * num4;
				num++;
			}
			if (flag)
			{
				vOut[num] = vIn[j];
				num++;
			}
			num2 = j;
		}
		return num;
	}
}
