using System.Diagnostics;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

[DebuggerDisplay("Res({handle.index}) : {loadAction} : {storeAction} : {memoryless}")]
internal readonly struct NativePassAttachment
{
	public readonly ResourceHandle handle;

	public readonly RenderBufferLoadAction loadAction;

	public readonly RenderBufferStoreAction storeAction;

	public readonly bool memoryless;

	public readonly int mipLevel;

	public readonly int depthSlice;

	public NativePassAttachment(ResourceHandle handle, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction, bool memoryless, int mipLevel, int depthSlice)
	{
		this.handle = handle;
		this.loadAction = loadAction;
		this.storeAction = storeAction;
		this.memoryless = memoryless;
		this.mipLevel = mipLevel;
		this.depthSlice = depthSlice;
	}
}
