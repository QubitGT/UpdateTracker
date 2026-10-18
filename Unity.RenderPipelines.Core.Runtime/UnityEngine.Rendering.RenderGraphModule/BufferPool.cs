namespace UnityEngine.Rendering.RenderGraphModule;

internal class BufferPool : RenderGraphResourcePool<GraphicsBuffer>
{
	protected override void ReleaseInternalResource(GraphicsBuffer res)
	{
		res.Release();
	}

	protected override string GetResourceName(in GraphicsBuffer res)
	{
		return "GraphicsBufferNameNotAvailable";
	}

	protected override long GetResourceSize(in GraphicsBuffer res)
	{
		return res.count * res.stride;
	}

	protected override string GetResourceTypeName()
	{
		return "GraphicsBuffer";
	}

	protected override int GetSortIndex(GraphicsBuffer res)
	{
		return res.GetHashCode();
	}
}
