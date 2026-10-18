namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\GPUDriven\\OcclusionCullingCommonShaderVariables.cs", needAccessors = false, generateCBuffer = true)]
internal struct OcclusionCullingCommonShaderVariables
{
	[HLSLArray(8, typeof(ShaderGenUInt4))]
	public unsafe fixed uint _OccluderMipBounds[32];

	[HLSLArray(6, typeof(Matrix4x4))]
	public unsafe fixed float _ViewProjMatrix[96];

	[HLSLArray(6, typeof(Vector4))]
	public unsafe fixed float _ViewOriginWorldSpace[24];

	[HLSLArray(6, typeof(Vector4))]
	public unsafe fixed float _FacingDirWorldSpace[24];

	[HLSLArray(6, typeof(Vector4))]
	public unsafe fixed float _RadialDirWorldSpace[24];

	public Vector4 _DepthSizeInOccluderPixels;

	public Vector4 _OccluderDepthPyramidSize;

	public uint _OccluderMipLayoutSizeX;

	public uint _OccluderMipLayoutSizeY;

	public uint _OcclusionTestDebugFlags;

	public uint _OcclusionCullingCommonPad0;

	public int _OcclusionTestCount;

	public int _OccluderSubviewIndices;

	public int _CullingSplitIndices;

	public int _CullingSplitMask;

	internal unsafe OcclusionCullingCommonShaderVariables(in OccluderContext occluderCtx, in InstanceOcclusionTestSubviewSettings subviewSettings, bool occlusionOverlayCountVisible, bool overrideOcclusionTestToAlwaysPass)
	{
		for (int i = 0; i < occluderCtx.subviewCount; i++)
		{
			if (occluderCtx.IsSubviewValid(i))
			{
				for (int j = 0; j < 16; j++)
				{
					_ViewProjMatrix[16 * i + j] = occluderCtx.subviewData[i].viewProjMatrix[j];
				}
				for (int k = 0; k < 4; k++)
				{
					_ViewOriginWorldSpace[4 * i + k] = occluderCtx.subviewData[i].viewOriginWorldSpace[k];
					_FacingDirWorldSpace[4 * i + k] = occluderCtx.subviewData[i].facingDirWorldSpace[k];
					_RadialDirWorldSpace[4 * i + k] = occluderCtx.subviewData[i].radialDirWorldSpace[k];
				}
			}
		}
		_OccluderMipLayoutSizeX = (uint)occluderCtx.occluderMipLayoutSize.x;
		_OccluderMipLayoutSizeY = (uint)occluderCtx.occluderMipLayoutSize.y;
		_OcclusionTestDebugFlags = (overrideOcclusionTestToAlwaysPass ? 1u : 0u) | (occlusionOverlayCountVisible ? 2u : 0u);
		_OcclusionCullingCommonPad0 = 0u;
		_OcclusionTestCount = subviewSettings.testCount;
		_OccluderSubviewIndices = subviewSettings.occluderSubviewIndices;
		_CullingSplitIndices = subviewSettings.cullingSplitIndices;
		_CullingSplitMask = subviewSettings.cullingSplitMask;
		_DepthSizeInOccluderPixels = occluderCtx.depthBufferSizeInOccluderPixels;
		Vector2Int occluderDepthPyramidSize = occluderCtx.occluderDepthPyramidSize;
		_OccluderDepthPyramidSize = new Vector4(occluderDepthPyramidSize.x, occluderDepthPyramidSize.y, 1f / (float)occluderDepthPyramidSize.x, 1f / (float)occluderDepthPyramidSize.y);
		for (int l = 0; l < occluderCtx.occluderMipBounds.Length; l++)
		{
			OccluderMipBounds occluderMipBounds = occluderCtx.occluderMipBounds[l];
			_OccluderMipBounds[4 * l] = (uint)occluderMipBounds.offset.x;
			_OccluderMipBounds[4 * l + 1] = (uint)occluderMipBounds.offset.y;
			_OccluderMipBounds[4 * l + 2] = (uint)occluderMipBounds.size.x;
			_OccluderMipBounds[4 * l + 3] = (uint)occluderMipBounds.size.y;
		}
	}
}
