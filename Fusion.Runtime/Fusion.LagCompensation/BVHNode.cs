#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Fusion.LagCompensation;

internal struct BVHNode
{
	internal enum Rot
	{
		NONE,
		L_RL,
		L_RR,
		R_LL,
		R_LR,
		LL_RR,
		LL_RL
	}

	internal const int MaxEntriesPerNode = 1;

	internal const int RootNodeIndex = 1;

	private static readonly HitboxRoot.HitboxComparerX ComparerX = new HitboxRoot.HitboxComparerX();

	private static readonly HitboxRoot.HitboxComparerY ComparerY = new HitboxRoot.HitboxComparerY();

	private static readonly HitboxRoot.HitboxComparerZ ComparerZ = new HitboxRoot.HitboxComparerZ();

	public Bounds Box;

	internal AABB _cachedBounds;

	private int _nodeIndex;

	private int _parentIndex;

	private int _leftIndex;

	private int _rightIndex;

	internal bool Active;

	internal int Depth;

	internal bool Used;

	internal int Next;

	internal HitboxRoot _root;

	internal bool _isLeaf;

	internal int Index => _nodeIndex;

	internal bool IsValid
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _nodeIndex > 0;
		}
	}

	internal bool IsRootNode
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _nodeIndex == 1;
		}
	}

	internal bool HasParent
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _parentIndex > 0;
		}
	}

	internal bool HasLeft
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _leftIndex > 0;
		}
	}

	internal bool HasRight
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _rightIndex > 0;
		}
	}

	internal bool IsLeaf
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _isLeaf;
		}
	}

	internal bool HasValidRoot
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return BehaviourUtils.IsAlive(_root) && BehaviourUtils.IsAlive(_root.Object);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ref BVHNode GetParent(BVH bvh)
	{
		return ref bvh._nodes[_parentIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ref BVHNode GetRight(BVH bvh)
	{
		return ref bvh._nodes[_rightIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ref BVHNode GetLeft(BVH bvh)
	{
		return ref bvh._nodes[_leftIndex];
	}

	public override string ToString()
	{
		return $"BVHNode: {_nodeIndex}";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void RefitObjectChanged(BVH bvh)
	{
		Assert.Check(BehaviourUtils.IsAlive(_root));
		Active = _root.HitboxRootActive;
		if (Active && RefitVolume(bvh) && HasParent)
		{
			bvh.refitNodes.Add(_parentIndex);
		}
	}

	private void ExpandVolume(BVH bvh, Vector3 objectpos, float radius, ref Bounds bounds, bool expandParent)
	{
		bool flag = false;
		if (objectpos.x - radius < bounds.min.x)
		{
			bounds.min = new Vector3(objectpos.x - radius, bounds.min.y, bounds.min.z);
			flag = true;
		}
		if (objectpos.x + radius > bounds.max.x)
		{
			bounds.max = new Vector3(objectpos.x + radius, bounds.max.y, bounds.max.z);
			flag = true;
		}
		if (objectpos.y - radius < bounds.min.y)
		{
			bounds.min = new Vector3(bounds.min.x, objectpos.y - radius, bounds.min.z);
			flag = true;
		}
		if (objectpos.y + radius > bounds.max.y)
		{
			bounds.max = new Vector3(bounds.max.x, objectpos.y + radius, bounds.max.z);
			flag = true;
		}
		if (objectpos.z - radius < bounds.min.z)
		{
			bounds.min = new Vector3(bounds.min.x, bounds.min.y, objectpos.z - radius);
			flag = true;
		}
		if (objectpos.z + radius > bounds.max.z)
		{
			bounds.max = new Vector3(bounds.max.x, bounds.max.y, objectpos.z + radius);
			flag = true;
		}
		if (expandParent && flag && HasParent)
		{
			GetParent(bvh).ChildExpanded(bvh, ref this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AssignVolume(Vector3 pos, float radius, ref Bounds bounds)
	{
		bounds.min = new Vector3(pos.x - radius, pos.y - radius, pos.z - radius);
		bounds.max = new Vector3(pos.x + radius, pos.y + radius, pos.z + radius);
	}

	private void ComputeVolume(BVH bvh)
	{
		if (BehaviourUtils.IsAlive(_root))
		{
			AssignVolume(_root.CachedTransform.TransformPoint(_root.Offset), _root.BroadRadius, ref Box);
		}
		else
		{
			ChildRefit(bvh, Index, propagate: false);
		}
	}

	private Bounds ComputeMinVolume(BVH bvh)
	{
		if (BehaviourUtils.IsAlive(_root))
		{
			Bounds bounds = default(Bounds);
			AssignVolume(_root.CachedTransform.TransformPoint(_root.Offset), _root.BroadRadius, ref bounds);
			return bounds;
		}
		Assert.Fail();
		ChildRefit(bvh, Index, propagate: false);
		return default(Bounds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool RefitVolume(BVH bvh)
	{
		Assert.Check(BehaviourUtils.IsAlive(_root));
		Bounds bounds = ComputeMinVolume(bvh);
		if (!Box.ContainBounds(bounds))
		{
			Box = bounds;
			Box.Expand(Box.size * bvh.ExpansionFactor);
			UpdateBoundsCache();
			return true;
		}
		return false;
	}

	internal static float SA(Bounds box)
	{
		float num = box.max.x - box.min.x;
		float num2 = box.max.y - box.min.y;
		float num3 = box.max.z - box.min.z;
		return 2f * (num * num2 + num * num3 + num2 * num3);
	}

	internal static float SA(ref Bounds box)
	{
		float num = box.max.x - box.min.x;
		float num2 = box.max.y - box.min.y;
		float num3 = box.max.z - box.min.z;
		return 2f * (num * num2 + num * num3 + num2 * num3);
	}

	internal static float SA(ref BVHNode node)
	{
		float num = node.Box.max.x - node.Box.min.x;
		float num2 = node.Box.max.y - node.Box.min.y;
		float num3 = node.Box.max.z - node.Box.min.z;
		return 2f * (num * num2 + num * num3 + num2 * num3);
	}

	internal static Bounds AABBofPair(ref BVHNode nodea, ref BVHNode nodeb)
	{
		Bounds box = nodea.Box;
		box.Encapsulate(nodeb.Box);
		return box;
	}

	private static Bounds GetEntryBounds(HitboxRoot entry)
	{
		float broadRadius = entry.BroadRadius;
		Bounds result = default(Bounds);
		result.min = new Vector3(0f - broadRadius, 0f - broadRadius, 0f - broadRadius);
		result.max = new Vector3(broadRadius, broadRadius, broadRadius);
		return result;
	}

	private static float SAofList(List<HitboxRoot> entries)
	{
		Assert.Check(entries.Count > 0);
		Bounds box = GetEntryBounds(entries[0]);
		foreach (HitboxRoot entry in entries)
		{
			Bounds entryBounds = GetEntryBounds(entry);
			box.Encapsulate(entryBounds);
		}
		return SA(ref box);
	}

	internal void SplitNode(BVH bvh, List<HitboxRoot> entries)
	{
		Assert.Check(entries != null);
		Assert.Check(entries.Count > 1);
		int num = entries.Count / 2;
		entries.Sort(ComparerX);
		List<HitboxRoot> range = entries.GetRange(0, num);
		List<HitboxRoot> range2 = entries.GetRange(num, entries.Count - num);
		float num2 = SAofList(range) * (float)range.Count + SAofList(range2) * (float)range2.Count;
		float num3 = num2;
		List<HitboxRoot> entries2 = range;
		List<HitboxRoot> entries3 = range2;
		entries.Sort(ComparerY);
		range = entries.GetRange(0, num);
		range2 = entries.GetRange(num, entries.Count - num);
		num2 = SAofList(range) * (float)range.Count + SAofList(range2) * (float)range2.Count;
		if (num2 < num3 || (num2 == num3 && UnityEngine.Random.value > 0.67f))
		{
			num3 = num2;
			entries2 = range;
			entries3 = range2;
		}
		entries.Sort(ComparerZ);
		range = entries.GetRange(0, num);
		range2 = entries.GetRange(num, entries.Count - num);
		num2 = SAofList(range) * (float)range.Count + SAofList(range2) * (float)range2.Count;
		if (num2 < num3 || (num2 == num3 && UnityEngine.Random.value > 0.67f))
		{
			num3 = num2;
			entries2 = range;
			entries3 = range2;
		}
		ref BVHNode nextNode = ref bvh.GetNextNode(out _leftIndex);
		ref BVHNode nextNode2 = ref bvh.GetNextNode(out _rightIndex);
		InitNode(ref nextNode, bvh, _leftIndex, _nodeIndex, Depth + 1, entries2);
		InitNode(ref nextNode2, bvh, _rightIndex, _nodeIndex, Depth + 1, entries3);
		_isLeaf = false;
		Active = true;
	}

	private static void AddObjectPushdown(BVH bvh, ref BVHNode curNode, HitboxRoot entry)
	{
		ref BVHNode parent = ref curNode.GetParent(bvh);
		bool flag = parent._rightIndex == curNode.Index;
		bvh.ReusableList.Clear();
		bvh.ReusableList.Add(entry);
		int index;
		ref BVHNode nextNode = ref bvh.GetNextNode(out index);
		InitNode(ref nextNode, bvh, index, parent.Index, curNode.Depth);
		int index2;
		ref BVHNode nextNode2 = ref bvh.GetNextNode(out index2);
		InitNode(ref nextNode2, bvh, index2, index, nextNode.Depth + 1, bvh.ReusableList);
		if (flag)
		{
			parent._rightIndex = index;
		}
		else
		{
			parent._leftIndex = index;
		}
		curNode._parentIndex = index;
		curNode.Depth = nextNode.Depth + 1;
		nextNode._leftIndex = curNode.Index;
		nextNode._rightIndex = nextNode2.Index;
		ChildRefit(bvh, index);
	}

	internal static void Add(BVH bvh, ref BVHNode startNode, HitboxRoot entry, ref Bounds newObBox, float newObSah)
	{
		Assert.Check(BehaviourUtils.IsAlive(entry));
		if ((!startNode.HasLeft || !startNode.HasRight) && startNode.IsRootNode)
		{
			Assert.Check(!startNode.HasLeft && !startNode.HasRight);
			bvh.ReusableList.Clear();
			bvh.ReusableList.Add(entry);
			Assert.Check(startNode._isLeaf == BehaviourUtils.IsAlive(startNode._root));
			if (startNode._isLeaf)
			{
				bvh.ReusableList.Add(startNode._root);
			}
			InitNode(ref startNode, bvh, startNode.Index, 0, 0, bvh.ReusableList);
			return;
		}
		ref BVHNode reference = ref startNode;
		while (!reference.IsLeaf)
		{
			if (!reference.HasLeft || !reference.HasRight)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(string.Format("Trying to add '{0}' to a {1}({2}) that does not have a Left({3}) and/or Right({4}) children and is not a Leaf. Interrupting operation to avoid an infinite loop.", entry, "BVHNode", reference.Index, reference._leftIndex, reference._rightIndex));
				bvh.BuildNodesLog(stringBuilder);
				throw new InvalidOperationException(stringBuilder.ToString());
			}
			ref BVHNode left = ref reference.GetLeft(bvh);
			ref BVHNode right = ref reference.GetRight(bvh);
			float num = SA(ref left);
			float num2 = SA(ref right);
			Bounds bounds = default(Bounds);
			bounds.min = left.Box.min;
			bounds.max = left.Box.max;
			Bounds box = bounds;
			bounds = default(Bounds);
			bounds.min = right.Box.min;
			bounds.max = right.Box.max;
			Bounds box2 = bounds;
			box.Encapsulate(newObBox);
			box2.Encapsulate(newObBox);
			float num3 = num2 + SA(box);
			float num4 = num + SA(box2);
			float num5 = SA(AABBofPair(ref left, ref right)) + newObSah;
			reference = ref !(num3 < num4) ? ref right : ref left;
		}
		AddObjectPushdown(bvh, ref reference, entry);
	}

	internal int NodesCount(BVH bvh)
	{
		if (BehaviourUtils.IsAlive(_root))
		{
			return 1;
		}
		return GetLeft(bvh).NodesCount(bvh) + GetRight(bvh).NodesCount(bvh);
	}

	internal void Remove(BVH bvh, HitboxRoot entry)
	{
		if (!_root)
		{
			throw new Exception("removeObject() called on nonLeaf!");
		}
		Assert.Check(BehaviourUtils.IsSame(_root, entry));
		bvh.Mapper.DeRegister(entry);
		if (IsRootNode)
		{
			InitNode(ref bvh.GetNode(_nodeIndex), bvh, _nodeIndex, 0, 0);
			return;
		}
		Assert.Check(HasParent);
		GetParent(bvh).RemoveLeaf(bvh, Index);
	}

	private void SetDepth(BVH bvh, int newdepth)
	{
		Depth = newdepth;
		if (newdepth > bvh.maxDepth)
		{
			bvh.maxDepth = newdepth;
		}
		if (HasLeft)
		{
			GetLeft(bvh).SetDepth(bvh, newdepth + 1);
		}
		if (HasRight)
		{
			GetRight(bvh).SetDepth(bvh, newdepth + 1);
		}
	}

	private void RemoveLeaf(BVH bvh, int removeIndex)
	{
		Assert.Check(!IsLeaf);
		if (removeIndex != _leftIndex && removeIndex != _rightIndex)
		{
			throw new Exception("removeLeaf doesn't match any leaf!");
		}
		ref BVHNode reference = ref this;
		ref BVHNode reference2 = ref this;
		if (removeIndex == _leftIndex)
		{
			reference = ref GetLeft(bvh);
			reference2 = ref GetRight(bvh);
			_leftIndex = 0;
		}
		else if (removeIndex == _rightIndex)
		{
			reference = ref GetRight(bvh);
			reference2 = ref GetLeft(bvh);
			_rightIndex = 0;
		}
		bvh.DisposeNode(removeIndex);
		Assert.Check(reference.IsLeaf);
		if (IsRootNode)
		{
			this = reference2;
			_nodeIndex = 1;
			_parentIndex = 0;
			Depth = 0;
			if (HasLeft)
			{
				ref BVHNode left = ref GetLeft(bvh);
				left._parentIndex = _nodeIndex;
				left.SetDepth(bvh, Depth + 1);
			}
			if (HasRight)
			{
				ref BVHNode right = ref GetRight(bvh);
				right._parentIndex = _nodeIndex;
				right.SetDepth(bvh, Depth + 1);
			}
			if (_isLeaf)
			{
				bvh.Mapper.DeRegister(_root);
				bvh.Mapper.RegisterMapping(_root, _nodeIndex);
			}
			bvh.DisposeNode(reference2.Index);
		}
		else
		{
			Assert.Check(HasParent);
			ref BVHNode parent = ref GetParent(bvh);
			Assert.Check(Index == parent._leftIndex || Index == parent._rightIndex);
			if (Index == parent._leftIndex)
			{
				parent._leftIndex = reference2.Index;
			}
			else
			{
				parent._rightIndex = reference2.Index;
			}
			Assert.Check(reference2.IsValid);
			reference2._parentIndex = _parentIndex;
			reference2.SetDepth(bvh, reference2.Depth - 1);
			Assert.Check(reference2.HasParent);
			reference2.GetParent(bvh).ChildRefit(bvh);
			bvh.DisposeNode(Index);
		}
	}

	internal void FindOverlappingLeaves(BVH bvh, Vector3 origin, float radius, List<BVHNode> overlapList)
	{
		if (BoundsIntersectsSphere(ToBounds(), origin, radius))
		{
			if (BehaviourUtils.IsAlive(_root))
			{
				overlapList.Add(this);
				return;
			}
			GetLeft(bvh).FindOverlappingLeaves(bvh, origin, radius, overlapList);
			GetRight(bvh).FindOverlappingLeaves(bvh, origin, radius, overlapList);
		}
	}

	private bool BoundsIntersectsSphere(Bounds bounds, Vector3 origin, float radius)
	{
		if (origin.x + radius < bounds.min.x || origin.y + radius < bounds.min.y || origin.z + radius < bounds.min.z || origin.x - radius > bounds.max.x || origin.y - radius > bounds.max.y || origin.z - radius > bounds.max.z)
		{
			return false;
		}
		return true;
	}

	internal void FindOverlappingLeaves(BVH bvh, Bounds aabb, List<BVHNode> overlapList)
	{
		if (ToBounds().Intersects(aabb))
		{
			if (BehaviourUtils.IsAlive(_root))
			{
				overlapList.Add(this);
				return;
			}
			GetLeft(bvh).FindOverlappingLeaves(bvh, aabb, overlapList);
			GetRight(bvh).FindOverlappingLeaves(bvh, aabb, overlapList);
		}
	}

	internal Bounds ToBounds()
	{
		Bounds result = default(Bounds);
		result.min = new Vector3(Box.min.x, Box.min.y, Box.min.z);
		result.max = new Vector3(Box.max.x, Box.max.y, Box.max.z);
		return result;
	}

	internal void ChildExpanded(BVH bvh, ref BVHNode child)
	{
		bool flag = false;
		if (child.Box.min.x < Box.min.x)
		{
			Box.min = new Vector3(child.Box.min.x, Box.min.y, Box.min.z);
			flag = true;
		}
		if (child.Box.max.x > Box.max.x)
		{
			Box.max = new Vector3(child.Box.max.x, Box.max.y, Box.max.z);
			flag = true;
		}
		if (child.Box.min.y < Box.min.y)
		{
			Box.min = new Vector3(Box.min.x, child.Box.min.y, Box.min.z);
			flag = true;
		}
		if (child.Box.max.y > Box.max.y)
		{
			Box.max = new Vector3(Box.max.x, child.Box.max.y, Box.max.z);
			flag = true;
		}
		if (child.Box.min.z < Box.min.z)
		{
			Box.min = new Vector3(Box.min.x, Box.min.y, child.Box.min.z);
			flag = true;
		}
		if (child.Box.max.z > Box.max.z)
		{
			Box.max = new Vector3(Box.max.x, Box.max.y, child.Box.max.z);
			flag = true;
		}
		if (flag)
		{
			UpdateBoundsCache();
		}
		if (flag && HasParent)
		{
			GetParent(bvh).ChildExpanded(bvh, ref this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateBoundsCache()
	{
		_cachedBounds = new AABB(Box);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ChildRefit(BVH bvh, bool propagate = true)
	{
		ChildRefit(bvh, Index, propagate);
	}

	internal static void ChildRefit(BVH bvh, int nodeIndex, bool propagate = true)
	{
		int num = 0;
		do
		{
			ref BVHNode node = ref bvh.GetNode(nodeIndex);
			ref BVHNode left = ref node.GetLeft(bvh);
			ref BVHNode right = ref node.GetRight(bvh);
			if (!node.Box.ContainBounds(left.Box) || !node.Box.ContainBounds(right.Box))
			{
				Bounds box = left.Box;
				box.Encapsulate(right.Box);
				node.Box = box;
				if (num < bvh.ParentsToExpand)
				{
					node.Box.Expand(node.Box.size * bvh.ExpansionFactor);
					num++;
				}
				node.UpdateBoundsCache();
				nodeIndex = node._parentIndex;
			}
			else
			{
				nodeIndex = 0;
			}
		}
		while (propagate && nodeIndex != 0 && !bvh.refitNodes.Contains(nodeIndex));
	}

	internal static void InitNode(ref BVHNode node, BVH bvh, int index, int parentIndex, int curDepth, List<HitboxRoot> entries = null)
	{
		node._nodeIndex = index;
		node._parentIndex = parentIndex;
		node._leftIndex = 0;
		node._rightIndex = 0;
		node._root = null;
		node._isLeaf = false;
		node.Active = true;
		node.Depth = curDepth;
		if (bvh.maxDepth < node.Depth)
		{
			bvh.maxDepth = node.Depth;
		}
		node.Box = default(Bounds);
		if (entries != null && entries.Count >= 1)
		{
			if (entries.Count <= 1)
			{
				Assert.Check(entries.Count == 1);
				node._root = entries[0];
				node.Active = node._root.HitboxRootActive;
				node._isLeaf = true;
				bvh.Mapper.RegisterMapping(node._root, node.Index);
				node.Box = node.ComputeMinVolume(bvh);
				node.Box.Expand(node.Box.size * bvh.ExpansionFactor);
				node.UpdateBoundsCache();
			}
			else
			{
				node.SplitNode(bvh, entries);
				node.ChildRefit(bvh, propagate: false);
			}
		}
	}

	public void BuildLog(StringBuilder builder)
	{
		builder.Append($"Index: {_nodeIndex}");
		builder.Append($", Active: {Active}");
		builder.Append($", Used: {Used}");
		builder.Append($", Next: {Next}");
		builder.Append($", Depth: {Depth}");
		builder.Append(", Root: '" + (BehaviourUtils.IsAlive(_root) ? _root.name : "NULL") + "'");
		builder.Append($", Parent: {_parentIndex}");
		builder.Append($", IsLeaf: {_isLeaf}");
		builder.Append($", Left: {_leftIndex}");
		builder.Append($", Right {_rightIndex}");
	}
}
