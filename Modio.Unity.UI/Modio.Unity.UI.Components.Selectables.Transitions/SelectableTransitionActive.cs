using System;
using UnityEngine;

namespace Modio.Unity.UI.Components.Selectables.Transitions;

[Serializable]
public class SelectableTransitionActive : ISelectableTransition
{
	[SerializeField]
	private GameObject _target;

	[SerializeField]
	private bool _normal;

	[SerializeField]
	private bool _highlighted;

	[SerializeField]
	private bool _pressed;

	[SerializeField]
	private bool _selected;

	[SerializeField]
	private bool _disabled;

	public void OnSelectionStateChanged(IModioUISelectable.SelectionState state, bool instant)
	{
		if (!(_target == null))
		{
			GameObject target = _target;
			target.SetActive(state switch
			{
				IModioUISelectable.SelectionState.Normal => _normal, 
				IModioUISelectable.SelectionState.Highlighted => _highlighted, 
				IModioUISelectable.SelectionState.Pressed => _pressed, 
				IModioUISelectable.SelectionState.Selected => _selected, 
				IModioUISelectable.SelectionState.Disabled => _disabled, 
				_ => _normal, 
			});
		}
	}
}
