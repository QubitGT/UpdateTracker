using System.Collections.Generic;
using UnityEngine;

public class OVRResources : MonoBehaviour
{
	private static AssetBundle resourceBundle;

	private static List<string> assetNames;

	public static Object Load(string path)
	{
		if (Debug.isDebugBuild)
		{
			if (resourceBundle == null)
			{
				Debug.Log("[OVRResources] Resource bundle was not loaded successfully");
				return null;
			}
			string text = assetNames.Find((string s) => s.Contains(path.ToLower()));
			return resourceBundle.LoadAsset(text);
		}
		return Resources.Load(path);
	}

	public static T Load<T>(string path) where T : Object
	{
		if (Debug.isDebugBuild)
		{
			if (resourceBundle == null)
			{
				Debug.Log("[OVRResources] Resource bundle was not loaded successfully");
				return null;
			}
			string text = assetNames.Find((string s) => s.Contains(path.ToLower()));
			return resourceBundle.LoadAsset<T>(text);
		}
		return Resources.Load<T>(path);
	}

	public static void SetResourceBundle(AssetBundle bundle)
	{
		resourceBundle = bundle;
		assetNames = new List<string>();
		assetNames.AddRange(resourceBundle.GetAllAssetNames());
	}
}
