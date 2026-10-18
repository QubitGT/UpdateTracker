using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features;

[Serializable]
public abstract class OpenXRFeature : ScriptableObject
{
	internal enum LoaderEvent
	{
		SubsystemCreate,
		SubsystemDestroy,
		SubsystemStart,
		SubsystemStop
	}

	internal enum NativeEvent
	{
		XrSetupConfigValues,
		XrSystemIdChanged,
		XrInstanceChanged,
		XrSessionChanged,
		XrBeginSession,
		XrSessionStateChanged,
		XrChangedSpaceApp,
		XrEndSession,
		XrDestroySession,
		XrDestroyInstance,
		XrIdle,
		XrReady,
		XrSynchronized,
		XrVisible,
		XrFocused,
		XrStopping,
		XrExiting,
		XrLossPending,
		XrInstanceLossPending,
		XrRestartRequested,
		XrRequestRestartLoop,
		XrRequestGetSystemLoop
	}

	[Flags]
	protected internal enum StatFlags
	{
		StatOptionNone = 0,
		ClearOnUpdate = 1,
		All = 1
	}

	[FormerlySerializedAs("enabled")]
	[HideInInspector]
	[SerializeField]
	private bool m_enabled;

	[HideInInspector]
	[SerializeField]
	internal string nameUi;

	[HideInInspector]
	[SerializeField]
	internal string version;

	[HideInInspector]
	[SerializeField]
	internal string featureIdInternal;

	[HideInInspector]
	[SerializeField]
	internal string openxrExtensionStrings;

	[HideInInspector]
	[SerializeField]
	internal string company;

	[HideInInspector]
	[SerializeField]
	internal int priority;

	[HideInInspector]
	[SerializeField]
	internal bool required;

	[NonSerialized]
	internal bool internalFieldsUpdated;

	private const string Library = "UnityOpenXR";

	internal bool failedInitialization { get; private set; }

	internal static bool requiredFeatureFailed { get; private set; }

	public bool enabled
	{
		get
		{
			if (m_enabled)
			{
				if (!(OpenXRLoaderBase.Instance == null))
				{
					return !failedInitialization;
				}
				return true;
			}
			return false;
		}
		set
		{
			if (enabled != value)
			{
				if (OpenXRLoaderBase.Instance != null)
				{
					Debug.LogError("OpenXRFeature.enabled cannot be changed while OpenXR is running");
					return;
				}
				m_enabled = value;
				OnEnabledChange();
			}
		}
	}

	protected static IntPtr xrGetInstanceProcAddr => Internal_GetProcAddressPtr(loaderDefault: false);

	protected internal virtual IntPtr HookGetInstanceProcAddr(IntPtr func)
	{
		return func;
	}

	protected internal virtual void OnSubsystemCreate()
	{
	}

	protected internal virtual void OnSubsystemStart()
	{
	}

	protected internal virtual void OnSubsystemStop()
	{
	}

	protected internal virtual void OnSubsystemDestroy()
	{
	}

	protected internal virtual bool OnInstanceCreate(ulong xrInstance)
	{
		return true;
	}

	protected internal virtual void OnSystemChange(ulong xrSystem)
	{
	}

	protected internal virtual void OnSessionCreate(ulong xrSession)
	{
	}

	protected internal virtual void OnAppSpaceChange(ulong xrSpace)
	{
	}

	protected internal virtual void OnSessionStateChange(int oldState, int newState)
	{
	}

	protected internal virtual void OnSessionBegin(ulong xrSession)
	{
	}

	protected internal virtual void OnSessionEnd(ulong xrSession)
	{
	}

	protected internal virtual void OnSessionExiting(ulong xrSession)
	{
	}

	protected internal virtual void OnSessionDestroy(ulong xrSession)
	{
	}

	protected internal virtual void OnInstanceDestroy(ulong xrInstance)
	{
	}

	protected internal virtual void OnSessionLossPending(ulong xrSession)
	{
	}

	protected internal virtual void OnInstanceLossPending(ulong xrInstance)
	{
	}

	protected internal virtual void OnFormFactorChange(int xrFormFactor)
	{
	}

	protected internal virtual void OnViewConfigurationTypeChange(int xrViewConfigurationType)
	{
	}

	protected internal virtual void OnEnvironmentBlendModeChange(XrEnvironmentBlendMode xrEnvironmentBlendMode)
	{
	}

	protected internal virtual void OnEnabledChange()
	{
	}

	protected static string PathToString(ulong path)
	{
		if (!Internal_PathToStringPtr(path, out var path2))
		{
			return null;
		}
		return Marshal.PtrToStringAnsi(path2);
	}

	protected static ulong StringToPath(string str)
	{
		if (!Internal_StringToPath(str, out var pathId))
		{
			return 0uL;
		}
		return pathId;
	}

	protected static ulong GetCurrentInteractionProfile(ulong userPath)
	{
		if (!Internal_GetCurrentInteractionProfile(userPath, out var interactionProfile))
		{
			return 0uL;
		}
		return interactionProfile;
	}

	protected static ulong GetCurrentInteractionProfile(string userPath)
	{
		return GetCurrentInteractionProfile(StringToPath(userPath));
	}

	protected static ulong GetCurrentAppSpace()
	{
		if (!Internal_GetAppSpace(out var appSpace))
		{
			return 0uL;
		}
		return appSpace;
	}

	protected static int GetViewConfigurationTypeForRenderPass(int renderPassIndex)
	{
		return Internal_GetViewTypeFromRenderIndex(renderPassIndex);
	}

	protected static void SetEnvironmentBlendMode(XrEnvironmentBlendMode xrEnvironmentBlendMode)
	{
		Internal_SetEnvironmentBlendMode(xrEnvironmentBlendMode);
	}

	protected static XrEnvironmentBlendMode GetEnvironmentBlendMode()
	{
		return Internal_GetEnvironmentBlendMode();
	}

	protected void CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) where TDescriptor : ISubsystemDescriptor where TSubsystem : ISubsystem
	{
		if (OpenXRLoaderBase.Instance == null)
		{
			Debug.LogError("CreateSubsystem called before loader was initialized");
		}
		else
		{
			OpenXRLoaderBase.Instance.CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);
		}
	}

	protected void StartSubsystem<T>() where T : class, ISubsystem
	{
		if (OpenXRLoaderBase.Instance == null)
		{
			Debug.LogError("StartSubsystem called before loader was initialized");
		}
		else
		{
			OpenXRLoaderBase.Instance.StartSubsystem<T>();
		}
	}

	protected void StopSubsystem<T>() where T : class, ISubsystem
	{
		if (OpenXRLoaderBase.Instance == null)
		{
			Debug.LogError("StopSubsystem called before loader was initialized");
		}
		else
		{
			OpenXRLoaderBase.Instance.StopSubsystem<T>();
		}
	}

	protected void DestroySubsystem<T>() where T : class, ISubsystem
	{
		if (OpenXRLoaderBase.Instance == null)
		{
			Debug.LogError("DestroySubsystem called before loader was initialized");
		}
		else
		{
			OpenXRLoaderBase.Instance.DestroySubsystem<T>();
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void Awake()
	{
	}

	internal static bool ReceiveLoaderEvent(OpenXRLoaderBase loader, LoaderEvent e)
	{
		OpenXRSettings instance = OpenXRSettings.Instance;
		if (instance == null)
		{
			return true;
		}
		OpenXRFeature[] features = instance.features;
		foreach (OpenXRFeature openXRFeature in features)
		{
			if (!(openXRFeature == null) && openXRFeature.enabled)
			{
				switch (e)
				{
				case LoaderEvent.SubsystemCreate:
					openXRFeature.OnSubsystemCreate();
					break;
				case LoaderEvent.SubsystemDestroy:
					openXRFeature.OnSubsystemDestroy();
					break;
				case LoaderEvent.SubsystemStart:
					openXRFeature.OnSubsystemStart();
					break;
				case LoaderEvent.SubsystemStop:
					openXRFeature.OnSubsystemStop();
					break;
				default:
					throw new ArgumentOutOfRangeException("e", e, null);
				}
			}
		}
		return true;
	}

	internal static void ReceiveNativeEvent(NativeEvent e, ulong payload)
	{
		if (null == OpenXRSettings.Instance)
		{
			return;
		}
		OpenXRFeature[] features = OpenXRSettings.Instance.features;
		foreach (OpenXRFeature openXRFeature in features)
		{
			if (!(openXRFeature == null) && openXRFeature.enabled)
			{
				switch (e)
				{
				case NativeEvent.XrSetupConfigValues:
					openXRFeature.OnFormFactorChange(Internal_GetFormFactor());
					openXRFeature.OnEnvironmentBlendModeChange(Internal_GetEnvironmentBlendMode());
					openXRFeature.OnViewConfigurationTypeChange(Internal_GetViewConfigurationType());
					break;
				case NativeEvent.XrSystemIdChanged:
					openXRFeature.OnSystemChange(payload);
					break;
				case NativeEvent.XrInstanceChanged:
					openXRFeature.failedInitialization = !openXRFeature.OnInstanceCreate(payload);
					requiredFeatureFailed |= openXRFeature.required && openXRFeature.failedInitialization;
					break;
				case NativeEvent.XrSessionChanged:
					openXRFeature.OnSessionCreate(payload);
					break;
				case NativeEvent.XrBeginSession:
					openXRFeature.OnSessionBegin(payload);
					break;
				case NativeEvent.XrChangedSpaceApp:
					openXRFeature.OnAppSpaceChange(payload);
					break;
				case NativeEvent.XrSessionStateChanged:
				{
					Internal_GetSessionState(out var oldState, out var newState);
					openXRFeature.OnSessionStateChange(oldState, newState);
					break;
				}
				case NativeEvent.XrEndSession:
					openXRFeature.OnSessionEnd(payload);
					break;
				case NativeEvent.XrExiting:
					openXRFeature.OnSessionExiting(payload);
					break;
				case NativeEvent.XrDestroySession:
					openXRFeature.OnSessionDestroy(payload);
					break;
				case NativeEvent.XrDestroyInstance:
					openXRFeature.OnInstanceDestroy(payload);
					break;
				case NativeEvent.XrLossPending:
					openXRFeature.OnSessionLossPending(payload);
					break;
				case NativeEvent.XrInstanceLossPending:
					openXRFeature.OnInstanceLossPending(payload);
					break;
				}
			}
		}
	}

	internal static void Initialize()
	{
		requiredFeatureFailed = false;
		OpenXRSettings instance = OpenXRSettings.Instance;
		if (instance == null || instance.features == null)
		{
			return;
		}
		OpenXRFeature[] features = instance.features;
		foreach (OpenXRFeature openXRFeature in features)
		{
			if (openXRFeature != null)
			{
				openXRFeature.failedInitialization = false;
			}
		}
	}

	internal static void HookGetInstanceProcAddr()
	{
		IntPtr func = Internal_GetProcAddressPtr(loaderDefault: true);
		OpenXRSettings instance = OpenXRSettings.Instance;
		if (instance != null && instance.features != null)
		{
			for (int num = instance.features.Length - 1; num >= 0; num--)
			{
				OpenXRFeature openXRFeature = instance.features[num];
				if (!(openXRFeature == null) && openXRFeature.enabled)
				{
					func = openXRFeature.HookGetInstanceProcAddr(func);
				}
			}
		}
		Internal_SetProcAddressPtrAndLoadStage1(func);
	}

	protected ulong GetAction(InputAction inputAction)
	{
		return OpenXRInput.GetActionHandle(inputAction);
	}

	protected ulong GetAction(InputDevice device, InputFeatureUsage usage)
	{
		return OpenXRInput.GetActionHandle(device, usage);
	}

	protected ulong GetAction(InputDevice device, string usageName)
	{
		return OpenXRInput.GetActionHandle(device, usageName);
	}

	protected internal static ulong RegisterStatsDescriptor(string statName, StatFlags statFlags)
	{
		return runtime_RegisterStatsDescriptor(statName, statFlags);
	}

	protected internal static void SetStatAsFloat(ulong statId, float value)
	{
		runtime_SetStatAsFloat(statId, value);
	}

	protected internal static void SetStatAsUInt(ulong statId, uint value)
	{
		runtime_SetStatAsUInt(statId, value);
	}

	[DllImport("UnityOpenXR", EntryPoint = "Internal_PathToString")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_PathToStringPtr(ulong pathId, out IntPtr path);

	[DllImport("UnityOpenXR")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_StringToPath([MarshalAs(UnmanagedType.LPStr)] string str, out ulong pathId);

	[DllImport("UnityOpenXR")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetCurrentInteractionProfile(ulong pathId, out ulong interactionProfile);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetFormFactor")]
	private static extern int Internal_GetFormFactor();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetViewConfigurationType")]
	private static extern int Internal_GetViewConfigurationType();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetViewTypeFromRenderIndex")]
	private static extern int Internal_GetViewTypeFromRenderIndex(int renderPassIndex);

	[DllImport("UnityOpenXR", EntryPoint = "OpenXRInputProvider_GetXRSession")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_GetXRSession(out ulong xrSession);

	[DllImport("UnityOpenXR", EntryPoint = "session_GetSessionState")]
	private static extern void Internal_GetSessionState(out int oldState, out int newState);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetEnvironmentBlendMode")]
	private static extern XrEnvironmentBlendMode Internal_GetEnvironmentBlendMode();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetEnvironmentBlendMode")]
	private static extern void Internal_SetEnvironmentBlendMode(XrEnvironmentBlendMode xrEnvironmentBlendMode);

	[DllImport("UnityOpenXR", EntryPoint = "OpenXRInputProvider_GetAppSpace")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_GetAppSpace(out ulong appSpace);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetProcAddressPtr")]
	internal static extern IntPtr Internal_GetProcAddressPtr([MarshalAs(UnmanagedType.I1)] bool loaderDefault);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetProcAddressPtrAndLoadStage1")]
	internal static extern void Internal_SetProcAddressPtrAndLoadStage1(IntPtr func);

	[DllImport("UnityOpenXR")]
	internal static extern ulong runtime_RegisterStatsDescriptor(string statName, StatFlags statFlags);

	[DllImport("UnityOpenXR")]
	internal static extern void runtime_SetStatAsFloat(ulong statId, float value);

	[DllImport("UnityOpenXR")]
	internal static extern void runtime_SetStatAsUInt(ulong statId, uint value);
}
