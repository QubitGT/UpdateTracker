using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class ConnectElements
{
	public static Face[] Connect(this ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		List<Face> list = mesh.EnsureFacesAreComposedOfContiguousTriangles(faces);
		HashSet<Face> hashSet = new HashSet<Face>(faces);
		if (list.Count > 0)
		{
			foreach (Face item in list)
			{
				hashSet.Add(item);
			}
		}
		IEnumerable<Edge> edges = hashSet.SelectMany((Face x) => x.edgesInternal);
		mesh.Connect(edges, out var addedFaces, out var _, returnFaces: true, returnEdges: false, hashSet);
		return addedFaces;
	}

	public static SimpleTuple<Face[], Edge[]> Connect(this ProBuilderMesh mesh, IEnumerable<Edge> edges)
	{
		mesh.Connect(edges, out var addedFaces, out var connections, returnFaces: true, returnEdges: true);
		return new SimpleTuple<Face[], Edge[]>(addedFaces, connections);
	}

	public static int[] Connect(this ProBuilderMesh mesh, IList<int> indexes)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		int num = mesh.sharedVerticesInternal.Length;
		Dictionary<int, int> lookup = mesh.sharedVertexLookup;
		HashSet<int> hashSet = new HashSet<int>(indexes.Select((int x) => lookup[x]));
		HashSet<int> hashSet2 = new HashSet<int>();
		foreach (int item in hashSet)
		{
			hashSet2.UnionWith(mesh.sharedVerticesInternal[item].arrayInternal);
		}
		Dictionary<Face, List<int>> dictionary = new Dictionary<Face, List<int>>();
		List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			int[] distinctIndexesInternal = face.distinctIndexesInternal;
			for (int j = 0; j < distinctIndexesInternal.Length; j++)
			{
				if (hashSet2.Contains(distinctIndexesInternal[j]))
				{
					dictionary.AddOrAppend(face, distinctIndexesInternal[j]);
				}
			}
		}
		List<ConnectFaceRebuildData> list = new List<ConnectFaceRebuildData>();
		List<Face> list2 = new List<Face>();
		HashSet<int> hashSet3 = new HashSet<int>(mesh.facesInternal.Select((Face x) => x.textureGroup));
		int k = 1;
		foreach (KeyValuePair<Face, List<int>> item2 in dictionary)
		{
			Face key = item2.Key;
			List<ConnectFaceRebuildData> list3 = ((item2.Value.Count == 2) ? ConnectIndexesPerFace(key, item2.Value[0], item2.Value[1], vertices, lookup) : ConnectIndexesPerFace(key, item2.Value, vertices, lookup, num++));
			if (list3 == null)
			{
				continue;
			}
			if (key.textureGroup < 0)
			{
				for (; hashSet3.Contains(k); k++)
				{
				}
				hashSet3.Add(k);
			}
			foreach (ConnectFaceRebuildData item3 in list3)
			{
				item3.faceRebuildData.face.textureGroup = ((key.textureGroup < 0) ? k : key.textureGroup);
				item3.faceRebuildData.face.uv = new AutoUnwrapSettings(key.uv);
				item3.faceRebuildData.face.smoothingGroup = key.smoothingGroup;
				item3.faceRebuildData.face.manualUV = key.manualUV;
				item3.faceRebuildData.face.submeshIndex = key.submeshIndex;
			}
			list2.Add(key);
			list.AddRange(list3);
		}
		FaceRebuildData.Apply(list.Select((ConnectFaceRebuildData x) => x.faceRebuildData), mesh, vertices);
		int num2 = mesh.DeleteFaces(list2).Length;
		lookup = mesh.sharedVertexLookup;
		HashSet<int> hashSet4 = new HashSet<int>();
		for (int l = 0; l < list.Count; l++)
		{
			for (int m = 0; m < list[l].newVertexIndexes.Count; m++)
			{
				hashSet4.Add(lookup[list[l].newVertexIndexes[m] + (list[l].faceRebuildData.Offset() - num2)]);
			}
		}
		mesh.ToMesh();
		return hashSet4.Select((int x) => mesh.sharedVerticesInternal[x][0]).ToArray();
	}

	internal static ActionResult Connect(this ProBuilderMesh mesh, IEnumerable<Edge> edges, out Face[] addedFaces, out Edge[] connections, bool returnFaces = false, bool returnEdges = false, HashSet<Face> faceMask = null)
	{
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		_ = mesh.sharedTextureLookup;
		HashSet<EdgeLookup> hashSet = new HashSet<EdgeLookup>(EdgeLookup.GetEdgeLookup(edges, sharedVertexLookup));
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh);
		Dictionary<Face, List<WingedEdge>> dictionary = new Dictionary<Face, List<WingedEdge>>();
		foreach (WingedEdge item in wingedEdges)
		{
			if (hashSet.Contains(item.edge))
			{
				if (dictionary.TryGetValue(item.face, out var value))
				{
					value.Add(item);
					continue;
				}
				dictionary.Add(item.face, new List<WingedEdge> { item });
			}
		}
		Dictionary<Face, List<WingedEdge>> dictionary2 = new Dictionary<Face, List<WingedEdge>>();
		foreach (KeyValuePair<Face, List<WingedEdge>> item2 in dictionary)
		{
			if (item2.Value.Count <= 1)
			{
				WingedEdge opposite = item2.Value[0].opposite;
				if (opposite == null || !dictionary.TryGetValue(opposite.face, out var value2) || value2.Count <= 1)
				{
					continue;
				}
			}
			dictionary2.Add(item2.Key, item2.Value);
		}
		List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
		List<ConnectFaceRebuildData> list = new List<ConnectFaceRebuildData>();
		List<Face> list2 = new List<Face>();
		HashSet<int> hashSet2 = new HashSet<int>(mesh.facesInternal.Select((Face x) => x.textureGroup));
		int i = 1;
		foreach (KeyValuePair<Face, List<WingedEdge>> item3 in dictionary2)
		{
			Face key = item3.Key;
			List<WingedEdge> value3 = item3.Value;
			int count = value3.Count;
			Vector3 lhs = Math.Normal(vertices, key.indexesInternal);
			if (count == 1 || (faceMask != null && !faceMask.Contains(key)))
			{
				if (InsertVertices(key, value3, vertices, out var data))
				{
					Vector3 rhs = Math.Normal(data.faceRebuildData.vertices, data.faceRebuildData.face.indexesInternal);
					if (Vector3.Dot(lhs, rhs) < 0f)
					{
						data.faceRebuildData.face.Reverse();
					}
					list.Add(data);
				}
			}
			else
			{
				if (count <= 1)
				{
					continue;
				}
				List<ConnectFaceRebuildData> list3 = ((count == 2) ? ConnectEdgesInFace(key, value3[0], value3[1], vertices) : ConnectEdgesInFace(key, value3, vertices));
				if (key.textureGroup < 0)
				{
					for (; hashSet2.Contains(i); i++)
					{
					}
					hashSet2.Add(i);
				}
				if (list3 == null)
				{
					connections = null;
					addedFaces = null;
					return new ActionResult(ActionResult.Status.Failure, "Unable to connect faces");
				}
				foreach (ConnectFaceRebuildData item4 in list3)
				{
					list2.Add(item4.faceRebuildData.face);
					Vector3 rhs2 = Math.Normal(item4.faceRebuildData.vertices, item4.faceRebuildData.face.indexesInternal);
					if (Vector3.Dot(lhs, rhs2) < 0f)
					{
						item4.faceRebuildData.face.Reverse();
					}
					item4.faceRebuildData.face.textureGroup = ((key.textureGroup < 0) ? i : key.textureGroup);
					item4.faceRebuildData.face.uv = new AutoUnwrapSettings(key.uv);
					item4.faceRebuildData.face.submeshIndex = key.submeshIndex;
					item4.faceRebuildData.face.smoothingGroup = key.smoothingGroup;
					item4.faceRebuildData.face.manualUV = key.manualUV;
				}
				list.AddRange(list3);
			}
		}
		FaceRebuildData.Apply(list.Select((ConnectFaceRebuildData x) => x.faceRebuildData), mesh, vertices);
		mesh.sharedTextures = new SharedVertex[0];
		int num = mesh.DeleteFaces(dictionary2.Keys).Length;
		mesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(mesh.positionsInternal);
		mesh.ToMesh();
		if (returnEdges)
		{
			HashSet<int> appended = new HashSet<int>();
			for (int j = 0; j < list.Count; j++)
			{
				for (int k = 0; k < list[j].newVertexIndexes.Count; k++)
				{
					appended.Add(list[j].newVertexIndexes[k] + list[j].faceRebuildData.Offset() - num);
				}
			}
			Dictionary<int, int> sharedVertexLookup2 = mesh.sharedVertexLookup;
			IEnumerable<EdgeLookup> edgeLookup = EdgeLookup.GetEdgeLookup(from x in list.SelectMany((ConnectFaceRebuildData x) => x.faceRebuildData.face.edgesInternal)
				where appended.Contains(x.a) && appended.Contains(x.b)
				select x, sharedVertexLookup2);
			connections = (from x in edgeLookup.Distinct()
				select x.local).ToArray();
		}
		else
		{
			connections = null;
		}
		if (returnFaces)
		{
			addedFaces = list2.ToArray();
		}
		else
		{
			addedFaces = null;
		}
		return new ActionResult(ActionResult.Status.Success, $"Connected {list.Count / 2} Edges");
	}

	private static List<ConnectFaceRebuildData> ConnectEdgesInFace(Face face, WingedEdge a, WingedEdge b, List<Vertex> vertices)
	{
		List<Edge> list = WingedEdge.SortEdgesByAdjacency(face);
		List<Vertex>[] array = new List<Vertex>[2]
		{
			new List<Vertex>(),
			new List<Vertex>()
		};
		List<int>[] array2 = new List<int>[2]
		{
			new List<int>(),
			new List<int>()
		};
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			array[num % 2].Add(vertices[list[i].a]);
			if (list[i].Equals(a.edge.local) || list[i].Equals(b.edge.local))
			{
				Vertex item = Vertex.Mix(vertices[list[i].a], vertices[list[i].b], 0.5f);
				array2[num % 2].Add(array[num % 2].Count);
				array[num % 2].Add(item);
				num++;
				array2[num % 2].Add(array[num % 2].Count);
				array[num % 2].Add(item);
			}
		}
		List<ConnectFaceRebuildData> list2 = new List<ConnectFaceRebuildData>();
		for (int j = 0; j < array.Length; j++)
		{
			FaceRebuildData faceRebuildData = AppendElements.FaceWithVertices(array[j], unordered: false);
			if (faceRebuildData != null)
			{
				list2.Add(new ConnectFaceRebuildData(faceRebuildData, array2[j]));
			}
		}
		return list2;
	}

	private static List<ConnectFaceRebuildData> ConnectEdgesInFace(Face face, List<WingedEdge> edges, List<Vertex> vertices)
	{
		List<Edge> list = WingedEdge.SortEdgesByAdjacency(face);
		int count = edges.Count;
		Vertex item = Vertex.Average(vertices, face.distinctIndexesInternal);
		List<List<Vertex>> list2 = ArrayUtility.Fill((int x) => new List<Vertex>(), count);
		List<List<int>> list3 = ArrayUtility.Fill((int x) => new List<int>(), count);
		HashSet<Edge> hashSet = new HashSet<Edge>(edges.Select((WingedEdge x) => x.edge.local));
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			list2[num % count].Add(vertices[list[i].a]);
			if (hashSet.Contains(list[i]))
			{
				Vertex item2 = Vertex.Mix(vertices[list[i].a], vertices[list[i].b], 0.5f);
				list3[num].Add(list2[num].Count);
				list2[num].Add(item2);
				list3[num].Add(list2[num].Count);
				list2[num].Add(item);
				num = (num + 1) % count;
				list2[num].Add(item2);
			}
		}
		List<ConnectFaceRebuildData> list4 = new List<ConnectFaceRebuildData>();
		for (int j = 0; j < list2.Count; j++)
		{
			FaceRebuildData faceRebuildData = AppendElements.FaceWithVertices(list2[j], unordered: false);
			if (faceRebuildData == null)
			{
				list4.Clear();
				return null;
			}
			list4.Add(new ConnectFaceRebuildData(faceRebuildData, list3[j]));
		}
		return list4;
	}

	private static bool InsertVertices(Face face, List<WingedEdge> edges, List<Vertex> vertices, out ConnectFaceRebuildData data)
	{
		List<Edge> list = WingedEdge.SortEdgesByAdjacency(face);
		List<Vertex> list2 = new List<Vertex>();
		List<int> list3 = new List<int>();
		HashSet<Edge> hashSet = new HashSet<Edge>(edges.Select((WingedEdge x) => x.edge.local));
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(vertices[list[i].a]);
			if (hashSet.Contains(list[i]))
			{
				list3.Add(list2.Count);
				list2.Add(Vertex.Mix(vertices[list[i].a], vertices[list[i].b], 0.5f));
			}
		}
		FaceRebuildData faceRebuildData = AppendElements.FaceWithVertices(list2, unordered: false);
		if (faceRebuildData != null)
		{
			faceRebuildData.face.textureGroup = face.textureGroup;
			faceRebuildData.face.uv = new AutoUnwrapSettings(face.uv);
			faceRebuildData.face.smoothingGroup = face.smoothingGroup;
			faceRebuildData.face.manualUV = face.manualUV;
			faceRebuildData.face.submeshIndex = face.submeshIndex;
			data = new ConnectFaceRebuildData(faceRebuildData, list3);
			return true;
		}
		data = null;
		return false;
	}

	private static List<ConnectFaceRebuildData> ConnectIndexesPerFace(Face face, int a, int b, List<Vertex> vertices, Dictionary<int, int> lookup)
	{
		List<Edge> list = WingedEdge.SortEdgesByAdjacency(face);
		List<Vertex>[] array = new List<Vertex>[2]
		{
			new List<Vertex>(),
			new List<Vertex>()
		};
		List<int>[] array2 = new List<int>[2]
		{
			new List<int>(),
			new List<int>()
		};
		List<int>[] array3 = new List<int>[2]
		{
			new List<int>(),
			new List<int>()
		};
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Contains(a) && list[i].Contains(b))
			{
				return null;
			}
			int a2 = list[i].a;
			array[num].Add(vertices[a2]);
			array2[num].Add(lookup[a2]);
			if (a2 == a || a2 == b)
			{
				num = (num + 1) % 2;
				array3[num].Add(array[num].Count);
				array[num].Add(vertices[a2]);
				array2[num].Add(lookup[a2]);
			}
		}
		List<ConnectFaceRebuildData> list2 = new List<ConnectFaceRebuildData>();
		Vector3 lhs = Math.Normal(vertices, face.indexesInternal);
		for (int j = 0; j < array.Length; j++)
		{
			FaceRebuildData faceRebuildData = AppendElements.FaceWithVertices(array[j], unordered: false);
			faceRebuildData.sharedIndexes = array2[j];
			Vector3 rhs = Math.Normal(array[j], faceRebuildData.face.indexesInternal);
			if (Vector3.Dot(lhs, rhs) < 0f)
			{
				faceRebuildData.face.Reverse();
			}
			list2.Add(new ConnectFaceRebuildData(faceRebuildData, array3[j]));
		}
		return list2;
	}

	private static List<ConnectFaceRebuildData> ConnectIndexesPerFace(Face face, List<int> indexes, List<Vertex> vertices, Dictionary<int, int> lookup, int sharedIndexOffset)
	{
		if (indexes.Count < 3)
		{
			return null;
		}
		List<Edge> list = WingedEdge.SortEdgesByAdjacency(face);
		int count = indexes.Count;
		List<List<Vertex>> list2 = ArrayUtility.Fill((int x) => new List<Vertex>(), count);
		List<List<int>> list3 = ArrayUtility.Fill((int x) => new List<int>(), count);
		List<List<int>> list4 = ArrayUtility.Fill((int x) => new List<int>(), count);
		Vertex item = Vertex.Average(vertices, indexes);
		Vector3 lhs = Math.Normal(vertices, face.indexesInternal);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			int a = list[i].a;
			list2[num].Add(vertices[a]);
			list3[num].Add(lookup[a]);
			if (indexes.Contains(a))
			{
				list4[num].Add(list2[num].Count);
				list2[num].Add(item);
				list3[num].Add(sharedIndexOffset);
				num = (num + 1) % count;
				list4[num].Add(list2[num].Count);
				list2[num].Add(vertices[a]);
				list3[num].Add(lookup[a]);
			}
		}
		List<ConnectFaceRebuildData> list5 = new List<ConnectFaceRebuildData>();
		for (int j = 0; j < list2.Count; j++)
		{
			if (list2[j].Count >= 3)
			{
				FaceRebuildData faceRebuildData = AppendElements.FaceWithVertices(list2[j], unordered: false);
				faceRebuildData.sharedIndexes = list3[j];
				Vector3 rhs = Math.Normal(list2[j], faceRebuildData.face.indexesInternal);
				if (Vector3.Dot(lhs, rhs) < 0f)
				{
					faceRebuildData.face.Reverse();
				}
				list5.Add(new ConnectFaceRebuildData(faceRebuildData, list4[j]));
			}
		}
		return list5;
	}
}
