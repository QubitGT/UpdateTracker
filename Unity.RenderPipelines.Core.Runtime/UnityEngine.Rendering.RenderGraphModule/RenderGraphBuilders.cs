using System;
using System.Diagnostics;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace UnityEngine.Rendering.RenderGraphModule;

internal class RenderGraphBuilders : IBaseRenderGraphBuilder, IDisposable, IComputeRenderGraphBuilder, IRasterRenderGraphBuilder, IUnsafeRenderGraphBuilder
{
	private RenderGraphPass m_RenderPass;

	private RenderGraphResourceRegistry m_Resources;

	private RenderGraph m_RenderGraph;

	private bool m_Disposed;

	public RenderGraphBuilders()
	{
		m_RenderPass = null;
		m_Resources = null;
		m_RenderGraph = null;
		m_Disposed = true;
	}

	public void Setup(RenderGraphPass renderPass, RenderGraphResourceRegistry resources, RenderGraph renderGraph)
	{
		m_RenderPass = renderPass;
		m_Resources = resources;
		m_RenderGraph = renderGraph;
		m_Disposed = false;
		renderPass.useAllGlobalTextures = false;
		if (renderPass.type == RenderGraphPassType.Raster)
		{
			CommandBuffer.ThrowOnSetRenderTarget = true;
		}
	}

	public void EnableAsyncCompute(bool value)
	{
		m_RenderPass.EnableAsyncCompute(value);
	}

	public void AllowPassCulling(bool value)
	{
		if (!value || !m_RenderPass.allowGlobalState)
		{
			m_RenderPass.AllowPassCulling(value);
		}
	}

	public void AllowGlobalStateModification(bool value)
	{
		m_RenderPass.AllowGlobalState(value);
		if (value)
		{
			AllowPassCulling(value: false);
		}
	}

	public void EnableFoveatedRasterization(bool value)
	{
		m_RenderPass.EnableFoveatedRasterization(value);
	}

	public BufferHandle CreateTransientBuffer(in BufferDesc desc)
	{
		BufferHandle result = m_Resources.CreateBuffer(in desc, m_RenderPass.index);
		UseResource(in result.handle, AccessFlags.ReadWrite, isTransient: true);
		return result;
	}

	public BufferHandle CreateTransientBuffer(in BufferHandle computebuffer)
	{
		BufferDesc desc = m_Resources.GetBufferResourceDesc(in computebuffer.handle);
		return CreateTransientBuffer(in desc);
	}

	public TextureHandle CreateTransientTexture(in TextureDesc desc)
	{
		TextureHandle result = m_Resources.CreateTexture(in desc, m_RenderPass.index);
		UseResource(in result.handle, AccessFlags.ReadWrite, isTransient: true);
		return result;
	}

	public TextureHandle CreateTransientTexture(in TextureHandle texture)
	{
		TextureDesc desc = m_Resources.GetTextureResourceDesc(in texture.handle);
		return CreateTransientTexture(in desc);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_Disposed)
		{
			return;
		}
		try
		{
			if (!disposing)
			{
				return;
			}
			m_RenderGraph.RenderGraphState = RenderGraphState.RecordingGraph;
			if (m_RenderPass.useAllGlobalTextures)
			{
				foreach (TextureHandle item in m_RenderGraph.AllGlobals())
				{
					TextureHandle input = item;
					if (input.IsValid())
					{
						UseTexture(in input, AccessFlags.Read);
					}
				}
			}
			foreach (var setGlobals in m_RenderPass.setGlobalsList)
			{
				m_RenderGraph.SetGlobal(setGlobals.Item1, setGlobals.Item2);
			}
			m_RenderGraph.OnPassAdded(m_RenderPass);
		}
		finally
		{
			if (m_RenderPass.type == RenderGraphPassType.Raster)
			{
				CommandBuffer.ThrowOnSetRenderTarget = false;
			}
			m_RenderPass = null;
			m_Resources = null;
			m_RenderGraph = null;
			m_Disposed = true;
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void ValidateWriteTo(in ResourceHandle handle)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (handle.IsVersioned)
			{
				string renderGraphResourceName = m_Resources.GetRenderGraphResourceName(in handle);
				throw new InvalidOperationException("Trying to write to a versioned resource handle. You can only write to unversioned resource handles to avoid branches in the resource history. (pass " + m_RenderPass.name + " resource" + renderGraphResourceName + ").");
			}
			if (m_RenderPass.IsWritten(in handle))
			{
				string renderGraphResourceName2 = m_Resources.GetRenderGraphResourceName(in handle);
				throw new InvalidOperationException("Trying to write a resource twice in a pass. You can only write the same resource once within a pass (pass " + m_RenderPass.name + " resource" + renderGraphResourceName2 + ").");
			}
		}
	}

	private ResourceHandle UseResource(in ResourceHandle handle, AccessFlags flags, bool isTransient = false)
	{
		if ((flags & AccessFlags.Discard) == 0)
		{
			ResourceHandle res = (handle.IsVersioned ? handle : m_Resources.GetLatestVersionHandle(in handle));
			if (isTransient)
			{
				m_RenderPass.AddTransientResource(in res);
				return GetLatestVersionHandle(in handle);
			}
			m_RenderPass.AddResourceRead(in res);
			m_Resources.IncrementReadCount(in handle);
			if ((flags & AccessFlags.Read) == 0)
			{
				m_RenderPass.implicitReadsList.Add(res);
			}
		}
		else if ((flags & AccessFlags.Read) != 0)
		{
			RenderGraphPass renderPass = m_RenderPass;
			ResourceHandle res2 = m_Resources.GetZeroVersionedHandle(in handle);
			renderPass.AddResourceRead(in res2);
			m_Resources.IncrementReadCount(in handle);
		}
		if ((flags & AccessFlags.Write) != 0)
		{
			RenderGraphPass renderPass2 = m_RenderPass;
			ResourceHandle res2 = m_Resources.GetNewVersionedHandle(in handle);
			renderPass2.AddResourceWrite(in res2);
			m_Resources.IncrementWriteCount(in handle);
		}
		return GetLatestVersionHandle(in handle);
	}

	public BufferHandle UseBuffer(in BufferHandle input, AccessFlags flags)
	{
		UseResource(in input.handle, flags);
		return input;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckNotUseFragment(TextureHandle tex)
	{
		if (!RenderGraph.enableValidityChecks)
		{
			return;
		}
		bool flag = false;
		flag = m_RenderPass.depthAccess.textureHandle.IsValid() && m_RenderPass.depthAccess.textureHandle.handle.index == tex.handle.index;
		if (!flag)
		{
			for (int i = 0; i <= m_RenderPass.colorBufferMaxIndex; i++)
			{
				if (m_RenderPass.colorBufferAccess[i].textureHandle.IsValid() && m_RenderPass.colorBufferAccess[i].textureHandle.handle.index == tex.handle.index)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			string renderGraphResourceName = m_Resources.GetRenderGraphResourceName(in tex.handle);
			throw new ArgumentException("Trying to UseTexture on a texture that is already used through SetRenderAttachment. Consider updating your code. (pass " + m_RenderPass.name + " resource" + renderGraphResourceName + ").");
		}
	}

	public void UseTexture(in TextureHandle input, AccessFlags flags)
	{
		UseResource(in input.handle, flags);
	}

	public void UseGlobalTexture(int propertyId, AccessFlags flags)
	{
		TextureHandle input = m_RenderGraph.GetGlobal(propertyId);
		if (input.IsValid())
		{
			UseTexture(in input, flags);
			return;
		}
		throw new ArgumentException($"Trying to read global texture property {propertyId} but no previous pass in the graph assigned a value to this global.");
	}

	public void UseAllGlobalTextures(bool enable)
	{
		m_RenderPass.useAllGlobalTextures = enable;
	}

	public void SetGlobalTextureAfterPass(in TextureHandle input, int propertyId)
	{
		m_RenderPass.setGlobalsList.Add(ValueTuple.Create(input, propertyId));
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckUseFragment(TextureHandle tex, bool isDepth)
	{
		if (!RenderGraph.enableValidityChecks)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < m_RenderPass.resourceReadLists[tex.handle.iType].Count; i++)
		{
			if (m_RenderPass.resourceReadLists[tex.handle.iType][i].index == tex.handle.index)
			{
				flag = true;
				break;
			}
		}
		for (int j = 0; j < m_RenderPass.resourceWriteLists[tex.handle.iType].Count; j++)
		{
			if (m_RenderPass.resourceWriteLists[tex.handle.iType][j].index == tex.handle.index)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			string renderGraphResourceName = m_Resources.GetRenderGraphResourceName(in tex.handle);
			throw new InvalidOperationException("Trying to SetRenderAttachment on a texture that is already used through UseTexture/SetRenderAttachment. Consider updating your code. (pass '" + m_RenderPass.name + "' resource '" + renderGraphResourceName + "').");
		}
		m_Resources.GetRenderTargetInfo(in tex.handle, out var outInfo);
		if (m_RenderGraph.nativeRenderPassesEnabled)
		{
			if (isDepth)
			{
				if (!GraphicsFormatUtility.IsDepthFormat(outInfo.format))
				{
					string renderGraphResourceName2 = m_Resources.GetRenderGraphResourceName(in tex.handle);
					throw new InvalidOperationException($"Trying to SetRenderAttachmentDepth on a texture that has a color format {outInfo.format}. Use a texture with a depth format instead. (pass '{m_RenderPass.name}' resource '{renderGraphResourceName2}').");
				}
			}
			else if (GraphicsFormatUtility.IsDepthFormat(outInfo.format))
			{
				string renderGraphResourceName3 = m_Resources.GetRenderGraphResourceName(in tex.handle);
				throw new InvalidOperationException("Trying to SetRenderAttachment on a texture that has a depth format. Use a texture with a color format instead. (pass '" + m_RenderPass.name + "' resource '" + renderGraphResourceName3 + "').");
			}
		}
		foreach (var setGlobals in m_RenderPass.setGlobalsList)
		{
			if (setGlobals.Item1.handle.index == tex.handle.index)
			{
				throw new InvalidOperationException("Trying to SetRenderAttachment on a texture that is currently set on a global texture slot. Shaders might be using the texture using samplers. You should ensure textures are not set as globals when using them as fragment attachments.");
			}
		}
	}

	public void SetRenderAttachment(TextureHandle tex, int index, AccessFlags flags, int mipLevel, int depthSlice)
	{
		ResourceHandle handle = UseResource(in tex.handle, flags);
		TextureHandle resource = default(TextureHandle);
		resource.handle = handle;
		m_RenderPass.SetColorBufferRaw(in resource, index, flags, mipLevel, depthSlice);
	}

	public void SetInputAttachment(TextureHandle tex, int index, AccessFlags flags, int mipLevel, int depthSlice)
	{
		ResourceHandle handle = UseResource(in tex.handle, flags);
		TextureHandle resource = default(TextureHandle);
		resource.handle = handle;
		m_RenderPass.SetFragmentInputRaw(in resource, index, flags, mipLevel, depthSlice);
	}

	public void SetRenderAttachmentDepth(TextureHandle tex, AccessFlags flags, int mipLevel, int depthSlice)
	{
		ResourceHandle handle = UseResource(in tex.handle, flags);
		TextureHandle resource = default(TextureHandle);
		resource.handle = handle;
		m_RenderPass.SetDepthBufferRaw(in resource, flags, mipLevel, depthSlice);
	}

	public TextureHandle SetRandomAccessAttachment(TextureHandle input, int index, AccessFlags flags = AccessFlags.Read)
	{
		ResourceHandle handle = UseResource(in input.handle, flags);
		TextureHandle textureHandle = default(TextureHandle);
		textureHandle.handle = handle;
		m_RenderPass.SetRandomWriteResourceRaw(in textureHandle.handle, index, preserveCounterValue: false, flags);
		return input;
	}

	public BufferHandle UseBufferRandomAccess(BufferHandle input, int index, AccessFlags flags = AccessFlags.Read)
	{
		BufferHandle bufferHandle = UseBuffer(in input, flags);
		m_RenderPass.SetRandomWriteResourceRaw(in bufferHandle.handle, index, preserveCounterValue: true, flags);
		return input;
	}

	public BufferHandle UseBufferRandomAccess(BufferHandle input, int index, bool preserveCounterValue, AccessFlags flags = AccessFlags.Read)
	{
		BufferHandle bufferHandle = UseBuffer(in input, flags);
		m_RenderPass.SetRandomWriteResourceRaw(in bufferHandle.handle, index, preserveCounterValue, flags);
		return input;
	}

	public void SetRenderFunc<PassData>(BaseRenderFunc<PassData, ComputeGraphContext> renderFunc) where PassData : class, new()
	{
		((ComputeRenderGraphPass<PassData>)m_RenderPass).renderFunc = renderFunc;
	}

	public void SetRenderFunc<PassData>(BaseRenderFunc<PassData, RasterGraphContext> renderFunc) where PassData : class, new()
	{
		((RasterRenderGraphPass<PassData>)m_RenderPass).renderFunc = renderFunc;
	}

	public void SetRenderFunc<PassData>(BaseRenderFunc<PassData, UnsafeGraphContext> renderFunc) where PassData : class, new()
	{
		((UnsafeRenderGraphPass<PassData>)m_RenderPass).renderFunc = renderFunc;
	}

	public void UseRendererList(in RendererListHandle input)
	{
		m_RenderPass.UseRendererList(in input);
	}

	private ResourceHandle GetLatestVersionHandle(in ResourceHandle handle)
	{
		if (m_Resources.GetRenderGraphResourceTransientIndex(in handle) >= 0)
		{
			return handle;
		}
		return m_Resources.GetLatestVersionHandle(in handle);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckResource(in ResourceHandle res, bool checkTransientReadWrite = false)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (!res.IsValid())
			{
				throw new Exception("Trying to use an invalid resource (pass " + m_RenderPass.name + ").");
			}
			int renderGraphResourceTransientIndex = m_Resources.GetRenderGraphResourceTransientIndex(in res);
			if (renderGraphResourceTransientIndex == m_RenderPass.index && checkTransientReadWrite)
			{
				Debug.LogError("Trying to read or write a transient resource at pass " + m_RenderPass.name + ".Transient resource are always assumed to be both read and written.");
			}
			if (renderGraphResourceTransientIndex != -1 && renderGraphResourceTransientIndex != m_RenderPass.index)
			{
				throw new ArgumentException($"Trying to use a transient {res.type} (pass index {renderGraphResourceTransientIndex}) in a different pass (pass index {m_RenderPass.index}).");
			}
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckFrameBufferFetchEmulationIsSupported(in TextureHandle tex)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (!RenderGraphUtils.IsFramebufferFetchEmulationSupportedOnCurrentPlatform())
			{
				throw new InvalidOperationException($"This API is not supported on the current platform: {SystemInfo.graphicsDeviceType}");
			}
			if (!RenderGraphUtils.IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform() && m_RenderGraph.GetRenderTargetInfo(tex).bindMS)
			{
				throw new InvalidOperationException($"This API is not supported with MSAA attachments on the current platform: {SystemInfo.graphicsDeviceType}");
			}
		}
	}

	public void SetShadingRateImageAttachment(in TextureHandle sriTextureHandle)
	{
		TextureHandle shadingRateImage = default(TextureHandle);
		shadingRateImage.handle = UseResource(in sriTextureHandle.handle, AccessFlags.Read);
		m_RenderPass.SetShadingRateImage(in shadingRateImage, AccessFlags.Read, 0, 0);
	}

	public void SetShadingRateFragmentSize(ShadingRateFragmentSize shadingRateFragmentSize)
	{
		m_RenderPass.SetShadingRateFragmentSize(shadingRateFragmentSize);
	}

	public void SetShadingRateCombiner(ShadingRateCombinerStage stage, ShadingRateCombiner combiner)
	{
		m_RenderPass.SetShadingRateCombiner(stage, combiner);
	}

	void IRasterRenderGraphBuilder.SetShadingRateImageAttachment(in TextureHandle tex)
	{
		SetShadingRateImageAttachment(in tex);
	}

	void IBaseRenderGraphBuilder.UseTexture(in TextureHandle input, AccessFlags flags)
	{
		UseTexture(in input, flags);
	}

	void IBaseRenderGraphBuilder.SetGlobalTextureAfterPass(in TextureHandle input, int propertyId)
	{
		SetGlobalTextureAfterPass(in input, propertyId);
	}

	BufferHandle IBaseRenderGraphBuilder.UseBuffer(in BufferHandle input, AccessFlags flags)
	{
		return UseBuffer(in input, flags);
	}

	TextureHandle IBaseRenderGraphBuilder.CreateTransientTexture(in TextureDesc desc)
	{
		return CreateTransientTexture(in desc);
	}

	TextureHandle IBaseRenderGraphBuilder.CreateTransientTexture(in TextureHandle texture)
	{
		return CreateTransientTexture(in texture);
	}

	BufferHandle IBaseRenderGraphBuilder.CreateTransientBuffer(in BufferDesc desc)
	{
		return CreateTransientBuffer(in desc);
	}

	BufferHandle IBaseRenderGraphBuilder.CreateTransientBuffer(in BufferHandle computebuffer)
	{
		return CreateTransientBuffer(in computebuffer);
	}

	void IBaseRenderGraphBuilder.UseRendererList(in RendererListHandle input)
	{
		UseRendererList(in input);
	}
}
