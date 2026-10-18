namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal enum LoadReason
{
	InvalidReason,
	LoadImported,
	LoadPreviouslyWritten,
	ClearImported,
	ClearCreated,
	FullyRewritten,
	Count
}
