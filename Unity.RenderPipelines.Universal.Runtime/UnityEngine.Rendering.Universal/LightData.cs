using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

public struct LightData
{
	private ContextContainer frameData;

	internal UniversalLightData universalLightData => frameData.Get<UniversalLightData>();

	public ref int mainLightIndex => ref frameData.Get<UniversalLightData>().mainLightIndex;

	public ref int additionalLightsCount => ref frameData.Get<UniversalLightData>().additionalLightsCount;

	public ref int maxPerObjectAdditionalLightsCount => ref frameData.Get<UniversalLightData>().maxPerObjectAdditionalLightsCount;

	public ref NativeArray<VisibleLight> visibleLights => ref frameData.Get<UniversalLightData>().visibleLights;

	public ref bool shadeAdditionalLightsPerVertex => ref frameData.Get<UniversalLightData>().shadeAdditionalLightsPerVertex;

	public ref bool supportsMixedLighting => ref frameData.Get<UniversalLightData>().supportsMixedLighting;

	public ref bool reflectionProbeBoxProjection => ref frameData.Get<UniversalLightData>().reflectionProbeBoxProjection;

	public ref bool reflectionProbeBlending => ref frameData.Get<UniversalLightData>().reflectionProbeBlending;

	public ref bool reflectionProbeAtlas => ref frameData.Get<UniversalLightData>().reflectionProbeAtlas;

	public ref bool supportsLightLayers => ref frameData.Get<UniversalLightData>().supportsLightLayers;

	public ref bool supportsAdditionalLights => ref frameData.Get<UniversalLightData>().supportsAdditionalLights;

	internal LightData(ContextContainer frameData)
	{
		this.frameData = frameData;
	}
}
