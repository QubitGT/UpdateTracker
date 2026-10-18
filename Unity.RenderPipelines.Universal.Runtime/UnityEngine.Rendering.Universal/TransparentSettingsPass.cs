using System;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal class TransparentSettingsPass : ScriptableRenderPass
{
	private bool m_shouldReceiveShadows;

	public TransparentSettingsPass(RenderPassEvent evt, bool shadowReceiveSupported)
	{
		base.profilingSampler = new ProfilingSampler("Set Transparent Parameters");
		base.renderPassEvent = evt;
		m_shouldReceiveShadows = shadowReceiveSupported;
	}

	public bool Setup()
	{
		return !m_shouldReceiveShadows;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		RasterCommandBuffer rasterCommandBuffer = CommandBufferHelpers.GetRasterCommandBuffer(renderingData.commandBuffer);
		using (new ProfilingScope(rasterCommandBuffer, base.profilingSampler))
		{
			ExecutePass(rasterCommandBuffer);
		}
	}

	public static void ExecutePass(RasterCommandBuffer rasterCommandBuffer)
	{
		MainLightShadowCasterPass.SetShadowParamsForEmptyShadowmap(rasterCommandBuffer);
		AdditionalLightsShadowCasterPass.SetShadowParamsForEmptyShadowmap(rasterCommandBuffer);
	}
}
