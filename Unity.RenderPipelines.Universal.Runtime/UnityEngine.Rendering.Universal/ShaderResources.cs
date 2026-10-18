using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[ReloadGroup]
[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
public sealed class ShaderResources
{
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	[Reload("Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
	public Shader blitPS;

	[Reload("Shaders/Utils/CopyDepth.shader", ReloadAttribute.Package.Root)]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	public Shader copyDepthPS;

	[Obsolete("Obsolete, this feature will be supported by new 'ScreenSpaceShadows' renderer feature", true)]
	public Shader screenSpaceShadowPS;

	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	[Reload("Shaders/Utils/Sampling.shader", ReloadAttribute.Package.Root)]
	public Shader samplingPS;

	[Reload("Shaders/Utils/StencilDeferred.shader", ReloadAttribute.Package.Root)]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	public Shader stencilDeferredPS;

	[Reload("Shaders/Utils/FallbackError.shader", ReloadAttribute.Package.Root)]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	public Shader fallbackErrorPS;

	[Reload("Shaders/Utils/FallbackLoading.shader", ReloadAttribute.Package.Root)]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	public Shader fallbackLoadingPS;

	[Obsolete("Use fallbackErrorPS instead", true)]
	public Shader materialErrorPS;

	[Reload("Shaders/Utils/CoreBlit.shader", ReloadAttribute.Package.Root)]
	[SerializeField]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	internal Shader coreBlitPS;

	[Reload("Shaders/Utils/CoreBlitColorAndDepth.shader", ReloadAttribute.Package.Root)]
	[SerializeField]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	internal Shader coreBlitColorAndDepthPS;

	[Reload("Shaders/Utils/BlitHDROverlay.shader", ReloadAttribute.Package.Root)]
	[SerializeField]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	internal Shader blitHDROverlay;

	[Reload("Shaders/CameraMotionVectors.shader", ReloadAttribute.Package.Root)]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	public Shader cameraMotionVector;

	[Reload("Shaders/PostProcessing/LensFlareScreenSpace.shader", ReloadAttribute.Package.Root)]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	public Shader screenSpaceLensFlare;

	[Reload("Shaders/PostProcessing/LensFlareDataDriven.shader", ReloadAttribute.Package.Root)]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeShaders on GraphicsSettings. #from(2023.3)", false)]
	public Shader dataDrivenLensFlare;
}
