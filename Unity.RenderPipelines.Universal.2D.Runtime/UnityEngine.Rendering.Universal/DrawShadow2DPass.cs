using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DrawShadow2DPass : ScriptableRenderPass
{
	internal class PassData
	{
		internal LayerBatch layerBatch;

		internal Renderer2DData rendererData;

		internal TextureHandle[] shadowTextures;

		internal TextureHandle shadowDepth;
	}

	private static readonly string k_ShadowPass = "Shadow2D UnsafePass";

	private static readonly string k_ShadowVolumetricPass = "Shadow2D Volumetric UnsafePass";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_ShadowPass);

	private static readonly ProfilingSampler m_ProfilingSamplerVolume = new ProfilingSampler(k_ShadowVolumetricPass);

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		throw new NotImplementedException();
	}

	private static void ExecuteShadowPass(UnsafeCommandBuffer cmd, PassData passData, Light2D light, int batchIndex)
	{
		cmd.SetRenderTarget(passData.shadowTextures[batchIndex], passData.shadowDepth);
		cmd.ClearRenderTarget(RTClearFlags.All, Color.clear, 1f, 0u);
		passData.rendererData.GetProjectedShadowMaterial();
		passData.rendererData.GetProjectedUnshadowMaterial();
		ShadowRendering.PrerenderShadows(cmd, passData.rendererData, ref passData.layerBatch, light, 0, light.shadowIntensity);
	}

	public void Render(RenderGraph graph, ContextContainer frameData, Renderer2DData rendererData, ref LayerBatch layerBatch, int batchIndex, bool isVolumetric = false)
	{
		Universal2DResourceData universal2DResourceData = frameData.Get<Universal2DResourceData>();
		frameData.Get<UniversalResourceData>();
		if (!layerBatch.lightStats.useShadows || (isVolumetric && !layerBatch.lightStats.useVolumetricShadowLights))
		{
			return;
		}
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = graph.AddUnsafePass<PassData>((!isVolumetric) ? k_ShadowPass : k_ShadowVolumetricPass, out passData, (!isVolumetric) ? m_ProfilingSampler : m_ProfilingSamplerVolume, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\DrawShadow2DPass.cs", 53);
		passData.layerBatch = layerBatch;
		passData.rendererData = rendererData;
		passData.shadowTextures = universal2DResourceData.shadowTextures[batchIndex];
		passData.shadowDepth = universal2DResourceData.shadowDepth;
		for (int i = 0; i < passData.shadowTextures.Length; i++)
		{
			unsafeRenderGraphBuilder.UseTexture(in passData.shadowTextures[i], AccessFlags.Write);
		}
		unsafeRenderGraphBuilder.UseTexture(in passData.shadowDepth, AccessFlags.Write);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			for (int j = 0; j < data.layerBatch.shadowIndices.Count; j++)
			{
				UnsafeCommandBuffer cmd = context.cmd;
				int index = data.layerBatch.shadowIndices[j];
				Light2D light = data.layerBatch.lights[index];
				ExecuteShadowPass(cmd, data, light, j);
			}
		});
	}
}
