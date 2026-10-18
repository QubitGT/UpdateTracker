using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.AddressableAssets;

internal class CleanBundleCacheOperation : AsyncOperationBase<bool>, IUpdateReceiver
{
	private AddressablesImpl m_Addressables;

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;

	private List<string> m_CacheDirsForRemoval;

	private Thread m_EnumerationThread;

	private string m_BaseCachePath;

	private bool m_UseMultiThreading;

	public CleanBundleCacheOperation(AddressablesImpl aa, bool forceSingleThreading)
	{
		m_Addressables = aa;
		m_UseMultiThreading = !forceSingleThreading && PlatformUtilities.PlatformUsesMultiThreading(Application.platform);
	}

	public AsyncOperationHandle<bool> Start(AsyncOperationHandle<IList<AsyncOperationHandle>> depOp)
	{
		m_DepOp = depOp.Acquire();
		return m_Addressables.ResourceManager.StartOperation(this, m_DepOp);
	}

	public void CompleteInternal(bool result, bool success, string errorMsg)
	{
		m_DepOp.Release();
		Complete(result, success, errorMsg);
	}

	protected override bool InvokeWaitForCompletion()
	{
		if (!m_DepOp.IsDone)
		{
			m_DepOp.WaitForCompletion();
		}
		if (!HasExecuted)
		{
			InvokeExecute();
		}
		if (m_EnumerationThread != null)
		{
			m_EnumerationThread.Join();
			RemoveCacheEntries();
		}
		return base.IsDone;
	}

	protected override void Destroy()
	{
		if (m_DepOp.IsValid())
		{
			m_DepOp.Release();
		}
	}

	public override void GetDependencies(List<AsyncOperationHandle> dependencies)
	{
		dependencies.Add(m_DepOp);
	}

	protected override void Execute()
	{
		if (m_DepOp.Status == AsyncOperationStatus.Failed)
		{
			CompleteInternal(result: false, success: false, "Could not clean cache because a dependent catalog operation failed.");
			return;
		}
		HashSet<string> cacheDirsInUse = GetCacheDirsInUse(m_DepOp.Result);
		if (!Caching.ready)
		{
			CompleteInternal(result: false, success: false, "Cache is not ready to be accessed.");
		}
		m_BaseCachePath = Caching.currentCacheForWriting.path;
		if (m_UseMultiThreading)
		{
			m_EnumerationThread = new Thread(DetermineCacheDirsNotInUse);
			m_EnumerationThread.Start(cacheDirsInUse);
		}
		else
		{
			DetermineCacheDirsNotInUse(cacheDirsInUse);
			RemoveCacheEntries();
		}
	}

	void IUpdateReceiver.Update(float unscaledDeltaTime)
	{
		if (m_UseMultiThreading && !m_EnumerationThread.IsAlive)
		{
			m_EnumerationThread = null;
			RemoveCacheEntries();
		}
	}

	private void RemoveCacheEntries()
	{
		foreach (string item in m_CacheDirsForRemoval)
		{
			Caching.ClearAllCachedVersions(Path.GetFileName(item));
		}
		CompleteInternal(result: true, success: true, null);
	}

	private void DetermineCacheDirsNotInUse(object data)
	{
		DetermineCacheDirsNotInUse((HashSet<string>)data);
	}

	private void DetermineCacheDirsNotInUse(HashSet<string> cacheDirsInUse)
	{
		m_CacheDirsForRemoval = new List<string>();
		if (!Directory.Exists(m_BaseCachePath))
		{
			return;
		}
		foreach (string item in Directory.EnumerateDirectories(m_BaseCachePath, "*", SearchOption.TopDirectoryOnly))
		{
			if (!cacheDirsInUse.Contains(item))
			{
				m_CacheDirsForRemoval.Add(item);
			}
		}
	}

	private HashSet<string> GetCacheDirsInUse(IList<AsyncOperationHandle> catalogOps)
	{
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < catalogOps.Count; i++)
		{
			IResourceLocator resourceLocator = catalogOps[i].Result as IResourceLocator;
			if (resourceLocator == null)
			{
				if (!(catalogOps[i].Result is ContentCatalogData contentCatalogData))
				{
					return hashSet;
				}
				resourceLocator = contentCatalogData.CreateCustomLocator(contentCatalogData.location.PrimaryKey);
			}
			foreach (IResourceLocation allLocation in resourceLocator.AllLocations)
			{
				if (allLocation.Data is AssetBundleRequestOptions assetBundleRequestOptions)
				{
					AssetBundleResource.GetLoadInfo(allLocation, m_Addressables.ResourceManager, out var loadType, out var _);
					if (loadType == AssetBundleResource.LoadType.Web)
					{
						string item = Path.Combine(Caching.currentCacheForWriting.path, assetBundleRequestOptions.BundleName);
						hashSet.Add(item);
					}
				}
			}
		}
		return hashSet;
	}
}
