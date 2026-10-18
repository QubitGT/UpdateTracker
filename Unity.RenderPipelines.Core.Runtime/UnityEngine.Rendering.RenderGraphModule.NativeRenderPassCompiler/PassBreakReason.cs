namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal enum PassBreakReason
{
	NotOptimized,
	TargetSizeMismatch,
	NextPassReadsTexture,
	NextPassTargetsTexture,
	NonRasterPass,
	DifferentDepthTextures,
	AttachmentLimitReached,
	SubPassLimitReached,
	EndOfGraph,
	FRStateMismatch,
	DifferentShadingRateImages,
	DifferentShadingRateStates,
	PassMergingDisabled,
	Merged,
	Count
}
