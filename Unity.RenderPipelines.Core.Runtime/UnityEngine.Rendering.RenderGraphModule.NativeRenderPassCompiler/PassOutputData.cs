using System.Diagnostics;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

[DebuggerDisplay("PassOutputData: Res({resource.index})")]
internal readonly struct PassOutputData
{
	public readonly ResourceHandle resource;

	public PassOutputData(ResourceHandle resource)
	{
		this.resource = resource;
	}
}
