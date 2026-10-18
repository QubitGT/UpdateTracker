using System.Runtime.CompilerServices;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal static class RenderGraphUtils
{
	private class PassData
	{
		internal TextureHandle texture;

		internal int nameID;
	}

	private static ProfilingSampler s_SetGlobalTextureProfilingSampler = new ProfilingSampler("Set Global Texture");

	internal const int GBufferSize = 7;

	internal const int DBufferSize = 3;

	internal const int LightTextureSize = 4;

	internal static void UseDBufferIfValid(IRasterRenderGraphBuilder builder, UniversalResourceData resourceData)
	{
		TextureHandle[] dBuffer = resourceData.dBuffer;
		for (int i = 0; i < 3; i++)
		{
			TextureHandle input = dBuffer[i];
			if (input.IsValid())
			{
				builder.UseTexture(in input);
			}
		}
	}

	public static void SetGlobalTexture(RenderGraph graph, int nameId, TextureHandle handle, string passName = "Set Global Texture", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = graph.AddRasterRenderPass<PassData>(passName, out passData, s_SetGlobalTextureProfilingSampler, file, line);
		passData.nameID = nameId;
		passData.texture = handle;
		rasterRenderGraphBuilder.UseTexture(in handle);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in handle, nameId);
		rasterRenderGraphBuilder.SetRenderFunc<PassData>(delegate
		{
		});
	}
}
