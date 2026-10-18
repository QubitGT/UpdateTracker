using System;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

[Serializable]
public class TextureGradient : IDisposable
{
	[SerializeField]
	private Gradient m_Gradient;

	private Texture2D m_Texture;

	private int m_RequestedTextureSize = -1;

	private bool m_IsTextureDirty;

	private bool m_Precise;

	[SerializeField]
	[HideInInspector]
	public GradientMode mode = GradientMode.PerceptualBlend;

	[SerializeField]
	[HideInInspector]
	public ColorSpace colorSpace = ColorSpace.Uninitialized;

	[field: SerializeField]
	[field: HideInInspector]
	public int textureSize { get; private set; }

	[HideInInspector]
	public GradientColorKey[] colorKeys => m_Gradient?.colorKeys;

	[HideInInspector]
	public GradientAlphaKey[] alphaKeys => m_Gradient?.alphaKeys;

	public TextureGradient(Gradient baseCurve)
		: this(baseCurve.colorKeys, baseCurve.alphaKeys)
	{
		mode = baseCurve.mode;
		colorSpace = baseCurve.colorSpace;
		m_Gradient.mode = baseCurve.mode;
		m_Gradient.colorSpace = baseCurve.colorSpace;
	}

	public TextureGradient(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys, GradientMode mode = GradientMode.PerceptualBlend, ColorSpace colorSpace = ColorSpace.Uninitialized, int requestedTextureSize = -1, bool precise = false)
	{
		Rebuild(colorKeys, alphaKeys, mode, colorSpace, requestedTextureSize, precise);
	}

	private void Rebuild(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys, GradientMode mode, ColorSpace colorSpace, int requestedTextureSize, bool precise)
	{
		m_Gradient = new Gradient();
		m_Gradient.mode = mode;
		m_Gradient.colorSpace = colorSpace;
		m_Gradient.SetKeys(colorKeys, alphaKeys);
		m_Precise = precise;
		m_RequestedTextureSize = requestedTextureSize;
		if (requestedTextureSize > 0)
		{
			textureSize = requestedTextureSize;
		}
		else
		{
			float num = 1f;
			float[] array = new float[colorKeys.Length + alphaKeys.Length];
			for (int i = 0; i < colorKeys.Length; i++)
			{
				array[i] = colorKeys[i].time;
			}
			for (int j = 0; j < alphaKeys.Length; j++)
			{
				array[colorKeys.Length + j] = alphaKeys[j].time;
			}
			Array.Sort(array);
			for (int k = 1; k < array.Length; k++)
			{
				int num2 = Math.Max(k - 1, 0);
				int num3 = Math.Min(k, array.Length - 1);
				float num4 = Mathf.Abs(array[num2] - array[num3]);
				if (num4 > 0f && num4 < num)
				{
					num = num4;
				}
			}
			float num5 = ((!precise && mode != GradientMode.Fixed) ? 2f : 4f);
			float f = num5 * Mathf.Ceil(1f / num + 1f);
			textureSize = Mathf.RoundToInt(f);
			textureSize = Math.Min(textureSize, 1024);
		}
		SetDirty();
	}

	public void Dispose()
	{
	}

	public void Release()
	{
		if (m_Texture != null)
		{
			CoreUtils.Destroy(m_Texture);
		}
		m_Texture = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDirty()
	{
		m_IsTextureDirty = true;
	}

	private static GraphicsFormat GetTextureFormat()
	{
		return GraphicsFormat.R8G8B8A8_UNorm;
	}

	public Texture2D GetTexture()
	{
		float num = 1f / (float)(textureSize - 1);
		if (m_Texture != null && m_Texture.width != textureSize)
		{
			Object.DestroyImmediate(m_Texture);
			m_Texture = null;
		}
		if (m_Texture == null)
		{
			m_Texture = new Texture2D(textureSize, 1, GetTextureFormat(), TextureCreationFlags.None);
			m_Texture.name = "GradientTexture";
			m_Texture.hideFlags = HideFlags.HideAndDontSave;
			m_Texture.filterMode = FilterMode.Bilinear;
			m_Texture.wrapMode = TextureWrapMode.Clamp;
			m_Texture.anisoLevel = 0;
			m_IsTextureDirty = true;
		}
		if (m_IsTextureDirty)
		{
			Color[] array = new Color[textureSize];
			for (int i = 0; i < textureSize; i++)
			{
				array[i] = Evaluate((float)i * num);
			}
			m_Texture.SetPixels(array);
			m_Texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
			m_IsTextureDirty = false;
			m_Texture.IncrementUpdateCount();
		}
		return m_Texture;
	}

	public Color Evaluate(float time)
	{
		if (textureSize <= 0)
		{
			return Color.black;
		}
		return m_Gradient.Evaluate(time);
	}

	public void SetKeys(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys, GradientMode mode, ColorSpace colorSpace)
	{
		m_Gradient.SetKeys(colorKeys, alphaKeys);
		m_Gradient.mode = mode;
		m_Gradient.colorSpace = colorSpace;
		Rebuild(colorKeys, alphaKeys, mode, colorSpace, m_RequestedTextureSize, m_Precise);
	}
}
