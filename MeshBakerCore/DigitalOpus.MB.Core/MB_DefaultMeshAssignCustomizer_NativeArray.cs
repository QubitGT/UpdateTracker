using Unity.Collections;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB_DefaultMeshAssignCustomizer_NativeArray : ScriptableObject, IAssignToMeshCustomizer_NativeArrays, IAssignToMeshCustomizer
{
	public virtual int UVchannelWithExtraParameter()
	{
		return -1;
	}

	public virtual void meshAssign_UV(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Vector3> outUVsInMesh, NativeSlice<float> sliceIndexes)
	{
	}

	public virtual void meshAssign_colors(MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Color> outUVsInMesh, NativeSlice<float> sliceIndexes)
	{
	}
}
