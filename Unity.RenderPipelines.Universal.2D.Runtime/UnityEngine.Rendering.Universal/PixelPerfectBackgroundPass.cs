using System;

namespace UnityEngine.Rendering.Universal;

internal class PixelPerfectBackgroundPass : ScriptableRenderPass
{
	private static readonly ProfilingSampler m_ProfilingScope = new ProfilingSampler("Pixel Perfect Background Pass");

	public PixelPerfectBackgroundPass(RenderPassEvent evt)
	{
		base.renderPassEvent = evt;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingScope))
		{
			CoreUtils.SetRenderTarget(commandBuffer, BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Color, Color.black);
		}
	}
}
