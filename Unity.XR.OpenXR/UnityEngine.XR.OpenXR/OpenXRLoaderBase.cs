using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using AOT;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR;

public class OpenXRLoaderBase : XRLoaderHelper
{
	private class FeatureLoggingInfo
	{
		public string m_nameUi;

		public string m_version;

		public string m_company;

		public string m_openxrExtensionStrings;

		public FeatureLoggingInfo(string nameUi, string version, string company, string extensionStrings)
		{
			m_nameUi = nameUi;
			m_version = version;
			m_company = company;
			m_openxrExtensionStrings = extensionStrings;
		}
	}

	internal enum LoaderState
	{
		Uninitialized,
		InitializeAttempted,
		Initialized,
		StartAttempted,
		Started,
		StopAttempted,
		Stopped,
		DeinitializeAttempted
	}

	internal delegate void ReceiveNativeEventDelegate(OpenXRFeature.NativeEvent e, ulong payload);

	private List<FeatureLoggingInfo> featureLoggingInfo;

	private const double k_IdlePollingWaitTimeInSeconds = 0.1;

	private static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();

	private static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();

	private List<LoaderState> validLoaderInitStates = new List<LoaderState>
	{
		LoaderState.Uninitialized,
		LoaderState.InitializeAttempted
	};

	private List<LoaderState> validLoaderStartStates = new List<LoaderState>
	{
		LoaderState.Initialized,
		LoaderState.StartAttempted,
		LoaderState.Stopped
	};

	private List<LoaderState> validLoaderStopStates = new List<LoaderState>
	{
		LoaderState.StartAttempted,
		LoaderState.Started,
		LoaderState.StopAttempted
	};

	private List<LoaderState> validLoaderDeinitStates = new List<LoaderState>
	{
		LoaderState.InitializeAttempted,
		LoaderState.Initialized,
		LoaderState.Stopped,
		LoaderState.DeinitializeAttempted
	};

	private List<LoaderState> runningStates = new List<LoaderState>
	{
		LoaderState.Initialized,
		LoaderState.StartAttempted,
		LoaderState.Started
	};

	private OpenXRFeature.NativeEvent currentOpenXRState;

	private bool actionSetsAttached;

	private UnhandledExceptionEventHandler unhandledExceptionHandler;

	internal bool DisableValidationChecksOnEnteringPlaymode;

	private double lastPollCheckTime;

	private const string LibraryName = "UnityOpenXR";

	internal static OpenXRLoaderBase Instance { get; private set; }

	internal LoaderState currentLoaderState { get; private set; }

	internal XRDisplaySubsystem displaySubsystem => GetLoadedSubsystem<XRDisplaySubsystem>();

	internal XRInputSubsystem inputSubsystem => Instance?.GetLoadedSubsystem<XRInputSubsystem>();

	private bool isInitialized
	{
		get
		{
			if (currentLoaderState != 0)
			{
				return currentLoaderState != LoaderState.DeinitializeAttempted;
			}
			return false;
		}
	}

	private bool isStarted => runningStates.Contains(currentLoaderState);

	private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
	{
		ulong section = DiagnosticReport.GetSection("Unhandled Exception Report");
		DiagnosticReport.AddSectionEntry(section, "Is Terminating", $"{args.IsTerminating}");
		Exception ex = (Exception)args.ExceptionObject;
		DiagnosticReport.AddSectionEntry(section, "Message", ex.Message ?? "");
		DiagnosticReport.AddSectionEntry(section, "Source", ex.Source ?? "");
		DiagnosticReport.AddSectionEntry(section, "Stack Trace", "\n" + ex.StackTrace);
		DiagnosticReport.DumpReport("Uncaught Exception");
	}

	public override bool Initialize()
	{
		if (currentLoaderState == LoaderState.Initialized)
		{
			return true;
		}
		if (!validLoaderInitStates.Contains(currentLoaderState))
		{
			return false;
		}
		if (Instance != null)
		{
			Debug.LogError("Only one OpenXRLoader can be initialized at any given time");
			return false;
		}
		DiagnosticReport.StartReport();
		try
		{
			if (InitializeInternal())
			{
				return true;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		Deinitialize();
		Instance = null;
		OpenXRAnalytics.SendInitializeEvent(success: false);
		return false;
	}

	private bool InitializeInternal()
	{
		Instance = this;
		currentLoaderState = LoaderState.InitializeAttempted;
		Internal_SetSuccessfullyInitialized(value: false);
		OpenXRInput.RegisterLayouts();
		OpenXRFeature.Initialize();
		if (!LoadOpenXRSymbols())
		{
			Debug.LogError("Failed to load openxr runtime loader.");
			return false;
		}
		OpenXRSettings.Instance.features = (from f in OpenXRSettings.Instance.features
			where f != null
			orderby f.priority descending, f.nameUi
			select f).ToArray();
		OpenXRFeature.HookGetInstanceProcAddr();
		if (!Internal_InitializeSession())
		{
			return false;
		}
		RequestOpenXRFeatures();
		RegisterOpenXRCallbacks();
		if (null != OpenXRSettings.Instance)
		{
			OpenXRSettings.Instance.ApplySettings();
		}
		if (!CreateSubsystems())
		{
			return false;
		}
		if (OpenXRFeature.requiredFeatureFailed)
		{
			return false;
		}
		SetApplicationInfo();
		OpenXRAnalytics.SendInitializeEvent(success: true);
		OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemCreate);
		DebugLogEnabledSpecExtensions();
		Application.onBeforeRender += ProcessOpenXRMessageLoop;
		currentLoaderState = LoaderState.Initialized;
		return true;
	}

	private bool CreateSubsystems()
	{
		if (displaySubsystem == null)
		{
			CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "OpenXR Display");
			if (displaySubsystem == null)
			{
				return false;
			}
		}
		if (inputSubsystem == null)
		{
			CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "OpenXR Input");
			if (inputSubsystem == null)
			{
				return false;
			}
		}
		return true;
	}

	internal void ProcessOpenXRMessageLoop()
	{
		if (currentOpenXRState == OpenXRFeature.NativeEvent.XrIdle || currentOpenXRState == OpenXRFeature.NativeEvent.XrStopping || currentOpenXRState == OpenXRFeature.NativeEvent.XrExiting || currentOpenXRState == OpenXRFeature.NativeEvent.XrLossPending || currentOpenXRState == OpenXRFeature.NativeEvent.XrInstanceLossPending)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if ((double)realtimeSinceStartup - lastPollCheckTime < 0.1)
			{
				return;
			}
			lastPollCheckTime = realtimeSinceStartup;
		}
		Internal_PumpMessageLoop();
	}

	public override bool Start()
	{
		if (currentLoaderState == LoaderState.Started)
		{
			return true;
		}
		if (!validLoaderStartStates.Contains(currentLoaderState))
		{
			return false;
		}
		currentLoaderState = LoaderState.StartAttempted;
		if (!StartInternal())
		{
			Stop();
			return false;
		}
		currentLoaderState = LoaderState.Started;
		return true;
	}

	private bool StartInternal()
	{
		if (!Internal_CreateSessionIfNeeded())
		{
			return false;
		}
		if (currentOpenXRState != OpenXRFeature.NativeEvent.XrReady || (currentLoaderState != LoaderState.StartAttempted && currentLoaderState != LoaderState.Started))
		{
			return true;
		}
		StartSubsystem<XRDisplaySubsystem>();
		XRDisplaySubsystem xRDisplaySubsystem = displaySubsystem;
		if (xRDisplaySubsystem != null && !xRDisplaySubsystem.running)
		{
			return false;
		}
		Internal_BeginSession();
		if (!actionSetsAttached)
		{
			OpenXRInput.AttachActionSets();
			actionSetsAttached = true;
		}
		XRDisplaySubsystem xRDisplaySubsystem2 = displaySubsystem;
		if (xRDisplaySubsystem2 != null && !xRDisplaySubsystem2.running)
		{
			StartSubsystem<XRDisplaySubsystem>();
		}
		XRInputSubsystem xRInputSubsystem = inputSubsystem;
		if (xRInputSubsystem != null && !xRInputSubsystem.running)
		{
			StartSubsystem<XRInputSubsystem>();
		}
		bool num = inputSubsystem?.running ?? false;
		bool flag = displaySubsystem?.running ?? false;
		if (num && flag)
		{
			OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemStart);
			return true;
		}
		return false;
	}

	public override bool Stop()
	{
		if (currentLoaderState == LoaderState.Stopped)
		{
			return true;
		}
		if (!validLoaderStopStates.Contains(currentLoaderState))
		{
			return false;
		}
		currentLoaderState = LoaderState.StopAttempted;
		bool num = inputSubsystem?.running ?? false;
		bool flag = displaySubsystem?.running ?? false;
		if (num || flag)
		{
			OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemStop);
		}
		if (num)
		{
			StopSubsystem<XRInputSubsystem>();
		}
		if (flag)
		{
			StopSubsystem<XRDisplaySubsystem>();
		}
		StopInternal();
		currentLoaderState = LoaderState.Stopped;
		return true;
	}

	private void StopInternal()
	{
		Internal_EndSession();
		ProcessOpenXRMessageLoop();
	}

	public override bool Deinitialize()
	{
		if (currentLoaderState == LoaderState.Uninitialized)
		{
			return true;
		}
		if (!validLoaderDeinitStates.Contains(currentLoaderState))
		{
			return false;
		}
		currentLoaderState = LoaderState.DeinitializeAttempted;
		try
		{
			Internal_RequestExitSession();
			Application.onBeforeRender -= ProcessOpenXRMessageLoop;
			ProcessOpenXRMessageLoop();
			OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemDestroy);
			DestroySubsystem<XRInputSubsystem>();
			DestroySubsystem<XRDisplaySubsystem>();
			DiagnosticReport.DumpReport("System Shutdown");
			Internal_DestroySession();
			ProcessOpenXRMessageLoop();
			Internal_UnloadOpenXRLibrary();
			currentLoaderState = LoaderState.Uninitialized;
			actionSetsAttached = false;
			if (unhandledExceptionHandler != null)
			{
				AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
				unhandledExceptionHandler = null;
			}
			return base.Deinitialize();
		}
		finally
		{
			Instance = null;
		}
	}

	internal new void CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) where TDescriptor : ISubsystemDescriptor where TSubsystem : ISubsystem
	{
		base.CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);
	}

	internal new void StartSubsystem<T>() where T : class, ISubsystem
	{
		base.StartSubsystem<T>();
	}

	internal new void StopSubsystem<T>() where T : class, ISubsystem
	{
		base.StopSubsystem<T>();
	}

	internal new void DestroySubsystem<T>() where T : class, ISubsystem
	{
		base.DestroySubsystem<T>();
	}

	private void SetApplicationInfo()
	{
		byte[] array = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Application.version));
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(array);
		}
		uint applicationVersionHash = BitConverter.ToUInt32(array, 0);
		Internal_SetApplicationInfo(Application.productName, Application.version, applicationVersionHash, Application.unityVersion);
	}

	internal static byte[] StringToWCHAR_T(string s)
	{
		return ((Environment.OSVersion.Platform == PlatformID.Unix) ? Encoding.UTF32 : Encoding.Unicode).GetBytes(s + "\0");
	}

	private bool LoadOpenXRSymbols()
	{
		if (!Internal_LoadOpenXRLibrary(StringToWCHAR_T("openxr_loader")))
		{
			return false;
		}
		return true;
	}

	private void RequestOpenXRFeatures()
	{
		OpenXRSettings instance = OpenXRSettings.Instance;
		if (instance == null || instance.features == null)
		{
			return;
		}
		featureLoggingInfo = new List<FeatureLoggingInfo>(instance.featureCount);
		OpenXRFeature[] features = instance.features;
		foreach (OpenXRFeature openXRFeature in features)
		{
			if (openXRFeature == null || !openXRFeature.enabled)
			{
				continue;
			}
			featureLoggingInfo.Add(new FeatureLoggingInfo(openXRFeature.nameUi, openXRFeature.version, openXRFeature.company, openXRFeature.openxrExtensionStrings));
			if (string.IsNullOrEmpty(openXRFeature.openxrExtensionStrings))
			{
				continue;
			}
			string[] array = openXRFeature.openxrExtensionStrings.Split(' ');
			foreach (string text in array)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					Internal_RequestEnableExtensionString(text);
				}
			}
		}
	}

	private void LogRequestedOpenXRFeatures()
	{
		OpenXRSettings instance = OpenXRSettings.Instance;
		if (instance == null || instance.features == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("");
		StringBuilder stringBuilder2 = new StringBuilder("");
		uint num = 0u;
		uint num2 = 0u;
		foreach (FeatureLoggingInfo item in featureLoggingInfo)
		{
			stringBuilder.Append("  " + item.m_nameUi + ": Version=" + item.m_version + ", Company=\"" + item.m_company + "\"");
			if (!string.IsNullOrEmpty(item.m_openxrExtensionStrings))
			{
				stringBuilder.Append(", Extensions=\"" + item.m_openxrExtensionStrings + "\"");
				string[] array = item.m_openxrExtensionStrings.Split(' ');
				foreach (string text in array)
				{
					if (!string.IsNullOrWhiteSpace(text) && !Internal_IsExtensionEnabled(text))
					{
						num2++;
						stringBuilder2.Append("  " + text + ": Feature=\"" + item.m_nameUi + "\": Version=" + item.m_version + ", Company=\"" + item.m_company + "\"\n");
					}
				}
			}
			stringBuilder.Append("\n");
		}
		ulong section = DiagnosticReport.GetSection("OpenXR Runtime Info");
		DiagnosticReport.AddSectionBreak(section);
		DiagnosticReport.AddSectionEntry(section, "Features requested to be enabled", $"({num})\n{stringBuilder.ToString()}");
		DiagnosticReport.AddSectionBreak(section);
		DiagnosticReport.AddSectionEntry(section, "Requested feature extensions not supported by runtime", $"({num2})\n{stringBuilder2.ToString()}");
	}

	private static void DebugLogEnabledSpecExtensions()
	{
		ulong section = DiagnosticReport.GetSection("OpenXR Runtime Info");
		DiagnosticReport.AddSectionBreak(section);
		string[] enabledExtensions = OpenXRRuntime.GetEnabledExtensions();
		StringBuilder stringBuilder = new StringBuilder($"({enabledExtensions.Length})\n");
		string[] array = enabledExtensions;
		foreach (string text in array)
		{
			stringBuilder.Append($"  {text}: Version={OpenXRRuntime.GetExtensionVersion(text)}\n");
		}
		DiagnosticReport.AddSectionEntry(section, "Runtime extensions enabled", stringBuilder.ToString());
	}

	[MonoPInvokeCallback(typeof(ReceiveNativeEventDelegate))]
	private static void ReceiveNativeEvent(OpenXRFeature.NativeEvent e, ulong payload)
	{
		OpenXRLoaderBase instance = Instance;
		if (instance != null)
		{
			instance.currentOpenXRState = e;
		}
		switch (e)
		{
		case OpenXRFeature.NativeEvent.XrRestartRequested:
			OpenXRRestarter.Instance.ShutdownAndRestart();
			break;
		case OpenXRFeature.NativeEvent.XrReady:
			instance.StartInternal();
			break;
		case OpenXRFeature.NativeEvent.XrBeginSession:
			instance.LogRequestedOpenXRFeatures();
			break;
		case OpenXRFeature.NativeEvent.XrFocused:
			DiagnosticReport.DumpReport("System Startup Completed");
			break;
		case OpenXRFeature.NativeEvent.XrRequestRestartLoop:
			Debug.Log("XR Initialization failed, will try to restart xr periodically.");
			OpenXRRestarter.Instance.PauseAndShutdownAndRestart();
			break;
		case OpenXRFeature.NativeEvent.XrRequestGetSystemLoop:
			OpenXRRestarter.Instance.PauseAndRetryInitialization();
			break;
		case OpenXRFeature.NativeEvent.XrStopping:
			instance.StopInternal();
			break;
		}
		OpenXRFeature.ReceiveNativeEvent(e, payload);
		if ((!(instance == null) && instance.isStarted) || e == OpenXRFeature.NativeEvent.XrInstanceChanged)
		{
			switch (e)
			{
			case OpenXRFeature.NativeEvent.XrExiting:
				OpenXRRestarter.Instance.Shutdown();
				break;
			case OpenXRFeature.NativeEvent.XrLossPending:
				OpenXRRestarter.Instance.ShutdownAndRestart();
				break;
			case OpenXRFeature.NativeEvent.XrInstanceLossPending:
				OpenXRRestarter.Instance.Shutdown();
				break;
			}
		}
	}

	internal static void RegisterOpenXRCallbacks()
	{
		Internal_SetCallbacks(ReceiveNativeEvent);
	}

	[DllImport("UnityOpenXR", EntryPoint = "main_LoadOpenXRLibrary")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_LoadOpenXRLibrary(byte[] loaderPath);

	[DllImport("UnityOpenXR", EntryPoint = "main_UnloadOpenXRLibrary")]
	internal static extern void Internal_UnloadOpenXRLibrary();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetCallbacks")]
	private static extern void Internal_SetCallbacks(ReceiveNativeEventDelegate callback);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "NativeConfig_SetApplicationInfo")]
	private static extern void Internal_SetApplicationInfo(string applicationName, string applicationVersion, uint applicationVersionHash, string engineVersion);

	[DllImport("UnityOpenXR", EntryPoint = "session_RequestExitSession")]
	internal static extern void Internal_RequestExitSession();

	[DllImport("UnityOpenXR", EntryPoint = "session_InitializeSession")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_InitializeSession();

	[DllImport("UnityOpenXR", EntryPoint = "session_CreateSessionIfNeeded")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_CreateSessionIfNeeded();

	[DllImport("UnityOpenXR", EntryPoint = "session_BeginSession")]
	internal static extern void Internal_BeginSession();

	[DllImport("UnityOpenXR", EntryPoint = "session_EndSession")]
	internal static extern void Internal_EndSession();

	[DllImport("UnityOpenXR", EntryPoint = "session_DestroySession")]
	internal static extern void Internal_DestroySession();

	[DllImport("UnityOpenXR", EntryPoint = "messagepump_PumpMessageLoop")]
	private static extern void Internal_PumpMessageLoop();

	[DllImport("UnityOpenXR", EntryPoint = "session_SetSuccessfullyInitialized")]
	internal static extern void Internal_SetSuccessfullyInitialized([MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "unity_ext_RequestEnableExtensionString")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_RequestEnableExtensionString(string extensionString);

	[DllImport("UnityOpenXR", EntryPoint = "unity_ext_IsExtensionEnabled")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_IsExtensionEnabled(string extensionName);
}
