using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

public sealed class WingedEdge : IEquatable<WingedEdge>
{
	private static readonly Dictionary<Edge, WingedEdge> k_OppositeEdgeDictionary = new Dictionary<Edge, WingedEdge>();

	public EdgeLookup edge { get; private set; }

	public Face face { get; private set; }

	public WingedEdge next { get; private set; }

	public WingedEdge previous { get; private set; }

	public WingedEdge opposite { get; private set; }

	private WingedEdge()
	{
	}

	public bool Equals(WingedEdge other)
	{
		if (other != null)
		{
			return edge.local.Equals(other.edge.local);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is WingedEdge other && Equals(other))
		{
			return true;
		}
		if (obj is Edge && edge.local.Equals((Edge)obj))
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return edge.local.GetHashCode();
	}

	public int Count()
	{
		WingedEdge wingedEdge = this;
		int num = 0;
		do
		{
			num++;
			wingedEdge = wingedEdge.next;
		}
		while (wingedEdge != null && wingedEdge != this);
		return num;
	}

	public override string ToString()
	{
		return string.Format("Common: {0}\nLocal: {1}\nOpposite: {2}\nFace: {3}", edge.common.ToString(), edge.local.ToString(), (opposite == null) ? "null" : opposite.edge.ToString(), face.ToString());
	}

	internal static int[] MakeQuad(WingedEdge left, WingedEdge right)
	{
		if (left.Count() != 3 || right.Count() != 3)
		{
			return null;
		}
		EdgeLookup[] array = new EdgeLookup[6]
		{
			left.edge,
			left.next.edge,
			left.next.next.edge,
			right.edge,
			right.next.edge,
			right.next.next.edge
		};
		int[] array2 = new int[6];
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 3; j < 6; j++)
			{
				if (array[i].Equals(array[j]))
				{
					num++;
					array2[i] = 1;
					array2[j] = 1;
					break;
				}
			}
		}
		if (num != 1)
		{
			return null;
		}
		int num2 = 0;
		EdgeLookup[] array3 = new EdgeLookup[4];
		for (int k = 0; k < 6; k++)
		{
			if (array2[k] < 1)
			{
				array3[num2++] = array[k];
			}
		}
		int[] array4 = new int[4]
		{
			array3[0].local.a,
			array3[0].local.b,
			-1,
			-1
		};
		int b = array3[0].common.b;
		int num3 = -1;
		if (array3[1].common.a == b)
		{
			array4[2] = array3[1].local.b;
			num3 = array3[1].common.b;
		}
		else if (array3[2].common.a == b)
		{
			array4[2] = array3[2].local.b;
			num3 = array3[2].common.b;
		}
		else if (array3[3].common.a == b)
		{
			array4[2] = array3[3].local.b;
			num3 = array3[3].common.b;
		}
		if (array3[1].common.a == num3)
		{
			array4[3] = array3[1].local.b;
		}
		else if (array3[2].common.a == num3)
		{
			array4[3] = array3[2].local.b;
		}
		else if (array3[3].common.a == num3)
		{
			array4[3] = array3[3].local.b;
		}
		if (array4[2] == -1 || array4[3] == -1)
		{
			return null;
		}
		return array4;
	}

	public WingedEdge GetAdjacentEdgeWithCommonIndex(int common)
	{
		if (next.edge.common.Contains(common))
		{
			return next;
		}
		if (previous.edge.common.Contains(common))
		{
			return previous;
		}
		return null;
	}

	public static List<Edge> SortEdgesByAdjacency(Face face)
	{
		if (face == null || face.edgesInternal == null)
		{
			throw new ArgumentNullException("face");
		}
		List<Edge> list = new List<Edge>(face.edgesInternal);
		SortEdgesByAdjacency(list);
		return list;
	}

	public static void SortEdgesByAdjacency(List<Edge> edges)
	{
		if (edges == null)
		{
			throw new ArgumentNullException("edges");
		}
		for (int i = 1; i < edges.Count; i++)
		{
			int b = edges[i - 1].b;
			for (int j = i + 1; j < edges.Count; j++)
			{
				if (edges[j].a == b || edges[j].b == b)
				{
					Edge value = edges[j];
					edges[j] = edges[i];
					edges[i] = value;
				}
			}
		}
	}

	public static Dictionary<int, List<WingedEdge>> GetSpokes(List<WingedEdge> wings)
	{
		if (wings == null)
		{
			throw new ArgumentNullException("wings");
		}
		Dictionary<int, List<WingedEdge>> dictionary = new Dictionary<int, List<WingedEdge>>();
		List<WingedEdge> value = null;
		for (int i = 0; i < wings.Count; i++)
		{
			if (dictionary.TryGetValue(wings[i].edge.common.a, out value))
			{
				value.Add(wings[i]);
			}
			else
			{
				dictionary.Add(wings[i].edge.common.a, new List<WingedEdge> { wings[i] });
			}
			if (dictionary.TryGetValue(wings[i].edge.common.b, out value))
			{
				value.Add(wings[i]);
				continue;
			}
			dictionary.Add(wings[i].edge.common.b, new List<WingedEdge> { wings[i] });
		}
		return dictionary;
	}

	internal static List<int> SortCommonIndexesByAdjacency(List<WingedEdge> wings, HashSet<int> common)
	{
		List<Edge> list = (from x in wings
			where common.Contains(x.edge.common.a) && common.Contains(x.edge.common.b)
			select x into y
			select y.edge.common).ToList();
		if (list.Count != common.Count)
		{
			return null;
		}
		SortEdgesByAdjacency(list);
		return list.ConvertAll((Edge x) => x.a);
	}

	public static List<WingedEdge> GetWingedEdges(ProBuilderMesh mesh, bool oneWingPerFace = false)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		return GetWingedEdges(mesh, mesh.facesInternal, oneWingPerFace);
	}

	public static List<WingedEdge> GetWingedEdges(ProBuilderMesh mesh, IEnumerable<Face> faces, bool oneWingPerFace = false)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		List<WingedEdge> list = new List<WingedEdge>();
		k_OppositeEdgeDictionary.Clear();
		foreach (Face face in faces)
		{
			List<Edge> list2 = SortEdgesByAdjacency(face);
			int count = list2.Count;
			WingedEdge wingedEdge = null;
			WingedEdge wingedEdge2 = null;
			for (int i = 0; i < count; i++)
			{
				Edge edge = list2[i];
				WingedEdge wingedEdge3 = new WingedEdge();
				wingedEdge3.edge = new EdgeLookup(sharedVertexLookup[edge.a], sharedVertexLookup[edge.b], edge.a, edge.b);
				wingedEdge3.face = face;
				if (i < 1)
				{
					wingedEdge = wingedEdge3;
				}
				if (i > 0)
				{
					wingedEdge3.previous = wingedEdge2;
					wingedEdge2.next = wingedEdge3;
				}
				if (i == count - 1)
				{
					wingedEdge3.next = wingedEdge;
					wingedEdge.previous = wingedEdge3;
				}
				wingedEdge2 = wingedEdge3;
				if (k_OppositeEdgeDictionary.TryGetValue(wingedEdge3.edge.common, out var value))
				{
					value.opposite = wingedEdge3;
					wingedEdge3.opposite = value;
				}
				else
				{
					wingedEdge3.opposite = null;
					k_OppositeEdgeDictionary.Add(wingedEdge3.edge.common, wingedEdge3);
				}
				if (!oneWingPerFace || i < 1)
				{
					list.Add(wingedEdge3);
				}
			}
		}
		return list;
	}
}
