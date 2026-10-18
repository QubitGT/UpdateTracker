using UnityEngine;

namespace DigitalOpus.MB.Core;

public static class MB_TextureCombinerSRPCustom_Standard
{
	private static bool _IsCreatingAtlasForProperty(MB3_TextureCombinerPipeline.TexturePipelineData data, string property)
	{
		for (int i = 0; i < data.texPropertyNames.Count; i++)
		{
			if (property.Equals(data.texPropertyNames[i].name))
			{
				if (MB3_TextureCombinerPipeline._ShouldWeCreateAtlasForThisProperty(i, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	internal static void ConfigureMaterialKeywords(MB3_TextureCombinerPipeline.TexturePipelineData data, Material resultMat)
	{
		if (MBVersion.IsMaterialKeywordValid(resultMat, "_NORMALMAP"))
		{
			if (_IsCreatingAtlasForProperty(data, "_BumpMap"))
			{
				resultMat.EnableKeyword("_NORMALMAP");
			}
			else
			{
				resultMat.DisableKeyword("_NORMALMAP");
			}
		}
		if (MBVersion.IsMaterialKeywordValid(resultMat, "_METALLICGLOSSMAP"))
		{
			if (_IsCreatingAtlasForProperty(data, "_MetallicGlossMap"))
			{
				resultMat.EnableKeyword("_METALLICGLOSSMAP");
			}
			else
			{
				resultMat.DisableKeyword("_METALLICGLOSSMAP");
			}
		}
		if (MBVersion.IsMaterialKeywordValid(resultMat, "_METALLICSPECGLOSSMAP"))
		{
			if (_IsCreatingAtlasForProperty(data, "_MetallicGlossMap"))
			{
				resultMat.EnableKeyword("_METALLICSPECGLOSSMAP");
			}
			else
			{
				resultMat.DisableKeyword("_METALLICSPECGLOSSMAP");
			}
		}
		if (MBVersion.IsMaterialKeywordValid(resultMat, "_PARALLAXMAP"))
		{
			if (_IsCreatingAtlasForProperty(data, "_ParallaxMap"))
			{
				resultMat.EnableKeyword("_PARALLAXMAP");
			}
			else
			{
				resultMat.DisableKeyword("_PARALLAXMAP");
			}
		}
		if (MBVersion.IsMaterialKeywordValid(resultMat, "_OCCLUSIONMAP"))
		{
			if (_IsCreatingAtlasForProperty(data, "_OcclusionMap"))
			{
				resultMat.EnableKeyword("_OCCLUSIONMAP");
			}
			else
			{
				resultMat.DisableKeyword("_OCCLUSIONMAP");
			}
		}
		if (MBVersion.IsMaterialKeywordValid(resultMat, "_EMISSIONMAP"))
		{
			if (_IsCreatingAtlasForProperty(data, "_EmissionMap"))
			{
				resultMat.EnableKeyword("_EMISSIONMAP");
			}
			else
			{
				resultMat.DisableKeyword("_EMISSIONMAP");
			}
		}
	}
}
