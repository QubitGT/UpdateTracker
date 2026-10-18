using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

[Serializable]
internal struct Triangle : IEquatable<Triangle>
{
	[SerializeField]
	private int m_A;

	[SerializeField]
	private int m_B;

	[SerializeField]
	private int m_C;

	public int a => m_A;

	public int b => m_B;

	public int c => m_C;

	public IEnumerable<int> indices => new int[3] { m_A, m_B, m_C };

	public Triangle(int a, int b, int c)
	{
		m_A = a;
		m_B = b;
		m_C = c;
	}

	public bool Equals(Triangle other)
	{
		if (m_A == other.a && m_B == other.b)
		{
			return m_C == other.c;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Triangle other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((m_A * 397) ^ m_B) * 397) ^ m_C;
	}

	public bool IsAdjacent(Triangle other)
	{
		if (!other.ContainsEdge(new Edge(a, b)) && !other.ContainsEdge(new Edge(b, c)))
		{
			return other.ContainsEdge(new Edge(c, a));
		}
		return true;
	}

	private bool ContainsEdge(Edge edge)
	{
		if (new Edge(a, b) == edge)
		{
			return true;
		}
		if (new Edge(b, c) == edge)
		{
			return true;
		}
		return new Edge(c, a) == edge;
	}
}
