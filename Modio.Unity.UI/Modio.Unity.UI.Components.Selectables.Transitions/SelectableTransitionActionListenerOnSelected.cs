using System;
using Modio.Unity.UI.Input;
using UnityEngine;
using UnityEngine.Events;

namespace Modio.Unity.UI.Components.Selectables.Transitions;

[Serializable]
public class SelectableTransitionActionListenerOnSelected : ISelectableTransition, IPropertyMonoBehaviourEvents
{
	[SerializeField]
	private ModioUIInput.ModioAction _inputAction;

	[SerializeField]
	private UnityEvent _onPressed;

	public void OnSelectionStateChanged(IModioUISelectable.SelectionState state, bool instant)
	{
		switch (state)
		{
		case IModioUISelectable.SelectionState.Selected:
			ModioUIInput.AddHandler(_inputAction, ActionPressed);
			break;
		default:
			ModioUIInput.RemoveHandler(_inputAction, ActionPressed);
			break;
		case IModioUISelectable.SelectionState.Pressed:
			break;
		}
	}

	private void ActionPressed()
	{
		_onPressed.Invoke();
	}

	public void Start()
	{
	}

	public void OnDestroy()
	{
		ModioUIInput.RemoveHandler(_inputAction, ActionPressed);
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
		ModioUIInput.RemoveHandler(_inputAction, ActionPressed);
	}
}
