using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/XR Screen Space Controller", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRScreenSpaceController.html")]
[Obsolete("XRScreenSpaceController has been deprecated in version 3.0.0. Its functionality has been distributed into different components.")]
public class XRScreenSpaceController : XRBaseController
{
	[Header("Touchscreen Gesture Actions")]
	[SerializeField]
	[Tooltip("When enabled, a Touchscreen Gesture Input Controller will be added to the Input System device list to detect touch gestures.")]
	private bool m_EnableTouchscreenGestureInputController = true;

	[SerializeField]
	[Tooltip("The action to use for the screen tap position. (Vector 2 Control).")]
	private InputActionProperty m_TapStartPositionAction = new InputActionProperty(new InputAction("Tap Start Position", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[Tooltip("The action to use for the current screen drag position. (Vector 2 Control).")]
	private InputActionProperty m_DragCurrentPositionAction = new InputActionProperty(new InputAction("Drag Current Position", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[Tooltip("The action to use for the delta of the screen drag. (Vector 2 Control).")]
	private InputActionProperty m_DragDeltaAction = new InputActionProperty(new InputAction("Drag Delta", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[FormerlySerializedAs("m_PinchStartPosition")]
	[Tooltip("The action to use for the screen pinch gesture start position. (Vector 2 Control).")]
	private InputActionProperty m_PinchStartPositionAction = new InputActionProperty(new InputAction("Pinch Start Position", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[Tooltip("The action to use for the gap of the screen pinch gesture. (Axis Control).")]
	private InputActionProperty m_PinchGapAction = new InputActionProperty(new InputAction(null, InputActionType.Value, null, null, null, "Axis"));

	[SerializeField]
	[Tooltip("The action to use for the delta of the screen pinch gesture. (Axis Control).")]
	private InputActionProperty m_PinchGapDeltaAction = new InputActionProperty(new InputAction("Pinch Gap Delta", InputActionType.Value, null, null, null, "Axis"));

	[SerializeField]
	[FormerlySerializedAs("m_TwistStartPosition")]
	[Tooltip("The action to use for the screen twist gesture start position. (Vector 2 Control).")]
	private InputActionProperty m_TwistStartPositionAction = new InputActionProperty(new InputAction("Twist Start Position", InputActionType.Value, null, null, null, "Vector2"));

	[SerializeField]
	[FormerlySerializedAs("m_TwistRotationDeltaAction")]
	[Tooltip("The action to use for the delta of the screen twist gesture. (Axis Control).")]
	private InputActionProperty m_TwistDeltaRotationAction = new InputActionProperty(new InputAction("Twist Delta Rotation", InputActionType.Value, null, null, null, "Axis"));

	[SerializeField]
	[FormerlySerializedAs("m_ScreenTouchCount")]
	[Tooltip("The number of concurrent touches on the screen. (Integer Control).")]
	private InputActionProperty m_ScreenTouchCountAction = new InputActionProperty(new InputAction("Screen Touch Count", InputActionType.Value, null, null, null, "Integer"));

	[SerializeField]
	[Tooltip("The camera associated with the screen, and through which screen presses/touches will be interpreted.")]
	private Camera m_ControllerCamera;

	[SerializeField]
	[Tooltip("Tells the XR Screen Space Controller to ignore interactions when hitting a screen space canvas.")]
	private bool m_BlockInteractionsWithScreenSpaceUI = true;

	[SerializeField]
	[Tooltip("Enables a rotation threshold that blocks pinch scale gestures when surpassed.")]
	private bool m_UseRotationThreshold = true;

	[SerializeField]
	[Tooltip("The threshold at which a gestures will be interpreted only as rotation and not a pinch scale gesture.")]
	private float m_RotationThreshold = 0.02f;

	private bool m_HasCheckedDisabledTrackingInputReferenceActions;

	private bool m_HasCheckedDisabledInputReferenceActions;

	private UIInputModule m_UIInputModule;

	public bool enableTouchscreenGestureInputController
	{
		get
		{
			return m_EnableTouchscreenGestureInputController;
		}
		set
		{
			m_EnableTouchscreenGestureInputController = value;
		}
	}

	public InputActionProperty tapStartPositionAction
	{
		get
		{
			return m_TapStartPositionAction;
		}
		set
		{
			SetInputActionProperty(ref m_TapStartPositionAction, value);
		}
	}

	public InputActionProperty dragCurrentPositionAction
	{
		get
		{
			return m_DragCurrentPositionAction;
		}
		set
		{
			SetInputActionProperty(ref m_DragCurrentPositionAction, value);
		}
	}

	public InputActionProperty dragDeltaAction
	{
		get
		{
			return m_DragDeltaAction;
		}
		set
		{
			SetInputActionProperty(ref m_DragDeltaAction, value);
		}
	}

	public InputActionProperty pinchStartPositionAction
	{
		get
		{
			return m_PinchStartPositionAction;
		}
		set
		{
			SetInputActionProperty(ref m_PinchStartPositionAction, value);
		}
	}

	public InputActionProperty pinchGapAction
	{
		get
		{
			return m_PinchGapAction;
		}
		set
		{
			SetInputActionProperty(ref m_PinchGapAction, value);
		}
	}

	public InputActionProperty pinchGapDeltaAction
	{
		get
		{
			return m_PinchGapDeltaAction;
		}
		set
		{
			SetInputActionProperty(ref m_PinchGapDeltaAction, value);
		}
	}

	public InputActionProperty twistStartPositionAction
	{
		get
		{
			return m_TwistStartPositionAction;
		}
		set
		{
			SetInputActionProperty(ref m_TwistStartPositionAction, value);
		}
	}

	public InputActionProperty twistDeltaRotationAction
	{
		get
		{
			return m_TwistDeltaRotationAction;
		}
		set
		{
			SetInputActionProperty(ref m_TwistDeltaRotationAction, value);
		}
	}

	public InputActionProperty screenTouchCountAction
	{
		get
		{
			return m_ScreenTouchCountAction;
		}
		set
		{
			SetInputActionProperty(ref m_ScreenTouchCountAction, value);
		}
	}

	public Camera controllerCamera
	{
		get
		{
			return m_ControllerCamera;
		}
		set
		{
			m_ControllerCamera = value;
		}
	}

	public bool blockInteractionsWithScreenSpaceUI
	{
		get
		{
			return m_BlockInteractionsWithScreenSpaceUI;
		}
		set
		{
			m_BlockInteractionsWithScreenSpaceUI = value;
		}
	}

	public bool useRotationThreshold
	{
		get
		{
			return m_UseRotationThreshold;
		}
		set
		{
			m_UseRotationThreshold = value;
		}
	}

	public float rotationThreshold
	{
		get
		{
			return m_RotationThreshold;
		}
		set
		{
			m_RotationThreshold = value;
		}
	}

	public float scaleDelta { get; private set; }

	[Obsolete("pinchStartPosition has been deprecated. Use pinchStartPositionAction instead. (UnityUpgradable) -> pinchStartPositionAction", true)]
	public InputActionProperty pinchStartPosition
	{
		get
		{
			return default(InputActionProperty);
		}
		set
		{
		}
	}

	[Obsolete("pinchGapDelta has been deprecated. Use pinchGapDeltaAction instead. (UnityUpgradable) -> pinchGapDeltaAction", true)]
	public InputActionProperty pinchGapDelta
	{
		get
		{
			return default(InputActionProperty);
		}
		set
		{
		}
	}

	[Obsolete("twistStartPosition has been deprecated. Use twistStartPositionAction instead. (UnityUpgradable) -> twistStartPositionAction", true)]
	public InputActionProperty twistStartPosition
	{
		get
		{
			return default(InputActionProperty);
		}
		set
		{
		}
	}

	[Obsolete("twistRotationDeltaAction has been deprecated. Use twistDeltaRotationAction instead. (UnityUpgradable) -> twistDeltaRotationAction", true)]
	public InputActionProperty twistRotationDeltaAction
	{
		get
		{
			return default(InputActionProperty);
		}
		set
		{
		}
	}

	[Obsolete("screenTouchCount has been deprecated. Use screenTouchCountAction instead. (UnityUpgradable) -> screenTouchCountAction", true)]
	public InputActionProperty screenTouchCount
	{
		get
		{
			return default(InputActionProperty);
		}
		set
		{
		}
	}

	protected void Start()
	{
		if (m_ControllerCamera == null)
		{
			m_ControllerCamera = Camera.main;
			if (m_ControllerCamera == null)
			{
				Debug.LogWarning("Could not find associated Camera in scene.This XRScreenSpaceController will be disabled.", this);
				base.enabled = false;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		EnableAllDirectActions();
		InitializeTouchscreenGestureController();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		DisableAllDirectActions();
		RemoveTouchscreenGestureController();
		m_UIInputModule = null;
	}

	protected override void UpdateTrackingInput(XRControllerState controllerState)
	{
		base.UpdateTrackingInput(controllerState);
		if (controllerState == null || IsPointerOverScreenSpaceCanvas())
		{
			return;
		}
		if (!m_HasCheckedDisabledTrackingInputReferenceActions && (m_DragCurrentPositionAction.action != null || m_TapStartPositionAction.action != null || m_TwistStartPositionAction.action != null))
		{
			if (IsDisabledReferenceAction(m_DragCurrentPositionAction) || IsDisabledReferenceAction(m_TapStartPositionAction) || IsDisabledReferenceAction(m_TwistStartPositionAction))
			{
				Debug.LogWarning("'Enable Input Tracking' is enabled, but the Tap, Drag, Pinch, and/or Twist Action is disabled. The pose of the controller will not be updated correctly until the Input Actions are enabled. Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action. The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.", this);
			}
			m_HasCheckedDisabledTrackingInputReferenceActions = true;
		}
		int num = m_ScreenTouchCountAction.action?.ReadValue<int>() ?? 0;
		if (TryGetCurrentPositionAction(num, out var action))
		{
			Vector2 vector = action.ReadValue<Vector2>();
			Vector3 vector2 = m_ControllerCamera.ScreenToWorldPoint(new Vector3(vector.x, vector.y, m_ControllerCamera.nearClipPlane));
			Vector3 normalized = (vector2 - m_ControllerCamera.transform.position).normalized;
			controllerState.position = ((base.transform.parent != null) ? base.transform.parent.InverseTransformPoint(vector2) : vector2);
			controllerState.rotation = Quaternion.LookRotation(normalized);
			controllerState.inputTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
			controllerState.isTracked = num > 0;
		}
		else
		{
			controllerState.inputTrackingState = InputTrackingState.None;
			controllerState.isTracked = false;
		}
	}

	protected override void UpdateInput(XRControllerState controllerState)
	{
		base.UpdateInput(controllerState);
		if (controllerState == null || IsPointerOverScreenSpaceCanvas())
		{
			return;
		}
		if (!m_HasCheckedDisabledInputReferenceActions && (m_TwistDeltaRotationAction.action != null || m_DragCurrentPositionAction.action != null || m_TapStartPositionAction.action != null))
		{
			if (IsDisabledReferenceAction(m_TwistDeltaRotationAction) || IsDisabledReferenceAction(m_DragCurrentPositionAction) || IsDisabledReferenceAction(m_TapStartPositionAction))
			{
				Debug.LogWarning("'Enable Input Actions' is enabled, but the Tap, Drag, Pinch, and/or Twist Action is disabled. The controller input will not be handled correctly until the Input Actions are enabled. Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action. The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.", this);
			}
			m_HasCheckedDisabledInputReferenceActions = true;
		}
		controllerState.ResetFrameDependentStates();
		InputAction action2;
		if (TryGetCurrentTwoInputSelectAction(out var action))
		{
			controllerState.selectInteractionState.SetFrameState(isActive: true, action.ReadValue<float>());
		}
		else if (TryGetCurrentOneInputSelectAction(out action2))
		{
			controllerState.selectInteractionState.SetFrameState(isActive: true, action2.ReadValue<Vector2>().magnitude);
		}
		else
		{
			controllerState.selectInteractionState.SetFrameState(isActive: false, 0f);
		}
		if (m_UseRotationThreshold && TryGetAbsoluteValue(m_TwistDeltaRotationAction.action, out var value) && value >= m_RotationThreshold)
		{
			scaleDelta = 0f;
		}
		else
		{
			scaleDelta = ((m_PinchGapDeltaAction.action != null) ? (m_PinchGapDeltaAction.action.ReadValue<float>() / Screen.dpi) : 0f);
		}
	}

	private bool TryGetCurrentPositionAction(int touchCount, out InputAction action)
	{
		if (touchCount <= 1)
		{
			if (m_DragCurrentPositionAction.action != null && m_DragCurrentPositionAction.action.IsInProgress())
			{
				action = m_DragCurrentPositionAction.action;
				return true;
			}
			if (m_TapStartPositionAction.action != null && m_TapStartPositionAction.action.WasPerformedThisFrame())
			{
				action = m_TapStartPositionAction.action;
				return true;
			}
		}
		action = null;
		return false;
	}

	private bool TryGetCurrentOneInputSelectAction(out InputAction action)
	{
		if (m_DragCurrentPositionAction.action != null && m_DragCurrentPositionAction.action.IsInProgress())
		{
			action = m_DragCurrentPositionAction.action;
			return true;
		}
		if (m_TapStartPositionAction.action != null && m_TapStartPositionAction.action.WasPerformedThisFrame())
		{
			action = m_TapStartPositionAction.action;
			return true;
		}
		action = null;
		return false;
	}

	private bool TryGetCurrentTwoInputSelectAction(out InputAction action)
	{
		if (m_PinchGapAction.action != null && m_PinchGapAction.action.IsInProgress())
		{
			action = m_PinchGapAction.action;
			return true;
		}
		if (m_PinchGapDeltaAction.action != null && m_PinchGapDeltaAction.action.IsInProgress())
		{
			action = m_PinchGapDeltaAction.action;
			return true;
		}
		if (m_TwistDeltaRotationAction.action != null && m_TwistDeltaRotationAction.action.IsInProgress())
		{
			action = m_TwistDeltaRotationAction.action;
			return true;
		}
		action = null;
		return false;
	}

	private static bool TryGetAbsoluteValue(InputAction action, out float value)
	{
		if (action != null && action.IsInProgress())
		{
			value = Mathf.Abs(action.ReadValue<float>());
			return true;
		}
		value = 0f;
		return false;
	}

	private bool FindUIInputModule()
	{
		EventSystem current = EventSystem.current;
		if (current != null && current.currentInputModule != null)
		{
			m_UIInputModule = current.currentInputModule as UIInputModule;
		}
		return m_UIInputModule != null;
	}

	private bool IsPointerOverScreenSpaceCanvas()
	{
		if (m_BlockInteractionsWithScreenSpaceUI && (m_UIInputModule != null || FindUIInputModule()))
		{
			GameObject currentGameObject = m_UIInputModule.GetCurrentGameObject(-1);
			if (currentGameObject == null)
			{
				return false;
			}
			RenderMode renderMode = currentGameObject.GetComponentInParent<Canvas>().renderMode;
			if (renderMode != 0)
			{
				return renderMode == RenderMode.ScreenSpaceCamera;
			}
			return true;
		}
		return false;
	}

	private void InitializeTouchscreenGestureController()
	{
	}

	private void RemoveTouchscreenGestureController()
	{
	}

	private void EnableAllDirectActions()
	{
		m_TapStartPositionAction.EnableDirectAction();
		m_DragCurrentPositionAction.EnableDirectAction();
		m_DragDeltaAction.EnableDirectAction();
		m_PinchStartPositionAction.EnableDirectAction();
		m_PinchGapAction.EnableDirectAction();
		m_PinchGapDeltaAction.EnableDirectAction();
		m_TwistStartPositionAction.EnableDirectAction();
		m_TwistDeltaRotationAction.EnableDirectAction();
		m_ScreenTouchCountAction.EnableDirectAction();
	}

	private void DisableAllDirectActions()
	{
		m_TapStartPositionAction.DisableDirectAction();
		m_DragCurrentPositionAction.DisableDirectAction();
		m_DragDeltaAction.DisableDirectAction();
		m_PinchStartPositionAction.DisableDirectAction();
		m_PinchGapAction.DisableDirectAction();
		m_PinchGapDeltaAction.DisableDirectAction();
		m_TwistStartPositionAction.DisableDirectAction();
		m_TwistDeltaRotationAction.DisableDirectAction();
		m_ScreenTouchCountAction.DisableDirectAction();
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
