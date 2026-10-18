using Modio.Unity.UI.Components;
using Modio.Unity.UI.Components.Selectables;
using UnityEngine;

namespace Modio.Unity.UI.Panels;

public class ModOptionsPopupPanel : ModioPanelBase
{
	private ModioUIMod _modioUIMod;

	private RectTransform _rectToPosition;

	private RectTransform _rectToPositionWithin;

	private ModioPopupPositioning _popupPositioning;

	private ModioUIButton _buttonToHighlight;

	protected override void Awake()
	{
		base.Awake();
		_modioUIMod = GetComponent<ModioUIMod>();
	}

	public void OpenPanel(ModioUIMod modUI)
	{
		OpenPanel();
		if (_popupPositioning == null)
		{
			_popupPositioning = GetComponentInChildren<ModioPopupPositioning>();
		}
		_modioUIMod.SetMod(modUI.Mod);
		ModioUIButton component = modUI.GetComponent<ModioUIButton>();
		_buttonToHighlight = component;
		_popupPositioning.PositionNextTo((RectTransform)modUI.transform);
	}

	public override void OnLostFocus()
	{
		if (_buttonToHighlight != null)
		{
			_buttonToHighlight.DoVisualOnlyStateTransition(IModioUISelectable.SelectionState.Normal, instant: false);
		}
		base.OnLostFocus();
	}

	public override void FocusedPanelLateUpdate()
	{
		base.FocusedPanelLateUpdate();
		if (_buttonToHighlight != null)
		{
			_buttonToHighlight.DoVisualOnlyStateTransition(IModioUISelectable.SelectionState.Highlighted, instant: false);
		}
	}
}
