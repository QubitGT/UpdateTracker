using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[CreateAssetMenu(fileName = "MeshAssignCustomizerSimpleAPIPutSliceIdxInUV0_z", menuName = "Mesh Baker/Assign To Mesh Customizer/Simple API Put Slice Index In UV0.z", order = 1)]
public class CustomizerPutSliceIndexInUV0_z : MB_DefaultMeshAssignCustomizer
{
	public override void meshAssign_UV0(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes)
	{
		if (textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.atlas)
		{
			mesh.uv = uvs;
		}
		else if (uvs.Length == sliceIndexes.Length)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < uvs.Length; i++)
			{
				list.Add(new Vector3(uvs[i].x, uvs[i].y, sliceIndexes[i]));
			}
			mesh.SetUVs(0, list);
		}
		else
		{
			Debug.LogError("UV slice buffer was not the same size as the uv buffer");
		}
	}
}
