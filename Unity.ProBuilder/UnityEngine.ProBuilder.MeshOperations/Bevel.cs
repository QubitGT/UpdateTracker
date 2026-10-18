using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class Bevel
{
	private static readonly int[] k_BridgeIndexesTri = new int[3] { 2, 1, 0 };

	public static List<Face> BevelEdges(ProBuilderMesh mesh, IList<Edge> edges, float amount)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		List<Vertex> list = new List<Vertex>(mesh.GetVertices());
		List<EdgeLookup> list2 = EdgeLookup.GetEdgeLookup(edges, sharedVertexLookup).Distinct().ToList();
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh);
		List<FaceRebuildData> list3 = new List<FaceRebuildData>();
		Dictionary<Face, List<int>> ignore = new Dictionary<Face, List<int>>();
		HashSet<int> hashSet = new HashSet<int>();
		int num = 0;
		Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>> dictionary = new Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>>();
		Dictionary<int, List<WingedEdge>> spokes = WingedEdge.GetSpokes(wingedEdges);
		HashSet<int> hashSet2 = new HashSet<int>();
		foreach (EdgeLookup item in list2)
		{
			if (hashSet2.Add(item.common.a))
			{
				foreach (WingedEdge item2 in spokes[item.common.a])
				{
					Edge local = item2.edge.local;
					amount = Mathf.Min(Vector3.Distance(list[local.a].position, list[local.b].position) - 0.001f, amount);
				}
			}
			if (!hashSet2.Add(item.common.b))
			{
				continue;
			}
			foreach (WingedEdge item3 in spokes[item.common.b])
			{
				Edge local2 = item3.edge.local;
				amount = Mathf.Min(Vector3.Distance(list[local2.a].position, list[local2.b].position) - 0.001f, amount);
			}
		}
		if (amount < 0.001f)
		{
			Log.Info("Bevel Distance > Available Surface");
			return null;
		}
		foreach (EdgeLookup lup in list2)
		{
			WingedEdge wingedEdge = wingedEdges.FirstOrDefault((WingedEdge x) => x.edge.Equals(lup));
			if (wingedEdge != null && wingedEdge.opposite != null)
			{
				num++;
				ignore.AddOrAppend(wingedEdge.face, wingedEdge.edge.common.a);
				ignore.AddOrAppend(wingedEdge.face, wingedEdge.edge.common.b);
				ignore.AddOrAppend(wingedEdge.opposite.face, wingedEdge.edge.common.a);
				ignore.AddOrAppend(wingedEdge.opposite.face, wingedEdge.edge.common.b);
				hashSet.Add(wingedEdge.edge.common.a);
				hashSet.Add(wingedEdge.edge.common.b);
				SlideEdge(list, wingedEdge, amount);
				SlideEdge(list, wingedEdge.opposite, amount);
				list3.AddRange(GetBridgeFaces(list, wingedEdge, wingedEdge.opposite, dictionary));
			}
		}
		if (num < 1)
		{
			Log.Info("Cannot Bevel Open Edges");
			return null;
		}
		List<Face> list4 = new List<Face>(list3.Select((FaceRebuildData x) => x.face));
		Dictionary<Face, List<SimpleTuple<WingedEdge, int>>> dictionary2 = new Dictionary<Face, List<SimpleTuple<WingedEdge, int>>>();
		foreach (int c in hashSet)
		{
			IEnumerable<WingedEdge> enumerable = wingedEdges.Where((WingedEdge x) => x.edge.common.Contains(c) && (!ignore.ContainsKey(x.face) || !ignore[x.face].Contains(c)));
			HashSet<Face> hashSet3 = new HashSet<Face>();
			foreach (WingedEdge item4 in enumerable)
			{
				if (hashSet3.Add(item4.face))
				{
					dictionary2.AddOrAppend(item4.face, new SimpleTuple<WingedEdge, int>(item4, c));
				}
			}
		}
		foreach (KeyValuePair<Face, List<SimpleTuple<WingedEdge, int>>> item5 in dictionary2)
		{
			Dictionary<int, List<int>> appendedVertices;
			FaceRebuildData faceRebuildData = VertexEditing.ExplodeVertex(list, item5.Value, amount, out appendedVertices);
			if (faceRebuildData == null)
			{
				continue;
			}
			list3.Add(faceRebuildData);
			foreach (KeyValuePair<int, List<int>> item6 in appendedVertices)
			{
				dictionary.AddOrAppend(item6.Key, new SimpleTuple<FaceRebuildData, List<int>>(faceRebuildData, item6.Value));
			}
		}
		FaceRebuildData.Apply(list3, mesh, list);
		int num2 = mesh.DeleteFaces(dictionary2.Keys).Length;
		mesh.sharedTextures = new SharedVertex[0];
		mesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(mesh.positionsInternal);
		SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
		sharedVertexLookup = mesh.sharedVertexLookup;
		List<HashSet<int>> list5 = new List<HashSet<int>>();
		foreach (KeyValuePair<int, List<SimpleTuple<FaceRebuildData, List<int>>>> item7 in dictionary)
		{
			if (item7.Value.Sum((SimpleTuple<FaceRebuildData, List<int>> x) => x.item2.Count) < 3)
			{
				continue;
			}
			HashSet<int> hashSet4 = new HashSet<int>();
			foreach (SimpleTuple<FaceRebuildData, List<int>> item8 in item7.Value)
			{
				int num3 = item8.item1.Offset() - num2;
				for (int i = 0; i < item8.item2.Count; i++)
				{
					hashSet4.Add(sharedVertexLookup[item8.item2[i] + num3]);
				}
			}
			list5.Add(hashSet4);
		}
		List<WingedEdge> wingedEdges2 = WingedEdge.GetWingedEdges(mesh, list3.Select((FaceRebuildData x) => x.face));
		list = new List<Vertex>(mesh.GetVertices());
		List<FaceRebuildData> list6 = new List<FaceRebuildData>();
		foreach (HashSet<int> item9 in list5)
		{
			if (item9.Count < 3)
			{
				continue;
			}
			if (item9.Count < 4)
			{
				List<Vertex> vertices = new List<Vertex>(mesh.GetVertices(item9.Select((int x) => sharedIndexes[x][0]).ToList()));
				list6.Add(AppendElements.FaceWithVertices(vertices));
				continue;
			}
			List<int> list7 = WingedEdge.SortCommonIndexesByAdjacency(wingedEdges2, item9);
			if (list7 != null)
			{
				List<Vertex> path = new List<Vertex>(mesh.GetVertices(list7.Select((int x) => sharedIndexes[x][0]).ToList()));
				list6.AddRange(AppendElements.TentCapWithVertices(path));
			}
		}
		FaceRebuildData.Apply(list6, mesh, list);
		mesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(mesh.positionsInternal);
		HashSet<Face> hashSet5 = new HashSet<Face>(list6.Select((FaceRebuildData x) => x.face));
		hashSet5.UnionWith(list4);
		list3.AddRange(list6);
		List<WingedEdge> wingedEdges3 = WingedEdge.GetWingedEdges(mesh, list3.Select((FaceRebuildData x) => x.face));
		for (int j = 0; j < wingedEdges3.Count; j++)
		{
			if (hashSet5.Count <= 0)
			{
				break;
			}
			WingedEdge wingedEdge2 = wingedEdges3[j];
			if (!hashSet5.Contains(wingedEdge2.face))
			{
				continue;
			}
			hashSet5.Remove(wingedEdge2.face);
			using WingedEdgeEnumerator wingedEdgeEnumerator = new WingedEdgeEnumerator(wingedEdge2);
			while (wingedEdgeEnumerator.MoveNext())
			{
				WingedEdge current7 = wingedEdgeEnumerator.Current;
				if (current7.opposite != null && !hashSet5.Contains(current7.opposite.face))
				{
					current7.face.submeshIndex = current7.opposite.face.submeshIndex;
					current7.face.uv = new AutoUnwrapSettings(current7.opposite.face.uv);
					SurfaceTopology.ConformOppositeNormal(current7.opposite);
					break;
				}
			}
		}
		mesh.ToMesh();
		return list4;
	}

	private static List<FaceRebuildData> GetBridgeFaces(IList<Vertex> vertices, WingedEdge left, WingedEdge right, Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>> holes)
	{
		List<FaceRebuildData> list = new List<FaceRebuildData>();
		FaceRebuildData faceRebuildData = new FaceRebuildData();
		EdgeLookup edge = left.edge;
		EdgeLookup edge2 = right.edge;
		faceRebuildData.vertices = new List<Vertex>
		{
			vertices[edge.local.a],
			vertices[edge.local.b],
			vertices[(edge.common.a == edge2.common.a) ? edge2.local.a : edge2.local.b],
			vertices[(edge.common.a == edge2.common.a) ? edge2.local.b : edge2.local.a]
		};
		Vector3 lhs = Math.Normal(vertices, left.face.indexesInternal);
		Vector3 rhs = Math.Normal(faceRebuildData.vertices, k_BridgeIndexesTri);
		int[] array = new int[6] { 2, 1, 0, 2, 3, 1 };
		if (Vector3.Dot(lhs, rhs) < 0f)
		{
			Array.Reverse(array);
		}
		faceRebuildData.face = new Face(array, left.face.submeshIndex, AutoUnwrapSettings.tile, -1, -1, -1, manualUVs: false);
		list.Add(faceRebuildData);
		holes.AddOrAppend(edge.common.a, new SimpleTuple<FaceRebuildData, List<int>>(faceRebuildData, new List<int> { 0, 2 }));
		holes.AddOrAppend(edge.common.b, new SimpleTuple<FaceRebuildData, List<int>>(faceRebuildData, new List<int> { 1, 3 }));
		return list;
	}

	private static void SlideEdge(IList<Vertex> vertices, WingedEdge we, float amount)
	{
		we.face.manualUV = true;
		we.face.textureGroup = -1;
		Edge leadingEdge = GetLeadingEdge(we, we.edge.common.a);
		Edge leadingEdge2 = GetLeadingEdge(we, we.edge.common.b);
		if (leadingEdge.IsValid() && leadingEdge2.IsValid())
		{
			Vertex vertex = vertices[leadingEdge.a] - vertices[leadingEdge.b];
			vertex.Normalize();
			Vertex vertex2 = vertices[leadingEdge2.a] - vertices[leadingEdge2.b];
			vertex2.Normalize();
			vertices[we.edge.local.a].Add(vertex * amount);
			vertices[we.edge.local.b].Add(vertex2 * amount);
		}
	}

	private static Edge GetLeadingEdge(WingedEdge wing, int common)
	{
		if (wing.previous.edge.common.a == common)
		{
			return new Edge(wing.previous.edge.local.b, wing.previous.edge.local.a);
		}
		if (wing.previous.edge.common.b == common)
		{
			return new Edge(wing.previous.edge.local.a, wing.previous.edge.local.b);
		}
		if (wing.next.edge.common.a == common)
		{
			return new Edge(wing.next.edge.local.b, wing.next.edge.local.a);
		}
		if (wing.next.edge.common.b == common)
		{
			return new Edge(wing.next.edge.local.a, wing.next.edge.local.b);
		}
		return Edge.Empty;
	}
}
