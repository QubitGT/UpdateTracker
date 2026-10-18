using System.Diagnostics;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("Buffer ({handle.index})")]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct BufferHandle
{
	private static BufferHandle s_NullHandle;

	internal ResourceHandle handle;

	public static BufferHandle nullHandle => s_NullHandle;

	internal BufferHandle(in ResourceHandle h)
	{
		handle = h;
	}

	internal BufferHandle(int handle, bool shared = false)
	{
		this.handle = new ResourceHandle(handle, RenderGraphResourceType.Buffer, shared);
	}

	public static implicit operator GraphicsBuffer(BufferHandle buffer)
	{
		if (!buffer.IsValid())
		{
			return null;
		}
		return RenderGraphResourceRegistry.current.GetBuffer(in buffer);
	}

	public bool IsValid()
	{
		return handle.IsValid();
	}
}
