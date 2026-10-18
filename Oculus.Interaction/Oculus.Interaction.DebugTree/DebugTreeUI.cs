using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Oculus.Interaction.DebugTree;

public abstract class DebugTreeUI<TLeaf> : MonoBehaviour where TLeaf : class
{
	[Tooltip("Node prefabs will be instantiated inside of this content area.")]
	[SerializeField]
	private RectTransform _contentArea;

	[Tooltip("This title text will display the GameObject name of the IActiveState.")]
	[SerializeField]
	[Optional]
	private TMP_Text _title;

	[Tooltip("If true, the tree UI will be built on Start.")]
	[SerializeField]
	private bool _buildTreeOnStart;

	private DebugTree<TLeaf> _tree;

	private Dictionary<ITreeNode<TLeaf>, INodeUI<TLeaf>> _nodeToUI = new Dictionary<ITreeNode<TLeaf>, INodeUI<TLeaf>>();

	protected abstract TLeaf Value { get; }

	protected abstract INodeUI<TLeaf> NodePrefab { get; }

	protected virtual void Start()
	{
		if (_buildTreeOnStart)
		{
			StartCoroutine(BuildTree());
		}
	}

	public IEnumerator BuildTree()
	{
		_tree = CreateTree(Value);
		Task task = _tree.RebuildAsync();
		yield return new WaitUntil(() => task.IsCompleted);
		_nodeToUI.Clear();
		ClearContentArea();
		SetTitleText();
		BuildTreeRecursive(_contentArea, _tree.GetRootNode(), isRoot: true);
	}

	private void BuildTreeRecursive(RectTransform parent, ITreeNode<TLeaf> node, bool isRoot)
	{
		INodeUI<TLeaf> nodeUI = Object.Instantiate(NodePrefab as Object, parent) as INodeUI<TLeaf>;
		bool flag = _nodeToUI.ContainsKey(node);
		nodeUI.Bind(node, isRoot, flag);
		if (flag)
		{
			return;
		}
		_nodeToUI.Add(node, nodeUI);
		foreach (ITreeNode<TLeaf> child in node.Children)
		{
			BuildTreeRecursive(nodeUI.ChildArea, child, isRoot: false);
		}
	}

	private void ClearContentArea()
	{
		for (int i = 0; i < _contentArea.childCount; i++)
		{
			Transform child = _contentArea.GetChild(i);
			if (child != null && child.TryGetComponent<INodeUI<TLeaf>>(out var _))
			{
				Object.Destroy(child.gameObject);
			}
		}
	}

	private void SetTitleText()
	{
		if (_title != null)
		{
			_title.text = TitleForValue(Value);
		}
	}

	protected abstract DebugTree<TLeaf> CreateTree(TLeaf value);

	protected abstract string TitleForValue(TLeaf value);
}
