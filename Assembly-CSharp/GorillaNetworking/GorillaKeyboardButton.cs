using TMPro;
using UnityEngine;

namespace GorillaNetworking;

public class GorillaKeyboardButton : GorillaKeyButton<GorillaKeyboardBindings>
{
	protected override void OnButtonPressedEvent()
	{
		GameEvents.OnGorrillaKeyboardButtonPressedEvent.Invoke(Binding);
	}

	protected override void OnEnableEvents()
	{
		base.OnEnableEvents();
		GorillaComputer.RegisterOnKeyLabelColorsChanged(UpdateOptionColor);
		UpdateOptionColor();
	}

	protected override void OnDisableEvents()
	{
		GorillaComputer.UnregisterOnKeyLabelColorsChanged(UpdateOptionColor);
		base.OnDisableEvents();
	}

	private void UpdateOptionColor()
	{
		GorillaComputer instance = GorillaComputer.instance;
		SetUnpressedColorOverride((instance != null && instance.TryGetKeyLabelColor(Binding, out var color)) ? new Color?(color) : GorillaComputer.GetFunctionKeyColor(Binding));
		if (GorillaComputer.GetFunctionKeyColor(Binding).HasValue)
		{
			TextMeshPro textMeshPro = ((ButtonText != null) ? ButtonText : GetComponentInChildren<TextMeshPro>(includeInactive: true));
			if (textMeshPro != null)
			{
				textMeshPro.color = GorillaComputer.FunctionKeyTextColor;
			}
		}
	}
}
