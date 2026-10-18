using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("Event/XR UI Input Module", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule.html")]
public class XRUIInputModule : UIInputModule
{
	private struct RegisteredInteractor
	{
		public IUIInteractor interactor;

		public TrackedDeviceModel model;

		internal bool deactivating;

		internal bool active;

		public RegisteredInteractor(IUIInteractor interactor, int deviceIndex)
		{
			this.interactor = interactor;
			model = new TrackedDeviceModel(deviceIndex)
			{
				interactor = interactor
			};
			active = true;
			deactivating = false;
		}
	}

	private struct RegisteredTouch
	{
		public bool isValid;

		public int touchId;

		public TouchModel model;

		public RegisteredTouch(Touch touch, int deviceIndex)
		{
			touchId = touch.fingerId;
			model = new TouchModel(deviceIndex);
			isValid = true;
		}
	}

	public enum ActiveInputMode
	{
		InputManagerBindings,
		InputSystemActions,
		Both
	}

	[HideInInspector]
	[SerializeField]
	private ActiveInputMode m_ActiveInputMode;

	[Header("Input Devices")]
	[SerializeField]
	[Tooltip("If true, will forward 3D tracked device data to UI elements.")]
	private bool m_EnableXRInput = true;

	[SerializeField]
	[Tooltip("If true, will forward 2D mouse data to UI elements. Ignored when any Input System UI Actions are used.")]
	private bool m_EnableMouseInput = true;

	[SerializeField]
	[Tooltip("If true, will forward 2D touch data to UI elements. Ignored when any Input System UI Actions are used.")]
	private bool m_EnableTouchInput = true;

	[SerializeField]
	[Tooltip("If true, will forward gamepad data to UI elements. Ignored when any Input System UI Actions are used.")]
	private bool m_EnableGamepadInput = true;

	[SerializeField]
	[Tooltip("If true, will forward joystick data to UI elements. Ignored when any Input System UI Actions are used.")]
	private bool m_EnableJoystickInput = true;

	[Header("Input System UI Actions")]
	[SerializeField]
	[Tooltip("Pointer input action reference, such as a mouse or single-finger touch device.")]
	private InputActionReference m_PointAction;

	[SerializeField]
	[Tooltip("Left-click input action reference, typically the left button on a mouse.")]
	private InputActionReference m_LeftClickAction;

	[SerializeField]
	[Tooltip("Middle-click input action reference, typically the middle button on a mouse.")]
	private InputActionReference m_MiddleClickAction;

	[SerializeField]
	[Tooltip("Right-click input action reference, typically the right button on a mouse.")]
	private InputActionReference m_RightClickAction;

	[SerializeField]
	[Tooltip("Scroll wheel input action reference, typically the scroll wheel on a mouse.")]
	private InputActionReference m_ScrollWheelAction;

	[SerializeField]
	[Tooltip("Navigation input action reference will change which UI element is currently selected to the one up, down, left of or right of the currently selected one.")]
	private InputActionReference m_NavigateAction;

	[SerializeField]
	[Tooltip("Submit input action reference will trigger a submission of the currently selected UI in the Event System.")]
	private InputActionReference m_SubmitAction;

	[SerializeField]
	[Tooltip("Cancel input action reference will trigger canceling out of the currently selected UI in the Event System.")]
	private InputActionReference m_CancelAction;

	[SerializeField]
	[Tooltip("When enabled, built-in Input System actions will be used if no Input System UI Actions are assigned.")]
	private bool m_EnableBuiltinActionsAsFallback = true;

	[HideInInspector]
	[SerializeField]
	[Tooltip("Name of the horizontal axis for gamepad/joystick UI navigation when using the old Input Manager.")]
	private string m_HorizontalAxis = "Horizontal";

	[HideInInspector]
	[SerializeField]
	[Tooltip("Name of the vertical axis for gamepad/joystick UI navigation when using the old Input Manager.")]
	private string m_VerticalAxis = "Vertical";

	[HideInInspector]
	[SerializeField]
	[Tooltip("Name of the gamepad/joystick button to use for UI selection or submission when using the old Input Manager.")]
	private string m_SubmitButton = "Submit";

	[HideInInspector]
	[SerializeField]
	[Tooltip("Name of the gamepad/joystick button to use for UI cancel or back commands when using the old Input Manager.")]
	private string m_CancelButton = "Cancel";

	private int m_RollingPointerId = 1;

	private Stack<int> m_DeletedPointerIds = new Stack<int>();

	private bool m_UseBuiltInInputSystemActions;

	private PointerModel m_PointerState;

	private NavigationModel m_NavigationState;

	internal const float kPixelPerLine = 20f;

	private readonly List<RegisteredTouch> m_RegisteredTouches = new List<RegisteredTouch>();

	private readonly List<RegisteredInteractor> m_RegisteredInteractors = new List<RegisteredInteractor>();

	private readonly LinkedPool<UIHoverEventArgs> m_UIHoverEventArgs = new LinkedPool<UIHoverEventArgs>(() => new UIHoverEventArgs(), null, null, null, collectionCheck: false);

	[Obsolete("activeInputMode has been deprecated in version 3.1.0. Input System Package (New) will be the default input handling mode used when active input handling is set to Both.")]
	public ActiveInputMode activeInputMode
	{
		get
		{
			return m_ActiveInputMode;
		}
		set
		{
			m_ActiveInputMode = value;
		}
	}

	public bool enableXRInput
	{
		get
		{
			return m_EnableXRInput;
		}
		set
		{
			m_EnableXRInput = value;
		}
	}

	public bool enableMouseInput
	{
		get
		{
			return m_EnableMouseInput;
		}
		set
		{
			m_EnableMouseInput = value;
		}
	}

	public bool enableTouchInput
	{
		get
		{
			return m_EnableTouchInput;
		}
		set
		{
			m_EnableTouchInput = value;
		}
	}

	public bool enableGamepadInput
	{
		get
		{
			return m_EnableGamepadInput;
		}
		set
		{
			m_EnableGamepadInput = value;
		}
	}

	public bool enableJoystickInput
	{
		get
		{
			return m_EnableJoystickInput;
		}
		set
		{
			m_EnableJoystickInput = value;
		}
	}

	public InputActionReference pointAction
	{
		get
		{
			return m_PointAction;
		}
		set
		{
			SetInputAction(ref m_PointAction, value);
		}
	}

	public InputActionReference leftClickAction
	{
		get
		{
			return m_LeftClickAction;
		}
		set
		{
			SetInputAction(ref m_LeftClickAction, value);
		}
	}

	public InputActionReference middleClickAction
	{
		get
		{
			return m_MiddleClickAction;
		}
		set
		{
			SetInputAction(ref m_MiddleClickAction, value);
		}
	}

	public InputActionReference rightClickAction
	{
		get
		{
			return m_RightClickAction;
		}
		set
		{
			SetInputAction(ref m_RightClickAction, value);
		}
	}

	public InputActionReference scrollWheelAction
	{
		get
		{
			return m_ScrollWheelAction;
		}
		set
		{
			SetInputAction(ref m_ScrollWheelAction, value);
		}
	}

	public InputActionReference navigateAction
	{
		get
		{
			return m_NavigateAction;
		}
		set
		{
			SetInputAction(ref m_NavigateAction, value);
		}
	}

	public InputActionReference submitAction
	{
		get
		{
			return m_SubmitAction;
		}
		set
		{
			SetInputAction(ref m_SubmitAction, value);
		}
	}

	public InputActionReference cancelAction
	{
		get
		{
			return m_CancelAction;
		}
		set
		{
			SetInputAction(ref m_CancelAction, value);
		}
	}

	public bool enableBuiltinActionsAsFallback
	{
		get
		{
			return m_EnableBuiltinActionsAsFallback;
		}
		set
		{
			m_EnableBuiltinActionsAsFallback = value;
			m_UseBuiltInInputSystemActions = m_EnableBuiltinActionsAsFallback && !InputActionReferencesAreSet();
		}
	}

	public string horizontalAxis
	{
		get
		{
			return m_HorizontalAxis;
		}
		set
		{
			m_HorizontalAxis = value;
		}
	}

	public string verticalAxis
	{
		get
		{
			return m_VerticalAxis;
		}
		set
		{
			m_VerticalAxis = value;
		}
	}

	public string submitButton
	{
		get
		{
			return m_SubmitButton;
		}
		set
		{
			m_SubmitButton = value;
		}
	}

	public string cancelButton
	{
		get
		{
			return m_CancelButton;
		}
		set
		{
			m_CancelButton = value;
		}
	}

	[Obsolete("maxRaycastDistance has been deprecated. Its value was unused, calling this property is unnecessary and should be removed.", true)]
	public float maxRaycastDistance
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_ActiveInputMode = ActiveInputMode.InputSystemActions;
		m_PointerState = new PointerModel(0);
		m_NavigationState = default(NavigationModel);
		m_UseBuiltInInputSystemActions = m_EnableBuiltinActionsAsFallback && !InputActionReferencesAreSet();
		if (m_ActiveInputMode != 0)
		{
			EnableAllActions();
		}
	}

	protected override void OnDisable()
	{
		RemovePointerEventData(m_PointerState.pointerId);
		if (m_ActiveInputMode != 0)
		{
			DisableAllActions();
		}
		base.OnDisable();
	}

	public void RegisterInteractor(IUIInteractor interactor)
	{
		if (interactor == null)
		{
			return;
		}
		for (int i = 0; i < m_RegisteredInteractors.Count; i++)
		{
			RegisteredInteractor value = m_RegisteredInteractors[i];
			if (value.interactor == interactor)
			{
				if (!value.active)
				{
					value.active = true;
					value.deactivating = false;
					value.model.Reset();
					m_RegisteredInteractors[i] = value;
				}
				return;
			}
		}
		if (!m_DeletedPointerIds.TryPop(out var result))
		{
			result = m_RollingPointerId++;
		}
		m_RegisteredInteractors.Add(new RegisteredInteractor(interactor, result));
	}

	public void UnregisterInteractor(IUIInteractor interactor)
	{
		if (interactor == null)
		{
			return;
		}
		for (int i = 0; i < m_RegisteredInteractors.Count; i++)
		{
			RegisteredInteractor value = m_RegisteredInteractors[i];
			if (value.interactor == interactor)
			{
				if (value.active)
				{
					value.deactivating = true;
					value.active = false;
					m_RegisteredInteractors[i] = value;
				}
				break;
			}
		}
	}

	public IUIInteractor GetInteractor(int pointerId)
	{
		for (int i = 0; i < m_RegisteredInteractors.Count; i++)
		{
			if (m_RegisteredInteractors[i].model.pointerId == pointerId && m_RegisteredInteractors[i].active)
			{
				return m_RegisteredInteractors[i].interactor;
			}
		}
		return null;
	}

	public bool GetTrackedDeviceModel(IUIInteractor interactor, out TrackedDeviceModel model)
	{
		for (int i = 0; i < m_RegisteredInteractors.Count; i++)
		{
			if (m_RegisteredInteractors[i].interactor == interactor)
			{
				model = m_RegisteredInteractors[i].model;
				return true;
			}
		}
		model = new TrackedDeviceModel(-1);
		return false;
	}

	protected override void DoProcess()
	{
		if (m_EnableXRInput)
		{
			for (int i = 0; i < m_RegisteredInteractors.Count; i++)
			{
				RegisteredInteractor value = m_RegisteredInteractors[i];
				GameObject pointerTarget = value.model.implementationData.pointerTarget;
				bool flag = value.interactor is Object @object && @object == null;
				if (flag || value.deactivating)
				{
					value.model.Reset(resetImplementation: false);
					ProcessTrackedDevice(ref value.model, force: true);
					RemovePointerEventData(value.model.pointerId);
					if (flag)
					{
						m_DeletedPointerIds.Push(value.model.pointerId);
						m_RegisteredInteractors.RemoveAt(i--);
						continue;
					}
					value.deactivating = false;
					value.model.Reset();
					m_RegisteredInteractors[i] = value;
				}
				else
				{
					if (!value.active)
					{
						continue;
					}
					value.interactor.UpdateUIModel(ref value.model);
					ProcessTrackedDevice(ref value.model);
					value.model.UpdatePokeSelectState();
					m_RegisteredInteractors[i] = value;
				}
				GameObject pointerTarget2 = value.model.implementationData.pointerTarget;
				if (pointerTarget != pointerTarget2)
				{
					UIHoverEventArgs v;
					using (m_UIHoverEventArgs.Get(out v))
					{
						v.interactorObject = value.interactor;
						v.deviceModel = value.model;
						if (v.interactorObject != null && v.interactorObject is IUIHoverInteractor iUIHoverInteractor)
						{
							if (pointerTarget != null)
							{
								v.uiObject = pointerTarget;
								iUIHoverInteractor.OnUIHoverExited(v);
							}
							if (pointerTarget2 != null && pointerTarget2.activeInHierarchy)
							{
								v.uiObject = pointerTarget2;
								iUIHoverInteractor.OnUIHoverEntered(v);
							}
						}
					}
				}
				if ((!(pointerTarget != null) || pointerTarget.activeInHierarchy) && ((object)pointerTarget == null || !(pointerTarget == null)))
				{
					continue;
				}
				UIHoverEventArgs v2;
				using (m_UIHoverEventArgs.Get(out v2))
				{
					if (pointerTarget == pointerTarget2)
					{
						value.model.Reset();
						m_RegisteredInteractors[i] = value;
					}
					IUIInteractor interactor = value.interactor;
					if (interactor != null && interactor is IUIHoverInteractor iUIHoverInteractor2)
					{
						v2.interactorObject = interactor;
						v2.uiObject = pointerTarget;
						v2.deviceModel = value.model;
						iUIHoverInteractor2.OnUIHoverExited(v2);
					}
				}
			}
		}
		if (m_ActiveInputMode != 0)
		{
			GetPointerStates();
		}
		ProcessPointerState(ref m_PointerState);
		ProcessNavigationState(ref m_NavigationState);
	}

	private void GetPointerStates()
	{
		if (m_UseBuiltInInputSystemActions)
		{
			if (m_EnableTouchInput && Touchscreen.current != null)
			{
				m_PointerState.position = Touchscreen.current.position.ReadValue();
				m_PointerState.displayIndex = Touchscreen.current.displayIndex.ReadValue();
			}
			if (m_EnableMouseInput && Mouse.current != null)
			{
				m_PointerState.position = Mouse.current.position.ReadValue();
				m_PointerState.displayIndex = Mouse.current.displayIndex.ReadValue();
				m_PointerState.scrollDelta = Mouse.current.scroll.ReadValue() * 0.05f;
				m_PointerState.leftButtonPressed = Mouse.current.leftButton.isPressed;
				m_PointerState.rightButtonPressed = Mouse.current.rightButton.isPressed;
				m_PointerState.middleButtonPressed = Mouse.current.middleButton.isPressed;
			}
			if (m_EnableGamepadInput && Gamepad.current != null)
			{
				m_NavigationState.move = Gamepad.current.leftStick.ReadValue() + Gamepad.current.dpad.ReadValue();
				m_NavigationState.submitButtonDown = Gamepad.current.buttonSouth.isPressed;
				m_NavigationState.cancelButtonDown = Gamepad.current.buttonEast.isPressed;
			}
			if (m_EnableJoystickInput && Joystick.current != null)
			{
				m_NavigationState.move = Joystick.current.stick.ReadValue() + ((Joystick.current.hatswitch != null) ? Joystick.current.hatswitch.ReadValue() : Vector2.zero);
				m_NavigationState.submitButtonDown = Joystick.current.trigger.isPressed;
				m_NavigationState.cancelButtonDown = false;
			}
			return;
		}
		if (IsActionEnabled(m_PointAction))
		{
			m_PointerState.position = m_PointAction.action.ReadValue<Vector2>();
			m_PointerState.displayIndex = GetDisplayIndexFor(m_PointAction.action.activeControl);
		}
		if (IsActionEnabled(m_ScrollWheelAction))
		{
			m_PointerState.scrollDelta = m_ScrollWheelAction.action.ReadValue<Vector2>() * 0.05f;
		}
		if (IsActionEnabled(m_LeftClickAction))
		{
			m_PointerState.leftButtonPressed = m_LeftClickAction.action.IsPressed();
		}
		if (IsActionEnabled(m_RightClickAction))
		{
			m_PointerState.rightButtonPressed = m_RightClickAction.action.IsPressed();
		}
		if (IsActionEnabled(m_MiddleClickAction))
		{
			m_PointerState.middleButtonPressed = m_MiddleClickAction.action.IsPressed();
		}
		if (IsActionEnabled(m_NavigateAction))
		{
			m_NavigationState.move = m_NavigateAction.action.ReadValue<Vector2>();
		}
		if (IsActionEnabled(m_SubmitAction))
		{
			m_NavigationState.submitButtonDown = m_SubmitAction.action.WasPerformedThisFrame();
		}
		if (IsActionEnabled(m_CancelAction))
		{
			m_NavigationState.cancelButtonDown = m_CancelAction.action.WasPerformedThisFrame();
		}
	}

	private bool InputActionReferencesAreSet()
	{
		if (!(m_PointAction != null) && !(m_LeftClickAction != null) && !(m_RightClickAction != null) && !(m_MiddleClickAction != null) && !(m_NavigateAction != null) && !(m_SubmitAction != null) && !(m_CancelAction != null))
		{
			return m_ScrollWheelAction != null;
		}
		return true;
	}

	private void EnableAllActions()
	{
		EnableInputAction(m_PointAction);
		EnableInputAction(m_LeftClickAction);
		EnableInputAction(m_RightClickAction);
		EnableInputAction(m_MiddleClickAction);
		EnableInputAction(m_NavigateAction);
		EnableInputAction(m_SubmitAction);
		EnableInputAction(m_CancelAction);
		EnableInputAction(m_ScrollWheelAction);
	}

	private void DisableAllActions()
	{
		DisableInputAction(m_PointAction);
		DisableInputAction(m_LeftClickAction);
		DisableInputAction(m_RightClickAction);
		DisableInputAction(m_MiddleClickAction);
		DisableInputAction(m_NavigateAction);
		DisableInputAction(m_SubmitAction);
		DisableInputAction(m_CancelAction);
		DisableInputAction(m_ScrollWheelAction);
	}

	private static bool IsActionEnabled(InputActionReference inputAction)
	{
		if (inputAction != null && inputAction.action != null)
		{
			return inputAction.action.enabled;
		}
		return false;
	}

	private static void EnableInputAction(InputActionReference inputAction)
	{
		if (!(inputAction == null) && inputAction.action != null)
		{
			inputAction.action.Enable();
		}
	}

	private static void DisableInputAction(InputActionReference inputAction)
	{
		if (!(inputAction == null) && inputAction.action != null)
		{
			inputAction.action.Disable();
		}
	}

	private void SetInputAction(ref InputActionReference inputAction, InputActionReference value)
	{
		if (Application.isPlaying && inputAction != null)
		{
			inputAction.action?.Disable();
		}
		inputAction = value;
		if (Application.isPlaying && base.isActiveAndEnabled && inputAction != null)
		{
			inputAction.action?.Enable();
		}
	}

	private int GetDisplayIndexFor(InputControl control)
	{
		int result = 0;
		if (control != null && control.device is Pointer pointer && pointer != null)
		{
			result = pointer.displayIndex.ReadValue();
		}
		return result;
	}
}
