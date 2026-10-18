namespace UnityEngine.Rendering.RenderGraphModule;

internal struct RendererListResource
{
	public RendererListParams desc;

	public RendererList rendererList;

	internal RendererListResource(in RendererListParams desc)
	{
		this.desc = desc;
		rendererList = default(RendererList);
	}
}
