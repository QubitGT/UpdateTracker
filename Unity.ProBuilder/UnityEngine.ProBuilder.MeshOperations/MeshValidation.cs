using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class MeshValidation
{
	private enum AttributeValidationStrategy
	{
		Resize,
		Nullify
	}

	public static bool ContainsDegenerateTriangles(this ProBuilderMesh mesh)
	{
		return mesh.ContainsDegenerateTriangles(mesh.facesInternal);
	}

	public static bool ContainsDegenerateTriangles(this ProBuilderMesh mesh, IList<Face> faces)
	{
		Vector3[] positionsInternal = mesh.positionsInternal;
		foreach (Face face in faces)
		{
			int[] indexesInternal = face.indexesInternal;
			for (int i = 0; i < indexesInternal.Length; i += 3)
			{
				if (Math.TriangleArea(positionsInternal[indexesInternal[i]], positionsInternal[indexesInternal[i + 1]], positionsInternal[indexesInternal[i + 2]]) <= Mathf.Epsilon)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool ContainsDegenerateTriangles(this ProBuilderMesh mesh, Face face)
	{
		Vector3[] positionsInternal = mesh.positionsInternal;
		int[] indexesInternal = face.indexesInternal;
		for (int i = 0; i < indexesInternal.Length; i += 3)
		{
			if (Math.TriangleArea(positionsInternal[indexesInternal[i]], positionsInternal[indexesInternal[i + 1]], positionsInternal[indexesInternal[i + 2]]) <= Mathf.Epsilon)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsNonContiguousTriangles(this ProBuilderMesh mesh, Face face)
	{
		Edge nextEdge = face.edgesInternal[0];
		Edge edge = nextEdge;
		int nextIndex = nextEdge.a;
		int num = 1;
		while (face.TryGetNextEdge(nextEdge, nextEdge.b, ref nextEdge, ref nextIndex) && nextEdge != edge && num < face.edgesInternal.Length)
		{
			num++;
		}
		return num != face.edgesInternal.Length;
	}

	public static List<Face> EnsureFacesAreComposedOfContiguousTriangles(this ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		List<Face> list = new List<Face>();
		foreach (Face face2 in faces)
		{
			if (!mesh.ContainsNonContiguousTriangles(face2))
			{
				continue;
			}
			List<List<Triangle>> list2 = mesh.CollectFaceGroups(face2);
			if (list2.Count < 2)
			{
				continue;
			}
			face2.SetIndexes(list2[0].SelectMany((Triangle x) => x.indices));
			for (int i = 1; i < list2.Count; i++)
			{
				Face face = new Face(face2);
				face.SetIndexes(list2[i].SelectMany((Triangle x) => x.indices));
				list.Add(face);
			}
		}
		List<Face> list3 = new List<Face>(mesh.facesInternal);
		list3.AddRange(list);
		mesh.faces = list3;
		return list;
	}

	internal static List<List<Triangle>> CollectFaceGroups(this ProBuilderMesh mesh, Face face)
	{
		List<List<Triangle>> list = new List<List<Triangle>>();
		int[] indexesInternal = face.indexesInternal;
		for (int i = 0; i < indexesInternal.Length; i += 3)
		{
			Triangle triangle = new Triangle(indexesInternal[i], indexesInternal[i + 1], indexesInternal[i + 2]);
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Any((Triangle x) => x.IsAdjacent(triangle)))
				{
					list[j].Add(triangle);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(new List<Triangle> { triangle });
			}
		}
		return list;
	}

	public static bool RemoveDegenerateTriangles(ProBuilderMesh mesh, List<int> removed = null)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		Dictionary<int, int> sharedTextureLookup = mesh.sharedTextureLookup;
		Vector3[] positionsInternal = mesh.positionsInternal;
		Dictionary<int, int> dictionary = new Dictionary<int, int>(sharedVertexLookup.Count);
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>(sharedTextureLookup.Count);
		List<Face> list = new List<Face>(mesh.faceCount);
		Dictionary<int, int> dictionary3 = new Dictionary<int, int>(8);
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			dictionary3.Clear();
			List<int> list2 = new List<int>();
			int[] indexesInternal = face.indexesInternal;
			for (int j = 0; j < indexesInternal.Length; j += 3)
			{
				if (!(Math.TriangleArea(positionsInternal[indexesInternal[j]], positionsInternal[indexesInternal[j + 1]], positionsInternal[indexesInternal[j + 2]]) > Mathf.Epsilon))
				{
					continue;
				}
				int num = indexesInternal[j];
				int num2 = indexesInternal[j + 1];
				int num3 = indexesInternal[j + 2];
				int num4 = sharedVertexLookup[num];
				int num5 = sharedVertexLookup[num2];
				int num6 = sharedVertexLookup[num3];
				if (num4 != num5 && num4 != num6 && num5 != num6)
				{
					if (!dictionary3.TryGetValue(num4, out var value))
					{
						dictionary3.Add(num4, num);
					}
					else
					{
						num = value;
					}
					if (!dictionary3.TryGetValue(num5, out value))
					{
						dictionary3.Add(num5, num2);
					}
					else
					{
						num2 = value;
					}
					if (!dictionary3.TryGetValue(num6, out value))
					{
						dictionary3.Add(num6, num3);
					}
					else
					{
						num3 = value;
					}
					list2.Add(num);
					list2.Add(num2);
					list2.Add(num3);
					if (!dictionary.ContainsKey(num))
					{
						dictionary.Add(num, num4);
					}
					if (!dictionary.ContainsKey(num2))
					{
						dictionary.Add(num2, num5);
					}
					if (!dictionary.ContainsKey(num3))
					{
						dictionary.Add(num3, num6);
					}
					if (sharedTextureLookup.ContainsKey(num) && !dictionary2.ContainsKey(num))
					{
						dictionary2.Add(num, sharedTextureLookup[num]);
					}
					if (sharedTextureLookup.ContainsKey(num2) && !dictionary2.ContainsKey(num2))
					{
						dictionary2.Add(num2, sharedTextureLookup[num2]);
					}
					if (sharedTextureLookup.ContainsKey(num3) && !dictionary2.ContainsKey(num3))
					{
						dictionary2.Add(num3, sharedTextureLookup[num3]);
					}
				}
			}
			if (list2.Count > 0)
			{
				face.indexesInternal = list2.ToArray();
				list.Add(face);
			}
		}
		mesh.faces = list;
		mesh.SetSharedVertices(dictionary);
		mesh.SetSharedTextures(dictionary2);
		return RemoveUnusedVertices(mesh, removed);
	}

	public static bool RemoveUnusedVertices(ProBuilderMesh mesh, List<int> removed = null)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		bool num = removed != null;
		if (num)
		{
			removed.Clear();
		}
		List<int> list = (num ? removed : new List<int>());
		HashSet<int> hashSet = new HashSet<int>(mesh.facesInternal.SelectMany((Face x) => x.indexes));
		for (int i = 0; i < mesh.positionsInternal.Length; i++)
		{
			if (!hashSet.Contains(i))
			{
				list.Add(i);
			}
		}
		mesh.DeleteVertices(list);
		return list.Count > 0;
	}

	internal static List<int> RebuildIndexes(IEnumerable<int> indices, List<int> removed)
	{
		List<int> list = new List<int>();
		int count = removed.Count;
		foreach (int index in indices)
		{
			int num = ArrayUtility.NearestIndexPriorToValue(removed, index) + 1;
			if (num <= -1 || num >= count || removed[num] != index)
			{
				list.Add(index - num);
			}
		}
		return list;
	}

	internal static List<Edge> RebuildEdges(IEnumerable<Edge> edges, List<int> removed)
	{
		List<Edge> list = new List<Edge>();
		int count = removed.Count;
		foreach (Edge edge in edges)
		{
			int num = ArrayUtility.NearestIndexPriorToValue(removed, edge.a) + 1;
			int num2 = ArrayUtility.NearestIndexPriorToValue(removed, edge.b) + 1;
			if ((num <= -1 || num >= count || removed[num] != edge.a) && (num2 <= -1 || num2 >= count || removed[num2] != edge.b))
			{
				list.Add(new Edge(edge.a - num, edge.b - num2));
			}
		}
		return list;
	}

	internal static void RebuildSelectionIndexes(ProBuilderMesh mesh, ref Face[] faces, ref Edge[] edges, ref int[] indices, IEnumerable<int> removed)
	{
		List<int> list = removed.ToList();
		list.Sort();
		if (faces != null && faces.Length != 0)
		{
			faces = faces.Where((Face x) => mesh.facesInternal.Contains(x)).ToArray();
		}
		if (edges != null && edges.Length != 0)
		{
			edges = RebuildEdges(edges, list).ToArray();
		}
		if (indices != null && indices.Length != 0)
		{
			indices = RebuildIndexes(indices, list).ToArray();
		}
	}

	internal static bool EnsureMeshIsValid(ProBuilderMesh mesh, out int removedVertices)
	{
		removedVertices = 0;
		if (mesh.ContainsDegenerateTriangles())
		{
			Face[] faces = mesh.selectedFacesInternal;
			Edge[] edges = mesh.selectedEdgesInternal;
			int[] indices = mesh.selectedIndexesInternal;
			List<int> list = new List<int>();
			if (RemoveDegenerateTriangles(mesh, list))
			{
				mesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(mesh.positionsInternal);
				RebuildSelectionIndexes(mesh, ref faces, ref edges, ref indices, list);
				mesh.selectedFacesInternal = faces;
				mesh.selectedEdgesInternal = edges;
				mesh.selectedIndexesInternal = indices;
				removedVertices = list.Count;
				return false;
			}
		}
		EnsureValidAttributes(mesh);
		return true;
	}

	private static void EnsureRealNumbers(IList<Vector2> attribute)
	{
		int i = 0;
		for (int num = attribute?.Count ?? 0; i < num; i++)
		{
			attribute[i] = Math.FixNaN(attribute[i]);
		}
	}

	private static void EnsureRealNumbers(IList<Vector3> attribute)
	{
		int i = 0;
		for (int num = attribute?.Count ?? 0; i < num; i++)
		{
			attribute[i] = Math.FixNaN(attribute[i]);
		}
	}

	private static void EnsureRealNumbers(IList<Vector4> attribute)
	{
		int i = 0;
		for (int num = attribute?.Count ?? 0; i < num; i++)
		{
			attribute[i] = Math.FixNaN(attribute[i]);
		}
	}

	private static void EnsureArraySize<T>(ref T[] attribute, int expectedVertexCount, AttributeValidationStrategy strategy = AttributeValidationStrategy.Nullify, T fill = default(T))
	{
		if (attribute == null || attribute.Length == expectedVertexCount)
		{
			return;
		}
		if (strategy == AttributeValidationStrategy.Nullify)
		{
			attribute = null;
			return;
		}
		int num = attribute.Length;
		Array.Resize(ref attribute, expectedVertexCount);
		for (int i = num - 1; i < expectedVertexCount; i++)
		{
			attribute[i] = fill;
		}
	}

	private static void EnsureListSize<T>(ref List<T> attribute, int expectedVertexCount, AttributeValidationStrategy strategy = AttributeValidationStrategy.Nullify, T fill = default(T))
	{
		if (attribute == null || attribute.Count == expectedVertexCount)
		{
			return;
		}
		if (strategy == AttributeValidationStrategy.Nullify)
		{
			attribute = null;
			return;
		}
		int count = attribute.Count;
		List<T> list = new List<T>(expectedVertexCount);
		int i = 0;
		for (int num = Mathf.Min(count, expectedVertexCount); i < num; i++)
		{
			list.Add(attribute[i]);
		}
		for (int j = list.Count - 1; j < expectedVertexCount; j++)
		{
			list.Add(fill);
		}
		attribute = list;
	}

	private static void EnsureValidAttributes(ProBuilderMesh mesh)
	{
		int vertexCount = mesh.vertexCount;
		Vector3[] attribute = mesh.normalsInternal;
		Color[] attribute2 = mesh.colorsInternal;
		Vector4[] attribute3 = mesh.tangentsInternal;
		Vector2[] attribute4 = mesh.texturesInternal;
		List<Vector4> attribute5 = mesh.textures2Internal;
		List<Vector4> attribute6 = mesh.textures3Internal;
		EnsureArraySize(ref attribute, vertexCount);
		EnsureArraySize(ref attribute2, vertexCount);
		EnsureArraySize(ref attribute3, vertexCount);
		EnsureArraySize(ref attribute, vertexCount);
		EnsureArraySize(ref attribute4, vertexCount);
		EnsureListSize(ref attribute5, vertexCount);
		EnsureListSize(ref attribute6, vertexCount);
		EnsureRealNumbers(attribute);
		EnsureRealNumbers(attribute3);
		EnsureRealNumbers(attribute);
		EnsureRealNumbers(attribute4);
		EnsureRealNumbers(attribute5);
		EnsureRealNumbers(attribute6);
	}
}
