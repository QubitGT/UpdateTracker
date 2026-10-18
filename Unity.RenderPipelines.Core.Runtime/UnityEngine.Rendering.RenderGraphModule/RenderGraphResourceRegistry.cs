using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.RenderGraphModule;

internal class RenderGraphResourceRegistry
{
	private delegate bool ResourceCreateCallback(InternalRenderGraphContext rgContext, IRenderGraphResource res);

	private delegate void ResourceCallback(InternalRenderGraphContext rgContext, IRenderGraphResource res);

	private class RenderGraphResourcesData
	{
		public DynamicArray<IRenderGraphResource> resourceArray = new DynamicArray<IRenderGraphResource>();

		public int sharedResourcesCount;

		public IRenderGraphResourcePool pool;

		public ResourceCreateCallback createResourceCallback;

		public ResourceCallback releaseResourceCallback;

		public RenderGraphResourcesData()
		{
			resourceArray.Resize(1);
		}

		public void Clear(bool onException, int frameIndex)
		{
			resourceArray.Resize(sharedResourcesCount + 1);
			if (pool != null)
			{
				pool.CheckFrameAllocation(onException, frameIndex);
			}
		}

		public void Cleanup()
		{
			for (int i = 1; i < sharedResourcesCount + 1; i++)
			{
				resourceArray[i]?.ReleaseGraphicsResource();
			}
			if (pool != null)
			{
				pool.Cleanup();
			}
		}

		public void PurgeUnusedGraphicsResources(int frameIndex)
		{
			if (pool != null)
			{
				pool.PurgeUnusedResources(frameIndex);
			}
		}

		public int AddNewRenderGraphResource<ResType>(out ResType outRes, bool pooledResource = true) where ResType : IRenderGraphResource, new()
		{
			int size = resourceArray.size;
			resourceArray.Resize(resourceArray.size + 1, keepContent: true);
			if (resourceArray[size] == null)
			{
				resourceArray[size] = new ResType();
			}
			outRes = resourceArray[size] as ResType;
			outRes.Reset(pooledResource ? pool : null);
			return size;
		}
	}

	private const int kSharedResourceLifetime = 30;

	private static RenderGraphResourceRegistry m_CurrentRegistry;

	private RenderGraphResourcesData[] m_RenderGraphResources = new RenderGraphResourcesData[3];

	private DynamicArray<RendererListResource> m_RendererListResources = new DynamicArray<RendererListResource>();

	private DynamicArray<RendererListLegacyResource> m_RendererListLegacyResources = new DynamicArray<RendererListLegacyResource>();

	private RenderGraphDebugParams m_RenderGraphDebug;

	private RenderGraphLogger m_ResourceLogger = new RenderGraphLogger();

	private RenderGraphLogger m_FrameInformationLogger;

	private int m_CurrentFrameIndex;

	private int m_ExecutionCount;

	private RTHandle m_CurrentBackbuffer;

	private const int kInitialRendererListCount = 256;

	private List<RendererList> m_ActiveRendererLists = new List<RendererList>(256);

	private static RenderTargetIdentifier emptyId = RenderTargetIdentifier.Invalid;

	private static RenderTargetIdentifier builtinCameraRenderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

	internal bool forceManualClearOfResource = true;

	internal static RenderGraphResourceRegistry current
	{
		get
		{
			return m_CurrentRegistry;
		}
		set
		{
			m_CurrentRegistry = value;
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckTextureResource(TextureResource texResource)
	{
		if (texResource.graphicsResource == null && !texResource.imported)
		{
			throw new InvalidOperationException("Trying to use a texture (" + texResource.GetName() + ") that was already released or not yet created. Make sure you declare it for reading in your pass or you don't read it before it's been written to at least once.");
		}
	}

	internal RTHandle GetTexture(in TextureHandle handle)
	{
		if (!handle.IsValid())
		{
			return null;
		}
		return GetTextureResource(in handle.handle).graphicsResource;
	}

	internal RTHandle GetTexture(int index)
	{
		return GetTextureResource(index).graphicsResource;
	}

	internal bool TextureNeedsFallback(in TextureHandle handle)
	{
		if (!handle.IsValid())
		{
			return false;
		}
		return GetTextureResource(in handle.handle).NeedsFallBack();
	}

	internal RendererList GetRendererList(in RendererListHandle handle)
	{
		if (!handle.IsValid())
		{
			return RendererList.nullRendererList;
		}
		switch (handle.type)
		{
		case RendererListHandleType.Renderers:
			if ((int)handle >= m_RendererListResources.size)
			{
				return RendererList.nullRendererList;
			}
			return m_RendererListResources[handle].rendererList;
		case RendererListHandleType.Legacy:
			if ((int)handle >= m_RendererListLegacyResources.size)
			{
				return RendererList.nullRendererList;
			}
			if (!m_RendererListLegacyResources[handle].isActive)
			{
				return RendererList.nullRendererList;
			}
			return m_RendererListLegacyResources[handle].rendererList;
		default:
			return RendererList.nullRendererList;
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckBufferResource(BufferResource bufferResource)
	{
		if (bufferResource.graphicsResource == null)
		{
			throw new InvalidOperationException("Trying to use a graphics buffer (" + bufferResource.GetName() + ") that was already released or not yet created. Make sure you declare it for reading in your pass or you don't read it before it's been written to at least once.");
		}
	}

	internal GraphicsBuffer GetBuffer(in BufferHandle handle)
	{
		if (!handle.IsValid())
		{
			return null;
		}
		return GetBufferResource(in handle.handle).graphicsResource;
	}

	internal GraphicsBuffer GetBuffer(int index)
	{
		return GetBufferResource(index).graphicsResource;
	}

	internal RayTracingAccelerationStructure GetRayTracingAccelerationStructure(in RayTracingAccelerationStructureHandle handle)
	{
		if (!handle.IsValid())
		{
			return null;
		}
		return GetRayTracingAccelerationStructureResource(in handle.handle).graphicsResource;
	}

	internal int GetSharedResourceCount(RenderGraphResourceType type)
	{
		return m_RenderGraphResources[(int)type].sharedResourcesCount;
	}

	private RenderGraphResourceRegistry()
	{
	}

	internal RenderGraphResourceRegistry(RenderGraphDebugParams renderGraphDebug, RenderGraphLogger frameInformationLogger)
	{
		m_RenderGraphDebug = renderGraphDebug;
		m_FrameInformationLogger = frameInformationLogger;
		for (int i = 0; i < 3; i++)
		{
			m_RenderGraphResources[i] = new RenderGraphResourcesData();
		}
		m_RenderGraphResources[0].createResourceCallback = CreateTextureCallback;
		m_RenderGraphResources[0].releaseResourceCallback = ReleaseTextureCallback;
		m_RenderGraphResources[0].pool = new TexturePool();
		m_RenderGraphResources[1].pool = new BufferPool();
		m_RenderGraphResources[2].pool = null;
	}

	internal void BeginRenderGraph(int executionCount)
	{
		m_ExecutionCount = executionCount;
		ResourceHandle.NewFrame(executionCount);
		if (m_RenderGraphDebug.enableLogging)
		{
			m_ResourceLogger.Initialize("RenderGraph Resources");
		}
	}

	internal void BeginExecute(int currentFrameIndex)
	{
		m_CurrentFrameIndex = currentFrameIndex;
		ManageSharedRenderGraphResources();
		current = this;
	}

	internal void EndExecute()
	{
		current = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckHandleValidity(in ResourceHandle res)
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckHandleValidity(RenderGraphResourceType type, int index)
	{
		if (RenderGraph.enableValidityChecks)
		{
			DynamicArray<IRenderGraphResource> resourceArray = m_RenderGraphResources[(int)type].resourceArray;
			if (index == 0)
			{
				throw new ArgumentException($"Trying to access resource of type {type} with an null resource index.");
			}
			if (index >= resourceArray.size)
			{
				throw new ArgumentException($"Trying to access resource of type {type} with an invalid resource index {index}");
			}
		}
	}

	internal void IncrementWriteCount(in ResourceHandle res)
	{
		m_RenderGraphResources[res.iType].resourceArray[res.index].IncrementWriteCount();
	}

	internal void IncrementReadCount(in ResourceHandle res)
	{
		m_RenderGraphResources[res.iType].resourceArray[res.index].IncrementReadCount();
	}

	internal void NewVersion(in ResourceHandle res)
	{
		m_RenderGraphResources[res.iType].resourceArray[res.index].NewVersion();
	}

	internal ResourceHandle GetLatestVersionHandle(in ResourceHandle res)
	{
		int num = m_RenderGraphResources[res.iType].resourceArray[res.index].version;
		if (IsRenderGraphResourceShared(in res))
		{
			num -= m_ExecutionCount;
		}
		return new ResourceHandle(in res, num);
	}

	internal int GetLatestVersionNumber(in ResourceHandle res)
	{
		int num = m_RenderGraphResources[res.iType].resourceArray[res.index].version;
		if (IsRenderGraphResourceShared(in res))
		{
			num -= m_ExecutionCount;
		}
		return num;
	}

	internal ResourceHandle GetZeroVersionedHandle(in ResourceHandle res)
	{
		return new ResourceHandle(in res, 0);
	}

	internal ResourceHandle GetNewVersionedHandle(in ResourceHandle res)
	{
		int num = m_RenderGraphResources[res.iType].resourceArray[res.index].NewVersion();
		if (IsRenderGraphResourceShared(in res))
		{
			num -= m_ExecutionCount;
		}
		return new ResourceHandle(in res, num);
	}

	internal IRenderGraphResource GetResourceLowLevel(in ResourceHandle res)
	{
		return m_RenderGraphResources[res.iType].resourceArray[res.index];
	}

	internal string GetRenderGraphResourceName(in ResourceHandle res)
	{
		return m_RenderGraphResources[res.iType].resourceArray[res.index].GetName();
	}

	internal string GetRenderGraphResourceName(RenderGraphResourceType type, int index)
	{
		return m_RenderGraphResources[(int)type].resourceArray[index].GetName();
	}

	internal bool IsRenderGraphResourceImported(in ResourceHandle res)
	{
		return m_RenderGraphResources[res.iType].resourceArray[res.index].imported;
	}

	internal bool IsRenderGraphResourceForceReleased(RenderGraphResourceType type, int index)
	{
		return m_RenderGraphResources[(int)type].resourceArray[index].forceRelease;
	}

	internal bool IsRenderGraphResourceShared(RenderGraphResourceType type, int index)
	{
		return index <= m_RenderGraphResources[(int)type].sharedResourcesCount;
	}

	internal bool IsRenderGraphResourceShared(in ResourceHandle res)
	{
		return IsRenderGraphResourceShared(res.type, res.index);
	}

	internal bool IsGraphicsResourceCreated(in ResourceHandle res)
	{
		return m_RenderGraphResources[res.iType].resourceArray[res.index].IsCreated();
	}

	internal bool IsRendererListCreated(in RendererListHandle res)
	{
		switch (res.type)
		{
		case RendererListHandleType.Renderers:
			return m_RendererListResources[res].rendererList.isValid;
		case RendererListHandleType.Legacy:
			if (m_RendererListLegacyResources[res].isActive)
			{
				return m_RendererListLegacyResources[res].rendererList.isValid;
			}
			return false;
		default:
			return false;
		}
	}

	internal bool IsRenderGraphResourceImported(RenderGraphResourceType type, int index)
	{
		return m_RenderGraphResources[(int)type].resourceArray[index].imported;
	}

	internal int GetRenderGraphResourceTransientIndex(in ResourceHandle res)
	{
		return m_RenderGraphResources[res.iType].resourceArray[res.index].transientPassIndex;
	}

	internal TextureHandle ImportTexture(in RTHandle rt, bool isBuiltin = false)
	{
		ImportResourceParams importParams = default(ImportResourceParams);
		importParams.clearOnFirstUse = false;
		importParams.discardOnLastUse = false;
		return ImportTexture(in rt, in importParams, isBuiltin);
	}

	internal TextureHandle ImportTexture(in RTHandle rt, in ImportResourceParams importParams, bool isBuiltin = false)
	{
		if (rt != null && !(rt.m_RT != null))
		{
			_ = rt.m_ExternalTexture != null;
		}
		TextureResource outRes;
		int handle = m_RenderGraphResources[0].AddNewRenderGraphResource<TextureResource>(out outRes);
		outRes.graphicsResource = rt;
		outRes.imported = true;
		RenderTexture renderTexture = ((rt == null) ? null : ((rt.m_RT != null) ? rt.m_RT : (rt.m_ExternalTexture as RenderTexture)));
		if ((bool)renderTexture)
		{
			outRes.desc = new TextureDesc(renderTexture);
			outRes.validDesc = true;
		}
		outRes.desc.clearBuffer = importParams.clearOnFirstUse;
		outRes.desc.clearColor = importParams.clearColor;
		outRes.desc.discardBuffer = importParams.discardOnLastUse;
		TextureHandle result = new TextureHandle(handle, shared: false, isBuiltin);
		_ = rt;
		return result;
	}

	internal TextureHandle ImportTexture(in RTHandle rt, RenderTargetInfo info, in ImportResourceParams importParams)
	{
		TextureResource outRes;
		int handle = m_RenderGraphResources[0].AddNewRenderGraphResource<TextureResource>(out outRes);
		outRes.graphicsResource = rt;
		outRes.imported = true;
		outRes.desc = default(TextureDesc);
		if (rt != null && rt.m_NameID != emptyId)
		{
			outRes.desc.format = info.format;
			outRes.desc.width = info.width;
			outRes.desc.height = info.height;
			outRes.desc.slices = info.volumeDepth;
			outRes.desc.msaaSamples = (MSAASamples)info.msaaSamples;
			outRes.desc.bindTextureMS = info.bindMS;
			outRes.desc.clearBuffer = importParams.clearOnFirstUse;
			outRes.desc.clearColor = importParams.clearColor;
			outRes.desc.discardBuffer = importParams.discardOnLastUse;
			outRes.validDesc = false;
		}
		return new TextureHandle(handle);
	}

	internal TextureHandle CreateSharedTexture(in TextureDesc desc, bool explicitRelease)
	{
		RenderGraphResourcesData renderGraphResourcesData = m_RenderGraphResources[0];
		int sharedResourcesCount = renderGraphResourcesData.sharedResourcesCount;
		TextureResource outRes = null;
		int handle = -1;
		for (int i = 1; i < sharedResourcesCount + 1; i++)
		{
			if (!renderGraphResourcesData.resourceArray[i].shared)
			{
				outRes = (TextureResource)renderGraphResourcesData.resourceArray[i];
				handle = i;
				break;
			}
		}
		if (outRes == null)
		{
			handle = m_RenderGraphResources[0].AddNewRenderGraphResource<TextureResource>(out outRes, pooledResource: false);
			renderGraphResourcesData.sharedResourcesCount++;
		}
		outRes.imported = true;
		outRes.shared = true;
		outRes.sharedExplicitRelease = explicitRelease;
		outRes.desc = desc;
		outRes.validDesc = true;
		return new TextureHandle(handle, shared: true);
	}

	internal void RefreshSharedTextureDesc(in TextureHandle texture, in TextureDesc desc)
	{
		TextureResource textureResource = GetTextureResource(in texture.handle);
		textureResource.ReleaseGraphicsResource();
		textureResource.desc = desc;
	}

	internal void ReleaseSharedTexture(in TextureHandle texture)
	{
		RenderGraphResourcesData renderGraphResourcesData = m_RenderGraphResources[0];
		if (texture.handle.index == renderGraphResourcesData.sharedResourcesCount)
		{
			renderGraphResourcesData.sharedResourcesCount--;
		}
		TextureResource textureResource = GetTextureResource(in texture.handle);
		textureResource.ReleaseGraphicsResource();
		textureResource.Reset();
	}

	internal TextureHandle ImportBackbuffer(RenderTargetIdentifier rt, in RenderTargetInfo info, in ImportResourceParams importParams)
	{
		if (m_CurrentBackbuffer != null)
		{
			m_CurrentBackbuffer.SetTexture(rt);
		}
		else
		{
			m_CurrentBackbuffer = RTHandles.Alloc(rt, "Backbuffer");
		}
		TextureResource outRes;
		int handle = m_RenderGraphResources[0].AddNewRenderGraphResource<TextureResource>(out outRes);
		outRes.graphicsResource = m_CurrentBackbuffer;
		outRes.imported = true;
		outRes.desc = default(TextureDesc);
		outRes.desc.width = info.width;
		outRes.desc.height = info.height;
		outRes.desc.slices = info.volumeDepth;
		outRes.desc.msaaSamples = (MSAASamples)info.msaaSamples;
		outRes.desc.bindTextureMS = info.bindMS;
		outRes.desc.format = info.format;
		outRes.desc.clearBuffer = importParams.clearOnFirstUse;
		outRes.desc.clearColor = importParams.clearColor;
		outRes.desc.discardBuffer = importParams.discardOnLastUse;
		outRes.validDesc = false;
		return new TextureHandle(handle);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void ValidateRenderTarget(in ResourceHandle res)
	{
		if (RenderGraph.enableValidityChecks)
		{
			GetRenderTargetInfo(in res, out var _);
		}
	}

	internal void GetRenderTargetInfo(in ResourceHandle res, out RenderTargetInfo outInfo)
	{
		TextureResource textureResource = GetTextureResource(in res);
		if (textureResource.imported)
		{
			RTHandle graphicsResource = textureResource.graphicsResource;
			if (graphicsResource == null)
			{
				outInfo = default(RenderTargetInfo);
			}
			else if (graphicsResource.m_RT != null)
			{
				outInfo = default(RenderTargetInfo);
				outInfo.width = graphicsResource.m_RT.width;
				outInfo.height = graphicsResource.m_RT.height;
				outInfo.volumeDepth = graphicsResource.m_RT.volumeDepth;
				outInfo.format = GetFormat(graphicsResource.m_RT.graphicsFormat, graphicsResource.m_RT.depthStencilFormat);
				outInfo.msaaSamples = graphicsResource.m_RT.antiAliasing;
				outInfo.bindMS = graphicsResource.m_RT.bindTextureMS;
			}
			else if (graphicsResource.m_ExternalTexture != null)
			{
				outInfo = default(RenderTargetInfo);
				outInfo.width = graphicsResource.m_ExternalTexture.width;
				outInfo.height = graphicsResource.m_ExternalTexture.height;
				outInfo.volumeDepth = 1;
				if (graphicsResource.m_ExternalTexture is RenderTexture)
				{
					RenderTexture renderTexture = (RenderTexture)graphicsResource.m_ExternalTexture;
					outInfo.format = GetFormat(renderTexture.graphicsFormat, renderTexture.depthStencilFormat);
					outInfo.msaaSamples = renderTexture.antiAliasing;
				}
				else
				{
					outInfo.format = graphicsResource.m_ExternalTexture.graphicsFormat;
					outInfo.msaaSamples = 1;
				}
				outInfo.bindMS = false;
			}
			else
			{
				if (!(graphicsResource.m_NameID != emptyId))
				{
					throw new Exception("Invalid imported texture. The RTHandle provided is invalid.");
				}
				TextureDesc textureResourceDesc = GetTextureResourceDesc(in res, noThrowOnInvalidDesc: true);
				outInfo.width = textureResourceDesc.width;
				outInfo.height = textureResourceDesc.height;
				outInfo.volumeDepth = textureResourceDesc.slices;
				outInfo.msaaSamples = (int)textureResourceDesc.msaaSamples;
				outInfo.format = textureResourceDesc.format;
				outInfo.bindMS = textureResourceDesc.bindTextureMS;
			}
		}
		else
		{
			TextureDesc textureResourceDesc2 = GetTextureResourceDesc(in res);
			Vector2Int vector2Int = textureResourceDesc2.CalculateFinalDimensions();
			outInfo = default(RenderTargetInfo);
			outInfo.width = vector2Int.x;
			outInfo.height = vector2Int.y;
			outInfo.volumeDepth = textureResourceDesc2.slices;
			outInfo.msaaSamples = (int)textureResourceDesc2.msaaSamples;
			outInfo.bindMS = textureResourceDesc2.bindTextureMS;
			outInfo.format = textureResourceDesc2.format;
		}
	}

	internal GraphicsFormat GetFormat(GraphicsFormat color, GraphicsFormat depthStencil)
	{
		if (depthStencil == GraphicsFormat.None)
		{
			return color;
		}
		return depthStencil;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void ValidateFormat(GraphicsFormat color, GraphicsFormat depthStencil)
	{
		if (RenderGraph.enableValidityChecks && color != 0 && depthStencil != 0)
		{
			throw new Exception("Invalid imported texture. Both a color and a depthStencil format are provided. The texture needs to either have a color format or a depth stencil format.");
		}
	}

	internal TextureHandle CreateTexture(in TextureDesc desc, int transientPassIndex = -1)
	{
		TextureResource outRes;
		int handle = m_RenderGraphResources[0].AddNewRenderGraphResource<TextureResource>(out outRes);
		outRes.desc = desc;
		outRes.validDesc = true;
		outRes.transientPassIndex = transientPassIndex;
		outRes.requestFallBack = desc.fallBackToBlackTexture;
		return new TextureHandle(handle);
	}

	internal int GetResourceCount(RenderGraphResourceType type)
	{
		return m_RenderGraphResources[(int)type].resourceArray.size;
	}

	internal int GetTextureResourceCount()
	{
		return GetResourceCount(RenderGraphResourceType.Texture);
	}

	internal TextureResource GetTextureResource(in ResourceHandle handle)
	{
		return m_RenderGraphResources[0].resourceArray[handle.index] as TextureResource;
	}

	internal TextureResource GetTextureResource(int index)
	{
		return m_RenderGraphResources[0].resourceArray[index] as TextureResource;
	}

	internal TextureDesc GetTextureResourceDesc(in ResourceHandle handle, bool noThrowOnInvalidDesc = false)
	{
		TextureResource obj = m_RenderGraphResources[0].resourceArray[handle.index] as TextureResource;
		if (!obj.validDesc && !noThrowOnInvalidDesc)
		{
			throw new ArgumentException("The passed in texture handle does not have a valid descriptor. (This is most commonly cause by the handle referencing a built-in texture such as the system back buffer.)", "handle");
		}
		return obj.desc;
	}

	internal RendererListHandle CreateRendererList(in RendererListDesc desc)
	{
		DynamicArray<RendererListResource> rendererListResources = m_RendererListResources;
		RendererListParams desc2 = RendererListDesc.ConvertToParameters(in desc);
		RendererListResource value = new RendererListResource(in desc2);
		return new RendererListHandle(rendererListResources.Add(in value));
	}

	internal RendererListHandle CreateRendererList(in RendererListParams desc)
	{
		DynamicArray<RendererListResource> rendererListResources = m_RendererListResources;
		RendererListResource value = new RendererListResource(in desc);
		return new RendererListHandle(rendererListResources.Add(in value));
	}

	internal RendererListHandle CreateShadowRendererList(ScriptableRenderContext context, ref ShadowDrawingSettings shadowDrawinSettings)
	{
		RendererListLegacyResource value = default(RendererListLegacyResource);
		value.rendererList = context.CreateShadowRendererList(ref shadowDrawinSettings);
		return new RendererListHandle(m_RendererListLegacyResources.Add(in value), RendererListHandleType.Legacy);
	}

	internal RendererListHandle CreateGizmoRendererList(ScriptableRenderContext context, in Camera camera, in GizmoSubset gizmoSubset)
	{
		RendererListLegacyResource value = default(RendererListLegacyResource);
		value.rendererList = context.CreateGizmoRendererList(camera, gizmoSubset);
		return new RendererListHandle(m_RendererListLegacyResources.Add(in value), RendererListHandleType.Legacy);
	}

	internal RendererListHandle CreateUIOverlayRendererList(ScriptableRenderContext context, in Camera camera, in UISubset uiSubset)
	{
		RendererListLegacyResource value = default(RendererListLegacyResource);
		value.rendererList = context.CreateUIOverlayRendererList(camera, uiSubset);
		return new RendererListHandle(m_RendererListLegacyResources.Add(in value), RendererListHandleType.Legacy);
	}

	internal RendererListHandle CreateWireOverlayRendererList(ScriptableRenderContext context, in Camera camera)
	{
		RendererListLegacyResource value = default(RendererListLegacyResource);
		value.rendererList = context.CreateWireOverlayRendererList(camera);
		return new RendererListHandle(m_RendererListLegacyResources.Add(in value), RendererListHandleType.Legacy);
	}

	internal RendererListHandle CreateSkyboxRendererList(ScriptableRenderContext context, in Camera camera)
	{
		RendererListLegacyResource value = default(RendererListLegacyResource);
		value.rendererList = context.CreateSkyboxRendererList(camera);
		return new RendererListHandle(m_RendererListLegacyResources.Add(in value), RendererListHandleType.Legacy);
	}

	internal RendererListHandle CreateSkyboxRendererList(ScriptableRenderContext context, in Camera camera, Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix)
	{
		RendererListLegacyResource value = default(RendererListLegacyResource);
		value.rendererList = context.CreateSkyboxRendererList(camera, projectionMatrix, viewMatrix);
		return new RendererListHandle(m_RendererListLegacyResources.Add(in value), RendererListHandleType.Legacy);
	}

	internal RendererListHandle CreateSkyboxRendererList(ScriptableRenderContext context, in Camera camera, Matrix4x4 projectionMatrixL, Matrix4x4 viewMatrixL, Matrix4x4 projectionMatrixR, Matrix4x4 viewMatrixR)
	{
		RendererListLegacyResource value = default(RendererListLegacyResource);
		value.rendererList = context.CreateSkyboxRendererList(camera, projectionMatrixL, viewMatrixL, projectionMatrixR, viewMatrixR);
		return new RendererListHandle(m_RendererListLegacyResources.Add(in value), RendererListHandleType.Legacy);
	}

	internal BufferHandle ImportBuffer(GraphicsBuffer graphicsBuffer, bool forceRelease = false)
	{
		BufferResource outRes;
		int handle = m_RenderGraphResources[1].AddNewRenderGraphResource<BufferResource>(out outRes);
		outRes.graphicsResource = graphicsBuffer;
		outRes.imported = true;
		outRes.forceRelease = forceRelease;
		outRes.validDesc = false;
		return new BufferHandle(handle);
	}

	internal BufferHandle CreateBuffer(in BufferDesc desc, int transientPassIndex = -1)
	{
		BufferResource outRes;
		int handle = m_RenderGraphResources[1].AddNewRenderGraphResource<BufferResource>(out outRes);
		outRes.desc = desc;
		outRes.validDesc = true;
		outRes.transientPassIndex = transientPassIndex;
		return new BufferHandle(handle);
	}

	internal BufferDesc GetBufferResourceDesc(in ResourceHandle handle, bool noThrowOnInvalidDesc = false)
	{
		BufferResource obj = m_RenderGraphResources[1].resourceArray[handle.index] as BufferResource;
		if (!obj.validDesc && !noThrowOnInvalidDesc)
		{
			throw new ArgumentException("The passed in buffer handle does not have a valid descriptor. (This is most commonly cause by importing the buffer.)", "handle");
		}
		return obj.desc;
	}

	internal int GetBufferResourceCount()
	{
		return GetResourceCount(RenderGraphResourceType.Buffer);
	}

	private BufferResource GetBufferResource(in ResourceHandle handle)
	{
		return m_RenderGraphResources[1].resourceArray[handle.index] as BufferResource;
	}

	private BufferResource GetBufferResource(int index)
	{
		return m_RenderGraphResources[1].resourceArray[index] as BufferResource;
	}

	private RayTracingAccelerationStructureResource GetRayTracingAccelerationStructureResource(in ResourceHandle handle)
	{
		return m_RenderGraphResources[2].resourceArray[handle.index] as RayTracingAccelerationStructureResource;
	}

	internal int GetRayTracingAccelerationStructureResourceCount()
	{
		return GetResourceCount(RenderGraphResourceType.AccelerationStructure);
	}

	internal RayTracingAccelerationStructureHandle ImportRayTracingAccelerationStructure(in RayTracingAccelerationStructure accelStruct, string name)
	{
		RayTracingAccelerationStructureResource outRes;
		int handle = m_RenderGraphResources[2].AddNewRenderGraphResource<RayTracingAccelerationStructureResource>(out outRes, pooledResource: false);
		outRes.graphicsResource = accelStruct;
		outRes.imported = true;
		outRes.forceRelease = false;
		outRes.desc.name = name;
		return new RayTracingAccelerationStructureHandle(handle);
	}

	internal void UpdateSharedResourceLastFrameIndex(int type, int index)
	{
		m_RenderGraphResources[type].resourceArray[index].sharedResourceLastFrameUsed = m_ExecutionCount;
	}

	internal void UpdateSharedResourceLastFrameIndex(in ResourceHandle handle)
	{
		UpdateSharedResourceLastFrameIndex((int)handle.type, handle.index);
	}

	private void ManageSharedRenderGraphResources()
	{
		for (int i = 0; i < 3; i++)
		{
			RenderGraphResourcesData renderGraphResourcesData = m_RenderGraphResources[i];
			for (int j = 1; j < renderGraphResourcesData.sharedResourcesCount + 1; j++)
			{
				IRenderGraphResource renderGraphResource = m_RenderGraphResources[i].resourceArray[j];
				bool flag = renderGraphResource.IsCreated();
				if (renderGraphResource.sharedResourceLastFrameUsed == m_ExecutionCount && !flag)
				{
					renderGraphResource.CreateGraphicsResource();
				}
				else if (flag && !renderGraphResource.sharedExplicitRelease && renderGraphResource.sharedResourceLastFrameUsed + 30 < m_ExecutionCount)
				{
					renderGraphResource.ReleaseGraphicsResource();
				}
			}
		}
	}

	internal bool CreatePooledResource(InternalRenderGraphContext rgContext, int type, int index)
	{
		bool? flag = false;
		IRenderGraphResource renderGraphResource = m_RenderGraphResources[type].resourceArray[index];
		if (!renderGraphResource.imported)
		{
			renderGraphResource.CreatePooledGraphicsResource();
			if (m_RenderGraphDebug.enableLogging)
			{
				renderGraphResource.LogCreation(m_FrameInformationLogger);
			}
			flag = m_RenderGraphResources[type].createResourceCallback?.Invoke(rgContext, renderGraphResource);
		}
		return flag.GetValueOrDefault();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool CreatePooledResource(InternalRenderGraphContext rgContext, in ResourceHandle handle)
	{
		return CreatePooledResource(rgContext, handle.iType, handle.index);
	}

	private bool CreateTextureCallback(InternalRenderGraphContext rgContext, IRenderGraphResource res)
	{
		TextureResource textureResource = res as TextureResource;
		FastMemoryDesc fastMemoryDesc = textureResource.desc.fastMemoryDesc;
		if (fastMemoryDesc.inFastMemory)
		{
			textureResource.graphicsResource.SwitchToFastMemory(rgContext.cmd, fastMemoryDesc.residencyFraction, fastMemoryDesc.flags);
		}
		bool result = false;
		if ((forceManualClearOfResource && textureResource.desc.clearBuffer) || m_RenderGraphDebug.clearRenderTargetsAtCreation)
		{
			ClearTexture(rgContext, textureResource);
			result = true;
		}
		return result;
	}

	internal void ClearResource(InternalRenderGraphContext rgContext, int type, int index)
	{
		if (m_RenderGraphResources[type].resourceArray[index] is TextureResource resource)
		{
			ClearTexture(rgContext, resource);
		}
	}

	private void ClearTexture(InternalRenderGraphContext rgContext, TextureResource resource)
	{
		if (resource != null)
		{
			bool num = m_RenderGraphDebug.clearRenderTargetsAtCreation && !resource.desc.clearBuffer;
			ClearFlag clearFlag = ((!GraphicsFormatUtility.IsDepthStencilFormat(resource.desc.format)) ? ClearFlag.Color : ClearFlag.DepthStencil);
			Color clearColor = (num ? Color.magenta : resource.desc.clearColor);
			CoreUtils.SetRenderTarget(rgContext.cmd, resource.graphicsResource, clearFlag, clearColor);
		}
	}

	internal void ReleasePooledResource(InternalRenderGraphContext rgContext, int type, int index)
	{
		IRenderGraphResource renderGraphResource = m_RenderGraphResources[type].resourceArray[index];
		if (!renderGraphResource.imported || renderGraphResource.forceRelease)
		{
			m_RenderGraphResources[type].releaseResourceCallback?.Invoke(rgContext, renderGraphResource);
			if (m_RenderGraphDebug.enableLogging)
			{
				renderGraphResource.LogRelease(m_FrameInformationLogger);
			}
			renderGraphResource.ReleasePooledGraphicsResource(m_CurrentFrameIndex);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ReleasePooledResource(InternalRenderGraphContext rgContext, in ResourceHandle handle)
	{
		ReleasePooledResource(rgContext, handle.iType, handle.index);
	}

	private void ReleaseTextureCallback(InternalRenderGraphContext rgContext, IRenderGraphResource res)
	{
		TextureResource textureResource = res as TextureResource;
		if (m_RenderGraphDebug.clearRenderTargetsAtRelease)
		{
			ClearFlag clearFlag = ((!GraphicsFormatUtility.IsDepthStencilFormat(textureResource.desc.format)) ? ClearFlag.Color : ClearFlag.DepthStencil);
			CoreUtils.SetRenderTarget(rgContext.cmd, textureResource.graphicsResource, clearFlag, Color.magenta);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void ValidateTextureDesc(in TextureDesc desc)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (desc.format == GraphicsFormat.None)
			{
				throw new ArgumentException("Texture was created with with no format. The texture needs to either have a color format or a depth stencil format.");
			}
			if (desc.dimension == TextureDimension.None || desc.dimension == TextureDimension.Any)
			{
				throw new ArgumentException("Texture was created with an invalid texture dimension.");
			}
			if (desc.slices == 0)
			{
				throw new ArgumentException("Texture was created with a slices parameter value of zero.");
			}
			if (desc.slices > 1 && (desc.dimension == TextureDimension.Tex2D || desc.dimension == TextureDimension.Cube) && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
			{
				throw new ArgumentException("Non-array texture was created with a slices parameter larger than one.");
			}
			if (desc.msaaSamples <= MSAASamples.None && desc.bindTextureMS)
			{
				throw new ArgumentException("A single sample texture was created with bindTextureMS.");
			}
			if (desc.sizeMode == TextureSizeMode.Explicit && (desc.width == 0 || desc.height == 0))
			{
				throw new ArgumentException("Texture using Explicit size mode was create with either width or height at zero.");
			}
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void ValidateRendererListDesc(in RendererListDesc desc)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (!desc.IsValid())
			{
				throw new ArgumentException("Renderer List descriptor is not valid.");
			}
			if (desc.renderQueueRange.lowerBound == 0 && desc.renderQueueRange.upperBound == 0)
			{
				throw new ArgumentException("Renderer List creation descriptor must have a valid RenderQueueRange.");
			}
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void ValidateBufferDesc(in BufferDesc desc)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (desc.stride % 4 != 0)
			{
				throw new ArgumentException("Invalid Graphics Buffer creation descriptor: Graphics Buffer stride must be at least 4.");
			}
			if (desc.count == 0)
			{
				throw new ArgumentException("Invalid Graphics Buffer creation descriptor: Graphics Buffer count  must be non zero.");
			}
		}
	}

	internal void CreateRendererLists(List<RendererListHandle> rendererLists, ScriptableRenderContext context, bool manualDispatch = false)
	{
		m_ActiveRendererLists.Clear();
		foreach (RendererListHandle rendererList in rendererLists)
		{
			switch (rendererList.type)
			{
			case RendererListHandleType.Renderers:
			{
				ref RendererListResource reference = ref m_RendererListResources[rendererList];
				reference.rendererList = context.CreateRendererList(ref reference.desc);
				m_ActiveRendererLists.Add(reference.rendererList);
				break;
			}
			case RendererListHandleType.Legacy:
				m_RendererListLegacyResources[rendererList].isActive = true;
				break;
			}
		}
		if (manualDispatch)
		{
			context.PrepareRendererListsAsync(m_ActiveRendererLists);
		}
	}

	internal void Clear(bool onException)
	{
		LogResources();
		for (int i = 0; i < 3; i++)
		{
			m_RenderGraphResources[i].Clear(onException, m_CurrentFrameIndex);
		}
		m_RendererListResources.Clear();
		m_RendererListLegacyResources.Clear();
		m_ActiveRendererLists.Clear();
	}

	internal void PurgeUnusedGraphicsResources()
	{
		for (int i = 0; i < 3; i++)
		{
			m_RenderGraphResources[i].PurgeUnusedGraphicsResources(m_CurrentFrameIndex);
		}
	}

	internal void Cleanup()
	{
		for (int i = 0; i < 3; i++)
		{
			m_RenderGraphResources[i].Cleanup();
		}
		RTHandles.Release(m_CurrentBackbuffer);
	}

	private void LogResources()
	{
		if (!m_RenderGraphDebug.enableLogging)
		{
			return;
		}
		m_ResourceLogger.LogLine("==== Render Graph Resource Log ====\n");
		for (int i = 0; i < 3; i++)
		{
			if (m_RenderGraphResources[i].pool != null)
			{
				m_RenderGraphResources[i].pool.LogResources(m_ResourceLogger);
				m_ResourceLogger.LogLine("");
			}
		}
	}

	internal void FlushLogs()
	{
		m_ResourceLogger.FlushLogs();
	}
}
