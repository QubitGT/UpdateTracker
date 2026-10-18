using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations;

internal class LoadAllTablesOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<IList<TTable>> where TTable : DetailedLocalizationTable<TEntry> where TEntry : TableEntry
{
	private readonly Action<AsyncOperationHandle<IList<TTable>>> m_LoadingCompletedAction;

	private AsyncOperationHandle<IList<TTable>> m_AllTablesOperation;

	private LocalizedDatabase<TTable, TEntry> m_Database;

	private Locale m_SelectedLocale;

	public static readonly ObjectPool<LoadAllTablesOperation<TTable, TEntry>> Pool = new ObjectPool<LoadAllTablesOperation<TTable, TEntry>>(() => new LoadAllTablesOperation<TTable, TEntry>(), null, null, null, collectionCheck: false);

	public LoadAllTablesOperation()
	{
		m_LoadingCompletedAction = LoadingCompleted;
	}

	public void Init(LocalizedDatabase<TTable, TEntry> database, Locale locale)
	{
		m_Database = database;
		m_SelectedLocale = locale;
	}

	protected override void Execute()
	{
		if (m_SelectedLocale == null)
		{
			m_SelectedLocale = LocalizationSettings.SelectedLocale;
			if (m_SelectedLocale == null)
			{
				Complete(null, success: false, "SelectedLocale is null. Could not load table.");
				return;
			}
		}
		string label = ((m_SelectedLocale != null) ? AddressHelper.FormatAssetLabel(m_SelectedLocale.Identifier) : AddressHelper.FormatAssetLabel(LocalizationSettings.SelectedLocaleAsync.Result.Identifier));
		m_AllTablesOperation = AddressablesInterface.LoadAssetsWithLabel<TTable>(label, null);
		if (m_AllTablesOperation.IsDone)
		{
			LoadingCompleted(m_AllTablesOperation);
			return;
		}
		m_AllTablesOperation.Completed += m_LoadingCompletedAction;
		base.CurrentOperation = m_AllTablesOperation;
	}

	private void LoadingCompleted(AsyncOperationHandle<IList<TTable>> obj)
	{
		if (obj.Result != null)
		{
			foreach (TTable item in obj.Result)
			{
				if (!(item == null))
				{
					m_Database.GetTableAsync(item.TableCollectionName, m_SelectedLocale);
				}
			}
		}
		Complete(obj.Result, obj.Status == AsyncOperationStatus.Succeeded, obj.OperationException?.Message);
	}

	protected override void Destroy()
	{
		base.Destroy();
		Pool.Release(this);
		AddressablesInterface.ReleaseAndReset(ref m_AllTablesOperation);
	}
}
