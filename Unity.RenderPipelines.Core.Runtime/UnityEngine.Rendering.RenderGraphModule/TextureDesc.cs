using System;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.RenderGraphModule;

public struct TextureDesc
{
	public TextureSizeMode sizeMode;

	public int width;

	public int height;

	public int slices;

	public Vector2 scale;

	public ScaleFunc func;

	public GraphicsFormat format;

	public FilterMode filterMode;

	public TextureWrapMode wrapMode;

	public TextureDimension dimension;

	public bool enableRandomWrite;

	public bool useMipMap;

	public bool autoGenerateMips;

	public bool isShadowMap;

	public int anisoLevel;

	public float mipMapBias;

	public MSAASamples msaaSamples;

	public bool bindTextureMS;

	public bool useDynamicScale;

	public bool useDynamicScaleExplicit;

	public RenderTextureMemoryless memoryless;

	public VRTextureUsage vrUsage;

	public bool enableShadingRate;

	public string name;

	public FastMemoryDesc fastMemoryDesc;

	public bool fallBackToBlackTexture;

	public bool disableFallBackToImportedTexture;

	public bool clearBuffer;

	public Color clearColor;

	public bool discardBuffer;

	public DepthBits depthBufferBits
	{
		get
		{
			return (DepthBits)GraphicsFormatUtility.GetDepthBits(format);
		}
		set
		{
			if (value == DepthBits.None)
			{
				if (GraphicsFormatUtility.IsDepthStencilFormat(format))
				{
					format = GraphicsFormat.None;
				}
			}
			else
			{
				format = GraphicsFormatUtility.GetDepthStencilFormat((int)value);
			}
		}
	}

	public GraphicsFormat colorFormat
	{
		get
		{
			if (!GraphicsFormatUtility.IsDepthStencilFormat(format))
			{
				return format;
			}
			return GraphicsFormat.None;
		}
		set
		{
			format = value;
		}
	}

	private void InitDefaultValues(bool dynamicResolution, bool xrReady)
	{
		useDynamicScale = dynamicResolution;
		vrUsage = VRTextureUsage.None;
		if (xrReady)
		{
			slices = TextureXR.slices;
			dimension = TextureXR.dimension;
		}
		else
		{
			slices = 1;
			dimension = TextureDimension.Tex2D;
		}
		discardBuffer = false;
	}

	public TextureDesc(int width, int height, bool dynamicResolution = false, bool xrReady = false)
	{
		this = default(TextureDesc);
		sizeMode = TextureSizeMode.Explicit;
		this.width = width;
		this.height = height;
		msaaSamples = MSAASamples.None;
		InitDefaultValues(dynamicResolution, xrReady);
	}

	public TextureDesc(Vector2 scale, bool dynamicResolution = false, bool xrReady = false)
	{
		this = default(TextureDesc);
		sizeMode = TextureSizeMode.Scale;
		this.scale = scale;
		msaaSamples = MSAASamples.None;
		dimension = TextureDimension.Tex2D;
		InitDefaultValues(dynamicResolution, xrReady);
	}

	public TextureDesc(ScaleFunc func, bool dynamicResolution = false, bool xrReady = false)
	{
		this = default(TextureDesc);
		sizeMode = TextureSizeMode.Functor;
		this.func = func;
		msaaSamples = MSAASamples.None;
		dimension = TextureDimension.Tex2D;
		InitDefaultValues(dynamicResolution, xrReady);
	}

	public TextureDesc(TextureDesc input)
	{
		this = input;
	}

	public TextureDesc(RenderTextureDescriptor input)
	{
		sizeMode = TextureSizeMode.Explicit;
		width = input.width;
		height = input.height;
		slices = input.volumeDepth;
		scale = Vector2.one;
		func = null;
		format = ((input.depthStencilFormat != 0) ? input.depthStencilFormat : input.graphicsFormat);
		filterMode = FilterMode.Bilinear;
		wrapMode = TextureWrapMode.Clamp;
		dimension = input.dimension;
		enableRandomWrite = input.enableRandomWrite;
		useMipMap = input.useMipMap;
		autoGenerateMips = input.autoGenerateMips;
		isShadowMap = input.shadowSamplingMode != ShadowSamplingMode.None;
		anisoLevel = 1;
		mipMapBias = 0f;
		msaaSamples = (MSAASamples)input.msaaSamples;
		bindTextureMS = input.bindMS;
		useDynamicScale = input.useDynamicScale;
		useDynamicScaleExplicit = false;
		memoryless = input.memoryless;
		vrUsage = input.vrUsage;
		name = "UnNamedFromRenderTextureDescriptor";
		fastMemoryDesc = default(FastMemoryDesc);
		fastMemoryDesc.inFastMemory = false;
		fallBackToBlackTexture = false;
		disableFallBackToImportedTexture = true;
		clearBuffer = true;
		clearColor = Color.black;
		discardBuffer = false;
		enableShadingRate = input.enableShadingRate;
	}

	public TextureDesc(RenderTexture input)
		: this(input.descriptor)
	{
		filterMode = input.filterMode;
		wrapMode = input.wrapMode;
		anisoLevel = input.anisoLevel;
		mipMapBias = input.mipMapBias;
		name = "UnNamedFromRenderTextureDescriptor";
	}

	public override int GetHashCode()
	{
		HashFNV1A32 hashFNV1A = HashFNV1A32.Create();
		int input;
		switch (sizeMode)
		{
		case TextureSizeMode.Explicit:
			hashFNV1A.Append(in width);
			hashFNV1A.Append(in height);
			break;
		case TextureSizeMode.Functor:
			if (func != null)
			{
				input = DelegateHashCodeUtils.GetFuncHashCode(func);
				hashFNV1A.Append(in input);
			}
			break;
		case TextureSizeMode.Scale:
			hashFNV1A.Append(in scale);
			break;
		}
		hashFNV1A.Append(in mipMapBias);
		hashFNV1A.Append(in slices);
		input = (int)format;
		hashFNV1A.Append(in input);
		input = (int)filterMode;
		hashFNV1A.Append(in input);
		input = (int)wrapMode;
		hashFNV1A.Append(in input);
		input = (int)dimension;
		hashFNV1A.Append(in input);
		input = (int)memoryless;
		hashFNV1A.Append(in input);
		input = (int)vrUsage;
		hashFNV1A.Append(in input);
		hashFNV1A.Append(in anisoLevel);
		hashFNV1A.Append(in enableRandomWrite);
		hashFNV1A.Append(in useMipMap);
		hashFNV1A.Append(in autoGenerateMips);
		hashFNV1A.Append(in isShadowMap);
		hashFNV1A.Append(in bindTextureMS);
		hashFNV1A.Append(in useDynamicScale);
		input = (int)msaaSamples;
		hashFNV1A.Append(in input);
		hashFNV1A.Append(in fastMemoryDesc.inFastMemory);
		hashFNV1A.Append(in enableShadingRate);
		return hashFNV1A.value;
	}

	public Vector2Int CalculateFinalDimensions()
	{
		return sizeMode switch
		{
			TextureSizeMode.Explicit => new Vector2Int(width, height), 
			TextureSizeMode.Scale => RTHandles.CalculateDimensions(scale), 
			TextureSizeMode.Functor => RTHandles.CalculateDimensions(func), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
