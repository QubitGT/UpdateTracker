using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class AdditionalLightsShadowCasterPass : ScriptableRenderPass
{
	private static class AdditionalShadowsConstantBuffer
	{
		public static readonly int _AdditionalLightsWorldToShadow = Shader.PropertyToID("_AdditionalLightsWorldToShadow");

		public static readonly int _AdditionalShadowParams = Shader.PropertyToID("_AdditionalShadowParams");

		public static readonly int _AdditionalShadowOffset0 = Shader.PropertyToID("_AdditionalShadowOffset0");

		public static readonly int _AdditionalShadowOffset1 = Shader.PropertyToID("_AdditionalShadowOffset1");

		public static readonly int _AdditionalShadowFadeParams = Shader.PropertyToID("_AdditionalShadowFadeParams");

		public static readonly int _AdditionalShadowmapSize = Shader.PropertyToID("_AdditionalShadowmapSize");

		public static readonly int _AdditionalLightsShadowmapID = Shader.PropertyToID("_AdditionalLightsShadowmapTexture");

		public static readonly int _AdditionalLightsWorldToShadow_SSBO = Shader.PropertyToID("_AdditionalLightsWorldToShadow_SSBO");

		public static readonly int _AdditionalShadowParams_SSBO = Shader.PropertyToID("_AdditionalShadowParams_SSBO");
	}

	private class PassData
	{
		internal int shadowmapID;

		internal bool emptyShadowmap;

		internal bool setKeywordForEmptyShadowmap;

		internal bool useStructuredBuffer;

		internal bool stripShadowsOffVariants;

		internal Matrix4x4 viewMatrix;

		internal Vector2Int allocatedShadowAtlasSize;

		internal TextureHandle shadowmapTexture;

		internal UniversalLightData lightData;

		internal UniversalShadowData shadowData;

		internal AdditionalLightsShadowCasterPass pass;

		internal readonly RendererList[] shadowRendererLists = new RendererList[256];

		internal readonly RendererListHandle[] shadowRendererListsHdl = new RendererListHandle[256];
	}

	[Obsolete("AdditionalLightsShadowCasterPass.m_AdditionalShadowsBufferId was deprecated. Shadow slice matrix is now passed to the GPU using an entry in buffer m_AdditionalLightsWorldToShadow_SSBO", true)]
	public static int m_AdditionalShadowsBufferId;

	[Obsolete("AdditionalLightsShadowCasterPass.m_AdditionalShadowsIndicesId was deprecated. Shadow slice index is now passed to the GPU using last member of an entry in buffer m_AdditionalShadowParams_SSBO", true)]
	public static int m_AdditionalShadowsIndicesId;

	internal RTHandle m_AdditionalLightsShadowmapHandle;

	private int renderTargetWidth;

	private int renderTargetHeight;

	private bool m_CreateEmptyShadowmap;

	private bool m_SetKeywordForEmptyShadowmap;

	private bool m_EmptyShadowmapNeedsClear;

	private bool m_IssuedMessageAboutShadowSlicesTooMany;

	private bool m_IssuedMessageAboutShadowMapsRescale;

	private bool m_IssuedMessageAboutShadowMapsTooBig;

	private bool m_IssuedMessageAboutRemovedShadowSlices;

	private static bool m_IssuedMessageAboutPointLightHardShadowResolutionTooSmall;

	private static bool m_IssuedMessageAboutPointLightSoftShadowResolutionTooSmall;

	private readonly bool m_UseStructuredBuffer;

	private float m_MaxShadowDistanceSq;

	private float m_CascadeBorder;

	private PassData m_PassData;

	private RTHandle m_EmptyAdditionalLightShadowmapTexture;

	private bool[] m_VisibleLightIndexToIsCastingShadows;

	private short[] m_VisibleLightIndexToAdditionalLightIndex;

	private short[] m_AdditionalLightIndexToVisibleLightIndex;

	private Vector4[] m_AdditionalLightIndexToShadowParams;

	private Matrix4x4[] m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix;

	private ShadowSliceData[] m_AdditionalLightsShadowSlices;

	private readonly List<byte> m_GlobalShadowSliceIndexToPerLightShadowSliceIndex = new List<byte>();

	private readonly List<short> m_ShadowSliceToAdditionalLightIndex = new List<short>();

	private readonly Dictionary<int, ulong> m_ShadowRequestsHashes = new Dictionary<int, ulong>();

	private readonly ProfilingSampler m_ProfilingSetupSampler = new ProfilingSampler("Setup Additional Shadows");

	private RenderTextureDescriptor m_AdditionalLightShadowDescriptor;

	private const int k_ShadowmapBufferBits = 16;

	private const int k_EmptyShadowMapDimensions = 1;

	private const float k_LightTypeIdentifierInShadowParams_Spot = 0f;

	private const float k_LightTypeIdentifierInShadowParams_Point = 1f;

	private const string k_AdditionalLightShadowMapTextureName = "_AdditionalLightsShadowmapTexture";

	private const string k_EmptyAdditionalLightShadowMapTextureName = "_EmptyAdditionalLightShadowmapTexture";

	private static readonly Vector4 c_DefaultShadowParams = new Vector4(0f, 0f, 0f, -1f);

	private static Vector4 s_EmptyAdditionalShadowFadeParams;

	private static Vector4[] s_EmptyAdditionalLightIndexToShadowParams;

	private static bool isAdditionalShadowParamsDirty;

	public AdditionalLightsShadowCasterPass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("Draw Additional Lights Shadowmap");
		base.renderPassEvent = evt;
		m_PassData = new PassData();
		m_UseStructuredBuffer = RenderingUtils.useStructuredBuffer;
		int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
		int num = maxVisibleAdditionalLights + 1;
		int num2 = (m_UseStructuredBuffer ? num : Math.Min(num, maxVisibleAdditionalLights));
		m_AdditionalLightIndexToVisibleLightIndex = new short[num2];
		m_VisibleLightIndexToAdditionalLightIndex = new short[num];
		m_VisibleLightIndexToIsCastingShadows = new bool[num];
		m_AdditionalLightIndexToShadowParams = new Vector4[num2];
		s_EmptyAdditionalLightIndexToShadowParams = new Vector4[num2];
		for (int i = 0; i < s_EmptyAdditionalLightIndexToShadowParams.Length; i++)
		{
			s_EmptyAdditionalLightIndexToShadowParams[i] = c_DefaultShadowParams;
		}
		if (!m_UseStructuredBuffer)
		{
			m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix = new Matrix4x4[maxVisibleAdditionalLights];
		}
		m_EmptyShadowmapNeedsClear = true;
	}

	public void Dispose()
	{
		m_AdditionalLightsShadowmapHandle?.Release();
		m_EmptyAdditionalLightShadowmapTexture?.Release();
	}

	internal static float CalcGuardAngle(float frustumAngleInDegrees, float guardBandSizeInTexels, float sliceResolutionInTexels)
	{
		float num = frustumAngleInDegrees * (MathF.PI / 180f) / 2f;
		float num2 = Mathf.Tan(num);
		float num3 = sliceResolutionInTexels / 2f;
		float num4 = guardBandSizeInTexels / 2f;
		float num5 = 1f + num4 / num3;
		float num6 = Mathf.Atan(num2 * num5) - num;
		return 2f * num6 * 57.29578f;
	}

	internal static float GetPointLightShadowFrustumFovBiasInDegrees(int shadowSliceResolution, bool shadowFiltering)
	{
		float num = 4f;
		if (shadowSliceResolution <= 8)
		{
			if (!m_IssuedMessageAboutPointLightHardShadowResolutionTooSmall)
			{
				Debug.LogWarning("Too many additional punctual lights shadows, increase shadow atlas size or remove some shadowed lights");
				m_IssuedMessageAboutPointLightHardShadowResolutionTooSmall = true;
			}
		}
		else if (shadowSliceResolution <= 16)
		{
			num = 43f;
		}
		else if (shadowSliceResolution <= 32)
		{
			num = 18.55f;
		}
		else if (shadowSliceResolution <= 64)
		{
			num = 8.63f;
		}
		else if (shadowSliceResolution <= 128)
		{
			num = 4.13f;
		}
		else if (shadowSliceResolution <= 256)
		{
			num = 2.03f;
		}
		else if (shadowSliceResolution <= 512)
		{
			num = 1f;
		}
		else if (shadowSliceResolution <= 1024)
		{
			num = 0.5f;
		}
		else if (shadowSliceResolution <= 2048)
		{
			num = 0.25f;
		}
		if (shadowFiltering)
		{
			if (shadowSliceResolution <= 16)
			{
				if (!m_IssuedMessageAboutPointLightSoftShadowResolutionTooSmall)
				{
					Debug.LogWarning("Too many additional punctual lights shadows to use Soft Shadows. Increase shadow atlas size, remove some shadowed lights or use Hard Shadows.");
					m_IssuedMessageAboutPointLightSoftShadowResolutionTooSmall = true;
				}
			}
			else if (shadowSliceResolution <= 32)
			{
				num += 9.35f;
			}
			else if (shadowSliceResolution <= 64)
			{
				num += 4.07f;
			}
			else if (shadowSliceResolution <= 128)
			{
				num += 1.77f;
			}
			else if (shadowSliceResolution <= 256)
			{
				num += 0.85f;
			}
			else if (shadowSliceResolution <= 512)
			{
				num += 0.39f;
			}
			else if (shadowSliceResolution <= 1024)
			{
				num += 0.17f;
			}
			else if (shadowSliceResolution <= 2048)
			{
				num += 0.074f;
			}
		}
		return num;
	}

	private ulong ResolutionLog2ForHash(int resolution)
	{
		return resolution switch
		{
			4096 => 12uL, 
			2048 => 11uL, 
			1024 => 10uL, 
			512 => 9uL, 
			_ => 8uL, 
		};
	}

	private ulong ComputeShadowRequestHash(UniversalLightData lightData, UniversalShadowData shadowData)
	{
		ulong num = 0uL;
		ulong num2 = 0uL;
		ulong num3 = 0uL;
		ulong num4 = 0uL;
		ulong num5 = 0uL;
		ulong num6 = 0uL;
		ulong num7 = 0uL;
		ulong num8 = 0uL;
		NativeArray<VisibleLight> visibleLights = lightData.visibleLights;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			ref VisibleLight reference = ref visibleLights.UnsafeElementAt(i);
			Light light = reference.light;
			if (ShadowUtils.IsValidShadowCastingLight(lightData, i, reference.lightType, light.shadows, light.shadowStrength))
			{
				switch (reference.lightType)
				{
				case LightType.Spot:
					num2++;
					break;
				case LightType.Point:
					num++;
					break;
				}
				switch (shadowData.resolution[i])
				{
				case 128:
					num3++;
					break;
				case 256:
					num4++;
					break;
				case 512:
					num5++;
					break;
				case 1024:
					num6++;
					break;
				case 2048:
					num7++;
					break;
				case 4096:
					num8++;
					break;
				}
			}
		}
		return (ResolutionLog2ForHash(shadowData.additionalLightsShadowmapWidth) - 8) | (num << 3) | (num2 << 11) | (num3 << 19) | (num4 << 27) | (num5 << 35) | (num6 << 43) | (num7 << 50) | (num8 << 57);
	}

	private float GetLightTypeIdentifierForShadowParams(LightType lightType)
	{
		return lightType switch
		{
			LightType.Spot => 0f, 
			LightType.Point => 1f, 
			_ => -1f, 
		};
	}

	private bool UsesBakedShadows(Light light)
	{
		return light.bakingOutput.lightmapBakeType != LightmapBakeType.Realtime;
	}

	public bool Setup(ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		UniversalShadowData shadowData = frameData.Get<UniversalShadowData>();
		return Setup(renderingData2, cameraData, lightData, shadowData);
	}

	public bool Setup(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, UniversalShadowData shadowData)
	{
		using (new ProfilingScope(m_ProfilingSetupSampler))
		{
			bool additionalLightShadowsEnabled = shadowData.additionalLightShadowsEnabled;
			if (!additionalLightShadowsEnabled)
			{
				if (AnyAdditionalLightHasMixedShadows(lightData))
				{
					return SetupForEmptyRendering(cameraData.renderer.stripShadowsOffVariants, additionalLightShadowsEnabled, lightData, shadowData);
				}
				return false;
			}
			if (!shadowData.supportsAdditionalLightShadows)
			{
				return SetupForEmptyRendering(cameraData.renderer.stripShadowsOffVariants, additionalLightShadowsEnabled, lightData, shadowData);
			}
			Clear();
			renderTargetWidth = shadowData.additionalLightsShadowmapWidth;
			renderTargetHeight = shadowData.additionalLightsShadowmapHeight;
			NativeArray<VisibleLight> visibleLights = lightData.visibleLights;
			ref AdditionalLightsShadowAtlasLayout shadowAtlasLayout = ref shadowData.shadowAtlasLayout;
			if (m_VisibleLightIndexToAdditionalLightIndex.Length < visibleLights.Length)
			{
				m_VisibleLightIndexToAdditionalLightIndex = new short[visibleLights.Length];
				m_VisibleLightIndexToIsCastingShadows = new bool[visibleLights.Length];
			}
			int num = (m_UseStructuredBuffer ? visibleLights.Length : Math.Min(visibleLights.Length, UniversalRenderPipeline.maxVisibleAdditionalLights));
			if (m_AdditionalLightIndexToVisibleLightIndex.Length < num)
			{
				m_AdditionalLightIndexToVisibleLightIndex = new short[num];
				m_AdditionalLightIndexToShadowParams = new Vector4[num];
			}
			int totalShadowSlicesCount = shadowAtlasLayout.GetTotalShadowSlicesCount();
			int totalShadowResolutionRequestCount = shadowAtlasLayout.GetTotalShadowResolutionRequestCount();
			int shadowSlicesScaleFactor = shadowAtlasLayout.GetShadowSlicesScaleFactor();
			bool flag = shadowAtlasLayout.HasTooManyShadowMaps();
			int atlasSize = shadowAtlasLayout.GetAtlasSize();
			if (totalShadowSlicesCount < totalShadowResolutionRequestCount && !m_IssuedMessageAboutRemovedShadowSlices)
			{
				Debug.LogWarning($"Too many additional punctual lights shadows to look good, URP removed {totalShadowResolutionRequestCount - totalShadowSlicesCount} shadow maps to make the others fit in the shadow atlas. To avoid this, increase shadow atlas size, remove some shadowed lights, replace soft shadows by hard shadows ; or replace point lights by spot lights");
				m_IssuedMessageAboutRemovedShadowSlices = true;
			}
			if (!m_IssuedMessageAboutShadowMapsTooBig && flag)
			{
				Debug.LogWarning($"Too many additional punctual lights shadows. URP tried reducing shadow resolutions by {shadowSlicesScaleFactor} but it was still too much. Increase shadow atlas size, decrease big shadow resolutions, or reduce the number of shadow maps active in the same frame (currently was {totalShadowSlicesCount}).");
				m_IssuedMessageAboutShadowMapsTooBig = true;
			}
			if (!m_IssuedMessageAboutShadowMapsRescale && shadowSlicesScaleFactor > 1)
			{
				Debug.Log($"Reduced additional punctual light shadows resolution by {shadowSlicesScaleFactor} to make {totalShadowSlicesCount} shadow maps fit in the {atlasSize}x{atlasSize} shadow atlas. To avoid this, increase shadow atlas size, decrease big shadow resolutions, or reduce the number of shadow maps active in the same frame");
				m_IssuedMessageAboutShadowMapsRescale = true;
			}
			if (m_AdditionalLightsShadowSlices == null || m_AdditionalLightsShadowSlices.Length < totalShadowSlicesCount)
			{
				m_AdditionalLightsShadowSlices = new ShadowSliceData[totalShadowSlicesCount];
			}
			if (m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix == null || (m_UseStructuredBuffer && m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix.Length < totalShadowSlicesCount))
			{
				m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix = new Matrix4x4[totalShadowSlicesCount];
			}
			for (int i = 0; i < num; i++)
			{
				m_AdditionalLightIndexToShadowParams[i] = c_DefaultShadowParams;
			}
			for (int j = 0; j < m_VisibleLightIndexToAdditionalLightIndex.Length; j++)
			{
				m_VisibleLightIndexToAdditionalLightIndex[j] = -1;
				m_VisibleLightIndexToIsCastingShadows[j] = false;
			}
			short num2 = 0;
			short num3 = 0;
			bool supportsSoftShadows = shadowData.supportsSoftShadows;
			UniversalRenderer obj = (UniversalRenderer)cameraData.renderer;
			bool flag2 = obj.renderingModeActual == RenderingMode.Deferred;
			bool shadowTransparentReceive = obj.shadowTransparentReceive;
			bool flag3 = !flag2 || shadowTransparentReceive;
			for (int k = 0; k < visibleLights.Length; k++)
			{
				if (k == lightData.mainLightIndex)
				{
					continue;
				}
				short num4 = ((!flag3) ? num3 : num2++);
				m_VisibleLightIndexToAdditionalLightIndex[k] = num4;
				if (num4 >= m_AdditionalLightIndexToVisibleLightIndex.Length)
				{
					continue;
				}
				m_AdditionalLightIndexToVisibleLightIndex[num4] = (short)k;
				if (m_ShadowSliceToAdditionalLightIndex.Count >= totalShadowSlicesCount)
				{
					continue;
				}
				ref VisibleLight reference = ref visibleLights.UnsafeElementAt(k);
				Light light = reference.light;
				if (light == null)
				{
					break;
				}
				LightType lightType = reference.lightType;
				bool flag4 = UsesBakedShadows(light);
				float lightTypeIdentifierForShadowParams = GetLightTypeIdentifierForShadowParams(lightType);
				int punctualLightShadowSlicesCount = ShadowUtils.GetPunctualLightShadowSlicesCount(in lightType);
				bool flag5 = ShadowUtils.IsValidShadowCastingLight(lightData, k, reference.lightType, light.shadows, light.shadowStrength);
				if (flag5 && m_ShadowSliceToAdditionalLightIndex.Count + punctualLightShadowSlicesCount > totalShadowSlicesCount)
				{
					if (!m_IssuedMessageAboutShadowSlicesTooMany)
					{
						Debug.Log("There are too many shadowed additional punctual lights active at the same time, URP will not render all the shadows. To ensure all shadows are rendered, reduce the number of shadowed additional lights in the scene ; make sure they are not active at the same time ; or replace point lights by spot lights (spot lights use less shadow maps than point lights).");
						m_IssuedMessageAboutShadowSlicesTooMany = true;
					}
					break;
				}
				float y = ShadowUtils.SoftShadowQualityToShaderProperty(light, supportsSoftShadows && light.shadows == LightShadows.Soft);
				int count = m_ShadowSliceToAdditionalLightIndex.Count;
				bool flag6 = false;
				for (byte b = 0; b < punctualLightShadowSlicesCount; b++)
				{
					int count2 = m_ShadowSliceToAdditionalLightIndex.Count;
					Bounds outBounds;
					bool shadowCasterBounds = renderingData.cullResults.GetShadowCasterBounds(k, out outBounds);
					if (!shadowData.supportsAdditionalLightShadows || !flag5 || !shadowCasterBounds)
					{
						if (flag4 && lightTypeIdentifierForShadowParams > -1f)
						{
							m_AdditionalLightIndexToShadowParams[num4] = new Vector4(light.shadowStrength, y, lightTypeIdentifierForShadowParams, num4);
							m_VisibleLightIndexToIsCastingShadows[k] = flag4;
						}
					}
					else if (shadowAtlasLayout.HasSpaceForLight(k))
					{
						switch (lightType)
						{
						case LightType.Spot:
						{
							ref URPLightShadowCullingInfos reference4 = ref shadowData.visibleLightsShadowCullingInfos.UnsafeElementAt(k);
							ref ShadowSliceData reference5 = ref reference4.slices.UnsafeElementAt(0);
							m_AdditionalLightsShadowSlices[count2].viewMatrix = reference5.viewMatrix;
							m_AdditionalLightsShadowSlices[count2].projectionMatrix = reference5.projectionMatrix;
							m_AdditionalLightsShadowSlices[count2].splitData = reference5.splitData;
							if (reference4.IsSliceValid(0))
							{
								m_ShadowSliceToAdditionalLightIndex.Add(num4);
								m_GlobalShadowSliceIndexToPerLightShadowSliceIndex.Add(b);
								m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[count2] = reference5.shadowTransform;
								m_AdditionalLightIndexToShadowParams[num4] = new Vector4(light.shadowStrength, y, lightTypeIdentifierForShadowParams, count);
								flag6 = true;
							}
							break;
						}
						case LightType.Point:
						{
							ref URPLightShadowCullingInfos reference2 = ref shadowData.visibleLightsShadowCullingInfos.UnsafeElementAt(k);
							ref ShadowSliceData reference3 = ref reference2.slices.UnsafeElementAt(b);
							m_AdditionalLightsShadowSlices[count2].viewMatrix = reference3.viewMatrix;
							m_AdditionalLightsShadowSlices[count2].projectionMatrix = reference3.projectionMatrix;
							m_AdditionalLightsShadowSlices[count2].splitData = reference3.splitData;
							if (reference2.IsSliceValid(b))
							{
								m_ShadowSliceToAdditionalLightIndex.Add(num4);
								m_GlobalShadowSliceIndexToPerLightShadowSliceIndex.Add(b);
								m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[count2] = reference3.shadowTransform;
								m_AdditionalLightIndexToShadowParams[num4] = new Vector4(light.shadowStrength, y, lightTypeIdentifierForShadowParams, count);
								flag6 = true;
							}
							break;
						}
						}
					}
				}
				if (flag6)
				{
					m_VisibleLightIndexToIsCastingShadows[k] = true;
					m_VisibleLightIndexToAdditionalLightIndex[k] = num4;
					m_AdditionalLightIndexToVisibleLightIndex[num4] = (short)k;
					num3++;
				}
				else
				{
					m_VisibleLightIndexToIsCastingShadows[k] = flag4;
					m_AdditionalLightIndexToShadowParams[num4] = new Vector4(light.shadowStrength, y, lightTypeIdentifierForShadowParams, c_DefaultShadowParams.w);
				}
			}
			if (num3 == 0)
			{
				return SetupForEmptyRendering(cameraData.renderer.stripShadowsOffVariants, additionalLightShadowsEnabled, lightData, shadowData);
			}
			int count3 = m_ShadowSliceToAdditionalLightIndex.Count;
			int num5 = 0;
			int num6 = 0;
			for (int l = 0; l < totalShadowSlicesCount; l++)
			{
				AdditionalLightsShadowAtlasLayout.ShadowResolutionRequest sortedShadowResolutionRequest = shadowAtlasLayout.GetSortedShadowResolutionRequest(l);
				num5 = Mathf.Max(num5, sortedShadowResolutionRequest.offsetX + sortedShadowResolutionRequest.allocatedResolution);
				num6 = Mathf.Max(num6, sortedShadowResolutionRequest.offsetY + sortedShadowResolutionRequest.allocatedResolution);
			}
			renderTargetWidth = Mathf.NextPowerOfTwo(num5);
			renderTargetHeight = Mathf.NextPowerOfTwo(num6);
			float num7 = 1f / (float)renderTargetWidth;
			float num8 = 1f / (float)renderTargetHeight;
			for (int m = 0; m < count3; m++)
			{
				int num9 = m_ShadowSliceToAdditionalLightIndex[m];
				if (!Mathf.Approximately(m_AdditionalLightIndexToShadowParams[num9].x, 0f) && !Mathf.Approximately(m_AdditionalLightIndexToShadowParams[num9].w, -1f))
				{
					int originalVisibleLightIndex = m_AdditionalLightIndexToVisibleLightIndex[num9];
					int sliceIndex = m_GlobalShadowSliceIndexToPerLightShadowSliceIndex[m];
					AdditionalLightsShadowAtlasLayout.ShadowResolutionRequest sliceShadowResolutionRequest = shadowAtlasLayout.GetSliceShadowResolutionRequest(originalVisibleLightIndex, sliceIndex);
					int allocatedResolution = sliceShadowResolutionRequest.allocatedResolution;
					Matrix4x4 identity = Matrix4x4.identity;
					identity.m00 = (float)allocatedResolution * num7;
					identity.m11 = (float)allocatedResolution * num8;
					m_AdditionalLightsShadowSlices[m].offsetX = sliceShadowResolutionRequest.offsetX;
					m_AdditionalLightsShadowSlices[m].offsetY = sliceShadowResolutionRequest.offsetY;
					m_AdditionalLightsShadowSlices[m].resolution = allocatedResolution;
					identity.m03 = (float)m_AdditionalLightsShadowSlices[m].offsetX * num7;
					identity.m13 = (float)m_AdditionalLightsShadowSlices[m].offsetY * num8;
					m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[m] = identity * m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[m];
				}
			}
			UpdateTextureDescriptorIfNeeded();
			m_MaxShadowDistanceSq = cameraData.maxShadowDistance * cameraData.maxShadowDistance;
			m_CascadeBorder = shadowData.mainLightShadowCascadeBorder;
			m_CreateEmptyShadowmap = false;
			base.useNativeRenderPass = true;
			return true;
		}
	}

	private void UpdateTextureDescriptorIfNeeded()
	{
		if (m_AdditionalLightShadowDescriptor.width != renderTargetWidth || m_AdditionalLightShadowDescriptor.height != renderTargetHeight || m_AdditionalLightShadowDescriptor.depthBufferBits != 16 || m_AdditionalLightShadowDescriptor.colorFormat != RenderTextureFormat.Shadowmap)
		{
			m_AdditionalLightShadowDescriptor = new RenderTextureDescriptor(renderTargetWidth, renderTargetHeight, RenderTextureFormat.Shadowmap, 16);
		}
	}

	private bool AnyAdditionalLightHasMixedShadows(UniversalLightData lightData)
	{
		for (int i = 0; i < lightData.visibleLights.Length; i++)
		{
			if (i != lightData.mainLightIndex)
			{
				Light light = lightData.visibleLights[i].light;
				if (light.shadows != 0 && light.bakingOutput.isBaked && light.bakingOutput.mixedLightingMode != 0 && light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool SetupForEmptyRendering(bool stripShadowsOffVariants, bool shadowsEnabled, UniversalLightData lightData, UniversalShadowData shadowData)
	{
		if (!stripShadowsOffVariants)
		{
			return false;
		}
		shadowData.isKeywordAdditionalLightShadowsEnabled = true;
		m_CreateEmptyShadowmap = true;
		base.useNativeRenderPass = false;
		m_SetKeywordForEmptyShadowmap = shadowsEnabled;
		ShadowUtils.GetScaleAndBiasForLinearDistanceFade(m_MaxShadowDistanceSq, m_CascadeBorder, out var scale, out var bias);
		s_EmptyAdditionalShadowFadeParams = new Vector4(scale, bias, 0f, 0f);
		NativeArray<VisibleLight> visibleLights = lightData.visibleLights;
		if (s_EmptyAdditionalLightIndexToShadowParams.Length < visibleLights.Length)
		{
			m_VisibleLightIndexToAdditionalLightIndex = new short[visibleLights.Length];
			m_VisibleLightIndexToIsCastingShadows = new bool[visibleLights.Length];
			s_EmptyAdditionalLightIndexToShadowParams = new Vector4[visibleLights.Length];
			isAdditionalShadowParamsDirty = true;
		}
		if (isAdditionalShadowParamsDirty)
		{
			isAdditionalShadowParamsDirty = false;
			Debug.LogWarning($"The number of visible additional lights {visibleLights.Length} exceeds the maximum supported lights {UniversalRenderPipeline.maxVisibleAdditionalLights}." + " Please refer URP documentation to change maximum number of visible lights or reduce the number of lights to maximum allowed additional lights.");
		}
		short num = 0;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			if (i == lightData.mainLightIndex)
			{
				continue;
			}
			Light light = visibleLights.UnsafeElementAt(i).light;
			if (light == null)
			{
				continue;
			}
			float lightTypeIdentifierForShadowParams = GetLightTypeIdentifierForShadowParams(light.type);
			if (!(lightTypeIdentifierForShadowParams < 0f))
			{
				short num2 = num++;
				LightShadows shadows = light.shadows;
				if (shadows != 0)
				{
					bool flag = shadows != LightShadows.Soft;
					bool supportsSoftShadows = shadowData.supportsSoftShadows;
					float y = ShadowUtils.SoftShadowQualityToShaderProperty(light, supportsSoftShadows && flag);
					s_EmptyAdditionalLightIndexToShadowParams[num2] = new Vector4(light.shadowStrength, y, lightTypeIdentifierForShadowParams, num2);
				}
				else
				{
					s_EmptyAdditionalLightIndexToShadowParams[num2] = c_DefaultShadowParams;
				}
				m_VisibleLightIndexToAdditionalLightIndex[i] = num2;
				m_VisibleLightIndexToIsCastingShadows[i] = UsesBakedShadows(light);
			}
		}
		return true;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (m_CreateEmptyShadowmap)
		{
			if (ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyAdditionalLightShadowmapTexture, 1, 1, 16, 1, 0f, "_EmptyAdditionalLightShadowmapTexture"))
			{
				m_EmptyShadowmapNeedsClear = true;
			}
			if (!m_EmptyShadowmapNeedsClear)
			{
				return;
			}
			ConfigureTarget(m_EmptyAdditionalLightShadowmapTexture);
			m_EmptyShadowmapNeedsClear = false;
		}
		else
		{
			ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_AdditionalLightsShadowmapHandle, renderTargetWidth, renderTargetHeight, 16, 1, 0f, "_AdditionalLightsShadowmapTexture");
			ConfigureTarget(m_AdditionalLightsShadowmapHandle);
		}
		ConfigureClear(ClearFlag.All, Color.black);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
		RasterCommandBuffer rasterCommandBuffer = CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer);
		if (m_CreateEmptyShadowmap)
		{
			if (m_SetKeywordForEmptyShadowmap)
			{
				rasterCommandBuffer.EnableKeyword(in ShaderGlobalKeywords.AdditionalLightShadows);
			}
			SetShadowParamsForEmptyShadowmap(rasterCommandBuffer);
			universalRenderingData.commandBuffer.SetGlobalTexture(AdditionalShadowsConstantBuffer._AdditionalLightsShadowmapID, m_EmptyAdditionalLightShadowmapTexture);
			return;
		}
		UniversalShadowData universalShadowData = frameData.Get<UniversalShadowData>();
		if (universalShadowData.supportsAdditionalLightShadows)
		{
			UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
			UniversalLightData lightData = frameData.Get<UniversalLightData>();
			InitPassData(ref m_PassData, cameraData, lightData, universalShadowData);
			m_PassData.allocatedShadowAtlasSize = m_AdditionalLightsShadowmapHandle.referenceSize;
			InitRendererLists(ref universalRenderingData.cullResults, ref m_PassData, context, null, useRenderGraph: false);
			RenderAdditionalShadowmapAtlas(rasterCommandBuffer, ref m_PassData, useRenderGraph: false);
			universalRenderingData.commandBuffer.SetGlobalTexture(AdditionalShadowsConstantBuffer._AdditionalLightsShadowmapID, m_AdditionalLightsShadowmapHandle.nameID);
		}
	}

	public int GetShadowLightIndexFromLightIndex(int visibleLightIndex)
	{
		if (visibleLightIndex < 0 || visibleLightIndex >= m_VisibleLightIndexToAdditionalLightIndex.Length || !m_VisibleLightIndexToIsCastingShadows[visibleLightIndex])
		{
			return -1;
		}
		return m_VisibleLightIndexToAdditionalLightIndex[visibleLightIndex];
	}

	private void Clear()
	{
		m_ShadowSliceToAdditionalLightIndex.Clear();
		m_GlobalShadowSliceIndexToPerLightShadowSliceIndex.Clear();
	}

	internal static void SetShadowParamsForEmptyShadowmap(RasterCommandBuffer rasterCommandBuffer)
	{
		rasterCommandBuffer.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowFadeParams, s_EmptyAdditionalShadowFadeParams);
		if (RenderingUtils.useStructuredBuffer)
		{
			ComputeBuffer additionalLightShadowParamsStructuredBuffer = ShaderData.instance.GetAdditionalLightShadowParamsStructuredBuffer(s_EmptyAdditionalLightIndexToShadowParams.Length);
			additionalLightShadowParamsStructuredBuffer.SetData(s_EmptyAdditionalLightIndexToShadowParams);
			rasterCommandBuffer.SetGlobalBuffer(AdditionalShadowsConstantBuffer._AdditionalShadowParams_SSBO, additionalLightShadowParamsStructuredBuffer);
		}
		else if (s_EmptyAdditionalLightIndexToShadowParams.Length <= UniversalRenderPipeline.maxVisibleAdditionalLights)
		{
			rasterCommandBuffer.SetGlobalVectorArray(AdditionalShadowsConstantBuffer._AdditionalShadowParams, s_EmptyAdditionalLightIndexToShadowParams);
		}
	}

	private void RenderAdditionalShadowmapAtlas(RasterCommandBuffer cmd, ref PassData data, bool useRenderGraph)
	{
		NativeArray<VisibleLight> visibleLights = data.lightData.visibleLights;
		bool flag = false;
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.AdditionalLightsShadow)))
		{
			if (!useRenderGraph)
			{
				ShadowUtils.SetWorldToCameraAndCameraToWorldMatrices(cmd, data.viewMatrix);
			}
			bool flag2 = false;
			int count = m_ShadowSliceToAdditionalLightIndex.Count;
			if (count > 0)
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.CastingPunctualLightShadow, value: true);
			}
			Vector4 b = new Vector4(-10f, -10f, -10f, -10f);
			for (int i = 0; i < count; i++)
			{
				int num = m_ShadowSliceToAdditionalLightIndex[i];
				if (!ShadowUtils.FastApproximately(m_AdditionalLightIndexToShadowParams[num].x, 0f) && !ShadowUtils.FastApproximately(m_AdditionalLightIndexToShadowParams[num].w, -1f))
				{
					int num2 = m_AdditionalLightIndexToVisibleLightIndex[num];
					ref VisibleLight reference = ref visibleLights.UnsafeElementAt(num2);
					ShadowSliceData shadowSliceData = m_AdditionalLightsShadowSlices[i];
					Vector4 shadowBias = ShadowUtils.GetShadowBias(ref reference, num2, data.shadowData, shadowSliceData.projectionMatrix, shadowSliceData.resolution);
					if (i == 0 || !ShadowUtils.FastApproximately(shadowBias, b))
					{
						ShadowUtils.SetShadowBias(cmd, shadowBias);
						b = shadowBias;
					}
					Vector3 lightPosition = reference.localToWorldMatrix.GetColumn(3);
					ShadowUtils.SetLightPosition(cmd, lightPosition);
					RendererList shadowRendererList = (useRenderGraph ? ((RendererList)data.shadowRendererListsHdl[i]) : data.shadowRendererLists[i]);
					ShadowUtils.RenderShadowSlice(cmd, ref shadowSliceData, ref shadowRendererList, shadowSliceData.projectionMatrix, shadowSliceData.viewMatrix);
					flag |= reference.light.shadows == LightShadows.Soft;
					flag2 = true;
				}
			}
			bool flag3 = data.shadowData.supportsMainLightShadows && data.lightData.mainLightIndex != -1 && visibleLights[data.lightData.mainLightIndex].light.shadows == LightShadows.Soft;
			bool flag4 = !data.stripShadowsOffVariants;
			data.shadowData.isKeywordAdditionalLightShadowsEnabled = !flag4 || flag2;
			cmd.SetKeyword(in ShaderGlobalKeywords.AdditionalLightShadows, data.shadowData.isKeywordAdditionalLightShadowsEnabled);
			bool flag5 = data.shadowData.supportsSoftShadows && (flag3 || flag);
			data.shadowData.isKeywordSoftShadowsEnabled = flag5;
			ShadowUtils.SetSoftShadowQualityShaderKeywords(cmd, data.shadowData);
			if (flag2)
			{
				SetupAdditionalLightsShadowReceiverConstants(cmd, data.allocatedShadowAtlasSize, data.useStructuredBuffer, flag5);
			}
		}
	}

	private void SetupAdditionalLightsShadowReceiverConstants(RasterCommandBuffer cmd, Vector2Int allocatedShadowAtlasSize, bool useStructuredBuffer, bool softShadows)
	{
		if (useStructuredBuffer)
		{
			ComputeBuffer additionalLightShadowParamsStructuredBuffer = ShaderData.instance.GetAdditionalLightShadowParamsStructuredBuffer(m_AdditionalLightIndexToShadowParams.Length);
			additionalLightShadowParamsStructuredBuffer.SetData(m_AdditionalLightIndexToShadowParams);
			cmd.SetGlobalBuffer(AdditionalShadowsConstantBuffer._AdditionalShadowParams_SSBO, additionalLightShadowParamsStructuredBuffer);
			ComputeBuffer additionalLightShadowSliceMatricesStructuredBuffer = ShaderData.instance.GetAdditionalLightShadowSliceMatricesStructuredBuffer(m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix.Length);
			additionalLightShadowSliceMatricesStructuredBuffer.SetData(m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix);
			cmd.SetGlobalBuffer(AdditionalShadowsConstantBuffer._AdditionalLightsWorldToShadow_SSBO, additionalLightShadowSliceMatricesStructuredBuffer);
		}
		else
		{
			cmd.SetGlobalVectorArray(AdditionalShadowsConstantBuffer._AdditionalShadowParams, m_AdditionalLightIndexToShadowParams);
			cmd.SetGlobalMatrixArray(AdditionalShadowsConstantBuffer._AdditionalLightsWorldToShadow, m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix);
		}
		ShadowUtils.GetScaleAndBiasForLinearDistanceFade(m_MaxShadowDistanceSq, m_CascadeBorder, out var scale, out var bias);
		cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowFadeParams, new Vector4(scale, bias, 0f, 0f));
		if (softShadows)
		{
			Vector2 vector = Vector2.one / allocatedShadowAtlasSize;
			Vector2 vector2 = vector * 0.5f;
			cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowOffset0, new Vector4(0f - vector2.x, 0f - vector2.y, vector2.x, 0f - vector2.y));
			cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowOffset1, new Vector4(0f - vector2.x, vector2.y, vector2.x, vector2.y));
			cmd.SetGlobalVector(AdditionalShadowsConstantBuffer._AdditionalShadowmapSize, new Vector4(vector.x, vector.y, allocatedShadowAtlasSize.x, allocatedShadowAtlasSize.y));
		}
	}

	private void InitPassData(ref PassData passData, UniversalCameraData cameraData, UniversalLightData lightData, UniversalShadowData shadowData)
	{
		passData.pass = this;
		passData.lightData = lightData;
		passData.shadowData = shadowData;
		passData.viewMatrix = cameraData.GetViewMatrix();
		passData.stripShadowsOffVariants = cameraData.renderer.stripShadowsOffVariants;
		passData.emptyShadowmap = m_CreateEmptyShadowmap;
		passData.setKeywordForEmptyShadowmap = m_SetKeywordForEmptyShadowmap;
		passData.useStructuredBuffer = m_UseStructuredBuffer;
	}

	private void InitRendererLists(ref CullingResults cullResults, ref PassData passData, ScriptableRenderContext context, RenderGraph renderGraph, bool useRenderGraph)
	{
		if (m_CreateEmptyShadowmap)
		{
			return;
		}
		for (int i = 0; i < m_ShadowSliceToAdditionalLightIndex.Count; i++)
		{
			int num = m_ShadowSliceToAdditionalLightIndex[i];
			int lightIndex = m_AdditionalLightIndexToVisibleLightIndex[num];
			ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullResults, lightIndex);
			shadowDrawingSettings.useRenderingLayerMaskTest = UniversalRenderPipeline.asset.useRenderingLayers;
			ShadowDrawingSettings settings = shadowDrawingSettings;
			if (useRenderGraph)
			{
				passData.shadowRendererListsHdl[i] = renderGraph.CreateShadowRendererList(ref settings);
			}
			else
			{
				passData.shadowRendererLists[i] = context.CreateShadowRendererList(ref settings);
			}
		}
	}

	internal TextureHandle Render(RenderGraph graph, ContextContainer frameData)
	{
		UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		UniversalShadowData shadowData = frameData.Get<UniversalShadowData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\AdditionalLightsShadowCasterPass.cs", 1003);
		InitPassData(ref passData, cameraData, lightData, shadowData);
		InitRendererLists(ref universalRenderingData.cullResults, ref passData, default(ScriptableRenderContext), graph, useRenderGraph: true);
		TextureHandle textureHandle;
		if (!m_CreateEmptyShadowmap)
		{
			for (int i = 0; i < m_ShadowSliceToAdditionalLightIndex.Count; i++)
			{
				rasterRenderGraphBuilder.UseRendererList(in passData.shadowRendererListsHdl[i]);
			}
			textureHandle = UniversalRenderer.CreateRenderGraphTexture(graph, m_AdditionalLightShadowDescriptor, "_AdditionalLightsShadowmapTexture", clear: true, (!ShadowUtils.m_ForceShadowPointSampling) ? FilterMode.Bilinear : FilterMode.Point);
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(textureHandle);
		}
		else
		{
			textureHandle = graph.defaultResources.defaultShadowTexture;
		}
		TextureDesc descriptor = textureHandle.GetDescriptor(graph);
		passData.allocatedShadowAtlasSize = new Vector2Int(descriptor.width, descriptor.height);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		if (textureHandle.IsValid())
		{
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in textureHandle, AdditionalShadowsConstantBuffer._AdditionalLightsShadowmapID);
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			if (!data.emptyShadowmap)
			{
				data.pass.RenderAdditionalShadowmapAtlas(cmd, ref data, useRenderGraph: true);
			}
			else
			{
				if (data.setKeywordForEmptyShadowmap)
				{
					cmd.EnableKeyword(in ShaderGlobalKeywords.AdditionalLightShadows);
				}
				SetShadowParamsForEmptyShadowmap(cmd);
			}
		});
		return textureHandle;
	}
}
