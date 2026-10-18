using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace UnityEngine.Rendering.RenderGraphModule.Util;

public static class RenderGraphUtils
{
	private class CopyPassData
	{
		public bool isMSAA;

		public bool force2DForXR;
	}

	public enum BlitFilterMode
	{
		ClampNearest,
		ClampBilinear
	}

	private class BlitPassData
	{
		public TextureHandle source;

		public TextureHandle destination;

		public Vector2 scale;

		public Vector2 offset;

		public int sourceSlice;

		public int destinationSlice;

		public int numSlices;

		public int sourceMip;

		public int destinationMip;

		public int numMips;

		public BlitFilterMode filterMode;

		public bool isXR;
	}

	public enum FullScreenGeometryType
	{
		Mesh,
		ProceduralTriangle,
		ProceduralQuad
	}

	public struct BlitMaterialParameters
	{
		private static readonly int blitTextureProperty = Shader.PropertyToID("_BlitTexture");

		private static readonly int blitSliceProperty = Shader.PropertyToID("_BlitTexArraySlice");

		private static readonly int blitMipProperty = Shader.PropertyToID("_BlitMipLevel");

		private static readonly int blitScaleBias = Shader.PropertyToID("_BlitScaleBias");

		public TextureHandle source;

		public TextureHandle destination;

		public Vector2 scale;

		public Vector2 offset;

		public int sourceSlice;

		public int destinationSlice;

		public int numSlices;

		public int sourceMip;

		public int destinationMip;

		public int numMips;

		public Material material;

		public int shaderPass;

		public MaterialPropertyBlock propertyBlock;

		public int sourceTexturePropertyID;

		public int sourceSlicePropertyID;

		public int sourceMipPropertyID;

		public int scaleBiasPropertyID;

		public FullScreenGeometryType geometry;

		public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Material material, int shaderPass)
			: this(source, destination, Vector2.one, Vector2.zero, material, shaderPass)
		{
		}

		public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Vector2 scale, Vector2 offset, Material material, int shaderPass)
		{
			this.source = source;
			this.destination = destination;
			this.scale = scale;
			this.offset = offset;
			sourceSlice = -1;
			destinationSlice = 0;
			numSlices = -1;
			sourceMip = -1;
			destinationMip = 0;
			numMips = 1;
			this.material = material;
			this.shaderPass = shaderPass;
			propertyBlock = null;
			sourceTexturePropertyID = blitTextureProperty;
			sourceSlicePropertyID = blitSliceProperty;
			sourceMipPropertyID = blitMipProperty;
			scaleBiasPropertyID = blitScaleBias;
			geometry = FullScreenGeometryType.ProceduralTriangle;
		}

		public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Material material, int shaderPass, MaterialPropertyBlock mpb, int destinationSlice, int destinationMip, int numSlices = -1, int numMips = 1, int sourceSlice = -1, int sourceMip = -1, FullScreenGeometryType geometry = FullScreenGeometryType.Mesh, int sourceTexturePropertyID = -1, int sourceSlicePropertyID = -1, int sourceMipPropertyID = -1)
			: this(source, destination, Vector2.one, Vector2.zero, material, shaderPass, mpb, destinationSlice, destinationMip, numSlices, numMips, sourceSlice, sourceMip, geometry, sourceTexturePropertyID, sourceSlicePropertyID, sourceMipPropertyID)
		{
		}

		public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Vector2 scale, Vector2 offset, Material material, int shaderPass, MaterialPropertyBlock mpb, int destinationSlice, int destinationMip, int numSlices = -1, int numMips = 1, int sourceSlice = -1, int sourceMip = -1, FullScreenGeometryType geometry = FullScreenGeometryType.Mesh, int sourceTexturePropertyID = -1, int sourceSlicePropertyID = -1, int sourceMipPropertyID = -1, int scaleBiasPropertyID = -1)
			: this(source, destination, scale, offset, material, shaderPass)
		{
			propertyBlock = mpb;
			this.sourceSlice = sourceSlice;
			this.destinationSlice = destinationSlice;
			this.numSlices = numSlices;
			this.sourceMip = sourceMip;
			this.destinationMip = destinationMip;
			this.numMips = numMips;
			if (sourceTexturePropertyID != -1)
			{
				this.sourceTexturePropertyID = sourceTexturePropertyID;
			}
			if (sourceSlicePropertyID != -1)
			{
				this.sourceSlicePropertyID = sourceSlicePropertyID;
			}
			if (sourceMipPropertyID != -1)
			{
				this.sourceMipPropertyID = sourceMipPropertyID;
			}
			if (scaleBiasPropertyID != -1)
			{
				this.scaleBiasPropertyID = scaleBiasPropertyID;
			}
			this.geometry = geometry;
		}

		public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Material material, int shaderPass, MaterialPropertyBlock mpb, FullScreenGeometryType geometry = FullScreenGeometryType.Mesh, int sourceTexturePropertyID = -1, int sourceSlicePropertyID = -1, int sourceMipPropertyID = -1)
			: this(source, destination, Vector2.one, Vector2.zero, material, shaderPass, mpb, geometry, sourceTexturePropertyID, sourceSlicePropertyID, sourceMipPropertyID)
		{
		}

		public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Vector2 scale, Vector2 offset, Material material, int shaderPass, MaterialPropertyBlock mpb, FullScreenGeometryType geometry = FullScreenGeometryType.Mesh, int sourceTexturePropertyID = -1, int sourceSlicePropertyID = -1, int sourceMipPropertyID = -1, int scaleBiasPropertyID = -1)
			: this(source, destination, scale, offset, material, shaderPass)
		{
			propertyBlock = mpb;
			if (sourceTexturePropertyID != -1)
			{
				this.sourceTexturePropertyID = sourceTexturePropertyID;
			}
			if (sourceSlicePropertyID != -1)
			{
				this.sourceSlicePropertyID = sourceSlicePropertyID;
			}
			if (sourceMipPropertyID != -1)
			{
				this.sourceMipPropertyID = sourceMipPropertyID;
			}
			if (scaleBiasPropertyID != -1)
			{
				this.scaleBiasPropertyID = scaleBiasPropertyID;
			}
			this.geometry = geometry;
		}
	}

	private class BlitMaterialPassData
	{
		public int sourceTexturePropertyID;

		public TextureHandle source;

		public TextureHandle destination;

		public Vector2 scale;

		public Vector2 offset;

		public Material material;

		public int shaderPass;

		public MaterialPropertyBlock propertyBlock;

		public int sourceSlice;

		public int destinationSlice;

		public int numSlices;

		public int sourceMip;

		public int destinationMip;

		public int numMips;

		public FullScreenGeometryType geometry;

		public int sourceSlicePropertyID;

		public int sourceMipPropertyID;

		public int scaleBiasPropertyID;

		public bool isXR;
	}

	private static MaterialPropertyBlock s_PropertyBlock = new MaterialPropertyBlock();

	private const string DisableTexture2DXArray = "DISABLE_TEXTURE2D_X_ARRAY";

	private static Vector4 s_BlitScaleBias = default(Vector4);

	public static bool CanAddCopyPassMSAA()
	{
		if (!IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform())
		{
			return false;
		}
		return Blitter.CanCopyMSAA();
	}

	public static bool CanAddCopyPassMSAA(in TextureDesc sourceDesc)
	{
		if (!IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform())
		{
			return false;
		}
		return Blitter.CanCopyMSAA(in sourceDesc);
	}

	internal static bool IsFramebufferFetchEmulationSupportedOnCurrentPlatform()
	{
		return true;
	}

	internal static bool IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform()
	{
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation4 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation5)
		{
			return SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation5NGGC;
		}
		return false;
	}

	public static bool IsFramebufferFetchSupportedOnCurrentPlatform(this RenderGraph graph, in TextureHandle tex)
	{
		if (!IsFramebufferFetchEmulationSupportedOnCurrentPlatform())
		{
			return false;
		}
		if (!IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform())
		{
			RenderTargetInfo renderTargetInfo = graph.GetRenderTargetInfo(tex);
			if (renderTargetInfo.msaaSamples > 1)
			{
				return renderTargetInfo.bindMS;
			}
		}
		return true;
	}

	public static void AddCopyPass(this RenderGraph graph, TextureHandle source, TextureHandle destination, string passName = "Copy Pass Utility", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
	{
		if (!graph.nativeRenderPassesEnabled)
		{
			throw new ArgumentException("CopyPass only supported for native render pass. Please use the blit functions instead for non native render pass platforms.");
		}
		TextureDesc sourceDesc = graph.GetTextureDesc(in source);
		TextureDesc textureDesc = graph.GetTextureDesc(in destination);
		if (sourceDesc.msaaSamples != textureDesc.msaaSamples)
		{
			throw new ArgumentException("MSAA samples from source and destination texture doesn't match.");
		}
		if (sourceDesc.width != textureDesc.width || sourceDesc.height != textureDesc.height)
		{
			throw new ArgumentException("Dimensions for source and destination texture doesn't match.");
		}
		if (sourceDesc.slices != textureDesc.slices)
		{
			throw new ArgumentException("Slice count for source and destination texture doesn't match.");
		}
		bool flag = sourceDesc.msaaSamples > MSAASamples.None;
		if (flag && !CanAddCopyPassMSAA(in sourceDesc))
		{
			throw new ArgumentException("Target does not support MSAA for AddCopyPass. Please use the blit alternative or use non MSAA textures.");
		}
		CopyPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<CopyPassData>(passName, out passData, file, line);
		passData.isMSAA = flag;
		bool useTexArray = TextureXR.useTexArray;
		bool flag2 = sourceDesc.slices > 1;
		passData.force2DForXR = useTexArray && !flag2;
		rasterRenderGraphBuilder.SetInputAttachment(source, 0);
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(CopyPassData data, RasterGraphContext context)
		{
			CopyRenderFunc(data, context);
		});
		if (passData.force2DForXR)
		{
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		}
	}

	public static void AddCopyPass(this RenderGraph graph, TextureHandle source, TextureHandle destination, int sourceSlice, int destinationSlice = 0, int sourceMip = 0, int destinationMip = 0, string passName = "Copy Pass Utility", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
	{
		graph.AddCopyPass(source, destination, passName, file, line);
	}

	private static void CopyRenderFunc(CopyPassData data, RasterGraphContext rgContext)
	{
		Blitter.CopyTexture(rgContext.cmd, data.isMSAA, data.force2DForXR);
	}

	internal static bool IsTextureXR(ref TextureDesc destDesc, int sourceSlice, int destinationSlice, int numSlices, int numMips)
	{
		if (TextureXR.useTexArray && destDesc.dimension == TextureDimension.Tex2DArray && destDesc.slices == TextureXR.slices && sourceSlice == 0 && destinationSlice == 0 && numSlices == TextureXR.slices && numMips == 1)
		{
			return true;
		}
		return false;
	}

	public static void AddBlitPass(this RenderGraph graph, TextureHandle source, TextureHandle destination, Vector2 scale, Vector2 offset, int sourceSlice = 0, int destinationSlice = 0, int numSlices = -1, int sourceMip = 0, int destinationMip = 0, int numMips = 1, BlitFilterMode filterMode = BlitFilterMode.ClampBilinear, string passName = "Blit Pass Utility", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
	{
		TextureDesc textureDesc = graph.GetTextureDesc(in source);
		TextureDesc destDesc = graph.GetTextureDesc(in destination);
		int num = (int)math.log2(math.max(math.max(textureDesc.width, textureDesc.height), textureDesc.slices)) + 1;
		int num2 = (int)math.log2(math.max(math.max(destDesc.width, destDesc.height), destDesc.slices)) + 1;
		if (numSlices == -1)
		{
			numSlices = textureDesc.slices - sourceSlice;
		}
		if (numSlices > textureDesc.slices - sourceSlice || numSlices > destDesc.slices - destinationSlice)
		{
			throw new ArgumentException("BlitPass: " + passName + " attempts to blit too many slices. The pass will be skipped.");
		}
		if (numMips == -1)
		{
			numMips = num - sourceMip;
		}
		if (numMips > num - sourceMip || numMips > num2 - destinationMip)
		{
			throw new ArgumentException("BlitPass: " + passName + " attempts to blit too many mips. The pass will be skipped.");
		}
		BlitPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = graph.AddUnsafePass<BlitPassData>(passName, out passData, file, line);
		passData.source = source;
		passData.destination = destination;
		passData.scale = scale;
		passData.offset = offset;
		passData.sourceSlice = sourceSlice;
		passData.destinationSlice = destinationSlice;
		passData.numSlices = numSlices;
		passData.sourceMip = sourceMip;
		passData.destinationMip = destinationMip;
		passData.numMips = numMips;
		passData.filterMode = filterMode;
		passData.isXR = IsTextureXR(ref destDesc, sourceSlice, destinationSlice, numSlices, numMips);
		unsafeRenderGraphBuilder.UseTexture(in source);
		unsafeRenderGraphBuilder.UseTexture(in destination, AccessFlags.Write);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(BlitPassData data, UnsafeGraphContext context)
		{
			BlitRenderFunc(data, context);
		});
	}

	private static void BlitRenderFunc(BlitPassData data, UnsafeGraphContext context)
	{
		s_BlitScaleBias.x = data.scale.x;
		s_BlitScaleBias.y = data.scale.y;
		s_BlitScaleBias.z = data.offset.x;
		s_BlitScaleBias.w = data.offset.y;
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		if (data.isXR)
		{
			context.cmd.SetRenderTarget(data.destination, 0, CubemapFace.Unknown, -1);
			Blitter.BlitTexture(nativeCommandBuffer, data.source, s_BlitScaleBias, data.sourceMip, data.filterMode == BlitFilterMode.ClampBilinear);
			return;
		}
		for (int i = 0; i < data.numSlices; i++)
		{
			for (int j = 0; j < data.numMips; j++)
			{
				context.cmd.SetRenderTarget(data.destination, data.destinationMip + j, CubemapFace.Unknown, data.destinationSlice + i);
				Blitter.BlitTexture(nativeCommandBuffer, data.source, s_BlitScaleBias, data.sourceMip + j, data.sourceSlice + i, data.filterMode == BlitFilterMode.ClampBilinear);
			}
		}
	}

	public static void AddBlitPass(this RenderGraph graph, BlitMaterialParameters blitParameters, string passName = "Blit Pass Utility w. Material", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
	{
		if (!blitParameters.destination.IsValid())
		{
			throw new ArgumentException("BlitPass: " + passName + " destination needs to be a valid texture handle.");
		}
		TextureDesc destDesc = graph.GetTextureDesc(in blitParameters.destination);
		int num = (int)math.log2(math.max(math.max(destDesc.width, destDesc.height), destDesc.slices)) + 1;
		if (blitParameters.numSlices == -1)
		{
			blitParameters.numSlices = destDesc.slices - blitParameters.destinationSlice;
		}
		if (blitParameters.numMips == -1)
		{
			blitParameters.numMips = num - blitParameters.destinationMip;
		}
		if (blitParameters.source.IsValid())
		{
			TextureDesc textureDesc = graph.GetTextureDesc(in blitParameters.source);
			int num2 = (int)math.log2(math.max(math.max(textureDesc.width, textureDesc.height), textureDesc.slices)) + 1;
			if (blitParameters.sourceSlice != -1 && blitParameters.numSlices > textureDesc.slices - blitParameters.sourceSlice)
			{
				throw new ArgumentException("BlitPass: " + passName + " attempts to blit too many slices. There are not enough slices in the source array. The pass will be skipped.");
			}
			if (blitParameters.sourceMip != -1 && blitParameters.numMips > num2 - blitParameters.sourceMip)
			{
				throw new ArgumentException("BlitPass: " + passName + " attempts to blit too many mips. There are not enough mips in the source texture. The pass will be skipped.");
			}
		}
		if (blitParameters.numSlices > destDesc.slices - blitParameters.destinationSlice)
		{
			throw new ArgumentException("BlitPass: " + passName + " attempts to blit too many slices. There are not enough slices in the destination array. The pass will be skipped.");
		}
		if (blitParameters.numMips > num - blitParameters.destinationMip)
		{
			throw new ArgumentException("BlitPass: " + passName + " attempts to blit too many mips. There are not enough mips in the destination texture. The pass will be skipped.");
		}
		BlitMaterialPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = graph.AddUnsafePass<BlitMaterialPassData>(passName, out passData, file, line);
		passData.sourceTexturePropertyID = blitParameters.sourceTexturePropertyID;
		passData.source = blitParameters.source;
		passData.destination = blitParameters.destination;
		passData.scale = blitParameters.scale;
		passData.offset = blitParameters.offset;
		passData.material = blitParameters.material;
		passData.shaderPass = blitParameters.shaderPass;
		passData.propertyBlock = blitParameters.propertyBlock;
		passData.sourceSlice = blitParameters.sourceSlice;
		passData.destinationSlice = blitParameters.destinationSlice;
		passData.numSlices = blitParameters.numSlices;
		passData.sourceMip = blitParameters.sourceMip;
		passData.destinationMip = blitParameters.destinationMip;
		passData.numMips = blitParameters.numMips;
		passData.geometry = blitParameters.geometry;
		passData.sourceSlicePropertyID = blitParameters.sourceSlicePropertyID;
		passData.sourceMipPropertyID = blitParameters.sourceMipPropertyID;
		passData.scaleBiasPropertyID = blitParameters.scaleBiasPropertyID;
		passData.isXR = IsTextureXR(ref destDesc, (passData.sourceSlice != -1) ? passData.sourceSlice : 0, passData.destinationSlice, passData.numSlices, passData.numMips);
		if (blitParameters.source.IsValid())
		{
			unsafeRenderGraphBuilder.UseTexture(in blitParameters.source);
		}
		unsafeRenderGraphBuilder.UseTexture(in blitParameters.destination, AccessFlags.Write);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(BlitMaterialPassData data, UnsafeGraphContext context)
		{
			BlitMaterialRenderFunc(data, context);
		});
	}

	private static void BlitMaterialRenderFunc(BlitMaterialPassData data, UnsafeGraphContext context)
	{
		s_BlitScaleBias.x = data.scale.x;
		s_BlitScaleBias.y = data.scale.y;
		s_BlitScaleBias.z = data.offset.x;
		s_BlitScaleBias.w = data.offset.y;
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		if (data.propertyBlock == null)
		{
			data.propertyBlock = s_PropertyBlock;
		}
		if (data.source.IsValid())
		{
			data.propertyBlock.SetTexture(data.sourceTexturePropertyID, data.source);
		}
		data.propertyBlock.SetVector(data.scaleBiasPropertyID, s_BlitScaleBias);
		if (data.isXR)
		{
			if (data.sourceSlice != -1)
			{
				data.propertyBlock.SetInt(data.sourceSlicePropertyID, 0);
			}
			if (data.sourceMip != -1)
			{
				data.propertyBlock.SetInt(data.sourceMipPropertyID, data.sourceMip);
			}
			context.cmd.SetRenderTarget(data.destination, 0, CubemapFace.Unknown, -1);
			switch (data.geometry)
			{
			case FullScreenGeometryType.Mesh:
				Blitter.DrawQuadMesh(nativeCommandBuffer, data.material, data.shaderPass, data.propertyBlock);
				break;
			case FullScreenGeometryType.ProceduralQuad:
				Blitter.DrawQuad(nativeCommandBuffer, data.material, data.shaderPass, data.propertyBlock);
				break;
			case FullScreenGeometryType.ProceduralTriangle:
				Blitter.DrawTriangle(nativeCommandBuffer, data.material, data.shaderPass, data.propertyBlock);
				break;
			}
			return;
		}
		for (int i = 0; i < data.numSlices; i++)
		{
			for (int j = 0; j < data.numMips; j++)
			{
				if (data.sourceSlice != -1)
				{
					data.propertyBlock.SetInt(data.sourceSlicePropertyID, data.sourceSlice + i);
				}
				if (data.sourceMip != -1)
				{
					data.propertyBlock.SetInt(data.sourceMipPropertyID, data.sourceMip + j);
				}
				context.cmd.SetRenderTarget(data.destination, data.destinationMip + j, CubemapFace.Unknown, data.destinationSlice + i);
				switch (data.geometry)
				{
				case FullScreenGeometryType.Mesh:
					Blitter.DrawQuadMesh(nativeCommandBuffer, data.material, data.shaderPass, data.propertyBlock);
					break;
				case FullScreenGeometryType.ProceduralQuad:
					Blitter.DrawQuad(nativeCommandBuffer, data.material, data.shaderPass, data.propertyBlock);
					break;
				case FullScreenGeometryType.ProceduralTriangle:
					Blitter.DrawTriangle(nativeCommandBuffer, data.material, data.shaderPass, data.propertyBlock);
					break;
				}
			}
		}
	}
}
