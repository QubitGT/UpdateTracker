using System;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace UnityEngine.Experimental.Rendering;

internal static class XRMirrorView
{
	private static readonly MaterialPropertyBlock s_MirrorViewMaterialProperty = new MaterialPropertyBlock();

	private static readonly ProfilingSampler k_MirrorViewProfilingSampler = new ProfilingSampler("XR Mirror View");

	private static readonly int k_SourceTex = Shader.PropertyToID("_SourceTex");

	private static readonly int k_SourceTexArraySlice = Shader.PropertyToID("_SourceTexArraySlice");

	private static readonly int k_ScaleBias = Shader.PropertyToID("_ScaleBias");

	private static readonly int k_ScaleBiasRt = Shader.PropertyToID("_ScaleBiasRt");

	private static readonly int k_SRGBRead = Shader.PropertyToID("_SRGBRead");

	private static readonly int k_SRGBWrite = Shader.PropertyToID("_SRGBWrite");

	private static readonly int k_MaxNits = Shader.PropertyToID("_MaxNits");

	private static readonly int k_SourceMaxNits = Shader.PropertyToID("_SourceMaxNits");

	private static readonly int k_SourceHDREncoding = Shader.PropertyToID("_SourceHDREncoding");

	private static readonly int k_ColorTransform = Shader.PropertyToID("_ColorTransform");

	internal static void RenderMirrorView(CommandBuffer cmd, Camera camera, Material mat, XRDisplaySubsystem display)
	{
		if ((Application.platform == RuntimePlatform.Android && !XRGraphicsAutomatedTests.running) || display == null || !display.running || mat == null)
		{
			return;
		}
		int preferredMirrorBlitMode = display.GetPreferredMirrorBlitMode();
		if (display.GetMirrorViewBlitDesc(null, out var outDesc, preferredMirrorBlitMode))
		{
			using (new ProfilingScope(cmd, k_MirrorViewProfilingSampler))
			{
				cmd.SetRenderTarget((camera.targetTexture != null) ? ((RenderTargetIdentifier)camera.targetTexture) : new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));
				if (outDesc.nativeBlitAvailable)
				{
					display.AddGraphicsThreadMirrorViewBlit(cmd, outDesc.nativeBlitInvalidStates, preferredMirrorBlitMode);
				}
				else
				{
					for (int i = 0; i < outDesc.blitParamsCount; i++)
					{
						outDesc.GetBlitParameter(i, out var blitParameter);
						Vector4 value = new Vector4(blitParameter.srcRect.width, blitParameter.srcRect.height, blitParameter.srcRect.x, blitParameter.srcRect.y);
						Vector4 value2 = new Vector4(blitParameter.destRect.width, blitParameter.destRect.height, blitParameter.destRect.x, blitParameter.destRect.y);
						if (camera.targetTexture != null || camera.cameraType == CameraType.SceneView || camera.cameraType == CameraType.Preview)
						{
							value.y = 0f - value.y;
							value.w += blitParameter.srcRect.height;
						}
						HDROutputSettings main = HDROutputSettings.main;
						if (blitParameter.srcHdrEncoded || main.active)
						{
							ColorGamut gamut = (main.active ? main.displayColorGamut : ColorGamut.sRGB);
							ColorGamut gamut2 = (blitParameter.srcHdrEncoded ? blitParameter.srcHdrColorGamut : ColorGamut.sRGB);
							ColorPrimaries colorPrimaries = ColorGamutUtility.GetColorPrimaries(gamut);
							ColorPrimaries colorPrimaries2 = ColorGamutUtility.GetColorPrimaries(gamut2);
							HDROutputUtils.ConfigureHDROutput(s_MirrorViewMaterialProperty, gamut);
							HDROutputUtils.ConfigureHDROutput(mat, HDROutputUtils.Operation.ColorConversion | HDROutputUtils.Operation.ColorEncoding);
							HDROutputUtils.GetColorEncodingForGamut(gamut2, out var encoding);
							s_MirrorViewMaterialProperty.SetInteger(k_SourceHDREncoding, encoding);
							float3x3 a = float3x3.identity;
							switch (colorPrimaries2)
							{
							case ColorPrimaries.Rec709:
								a = ColorSpaceUtils.Rec709ToRec2020Mat;
								break;
							case ColorPrimaries.P3:
								a = ColorSpaceUtils.P3D65ToRec2020Mat;
								break;
							}
							float3x3 b = float3x3.identity;
							switch (colorPrimaries)
							{
							case ColorPrimaries.Rec709:
								b = ColorSpaceUtils.Rec2020ToRec709Mat;
								break;
							case ColorPrimaries.P3:
								b = ColorSpaceUtils.Rec2020ToP3D65Mat;
								break;
							}
							float3x3 float3x = math.mul(a, b);
							Matrix4x4 value3 = new Matrix4x4(new float4(float3x.c0, 0f), new float4(float3x.c1, 0f), new float4(float3x.c2, 0f), new Vector4(0f, 0f, 0f, 0f));
							s_MirrorViewMaterialProperty.SetMatrix(k_ColorTransform, value3);
							s_MirrorViewMaterialProperty.SetFloat(k_MaxNits, main.active ? ((float)main.maxToneMapLuminance) : 160f);
							s_MirrorViewMaterialProperty.SetFloat(k_SourceMaxNits, blitParameter.srcHdrEncoded ? ((float)blitParameter.srcHdrMaxLuminance) : 160f);
						}
						bool flag = !blitParameter.srcTex.sRGB && (blitParameter.srcTex.graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || blitParameter.srcTex.graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm);
						s_MirrorViewMaterialProperty.SetFloat(k_SRGBRead, flag ? 1f : 0f);
						s_MirrorViewMaterialProperty.SetFloat(k_SRGBWrite, (QualitySettings.activeColorSpace == ColorSpace.Linear) ? 0f : 1f);
						s_MirrorViewMaterialProperty.SetTexture(k_SourceTex, blitParameter.srcTex);
						s_MirrorViewMaterialProperty.SetVector(k_ScaleBias, value);
						s_MirrorViewMaterialProperty.SetVector(k_ScaleBiasRt, value2);
						s_MirrorViewMaterialProperty.SetFloat(k_SourceTexArraySlice, blitParameter.srcTexArraySlice);
						if (XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster) && blitParameter.foveatedRenderingInfo != IntPtr.Zero)
						{
							cmd.ConfigureFoveatedRendering(blitParameter.foveatedRenderingInfo);
							cmd.EnableShaderKeyword("_FOVEATED_RENDERING_NON_UNIFORM_RASTER");
						}
						if (blitParameter.srcTex.dimension != TextureDimension.Tex2DArray)
						{
							cmd.EnableShaderKeyword("DISABLE_TEXTURE2D_X_ARRAY");
						}
						cmd.DrawProcedural(Matrix4x4.identity, mat, 0, MeshTopology.Quads, 4, 1, s_MirrorViewMaterialProperty);
						if (blitParameter.srcTex.dimension != TextureDimension.Tex2DArray && TextureXR.useTexArray)
						{
							cmd.DisableShaderKeyword("DISABLE_TEXTURE2D_X_ARRAY");
						}
					}
				}
			}
		}
		if (XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster))
		{
			cmd.DisableShaderKeyword("_FOVEATED_RENDERING_NON_UNIFORM_RASTER");
			cmd.ConfigureFoveatedRendering(IntPtr.Zero);
		}
	}
}
