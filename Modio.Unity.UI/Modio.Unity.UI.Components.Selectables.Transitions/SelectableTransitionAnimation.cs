using System;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables.Transitions;

[Serializable]
public class SelectableTransitionAnimation : ISelectableTransition
{
	[SerializeField]
	private Animator _target;

	[SerializeField]
	private AnimationTriggers _animationTriggers;

	public void OnSelectionStateChanged(IModioUISelectable.SelectionState state, bool instant)
	{
		if (!(_target == null) && _target.isActiveAndEnabled && _target.hasBoundPlayables)
		{
			string text = state switch
			{
				IModioUISelectable.SelectionState.Normal => _animationTriggers.normalTrigger, 
				IModioUISelectable.SelectionState.Highlighted => _animationTriggers.highlightedTrigger, 
				IModioUISelectable.SelectionState.Pressed => _animationTriggers.pressedTrigger, 
				IModioUISelectable.SelectionState.Selected => _animationTriggers.selectedTrigger, 
				IModioUISelectable.SelectionState.Disabled => _animationTriggers.disabledTrigger, 
				_ => null, 
			};
			if (!string.IsNullOrEmpty(text))
			{
				_target.ResetTrigger(_animationTriggers.normalTrigger);
				_target.ResetTrigger(_animationTriggers.highlightedTrigger);
				_target.ResetTrigger(_animationTriggers.pressedTrigger);
				_target.ResetTrigger(_animationTriggers.selectedTrigger);
				_target.ResetTrigger(_animationTriggers.disabledTrigger);
				_target.SetTrigger(text);
			}
		}
	}
}
