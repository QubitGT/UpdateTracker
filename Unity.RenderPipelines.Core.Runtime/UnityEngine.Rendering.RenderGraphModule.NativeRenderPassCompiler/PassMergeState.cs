namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal enum PassMergeState
{
	None = -1,
	Begin,
	SubPass,
	End
}
