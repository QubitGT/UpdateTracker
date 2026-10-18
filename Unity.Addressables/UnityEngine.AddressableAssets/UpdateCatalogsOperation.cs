using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UnityEngine.AddressableAssets;

internal class UpdateCatalogsOperation : AsyncOperationBase<List<IResourceLocator>>
{
	private AddressablesImpl m_Addressables;

	private List<ResourceLocatorInfo> m_LocatorInfos;

	internal AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;

	private AsyncOperationHandle<bool> m_CleanCacheOp;

	private bool m_AutoCleanBundleCache;

	public UpdateCatalogsOperation(AddressablesImpl aa)
	{
		m_Addressables = aa;
	}

	public AsyncOperationHandle<List<IResourceLocator>> Start(IEnumerable<string> catalogIds, bool autoCleanBundleCache)
	{
		m_LocatorInfos = new List<ResourceLocatorInfo>();
		List<IResourceLocation> list = new List<IResourceLocation>();
		foreach (string catalogId in catalogIds)
		{
			if (catalogId != null)
			{
				ResourceLocatorInfo locatorInfo = m_Addressables.GetLocatorInfo(catalogId);
				list.Add(locatorInfo.CatalogLocation);
				m_LocatorInfos.Add(locatorInfo);
			}
		}
		if (list.Count == 0)
		{
			return m_Addressables.ResourceManager.CreateCompletedOperation<List<IResourceLocator>>(null, "Content update not available.");
		}
		if (m_Addressables.ResourceManager.ResourceProviders.FirstOrDefault((IResourceProvider rp) => rp.GetType() == typeof(ContentCatalogProvider)) is ContentCatalogProvider contentCatalogProvider)
		{
			contentCatalogProvider.DisableCatalogUpdateOnStart = false;
		}
		m_DepOp = m_Addressables.ResourceManager.CreateGroupOperation<object>(list);
		m_AutoCleanBundleCache = autoCleanBundleCache;
		return m_Addressables.ResourceManager.StartOperation(this, m_DepOp);
	}

	protected override bool InvokeWaitForCompletion()
	{
		if (base.IsDone)
		{
			return true;
		}
		if (m_DepOp.IsValid() && !m_DepOp.IsDone)
		{
			m_DepOp.WaitForCompletion();
		}
		m_RM?.Update(Time.unscaledDeltaTime);
		if (!HasExecuted)
		{
			InvokeExecute();
		}
		if (m_CleanCacheOp.IsValid() && !m_CleanCacheOp.IsDone)
		{
			m_CleanCacheOp.WaitForCompletion();
		}
		m_Addressables.ResourceManager.Update(Time.unscaledDeltaTime);
		return base.IsDone;
	}

	protected override void Destroy()
	{
		m_DepOp.Release();
	}

	public override void GetDependencies(List<AsyncOperationHandle> dependencies)
	{
		dependencies.Add(m_DepOp);
	}

	protected override void Execute()
	{
		List<IResourceLocator> list = new List<IResourceLocator>(m_DepOp.Result.Count);
		for (int i = 0; i < m_DepOp.Result.Count; i++)
		{
			IResourceLocator resourceLocator = m_DepOp.Result[i].Result as IResourceLocator;
			string hash = null;
			IResourceLocation loc = null;
			if (resourceLocator == null)
			{
				ContentCatalogData obj = m_DepOp.Result[i].Result as ContentCatalogData;
				resourceLocator = obj.CreateCustomLocator(obj.location.PrimaryKey);
				hash = obj.LocalHash;
				loc = obj.location;
			}
			m_LocatorInfos[i].UpdateContent(resourceLocator, hash, loc);
			list.Add(m_LocatorInfos[i].Locator);
		}
		if (!m_AutoCleanBundleCache)
		{
			if (m_DepOp.Status == AsyncOperationStatus.Succeeded)
			{
				Complete(list, success: true, (string)null);
				return;
			}
			if (m_DepOp.Status == AsyncOperationStatus.Failed)
			{
				Complete(list, success: false, "Cannot update catalogs. Failed to load catalog: " + m_DepOp.OperationException.Message);
				return;
			}
			string text = "Cannot update catalogs. Catalog loading operation is still in progress when it should already be completed. ";
			text += ((m_DepOp.OperationException != null) ? m_DepOp.OperationException.Message : "");
			Complete(list, success: false, text);
		}
		else
		{
			m_CleanCacheOp = m_Addressables.CleanBundleCache(m_DepOp, forceSingleThreading: false);
			OnCleanCacheCompleted(m_CleanCacheOp, list);
		}
	}

	private void OnCleanCacheCompleted(AsyncOperationHandle<bool> handle, List<IResourceLocator> catalogs)
	{
		handle.Completed += delegate(AsyncOperationHandle<bool> obj)
		{
			bool flag = obj.Status == AsyncOperationStatus.Succeeded;
			Complete(catalogs, flag, flag ? null : $"{obj.DebugName}, status={obj.Status}, result={obj.Result} catalogs updated, but failed to clean bundle cache.");
		};
	}
}
