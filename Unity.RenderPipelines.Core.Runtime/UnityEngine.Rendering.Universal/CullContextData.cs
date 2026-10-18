using System;

namespace UnityEngine.Rendering.Universal;

public class CullContextData : ContextItem
{
	internal ScriptableRenderContext? m_RenderContext;

	public override void Reset()
	{
		m_RenderContext = null;
	}

	public void SetRenderContext(in ScriptableRenderContext renderContext)
	{
		m_RenderContext = renderContext;
	}

	public CullingResults Cull(ref ScriptableCullingParameters parameters)
	{
		if (!m_RenderContext.HasValue)
		{
			throw new InvalidOperationException("The ScriptableRenderContext member is not set.");
		}
		return m_RenderContext.Value.Cull(ref parameters);
	}

	public void CullShadowCasters(CullingResults cullingResults, ShadowCastersCullingInfos shadowCastersCullingInfos)
	{
		if (!m_RenderContext.HasValue)
		{
			throw new InvalidOperationException("The ScriptableRenderContext member is not set.");
		}
		m_RenderContext.Value.CullShadowCasters(cullingResults, shadowCastersCullingInfos);
	}
}
