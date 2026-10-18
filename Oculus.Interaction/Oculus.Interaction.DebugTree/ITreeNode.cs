using System.Collections.Generic;

namespace Oculus.Interaction.DebugTree;

public interface ITreeNode<TLeaf> where TLeaf : class
{
	TLeaf Value { get; }

	IEnumerable<ITreeNode<TLeaf>> Children { get; }
}
