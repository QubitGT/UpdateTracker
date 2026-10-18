using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class TextureBlenderStandardSpecular : TextureBlender
{
	private enum Prop
	{
		doColor,
		doSpecular,
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

	private float m_SpecGlossMapScale;

	private Color m_specColor;

	private bool m_hasSpecGlossMap;

	private float m_bumpScale;

	private bool m_shaderDoesEmission;

	private Color m_emissionColor;

	private Prop propertyToDo = Prop.doNone;

	private Color m_generatingTintedAtlaColor = Color.white;

	private Color m_generatingTintedAtlaSpecular = Color.black;

	private float m_generatingTintedAtlaGlossiness = 1f;

	private float m_generatingTintedAtlaSpecGlossMapScale = 1f;

	private float m_generatingTintedAtlaBumpScale = 1f;

	private Color m_generatingTintedAtlaEmission = Color.white;

	private const float m_generatedAlphaCutoff = 0.5f;

	private Color m_notGeneratingAtlasDefaultColor = Color.white;

	private Color m_notGeneratingAtlasDefaultSpecularColor = new Color(0f, 0f, 0f, 1f);

	private float m_notGeneratingAtlasDefaultGlossiness = 0.5f;

	private Color m_notGeneratingAtlasDefaultEmisionColor = Color.black;

	public bool DoesShaderNameMatch(string shaderName)
	{
		return shaderName.Equals("Standard (Specular setup)");
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
				m_tintColor = m_generatingTintedAtlaColor;
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
		else if (shaderTexturePropertyName.Equals("_SpecGlossMap"))
		{
			propertyToDo = Prop.doSpecular;
			m_specColor = m_generatingTintedAtlaSpecular;
			if (sourceMat.GetTexture("_SpecGlossMap") != null)
			{
				m_hasSpecGlossMap = true;
			}
			else
			{
				m_hasSpecGlossMap = false;
			}
			if (sourceMat.HasProperty("_SpecColor"))
			{
				m_specColor = sourceMat.GetColor("_SpecColor");
			}
			else
			{
				m_specColor = new Color(0f, 0f, 0f, 1f);
			}
			if (sourceMat.HasProperty("_GlossMapScale"))
			{
				m_SpecGlossMapScale = sourceMat.GetFloat("_GlossMapScale");
			}
			else
			{
				m_SpecGlossMapScale = 1f;
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
				m_bumpScale = m_generatingTintedAtlaBumpScale;
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
				m_generatingTintedAtlaColor = m_notGeneratingAtlasDefaultEmisionColor;
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
		if (propertyToDo == Prop.doSpecular)
		{
			if (m_hasSpecGlossMap)
			{
				pixelColor = new Color(pixelColor.r, pixelColor.g, pixelColor.b, pixelColor.a * m_SpecGlossMapScale);
				return pixelColor;
			}
			Color specColor = m_specColor;
			specColor.a = m_glossiness;
			return specColor;
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
		if (!TextureBlenderFallback._compareColor(a, b, m_generatingTintedAtlaColor, "_Color"))
		{
			return false;
		}
		if (!TextureBlenderFallback._compareColor(a, b, m_generatingTintedAtlaSpecular, "_SpecColor"))
		{
			return false;
		}
		if (a.HasProperty("_Mode") && b.HasProperty("_Mode") && a.GetFloat("_Mode") == 1f && b.GetFloat("_Mode") == 1f && a.HasProperty("_Cutoff") && b.HasProperty("_Cutoff") && a.HasProperty("_Cutoff") != b.HasProperty("_Cutoff"))
		{
			return false;
		}
		bool flag = a.HasProperty("_SpecGlossMap") && a.GetTexture("_SpecGlossMap") != null;
		bool flag2 = b.HasProperty("_SpecGlossMap") && b.GetTexture("_SpecGlossMap") != null;
		if (flag && flag2)
		{
			if (!TextureBlenderFallback._compareFloat(a, b, m_generatingTintedAtlaSpecGlossMapScale, "_GlossMapScale"))
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
			if (!TextureBlenderFallback._compareFloat(a, b, m_generatingTintedAtlaGlossiness, "_Glossiness"))
			{
				return false;
			}
		}
		if (!TextureBlenderFallback._compareFloat(a, b, m_generatingTintedAtlaBumpScale, "_BumpScale"))
		{
			return false;
		}
		if (a.IsKeywordEnabled("_EMISSION") != b.IsKeywordEnabled("_EMISSION"))
		{
			return false;
		}
		if (a.IsKeywordEnabled("_EMISSION") && !TextureBlenderFallback._compareColor(a, b, m_generatingTintedAtlaEmission, "_EmissionColor"))
		{
			return false;
		}
		return true;
	}

	public void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial)
	{
		if (resultMaterial.GetTexture("_MainTex") != null)
		{
			resultMaterial.SetColor("_Color", m_generatingTintedAtlaColor);
			if (resultMaterial.GetFloat("_Mode") == 1f)
			{
				resultMaterial.SetFloat("_Cutoff", 0.5f);
			}
		}
		else
		{
			resultMaterial.SetColor("_Color", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Color", m_notGeneratingAtlasDefaultColor));
		}
		if (resultMaterial.GetTexture("_SpecGlossMap") != null)
		{
			resultMaterial.SetColor("_SpecColor", m_generatingTintedAtlaSpecular);
			resultMaterial.SetFloat("_GlossMapScale", m_generatingTintedAtlaSpecGlossMapScale);
			resultMaterial.SetFloat("_Glossiness", m_generatingTintedAtlaGlossiness);
		}
		else
		{
			resultMaterial.SetColor("_SpecColor", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_SpecColor", m_notGeneratingAtlasDefaultSpecularColor));
			resultMaterial.SetFloat("_Glossiness", (float)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Glossiness", m_notGeneratingAtlasDefaultGlossiness));
		}
		if (resultMaterial.GetTexture("_BumpMap") != null)
		{
			resultMaterial.SetFloat("_BumpScale", m_generatingTintedAtlaBumpScale);
		}
		else
		{
			resultMaterial.SetFloat("_BumpScale", m_generatingTintedAtlaBumpScale);
		}
		if (resultMaterial.GetTexture("_EmissionMap") != null)
		{
			resultMaterial.EnableKeyword("_EMISSION");
			resultMaterial.SetColor("_EmissionColor", Color.white);
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
			if (texPropertyName.name.Equals("_SpecGlossMap"))
			{
				if (mat != null && mat.HasProperty("_SpecColor"))
				{
					try
					{
						Color color = mat.GetColor("_SpecColor");
						if (mat.HasProperty("_Glossiness"))
						{
							try
							{
								color.a = mat.GetFloat("_Glossiness");
							}
							catch (Exception)
							{
							}
						}
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_SpecColor", color);
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_Glossiness", color.a);
						return color;
					}
					catch (Exception)
					{
					}
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
						Color color2 = mat.GetColor("_EmissionColor");
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_EmissionColor", color2);
						return color2;
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
