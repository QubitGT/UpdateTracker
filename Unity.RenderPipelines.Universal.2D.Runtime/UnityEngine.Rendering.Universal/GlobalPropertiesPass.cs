using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class GlobalPropertiesPass : ScriptableRenderPass
{
	private class PassData
	{
		internal Vector2Int screenParams;
	}

	private static readonly string k_SetGlobalProperties = "SetGlobalProperties";

	private static readonly ProfilingSampler m_SetGlobalPropertiesProfilingSampler = new ProfilingSampler(k_SetGlobalProperties);

	internal static void Setup(RenderGraph graph, ContextContainer frameData, Renderer2DData rendererData, UniversalCameraData cameraData, bool useLights)
	{
		Universal2DResourceData universal2DResourceData = frameData.Get<Universal2DResourceData>();
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<PassData>(k_SetGlobalProperties, out passData, m_SetGlobalPropertiesProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\2D\\Rendergraph\\GlobalPropertiesPass.cs", 19);
		passData.screenParams = Vector2Int.zero;
		cameraData.camera.TryGetComponent<PixelPerfectCamera>(out var component);
		if (component != null && component.enabled && component.offscreenRTSize != Vector2Int.zero)
		{
			passData.screenParams = component.offscreenRTSize;
		}
		if (useLights)
		{
			TextureHandle textureHandle = graph.ImportTexture(Light2DLookupTexture.GetLightLookupTexture_Rendergraph());
			TextureHandle textureHandle2 = graph.ImportTexture(Light2DLookupTexture.GetFallOffLookupTexture_Rendergraph());
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in textureHandle, Light2DLookupTexture.k_LightLookupID);
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in textureHandle2, Light2DLookupTexture.k_FalloffLookupID);
		}
		if (rendererData.useCameraSortingLayerTexture)
		{
			TextureHandle cameraSortingLayerTexture = universal2DResourceData.cameraSortingLayerTexture;
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in cameraSortingLayerTexture, CopyCameraSortingLayerPass.k_CameraSortingLayerTextureId);
		}
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			if (data.screenParams != Vector2Int.zero)
			{
				int x = data.screenParams.x;
				int y = data.screenParams.y;
				context.cmd.SetGlobalVector(ShaderPropertyId.screenParams, new Vector4(x, y, 1f + 1f / (float)x, 1f + 1f / (float)y));
			}
		});
	}
}
