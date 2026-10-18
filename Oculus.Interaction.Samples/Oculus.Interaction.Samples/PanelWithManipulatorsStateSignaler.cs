using System;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class PanelWithManipulatorsStateSignaler : MonoBehaviour
{
	public enum State
	{
		Default,
		Selected,
		Idle
	}

	private State _state;

	public State CurrentState
	{
		get
		{
			return _state;
		}
		set
		{
			if (value != _state)
			{
				_state = value;
				this.WhenStateChanged(_state);
			}
		}
	}

	public event Action<State> WhenStateChanged = delegate
	{
	};
}
