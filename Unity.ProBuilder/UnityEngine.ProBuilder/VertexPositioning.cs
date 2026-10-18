using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

public static class VertexPositioning
{
	private static List<int> s_CoincidentVertices = new List<int>();

	public static Vector3[] VerticesInWorldSpace(this ProBuilderMesh mesh)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		int vertexCount = mesh.vertexCount;
		Vector3[] array = new Vector3[vertexCount];
		Vector3[] positionsInternal = mesh.positionsInternal;
		for (int i = 0; i < vertexCount; i++)
		{
			array[i] = mesh.transform.TransformPoint(positionsInternal[i]);
		}
		return array;
	}

	public static void TranslateVerticesInWorldSpace(this ProBuilderMesh mesh, int[] indexes, Vector3 offset)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		mesh.TranslateVerticesInWorldSpace(indexes, offset, 0f, snapAxisOnly: false);
	}

	internal static void TranslateVerticesInWorldSpace(this ProBuilderMesh mesh, int[] indexes, Vector3 offset, float snapValue, bool snapAxisOnly)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		int num = 0;
		mesh.GetCoincidentVertices(indexes, s_CoincidentVertices);
		Matrix4x4 worldToLocalMatrix = mesh.transform.worldToLocalMatrix;
		Vector3 vector = worldToLocalMatrix * offset;
		Vector3[] positionsInternal = mesh.positionsInternal;
		if (Mathf.Abs(snapValue) > Mathf.Epsilon)
		{
			Matrix4x4 localToWorldMatrix = mesh.transform.localToWorldMatrix;
			Vector3Mask vector3Mask = (snapAxisOnly ? new Vector3Mask(offset, 0.0001f) : Vector3Mask.XYZ);
			for (num = 0; num < s_CoincidentVertices.Count; num++)
			{
				Vector3 val = localToWorldMatrix.MultiplyPoint3x4(positionsInternal[s_CoincidentVertices[num]] + vector);
				positionsInternal[s_CoincidentVertices[num]] = worldToLocalMatrix.MultiplyPoint3x4(ProBuilderSnapping.Snap(val, (Vector3)vector3Mask * snapValue));
			}
		}
		else
		{
			for (num = 0; num < s_CoincidentVertices.Count; num++)
			{
				positionsInternal[s_CoincidentVertices[num]] += vector;
			}
		}
		mesh.positionsInternal = positionsInternal;
		mesh.mesh.vertices = positionsInternal;
	}

	public static void TranslateVertices(this ProBuilderMesh mesh, IEnumerable<int> indexes, Vector3 offset)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		mesh.GetCoincidentVertices(indexes, s_CoincidentVertices);
		TranslateVerticesInternal(mesh, s_CoincidentVertices, offset);
	}

	public static void TranslateVertices(this ProBuilderMesh mesh, IEnumerable<Edge> edges, Vector3 offset)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		mesh.GetCoincidentVertices(edges, s_CoincidentVertices);
		TranslateVerticesInternal(mesh, s_CoincidentVertices, offset);
	}

	public static void TranslateVertices(this ProBuilderMesh mesh, IEnumerable<Face> faces, Vector3 offset)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		mesh.GetCoincidentVertices(faces, s_CoincidentVertices);
		TranslateVerticesInternal(mesh, s_CoincidentVertices, offset);
	}

	private static void TranslateVerticesInternal(ProBuilderMesh mesh, IEnumerable<int> indices, Vector3 offset)
	{
		Vector3[] positionsInternal = mesh.positionsInternal;
		int i = 0;
		for (int count = s_CoincidentVertices.Count; i < count; i++)
		{
			positionsInternal[s_CoincidentVertices[i]] += offset;
		}
		mesh.mesh.vertices = positionsInternal;
	}

	public static void SetSharedVertexPosition(this ProBuilderMesh mesh, int sharedVertexHandle, Vector3 position)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		Vector3[] positionsInternal = mesh.positionsInternal;
		foreach (int item in mesh.sharedVerticesInternal[sharedVertexHandle])
		{
			positionsInternal[item] = position;
		}
		mesh.positionsInternal = positionsInternal;
		mesh.mesh.vertices = positionsInternal;
	}

	internal static void SetSharedVertexValues(this ProBuilderMesh mesh, int sharedVertexHandle, Vertex vertex)
	{
		Vertex[] vertices = mesh.GetVertices();
		foreach (int item in mesh.sharedVerticesInternal[sharedVertexHandle])
		{
			vertices[item] = vertex;
		}
		mesh.SetVertices(vertices);
	}
}
