using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Liv.Lck.Cosmetics;

public static class LckCosmeticUtils
{
	public struct CosmeticRootInfo
	{
		public string RootPath;

		public string Type;
	}

	public static List<CosmeticRootInfo> ParseRootsFromMetadata(IReadOnlyDictionary<string, object> metadata)
	{
		if (!metadata.TryGetValue("CosmeticRoots", out var value))
		{
			Debug.LogWarning("LCK: Cosmetic metadata does not include top level CosmeticRoots key");
			return null;
		}
		if (!(value is IList<object> list))
		{
			Debug.LogWarning($"LCK: Cosmetic metadata has invalid CosmeticRoots - unexpected type: {value.GetType()}");
			return null;
		}
		List<CosmeticRootInfo> list2 = new List<CosmeticRootInfo>();
		foreach (object item in list)
		{
			object value2;
			object value3;
			if (!(item is IReadOnlyDictionary<object, object> readOnlyDictionary))
			{
				Debug.LogWarning($"LCK: Cosmetic metadata has invalid CosmeticRoots item - unexpected type: {item.GetType()}");
			}
			else if (readOnlyDictionary.TryGetValue("rootPath", out value2) && value2 is string rootPath && readOnlyDictionary.TryGetValue("type", out value3) && value3 is string type)
			{
				list2.Add(new CosmeticRootInfo
				{
					RootPath = rootPath,
					Type = type
				});
			}
			else
			{
				Debug.LogWarning("LCK: Cosmetic metadata has invalid CosmeticRoots item - missing rootPath / type");
			}
		}
		return list2;
	}

	public static List<CosmeticRootInfo> ParseRootsFromTomlString(string tomlContent)
	{
		List<CosmeticRootInfo> list = new List<CosmeticRootInfo>();
		string[] array = tomlContent.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		string text = null;
		string text2 = null;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text3 = array2[i].Trim();
			if (text3.StartsWith("[[CosmeticRoots]]"))
			{
				text = null;
				text2 = null;
			}
			else if (text3.StartsWith("rootPath"))
			{
				text = text3.Split(new char[1] { '=' }, 2)[1].Trim().Trim('"');
			}
			else if (text3.StartsWith("type"))
			{
				text2 = text3.Split(new char[1] { '=' }, 2)[1].Trim().Trim('"');
			}
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
			{
				list.Add(new CosmeticRootInfo
				{
					RootPath = text,
					Type = text2
				});
				text = null;
				text2 = null;
			}
		}
		return list;
	}

	public static async Task<List<UnityEngine.Object>> LoadRootsFromBundleAsync(AssetBundle bundle, List<CosmeticRootInfo> rootInfos, string cosmeticIdForLogging)
	{
		List<AssetBundleRequest> loadingRequests = new List<AssetBundleRequest>();
		foreach (CosmeticRootInfo rootInfo in rootInfos)
		{
			Type type = ResolveType(rootInfo.Type);
			if (type == null)
			{
				Debug.LogWarning("LCK: Could not resolve type '" + rootInfo.Type + "' for asset '" + rootInfo.RootPath + "' in cosmetic '" + cosmeticIdForLogging + "'. Skipping.");
			}
			else
			{
				loadingRequests.Add(bundle.LoadAssetAsync(rootInfo.RootPath, type));
			}
		}
		await Task.WhenAll(loadingRequests.Select((AssetBundleRequest req) => req.AsTask()).ToArray());
		return (from req in loadingRequests
			where req.asset != null
			select req.asset).ToList();
	}

	public static List<UnityEngine.Object> LoadRootsFromBundle(AssetBundle bundle, List<CosmeticRootInfo> rootInfos, string cosmeticIdForLogging)
	{
		List<UnityEngine.Object> list = new List<UnityEngine.Object>();
		foreach (CosmeticRootInfo rootInfo in rootInfos)
		{
			Type type = ResolveType(rootInfo.Type);
			if (type == null)
			{
				Debug.LogWarning("LCK: Could not resolve type '" + rootInfo.Type + "' for asset '" + rootInfo.RootPath + "' in cosmetic '" + cosmeticIdForLogging + "'. Skipping.");
				continue;
			}
			UnityEngine.Object @object = bundle.LoadAsset(rootInfo.RootPath, type);
			if (@object != null)
			{
				list.Add(@object);
				continue;
			}
			Debug.LogWarning("LCK: Failed to load asset at path '" + rootInfo.RootPath + "' of type '" + type.Name + "' from bundle for cosmetic '" + cosmeticIdForLogging + "'.");
		}
		return list;
	}

	private static Type ResolveType(string typeName)
	{
		Type type = Type.GetType(typeName);
		if (type != null)
		{
			return type;
		}
		type = Type.GetType("UnityEngine." + typeName + ", UnityEngine.CoreModule");
		if (type != null)
		{
			return type;
		}
		type = Type.GetType("UnityEngine." + typeName + ", UnityEngine");
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType("UnityEngine." + typeName);
			if (type != null)
			{
				return type;
			}
		}
		Debug.LogWarning("LCK: Could not resolve type '" + typeName + "' in any loaded assembly.");
		return null;
	}
}
