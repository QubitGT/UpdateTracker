using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class TextureUtils
{
	public static void RenderTextureToTexture2D(RenderTexture renderTexture, Texture2D texture)
	{
		RenderTexture.active = renderTexture;
		texture.ReadPixels(new Rect(0f, 0f, texture.width, texture.height), 0, 0);
		texture.Apply();
	}
}
