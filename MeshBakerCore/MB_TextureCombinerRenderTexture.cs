using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB_TextureCombinerRenderTexture
{
	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	private Material mat;

	private RenderTexture _destinationTexture;

	private Camera myCamera;

	private int _padding;

	private bool _isNormalMap;

	private bool _fixOutOfBoundsUVs;

	private bool _doRenderAtlas;

	private Rect[] rs;

	private List<MB_TexSet> textureSets;

	private int indexOfTexSetToRender;

	private ShaderTextureProperty _texPropertyName;

	private MB3_TextureCombinerNonTextureProperties _resultMaterialTextureBlender;

	private Texture2D targTex;

	public Texture2D DoRenderAtlas(GameObject gameObject, int width, int height, int padding, Rect[] rss, List<MB_TexSet> textureSetss, int indexOfTexSetToRenders, ShaderTextureProperty texPropertyname, MB3_TextureCombinerNonTextureProperties resultMaterialTextureBlender, bool isNormalMap, bool fixOutOfBoundsUVs, bool considerNonTextureProperties, MB3_TextureCombiner texCombiner, MB2_LogLevel LOG_LEV)
	{
		LOG_LEVEL = LOG_LEV;
		textureSets = textureSetss;
		indexOfTexSetToRender = indexOfTexSetToRenders;
		_texPropertyName = texPropertyname;
		_padding = padding;
		_isNormalMap = isNormalMap;
		_fixOutOfBoundsUVs = fixOutOfBoundsUVs;
		_resultMaterialTextureBlender = resultMaterialTextureBlender;
		rs = rss;
		Shader shader = ((!_isNormalMap) ? Shader.Find("MeshBaker/AlbedoShader") : ((!MBVersion.IsSwizzledNormalMapPlatform()) ? Shader.Find("MeshBaker/AlbedoShader") : Shader.Find("MeshBaker/NormalMapShaderSwizzle")));
		if (shader == null)
		{
			UnityEngine.Debug.LogError("Could not find shader for RenderTexture. Try reimporting mesh baker");
			return null;
		}
		mat = new Material(shader);
		RenderTextureReadWrite readWrite = ((!texPropertyname.isGammaCorrected) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
		_destinationTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, readWrite);
		_destinationTexture.filterMode = FilterMode.Point;
		myCamera = gameObject.GetComponent<Camera>();
		myCamera.orthographic = true;
		myCamera.orthographicSize = height >> 1;
		myCamera.aspect = (float)width / (float)height;
		myCamera.targetTexture = _destinationTexture;
		myCamera.clearFlags = CameraClearFlags.Color;
		Transform component = myCamera.GetComponent<Transform>();
		component.localPosition = new Vector3((float)width / 2f, (float)height / 2f, 3f);
		component.localRotation = Quaternion.Euler(0f, 180f, 180f);
		_doRenderAtlas = true;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log(string.Format("Begin Camera.Render destTex w={0} h={1} camPos={2} camSize={3} camAspect={4}", width, height, component.localPosition, myCamera.orthographicSize, myCamera.aspect.ToString("f5")));
		}
		myCamera.Render();
		_doRenderAtlas = false;
		MB_Utility.Destroy(mat);
		MB_Utility.Destroy(_destinationTexture);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Finished Camera.Render ");
		}
		Texture2D texture2D = targTex;
		targTex = null;
		if (texture2D == null)
		{
			UnityEngine.Debug.LogError(" Generated atlas was null. This can happen when using HDRP. Try using the Texture Packer 'Mesh Baker Texture Packer Fast V2' ");
		}
		return texture2D;
	}

	public void OnRenderObject()
	{
		if (!_doRenderAtlas)
		{
			return;
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		bool yIsFlipped = YisFlipped(LOG_LEVEL);
		for (int i = 0; i < rs.Length; i++)
		{
			MeshBakerMaterialTexture meshBakerMaterialTexture = textureSets[i].ts[indexOfTexSetToRender];
			Texture2D texture2D = meshBakerMaterialTexture.GetTexture2D();
			if (LOG_LEVEL >= MB2_LogLevel.trace && texture2D != null)
			{
				string[] obj = new string[14]
				{
					"Added ",
					texture2D?.ToString(),
					" to atlas w=",
					texture2D.width.ToString(),
					" h=",
					texture2D.height.ToString(),
					" offset=",
					meshBakerMaterialTexture.matTilingRect.min.ToString(),
					" scale=",
					meshBakerMaterialTexture.matTilingRect.size.ToString(),
					" rect=",
					null,
					null,
					null
				};
				Rect rect = rs[i];
				obj[11] = rect.ToString();
				obj[12] = " padding=";
				obj[13] = _padding.ToString();
				UnityEngine.Debug.Log(string.Concat(obj));
			}
			CopyScaledAndTiledToAtlas(textureSets[i], meshBakerMaterialTexture, textureSets[i].obUVoffset, textureSets[i].obUVscale, rs[i], _texPropertyName, _resultMaterialTextureBlender, yIsFlipped);
		}
		stopwatch.Stop();
		stopwatch.Start();
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Total time for Graphics.DrawTexture calls " + stopwatch.ElapsedMilliseconds.ToString("f5"));
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Copying RenderTexture to Texture2D. destW" + _destinationTexture.width + " destH" + _destinationTexture.height);
		}
		Texture2D tempTexture = new Texture2D(_destinationTexture.width, _destinationTexture.height, TextureFormat.ARGB32, mipChain: true, !_texPropertyName.isGammaCorrected);
		ConvertRenderTextureToTexture2D(_destinationTexture, yIsFlipped, doLinearColorSpace: false, LOG_LEVEL, tempTexture);
		myCamera.targetTexture = null;
		targTex = tempTexture;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Total time to copy RenderTexture to Texture2D " + stopwatch.ElapsedMilliseconds.ToString("f5"));
		}
	}

	public static void ConvertRenderTextureToTexture2D(RenderTexture _destinationTexture, bool yIsFlipped, bool doLinearColorSpace, MB2_LogLevel LOG_LEVEL, Texture2D tempTexture)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _destinationTexture;
		int num = Mathf.CeilToInt((float)_destinationTexture.width / 512f);
		int num2 = Mathf.CeilToInt((float)_destinationTexture.height / 512f);
		if (num == 0 || num2 == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("Copying all in one shot");
			}
			tempTexture.ReadPixels(new Rect(0f, 0f, _destinationTexture.width, _destinationTexture.height), 0, 0, recalculateMipMaps: true);
		}
		else
		{
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("yIsFlipped copying blocks");
			}
			if (!yIsFlipped)
			{
				for (int i = 0; i < num; i++)
				{
					for (int j = 0; j < num2; j++)
					{
						int num3 = i * 512;
						int num4 = j * 512;
						Rect source = new Rect(num3, num4, 512f, 512f);
						tempTexture.ReadPixels(source, i * 512, j * 512, recalculateMipMaps: true);
					}
				}
			}
			else
			{
				for (int k = 0; k < num; k++)
				{
					for (int l = 0; l < num2; l++)
					{
						int num5 = k * 512;
						int num6 = _destinationTexture.height - 512 - l * 512;
						Rect source2 = new Rect(num5, num6, 512f, 512f);
						tempTexture.ReadPixels(source2, k * 512, l * 512, recalculateMipMaps: true);
					}
				}
			}
		}
		RenderTexture.active = active;
		tempTexture.Apply();
		if (LOG_LEVEL >= MB2_LogLevel.trace && tempTexture.height <= 16 && tempTexture.width <= 16)
		{
			_printTexture(tempTexture);
		}
	}

	private Color32 ConvertNormalFormatFromUnity_ToStandard(Color32 c)
	{
		Vector3 zero = Vector3.zero;
		zero.x = (float)(int)c.a * 2f - 1f;
		zero.y = (float)(int)c.g * 2f - 1f;
		zero.z = Mathf.Sqrt(1f - zero.x * zero.x - zero.y * zero.y);
		Color32 result = default(Color32);
		result.a = 1;
		result.r = (byte)((zero.x + 1f) * 0.5f);
		result.g = (byte)((zero.y + 1f) * 0.5f);
		result.b = (byte)((zero.z + 1f) * 0.5f);
		return result;
	}

	public static bool YisFlipped(MB2_LogLevel LOG_LEVEL)
	{
		bool result = (MBVersion.GraphicsUVStartsAtTop() ? true : false);
		if (LOG_LEVEL == MB2_LogLevel.debug)
		{
			string text = SystemInfo.graphicsDeviceVersion.ToLower();
			UnityEngine.Debug.Log("Graphics device version is: " + text + " flipY:" + result);
		}
		return result;
	}

	private void CopyScaledAndTiledToAtlas(MB_TexSet texSet, MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale, Rect rec, ShaderTextureProperty texturePropertyName, MB3_TextureCombinerNonTextureProperties resultMatTexBlender, bool yIsFlipped)
	{
		Rect rect = rec;
		myCamera.backgroundColor = resultMatTexBlender.GetColorForTemporaryTexture(texSet.matsAndGOs.mats[0].mat, texturePropertyName);
		rect.y = 1f - (rect.y + rect.height);
		rect.x *= _destinationTexture.width;
		rect.y *= _destinationTexture.height;
		rect.width *= _destinationTexture.width;
		rect.height *= _destinationTexture.height;
		Rect rect2 = rect;
		rect2.x -= _padding;
		rect2.y -= _padding;
		rect2.width += _padding * 2;
		rect2.height += _padding * 2;
		Rect screenRect = default(Rect);
		Rect rect3 = texSet.ts[indexOfTexSetToRender].GetEncapsulatingSamplingRect().GetRect();
		Texture2D texture2D = source.GetTexture2D();
		TextureWrapMode wrapMode = texture2D.wrapMode;
		if (rect3.width == 1f && rect3.height == 1f && rect3.x == 0f && rect3.y == 0f)
		{
			texture2D.wrapMode = TextureWrapMode.Clamp;
		}
		else
		{
			texture2D.wrapMode = TextureWrapMode.Repeat;
		}
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			string[] obj = new string[8] { "DrawTexture tex=", texture2D.name, " destRect=", null, null, null, null, null };
			Rect rect4 = rect;
			obj[3] = rect4.ToString();
			obj[4] = " srcRect=";
			rect4 = rect3;
			obj[5] = rect4.ToString();
			obj[6] = " Mat=";
			obj[7] = mat?.ToString();
			UnityEngine.Debug.Log(string.Concat(obj));
		}
		Rect sourceRect = default(Rect);
		sourceRect.x = rect3.x;
		sourceRect.y = rect3.y + 1f - 1f / (float)texture2D.height;
		sourceRect.width = rect3.width;
		sourceRect.height = 1f / (float)texture2D.height;
		screenRect.x = rect.x;
		screenRect.y = rect2.y;
		screenRect.width = rect.width;
		screenRect.height = _padding;
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _destinationTexture;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = rect3.x;
		sourceRect.y = rect3.y;
		sourceRect.width = rect3.width;
		sourceRect.height = 1f / (float)texture2D.height;
		screenRect.x = rect.x;
		screenRect.y = rect.y + rect.height;
		screenRect.width = rect.width;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = rect3.x;
		sourceRect.y = rect3.y;
		sourceRect.width = 1f / (float)texture2D.width;
		sourceRect.height = rect3.height;
		screenRect.x = rect2.x;
		screenRect.y = rect.y;
		screenRect.width = _padding;
		screenRect.height = rect.height;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = rect3.x + 1f - 1f / (float)texture2D.width;
		sourceRect.y = rect3.y;
		sourceRect.width = 1f / (float)texture2D.width;
		sourceRect.height = rect3.height;
		screenRect.x = rect.x + rect.width;
		screenRect.y = rect.y;
		screenRect.width = _padding;
		screenRect.height = rect.height;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = rect3.x;
		sourceRect.y = rect3.y + 1f - 1f / (float)texture2D.height;
		sourceRect.width = 1f / (float)texture2D.width;
		sourceRect.height = 1f / (float)texture2D.height;
		screenRect.x = rect2.x;
		screenRect.y = rect2.y;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = rect3.x + 1f - 1f / (float)texture2D.width;
		sourceRect.y = rect3.y + 1f - 1f / (float)texture2D.height;
		sourceRect.width = 1f / (float)texture2D.width;
		sourceRect.height = 1f / (float)texture2D.height;
		screenRect.x = rect.x + rect.width;
		screenRect.y = rect2.y;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = rect3.x;
		sourceRect.y = rect3.y;
		sourceRect.width = 1f / (float)texture2D.width;
		sourceRect.height = 1f / (float)texture2D.height;
		screenRect.x = rect2.x;
		screenRect.y = rect.y + rect.height;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = rect3.x + 1f - 1f / (float)texture2D.width;
		sourceRect.y = rect3.y;
		sourceRect.width = 1f / (float)texture2D.width;
		sourceRect.height = 1f / (float)texture2D.height;
		screenRect.x = rect.x + rect.width;
		screenRect.y = rect.y + rect.height;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, texture2D, sourceRect, 0, 0, 0, 0, mat);
		Graphics.DrawTexture(rect, texture2D, rect3, 0, 0, 0, 0, mat);
		RenderTexture.active = active;
		texture2D.wrapMode = wrapMode;
	}

	private static void _printTexture(Texture2D t)
	{
		if (t.width * t.height > 100)
		{
			UnityEngine.Debug.Log("Not printing texture too large.");
			return;
		}
		try
		{
			Color32[] pixels = t.GetPixels32();
			string text = "";
			for (int i = 0; i < t.height; i++)
			{
				for (int j = 0; j < t.width; j++)
				{
					string text2 = text;
					Color32 color = pixels[i * t.width + j];
					text = text2 + color.ToString() + ", ";
				}
				text += "\n";
			}
			UnityEngine.Debug.Log(text);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log("Could not print texture. texture may not be readable." + ex.Message + "\n" + ex.StackTrace.ToString());
		}
	}
}
