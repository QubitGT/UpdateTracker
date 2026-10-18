using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace g3;

public abstract class DGraph
{
	public struct EdgeSplitInfo
	{
		public int vNew;

		public int eNewBN;
	}

	public struct EdgeCollapseInfo
	{
		public int vKept;

		public int vRemoved;

		public int eCollapsed;
	}

	public enum FailMode
	{
		DebugAssert,
		gDevAssert,
		Throw,
		ReturnOnly
	}

	public const int InvalidID = -1;

	public const int DuplicateEdgeID = -2;

	public static readonly Index2i InvalidEdgeV = new Index2i(-1, -1);

	public static readonly Index3i InvalidEdge3 = new Index3i(-1, -1, -1);

	protected RefCountVector vertices_refcount;

	protected DVector<List<int>> vertex_edges;

	protected RefCountVector edges_refcount;

	protected DVector<int> edges;

	protected int timestamp;

	protected int shape_timestamp;

	protected int max_group_id;

	public int Timestamp => timestamp;

	public int ShapeTimestamp => shape_timestamp;

	public int VertexCount => vertices_refcount.count;

	public int EdgeCount => edges_refcount.count;

	public int MaxVertexID => vertices_refcount.max_index;

	public int MaxEdgeID => edges_refcount.max_index;

	public int MaxGroupID => max_group_id;

	public bool IsCompact
	{
		get
		{
			if (vertices_refcount.is_dense)
			{
				return edges_refcount.is_dense;
			}
			return false;
		}
	}

	public bool IsCompactV => vertices_refcount.is_dense;

	public DGraph()
	{
		vertex_edges = new DVector<List<int>>();
		vertices_refcount = new RefCountVector();
		edges = new DVector<int>();
		edges_refcount = new RefCountVector();
		max_group_id = 0;
	}

	protected void updateTimeStamp(bool bShapeChange)
	{
		timestamp++;
		if (bShapeChange)
		{
			shape_timestamp++;
		}
	}

	public bool IsVertex(int vID)
	{
		return vertices_refcount.isValid(vID);
	}

	public bool IsEdge(int eID)
	{
		return edges_refcount.isValid(eID);
	}

	public ReadOnlyCollection<int> GetVtxEdges(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return null;
		}
		return vertex_edges[vID].AsReadOnly();
	}

	public int GetVtxEdgeCount(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return -1;
		}
		return vertex_edges[vID].Count;
	}

	public int GetMaxVtxEdgeCount()
	{
		int num = 0;
		foreach (int item in vertices_refcount)
		{
			num = Math.Max(num, vertex_edges[item].Count);
		}
		return num;
	}

	public int GetEdgeGroup(int eid)
	{
		if (!edges_refcount.isValid(eid))
		{
			return -1;
		}
		return edges[3 * eid + 2];
	}

	public void SetEdgeGroup(int eid, int group_id)
	{
		if (edges_refcount.isValid(eid))
		{
			edges[3 * eid + 2] = group_id;
			max_group_id = Math.Max(max_group_id, group_id + 1);
			updateTimeStamp(bShapeChange: false);
		}
	}

	public int AllocateEdgeGroup()
	{
		return max_group_id++;
	}

	public Index2i GetEdgeV(int eID)
	{
		if (!edges_refcount.isValid(eID))
		{
			return InvalidEdgeV;
		}
		return new Index2i(edges[3 * eID], edges[3 * eID + 1]);
	}

	public Index3i GetEdge(int eID)
	{
		int num = 3 * eID;
		if (!edges_refcount.isValid(eID))
		{
			return InvalidEdge3;
		}
		return new Index3i(edges[num], edges[num + 1], edges[num + 2]);
	}

	protected int append_vertex_internal()
	{
		int num = vertices_refcount.allocate();
		vertex_edges.insert(new List<int>(), num);
		updateTimeStamp(bShapeChange: true);
		return num;
	}

	public int AppendEdge(int v0, int v1, int gid = -1)
	{
		return AppendEdge(new Index2i(v0, v1), gid);
	}

	public int AppendEdge(Index2i ev, int gid = -1)
	{
		if (!IsVertex(ev[0]) || !IsVertex(ev[1]))
		{
			return -1;
		}
		if (ev[0] == ev[1])
		{
			return -1;
		}
		if (FindEdge(ev[0], ev[1]) != -1)
		{
			return -2;
		}
		vertices_refcount.increment(ev[0], 1);
		vertices_refcount.increment(ev[1], 1);
		max_group_id = Math.Max(max_group_id, gid + 1);
		int result = add_edge(ev[0], ev[1], gid);
		updateTimeStamp(bShapeChange: true);
		return result;
	}

	protected int add_edge(int a, int b, int gid)
	{
		if (b < a)
		{
			int num = b;
			b = a;
			a = num;
		}
		int num2 = edges_refcount.allocate();
		int num3 = 3 * num2;
		edges.insert(a, num3);
		edges.insert(b, num3 + 1);
		edges.insert(gid, num3 + 2);
		vertex_edges[a].Add(num2);
		vertex_edges[b].Add(num2);
		return num2;
	}

	public IEnumerable<int> VertexIndices()
	{
		foreach (int item in vertices_refcount)
		{
			yield return item;
		}
	}

	public IEnumerable<int> EdgeIndices()
	{
		foreach (int item in edges_refcount)
		{
			yield return item;
		}
	}

	public IEnumerable<Index3i> Edges()
	{
		foreach (int item in edges_refcount)
		{
			int num2 = 3 * item;
			yield return new Index3i(edges[num2], edges[num2 + 1], edges[num2 + 2]);
		}
	}

	public IEnumerable<int> VtxVerticesItr(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			List<int> vedges = vertex_edges[vID];
			int N = vedges.Count;
			int i = 0;
			while (i < N)
			{
				yield return edge_other_v(vedges[i], vID);
				int num = i + 1;
				i = num;
			}
		}
	}

	public IEnumerable<int> VtxEdgesItr(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			List<int> vedges = vertex_edges[vID];
			int N = vedges.Count;
			int i = 0;
			while (i < N)
			{
				yield return vedges[i];
				int num = i + 1;
				i = num;
			}
		}
	}

	public int FindEdge(int vA, int vB)
	{
		int vid = Math.Max(vA, vB);
		List<int> list = vertex_edges[Math.Min(vA, vB)];
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (edge_has_v(list[i], vid))
			{
				return list[i];
			}
		}
		return -1;
	}

	public MeshResult RemoveEdge(int eID, bool bRemoveIsolatedVertices)
	{
		if (!edges_refcount.isValid(eID))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		int num = 3 * eID;
		Index2i index2i = new Index2i(edges[num], edges[num + 1]);
		vertex_edges[index2i.a].Remove(eID);
		vertex_edges[index2i.b].Remove(eID);
		edges_refcount.decrement(eID, 1);
		for (int i = 0; i < 2; i++)
		{
			int num2 = index2i[i];
			vertices_refcount.decrement(num2, 1);
			if (bRemoveIsolatedVertices && vertices_refcount.refCount(num2) == 1)
			{
				vertices_refcount.decrement(num2, 1);
				vertex_edges[num2] = null;
			}
		}
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	public MeshResult RemoveVertex(int vid, bool bRemoveIsolatedVertices)
	{
		foreach (int item in new List<int>(GetVtxEdges(vid)))
		{
			MeshResult meshResult = RemoveEdge(item, bRemoveIsolatedVertices);
			if (meshResult != 0)
			{
				return meshResult;
			}
		}
		return MeshResult.Ok;
	}

	public MeshResult SplitEdge(int vA, int vB, out EdgeSplitInfo split)
	{
		int num = FindEdge(vA, vB);
		if (num == -1)
		{
			split = default(EdgeSplitInfo);
			return MeshResult.Failed_NotAnEdge;
		}
		return SplitEdge(num, out split);
	}

	public MeshResult SplitEdge(int eab, out EdgeSplitInfo split)
	{
		split = default(EdgeSplitInfo);
		if (!IsEdge(eab))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		int num = 3 * eab;
		int a = edges[num];
		int num2 = edges[num + 1];
		int gid = edges[num + 2];
		int num3 = append_new_split_vertex(a, num2);
		replace_edge_vertex(eab, num2, num3);
		vertex_edges[num2].Remove(eab);
		vertex_edges[num3].Add(eab);
		int eNewBN = add_edge(num3, num2, gid);
		vertices_refcount.increment(num3, 2);
		split.vNew = num3;
		split.eNewBN = eNewBN;
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	protected virtual int append_new_split_vertex(int a, int b)
	{
		throw new NotImplementedException("DGraph2.append_new_split_vertex");
	}

	public MeshResult CollapseEdge(int vKeep, int vRemove, out EdgeCollapseInfo collapse)
	{
		bool flag = true;
		collapse = default(EdgeCollapseInfo);
		if (!IsVertex(vKeep) || !IsVertex(vRemove))
		{
			return MeshResult.Failed_NotAnEdge;
		}
		int num = FindEdge(vRemove, vKeep);
		if (num == -1)
		{
			return MeshResult.Failed_NotAnEdge;
		}
		List<int> list = vertex_edges[vKeep];
		List<int> list2 = vertex_edges[vRemove];
		bool flag2 = false;
		while (!flag2)
		{
			flag2 = true;
			foreach (int item in list2)
			{
				int num2 = edge_other_v(item, vRemove);
				if (num2 != vKeep && FindEdge(vKeep, num2) != -1)
				{
					RemoveEdge(item, flag);
					flag2 = false;
					break;
				}
			}
		}
		list.Remove(num);
		foreach (int item2 in list2)
		{
			if (edge_other_v(item2, vRemove) != vKeep)
			{
				replace_edge_vertex(item2, vRemove, vKeep);
				vertices_refcount.decrement(vRemove, 1);
				list.Add(item2);
				vertices_refcount.increment(vKeep, 1);
			}
		}
		edges_refcount.decrement(num, 1);
		vertices_refcount.decrement(vKeep, 1);
		vertices_refcount.decrement(vRemove, 1);
		if (flag)
		{
			vertices_refcount.decrement(vRemove, 1);
			vertex_edges[vRemove] = null;
		}
		list2.Clear();
		collapse.vKept = vKeep;
		collapse.vRemoved = vRemove;
		collapse.eCollapsed = num;
		updateTimeStamp(bShapeChange: true);
		return MeshResult.Ok;
	}

	protected bool edge_has_v(int eid, int vid)
	{
		int num = 3 * eid;
		if (edges[num] != vid)
		{
			return edges[num + 1] == vid;
		}
		return true;
	}

	protected int edge_other_v(int eID, int vID)
	{
		int num = 3 * eID;
		int num2 = edges[num];
		int num3 = edges[num + 1];
		if (num2 != vID)
		{
			if (num3 != vID)
			{
				return -1;
			}
			return num2;
		}
		return num3;
	}

	protected int replace_edge_vertex(int eID, int vOld, int vNew)
	{
		int num = 3 * eID;
		int num2 = edges[num];
		int num3 = edges[num + 1];
		if (num2 == vOld)
		{
			edges[num] = Math.Min(num3, vNew);
			edges[num + 1] = Math.Max(num3, vNew);
			return 0;
		}
		if (num3 == vOld)
		{
			edges[num] = Math.Min(num2, vNew);
			edges[num + 1] = Math.Max(num2, vNew);
			return 1;
		}
		return -1;
	}

	public bool IsBoundaryVertex(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			return vertex_edges[vID].Count == 1;
		}
		return false;
	}

	public bool IsJunctionVertex(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			return vertex_edges[vID].Count > 2;
		}
		return false;
	}

	public bool IsRegularVertex(int vID)
	{
		if (vertices_refcount.isValid(vID))
		{
			return vertex_edges[vID].Count == 2;
		}
		return false;
	}

	public virtual bool CheckValidity(FailMode eFailMode = FailMode.Throw)
	{
		bool is_ok = true;
		Action<bool> action = delegate(bool b)
		{
			is_ok &= b;
		};
		switch (eFailMode)
		{
		case FailMode.DebugAssert:
			action = delegate(bool b)
			{
				is_ok &= b;
			};
			break;
		case FailMode.gDevAssert:
			action = delegate(bool b)
			{
				is_ok &= b;
			};
			break;
		case FailMode.Throw:
			action = delegate(bool b)
			{
				if (!b)
				{
					throw new Exception("DGraph3.CheckValidity: check failed");
				}
			};
			break;
		}
		foreach (int item in EdgeIndices())
		{
			action(IsEdge(item));
			action(edges_refcount.refCount(item) == 1);
			Index2i edgeV = GetEdgeV(item);
			action(IsVertex(edgeV[0]));
			action(IsVertex(edgeV[1]));
			action(edgeV[0] < edgeV[1]);
		}
		if (vertices_refcount.is_dense)
		{
			for (int i = 0; i < VertexCount; i++)
			{
				action(vertices_refcount.isValid(i));
			}
		}
		foreach (int item2 in VertexIndices())
		{
			action(IsVertex(item2));
			List<int> list = vertex_edges[item2];
			foreach (int item3 in list)
			{
				action(IsEdge(item3));
				action(edge_has_v(item3, item2));
				int num = edge_other_v(item3, item2);
				int num2 = FindEdge(item2, num);
				action(num2 != -1);
				action(num2 == item3);
				num2 = FindEdge(num, item2);
				action(num2 != -1);
				action(num2 == item3);
			}
			action(vertices_refcount.refCount(item2) == list.Count + 1);
		}
		subclass_validity_checks(action);
		return is_ok;
	}

	protected virtual void subclass_validity_checks(Action<bool> CheckOrFailF)
	{
	}

	[Conditional("DEBUG")]
	public void debug_check_is_vertex(int v)
	{
		if (!IsVertex(v))
		{
			throw new Exception("DGraph.debug_is_vertex - not a vertex!");
		}
	}

	[Conditional("DEBUG")]
	public void debug_check_is_edge(int e)
	{
		if (!IsEdge(e))
		{
			throw new Exception("DGraph.debug_is_edge - not an edge!");
		}
	}
}
