using System;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal struct PostProcessPasses : IDisposable
{
	private ColorGradingLutPass m_ColorGradingLutPass;

	private PostProcessPass m_PostProcessPass;

	private PostProcessPass m_FinalPostProcessPass;

	internal RTHandle m_AfterPostProcessColor;

	internal RTHandle m_ColorGradingLut;

	private PostProcessData m_RendererPostProcessData;

	private PostProcessData m_CurrentPostProcessData;

	private Material m_BlitMaterial;

	public ColorGradingLutPass colorGradingLutPass => m_ColorGradingLutPass;

	public PostProcessPass postProcessPass => m_PostProcessPass;

	public PostProcessPass finalPostProcessPass => m_FinalPostProcessPass;

	public RTHandle afterPostProcessColor => m_AfterPostProcessColor;

	public RTHandle colorGradingLut => m_ColorGradingLut;

	public bool isCreated => m_CurrentPostProcessData != null;

	public PostProcessPasses(PostProcessData rendererPostProcessData, ref PostProcessParams postProcessParams)
	{
		m_ColorGradingLutPass = null;
		m_PostProcessPass = null;
		m_FinalPostProcessPass = null;
		m_CurrentPostProcessData = null;
		m_AfterPostProcessColor = null;
		m_ColorGradingLut = null;
		m_RendererPostProcessData = rendererPostProcessData;
		m_BlitMaterial = postProcessParams.blitMaterial;
		Recreate(rendererPostProcessData, ref postProcessParams);
	}

	public void Recreate(PostProcessData data, ref PostProcessParams ppParams)
	{
		if ((bool)m_RendererPostProcessData)
		{
			data = m_RendererPostProcessData;
		}
		if (!(data == m_CurrentPostProcessData))
		{
			if (m_CurrentPostProcessData != null)
			{
				m_ColorGradingLutPass?.Cleanup();
				m_PostProcessPass?.Cleanup();
				m_FinalPostProcessPass?.Cleanup();
				m_ColorGradingLutPass = null;
				m_PostProcessPass = null;
				m_FinalPostProcessPass = null;
				m_CurrentPostProcessData = null;
			}
			if (data != null)
			{
				m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrePasses, data);
				m_PostProcessPass = new PostProcessPass((RenderPassEvent)599, data, ref ppParams);
				m_FinalPostProcessPass = new PostProcessPass((RenderPassEvent)999, data, ref ppParams);
				m_CurrentPostProcessData = data;
			}
		}
	}

	public void Dispose()
	{
		m_ColorGradingLutPass?.Cleanup();
		m_PostProcessPass?.Cleanup();
		m_FinalPostProcessPass?.Cleanup();
		m_AfterPostProcessColor?.Release();
		m_ColorGradingLut?.Release();
	}

	internal void ReleaseRenderTargets()
	{
		m_AfterPostProcessColor?.Release();
		m_PostProcessPass?.Dispose();
		m_FinalPostProcessPass?.Dispose();
		m_ColorGradingLut?.Release();
	}
}
