using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

public class ValveOpenXRRefreshRateFeature : OpenXRFeature
{
	internal delegate int Type_xrGetInstanceProcAddr(ulong instance, [MarshalAs(UnmanagedType.LPStr)] string name, out IntPtr function);

	internal delegate int Type_xrGetDisplayRefreshRateFB(ulong session, out float displayRefreshRate);

	internal delegate int Type_xrRequestDisplayRefreshRateFB(ulong session, float displayRefreshRate);

	internal delegate int Type_xrEnumerateDisplayRefreshRatesFB(ulong session, uint displayRefreshRateCapacityInput, out uint displayRefreshRateCountOutput, [Out] float[] displayRefreshRates);

	public const string featureId = "com.valve.openxr.refreshrate";

	private bool _initialized;

	private ulong instanceHandle = ulong.MaxValue;

	private ulong sessionHandle = ulong.MaxValue;

	private Type_xrGetInstanceProcAddr xrGetInstanceProcAddrDelegate;

	private Type_xrGetDisplayRefreshRateFB xrGetDisplayRefreshRateFB;

	private Type_xrRequestDisplayRefreshRateFB xrRequestDisplayRefreshRateFB;

	private Type_xrEnumerateDisplayRefreshRatesFB xrEnumerateDisplayRefreshRatesFB;

	public bool initialized => _initialized;

	public event OnRefreshRateFeatureAvailableDelegate OnRefreshRateFeatureAvailable;

	protected override bool OnInstanceCreate(ulong xrInstance)
	{
		Debug.Log("Extension: XR_FB_display_refresh_rate - OnInstanceCreate");
		instanceHandle = xrInstance;
		return base.OnInstanceCreate(xrInstance);
	}

	protected override void OnSessionBegin(ulong xrSession)
	{
		Debug.Log("Extension: XR_FB_display_refresh_rate - OnSessionBegin: " + xrSession);
		if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
		{
			Debug.LogError("ERROR: extension not enabled: XR_FB_display_refresh_rate");
			base.OnSessionBegin(xrSession);
			return;
		}
		sessionHandle = xrSession;
		InitializeFunctions();
		if (this.OnRefreshRateFeatureAvailable != null)
		{
			this.OnRefreshRateFeatureAvailable();
		}
		_initialized = true;
		base.OnSessionBegin(xrSession);
	}

	protected override void OnSessionDestroy(ulong xrSession)
	{
		_initialized = false;
		this.OnRefreshRateFeatureAvailable = null;
		base.OnSessionDestroy(xrSession);
	}

	protected override void OnSessionEnd(ulong xrSession)
	{
		_initialized = false;
		this.OnRefreshRateFeatureAvailable = null;
		base.OnSessionDestroy(xrSession);
	}

	private void InitializeFunctions()
	{
		xrGetInstanceProcAddrDelegate = (Type_xrGetInstanceProcAddr)Marshal.GetDelegateForFunctionPointer(OpenXRFeature.xrGetInstanceProcAddr, typeof(Type_xrGetInstanceProcAddr));
		IntPtr function = IntPtr.Zero;
		int num = xrGetInstanceProcAddrDelegate(instanceHandle, "xrGetDisplayRefreshRateFB", out function);
		if (num != 0)
		{
			Debug.LogError("ERROR: Error getting function pointer for: xrGetDisplayRefreshRateFB. " + num);
		}
		xrGetDisplayRefreshRateFB = Marshal.GetDelegateForFunctionPointer<Type_xrGetDisplayRefreshRateFB>(function);
		IntPtr function2 = IntPtr.Zero;
		num = xrGetInstanceProcAddrDelegate(instanceHandle, "xrRequestDisplayRefreshRateFB", out function2);
		if (num != 0)
		{
			Debug.LogError("ERROR: Error getting function pointer for: xrRequestDisplayRefreshRateFB. " + num);
		}
		xrRequestDisplayRefreshRateFB = Marshal.GetDelegateForFunctionPointer<Type_xrRequestDisplayRefreshRateFB>(function2);
		IntPtr function3 = IntPtr.Zero;
		num = xrGetInstanceProcAddrDelegate(instanceHandle, "xrEnumerateDisplayRefreshRatesFB", out function3);
		if (num != 0)
		{
			Debug.LogError("ERROR: Error getting function pointer for: xrEnumerateDisplayRefreshRatesFB. " + num);
		}
		xrEnumerateDisplayRefreshRatesFB = Marshal.GetDelegateForFunctionPointer<Type_xrEnumerateDisplayRefreshRatesFB>(function3);
	}

	public float GetRefreshRate()
	{
		if (sessionHandle == ulong.MaxValue)
		{
			Debug.LogError("ERROR: RefreshRateFeature not set up.");
			if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
			{
				Debug.LogError("ERROR: extension not enabled: XR_FB_display_refresh_rate");
			}
			return -1f;
		}
		float displayRefreshRate;
		int num = xrGetDisplayRefreshRateFB(sessionHandle, out displayRefreshRate);
		if (num != 0)
		{
			Debug.Log("WARNING: Result from xrGetDisplayRefreshRateFB was non-zero: " + num + " :: " + displayRefreshRate);
		}
		return displayRefreshRate;
	}

	public int SetRefreshRate(float refreshrate)
	{
		if (sessionHandle == ulong.MaxValue)
		{
			Debug.LogError("ERROR: RefreshRateFeature not set up.");
			if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
			{
				Debug.LogError("ERROR: extension not enabled: XR_FB_display_refresh_rate");
			}
			return -1;
		}
		int num = xrRequestDisplayRefreshRateFB(sessionHandle, refreshrate);
		if (num != 0)
		{
			Debug.Log("WARNING: Result from xrRequestDisplayRefreshRateFB was non-zero: " + num + " :: " + refreshrate);
		}
		return num;
	}

	public int EnumerateRefreshRates(ref List<float> displayRefreshRates)
	{
		if (sessionHandle == ulong.MaxValue)
		{
			Debug.LogError("ERROR: RefreshRateFeature not set up.");
			if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
			{
				Debug.LogError("ERROR: extension not enabled: XR_FB_display_refresh_rate");
			}
			return -1;
		}
		displayRefreshRates.Clear();
		int num = xrEnumerateDisplayRefreshRatesFB(sessionHandle, 0u, out var displayRefreshRateCountOutput, null);
		if (num != 0)
		{
			Debug.Log("WARNING: Result from xrEnumerateDisplayRefreshRatesFB was non-zero: " + num);
			return num;
		}
		float[] array = new float[displayRefreshRateCountOutput];
		num = xrEnumerateDisplayRefreshRatesFB(sessionHandle, displayRefreshRateCountOutput, out displayRefreshRateCountOutput, array);
		if (num != 0)
		{
			Debug.Log("WARNING: Result from xrEnumerateDisplayRefreshRatesFB(2) was non-zero: " + num);
			return num;
		}
		displayRefreshRates.AddRange(array);
		return num;
	}
}
