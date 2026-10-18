using System.Diagnostics;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("RayTracingAccelerationStructureResource ({desc.name})")]
internal class RayTracingAccelerationStructureResource : RenderGraphResource<RayTracingAccelerationStructureDesc, RayTracingAccelerationStructure>
{
	public override string GetName()
	{
		return desc.name;
	}
}
