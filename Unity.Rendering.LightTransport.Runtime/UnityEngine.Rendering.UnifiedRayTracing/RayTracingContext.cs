using System;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal sealed class RayTracingContext : IDisposable
{
	public RayTracingResources Resources;

	private readonly IRayTracingBackend m_Backend;

	private readonly ReferenceCounter m_AccelStructCounter = new ReferenceCounter();

	private readonly GraphicsBuffer m_DispatchBuffer;

	public RayTracingBackend BackendType { get; private set; }

	public RayTracingContext(RayTracingBackend backend, RayTracingResources resources)
	{
		if (!IsBackendSupported(backend))
		{
			throw new InvalidOperationException("Unsupported backend: " + backend);
		}
		BackendType = backend;
		switch (backend)
		{
		case RayTracingBackend.Hardware:
			m_Backend = new HardwareRayTracingBackend(resources);
			break;
		case RayTracingBackend.Compute:
			m_Backend = new ComputeRayTracingBackend(resources);
			break;
		}
		Resources = resources;
		m_DispatchBuffer = RayTracingHelper.CreateDispatchDimensionBuffer();
	}

	public void Dispose()
	{
		if (m_AccelStructCounter.value != 0L)
		{
			Debug.LogError("Memory Leak. Please call .Dispose() on all the IAccelerationStructure resources that have been created with this context before calling RayTracingContext.Dispose()");
		}
		m_DispatchBuffer?.Release();
	}

	public static bool IsBackendSupported(RayTracingBackend backend)
	{
		return backend switch
		{
			RayTracingBackend.Hardware => SystemInfo.supportsRayTracing, 
			RayTracingBackend.Compute => SystemInfo.supportsComputeShaders, 
			_ => false, 
		};
	}

	public IRayTracingShader CreateRayTracingShader(Object shader)
	{
		return m_Backend.CreateRayTracingShader(shader, "MainRayGenShader", m_DispatchBuffer);
	}

	public static uint GetScratchBufferStrideInBytes()
	{
		return 4u;
	}

	public IRayTracingShader CreateRayTracingShader(RayTracingShader rtShader)
	{
		return m_Backend.CreateRayTracingShader(rtShader, "MainRayGenShader", m_DispatchBuffer);
	}

	public IRayTracingShader CreateRayTracingShader(ComputeShader computeShader)
	{
		return m_Backend.CreateRayTracingShader(computeShader, "MainRayGenShader", m_DispatchBuffer);
	}

	public IRayTracingAccelStruct CreateAccelerationStructure(AccelerationStructureOptions options)
	{
		return m_Backend.CreateAccelerationStructure(options, m_AccelStructCounter);
	}

	public ulong GetRequiredTraceScratchBufferSizeInBytes(uint width, uint height, uint depth)
	{
		return m_Backend.GetRequiredTraceScratchBufferSizeInBytes(width, height, depth);
	}
}
