using System;

namespace UnityEngine.Rendering.Universal;

[Obsolete("This is obsolete, UnityEngine.Rendering.ShaderVariantLogLevel instead.", true)]
public enum ShaderVariantLogLevel
{
	Disabled,
	[InspectorName("Only URP Shaders")]
	OnlyUniversalRPShaders,
	[InspectorName("All Shaders")]
	AllShaders
}
