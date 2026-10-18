using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations;

internal class PreloadDatabaseOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<LocalizedDatabase<TTable, TEntry>> where TTable : DetailedLocalizationTable<TEntry> where TEntry : TableEntry
{
	private readonly Action<AsyncOperationHandle> m_CompleteOperation;

	private readonly Action<AsyncOperationHandle<IList<AsyncOperationHandle>>> m_CompleteGenericGroup;

	private LocalizedDatabase<TTable, TEntry> m_Database;

	public static readonly ObjectPool<PreloadDatabaseOperation<TTable, TEntry>> Pool = new ObjectPool<PreloadDatabaseOperation<TTable, TEntry>>(() => new PreloadDatabaseOperation<TTable, TEntry>(), null, null, null, collectionCheck: false);

	protected override float Progress
	{
		get
		{
			if (!base.CurrentOperation.IsValid())
			{
				return base.Progress;
			}
			return base.CurrentOperation.PercentComplete;
		}
	}

	protected override string DebugName => $"Preload {m_Database.GetType()}";

	public PreloadDatabaseOperation()
	{
		m_CompleteOperation = CompleteOperation;
		m_CompleteGenericGroup = CompleteGenericGroup;
	}

	public void Init(LocalizedDatabase<TTable, TEntry> database)
	{
		m_Database = database;
	}

	protected override void Execute()
	{
		AsyncOperationHandle<Locale> selectedLocaleAsync = LocalizationSettings.SelectedLocaleAsync;
		if (selectedLocaleAsync.Result == null)
		{
			Complete(m_Database, success: true, (string)null);
			return;
		}
		switch (LocalizationSettings.PreloadBehavior)
		{
		case PreloadBehavior.NoPreloading:
			Complete(m_Database, success: true, (string)null);
			break;
		case PreloadBehavior.PreloadSelectedLocale:
		{
			AsyncOperationHandle asyncOperationHandle = PreloadLocale(selectedLocaleAsync.Result);
			if (asyncOperationHandle.IsDone)
			{
				m_CompleteOperation(asyncOperationHandle);
				break;
			}
			asyncOperationHandle.Completed += m_CompleteOperation;
			base.CurrentOperation = asyncOperationHandle;
			break;
		}
		case PreloadBehavior.PreloadSelectedLocaleAndFallbacks:
		{
			HashSet<Locale> value;
			using (CollectionPool<HashSet<Locale>, Locale>.Get(out value))
			{
				value.Add(selectedLocaleAsync.Result);
				GetAllFallbackLocales(selectedLocaleAsync.Result, value);
				PreloadLocales(value);
				break;
			}
		}
		case PreloadBehavior.PreloadAllLocales:
			PreloadLocales(LocalizationSettings.AvailableLocales.Locales);
			break;
		}
	}

	private void GetAllFallbackLocales(Locale current, HashSet<Locale> locales)
	{
		foreach (Locale fallback in current.GetFallbacks())
		{
			if (!locales.Contains(fallback))
			{
				locales.Add(fallback);
				GetAllFallbackLocales(fallback, locales);
			}
		}
	}

	private AsyncOperationHandle PreloadLocale(Locale locale)
	{
		PreloadLocaleOperation<TTable, TEntry> preloadLocaleOperation = PreloadLocaleOperation<TTable, TEntry>.Pool.Get();
		preloadLocaleOperation.Init(m_Database, locale);
		return AddressablesInterface.ResourceManager.StartOperation(preloadLocaleOperation, default(AsyncOperationHandle));
	}

	private void PreloadLocales(ICollection<Locale> locales)
	{
		List<AsyncOperationHandle> value;
		using (CollectionPool<List<AsyncOperationHandle>, AsyncOperationHandle>.Get(out value))
		{
			foreach (Locale locale in locales)
			{
				AsyncOperationHandle item = PreloadLocale(locale);
				if (!item.IsDone)
				{
					value.Add(item);
				}
			}
			if (value.Count > 0)
			{
				AsyncOperationHandle<IList<AsyncOperationHandle>> asyncOperationHandle = AddressablesInterface.CreateGroupOperation(value);
				asyncOperationHandle.Completed += m_CompleteGenericGroup;
				base.CurrentOperation = asyncOperationHandle;
			}
			else
			{
				Complete(m_Database, success: true, (string)null);
			}
		}
	}

	private void CompleteOperation(AsyncOperationHandle operationHandle)
	{
		AddressablesInterface.Release(operationHandle);
		Complete(m_Database, success: true, (string)null);
	}

	private void CompleteGenericGroup(AsyncOperationHandle<IList<AsyncOperationHandle>> operationHandle)
	{
		AddressablesInterface.Release(operationHandle);
		Complete(m_Database, success: true, (string)null);
	}

	protected override void Destroy()
	{
		base.Destroy();
		Pool.Release(this);
	}
}
