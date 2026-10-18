using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal static class RendererLighting
{
	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Normals");

	private static readonly ShaderTagId k_NormalsRenderingPassName = new ShaderTagId("NormalsRendering");

	public static readonly Color k_NormalClearColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private static readonly string k_UsePointLightCookiesKeyword = "USE_POINT_LIGHT_COOKIES";

	private static readonly string k_LightQualityFastKeyword = "LIGHT_QUALITY_FAST";

	private static readonly string k_UseNormalMap = "USE_NORMAL_MAP";

	private static readonly string k_UseShadowMap = "USE_SHADOW_MAP";

	private static readonly string k_UseAdditiveBlendingKeyword = "USE_ADDITIVE_BLENDING";

	private static readonly string k_UseVolumetric = "USE_VOLUMETRIC";

	private static readonly string[] k_UseBlendStyleKeywords = new string[4] { "USE_SHAPE_LIGHT_TYPE_0", "USE_SHAPE_LIGHT_TYPE_1", "USE_SHAPE_LIGHT_TYPE_2", "USE_SHAPE_LIGHT_TYPE_3" };

	private static readonly int[] k_BlendFactorsPropIDs = new int[4]
	{
		Shader.PropertyToID("_ShapeLightBlendFactors0"),
		Shader.PropertyToID("_ShapeLightBlendFactors1"),
		Shader.PropertyToID("_ShapeLightBlendFactors2"),
		Shader.PropertyToID("_ShapeLightBlendFactors3")
	};

	private static readonly int[] k_MaskFilterPropIDs = new int[4]
	{
		Shader.PropertyToID("_ShapeLightMaskFilter0"),
		Shader.PropertyToID("_ShapeLightMaskFilter1"),
		Shader.PropertyToID("_ShapeLightMaskFilter2"),
		Shader.PropertyToID("_ShapeLightMaskFilter3")
	};

	private static readonly int[] k_InvertedFilterPropIDs = new int[4]
	{
		Shader.PropertyToID("_ShapeLightInvertedFilter0"),
		Shader.PropertyToID("_ShapeLightInvertedFilter1"),
		Shader.PropertyToID("_ShapeLightInvertedFilter2"),
		Shader.PropertyToID("_ShapeLightInvertedFilter3")
	};

	public static readonly string[] k_ShapeLightTextureIDs = new string[4] { "_ShapeLightTexture0", "_ShapeLightTexture1", "_ShapeLightTexture2", "_ShapeLightTexture3" };

	private static GraphicsFormat s_RenderTextureFormatToUse = GraphicsFormat.R8G8B8A8_UNorm;

	private static bool s_HasSetupRenderTextureFormatToUse;

	private static readonly int k_SrcBlendID = Shader.PropertyToID("_SrcBlend");

	private static readonly int k_DstBlendID = Shader.PropertyToID("_DstBlend");

	private static readonly int k_CookieTexID = Shader.PropertyToID("_CookieTex");

	private static readonly int k_PointLightCookieTexID = Shader.PropertyToID("_PointLightCookieTex");

	private static readonly int k_L2DInvMatrix = Shader.PropertyToID("L2DInvMatrix");

	private static readonly int k_L2DColor = Shader.PropertyToID("L2DColor");

	private static readonly int k_L2DPosition = Shader.PropertyToID("L2DPosition");

	private static readonly int k_L2DFalloffIntensity = Shader.PropertyToID("L2DFalloffIntensity");

	private static readonly int k_L2DFalloffDistance = Shader.PropertyToID("L2DFalloffDistance");

	private static readonly int k_L2DOuterAngle = Shader.PropertyToID("L2DOuterAngle");

	private static readonly int k_L2DInnerAngle = Shader.PropertyToID("L2DInnerAngle");

	private static readonly int k_L2DInnerRadiusMult = Shader.PropertyToID("L2DInnerRadiusMult");

	private static readonly int k_L2DVolumeOpacity = Shader.PropertyToID("L2DVolumeOpacity");

	private static readonly int k_L2DShadowIntensity = Shader.PropertyToID("L2DShadowIntensity");

	private static readonly int k_L2DLightType = Shader.PropertyToID("L2DLightType");

	internal static LightBatch lightBatch = new LightBatch();

	internal static GraphicsFormat GetRenderTextureFormat()
	{
		if (!s_HasSetupRenderTextureFormatToUse)
		{
			if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
			{
				s_RenderTextureFormatToUse = GraphicsFormat.B10G11R11_UFloatPack32;
			}
			else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Blend))
			{
				s_RenderTextureFormatToUse = GraphicsFormat.R16G16B16A16_SFloat;
			}
			s_HasSetupRenderTextureFormatToUse = true;
		}
		return s_RenderTextureFormatToUse;
	}

	public static void CreateNormalMapRenderTexture(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, float renderScale)
	{
		RenderTextureDescriptor descriptor = new RenderTextureDescriptor((int)((float)renderingData.cameraData.cameraTargetDescriptor.width * renderScale), (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * renderScale));
		descriptor.graphicsFormat = GetRenderTextureFormat();
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		descriptor.depthStencilFormat = GraphicsFormat.None;
		descriptor.msaaSamples = renderingData.cameraData.cameraTargetDescriptor.msaaSamples;
		descriptor.dimension = TextureDimension.Tex2D;
		RenderingUtils.ReAllocateHandleIfNeeded(ref pass.rendererData.normalsRenderTarget, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_NormalMap");
		cmd.SetGlobalTexture(pass.rendererData.normalsRenderTarget.name, pass.rendererData.normalsRenderTarget.nameID);
	}

	public static RenderTextureDescriptor GetBlendStyleRenderTextureDesc(this IRenderPass2D pass, RenderingData renderingData)
	{
		float num = Mathf.Clamp(pass.rendererData.lightRenderTextureScale, 0.01f, 1f);
		int width = (int)((float)renderingData.cameraData.cameraTargetDescriptor.width * num);
		int height = (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * num);
		RenderTextureDescriptor result = new RenderTextureDescriptor(width, height);
		result.graphicsFormat = GetRenderTextureFormat();
		result.useMipMap = false;
		result.autoGenerateMips = false;
		result.depthStencilFormat = GraphicsFormat.None;
		result.msaaSamples = 1;
		result.dimension = TextureDimension.Tex2D;
		return result;
	}

	public static void CreateCameraSortingLayerRenderTexture(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, Downsampling downsamplingMethod)
	{
		float num = 1f;
		switch (downsamplingMethod)
		{
		case Downsampling._2xBilinear:
			num = 0.5f;
			break;
		case Downsampling._4xBox:
		case Downsampling._4xBilinear:
			num = 0.25f;
			break;
		}
		int width = (int)((float)renderingData.cameraData.cameraTargetDescriptor.width * num);
		int height = (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * num);
		RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height);
		descriptor.graphicsFormat = renderingData.cameraData.cameraTargetDescriptor.graphicsFormat;
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		descriptor.depthStencilFormat = GraphicsFormat.None;
		descriptor.msaaSamples = 1;
		RenderingUtils.ReAllocateHandleIfNeeded(ref pass.rendererData.cameraSortingLayerRenderTarget, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_CameraSortingLayerTexture");
		cmd.SetGlobalTexture(pass.rendererData.cameraSortingLayerRenderTarget.name, pass.rendererData.cameraSortingLayerRenderTarget.nameID);
	}

	internal static void EnableBlendStyle(IRasterCommandBuffer cmd, int blendStyleIndex, bool enabled)
	{
		string keyword = k_UseBlendStyleKeywords[blendStyleIndex];
		if (enabled)
		{
			cmd.EnableShaderKeyword(keyword);
		}
		else
		{
			cmd.DisableShaderKeyword(keyword);
		}
	}

	internal static void DisableAllKeywords(RasterCommandBuffer cmd)
	{
		string[] array = k_UseBlendStyleKeywords;
		foreach (string keyword in array)
		{
			cmd.DisableShaderKeyword(keyword);
		}
	}

	internal static void GetTransparencySortingMode(Renderer2DData rendererData, Camera camera, ref SortingSettings sortingSettings)
	{
		TransparencySortMode transparencySortMode = rendererData.transparencySortMode;
		if (transparencySortMode == TransparencySortMode.Default)
		{
			transparencySortMode = ((!camera.orthographic) ? TransparencySortMode.Perspective : TransparencySortMode.Orthographic);
		}
		switch (transparencySortMode)
		{
		case TransparencySortMode.Perspective:
			sortingSettings.distanceMetric = DistanceMetric.Perspective;
			break;
		case TransparencySortMode.Orthographic:
			sortingSettings.distanceMetric = DistanceMetric.Orthographic;
			break;
		default:
			sortingSettings.distanceMetric = DistanceMetric.CustomAxis;
			sortingSettings.customAxis = rendererData.transparencySortAxis;
			break;
		}
	}

	private static bool CanRenderLight(IRenderPass2D pass, Light2D light, int blendStyleIndex, int layerToRender, bool isVolume, bool hasShadows, ref Mesh lightMesh, ref Material lightMaterial)
	{
		if (light != null && light.lightType != Light2D.LightType.Global && light.blendStyleIndex == blendStyleIndex && light.IsLitLayer(layerToRender))
		{
			lightMesh = light.lightMesh;
			if (lightMesh == null)
			{
				return false;
			}
			lightMaterial = pass.rendererData.GetLightMaterial(light, isVolume, hasShadows);
			if (lightMaterial == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	internal static bool CanCastShadows(Light2D light, int layerToRender)
	{
		if (light.shadowsEnabled && light.shadowIntensity > 0f)
		{
			return light.IsLitLayer(layerToRender);
		}
		return false;
	}

	private static bool CanCastVolumetricShadows(Light2D light, int endLayerValue)
	{
		int topMostLitLayer = light.GetTopMostLitLayer();
		if (light.volumetricShadowsEnabled && light.shadowVolumeIntensity > 0f)
		{
			return topMostLitLayer == endLayerValue;
		}
		return false;
	}

	internal static void RenderLight(IRenderPass2D pass, CommandBuffer cmd, Light2D light, bool isVolume, int blendStyleIndex, int layerToRender, bool hasShadows, bool batchingSupported, ref int shadowLightCount)
	{
		Mesh lightMesh = null;
		Material lightMaterial = null;
		if (CanRenderLight(pass, light, blendStyleIndex, layerToRender, isVolume, hasShadows, ref lightMesh, ref lightMaterial))
		{
			int lightHash;
			bool flag = lightBatch.CanBatch(light, lightMaterial, light.batchSlotIndex, out lightHash);
			bool flag2 = SetCookieShaderGlobals(cmd, light);
			if ((hasShadows || flag2 || !flag) && batchingSupported)
			{
				lightBatch.Flush(CommandBufferHelpers.GetRasterCommandBuffer(cmd));
			}
			if (hasShadows)
			{
				ShadowRendering.SetGlobalShadowTexture(cmd, light, shadowLightCount++);
			}
			int slot = lightBatch.SlotIndex(light.batchSlotIndex);
			SetPerLightShaderGlobals(CommandBufferHelpers.GetRasterCommandBuffer(cmd), light, slot, isVolume, hasShadows, batchingSupported);
			if (light.lightType == Light2D.LightType.Point)
			{
				SetPerPointLightShaderGlobals(CommandBufferHelpers.GetRasterCommandBuffer(cmd), light, slot, batchingSupported);
			}
			if (batchingSupported)
			{
				lightBatch.AddBatch(light, lightMaterial, light.GetMatrix(), lightMesh, 0, lightHash, light.batchSlotIndex);
			}
			else
			{
				cmd.DrawMesh(lightMesh, light.GetMatrix(), lightMaterial);
			}
		}
	}

	private static void RenderLightSet(IRenderPass2D pass, RenderingData renderingData, int blendStyleIndex, CommandBuffer cmd, ref LayerBatch layer, RenderTargetIdentifier renderTexture, List<Light2D> lights)
	{
		uint maxTextureCount = ShadowRendering.maxTextureCount;
		bool flag = true;
		if (maxTextureCount < 1)
		{
			Debug.LogError("maxShadowTextureCount cannot be less than 1");
			return;
		}
		NativeArray<bool> nativeArray = new NativeArray<bool>(lights.Count, Allocator.Temp);
		int j;
		for (int i = 0; i < lights.Count; i += j)
		{
			long num = (uint)lights.Count - i;
			j = 0;
			int num2 = 0;
			for (; j < num; j++)
			{
				if (num2 >= maxTextureCount)
				{
					break;
				}
				int index = i + j;
				Light2D light2D = lights[index];
				if (CanCastShadows(light2D, layer.startLayerID))
				{
					nativeArray[index] = false;
					if (pass.PrerenderShadows(renderingData, cmd, ref layer, light2D, num2, light2D.shadowIntensity))
					{
						nativeArray[index] = true;
						num2++;
					}
				}
			}
			if (num2 > 0 || flag)
			{
				cmd.SetRenderTarget(renderTexture, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
				flag = false;
			}
			num2 = 0;
			for (int k = 0; k < j; k++)
			{
				int index2 = i + k;
				RenderLight(pass, cmd, lights[index2], isVolume: false, blendStyleIndex, layer.startLayerID, nativeArray[index2], LightBatch.isBatchingSupported, ref num2);
			}
			lightBatch.Flush(CommandBufferHelpers.GetRasterCommandBuffer(cmd));
			for (int num3 = num2 - 1; num3 >= 0; num3--)
			{
				ShadowRendering.ReleaseShadowRenderTexture(cmd, num3);
			}
		}
		nativeArray.Dispose();
	}

	public static void RenderLightVolumes(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, ref LayerBatch layer, RenderTargetIdentifier renderTexture, RenderTargetIdentifier depthTexture, RenderBufferStoreAction intermediateStoreAction, RenderBufferStoreAction finalStoreAction, bool requiresRTInit, List<Light2D> lights)
	{
		uint maxTextureCount = ShadowRendering.maxTextureCount;
		NativeArray<bool> nativeArray = new NativeArray<bool>(lights.Count, Allocator.Temp);
		if (maxTextureCount < 1)
		{
			Debug.LogError("maxShadowLightCount cannot be less than 1");
			return;
		}
		int num = lights.Count;
		if (intermediateStoreAction != finalStoreAction)
		{
			for (int num2 = lights.Count - 1; num2 >= 0; num2--)
			{
				if (lights[num2].renderVolumetricShadows)
				{
					num = num2;
					break;
				}
			}
		}
		int j;
		for (int i = 0; i < lights.Count; i += j)
		{
			long num3 = (uint)lights.Count - i;
			j = 0;
			int num4 = 0;
			for (; j < num3; j++)
			{
				if (num4 >= maxTextureCount)
				{
					break;
				}
				int index = i + j;
				Light2D light2D = lights[index];
				if (CanCastVolumetricShadows(light2D, layer.endLayerValue))
				{
					nativeArray[index] = false;
					if (pass.PrerenderShadows(renderingData, cmd, ref layer, light2D, num4, light2D.shadowVolumeIntensity))
					{
						nativeArray[index] = true;
						num4++;
					}
				}
			}
			if (num4 > 0 || requiresRTInit)
			{
				RenderBufferStoreAction renderBufferStoreAction = ((i + j >= num) ? finalStoreAction : intermediateStoreAction);
				cmd.SetRenderTarget(renderTexture, RenderBufferLoadAction.Load, renderBufferStoreAction, depthTexture, RenderBufferLoadAction.Load, renderBufferStoreAction);
				requiresRTInit = false;
			}
			num4 = 0;
			for (int k = 0; k < j; k++)
			{
				int index2 = i + k;
				Light2D light2D2 = lights[index2];
				if (!(light2D2.volumeIntensity <= 0f) && light2D2.volumetricEnabled && layer.endLayerValue == light2D2.GetTopMostLitLayer())
				{
					RenderLight(pass, cmd, light2D2, isVolume: true, light2D2.blendStyleIndex, layer.startLayerID, nativeArray[index2], LightBatch.isBatchingSupported, ref num4);
				}
			}
			lightBatch.Flush(CommandBufferHelpers.GetRasterCommandBuffer(cmd));
			for (int num5 = num4 - 1; num5 >= 0; num5--)
			{
				ShadowRendering.ReleaseShadowRenderTexture(cmd, num5);
			}
		}
		nativeArray.Dispose();
	}

	internal static void SetLightShaderGlobals(Renderer2DData rendererData, RasterCommandBuffer cmd)
	{
		for (int i = 0; i < rendererData.lightBlendStyles.Length; i++)
		{
			Light2DBlendStyle light2DBlendStyle = rendererData.lightBlendStyles[i];
			if (i < k_BlendFactorsPropIDs.Length)
			{
				cmd.SetGlobalVector(k_BlendFactorsPropIDs[i], light2DBlendStyle.blendFactors);
				cmd.SetGlobalVector(k_MaskFilterPropIDs[i], light2DBlendStyle.maskTextureChannelFilter.mask);
				cmd.SetGlobalVector(k_InvertedFilterPropIDs[i], light2DBlendStyle.maskTextureChannelFilter.inverted);
				continue;
			}
			break;
		}
	}

	internal static void SetLightShaderGlobals(RasterCommandBuffer cmd, Light2DBlendStyle[] lightBlendStyles, int[] blendStyleIndices)
	{
		foreach (int num in blendStyleIndices)
		{
			if (num < k_BlendFactorsPropIDs.Length)
			{
				Light2DBlendStyle light2DBlendStyle = lightBlendStyles[num];
				cmd.SetGlobalVector(k_BlendFactorsPropIDs[num], light2DBlendStyle.blendFactors);
				cmd.SetGlobalVector(k_MaskFilterPropIDs[num], light2DBlendStyle.maskTextureChannelFilter.mask);
				cmd.SetGlobalVector(k_InvertedFilterPropIDs[num], light2DBlendStyle.maskTextureChannelFilter.inverted);
				continue;
			}
			break;
		}
	}

	private static float GetNormalizedInnerRadius(Light2D light)
	{
		return light.pointLightInnerRadius / light.pointLightOuterRadius;
	}

	private static float GetNormalizedAngle(float angle)
	{
		return angle / 360f;
	}

	private static void GetScaledLightInvMatrix(Light2D light, out Matrix4x4 retMatrix)
	{
		float pointLightOuterRadius = light.pointLightOuterRadius;
		Vector3 one = Vector3.one;
		Vector3 s = new Vector3(one.x * pointLightOuterRadius, one.y * pointLightOuterRadius, one.z * pointLightOuterRadius);
		Transform transform = light.transform;
		Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, s);
		retMatrix = Matrix4x4.Inverse(m);
	}

	internal static void SetPerLightShaderGlobals(IRasterCommandBuffer cmd, Light2D light, int slot, bool isVolumetric, bool hasShadows, bool batchingSupported)
	{
		Color value = light.intensity * light.color.a * light.color;
		value.a = 1f;
		float num = (light.volumetricEnabled ? light.volumeIntensity : 1f);
		if (batchingSupported)
		{
			PerLight2D light2 = lightBatch.GetLight(slot);
			light2.Position = new float4(light.transform.position, light.normalMapDistance);
			light2.FalloffIntensity = light.falloffIntensity;
			light2.FalloffDistance = light.shapeLightFalloffSize;
			light2.Color = new float4(value.r, value.g, value.b, value.a);
			light2.VolumeOpacity = num;
			light2.LightType = (int)light.lightType;
			light2.ShadowIntensity = 1f;
			if (hasShadows)
			{
				light2.ShadowIntensity = (isVolumetric ? (1f - light.shadowVolumeIntensity) : (1f - light.shadowIntensity));
			}
			lightBatch.SetLight(slot, light2);
		}
		else
		{
			cmd.SetGlobalVector(k_L2DPosition, new float4(light.transform.position, light.normalMapDistance));
			cmd.SetGlobalFloat(k_L2DFalloffIntensity, light.falloffIntensity);
			cmd.SetGlobalFloat(k_L2DFalloffDistance, light.shapeLightFalloffSize);
			cmd.SetGlobalColor(k_L2DColor, value);
			cmd.SetGlobalFloat(k_L2DVolumeOpacity, num);
			cmd.SetGlobalInt(k_L2DLightType, (int)light.lightType);
			cmd.SetGlobalFloat(k_L2DShadowIntensity, (!hasShadows) ? 1f : (isVolumetric ? (1f - light.shadowVolumeIntensity) : (1f - light.shadowIntensity)));
		}
		if (hasShadows)
		{
			ShadowRendering.SetGlobalShadowProp(cmd);
		}
	}

	internal static void SetPerPointLightShaderGlobals(IRasterCommandBuffer cmd, Light2D light, int slot, bool batchingSupported)
	{
		GetScaledLightInvMatrix(light, out var retMatrix);
		float normalizedInnerRadius = GetNormalizedInnerRadius(light);
		float normalizedAngle = GetNormalizedAngle(light.pointLightInnerAngle);
		float normalizedAngle2 = GetNormalizedAngle(light.pointLightOuterAngle);
		float num = 1f / (1f - normalizedInnerRadius);
		if (batchingSupported)
		{
			PerLight2D light2 = lightBatch.GetLight(slot);
			light2.InvMatrix = new float4x4(retMatrix.GetColumn(0), retMatrix.GetColumn(1), retMatrix.GetColumn(2), retMatrix.GetColumn(3));
			light2.InnerRadiusMult = num;
			light2.InnerAngle = normalizedAngle;
			light2.OuterAngle = normalizedAngle2;
			lightBatch.SetLight(slot, light2);
		}
		else
		{
			cmd.SetGlobalMatrix(k_L2DInvMatrix, retMatrix);
			cmd.SetGlobalFloat(k_L2DInnerRadiusMult, num);
			cmd.SetGlobalFloat(k_L2DInnerAngle, normalizedAngle);
			cmd.SetGlobalFloat(k_L2DOuterAngle, normalizedAngle2);
		}
	}

	internal static bool SetCookieShaderGlobals(CommandBuffer cmd, Light2D light)
	{
		if (light.useCookieSprite)
		{
			cmd.SetGlobalTexture((light.lightType == Light2D.LightType.Sprite) ? k_CookieTexID : k_PointLightCookieTexID, light.lightCookieSprite.texture);
		}
		return light.useCookieSprite;
	}

	internal static void SetCookieShaderProperties(Light2D light, MaterialPropertyBlock properties)
	{
		if (light.useCookieSprite && light.m_CookieSpriteTextureHandle.IsValid())
		{
			properties.SetTexture((light.lightType == Light2D.LightType.Sprite) ? k_CookieTexID : k_PointLightCookieTexID, light.m_CookieSpriteTextureHandle);
		}
	}

	public static void ClearDirtyLighting(this IRenderPass2D pass, CommandBuffer cmd, uint blendStylesUsed)
	{
		for (int i = 0; i < pass.rendererData.lightBlendStyles.Length; i++)
		{
			if ((blendStylesUsed & (uint)(1 << i)) != 0 && pass.rendererData.lightBlendStyles[i].isDirty)
			{
				CoreUtils.SetRenderTarget(cmd, pass.rendererData.lightBlendStyles[i].renderTargetHandle, ClearFlag.Color, Color.black);
				pass.rendererData.lightBlendStyles[i].isDirty = false;
			}
		}
	}

	internal static void RenderNormals(this IRenderPass2D pass, ScriptableRenderContext context, RenderingData renderingData, DrawingSettings drawSettings, FilteringSettings filterSettings, RTHandle depthTarget, bool bFirstClear)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			float num = 0f;
			CreateNormalMapRenderTexture(renderScale: (depthTarget == null) ? Mathf.Clamp(pass.rendererData.lightRenderTextureScale, 0.01f, 1f) : 1f, pass: pass, renderingData: renderingData, cmd: commandBuffer);
			RenderBufferStoreAction renderBufferStoreAction = ((renderingData.cameraData.cameraTargetDescriptor.msaaSamples > 1) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.Store);
			ClearFlag clearFlag = ((!(pass.rendererData.useDepthStencilBuffer && bFirstClear)) ? ClearFlag.Color : ClearFlag.All);
			if (depthTarget != null)
			{
				CoreUtils.SetRenderTarget(commandBuffer, pass.rendererData.normalsRenderTarget, RenderBufferLoadAction.DontCare, renderBufferStoreAction, depthTarget, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, clearFlag, k_NormalClearColor);
			}
			else
			{
				CoreUtils.SetRenderTarget(commandBuffer, pass.rendererData.normalsRenderTarget, RenderBufferLoadAction.DontCare, renderBufferStoreAction, clearFlag, k_NormalClearColor);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			drawSettings.SetShaderPassName(0, k_NormalsRenderingPassName);
			RendererListParams param = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
			RendererList rendererList = context.CreateRendererList(ref param);
			commandBuffer.DrawRendererList(rendererList);
		}
	}

	public static void RenderLights(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmd, ref LayerBatch layerBatch, ref RenderTextureDescriptor rtDesc)
	{
		List<Light2D> visibleLights = pass.rendererData.lightCullResult.visibleLights;
		for (int i = 0; i < visibleLights.Count; i++)
		{
			visibleLights[i].CacheValues();
		}
		ShadowCasterGroup2DManager.CacheValues();
		Light2DBlendStyle[] lightBlendStyles = pass.rendererData.lightBlendStyles;
		for (int j = 0; j < lightBlendStyles.Length; j++)
		{
			if ((layerBatch.lightStats.blendStylesUsed & (uint)(1 << j)) != 0)
			{
				string name = lightBlendStyles[j].name;
				cmd.BeginSample(name);
				if (!Light2DManager.GetGlobalColor(layerBatch.startLayerID, j, out var color))
				{
					color = Color.black;
				}
				bool num = (layerBatch.lightStats.blendStylesWithLights & (uint)(1 << j)) != 0;
				RenderTextureDescriptor desc = rtDesc;
				if (!num)
				{
					int width = (desc.height = 4);
					desc.width = width;
				}
				RenderTargetIdentifier rTId = layerBatch.GetRTId(cmd, desc, j);
				cmd.SetRenderTarget(rTId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
				cmd.ClearRenderTarget(clearDepth: false, clearColor: true, color);
				if (num)
				{
					RenderLightSet(pass, renderingData, j, cmd, ref layerBatch, rTId, pass.rendererData.lightCullResult.visibleLights);
				}
				cmd.EndSample(name);
			}
		}
	}

	private static void SetBlendModes(Material material, BlendMode src, BlendMode dst)
	{
		material.SetFloat(k_SrcBlendID, (float)src);
		material.SetFloat(k_DstBlendID, (float)dst);
	}

	private static uint GetLightMaterialIndex(Light2D light, bool isVolume, bool useShadows)
	{
		bool isPointLight = light.isPointLight;
		int num = 0;
		uint num2 = (isVolume ? ((uint)(1 << num)) : 0u);
		num++;
		uint num3 = ((isVolume && !isPointLight) ? ((uint)(1 << num)) : 0u);
		num++;
		uint num4 = ((light.overlapOperation != Light2D.OverlapOperation.AlphaBlend) ? ((uint)(1 << num)) : 0u);
		num++;
		uint num5 = ((isPointLight && light.lightCookieSprite != null && light.lightCookieSprite.texture != null) ? ((uint)(1 << num)) : 0u);
		num++;
		int num6 = ((light.normalMapQuality == Light2D.NormalMapQuality.Fast) ? (1 << num) : 0);
		num++;
		uint num7 = ((light.normalMapQuality != Light2D.NormalMapQuality.Disabled) ? ((uint)(1 << num)) : 0u);
		num++;
		uint num8 = (useShadows ? ((uint)(1 << num)) : 0u);
		return (uint)num6 | num5 | num4 | num3 | num2 | num7 | num8;
	}

	private static Material CreateLightMaterial(Renderer2DData rendererData, Light2D light, bool isVolume, bool useShadows)
	{
		if (!GraphicsSettings.TryGetRenderPipelineSettings<Renderer2DResources>(out var settings))
		{
			return null;
		}
		bool isPointLight = light.isPointLight;
		Material material = CoreUtils.CreateEngineMaterial(settings.lightShader);
		if (!isVolume)
		{
			if (light.overlapOperation == Light2D.OverlapOperation.Additive)
			{
				SetBlendModes(material, BlendMode.One, BlendMode.One);
				material.EnableKeyword(k_UseAdditiveBlendingKeyword);
			}
			else
			{
				SetBlendModes(material, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);
			}
		}
		else
		{
			material.EnableKeyword(k_UseVolumetric);
			if (light.lightType == Light2D.LightType.Point)
			{
				SetBlendModes(material, BlendMode.One, BlendMode.One);
			}
			else
			{
				SetBlendModes(material, BlendMode.SrcAlpha, BlendMode.One);
			}
		}
		if (isPointLight && light.lightCookieSprite != null && light.lightCookieSprite.texture != null)
		{
			material.EnableKeyword(k_UsePointLightCookiesKeyword);
		}
		if (light.normalMapQuality == Light2D.NormalMapQuality.Fast)
		{
			material.EnableKeyword(k_LightQualityFastKeyword);
		}
		if (light.normalMapQuality != Light2D.NormalMapQuality.Disabled)
		{
			material.EnableKeyword(k_UseNormalMap);
		}
		if (useShadows)
		{
			material.EnableKeyword(k_UseShadowMap);
		}
		return material;
	}

	internal static Material GetLightMaterial(this Renderer2DData rendererData, Light2D light, bool isVolume, bool useShadows)
	{
		uint lightMaterialIndex = GetLightMaterialIndex(light, isVolume, useShadows);
		if (!rendererData.lightMaterials.TryGetValue(lightMaterialIndex, out var value))
		{
			value = CreateLightMaterial(rendererData, light, isVolume, useShadows);
			rendererData.lightMaterials[lightMaterialIndex] = value;
		}
		return value;
	}
}
