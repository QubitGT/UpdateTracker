using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

[Serializable]
public struct GlobalDynamicResolutionSettings
{
	public bool enabled;

	public bool useMipBias;

	public List<AdvancedUpscalers> advancedUpscalersByPriority;

	public uint DLSSPerfQualitySetting;

	public DynamicResolutionHandler.UpsamplerScheduleType DLSSInjectionPoint;

	public DynamicResolutionHandler.UpsamplerScheduleType TAAUInjectionPoint;

	public DynamicResolutionHandler.UpsamplerScheduleType STPInjectionPoint;

	public DynamicResolutionHandler.UpsamplerScheduleType defaultInjectionPoint;

	public bool DLSSUseOptimalSettings;

	[Range(0f, 1f)]
	public float DLSSSharpness;

	public bool FSR2EnableSharpness;

	[Range(0f, 1f)]
	public float FSR2Sharpness;

	public bool FSR2UseOptimalSettings;

	public uint FSR2QualitySetting;

	public DynamicResolutionHandler.UpsamplerScheduleType FSR2InjectionPoint;

	public bool fsrOverrideSharpness;

	[Range(0f, 1f)]
	public float fsrSharpness;

	public float maxPercentage;

	public float minPercentage;

	public DynamicResolutionType dynResType;

	public DynamicResUpscaleFilter upsampleFilter;

	public bool forceResolution;

	public float forcedPercentage;

	public float lowResTransparencyMinimumThreshold;

	public float rayTracingHalfResThreshold;

	public float lowResSSGIMinimumThreshold;

	public float lowResVolumetricCloudsMinimumThreshold;

	[Obsolete("Obsolete, used only for data migration. Use the advancedUpscalersByPriority list instead to add the proper supported advanced upscaler by priority.")]
	public bool enableDLSS;

	public static GlobalDynamicResolutionSettings NewDefault()
	{
		GlobalDynamicResolutionSettings result = default(GlobalDynamicResolutionSettings);
		result.useMipBias = false;
		result.maxPercentage = 100f;
		result.minPercentage = 100f;
		result.dynResType = DynamicResolutionType.Hardware;
		result.upsampleFilter = DynamicResUpscaleFilter.CatmullRom;
		result.forcedPercentage = 100f;
		result.lowResTransparencyMinimumThreshold = 0f;
		result.lowResVolumetricCloudsMinimumThreshold = 50f;
		result.rayTracingHalfResThreshold = 50f;
		result.DLSSUseOptimalSettings = true;
		result.DLSSPerfQualitySetting = 0u;
		result.DLSSSharpness = 0.5f;
		result.DLSSInjectionPoint = DynamicResolutionHandler.UpsamplerScheduleType.BeforePost;
		result.FSR2InjectionPoint = DynamicResolutionHandler.UpsamplerScheduleType.BeforePost;
		result.TAAUInjectionPoint = DynamicResolutionHandler.UpsamplerScheduleType.BeforePost;
		result.defaultInjectionPoint = DynamicResolutionHandler.UpsamplerScheduleType.AfterPost;
		result.advancedUpscalersByPriority = new List<AdvancedUpscalers> { AdvancedUpscalers.STP };
		result.fsrOverrideSharpness = false;
		result.fsrSharpness = 0.92f;
		return result;
	}
}
