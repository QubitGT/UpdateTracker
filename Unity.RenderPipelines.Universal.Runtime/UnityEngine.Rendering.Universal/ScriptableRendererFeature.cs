using System;

namespace UnityEngine.Rendering.Universal;

[ExcludeFromPreset]
public abstract class ScriptableRendererFeature : ScriptableObject, IDisposable
{
	[SerializeField]
	[HideInInspector]
	private bool m_Active = true;

	public bool isActive => m_Active;

	public abstract void Create();

	public virtual void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
	{
	}

	public abstract void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData);

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public virtual void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
	}

	private void OnEnable()
	{
		if (RenderPipelineManager.currentPipeline is UniversalRenderPipeline)
		{
			Create();
		}
	}

	private void OnValidate()
	{
		if (RenderPipelineManager.currentPipeline is UniversalRenderPipeline)
		{
			Create();
		}
	}

	internal virtual bool SupportsNativeRenderPass()
	{
		return false;
	}

	internal virtual bool RequireRenderingLayers(bool isDeferred, bool needsGBufferAccurateNormals, out RenderingLayerUtils.Event atEvent, out RenderingLayerUtils.MaskSize maskSize)
	{
		atEvent = RenderingLayerUtils.Event.DepthNormalPrePass;
		maskSize = RenderingLayerUtils.MaskSize.Bits8;
		return false;
	}

	public void SetActive(bool active)
	{
		m_Active = active;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
