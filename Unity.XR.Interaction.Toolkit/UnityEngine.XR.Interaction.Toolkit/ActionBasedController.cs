using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("/XR Controller (Action-based)", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedController.html")]
[Obsolete("ActionBasedController has been deprecated in version 3.0.0. Its functionality has been distributed into different components.")]
public class ActionBasedController : XRBaseController
{
	[SerializeField]
	private InputActionProperty m_PositionAction = new InputActionProperty(new InputAction("Position", InputActionType.Value, null, null, null, "Vector3"));

	[SerializeField]
	private InputActionProperty m_RotationAction = new InputActionProperty(new InputAction("Rotation", InputActionType.Value, null, null, null, "Quaternion"));

	[SerializeField]
	private InputActionProperty m_IsTrackedAction = new InputActionProperty(new InputAction("Is Tracked", InputActionType.Button)
	{
		wantsInitialStateCheck = true
	});

	[SerializeField]
	private InputActionProperty m_TrackingStateAction = new InputActionProperty(new InputAction("Tracking State", InputActionType.Value, null, null, null, "Integer"));

	[SerializeField]
	private InputActionProperty m_SelectAction = new InputActionProperty(new InputAction("Select", InputActionType.Button));

	[SerializeField]
	private InputActionProperty m_SelectActionValue = new InputActionProperty(new InputAction("Select Value", InputActionType.Value, null, null, null, "Axis"));

	[SerializeField]
	private InputActionProperty m_ActivateAction = new InputActionProperty(new InputAction("Activate", InputActionType.Button));

	[SerializeField]
	private InputActionProperty m_ActivateActionValue = new InputActionProperty(new InputAction("Activate Value", InputActionType.Value, null, null, null, "Axis"));

	[SerializeField]
	private InputActionProperty m_UIPressAction = new InputActionProperty(new InputAction("UI Press", InputActionType.Button));

	[SerializeField]
	private InputActionProperty m_UIPressActionValue = new InputActionProperty(new InputAction("UI Press Value", InputActionType.Value, null, null, null, "Axis"));

	[SerializeField]
	private InputActionProperty m_UIScrollAction = new InputActionProperty(new InputAction("UI Scroll", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	private InputActionProperty m_HapticDeviceAction = new InputActionProperty(new InputAction("Haptic Device", InputActionType.PassThrough));

	[SerializeField]
	private InputActionProperty m_RotateAnchorAction = new InputActionProperty(new InputAction("Rotate Anchor", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	private InputActionProperty m_DirectionalAnchorRotationAction = new InputActionProperty(new InputAction("Directional Anchor Rotation", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	private InputActionProperty m_TranslateAnchorAction = new InputActionProperty(new InputAction("Translate Anchor", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	private InputActionProperty m_ScaleToggleAction = new InputActionProperty(new InputAction("Scale Toggle", InputActionType.Button));

	[SerializeField]
	private InputActionProperty m_ScaleDeltaAction = new InputActionProperty(new InputAction("Scale Delta", InputActionType.Value, null, null, null, "Vector2"));

	private bool m_HasCheckedDisabledTrackingInputReferenceActions;

	private bool m_HasCheckedDisabledInputReferenceActions;

	private readonly HapticControlActionManager m_HapticControlActionManager = new HapticControlActionManager();

	[Obsolete("Deprecated, this obsolete property is not used when Input System version is 1.1.0 or higher. Configure press point on the action or binding instead.", true)]
	public float buttonPressPoint
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public InputActionProperty positionAction
	{
		get
		{
			return m_PositionAction;
		}
		set
		{
			SetInputActionProperty(ref m_PositionAction, value);
		}
	}

	public InputActionProperty rotationAction
	{
		get
		{
			return m_RotationAction;
		}
		set
		{
			SetInputActionProperty(ref m_RotationAction, value);
		}
	}

	public InputActionProperty isTrackedAction
	{
		get
		{
			return m_IsTrackedAction;
		}
		set
		{
			SetInputActionProperty(ref m_IsTrackedAction, value);
		}
	}

	public InputActionProperty trackingStateAction
	{
		get
		{
			return m_TrackingStateAction;
		}
		set
		{
			SetInputActionProperty(ref m_TrackingStateAction, value);
		}
	}

	public InputActionProperty selectAction
	{
		get
		{
			return m_SelectAction;
		}
		set
		{
			SetInputActionProperty(ref m_SelectAction, value);
		}
	}

	public InputActionProperty selectActionValue
	{
		get
		{
			return m_SelectActionValue;
		}
		set
		{
			SetInputActionProperty(ref m_SelectActionValue, value);
		}
	}

	public InputActionProperty activateAction
	{
		get
		{
			return m_ActivateAction;
		}
		set
		{
			SetInputActionProperty(ref m_ActivateAction, value);
		}
	}

	public InputActionProperty activateActionValue
	{
		get
		{
			return m_ActivateActionValue;
		}
		set
		{
			SetInputActionProperty(ref m_ActivateActionValue, value);
		}
	}

	public InputActionProperty uiPressAction
	{
		get
		{
			return m_UIPressAction;
		}
		set
		{
			SetInputActionProperty(ref m_UIPressAction, value);
		}
	}

	public InputActionProperty uiPressActionValue
	{
		get
		{
			return m_UIPressActionValue;
		}
		set
		{
			SetInputActionProperty(ref m_UIPressActionValue, value);
		}
	}

	public InputActionProperty uiScrollAction
	{
		get
		{
			return m_UIScrollAction;
		}
		set
		{
			SetInputActionProperty(ref m_UIScrollAction, value);
		}
	}

	public InputActionProperty hapticDeviceAction
	{
		get
		{
			return m_HapticDeviceAction;
		}
		set
		{
			SetInputActionProperty(ref m_HapticDeviceAction, value);
		}
	}

	public InputActionProperty rotateAnchorAction
	{
		get
		{
			return m_RotateAnchorAction;
		}
		set
		{
			SetInputActionProperty(ref m_RotateAnchorAction, value);
		}
	}

	public InputActionProperty directionalAnchorRotationAction
	{
		get
		{
			return m_DirectionalAnchorRotationAction;
		}
		set
		{
			SetInputActionProperty(ref m_DirectionalAnchorRotationAction, value);
		}
	}

	public InputActionProperty translateAnchorAction
	{
		get
		{
			return m_TranslateAnchorAction;
		}
		set
		{
			SetInputActionProperty(ref m_TranslateAnchorAction, value);
		}
	}

	public InputActionProperty scaleToggleAction
	{
		get
		{
			return m_ScaleToggleAction;
		}
		set
		{
			SetInputActionProperty(ref m_ScaleToggleAction, value);
		}
	}

	public InputActionProperty scaleDeltaAction
	{
		get
		{
			return m_ScaleDeltaAction;
		}
		set
		{
			SetInputActionProperty(ref m_ScaleDeltaAction, value);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		EnableAllDirectActions();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		DisableAllDirectActions();
	}

	protected override void UpdateTrackingInput(XRControllerState controllerState)
	{
		base.UpdateTrackingInput(controllerState);
		if (controllerState == null)
		{
			return;
		}
		InputAction action = m_PositionAction.action;
		InputAction action2 = m_RotationAction.action;
		InputAction action3 = m_IsTrackedAction.action;
		InputAction action4 = m_TrackingStateAction.action;
		if (!m_HasCheckedDisabledTrackingInputReferenceActions && (action != null || action2 != null))
		{
			if (IsDisabledReferenceAction(m_PositionAction) || IsDisabledReferenceAction(m_RotationAction))
			{
				Debug.LogWarning("'Enable Input Tracking' is enabled, but Position and/or Rotation Action is disabled. The pose of the controller will not be updated correctly until the Input Actions are enabled. Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action. The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.", this);
			}
			m_HasCheckedDisabledTrackingInputReferenceActions = true;
		}
		controllerState.isTracked = false;
		controllerState.inputTrackingState = InputTrackingState.None;
		if (action3 != null && action3.bindings.Count > 0)
		{
			controllerState.isTracked = IsPressed(action3);
		}
		else if (action4?.activeControl?.device is TrackedDevice trackedDevice)
		{
			controllerState.isTracked = trackedDevice.isTracked.isPressed;
		}
		else
		{
			TrackedDevice obj = action?.activeControl?.device as TrackedDevice;
			TrackedDevice trackedDevice2 = action2?.activeControl?.device as TrackedDevice;
			bool flag = obj?.isTracked.isPressed ?? false;
			if (obj != trackedDevice2)
			{
				bool flag2 = trackedDevice2?.isTracked.isPressed ?? false;
				controllerState.isTracked = flag && flag2;
			}
			else
			{
				controllerState.isTracked = flag;
			}
		}
		if (action4 != null && action4.bindings.Count > 0)
		{
			controllerState.inputTrackingState = (InputTrackingState)action4.ReadValue<int>();
		}
		else if (action3?.activeControl?.device is TrackedDevice trackedDevice3)
		{
			controllerState.inputTrackingState = (InputTrackingState)trackedDevice3.trackingState.ReadValue();
		}
		else
		{
			TrackedDevice trackedDevice4 = action?.activeControl?.device as TrackedDevice;
			TrackedDevice trackedDevice5 = action2?.activeControl?.device as TrackedDevice;
			InputTrackingState inputTrackingState = (InputTrackingState)(trackedDevice4?.trackingState.ReadValue() ?? 0);
			if (trackedDevice4 != trackedDevice5)
			{
				InputTrackingState inputTrackingState2 = (InputTrackingState)(trackedDevice5?.trackingState.ReadValue() ?? 0);
				controllerState.inputTrackingState = (inputTrackingState & InputTrackingState.Position) | (inputTrackingState2 & InputTrackingState.Rotation);
			}
			else
			{
				controllerState.inputTrackingState = inputTrackingState;
			}
		}
		if (action != null && (controllerState.inputTrackingState & InputTrackingState.Position) != 0)
		{
			controllerState.position = action.ReadValue<Vector3>();
		}
		if (action2 != null && (controllerState.inputTrackingState & InputTrackingState.Rotation) != 0)
		{
			controllerState.rotation = action2.ReadValue<Quaternion>();
		}
	}

	protected override void UpdateInput(XRControllerState controllerState)
	{
		base.UpdateInput(controllerState);
		if (controllerState == null)
		{
			return;
		}
		if (!m_HasCheckedDisabledInputReferenceActions && (m_SelectAction.action != null || m_ActivateAction.action != null || m_UIPressAction.action != null))
		{
			if (IsDisabledReferenceAction(m_SelectAction) || IsDisabledReferenceAction(m_ActivateAction) || IsDisabledReferenceAction(m_UIPressAction))
			{
				Debug.LogWarning("'Enable Input Actions' is enabled, but Select, Activate, and/or UI Press Action is disabled. The controller input will not be handled correctly until the Input Actions are enabled. Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action. The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.", this);
			}
			m_HasCheckedDisabledInputReferenceActions = true;
		}
		controllerState.ResetFrameDependentStates();
		InputAction action = m_SelectActionValue.action;
		if (action == null || action.bindings.Count <= 0)
		{
			action = m_SelectAction.action;
		}
		controllerState.selectInteractionState.SetFrameState(IsPressed(m_SelectAction.action), ReadValue(action));
		InputAction action2 = m_ActivateActionValue.action;
		if (action2 == null || action2.bindings.Count <= 0)
		{
			action2 = m_ActivateAction.action;
		}
		controllerState.activateInteractionState.SetFrameState(IsPressed(m_ActivateAction.action), ReadValue(action2));
		InputAction action3 = m_UIPressActionValue.action;
		if (action3 == null || action3.bindings.Count <= 0)
		{
			action3 = m_UIPressAction.action;
		}
		controllerState.uiPressInteractionState.SetFrameState(IsPressed(m_UIPressAction.action), ReadValue(action3));
		InputAction action4 = m_UIScrollAction.action;
		if (action4 != null)
		{
			controllerState.uiScrollValue = action4.ReadValue<Vector2>();
		}
	}

	protected virtual bool IsPressed(InputAction action)
	{
		if (action == null)
		{
			return false;
		}
		return action.phase switch
		{
			InputActionPhase.Disabled => false, 
			InputActionPhase.Performed => true, 
			_ => action.WasPerformedThisFrame(), 
		};
	}

	protected virtual float ReadValue(InputAction action)
	{
		if (action == null)
		{
			return 0f;
		}
		if (action.activeControl is AxisControl)
		{
			return action.ReadValue<float>();
		}
		if (action.activeControl is Vector2Control)
		{
			return action.ReadValue<Vector2>().magnitude;
		}
		if (!IsPressed(action))
		{
			return 0f;
		}
		return 1f;
	}

	public override bool SendHapticImpulse(float amplitude, float duration)
	{
		return (m_HapticControlActionManager.GetChannelGroup(m_HapticDeviceAction.action)?.GetChannel()?.SendHapticImpulse(amplitude, duration)).GetValueOrDefault();
	}

	private void EnableAllDirectActions()
	{
		m_PositionAction.EnableDirectAction();
		m_RotationAction.EnableDirectAction();
		m_IsTrackedAction.EnableDirectAction();
		m_TrackingStateAction.EnableDirectAction();
		m_SelectAction.EnableDirectAction();
		m_SelectActionValue.EnableDirectAction();
		m_ActivateAction.EnableDirectAction();
		m_ActivateActionValue.EnableDirectAction();
		m_UIPressAction.EnableDirectAction();
		m_UIPressActionValue.EnableDirectAction();
		m_UIScrollAction.EnableDirectAction();
		m_HapticDeviceAction.EnableDirectAction();
		m_RotateAnchorAction.EnableDirectAction();
		m_DirectionalAnchorRotationAction.EnableDirectAction();
		m_TranslateAnchorAction.EnableDirectAction();
		m_ScaleToggleAction.EnableDirectAction();
		m_ScaleDeltaAction.EnableDirectAction();
	}

	private void DisableAllDirectActions()
	{
		m_PositionAction.DisableDirectAction();
		m_RotationAction.DisableDirectAction();
		m_IsTrackedAction.DisableDirectAction();
		m_TrackingStateAction.DisableDirectAction();
		m_SelectAction.DisableDirectAction();
		m_SelectActionValue.DisableDirectAction();
		m_ActivateAction.DisableDirectAction();
		m_ActivateActionValue.DisableDirectAction();
		m_UIPressAction.DisableDirectAction();
		m_UIPressActionValue.DisableDirectAction();
		m_UIScrollAction.DisableDirectAction();
		m_HapticDeviceAction.DisableDirectAction();
		m_RotateAnchorAction.DisableDirectAction();
		m_DirectionalAnchorRotationAction.DisableDirectAction();
		m_TranslateAnchorAction.DisableDirectAction();
		m_ScaleToggleAction.DisableDirectAction();
		m_ScaleDeltaAction.DisableDirectAction();
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

	private static bool IsDisabledReferenceAction(InputActionProperty property)
	{
		if (property.reference != null && property.reference.action != null)
		{
			return !property.reference.action.enabled;
		}
		return false;
	}
}
