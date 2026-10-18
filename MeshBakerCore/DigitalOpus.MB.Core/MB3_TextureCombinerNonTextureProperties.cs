using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_TextureCombinerNonTextureProperties
{
	public interface MaterialProperty
	{
		string PropertyName { get; set; }

		MaterialPropertyValueAveraged GetAverageCalculator();

		object GetDefaultValue();
	}

	public class MaterialPropertyFloat : MaterialProperty
	{
		private MaterialPropertyValueAveragedFloat _averageCalc;

		private float _defaultValue;

		public string PropertyName { get; set; }

		public MaterialPropertyFloat(string name, float defValue)
		{
			_averageCalc = new MaterialPropertyValueAveragedFloat();
			_defaultValue = defValue;
			PropertyName = name;
		}

		public MaterialPropertyValueAveraged GetAverageCalculator()
		{
			return _averageCalc;
		}

		public object GetDefaultValue()
		{
			return _defaultValue;
		}
	}

	public class MaterialPropertyColor : MaterialProperty
	{
		private MaterialPropertyValueAveragedColor _averageCalc;

		private Color _defaultValue;

		public string PropertyName { get; set; }

		public MaterialPropertyColor(string name, Color defaultVal)
		{
			_averageCalc = new MaterialPropertyValueAveragedColor();
			_defaultValue = defaultVal;
			PropertyName = name;
		}

		public MaterialPropertyValueAveraged GetAverageCalculator()
		{
			return _averageCalc;
		}

		public object GetDefaultValue()
		{
			return _defaultValue;
		}
	}

	public interface MaterialPropertyValueAveraged
	{
		void TryGetPropValueFromMaterialAndBlendIntoAverage(Material mat, MaterialProperty property);

		object GetAverage();

		int NumValues();

		void SetAverageValueOrDefaultOnMaterial(Material mat, MaterialProperty property);
	}

	public class MaterialPropertyValueAveragedFloat : MaterialPropertyValueAveraged
	{
		public float averageVal;

		public int numValues;

		public void TryGetPropValueFromMaterialAndBlendIntoAverage(Material mat, MaterialProperty property)
		{
			if (mat.HasProperty(property.PropertyName))
			{
				float @float = mat.GetFloat(property.PropertyName);
				averageVal = averageVal * (float)numValues / (float)(numValues + 1) + @float / (float)(numValues + 1);
				numValues++;
			}
		}

		public object GetAverage()
		{
			return averageVal;
		}

		public int NumValues()
		{
			return numValues;
		}

		public void SetAverageValueOrDefaultOnMaterial(Material mat, MaterialProperty property)
		{
			if (mat.HasProperty(property.PropertyName))
			{
				if (numValues > 0)
				{
					mat.SetFloat(property.PropertyName, averageVal);
				}
				else
				{
					mat.SetFloat(property.PropertyName, (float)property.GetDefaultValue());
				}
			}
		}
	}

	public class MaterialPropertyValueAveragedColor : MaterialPropertyValueAveraged
	{
		public Color averageVal;

		public int numValues;

		public void TryGetPropValueFromMaterialAndBlendIntoAverage(Material mat, MaterialProperty property)
		{
			if (mat.HasProperty(property.PropertyName))
			{
				Color color = mat.GetColor(property.PropertyName);
				averageVal = averageVal * numValues / (numValues + 1) + color / (numValues + 1);
				numValues++;
			}
		}

		public object GetAverage()
		{
			return averageVal;
		}

		public int NumValues()
		{
			return numValues;
		}

		public void SetAverageValueOrDefaultOnMaterial(Material mat, MaterialProperty property)
		{
			if (mat.HasProperty(property.PropertyName))
			{
				if (numValues > 0)
				{
					mat.SetColor(property.PropertyName, averageVal);
				}
				else
				{
					mat.SetColor(property.PropertyName, (Color)property.GetDefaultValue());
				}
			}
		}
	}

	public struct TexPropertyNameColorPair
	{
		public string name;

		public Color color;

		public TexPropertyNameColorPair(string nm, Color col)
		{
			name = nm;
			color = col;
		}
	}

	private interface NonTextureProperties
	{
		bool NonTexturePropertiesAreEqual(Material a, Material b);

		Texture2D TintTextureWithTextureCombiner(Texture2D t, MB_TexSet sourceMaterial, ShaderTextureProperty shaderPropertyName);

		void AdjustNonTextureProperties(Material resultMat, List<ShaderTextureProperty> texPropertyNames, MB2_EditorMethodsInterface editorMethods);

		Color GetColorForTemporaryTexture(Material matIfBlender, ShaderTextureProperty texProperty);

		Color GetColorAsItWouldAppearInAtlasIfNoTexture(Material matIfBlender, ShaderTextureProperty texProperty);
	}

	private class NonTexturePropertiesDontBlendProps : NonTextureProperties
	{
		private MB3_TextureCombinerNonTextureProperties _textureProperties;

		public NonTexturePropertiesDontBlendProps(MB3_TextureCombinerNonTextureProperties textureProperties)
		{
			_textureProperties = textureProperties;
		}

		public bool NonTexturePropertiesAreEqual(Material a, Material b)
		{
			return true;
		}

		public Texture2D TintTextureWithTextureCombiner(Texture2D t, MB_TexSet sourceMaterial, ShaderTextureProperty shaderPropertyName)
		{
			Debug.LogError("TintTextureWithTextureCombiner should never be called if resultMaterialTextureBlender is null");
			return t;
		}

		public void AdjustNonTextureProperties(Material resultMat, List<ShaderTextureProperty> texPropertyNames, MB2_EditorMethodsInterface editorMethods)
		{
			if (resultMat == null || texPropertyNames == null)
			{
				return;
			}
			for (int i = 0; i < _textureProperties._nonTextureProperties.Length; i++)
			{
				MaterialProperty materialProperty = _textureProperties._nonTextureProperties[i];
				if (resultMat.HasProperty(materialProperty.PropertyName))
				{
					materialProperty.GetAverageCalculator().SetAverageValueOrDefaultOnMaterial(resultMat, materialProperty);
				}
			}
			editorMethods?.CommitChangesToAssets();
		}

		public Color GetColorAsItWouldAppearInAtlasIfNoTexture(Material matIfBlender, ShaderTextureProperty texProperty)
		{
			return Color.white;
		}

		public Color GetColorForTemporaryTexture(Material matIfBlender, ShaderTextureProperty texProperty)
		{
			if (texProperty.isNormalMap)
			{
				if (MBVersion.IsSwizzledNormalMapPlatform())
				{
					return MB3_TextureCombiner.NEUTRAL_NORMAL_MAP_COLOR_SWIZZLED;
				}
				return MB3_TextureCombiner.NEUTRAL_NORMAL_MAP_COLOR_NON_SWIZZLED;
			}
			if (_textureProperties.textureProperty2DefaultColorMap.ContainsKey(texProperty.name))
			{
				return _textureProperties.textureProperty2DefaultColorMap[texProperty.name];
			}
			return new Color(1f, 1f, 1f, 0f);
		}
	}

	private class NonTexturePropertiesBlendProps : NonTextureProperties
	{
		private MB3_TextureCombinerNonTextureProperties _textureProperties;

		private TextureBlender resultMaterialTextureBlender;

		public NonTexturePropertiesBlendProps(MB3_TextureCombinerNonTextureProperties textureProperties, TextureBlender resultMats)
		{
			resultMaterialTextureBlender = resultMats;
			_textureProperties = textureProperties;
		}

		public bool NonTexturePropertiesAreEqual(Material a, Material b)
		{
			return resultMaterialTextureBlender.NonTexturePropertiesAreEqual(a, b);
		}

		public Texture2D TintTextureWithTextureCombiner(Texture2D t, MB_TexSet sourceMaterial, ShaderTextureProperty shaderPropertyName)
		{
			resultMaterialTextureBlender.OnBeforeTintTexture(sourceMaterial.matsAndGOs.mats[0].mat, shaderPropertyName.name);
			if (_textureProperties.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				Debug.Log($"Blending texture {t.name} mat {sourceMaterial.matsAndGOs.mats[0].mat} with non-texture properties using TextureBlender {resultMaterialTextureBlender}");
			}
			for (int i = 0; i < t.height; i++)
			{
				Color[] pixels = t.GetPixels(0, i, t.width, 1);
				for (int j = 0; j < pixels.Length; j++)
				{
					pixels[j] = resultMaterialTextureBlender.OnBlendTexturePixel(shaderPropertyName.name, pixels[j]);
				}
				t.SetPixels(0, i, t.width, 1, pixels);
			}
			t.Apply();
			return t;
		}

		public void AdjustNonTextureProperties(Material resultMat, List<ShaderTextureProperty> texPropertyNames, MB2_EditorMethodsInterface editorMethods)
		{
			if (!(resultMat == null) && texPropertyNames != null)
			{
				if (_textureProperties.LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("Adjusting non texture properties using TextureBlender for shader: " + resultMat.shader.name);
				}
				resultMaterialTextureBlender.SetNonTexturePropertyValuesOnResultMaterial(resultMat);
				editorMethods?.CommitChangesToAssets();
			}
		}

		public Color GetColorAsItWouldAppearInAtlasIfNoTexture(Material matIfBlender, ShaderTextureProperty texProperty)
		{
			resultMaterialTextureBlender.OnBeforeTintTexture(matIfBlender, texProperty.name);
			Color colorForTemporaryTexture = GetColorForTemporaryTexture(matIfBlender, texProperty);
			return resultMaterialTextureBlender.OnBlendTexturePixel(texProperty.name, colorForTemporaryTexture);
		}

		public Color GetColorForTemporaryTexture(Material matIfBlender, ShaderTextureProperty texProperty)
		{
			return resultMaterialTextureBlender.GetColorIfNoTexture(matIfBlender, texProperty);
		}
	}

	private TexPropertyNameColorPair[] defaultTextureProperty2DefaultColorMap = new TexPropertyNameColorPair[6]
	{
		new TexPropertyNameColorPair("_MainTex", new Color(1f, 1f, 1f, 0f)),
		new TexPropertyNameColorPair("_MetallicGlossMap", new Color(0f, 0f, 0f, 1f)),
		new TexPropertyNameColorPair("_ParallaxMap", new Color(0f, 0f, 0f, 0f)),
		new TexPropertyNameColorPair("_OcclusionMap", new Color(1f, 1f, 1f, 1f)),
		new TexPropertyNameColorPair("_EmissionMap", new Color(0f, 0f, 0f, 0f)),
		new TexPropertyNameColorPair("_DetailMask", new Color(0f, 0f, 0f, 0f))
	};

	private MaterialProperty[] _nonTextureProperties = new MaterialProperty[8]
	{
		new MaterialPropertyColor("_Color", Color.white),
		new MaterialPropertyFloat("_Glossiness", 0.5f),
		new MaterialPropertyFloat("_GlossMapScale", 1f),
		new MaterialPropertyFloat("_Metallic", 0f),
		new MaterialPropertyFloat("_BumpScale", 0.1f),
		new MaterialPropertyFloat("_Parallax", 0.02f),
		new MaterialPropertyFloat("_OcclusionStrength", 1f),
		new MaterialPropertyColor("_EmissionColor", Color.black)
	};

	private MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	private bool _considerNonTextureProperties;

	private TextureBlender resultMaterialTextureBlender;

	private TextureBlender[] textureBlenders = new TextureBlender[0];

	private Dictionary<string, Color> textureProperty2DefaultColorMap = new Dictionary<string, Color>();

	private NonTextureProperties _nonTexturePropertiesBlender;

	public MB3_TextureCombinerNonTextureProperties(MB2_LogLevel ll, bool considerNonTextureProps)
	{
		_considerNonTextureProperties = considerNonTextureProps;
		textureProperty2DefaultColorMap = new Dictionary<string, Color>();
		for (int i = 0; i < defaultTextureProperty2DefaultColorMap.Length; i++)
		{
			textureProperty2DefaultColorMap.Add(defaultTextureProperty2DefaultColorMap[i].name, defaultTextureProperty2DefaultColorMap[i].color);
			_nonTexturePropertiesBlender = new NonTexturePropertiesDontBlendProps(this);
		}
	}

	internal void CollectAverageValuesOfNonTextureProperties(Material resultMaterial, Material mat)
	{
		for (int i = 0; i < _nonTextureProperties.Length; i++)
		{
			MaterialProperty materialProperty = _nonTextureProperties[i];
			if (resultMaterial.HasProperty(materialProperty.PropertyName))
			{
				materialProperty.GetAverageCalculator().TryGetPropValueFromMaterialAndBlendIntoAverage(mat, materialProperty);
			}
		}
	}

	internal void LoadTextureBlendersIfNeeded(Material resultMaterial)
	{
		if (_considerNonTextureProperties)
		{
			LoadTextureBlenders();
			FindBestTextureBlender(resultMaterial);
		}
	}

	private static bool InterfaceFilter(Type typeObj, object criteriaObj)
	{
		return typeObj.ToString() == criteriaObj.ToString();
	}

	private void FindBestTextureBlender(Material resultMaterial)
	{
		resultMaterialTextureBlender = FindMatchingTextureBlender(resultMaterial.shader.name);
		if (resultMaterialTextureBlender != null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Using Consider Non-Texture Properties found a TextureBlender for result material. Using: " + resultMaterialTextureBlender);
			}
		}
		else
		{
			resultMaterialTextureBlender = new TextureBlenderFallback();
		}
		if (resultMaterialTextureBlender is TextureBlenderFallback && LOG_LEVEL >= MB2_LogLevel.error)
		{
			Debug.LogWarning("Using _considerNonTextureProperties could not find a TextureBlender that matches the shader on the result material (" + resultMaterial.shader.name + "). Using the Fallback Texture Blender.");
		}
		_nonTexturePropertiesBlender = new NonTexturePropertiesBlendProps(this, resultMaterialTextureBlender);
	}

	private void LoadTextureBlenders()
	{
		string filterCriteria = "DigitalOpus.MB.Core.TextureBlender";
		TypeFilter filter = InterfaceFilter;
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			IEnumerable enumerable = null;
			try
			{
				enumerable = assembly.GetTypes();
			}
			catch (Exception ex)
			{
				ex.Equals(null);
			}
			if (enumerable == null)
			{
				continue;
			}
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.FindInterfaces(filter, filterCriteria).Length != 0)
				{
					list.Add(type);
				}
			}
		}
		TextureBlender textureBlender = null;
		List<TextureBlender> list2 = new List<TextureBlender>();
		foreach (Type item in list)
		{
			if (!item.IsAbstract && !item.IsInterface)
			{
				TextureBlender textureBlender2 = (TextureBlender)Activator.CreateInstance(item);
				if (textureBlender2 is TextureBlenderFallback)
				{
					textureBlender = textureBlender2;
				}
				else
				{
					list2.Add(textureBlender2);
				}
			}
		}
		if (textureBlender != null)
		{
			list2.Add(textureBlender);
		}
		textureBlenders = list2.ToArray();
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log($"Loaded {textureBlenders.Length} TextureBlenders.");
		}
	}

	internal bool NonTexturePropertiesAreEqual(Material a, Material b)
	{
		return _nonTexturePropertiesBlender.NonTexturePropertiesAreEqual(a, b);
	}

	internal Texture2D TintTextureWithTextureCombiner(Texture2D t, MB_TexSet sourceMaterial, ShaderTextureProperty shaderPropertyName)
	{
		return _nonTexturePropertiesBlender.TintTextureWithTextureCombiner(t, sourceMaterial, shaderPropertyName);
	}

	internal void AdjustNonTextureProperties(Material resultMat, List<ShaderTextureProperty> texPropertyNames, MB2_EditorMethodsInterface editorMethods)
	{
		if (!(resultMat == null) && texPropertyNames != null)
		{
			_nonTexturePropertiesBlender.AdjustNonTextureProperties(resultMat, texPropertyNames, editorMethods);
		}
	}

	internal Color GetColorAsItWouldAppearInAtlasIfNoTexture(Material matIfBlender, ShaderTextureProperty texProperty)
	{
		return _nonTexturePropertiesBlender.GetColorAsItWouldAppearInAtlasIfNoTexture(matIfBlender, texProperty);
	}

	internal Color GetColorForTemporaryTexture(Material matIfBlender, ShaderTextureProperty texProperty)
	{
		return _nonTexturePropertiesBlender.GetColorForTemporaryTexture(matIfBlender, texProperty);
	}

	private TextureBlender FindMatchingTextureBlender(string shaderName)
	{
		for (int i = 0; i < textureBlenders.Length; i++)
		{
			if (textureBlenders[i].DoesShaderNameMatch(shaderName))
			{
				return textureBlenders[i];
			}
		}
		return null;
	}
}
