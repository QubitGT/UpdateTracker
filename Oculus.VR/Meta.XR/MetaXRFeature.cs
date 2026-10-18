using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Meta.XR;

public class MetaXRFeature : OpenXRFeature
{
	public const string featureId = "com.meta.openxr.feature.metaxr";

	public bool userPresent
	{
		get
		{
			if (OVRPlugin.UnityOpenXR.Enabled)
			{
				return OVRPlugin.userPresent;
			}
			return false;
		}
	}

	protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
	{
		OVRPlugin.UnityOpenXR.Enabled = true;
		Debug.Log($"[MetaXRFeature] HookGetInstanceProcAddr: {func}");
		Debug.Log("[MetaXRFeature] SetClientVersion");
		OVRPlugin.UnityOpenXR.SetClientVersion();
		return OVRPlugin.UnityOpenXR.HookGetInstanceProcAddr(func);
	}

	protected override bool OnInstanceCreate(ulong xrInstance)
	{
		bool flag = false;
		string[] availableExtensions = OpenXRRuntime.GetAvailableExtensions();
		for (int i = 0; i < availableExtensions.Length; i++)
		{
			if (availableExtensions[i] == "XR_META_headset_id")
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Debug.Log("[MetaXRFeature] OpenXR runtime supports XR_META_headset_id extension. MetaXRFeature is enabled.");
		}
		else
		{
			string text = OpenXRRuntime.name.ToLower();
			if (!text.Contains("meta") && !text.Contains("oculus"))
			{
				Debug.LogWarningFormat("[MetaXRFeature] MetaXRFeature is disabled on non-Oculus/Meta OpenXR Runtime. Runtime name: {0}", OpenXRRuntime.name);
				return false;
			}
		}
		Debug.Log($"[MetaXRFeature] OnInstanceCreate: {xrInstance}");
		bool num = OVRPlugin.UnityOpenXR.OnInstanceCreate(xrInstance);
		if (!num)
		{
			Debug.LogWarning("[MetaXRFeature] OnInstanceCreate returned an error. If you are using Quest Link, please verify if it's started.");
		}
		return num;
	}

	protected override void OnInstanceDestroy(ulong xrInstance)
	{
		Debug.Log($"[MetaXRFeature] OnInstanceDestroy: {xrInstance}");
		OVRPlugin.UnityOpenXR.OnInstanceDestroy(xrInstance);
	}

	protected override void OnSessionCreate(ulong xrSession)
	{
		Debug.Log($"[MetaXRFeature] OnSessionCreate: {xrSession}");
		OVRPlugin.UnityOpenXR.OnSessionCreate(xrSession);
	}

	protected override void OnAppSpaceChange(ulong xrSpace)
	{
		Debug.Log($"[MetaXRFeature] OnAppSpaceChange: {xrSpace}");
		int num = 0;
		if (OpenXRSettings.AllowRecentering)
		{
			num |= 1;
		}
		OVRPlugin.UnityOpenXR.OnAppSpaceChange2(xrSpace, num);
	}

	protected override void OnSessionStateChange(int oldState, int newState)
	{
		Debug.Log($"[MetaXRFeature] OnSessionStateChange: {oldState} -> {newState}");
		OVRPlugin.UnityOpenXR.OnSessionStateChange(oldState, newState);
	}

	protected override void OnSessionBegin(ulong xrSession)
	{
		Debug.Log($"[MetaXRFeature] OnSessionBegin: {xrSession}");
		OVRPlugin.UnityOpenXR.OnSessionBegin(xrSession);
	}

	protected override void OnSessionEnd(ulong xrSession)
	{
		Debug.Log($"[MetaXRFeature] OnSessionEnd: {xrSession}");
		OVRPlugin.UnityOpenXR.OnSessionEnd(xrSession);
	}

	protected override void OnSessionExiting(ulong xrSession)
	{
		Debug.Log($"[MetaXRFeature] OnSessionExiting: {xrSession}");
		OVRPlugin.UnityOpenXR.OnSessionExiting(xrSession);
	}

	protected override void OnSessionDestroy(ulong xrSession)
	{
		Debug.Log($"[MetaXRFeature] OnSessionDestroy: {xrSession}");
		OVRPlugin.UnityOpenXR.OnSessionDestroy(xrSession);
	}
}
