using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

internal static class SelectPathFaces
{
	private static int[] s_cachedPredecessors;

	private static int s_cachedStart;

	private static ProBuilderMesh s_cachedMesh;

	private static int s_cachedFacesCount;

	private static List<WingedEdge> s_cachedWings;

	private static Dictionary<Face, int> s_cachedFacesIndex = new Dictionary<Face, int>();

	public static List<int> GetPath(ProBuilderMesh mesh, int start, int end)
	{
		if (mesh == null)
		{
			throw new ArgumentException("Parameter cannot be null", "mesh");
		}
		if (start < 0 || start > mesh.faceCount - 1)
		{
			throw new ArgumentException("Parameter is out of bounds", "start");
		}
		if (end < 0 || end > mesh.faceCount - 1)
		{
			throw new ArgumentException("Parameter is out of bounds", "end");
		}
		if (start == s_cachedStart && mesh == s_cachedMesh && mesh.faceCount == s_cachedFacesCount)
		{
			return GetMinimalPath(s_cachedPredecessors, start, end);
		}
		int[] predecessors = Dijkstra(mesh, start);
		List<int> minimalPath = GetMinimalPath(predecessors, start, end);
		s_cachedPredecessors = predecessors;
		s_cachedStart = start;
		s_cachedMesh = mesh;
		return minimalPath;
	}

	private static int[] Dijkstra(ProBuilderMesh mesh, int start)
	{
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		if (s_cachedMesh != mesh || s_cachedFacesCount != mesh.faceCount)
		{
			s_cachedWings = WingedEdge.GetWingedEdges(mesh, oneWingPerFace: true);
			s_cachedFacesIndex.Clear();
			s_cachedFacesCount = mesh.faceCount;
			for (int i = 0; i < mesh.facesInternal.Length; i++)
			{
				s_cachedFacesIndex.Add(mesh.facesInternal[i], i);
			}
		}
		int count = s_cachedWings.Count;
		float[] array = new float[count];
		int[] array2 = new int[count];
		for (int j = 0; j < count; j++)
		{
			array[j] = float.MaxValue;
			array2[j] = -1;
		}
		int num = start;
		array[num] = 0f;
		hashSet.Add(num);
		while (hashSet.Count < count)
		{
			WingedEdge wingedEdge = s_cachedWings[num];
			WingedEdge wingedEdge2 = wingedEdge;
			do
			{
				WingedEdge opposite = wingedEdge2.opposite;
				if (opposite == null)
				{
					wingedEdge2 = wingedEdge2.next;
					continue;
				}
				int num2 = s_cachedFacesIndex[opposite.face];
				float weight = GetWeight(num, num2, mesh);
				if (array[num] + weight < array[num2])
				{
					array[num2] = array[num] + weight;
					array2[num2] = num;
				}
				if (!hashSet2.Contains(num2) && !hashSet.Contains(num2))
				{
					hashSet2.Add(num2);
				}
				wingedEdge2 = wingedEdge2.next;
			}
			while (wingedEdge2 != wingedEdge);
			if (hashSet2.Count == 0)
			{
				return array2;
			}
			float num3 = float.MaxValue;
			foreach (int item in hashSet2)
			{
				if (array[item] < num3)
				{
					num3 = array[item];
					num = item;
				}
			}
			hashSet.Add(num);
			hashSet2.Remove(num);
		}
		return array2;
	}

	private static float GetWeight(int face1, int face2, ProBuilderMesh mesh)
	{
		Vector3 vector = Math.Normal(mesh, mesh.facesInternal[face1]);
		Vector3 vector2 = Math.Normal(mesh, mesh.facesInternal[face2]);
		float num = (1f - Vector3.Dot(vector.normalized, vector2.normalized)) * 2f;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		int[] indexesInternal = mesh.facesInternal[face1].indexesInternal;
		foreach (int num2 in indexesInternal)
		{
			zero += mesh.positionsInternal[num2] / mesh.facesInternal[face1].indexesInternal.Length;
		}
		indexesInternal = mesh.facesInternal[face2].indexesInternal;
		foreach (int num3 in indexesInternal)
		{
			zero2 += mesh.positionsInternal[num3] / mesh.facesInternal[face2].indexesInternal.Length;
		}
		float num4 = (zero2 - zero).magnitude * 1f;
		return 10f + num4 + num;
	}

	private static List<int> GetMinimalPath(int[] predecessors, int start, int end)
	{
		if (predecessors[end] == -1)
		{
			return null;
		}
		Stack<int> stack = new Stack<int>();
		for (int num = end; num != start; num = predecessors[num])
		{
			stack.Push(num);
		}
		return stack.ToList();
	}
}
