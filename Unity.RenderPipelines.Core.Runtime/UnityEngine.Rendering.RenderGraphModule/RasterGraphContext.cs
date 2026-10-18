using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct RasterGraphContext : IDerivedRendergraphContext
{
	private InternalRenderGraphContext wrappedContext;

	public RasterCommandBuffer cmd;

	internal static RasterCommandBuffer rastercmd = new RasterCommandBuffer(null, null, isAsync: false);

	public RenderGraphDefaultResources defaultResources => wrappedContext.defaultResources;

	public RenderGraphObjectPool renderGraphPool => wrappedContext.renderGraphPool;

	public void FromInternalContext(InternalRenderGraphContext context)
	{
		wrappedContext = context;
		rastercmd.m_WrappedCommandBuffer = wrappedContext.cmd;
		rastercmd.m_ExecutingPass = context.executingPass;
		cmd = rastercmd;
	}
}
