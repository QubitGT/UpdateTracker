using System;
using System.Collections.Generic;

namespace g3;

public class MeshLocalParam
{
	public enum UVModes
	{
		ExponentialMap,
		ExponentialMap_UpwindAvg,
		PlanarProjection
	}

	private class GraphNode : DynamicPriorityQueueNode, IEquatable<GraphNode>
	{
		public int id;

		public GraphNode parent;

		public float graph_distance;

		public Vector2f uv;

		public bool frozen;

		public bool Equals(GraphNode other)
		{
			return id == other.id;
		}
	}

	public static readonly Vector2f InvalidUV = new Vector2f(float.MaxValue, float.MaxValue);

	public UVModes UVMode = UVModes.ExponentialMap_UpwindAvg;

	private DynamicPriorityQueue<GraphNode> SparseQueue;

	private SparseObjectList<GraphNode> SparseNodes;

	private MemoryPool<GraphNode> SparseNodePool;

	private Func<int, Vector3f> PositionF;

	private Func<int, Vector3f> NormalF;

	private Func<int, IEnumerable<int>> NeighboursF;

	private Frame3f SeedFrame;

	private float max_graph_distance;

	private float max_uv_distance;

	public float MaxGraphDistance => max_graph_distance;

	public float MaxUVDistance => max_uv_distance;

	public MeshLocalParam(int nMaxID, Func<int, Vector3f> nodePositionF, Func<int, Vector3f> nodeNormalF, Func<int, IEnumerable<int>> neighboursF)
	{
		PositionF = nodePositionF;
		NormalF = nodeNormalF;
		NeighboursF = neighboursF;
		SparseQueue = new DynamicPriorityQueue<GraphNode>();
		SparseNodes = new SparseObjectList<GraphNode>(nMaxID, 0);
		SparseNodePool = new MemoryPool<GraphNode>();
		max_graph_distance = float.MinValue;
		max_uv_distance = float.MinValue;
	}

	public void Reset()
	{
		SparseQueue.Clear(bFreeMemory: false);
		SparseNodes.Clear();
		SparseNodePool.ReturnAll();
		max_graph_distance = float.MinValue;
	}

	public void ComputeToMaxDistance(Frame3f seedFrame, Index3i seedNbrs, float fMaxGraphDistance)
	{
		SeedFrame = seedFrame;
		for (int i = 0; i < 3; i++)
		{
			int num = seedNbrs[i];
			GraphNode graphNode = get_node(num);
			graphNode.uv = compute_local_uv(ref SeedFrame, PositionF(num));
			graphNode.graph_distance = graphNode.uv.Length;
			graphNode.frozen = true;
			SparseQueue.Enqueue(graphNode, graphNode.graph_distance);
		}
		while (SparseQueue.Count > 0)
		{
			GraphNode graphNode2 = SparseQueue.Dequeue();
			max_graph_distance = Math.Max(graphNode2.graph_distance, max_graph_distance);
			if (max_graph_distance > fMaxGraphDistance)
			{
				return;
			}
			if (graphNode2.parent != null)
			{
				switch (UVMode)
				{
				case UVModes.ExponentialMap:
					update_uv_expmap(graphNode2);
					break;
				case UVModes.ExponentialMap_UpwindAvg:
					update_uv_upwind_expmap(graphNode2);
					break;
				case UVModes.PlanarProjection:
					update_uv_planar(graphNode2);
					break;
				}
			}
			float lengthSquared = graphNode2.uv.LengthSquared;
			if (lengthSquared > max_uv_distance)
			{
				max_uv_distance = lengthSquared;
			}
			graphNode2.frozen = true;
			update_neighbours_sparse(graphNode2);
		}
		max_uv_distance = (float)Math.Sqrt(max_uv_distance);
	}

	public void TransformUV(float fScale, Vector2f vTranslate)
	{
		foreach (KeyValuePair<int, GraphNode> item in SparseNodes.NonZeroValues())
		{
			GraphNode value = item.Value;
			if (value.frozen)
			{
				value.uv = value.uv * fScale + vTranslate;
			}
		}
	}

	public Vector2f GetUV(int id)
	{
		return SparseNodes[id]?.uv ?? InvalidUV;
	}

	public void ApplyUVs(Action<int, Vector2f> applyF)
	{
		foreach (KeyValuePair<int, GraphNode> item in SparseNodes.NonZeroValues())
		{
			GraphNode value = item.Value;
			if (value.frozen)
			{
				applyF(value.id, value.uv);
			}
		}
	}

	private Vector2f compute_local_uv(ref Frame3f f, Vector3f pos)
	{
		pos -= f.Origin;
		return new Vector2f(pos.Dot(f.X), pos.Dot(f.Y));
	}

	private Vector2f propagate_uv(Vector3f pos, Vector2f nbrUV, ref Frame3f fNbr, ref Frame3f fSeed)
	{
		Vector2f vector2f = compute_local_uv(ref fNbr, pos);
		Frame3f frame3f = fSeed;
		frame3f.AlignAxis(2, fNbr.Z);
		Vector3f x = frame3f.X;
		Vector3f x2 = fNbr.X;
		float num = x2.Dot(x);
		float num2 = 1f - num * num;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		float num3 = (float)Math.Sqrt(num2);
		if (x2.Cross(x).Dot(fNbr.Z) < 0f)
		{
			num3 = 0f - num3;
		}
		Matrix2f matrix2f = new Matrix2f(num, num3, 0f - num3, num);
		return nbrUV + matrix2f * vector2f;
	}

	private void update_uv_expmap(GraphNode node)
	{
		int id = node.id;
		int id2 = node.parent.id;
		Vector3f origin = PositionF(id2);
		Frame3f fNbr = new Frame3f(origin, NormalF(id2));
		node.uv = propagate_uv(PositionF(id), node.parent.uv, ref fNbr, ref SeedFrame);
	}

	private void update_uv_upwind_expmap(GraphNode node)
	{
		int id = node.id;
		Vector3f pos = PositionF(id);
		Vector2f zero = Vector2f.Zero;
		float num = 0f;
		int num2 = 0;
		foreach (int item in NeighboursF(node.id))
		{
			GraphNode graphNode = get_node(item, bCreateIfMissing: false);
			if (graphNode.frozen)
			{
				Vector3f vector3f = PositionF(item);
				Frame3f fNbr = new Frame3f(vector3f, NormalF(item));
				Vector2f vector2f = propagate_uv(pos, graphNode.uv, ref fNbr, ref SeedFrame);
				float num3 = 1f / (pos.DistanceSquared(vector3f) + 1E-06f);
				zero += num3 * vector2f;
				num += num3;
				num2++;
			}
		}
		zero /= num;
		node.uv = zero;
	}

	private void update_uv_planar(GraphNode g)
	{
		g.uv = compute_local_uv(ref SeedFrame, PositionF(g.id));
	}

	private GraphNode get_node(int id, bool bCreateIfMissing = true)
	{
		GraphNode graphNode = SparseNodes[id];
		if (graphNode == null)
		{
			graphNode = SparseNodePool.Allocate();
			graphNode.id = id;
			graphNode.parent = null;
			graphNode.frozen = false;
			graphNode.uv = Vector2f.Zero;
			graphNode.graph_distance = float.MaxValue;
			SparseNodes[id] = graphNode;
		}
		return graphNode;
	}

	private void update_neighbours_sparse(GraphNode parent)
	{
		Vector3f vector3f = PositionF(parent.id);
		float graph_distance = parent.graph_distance;
		foreach (int item in NeighboursF(parent.id))
		{
			GraphNode graphNode = get_node(item);
			if (graphNode.frozen)
			{
				continue;
			}
			float num = graph_distance + vector3f.Distance(PositionF(item));
			if (SparseQueue.Contains(graphNode))
			{
				if (num < graphNode.priority)
				{
					graphNode.parent = parent;
					graphNode.graph_distance = num;
					SparseQueue.Update(graphNode, graphNode.graph_distance);
				}
			}
			else
			{
				graphNode.parent = parent;
				graphNode.graph_distance = num;
				SparseQueue.Enqueue(graphNode, graphNode.graph_distance);
			}
		}
	}
}
