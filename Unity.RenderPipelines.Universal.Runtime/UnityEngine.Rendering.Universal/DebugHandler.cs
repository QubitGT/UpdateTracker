using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DebugHandler : IDebugDisplaySettingsQuery
{
	private class DebugFinalValidationPassData
	{
		public bool isFinalPass;

		public bool resolveFinalTarget;

		public bool isActiveForCamera;

		public bool hasDebugRenderTarget;

		public TextureHandle debugRenderTargetHandle;

		public int debugTexturePropertyId;

		public Vector4 debugRenderTargetPixelRect;

		public int debugRenderTargetSupportsStereo;

		public Vector4 debugRenderTargetRangeRemap;

		public TextureHandle debugFontTextureHandle;

		public DebugDisplaySettingsRendering renderingSettings;
	}

	private class DebugSetupPassData
	{
		public bool isActiveForCamera;

		public DebugDisplaySettingsMaterial materialSettings;

		public DebugDisplaySettingsRendering renderingSettings;

		public DebugDisplaySettingsLighting lightingSettings;
	}

	private static readonly int k_DebugColorInvalidModePropertyId = Shader.PropertyToID("_DebugColorInvalidMode");

	private static readonly int k_DebugCurrentRealTimeId = Shader.PropertyToID("_DebugCurrentRealTime");

	private static readonly int k_DebugColorPropertyId = Shader.PropertyToID("_DebugColor");

	private static readonly int k_DebugTexturePropertyId = Shader.PropertyToID("_DebugTexture");

	private static readonly int k_DebugFontId = Shader.PropertyToID("_DebugFont");

	private static readonly int k_DebugTextureNoStereoPropertyId = Shader.PropertyToID("_DebugTextureNoStereo");

	private static readonly int k_DebugTextureDisplayRect = Shader.PropertyToID("_DebugTextureDisplayRect");

	private static readonly int k_DebugRenderTargetSupportsStereo = Shader.PropertyToID("_DebugRenderTargetSupportsStereo");

	private static readonly int k_DebugRenderTargetRangeRemap = Shader.PropertyToID("_DebugRenderTargetRangeRemap");

	private static readonly int k_DebugMaterialModeId = Shader.PropertyToID("_DebugMaterialMode");

	private static readonly int k_DebugVertexAttributeModeId = Shader.PropertyToID("_DebugVertexAttributeMode");

	private static readonly int k_DebugMaterialValidationModeId = Shader.PropertyToID("_DebugMaterialValidationMode");

	private static readonly int k_DebugMipInfoModeId = Shader.PropertyToID("_DebugMipInfoMode");

	private static readonly int k_DebugMipMapStatusModeId = Shader.PropertyToID("_DebugMipMapStatusMode");

	private static readonly int k_DebugMipMapShowStatusCodeId = Shader.PropertyToID("_DebugMipMapShowStatusCode");

	private static readonly int k_DebugMipMapOpacityId = Shader.PropertyToID("_DebugMipMapOpacity");

	private static readonly int k_DebugMipMapRecentlyUpdatedCooldownId = Shader.PropertyToID("_DebugMipMapRecentlyUpdatedCooldown");

	private static readonly int k_DebugMipMapTerrainTextureModeId = Shader.PropertyToID("_DebugMipMapTerrainTextureMode");

	private static readonly int k_DebugSceneOverrideModeId = Shader.PropertyToID("_DebugSceneOverrideMode");

	private static readonly int k_DebugFullScreenModeId = Shader.PropertyToID("_DebugFullScreenMode");

	private static readonly int k_DebugValidationModeId = Shader.PropertyToID("_DebugValidationMode");

	private static readonly int k_DebugValidateBelowMinThresholdColorPropertyId = Shader.PropertyToID("_DebugValidateBelowMinThresholdColor");

	private static readonly int k_DebugValidateAboveMaxThresholdColorPropertyId = Shader.PropertyToID("_DebugValidateAboveMaxThresholdColor");

	private static readonly int k_DebugMaxPixelCost = Shader.PropertyToID("_DebugMaxPixelCost");

	private static readonly int k_DebugLightingModeId = Shader.PropertyToID("_DebugLightingMode");

	private static readonly int k_DebugLightingFeatureFlagsId = Shader.PropertyToID("_DebugLightingFeatureFlags");

	private static readonly int k_DebugValidateAlbedoMinLuminanceId = Shader.PropertyToID("_DebugValidateAlbedoMinLuminance");

	private static readonly int k_DebugValidateAlbedoMaxLuminanceId = Shader.PropertyToID("_DebugValidateAlbedoMaxLuminance");

	private static readonly int k_DebugValidateAlbedoSaturationToleranceId = Shader.PropertyToID("_DebugValidateAlbedoSaturationTolerance");

	private static readonly int k_DebugValidateAlbedoHueToleranceId = Shader.PropertyToID("_DebugValidateAlbedoHueTolerance");

	private static readonly int k_DebugValidateAlbedoCompareColorId = Shader.PropertyToID("_DebugValidateAlbedoCompareColor");

	private static readonly int k_DebugValidateMetallicMinValueId = Shader.PropertyToID("_DebugValidateMetallicMinValue");

	private static readonly int k_DebugValidateMetallicMaxValueId = Shader.PropertyToID("_DebugValidateMetallicMaxValue");

	private static readonly int k_ValidationChannelsId = Shader.PropertyToID("_ValidationChannels");

	private static readonly int k_RangeMinimumId = Shader.PropertyToID("_RangeMinimum");

	private static readonly int k_RangeMaximumId = Shader.PropertyToID("_RangeMaximum");

	private static readonly ProfilingSampler s_DebugSetupSampler = new ProfilingSampler("Setup Debug Properties");

	private static readonly ProfilingSampler s_DebugFinalValidationSampler = new ProfilingSampler("UpdateShaderGlobalPropertiesForFinalValidationPass");

	private DebugSetupPassData s_DebugSetupPassData = new DebugSetupPassData();

	private DebugFinalValidationPassData s_DebugFinalValidationPassData = new DebugFinalValidationPassData();

	private readonly Material m_ReplacementMaterial;

	private readonly Material m_HDRDebugViewMaterial;

	private HDRDebugViewPass m_HDRDebugViewPass;

	private RTHandle m_DebugScreenColorHandle;

	private RTHandle m_DebugScreenDepthHandle;

	private readonly UniversalRenderPipelineRuntimeTextures m_RuntimeTextures;

	private bool m_HasDebugRenderTarget;

	private bool m_DebugRenderTargetSupportsStereo;

	private Vector4 m_DebugRenderTargetPixelRect;

	private Vector4 m_DebugRenderTargetRangeRemap;

	private RTHandle m_DebugRenderTarget;

	private RTHandle m_DebugFontTexture;

	private GraphicsBuffer m_debugDisplayConstant;

	private readonly UniversalRenderPipelineDebugDisplaySettings m_DebugDisplaySettings;

	private DebugDisplaySettingsLighting LightingSettings => m_DebugDisplaySettings.lightingSettings;

	private DebugDisplaySettingsMaterial MaterialSettings => m_DebugDisplaySettings.materialSettings;

	private DebugDisplaySettingsRendering RenderingSettings => m_DebugDisplaySettings.renderingSettings;

	public bool AreAnySettingsActive => m_DebugDisplaySettings.AreAnySettingsActive;

	public bool IsPostProcessingAllowed => m_DebugDisplaySettings.IsPostProcessingAllowed;

	public bool IsLightingActive => m_DebugDisplaySettings.IsLightingActive;

	internal bool IsActiveModeUnsupportedForDeferred
	{
		get
		{
			if (m_DebugDisplaySettings.lightingSettings.lightingDebugMode == DebugLightingMode.None && m_DebugDisplaySettings.lightingSettings.lightingFeatureFlags == DebugLightingFeatureFlags.None && m_DebugDisplaySettings.renderingSettings.sceneOverrideMode == DebugSceneOverrideMode.None && m_DebugDisplaySettings.materialSettings.materialDebugMode == DebugMaterialMode.None && m_DebugDisplaySettings.materialSettings.vertexAttributeDebugMode == DebugVertexAttributeMode.None && m_DebugDisplaySettings.materialSettings.materialValidationMode == DebugMaterialValidationMode.None)
			{
				return m_DebugDisplaySettings.renderingSettings.mipInfoMode != DebugMipInfoMode.None;
			}
			return true;
		}
	}

	internal Material ReplacementMaterial => m_ReplacementMaterial;

	internal UniversalRenderPipelineDebugDisplaySettings DebugDisplaySettings => m_DebugDisplaySettings;

	internal ref RTHandle DebugScreenColorHandle => ref m_DebugScreenColorHandle;

	internal ref RTHandle DebugScreenDepthHandle => ref m_DebugScreenDepthHandle;

	internal HDRDebugViewPass hdrDebugViewPass => m_HDRDebugViewPass;

	internal bool IsScreenClearNeeded
	{
		get
		{
			Color color = Color.black;
			return TryGetScreenClearColor(ref color);
		}
	}

	internal bool IsRenderPassSupported
	{
		get
		{
			if (RenderingSettings.sceneOverrideMode != 0)
			{
				return RenderingSettings.sceneOverrideMode == DebugSceneOverrideMode.Overdraw;
			}
			return true;
		}
	}

	internal bool IsDepthPrimingCompatible => RenderingSettings.sceneOverrideMode != DebugSceneOverrideMode.Wireframe;

	internal int stpDebugViewIndex => RenderingSettings.stpDebugViewIndex;

	public bool TryGetScreenClearColor(ref Color color)
	{
		return m_DebugDisplaySettings.TryGetScreenClearColor(ref color);
	}

	internal bool HDRDebugViewIsActive(bool resolveFinalTarget)
	{
		return DebugDisplaySettings.lightingSettings.hdrDebugMode != HDRDebugMode.None && resolveFinalTarget;
	}

	internal bool WriteToDebugScreenTexture(bool resolveFinalTarget)
	{
		return HDRDebugViewIsActive(resolveFinalTarget);
	}

	internal DebugHandler()
	{
		m_DebugDisplaySettings = DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance;
		if (GraphicsSettings.TryGetRenderPipelineSettings<UniversalRenderPipelineDebugShaders>(out var settings))
		{
			m_ReplacementMaterial = ((settings.debugReplacementPS != null) ? CoreUtils.CreateEngineMaterial(settings.debugReplacementPS) : null);
			m_HDRDebugViewMaterial = ((settings.hdrDebugViewPS != null) ? CoreUtils.CreateEngineMaterial(settings.hdrDebugViewPS) : null);
		}
		m_HDRDebugViewPass = new HDRDebugViewPass(m_HDRDebugViewMaterial);
		m_RuntimeTextures = GraphicsSettings.GetRenderPipelineSettings<UniversalRenderPipelineRuntimeTextures>();
		if (m_RuntimeTextures != null)
		{
			m_DebugFontTexture = RTHandles.Alloc(m_RuntimeTextures.debugFontTexture);
		}
		m_debugDisplayConstant = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 32, Marshal.SizeOf(typeof(Vector4)));
	}

	public void Dispose()
	{
		m_HDRDebugViewPass.Dispose();
		m_DebugScreenColorHandle?.Release();
		m_DebugScreenDepthHandle?.Release();
		m_DebugFontTexture?.Release();
		m_debugDisplayConstant.Dispose();
		CoreUtils.Destroy(m_HDRDebugViewMaterial);
		CoreUtils.Destroy(m_ReplacementMaterial);
	}

	internal bool IsActiveForCamera(bool isPreviewCamera)
	{
		if (!isPreviewCamera)
		{
			return AreAnySettingsActive;
		}
		return false;
	}

	internal bool TryGetFullscreenDebugMode(out DebugFullScreenMode debugFullScreenMode)
	{
		int textureHeightPercent;
		return TryGetFullscreenDebugMode(out debugFullScreenMode, out textureHeightPercent);
	}

	internal bool TryGetFullscreenDebugMode(out DebugFullScreenMode debugFullScreenMode, out int textureHeightPercent)
	{
		debugFullScreenMode = RenderingSettings.fullScreenDebugMode;
		textureHeightPercent = RenderingSettings.fullScreenDebugModeOutputSizeScreenPercent;
		return debugFullScreenMode != DebugFullScreenMode.None;
	}

	internal static void ConfigureColorDescriptorForDebugScreen(ref RenderTextureDescriptor descriptor, int cameraWidth, int cameraHeight)
	{
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		descriptor.useDynamicScale = true;
		descriptor.depthStencilFormat = GraphicsFormat.None;
	}

	internal static void ConfigureDepthDescriptorForDebugScreen(ref RenderTextureDescriptor descriptor, GraphicsFormat depthStencilFormat, int cameraWidth, int cameraHeight)
	{
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		descriptor.useDynamicScale = true;
		descriptor.depthStencilFormat = depthStencilFormat;
		descriptor.graphicsFormat = GraphicsFormat.None;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void SetupShaderProperties(RasterCommandBuffer cmd, int passIndex = 0)
	{
		if (LightingSettings.lightingDebugMode == DebugLightingMode.ShadowCascades)
		{
			cmd.EnableShaderKeyword("_DEBUG_ENVIRONMENTREFLECTIONS_OFF");
		}
		else
		{
			cmd.DisableShaderKeyword("_DEBUG_ENVIRONMENTREFLECTIONS_OFF");
		}
		m_debugDisplayConstant.SetData(MaterialSettings.debugRenderingLayersColors, 0, 0, 32);
		cmd.SetGlobalConstantBuffer(m_debugDisplayConstant, "_DebugDisplayConstant", 0, m_debugDisplayConstant.count * m_debugDisplayConstant.stride);
		if (MaterialSettings.renderingLayersSelectedLight)
		{
			cmd.SetGlobalInt("_DebugRenderingLayerMask", (int)MaterialSettings.GetDebugLightLayersMask());
		}
		else
		{
			cmd.SetGlobalInt("_DebugRenderingLayerMask", (int)MaterialSettings.renderingLayerMask);
		}
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Overdraw:
		{
			float num = 1f / (float)RenderingSettings.maxOverdrawCount;
			cmd.SetGlobalColor(k_DebugColorPropertyId, new Color(num, num, num, 1f));
			break;
		}
		case DebugSceneOverrideMode.Wireframe:
			cmd.SetGlobalColor(k_DebugColorPropertyId, Color.black);
			break;
		case DebugSceneOverrideMode.SolidWireframe:
			cmd.SetGlobalColor(k_DebugColorPropertyId, (passIndex == 0) ? Color.white : Color.black);
			break;
		case DebugSceneOverrideMode.ShadedWireframe:
			switch (passIndex)
			{
			case 0:
				cmd.SetKeyword(in ShaderGlobalKeywords.DEBUG_DISPLAY, value: false);
				break;
			case 1:
				cmd.SetGlobalColor(k_DebugColorPropertyId, Color.black);
				cmd.SetKeyword(in ShaderGlobalKeywords.DEBUG_DISPLAY, value: true);
				break;
			}
			break;
		}
		switch (MaterialSettings.materialValidationMode)
		{
		case DebugMaterialValidationMode.Albedo:
			cmd.SetGlobalFloat(k_DebugValidateAlbedoMinLuminanceId, MaterialSettings.albedoMinLuminance);
			cmd.SetGlobalFloat(k_DebugValidateAlbedoMaxLuminanceId, MaterialSettings.albedoMaxLuminance);
			cmd.SetGlobalFloat(k_DebugValidateAlbedoSaturationToleranceId, MaterialSettings.albedoSaturationTolerance);
			cmd.SetGlobalFloat(k_DebugValidateAlbedoHueToleranceId, MaterialSettings.albedoHueTolerance);
			cmd.SetGlobalColor(k_DebugValidateAlbedoCompareColorId, MaterialSettings.albedoCompareColor.linear);
			break;
		case DebugMaterialValidationMode.Metallic:
			cmd.SetGlobalFloat(k_DebugValidateMetallicMinValueId, MaterialSettings.metallicMinValue);
			cmd.SetGlobalFloat(k_DebugValidateMetallicMaxValueId, MaterialSettings.metallicMaxValue);
			break;
		}
	}

	internal void SetDebugRenderTarget(RTHandle renderTarget, Rect displayRect, bool supportsStereo, Vector4 dataRangeRemap)
	{
		m_HasDebugRenderTarget = true;
		m_DebugRenderTargetSupportsStereo = supportsStereo;
		m_DebugRenderTarget = renderTarget;
		m_DebugRenderTargetPixelRect = new Vector4(displayRect.x, displayRect.y, displayRect.width, displayRect.height);
		m_DebugRenderTargetRangeRemap = dataRangeRemap;
	}

	internal void ResetDebugRenderTarget()
	{
		m_HasDebugRenderTarget = false;
	}

	private DebugFinalValidationPassData InitDebugFinalValidationPassData(DebugFinalValidationPassData passData, UniversalCameraData cameraData, bool isFinalPass)
	{
		passData.isFinalPass = isFinalPass;
		passData.resolveFinalTarget = cameraData.resolveFinalTarget;
		passData.isActiveForCamera = IsActiveForCamera(cameraData.isPreviewCamera);
		passData.hasDebugRenderTarget = m_HasDebugRenderTarget;
		passData.debugRenderTargetHandle = TextureHandle.nullHandle;
		passData.debugTexturePropertyId = (m_DebugRenderTargetSupportsStereo ? k_DebugTexturePropertyId : k_DebugTextureNoStereoPropertyId);
		passData.debugRenderTargetPixelRect = m_DebugRenderTargetPixelRect;
		passData.debugRenderTargetSupportsStereo = (m_DebugRenderTargetSupportsStereo ? 1 : 0);
		passData.debugRenderTargetRangeRemap = m_DebugRenderTargetRangeRemap;
		passData.debugFontTextureHandle = TextureHandle.nullHandle;
		passData.renderingSettings = RenderingSettings;
		return passData;
	}

	private static void UpdateShaderGlobalPropertiesForFinalValidationPass(RasterCommandBuffer cmd, DebugFinalValidationPassData data)
	{
		if (!data.isFinalPass || !data.resolveFinalTarget)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.DEBUG_DISPLAY, value: false);
			return;
		}
		if (data.isActiveForCamera)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.DEBUG_DISPLAY, value: true);
		}
		else
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.DEBUG_DISPLAY, value: false);
		}
		if (data.hasDebugRenderTarget)
		{
			if (data.debugRenderTargetHandle.IsValid())
			{
				cmd.SetGlobalTexture(data.debugTexturePropertyId, data.debugRenderTargetHandle);
			}
			cmd.SetGlobalVector(k_DebugTextureDisplayRect, data.debugRenderTargetPixelRect);
			cmd.SetGlobalInteger(k_DebugRenderTargetSupportsStereo, data.debugRenderTargetSupportsStereo);
			cmd.SetGlobalVector(k_DebugRenderTargetRangeRemap, data.debugRenderTargetRangeRemap);
		}
		DebugDisplaySettingsRendering renderingSettings = data.renderingSettings;
		if (renderingSettings.validationMode == DebugValidationMode.HighlightOutsideOfRange)
		{
			cmd.SetGlobalInteger(k_ValidationChannelsId, (int)renderingSettings.validationChannels);
			cmd.SetGlobalFloat(k_RangeMinimumId, renderingSettings.validationRangeMin);
			cmd.SetGlobalFloat(k_RangeMaximumId, renderingSettings.validationRangeMax);
		}
		if (renderingSettings.mipInfoMode != 0)
		{
			cmd.SetGlobalTexture(k_DebugFontId, data.debugFontTextureHandle);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void UpdateShaderGlobalPropertiesForFinalValidationPass(CommandBuffer cmd, UniversalCameraData cameraData, bool isFinalPass)
	{
		UpdateShaderGlobalPropertiesForFinalValidationPass(CommandBufferHelpers.GetRasterCommandBuffer(cmd), InitDebugFinalValidationPassData(s_DebugFinalValidationPassData, cameraData, isFinalPass));
		cmd.SetGlobalTexture(s_DebugFinalValidationPassData.debugTexturePropertyId, m_DebugRenderTarget);
		if (RenderingSettings.mipInfoMode != 0)
		{
			cmd.SetGlobalTexture(k_DebugFontId, m_RuntimeTextures.debugFontTexture);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void UpdateShaderGlobalPropertiesForFinalValidationPass(RenderGraph renderGraph, UniversalCameraData cameraData, bool isFinalPass)
	{
		DebugFinalValidationPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<DebugFinalValidationPassData>("UpdateShaderGlobalPropertiesForFinalValidationPass", out passData, s_DebugFinalValidationSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Debug\\DebugHandler.cs", 434);
		InitDebugFinalValidationPassData(passData, cameraData, isFinalPass);
		if (m_DebugRenderTarget != null)
		{
			passData.debugRenderTargetHandle = renderGraph.ImportTexture(m_DebugRenderTarget);
		}
		if (m_DebugFontTexture != null)
		{
			passData.debugFontTextureHandle = renderGraph.ImportTexture(m_DebugFontTexture);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		if (passData.debugRenderTargetHandle.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in passData.debugRenderTargetHandle);
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in passData.debugRenderTargetHandle, passData.debugTexturePropertyId);
		}
		if (passData.debugFontTextureHandle.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in passData.debugFontTextureHandle);
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in passData.debugFontTextureHandle, k_DebugFontId);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(DebugFinalValidationPassData data, RasterGraphContext context)
		{
			UpdateShaderGlobalPropertiesForFinalValidationPass(context.cmd, data);
		});
	}

	private DebugSetupPassData InitDebugSetupPassData(DebugSetupPassData passData, bool isPreviewCamera)
	{
		passData.isActiveForCamera = IsActiveForCamera(isPreviewCamera);
		passData.materialSettings = MaterialSettings;
		passData.renderingSettings = RenderingSettings;
		passData.lightingSettings = LightingSettings;
		return passData;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private static void Setup(RasterCommandBuffer cmd, DebugSetupPassData passData)
	{
		if (passData.isActiveForCamera)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.DEBUG_DISPLAY, value: true);
			cmd.SetGlobalFloat(k_DebugMaterialModeId, (float)passData.materialSettings.materialDebugMode);
			cmd.SetGlobalFloat(k_DebugVertexAttributeModeId, (float)passData.materialSettings.vertexAttributeDebugMode);
			cmd.SetGlobalInteger(k_DebugMaterialValidationModeId, (int)passData.materialSettings.materialValidationMode);
			cmd.SetGlobalInteger(k_DebugMipInfoModeId, (int)passData.renderingSettings.mipInfoMode);
			cmd.SetGlobalInteger(k_DebugMipMapStatusModeId, (int)passData.renderingSettings.mipDebugStatusMode);
			cmd.SetGlobalInteger(k_DebugMipMapShowStatusCodeId, passData.renderingSettings.mipDebugStatusShowCode ? 1 : 0);
			cmd.SetGlobalFloat(k_DebugMipMapOpacityId, passData.renderingSettings.mipDebugOpacity);
			cmd.SetGlobalFloat(k_DebugMipMapRecentlyUpdatedCooldownId, passData.renderingSettings.mipDebugRecentUpdateCooldown);
			cmd.SetGlobalFloat(k_DebugMipMapTerrainTextureModeId, (float)passData.renderingSettings.mipDebugTerrainTexture);
			cmd.SetGlobalInteger(k_DebugSceneOverrideModeId, (int)passData.renderingSettings.sceneOverrideMode);
			cmd.SetGlobalInteger(k_DebugFullScreenModeId, (int)passData.renderingSettings.fullScreenDebugMode);
			cmd.SetGlobalInteger(k_DebugMaxPixelCost, passData.renderingSettings.maxOverdrawCount);
			cmd.SetGlobalInteger(k_DebugValidationModeId, (int)passData.renderingSettings.validationMode);
			cmd.SetGlobalColor(k_DebugValidateBelowMinThresholdColorPropertyId, Color.red);
			cmd.SetGlobalColor(k_DebugValidateAboveMaxThresholdColorPropertyId, Color.blue);
			cmd.SetGlobalFloat(k_DebugLightingModeId, (float)passData.lightingSettings.lightingDebugMode);
			cmd.SetGlobalInteger(k_DebugLightingFeatureFlagsId, (int)passData.lightingSettings.lightingFeatureFlags);
			cmd.SetGlobalColor(k_DebugColorInvalidModePropertyId, Color.red);
			cmd.SetGlobalFloat(k_DebugCurrentRealTimeId, Time.realtimeSinceStartup);
		}
		else
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.DEBUG_DISPLAY, value: false);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void Setup(CommandBuffer cmd, bool isPreviewCamera)
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void Setup(RenderGraph renderGraph, bool isPreviewCamera)
	{
		DebugSetupPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<DebugSetupPassData>(s_DebugSetupSampler.name, out passData, s_DebugSetupSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Debug\\DebugHandler.cs", 540);
		InitDebugSetupPassData(passData, isPreviewCamera);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc<DebugSetupPassData>(delegate
		{
		});
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void Render(RenderGraph renderGraph, UniversalCameraData cameraData, TextureHandle srcColor, TextureHandle overlayTexture, TextureHandle dstColor)
	{
		if (IsActiveForCamera(cameraData.isPreviewCamera) && HDRDebugViewIsActive(cameraData.resolveFinalTarget))
		{
			m_HDRDebugViewPass.RenderHDRDebug(renderGraph, cameraData, srcColor, overlayTexture, dstColor, LightingSettings.hdrDebugMode);
		}
	}

	internal DebugRendererLists CreateRendererListsWithDebugRenderState(ScriptableRenderContext context, ref CullingResults cullResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock)
	{
		DebugRendererLists debugRendererLists = new DebugRendererLists(this, filteringSettings);
		debugRendererLists.CreateRendererListsWithDebugRenderState(context, ref cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
		return debugRendererLists;
	}

	internal DebugRendererLists CreateRendererListsWithDebugRenderState(RenderGraph renderGraph, ref CullingResults cullResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock)
	{
		DebugRendererLists debugRendererLists = new DebugRendererLists(this, filteringSettings);
		debugRendererLists.CreateRendererListsWithDebugRenderState(renderGraph, ref cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
		return debugRendererLists;
	}
}
