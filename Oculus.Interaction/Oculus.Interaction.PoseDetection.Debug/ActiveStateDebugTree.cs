using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Oculus.Interaction.DebugTree;

namespace Oculus.Interaction.PoseDetection.Debug;

public class ActiveStateDebugTree : DebugTree<IActiveState>
{
	private static Dictionary<Type, IActiveStateModel> _models = new Dictionary<Type, IActiveStateModel>();

	public ActiveStateDebugTree(IActiveState root)
		: base(root)
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void RegisterModel<TType>(IActiveStateModel stateModel) where TType : class, IActiveState
	{
		_models[typeof(TType)] = stateModel;
	}

	protected override async Task<IEnumerable<IActiveState>> TryGetChildrenAsync(IActiveState node)
	{
		if (_models.TryGetValue(node.GetType(), out var value))
		{
			return await value.GetChildrenAsync(node);
		}
		return Enumerable.Empty<IActiveState>();
	}
}
