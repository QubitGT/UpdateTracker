using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class TextureBlenderHDRPLit : TextureBlender
{
	private enum Prop
	{
		doColor,
		doMask,
		doSpecular,
		doEmission,
		doNone
	}

	private enum MaterialType
	{
		unknown,
		subsurfaceScattering,
		standard,
		anisotropy,
		iridescence,
		specularColor,
		translucent
	}

	private TextureBlenderMaterialPropertyCacheHelper sourceMaterialPropertyCache = new TextureBlenderMaterialPropertyCacheHelper();

	private MaterialType m_materialType;

	private Color m_tintColor;

	private bool m_hasMaskMap;

	private float m_smoothness;

	private float m_metallic;

	private bool m_hasSpecMap;

	private Color m_specularColor;

	private Color m_emissiveColor;

	private Prop propertyToDo = Prop.doNone;

	private Color m_generatingTintedAtlaColor = Color.white;

	private Color m_generatingTintedAtlaSpecular = Color.white;

	private Color m_generatingTintedAtlaEmission = Color.white;

	private Color m_notGeneratingAtlasDefaultColor = Color.white;

	private float m_notGeneratingAtlasDefaultMetallic;

	private float m_notGeneratingAtlasDefaultSmoothness = 0.5f;

	private Color m_notGeneratingAtlasDefaultSpecular = Color.white;

	private Color m_notGeneratingAtlasDefaultEmissiveColor = Color.black;

	public bool DoesShaderNameMatch(string shaderName)
	{
		return shaderName.Equals("HDRP/Lit");
	}

	private MaterialType _MapFloatToMaterialType(float materialType)
	{
		if (materialType == 0f)
		{
			return MaterialType.subsurfaceScattering;
		}
		if (materialType == 1f)
		{
			return MaterialType.standard;
		}
		if (materialType == 2f)
		{
			return MaterialType.anisotropy;
		}
		if (materialType == 3f)
		{
			return MaterialType.iridescence;
		}
		if (materialType == 4f)
		{
			return MaterialType.specularColor;
		}
		if (materialType == 5f)
		{
			return MaterialType.translucent;
		}
		return MaterialType.unknown;
	}

	private float _MapMaterialTypeToFloat(MaterialType materialType)
	{
		return materialType switch
		{
			MaterialType.subsurfaceScattering => 0f, 
			MaterialType.standard => 1f, 
			MaterialType.anisotropy => 2f, 
			MaterialType.iridescence => 3f, 
			MaterialType.specularColor => 4f, 
			MaterialType.translucent => 5f, 
			_ => -1f, 
		};
	}

	public void OnBeforeTintTexture(Material sourceMat, string shaderTexturePropertyName)
	{
		if (m_materialType == MaterialType.unknown)
		{
			if (sourceMat.HasProperty("_MaterialID"))
			{
				m_materialType = _MapFloatToMaterialType(sourceMat.GetFloat("_MaterialID"));
			}
		}
		else if (sourceMat.HasProperty("_MaterialID") && _MapFloatToMaterialType(sourceMat.GetFloat("_MaterialID")) != m_materialType)
		{
			Debug.LogError("Using the High Definition Render Pipeline TextureBlender to blend non-texture-properties. Some of the source materials use different 'MaterialType'. These  cannot be blended properly. Results will be unpredictable.");
		}
		if (shaderTexturePropertyName.Equals("_BaseColorMap"))
		{
			propertyToDo = Prop.doColor;
			if (sourceMat.HasProperty("_BaseColor"))
			{
				m_tintColor = sourceMat.GetColor("_BaseColor");
			}
			else
			{
				m_tintColor = m_notGeneratingAtlasDefaultColor;
			}
		}
		else if (shaderTexturePropertyName.Equals("_MaskMap"))
		{
			propertyToDo = Prop.doMask;
			if (sourceMat.HasProperty("_MaskMap") && sourceMat.GetTexture("_MaskMap") != null)
			{
				m_hasMaskMap = true;
				return;
			}
			m_hasMaskMap = false;
			if (m_materialType == MaterialType.standard && sourceMat.HasProperty("_Metallic"))
			{
				m_metallic = sourceMat.GetFloat("_Metallic");
			}
			else
			{
				m_metallic = m_notGeneratingAtlasDefaultMetallic;
			}
			if (sourceMat.HasProperty("_Smoothness"))
			{
				m_smoothness = sourceMat.GetFloat("_Smoothness");
			}
			else
			{
				m_smoothness = m_notGeneratingAtlasDefaultSmoothness;
			}
		}
		else if (shaderTexturePropertyName.Equals("_SpecularColorMap") && m_materialType == MaterialType.specularColor)
		{
			propertyToDo = Prop.doSpecular;
			if (sourceMat.HasProperty("_SpecularColorMap") && sourceMat.GetTexture("_SpecularColorMap") != null)
			{
				m_hasSpecMap = true;
			}
			else
			{
				m_hasSpecMap = false;
			}
			if (sourceMat.HasProperty("_SpecularColor"))
			{
				m_specularColor = sourceMat.GetColor("_SpecularColor");
			}
		}
		else if (shaderTexturePropertyName.Equals("_EmissiveColorMap"))
		{
			propertyToDo = Prop.doEmission;
			if (sourceMat.HasProperty("_EmissiveColor"))
			{
				m_emissiveColor = sourceMat.GetColor("_EmissiveColor");
			}
			else
			{
				m_emissiveColor = m_notGeneratingAtlasDefaultEmissiveColor;
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
		if (propertyToDo == Prop.doMask)
		{
			if (m_hasMaskMap)
			{
				return new Color(pixelColor.r * m_metallic, pixelColor.g, pixelColor.b, pixelColor.a * m_smoothness);
			}
			return new Color(m_metallic, 0f, 0f, m_smoothness);
		}
		if (propertyToDo == Prop.doSpecular)
		{
			if (m_hasSpecMap)
			{
				return new Color(pixelColor.r * m_specularColor.r, pixelColor.g * m_specularColor.g, pixelColor.b * m_specularColor.g, pixelColor.a * m_specularColor.a);
			}
			return m_specularColor;
		}
		if (propertyToDo == Prop.doEmission)
		{
			return new Color(pixelColor.r * m_emissiveColor.r, pixelColor.g * m_emissiveColor.g, pixelColor.b * m_emissiveColor.b, pixelColor.a * m_emissiveColor.a);
		}
		return pixelColor;
	}

	public bool NonTexturePropertiesAreEqual(Material a, Material b)
	{
		if (!TextureBlenderFallback._compareColor(a, b, m_generatingTintedAtlaColor, "_BaseColor"))
		{
			return false;
		}
		bool num = a.HasProperty("_MaskMap") && a.GetTexture("_MaskMap") != null;
		bool flag = b.HasProperty("_MaskMap") && b.GetTexture("_MaskMap") != null;
		if (!num && !flag && !TextureBlenderFallback._compareFloat(a, b, m_notGeneratingAtlasDefaultMetallic, "_Metallic") && !TextureBlenderFallback._compareFloat(a, b, m_notGeneratingAtlasDefaultSmoothness, "_Smoothness"))
		{
			return false;
		}
		if (m_materialType == MaterialType.specularColor && !TextureBlenderFallback._compareColor(a, b, m_notGeneratingAtlasDefaultSpecular, "_SpecularColor"))
		{
			return false;
		}
		if (!TextureBlenderFallback._compareColor(a, b, m_notGeneratingAtlasDefaultEmissiveColor, "_EmissiveColor"))
		{
			return false;
		}
		return true;
	}

	public void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial)
	{
		if (m_materialType != 0)
		{
			resultMaterial.SetFloat("_MaterialID", _MapMaterialTypeToFloat(m_materialType));
		}
		if (resultMaterial.GetTexture("_BaseColorMap") != null)
		{
			resultMaterial.SetColor("_BaseColor", m_generatingTintedAtlaColor);
		}
		else
		{
			resultMaterial.SetColor("_BaseColor", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_BaseColor", m_notGeneratingAtlasDefaultColor));
		}
		if (!(resultMaterial.GetTexture("_MaskMap") != null))
		{
			resultMaterial.SetFloat("_Metallic", (float)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_Metallic", m_notGeneratingAtlasDefaultMetallic));
			resultMaterial.SetFloat("_Smoothness", m_notGeneratingAtlasDefaultSmoothness);
		}
		if (m_materialType == MaterialType.specularColor)
		{
			if (resultMaterial.GetTexture("_SpecularColorMap") != null)
			{
				resultMaterial.SetColor("_SpecularColor", m_generatingTintedAtlaSpecular);
				resultMaterial.SetFloat("_AORemapMin", 1f);
			}
			else
			{
				resultMaterial.SetColor("_SpecularColor", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_SpecularColor", m_notGeneratingAtlasDefaultSpecular));
				resultMaterial.SetFloat("_AORemapMin", 1f);
			}
		}
		if (resultMaterial.GetTexture("_EmissiveColorMap") != null)
		{
			resultMaterial.SetColor("_EmissiveColor", m_generatingTintedAtlaEmission);
		}
		else
		{
			resultMaterial.SetColor("_EmissiveColor", (Color)sourceMaterialPropertyCache.GetValueIfAllSourceAreTheSameOrDefault("_EmissiveColor", m_notGeneratingAtlasDefaultEmissiveColor));
		}
	}

	public Color GetColorIfNoTexture(Material mat, ShaderTextureProperty texPropertyName)
	{
		if (texPropertyName.name.Equals("_BaseColorMap"))
		{
			if (mat != null && mat.HasProperty("_BaseColor"))
			{
				return m_notGeneratingAtlasDefaultColor;
			}
		}
		else
		{
			if (texPropertyName.name.Equals("_BumpMap"))
			{
				return TextureBlenderFallback.GetDefaultNormalMapColor();
			}
			if (texPropertyName.name.Equals("_Metallic"))
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
					return new Color(0f, 0f, 0f, m_notGeneratingAtlasDefaultSmoothness);
				}
				return new Color(0f, 0f, 0f, m_notGeneratingAtlasDefaultSmoothness);
			}
			if (texPropertyName.name.Equals("_Smoothness"))
			{
				if (mat != null && mat.HasProperty("_Smoothness"))
				{
					try
					{
						float float2 = mat.GetFloat("_Smoothness");
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_Smoothness", float2);
					}
					catch (Exception)
					{
					}
					return new Color(0f, 0f, 0f, m_notGeneratingAtlasDefaultSmoothness);
				}
				return new Color(0f, 0f, 0f, m_notGeneratingAtlasDefaultSmoothness);
			}
			if (texPropertyName.name.Equals("_SpecularColorMap"))
			{
				if (mat != null && mat.HasProperty("_SpecularColor"))
				{
					try
					{
						Color color = mat.GetColor("_SpecularColor");
						sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_SpecularColor", color);
					}
					catch (Exception)
					{
					}
				}
				return m_notGeneratingAtlasDefaultSpecular;
			}
			if (texPropertyName.name.Equals("_EmissiveColorMap") && mat != null)
			{
				if (!mat.HasProperty("_EmissiveColor"))
				{
					return m_notGeneratingAtlasDefaultEmissiveColor;
				}
				try
				{
					Color color2 = mat.GetColor("_EmissiveColor");
					sourceMaterialPropertyCache.CacheMaterialProperty(mat, "_EmissiveColor", color2);
				}
				catch (Exception)
				{
				}
			}
		}
		return new Color(1f, 1f, 1f, 0f);
	}
}
