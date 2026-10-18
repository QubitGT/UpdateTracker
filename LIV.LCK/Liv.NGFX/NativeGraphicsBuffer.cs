using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Liv.NGFX;

public class NativeGraphicsBuffer<T> : IDisposable
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BufferCreateInfo
	{
		public static EventType eventType;

		private IntPtr m_context;

		private IntPtr m_handle;

		private int m_count;

		private int m_stride;

		private GraphicsBuffer.Target m_target;

		private uint m_out_id;

		public BufferCreateInfo(IntPtr ctx, uint id, IntPtr handle, int count, int stride, GraphicsBuffer.Target target)
		{
			m_context = ctx;
			m_handle = handle;
			m_count = count;
			m_stride = stride;
			m_target = target;
			m_out_id = id;
		}

		public uint id()
		{
			return m_out_id;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BufferCopyInfo
	{
		public static EventType eventType = EventType.GraphicsBufferCopy;

		private IntPtr m_context;

		private uint m_src;

		private uint m_dst;

		private uint m_size;

		public BufferCopyInfo(IntPtr ctx, uint src, uint dst, uint size)
		{
			m_context = ctx;
			m_src = src;
			m_dst = dst;
			m_size = size;
		}
	}

	private GraphicsBuffer m_buffer;

	private uint m_id;

	private int m_count;

	private bool m_valid;

	private IntPtr m_context = IntPtr.Zero;

	public uint id => m_id;

	public int count => m_count;

	public GraphicsBuffer buffer => m_buffer;

	public NativeGraphicsBuffer(IntPtr ctx, int count, GraphicsBuffer.Target target)
	{
		m_context = ctx;
		m_count = count;
		int stride = Marshal.SizeOf(typeof(T));
		m_buffer = new GraphicsBuffer(target, count, stride);
		m_buffer.GetNativeBufferPtr();
		m_id = NI.AllocResource(m_context);
		m_buffer.name = "NativeGraphicsBuffer " + m_id;
		Handle<BufferCreateInfo> handle = new Handle<BufferCreateInfo>(new BufferCreateInfo(ctx, m_id, m_buffer.GetNativeBufferPtr(), count, stride, target));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)BufferCreateInfo.eventType, handle.ptr());
		Graphics.ExecuteCommandBuffer(commandBuffer);
		m_valid = true;
	}

	public NativeGraphicsBuffer(IntPtr ctx, int count, IntPtr nativeBuffer, GraphicsBuffer.Target target)
	{
		m_context = ctx;
		m_count = count;
		int stride = Marshal.SizeOf(typeof(T));
		m_id = NI.AllocResource(m_context);
		Handle<BufferCreateInfo> handle = new Handle<BufferCreateInfo>(new BufferCreateInfo(ctx, m_id, nativeBuffer, count, stride, target));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)BufferCreateInfo.eventType, handle.ptr());
		Graphics.ExecuteCommandBuffer(commandBuffer);
		m_valid = true;
	}

	public NativeGraphicsBuffer(IntPtr ctx, GraphicsBuffer buffer, GraphicsBuffer.Target target)
	{
		m_context = ctx;
		m_count = buffer.count;
		int stride = buffer.stride;
		m_buffer = buffer;
		m_buffer.GetNativeBufferPtr();
		m_id = NI.AllocResource(m_context);
		Handle<BufferCreateInfo> handle = new Handle<BufferCreateInfo>(new BufferCreateInfo(ctx, m_id, m_buffer.GetNativeBufferPtr(), count, stride, target));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)BufferCreateInfo.eventType, handle.ptr());
		Graphics.ExecuteCommandBuffer(commandBuffer);
		m_valid = true;
	}

	~NativeGraphicsBuffer()
	{
	}

	public void BufferCopy(NativeGraphicsBuffer<T> dst, uint size)
	{
		Handle<BufferCopyInfo> handle = new Handle<BufferCopyInfo>(new BufferCopyInfo(m_context, m_id, dst.m_id, size));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)BufferCopyInfo.eventType, handle.ptr());
		Graphics.ExecuteCommandBuffer(commandBuffer);
	}

	public static implicit operator GraphicsBuffer(NativeGraphicsBuffer<T> o)
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
			m_buffer.Dispose();
			m_buffer = null;
		}
	}
}
