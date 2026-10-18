using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.OpenXR.Features.Extensions.PerformanceSettings;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Mock;

public class MockRuntime : OpenXRFeature
{
	public enum ScriptEvent
	{
		Unknown,
		EndFrame,
		HapticImpulse,
		HapticStop
	}

	public delegate void ScriptEventDelegate(ScriptEvent evt, ulong param);

	public delegate XrResult BeforeFunctionDelegate(string functionName);

	public delegate void AfterFunctionDelegate(string functionName, XrResult result);

	private static Dictionary<string, AfterFunctionDelegate> s_AfterFunctionCallbacks;

	private static Dictionary<string, BeforeFunctionDelegate> s_BeforeFunctionCallbacks;

	public const string featureId = "com.unity.openxr.feature.mockruntime";

	public bool ignoreValidationErrors;

	private const string extLib = "mock_api";

	internal Func<IntPtr, IntPtr> MockFunctionInterceptor;

	public static MockRuntime Instance => OpenXRSettings.Instance.GetFeature<MockRuntime>();

	public static event ScriptEventDelegate onScriptEvent;

	[MonoPInvokeCallback(typeof(ScriptEventDelegate))]
	private static void ReceiveScriptEvent(ScriptEvent evt, ulong param)
	{
		MockRuntime.onScriptEvent?.Invoke(evt, param);
	}

	[MonoPInvokeCallback(typeof(BeforeFunctionDelegate))]
	private static XrResult BeforeFunctionCallback(string function)
	{
		return GetBeforeFunctionCallback(function)?.Invoke(function) ?? XrResult.Success;
	}

	[MonoPInvokeCallback(typeof(BeforeFunctionDelegate))]
	private static void AfterFunctionCallback(string function, XrResult result)
	{
		GetAfterFunctionCallback(function)?.Invoke(function, result);
	}

	public static void SetFunctionCallback(string function, BeforeFunctionDelegate beforeCallback, AfterFunctionDelegate afterCallback)
	{
		if (beforeCallback != null)
		{
			if (s_BeforeFunctionCallbacks == null)
			{
				s_BeforeFunctionCallbacks = new Dictionary<string, BeforeFunctionDelegate>();
			}
			s_BeforeFunctionCallbacks[function] = beforeCallback;
		}
		else if (s_BeforeFunctionCallbacks != null)
		{
			s_BeforeFunctionCallbacks.Remove(function);
			if (s_BeforeFunctionCallbacks.Count == 0)
			{
				s_BeforeFunctionCallbacks = null;
			}
		}
		if (afterCallback != null)
		{
			if (s_AfterFunctionCallbacks == null)
			{
				s_AfterFunctionCallbacks = new Dictionary<string, AfterFunctionDelegate>();
			}
			s_AfterFunctionCallbacks[function] = afterCallback;
		}
		else if (s_AfterFunctionCallbacks != null)
		{
			s_AfterFunctionCallbacks.Remove(function);
			if (s_AfterFunctionCallbacks.Count == 0)
			{
				s_AfterFunctionCallbacks = null;
			}
		}
		MockRuntime_RegisterFunctionCallbacks((s_BeforeFunctionCallbacks != null) ? new BeforeFunctionDelegate(BeforeFunctionCallback) : null, (s_AfterFunctionCallbacks != null) ? new AfterFunctionDelegate(AfterFunctionCallback) : null);
	}

	public static void SetFunctionCallback(string function, BeforeFunctionDelegate beforeCallback)
	{
		SetFunctionCallback(function, beforeCallback, GetAfterFunctionCallback(function));
	}

	public static void SetFunctionCallback(string function, AfterFunctionDelegate afterCallback)
	{
		SetFunctionCallback(function, GetBeforeFunctionCallback(function), afterCallback);
	}

	public static BeforeFunctionDelegate GetBeforeFunctionCallback(string function)
	{
		if (s_BeforeFunctionCallbacks == null)
		{
			return null;
		}
		if (!s_BeforeFunctionCallbacks.TryGetValue(function, out var value))
		{
			return null;
		}
		return value;
	}

	public static AfterFunctionDelegate GetAfterFunctionCallback(string function)
	{
		if (s_AfterFunctionCallbacks == null)
		{
			return null;
		}
		if (!s_AfterFunctionCallbacks.TryGetValue(function, out var value))
		{
			return null;
		}
		return value;
	}

	public static void ClearFunctionCallbacks()
	{
		s_BeforeFunctionCallbacks = null;
		s_AfterFunctionCallbacks = null;
		MockRuntime_RegisterFunctionCallbacks(null, null);
	}

	public static void ResetDefaults()
	{
		MockRuntime.onScriptEvent = null;
		ClearFunctionCallbacks();
	}

	protected internal override void OnInstanceDestroy(ulong instance)
	{
		ClearFunctionCallbacks();
	}

	[DllImport("mock_api", EntryPoint = "MockRuntime_HookCreateInstance")]
	public static extern IntPtr HookCreateInstance(IntPtr func);

	[DllImport("mock_api", EntryPoint = "MockRuntime_SetKeepFunctionCallbacks")]
	public static extern void SetKeepFunctionCallbacks([MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("mock_api", EntryPoint = "MockRuntime_SetView")]
	public static extern void SetViewPose(XrViewConfigurationType viewConfigurationType, int viewIndex, Vector3 position, Quaternion orientation, Vector4 fov);

	[DllImport("mock_api", EntryPoint = "MockRuntime_SetViewState")]
	public static extern void SetViewState(XrViewConfigurationType viewConfigurationType, XrViewStateFlags viewStateFlags);

	[DllImport("mock_api", EntryPoint = "MockRuntime_SetReferenceSpace")]
	public static extern void SetSpace(XrReferenceSpaceType referenceSpace, Vector3 position, Quaternion orientation, XrSpaceLocationFlags locationFlags);

	[DllImport("mock_api", EntryPoint = "MockRuntime_SetActionSpace")]
	public static extern void SetSpace(ulong actionHandle, Vector3 position, Quaternion orientation, XrSpaceLocationFlags locationFlags);

	[DllImport("mock_api", EntryPoint = "MockRuntime_RegisterScriptEventCallback")]
	private static extern XrResult Internal_RegisterScriptEventCallback(ScriptEventDelegate callback);

	[DllImport("mock_api", EntryPoint = "MockRuntime_TransitionToState")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_TransitionToState(XrSessionState state, [MarshalAs(UnmanagedType.I1)] bool forceTransition);

	[DllImport("mock_api", EntryPoint = "MockRuntime_GetSessionState")]
	private static extern XrSessionState Internal_GetSessionState();

	[DllImport("mock_api", EntryPoint = "MockRuntime_RequestExitSession")]
	public static extern void RequestExitSession();

	[DllImport("mock_api", EntryPoint = "MockRuntime_CauseInstanceLoss")]
	public static extern void CauseInstanceLoss();

	[DllImport("mock_api", EntryPoint = "MockRuntime_CauseUserPresenceChange")]
	public static extern void CauseUserPresenceChange([MarshalAs(UnmanagedType.U1)] bool hasUserPresent);

	[DllImport("mock_api", EntryPoint = "MockRuntime_SetReferenceSpaceBounds")]
	internal static extern void SetReferenceSpaceBounds(XrReferenceSpaceType referenceSpace, Vector2 bounds);

	[DllImport("mock_api", EntryPoint = "MockRuntime_GetEndFrameStats")]
	internal static extern void GetEndFrameStats(out int primaryLayerCount, out int secondaryLayerCount);

	[DllImport("mock_api", EntryPoint = "MockRuntime_ActivateSecondaryView")]
	internal static extern void ActivateSecondaryView(XrViewConfigurationType viewConfigurationType, [MarshalAs(UnmanagedType.I1)] bool activate);

	[DllImport("mock_api")]
	private static extern void MockRuntime_RegisterFunctionCallbacks(BeforeFunctionDelegate hookBefore, AfterFunctionDelegate hookAfter);

	[DllImport("mock_api", EntryPoint = "MockRuntime_MetaPerformanceMetrics_SeedCounterOnce_Float")]
	internal static extern void MetaPerformanceMetrics_SeedCounterOnce_Float(string xrPathString, float value, uint unit);

	[DllImport("mock_api", EntryPoint = "MockRuntime_PerformanceSettings_CauseNotification")]
	internal static extern void PerformanceSettings_CauseNotification(PerformanceDomain domain, PerformanceSubDomain subDomain, PerformanceNotificationLevel level);

	[DllImport("mock_api", EntryPoint = "MockRuntime_PerformanceSettings_GetPerformanceLevelHint")]
	internal static extern PerformanceLevelHint PerformanceSettings_GetPerformanceLevelHint(PerformanceDomain domain);

	internal static bool IsAndroidThreadTypeRegistered(uint threadType)
	{
		return false;
	}

	internal static ulong GetRegisteredAndroidThreadsCount()
	{
		return 0uL;
	}

	internal void AddTestHookGetInstanceProcAddr(Func<IntPtr, IntPtr> nativeFunctionHook)
	{
		MockFunctionInterceptor = nativeFunctionHook;
	}

	internal void ClearTestHookGetInstanceProcAddr()
	{
		MockFunctionInterceptor = null;
	}
}
