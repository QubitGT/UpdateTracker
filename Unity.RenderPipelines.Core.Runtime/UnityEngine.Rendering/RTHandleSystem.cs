using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

public class RTHandleSystem : IDisposable
{
	internal enum ResizeMode
	{
		Auto,
		OnDemand
	}

	private bool m_HardwareDynamicResRequested;

	private HashSet<RTHandle> m_AutoSizedRTs;

	private RTHandle[] m_AutoSizedRTsArray;

	private HashSet<RTHandle> m_ResizeOnDemandRTs;

	private RTHandleProperties m_RTHandleProperties;

	private int m_MaxWidths;

	private int m_MaxHeights;

	public RTHandleProperties rtHandleProperties => m_RTHandleProperties;

	public RTHandleSystem()
	{
		m_AutoSizedRTs = new HashSet<RTHandle>();
		m_ResizeOnDemandRTs = new HashSet<RTHandle>();
		m_MaxWidths = 1;
		m_MaxHeights = 1;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void Initialize(int width, int height)
	{
		if (m_AutoSizedRTs.Count != 0)
		{
			string arg = "Unreleased RTHandles:";
			foreach (RTHandle autoSizedRT in m_AutoSizedRTs)
			{
				arg = $"{arg}\n    {autoSizedRT.name}";
			}
			Debug.LogError($"RTHandleSystem.Initialize should only be called once before allocating any Render Texture. This may be caused by an unreleased RTHandle resource.\n{arg}\n");
		}
		m_MaxWidths = width;
		m_MaxHeights = height;
		m_HardwareDynamicResRequested = DynamicResolutionHandler.instance.RequestsHardwareDynamicResolution();
	}

	[Obsolete("useLegacyDynamicResControl is deprecated. Please use SetHardwareDynamicResolutionState() instead.")]
	public void Initialize(int width, int height, bool useLegacyDynamicResControl = false)
	{
		Initialize(width, height);
		if (useLegacyDynamicResControl)
		{
			m_HardwareDynamicResRequested = true;
		}
	}

	public void Release(RTHandle rth)
	{
		rth?.Release();
	}

	internal void Remove(RTHandle rth)
	{
		m_AutoSizedRTs.Remove(rth);
	}

	public void ResetReferenceSize(int width, int height)
	{
		m_MaxWidths = width;
		m_MaxHeights = height;
		SetReferenceSize(width, height, reset: true);
	}

	public void SetReferenceSize(int width, int height)
	{
		SetReferenceSize(width, height, reset: false);
	}

	public void SetReferenceSize(int width, int height, bool reset)
	{
		m_RTHandleProperties.previousViewportSize = m_RTHandleProperties.currentViewportSize;
		m_RTHandleProperties.previousRenderTargetSize = m_RTHandleProperties.currentRenderTargetSize;
		Vector2 vector = new Vector2(GetMaxWidth(), GetMaxHeight());
		width = Mathf.Max(width, 1);
		height = Mathf.Max(height, 1);
		bool flag = width > GetMaxWidth() || height > GetMaxHeight() || reset;
		if (flag)
		{
			Resize(width, height, flag);
		}
		m_RTHandleProperties.currentViewportSize = new Vector2Int(width, height);
		m_RTHandleProperties.currentRenderTargetSize = new Vector2Int(GetMaxWidth(), GetMaxHeight());
		if (m_RTHandleProperties.previousViewportSize.x == 0)
		{
			m_RTHandleProperties.previousViewportSize = m_RTHandleProperties.currentViewportSize;
			m_RTHandleProperties.previousRenderTargetSize = m_RTHandleProperties.currentRenderTargetSize;
			vector = new Vector2(GetMaxWidth(), GetMaxHeight());
		}
		Vector2 vector2 = CalculateRatioAgainstMaxSize(in m_RTHandleProperties.currentViewportSize);
		if (DynamicResolutionHandler.instance.HardwareDynamicResIsEnabled() && m_HardwareDynamicResRequested)
		{
			m_RTHandleProperties.rtHandleScale = new Vector4(vector2.x, vector2.y, m_RTHandleProperties.rtHandleScale.x, m_RTHandleProperties.rtHandleScale.y);
			return;
		}
		Vector2 vector3 = m_RTHandleProperties.previousViewportSize / vector;
		m_RTHandleProperties.rtHandleScale = new Vector4(vector2.x, vector2.y, vector3.x, vector3.y);
	}

	internal Vector2 CalculateRatioAgainstMaxSize(in Vector2Int viewportSize)
	{
		Vector2 vector = new Vector2(GetMaxWidth(), GetMaxHeight());
		if (DynamicResolutionHandler.instance.HardwareDynamicResIsEnabled() && m_HardwareDynamicResRequested && viewportSize != DynamicResolutionHandler.instance.finalViewport)
		{
			Vector2 scales = (Vector2)viewportSize / (Vector2)DynamicResolutionHandler.instance.finalViewport;
			vector = DynamicResolutionHandler.instance.ApplyScalesOnSize(new Vector2Int(GetMaxWidth(), GetMaxHeight()), scales);
		}
		return new Vector2((float)viewportSize.x / vector.x, (float)viewportSize.y / vector.y);
	}

	public void SetHardwareDynamicResolutionState(bool enableHWDynamicRes)
	{
		if (enableHWDynamicRes == m_HardwareDynamicResRequested)
		{
			return;
		}
		m_HardwareDynamicResRequested = enableHWDynamicRes;
		Array.Resize(ref m_AutoSizedRTsArray, m_AutoSizedRTs.Count);
		m_AutoSizedRTs.CopyTo(m_AutoSizedRTsArray);
		int i = 0;
		for (int num = m_AutoSizedRTsArray.Length; i < num; i++)
		{
			RTHandle rTHandle = m_AutoSizedRTsArray[i];
			RenderTexture rT = rTHandle.m_RT;
			if ((bool)rT)
			{
				rT.Release();
				rT.useDynamicScale = m_HardwareDynamicResRequested && rTHandle.m_EnableHWDynamicScale;
				rT.Create();
			}
		}
	}

	internal void SwitchResizeMode(RTHandle rth, ResizeMode mode)
	{
		if (!rth.useScaling)
		{
			return;
		}
		switch (mode)
		{
		case ResizeMode.OnDemand:
			m_AutoSizedRTs.Remove(rth);
			m_ResizeOnDemandRTs.Add(rth);
			break;
		case ResizeMode.Auto:
			if (m_ResizeOnDemandRTs.Contains(rth))
			{
				DemandResize(rth);
			}
			m_ResizeOnDemandRTs.Remove(rth);
			m_AutoSizedRTs.Add(rth);
			break;
		}
	}

	private void DemandResize(RTHandle rth)
	{
		RenderTexture rT = rth.m_RT;
		rth.referenceSize = new Vector2Int(m_MaxWidths, m_MaxHeights);
		Vector2Int scaledSize = rth.GetScaledSize(rth.referenceSize);
		scaledSize = Vector2Int.Max(Vector2Int.one, scaledSize);
		if (rT.width != scaledSize.x || rT.height != scaledSize.y)
		{
			rT.Release();
			rT.width = scaledSize.x;
			rT.height = scaledSize.y;
			rT.name = CoreUtils.GetRenderTargetAutoName(rT.width, rT.height, rT.volumeDepth, (rT.depthStencilFormat != 0) ? rT.depthStencilFormat : rT.graphicsFormat, rT.dimension, rth.m_Name, rT.useMipMap, rth.m_EnableMSAA, (MSAASamples)rT.antiAliasing, rT.useDynamicScale, rT.useDynamicScaleExplicit);
			rT.Create();
		}
	}

	public int GetMaxWidth()
	{
		return m_MaxWidths;
	}

	public int GetMaxHeight()
	{
		return m_MaxHeights;
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			Array.Resize(ref m_AutoSizedRTsArray, m_AutoSizedRTs.Count);
			m_AutoSizedRTs.CopyTo(m_AutoSizedRTsArray);
			int i = 0;
			for (int num = m_AutoSizedRTsArray.Length; i < num; i++)
			{
				RTHandle rth = m_AutoSizedRTsArray[i];
				Release(rth);
			}
			m_AutoSizedRTs.Clear();
			Array.Resize(ref m_AutoSizedRTsArray, m_ResizeOnDemandRTs.Count);
			m_ResizeOnDemandRTs.CopyTo(m_AutoSizedRTsArray);
			int j = 0;
			for (int num2 = m_AutoSizedRTsArray.Length; j < num2; j++)
			{
				RTHandle rth2 = m_AutoSizedRTsArray[j];
				Release(rth2);
			}
			m_ResizeOnDemandRTs.Clear();
			m_AutoSizedRTsArray = null;
		}
	}

	private void Resize(int width, int height, bool sizeChanged)
	{
		m_MaxWidths = Math.Max(width, m_MaxWidths);
		m_MaxHeights = Math.Max(height, m_MaxHeights);
		Vector2Int vector2Int = new Vector2Int(m_MaxWidths, m_MaxHeights);
		Array.Resize(ref m_AutoSizedRTsArray, m_AutoSizedRTs.Count);
		m_AutoSizedRTs.CopyTo(m_AutoSizedRTsArray);
		int i = 0;
		for (int num = m_AutoSizedRTsArray.Length; i < num; i++)
		{
			RTHandle rTHandle = m_AutoSizedRTsArray[i];
			rTHandle.referenceSize = vector2Int;
			RenderTexture rT = rTHandle.m_RT;
			rT.Release();
			Vector2Int scaledSize = rTHandle.GetScaledSize(vector2Int);
			rT.width = Mathf.Max(scaledSize.x, 1);
			rT.height = Mathf.Max(scaledSize.y, 1);
			rT.name = CoreUtils.GetRenderTargetAutoName(rT.width, rT.height, rT.volumeDepth, (rT.depthStencilFormat != 0) ? rT.depthStencilFormat : rT.graphicsFormat, rT.dimension, rTHandle.m_Name, rT.useMipMap, rTHandle.m_EnableMSAA, (MSAASamples)rT.antiAliasing, rT.useDynamicScale, rT.useDynamicScaleExplicit);
			rT.Create();
		}
	}

	public RTHandle Alloc(int width, int height, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		GraphicsFormat format = ((depthBufferBits != 0) ? GraphicsFormatUtility.GetDepthStencilFormat((int)depthBufferBits) : colorFormat);
		return Alloc(width, height, format, wrapMode, wrapMode, wrapMode, slices, filterMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public RTHandle Alloc(int width, int height, GraphicsFormat format, int slices = 1, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return Alloc(width, height, format, wrapMode, wrapMode, wrapMode, slices, filterMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public RTHandle Alloc(int width, int height, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV, TextureWrapMode wrapModeW = TextureWrapMode.Repeat, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		GraphicsFormat format = ((depthBufferBits != 0) ? GraphicsFormatUtility.GetDepthStencilFormat((int)depthBufferBits) : colorFormat);
		return Alloc(width, height, format, wrapModeU, wrapModeV, wrapModeW, slices, filterMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public RTHandle Alloc(int width, int height, GraphicsFormat format, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV, TextureWrapMode wrapModeW = TextureWrapMode.Repeat, int slices = 1, FilterMode filterMode = FilterMode.Point, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		RenderTexture rt = CreateRenderTexture(width, height, format, slices, filterMode, wrapModeU, wrapModeV, wrapModeW, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, enableShadingRate: false, name);
		RTHandle rTHandle = new RTHandle(this);
		rTHandle.SetRenderTexture(rt);
		rTHandle.useScaling = false;
		rTHandle.m_EnableRandomWrite = enableRandomWrite;
		rTHandle.m_EnableMSAA = msaaSamples != MSAASamples.None;
		rTHandle.m_EnableHWDynamicScale = useDynamicScale;
		rTHandle.m_Name = name;
		rTHandle.referenceSize = new Vector2Int(width, height);
		return rTHandle;
	}

	private RenderTexture CreateRenderTexture(int width, int height, GraphicsFormat format, int slices, FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV, TextureWrapMode wrapModeW, TextureDimension dimension, bool enableRandomWrite, bool useMipMap, bool autoGenerateMips, bool isShadowMap, int anisoLevel, float mipMapBias, MSAASamples msaaSamples, bool bindTextureMS, bool useDynamicScale, bool useDynamicScaleExplicit, RenderTextureMemoryless memoryless, VRTextureUsage vrUsage, bool enableShadingRate, string name)
	{
		bool flag = msaaSamples != MSAASamples.None;
		if (!flag && bindTextureMS)
		{
			Debug.LogWarning("RTHandle allocated without MSAA but with bindMS set to true, forcing bindMS to false.");
			bindTextureMS = false;
		}
		if (flag && enableRandomWrite)
		{
			Debug.LogWarning("RTHandle that is MSAA-enabled cannot allocate MSAA RT with 'enableRandomWrite = true'.");
			enableRandomWrite = false;
		}
		bool flag2 = GraphicsFormatUtility.IsDepthStencilFormat(format);
		if (enableShadingRate && (isShadowMap || flag2))
		{
			Debug.LogWarning("RTHandle allocated with incompatible enableShadingRate, forcing enableShadingRate to false.");
			enableShadingRate = false;
		}
		ShadowSamplingMode shadowSamplingMode = ShadowSamplingMode.None;
		GraphicsFormat depthStencilFormat;
		GraphicsFormat colorFormat;
		GraphicsFormat stencilFormat;
		string renderTargetAutoName;
		if (isShadowMap)
		{
			int num = GraphicsFormatUtility.GetDepthBits(format);
			if (num < 16)
			{
				num = 16;
			}
			depthStencilFormat = GraphicsFormatUtility.GetDepthStencilFormat(num, 0);
			colorFormat = GraphicsFormat.None;
			stencilFormat = GraphicsFormat.None;
			shadowSamplingMode = ShadowSamplingMode.CompareDepths;
			renderTargetAutoName = CoreUtils.GetRenderTargetAutoName(width, height, slices, RenderTextureFormat.Shadowmap, name, useMipMap, flag, msaaSamples);
		}
		else if (flag2)
		{
			colorFormat = GraphicsFormat.None;
			depthStencilFormat = format;
			stencilFormat = GetStencilFormat(format);
			renderTargetAutoName = CoreUtils.GetRenderTargetAutoName(width, height, slices, format, dimension, name, useMipMap, flag, msaaSamples, useDynamicScale, useDynamicScaleExplicit);
		}
		else
		{
			colorFormat = format;
			depthStencilFormat = GraphicsFormat.None;
			stencilFormat = GraphicsFormat.None;
			renderTargetAutoName = CoreUtils.GetRenderTargetAutoName(width, height, slices, format, dimension, name, useMipMap, flag, msaaSamples, useDynamicScale, useDynamicScaleExplicit);
		}
		RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height, colorFormat, depthStencilFormat);
		desc.msaaSamples = (int)msaaSamples;
		desc.volumeDepth = slices;
		desc.stencilFormat = stencilFormat;
		desc.dimension = dimension;
		desc.shadowSamplingMode = shadowSamplingMode;
		desc.vrUsage = vrUsage;
		desc.memoryless = memoryless;
		desc.useMipMap = useMipMap;
		desc.autoGenerateMips = autoGenerateMips;
		desc.enableRandomWrite = enableRandomWrite;
		desc.bindMS = bindTextureMS;
		desc.useDynamicScale = m_HardwareDynamicResRequested && useDynamicScale;
		desc.useDynamicScaleExplicit = m_HardwareDynamicResRequested && useDynamicScaleExplicit;
		desc.enableShadingRate = enableShadingRate;
		RenderTexture renderTexture = new RenderTexture(desc);
		renderTexture.name = renderTargetAutoName;
		renderTexture.anisoLevel = anisoLevel;
		renderTexture.mipMapBias = mipMapBias;
		renderTexture.hideFlags = HideFlags.HideAndDontSave;
		renderTexture.filterMode = filterMode;
		renderTexture.wrapModeU = wrapModeU;
		renderTexture.wrapModeV = wrapModeV;
		renderTexture.wrapModeW = wrapModeW;
		renderTexture.Create();
		return renderTexture;
	}

	public RTHandle Alloc(int width, int height, RTHandleAllocInfo info)
	{
		RenderTexture rt = CreateRenderTexture(width, height, info.format, info.slices, info.filterMode, info.wrapModeU, info.wrapModeV, info.wrapModeW, info.dimension, info.enableRandomWrite, info.useMipMap, info.autoGenerateMips, info.isShadowMap, info.anisoLevel, info.mipMapBias, info.msaaSamples, info.bindTextureMS, info.useDynamicScale, info.useDynamicScaleExplicit, info.memoryless, info.vrUsage, info.enableShadingRate, info.name);
		RTHandle rTHandle = new RTHandle(this);
		rTHandle.SetRenderTexture(rt);
		rTHandle.useScaling = false;
		rTHandle.m_EnableRandomWrite = info.enableRandomWrite;
		rTHandle.m_EnableMSAA = info.msaaSamples != MSAASamples.None;
		rTHandle.m_EnableHWDynamicScale = info.useDynamicScale;
		rTHandle.m_Name = info.name;
		rTHandle.referenceSize = new Vector2Int(width, height);
		if (info.enableShadingRate)
		{
			rTHandle.scaleFunc = (Vector2Int refSize) => ShadingRateImage.GetAllocTileSize(refSize);
		}
		return rTHandle;
	}

	public Vector2Int CalculateDimensions(Vector2 scaleFactor)
	{
		return CalculateDimensions(scaleFactor, new Vector2Int(GetMaxWidth(), GetMaxHeight()));
	}

	private static Vector2Int CalculateDimensions(Vector2 scaleFactor, Vector2Int size)
	{
		return new Vector2Int(Mathf.Max(Mathf.RoundToInt(scaleFactor.x * (float)size.x), 1), Mathf.Max(Mathf.RoundToInt(scaleFactor.y * (float)size.y), 1));
	}

	public RTHandle Alloc(Vector2 scaleFactor, GraphicsFormat format, int slices = 1, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		Vector2Int referenceSize = CalculateDimensions(scaleFactor);
		bool enableShadingRate = false;
		RTHandle rTHandle = AllocAutoSizedRenderTexture(referenceSize.x, referenceSize.y, slices, format, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, enableShadingRate, name);
		rTHandle.referenceSize = referenceSize;
		rTHandle.scaleFactor = scaleFactor;
		return rTHandle;
	}

	public RTHandle Alloc(Vector2 scaleFactor, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		GraphicsFormat format = ((depthBufferBits != 0) ? GraphicsFormatUtility.GetDepthStencilFormat((int)depthBufferBits) : colorFormat);
		return Alloc(scaleFactor, format, slices, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public RTHandle Alloc(Vector2 scaleFactor, RTHandleAllocInfo info)
	{
		int num = Mathf.Max(Mathf.RoundToInt(scaleFactor.x * (float)GetMaxWidth()), 1);
		int num2 = Mathf.Max(Mathf.RoundToInt(scaleFactor.y * (float)GetMaxHeight()), 1);
		RTHandle rTHandle = AllocAutoSizedRenderTexture(num, num2, info);
		rTHandle.referenceSize = new Vector2Int(num, num2);
		if (info.enableShadingRate)
		{
			rTHandle.scaleFunc = (Vector2Int refSize) => ShadingRateImage.GetAllocTileSize(CalculateDimensions(scaleFactor, refSize));
		}
		else
		{
			rTHandle.scaleFactor = scaleFactor;
		}
		return rTHandle;
	}

	public Vector2Int CalculateDimensions(ScaleFunc scaleFunc)
	{
		Vector2Int vector2Int = scaleFunc(new Vector2Int(GetMaxWidth(), GetMaxHeight()));
		return new Vector2Int(Mathf.Max(vector2Int.x, 1), Mathf.Max(vector2Int.y, 1));
	}

	public RTHandle Alloc(ScaleFunc scaleFunc, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		GraphicsFormat format = ((depthBufferBits != 0) ? GraphicsFormatUtility.GetDepthStencilFormat((int)depthBufferBits) : colorFormat);
		return Alloc(scaleFunc, format, slices, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, name);
	}

	public RTHandle Alloc(ScaleFunc scaleFunc, GraphicsFormat format, int slices = 1, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, bool useDynamicScaleExplicit = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		Vector2Int referenceSize = CalculateDimensions(scaleFunc);
		bool enableShadingRate = false;
		RTHandle rTHandle = AllocAutoSizedRenderTexture(referenceSize.x, referenceSize.y, slices, format, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, enableShadingRate, name);
		rTHandle.referenceSize = referenceSize;
		rTHandle.scaleFunc = scaleFunc;
		return rTHandle;
	}

	public RTHandle Alloc(ScaleFunc scaleFunc, RTHandleAllocInfo info)
	{
		Vector2Int vector2Int = scaleFunc(new Vector2Int(GetMaxWidth(), GetMaxHeight()));
		int num = Mathf.Max(vector2Int.x, 1);
		int num2 = Mathf.Max(vector2Int.y, 1);
		RTHandle rTHandle = AllocAutoSizedRenderTexture(num, num2, info);
		rTHandle.referenceSize = new Vector2Int(num, num2);
		if (info.enableShadingRate)
		{
			rTHandle.scaleFunc = (Vector2Int refSize) => ShadingRateImage.GetAllocTileSize(scaleFunc(refSize));
		}
		else
		{
			rTHandle.scaleFunc = scaleFunc;
		}
		return rTHandle;
	}

	internal RTHandle AllocAutoSizedRenderTexture(int width, int height, int slices, GraphicsFormat format, FilterMode filterMode, TextureWrapMode wrapMode, TextureDimension dimension, bool enableRandomWrite, bool useMipMap, bool autoGenerateMips, bool isShadowMap, int anisoLevel, float mipMapBias, MSAASamples msaaSamples, bool bindTextureMS, bool useDynamicScale, bool useDynamicScaleExplicit, RenderTextureMemoryless memoryless, VRTextureUsage vrUsage, bool enableShadingRate, string name)
	{
		if (enableShadingRate)
		{
			Vector2Int allocTileSize = ShadingRateImage.GetAllocTileSize(width, height);
			width = allocTileSize.x;
			height = allocTileSize.y;
		}
		RenderTexture rt = CreateRenderTexture(width, height, format, slices, filterMode, wrapMode, wrapMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, useDynamicScaleExplicit, memoryless, vrUsage, enableShadingRate, name);
		RTHandle rTHandle = new RTHandle(this);
		rTHandle.SetRenderTexture(rt);
		rTHandle.m_EnableMSAA = msaaSamples != MSAASamples.None;
		rTHandle.m_EnableRandomWrite = enableRandomWrite;
		rTHandle.useScaling = true;
		rTHandle.m_EnableHWDynamicScale = useDynamicScale;
		rTHandle.m_Name = name;
		m_AutoSizedRTs.Add(rTHandle);
		return rTHandle;
	}

	internal RTHandle AllocAutoSizedRenderTexture(int width, int height, RTHandleAllocInfo info)
	{
		if (info.enableShadingRate)
		{
			Vector2Int allocTileSize = ShadingRateImage.GetAllocTileSize(width, height);
			width = allocTileSize.x;
			height = allocTileSize.y;
		}
		RenderTexture rt = CreateRenderTexture(width, height, info.format, info.slices, info.filterMode, info.wrapModeU, info.wrapModeV, info.wrapModeW, info.dimension, info.enableRandomWrite, info.useMipMap, info.autoGenerateMips, info.isShadowMap, info.anisoLevel, info.mipMapBias, info.msaaSamples, info.bindTextureMS, info.useDynamicScale, info.useDynamicScaleExplicit, info.memoryless, info.vrUsage, info.enableShadingRate, info.name);
		RTHandle rTHandle = new RTHandle(this);
		rTHandle.SetRenderTexture(rt);
		rTHandle.m_EnableMSAA = info.msaaSamples != MSAASamples.None;
		rTHandle.m_EnableRandomWrite = info.enableRandomWrite;
		rTHandle.useScaling = true;
		rTHandle.m_EnableHWDynamicScale = info.useDynamicScale;
		rTHandle.m_Name = info.name;
		m_AutoSizedRTs.Add(rTHandle);
		return rTHandle;
	}

	public RTHandle Alloc(RenderTexture texture, bool transferOwnership = false)
	{
		RTHandle rTHandle = new RTHandle(this);
		rTHandle.SetRenderTexture(texture, transferOwnership);
		rTHandle.m_EnableMSAA = false;
		rTHandle.m_EnableRandomWrite = false;
		rTHandle.useScaling = false;
		rTHandle.m_EnableHWDynamicScale = false;
		rTHandle.m_Name = texture.name;
		return rTHandle;
	}

	public RTHandle Alloc(Texture texture)
	{
		RTHandle rTHandle = new RTHandle(this);
		rTHandle.SetTexture(texture);
		rTHandle.m_EnableMSAA = false;
		rTHandle.m_EnableRandomWrite = false;
		rTHandle.useScaling = false;
		rTHandle.m_EnableHWDynamicScale = false;
		rTHandle.m_Name = texture.name;
		return rTHandle;
	}

	public RTHandle Alloc(RenderTargetIdentifier texture)
	{
		return Alloc(texture, "");
	}

	public RTHandle Alloc(RenderTargetIdentifier texture, string name)
	{
		RTHandle rTHandle = new RTHandle(this);
		rTHandle.SetTexture(texture);
		rTHandle.m_EnableMSAA = false;
		rTHandle.m_EnableRandomWrite = false;
		rTHandle.useScaling = false;
		rTHandle.m_EnableHWDynamicScale = false;
		rTHandle.m_Name = name;
		return rTHandle;
	}

	private static RTHandle Alloc(RTHandle tex)
	{
		Debug.LogError("Allocation a RTHandle from another one is forbidden.");
		return null;
	}

	internal string DumpRTInfo()
	{
		string text = "";
		Array.Resize(ref m_AutoSizedRTsArray, m_AutoSizedRTs.Count);
		m_AutoSizedRTs.CopyTo(m_AutoSizedRTsArray);
		int i = 0;
		for (int num = m_AutoSizedRTsArray.Length; i < num; i++)
		{
			RenderTexture rt = m_AutoSizedRTsArray[i].rt;
			text = $"{text}\nRT ({i})\t Format: {rt.format} W: {rt.width} H {rt.height}\n";
		}
		return text;
	}

	private GraphicsFormat GetStencilFormat(GraphicsFormat depthStencilFormat)
	{
		if (!GraphicsFormatUtility.IsStencilFormat(depthStencilFormat) || !SystemInfo.IsFormatSupported(GraphicsFormat.R8_UInt, GraphicsFormatUsage.StencilSampling))
		{
			return GraphicsFormat.None;
		}
		return GraphicsFormat.R8_UInt;
	}
}
