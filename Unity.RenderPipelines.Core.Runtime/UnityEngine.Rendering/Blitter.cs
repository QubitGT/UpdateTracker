using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace UnityEngine.Rendering;

public static class Blitter
{
	private static class BlitShaderIDs
	{
		public static readonly int _BlitTexture = Shader.PropertyToID("_BlitTexture");

		public static readonly int _BlitCubeTexture = Shader.PropertyToID("_BlitCubeTexture");

		public static readonly int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");

		public static readonly int _BlitScaleBiasRt = Shader.PropertyToID("_BlitScaleBiasRt");

		public static readonly int _BlitMipLevel = Shader.PropertyToID("_BlitMipLevel");

		public static readonly int _BlitTexArraySlice = Shader.PropertyToID("_BlitTexArraySlice");

		public static readonly int _BlitTextureSize = Shader.PropertyToID("_BlitTextureSize");

		public static readonly int _BlitPaddingSize = Shader.PropertyToID("_BlitPaddingSize");

		public static readonly int _BlitDecodeInstructions = Shader.PropertyToID("_BlitDecodeInstructions");

		public static readonly int _InputDepth = Shader.PropertyToID("_InputDepthTexture");
	}

	private enum BlitShaderPassNames
	{
		Nearest,
		Bilinear,
		NearestQuad,
		BilinearQuad,
		NearestQuadPadding,
		BilinearQuadPadding,
		NearestQuadPaddingRepeat,
		BilinearQuadPaddingRepeat,
		BilinearQuadPaddingOctahedral,
		NearestQuadPaddingAlphaBlend,
		BilinearQuadPaddingAlphaBlend,
		NearestQuadPaddingAlphaBlendRepeat,
		BilinearQuadPaddingAlphaBlendRepeat,
		BilinearQuadPaddingAlphaBlendOctahedral,
		CubeToOctahedral,
		CubeToOctahedralLuminance,
		CubeToOctahedralAlpha,
		CubeToOctahedralRed,
		BilinearQuadLuminance,
		BilinearQuadAlpha,
		BilinearQuadRed,
		NearestCubeToOctahedralPadding,
		BilinearCubeToOctahedralPadding
	}

	private enum BlitColorAndDepthPassNames
	{
		ColorOnly,
		ColorAndDepth
	}

	private static Material s_Copy;

	private static Material s_Blit;

	private static Material s_BlitTexArray;

	private static Material s_BlitTexArraySingleSlice;

	private static Material s_BlitColorAndDepth;

	private static MaterialPropertyBlock s_PropertyBlock = new MaterialPropertyBlock();

	private static Mesh s_TriangleMesh;

	private static Mesh s_QuadMesh;

	private static LocalKeyword s_DecodeHdrKeyword;

	private static int[] s_BlitShaderPassIndicesMap;

	private static int[] s_BlitColorAndDepthShaderPassIndicesMap;

	public static void Initialize(Shader blitPS, Shader blitColorAndDepthPS)
	{
		if (s_Blit != null)
		{
			throw new Exception("Blitter is already initialized. Please only initialize the blitter once or you will leak engine resources. If you need to re-initialize the blitter with different shaders destroy & recreate it.");
		}
		s_Copy = CoreUtils.CreateEngineMaterial(GraphicsSettings.GetRenderPipelineSettings<RenderGraphUtilsResources>().coreCopyPS);
		s_Blit = CoreUtils.CreateEngineMaterial(blitPS);
		s_BlitColorAndDepth = CoreUtils.CreateEngineMaterial(blitColorAndDepthPS);
		s_DecodeHdrKeyword = new LocalKeyword(blitPS, "BLIT_DECODE_HDR");
		if (TextureXR.useTexArray)
		{
			s_Blit.EnableKeyword("DISABLE_TEXTURE2D_X_ARRAY");
			s_BlitTexArray = CoreUtils.CreateEngineMaterial(blitPS);
			s_BlitTexArraySingleSlice = CoreUtils.CreateEngineMaterial(blitPS);
			s_BlitTexArraySingleSlice.EnableKeyword("BLIT_SINGLE_SLICE");
		}
		float z2 = -1f;
		if (SystemInfo.usesReversedZBuffer)
		{
			z2 = 1f;
		}
		if (SystemInfo.graphicsShaderLevel < 30 && !s_TriangleMesh)
		{
			s_TriangleMesh = new Mesh();
			s_TriangleMesh.vertices = GetFullScreenTriangleVertexPosition(z2);
			s_TriangleMesh.uv = GetFullScreenTriangleTexCoord();
			s_TriangleMesh.triangles = new int[3] { 0, 1, 2 };
		}
		if (!s_QuadMesh)
		{
			s_QuadMesh = new Mesh();
			s_QuadMesh.vertices = GetQuadVertexPosition(z2);
			s_QuadMesh.uv = GetQuadTexCoord();
			s_QuadMesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		}
		string[] names = Enum.GetNames(typeof(BlitShaderPassNames));
		s_BlitShaderPassIndicesMap = new int[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			s_BlitShaderPassIndicesMap[i] = s_Blit.FindPass(names[i]);
		}
		names = Enum.GetNames(typeof(BlitColorAndDepthPassNames));
		s_BlitColorAndDepthShaderPassIndicesMap = new int[names.Length];
		for (int j = 0; j < names.Length; j++)
		{
			s_BlitColorAndDepthShaderPassIndicesMap[j] = s_BlitColorAndDepth.FindPass(names[j]);
		}
		static Vector2[] GetFullScreenTriangleTexCoord()
		{
			Vector2[] array3 = new Vector2[3];
			for (int k = 0; k < 3; k++)
			{
				if (SystemInfo.graphicsUVStartsAtTop)
				{
					array3[k] = new Vector2((k << 1) & 2, 1f - (float)(k & 2));
				}
				else
				{
					array3[k] = new Vector2((k << 1) & 2, k & 2);
				}
			}
			return array3;
		}
		static Vector3[] GetFullScreenTriangleVertexPosition(float z)
		{
			Vector3[] array4 = new Vector3[3];
			for (int l = 0; l < 3; l++)
			{
				Vector2 vector = new Vector2((l << 1) & 2, l & 2);
				array4[l] = new Vector3(vector.x * 2f - 1f, vector.y * 2f - 1f, z);
			}
			return array4;
		}
		static Vector2[] GetQuadTexCoord()
		{
			Vector2[] array = new Vector2[4];
			for (uint num = 0u; num < 4; num++)
			{
				uint num2 = num >> 1;
				uint num3 = num & 1u;
				float x = num2;
				float num4 = (num2 + num3) & 1u;
				if (SystemInfo.graphicsUVStartsAtTop)
				{
					num4 = 1f - num4;
				}
				array[num] = new Vector2(x, num4);
			}
			return array;
		}
		static Vector3[] GetQuadVertexPosition(float z)
		{
			Vector3[] array2 = new Vector3[4];
			for (uint num5 = 0u; num5 < 4; num5++)
			{
				uint num6 = num5 >> 1;
				uint num7 = num5 & 1u;
				float x2 = num6;
				float y = (1 - (num6 + num7)) & 1u;
				array2[num5] = new Vector3(x2, y, z);
			}
			return array2;
		}
	}

	public static void Cleanup()
	{
		CoreUtils.Destroy(s_Copy);
		s_Copy = null;
		CoreUtils.Destroy(s_Blit);
		s_Blit = null;
		CoreUtils.Destroy(s_BlitColorAndDepth);
		s_BlitColorAndDepth = null;
		CoreUtils.Destroy(s_BlitTexArray);
		s_BlitTexArray = null;
		CoreUtils.Destroy(s_BlitTexArraySingleSlice);
		s_BlitTexArraySingleSlice = null;
		CoreUtils.Destroy(s_TriangleMesh);
		s_TriangleMesh = null;
		CoreUtils.Destroy(s_QuadMesh);
		s_QuadMesh = null;
	}

	public static Material GetBlitMaterial(TextureDimension dimension, bool singleSlice = false)
	{
		Material material = ((dimension != TextureDimension.Tex2DArray) ? null : (singleSlice ? s_BlitTexArraySingleSlice : s_BlitTexArray));
		if (!(material == null))
		{
			return material;
		}
		return s_Blit;
	}

	internal static void DrawTriangle(RasterCommandBuffer cmd, Material material, int shaderPass)
	{
		DrawTriangle(cmd.m_WrappedCommandBuffer, material, shaderPass);
	}

	internal static void DrawTriangle(CommandBuffer cmd, Material material, int shaderPass)
	{
		DrawTriangle(cmd, material, shaderPass, s_PropertyBlock);
	}

	internal static void DrawTriangle(CommandBuffer cmd, Material material, int shaderPass, MaterialPropertyBlock propertyBlock)
	{
		if (SystemInfo.graphicsShaderLevel < 30)
		{
			cmd.DrawMesh(s_TriangleMesh, Matrix4x4.identity, material, 0, shaderPass, propertyBlock);
		}
		else
		{
			cmd.DrawProcedural(Matrix4x4.identity, material, shaderPass, MeshTopology.Triangles, 3, 1, propertyBlock);
		}
	}

	internal static void DrawQuadMesh(CommandBuffer cmd, Material material, int shaderPass, MaterialPropertyBlock propertyBlock)
	{
		cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, material, 0, shaderPass, propertyBlock);
	}

	internal static void DrawQuad(RasterCommandBuffer cmd, Material material, int shaderPass, MaterialPropertyBlock propertyBlock)
	{
		DrawQuad(cmd.m_WrappedCommandBuffer, material, shaderPass, propertyBlock);
	}

	internal static void DrawQuad(CommandBuffer cmd, Material material, int shaderPass)
	{
		DrawQuad(cmd, material, shaderPass, s_PropertyBlock);
	}

	internal static void DrawQuad(CommandBuffer cmd, Material material, int shaderPass, MaterialPropertyBlock propertyBlock)
	{
		if (SystemInfo.graphicsShaderLevel < 30)
		{
			cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, material, 0, shaderPass, propertyBlock);
		}
		else
		{
			cmd.DrawProcedural(Matrix4x4.identity, material, shaderPass, MeshTopology.Quads, 4, 1, propertyBlock);
		}
	}

	internal static bool CanCopyMSAA()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStation4)
		{
			return false;
		}
		return s_Copy.passCount == 2;
	}

	internal static bool CanCopyMSAA(in TextureDesc sourceDesc)
	{
		bool flag = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12;
		if (SystemInfo.supportsMultisampleAutoResolve && !flag && !sourceDesc.bindTextureMS)
		{
			return false;
		}
		return CanCopyMSAA();
	}

	internal static void CopyTexture(RasterCommandBuffer cmd, bool isMSAA, bool force2DForXR = false)
	{
		if (force2DForXR)
		{
			cmd.EnableShaderKeyword("DISABLE_TEXTURE2D_X_ARRAY");
		}
		DrawTriangle(cmd, s_Copy, isMSAA ? 1 : 0);
		if (force2DForXR)
		{
			cmd.DisableShaderKeyword("DISABLE_TEXTURE2D_X_ARRAY");
		}
	}

	internal static void BlitTexture(CommandBuffer cmd, RTHandle source, Vector4 scaleBias, float sourceMipLevel, int sourceDepthSlice, bool bilinear)
	{
		BlitTexture(cmd, source, scaleBias, GetBlitMaterial(TextureDimension.Tex2D), s_BlitShaderPassIndicesMap[bilinear ? 1 : 0], sourceMipLevel, sourceDepthSlice);
	}

	internal static void BlitTexture(CommandBuffer cmd, RTHandle source, Vector4 scaleBias, Material material, int pass, float sourceMipLevel, int sourceDepthSlice)
	{
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, sourceMipLevel);
		s_PropertyBlock.SetInt(BlitShaderIDs._BlitTexArraySlice, sourceDepthSlice);
		BlitTexture(cmd, source, scaleBias, material, pass);
	}

	public static void BlitTexture(RasterCommandBuffer cmd, RTHandle source, Vector4 scaleBias, float mipLevel, bool bilinear)
	{
		BlitTexture(cmd.m_WrappedCommandBuffer, source, scaleBias, mipLevel, bilinear);
	}

	public static void BlitTexture(CommandBuffer cmd, RTHandle source, Vector4 scaleBias, float mipLevel, bool bilinear)
	{
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevel);
		BlitTexture(cmd, source, scaleBias, GetBlitMaterial(TextureXR.dimension), s_BlitShaderPassIndicesMap[bilinear ? 1 : 0]);
	}

	public static void BlitTexture2D(RasterCommandBuffer cmd, RTHandle source, Vector4 scaleBias, float mipLevel, bool bilinear)
	{
		BlitTexture2D(cmd.m_WrappedCommandBuffer, source, scaleBias, mipLevel, bilinear);
	}

	public static void BlitTexture2D(CommandBuffer cmd, RTHandle source, Vector4 scaleBias, float mipLevel, bool bilinear)
	{
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevel);
		BlitTexture(cmd, source, scaleBias, GetBlitMaterial(TextureDimension.Tex2D), s_BlitShaderPassIndicesMap[bilinear ? 1 : 0]);
	}

	public static void BlitColorAndDepth(RasterCommandBuffer cmd, Texture sourceColor, RenderTexture sourceDepth, Vector4 scaleBias, float mipLevel, bool blitDepth)
	{
		BlitColorAndDepth(cmd.m_WrappedCommandBuffer, sourceColor, sourceDepth, scaleBias, mipLevel, blitDepth);
	}

	public static void BlitColorAndDepth(CommandBuffer cmd, Texture sourceColor, RenderTexture sourceDepth, Vector4 scaleBias, float mipLevel, bool blitDepth)
	{
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevel);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBias);
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, sourceColor);
		if (blitDepth)
		{
			s_PropertyBlock.SetTexture(BlitShaderIDs._InputDepth, sourceDepth, RenderTextureSubElement.Depth);
		}
		DrawTriangle(cmd, s_BlitColorAndDepth, s_BlitColorAndDepthShaderPassIndicesMap[blitDepth ? 1 : 0]);
	}

	public static void BlitTexture(RasterCommandBuffer cmd, RTHandle source, Vector4 scaleBias, Material material, int pass)
	{
		BlitTexture(cmd.m_WrappedCommandBuffer, source, scaleBias, material, pass);
	}

	public static void BlitTexture(UnsafeCommandBuffer cmd, RTHandle source, Vector4 scaleBias, Material material, int pass)
	{
		BlitTexture(cmd.m_WrappedCommandBuffer, source, scaleBias, material, pass);
	}

	public static void BlitTexture(CommandBuffer cmd, RTHandle source, Vector4 scaleBias, Material material, int pass)
	{
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBias);
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, source);
		DrawTriangle(cmd, material, pass);
	}

	public static void BlitTexture(RasterCommandBuffer cmd, RenderTargetIdentifier source, Vector4 scaleBias, Material material, int pass)
	{
		BlitTexture(cmd.m_WrappedCommandBuffer, source, scaleBias, material, pass);
	}

	public static void BlitTexture(CommandBuffer cmd, RenderTargetIdentifier source, Vector4 scaleBias, Material material, int pass)
	{
		s_PropertyBlock.Clear();
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBias);
		cmd.SetGlobalTexture(BlitShaderIDs._BlitTexture, source);
		DrawTriangle(cmd, material, pass);
	}

	public static void BlitTexture(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int pass)
	{
		s_PropertyBlock.Clear();
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, Vector2.one);
		cmd.SetGlobalTexture(BlitShaderIDs._BlitTexture, source);
		cmd.SetRenderTarget(destination);
		DrawTriangle(cmd, material, pass);
	}

	public static void BlitTexture(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction, Material material, int pass)
	{
		s_PropertyBlock.Clear();
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, Vector2.one);
		cmd.SetGlobalTexture(BlitShaderIDs._BlitTexture, source);
		cmd.SetRenderTarget(destination, loadAction, storeAction);
		DrawTriangle(cmd, material, pass);
	}

	public static void BlitTexture(CommandBuffer cmd, Vector4 scaleBias, Material material, int pass)
	{
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBias);
		DrawTriangle(cmd, material, pass);
	}

	public static void BlitTexture(RasterCommandBuffer cmd, Vector4 scaleBias, Material material, int pass)
	{
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBias);
		DrawTriangle(cmd, material, pass);
	}

	public static void BlitCameraTexture(CommandBuffer cmd, RTHandle source, RTHandle destination, float mipLevel = 0f, bool bilinear = false)
	{
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		CoreUtils.SetRenderTarget(cmd, destination);
		BlitTexture(cmd, source, vector, mipLevel, bilinear);
	}

	public static void BlitCameraTexture2D(CommandBuffer cmd, RTHandle source, RTHandle destination, float mipLevel = 0f, bool bilinear = false)
	{
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		CoreUtils.SetRenderTarget(cmd, destination);
		BlitTexture2D(cmd, source, vector, mipLevel, bilinear);
	}

	public static void BlitCameraTexture(CommandBuffer cmd, RTHandle source, RTHandle destination, Material material, int pass)
	{
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		CoreUtils.SetRenderTarget(cmd, destination);
		BlitTexture(cmd, source, vector, material, pass);
	}

	public static void BlitCameraTexture(CommandBuffer cmd, RTHandle source, RTHandle destination, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction, Material material, int pass)
	{
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		CoreUtils.SetRenderTarget(cmd, destination, loadAction, storeAction, ClearFlag.None, Color.clear);
		BlitTexture(cmd, source, vector, material, pass);
	}

	public static void BlitCameraTexture(CommandBuffer cmd, RTHandle source, RTHandle destination, Vector4 scaleBias, float mipLevel = 0f, bool bilinear = false)
	{
		CoreUtils.SetRenderTarget(cmd, destination);
		BlitTexture(cmd, source, scaleBias, mipLevel, bilinear);
	}

	public static void BlitCameraTexture(CommandBuffer cmd, RTHandle source, RTHandle destination, Rect destViewport, float mipLevel = 0f, bool bilinear = false)
	{
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		CoreUtils.SetRenderTarget(cmd, destination);
		cmd.SetViewport(destViewport);
		BlitTexture(cmd, source, vector, mipLevel, bilinear);
	}

	public static void BlitQuad(CommandBuffer cmd, Texture source, Vector4 scaleBiasTex, Vector4 scaleBiasRT, int mipLevelTex, bool bilinear)
	{
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, source);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBiasTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[bilinear ? 3 : 2]);
	}

	public static void BlitQuadWithPadding(CommandBuffer cmd, Texture source, Vector2 textureSize, Vector4 scaleBiasTex, Vector4 scaleBiasRT, int mipLevelTex, bool bilinear, int paddingInPixels)
	{
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, source);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBiasTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitTextureSize, textureSize);
		s_PropertyBlock.SetInt(BlitShaderIDs._BlitPaddingSize, paddingInPixels);
		if (source.wrapMode == TextureWrapMode.Repeat)
		{
			DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[bilinear ? 7 : 6]);
		}
		else
		{
			DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[bilinear ? 5 : 4]);
		}
	}

	public static void BlitQuadWithPaddingMultiply(CommandBuffer cmd, Texture source, Vector2 textureSize, Vector4 scaleBiasTex, Vector4 scaleBiasRT, int mipLevelTex, bool bilinear, int paddingInPixels)
	{
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, source);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBiasTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitTextureSize, textureSize);
		s_PropertyBlock.SetInt(BlitShaderIDs._BlitPaddingSize, paddingInPixels);
		if (source.wrapMode == TextureWrapMode.Repeat)
		{
			DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[bilinear ? 12 : 11]);
		}
		else
		{
			DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[bilinear ? 10 : 9]);
		}
	}

	public static void BlitOctahedralWithPadding(CommandBuffer cmd, Texture source, Vector2 textureSize, Vector4 scaleBiasTex, Vector4 scaleBiasRT, int mipLevelTex, bool bilinear, int paddingInPixels)
	{
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, source);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBiasTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitTextureSize, textureSize);
		s_PropertyBlock.SetInt(BlitShaderIDs._BlitPaddingSize, paddingInPixels);
		DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[8]);
	}

	public static void BlitOctahedralWithPaddingMultiply(CommandBuffer cmd, Texture source, Vector2 textureSize, Vector4 scaleBiasTex, Vector4 scaleBiasRT, int mipLevelTex, bool bilinear, int paddingInPixels)
	{
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, source);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBiasTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitTextureSize, textureSize);
		s_PropertyBlock.SetInt(BlitShaderIDs._BlitPaddingSize, paddingInPixels);
		DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[13]);
	}

	public static void BlitCubeToOctahedral2DQuad(CommandBuffer cmd, Texture source, Vector4 scaleBiasRT, int mipLevelTex)
	{
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitCubeTexture, source);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[14]);
	}

	public static void BlitCubeToOctahedral2DQuadWithPadding(CommandBuffer cmd, Texture source, Vector2 textureSize, Vector4 scaleBiasRT, int mipLevelTex, bool bilinear, int paddingInPixels, Vector4? decodeInstructions = null)
	{
		Material blitMaterial = GetBlitMaterial(source.dimension);
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitCubeTexture, source);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitTextureSize, textureSize);
		s_PropertyBlock.SetInt(BlitShaderIDs._BlitPaddingSize, paddingInPixels);
		cmd.SetKeyword(blitMaterial, in s_DecodeHdrKeyword, decodeInstructions.HasValue);
		if (decodeInstructions.HasValue)
		{
			s_PropertyBlock.SetVector(BlitShaderIDs._BlitDecodeInstructions, decodeInstructions.Value);
		}
		DrawQuad(cmd, blitMaterial, s_BlitShaderPassIndicesMap[bilinear ? 22 : 21]);
		cmd.SetKeyword(blitMaterial, in s_DecodeHdrKeyword, value: false);
	}

	public static void BlitCubeToOctahedral2DQuadSingleChannel(CommandBuffer cmd, Texture source, Vector4 scaleBiasRT, int mipLevelTex)
	{
		int num = 15;
		if (GraphicsFormatUtility.GetComponentCount(source.graphicsFormat) == 1)
		{
			if (GraphicsFormatUtility.IsAlphaOnlyFormat(source.graphicsFormat))
			{
				num = 16;
			}
			if (GraphicsFormatUtility.GetSwizzleR(source.graphicsFormat) == FormatSwizzle.FormatSwizzleR)
			{
				num = 17;
			}
		}
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitCubeTexture, source);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[num]);
	}

	public static void BlitQuadSingleChannel(CommandBuffer cmd, Texture source, Vector4 scaleBiasTex, Vector4 scaleBiasRT, int mipLevelTex)
	{
		int num = 18;
		if (GraphicsFormatUtility.GetComponentCount(source.graphicsFormat) == 1)
		{
			if (GraphicsFormatUtility.IsAlphaOnlyFormat(source.graphicsFormat))
			{
				num = 19;
			}
			if (GraphicsFormatUtility.GetSwizzleR(source.graphicsFormat) == FormatSwizzle.FormatSwizzleR)
			{
				num = 20;
			}
		}
		s_PropertyBlock.SetTexture(BlitShaderIDs._BlitTexture, source);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBias, scaleBiasTex);
		s_PropertyBlock.SetVector(BlitShaderIDs._BlitScaleBiasRt, scaleBiasRT);
		s_PropertyBlock.SetFloat(BlitShaderIDs._BlitMipLevel, mipLevelTex);
		DrawQuad(cmd, GetBlitMaterial(source.dimension), s_BlitShaderPassIndicesMap[num]);
	}
}
