using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DrawLight2DPass : ScriptableRenderPass
{
	internal class PassData
	{
		internal LayerBatch layerBatch;

		internal Renderer2DData rendererData;

		internal bool isVolumetric;

		internal TextureHandle normalMap;

		internal TextureHandle[] shadowTextures;

		internal int lightTextureIndex;
	}

	private static readonly string k_LightPass = "Light2D Pass";

	private static readonly string k_LightSRTPass = "Light2D SRT Pass";

	private static readonly string k_LightVolumetricPass = "Light2D Volumetric Pass";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_LightPass);

	private static readonly ProfilingSampler m_ProfilingSampleSRT = new ProfilingSampler(k_LightSRTPass);

	private static readonly ProfilingSampler m_ProfilingSamplerVolume = new ProfilingSampler(k_LightVolumetricPass);

	internal static readonly int k_InverseHDREmulationScaleID = Shader.PropertyToID("_InverseHDREmulationScale");

	internal static readonly string k_NormalMapID = "_NormalMap";

	internal static readonly string k_ShadowMapID = "_ShadowTex";

	private TextureHandle[] intermediateTexture = new TextureHandle[1];

	internal static MaterialPropertyBlock s_PropertyBlock = new MaterialPropertyBlock();

	internal void Setup(RenderGraph renderGraph, ref Renderer2DData rendererData)
	{
		foreach (Light2D visibleLight in rendererData.lightCullResult.visibleLights)
		{
			if (visibleLight.useCookieSprite && visibleLight.m_CookieSpriteTexture != null)
			{
				visibleLight.m_CookieSpriteTextureHandle = renderGraph.ImportTexture(visibleLight.m_CookieSpriteTexture);
			}
		}
	}

	private static bool TryGetShadowIndex(ref LayerBatch layerBatch, int lightIndex, out int shadowIndex)
	{
		shadowIndex = 0;
		for (int i = 0; i < layerBatch.shadowIndices.Count; i++)
		{
			if (layerBatch.shadowIndices[i] == lightIndex)
			{
				shadowIndex = i;
				return true;
			}
		}
		return false;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		throw new NotImplementedException();
	}

	private static void Execute(RasterCommandBuffer cmd, PassData passData, ref LayerBatch layerBatch, int lightTextureIndex)
	{
		cmd.SetGlobalFloat(k_InverseHDREmulationScaleID, 1f / passData.rendererData.hdrEmulationScale);
		int num = layerBatch.activeBlendStylesIndices[lightTextureIndex];
		string name = passData.rendererData.lightBlendStyles[num].name;
		cmd.BeginSample(name);
		int blendStyleIndex = (Renderer2D.supportsMRT ? lightTextureIndex : 0);
		if (!passData.isVolumetric)
		{
			RendererLighting.EnableBlendStyle(cmd, blendStyleIndex, enabled: true);
		}
		List<Light2D> lights = passData.layerBatch.lights;
		for (int i = 0; i < lights.Count; i++)
		{
			Light2D light2D = lights[i];
			if (!(light2D == null) && light2D.lightType != Light2D.LightType.Global && light2D.blendStyleIndex == num && (!passData.isVolumetric || (!(light2D.volumeIntensity <= 0f) && light2D.volumetricEnabled && layerBatch.endLayerValue == light2D.GetTopMostLitLayer())))
			{
				bool flag = passData.layerBatch.lightStats.useShadows && layerBatch.shadowIndices.Contains(i);
				Material lightMaterial = passData.rendererData.GetLightMaterial(light2D, passData.isVolumetric, flag);
				Mesh lightMesh = light2D.lightMesh;
				int batchSlotIndex = light2D.batchSlotIndex;
				int slot = RendererLighting.lightBatch.SlotIndex(batchSlotIndex);
				if (!RendererLighting.lightBatch.CanBatch(light2D, lightMaterial, batchSlotIndex, out var lightHash) && LightBatch.isBatchingSupported)
				{
					RendererLighting.lightBatch.Flush(cmd);
				}
				if (passData.layerBatch.lightStats.useNormalMap)
				{
					s_PropertyBlock.SetTexture(k_NormalMapID, passData.normalMap);
				}
				if (flag && TryGetShadowIndex(ref layerBatch, i, out var shadowIndex))
				{
					s_PropertyBlock.SetTexture(k_ShadowMapID, passData.shadowTextures[shadowIndex]);
				}
				if (!passData.isVolumetric || (passData.isVolumetric && light2D.volumetricEnabled))
				{
					RendererLighting.SetCookieShaderProperties(light2D, s_PropertyBlock);
				}
				RendererLighting.SetPerLightShaderGlobals(cmd, light2D, slot, passData.isVolumetric, flag, LightBatch.isBatchingSupported);
				if (light2D.normalMapQuality != Light2D.NormalMapQuality.Disabled || light2D.lightType == Light2D.LightType.Point)
				{
					RendererLighting.SetPerPointLightShaderGlobals(cmd, light2D, slot, LightBatch.isBatchingSupported);
				}
				if (LightBatch.isBatchingSupported)
				{
					RendererLighting.lightBatch.AddBatch(light2D, lightMaterial, light2D.GetMatrix(), lightMesh, 0, lightHash, batchSlotIndex);
					RendererLighting.lightBatch.Flush(cmd);
				}
				else
				{
					cmd.DrawMesh(lightMesh, light2D.GetMatrix(), lightMaterial, 0, 0, s_PropertyBlock);
				}
			}
		}
		RendererLighting.EnableBlendStyle(cmd, blendStyleIndex, enabled: false);
		cmd.EndSample(name);
	}

	private void InitializeRenderPass(IRasterRenderGraphBuilder builder, ContextContainer frameData, PassData passData, Renderer2DData rendererData, ref LayerBatch layerBatch, int batchIndex, bool isVolumetric = false)
	{
		Universal2DResourceData universal2DResourceData = frameData.Get<Universal2DResourceData>();
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		intermediateTexture[0] = universalResourceData.activeColorTexture;
		if (layerBatch.lightStats.useNormalMap)
		{
			builder.UseTexture(in universal2DResourceData.normalsTexture[batchIndex]);
		}
		if (layerBatch.lightStats.useShadows)
		{
			passData.shadowTextures = universal2DResourceData.shadowTextures[batchIndex];
			for (int i = 0; i < passData.shadowTextures.Length; i++)
			{
				builder.UseTexture(in passData.shadowTextures[i]);
			}
		}
		foreach (Light2D light in layerBatch.lights)
		{
			if (!(light == null) && light.m_CookieSpriteTextureHandle.IsValid() && (!isVolumetric || (isVolumetric && light.volumetricEnabled)))
			{
				builder.UseTexture(in light.m_CookieSpriteTextureHandle);
			}
		}
		passData.layerBatch = layerBatch;
		passData.rendererData = rendererData;
		passData.isVolumetric = isVolumetric;
		passData.normalMap = (layerBatch.lightStats.useNormalMap ? universal2DResourceData.normalsTexture[batchIndex] : TextureHandle.nullHandle);
		builder.AllowGlobalStateModification(value: true);
	}

	internal void Render(RenderGraph graph, ContextContainer frameData, Renderer2DData rendererData, ref LayerBatch layerBatch, int batchIndex, bool isVolumetric = false)
	{
		Universal2DResourceData universal2DResourceData = frameData.Get<Universal2DResourceData>();
		bool flag = ScriptableRenderPass.GetActiveDebugHandler(frameData.Get<UniversalCameraData>())?.IsLightingActive ?? true;
		if (!layerBatch.lightStats.useLights || (isVolumetric && !layerBatch.lightStats.useVolumetricLights) || !flag)
		{
			return;
		}
		if (!isVolumetric && !Renderer2D.supportsMRT)
		{
			for (int i = 0; i < layerBatch.activeBlendStylesIndices.Length; i++)
			{
				PassData passData;
				using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<PassData>(k_LightSRTPass, out passData, m_ProfilingSampleSRT, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\DrawLight2DPass.cs", 202);
				InitializeRenderPass(rasterRenderGraphBuilder, frameData, passData, rendererData, ref layerBatch, batchIndex, isVolumetric);
				TextureHandle[] array = universal2DResourceData.lightTextures[batchIndex];
				rasterRenderGraphBuilder.SetRenderAttachment(array[i], 0);
				passData.lightTextureIndex = i;
				rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
				{
					Execute(context.cmd, data, ref data.layerBatch, data.lightTextureIndex);
				});
			}
			return;
		}
		PassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = graph.AddRasterRenderPass<PassData>((!isVolumetric) ? k_LightPass : k_LightVolumetricPass, out passData2, (!isVolumetric) ? m_ProfilingSampler : m_ProfilingSamplerVolume, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\DrawLight2DPass.cs", 222);
		InitializeRenderPass(rasterRenderGraphBuilder2, frameData, passData2, rendererData, ref layerBatch, batchIndex, isVolumetric);
		TextureHandle[] array2 = ((!isVolumetric) ? universal2DResourceData.lightTextures[batchIndex] : intermediateTexture);
		for (int j = 0; j < array2.Length; j++)
		{
			rasterRenderGraphBuilder2.SetRenderAttachment(array2[j], j);
		}
		rasterRenderGraphBuilder2.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			for (int k = 0; k < data.layerBatch.activeBlendStylesIndices.Length; k++)
			{
				Execute(context.cmd, data, ref data.layerBatch, k);
			}
		});
	}
}
