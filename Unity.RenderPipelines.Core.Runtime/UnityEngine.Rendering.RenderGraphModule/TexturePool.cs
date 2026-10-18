using UnityEngine.Profiling;

namespace UnityEngine.Rendering.RenderGraphModule;

internal class TexturePool : RenderGraphResourcePool<RTHandle>
{
	protected override void ReleaseInternalResource(RTHandle res)
	{
		res.Release();
	}

	protected override string GetResourceName(in RTHandle res)
	{
		return res.rt.name;
	}

	protected override long GetResourceSize(in RTHandle res)
	{
		return Profiler.GetRuntimeMemorySizeLong(res.rt);
	}

	protected override string GetResourceTypeName()
	{
		return "Texture";
	}

	protected override int GetSortIndex(RTHandle res)
	{
		return res.GetInstanceID();
	}
}
