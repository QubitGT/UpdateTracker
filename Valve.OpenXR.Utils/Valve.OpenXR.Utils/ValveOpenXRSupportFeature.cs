using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace Valve.OpenXR.Utils;

public class ValveOpenXRSupportFeature : OpenXRFeature
{
	private delegate XrResult GetInstanceProcAddrDelegate(ulong instance, string name, ref IntPtr procAddr);

	public const string featureId = "com.valvesoftware.openxr.utils.support";

	[SerializeField]
	[Tooltip("Optimization that allows 4x MSAA textures to be memoryless on Vulkan")]
	internal bool optimizeBufferDiscards;

	[SerializeField]
	[Tooltip("Vulkan only")]
	internal bool lateLatchingMode;

	[SerializeField]
	[Tooltip("Vulkan only")]
	internal bool lateLatchingDebug;

	private ulong _xrInstance;

	private ulong _xrSession;

	private GetInstanceProcAddrDelegate _getInstanceProcAddr;

	private Type[] incompatibleFeatureTypes = new Type[1] { typeof(MetaQuestFeature) };

	private ulong XrInstance => _xrInstance;

	private ulong XrSession => _xrSession;

	internal static ValveOpenXRSupportFeature Instance
	{
		get
		{
			OpenXRSettings activeBuildTargetInstance = OpenXRSettings.ActiveBuildTargetInstance;
			List<ValveOpenXRSupportFeature> list = new List<ValveOpenXRSupportFeature>();
			if (activeBuildTargetInstance.GetFeatures(list) <= 0)
			{
				return null;
			}
			return list.First();
		}
	}

	public static event InstanceCreated OnInstanceCreated;

	public static event InstanceDestroyed OnInstanceDestroyed;

	public static event SessionCreated OnSessionCreated;

	public static event SessionDestroyed OnSessionDestroyed;

	public static bool HasOpenXRInstance()
	{
		return Instance.XrInstance != 0;
	}

	public static ulong GetOpenXRInstance()
	{
		return Instance.XrInstance;
	}

	public static bool HasSession()
	{
		return Instance.XrSession != 0;
	}

	public static ulong GetSession()
	{
		return Instance.XrSession;
	}

	public static T GetOpenXrInstanceProc<T>(string procName)
	{
		IntPtr openXrInstanceProc = GetOpenXrInstanceProc(procName);
		if (!(openXrInstanceProc != IntPtr.Zero))
		{
			return default(T);
		}
		return Marshal.GetDelegateForFunctionPointer<T>(openXrInstanceProc);
	}

	public static IntPtr GetOpenXrInstanceProc(string procName)
	{
		return Instance.GetOpenXrInstanceProcInternal(procName);
	}

	protected override bool OnInstanceCreate(ulong xrInstance)
	{
		_xrInstance = xrInstance;
		ValveOpenXRSupportFeature.OnInstanceCreated?.Invoke(xrInstance);
		return true;
	}

	protected override void OnInstanceDestroy(ulong xrInstance)
	{
		ValveOpenXRSupportFeature.OnInstanceDestroyed?.Invoke(xrInstance);
		_xrInstance = 0uL;
	}

	protected override void OnSessionCreate(ulong xrSession)
	{
		_xrSession = xrSession;
		ValveOpenXRSupportFeature.OnSessionCreated?.Invoke(_xrSession);
	}

	protected override void OnSessionDestroy(ulong xrSession)
	{
		ValveOpenXRSupportFeature.OnSessionDestroyed?.Invoke(_xrSession);
		_xrSession = 0uL;
	}

	private IntPtr GetOpenXrInstanceProcInternal(string procName)
	{
		if (_getInstanceProcAddr == null)
		{
			if (OpenXRFeature.xrGetInstanceProcAddr == IntPtr.Zero)
			{
				Debug.LogWarning("Unity's OpenXR GetInstance function accessor is invalid!");
				return IntPtr.Zero;
			}
			_getInstanceProcAddr = Marshal.GetDelegateForFunctionPointer<GetInstanceProcAddrDelegate>(OpenXRFeature.xrGetInstanceProcAddr);
		}
		IntPtr procAddr = IntPtr.Zero;
		if (_getInstanceProcAddr != null && _getInstanceProcAddr(_xrInstance, procName, ref procAddr) < XrResult.Success)
		{
			Debug.LogWarning("Failed to find OpenXR instance function '" + procName + "'");
			return IntPtr.Zero;
		}
		return procAddr;
	}
}
