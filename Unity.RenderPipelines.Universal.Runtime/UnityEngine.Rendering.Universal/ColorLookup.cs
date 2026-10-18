using System;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Post-processing/Color Lookup")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class ColorLookup : VolumeComponent, IPostProcessComponent
{
	[Tooltip("A 2D Lookup Texture (LUT) to use for color grading.")]
	public TextureParameter texture = new TextureParameter(null);

	[Tooltip("How much of the lookup texture will contribute to the color grading effect.")]
	public ClampedFloatParameter contribution = new ClampedFloatParameter(0f, 0f, 1f);

	public bool IsActive()
	{
		if (contribution.value > 0f)
		{
			return ValidateLUT();
		}
		return false;
	}

	[Obsolete("Unused #from(2023.1)", false)]
	public bool IsTileCompatible()
	{
		return true;
	}

	public bool ValidateLUT()
	{
		UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
		if (asset == null || texture.value == null)
		{
			return false;
		}
		int colorGradingLutSize = asset.colorGradingLutSize;
		if (texture.value.height != colorGradingLutSize)
		{
			return false;
		}
		bool flag = false;
		Texture value = texture.value;
		if (!(value is Texture2D texture2D))
		{
			if (value is RenderTexture renderTexture)
			{
				flag |= renderTexture.dimension == TextureDimension.Tex2D && renderTexture.width == colorGradingLutSize * colorGradingLutSize && !renderTexture.sRGB;
			}
		}
		else
		{
			flag |= texture2D.width == colorGradingLutSize * colorGradingLutSize && !GraphicsFormatUtility.IsSRGBFormat(texture2D.graphicsFormat);
		}
		return flag;
	}
}
