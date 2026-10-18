#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Fusion.LagCompensation;

internal class BVH : ILagCompensationBroadphase
{
	internal BVHNode[] _nodes;

	internal Mapper Mapper;

	internal int maxDepth = 0;

	internal HashSet<int> refitNodes = new HashSet<int>();

	internal readonly List<HitboxRoot> ReusableList = new List<HitboxRoot>(2);

	private int _nodesCount = 1;

	private int _usedNodesCount = 0;

	private int _freeNodesHead = 0;

	private const float DEFAULT_EXPANSION_FACTOR = 0.15f;

	private const int DEFAULT_PARENTS_TO_EXPAND = 3;

	internal float ExpansionFactor;

	internal int ParentsToExpand;

	internal ref BVHNode rootBVH => ref _nodes[1];

	internal int UsedNodesCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _usedNodesCount;
		}
	}

	public void CopyFrom(ILagCompensationBroadphase other)
	{
		BVH bVH = (BVH)other;
		if (bVH._nodesCount > _nodes.Length)
		{
			ResizeNodesArray(bVH._nodesCount - _nodes.Length);
		}
		Array.Clear(_nodes, 0, _nodes.Length);
		Array.Copy(bVH._nodes, 0, _nodes, 0, bVH._nodesCount);
		maxDepth = bVH.maxDepth;
		_nodesCount = bVH._nodesCount;
		_usedNodesCount = bVH._usedNodesCount;
		_freeNodesHead = bVH._freeNodesHead;
	}

	internal ref BVHNode GetNextNode(out int index)
	{
		if (_freeNodesHead == 0)
		{
			index = _nodesCount++;
		}
		else
		{
			index = _freeNodesHead;
			_freeNodesHead = _nodes[_freeNodesHead].Next;
		}
		ref BVHNode reference = ref _nodes[index];
		Assert.Check(!reference.Used, "Retrieving a node that is already marked as used {0}", index);
		reference = default(BVHNode);
		reference.Used = true;
		_usedNodesCount++;
		return ref reference;
	}

	internal void DisposeNode(int index)
	{
		Assert.Check(_nodes[index].Used, "Disposing a node that is not marked as Used. {0}", index);
		ref BVHNode reference = ref _nodes[index];
		reference.Used = false;
		reference.Next = _freeNodesHead;
		_freeNodesHead = index;
		_usedNodesCount--;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ref BVHNode GetNode(int index)
	{
		return ref _nodes[index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Update(HitboxRoot changed, int tick)
	{
		if (Mapper.TryGetLeafIndex(changed, out var index))
		{
			ref BVHNode node = ref GetNode(index);
			Assert.Check(BehaviourUtils.IsSame(node._root, changed));
			node.RefitObjectChanged(this);
		}
	}

	public void Traverse(IBoundsTraversalTest hitTest, HashSet<HitboxRoot> candidateRoots, int layerMask)
	{
		TraverseInternal(ref rootBVH, hitTest, candidateRoots, layerMask);
	}

	private void TraverseInternal(ref BVHNode curNode, IBoundsTraversalTest hitTest, HashSet<HitboxRoot> candidateRoots, int layermask)
	{
		if (curNode.IsValid && hitTest.Check(ref curNode._cachedBounds))
		{
			if (curNode.IsLeaf && curNode.Active && curNode.HasValidRoot)
			{
				candidateRoots.Add(curNode._root);
			}
			TraverseInternal(ref curNode.GetLeft(this), hitTest, candidateRoots, layermask);
			TraverseInternal(ref curNode.GetRight(this), hitTest, candidateRoots, layermask);
		}
	}

	public void PosUpdateRefit()
	{
		foreach (int refitNode in refitNodes)
		{
			GetNode(refitNode).ChildRefit(this);
		}
		refitNodes.Clear();
	}

	public void Add(HitboxRoot root)
	{
		Bounds box = root.GetBounds();
		float newObSah = BVHNode.SA(ref box);
		if (_nodesCount >= _nodes.Length)
		{
			ResizeNodesArray(_nodes.Length);
		}
		BVHNode.Add(this, ref rootBVH, root, ref box, newObSah);
	}

	internal static Bounds BoundsFromSphere(Vector3 pos, float radius)
	{
		Bounds result = default(Bounds);
		result.min = new Vector3(pos.x - radius, pos.y - radius, pos.z - radius);
		result.max = new Vector3(pos.x + radius, pos.y + radius, pos.z + radius);
		return result;
	}

	public bool Remove(HitboxRoot root)
	{
		if (Mapper.TryGetLeafIndex(root, out var index))
		{
			GetNode(index).Remove(this, root);
			return true;
		}
		return false;
	}

	internal BVH(Mapper mapper, int nodesCapacity, List<HitboxRoot> initialEntries = null, float expansionFactor = 0.15f, int parentsToExpand = 3)
	{
		_nodes = new BVHNode[Mathf.Max(32, nodesCapacity)];
		Mapper = mapper;
		ExpansionFactor = expansionFactor;
		ParentsToExpand = parentsToExpand;
		int index;
		ref BVHNode nextNode = ref GetNextNode(out index);
		Assert.Check(index == 1);
		BVHNode.InitNode(ref nextNode, this, index, 0, 0, initialEntries);
		Assert.Check(nextNode.IsRootNode);
	}

	internal void BuildNodesLog(StringBuilder builder)
	{
		builder.AppendLine($"Nodes count: {_nodesCount}, Used nodes: {UsedNodesCount}");
		for (int i = 0; i < _nodesCount; i++)
		{
			builder.Append($"[{i}]: ");
			_nodes[i].BuildLog(builder);
			builder.AppendLine();
		}
	}

	private void ResizeNodesArray(int minimumIncrease)
	{
		int newSize = _nodes.Length * Math.Max(2, Mathf.FloorToInt((float)minimumIncrease / (float)_nodes.Length + 1f));
		Array.Resize(ref _nodes, newSize);
	}
}
