using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Meta.XR.BuildingBlocks;

public class ControllerButtonsMapper : MonoBehaviour
{
	[Serializable]
	public struct ButtonClickAction
	{
		public enum ButtonClickMode
		{
			OnButtonUp,
			OnButtonDown,
			OnButton
		}

		public string Title;

		public OVRInput.Button Button;

		public ButtonClickMode ButtonMode;

		public InputActionReference InputActionReference;

		public UnityEvent<InputAction.CallbackContext> CallbackWithContext;

		public UnityEvent Callback;

		public void OnCallbackWithContext(InputAction.CallbackContext callbackContext)
		{
			CallbackWithContext?.Invoke(callbackContext);
		}
	}

	[SerializeField]
	private List<ButtonClickAction> _buttonClickActions;

	internal const bool UseNewInputSystem = true;

	internal const bool UseLegacyInputSystem = false;

	public List<ButtonClickAction> ButtonClickActions
	{
		get
		{
			return _buttonClickActions;
		}
		set
		{
			_buttonClickActions = value;
		}
	}

	private void OnEnable()
	{
		foreach (ButtonClickAction buttonClickAction in ButtonClickActions)
		{
			if (!(buttonClickAction.InputActionReference == null))
			{
				buttonClickAction.InputActionReference.action.Enable();
				buttonClickAction.InputActionReference.action.performed += buttonClickAction.OnCallbackWithContext;
			}
		}
	}

	private void OnDisable()
	{
		foreach (ButtonClickAction buttonClickAction in ButtonClickActions)
		{
			if (!(buttonClickAction.InputActionReference == null))
			{
				buttonClickAction.InputActionReference.action.Disable();
				buttonClickAction.InputActionReference.action.performed -= buttonClickAction.OnCallbackWithContext;
			}
		}
	}

	private void Update()
	{
		foreach (ButtonClickAction buttonClickAction in ButtonClickActions)
		{
			if (IsActionTriggered(buttonClickAction))
			{
				buttonClickAction.Callback?.Invoke();
			}
		}
	}

	private static bool IsActionTriggered(ButtonClickAction buttonClickAction)
	{
		if (!IsLegacyInputActionTriggered(buttonClickAction.ButtonMode, buttonClickAction.Button))
		{
			return IsNewInputSystemActionTriggered(buttonClickAction);
		}
		return true;
	}

	private static bool IsLegacyInputActionTriggered(ButtonClickAction.ButtonClickMode buttonMode, OVRInput.Button button)
	{
		return false;
	}

	private static bool IsNewInputSystemActionTriggered(ButtonClickAction buttonClickAction)
	{
		if (buttonClickAction.InputActionReference != null)
		{
			return buttonClickAction.InputActionReference.action.triggered;
		}
		return false;
	}
}
