using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine.ProBuilder;

internal static class MeshHandles
{
	private static List<Vector3> s_Vector2List = new List<Vector3>();

	private static List<Vector3> s_Vector3List = new List<Vector3>();

	private static List<Vector4> s_Vector4List = new List<Vector4>();

	private static List<int> s_IndexList = new List<int>();

	private static List<int> s_SharedVertexIndexList = new List<int>();

	private static readonly Vector2 k_Billboard0 = new Vector2(-1f, -1f);

	private static readonly Vector2 k_Billboard1 = new Vector2(-1f, 1f);

	private static readonly Vector2 k_Billboard2 = new Vector2(1f, -1f);

	private static readonly Vector2 k_Billboard3 = new Vector2(1f, 1f);

	internal static void CreateFaceMesh(ProBuilderMesh mesh, Mesh target)
	{
		target.Clear();
		target.vertices = mesh.positionsInternal;
		target.triangles = mesh.selectedFacesInternal.SelectMany((Face x) => x.indexes).ToArray();
	}

	internal static void CreateFaceMeshFromFaces(ProBuilderMesh mesh, IList<Face> faces, Mesh target)
	{
		target.Clear();
		target.vertices = mesh.positionsInternal;
		target.triangles = faces.SelectMany((Face x) => x.indexes).ToArray();
	}

	internal static void CreateEdgeMesh(ProBuilderMesh mesh, Mesh target)
	{
		int num = 0;
		int faceCount = mesh.faceCount;
		for (int i = 0; i < faceCount; i++)
		{
			num += mesh.facesInternal[i].edgesInternal.Length;
		}
		s_IndexList.Clear();
		s_IndexList.Capacity = num * 2;
		int num2 = 0;
		for (int j = 0; j < faceCount; j++)
		{
			if (num2 >= num)
			{
				break;
			}
			for (int k = 0; k < mesh.facesInternal[j].edgesInternal.Length; k++)
			{
				if (num2 >= num)
				{
					break;
				}
				Edge edge = mesh.facesInternal[j].edgesInternal[k];
				s_IndexList.Add(edge.a);
				s_IndexList.Add(edge.b);
				num2++;
			}
		}
		target.Clear();
		target.indexFormat = ((num * 2 > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		target.name = "ProBuilder::EdgeMesh" + target.GetInstanceID();
		target.vertices = mesh.positionsInternal;
		target.subMeshCount = 1;
		target.SetIndices(s_IndexList, MeshTopology.Lines, 0);
	}

	internal static void CreateEdgeMesh(ProBuilderMesh mesh, Mesh target, Edge[] edges)
	{
		int num = edges.Length;
		int num2 = num * 2;
		s_IndexList.Clear();
		s_IndexList.Capacity = num2;
		for (int i = 0; i < num; i++)
		{
			Edge edge = edges[i];
			s_IndexList.Add(edge.a);
			s_IndexList.Add(edge.b);
		}
		target.Clear();
		target.indexFormat = ((num2 > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		target.name = "ProBuilder::EdgeMesh" + target.GetInstanceID();
		target.vertices = mesh.positionsInternal;
		target.subMeshCount = 1;
		target.SetIndices(s_IndexList, MeshTopology.Lines, 0);
	}

	internal static void CreateVertexMesh(ProBuilderMesh mesh, Mesh target)
	{
		s_SharedVertexIndexList.Clear();
		int num = mesh.sharedVerticesInternal.Length;
		s_SharedVertexIndexList.Capacity = num;
		for (int i = 0; i < num; i++)
		{
			s_SharedVertexIndexList.Add(mesh.sharedVerticesInternal[i][0]);
		}
		CreateVertexMesh(mesh, target, s_SharedVertexIndexList);
	}

	internal static void CreateVertexMesh(ProBuilderMesh mesh, Mesh target, IList<int> indexes)
	{
		if (BuiltinMaterials.geometryShadersSupported)
		{
			CreatePointMesh(mesh.positionsInternal, indexes, target);
		}
		else
		{
			CreatePointBillboardMesh(mesh.positionsInternal, indexes, target);
		}
	}

	private static void CreatePointMesh(Vector3[] positions, IList<int> indexes, Mesh target)
	{
		int num = positions.Length;
		target.Clear();
		target.indexFormat = ((num > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		target.name = "ProBuilder::PointMesh";
		target.vertices = positions;
		target.subMeshCount = 1;
		if (indexes is int[])
		{
			target.SetIndices((int[])indexes, MeshTopology.Points, 0);
		}
		else if (indexes is List<int>)
		{
			target.SetIndices((List<int>)indexes, MeshTopology.Points, 0);
		}
		else
		{
			target.SetIndices(indexes.ToArray(), MeshTopology.Points, 0);
		}
	}

	internal static void CreatePointBillboardMesh(IList<Vector3> positions, Mesh target)
	{
		int count = positions.Count;
		int num = count * 4;
		s_Vector2List.Clear();
		s_Vector3List.Clear();
		s_IndexList.Clear();
		s_Vector2List.Capacity = num;
		s_Vector3List.Capacity = num;
		s_IndexList.Capacity = num;
		for (int i = 0; i < count; i++)
		{
			s_Vector3List.Add(positions[i]);
			s_Vector3List.Add(positions[i]);
			s_Vector3List.Add(positions[i]);
			s_Vector3List.Add(positions[i]);
			s_Vector2List.Add(k_Billboard0);
			s_Vector2List.Add(k_Billboard1);
			s_Vector2List.Add(k_Billboard2);
			s_Vector2List.Add(k_Billboard3);
			s_IndexList.Add(i * 4);
			s_IndexList.Add(i * 4 + 1);
			s_IndexList.Add(i * 4 + 3);
			s_IndexList.Add(i * 4 + 2);
		}
		target.Clear();
		target.indexFormat = ((num > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		target.SetVertices(s_Vector3List);
		target.SetUVs(0, s_Vector2List);
		target.subMeshCount = 1;
		target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
	}

	private static void CreatePointBillboardMesh(IList<Vector3> positions, IList<int> indexes, Mesh target)
	{
		int count = indexes.Count;
		int num = count * 4;
		s_Vector2List.Clear();
		s_Vector3List.Clear();
		s_IndexList.Clear();
		s_Vector2List.Capacity = num;
		s_Vector3List.Capacity = num;
		s_IndexList.Capacity = num;
		for (int i = 0; i < count; i++)
		{
			int index = indexes[i];
			s_Vector3List.Add(positions[index]);
			s_Vector3List.Add(positions[index]);
			s_Vector3List.Add(positions[index]);
			s_Vector3List.Add(positions[index]);
			s_Vector2List.Add(k_Billboard0);
			s_Vector2List.Add(k_Billboard1);
			s_Vector2List.Add(k_Billboard2);
			s_Vector2List.Add(k_Billboard3);
			s_IndexList.Add(i * 4);
			s_IndexList.Add(i * 4 + 1);
			s_IndexList.Add(i * 4 + 3);
			s_IndexList.Add(i * 4 + 2);
		}
		target.Clear();
		target.indexFormat = ((num > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		target.SetVertices(s_Vector3List);
		target.SetUVs(0, s_Vector2List);
		target.subMeshCount = 1;
		target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
	}

	internal static void CreateEdgeBillboardMesh(ProBuilderMesh mesh, Mesh target)
	{
		target.Clear();
		int edgeCount = mesh.edgeCount;
		target.indexFormat = ((edgeCount > 16383) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		Vector3[] positionsInternal = mesh.positionsInternal;
		s_Vector3List.Clear();
		s_Vector4List.Clear();
		s_IndexList.Clear();
		s_Vector3List.Capacity = edgeCount * 4;
		s_Vector4List.Capacity = edgeCount * 4;
		s_IndexList.Capacity = edgeCount * 4;
		int num = 0;
		Face[] facesInternal = mesh.facesInternal;
		for (int i = 0; i < facesInternal.Length; i++)
		{
			Edge[] edgesInternal = facesInternal[i].edgesInternal;
			for (int j = 0; j < edgesInternal.Length; j++)
			{
				Edge edge = edgesInternal[j];
				Vector3 vector = positionsInternal[edge.a];
				Vector3 vector2 = positionsInternal[edge.b];
				Vector3 vector3 = vector2 + (vector2 - vector);
				s_Vector3List.Add(vector);
				s_Vector3List.Add(vector);
				s_Vector3List.Add(vector2);
				s_Vector3List.Add(vector2);
				s_Vector4List.Add(new Vector4(vector2.x, vector2.y, vector2.z, 1f));
				s_Vector4List.Add(new Vector4(vector2.x, vector2.y, vector2.z, -1f));
				s_Vector4List.Add(new Vector4(vector3.x, vector3.y, vector3.z, 1f));
				s_Vector4List.Add(new Vector4(vector3.x, vector3.y, vector3.z, -1f));
				s_IndexList.Add(num);
				s_IndexList.Add(num + 1);
				s_IndexList.Add(num + 3);
				s_IndexList.Add(num + 2);
				num += 4;
			}
		}
		target.SetVertices(s_Vector3List);
		target.SetTangents(s_Vector4List);
		target.subMeshCount = 1;
		target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
	}

	internal static void CreateEdgeBillboardMesh(ProBuilderMesh mesh, Mesh target, ICollection<Edge> edges)
	{
		target.Clear();
		int count = edges.Count;
		target.indexFormat = ((count > 16383) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
		Vector3[] positionsInternal = mesh.positionsInternal;
		s_Vector3List.Clear();
		s_Vector4List.Clear();
		s_IndexList.Clear();
		s_Vector3List.Capacity = count * 4;
		s_Vector4List.Capacity = count * 4;
		s_IndexList.Capacity = count * 4;
		int num = 0;
		foreach (Edge edge in edges)
		{
			Vector3 vector = positionsInternal[edge.a];
			Vector3 vector2 = positionsInternal[edge.b];
			Vector3 vector3 = vector2 + (vector2 - vector);
			s_Vector3List.Add(vector);
			s_Vector3List.Add(vector);
			s_Vector3List.Add(vector2);
			s_Vector3List.Add(vector2);
			s_Vector4List.Add(new Vector4(vector2.x, vector2.y, vector2.z, 1f));
			s_Vector4List.Add(new Vector4(vector2.x, vector2.y, vector2.z, -1f));
			s_Vector4List.Add(new Vector4(vector3.x, vector3.y, vector3.z, 1f));
			s_Vector4List.Add(new Vector4(vector3.x, vector3.y, vector3.z, -1f));
			s_IndexList.Add(num);
			s_IndexList.Add(num + 1);
			s_IndexList.Add(num + 3);
			s_IndexList.Add(num + 2);
			num += 4;
		}
		target.SetVertices(s_Vector3List);
		target.SetTangents(s_Vector4List);
		target.subMeshCount = 1;
		target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
	}
}
