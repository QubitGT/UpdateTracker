using System;
using Oculus.Interaction.DebugTree;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class ActiveStateDebugTreeUI : DebugTreeUI<IActiveState>
{
	[Tooltip("The IActiveState to debug.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	[Tooltip("The node prefab which will be used to build the visual tree.")]
	[SerializeField]
	[Interface(typeof(INodeUI<IActiveState>), new Type[] { })]
	private Component _nodePrefab;

	protected override IActiveState Value => _activeState as IActiveState;

	protected override INodeUI<IActiveState> NodePrefab => _nodePrefab as INodeUI<IActiveState>;

	protected override DebugTree<IActiveState> CreateTree(IActiveState value)
	{
		return new ActiveStateDebugTree(value);
	}

	protected override string TitleForValue(IActiveState value)
	{
		UnityEngine.Object @object = value as UnityEngine.Object;
		if (!(@object != null))
		{
			return "";
		}
		return @object.name;
	}
}
