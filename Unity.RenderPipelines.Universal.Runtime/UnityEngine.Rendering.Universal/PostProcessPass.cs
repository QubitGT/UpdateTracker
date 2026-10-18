using System;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class PostProcessPass : ScriptableRenderPass
{
	private class MaterialLibrary
	{
		public readonly Material stopNaN;

		public readonly Material subpixelMorphologicalAntialiasing;

		public readonly Material gaussianDepthOfField;

		public readonly Material gaussianDepthOfFieldCoC;

		public readonly Material bokehDepthOfField;

		public readonly Material bokehDepthOfFieldCoC;

		public readonly Material cameraMotionBlur;

		public readonly Material paniniProjection;

		public readonly Material bloom;

		public readonly Material[] bloomUpsample;

		public readonly Material temporalAntialiasing;

		public readonly Material scalingSetup;

		public readonly Material easu;

		public readonly Material uber;

		public readonly Material finalPass;

		public readonly Material lensFlareDataDriven;

		public readonly Material lensFlareScreenSpace;

		public MaterialLibrary(PostProcessData data)
		{
			stopNaN = Load(data.shaders.stopNanPS);
			subpixelMorphologicalAntialiasing = Load(data.shaders.subpixelMorphologicalAntialiasingPS);
			gaussianDepthOfField = Load(data.shaders.gaussianDepthOfFieldPS);
			gaussianDepthOfFieldCoC = Load(data.shaders.gaussianDepthOfFieldPS);
			bokehDepthOfField = Load(data.shaders.bokehDepthOfFieldPS);
			bokehDepthOfFieldCoC = Load(data.shaders.bokehDepthOfFieldPS);
			cameraMotionBlur = Load(data.shaders.cameraMotionBlurPS);
			paniniProjection = Load(data.shaders.paniniProjectionPS);
			bloom = Load(data.shaders.bloomPS);
			temporalAntialiasing = Load(data.shaders.temporalAntialiasingPS);
			scalingSetup = Load(data.shaders.scalingSetupPS);
			easu = Load(data.shaders.easuPS);
			uber = Load(data.shaders.uberPostPS);
			finalPass = Load(data.shaders.finalPostPassPS);
			lensFlareDataDriven = Load(data.shaders.LensFlareDataDrivenPS);
			lensFlareScreenSpace = Load(data.shaders.LensFlareScreenSpacePS);
			bloomUpsample = new Material[16];
			for (uint num = 0u; num < 16; num++)
			{
				bloomUpsample[num] = Load(data.shaders.bloomPS);
			}
		}

		private Material Load(Shader shader)
		{
			if (shader == null)
			{
				Debug.LogErrorFormat("Missing shader. PostProcessing render passes will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			if (!shader.isSupported)
			{
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}

		internal void Cleanup()
		{
			CoreUtils.Destroy(stopNaN);
			CoreUtils.Destroy(subpixelMorphologicalAntialiasing);
			CoreUtils.Destroy(gaussianDepthOfField);
			CoreUtils.Destroy(gaussianDepthOfFieldCoC);
			CoreUtils.Destroy(bokehDepthOfField);
			CoreUtils.Destroy(bokehDepthOfFieldCoC);
			CoreUtils.Destroy(cameraMotionBlur);
			CoreUtils.Destroy(paniniProjection);
			CoreUtils.Destroy(bloom);
			CoreUtils.Destroy(temporalAntialiasing);
			CoreUtils.Destroy(scalingSetup);
			CoreUtils.Destroy(easu);
			CoreUtils.Destroy(uber);
			CoreUtils.Destroy(finalPass);
			CoreUtils.Destroy(lensFlareDataDriven);
			CoreUtils.Destroy(lensFlareScreenSpace);
			for (uint num = 0u; num < 16; num++)
			{
				CoreUtils.Destroy(bloomUpsample[num]);
			}
		}
	}

	private static class ShaderConstants
	{
		public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");

		public static readonly int _TempTarget2 = Shader.PropertyToID("_TempTarget2");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _StencilMask = Shader.PropertyToID("_StencilMask");

		public static readonly int _FullCoCTexture = Shader.PropertyToID("_FullCoCTexture");

		public static readonly int _HalfCoCTexture = Shader.PropertyToID("_HalfCoCTexture");

		public static readonly int _DofTexture = Shader.PropertyToID("_DofTexture");

		public static readonly int _CoCParams = Shader.PropertyToID("_CoCParams");

		public static readonly int _BokehKernel = Shader.PropertyToID("_BokehKernel");

		public static readonly int _BokehConstants = Shader.PropertyToID("_BokehConstants");

		public static readonly int _PongTexture = Shader.PropertyToID("_PongTexture");

		public static readonly int _PingTexture = Shader.PropertyToID("_PingTexture");

		public static readonly int _Metrics = Shader.PropertyToID("_Metrics");

		public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");

		public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");

		public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");

		public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

		public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");

		public static readonly int _Params = Shader.PropertyToID("_Params");

		public static readonly int _SourceTexLowMip = Shader.PropertyToID("_SourceTexLowMip");

		public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");

		public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");

		public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");

		public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");

		public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");

		public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");

		public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");

		public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");

		public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");

		public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");

		public static readonly int _Vignette_ParamsXR = Shader.PropertyToID("_Vignette_ParamsXR");

		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");

		public static readonly int _InternalLut = Shader.PropertyToID("_InternalLut");

		public static readonly int _UserLut = Shader.PropertyToID("_UserLut");

		public static readonly int _DownSampleScaleFactor = Shader.PropertyToID("_DownSampleScaleFactor");

		public static readonly int _FlareOcclusionRemapTex = Shader.PropertyToID("_FlareOcclusionRemapTex");

		public static readonly int _FlareOcclusionTex = Shader.PropertyToID("_FlareOcclusionTex");

		public static readonly int _FlareOcclusionIndex = Shader.PropertyToID("_FlareOcclusionIndex");

		public static readonly int _FlareTex = Shader.PropertyToID("_FlareTex");

		public static readonly int _FlareColorValue = Shader.PropertyToID("_FlareColorValue");

		public static readonly int _FlareData0 = Shader.PropertyToID("_FlareData0");

		public static readonly int _FlareData1 = Shader.PropertyToID("_FlareData1");

		public static readonly int _FlareData2 = Shader.PropertyToID("_FlareData2");

		public static readonly int _FlareData3 = Shader.PropertyToID("_FlareData3");

		public static readonly int _FlareData4 = Shader.PropertyToID("_FlareData4");

		public static readonly int _FlareData5 = Shader.PropertyToID("_FlareData5");

		public static readonly int _FullscreenProjMat = Shader.PropertyToID("_FullscreenProjMat");
	}

	private class UpdateCameraResolutionPassData
	{
		internal Vector2Int newCameraTargetSize;
	}

	private class StopNaNsPassData
	{
		internal TextureHandle stopNaNTarget;

		internal TextureHandle sourceTexture;

		internal Material stopNaN;
	}

	private class SMAASetupPassData
	{
		internal Vector4 metrics;

		internal Texture2D areaTexture;

		internal Texture2D searchTexture;

		internal float stencilRef;

		internal float stencilMask;

		internal AntialiasingQuality antialiasingQuality;

		internal Material material;
	}

	private class SMAAPassData
	{
		internal TextureHandle sourceTexture;

		internal TextureHandle depthStencilTexture;

		internal TextureHandle blendTexture;

		internal Material material;
	}

	private class UberSetupBloomPassData
	{
		internal Vector4 bloomParams;

		internal Vector4 dirtScaleOffset;

		internal float dirtIntensity;

		internal Texture dirtTexture;

		internal bool highQualityFilteringValue;

		internal TextureHandle bloomTexture;

		internal Material uberMaterial;
	}

	private class BloomPassData
	{
		internal int mipCount;

		internal Material material;

		internal Material[] upsampleMaterials;

		internal TextureHandle sourceTexture;

		internal TextureHandle[] bloomMipUp;

		internal TextureHandle[] bloomMipDown;
	}

	internal struct BloomMaterialParams
	{
		internal Vector4 parameters;

		internal bool highQualityFiltering;

		internal bool enableAlphaOutput;

		internal bool Equals(ref BloomMaterialParams other)
		{
			if (parameters == other.parameters && highQualityFiltering == other.highQualityFiltering)
			{
				return enableAlphaOutput == other.enableAlphaOutput;
			}
			return false;
		}
	}

	private class DoFGaussianPassData
	{
		internal int downsample;

		internal RenderingData renderingData;

		internal Vector3 cocParams;

		internal bool highQualitySamplingValue;

		internal TextureHandle sourceTexture;

		internal TextureHandle depthTexture;

		internal Material material;

		internal Material materialCoC;

		internal TextureHandle halfCoCTexture;

		internal TextureHandle fullCoCTexture;

		internal TextureHandle pingTexture;

		internal TextureHandle pongTexture;

		internal RenderTargetIdentifier[] multipleRenderTargets = new RenderTargetIdentifier[2];

		internal TextureHandle destination;
	}

	private class DoFBokehPassData
	{
		internal Vector4[] bokehKernel;

		internal int downSample;

		internal float uvMargin;

		internal Vector4 cocParams;

		internal bool useFastSRGBLinearConversion;

		internal TextureHandle sourceTexture;

		internal TextureHandle depthTexture;

		internal Material material;

		internal Material materialCoC;

		internal TextureHandle halfCoCTexture;

		internal TextureHandle fullCoCTexture;

		internal TextureHandle pingTexture;

		internal TextureHandle pongTexture;

		internal TextureHandle destination;
	}

	private class PaniniProjectionPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal Vector4 paniniParams;

		internal bool isPaniniGeneric;
	}

	private class MotionBlurPassData
	{
		internal TextureHandle sourceTexture;

		internal TextureHandle motionVectors;

		internal Material material;

		internal int passIndex;

		internal Camera camera;

		internal XRPass xr;

		internal float intensity;

		internal float clamp;

		internal bool enableAlphaOutput;
	}

	private class LensFlarePassData
	{
		internal TextureHandle destinationTexture;

		internal UniversalCameraData cameraData;

		internal Material material;

		internal Rect viewport;

		internal float paniniDistance;

		internal float paniniCropToFit;

		internal float width;

		internal float height;

		internal bool usePanini;
	}

	private class LensFlareScreenSpacePassData
	{
		internal TextureHandle streakTmpTexture;

		internal TextureHandle streakTmpTexture2;

		internal TextureHandle originalBloomTexture;

		internal TextureHandle screenSpaceLensFlareBloomMipTexture;

		internal TextureHandle result;

		internal int actualWidth;

		internal int actualHeight;

		internal Camera camera;

		internal Material material;

		internal ScreenSpaceLensFlare lensFlareScreenSpace;

		internal int downsample;
	}

	private class PostProcessingFinalSetupPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal UniversalCameraData cameraData;
	}

	private class PostProcessingFinalFSRScalePassData
	{
		internal TextureHandle sourceTexture;

		internal Material material;

		internal bool enableAlphaOutput;

		internal Vector2 fsrInputSize;

		internal Vector2 fsrOutputSize;
	}

	private class PostProcessingFinalBlitPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal UniversalCameraData cameraData;

		internal FinalBlitSettings settings;
	}

	public struct FinalBlitSettings
	{
		public bool isFxaaEnabled;

		public bool isFsrEnabled;

		public bool isTaaSharpeningEnabled;

		public bool requireHDROutput;

		public bool resolveToDebugScreen;

		public bool isAlphaOutputEnabled;

		public HDROutputUtils.Operation hdrOperations;

		public static FinalBlitSettings Create()
		{
			FinalBlitSettings result = default(FinalBlitSettings);
			result.isFxaaEnabled = false;
			result.isFsrEnabled = false;
			result.isTaaSharpeningEnabled = false;
			result.requireHDROutput = false;
			result.resolveToDebugScreen = false;
			result.isAlphaOutputEnabled = false;
			result.hdrOperations = HDROutputUtils.Operation.None;
			return result;
		}
	}

	private class UberPostPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle lutTexture;

		internal TextureHandle bloomTexture;

		internal Vector4 lutParams;

		internal TextureHandle userLutTexture;

		internal Vector4 userLutParams;

		internal Material material;

		internal UniversalCameraData cameraData;

		internal TonemappingMode toneMappingMode;

		internal bool isHdrGrading;

		internal bool isBackbuffer;

		internal bool enableAlphaOutput;

		internal bool hasFinalPass;
	}

	private class PostFXSetupPassData
	{
	}

	private RenderTextureDescriptor m_Descriptor;

	private RTHandle m_Source;

	private RTHandle m_Destination;

	private RTHandle m_Depth;

	private RTHandle m_InternalLut;

	private RTHandle m_MotionVectors;

	private RTHandle m_FullCoCTexture;

	private RTHandle m_HalfCoCTexture;

	private RTHandle m_PingTexture;

	private RTHandle m_PongTexture;

	private RTHandle[] m_BloomMipDown;

	private RTHandle[] m_BloomMipUp;

	private string[] m_BloomMipDownName;

	private string[] m_BloomMipUpName;

	private TextureHandle[] _BloomMipUp;

	private TextureHandle[] _BloomMipDown;

	private RTHandle m_BlendTexture;

	private RTHandle m_EdgeColorTexture;

	private RTHandle m_EdgeStencilTexture;

	private RTHandle m_TempTarget;

	private RTHandle m_TempTarget2;

	private RTHandle m_StreakTmpTexture;

	private RTHandle m_StreakTmpTexture2;

	private RTHandle m_ScreenSpaceLensFlareResult;

	private RTHandle m_UserLut;

	private const string k_RenderPostProcessingTag = "Blit PostProcessing Effects";

	private const string k_RenderFinalPostProcessingTag = "Blit Final PostProcessing";

	private static readonly ProfilingSampler m_ProfilingRenderPostProcessing = new ProfilingSampler("Blit PostProcessing Effects");

	private static readonly ProfilingSampler m_ProfilingRenderFinalPostProcessing = new ProfilingSampler("Blit Final PostProcessing");

	private MaterialLibrary m_Materials;

	private PostProcessData m_Data;

	private DepthOfField m_DepthOfField;

	private MotionBlur m_MotionBlur;

	private ScreenSpaceLensFlare m_LensFlareScreenSpace;

	private PaniniProjection m_PaniniProjection;

	private Bloom m_Bloom;

	private LensDistortion m_LensDistortion;

	private ChromaticAberration m_ChromaticAberration;

	private Vignette m_Vignette;

	private ColorLookup m_ColorLookup;

	private ColorAdjustments m_ColorAdjustments;

	private Tonemapping m_Tonemapping;

	private FilmGrain m_FilmGrain;

	private const int k_GaussianDoFPassComputeCoc = 0;

	private const int k_GaussianDoFPassDownscalePrefilter = 1;

	private const int k_GaussianDoFPassBlurH = 2;

	private const int k_GaussianDoFPassBlurV = 3;

	private const int k_GaussianDoFPassComposite = 4;

	private const int k_BokehDoFPassComputeCoc = 0;

	private const int k_BokehDoFPassDownscalePrefilter = 1;

	private const int k_BokehDoFPassBlur = 2;

	private const int k_BokehDoFPassPostFilter = 3;

	private const int k_BokehDoFPassComposite = 4;

	private const int k_MaxPyramidSize = 16;

	private readonly GraphicsFormat m_DefaultColorFormat;

	private bool m_DefaultColorFormatIsAlpha;

	private readonly GraphicsFormat m_SMAAEdgeFormat;

	private readonly GraphicsFormat m_GaussianCoCFormat;

	private int m_DitheringTextureIndex;

	private RenderTargetIdentifier[] m_MRT2;

	private Vector4[] m_BokehKernel;

	private int m_BokehHash;

	private float m_BokehMaxRadius;

	private float m_BokehRCPAspect;

	private bool m_IsFinalPass;

	private bool m_HasFinalPass;

	private bool m_EnableColorEncodingIfNeeded;

	private bool m_UseFastSRGBLinearConversion;

	private bool m_SupportScreenSpaceLensFlare;

	private bool m_SupportDataDrivenLensFlare;

	private bool m_ResolveToScreen;

	private bool m_UseSwapBuffer;

	private RTHandle m_ScalingSetupTarget;

	private RTHandle m_UpscaledTarget;

	private Material m_BlitMaterial;

	private BloomMaterialParams m_BloomParamsPrev;

	internal static readonly int k_ShaderPropertyId_ViewProjM = Shader.PropertyToID("_ViewProjM");

	internal static readonly int k_ShaderPropertyId_PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");

	internal static readonly int k_ShaderPropertyId_ViewProjMStereo = Shader.PropertyToID("_ViewProjMStereo");

	internal static readonly int k_ShaderPropertyId_PrevViewProjMStereo = Shader.PropertyToID("_PrevViewProjMStereo");

	private static readonly int s_CameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	private const string _TemporalAATargetName = "_TemporalAATarget";

	private const string _UpscaledColorTargetName = "_CameraColorUpscaledSTP";

	public PostProcessPass(RenderPassEvent evt, PostProcessData data, ref PostProcessParams postProcessParams)
	{
		base.profilingSampler = new ProfilingSampler("PostProcessPass");
		base.renderPassEvent = evt;
		m_Data = data;
		m_Materials = new MaterialLibrary(data);
		m_BloomMipUp = new RTHandle[16];
		m_BloomMipDown = new RTHandle[16];
		m_BloomMipDownName = new string[16];
		m_BloomMipUpName = new string[16];
		_BloomMipUp = new TextureHandle[16];
		_BloomMipDown = new TextureHandle[16];
		for (int i = 0; i < 16; i++)
		{
			m_BloomMipUpName[i] = "_BloomMipUp" + i;
			m_BloomMipDownName[i] = "_BloomMipDown" + i;
		}
		m_MRT2 = new RenderTargetIdentifier[2];
		base.useNativeRenderPass = false;
		m_BlitMaterial = postProcessParams.blitMaterial;
		bool num = IsHDRFormat(postProcessParams.requestColorFormat);
		bool defaultColorFormatIsAlpha = IsAlphaFormat(postProcessParams.requestColorFormat);
		if (num)
		{
			m_DefaultColorFormatIsAlpha = defaultColorFormatIsAlpha;
			if (SystemInfo.IsFormatSupported(postProcessParams.requestColorFormat, GraphicsFormatUsage.Blend))
			{
				m_DefaultColorFormat = postProcessParams.requestColorFormat;
			}
			else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
			{
				m_DefaultColorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
				m_DefaultColorFormatIsAlpha = false;
			}
			else
			{
				m_DefaultColorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			}
		}
		else
		{
			m_DefaultColorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			m_DefaultColorFormatIsAlpha = true;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_UNorm, GraphicsFormatUsage.Render) && SystemInfo.graphicsDeviceVendor.ToLowerInvariant().Contains("arm"))
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8_UNorm;
		}
		else
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, GraphicsFormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_UNorm;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, GraphicsFormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_SFloat;
		}
		else
		{
			m_GaussianCoCFormat = GraphicsFormat.R8_UNorm;
		}
	}

	public void Cleanup()
	{
		m_Materials.Cleanup();
		Dispose();
	}

	public void Dispose()
	{
		RTHandle[] bloomMipDown = m_BloomMipDown;
		for (int i = 0; i < bloomMipDown.Length; i++)
		{
			bloomMipDown[i]?.Release();
		}
		bloomMipDown = m_BloomMipUp;
		for (int i = 0; i < bloomMipDown.Length; i++)
		{
			bloomMipDown[i]?.Release();
		}
		m_ScalingSetupTarget?.Release();
		m_UpscaledTarget?.Release();
		m_FullCoCTexture?.Release();
		m_HalfCoCTexture?.Release();
		m_PingTexture?.Release();
		m_PongTexture?.Release();
		m_BlendTexture?.Release();
		m_EdgeColorTexture?.Release();
		m_EdgeStencilTexture?.Release();
		m_TempTarget?.Release();
		m_TempTarget2?.Release();
		m_StreakTmpTexture?.Release();
		m_StreakTmpTexture2?.Release();
		m_ScreenSpaceLensFlareResult?.Release();
		m_UserLut?.Release();
	}

	public void Setup(in RenderTextureDescriptor baseDescriptor, in RTHandle source, bool resolveToScreen, in RTHandle depth, in RTHandle internalLut, in RTHandle motionVectors, bool hasFinalPass, bool enableColorEncoding)
	{
		m_Descriptor = baseDescriptor;
		m_Descriptor.useMipMap = false;
		m_Descriptor.autoGenerateMips = false;
		m_Source = source;
		m_Depth = depth;
		m_InternalLut = internalLut;
		m_MotionVectors = motionVectors;
		m_IsFinalPass = false;
		m_HasFinalPass = hasFinalPass;
		m_EnableColorEncodingIfNeeded = enableColorEncoding;
		m_ResolveToScreen = resolveToScreen;
		m_UseSwapBuffer = true;
		m_Destination = ScriptableRenderPass.k_CameraTarget;
	}

	public void SetupFinalPass(in RTHandle source, bool useSwapBuffer = false, bool enableColorEncoding = true)
	{
		m_Source = source;
		m_IsFinalPass = true;
		m_HasFinalPass = false;
		m_EnableColorEncodingIfNeeded = enableColorEncoding;
		m_UseSwapBuffer = useSwapBuffer;
		m_Destination = ScriptableRenderPass.k_CameraTarget;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		base.overrideCameraTarget = true;
	}

	public bool CanRunOnTile()
	{
		return false;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_DepthOfField = stack.GetComponent<DepthOfField>();
		m_MotionBlur = stack.GetComponent<MotionBlur>();
		m_LensFlareScreenSpace = stack.GetComponent<ScreenSpaceLensFlare>();
		m_PaniniProjection = stack.GetComponent<PaniniProjection>();
		m_Bloom = stack.GetComponent<Bloom>();
		m_LensDistortion = stack.GetComponent<LensDistortion>();
		m_ChromaticAberration = stack.GetComponent<ChromaticAberration>();
		m_Vignette = stack.GetComponent<Vignette>();
		m_ColorLookup = stack.GetComponent<ColorLookup>();
		m_ColorAdjustments = stack.GetComponent<ColorAdjustments>();
		m_Tonemapping = stack.GetComponent<Tonemapping>();
		m_FilmGrain = stack.GetComponent<FilmGrain>();
		m_UseFastSRGBLinearConversion = renderingData.postProcessingData.useFastSRGBLinearConversion;
		m_SupportScreenSpaceLensFlare = renderingData.postProcessingData.supportScreenSpaceLensFlare;
		m_SupportDataDrivenLensFlare = renderingData.postProcessingData.supportDataDrivenLensFlare;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		if (m_IsFinalPass)
		{
			using (new ProfilingScope(commandBuffer, m_ProfilingRenderFinalPostProcessing))
			{
				RenderFinalPass(commandBuffer, ref renderingData);
				return;
			}
		}
		if (!CanRunOnTile())
		{
			using (new ProfilingScope(commandBuffer, m_ProfilingRenderPostProcessing))
			{
				Render(commandBuffer, ref renderingData);
			}
		}
	}

	private bool IsHDRFormat(GraphicsFormat format)
	{
		if (format != GraphicsFormat.B10G11R11_UFloatPack32 && !GraphicsFormatUtility.IsHalfFormat(format))
		{
			return GraphicsFormatUtility.IsFloatFormat(format);
		}
		return true;
	}

	private bool IsAlphaFormat(GraphicsFormat format)
	{
		return GraphicsFormatUtility.HasAlphaChannel(format);
	}

	private RenderTextureDescriptor GetCompatibleDescriptor()
	{
		return GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
	}

	private RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, GraphicsFormat depthStencilFormat = GraphicsFormat.None)
	{
		return GetCompatibleDescriptor(m_Descriptor, width, height, format, depthStencilFormat);
	}

	internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, GraphicsFormat depthStencilFormat = GraphicsFormat.None)
	{
		desc.depthStencilFormat = depthStencilFormat;
		desc.msaaSamples = 1;
		desc.width = width;
		desc.height = height;
		desc.graphicsFormat = format;
		return desc;
	}

	private bool RequireSRGBConversionBlitToBackBuffer(bool requireSrgbConversion)
	{
		if (requireSrgbConversion)
		{
			return m_EnableColorEncodingIfNeeded;
		}
		return false;
	}

	private bool RequireHDROutput(UniversalCameraData cameraData)
	{
		if (cameraData.isHDROutputActive)
		{
			return cameraData.captureActions == null;
		}
		return false;
	}

	private void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
		ref ScriptableRenderer renderer = ref cameraData.renderer;
		bool isSceneViewCamera = cameraData.isSceneViewCamera;
		bool flag = cameraData.isStopNaNEnabled && m_Materials.stopNaN != null;
		bool flag2 = cameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing;
		Material material = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? m_Materials.gaussianDepthOfField : m_Materials.bokehDepthOfField);
		bool flag3 = m_DepthOfField.IsActive() && !isSceneViewCamera && material != null;
		bool flag4 = !LensFlareCommonSRP.Instance.IsEmpty() && m_SupportDataDrivenLensFlare;
		bool flag5 = m_LensFlareScreenSpace.IsActive() && m_SupportScreenSpaceLensFlare;
		bool flag6 = m_MotionBlur.IsActive() && !isSceneViewCamera;
		bool flag7 = m_PaniniProjection.IsActive() && !isSceneViewCamera;
		flag6 = flag6 && Application.isPlaying;
		bool flag8 = cameraData.IsTemporalAAEnabled();
		if (cameraData.IsTemporalAARequested() && !flag8)
		{
			TemporalAA.ValidateAndWarn(cameraData);
		}
		int amountOfPassesRemaining = (flag ? 1 : 0) + (flag2 ? 1 : 0) + (flag3 ? 1 : 0) + (flag4 ? 1 : 0) + (flag8 ? 1 : 0) + (flag6 ? 1 : 0) + (flag7 ? 1 : 0);
		if (m_UseSwapBuffer && amountOfPassesRemaining > 0)
		{
			renderer.EnableSwapBufferMSAA(enable: false);
		}
		RTHandle source = (m_UseSwapBuffer ? renderer.cameraColorTargetHandle : m_Source);
		RTHandle destination = (m_UseSwapBuffer ? renderer.GetCameraColorFrontBuffer(cmd) : null);
		cmd.SetGlobalMatrix(ShaderConstants._FullscreenProjMat, GL.GetGPUProjectionMatrix(Matrix4x4.identity, renderIntoTexture: true));
		if (flag)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.StopNaNs)))
			{
				Blitter.BlitCameraTexture(cmd, GetSource(), GetDestination(), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_Materials.stopNaN, 0);
				Swap(ref renderer);
			}
		}
		if (flag2)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.SMAA)))
			{
				DoSubpixelMorphologicalAntialiasing(ref renderingData.cameraData, cmd, GetSource(), GetDestination());
				Swap(ref renderer);
			}
		}
		if (flag3)
		{
			URPProfileId marker = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? URPProfileId.GaussianDepthOfField : URPProfileId.BokehDepthOfField);
			using (new ProfilingScope(cmd, ProfilingSampler.Get(marker)))
			{
				DoDepthOfField(ref renderingData.cameraData, cmd, GetSource(), GetDestination(), cameraData.pixelRect);
				Swap(ref renderer);
			}
		}
		if (flag8)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.TemporalAA)))
			{
				TemporalAA.ExecutePass(cmd, m_Materials.temporalAntialiasing, ref renderingData.cameraData, source, destination, m_MotionVectors?.rt);
				Swap(ref renderer);
			}
		}
		if (flag6)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.MotionBlur)))
			{
				DoMotionBlur(cmd, GetSource(), GetDestination(), m_MotionVectors, ref renderingData.cameraData);
				Swap(ref renderer);
			}
		}
		if (flag7)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.PaniniProjection)))
			{
				DoPaniniProjection(cameraData.camera, cmd, GetSource(), GetDestination());
				Swap(ref renderer);
			}
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.UberPostProcess)))
		{
			m_Materials.uber.shaderKeywords = null;
			bool num = m_Bloom.IsActive();
			bool flag9 = m_LensFlareScreenSpace.IsActive();
			if (num || flag9)
			{
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.Bloom)))
				{
					SetupBloom(cmd, GetSource(), m_Materials.uber, cameraData.isAlphaOutputEnabled);
				}
			}
			if (flag5)
			{
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.LensFlareScreenSpace)))
				{
					int num2 = Mathf.Clamp(m_LensFlareScreenSpace.bloomMip.value, 0, m_Bloom.maxIterations.value / 2);
					DoLensFlareScreenSpace(cameraData.camera, cmd, GetSource(), m_BloomMipUp[0], m_BloomMipUp[num2]);
				}
			}
			if (flag4)
			{
				bool usePanini;
				float paniniDistance;
				float paniniCropToFit;
				if (m_PaniniProjection.IsActive())
				{
					usePanini = true;
					paniniDistance = m_PaniniProjection.distance.value;
					paniniCropToFit = m_PaniniProjection.cropToFit.value;
				}
				else
				{
					usePanini = false;
					paniniDistance = 1f;
					paniniCropToFit = 1f;
				}
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.LensFlareDataDrivenComputeOcclusion)))
				{
					LensFlareDataDrivenComputeOcclusion(ref cameraData, cmd, GetSource(), usePanini, paniniDistance, paniniCropToFit);
				}
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.LensFlareDataDriven)))
				{
					LensFlareDataDriven(ref cameraData, cmd, GetSource(), usePanini, paniniDistance, paniniCropToFit);
				}
			}
			SetupLensDistortion(m_Materials.uber, isSceneViewCamera);
			SetupChromaticAberration(m_Materials.uber);
			SetupVignette(m_Materials.uber, cameraData.xr, m_Descriptor.width, m_Descriptor.height);
			SetupColorGrading(cmd, ref renderingData, m_Materials.uber);
			SetupGrain(cameraData, m_Materials.uber);
			SetupDithering(cameraData, m_Materials.uber);
			if (RequireSRGBConversionBlitToBackBuffer(cameraData.requireSrgbConversion))
			{
				m_Materials.uber.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
			}
			if (RequireHDROutput(cameraData))
			{
				HDROutputUtils.Operation hdrOperations = ((!m_HasFinalPass && m_EnableColorEncodingIfNeeded) ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
				SetupHDROutput(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, m_Materials.uber, hdrOperations, cameraData.rendersOverlayUI);
			}
			if (m_UseFastSRGBLinearConversion)
			{
				m_Materials.uber.EnableKeyword("_USE_FAST_SRGB_LINEAR_CONVERSION");
			}
			CoreUtils.SetKeyword(m_Materials.uber, "_ENABLE_ALPHA_OUTPUT", cameraData.isAlphaOutputEnabled);
			DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(cameraData);
			bool flag10 = activeDebugHandler?.WriteToDebugScreenTexture(cameraData.resolveFinalTarget) ?? false;
			RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
			if (m_Destination == ScriptableRenderPass.k_CameraTarget && !cameraData.isDefaultViewport)
			{
				loadAction = RenderBufferLoadAction.Load;
			}
			RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
			if (cameraData.xr.enabled)
			{
				renderTargetIdentifier = cameraData.xr.renderTarget;
			}
			if (!m_UseSwapBuffer)
			{
				m_ResolveToScreen = cameraData.resolveFinalTarget || m_Destination.nameID == renderTargetIdentifier || m_HasFinalPass;
			}
			if (m_UseSwapBuffer && !m_ResolveToScreen)
			{
				if (!m_HasFinalPass)
				{
					renderer.EnableSwapBufferMSAA(enable: true);
					destination = renderer.GetCameraColorFrontBuffer(cmd);
				}
				Blitter.BlitCameraTexture(cmd, GetSource(), destination, loadAction, RenderBufferStoreAction.Store, m_Materials.uber, 0);
				renderer.ConfigureCameraColorTarget(destination);
				Swap(ref renderer);
			}
			else if (!m_UseSwapBuffer)
			{
				RTHandle source2 = GetSource();
				Blitter.BlitCameraTexture(cmd, source2, GetDestination(), loadAction, RenderBufferStoreAction.Store, m_Materials.uber, 0);
				CommandBuffer cmd2 = cmd;
				RTHandle source3 = GetDestination();
				RTHandle destination2 = m_Destination;
				Material blitMaterial = m_BlitMaterial;
				RenderTexture rt = m_Destination.rt;
				Blitter.BlitCameraTexture(cmd2, source3, destination2, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, blitMaterial, ((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? 1 : 0);
			}
			else if (m_ResolveToScreen)
			{
				if (flag10)
				{
					Blitter.BlitCameraTexture(cmd, GetSource(), activeDebugHandler.DebugScreenColorHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, m_Materials.uber, 0);
					renderer.ConfigureCameraTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
					return;
				}
				RTHandleStaticHelpers.SetRTHandleStaticWrapper((cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : renderTargetIdentifier);
				RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
				RenderingUtils.FinalBlit(cmd, cameraData, GetSource(), s_RTHandleWrapper, loadAction, RenderBufferStoreAction.Store, m_Materials.uber, 0);
				renderer.ConfigureCameraColorTarget(s_RTHandleWrapper);
			}
		}
		RTHandle GetDestination()
		{
			if (destination == null)
			{
				ref RTHandle tempTarget = ref m_TempTarget;
				RenderTextureDescriptor descriptor = GetCompatibleDescriptor();
				RenderingUtils.ReAllocateHandleIfNeeded(ref tempTarget, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_TempTarget");
				destination = m_TempTarget;
			}
			else if (destination == m_Source && m_Descriptor.msaaSamples > 1)
			{
				ref RTHandle tempTarget2 = ref m_TempTarget2;
				RenderTextureDescriptor descriptor = GetCompatibleDescriptor();
				RenderingUtils.ReAllocateHandleIfNeeded(ref tempTarget2, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_TempTarget2");
				destination = m_TempTarget2;
			}
			return destination;
		}
		RTHandle GetSource()
		{
			return source;
		}
		void Swap(ref ScriptableRenderer r)
		{
			int num3 = amountOfPassesRemaining - 1;
			amountOfPassesRemaining = num3;
			if (m_UseSwapBuffer)
			{
				r.SwapColorBuffer(cmd);
				source = r.cameraColorTargetHandle;
				if (amountOfPassesRemaining == 0 && !m_HasFinalPass)
				{
					r.EnableSwapBufferMSAA(enable: true);
				}
				destination = r.GetCameraColorFrontBuffer(cmd);
			}
			else
			{
				CoreUtils.Swap(ref source, ref destination);
			}
		}
	}

	private void DoSubpixelMorphologicalAntialiasing(ref CameraData cameraData, CommandBuffer cmd, RTHandle source, RTHandle destination)
	{
		Rect viewport = new Rect(Vector2.zero, new Vector2(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height));
		Material subpixelMorphologicalAntialiasing = m_Materials.subpixelMorphologicalAntialiasing;
		ref RTHandle edgeStencilTexture = ref m_EdgeStencilTexture;
		RenderTextureDescriptor descriptor = GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.None, GraphicsFormatUtility.GetDepthStencilFormat(24));
		RenderingUtils.ReAllocateHandleIfNeeded(ref edgeStencilTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_EdgeStencilTexture");
		ref RTHandle edgeColorTexture = ref m_EdgeColorTexture;
		descriptor = GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_SMAAEdgeFormat);
		RenderingUtils.ReAllocateHandleIfNeeded(ref edgeColorTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_EdgeColorTexture");
		ref RTHandle blendTexture = ref m_BlendTexture;
		descriptor = GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8G8B8A8_UNorm);
		RenderingUtils.ReAllocateHandleIfNeeded(ref blendTexture, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, 1, 0f, "_BlendTexture");
		Vector2Int vector2Int = (m_EdgeColorTexture.useScaling ? m_EdgeColorTexture.rtHandleProperties.currentRenderTargetSize : new Vector2Int(m_EdgeColorTexture.rt.width, m_EdgeColorTexture.rt.height));
		subpixelMorphologicalAntialiasing.SetVector(ShaderConstants._Metrics, new Vector4(1f / (float)vector2Int.x, 1f / (float)vector2Int.y, vector2Int.x, vector2Int.y));
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._AreaTexture, m_Data.textures.smaaAreaTex);
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._SearchTexture, m_Data.textures.smaaSearchTex);
		subpixelMorphologicalAntialiasing.SetFloat(ShaderConstants._StencilRef, 64f);
		subpixelMorphologicalAntialiasing.SetFloat(ShaderConstants._StencilMask, 64f);
		subpixelMorphologicalAntialiasing.shaderKeywords = null;
		switch (cameraData.antialiasingQuality)
		{
		case AntialiasingQuality.Low:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_LOW");
			break;
		case AntialiasingQuality.Medium:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_MEDIUM");
			break;
		case AntialiasingQuality.High:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_HIGH");
			break;
		}
		RenderingUtils.Blit(cmd, source, viewport, m_EdgeColorTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_EdgeStencilTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.ColorStencil, Color.clear, subpixelMorphologicalAntialiasing);
		RenderingUtils.Blit(cmd, m_EdgeColorTexture, viewport, m_BlendTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_EdgeStencilTexture, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare, ClearFlag.Color, Color.clear, subpixelMorphologicalAntialiasing, 1);
		cmd.SetGlobalTexture(ShaderConstants._BlendTexture, m_BlendTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, subpixelMorphologicalAntialiasing, 2);
	}

	private void DoDepthOfField(ref CameraData cameraData, CommandBuffer cmd, RTHandle source, RTHandle destination, Rect pixelRect)
	{
		if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
		{
			DoGaussianDepthOfField(cmd, source, destination, pixelRect, cameraData.isAlphaOutputEnabled);
		}
		else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
		{
			DoBokehDepthOfField(cmd, source, destination, pixelRect, cameraData.isAlphaOutputEnabled);
		}
	}

	private void DoGaussianDepthOfField(CommandBuffer cmd, RTHandle source, RTHandle destination, Rect pixelRect, bool enableAlphaOutput)
	{
		int num = 2;
		Material gaussianDepthOfField = m_Materials.gaussianDepthOfField;
		int num2 = m_Descriptor.width / num;
		int height = m_Descriptor.height / num;
		float value = m_DepthOfField.gaussianStart.value;
		float y = Mathf.Max(value, m_DepthOfField.gaussianEnd.value);
		float a = m_DepthOfField.gaussianMaxRadius.value * ((float)num2 / 1080f);
		a = Mathf.Min(a, 2f);
		CoreUtils.SetKeyword(gaussianDepthOfField, "_ENABLE_ALPHA_OUTPUT", enableAlphaOutput);
		CoreUtils.SetKeyword(gaussianDepthOfField, "_HIGH_QUALITY_SAMPLING", m_DepthOfField.highQualitySampling.value);
		gaussianDepthOfField.SetVector(ShaderConstants._CoCParams, new Vector3(value, y, a));
		ref RTHandle fullCoCTexture = ref m_FullCoCTexture;
		RenderTextureDescriptor descriptor = GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_GaussianCoCFormat);
		RenderingUtils.ReAllocateHandleIfNeeded(ref fullCoCTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_FullCoCTexture");
		ref RTHandle halfCoCTexture = ref m_HalfCoCTexture;
		descriptor = GetCompatibleDescriptor(num2, height, m_GaussianCoCFormat);
		RenderingUtils.ReAllocateHandleIfNeeded(ref halfCoCTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_HalfCoCTexture");
		ref RTHandle pingTexture = ref m_PingTexture;
		descriptor = GetCompatibleDescriptor(num2, height, GraphicsFormat.R16G16B16A16_SFloat);
		RenderingUtils.ReAllocateHandleIfNeeded(ref pingTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_PingTexture");
		ref RTHandle pongTexture = ref m_PongTexture;
		descriptor = GetCompatibleDescriptor(num2, height, GraphicsFormat.R16G16B16A16_SFloat);
		RenderingUtils.ReAllocateHandleIfNeeded(ref pongTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_PongTexture");
		PostProcessUtils.SetSourceSize(cmd, m_FullCoCTexture);
		cmd.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)num, 1f / (float)num, num, num));
		Blitter.BlitCameraTexture(cmd, source, m_FullCoCTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 0);
		m_MRT2[0] = m_HalfCoCTexture.nameID;
		m_MRT2[1] = m_PingTexture.nameID;
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, m_FullCoCTexture.nameID);
		CoreUtils.SetRenderTarget(cmd, m_MRT2, m_HalfCoCTexture);
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		Blitter.BlitTexture(cmd, source, vector, gaussianDepthOfField, 1);
		cmd.SetGlobalTexture(ShaderConstants._HalfCoCTexture, m_HalfCoCTexture.nameID);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		Blitter.BlitCameraTexture(cmd, m_PingTexture, m_PongTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 2);
		Blitter.BlitCameraTexture(cmd, m_PongTexture, m_PingTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, m_PingTexture.nameID);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, m_FullCoCTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 4);
	}

	private void PrepareBokehKernel(float maxRadius, float rcpAspect)
	{
		if (m_BokehKernel == null)
		{
			m_BokehKernel = new Vector4[42];
		}
		int num = 0;
		float num2 = m_DepthOfField.bladeCount.value;
		float p = 1f - m_DepthOfField.bladeCurvature.value;
		float num3 = m_DepthOfField.bladeRotation.value * (MathF.PI / 180f);
		for (int i = 1; i < 4; i++)
		{
			float num4 = 1f / 7f;
			float num5 = ((float)i + num4) / (3f + num4);
			int num6 = i * 7;
			for (int j = 0; j < num6; j++)
			{
				float num7 = MathF.PI * 2f * (float)j / (float)num6;
				float num8 = Mathf.Cos(MathF.PI / num2);
				float num9 = Mathf.Cos(num7 - MathF.PI * 2f / num2 * Mathf.Floor((num2 * num7 + MathF.PI) / (MathF.PI * 2f)));
				float num10 = num5 * Mathf.Pow(num8 / num9, p);
				float num11 = num10 * Mathf.Cos(num7 - num3);
				float num12 = num10 * Mathf.Sin(num7 - num3);
				float num13 = num11 * maxRadius;
				float num14 = num12 * maxRadius;
				float num15 = num13 * num13;
				float num16 = num14 * num14;
				float z = Mathf.Sqrt(num15 + num16);
				float w = num13 * rcpAspect;
				m_BokehKernel[num] = new Vector4(num13, num14, z, w);
				num++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetMaxBokehRadiusInPixels(float viewportHeight)
	{
		return Mathf.Min(0.05f, 14f / viewportHeight);
	}

	private void DoBokehDepthOfField(CommandBuffer cmd, RTHandle source, RTHandle destination, Rect pixelRect, bool enableAlphaOutput)
	{
		int num = 2;
		Material bokehDepthOfField = m_Materials.bokehDepthOfField;
		int num2 = m_Descriptor.width / num;
		int num3 = m_Descriptor.height / num;
		float num4 = m_DepthOfField.focalLength.value / 1000f;
		float num5 = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
		float value = m_DepthOfField.focusDistance.value;
		float y = num5 * num4 / (value - num4);
		float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(m_Descriptor.height);
		float num6 = 1f / ((float)num2 / (float)num3);
		CoreUtils.SetKeyword(bokehDepthOfField, "_ENABLE_ALPHA_OUTPUT", enableAlphaOutput);
		CoreUtils.SetKeyword(bokehDepthOfField, "_USE_FAST_SRGB_LINEAR_CONVERSION", m_UseFastSRGBLinearConversion);
		cmd.SetGlobalVector(ShaderConstants._CoCParams, new Vector4(value, y, maxBokehRadiusInPixels, num6));
		int hashCode = m_DepthOfField.GetHashCode();
		if (hashCode != m_BokehHash || maxBokehRadiusInPixels != m_BokehMaxRadius || num6 != m_BokehRCPAspect)
		{
			m_BokehHash = hashCode;
			m_BokehMaxRadius = maxBokehRadiusInPixels;
			m_BokehRCPAspect = num6;
			PrepareBokehKernel(maxBokehRadiusInPixels, num6);
		}
		cmd.SetGlobalVectorArray(ShaderConstants._BokehKernel, m_BokehKernel);
		ref RTHandle fullCoCTexture = ref m_FullCoCTexture;
		RenderTextureDescriptor descriptor = GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8_UNorm);
		RenderingUtils.ReAllocateHandleIfNeeded(ref fullCoCTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_FullCoCTexture");
		ref RTHandle pingTexture = ref m_PingTexture;
		descriptor = GetCompatibleDescriptor(num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		RenderingUtils.ReAllocateHandleIfNeeded(ref pingTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_PingTexture");
		ref RTHandle pongTexture = ref m_PongTexture;
		descriptor = GetCompatibleDescriptor(num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		RenderingUtils.ReAllocateHandleIfNeeded(ref pongTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_PongTexture");
		PostProcessUtils.SetSourceSize(cmd, m_FullCoCTexture);
		cmd.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)num, 1f / (float)num, num, num));
		float num7 = 1f / (float)m_Descriptor.height * (float)num;
		cmd.SetGlobalVector(ShaderConstants._BokehConstants, new Vector4(num7, num7 * 2f));
		Blitter.BlitCameraTexture(cmd, source, m_FullCoCTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 0);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, m_FullCoCTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, m_PingTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 1);
		Blitter.BlitCameraTexture(cmd, m_PingTexture, m_PongTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 2);
		Blitter.BlitCameraTexture(cmd, m_PongTexture, m_PingTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._DofTexture, m_PingTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 4);
	}

	private static float GetLensFlareLightAttenuation(Light light, Camera cam, Vector3 wo)
	{
		if (light != null)
		{
			return light.type switch
			{
				LightType.Directional => LensFlareCommonSRP.ShapeAttenuationDirLight(light.transform.forward, cam.transform.forward), 
				LightType.Point => LensFlareCommonSRP.ShapeAttenuationPointLight(), 
				LightType.Spot => LensFlareCommonSRP.ShapeAttenuationSpotConeLight(light.transform.forward, wo, light.spotAngle, light.innerSpotAngle / 180f), 
				_ => 1f, 
			};
		}
		return 1f;
	}

	private void LensFlareDataDrivenComputeOcclusion(ref UniversalCameraData cameraData, CommandBuffer cmd, RenderTargetIdentifier source, bool usePanini, float paniniDistance, float paniniCropToFit)
	{
		if (!LensFlareCommonSRP.IsOcclusionRTCompatible())
		{
			return;
		}
		Camera camera = cameraData.camera;
		Matrix4x4 viewProjMatrix;
		if (cameraData.xr.enabled)
		{
			if (cameraData.xr.singlePassEnabled)
			{
				viewProjMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true) * cameraData.GetViewMatrix();
			}
			else
			{
				viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
				_ = cameraData.xr.multipassId;
			}
		}
		else
		{
			viewProjMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true) * cameraData.GetViewMatrix();
		}
		cmd.SetGlobalTexture(m_Depth.name, m_Depth.nameID);
		LensFlareCommonSRP.ComputeOcclusion(m_Materials.lensFlareDataDriven, camera, cameraData.xr, cameraData.xr.multipassId, m_Descriptor.width, m_Descriptor.height, usePanini, paniniDistance, paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, cmd, taaEnabled: false, hasCloudLayer: false, null, null);
		if (cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
		{
			for (int i = 1; i < cameraData.xr.viewCount; i++)
			{
				Matrix4x4 viewProjMatrix2 = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(i), renderIntoTexture: true) * cameraData.GetViewMatrix(i);
				cmd.SetGlobalTexture(m_Depth.name, m_Depth.nameID);
				LensFlareCommonSRP.ComputeOcclusion(m_Materials.lensFlareDataDriven, camera, cameraData.xr, i, m_Descriptor.width, m_Descriptor.height, usePanini, paniniDistance, paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix2, cmd, taaEnabled: false, hasCloudLayer: false, null, null);
			}
		}
	}

	private void LensFlareDataDriven(ref UniversalCameraData cameraData, CommandBuffer cmd, RenderTargetIdentifier source, bool usePanini, float paniniDistance, float paniniCropToFit)
	{
		Camera camera = cameraData.camera;
		Rect viewport = new Rect(Vector2.zero, new Vector2(m_Descriptor.width, m_Descriptor.height));
		if (!cameraData.xr.enabled || (cameraData.xr.enabled && !cameraData.xr.singlePassEnabled))
		{
			Matrix4x4 viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
			LensFlareCommonSRP.DoLensFlareDataDrivenCommon(m_Materials.lensFlareDataDriven, camera, viewport, cameraData.xr, cameraData.xr.multipassId, m_Descriptor.width, m_Descriptor.height, usePanini, paniniDistance, paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, cmd, taaEnabled: false, hasCloudLayer: false, null, null, source, (Light light, Camera cam, Vector3 wo) => GetLensFlareLightAttenuation(light, cam, wo), debugView: false);
			return;
		}
		for (int i = 0; i < cameraData.xr.viewCount; i++)
		{
			Matrix4x4 viewProjMatrix2 = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(i), renderIntoTexture: true) * cameraData.GetViewMatrix(i);
			LensFlareCommonSRP.DoLensFlareDataDrivenCommon(m_Materials.lensFlareDataDriven, camera, viewport, cameraData.xr, cameraData.xr.multipassId, m_Descriptor.width, m_Descriptor.height, usePanini, paniniDistance, paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix2, cmd, taaEnabled: false, hasCloudLayer: false, null, null, source, (Light light, Camera cam, Vector3 wo) => GetLensFlareLightAttenuation(light, cam, wo), debugView: false);
		}
	}

	private void DoLensFlareScreenSpace(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, RTHandle originalBloomTexture, RTHandle screenSpaceLensFlareBloomMipTexture)
	{
		int value = (int)m_LensFlareScreenSpace.resolution.value;
		int width = Mathf.Max(1, m_Descriptor.width / value);
		int height = Mathf.Max(1, m_Descriptor.height / value);
		RenderTextureDescriptor descriptor = GetCompatibleDescriptor(width, height, m_DefaultColorFormat);
		if (m_LensFlareScreenSpace.IsStreaksActive())
		{
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_StreakTmpTexture, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_StreakTmpTexture");
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_StreakTmpTexture2, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_StreakTmpTexture2");
		}
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_ScreenSpaceLensFlareResult, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_ScreenSpaceLensFlareResult");
		LensFlareCommonSRP.DoLensFlareScreenSpaceCommon(m_Materials.lensFlareScreenSpace, camera, m_Descriptor.width, m_Descriptor.height, m_LensFlareScreenSpace.tintColor.value, originalBloomTexture, screenSpaceLensFlareBloomMipTexture, null, m_StreakTmpTexture, m_StreakTmpTexture2, new Vector4(m_LensFlareScreenSpace.intensity.value, m_LensFlareScreenSpace.firstFlareIntensity.value, m_LensFlareScreenSpace.secondaryFlareIntensity.value, m_LensFlareScreenSpace.warpedFlareIntensity.value), new Vector4(m_LensFlareScreenSpace.vignetteEffect.value, m_LensFlareScreenSpace.startingPosition.value, m_LensFlareScreenSpace.scale.value, 0f), new Vector4(m_LensFlareScreenSpace.samples.value, m_LensFlareScreenSpace.sampleDimmer.value, m_LensFlareScreenSpace.chromaticAbberationIntensity.value, 0f), new Vector4(m_LensFlareScreenSpace.streaksIntensity.value, m_LensFlareScreenSpace.streaksLength.value, m_LensFlareScreenSpace.streaksOrientation.value, m_LensFlareScreenSpace.streaksThreshold.value), new Vector4(value, m_LensFlareScreenSpace.warpedFlareScale.value.x, m_LensFlareScreenSpace.warpedFlareScale.value.y, 0f), cmd, m_ScreenSpaceLensFlareResult, debugView: false);
		cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, originalBloomTexture);
	}

	internal static void UpdateMotionBlurMatrices(ref Material material, Camera camera, XRPass xr)
	{
		MotionVectorsPersistentData motionVectorsPersistentData = null;
		if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
		{
			motionVectorsPersistentData = component.motionVectorsPersistentData;
		}
		if (motionVectorsPersistentData == null)
		{
			return;
		}
		if (xr.enabled && xr.singlePassEnabled)
		{
			material.SetMatrixArray(k_ShaderPropertyId_PrevViewProjMStereo, motionVectorsPersistentData.previousViewProjectionStereo);
			material.SetMatrixArray(k_ShaderPropertyId_ViewProjMStereo, motionVectorsPersistentData.viewProjectionStereo);
			return;
		}
		int num = 0;
		if (xr.enabled)
		{
			num = xr.multipassId;
		}
		material.SetMatrix(k_ShaderPropertyId_PrevViewProjM, motionVectorsPersistentData.previousViewProjectionStereo[num]);
		material.SetMatrix(k_ShaderPropertyId_ViewProjM, motionVectorsPersistentData.viewProjectionStereo[num]);
	}

	private void DoMotionBlur(CommandBuffer cmd, RTHandle source, RTHandle destination, RTHandle motionVectors, ref CameraData cameraData)
	{
		Material material = m_Materials.cameraMotionBlur;
		UpdateMotionBlurMatrices(ref material, cameraData.camera, cameraData.xr);
		material.SetFloat("_Intensity", m_MotionBlur.intensity.value);
		material.SetFloat("_Clamp", m_MotionBlur.clamp.value);
		int num = (int)m_MotionBlur.quality.value;
		if (m_MotionBlur.mode.value == MotionBlurMode.CameraAndObjects)
		{
			num += 3;
			material.SetTexture("_MotionVectorTexture", motionVectors);
		}
		PostProcessUtils.SetSourceSize(cmd, source);
		CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", cameraData.isAlphaOutputEnabled);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, material, num);
	}

	private void DoPaniniProjection(Camera camera, CommandBuffer cmd, RTHandle source, RTHandle destination)
	{
		float value = m_PaniniProjection.distance.value;
		Vector2 vector = CalcViewExtents(camera, m_Descriptor.width, m_Descriptor.height);
		Vector2 vector2 = CalcCropExtents(camera, value, m_Descriptor.width, m_Descriptor.height);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value2 = Mathf.Min(a, b);
		float num = value;
		float w = Mathf.Lerp(1f, Mathf.Clamp01(value2), m_PaniniProjection.cropToFit.value);
		Material paniniProjection = m_Materials.paniniProjection;
		paniniProjection.SetVector(ShaderConstants._Params, new Vector4(vector.x, vector.y, num, w));
		paniniProjection.EnableKeyword((1f - Mathf.Abs(num) > float.Epsilon) ? "_GENERIC" : "_UNIT_DISTANCE");
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, paniniProjection, 0);
	}

	private Vector2 CalcViewExtents(Camera camera, int width, int height)
	{
		float num = camera.fieldOfView * (MathF.PI / 180f);
		float num2 = (float)width / (float)height;
		float num3 = Mathf.Tan(0.5f * num);
		return new Vector2(num2 * num3, num3);
	}

	private Vector2 CalcCropExtents(Camera camera, float d, int width, int height)
	{
		float num = 1f + d;
		Vector2 vector = CalcViewExtents(camera, width, height);
		float num2 = Mathf.Sqrt(vector.x * vector.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return vector * num3 * (num / num4);
	}

	private void SetupBloom(CommandBuffer cmd, RTHandle source, Material uberMaterial, bool enableAlphaOutput)
	{
		int num = 1;
		num = m_Bloom.downscale.value switch
		{
			BloomDownscaleMode.Half => 1, 
			BloomDownscaleMode.Quarter => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		int num2 = Mathf.Max(1, m_Descriptor.width >> num);
		int num3 = Mathf.Max(1, m_Descriptor.height >> num);
		int num4 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(num2, num3), 2f) - 1f), 1, m_Bloom.maxIterations.value);
		float value = m_Bloom.clamp.value;
		float num5 = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
		float w = num5 * 0.5f;
		float x = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
		Material bloom = m_Materials.bloom;
		bloom.SetVector(ShaderConstants._Params, new Vector4(x, value, num5, w));
		CoreUtils.SetKeyword(bloom, "_BLOOM_HQ", m_Bloom.highQualityFiltering.value);
		CoreUtils.SetKeyword(bloom, "_ENABLE_ALPHA_OUTPUT", enableAlphaOutput);
		RenderTextureDescriptor descriptor = GetCompatibleDescriptor(num2, num3, m_DefaultColorFormat);
		for (int i = 0; i < num4; i++)
		{
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_BloomMipUp[i], in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, m_BloomMipUpName[i]);
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_BloomMipDown[i], in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, m_BloomMipDownName[i]);
			descriptor.width = Mathf.Max(1, descriptor.width >> 1);
			descriptor.height = Mathf.Max(1, descriptor.height >> 1);
		}
		Blitter.BlitCameraTexture(cmd, source, m_BloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 0);
		RTHandle source2 = m_BloomMipDown[0];
		for (int j = 1; j < num4; j++)
		{
			Blitter.BlitCameraTexture(cmd, source2, m_BloomMipUp[j], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 1);
			Blitter.BlitCameraTexture(cmd, m_BloomMipUp[j], m_BloomMipDown[j], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 2);
			source2 = m_BloomMipDown[j];
		}
		for (int num6 = num4 - 2; num6 >= 0; num6--)
		{
			RTHandle rTHandle = ((num6 == num4 - 2) ? m_BloomMipDown[num6 + 1] : m_BloomMipUp[num6 + 1]);
			RTHandle source3 = m_BloomMipDown[num6];
			RTHandle destination = m_BloomMipUp[num6];
			cmd.SetGlobalTexture(ShaderConstants._SourceTexLowMip, rTHandle);
			Blitter.BlitCameraTexture(cmd, source3, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 3);
		}
		Color color = m_Bloom.tint.value.linear;
		float num7 = ColorUtils.Luminance(in color);
		color = ((num7 > 0f) ? (color * (1f / num7)) : Color.white);
		uberMaterial.SetVector(value: new Vector4(m_Bloom.intensity.value, color.r, color.g, color.b), nameID: ShaderConstants._Bloom_Params);
		cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, m_BloomMipUp[0]);
		Texture texture = ((m_Bloom.dirtTexture.value == null) ? Texture2D.blackTexture : m_Bloom.dirtTexture.value);
		float num8 = (float)texture.width / (float)texture.height;
		float num9 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 value3 = new Vector4(1f, 1f, 0f, 0f);
		float value4 = m_Bloom.dirtIntensity.value;
		if (num8 > num9)
		{
			value3.x = num9 / num8;
			value3.z = (1f - value3.x) * 0.5f;
		}
		else if (num9 > num8)
		{
			value3.y = num8 / num9;
			value3.w = (1f - value3.y) * 0.5f;
		}
		uberMaterial.SetVector(ShaderConstants._LensDirt_Params, value3);
		uberMaterial.SetFloat(ShaderConstants._LensDirt_Intensity, value4);
		uberMaterial.SetTexture(ShaderConstants._LensDirt_Texture, texture);
		if (m_Bloom.highQualityFiltering.value)
		{
			uberMaterial.EnableKeyword((value4 > 0f) ? "_BLOOM_HQ_DIRT" : "_BLOOM_HQ");
		}
		else
		{
			uberMaterial.EnableKeyword((value4 > 0f) ? "_BLOOM_LQ_DIRT" : "_BLOOM_LQ");
		}
	}

	private void SetupLensDistortion(Material material, bool isSceneView)
	{
		float b = 1.6f * Mathf.Max(Mathf.Abs(m_LensDistortion.intensity.value * 100f), 1f);
		float num = MathF.PI / 180f * Mathf.Min(160f, b);
		float y = 2f * Mathf.Tan(num * 0.5f);
		Vector2 vector = m_LensDistortion.center.value * 2f - Vector2.one;
		Vector4 value = new Vector4(vector.x, vector.y, Mathf.Max(m_LensDistortion.xMultiplier.value, 0.0001f), Mathf.Max(m_LensDistortion.yMultiplier.value, 0.0001f));
		Vector4 value2 = new Vector4((m_LensDistortion.intensity.value >= 0f) ? num : (1f / num), y, 1f / m_LensDistortion.scale.value, m_LensDistortion.intensity.value * 100f);
		material.SetVector(ShaderConstants._Distortion_Params1, value);
		material.SetVector(ShaderConstants._Distortion_Params2, value2);
		if (m_LensDistortion.IsActive() && !isSceneView)
		{
			material.EnableKeyword("_DISTORTION");
		}
	}

	private void SetupChromaticAberration(Material material)
	{
		material.SetFloat(ShaderConstants._Chroma_Params, m_ChromaticAberration.intensity.value * 0.05f);
		if (m_ChromaticAberration.IsActive())
		{
			material.EnableKeyword("_CHROMATIC_ABERRATION");
		}
	}

	private void SetupVignette(Material material, XRPass xrPass, int width, int height)
	{
		Color value = m_Vignette.color.value;
		Vector2 center = m_Vignette.center.value;
		float num = (float)width / (float)height;
		if (xrPass != null && xrPass.enabled)
		{
			if (xrPass.singlePassEnabled)
			{
				material.SetVector(ShaderConstants._Vignette_ParamsXR, xrPass.ApplyXRViewCenterOffset(center));
			}
			else
			{
				center = xrPass.ApplyXRViewCenterOffset(center);
			}
		}
		Vector4 value2 = new Vector4(value.r, value.g, value.b, m_Vignette.rounded.value ? num : 1f);
		Vector4 value3 = new Vector4(center.x, center.y, m_Vignette.intensity.value * 3f, m_Vignette.smoothness.value * 5f);
		material.SetVector(ShaderConstants._Vignette_Params1, value2);
		material.SetVector(ShaderConstants._Vignette_Params2, value3);
	}

	private void SetupColorGrading(CommandBuffer cmd, ref RenderingData renderingData, Material material)
	{
		ref PostProcessingData postProcessingData = ref renderingData.postProcessingData;
		bool flag = postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.lutSize;
		int num = lutSize * lutSize;
		float w = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
		material.SetTexture(ShaderConstants._InternalLut, m_InternalLut);
		material.SetVector(ShaderConstants._Lut_Params, new Vector4(1f / (float)num, 1f / (float)lutSize, (float)lutSize - 1f, w));
		material.SetTexture(ShaderConstants._UserLut, m_ColorLookup.texture.value);
		material.SetVector(ShaderConstants._UserLut_Params, (!m_ColorLookup.IsActive()) ? Vector4.zero : new Vector4(1f / (float)m_ColorLookup.texture.value.width, 1f / (float)m_ColorLookup.texture.value.height, (float)m_ColorLookup.texture.value.height - 1f, m_ColorLookup.contribution.value));
		if (flag)
		{
			material.EnableKeyword("_HDR_GRADING");
			return;
		}
		switch (m_Tonemapping.mode.value)
		{
		case TonemappingMode.Neutral:
			material.EnableKeyword("_TONEMAP_NEUTRAL");
			break;
		case TonemappingMode.ACES:
			material.EnableKeyword("_TONEMAP_ACES");
			break;
		}
	}

	private void SetupGrain(UniversalCameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && m_FilmGrain.IsActive())
		{
			material.EnableKeyword("_FILM_GRAIN");
			PostProcessUtils.ConfigureFilmGrain(m_Data, m_FilmGrain, cameraData.pixelWidth, cameraData.pixelHeight, material);
		}
	}

	private void SetupDithering(UniversalCameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && cameraData.isDitheringEnabled)
		{
			material.EnableKeyword("_DITHERING");
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Data, m_DitheringTextureIndex, cameraData.pixelWidth, cameraData.pixelHeight, material);
		}
	}

	private void SetupHDROutput(HDROutputUtils.HDRDisplayInformation hdrDisplayInformation, ColorGamut hdrDisplayColorGamut, Material material, HDROutputUtils.Operation hdrOperations, bool rendersOverlayUI)
	{
		UniversalRenderPipeline.GetHDROutputLuminanceParameters(hdrDisplayInformation, hdrDisplayColorGamut, m_Tonemapping, out var hdrOutputParameters);
		material.SetVector(ShaderPropertyId.hdrOutputLuminanceParams, hdrOutputParameters);
		HDROutputUtils.ConfigureHDROutput(material, hdrDisplayColorGamut, hdrOperations);
		CoreUtils.SetKeyword(material, "_HDR_OVERLAY", rendersOverlayUI);
	}

	private void RenderFinalPass(CommandBuffer cmd, ref RenderingData renderingData)
	{
		UniversalCameraData universalCameraData = renderingData.frameData.Get<UniversalCameraData>();
		Material finalPass = m_Materials.finalPass;
		finalPass.shaderKeywords = null;
		PostProcessUtils.SetSourceSize(cmd, universalCameraData.renderer.cameraColorTargetHandle);
		SetupGrain(renderingData.cameraData.universalCameraData, finalPass);
		SetupDithering(renderingData.cameraData.universalCameraData, finalPass);
		if (RequireSRGBConversionBlitToBackBuffer(renderingData.cameraData.requireSrgbConversion))
		{
			finalPass.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
		}
		HDROutputUtils.Operation operation = HDROutputUtils.Operation.None;
		bool flag = RequireHDROutput(renderingData.cameraData.universalCameraData);
		if (flag)
		{
			operation = (m_EnableColorEncodingIfNeeded ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
			if (!universalCameraData.postProcessEnabled)
			{
				operation |= HDROutputUtils.Operation.ColorConversion;
			}
			SetupHDROutput(universalCameraData.hdrDisplayInformation, universalCameraData.hdrDisplayColorGamut, finalPass, operation, universalCameraData.rendersOverlayUI);
		}
		CoreUtils.SetKeyword(finalPass, "_ENABLE_ALPHA_OUTPUT", universalCameraData.isAlphaOutputEnabled);
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		bool flag2 = activeDebugHandler?.WriteToDebugScreenTexture(universalCameraData.resolveFinalTarget) ?? false;
		if (m_UseSwapBuffer)
		{
			m_Source = universalCameraData.renderer.GetCameraColorBackBuffer(cmd);
		}
		RTHandle source = m_Source;
		RenderBufferLoadAction loadAction = (universalCameraData.isDefaultViewport ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		bool flag3 = universalCameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		bool flag4 = universalCameraData.imageScalingMode == ImageScalingMode.Upscaling && universalCameraData.upscalingFilter == ImageUpscalingFilter.FSR;
		bool flag5 = universalCameraData.IsTemporalAAEnabled() && universalCameraData.taaSettings.contrastAdaptiveSharpening > 0f && !flag4;
		bool isAlphaOutputEnabled = universalCameraData.isAlphaOutputEnabled;
		if (universalCameraData.imageScalingMode != 0)
		{
			bool num = flag3 || flag4;
			RenderTextureDescriptor descriptor = universalCameraData.cameraTargetDescriptor;
			descriptor.msaaSamples = 1;
			descriptor.depthStencilFormat = GraphicsFormat.None;
			if (!flag)
			{
				descriptor.graphicsFormat = UniversalRenderPipeline.MakeUnormRenderTextureGraphicsFormat();
			}
			m_Materials.scalingSetup.shaderKeywords = null;
			if (num)
			{
				if (flag)
				{
					SetupHDROutput(universalCameraData.hdrDisplayInformation, universalCameraData.hdrDisplayColorGamut, m_Materials.scalingSetup, operation, universalCameraData.rendersOverlayUI);
				}
				if (flag3)
				{
					m_Materials.scalingSetup.EnableKeyword("_FXAA");
				}
				if (flag4)
				{
					m_Materials.scalingSetup.EnableKeyword(operation.HasFlag(HDROutputUtils.Operation.ColorEncoding) ? "_GAMMA_20_AND_HDR_INPUT" : "_GAMMA_20");
				}
				if (isAlphaOutputEnabled)
				{
					m_Materials.scalingSetup.EnableKeyword("_ENABLE_ALPHA_OUTPUT");
				}
				RenderingUtils.ReAllocateHandleIfNeeded(ref m_ScalingSetupTarget, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, 1, 0f, "_ScalingSetupTexture");
				Blitter.BlitCameraTexture(cmd, m_Source, m_ScalingSetupTarget, loadAction, RenderBufferStoreAction.Store, m_Materials.scalingSetup, 0);
				source = m_ScalingSetupTarget;
			}
			switch (universalCameraData.imageScalingMode)
			{
			case ImageScalingMode.Upscaling:
				switch (universalCameraData.upscalingFilter)
				{
				case ImageUpscalingFilter.Point:
					if (!flag5)
					{
						finalPass.EnableKeyword("_POINT_SAMPLING");
					}
					break;
				case ImageUpscalingFilter.FSR:
				{
					m_Materials.easu.shaderKeywords = null;
					RenderTextureDescriptor descriptor2 = universalCameraData.cameraTargetDescriptor;
					descriptor2.msaaSamples = 1;
					descriptor2.depthStencilFormat = GraphicsFormat.None;
					descriptor2.width = universalCameraData.pixelWidth;
					descriptor2.height = universalCameraData.pixelHeight;
					RenderingUtils.ReAllocateHandleIfNeeded(ref m_UpscaledTarget, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, 1, 0f, "_UpscaledTexture");
					Vector2 vector = new Vector2(universalCameraData.cameraTargetDescriptor.width, universalCameraData.cameraTargetDescriptor.height);
					Vector2 outputImageSizeInPixels = new Vector2(universalCameraData.pixelWidth, universalCameraData.pixelHeight);
					FSRUtils.SetEasuConstants(cmd, vector, vector, outputImageSizeInPixels);
					if (isAlphaOutputEnabled)
					{
						CoreUtils.SetKeyword(m_Materials.easu, "_ENABLE_ALPHA_OUTPUT", isAlphaOutputEnabled);
					}
					Blitter.BlitCameraTexture(cmd, source, m_UpscaledTarget, loadAction, RenderBufferStoreAction.Store, m_Materials.easu, 0);
					float sharpnessLinear = (universalCameraData.fsrOverrideSharpness ? universalCameraData.fsrSharpness : 0.92f);
					if (universalCameraData.fsrSharpness > 0f)
					{
						finalPass.EnableKeyword(flag ? "_EASU_RCAS_AND_HDR_INPUT" : "_RCAS");
						FSRUtils.SetRcasConstantsLinear(cmd, sharpnessLinear);
					}
					source = m_UpscaledTarget;
					PostProcessUtils.SetSourceSize(cmd, m_UpscaledTarget);
					break;
				}
				}
				break;
			case ImageScalingMode.Downscaling:
				flag5 = false;
				break;
			}
		}
		else if (flag3)
		{
			finalPass.EnableKeyword("_FXAA");
		}
		if (flag5)
		{
			finalPass.EnableKeyword("_RCAS");
			FSRUtils.SetRcasConstantsLinear(cmd, universalCameraData.taaSettings.contrastAdaptiveSharpening);
		}
		RenderTargetIdentifier cameraTargetIdentifier = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
		if (flag2)
		{
			Blitter.BlitCameraTexture(cmd, source, activeDebugHandler.DebugScreenColorHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, finalPass, 0);
			universalCameraData.renderer.ConfigureCameraTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
		}
		else
		{
			RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTargetIdentifier);
			RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
			RenderingUtils.FinalBlit(cmd, universalCameraData, source, s_RTHandleWrapper, loadAction, RenderBufferStoreAction.Store, finalPass, 0);
		}
	}

	private void UpdateCameraResolution(RenderGraph renderGraph, UniversalCameraData cameraData, Vector2Int newCameraTargetSize)
	{
		cameraData.cameraTargetDescriptor.width = newCameraTargetSize.x;
		cameraData.cameraTargetDescriptor.height = newCameraTargetSize.y;
		UpdateCameraResolutionPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<UpdateCameraResolutionPassData>("Update Camera Resolution", out passData, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 26);
		passData.newCameraTargetSize = newCameraTargetSize;
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(UpdateCameraResolutionPassData data, UnsafeGraphContext ctx)
		{
			ctx.cmd.SetGlobalVector(ShaderPropertyId.screenSize, new Vector4(data.newCameraTargetSize.x, data.newCameraTargetSize.y, 1f / (float)data.newCameraTargetSize.x, 1f / (float)data.newCameraTargetSize.y));
		});
	}

	internal static TextureHandle CreateCompatibleTexture(RenderGraph renderGraph, in TextureHandle source, string name, bool clear, FilterMode filterMode)
	{
		TextureDesc desc = source.GetDescriptor(renderGraph);
		MakeCompatible(ref desc);
		desc.name = name;
		desc.clearBuffer = clear;
		desc.filterMode = filterMode;
		return renderGraph.CreateTexture(in desc);
	}

	internal static TextureHandle CreateCompatibleTexture(RenderGraph renderGraph, in TextureDesc desc, string name, bool clear, FilterMode filterMode)
	{
		TextureDesc desc2 = GetCompatibleDescriptor(desc);
		desc2.name = name;
		desc2.clearBuffer = clear;
		desc2.filterMode = filterMode;
		return renderGraph.CreateTexture(in desc2);
	}

	internal static TextureDesc GetCompatibleDescriptor(TextureDesc desc, int width, int height, GraphicsFormat format)
	{
		desc.width = width;
		desc.height = height;
		desc.format = format;
		MakeCompatible(ref desc);
		return desc;
	}

	internal static TextureDesc GetCompatibleDescriptor(TextureDesc desc)
	{
		MakeCompatible(ref desc);
		return desc;
	}

	internal static void MakeCompatible(ref TextureDesc desc)
	{
		desc.msaaSamples = MSAASamples.None;
		desc.useMipMap = false;
		desc.autoGenerateMips = false;
		desc.anisoLevel = 0;
		desc.discardBuffer = false;
	}

	public void RenderStopNaN(RenderGraph renderGraph, in TextureHandle activeCameraColor, out TextureHandle stopNaNTarget)
	{
		stopNaNTarget = CreateCompatibleTexture(renderGraph, in activeCameraColor, "_StopNaNsTarget", clear: true, FilterMode.Bilinear);
		StopNaNsPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<StopNaNsPassData>("Stop NaNs", out passData, ProfilingSampler.Get(URPProfileId.RG_StopNaNs), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 106);
		passData.stopNaNTarget = stopNaNTarget;
		rasterRenderGraphBuilder.SetRenderAttachment(stopNaNTarget, 0, AccessFlags.ReadWrite);
		passData.sourceTexture = activeCameraColor;
		rasterRenderGraphBuilder.UseTexture(in activeCameraColor);
		passData.stopNaN = m_Materials.stopNaN;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(StopNaNsPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, data.stopNaN, 0);
		});
	}

	public void RenderSMAA(RenderGraph renderGraph, UniversalResourceData resourceData, AntialiasingQuality antialiasingQuality, in TextureHandle source, out TextureHandle SMAATarget)
	{
		TextureDesc desc = renderGraph.GetTextureDesc(in source);
		SMAATarget = CreateCompatibleTexture(renderGraph, in desc, "_SMAATarget", clear: true, FilterMode.Bilinear);
		desc.clearColor = Color.black;
		desc.clearColor.a = 0f;
		TextureDesc desc2 = desc;
		desc2.format = m_SMAAEdgeFormat;
		TextureHandle textureHandle = CreateCompatibleTexture(renderGraph, in desc2, "_EdgeStencilTexture", clear: true, FilterMode.Bilinear);
		TextureDesc desc3 = desc;
		desc3.format = GraphicsFormatUtility.GetDepthStencilFormat(24);
		TextureHandle textureHandle2 = CreateCompatibleTexture(renderGraph, in desc3, "_EdgeTexture", clear: true, FilterMode.Bilinear);
		TextureDesc desc4 = desc;
		desc4.format = GraphicsFormat.R8G8B8A8_UNorm;
		TextureHandle textureHandle3 = CreateCompatibleTexture(renderGraph, in desc4, "_BlendTexture", clear: true, FilterMode.Point);
		Material subpixelMorphologicalAntialiasing = m_Materials.subpixelMorphologicalAntialiasing;
		SMAASetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<SMAASetupPassData>("SMAA Material Setup", out passData, ProfilingSampler.Get(URPProfileId.RG_SMAAMaterialSetup), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 169))
		{
			passData.metrics = new Vector4(1f / (float)desc.width, 1f / (float)desc.height, desc.width, desc.height);
			passData.areaTexture = m_Data.textures.smaaAreaTex;
			passData.searchTexture = m_Data.textures.smaaSearchTex;
			passData.stencilRef = 64f;
			passData.stencilMask = 64f;
			passData.antialiasingQuality = antialiasingQuality;
			passData.material = subpixelMorphologicalAntialiasing;
			rasterRenderGraphBuilder.AllowPassCulling(value: false);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(SMAASetupPassData data, RasterGraphContext context)
			{
				data.material.SetVector(ShaderConstants._Metrics, data.metrics);
				data.material.SetTexture(ShaderConstants._AreaTexture, data.areaTexture);
				data.material.SetTexture(ShaderConstants._SearchTexture, data.searchTexture);
				data.material.SetFloat(ShaderConstants._StencilRef, data.stencilRef);
				data.material.SetFloat(ShaderConstants._StencilMask, data.stencilMask);
				data.material.shaderKeywords = null;
				switch (data.antialiasingQuality)
				{
				case AntialiasingQuality.Low:
					data.material.EnableKeyword("_SMAA_PRESET_LOW");
					break;
				case AntialiasingQuality.Medium:
					data.material.EnableKeyword("_SMAA_PRESET_MEDIUM");
					break;
				case AntialiasingQuality.High:
					data.material.EnableKeyword("_SMAA_PRESET_HIGH");
					break;
				}
			});
		}
		SMAAPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Edge Detection", out passData2, ProfilingSampler.Get(URPProfileId.RG_SMAAEdgeDetection), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 210))
		{
			rasterRenderGraphBuilder2.SetRenderAttachment(textureHandle, 0);
			passData2.depthStencilTexture = textureHandle2;
			rasterRenderGraphBuilder2.SetRenderAttachmentDepth(textureHandle2);
			passData2.sourceTexture = source;
			rasterRenderGraphBuilder2.UseTexture(in source);
			TextureHandle cameraDepth = resourceData.cameraDepth;
			rasterRenderGraphBuilder2.UseTexture(in cameraDepth);
			passData2.material = subpixelMorphologicalAntialiasing;
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
			{
				Material material3 = data.material;
				RasterCommandBuffer cmd3 = context.cmd;
				RTHandle rTHandle3 = data.sourceTexture;
				Vector2 vector3 = (rTHandle3.useScaling ? new Vector2(rTHandle3.rtHandleProperties.rtHandleScale.x, rTHandle3.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd3, rTHandle3, vector3, material3, 0);
			});
		}
		SMAAPassData passData3;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Blend weights", out passData3, ProfilingSampler.Get(URPProfileId.RG_SMAABlendWeight), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 232))
		{
			rasterRenderGraphBuilder3.SetRenderAttachment(textureHandle3, 0);
			passData3.depthStencilTexture = textureHandle2;
			rasterRenderGraphBuilder3.SetRenderAttachmentDepth(textureHandle2, AccessFlags.Read);
			passData3.sourceTexture = textureHandle;
			rasterRenderGraphBuilder3.UseTexture(in textureHandle);
			passData3.material = subpixelMorphologicalAntialiasing;
			rasterRenderGraphBuilder3.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
			{
				Material material2 = data.material;
				RasterCommandBuffer cmd2 = context.cmd;
				RTHandle rTHandle2 = data.sourceTexture;
				Vector2 vector2 = (rTHandle2.useScaling ? new Vector2(rTHandle2.rtHandleProperties.rtHandleScale.x, rTHandle2.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd2, rTHandle2, vector2, material2, 1);
			});
		}
		SMAAPassData passData4;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder4 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Neighborhood blending", out passData4, ProfilingSampler.Get(URPProfileId.RG_SMAANeighborhoodBlend), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 253);
		rasterRenderGraphBuilder4.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder4.SetRenderAttachment(SMAATarget, 0);
		passData4.sourceTexture = source;
		rasterRenderGraphBuilder4.UseTexture(in source);
		passData4.blendTexture = textureHandle3;
		rasterRenderGraphBuilder4.UseTexture(in textureHandle3);
		passData4.material = subpixelMorphologicalAntialiasing;
		rasterRenderGraphBuilder4.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
		{
			Material material = data.material;
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			material.SetTexture(ShaderConstants._BlendTexture, data.blendTexture);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, material, 2);
		});
	}

	public void UberPostSetupBloomPass(RenderGraph rendergraph, Material uberMaterial, in TextureDesc srcDesc)
	{
		using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_UberPostSetupBloomPass)))
		{
			Color color = m_Bloom.tint.value.linear;
			float num = ColorUtils.Luminance(in color);
			color = ((num > 0f) ? (color * (1f / num)) : Color.white);
			Vector4 value = new Vector4(m_Bloom.intensity.value, color.r, color.g, color.b);
			Texture texture = ((m_Bloom.dirtTexture.value == null) ? Texture2D.blackTexture : m_Bloom.dirtTexture.value);
			float num2 = (float)texture.width / (float)texture.height;
			float num3 = (float)srcDesc.width / (float)srcDesc.height;
			Vector4 value2 = new Vector4(1f, 1f, 0f, 0f);
			float value3 = m_Bloom.dirtIntensity.value;
			if (num2 > num3)
			{
				value2.x = num3 / num2;
				value2.z = (1f - value2.x) * 0.5f;
			}
			else if (num3 > num2)
			{
				value2.y = num2 / num3;
				value2.w = (1f - value2.y) * 0.5f;
			}
			bool value4 = m_Bloom.highQualityFiltering.value;
			uberMaterial.SetVector(ShaderConstants._Bloom_Params, value);
			uberMaterial.SetVector(ShaderConstants._LensDirt_Params, value2);
			uberMaterial.SetFloat(ShaderConstants._LensDirt_Intensity, value3);
			uberMaterial.SetTexture(ShaderConstants._LensDirt_Texture, texture);
			if (value4)
			{
				uberMaterial.EnableKeyword((value3 > 0f) ? "_BLOOM_HQ_DIRT" : "_BLOOM_HQ");
			}
			else
			{
				uberMaterial.EnableKeyword((value3 > 0f) ? "_BLOOM_LQ_DIRT" : "_BLOOM_LQ");
			}
		}
	}

	public Vector2Int CalcBloomResolution(Bloom bloom, in TextureDesc bloomSourceDesc)
	{
		int num = 1;
		num = m_Bloom.downscale.value switch
		{
			BloomDownscaleMode.Half => 1, 
			BloomDownscaleMode.Quarter => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		int x = Mathf.Max(1, bloomSourceDesc.width >> num);
		int y = Mathf.Max(1, bloomSourceDesc.height >> num);
		return new Vector2Int(x, y);
	}

	public int CalcBloomMipCount(Bloom bloom, Vector2Int bloomResolution)
	{
		return Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(bloomResolution.x, bloomResolution.y), 2f) - 1f), 1, m_Bloom.maxIterations.value);
	}

	public void RenderBloomTexture(RenderGraph renderGraph, in TextureHandle source, out TextureHandle destination, bool enableAlphaOutput)
	{
		TextureDesc bloomSourceDesc = source.GetDescriptor(renderGraph);
		Vector2Int bloomResolution = CalcBloomResolution(m_Bloom, in bloomSourceDesc);
		int num = CalcBloomMipCount(m_Bloom, bloomResolution);
		int num2 = bloomResolution.x;
		int num3 = bloomResolution.y;
		using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_BloomSetup)))
		{
			float value = m_Bloom.clamp.value;
			float num4 = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
			float w = num4 * 0.5f;
			float x = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
			BloomMaterialParams other = default(BloomMaterialParams);
			other.parameters = new Vector4(x, value, num4, w);
			other.highQualityFiltering = m_Bloom.highQualityFiltering.value;
			other.enableAlphaOutput = enableAlphaOutput;
			Material bloom = m_Materials.bloom;
			bool num5 = !m_BloomParamsPrev.Equals(ref other);
			bool flag = bloom.HasProperty(ShaderConstants._Params);
			if (num5 || !flag)
			{
				bloom.SetVector(ShaderConstants._Params, other.parameters);
				CoreUtils.SetKeyword(bloom, "_BLOOM_HQ", other.highQualityFiltering);
				CoreUtils.SetKeyword(bloom, "_ENABLE_ALPHA_OUTPUT", other.enableAlphaOutput);
				for (uint num6 = 0u; num6 < 16; num6++)
				{
					Material obj = m_Materials.bloomUpsample[num6];
					obj.SetVector(ShaderConstants._Params, other.parameters);
					CoreUtils.SetKeyword(obj, "_BLOOM_HQ", other.highQualityFiltering);
					CoreUtils.SetKeyword(obj, "_ENABLE_ALPHA_OUTPUT", other.enableAlphaOutput);
				}
				m_BloomParamsPrev = other;
			}
			TextureDesc desc = GetCompatibleDescriptor(bloomSourceDesc, num2, num3, m_DefaultColorFormat);
			_BloomMipDown[0] = CreateCompatibleTexture(renderGraph, in desc, m_BloomMipDownName[0], clear: false, FilterMode.Bilinear);
			_BloomMipUp[0] = CreateCompatibleTexture(renderGraph, in desc, m_BloomMipUpName[0], clear: false, FilterMode.Bilinear);
			for (int i = 1; i < num; i++)
			{
				num2 = Mathf.Max(1, num2 >> 1);
				num3 = Mathf.Max(1, num3 >> 1);
				ref TextureHandle reference = ref _BloomMipDown[i];
				ref TextureHandle reference2 = ref _BloomMipUp[i];
				desc.width = num2;
				desc.height = num3;
				reference = CreateCompatibleTexture(renderGraph, in desc, m_BloomMipDownName[i], clear: false, FilterMode.Bilinear);
				reference2 = CreateCompatibleTexture(renderGraph, in desc, m_BloomMipUpName[i], clear: false, FilterMode.Bilinear);
			}
		}
		BloomPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<BloomPassData>("Blit Bloom Mipmaps", out passData, ProfilingSampler.Get(URPProfileId.Bloom), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 465);
		passData.mipCount = num;
		passData.material = m_Materials.bloom;
		passData.upsampleMaterials = m_Materials.bloomUpsample;
		passData.sourceTexture = source;
		passData.bloomMipDown = _BloomMipDown;
		passData.bloomMipUp = _BloomMipUp;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseTexture(in source);
		for (int j = 0; j < num; j++)
		{
			unsafeRenderGraphBuilder.UseTexture(in _BloomMipDown[j], AccessFlags.ReadWrite);
			unsafeRenderGraphBuilder.UseTexture(in _BloomMipUp[j], AccessFlags.ReadWrite);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(BloomPassData data, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			Material material = data.material;
			int mipCount = data.mipCount;
			RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
			RenderBufferStoreAction storeAction = RenderBufferStoreAction.Store;
			using (new ProfilingScope(nativeCommandBuffer, ProfilingSampler.Get(URPProfileId.RG_BloomPrefilter)))
			{
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.sourceTexture, data.bloomMipDown[0], loadAction, storeAction, material, 0);
			}
			using (new ProfilingScope(nativeCommandBuffer, ProfilingSampler.Get(URPProfileId.RG_BloomDownsample)))
			{
				TextureHandle textureHandle = data.bloomMipDown[0];
				for (int k = 1; k < mipCount; k++)
				{
					TextureHandle textureHandle2 = data.bloomMipDown[k];
					TextureHandle textureHandle3 = data.bloomMipUp[k];
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle, textureHandle3, loadAction, storeAction, material, 1);
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle3, textureHandle2, loadAction, storeAction, material, 2);
					textureHandle = textureHandle2;
				}
			}
			using (new ProfilingScope(nativeCommandBuffer, ProfilingSampler.Get(URPProfileId.RG_BloomUpsample)))
			{
				for (int num7 = mipCount - 2; num7 >= 0; num7--)
				{
					TextureHandle textureHandle4 = ((num7 == mipCount - 2) ? data.bloomMipDown[num7 + 1] : data.bloomMipUp[num7 + 1]);
					TextureHandle textureHandle5 = data.bloomMipDown[num7];
					TextureHandle textureHandle6 = data.bloomMipUp[num7];
					Material material2 = data.upsampleMaterials[num7];
					material2.SetTexture(ShaderConstants._SourceTexLowMip, textureHandle4);
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle5, textureHandle6, loadAction, storeAction, material2, 3);
				}
			}
		});
		destination = passData.bloomMipUp[0];
	}

	public void RenderDoF(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, in TextureHandle source, out TextureHandle destination)
	{
		Material dofMaterial = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? m_Materials.gaussianDepthOfField : m_Materials.bokehDepthOfField);
		destination = CreateCompatibleTexture(renderGraph, in source, "_DoFTarget", clear: true, FilterMode.Bilinear);
		CoreUtils.SetKeyword(dofMaterial, "_ENABLE_ALPHA_OUTPUT", cameraData.isAlphaOutputEnabled);
		if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
		{
			RenderDoFGaussian(renderGraph, resourceData, cameraData, in source, destination, ref dofMaterial);
		}
		else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
		{
			RenderDoFBokeh(renderGraph, resourceData, cameraData, in source, in destination, ref dofMaterial);
		}
	}

	public void RenderDoFGaussian(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, in TextureHandle source, TextureHandle destination, ref Material dofMaterial)
	{
		TextureDesc descriptor = source.GetDescriptor(renderGraph);
		Material material = dofMaterial;
		int num = 2;
		int num2 = descriptor.width / num;
		int height = descriptor.height / num;
		TextureDesc desc = GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, m_GaussianCoCFormat);
		TextureHandle fullCoCTexture = CreateCompatibleTexture(renderGraph, in desc, "_FullCoCTexture", clear: true, FilterMode.Bilinear);
		TextureDesc desc2 = GetCompatibleDescriptor(descriptor, num2, height, m_GaussianCoCFormat);
		TextureHandle halfCoCTexture = CreateCompatibleTexture(renderGraph, in desc2, "_HalfCoCTexture", clear: true, FilterMode.Bilinear);
		TextureDesc desc3 = GetCompatibleDescriptor(descriptor, num2, height, m_DefaultColorFormat);
		TextureHandle pingTexture = CreateCompatibleTexture(renderGraph, in desc3, "_PingTexture", clear: true, FilterMode.Bilinear);
		TextureDesc desc4 = GetCompatibleDescriptor(descriptor, num2, height, m_DefaultColorFormat);
		TextureHandle pongTexture = CreateCompatibleTexture(renderGraph, in desc4, "_PongTexture", clear: true, FilterMode.Bilinear);
		DoFGaussianPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<DoFGaussianPassData>("Depth of Field - Gaussian", out passData, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 605);
		float value = m_DepthOfField.gaussianStart.value;
		float y = Mathf.Max(value, m_DepthOfField.gaussianEnd.value);
		float a = m_DepthOfField.gaussianMaxRadius.value * ((float)num2 / 1080f);
		a = Mathf.Min(a, 2f);
		passData.downsample = num;
		passData.cocParams = new Vector3(value, y, a);
		passData.highQualitySamplingValue = m_DepthOfField.highQualitySampling.value;
		passData.material = material;
		passData.materialCoC = m_Materials.gaussianDepthOfFieldCoC;
		passData.sourceTexture = source;
		unsafeRenderGraphBuilder.UseTexture(in source);
		passData.depthTexture = resourceData.cameraDepthTexture;
		TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;
		unsafeRenderGraphBuilder.UseTexture(in cameraDepthTexture);
		passData.fullCoCTexture = fullCoCTexture;
		unsafeRenderGraphBuilder.UseTexture(in fullCoCTexture, AccessFlags.ReadWrite);
		passData.halfCoCTexture = halfCoCTexture;
		unsafeRenderGraphBuilder.UseTexture(in halfCoCTexture, AccessFlags.ReadWrite);
		passData.pingTexture = pingTexture;
		unsafeRenderGraphBuilder.UseTexture(in pingTexture, AccessFlags.ReadWrite);
		passData.pongTexture = pongTexture;
		unsafeRenderGraphBuilder.UseTexture(in pongTexture, AccessFlags.ReadWrite);
		passData.destination = destination;
		unsafeRenderGraphBuilder.UseTexture(in destination, AccessFlags.Write);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DoFGaussianPassData data, UnsafeGraphContext context)
		{
			Material material2 = data.material;
			Material materialCoC = data.materialCoC;
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			RTHandle rTHandle = data.sourceTexture;
			RTHandle destination2 = data.destination;
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_SetupDoF)))
			{
				material2.SetVector(ShaderConstants._CoCParams, data.cocParams);
				CoreUtils.SetKeyword(material2, "_HIGH_QUALITY_SAMPLING", data.highQualitySamplingValue);
				materialCoC.SetVector(ShaderConstants._CoCParams, data.cocParams);
				CoreUtils.SetKeyword(materialCoC, "_HIGH_QUALITY_SAMPLING", data.highQualitySamplingValue);
				PostProcessUtils.SetSourceSize(nativeCommandBuffer, data.sourceTexture);
				material2.SetVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)data.downsample, 1f / (float)data.downsample, data.downsample, data.downsample));
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFComputeCOC)))
			{
				material2.SetTexture(s_CameraDepthTextureID, data.depthTexture);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.sourceTexture, data.fullCoCTexture, data.materialCoC, 0);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFDownscalePrefilter)))
			{
				material2.SetTexture(ShaderConstants._FullCoCTexture, data.fullCoCTexture);
				data.multipleRenderTargets[0] = data.halfCoCTexture;
				data.multipleRenderTargets[1] = data.pingTexture;
				CoreUtils.SetRenderTarget(nativeCommandBuffer, data.multipleRenderTargets, data.halfCoCTexture);
				Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(nativeCommandBuffer, data.sourceTexture, vector, material2, 1);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFBlurH)))
			{
				material2.SetTexture(ShaderConstants._HalfCoCTexture, data.halfCoCTexture);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.pingTexture, data.pongTexture, material2, 2);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFBlurV)))
			{
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.pongTexture, data.pingTexture, material2, 3);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFComposite)))
			{
				material2.SetTexture(ShaderConstants._ColorTexture, data.pingTexture);
				material2.SetTexture(ShaderConstants._FullCoCTexture, data.fullCoCTexture);
				Blitter.BlitCameraTexture(nativeCommandBuffer, rTHandle, destination2, material2, 4);
			}
		});
	}

	public void RenderDoFBokeh(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, in TextureHandle source, in TextureHandle destination, ref Material dofMaterial)
	{
		TextureDesc descriptor = source.GetDescriptor(renderGraph);
		int num = 2;
		Material material = dofMaterial;
		int num2 = descriptor.width / num;
		int num3 = descriptor.height / num;
		TextureDesc desc = GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, GraphicsFormat.R8_UNorm);
		TextureHandle fullCoCTexture = CreateCompatibleTexture(renderGraph, in desc, "_FullCoCTexture", clear: true, FilterMode.Bilinear);
		TextureDesc desc2 = GetCompatibleDescriptor(descriptor, num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		TextureHandle pingTexture = CreateCompatibleTexture(renderGraph, in desc2, "_PingTexture", clear: true, FilterMode.Bilinear);
		TextureDesc desc3 = GetCompatibleDescriptor(descriptor, num2, num3, GraphicsFormat.R16G16B16A16_SFloat);
		TextureHandle pongTexture = CreateCompatibleTexture(renderGraph, in desc3, "_PongTexture", clear: true, FilterMode.Bilinear);
		DoFBokehPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<DoFBokehPassData>("Depth of Field - Bokeh", out passData, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 758);
		float num4 = m_DepthOfField.focalLength.value / 1000f;
		float num5 = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
		float value = m_DepthOfField.focusDistance.value;
		float y = num5 * num4 / (value - num4);
		float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(descriptor.height);
		float num6 = 1f / ((float)num2 / (float)num3);
		int hashCode = m_DepthOfField.GetHashCode();
		if (hashCode != m_BokehHash || maxBokehRadiusInPixels != m_BokehMaxRadius || num6 != m_BokehRCPAspect)
		{
			m_BokehHash = hashCode;
			m_BokehMaxRadius = maxBokehRadiusInPixels;
			m_BokehRCPAspect = num6;
			PrepareBokehKernel(maxBokehRadiusInPixels, num6);
		}
		float uvMargin = 1f / (float)descriptor.height * (float)num;
		passData.bokehKernel = m_BokehKernel;
		passData.downSample = num;
		passData.uvMargin = uvMargin;
		passData.cocParams = new Vector4(value, y, maxBokehRadiusInPixels, num6);
		passData.useFastSRGBLinearConversion = m_UseFastSRGBLinearConversion;
		passData.sourceTexture = source;
		unsafeRenderGraphBuilder.UseTexture(in source);
		passData.depthTexture = resourceData.cameraDepthTexture;
		TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;
		unsafeRenderGraphBuilder.UseTexture(in cameraDepthTexture);
		passData.material = material;
		passData.materialCoC = m_Materials.bokehDepthOfFieldCoC;
		passData.fullCoCTexture = fullCoCTexture;
		unsafeRenderGraphBuilder.UseTexture(in fullCoCTexture, AccessFlags.ReadWrite);
		passData.pingTexture = pingTexture;
		unsafeRenderGraphBuilder.UseTexture(in pingTexture, AccessFlags.ReadWrite);
		passData.pongTexture = pongTexture;
		unsafeRenderGraphBuilder.UseTexture(in pongTexture, AccessFlags.ReadWrite);
		passData.destination = destination;
		unsafeRenderGraphBuilder.UseTexture(in destination, AccessFlags.Write);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DoFBokehPassData data, UnsafeGraphContext context)
		{
			Material material2 = data.material;
			Material materialCoC = data.materialCoC;
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			RTHandle source2 = data.sourceTexture;
			RTHandle destination2 = data.destination;
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_SetupDoF)))
			{
				CoreUtils.SetKeyword(material2, "_USE_FAST_SRGB_LINEAR_CONVERSION", data.useFastSRGBLinearConversion);
				CoreUtils.SetKeyword(materialCoC, "_USE_FAST_SRGB_LINEAR_CONVERSION", data.useFastSRGBLinearConversion);
				material2.SetVector(ShaderConstants._CoCParams, data.cocParams);
				material2.SetVectorArray(ShaderConstants._BokehKernel, data.bokehKernel);
				material2.SetVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)data.downSample, 1f / (float)data.downSample, data.downSample, data.downSample));
				material2.SetVector(ShaderConstants._BokehConstants, new Vector4(data.uvMargin, data.uvMargin * 2f));
				PostProcessUtils.SetSourceSize(nativeCommandBuffer, data.sourceTexture);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFComputeCOC)))
			{
				material2.SetTexture(s_CameraDepthTextureID, data.depthTexture);
				Blitter.BlitCameraTexture(nativeCommandBuffer, source2, data.fullCoCTexture, material2, 0);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFDownscalePrefilter)))
			{
				material2.SetTexture(ShaderConstants._FullCoCTexture, data.fullCoCTexture);
				Blitter.BlitCameraTexture(nativeCommandBuffer, source2, data.pingTexture, material2, 1);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFBlurBokeh)))
			{
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.pingTexture, data.pongTexture, material2, 2);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFPostFilter)))
			{
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.pongTexture, data.pingTexture, material2, 3);
			}
			using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RG_DOFComposite)))
			{
				material2.SetTexture(ShaderConstants._DofTexture, data.pingTexture);
				Blitter.BlitCameraTexture(nativeCommandBuffer, source2, destination2, material2, 4);
			}
		});
	}

	public void RenderPaniniProjection(RenderGraph renderGraph, Camera camera, in TextureHandle source, out TextureHandle destination)
	{
		destination = CreateCompatibleTexture(renderGraph, in source, "_PaniniProjectionTarget", clear: true, FilterMode.Bilinear);
		TextureDesc descriptor = source.GetDescriptor(renderGraph);
		float value = m_PaniniProjection.distance.value;
		Vector2 vector = CalcViewExtents(camera, descriptor.width, descriptor.height);
		Vector2 vector2 = CalcCropExtents(camera, value, descriptor.width, descriptor.height);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value2 = Mathf.Min(a, b);
		float num = value;
		float w = Mathf.Lerp(1f, Mathf.Clamp01(value2), m_PaniniProjection.cropToFit.value);
		PaniniProjectionPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PaniniProjectionPassData>("Panini Projection", out passData, ProfilingSampler.Get(URPProfileId.PaniniProjection), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 899);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.material = m_Materials.paniniProjection;
		passData.paniniParams = new Vector4(vector.x, vector.y, num, w);
		passData.isPaniniGeneric = 1f - Mathf.Abs(num) > float.Epsilon;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PaniniProjectionPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			cmd.SetGlobalVector(ShaderConstants._Params, data.paniniParams);
			data.material.EnableKeyword(data.isPaniniGeneric ? "_GENERIC" : "_UNIT_DISTANCE");
			Vector2 vector3 = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector3, data.material, 0);
		});
	}

	private void RenderTemporalAA(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, ref TextureHandle source, out TextureHandle destination)
	{
		destination = CreateCompatibleTexture(renderGraph, in source, "_TemporalAATarget", clear: false, FilterMode.Bilinear);
		TextureHandle srcDepth = resourceData.cameraDepth;
		TextureHandle srcMotionVectors = resourceData.motionVectorColor;
		TemporalAA.Render(renderGraph, m_Materials.temporalAntialiasing, cameraData, ref source, ref srcDepth, ref srcMotionVectors, ref destination);
	}

	private void RenderSTP(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, ref TextureHandle source, out TextureHandle destination)
	{
		TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;
		TextureHandle motionVectorColor = resourceData.motionVectorColor;
		TextureDesc descriptor = source.GetDescriptor(renderGraph);
		TextureDesc desc = GetCompatibleDescriptor(descriptor, cameraData.pixelWidth, cameraData.pixelHeight, GraphicsFormatUtility.GetLinearFormat(descriptor.format));
		desc.enableRandomWrite = true;
		destination = CreateCompatibleTexture(renderGraph, in desc, "_CameraColorUpscaledSTP", clear: false, FilterMode.Bilinear);
		int frameCount = Time.frameCount;
		Texture2D noiseTexture = m_Data.textures.blueNoise16LTex[frameCount & (m_Data.textures.blueNoise16LTex.Length - 1)];
		StpUtils.Execute(renderGraph, resourceData, cameraData, source, cameraDepthTexture, motionVectorColor, destination, noiseTexture);
		UpdateCameraResolution(renderGraph, cameraData, new Vector2Int(desc.width, desc.height));
	}

	public void RenderMotionBlur(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, in TextureHandle source, out TextureHandle destination)
	{
		Material cameraMotionBlur = m_Materials.cameraMotionBlur;
		destination = CreateCompatibleTexture(renderGraph, in source, "_MotionBlurTarget", clear: true, FilterMode.Bilinear);
		TextureHandle motionVectorColor = resourceData.motionVectorColor;
		TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;
		MotionBlurMode value = m_MotionBlur.mode.value;
		int value2 = (int)m_MotionBlur.quality.value;
		value2 += ((value == MotionBlurMode.CameraAndObjects) ? 3 : 0);
		MotionBlurPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<MotionBlurPassData>("Motion Blur", out passData, ProfilingSampler.Get(URPProfileId.RG_MotionBlur), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1004);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		if (value == MotionBlurMode.CameraAndObjects)
		{
			passData.motionVectors = motionVectorColor;
			rasterRenderGraphBuilder.UseTexture(in motionVectorColor);
		}
		else
		{
			passData.motionVectors = TextureHandle.nullHandle;
		}
		rasterRenderGraphBuilder.UseTexture(in cameraDepthTexture);
		passData.material = cameraMotionBlur;
		passData.passIndex = value2;
		passData.camera = cameraData.camera;
		passData.xr = cameraData.xr;
		passData.enableAlphaOutput = cameraData.isAlphaOutputEnabled;
		passData.intensity = m_MotionBlur.intensity.value;
		passData.clamp = m_MotionBlur.clamp.value;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(MotionBlurPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			UpdateMotionBlurMatrices(ref data.material, data.camera, data.xr);
			data.material.SetFloat("_Intensity", data.intensity);
			data.material.SetFloat("_Clamp", data.clamp);
			CoreUtils.SetKeyword(data.material, "_ENABLE_ALPHA_OUTPUT", data.enableAlphaOutput);
			PostProcessUtils.SetSourceSize(cmd, data.sourceTexture);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, data.material, data.passIndex);
		});
	}

	private void LensFlareDataDrivenComputeOcclusion(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, in TextureDesc srcDesc)
	{
		if (!LensFlareCommonSRP.IsOcclusionRTCompatible())
		{
			return;
		}
		LensFlarePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<LensFlarePassData>("Lens Flare Compute Occlusion", out passData, ProfilingSampler.Get(URPProfileId.LensFlareDataDrivenComputeOcclusion), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1073);
		_ = LensFlareCommonSRP.occlusionRT;
		TextureHandle textureHandle = (passData.destinationTexture = renderGraph.ImportTexture(LensFlareCommonSRP.occlusionRT));
		unsafeRenderGraphBuilder.UseTexture(in textureHandle, AccessFlags.Write);
		passData.cameraData = cameraData;
		passData.viewport = cameraData.pixelRect;
		passData.material = m_Materials.lensFlareDataDriven;
		passData.width = srcDesc.width;
		passData.height = srcDesc.height;
		if (m_PaniniProjection.IsActive())
		{
			passData.usePanini = true;
			passData.paniniDistance = m_PaniniProjection.distance.value;
			passData.paniniCropToFit = m_PaniniProjection.cropToFit.value;
		}
		else
		{
			passData.usePanini = false;
			passData.paniniDistance = 1f;
			passData.paniniCropToFit = 1f;
		}
		TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;
		unsafeRenderGraphBuilder.UseTexture(in cameraDepthTexture);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(LensFlarePassData data, UnsafeGraphContext ctx)
		{
			Camera camera = data.cameraData.camera;
			XRPass xr = data.cameraData.xr;
			Matrix4x4 viewProjMatrix;
			if (xr.enabled)
			{
				if (xr.singlePassEnabled)
				{
					viewProjMatrix = GL.GetGPUProjectionMatrix(data.cameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true) * data.cameraData.GetViewMatrix();
				}
				else
				{
					viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
					_ = data.cameraData.xr.multipassId;
				}
			}
			else
			{
				viewProjMatrix = GL.GetGPUProjectionMatrix(data.cameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true) * data.cameraData.GetViewMatrix();
			}
			LensFlareCommonSRP.ComputeOcclusion(data.material, camera, xr, xr.multipassId, data.width, data.height, data.usePanini, data.paniniDistance, data.paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, ctx.cmd, taaEnabled: false, hasCloudLayer: false, null, null);
			if (xr.enabled && xr.singlePassEnabled)
			{
				for (int i = 1; i < xr.viewCount; i++)
				{
					Matrix4x4 viewProjMatrix2 = GL.GetGPUProjectionMatrix(data.cameraData.GetProjectionMatrixNoJitter(i), renderIntoTexture: true) * data.cameraData.GetViewMatrix(i);
					LensFlareCommonSRP.ComputeOcclusion(data.material, camera, xr, i, data.width, data.height, data.usePanini, data.paniniDistance, data.paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix2, ctx.cmd, taaEnabled: false, hasCloudLayer: false, null, null);
				}
			}
		});
	}

	public void RenderLensFlareDataDriven(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, in TextureHandle destination, in TextureDesc srcDesc)
	{
		LensFlarePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<LensFlarePassData>("Lens Flare Data Driven Pass", out passData, ProfilingSampler.Get(URPProfileId.LensFlareDataDriven), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1171);
		passData.destinationTexture = destination;
		unsafeRenderGraphBuilder.UseTexture(in destination, AccessFlags.Write);
		passData.cameraData = cameraData;
		passData.material = m_Materials.lensFlareDataDriven;
		passData.width = srcDesc.width;
		passData.height = srcDesc.height;
		passData.viewport.x = 0f;
		passData.viewport.y = 0f;
		passData.viewport.width = srcDesc.width;
		passData.viewport.height = srcDesc.height;
		if (m_PaniniProjection.IsActive())
		{
			passData.usePanini = true;
			passData.paniniDistance = m_PaniniProjection.distance.value;
			passData.paniniCropToFit = m_PaniniProjection.cropToFit.value;
		}
		else
		{
			passData.usePanini = false;
			passData.paniniDistance = 1f;
			passData.paniniCropToFit = 1f;
		}
		if (LensFlareCommonSRP.IsOcclusionRTCompatible())
		{
			TextureHandle textureHandle = renderGraph.ImportTexture(LensFlareCommonSRP.occlusionRT);
			unsafeRenderGraphBuilder.UseTexture(in textureHandle);
		}
		else
		{
			TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;
			unsafeRenderGraphBuilder.UseTexture(in cameraDepthTexture);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(LensFlarePassData data, UnsafeGraphContext ctx)
		{
			Camera camera = data.cameraData.camera;
			XRPass xr = data.cameraData.xr;
			if (!xr.enabled || (xr.enabled && !xr.singlePassEnabled))
			{
				Matrix4x4 viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
				LensFlareCommonSRP.DoLensFlareDataDrivenCommon(data.material, data.cameraData.camera, data.viewport, xr, data.cameraData.xr.multipassId, data.width, data.height, data.usePanini, data.paniniDistance, data.paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, ctx.cmd, taaEnabled: false, hasCloudLayer: false, null, null, data.destinationTexture, (Light light, Camera cam, Vector3 wo) => GetLensFlareLightAttenuation(light, cam, wo), debugView: false);
			}
			else
			{
				for (int i = 0; i < xr.viewCount; i++)
				{
					Matrix4x4 viewProjMatrix2 = GL.GetGPUProjectionMatrix(data.cameraData.GetProjectionMatrixNoJitter(i), renderIntoTexture: true) * data.cameraData.GetViewMatrix(i);
					LensFlareCommonSRP.DoLensFlareDataDrivenCommon(data.material, data.cameraData.camera, data.viewport, xr, data.cameraData.xr.multipassId, data.width, data.height, data.usePanini, data.paniniDistance, data.paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix2, ctx.cmd, taaEnabled: false, hasCloudLayer: false, null, null, data.destinationTexture, (Light light, Camera cam, Vector3 wo) => GetLensFlareLightAttenuation(light, cam, wo), debugView: false);
				}
			}
		});
	}

	public TextureHandle RenderLensFlareScreenSpace(RenderGraph renderGraph, Camera camera, in TextureDesc srcDesc, TextureHandle originalBloomTexture, TextureHandle screenSpaceLensFlareBloomMipTexture, bool sameInputOutputTex)
	{
		int value = (int)m_LensFlareScreenSpace.resolution.value;
		int width = Math.Max(srcDesc.width / value, 1);
		int height = Math.Max(srcDesc.height / value, 1);
		TextureDesc desc = GetCompatibleDescriptor(srcDesc, width, height, m_DefaultColorFormat);
		TextureHandle streakTmpTexture = CreateCompatibleTexture(renderGraph, in desc, "_StreakTmpTexture", clear: true, FilterMode.Bilinear);
		TextureHandle streakTmpTexture2 = CreateCompatibleTexture(renderGraph, in desc, "_StreakTmpTexture2", clear: true, FilterMode.Bilinear);
		TextureHandle result = CreateCompatibleTexture(renderGraph, in desc, "_LensFlareScreenSpace", clear: true, FilterMode.Bilinear);
		LensFlareScreenSpacePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<LensFlareScreenSpacePassData>("Blit Lens Flare Screen Space", out passData, ProfilingSampler.Get(URPProfileId.LensFlareScreenSpace), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1292);
		passData.streakTmpTexture = streakTmpTexture;
		unsafeRenderGraphBuilder.UseTexture(in streakTmpTexture, AccessFlags.ReadWrite);
		passData.streakTmpTexture2 = streakTmpTexture2;
		unsafeRenderGraphBuilder.UseTexture(in streakTmpTexture2, AccessFlags.ReadWrite);
		passData.screenSpaceLensFlareBloomMipTexture = screenSpaceLensFlareBloomMipTexture;
		unsafeRenderGraphBuilder.UseTexture(in screenSpaceLensFlareBloomMipTexture, AccessFlags.ReadWrite);
		passData.originalBloomTexture = originalBloomTexture;
		if (!sameInputOutputTex)
		{
			unsafeRenderGraphBuilder.UseTexture(in originalBloomTexture, AccessFlags.ReadWrite);
		}
		passData.actualWidth = srcDesc.width;
		passData.actualHeight = srcDesc.height;
		passData.camera = camera;
		passData.material = m_Materials.lensFlareScreenSpace;
		passData.lensFlareScreenSpace = m_LensFlareScreenSpace;
		passData.downsample = value;
		passData.result = result;
		unsafeRenderGraphBuilder.UseTexture(in result, AccessFlags.ReadWrite);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(LensFlareScreenSpacePassData data, UnsafeGraphContext context)
		{
			UnsafeCommandBuffer cmd = context.cmd;
			Camera camera2 = data.camera;
			ScreenSpaceLensFlare lensFlareScreenSpace = data.lensFlareScreenSpace;
			LensFlareCommonSRP.DoLensFlareScreenSpaceCommon(data.material, camera2, data.actualWidth, data.actualHeight, data.lensFlareScreenSpace.tintColor.value, data.originalBloomTexture, data.screenSpaceLensFlareBloomMipTexture, null, data.streakTmpTexture, data.streakTmpTexture2, new Vector4(lensFlareScreenSpace.intensity.value, lensFlareScreenSpace.firstFlareIntensity.value, lensFlareScreenSpace.secondaryFlareIntensity.value, lensFlareScreenSpace.warpedFlareIntensity.value), new Vector4(lensFlareScreenSpace.vignetteEffect.value, lensFlareScreenSpace.startingPosition.value, lensFlareScreenSpace.scale.value, 0f), new Vector4(lensFlareScreenSpace.samples.value, lensFlareScreenSpace.sampleDimmer.value, lensFlareScreenSpace.chromaticAbberationIntensity.value, 0f), new Vector4(lensFlareScreenSpace.streaksIntensity.value, lensFlareScreenSpace.streaksLength.value, lensFlareScreenSpace.streaksOrientation.value, lensFlareScreenSpace.streaksThreshold.value), new Vector4(data.downsample, lensFlareScreenSpace.warpedFlareScale.value.x, lensFlareScreenSpace.warpedFlareScale.value.y, 0f), cmd, data.result, debugView: false);
		});
		return originalBloomTexture;
	}

	private static void ScaleViewport(RasterCommandBuffer cmd, RTHandle sourceTextureHdl, RTHandle dest, UniversalCameraData cameraData, bool hasFinalPass)
	{
		RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
		if (cameraData.xr.enabled)
		{
			renderTargetIdentifier = cameraData.xr.renderTarget;
		}
		if (dest.nameID == renderTargetIdentifier || cameraData.targetTexture != null)
		{
			if (hasFinalPass || !cameraData.resolveFinalTarget)
			{
				int width = cameraData.cameraTargetDescriptor.width;
				int height = cameraData.cameraTargetDescriptor.height;
				Rect viewport = new Rect(0f, 0f, width, height);
				cmd.SetViewport(viewport);
			}
			else
			{
				cmd.SetViewport(cameraData.pixelRect);
			}
		}
	}

	private static void ScaleViewportAndBlit(RasterCommandBuffer cmd, RTHandle sourceTextureHdl, RTHandle dest, UniversalCameraData cameraData, Material material, bool hasFinalPass)
	{
		Vector4 finalBlitScaleBias = RenderingUtils.GetFinalBlitScaleBias(sourceTextureHdl, dest, cameraData);
		ScaleViewport(cmd, sourceTextureHdl, dest, cameraData, hasFinalPass);
		Blitter.BlitTexture(cmd, sourceTextureHdl, finalBlitScaleBias, material, 0);
	}

	private static void ScaleViewportAndDrawVisibilityMesh(RasterCommandBuffer cmd, RTHandle sourceTextureHdl, RTHandle dest, UniversalCameraData cameraData, Material material, bool hasFinalPass)
	{
		Vector4 finalBlitScaleBias = RenderingUtils.GetFinalBlitScaleBias(sourceTextureHdl, dest, cameraData);
		ScaleViewport(cmd, sourceTextureHdl, dest, cameraData, hasFinalPass);
		MaterialPropertyBlock materialPropertyBlock = XRSystemUniversal.GetMaterialPropertyBlock();
		materialPropertyBlock.SetVector(Shader.PropertyToID("_BlitScaleBias"), finalBlitScaleBias);
		materialPropertyBlock.SetTexture(Shader.PropertyToID("_BlitTexture"), sourceTextureHdl);
		cameraData.xr.RenderVisibleMeshCustomMaterial(cmd, cameraData.xr.occlusionMeshScale, material, materialPropertyBlock, 1, cameraData.IsRenderTargetProjectionMatrixFlipped(dest));
	}

	public void RenderFinalSetup(RenderGraph renderGraph, UniversalCameraData cameraData, in TextureHandle source, in TextureHandle destination, ref FinalBlitSettings settings)
	{
		PostProcessingFinalSetupPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalSetupPassData>("Postprocessing Final Setup Pass", out passData, ProfilingSampler.Get(URPProfileId.RG_FinalSetup), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1433);
		Material scalingSetup = m_Materials.scalingSetup;
		scalingSetup.shaderKeywords = null;
		if (settings.isFxaaEnabled)
		{
			scalingSetup.EnableKeyword("_FXAA");
		}
		if (settings.isFsrEnabled)
		{
			scalingSetup.EnableKeyword(settings.hdrOperations.HasFlag(HDROutputUtils.Operation.ColorEncoding) ? "_GAMMA_20_AND_HDR_INPUT" : "_GAMMA_20");
		}
		if (settings.hdrOperations.HasFlag(HDROutputUtils.Operation.ColorEncoding))
		{
			SetupHDROutput(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, scalingSetup, settings.hdrOperations, cameraData.rendersOverlayUI);
		}
		if (settings.isAlphaOutputEnabled)
		{
			CoreUtils.SetKeyword(scalingSetup, "_ENABLE_ALPHA_OUTPUT", settings.isAlphaOutputEnabled);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.cameraData = cameraData;
		passData.material = scalingSetup;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalSetupPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			RTHandle rTHandle = data.sourceTexture;
			PostProcessUtils.SetSourceSize(cmd, rTHandle);
			bool hasFinalPass = true;
			ScaleViewportAndBlit(context.cmd, rTHandle, data.destinationTexture, data.cameraData, data.material, hasFinalPass);
		});
	}

	public void RenderFinalFSRScale(RenderGraph renderGraph, in TextureHandle source, in TextureDesc srcDesc, in TextureHandle destination, in TextureDesc dstDesc, bool enableAlphaOutput)
	{
		m_Materials.easu.shaderKeywords = null;
		PostProcessingFinalFSRScalePassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalFSRScalePassData>("Postprocessing Final FSR Scale Pass", out passData, ProfilingSampler.Get(URPProfileId.RG_FinalFSRScale), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1486);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.material = m_Materials.easu;
		passData.enableAlphaOutput = enableAlphaOutput;
		passData.fsrInputSize = new Vector2(srcDesc.width, srcDesc.height);
		passData.fsrOutputSize = new Vector2(dstDesc.width, dstDesc.height);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalFSRScalePassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			TextureHandle sourceTexture = data.sourceTexture;
			Material material = data.material;
			bool enableAlphaOutput2 = data.enableAlphaOutput;
			RTHandle rTHandle = sourceTexture;
			FSRUtils.SetEasuConstants(cmd, data.fsrInputSize, data.fsrInputSize, data.fsrOutputSize);
			CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", enableAlphaOutput2);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Blitter.BlitTexture(cmd, rTHandle, vector, material, 0);
		});
	}

	public void RenderFinalBlit(RenderGraph renderGraph, UniversalCameraData cameraData, in TextureHandle source, in TextureHandle overlayUITexture, in TextureHandle postProcessingTarget, ref FinalBlitSettings settings)
	{
		PostProcessingFinalBlitPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalBlitPassData>("Postprocessing Final Blit Pass", out passData, ProfilingSampler.Get(URPProfileId.RG_FinalBlit), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1568);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = postProcessingTarget;
		rasterRenderGraphBuilder.SetRenderAttachment(postProcessingTarget, 0);
		passData.sourceTexture = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.cameraData = cameraData;
		passData.material = m_Materials.finalPass;
		passData.settings = settings;
		if (settings.requireHDROutput && m_EnableColorEncodingIfNeeded && cameraData.rendersOverlayUI)
		{
			rasterRenderGraphBuilder.UseTexture(in overlayUITexture);
		}
		if (cameraData.xr.enabled)
		{
			bool flag = !XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster);
			rasterRenderGraphBuilder.EnableFoveatedRasterization(cameraData.xr.supportsFoveatedRendering && flag);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalBlitPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			Material material = data.material;
			bool isFxaaEnabled = data.settings.isFxaaEnabled;
			bool isFsrEnabled = data.settings.isFsrEnabled;
			bool isTaaSharpeningEnabled = data.settings.isTaaSharpeningEnabled;
			bool requireHDROutput = data.settings.requireHDROutput;
			bool resolveToDebugScreen = data.settings.resolveToDebugScreen;
			bool isAlphaOutputEnabled = data.settings.isAlphaOutputEnabled;
			RTHandle rTHandle = data.sourceTexture;
			RTHandle rTHandle2 = data.destinationTexture;
			PostProcessUtils.SetSourceSize(cmd, data.sourceTexture);
			if (isFxaaEnabled)
			{
				material.EnableKeyword("_FXAA");
			}
			if (isFsrEnabled)
			{
				float sharpnessLinear = (data.cameraData.fsrOverrideSharpness ? data.cameraData.fsrSharpness : 0.92f);
				if (data.cameraData.fsrSharpness > 0f)
				{
					material.EnableKeyword(requireHDROutput ? "_EASU_RCAS_AND_HDR_INPUT" : "_RCAS");
					FSRUtils.SetRcasConstantsLinear(cmd, sharpnessLinear);
				}
			}
			else if (isTaaSharpeningEnabled)
			{
				material.EnableKeyword("_RCAS");
				FSRUtils.SetRcasConstantsLinear(cmd, data.cameraData.taaSettings.contrastAdaptiveSharpening);
			}
			if (isAlphaOutputEnabled)
			{
				CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", isAlphaOutputEnabled);
			}
			bool flag2 = !data.cameraData.isSceneViewCamera;
			if (data.cameraData.xr.enabled)
			{
				flag2 = rTHandle2 == data.cameraData.xr.renderTarget;
			}
			flag2 = flag2 && !resolveToDebugScreen;
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			bool flag3 = flag2 && data.cameraData.targetTexture == null && SystemInfo.graphicsUVStartsAtTop;
			Vector4 vector2 = (flag3 ? new Vector4(vector.x, 0f - vector.y, 0f, vector.y) : new Vector4(vector.x, vector.y, 0f, 0f));
			cmd.SetViewport(data.cameraData.pixelRect);
			if (data.cameraData.xr.enabled && data.cameraData.xr.hasValidVisibleMesh)
			{
				MaterialPropertyBlock materialPropertyBlock = XRSystemUniversal.GetMaterialPropertyBlock();
				materialPropertyBlock.SetVector(Shader.PropertyToID("_BlitScaleBias"), vector2);
				materialPropertyBlock.SetTexture(Shader.PropertyToID("_BlitTexture"), rTHandle);
				data.cameraData.xr.RenderVisibleMeshCustomMaterial(cmd, data.cameraData.xr.occlusionMeshScale, material, materialPropertyBlock, 1, !flag3);
			}
			else
			{
				Blitter.BlitTexture(cmd, rTHandle, vector2, material, 0);
			}
		});
	}

	public void RenderFinalPassRenderGraph(RenderGraph renderGraph, ContextContainer frameData, in TextureHandle source, in TextureHandle overlayUITexture, in TextureHandle postProcessingTarget, bool enableColorEncodingIfNeeded)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Tonemapping = stack.GetComponent<Tonemapping>();
		m_FilmGrain = stack.GetComponent<FilmGrain>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		Material finalPass = m_Materials.finalPass;
		finalPass.shaderKeywords = null;
		FinalBlitSettings settings = FinalBlitSettings.Create();
		TextureDesc srcDesc = renderGraph.GetTextureDesc(in source);
		TextureDesc desc = srcDesc;
		desc.width = universalCameraData.pixelWidth;
		desc.height = universalCameraData.pixelHeight;
		m_HasFinalPass = false;
		m_IsFinalPass = true;
		m_EnableColorEncodingIfNeeded = enableColorEncodingIfNeeded;
		if (m_FilmGrain.IsActive())
		{
			finalPass.EnableKeyword("_FILM_GRAIN");
			PostProcessUtils.ConfigureFilmGrain(m_Data, m_FilmGrain, desc.width, desc.height, finalPass);
		}
		if (universalCameraData.isDitheringEnabled)
		{
			finalPass.EnableKeyword("_DITHERING");
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Data, m_DitheringTextureIndex, desc.width, desc.height, finalPass);
		}
		if (RequireSRGBConversionBlitToBackBuffer(universalCameraData.requireSrgbConversion))
		{
			finalPass.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
		}
		settings.hdrOperations = HDROutputUtils.Operation.None;
		settings.requireHDROutput = RequireHDROutput(universalCameraData);
		if (settings.requireHDROutput)
		{
			settings.hdrOperations = (m_EnableColorEncodingIfNeeded ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
			if (!universalCameraData.postProcessEnabled)
			{
				settings.hdrOperations |= HDROutputUtils.Operation.ColorConversion;
			}
			SetupHDROutput(universalCameraData.hdrDisplayInformation, universalCameraData.hdrDisplayColorGamut, finalPass, settings.hdrOperations, universalCameraData.rendersOverlayUI);
		}
		bool resolveToDebugScreen = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData)?.WriteToDebugScreenTexture(universalCameraData.resolveFinalTarget) ?? false;
		settings.resolveToDebugScreen = resolveToDebugScreen;
		settings.isAlphaOutputEnabled = universalCameraData.isAlphaOutputEnabled;
		settings.isFxaaEnabled = universalCameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		settings.isFsrEnabled = universalCameraData.imageScalingMode == ImageScalingMode.Upscaling && universalCameraData.upscalingFilter == ImageUpscalingFilter.FSR;
		settings.isTaaSharpeningEnabled = universalCameraData.IsTemporalAAEnabled() && universalCameraData.taaSettings.contrastAdaptiveSharpening > 0f && !settings.isFsrEnabled && !universalCameraData.IsSTPEnabled();
		TextureDesc desc2 = srcDesc;
		if (!settings.requireHDROutput)
		{
			desc2.format = UniversalRenderPipeline.MakeUnormRenderTextureGraphicsFormat();
		}
		TextureHandle destination = CreateCompatibleTexture(renderGraph, in desc2, "scalingSetupTarget", clear: true, FilterMode.Point);
		TextureHandle destination2 = CreateCompatibleTexture(renderGraph, in desc, "_UpscaledTexture", clear: true, FilterMode.Point);
		TextureHandle source2 = source;
		if (universalCameraData.imageScalingMode != 0)
		{
			if (settings.isFxaaEnabled || settings.isFsrEnabled)
			{
				RenderFinalSetup(renderGraph, universalCameraData, in source2, in destination, ref settings);
				source2 = destination;
				settings.isFxaaEnabled = false;
			}
			switch (universalCameraData.imageScalingMode)
			{
			case ImageScalingMode.Upscaling:
				switch (universalCameraData.upscalingFilter)
				{
				case ImageUpscalingFilter.Point:
					if (!settings.isTaaSharpeningEnabled)
					{
						finalPass.EnableKeyword("_POINT_SAMPLING");
					}
					break;
				case ImageUpscalingFilter.FSR:
					RenderFinalFSRScale(renderGraph, in source2, in srcDesc, in destination2, in desc, settings.isAlphaOutputEnabled);
					source2 = destination2;
					break;
				}
				break;
			case ImageScalingMode.Downscaling:
				settings.isTaaSharpeningEnabled = false;
				break;
			}
		}
		else if (settings.isFxaaEnabled)
		{
			finalPass.EnableKeyword("_FXAA");
		}
		RenderFinalBlit(renderGraph, universalCameraData, in source2, in overlayUITexture, in postProcessingTarget, ref settings);
	}

	private TextureHandle TryGetCachedUserLutTextureHandle(RenderGraph renderGraph)
	{
		if (m_ColorLookup.texture.value == null)
		{
			if (m_UserLut != null)
			{
				m_UserLut.Release();
				m_UserLut = null;
			}
		}
		else if (m_UserLut == null || m_UserLut.externalTexture != m_ColorLookup.texture.value)
		{
			m_UserLut?.Release();
			m_UserLut = RTHandles.Alloc(m_ColorLookup.texture.value);
		}
		if (m_UserLut == null)
		{
			return TextureHandle.nullHandle;
		}
		return renderGraph.ImportTexture(m_UserLut);
	}

	public void RenderUberPost(RenderGraph renderGraph, ContextContainer frameData, UniversalCameraData cameraData, UniversalPostProcessingData postProcessingData, in TextureHandle sourceTexture, in TextureHandle destTexture, in TextureHandle lutTexture, in TextureHandle bloomTexture, in TextureHandle overlayUITexture, bool requireHDROutput, bool enableAlphaOutput, bool resolveToDebugScreen, bool hasFinalPass)
	{
		Material uber = m_Materials.uber;
		bool isHdrGrading = postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.lutSize;
		int num = lutSize * lutSize;
		float w = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
		Vector4 lutParams = new Vector4(1f / (float)num, 1f / (float)lutSize, (float)lutSize - 1f, w);
		TextureHandle userLutTexture = TryGetCachedUserLutTextureHandle(renderGraph);
		Vector4 userLutParams = ((!m_ColorLookup.IsActive()) ? Vector4.zero : new Vector4(1f / (float)m_ColorLookup.texture.value.width, 1f / (float)m_ColorLookup.texture.value.height, (float)m_ColorLookup.texture.value.height - 1f, m_ColorLookup.contribution.value));
		UberPostPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<UberPostPassData>("Blit Post Processing", out passData, ProfilingSampler.Get(URPProfileId.RG_UberPost), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 1889);
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		if (cameraData.xr.enabled)
		{
			bool flag = cameraData.xrUniversal.canFoveateIntermediatePasses || universalResourceData.isActiveTargetBackBuffer;
			flag &= !XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster);
			rasterRenderGraphBuilder.EnableFoveatedRasterization(cameraData.xr.supportsFoveatedRendering && flag);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = destTexture;
		rasterRenderGraphBuilder.SetRenderAttachment(destTexture, 0);
		passData.sourceTexture = sourceTexture;
		rasterRenderGraphBuilder.UseTexture(in sourceTexture);
		passData.lutTexture = lutTexture;
		rasterRenderGraphBuilder.UseTexture(in lutTexture);
		passData.lutParams = lutParams;
		passData.userLutTexture = userLutTexture;
		if (userLutTexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in userLutTexture);
		}
		if (m_Bloom.IsActive())
		{
			rasterRenderGraphBuilder.UseTexture(in bloomTexture);
			passData.bloomTexture = bloomTexture;
		}
		if (requireHDROutput && m_EnableColorEncodingIfNeeded && overlayUITexture.IsValid())
		{
			rasterRenderGraphBuilder.UseTexture(in overlayUITexture);
		}
		passData.userLutParams = userLutParams;
		passData.cameraData = cameraData;
		passData.material = uber;
		passData.toneMappingMode = m_Tonemapping.mode.value;
		passData.isHdrGrading = isHdrGrading;
		passData.enableAlphaOutput = enableAlphaOutput;
		passData.hasFinalPass = hasFinalPass;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(UberPostPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			_ = data.cameraData.camera;
			Material material = data.material;
			RTHandle sourceTextureHdl = data.sourceTexture;
			material.SetTexture(ShaderConstants._InternalLut, data.lutTexture);
			material.SetVector(ShaderConstants._Lut_Params, data.lutParams);
			material.SetTexture(ShaderConstants._UserLut, data.userLutTexture);
			material.SetVector(ShaderConstants._UserLut_Params, data.userLutParams);
			if (data.bloomTexture.IsValid())
			{
				material.SetTexture(ShaderConstants._Bloom_Texture, data.bloomTexture);
			}
			if (data.isHdrGrading)
			{
				material.EnableKeyword("_HDR_GRADING");
			}
			else
			{
				switch (data.toneMappingMode)
				{
				case TonemappingMode.Neutral:
					material.EnableKeyword("_TONEMAP_NEUTRAL");
					break;
				case TonemappingMode.ACES:
					material.EnableKeyword("_TONEMAP_ACES");
					break;
				}
			}
			CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", data.enableAlphaOutput);
			if (data.cameraData.xr.enabled && data.cameraData.xr.hasValidVisibleMesh)
			{
				ScaleViewportAndDrawVisibilityMesh(cmd, sourceTextureHdl, data.destinationTexture, data.cameraData, material, data.hasFinalPass);
			}
			else
			{
				ScaleViewportAndBlit(cmd, sourceTextureHdl, data.destinationTexture, data.cameraData, material, data.hasFinalPass);
			}
		});
	}

	public void RenderPostProcessingRenderGraph(RenderGraph renderGraph, ContextContainer frameData, in TextureHandle activeCameraColorTexture, in TextureHandle lutTexture, in TextureHandle overlayUITexture, in TextureHandle postProcessingTarget, bool hasFinalPass, bool resolveToDebugScreen, bool enableColorEndingIfNeeded)
	{
		UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
		frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		UniversalPostProcessingData universalPostProcessingData = frameData.Get<UniversalPostProcessingData>();
		VolumeStack stack = VolumeManager.instance.stack;
		m_DepthOfField = stack.GetComponent<DepthOfField>();
		m_MotionBlur = stack.GetComponent<MotionBlur>();
		m_PaniniProjection = stack.GetComponent<PaniniProjection>();
		m_Bloom = stack.GetComponent<Bloom>();
		m_LensFlareScreenSpace = stack.GetComponent<ScreenSpaceLensFlare>();
		m_LensDistortion = stack.GetComponent<LensDistortion>();
		m_ChromaticAberration = stack.GetComponent<ChromaticAberration>();
		m_Vignette = stack.GetComponent<Vignette>();
		m_ColorLookup = stack.GetComponent<ColorLookup>();
		m_ColorAdjustments = stack.GetComponent<ColorAdjustments>();
		m_Tonemapping = stack.GetComponent<Tonemapping>();
		m_FilmGrain = stack.GetComponent<FilmGrain>();
		m_UseFastSRGBLinearConversion = universalPostProcessingData.useFastSRGBLinearConversion;
		m_SupportDataDrivenLensFlare = universalPostProcessingData.supportDataDrivenLensFlare;
		m_SupportScreenSpaceLensFlare = universalPostProcessingData.supportScreenSpaceLensFlare;
		m_HasFinalPass = hasFinalPass;
		m_EnableColorEncodingIfNeeded = enableColorEndingIfNeeded;
		ref ScriptableRenderer renderer = ref universalCameraData.renderer;
		bool isSceneViewCamera = universalCameraData.isSceneViewCamera;
		bool flag = universalCameraData.isStopNaNEnabled && m_Materials.stopNaN != null;
		bool flag2 = universalCameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing;
		Material material = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? m_Materials.gaussianDepthOfField : m_Materials.bokehDepthOfField);
		bool flag3 = m_DepthOfField.IsActive() && !isSceneViewCamera && material != null;
		bool flag4 = !LensFlareCommonSRP.Instance.IsEmpty() && m_SupportDataDrivenLensFlare;
		bool flag5 = m_LensFlareScreenSpace.IsActive() && m_SupportScreenSpaceLensFlare;
		bool flag6 = m_MotionBlur.IsActive() && !isSceneViewCamera;
		bool flag7 = m_PaniniProjection.IsActive() && !isSceneViewCamera;
		if (universalCameraData.imageScalingMode == ImageScalingMode.Upscaling)
		{
			_ = universalCameraData.upscalingFilter == ImageUpscalingFilter.FSR;
		}
		else
			_ = 0;
		flag6 = flag6 && Application.isPlaying;
		if (flag6 && m_MotionBlur.mode.value == MotionBlurMode.CameraAndObjects)
		{
			flag6 &= renderer.SupportsMotionVectors();
			if (!flag6)
			{
				string message = "Disabling Motion Blur for Camera And Objects because the renderer does not implement motion vectors.";
				if (Time.frameCount % 60 == 0)
				{
					Debug.LogWarning(message);
				}
			}
		}
		bool flag8 = universalCameraData.IsTemporalAAEnabled();
		bool flag9 = universalCameraData.IsSTPRequested();
		bool flag10 = flag8 && flag9;
		if (!flag8 && universalCameraData.IsTemporalAARequested())
		{
			TemporalAA.ValidateAndWarn(universalCameraData, flag9);
		}
		PostFXSetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostFXSetupPassData>("Setup PostFX passes", out passData, ProfilingSampler.Get(URPProfileId.RG_SetupPostFX), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\PostProcessPassRenderGraph.cs", 2053))
		{
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PostFXSetupPassData data, RasterGraphContext context)
			{
				context.cmd.SetGlobalMatrix(ShaderConstants._FullscreenProjMat, GL.GetGPUProjectionMatrix(Matrix4x4.identity, renderIntoTexture: true));
			});
		}
		TextureHandle activeCameraColor = activeCameraColorTexture;
		if (flag)
		{
			RenderStopNaN(renderGraph, in activeCameraColor, out var stopNaNTarget);
			activeCameraColor = stopNaNTarget;
		}
		if (flag2)
		{
			RenderSMAA(renderGraph, resourceData, universalCameraData.antialiasingQuality, in activeCameraColor, out var SMAATarget);
			activeCameraColor = SMAATarget;
		}
		if (flag3)
		{
			RenderDoF(renderGraph, resourceData, universalCameraData, in activeCameraColor, out var destination);
			activeCameraColor = destination;
		}
		if (flag8)
		{
			if (flag10)
			{
				RenderSTP(renderGraph, resourceData, universalCameraData, ref activeCameraColor, out var destination2);
				activeCameraColor = destination2;
			}
			else
			{
				RenderTemporalAA(renderGraph, resourceData, universalCameraData, ref activeCameraColor, out var destination3);
				activeCameraColor = destination3;
			}
		}
		if (flag6)
		{
			RenderMotionBlur(renderGraph, resourceData, universalCameraData, in activeCameraColor, out var destination4);
			activeCameraColor = destination4;
		}
		if (flag7)
		{
			RenderPaniniProjection(renderGraph, universalCameraData.camera, in activeCameraColor, out var destination5);
			activeCameraColor = destination5;
		}
		m_Materials.uber.shaderKeywords = null;
		TextureDesc bloomSourceDesc = activeCameraColor.GetDescriptor(renderGraph);
		TextureHandle destination6 = TextureHandle.nullHandle;
		if (m_Bloom.IsActive() || flag5)
		{
			RenderBloomTexture(renderGraph, in activeCameraColor, out destination6, universalCameraData.isAlphaOutputEnabled);
			if (flag5)
			{
				int num = CalcBloomMipCount(m_Bloom, CalcBloomResolution(m_Bloom, in bloomSourceDesc));
				int num2 = Mathf.Clamp(max: Mathf.Clamp(num - 1, 0, m_Bloom.maxIterations.value / 2), value: m_LensFlareScreenSpace.bloomMip.value, min: 0);
				TextureHandle screenSpaceLensFlareBloomMipTexture = _BloomMipUp[num2];
				bool sameInputOutputTex = num2 == 0;
				if (num == 1)
				{
					screenSpaceLensFlareBloomMipTexture = _BloomMipDown[0];
				}
				destination6 = RenderLensFlareScreenSpace(renderGraph, universalCameraData.camera, in bloomSourceDesc, destination6, screenSpaceLensFlareBloomMipTexture, sameInputOutputTex);
			}
			UberPostSetupBloomPass(renderGraph, m_Materials.uber, in bloomSourceDesc);
		}
		if (flag4)
		{
			LensFlareDataDrivenComputeOcclusion(renderGraph, resourceData, universalCameraData, in bloomSourceDesc);
			RenderLensFlareDataDriven(renderGraph, resourceData, universalCameraData, in activeCameraColor, in bloomSourceDesc);
		}
		SetupLensDistortion(m_Materials.uber, isSceneViewCamera);
		SetupChromaticAberration(m_Materials.uber);
		SetupVignette(m_Materials.uber, universalCameraData.xr, bloomSourceDesc.width, bloomSourceDesc.height);
		SetupGrain(universalCameraData, m_Materials.uber);
		SetupDithering(universalCameraData, m_Materials.uber);
		if (RequireSRGBConversionBlitToBackBuffer(universalCameraData.requireSrgbConversion))
		{
			m_Materials.uber.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
		}
		if (m_UseFastSRGBLinearConversion)
		{
			m_Materials.uber.EnableKeyword("_USE_FAST_SRGB_LINEAR_CONVERSION");
		}
		bool flag11 = RequireHDROutput(universalCameraData);
		if (flag11)
		{
			HDROutputUtils.Operation hdrOperations = ((!m_HasFinalPass && m_EnableColorEncodingIfNeeded) ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
			SetupHDROutput(universalCameraData.hdrDisplayInformation, universalCameraData.hdrDisplayColorGamut, m_Materials.uber, hdrOperations, universalCameraData.rendersOverlayUI);
		}
		bool isAlphaOutputEnabled = universalCameraData.isAlphaOutputEnabled;
		ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		RenderUberPost(renderGraph, frameData, universalCameraData, universalPostProcessingData, in activeCameraColor, in postProcessingTarget, in lutTexture, in destination6, in overlayUITexture, flag11, isAlphaOutputEnabled, resolveToDebugScreen, hasFinalPass);
	}
}
