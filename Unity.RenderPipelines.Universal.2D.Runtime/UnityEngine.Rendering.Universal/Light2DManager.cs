using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal static class Light2DManager
{
	private static SortingLayer[] s_SortingLayers;

	public static List<Light2D> lights { get; } = new List<Light2D>();


	internal static void Initialize()
	{
	}

	internal static void Dispose()
	{
	}

	public static void RegisterLight(Light2D light)
	{
		lights.Add(light);
		ErrorIfDuplicateGlobalLight(light);
	}

	public static void DeregisterLight(Light2D light)
	{
		lights.Remove(light);
	}

	public static void ErrorIfDuplicateGlobalLight(Light2D light)
	{
		if (light.lightType != Light2D.LightType.Global)
		{
			return;
		}
		int[] targetSortingLayers = light.targetSortingLayers;
		foreach (int num in targetSortingLayers)
		{
			if (ContainsDuplicateGlobalLight(num, light.blendStyleIndex))
			{
				Debug.LogError("More than one global light on layer " + SortingLayer.IDToName(num) + " for light blend style index " + light.blendStyleIndex);
			}
		}
	}

	public static bool GetGlobalColor(int sortingLayerIndex, int blendStyleIndex, out Color color)
	{
		bool flag = false;
		color = Color.black;
		foreach (Light2D light in lights)
		{
			if (light.lightType == Light2D.LightType.Global && light.blendStyleIndex == blendStyleIndex && light.IsLitLayer(sortingLayerIndex))
			{
				if (true)
				{
					color = light.color * light.intensity;
					return true;
				}
				if (!flag)
				{
					color = light.color * light.intensity;
					flag = true;
				}
			}
		}
		return flag;
	}

	private static bool ContainsDuplicateGlobalLight(int sortingLayerIndex, int blendStyleIndex)
	{
		int num = 0;
		foreach (Light2D light in lights)
		{
			if (light.lightType == Light2D.LightType.Global && light.blendStyleIndex == blendStyleIndex && light.IsLitLayer(sortingLayerIndex))
			{
				if (num > 0)
				{
					return true;
				}
				num++;
			}
		}
		return false;
	}

	public static SortingLayer[] GetCachedSortingLayer()
	{
		if (s_SortingLayers == null)
		{
			s_SortingLayers = SortingLayer.layers;
		}
		return s_SortingLayers;
	}
}
