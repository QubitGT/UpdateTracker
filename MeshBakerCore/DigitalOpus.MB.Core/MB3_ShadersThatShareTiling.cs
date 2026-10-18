using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_ShadersThatShareTiling
{
	public struct ShaderThatSharesTiling
	{
		public string shadername;

		public bool allPropsShareTiling;

		public string tilingTexturePropName;
	}

	private static MB3_ShadersThatShareTiling _singleton;

	private Dictionary<string, ShaderThatSharesTiling> shadersThatShareTiling;

	public static MB3_ShadersThatShareTiling GetShadersThatShareTiling()
	{
		if (_singleton == null)
		{
			Init();
		}
		return _singleton;
	}

	public static void GetScaleAndOffsetForTextureProp(Material m, string texturePropName, out Vector2 offset, out Vector2 scale)
	{
		if (GetShadersThatShareTiling().shadersThatShareTiling.TryGetValue(m.shader.name, out var value) && value.allPropsShareTiling && m.HasProperty(value.tilingTexturePropName))
		{
			scale = m.GetTextureScale(value.tilingTexturePropName);
			offset = m.GetTextureOffset(value.tilingTexturePropName);
		}
		else
		{
			scale = m.GetTextureScale(texturePropName);
			offset = m.GetTextureOffset(texturePropName);
		}
	}

	private static void Init()
	{
		_singleton = new MB3_ShadersThatShareTiling();
		Dictionary<string, ShaderThatSharesTiling> dictionary = (_singleton.shadersThatShareTiling = new Dictionary<string, ShaderThatSharesTiling>());
		ShaderThatSharesTiling value = default(ShaderThatSharesTiling);
		value.shadername = "Standard";
		value.allPropsShareTiling = true;
		value.tilingTexturePropName = "_MainTex";
		ShaderThatSharesTiling value2 = default(ShaderThatSharesTiling);
		value2.shadername = "Standard (Specular setup)";
		value2.allPropsShareTiling = true;
		value2.tilingTexturePropName = "_MainTex";
		ShaderThatSharesTiling value3 = default(ShaderThatSharesTiling);
		value3.shadername = "Universal Render Pipeline/Lit";
		value3.allPropsShareTiling = true;
		value3.tilingTexturePropName = "_BaseMap";
		ShaderThatSharesTiling shaderThatSharesTiling = default(ShaderThatSharesTiling);
		shaderThatSharesTiling.shadername = "Universal Render Pipeline/Simple Lit";
		shaderThatSharesTiling.allPropsShareTiling = true;
		shaderThatSharesTiling.tilingTexturePropName = "_BaseMap";
		ShaderThatSharesTiling shaderThatSharesTiling2 = default(ShaderThatSharesTiling);
		shaderThatSharesTiling2.shadername = "Universal Render Pipeline/Complex Lit";
		shaderThatSharesTiling2.allPropsShareTiling = true;
		shaderThatSharesTiling2.tilingTexturePropName = "_BaseMap";
		ShaderThatSharesTiling shaderThatSharesTiling3 = default(ShaderThatSharesTiling);
		shaderThatSharesTiling3.shadername = "Universal Render Pipeline/Baked Lit";
		shaderThatSharesTiling3.allPropsShareTiling = true;
		shaderThatSharesTiling3.tilingTexturePropName = "_BaseMap";
		dictionary.Add(value.shadername, value);
		dictionary.Add(value2.shadername, value2);
		dictionary.Add(value3.shadername, value3);
		dictionary.Add(shaderThatSharesTiling.shadername, value3);
		dictionary.Add(shaderThatSharesTiling2.shadername, value3);
		dictionary.Add(shaderThatSharesTiling3.shadername, value3);
	}
}
