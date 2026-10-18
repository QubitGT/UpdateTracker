using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;

namespace Unity.XR.OpenVR;

public class OpenVRLoader : XRLoaderHelper
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct UserDefinedSettings
	{
		public ushort stereoRenderingMode;

		public ushort initializationType;

		public ushort mirrorViewMode;

		[MarshalAs(UnmanagedType.LPStr)]
		public string editorAppKey;

		[MarshalAs(UnmanagedType.LPStr)]
		public string actionManifestPath;

		[MarshalAs(UnmanagedType.LPStr)]
		public string applicationName;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void TickCallbackDelegate(int value);

	private static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();

	private static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();

	private bool running;

	private FileInfo watcherFile;

	private FileSystemWatcher watcher;

	private const string mirrorViewPath = "openvr_mirrorview.cfg";

	private OpenVRSettings settings;

	private UnityEvent[] events;

	public XRDisplaySubsystem displaySubsystem => ((XRLoaderHelper)this).GetLoadedSubsystem<XRDisplaySubsystem>();

	public XRInputSubsystem inputSubsystem => ((XRLoaderHelper)this).GetLoadedSubsystem<XRInputSubsystem>();

	public override bool Initialize()
	{
		CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "OpenVR Display");
		EVRInitError initializationResult = GetInitializationResult();
		if (initializationResult != 0)
		{
			DestroySubsystem<XRDisplaySubsystem>();
			Debug.LogError("<b>[OpenVR]</b> Could not initialize OpenVR. Error code: " + initializationResult);
			return false;
		}
		CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "OpenVR Input");
		OpenVREvents.Initialize();
		TickCallbackDelegate tickCallbackDelegate = TickCallback;
		RegisterTickCallback(tickCallbackDelegate);
		tickCallbackDelegate(0);
		if (displaySubsystem != null)
		{
			return inputSubsystem != null;
		}
		return false;
	}

	private string GetEscapedApplicationName()
	{
		if (string.IsNullOrEmpty(Application.productName))
		{
			return "";
		}
		return Application.productName.Replace("\\", "\\\\").Replace("\"", "\\\"");
	}

	private void WatchForReload()
	{
	}

	private void CleanupReloadWatcher()
	{
	}

	public override bool Start()
	{
		running = true;
		WatchForReload();
		StartSubsystem<XRDisplaySubsystem>();
		StartSubsystem<XRInputSubsystem>();
		SetupFileSystemWatchers();
		return true;
	}

	private void SetupFileSystemWatchers()
	{
		SetupFileSystemWatcher();
	}

	private void SetupFileSystemWatcher()
	{
		try
		{
			settings = OpenVRSettings.GetSettings();
			if (watcher == null && running)
			{
				watcherFile = new FileInfo("openvr_mirrorview.cfg");
				watcher = new FileSystemWatcher(watcherFile.DirectoryName, watcherFile.Name);
				watcher.NotifyFilter = NotifyFilters.LastWrite;
				watcher.Created += OnChanged;
				watcher.Changed += OnChanged;
				watcher.EnableRaisingEvents = true;
				if (watcherFile.Exists)
				{
					OnChanged(null, null);
				}
			}
		}
		catch
		{
		}
	}

	private void DestroyMirrorModeWatcher()
	{
		if (watcher != null)
		{
			watcher.Created -= OnChanged;
			watcher.Changed -= OnChanged;
			watcher.EnableRaisingEvents = false;
			watcher.Dispose();
			watcher = null;
		}
	}

	private void OnChanged(object source, FileSystemEventArgs e)
	{
		ReadMirrorModeConfig();
	}

	private void ReadMirrorModeConfig()
	{
		try
		{
			string[] array = File.ReadAllLines("openvr_mirrorview.cfg");
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				if (array2.Length == 2 && array2[0] == "MirrorViewMode")
				{
					string text = array2[1];
					OpenVRSettings.MirrorViewModes mirrorViewModes = OpenVRSettings.MirrorViewModes.None;
					if (text.Equals("left", StringComparison.CurrentCultureIgnoreCase))
					{
						mirrorViewModes = OpenVRSettings.MirrorViewModes.Left;
					}
					else if (text.Equals("right", StringComparison.CurrentCultureIgnoreCase))
					{
						mirrorViewModes = OpenVRSettings.MirrorViewModes.Right;
					}
					else if (text.Equals("openvr", StringComparison.CurrentCultureIgnoreCase))
					{
						mirrorViewModes = OpenVRSettings.MirrorViewModes.OpenVR;
					}
					else if (text.Equals("none", StringComparison.CurrentCultureIgnoreCase))
					{
						mirrorViewModes = OpenVRSettings.MirrorViewModes.None;
					}
					else
					{
						Debug.LogError("<b>[OpenVR]</b> Invalid mode specified in openvr_mirrorview.cfg. Options are: Left, Right, None, and OpenVR.");
					}
					Debug.Log("<b>[OpenVR]</b> Mirror View Mode changed via file to: " + mirrorViewModes);
					OpenVRSettings.SetMirrorViewMode((ushort)mirrorViewModes);
				}
			}
		}
		catch
		{
		}
	}

	public override bool Stop()
	{
		running = false;
		CleanupTick();
		CleanupReloadWatcher();
		DestroyMirrorModeWatcher();
		StopSubsystem<XRInputSubsystem>();
		StopSubsystem<XRDisplaySubsystem>();
		return true;
	}

	public override bool Deinitialize()
	{
		CleanupTick();
		CleanupReloadWatcher();
		DestroyMirrorModeWatcher();
		DestroySubsystem<XRInputSubsystem>();
		DestroySubsystem<XRDisplaySubsystem>();
		return true;
	}

	private static void CleanupTick()
	{
		RegisterTickCallback(null);
	}

	[DllImport("XRSDKOpenVR", CharSet = CharSet.Auto)]
	private static extern void SetUserDefinedSettings(UserDefinedSettings settings);

	[DllImport("XRSDKOpenVR", CharSet = CharSet.Auto)]
	private static extern EVRInitError GetInitializationResult();

	[DllImport("XRSDKOpenVR", CharSet = CharSet.Auto)]
	private static extern void RegisterTickCallback([MarshalAs(UnmanagedType.FunctionPtr)] TickCallbackDelegate callbackPointer);

	[MonoPInvokeCallback(typeof(TickCallbackDelegate))]
	public static void TickCallback(int value)
	{
		OpenVREvents.Update();
	}
}
