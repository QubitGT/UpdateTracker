using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class Axis1DSwitch : MonoBehaviour, IAxis1D
{
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	private IActiveState ActiveState;

	[SerializeField]
	[Interface(typeof(IAxis1D), new Type[] { })]
	private UnityEngine.Object _axisWhenActive;

	[SerializeField]
	[Interface(typeof(IAxis1D), new Type[] { })]
	private UnityEngine.Object _axisWhenInactive;

	protected IAxis1D AxisWhenActive;

	protected IAxis1D AxisWhenInactive;

	protected IAxis1D Current
	{
		get
		{
			if (!ActiveState.Active)
			{
				return AxisWhenInactive;
			}
			return AxisWhenActive;
		}
	}

	protected virtual void Awake()
	{
		ActiveState = _activeState as IActiveState;
		AxisWhenActive = _axisWhenActive as IAxis1D;
		AxisWhenInactive = _axisWhenInactive as IAxis1D;
	}

	protected virtual void Start()
	{
	}

	public float Value()
	{
		return Current.Value();
	}

	public void InjectAllAxis1DSwitch(IActiveState activeState, IAxis1D axisWhenActive, IAxis1D axisWhenInactive)
	{
		InjectActiveState(activeState);
		InjectAxisWhenActive(axisWhenActive);
		InjectAxisWhenInactive(axisWhenInactive);
	}

	public void InjectActiveState(IActiveState activeState)
	{
		_activeState = activeState as UnityEngine.Object;
		ActiveState = activeState;
	}

	private void InjectAxisWhenActive(IAxis1D axisWhenActive)
	{
		AxisWhenActive = axisWhenActive;
		_axisWhenActive = axisWhenActive as UnityEngine.Object;
	}

	private void InjectAxisWhenInactive(IAxis1D axisWhenInactive)
	{
		AxisWhenInactive = axisWhenInactive;
		_axisWhenInactive = axisWhenInactive as UnityEngine.Object;
	}
}
