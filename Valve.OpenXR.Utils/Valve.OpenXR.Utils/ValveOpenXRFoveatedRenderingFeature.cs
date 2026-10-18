using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace Valve.OpenXR.Utils;

public class ValveOpenXRFoveatedRenderingFeature : OpenXRFeature
{
	private delegate XrResult XrGetFoveationEyeTrackedStateMETADelegate(ulong session, ref XrFoveationEyeTrackedStateMETA foveationState);

	internal static class NativeMethods
	{
		[DllImport("UnityOpenXR")]
		internal static extern void FBSetFoveationLevel(ulong session, uint level, float verticalOffset, uint dynamic);

		[DllImport("UnityOpenXR")]
		internal static extern void FBGetFoveationLevel(out uint level);

		[DllImport("UnityOpenXR")]
		internal static extern void FBGetFoveationDynamic(out uint dynamic);

		[DllImport("UnityOpenXR")]
		internal static extern void MetaSetFoveationEyeTracked(ulong session, bool isEyeTracked);

		[DllImport("UnityOpenXR")]
		internal static extern void MetaGetFoveationEyeTracked(out bool isEyeTracked);

		[DllImport("UnityOpenXR", EntryPoint = "OculusFoveation_SetHasEyeTrackingPermissions")]
		internal static extern void Internal_SetHasEyeTrackingPermissions([MarshalAs(UnmanagedType.I1)] bool value);
	}

	private struct XrFoveationEyeTrackedStateMETA
	{
		public XrStructureType type;

		public IntPtr next;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public XrVector2f[] foveationCenter;

		public XrFoveationEyeTrackedStateFlagsMETA flags;
	}

	[Flags]
	private enum XrFoveationEyeTrackedStateFlagsMETA
	{
		XR_FOVEATION_EYE_TRACKED_STATE_VALID_BIT_META = 1
	}

	public const string featureId = "com.valvesoftware.openxr.utils.foveated_rendering";

	private const string extensionStrings = "XR_FB_foveation XR_FB_foveation_configuration XR_FB_foveation_vulkan XR_FB_swapchain_update_state XR_META_foveation_eye_tracked XR_META_vulkan_swapchain_create_info";

	[SerializeField]
	private bool applySettingsOnStartup;

	[SerializeField]
	[Range(0f, 1f)]
	public float initialFoveationLevel;

	[SerializeField]
	public bool initialUseEyeTracking;

	private XrGetFoveationEyeTrackedStateMETADelegate _xrGetFoveationEyeTrackedStateMETA;

	private ulong _xrSession;

	public float foveatedRenderingLevel
	{
		get
		{
			float result = 0f;
			List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
			SubsystemManager.GetSubsystems(list);
			foreach (XRDisplaySubsystem item in list)
			{
				if (item.running)
				{
					result = item.foveatedRenderingLevel;
					break;
				}
			}
			return result;
		}
		set
		{
			List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
			SubsystemManager.GetSubsystems(list);
			foreach (XRDisplaySubsystem item in list)
			{
				if (item.running)
				{
					item.foveatedRenderingLevel = value;
					break;
				}
			}
		}
	}

	public bool eyeTrackedFoveation
	{
		get
		{
			bool result = false;
			List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
			SubsystemManager.GetSubsystems(list);
			foreach (XRDisplaySubsystem item in list)
			{
				if (item.running)
				{
					result = item.foveatedRenderingFlags == XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
					break;
				}
			}
			return result;
		}
		set
		{
			List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
			SubsystemManager.GetSubsystems(list);
			foreach (XRDisplaySubsystem item in list)
			{
				item.foveatedRenderingFlags = (value ? XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed : XRDisplaySubsystem.FoveatedRenderingFlags.None);
			}
		}
	}

	public bool GetFoveationEyeTrackedCenter(ref Vector2 leftEye, ref Vector2 rightEye)
	{
		if (_xrGetFoveationEyeTrackedStateMETA == null)
		{
			_xrGetFoveationEyeTrackedStateMETA = ValveOpenXRSupportFeature.GetOpenXrInstanceProc<XrGetFoveationEyeTrackedStateMETADelegate>("xrGetFoveationEyeTrackedStateMETA");
		}
		if (_xrGetFoveationEyeTrackedStateMETA == null)
		{
			return false;
		}
		XrFoveationEyeTrackedStateMETA xrFoveationEyeTrackedStateMETA = default(XrFoveationEyeTrackedStateMETA);
		xrFoveationEyeTrackedStateMETA.type = XrStructureType.XR_TYPE_FOVEATION_EYE_TRACKED_STATE_META;
		XrFoveationEyeTrackedStateMETA foveationState = xrFoveationEyeTrackedStateMETA;
		if (_xrGetFoveationEyeTrackedStateMETA(_xrSession, ref foveationState) != 0 || foveationState.flags != XrFoveationEyeTrackedStateFlagsMETA.XR_FOVEATION_EYE_TRACKED_STATE_VALID_BIT_META)
		{
			return false;
		}
		leftEye.Set(foveationState.foveationCenter[0].X, foveationState.foveationCenter[0].Y);
		rightEye.Set(foveationState.foveationCenter[1].X, foveationState.foveationCenter[1].Y);
		return true;
	}

	protected override void OnSessionCreate(ulong xrSession)
	{
		_xrSession = xrSession;
		NativeMethods.Internal_SetHasEyeTrackingPermissions(value: true);
	}

	protected override void OnSessionStateChange(int oldState, int newState)
	{
		if (oldState == 4 && newState == 5 && applySettingsOnStartup)
		{
			foveatedRenderingLevel = initialFoveationLevel;
			eyeTrackedFoveation = initialUseEyeTracking;
		}
	}
}
