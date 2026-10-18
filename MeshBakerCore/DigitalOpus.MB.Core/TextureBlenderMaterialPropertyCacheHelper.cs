using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class TextureBlenderMaterialPropertyCacheHelper
{
	private struct MaterialPropertyPair
	{
		public Material material;

		public string property;

		public MaterialPropertyPair(Material m, string prop)
		{
			material = m;
			property = prop;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MaterialPropertyPair materialPropertyPair))
			{
				return false;
			}
			if (!material.Equals(materialPropertyPair.material))
			{
				return false;
			}
			if (property != materialPropertyPair.property)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	private Dictionary<MaterialPropertyPair, object> nonTexturePropertyValuesForSourceMaterials = new Dictionary<MaterialPropertyPair, object>();

	private bool AllNonTexturePropertyValuesAreEqual(string prop)
	{
		bool flag = false;
		object obj = null;
		foreach (MaterialPropertyPair key in nonTexturePropertyValuesForSourceMaterials.Keys)
		{
			if (key.property.Equals(prop))
			{
				if (!flag)
				{
					obj = nonTexturePropertyValuesForSourceMaterials[key];
					flag = true;
				}
				else if (!obj.Equals(nonTexturePropertyValuesForSourceMaterials[key]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void CacheMaterialProperty(Material m, string property, object value)
	{
		nonTexturePropertyValuesForSourceMaterials[new MaterialPropertyPair(m, property)] = value;
	}

	public object GetValueIfAllSourceAreTheSameOrDefault(string property, object defaultValue)
	{
		if (AllNonTexturePropertyValuesAreEqual(property))
		{
			foreach (MaterialPropertyPair key in nonTexturePropertyValuesForSourceMaterials.Keys)
			{
				if (key.property.Equals(property))
				{
					return nonTexturePropertyValuesForSourceMaterials[key];
				}
			}
		}
		return defaultValue;
	}
}
