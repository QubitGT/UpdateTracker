using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Interaction.PoseDetection.Debug;
using UnityEngine;

namespace Oculus.Interaction;

public class ActiveStateToggle : MonoBehaviour, IActiveState
{
	public enum StatePrecedence
	{
		On,
		Off
	}

	private class DebugModel : ActiveStateModel<ActiveStateToggle>
	{
		protected override Task<IEnumerable<IActiveState>> GetChildrenAsync(ActiveStateToggle activeState)
		{
			return Task.FromResult((IEnumerable<IActiveState>)new IActiveState[2] { activeState.On, activeState.Off });
		}
	}

	[Tooltip("When this ActiveState is Active, the ActiveStateToggle will be Active.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _on;

	private IActiveState On;

	[Tooltip("When this ActiveState is Inactive, the ActiveStateToggle will be Inactive.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _off;

	private IActiveState Off;

	[Tooltip("If both On and Off conditions are Active simultaneously, this condition will take precedence and dictate the output state.")]
	[SerializeField]
	private StatePrecedence _precedence;

	private bool _internalActive;

	public StatePrecedence Precedence
	{
		get
		{
			return _precedence;
		}
		set
		{
			_precedence = value;
		}
	}

	public bool Active
	{
		get
		{
			if (Precedence == StatePrecedence.Off)
			{
				if (Off.Active)
				{
					_internalActive = false;
				}
				else if (On.Active)
				{
					_internalActive = true;
				}
			}
			else if (Precedence == StatePrecedence.On)
			{
				if (On.Active)
				{
					_internalActive = true;
				}
				else if (Off.Active)
				{
					_internalActive = false;
				}
			}
			if (_internalActive)
			{
				return base.isActiveAndEnabled;
			}
			return false;
		}
	}

	protected virtual void Awake()
	{
		On = _on as IActiveState;
		Off = _off as IActiveState;
	}

	protected virtual void Start()
	{
	}

	static ActiveStateToggle()
	{
	}

	public void InjectAllActiveStateToggle(IActiveState on, IActiveState off)
	{
		InjectOn(on);
		InjectOff(off);
	}

	public void InjectOn(IActiveState activeState)
	{
		_on = activeState as UnityEngine.Object;
		On = activeState;
	}

	public void InjectOff(IActiveState activeState)
	{
		_off = activeState as UnityEngine.Object;
		Off = activeState;
	}
}
