using System;

namespace UnityEngine.Rendering;

public static class HDROutputUtils
{
	[Flags]
	public enum Operation
	{
		None = 0,
		ColorConversion = 1,
		ColorEncoding = 2
	}

	public struct HDRDisplayInformation
	{
		public int maxFullFrameToneMapLuminance;

		public int maxToneMapLuminance;

		public int minToneMapLuminance;

		public float paperWhiteNits;

		public HDRDisplayInformation(int maxFullFrameToneMapLuminance, int maxToneMapLuminance, int minToneMapLuminance, float hdrPaperWhiteNits)
		{
			this.maxFullFrameToneMapLuminance = maxFullFrameToneMapLuminance;
			this.maxToneMapLuminance = maxToneMapLuminance;
			this.minToneMapLuminance = minToneMapLuminance;
			paperWhiteNits = hdrPaperWhiteNits;
		}
	}

	public static class ShaderKeywords
	{
		public const string HDR_COLORSPACE_CONVERSION = "HDR_COLORSPACE_CONVERSION";

		public const string HDR_ENCODING = "HDR_ENCODING";

		public const string HDR_COLORSPACE_CONVERSION_AND_ENCODING = "HDR_COLORSPACE_CONVERSION_AND_ENCODING";

		public const string HDR_INPUT = "HDR_INPUT";

		internal static readonly ShaderKeyword HDRColorSpaceConversion = new ShaderKeyword("HDR_COLORSPACE_CONVERSION");

		internal static readonly ShaderKeyword HDREncoding = new ShaderKeyword("HDR_ENCODING");

		internal static readonly ShaderKeyword HDRColorSpaceConversionAndEncoding = new ShaderKeyword("HDR_COLORSPACE_CONVERSION_AND_ENCODING");

		internal static readonly ShaderKeyword HDRInput = new ShaderKeyword("HDR_INPUT");
	}

	private static class ShaderPropertyId
	{
		public static readonly int hdrColorSpace = Shader.PropertyToID("_HDRColorspace");

		public static readonly int hdrEncoding = Shader.PropertyToID("_HDREncoding");
	}

	public static bool GetColorSpaceForGamut(ColorGamut gamut, out int colorspace)
	{
		if (ColorGamutUtility.GetWhitePoint(gamut) != 0)
		{
			Debug.LogWarningFormat("{0} white point is currently unsupported for outputting to HDR.", gamut.ToString());
			colorspace = -1;
			return false;
		}
		switch (ColorGamutUtility.GetColorPrimaries(gamut))
		{
		case ColorPrimaries.Rec709:
			colorspace = 0;
			return true;
		case ColorPrimaries.Rec2020:
			colorspace = 1;
			return true;
		case ColorPrimaries.P3:
			colorspace = 2;
			return true;
		default:
			Debug.LogWarningFormat("{0} color space is currently unsupported for outputting to HDR.", gamut.ToString());
			colorspace = -1;
			return false;
		}
	}

	public static bool GetColorEncodingForGamut(ColorGamut gamut, out int encoding)
	{
		switch (ColorGamutUtility.GetTransferFunction(gamut))
		{
		case TransferFunction.Linear:
			encoding = 3;
			return true;
		case TransferFunction.PQ:
			encoding = 2;
			return true;
		case TransferFunction.Gamma22:
			encoding = 4;
			return true;
		case TransferFunction.sRGB:
			encoding = 0;
			return true;
		default:
			Debug.LogWarningFormat("{0} color encoding is currently unsupported for outputting to HDR.", gamut.ToString());
			encoding = -1;
			return false;
		}
	}

	public static void ConfigureHDROutput(Material material, ColorGamut gamut, Operation operations)
	{
		if (GetColorSpaceForGamut(gamut, out var colorspace) && GetColorEncodingForGamut(gamut, out var encoding))
		{
			material.SetInteger(ShaderPropertyId.hdrColorSpace, colorspace);
			material.SetInteger(ShaderPropertyId.hdrEncoding, encoding);
			CoreUtils.SetKeyword(material, ShaderKeywords.HDRColorSpaceConversionAndEncoding.name, operations.HasFlag(Operation.ColorConversion) && operations.HasFlag(Operation.ColorEncoding));
			CoreUtils.SetKeyword(material, ShaderKeywords.HDREncoding.name, operations.HasFlag(Operation.ColorEncoding) && !operations.HasFlag(Operation.ColorConversion));
			CoreUtils.SetKeyword(material, ShaderKeywords.HDRColorSpaceConversion.name, operations.HasFlag(Operation.ColorConversion) && !operations.HasFlag(Operation.ColorEncoding));
			CoreUtils.SetKeyword(material, ShaderKeywords.HDRInput.name, operations == Operation.None);
		}
	}

	public static void ConfigureHDROutput(MaterialPropertyBlock properties, ColorGamut gamut)
	{
		if (GetColorSpaceForGamut(gamut, out var colorspace) && GetColorEncodingForGamut(gamut, out var encoding))
		{
			properties.SetInteger(ShaderPropertyId.hdrColorSpace, colorspace);
			properties.SetInteger(ShaderPropertyId.hdrEncoding, encoding);
		}
	}

	public static void ConfigureHDROutput(Material material, Operation operations)
	{
		CoreUtils.SetKeyword(material, ShaderKeywords.HDRColorSpaceConversionAndEncoding.name, operations.HasFlag(Operation.ColorConversion) && operations.HasFlag(Operation.ColorEncoding));
		CoreUtils.SetKeyword(material, ShaderKeywords.HDREncoding.name, operations.HasFlag(Operation.ColorEncoding) && !operations.HasFlag(Operation.ColorConversion));
		CoreUtils.SetKeyword(material, ShaderKeywords.HDRColorSpaceConversion.name, operations.HasFlag(Operation.ColorConversion) && !operations.HasFlag(Operation.ColorEncoding));
		CoreUtils.SetKeyword(material, ShaderKeywords.HDRInput.name, operations == Operation.None);
	}

	public static void ConfigureHDROutput(ComputeShader computeShader, ColorGamut gamut, Operation operations)
	{
		if (GetColorSpaceForGamut(gamut, out var colorspace) && GetColorEncodingForGamut(gamut, out var encoding))
		{
			computeShader.SetInt(ShaderPropertyId.hdrColorSpace, colorspace);
			computeShader.SetInt(ShaderPropertyId.hdrEncoding, encoding);
			CoreUtils.SetKeyword(computeShader, ShaderKeywords.HDRColorSpaceConversionAndEncoding.name, operations.HasFlag(Operation.ColorConversion) && operations.HasFlag(Operation.ColorEncoding));
			CoreUtils.SetKeyword(computeShader, ShaderKeywords.HDREncoding.name, operations.HasFlag(Operation.ColorEncoding) && !operations.HasFlag(Operation.ColorConversion));
			CoreUtils.SetKeyword(computeShader, ShaderKeywords.HDRColorSpaceConversion.name, operations.HasFlag(Operation.ColorConversion) && !operations.HasFlag(Operation.ColorEncoding));
			CoreUtils.SetKeyword(computeShader, ShaderKeywords.HDRInput.name, operations == Operation.None);
		}
	}

	public static bool IsShaderVariantValid(ShaderKeywordSet shaderKeywordSet, bool isHDREnabled)
	{
		bool flag = shaderKeywordSet.IsEnabled(ShaderKeywords.HDREncoding) || shaderKeywordSet.IsEnabled(ShaderKeywords.HDRColorSpaceConversion) || shaderKeywordSet.IsEnabled(ShaderKeywords.HDRColorSpaceConversionAndEncoding) || shaderKeywordSet.IsEnabled(ShaderKeywords.HDRInput);
		if (!isHDREnabled && flag)
		{
			return false;
		}
		return true;
	}
}
