using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.Csg;

internal static class VertexUtility
{
	public static void GetArrays(IList<Vertex> vertices, out Vector3[] position, out Color[] color, out Vector2[] uv0, out Vector3[] normal, out Vector4[] tangent, out Vector2[] uv2, out List<Vector4> uv3, out List<Vector4> uv4)
	{
		GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4, VertexAttributes.All);
	}

	public static void GetArrays(IList<Vertex> vertices, out Vector3[] position, out Color[] color, out Vector2[] uv0, out Vector3[] normal, out Vector4[] tangent, out Vector2[] uv2, out List<Vector4> uv3, out List<Vector4> uv4, VertexAttributes attributes)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		int count = vertices.Count;
		Vertex vertex = ((count < 1) ? default(Vertex) : vertices[0]);
		bool flag = (attributes & VertexAttributes.Position) == VertexAttributes.Position && vertex.hasPosition;
		bool flag2 = (attributes & VertexAttributes.Color) == VertexAttributes.Color && vertex.hasColor;
		bool flag3 = (attributes & VertexAttributes.Texture0) == VertexAttributes.Texture0 && vertex.hasUV0;
		bool flag4 = (attributes & VertexAttributes.Normal) == VertexAttributes.Normal && vertex.hasNormal;
		bool flag5 = (attributes & VertexAttributes.Tangent) == VertexAttributes.Tangent && vertex.hasTangent;
		bool flag6 = (attributes & VertexAttributes.Texture1) == VertexAttributes.Texture1 && vertex.hasUV2;
		bool flag7 = (attributes & VertexAttributes.Texture2) == VertexAttributes.Texture2 && vertex.hasUV3;
		bool flag8 = (attributes & VertexAttributes.Texture3) == VertexAttributes.Texture3 && vertex.hasUV4;
		position = (flag ? new Vector3[count] : null);
		color = (flag2 ? new Color[count] : null);
		uv0 = (flag3 ? new Vector2[count] : null);
		normal = (flag4 ? new Vector3[count] : null);
		tangent = (flag5 ? new Vector4[count] : null);
		uv2 = (flag6 ? new Vector2[count] : null);
		uv3 = (flag7 ? new List<Vector4>(count) : null);
		uv4 = (flag8 ? new List<Vector4>(count) : null);
		for (int i = 0; i < count; i++)
		{
			if (flag)
			{
				position[i] = vertices[i].position;
			}
			if (flag2)
			{
				color[i] = vertices[i].color;
			}
			if (flag3)
			{
				uv0[i] = vertices[i].uv0;
			}
			if (flag4)
			{
				normal[i] = vertices[i].normal;
			}
			if (flag5)
			{
				tangent[i] = vertices[i].tangent;
			}
			if (flag6)
			{
				uv2[i] = vertices[i].uv2;
			}
			if (flag7)
			{
				uv3.Add(vertices[i].uv3);
			}
			if (flag8)
			{
				uv4.Add(vertices[i].uv4);
			}
		}
	}

	public static Vertex[] GetVertices(this Mesh mesh)
	{
		if (mesh == null)
		{
			return null;
		}
		int vertexCount = mesh.vertexCount;
		Vertex[] array = new Vertex[vertexCount];
		Vector3[] vertices = mesh.vertices;
		Color[] colors = mesh.colors;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		Vector2[] uv = mesh.uv;
		Vector2[] uv2 = mesh.uv2;
		List<Vector4> list = new List<Vector4>();
		List<Vector4> list2 = new List<Vector4>();
		mesh.GetUVs(2, list);
		mesh.GetUVs(3, list2);
		bool flag = vertices != null && vertices.Length == vertexCount;
		bool flag2 = colors != null && colors.Length == vertexCount;
		bool flag3 = normals != null && normals.Length == vertexCount;
		bool flag4 = tangents != null && tangents.Length == vertexCount;
		bool flag5 = uv != null && uv.Length == vertexCount;
		bool flag6 = uv2 != null && uv2.Length == vertexCount;
		bool flag7 = list.Count == vertexCount;
		bool flag8 = list2.Count == vertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			array[i] = default(Vertex);
			if (flag)
			{
				array[i].position = vertices[i];
			}
			if (flag2)
			{
				array[i].color = colors[i];
			}
			if (flag3)
			{
				array[i].normal = normals[i];
			}
			if (flag4)
			{
				array[i].tangent = tangents[i];
			}
			if (flag5)
			{
				array[i].uv0 = uv[i];
			}
			if (flag6)
			{
				array[i].uv2 = uv2[i];
			}
			if (flag7)
			{
				array[i].uv3 = list[i];
			}
			if (flag8)
			{
				array[i].uv4 = list2[i];
			}
		}
		return array;
	}

	public static void SetMesh(Mesh mesh, IList<Vertex> vertices)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		Vector3[] position = null;
		Color[] color = null;
		Vector2[] uv = null;
		Vector3[] normal = null;
		Vector4[] tangent = null;
		Vector2[] uv2 = null;
		List<Vector4> uv3 = null;
		List<Vector4> uv4 = null;
		GetArrays(vertices, out position, out color, out uv, out normal, out tangent, out uv2, out uv3, out uv4);
		mesh.Clear();
		Vertex vertex = vertices[0];
		if (vertex.hasPosition)
		{
			mesh.vertices = position;
		}
		if (vertex.hasColor)
		{
			mesh.colors = color;
		}
		if (vertex.hasUV0)
		{
			mesh.uv = uv;
		}
		if (vertex.hasNormal)
		{
			mesh.normals = normal;
		}
		if (vertex.hasTangent)
		{
			mesh.tangents = tangent;
		}
		if (vertex.hasUV2)
		{
			mesh.uv2 = uv2;
		}
		if (vertex.hasUV3 && uv3 != null)
		{
			mesh.SetUVs(2, uv3);
		}
		if (vertex.hasUV4 && uv4 != null)
		{
			mesh.SetUVs(3, uv4);
		}
	}

	public static Vertex Mix(this Vertex x, Vertex y, float weight)
	{
		float num = 1f - weight;
		Vertex result = default(Vertex);
		result.position = x.position * num + y.position * weight;
		if (x.hasColor && y.hasColor)
		{
			result.color = x.color * num + y.color * weight;
		}
		else if (x.hasColor)
		{
			result.color = x.color;
		}
		else if (y.hasColor)
		{
			result.color = y.color;
		}
		if (x.hasNormal && y.hasNormal)
		{
			result.normal = x.normal * num + y.normal * weight;
		}
		else if (x.hasNormal)
		{
			result.normal = x.normal;
		}
		else if (y.hasNormal)
		{
			result.normal = y.normal;
		}
		if (x.hasTangent && y.hasTangent)
		{
			result.tangent = x.tangent * num + y.tangent * weight;
		}
		else if (x.hasTangent)
		{
			result.tangent = x.tangent;
		}
		else if (y.hasTangent)
		{
			result.tangent = y.tangent;
		}
		if (x.hasUV0 && y.hasUV0)
		{
			result.uv0 = x.uv0 * num + y.uv0 * weight;
		}
		else if (x.hasUV0)
		{
			result.uv0 = x.uv0;
		}
		else if (y.hasUV0)
		{
			result.uv0 = y.uv0;
		}
		if (x.hasUV2 && y.hasUV2)
		{
			result.uv2 = x.uv2 * num + y.uv2 * weight;
		}
		else if (x.hasUV2)
		{
			result.uv2 = x.uv2;
		}
		else if (y.hasUV2)
		{
			result.uv2 = y.uv2;
		}
		if (x.hasUV3 && y.hasUV3)
		{
			result.uv3 = x.uv3 * num + y.uv3 * weight;
		}
		else if (x.hasUV3)
		{
			result.uv3 = x.uv3;
		}
		else if (y.hasUV3)
		{
			result.uv3 = y.uv3;
		}
		if (x.hasUV4 && y.hasUV4)
		{
			result.uv4 = x.uv4 * num + y.uv4 * weight;
		}
		else if (x.hasUV4)
		{
			result.uv4 = x.uv4;
		}
		else if (y.hasUV4)
		{
			result.uv4 = y.uv4;
		}
		return result;
	}

	public static Vertex TransformVertex(this Transform transform, Vertex vertex)
	{
		Vertex result = default(Vertex);
		if (vertex.HasArrays(VertexAttributes.Position))
		{
			result.position = transform.TransformPoint(vertex.position);
		}
		if (vertex.HasArrays(VertexAttributes.Color))
		{
			result.color = vertex.color;
		}
		if (vertex.HasArrays(VertexAttributes.Normal))
		{
			result.normal = transform.TransformDirection(vertex.normal);
		}
		if (vertex.HasArrays(VertexAttributes.Tangent))
		{
			result.tangent = transform.rotation * vertex.tangent;
		}
		if (vertex.HasArrays(VertexAttributes.Texture0))
		{
			result.uv0 = vertex.uv0;
		}
		if (vertex.HasArrays(VertexAttributes.Texture1))
		{
			result.uv2 = vertex.uv2;
		}
		if (vertex.HasArrays(VertexAttributes.Texture2))
		{
			result.uv3 = vertex.uv3;
		}
		if (vertex.HasArrays(VertexAttributes.Texture3))
		{
			result.uv4 = vertex.uv4;
		}
		return result;
	}
}
