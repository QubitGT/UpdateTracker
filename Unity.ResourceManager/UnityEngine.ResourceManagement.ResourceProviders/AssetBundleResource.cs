using System;
using System.IO;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.ResourceProviders;

public class AssetBundleResource : IAssetBundleResource, IUpdateReceiver
{
	public enum LoadType
	{
		None,
		Local,
		Web
	}

	internal enum CacheStatus
	{
		Unknown,
		Cached,
		NotCached
	}

	private AssetBundle m_AssetBundle;

	private AsyncOperation m_RequestOperation;

	internal WebRequestQueueOperation m_WebRequestQueueOperation;

	internal ProvideHandle m_ProvideHandle;

	internal AssetBundleRequestOptions m_Options;

	internal CacheStatus cacheStatus;

	[NonSerialized]
	private bool m_RequestCompletedCallbackCalled;

	private int m_Retries;

	private BundleSource m_Source;

	private long m_BytesToDownload;

	private long m_DownloadedBytes;

	private bool m_Completed;

	private AssetBundleUnloadOperation m_UnloadOperation;

	private const int k_WaitForWebRequestMainThreadSleep = 1;

	private string m_TransformedInternalId;

	private AssetBundleRequest m_PreloadRequest;

	private bool m_PreloadCompleted;

	private ulong m_LastDownloadedByteCount;

	private float m_TimeoutTimer;

	private int m_TimeoutOverFrames;

	internal bool m_DownloadOnly;

	private int m_LastFrameCount = -1;

	private float m_TimeSecSinceLastUpdate;

	internal Func<UnityWebRequestResult, bool> m_RequestRetryCallback = (UnityWebRequestResult x) => x.ShouldRetryDownloadError();

	private bool HasTimedOut
	{
		get
		{
			if (m_Options != null && m_TimeoutTimer >= (float)m_Options.Timeout)
			{
				return m_TimeoutOverFrames > 5;
			}
			return false;
		}
	}

	internal long BytesToDownload
	{
		get
		{
			if (m_BytesToDownload == -1)
			{
				if (m_Options != null && !IsCached())
				{
					m_BytesToDownload = m_Options.ComputeSize(m_ProvideHandle.Location, m_ProvideHandle.ResourceManager);
				}
				else
				{
					m_BytesToDownload = 0L;
				}
			}
			return m_BytesToDownload;
		}
	}

	internal bool IsCached()
	{
		if (cacheStatus != 0)
		{
			return cacheStatus == CacheStatus.Cached;
		}
		cacheStatus = CacheStatus.NotCached;
		Hash128 hash = Hash128.Parse(m_Options.Hash);
		if (hash.isValid && Caching.IsVersionCached(new CachedAssetBundle(m_Options.BundleName, hash)))
		{
			cacheStatus = CacheStatus.Cached;
		}
		return cacheStatus == CacheStatus.Cached;
	}

	internal UnityWebRequest CreateWebRequest(IResourceLocation loc)
	{
		string url = m_ProvideHandle.ResourceManager.TransformInternalId(loc);
		return CreateWebRequest(url);
	}

	internal UnityWebRequest CreateWebRequest(string url)
	{
		Uri uri = new Uri(Uri.UnescapeDataString(url).Replace(" ", "%20"));
		if (m_Options == null)
		{
			m_Source = BundleSource.Download;
			return UnityWebRequestAssetBundle.GetAssetBundle(uri);
		}
		UnityWebRequest unityWebRequest;
		if (!string.IsNullOrEmpty(m_Options.Hash))
		{
			bool flag = IsCached();
			CachedAssetBundle cachedAssetBundle = new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
			m_Source = (flag ? BundleSource.Cache : BundleSource.Download);
			unityWebRequest = ((!m_Options.UseCrcForCachedBundle && m_Source != BundleSource.Download) ? UnityWebRequestAssetBundle.GetAssetBundle(uri, cachedAssetBundle) : UnityWebRequestAssetBundle.GetAssetBundle(uri, cachedAssetBundle, m_Options.Crc));
		}
		else
		{
			m_Source = BundleSource.Download;
			unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri, m_Options.Crc);
		}
		if (m_Options.RedirectLimit >= 0 && m_Options.RedirectLimit < 129)
		{
			unityWebRequest.redirectLimit = m_Options.RedirectLimit;
		}
		if (m_ProvideHandle.ResourceManager.CertificateHandlerInstance != null)
		{
			unityWebRequest.certificateHandler = m_ProvideHandle.ResourceManager.CertificateHandlerInstance;
			unityWebRequest.disposeCertificateHandlerOnDispose = false;
		}
		m_ProvideHandle.ResourceManager.WebRequestOverride?.Invoke(unityWebRequest);
		return unityWebRequest;
	}

	public AssetBundleRequest GetAssetPreloadRequest()
	{
		if (m_PreloadCompleted || GetAssetBundle() == null || m_Options == null)
		{
			return null;
		}
		if (m_Options.AssetLoadMode == AssetLoadMode.AllPackedAssetsAndDependencies)
		{
			if (m_PreloadRequest == null)
			{
				m_PreloadRequest = m_AssetBundle.LoadAllAssetsAsync();
				m_PreloadRequest.completed += delegate
				{
					m_PreloadCompleted = true;
				};
			}
			return m_PreloadRequest;
		}
		return null;
	}

	private float PercentComplete()
	{
		if (m_RequestOperation == null)
		{
			return 0f;
		}
		return m_RequestOperation.progress;
	}

	private DownloadStatus GetDownloadStatus()
	{
		if (m_Options == null)
		{
			return default(DownloadStatus);
		}
		DownloadStatus downloadStatus = default(DownloadStatus);
		downloadStatus.TotalBytes = BytesToDownload;
		downloadStatus.IsDone = PercentComplete() >= 1f;
		DownloadStatus result = downloadStatus;
		if (BytesToDownload > 0)
		{
			if (m_WebRequestQueueOperation != null && string.IsNullOrEmpty(m_WebRequestQueueOperation.m_WebRequest.error))
			{
				m_DownloadedBytes = (long)m_WebRequestQueueOperation.m_WebRequest.downloadedBytes;
			}
			else if (m_RequestOperation != null && m_RequestOperation is UnityWebRequestAsyncOperation unityWebRequestAsyncOperation && string.IsNullOrEmpty(unityWebRequestAsyncOperation.webRequest.error))
			{
				m_DownloadedBytes = (long)unityWebRequestAsyncOperation.webRequest.downloadedBytes;
			}
		}
		result.DownloadedBytes = m_DownloadedBytes;
		return result;
	}

	public AssetBundle GetAssetBundle()
	{
		_ = m_ProvideHandle.IsValid;
		return m_AssetBundle;
	}

	private void OnUnloadOperationComplete(AsyncOperation op)
	{
		m_UnloadOperation = null;
		BeginOperation();
	}

	public void Start(ProvideHandle provideHandle, AssetBundleUnloadOperation unloadOp, Func<UnityWebRequestResult, bool> requestRetryCallback)
	{
		m_Retries = 0;
		m_AssetBundle = null;
		m_RequestOperation = null;
		m_RequestCompletedCallbackCalled = false;
		m_ProvideHandle = provideHandle;
		m_Options = m_ProvideHandle.Location.Data as AssetBundleRequestOptions;
		m_BytesToDownload = -1L;
		m_DownloadOnly = m_ProvideHandle.Location is DownloadOnlyLocation;
		if (m_DownloadOnly && m_Options == null)
		{
			m_ProvideHandle.Complete<AssetBundleResource>(null, status: false, new RemoteProviderException("Attempt made to download bundle with stripped AssetBundleRequestOptions.  Ensure that StripDownloadOptions is not enabled for this bundle's group. '" + m_TransformedInternalId + "'."));
			return;
		}
		m_ProvideHandle.SetProgressCallback(PercentComplete);
		m_ProvideHandle.SetDownloadProgressCallbacks(GetDownloadStatus);
		m_ProvideHandle.SetWaitForCompletionCallback(WaitForCompletionHandler);
		m_RequestRetryCallback = requestRetryCallback;
		m_UnloadOperation = unloadOp;
		if (m_UnloadOperation != null && !m_UnloadOperation.isDone)
		{
			m_UnloadOperation.completed += OnUnloadOperationComplete;
		}
		else
		{
			BeginOperation();
		}
	}

	private bool WaitForCompletionHandler()
	{
		if (m_UnloadOperation != null && !m_UnloadOperation.isDone)
		{
			m_UnloadOperation.completed -= OnUnloadOperationComplete;
			m_UnloadOperation.WaitForCompletion();
			m_UnloadOperation = null;
			BeginOperation();
		}
		if (m_RequestOperation == null)
		{
			if (m_WebRequestQueueOperation == null)
			{
				return false;
			}
			WebRequestQueue.WaitForRequestToBeActive(m_WebRequestQueueOperation, 1);
		}
		if (m_RequestOperation is UnityWebRequestAsyncOperation unityWebRequestAsyncOperation)
		{
			while (!UnityWebRequestUtilities.IsAssetBundleDownloaded(unityWebRequestAsyncOperation))
			{
				Thread.Sleep(1);
			}
			if (m_Source == BundleSource.Cache)
			{
				DownloadHandlerAssetBundle downloadHandlerAssetBundle = (DownloadHandlerAssetBundle)(unityWebRequestAsyncOperation?.webRequest?.downloadHandler);
				if (downloadHandlerAssetBundle.autoLoadAssetBundle)
				{
					m_AssetBundle = downloadHandlerAssetBundle.assetBundle;
				}
			}
			WebRequestQueue.DequeueRequest(unityWebRequestAsyncOperation);
			if (!m_RequestCompletedCallbackCalled)
			{
				m_RequestOperation.completed -= WebRequestOperationCompleted;
				WebRequestOperationCompleted(m_RequestOperation);
			}
		}
		if (!m_Completed && m_Source == BundleSource.Local && !m_RequestCompletedCallbackCalled)
		{
			m_RequestOperation.completed -= LocalRequestOperationCompleted;
			LocalRequestOperationCompleted(m_RequestOperation);
		}
		if (!m_Completed && m_RequestOperation.isDone)
		{
			m_ProvideHandle.Complete(this, m_AssetBundle != null, null);
			m_Completed = true;
		}
		return m_Completed;
	}

	private void AddCallbackInvokeIfDone(AsyncOperation operation, Action<AsyncOperation> callback)
	{
		if (operation.isDone)
		{
			callback(operation);
		}
		else
		{
			operation.completed += callback;
		}
	}

	public static void GetLoadInfo(ProvideHandle handle, out LoadType loadType, out string path)
	{
		GetLoadInfo(handle.Location, handle.ResourceManager, out loadType, out path);
	}

	internal static void GetLoadInfo(IResourceLocation location, ResourceManager resourceManager, out LoadType loadType, out string path)
	{
		if (!(location?.Data is AssetBundleRequestOptions assetBundleRequestOptions))
		{
			loadType = LoadType.Local;
			path = resourceManager.TransformInternalId(location);
			if (ResourceManagerConfig.ShouldPathUseWebRequest(path))
			{
				Debug.LogWarning($"Location {location} appears to be remote but the download option have been stripped.  Ensure that the group that contains this bundle does not have StripDownloadOptions enabled.");
			}
			return;
		}
		path = resourceManager.TransformInternalId(location);
		if (Application.platform == RuntimePlatform.Android && path.StartsWith("jar:", StringComparison.Ordinal))
		{
			loadType = ((!assetBundleRequestOptions.UseUnityWebRequestForLocalBundles) ? LoadType.Local : LoadType.Web);
		}
		else if (ResourceManagerConfig.ShouldPathUseWebRequest(path))
		{
			loadType = LoadType.Web;
		}
		else if (assetBundleRequestOptions.UseUnityWebRequestForLocalBundles)
		{
			path = "file:///" + Path.GetFullPath(path);
			loadType = LoadType.Web;
		}
		else
		{
			loadType = LoadType.Local;
		}
		if (loadType == LoadType.Web)
		{
			path = path.Replace('\\', '/');
		}
	}

	private void BeginOperation()
	{
		m_DownloadedBytes = 0L;
		m_RequestCompletedCallbackCalled = false;
		GetLoadInfo(m_ProvideHandle, out var loadType, out m_TransformedInternalId);
		bool flag = m_ProvideHandle.Location is DownloadOnlyLocation;
		if (loadType == LoadType.Local)
		{
			if (flag)
			{
				m_Source = BundleSource.Local;
				m_RequestOperation = null;
				m_ProvideHandle.Complete<AssetBundleResource>(null, status: true, null);
				m_Completed = true;
			}
			else
			{
				LoadLocalBundle();
			}
			return;
		}
		bool useCrcForCachedBundle = m_Options.UseCrcForCachedBundle;
		new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
		bool flag2 = IsCached();
		if (loadType == LoadType.Web && flag && flag2 && !useCrcForCachedBundle)
		{
			m_Source = BundleSource.Cache;
			m_RequestOperation = null;
			m_ProvideHandle.Complete<AssetBundleResource>(null, status: true, null);
			m_Completed = true;
		}
		else if (loadType == LoadType.Web)
		{
			m_WebRequestQueueOperation = EnqueueWebRequest(m_TransformedInternalId);
			AddBeginWebRequestHandler(m_WebRequestQueueOperation);
		}
		else
		{
			m_Source = BundleSource.None;
			m_RequestOperation = null;
			m_ProvideHandle.Complete<AssetBundleResource>(null, status: false, new RemoteProviderException($"Invalid path in AssetBundleProvider: '{m_TransformedInternalId}'.", m_ProvideHandle.Location));
			m_Completed = true;
		}
	}

	private void LoadLocalBundle()
	{
		m_Source = BundleSource.Local;
		m_RequestOperation = AssetBundle.LoadFromFileAsync(m_TransformedInternalId, (m_Options != null) ? m_Options.Crc : 0u);
		AddCallbackInvokeIfDone(m_RequestOperation, LocalRequestOperationCompleted);
	}

	internal WebRequestQueueOperation EnqueueWebRequest(string internalId)
	{
		UnityWebRequest unityWebRequest = CreateWebRequest(internalId);
		((DownloadHandlerAssetBundle)unityWebRequest.downloadHandler).autoLoadAssetBundle = !(m_ProvideHandle.Location is DownloadOnlyLocation);
		unityWebRequest.disposeDownloadHandlerOnDispose = false;
		return WebRequestQueue.QueueRequest(unityWebRequest);
	}

	internal void AddBeginWebRequestHandler(WebRequestQueueOperation webRequestQueueOperation)
	{
		if (webRequestQueueOperation.IsDone)
		{
			BeginWebRequestOperation(webRequestQueueOperation.Result);
			return;
		}
		webRequestQueueOperation.OnComplete = (Action<UnityWebRequestAsyncOperation>)Delegate.Combine(webRequestQueueOperation.OnComplete, (Action<UnityWebRequestAsyncOperation>)delegate(UnityWebRequestAsyncOperation asyncOp)
		{
			BeginWebRequestOperation(asyncOp);
		});
	}

	private void BeginWebRequestOperation(AsyncOperation asyncOp)
	{
		m_TimeoutTimer = 0f;
		m_TimeoutOverFrames = 0;
		m_LastDownloadedByteCount = 0uL;
		m_RequestOperation = asyncOp;
		if (m_RequestOperation == null || m_RequestOperation.isDone)
		{
			WebRequestOperationCompleted(m_RequestOperation);
			return;
		}
		if (m_Options != null && m_Options.Timeout > 0)
		{
			m_ProvideHandle.ResourceManager.AddUpdateReceiver(this);
		}
		m_RequestOperation.completed += WebRequestOperationCompleted;
	}

	public void Update(float unscaledDeltaTime)
	{
		if (m_RequestOperation == null || !(m_RequestOperation is UnityWebRequestAsyncOperation { isDone: false } unityWebRequestAsyncOperation))
		{
			return;
		}
		if (m_LastDownloadedByteCount != unityWebRequestAsyncOperation.webRequest.downloadedBytes)
		{
			m_TimeoutTimer = 0f;
			m_TimeoutOverFrames = 0;
			m_LastDownloadedByteCount = unityWebRequestAsyncOperation.webRequest.downloadedBytes;
			m_LastFrameCount = -1;
			m_TimeSecSinceLastUpdate = 0f;
			return;
		}
		float num = unscaledDeltaTime;
		if (m_LastFrameCount == Time.frameCount)
		{
			num = Time.realtimeSinceStartup - m_TimeSecSinceLastUpdate;
		}
		m_TimeoutTimer += num;
		if (HasTimedOut)
		{
			unityWebRequestAsyncOperation.webRequest.Abort();
		}
		m_TimeoutOverFrames++;
		m_LastFrameCount = Time.frameCount;
		m_TimeSecSinceLastUpdate = Time.realtimeSinceStartup;
	}

	private void LocalRequestOperationCompleted(AsyncOperation op)
	{
		if (!m_RequestCompletedCallbackCalled)
		{
			m_RequestCompletedCallbackCalled = true;
			UnityWebRequestUtilities.LogOperationResult(op);
			CompleteBundleLoad((op as AssetBundleCreateRequest).assetBundle);
		}
	}

	private void CompleteBundleLoad(AssetBundle bundle)
	{
		m_AssetBundle = bundle;
		if (m_AssetBundle != null)
		{
			m_ProvideHandle.Complete(this, status: true, null);
		}
		else
		{
			m_ProvideHandle.Complete<AssetBundleResource>(null, status: false, new RemoteProviderException($"Invalid path in AssetBundleProvider: '{m_TransformedInternalId}'.", m_ProvideHandle.Location));
		}
		m_Completed = true;
	}

	private void WebRequestOperationCompleted(AsyncOperation op)
	{
		if (m_RequestCompletedCallbackCalled)
		{
			return;
		}
		m_RequestCompletedCallbackCalled = true;
		if (m_Options != null && m_Options.Timeout > 0)
		{
			m_ProvideHandle.ResourceManager.RemoveUpdateReciever(this);
		}
		UnityWebRequest unityWebRequest = (op as UnityWebRequestAsyncOperation)?.webRequest;
		DownloadHandlerAssetBundle downloadHandlerAssetBundle = unityWebRequest?.downloadHandler as DownloadHandlerAssetBundle;
		UnityWebRequestResult result = null;
		if (unityWebRequest != null && !UnityWebRequestUtilities.RequestHasErrors(unityWebRequest, out result))
		{
			if (!m_Completed)
			{
				if (!(m_ProvideHandle.Location is DownloadOnlyLocation))
				{
					m_AssetBundle = downloadHandlerAssetBundle.assetBundle;
				}
				downloadHandlerAssetBundle.Dispose();
				downloadHandlerAssetBundle = null;
				m_ProvideHandle.Complete(this, status: true, null);
				m_Completed = true;
			}
			if (m_Options != null && !string.IsNullOrEmpty(m_Options.Hash) && m_Options.ClearOtherCachedVersionsWhenLoaded)
			{
				Caching.ClearOtherCachedVersions(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
			}
		}
		else
		{
			if (HasTimedOut)
			{
				result.Error = "Request timeout";
			}
			unityWebRequest = m_WebRequestQueueOperation.m_WebRequest;
			if (result == null)
			{
				result = new UnityWebRequestResult(m_WebRequestQueueOperation.m_WebRequest);
			}
			downloadHandlerAssetBundle = unityWebRequest.downloadHandler as DownloadHandlerAssetBundle;
			downloadHandlerAssetBundle.Dispose();
			downloadHandlerAssetBundle = null;
			if (m_Options != null)
			{
				bool flag = false;
				string format = $"Web request failed, retrying ({m_Retries}/{m_Options.RetryCount})...\n{result}";
				bool flag2 = m_RequestRetryCallback(result);
				if (!string.IsNullOrEmpty(m_Options.Hash) && m_Source == BundleSource.Cache)
				{
					format = $"Web request failed to load from cache. The cached AssetBundle will be cleared from the cache and re-downloaded. Retrying...\n{result}";
					Caching.ClearCachedVersion(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
					if (m_Retries == 0 && flag2)
					{
						Debug.LogFormat(format);
						BeginOperation();
						m_Retries++;
						flag = true;
					}
				}
				if (!flag)
				{
					if (m_Retries < m_Options.RetryCount && flag2)
					{
						m_Retries++;
						Debug.LogFormat(format);
						BeginOperation();
					}
					else
					{
						format = "Unable to load asset bundle from : " + unityWebRequest.url;
						if (!flag2 && m_Options.RetryCount > 0)
						{
							format += $"\nRetry count set to {m_Options.RetryCount} but cannot retry request due to error {result.Error}. To override use a custom AssetBundle provider.";
						}
						RemoteProviderException exception = new RemoteProviderException(format, m_ProvideHandle.Location, result);
						m_ProvideHandle.Complete<AssetBundleResource>(null, status: false, exception);
						m_Completed = true;
					}
				}
			}
		}
		unityWebRequest.Dispose();
	}

	public bool Unload(out AssetBundleUnloadOperation unloadOp)
	{
		unloadOp = null;
		if (m_AssetBundle != null)
		{
			unloadOp = m_AssetBundle.UnloadAsync(unloadAllLoadedObjects: true);
			m_AssetBundle = null;
		}
		m_RequestOperation = null;
		return unloadOp != null;
	}
}
