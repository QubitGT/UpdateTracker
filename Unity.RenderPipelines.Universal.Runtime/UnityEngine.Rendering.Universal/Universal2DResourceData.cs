using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class Universal2DResourceData : UniversalResourceDataBase
{
	private TextureHandle[][] _lightTextures = new TextureHandle[0][];

	private TextureHandle[] _cameraNormalsTexture = new TextureHandle[0];

	private TextureHandle _normalsDepth;

	private TextureHandle[][] _shadowTextures = new TextureHandle[0][];

	private TextureHandle _shadowDepth;

	private TextureHandle _upscaleTexture;

	private TextureHandle _cameraSortingLayerTexture;

	internal TextureHandle[][] lightTextures
	{
		get
		{
			return CheckAndGetTextureHandle(ref _lightTextures);
		}
		set
		{
			CheckAndSetTextureHandle(ref _lightTextures, value);
		}
	}

	internal TextureHandle[] normalsTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _cameraNormalsTexture);
		}
		set
		{
			CheckAndSetTextureHandle(ref _cameraNormalsTexture, value);
		}
	}

	internal TextureHandle normalsDepth
	{
		get
		{
			return CheckAndGetTextureHandle(ref _normalsDepth);
		}
		set
		{
			CheckAndSetTextureHandle(ref _normalsDepth, value);
		}
	}

	internal TextureHandle[][] shadowTextures
	{
		get
		{
			return CheckAndGetTextureHandle(ref _shadowTextures);
		}
		set
		{
			CheckAndSetTextureHandle(ref _shadowTextures, value);
		}
	}

	internal TextureHandle shadowDepth
	{
		get
		{
			return CheckAndGetTextureHandle(ref _shadowDepth);
		}
		set
		{
			CheckAndSetTextureHandle(ref _shadowDepth, value);
		}
	}

	internal TextureHandle upscaleTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _upscaleTexture);
		}
		set
		{
			CheckAndSetTextureHandle(ref _upscaleTexture, value);
		}
	}

	internal TextureHandle cameraSortingLayerTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _cameraSortingLayerTexture);
		}
		set
		{
			CheckAndSetTextureHandle(ref _cameraSortingLayerTexture, value);
		}
	}

	private TextureHandle[][] CheckAndGetTextureHandle(ref TextureHandle[][] handle)
	{
		if (!CheckAndWarnAboutAccessibility())
		{
			return new TextureHandle[1][] { new TextureHandle[1] { TextureHandle.nullHandle } };
		}
		return handle;
	}

	private void CheckAndSetTextureHandle(ref TextureHandle[][] handle, TextureHandle[][] newHandle)
	{
		if (CheckAndWarnAboutAccessibility())
		{
			if (handle == null || handle.Length != newHandle.Length)
			{
				handle = new TextureHandle[newHandle.Length][];
			}
			for (int i = 0; i < newHandle.Length; i++)
			{
				handle[i] = newHandle[i];
			}
		}
	}

	public override void Reset()
	{
		_normalsDepth = TextureHandle.nullHandle;
		_shadowDepth = TextureHandle.nullHandle;
		_upscaleTexture = TextureHandle.nullHandle;
		_cameraSortingLayerTexture = TextureHandle.nullHandle;
		for (int i = 0; i < _cameraNormalsTexture.Length; i++)
		{
			_cameraNormalsTexture[i] = TextureHandle.nullHandle;
		}
		for (int j = 0; j < _shadowTextures.Length; j++)
		{
			for (int k = 0; k < _shadowTextures[j].Length; k++)
			{
				_shadowTextures[j][k] = TextureHandle.nullHandle;
			}
		}
		for (int l = 0; l < _lightTextures.Length; l++)
		{
			for (int m = 0; m < _lightTextures[l].Length; m++)
			{
				_lightTextures[l][m] = TextureHandle.nullHandle;
			}
		}
	}
}
