using System;
using System.Collections.Generic;

namespace g3;

public class DijkstraGraphDistance
{
	private class GraphNode : DynamicPriorityQueueNode, IEquatable<GraphNode>
	{
		public int id;

		public GraphNode parent;

		public bool frozen;

		public bool Equals(GraphNode other)
		{
			return id == other.id;
		}
	}

	private struct GraphNodeStruct : IEquatable<GraphNodeStruct>
	{
		public int id;

		public int parent;

		public bool frozen;

		public float distance;

		public static readonly GraphNodeStruct Zero = new GraphNodeStruct
		{
			id = -1,
			parent = -1,
			distance = float.MaxValue,
			frozen = false
		};

		public GraphNodeStruct(int id, int parent, float distance)
		{
			this.id = id;
			this.parent = parent;
			this.distance = distance;
			frozen = false;
		}

		public bool Equals(GraphNodeStruct other)
		{
			return id == other.id;
		}
	}

	public const float InvalidValue = float.MaxValue;

	public bool TrackOrder;

	private DynamicPriorityQueue<GraphNode> SparseQueue;

	private SparseObjectList<GraphNode> SparseNodes;

	private MemoryPool<GraphNode> SparseNodePool;

	private IndexPriorityQueue DenseQueue;

	private GraphNodeStruct[] DenseNodes;

	private Func<int, bool> NodeFilterF;

	private Func<int, int, float> NodeDistanceF;

	private Func<int, IEnumerable<int>> NeighboursF;

	private List<int> Seeds;

	private float max_value;

	private List<int> order;

	public float MaxDistance => max_value;

	public DijkstraGraphDistance(int nMaxID, bool bSparse, Func<int, bool> nodeFilterF, Func<int, int, float> nodeDistanceF, Func<int, IEnumerable<int>> neighboursF, IEnumerable<Vector2d> seeds = null)
	{
		NodeFilterF = nodeFilterF;
		NodeDistanceF = nodeDistanceF;
		NeighboursF = neighboursF;
		if (bSparse)
		{
			SparseQueue = new DynamicPriorityQueue<GraphNode>();
			SparseNodes = new SparseObjectList<GraphNode>(nMaxID, 0);
			SparseNodePool = new MemoryPool<GraphNode>();
		}
		else
		{
			DenseQueue = new IndexPriorityQueue(nMaxID);
			DenseNodes = new GraphNodeStruct[nMaxID];
		}
		Seeds = new List<int>();
		max_value = float.MinValue;
		if (seeds == null)
		{
			return;
		}
		foreach (Vector2d seed in seeds)
		{
			AddSeed((int)seed.x, (float)seed.y);
		}
	}

	public static DijkstraGraphDistance MeshVertices(DMesh3 mesh, bool bSparse = false)
	{
		if (!bSparse)
		{
			return new DijkstraGraphDistance(mesh.MaxVertexID, bSparse: false, (int id) => true, (int a, int b) => (float)mesh.GetVertex(a).Distance(mesh.GetVertex(b)), mesh.VtxVerticesItr);
		}
		return new DijkstraGraphDistance(mesh.MaxVertexID, bSparse: true, (int id) => mesh.IsVertex(id), (int a, int b) => (float)mesh.GetVertex(a).Distance(mesh.GetVertex(b)), mesh.VtxVerticesItr);
	}

	public static DijkstraGraphDistance MeshTriangles(DMesh3 mesh, bool bSparse = false)
	{
		Func<int, int, float> nodeDistanceF = (int a, int b) => (float)mesh.GetTriCentroid(a).Distance(mesh.GetTriCentroid(b));
		if (!bSparse)
		{
			return new DijkstraGraphDistance(mesh.MaxTriangleID, bSparse: false, (int id) => true, nodeDistanceF, mesh.TriTrianglesItr);
		}
		return new DijkstraGraphDistance(mesh.MaxTriangleID, bSparse: true, (int id) => mesh.IsTriangle(id), nodeDistanceF, mesh.TriTrianglesItr);
	}

	public void Reset()
	{
		if (SparseNodes != null)
		{
			SparseQueue.Clear(bFreeMemory: false);
			SparseNodes.Clear();
			SparseNodePool.ReturnAll();
		}
		else
		{
			DenseQueue.Clear(bFreeMemory: false);
			Array.Clear(DenseNodes, 0, DenseNodes.Length);
		}
		Seeds = new List<int>();
		max_value = float.MinValue;
	}

	public void AddSeed(int id, float seed_dist)
	{
		if (SparseNodes != null)
		{
			GraphNode node = get_node(id);
			SparseQueue.Enqueue(node, seed_dist);
		}
		else
		{
			enqueue_node_dense(id, seed_dist, -1);
		}
		Seeds.Add(id);
	}

	public bool IsSeed(int id)
	{
		return Seeds.Contains(id);
	}

	public void Compute()
	{
		if (TrackOrder)
		{
			order = new List<int>();
		}
		if (SparseNodes != null)
		{
			Compute_Sparse();
		}
		else
		{
			Compute_Dense();
		}
	}

	protected void Compute_Sparse()
	{
		while (SparseQueue.Count > 0)
		{
			GraphNode graphNode = SparseQueue.Dequeue();
			max_value = Math.Max(graphNode.priority, max_value);
			graphNode.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNode.id);
			}
			update_neighbours_sparse(graphNode);
		}
	}

	protected void Compute_Dense()
	{
		while (DenseQueue.Count > 0)
		{
			float firstPriority = DenseQueue.FirstPriority;
			int num = DenseQueue.Dequeue();
			GraphNodeStruct graphNodeStruct = DenseNodes[num];
			graphNodeStruct.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNodeStruct.id);
			}
			graphNodeStruct.distance = max_value;
			DenseNodes[num] = graphNodeStruct;
			max_value = Math.Max(firstPriority, max_value);
			update_neighbours_dense(graphNodeStruct.id);
		}
	}

	public void ComputeToMaxDistance(float fMaxDistance)
	{
		if (TrackOrder)
		{
			order = new List<int>();
		}
		if (SparseNodes != null)
		{
			ComputeToMaxDistance_Sparse(fMaxDistance);
		}
		else
		{
			ComputeToMaxDistance_Dense(fMaxDistance);
		}
	}

	protected void ComputeToMaxDistance_Sparse(float fMaxDistance)
	{
		while (SparseQueue.Count > 0)
		{
			GraphNode graphNode = SparseQueue.Dequeue();
			max_value = Math.Max(graphNode.priority, max_value);
			if (max_value > fMaxDistance)
			{
				break;
			}
			graphNode.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNode.id);
			}
			update_neighbours_sparse(graphNode);
		}
	}

	protected void ComputeToMaxDistance_Dense(float fMaxDistance)
	{
		while (DenseQueue.Count > 0)
		{
			float firstPriority = DenseQueue.FirstPriority;
			max_value = Math.Max(firstPriority, max_value);
			if (max_value > fMaxDistance)
			{
				break;
			}
			int num = DenseQueue.Dequeue();
			GraphNodeStruct graphNodeStruct = DenseNodes[num];
			graphNodeStruct.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNodeStruct.id);
			}
			graphNodeStruct.distance = max_value;
			DenseNodes[num] = graphNodeStruct;
			update_neighbours_dense(graphNodeStruct.id);
		}
	}

	public void ComputeToNode(int node_id, float fMaxDistance = float.MaxValue)
	{
		if (TrackOrder)
		{
			order = new List<int>();
		}
		if (SparseNodes != null)
		{
			ComputeToNode_Sparse(node_id, fMaxDistance);
		}
		else
		{
			ComputeToNode_Dense(node_id, fMaxDistance);
		}
	}

	protected void ComputeToNode_Sparse(int node_id, float fMaxDistance)
	{
		while (SparseQueue.Count > 0)
		{
			GraphNode graphNode = SparseQueue.Dequeue();
			max_value = Math.Max(graphNode.priority, max_value);
			if (max_value > fMaxDistance)
			{
				break;
			}
			graphNode.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNode.id);
			}
			if (graphNode.id == node_id)
			{
				break;
			}
			update_neighbours_sparse(graphNode);
		}
	}

	protected void ComputeToNode_Dense(int node_id, float fMaxDistance)
	{
		while (DenseQueue.Count > 0)
		{
			float firstPriority = DenseQueue.FirstPriority;
			max_value = Math.Max(firstPriority, max_value);
			if (max_value > fMaxDistance)
			{
				break;
			}
			int num = DenseQueue.Dequeue();
			GraphNodeStruct graphNodeStruct = DenseNodes[num];
			graphNodeStruct.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNodeStruct.id);
			}
			graphNodeStruct.distance = max_value;
			DenseNodes[num] = graphNodeStruct;
			if (graphNodeStruct.id == node_id)
			{
				break;
			}
			update_neighbours_dense(graphNodeStruct.id);
		}
	}

	public int ComputeToNode(Func<int, bool> terminatingNodeF, float fMaxDistance = float.MaxValue)
	{
		if (TrackOrder)
		{
			order = new List<int>();
		}
		if (SparseNodes != null)
		{
			return ComputeToNode_Sparse(terminatingNodeF, fMaxDistance);
		}
		return ComputeToNode_Dense(terminatingNodeF, fMaxDistance);
	}

	protected int ComputeToNode_Sparse(Func<int, bool> terminatingNodeF, float fMaxDistance)
	{
		while (SparseQueue.Count > 0)
		{
			GraphNode graphNode = SparseQueue.Dequeue();
			max_value = Math.Max(graphNode.priority, max_value);
			if (max_value > fMaxDistance)
			{
				return -1;
			}
			graphNode.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNode.id);
			}
			if (terminatingNodeF(graphNode.id))
			{
				return graphNode.id;
			}
			update_neighbours_sparse(graphNode);
		}
		return -1;
	}

	protected int ComputeToNode_Dense(Func<int, bool> terminatingNodeF, float fMaxDistance)
	{
		while (DenseQueue.Count > 0)
		{
			float firstPriority = DenseQueue.FirstPriority;
			max_value = Math.Max(firstPriority, max_value);
			if (max_value > fMaxDistance)
			{
				return -1;
			}
			int num = DenseQueue.Dequeue();
			GraphNodeStruct graphNodeStruct = DenseNodes[num];
			graphNodeStruct.frozen = true;
			if (TrackOrder)
			{
				order.Add(graphNodeStruct.id);
			}
			graphNodeStruct.distance = max_value;
			DenseNodes[num] = graphNodeStruct;
			if (terminatingNodeF(graphNodeStruct.id))
			{
				return graphNodeStruct.id;
			}
			update_neighbours_dense(graphNodeStruct.id);
		}
		return -1;
	}

	public float GetDistance(int id)
	{
		if (SparseNodes != null)
		{
			return SparseNodes[id]?.priority ?? float.MaxValue;
		}
		GraphNodeStruct graphNodeStruct = DenseNodes[id];
		if (!graphNodeStruct.frozen)
		{
			return float.MaxValue;
		}
		return graphNodeStruct.distance;
	}

	public List<int> GetOrder()
	{
		if (!TrackOrder)
		{
			throw new InvalidOperationException("DijkstraGraphDistance.GetOrder: Must set TrackOrder = true");
		}
		return order;
	}

	public bool GetPathToSeed(int fromv, List<int> path)
	{
		if (SparseNodes != null)
		{
			GraphNode graphNode = get_node(fromv);
			if (!graphNode.frozen)
			{
				return false;
			}
			path.Add(fromv);
			while (graphNode.parent != null)
			{
				path.Add(graphNode.parent.id);
				graphNode = graphNode.parent;
			}
			return true;
		}
		GraphNodeStruct graphNodeStruct = DenseNodes[fromv];
		if (!graphNodeStruct.frozen)
		{
			return false;
		}
		path.Add(fromv);
		while (graphNodeStruct.parent != -1)
		{
			path.Add(graphNodeStruct.parent);
			graphNodeStruct = DenseNodes[graphNodeStruct.parent];
		}
		return true;
	}

	private GraphNode get_node(int id)
	{
		GraphNode graphNode = SparseNodes[id];
		if (graphNode == null)
		{
			graphNode = SparseNodePool.Allocate();
			graphNode.id = id;
			graphNode.parent = null;
			graphNode.frozen = false;
			SparseNodes[id] = graphNode;
		}
		return graphNode;
	}

	private void update_neighbours_sparse(GraphNode parent)
	{
		float priority = parent.priority;
		foreach (int item in NeighboursF(parent.id))
		{
			if (!NodeFilterF(item))
			{
				continue;
			}
			GraphNode graphNode = get_node(item);
			if (graphNode.frozen)
			{
				continue;
			}
			float num = NodeDistanceF(parent.id, item) + priority;
			if (num == float.MaxValue)
			{
				continue;
			}
			if (SparseQueue.Contains(graphNode))
			{
				if (num < graphNode.priority)
				{
					graphNode.parent = parent;
					SparseQueue.Update(graphNode, num);
				}
			}
			else
			{
				graphNode.parent = parent;
				SparseQueue.Enqueue(graphNode, num);
			}
		}
	}

	private void enqueue_node_dense(int id, float dist, int parent_id)
	{
		GraphNodeStruct graphNodeStruct = new GraphNodeStruct(id, parent_id, dist);
		DenseNodes[id] = graphNodeStruct;
		DenseQueue.Insert(id, dist);
	}

	private void update_neighbours_dense(int parent_id)
	{
		float distance = DenseNodes[parent_id].distance;
		foreach (int item in NeighboursF(parent_id))
		{
			if (!NodeFilterF(item))
			{
				continue;
			}
			GraphNodeStruct graphNodeStruct = DenseNodes[item];
			if (graphNodeStruct.frozen)
			{
				continue;
			}
			float num = NodeDistanceF(parent_id, item) + distance;
			if (num == float.MaxValue)
			{
				continue;
			}
			if (DenseQueue.Contains(item))
			{
				if (num < graphNodeStruct.distance)
				{
					graphNodeStruct.parent = parent_id;
					DenseQueue.Update(item, num);
					DenseNodes[item] = graphNodeStruct;
				}
			}
			else
			{
				enqueue_node_dense(item, num, parent_id);
			}
		}
	}
}
