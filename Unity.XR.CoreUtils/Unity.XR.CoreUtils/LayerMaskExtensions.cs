using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class LayerMaskExtensions
{
	public static int GetFirstLayerIndex(this LayerMask layerMask)
	{
		if (layerMask.value == 0)
		{
			return -1;
		}
		int num = 0;
		int num2 = layerMask.value;
		while ((num2 & 1) == 0)
		{
			num2 >>= 1;
			num++;
		}
		return num;
	}

	public static bool Contains(this LayerMask mask, int layer)
	{
		return ((uint)(int)mask & (1 << layer)) > 0;
	}
}
