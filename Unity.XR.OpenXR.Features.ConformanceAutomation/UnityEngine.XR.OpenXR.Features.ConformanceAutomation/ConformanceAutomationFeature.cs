using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.ConformanceAutomation;

public class ConformanceAutomationFeature : OpenXRFeature
{
	public const string featureId = "com.unity.openxr.feature.conformance";

	private static ulong xrInstance;

	private static ulong xrSession;

	private const string ExtLib = "ConformanceAutomationExt";

	protected internal override bool OnInstanceCreate(ulong instance)
	{
		if (!OpenXRRuntime.IsExtensionEnabled("XR_EXT_conformance_automation"))
		{
			Debug.LogError("XR_EXT_conformance_automation is not enabled. Disabling ConformanceAutomationExt");
			return false;
		}
		xrInstance = instance;
		xrSession = 0uL;
		initialize(OpenXRFeature.xrGetInstanceProcAddr, xrInstance);
		return true;
	}

	protected internal override void OnInstanceDestroy(ulong xrInstance)
	{
		base.OnInstanceDestroy(xrInstance);
		ConformanceAutomationFeature.xrInstance = 0uL;
	}

	protected internal override void OnSessionCreate(ulong xrSessionId)
	{
		xrSession = xrSessionId;
		base.OnSessionCreate(xrSession);
	}

	protected internal override void OnSessionDestroy(ulong xrSessionId)
	{
		base.OnSessionDestroy(xrSessionId);
		xrSession = 0uL;
	}

	public static bool ConformanceAutomationSetActive(string interactionProfile, string topLevelPath, bool isActive)
	{
		return xrSetInputDeviceActiveEXT(xrSession, OpenXRFeature.StringToPath(interactionProfile), OpenXRFeature.StringToPath(topLevelPath), isActive);
	}

	public static bool ConformanceAutomationSetBool(string topLevelPath, string inputSourcePath, bool state)
	{
		return xrSetInputDeviceStateBoolEXT(xrSession, OpenXRFeature.StringToPath(topLevelPath), OpenXRFeature.StringToPath(inputSourcePath), state);
	}

	public static bool ConformanceAutomationSetFloat(string topLevelPath, string inputSourcePath, float state)
	{
		return xrSetInputDeviceStateFloatEXT(xrSession, OpenXRFeature.StringToPath(topLevelPath), OpenXRFeature.StringToPath(inputSourcePath), state);
	}

	public static bool ConformanceAutomationSetVec2(string topLevelPath, string inputSourcePath, Vector2 state)
	{
		return xrSetInputDeviceStateVector2fEXT(xrSession, OpenXRFeature.StringToPath(topLevelPath), OpenXRFeature.StringToPath(inputSourcePath), new XrVector2f(state));
	}

	public static bool ConformanceAutomationSetPose(string topLevelPath, string inputSourcePath, Vector3 position, Quaternion orientation)
	{
		return xrSetInputDeviceLocationEXT(xrSession, OpenXRFeature.StringToPath(topLevelPath), OpenXRFeature.StringToPath(inputSourcePath), OpenXRFeature.GetCurrentAppSpace(), new XrPosef(position, orientation));
	}

	public static bool ConformanceAutomationSetVelocity(string topLevelPath, string inputSourcePath, bool linearValid, Vector3 linear, bool angularValid, Vector3 angular)
	{
		return xrSetInputDeviceVelocityUNITY(xrSession, OpenXRFeature.StringToPath(topLevelPath), OpenXRFeature.StringToPath(inputSourcePath), linearValid, new XrVector3f(linear), angularValid, new XrVector3f(-1f * angular));
	}

	[DllImport("ConformanceAutomationExt", EntryPoint = "script_initialize")]
	private static extern void initialize(IntPtr xrGetInstanceProcAddr, ulong xrInstance);

	[DllImport("ConformanceAutomationExt", EntryPoint = "script_xrSetInputDeviceActiveEXT")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool xrSetInputDeviceActiveEXT(ulong xrSession, ulong interactionProfile, ulong topLevelPath, [MarshalAs(UnmanagedType.I1)] bool isActive);

	[DllImport("ConformanceAutomationExt", EntryPoint = "script_xrSetInputDeviceStateBoolEXT")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool xrSetInputDeviceStateBoolEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, [MarshalAs(UnmanagedType.I1)] bool state);

	[DllImport("ConformanceAutomationExt", EntryPoint = "script_xrSetInputDeviceStateFloatEXT")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool xrSetInputDeviceStateFloatEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, float state);

	[DllImport("ConformanceAutomationExt", EntryPoint = "script_xrSetInputDeviceStateVector2fEXT")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool xrSetInputDeviceStateVector2fEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, XrVector2f state);

	[DllImport("ConformanceAutomationExt", EntryPoint = "script_xrSetInputDeviceLocationEXT")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool xrSetInputDeviceLocationEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, ulong space, XrPosef pose);

	[DllImport("ConformanceAutomationExt", EntryPoint = "script_xrSetInputDeviceVelocityUNITY")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool xrSetInputDeviceVelocityUNITY(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, [MarshalAs(UnmanagedType.I1)] bool linearValid, XrVector3f linear, [MarshalAs(UnmanagedType.I1)] bool angularValid, XrVector3f angular);
}
