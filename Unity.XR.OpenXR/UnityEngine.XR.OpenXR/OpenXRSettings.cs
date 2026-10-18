using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.Serialization;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEngine.XR.OpenXR;

[Serializable]
public class OpenXRSettings : ScriptableObject, ISerializationCallbackReceiver
{
	public enum ColorSubmissionModeGroup
	{
		[InspectorName("8 bits per channel (LDR, default)")]
		kRenderTextureFormatGroup8888,
		[InspectorName("10 bits floating-point per color channel, 2 bit alpha (HDR)")]
		kRenderTextureFormatGroup1010102_Float,
		[InspectorName("16 bits floating-point per channel (HDR)")]
		kRenderTextureFormatGroup16161616_Float,
		[InspectorName("5,6,5 bit packed (LDR, mobile)")]
		kRenderTextureFormatGroup565,
		[InspectorName("11,11,10 bit packed floating-point (HDR)")]
		kRenderTextureFormatGroup111110_Float
	}

	[Serializable]
	public class ColorSubmissionModeList
	{
		public ColorSubmissionModeGroup[] m_List = new ColorSubmissionModeGroup[1];
	}

	public enum RenderMode
	{
		MultiPass,
		SinglePassInstanced
	}

	public enum LatencyOptimization
	{
		PrioritizeRendering,
		PrioritizeInputPolling
	}

	public enum DepthSubmissionMode
	{
		None,
		Depth16Bit,
		Depth24Bit
	}

	public enum BackendFovationApi : byte
	{
		Legacy,
		SRPFoveation
	}

	public enum SpaceWarpMotionVectorTextureFormat
	{
		RGBA16f,
		RG16f
	}

	public enum MultiviewRenderRegionsOptimizationMode : byte
	{
		None,
		FinalPass,
		AllPasses
	}

	[FormerlySerializedAs("extensions")]
	[HideInInspector]
	[SerializeField]
	internal OpenXRFeature[] features = new OpenXRFeature[0];

	public static readonly ColorSubmissionModeGroup kDefaultColorMode;

	[SerializeField]
	private RenderMode m_renderMode = RenderMode.SinglePassInstanced;

	[SerializeField]
	private LatencyOptimization m_latencyOptimization;

	[SerializeField]
	private bool m_autoColorSubmissionMode = true;

	[SerializeField]
	private ColorSubmissionModeList m_colorSubmissionModes = new ColorSubmissionModeList();

	[SerializeField]
	private DepthSubmissionMode m_depthSubmissionMode;

	[SerializeField]
	private SpaceWarpMotionVectorTextureFormat m_spacewarpMotionVectorTextureFormat;

	[SerializeField]
	private bool m_optimizeBufferDiscards;

	[SerializeField]
	private bool m_symmetricProjection;

	[SerializeField]
	[HideInInspector]
	[Obsolete("m_optimizeMultiviewRenderRegions is deprecated. Use m_multiviewRenderRegionsOptimizationMode instead.", false)]
	private bool m_optimizeMultiviewRenderRegions;

	[SerializeField]
	[HideInInspector]
	private MultiviewRenderRegionsOptimizationMode m_multiviewRenderRegionsOptimizationMode;

	[SerializeField]
	[HideInInspector]
	private bool m_hasMigratedMultiviewRenderRegionSetting;

	[SerializeField]
	private BackendFovationApi m_foveatedRenderingApi;

	[SerializeField]
	private bool m_useOpenXRPredictedTime;

	private const string LibraryName = "UnityOpenXR";

	private static OpenXRSettings s_RuntimeInstance;

	public int featureCount => features.Length;

	public RenderMode renderMode
	{
		get
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				return Internal_GetRenderMode();
			}
			return m_renderMode;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetRenderMode(value);
			}
			else
			{
				m_renderMode = value;
			}
		}
	}

	public LatencyOptimization latencyOptimization
	{
		get
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				return Internal_GetLatencyOptimization();
			}
			return m_latencyOptimization;
		}
		set
		{
			m_latencyOptimization = value;
		}
	}

	public bool autoColorSubmissionMode
	{
		get
		{
			return m_autoColorSubmissionMode;
		}
		set
		{
			m_autoColorSubmissionMode = value;
		}
	}

	public ColorSubmissionModeGroup[] colorSubmissionModes
	{
		get
		{
			if (m_autoColorSubmissionMode)
			{
				return new ColorSubmissionModeGroup[1] { kDefaultColorMode };
			}
			if (OpenXRLoaderBase.Instance != null)
			{
				int num = Internal_GetColorSubmissionModes(null, 0);
				int[] array = new int[num];
				Internal_GetColorSubmissionModes(array, num);
				return array.Select((int i) => (ColorSubmissionModeGroup)i).ToArray();
			}
			return m_colorSubmissionModes.m_List;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetColorSubmissionModes(value.Select((ColorSubmissionModeGroup e) => (int)e).ToArray(), value.Length);
			}
			else
			{
				m_colorSubmissionModes.m_List = value;
			}
		}
	}

	public DepthSubmissionMode depthSubmissionMode
	{
		get
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				return Internal_GetDepthSubmissionMode();
			}
			return m_depthSubmissionMode;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetDepthSubmissionMode(value);
			}
			else
			{
				m_depthSubmissionMode = value;
			}
		}
	}

	public SpaceWarpMotionVectorTextureFormat spacewarpMotionVectorTextureFormat
	{
		get
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				return Internal_GetSpaceWarpMotionVectorTextureFormat();
			}
			return m_spacewarpMotionVectorTextureFormat;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetSpaceWarpMotionVectorTextureFormat(value);
			}
			else
			{
				m_spacewarpMotionVectorTextureFormat = value;
			}
		}
	}

	public bool optimizeBufferDiscards
	{
		get
		{
			return m_optimizeBufferDiscards;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetOptimizeBufferDiscards(value);
			}
			else
			{
				m_optimizeBufferDiscards = value;
			}
		}
	}

	public bool symmetricProjection
	{
		get
		{
			return m_symmetricProjection;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetSymmetricProjection(value);
			}
			else
			{
				m_symmetricProjection = value;
			}
		}
	}

	[Obsolete("optimizeMultiviewRenderRegions is deprecated. Use multiviewRenderRegionsMode instead.", false)]
	public bool optimizeMultiviewRenderRegions
	{
		get
		{
			if (m_multiviewRenderRegionsOptimizationMode != MultiviewRenderRegionsOptimizationMode.FinalPass)
			{
				return m_multiviewRenderRegionsOptimizationMode == MultiviewRenderRegionsOptimizationMode.AllPasses;
			}
			return true;
		}
		set
		{
			MultiviewRenderRegionsOptimizationMode mode = (value ? MultiviewRenderRegionsOptimizationMode.FinalPass : MultiviewRenderRegionsOptimizationMode.None);
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetMultiviewRenderRegionsOptimizationMode(mode);
				return;
			}
			m_optimizeMultiviewRenderRegions = value;
			m_multiviewRenderRegionsOptimizationMode = mode;
		}
	}

	public MultiviewRenderRegionsOptimizationMode multiviewRenderRegionsOptimizationMode
	{
		get
		{
			return m_multiviewRenderRegionsOptimizationMode;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetMultiviewRenderRegionsOptimizationMode(value);
			}
			else
			{
				m_multiviewRenderRegionsOptimizationMode = value;
			}
		}
	}

	public BackendFovationApi foveatedRenderingApi
	{
		get
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				return Internal_GetUsedFoveatedRenderingApi();
			}
			return m_foveatedRenderingApi;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetUsedFoveatedRenderingApi(value);
			}
			else
			{
				m_foveatedRenderingApi = value;
			}
		}
	}

	public bool useOpenXRPredictedTime
	{
		get
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				return Internal_GetUseOpenXRPredictedTime();
			}
			return m_useOpenXRPredictedTime;
		}
		set
		{
			if (OpenXRLoaderBase.Instance != null)
			{
				Internal_SetUseOpenXRPredictedTime(value);
			}
			else
			{
				m_useOpenXRPredictedTime = value;
			}
		}
	}

	public static OpenXRSettings ActiveBuildTargetInstance => GetInstance(useActiveBuildTarget: true);

	public static OpenXRSettings Instance => GetInstance(useActiveBuildTarget: false);

	public static bool AllowRecentering => Internal_GetAllowRecentering();

	public static float FloorOffset => Internal_GetFloorOffset();

	public TFeature GetFeature<TFeature>() where TFeature : OpenXRFeature
	{
		return (TFeature)GetFeature(typeof(TFeature));
	}

	public OpenXRFeature GetFeature(Type featureType)
	{
		OpenXRFeature[] array = features;
		foreach (OpenXRFeature openXRFeature in array)
		{
			if (featureType.IsInstanceOfType(openXRFeature))
			{
				return openXRFeature;
			}
		}
		return null;
	}

	public OpenXRFeature[] GetFeatures<TFeature>()
	{
		return GetFeatures(typeof(TFeature));
	}

	public OpenXRFeature[] GetFeatures(Type featureType)
	{
		List<OpenXRFeature> list = new List<OpenXRFeature>();
		OpenXRFeature[] array = features;
		foreach (OpenXRFeature openXRFeature in array)
		{
			if (featureType.IsInstanceOfType(openXRFeature))
			{
				list.Add(openXRFeature);
			}
		}
		return list.ToArray();
	}

	public int GetFeatures<TFeature>(List<TFeature> featuresOut) where TFeature : OpenXRFeature
	{
		featuresOut.Clear();
		OpenXRFeature[] array = features;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is TFeature item)
			{
				featuresOut.Add(item);
			}
		}
		return featuresOut.Count;
	}

	public int GetFeatures(Type featureType, List<OpenXRFeature> featuresOut)
	{
		featuresOut.Clear();
		OpenXRFeature[] array = features;
		foreach (OpenXRFeature openXRFeature in array)
		{
			if (featureType.IsInstanceOfType(openXRFeature))
			{
				featuresOut.Add(openXRFeature);
			}
		}
		return featuresOut.Count;
	}

	public OpenXRFeature[] GetFeatures()
	{
		return ((OpenXRFeature[])features?.Clone()) ?? new OpenXRFeature[0];
	}

	public int GetFeatures(List<OpenXRFeature> featuresOut)
	{
		featuresOut.Clear();
		featuresOut.AddRange(features);
		return featuresOut.Count;
	}

	private void ApplyPermissionSettings()
	{
	}

	[DllImport("UnityOpenXR", EntryPoint = "OculusFoveation_SetHasEyeTrackingPermissions")]
	internal static extern void Internal_SetHasEyeTrackingPermissions([MarshalAs(UnmanagedType.I1)] bool value);

	private void ApplyRenderSettings()
	{
		Internal_SetSymmetricProjection(m_symmetricProjection);
		Internal_SetMultiviewRenderRegionsOptimizationMode(m_multiviewRenderRegionsOptimizationMode);
		Internal_SetUseOpenXRPredictedTime(m_useOpenXRPredictedTime);
		Internal_SetUsedFoveatedRenderingApi(m_foveatedRenderingApi);
		Internal_SetRenderMode(m_renderMode);
		Internal_SetLatencyOptimization(m_latencyOptimization);
		Internal_SetColorSubmissionModes(m_colorSubmissionModes.m_List.Select((ColorSubmissionModeGroup e) => (int)e).ToArray(), m_colorSubmissionModes.m_List.Length);
		Internal_SetDepthSubmissionMode(m_depthSubmissionMode);
		Internal_SetSpaceWarpMotionVectorTextureFormat(m_spacewarpMotionVectorTextureFormat);
		Internal_SetOptimizeBufferDiscards(m_optimizeBufferDiscards);
	}

	public void OnBeforeSerialize()
	{
		m_optimizeMultiviewRenderRegions = m_multiviewRenderRegionsOptimizationMode != MultiviewRenderRegionsOptimizationMode.None;
	}

	public void OnAfterDeserialize()
	{
		if (!m_hasMigratedMultiviewRenderRegionSetting)
		{
			if (m_optimizeMultiviewRenderRegions)
			{
				m_multiviewRenderRegionsOptimizationMode = MultiviewRenderRegionsOptimizationMode.FinalPass;
			}
			else
			{
				m_multiviewRenderRegionsOptimizationMode = MultiviewRenderRegionsOptimizationMode.None;
			}
			m_hasMigratedMultiviewRenderRegionSetting = true;
		}
	}

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetRenderMode")]
	private static extern void Internal_SetRenderMode(RenderMode renderMode);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetRenderMode")]
	private static extern RenderMode Internal_GetRenderMode();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetLatencyOptimization")]
	private static extern void Internal_SetLatencyOptimization(LatencyOptimization latencyOptimzation);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetLatencyOptimization")]
	private static extern LatencyOptimization Internal_GetLatencyOptimization();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetDepthSubmissionMode")]
	private static extern void Internal_SetDepthSubmissionMode(DepthSubmissionMode depthSubmissionMode);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetDepthSubmissionMode")]
	private static extern DepthSubmissionMode Internal_GetDepthSubmissionMode();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetSpaceWarpMotionVectorTextureFormat")]
	private static extern void Internal_SetSpaceWarpMotionVectorTextureFormat(SpaceWarpMotionVectorTextureFormat spaceWarpMotionVectorTextureFormat);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetSpaceWarpMotionVectorTextureFormat")]
	private static extern SpaceWarpMotionVectorTextureFormat Internal_GetSpaceWarpMotionVectorTextureFormat();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetSymmetricProjection")]
	private static extern void Internal_SetSymmetricProjection([MarshalAs(UnmanagedType.I1)] bool enabled);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetMultiviewRenderRegionsOptimizationMode")]
	private static extern void Internal_SetMultiviewRenderRegionsOptimizationMode(MultiviewRenderRegionsOptimizationMode mode);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetOptimizeBufferDiscards")]
	private static extern void Internal_SetOptimizeBufferDiscards([MarshalAs(UnmanagedType.I1)] bool enabled);

	[DllImport("UnityOpenXR", EntryPoint = "OculusFoveation_SetUsedApi")]
	private static extern void Internal_SetUsedFoveatedRenderingApi(BackendFovationApi api);

	[DllImport("UnityOpenXR", EntryPoint = "OculusFoveation_GetUsedApi")]
	internal static extern BackendFovationApi Internal_GetUsedFoveatedRenderingApi();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetColorSubmissionMode")]
	private static extern void Internal_SetColorSubmissionMode(ColorSubmissionModeGroup[] colorSubmissionMode);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetColorSubmissionModes")]
	private static extern void Internal_SetColorSubmissionModes(int[] colorSubmissionMode, int arraySize);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetColorSubmissionModes")]
	private static extern int Internal_GetColorSubmissionModes([Out] int[] colorSubmissionMode, int arraySize);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetIsUsingLegacyXRDisplay")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetIsUsingLegacyXRDisplay();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetUseOpenXRPredictedTime")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetUseOpenXRPredictedTime();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetUseOpenXRPredictedTime")]
	private static extern void Internal_SetUseOpenXRPredictedTime([MarshalAs(UnmanagedType.I1)] bool enabled);

	private void Awake()
	{
		s_RuntimeInstance = this;
	}

	internal void ApplySettings()
	{
		ApplyRenderSettings();
		ApplyPermissionSettings();
	}

	private static OpenXRSettings GetInstance(bool useActiveBuildTarget)
	{
		OpenXRSettings openXRSettings = null;
		openXRSettings = s_RuntimeInstance;
		if (openXRSettings == null)
		{
			openXRSettings = ScriptableObject.CreateInstance<OpenXRSettings>();
		}
		return openXRSettings;
	}

	public static void SetAllowRecentering(bool allowRecentering, float floorOffset = 1.5f)
	{
		Internal_SetAllowRecentering(allowRecentering, floorOffset);
	}

	public static void RefreshRecenterSpace()
	{
		Internal_RegenerateTrackingOrigin();
	}

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetAllowRecentering")]
	private static extern void Internal_SetAllowRecentering([MarshalAs(UnmanagedType.U1)] bool active, float height);

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_RegenerateTrackingOrigin")]
	private static extern void Internal_RegenerateTrackingOrigin();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetAllowRecentering")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetAllowRecentering();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetFloorOffsetHeight")]
	private static extern float Internal_GetFloorOffset();
}
