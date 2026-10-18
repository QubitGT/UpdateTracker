using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_CopyBoneWeights
{
	public static void CopyBoneWeightsFromSeamMeshToOtherMeshes(float radius, Mesh seamMesh, Mesh[] targetMeshes, Transform[][] newBonesForSMRs, Transform[] seamMeshBones, Transform[][] targMeshBones)
	{
		List<int> list = new List<int>();
		if (seamMesh == null)
		{
			Debug.LogError($"The SeamMesh cannot be null");
			return;
		}
		if (seamMesh.vertexCount == 0)
		{
			Debug.LogError("The seam mesh has no vertices. Check that the Asset Importer for the seam mesh does not have 'Optimize Mesh' checked.");
			return;
		}
		Vector3[] vertices = seamMesh.vertices;
		BoneWeight[] boneWeights = seamMesh.boneWeights;
		Vector3[] normals = seamMesh.normals;
		Vector4[] tangents = seamMesh.tangents;
		Vector2[] uv = seamMesh.uv;
		if (uv.Length != vertices.Length)
		{
			Debug.LogError("The seam mesh needs uvs to identify which vertices are part of the seam. Vertices with UV > .5 are part of the seam. Vertices with UV < .5 are not part of the seam.");
			return;
		}
		for (int i = 0; i < uv.Length; i++)
		{
			if (uv[i].x > 0.5f && uv[i].y > 0.5f)
			{
				list.Add(i);
			}
		}
		Debug.Log($"The seam mesh has {seamMesh.vertices.Length} vertices of which {list.Count} are seam vertices.");
		if (list.Count == 0)
		{
			Debug.LogError("None of the vertices in the Seam Mesh were marked as seam vertices. To mark a vertex as a seam vertex the UV must be greater than (.5,.5). Vertices with UV less than (.5,.5) are excluded.");
			return;
		}
		bool flag = false;
		if (radius <= 0f)
		{
			Debug.LogError("radius must be greater than zero.");
		}
		for (int j = 0; j < targetMeshes.Length; j++)
		{
			if (targetMeshes[j] == null)
			{
				Debug.LogError($"Mesh {j} was null");
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		Dictionary<Transform, int> dictionary = new Dictionary<Transform, int>();
		int[] array = new int[seamMeshBones.Length];
		for (int k = 0; k < seamMeshBones.Length; k++)
		{
			dictionary.Add(seamMeshBones[k], k);
		}
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		for (int l = 0; l < targetMeshes.Length; l++)
		{
			Mesh mesh = targetMeshes[l];
			if (mesh == seamMesh)
			{
				continue;
			}
			Vector3[] vertices2 = mesh.vertices;
			BoneWeight[] boneWeights2 = mesh.boneWeights;
			Vector3[] normals2 = mesh.normals;
			Vector4[] tangents2 = mesh.tangents;
			dictionary2.Clear();
			for (int m = 0; m < array.Length; m++)
			{
				array[m] = -1;
			}
			Transform[] array2 = targMeshBones[l];
			for (int n = 0; n < array2.Length; n++)
			{
				Transform key = array2[n];
				if (dictionary.ContainsKey(key))
				{
					int num = dictionary[key];
					array[num] = n;
				}
			}
			int num2 = 0;
			for (int num3 = 0; num3 < vertices2.Length; num3++)
			{
				for (int num4 = 0; num4 < list.Count; num4++)
				{
					int num5 = list[num4];
					if (Vector3.Distance(vertices2[num3], vertices[num5]) <= radius)
					{
						if (seamMesh == targetMeshes[l] && num3 != num5)
						{
							Debug.LogError("Same mesh but different verts overlapped. radius too big " + num3 + "  " + num5);
						}
						num2++;
						BoneWeight seamMeshBw = boneWeights[num5];
						RemapBoneWeightIndexes(targetMeshes[l].name, ref seamMeshBw, array, targMeshBones[l], dictionary2, seamMeshBones);
						boneWeights2[num3] = seamMeshBw;
						vertices2[num3] = vertices[num5];
						if (normals2.Length == vertices2.Length && normals.Length == normals.Length)
						{
							normals2[num3] = normals[num5];
						}
						if (tangents2.Length == vertices2.Length && tangents.Length == vertices.Length)
						{
							tangents2[num3] = tangents[num5];
						}
					}
				}
			}
			if (dictionary2.Count > 0)
			{
				int num6 = targMeshBones[l].Length + dictionary2.Count;
				Transform[] array3 = new Transform[num6];
				Matrix4x4[] array4 = new Matrix4x4[num6];
				Matrix4x4[] bindposes = targetMeshes[l].bindposes;
				for (int num7 = 0; num7 < targMeshBones[l].Length; num7++)
				{
					array4[num7] = bindposes[num7];
					array3[num7] = targMeshBones[l][num7];
				}
				Matrix4x4[] bindposes2 = seamMesh.bindposes;
				foreach (KeyValuePair<int, int> item in dictionary2)
				{
					array3[item.Value] = seamMeshBones[item.Key];
					array4[item.Value] = bindposes2[item.Key];
				}
				for (int num8 = 0; num8 < dictionary2.Count; num8++)
				{
					if (array3[num8] == null)
					{
						Debug.LogError("Should never happend. Not all target indexes were covered.");
					}
				}
				newBonesForSMRs[l] = array3;
				targetMeshes[l].bindposes = array4;
			}
			if (num2 > 0)
			{
				targetMeshes[l].vertices = vertices2;
				targetMeshes[l].boneWeights = boneWeights2;
				targetMeshes[l].normals = normals2;
				targetMeshes[l].tangents = tangents2;
			}
			Debug.Log(string.Format("Copied boneweights for {1} vertices in mesh {0} that matched positions in the seam mesh.", targetMeshes[l].name, num2));
		}
	}

	private static void RemapBoneWeightIndexes(string nm, ref BoneWeight seamMeshBw, int[] map_seamMeshIdx2targMeshIdx, Transform[] targBones, Dictionary<int, int> extraBones, Transform[] seamBones)
	{
		int num = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex0];
		int num2 = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex1];
		int num3 = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex2];
		int num4 = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex3];
		if (num == -1)
		{
			num = targBones.Length + extraBones.Count;
			extraBones.Add(seamMeshBw.boneIndex0, num);
			map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex0] = num;
		}
		if (num2 == -1)
		{
			num2 = targBones.Length + extraBones.Count;
			extraBones.Add(seamMeshBw.boneIndex1, num2);
			map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex1] = num2;
		}
		if (num3 == -1)
		{
			num3 = targBones.Length + extraBones.Count;
			extraBones.Add(seamMeshBw.boneIndex2, num3);
			map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex2] = num3;
		}
		if (num4 == -1)
		{
			num4 = targBones.Length + extraBones.Count;
			extraBones.Add(seamMeshBw.boneIndex3, num4);
			map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex3] = num4;
		}
		seamMeshBw.boneIndex0 = num;
		seamMeshBw.boneIndex1 = num2;
		seamMeshBw.boneIndex2 = num3;
		seamMeshBw.boneIndex3 = num4;
	}
}
