using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine.Rendering.Universal.UTess;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct TessEventCompare : IComparer<UEvent>
{
	public int Compare(UEvent a, UEvent b)
	{
		float num = a.a.x - b.a.x;
		if (0f != num)
		{
			if (!(num > 0f))
			{
				return -1;
			}
			return 1;
		}
		num = a.a.y - b.a.y;
		if (0f != num)
		{
			if (!(num > 0f))
			{
				return -1;
			}
			return 1;
		}
		int num2 = a.type - b.type;
		if (num2 != 0)
		{
			return num2;
		}
		if (a.type != 0)
		{
			float num3 = ModuleHandle.OrientFast(a.a, a.b, b.b);
			if (0f != num3)
			{
				if (!(num3 > 0f))
				{
					return -1;
				}
				return 1;
			}
		}
		return a.idx - b.idx;
	}
}
