using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public class RenderGraphDefaultResources
{
	private RTHandle m_BlackTexture2D;

	private RTHandle m_WhiteTexture2D;

	private RTHandle m_ShadowTexture2D;

	public TextureHandle blackTexture { get; private set; }

	public TextureHandle whiteTexture { get; private set; }

	public TextureHandle clearTextureXR { get; private set; }

	public TextureHandle magentaTextureXR { get; private set; }

	public TextureHandle blackTextureXR { get; private set; }

	public TextureHandle blackTextureArrayXR { get; private set; }

	public TextureHandle blackUIntTextureXR { get; private set; }

	public TextureHandle blackTexture3DXR { get; private set; }

	public TextureHandle whiteTextureXR { get; private set; }

	public TextureHandle defaultShadowTexture { get; private set; }

	internal RenderGraphDefaultResources()
	{
		InitDefaultResourcesIfNeeded();
	}

	private void InitDefaultResourcesIfNeeded()
	{
		if (m_BlackTexture2D == null)
		{
			m_BlackTexture2D = RTHandles.Alloc(Texture2D.blackTexture);
		}
		if (m_WhiteTexture2D == null)
		{
			m_WhiteTexture2D = RTHandles.Alloc(Texture2D.whiteTexture);
		}
		if (m_ShadowTexture2D == null)
		{
			m_ShadowTexture2D = RTHandles.Alloc(1, 1, CoreUtils.GetDefaultDepthOnlyFormat(), 1, FilterMode.Point, TextureWrapMode.Repeat, TextureDimension.Tex2D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: true, isShadowMap: true, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, useDynamicScaleExplicit: false, RenderTextureMemoryless.None, VRTextureUsage.None, "DefaultShadowTexture");
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			commandBuffer.SetRenderTarget(m_ShadowTexture2D);
			commandBuffer.ClearRenderTarget(RTClearFlags.All, Color.white);
			Graphics.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}

	internal void Cleanup()
	{
		m_BlackTexture2D?.Release();
		m_BlackTexture2D = null;
		m_WhiteTexture2D?.Release();
		m_WhiteTexture2D = null;
		m_ShadowTexture2D?.Release();
		m_ShadowTexture2D = null;
	}

	internal void InitializeForRendering(RenderGraph renderGraph)
	{
		InitDefaultResourcesIfNeeded();
		blackTexture = renderGraph.ImportTexture(m_BlackTexture2D, isBuiltin: true);
		whiteTexture = renderGraph.ImportTexture(m_WhiteTexture2D, isBuiltin: true);
		defaultShadowTexture = renderGraph.ImportTexture(m_ShadowTexture2D, isBuiltin: true);
		clearTextureXR = renderGraph.ImportTexture(TextureXR.GetClearTexture(), isBuiltin: true);
		magentaTextureXR = renderGraph.ImportTexture(TextureXR.GetMagentaTexture(), isBuiltin: true);
		blackTextureXR = renderGraph.ImportTexture(TextureXR.GetBlackTexture(), isBuiltin: true);
		blackTextureArrayXR = renderGraph.ImportTexture(TextureXR.GetBlackTextureArray(), isBuiltin: true);
		blackUIntTextureXR = renderGraph.ImportTexture(TextureXR.GetBlackUIntTexture(), isBuiltin: true);
		blackTexture3DXR = renderGraph.ImportTexture(TextureXR.GetBlackTexture3D(), isBuiltin: true);
		whiteTextureXR = renderGraph.ImportTexture(TextureXR.GetWhiteTexture(), isBuiltin: true);
	}
}
