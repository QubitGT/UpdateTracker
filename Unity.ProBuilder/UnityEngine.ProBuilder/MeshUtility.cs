using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.ProBuilder;

public static class MeshUtility
{
	internal static Vertex[] GeneratePerTriangleMesh(Mesh mesh)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		Vertex[] vertices = mesh.GetVertices();
		int subMeshCount = mesh.subMeshCount;
		Vertex[] array = new Vertex[mesh.triangles.Length];
		int[][] array2 = new int[subMeshCount][];
		int num = 0;
		for (int i = 0; i < subMeshCount; i++)
		{
			array2[i] = mesh.GetTriangles(i);
			int num2 = array2[i].Length;
			for (int j = 0; j < num2; j++)
			{
				array[num++] = new Vertex(vertices[array2[i][j]]);
				array2[i][j] = num - 1;
			}
		}
		Vertex.SetMesh(mesh, array);
		mesh.subMeshCount = subMeshCount;
		for (int k = 0; k < subMeshCount; k++)
		{
			mesh.SetTriangles(array2[k], k);
		}
		return array;
	}

	public static void GenerateTangent(Mesh mesh)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		int[] triangles = mesh.triangles;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Vector3[] normals = mesh.normals;
		int num = triangles.Length;
		int num2 = vertices.Length;
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		Vector4[] array3 = new Vector4[num2];
		for (long num3 = 0L; num3 < num; num3 += 3)
		{
			long num4 = triangles[num3];
			long num5 = triangles[num3 + 1];
			long num6 = triangles[num3 + 2];
			Vector3 vector = vertices[num4];
			Vector3 vector2 = vertices[num5];
			Vector3 vector3 = vertices[num6];
			Vector2 vector4 = uv[num4];
			Vector2 vector5 = uv[num5];
			Vector2 vector6 = uv[num6];
			float num7 = vector2.x - vector.x;
			float num8 = vector3.x - vector.x;
			float num9 = vector2.y - vector.y;
			float num10 = vector3.y - vector.y;
			float num11 = vector2.z - vector.z;
			float num12 = vector3.z - vector.z;
			float num13 = vector5.x - vector4.x;
			float num14 = vector6.x - vector4.x;
			float num15 = vector5.y - vector4.y;
			float num16 = vector6.y - vector4.y;
			float num17 = 1f / (num13 * num16 - num14 * num15);
			Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num17, (num16 * num9 - num15 * num10) * num17, (num16 * num11 - num15 * num12) * num17);
			Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num17, (num13 * num10 - num14 * num9) * num17, (num13 * num12 - num14 * num11) * num17);
			array[num4] += vector7;
			array[num5] += vector7;
			array[num6] += vector7;
			array2[num4] += vector8;
			array2[num5] += vector8;
			array2[num6] += vector8;
		}
		for (long num18 = 0L; num18 < num2; num18++)
		{
			Vector3 normal = normals[num18];
			Vector3 tangent = array[num18];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array3[num18].x = tangent.x;
			array3[num18].y = tangent.y;
			array3[num18].z = tangent.z;
			array3[num18].w = ((Vector3.Dot(Vector3.Cross(normal, tangent), array2[num18]) < 0f) ? (-1f) : 1f);
		}
		mesh.tangents = array3;
	}

	public static Mesh DeepCopy(Mesh source)
	{
		Mesh mesh = new Mesh();
		CopyTo(source, mesh);
		return mesh;
	}

	public static void CopyTo(Mesh source, Mesh destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		Vector3[] array = new Vector3[source.vertices.Length];
		int[][] array2 = new int[source.subMeshCount][];
		Vector2[] array3 = new Vector2[source.uv.Length];
		Vector2[] array4 = new Vector2[source.uv2.Length];
		Vector4[] array5 = new Vector4[source.tangents.Length];
		Vector3[] array6 = new Vector3[source.normals.Length];
		Color32[] array7 = new Color32[source.colors32.Length];
		Array.Copy(source.vertices, array, array.Length);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = source.GetTriangles(i);
		}
		Array.Copy(source.uv, array3, array3.Length);
		Array.Copy(source.uv2, array4, array4.Length);
		Array.Copy(source.normals, array6, array6.Length);
		Array.Copy(source.tangents, array5, array5.Length);
		Array.Copy(source.colors32, array7, array7.Length);
		destination.Clear();
		destination.name = source.name;
		destination.vertices = array;
		destination.subMeshCount = array2.Length;
		for (int j = 0; j < array2.Length; j++)
		{
			destination.SetTriangles(array2[j], j);
		}
		destination.uv = array3;
		destination.uv2 = array4;
		destination.tangents = array5;
		destination.normals = array6;
		destination.colors32 = array7;
	}

	internal static T GetMeshChannel<T>(GameObject gameObject, Func<Mesh, T> attributeGetter) where T : IList
	{
		if (gameObject == null)
		{
			throw new ArgumentNullException("gameObject");
		}
		if (attributeGetter == null)
		{
			throw new ArgumentNullException("attributeGetter");
		}
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		Mesh mesh = ((component != null) ? component.sharedMesh : null);
		T result = default(T);
		if (mesh == null)
		{
			return result;
		}
		int vertexCount = mesh.vertexCount;
		MeshRenderer component2 = gameObject.GetComponent<MeshRenderer>();
		Mesh mesh2 = ((component2 != null) ? component2.additionalVertexStreams : null);
		if (mesh2 != null)
		{
			result = attributeGetter(mesh2);
			if (result != null && result.Count == vertexCount)
			{
				return result;
			}
		}
		result = attributeGetter(mesh);
		if (result == null || result.Count != vertexCount)
		{
			return default(T);
		}
		return result;
	}

	private static void PrintAttribute<T>(StringBuilder sb, string title, IEnumerable<T> attrib, string fmt)
	{
		sb.AppendLine("  - " + title);
		if (attrib != null && attrib.Any())
		{
			foreach (T item in attrib)
			{
				sb.AppendLine(string.Format("    " + fmt, item));
			}
			return;
		}
		sb.AppendLine("\tnull");
	}

	public static string Print(Mesh mesh)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		StringBuilder stringBuilder = new StringBuilder();
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Color[] colors = mesh.colors;
		Vector4[] tangents = mesh.tangents;
		List<Vector4> list = new List<Vector4>();
		Vector2[] uv = mesh.uv2;
		List<Vector4> list2 = new List<Vector4>();
		List<Vector4> list3 = new List<Vector4>();
		mesh.GetUVs(0, list);
		mesh.GetUVs(2, list2);
		mesh.GetUVs(3, list3);
		stringBuilder.AppendLine("# Sanity Check");
		stringBuilder.AppendLine(SanityCheck(mesh));
		stringBuilder.AppendLine($"# Attributes ({mesh.vertexCount})");
		PrintAttribute(stringBuilder, $"positions ({vertices.Length})", vertices, "pos: {0:F2}");
		PrintAttribute(stringBuilder, $"normals ({normals.Length})", normals, "nrm: {0:F2}");
		PrintAttribute(stringBuilder, $"colors ({colors.Length})", colors, "col: {0:F2}");
		PrintAttribute(stringBuilder, $"tangents ({tangents.Length})", tangents, "tan: {0:F2}");
		PrintAttribute(stringBuilder, $"uv0 ({list.Count})", list, "uv0: {0:F2}");
		PrintAttribute(stringBuilder, $"uv2 ({uv.Length})", uv, "uv2: {0:F2}");
		PrintAttribute(stringBuilder, $"uv3 ({list2.Count})", list2, "uv3: {0:F2}");
		PrintAttribute(stringBuilder, $"uv4 ({list3.Count})", list3, "uv4: {0:F2}");
		stringBuilder.AppendLine("# Topology");
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			MeshTopology topology = mesh.GetTopology(i);
			int[] indices = mesh.GetIndices(i);
			stringBuilder.AppendLine($"  Submesh[{i}] ({topology})");
			switch (topology)
			{
			case MeshTopology.Points:
			{
				for (int m = 0; m < indices.Length; m++)
				{
					stringBuilder.AppendLine($"\t{indices[m]}");
				}
				break;
			}
			case MeshTopology.Lines:
			{
				for (int k = 0; k < indices.Length; k += 2)
				{
					stringBuilder.AppendLine($"\t{indices[k]}, {indices[k + 1]}");
				}
				break;
			}
			case MeshTopology.Triangles:
			{
				for (int l = 0; l < indices.Length; l += 3)
				{
					stringBuilder.AppendLine($"\t{indices[l]}, {indices[l + 1]}, {indices[l + 2]}");
				}
				break;
			}
			case MeshTopology.Quads:
			{
				for (int j = 0; j < indices.Length; j += 4)
				{
					stringBuilder.AppendLine($"\t{indices[j]}, {indices[j + 1]}, {indices[j + 2]}, {indices[j + 3]}");
				}
				break;
			}
			}
		}
		return stringBuilder.ToString();
	}

	public static uint GetIndexCount(Mesh mesh)
	{
		uint num = 0u;
		if (mesh == null)
		{
			return num;
		}
		int i = 0;
		for (int subMeshCount = mesh.subMeshCount; i < subMeshCount; i++)
		{
			num += mesh.GetIndexCount(i);
		}
		return num;
	}

	public static uint GetPrimitiveCount(Mesh mesh)
	{
		uint num = 0u;
		if (mesh == null)
		{
			return num;
		}
		int i = 0;
		for (int subMeshCount = mesh.subMeshCount; i < subMeshCount; i++)
		{
			if (mesh.GetTopology(i) == MeshTopology.Triangles)
			{
				num += mesh.GetIndexCount(i) / 3;
			}
			else if (mesh.GetTopology(i) == MeshTopology.Quads)
			{
				num += mesh.GetIndexCount(i) / 4;
			}
		}
		return num;
	}

	public static void Compile(ProBuilderMesh probuilderMesh, Mesh targetMesh, MeshTopology preferredTopology = MeshTopology.Triangles)
	{
		if (probuilderMesh == null)
		{
			throw new ArgumentNullException("probuilderMesh");
		}
		if (targetMesh == null)
		{
			throw new ArgumentNullException("targetMesh");
		}
		targetMesh.Clear();
		targetMesh.vertices = probuilderMesh.positionsInternal;
		targetMesh.uv = probuilderMesh.texturesInternal;
		if (probuilderMesh.HasArrays(MeshArrays.Texture2))
		{
			List<Vector4> uvs = new List<Vector4>();
			probuilderMesh.GetUVs(2, uvs);
			targetMesh.SetUVs(2, uvs);
		}
		if (probuilderMesh.HasArrays(MeshArrays.Texture3))
		{
			List<Vector4> uvs2 = new List<Vector4>();
			probuilderMesh.GetUVs(3, uvs2);
			targetMesh.SetUVs(3, uvs2);
		}
		targetMesh.normals = probuilderMesh.GetNormals();
		targetMesh.tangents = probuilderMesh.GetTangents();
		if (probuilderMesh.HasArrays(MeshArrays.Color))
		{
			targetMesh.colors = probuilderMesh.colorsInternal;
		}
		int submeshCount = probuilderMesh.GetComponent<Renderer>().sharedMaterials.Length;
		Submesh[] submeshes = Submesh.GetSubmeshes(probuilderMesh.facesInternal, submeshCount, preferredTopology);
		targetMesh.subMeshCount = submeshes.Length;
		for (int i = 0; i < targetMesh.subMeshCount; i++)
		{
			targetMesh.SetIndices(submeshes[i].m_Indexes, submeshes[i].m_Topology, i, calculateBounds: false);
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
			array[i] = new Vertex();
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

	public static void CollapseSharedVertices(Mesh mesh, Vertex[] vertices = null)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		bool flag = vertices != null;
		if (vertices == null)
		{
			vertices = mesh.GetVertices();
		}
		int subMeshCount = mesh.subMeshCount;
		List<Dictionary<Vertex, int>> list = new List<Dictionary<Vertex, int>>();
		int[][] array = new int[subMeshCount][];
		int num = 0;
		for (int i = 0; i < subMeshCount; i++)
		{
			array[i] = mesh.GetTriangles(i);
			Dictionary<Vertex, int> dictionary = new Dictionary<Vertex, int>();
			for (int j = 0; j < array[i].Length; j++)
			{
				Vertex key = vertices[array[i][j]];
				if (dictionary.TryGetValue(key, out var value))
				{
					array[i][j] = value;
					continue;
				}
				array[i][j] = num;
				dictionary.Add(key, num);
				num++;
			}
			list.Add(dictionary);
		}
		Vertex[] array2 = list.SelectMany((Dictionary<Vertex, int> x) => x.Keys).ToArray();
		if (flag | (array2.Length != vertices.Length))
		{
			Vertex.SetMesh(mesh, array2);
			mesh.subMeshCount = subMeshCount;
			for (int k = 0; k < subMeshCount; k++)
			{
				mesh.SetTriangles(array[k], k);
			}
		}
	}

	public static void FitToSize(ProBuilderMesh mesh, Bounds currentSize, Vector3 sizeToFit)
	{
		if (mesh.vertexCount < 1)
		{
			return;
		}
		Vector3 vector = sizeToFit.Abs().DivideBy(currentSize.size);
		if (!(vector == Vector3.one) && !(vector == Vector3.zero))
		{
			Vector3[] positionsInternal = mesh.positionsInternal;
			if (System.Math.Abs(currentSize.size.x) < 0.001f)
			{
				vector.x = 0f;
			}
			if (System.Math.Abs(currentSize.size.y) < 0.001f)
			{
				vector.y = 0f;
			}
			if (System.Math.Abs(currentSize.size.z) < 0.001f)
			{
				vector.z = 0f;
			}
			int i = 0;
			for (int vertexCount = mesh.vertexCount; i < vertexCount; i++)
			{
				positionsInternal[i] -= currentSize.center;
				positionsInternal[i].Scale(vector);
				positionsInternal[i] += currentSize.center;
			}
			mesh.Rebuild();
		}
	}

	internal static string SanityCheck(ProBuilderMesh mesh)
	{
		return SanityCheck(mesh.GetVertices());
	}

	internal static string SanityCheck(Mesh mesh)
	{
		return SanityCheck(mesh.GetVertices());
	}

	internal static string SanityCheck(IList<Vertex> vertices)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int count = vertices.Count; i < count; i++)
		{
			Vertex vertex = vertices[i];
			if (!Math.IsNumber(vertex.position) || !Math.IsNumber(vertex.color) || !Math.IsNumber(vertex.uv0) || !Math.IsNumber(vertex.normal) || !Math.IsNumber(vertex.tangent) || !Math.IsNumber(vertex.uv2) || !Math.IsNumber(vertex.uv3) || !Math.IsNumber(vertex.uv4))
			{
				stringBuilder.AppendFormat("vertex {0} contains invalid values:\n{1}\n\n", i, vertex.ToString());
			}
		}
		return stringBuilder.ToString();
	}

	internal static bool IsUsedInParticleSystem(ProBuilderMesh pbmesh)
	{
		if (pbmesh.TryGetComponent<ParticleSystem>(out var component))
		{
			ParticleSystem.ShapeModule shape = component.shape;
			if (shape.meshRenderer == pbmesh.renderer)
			{
				shape.meshRenderer = null;
				return true;
			}
		}
		return false;
	}

	internal static void RestoreParticleSystem(ProBuilderMesh pbmesh)
	{
		if (pbmesh.TryGetComponent<ParticleSystem>(out var component))
		{
			ParticleSystem.ShapeModule shape = component.shape;
			shape.meshRenderer = pbmesh.renderer;
		}
	}

	internal static Bounds GetBounds(this ProBuilderMesh mesh)
	{
		if (mesh.mesh != null)
		{
			return mesh.mesh.bounds;
		}
		return Math.GetBounds(mesh.positionsInternal);
	}
}
