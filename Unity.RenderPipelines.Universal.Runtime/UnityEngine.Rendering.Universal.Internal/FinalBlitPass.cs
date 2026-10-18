using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class FinalBlitPass : ScriptableRenderPass
{
	private static class BlitPassNames
	{
		public const string NearestSampler = "NearestDebugDraw";

		public const string BilinearSampler = "BilinearDebugDraw";
	}

	private enum BlitType
	{
		Core,
		HDR,
		Count
	}

	private struct BlitMaterialData
	{
		public Material material;

		public int nearestSamplerPass;

		public int bilinearSamplerPass;
	}

	private class PassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal int sourceID;

		internal Vector4 hdrOutputLuminanceParams;

		internal bool requireSrgbConversion;

		internal bool enableAlphaOutput;

		internal BlitMaterialData blitMaterialData;

		internal UniversalCameraData cameraData;
	}

	private RTHandle m_Source;

	private PassData m_PassData;

	private static readonly int s_CameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	private BlitMaterialData[] m_BlitMaterialData;

	public FinalBlitPass(RenderPassEvent evt, Material blitMaterial, Material blitHDRMaterial)
	{
		base.profilingSampler = ProfilingSampler.Get(URPProfileId.BlitFinalToBackBuffer);
		base.useNativeRenderPass = false;
		m_PassData = new PassData();
		base.renderPassEvent = evt;
		m_BlitMaterialData = new BlitMaterialData[2];
		for (int i = 0; i < 2; i++)
		{
			m_BlitMaterialData[i].material = ((i == 0) ? blitMaterial : blitHDRMaterial);
			m_BlitMaterialData[i].nearestSamplerPass = m_BlitMaterialData[i].material?.FindPass("NearestDebugDraw") ?? (-1);
			m_BlitMaterialData[i].bilinearSamplerPass = m_BlitMaterialData[i].material?.FindPass("BilinearDebugDraw") ?? (-1);
		}
	}

	public void Dispose()
	{
	}

	[Obsolete("Use RTHandles for colorHandle", true)]
	public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
	{
		throw new NotSupportedException("Setup with RenderTargetHandle has been deprecated. Use it with RTHandles instead.");
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, RTHandle colorHandle)
	{
		m_Source = colorHandle;
	}

	private static void SetupHDROutput(ColorGamut hdrDisplayColorGamut, Material material, HDROutputUtils.Operation hdrOperation, Vector4 hdrOutputParameters, bool rendersOverlayUI)
	{
		material.SetVector(ShaderPropertyId.hdrOutputLuminanceParams, hdrOutputParameters);
		HDROutputUtils.ConfigureHDROutput(material, hdrDisplayColorGamut, hdrOperation);
		CoreUtils.SetKeyword(material, "_HDR_OVERLAY", rendersOverlayUI);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		UniversalCameraData universalCameraData = renderingData.frameData.Get<UniversalCameraData>();
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		if (activeDebugHandler != null && activeDebugHandler.WriteToDebugScreenTexture(universalCameraData.resolveFinalTarget))
		{
			ConfigureTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		UniversalCameraData universalCameraData = renderingData.frameData.Get<UniversalCameraData>();
		bool isHDROutputActive = renderingData.cameraData.isHDROutputActive;
		bool enableAlphaOutput = false;
		InitPassData(universalCameraData, ref m_PassData, isHDROutputActive ? BlitType.HDR : BlitType.Core, enableAlphaOutput);
		if (m_PassData.blitMaterialData.material == null)
		{
			Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_PassData.blitMaterialData, GetType().Name);
			return;
		}
		RenderTargetIdentifier cameraTargetIdentifier = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(universalCameraData);
		bool flag = activeDebugHandler?.WriteToDebugScreenTexture(universalCameraData.resolveFinalTarget) ?? false;
		RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTargetIdentifier);
		RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		if (m_Source == universalCameraData.renderer.GetCameraColorFrontBuffer(commandBuffer))
		{
			m_Source = renderingData.cameraData.renderer.cameraColorTargetHandle;
		}
		using (new ProfilingScope(commandBuffer, base.profilingSampler))
		{
			m_PassData.blitMaterialData.material.enabledKeywords = null;
			commandBuffer.SetKeyword(in ShaderGlobalKeywords.LinearToSRGBConversion, universalCameraData.requireSrgbConversion);
			if (isHDROutputActive)
			{
				Tonemapping component = VolumeManager.instance.stack.GetComponent<Tonemapping>();
				UniversalRenderPipeline.GetHDROutputLuminanceParameters(universalCameraData.hdrDisplayInformation, universalCameraData.hdrDisplayColorGamut, component, out var hdrOutputParameters);
				HDROutputUtils.Operation operation = HDROutputUtils.Operation.None;
				if (activeDebugHandler == null || !activeDebugHandler.HDRDebugViewIsActive(universalCameraData.resolveFinalTarget))
				{
					operation |= HDROutputUtils.Operation.ColorEncoding;
				}
				if (!universalCameraData.postProcessEnabled)
				{
					operation |= HDROutputUtils.Operation.ColorConversion;
				}
				SetupHDROutput(universalCameraData.hdrDisplayColorGamut, m_PassData.blitMaterialData.material, operation, hdrOutputParameters, universalCameraData.rendersOverlayUI);
			}
			if (flag)
			{
				RenderTexture rt = m_Source.rt;
				int pass = (((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? m_PassData.blitMaterialData.bilinearSamplerPass : m_PassData.blitMaterialData.nearestSamplerPass);
				Vector2 vector = (m_Source.useScaling ? new Vector2(m_Source.rtHandleProperties.rtHandleScale.x, m_Source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(commandBuffer, m_Source, vector, m_PassData.blitMaterialData.material, pass);
				universalCameraData.renderer.ConfigureCameraTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
				return;
			}
			if (GL.wireframe && universalCameraData.isSceneViewCamera)
			{
				commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
				commandBuffer.Blit(m_Source.nameID, s_RTHandleWrapper.nameID);
				return;
			}
			RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
			if (!universalCameraData.isSceneViewCamera && !universalCameraData.isDefaultViewport)
			{
				loadAction = RenderBufferLoadAction.Load;
			}
			if (universalCameraData.xr.enabled)
			{
				loadAction = RenderBufferLoadAction.Load;
			}
			CoreUtils.SetRenderTarget(renderingData.commandBuffer, s_RTHandleWrapper.nameID, loadAction, RenderBufferStoreAction.Store, ClearFlag.None, Color.clear);
			ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData, m_Source, s_RTHandleWrapper, universalCameraData);
			universalCameraData.renderer.ConfigureCameraTarget(s_RTHandleWrapper, s_RTHandleWrapper);
		}
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData data, RTHandle source, RTHandle destination, UniversalCameraData cameraData)
	{
		bool flag = !cameraData.isSceneViewCamera;
		if (cameraData.xr.enabled)
		{
			flag = new RenderTargetIdentifier(destination.nameID, 0, CubemapFace.Unknown, -1) == new RenderTargetIdentifier(cameraData.xr.renderTarget, 0, CubemapFace.Unknown, -1);
		}
		Vector4 finalBlitScaleBias = RenderingUtils.GetFinalBlitScaleBias(source, destination, cameraData);
		if (flag)
		{
			cmd.SetViewport(cameraData.pixelRect);
		}
		cmd.SetWireframe(enable: false);
		CoreUtils.SetKeyword(data.blitMaterialData.material, "_ENABLE_ALPHA_OUTPUT", data.enableAlphaOutput);
		RenderTexture rt = source.rt;
		int pass = (((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? data.blitMaterialData.bilinearSamplerPass : data.blitMaterialData.nearestSamplerPass);
		Blitter.BlitTexture(cmd, source, finalBlitScaleBias, data.blitMaterialData.material, pass);
	}

	private void InitPassData(UniversalCameraData cameraData, ref PassData passData, BlitType blitType, bool enableAlphaOutput)
	{
		passData.cameraData = cameraData;
		passData.requireSrgbConversion = cameraData.requireSrgbConversion;
		passData.enableAlphaOutput = enableAlphaOutput;
		passData.blitMaterialData = m_BlitMaterialData[(int)blitType];
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData, UniversalCameraData cameraData, in TextureHandle src, in TextureHandle dest, TextureHandle overlayUITexture)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\FinalBlitPass.cs", 270);
		frameData.Get<UniversalResourceData>();
		bool flag = cameraData.renderer is UniversalRenderer;
		if (cameraData.requiresDepthTexture && flag)
		{
			rasterRenderGraphBuilder.UseGlobalTexture(s_CameraDepthTextureID);
		}
		bool isHDROutputActive = cameraData.isHDROutputActive;
		bool isAlphaOutputEnabled = cameraData.isAlphaOutputEnabled;
		InitPassData(cameraData, ref passData, isHDROutputActive ? BlitType.HDR : BlitType.Core, isAlphaOutputEnabled);
		passData.sourceID = ShaderPropertyId.sourceTex;
		passData.source = src;
		rasterRenderGraphBuilder.UseTexture(in src);
		passData.destination = dest;
		AccessFlags flags = AccessFlags.Write;
		bool flag2 = !XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster);
		rasterRenderGraphBuilder.EnableFoveatedRasterization(cameraData.xr.supportsFoveatedRendering && flag2);
		if (cameraData.xr.enabled && cameraData.isDefaultViewport && !isAlphaOutputEnabled)
		{
			flags = AccessFlags.WriteAll;
		}
		rasterRenderGraphBuilder.SetRenderAttachment(dest, 0, flags);
		if (isHDROutputActive && overlayUITexture.IsValid())
		{
			Tonemapping component = VolumeManager.instance.stack.GetComponent<Tonemapping>();
			UniversalRenderPipeline.GetHDROutputLuminanceParameters(passData.cameraData.hdrDisplayInformation, passData.cameraData.hdrDisplayColorGamut, component, out passData.hdrOutputLuminanceParams);
			rasterRenderGraphBuilder.UseTexture(in overlayUITexture);
		}
		else
		{
			passData.hdrOutputLuminanceParams = new Vector4(-1f, -1f, -1f, -1f);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			data.blitMaterialData.material.enabledKeywords = null;
			context.cmd.SetKeyword(in ShaderGlobalKeywords.LinearToSRGBConversion, data.requireSrgbConversion);
			data.blitMaterialData.material.SetTexture(data.sourceID, data.source);
			DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(data.cameraData);
			bool num = activeDebugHandler?.WriteToDebugScreenTexture(data.cameraData.resolveFinalTarget) ?? false;
			if (data.hdrOutputLuminanceParams.w >= 0f)
			{
				HDROutputUtils.Operation operation = HDROutputUtils.Operation.None;
				if (activeDebugHandler == null || !activeDebugHandler.HDRDebugViewIsActive(data.cameraData.resolveFinalTarget))
				{
					operation |= HDROutputUtils.Operation.ColorEncoding;
				}
				if (!data.cameraData.postProcessEnabled)
				{
					operation |= HDROutputUtils.Operation.ColorConversion;
				}
				SetupHDROutput(data.cameraData.hdrDisplayColorGamut, data.blitMaterialData.material, operation, data.hdrOutputLuminanceParams, data.cameraData.rendersOverlayUI);
			}
			if (num)
			{
				RTHandle rTHandle = data.source;
				Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				RenderTexture rt = rTHandle.rt;
				int pass = (((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? data.blitMaterialData.bilinearSamplerPass : data.blitMaterialData.nearestSamplerPass);
				Blitter.BlitTexture(context.cmd, rTHandle, vector, data.blitMaterialData.material, pass);
			}
			else
			{
				ExecutePass(context.cmd, data, data.source, data.destination, data.cameraData);
			}
		});
	}
}
