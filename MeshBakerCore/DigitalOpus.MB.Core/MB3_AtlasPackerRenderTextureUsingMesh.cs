using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_AtlasPackerRenderTextureUsingMesh
{
	public class MeshRectInfo
	{
		public int vertIdx;

		public int triIdx;

		public int atlasIdx;
	}

	public class MeshAtlas
	{
		internal static void BuildAtlas(AtlasPackingResult packedAtlasRects, List<MB_TexSet> distinctMaterialTextures, int propIdx, int atlasSizeX, int atlasSizeY, Mesh m, List<Material> generatedMats, ShaderTextureProperty property, MB3_TextureCombinerPipeline.TexturePipelineData data, MB3_TextureCombiner combiner, MB2_EditorMethodsInterface textureEditorMethods, MB2_LogLevel LOG_LEVEL)
		{
			generatedMats.Clear();
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			List<int>[] array = new List<int>[distinctMaterialTextures.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new List<int>();
			}
			MeshBakerMaterialTexture.readyToBuildAtlases = true;
			GC.Collect();
			MB3_TextureCombinerPackerRoot.CreateTemporaryTexturesForAtlas(data.distinctMaterialTextures, combiner, propIdx, data);
			Rect[] rects = packedAtlasRects.rects;
			for (int j = 0; j < distinctMaterialTextures.Count; j++)
			{
				MB_TexSet mB_TexSet = distinctMaterialTextures[j];
				MeshBakerMaterialTexture meshBakerMaterialTexture = mB_TexSet.ts[propIdx];
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log($"Adding texture {meshBakerMaterialTexture.GetTexName()} to atlas {property.name} for texSet {j} srcMat {mB_TexSet.matsAndGOs.mats[0].GetMaterialName()}");
				}
				Rect rect = rects[j];
				Texture2D texture2D = meshBakerMaterialTexture.GetTexture2D();
				int num = Mathf.RoundToInt(rect.x * (float)atlasSizeX);
				int num2 = Mathf.RoundToInt(rect.y * (float)atlasSizeY);
				int num3 = Mathf.RoundToInt(rect.width * (float)atlasSizeX);
				int num4 = Mathf.RoundToInt(rect.height * (float)atlasSizeY);
				rect = new Rect(num, num2, num3, num4);
				if (num3 == 0 || num4 == 0)
				{
					Rect rect2 = rect;
					UnityEngine.Debug.LogError("Image in atlas has no height or width " + rect2.ToString());
				}
				DRect encapsulatingSamplingRect = mB_TexSet.ts[propIdx].GetEncapsulatingSamplingRect();
				AtlasPadding atlasPadding = packedAtlasRects.padding[j];
				AddNineSlicedRect(rect, atlasPadding.leftRight, atlasPadding.topBottom, encapsulatingSamplingRect.GetRect(), list, list2, array[j], texture2D.width, texture2D.height, texture2D.name);
				Material material = new Material(Shader.Find("MeshBaker/Unlit/UnlitWithAlpha"));
				bool isSavingAsANormalMapAssetThatWillBeImported = property.isNormalMap && data._saveAtlasesAsAssets;
				switch (MBVersion.DetectPipeline())
				{
				case MBVersion.PipelineType.URP:
					ConfigureMaterial_DefaultPipeline(material, texture2D, isSavingAsANormalMapAssetThatWillBeImported, LOG_LEVEL);
					break;
				case MBVersion.PipelineType.HDRP:
					ConfigureMaterial_DefaultPipeline(material, texture2D, isSavingAsANormalMapAssetThatWillBeImported, LOG_LEVEL);
					break;
				default:
					ConfigureMaterial_DefaultPipeline(material, texture2D, isSavingAsANormalMapAssetThatWillBeImported, LOG_LEVEL);
					break;
				}
				generatedMats.Add(material);
			}
			m.Clear();
			m.vertices = list.ToArray();
			m.uv = list2.ToArray();
			m.subMeshCount = array.Length;
			for (int k = 0; k < m.subMeshCount; k++)
			{
				m.SetIndices(array[k].ToArray(), MeshTopology.Triangles, k);
			}
			MeshBakerMaterialTexture.readyToBuildAtlases = false;
		}

		private static void ConfigureMaterial_DefaultPipeline(Material mt, Texture2D t, bool isSavingAsANormalMapAssetThatWillBeImported, MB2_LogLevel LOG_LEVEL)
		{
			Shader shader = null;
			shader = Shader.Find("MeshBaker/Unlit/UnlitWithAlpha");
			mt.shader = shader;
			mt.SetTexture("_MainTex", t);
			if (isSavingAsANormalMapAssetThatWillBeImported)
			{
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log("Unswizling normal map channels NM");
				}
				mt.SetFloat("_SwizzleNormalMapChannelsNM", 1f);
				mt.EnableKeyword("_SWIZZLE_NORMAL_CHANNELS_NM");
			}
			else
			{
				mt.SetFloat("_SwizzleNormalMapChannelsNM", 0f);
				mt.DisableKeyword("_SWIZZLE_NORMAL_CHANNELS_NM");
			}
		}

		public static MeshRectInfo AddQuad(Rect wldRect, Rect uvRect, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
		{
			MeshRectInfo meshRectInfo = new MeshRectInfo();
			int num = (meshRectInfo.vertIdx = verts.Count);
			meshRectInfo.triIdx = tris.Count;
			verts.Add(new Vector3(wldRect.x, wldRect.y, 0f));
			verts.Add(new Vector3(wldRect.x + wldRect.width, wldRect.y, 0f));
			verts.Add(new Vector3(wldRect.x, wldRect.y + wldRect.height, 0f));
			verts.Add(new Vector3(wldRect.x + wldRect.width, wldRect.y + wldRect.height, 0f));
			uvs.Add(new Vector2(uvRect.x, uvRect.y));
			uvs.Add(new Vector2(uvRect.x + uvRect.width, uvRect.y));
			uvs.Add(new Vector2(uvRect.x, uvRect.y + uvRect.height));
			uvs.Add(new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height));
			tris.Add(num);
			tris.Add(num + 2);
			tris.Add(num + 1);
			tris.Add(num + 2);
			tris.Add(num + 3);
			tris.Add(num + 1);
			return meshRectInfo;
		}

		public static void AddNineSlicedRect(Rect atlasRectRaw, float paddingX, float paddingY, Rect srcUVRectt, List<Vector3> verts, List<Vector2> uvs, List<int> tris, float srcTexWidth, float srcTexHeight, string texName)
		{
			float num = 0.5f / srcTexWidth;
			float num2 = 0.5f / srcTexHeight;
			float num3 = 0f;
			float num4 = 0f;
			Rect uvRect = srcUVRectt;
			Rect rect = srcUVRectt;
			rect.x += num;
			rect.y += num2;
			rect.width -= num * 2f;
			rect.height -= num2 * 2f;
			Rect rect2 = atlasRectRaw;
			AddQuad(atlasRectRaw, uvRect, verts, uvs, tris);
			bool num5 = paddingY > 0f;
			bool flag = paddingX > 0f;
			if (num5)
			{
				AddQuad(uvRect: new Rect(uvRect.x, uvRect.y + uvRect.height - num2 - num4, uvRect.width, num4), wldRect: new Rect(rect2.x, rect2.y + rect2.height, rect2.width, paddingY), verts: verts, uvs: uvs, tris: tris);
			}
			if (num5)
			{
				AddQuad(uvRect: new Rect(uvRect.x, uvRect.y + num2 - num4, uvRect.width, num4), wldRect: new Rect(rect2.x, rect2.y - paddingY, rect2.width, paddingY), verts: verts, uvs: uvs, tris: tris);
			}
			if (flag)
			{
				AddQuad(uvRect: new Rect(uvRect.x + num, uvRect.y, num3, uvRect.height), wldRect: new Rect(rect2.x - paddingX, rect2.y, paddingX, rect2.height), verts: verts, uvs: uvs, tris: tris);
			}
			if (flag)
			{
				AddQuad(uvRect: new Rect(uvRect.x + uvRect.width - num - num3, uvRect.y, num3, uvRect.height), wldRect: new Rect(rect2.x + rect2.width, rect2.y, paddingX, rect2.height), verts: verts, uvs: uvs, tris: tris);
			}
			if (num5 && flag)
			{
				AddQuad(uvRect: new Rect(uvRect.x + num, uvRect.y + num2, num3, num4), wldRect: new Rect(rect2.x - paddingX, rect2.y - paddingY, paddingX, paddingY), verts: verts, uvs: uvs, tris: tris);
			}
			if (num5 && flag)
			{
				AddQuad(uvRect: new Rect(uvRect.x + num, uvRect.y + uvRect.height - num2 - num4, num3, num4), wldRect: new Rect(rect2.x - paddingX, rect2.y + rect2.height, paddingX, paddingY), verts: verts, uvs: uvs, tris: tris);
			}
			if (num5 && flag)
			{
				AddQuad(uvRect: new Rect(uvRect.x + uvRect.width - num - num3, uvRect.y + uvRect.height - num2 - num4, num3, num4), wldRect: new Rect(rect2.x + rect2.width, rect2.y + rect2.height, paddingX, paddingY), verts: verts, uvs: uvs, tris: tris);
			}
			if (num5 && flag)
			{
				AddQuad(uvRect: new Rect(uvRect.x + uvRect.width - num - num3, uvRect.y + num2 - num4, num3, num4), wldRect: new Rect(rect2.x + rect2.width, rect2.y - paddingY, paddingX, paddingY), verts: verts, uvs: uvs, tris: tris);
			}
		}
	}

	public int camMaskLayer;

	public int width;

	public int height;

	public int padding;

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	private bool _initialized;

	private bool _camSetup;

	public void Initialize(int camMaskLayer, int width, int height, int padding, MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info)
	{
		this.camMaskLayer = camMaskLayer;
		this.width = width;
		this.height = height;
		this.padding = padding;
		this.LOG_LEVEL = LOG_LEVEL;
		_initialized = true;
	}

	internal void SetupCameraGameObject(GameObject camGameObject)
	{
		LayerMask layerMask = 1 << camMaskLayer;
		Camera camera = camGameObject.AddComponent<Camera>();
		camera.enabled = false;
		camera.orthographic = true;
		camera.orthographicSize = (float)height / 2f;
		camera.aspect = (float)width / (float)height;
		camera.rect = new Rect(0f, 0f, 1f, 1f);
		camera.clearFlags = CameraClearFlags.Color;
		camera.cullingMask = layerMask;
		Transform component = camera.GetComponent<Transform>();
		component.localPosition = new Vector3((float)width / 2f, (float)height / 2f, 0f);
		component.localRotation = Quaternion.Euler(0f, 0f, 0f);
		MBVersion.DoSpecialRenderPipeline_TexturePackerFastSetup(camGameObject);
		_camSetup = true;
	}

	internal Texture2D DoRenderAtlas(GameObject go, int width, int height, bool isNormalMap, ShaderTextureProperty propertyName)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		RenderTexture renderTexture = ((!isNormalMap) ? new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) : new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB));
		renderTexture.filterMode = FilterMode.Point;
		Camera component = go.GetComponent<Camera>();
		component.targetTexture = renderTexture;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log(string.Format("Begin Camera.Render destTex w={0} h={1} camPos={2} camSize={3} camAspect={4}", width, height, go.transform.localPosition, component.orthographicSize, component.aspect.ToString("f5")));
		}
		component.Render();
		Stopwatch stopwatch2 = new Stopwatch();
		stopwatch2.Start();
		Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, mipChain: true, linear: false);
		MB_TextureCombinerRenderTexture.ConvertRenderTextureToTexture2D(renderTexture, MB_TextureCombinerRenderTexture.YisFlipped(LOG_LEVEL), isNormalMap, LOG_LEVEL, texture2D);
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			UnityEngine.Debug.Log("Finished rendering atlas " + propertyName.name + "  db_time_DoRenderAtlas:" + (float)stopwatch.ElapsedMilliseconds * 0.001f + "  db_ConvertRenderTextureToTexture2D:" + (float)stopwatch2.ElapsedMilliseconds * 0.001f);
		}
		MB_Utility.Destroy(renderTexture);
		return texture2D;
	}
}
