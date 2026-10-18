using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct TessCellCompare : IComparer<int3>
{
	public int Compare(int3 a, int3 b)
	{
		int num = a.x - b.x;
		if (num != 0)
		{
			return num;
		}
		num = a.y - b.y;
		if (num != 0)
		{
			return num;
		}
		return a.z - b.z;
	}
}
