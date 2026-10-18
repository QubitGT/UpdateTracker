using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB_TextureArrays
{
	internal class TexturePropertyData
	{
		public bool[] doMips;

		public int[] numMipMaps;

		public TextureFormat[] formats;

		public MB_TextureCompressionQuality[] compressionQualities;

		public Vector2[] sizes;
	}

	internal static bool[] DetermineWhichPropertiesHaveTextures(MB_AtlasesAndRects[] resultAtlasesAndRectSlices)
	{
		bool[] array = new bool[resultAtlasesAndRectSlices[0].texPropertyNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = false;
		}
		int num = resultAtlasesAndRectSlices.Length;
		for (int j = 0; j < num; j++)
		{
			MB_AtlasesAndRects mB_AtlasesAndRects = resultAtlasesAndRectSlices[j];
			for (int k = 0; k < array.Length; k++)
			{
				if (mB_AtlasesAndRects.atlases[k] != null)
				{
					array[k] = true;
				}
			}
		}
		return array;
	}

	private static bool IsLinearProperty(List<ShaderTextureProperty> shaderPropertyNames, string shaderProperty)
	{
		for (int i = 0; i < shaderPropertyNames.Count; i++)
		{
			if (shaderPropertyNames[i].name == shaderProperty && shaderPropertyNames[i].isNormalMap)
			{
				return true;
			}
		}
		return false;
	}

	internal static Texture2DArray[] CreateTextureArraysForResultMaterial(TexturePropertyData texPropertyData, List<ShaderTextureProperty> masterListOfTexProperties, MB_AtlasesAndRects[] resultAtlasesAndRectSlices, bool[] hasTexForProperty, MB3_TextureCombiner combiner, MB2_LogLevel LOG_LEVEL)
	{
		string[] texPropertyNames = resultAtlasesAndRectSlices[0].texPropertyNames;
		Texture2DArray[] array = new Texture2DArray[texPropertyNames.Length];
		for (int i = 0; i < texPropertyNames.Length; i++)
		{
			if (!hasTexForProperty[i])
			{
				continue;
			}
			string text = texPropertyNames[i];
			int num = resultAtlasesAndRectSlices.Length;
			int num2 = (int)texPropertyData.sizes[i].x;
			int num3 = (int)texPropertyData.sizes[i].y;
			int num4 = texPropertyData.numMipMaps[i];
			TextureFormat textureFormat = texPropertyData.formats[i];
			bool flag = texPropertyData.doMips[i];
			bool flag2 = MB3_TextureCombiner.ShouldTextureBeLinear(masterListOfTexProperties[i]);
			Texture2DArray texture2DArray = new Texture2DArray(num2, num3, num, textureFormat, flag, flag2);
			if (LOG_LEVEL >= MB2_LogLevel.info)
			{
				Debug.LogFormat("Creating Texture2DArray for property: {0} w: {1} h: {2} format: {3} doMips: {4} isLinear: {5}", text, num2, num3, textureFormat, flag, flag2);
			}
			for (int j = 0; j < num; j++)
			{
				Texture2D texture2D = resultAtlasesAndRectSlices[j].atlases[i];
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.LogFormat("Slice: {0}  texture: {1}  w: {2}  h: {3}  prop: {4}", j, texture2D, texture2D.width, texture2D.height, texPropertyNames[i]);
				}
				bool flag3 = false;
				if (texture2D == null)
				{
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						Debug.LogFormat("Texture is null for slice: {0} creating temporary texture", j);
					}
					texture2D = combiner._createTemporaryTexture(text, num2, num3, textureFormat, flag, flag2);
				}
				for (int k = 0; k < num4; k++)
				{
					Graphics.CopyTexture(texture2D, 0, k, texture2DArray, j, k);
				}
				if (flag3)
				{
					MB_Utility.Destroy(texture2D);
				}
			}
			array[i] = texture2DArray;
		}
		return array;
	}

	internal static bool ConvertTexturesToReadableFormat(TexturePropertyData texturePropertyData, MB_AtlasesAndRects[] resultAtlasesAndRectSlices, bool[] hasTexForProperty, List<ShaderTextureProperty> textureShaderProperties, MB3_TextureCombiner combiner, MB2_LogLevel logLevel, List<Texture2D> createdTemporaryTextureAssets, MB2_EditorMethodsInterface textureEditorMethods)
	{
		for (int i = 0; i < hasTexForProperty.Length; i++)
		{
			if (!hasTexForProperty[i])
			{
				continue;
			}
			TextureFormat textureFormat = texturePropertyData.formats[i];
			MB_TextureCompressionQuality compressionQuality = texturePropertyData.compressionQualities[i];
			if (textureEditorMethods != null && !textureEditorMethods.TextureImporterFormatExistsForTextureFormat(textureFormat))
			{
				Debug.LogError("Could not find target importer format matching " + textureFormat);
				return false;
			}
			int num = resultAtlasesAndRectSlices.Length;
			int num2 = (int)texturePropertyData.sizes[i].x;
			int num3 = (int)texturePropertyData.sizes[i].y;
			for (int j = 0; j < num; j++)
			{
				Texture2D texture2D = resultAtlasesAndRectSlices[j].atlases[i];
				if (texture2D != null)
				{
					if (!MBVersion.IsTextureReadable(texture2D))
					{
						if (textureEditorMethods == null)
						{
							Debug.LogError("Source texture must be readable: " + texture2D);
							return false;
						}
						textureEditorMethods.SetReadWriteFlag(texture2D, isReadable: true, addToList: true);
					}
					bool flag = true;
					if (textureEditorMethods != null)
					{
						flag = textureEditorMethods.IsAnAsset(texture2D);
					}
					if (logLevel >= MB2_LogLevel.trace)
					{
						Debug.Log("Considering format of property:" + textureShaderProperties[i].name + " texture: '" + texture2D.name + "' format:" + texture2D.format);
					}
					if (Application.isPlaying)
					{
						if (texture2D.width != num2 || texture2D.height != num3 || texture2D.format != textureFormat)
						{
							Debug.LogError("If creating Texture Arrays at runtime then source textures must be the same size as the texture array and in the same format as the texture array.Texture " + texture2D.name + " is not in the correct format or does not have the correct size. (" + texture2D.width + ", " + texture2D.height + ", " + texture2D.format);
							return false;
						}
					}
					else if (texture2D.width != num2 || texture2D.height != num3 || (!flag && texture2D.format != textureFormat))
					{
						resultAtlasesAndRectSlices[j].atlases[i] = textureEditorMethods.CreateTemporaryAssetCopyForTextureArray(textureShaderProperties[i], texture2D, num2, num3, textureFormat, logLevel);
						createdTemporaryTextureAssets.Add(resultAtlasesAndRectSlices[j].atlases[i]);
					}
					else if (texture2D.format != textureFormat)
					{
						textureEditorMethods.ConvertTextureFormat_PlatformOverride(texture2D, textureFormat, compressionQuality, textureShaderProperties[i].isNormalMap);
					}
				}
				if (resultAtlasesAndRectSlices[j].atlases[i].format != textureFormat)
				{
					Debug.LogError("Could not convert texture to format " + textureFormat.ToString() + ". This can happen if the target build platform in build settings does not support textures in this format. It may be necessary to switch the build platform in order to build texture arrays in this format.");
					return false;
				}
			}
		}
		return true;
	}

	internal static void FindBestSizeAndMipCountAndFormatForTextureArrays(List<ShaderTextureProperty> texPropertyNames, int maxAtlasSize, MB_TextureArrayFormatSet targetFormatSet, MB_AtlasesAndRects[] resultAtlasesAndRectSlices, TexturePropertyData texturePropertyData)
	{
		texturePropertyData.sizes = new Vector2[texPropertyNames.Count];
		texturePropertyData.doMips = new bool[texPropertyNames.Count];
		texturePropertyData.numMipMaps = new int[texPropertyNames.Count];
		texturePropertyData.formats = new TextureFormat[texPropertyNames.Count];
		texturePropertyData.compressionQualities = new MB_TextureCompressionQuality[texPropertyNames.Count];
		for (int i = 0; i < texPropertyNames.Count; i++)
		{
			int num = resultAtlasesAndRectSlices.Length;
			texturePropertyData.sizes[i] = new Vector3(16f, 16f, 1f);
			bool flag = false;
			int num2 = 1;
			for (int j = 0; j < num; j++)
			{
				Texture2D texture2D = resultAtlasesAndRectSlices[j].atlases[i];
				if (texture2D != null)
				{
					if (texture2D.mipmapCount > 1)
					{
						flag = true;
					}
					num2 = Mathf.Max(num2, texture2D.mipmapCount);
					texturePropertyData.sizes[i].x = Mathf.Min(Mathf.Max(texturePropertyData.sizes[i].x, texture2D.width), maxAtlasSize);
					texturePropertyData.sizes[i].y = Mathf.Min(Mathf.Max(texturePropertyData.sizes[i].y, texture2D.height), maxAtlasSize);
					texturePropertyData.formats[i] = targetFormatSet.GetFormatForProperty(texPropertyNames[i].name, out texturePropertyData.compressionQualities[i]);
				}
			}
			int a = Mathf.CeilToInt(Mathf.Log(maxAtlasSize, 2f)) + 1;
			texturePropertyData.numMipMaps[i] = Mathf.Min(a, num2);
			texturePropertyData.doMips[i] = flag;
		}
	}

	public static IEnumerator _CreateAtlasesCoroutineSingleResultMaterial(int resMatIdx, MB_TextureArrayResultMaterial bakedMatsAndSlicesResMat, MB_MultiMaterialTexArray resMatConfig, List<GameObject> objsToMesh, MB3_TextureCombiner combiner, MB_TextureArrayFormatSet[] textureArrayOutputFormats, MB_MultiMaterialTexArray[] resultMaterialsTexArray, List<ShaderTextureProperty> customShaderProperties, List<string> texPropNamesToIgnore, ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CreateAtlasesCoroutineResult coroutineResult, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null, float maxTimePerFrame = 0.01f)
	{
		MB2_LogLevel LOG_LEVEL = combiner.LOG_LEVEL;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("=== STAGE 1   Baking atlases for result material " + resMatIdx + " num slices:" + resMatConfig.slices.Count);
		}
		List<MB3_TextureCombiner.TemporaryTexture> generatedTemporaryAtlases = new List<MB3_TextureCombiner.TemporaryTexture>();
		combiner.saveAtlasesAsAssets = false;
		List<MB_TexArraySlice> slicesConfig = resMatConfig.slices;
		for (int sliceIdx = 0; sliceIdx < slicesConfig.Count; sliceIdx++)
		{
			List<MB_TexArraySliceRendererMatPair> srcMatAndObjPairs = slicesConfig[sliceIdx].sourceMaterials;
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				Debug.Log(" Baking atlases for result material:" + resMatIdx + " slice:" + sliceIdx);
			}
			Material combinedMaterial = resMatConfig.combinedMaterial;
			combiner.fixOutOfBoundsUVs = slicesConfig[sliceIdx].considerMeshUVs;
			MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult coroutineResult2 = new MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult();
			MB_AtlasesAndRects sliceAtlasesAndRectOutput = bakedMatsAndSlicesResMat.slices[sliceIdx];
			List<Material> list = new List<Material>();
			slicesConfig[sliceIdx].GetAllUsedMaterials(list);
			yield return combiner.CombineTexturesIntoAtlasesCoroutine(progressInfo, sliceAtlasesAndRectOutput, combinedMaterial, slicesConfig[sliceIdx].GetAllUsedRenderers(objsToMesh), list, texPropNamesToIgnore, editorMethods, coroutineResult2, maxTimePerFrame);
			coroutineResult.success = coroutineResult2.success;
			if (!coroutineResult.success)
			{
				coroutineResult.isFinished = true;
				yield break;
			}
			for (int i = 0; i < sliceAtlasesAndRectOutput.atlases.Length; i++)
			{
				Texture2D texture2D = sliceAtlasesAndRectOutput.atlases[i];
				if (!(texture2D != null))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < srcMatAndObjPairs.Count; j++)
				{
					Material sourceMaterial = srcMatAndObjPairs[j].sourceMaterial;
					if (sourceMaterial.HasProperty(sliceAtlasesAndRectOutput.texPropertyNames[i]) && sourceMaterial.GetTexture(sliceAtlasesAndRectOutput.texPropertyNames[i]) == texture2D)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					generatedTemporaryAtlases.Add(new MB3_TextureCombiner.TemporaryTexture(sliceAtlasesAndRectOutput.texPropertyNames[i], texture2D));
				}
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("Property: " + sliceAtlasesAndRectOutput.texPropertyNames[i] + " atlasWasSrcTex:" + flag);
				}
			}
		}
		combiner.saveAtlasesAsAssets = saveAtlasesAsAssets;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("=== STAGE 2 Generate Temporary Textures");
		}
		for (int k = 0; k < generatedTemporaryAtlases.Count; k++)
		{
			combiner.AddTemporaryTexture(generatedTemporaryAtlases[k]);
		}
		List<ShaderTextureProperty> list2 = new List<ShaderTextureProperty>();
		MB3_TextureCombinerPipeline._CollectPropertyNames(list2, customShaderProperties, texPropNamesToIgnore, resMatConfig.combinedMaterial, LOG_LEVEL);
		bool[] array = DetermineWhichPropertiesHaveTextures(bakedMatsAndSlicesResMat.slices);
		List<Texture2D> list3 = new List<Texture2D>();
		try
		{
			Dictionary<string, MB_TexArrayForProperty> dictionary = new Dictionary<string, MB_TexArrayForProperty>();
			for (int l = 0; l < list2.Count; l++)
			{
				if (array[l])
				{
					dictionary[list2[l].name] = new MB_TexArrayForProperty(list2[l].name, new MB_TextureArrayReference[textureArrayOutputFormats.Length]);
				}
			}
			MB3_TextureCombinerNonTextureProperties mB3_TextureCombinerNonTextureProperties = new MB3_TextureCombinerNonTextureProperties(LOG_LEVEL, combiner.considerNonTextureProperties);
			mB3_TextureCombinerNonTextureProperties.LoadTextureBlendersIfNeeded(resMatConfig.combinedMaterial);
			mB3_TextureCombinerNonTextureProperties.AdjustNonTextureProperties(resMatConfig.combinedMaterial, list2, editorMethods);
			for (int m = 0; m < textureArrayOutputFormats.Length; m++)
			{
				MB_TextureArrayFormatSet mB_TextureArrayFormatSet = textureArrayOutputFormats[m];
				editorMethods?.Clear();
				TexturePropertyData texturePropertyData = new TexturePropertyData();
				FindBestSizeAndMipCountAndFormatForTextureArrays(list2, combiner.maxAtlasSize, mB_TextureArrayFormatSet, bakedMatsAndSlicesResMat.slices, texturePropertyData);
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("=== STAGE 3 formatSet: " + mB_TextureArrayFormatSet.name + " generate necessary temporary textures ");
				}
				for (int n = 0; n < array.Length; n++)
				{
					if (!array[n])
					{
						continue;
					}
					TextureFormat format = texturePropertyData.formats[n];
					int num = bakedMatsAndSlicesResMat.slices.Length;
					int num2 = (int)texturePropertyData.sizes[n].x;
					int num3 = (int)texturePropertyData.sizes[n].y;
					for (int num4 = 0; num4 < num; num4++)
					{
						if (bakedMatsAndSlicesResMat.slices[num4].atlases[n] == null)
						{
							Texture2D texture2D2 = new Texture2D(num2, num3, TextureFormat.ARGB32, texturePropertyData.doMips[n]);
							ShaderTextureProperty shaderTextureProperty = list2[n];
							Color c = ((!shaderTextureProperty.isNormalMap) ? mB3_TextureCombinerNonTextureProperties.GetColorForTemporaryTexture(resMatConfig.slices[num4].sourceMaterials[0].sourceMaterial, shaderTextureProperty) : MB3_TextureCombiner.NEUTRAL_NORMAL_MAP_COLOR_NON_SWIZZLED);
							MB_Utility.setSolidColor(texture2D2, c);
							bakedMatsAndSlicesResMat.slices[num4].atlases[n] = editorMethods.CreateTemporaryAssetCopyForTextureArray(list2[n], texture2D2, num2, num3, format, LOG_LEVEL);
							list3.Add(bakedMatsAndSlicesResMat.slices[num4].atlases[n]);
							MB_Utility.Destroy(texture2D2);
						}
					}
				}
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("=== STAGE 4 formatSet: " + mB_TextureArrayFormatSet.name + " Converting source textures to readable formats.");
				}
				if (!ConvertTexturesToReadableFormat(texturePropertyData, bakedMatsAndSlicesResMat.slices, array, list2, combiner, LOG_LEVEL, list3, editorMethods))
				{
					continue;
				}
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("=== STAGE 5 formatSet: " + mB_TextureArrayFormatSet.name + " Creating texture array");
				}
				Texture2DArray[] array2 = CreateTextureArraysForResultMaterial(texturePropertyData, list2, bakedMatsAndSlicesResMat.slices, array, combiner, LOG_LEVEL);
				for (int num5 = 0; num5 < array2.Length; num5++)
				{
					if (array[num5])
					{
						MB_TextureArrayReference mB_TextureArrayReference = new MB_TextureArrayReference(mB_TextureArrayFormatSet.name, array2[num5]);
						dictionary[list2[num5].name].formats[m] = mB_TextureArrayReference;
						if (saveAtlasesAsAssets)
						{
							editorMethods.SaveTextureArrayToAssetDatabase(array2[num5], mB_TextureArrayFormatSet.GetFormatForProperty(list2[num5].name, out var _), bakedMatsAndSlicesResMat.slices[0].texPropertyNames[num5], num5, resMatConfig.combinedMaterial);
						}
					}
				}
			}
			resMatConfig.textureProperties = new List<MB_TexArrayForProperty>();
			foreach (MB_TexArrayForProperty value in dictionary.Values)
			{
				resMatConfig.textureProperties.Add(value);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace.ToString());
			coroutineResult.isFinished = true;
			coroutineResult.success = false;
		}
		finally
		{
			editorMethods?.RestoreReadFlagsAndFormats(progressInfo);
			combiner._destroyAllTemporaryTextures();
			for (int num6 = 0; num6 < list3.Count; num6++)
			{
				editorMethods?.DestroyAsset(list3[num6]);
			}
			list3.Clear();
		}
	}
}
