using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables.Transitions;

[Serializable]
public class SelectableTransitionColorTint : ISelectableTransition
{
	[SerializeField]
	private Graphic _target;

	[SerializeField]
	private ColorBlock _colorBlock = ColorBlock.defaultColorBlock;

	private Coroutine _coroutine;

	public void OnSelectionStateChanged(IModioUISelectable.SelectionState state, bool instant)
	{
		if (_target == null)
		{
			return;
		}
		Color color = state switch
		{
			IModioUISelectable.SelectionState.Normal => _colorBlock.normalColor, 
			IModioUISelectable.SelectionState.Highlighted => _colorBlock.highlightedColor, 
			IModioUISelectable.SelectionState.Pressed => _colorBlock.pressedColor, 
			IModioUISelectable.SelectionState.Selected => _colorBlock.selectedColor, 
			IModioUISelectable.SelectionState.Disabled => _colorBlock.disabledColor, 
			_ => _colorBlock.normalColor, 
		};
		if (_target.gameObject.activeInHierarchy)
		{
			if (_coroutine != null)
			{
				_target.StopCoroutine(_coroutine);
			}
			_coroutine = _target.StartCoroutine(CrossFadeColor(color, (!instant) ? _colorBlock.fadeDuration : 0f));
		}
		else
		{
			_target.color = color;
		}
	}

	private IEnumerator CrossFadeColor(Color targetColor, float duration)
	{
		Color startColor = _target.color;
		for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
		{
			_target.color = Color.Lerp(startColor, targetColor, t);
			yield return null;
		}
		_target.color = targetColor;
		_coroutine = null;
	}
}
