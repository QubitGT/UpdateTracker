using System;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.Collections;

namespace Oculus.Interaction.Body.Input;

public abstract class BodySkeletonMapping<TSourceJointId> : ISkeletonMapping where TSourceJointId : Enum
{
	private class SkeletonTree
	{
		public class Node
		{
			public readonly TSourceJointId SourceJointId;

			public readonly BodyJointId BodyJointId;

			public Node Parent;

			public List<Node> Children = new List<Node>();

			public Node(TSourceJointId sourceJointId, BodyJointId bodyJointId)
			{
				SourceJointId = sourceJointId;
				BodyJointId = bodyJointId;
			}
		}

		public readonly Node Root;

		public readonly IReadOnlyList<Node> Nodes;

		public SkeletonTree(TSourceJointId root, IReadOnlyDictionary<BodyJointId, JointInfo> mapping)
		{
			Dictionary<TSourceJointId, Node> dictionary = new Dictionary<TSourceJointId, Node>();
			foreach (KeyValuePair<BodyJointId, JointInfo> item in mapping)
			{
				BodyJointId key = item.Key;
				JointInfo value = item.Value;
				dictionary[value.SourceJointId] = new Node(value.SourceJointId, key);
			}
			foreach (JointInfo value2 in mapping.Values)
			{
				Node node = dictionary[value2.SourceJointId];
				node.Parent = dictionary[value2.ParentJointId];
				node.Parent.Children.Add(node);
			}
			Nodes = new List<Node>(dictionary.Values);
			Root = dictionary[root];
		}
	}

	protected readonly struct JointInfo
	{
		public readonly TSourceJointId SourceJointId;

		public readonly TSourceJointId ParentJointId;

		public JointInfo(TSourceJointId sourceJointId, TSourceJointId parentJointId)
		{
			SourceJointId = sourceJointId;
			ParentJointId = parentJointId;
		}
	}

	private readonly SkeletonTree _tree;

	private readonly IEnumerableHashSet<BodyJointId> _joints;

	private readonly IReadOnlyDictionary<TSourceJointId, BodyJointId> _forwardMap;

	private readonly IReadOnlyDictionary<BodyJointId, TSourceJointId> _reverseMap;

	private readonly IReadOnlyDictionary<BodyJointId, BodyJointId> _jointToParent;

	public IEnumerableHashSet<BodyJointId> Joints => _joints;

	public bool TryGetParentJointId(BodyJointId jointId, out BodyJointId parentJointId)
	{
		return _jointToParent.TryGetValue(jointId, out parentJointId);
	}

	public bool TryGetSourceJointId(BodyJointId jointId, out TSourceJointId sourceJointId)
	{
		return _reverseMap.TryGetValue(jointId, out sourceJointId);
	}

	public bool TryGetBodyJointId(TSourceJointId jointId, out BodyJointId bodyJointId)
	{
		return _forwardMap.TryGetValue(jointId, out bodyJointId);
	}

	protected TSourceJointId GetSourceJointFromBodyJoint(BodyJointId jointId)
	{
		return _reverseMap[jointId];
	}

	protected BodyJointId GetBodyJointFromSourceJoint(TSourceJointId sourceJointId)
	{
		return _forwardMap[sourceJointId];
	}

	protected BodySkeletonMapping(TSourceJointId root, IReadOnlyDictionary<BodyJointId, JointInfo> jointMapping)
	{
		_tree = new SkeletonTree(root, jointMapping);
		_joints = new EnumerableHashSet<BodyJointId>(_tree.Nodes.Select((SkeletonTree.Node n) => n.BodyJointId));
		_forwardMap = _tree.Nodes.ToDictionary((SkeletonTree.Node n) => n.SourceJointId, (SkeletonTree.Node n) => n.BodyJointId);
		_reverseMap = _tree.Nodes.ToDictionary((SkeletonTree.Node n) => n.BodyJointId, (SkeletonTree.Node n) => n.SourceJointId);
		_jointToParent = _tree.Nodes.ToDictionary((SkeletonTree.Node n) => n.BodyJointId, (SkeletonTree.Node n) => n.Parent.BodyJointId);
	}
}
