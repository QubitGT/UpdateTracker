using System;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

public static class RTHandles
{
	private static RTHandleSystem s_DefaultInstance = new RTHandleSystem();

	public static int maxWidth => s_DefaultInstance.GetMaxWidth();

	public static int maxHeight => s_DefaultInstance.GetMaxHeight();

	public static RTHandleProperties rtHandleProperties => s_DefaultInstance.rtHandleProperties;

	public static Vector2Int CalculateDimensions(Vector2 scaleFactor)
	{
		return s_DefaultInstance.CalculateDimensions(scaleFactor);
	}

	public static Vector2Int CalculateDimensions(ScaleFunc scaleFunc)
	{
		return s_DefaultInstance.CalculateDimensions(scaleFunc);
	}

	public static RTHandle Alloc(int width, int height, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(width, height, slices, depthBufferBits, colorFormat, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(int width, int height, GraphicsFormat format, int slices = 1, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(width, height, format, slices, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(int width, int height, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV, TextureWrapMode wrapModeW = TextureWrapMode.Repeat, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(width, height, wrapModeU, wrapModeV, wrapModeW, slices, depthBufferBits, colorFormat, filterMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(int width, int height, RTHandleAllocInfo info)
	{
		return s_DefaultInstance.Alloc(width, height, info);
	}

	public static RTHandle Alloc(in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		return s_DefaultInstance.Alloc(descriptor.width, descriptor.height, GetRTHandleAllocInfo(in descriptor, filterMode, wrapMode, anisoLevel, mipMapBias, name));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static GraphicsFormat GetFormat(GraphicsFormat colorFormat, GraphicsFormat depthStencilFormat)
	{
		if (depthStencilFormat != 0)
		{
			return depthStencilFormat;
		}
		return colorFormat;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static RTHandleAllocInfo GetRTHandleAllocInfo(in RenderTextureDescriptor desc, FilterMode filterMode, TextureWrapMode wrapMode, int anisoLevel, float mipMapBias, string name)
	{
		RTHandleAllocInfo result = new RTHandleAllocInfo(name);
		result.slices = desc.volumeDepth;
		result.format = GetFormat(desc.graphicsFormat, desc.depthStencilFormat);
		result.filterMode = filterMode;
		result.wrapModeU = wrapMode;
		result.wrapModeV = wrapMode;
		result.wrapModeW = wrapMode;
		result.dimension = desc.dimension;
		result.enableRandomWrite = desc.enableRandomWrite;
		result.useMipMap = desc.useMipMap;
		result.autoGenerateMips = desc.autoGenerateMips;
		result.isShadowMap = desc.shadowSamplingMode != ShadowSamplingMode.None;
		result.anisoLevel = anisoLevel;
		result.mipMapBias = mipMapBias;
		result.msaaSamples = (MSAASamples)desc.msaaSamples;
		result.bindTextureMS = desc.bindMS;
		result.useDynamicScale = desc.useDynamicScale;
		result.useDynamicScaleExplicit = desc.useDynamicScaleExplicit;
		result.memoryless = desc.memoryless;
		result.vrUsage = desc.vrUsage;
		result.enableShadingRate = desc.enableShadingRate;
		return result;
	}

	public static RTHandle Alloc(Vector2 scaleFactor, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFactor, slices, depthBufferBits, colorFormat, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(Vector2 scaleFactor, GraphicsFormat format, int slices = 1, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFactor, format, slices, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(Vector2 scaleFactor, in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFactor, GetRTHandleAllocInfo(in descriptor, filterMode, wrapMode, anisoLevel, mipMapBias, name));
	}

	public static RTHandle Alloc(Vector2 scaleFactor, RTHandleAllocInfo info)
	{
		return s_DefaultInstance.Alloc(scaleFactor, info);
	}

	public static RTHandle Alloc(ScaleFunc scaleFunc, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFunc, slices, depthBufferBits, colorFormat, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(ScaleFunc scaleFunc, GraphicsFormat format, int slices = 1, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFunc, format, slices, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(ScaleFunc scaleFunc, in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFunc, GetRTHandleAllocInfo(in descriptor, filterMode, wrapMode, anisoLevel, mipMapBias, name));
	}

	public static RTHandle Alloc(ScaleFunc scaleFunc, RTHandleAllocInfo info)
	{
		return s_DefaultInstance.Alloc(scaleFunc, info);
	}

	public static RTHandle Alloc(Texture tex)
	{
		return s_DefaultInstance.Alloc(tex);
	}

	public static RTHandle Alloc(RenderTexture tex, bool transferOwnership = false)
	{
		return s_DefaultInstance.Alloc(tex, transferOwnership);
	}

	public static RTHandle Alloc(RenderTargetIdentifier tex)
	{
		return s_DefaultInstance.Alloc(tex);
	}

	public static RTHandle Alloc(RenderTargetIdentifier tex, string name)
	{
		return s_DefaultInstance.Alloc(tex, name);
	}

	private static RTHandle Alloc(RTHandle tex)
	{
		Debug.LogError("Allocation a RTHandle from another one is forbidden.");
		return null;
	}

	public static void Initialize(int width, int height)
	{
		s_DefaultInstance.Initialize(width, height);
	}

	[Obsolete("useLegacyDynamicResControl is deprecated. Please use SetHardwareDynamicResolutionState() instead.")]
	public static void Initialize(int width, int height, bool useLegacyDynamicResControl = false)
	{
		s_DefaultInstance.Initialize(width, height, useLegacyDynamicResControl);
	}

	public static void Release(RTHandle rth)
	{
		s_DefaultInstance.Release(rth);
	}

	public static void SetHardwareDynamicResolutionState(bool hwDynamicResRequested)
	{
		s_DefaultInstance.SetHardwareDynamicResolutionState(hwDynamicResRequested);
	}

	public static void SetReferenceSize(int width, int height)
	{
		s_DefaultInstance.SetReferenceSize(width, height);
	}

	public static void ResetReferenceSize(int width, int height)
	{
		s_DefaultInstance.ResetReferenceSize(width, height);
	}

	public static Vector2 CalculateRatioAgainstMaxSize(int width, int height)
	{
		RTHandleSystem rTHandleSystem = s_DefaultInstance;
		Vector2Int viewportSize = new Vector2Int(width, height);
		return rTHandleSystem.CalculateRatioAgainstMaxSize(in viewportSize);
	}
}
