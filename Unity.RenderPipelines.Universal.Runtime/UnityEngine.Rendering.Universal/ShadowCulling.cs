using Unity.Collections;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal static class ShadowCulling
{
	private static readonly ProfilingSampler computeShadowCasterCullingInfosMarker = new ProfilingSampler("UniversalRenderPipeline.ComputeShadowCasterCullingInfos");

	public static NativeArray<URPLightShadowCullingInfos> CullShadowCasters(ref ScriptableRenderContext context, UniversalShadowData shadowData, ref AdditionalLightsShadowAtlasLayout shadowAtlasLayout, ref CullingResults cullResults)
	{
		ComputeShadowCasterCullingInfos(shadowData, ref shadowAtlasLayout, ref cullResults, out var shadowCullingInfos, out var urpVisibleLightsShadowCullingInfos);
		context.CullShadowCasters(cullResults, shadowCullingInfos);
		return urpVisibleLightsShadowCullingInfos;
	}

	private static void ComputeShadowCasterCullingInfos(UniversalShadowData shadowData, ref AdditionalLightsShadowAtlasLayout shadowAtlasLayout, ref CullingResults cullingResults, out ShadowCastersCullingInfos shadowCullingInfos, out NativeArray<URPLightShadowCullingInfos> urpVisibleLightsShadowCullingInfos)
	{
		using (new ProfilingScope(computeShadowCasterCullingInfosMarker))
		{
			NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
			NativeArray<ShadowSplitData> nativeArray = new NativeArray<ShadowSplitData>(visibleLights.Length * 6, Allocator.Temp);
			NativeArray<LightShadowCasterCullingInfo> perLightInfos = new NativeArray<LightShadowCasterCullingInfo>(visibleLights.Length, Allocator.Temp);
			urpVisibleLightsShadowCullingInfos = new NativeArray<URPLightShadowCullingInfos>(visibleLights.Length, Allocator.Temp);
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < visibleLights.Length; i++)
			{
				ref VisibleLight reference = ref cullingResults.visibleLights.UnsafeElementAt(i);
				LightType lightType = reference.lightType;
				NativeArray<ShadowSliceData> slices = default(NativeArray<ShadowSliceData>);
				uint num3 = 0u;
				switch (lightType)
				{
				case LightType.Directional:
				{
					if (!shadowData.supportsMainLightShadows)
					{
						continue;
					}
					int mainLightShadowCascadesCount = shadowData.mainLightShadowCascadesCount;
					int mainLightRenderTargetWidth = shadowData.mainLightRenderTargetWidth;
					int mainLightRenderTargetHeight = shadowData.mainLightRenderTargetHeight;
					int mainLightShadowResolution = shadowData.mainLightShadowResolution;
					slices = new NativeArray<ShadowSliceData>(mainLightShadowCascadesCount, Allocator.Temp);
					num3 = 0u;
					for (int j = 0; j < mainLightShadowCascadesCount; j++)
					{
						ShadowSliceData shadowSliceData = default(ShadowSliceData);
						if (ShadowUtils.ExtractDirectionalLightMatrix(ref cullingResults, shadowData, i, j, mainLightRenderTargetWidth, mainLightRenderTargetHeight, mainLightShadowResolution, reference.light.shadowNearPlane, out var _, out shadowSliceData))
						{
							num3 |= (uint)(1 << j);
						}
						slices[j] = shadowSliceData;
						nativeArray[num2 + j] = shadowSliceData.splitData;
					}
					break;
				}
				case LightType.Point:
				{
					if (!shadowData.supportsAdditionalLightShadows || !shadowAtlasLayout.HasSpaceForLight(i))
					{
						continue;
					}
					int punctualLightShadowSlicesCount = ShadowUtils.GetPunctualLightShadowSlicesCount(in lightType);
					ushort allocatedResolution = shadowAtlasLayout.GetSliceShadowResolutionRequest(i, 0).allocatedResolution;
					bool shadowFiltering = reference.light.shadows == LightShadows.Soft;
					float pointLightShadowFrustumFovBiasInDegrees = AdditionalLightsShadowCasterPass.GetPointLightShadowFrustumFovBiasInDegrees(allocatedResolution, shadowFiltering);
					slices = new NativeArray<ShadowSliceData>(punctualLightShadowSlicesCount, Allocator.Temp);
					num3 = 0u;
					for (int k = 0; k < punctualLightShadowSlicesCount; k++)
					{
						ShadowSliceData value2 = default(ShadowSliceData);
						if (ShadowUtils.ExtractPointLightMatrix(ref cullingResults, shadowData, i, (CubemapFace)k, pointLightShadowFrustumFovBiasInDegrees, out value2.shadowTransform, out value2.viewMatrix, out value2.projectionMatrix, out value2.splitData))
						{
							num3 |= (uint)(1 << k);
						}
						slices[k] = value2;
						nativeArray[num2 + k] = value2.splitData;
					}
					break;
				}
				case LightType.Spot:
				{
					if (!shadowData.supportsAdditionalLightShadows || !shadowAtlasLayout.HasSpaceForLight(i))
					{
						continue;
					}
					slices = new NativeArray<ShadowSliceData>(1, Allocator.Temp);
					num3 = 0u;
					ShadowSliceData value = default(ShadowSliceData);
					if (ShadowUtils.ExtractSpotLightMatrix(ref cullingResults, shadowData, i, out value.shadowTransform, out value.viewMatrix, out value.projectionMatrix, out value.splitData))
					{
						num3 |= 1u;
					}
					slices[0] = value;
					nativeArray[num2] = value.splitData;
					break;
				}
				}
				URPLightShadowCullingInfos value3 = default(URPLightShadowCullingInfos);
				value3.slices = slices;
				value3.slicesValidMask = num3;
				urpVisibleLightsShadowCullingInfos[i] = value3;
				perLightInfos[i] = new LightShadowCasterCullingInfo
				{
					splitRange = new RangeInt(num2, slices.Length),
					projectionType = GetCullingProjectionType(lightType)
				};
				num2 += slices.Length;
				num += slices.Length;
			}
			shadowCullingInfos = default(ShadowCastersCullingInfos);
			shadowCullingInfos.splitBuffer = nativeArray.GetSubArray(0, num);
			shadowCullingInfos.perLightInfos = perLightInfos;
		}
	}

	private static BatchCullingProjectionType GetCullingProjectionType(LightType type)
	{
		return type switch
		{
			LightType.Point => BatchCullingProjectionType.Perspective, 
			LightType.Spot => BatchCullingProjectionType.Perspective, 
			LightType.Directional => BatchCullingProjectionType.Orthographic, 
			_ => BatchCullingProjectionType.Unknown, 
		};
	}
}
