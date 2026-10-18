using System;

namespace UnityEngine.Rendering;

public abstract class RenderPipelineGlobalSettings<TGlobalRenderPipelineSettings, TRenderPipeline> : RenderPipelineGlobalSettings where TGlobalRenderPipelineSettings : RenderPipelineGlobalSettings where TRenderPipeline : RenderPipeline
{
	private static Lazy<TGlobalRenderPipelineSettings> s_Instance = new Lazy<TGlobalRenderPipelineSettings>(() => GraphicsSettings.GetSettingsForRenderPipeline<TRenderPipeline>() as TGlobalRenderPipelineSettings);

	public static TGlobalRenderPipelineSettings instance => s_Instance.Value;

	public virtual void Reset()
	{
	}
}
