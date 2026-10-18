using System;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations;

internal class LoadSubAssetOperation<TObject> : WaitForCurrentOperationAsyncOperationBase<TObject> where TObject : Object
{
	private readonly Action<AsyncOperationHandle<TObject>> m_AssetLoadedAction;

	private AsyncOperationHandle<TObject> m_AssetOperation;

	private AsyncOperationHandle<Object[]> m_PreloadOperations;

	private string m_Address;

	private bool m_IsSubAsset;

	private string m_SubAssetName;

	public static readonly ObjectPool<LoadSubAssetOperation<TObject>> Pool = new ObjectPool<LoadSubAssetOperation<TObject>>(() => new LoadSubAssetOperation<TObject>(), null, null, null, collectionCheck: false);

	public LoadSubAssetOperation()
	{
		m_AssetLoadedAction = AssetLoaded;
	}

	public void Init(AsyncOperationHandle<Object[]> preloadOperations, string address, bool isSubAsset, string subAssetName)
	{
		base.Dependency = preloadOperations;
		m_PreloadOperations = preloadOperations;
		if (m_PreloadOperations.IsValid())
		{
			AddressablesInterface.Acquire(m_PreloadOperations);
		}
		m_Address = address;
		m_IsSubAsset = isSubAsset;
		m_SubAssetName = subAssetName;
	}

	protected override void Execute()
	{
		if (m_PreloadOperations.IsValid())
		{
			if (m_PreloadOperations.Status != AsyncOperationStatus.Succeeded)
			{
				Complete(null, success: false, m_PreloadOperations.OperationException.Message);
				return;
			}
			Object[] result = m_PreloadOperations.Result;
			foreach (Object @object in result)
			{
				if (@object is TObject result2 && (!m_IsSubAsset || !(m_SubAssetName != @object.name)))
				{
					Complete(result2, success: true, (string)null);
					return;
				}
			}
		}
		m_AssetOperation = AddressablesInterface.LoadAssetFromGUID<TObject>(m_Address);
		if (m_AssetOperation.IsDone)
		{
			AssetLoaded(m_AssetOperation);
			return;
		}
		base.CurrentOperation = m_AssetOperation;
		m_AssetOperation.Completed += m_AssetLoadedAction;
	}

	private void AssetLoaded(AsyncOperationHandle<TObject> handle)
	{
		if (handle.Status != AsyncOperationStatus.Succeeded)
		{
			Complete(null, success: false, $"Failed to load sub-asset {m_SubAssetName} from the address {m_Address}.");
		}
		else
		{
			Complete(handle.Result, success: true, (string)null);
		}
	}

	protected override void Destroy()
	{
		AddressablesInterface.ReleaseAndReset(ref m_PreloadOperations);
		AddressablesInterface.ReleaseAndReset(ref m_AssetOperation);
		base.Destroy();
		Pool.Release(this);
	}
}
