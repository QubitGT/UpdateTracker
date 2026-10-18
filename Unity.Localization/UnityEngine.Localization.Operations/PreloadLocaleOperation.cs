using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization.Operations;

internal class PreloadLocaleOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<LocalizedDatabase<TTable, TEntry>> where TTable : DetailedLocalizationTable<TEntry> where TEntry : TableEntry
{
	private readonly Action<AsyncOperationHandle<IList<IResourceLocation>>> m_LoadTablesAction;

	private readonly Action<AsyncOperationHandle<TTable>> m_LoadTableContentsAction;

	private readonly Action<AsyncOperationHandle> m_FinishPreloadingAction;

	private readonly Action<AsyncOperationHandle<IList<AsyncOperationHandle>>> m_PreloadTablesCompletedAction;

	private LocalizedDatabase<TTable, TEntry> m_Database;

	private Locale m_Locale;

	private AsyncOperationHandle<IList<IResourceLocation>> m_LoadResourcesOperation;

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_LoadTablesGroupOperation;

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_LoadTableContentsOperation;

	private readonly List<AsyncOperationHandle> m_LoadTablesOperations = new List<AsyncOperationHandle>();

	private readonly List<AsyncOperationHandle> m_PreloadTableContentsOperations = new List<AsyncOperationHandle>();

	private readonly List<string> m_ResourceLabels = new List<string>();

	private float m_Progress;

	public static readonly ObjectPool<PreloadLocaleOperation<TTable, TEntry>> Pool = new ObjectPool<PreloadLocaleOperation<TTable, TEntry>>(() => new PreloadLocaleOperation<TTable, TEntry>(), null, null, null, collectionCheck: false);

	protected override float Progress => m_Progress;

	protected override string DebugName => $"Preload ({m_Locale}) {m_Database.GetType()}";

	public PreloadLocaleOperation()
	{
		m_LoadTablesAction = LoadTables;
		m_LoadTableContentsAction = LoadTableContents;
		m_FinishPreloadingAction = FinishPreloading;
		m_PreloadTablesCompletedAction = PreloadTablesCompleted;
	}

	public void Init(LocalizedDatabase<TTable, TEntry> database, Locale locale)
	{
		m_Database = database;
		m_Locale = locale;
		m_LoadTablesOperations.Clear();
		m_PreloadTableContentsOperations.Clear();
	}

	protected override void Execute()
	{
		BeginPreloading();
	}

	private void BeginPreloading()
	{
		m_Progress = 0f;
		string item = AddressHelper.FormatAssetLabel(m_Locale.Identifier);
		m_ResourceLabels.Clear();
		m_ResourceLabels.Add(item);
		m_ResourceLabels.Add("Preload");
		m_LoadResourcesOperation = AddressablesInterface.LoadResourceLocationsWithLabelsAsync(m_ResourceLabels, Addressables.MergeMode.Intersection, typeof(TTable));
		if (!m_LoadResourcesOperation.IsValid())
		{
			CompleteAndRelease(success: true, null);
			return;
		}
		if (m_LoadResourcesOperation.IsDone)
		{
			LoadTables(m_LoadResourcesOperation);
			return;
		}
		base.CurrentOperation = m_LoadResourcesOperation;
		m_LoadResourcesOperation.Completed += m_LoadTablesAction;
	}

	private void LoadTables(AsyncOperationHandle<IList<IResourceLocation>> loadResourcesOperation)
	{
		if (loadResourcesOperation.Status != AsyncOperationStatus.Succeeded)
		{
			CompleteAndRelease(success: false, "Failed to locate preload tables for " + m_Locale);
			return;
		}
		if (loadResourcesOperation.Result.Count == 0)
		{
			m_Progress = 1f;
			CompleteAndRelease(success: true, null);
			return;
		}
		foreach (IResourceLocation item in loadResourcesOperation.Result)
		{
			AsyncOperationHandle<TTable> asyncOperationHandle = AddressablesInterface.LoadTableFromLocation<TTable>(item);
			m_LoadTablesOperations.Add(asyncOperationHandle);
			if (asyncOperationHandle.IsDone)
			{
				LoadTableContents(asyncOperationHandle);
			}
			else
			{
				asyncOperationHandle.Completed += m_LoadTableContentsAction;
			}
		}
		m_LoadTablesGroupOperation = AddressablesInterface.CreateGroupOperation(m_LoadTablesOperations);
		if (m_LoadTablesGroupOperation.IsDone)
		{
			PreloadTablesCompleted(m_LoadTablesGroupOperation);
			return;
		}
		base.CurrentOperation = m_LoadTablesGroupOperation;
		m_LoadTablesGroupOperation.Completed += m_PreloadTablesCompletedAction;
	}

	private void LoadTableContents(AsyncOperationHandle<TTable> operation)
	{
		m_Progress += 1f / (float)m_LoadTablesOperations.Count;
		if (operation.Result == null)
		{
			return;
		}
		TTable result = operation.Result;
		string tableCollectionName = result.TableCollectionName;
		if (m_Database.TableOperations.TryGetValue((result.LocaleIdentifier, tableCollectionName), out var value))
		{
			LocalizationBehaviour.ReleaseNextFrame(operation);
			if (value.IsDone && (object)value.Result != result)
			{
				Debug.LogError($"A table with the same key `{tableCollectionName}` already exists. Something went wrong during preloading of {m_Locale}. Table {result} does not match {value.Result}.");
				return;
			}
		}
		else
		{
			m_Database.RegisterCompletedTableOperation(operation);
		}
		if (result is IPreloadRequired { PreloadOperation: { IsDone: false } preloadOperation })
		{
			m_PreloadTableContentsOperations.Add(preloadOperation);
		}
	}

	private void PreloadTablesCompleted(AsyncOperationHandle<IList<AsyncOperationHandle>> obj)
	{
		if (m_PreloadTableContentsOperations.Count == 0)
		{
			CompleteAndRelease(success: true, null);
			return;
		}
		m_LoadTableContentsOperation = AddressablesInterface.CreateGroupOperation(m_PreloadTableContentsOperations);
		if (m_LoadTableContentsOperation.IsDone)
		{
			FinishPreloading(m_LoadTableContentsOperation);
			return;
		}
		base.CurrentOperation = m_LoadTableContentsOperation;
		m_LoadTableContentsOperation.CompletedTypeless += m_FinishPreloadingAction;
	}

	private void FinishPreloading(AsyncOperationHandle op)
	{
		m_Progress = 1f;
		CompleteAndRelease(op.Status == AsyncOperationStatus.Succeeded, null);
	}

	private void CompleteAndRelease(bool success, string errorMsg)
	{
		AddressablesInterface.ReleaseAndReset(ref m_LoadResourcesOperation);
		AddressablesInterface.ReleaseAndReset(ref m_LoadTablesGroupOperation);
		AddressablesInterface.ReleaseAndReset(ref m_LoadTableContentsOperation);
		Complete(m_Database, success, errorMsg);
	}

	protected override void Destroy()
	{
		base.Destroy();
		Pool.Release(this);
	}
}
