using UnityEngine.Rendering.RadeonRays;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal class ComputeRayTracingBackend : IRayTracingBackend
{
	private readonly RayTracingResources m_Resources;

	public ComputeRayTracingBackend(RayTracingResources resources)
	{
		m_Resources = resources;
	}

	public IRayTracingShader CreateRayTracingShader(Object shader, string kernelName, GraphicsBuffer dispatchBuffer)
	{
		return new ComputeRayTracingShader((ComputeShader)shader, kernelName, dispatchBuffer);
	}

	public IRayTracingAccelStruct CreateAccelerationStructure(AccelerationStructureOptions options, ReferenceCounter counter)
	{
		return new ComputeRayTracingAccelStruct(options, m_Resources, counter);
	}

	public ulong GetRequiredTraceScratchBufferSizeInBytes(uint width, uint height, uint depth)
	{
		return RadeonRaysAPI.GetTraceMemoryRequirements(width * height * depth) * RayTracingContext.GetScratchBufferStrideInBytes();
	}
}
