using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oculus.Interaction.DebugTree;

public abstract class DebugTree<TLeaf> where TLeaf : class
{
	private class Node : ITreeNode<TLeaf>
	{
		TLeaf ITreeNode<TLeaf>.Value => Value;

		IEnumerable<ITreeNode<TLeaf>> ITreeNode<TLeaf>.Children => Children;

		public TLeaf Value { get; set; }

		public List<Node> Children { get; set; }
	}

	private Dictionary<TLeaf, Node> _existingNodes = new Dictionary<TLeaf, Node>();

	private readonly TLeaf Root;

	private Node _rootNode;

	public DebugTree(TLeaf root)
	{
		Root = root;
	}

	public ITreeNode<TLeaf> GetRootNode()
	{
		return _rootNode;
	}

	[Obsolete("Use async method instead.", true)]
	public void Rebuild()
	{
		throw new NotImplementedException();
	}

	public async Task RebuildAsync()
	{
		_rootNode = await BuildTreeAsync(Root);
	}

	private async Task<Node> BuildTreeAsync(TLeaf root)
	{
		_existingNodes.Clear();
		return await BuildTreeRecursiveAsync(root);
	}

	private async Task<Node> BuildTreeRecursiveAsync(TLeaf value)
	{
		if (value == null)
		{
			return null;
		}
		if (_existingNodes.ContainsKey(value))
		{
			return _existingNodes[value];
		}
		List<Node> children = new List<Node>();
		foreach (TLeaf item in await TryGetChildrenAsync(value))
		{
			Node node = await BuildTreeRecursiveAsync(item);
			if (node != null)
			{
				children.Add(node);
			}
		}
		Node node2 = new Node
		{
			Value = value,
			Children = children
		};
		_existingNodes.Add(value, node2);
		return node2;
	}

	[Obsolete("Use async method instead.", true)]
	protected virtual bool TryGetChildren(TLeaf node, out IEnumerable<TLeaf> children)
	{
		throw new NotImplementedException();
	}

	protected abstract Task<IEnumerable<TLeaf>> TryGetChildrenAsync(TLeaf node);
}
