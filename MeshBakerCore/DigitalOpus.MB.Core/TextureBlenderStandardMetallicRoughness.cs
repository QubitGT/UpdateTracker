using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class TextureBlenderStandardMetallicRoughness : TextureBlender
{
	private enum Prop
	{
		doColor,
		doMetallic,
		doRoughness,
		doEmission,
		doBump,
		doNone
	}

	private static Color NeutralNormalMap = new Color(0.5f, 0.5f, 1f);

	private TextureBlenderMaterialPropertyCacheHelper sourceMaterialPropertyCache = new TextureBlenderMaterialPropertyCacheHelper();

	private Color m_tintColor;

	private float m_roughness;

	private float m_metallic;

	private bool m_hasMetallicGlossMap;

	private bool m_hasSpecGlossMap;

	private float m_bumpScale;

	private bool m_shaderDoesEmission;

	private Color m_emissionColor;

	private Prop propertyToDo = Prop.doNone;

	private Color m_generatingTintedAtlasColor = Color.white;

	private float m_generatingTintedAtlasMetallic;

	private float m_generatingTintedAtlasRoughness = 0.5f;

	private float m_generatingTintedAtlasBumpScale = 1f;

	private Color m_generatingTintedAtlasEmission = Color.white;

	private Color m_notGeneratingAtlasDefaultColor = Color.white;

	private float m_notGeneratingAtlasDefaultMetallic;

	private float m_notGeneratingAtlasDefaultGlossiness = 0.5f;

	private Color m_notGeneratingAtlasDefaultEmisionColor = Color.black;

	public bool DoesShaderNameMatch(string shaderName)
	{
		return shaderName.Equals("Standard (Roughness setup)");
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
		}
		else if (shaderTexturePropertyName.Equals("_SpecGlossMap"))
		{
			propertyToDo = Prop.doRoughness;
			m_roughness = m_generatingTintedAtlasRoughness;
			if (sourceMat.GetTexture("_SpecGlossMap") != null)
			{
				m_hasSpecGlossMap = true;
			}
			else
			{
				m_hasSpecGlossMap = false;
			}
			if (sourceMat.HasProperty("_Glossiness"))
			{
				m_roughness = sourceMat.GetFloat("_Glossiness");
			}
			else
			{
				m_roughness = 1f;
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
			return new Color(pixelColor.r * m_tintColor.r, pixelColor.g * m_tintColor.g, pixelColor.b * m_tintColor.b, pixelColor.a * m_tintColor.a);
		}
		if (propertyToDo == Prop.doMetallic)
		{
			if (m_hasMetallicGlossMap)
			{
				return pixelColor;
			}
			return new Color(m_metallic, 0f, 0f, m_roughness);
		}
		if (propertyToDo == Prop.doRoughness)
		{
			if (m_hasSpecGlossMap)
			{
				return pixelColor;
			}
			return new Color(m_roughness, 0f, 0f, 0f);
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
		bool num = a.HasProperty("_MetallicGlossMap") && a.GetTexture("_MetallicGlossMap") != null;
		bool flag = b.HasProperty("_MetallicGlossMap") && b.GetTexture("_MetallicGlossMap") != null;
		if (!num && !flag)
		{
			if (!TextureBlenderFallback._compareFloat(a, b, m_notGeneratingAtlasDefaultMetallic, "_Metallic"))
			{
				return false;
			}
			bool num2 = a.HasProperty("_SpecGlossMap") && a.GetTexture("_SpecGlossMap") != null;
			bool flag2 = b.HasProperty("_SpecGlossMap") && b.GetTexture("_SpecGlossMap") != null;
			if (!num2 && !flag2)
			{
				if (!TextureBlenderFallback._compareFloat(a, b, m_generatingTintedAtlasRoughness, "_Glossiness"))
				{
					return false;
				}
				if (!TextureBlenderFallback._compareFloat(a, b, m_generatingTintedAtlasBumpScale, "_bumpScale"))
				{
					return false;
				}
				if (!TextureBlenderFallback._compareFloat(a, b, m_generatingTintedAtlasRoughness, "_Glossiness"))
				{
					return false;
				}
				if (a.IsKeywordEnabled("_EMISSION") != b.IsKeywordEnabled("_EMISSION"))
				{
					return false;
				}
				if (a.IsKeywordEnabled("_EMISSION") && !TextureBlenderFallback._compareColor(a, b, m_generatingTintedAtlasEmission, "_EmissionColor"))
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial)
	{
		if (resultMaterial.GetTexture("_MainTex") != null)
		{
			resultMaterial.SetColor("_Color", m_generatingTintedAtlasColor);
		}
		else
		{
			resultMaterial.SetColor("_Color", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Color", m_notGeneratingAtlasDefaultColor));
		}
		if (resultMaterial.GetTexture("_MetallicGlossMap") != null)
		{
			resultMaterial.SetFloat("_Metallic", m_generatingTintedAtlasMetallic);
		}
		else
		{
			resultMaterial.SetFloat("_Metallic", (float)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Metallic", m_notGeneratingAtlasDefaultMetallic));
		}
		if (!(resultMaterial.GetTexture("_SpecGlossMap") != null))
		{
			resultMaterial.SetFloat("_Glossiness", (float)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Glossiness", m_notGeneratingAtlasDefaultGlossiness));
		}
		if (resultMaterial.GetTexture("_BumpMap") != null)
		{
			resultMaterial.SetFloat("_BumpScale", m_generatingTintedAtlasBumpScale);
		}
		else
		{
			resultMaterial.SetFloat("_BumpScale", m_generatingTintedAtlasBumpScale);
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
			if (texPropertyName.name.Equals("_MetallicGlossMap"))
			{
				if (mat != null && mat.HasProperty("_Metallic"))
				{
					try
					{
						float @float = mat.GetFloat("_Metallic");
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_Metallic", @float);
					}
					catch (Exception)
					{
					}
					return new Color(0f, 0f, 0f, 0.5f);
				}
				return new Color(0f, 0f, 0f, 0.5f);
			}
			if (texPropertyName.name.Equals("_SpecGlossMap"))
			{
				bool flag = false;
				try
				{
					Color color = new Color(0f, 0f, 0f, 0.5f);
					if (mat.HasProperty("_Glossiness"))
					{
						try
						{
							flag = true;
							color.a = mat.GetFloat("_Glossiness");
						}
						catch (Exception)
						{
						}
					}
					sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_Glossiness", color.a);
					return new Color(0f, 0f, 0f, 0.5f);
				}
				catch (Exception)
				{
				}
				if (!flag)
				{
					return new Color(0f, 0f, 0f, 0.5f);
				}
			}
			else
			{
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
		}
		return new Color(1f, 1f, 1f, 0f);
	}
}
