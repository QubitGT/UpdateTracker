using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

public struct EdgeLookup : IEquatable<EdgeLookup>
{
	private Edge m_Local;

	private Edge m_Common;

	public Edge local
	{
		get
		{
			return m_Local;
		}
		set
		{
			m_Local = value;
		}
	}

	public Edge common
	{
		get
		{
			return m_Common;
		}
		set
		{
			m_Common = value;
		}
	}

	public EdgeLookup(Edge common, Edge local)
	{
		m_Common = common;
		m_Local = local;
	}

	public EdgeLookup(int cx, int cy, int x, int y)
	{
		m_Common = new Edge(cx, cy);
		m_Local = new Edge(x, y);
	}

	public bool Equals(EdgeLookup other)
	{
		return other.common.Equals(common);
	}

	public override bool Equals(object obj)
	{
		if (obj != null)
		{
			return Equals((EdgeLookup)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return common.GetHashCode();
	}

	public static bool operator ==(EdgeLookup a, EdgeLookup b)
	{
		return object.Equals(a, b);
	}

	public static bool operator !=(EdgeLookup a, EdgeLookup b)
	{
		return !object.Equals(a, b);
	}

	public override string ToString()
	{
		return $"Common: ({common.a}, {common.b}), local: ({local.a}, {local.b})";
	}

	public static IEnumerable<EdgeLookup> GetEdgeLookup(IEnumerable<Edge> edges, Dictionary<int, int> lookup)
	{
		return edges.Select((Edge x) => new EdgeLookup(new Edge(lookup[x.a], lookup[x.b]), x));
	}

	public static HashSet<EdgeLookup> GetEdgeLookupHashSet(IEnumerable<Edge> edges, Dictionary<int, int> lookup)
	{
		if (lookup == null || edges == null)
		{
			return null;
		}
		HashSet<EdgeLookup> hashSet = new HashSet<EdgeLookup>();
		foreach (Edge edge in edges)
		{
			hashSet.Add(new EdgeLookup(new Edge(lookup[edge.a], lookup[edge.b]), edge));
		}
		return hashSet;
	}
}
