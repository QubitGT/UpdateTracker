using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

internal struct OccluderHandles
{
	public TextureHandle occluderDepthPyramid;

	public BufferHandle occlusionDebugOverlay;

	public bool IsValid()
	{
		return occluderDepthPyramid.IsValid();
	}

	public void UseForOcclusionTest(IBaseRenderGraphBuilder builder)
	{
		builder.UseTexture(in occluderDepthPyramid);
		if (occlusionDebugOverlay.IsValid())
		{
			builder.UseBuffer(in occlusionDebugOverlay, AccessFlags.ReadWrite);
		}
	}

	public void UseForOccluderUpdate(IBaseRenderGraphBuilder builder)
	{
		builder.UseTexture(in occluderDepthPyramid, AccessFlags.ReadWrite);
		if (occlusionDebugOverlay.IsValid())
		{
			builder.UseBuffer(in occlusionDebugOverlay, AccessFlags.ReadWrite);
		}
	}
}
