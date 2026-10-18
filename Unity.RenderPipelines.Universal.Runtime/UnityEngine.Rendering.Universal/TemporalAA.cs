using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal;

public static class TemporalAA
{
	internal static class ShaderConstants
	{
		public static readonly int _TaaAccumulationTex = Shader.PropertyToID("_TaaAccumulationTex");

		public static readonly int _TaaMotionVectorTex = Shader.PropertyToID("_TaaMotionVectorTex");

		public static readonly int _TaaFilterWeights = Shader.PropertyToID("_TaaFilterWeights");

		public static readonly int _TaaFrameInfluence = Shader.PropertyToID("_TaaFrameInfluence");

		public static readonly int _TaaVarianceClampScale = Shader.PropertyToID("_TaaVarianceClampScale");

		public static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
	}

	internal static class ShaderKeywords
	{
		public static readonly string TAA_LOW_PRECISION_SOURCE = "TAA_LOW_PRECISION_SOURCE";
	}

	[Serializable]
	public struct Settings
	{
		[SerializeField]
		[FormerlySerializedAs("quality")]
		internal TemporalAAQuality m_Quality;

		[SerializeField]
		[FormerlySerializedAs("frameInfluence")]
		internal float m_FrameInfluence;

		[SerializeField]
		[FormerlySerializedAs("jitterScale")]
		internal float m_JitterScale;

		[SerializeField]
		[FormerlySerializedAs("mipBias")]
		internal float m_MipBias;

		[SerializeField]
		[FormerlySerializedAs("varianceClampScale")]
		internal float m_VarianceClampScale;

		[SerializeField]
		[FormerlySerializedAs("contrastAdaptiveSharpening")]
		internal float m_ContrastAdaptiveSharpening;

		[NonSerialized]
		internal int resetHistoryFrames;

		[NonSerialized]
		internal int jitterFrameCountOffset;

		public TemporalAAQuality quality
		{
			get
			{
				return m_Quality;
			}
			set
			{
				m_Quality = (TemporalAAQuality)Mathf.Clamp((int)value, 0, 4);
			}
		}

		public float baseBlendFactor
		{
			get
			{
				return 1f - m_FrameInfluence;
			}
			set
			{
				m_FrameInfluence = Mathf.Clamp01(1f - value);
			}
		}

		public float jitterScale
		{
			get
			{
				return m_JitterScale;
			}
			set
			{
				m_JitterScale = Mathf.Clamp01(value);
			}
		}

		public float mipBias
		{
			get
			{
				return m_MipBias;
			}
			set
			{
				m_MipBias = Mathf.Clamp(value, -1f, 0f);
			}
		}

		public float varianceClampScale
		{
			get
			{
				return m_VarianceClampScale;
			}
			set
			{
				m_VarianceClampScale = Mathf.Clamp(value, 0.001f, 10f);
			}
		}

		public float contrastAdaptiveSharpening
		{
			get
			{
				return m_ContrastAdaptiveSharpening;
			}
			set
			{
				m_ContrastAdaptiveSharpening = Mathf.Clamp01(value);
			}
		}

		public static Settings Create()
		{
			Settings result = default(Settings);
			result.m_Quality = TemporalAAQuality.High;
			result.m_FrameInfluence = 0.1f;
			result.m_JitterScale = 1f;
			result.m_MipBias = 0f;
			result.m_VarianceClampScale = 0.9f;
			result.m_ContrastAdaptiveSharpening = 0f;
			result.resetHistoryFrames = 0;
			result.jitterFrameCountOffset = 0;
			return result;
		}
	}

	internal delegate void JitterFunc(int frameIndex, out Vector2 jitter, out bool allowScaling);

	private class TaaPassData
	{
		internal TextureHandle dstTex;

		internal TextureHandle srcColorTex;

		internal TextureHandle srcDepthTex;

		internal TextureHandle srcMotionVectorTex;

		internal TextureHandle srcTaaAccumTex;

		internal Material material;

		internal int passIndex;

		internal float taaFrameInfluence;

		internal float taaVarianceClampScale;

		internal float[] taaFilterWeights;

		internal bool taaLowPrecisionSource;

		internal bool taaAlphaOutput;
	}

	internal static JitterFunc s_JitterFunc = CalculateJitter;

	private static readonly Vector2[] taaFilterOffsets = new Vector2[9]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2(0f, -1f),
		new Vector2(-1f, 1f),
		new Vector2(1f, -1f),
		new Vector2(1f, 1f),
		new Vector2(-1f, -1f)
	};

	private static readonly float[] taaFilterWeights = new float[taaFilterOffsets.Length + 1];

	internal static GraphicsFormat[] AccumulationFormatList = new GraphicsFormat[4]
	{
		GraphicsFormat.R16G16B16A16_SFloat,
		GraphicsFormat.B10G11R11_UFloatPack32,
		GraphicsFormat.R8G8B8A8_UNorm,
		GraphicsFormat.B8G8R8A8_UNorm
	};

	private static uint s_warnCounter = 0u;

	internal static int CalculateTaaFrameIndex(ref Settings settings)
	{
		int jitterFrameCountOffset = settings.jitterFrameCountOffset;
		return Time.frameCount + jitterFrameCountOffset;
	}

	internal static Matrix4x4 CalculateJitterMatrix(UniversalCameraData cameraData, JitterFunc jitterFunc)
	{
		Matrix4x4 result = Matrix4x4.identity;
		if (cameraData.IsTemporalAAEnabled())
		{
			int frameIndex = CalculateTaaFrameIndex(ref cameraData.taaSettings);
			float num = cameraData.cameraTargetDescriptor.width;
			float num2 = cameraData.cameraTargetDescriptor.height;
			float jitterScale = cameraData.taaSettings.jitterScale;
			jitterFunc(frameIndex, out var jitter, out var allowScaling);
			if (allowScaling)
			{
				jitter *= jitterScale;
			}
			float x = jitter.x * (2f / num);
			float y = jitter.y * (2f / num2);
			result = Matrix4x4.Translate(new Vector3(x, y, 0f));
		}
		return result;
	}

	internal static void CalculateJitter(int frameIndex, out Vector2 jitter, out bool allowScaling)
	{
		float x = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 2) - 0.5f;
		float y = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 3) - 0.5f;
		jitter = new Vector2(x, y);
		allowScaling = true;
	}

	internal static float[] CalculateFilterWeights(ref Settings settings)
	{
		int frameIndex = CalculateTaaFrameIndex(ref settings);
		float num = 0f;
		for (int i = 0; i < 9; i++)
		{
			CalculateJitter(frameIndex, out var jitter, out var _);
			jitter *= settings.jitterScale;
			float num2 = taaFilterOffsets[i].x - jitter.x;
			float num3 = taaFilterOffsets[i].y - jitter.y;
			float num4 = num2 * num2 + num3 * num3;
			taaFilterWeights[i] = Mathf.Exp(-2.2727273f * num4);
			num += taaFilterWeights[i];
		}
		for (int j = 0; j < 9; j++)
		{
			taaFilterWeights[j] /= num;
		}
		return taaFilterWeights;
	}

	internal static RenderTextureDescriptor TemporalAADescFromCameraDesc(ref RenderTextureDescriptor cameraDesc)
	{
		RenderTextureDescriptor result = cameraDesc;
		result.width = cameraDesc.width;
		result.height = cameraDesc.height;
		result.msaaSamples = 1;
		result.volumeDepth = cameraDesc.volumeDepth;
		result.mipCount = 0;
		result.graphicsFormat = cameraDesc.graphicsFormat;
		result.sRGB = false;
		result.depthStencilFormat = GraphicsFormat.None;
		result.dimension = cameraDesc.dimension;
		result.vrUsage = cameraDesc.vrUsage;
		result.memoryless = RenderTextureMemoryless.None;
		result.useMipMap = false;
		result.autoGenerateMips = false;
		result.enableRandomWrite = false;
		result.bindMS = false;
		result.useDynamicScale = false;
		if (!SystemInfo.IsFormatSupported(result.graphicsFormat, GraphicsFormatUsage.Render))
		{
			result.graphicsFormat = GraphicsFormat.None;
			for (int i = 0; i < AccumulationFormatList.Length; i++)
			{
				if (SystemInfo.IsFormatSupported(AccumulationFormatList[i], GraphicsFormatUsage.Render))
				{
					result.graphicsFormat = AccumulationFormatList[i];
					break;
				}
			}
		}
		return result;
	}

	internal static string ValidateAndWarn(UniversalCameraData cameraData, bool isSTPRequested = false)
	{
		string text = null;
		if (text == null && !cameraData.postProcessEnabled)
		{
			text = "because camera has post-processing disabled.";
		}
		if (cameraData.taaHistory == null)
		{
			text = "due to invalid persistent data.";
		}
		if (text == null && cameraData.cameraTargetDescriptor.msaaSamples != 1)
		{
			text = ((cameraData.xr == null || !cameraData.xr.enabled) ? "because MSAA is on. Turn MSAA off on the camera or current URP Asset." : "because MSAA is on. MSAA must be disabled globally for all cameras in XR mode.");
		}
		if (text == null && cameraData.camera.TryGetComponent<UniversalAdditionalCameraData>(out var component) && (component.renderType == CameraRenderType.Overlay || component.cameraStack.Count > 0))
		{
			text = "because camera is stacked.";
		}
		if (text == null && cameraData.camera.allowDynamicResolution)
		{
			text = "because camera has dynamic resolution enabled. You can use a constant render scale instead.";
		}
		if (text == null && !cameraData.renderer.SupportsMotionVectors())
		{
			text = "because the renderer does not implement motion vectors. Motion vectors are required.";
		}
		if (s_warnCounter % 60 == 0)
		{
			Debug.LogWarning("Disabling TAA " + (isSTPRequested ? "and STP " : "") + text);
		}
		s_warnCounter++;
		return text;
	}

	internal static void ExecutePass(CommandBuffer cmd, Material taaMaterial, ref CameraData cameraData, RTHandle source, RTHandle destination, RenderTexture motionVectors)
	{
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.TemporalAA)))
		{
			int num = 0;
			num = cameraData.xr.multipassId;
			bool flag = cameraData.taaHistory.GetAccumulationVersion(num) != Time.frameCount;
			RTHandle accumulationTexture = cameraData.taaHistory.GetAccumulationTexture(num);
			taaMaterial.SetTexture(ShaderConstants._TaaAccumulationTex, accumulationTexture);
			taaMaterial.SetTexture(ShaderConstants._TaaMotionVectorTex, flag ? ((Texture)motionVectors) : ((Texture)Texture2D.blackTexture));
			ref Settings taaSettings = ref cameraData.taaSettings;
			float value = ((taaSettings.resetHistoryFrames == 0) ? taaSettings.m_FrameInfluence : 1f);
			taaMaterial.SetFloat(ShaderConstants._TaaFrameInfluence, value);
			taaMaterial.SetFloat(ShaderConstants._TaaVarianceClampScale, taaSettings.varianceClampScale);
			if (taaSettings.quality == TemporalAAQuality.VeryHigh)
			{
				taaMaterial.SetFloatArray(ShaderConstants._TaaFilterWeights, CalculateFilterWeights(ref taaSettings));
			}
			GraphicsFormat graphicsFormat = accumulationTexture.rt.graphicsFormat;
			if (graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm || graphicsFormat == GraphicsFormat.B10G11R11_UFloatPack32)
			{
				taaMaterial.EnableKeyword(ShaderKeywords.TAA_LOW_PRECISION_SOURCE);
			}
			else
			{
				taaMaterial.DisableKeyword(ShaderKeywords.TAA_LOW_PRECISION_SOURCE);
			}
			CoreUtils.SetKeyword(taaMaterial, "_ENABLE_ALPHA_OUTPUT", cameraData.isAlphaOutputEnabled);
			Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, taaMaterial, (int)taaSettings.quality);
			if (flag)
			{
				int pass = taaMaterial.shader.passCount - 1;
				Blitter.BlitCameraTexture(cmd, destination, accumulationTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, taaMaterial, pass);
				cameraData.taaHistory.SetAccumulationVersion(num, Time.frameCount);
			}
		}
	}

	internal static void Render(RenderGraph renderGraph, Material taaMaterial, UniversalCameraData cameraData, ref TextureHandle srcColor, ref TextureHandle srcDepth, ref TextureHandle srcMotionVectors, ref TextureHandle dstColor)
	{
		int num = 0;
		num = cameraData.xr.multipassId;
		ref Settings taaSettings = ref cameraData.taaSettings;
		bool flag = cameraData.taaHistory.GetAccumulationVersion(num) != Time.frameCount;
		float taaFrameInfluence = ((taaSettings.resetHistoryFrames == 0) ? taaSettings.m_FrameInfluence : 1f);
		RTHandle accumulationTexture = cameraData.taaHistory.GetAccumulationTexture(num);
		TextureHandle input = renderGraph.ImportTexture(accumulationTexture);
		TextureHandle input2 = (flag ? srcMotionVectors : renderGraph.defaultResources.blackTexture);
		TaaPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<TaaPassData>("Temporal Anti-aliasing", out passData, ProfilingSampler.Get(URPProfileId.RG_TAA), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\TemporalAA.cs", 484))
		{
			passData.dstTex = dstColor;
			rasterRenderGraphBuilder.SetRenderAttachment(dstColor, 0);
			passData.srcColorTex = srcColor;
			rasterRenderGraphBuilder.UseTexture(in srcColor);
			passData.srcDepthTex = srcDepth;
			rasterRenderGraphBuilder.UseTexture(in srcDepth);
			passData.srcMotionVectorTex = input2;
			rasterRenderGraphBuilder.UseTexture(in input2);
			passData.srcTaaAccumTex = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.material = taaMaterial;
			passData.passIndex = (int)taaSettings.quality;
			passData.taaFrameInfluence = taaFrameInfluence;
			passData.taaVarianceClampScale = taaSettings.varianceClampScale;
			if (taaSettings.quality == TemporalAAQuality.VeryHigh)
			{
				passData.taaFilterWeights = CalculateFilterWeights(ref taaSettings);
			}
			else
			{
				passData.taaFilterWeights = null;
			}
			GraphicsFormat graphicsFormat = accumulationTexture.rt.graphicsFormat;
			if (graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm || graphicsFormat == GraphicsFormat.B10G11R11_UFloatPack32)
			{
				passData.taaLowPrecisionSource = true;
			}
			else
			{
				passData.taaLowPrecisionSource = false;
			}
			passData.taaAlphaOutput = cameraData.isAlphaOutputEnabled;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(TaaPassData data, RasterGraphContext context)
			{
				data.material.SetFloat(ShaderConstants._TaaFrameInfluence, data.taaFrameInfluence);
				data.material.SetFloat(ShaderConstants._TaaVarianceClampScale, data.taaVarianceClampScale);
				data.material.SetTexture(ShaderConstants._TaaAccumulationTex, data.srcTaaAccumTex);
				data.material.SetTexture(ShaderConstants._TaaMotionVectorTex, data.srcMotionVectorTex);
				data.material.SetTexture(ShaderConstants._CameraDepthTexture, data.srcDepthTex);
				CoreUtils.SetKeyword(data.material, ShaderKeywords.TAA_LOW_PRECISION_SOURCE, data.taaLowPrecisionSource);
				CoreUtils.SetKeyword(data.material, "_ENABLE_ALPHA_OUTPUT", data.taaAlphaOutput);
				if (data.taaFilterWeights != null)
				{
					data.material.SetFloatArray(ShaderConstants._TaaFilterWeights, data.taaFilterWeights);
				}
				Blitter.BlitTexture(context.cmd, data.srcColorTex, Vector2.one, data.material, data.passIndex);
			});
		}
		if (!flag)
		{
			return;
		}
		int passIndex = taaMaterial.shader.passCount - 1;
		TaaPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<TaaPassData>("Temporal Anti-aliasing Copy History", out passData2, ProfilingSampler.Get(URPProfileId.RG_TAACopyHistory), ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\TemporalAA.cs", 543))
		{
			passData2.dstTex = input;
			rasterRenderGraphBuilder2.SetRenderAttachment(input, 0);
			passData2.srcColorTex = dstColor;
			rasterRenderGraphBuilder2.UseTexture(in dstColor);
			passData2.material = taaMaterial;
			passData2.passIndex = passIndex;
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(TaaPassData data, RasterGraphContext context)
			{
				Blitter.BlitTexture(context.cmd, data.srcColorTex, Vector2.one, data.material, data.passIndex);
			});
		}
		cameraData.taaHistory.SetAccumulationVersion(num, Time.frameCount);
	}
}
