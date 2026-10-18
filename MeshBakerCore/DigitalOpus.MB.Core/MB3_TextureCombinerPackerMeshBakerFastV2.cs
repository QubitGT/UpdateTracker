using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DigitalOpus.MB.Core;

internal class MB3_TextureCombinerPackerMeshBakerFastV2 : MB_ITextureCombinerPacker
{
	private Mesh mesh;

	private GameObject renderAtlasesGO;

	private GameObject cameraGameObject;

	public bool Validate(MB3_TextureCombinerPipeline.TexturePipelineData data)
	{
		string text = LayerMask.LayerToName(data._layerTexturePackerFastV2);
		if (text == null || text.Length == 0)
		{
			UnityEngine.Debug.LogError("The MB3_MeshBaker -> 'Atlas Render Layer' has not been set. This should be set to a layer that has no other renderers on it.");
			return false;
		}
		if (Application.isEditor)
		{
			Renderer[] array = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject.layer == data._layerTexturePackerFastV2)
				{
					flag = true;
				}
			}
			if (flag)
			{
				UnityEngine.Debug.LogError("There are Renderers in the scene that are on layer '" + text + "'. 'Atlas Render Layer' layer should have no renderers that use it.");
				return false;
			}
		}
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
		int atlasX = packedAtlasRects.atlasX;
		int atlasY = packedAtlasRects.atlasY;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Generated atlas will be " + atlasX + "x" + atlasY);
		}
		int layerTexturePackerFastV = data._layerTexturePackerFastV2;
		mesh = new Mesh();
		renderAtlasesGO = null;
		cameraGameObject = null;
		try
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			renderAtlasesGO = new GameObject("MBrenderAtlasesGO");
			cameraGameObject = new GameObject("MBCameraGameObject");
			MB3_AtlasPackerRenderTextureUsingMesh mB3_AtlasPackerRenderTextureUsingMesh = new MB3_AtlasPackerRenderTextureUsingMesh();
			OneTimeSetup(mB3_AtlasPackerRenderTextureUsingMesh, renderAtlasesGO, cameraGameObject, atlasX, atlasY, data._atlasPadding_pix, layerTexturePackerFastV, LOG_LEVEL);
			if (data._considerNonTextureProperties && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Blend Non-Texture Properties has limited functionality when used with Mesh Baker Texture Packer Fast.");
			}
			List<Material> list = new List<Material>();
			for (int i = 0; i < data.numAtlases; i++)
			{
				Texture2D texture2D;
				if (!MB3_TextureCombinerPipeline._ShouldWeCreateAtlasForThisProperty(i, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
				{
					texture2D = null;
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						UnityEngine.Debug.Log("Not creating atlas for " + data.texPropertyNames[i].name + " because textures are null and default value parameters are the same.");
					}
				}
				else
				{
					progressInfo?.Invoke("Creating Atlas '" + data.texPropertyNames[i].name + "'", 0.01f);
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						UnityEngine.Debug.Log("About to render " + data.texPropertyNames[i].name + " isNormal=" + data.texPropertyNames[i].isNormalMap);
					}
					list.Clear();
					MB3_AtlasPackerRenderTextureUsingMesh.MeshAtlas.BuildAtlas(packedAtlasRects, data.distinctMaterialTextures, i, packedAtlasRects.atlasX, packedAtlasRects.atlasY, mesh, list, data.texPropertyNames[i], data, combiner, textureEditorMethods, LOG_LEVEL);
					renderAtlasesGO.GetComponent<MeshFilter>().sharedMesh = mesh;
					MeshRenderer component = renderAtlasesGO.GetComponent<MeshRenderer>();
					Material[] sharedMaterials = list.ToArray();
					component.sharedMaterials = sharedMaterials;
					texture2D = mB3_AtlasPackerRenderTextureUsingMesh.DoRenderAtlas(cameraGameObject, packedAtlasRects.atlasX, packedAtlasRects.atlasY, data.texPropertyNames[i].isNormalMap, data.texPropertyNames[i]);
					for (int j = 0; j < list.Count; j++)
					{
						MB_Utility.Destroy(list[j]);
					}
					list.Clear();
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						UnityEngine.Debug.Log("Saving atlas " + data.texPropertyNames[i].name + " w=" + texture2D.width + " h=" + texture2D.height + " id=" + texture2D.GetInstanceID());
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
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.LogFormat("Timing MB3_TextureCombinerPackerMeshBakerFastV2.CreateAtlases={0}", (float)stopwatch.ElapsedMilliseconds * 0.001f);
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex.Message + "\n" + ex.StackTrace.ToString());
		}
		finally
		{
			if (renderAtlasesGO != null)
			{
				MB_Utility.Destroy(renderAtlasesGO);
			}
			if (cameraGameObject != null)
			{
				MB_Utility.Destroy(cameraGameObject);
			}
			if (mesh != null)
			{
				MB_Utility.Destroy(mesh);
			}
		}
		yield break;
	}

	private void OneTimeSetup(MB3_AtlasPackerRenderTextureUsingMesh atlasRenderer, GameObject atlasMesh, GameObject cameraGameObject, int atlasWidth, int atlasHeight, int padding, int layer, MB2_LogLevel logLevel)
	{
		atlasMesh.AddComponent<MeshFilter>();
		atlasMesh.AddComponent<MeshRenderer>();
		atlasMesh.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		atlasMesh.transform.position = new Vector3(0f, 0f, 0.5f);
		atlasMesh.gameObject.layer = layer;
		atlasRenderer.Initialize(layer, atlasWidth, atlasHeight, padding, logLevel);
		atlasRenderer.SetupCameraGameObject(cameraGameObject);
	}
}
