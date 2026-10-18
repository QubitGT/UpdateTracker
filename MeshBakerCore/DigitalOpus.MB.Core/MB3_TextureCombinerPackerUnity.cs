using System;
using System.Collections;
using UnityEngine;

namespace DigitalOpus.MB.Core;

internal class MB3_TextureCombinerPackerUnity : MB3_TextureCombinerPackerRoot
{
	public override bool Validate(MB3_TextureCombinerPipeline.TexturePipelineData data)
	{
		return true;
	}

	public override AtlasPackingResult[] CalculateAtlasRectangles(MB3_TextureCombinerPipeline.TexturePipelineData data, bool doMultiAtlas, MB2_LogLevel LOG_LEVEL)
	{
		return new AtlasPackingResult[1]
		{
			new AtlasPackingResult(new AtlasPadding[0])
		};
	}

	public override IEnumerator CreateAtlases(ProgressUpdateDelegate progressInfo, MB3_TextureCombinerPipeline.TexturePipelineData data, MB3_TextureCombiner combiner, AtlasPackingResult packedAtlasRects, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, MB2_LogLevel LOG_LEVEL)
	{
		long num = 0L;
		int w = 1;
		int h = 1;
		Rect[] array = null;
		for (int i = 0; i < data.numAtlases; i++)
		{
			ShaderTextureProperty shaderTextureProperty = data.texPropertyNames[i];
			Texture2D texture2D;
			if (!MB3_TextureCombinerPipeline._ShouldWeCreateAtlasForThisProperty(i, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
			{
				texture2D = null;
			}
			else
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.LogWarning("Beginning loop " + i + " num temporary textures " + combiner._getNumTemporaryTextures());
				}
				MB3_TextureCombinerPackerRoot.CreateTemporaryTexturesForAtlas(data.distinctMaterialTextures, combiner, i, data);
				Texture2D[] array2 = new Texture2D[data.distinctMaterialTextures.Count];
				for (int j = 0; j < data.distinctMaterialTextures.Count; j++)
				{
					MB_TexSet mB_TexSet = data.distinctMaterialTextures[j];
					int idealWidth_pix = mB_TexSet.idealWidth_pix;
					int idealHeight_pix = mB_TexSet.idealHeight_pix;
					Texture2D texture2D2 = mB_TexSet.ts[i].GetTexture2D();
					progressInfo?.Invoke("Adjusting for scale and offset " + texture2D2, 0.01f);
					textureEditorMethods?.SetReadWriteFlag(texture2D2, isReadable: true, addToList: true);
					texture2D2 = GetAdjustedForScaleAndOffset2(shaderTextureProperty, mB_TexSet.ts[i], mB_TexSet.obUVoffset, mB_TexSet.obUVscale, data, combiner, LOG_LEVEL);
					if (texture2D2.width != idealWidth_pix || texture2D2.height != idealHeight_pix)
					{
						progressInfo?.Invoke("Resizing texture '" + texture2D2?.ToString() + "'", 0.01f);
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							Debug.LogWarning("Copying and resizing texture " + shaderTextureProperty.name + " from " + texture2D2.width + "x" + texture2D2.height + " to " + idealWidth_pix + "x" + idealHeight_pix);
						}
						texture2D2 = combiner._resizeTexture(shaderTextureProperty, texture2D2, idealWidth_pix, idealHeight_pix);
					}
					num += texture2D2.width * texture2D2.height;
					if (data._considerNonTextureProperties)
					{
						texture2D2 = combiner._createTextureCopy(shaderTextureProperty, texture2D2);
						data.nonTexturePropertyBlender.TintTextureWithTextureCombiner(texture2D2, data.distinctMaterialTextures[j], shaderTextureProperty);
					}
					array2[j] = texture2D2;
				}
				textureEditorMethods?.CheckBuildSettings(num);
				if (Math.Sqrt(num) > 6144.0 && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("The maximum possible atlas size is " + 8192 + ". Textures may be shrunk");
				}
				texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: true);
				progressInfo?.Invoke("Packing texture atlas " + shaderTextureProperty.name, 0.25f);
				if (i == 0)
				{
					progressInfo?.Invoke("Estimated min size of atlases: " + Math.Sqrt(num).ToString("F0"), 0.1f);
					if (LOG_LEVEL >= MB2_LogLevel.info)
					{
						Debug.Log("Estimated atlas minimum size:" + Math.Sqrt(num).ToString("F0"));
					}
					array = texture2D.PackTextures(array2, data._atlasPadding_pix, 8192, makeNoLongerReadable: false);
					if (LOG_LEVEL >= MB2_LogLevel.info)
					{
						Debug.Log("After pack textures atlas numTextures " + array2.Length + " size " + texture2D.width + " " + texture2D.height);
					}
					w = texture2D.width;
					h = texture2D.height;
					texture2D.Apply();
				}
				else
				{
					progressInfo?.Invoke("Copying Textures Into: " + shaderTextureProperty.name, 0.1f);
					texture2D = _copyTexturesIntoAtlas(shaderTextureProperty, array2, data._atlasPadding_pix, array, w, h, combiner);
				}
			}
			atlases[i] = texture2D;
			if (data._saveAtlasesAsAssets && textureEditorMethods != null)
			{
				MB3_TextureCombinerPackerRoot.SaveAtlasAndConfigureResultMaterial(data, textureEditorMethods, atlases[i], data.texPropertyNames[i], i);
			}
			data.resultMaterial.SetTextureOffset(shaderTextureProperty.name, Vector2.zero);
			data.resultMaterial.SetTextureScale(shaderTextureProperty.name, Vector2.one);
			combiner._destroyTemporaryTextures(shaderTextureProperty.name);
			GC.Collect();
		}
		packedAtlasRects.rects = array;
		yield break;
	}

	internal static Texture2D _copyTexturesIntoAtlas(ShaderTextureProperty prop, Texture2D[] texToPack, int padding, Rect[] rs, int w, int h, MB3_TextureCombiner combiner)
	{
		Texture2D texture2D = new Texture2D(w, h, TextureFormat.ARGB32, mipChain: true);
		MB_Utility.setSolidColor(texture2D, Color.clear);
		for (int i = 0; i < rs.Length; i++)
		{
			Rect rect = rs[i];
			Texture2D texture2D2 = texToPack[i];
			Texture2D texture2D3 = null;
			int x = Mathf.RoundToInt(rect.x * (float)w);
			int y = Mathf.RoundToInt(rect.y * (float)h);
			int num = Mathf.RoundToInt(rect.width * (float)w);
			int num2 = Mathf.RoundToInt(rect.height * (float)h);
			if (texture2D2.width != num && texture2D2.height != num2)
			{
				texture2D3 = (texture2D2 = MB_Utility.resampleTexture(texture2D2, prop.isGammaCorrected, num, num2));
			}
			texture2D.SetPixels(x, y, num, num2, texture2D2.GetPixels());
			if (texture2D3 != null)
			{
				MB_Utility.Destroy(texture2D3);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	internal static Texture2D GetAdjustedForScaleAndOffset2(ShaderTextureProperty propertyName, MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale, MB3_TextureCombinerPipeline.TexturePipelineData data, MB3_TextureCombiner combiner, MB2_LogLevel LOG_LEVEL)
	{
		Texture2D texture2D = source.GetTexture2D();
		if (source.matTilingRect.x == 0.0 && source.matTilingRect.y == 0.0 && source.matTilingRect.width == 1.0 && source.matTilingRect.height == 1.0)
		{
			if (!data._fixOutOfBoundsUVs)
			{
				return texture2D;
			}
			if (obUVoffset.x == 0f && obUVoffset.y == 0f && obUVscale.x == 1f && obUVscale.y == 1f)
			{
				return texture2D;
			}
		}
		Vector2 adjustedForScaleAndOffset2Dimensions = MB3_TextureCombinerPipeline.GetAdjustedForScaleAndOffset2Dimensions(source, obUVoffset, obUVscale, data, LOG_LEVEL);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			string[] obj = new string[6]
			{
				"GetAdjustedForScaleAndOffset2: ",
				texture2D?.ToString(),
				" ",
				null,
				null,
				null
			};
			Vector2 vector = obUVoffset;
			obj[3] = vector.ToString();
			obj[4] = " ";
			vector = obUVscale;
			obj[5] = vector.ToString();
			Debug.LogWarning(string.Concat(obj));
		}
		float x = adjustedForScaleAndOffset2Dimensions.x;
		float y = adjustedForScaleAndOffset2Dimensions.y;
		float num = (float)source.matTilingRect.width;
		float num2 = (float)source.matTilingRect.height;
		float num3 = (float)source.matTilingRect.x;
		float num4 = (float)source.matTilingRect.y;
		if (data._fixOutOfBoundsUVs)
		{
			num *= obUVscale.x;
			num2 *= obUVscale.y;
			num3 = (float)(source.matTilingRect.x * (double)obUVscale.x + (double)obUVoffset.x);
			num4 = (float)(source.matTilingRect.y * (double)obUVscale.y + (double)obUVoffset.y);
		}
		Texture2D texture2D2 = combiner._createTemporaryTexture(propertyName.name, (int)x, (int)y, TextureFormat.ARGB32, mipMaps: true, MB3_TextureCombiner.ShouldTextureBeLinear(propertyName));
		for (int i = 0; i < texture2D2.width; i++)
		{
			for (int j = 0; j < texture2D2.height; j++)
			{
				float u = (float)i / x * num + num3;
				float v = (float)j / y * num2 + num4;
				texture2D2.SetPixel(i, j, texture2D.GetPixelBilinear(u, v));
			}
		}
		texture2D2.Apply();
		return texture2D2;
	}
}
