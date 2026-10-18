using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class GameObjectExtensions
{
	public static void SetHideFlagsRecursively(this GameObject gameObject, HideFlags hideFlags)
	{
		gameObject.hideFlags = hideFlags;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetHideFlagsRecursively(hideFlags);
		}
	}

	public static void AddToHideFlagsRecursively(this GameObject gameObject, HideFlags hideFlags)
	{
		gameObject.hideFlags |= hideFlags;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.AddToHideFlagsRecursively(hideFlags);
		}
	}

	public static void SetLayerRecursively(this GameObject gameObject, int layer)
	{
		gameObject.layer = layer;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetLayerRecursively(layer);
		}
	}

	public static void SetLayerAndAddToHideFlagsRecursively(this GameObject gameObject, int layer, HideFlags hideFlags)
	{
		gameObject.layer = layer;
		gameObject.hideFlags |= hideFlags;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetLayerAndAddToHideFlagsRecursively(layer, hideFlags);
		}
	}

	public static void SetLayerAndHideFlagsRecursively(this GameObject gameObject, int layer, HideFlags hideFlags)
	{
		gameObject.layer = layer;
		gameObject.hideFlags = hideFlags;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetLayerAndHideFlagsRecursively(layer, hideFlags);
		}
	}

	public static void SetRunInEditModeRecursively(this GameObject gameObject, bool enabled)
	{
	}
}
