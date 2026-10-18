using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[MovedFrom("")]
public class FullScreenPassRendererFeature : ScriptableRendererFeature, ISerializationCallbackReceiver
{
	public enum InjectionPoint
	{
		BeforeRenderingTransparents = 450,
		BeforeRenderingPostProcessing = 550,
		AfterRenderingPostProcessing = 600
	}

	internal class FullScreenRenderPass : ScriptableRenderPass
	{
		private class CopyPassData
		{
			internal TextureHandle inputTexture;
		}

		private class MainPassData
		{
			internal Material material;

			internal int passIndex;

			internal TextureHandle inputTexture;
		}

		private Material m_Material;

		private int m_PassIndex;

		private bool m_FetchActiveColor;

		private bool m_BindDepthStencilAttachment;

		private RTHandle m_CopiedColor;

		private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();

		public FullScreenRenderPass(string passName)
		{
			base.profilingSampler = new ProfilingSampler(passName);
		}

		public void SetupMembers(Material material, int passIndex, bool fetchActiveColor, bool bindDepthStencilAttachment)
		{
			m_Material = material;
			m_PassIndex = passIndex;
			m_FetchActiveColor = fetchActiveColor;
			m_BindDepthStencilAttachment = bindDepthStencilAttachment;
		}

		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ResetTarget();
			if (m_FetchActiveColor)
			{
				ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
			}
		}

		internal void ReAllocate(RenderTextureDescriptor desc)
		{
			desc.msaaSamples = 1;
			desc.depthStencilFormat = GraphicsFormat.None;
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_CopiedColor, in desc, FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, "_FullscreenPassColorCopy");
		}

		public void Dispose()
		{
			m_CopiedColor?.Release();
		}

		private static void ExecuteCopyColorPass(RasterCommandBuffer cmd, RTHandle sourceTexture)
		{
			Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1f, 1f, 0f, 0f), 0f, bilinear: false);
		}

		private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
		{
			s_SharedPropertyBlock.Clear();
			if (sourceTexture != null)
			{
				s_SharedPropertyBlock.SetTexture(ShaderPropertyId.blitTexture, sourceTexture);
			}
			s_SharedPropertyBlock.SetVector(ShaderPropertyId.blitScaleBias, new Vector4(1f, 1f, 0f, 0f));
			cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
		}

		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			ref CameraData cameraData = ref renderingData.cameraData;
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, base.profilingSampler))
			{
				RasterCommandBuffer rasterCommandBuffer = CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer);
				if (m_FetchActiveColor)
				{
					CoreUtils.SetRenderTarget(commandBuffer, m_CopiedColor);
					ExecuteCopyColorPass(rasterCommandBuffer, cameraData.renderer.cameraColorTargetHandle);
				}
				if (m_BindDepthStencilAttachment)
				{
					CoreUtils.SetRenderTarget(commandBuffer, cameraData.renderer.cameraColorTargetHandle, cameraData.renderer.cameraDepthTargetHandle);
				}
				else
				{
					CoreUtils.SetRenderTarget(commandBuffer, cameraData.renderer.cameraColorTargetHandle);
				}
				ExecuteMainPass(rasterCommandBuffer, m_FetchActiveColor ? m_CopiedColor : null, m_Material, m_PassIndex);
			}
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
			UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
			TextureHandle activeColorTexture;
			TextureHandle textureHandle;
			if (m_FetchActiveColor)
			{
				TextureHandle texture = universalResourceData.cameraColor;
				TextureDesc desc = renderGraph.GetTextureDesc(in texture);
				desc.name = "_CameraColorFullScreenPass";
				desc.clearBuffer = false;
				activeColorTexture = universalResourceData.activeColorTexture;
				textureHandle = renderGraph.CreateTexture(in desc);
				CopyPassData passData;
				using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<CopyPassData>("Copy Color Full Screen", out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\RendererFeatures\\FullScreenPassRendererFeature.cs", 226))
				{
					passData.inputTexture = activeColorTexture;
					rasterRenderGraphBuilder.UseTexture(in passData.inputTexture);
					rasterRenderGraphBuilder.SetRenderAttachment(textureHandle, 0);
					rasterRenderGraphBuilder.SetRenderFunc(delegate(CopyPassData data, RasterGraphContext rgContext)
					{
						ExecuteCopyColorPass(rgContext.cmd, data.inputTexture);
					});
				}
				activeColorTexture = textureHandle;
			}
			else
			{
				activeColorTexture = TextureHandle.nullHandle;
			}
			textureHandle = universalResourceData.activeColorTexture;
			MainPassData passData2;
			using IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<MainPassData>(base.passName, out passData2, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\RendererFeatures\\FullScreenPassRendererFeature.cs", 250);
			passData2.material = m_Material;
			passData2.passIndex = m_PassIndex;
			passData2.inputTexture = activeColorTexture;
			if (passData2.inputTexture.IsValid())
			{
				rasterRenderGraphBuilder2.UseTexture(in passData2.inputTexture);
			}
			bool num = (base.input & ScriptableRenderPassInput.Color) != 0;
			bool flag = (base.input & ScriptableRenderPassInput.Depth) != 0;
			bool flag2 = (base.input & ScriptableRenderPassInput.Motion) != 0;
			bool flag3 = (base.input & ScriptableRenderPassInput.Normal) != 0;
			if (num && universalCameraData.renderer.SupportsCameraOpaque())
			{
				TextureHandle texture = universalResourceData.cameraOpaqueTexture;
				rasterRenderGraphBuilder2.UseTexture(in texture);
			}
			if (flag)
			{
				TextureHandle texture = universalResourceData.cameraDepthTexture;
				rasterRenderGraphBuilder2.UseTexture(in texture);
			}
			if (flag2 && universalCameraData.renderer.SupportsMotionVectors())
			{
				TextureHandle texture = universalResourceData.motionVectorColor;
				rasterRenderGraphBuilder2.UseTexture(in texture);
				texture = universalResourceData.motionVectorDepth;
				rasterRenderGraphBuilder2.UseTexture(in texture);
			}
			if (flag3 && universalCameraData.renderer.SupportsCameraNormals())
			{
				TextureHandle texture = universalResourceData.cameraNormalsTexture;
				rasterRenderGraphBuilder2.UseTexture(in texture);
			}
			rasterRenderGraphBuilder2.SetRenderAttachment(textureHandle, 0);
			if (m_BindDepthStencilAttachment)
			{
				rasterRenderGraphBuilder2.SetRenderAttachmentDepth(universalResourceData.activeDepthTexture);
			}
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(MainPassData data, RasterGraphContext rgContext)
			{
				ExecuteMainPass(rgContext.cmd, data.inputTexture, data.material, data.passIndex);
			});
		}
	}

	private enum Version
	{
		Uninitialised = -1,
		Initial = 0,
		AddFetchColorBufferCheckbox = 1,
		Count = 2,
		Latest = 1
	}

	public InjectionPoint injectionPoint = InjectionPoint.AfterRenderingPostProcessing;

	public bool fetchColorBuffer = true;

	public ScriptableRenderPassInput requirements;

	public Material passMaterial;

	public int passIndex;

	public bool bindDepthStencilAttachment;

	private FullScreenRenderPass m_FullScreenPass;

	[SerializeField]
	[HideInInspector]
	private Version m_Version = Version.Uninitialised;

	public override void Create()
	{
		m_FullScreenPass = new FullScreenRenderPass(base.name);
	}

	internal override bool RequireRenderingLayers(bool isDeferred, bool needsGBufferAccurateNormals, out RenderingLayerUtils.Event atEvent, out RenderingLayerUtils.MaskSize maskSize)
	{
		atEvent = RenderingLayerUtils.Event.Opaque;
		maskSize = RenderingLayerUtils.MaskSize.Bits8;
		return false;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType != CameraType.Preview && renderingData.cameraData.cameraType != CameraType.Reflection && !UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
		{
			if (passMaterial == null)
			{
				Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - no material is assigned. Please make sure a material is assigned for this feature on the renderer asset.", base.name);
				return;
			}
			if (passIndex < 0 || passIndex >= passMaterial.passCount)
			{
				Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - the pass index is out of bounds for the material.", base.name);
				return;
			}
			m_FullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;
			m_FullScreenPass.ConfigureInput(requirements);
			m_FullScreenPass.SetupMembers(passMaterial, passIndex, fetchColorBuffer, bindDepthStencilAttachment);
			m_FullScreenPass.requiresIntermediateTexture = fetchColorBuffer;
			renderer.EnqueuePass(m_FullScreenPass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		m_FullScreenPass.Dispose();
	}

	private void UpgradeIfNeeded()
	{
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (m_Version == Version.Uninitialised)
		{
			m_Version = Version.AddFetchColorBufferCheckbox;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_Version == Version.Uninitialised)
		{
			m_Version = Version.Initial;
		}
		UpgradeIfNeeded();
	}
}
