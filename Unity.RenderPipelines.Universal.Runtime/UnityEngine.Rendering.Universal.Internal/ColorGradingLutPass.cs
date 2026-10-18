using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class ColorGradingLutPass : ScriptableRenderPass
{
	private class PassData
	{
		internal UniversalCameraData cameraData;

		internal UniversalPostProcessingData postProcessingData;

		internal Material lutBuilderLdr;

		internal Material lutBuilderHdr;

		internal bool allowColorGradingACESHDR;

		internal TextureHandle internalLut;
	}

	private static class ShaderConstants
	{
		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _ColorBalance = Shader.PropertyToID("_ColorBalance");

		public static readonly int _ColorFilter = Shader.PropertyToID("_ColorFilter");

		public static readonly int _ChannelMixerRed = Shader.PropertyToID("_ChannelMixerRed");

		public static readonly int _ChannelMixerGreen = Shader.PropertyToID("_ChannelMixerGreen");

		public static readonly int _ChannelMixerBlue = Shader.PropertyToID("_ChannelMixerBlue");

		public static readonly int _HueSatCon = Shader.PropertyToID("_HueSatCon");

		public static readonly int _Lift = Shader.PropertyToID("_Lift");

		public static readonly int _Gamma = Shader.PropertyToID("_Gamma");

		public static readonly int _Gain = Shader.PropertyToID("_Gain");

		public static readonly int _Shadows = Shader.PropertyToID("_Shadows");

		public static readonly int _Midtones = Shader.PropertyToID("_Midtones");

		public static readonly int _Highlights = Shader.PropertyToID("_Highlights");

		public static readonly int _ShaHiLimits = Shader.PropertyToID("_ShaHiLimits");

		public static readonly int _SplitShadows = Shader.PropertyToID("_SplitShadows");

		public static readonly int _SplitHighlights = Shader.PropertyToID("_SplitHighlights");

		public static readonly int _CurveMaster = Shader.PropertyToID("_CurveMaster");

		public static readonly int _CurveRed = Shader.PropertyToID("_CurveRed");

		public static readonly int _CurveGreen = Shader.PropertyToID("_CurveGreen");

		public static readonly int _CurveBlue = Shader.PropertyToID("_CurveBlue");

		public static readonly int _CurveHueVsHue = Shader.PropertyToID("_CurveHueVsHue");

		public static readonly int _CurveHueVsSat = Shader.PropertyToID("_CurveHueVsSat");

		public static readonly int _CurveLumVsSat = Shader.PropertyToID("_CurveLumVsSat");

		public static readonly int _CurveSatVsSat = Shader.PropertyToID("_CurveSatVsSat");
	}

	private readonly Material m_LutBuilderLdr;

	private readonly Material m_LutBuilderHdr;

	internal readonly GraphicsFormat m_HdrLutFormat;

	internal readonly GraphicsFormat m_LdrLutFormat;

	private PassData m_PassData;

	private RTHandle m_InternalLut;

	private bool m_AllowColorGradingACESHDR = true;

	public ColorGradingLutPass(RenderPassEvent evt, PostProcessData data)
	{
		base.profilingSampler = new ProfilingSampler("Blit Color LUT");
		base.renderPassEvent = evt;
		base.overrideCameraTarget = true;
		m_LutBuilderLdr = Load(data.shaders.lutBuilderLdrPS);
		m_LutBuilderHdr = Load(data.shaders.lutBuilderHdrPS);
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Blend))
		{
			m_HdrLutFormat = GraphicsFormat.R16G16B16A16_SFloat;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
		{
			m_HdrLutFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		}
		else
		{
			m_HdrLutFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		m_LdrLutFormat = GraphicsFormat.R8G8B8A8_UNorm;
		base.useNativeRenderPass = false;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && Graphics.minOpenGLESVersion <= OpenGLESVersion.OpenGLES30 && SystemInfo.graphicsDeviceName.StartsWith("Adreno (TM) 3"))
		{
			m_AllowColorGradingACESHDR = false;
		}
		m_PassData = new PassData();
		static Material Load(Shader shader)
		{
			if (shader == null)
			{
				Debug.LogError("Missing shader. ColorGradingLutPass render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}
	}

	public void Setup(in RTHandle internalLut)
	{
		m_InternalLut = internalLut;
	}

	public void ConfigureDescriptor(in PostProcessingData postProcessingData, out RenderTextureDescriptor descriptor, out FilterMode filterMode)
	{
		UniversalPostProcessingData postProcessingData2 = postProcessingData.universalPostProcessingData;
		ConfigureDescriptor(in postProcessingData2, out descriptor, out filterMode);
	}

	public void ConfigureDescriptor(in UniversalPostProcessingData postProcessingData, out RenderTextureDescriptor descriptor, out FilterMode filterMode)
	{
		bool num = postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.lutSize;
		int width = lutSize * lutSize;
		GraphicsFormat colorFormat = (num ? m_HdrLutFormat : m_LdrLutFormat);
		descriptor = new RenderTextureDescriptor(width, lutSize, colorFormat, 0);
		descriptor.vrUsage = VRTextureUsage.None;
		filterMode = FilterMode.Bilinear;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalPostProcessingData postProcessingData = frameData.Get<UniversalPostProcessingData>();
		m_PassData.cameraData = cameraData;
		m_PassData.postProcessingData = postProcessingData;
		m_PassData.lutBuilderLdr = m_LutBuilderLdr;
		m_PassData.lutBuilderHdr = m_LutBuilderHdr;
		m_PassData.allowColorGradingACESHDR = m_AllowColorGradingACESHDR;
		if (renderingData.cameraData.xr.supportsFoveatedRendering)
		{
			renderingData.commandBuffer.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
		}
		CoreUtils.SetRenderTarget(renderingData.commandBuffer, m_InternalLut, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.None, Color.clear);
		ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData, m_InternalLut);
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData passData, RTHandle internalLutTarget)
	{
		Material lutBuilderLdr = passData.lutBuilderLdr;
		Material lutBuilderHdr = passData.lutBuilderHdr;
		bool allowColorGradingACESHDR = passData.allowColorGradingACESHDR;
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.ColorGradingLUT)))
		{
			VolumeStack stack = VolumeManager.instance.stack;
			ChannelMixer component = stack.GetComponent<ChannelMixer>();
			ColorAdjustments component2 = stack.GetComponent<ColorAdjustments>();
			ColorCurves component3 = stack.GetComponent<ColorCurves>();
			LiftGammaGain component4 = stack.GetComponent<LiftGammaGain>();
			ShadowsMidtonesHighlights component5 = stack.GetComponent<ShadowsMidtonesHighlights>();
			SplitToning component6 = stack.GetComponent<SplitToning>();
			Tonemapping component7 = stack.GetComponent<Tonemapping>();
			WhiteBalance component8 = stack.GetComponent<WhiteBalance>();
			bool num = passData.postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
			Material material = (num ? lutBuilderHdr : lutBuilderLdr);
			Vector3 vector = ColorUtils.ColorBalanceToLMSCoeffs(component8.temperature.value, component8.tint.value);
			Vector4 value = new Vector4(component2.hueShift.value / 360f, component2.saturation.value / 100f + 1f, component2.contrast.value / 100f + 1f, 0f);
			Vector4 value2 = new Vector4(component.redOutRedIn.value / 100f, component.redOutGreenIn.value / 100f, component.redOutBlueIn.value / 100f, 0f);
			Vector4 value3 = new Vector4(component.greenOutRedIn.value / 100f, component.greenOutGreenIn.value / 100f, component.greenOutBlueIn.value / 100f, 0f);
			Vector4 value4 = new Vector4(component.blueOutRedIn.value / 100f, component.blueOutGreenIn.value / 100f, component.blueOutBlueIn.value / 100f, 0f);
			Vector4 value5 = new Vector4(component5.shadowsStart.value, component5.shadowsEnd.value, component5.highlightsStart.value, component5.highlightsEnd.value);
			Vector4 inShadows = component5.shadows.value;
			Vector4 inMidtones = component5.midtones.value;
			Vector4 inHighlights = component5.highlights.value;
			(Vector4, Vector4, Vector4) tuple = ColorUtils.PrepareShadowsMidtonesHighlights(in inShadows, in inMidtones, in inHighlights);
			Vector4 item = tuple.Item1;
			Vector4 item2 = tuple.Item2;
			Vector4 item3 = tuple.Item3;
			inShadows = component4.lift.value;
			inMidtones = component4.gamma.value;
			inHighlights = component4.gain.value;
			(Vector4, Vector4, Vector4) tuple2 = ColorUtils.PrepareLiftGammaGain(in inShadows, in inMidtones, in inHighlights);
			Vector4 item4 = tuple2.Item1;
			Vector4 item5 = tuple2.Item2;
			Vector4 item6 = tuple2.Item3;
			inShadows = component6.shadows.value;
			inMidtones = component6.highlights.value;
			(Vector4, Vector4) tuple3 = ColorUtils.PrepareSplitToning(in inShadows, in inMidtones, component6.balance.value);
			Vector4 item7 = tuple3.Item1;
			Vector4 item8 = tuple3.Item2;
			int lutSize = passData.postProcessingData.lutSize;
			int num2 = lutSize * lutSize;
			material.SetVector(value: new Vector4(lutSize, 0.5f / (float)num2, 0.5f / (float)lutSize, (float)lutSize / ((float)lutSize - 1f)), nameID: ShaderConstants._Lut_Params);
			material.SetVector(ShaderConstants._ColorBalance, vector);
			material.SetVector(ShaderConstants._ColorFilter, component2.colorFilter.value.linear);
			material.SetVector(ShaderConstants._ChannelMixerRed, value2);
			material.SetVector(ShaderConstants._ChannelMixerGreen, value3);
			material.SetVector(ShaderConstants._ChannelMixerBlue, value4);
			material.SetVector(ShaderConstants._HueSatCon, value);
			material.SetVector(ShaderConstants._Lift, item4);
			material.SetVector(ShaderConstants._Gamma, item5);
			material.SetVector(ShaderConstants._Gain, item6);
			material.SetVector(ShaderConstants._Shadows, item);
			material.SetVector(ShaderConstants._Midtones, item2);
			material.SetVector(ShaderConstants._Highlights, item3);
			material.SetVector(ShaderConstants._ShaHiLimits, value5);
			material.SetVector(ShaderConstants._SplitShadows, item7);
			material.SetVector(ShaderConstants._SplitHighlights, item8);
			material.SetTexture(ShaderConstants._CurveMaster, component3.master.value.GetTexture());
			material.SetTexture(ShaderConstants._CurveRed, component3.red.value.GetTexture());
			material.SetTexture(ShaderConstants._CurveGreen, component3.green.value.GetTexture());
			material.SetTexture(ShaderConstants._CurveBlue, component3.blue.value.GetTexture());
			material.SetTexture(ShaderConstants._CurveHueVsHue, component3.hueVsHue.value.GetTexture());
			material.SetTexture(ShaderConstants._CurveHueVsSat, component3.hueVsSat.value.GetTexture());
			material.SetTexture(ShaderConstants._CurveLumVsSat, component3.lumVsSat.value.GetTexture());
			material.SetTexture(ShaderConstants._CurveSatVsSat, component3.satVsSat.value.GetTexture());
			if (num)
			{
				material.shaderKeywords = null;
				switch (component7.mode.value)
				{
				case TonemappingMode.Neutral:
					material.EnableKeyword("_TONEMAP_NEUTRAL");
					break;
				case TonemappingMode.ACES:
					material.EnableKeyword(allowColorGradingACESHDR ? "_TONEMAP_ACES" : "_TONEMAP_NEUTRAL");
					break;
				}
				if (passData.cameraData.isHDROutputActive)
				{
					UniversalRenderPipeline.GetHDROutputLuminanceParameters(passData.cameraData.hdrDisplayInformation, passData.cameraData.hdrDisplayColorGamut, component7, out var hdrOutputParameters);
					UniversalRenderPipeline.GetHDROutputGradingParameters(component7, out var hdrOutputParameters2);
					material.SetVector(ShaderPropertyId.hdrOutputLuminanceParams, hdrOutputParameters);
					material.SetVector(ShaderPropertyId.hdrOutputGradingParams, hdrOutputParameters2);
					HDROutputUtils.ConfigureHDROutput(material, passData.cameraData.hdrDisplayColorGamut, HDROutputUtils.Operation.ColorConversion);
				}
			}
			passData.cameraData.xr.StopSinglePass(cmd);
			Blitter.BlitTexture(cmd, internalLutTarget, Vector2.one, material, 0);
			passData.cameraData.xr.StartSinglePass(cmd);
		}
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, out TextureHandle internalColorLut)
	{
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalPostProcessingData postProcessingData = frameData.Get<UniversalPostProcessingData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\ColorGradingLutPass.cs", 283);
		ConfigureDescriptor(in postProcessingData, out var descriptor, out var filterMode);
		internalColorLut = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_InternalGradingLut", clear: true, filterMode);
		passData.cameraData = cameraData;
		passData.postProcessingData = postProcessingData;
		passData.internalLut = internalColorLut;
		rasterRenderGraphBuilder.SetRenderAttachment(internalColorLut, 0, AccessFlags.WriteAll);
		passData.lutBuilderLdr = m_LutBuilderLdr;
		passData.lutBuilderHdr = m_LutBuilderHdr;
		passData.allowColorGradingACESHDR = m_AllowColorGradingACESHDR;
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data, data.internalLut);
		});
	}

	public void Cleanup()
	{
		CoreUtils.Destroy(m_LutBuilderLdr);
		CoreUtils.Destroy(m_LutBuilderHdr);
	}
}
