using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Liv.NGFX;

public class NativeTexture : IDisposable
{
	public enum Format
	{
		RGBA,
		Depth
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TextureCreateInfo
	{
		public static EventType eventType = EventType.TextureCreate;

		private IntPtr m_context;

		private IntPtr m_handle;

		private int m_width;

		private int m_height;

		private Format m_format;

		private uint m_out_id;

		public TextureCreateInfo(IntPtr ctx, uint id, IntPtr handle, int width, int height, Format format)
		{
			m_context = ctx;
			m_handle = handle;
			m_width = width;
			m_height = height;
			m_format = format;
			m_out_id = id;
		}

		public uint id()
		{
			return m_out_id;
		}
	}

	private Texture2D m_texture;

	private uint m_id;

	private bool m_valid;

	private IntPtr m_context = IntPtr.Zero;

	public uint id => m_id;

	public Texture2D texture => m_texture;

	public NativeTexture(IntPtr ctx, int width, int height, Format format)
	{
		m_context = ctx;
		m_texture = new Texture2D(width, height, FormatToUnity(format), mipChain: false);
		m_id = NI.AllocResource(m_context);
		Handle<TextureCreateInfo> handle = new Handle<TextureCreateInfo>(new TextureCreateInfo(ctx, m_id, m_texture.GetNativeTexturePtr(), width, height, format));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(NI.GetPluginEventFunction(), (int)TextureCreateInfo.eventType, handle.ptr());
		Graphics.ExecuteCommandBuffer(commandBuffer);
		m_valid = true;
	}

	~NativeTexture()
	{
	}

	public TextureFormat FormatToUnity(Format fmt)
	{
		return fmt switch
		{
			Format.RGBA => TextureFormat.RGBA32, 
			Format.Depth => TextureFormat.R16, 
			_ => throw new NotSupportedException(), 
		};
	}

	public static implicit operator Texture2D(NativeTexture o)
	{
		return o.m_texture;
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
			m_texture = null;
		}
	}
}
