using System;
using System.Collections;
using UnityEngine;

namespace DigitalOpus.MB.Core;

internal class MB3_TextureCombinerPackerMeshBakerFast : MB_ITextureCombinerPacker
{
	public bool Validate(MB3_TextureCombinerPipeline.TexturePipelineData data)
	{
		return true;
	}

	public IEnumerator ConvertTexturesToReadableFormats(ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult result, MB3_TextureCombinerPipeline.TexturePipelineData data, MB3_TextureCombiner combiner, MB2_EditorMethodsInterface textureEditorMethods, MB2_LogLevel LOG_LEVEL)
	{
		yield break;
	}

	public virtual AtlasPackingResult[] CalculateAtlasRectangles(MB3_TextureCombinerPipeline.TexturePipelineData data, bool doMultiAtlas, MB2_LogLevel LOG_LEVEL)
	{
		return MB3_TextureCombinerPackerRoot.CalculateAtlasRectanglesStatic(data, doMultiAtlas, LOG_LEVEL);
	}

	public IEnumerator CreateAtlases(ProgressUpdateDelegate progressInfo, MB3_TextureCombinerPipeline.TexturePipelineData data, MB3_TextureCombiner combiner, AtlasPackingResult packedAtlasRects, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, MB2_LogLevel LOG_LEVEL)
	{
		Rect[] rects = packedAtlasRects.rects;
		int atlasX = packedAtlasRects.atlasX;
		int atlasY = packedAtlasRects.atlasY;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Generated atlas will be " + atlasX + "x" + atlasY);
		}
		GameObject gameObject = null;
		try
		{
			gameObject = new GameObject("MBrenderAtlasesGO");
			MB3_AtlasPackerRenderTexture mB3_AtlasPackerRenderTexture = gameObject.AddComponent<MB3_AtlasPackerRenderTexture>();
			gameObject.AddComponent<Camera>();
			if (data._considerNonTextureProperties && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogError("Blend Non-Texture Properties has limited functionality when used with Mesh Baker Texture Packer Fast. If no texture is pesent, then a small texture matching the non-texture property will be created and used in the atlas. But non-texture properties will not be blended into texture.");
			}
			for (int i = 0; i < data.numAtlases; i++)
			{
				Texture2D texture2D;
				if (!MB3_TextureCombinerPipeline._ShouldWeCreateAtlasForThisProperty(i, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
				{
					texture2D = null;
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						Debug.Log("Not creating atlas for " + data.texPropertyNames[i].name + " because textures are null and default value parameters are the same.");
					}
				}
				else
				{
					GC.Collect();
					MB3_TextureCombinerPackerRoot.CreateTemporaryTexturesForAtlas(data.distinctMaterialTextures, combiner, i, data);
					progressInfo?.Invoke("Creating Atlas '" + data.texPropertyNames[i].name + "'", 0.01f);
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						Debug.Log("About to render " + data.texPropertyNames[i].name + " isNormal=" + data.texPropertyNames[i].isNormalMap);
					}
					mB3_AtlasPackerRenderTexture.LOG_LEVEL = LOG_LEVEL;
					mB3_AtlasPackerRenderTexture.width = atlasX;
					mB3_AtlasPackerRenderTexture.height = atlasY;
					mB3_AtlasPackerRenderTexture.padding = data._atlasPadding_pix;
					mB3_AtlasPackerRenderTexture.rects = rects;
					mB3_AtlasPackerRenderTexture.textureSets = data.distinctMaterialTextures;
					mB3_AtlasPackerRenderTexture.indexOfTexSetToRender = i;
					mB3_AtlasPackerRenderTexture.texPropertyName = data.texPropertyNames[i];
					mB3_AtlasPackerRenderTexture.isNormalMap = data.texPropertyNames[i].isNormalMap;
					mB3_AtlasPackerRenderTexture.fixOutOfBoundsUVs = data._fixOutOfBoundsUVs;
					mB3_AtlasPackerRenderTexture.considerNonTextureProperties = data._considerNonTextureProperties;
					mB3_AtlasPackerRenderTexture.resultMaterialTextureBlender = data.nonTexturePropertyBlender;
					texture2D = mB3_AtlasPackerRenderTexture.OnRenderAtlas(combiner);
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						Debug.Log("Saving atlas " + data.texPropertyNames[i].name + " w=" + texture2D.width + " h=" + texture2D.height + " id=" + texture2D.GetInstanceID());
					}
				}
				atlases[i] = texture2D;
				progressInfo?.Invoke("Saving atlas: '" + data.texPropertyNames[i].name + "'", 0.04f);
				if (data.resultType == MB2_TextureBakeResults.ResultType.atlas)
				{
					MB3_TextureCombinerPackerRoot.SaveAtlasAndConfigureResultMaterial(data, textureEditorMethods, atlases[i], data.texPropertyNames[i], i);
				}
				combiner._destroyTemporaryTextures(data.texPropertyNames[i].name);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace.ToString());
		}
		finally
		{
			if (gameObject != null)
			{
				MB_Utility.Destroy(gameObject);
			}
		}
		yield break;
	}
}
