using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class ElementSelection
{
	private const int k_MaxHoleIterations = 2048;

	private static readonly Vector3 Vector3_Zero = new Vector3(0f, 0f, 0f);

	public static void GetNeighborFaces(ProBuilderMesh mesh, Edge edge, List<Face> neighborFaces)
	{
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		Edge edge2 = new Edge(sharedVertexLookup[edge.a], sharedVertexLookup[edge.b]);
		Edge edge3 = new Edge(0, 0);
		for (int i = 0; i < mesh.facesInternal.Length; i++)
		{
			Edge[] edgesInternal = mesh.facesInternal[i].edgesInternal;
			for (int j = 0; j < edgesInternal.Length; j++)
			{
				edge3.a = edgesInternal[j].a;
				edge3.b = edgesInternal[j].b;
				if ((edge2.a == sharedVertexLookup[edge3.a] && edge2.b == sharedVertexLookup[edge3.b]) || (edge2.a == sharedVertexLookup[edge3.b] && edge2.b == sharedVertexLookup[edge3.a]))
				{
					neighborFaces.Add(mesh.facesInternal[i]);
					break;
				}
			}
		}
	}

	internal static List<SimpleTuple<Face, Edge>> GetNeighborFaces(ProBuilderMesh mesh, Edge edge)
	{
		List<SimpleTuple<Face, Edge>> list = new List<SimpleTuple<Face, Edge>>();
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		Edge edge2 = new Edge(sharedVertexLookup[edge.a], sharedVertexLookup[edge.b]);
		Edge edge3 = new Edge(0, 0);
		for (int i = 0; i < mesh.facesInternal.Length; i++)
		{
			Edge[] edgesInternal = mesh.facesInternal[i].edgesInternal;
			for (int j = 0; j < edgesInternal.Length; j++)
			{
				edge3.a = edgesInternal[j].a;
				edge3.b = edgesInternal[j].b;
				if ((edge2.a == sharedVertexLookup[edge3.a] && edge2.b == sharedVertexLookup[edge3.b]) || (edge2.a == sharedVertexLookup[edge3.b] && edge2.b == sharedVertexLookup[edge3.a]))
				{
					list.Add(new SimpleTuple<Face, Edge>(mesh.facesInternal[i], edgesInternal[j]));
					break;
				}
			}
		}
		return list;
	}

	internal static List<Face> GetNeighborFaces(ProBuilderMesh mesh, int[] indexes)
	{
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		List<Face> list = new List<Face>();
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int key in indexes)
		{
			hashSet.Add(sharedVertexLookup[key]);
		}
		for (int j = 0; j < mesh.facesInternal.Length; j++)
		{
			int[] distinctIndexesInternal = mesh.facesInternal[j].distinctIndexesInternal;
			for (int k = 0; k < distinctIndexesInternal.Length; k++)
			{
				if (hashSet.Contains(sharedVertexLookup[distinctIndexesInternal[k]]))
				{
					list.Add(mesh.facesInternal[j]);
					break;
				}
			}
		}
		return list;
	}

	internal static Edge[] GetConnectedEdges(ProBuilderMesh mesh, int[] indexes)
	{
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		List<Edge> list = new List<Edge>();
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < indexes.Length; i++)
		{
			hashSet.Add(sharedVertexLookup[indexes[i]]);
		}
		HashSet<Edge> hashSet2 = new HashSet<Edge>();
		Edge item = new Edge(0, 0);
		Face[] facesInternal = mesh.facesInternal;
		for (int j = 0; j < facesInternal.Length; j++)
		{
			foreach (Edge edge in facesInternal[j].edges)
			{
				Edge item2 = new Edge(sharedVertexLookup[edge.a], sharedVertexLookup[edge.b]);
				if (hashSet.Contains(item2.a) || (hashSet.Contains(item2.b) && !hashSet2.Contains(item)))
				{
					list.Add(edge);
					hashSet2.Add(item2);
				}
			}
		}
		return list.ToArray();
	}

	public static IEnumerable<Edge> GetPerimeterEdges(this ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (faces == null)
		{
			throw new ArgumentNullException("faces");
		}
		List<Edge> list = faces.SelectMany((Face x) => x.edgesInternal).ToList();
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		int count = list.Count;
		Dictionary<Edge, List<Edge>> dictionary = new Dictionary<Edge, List<Edge>>();
		for (int i = 0; i < count; i++)
		{
			Edge key = new Edge(sharedVertexLookup[list[i].a], sharedVertexLookup[list[i].b]);
			if (dictionary.TryGetValue(key, out var value))
			{
				value.Add(list[i]);
				continue;
			}
			dictionary.Add(key, new List<Edge> { list[i] });
		}
		return from x in dictionary
			where x.Value.Count < 2
			select x.Value[0];
	}

	internal static int[] GetPerimeterEdges(ProBuilderMesh mesh, IList<Edge> edges)
	{
		int num = edges?.Count ?? 0;
		Edge[] array = mesh.GetSharedVertexHandleEdges(edges).ToArray();
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length - 1; i++)
		{
			for (int j = i + 1; j < array.Length; j++)
			{
				if (array[i].a == array[j].a || array[i].a == array[j].b || array[i].b == array[j].a || array[i].b == array[j].b)
				{
					array2[i]++;
					array2[j]++;
				}
			}
		}
		int num2 = Math.Min(array2);
		List<int> list = new List<int>();
		for (int k = 0; k < array2.Length; k++)
		{
			if (array2[k] <= num2)
			{
				list.Add(k);
			}
		}
		if (list.Count == num)
		{
			return new int[0];
		}
		return list.ToArray();
	}

	internal static IEnumerable<Face> GetPerimeterFaces(ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		Dictionary<Edge, List<Face>> dictionary = new Dictionary<Edge, List<Face>>();
		foreach (Face face in faces)
		{
			Edge[] edgesInternal = face.edgesInternal;
			for (int i = 0; i < edgesInternal.Length; i++)
			{
				Edge edge = edgesInternal[i];
				Edge key = new Edge(sharedVertexLookup[edge.a], sharedVertexLookup[edge.b]);
				if (dictionary.ContainsKey(key))
				{
					dictionary[key].Add(face);
					continue;
				}
				dictionary.Add(key, new List<Face> { face });
			}
		}
		return (from x in dictionary
			where x.Value.Count < 2
			select x.Value[0]).Distinct();
	}

	internal static int[] GetPerimeterVertices(ProBuilderMesh mesh, int[] indexes, Edge[] universal_edges_all)
	{
		int num = indexes.Length;
		_ = mesh.sharedVerticesInternal;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = mesh.GetSharedVertexHandle(indexes[i]);
		}
		int[] array2 = new int[indexes.Length];
		for (int j = 0; j < indexes.Length - 1; j++)
		{
			for (int k = j + 1; k < indexes.Length; k++)
			{
				if (universal_edges_all.Contains(array[j], array[k]))
				{
					array2[j]++;
					array2[k]++;
				}
			}
		}
		int num2 = Math.Min(array2);
		List<int> list = new List<int>();
		for (int l = 0; l < num; l++)
		{
			if (array2[l] <= num2)
			{
				list.Add(l);
			}
		}
		if (list.Count >= num)
		{
			return new int[0];
		}
		return list.ToArray();
	}

	private static WingedEdge EdgeRingNext(WingedEdge edge)
	{
		if (edge == null)
		{
			return null;
		}
		WingedEdge wingedEdge = edge.next;
		WingedEdge previous = edge.previous;
		int num = 0;
		while (wingedEdge != previous && wingedEdge != edge)
		{
			wingedEdge = wingedEdge.next;
			if (wingedEdge == previous)
			{
				return null;
			}
			previous = previous.previous;
			num++;
		}
		if (num % 2 == 0 || wingedEdge == edge)
		{
			wingedEdge = null;
		}
		return wingedEdge;
	}

	internal static IEnumerable<Edge> GetEdgeRing(ProBuilderMesh pb, IEnumerable<Edge> edges)
	{
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(pb);
		List<EdgeLookup> source = EdgeLookup.GetEdgeLookup(edges, pb.sharedVertexLookup).ToList();
		source = source.Distinct().ToList();
		Dictionary<Edge, WingedEdge> dictionary = new Dictionary<Edge, WingedEdge>();
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			if (!dictionary.ContainsKey(wingedEdges[i].edge.common))
			{
				dictionary.Add(wingedEdges[i].edge.common, wingedEdges[i]);
			}
		}
		HashSet<EdgeLookup> hashSet = new HashSet<EdgeLookup>();
		int j = 0;
		for (int count = source.Count; j < count; j++)
		{
			if (!dictionary.TryGetValue(source[j].common, out var value) || hashSet.Contains(value.edge))
			{
				continue;
			}
			WingedEdge wingedEdge = value;
			while (wingedEdge != null && hashSet.Add(wingedEdge.edge))
			{
				wingedEdge = EdgeRingNext(wingedEdge);
				if (wingedEdge != null && wingedEdge.opposite != null)
				{
					wingedEdge = wingedEdge.opposite;
				}
			}
			wingedEdge = EdgeRingNext(value.opposite);
			if (wingedEdge != null && wingedEdge.opposite != null)
			{
				wingedEdge = wingedEdge.opposite;
			}
			while (wingedEdge != null && hashSet.Add(wingedEdge.edge))
			{
				wingedEdge = EdgeRingNext(wingedEdge);
				if (wingedEdge != null && wingedEdge.opposite != null)
				{
					wingedEdge = wingedEdge.opposite;
				}
			}
		}
		return hashSet.Select((EdgeLookup x) => x.local);
	}

	internal static IEnumerable<Edge> GetEdgeRingIterative(ProBuilderMesh pb, IEnumerable<Edge> edges)
	{
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(pb);
		List<EdgeLookup> source = EdgeLookup.GetEdgeLookup(edges, pb.sharedVertexLookup).ToList();
		source = source.Distinct().ToList();
		Dictionary<Edge, WingedEdge> dictionary = new Dictionary<Edge, WingedEdge>();
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			if (!dictionary.ContainsKey(wingedEdges[i].edge.common))
			{
				dictionary.Add(wingedEdges[i].edge.common, wingedEdges[i]);
			}
		}
		HashSet<EdgeLookup> hashSet = new HashSet<EdgeLookup>();
		int j = 0;
		for (int count = source.Count; j < count; j++)
		{
			if (dictionary.TryGetValue(source[j].common, out var value))
			{
				WingedEdge wingedEdge = value;
				if (!hashSet.Contains(wingedEdge.edge))
				{
					hashSet.Add(wingedEdge.edge);
				}
				WingedEdge wingedEdge2 = EdgeRingNext(wingedEdge);
				if (wingedEdge2 != null && wingedEdge2.opposite != null && !hashSet.Contains(wingedEdge2.edge))
				{
					hashSet.Add(wingedEdge2.edge);
				}
				WingedEdge wingedEdge3 = EdgeRingNext(wingedEdge.opposite);
				if (wingedEdge3 != null && wingedEdge3.opposite != null && !hashSet.Contains(wingedEdge3.edge))
				{
					hashSet.Add(wingedEdge3.edge);
				}
			}
		}
		return hashSet.Select((EdgeLookup x) => x.local);
	}

	internal static bool GetEdgeLoop(ProBuilderMesh mesh, IEnumerable<Edge> edges, out Edge[] loop)
	{
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh);
		HashSet<EdgeLookup> hashSet = new HashSet<EdgeLookup>(EdgeLookup.GetEdgeLookup(edges, mesh.sharedVertexLookup));
		HashSet<EdgeLookup> hashSet2 = new HashSet<EdgeLookup>();
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			if (!hashSet2.Contains(wingedEdges[i].edge) && hashSet.Contains(wingedEdges[i].edge) && !GetEdgeLoopInternal(wingedEdges[i], wingedEdges[i].edge.common.b, hashSet2))
			{
				GetEdgeLoopInternal(wingedEdges[i], wingedEdges[i].edge.common.a, hashSet2);
			}
		}
		loop = hashSet2.Select((EdgeLookup x) => x.local).ToArray();
		return true;
	}

	internal static bool GetEdgeLoopIterative(ProBuilderMesh mesh, IEnumerable<Edge> edges, out Edge[] loop)
	{
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh);
		HashSet<EdgeLookup> hashSet = new HashSet<EdgeLookup>(EdgeLookup.GetEdgeLookup(edges, mesh.sharedVertexLookup));
		HashSet<EdgeLookup> hashSet2 = new HashSet<EdgeLookup>();
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			if (hashSet.Contains(wingedEdges[i].edge))
			{
				GetEdgeLoopInternalIterative(wingedEdges[i], wingedEdges[i].edge.common, hashSet2);
			}
		}
		loop = hashSet2.Select((EdgeLookup x) => x.local).ToArray();
		return true;
	}

	private static bool GetEdgeLoopInternal(WingedEdge start, int startIndex, HashSet<EdgeLookup> used)
	{
		int num = startIndex;
		WingedEdge wingedEdge = start;
		do
		{
			used.Add(wingedEdge.edge);
			List<WingedEdge> list = GetSpokes(wingedEdge, num, allowHoles: true).DistinctBy((WingedEdge x) => x.edge.common).ToList();
			wingedEdge = null;
			if (list.Count == 4)
			{
				wingedEdge = list[2];
				num = ((wingedEdge.edge.common.a == num) ? wingedEdge.edge.common.b : wingedEdge.edge.common.a);
			}
		}
		while (wingedEdge != null && !used.Contains(wingedEdge.edge));
		return wingedEdge != null;
	}

	private static void GetEdgeLoopInternalIterative(WingedEdge start, Edge edge, HashSet<EdgeLookup> used)
	{
		int a = edge.a;
		int b = edge.b;
		WingedEdge wingedEdge = start;
		if (!used.Contains(wingedEdge.edge))
		{
			used.Add(wingedEdge.edge);
		}
		List<WingedEdge> list = GetSpokes(wingedEdge, a, allowHoles: true).DistinctBy((WingedEdge x) => x.edge.common).ToList();
		List<WingedEdge> list2 = GetSpokes(wingedEdge, b, allowHoles: true).DistinctBy((WingedEdge x) => x.edge.common).ToList();
		if (list.Count == 4)
		{
			wingedEdge = list[2];
			if (!used.Contains(wingedEdge.edge))
			{
				used.Add(wingedEdge.edge);
			}
		}
		if (list2.Count == 4)
		{
			wingedEdge = list2[2];
			if (!used.Contains(wingedEdge.edge))
			{
				used.Add(wingedEdge.edge);
			}
		}
	}

	private static WingedEdge NextSpoke(WingedEdge wing, int pivot, bool opp)
	{
		if (opp)
		{
			return wing.opposite;
		}
		if (wing.next.edge.common.Contains(pivot))
		{
			return wing.next;
		}
		if (wing.previous.edge.common.Contains(pivot))
		{
			return wing.previous;
		}
		return null;
	}

	internal static List<WingedEdge> GetSpokes(WingedEdge wing, int sharedIndex, bool allowHoles = false)
	{
		List<WingedEdge> list = new List<WingedEdge>();
		WingedEdge wingedEdge = wing;
		bool flag = false;
		do
		{
			if (list.Contains(wingedEdge))
			{
				return list;
			}
			list.Add(wingedEdge);
			wingedEdge = NextSpoke(wingedEdge, sharedIndex, flag);
			flag = !flag;
			if (wingedEdge != null && wingedEdge.edge.common.Equals(wing.edge.common))
			{
				return list;
			}
		}
		while (wingedEdge != null);
		if (!allowHoles)
		{
			return null;
		}
		wingedEdge = wing.opposite;
		flag = false;
		List<WingedEdge> list2 = new List<WingedEdge>();
		while (wingedEdge != null && !wingedEdge.edge.common.Equals(wing.edge.common))
		{
			list2.Add(wingedEdge);
			wingedEdge = NextSpoke(wingedEdge, sharedIndex, flag);
			flag = !flag;
		}
		list2.Reverse();
		list.AddRange(list2);
		return list;
	}

	public static HashSet<Face> GrowSelection(ProBuilderMesh mesh, IEnumerable<Face> faces, float maxAngleDiff = -1f)
	{
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh, oneWingPerFace: true);
		HashSet<Face> hashSet = new HashSet<Face>(faces);
		HashSet<Face> hashSet2 = new HashSet<Face>();
		Vector3 from = Vector3.zero;
		bool flag = maxAngleDiff > 0f;
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			if (!hashSet.Contains(wingedEdges[i].face))
			{
				continue;
			}
			if (flag)
			{
				from = Math.Normal(mesh, wingedEdges[i].face);
			}
			using WingedEdgeEnumerator wingedEdgeEnumerator = new WingedEdgeEnumerator(wingedEdges[i]);
			while (wingedEdgeEnumerator.MoveNext())
			{
				WingedEdge current = wingedEdgeEnumerator.Current;
				if (current.opposite == null || hashSet.Contains(current.opposite.face))
				{
					continue;
				}
				if (flag)
				{
					Vector3 to = Math.Normal(mesh, current.opposite.face);
					if (Vector3.Angle(from, to) < maxAngleDiff)
					{
						hashSet2.Add(current.opposite.face);
					}
				}
				else
				{
					hashSet2.Add(current.opposite.face);
				}
			}
		}
		return hashSet2;
	}

	internal static void Flood(WingedEdge wing, HashSet<Face> selection)
	{
		Flood(null, wing, Vector3_Zero, -1f, selection);
	}

	internal static void Flood(ProBuilderMesh pb, WingedEdge wing, Vector3 wingNrm, float maxAngle, HashSet<Face> selection)
	{
		WingedEdge wingedEdge = wing;
		do
		{
			WingedEdge opposite = wingedEdge.opposite;
			if (opposite != null && !selection.Contains(opposite.face))
			{
				if (maxAngle > 0f)
				{
					Vector3 vector = Math.Normal(pb, opposite.face);
					if (Vector3.Angle(wingNrm, vector) < maxAngle && selection.Add(opposite.face))
					{
						Flood(pb, opposite, vector, maxAngle, selection);
					}
				}
				else if (selection.Add(opposite.face))
				{
					Flood(pb, opposite, wingNrm, maxAngle, selection);
				}
			}
			wingedEdge = wingedEdge.next;
		}
		while (wingedEdge != wing);
	}

	public static HashSet<Face> FloodSelection(ProBuilderMesh mesh, IList<Face> faces, float maxAngleDiff)
	{
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh, oneWingPerFace: true);
		HashSet<Face> hashSet = new HashSet<Face>(faces);
		HashSet<Face> hashSet2 = new HashSet<Face>();
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			if (!hashSet2.Contains(wingedEdges[i].face) && hashSet.Contains(wingedEdges[i].face))
			{
				hashSet2.Add(wingedEdges[i].face);
				Flood(mesh, wingedEdges[i], (maxAngleDiff > 0f) ? Math.Normal(mesh, wingedEdges[i].face) : Vector3_Zero, maxAngleDiff, hashSet2);
			}
		}
		return hashSet2;
	}

	public static HashSet<Face> GetFaceLoop(ProBuilderMesh mesh, Face[] faces, bool ring = false)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (faces == null)
		{
			throw new ArgumentNullException("faces");
		}
		HashSet<Face> hashSet = new HashSet<Face>();
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh);
		foreach (Face face in faces)
		{
			hashSet.UnionWith(GetFaceLoop(wingedEdges, face, ring));
		}
		return hashSet;
	}

	public static HashSet<Face> GetFaceRingAndLoop(ProBuilderMesh mesh, Face[] faces)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (faces == null)
		{
			throw new ArgumentNullException("faces");
		}
		HashSet<Face> hashSet = new HashSet<Face>();
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh);
		foreach (Face face in faces)
		{
			hashSet.UnionWith(GetFaceLoop(wingedEdges, face, ring: true));
			hashSet.UnionWith(GetFaceLoop(wingedEdges, face, ring: false));
		}
		return hashSet;
	}

	private static HashSet<Face> GetFaceLoop(List<WingedEdge> wings, Face face, bool ring)
	{
		HashSet<Face> hashSet = new HashSet<Face>();
		if (face == null)
		{
			return hashSet;
		}
		WingedEdge wingedEdge = wings.FirstOrDefault((WingedEdge x) => x.face == face);
		if (wingedEdge == null)
		{
			return hashSet;
		}
		if (ring)
		{
			wingedEdge = wingedEdge.next ?? wingedEdge.previous;
		}
		for (int i = 0; i < 2; i++)
		{
			WingedEdge wingedEdge2 = wingedEdge;
			if (i == 1)
			{
				if (wingedEdge.opposite == null || wingedEdge.opposite.face == null)
				{
					break;
				}
				wingedEdge2 = wingedEdge.opposite;
			}
			while (hashSet.Add(wingedEdge2.face) && wingedEdge2.Count() == 4)
			{
				wingedEdge2 = wingedEdge2.next.next.opposite;
				if (wingedEdge2 == null || wingedEdge2.face == null)
				{
					break;
				}
			}
		}
		return hashSet;
	}

	internal static List<List<Edge>> FindHoles(ProBuilderMesh mesh, IEnumerable<int> indexes)
	{
		HashSet<int> sharedVertexHandles = mesh.GetSharedVertexHandles(indexes);
		List<List<Edge>> list = new List<List<Edge>>();
		foreach (List<WingedEdge> item in FindHoles(WingedEdge.GetWingedEdges(mesh), sharedVertexHandles))
		{
			list.Add(item.Select((WingedEdge x) => x.edge.local).ToList());
		}
		return list;
	}

	internal static List<List<WingedEdge>> FindHoles(List<WingedEdge> wings, HashSet<int> common)
	{
		HashSet<WingedEdge> hashSet = new HashSet<WingedEdge>();
		List<List<WingedEdge>> list = new List<List<WingedEdge>>();
		for (int i = 0; i < wings.Count; i++)
		{
			WingedEdge wingedEdge = wings[i];
			if (wingedEdge.opposite != null || hashSet.Contains(wingedEdge) || (!common.Contains(wingedEdge.edge.common.a) && !common.Contains(wingedEdge.edge.common.b)))
			{
				continue;
			}
			List<WingedEdge> list2 = new List<WingedEdge>();
			WingedEdge wingedEdge2 = wingedEdge;
			int num = wingedEdge2.edge.common.a;
			int num2 = 0;
			while (wingedEdge2 != null && num2++ < 2048)
			{
				hashSet.Add(wingedEdge2);
				list2.Add(wingedEdge2);
				num = ((wingedEdge2.edge.common.a == num) ? wingedEdge2.edge.common.b : wingedEdge2.edge.common.a);
				wingedEdge2 = FindNextEdgeInHole(wingedEdge2, num);
				if (wingedEdge2 == wingedEdge)
				{
					break;
				}
			}
			List<SimpleTuple<int, int>> list3 = new List<SimpleTuple<int, int>>();
			for (int j = 0; j < list2.Count; j++)
			{
				WingedEdge wingedEdge3 = list2[j];
				for (int num3 = j - 1; num3 > -1; num3--)
				{
					if (wingedEdge3.edge.common.b == list2[num3].edge.common.a)
					{
						list3.Add(new SimpleTuple<int, int>(num3, j));
						break;
					}
				}
			}
			int count = list3.Count;
			list3.Sort((SimpleTuple<int, int> x, SimpleTuple<int, int> y) => x.item1.CompareTo(y.item1));
			int[] array = new int[count];
			for (int num4 = count - 1; num4 > -1; num4--)
			{
				int item = list3[num4].item1;
				int num5 = list3[num4].item2 - array[num4] - item + 1;
				List<WingedEdge> range = list2.GetRange(item, num5);
				list2.RemoveRange(item, num5);
				for (int num6 = num4 - 1; num6 > -1; num6--)
				{
					if (list3[num6].item2 > list3[num4].item2)
					{
						array[num6] += num5;
					}
				}
				if (count < 2 || range.Any((WingedEdge w) => common.Contains(w.edge.common.a)) || range.Any((WingedEdge w) => common.Contains(w.edge.common.b)))
				{
					list.Add(range);
				}
			}
		}
		return list;
	}

	private static WingedEdge FindNextEdgeInHole(WingedEdge wing, int common)
	{
		WingedEdge adjacentEdgeWithCommonIndex = wing.GetAdjacentEdgeWithCommonIndex(common);
		int num = 0;
		while (adjacentEdgeWithCommonIndex != null && adjacentEdgeWithCommonIndex != wing && num++ < 2048)
		{
			if (adjacentEdgeWithCommonIndex.opposite == null)
			{
				return adjacentEdgeWithCommonIndex;
			}
			adjacentEdgeWithCommonIndex = adjacentEdgeWithCommonIndex.opposite.GetAdjacentEdgeWithCommonIndex(common);
		}
		return null;
	}
}
