namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal enum StoreReason
{
	InvalidReason,
	StoreImported,
	StoreUsedByLaterPass,
	DiscardImported,
	DiscardUnused,
	DiscardBindMs,
	NoMSAABuffer,
	Count
}
