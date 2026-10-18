using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class TextureBlenderStandardMetallic : TextureBlender
{
	private enum Prop
	{
		doColor,
		doMetallic,
		doEmission,
		doBump,
		doNone
	}

	private static Color NeutralNormalMap = new Color(0.5f, 0.5f, 1f);

	private TextureBlenderMaterialPropertyCacheHelper sourceMaterialPropertyCache = new TextureBlenderMaterialPropertyCacheHelper();

	private Color m_tintColor;

	private bool m_doScaleAlphaCutoff;

	private float m_alphaCutoff;

	private float m_glossiness;

	private float m_glossMapScale;

	private float m_metallic;

	private bool m_hasMetallicGlossMap;

	private float m_bumpScale;

	private bool m_shaderDoesEmission;

	private Color m_emissionColor;

	private Prop propertyToDo = Prop.doNone;

	private Color m_generatingTintedAtlasColor = Color.white;

	private float m_generatingTintedAtlasMetallic;

	private float m_generatingTintedAtlasGlossiness = 1f;

	private float m_generatingTintedAtlasGlossMapScale = 1f;

	private float m_generatingTintedAtlasBumpScale = 1f;

	private Color m_generatingTintedAtlasEmission = Color.white;

	private const float m_generatedAlphaCutoff = 0.5f;

	private Color m_notGeneratingAtlasDefaultColor = Color.white;

	private float m_notGeneratingAtlasDefaultMetallic;

	private float m_notGeneratingAtlasDefaultGlossiness = 0.5f;

	private Color m_notGeneratingAtlasDefaultEmisionColor = Color.black;

	public bool DoesShaderNameMatch(string shaderName)
	{
		if (!shaderName.Equals("Standard"))
		{
			return shaderName.EndsWith("StandardTextureArray");
		}
		return true;
	}

	public void OnBeforeTintTexture(Material sourceMat, string shaderTexturePropertyName)
	{
		if (shaderTexturePropertyName.Equals("_MainTex"))
		{
			propertyToDo = Prop.doColor;
			if (sourceMat.HasProperty("_Color"))
			{
				m_tintColor = sourceMat.GetColor("_Color");
			}
			else
			{
				m_tintColor = m_generatingTintedAtlasColor;
			}
			if (sourceMat.HasProperty("_Mode") && sourceMat.HasProperty("_Cutoff") && sourceMat.GetFloat("_Mode") == 1f)
			{
				m_doScaleAlphaCutoff = true;
				m_alphaCutoff = sourceMat.GetFloat("_Cutoff");
				m_alphaCutoff = Mathf.Clamp(m_alphaCutoff, 0.0001f, 0.9999f);
			}
			else
			{
				m_doScaleAlphaCutoff = false;
				m_alphaCutoff = 0.5f;
			}
		}
		else if (shaderTexturePropertyName.Equals("_MetallicGlossMap"))
		{
			propertyToDo = Prop.doMetallic;
			m_metallic = m_generatingTintedAtlasMetallic;
			if (sourceMat.GetTexture("_MetallicGlossMap") != null)
			{
				m_hasMetallicGlossMap = true;
			}
			else
			{
				m_hasMetallicGlossMap = false;
			}
			if (sourceMat.HasProperty("_Metallic"))
			{
				m_metallic = sourceMat.GetFloat("_Metallic");
			}
			else
			{
				m_metallic = 0f;
			}
			if (sourceMat.HasProperty("_GlossMapScale"))
			{
				m_glossMapScale = sourceMat.GetFloat("_GlossMapScale");
			}
			else
			{
				m_glossMapScale = 1f;
			}
			if (sourceMat.HasProperty("_Glossiness"))
			{
				m_glossiness = sourceMat.GetFloat("_Glossiness");
			}
			else
			{
				m_glossiness = 0f;
			}
		}
		else if (shaderTexturePropertyName.Equals("_BumpMap"))
		{
			propertyToDo = Prop.doBump;
			if (sourceMat.HasProperty(shaderTexturePropertyName))
			{
				if (sourceMat.HasProperty("_BumpScale"))
				{
					m_bumpScale = sourceMat.GetFloat("_BumpScale");
				}
			}
			else
			{
				m_bumpScale = m_generatingTintedAtlasBumpScale;
			}
		}
		else if (shaderTexturePropertyName.Equals("_EmissionMap"))
		{
			propertyToDo = Prop.doEmission;
			m_shaderDoesEmission = sourceMat.IsKeywordEnabled("_EMISSION");
			if (sourceMat.HasProperty("_EmissionColor"))
			{
				m_emissionColor = sourceMat.GetColor("_EmissionColor");
			}
			else
			{
				m_emissionColor = m_notGeneratingAtlasDefaultEmisionColor;
			}
		}
		else
		{
			propertyToDo = Prop.doNone;
		}
	}

	public Color OnBlendTexturePixel(string propertyToDoshaderPropertyName, Color pixelColor)
	{
		if (propertyToDo == Prop.doColor)
		{
			Color result = new Color(pixelColor.r * m_tintColor.r, pixelColor.g * m_tintColor.g, pixelColor.b * m_tintColor.b, pixelColor.a * m_tintColor.a);
			if (m_doScaleAlphaCutoff)
			{
				if (result.a >= m_alphaCutoff)
				{
					result.a = 0.5f + 0.5f * (result.a - m_alphaCutoff) / (1f - m_alphaCutoff);
				}
				else
				{
					result.a = 0.5f * result.a / m_alphaCutoff;
				}
			}
			return result;
		}
		if (propertyToDo == Prop.doMetallic)
		{
			if (m_hasMetallicGlossMap)
			{
				pixelColor = new Color(pixelColor.r, pixelColor.g, pixelColor.b, pixelColor.a * m_glossMapScale);
				return pixelColor;
			}
			return new Color(m_metallic, 0f, 0f, m_glossiness);
		}
		if (propertyToDo == Prop.doBump)
		{
			return Color.Lerp(NeutralNormalMap, pixelColor, m_bumpScale);
		}
		if (propertyToDo == Prop.doEmission)
		{
			if (m_shaderDoesEmission)
			{
				return new Color(pixelColor.r * m_emissionColor.r, pixelColor.g * m_emissionColor.g, pixelColor.b * m_emissionColor.b, pixelColor.a * m_emissionColor.a);
			}
			return Color.black;
		}
		return pixelColor;
	}

	public bool NonTexturePropertiesAreEqual(Material a, Material b)
	{
		if (!TextureBlenderFallback._compareColor(a, b, m_notGeneratingAtlasDefaultColor, "_Color"))
		{
			return false;
		}
		if (a.HasProperty("_Mode") && b.HasProperty("_Mode") && a.GetFloat("_Mode") == 1f && b.GetFloat("_Mode") == 1f && a.HasProperty("_Cutoff") && b.HasProperty("_Cutoff") && a.HasProperty("_Cutoff") != b.HasProperty("_Cutoff"))
		{
			return false;
		}
		if (!TextureBlenderFallback._compareFloat(a, b, m_notGeneratingAtlasDefaultGlossiness, "_Glossiness"))
		{
			return false;
		}
		bool flag = a.HasProperty("_MetallicGlossMap") && a.GetTexture("_MetallicGlossMap") != null;
		bool flag2 = b.HasProperty("_MetallicGlossMap") && b.GetTexture("_MetallicGlossMap") != null;
		if (flag && flag2)
		{
			if (!TextureBlenderFallback._compareFloat(a, b, m_notGeneratingAtlasDefaultMetallic, "_GlossMapScale"))
			{
				return false;
			}
		}
		else
		{
			if (flag || flag2)
			{
				return false;
			}
			if (!TextureBlenderFallback._compareFloat(a, b, m_notGeneratingAtlasDefaultMetallic, "_Metallic"))
			{
				return false;
			}
		}
		if (a.IsKeywordEnabled("_EMISSION") != b.IsKeywordEnabled("_EMISSION"))
		{
			return false;
		}
		if (a.IsKeywordEnabled("_EMISSION") && !TextureBlenderFallback._compareColor(a, b, m_notGeneratingAtlasDefaultEmisionColor, "_EmissionColor"))
		{
			return false;
		}
		return true;
	}

	public void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial)
	{
		if (resultMaterial.GetTexture("_MainTex") != null)
		{
			resultMaterial.SetColor("_Color", m_generatingTintedAtlasColor);
			if (resultMaterial.GetFloat("_Mode") == 1f)
			{
				resultMaterial.SetFloat("_Cutoff", 0.5f);
			}
		}
		else
		{
			resultMaterial.SetColor("_Color", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Color", m_notGeneratingAtlasDefaultColor));
		}
		if (resultMaterial.GetTexture("_MetallicGlossMap") != null)
		{
			resultMaterial.SetFloat("_Metallic", m_generatingTintedAtlasMetallic);
			resultMaterial.SetFloat("_GlossMapScale", m_generatingTintedAtlasGlossMapScale);
			resultMaterial.SetFloat("_Glossiness", m_generatingTintedAtlasGlossiness);
		}
		else
		{
			resultMaterial.SetFloat("_Metallic", (float)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Metallic", m_notGeneratingAtlasDefaultMetallic));
			resultMaterial.SetFloat("_Glossiness", (float)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Glossiness", m_notGeneratingAtlasDefaultGlossiness));
		}
		if (resultMaterial.GetTexture("_BumpMap") != null)
		{
			resultMaterial.SetFloat("_BumpScale", m_generatingTintedAtlasBumpScale);
		}
		if (resultMaterial.GetTexture("_EmissionMap") != null)
		{
			resultMaterial.EnableKeyword("_EMISSION");
			resultMaterial.SetColor("_EmissionColor", m_generatingTintedAtlasEmission);
		}
		else
		{
			resultMaterial.DisableKeyword("_EMISSION");
			resultMaterial.SetColor("_EmissionColor", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_EmissionColor", m_notGeneratingAtlasDefaultEmisionColor));
		}
	}

	public Color GetColorIfNoTexture(Material mat, ShaderTextureProperty texPropertyName)
	{
		if (texPropertyName.name.Equals("_BumpMap"))
		{
			return TextureBlenderFallback.GetDefaultNormalMapColor();
		}
		if (texPropertyName.name.Equals("_MainTex"))
		{
			if (mat != null && mat.HasProperty("_Color"))
			{
				return Color.white;
			}
		}
		else
		{
			if (texPropertyName.name.Equals("_MetallicGlossMap"))
			{
				if (mat != null && mat.HasProperty("_Metallic"))
				{
					try
					{
						float @float = mat.GetFloat("_Metallic");
						Color result = new Color(@float, @float, @float);
						if (mat.HasProperty("_Glossiness"))
						{
							try
							{
								result.a = mat.GetFloat("_Glossiness");
							}
							catch (Exception)
							{
							}
						}
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_Metallic", @float);
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_Glossiness", result.a);
						return result;
					}
					catch (Exception)
					{
					}
					return new Color(0f, 0f, 0f, 0.5f);
				}
				return new Color(0f, 0f, 0f, 0.5f);
			}
			if (texPropertyName.name.Equals("_ParallaxMap"))
			{
				return new Color(0f, 0f, 0f, 0f);
			}
			if (texPropertyName.name.Equals("_OcclusionMap"))
			{
				return new Color(1f, 1f, 1f, 1f);
			}
			if (texPropertyName.name.Equals("_EmissionMap"))
			{
				if (mat != null)
				{
					if (!mat.IsKeywordEnabled("_EMISSION"))
					{
						return Color.black;
					}
					if (!mat.HasProperty("_EmissionColor"))
					{
						return Color.black;
					}
					try
					{
						Color color = mat.GetColor("_EmissionColor");
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_EmissionColor", color);
						return color;
					}
					catch (Exception)
					{
					}
				}
			}
			else if (texPropertyName.name.Equals("_DetailMask"))
			{
				return new Color(0f, 0f, 0f, 0f);
			}
		}
		return new Color(1f, 1f, 1f, 0f);
	}
}
