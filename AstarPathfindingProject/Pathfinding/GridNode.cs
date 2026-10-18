using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding;

public class GridNode : GridNodeBase
{
	private static GridGraph[] _gridGraphs = new GridGraph[0];

	private const int GridFlagsConnectionOffset = 0;

	private const int GridFlagsConnectionBit0 = 1;

	private const int GridFlagsConnectionMask = 255;

	private const int GridFlagsEdgeNodeOffset = 10;

	private const int GridFlagsEdgeNodeMask = 1024;

	internal ushort InternalGridFlags
	{
		get
		{
			return gridFlags;
		}
		set
		{
			gridFlags = value;
		}
	}

	public override bool HasConnectionsToAllEightNeighbours => (InternalGridFlags & 0xFF) == 255;

	public bool EdgeNode
	{
		get
		{
			return (gridFlags & 0x400) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & 0xFFFFFBFFu) | (value ? 1024u : 0u));
		}
	}

	public GridNode(AstarPath astar)
		: base(astar)
	{
	}

	public static GridGraph GetGridGraph(uint graphIndex)
	{
		return _gridGraphs[graphIndex];
	}

	public static void SetGridGraph(int graphIndex, GridGraph graph)
	{
		if (_gridGraphs.Length <= graphIndex)
		{
			GridGraph[] array = new GridGraph[graphIndex + 1];
			for (int i = 0; i < _gridGraphs.Length; i++)
			{
				array[i] = _gridGraphs[i];
			}
			_gridGraphs = array;
		}
		_gridGraphs[graphIndex] = graph;
	}

	public static void ClearGridGraph(int graphIndex, GridGraph graph)
	{
		if (graphIndex < _gridGraphs.Length && _gridGraphs[graphIndex] == graph)
		{
			_gridGraphs[graphIndex] = null;
		}
	}

	public override bool HasConnectionInDirection(int dir)
	{
		return ((gridFlags >> dir) & 1) != 0;
	}

	[Obsolete("Use HasConnectionInDirection")]
	public bool GetConnectionInternal(int dir)
	{
		return HasConnectionInDirection(dir);
	}

	public void SetConnectionInternal(int dir, bool value)
	{
		gridFlags = (ushort)((gridFlags & ~(1 << dir)) | ((value ? 1 : 0) << dir));
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public void SetAllConnectionInternal(int connections)
	{
		gridFlags = (ushort)((gridFlags & 0xFFFFFF00u) | (uint)connections);
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public void ResetConnectionsInternal()
	{
		gridFlags = (ushort)(gridFlags & 0xFFFFFF00u);
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public override GridNodeBase GetNeighbourAlongDirection(int direction)
	{
		if (HasConnectionInDirection(direction))
		{
			GridGraph gridGraph = GetGridGraph(base.GraphIndex);
			return gridGraph.nodes[base.NodeInGridIndex + gridGraph.neighbourOffsets[direction]];
		}
		return null;
	}

	public override void ClearConnections(bool alsoReverse)
	{
		if (alsoReverse)
		{
			for (int i = 0; i < 8; i++)
			{
				if (GetNeighbourAlongDirection(i) is GridNode gridNode)
				{
					gridNode.SetConnectionInternal((i < 4) ? ((i + 2) % 4) : ((i - 2) % 4 + 4), value: false);
				}
			}
		}
		ResetConnectionsInternal();
		base.ClearConnections(alsoReverse);
	}

	public override void GetConnections(Action<GraphNode> action)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNodeBase[] nodes = gridGraph.nodes;
		for (int i = 0; i < 8; i++)
		{
			if (HasConnectionInDirection(i))
			{
				GridNodeBase gridNodeBase = nodes[base.NodeInGridIndex + neighbourOffsets[i]];
				if (gridNodeBase != null)
				{
					action(gridNodeBase);
				}
			}
		}
		base.GetConnections(action);
	}

	public override Vector3 ClosestPointOnNode(Vector3 p)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		p = gridGraph.transform.InverseTransform(p);
		int num = base.NodeInGridIndex % gridGraph.width;
		int num2 = base.NodeInGridIndex / gridGraph.width;
		float y = gridGraph.transform.InverseTransform((Vector3)position).y;
		Vector3 point = new Vector3(Mathf.Clamp(p.x, num, (float)num + 1f), y, Mathf.Clamp(p.z, num2, (float)num2 + 1f));
		return gridGraph.transform.Transform(point);
	}

	public override bool GetPortal(GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards)
	{
		if (backwards)
		{
			return true;
		}
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNodeBase[] nodes = gridGraph.nodes;
		for (int i = 0; i < 4; i++)
		{
			if (HasConnectionInDirection(i) && other == nodes[base.NodeInGridIndex + neighbourOffsets[i]])
			{
				Vector3 vector = (Vector3)(position + other.position) * 0.5f;
				Vector3 vector2 = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
				vector2.Normalize();
				vector2 *= gridGraph.nodeSize * 0.5f;
				left.Add(vector - vector2);
				right.Add(vector + vector2);
				return true;
			}
		}
		for (int j = 4; j < 8; j++)
		{
			if (!HasConnectionInDirection(j) || other != nodes[base.NodeInGridIndex + neighbourOffsets[j]])
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			if (HasConnectionInDirection(j - 4))
			{
				GridNodeBase gridNodeBase = nodes[base.NodeInGridIndex + neighbourOffsets[j - 4]];
				if (gridNodeBase.Walkable && gridNodeBase.HasConnectionInDirection((j - 4 + 1) % 4))
				{
					flag = true;
				}
			}
			if (HasConnectionInDirection((j - 4 + 1) % 4))
			{
				GridNodeBase gridNodeBase2 = nodes[base.NodeInGridIndex + neighbourOffsets[(j - 4 + 1) % 4]];
				if (gridNodeBase2.Walkable && gridNodeBase2.HasConnectionInDirection(j - 4))
				{
					flag2 = true;
				}
			}
			Vector3 vector3 = (Vector3)(position + other.position) * 0.5f;
			Vector3 vector4 = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
			vector4.Normalize();
			vector4 *= gridGraph.nodeSize * 1.4142f;
			left.Add(vector3 - (flag2 ? vector4 : Vector3.zero));
			right.Add(vector3 + (flag ? vector4 : Vector3.zero));
			return true;
		}
		return false;
	}

	public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNodeBase[] nodes = gridGraph.nodes;
		pathNode.UpdateG(path);
		handler.heap.Add(pathNode);
		ushort pathID = handler.PathID;
		int num = base.NodeInGridIndex;
		for (int i = 0; i < 8; i++)
		{
			if (HasConnectionInDirection(i))
			{
				GridNodeBase gridNodeBase = nodes[num + neighbourOffsets[i]];
				PathNode pathNode2 = handler.GetPathNode(gridNodeBase);
				if (pathNode2.parent == pathNode && pathNode2.pathID == pathID)
				{
					gridNodeBase.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}
		base.UpdateRecursiveG(path, pathNode, handler);
	}

	public override void Open(Path path, PathNode pathNode, PathHandler handler)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		ushort pathID = handler.PathID;
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		uint[] neighbourCosts = gridGraph.neighbourCosts;
		GridNodeBase[] nodes = gridGraph.nodes;
		int num = base.NodeInGridIndex;
		for (int i = 0; i < 8; i++)
		{
			if (!HasConnectionInDirection(i))
			{
				continue;
			}
			GridNodeBase gridNodeBase = nodes[num + neighbourOffsets[i]];
			if (path.CanTraverse(gridNodeBase))
			{
				PathNode pathNode2 = handler.GetPathNode(gridNodeBase);
				uint num2 = neighbourCosts[i];
				if (pathNode2.pathID != pathID)
				{
					pathNode2.parent = pathNode;
					pathNode2.pathID = pathID;
					pathNode2.cost = num2;
					pathNode2.H = path.CalculateHScore(gridNodeBase);
					pathNode2.UpdateG(path);
					handler.heap.Add(pathNode2);
				}
				else if (pathNode.G + num2 + path.GetTraversalCost(gridNodeBase) < pathNode2.G)
				{
					pathNode2.cost = num2;
					pathNode2.parent = pathNode;
					gridNodeBase.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}
		base.Open(path, pathNode, handler);
	}

	public override void SerializeNode(GraphSerializationContext ctx)
	{
		base.SerializeNode(ctx);
		ctx.SerializeInt3(position);
		ctx.writer.Write(gridFlags);
	}

	public override void DeserializeNode(GraphSerializationContext ctx)
	{
		base.DeserializeNode(ctx);
		position = ctx.DeserializeInt3();
		gridFlags = ctx.reader.ReadUInt16();
	}

	public override void AddConnection(GraphNode node, uint cost)
	{
		if (node is GridNode gridNode && gridNode.GraphIndex == base.GraphIndex)
		{
			RemoveGridConnection(gridNode);
		}
		base.AddConnection(node, cost);
	}

	public override void RemoveConnection(GraphNode node)
	{
		base.RemoveConnection(node);
		if (node is GridNode gridNode && gridNode.GraphIndex == base.GraphIndex)
		{
			RemoveGridConnection(gridNode);
		}
	}

	protected void RemoveGridConnection(GridNode node)
	{
		int num = base.NodeInGridIndex;
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		for (int i = 0; i < 8; i++)
		{
			if (num + gridGraph.neighbourOffsets[i] == node.NodeInGridIndex && GetNeighbourAlongDirection(i) == node)
			{
				SetConnectionInternal(i, value: false);
				break;
			}
		}
	}
}
