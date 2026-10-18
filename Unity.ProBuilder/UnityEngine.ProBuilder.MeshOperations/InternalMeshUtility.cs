using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

internal static class InternalMeshUtility
{
	internal static Vector3 AverageNormalWithIndexes(SharedVertex shared, int[] all, IList<Vector3> norm)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = 0; i < all.Length; i++)
		{
			if (shared.Contains(all[i]))
			{
				zero += norm[all[i]];
				num++;
			}
		}
		return zero / num;
	}

	public static ProBuilderMesh CreateMeshWithTransform(Transform t, bool preserveFaces)
	{
		Mesh sharedMesh = t.GetComponent<MeshFilter>().sharedMesh;
		Vector3[] meshChannel = MeshUtility.GetMeshChannel(t.gameObject, (Mesh x) => x.vertices);
		Color[] meshChannel2 = MeshUtility.GetMeshChannel(t.gameObject, (Mesh x) => x.colors);
		Vector2[] meshChannel3 = MeshUtility.GetMeshChannel(t.gameObject, (Mesh x) => x.uv);
		List<Vector3> list = (preserveFaces ? new List<Vector3>(sharedMesh.vertices) : new List<Vector3>());
		List<Color> list2 = (preserveFaces ? new List<Color>(sharedMesh.colors) : new List<Color>());
		List<Vector2> list3 = (preserveFaces ? new List<Vector2>(sharedMesh.uv) : new List<Vector2>());
		List<Face> list4 = new List<Face>();
		for (int i = 0; i < sharedMesh.subMeshCount; i++)
		{
			int[] triangles = sharedMesh.GetTriangles(i);
			for (int j = 0; j < triangles.Length; j += 3)
			{
				int num = -1;
				if (preserveFaces)
				{
					for (int k = 0; k < list4.Count; k++)
					{
						if (list4[k].distinctIndexesInternal.Contains(triangles[j]) || list4[k].distinctIndexesInternal.Contains(triangles[j + 1]) || list4[k].distinctIndexesInternal.Contains(triangles[j + 2]))
						{
							num = k;
							break;
						}
					}
				}
				if (num > -1 && preserveFaces)
				{
					int num2 = list4[num].indexesInternal.Length;
					int[] array = new int[num2 + 3];
					Array.Copy(list4[num].indexesInternal, 0, array, 0, num2);
					array[num2] = triangles[j];
					array[num2 + 1] = triangles[j + 1];
					array[num2 + 2] = triangles[j + 2];
					list4[num].indexesInternal = array;
					continue;
				}
				int[] triangles2;
				if (preserveFaces)
				{
					triangles2 = new int[3]
					{
						triangles[j],
						triangles[j + 1],
						triangles[j + 2]
					};
				}
				else
				{
					list.Add(meshChannel[triangles[j]]);
					list.Add(meshChannel[triangles[j + 1]]);
					list.Add(meshChannel[triangles[j + 2]]);
					list2.Add((meshChannel2 != null) ? meshChannel2[triangles[j]] : Color.white);
					list2.Add((meshChannel2 != null) ? meshChannel2[triangles[j + 1]] : Color.white);
					list2.Add((meshChannel2 != null) ? meshChannel2[triangles[j + 2]] : Color.white);
					list3.Add(meshChannel3[triangles[j]]);
					list3.Add(meshChannel3[triangles[j + 1]]);
					list3.Add(meshChannel3[triangles[j + 2]]);
					triangles2 = new int[3]
					{
						j,
						j + 1,
						j + 2
					};
				}
				list4.Add(new Face(triangles2, i, AutoUnwrapSettings.tile, 0, -1, -1, manualUVs: true));
			}
		}
		GameObject gameObject = Object.Instantiate(t.gameObject);
		gameObject.GetComponent<MeshFilter>().sharedMesh = null;
		ProBuilderMesh proBuilderMesh = gameObject.AddComponent<ProBuilderMesh>();
		proBuilderMesh.RebuildWithPositionsAndFaces(list.ToArray(), list4.ToArray());
		proBuilderMesh.colorsInternal = list2.ToArray();
		proBuilderMesh.textures = list3;
		proBuilderMesh.gameObject.name = t.name;
		gameObject.transform.position = t.position;
		gameObject.transform.localRotation = t.localRotation;
		gameObject.transform.localScale = t.localScale;
		proBuilderMesh.CenterPivot(null);
		return proBuilderMesh;
	}

	public static bool ResetPbObjectWithMeshFilter(ProBuilderMesh pb, bool preserveFaces)
	{
		MeshFilter component = pb.gameObject.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			Log.Error(pb.name + " does not have a mesh or Mesh Filter component.");
			return false;
		}
		Mesh sharedMesh = component.sharedMesh;
		int vertexCount = sharedMesh.vertexCount;
		Vector3[] meshChannel = MeshUtility.GetMeshChannel(pb.gameObject, (Mesh x) => x.vertices);
		Color[] meshChannel2 = MeshUtility.GetMeshChannel(pb.gameObject, (Mesh x) => x.colors);
		Vector2[] meshChannel3 = MeshUtility.GetMeshChannel(pb.gameObject, (Mesh x) => x.uv);
		List<Vector3> list = (preserveFaces ? new List<Vector3>(sharedMesh.vertices) : new List<Vector3>());
		List<Color> list2 = (preserveFaces ? new List<Color>(sharedMesh.colors) : new List<Color>());
		List<Vector2> list3 = (preserveFaces ? new List<Vector2>(sharedMesh.uv) : new List<Vector2>());
		List<Face> list4 = new List<Face>();
		MeshRenderer meshRenderer = pb.gameObject.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = pb.gameObject.AddComponent<MeshRenderer>();
		}
		int num = meshRenderer.sharedMaterials.Length;
		for (int i = 0; i < sharedMesh.subMeshCount; i++)
		{
			int[] triangles = sharedMesh.GetTriangles(i);
			for (int j = 0; j < triangles.Length; j += 3)
			{
				int num2 = -1;
				if (preserveFaces)
				{
					for (int k = 0; k < list4.Count; k++)
					{
						if (list4[k].distinctIndexesInternal.Contains(triangles[j]) || list4[k].distinctIndexesInternal.Contains(triangles[j + 1]) || list4[k].distinctIndexesInternal.Contains(triangles[j + 2]))
						{
							num2 = k;
							break;
						}
					}
				}
				if (num2 > -1 && preserveFaces)
				{
					int num3 = list4[num2].indexesInternal.Length;
					int[] array = new int[num3 + 3];
					Array.Copy(list4[num2].indexesInternal, 0, array, 0, num3);
					array[num3] = triangles[j];
					array[num3 + 1] = triangles[j + 1];
					array[num3 + 2] = triangles[j + 2];
					list4[num2].indexesInternal = array;
					continue;
				}
				int[] triangles2;
				if (preserveFaces)
				{
					triangles2 = new int[3]
					{
						triangles[j],
						triangles[j + 1],
						triangles[j + 2]
					};
				}
				else
				{
					list.Add(meshChannel[triangles[j]]);
					list.Add(meshChannel[triangles[j + 1]]);
					list.Add(meshChannel[triangles[j + 2]]);
					list2.Add((meshChannel2 != null && meshChannel2.Length == vertexCount) ? meshChannel2[triangles[j]] : Color.white);
					list2.Add((meshChannel2 != null && meshChannel2.Length == vertexCount) ? meshChannel2[triangles[j + 1]] : Color.white);
					list2.Add((meshChannel2 != null && meshChannel2.Length == vertexCount) ? meshChannel2[triangles[j + 2]] : Color.white);
					list3.Add(meshChannel3[triangles[j]]);
					list3.Add(meshChannel3[triangles[j + 1]]);
					list3.Add(meshChannel3[triangles[j + 2]]);
					triangles2 = new int[3]
					{
						j,
						j + 1,
						j + 2
					};
				}
				list4.Add(new Face(triangles2, Math.Clamp(i, 0, num - 1), AutoUnwrapSettings.tile, 0, -1, -1, manualUVs: true));
			}
		}
		pb.positionsInternal = list.ToArray();
		pb.texturesInternal = list3.ToArray();
		pb.facesInternal = list4.ToArray();
		pb.sharedVerticesInternal = SharedVertex.GetSharedVerticesWithPositions(list.ToArray());
		pb.colorsInternal = list2.ToArray();
		return true;
	}

	internal static void FilterUnusedSubmeshIndexes(ProBuilderMesh mesh)
	{
		Material[] sharedMaterials = mesh.renderer.sharedMaterials;
		int num = sharedMaterials.Length;
		bool[] array = new bool[num];
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			array[Math.Clamp(face.submeshIndex, 0, num - 1)] = true;
		}
		IEnumerable<int> enumerable = array.AllIndexesOf((bool x) => !x);
		if (!enumerable.Any())
		{
			return;
		}
		facesInternal = mesh.facesInternal;
		foreach (Face face2 in facesInternal)
		{
			int submeshIndex = face2.submeshIndex;
			foreach (int item in enumerable)
			{
				if (submeshIndex > item)
				{
					face2.submeshIndex--;
				}
			}
		}
		mesh.renderer.sharedMaterials = sharedMaterials.RemoveAt(enumerable);
	}
}
