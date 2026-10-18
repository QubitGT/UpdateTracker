using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;

namespace UnityEngine.AddressableAssets;

internal class AddressablesImpl : IEqualityComparer<IResourceLocation>
{
	private class LoadResourceLocationKeyOp : AsyncOperationBase<IList<IResourceLocation>>
	{
		private object m_Keys;

		private IList<IResourceLocation> m_locations;

		private AddressablesImpl m_Addressables;

		private Type m_ResourceType;

		protected override string DebugName => m_Keys.ToString();

		public void Init(AddressablesImpl aa, Type t, object keys)
		{
			m_Keys = keys;
			m_ResourceType = t;
			m_Addressables = aa;
		}

		protected override bool InvokeWaitForCompletion()
		{
			m_RM?.Update(Time.unscaledDeltaTime);
			if (!HasExecuted)
			{
				InvokeExecute();
			}
			return true;
		}

		protected override void Execute()
		{
			m_Addressables.GetResourceLocations(m_Keys, m_ResourceType, out m_locations);
			if (m_locations == null)
			{
				m_locations = new List<IResourceLocation>();
			}
			Complete(m_locations, success: true, string.Empty);
		}
	}

	private class LoadResourceLocationKeysOp : AsyncOperationBase<IList<IResourceLocation>>
	{
		private IEnumerable m_Key;

		private Addressables.MergeMode m_MergeMode;

		private IList<IResourceLocation> m_locations;

		private AddressablesImpl m_Addressables;

		private Type m_ResourceType;

		protected override string DebugName => "LoadResourceLocationKeysOp";

		public void Init(AddressablesImpl aa, Type t, IEnumerable key, Addressables.MergeMode mergeMode)
		{
			m_Key = key;
			m_ResourceType = t;
			m_MergeMode = mergeMode;
			m_Addressables = aa;
		}

		protected override void Execute()
		{
			m_Addressables.GetResourceLocations(m_Key, m_ResourceType, m_MergeMode, out m_locations);
			if (m_locations == null)
			{
				m_locations = new List<IResourceLocation>();
			}
			Complete(m_locations, success: true, string.Empty);
		}

		protected override bool InvokeWaitForCompletion()
		{
			m_RM?.Update(Time.unscaledDeltaTime);
			if (!HasExecuted)
			{
				InvokeExecute();
			}
			return true;
		}
	}

	private ResourceManager m_ResourceManager;

	private IInstanceProvider m_InstanceProvider;

	private int m_CatalogRequestsTimeout;

	internal const string kCacheDataFolder = "{UnityEngine.Application.persistentDataPath}/com.unity.addressables/";

	public ISceneProvider SceneProvider;

	internal List<ResourceLocatorInfo> m_ResourceLocators = new List<ResourceLocatorInfo>();

	private AsyncOperationHandle<IResourceLocator> m_InitializationOperation;

	private AsyncOperationHandle<List<string>> m_ActiveCheckUpdateOperation;

	internal AsyncOperationHandle<List<IResourceLocator>> m_ActiveUpdateOperation;

	private Action<AsyncOperationHandle> m_OnHandleCompleteAction;

	private Action<AsyncOperationHandle> m_OnSceneHandleCompleteAction;

	private Action<AsyncOperationHandle> m_OnHandleDestroyedAction;

	private Dictionary<object, AsyncOperationHandle> m_resultToHandle = new Dictionary<object, AsyncOperationHandle>();

	internal HashSet<AsyncOperationHandle> m_SceneInstances = new HashSet<AsyncOperationHandle>();

	private AsyncOperationHandle<bool> m_ActiveCleanBundleCacheOperation;

	internal bool hasStartedInitialization;

	public IInstanceProvider InstanceProvider
	{
		get
		{
			return m_InstanceProvider;
		}
		set
		{
			m_InstanceProvider = value;
			if (m_InstanceProvider is IUpdateReceiver receiver)
			{
				m_ResourceManager.AddUpdateReceiver(receiver);
			}
		}
	}

	public ResourceManager ResourceManager => m_ResourceManager;

	public int CatalogRequestsTimeout
	{
		get
		{
			return m_CatalogRequestsTimeout;
		}
		set
		{
			m_CatalogRequestsTimeout = value;
		}
	}

	internal int ActiveSceneInstances => m_SceneInstances.Count;

	internal int TrackedHandleCount => m_resultToHandle.Count;

	public Func<IResourceLocation, string> InternalIdTransformFunc
	{
		get
		{
			return ResourceManager.InternalIdTransformFunc;
		}
		set
		{
			ResourceManager.InternalIdTransformFunc = value;
		}
	}

	public Action<UnityWebRequest> WebRequestOverride
	{
		get
		{
			return ResourceManager.WebRequestOverride;
		}
		set
		{
			ResourceManager.WebRequestOverride = value;
		}
	}

	public AsyncOperationHandle ChainOperation
	{
		get
		{
			if (!hasStartedInitialization)
			{
				return InitializeAsync();
			}
			if (m_InitializationOperation.IsValid() && !m_InitializationOperation.IsDone)
			{
				return m_InitializationOperation;
			}
			if (m_ActiveUpdateOperation.IsValid() && !m_ActiveUpdateOperation.IsDone)
			{
				return m_ActiveUpdateOperation;
			}
			Debug.LogWarning("ChainOperation property should not be accessed unless ShouldChainRequest is true.");
			return default(AsyncOperationHandle);
		}
	}

	internal bool ShouldChainRequest
	{
		get
		{
			if (!hasStartedInitialization)
			{
				return true;
			}
			if (m_InitializationOperation.IsValid() && !m_InitializationOperation.IsDone)
			{
				return true;
			}
			if (m_ActiveUpdateOperation.IsValid())
			{
				return !m_ActiveUpdateOperation.IsDone;
			}
			return false;
		}
	}

	public string StreamingAssetsSubFolder => "aa";

	public string BuildPath => Addressables.LibraryPath + StreamingAssetsSubFolder + "/" + PlatformMappingService.GetPlatformPathSubFolder();

	public string PlayerBuildDataPath => Application.streamingAssetsPath + "/" + StreamingAssetsSubFolder;

	public string RuntimePath => PlayerBuildDataPath;

	public IEnumerable<IResourceLocator> ResourceLocators => m_ResourceLocators.Select((ResourceLocatorInfo l) => l.Locator);

	internal IEnumerable<string> CatalogsWithAvailableUpdates => from s in m_ResourceLocators
		where s.ContentUpdateAvailable
		select s.Locator.LocatorId;

	public AddressablesImpl(IAllocationStrategy alloc)
	{
		m_ResourceManager = new ResourceManager(alloc);
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	internal void ReleaseSceneManagerOperation()
	{
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}

	internal void OnSceneUnloaded(Scene scene)
	{
		foreach (AsyncOperationHandle sceneInstance in m_SceneInstances)
		{
			if (!sceneInstance.IsValid())
			{
				m_SceneInstances.Remove(sceneInstance);
				break;
			}
			AsyncOperationHandle<SceneInstance> sceneLoadHandle = sceneInstance.Convert<SceneInstance>();
			if (sceneLoadHandle.Result.Scene == scene)
			{
				m_SceneInstances.Remove(sceneInstance);
				m_resultToHandle.Remove(sceneInstance.Result);
				if (sceneLoadHandle.Result.ReleaseSceneOnSceneUnloaded)
				{
					SceneProvider.ReleaseScene(m_ResourceManager, sceneLoadHandle).ReleaseHandleOnCompletion();
				}
				break;
			}
		}
		m_ResourceManager.CleanupSceneInstances(scene);
	}

	public void Log(string msg)
	{
		Debug.Log(msg);
	}

	public void LogFormat(string format, params object[] args)
	{
		Debug.LogFormat(format, args);
	}

	public void LogWarning(string msg)
	{
		Debug.LogWarning(msg);
	}

	public void LogWarningFormat(string format, params object[] args)
	{
		Debug.LogWarningFormat(format, args);
	}

	public void LogError(string msg)
	{
		Debug.LogError(msg);
	}

	public void LogException(AsyncOperationHandle op, Exception ex)
	{
		if (op.Status == AsyncOperationStatus.Failed)
		{
			Debug.LogError(ex.ToString());
		}
	}

	public void LogException(Exception ex)
	{
	}

	public void LogErrorFormat(string format, params object[] args)
	{
		Debug.LogErrorFormat(format, args);
	}

	public string ResolveInternalId(string id)
	{
		string text = AddressablesRuntimeProperties.EvaluateString(id);
		if (text.Length >= 260 && text.StartsWith(Application.dataPath, StringComparison.Ordinal))
		{
			text = text.Substring(Application.dataPath.Length + 1);
		}
		return text;
	}

	public void AddResourceLocator(IResourceLocator loc, string localCatalogHash = null, IResourceLocation remoteCatalogLocation = null)
	{
		m_ResourceLocators.Add(new ResourceLocatorInfo(loc, localCatalogHash, remoteCatalogLocation));
	}

	public void RemoveResourceLocator(IResourceLocator loc)
	{
		m_ResourceLocators.RemoveAll((ResourceLocatorInfo l) => l.Locator == loc);
	}

	public void ClearResourceLocators()
	{
		m_ResourceLocators.Clear();
	}

	internal bool GetResourceLocations(object key, Type type, out IList<IResourceLocation> locations)
	{
		if (type == null && key is AssetReference)
		{
			type = (key as AssetReference).SubObjectType;
		}
		key = EvaluateKey(key);
		locations = null;
		HashSet<IResourceLocation> hashSet = null;
		foreach (ResourceLocatorInfo resourceLocator in m_ResourceLocators)
		{
			if (!resourceLocator.Locator.Locate(key, type, out var locations2))
			{
				continue;
			}
			if (locations == null)
			{
				locations = locations2;
				continue;
			}
			if (hashSet == null)
			{
				hashSet = new HashSet<IResourceLocation>();
				foreach (IResourceLocation location in locations)
				{
					hashSet.Add(location);
				}
			}
			hashSet.UnionWith(locations2);
		}
		if (hashSet == null)
		{
			return locations != null;
		}
		locations = new List<IResourceLocation>(hashSet);
		return true;
	}

	internal bool GetResourceLocations(IEnumerable keys, Type type, Addressables.MergeMode merge, out IList<IResourceLocation> locations)
	{
		locations = null;
		HashSet<IResourceLocation> hashSet = null;
		foreach (object key in keys)
		{
			if (GetResourceLocations(key, type, out var locations2))
			{
				if (locations == null)
				{
					locations = locations2;
					if (merge == Addressables.MergeMode.None)
					{
						return true;
					}
					continue;
				}
				if (hashSet == null)
				{
					hashSet = new HashSet<IResourceLocation>(locations, this);
				}
				switch (merge)
				{
				case Addressables.MergeMode.Intersection:
					hashSet.IntersectWith(locations2);
					break;
				case Addressables.MergeMode.Union:
					hashSet.UnionWith(locations2);
					break;
				}
			}
			else if (merge == Addressables.MergeMode.Intersection)
			{
				locations = null;
				return false;
			}
		}
		if (hashSet == null)
		{
			return locations != null;
		}
		if (hashSet.Count == 0)
		{
			locations = null;
			return false;
		}
		locations = new List<IResourceLocation>(hashSet);
		return true;
	}

	public AsyncOperationHandle<IResourceLocator> InitializeAsync(string runtimeDataPath, string providerSuffix = null, bool autoReleaseHandle = true)
	{
		if (hasStartedInitialization)
		{
			if (m_InitializationOperation.IsValid())
			{
				return m_InitializationOperation;
			}
			AsyncOperationHandle<IResourceLocator> result = ResourceManager.CreateCompletedOperation(m_ResourceLocators[0].Locator, null);
			if (autoReleaseHandle)
			{
				result.ReleaseHandleOnCompletion();
			}
			return result;
		}
		if (ResourceManager.ExceptionHandler == null)
		{
			ResourceManager.ExceptionHandler = LogException;
		}
		hasStartedInitialization = true;
		if (m_InitializationOperation.IsValid())
		{
			return m_InitializationOperation;
		}
		GC.KeepAlive(Application.streamingAssetsPath);
		GC.KeepAlive(Application.persistentDataPath);
		if (string.IsNullOrEmpty(runtimeDataPath))
		{
			return ResourceManager.CreateCompletedOperation<IResourceLocator>(null, $"Invalid Key: {runtimeDataPath}");
		}
		m_OnHandleCompleteAction = OnHandleCompleted;
		m_OnSceneHandleCompleteAction = OnSceneHandleCompleted;
		m_OnHandleDestroyedAction = OnHandleDestroyed;
		if (!m_InitializationOperation.IsValid())
		{
			m_InitializationOperation = InitializationOperation.CreateInitializationOperation(this, runtimeDataPath, providerSuffix);
		}
		if (autoReleaseHandle)
		{
			m_InitializationOperation.ReleaseHandleOnCompletion();
		}
		return m_InitializationOperation;
	}

	public AsyncOperationHandle<IResourceLocator> InitializeAsync()
	{
		string id = RuntimePath + "/settings.json";
		return InitializeAsync(ResolveInternalId(id));
	}

	public AsyncOperationHandle<IResourceLocator> InitializeAsync(bool autoReleaseHandle)
	{
		string id = RuntimePath + "/settings.json";
		return InitializeAsync(ResolveInternalId(id), null, autoReleaseHandle);
	}

	public ResourceLocationBase CreateCatalogLocationWithHashDependencies<T>(IResourceLocation catalogLocation) where T : IResourceProvider
	{
		return CreateCatalogLocationWithHashDependencies<T>(catalogLocation.InternalId);
	}

	public ResourceLocationBase CreateCatalogLocationWithHashDependencies<T>(string catalogLocation) where T : IResourceProvider
	{
		string hashFilePath = catalogLocation.Replace(".bin", ".hash");
		return CreateCatalogLocationWithHashDependencies<T>(catalogLocation, hashFilePath);
	}

	public ResourceLocationBase CreateCatalogLocationWithHashDependencies<T>(string catalogPath, string hashFilePath) where T : IResourceProvider
	{
		ResourceLocationBase resourceLocationBase = new ResourceLocationBase(catalogPath, catalogPath, typeof(T).FullName, typeof(IResourceLocator))
		{
			Data = new ProviderLoadRequestOptions
			{
				IgnoreFailures = false,
				WebRequestTimeout = CatalogRequestsTimeout
			}
		};
		if (!string.IsNullOrEmpty(hashFilePath))
		{
			ProviderLoadRequestOptions providerLoadRequestOptions = new ProviderLoadRequestOptions
			{
				IgnoreFailures = true,
				WebRequestTimeout = CatalogRequestsTimeout
			};
			string text = hashFilePath;
			if (ResourceManagerConfig.IsPathRemote(hashFilePath))
			{
				text = ResourceManagerConfig.StripQueryParameters(hashFilePath);
			}
			ResourceLocationBase item = new ResourceLocationBase(hashFilePath, hashFilePath, typeof(TextDataProvider).FullName, typeof(string))
			{
				Data = providerLoadRequestOptions.Copy()
			};
			resourceLocationBase.Dependencies.Add(item);
			string text2 = ResolveInternalId("{UnityEngine.Application.persistentDataPath}/com.unity.addressables/" + text.GetHashCode() + ".hash");
			ResourceLocationBase item2 = new ResourceLocationBase(text2, text2, typeof(TextDataProvider).FullName, typeof(string))
			{
				Data = providerLoadRequestOptions.Copy()
			};
			resourceLocationBase.Dependencies.Add(item2);
			resourceLocationBase.Dependencies.Add(item2);
		}
		return resourceLocationBase;
	}

	[Conditional("UNITY_EDITOR")]
	private void QueueEditorUpdateIfNeeded()
	{
	}

	public AsyncOperationHandle<IResourceLocator> LoadContentCatalogAsync(string catalogPath, bool autoReleaseHandle = true, string providerSuffix = null)
	{
		ResourceLocationBase loc = CreateCatalogLocationWithHashDependencies<ContentCatalogProvider>(catalogPath);
		if (ShouldChainRequest)
		{
			return ResourceManager.CreateChainOperation(ChainOperation, (AsyncOperationHandle op) => LoadContentCatalogAsync(catalogPath, autoReleaseHandle, providerSuffix));
		}
		AsyncOperationHandle<IResourceLocator> result = InitializationOperation.LoadContentCatalog(this, loc, providerSuffix);
		if (autoReleaseHandle)
		{
			result.ReleaseHandleOnCompletion();
		}
		return result;
	}

	private AsyncOperationHandle<SceneInstance> TrackHandle(AsyncOperationHandle<SceneInstance> handle)
	{
		handle.Completed += delegate(AsyncOperationHandle<SceneInstance> sceneHandle)
		{
			m_OnSceneHandleCompleteAction(sceneHandle);
		};
		return handle;
	}

	private AsyncOperationHandle<TObject> TrackHandle<TObject>(AsyncOperationHandle<TObject> handle)
	{
		handle.CompletedTypeless += m_OnHandleCompleteAction;
		return handle;
	}

	private AsyncOperationHandle TrackHandle(AsyncOperationHandle handle)
	{
		handle.Completed += m_OnHandleCompleteAction;
		return handle;
	}

	internal void ClearTrackHandles()
	{
		m_resultToHandle.Clear();
	}

	public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(IResourceLocation location)
	{
		if (ShouldChainRequest)
		{
			return TrackHandle(LoadAssetWithChain<TObject>(ChainOperation, location));
		}
		return TrackHandle(ResourceManager.ProvideResource<TObject>(location));
	}

	private AsyncOperationHandle<TObject> LoadAssetWithChain<TObject>(AsyncOperationHandle dep, IResourceLocation loc)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadAssetAsync<TObject>(loc));
	}

	private AsyncOperationHandle<TObject> LoadAssetWithChain<TObject>(AsyncOperationHandle dep, object key)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadAssetAsync<TObject>(key));
	}

	public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key)
	{
		if (ShouldChainRequest)
		{
			return TrackHandle(LoadAssetWithChain<TObject>(ChainOperation, key));
		}
		key = EvaluateKey(key);
		Type type = typeof(TObject);
		if (type.IsArray)
		{
			type = type.GetElementType();
		}
		else if (type.IsGenericType && typeof(IList<>) == type.GetGenericTypeDefinition())
		{
			type = type.GetGenericArguments()[0];
		}
		foreach (ResourceLocatorInfo resourceLocator in m_ResourceLocators)
		{
			if (!resourceLocator.Locator.Locate(key, type, out var locations))
			{
				continue;
			}
			foreach (IResourceLocation item in locations)
			{
				if (ResourceManager.GetResourceProvider(typeof(TObject), item) != null)
				{
					return TrackHandle(ResourceManager.ProvideResource<TObject>(item));
				}
			}
		}
		return ResourceManager.CreateCompletedOperationWithException(default(TObject), new InvalidKeyException(key, type, this));
	}

	public AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsWithChain(AsyncOperationHandle dep, IEnumerable keys, Addressables.MergeMode mode, Type type)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadResourceLocationsAsync(keys, mode, type));
	}

	public AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(IEnumerable keys, Addressables.MergeMode mode, Type type = null)
	{
		if (ShouldChainRequest)
		{
			return TrackHandle(LoadResourceLocationsWithChain(ChainOperation, keys, mode, type));
		}
		LoadResourceLocationKeysOp loadResourceLocationKeysOp = new LoadResourceLocationKeysOp();
		loadResourceLocationKeysOp.Init(this, type, keys, mode);
		return TrackHandle(ResourceManager.StartOperation(loadResourceLocationKeysOp, default(AsyncOperationHandle)));
	}

	public AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsWithChain(AsyncOperationHandle dep, object key, Type type)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadResourceLocationsAsync(key, type));
	}

	public AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(object key, Type type = null)
	{
		if (ShouldChainRequest)
		{
			return TrackHandle(LoadResourceLocationsWithChain(ChainOperation, key, type));
		}
		LoadResourceLocationKeyOp loadResourceLocationKeyOp = new LoadResourceLocationKeyOp();
		loadResourceLocationKeyOp.Init(this, type, key);
		return TrackHandle(ResourceManager.StartOperation(loadResourceLocationKeyOp, default(AsyncOperationHandle)));
	}

	public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<IResourceLocation> locations, Action<TObject> callback, bool releaseDependenciesOnFailure)
	{
		if (ShouldChainRequest)
		{
			return TrackHandle(LoadAssetsWithChain(ChainOperation, locations, callback, releaseDependenciesOnFailure));
		}
		return TrackHandle(ResourceManager.ProvideResources(locations, releaseDependenciesOnFailure, callback));
	}

	private AsyncOperationHandle<IList<TObject>> LoadAssetsWithChain<TObject>(AsyncOperationHandle dep, IList<IResourceLocation> locations, Action<TObject> callback, bool releaseDependenciesOnFailure)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadAssetsAsync(locations, callback, releaseDependenciesOnFailure));
	}

	private AsyncOperationHandle<IList<TObject>> LoadAssetsWithChain<TObject>(AsyncOperationHandle dep, IEnumerable keys, Action<TObject> callback, Addressables.MergeMode mode, bool releaseDependenciesOnFailure)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadAssetsAsync(keys, callback, mode, releaseDependenciesOnFailure));
	}

	public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, Addressables.MergeMode mode, bool releaseDependenciesOnFailure)
	{
		if (keys is string text)
		{
			keys = new string[1] { text };
		}
		if (ShouldChainRequest)
		{
			return TrackHandle(LoadAssetsWithChain(ChainOperation, keys, callback, mode, releaseDependenciesOnFailure));
		}
		if (!GetResourceLocations(keys, typeof(TObject), mode, out var locations))
		{
			return ResourceManager.CreateCompletedOperationWithException<IList<TObject>>(null, new InvalidKeyException(keys, typeof(TObject), mode, this));
		}
		return LoadAssetsAsync(locations, callback, releaseDependenciesOnFailure);
	}

	private AsyncOperationHandle<IList<TObject>> LoadAssetsWithChain<TObject>(AsyncOperationHandle dep, object key, Action<TObject> callback, bool releaseDependenciesOnFailure)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op2) => LoadAssetsAsync(key, callback, releaseDependenciesOnFailure));
	}

	public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback, bool releaseDependenciesOnFailure)
	{
		if (ShouldChainRequest)
		{
			return TrackHandle(LoadAssetsWithChain(ChainOperation, key, callback, releaseDependenciesOnFailure));
		}
		if (!GetResourceLocations(key, typeof(TObject), out var locations))
		{
			return ResourceManager.CreateCompletedOperationWithException<IList<TObject>>(null, new InvalidKeyException(key, typeof(TObject), this));
		}
		return LoadAssetsAsync(locations, callback, releaseDependenciesOnFailure);
	}

	private void OnHandleDestroyed(AsyncOperationHandle handle)
	{
		if (handle.Status == AsyncOperationStatus.Succeeded)
		{
			m_resultToHandle.Remove(handle.Result);
		}
	}

	private void OnSceneHandleCompleted(AsyncOperationHandle handle)
	{
		if (handle.Status == AsyncOperationStatus.Succeeded)
		{
			m_SceneInstances.Add(handle);
			if (m_resultToHandle.TryAdd(handle.Result, handle))
			{
				handle.Destroyed += m_OnHandleDestroyedAction;
			}
		}
	}

	private void OnHandleCompleted(AsyncOperationHandle handle)
	{
		if (handle.Status == AsyncOperationStatus.Succeeded && m_resultToHandle.TryAdd(handle.Result, handle))
		{
			handle.Destroyed += m_OnHandleDestroyedAction;
		}
	}

	public void Release<TObject>(TObject obj)
	{
		AsyncOperationHandle value;
		if (obj == null)
		{
			LogWarning("Addressables.Release() - trying to release null object.");
		}
		else if (m_resultToHandle.TryGetValue(obj, out value))
		{
			value.Release();
		}
		else
		{
			LogError("Addressables.Release was called on an object that Addressables was not previously aware of.  Thus nothing is being released");
		}
	}

	public void Release<TObject>(AsyncOperationHandle<TObject> handle)
	{
		m_ResourceManager.Release(handle);
	}

	public void Release(AsyncOperationHandle handle)
	{
		m_ResourceManager.Release(handle);
	}

	private AsyncOperationHandle<long> GetDownloadSizeWithChain(AsyncOperationHandle dep, object key)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => GetDownloadSizeAsync(key));
	}

	private AsyncOperationHandle<long> ComputeCatalogSizeWithChain(IResourceLocation catalogLoc)
	{
		if (!catalogLoc.HasDependencies)
		{
			return ResourceManager.CreateCompletedOperation(0L, "Attempting to get the remote header size of a content catalog, but no dependencies pointing to a remote location could be found for location " + catalogLoc.InternalId + ". Catalog location dependencies can be setup using CreateCatalogLocationWithHashDependencies");
		}
		AsyncOperationHandle dependentOp = ResourceManager.ProvideResource<string>(catalogLoc.Dependencies[0]);
		return ResourceManager.CreateChainOperation(dependentOp, delegate(AsyncOperationHandle op)
		{
			try
			{
				Hash128 remoteHash = Hash128.Parse(op.Result.ToString());
				if (!IsCatalogCached(catalogLoc, remoteHash))
				{
					return GetRemoteCatalogHeaderSize(catalogLoc);
				}
			}
			catch (Exception arg)
			{
				return ResourceManager.CreateCompletedOperation(0L, $"Fetching the remote catalog size failed. {arg}");
			}
			return ResourceManager.CreateCompletedOperation(0L, string.Empty);
		});
	}

	internal bool IsCatalogCached(IResourceLocation catalogLoc, Hash128 remoteHash)
	{
		if (!catalogLoc.HasDependencies || catalogLoc.Dependencies.Count != 2 || !File.Exists(catalogLoc.Dependencies[1].InternalId) || remoteHash != Hash128.Parse(File.ReadAllText(catalogLoc.Dependencies[1].InternalId)))
		{
			return false;
		}
		return true;
	}

	internal AsyncOperationHandle<long> GetRemoteCatalogHeaderSize(IResourceLocation catalogLoc)
	{
		if (!catalogLoc.HasDependencies)
		{
			return ResourceManager.CreateCompletedOperation(0L, "Attempting to get the remote header size of a content catalog, but no dependencies pointing to a remote location could be found for location " + catalogLoc.InternalId + ". Catalog location dependencies can be setup using CreateCatalogLocationWithHashDependencies");
		}
		AsyncOperationBase<UnityWebRequest> operation = new UnityWebRequestOperation(new UnityWebRequest(catalogLoc.Dependencies[0].InternalId.Replace(".hash", ".bin"), "HEAD"));
		return ResourceManager.CreateChainOperation(ResourceManager.StartOperation(operation, default(AsyncOperationHandle)), delegate(AsyncOperationHandle<UnityWebRequest> getOp)
		{
			string text = getOp.Result?.GetResponseHeader("Content-Length");
			long result;
			return (text != null && long.TryParse(text, out result)) ? ResourceManager.CreateCompletedOperation(result, "") : ResourceManager.CreateCompletedOperation(0L, "Attempting to get the remote header of a catalog failed.");
		});
	}

	private AsyncOperationHandle<long> GetDownloadSizeWithChain(AsyncOperationHandle dep, IEnumerable keys)
	{
		return ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => GetDownloadSizeAsync(keys));
	}

	public AsyncOperationHandle<long> GetDownloadSizeAsync(object key)
	{
		return GetDownloadSizeAsync(new object[1] { key });
	}

	public AsyncOperationHandle<long> GetDownloadSizeAsync(IEnumerable keys)
	{
		if (ShouldChainRequest)
		{
			return TrackHandle(GetDownloadSizeWithChain(ChainOperation, keys));
		}
		List<IResourceLocation> list = new List<IResourceLocation>();
		foreach (object key in keys)
		{
			IList<IResourceLocation> locations;
			if (key is IList<IResourceLocation>)
			{
				locations = key as IList<IResourceLocation>;
			}
			else if (key is IResourceLocation)
			{
				foreach (ResourceLocatorInfo resourceLocator in m_ResourceLocators)
				{
					if (resourceLocator.CatalogLocation == key as IResourceLocation)
					{
						return ComputeCatalogSizeWithChain(key as IResourceLocation);
					}
				}
				locations = new List<IResourceLocation>(1) { key as IResourceLocation };
			}
			else if (!GetResourceLocations(key, typeof(object), out locations))
			{
				return ResourceManager.CreateCompletedOperationWithException(0L, new InvalidKeyException(key, typeof(object), this));
			}
			foreach (IResourceLocation item in locations)
			{
				if (item.HasDependencies)
				{
					list.AddRange(item.Dependencies);
				}
			}
		}
		GetDownloadSizeOperation getDownloadSizeOperation = new GetDownloadSizeOperation();
		getDownloadSizeOperation.Init(list.Distinct(new ResourceLocationComparer()), ResourceManager);
		return ResourceManager.StartOperation(getDownloadSizeOperation, default(AsyncOperationHandle));
	}

	private AsyncOperationHandle DownloadDependenciesAsyncWithChain(AsyncOperationHandle dep, object key, bool autoReleaseHandle)
	{
		AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle = ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => DownloadDependenciesAsync(key).Convert<IList<IAssetBundleResource>>());
		if (autoReleaseHandle)
		{
			asyncOperationHandle.ReleaseHandleOnCompletion();
		}
		return asyncOperationHandle;
	}

	internal static void WrapAsDownloadLocations(List<IResourceLocation> locations)
	{
		for (int i = 0; i < locations.Count; i++)
		{
			locations[i] = new DownloadOnlyLocation(locations[i]);
		}
	}

	private static List<IResourceLocation> GatherDependenciesFromLocations(IList<IResourceLocation> locations)
	{
		HashSet<IResourceLocation> hashSet = new HashSet<IResourceLocation>(new ResourceLocationComparer());
		foreach (IResourceLocation location in locations)
		{
			if (location.ResourceType == typeof(IAssetBundleResource))
			{
				hashSet.Add(location);
			}
			if (!location.HasDependencies)
			{
				continue;
			}
			foreach (IResourceLocation dependency in location.Dependencies)
			{
				if (dependency.ResourceType == typeof(IAssetBundleResource))
				{
					hashSet.Add(dependency);
				}
			}
		}
		return new List<IResourceLocation>(hashSet);
	}

	public AsyncOperationHandle DownloadDependenciesAsync(object key, bool autoReleaseHandle = false)
	{
		if (ShouldChainRequest)
		{
			return DownloadDependenciesAsyncWithChain(ChainOperation, key, autoReleaseHandle);
		}
		if (!GetResourceLocations(key, typeof(object), out var locations))
		{
			AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle = ResourceManager.CreateCompletedOperationWithException<IList<IAssetBundleResource>>(null, new InvalidKeyException(key, typeof(object), this));
			if (autoReleaseHandle)
			{
				asyncOperationHandle.ReleaseHandleOnCompletion();
			}
			return asyncOperationHandle;
		}
		List<IResourceLocation> locations2 = GatherDependenciesFromLocations(locations);
		WrapAsDownloadLocations(locations2);
		AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle2 = LoadAssetsAsync<IAssetBundleResource>(locations2, null, releaseDependenciesOnFailure: true);
		if (autoReleaseHandle)
		{
			asyncOperationHandle2.ReleaseHandleOnCompletion();
		}
		return asyncOperationHandle2;
	}

	private AsyncOperationHandle DownloadDependenciesAsyncWithChain(AsyncOperationHandle dep, IList<IResourceLocation> locations, bool autoReleaseHandle)
	{
		AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle = ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => DownloadDependenciesAsync(locations).Convert<IList<IAssetBundleResource>>());
		if (autoReleaseHandle)
		{
			asyncOperationHandle.ReleaseHandleOnCompletion();
		}
		return asyncOperationHandle;
	}

	public AsyncOperationHandle DownloadDependenciesAsync(IList<IResourceLocation> locations, bool autoReleaseHandle = false)
	{
		if (ShouldChainRequest)
		{
			return DownloadDependenciesAsyncWithChain(ChainOperation, locations, autoReleaseHandle);
		}
		List<IResourceLocation> locations2 = GatherDependenciesFromLocations(locations);
		WrapAsDownloadLocations(locations2);
		AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle = LoadAssetsAsync<IAssetBundleResource>(locations2, null, releaseDependenciesOnFailure: true);
		if (autoReleaseHandle)
		{
			asyncOperationHandle.ReleaseHandleOnCompletion();
		}
		return asyncOperationHandle;
	}

	private AsyncOperationHandle DownloadDependenciesAsyncWithChain(AsyncOperationHandle dep, IEnumerable keys, Addressables.MergeMode mode, bool autoReleaseHandle)
	{
		AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle = ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => DownloadDependenciesAsync(keys, mode).Convert<IList<IAssetBundleResource>>());
		if (autoReleaseHandle)
		{
			asyncOperationHandle.ReleaseHandleOnCompletion();
		}
		return asyncOperationHandle;
	}

	public AsyncOperationHandle DownloadDependenciesAsync(IEnumerable keys, Addressables.MergeMode mode, bool autoReleaseHandle = false)
	{
		if (ShouldChainRequest)
		{
			return DownloadDependenciesAsyncWithChain(ChainOperation, keys, mode, autoReleaseHandle);
		}
		if (!GetResourceLocations(keys, typeof(object), mode, out var locations))
		{
			AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle = ResourceManager.CreateCompletedOperationWithException<IList<IAssetBundleResource>>(null, new InvalidKeyException(keys, typeof(object), mode, this));
			if (autoReleaseHandle)
			{
				asyncOperationHandle.ReleaseHandleOnCompletion();
			}
			return asyncOperationHandle;
		}
		List<IResourceLocation> locations2 = GatherDependenciesFromLocations(locations);
		WrapAsDownloadLocations(locations2);
		AsyncOperationHandle<IList<IAssetBundleResource>> asyncOperationHandle2 = LoadAssetsAsync<IAssetBundleResource>(locations2, null, releaseDependenciesOnFailure: true);
		if (autoReleaseHandle)
		{
			asyncOperationHandle2.ReleaseHandleOnCompletion();
		}
		return asyncOperationHandle2;
	}

	internal bool ClearDependencyCacheForKey(object key)
	{
		bool flag = true;
		IList<IResourceLocation> list = null;
		IList<IResourceLocation> locations;
		if (key is IResourceLocation && (key as IResourceLocation).HasDependencies)
		{
			list = GatherDependenciesFromLocations((key as IResourceLocation).Dependencies);
		}
		else if (GetResourceLocations(key, typeof(object), out locations))
		{
			list = GatherDependenciesFromLocations(locations);
		}
		if (list != null)
		{
			foreach (IResourceLocation item in list)
			{
				if (!(item.Data is AssetBundleRequestOptions { BundleName: var bundleName } assetBundleRequestOptions))
				{
					continue;
				}
				if (m_ResourceManager.GetOperationFromCache(item, typeof(IAssetBundleResource)) != null)
				{
					Debug.LogWarning("Attempting to clear cached version including " + bundleName + ", while " + bundleName + " is currently loaded.");
					if (!string.IsNullOrEmpty(assetBundleRequestOptions.Hash))
					{
						Hash128 hash = Hash128.Parse(assetBundleRequestOptions.Hash);
						Caching.ClearOtherCachedVersions(bundleName, hash);
					}
				}
				else
				{
					flag = flag && Caching.ClearAllCachedVersions(bundleName);
				}
			}
		}
		return flag;
	}

	internal void AutoReleaseHandleOnTypelessCompletion<TObject>(AsyncOperationHandle<TObject> handle)
	{
		handle.CompletedTypeless += delegate(AsyncOperationHandle op)
		{
			op.Release();
		};
	}

	public AsyncOperationHandle<bool> ClearDependencyCacheAsync(object key, bool autoReleaseHandle)
	{
		if (ShouldChainRequest)
		{
			AsyncOperationHandle<bool> result = ResourceManager.CreateChainOperation(ChainOperation, (AsyncOperationHandle op) => ClearDependencyCacheAsync(key, autoReleaseHandle));
			if (autoReleaseHandle)
			{
				result.ReleaseHandleOnCompletion();
			}
			return result;
		}
		bool flag = ClearDependencyCacheForKey(key);
		AsyncOperationHandle<bool> result2 = ResourceManager.CreateCompletedOperation(flag, flag ? string.Empty : "Unable to clear the cache.  AssetBundle's may still be loaded for the given key.");
		if (autoReleaseHandle)
		{
			result2.ReleaseHandleOnCompletion();
		}
		return result2;
	}

	public AsyncOperationHandle<bool> ClearDependencyCacheAsync(IList<IResourceLocation> locations, bool autoReleaseHandle)
	{
		if (ShouldChainRequest)
		{
			AsyncOperationHandle<bool> result = ResourceManager.CreateChainOperation(ChainOperation, (AsyncOperationHandle op) => ClearDependencyCacheAsync(locations, autoReleaseHandle));
			if (autoReleaseHandle)
			{
				result.ReleaseHandleOnCompletion();
			}
			return result;
		}
		bool flag = true;
		foreach (IResourceLocation location in locations)
		{
			flag = flag && ClearDependencyCacheForKey(location);
		}
		AsyncOperationHandle<bool> result2 = ResourceManager.CreateCompletedOperation(flag, flag ? string.Empty : "Unable to clear the cache.  AssetBundle's may still be loaded for the given key(s).");
		if (autoReleaseHandle)
		{
			result2.ReleaseHandleOnCompletion();
		}
		return result2;
	}

	public AsyncOperationHandle<bool> ClearDependencyCacheAsync(IEnumerable keys, bool autoReleaseHandle)
	{
		if (ShouldChainRequest)
		{
			AsyncOperationHandle<bool> result = ResourceManager.CreateChainOperation(ChainOperation, (AsyncOperationHandle op) => ClearDependencyCacheAsync(keys, autoReleaseHandle));
			if (autoReleaseHandle)
			{
				result.ReleaseHandleOnCompletion();
			}
			return result;
		}
		bool flag = true;
		foreach (object key in keys)
		{
			flag = flag && ClearDependencyCacheForKey(key);
		}
		AsyncOperationHandle<bool> result2 = ResourceManager.CreateCompletedOperation(flag, flag ? string.Empty : "Unable to clear the cache.  AssetBundle's may still be loaded for the given key(s).");
		if (autoReleaseHandle)
		{
			result2.ReleaseHandleOnCompletion();
		}
		return result2;
	}

	public AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
	{
		return InstantiateAsync(location, new InstantiationParameters(parent, instantiateInWorldSpace), trackHandle);
	}

	public AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
	{
		return InstantiateAsync(location, new InstantiationParameters(position, rotation, parent), trackHandle);
	}

	public AsyncOperationHandle<GameObject> InstantiateAsync(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
	{
		return InstantiateAsync(key, new InstantiationParameters(parent, instantiateInWorldSpace), trackHandle);
	}

	public AsyncOperationHandle<GameObject> InstantiateAsync(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
	{
		return InstantiateAsync(key, new InstantiationParameters(position, rotation, parent), trackHandle);
	}

	private AsyncOperationHandle<GameObject> InstantiateWithChain(AsyncOperationHandle dep, object key, InstantiationParameters instantiateParameters, bool trackHandle = true)
	{
		AsyncOperationHandle<GameObject> result = ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => InstantiateAsync(key, instantiateParameters, trackHandle: false));
		if (trackHandle)
		{
			result.CompletedTypeless += m_OnHandleCompleteAction;
		}
		return result;
	}

	public AsyncOperationHandle<GameObject> InstantiateAsync(object key, InstantiationParameters instantiateParameters, bool trackHandle = true)
	{
		if (ShouldChainRequest)
		{
			return InstantiateWithChain(ChainOperation, key, instantiateParameters, trackHandle);
		}
		key = EvaluateKey(key);
		foreach (ResourceLocatorInfo resourceLocator in m_ResourceLocators)
		{
			if (resourceLocator.Locator.Locate(key, typeof(GameObject), out var locations))
			{
				return InstantiateAsync(locations[0], instantiateParameters, trackHandle);
			}
		}
		return ResourceManager.CreateCompletedOperationWithException<GameObject>(null, new InvalidKeyException(key, typeof(GameObject), this));
	}

	private AsyncOperationHandle<GameObject> InstantiateWithChain(AsyncOperationHandle dep, IResourceLocation location, InstantiationParameters instantiateParameters, bool trackHandle = true)
	{
		AsyncOperationHandle<GameObject> result = ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => InstantiateAsync(location, instantiateParameters, trackHandle: false));
		if (trackHandle)
		{
			result.CompletedTypeless += m_OnHandleCompleteAction;
		}
		return result;
	}

	public AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, InstantiationParameters instantiateParameters, bool trackHandle = true)
	{
		if (ShouldChainRequest)
		{
			return InstantiateWithChain(ChainOperation, location, instantiateParameters, trackHandle);
		}
		AsyncOperationHandle<GameObject> result = ResourceManager.ProvideInstance(InstanceProvider, location, instantiateParameters);
		if (!trackHandle)
		{
			return result;
		}
		result.CompletedTypeless += m_OnHandleCompleteAction;
		return result;
	}

	public bool ReleaseInstance(GameObject instance)
	{
		if (instance == null)
		{
			LogWarning("Addressables.ReleaseInstance() - trying to release null object.");
			return false;
		}
		if (m_resultToHandle.TryGetValue(instance, out var value))
		{
			value.Release();
			return true;
		}
		return false;
	}

	internal AsyncOperationHandle<SceneInstance> LoadSceneWithChain(AsyncOperationHandle dep, object key, LoadSceneParameters loadSceneParameters, SceneReleaseMode releaseMode = SceneReleaseMode.ReleaseSceneWhenSceneUnloaded, bool activateOnLoad = true, int priority = 100)
	{
		return TrackHandle(ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadSceneAsync(key, loadSceneParameters, releaseMode, activateOnLoad, priority, trackHandle: false)));
	}

	internal AsyncOperationHandle<SceneInstance> LoadSceneWithChain(AsyncOperationHandle dep, IResourceLocation key, LoadSceneParameters loadSceneParameters, SceneReleaseMode releaseMode = SceneReleaseMode.ReleaseSceneWhenSceneUnloaded, bool activateOnLoad = true, int priority = 100)
	{
		return TrackHandle(ResourceManager.CreateChainOperation(dep, (AsyncOperationHandle op) => LoadSceneAsync(key, loadSceneParameters, releaseMode, activateOnLoad, priority, trackHandle: false)));
	}

	public AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneParameters loadSceneParameters, SceneReleaseMode releaseMode = SceneReleaseMode.ReleaseSceneWhenSceneUnloaded, bool activateOnLoad = true, int priority = 100, bool trackHandle = true)
	{
		if (ShouldChainRequest)
		{
			return LoadSceneWithChain(ChainOperation, key, loadSceneParameters, releaseMode, activateOnLoad, priority);
		}
		if (!GetResourceLocations(key, typeof(SceneInstance), out var locations))
		{
			return ResourceManager.CreateCompletedOperationWithException(default(SceneInstance), new InvalidKeyException(key, typeof(SceneInstance), this));
		}
		return LoadSceneAsync(locations[0], loadSceneParameters, releaseMode, activateOnLoad, priority, trackHandle);
	}

	public AsyncOperationHandle<SceneInstance> LoadSceneAsync(IResourceLocation location, LoadSceneParameters loadSceneParameters, SceneReleaseMode releaseMode = SceneReleaseMode.ReleaseSceneWhenSceneUnloaded, bool activateOnLoad = true, int priority = 100, bool trackHandle = true)
	{
		if (ShouldChainRequest)
		{
			return LoadSceneWithChain(ChainOperation, location, loadSceneParameters, releaseMode, activateOnLoad, priority);
		}
		AsyncOperationHandle<SceneInstance> asyncOperationHandle = ResourceManager.ProvideScene(SceneProvider, location, loadSceneParameters, releaseMode, activateOnLoad, priority);
		if (trackHandle)
		{
			return TrackHandle(asyncOperationHandle);
		}
		return asyncOperationHandle;
	}

	public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true)
	{
		if (!m_resultToHandle.TryGetValue(scene, out var value))
		{
			string text = $"Addressables.UnloadSceneAsync() - Cannot find handle for scene {scene}";
			LogWarning(text);
			return ResourceManager.CreateCompletedOperation(scene, text);
		}
		if (value.m_InternalOp.IsRunning)
		{
			return CreateUnloadSceneWithChain(value, unloadOptions, autoReleaseHandle);
		}
		return UnloadSceneAsync(value, unloadOptions, autoReleaseHandle);
	}

	public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true)
	{
		if (handle.m_InternalOp.IsRunning)
		{
			return CreateUnloadSceneWithChain(handle, unloadOptions, autoReleaseHandle);
		}
		return UnloadSceneAsync(handle.Convert<SceneInstance>(), unloadOptions, autoReleaseHandle);
	}

	public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true)
	{
		if (handle.m_InternalOp.IsRunning)
		{
			return CreateUnloadSceneWithChain(handle, unloadOptions, autoReleaseHandle);
		}
		return InternalUnloadScene(handle, unloadOptions, autoReleaseHandle);
	}

	internal AsyncOperationHandle<SceneInstance> CreateUnloadSceneWithChain(AsyncOperationHandle handle, UnloadSceneOptions unloadOptions, bool autoReleaseHandle)
	{
		return m_ResourceManager.CreateChainOperation(handle, (AsyncOperationHandle completedHandle) => InternalUnloadScene(completedHandle.Convert<SceneInstance>(), unloadOptions, autoReleaseHandle));
	}

	internal AsyncOperationHandle<SceneInstance> CreateUnloadSceneWithChain(AsyncOperationHandle<SceneInstance> handle, UnloadSceneOptions unloadOptions, bool autoReleaseHandle)
	{
		return m_ResourceManager.CreateChainOperation(handle, (AsyncOperationHandle<SceneInstance> completedHandle) => InternalUnloadScene(completedHandle, unloadOptions, autoReleaseHandle));
	}

	internal AsyncOperationHandle<SceneInstance> InternalUnloadScene(AsyncOperationHandle<SceneInstance> handle, UnloadSceneOptions unloadOptions, bool autoReleaseHandle)
	{
		AsyncOperationHandle<SceneInstance> result = SceneProvider.ReleaseScene(ResourceManager, handle, unloadOptions);
		if (autoReleaseHandle)
		{
			result.ReleaseHandleOnCompletion();
		}
		return result;
	}

	private object EvaluateKey(object obj)
	{
		if (obj is IKeyEvaluator)
		{
			return (obj as IKeyEvaluator).RuntimeKey;
		}
		return obj;
	}

	internal AsyncOperationHandle<List<string>> CheckForCatalogUpdates(bool autoReleaseHandle = true)
	{
		if (ShouldChainRequest)
		{
			return CheckForCatalogUpdatesWithChain(autoReleaseHandle);
		}
		if (m_ActiveCheckUpdateOperation.IsValid())
		{
			m_ActiveCheckUpdateOperation.Release();
		}
		m_ActiveCheckUpdateOperation = new CheckCatalogsOperation(this).Start(m_ResourceLocators);
		if (autoReleaseHandle)
		{
			AutoReleaseHandleOnTypelessCompletion(m_ActiveCheckUpdateOperation);
		}
		return m_ActiveCheckUpdateOperation;
	}

	internal AsyncOperationHandle<List<string>> CheckForCatalogUpdatesWithChain(bool autoReleaseHandle)
	{
		return ResourceManager.CreateChainOperation(ChainOperation, (AsyncOperationHandle op) => CheckForCatalogUpdates(autoReleaseHandle));
	}

	public ResourceLocatorInfo GetLocatorInfo(string c)
	{
		foreach (ResourceLocatorInfo resourceLocator in m_ResourceLocators)
		{
			if (resourceLocator.Locator.LocatorId == c)
			{
				return resourceLocator;
			}
		}
		return null;
	}

	internal AsyncOperationHandle<List<IResourceLocator>> UpdateCatalogs(IEnumerable<string> catalogIds = null, bool autoReleaseHandle = true, bool autoCleanBundleCache = false)
	{
		if (m_ActiveUpdateOperation.IsValid())
		{
			return m_ActiveUpdateOperation;
		}
		if (catalogIds == null && !CatalogsWithAvailableUpdates.Any())
		{
			return m_ResourceManager.CreateChainOperation(CheckForCatalogUpdates(), (AsyncOperationHandle<List<string>> depOp) => UpdateCatalogs(CatalogsWithAvailableUpdates, autoReleaseHandle, autoCleanBundleCache));
		}
		AsyncOperationHandle<List<IResourceLocator>> asyncOperationHandle = new UpdateCatalogsOperation(this).Start((catalogIds == null) ? CatalogsWithAvailableUpdates : catalogIds, autoCleanBundleCache);
		if (autoReleaseHandle)
		{
			AutoReleaseHandleOnTypelessCompletion(asyncOperationHandle);
		}
		return asyncOperationHandle;
	}

	public bool Equals(IResourceLocation x, IResourceLocation y)
	{
		if (x.PrimaryKey.Equals(y.PrimaryKey) && x.ResourceType.Equals(y.ResourceType))
		{
			return x.InternalId.Equals(y.InternalId);
		}
		return false;
	}

	public int GetHashCode(IResourceLocation loc)
	{
		return loc.PrimaryKey.GetHashCode() * 31 + loc.ResourceType.GetHashCode();
	}

	internal AsyncOperationHandle<bool> CleanBundleCache(IEnumerable<string> catalogIds, bool forceSingleThreading)
	{
		if (ShouldChainRequest)
		{
			return CleanBundleCacheWithChain(catalogIds, forceSingleThreading);
		}
		if (catalogIds == null)
		{
			catalogIds = m_ResourceLocators.Select((ResourceLocatorInfo s) => s.Locator.LocatorId);
		}
		List<IResourceLocation> list = new List<IResourceLocation>();
		foreach (string catalogId in catalogIds)
		{
			if (catalogId != null)
			{
				ResourceLocatorInfo locatorInfo = GetLocatorInfo(catalogId);
				if (locatorInfo != null && locatorInfo.CatalogLocation != null)
				{
					list.Add(locatorInfo.CatalogLocation);
				}
			}
		}
		if (list.Count == 0)
		{
			return ResourceManager.CreateCompletedOperation(result: false, "Provided catalogs do not load data from a catalog file. This can occur when using the \"Use Asset Database (fastest)\" playmode script. Bundle cache was not modified.");
		}
		return CleanBundleCache(ResourceManager.CreateGroupOperation<object>(list), forceSingleThreading);
	}

	internal AsyncOperationHandle<bool> CleanBundleCache(AsyncOperationHandle<IList<AsyncOperationHandle>> depOp, bool forceSingleThreading)
	{
		if (ShouldChainRequest)
		{
			return CleanBundleCacheWithChain(depOp, forceSingleThreading);
		}
		if (m_ActiveCleanBundleCacheOperation.IsValid() && !m_ActiveCleanBundleCacheOperation.IsDone)
		{
			return ResourceManager.CreateCompletedOperation(result: false, "Bundle cache is already being cleaned.");
		}
		m_ActiveCleanBundleCacheOperation = new CleanBundleCacheOperation(this, forceSingleThreading).Start(depOp);
		return m_ActiveCleanBundleCacheOperation;
	}

	internal AsyncOperationHandle<bool> CleanBundleCacheWithChain(AsyncOperationHandle<IList<AsyncOperationHandle>> depOp, bool forceSingleThreading)
	{
		return ResourceManager.CreateChainOperation(ChainOperation, (AsyncOperationHandle op) => CleanBundleCache(depOp, forceSingleThreading));
	}

	internal AsyncOperationHandle<bool> CleanBundleCacheWithChain(IEnumerable<string> catalogIds, bool forceSingleThreading)
	{
		return ResourceManager.CreateChainOperation(ChainOperation, (AsyncOperationHandle op) => CleanBundleCache(catalogIds, forceSingleThreading));
	}
}
