using System;
using System.Diagnostics;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct RenderGraphBuilder : IDisposable
{
	private RenderGraphPass m_RenderPass;

	private RenderGraphResourceRegistry m_Resources;

	private RenderGraph m_RenderGraph;

	private bool m_Disposed;

	public TextureHandle UseColorBuffer(in TextureHandle input, int index)
	{
		m_Resources.IncrementWriteCount(in input.handle);
		m_RenderPass.SetColorBuffer(in input, index);
		return input;
	}

	public TextureHandle UseDepthBuffer(in TextureHandle input, DepthAccess flags)
	{
		if ((flags & DepthAccess.Write) != 0)
		{
			m_Resources.IncrementWriteCount(in input.handle);
		}
		if ((flags & DepthAccess.Read) != 0 && !m_Resources.IsRenderGraphResourceImported(in input.handle) && m_Resources.TextureNeedsFallback(in input))
		{
			WriteTexture(in input);
		}
		m_RenderPass.SetDepthBuffer(in input, flags);
		return input;
	}

	public TextureHandle ReadTexture(in TextureHandle input)
	{
		if (!m_Resources.IsRenderGraphResourceImported(in input.handle) && m_Resources.TextureNeedsFallback(in input))
		{
			TextureResource textureResource = m_Resources.GetTextureResource(in input.handle);
			textureResource.desc.clearBuffer = true;
			textureResource.desc.clearColor = Color.black;
			if (m_RenderGraph.GetImportedFallback(textureResource.desc, out var fallback))
			{
				return fallback;
			}
			WriteTexture(in input);
		}
		m_RenderPass.AddResourceRead(in input.handle);
		return input;
	}

	public TextureHandle WriteTexture(in TextureHandle input)
	{
		m_Resources.IncrementWriteCount(in input.handle);
		m_RenderPass.AddResourceWrite(in input.handle);
		return input;
	}

	public TextureHandle ReadWriteTexture(in TextureHandle input)
	{
		m_Resources.IncrementWriteCount(in input.handle);
		m_RenderPass.AddResourceWrite(in input.handle);
		m_RenderPass.AddResourceRead(in input.handle);
		return input;
	}

	public TextureHandle CreateTransientTexture(in TextureDesc desc)
	{
		TextureHandle result = m_Resources.CreateTexture(in desc, m_RenderPass.index);
		m_RenderPass.AddTransientResource(in result.handle);
		return result;
	}

	public TextureHandle CreateTransientTexture(in TextureHandle texture)
	{
		TextureDesc desc = m_Resources.GetTextureResourceDesc(in texture.handle);
		TextureHandle result = m_Resources.CreateTexture(in desc, m_RenderPass.index);
		m_RenderPass.AddTransientResource(in result.handle);
		return result;
	}

	public RayTracingAccelerationStructureHandle WriteRayTracingAccelerationStructure(in RayTracingAccelerationStructureHandle input)
	{
		m_Resources.IncrementWriteCount(in input.handle);
		m_RenderPass.AddResourceWrite(in input.handle);
		return input;
	}

	public RayTracingAccelerationStructureHandle ReadRayTracingAccelerationStructure(in RayTracingAccelerationStructureHandle input)
	{
		m_RenderPass.AddResourceRead(in input.handle);
		return input;
	}

	public RendererListHandle UseRendererList(in RendererListHandle input)
	{
		if (input.IsValid())
		{
			m_RenderPass.UseRendererList(in input);
		}
		return input;
	}

	public BufferHandle ReadBuffer(in BufferHandle input)
	{
		m_RenderPass.AddResourceRead(in input.handle);
		return input;
	}

	public BufferHandle WriteBuffer(in BufferHandle input)
	{
		m_RenderPass.AddResourceWrite(in input.handle);
		m_Resources.IncrementWriteCount(in input.handle);
		return input;
	}

	public BufferHandle CreateTransientBuffer(in BufferDesc desc)
	{
		BufferHandle result = m_Resources.CreateBuffer(in desc, m_RenderPass.index);
		m_RenderPass.AddTransientResource(in result.handle);
		return result;
	}

	public BufferHandle CreateTransientBuffer(in BufferHandle graphicsbuffer)
	{
		BufferDesc desc = m_Resources.GetBufferResourceDesc(in graphicsbuffer.handle);
		BufferHandle result = m_Resources.CreateBuffer(in desc, m_RenderPass.index);
		m_RenderPass.AddTransientResource(in result.handle);
		return result;
	}

	public void SetRenderFunc<PassData>(BaseRenderFunc<PassData, RenderGraphContext> renderFunc) where PassData : class, new()
	{
		((RenderGraphPass<PassData>)m_RenderPass).renderFunc = renderFunc;
	}

	public void EnableAsyncCompute(bool value)
	{
		m_RenderPass.EnableAsyncCompute(value);
	}

	public void AllowPassCulling(bool value)
	{
		m_RenderPass.AllowPassCulling(value);
	}

	public void EnableFoveatedRasterization(bool value)
	{
		m_RenderPass.EnableFoveatedRasterization(value);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void AllowRendererListCulling(bool value)
	{
		m_RenderPass.AllowRendererListCulling(value);
	}

	public RendererListHandle DependsOn(in RendererListHandle input)
	{
		m_RenderPass.UseRendererList(in input);
		return input;
	}

	internal RenderGraphBuilder(RenderGraphPass renderPass, RenderGraphResourceRegistry resources, RenderGraph renderGraph)
	{
		m_RenderPass = renderPass;
		m_Resources = resources;
		m_RenderGraph = renderGraph;
		m_Disposed = false;
	}

	private void Dispose(bool disposing)
	{
		if (!m_Disposed)
		{
			m_RenderGraph.RenderGraphState = RenderGraphState.RecordingGraph;
			m_RenderGraph.OnPassAdded(m_RenderPass);
			m_Disposed = true;
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void CheckResource(in ResourceHandle res, bool checkTransientReadWrite = true)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (!res.IsValid())
			{
				throw new ArgumentException("Trying to use an invalid resource (pass " + m_RenderPass.name + ").");
			}
			int renderGraphResourceTransientIndex = m_Resources.GetRenderGraphResourceTransientIndex(in res);
			if (renderGraphResourceTransientIndex == m_RenderPass.index && checkTransientReadWrite)
			{
				Debug.LogError("Trying to read or write a transient resource at pass " + m_RenderPass.name + ".Transient resource are always assumed to be both read and written.");
			}
			if (renderGraphResourceTransientIndex != -1 && renderGraphResourceTransientIndex != m_RenderPass.index)
			{
				throw new ArgumentException($"Trying to use a transient texture (pass index {renderGraphResourceTransientIndex}) in a different pass (pass index {m_RenderPass.index}).");
			}
		}
	}

	internal void GenerateDebugData(bool value)
	{
		m_RenderPass.GenerateDebugData(value);
	}
}
