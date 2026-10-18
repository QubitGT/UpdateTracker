using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.Csg;

internal sealed class Model
{
	private List<Vertex> m_Vertices;

	private List<Material> m_Materials;

	private List<List<int>> m_Indices;

	public List<Material> materials
	{
		get
		{
			return m_Materials;
		}
		set
		{
			m_Materials = value;
		}
	}

	public List<Vertex> vertices
	{
		get
		{
			return m_Vertices;
		}
		set
		{
			m_Vertices = value;
		}
	}

	public List<List<int>> indices
	{
		get
		{
			return m_Indices;
		}
		set
		{
			m_Indices = value;
		}
	}

	public Mesh mesh => (Mesh)this;

	public Model(GameObject gameObject)
		: this(gameObject.GetComponent<MeshFilter>()?.sharedMesh, gameObject.GetComponent<MeshRenderer>()?.sharedMaterials, gameObject.GetComponent<Transform>())
	{
	}

	public Model(Mesh mesh, Material[] materials, Transform transform)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		m_Vertices = (from x in mesh.GetVertices()
			select transform.TransformVertex(x)).ToList();
		m_Materials = new List<Material>(materials);
		m_Indices = new List<List<int>>();
		int i = 0;
		for (int subMeshCount = mesh.subMeshCount; i < subMeshCount; i++)
		{
			if (mesh.GetTopology(i) == MeshTopology.Triangles)
			{
				List<int> item = new List<int>();
				mesh.GetIndices(item, i);
				m_Indices.Add(item);
			}
		}
	}

	internal Model(List<Polygon> polygons)
	{
		m_Vertices = new List<Vertex>();
		Dictionary<Material, List<int>> dictionary = new Dictionary<Material, List<int>>();
		int num = 0;
		for (int i = 0; i < polygons.Count; i++)
		{
			Polygon polygon = polygons[i];
			if (!dictionary.TryGetValue(polygon.material, out var value))
			{
				dictionary.Add(polygon.material, value = new List<int>());
			}
			for (int j = 2; j < polygon.vertices.Count; j++)
			{
				m_Vertices.Add(polygon.vertices[0]);
				value.Add(num++);
				m_Vertices.Add(polygon.vertices[j - 1]);
				value.Add(num++);
				m_Vertices.Add(polygon.vertices[j]);
				value.Add(num++);
			}
		}
		m_Materials = dictionary.Keys.ToList();
		m_Indices = dictionary.Values.ToList();
	}

	internal List<Polygon> ToPolygons()
	{
		List<Polygon> list = new List<Polygon>();
		int i = 0;
		for (int count = m_Indices.Count; i < count; i++)
		{
			List<int> list2 = m_Indices[i];
			int j = 0;
			_ = list2.Count;
			for (; j < list2.Count; j += 3)
			{
				List<Vertex> list3 = new List<Vertex>
				{
					m_Vertices[list2[j]],
					m_Vertices[list2[j + 1]],
					m_Vertices[list2[j + 2]]
				};
				list.Add(new Polygon(list3, m_Materials[i]));
			}
		}
		return list;
	}

	public static explicit operator Mesh(Model model)
	{
		Mesh mesh = new Mesh();
		VertexUtility.SetMesh(mesh, model.m_Vertices);
		mesh.subMeshCount = model.m_Indices.Count;
		int i = 0;
		for (int subMeshCount = mesh.subMeshCount; i < subMeshCount; i++)
		{
			mesh.SetIndices(model.m_Indices[i], MeshTopology.Triangles, i);
		}
		return mesh;
	}
}
