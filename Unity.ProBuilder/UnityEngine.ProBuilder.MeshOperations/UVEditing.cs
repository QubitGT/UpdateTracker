using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

internal static class UVEditing
{
	public static bool AutoStitch(ProBuilderMesh mesh, Face f1, Face f2, int channel)
	{
		WingedEdge wingedEdge = WingedEdge.GetWingedEdges(mesh, new Face[2] { f1, f2 }).FirstOrDefault((WingedEdge x) => x.face == f1 && x.opposite != null && x.opposite.face == f2);
		if (wingedEdge == null)
		{
			return false;
		}
		if (f1.manualUV)
		{
			f2.manualUV = true;
		}
		f1.textureGroup = -1;
		f2.textureGroup = -1;
		Projection.PlanarProject(mesh, f2);
		if (AlignEdges(mesh, f2, wingedEdge.edge.local, wingedEdge.opposite.edge.local, channel))
		{
			if (!f2.manualUV)
			{
				UvUnwrapping.SetAutoAndAlignUnwrapParamsToUVs(mesh, new Face[1] { f2 });
			}
			return true;
		}
		return false;
	}

	private static bool AlignEdges(ProBuilderMesh mesh, Face faceToMove, Edge edgeToAlignTo, Edge edgeToBeAligned, int channel)
	{
		Vector2[] uVs = GetUVs(mesh, channel);
		SharedVertex[] sharedVerticesInternal = mesh.sharedVerticesInternal;
		int[] array = new int[2] { edgeToAlignTo.a, -1 };
		int[] array2 = new int[2] { edgeToAlignTo.b, -1 };
		int sharedVertexHandle = mesh.GetSharedVertexHandle(edgeToAlignTo.a);
		if (sharedVertexHandle < 0)
		{
			return false;
		}
		if (sharedVerticesInternal[sharedVertexHandle].Contains(edgeToBeAligned.a))
		{
			array[1] = edgeToBeAligned.a;
			array2[1] = edgeToBeAligned.b;
		}
		else
		{
			array[1] = edgeToBeAligned.b;
			array2[1] = edgeToBeAligned.a;
		}
		float num = Vector2.Distance(uVs[edgeToAlignTo.a], uVs[edgeToAlignTo.b]);
		float num2 = Vector2.Distance(uVs[edgeToBeAligned.a], uVs[edgeToBeAligned.b]);
		float num3 = num / num2;
		int[] distinctIndexesInternal = faceToMove.distinctIndexesInternal;
		foreach (int num4 in distinctIndexesInternal)
		{
			uVs[num4] = uVs[num4].ScaleAroundPoint(Vector2.zero, Vector2.one * num3);
		}
		Vector2 vector = (uVs[edgeToAlignTo.a] + uVs[edgeToAlignTo.b]) / 2f;
		Vector2 vector2 = (uVs[edgeToBeAligned.a] + uVs[edgeToBeAligned.b]) / 2f;
		Vector2 vector3 = vector - vector2;
		distinctIndexesInternal = faceToMove.distinctIndexesInternal;
		foreach (int num5 in distinctIndexesInternal)
		{
			uVs[num5] += vector3;
		}
		Vector2 vector4 = uVs[array2[0]] - uVs[array[0]];
		Vector2 vector5 = uVs[array2[1]] - uVs[array[1]];
		float num6 = Vector2.Angle(vector4, vector5);
		if (Vector3.Cross(vector4, vector5).z < 0f)
		{
			num6 = 360f - num6;
		}
		distinctIndexesInternal = faceToMove.distinctIndexesInternal;
		foreach (int num7 in distinctIndexesInternal)
		{
			uVs[num7] = uVs[num7].RotateAroundPoint(vector, num6);
		}
		float num8 = Mathf.Abs(Vector2.Distance(uVs[array[0]], uVs[array[1]])) + Mathf.Abs(Vector2.Distance(uVs[array2[0]], uVs[array2[1]]));
		if (num8 > 0.02f)
		{
			distinctIndexesInternal = faceToMove.distinctIndexesInternal;
			foreach (int num9 in distinctIndexesInternal)
			{
				uVs[num9] = uVs[num9].RotateAroundPoint(vector, 180f);
			}
			float num10 = Mathf.Abs(Vector2.Distance(uVs[array[0]], uVs[array[1]])) + Mathf.Abs(Vector2.Distance(uVs[array2[0]], uVs[array2[1]]));
			if (num10 < num8)
			{
				num8 = num10;
			}
			else
			{
				distinctIndexesInternal = faceToMove.distinctIndexesInternal;
				foreach (int num11 in distinctIndexesInternal)
				{
					uVs[num11] = uVs[num11].RotateAroundPoint(vector, 180f);
				}
			}
		}
		mesh.SplitUVs(faceToMove.distinctIndexesInternal);
		mesh.SetTexturesCoincident(array);
		mesh.SetTexturesCoincident(array2);
		ApplyUVs(mesh, uVs, channel);
		return true;
	}

	internal static Vector2[] GetUVs(ProBuilderMesh mesh, int channel)
	{
		if (channel != 1)
		{
			if ((uint)(channel - 2) <= 1u)
			{
				if ((channel == 2) ? mesh.HasArrays(MeshArrays.Texture2) : mesh.HasArrays(MeshArrays.Texture3))
				{
					List<Vector4> list = new List<Vector4>();
					mesh.GetUVs(channel, list);
					return ((IEnumerable<Vector4>)list).Select((Func<Vector4, Vector2>)((Vector4 x) => x)).ToArray();
				}
				return null;
			}
			return mesh.texturesInternal;
		}
		if (mesh.mesh == null)
		{
			return null;
		}
		return mesh.mesh.uv2;
	}

	internal static void ApplyUVs(ProBuilderMesh mesh, Vector2[] uvs, int channel, bool applyToMesh = true)
	{
		switch (channel)
		{
		case 0:
			mesh.texturesInternal = uvs;
			if (applyToMesh && mesh.mesh != null)
			{
				mesh.mesh.uv = uvs;
			}
			break;
		case 1:
			if (applyToMesh && mesh.mesh != null)
			{
				mesh.mesh.uv2 = uvs;
			}
			break;
		case 2:
		case 3:
		{
			int vertexCount = mesh.vertexCount;
			if (vertexCount != uvs.Length)
			{
				throw new IndexOutOfRangeException("uvs");
			}
			List<Vector4> list = new List<Vector4>(vertexCount);
			for (int i = 0; i < vertexCount; i++)
			{
				list.Add(uvs[i]);
			}
			mesh.SetUVs(channel, list);
			if (applyToMesh && mesh.mesh != null)
			{
				mesh.mesh.SetUVs(channel, list);
			}
			break;
		}
		}
	}

	public static void SewUVs(this ProBuilderMesh mesh, int[] indexes, float delta)
	{
		Vector2[] array = mesh.texturesInternal;
		if (array == null || array.Length != mesh.vertexCount)
		{
			array = new Vector2[mesh.vertexCount];
		}
		Dictionary<int, int> sharedTextureLookup = mesh.sharedTextureLookup;
		for (int i = 0; i < indexes.Length - 1; i++)
		{
			for (int j = i + 1; j < indexes.Length; j++)
			{
				if (!sharedTextureLookup.TryGetValue(indexes[i], out var value))
				{
					sharedTextureLookup.Add(indexes[i], value = sharedTextureLookup.Count);
				}
				if (!sharedTextureLookup.TryGetValue(indexes[j], out var b))
				{
					sharedTextureLookup.Add(indexes[j], b = sharedTextureLookup.Count);
				}
				if (value != b && Vector2.Distance(array[indexes[i]], array[indexes[j]]) < delta)
				{
					Vector3 vector = (array[indexes[i]] + array[indexes[j]]) / 2f;
					array[indexes[i]] = vector;
					array[indexes[j]] = vector;
					int[] array2 = (from x in sharedTextureLookup
						where x.Value == b
						select x into y
						select y.Key).ToArray();
					foreach (int key in array2)
					{
						sharedTextureLookup[key] = value;
					}
				}
			}
		}
		mesh.SetSharedTextures(sharedTextureLookup);
	}

	public static void CollapseUVs(this ProBuilderMesh mesh, int[] indexes)
	{
		Vector2[] texturesInternal = mesh.texturesInternal;
		Vector2 vector = Math.Average(texturesInternal.ValuesWithIndexes(indexes));
		foreach (int num in indexes)
		{
			texturesInternal[num] = vector;
		}
		mesh.SetTexturesCoincident(indexes);
	}

	public static void SplitUVs(this ProBuilderMesh mesh, IEnumerable<int> indexes)
	{
		Dictionary<int, int> sharedTextureLookup = mesh.sharedTextureLookup;
		int count = sharedTextureLookup.Count;
		foreach (int index in indexes)
		{
			if (sharedTextureLookup.TryGetValue(index, out var _))
			{
				sharedTextureLookup[index] = count++;
			}
		}
		mesh.SetSharedTextures(sharedTextureLookup);
	}

	internal static void SplitUVs(ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		Dictionary<int, int> sharedTextureLookup = mesh.sharedTextureLookup;
		int count = sharedTextureLookup.Count;
		foreach (Face face in faces)
		{
			int[] distinctIndexesInternal = face.distinctIndexesInternal;
			foreach (int key in distinctIndexesInternal)
			{
				if (sharedTextureLookup.TryGetValue(key, out var _))
				{
					sharedTextureLookup[key] = count++;
				}
			}
		}
		mesh.SetSharedTextures(sharedTextureLookup);
	}

	internal static void ProjectFacesAuto(ProBuilderMesh mesh, Face[] faces, int channel)
	{
		if (faces.Length >= 1)
		{
			int[] array = faces.SelectMany((Face x) => x.distinctIndexesInternal).ToArray();
			Vector3 zero = Vector3.zero;
			Face[] array2 = faces;
			foreach (Face face in array2)
			{
				Vector3 vector = Math.Normal(mesh, face);
				zero += vector;
			}
			zero /= (float)faces.Length;
			Vector2[] array3 = Projection.PlanarProject(mesh.positionsInternal, array, zero);
			Vector2[] uVs = GetUVs(mesh, channel);
			for (int j = 0; j < array.Length; j++)
			{
				uVs[array[j]] = array3[j];
			}
			ApplyUVs(mesh, uVs, channel);
			array2 = faces;
			foreach (Face face2 in array2)
			{
				face2.elementGroup = -1;
				mesh.SplitUVs(face2.distinctIndexesInternal);
			}
			mesh.SewUVs(faces.SelectMany((Face x) => x.distinctIndexesInternal).ToArray(), 0.001f);
		}
	}

	public static void ProjectFacesBox(ProBuilderMesh mesh, Face[] faces, int channel = 0)
	{
		Vector2[] uVs = GetUVs(mesh, channel);
		Dictionary<ProjectionAxis, List<Face>> dictionary = new Dictionary<ProjectionAxis, List<Face>>();
		for (int i = 0; i < faces.Length; i++)
		{
			ProjectionAxis key = Projection.VectorToProjectionAxis(Math.Normal(mesh, faces[i]));
			if (dictionary.ContainsKey(key))
			{
				dictionary[key].Add(faces[i]);
			}
			else
			{
				dictionary.Add(key, new List<Face> { faces[i] });
			}
			faces[i].elementGroup = -1;
			faces[i].manualUV = true;
		}
		foreach (KeyValuePair<ProjectionAxis, List<Face>> item in dictionary)
		{
			int[] array = item.Value.SelectMany((Face x) => x.distinctIndexesInternal).ToArray();
			Vector2[] array2 = Projection.PlanarProject(mesh.positionsInternal, array, Projection.ProjectionAxisToVector(item.Key));
			for (int j = 0; j < array.Length; j++)
			{
				uVs[array[j]] = array2[j];
			}
			mesh.SplitUVs(array);
		}
		ApplyUVs(mesh, uVs, channel);
	}

	internal static Vector2 FindMinimalUV(Vector2[] uvs, int[] indices = null, float xMin = 0f, float yMin = 0f)
	{
		int num = ((indices == null) ? uvs.Length : indices.Length);
		bool flag = xMin == 0f && yMin == 0f;
		for (int i = 0; i < num; i++)
		{
			int num2 = ((indices == null) ? i : indices[i]);
			if (flag)
			{
				xMin = uvs[num2].x;
				yMin = uvs[num2].y;
				flag = false;
				continue;
			}
			if (uvs[num2].x < xMin)
			{
				xMin = uvs[num2].x;
			}
			if (uvs[num2].y < yMin)
			{
				yMin = uvs[num2].y;
			}
		}
		return new Vector2(xMin, yMin);
	}

	public static void ProjectFacesBox(ProBuilderMesh mesh, Face[] faces, Vector2 lowerLeftAnchor, int channel = 0)
	{
		Vector2[] uVs = GetUVs(mesh, channel);
		Dictionary<ProjectionAxis, List<Face>> dictionary = new Dictionary<ProjectionAxis, List<Face>>();
		for (int i = 0; i < faces.Length; i++)
		{
			ProjectionAxis key = Projection.VectorToProjectionAxis(Math.Normal(mesh, faces[i]));
			if (dictionary.ContainsKey(key))
			{
				dictionary[key].Add(faces[i]);
			}
			else
			{
				dictionary.Add(key, new List<Face> { faces[i] });
			}
			faces[i].elementGroup = -1;
			faces[i].manualUV = true;
		}
		foreach (KeyValuePair<ProjectionAxis, List<Face>> item in dictionary)
		{
			int[] array = item.Value.SelectMany((Face x) => x.distinctIndexesInternal).ToArray();
			Vector2[] array2 = Projection.PlanarProject(mesh.positionsInternal, array, Projection.ProjectionAxisToVector(item.Key));
			Vector2 vector = FindMinimalUV(array2);
			for (int j = 0; j < array.Length; j++)
			{
				uVs[array[j]] = array2[j] - vector;
			}
			mesh.SplitUVs(array);
		}
		ApplyUVs(mesh, uVs, channel);
	}

	public static void ProjectFacesSphere(ProBuilderMesh pb, int[] indexes, int channel = 0)
	{
		Face[] facesInternal = pb.facesInternal;
		foreach (Face face in facesInternal)
		{
			if (face.distinctIndexesInternal.ContainsMatch(indexes))
			{
				face.elementGroup = -1;
				face.manualUV = true;
			}
		}
		pb.SplitUVs(indexes);
		Vector2[] array = Projection.SphericalProject(pb.positionsInternal, indexes);
		Vector2[] uVs = GetUVs(pb, channel);
		for (int j = 0; j < indexes.Length; j++)
		{
			uVs[indexes[j]] = array[j];
		}
		ApplyUVs(pb, uVs, channel);
	}

	public static Vector2[] FitUVs(Vector2[] uvs)
	{
		Vector2 vector = Math.SmallestVector2(uvs);
		for (int i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= vector;
		}
		float num = Math.MakeNonZero(Math.LargestValue(Math.LargestVector2(uvs)));
		for (int i = 0; i < uvs.Length; i++)
		{
			uvs[i] /= num;
		}
		return uvs;
	}
}
