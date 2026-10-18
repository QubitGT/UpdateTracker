using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Locomotion/Legacy/Continuous Move Provider (Action-based)", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider.html")]
[Obsolete("ActionBasedContinuousMoveProvider has been deprecated in version 3.0.0. Use ContinuousMoveProvider instead.", false)]
public class ActionBasedContinuousMoveProvider : ContinuousMoveProviderBase
{
	[SerializeField]
	[Tooltip("The Input System Action that will be used to read Move data from the left hand controller. Must be a Value Vector2 Control.")]
	private InputActionProperty m_LeftHandMoveAction = new InputActionProperty(new InputAction("Left Hand Move", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[Tooltip("The Input System Action that will be used to read Move data from the right hand controller. Must be a Value Vector2 Control.")]
	private InputActionProperty m_RightHandMoveAction = new InputActionProperty(new InputAction("Right Hand Move", InputActionType.Value, null, null, null, "Vector2"));

	public InputActionProperty leftHandMoveAction
	{
		get
		{
			return m_LeftHandMoveAction;
		}
		set
		{
			SetInputActionProperty(ref m_LeftHandMoveAction, value);
		}
	}

	public InputActionProperty rightHandMoveAction
	{
		get
		{
			return m_RightHandMoveAction;
		}
		set
		{
			SetInputActionProperty(ref m_RightHandMoveAction, value);
		}
	}

	protected void OnEnable()
	{
		m_LeftHandMoveAction.EnableDirectAction();
		m_RightHandMoveAction.EnableDirectAction();
	}

	protected void OnDisable()
	{
		m_LeftHandMoveAction.DisableDirectAction();
		m_RightHandMoveAction.DisableDirectAction();
	}

	protected override Vector2 ReadInput()
	{
		Vector2 obj = m_LeftHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
		Vector2 vector = m_RightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
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
