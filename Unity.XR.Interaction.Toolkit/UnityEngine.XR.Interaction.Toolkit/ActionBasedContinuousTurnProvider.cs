using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Locomotion/Legacy/Continuous Turn Provider (Action-based)", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousTurnProvider.html")]
[Obsolete("ActionBasedContinuousTurnProvider has been deprecated in version 3.0.0. Use ContinuousTurnProvider instead.")]
public class ActionBasedContinuousTurnProvider : ContinuousTurnProviderBase
{
	[SerializeField]
	[Tooltip("The Input System Action that will be used to read Turn data from the left hand controller. Must be a Value Vector2 Control.")]
	private InputActionProperty m_LeftHandTurnAction = new InputActionProperty(new InputAction("Left Hand Turn", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[Tooltip("The Input System Action that will be used to read Turn data from the right hand controller. Must be a Value Vector2 Control.")]
	private InputActionProperty m_RightHandTurnAction = new InputActionProperty(new InputAction("Right Hand Turn", InputActionType.Value, null, null, null, "Vector2"));

	public InputActionProperty leftHandTurnAction
	{
		get
		{
			return m_LeftHandTurnAction;
		}
		set
		{
			SetInputActionProperty(ref m_LeftHandTurnAction, value);
		}
	}

	public InputActionProperty rightHandTurnAction
	{
		get
		{
			return m_RightHandTurnAction;
		}
		set
		{
			SetInputActionProperty(ref m_RightHandTurnAction, value);
		}
	}

	protected void OnEnable()
	{
		m_LeftHandTurnAction.EnableDirectAction();
		m_RightHandTurnAction.EnableDirectAction();
	}

	protected void OnDisable()
	{
		m_LeftHandTurnAction.DisableDirectAction();
		m_RightHandTurnAction.DisableDirectAction();
	}

	protected override Vector2 ReadInput()
	{
		Vector2 obj = m_LeftHandTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
		Vector2 vector = m_RightHandTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
		return obj + vector;
	}

	private void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
	{
		if (Application.isPlaying)
		{
			property.DisableDirectAction();
		}
		property = value;
		if (Application.isPlaying && base.isActiveAndEnabled)
		{
			property.EnableDirectAction();
		}
	}
}
