using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public class GraphUpdateObject
{
	public Bounds bounds;

	public bool updatePhysics = true;

	public bool resetPenaltyOnPhysics = true;

	public bool updateErosion = true;

	public NNConstraint nnConstraint = NNConstraint.None;

	public int addPenalty;

	public bool modifyWalkability;

	public bool setWalkability;

	public bool modifyTag;

	public int setTag;

	public bool trackChangedNodes;

	public List<GraphNode> changedNodes;

	private List<uint> backupData;

	private List<Int3> backupPositionData;

	public GraphUpdateShape shape;

	internal int internalStage = -1;

	internal const int STAGE_CREATED = -1;

	internal const int STAGE_PENDING = -2;

	internal const int STAGE_ABORTED = -3;

	internal const int STAGE_APPLIED = 0;

	[Obsolete("Not necessary anymore")]
	public bool requiresFloodFill
	{
		set
		{
		}
	}

	public GraphUpdateStage stage => internalStage switch
	{
		-1 => GraphUpdateStage.Created, 
		0 => GraphUpdateStage.Applied, 
		-3 => GraphUpdateStage.Aborted, 
		_ => GraphUpdateStage.Pending, 
	};

	public virtual void WillUpdateNode(GraphNode node)
	{
		if (trackChangedNodes && node != null)
		{
			if (changedNodes == null)
			{
				changedNodes = ListPool<GraphNode>.Claim();
				backupData = ListPool<uint>.Claim();
				backupPositionData = ListPool<Int3>.Claim();
			}
			changedNodes.Add(node);
			backupPositionData.Add(node.position);
			backupData.Add(node.Penalty);
			backupData.Add(node.Flags);
			if (node is GridNode gridNode)
			{
				backupData.Add(gridNode.InternalGridFlags);
			}
		}
	}

	public virtual void RevertFromBackup()
	{
		if (trackChangedNodes)
		{
			if (changedNodes == null)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < changedNodes.Count; i++)
			{
				changedNodes[i].Penalty = backupData[num];
				num++;
				int hierarchicalNodeIndex = changedNodes[i].HierarchicalNodeIndex;
				changedNodes[i].Flags = backupData[num];
				changedNodes[i].HierarchicalNodeIndex = hierarchicalNodeIndex;
				num++;
				if (changedNodes[i] is GridNode gridNode)
				{
					gridNode.InternalGridFlags = (ushort)backupData[num];
					num++;
				}
				changedNodes[i].position = backupPositionData[i];
				changedNodes[i].SetConnectivityDirty();
			}
			ListPool<GraphNode>.Release(ref changedNodes);
			ListPool<uint>.Release(ref backupData);
			ListPool<Int3>.Release(ref backupPositionData);
			return;
		}
		throw new InvalidOperationException("Changed nodes have not been tracked, cannot revert from backup. Please set trackChangedNodes to true before applying the update.");
	}

	public virtual void Apply(GraphNode node)
	{
		if (shape == null || shape.Contains(node))
		{
			node.Penalty = (uint)(node.Penalty + addPenalty);
			if (modifyWalkability)
			{
				node.Walkable = setWalkability;
			}
			if (modifyTag)
			{
				node.Tag = (uint)setTag;
			}
		}
	}

	public GraphUpdateObject()
	{
	}

	public GraphUpdateObject(Bounds b)
	{
		bounds = b;
	}
}
