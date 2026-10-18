using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.AddressableAssets.ResourceProviders;

[DisplayName("Content Catalog Provider")]
public class ContentCatalogProvider : ResourceProviderBase
{
	public enum DependencyHashIndex
	{
		Remote,
		Cache,
		Local,
		Count
	}

	internal class InternalOp
	{
		internal class BundledCatalog
		{
			private readonly string m_BundlePath;

			private bool m_OpInProgress;

			private AssetBundleCreateRequest m_LoadBundleRequest;

			internal AssetBundle m_CatalogAssetBundle;

			private AssetBundleRequest m_LoadTextAssetRequest;

			private ContentCatalogData m_CatalogData;

			private WebRequestQueueOperation m_WebRequestQueueOperation;

			private AsyncOperation m_RequestOperation;

			private int m_WebRequestTimeout;

			public bool OpInProgress => m_OpInProgress;

			public bool OpIsSuccess
			{
				get
				{
					if (!m_OpInProgress)
					{
						return m_CatalogData != null;
					}
					return false;
				}
			}

			public event Action<ContentCatalogData> OnLoaded;

			public BundledCatalog(string bundlePath, int webRequestTimeout = 0)
			{
				if (string.IsNullOrEmpty(bundlePath))
				{
					throw new ArgumentNullException("bundlePath", "Catalog bundle path is null.");
				}
				if (!bundlePath.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase))
				{
					throw new ArgumentException("You must supply a valid bundle file path.");
				}
				m_BundlePath = bundlePath;
				m_WebRequestTimeout = webRequestTimeout;
			}

			~BundledCatalog()
			{
				Unload();
			}

			private void Unload()
			{
				m_CatalogAssetBundle?.Unload(unloadAllLoadedObjects: true);
				m_CatalogAssetBundle = null;
			}

			public void LoadCatalogFromBundleAsync()
			{
				if (m_OpInProgress)
				{
					Addressables.LogError("Operation in progress : A catalog is already being loaded. Please wait for the operation to complete.");
					return;
				}
				m_OpInProgress = true;
				if (ResourceManagerConfig.ShouldPathUseWebRequest(m_BundlePath))
				{
					UnityWebRequest assetBundle = UnityWebRequestAssetBundle.GetAssetBundle(m_BundlePath);
					if (m_WebRequestTimeout > 0)
					{
						assetBundle.timeout = m_WebRequestTimeout;
					}
					m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(assetBundle);
					if (m_WebRequestQueueOperation.IsDone)
					{
						m_RequestOperation = m_WebRequestQueueOperation.Result;
						if (m_RequestOperation.isDone)
						{
							WebRequestOperationCompleted(m_RequestOperation);
						}
						else
						{
							m_RequestOperation.completed += WebRequestOperationCompleted;
						}
					}
					else
					{
						WebRequestQueueOperation webRequestQueueOperation = m_WebRequestQueueOperation;
						webRequestQueueOperation.OnComplete = (Action<UnityWebRequestAsyncOperation>)Delegate.Combine(webRequestQueueOperation.OnComplete, (Action<UnityWebRequestAsyncOperation>)delegate(UnityWebRequestAsyncOperation asyncOp)
						{
							m_RequestOperation = asyncOp;
							m_RequestOperation.completed += WebRequestOperationCompleted;
						});
					}
					return;
				}
				m_LoadBundleRequest = AssetBundle.LoadFromFileAsync(m_BundlePath);
				m_LoadBundleRequest.completed += delegate(AsyncOperation loadOp)
				{
					if (loadOp is AssetBundleCreateRequest assetBundleCreateRequest && assetBundleCreateRequest.assetBundle != null)
					{
						m_CatalogAssetBundle = assetBundleCreateRequest.assetBundle;
						m_LoadTextAssetRequest = m_CatalogAssetBundle.LoadAllAssetsAsync<TextAsset>();
						if (m_LoadTextAssetRequest.isDone)
						{
							LoadTextAssetRequestComplete(m_LoadTextAssetRequest);
						}
						m_LoadTextAssetRequest.completed += LoadTextAssetRequestComplete;
					}
					else
					{
						Addressables.LogError("Unable to load dependent bundle from file location : " + m_BundlePath);
						m_OpInProgress = false;
					}
				};
			}

			private void WebRequestOperationCompleted(AsyncOperation op)
			{
				UnityWebRequestUtilities.LogOperationResult(op);
				UnityWebRequest webRequest = (op as UnityWebRequestAsyncOperation).webRequest;
				DownloadHandlerAssetBundle downloadHandlerAssetBundle = webRequest.downloadHandler as DownloadHandlerAssetBundle;
				if (!UnityWebRequestUtilities.RequestHasErrors(webRequest, out var _))
				{
					m_CatalogAssetBundle = downloadHandlerAssetBundle.assetBundle;
					m_LoadTextAssetRequest = m_CatalogAssetBundle.LoadAllAssetsAsync<TextAsset>();
					if (m_LoadTextAssetRequest.isDone)
					{
						LoadTextAssetRequestComplete(m_LoadTextAssetRequest);
					}
					m_LoadTextAssetRequest.completed += LoadTextAssetRequestComplete;
				}
				else
				{
					Addressables.LogError("Unable to load dependent bundle from remote location : " + m_BundlePath);
					m_OpInProgress = false;
				}
				webRequest.Dispose();
			}

			private void LoadTextAssetRequestComplete(AsyncOperation op)
			{
				if (op is AssetBundleRequest { asset: TextAsset { text: not null } asset })
				{
					m_CatalogData = JsonUtility.FromJson<ContentCatalogData>(asset.text);
					this.OnLoaded?.Invoke(m_CatalogData);
				}
				else
				{
					Addressables.LogError("No catalog text assets where found in bundle " + m_BundlePath);
				}
				Unload();
				m_OpInProgress = false;
			}

			public bool WaitForCompletion()
			{
				if (m_LoadBundleRequest.assetBundle == null)
				{
					return false;
				}
				if (!(m_LoadTextAssetRequest.asset != null))
				{
					return m_LoadTextAssetRequest.allAssets != null;
				}
				return true;
			}
		}

		private string m_LocalDataPath;

		private string m_RemoteHashValue;

		internal string m_LocalHashValue;

		private ProvideHandle m_ProviderInterface;

		internal ContentCatalogData m_ContentCatalogData;

		private AsyncOperationHandle<ContentCatalogData> m_ContentCatalogDataLoadOp;

		private BundledCatalog m_BundledCatalog;

		private bool m_Retried;

		private bool m_DisableCatalogUpdateOnStart;

		private bool m_IsLocalCatalogInBundle;

		private const string kCatalogExt = ".bin";

		public void Start(ProvideHandle providerInterface, bool disableCatalogUpdateOnStart, bool isLocalCatalogInBundle)
		{
			m_ProviderInterface = providerInterface;
			m_DisableCatalogUpdateOnStart = disableCatalogUpdateOnStart;
			m_IsLocalCatalogInBundle = isLocalCatalogInBundle;
			m_ProviderInterface.SetWaitForCompletionCallback(WaitForCompletionCallback);
			m_LocalDataPath = null;
			m_RemoteHashValue = null;
			List<object> list = new List<object>();
			m_ProviderInterface.GetDependencies(list);
			string idToLoad = DetermineIdToLoad(m_ProviderInterface.Location, list, disableCatalogUpdateOnStart);
			bool loadCatalogFromLocalBundle = isLocalCatalogInBundle && CanLoadCatalogFromBundle(idToLoad, m_ProviderInterface.Location);
			LoadCatalog(idToLoad, loadCatalogFromLocalBundle);
		}

		private bool WaitForCompletionCallback()
		{
			if (m_ContentCatalogData != null)
			{
				return true;
			}
			bool flag;
			if (m_BundledCatalog != null)
			{
				flag = m_BundledCatalog.WaitForCompletion();
			}
			else
			{
				flag = m_ContentCatalogDataLoadOp.IsDone;
				if (!flag)
				{
					m_ContentCatalogDataLoadOp.WaitForCompletion();
				}
			}
			if (flag && m_ContentCatalogData == null)
			{
				m_ProviderInterface.ResourceManager.Update(Time.unscaledDeltaTime);
			}
			return flag;
		}

		public void Release()
		{
			m_ContentCatalogData?.CleanData();
		}

		internal bool CanLoadCatalogFromBundle(string idToLoad, IResourceLocation location)
		{
			if (Path.GetExtension(idToLoad) == ".bundle")
			{
				return idToLoad.Equals(GetTransformedInternalId(location));
			}
			return false;
		}

		internal void LoadCatalog(string idToLoad, bool loadCatalogFromLocalBundle)
		{
			try
			{
				ProviderLoadRequestOptions providerLoadRequestOptions = null;
				if (m_ProviderInterface.Location.Data is ProviderLoadRequestOptions providerLoadRequestOptions2)
				{
					providerLoadRequestOptions = providerLoadRequestOptions2.Copy();
				}
				if (loadCatalogFromLocalBundle)
				{
					int webRequestTimeout = providerLoadRequestOptions?.WebRequestTimeout ?? 0;
					m_BundledCatalog = new BundledCatalog(idToLoad, webRequestTimeout);
					m_BundledCatalog.OnLoaded += delegate(ContentCatalogData ccd)
					{
						m_ContentCatalogData = ccd;
						OnCatalogLoaded(ccd);
					};
					m_BundledCatalog.LoadCatalogFromBundleAsync();
				}
				else if (Path.GetExtension(idToLoad) == ".json")
				{
					m_ProviderInterface.Complete<ContentCatalogData>(null, status: false, new Exception("Expecting to load catalogs in binary format but the catalog provided is in .json format. To load it enable Addressable Asset Settings > Catalog > Enable Json Catalog."));
				}
				else
				{
					ResourceLocationBase resourceLocationBase = new ResourceLocationBase(idToLoad, idToLoad, typeof(BinaryAssetProvider<ContentCatalogData.Serializer>).FullName, typeof(ContentCatalogData));
					resourceLocationBase.Data = providerLoadRequestOptions;
					m_ProviderInterface.ResourceManager.ResourceProviders.Add(new BinaryAssetProvider<ContentCatalogData.Serializer>());
					m_ContentCatalogDataLoadOp = m_ProviderInterface.ResourceManager.ProvideResource<ContentCatalogData>(resourceLocationBase);
					m_ContentCatalogDataLoadOp.Completed += CatalogLoadOpCompleteCallback;
				}
			}
			catch (Exception exception)
			{
				m_ProviderInterface.Complete<ContentCatalogData>(null, status: false, exception);
			}
		}

		private void CatalogLoadOpCompleteCallback(AsyncOperationHandle<ContentCatalogData> op)
		{
			m_ContentCatalogData = op.Result;
			op.Release();
			OnCatalogLoaded(m_ContentCatalogData);
		}

		private string GetTransformedInternalId(IResourceLocation loc)
		{
			if (m_ProviderInterface.ResourceManager == null)
			{
				return loc.InternalId;
			}
			return m_ProviderInterface.ResourceManager.TransformInternalId(loc);
		}

		internal string DetermineIdToLoad(IResourceLocation location, IList<object> dependencyObjects, bool disableCatalogUpdateOnStart = false)
		{
			string result = GetTransformedInternalId(location);
			if (dependencyObjects != null && location.Dependencies != null && dependencyObjects.Count == 3 && location.Dependencies.Count == 3)
			{
				string text = dependencyObjects[0] as string;
				m_LocalHashValue = dependencyObjects[1] as string;
				if (string.IsNullOrEmpty(m_LocalHashValue))
				{
					m_LocalHashValue = dependencyObjects[2] as string;
				}
				if (string.IsNullOrEmpty(text) || disableCatalogUpdateOnStart)
				{
					if (!string.IsNullOrEmpty(m_LocalHashValue) && !m_Retried && !string.IsNullOrEmpty(Application.persistentDataPath))
					{
						result = ((!string.IsNullOrEmpty(dependencyObjects[1] as string)) ? GetTransformedInternalId(location.Dependencies[1]).Replace(".hash", ".bin") : GetTransformedInternalId(location.Dependencies[2]).Replace(".hash", ".bin"));
					}
				}
				else if (text == m_LocalHashValue && !m_Retried)
				{
					result = ((!string.IsNullOrEmpty(dependencyObjects[1] as string)) ? GetTransformedInternalId(location.Dependencies[1]).Replace(".hash", ".bin") : GetTransformedInternalId(location.Dependencies[2]).Replace(".hash", ".bin"));
				}
				else
				{
					result = GetTransformedInternalId(location.Dependencies[0]).Replace(".hash", ".bin");
					m_RemoteHashValue = text;
					if (!string.IsNullOrEmpty(Application.persistentDataPath))
					{
						m_LocalDataPath = GetTransformedInternalId(location.Dependencies[1]).Replace(".hash", ".bin");
					}
				}
			}
			return result;
		}

		private void OnCatalogLoaded(ContentCatalogData ccd)
		{
			if (ccd != null)
			{
				ccd.location = m_ProviderInterface.Location;
				ccd.LocalHash = m_LocalHashValue;
				if (!string.IsNullOrEmpty(m_RemoteHashValue) && !string.IsNullOrEmpty(m_LocalDataPath))
				{
					string directoryName = Path.GetDirectoryName(m_LocalDataPath);
					string localDataPath = m_LocalDataPath;
					try
					{
						if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						File.WriteAllBytes(localDataPath, ccd.GetBytes());
						File.WriteAllText(localDataPath.Replace(".bin", ".hash"), m_RemoteHashValue);
					}
					catch (UnauthorizedAccessException ex)
					{
						Addressables.LogWarning("Did not save cached content catalog. Missing access permissions for location " + localDataPath + " : " + ex.Message);
						m_ProviderInterface.Complete(ccd, status: true, null);
						return;
					}
					catch (Exception innerException)
					{
						string transformedInternalId = GetTransformedInternalId(m_ProviderInterface.Location.Dependencies[0]);
						string message = "Unable to load ContentCatalogData from location " + transformedInternalId + ". Failed to cache catalog to location " + localDataPath + ".";
						ccd = null;
						m_ProviderInterface.Complete(ccd, status: false, new Exception(message, innerException));
						return;
					}
					ccd.LocalHash = m_RemoteHashValue;
				}
				else if (string.IsNullOrEmpty(m_LocalDataPath) && string.IsNullOrEmpty(Application.persistentDataPath))
				{
					Addressables.LogWarning("Did not save cached content catalog because Application.persistentDataPath is an empty path.");
				}
				m_ProviderInterface.Complete(ccd, status: true, null);
				return;
			}
			string text = $"Unable to load ContentCatalogData from location {m_ProviderInterface.Location}";
			if (!m_Retried)
			{
				m_Retried = true;
				string transformedInternalId2 = GetTransformedInternalId(m_ProviderInterface.Location.Dependencies[1]);
				if (m_ContentCatalogDataLoadOp.LocationName == transformedInternalId2.Replace(".hash", ".bin"))
				{
					try
					{
						File.Delete(transformedInternalId2);
					}
					catch (Exception)
					{
						text = text + ". Unable to delete cache data from location " + transformedInternalId2;
						m_ProviderInterface.Complete(ccd, status: false, new Exception(text));
						return;
					}
				}
				Addressables.LogWarning(text + ". Attempting to retry...");
				Start(m_ProviderInterface, m_DisableCatalogUpdateOnStart, m_IsLocalCatalogInBundle);
			}
			else
			{
				m_ProviderInterface.Complete(ccd, status: false, new Exception(text + " on second attempt."));
			}
		}
	}

	public bool DisableCatalogUpdateOnStart;

	public bool IsLocalCatalogInBundle;

	internal Dictionary<IResourceLocation, InternalOp> m_LocationToCatalogLoadOpMap = new Dictionary<IResourceLocation, InternalOp>();

	public ContentCatalogProvider(ResourceManager resourceManagerInstance)
	{
		m_BehaviourFlags = ProviderBehaviourFlags.CanProvideWithFailedDependencies;
	}

	public override void Release(IResourceLocation location, object obj)
	{
		if (m_LocationToCatalogLoadOpMap.ContainsKey(location))
		{
			m_LocationToCatalogLoadOpMap[location].Release();
			m_LocationToCatalogLoadOpMap.Remove(location);
		}
		base.Release(location, obj);
	}

	public override void Provide(ProvideHandle providerInterface)
	{
		if (!m_LocationToCatalogLoadOpMap.ContainsKey(providerInterface.Location))
		{
			m_LocationToCatalogLoadOpMap.Add(providerInterface.Location, new InternalOp());
		}
		m_LocationToCatalogLoadOpMap[providerInterface.Location].Start(providerInterface, DisableCatalogUpdateOnStart, IsLocalCatalogInBundle);
	}
}
