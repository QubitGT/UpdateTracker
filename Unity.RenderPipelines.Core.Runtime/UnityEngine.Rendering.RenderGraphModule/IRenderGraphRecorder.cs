using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public interface IRenderGraphRecorder
{
	void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData);
}
