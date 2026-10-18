using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct RenderGraphParameters
{
	public string executionName;

	public int currentFrameIndex;

	public bool rendererListCulling;

	public ScriptableRenderContext scriptableRenderContext;

	public CommandBuffer commandBuffer;

	internal bool invalidContextForTesting;
}
