using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal sealed class StencilCrossFadeRenderPass
{
	private class PassData
	{
		public TextureHandle depthTarget;

		public Material[] stencilDitherMaskSeedMaterials;
	}

	private Material[] m_StencilDitherMaskSeedMaterials;

	private readonly int _StencilDitherPattern = Shader.PropertyToID("_StencilDitherPattern");

	private readonly int _StencilRefDitherMask = Shader.PropertyToID("_StencilRefDitherMask");

	private readonly int _StencilWriteDitherMask = Shader.PropertyToID("_StencilWriteDitherMask");

	private readonly ProfilingSampler m_ProfilingSampler;

	internal StencilCrossFadeRenderPass(Shader shader)
	{
		m_StencilDitherMaskSeedMaterials = new Material[3];
		m_ProfilingSampler = new ProfilingSampler("StencilDitherMaskSeed");
		int[] array = new int[3] { 4, 8, 12 };
		int num = 12;
		for (int i = 0; i < m_StencilDitherMaskSeedMaterials.Length; i++)
		{
			m_StencilDitherMaskSeedMaterials[i] = CoreUtils.CreateEngineMaterial(shader);
			m_StencilDitherMaskSeedMaterials[i].SetInteger(_StencilDitherPattern, i + 1);
			m_StencilDitherMaskSeedMaterials[i].SetFloat(_StencilWriteDitherMask, num);
			m_StencilDitherMaskSeedMaterials[i].SetFloat(_StencilRefDitherMask, array[i]);
		}
	}

	public void Dispose()
	{
		Material[] stencilDitherMaskSeedMaterials = m_StencilDitherMaskSeedMaterials;
		for (int i = 0; i < stencilDitherMaskSeedMaterials.Length; i++)
		{
			CoreUtils.Destroy(stencilDitherMaskSeedMaterials[i]);
		}
		m_StencilDitherMaskSeedMaterials = null;
	}

	public void Render(RenderGraph renderGraph, ScriptableRenderContext context, TextureHandle depthTarget)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>("Prepare Cross Fade Stencil", out passData, m_ProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\StencilCrossFadeRenderPass.cs", 61);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(depthTarget);
		passData.stencilDitherMaskSeedMaterials = m_StencilDitherMaskSeedMaterials;
		passData.depthTarget = depthTarget;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data.depthTarget, data.stencilDitherMaskSeedMaterials);
		});
	}

	private static void ExecutePass(RasterCommandBuffer cmd, RTHandle depthTarget, Material[] stencilDitherMaskSeedMaterials)
	{
		Vector2Int scaledSize = depthTarget.GetScaledSize(depthTarget.rtHandleProperties.currentViewportSize);
		Rect viewport = new Rect(0f, 0f, scaledSize.x, scaledSize.y);
		cmd.SetViewport(viewport);
		for (int i = 0; i < stencilDitherMaskSeedMaterials.Length; i++)
		{
			cmd.DrawProcedural(Matrix4x4.identity, stencilDitherMaskSeedMaterials[i], 0, MeshTopology.Triangles, 3, 1);
		}
	}
}
