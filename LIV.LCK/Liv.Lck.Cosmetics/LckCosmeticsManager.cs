using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Liv.Lck.Core.Cosmetics;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck.Cosmetics;

[Preserve]
public class LckCosmeticsManager : ILckCosmeticsManager, IDisposable
{
	private struct CosmeticRootInfo
	{
		public string RootPath;

		public string Type;
	}

	private readonly ILckCosmeticsCoordinator _cosmeticsCoordinator;

	private readonly Dictionary<string, List<ILckCosmeticDependant>> _dependantRegistry = new Dictionary<string, List<ILckCosmeticDependant>>();

	private readonly Dictionary<string, LckAvailableCosmeticInfo> _availableCosmeticsCache = new Dictionary<string, LckAvailableCosmeticInfo>();

	private readonly Dictionary<string, List<UnityEngine.Object>> _loadedCosmeticCache = new Dictionary<string, List<UnityEngine.Object>>();

	private readonly Dictionary<string, Task<List<UnityEngine.Object>>> _loadingTasks = new Dictionary<string, Task<List<UnityEngine.Object>>>();

	private readonly Dictionary<string, AssetBundle> _loadedAssetBundles = new Dictionary<string, AssetBundle>();

	[Preserve]
	public LckCosmeticsManager(ILckCosmeticsCoordinator cosmeticsCoordinator)
	{
		_cosmeticsCoordinator = cosmeticsCoordinator;
		_cosmeticsCoordinator.OnCosmeticAvailable += HandleCosmeticAvailable;
		LckLog.Log("LCK: LckCosmeticsManager initialized.", ".ctor", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 36);
	}

	public void RegisterDependant(ILckCosmeticDependant dependant)
	{
		string cosmeticType = dependant.GetCosmeticType();
		if (!string.IsNullOrEmpty(cosmeticType))
		{
			if (!_dependantRegistry.TryGetValue(cosmeticType, out var value))
			{
				value = new List<ILckCosmeticDependant>();
				_dependantRegistry[cosmeticType] = value;
			}
			if (!value.Contains(dependant))
			{
				value.Add(dependant);
			}
			LckLog.Log("LCK: RegisterDependant of type " + cosmeticType + " for player " + dependant.PlayerId + ". ", "RegisterDependant", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 52);
			CheckAndApplyCachedCosmetics(dependant);
		}
	}

	public void UnregisterDependant(ILckCosmeticDependant dependant)
	{
		string cosmeticType = dependant.GetCosmeticType();
		if (!string.IsNullOrEmpty(cosmeticType) && _dependantRegistry.TryGetValue(cosmeticType, out var value))
		{
			LckLog.Log("LCK: UnregisterDependant of type " + cosmeticType + " for player " + dependant.PlayerId + ".", "UnregisterDependant", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 65);
			value.Remove(dependant);
		}
	}

	public void Dispose()
	{
		if (_cosmeticsCoordinator != null)
		{
			_cosmeticsCoordinator.OnCosmeticAvailable -= HandleCosmeticAvailable;
		}
		foreach (AssetBundle item in _loadedAssetBundles.Values.Where((AssetBundle bundle) => bundle != null))
		{
			item.Unload(unloadAllLoadedObjects: true);
		}
		_loadedAssetBundles.Clear();
		LckLog.Log("LCK: LckCosmeticsManager disposed.", "Dispose", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 83);
	}

	private async void HandleCosmeticAvailable(LckAvailableCosmeticInfo incomingCosmeticInfo)
	{
		try
		{
			UpdateAvailableCosmeticsCache(incomingCosmeticInfo);
			string cosmeticId = incomingCosmeticInfo.CosmeticInfo.CosmeticId;
			if (_loadedCosmeticCache.TryGetValue(cosmeticId, out var cachedAssets))
			{
				LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
				{
					DistributeLoadedCosmetic(incomingCosmeticInfo, cachedAssets);
				});
				return;
			}
			Task<List<UnityEngine.Object>> value;
			lock (_loadingTasks)
			{
				if (!_loadingTasks.TryGetValue(cosmeticId, out value))
				{
					TaskCompletionSource<List<UnityEngine.Object>> tcs = new TaskCompletionSource<List<UnityEngine.Object>>();
					value = tcs.Task;
					_loadingTasks.Add(cosmeticId, value);
					LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(async delegate
					{
						try
						{
							List<UnityEngine.Object> result = await LoadCosmetic(incomingCosmeticInfo.CosmeticInfo);
							tcs.SetResult(result);
						}
						catch (Exception exception)
						{
							tcs.SetException(exception);
						}
					});
				}
			}
			List<UnityEngine.Object> loadedAssets = await value;
			if (loadedAssets != null && loadedAssets.Count > 0)
			{
				LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
				{
					DistributeLoadedCosmetic(incomingCosmeticInfo, loadedAssets);
				});
			}
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK: HandleCosmeticAvailable failed with exception: " + ex.Message + ".", "HandleCosmeticAvailable", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 139);
		}
	}

	private void UpdateAvailableCosmeticsCache(LckAvailableCosmeticInfo cosmeticInfo)
	{
		lock (_availableCosmeticsCache)
		{
			string cosmeticId = cosmeticInfo.CosmeticInfo.CosmeticId;
			if (_availableCosmeticsCache.TryGetValue(cosmeticId, out var value))
			{
				HashSet<string> hashSet = new HashSet<string>(value.PlayerIds);
				hashSet.UnionWith(cosmeticInfo.PlayerIds);
				value.PlayerIds = hashSet.ToArray();
				_availableCosmeticsCache[cosmeticId] = value;
			}
			else
			{
				_availableCosmeticsCache[cosmeticId] = new LckAvailableCosmeticInfo
				{
					CosmeticInfo = cosmeticInfo.CosmeticInfo,
					PlayerIds = cosmeticInfo.PlayerIds.ToArray()
				};
			}
		}
	}

	private async Task<List<UnityEngine.Object>> LoadCosmetic(LckCosmeticInfo cosmeticInfo)
	{
		string cosmeticId = cosmeticInfo.CosmeticId;
		try
		{
			string text = Path.GetExtension(cosmeticInfo.CosmeticFilepath)?.ToLowerInvariant();
			if (!(text == ".lckcosmeticbundle"))
			{
				if (text == ".glb")
				{
					LckLog.LogWarning("LCK: Support for loading '" + cosmeticId + "' from a .glb file is not yet implemented.", "LoadCosmetic", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 180);
					return null;
				}
				LckLog.LogError("LCK: Unrecognised cosmetic file extension '" + text + "' for cosmetic '" + cosmeticId + "'. Cannot load.", "LoadCosmetic", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 184);
				return null;
			}
			return await LoadRootsFromAssetBundleAsync(cosmeticInfo);
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error: Exception while loading asset " + cosmeticId + ". " + ex.Message, "LoadCosmetic", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 190);
			return null;
		}
		finally
		{
			lock (_loadingTasks)
			{
				_loadingTasks.Remove(cosmeticId);
			}
		}
	}

	private async Task<List<UnityEngine.Object>> LoadRootsFromAssetBundleAsync(LckCosmeticInfo cosmeticInfo)
	{
		string cosmeticId = cosmeticInfo.CosmeticId;
		string bundlePath = cosmeticInfo.CosmeticFilepath;
		AssetBundle assetBundle;
		if (_loadedAssetBundles.TryGetValue(bundlePath, out var value))
		{
			assetBundle = value;
		}
		else
		{
			AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(bundlePath);
			await bundleLoadRequest;
			assetBundle = bundleLoadRequest.assetBundle;
			if (assetBundle == null)
			{
				LckLog.LogError("LCK: Failed to load AssetBundle for cosmetic '" + cosmeticId + "' from: " + bundlePath, "LoadRootsFromAssetBundleAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 220);
				return null;
			}
			_loadedAssetBundles[bundlePath] = assetBundle;
		}
		List<LckCosmeticUtils.CosmeticRootInfo> list = LckCosmeticUtils.ParseRootsFromMetadata(cosmeticInfo.CosmeticMetadata);
		if (list == null || list.Count == 0)
		{
			LckLog.LogError("LCK: Cosmetic '" + cosmeticId + "' has no CosmeticRoots defined in its metadata.", "LoadRootsFromAssetBundleAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 229);
			return null;
		}
		List<UnityEngine.Object> list2 = await LckCosmeticUtils.LoadRootsFromBundleAsync(assetBundle, list, cosmeticId);
		if (list2.Count == 0)
		{
			LckLog.LogError("LCK: Failed to load ANY assets for cosmetic '" + cosmeticId + "'.", "LoadRootsFromAssetBundleAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 236);
			return null;
		}
		_loadedCosmeticCache[cosmeticId] = list2;
		LckLog.Log($"LCK: Successfully loaded and cached {list2.Count} assets for cosmetic '{cosmeticId}'.", "LoadRootsFromAssetBundleAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 241);
		return list2;
	}

	private void DistributeLoadedCosmetic(LckAvailableCosmeticInfo cosmeticInfo, List<UnityEngine.Object> assets)
	{
		if (cosmeticInfo.CosmeticInfo.CosmeticMetadata.TryGetValue("CosmeticType", out var value))
		{
			string text = value.ToString();
			if (text != null)
			{
				if (!_dependantRegistry.TryGetValue(text, out var value2))
				{
					return;
				}
				HashSet<string> entitledPlayerIds = new HashSet<string>(cosmeticInfo.PlayerIds);
				{
					foreach (ILckCosmeticDependant item in value2.Where((ILckCosmeticDependant d) => entitledPlayerIds.Contains(d.PlayerId)))
					{
						item.OnCosmeticLoaded(assets);
					}
					return;
				}
			}
		}
		LckLog.LogError("LCK: Failed to find CosmeticType in metadata of " + cosmeticInfo.CosmeticInfo.CosmeticId + ".", "DistributeLoadedCosmetic", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 251);
	}

	private void CheckAndApplyCachedCosmetics(ILckCosmeticDependant dependant)
	{
		string cosmeticType = dependant.GetCosmeticType();
		string playerId = dependant.PlayerId;
		lock (_availableCosmeticsCache)
		{
			foreach (LckAvailableCosmeticInfo value3 in _availableCosmeticsCache.Values)
			{
				LckCosmeticInfo cosmeticInfo = value3.CosmeticInfo;
				if (cosmeticInfo.CosmeticMetadata.TryGetValue("CosmeticType", out var value) && !(value.ToString() != cosmeticType) && value3.PlayerIds.Contains(playerId) && _loadedCosmeticCache.TryGetValue(cosmeticInfo.CosmeticId, out var value2))
				{
					dependant.OnCosmeticLoaded(value2);
				}
			}
		}
	}

	private List<CosmeticRootInfo> ParseCosmeticRoots(IReadOnlyDictionary<string, object> metadata)
	{
		if (!metadata.TryGetValue("CosmeticRoots", out var value))
		{
			LckLog.LogWarning("LCK: Cosmetic metadata does not include top level CosmeticRoots key", "ParseCosmeticRoots", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 295);
			return null;
		}
		if (!(value is IList<object> list))
		{
			LckLog.LogWarning($"LCK: Cosmetic metadata has invalid CosmeticRoots - unexpected type: {value.GetType()}", "ParseCosmeticRoots", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 301);
			return null;
		}
		List<CosmeticRootInfo> list2 = new List<CosmeticRootInfo>();
		foreach (object item in list)
		{
			object value2;
			object value3;
			if (!(item is IReadOnlyDictionary<object, object> readOnlyDictionary))
			{
				LckLog.LogWarning($"LCK: Cosmetic metadata has invalid CosmeticRoots item - unexpected type: {item.GetType()}", "ParseCosmeticRoots", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 310);
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
				LckLog.LogWarning("LCK: Cosmetic metadata has invalid CosmeticRoots item - missing rootPath / type", "ParseCosmeticRoots", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticsManager.cs", 321);
			}
		}
		return list2;
	}

	private Type ResolveType(string typeName)
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
		return null;
	}
}
