using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class SurfaceTopology
{
	public static Face[] ToTriangles(this ProBuilderMesh mesh, IList<Face> faces)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (faces == null)
		{
			throw new ArgumentNullException("faces");
		}
		List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		List<FaceRebuildData> list = new List<FaceRebuildData>();
		foreach (Face face in faces)
		{
			List<FaceRebuildData> collection = BreakFaceIntoTris(face, vertices, sharedVertexLookup);
			list.AddRange(collection);
		}
		FaceRebuildData.Apply(list, mesh, vertices);
		mesh.DeleteFaces(faces);
		mesh.ToMesh();
		return list.Select((FaceRebuildData x) => x.face).ToArray();
	}

	private static List<FaceRebuildData> BreakFaceIntoTris(Face face, List<Vertex> vertices, Dictionary<int, int> lookup)
	{
		int[] indexesInternal = face.indexesInternal;
		int num = indexesInternal.Length;
		List<FaceRebuildData> list = new List<FaceRebuildData>(num / 3);
		for (int i = 0; i < num; i += 3)
		{
			FaceRebuildData faceRebuildData = new FaceRebuildData();
			faceRebuildData.face = new Face(face);
			faceRebuildData.face.indexesInternal = new int[3] { 0, 1, 2 };
			faceRebuildData.vertices = new List<Vertex>
			{
				vertices[indexesInternal[i]],
				vertices[indexesInternal[i + 1]],
				vertices[indexesInternal[i + 2]]
			};
			faceRebuildData.sharedIndexes = new List<int>
			{
				lookup[indexesInternal[i]],
				lookup[indexesInternal[i + 1]],
				lookup[indexesInternal[i + 2]]
			};
			list.Add(faceRebuildData);
		}
		return list;
	}

	public static WindingOrder GetWindingOrder(this ProBuilderMesh mesh, Face face)
	{
		return GetWindingOrder(Projection.PlanarProject(mesh.positionsInternal, face.distinctIndexesInternal));
	}

	private static WindingOrder GetWindingOrder(IList<Vertex> vertices, IList<int> indexes)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		return GetWindingOrder(Projection.PlanarProject(vertices.Select((Vertex x) => x.position).ToArray(), indexes));
	}

	public static WindingOrder GetWindingOrder(IList<Vector2> points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		float num = 0f;
		int count = points.Count;
		for (int i = 0; i < count; i++)
		{
			Vector2 vector = points[i];
			Vector2 vector2 = ((i < count - 1) ? points[i + 1] : points[0]);
			num += (vector2.x - vector.x) * (vector2.y + vector.y);
		}
		if (num != 0f)
		{
			if (!(num > 0f))
			{
				return WindingOrder.CounterClockwise;
			}
			return WindingOrder.Clockwise;
		}
		return WindingOrder.Unknown;
	}

	public static bool FlipEdge(this ProBuilderMesh mesh, Face face)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (face == null)
		{
			throw new ArgumentNullException("face");
		}
		int[] indexesInternal = face.indexesInternal;
		if (indexesInternal.Length != 6)
		{
			return false;
		}
		int[] array = ArrayUtility.Fill(1, indexesInternal.Length);
		for (int i = 0; i < indexesInternal.Length - 1; i++)
		{
			for (int j = i + 1; j < indexesInternal.Length; j++)
			{
				if (indexesInternal[i] == indexesInternal[j])
				{
					array[i]++;
					array[j]++;
				}
			}
		}
		if (array[0] + array[1] + array[2] != 5 || array[3] + array[4] + array[5] != 5)
		{
			return false;
		}
		int num = indexesInternal[(array[0] != 1) ? ((array[1] == 1) ? 1 : 2) : 0];
		int num2 = indexesInternal[(array[3] == 1) ? 3 : ((array[4] == 1) ? 4 : 5)];
		int num3 = -1;
		if (array[0] == 2)
		{
			num3 = indexesInternal[0];
			indexesInternal[0] = num2;
		}
		else if (array[1] == 2)
		{
			num3 = indexesInternal[1];
			indexesInternal[1] = num2;
		}
		else if (array[2] == 2)
		{
			num3 = indexesInternal[2];
			indexesInternal[2] = num2;
		}
		if (array[3] == 2 && indexesInternal[3] != num3)
		{
			indexesInternal[3] = num;
		}
		else if (array[4] == 2 && indexesInternal[4] != num3)
		{
			indexesInternal[4] = num;
		}
		else if (array[5] == 2 && indexesInternal[5] != num3)
		{
			indexesInternal[5] = num;
		}
		face.InvalidateCache();
		return true;
	}

	public static ActionResult ConformNormals(this ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh, faces);
		HashSet<Face> hashSet = new HashSet<Face>();
		int num = 0;
		for (int i = 0; i < wingedEdges.Count; i++)
		{
			if (hashSet.Contains(wingedEdges[i].face))
			{
				continue;
			}
			Dictionary<Face, bool> dictionary = new Dictionary<Face, bool>();
			GetWindingFlags(wingedEdges[i], flag: true, dictionary);
			int num2 = 0;
			foreach (KeyValuePair<Face, bool> item in dictionary)
			{
				num2 += (item.Value ? 1 : (-1));
			}
			bool flag = num2 > 0;
			foreach (KeyValuePair<Face, bool> item2 in dictionary)
			{
				if (flag != item2.Value)
				{
					num++;
					item2.Key.Reverse();
				}
			}
			hashSet.UnionWith(dictionary.Keys);
		}
		if (num > 0)
		{
			return new ActionResult(ActionResult.Status.Success, (num > 1) ? $"Flipped {num} faces" : "Flipped 1 face");
		}
		return new ActionResult(ActionResult.Status.NoChange, "Faces Uniform");
	}

	private static void GetWindingFlags(WingedEdge edge, bool flag, Dictionary<Face, bool> flags)
	{
		flags.Add(edge.face, flag);
		WingedEdge wingedEdge = edge;
		do
		{
			WingedEdge opposite = wingedEdge.opposite;
			if (opposite != null && !flags.ContainsKey(opposite.face))
			{
				Edge commonEdgeInWindingOrder = GetCommonEdgeInWindingOrder(wingedEdge);
				Edge commonEdgeInWindingOrder2 = GetCommonEdgeInWindingOrder(opposite);
				GetWindingFlags(opposite, (commonEdgeInWindingOrder.a == commonEdgeInWindingOrder2.a) ? (!flag) : flag, flags);
			}
			wingedEdge = wingedEdge.next;
		}
		while (wingedEdge != edge);
	}

	internal static ActionResult ConformOppositeNormal(WingedEdge source)
	{
		if (source == null || source.opposite == null)
		{
			return new ActionResult(ActionResult.Status.Failure, "Source edge does not share an edge with another face.");
		}
		Edge commonEdgeInWindingOrder = GetCommonEdgeInWindingOrder(source);
		Edge commonEdgeInWindingOrder2 = GetCommonEdgeInWindingOrder(source.opposite);
		if (commonEdgeInWindingOrder.a == commonEdgeInWindingOrder2.a)
		{
			source.opposite.face.Reverse();
			return new ActionResult(ActionResult.Status.Success, "Reversed target face winding order.");
		}
		return new ActionResult(ActionResult.Status.NoChange, "Faces already unified.");
	}

	private static Edge GetCommonEdgeInWindingOrder(WingedEdge wing)
	{
		int[] indexesInternal = wing.face.indexesInternal;
		int num = indexesInternal.Length;
		for (int i = 0; i < num; i += 3)
		{
			Edge local = wing.edge.local;
			int num2 = indexesInternal[i];
			int num3 = indexesInternal[i + 1];
			int num4 = indexesInternal[i + 2];
			if (local.a == num2 && local.b == num3)
			{
				return wing.edge.common;
			}
			if (local.a == num3 && local.b == num2)
			{
				return new Edge(wing.edge.common.b, wing.edge.common.a);
			}
			if (local.a == num3 && local.b == num4)
			{
				return wing.edge.common;
			}
			if (local.a == num4 && local.b == num3)
			{
				return new Edge(wing.edge.common.b, wing.edge.common.a);
			}
			if (local.a == num4 && local.b == num2)
			{
				return wing.edge.common;
			}
			if (local.a == num2 && local.b == num4)
			{
				return new Edge(wing.edge.common.b, wing.edge.common.a);
			}
		}
		return Edge.Empty;
	}

	internal static void MatchNormal(Face source, Face target, Dictionary<int, int> lookup)
	{
		List<EdgeLookup> list = EdgeLookup.GetEdgeLookup(source.edgesInternal, lookup).ToList();
		List<EdgeLookup> list2 = EdgeLookup.GetEdgeLookup(target.edgesInternal, lookup).ToList();
		bool flag = false;
		int num = 0;
		while (!flag && num < list.Count)
		{
			Edge common = list[num].common;
			int num2 = 0;
			while (!flag && num2 < list2.Count)
			{
				Edge common2 = list2[num2].common;
				if (common.Equals(common2))
				{
					if (common.a == common2.a)
					{
						target.Reverse();
					}
					flag = true;
				}
				num2++;
			}
			num++;
		}
	}
}
