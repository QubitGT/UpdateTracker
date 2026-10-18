using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations;

internal class PreloadTablesOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<LocalizedDatabase<TTable, TEntry>> where TTable : DetailedLocalizationTable<TEntry> where TEntry : TableEntry
{
	private LocalizedDatabase<TTable, TEntry> m_Database;

	private readonly List<AsyncOperationHandle<TTable>> m_LoadTables = new List<AsyncOperationHandle<TTable>>();

	private readonly List<AsyncOperationHandle> m_LoadTablesOperation = new List<AsyncOperationHandle>();

	private readonly List<AsyncOperationHandle> m_PreloadTablesOperations = new List<AsyncOperationHandle>();

	private readonly Action<AsyncOperationHandle<IList<AsyncOperationHandle>>> m_LoadTableContentsAction;

	private readonly Action<AsyncOperationHandle> m_FinishPreloadingAction;

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_LoadTablesOperationHandle;

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_PreloadTablesContentsHandle;

	private IList<TableReference> m_TableReferences;

	private Locale m_SelectedLocale;

	public static readonly ObjectPool<PreloadTablesOperation<TTable, TEntry>> Pool = new ObjectPool<PreloadTablesOperation<TTable, TEntry>>(() => new PreloadTablesOperation<TTable, TEntry>(), null, null, null, collectionCheck: false);

	public PreloadTablesOperation()
	{
		m_LoadTableContentsAction = delegate(AsyncOperationHandle<IList<AsyncOperationHandle>> a)
		{
			LoadTableContents();
			AddressablesInterface.Release(a);
		};
		m_FinishPreloadingAction = FinishPreloading;
	}

	public void Init(LocalizedDatabase<TTable, TEntry> database, IList<TableReference> tableReference, Locale locale = null)
	{
		m_Database = database;
		m_TableReferences = tableReference;
		m_SelectedLocale = locale;
	}

	protected override void Execute()
	{
		BeginPreloadingTables();
	}

	private void BeginPreloadingTables()
	{
		foreach (TableReference tableReference in m_TableReferences)
		{
			AsyncOperationHandle<TTable> tableAsync = m_Database.GetTableAsync(tableReference, m_SelectedLocale);
			m_LoadTables.Add(tableAsync);
			if (!tableAsync.IsDone)
			{
				m_LoadTablesOperation.Add(tableAsync);
			}
		}
		if (m_LoadTablesOperation.Count > 0)
		{
			m_LoadTablesOperationHandle = AddressablesInterface.CreateGroupOperation(m_LoadTablesOperation);
			if (!m_LoadTablesOperationHandle.IsDone)
			{
				base.CurrentOperation = m_LoadTablesOperationHandle;
				m_LoadTablesOperationHandle.Completed += m_LoadTableContentsAction;
				return;
			}
		}
		LoadTableContents();
	}

	private void LoadTableContents()
	{
		foreach (AsyncOperationHandle<TTable> loadTable in m_LoadTables)
		{
			if (loadTable.Result == null)
			{
				Complete(null, success: false, "Table is null.");
				return;
			}
			if (loadTable.Result is IPreloadRequired preloadRequired)
			{
				m_PreloadTablesOperations.Add(preloadRequired.PreloadOperation);
			}
		}
		if (m_PreloadTablesOperations.Count == 0)
		{
			Complete(m_Database, success: true, (string)null);
			return;
		}
		m_PreloadTablesContentsHandle = AddressablesInterface.CreateGroupOperation(m_PreloadTablesOperations);
		if (!m_PreloadTablesContentsHandle.IsDone)
		{
			base.CurrentOperation = m_PreloadTablesContentsHandle;
			m_PreloadTablesContentsHandle.CompletedTypeless += m_FinishPreloadingAction;
		}
		else
		{
			FinishPreloading(m_PreloadTablesContentsHandle);
		}
	}

	private void FinishPreloading(AsyncOperationHandle op)
	{
		Complete(m_Database, op.Status == AsyncOperationStatus.Succeeded, (string)null);
	}

	protected override void Destroy()
	{
		base.Destroy();
		AddressablesInterface.ReleaseAndReset(ref m_LoadTablesOperationHandle);
		AddressablesInterface.ReleaseAndReset(ref m_PreloadTablesContentsHandle);
		m_LoadTables.Clear();
		m_LoadTablesOperation.Clear();
		m_PreloadTablesOperations.Clear();
		m_TableReferences = null;
		Pool.Release(this);
	}
}
