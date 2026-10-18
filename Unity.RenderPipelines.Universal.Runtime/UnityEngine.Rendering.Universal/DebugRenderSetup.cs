using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DebugRenderSetup : IDisposable
{
	private readonly DebugHandler m_DebugHandler;

	private readonly FilteringSettings m_FilteringSettings;

	private readonly int m_Index;

	private DebugDisplaySettingsMaterial MaterialSettings => m_DebugHandler.DebugDisplaySettings.materialSettings;

	private DebugDisplaySettingsRendering RenderingSettings => m_DebugHandler.DebugDisplaySettings.renderingSettings;

	private DebugDisplaySettingsLighting LightingSettings => m_DebugHandler.DebugDisplaySettings.lightingSettings;

	internal void Begin(RasterCommandBuffer cmd)
	{
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Wireframe:
			cmd.SetWireframe(enable: true);
			break;
		case DebugSceneOverrideMode.SolidWireframe:
		case DebugSceneOverrideMode.ShadedWireframe:
			if (m_Index == 1)
			{
				cmd.SetWireframe(enable: true);
			}
			break;
		}
	}

	internal void End(RasterCommandBuffer cmd)
	{
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Wireframe:
			cmd.SetWireframe(enable: false);
			break;
		case DebugSceneOverrideMode.SolidWireframe:
		case DebugSceneOverrideMode.ShadedWireframe:
			if (m_Index == 1)
			{
				cmd.SetWireframe(enable: false);
			}
			break;
		}
	}

	internal DebugRenderSetup(DebugHandler debugHandler, int index, FilteringSettings filteringSettings)
	{
		m_DebugHandler = debugHandler;
		m_FilteringSettings = filteringSettings;
		m_Index = index;
	}

	internal void CreateRendererList(ScriptableRenderContext context, ref CullingResults cullResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock, ref RendererList rendererList)
	{
		RenderingUtils.CreateRendererListWithRenderStateBlock(context, ref cullResults, drawingSettings, filteringSettings, renderStateBlock, ref rendererList);
	}

	internal void CreateRendererList(RenderGraph renderGraph, ref CullingResults cullResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock, ref RendererListHandle rendererListHdl)
	{
		RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref cullResults, drawingSettings, filteringSettings, renderStateBlock, ref rendererListHdl);
	}

	internal void DrawWithRendererList(RasterCommandBuffer cmd, ref RendererList rendererList)
	{
		if (rendererList.isValid)
		{
			cmd.DrawRendererList(rendererList);
		}
	}

	internal DrawingSettings CreateDrawingSettings(DrawingSettings drawingSettings)
	{
		if (MaterialSettings.vertexAttributeDebugMode != 0)
		{
			Material replacementMaterial = m_DebugHandler.ReplacementMaterial;
			DrawingSettings result = drawingSettings;
			result.overrideMaterial = replacementMaterial;
			result.overrideMaterialPassIndex = 0;
			return result;
		}
		return drawingSettings;
	}

	internal RenderStateBlock GetRenderStateBlock(RenderStateBlock renderStateBlock)
	{
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Overdraw:
		{
			bool num = m_FilteringSettings.renderQueueRange == RenderQueueRange.opaque || m_FilteringSettings.renderQueueRange == RenderQueueRange.all;
			bool flag = m_FilteringSettings.renderQueueRange == RenderQueueRange.transparent || m_FilteringSettings.renderQueueRange == RenderQueueRange.all;
			bool flag2 = m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.Opaque || m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.All;
			bool flag3 = m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.Transparent || m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.All;
			BlendMode destinationColorBlendMode = (((num && flag2) || (flag && flag3)) ? BlendMode.One : BlendMode.Zero);
			RenderTargetBlendState blendState = new RenderTargetBlendState(ColorWriteMask.All, BlendMode.One, destinationColorBlendMode);
			renderStateBlock.blendState = new BlendState
			{
				blendState0 = blendState
			};
			renderStateBlock.mask = RenderStateMask.Blend;
			break;
		}
		case DebugSceneOverrideMode.Wireframe:
			renderStateBlock.rasterState = new RasterState(CullMode.Off);
			renderStateBlock.mask = RenderStateMask.Raster;
			break;
		case DebugSceneOverrideMode.SolidWireframe:
		case DebugSceneOverrideMode.ShadedWireframe:
			if (m_Index == 1)
			{
				renderStateBlock.rasterState = new RasterState(CullMode.Back, -1, -1f);
				renderStateBlock.mask = RenderStateMask.Raster;
			}
			break;
		}
		return renderStateBlock;
	}

	internal int GetIndex()
	{
		return m_Index;
	}

	public void Dispose()
	{
	}
}
