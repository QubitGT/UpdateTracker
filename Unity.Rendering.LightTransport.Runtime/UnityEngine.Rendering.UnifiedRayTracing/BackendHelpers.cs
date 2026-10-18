using System;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal static class BackendHelpers
{
	internal static string GetFileNameOfShader(RayTracingBackend backend, string fileName)
	{
		return fileName + "." + backend switch
		{
			RayTracingBackend.Hardware => "raytrace", 
			RayTracingBackend.Compute => "compute", 
			_ => throw new ArgumentOutOfRangeException("backend", backend, null), 
		};
	}

	internal static Type GetTypeOfShader(RayTracingBackend backend)
	{
		return backend switch
		{
			RayTracingBackend.Hardware => typeof(RayTracingShader), 
			RayTracingBackend.Compute => typeof(ComputeShader), 
			_ => throw new ArgumentOutOfRangeException("backend", backend, null), 
		};
	}
}
