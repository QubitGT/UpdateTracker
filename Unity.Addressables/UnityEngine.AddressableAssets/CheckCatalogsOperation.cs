using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UnityEngine.AddressableAssets;

internal class CheckCatalogsOperation : AsyncOperationBase<List<string>>
{
	private AddressablesImpl m_Addressables;

	private List<string> m_LocalHashes;

	private List<ResourceLocatorInfo> m_LocatorInfos;

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;

	public CheckCatalogsOperation(AddressablesImpl aa)
	{
		m_Addressables = aa;
	}

	public AsyncOperationHandle<List<string>> Start(List<ResourceLocatorInfo> locatorInfos)
	{
		m_LocatorInfos = new List<ResourceLocatorInfo>(locatorInfos.Count);
		m_LocalHashes = new List<string>(locatorInfos.Count);
		List<IResourceLocation> list = new List<IResourceLocation>(locatorInfos.Count);
		foreach (ResourceLocatorInfo locatorInfo in locatorInfos)
		{
			if (locatorInfo.CanUpdateContent)
			{
				list.Add(locatorInfo.HashLocation);
				m_LocalHashes.Add(locatorInfo.LocalHash);
				m_LocatorInfos.Add(locatorInfo);
			}
		}
		if (m_Addressables.ResourceManager.ResourceProviders.FirstOrDefault((IResourceProvider rp) => rp.GetType() == typeof(ContentCatalogProvider)) is ContentCatalogProvider contentCatalogProvider)
		{
			contentCatalogProvider.DisableCatalogUpdateOnStart = false;
		}
		m_DepOp = m_Addressables.ResourceManager.CreateGroupOperation<string>(list);
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
		m_RM?.Update(Time.unscaledDeltaTime);
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

	internal static List<string> ProcessDependentOpResults(IList<AsyncOperationHandle> results, List<ResourceLocatorInfo> locatorInfos, List<string> localHashes, out string errorString, out bool success)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < results.Count; i++)
		{
			AsyncOperationHandle asyncOperationHandle = results[i];
			string text = asyncOperationHandle.Result as string;
			if (!string.IsNullOrEmpty(text) && text != localHashes[i])
			{
				list.Add(locatorInfos[i].Locator.LocatorId);
				locatorInfos[i].ContentUpdateAvailable = true;
			}
			else if (asyncOperationHandle.OperationException != null)
			{
				list.Add(null);
				locatorInfos[i].ContentUpdateAvailable = false;
				list2.Add(asyncOperationHandle.OperationException.Message);
			}
		}
		errorString = null;
		if (list2.Count > 0)
		{
			if (list2.Count == list.Count)
			{
				list = null;
				errorString = "CheckCatalogsOperation failed with the following errors: ";
			}
			else
			{
				errorString = "Partial success in CheckCatalogsOperation with the following errors: ";
			}
			foreach (string item in list2)
			{
				errorString = errorString + "\n" + item;
			}
		}
		success = list2.Count == 0;
		return list;
	}

	protected override void Execute()
	{
		string errorString;
		bool success;
		List<string> result = ProcessDependentOpResults(m_DepOp.Result, m_LocatorInfos, m_LocalHashes, out errorString, out success);
		Complete(result, success, errorString);
	}
}
