using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("Texture ({handle.index})")]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct TextureHandle
{
	private static TextureHandle s_NullHandle;

	internal ResourceHandle handle;

	private bool builtin;

	public static TextureHandle nullHandle => s_NullHandle;

	internal TextureHandle(in ResourceHandle h)
	{
		handle = h;
		builtin = false;
	}

	internal TextureHandle(int handle, bool shared = false, bool builtin = false)
	{
		this.handle = new ResourceHandle(handle, RenderGraphResourceType.Texture, shared);
		this.builtin = builtin;
	}

	public static implicit operator RenderTargetIdentifier(TextureHandle texture)
	{
		if (!texture.IsValid())
		{
			return default(RenderTargetIdentifier);
		}
		return RenderGraphResourceRegistry.current.GetTexture(in texture);
	}

	public static implicit operator Texture(TextureHandle texture)
	{
		return texture.IsValid() ? RenderGraphResourceRegistry.current.GetTexture(in texture) : null;
	}

	public static implicit operator RenderTexture(TextureHandle texture)
	{
		return texture.IsValid() ? RenderGraphResourceRegistry.current.GetTexture(in texture) : null;
	}

	public static implicit operator RTHandle(TextureHandle texture)
	{
		if (!texture.IsValid())
		{
			return null;
		}
		return RenderGraphResourceRegistry.current.GetTexture(in texture);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsValid()
	{
		return handle.IsValid();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsBuiltin()
	{
		return builtin;
	}

	public TextureDesc GetDescriptor(RenderGraph renderGraph)
	{
		return renderGraph.GetTextureDesc(in this);
	}
}
