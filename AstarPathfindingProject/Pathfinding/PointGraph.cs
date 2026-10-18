using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[JsonOptIn]
[Preserve]
public class PointGraph : NavGraph, IUpdatableGraph
{
	public enum NodeDistanceMode
	{
		Node,
		Connection
	}

	[JsonMember]
	public Transform root;

	[JsonMember]
	public string searchTag;

	[JsonMember]
	public float maxDistance;

	[JsonMember]
	public Vector3 limits;

	[JsonMember]
	public bool raycast = true;

	[JsonMember]
	public bool use2DPhysics;

	[JsonMember]
	public bool thickRaycast;

	[JsonMember]
	public float thickRaycastRadius = 1f;

	[JsonMember]
	public bool recursive = true;

	[JsonMember]
	public LayerMask mask;

	[JsonMember]
	public bool optimizeForSparseGraph;

	private PointKDTree lookupTree = new PointKDTree();

	private long maximumConnectionLength;

	public PointNode[] nodes;

	[JsonMember]
	public NodeDistanceMode nearestNodeDistanceMode;

	public int nodeCount { get; protected set; }

	public override int CountNodes()
	{
		return nodeCount;
	}

	public override void GetNodes(Action<GraphNode> action)
	{
		if (nodes != null)
		{
			int num = nodeCount;
			for (int i = 0; i < num; i++)
			{
				action(nodes[i]);
			}
		}
	}

	public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		return GetNearestInternal(position, constraint, fastCheck: true);
	}

	public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
	{
		return GetNearestInternal(position, constraint, fastCheck: false);
	}

	private NNInfoInternal GetNearestInternal(Vector3 position, NNConstraint constraint, bool fastCheck)
	{
		if (nodes == null)
		{
			return default(NNInfoInternal);
		}
		Int3 @int = (Int3)position;
		if (optimizeForSparseGraph)
		{
			if (nearestNodeDistanceMode == NodeDistanceMode.Node)
			{
				return new NNInfoInternal(lookupTree.GetNearest(@int, fastCheck ? null : constraint));
			}
			GraphNode nearestConnection = lookupTree.GetNearestConnection(@int, fastCheck ? null : constraint, maximumConnectionLength);
			if (nearestConnection == null)
			{
				return default(NNInfoInternal);
			}
			return FindClosestConnectionPoint(nearestConnection as PointNode, position);
		}
		float num = ((constraint == null || constraint.constrainDistance) ? AstarPath.active.maxNearestNodeDistanceSqr : float.PositiveInfinity);
		num *= 1000000f;
		NNInfoInternal result = new NNInfoInternal(null);
		long num2 = long.MaxValue;
		long num3 = long.MaxValue;
		for (int i = 0; i < nodeCount; i++)
		{
			PointNode pointNode = nodes[i];
			long sqrMagnitudeLong = (@int - pointNode.position).sqrMagnitudeLong;
			if (sqrMagnitudeLong < num2)
			{
				num2 = sqrMagnitudeLong;
				result.node = pointNode;
			}
			if (sqrMagnitudeLong < num3 && (float)sqrMagnitudeLong < num && (constraint == null || constraint.Suitable(pointNode)))
			{
				num3 = sqrMagnitudeLong;
				result.constrainedNode = pointNode;
			}
		}
		if (!fastCheck)
		{
			result.node = result.constrainedNode;
		}
		result.UpdateInfo();
		return result;
	}

	private NNInfoInternal FindClosestConnectionPoint(PointNode node, Vector3 position)
	{
		Vector3 clampedPosition = (Vector3)node.position;
		Connection[] connections = node.connections;
		Vector3 vector = (Vector3)node.position;
		float num = float.PositiveInfinity;
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				Vector3 lineEnd = ((Vector3)connections[i].node.position + vector) * 0.5f;
				Vector3 vector2 = VectorMath.ClosestPointOnSegment(vector, lineEnd, position);
				float sqrMagnitude = (vector2 - position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					clampedPosition = vector2;
				}
			}
		}
		NNInfoInternal result = default(NNInfoInternal);
		result.node = node;
		result.clampedPosition = clampedPosition;
		return result;
	}

	public PointNode AddNode(Int3 position)
	{
		return AddNode(new PointNode(active), position);
	}

	public T AddNode<T>(T node, Int3 position) where T : PointNode
	{
		if (nodes == null || nodeCount == nodes.Length)
		{
			PointNode[] array = new PointNode[(nodes != null) ? Math.Max(nodes.Length + 4, nodes.Length * 2) : 4];
			if (nodes != null)
			{
				nodes.CopyTo(array, 0);
			}
			nodes = array;
		}
		node.SetPosition(position);
		node.GraphIndex = graphIndex;
		node.Walkable = true;
		nodes[nodeCount] = node;
		nodeCount++;
		if (optimizeForSparseGraph)
		{
			AddToLookup(node);
		}
		return node;
	}

	protected static int CountChildren(Transform tr)
	{
		int num = 0;
		foreach (Transform item in tr)
		{
			num++;
			num += CountChildren(item);
		}
		return num;
	}

	protected void AddChildren(ref int c, Transform tr)
	{
		foreach (Transform item in tr)
		{
			nodes[c].position = (Int3)item.position;
			nodes[c].Walkable = true;
			nodes[c].gameObject = item.gameObject;
			c++;
			AddChildren(ref c, item);
		}
	}

	public void RebuildNodeLookup()
	{
		if (!optimizeForSparseGraph || nodes == null)
		{
			lookupTree = new PointKDTree();
		}
		else
		{
			PointKDTree pointKDTree = lookupTree;
			GraphNode[] array = nodes;
			pointKDTree.Rebuild(array, 0, nodeCount);
		}
		RebuildConnectionDistanceLookup();
	}

	public void RebuildConnectionDistanceLookup()
	{
		maximumConnectionLength = 0L;
		if (nearestNodeDistanceMode != NodeDistanceMode.Connection)
		{
			return;
		}
		for (int i = 0; i < nodeCount; i++)
		{
			PointNode pointNode = nodes[i];
			Connection[] connections = pointNode.connections;
			if (connections != null)
			{
				for (int j = 0; j < connections.Length; j++)
				{
					long sqrMagnitudeLong = (pointNode.position - connections[j].node.position).sqrMagnitudeLong;
					RegisterConnectionLength(sqrMagnitudeLong);
				}
			}
		}
	}

	private void AddToLookup(PointNode node)
	{
		lookupTree.Add(node);
	}

	public void RegisterConnectionLength(long sqrLength)
	{
		maximumConnectionLength = Math.Max(maximumConnectionLength, sqrLength);
	}

	protected virtual PointNode[] CreateNodes(int count)
	{
		PointNode[] array = new PointNode[count];
		for (int i = 0; i < nodeCount; i++)
		{
			array[i] = new PointNode(active);
		}
		return array;
	}

	protected override IEnumerable<Progress> ScanInternal()
	{
		yield return new Progress(0f, "Searching for GameObjects");
		if (root == null)
		{
			GameObject[] gos = ((searchTag != null) ? GameObject.FindGameObjectsWithTag(searchTag) : null);
			if (gos == null)
			{
				nodes = new PointNode[0];
				nodeCount = 0;
			}
			else
			{
				yield return new Progress(0.1f, "Creating nodes");
				nodeCount = gos.Length;
				nodes = CreateNodes(nodeCount);
				for (int i = 0; i < gos.Length; i++)
				{
					nodes[i].position = (Int3)gos[i].transform.position;
					nodes[i].Walkable = true;
					nodes[i].gameObject = gos[i].gameObject;
				}
			}
		}
		else if (!recursive)
		{
			nodeCount = root.childCount;
			nodes = CreateNodes(nodeCount);
			int num = 0;
			foreach (Transform item in root)
			{
				nodes[num].position = (Int3)item.position;
				nodes[num].Walkable = true;
				nodes[num].gameObject = item.gameObject;
				num++;
			}
		}
		else
		{
			nodeCount = CountChildren(root);
			nodes = CreateNodes(nodeCount);
			int c = 0;
			AddChildren(ref c, root);
		}
		yield return new Progress(0.15f, "Building node lookup");
		RebuildNodeLookup();
		foreach (Progress item2 in ConnectNodesAsync())
		{
			yield return item2.MapTo(0.15f, 0.95f);
		}
		yield return new Progress(0.95f, "Building connection distances");
		RebuildConnectionDistanceLookup();
	}

	public void ConnectNodes()
	{
		IEnumerator<Progress> enumerator = ConnectNodesAsync().GetEnumerator();
		while (enumerator.MoveNext())
		{
		}
		RebuildConnectionDistanceLookup();
	}

	private IEnumerable<Progress> ConnectNodesAsync()
	{
		if (!(maxDistance >= 0f))
		{
			yield break;
		}
		List<Connection> connections = new List<Connection>();
		List<GraphNode> candidateConnections = new List<GraphNode>();
		long maxSquaredRange;
		if (maxDistance == 0f && (limits.x == 0f || limits.y == 0f || limits.z == 0f))
		{
			maxSquaredRange = long.MaxValue;
		}
		else
		{
			maxSquaredRange = (long)(Mathf.Max(limits.x, Mathf.Max(limits.y, Mathf.Max(limits.z, maxDistance))) * 1000f) + 1;
			maxSquaredRange *= maxSquaredRange;
		}
		for (int i = 0; i < nodeCount; i++)
		{
			if (i % 512 == 0)
			{
				yield return new Progress((float)i / (float)nodeCount, "Connecting nodes");
			}
			connections.Clear();
			PointNode pointNode = nodes[i];
			if (optimizeForSparseGraph)
			{
				candidateConnections.Clear();
				lookupTree.GetInRange(pointNode.position, maxSquaredRange, candidateConnections);
				for (int j = 0; j < candidateConnections.Count; j++)
				{
					PointNode pointNode2 = candidateConnections[j] as PointNode;
					if (pointNode2 != pointNode && IsValidConnection(pointNode, pointNode2, out var dist))
					{
						connections.Add(new Connection(pointNode2, (uint)Mathf.RoundToInt(dist * 1000f)));
					}
				}
			}
			else
			{
				for (int k = 0; k < nodeCount; k++)
				{
					if (i != k)
					{
						PointNode pointNode3 = nodes[k];
						if (IsValidConnection(pointNode, pointNode3, out var dist2))
						{
							connections.Add(new Connection(pointNode3, (uint)Mathf.RoundToInt(dist2 * 1000f)));
						}
					}
				}
			}
			pointNode.connections = connections.ToArray();
			pointNode.SetConnectivityDirty();
		}
	}

	public virtual bool IsValidConnection(GraphNode a, GraphNode b, out float dist)
	{
		dist = 0f;
		if (!a.Walkable || !b.Walkable)
		{
			return false;
		}
		Vector3 vector = (Vector3)(b.position - a.position);
		if ((!Mathf.Approximately(limits.x, 0f) && Mathf.Abs(vector.x) > limits.x) || (!Mathf.Approximately(limits.y, 0f) && Mathf.Abs(vector.y) > limits.y) || (!Mathf.Approximately(limits.z, 0f) && Mathf.Abs(vector.z) > limits.z))
		{
			return false;
		}
		dist = vector.magnitude;
		if (maxDistance == 0f || dist < maxDistance)
		{
			if (raycast)
			{
				Ray ray = new Ray((Vector3)a.position, vector);
				Ray ray2 = new Ray((Vector3)b.position, -vector);
				if (use2DPhysics)
				{
					if (thickRaycast)
					{
						if (!Physics2D.CircleCast(ray.origin, thickRaycastRadius, ray.direction, dist, mask))
						{
							return !Physics2D.CircleCast(ray2.origin, thickRaycastRadius, ray2.direction, dist, mask);
						}
						return false;
					}
					if (!Physics2D.Linecast((Vector3)a.position, (Vector3)b.position, mask))
					{
						return !Physics2D.Linecast((Vector3)b.position, (Vector3)a.position, mask);
					}
					return false;
				}
				if (thickRaycast)
				{
					if (!Physics.SphereCast(ray, thickRaycastRadius, dist, mask))
					{
						return !Physics.SphereCast(ray2, thickRaycastRadius, dist, mask);
					}
					return false;
				}
				if (!Physics.Linecast((Vector3)a.position, (Vector3)b.position, mask))
				{
					return !Physics.Linecast((Vector3)b.position, (Vector3)a.position, mask);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	GraphUpdateThreading IUpdatableGraph.CanUpdateAsync(GraphUpdateObject o)
	{
		return GraphUpdateThreading.UnityThread;
	}

	void IUpdatableGraph.UpdateAreaInit(GraphUpdateObject o)
	{
	}

	void IUpdatableGraph.UpdateAreaPost(GraphUpdateObject o)
	{
	}

	void IUpdatableGraph.UpdateArea(GraphUpdateObject guo)
	{
		if (nodes == null)
		{
			return;
		}
		for (int i = 0; i < nodeCount; i++)
		{
			PointNode pointNode = nodes[i];
			if (guo.bounds.Contains((Vector3)pointNode.position))
			{
				guo.WillUpdateNode(pointNode);
				guo.Apply(pointNode);
			}
		}
		if (!guo.updatePhysics)
		{
			return;
		}
		Bounds bounds = guo.bounds;
		if (thickRaycast)
		{
			bounds.Expand(thickRaycastRadius * 2f);
		}
		List<Connection> list = ListPool<Connection>.Claim();
		for (int j = 0; j < nodeCount; j++)
		{
			PointNode pointNode2 = nodes[j];
			Vector3 a = (Vector3)pointNode2.position;
			List<Connection> list2 = null;
			for (int k = 0; k < nodeCount; k++)
			{
				if (k == j)
				{
					continue;
				}
				Vector3 b = (Vector3)nodes[k].position;
				if (!VectorMath.SegmentIntersectsBounds(bounds, a, b))
				{
					continue;
				}
				PointNode pointNode3 = nodes[k];
				bool flag = pointNode2.ContainsConnection(pointNode3);
				float dist;
				bool flag2 = IsValidConnection(pointNode2, pointNode3, out dist);
				if (list2 == null && flag != flag2)
				{
					list.Clear();
					list2 = list;
					list2.AddRange(pointNode2.connections);
				}
				if (!flag && flag2)
				{
					uint cost = (uint)Mathf.RoundToInt(dist * 1000f);
					list2.Add(new Connection(pointNode3, cost));
					RegisterConnectionLength((pointNode3.position - pointNode2.position).sqrMagnitudeLong);
				}
				else
				{
					if (!flag || flag2)
					{
						continue;
					}
					for (int l = 0; l < list2.Count; l++)
					{
						if (list2[l].node == pointNode3)
						{
							list2.RemoveAt(l);
							break;
						}
					}
				}
			}
			if (list2 != null)
			{
				pointNode2.connections = list2.ToArray();
				pointNode2.SetConnectivityDirty();
			}
		}
		ListPool<Connection>.Release(ref list);
	}

	protected override void PostDeserialization(GraphSerializationContext ctx)
	{
		RebuildNodeLookup();
	}

	public override void RelocateNodes(Matrix4x4 deltaMatrix)
	{
		base.RelocateNodes(deltaMatrix);
		RebuildNodeLookup();
	}

	protected override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
	{
		base.DeserializeSettingsCompatibility(ctx);
		root = ctx.DeserializeUnityObject() as Transform;
		searchTag = ctx.reader.ReadString();
		maxDistance = ctx.reader.ReadSingle();
		limits = ctx.DeserializeVector3();
		raycast = ctx.reader.ReadBoolean();
		use2DPhysics = ctx.reader.ReadBoolean();
		thickRaycast = ctx.reader.ReadBoolean();
		thickRaycastRadius = ctx.reader.ReadSingle();
		recursive = ctx.reader.ReadBoolean();
		ctx.reader.ReadBoolean();
		mask = ctx.reader.ReadInt32();
		optimizeForSparseGraph = ctx.reader.ReadBoolean();
		ctx.reader.ReadBoolean();
	}

	protected override void SerializeExtraInfo(GraphSerializationContext ctx)
	{
		if (nodes == null)
		{
			ctx.writer.Write(-1);
		}
		ctx.writer.Write(nodeCount);
		for (int i = 0; i < nodeCount; i++)
		{
			if (nodes[i] == null)
			{
				ctx.writer.Write(-1);
				continue;
			}
			ctx.writer.Write(0);
			nodes[i].SerializeNode(ctx);
		}
	}

	protected override void DeserializeExtraInfo(GraphSerializationContext ctx)
	{
		int num = ctx.reader.ReadInt32();
		if (num == -1)
		{
			nodes = null;
			return;
		}
		nodes = new PointNode[num];
		nodeCount = num;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (ctx.reader.ReadInt32() != -1)
			{
				nodes[i] = new PointNode(active);
				nodes[i].DeserializeNode(ctx);
			}
		}
	}
}
