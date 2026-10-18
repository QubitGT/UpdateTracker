using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace UnityEngine.ProBuilder;

[EditorBrowsable(EditorBrowsableState.Never)]
public class SceneSelection : IEquatable<SceneSelection>
{
	public GameObject gameObject;

	public ProBuilderMesh mesh;

	private List<int> m_Vertices;

	private List<Edge> m_Edges;

	private List<Face> m_Faces;

	[Obsolete("Use SetSingleVertex")]
	public int vertex;

	[Obsolete("Use SetSingleEdge")]
	public Edge edge;

	[Obsolete("Use SetSingleFace")]
	public Face face;

	public List<int> vertexes
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

	public List<Edge> edges
	{
		get
		{
			return m_Edges;
		}
		set
		{
			m_Edges = value;
		}
	}

	public List<Face> faces
	{
		get
		{
			return m_Faces;
		}
		set
		{
			m_Faces = value;
		}
	}

	public SceneSelection(GameObject gameObject = null)
	{
		this.gameObject = gameObject;
		m_Vertices = new List<int>();
		m_Edges = new List<Edge>();
		m_Faces = new List<Face>();
	}

	public SceneSelection(ProBuilderMesh mesh, int vertex)
		: this(mesh, new List<int> { vertex })
	{
	}

	public SceneSelection(ProBuilderMesh mesh, Edge edge)
		: this(mesh, new List<Edge> { edge })
	{
	}

	public SceneSelection(ProBuilderMesh mesh, Face face)
		: this(mesh, new List<Face> { face })
	{
	}

	internal SceneSelection(ProBuilderMesh mesh, List<int> vertexes)
		: this((mesh != null) ? mesh.gameObject : null)
	{
		this.mesh = mesh;
		m_Vertices = vertexes;
		m_Edges = new List<Edge>();
		m_Faces = new List<Face>();
	}

	internal SceneSelection(ProBuilderMesh mesh, List<Edge> edges)
		: this((mesh != null) ? mesh.gameObject : null)
	{
		this.mesh = mesh;
		vertexes = new List<int>();
		this.edges = edges;
		faces = new List<Face>();
	}

	internal SceneSelection(ProBuilderMesh mesh, List<Face> faces)
		: this((mesh != null) ? mesh.gameObject : null)
	{
		this.mesh = mesh;
		vertexes = new List<int>();
		edges = new List<Edge>();
		this.faces = faces;
	}

	public void SetSingleFace(Face face)
	{
		faces.Clear();
		faces.Add(face);
	}

	public void SetSingleVertex(int vertex)
	{
		vertexes.Clear();
		vertexes.Add(vertex);
	}

	public void SetSingleEdge(Edge edge)
	{
		edges.Clear();
		edges.Add(edge);
	}

	public void Clear()
	{
		gameObject = null;
		mesh = null;
		faces.Clear();
		edges.Clear();
		vertexes.Clear();
	}

	public void CopyTo(SceneSelection dst)
	{
		dst.gameObject = gameObject;
		dst.mesh = mesh;
		dst.faces.Clear();
		dst.edges.Clear();
		dst.vertexes.Clear();
		dst.faces.AddRange(faces);
		dst.edges.AddRange(edges);
		dst.vertexes.AddRange(vertexes);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("GameObject: " + ((gameObject != null) ? gameObject.name : null));
		stringBuilder.AppendLine("ProBuilderMesh: " + ((mesh != null) ? mesh.name : null));
		stringBuilder.AppendLine("Face: " + ((faces != null) ? faces.ToString() : null));
		stringBuilder.AppendLine("Edge: " + edges.ToString());
		stringBuilder.AppendLine("Vertex: " + vertexes);
		return stringBuilder.ToString();
	}

	public bool Equals(SceneSelection other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (object.Equals(gameObject, other.gameObject) && object.Equals(mesh, other.mesh) && vertexes.SequenceEqual(other.vertexes) && edges.SequenceEqual(other.edges))
		{
			return faces.SequenceEqual(other.faces);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SceneSelection)obj);
	}

	public override int GetHashCode()
	{
		return (((((((((gameObject != null) ? gameObject.GetHashCode() : 0) * 397) ^ ((mesh != null) ? mesh.GetHashCode() : 0)) * 397) ^ ((vertexes != null) ? vertexes.GetHashCode() : 0)) * 397) ^ ((edges != null) ? edges.GetHashCode() : 0)) * 397) ^ ((faces != null) ? faces.GetHashCode() : 0);
	}

	public static bool operator ==(SceneSelection left, SceneSelection right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(SceneSelection left, SceneSelection right)
	{
		return !object.Equals(left, right);
	}
}
