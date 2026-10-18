using System;
using System.Collections.Generic;
using UnityEngine.Localization.Operations;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings;

[Serializable]
public abstract class LocalizedDatabase<TTable, TEntry> : IPreloadRequired, IReset, IDisposable where TTable : DetailedLocalizationTable<TEntry> where TEntry : TableEntry
{
	public struct TableEntryResult
	{
		public TEntry Entry { get; }

		public TTable Table { get; }

		internal TableEntryResult(TEntry entry, TTable table)
		{
			Entry = entry;
			Table = table;
		}
	}

	[SerializeField]
	private TableReference m_DefaultTableReference;

	[SerializeReference]
	private ITableProvider m_CustomTableProvider;

	[SerializeReference]
	private ITablePostprocessor m_CustomTablePostprocessor;

	[SerializeField]
	private AsynchronousBehaviour m_AsynchronousBehaviour;

	[SerializeField]
	private bool m_UseFallback;

	internal AsyncOperationHandle m_PreloadOperationHandle;

	private Action<AsyncOperationHandle> m_ReleaseNextFrame;

	private readonly Action<AsyncOperationHandle<TTable>> m_PatchTableContentsAction;

	private readonly Action<AsyncOperationHandle<TTable>> m_RegisterSharedTableAndGuidOperationAction;

	private readonly Action<AsyncOperationHandle<TTable>> m_RegisterCompletedTableOperationAction;

	internal static readonly LocaleIdentifier k_SelectedLocaleId = new LocaleIdentifier("selected locale placeholder");

	public AsyncOperationHandle PreloadOperation
	{
		get
		{
			if (!m_PreloadOperationHandle.IsValid())
			{
				PreloadDatabaseOperation<TTable, TEntry> preloadDatabaseOperation = PreloadDatabaseOperation<TTable, TEntry>.Pool.Get();
				preloadDatabaseOperation.Init(this);
				m_PreloadOperationHandle = AddressablesInterface.ResourceManager.StartOperation(preloadDatabaseOperation, default(AsyncOperationHandle));
			}
			return m_PreloadOperationHandle;
		}
	}

	internal Action<AsyncOperationHandle> ReleaseNextFrame => m_ReleaseNextFrame;

	internal Dictionary<(LocaleIdentifier localeIdentifier, string tableNameOrGuid), AsyncOperationHandle<TTable>> TableOperations { get; } = new Dictionary<(LocaleIdentifier, string), AsyncOperationHandle<TTable>>();


	internal Dictionary<Guid, AsyncOperationHandle<SharedTableData>> SharedTableDataOperations { get; } = new Dictionary<Guid, AsyncOperationHandle<SharedTableData>>();


	public virtual TableReference DefaultTable
	{
		get
		{
			return m_DefaultTableReference;
		}
		set
		{
			m_DefaultTableReference = value;
		}
	}

	public ITableProvider TableProvider
	{
		get
		{
			return m_CustomTableProvider;
		}
		set
		{
			m_CustomTableProvider = value;
		}
	}

	public ITablePostprocessor TablePostprocessor
	{
		get
		{
			return m_CustomTablePostprocessor;
		}
		set
		{
			m_CustomTablePostprocessor = value;
		}
	}

	public bool UseFallback
	{
		get
		{
			return m_UseFallback;
		}
		set
		{
			m_UseFallback = value;
		}
	}

	public AsynchronousBehaviour AsynchronousBehaviour
	{
		get
		{
			return m_AsynchronousBehaviour;
		}
		set
		{
			m_AsynchronousBehaviour = value;
		}
	}

	public LocalizedDatabase()
	{
		m_PatchTableContentsAction = PatchTableContents;
		m_RegisterSharedTableAndGuidOperationAction = RegisterSharedTableAndGuidOperation;
		m_RegisterCompletedTableOperationAction = RegisterCompletedTableOperation;
		m_ReleaseNextFrame = LocalizationBehaviour.ReleaseNextFrame;
	}

	internal TableReference GetDefaultTable()
	{
		if (m_DefaultTableReference.ReferenceType == TableReference.Type.Empty)
		{
			throw new Exception("Trying to get the DefaultTable however the " + GetType().Name + " DefaultTable value has not been set. This can be configured in the Localization Settings.");
		}
		return m_DefaultTableReference;
	}

	internal void RegisterCompletedTableOperation(AsyncOperationHandle<TTable> tableOperation)
	{
		if (!tableOperation.IsDone)
		{
			tableOperation.Completed += m_RegisterCompletedTableOperationAction;
			return;
		}
		TTable result = tableOperation.Result;
		if (!(result == null))
		{
			RegisterTableNameOperation(tableOperation, result.LocaleIdentifier, result.TableCollectionName);
			if (tableOperation.IsValid())
			{
				RegisterSharedTableAndGuidOperation(tableOperation);
			}
		}
	}

	private void RegisterTableNameOperation(AsyncOperationHandle<TTable> tableOperation, LocaleIdentifier localeIdentifier, string tableName)
	{
		(LocaleIdentifier, string) key = (localeIdentifier, tableName);
		if (TableOperations.ContainsKey(key))
		{
			return;
		}
		TableOperations[key] = tableOperation;
		if (TablePostprocessor != null)
		{
			if (tableOperation.IsDone)
			{
				PatchTableContents(tableOperation);
			}
			else
			{
				tableOperation.Completed += m_PatchTableContentsAction;
			}
		}
	}

	private void RegisterSharedTableAndGuidOperation(AsyncOperationHandle<TTable> tableOperation)
	{
		if (!tableOperation.IsDone)
		{
			tableOperation.Completed += m_RegisterSharedTableAndGuidOperationAction;
			return;
		}
		TTable result = tableOperation.Result;
		if (!(result == null))
		{
			Guid tableCollectionNameGuid = result.SharedData.TableCollectionNameGuid;
			if (!SharedTableDataOperations.ContainsKey(tableCollectionNameGuid))
			{
				SharedTableDataOperations[tableCollectionNameGuid] = AddressablesInterface.ResourceManager.CreateCompletedOperation(result.SharedData, null);
			}
			(LocaleIdentifier, string) key = (result.LocaleIdentifier, TableReference.StringFromGuid(tableCollectionNameGuid));
			if (!TableOperations.ContainsKey(key))
			{
				AddressablesInterface.Acquire(tableOperation);
				TableOperations[key] = tableOperation;
			}
		}
	}

	public AsyncOperationHandle<TTable> GetDefaultTableAsync()
	{
		return GetTableAsync(GetDefaultTable());
	}

	public virtual AsyncOperationHandle<TTable> GetTableAsync(TableReference tableReference, Locale locale = null)
	{
		bool num = locale != null || LocalizationSettings.SelectedLocaleAsync.IsDone;
		bool flag = true;
		if (num)
		{
			if (locale == null)
			{
				if (LocalizationSettings.SelectedLocaleAsync.Result == null)
				{
					return AddressablesInterface.ResourceManager.CreateCompletedOperation<TTable>(null, "SelectedLocale is null. Database could not get table.");
				}
				locale = LocalizationSettings.SelectedLocaleAsync.Result;
			}
			flag = false;
		}
		tableReference.Validate();
		string text = ((tableReference.ReferenceType == TableReference.Type.Guid) ? TableReference.StringFromGuid(tableReference.TableCollectionNameGuid) : tableReference.TableCollectionName);
		LocaleIdentifier localeIdentifier = (flag ? k_SelectedLocaleId : locale.Identifier);
		if (TableOperations.TryGetValue((localeIdentifier, text), out var value))
		{
			if (value.IsValid())
			{
				return value;
			}
			TableOperations.Remove((localeIdentifier, text));
		}
		LoadTableOperation<TTable, TEntry> loadTableOperation = CreateLoadTableOperation();
		loadTableOperation.Init(this, tableReference, locale);
		loadTableOperation.Dependency = LocalizationSettings.InitializationOperation;
		AsyncOperationHandle<TTable> asyncOperationHandle = AddressablesInterface.ResourceManager.StartOperation(loadTableOperation, LocalizationSettings.InitializationOperation);
		if (flag || tableReference.ReferenceType == TableReference.Type.Guid)
		{
			if (!flag)
			{
				AddressablesInterface.Acquire(asyncOperationHandle);
			}
			TableOperations[(localeIdentifier, text)] = asyncOperationHandle;
		}
		else
		{
			RegisterTableNameOperation(asyncOperationHandle, localeIdentifier, text);
		}
		RegisterCompletedTableOperation(asyncOperationHandle);
		return asyncOperationHandle;
	}

	public virtual TTable GetTable(TableReference tableReference, Locale locale = null)
	{
		return GetTableAsync(tableReference, locale).WaitForCompletion();
	}

	public AsyncOperationHandle PreloadTables(TableReference tableReference, Locale locale = null)
	{
		PreloadTablesOperation<TTable, TEntry> preloadTablesOperation = CreatePreloadTablesOperation();
		preloadTablesOperation.Init(this, new TableReference[1] { tableReference }, locale);
		preloadTablesOperation.Dependency = LocalizationSettings.InitializationOperation;
		AsyncOperationHandle<LocalizedDatabase<TTable, TEntry>> asyncOperationHandle = AddressablesInterface.ResourceManager.StartOperation(preloadTablesOperation, LocalizationSettings.InitializationOperation);
		if (LocalizationSettings.Instance.IsPlaying)
		{
			asyncOperationHandle.CompletedTypeless += ReleaseNextFrame;
		}
		return asyncOperationHandle;
	}

	public AsyncOperationHandle PreloadTables(IList<TableReference> tableReferences, Locale locale = null)
	{
		PreloadTablesOperation<TTable, TEntry> preloadTablesOperation = CreatePreloadTablesOperation();
		preloadTablesOperation.Init(this, tableReferences, locale);
		preloadTablesOperation.Dependency = LocalizationSettings.InitializationOperation;
		AsyncOperationHandle<LocalizedDatabase<TTable, TEntry>> asyncOperationHandle = AddressablesInterface.ResourceManager.StartOperation(preloadTablesOperation, LocalizationSettings.InitializationOperation);
		if (LocalizationSettings.Instance.IsPlaying)
		{
			asyncOperationHandle.CompletedTypeless += ReleaseNextFrame;
		}
		return asyncOperationHandle;
	}

	public void ReleaseAllTables(Locale locale = null)
	{
		HashSet<TTable> value;
		using (CollectionPool<HashSet<TTable>, TTable>.Get(out value))
		{
			foreach (AsyncOperationHandle<TTable> value2 in TableOperations.Values)
			{
				if (value2.IsValid() && (!(locale != null) || !(value2.Result.LocaleIdentifier != locale.Identifier)))
				{
					if (value2.Result != null && !value.Contains(value2.Result))
					{
						ReleaseTableContents(value2.Result);
						value.Add(value2.Result);
					}
					AddressablesInterface.Release(value2);
				}
			}
		}
		foreach (KeyValuePair<Guid, AsyncOperationHandle<SharedTableData>> sharedTableDataOperation in SharedTableDataOperations)
		{
			AddressablesInterface.SafeRelease(sharedTableDataOperation.Value);
		}
		SharedTableDataOperations.Clear();
		if (m_PreloadOperationHandle.IsValid())
		{
			if (m_PreloadOperationHandle.IsDone)
			{
				AddressablesInterface.Release(m_PreloadOperationHandle);
			}
			m_PreloadOperationHandle = default(AsyncOperationHandle);
		}
		TableOperations.Clear();
	}

	public void ReleaseTable(TableReference tableReference, Locale locale = null)
	{
		tableReference.Validate();
		bool flag = locale == LocalizationSettings.SelectedLocaleAsync.Result;
		if (locale == null)
		{
			locale = LocalizationSettings.SelectedLocaleAsync.Result;
			flag = true;
			if (locale == null)
			{
				return;
			}
		}
		SharedTableData sharedTableData;
		if (tableReference.ReferenceType == TableReference.Type.Guid)
		{
			if (!SharedTableDataOperations.TryGetValue(tableReference.TableCollectionNameGuid, out var value) || value.Result == null)
			{
				return;
			}
			sharedTableData = value.Result;
		}
		else
		{
			(LocaleIdentifier, string) key = (locale.Identifier, tableReference.TableCollectionName);
			if (!TableOperations.TryGetValue(key, out var value2) || value2.Result == null)
			{
				return;
			}
			sharedTableData = value2.Result.SharedData;
		}
		if (sharedTableData == null)
		{
			return;
		}
		int num = 0;
		bool flag2 = false;
		List<(LocaleIdentifier, string)> value3;
		using (CollectionPool<List<(LocaleIdentifier, string)>, (LocaleIdentifier, string)>.Get(out value3))
		{
			foreach (KeyValuePair<(LocaleIdentifier, string), AsyncOperationHandle<TTable>> tableOperation in TableOperations)
			{
				if (!tableOperation.Value.IsValid() || tableOperation.Value.Result == null || tableOperation.Value.Result.SharedData != sharedTableData)
				{
					continue;
				}
				if (tableOperation.Key.Item1 == locale.Identifier || (flag && tableOperation.Key.Item1 == k_SelectedLocaleId))
				{
					if (!flag2)
					{
						ReleaseTableContents(tableOperation.Value.Result);
						flag2 = true;
					}
					AddressablesInterface.SafeRelease(tableOperation.Value);
					value3.Add(tableOperation.Key);
				}
				else
				{
					num++;
				}
			}
			foreach (var item in value3)
			{
				TableOperations.Remove(item);
			}
			if (num == 0 && SharedTableDataOperations.TryGetValue(sharedTableData.TableCollectionNameGuid, out var value4))
			{
				AddressablesInterface.SafeRelease(value4);
				SharedTableDataOperations.Remove(sharedTableData.TableCollectionNameGuid);
			}
		}
	}

	public virtual AsyncOperationHandle<IList<TTable>> GetAllTables(Locale locale = null)
	{
		LoadAllTablesOperation<TTable, TEntry> loadAllTablesOperation = LoadAllTablesOperation<TTable, TEntry>.Pool.Get();
		loadAllTablesOperation.Init(this, locale);
		loadAllTablesOperation.Dependency = LocalizationSettings.InitializationOperation;
		AsyncOperationHandle<IList<TTable>> result = AddressablesInterface.ResourceManager.StartOperation(loadAllTablesOperation, LocalizationSettings.InitializationOperation);
		if (LocalizationSettings.Instance.IsPlaying)
		{
			result.CompletedTypeless += ReleaseNextFrame;
		}
		return result;
	}

	public virtual bool IsTableLoaded(TableReference tableReference, Locale locale = null)
	{
		string item = ((tableReference.ReferenceType == TableReference.Type.Guid) ? TableReference.StringFromGuid(tableReference.TableCollectionNameGuid) : tableReference.TableCollectionName);
		(LocaleIdentifier, string) key = ((locale != null) ? (locale.Identifier, item) : (LocalizationSettings.SelectedLocaleAsync.Result.Identifier, item));
		if (TableOperations.TryGetValue(key, out var value))
		{
			return value.Status == AsyncOperationStatus.Succeeded;
		}
		return false;
	}

	internal virtual LoadTableOperation<TTable, TEntry> CreateLoadTableOperation()
	{
		return LoadTableOperation<TTable, TEntry>.Pool.Get();
	}

	internal virtual PreloadTablesOperation<TTable, TEntry> CreatePreloadTablesOperation()
	{
		return PreloadTablesOperation<TTable, TEntry>.Pool.Get();
	}

	public virtual AsyncOperationHandle<TableEntryResult> GetTableEntryAsync(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale = null, FallbackBehavior fallbackBehavior = FallbackBehavior.UseProjectSettings)
	{
		AsyncOperationHandle<TTable> tableAsync = GetTableAsync(tableReference, locale);
		GetTableEntryOperation<TTable, TEntry> getTableEntryOperation = GetTableEntryOperation<TTable, TEntry>.Pool.Get();
		bool useFallBack = ((fallbackBehavior != 0) ? (fallbackBehavior == FallbackBehavior.UseFallback) : UseFallback);
		getTableEntryOperation.Init(this, tableAsync, tableReference, tableEntryReference, locale, useFallBack, autoRelease: true);
		getTableEntryOperation.Dependency = tableAsync;
		return AddressablesInterface.ResourceManager.StartOperation(getTableEntryOperation, tableAsync);
	}

	public virtual TableEntryResult GetTableEntry(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale = null, FallbackBehavior fallbackBehavior = FallbackBehavior.UseProjectSettings)
	{
		return GetTableEntryAsync(tableReference, tableEntryReference, locale, fallbackBehavior).WaitForCompletion();
	}

	internal AsyncOperationHandle<SharedTableData> GetSharedTableData(Guid tableNameGuid)
	{
		if (SharedTableDataOperations.TryGetValue(tableNameGuid, out var value))
		{
			return value;
		}
		value = AddressablesInterface.LoadAssetFromGUID<SharedTableData>(TableReference.StringFromGuid(tableNameGuid));
		SharedTableDataOperations[tableNameGuid] = value;
		return value;
	}

	internal virtual void ReleaseTableContents(TTable table)
	{
	}

	public virtual void OnLocaleChanged(Locale locale)
	{
		ReleaseAllTables();
	}

	private void PatchTableContents(AsyncOperationHandle<TTable> tableOperation)
	{
		if (TablePostprocessor != null && tableOperation.Result != null)
		{
			TablePostprocessor.PostprocessTable(tableOperation.Result);
		}
	}

	public void ResetState()
	{
		ReleaseAllTables();
	}

	void IDisposable.Dispose()
	{
		ReleaseAllTables();
	}
}
