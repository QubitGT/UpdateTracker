using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class ExtrudeElements
{
	public static Face[] Extrude(this ProBuilderMesh mesh, IEnumerable<Face> faces, ExtrudeMethod method, float distance)
	{
		if (method == ExtrudeMethod.IndividualFaces)
		{
			return ExtrudePerFace(mesh, faces, distance);
		}
		return ExtrudeAsGroups(mesh, faces, method == ExtrudeMethod.FaceNormal, distance);
	}

	public static Edge[] Extrude(this ProBuilderMesh mesh, IEnumerable<Edge> edges, float distance, bool extrudeAsGroup, bool enableManifoldExtrude)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (edges == null)
		{
			throw new ArgumentNullException("edges");
		}
		SharedVertex[] sharedVerticesInternal = mesh.sharedVerticesInternal;
		List<Edge> list = new List<Edge>();
		List<Face> list2 = new List<Face>();
		Face[] facesInternal;
		foreach (Edge edge2 in edges)
		{
			int num = 0;
			Face item = null;
			facesInternal = mesh.facesInternal;
			foreach (Face face in facesInternal)
			{
				if (mesh.IndexOf(face.edgesInternal, edge2) > -1)
				{
					item = face;
					if (++num > 1)
					{
						break;
					}
				}
			}
			if (enableManifoldExtrude || num < 2)
			{
				list.Add(edge2);
				list2.Add(item);
			}
		}
		if (list.Count < 1)
		{
			return null;
		}
		Vector3[] positionsInternal = mesh.positionsInternal;
		if (!mesh.HasArrays(MeshArrays.Normal))
		{
			mesh.Refresh(RefreshMask.Normals);
		}
		IList<Vector3> normals = mesh.normals;
		int[] array = new int[list.Count * 2];
		int num2 = 0;
		for (int j = 0; j < list.Count; j++)
		{
			array[num2++] = list[j].a;
			array[num2++] = list[j].b;
		}
		List<Edge> list3 = new List<Edge>();
		List<Edge> list4 = new List<Edge>();
		bool flag = mesh.HasArrays(MeshArrays.Color);
		for (int k = 0; k < list.Count; k++)
		{
			Edge edge = list[k];
			Face face2 = list2[k];
			Vector3 vector = (extrudeAsGroup ? InternalMeshUtility.AverageNormalWithIndexes(sharedVerticesInternal[mesh.GetSharedVertexHandle(edge.a)], array, normals) : Math.Normal(mesh, face2));
			Vector3 vector2 = (extrudeAsGroup ? InternalMeshUtility.AverageNormalWithIndexes(sharedVerticesInternal[mesh.GetSharedVertexHandle(edge.b)], array, normals) : Math.Normal(mesh, face2));
			int sharedVertexHandle = mesh.GetSharedVertexHandle(edge.a);
			int sharedVertexHandle2 = mesh.GetSharedVertexHandle(edge.b);
			Vector3[] positions = new Vector3[4]
			{
				positionsInternal[edge.a],
				positionsInternal[edge.b],
				positionsInternal[edge.a] + vector.normalized * distance,
				positionsInternal[edge.b] + vector2.normalized * distance
			};
			Color[] colors = (flag ? new Color[4]
			{
				mesh.colorsInternal[edge.a],
				mesh.colorsInternal[edge.b],
				mesh.colorsInternal[edge.a],
				mesh.colorsInternal[edge.b]
			} : null);
			Face face3 = mesh.AppendFace(positions, colors, new Vector2[4], new Vector4[4], new Vector4[4], new Face(new int[6] { 2, 1, 0, 2, 3, 1 }, face2.submeshIndex, AutoUnwrapSettings.tile, 0, -1, -1, manualUVs: false), new int[4] { sharedVertexHandle, sharedVertexHandle2, -1, -1 });
			list4.Add(new Edge(face3.indexesInternal[3], face3.indexesInternal[4]));
			list3.Add(new Edge(sharedVertexHandle, face3.indexesInternal[3]));
			list3.Add(new Edge(sharedVertexHandle2, face3.indexesInternal[4]));
		}
		if (extrudeAsGroup)
		{
			for (int l = 0; l < list3.Count; l++)
			{
				int a = list3[l].a;
				for (int m = 0; m < list3.Count; m++)
				{
					if (m != l && list3[m].a == a)
					{
						mesh.SetVerticesCoincident(new int[2]
						{
							list3[m].b,
							list3[l].b
						});
						break;
					}
				}
			}
		}
		facesInternal = mesh.facesInternal;
		for (int i = 0; i < facesInternal.Length; i++)
		{
			facesInternal[i].InvalidateCache();
		}
		return list4.ToArray();
	}

	public static List<Face> DetachFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		return mesh.DetachFaces(faces, deleteSourceFaces: true);
	}

	public static List<Face> DetachFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces, bool deleteSourceFaces)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (faces == null)
		{
			throw new ArgumentNullException("faces");
		}
		List<Vertex> list = new List<Vertex>(mesh.GetVertices());
		int num = mesh.sharedVerticesInternal.Length;
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		List<FaceRebuildData> list2 = new List<FaceRebuildData>();
		foreach (Face face in faces)
		{
			FaceRebuildData faceRebuildData = new FaceRebuildData();
			faceRebuildData.vertices = new List<Vertex>();
			faceRebuildData.sharedIndexes = new List<int>();
			faceRebuildData.face = new Face(face);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			int[] array = new int[face.indexesInternal.Length];
			for (int i = 0; i < face.indexesInternal.Length; i++)
			{
				if (dictionary.TryGetValue(face.indexesInternal[i], out var value))
				{
					array[i] = value;
					continue;
				}
				value = (array[i] = faceRebuildData.vertices.Count);
				dictionary.Add(face.indexesInternal[i], value);
				faceRebuildData.vertices.Add(list[face.indexesInternal[i]]);
				faceRebuildData.sharedIndexes.Add(sharedVertexLookup[face.indexesInternal[i]] + num);
			}
			faceRebuildData.face.indexesInternal = array.ToArray();
			list2.Add(faceRebuildData);
		}
		FaceRebuildData.Apply(list2, mesh, list);
		if (deleteSourceFaces)
		{
			mesh.DeleteFaces(faces);
		}
		mesh.ToMesh();
		return list2.Select((FaceRebuildData x) => x.face).ToList();
	}

	private static Face[] ExtrudePerFace(ProBuilderMesh pb, IEnumerable<Face> faces, float distance)
	{
		Face[] array = (faces as Face[]) ?? faces.ToArray();
		if (!array.Any())
		{
			return null;
		}
		List<Vertex> list = new List<Vertex>(pb.GetVertices());
		int num = pb.sharedVerticesInternal.Length;
		int num2 = 0;
		int num3 = 0;
		Dictionary<int, int> sharedVertexLookup = pb.sharedVertexLookup;
		Dictionary<int, int> sharedTextureLookup = pb.sharedTextureLookup;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Face[] array2 = new Face[array.Sum((Face x) => x.edges.Count)];
		Face[] array3 = array;
		foreach (Face face in array3)
		{
			face.smoothingGroup = 0;
			face.textureGroup = -1;
			Vector3 vector = Math.Normal(pb, face) * distance;
			Edge[] edgesInternal = face.edgesInternal;
			dictionary.Clear();
			for (int j = 0; j < edgesInternal.Length; j++)
			{
				int count = list.Count;
				int a = edgesInternal[j].a;
				int b = edgesInternal[j].b;
				if (!dictionary.ContainsKey(a))
				{
					dictionary.Add(a, sharedVertexLookup[a]);
					sharedVertexLookup[a] = num + num2++;
				}
				if (!dictionary.ContainsKey(b))
				{
					dictionary.Add(b, sharedVertexLookup[b]);
					sharedVertexLookup[b] = num + num2++;
				}
				sharedVertexLookup.Add(count, dictionary[a]);
				sharedVertexLookup.Add(count + 1, dictionary[b]);
				sharedVertexLookup.Add(count + 2, sharedVertexLookup[a]);
				sharedVertexLookup.Add(count + 3, sharedVertexLookup[b]);
				Vertex vertex = new Vertex(list[a]);
				Vertex vertex2 = new Vertex(list[b]);
				vertex.position += vector;
				vertex2.position += vector;
				list.Add(new Vertex(list[a]));
				list.Add(new Vertex(list[b]));
				list.Add(vertex);
				list.Add(vertex2);
				Face face2 = new Face(new int[6]
				{
					count,
					count + 1,
					count + 2,
					count + 1,
					count + 3,
					count + 2
				}, face.submeshIndex, new AutoUnwrapSettings(face.uv), face.smoothingGroup, -1, -1, manualUVs: false);
				array2[num3++] = face2;
			}
			for (int k = 0; k < face.distinctIndexesInternal.Length; k++)
			{
				list[face.distinctIndexesInternal[k]].position += vector;
				if (sharedTextureLookup != null && sharedTextureLookup.ContainsKey(face.distinctIndexesInternal[k]))
				{
					sharedTextureLookup.Remove(face.distinctIndexesInternal[k]);
				}
			}
		}
		pb.SetVertices(list);
		int faceCount = pb.faceCount;
		int num4 = array2.Length;
		Face[] array4 = new Face[faceCount + num4];
		Array.Copy(pb.facesInternal, 0, array4, 0, faceCount);
		Array.Copy(array2, 0, array4, faceCount, num4);
		pb.faces = array4;
		pb.SetSharedVertices(sharedVertexLookup);
		pb.SetSharedTextures(sharedTextureLookup);
		return array2;
	}

	private static Face[] ExtrudeAsGroups(ProBuilderMesh mesh, IEnumerable<Face> faces, bool compensateAngleVertexDistance, float distance)
	{
		if (faces == null || !faces.Any())
		{
			return null;
		}
		List<Vertex> list = new List<Vertex>(mesh.GetVertices());
		int num = mesh.sharedVerticesInternal.Length;
		int num2 = 0;
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		Dictionary<int, int> sharedTextureLookup = mesh.sharedTextureLookup;
		List<Face> list2 = new List<Face>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
		Dictionary<int, SimpleTuple<Vector3, Vector3, List<int>>> dictionary4 = new Dictionary<int, SimpleTuple<Vector3, Vector3, List<int>>>();
		foreach (HashSet<Face> faceGroup in GetFaceGroups(WingedEdge.GetWingedEdges(mesh, faces, oneWingPerFace: true)))
		{
			Dictionary<EdgeLookup, Face> perimeterEdges = GetPerimeterEdges(faceGroup, sharedVertexLookup);
			dictionary2.Clear();
			dictionary.Clear();
			foreach (KeyValuePair<EdgeLookup, Face> item2 in perimeterEdges)
			{
				EdgeLookup key = item2.Key;
				Face value = item2.Value;
				int count = list.Count;
				int a = key.local.a;
				int b = key.local.b;
				if (!dictionary.ContainsKey(a))
				{
					dictionary.Add(a, sharedVertexLookup[a]);
					int value2 = -1;
					if (dictionary2.TryGetValue(sharedVertexLookup[a], out value2))
					{
						sharedVertexLookup[a] = value2;
					}
					else
					{
						value2 = num + num2++;
						dictionary2.Add(sharedVertexLookup[a], value2);
						sharedVertexLookup[a] = value2;
					}
				}
				if (!dictionary.ContainsKey(b))
				{
					dictionary.Add(b, sharedVertexLookup[b]);
					int value3 = -1;
					if (dictionary2.TryGetValue(sharedVertexLookup[b], out value3))
					{
						sharedVertexLookup[b] = value3;
					}
					else
					{
						value3 = num + num2++;
						dictionary2.Add(sharedVertexLookup[b], value3);
						sharedVertexLookup[b] = value3;
					}
				}
				sharedVertexLookup.Add(count, dictionary[a]);
				sharedVertexLookup.Add(count + 1, dictionary[b]);
				sharedVertexLookup.Add(count + 2, sharedVertexLookup[a]);
				sharedVertexLookup.Add(count + 3, sharedVertexLookup[b]);
				dictionary3.Add(count + 2, a);
				dictionary3.Add(count + 3, b);
				list.Add(new Vertex(list[a]));
				list.Add(new Vertex(list[b]));
				list.Add(null);
				list.Add(null);
				Face item = new Face(new int[6]
				{
					count,
					count + 1,
					count + 2,
					count + 1,
					count + 3,
					count + 2
				}, value.submeshIndex, new AutoUnwrapSettings(value.uv), 0, -1, -1, manualUVs: false);
				list2.Add(item);
			}
			foreach (Face item3 in faceGroup)
			{
				item3.textureGroup = -1;
				Vector3 vector = Math.Normal(mesh, item3);
				for (int i = 0; i < item3.distinctIndexesInternal.Length; i++)
				{
					int num3 = item3.distinctIndexesInternal[i];
					if (!dictionary.ContainsKey(num3) && dictionary2.ContainsKey(sharedVertexLookup[num3]))
					{
						sharedVertexLookup[num3] = dictionary2[sharedVertexLookup[num3]];
					}
					int key2 = sharedVertexLookup[num3];
					if (sharedTextureLookup != null && sharedTextureLookup.ContainsKey(item3.distinctIndexesInternal[i]))
					{
						sharedTextureLookup.Remove(item3.distinctIndexesInternal[i]);
					}
					if (dictionary4.TryGetValue(key2, out var value4))
					{
						value4.item1 += vector;
						value4.item3.Add(num3);
						dictionary4[key2] = value4;
					}
					else
					{
						dictionary4.Add(key2, new SimpleTuple<Vector3, Vector3, List<int>>(vector, vector, new List<int> { num3 }));
					}
				}
			}
		}
		foreach (KeyValuePair<int, SimpleTuple<Vector3, Vector3, List<int>>> item4 in dictionary4)
		{
			Vector3 vector2 = item4.Value.item1 / item4.Value.item3.Count;
			vector2.Normalize();
			float num4 = (compensateAngleVertexDistance ? Math.Secant(Vector3.Angle(vector2, item4.Value.item2) * (MathF.PI / 180f)) : 1f);
			vector2.x *= distance * num4;
			vector2.y *= distance * num4;
			vector2.z *= distance * num4;
			foreach (int item5 in item4.Value.item3)
			{
				list[item5].position += vector2;
			}
		}
		foreach (KeyValuePair<int, int> item6 in dictionary3)
		{
			list[item6.Key] = new Vertex(list[item6.Value]);
		}
		mesh.SetVertices(list);
		int faceCount = mesh.faceCount;
		int count2 = list2.Count;
		Face[] array = new Face[faceCount + count2];
		Array.Copy(mesh.facesInternal, 0, array, 0, faceCount);
		int j = faceCount;
		for (int num5 = faceCount + count2; j < num5; j++)
		{
			array[j] = list2[j - faceCount];
		}
		mesh.faces = array;
		mesh.SetSharedVertices(sharedVertexLookup);
		mesh.SetSharedTextures(sharedTextureLookup);
		return list2.ToArray();
	}

	private static List<HashSet<Face>> GetFaceGroups(List<WingedEdge> wings)
	{
		HashSet<Face> hashSet = new HashSet<Face>();
		List<HashSet<Face>> list = new List<HashSet<Face>>();
		foreach (WingedEdge wing in wings)
		{
			if (!hashSet.Add(wing.face))
			{
				continue;
			}
			HashSet<Face> hashSet2 = new HashSet<Face> { wing.face };
			ElementSelection.Flood(wing, hashSet2);
			foreach (Face item in hashSet2)
			{
				hashSet.Add(item);
			}
			list.Add(hashSet2);
		}
		return list;
	}

	private static Dictionary<EdgeLookup, Face> GetPerimeterEdges(HashSet<Face> faces, Dictionary<int, int> lookup)
	{
		Dictionary<EdgeLookup, Face> dictionary = new Dictionary<EdgeLookup, Face>();
		HashSet<EdgeLookup> hashSet = new HashSet<EdgeLookup>();
		foreach (Face face in faces)
		{
			Edge[] edgesInternal = face.edgesInternal;
			for (int i = 0; i < edgesInternal.Length; i++)
			{
				Edge edge = edgesInternal[i];
				EdgeLookup edgeLookup = new EdgeLookup(lookup[edge.a], lookup[edge.b], edge.a, edge.b);
				if (!hashSet.Add(edgeLookup))
				{
					if (dictionary.ContainsKey(edgeLookup))
					{
						dictionary.Remove(edgeLookup);
					}
				}
				else
				{
					dictionary.Add(edgeLookup, face);
				}
			}
		}
		return dictionary;
	}
}
