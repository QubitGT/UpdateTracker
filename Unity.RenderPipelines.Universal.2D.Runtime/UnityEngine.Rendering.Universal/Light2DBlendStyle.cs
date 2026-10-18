using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.Universal", "Unity.RenderPipelines.Universal.Runtime", null)]
public struct Light2DBlendStyle
{
	internal enum TextureChannel
	{
		None,
		R,
		G,
		B,
		A,
		OneMinusR,
		OneMinusG,
		OneMinusB,
		OneMinusA
	}

	internal struct MaskChannelFilter
	{
		public Vector4 mask { get; private set; }

		public Vector4 inverted { get; private set; }

		public MaskChannelFilter(Vector4 m, Vector4 i)
		{
			mask = m;
			inverted = i;
		}
	}

	internal enum BlendMode
	{
		Additive,
		Multiply,
		Subtractive
	}

	[Serializable]
	internal struct BlendFactors
	{
		public float multiplicative;

		public float additive;
	}

	public string name;

	[SerializeField]
	internal TextureChannel maskTextureChannel;

	[SerializeField]
	internal BlendMode blendMode;

	internal int renderTargetHandleId;

	internal RTHandle renderTargetHandle;

	internal Vector2 blendFactors
	{
		get
		{
			Vector2 result = default(Vector2);
			switch (blendMode)
			{
			case BlendMode.Additive:
				result.x = 0f;
				result.y = 1f;
				break;
			case BlendMode.Multiply:
				result.x = 1f;
				result.y = 0f;
				break;
			case BlendMode.Subtractive:
				result.x = 0f;
				result.y = -1f;
				break;
			default:
				result.x = 1f;
				result.y = 0f;
				break;
			}
			return result;
		}
	}

	internal MaskChannelFilter maskTextureChannelFilter => maskTextureChannel switch
	{
		TextureChannel.R => new MaskChannelFilter(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 0f, 0f, 0f)), 
		TextureChannel.OneMinusR => new MaskChannelFilter(new Vector4(1f, 0f, 0f, 0f), new Vector4(1f, 0f, 0f, 0f)), 
		TextureChannel.G => new MaskChannelFilter(new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 0f, 0f)), 
		TextureChannel.OneMinusG => new MaskChannelFilter(new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f)), 
		TextureChannel.B => new MaskChannelFilter(new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0f, 0f, 0f)), 
		TextureChannel.OneMinusB => new MaskChannelFilter(new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0f, 1f, 0f)), 
		TextureChannel.A => new MaskChannelFilter(new Vector4(0f, 0f, 0f, 1f), new Vector4(0f, 0f, 0f, 0f)), 
		TextureChannel.OneMinusA => new MaskChannelFilter(new Vector4(0f, 0f, 0f, 1f), new Vector4(0f, 0f, 0f, 1f)), 
		_ => new MaskChannelFilter(Vector4.zero, Vector4.zero), 
	};

	internal bool isDirty { get; set; }

	internal bool hasRenderTarget { get; set; }
}
