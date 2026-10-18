using System;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables.Transitions;

[Serializable]
public class SelectableTransitionSpriteSwap : ISelectableTransition
{
	[SerializeField]
	private Image _target;

	[SerializeField]
	private SpriteState _spriteState;

	[SerializeField]
	private Sprite _overrideDefault;

	private bool _isInitialised;

	private Sprite _defaultSprite;

	public void OnSelectionStateChanged(IModioUISelectable.SelectionState state, bool instant)
	{
		if (!(_target == null))
		{
			if (!_isInitialised)
			{
				_defaultSprite = _target.sprite;
			}
			_isInitialised = true;
			Image target = _target;
			target.sprite = state switch
			{
				IModioUISelectable.SelectionState.Normal => (_overrideDefault != null) ? _overrideDefault : _defaultSprite, 
				IModioUISelectable.SelectionState.Highlighted => _spriteState.highlightedSprite, 
				IModioUISelectable.SelectionState.Pressed => _spriteState.pressedSprite, 
				IModioUISelectable.SelectionState.Selected => _spriteState.selectedSprite, 
				IModioUISelectable.SelectionState.Disabled => _spriteState.disabledSprite, 
				_ => _defaultSprite, 
			};
		}
	}
}
