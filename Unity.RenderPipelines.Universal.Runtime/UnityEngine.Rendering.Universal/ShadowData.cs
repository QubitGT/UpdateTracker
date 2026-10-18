using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

public struct ShadowData
{
	private ContextContainer frameData;

	internal UniversalShadowData universalShadowData => frameData.Get<UniversalShadowData>();

	public ref bool supportsMainLightShadows => ref frameData.Get<UniversalShadowData>().supportsMainLightShadows;

	internal ref bool mainLightShadowsEnabled => ref frameData.Get<UniversalShadowData>().mainLightShadowsEnabled;

	public ref int mainLightShadowmapWidth => ref frameData.Get<UniversalShadowData>().mainLightShadowmapWidth;

	public ref int mainLightShadowmapHeight => ref frameData.Get<UniversalShadowData>().mainLightShadowmapHeight;

	public ref int mainLightShadowCascadesCount => ref frameData.Get<UniversalShadowData>().mainLightShadowCascadesCount;

	public ref Vector3 mainLightShadowCascadesSplit => ref frameData.Get<UniversalShadowData>().mainLightShadowCascadesSplit;

	public ref float mainLightShadowCascadeBorder => ref frameData.Get<UniversalShadowData>().mainLightShadowCascadeBorder;

	public ref bool supportsAdditionalLightShadows => ref frameData.Get<UniversalShadowData>().supportsAdditionalLightShadows;

	internal ref bool additionalLightShadowsEnabled => ref frameData.Get<UniversalShadowData>().additionalLightShadowsEnabled;

	public ref int additionalLightsShadowmapWidth => ref frameData.Get<UniversalShadowData>().additionalLightsShadowmapWidth;

	public ref int additionalLightsShadowmapHeight => ref frameData.Get<UniversalShadowData>().additionalLightsShadowmapHeight;

	public ref bool supportsSoftShadows => ref frameData.Get<UniversalShadowData>().supportsSoftShadows;

	public ref int shadowmapDepthBufferBits => ref frameData.Get<UniversalShadowData>().shadowmapDepthBufferBits;

	public ref List<Vector4> bias => ref frameData.Get<UniversalShadowData>().bias;

	public ref List<int> resolution => ref frameData.Get<UniversalShadowData>().resolution;

	internal ref bool isKeywordAdditionalLightShadowsEnabled => ref frameData.Get<UniversalShadowData>().isKeywordAdditionalLightShadowsEnabled;

	internal ref bool isKeywordSoftShadowsEnabled => ref frameData.Get<UniversalShadowData>().isKeywordSoftShadowsEnabled;

	internal ref int mainLightShadowResolution => ref frameData.Get<UniversalShadowData>().mainLightShadowResolution;

	internal ref int mainLightRenderTargetWidth => ref frameData.Get<UniversalShadowData>().mainLightRenderTargetWidth;

	internal ref int mainLightRenderTargetHeight => ref frameData.Get<UniversalShadowData>().mainLightRenderTargetHeight;

	internal ref NativeArray<URPLightShadowCullingInfos> visibleLightsShadowCullingInfos => ref frameData.Get<UniversalShadowData>().visibleLightsShadowCullingInfos;

	internal ref AdditionalLightsShadowAtlasLayout shadowAtlasLayout => ref frameData.Get<UniversalShadowData>().shadowAtlasLayout;

	internal ShadowData(ContextContainer frameData)
	{
		this.frameData = frameData;
	}
}
