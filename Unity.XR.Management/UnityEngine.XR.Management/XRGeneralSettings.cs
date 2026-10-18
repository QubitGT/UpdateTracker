namespace UnityEngine.XR.Management;

public class XRGeneralSettings : ScriptableObject
{
	public static string k_SettingsKey = "com.unity.xr.management.loader_settings";

	internal static XRGeneralSettings s_RuntimeSettingsInstance = null;

	[SerializeField]
	internal XRManagerSettings m_LoaderManagerInstance;

	[SerializeField]
	[Tooltip("Toggling this on/off will enable/disable the automatic startup of XR at run time.")]
	internal bool m_InitManagerOnStart = true;

	private XRManagerSettings m_XRManager;

	private bool m_ProviderIntialized;

	private bool m_ProviderStarted;

	public XRManagerSettings Manager
	{
		get
		{
			return m_LoaderManagerInstance;
		}
		set
		{
			m_LoaderManagerInstance = value;
		}
	}

	public static XRGeneralSettings Instance => s_RuntimeSettingsInstance;

	public XRManagerSettings AssignedSettings => m_LoaderManagerInstance;

	public bool InitManagerOnStart => m_InitManagerOnStart;

	private void Awake()
	{
		Debug.Log("XRGeneral Settings awakening...");
		s_RuntimeSettingsInstance = this;
		Application.quitting += Quit;
		Object.DontDestroyOnLoad(s_RuntimeSettingsInstance);
	}

	private static void Quit()
	{
		XRGeneralSettings instance = Instance;
		if (!(instance == null))
		{
			instance.DeInitXRSDK();
		}
	}

	private void Start()
	{
		StartXRSDK();
	}

	private void OnDestroy()
	{
		DeInitXRSDK();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	internal static void AttemptInitializeXRSDKOnLoad()
	{
		XRGeneralSettings instance = Instance;
		if (!(instance == null) && instance.InitManagerOnStart)
		{
			instance.InitXRSDK();
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	internal static void AttemptStartXRSDKOnBeforeSplashScreen()
	{
		XRGeneralSettings instance = Instance;
		if (!(instance == null) && instance.InitManagerOnStart)
		{
			instance.StartXRSDK();
		}
	}

	private void InitXRSDK()
	{
		if (!(Instance == null) && !(Instance.m_LoaderManagerInstance == null) && Instance.m_InitManagerOnStart)
		{
			m_XRManager = Instance.m_LoaderManagerInstance;
			if (m_XRManager == null)
			{
				Debug.LogError("Assigned GameObject for XR Management loading is invalid. No XR Providers will be automatically loaded.");
				return;
			}
			m_XRManager.automaticLoading = false;
			m_XRManager.automaticRunning = false;
			m_XRManager.InitializeLoaderSync();
			m_ProviderIntialized = true;
		}
	}

	private void StartXRSDK()
	{
		if (m_XRManager != null && m_XRManager.activeLoader != null)
		{
			m_XRManager.StartSubsystems();
			m_ProviderStarted = true;
		}
	}

	private void StopXRSDK()
	{
		if (m_XRManager != null && m_XRManager.activeLoader != null)
		{
			m_XRManager.StopSubsystems();
			m_ProviderStarted = false;
		}
	}

	private void DeInitXRSDK()
	{
		if (m_XRManager != null && m_XRManager.activeLoader != null)
		{
			m_XRManager.DeinitializeLoader();
			m_XRManager = null;
			m_ProviderIntialized = false;
		}
	}
}
