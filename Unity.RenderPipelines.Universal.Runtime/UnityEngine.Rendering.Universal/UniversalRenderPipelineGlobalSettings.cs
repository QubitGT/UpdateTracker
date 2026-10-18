using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal;

[DisplayInfo(name = "URP Global Settings Asset", order = 40002)]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[DisplayName("URP")]
internal class UniversalRenderPipelineGlobalSettings : RenderPipelineGlobalSettings<UniversalRenderPipelineGlobalSettings, UniversalRenderPipeline>
{
	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal ShaderStrippingSetting m_ShaderStrippingSetting = new ShaderStrippingSetting();

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal URPShaderStrippingSetting m_URPShaderStrippingSetting = new URPShaderStrippingSetting();

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal UnityEngine.Rendering.ShaderVariantLogLevel m_ShaderVariantLogLevel;

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal bool m_ExportShaderVariants = true;

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal bool m_StripDebugVariants = true;

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal bool m_StripUnusedPostProcessingVariants;

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal bool m_StripUnusedVariants = true;

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal bool m_StripScreenCoordOverrideVariants = true;

	[Obsolete("Please use stripRuntimeDebugShaders instead. #from(23.1)", false)]
	public bool supportRuntimeDebugDisplay;

	[SerializeField]
	[Obsolete("Keep for migration. #from(23.2)")]
	internal bool m_EnableRenderGraph;

	[SerializeField]
	private RenderPipelineGraphicsSettingsContainer m_Settings = new RenderPipelineGraphicsSettingsContainer();

	internal const int k_LastVersion = 8;

	[SerializeField]
	[FormerlySerializedAs("k_AssetVersion")]
	internal int m_AssetVersion = 8;

	public const string defaultAssetName = "UniversalRenderPipelineGlobalSettings";

	[SerializeField]
	[FormerlySerializedAs("m_DefaultVolumeProfile")]
	[Obsolete("Kept For Migration. #from(2023.3)")]
	internal VolumeProfile m_ObsoleteDefaultVolumeProfile;

	[SerializeField]
	internal string[] m_RenderingLayerNames = new string[1] { "Default" };

	[SerializeField]
	private uint m_ValidRenderingLayers;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName0;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName1;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName2;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName3;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName4;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName5;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName6;

	[Obsolete("This is obsolete, please use renderingLayerNames instead.", false)]
	public string lightLayerName7;

	[SerializeField]
	internal ProbeVolumeSceneData apvScenesData;

	[Obsolete("No longer used as Shader Prefiltering automatically strips out unused LOD Crossfade variants. Please use the LOD Crossfade setting in the URP Asset to disable the feature if not used. #from(2023.1)", false)]
	public bool stripUnusedLODCrossFadeVariants
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	protected override List<IRenderPipelineGraphicsSettings> settingsList => m_Settings.settingsList;

	[Obsolete("This is obsolete, please use prefixedRenderingLayerMaskNames instead.", true)]
	public string[] prefixedLightLayerNames => new string[0];

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string[] lightLayerNames => new string[0];

	internal bool IsAtLastVersion()
	{
		return 8 == m_AssetVersion;
	}

	public override void Reset()
	{
		base.Reset();
		DecalProjector.UpdateAllDecalProperties();
	}

	internal static VolumeProfile GetOrCreateDefaultVolumeProfile(VolumeProfile defaultVolumeProfile)
	{
		return defaultVolumeProfile;
	}

	internal void ResetRenderingLayerNames()
	{
		m_RenderingLayerNames = new string[1] { "Default" };
	}

	internal ProbeVolumeSceneData GetOrCreateAPVSceneData()
	{
		if (apvScenesData == null)
		{
			apvScenesData = new ProbeVolumeSceneData(this);
		}
		apvScenesData.SetParentObject(this);
		return apvScenesData;
	}
}
