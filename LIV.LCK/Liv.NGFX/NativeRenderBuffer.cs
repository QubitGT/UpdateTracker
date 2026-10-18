using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Liv.NGFX;

public class NativeRenderBuffer : IDisposable
{
	public enum Format
	{
		RGBA,
		Depth
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RenderBufferCreateInfo
	{
		public static EventType eventType = EventType.RenderBufferCreate;

		private IntPtr m_context;

		private IntPtr m_handle;

		private int m_width;

		private int m_height;

		private int m_mips;

		private GraphicsFormat m_format;

		private uint m_out_id;

		public RenderBufferCreateInfo(IntPtr ctx, uint id, IntPtr handle, int width, int height, int mips, GraphicsFormat format)
		{
			m_context = ctx;
			m_handle = handle;
			m_width = width;
			m_height = height;
			m_mips = mips;
			m_format = format;
			m_out_id = id;
		}

		public uint id()
		{
			return m_out_id;
		}
	}

	private RenderBuffer m_buffer;

	private uint m_id;

	private int m_mips;

	private bool m_valid;

	private IntPtr m_context = IntPtr.Zero;

	public uint id => m_id;

	public RenderBuffer buffer => m_buffer;

	public NativeRenderBuffer(IntPtr ctx, RenderBuffer rb, int width, int height, int mips, GraphicsFormat format)
	{
		m_context = ctx;
		m_buffer = rb;
		m_mips = mips;
		m_id = NI.AllocResource(m_context);
		Handle<RenderBufferCreateInfo> handle = new Handle<RenderBufferCreateInfo>(new RenderBufferCreateInfo(ctx, m_id, rb.GetNativeRenderBufferPtr(), width, height, mips, format));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)RenderBufferCreateInfo.eventType, handle.ptr());
		Graphics.ExecuteCommandBuffer(commandBuffer);
		m_valid = true;
	}

	public NativeRenderBuffer(IntPtr ctx, RenderBuffer rb, IntPtr texturePtr, int width, int height, int mips, GraphicsFormat format)
	{
		m_context = ctx;
		m_buffer = rb;
		m_mips = mips;
		m_id = NI.AllocResource(m_context);
		Handle<RenderBufferCreateInfo> handle = new Handle<RenderBufferCreateInfo>(new RenderBufferCreateInfo(ctx, m_id, texturePtr, width, height, mips, format));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)RenderBufferCreateInfo.eventType, handle.ptr());
		Graphics.ExecuteCommandBuffer(commandBuffer);
		m_valid = true;
	}

	~NativeRenderBuffer()
	{
	}

	public static implicit operator RenderBuffer(NativeRenderBuffer o)
	{
		return o.m_buffer;
	}

	public void Dispose()
	{
		if (m_valid)
		{
			Handle<ResourceDestroyInfo> handle = new Handle<ResourceDestroyInfo>(new ResourceDestroyInfo(m_context, m_id));
			CommandBuffer commandBuffer = new CommandBuffer();
			commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)ResourceDestroyInfo.eventType, handle.ptr());
			Graphics.ExecuteCommandBuffer(commandBuffer);
			m_valid = false;
		}
	}
}
