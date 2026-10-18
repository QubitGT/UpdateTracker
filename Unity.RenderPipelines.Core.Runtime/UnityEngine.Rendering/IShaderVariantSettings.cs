using System;

namespace UnityEngine.Rendering;

[Obsolete("Use GraphicsSettings.GetRenderPipelineSettings<ShaderStrippingSetting>(). #from(23.3)")]
public interface IShaderVariantSettings
{
	ShaderVariantLogLevel shaderVariantLogLevel { get; set; }

	bool exportShaderVariants { get; set; }

	bool stripDebugVariants
	{
		get
		{
			return false;
		}
		set
		{
		}
	}
}
