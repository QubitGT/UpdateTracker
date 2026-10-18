using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization.Operations;

internal class LoadTableOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<TTable> where TTable : DetailedLocalizationTable<TEntry> where TEntry : TableEntry
{
	private readonly Action<AsyncOperationHandle<SharedTableData>> m_LoadTableByGuidAction;

	private readonly Action<AsyncOperationHandle<IList<IResourceLocation>>> m_LoadTableResourceAction;

	private readonly Action<AsyncOperationHandle<TTable>> m_TableLoadedAction;

	private readonly Action<AsyncOperationHandle<TTable>> m_CustomTableLoadedAction;

	private LocalizedDatabase<TTable, TEntry> m_Database;

	private TableReference m_TableReference;

	private AsyncOperationHandle<TTable> m_LoadTableOperation;

	private Locale m_SelectedLocale;

	private string m_CollectionName;

	public static readonly ObjectPool<LoadTableOperation<TTable, TEntry>> Pool = new ObjectPool<LoadTableOperation<TTable, TEntry>>(() => new LoadTableOperation<TTable, TEntry>(), null, null, null, collectionCheck: false);

	public Action<AsyncOperationHandle<TTable>> RegisterTableOperation { get; private set; }

	public LoadTableOperation()
	{
		m_LoadTableByGuidAction = LoadTableByGuid;
		m_LoadTableResourceAction = LoadTableResource;
		m_TableLoadedAction = TableLoaded;
		m_CustomTableLoadedAction = CustomTableLoaded;
	}

	public void Init(LocalizedDatabase<TTable, TEntry> database, TableReference tableReference, Locale locale)
	{
		m_Database = database;
		m_TableReference = tableReference;
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
		if (m_TableReference.ReferenceType == TableReference.Type.Guid)
		{
			AsyncOperationHandle<SharedTableData> sharedTableData = m_Database.GetSharedTableData(m_TableReference.TableCollectionNameGuid);
			if (sharedTableData.IsDone)
			{
				LoadTableByGuid(sharedTableData);
				return;
			}
			base.CurrentOperation = sharedTableData;
			sharedTableData.Completed += m_LoadTableByGuidAction;
		}
		else
		{
			FindTableByName(m_TableReference.TableCollectionName);
		}
	}

	private void LoadTableByGuid(AsyncOperationHandle<SharedTableData> operationHandle)
	{
		if (operationHandle.Status == AsyncOperationStatus.Succeeded)
		{
			FindTableByName(operationHandle.Result.TableCollectionName);
		}
		else
		{
			Complete(null, success: false, $"Failed to extract table name from shared table data {m_TableReference}. Load Shared Table data operation failed.");
		}
	}

	private void FindTableByName(string collectionName)
	{
		m_CollectionName = collectionName;
		if (!TryLoadWithTableProvider())
		{
			DefaultLoadTableByName();
		}
	}

	private bool TryLoadWithTableProvider()
	{
		if (m_Database.TableProvider != null)
		{
			m_LoadTableOperation = m_Database.TableProvider.ProvideTableAsync<TTable>(m_CollectionName, m_SelectedLocale);
			if (m_LoadTableOperation.IsValid())
			{
				if (m_LoadTableOperation.IsDone)
				{
					CustomTableLoaded(m_LoadTableOperation);
				}
				else
				{
					m_LoadTableOperation.Completed += m_CustomTableLoadedAction;
					base.CurrentOperation = m_LoadTableOperation;
				}
				return true;
			}
		}
		return false;
	}

	private void DefaultLoadTableByName()
	{
		AsyncOperationHandle<IList<IResourceLocation>> asyncOperationHandle = AddressablesInterface.LoadTableLocationsAsync(m_CollectionName, m_SelectedLocale.Identifier, typeof(TTable));
		if (asyncOperationHandle.IsDone)
		{
			LoadTableResource(asyncOperationHandle);
			return;
		}
		base.CurrentOperation = asyncOperationHandle;
		asyncOperationHandle.Completed += m_LoadTableResourceAction;
	}

	private void LoadTableResource(AsyncOperationHandle<IList<IResourceLocation>> operationHandle)
	{
		if (operationHandle.Status != AsyncOperationStatus.Succeeded || operationHandle.Result.Count == 0)
		{
			AddressablesInterface.Release(operationHandle);
			Complete(null, success: true, $"Could not find a {m_SelectedLocale} table with the name '{m_CollectionName}`");
			return;
		}
		m_LoadTableOperation = AddressablesInterface.LoadTableFromLocation<TTable>(operationHandle.Result[0]);
		if (m_LoadTableOperation.IsDone)
		{
			TableLoaded(m_LoadTableOperation);
		}
		else
		{
			base.CurrentOperation = m_LoadTableOperation;
			m_LoadTableOperation.Completed += m_TableLoadedAction;
		}
		AddressablesInterface.Release(operationHandle);
	}

	private void CustomTableLoaded(AsyncOperationHandle<TTable> operationHandle)
	{
		if (operationHandle.Status == AsyncOperationStatus.Succeeded && operationHandle.Result != null)
		{
			Complete(operationHandle.Result, success: true, (string)null);
		}
		else
		{
			DefaultLoadTableByName();
		}
	}

	private void TableLoaded(AsyncOperationHandle<TTable> operationHandle)
	{
		Complete(operationHandle.Result, operationHandle.Status == AsyncOperationStatus.Succeeded, (string)null);
	}

	protected override void Destroy()
	{
		base.Destroy();
		AddressablesInterface.ReleaseAndReset(ref m_LoadTableOperation);
		Pool.Release(this);
	}

	public override string ToString()
	{
		return $"{GetType().Name}, Selected Locale: {m_SelectedLocale}, Table: {m_TableReference}";
	}
}
