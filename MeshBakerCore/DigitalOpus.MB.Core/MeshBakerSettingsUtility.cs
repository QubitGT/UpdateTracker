namespace DigitalOpus.MB.Core;

public static class MeshBakerSettingsUtility
{
	public static MB_MeshVertexChannelFlags GetMeshChannelsAsFlags(MB_IMeshBakerSettings settings, bool doVerts, bool uvsSliceIdx_w)
	{
		return (doVerts ? MB_MeshVertexChannelFlags.vertex : MB_MeshVertexChannelFlags.none) | (settings.doNorm ? MB_MeshVertexChannelFlags.normal : MB_MeshVertexChannelFlags.none) | (settings.doTan ? MB_MeshVertexChannelFlags.tangent : MB_MeshVertexChannelFlags.none) | (settings.doCol ? MB_MeshVertexChannelFlags.colors : MB_MeshVertexChannelFlags.none) | (settings.doUV ? MB_MeshVertexChannelFlags.uv0 : MB_MeshVertexChannelFlags.none) | (uvsSliceIdx_w ? MB_MeshVertexChannelFlags.nuvsSliceIdx : MB_MeshVertexChannelFlags.none) | (DoUV2getDataFromSourceMeshes(ref settings) ? MB_MeshVertexChannelFlags.uv2 : MB_MeshVertexChannelFlags.none) | (settings.doUV3 ? MB_MeshVertexChannelFlags.uv3 : MB_MeshVertexChannelFlags.none) | (settings.doUV4 ? MB_MeshVertexChannelFlags.uv4 : MB_MeshVertexChannelFlags.none) | (settings.doUV5 ? MB_MeshVertexChannelFlags.uv5 : MB_MeshVertexChannelFlags.none) | (settings.doUV6 ? MB_MeshVertexChannelFlags.uv6 : MB_MeshVertexChannelFlags.none) | (settings.doUV7 ? MB_MeshVertexChannelFlags.uv7 : MB_MeshVertexChannelFlags.none) | (settings.doUV8 ? MB_MeshVertexChannelFlags.uv8 : MB_MeshVertexChannelFlags.none) | ((settings.renderType == MB_RenderType.skinnedMeshRenderer) ? MB_MeshVertexChannelFlags.blendWeight : MB_MeshVertexChannelFlags.none) | ((settings.renderType == MB_RenderType.skinnedMeshRenderer) ? MB_MeshVertexChannelFlags.blendIndices : MB_MeshVertexChannelFlags.none);
	}

	public static bool DoUV2getDataFromSourceMeshes(ref MB_IMeshBakerSettings settings)
	{
		if (settings.lightmapOption != MB2_LightmapOptions.copy_UV2_unchanged && settings.lightmapOption != 0)
		{
			return settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects;
		}
		return true;
	}
}
