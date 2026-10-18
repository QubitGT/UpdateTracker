using Unity.Collections;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public interface IAssignToMeshCustomizer_NativeArrays : IAssignToMeshCustomizer
{
	int UVchannelWithExtraParameter();

	void meshAssign_UV(int channel_0_to_7, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Vector3> outUVsInMesh, NativeSlice<float> sliceIndexes);

	void meshAssign_colors(MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Color> outUVsInMesh, NativeSlice<float> sliceIndexes);
}
