using System;

namespace UnityEngine.ProBuilder;

public static class Normals
{
	private static Vector3[] s_SmoothAvg = new Vector3[30];

	private static float[] s_SmoothAvgCount = new float[30];

	private static int[] s_CachedIntArray = new int[65535];

	private static void ClearIntArray(int count)
	{
		if (count > s_CachedIntArray.Length)
		{
			Array.Resize(ref s_CachedIntArray, count);
		}
		for (int i = 0; i < count; i++)
		{
			s_CachedIntArray[i] = 0;
		}
	}

	public static void CalculateTangents(ProBuilderMesh mesh)
	{
		int vertexCount = mesh.vertexCount;
		if (!mesh.HasArrays(MeshArrays.Tangent))
		{
			mesh.tangentsInternal = new Vector4[vertexCount];
		}
		if (!mesh.HasArrays(MeshArrays.Position) || !mesh.HasArrays(MeshArrays.Texture0))
		{
			return;
		}
		Vector3[] normals = mesh.GetNormals();
		Vector3[] positionsInternal = mesh.positionsInternal;
		Vector2[] texturesInternal = mesh.texturesInternal;
		Vector3[] array = new Vector3[vertexCount];
		Vector3[] array2 = new Vector3[vertexCount];
		Vector4[] tangentsInternal = mesh.tangentsInternal;
		Face[] facesInternal = mesh.facesInternal;
		for (int i = 0; i < facesInternal.Length; i++)
		{
			int[] indexesInternal = facesInternal[i].indexesInternal;
			int j = 0;
			for (int num = indexesInternal.Length; j < num; j += 3)
			{
				long num2 = indexesInternal[j];
				long num3 = indexesInternal[j + 1];
				long num4 = indexesInternal[j + 2];
				Vector3 vector = positionsInternal[num2];
				Vector3 vector2 = positionsInternal[num3];
				Vector3 vector3 = positionsInternal[num4];
				Vector2 vector4 = texturesInternal[num2];
				Vector2 vector5 = texturesInternal[num3];
				Vector2 vector6 = texturesInternal[num4];
				float num5 = vector2.x - vector.x;
				float num6 = vector3.x - vector.x;
				float num7 = vector2.y - vector.y;
				float num8 = vector3.y - vector.y;
				float num9 = vector2.z - vector.z;
				float num10 = vector3.z - vector.z;
				float num11 = vector5.x - vector4.x;
				float num12 = vector6.x - vector4.x;
				float num13 = vector5.y - vector4.y;
				float num14 = vector6.y - vector4.y;
				float num15 = 1f / (num11 * num14 - num12 * num13);
				Vector3 vector7 = new Vector3((num14 * num5 - num13 * num6) * num15, (num14 * num7 - num13 * num8) * num15, (num14 * num9 - num13 * num10) * num15);
				Vector3 vector8 = new Vector3((num11 * num6 - num12 * num5) * num15, (num11 * num8 - num12 * num7) * num15, (num11 * num10 - num12 * num9) * num15);
				array[num2] += vector7;
				array[num3] += vector7;
				array[num4] += vector7;
				array2[num2] += vector8;
				array2[num3] += vector8;
				array2[num4] += vector8;
			}
		}
		for (long num16 = 0L; num16 < vertexCount; num16++)
		{
			Vector3 normal = normals[num16];
			Vector3 tangent = Math.EnsureUnitVector(array[num16]);
			Vector3.OrthoNormalize(ref normal, ref tangent);
			tangentsInternal[num16].x = tangent.x;
			tangentsInternal[num16].y = tangent.y;
			tangentsInternal[num16].z = tangent.z;
			tangentsInternal[num16].w = ((Vector3.Dot(Vector3.Cross(normal, tangent), array2[num16]) < 0f) ? (-1f) : 1f);
		}
	}

	private static void CalculateHardNormals(ProBuilderMesh mesh)
	{
		int vertexCount = mesh.vertexCount;
		Vector3[] positionsInternal = mesh.positionsInternal;
		Face[] facesInternal = mesh.facesInternal;
		ClearIntArray(vertexCount);
		if (!mesh.HasArrays(MeshArrays.Normal))
		{
			mesh.normalsInternal = new Vector3[vertexCount];
		}
		Vector3[] normalsInternal = mesh.normalsInternal;
		for (int i = 0; i < vertexCount; i++)
		{
			normalsInternal[i].x = 0f;
			normalsInternal[i].y = 0f;
			normalsInternal[i].z = 0f;
		}
		int j = 0;
		for (int num = facesInternal.Length; j < num; j++)
		{
			int[] indexesInternal = facesInternal[j].indexesInternal;
			for (int k = 0; k < indexesInternal.Length; k += 3)
			{
				int num2 = indexesInternal[k];
				int num3 = indexesInternal[k + 1];
				int num4 = indexesInternal[k + 2];
				Vector3 vector = Math.Normal(positionsInternal[num2], positionsInternal[num3], positionsInternal[num4]);
				vector.Normalize();
				normalsInternal[num2].x += vector.x;
				normalsInternal[num3].x += vector.x;
				normalsInternal[num4].x += vector.x;
				normalsInternal[num2].y += vector.y;
				normalsInternal[num3].y += vector.y;
				normalsInternal[num4].y += vector.y;
				normalsInternal[num2].z += vector.z;
				normalsInternal[num3].z += vector.z;
				normalsInternal[num4].z += vector.z;
				s_CachedIntArray[num2]++;
				s_CachedIntArray[num3]++;
				s_CachedIntArray[num4]++;
			}
		}
		for (int l = 0; l < vertexCount; l++)
		{
			normalsInternal[l].x = normalsInternal[l].x / (float)s_CachedIntArray[l];
			normalsInternal[l].y = normalsInternal[l].y / (float)s_CachedIntArray[l];
			normalsInternal[l].z = normalsInternal[l].z / (float)s_CachedIntArray[l];
		}
	}

	public static void CalculateNormals(ProBuilderMesh mesh)
	{
		CalculateHardNormals(mesh);
		SharedVertex[] sharedVerticesInternal = mesh.sharedVerticesInternal;
		Face[] facesInternal = mesh.facesInternal;
		Vector3[] normalsInternal = mesh.normalsInternal;
		int num = 24;
		ClearIntArray(mesh.vertexCount);
		int i = 0;
		for (int faceCount = mesh.faceCount; i < faceCount; i++)
		{
			Face face = facesInternal[i];
			int[] distinctIndexesInternal = face.distinctIndexesInternal;
			int j = 0;
			for (int num2 = distinctIndexesInternal.Length; j < num2; j++)
			{
				s_CachedIntArray[distinctIndexesInternal[j]] = face.smoothingGroup;
				if (face.smoothingGroup >= num)
				{
					num = face.smoothingGroup + 1;
				}
			}
		}
		if (num > s_SmoothAvg.Length)
		{
			Array.Resize(ref s_SmoothAvg, num);
			Array.Resize(ref s_SmoothAvgCount, num);
		}
		for (int k = 0; k < sharedVerticesInternal.Length; k++)
		{
			for (int l = 0; l < num; l++)
			{
				s_SmoothAvg[l].x = 0f;
				s_SmoothAvg[l].y = 0f;
				s_SmoothAvg[l].z = 0f;
				s_SmoothAvgCount[l] = 0f;
			}
			for (int m = 0; m < sharedVerticesInternal[k].Count; m++)
			{
				int num3 = sharedVerticesInternal[k][m];
				int num4 = s_CachedIntArray[num3];
				if (num4 > 0)
				{
					s_SmoothAvg[num4].x += normalsInternal[num3].x;
					s_SmoothAvg[num4].y += normalsInternal[num3].y;
					s_SmoothAvg[num4].z += normalsInternal[num3].z;
					s_SmoothAvgCount[num4] += 1f;
				}
			}
			for (int n = 0; n < sharedVerticesInternal[k].Count; n++)
			{
				int num5 = sharedVerticesInternal[k][n];
				int num6 = s_CachedIntArray[num5];
				if (num6 > 0)
				{
					normalsInternal[num5].x = s_SmoothAvg[num6].x / s_SmoothAvgCount[num6];
					normalsInternal[num5].y = s_SmoothAvg[num6].y / s_SmoothAvgCount[num6];
					normalsInternal[num5].z = s_SmoothAvg[num6].z / s_SmoothAvgCount[num6];
					normalsInternal[num5] = Math.EnsureUnitVector(normalsInternal[num5]);
				}
			}
		}
	}
}
