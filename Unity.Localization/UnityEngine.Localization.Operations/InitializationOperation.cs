using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UnityEngine.Localization.Operations;

internal class InitializationOperation : WaitForCurrentOperationAsyncOperationBase<LocalizationSettings>
{
	private class UnloadBundlesOperation : AsyncOperationBase<object>
	{
		private readonly Action<AsyncOperation> m_OperationCompleted;

		private readonly List<AsyncOperation> m_UnloadBundleOperations = new List<AsyncOperation>();

		public UnloadBundlesOperation()
		{
			m_OperationCompleted = OnOperationCompleted;
		}

		protected override void Execute()
		{
			if (AssetBundleProvider.UnloadingAssetBundleCount == 0)
			{
				Complete((object)null, success: true, (string)null);
				return;
			}
			m_UnloadBundleOperations.Clear();
			foreach (AssetBundleUnloadOperation value in AssetBundleProvider.UnloadingBundles.Values)
			{
				if (!value.isDone)
				{
					m_UnloadBundleOperations.Add(value);
					value.completed += m_OperationCompleted;
				}
			}
		}

		private void OnOperationCompleted(AsyncOperation obj)
		{
			m_UnloadBundleOperations.Remove(obj);
			if (m_UnloadBundleOperations.Count == 0)
			{
				Complete((object)null, success: true, (string)null);
			}
		}

		protected override bool InvokeWaitForCompletion()
		{
			AssetBundleProvider.WaitForAllUnloadingBundlesToComplete();
			return true;
		}

		protected override void Destroy()
		{
			GenericPool<UnloadBundlesOperation>.Release(this);
		}
	}

	private AsyncOperationHandle m_UnloadBundlesOperationHandle;

	private readonly Action<AsyncOperationHandle> m_LoadLocales;

	internal const string k_LocaleError = "Failed to initialize localization, could not load the selected locale.\n{0}";

	internal const string k_PreloadAssetTablesError = "Failed to initialize localization, could not preload asset tables.\n{0}";

	internal const string k_PreloadStringTablesError = "Failed to initialize localization, could not preload string tables.\n{0}";

	private readonly Action<AsyncOperationHandle<Locale>> m_LoadLocalesCompletedAction;

	private readonly Action<AsyncOperationHandle> m_FinishPreloadingTablesAction;

	private LocalizationSettings m_Settings;

	private readonly List<AsyncOperationHandle> m_LoadDatabasesOperations = new List<AsyncOperationHandle>();

	private AsyncOperationHandle<IList<AsyncOperationHandle>> m_PreloadDatabasesOperation;

	private int m_RemainingSteps;

	private const int k_PreloadSteps = 3;

	public static readonly ObjectPool<InitializationOperation> Pool = new ObjectPool<InitializationOperation>(() => new InitializationOperation(), null, null, null, collectionCheck: false);

	protected override float Progress
	{
		get
		{
			if (base.CurrentOperation.IsValid())
			{
				return ((float)(3 - m_RemainingSteps) + base.CurrentOperation.PercentComplete) / 4f;
			}
			return base.Progress;
		}
	}

	protected override string DebugName => "Localization Settings Initialization";

	public InitializationOperation()
	{
		m_LoadLocalesCompletedAction = LoadLocalesCompleted;
		m_FinishPreloadingTablesAction = delegate
		{
			PreloadTablesCompleted();
		};
		m_LoadLocales = delegate
		{
			LoadLocales();
		};
	}

	public void Init(LocalizationSettings settings)
	{
		m_Settings = settings;
		m_LoadDatabasesOperations.Clear();
		m_RemainingSteps = 3;
	}

	protected override void Execute()
	{
		UnloadBundlesOperation operation = GenericPool<UnloadBundlesOperation>.Get();
		m_UnloadBundlesOperationHandle = AddressablesInterface.ResourceManager.StartOperation(operation, default(AsyncOperationHandle));
		if (!m_UnloadBundlesOperationHandle.IsDone)
		{
			base.CurrentOperation = m_UnloadBundlesOperationHandle;
			m_UnloadBundlesOperationHandle.Completed += m_LoadLocales;
		}
		else
		{
			LoadLocales();
		}
	}

	private void LoadLocales()
	{
		AddressablesInterface.SafeRelease(m_UnloadBundlesOperationHandle);
		AsyncOperationHandle<Locale> selectedLocaleAsync = m_Settings.GetSelectedLocaleAsync();
		if (!selectedLocaleAsync.IsDone)
		{
			base.CurrentOperation = selectedLocaleAsync;
			selectedLocaleAsync.Completed += m_LoadLocalesCompletedAction;
		}
		else
		{
			LoadLocalesCompleted(selectedLocaleAsync);
		}
	}

	private bool CheckOperationSucceeded(AsyncOperationHandle handle, string errorMessage)
	{
		if (handle.Status != AsyncOperationStatus.Succeeded)
		{
			FinishInitializing(success: false, string.Format(errorMessage, handle.OperationException?.Message));
			return false;
		}
		return true;
	}

	private void LoadLocalesCompleted(AsyncOperationHandle<Locale> operationHandle)
	{
		if (CheckOperationSucceeded(operationHandle, "Failed to initialize localization, could not load the selected locale.\n{0}"))
		{
			PreloadTables();
		}
	}

	private void PreloadTables()
	{
		m_RemainingSteps--;
		IPreloadRequired assetDatabase = m_Settings.GetAssetDatabase();
		if (assetDatabase != null && !assetDatabase.PreloadOperation.IsDone)
		{
			m_LoadDatabasesOperations.Add(assetDatabase.PreloadOperation);
		}
		else
		{
			m_RemainingSteps--;
		}
		IPreloadRequired stringDatabase = m_Settings.GetStringDatabase();
		if (stringDatabase != null && !stringDatabase.PreloadOperation.IsDone)
		{
			m_LoadDatabasesOperations.Add(stringDatabase.PreloadOperation);
		}
		else
		{
			m_RemainingSteps--;
		}
		if (m_LoadDatabasesOperations.Count > 0)
		{
			m_PreloadDatabasesOperation = AddressablesInterface.CreateGroupOperation(m_LoadDatabasesOperations);
			base.CurrentOperation = m_PreloadDatabasesOperation;
			m_PreloadDatabasesOperation.CompletedTypeless += m_FinishPreloadingTablesAction;
		}
		else
		{
			PreloadTablesCompleted();
		}
	}

	private void PreloadTablesCompleted()
	{
		IPreloadRequired assetDatabase = m_Settings.GetAssetDatabase();
		if (assetDatabase == null || CheckOperationSucceeded(assetDatabase.PreloadOperation, "Failed to initialize localization, could not preload asset tables.\n{0}"))
		{
			IPreloadRequired stringDatabase = m_Settings.GetStringDatabase();
			if (stringDatabase == null || CheckOperationSucceeded(stringDatabase.PreloadOperation, "Failed to initialize localization, could not preload string tables.\n{0}"))
			{
				FinishInitializing(success: true, null);
			}
		}
	}

	private void PostInitializeExtensions()
	{
		foreach (IStartupLocaleSelector startupLocaleSelector in m_Settings.GetStartupLocaleSelectors())
		{
			(startupLocaleSelector as IInitialize)?.PostInitialization(m_Settings);
		}
		(m_Settings.GetAvailableLocales() as IInitialize)?.PostInitialization(m_Settings);
		(m_Settings.GetAssetDatabase() as IInitialize)?.PostInitialization(m_Settings);
		(m_Settings.GetStringDatabase() as IInitialize)?.PostInitialization(m_Settings);
	}

	private void FinishInitializing(AsyncOperationHandle op)
	{
		FinishInitializing(op.Status == AsyncOperationStatus.Succeeded, op.OperationException?.Message);
	}

	private void FinishInitializing(bool success, string error)
	{
		AddressablesInterface.ReleaseAndReset(ref m_PreloadDatabasesOperation);
		PostInitializeExtensions();
		Complete(m_Settings, success, error);
	}

	protected override void Destroy()
	{
		base.Destroy();
		Pool.Release(this);
	}
}
