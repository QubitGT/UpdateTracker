using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

public static class ShadowUtils
{
	internal static readonly bool m_ForceShadowPointSampling;

	internal const int kMinimumPunctualLightHardShadowResolution = 8;

	internal const int kMinimumPunctualLightSoftShadowResolution = 16;

	static ShadowUtils()
	{
		m_ForceShadowPointSampling = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal && GraphicsSettings.HasShaderDefine(Graphics.activeTier, BuiltinShaderDefine.UNITY_METAL_SHADOWS_USE_POINT_FILTERING);
	}

	public static bool ExtractDirectionalLightMatrix(ref CullingResults cullResults, ref ShadowData shadowData, int shadowLightIndex, int cascadeIndex, int shadowmapWidth, int shadowmapHeight, int shadowResolution, float shadowNearPlane, out Vector4 cascadeSplitDistance, out ShadowSliceData shadowSliceData, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix)
	{
		bool result = ExtractDirectionalLightMatrix(ref cullResults, ref shadowData, shadowLightIndex, cascadeIndex, shadowmapWidth, shadowmapHeight, shadowResolution, shadowNearPlane, out cascadeSplitDistance, out shadowSliceData);
		viewMatrix = shadowSliceData.viewMatrix;
		projMatrix = shadowSliceData.projectionMatrix;
		return result;
	}

	public static bool ExtractDirectionalLightMatrix(ref CullingResults cullResults, ref ShadowData shadowData, int shadowLightIndex, int cascadeIndex, int shadowmapWidth, int shadowmapHeight, int shadowResolution, float shadowNearPlane, out Vector4 cascadeSplitDistance, out ShadowSliceData shadowSliceData)
	{
		return ExtractDirectionalLightMatrix(ref cullResults, shadowData.universalShadowData, shadowLightIndex, cascadeIndex, shadowmapWidth, shadowmapHeight, shadowResolution, shadowNearPlane, out cascadeSplitDistance, out shadowSliceData);
	}

	public static bool ExtractDirectionalLightMatrix(ref CullingResults cullResults, UniversalShadowData shadowData, int shadowLightIndex, int cascadeIndex, int shadowmapWidth, int shadowmapHeight, int shadowResolution, float shadowNearPlane, out Vector4 cascadeSplitDistance, out ShadowSliceData shadowSliceData)
	{
		bool result = cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex, cascadeIndex, shadowData.mainLightShadowCascadesCount, shadowData.mainLightShadowCascadesSplit, shadowResolution, shadowNearPlane, out shadowSliceData.viewMatrix, out shadowSliceData.projectionMatrix, out shadowSliceData.splitData);
		cascadeSplitDistance = shadowSliceData.splitData.cullingSphere;
		shadowSliceData.offsetX = cascadeIndex % 2 * shadowResolution;
		shadowSliceData.offsetY = cascadeIndex / 2 * shadowResolution;
		shadowSliceData.resolution = shadowResolution;
		shadowSliceData.shadowTransform = GetShadowTransform(shadowSliceData.projectionMatrix, shadowSliceData.viewMatrix);
		shadowSliceData.splitData.shadowCascadeBlendCullingFactor = 1f;
		if (shadowData.mainLightShadowCascadesCount > 1)
		{
			ApplySliceTransform(ref shadowSliceData, shadowmapWidth, shadowmapHeight);
		}
		return result;
	}

	public static bool ExtractSpotLightMatrix(ref CullingResults cullResults, ref ShadowData shadowData, int shadowLightIndex, out Matrix4x4 shadowMatrix, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData)
	{
		return ExtractSpotLightMatrix(ref cullResults, shadowData.universalShadowData, shadowLightIndex, out shadowMatrix, out viewMatrix, out projMatrix, out splitData);
	}

	public static bool ExtractSpotLightMatrix(ref CullingResults cullResults, UniversalShadowData shadowData, int shadowLightIndex, out Matrix4x4 shadowMatrix, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData)
	{
		bool result = cullResults.ComputeSpotShadowMatricesAndCullingPrimitives(shadowLightIndex, out viewMatrix, out projMatrix, out splitData);
		shadowMatrix = GetShadowTransform(projMatrix, viewMatrix);
		return result;
	}

	public static bool ExtractPointLightMatrix(ref CullingResults cullResults, ref ShadowData shadowData, int shadowLightIndex, CubemapFace cubemapFace, float fovBias, out Matrix4x4 shadowMatrix, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData)
	{
		return ExtractPointLightMatrix(ref cullResults, shadowData.universalShadowData, shadowLightIndex, cubemapFace, fovBias, out shadowMatrix, out viewMatrix, out projMatrix, out splitData);
	}

	public static bool ExtractPointLightMatrix(ref CullingResults cullResults, UniversalShadowData shadowData, int shadowLightIndex, CubemapFace cubemapFace, float fovBias, out Matrix4x4 shadowMatrix, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData)
	{
		bool result = cullResults.ComputePointShadowMatricesAndCullingPrimitives(shadowLightIndex, cubemapFace, fovBias, out viewMatrix, out projMatrix, out splitData);
		viewMatrix.m10 = 0f - viewMatrix.m10;
		viewMatrix.m11 = 0f - viewMatrix.m11;
		viewMatrix.m12 = 0f - viewMatrix.m12;
		viewMatrix.m13 = 0f - viewMatrix.m13;
		shadowMatrix = GetShadowTransform(projMatrix, viewMatrix);
		return result;
	}

	public static void RenderShadowSlice(CommandBuffer cmd, ref ScriptableRenderContext context, ref ShadowSliceData shadowSliceData, ref ShadowDrawingSettings settings, Matrix4x4 proj, Matrix4x4 view)
	{
		cmd.SetGlobalDepthBias(1f, 2.5f);
		cmd.SetViewport(new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution));
		cmd.SetViewProjectionMatrices(view, proj);
		RendererList rendererList = context.CreateShadowRendererList(ref settings);
		cmd.DrawRendererList(rendererList);
		cmd.DisableScissorRect();
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		cmd.SetGlobalDepthBias(0f, 0f);
	}

	internal static void RenderShadowSlice(RasterCommandBuffer cmd, ref ShadowSliceData shadowSliceData, ref RendererList shadowRendererList, Matrix4x4 proj, Matrix4x4 view)
	{
		cmd.SetGlobalDepthBias(1f, 2.5f);
		cmd.SetViewport(new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution));
		cmd.SetViewProjectionMatrices(view, proj);
		if (shadowRendererList.isValid)
		{
			cmd.DrawRendererList(shadowRendererList);
		}
		cmd.DisableScissorRect();
		cmd.SetGlobalDepthBias(0f, 0f);
	}

	public static void RenderShadowSlice(CommandBuffer cmd, ref ScriptableRenderContext context, ref ShadowSliceData shadowSliceData, ref ShadowDrawingSettings settings)
	{
		RenderShadowSlice(cmd, ref context, ref shadowSliceData, ref settings, shadowSliceData.projectionMatrix, shadowSliceData.viewMatrix);
	}

	public static int GetMaxTileResolutionInAtlas(int atlasWidth, int atlasHeight, int tileCount)
	{
		int num = Mathf.Min(atlasWidth, atlasHeight);
		for (int num2 = atlasWidth / num * atlasHeight / num; num2 < tileCount; num2 = atlasWidth / num * atlasHeight / num)
		{
			num >>= 1;
		}
		return num;
	}

	public static void ApplySliceTransform(ref ShadowSliceData shadowSliceData, int atlasWidth, int atlasHeight)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		float num = 1f / (float)atlasWidth;
		float num2 = 1f / (float)atlasHeight;
		identity.m00 = (float)shadowSliceData.resolution * num;
		identity.m11 = (float)shadowSliceData.resolution * num2;
		identity.m03 = (float)shadowSliceData.offsetX * num;
		identity.m13 = (float)shadowSliceData.offsetY * num2;
		shadowSliceData.shadowTransform = identity * shadowSliceData.shadowTransform;
	}

	public static Vector4 GetShadowBias(ref VisibleLight shadowLight, int shadowLightIndex, ref ShadowData shadowData, Matrix4x4 lightProjectionMatrix, float shadowResolution)
	{
		return GetShadowBias(ref shadowLight, shadowLightIndex, shadowData.bias, shadowData.supportsSoftShadows, lightProjectionMatrix, shadowResolution);
	}

	public static Vector4 GetShadowBias(ref VisibleLight shadowLight, int shadowLightIndex, UniversalShadowData shadowData, Matrix4x4 lightProjectionMatrix, float shadowResolution)
	{
		return GetShadowBias(ref shadowLight, shadowLightIndex, shadowData.bias, shadowData.supportsSoftShadows, lightProjectionMatrix, shadowResolution);
	}

	private static Vector4 GetShadowBias(ref VisibleLight shadowLight, int shadowLightIndex, List<Vector4> bias, bool supportsSoftShadows, Matrix4x4 lightProjectionMatrix, float shadowResolution)
	{
		if (shadowLightIndex < 0 || shadowLightIndex >= bias.Count)
		{
			Debug.LogWarning($"{shadowLightIndex} is not a valid light index.");
			return Vector4.zero;
		}
		float num;
		if (shadowLight.lightType == LightType.Directional)
		{
			num = 2f / lightProjectionMatrix.m00;
		}
		else if (shadowLight.lightType == LightType.Spot)
		{
			num = Mathf.Tan(shadowLight.spotAngle * 0.5f * (MathF.PI / 180f)) * shadowLight.range;
		}
		else if (shadowLight.lightType == LightType.Point)
		{
			float pointLightShadowFrustumFovBiasInDegrees = AdditionalLightsShadowCasterPass.GetPointLightShadowFrustumFovBiasInDegrees((int)shadowResolution, shadowLight.light.shadows == LightShadows.Soft);
			num = Mathf.Tan((90f + pointLightShadowFrustumFovBiasInDegrees) * 0.5f * (MathF.PI / 180f)) * shadowLight.range;
		}
		else
		{
			Debug.LogWarning("Only point, spot and directional shadow casters are supported in universal pipeline");
			num = 0f;
		}
		float num2 = num / shadowResolution;
		float num3 = (0f - bias[shadowLightIndex].x) * num2;
		float num4 = (0f - bias[shadowLightIndex].y) * num2;
		if (shadowLight.lightType == LightType.Point)
		{
			num4 = 0f;
		}
		if (supportsSoftShadows && shadowLight.light.shadows == LightShadows.Soft)
		{
			SoftShadowQuality softShadowQuality = SoftShadowQuality.Medium;
			if (shadowLight.light.TryGetComponent<UniversalAdditionalLightData>(out var component))
			{
				softShadowQuality = component.softShadowQuality;
			}
			float num5 = 2.5f;
			switch (softShadowQuality)
			{
			case SoftShadowQuality.High:
				num5 = 3.5f;
				break;
			case SoftShadowQuality.Medium:
				num5 = 2.5f;
				break;
			case SoftShadowQuality.Low:
				num5 = 1.5f;
				break;
			}
			num3 *= num5;
			num4 *= num5;
		}
		return new Vector4(num3, num4, (float)shadowLight.lightType, 0f);
	}

	internal static void GetScaleAndBiasForLinearDistanceFade(float fadeDistance, float border, out float scale, out float bias)
	{
		if (border < 0.0001f)
		{
			bias = (0f - fadeDistance) * (scale = 1000f);
			return;
		}
		border = 1f - border;
		border *= border;
		float num = border * fadeDistance;
		scale = 1f / (fadeDistance - num);
		bias = (0f - num) / (fadeDistance - num);
	}

	public static void SetupShadowCasterConstantBuffer(CommandBuffer cmd, ref VisibleLight shadowLight, Vector4 shadowBias)
	{
		SetupShadowCasterConstantBuffer(CommandBufferHelpers.GetRasterCommandBuffer(cmd), ref shadowLight, shadowBias);
	}

	internal static void SetupShadowCasterConstantBuffer(RasterCommandBuffer cmd, ref VisibleLight shadowLight, Vector4 shadowBias)
	{
		SetShadowBias(cmd, shadowBias);
		Vector3 lightDirection = -shadowLight.localToWorldMatrix.GetColumn(2);
		SetLightDirection(cmd, lightDirection);
		Vector3 lightPosition = shadowLight.localToWorldMatrix.GetColumn(3);
		SetLightPosition(cmd, lightPosition);
	}

	internal static void SetShadowBias(RasterCommandBuffer cmd, Vector4 shadowBias)
	{
		cmd.SetGlobalVector(ShaderPropertyId.shadowBias, shadowBias);
	}

	internal static void SetLightDirection(RasterCommandBuffer cmd, Vector3 lightDirection)
	{
		cmd.SetGlobalVector(ShaderPropertyId.lightDirection, new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0f));
	}

	internal static void SetLightPosition(RasterCommandBuffer cmd, Vector3 lightPosition)
	{
		cmd.SetGlobalVector(ShaderPropertyId.lightPosition, new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1f));
	}

	internal static void SetCameraPosition(RasterCommandBuffer cmd, Vector3 worldSpaceCameraPos)
	{
		cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, worldSpaceCameraPos);
	}

	internal static void SetWorldToCameraAndCameraToWorldMatrices(RasterCommandBuffer cmd, Matrix4x4 viewMatrix)
	{
		Matrix4x4 value = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * viewMatrix;
		Matrix4x4 inverse = value.inverse;
		cmd.SetGlobalMatrix(ShaderPropertyId.worldToCameraMatrix, value);
		cmd.SetGlobalMatrix(ShaderPropertyId.cameraToWorldMatrix, inverse);
	}

	private static RenderTextureDescriptor GetTemporaryShadowTextureDescriptor(int width, int height, int bits)
	{
		GraphicsFormat depthStencilFormat = GraphicsFormatUtility.GetDepthStencilFormat(bits, 0);
		RenderTextureDescriptor result = new RenderTextureDescriptor(width, height, GraphicsFormat.None, depthStencilFormat);
		result.shadowSamplingMode = ((!RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap)) ? ShadowSamplingMode.None : ShadowSamplingMode.CompareDepths);
		return result;
	}

	[Obsolete("Use AllocShadowRT or ShadowRTReAllocateIfNeeded", true)]
	public static RenderTexture GetTemporaryShadowTexture(int width, int height, int bits)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(GetTemporaryShadowTextureDescriptor(width, height, bits));
		temporary.filterMode = ((!m_ForceShadowPointSampling) ? FilterMode.Bilinear : FilterMode.Point);
		temporary.wrapMode = TextureWrapMode.Clamp;
		return temporary;
	}

	public static bool ShadowRTNeedsReAlloc(RTHandle handle, int width, int height, int bits, int anisoLevel, float mipMapBias, string name)
	{
		if (handle == null || handle.rt == null)
		{
			return true;
		}
		RenderTextureDescriptor temporaryShadowTextureDescriptor = GetTemporaryShadowTextureDescriptor(width, height, bits);
		if (m_ForceShadowPointSampling)
		{
			if (handle.rt.filterMode != 0)
			{
				return true;
			}
		}
		else if (handle.rt.filterMode != FilterMode.Bilinear)
		{
			return true;
		}
		TextureDesc descriptor = RTHandleResourcePool.CreateTextureDesc(temporaryShadowTextureDescriptor, TextureSizeMode.Explicit, anisoLevel, mipMapBias, (!m_ForceShadowPointSampling) ? FilterMode.Bilinear : FilterMode.Point, TextureWrapMode.Clamp, name);
		return RenderingUtils.RTHandleNeedsReAlloc(handle, in descriptor, scaled: false);
	}

	public static RTHandle AllocShadowRT(int width, int height, int bits, int anisoLevel, float mipMapBias, string name)
	{
		RenderTextureDescriptor descriptor = GetTemporaryShadowTextureDescriptor(width, height, bits);
		return RTHandles.Alloc(in descriptor, (!m_ForceShadowPointSampling) ? FilterMode.Bilinear : FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: true, 1, 0f, name);
	}

	public static bool ShadowRTReAllocateIfNeeded(ref RTHandle handle, int width, int height, int bits, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		if (ShadowRTNeedsReAlloc(handle, width, height, bits, anisoLevel, mipMapBias, name))
		{
			handle?.Release();
			handle = AllocShadowRT(width, height, bits, anisoLevel, mipMapBias, name);
			return true;
		}
		return false;
	}

	private static Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
	{
		if (SystemInfo.usesReversedZBuffer)
		{
			proj.m20 = 0f - proj.m20;
			proj.m21 = 0f - proj.m21;
			proj.m22 = 0f - proj.m22;
			proj.m23 = 0f - proj.m23;
		}
		Matrix4x4 matrix4x = proj * view;
		Matrix4x4 identity = Matrix4x4.identity;
		identity.m00 = 0.5f;
		identity.m11 = 0.5f;
		identity.m22 = 0.5f;
		identity.m03 = 0.5f;
		identity.m23 = 0.5f;
		identity.m13 = 0.5f;
		return identity * matrix4x;
	}

	internal static float SoftShadowQualityToShaderProperty(Light light, bool softShadowsEnabled)
	{
		float num = (softShadowsEnabled ? 1f : 0f);
		if (light.TryGetComponent<UniversalAdditionalLightData>(out var component))
		{
			num *= (float)Math.Max((int)((component.softShadowQuality != 0) ? new SoftShadowQuality?(component.softShadowQuality) : UniversalRenderPipeline.asset?.softShadowQuality).Value, 1);
		}
		return num;
	}

	internal static bool SupportsPerLightSoftShadowQuality()
	{
		return true;
	}

	internal static void SetPerLightSoftShadowKeyword(RasterCommandBuffer cmd, bool hasSoftShadows)
	{
		if (SupportsPerLightSoftShadowQuality())
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadows, hasSoftShadows);
		}
	}

	internal static void SetSoftShadowQualityShaderKeywords(RasterCommandBuffer cmd, UniversalShadowData shadowData)
	{
		cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadows, shadowData.isKeywordSoftShadowsEnabled);
		if (SupportsPerLightSoftShadowQuality())
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsLow, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsMedium, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsHigh, value: false);
			return;
		}
		if (shadowData.isKeywordSoftShadowsEnabled)
		{
			UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
			if ((object)asset != null && asset.softShadowQuality == SoftShadowQuality.Low)
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsLow, value: true);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsMedium, value: false);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsHigh, value: false);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadows, value: false);
				return;
			}
		}
		if (shadowData.isKeywordSoftShadowsEnabled)
		{
			UniversalRenderPipelineAsset asset2 = UniversalRenderPipeline.asset;
			if ((object)asset2 != null && asset2.softShadowQuality == SoftShadowQuality.Medium)
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsLow, value: false);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsMedium, value: true);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsHigh, value: false);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadows, value: false);
				return;
			}
		}
		if (shadowData.isKeywordSoftShadowsEnabled)
		{
			UniversalRenderPipelineAsset asset3 = UniversalRenderPipeline.asset;
			if ((object)asset3 != null && asset3.softShadowQuality == SoftShadowQuality.High)
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsLow, value: false);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsMedium, value: false);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsHigh, value: true);
				cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadows, value: false);
			}
		}
	}

	internal static bool IsValidShadowCastingLight(UniversalLightData lightData, int i)
	{
		ref VisibleLight reference = ref lightData.visibleLights.UnsafeElementAt(i);
		Light light = reference.light;
		if (light == null)
		{
			return false;
		}
		return IsValidShadowCastingLight(lightData, i, reference.lightType, light.shadows, light.shadowStrength);
	}

	internal static bool IsValidShadowCastingLight(UniversalLightData lightData, int i, LightType lightType, LightShadows lightShadows, float shadowStrength)
	{
		if (i == lightData.mainLightIndex)
		{
			return false;
		}
		if (lightType == LightType.Directional)
		{
			return false;
		}
		if (lightShadows != 0)
		{
			return shadowStrength > 0f;
		}
		return false;
	}

	internal static int GetPunctualLightShadowSlicesCount(in LightType lightType)
	{
		return lightType switch
		{
			LightType.Spot => 1, 
			LightType.Point => 6, 
			_ => 0, 
		};
	}

	internal static bool FastApproximately(float a, float b)
	{
		return Mathf.Abs(a - b) < 1E-06f;
	}

	internal static bool FastApproximately(Vector4 a, Vector4 b)
	{
		if (FastApproximately(a.x, b.x) && FastApproximately(a.y, b.y) && FastApproximately(a.z, b.z))
		{
			return FastApproximately(a.w, b.w);
		}
		return false;
	}

	internal static int MinimalPunctualLightShadowResolution(bool softShadow)
	{
		if (!softShadow)
		{
			return 8;
		}
		return 16;
	}
}
