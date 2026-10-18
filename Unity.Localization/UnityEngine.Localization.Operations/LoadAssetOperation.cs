using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations;

internal class LoadAssetOperation<TObject> : WaitForCurrentOperationAsyncOperationBase<TObject> where TObject : Object
{
	private readonly Action<AsyncOperationHandle<TObject>> m_AssetLoadedAction;

	private AsyncOperationHandle<LocalizedDatabase<UnityEngine.Localization.Tables.AssetTable, UnityEngine.Localization.Tables.AssetTableEntry>.TableEntryResult> m_TableEntryOperation;

	private AsyncOperationHandle<TObject> m_LoadAssetOperation;

	private bool m_AutoRelease;

	public static readonly ObjectPool<LoadAssetOperation<TObject>> Pool = new ObjectPool<LoadAssetOperation<TObject>>(() => new LoadAssetOperation<TObject>(), null, null, null, collectionCheck: false);

	public LoadAssetOperation()
	{
		m_AssetLoadedAction = AssetLoaded;
	}

	public void Init(AsyncOperationHandle<LocalizedDatabase<UnityEngine.Localization.Tables.AssetTable, UnityEngine.Localization.Tables.AssetTableEntry>.TableEntryResult> loadTableEntryOperation, bool autoRelease)
	{
		m_TableEntryOperation = loadTableEntryOperation;
		AddressablesInterface.Acquire(m_TableEntryOperation);
		m_AutoRelease = autoRelease;
	}

	protected override void Execute()
	{
		if (m_TableEntryOperation.Status != AsyncOperationStatus.Succeeded)
		{
			Complete(null, success: false, "Load Table Entry Operation Failed");
			AddressablesInterface.Release(m_TableEntryOperation);
			return;
		}
		if (m_TableEntryOperation.Result.Table == null || m_TableEntryOperation.Result.Entry == null)
		{
			CompleteAndRelease(null, success: true, null);
			return;
		}
		m_LoadAssetOperation = m_TableEntryOperation.Result.Table.GetAssetAsync<TObject>(m_TableEntryOperation.Result.Entry);
		AddressablesInterface.Acquire(m_LoadAssetOperation);
		if (m_LoadAssetOperation.IsDone)
		{
			AssetLoaded(m_LoadAssetOperation);
			return;
		}
		base.CurrentOperation = m_LoadAssetOperation;
		m_LoadAssetOperation.Completed += m_AssetLoadedAction;
	}

	private void AssetLoaded(AsyncOperationHandle<TObject> handle)
	{
		if (handle.Status != AsyncOperationStatus.Succeeded)
		{
			CompleteAndRelease(null, success: false, "GetAssetAsync failed to load the asset.");
		}
		else
		{
			CompleteAndRelease(handle.Result, success: true, null);
		}
	}

	public void CompleteAndRelease(TObject result, bool success, string errorMsg)
	{
		Complete(result, success, errorMsg);
		AddressablesInterface.Release(m_TableEntryOperation);
		if (m_AutoRelease && LocalizationSettings.Instance.IsPlaying)
		{
			LocalizationBehaviour.ReleaseNextFrame(base.Handle);
		}
	}

	protected override void Destroy()
	{
		AddressablesInterface.ReleaseAndReset(ref m_LoadAssetOperation);
		base.Destroy();
		Pool.Release(this);
	}
}
