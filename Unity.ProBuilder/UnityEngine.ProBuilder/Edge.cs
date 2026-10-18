using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

[Serializable]
public struct Edge : IEquatable<Edge>
{
	public int a;

	public int b;

	public static readonly Edge Empty = new Edge(-1, -1);

	public Edge(int a, int b)
	{
		this.a = a;
		this.b = b;
	}

	public bool IsValid()
	{
		if (a > -1 && b > -1)
		{
			return a != b;
		}
		return false;
	}

	public override string ToString()
	{
		return "[" + a + ", " + b + "]";
	}

	public bool Equals(Edge other)
	{
		if (a != other.a || b != other.b)
		{
			if (a == other.b)
			{
				return b == other.a;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is Edge)
		{
			return Equals((Edge)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (27 * 29 + ((a < b) ? a : b)) * 29 + ((a < b) ? b : a);
	}

	public static Edge operator +(Edge a, Edge b)
	{
		return new Edge(a.a + b.a, a.b + b.b);
	}

	public static Edge operator -(Edge a, Edge b)
	{
		return new Edge(a.a - b.a, a.b - b.b);
	}

	public static Edge operator +(Edge a, int b)
	{
		return new Edge(a.a + b, a.b + b);
	}

	public static Edge operator -(Edge a, int b)
	{
		return new Edge(a.a - b, a.b - b);
	}

	public static bool operator ==(Edge a, Edge b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Edge a, Edge b)
	{
		return !(a == b);
	}

	public static Edge Add(Edge a, Edge b)
	{
		return a + b;
	}

	public static Edge Subtract(Edge a, Edge b)
	{
		return a - b;
	}

	public bool Equals(Edge other, Dictionary<int, int> lookup)
	{
		if (lookup == null)
		{
			return Equals(other);
		}
		int num = lookup[a];
		int num2 = lookup[b];
		int num3 = lookup[other.a];
		int num4 = lookup[other.b];
		if (num != num3 || num2 != num4)
		{
			if (num == num4)
			{
				return num2 == num3;
			}
			return false;
		}
		return true;
	}

	public bool Contains(int index)
	{
		if (a != index)
		{
			return b == index;
		}
		return true;
	}

	public bool Contains(Edge other)
	{
		if (a != other.a && b != other.a && a != other.b)
		{
			return b == other.a;
		}
		return true;
	}

	internal bool Contains(int index, Dictionary<int, int> lookup)
	{
		int num = lookup[index];
		if (lookup[a] != num)
		{
			return lookup[b] == num;
		}
		return true;
	}

	internal static void GetIndices(IEnumerable<Edge> edges, List<int> indices)
	{
		indices.Clear();
		foreach (Edge edge in edges)
		{
			indices.Add(edge.a);
			indices.Add(edge.b);
		}
	}
}
