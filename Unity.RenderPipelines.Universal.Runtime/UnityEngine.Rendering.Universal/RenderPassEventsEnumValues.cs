using System;

namespace UnityEngine.Rendering.Universal;

internal static class RenderPassEventsEnumValues
{
	public static int[] values;

	static RenderPassEventsEnumValues()
	{
		Array array = Enum.GetValues(typeof(RenderPassEvent));
		values = new int[array.Length];
		int num = 0;
		foreach (int item in array)
		{
			values[num] = item;
			num++;
		}
	}
}
