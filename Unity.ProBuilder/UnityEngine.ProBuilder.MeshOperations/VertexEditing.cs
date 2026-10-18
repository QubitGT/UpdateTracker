using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder.KdTree;
using UnityEngine.ProBuilder.KdTree.Math;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class VertexEditing
{
	public static int MergeVertices(this ProBuilderMesh mesh, int[] indexes, bool collapseToFirst = false)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		Vertex[] vertices = mesh.GetVertices();
		Vertex vertex = (collapseToFirst ? vertices[indexes[0]] : Vertex.Average(vertices, indexes));
		mesh.SetVerticesCoincident(indexes);
		mesh.SplitUVs(indexes);
		int sharedVertexHandle = mesh.GetSharedVertexHandle(indexes[0]);
		mesh.SetSharedVertexValues(sharedVertexHandle, vertex);
		SharedVertex sharedVertex = mesh.sharedVerticesInternal[sharedVertexHandle];
		List<int> list = new List<int>();
		MeshValidation.RemoveDegenerateTriangles(mesh, list);
		int num = -1;
		for (int i = 0; i < sharedVertex.Count; i++)
		{
			if (!list.Contains(sharedVertex[i]))
			{
				num = sharedVertex[i];
			}
		}
		int num2 = num;
		for (int j = 0; j < list.Count; j++)
		{
			if (num > list[j])
			{
				num2--;
			}
		}
		return num2;
	}

	public static void SplitVertices(this ProBuilderMesh mesh, Edge edge)
	{
		mesh.SplitVertices(new int[2] { edge.a, edge.b });
	}

	public static void SplitVertices(this ProBuilderMesh mesh, IEnumerable<int> vertices)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		int num = sharedVertexLookup.Count;
		foreach (int vertex in vertices)
		{
			num = (sharedVertexLookup[vertex] = num + 1);
		}
		mesh.SetSharedVertices(sharedVertexLookup);
	}

	public static int[] WeldVertices(this ProBuilderMesh mesh, IEnumerable<int> indexes, float neighborRadius)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		Vertex[] vertices = mesh.GetVertices();
		SharedVertex[] sharedVerticesInternal = mesh.sharedVerticesInternal;
		HashSet<int> sharedVertexHandles = mesh.GetSharedVertexHandles(indexes);
		int count = sharedVertexHandles.Count;
		int num = System.Math.Min(32, sharedVertexHandles.Count);
		KdTree<float, int> kdTree = new KdTree<float, int>(3, new FloatMath(), AddDuplicateBehavior.Collect);
		foreach (int item in sharedVertexHandles)
		{
			Vector3 position = vertices[sharedVerticesInternal[item][0]].position;
			kdTree.Add(new float[3] { position.x, position.y, position.z }, item);
		}
		float[] array = new float[3];
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, Vector3> dictionary2 = new Dictionary<int, Vector3>();
		int num2 = sharedVerticesInternal.Length;
		foreach (int item2 in sharedVertexHandles)
		{
			if (dictionary.ContainsKey(item2))
			{
				continue;
			}
			Vector3 position2 = vertices[sharedVerticesInternal[item2][0]].position;
			array[0] = position2.x;
			array[1] = position2.y;
			array[2] = position2.z;
			KdTreeNode<float, int>[] array2 = kdTree.RadialSearch(array, neighborRadius, num);
			if (num < count && array2.Length >= num)
			{
				array2 = kdTree.RadialSearch(array, neighborRadius, count);
				num = System.Math.Min(count, array2.Length + array2.Length / 2);
			}
			Vector3 zero = Vector3.zero;
			float num3 = 0f;
			for (int i = 0; i < array2.Length; i++)
			{
				int value = array2[i].Value;
				if (dictionary.ContainsKey(value))
				{
					continue;
				}
				zero.x += array2[i].Point[0];
				zero.y += array2[i].Point[1];
				zero.z += array2[i].Point[2];
				dictionary.Add(value, num2);
				num3 += 1f;
				if (array2[i].Duplicates != null)
				{
					for (int j = 0; j < array2[i].Duplicates.Count; j++)
					{
						dictionary.Add(array2[i].Duplicates[j], num2);
					}
				}
			}
			zero.x /= num3;
			zero.y /= num3;
			zero.z /= num3;
			dictionary2.Add(num2, zero);
			num2++;
		}
		int[] array3 = new int[dictionary.Count];
		int num4 = 0;
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		foreach (KeyValuePair<int, int> item3 in dictionary)
		{
			SharedVertex sharedVertex = sharedVerticesInternal[item3.Key];
			array3[num4++] = sharedVertex[0];
			for (int k = 0; k < sharedVertex.Count; k++)
			{
				sharedVertexLookup[sharedVertex[k]] = item3.Value;
				vertices[sharedVertex[k]].position = dictionary2[item3.Value];
			}
		}
		mesh.SetSharedVertices(sharedVertexLookup);
		mesh.SetVertices(vertices);
		return array3;
	}

	internal static FaceRebuildData ExplodeVertex(IList<Vertex> vertices, IList<SimpleTuple<WingedEdge, int>> edgeAndCommonIndex, float distance, out Dictionary<int, List<int>> appendedVertices)
	{
		Face face = edgeAndCommonIndex.FirstOrDefault().item1.face;
		List<Edge> list = WingedEdge.SortEdgesByAdjacency(face);
		appendedVertices = new Dictionary<int, List<int>>();
		Vector3 lhs = Math.Normal(vertices, face.indexesInternal);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (SimpleTuple<WingedEdge, int> item3 in edgeAndCommonIndex)
		{
			if (item3.item2 == item3.item1.edge.common.a)
			{
				dictionary.Add(item3.item1.edge.local.a, item3.item2);
			}
			else
			{
				dictionary.Add(item3.item1.edge.local.b, item3.item2);
			}
		}
		int count = list.Count;
		List<Vertex> list2 = new List<Vertex>();
		for (int i = 0; i < count; i++)
		{
			int b = list[i].b;
			if (dictionary.ContainsKey(b))
			{
				Vertex vertex = vertices[list[i].a];
				Vertex vertex2 = vertices[list[i].b];
				Vertex vertex3 = vertices[list[(i + 1) % count].b];
				Vertex vertex4 = vertex - vertex2;
				Vertex vertex5 = vertex3 - vertex2;
				vertex4.Normalize();
				vertex5.Normalize();
				Vertex item = vertices[b] + vertex4 * distance;
				Vertex item2 = vertices[b] + vertex5 * distance;
				appendedVertices.AddOrAppend(dictionary[b], list2.Count);
				list2.Add(item);
				appendedVertices.AddOrAppend(dictionary[b], list2.Count);
				list2.Add(item2);
			}
			else
			{
				list2.Add(vertices[b]);
			}
		}
		if (Triangulation.TriangulateVertices(list2, out var triangles, unordered: false))
		{
			FaceRebuildData obj = new FaceRebuildData
			{
				vertices = list2,
				face = new Face(face)
			};
			Vector3 rhs = Math.Normal(list2, triangles);
			if (Vector3.Dot(lhs, rhs) < 0f)
			{
				triangles.Reverse();
			}
			obj.face.indexesInternal = triangles.ToArray();
			return obj;
		}
		return null;
	}

	private static Edge AlignEdgeWithDirection(EdgeLookup edge, int commonIndex)
	{
		if (edge.common.a == commonIndex)
		{
			return new Edge(edge.local.a, edge.local.b);
		}
		return new Edge(edge.local.b, edge.local.a);
	}
}
