using UnityEngine;

namespace Oculus.Interaction.DebugTree;

public interface INodeUI<TLeaf> where TLeaf : class
{
	RectTransform ChildArea { get; }

	void Bind(ITreeNode<TLeaf> node, bool isRoot, bool isDuplicate);
}
