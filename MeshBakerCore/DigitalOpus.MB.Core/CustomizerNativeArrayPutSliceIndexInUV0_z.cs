using Unity.Collections;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[CreateAssetMenu(fileName = "MeshAssignCustomizerNativeArrayPutSliceIdxInUV0_z", menuName = "Mesh Baker/Assign To Mesh Customizer/NativeArray API Put Slice Index In UV0.z", order = 1)]
public class CustomizerNativeArrayPutSliceIndexInUV0_z : MB_DefaultMeshAssignCustomizer_NativeArray
{
	public override int UVchannelWithExtraParameter()
	{
		return 0;
	}

	public override void meshAssign_UV(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Vector3> outUVsInMesh, NativeSlice<float> sliceIndexes)
	{
		if (textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.atlas)
		{
			return;
		}
		if (outUVsInMesh.Length == sliceIndexes.Length)
		{
			for (int i = 0; i < outUVsInMesh.Length; i++)
			{
				outUVsInMesh[i] = new Vector3(outUVsInMesh[i].x, outUVsInMesh[i].y, sliceIndexes[i]);
			}
		}
		else
		{
			Debug.LogError("UV slice buffer was not the same size as the uv buffer");
		}
	}
}
