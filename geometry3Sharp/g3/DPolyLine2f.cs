using System.Collections.Generic;

namespace g3;

public class DPolyLine2f
{
	public struct Edge
	{
		public int v1;

		public int v2;

		public Edge(int vertex1, int vertex2)
		{
			v1 = vertex1;
			v2 = vertex2;
		}
	}

	public struct Vertex
	{
		public int index;

		public float x;

		public float y;

		public Vertex(float fX, float fY, int nIndex)
		{
			x = fX;
			y = fY;
			index = nIndex;
		}
	}

	private List<Vertex> m_vertices;

	private List<Edge> m_edges;

	public List<Edge> Edges => m_edges;

	public List<Vertex> Vertices => m_vertices;

	public int VertexCount => m_vertices.Count;

	public int EdgeCount => m_edges.Count;

	public DPolyLine2f()
	{
		m_vertices = new List<Vertex>();
		m_edges = new List<Edge>();
	}

	public DPolyLine2f(DPolyLine2f copy)
	{
		m_vertices = new List<Vertex>(copy.m_vertices);
		m_edges = new List<Edge>(copy.m_edges);
	}

	public void Clear()
	{
		m_vertices.Clear();
		m_edges.Clear();
	}

	public Vertex GetVertex(int i)
	{
		return m_vertices[i];
	}

	public int AddVertex(float fX, float fY)
	{
		int count = m_vertices.Count;
		m_vertices.Add(new Vertex(fX, fY, count));
		return count;
	}

	public int AddEdge(int v1, int v2)
	{
		int count = m_edges.Count;
		m_edges.Add(new Edge(v1, v2));
		return count;
	}

	public bool OrderVertices()
	{
		List<Vertex> list = new List<Vertex>(m_vertices.Count);
		List<Edge> list2 = new List<Edge>(m_edges.Count);
		int[] array = new int[2];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		list[num2++] = m_vertices[num];
		int num4 = -1;
		while (num2 != m_vertices.Count)
		{
			int num5 = 0;
			for (int i = 0; i < m_edges.Count; i++)
			{
				if (m_edges[i].v1 == num || m_edges[i].v2 == num)
				{
					if (num5 > 1)
					{
						return false;
					}
					array[num5++] = i;
				}
			}
			if (num5 != 2)
			{
				return false;
			}
			int num6 = 0;
			if (num4 == -1)
			{
				num6 = 0;
			}
			else if (array[0] == num4)
			{
				num6 = array[1];
			}
			else
			{
				if (array[1] != num4)
				{
					return false;
				}
				num6 = array[0];
			}
			int num7 = ((m_edges[num6].v1 == num) ? m_edges[num6].v2 : m_edges[num6].v1);
			list[num2++] = m_vertices[num7];
			list2[num3++] = new Edge(num, num7);
			num = num7;
			num4 = num6;
		}
		list2[num3++] = new Edge(num, 0);
		m_edges = list2;
		m_vertices = list;
		return true;
	}
}
