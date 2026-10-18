namespace UnityEngine.Rendering.Universal;

public class UniversalRenderingData : ContextItem
{
	internal CommandBuffer m_CommandBuffer;

	public CullingResults cullResults;

	public bool supportsDynamicBatching;

	public PerObjectData perObjectData;

	internal CommandBuffer commandBuffer
	{
		get
		{
			if (m_CommandBuffer == null)
			{
				Debug.LogError("UniversalRenderingData.commandBuffer is null. RenderGraph does not support this property. Please use the command buffer provided by the RenderGraphContext.");
			}
			return m_CommandBuffer;
		}
	}

	public RenderingMode renderingMode { get; internal set; }

	public LayerMask prepassLayerMask { get; internal set; }

	public LayerMask opaqueLayerMask { get; internal set; }

	public LayerMask transparentLayerMask { get; internal set; }

	public bool stencilLodCrossFadeEnabled { get; internal set; }

	public override void Reset()
	{
		m_CommandBuffer = null;
		cullResults = default(CullingResults);
		supportsDynamicBatching = false;
		perObjectData = PerObjectData.None;
		renderingMode = RenderingMode.Forward;
		stencilLodCrossFadeEnabled = false;
		prepassLayerMask = -1;
		opaqueLayerMask = -1;
		transparentLayerMask = -1;
	}
}
