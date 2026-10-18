using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DebugRendererLists
{
	private readonly DebugHandler m_DebugHandler;

	private readonly FilteringSettings m_FilteringSettings;

	private List<DebugRenderSetup> m_DebugRenderSetups = new List<DebugRenderSetup>(2);

	private List<RendererList> m_ActiveDebugRendererList = new List<RendererList>(2);

	private List<RendererListHandle> m_ActiveDebugRendererListHdl = new List<RendererListHandle>(2);

	public DebugRendererLists(DebugHandler debugHandler, FilteringSettings filteringSettings)
	{
		m_DebugHandler = debugHandler;
		m_FilteringSettings = filteringSettings;
	}

	private void CreateDebugRenderSetups(FilteringSettings filteringSettings)
	{
		DebugSceneOverrideMode sceneOverrideMode = m_DebugHandler.DebugDisplaySettings.renderingSettings.sceneOverrideMode;
		int num = ((sceneOverrideMode != DebugSceneOverrideMode.SolidWireframe && sceneOverrideMode != DebugSceneOverrideMode.ShadedWireframe) ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			m_DebugRenderSetups.Add(new DebugRenderSetup(m_DebugHandler, i, filteringSettings));
		}
	}

	private void DisposeDebugRenderLists()
	{
		foreach (DebugRenderSetup debugRenderSetup in m_DebugRenderSetups)
		{
			debugRenderSetup.Dispose();
		}
		m_DebugRenderSetups.Clear();
		m_ActiveDebugRendererList.Clear();
		m_ActiveDebugRendererListHdl.Clear();
	}

	internal void CreateRendererListsWithDebugRenderState(ScriptableRenderContext context, ref CullingResults cullResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock)
	{
		CreateDebugRenderSetups(filteringSettings);
		foreach (DebugRenderSetup debugRenderSetup in m_DebugRenderSetups)
		{
			DrawingSettings ds = debugRenderSetup.CreateDrawingSettings(drawingSettings);
			RenderStateBlock renderStateBlock2 = debugRenderSetup.GetRenderStateBlock(renderStateBlock);
			RendererList rl = default(RendererList);
			RenderingUtils.CreateRendererListWithRenderStateBlock(context, ref cullResults, ds, filteringSettings, renderStateBlock2, ref rl);
			m_ActiveDebugRendererList.Add(rl);
		}
	}

	internal void CreateRendererListsWithDebugRenderState(RenderGraph renderGraph, ref CullingResults cullResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock)
	{
		CreateDebugRenderSetups(filteringSettings);
		foreach (DebugRenderSetup debugRenderSetup in m_DebugRenderSetups)
		{
			DrawingSettings ds = debugRenderSetup.CreateDrawingSettings(drawingSettings);
			RenderStateBlock renderStateBlock2 = debugRenderSetup.GetRenderStateBlock(renderStateBlock);
			RendererListHandle rl = default(RendererListHandle);
			RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref cullResults, ds, filteringSettings, renderStateBlock2, ref rl);
			m_ActiveDebugRendererListHdl.Add(rl);
		}
	}

	internal void PrepareRendererListForRasterPass(IRasterRenderGraphBuilder builder)
	{
		foreach (RendererListHandle item in m_ActiveDebugRendererListHdl)
		{
			RendererListHandle input = item;
			builder.UseRendererList(in input);
		}
	}

	internal void DrawWithRendererList(RasterCommandBuffer cmd)
	{
		foreach (DebugRenderSetup debugRenderSetup in m_DebugRenderSetups)
		{
			debugRenderSetup.Begin(cmd);
			RendererList rendererList = default(RendererList);
			if (m_ActiveDebugRendererList.Count > 0)
			{
				rendererList = m_ActiveDebugRendererList[debugRenderSetup.GetIndex()];
			}
			else if (m_ActiveDebugRendererListHdl.Count > 0)
			{
				rendererList = m_ActiveDebugRendererListHdl[debugRenderSetup.GetIndex()];
			}
			debugRenderSetup.DrawWithRendererList(cmd, ref rendererList);
			debugRenderSetup.End(cmd);
		}
		DisposeDebugRenderLists();
	}
}
