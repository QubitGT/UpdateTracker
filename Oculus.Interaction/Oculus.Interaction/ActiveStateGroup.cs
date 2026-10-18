using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Interaction.PoseDetection.Debug;
using UnityEngine;

namespace Oculus.Interaction;

public class ActiveStateGroup : MonoBehaviour, IActiveState
{
	public enum ActiveStateGroupLogicOperator
	{
		AND,
		OR,
		XOR
	}

	private class DebugModel : ActiveStateModel<ActiveStateGroup>
	{
		protected override Task<IEnumerable<IActiveState>> GetChildrenAsync(ActiveStateGroup instance)
		{
			return Task.FromResult((IEnumerable<IActiveState>)instance.ActiveStates);
		}
	}

	[Tooltip("The logic operator will be applied to these IActiveStates.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private List<UnityEngine.Object> _activeStates;

	private List<IActiveState> ActiveStates;

	[Tooltip("IActiveStates will have this boolean logic operator applied.")]
	[SerializeField]
	private ActiveStateGroupLogicOperator _logicOperator;

	public bool Active
	{
		get
		{
			if (ActiveStates == null)
			{
				return false;
			}
			switch (_logicOperator)
			{
			case ActiveStateGroupLogicOperator.AND:
				foreach (IActiveState activeState in ActiveStates)
				{
					if (!activeState.Active)
					{
						return false;
					}
				}
				return true;
			case ActiveStateGroupLogicOperator.OR:
				foreach (IActiveState activeState2 in ActiveStates)
				{
					if (activeState2.Active)
					{
						return true;
					}
				}
				return false;
			case ActiveStateGroupLogicOperator.XOR:
			{
				bool flag = false;
				{
					foreach (IActiveState activeState3 in ActiveStates)
					{
						if (activeState3.Active)
						{
							if (flag)
							{
								return false;
							}
							flag = true;
						}
					}
					return flag;
				}
			}
			default:
				return false;
			}
		}
	}

	protected virtual void Awake()
	{
		ActiveStates = _activeStates.ConvertAll((UnityEngine.Object mono) => mono as IActiveState);
	}

	protected virtual void Start()
	{
	}

	static ActiveStateGroup()
	{
	}

	public void InjectAllActiveStateGroup(List<IActiveState> activeStates)
	{
		InjectActiveStates(activeStates);
	}

	public void InjectActiveStates(List<IActiveState> activeStates)
	{
		ActiveStates = activeStates;
		_activeStates = activeStates.ConvertAll((IActiveState activeState) => activeState as UnityEngine.Object);
	}

	public void InjectOptionalLogicOperator(ActiveStateGroupLogicOperator logicOperator)
	{
		_logicOperator = logicOperator;
	}
}
