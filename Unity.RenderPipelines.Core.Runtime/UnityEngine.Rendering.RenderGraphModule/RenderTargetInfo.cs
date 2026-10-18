using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct RenderTargetInfo
{
	public int width;

	public int height;

	public int volumeDepth;

	public int msaaSamples;

	public GraphicsFormat format;

	public bool bindMS;
}
