using System.Collections.Generic;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class QuadUtility
{
	public static List<Face> ToQuads(this ProBuilderMesh mesh, IList<Face> faces, bool smoothing = true)
	{
		HashSet<Face> hashSet = new HashSet<Face>();
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh, faces, oneWingPerFace: true);
		Dictionary<EdgeLookup, float> dictionary = new Dictionary<EdgeLookup, float>();
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			using WingedEdgeEnumerator wingedEdgeEnumerator = new WingedEdgeEnumerator(wingedEdges[i]);
			while (wingedEdgeEnumerator.MoveNext())
			{
				WingedEdge current = wingedEdgeEnumerator.Current;
				if (current.opposite != null && !dictionary.ContainsKey(current.edge))
				{
					float quadScore = mesh.GetQuadScore(current, current.opposite);
					dictionary.Add(current.edge, quadScore);
				}
			}
		}
		List<SimpleTuple<Face, Face>> list = new List<SimpleTuple<Face, Face>>();
		foreach (WingedEdge item in wingedEdges)
		{
			if (!hashSet.Add(item.face))
			{
				continue;
			}
			float num = 0f;
			Face face = null;
			using (WingedEdgeEnumerator wingedEdgeEnumerator2 = new WingedEdgeEnumerator(item))
			{
				while (wingedEdgeEnumerator2.MoveNext())
				{
					WingedEdge current3 = wingedEdgeEnumerator2.Current;
					if ((current3.opposite == null || !hashSet.Contains(current3.opposite.face)) && dictionary.TryGetValue(current3.edge, out var value) && value > num && item.face == GetBestQuadConnection(current3.opposite, dictionary))
					{
						num = value;
						face = current3.opposite.face;
					}
				}
			}
			if (face != null)
			{
				hashSet.Add(face);
				list.Add(new SimpleTuple<Face, Face>(item.face, face));
			}
		}
		return MergeElements.MergePairs(mesh, list, smoothing);
	}

	private static Face GetBestQuadConnection(WingedEdge wing, Dictionary<EdgeLookup, float> connections)
	{
		float num = 0f;
		Face result = null;
		using WingedEdgeEnumerator wingedEdgeEnumerator = new WingedEdgeEnumerator(wing);
		while (wingedEdgeEnumerator.MoveNext())
		{
			WingedEdge current = wingedEdgeEnumerator.Current;
			float value = 0f;
			if (connections.TryGetValue(current.edge, out value) && value > num)
			{
				num = connections[current.edge];
				result = current.opposite.face;
			}
		}
		return result;
	}

	private static float GetQuadScore(this ProBuilderMesh mesh, WingedEdge left, WingedEdge right, float normalThreshold = 0.9f)
	{
		Vertex[] vertices = mesh.GetVertices();
		int[] array = WingedEdge.MakeQuad(left, right);
		if (array == null)
		{
			return 0f;
		}
		Vector3 lhs = Math.Normal(vertices[array[0]].position, vertices[array[1]].position, vertices[array[2]].position);
		Vector3 rhs = Math.Normal(vertices[array[2]].position, vertices[array[3]].position, vertices[array[0]].position);
		float num = Vector3.Dot(lhs, rhs);
		if (num < normalThreshold)
		{
			return 0f;
		}
		Vector3 vector = vertices[array[1]].position - vertices[array[0]].position;
		Vector3 vector2 = vertices[array[2]].position - vertices[array[1]].position;
		Vector3 vector3 = vertices[array[3]].position - vertices[array[2]].position;
		Vector3 vector4 = vertices[array[0]].position - vertices[array[3]].position;
		vector.Normalize();
		vector2.Normalize();
		vector3.Normalize();
		vector4.Normalize();
		float num2 = Mathf.Abs(Vector3.Dot(vector, vector2));
		float num3 = Mathf.Abs(Vector3.Dot(vector2, vector3));
		float num4 = Mathf.Abs(Vector3.Dot(vector3, vector4));
		float num5 = Mathf.Abs(Vector3.Dot(vector4, vector));
		num += 1f - (num2 + num3 + num4 + num5) * 0.25f;
		num += Mathf.Abs(Vector3.Dot(vector, vector3)) * 0.5f;
		num += Mathf.Abs(Vector3.Dot(vector2, vector4)) * 0.5f;
		return num * 0.33f;
	}
}
