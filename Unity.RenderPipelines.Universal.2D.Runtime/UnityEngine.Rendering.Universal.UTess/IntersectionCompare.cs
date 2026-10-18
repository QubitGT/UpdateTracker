using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

internal struct IntersectionCompare : IComparer<int2>
{
	public NativeArray<double2> points;

	public NativeArray<int2> edges;

	public unsafe fixed double xvasort[4];

	public unsafe fixed double xvbsort[4];

	public unsafe int Compare(int2 a, int2 b)
	{
		int2 @int = edges[a.x];
		int2 int2 = edges[a.y];
		int2 int3 = edges[b.x];
		int2 int4 = edges[b.y];
		ref double fixedElementField = ref xvasort[0];
		fixedElementField = points[@int.x].x;
		xvasort[1] = points[@int.y].x;
		xvasort[2] = points[int2.x].x;
		xvasort[3] = points[int2.y].x;
		ref double fixedElementField2 = ref xvbsort[0];
		fixedElementField2 = points[int3.x].x;
		xvbsort[1] = points[int3.y].x;
		xvbsort[2] = points[int4.x].x;
		xvbsort[3] = points[int4.y].x;
		fixed (double* array = xvasort)
		{
			ModuleHandle.InsertionSort<double, XCompare>(array, 0, 3, default(XCompare));
		}
		fixed (double* array = xvbsort)
		{
			ModuleHandle.InsertionSort<double, XCompare>(array, 0, 3, default(XCompare));
		}
		for (int i = 0; i < 4; i++)
		{
			if (xvasort[i] - xvbsort[i] != 0.0)
			{
				if (!(xvasort[i] < xvbsort[i]))
				{
					return 1;
				}
				return -1;
			}
		}
		if (!(points[@int.x].y < points[@int.x].y))
		{
			return 1;
		}
		return -1;
	}
}
