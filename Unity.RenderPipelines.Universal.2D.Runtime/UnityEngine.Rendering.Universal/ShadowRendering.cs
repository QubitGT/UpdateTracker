using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal static class ShadowRendering
{
	internal enum ShadowTestType
	{
		Always,
		Unshadow
	}

	private static readonly int k_LightPosID = Shader.PropertyToID("_LightPos");

	private static readonly int k_ShadowRadiusID = Shader.PropertyToID("_ShadowRadius");

	private static readonly int k_ShadowColorMaskID = Shader.PropertyToID("_ShadowColorMask");

	private static readonly int k_ShadowModelMatrixID = Shader.PropertyToID("_ShadowModelMatrix");

	private static readonly int k_ShadowModelInvMatrixID = Shader.PropertyToID("_ShadowModelInvMatrix");

	private static readonly int k_ShadowModelScaleID = Shader.PropertyToID("_ShadowModelScale");

	private static readonly int k_ShadowContractionDistanceID = Shader.PropertyToID("_ShadowContractionDistance");

	private static readonly int k_ShadowAlphaCutoffID = Shader.PropertyToID("_ShadowAlphaCutoff");

	private static readonly int k_SoftShadowAngle = Shader.PropertyToID("_SoftShadowAngle");

	private static readonly int k_ShadowSoftnessFalloffIntensityID = Shader.PropertyToID("_ShadowSoftnessFalloffIntensity");

	private static readonly int k_ShadowShadowColorID = Shader.PropertyToID("_ShadowColor");

	private static readonly int k_ShadowUnshadowColorID = Shader.PropertyToID("_UnshadowColor");

	private static readonly ProfilingSampler m_ProfilingSamplerShadows = new ProfilingSampler("Draw 2D Shadow Texture");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsA = new ProfilingSampler("Draw 2D Shadows (A)");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsR = new ProfilingSampler("Draw 2D Shadows (R)");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsG = new ProfilingSampler("Draw 2D Shadows (G)");

	private static readonly ProfilingSampler m_ProfilingSamplerShadowsB = new ProfilingSampler("Draw 2D Shadows (B)");

	private static readonly float k_MaxShadowSoftnessAngle = 15f;

	private static readonly Color k_ShadowColorLookup = new Color(0f, 0f, 1f, 0f);

	private static readonly Color k_UnshadowColorLookup = new Color(0f, 1f, 0f, 0f);

	private static RTHandle[] m_RenderTargets = null;

	private static int[] m_RenderTargetIds = null;

	private static RenderTargetIdentifier[] m_LightInputTextures = null;

	private static readonly ProfilingSampler[] m_ProfilingSamplerShadowColorsLookup = new ProfilingSampler[4] { m_ProfilingSamplerShadowsA, m_ProfilingSamplerShadowsB, m_ProfilingSamplerShadowsG, m_ProfilingSamplerShadowsR };

	public static uint maxTextureCount { get; private set; }

	public static RenderTargetIdentifier[] lightInputTextures => m_LightInputTextures;

	internal static void InitializeBudget(uint maxTextureCount)
	{
		if (m_RenderTargets == null || m_RenderTargets.Length != maxTextureCount)
		{
			m_RenderTargets = new RTHandle[maxTextureCount];
			m_RenderTargetIds = new int[maxTextureCount];
			ShadowRendering.maxTextureCount = maxTextureCount;
			for (int i = 0; i < maxTextureCount; i++)
			{
				m_RenderTargetIds[i] = Shader.PropertyToID($"ShadowTex_{i}");
				m_RenderTargets[i] = RTHandles.Alloc(m_RenderTargetIds[i], $"ShadowTex_{i}");
			}
		}
		if (m_LightInputTextures == null || m_LightInputTextures.Length != maxTextureCount)
		{
			m_LightInputTextures = new RenderTargetIdentifier[maxTextureCount];
		}
	}

	private static Material CreateMaterial(Shader shader, int offset, int pass)
	{
		Material material = CoreUtils.CreateEngineMaterial(shader);
		material.SetInt(k_ShadowColorMaskID, 1 << offset + 1);
		material.SetPass(pass);
		return material;
	}

	private static Material GetProjectedShadowMaterial(Material material, Func<Renderer2DResources, Shader> shaderFunc, int offset, int pass)
	{
		if (material != null)
		{
			return material;
		}
		if (!GraphicsSettings.TryGetRenderPipelineSettings<Renderer2DResources>(out var settings))
		{
			return null;
		}
		Shader shader = shaderFunc(settings);
		if (material != null && material.shader != shader)
		{
			material = null;
		}
		if (material == null)
		{
			material = CoreUtils.CreateEngineMaterial(shader);
			material.SetInt(k_ShadowColorMaskID, 1 << offset + 1);
			material.SetPass(pass);
		}
		return material;
	}

	internal static Material GetProjectedShadowMaterial(this Renderer2DData rendererData)
	{
		rendererData.projectedShadowMaterial = GetProjectedShadowMaterial(rendererData.projectedShadowMaterial, (Renderer2DResources r) => r.projectedShadowShader, 0, 0);
		return rendererData.projectedShadowMaterial;
	}

	internal static Material GetProjectedUnshadowMaterial(this Renderer2DData rendererData)
	{
		rendererData.projectedUnshadowMaterial = GetProjectedShadowMaterial(rendererData.projectedUnshadowMaterial, (Renderer2DResources r) => r.projectedShadowShader, 1, 1);
		return rendererData.projectedUnshadowMaterial;
	}

	private static Material GetSpriteShadowMaterial(this Renderer2DData rendererData)
	{
		rendererData.spriteSelfShadowMaterial = GetProjectedShadowMaterial(rendererData.spriteSelfShadowMaterial, (Renderer2DResources r) => r.spriteShadowShader, 0, 0);
		return rendererData.spriteSelfShadowMaterial;
	}

	private static Material GetSpriteUnshadowMaterial(this Renderer2DData rendererData)
	{
		rendererData.spriteUnshadowMaterial = GetProjectedShadowMaterial(rendererData.spriteUnshadowMaterial, (Renderer2DResources r) => r.spriteUnshadowShader, 1, 0);
		return rendererData.spriteUnshadowMaterial;
	}

	private static Material GetGeometryShadowMaterial(this Renderer2DData rendererData)
	{
		rendererData.geometrySelfShadowMaterial = GetProjectedShadowMaterial(rendererData.geometrySelfShadowMaterial, (Renderer2DResources r) => r.geometryShadowShader, 0, 0);
		return rendererData.geometrySelfShadowMaterial;
	}

	private static Material GetGeometryUnshadowMaterial(this Renderer2DData rendererData)
	{
		rendererData.geometryUnshadowMaterial = GetProjectedShadowMaterial(rendererData.geometryUnshadowMaterial, (Renderer2DResources r) => r.geometryUnshadowShader, 1, 0);
		return rendererData.geometryUnshadowMaterial;
	}

	private static void CalculateFrustumCornersPerspective(Camera camera, float distance, NativeArray<Vector3> corners)
	{
		float fieldOfView = camera.fieldOfView;
		float num = Mathf.Tan(0.5f * fieldOfView * (MathF.PI / 180f)) * distance;
		float num2 = num * camera.aspect;
		corners[0] = new Vector3(num2, num, distance);
		corners[1] = new Vector3(num2, 0f - num, distance);
		corners[2] = new Vector3(0f - num2, num, distance);
		corners[3] = new Vector3(0f - num2, 0f - num, distance);
	}

	private static void CalculateFrustumCornersOrthographic(Camera camera, float distance, NativeArray<Vector3> corners)
	{
		float orthographicSize = camera.orthographicSize;
		float num = orthographicSize * camera.aspect;
		corners[0] = new Vector3(num, orthographicSize, distance);
		corners[1] = new Vector3(num, 0f - orthographicSize, distance);
		corners[2] = new Vector3(0f - num, orthographicSize, distance);
		corners[3] = new Vector3(0f - num, 0f - orthographicSize, distance);
	}

	private static Bounds CalculateWorldSpaceBounds(Camera camera, ILight2DCullResult cullResult)
	{
		NativeArray<Vector3> corners = new NativeArray<Vector3>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector3> corners2 = new NativeArray<Vector3>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		if (camera.orthographic)
		{
			CalculateFrustumCornersOrthographic(camera, camera.nearClipPlane, corners);
			CalculateFrustumCornersOrthographic(camera, camera.farClipPlane, corners2);
		}
		else
		{
			CalculateFrustumCornersPerspective(camera, camera.nearClipPlane, corners);
			CalculateFrustumCornersPerspective(camera, camera.farClipPlane, corners2);
		}
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < 4; i++)
		{
			vector2 = Vector3.Max(vector2, corners[i]);
			vector2 = Vector3.Max(vector2, corners2[i]);
			vector = Vector3.Min(vector, corners[i]);
			vector = Vector3.Min(vector, corners2[i]);
		}
		corners.Dispose();
		corners2.Dispose();
		vector2 = camera.transform.TransformPoint(vector2);
		vector = camera.transform.TransformPoint(vector);
		for (int j = 0; j < cullResult.visibleLights.Count; j++)
		{
			Vector3 position = cullResult.visibleLights[j].transform.position;
			vector2 = Vector3.Max(vector2, position);
			vector = Vector3.Min(vector, position);
		}
		Vector3 center = 0.5f * (vector + vector2);
		Vector3 size = vector2 - vector;
		return new Bounds(center, size);
	}

	internal static void CallOnBeforeRender(Camera camera, ILight2DCullResult cullResult)
	{
		if (ShadowCasterGroup2DManager.shadowCasterGroups == null)
		{
			return;
		}
		Bounds bounds = CalculateWorldSpaceBounds(camera, cullResult);
		List<ShadowCasterGroup2D> shadowCasterGroups = ShadowCasterGroup2DManager.shadowCasterGroups;
		for (int i = 0; i < shadowCasterGroups.Count; i++)
		{
			List<ShadowCaster2D> shadowCasters = shadowCasterGroups[i].GetShadowCasters();
			if (shadowCasters == null)
			{
				continue;
			}
			for (int j = 0; j < shadowCasters.Count; j++)
			{
				ShadowCaster2D shadowCaster2D = shadowCasters[j];
				if (shadowCaster2D != null && shadowCaster2D.shadowCastingSource == ShadowCaster2D.ShadowCastingSources.ShapeProvider)
				{
					ShapeProviderUtility.CallOnBeforeRender(shadowCaster2D.shadowShape2DProvider, shadowCaster2D.shadowShape2DComponent, shadowCaster2D.m_ShadowMesh, bounds);
				}
			}
		}
	}

	private static void CreateShadowRenderTexture(IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmdBuffer, int shadowIndex)
	{
		CreateShadowRenderTexture(pass, m_RenderTargetIds[shadowIndex], renderingData, cmdBuffer);
	}

	internal static void PrerenderShadows(UnsafeCommandBuffer cmdBuffer, Renderer2DData rendererData, ref LayerBatch layer, Light2D light, int shadowIndex, float shadowIntensity)
	{
		RenderShadows(cmdBuffer, rendererData, ref layer, light);
	}

	internal static bool PrerenderShadows(this IRenderPass2D pass, RenderingData renderingData, CommandBuffer cmdBuffer, ref LayerBatch layer, Light2D light, int shadowIndex, float shadowIntensity)
	{
		CreateShadowRenderTexture(pass, renderingData, cmdBuffer, shadowIndex);
		bool num = layer.shadowCasters.Count != 0;
		if (num)
		{
			cmdBuffer.SetRenderTarget(m_RenderTargets[shadowIndex].nameID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
			cmdBuffer.ClearRenderTarget(RTClearFlags.All, Color.clear);
			RenderShadows(CommandBufferHelpers.GetUnsafeCommandBuffer(cmdBuffer), pass.rendererData, ref layer, light);
		}
		m_LightInputTextures[shadowIndex] = m_RenderTargets[shadowIndex].nameID;
		return num;
	}

	private static void CreateShadowRenderTexture(IRenderPass2D pass, int handleId, RenderingData renderingData, CommandBuffer cmdBuffer)
	{
		float num = Mathf.Clamp(pass.rendererData.lightRenderTextureScale, 0.01f, 1f);
		int width = (int)((float)renderingData.cameraData.cameraTargetDescriptor.width * num);
		int height = (int)((float)renderingData.cameraData.cameraTargetDescriptor.height * num);
		RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height);
		desc.useMipMap = false;
		desc.autoGenerateMips = false;
		desc.depthStencilFormat = GraphicsFormatUtility.GetDepthStencilFormat(24);
		desc.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		desc.msaaSamples = 1;
		desc.dimension = TextureDimension.Tex2D;
		cmdBuffer.GetTemporaryRT(handleId, desc, FilterMode.Bilinear);
	}

	internal static void ReleaseShadowRenderTexture(CommandBuffer cmdBuffer, int shadowIndex)
	{
		cmdBuffer.ReleaseTemporaryRT(m_RenderTargetIds[shadowIndex]);
	}

	private static void SetShadowProjectionGlobals(UnsafeCommandBuffer cmdBuffer, ShadowCaster2D shadowCaster, Light2D light)
	{
		cmdBuffer.SetGlobalVector(k_ShadowModelScaleID, shadowCaster.m_CachedLossyScale);
		cmdBuffer.SetGlobalMatrix(k_ShadowModelMatrixID, shadowCaster.m_CachedShadowMatrix);
		cmdBuffer.SetGlobalMatrix(k_ShadowModelInvMatrixID, shadowCaster.m_CachedInverseShadowMatrix);
		cmdBuffer.SetGlobalFloat(k_ShadowSoftnessFalloffIntensityID, light.shadowSoftnessFalloffIntensity);
		if (shadowCaster.edgeProcessing == ShadowCaster2D.EdgeProcessing.None)
		{
			cmdBuffer.SetGlobalFloat(k_ShadowContractionDistanceID, shadowCaster.trimEdge);
		}
		else
		{
			cmdBuffer.SetGlobalFloat(k_ShadowContractionDistanceID, 0f);
		}
	}

	internal static void SetGlobalShadowTexture(CommandBuffer cmdBuffer, Light2D light, int shadowIndex)
	{
		cmdBuffer.SetGlobalTexture("_ShadowTex", m_LightInputTextures[shadowIndex]);
		cmdBuffer.SetGlobalColor(k_ShadowShadowColorID, k_ShadowColorLookup);
		cmdBuffer.SetGlobalColor(k_ShadowUnshadowColorID, k_UnshadowColorLookup);
	}

	internal static void SetGlobalShadowProp(IRasterCommandBuffer cmdBuffer)
	{
		cmdBuffer.SetGlobalColor(k_ShadowShadowColorID, k_ShadowColorLookup);
		cmdBuffer.SetGlobalColor(k_ShadowUnshadowColorID, k_UnshadowColorLookup);
	}

	private static bool ShadowCasterIsVisible(ShadowCaster2D shadowCaster)
	{
		return true;
	}

	private static Renderer GetRendererFromCaster(ShadowCaster2D shadowCaster, Light2D light, int layerToRender)
	{
		Renderer component = null;
		if (shadowCaster.IsLit(light) && shadowCaster != null && shadowCaster.IsShadowedLayer(layerToRender))
		{
			shadowCaster.TryGetComponent<Renderer>(out component);
		}
		return component;
	}

	private static void RenderProjectedShadows(UnsafeCommandBuffer cmdBuffer, int layerToRender, Light2D light, List<ShadowCaster2D> shadowCasters, Material projectedShadowsMaterial, int pass, ShadowTestType shadowTestType)
	{
		for (int i = 0; i < shadowCasters.Count; i++)
		{
			ShadowCaster2D shadowCaster2D = shadowCasters[i];
			if (ShadowTest(shadowTestType, shadowCaster2D) && ShadowCasterIsVisible(shadowCaster2D) && shadowCaster2D.castsShadows && shadowCaster2D.IsLit(light) && shadowCaster2D != null && projectedShadowsMaterial != null && shadowCaster2D.IsShadowedLayer(layerToRender) && shadowCaster2D.shadowCastingSource != 0 && shadowCaster2D.mesh != null)
			{
				SetShadowProjectionGlobals(cmdBuffer, shadowCaster2D, light);
				cmdBuffer.DrawMesh(shadowCaster2D.mesh, shadowCaster2D.transform.localToWorldMatrix, projectedShadowsMaterial, 0, pass);
			}
		}
	}

	private static int GetRendererSubmeshes(Renderer renderer, ShadowCaster2D shadowCaster2D)
	{
		return shadowCaster2D.spriteMaterialCount;
	}

	private static void RenderSpriteShadow(UnsafeCommandBuffer cmdBuffer, int layerToRender, Light2D light, List<ShadowCaster2D> shadowCasters, Material spriteShadowMaterial, Material spriteUnshadowMaterial, Material geometryShadowMaterial, Material geometryUnshadowMaterial, int pass, ShadowTestType shadowTestType)
	{
		for (int i = 0; i < shadowCasters.Count; i++)
		{
			ShadowCaster2D shadowCaster2D = shadowCasters[i];
			if (!ShadowTest(shadowTestType, shadowCaster2D) || !shadowCaster2D.IsLit(light))
			{
				continue;
			}
			Renderer rendererFromCaster = GetRendererFromCaster(shadowCaster2D, light, layerToRender);
			cmdBuffer.SetGlobalFloat(k_ShadowAlphaCutoffID, shadowCaster2D.alphaCutoff);
			if (rendererFromCaster != null)
			{
				if (ShadowCasterIsVisible(shadowCaster2D) && shadowCaster2D.selfShadows)
				{
					int rendererSubmeshes = GetRendererSubmeshes(rendererFromCaster, shadowCaster2D);
					for (int j = 0; j < rendererSubmeshes; j++)
					{
						cmdBuffer.DrawRenderer(rendererFromCaster, spriteShadowMaterial, j, pass);
					}
				}
				else
				{
					int rendererSubmeshes2 = GetRendererSubmeshes(rendererFromCaster, shadowCaster2D);
					for (int k = 0; k < rendererSubmeshes2; k++)
					{
						cmdBuffer.DrawRenderer(rendererFromCaster, spriteUnshadowMaterial, k, pass);
					}
				}
			}
			else if (shadowCaster2D.mesh != null)
			{
				if (ShadowCasterIsVisible(shadowCaster2D) && shadowCaster2D.selfShadows)
				{
					cmdBuffer.DrawMesh(shadowCaster2D.mesh, shadowCaster2D.transform.localToWorldMatrix, geometryShadowMaterial, 0, pass);
				}
				else
				{
					cmdBuffer.DrawMesh(shadowCaster2D.mesh, shadowCaster2D.transform.localToWorldMatrix, geometryUnshadowMaterial, 0, pass);
				}
			}
		}
	}

	internal static bool ShadowTest(ShadowTestType shadowTestType, ShadowCaster2D shadowCaster)
	{
		return shadowTestType switch
		{
			ShadowTestType.Always => true, 
			ShadowTestType.Unshadow => !shadowCaster.selfShadows, 
			_ => false, 
		};
	}

	private static void RenderShadows(UnsafeCommandBuffer cmdBuffer, Renderer2DData rendererData, ref LayerBatch layer, Light2D light)
	{
		using (new ProfilingScope(cmdBuffer, m_ProfilingSamplerShadows))
		{
			float value = light.boundingSphere.radius + (light.transform.position - light.boundingSphere.position).magnitude;
			cmdBuffer.SetGlobalVector(k_LightPosID, light.transform.position);
			cmdBuffer.SetGlobalFloat(k_ShadowRadiusID, value);
			cmdBuffer.SetGlobalFloat(k_SoftShadowAngle, MathF.PI / 180f * light.shadowSoftness * k_MaxShadowSoftnessAngle);
			Material projectedShadowMaterial = rendererData.GetProjectedShadowMaterial();
			rendererData.GetProjectedUnshadowMaterial();
			Material spriteShadowMaterial = rendererData.GetSpriteShadowMaterial();
			Material spriteUnshadowMaterial = rendererData.GetSpriteUnshadowMaterial();
			Material geometryShadowMaterial = rendererData.GetGeometryShadowMaterial();
			Material geometryUnshadowMaterial = rendererData.GetGeometryUnshadowMaterial();
			for (int i = 0; i < layer.shadowCasters.Count; i++)
			{
				List<ShadowCaster2D> shadowCasters = layer.shadowCasters[i].GetShadowCasters();
				RenderSpriteShadow(cmdBuffer, layer.startLayerID, light, shadowCasters, spriteShadowMaterial, spriteUnshadowMaterial, geometryShadowMaterial, geometryUnshadowMaterial, 0, ShadowTestType.Always);
				RenderProjectedShadows(cmdBuffer, layer.startLayerID, light, shadowCasters, projectedShadowMaterial, 0, ShadowTestType.Always);
				RenderProjectedShadows(cmdBuffer, layer.startLayerID, light, shadowCasters, projectedShadowMaterial, 1, ShadowTestType.Unshadow);
				RenderSpriteShadow(cmdBuffer, layer.startLayerID, light, shadowCasters, spriteShadowMaterial, spriteUnshadowMaterial, geometryShadowMaterial, geometryUnshadowMaterial, 1, ShadowTestType.Unshadow);
			}
		}
	}
}
