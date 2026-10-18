using Modio.Unity.UI.Components;
using Modio.Unity.UI.Input;
using UnityEngine;

namespace Modio.Unity.UI.Panels;

public class ModFilterPanel : ModioPanelBase
{
	private ModioUIFilterDisplay _filterDisplay;

	protected override void Awake()
	{
		_filterDisplay = GetComponentInChildren<ModioUIFilterDisplay>();
		base.Awake();
	}

	protected override void CancelPressed()
	{
		ClosePanel();
		_filterDisplay.ApplyFilter();
	}

	public override void DoDefaultSelection()
	{
		GameObject defaultSelection = _filterDisplay.GetDefaultSelection();
		if ((bool)defaultSelection)
		{
			SetSelectedGameObject(defaultSelection);
		}
		else
		{
			base.DoDefaultSelection();
		}
	}

	public override void OnGainedFocus(GainedFocusCause selectionBehaviour)
	{
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.Filter, CancelPressed);
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.FilterClear, _filterDisplay.ClearFilter);
		base.OnGainedFocus(selectionBehaviour);
	}

	public override void OnLostFocus()
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Filter, CancelPressed);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.FilterClear, _filterDisplay.ClearFilter);
		base.OnLostFocus();
	}
}
