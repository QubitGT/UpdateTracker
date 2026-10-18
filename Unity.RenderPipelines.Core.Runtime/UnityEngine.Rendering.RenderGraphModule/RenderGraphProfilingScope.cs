using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public struct RenderGraphProfilingScope : IDisposable
{
	public RenderGraphProfilingScope(RenderGraph renderGraph, ProfilingSampler sampler)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
	}
}
