namespace UnityEngine.Rendering.Universal;

internal static class ScriptableRenderPass2DExtension
{
	internal static void GetInjectionPoint2D(this ScriptableRenderPass renderPass, out RenderPassEvent2D rpEvent, out int rpLayer)
	{
		rpLayer = int.MinValue;
		if (renderPass.renderPassEvent <= RenderPassEvent.BeforeRenderingTransparents)
		{
			rpEvent = RenderPassEvent2D.BeforeRendering;
		}
		else if (renderPass.renderPassEvent <= RenderPassEvent.BeforeRenderingPostProcessing)
		{
			rpEvent = RenderPassEvent2D.BeforeRenderingPostProcessing;
		}
		else if (renderPass.renderPassEvent <= RenderPassEvent.AfterRenderingPostProcessing)
		{
			rpEvent = RenderPassEvent2D.AfterRenderingPostProcessing;
		}
		else
		{
			rpEvent = RenderPassEvent2D.AfterRendering;
		}
	}
}
