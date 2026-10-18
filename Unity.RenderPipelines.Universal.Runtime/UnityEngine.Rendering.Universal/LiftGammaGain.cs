using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Post-processing/Lift, Gamma, Gain")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class LiftGammaGain : VolumeComponent, IPostProcessComponent
{
	public Vector4Parameter lift = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	public Vector4Parameter gamma = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	public Vector4Parameter gain = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	public bool IsActive()
	{
		Vector4 vector = new Vector4(1f, 1f, 1f, 0f);
		if (!(lift != vector) && !(gamma != vector))
		{
			return gain != vector;
		}
		return true;
	}

	[Obsolete("Unused #from(2023.1)", false)]
	public bool IsTileCompatible()
	{
		return true;
	}
}
