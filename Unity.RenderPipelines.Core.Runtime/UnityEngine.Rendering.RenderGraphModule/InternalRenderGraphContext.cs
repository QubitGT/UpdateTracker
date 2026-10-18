using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public class InternalRenderGraphContext
{
	internal ScriptableRenderContext renderContext;

	internal CommandBuffer cmd;

	internal RenderGraphObjectPool renderGraphPool;

	internal RenderGraphDefaultResources defaultResources;

	internal RenderGraphPass executingPass;

	internal bool contextlessTesting;
}
