using System.Diagnostics;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("RayTracingAccelerationStructure ({handle.index})")]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct RayTracingAccelerationStructureHandle
{
	private static RayTracingAccelerationStructureHandle s_NullHandle;

	internal ResourceHandle handle;

	public static RayTracingAccelerationStructureHandle nullHandle => s_NullHandle;

	internal RayTracingAccelerationStructureHandle(int handle)
	{
		this.handle = new ResourceHandle(handle, RenderGraphResourceType.AccelerationStructure, shared: false);
	}

	public static implicit operator RayTracingAccelerationStructure(RayTracingAccelerationStructureHandle handle)
	{
		if (!handle.IsValid())
		{
			return null;
		}
		return RenderGraphResourceRegistry.current.GetRayTracingAccelerationStructure(in handle);
	}

	public bool IsValid()
	{
		return handle.IsValid();
	}
}
