using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public class ComputeGraphContext : IDerivedRendergraphContext
{
	private InternalRenderGraphContext wrappedContext;

	public ComputeCommandBuffer cmd;

	internal static ComputeCommandBuffer computecmd = new ComputeCommandBuffer(null, null, isAsync: false);

	public RenderGraphDefaultResources defaultResources => wrappedContext.defaultResources;

	public RenderGraphObjectPool renderGraphPool => wrappedContext.renderGraphPool;

	public void FromInternalContext(InternalRenderGraphContext context)
	{
		wrappedContext = context;
		computecmd.m_WrappedCommandBuffer = wrappedContext.cmd;
		computecmd.m_ExecutingPass = context.executingPass;
		cmd = computecmd;
	}
}
