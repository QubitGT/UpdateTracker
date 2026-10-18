using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Locomotion/Legacy/Snap Turn Provider (Action-based)", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedSnapTurnProvider.html")]
[Obsolete("ActionBasedSnapTurnProvider has been deprecated in version 3.0.0. Use SnapTurnProvider instead.")]
public class ActionBasedSnapTurnProvider : SnapTurnProviderBase
{
	[SerializeField]
	[Tooltip("The Input System Action that will be used to read Snap Turn data from the left hand controller. Must be a Value Vector2 Control.")]
	private InputActionProperty m_LeftHandSnapTurnAction = new InputActionProperty(new InputAction("Left Hand Snap Turn", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[Tooltip("The Input System Action that will be used to read Snap Turn data from the right hand controller. Must be a Value Vector2 Control.")]
	private InputActionProperty m_RightHandSnapTurnAction = new InputActionProperty(new InputAction("Right Hand Snap Turn", InputActionType.Value, null, null, null, "Vector2"));

	public InputActionProperty leftHandSnapTurnAction
	{
		get
		{
			return m_LeftHandSnapTurnAction;
		}
		set
		{
			SetInputActionProperty(ref m_LeftHandSnapTurnAction, value);
		}
	}

	public InputActionProperty rightHandSnapTurnAction
	{
		get
		{
			return m_RightHandSnapTurnAction;
		}
		set
		{
			SetInputActionProperty(ref m_RightHandSnapTurnAction, value);
		}
	}

	protected void OnEnable()
	{
		m_LeftHandSnapTurnAction.EnableDirectAction();
		m_RightHandSnapTurnAction.EnableDirectAction();
	}

	protected void OnDisable()
	{
		m_LeftHandSnapTurnAction.DisableDirectAction();
		m_RightHandSnapTurnAction.DisableDirectAction();
	}

	protected override Vector2 ReadInput()
	{
		Vector2 obj = m_LeftHandSnapTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
		Vector2 vector = m_RightHandSnapTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
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
