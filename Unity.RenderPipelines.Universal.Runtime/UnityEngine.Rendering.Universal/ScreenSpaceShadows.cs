using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

[SupportedOnRenderer(typeof(UniversalRendererData))]
[DisallowMultipleRendererFeature("Screen Space Shadows")]
[Tooltip("Screen Space Shadows")]
internal class ScreenSpaceShadows : ScriptableRendererFeature
{
	private class ScreenSpaceShadowsPass : ScriptableRenderPass
	{
		private class PassData
		{
			internal TextureHandle target;

			internal Material material;

			internal int shadowmapID;
		}

		private Material m_Material;

		private ScreenSpaceShadowsSettings m_CurrentSettings;

		private RTHandle m_RenderTarget;

		private int m_ScreenSpaceShadowmapTextureID;

		private PassData m_PassData;

		internal ScreenSpaceShadowsPass()
		{
			base.profilingSampler = new ProfilingSampler("Blit Screen Space Shadows");
			m_CurrentSettings = new ScreenSpaceShadowsSettings();
			m_ScreenSpaceShadowmapTextureID = Shader.PropertyToID("_ScreenSpaceShadowmapTexture");
			m_PassData = new PassData();
		}

		public void Dispose()
		{
			m_RenderTarget?.Release();
		}

		internal bool Setup(ScreenSpaceShadowsSettings featureSettings, Material material)
		{
			m_CurrentSettings = featureSettings;
			m_Material = material;
			ConfigureInput(ScriptableRenderPassInput.Depth);
			return m_Material != null;
		}

		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthStencilFormat = GraphicsFormat.None;
			descriptor.msaaSamples = 1;
			descriptor.graphicsFormat = (SystemInfo.IsFormatSupported(GraphicsFormat.R8_UNorm, GraphicsFormatUsage.Blend) ? GraphicsFormat.R8_UNorm : GraphicsFormat.B8G8R8A8_UNorm);
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_RenderTarget, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, 1, 0f, "_ScreenSpaceShadowmapTexture");
			cmd.SetGlobalTexture(m_RenderTarget.name, m_RenderTarget.nameID);
			ConfigureTarget(m_RenderTarget);
			ConfigureClear(ClearFlag.None, Color.white);
		}

		private void InitPassData(ref PassData passData)
		{
			passData.material = m_Material;
			passData.shadowmapID = m_ScreenSpaceShadowmapTextureID;
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			if (m_Material == null)
			{
				Debug.LogErrorFormat("{0}.Execute(): Missing material. ScreenSpaceShadows pass will not execute. Check for missing reference in the renderer resources.", GetType().Name);
				return;
			}
			RenderTextureDescriptor cameraTargetDescriptor = frameData.Get<UniversalCameraData>().cameraTargetDescriptor;
			cameraTargetDescriptor.depthStencilFormat = GraphicsFormat.None;
			cameraTargetDescriptor.msaaSamples = 1;
			cameraTargetDescriptor.graphicsFormat = (SystemInfo.IsFormatSupported(GraphicsFormat.R8_UNorm, GraphicsFormatUsage.Blend) ? GraphicsFormat.R8_UNorm : GraphicsFormat.B8G8R8A8_UNorm);
			TextureHandle target = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_ScreenSpaceShadowmapTexture", clear: true);
			PassData passData;
			using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\RendererFeatures\\ScreenSpaceShadows.cs", 203);
			passData.target = target;
			unsafeRenderGraphBuilder.UseTexture(in target, AccessFlags.WriteAll);
			InitPassData(ref passData);
			unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
			if (target.IsValid())
			{
				unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in target, m_ScreenSpaceShadowmapTextureID);
			}
			unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext rgContext)
			{
				ExecutePass(rgContext.cmd, data, data.target);
			});
		}

		private static void ExecutePass(RasterCommandBuffer cmd, PassData data, RTHandle target)
		{
			Blitter.BlitTexture(cmd, target, Vector2.one, data.material, 0);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadows, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadowCascades, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadowScreen, value: true);
		}

		private static void ExecutePass(UnsafeCommandBuffer cmd, PassData data, RTHandle target)
		{
			Blitter.BlitTexture(cmd, target, Vector2.one, data.material, 0);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadows, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadowCascades, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadowScreen, value: true);
		}

		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (m_Material == null)
			{
				Debug.LogErrorFormat("{0}.Execute(): Missing material. ScreenSpaceShadows pass will not execute. Check for missing reference in the renderer resources.", GetType().Name);
				return;
			}
			InitPassData(ref m_PassData);
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, base.profilingSampler))
			{
				ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), m_PassData, m_RenderTarget);
			}
		}
	}

	private class ScreenSpaceShadowsPostPass : ScriptableRenderPass
	{
		internal class PassData
		{
			internal ScreenSpaceShadowsPostPass pass;

			internal UniversalShadowData shadowData;
		}

		private static readonly RTHandle k_CurrentActive = RTHandles.Alloc(BuiltinRenderTextureType.CurrentActive);

		internal ScreenSpaceShadowsPostPass()
		{
			base.profilingSampler = new ProfilingSampler("Set Screen Space Shadow Keywords");
		}

		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureTarget(k_CurrentActive);
		}

		private static void ExecutePass(RasterCommandBuffer cmd, UniversalShadowData shadowData)
		{
			int mainLightShadowCascadesCount = shadowData.mainLightShadowCascadesCount;
			bool supportsMainLightShadows = shadowData.supportsMainLightShadows;
			bool value = supportsMainLightShadows && mainLightShadowCascadesCount == 1;
			bool value2 = supportsMainLightShadows && mainLightShadowCascadesCount > 1;
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadowScreen, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadows, value);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadowCascades, value2);
		}

		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			UniversalShadowData shadowData = renderingData.frameData.Get<UniversalShadowData>();
			using (new ProfilingScope(commandBuffer, base.profilingSampler))
			{
				ExecutePass(CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer), shadowData);
			}
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			PassData passData;
			using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(base.passName, out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\RendererFeatures\\ScreenSpaceShadows.cs", 308);
			TextureHandle activeColorTexture = frameData.Get<UniversalResourceData>().activeColorTexture;
			rasterRenderGraphBuilder.SetRenderAttachment(activeColorTexture, 0);
			passData.shadowData = frameData.Get<UniversalShadowData>();
			passData.pass = this;
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext rgContext)
			{
				ExecutePass(rgContext.cmd, data.shadowData);
			});
		}
	}

	[SerializeField]
	[HideInInspector]
	private Shader m_Shader;

	[SerializeField]
	private ScreenSpaceShadowsSettings m_Settings = new ScreenSpaceShadowsSettings();

	private Material m_Material;

	private ScreenSpaceShadowsPass m_SSShadowsPass;

	private ScreenSpaceShadowsPostPass m_SSShadowsPostPass;

	private const string k_ShaderName = "Hidden/Universal Render Pipeline/ScreenSpaceShadows";

	public override void Create()
	{
		if (m_SSShadowsPass == null)
		{
			m_SSShadowsPass = new ScreenSpaceShadowsPass();
		}
		if (m_SSShadowsPostPass == null)
		{
			m_SSShadowsPostPass = new ScreenSpaceShadowsPostPass();
		}
		LoadMaterial();
		m_SSShadowsPass.renderPassEvent = RenderPassEvent.BeforeRenderingGbuffer;
		m_SSShadowsPostPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
		{
			if (!LoadMaterial())
			{
				Debug.LogErrorFormat("{0}.AddRenderPasses(): Missing material. {1} render pass will not be added. Check for missing reference in the renderer resources.", GetType().Name, base.name);
			}
			else if (renderingData.shadowData.supportsMainLightShadows && renderingData.lightData.mainLightIndex != -1 && m_SSShadowsPass.Setup(m_Settings, m_Material))
			{
				bool flag = renderer is UniversalRenderer universalRenderer && universalRenderer.usesDeferredLighting;
				m_SSShadowsPass.renderPassEvent = (flag ? RenderPassEvent.BeforeRenderingGbuffer : ((RenderPassEvent)201));
				renderer.EnqueuePass(m_SSShadowsPass);
				renderer.EnqueuePass(m_SSShadowsPostPass);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		m_SSShadowsPass?.Dispose();
		m_SSShadowsPass = null;
		CoreUtils.Destroy(m_Material);
	}

	private bool LoadMaterial()
	{
		if (m_Material != null)
		{
			return true;
		}
		if (m_Shader == null)
		{
			m_Shader = Shader.Find("Hidden/Universal Render Pipeline/ScreenSpaceShadows");
			if (m_Shader == null)
			{
				return false;
			}
		}
		m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
		return m_Material != null;
	}
}
