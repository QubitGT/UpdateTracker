using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Oculus.Interaction.DebugTree;

public class InteractorGroupDebugTreeUI : DebugTreeUI<IInteractor>
{
	private class InteractorGroupDebugTree : DebugTree<IInteractor>
	{
		public InteractorGroupDebugTree(IInteractor root)
			: base(root)
		{
		}

		protected override Task<IEnumerable<IInteractor>> TryGetChildrenAsync(IInteractor node)
		{
			if (node is InteractorGroup)
			{
				return Task.FromResult((IEnumerable<IInteractor>)(node as InteractorGroup).Interactors);
			}
			return Task.FromResult(Enumerable.Empty<IInteractor>());
		}
	}

	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	private UnityEngine.Object _root;

	[Tooltip("The node prefab which will be used to build the visual tree.")]
	[SerializeField]
	[Interface(typeof(INodeUI<IInteractor>), new Type[] { })]
	private Component _nodePrefab;

	protected override IInteractor Value => _root as IInteractor;

	protected override INodeUI<IInteractor> NodePrefab => _nodePrefab as INodeUI<IInteractor>;

	protected override DebugTree<IInteractor> CreateTree(IInteractor value)
	{
		return new InteractorGroupDebugTree(value);
	}

	protected override string TitleForValue(IInteractor value)
	{
		UnityEngine.Object @object = value as UnityEngine.Object;
		if (!(@object != null))
		{
			return "";
		}
		return @object.name;
	}
}
