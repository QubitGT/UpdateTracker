using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Device;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Operations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.Localization.Settings;

public class LocalizationSettings : ScriptableObject, IReset, IDisposable
{
	internal const string ConfigName = "com.unity.localization.settings";

	internal const string ConfigEditorLocale = "com.unity.localization-edit-locale";

	internal const string IgnoreSettings = "IgnoreSettings";

	internal const string LocaleLabel = "Locale";

	internal const string PreloadLabel = "Preload";

	[SerializeReference]
	private List<IStartupLocaleSelector> m_StartupSelectors = new List<IStartupLocaleSelector>
	{
		new CommandLineLocaleSelector(),
		new SystemLocaleSelector(),
		new SpecificLocaleSelector()
	};

	[SerializeReference]
	private ILocalesProvider m_AvailableLocales = new LocalesProvider();

	[SerializeReference]
	private LocalizedAssetDatabase m_AssetDatabase = new LocalizedAssetDatabase();

	[SerializeReference]
	private LocalizedStringDatabase m_StringDatabase = new LocalizedStringDatabase();

	[MetadataType(MetadataType.LocalizationSettings)]
	[SerializeField]
	private MetadataCollection m_Metadata = new MetadataCollection();

	[SerializeField]
	internal LocaleIdentifier m_ProjectLocaleIdentifier = "en";

	[SerializeField]
	private PreloadBehavior m_PreloadBehavior = PreloadBehavior.PreloadSelectedLocale;

	[SerializeField]
	private bool m_InitializeSynchronously;

	internal AsyncOperationHandle<LocalizationSettings> m_InitializingOperationHandle;

	private AsyncOperationHandle<Locale> m_SelectedLocaleAsync;

	private Locale m_ProjectLocale;

	private CallbackArray<Action<Locale>> m_SelectedLocaleChanged;

	internal static LocalizationSettings s_Instance;

	internal bool IsChangingSelectedLocale { get; private set; }

	internal bool HasSelectedLocaleChangedSubscribers => m_SelectedLocaleChanged.Length != 0;

	public static bool HasSettings
	{
		get
		{
			if ((object)s_Instance == null)
			{
				s_Instance = GetInstanceDontCreateDefault();
			}
			return (object)s_Instance != null;
		}
	}

	public static AsyncOperationHandle<LocalizationSettings> InitializationOperation => Instance.GetInitializationOperation();

	public static LocalizationSettings Instance
	{
		get
		{
			if ((object)s_Instance == null)
			{
				s_Instance = GetOrCreateSettings();
			}
			return s_Instance;
		}
		set
		{
			s_Instance = value;
		}
	}

	public static List<IStartupLocaleSelector> StartupLocaleSelectors => Instance.GetStartupLocaleSelectors();

	public static ILocalesProvider AvailableLocales
	{
		get
		{
			return Instance.GetAvailableLocales();
		}
		set
		{
			Instance.SetAvailableLocales(value);
		}
	}

	public static LocalizedAssetDatabase AssetDatabase
	{
		get
		{
			return Instance.GetAssetDatabase();
		}
		set
		{
			Instance.SetAssetDatabase(value);
		}
	}

	public static LocalizedStringDatabase StringDatabase
	{
		get
		{
			return Instance.GetStringDatabase();
		}
		set
		{
			Instance.SetStringDatabase(value);
		}
	}

	public static MetadataCollection Metadata => Instance.GetMetadata();

	public static Locale SelectedLocale
	{
		get
		{
			return Instance.GetSelectedLocale();
		}
		set
		{
			Instance.SetSelectedLocale(value);
		}
	}

	public static AsyncOperationHandle<Locale> SelectedLocaleAsync => Instance.GetSelectedLocaleAsync();

	public static Locale ProjectLocale
	{
		get
		{
			if (Instance.m_ProjectLocale == null || Instance.m_ProjectLocale.Identifier != Instance.m_ProjectLocaleIdentifier)
			{
				if (Instance.GetAvailableLocales() is IPreloadRequired { PreloadOperation: { IsDone: false }, PreloadOperation: var preloadOperation2 })
				{
					preloadOperation2.WaitForCompletion();
				}
				Instance.m_ProjectLocale = AvailableLocales?.GetLocale(Instance.m_ProjectLocaleIdentifier);
			}
			return Instance.m_ProjectLocale;
		}
		set
		{
			Instance.m_ProjectLocale = value;
			Instance.m_ProjectLocaleIdentifier = ((value != null) ? value.Identifier : default(LocaleIdentifier));
		}
	}

	public static bool InitializeSynchronously
	{
		get
		{
			return Instance.m_InitializeSynchronously;
		}
		set
		{
			Instance.m_InitializeSynchronously = value;
		}
	}

	public static PreloadBehavior PreloadBehavior
	{
		get
		{
			return Instance.m_PreloadBehavior;
		}
		set
		{
			Instance.m_PreloadBehavior = value;
		}
	}

	internal bool IsChangingPlayMode
	{
		get
		{
			if (IsPlayingOrWillChangePlaymode)
			{
				return !IsPlaying;
			}
			return false;
		}
	}

	internal bool IsPlayingOrWillChangePlaymode => true;

	internal bool IsPlaying => Application.isPlaying;

	internal virtual RuntimePlatform Platform => UnityEngine.Device.Application.platform;

	public event Action<Locale> OnSelectedLocaleChanged
	{
		add
		{
			m_SelectedLocaleChanged.Add(value);
		}
		remove
		{
			m_SelectedLocaleChanged.RemoveByMovingTail(value);
		}
	}

	public static event Action<Locale> SelectedLocaleChanged
	{
		add
		{
			Instance.OnSelectedLocaleChanged += value;
		}
		remove
		{
			Instance.OnSelectedLocaleChanged -= value;
		}
	}

	internal virtual void OnEnable()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}
	}

	internal static void ValidateSettingsExist(string error = "")
	{
		if (!HasSettings)
		{
			throw new Exception("There is no active LocalizationSettings.\n " + error);
		}
	}

	public virtual AsyncOperationHandle<LocalizationSettings> GetInitializationOperation()
	{
		if (!m_InitializingOperationHandle.IsValid())
		{
			InitializationOperation initializationOperation = UnityEngine.Localization.Operations.InitializationOperation.Pool.Get();
			initializationOperation.Init(this);
			initializationOperation.Dependency = AddressablesInterface.Instance.InitializeAddressablesAsync();
			m_InitializingOperationHandle = AddressablesInterface.ResourceManager.StartOperation(initializationOperation, initializationOperation.Dependency);
			if (!m_InitializingOperationHandle.IsDone && m_InitializeSynchronously && IsPlaying)
			{
				m_InitializingOperationHandle.WaitForCompletion();
			}
		}
		return m_InitializingOperationHandle;
	}

	public List<IStartupLocaleSelector> GetStartupLocaleSelectors()
	{
		return m_StartupSelectors;
	}

	public void SetAvailableLocales(ILocalesProvider available)
	{
		m_AvailableLocales = available;
	}

	public virtual ILocalesProvider GetAvailableLocales()
	{
		return m_AvailableLocales;
	}

	public void SetAssetDatabase(LocalizedAssetDatabase database)
	{
		m_AssetDatabase = database;
	}

	public virtual LocalizedAssetDatabase GetAssetDatabase()
	{
		return m_AssetDatabase;
	}

	public void SetStringDatabase(LocalizedStringDatabase database)
	{
		m_StringDatabase = database;
	}

	public virtual LocalizedStringDatabase GetStringDatabase()
	{
		return m_StringDatabase;
	}

	public MetadataCollection GetMetadata()
	{
		return m_Metadata;
	}

	public void ForceRefresh()
	{
		if (m_SelectedLocaleAsync.IsValid() && m_SelectedLocaleAsync.IsDone)
		{
			InvokeSelectedLocaleChanged(m_SelectedLocaleAsync.Result);
		}
		else
		{
			InvokeSelectedLocaleChanged(null);
		}
	}

	internal void SendLocaleChangedEvents(Locale locale)
	{
		m_StringDatabase?.OnLocaleChanged(locale);
		m_AssetDatabase?.OnLocaleChanged(locale);
		if (m_InitializingOperationHandle.IsValid())
		{
			AddressablesInterface.SafeRelease(m_InitializingOperationHandle);
			m_InitializingOperationHandle = default(AsyncOperationHandle<LocalizationSettings>);
		}
		if (GetInitializationOperation().Status == AsyncOperationStatus.Succeeded)
		{
			InvokeSelectedLocaleChanged(locale);
		}
		else
		{
			ComponentSingleton<LocalizationBehaviour>.Instance.StartCoroutine(InitializeAndCallSelectedLocaleChangedCoroutine(locale));
		}
	}

	private IEnumerator InitializeAndCallSelectedLocaleChangedCoroutine(Locale locale)
	{
		yield return m_InitializingOperationHandle;
		InvokeSelectedLocaleChanged(locale);
	}

	private void InvokeSelectedLocaleChanged(Locale locale)
	{
		IsChangingSelectedLocale = true;
		try
		{
			m_SelectedLocaleChanged.LockForChanges();
			int length = m_SelectedLocaleChanged.Length;
			if (length == 1)
			{
				m_SelectedLocaleChanged.SingleDelegate(locale);
			}
			else if (length > 1)
			{
				Action<Locale>[] multiDelegates = m_SelectedLocaleChanged.MultiDelegates;
				for (int i = 0; i < length; i++)
				{
					multiDelegates[i](locale);
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		IsChangingSelectedLocale = false;
		m_SelectedLocaleChanged.UnlockForChanges();
	}

	private Locale SelectActiveLocale()
	{
		if (m_AvailableLocales == null)
		{
			Debug.LogError("AvailableLocales is null, can not pick a Locale.");
			return null;
		}
		if (m_AvailableLocales.Locales == null)
		{
			Debug.LogError("AvailableLocales.Locales is null, can not pick a Locale.");
			return null;
		}
		return SelectLocaleUsingStartupSelectors();
	}

	protected internal virtual Locale SelectLocaleUsingStartupSelectors()
	{
		foreach (IStartupLocaleSelector startupSelector in m_StartupSelectors)
		{
			Locale startupLocale = startupSelector.GetStartupLocale(GetAvailableLocales());
			if (startupLocale != null)
			{
				return startupLocale;
			}
		}
		StringBuilder value;
		using (StringBuilderPool.Get(out value))
		{
			value.AppendLine("No Locale could be selected:");
			if (m_AvailableLocales.Locales.Count == 0)
			{
				value.AppendLine("No Locales were available. Did you build the Addressables?");
			}
			else
			{
				value.AppendLine($"The following ({m_AvailableLocales.Locales.Count}) Locales were considered:");
				foreach (Locale locale in m_AvailableLocales.Locales)
				{
					value.AppendLine($"\t{locale}");
				}
			}
			value.AppendLine($"The following ({m_StartupSelectors.Count}) IStartupLocaleSelectors were used:");
			foreach (IStartupLocaleSelector startupSelector2 in m_StartupSelectors)
			{
				value.AppendLine($"\t{startupSelector2}");
			}
			Debug.LogError(value.ToString(), this);
		}
		return null;
	}

	public void SetSelectedLocale(Locale locale)
	{
		if (m_SelectedLocaleAsync.IsValid())
		{
			Locale result = m_SelectedLocaleAsync.Result;
			if ((object)result == locale || (result != null && locale != null && result.name == locale.name && result.Identifier.Code == locale.Identifier.Code))
			{
				return;
			}
		}
		GetInitializationOperation();
		if ((!(locale == null) || !IsPlayingOrWillChangePlaymode) && (!m_SelectedLocaleAsync.IsValid() || (object)m_SelectedLocaleAsync.Result != locale))
		{
			if (m_SelectedLocaleAsync.IsValid())
			{
				AddressablesInterface.Release(m_SelectedLocaleAsync);
			}
			m_SelectedLocaleAsync = AddressablesInterface.ResourceManager.CreateCompletedOperation(locale, null);
			SendLocaleChangedEvents(locale);
		}
	}

	public virtual AsyncOperationHandle<Locale> GetSelectedLocaleAsync()
	{
		if (!m_SelectedLocaleAsync.IsValid())
		{
			if (GetAvailableLocales() is IPreloadRequired { PreloadOperation: { IsDone: false } } preloadRequired)
			{
				m_SelectedLocaleAsync = AddressablesInterface.ResourceManager.CreateChainOperation(preloadRequired.PreloadOperation, (AsyncOperationHandle op) => AddressablesInterface.ResourceManager.CreateCompletedOperation(SelectActiveLocale(), null));
			}
			else
			{
				m_SelectedLocaleAsync = AddressablesInterface.ResourceManager.CreateCompletedOperation(SelectActiveLocale(), null);
			}
		}
		return m_SelectedLocaleAsync;
	}

	public virtual Locale GetSelectedLocale()
	{
		AsyncOperationHandle<Locale> selectedLocaleAsync = GetSelectedLocaleAsync();
		if (selectedLocaleAsync.IsDone)
		{
			return selectedLocaleAsync.Result;
		}
		return selectedLocaleAsync.WaitForCompletion();
	}

	public virtual void OnLocaleRemoved(Locale locale)
	{
		if (m_SelectedLocaleAsync.IsValid() && (object)m_SelectedLocaleAsync.Result == locale)
		{
			AddressablesInterface.Release(m_SelectedLocaleAsync);
			m_SelectedLocaleAsync = default(AsyncOperationHandle<Locale>);
		}
	}

	public void ResetState()
	{
		m_SelectedLocaleAsync = default(AsyncOperationHandle<Locale>);
		m_InitializingOperationHandle = default(AsyncOperationHandle<LocalizationSettings>);
		(m_AvailableLocales as IReset)?.ResetState();
		((IReset)m_AssetDatabase)?.ResetState();
		((IReset)m_StringDatabase)?.ResetState();
	}

	void IDisposable.Dispose()
	{
		if (m_InitializingOperationHandle.IsValid())
		{
			if (!m_InitializingOperationHandle.IsDone)
			{
				m_InitializingOperationHandle.WaitForCompletion();
			}
			AddressablesInterface.Release(m_InitializingOperationHandle);
		}
		if (m_SelectedLocaleAsync.IsValid())
		{
			AddressablesInterface.Release(m_SelectedLocaleAsync);
		}
		m_InitializingOperationHandle = default(AsyncOperationHandle<LocalizationSettings>);
		m_SelectedLocaleAsync = default(AsyncOperationHandle<Locale>);
		(m_AvailableLocales as IDisposable)?.Dispose();
		((IDisposable)m_AssetDatabase)?.Dispose();
		((IDisposable)m_StringDatabase)?.Dispose();
		GC.SuppressFinalize(this);
	}

	public static LocalizationSettings GetInstanceDontCreateDefault()
	{
		if ((object)s_Instance != null)
		{
			return s_Instance;
		}
		return Object.FindFirstObjectByType<LocalizationSettings>();
	}

	private static LocalizationSettings GetOrCreateSettings()
	{
		LocalizationSettings localizationSettings = GetInstanceDontCreateDefault();
		if ((object)localizationSettings == null)
		{
			Debug.LogWarning("Could not find localization settings. Default will be used.");
			localizationSettings = ScriptableObject.CreateInstance<LocalizationSettings>();
			localizationSettings.name = "Default Localization Settings";
		}
		return localizationSettings;
	}
}
