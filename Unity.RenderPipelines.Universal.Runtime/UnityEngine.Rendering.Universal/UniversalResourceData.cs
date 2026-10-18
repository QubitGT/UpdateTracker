using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public class UniversalResourceData : UniversalResourceDataBase
{
	private TextureHandle _backBufferColor;

	private TextureHandle _backBufferDepth;

	private TextureHandle _cameraColor;

	private TextureHandle _cameraDepth;

	private TextureHandle _mainShadowsTexture;

	private TextureHandle _additionalShadowsTexture;

	private TextureHandle[] _gBuffer = new TextureHandle[7];

	private TextureHandle _cameraOpaqueTexture;

	private TextureHandle _cameraDepthTexture;

	private TextureHandle _cameraNormalsTexture;

	private TextureHandle _motionVectorColor;

	private TextureHandle _motionVectorDepth;

	private TextureHandle _internalColorLut;

	internal TextureHandle _debugScreenColor;

	internal TextureHandle _debugScreenDepth;

	private TextureHandle _afterPostProcessColor;

	private TextureHandle _overlayUITexture;

	private TextureHandle _renderingLayersTexture;

	private TextureHandle[] _dBuffer = new TextureHandle[3];

	private TextureHandle _dBufferDepth;

	private TextureHandle _ssaoTexture;

	private TextureHandle _stpDebugView;

	internal ActiveID activeColorID { get; set; }

	public TextureHandle activeColorTexture
	{
		get
		{
			if (!CheckAndWarnAboutAccessibility())
			{
				return TextureHandle.nullHandle;
			}
			return activeColorID switch
			{
				ActiveID.Camera => cameraColor, 
				ActiveID.BackBuffer => backBufferColor, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	internal ActiveID activeDepthID { get; set; }

	public TextureHandle activeDepthTexture
	{
		get
		{
			if (!CheckAndWarnAboutAccessibility())
			{
				return TextureHandle.nullHandle;
			}
			return activeDepthID switch
			{
				ActiveID.Camera => cameraDepth, 
				ActiveID.BackBuffer => backBufferDepth, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	public bool isActiveTargetBackBuffer
	{
		get
		{
			if (!base.isAccessible)
			{
				Debug.LogError("Trying to access frameData outside of the current frame setup.");
				return false;
			}
			return activeColorID == ActiveID.BackBuffer;
		}
	}

	public TextureHandle backBufferColor
	{
		get
		{
			return CheckAndGetTextureHandle(ref _backBufferColor);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _backBufferColor, value);
		}
	}

	public TextureHandle backBufferDepth
	{
		get
		{
			return CheckAndGetTextureHandle(ref _backBufferDepth);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _backBufferDepth, value);
		}
	}

	public TextureHandle cameraColor
	{
		get
		{
			return CheckAndGetTextureHandle(ref _cameraColor);
		}
		set
		{
			CheckAndSetTextureHandle(ref _cameraColor, value);
		}
	}

	public TextureHandle cameraDepth
	{
		get
		{
			return CheckAndGetTextureHandle(ref _cameraDepth);
		}
		set
		{
			CheckAndSetTextureHandle(ref _cameraDepth, value);
		}
	}

	public TextureHandle mainShadowsTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _mainShadowsTexture);
		}
		set
		{
			CheckAndSetTextureHandle(ref _mainShadowsTexture, value);
		}
	}

	public TextureHandle additionalShadowsTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _additionalShadowsTexture);
		}
		set
		{
			CheckAndSetTextureHandle(ref _additionalShadowsTexture, value);
		}
	}

	public TextureHandle[] gBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref _gBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref _gBuffer, value);
		}
	}

	public TextureHandle cameraOpaqueTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _cameraOpaqueTexture);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _cameraOpaqueTexture, value);
		}
	}

	public TextureHandle cameraDepthTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _cameraDepthTexture);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _cameraDepthTexture, value);
		}
	}

	public TextureHandle cameraNormalsTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _cameraNormalsTexture);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _cameraNormalsTexture, value);
		}
	}

	public TextureHandle motionVectorColor
	{
		get
		{
			return CheckAndGetTextureHandle(ref _motionVectorColor);
		}
		set
		{
			CheckAndSetTextureHandle(ref _motionVectorColor, value);
		}
	}

	public TextureHandle motionVectorDepth
	{
		get
		{
			return CheckAndGetTextureHandle(ref _motionVectorDepth);
		}
		set
		{
			CheckAndSetTextureHandle(ref _motionVectorDepth, value);
		}
	}

	public TextureHandle internalColorLut
	{
		get
		{
			return CheckAndGetTextureHandle(ref _internalColorLut);
		}
		set
		{
			CheckAndSetTextureHandle(ref _internalColorLut, value);
		}
	}

	internal TextureHandle debugScreenColor
	{
		get
		{
			return CheckAndGetTextureHandle(ref _debugScreenColor);
		}
		set
		{
			CheckAndSetTextureHandle(ref _debugScreenColor, value);
		}
	}

	internal TextureHandle debugScreenDepth
	{
		get
		{
			return CheckAndGetTextureHandle(ref _debugScreenDepth);
		}
		set
		{
			CheckAndSetTextureHandle(ref _debugScreenDepth, value);
		}
	}

	public TextureHandle afterPostProcessColor
	{
		get
		{
			return CheckAndGetTextureHandle(ref _afterPostProcessColor);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _afterPostProcessColor, value);
		}
	}

	public TextureHandle overlayUITexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _overlayUITexture);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _overlayUITexture, value);
		}
	}

	public TextureHandle renderingLayersTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _renderingLayersTexture);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _renderingLayersTexture, value);
		}
	}

	public TextureHandle[] dBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref _dBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref _dBuffer, value);
		}
	}

	public TextureHandle dBufferDepth
	{
		get
		{
			return CheckAndGetTextureHandle(ref _dBufferDepth);
		}
		set
		{
			CheckAndSetTextureHandle(ref _dBufferDepth, value);
		}
	}

	public TextureHandle ssaoTexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref _ssaoTexture);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref _ssaoTexture, value);
		}
	}

	internal TextureHandle stpDebugView
	{
		get
		{
			return CheckAndGetTextureHandle(ref _stpDebugView);
		}
		set
		{
			CheckAndSetTextureHandle(ref _stpDebugView, value);
		}
	}

	public override void Reset()
	{
		_backBufferColor = TextureHandle.nullHandle;
		_backBufferDepth = TextureHandle.nullHandle;
		_cameraColor = TextureHandle.nullHandle;
		_cameraDepth = TextureHandle.nullHandle;
		_mainShadowsTexture = TextureHandle.nullHandle;
		_additionalShadowsTexture = TextureHandle.nullHandle;
		_cameraOpaqueTexture = TextureHandle.nullHandle;
		_cameraDepthTexture = TextureHandle.nullHandle;
		_cameraNormalsTexture = TextureHandle.nullHandle;
		_motionVectorColor = TextureHandle.nullHandle;
		_motionVectorDepth = TextureHandle.nullHandle;
		_internalColorLut = TextureHandle.nullHandle;
		_debugScreenColor = TextureHandle.nullHandle;
		_debugScreenDepth = TextureHandle.nullHandle;
		_afterPostProcessColor = TextureHandle.nullHandle;
		_overlayUITexture = TextureHandle.nullHandle;
		_renderingLayersTexture = TextureHandle.nullHandle;
		_dBufferDepth = TextureHandle.nullHandle;
		_ssaoTexture = TextureHandle.nullHandle;
		_stpDebugView = TextureHandle.nullHandle;
		for (int i = 0; i < _gBuffer.Length; i++)
		{
			_gBuffer[i] = TextureHandle.nullHandle;
		}
		for (int j = 0; j < _dBuffer.Length; j++)
		{
			_dBuffer[j] = TextureHandle.nullHandle;
		}
	}
}
