using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public class HierarchicalGraph
{
	private const int Tiling = 16;

	private const int MaxChildrenPerNode = 256;

	private const int MinChildrenPerNode = 128;

	private List<GraphNode>[] children = new List<GraphNode>[0];

	private List<int>[] connections = new List<int>[0];

	private int[] areas = new int[0];

	private byte[] dirty = new byte[0];

	public Action onConnectedComponentsChanged;

	private Action<GraphNode> connectionCallback;

	private Queue<GraphNode> temporaryQueue = new Queue<GraphNode>();

	private List<GraphNode> currentChildren;

	private List<int> currentConnections;

	private int currentHierarchicalNodeIndex;

	private Stack<int> temporaryStack = new Stack<int>();

	private int numDirtyNodes;

	private GraphNode[] dirtyNodes = new GraphNode[128];

	private Stack<int> freeNodeIndices = new Stack<int>();

	private int gizmoVersion;

	public int version { get; private set; }

	public int NumConnectedComponents { get; private set; }

	public HierarchicalGraph()
	{
		connectionCallback = delegate(GraphNode neighbour)
		{
			int hierarchicalNodeIndex = neighbour.HierarchicalNodeIndex;
			if (hierarchicalNodeIndex == 0)
			{
				if (currentChildren.Count < 256 && neighbour.Walkable)
				{
					neighbour.HierarchicalNodeIndex = currentHierarchicalNodeIndex;
					temporaryQueue.Enqueue(neighbour);
					currentChildren.Add(neighbour);
				}
			}
			else if (hierarchicalNodeIndex != currentHierarchicalNodeIndex && !currentConnections.Contains(hierarchicalNodeIndex))
			{
				currentConnections.Add(hierarchicalNodeIndex);
			}
		};
		Grow();
	}

	private void Grow()
	{
		List<GraphNode>[] array = new List<GraphNode>[Math.Max(64, children.Length * 2)];
		List<int>[] array2 = new List<int>[array.Length];
		int[] array3 = new int[array.Length];
		byte[] array4 = new byte[array.Length];
		children.CopyTo(array, 0);
		connections.CopyTo(array2, 0);
		areas.CopyTo(array3, 0);
		dirty.CopyTo(array4, 0);
		for (int i = children.Length; i < array.Length; i++)
		{
			array[i] = ListPool<GraphNode>.Claim(256);
			array2[i] = new List<int>();
			if (i > 0)
			{
				freeNodeIndices.Push(i);
			}
		}
		children = array;
		connections = array2;
		areas = array3;
		dirty = array4;
	}

	private int GetHierarchicalNodeIndex()
	{
		if (freeNodeIndices.Count == 0)
		{
			Grow();
		}
		return freeNodeIndices.Pop();
	}

	internal void OnCreatedNode(GraphNode node)
	{
		if (node.NodeIndex >= dirtyNodes.Length)
		{
			GraphNode[] array = new GraphNode[Math.Max(node.NodeIndex + 1, dirtyNodes.Length * 2)];
			dirtyNodes.CopyTo(array, 0);
			dirtyNodes = array;
		}
		AddDirtyNode(node);
	}

	public void AddDirtyNode(GraphNode node)
	{
		if (node.IsHierarchicalNodeDirty)
		{
			return;
		}
		node.IsHierarchicalNodeDirty = true;
		if (numDirtyNodes < dirtyNodes.Length)
		{
			dirtyNodes[numDirtyNodes] = node;
			numDirtyNodes++;
			return;
		}
		int val = 0;
		for (int num = numDirtyNodes - 1; num >= 0; num--)
		{
			if (dirtyNodes[num].Destroyed)
			{
				numDirtyNodes--;
				dirty[dirtyNodes[num].HierarchicalNodeIndex] = 1;
				dirtyNodes[num] = dirtyNodes[numDirtyNodes];
				dirtyNodes[numDirtyNodes] = null;
			}
			else
			{
				val = Math.Max(val, dirtyNodes[num].NodeIndex);
			}
		}
		if (numDirtyNodes >= dirtyNodes.Length)
		{
			throw new Exception("Failed to compactify dirty nodes array. This should not happen. " + val + " " + numDirtyNodes + " " + dirtyNodes.Length);
		}
		AddDirtyNode(node);
	}

	public uint GetConnectedComponent(int hierarchicalNodeIndex)
	{
		return (uint)areas[hierarchicalNodeIndex];
	}

	private void RemoveHierarchicalNode(int hierarchicalNode, bool removeAdjacentSmallNodes)
	{
		freeNodeIndices.Push(hierarchicalNode);
		List<int> list = connections[hierarchicalNode];
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i];
			if (dirty[num] == 0)
			{
				if (removeAdjacentSmallNodes && children[num].Count < 128)
				{
					dirty[num] = 2;
					RemoveHierarchicalNode(num, removeAdjacentSmallNodes: false);
				}
				else
				{
					connections[num].Remove(hierarchicalNode);
				}
			}
		}
		list.Clear();
		List<GraphNode> list2 = children[hierarchicalNode];
		for (int j = 0; j < list2.Count; j++)
		{
			AddDirtyNode(list2[j]);
		}
		list2.ClearFast();
	}

	public void RecalculateIfNecessary()
	{
		if (numDirtyNodes <= 0)
		{
			return;
		}
		for (int i = 0; i < numDirtyNodes; i++)
		{
			dirty[dirtyNodes[i].HierarchicalNodeIndex] = 1;
		}
		for (int j = 1; j < dirty.Length; j++)
		{
			if (dirty[j] == 1)
			{
				RemoveHierarchicalNode(j, removeAdjacentSmallNodes: true);
			}
		}
		for (int k = 1; k < dirty.Length; k++)
		{
			dirty[k] = 0;
		}
		for (int l = 0; l < numDirtyNodes; l++)
		{
			dirtyNodes[l].HierarchicalNodeIndex = 0;
		}
		for (int m = 0; m < numDirtyNodes; m++)
		{
			GraphNode graphNode = dirtyNodes[m];
			dirtyNodes[m] = null;
			graphNode.IsHierarchicalNodeDirty = false;
			if (graphNode.HierarchicalNodeIndex == 0 && graphNode.Walkable && !graphNode.Destroyed)
			{
				FindHierarchicalNodeChildren(GetHierarchicalNodeIndex(), graphNode);
			}
		}
		numDirtyNodes = 0;
		FloodFill();
		gizmoVersion++;
	}

	public void RecalculateAll()
	{
		AstarPath.active.data.GetNodes(delegate(GraphNode node)
		{
			AddDirtyNode(node);
		});
		RecalculateIfNecessary();
	}

	private void FloodFill()
	{
		for (int i = 0; i < areas.Length; i++)
		{
			areas[i] = 0;
		}
		Stack<int> stack = temporaryStack;
		int num = 0;
		for (int j = 1; j < areas.Length; j++)
		{
			if (areas[j] != 0)
			{
				continue;
			}
			num++;
			areas[j] = num;
			stack.Push(j);
			while (stack.Count > 0)
			{
				int num2 = stack.Pop();
				List<int> list = connections[num2];
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					int num4 = list[num3];
					if (areas[num4] != num)
					{
						areas[num4] = num;
						stack.Push(num4);
					}
				}
			}
		}
		NumConnectedComponents = Math.Max(1, num + 1);
		version++;
	}

	private void FindHierarchicalNodeChildren(int hierarchicalNode, GraphNode startNode)
	{
		currentChildren = children[hierarchicalNode];
		currentConnections = connections[hierarchicalNode];
		currentHierarchicalNodeIndex = hierarchicalNode;
		Queue<GraphNode> queue = temporaryQueue;
		queue.Enqueue(startNode);
		startNode.HierarchicalNodeIndex = hierarchicalNode;
		currentChildren.Add(startNode);
		while (queue.Count > 0)
		{
			queue.Dequeue().GetConnections(connectionCallback);
		}
		for (int i = 0; i < currentConnections.Count; i++)
		{
			connections[currentConnections[i]].Add(hierarchicalNode);
		}
		queue.Clear();
	}

	public void OnDrawGizmos(RetainedGizmos gizmos)
	{
		RetainedGizmos.Hasher hasher = new RetainedGizmos.Hasher(AstarPath.active);
		hasher.AddHash(gizmoVersion);
		if (gizmos.Draw(hasher))
		{
			return;
		}
		RetainedGizmos.Builder builder = ObjectPool<RetainedGizmos.Builder>.Claim();
		Vector3[] array = ArrayPool<Vector3>.Claim(areas.Length);
		for (int i = 0; i < areas.Length; i++)
		{
			Int3 zero = Int3.zero;
			List<GraphNode> list = children[i];
			if (list.Count > 0)
			{
				for (int j = 0; j < list.Count; j++)
				{
					zero += list[j].position;
				}
				zero /= (float)list.Count;
				array[i] = (Vector3)zero;
			}
		}
		for (int k = 0; k < areas.Length; k++)
		{
			if (children[k].Count <= 0)
			{
				continue;
			}
			for (int l = 0; l < connections[k].Count; l++)
			{
				if (connections[k][l] > k)
				{
					builder.DrawLine(array[k], array[connections[k][l]], Color.black);
				}
			}
		}
		builder.Submit(gizmos, hasher);
	}
}
