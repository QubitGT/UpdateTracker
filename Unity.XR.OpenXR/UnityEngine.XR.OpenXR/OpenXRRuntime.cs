using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR;

public static class OpenXRRuntime
{
	private const string LibraryName = "UnityOpenXR";

	public static string name
	{
		get
		{
			if (!Internal_GetRuntimeName(out var runtimeNamePtr))
			{
				return "";
			}
			return Marshal.PtrToStringAnsi(runtimeNamePtr);
		}
	}

	public static string version
	{
		get
		{
			if (!Internal_GetRuntimeVersion(out var major, out var minor, out var patch))
			{
				return "";
			}
			return $"{major}.{minor}.{patch}";
		}
	}

	public static string apiVersion
	{
		get
		{
			if (!Internal_GetAPIVersion(out var major, out var minor, out var patch))
			{
				return "";
			}
			return $"{major}.{minor}.{patch}";
		}
	}

	public static string pluginVersion
	{
		get
		{
			if (!Internal_GetPluginVersion(out var pluginVersionPtr))
			{
				return "";
			}
			return Marshal.PtrToStringAnsi(pluginVersionPtr);
		}
	}

	public static bool retryInitializationOnFormFactorErrors
	{
		get
		{
			return Internal_GetSoftRestartLoopAtInitialization();
		}
		set
		{
			Internal_SetSoftRestartLoopAtInitialization(value);
		}
	}

	public static event Func<bool> wantsToQuit;

	public static event Func<bool> wantsToRestart;

	internal static bool isRuntimeAPIVersionGreaterThan1_1()
	{
		if (Internal_GetAPIVersion(out var major, out var minor, out var _) && major >= 1 && minor >= 1)
		{
			return true;
		}
		return false;
	}

	public static bool IsExtensionEnabled(string extensionName)
	{
		return Internal_IsExtensionEnabled(extensionName);
	}

	public static uint GetExtensionVersion(string extensionName)
	{
		return Internal_GetExtensionVersion(extensionName);
	}

	public static string[] GetEnabledExtensions()
	{
		string[] array = new string[Internal_GetEnabledExtensionCount()];
		for (int i = 0; i < array.Length; i++)
		{
			Internal_GetEnabledExtensionName((uint)i, out var extensionName);
			array[i] = extensionName ?? "";
		}
		return array;
	}

	public static string[] GetAvailableExtensions()
	{
		string[] array = new string[Internal_GetAvailableExtensionCount()];
		for (int i = 0; i < array.Length; i++)
		{
			Internal_GetAvailableExtensionName((uint)i, out var extensionName);
			array[i] = extensionName ?? "";
		}
		return array;
	}

	private static bool InvokeEvent(Func<bool> func)
	{
		if (func == null)
		{
			return true;
		}
		Delegate[] invocationList = func.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Func<bool> func2 = (Func<bool>)invocationList[i];
			try
			{
				if (!func2())
				{
					return false;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		return true;
	}

	internal static bool ShouldQuit()
	{
		return InvokeEvent(OpenXRRuntime.wantsToQuit);
	}

	internal static bool ShouldRestart()
	{
		return InvokeEvent(OpenXRRuntime.wantsToRestart);
	}

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetRuntimeName")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetRuntimeName(out IntPtr runtimeNamePtr);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetRuntimeVersion")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetRuntimeVersion(out ushort major, out ushort minor, out uint patch);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetAPIVersion")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetAPIVersion(out ushort major, out ushort minor, out uint patch);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetPluginVersion")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetPluginVersion(out IntPtr pluginVersionPtr);

	[DllImport("UnityOpenXR", EntryPoint = "unity_ext_IsExtensionEnabled")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_IsExtensionEnabled(string extensionName);

	[DllImport("UnityOpenXR", EntryPoint = "unity_ext_GetExtensionVersion")]
	private static extern uint Internal_GetExtensionVersion(string extensionName);

	[DllImport("UnityOpenXR", EntryPoint = "unity_ext_GetEnabledExtensionCount")]
	private static extern uint Internal_GetEnabledExtensionCount();

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "unity_ext_GetEnabledExtensionName")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetEnabledExtensionNamePtr(uint index, out IntPtr outName);

	[DllImport("UnityOpenXR", EntryPoint = "session_SetSoftRestartLoopAtInitialization")]
	private static extern void Internal_SetSoftRestartLoopAtInitialization([MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("UnityOpenXR", EntryPoint = "session_GetSoftRestartLoopAtInitialization")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetSoftRestartLoopAtInitialization();

	private static bool Internal_GetEnabledExtensionName(uint index, out string extensionName)
	{
		if (!Internal_GetEnabledExtensionNamePtr(index, out var outName))
		{
			extensionName = "";
			return false;
		}
		extensionName = Marshal.PtrToStringAnsi(outName);
		return true;
	}

	[DllImport("UnityOpenXR", EntryPoint = "unity_ext_GetAvailableExtensionCount")]
	private static extern uint Internal_GetAvailableExtensionCount();

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "unity_ext_GetAvailableExtensionName")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetAvailableExtensionNamePtr(uint index, out IntPtr extensionName);

	private static bool Internal_GetAvailableExtensionName(uint index, out string extensionName)
	{
		if (!Internal_GetAvailableExtensionNamePtr(index, out var extensionName2))
		{
			extensionName = "";
			return false;
		}
		extensionName = Marshal.PtrToStringAnsi(extensionName2);
		return true;
	}

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "session_GetLastError")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetLastError(out IntPtr error);

	internal static bool GetLastError(out string error)
	{
		if (!Internal_GetLastError(out var error2))
		{
			error = "";
			return false;
		}
		error = Marshal.PtrToStringAnsi(error2);
		return true;
	}

	internal static void LogLastError()
	{
		if (GetLastError(out var error))
		{
			Debug.LogError(error);
		}
	}
}
