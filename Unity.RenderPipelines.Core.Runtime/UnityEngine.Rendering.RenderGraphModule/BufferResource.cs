using System.Diagnostics;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("BufferResource ({desc.name})")]
internal class BufferResource : RenderGraphResource<BufferDesc, GraphicsBuffer>
{
	public override string GetName()
	{
		if (imported)
		{
			return "ImportedGraphicsBuffer";
		}
		return desc.name;
	}

	public override int GetDescHashCode()
	{
		return desc.GetHashCode();
	}

	public override void CreateGraphicsResource()
	{
		GetName();
		graphicsResource = new GraphicsBuffer(desc.target, desc.usageFlags, desc.count, desc.stride);
	}

	public override void UpdateGraphicsResource()
	{
		if (graphicsResource != null)
		{
			graphicsResource.name = GetName();
		}
	}

	public override void ReleaseGraphicsResource()
	{
		if (graphicsResource != null)
		{
			graphicsResource.Release();
		}
		base.ReleaseGraphicsResource();
	}

	public override void LogCreation(RenderGraphLogger logger)
	{
		logger.LogLine("Created GraphicsBuffer: " + desc.name);
	}

	public override void LogRelease(RenderGraphLogger logger)
	{
		logger.LogLine("Released GraphicsBuffer: " + desc.name);
	}
}
