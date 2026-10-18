using System;
using System.Diagnostics;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public class BaseCommandBuffer
{
	protected internal CommandBuffer m_WrappedCommandBuffer;

	internal RenderGraphPass m_ExecutingPass;

	public string name => m_WrappedCommandBuffer.name;

	public int sizeInBytes => m_WrappedCommandBuffer.sizeInBytes;

	internal BaseCommandBuffer(CommandBuffer wrapped, RenderGraphPass executingPass, bool isAsync)
	{
		m_WrappedCommandBuffer = wrapped;
		m_ExecutingPass = executingPass;
		if (isAsync)
		{
			m_WrappedCommandBuffer.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	protected internal void ThrowIfGlobalStateNotAllowed()
	{
		if (m_ExecutingPass != null && !m_ExecutingPass.allowGlobalState)
		{
			throw new InvalidOperationException(m_ExecutingPass.name + ": Modifying global state from this command buffer is not allowed. Please ensure your render graph pass allows modifying global state.");
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	protected internal void ThrowIfRasterNotAllowed()
	{
		if (m_ExecutingPass != null && !m_ExecutingPass.HasRenderAttachments())
		{
			throw new InvalidOperationException(m_ExecutingPass.name + ": Using raster commands from a pass with no active render target is not allowed as it will use an undefined render target state. Please set up pass render targets using SetRenderAttachments.");
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	protected internal void ValidateTextureHandle(TextureHandle h)
	{
		if (RenderGraph.enableValidityChecks && m_ExecutingPass != null && !h.IsBuiltin())
		{
			if (!m_ExecutingPass.IsRead(in h.handle) && !m_ExecutingPass.IsWritten(in h.handle) && !m_ExecutingPass.IsTransient(in h.handle))
			{
				throw new Exception("Pass '" + m_ExecutingPass.name + "' is trying to bind a texture on the command buffer that is not registered by its builder. Please indicate to the pass builder how the texture is used (UseTexture/CreateTransientTexture).");
			}
			if (m_ExecutingPass.IsAttachment(in h))
			{
				throw new Exception("Pass '" + m_ExecutingPass.name + "' is trying to bind a texture on the command buffer that is already set as a fragment attachment (SetRenderAttachment/SetRenderAttachmentDepth). A texture cannot be used as both in one pass, please fix its usage in the pass builder.");
			}
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	protected internal void ValidateTextureHandleRead(TextureHandle h)
	{
		if (RenderGraph.enableValidityChecks && m_ExecutingPass != null)
		{
			if (!m_ExecutingPass.IsRead(in h.handle) && !m_ExecutingPass.IsTransient(in h.handle))
			{
				throw new Exception("Pass '" + m_ExecutingPass.name + "' is trying to read a texture on the command buffer that is not registered by its builder. Please indicate to the pass builder that the texture is read (UseTexture/CreateTransientTexture).");
			}
			if (m_ExecutingPass.IsAttachment(in h))
			{
				throw new Exception("Pass '" + m_ExecutingPass.name + "' is trying to bind a texture on the command buffer that is already set as a fragment attachment (SetRenderAttachment/SetRenderAttachmentDepth). A texture cannot be used as both in one pass, please fix its usage in the pass builder.");
			}
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	protected internal void ValidateTextureHandleWrite(TextureHandle h)
	{
		if (RenderGraph.enableValidityChecks && m_ExecutingPass != null)
		{
			if (h.IsBuiltin())
			{
				throw new Exception("Pass '" + m_ExecutingPass.name + "' is trying to write to a built-in texture. This is not allowed built-in textures are small default resources like `white` or `black` that cannot be written to.");
			}
			if (!m_ExecutingPass.IsWritten(in h.handle) && !m_ExecutingPass.IsTransient(in h.handle))
			{
				throw new Exception("Pass '" + m_ExecutingPass.name + "' is trying to write a texture on the command buffer that is not registered by its builder. Please indicate to the pass builder that the texture is written (UseTexture/CreateTransientTexture).");
			}
			if (m_ExecutingPass.IsAttachment(in h))
			{
				throw new Exception("Pass '" + m_ExecutingPass.name + "' is trying to bind a texture on the command buffer that is already set as a fragment attachment (SetRenderAttachment/SetRenderAttachmentDepth). A texture cannot be used as both in one pass, please fix its usage in the pass builder.");
			}
		}
	}
}
