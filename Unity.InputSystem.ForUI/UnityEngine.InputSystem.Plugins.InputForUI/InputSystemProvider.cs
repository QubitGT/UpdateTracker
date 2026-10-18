using System;
using System.Collections.Generic;
using Unity.IntegerTime;
using UnityEngine.InputForUI;
using UnityEngine.InputSystem.Controls;

namespace UnityEngine.InputSystem.Plugins.InputForUI;

internal class InputSystemProvider : IEventProviderImpl
{
	public static class Actions
	{
		public static readonly string PointAction = "UI/Point";

		public static readonly string MoveAction = "UI/Navigate";

		public static readonly string SubmitAction = "UI/Submit";

		public static readonly string CancelAction = "UI/Cancel";

		public static readonly string LeftClickAction = "UI/Click";

		public static readonly string MiddleClickAction = "UI/MiddleClick";

		public static readonly string RightClickAction = "UI/RightClick";

		public static readonly string ScrollWheelAction = "UI/ScrollWheel";
	}

	private InputEventPartialProvider m_InputEventPartialProvider;

	private DefaultInputActions m_DefaultInputActions;

	private InputActionAsset m_InputActionAsset;

	private InputActionReference m_PointAction;

	private InputActionReference m_MoveAction;

	private InputActionReference m_SubmitAction;

	private InputActionReference m_CancelAction;

	private InputActionReference m_LeftClickAction;

	private InputActionReference m_MiddleClickAction;

	private InputActionReference m_RightClickAction;

	private InputActionReference m_ScrollWheelAction;

	private InputAction m_NextPreviousAction;

	private List<Event> m_Events = new List<Event>();

	private PointerState m_MouseState;

	private PointerState m_PenState;

	private bool m_SeenPenEvents;

	private PointerState m_TouchState;

	private bool m_SeenTouchEvents;

	private const float k_SmallestReportedMovementSqrDist = 0.01f;

	private NavigationEventRepeatHelper m_RepeatHelper = new NavigationEventRepeatHelper();

	private bool m_ResetSeenEventsOnUpdate;

	private const float kScrollUGUIScaleFactor = 3f;

	private static Action<InputActionAsset> s_OnRegisterActions;

	private const uint k_DefaultPlayerId = 0u;

	private EventModifiers m_EventModifiers => m_InputEventPartialProvider._eventModifiers;

	private DiscreteTime m_CurrentTime => (DiscreteTime)Time.timeAsRational;

	public uint playerCount => 1u;

	static InputSystemProvider()
	{
		EventProvider.SetInputSystemProvider(new InputSystemProvider());
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Bootstrap()
	{
	}

	public void Initialize()
	{
		if (m_InputEventPartialProvider == null)
		{
			m_InputEventPartialProvider = new InputEventPartialProvider();
		}
		m_InputEventPartialProvider.Initialize();
		m_Events.Clear();
		m_MouseState.Reset();
		m_PenState.Reset();
		m_SeenPenEvents = false;
		m_TouchState.Reset();
		m_SeenTouchEvents = false;
		SelectInputActionAsset();
		RegisterActions();
		RegisterFixedActions();
		InputSystem.onActionsChange += OnActionsChange;
	}

	public void Shutdown()
	{
		UnregisterActions();
		UnregisterFixedActions();
		m_InputEventPartialProvider.Shutdown();
		m_InputEventPartialProvider = null;
		if (m_DefaultInputActions != null)
		{
			m_DefaultInputActions.Dispose();
			m_DefaultInputActions = null;
		}
		InputSystem.onActionsChange -= OnActionsChange;
	}

	public void OnActionsChange()
	{
		UnregisterActions();
		SelectInputActionAsset();
		RegisterActions();
	}

	public void Update()
	{
		m_InputEventPartialProvider.Update();
		m_Events.Sort((Event a, Event b) => SortEvents(a, b));
		DiscreteTime currentTime = (DiscreteTime)Time.timeAsRational;
		DirectionNavigation(currentTime);
		foreach (Event @event in m_Events)
		{
			Event ev = @event;
			if (m_SeenTouchEvents && ev.type == Event.Type.PointerEvent && ev.eventSource == EventSource.Pen)
			{
				m_PenState.Reset();
			}
			else if ((m_SeenTouchEvents || m_SeenPenEvents) && ev.type == Event.Type.PointerEvent && (ev.eventSource == EventSource.Mouse || ev.eventSource == EventSource.Unspecified))
			{
				m_MouseState.Reset();
			}
			else
			{
				EventProvider.Dispatch(in ev);
			}
		}
		if (m_ResetSeenEventsOnUpdate)
		{
			ResetSeenEvents();
			m_ResetSeenEventsOnUpdate = false;
		}
		m_Events.Clear();
	}

	private void ResetSeenEvents()
	{
		m_SeenTouchEvents = false;
		m_SeenPenEvents = false;
	}

	public bool ActionAssetIsNotNull()
	{
		return m_InputActionAsset != null;
	}

	private void DirectionNavigation(DiscreteTime currentTime)
	{
		(Vector2, bool) tuple = ReadCurrentNavigationMoveVector();
		Vector2 item = tuple.Item1;
		bool axisButtonsWherePressedThisFrame = tuple.Item2;
		NavigationEvent.Direction direction = NavigationEvent.DetermineMoveDirection(item);
		if (direction == NavigationEvent.Direction.None)
		{
			direction = ReadNextPreviousDirection();
			axisButtonsWherePressedThisFrame = m_NextPreviousAction.WasPressedThisFrame();
		}
		if (direction == NavigationEvent.Direction.None)
		{
			m_RepeatHelper.Reset();
		}
		else if (m_RepeatHelper.ShouldSendMoveEvent(currentTime, direction, axisButtonsWherePressedThisFrame))
		{
			NavigationEvent navigationEvent = default(NavigationEvent);
			navigationEvent.type = NavigationEvent.Type.Move;
			navigationEvent.direction = direction;
			navigationEvent.timestamp = currentTime;
			navigationEvent.eventSource = GetEventSource(GetActiveDeviceFromDirection(direction));
			navigationEvent.playerId = 0u;
			navigationEvent.eventModifiers = m_EventModifiers;
			Event ev = Event.From(navigationEvent);
			EventProvider.Dispatch(in ev);
		}
	}

	private InputDevice GetActiveDeviceFromDirection(NavigationEvent.Direction direction)
	{
		switch (direction)
		{
		case NavigationEvent.Direction.Left:
		case NavigationEvent.Direction.Up:
		case NavigationEvent.Direction.Right:
		case NavigationEvent.Direction.Down:
			if (m_MoveAction != null)
			{
				return m_MoveAction.action.activeControl.device;
			}
			break;
		case NavigationEvent.Direction.Next:
		case NavigationEvent.Direction.Previous:
			if (m_NextPreviousAction != null)
			{
				return m_NextPreviousAction.activeControl.device;
			}
			break;
		}
		return Keyboard.current;
	}

	private (Vector2, bool) ReadCurrentNavigationMoveVector()
	{
		if (m_MoveAction == null)
		{
			return (default(Vector2), false);
		}
		Vector2 item = m_MoveAction.action.ReadValue<Vector2>();
		bool item2 = m_MoveAction.action.WasPressedThisFrame();
		return (item, item2);
	}

	private NavigationEvent.Direction ReadNextPreviousDirection()
	{
		if (m_NextPreviousAction.IsPressed() && m_NextPreviousAction.activeControl.device is Keyboard)
		{
			if (!(m_NextPreviousAction.activeControl.device as Keyboard).shiftKey.isPressed)
			{
				return NavigationEvent.Direction.Next;
			}
			return NavigationEvent.Direction.Previous;
		}
		return NavigationEvent.Direction.None;
	}

	private static int SortEvents(Event a, Event b)
	{
		return Event.CompareType(a, b);
	}

	public void OnFocusChanged(bool focus)
	{
		m_InputEventPartialProvider.OnFocusChanged(focus);
	}

	public bool RequestCurrentState(Event.Type type)
	{
		if (m_InputEventPartialProvider.RequestCurrentState(type))
		{
			return true;
		}
		switch (type)
		{
		case Event.Type.PointerEvent:
			if (m_TouchState.LastPositionValid)
			{
				Event ev = Event.From(ToPointerStateEvent(m_CurrentTime, in m_TouchState, EventSource.Touch));
				EventProvider.Dispatch(in ev);
			}
			if (m_PenState.LastPositionValid)
			{
				Event ev = Event.From(ToPointerStateEvent(m_CurrentTime, in m_PenState, EventSource.Pen));
				EventProvider.Dispatch(in ev);
			}
			if (m_MouseState.LastPositionValid)
			{
				Event ev = Event.From(ToPointerStateEvent(m_CurrentTime, in m_MouseState, EventSource.Mouse));
				EventProvider.Dispatch(in ev);
			}
			if (!m_TouchState.LastPositionValid && !m_PenState.LastPositionValid)
			{
				return m_MouseState.LastPositionValid;
			}
			return true;
		default:
			return false;
		}
	}

	private static Vector2 ScreenBottomLeftToPanelPosition(Vector2 position, int targetDisplay)
	{
		int num = Screen.height;
		if (targetDisplay > 0 && targetDisplay < Display.displays.Length)
		{
			num = Display.displays[targetDisplay].systemHeight;
		}
		position.y = (float)num - position.y;
		return position;
	}

	private PointerEvent ToPointerStateEvent(DiscreteTime currentTime, in PointerState state, EventSource eventSource)
	{
		PointerEvent result = default(PointerEvent);
		result.type = PointerEvent.Type.State;
		result.pointerIndex = 0;
		result.position = state.LastPosition;
		result.deltaPosition = Vector2.zero;
		result.scroll = Vector2.zero;
		result.displayIndex = state.LastDisplayIndex;
		result.button = PointerEvent.Button.None;
		result.buttonsState = state.ButtonsState;
		result.clickCount = 0;
		result.timestamp = currentTime;
		result.eventSource = eventSource;
		result.playerId = 0u;
		result.eventModifiers = m_EventModifiers;
		return result;
	}

	private EventSource GetEventSource(InputAction.CallbackContext ctx)
	{
		InputDevice device = ctx.control.device;
		return GetEventSource(device);
	}

	private EventSource GetEventSource(InputDevice device)
	{
		if (device is Touchscreen)
		{
			return EventSource.Touch;
		}
		if (device is Pen)
		{
			return EventSource.Pen;
		}
		if (device is Mouse)
		{
			return EventSource.Mouse;
		}
		if (device is Keyboard)
		{
			return EventSource.Keyboard;
		}
		if (device is Gamepad)
		{
			return EventSource.Gamepad;
		}
		return EventSource.Unspecified;
	}

	private ref PointerState GetPointerStateForSource(EventSource eventSource)
	{
		return eventSource switch
		{
			EventSource.Touch => ref m_TouchState, 
			EventSource.Pen => ref m_PenState, 
			_ => ref m_MouseState, 
		};
	}

	private void DispatchFromCallback(in Event ev)
	{
		m_Events.Add(ev);
	}

	private static int FindTouchFingerIndex(Touchscreen touchscreen, InputAction.CallbackContext ctx)
	{
		if (touchscreen == null)
		{
			return 0;
		}
		Vector2Control vector2Control = ((ctx.control is Vector2Control) ? ((Vector2Control)ctx.control) : null);
		TouchPressControl touchPressControl = ((ctx.control is TouchPressControl) ? ((TouchPressControl)ctx.control) : null);
		TouchControl touchControl = ((ctx.control is TouchControl) ? ((TouchControl)ctx.control) : null);
		for (int i = 0; i < touchscreen.touches.Count; i++)
		{
			if (vector2Control != null && vector2Control == touchscreen.touches[i].position)
			{
				return i;
			}
			if (touchPressControl != null && touchPressControl == touchscreen.touches[i].press)
			{
				return i;
			}
			if (touchControl != null && touchControl == touchscreen.touches[i])
			{
				return i;
			}
		}
		return 0;
	}

	private void OnPointerPerformed(InputAction.CallbackContext ctx)
	{
		EventSource eventSource = GetEventSource(ctx);
		ref PointerState pointerStateForSource = ref GetPointerStateForSource(eventSource);
		Pointer pointer = ((ctx.control.device is Pointer) ? ((Pointer)ctx.control.device) : null);
		Pen pen = ((ctx.control.device is Pen) ? ((Pen)ctx.control.device) : null);
		Touchscreen touchscreen = ((ctx.control.device is Touchscreen) ? ((Touchscreen)ctx.control.device) : null);
		TouchControl touchControl = ((ctx.control is TouchControl) ? ((TouchControl)ctx.control) : null);
		int pointerIndex = FindTouchFingerIndex(touchscreen, ctx);
		m_ResetSeenEventsOnUpdate = false;
		if (touchControl != null || touchscreen != null)
		{
			m_SeenTouchEvents = true;
		}
		else if (pen != null)
		{
			m_SeenPenEvents = true;
		}
		Vector2 position = ctx.ReadValue<Vector2>();
		int num = pointer?.displayIndex.ReadValue() ?? touchscreen?.displayIndex.ReadValue() ?? pen?.displayIndex.ReadValue() ?? 0;
		Vector2 vector = ScreenBottomLeftToPanelPosition(position, num);
		Vector2 deltaPosition = (pointerStateForSource.LastPositionValid ? (vector - pointerStateForSource.LastPosition) : Vector2.zero);
		Vector2 tilt = pen?.tilt.ReadValue() ?? Vector2.zero;
		float twist = pen?.twist.ReadValue() ?? 0f;
		float pressure = pen?.pressure.ReadValue() ?? touchControl?.pressure.ReadValue() ?? 0f;
		bool isInverted = pen?.eraser.isPressed ?? false;
		if (deltaPosition.sqrMagnitude >= 0.01f)
		{
			Event ev = Event.From(new PointerEvent
			{
				type = PointerEvent.Type.PointerMoved,
				pointerIndex = pointerIndex,
				position = vector,
				deltaPosition = deltaPosition,
				scroll = Vector2.zero,
				displayIndex = num,
				tilt = tilt,
				twist = twist,
				pressure = pressure,
				isInverted = isInverted,
				button = PointerEvent.Button.None,
				buttonsState = pointerStateForSource.ButtonsState,
				clickCount = 0,
				timestamp = m_CurrentTime,
				eventSource = eventSource,
				playerId = 0u,
				eventModifiers = m_EventModifiers
			});
			DispatchFromCallback(in ev);
			pointerStateForSource.OnMove(m_CurrentTime, vector, num);
		}
		else if (!pointerStateForSource.LastPositionValid)
		{
			pointerStateForSource.OnMove(m_CurrentTime, vector, num);
		}
	}

	private void OnSubmitPerformed(InputAction.CallbackContext ctx)
	{
		Event ev = Event.From(new NavigationEvent
		{
			type = NavigationEvent.Type.Submit,
			direction = NavigationEvent.Direction.None,
			timestamp = m_CurrentTime,
			eventSource = GetEventSource(ctx),
			playerId = 0u,
			eventModifiers = m_EventModifiers
		});
		DispatchFromCallback(in ev);
	}

	private void OnCancelPerformed(InputAction.CallbackContext ctx)
	{
		Event ev = Event.From(new NavigationEvent
		{
			type = NavigationEvent.Type.Cancel,
			direction = NavigationEvent.Direction.None,
			timestamp = m_CurrentTime,
			eventSource = GetEventSource(ctx),
			playerId = 0u,
			eventModifiers = m_EventModifiers
		});
		DispatchFromCallback(in ev);
	}

	private void OnClickPerformed(InputAction.CallbackContext ctx, EventSource eventSource, PointerEvent.Button button)
	{
		ref PointerState pointerStateForSource = ref GetPointerStateForSource(eventSource);
		Touchscreen touchscreen = ((ctx.control.device is Touchscreen) ? ((Touchscreen)ctx.control.device) : null);
		TouchControl obj = ((ctx.control is TouchControl) ? ((TouchControl)ctx.control) : null);
		int pointerIndex = FindTouchFingerIndex(touchscreen, ctx);
		m_ResetSeenEventsOnUpdate = true;
		if (obj != null || touchscreen != null)
		{
			m_SeenTouchEvents = true;
		}
		bool previousState = pointerStateForSource.ButtonsState.Get(button);
		bool flag = ctx.ReadValueAsButton();
		pointerStateForSource.OnButtonChange(m_CurrentTime, button, previousState, flag);
		Event ev = Event.From(new PointerEvent
		{
			type = (flag ? PointerEvent.Type.ButtonPressed : PointerEvent.Type.ButtonReleased),
			pointerIndex = pointerIndex,
			position = pointerStateForSource.LastPosition,
			deltaPosition = Vector2.zero,
			scroll = Vector2.zero,
			displayIndex = pointerStateForSource.LastDisplayIndex,
			tilt = Vector2.zero,
			twist = 0f,
			pressure = 0f,
			isInverted = false,
			button = button,
			buttonsState = pointerStateForSource.ButtonsState,
			clickCount = pointerStateForSource.ClickCount,
			timestamp = m_CurrentTime,
			eventSource = eventSource,
			playerId = 0u,
			eventModifiers = m_EventModifiers
		});
		DispatchFromCallback(in ev);
	}

	private void OnLeftClickPerformed(InputAction.CallbackContext ctx)
	{
		OnClickPerformed(ctx, GetEventSource(ctx), PointerEvent.Button.Primary);
	}

	private void OnMiddleClickPerformed(InputAction.CallbackContext ctx)
	{
		OnClickPerformed(ctx, GetEventSource(ctx), PointerEvent.Button.PenBarrelButton);
	}

	private void OnRightClickPerformed(InputAction.CallbackContext ctx)
	{
		OnClickPerformed(ctx, GetEventSource(ctx), PointerEvent.Button.PenEraserInTouch);
	}

	private void OnScrollWheelPerformed(InputAction.CallbackContext ctx)
	{
		Vector2 vector = ctx.ReadValue<Vector2>() / InputSystem.scrollWheelDeltaPerTick;
		if (!(vector.sqrMagnitude < 0.01f))
		{
			EventSource eventSource = GetEventSource(ctx);
			ref PointerState pointerStateForSource = ref GetPointerStateForSource(eventSource);
			Vector2 position = Vector2.zero;
			int displayIndex = 0;
			if (pointerStateForSource.LastPositionValid)
			{
				position = pointerStateForSource.LastPosition;
				displayIndex = pointerStateForSource.LastDisplayIndex;
			}
			else if (eventSource == EventSource.Mouse && Mouse.current != null)
			{
				position = Mouse.current.position.ReadValue();
				displayIndex = Mouse.current.displayIndex.ReadValue();
			}
			Vector2 vector2 = default(Vector2);
			vector2.x = vector.x * 3f;
			vector2.y = (0f - vector.y) * 3f;
			Vector2 scroll = vector2;
			Event ev = Event.From(new PointerEvent
			{
				type = PointerEvent.Type.Scroll,
				pointerIndex = 0,
				position = position,
				deltaPosition = Vector2.zero,
				scroll = scroll,
				displayIndex = displayIndex,
				tilt = Vector2.zero,
				twist = 0f,
				pressure = 0f,
				isInverted = false,
				button = PointerEvent.Button.None,
				buttonsState = pointerStateForSource.ButtonsState,
				clickCount = 0,
				timestamp = m_CurrentTime,
				eventSource = EventSource.Mouse,
				playerId = 0u,
				eventModifiers = m_EventModifiers
			});
			DispatchFromCallback(in ev);
		}
	}

	private void RegisterFixedActions()
	{
		m_NextPreviousAction = new InputAction("nextPreviousAction", InputActionType.Button);
		m_NextPreviousAction.AddBinding("<Keyboard>/tab");
		m_NextPreviousAction.Enable();
	}

	private void UnregisterFixedActions()
	{
		if (m_NextPreviousAction != null)
		{
			m_NextPreviousAction.Disable();
			m_NextPreviousAction = null;
		}
	}

	private void RegisterActions()
	{
		s_OnRegisterActions?.Invoke(m_InputActionAsset);
		m_PointAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.PointAction));
		m_MoveAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.MoveAction));
		m_SubmitAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.SubmitAction));
		m_CancelAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.CancelAction));
		m_LeftClickAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.LeftClickAction));
		m_MiddleClickAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.MiddleClickAction));
		m_RightClickAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.RightClickAction));
		m_ScrollWheelAction = InputActionReference.Create(m_InputActionAsset.FindAction(Actions.ScrollWheelAction));
		if (m_PointAction != null && m_PointAction.action != null)
		{
			m_PointAction.action.performed += OnPointerPerformed;
		}
		if (m_SubmitAction != null && m_SubmitAction.action != null)
		{
			m_SubmitAction.action.performed += OnSubmitPerformed;
		}
		if (m_CancelAction != null && m_CancelAction.action != null)
		{
			m_CancelAction.action.performed += OnCancelPerformed;
		}
		if (m_LeftClickAction != null && m_LeftClickAction.action != null)
		{
			m_LeftClickAction.action.performed += OnLeftClickPerformed;
		}
		if (m_MiddleClickAction != null && m_MiddleClickAction.action != null)
		{
			m_MiddleClickAction.action.performed += OnMiddleClickPerformed;
		}
		if (m_RightClickAction != null && m_RightClickAction.action != null)
		{
			m_RightClickAction.action.performed += OnRightClickPerformed;
		}
		if (m_ScrollWheelAction != null && m_ScrollWheelAction.action != null)
		{
			m_ScrollWheelAction.action.performed += OnScrollWheelPerformed;
		}
		if (InputSystem.actions == null)
		{
			m_InputActionAsset.FindActionMap("UI", throwIfNotFound: true).Enable();
		}
		else
		{
			m_InputActionAsset.Enable();
		}
	}

	private void UnregisterActions()
	{
		if (m_PointAction != null && m_PointAction.action != null)
		{
			m_PointAction.action.performed -= OnPointerPerformed;
		}
		if (m_SubmitAction != null && m_SubmitAction.action != null)
		{
			m_SubmitAction.action.performed -= OnSubmitPerformed;
		}
		if (m_CancelAction != null && m_CancelAction.action != null)
		{
			m_CancelAction.action.performed -= OnCancelPerformed;
		}
		if (m_LeftClickAction != null && m_LeftClickAction.action != null)
		{
			m_LeftClickAction.action.performed -= OnLeftClickPerformed;
		}
		if (m_MiddleClickAction != null && m_MiddleClickAction.action != null)
		{
			m_MiddleClickAction.action.performed -= OnMiddleClickPerformed;
		}
		if (m_RightClickAction != null && m_RightClickAction.action != null)
		{
			m_RightClickAction.action.performed -= OnRightClickPerformed;
		}
		if (m_ScrollWheelAction != null && m_ScrollWheelAction.action != null)
		{
			m_ScrollWheelAction.action.performed -= OnScrollWheelPerformed;
		}
		m_PointAction = null;
		m_MoveAction = null;
		m_SubmitAction = null;
		m_CancelAction = null;
		m_LeftClickAction = null;
		m_MiddleClickAction = null;
		m_RightClickAction = null;
		m_ScrollWheelAction = null;
		if (m_InputActionAsset != null)
		{
			m_InputActionAsset.Disable();
		}
	}

	private void SelectInputActionAsset()
	{
		InputActionAsset actions = InputSystem.actions;
		if (actions != null && actions.FindActionMap("UI") != null)
		{
			m_InputActionAsset = InputSystem.actions;
			return;
		}
		if (m_DefaultInputActions == null)
		{
			m_DefaultInputActions = new DefaultInputActions();
		}
		m_InputActionAsset = m_DefaultInputActions.asset;
	}

	internal static void SetOnRegisterActions(Action<InputActionAsset> callback)
	{
		s_OnRegisterActions = callback;
	}
}
