using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[Obsolete("Moved to UniversalRenderPipelineRuntimeXRResources on GraphicsSettings. #from(2023.3)", false)]
public class XRSystemData : ScriptableObject
{
	[Serializable]
	[ReloadGroup]
	[Obsolete("Moved to UniversalRenderPipelineRuntimeXRResources on GraphicsSettings. #from(2023.3)", false)]
	public sealed class ShaderResources
	{
		[Reload("Shaders/XR/XROcclusionMesh.shader", ReloadAttribute.Package.Root)]
		public Shader xrOcclusionMeshPS;

		[Reload("Shaders/XR/XRMirrorView.shader", ReloadAttribute.Package.Root)]
		public Shader xrMirrorViewPS;
	}

	[Obsolete("Moved to UniversalRenderPipelineRuntimeXRResources on GraphicsSettings. #from(2023.3)", false)]
	public ShaderResources shaders;
}
