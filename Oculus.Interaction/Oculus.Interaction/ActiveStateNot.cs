using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Interaction.PoseDetection.Debug;
using UnityEngine;

namespace Oculus.Interaction;

public class ActiveStateNot : MonoBehaviour, IActiveState
{
	private class DebugModel : ActiveStateModel<ActiveStateNot>
	{
		protected override Task<IEnumerable<IActiveState>> GetChildrenAsync(ActiveStateNot activeState)
		{
			return Task.FromResult((IEnumerable<IActiveState>)new IActiveState[1] { activeState.ActiveState });
		}
	}

	[Tooltip("The IActiveState that the NOT operation will be applied to.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	private IActiveState ActiveState;

	public bool Active => !ActiveState.Active;

	protected virtual void Awake()
	{
		ActiveState = _activeState as IActiveState;
	}

	protected virtual void Start()
	{
	}

	static ActiveStateNot()
	{
	}

	public void InjectAllActiveStateNot(IActiveState activeState)
	{
		InjectActiveState(activeState);
	}

	public void InjectActiveState(IActiveState activeState)
	{
		_activeState = activeState as UnityEngine.Object;
		ActiveState = activeState;
	}
}
