using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public static class Vrs
{
	private class ConversionPassData
	{
		public TextureHandle sriTextureHandle;

		public TextureHandle mainTexHandle;

		public TextureDimension mainTexDimension;

		public BufferHandle mainTexLutHandle;

		public BufferHandle validatedShadingRateFragmentSizeHandle;

		public ComputeShader computeShader;

		public int kernelIndex;

		public Vector4 scaleBias;

		public Vector2Int dispatchSize;

		public bool yFlip;
	}

	private class VisualizationPassData
	{
		public Material material;

		public TextureHandle source;

		public BufferHandle lut;

		public TextureHandle dummy;

		public Vector4 visualizationParams;
	}

	internal static readonly int shadingRateFragmentSizeCount = Enum.GetNames(typeof(ShadingRateFragmentSize)).Length;

	private static VrsResources s_VrsResources;

	public static bool IsColorMaskTextureConversionSupported()
	{
		if (SystemInfo.supportsComputeShaders && ShadingRateInfo.supportsPerImageTile)
		{
			return IsInitialized();
		}
		return false;
	}

	public static bool IsInitialized()
	{
		if (s_VrsResources != null && s_VrsResources.textureComputeShader != null && s_VrsResources.textureReduceKernel != -1)
		{
			return s_VrsResources.textureCopyKernel != -1;
		}
		return false;
	}

	public static void InitializeResources()
	{
		bool flag = SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3;
		if (SystemInfo.supportsComputeShaders && flag)
		{
			s_VrsResources = new VrsResources(GraphicsSettings.GetRenderPipelineSettings<VrsRenderPipelineRuntimeResources>());
		}
	}

	public static void DisposeResources()
	{
		s_VrsResources?.Dispose();
		s_VrsResources = null;
	}

	public static TextureHandle ColorMaskTextureToShadingRateImage(RenderGraph renderGraph, RTHandle sriRtHandle, RTHandle colorMaskRtHandle, bool yFlip)
	{
		if (renderGraph == null || sriRtHandle == null || colorMaskRtHandle == null)
		{
			Debug.LogError("TextureToShadingRateImage: invalid argument.");
			return TextureHandle.nullHandle;
		}
		TextureHandle sriTextureHandle = renderGraph.ImportShadingRateImageTexture(sriRtHandle);
		TextureHandle colorMaskHandle = renderGraph.ImportTexture(colorMaskRtHandle);
		return ColorMaskTextureToShadingRateImage(renderGraph, sriTextureHandle, colorMaskHandle, ((Texture)colorMaskRtHandle).dimension, yFlip);
	}

	public static TextureHandle ColorMaskTextureToShadingRateImage(RenderGraph renderGraph, TextureHandle sriTextureHandle, TextureHandle colorMaskHandle, TextureDimension colorMaskDimension, bool yFlip)
	{
		if (!IsColorMaskTextureConversionSupported())
		{
			Debug.LogError("ColorMaskTextureToShadingRateImage: conversion not supported.");
			return TextureHandle.nullHandle;
		}
		TextureDesc descriptor = sriTextureHandle.GetDescriptor(renderGraph);
		if (descriptor.dimension != TextureDimension.Tex2D)
		{
			Debug.LogError("ColorMaskTextureToShadingRateImage: Vrs image not a texture 2D.");
			return TextureHandle.nullHandle;
		}
		if (colorMaskDimension != TextureDimension.Tex2D && colorMaskDimension != TextureDimension.Tex2DArray)
		{
			Debug.LogError("ColorMaskTextureToShadingRateImage: Input texture dimension not supported.");
			return TextureHandle.nullHandle;
		}
		ConversionPassData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = renderGraph.AddComputePass<ConversionPassData>("TextureToShadingRateImage", out passData, s_VrsResources.conversionProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\Vrs\\Vrs.cs", 159);
		passData.sriTextureHandle = sriTextureHandle;
		passData.mainTexHandle = colorMaskHandle;
		passData.mainTexDimension = colorMaskDimension;
		passData.mainTexLutHandle = renderGraph.ImportBuffer(s_VrsResources.conversionLutBuffer);
		passData.validatedShadingRateFragmentSizeHandle = renderGraph.ImportBuffer(s_VrsResources.validatedShadingRateFragmentSizeBuffer);
		passData.computeShader = s_VrsResources.textureComputeShader;
		passData.kernelIndex = s_VrsResources.textureReduceKernel;
		passData.scaleBias = new Vector4
		{
			x = 1f / (float)(descriptor.width * s_VrsResources.tileSize.x),
			y = 1f / (float)(descriptor.height * s_VrsResources.tileSize.y),
			z = descriptor.width,
			w = descriptor.height
		};
		passData.dispatchSize = new Vector2Int(descriptor.width, descriptor.height);
		passData.yFlip = yFlip;
		computeRenderGraphBuilder.UseTexture(in passData.sriTextureHandle, AccessFlags.Write);
		computeRenderGraphBuilder.UseTexture(in passData.mainTexHandle);
		computeRenderGraphBuilder.UseBuffer(in passData.mainTexLutHandle);
		computeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		computeRenderGraphBuilder.SetRenderFunc(delegate(ConversionPassData innerPassData, ComputeGraphContext context)
		{
			ConversionDispatch(context.cmd, innerPassData);
		});
		return passData.sriTextureHandle;
	}

	public static void ShadingRateImageToColorMaskTexture(RenderGraph renderGraph, in TextureHandle sriTextureHandle, in TextureHandle colorMaskHandle)
	{
		if (s_VrsResources == null)
		{
			Debug.LogError("ShadingRateImageToColorMaskTexture: VRS not initialized.");
			return;
		}
		if (!colorMaskHandle.IsValid())
		{
			Debug.LogError("ShadingRateImageToColorMaskTexture: Output target handle is not valid.");
			return;
		}
		VisualizationPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<VisualizationPassData>("ShadingRateImageToTexture", out passData, s_VrsResources.visualizationProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\Vrs\\Vrs.cs", 214);
		passData.material = s_VrsResources.visualizationMaterial;
		if (sriTextureHandle.IsValid())
		{
			passData.source = sriTextureHandle;
		}
		else
		{
			passData.source = renderGraph.defaultResources.blackTexture;
		}
		passData.lut = renderGraph.ImportBuffer(s_VrsResources.visualizationLutBuffer);
		passData.dummy = renderGraph.defaultResources.blackTexture;
		passData.visualizationParams = new Vector4(1f / (float)s_VrsResources.tileSize.x, 1f / (float)s_VrsResources.tileSize.y, 0f, 0f);
		rasterRenderGraphBuilder.UseTexture(in passData.source);
		rasterRenderGraphBuilder.UseBuffer(in passData.lut);
		rasterRenderGraphBuilder.UseTexture(in passData.dummy);
		rasterRenderGraphBuilder.SetRenderAttachment(colorMaskHandle, 0);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(VisualizationPassData innerPassData, RasterGraphContext context)
		{
			innerPassData.material.SetTexture(VrsShaders.s_ShadingRateImage, innerPassData.source);
			innerPassData.material.SetBuffer(VrsShaders.s_VisualizationLut, innerPassData.lut);
			innerPassData.material.SetVector(VrsShaders.s_VisualizationParams, innerPassData.visualizationParams);
			Blitter.BlitTexture(context.cmd, innerPassData.dummy, new Vector4(1f, 1f, 0f, 0f), innerPassData.material, 0);
		});
	}

	private static void ConversionDispatch(ComputeCommandBuffer cmd, ConversionPassData conversionPassData)
	{
		LocalKeyword keyword = new LocalKeyword(conversionPassData.computeShader, "DISABLE_TEXTURE2D_X_ARRAY");
		if (conversionPassData.mainTexDimension == TextureDimension.Tex2DArray)
		{
			cmd.DisableKeyword(conversionPassData.computeShader, in keyword);
		}
		else
		{
			cmd.EnableKeyword(conversionPassData.computeShader, in keyword);
		}
		LocalKeyword keyword2 = new LocalKeyword(conversionPassData.computeShader, "APPLY_Y_FLIP");
		if (conversionPassData.yFlip)
		{
			cmd.EnableKeyword(conversionPassData.computeShader, in keyword2);
		}
		else
		{
			cmd.DisableKeyword(conversionPassData.computeShader, in keyword2);
		}
		cmd.SetComputeTextureParam(conversionPassData.computeShader, conversionPassData.kernelIndex, VrsShaders.s_MainTex, conversionPassData.mainTexHandle);
		cmd.SetComputeBufferParam(conversionPassData.computeShader, conversionPassData.kernelIndex, VrsShaders.s_MainTexLut, conversionPassData.mainTexLutHandle);
		cmd.SetComputeBufferParam(conversionPassData.computeShader, conversionPassData.kernelIndex, VrsShaders.s_ShadingRateNativeValues, conversionPassData.validatedShadingRateFragmentSizeHandle);
		cmd.SetComputeTextureParam(conversionPassData.computeShader, conversionPassData.kernelIndex, VrsShaders.s_ShadingRateImage, conversionPassData.sriTextureHandle);
		cmd.SetComputeVectorParam(conversionPassData.computeShader, VrsShaders.s_ScaleBias, conversionPassData.scaleBias);
		cmd.DispatchCompute(conversionPassData.computeShader, conversionPassData.kernelIndex, conversionPassData.dispatchSize.x, conversionPassData.dispatchSize.y, 1);
	}

	public static void ColorMaskTextureToShadingRateImageDispatch(CommandBuffer cmd, RTHandle sriDestination, Texture colorMaskSource, bool yFlip = true)
	{
		if (sriDestination == null)
		{
			Debug.LogError("ColorMaskTextureToShadingRateImageDispatch: VRS destination shading rate texture is null.");
			return;
		}
		if (colorMaskSource == null)
		{
			Debug.LogError("ColorMaskTextureToShadingRateImageDispatch: VRS source color texture is null.");
			return;
		}
		if (!IsInitialized())
		{
			Debug.LogError("ColorMaskTextureToShadingRateImageDispatch: VRS is not initialized.");
			return;
		}
		ComputeShader textureComputeShader = s_VrsResources.textureComputeShader;
		int textureReduceKernel = s_VrsResources.textureReduceKernel;
		GraphicsBuffer conversionLutBuffer = s_VrsResources.conversionLutBuffer;
		GraphicsBuffer validatedShadingRateFragmentSizeBuffer = s_VrsResources.validatedShadingRateFragmentSizeBuffer;
		int width = sriDestination.rt.width;
		int height = sriDestination.rt.height;
		Vector4 vector = default(Vector4);
		vector.x = 1f / (float)(width * s_VrsResources.tileSize.x);
		vector.y = 1f / (float)(height * s_VrsResources.tileSize.y);
		vector.z = width;
		vector.w = height;
		Vector4 val = vector;
		Vector2Int vector2Int = new Vector2Int(width, height);
		LocalKeyword keyword = new LocalKeyword(textureComputeShader, "DISABLE_TEXTURE2D_X_ARRAY");
		if ((object)colorMaskSource != null && colorMaskSource.dimension == TextureDimension.Tex2DArray)
		{
			cmd.DisableKeyword(textureComputeShader, in keyword);
		}
		else
		{
			cmd.EnableKeyword(textureComputeShader, in keyword);
		}
		LocalKeyword keyword2 = new LocalKeyword(textureComputeShader, "APPLY_Y_FLIP");
		if (yFlip)
		{
			cmd.EnableKeyword(textureComputeShader, in keyword2);
		}
		else
		{
			cmd.DisableKeyword(textureComputeShader, in keyword2);
		}
		cmd.SetComputeTextureParam(textureComputeShader, textureReduceKernel, VrsShaders.s_MainTex, colorMaskSource);
		cmd.SetComputeBufferParam(textureComputeShader, textureReduceKernel, VrsShaders.s_MainTexLut, conversionLutBuffer);
		cmd.SetComputeBufferParam(textureComputeShader, textureReduceKernel, VrsShaders.s_ShadingRateNativeValues, validatedShadingRateFragmentSizeBuffer);
		cmd.SetComputeTextureParam(textureComputeShader, textureReduceKernel, VrsShaders.s_ShadingRateImage, sriDestination);
		cmd.SetComputeVectorParam(textureComputeShader, VrsShaders.s_ScaleBias, val);
		cmd.DispatchCompute(textureComputeShader, textureReduceKernel, vector2Int.x, vector2Int.y, 1);
	}

	public static void ShadingRateImageToColorMaskTextureBlit(CommandBuffer cmd, RTHandle sriSource, RTHandle colorMaskDestination)
	{
		if (sriSource == null)
		{
			Debug.LogError("ShadingRateImageToColorMaskTextureBlit: VRS source shading rate texture is null.");
			return;
		}
		if (colorMaskDestination == null)
		{
			Debug.LogError("ShadingRateImageToColorMaskTextureBlit: VRS destination color texture is null.");
			return;
		}
		if (!IsInitialized())
		{
			Debug.LogError("ShadingRateImageToColorMaskTextureBlit: VRS is not initialized.");
			return;
		}
		Material visualizationMaterial = s_VrsResources.visualizationMaterial;
		GraphicsBuffer visualizationLutBuffer = s_VrsResources.visualizationLutBuffer;
		Vector4 value = new Vector4(1f / (float)s_VrsResources.tileSize.x, 1f / (float)s_VrsResources.tileSize.y, 0f, 0f);
		visualizationMaterial.SetTexture(VrsShaders.s_ShadingRateImage, sriSource);
		visualizationMaterial.SetBuffer(VrsShaders.s_VisualizationLut, visualizationLutBuffer);
		visualizationMaterial.SetVector(VrsShaders.s_VisualizationParams, value);
		CoreUtils.SetRenderTarget(cmd, colorMaskDestination);
		Blitter.BlitTexture(cmd, new Vector4(1f, 1f, 0f, 0f), visualizationMaterial, 0);
	}
}
