using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

public class UniversalShadowData : ContextItem
{
	public bool supportsMainLightShadows;

	internal bool mainLightShadowsEnabled;

	public int mainLightShadowmapWidth;

	public int mainLightShadowmapHeight;

	public int mainLightShadowCascadesCount;

	public Vector3 mainLightShadowCascadesSplit;

	public float mainLightShadowCascadeBorder;

	public bool supportsAdditionalLightShadows;

	internal bool additionalLightShadowsEnabled;

	public int additionalLightsShadowmapWidth;

	public int additionalLightsShadowmapHeight;

	public bool supportsSoftShadows;

	public int shadowmapDepthBufferBits;

	public List<Vector4> bias;

	public List<int> resolution;

	internal bool isKeywordAdditionalLightShadowsEnabled;

	internal bool isKeywordSoftShadowsEnabled;

	internal int mainLightShadowResolution;

	internal int mainLightRenderTargetWidth;

	internal int mainLightRenderTargetHeight;

	internal NativeArray<URPLightShadowCullingInfos> visibleLightsShadowCullingInfos;

	internal AdditionalLightsShadowAtlasLayout shadowAtlasLayout;

	public override void Reset()
	{
		supportsMainLightShadows = false;
		mainLightShadowmapWidth = 0;
		mainLightShadowmapHeight = 0;
		mainLightShadowCascadesCount = 0;
		mainLightShadowCascadesSplit = Vector3.zero;
		mainLightShadowCascadeBorder = 0f;
		supportsAdditionalLightShadows = false;
		additionalLightsShadowmapWidth = 0;
		additionalLightsShadowmapHeight = 0;
		supportsSoftShadows = false;
		shadowmapDepthBufferBits = 0;
		bias?.Clear();
		resolution?.Clear();
		isKeywordAdditionalLightShadowsEnabled = false;
		isKeywordSoftShadowsEnabled = false;
		mainLightShadowResolution = 0;
		mainLightRenderTargetWidth = 0;
		mainLightRenderTargetHeight = 0;
		visibleLightsShadowCullingInfos = default(NativeArray<URPLightShadowCullingInfos>);
		shadowAtlasLayout = default(AdditionalLightsShadowAtlasLayout);
	}
}
