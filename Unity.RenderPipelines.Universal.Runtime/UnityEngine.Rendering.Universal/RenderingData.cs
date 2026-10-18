namespace UnityEngine.Rendering.Universal;

public struct RenderingData
{
	internal ContextContainer frameData;

	public CameraData cameraData;

	public LightData lightData;

	public ShadowData shadowData;

	public PostProcessingData postProcessingData;

	internal UniversalRenderingData universalRenderingData => frameData.Get<UniversalRenderingData>();

	internal ref CommandBuffer commandBuffer
	{
		get
		{
			ref CommandBuffer reference = ref frameData.Get<UniversalRenderingData>().m_CommandBuffer;
			if (reference == null)
			{
				Debug.LogError("RenderingData.commandBuffer is null. RenderGraph does not support this property. Please use the command buffer provided by the RenderGraphContext.");
			}
			return ref reference;
		}
	}

	public ref CullingResults cullResults => ref frameData.Get<UniversalRenderingData>().cullResults;

	public ref bool supportsDynamicBatching => ref frameData.Get<UniversalRenderingData>().supportsDynamicBatching;

	public ref PerObjectData perObjectData => ref frameData.Get<UniversalRenderingData>().perObjectData;

	public ref bool postProcessingEnabled => ref frameData.Get<UniversalPostProcessingData>().isEnabled;

	internal RenderingData(ContextContainer frameData)
	{
		this.frameData = frameData;
		cameraData = new CameraData(frameData);
		lightData = new LightData(frameData);
		shadowData = new ShadowData(frameData);
		postProcessingData = new PostProcessingData(frameData);
	}
}
