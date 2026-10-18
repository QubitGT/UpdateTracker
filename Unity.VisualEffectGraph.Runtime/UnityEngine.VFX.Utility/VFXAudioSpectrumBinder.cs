using System;
using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Audio Spectrum Binder")]
[VFXBinder("Audio/Audio Spectrum to AttributeMap")]
internal class VFXAudioSpectrumBinder : VFXBinderBase
{
	public enum AudioSourceMode
	{
		AudioSource,
		AudioListener
	}

	[VFXPropertyBinding(new string[] { "System.UInt32" })]
	[SerializeField]
	[FormerlySerializedAs("m_CountParameter")]
	protected ExposedProperty m_CountProperty = "Count";

	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	[SerializeField]
	[FormerlySerializedAs("m_TextureParameter")]
	protected ExposedProperty m_TextureProperty = "SpectrumTexture";

	public FFTWindow FFTWindow = FFTWindow.BlackmanHarris;

	public uint Samples = 64u;

	public AudioSourceMode Mode;

	public AudioSource AudioSource;

	private Texture2D m_Texture;

	private float[] m_AudioCache;

	private Color[] m_ColorCache;

	public string CountProperty
	{
		get
		{
			return (string)m_CountProperty;
		}
		set
		{
			m_CountProperty = value;
		}
	}

	public string TextureProperty
	{
		get
		{
			return (string)m_TextureProperty;
		}
		set
		{
			m_TextureProperty = value;
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		bool num = Mode != 0 || AudioSource != null;
		bool flag = component.HasTexture(TextureProperty);
		bool flag2 = component.HasUInt(CountProperty);
		return num && flag && flag2;
	}

	private void UpdateTexture()
	{
		if (m_Texture == null || m_Texture.width != Samples)
		{
			m_Texture = new Texture2D((int)Samples, 1, TextureFormat.RFloat, mipChain: false);
			m_AudioCache = new float[Samples];
			m_ColorCache = new Color[Samples];
		}
		if (Mode == AudioSourceMode.AudioListener)
		{
			AudioListener.GetSpectrumData(m_AudioCache, 0, FFTWindow);
		}
		else
		{
			if (Mode != 0)
			{
				throw new NotImplementedException();
			}
			AudioSource.GetSpectrumData(m_AudioCache, 0, FFTWindow);
		}
		for (int i = 0; i < Samples; i++)
		{
			m_ColorCache[i] = new Color(m_AudioCache[i], 0f, 0f, 0f);
		}
		m_Texture.SetPixels(m_ColorCache);
		m_Texture.name = "AudioSpectrum" + Samples;
		m_Texture.Apply();
	}

	public override void UpdateBinding(VisualEffect component)
	{
		UpdateTexture();
		component.SetTexture(TextureProperty, m_Texture);
		component.SetUInt(CountProperty, Samples);
	}

	public override string ToString()
	{
		return string.Format("Audio Spectrum : '{0} samples' -> {1}", m_CountProperty, (Mode == AudioSourceMode.AudioSource) ? "AudioSource" : "AudioListener");
	}
}
