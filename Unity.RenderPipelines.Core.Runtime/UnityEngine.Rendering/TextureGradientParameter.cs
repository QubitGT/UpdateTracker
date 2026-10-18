using System;

namespace UnityEngine.Rendering;

[Serializable]
public class TextureGradientParameter : VolumeParameter<TextureGradient>
{
	public TextureGradientParameter(TextureGradient value, bool overrideState = false)
		: base(value, overrideState)
	{
	}

	public override void Release()
	{
		m_Value.Release();
	}
}
