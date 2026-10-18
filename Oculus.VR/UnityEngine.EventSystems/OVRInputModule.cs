using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-isdk-input-processing/")]
public class OVRInputModule : PointerInputModule
{
	[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
	public enum InputMode
	{
		Mouse,
		Buttons
	}

	public interface InputSource
	{
		bool IsPressed();

		bool IsReleased();

		Transform GetPointerRayTransform();

		bool IsValid();

		bool IsActive();

		void UpdatePointerRay(OVRInputRayData rayData);

		OVRPlugin.Hand GetHand();
	}

	[Tooltip("Object which points with Z axis. E.g. CentreEyeAnchor from OVRCameraRig")]
	public Transform rayTransform;

	public OVRCursor m_Cursor;

	[Tooltip("Gamepad button to act as gaze click")]
	public OVRInput.Button joyPadClickButton = OVRInput.Button.One;

	[Tooltip("Keyboard button to act as gaze click")]
	public KeyCode gazeClickKey = KeyCode.Space;

	[Header("Physics")]
	[Tooltip("Perform an sphere cast to determine correct depth for gaze pointer")]
	public bool performSphereCastForGazepointer;

	[Header("Gamepad Stick Scroll")]
	[Tooltip("Enable scrolling with the right stick on a gamepad")]
	public bool useRightStickScroll = true;

	[Tooltip("Deadzone for right stick to prevent accidental scrolling")]
	public float rightStickDeadZone = 0.15f;

	[Header("Touchpad Swipe Scroll")]
	[Tooltip("Enable scrolling by swiping the touchpad")]
	public bool useSwipeScroll = true;

	[Tooltip("Minimum trackpad movement in pixels to start swiping")]
	public float swipeDragThreshold = 2f;

	[Tooltip("Distance scrolled when swipe scroll occurs")]
	public float swipeDragScale = 1f;

	[Tooltip("Invert X axis on touchpad")]
	public bool InvertSwipeXAxis;

	[NonSerialized]
	public OVRRaycaster activeGraphicRaycaster;

	[Header("Dragging")]
	[Tooltip("Minimum pointer movement in degrees to start dragging")]
	public float angleDragThreshold = 1f;

	[SerializeField]
	private float m_SpherecastRadius = 1f;

	private float m_NextAction;

	private Vector2 m_LastMousePosition;

	private Vector2 m_MousePosition;

	protected HashSet<GameObject> _objectsHitThisFrame = new HashSet<GameObject>();

	[Header("Standalone Input Module")]
	[SerializeField]
	private string m_HorizontalAxis = "Horizontal";

	[SerializeField]
	private string m_VerticalAxis = "Vertical";

	[SerializeField]
	private string m_SubmitButton = "Submit";

	[SerializeField]
	private string m_CancelButton = "Cancel";

	[SerializeField]
	private float m_InputActionsPerSecond = 10f;

	[SerializeField]
	private bool m_AllowActivationOnMobileDevice;

	protected Dictionary<int, OVRPointerEventData> m_VRRayPointerData = new Dictionary<int, OVRPointerEventData>();

	protected readonly MouseState m_MouseState = new MouseState();

	private List<InputSource> _trackedInputSources = new List<InputSource>();

	private static List<InputSource> _pendingInputSources = new List<InputSource>();

	[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
	public InputMode inputMode => InputMode.Mouse;

	public bool allowActivationOnMobileDevice
	{
		get
		{
			return m_AllowActivationOnMobileDevice;
		}
		set
		{
			m_AllowActivationOnMobileDevice = value;
		}
	}

	public float inputActionsPerSecond
	{
		get
		{
			return m_InputActionsPerSecond;
		}
		set
		{
			m_InputActionsPerSecond = value;
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

	public static OVRInputModule instance { get; private set; }

	protected OVRInputModule()
	{
	}

	protected override void Awake()
	{
		base.Awake();
		instance = this;
		for (int i = 0; i < _pendingInputSources.Count; i++)
		{
			InputSource inputSource = _pendingInputSources[i];
			if (inputSource != null && inputSource.IsValid())
			{
				TrackInputSource(inputSource);
			}
		}
		_pendingInputSources.Clear();
	}

	public override void UpdateModule()
	{
	}

	public override bool IsModuleSupported()
	{
		if (!m_AllowActivationOnMobileDevice)
		{
			return Input.mousePresent;
		}
		return true;
	}

	public override bool ShouldActivateModule()
	{
		base.ShouldActivateModule();
		return false;
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
		GameObject gameObject = base.eventSystem.currentSelectedGameObject;
		if (gameObject == null)
		{
			gameObject = base.eventSystem.firstSelectedGameObject;
		}
		base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		ClearSelection();
	}

	private bool SendSubmitEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		return GetBaseEventData().used;
	}

	private bool AllowMoveEventProcessing(float time)
	{
		return false;
	}

	private Vector2 GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxisRaw(m_HorizontalAxis);
		zero.y = Input.GetAxisRaw(m_VerticalAxis);
		if (Input.GetButtonDown(m_HorizontalAxis))
		{
			if (zero.x < 0f)
			{
				zero.x = -1f;
			}
			if (zero.x > 0f)
			{
				zero.x = 1f;
			}
		}
		if (Input.GetButtonDown(m_VerticalAxis))
		{
			if (zero.y < 0f)
			{
				zero.y = -1f;
			}
			if (zero.y > 0f)
			{
				zero.y = 1f;
			}
		}
		return zero;
	}

	private bool SendMoveEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		if (!AllowMoveEventProcessing(unscaledTime))
		{
			return false;
		}
		Vector2 rawMoveVector = GetRawMoveVector();
		AxisEventData axisEventData = GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
		if (!Mathf.Approximately(axisEventData.moveVector.x, 0f) || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
		}
		m_NextAction = unscaledTime + 1f / m_InputActionsPerSecond;
		return axisEventData.used;
	}

	private bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	private void ProcessMousePress(MouseButtonEventData data)
	{
		PointerEventData buttonData = data.buttonData;
		GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			buttonData.eligibleForClick = true;
			buttonData.delta = Vector2.zero;
			buttonData.dragging = false;
			buttonData.useDragThreshold = true;
			buttonData.pressPosition = buttonData.position;
			buttonData.IsVRPointer();
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			DeselectIfSelectionChanged(gameObject, buttonData);
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			if (!_objectsHitThisFrame.Contains(gameObject2))
			{
				_objectsHitThisFrame.Add(gameObject2);
				float unscaledTime = Time.unscaledTime;
				if (gameObject2 == buttonData.lastPress)
				{
					if (unscaledTime - buttonData.clickTime < 0.3f)
					{
						int clickCount = buttonData.clickCount + 1;
						buttonData.clickCount = clickCount;
					}
					else
					{
						buttonData.clickCount = 1;
					}
					buttonData.clickTime = unscaledTime;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.pointerPress = gameObject2;
				buttonData.rawPointerPress = gameObject;
				buttonData.clickTime = unscaledTime;
				buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				if (buttonData.pointerDrag != null)
				{
					ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
				}
			}
		}
		if (data.ReleasedThisFrame())
		{
			ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
			{
				ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
			}
			else if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.dropHandler);
			}
			buttonData.eligibleForClick = false;
			buttonData.pointerPress = null;
			buttonData.rawPointerPress = null;
			if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
			if (gameObject != buttonData.pointerEnter)
			{
				HandlePointerExitAndEnter(buttonData, null);
				HandlePointerExitAndEnter(buttonData, gameObject);
			}
		}
	}

	protected void ProcessMouseEvent(MouseState mouseData)
	{
		bool pressed = mouseData.AnyPressesThisFrame();
		bool released = mouseData.AnyReleasesThisFrame();
		MouseButtonEventData eventData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;
		if (UseMouse(pressed, released, eventData.buttonData))
		{
			ProcessMousePress(eventData);
			ProcessMove(eventData.buttonData);
			ProcessDrag(eventData.buttonData);
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
			{
				ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), eventData.buttonData, ExecuteEvents.scrollHandler);
			}
		}
	}

	public override void Process()
	{
		bool flag = SendUpdateEventToSelectedObject();
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				SendSubmitEventToSelectedObject();
			}
		}
		bool flag2 = false;
		for (int num = _trackedInputSources.Count - 1; num >= 0; num--)
		{
			if (_trackedInputSources[num].IsActive())
			{
				flag2 = true;
				MouseState mouseStateFromInputSource = GetMouseStateFromInputSource(_trackedInputSources[num], num);
				ProcessMouseEvent(mouseStateFromInputSource);
			}
		}
		if (!flag2 && rayTransform != null)
		{
			ProcessMouseEvent(GetMouseStateFromRaycast(rayTransform));
		}
		_objectsHitThisFrame.Clear();
		ProcessMouseEvent(GetCanvasPointerData());
	}

	private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
	{
		if (pressed || released || IsPointerMoving(pointerData) || pointerData.IsScrolling())
		{
			return true;
		}
		return false;
	}

	protected void CopyFromTo(OVRPointerEventData from, OVRPointerEventData to)
	{
		to.position = from.position;
		to.delta = from.delta;
		to.scrollDelta = from.scrollDelta;
		to.pointerCurrentRaycast = from.pointerCurrentRaycast;
		to.pointerEnter = from.pointerEnter;
		to.worldSpaceRay = from.worldSpaceRay;
	}

	protected new void CopyFromTo(PointerEventData from, PointerEventData to)
	{
		to.position = from.position;
		to.delta = from.delta;
		to.scrollDelta = from.scrollDelta;
		to.pointerCurrentRaycast = from.pointerCurrentRaycast;
		to.pointerEnter = from.pointerEnter;
	}

	protected bool GetPointerData(int id, out OVRPointerEventData data, bool create)
	{
		if (!m_VRRayPointerData.TryGetValue(id, out data) && create)
		{
			data = new OVRPointerEventData(base.eventSystem)
			{
				pointerId = id
			};
			m_VRRayPointerData.Add(id, data);
			return true;
		}
		return false;
	}

	protected new void ClearSelection()
	{
		BaseEventData baseEventData = GetBaseEventData();
		foreach (PointerEventData value in m_PointerData.Values)
		{
			HandlePointerExitAndEnter(value, null);
		}
		foreach (OVRPointerEventData value2 in m_VRRayPointerData.Values)
		{
			HandlePointerExitAndEnter(value2, null);
		}
		m_PointerData.Clear();
		base.eventSystem.SetSelectedGameObject(null, baseEventData);
	}

	protected static Vector3 GetRectTransformNormal(RectTransform rectTransform)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector3 lhs = array[3] - array[0];
		Vector3 rhs = array[1] - array[0];
		rectTransform.GetWorldCorners(array);
		return Vector3.Cross(lhs, rhs).normalized;
	}

	protected virtual MouseState GetMouseStateFromInputSource(InputSource inputSource, int id)
	{
		Transform pointerRayTransform = inputSource.GetPointerRayTransform();
		GetPointerData(id, out var data, create: true);
		data.Reset();
		OVRInputRayData rayData = default(OVRInputRayData);
		if (pointerRayTransform != null)
		{
			data.worldSpaceRay = new Ray(pointerRayTransform.position, pointerRayTransform.forward);
			data.scrollDelta = GetExtraScrollDelta();
			data.button = PointerEventData.InputButton.Left;
			data.useDragThreshold = true;
			base.eventSystem.RaycastAll(data, m_RaycastResultCache);
			RaycastResult raycastResult2 = (data.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache));
			m_RaycastResultCache.Clear();
			rayData.IsOverCanvas = raycastResult2.isValid;
			if (rayData.IsOverCanvas)
			{
				rayData.DistanceToCanvas = raycastResult2.distance;
				rayData.WorldPosition = raycastResult2.worldPosition;
				rayData.WorldNormal = raycastResult2.worldNormal;
			}
			if ((bool)m_Cursor)
			{
				m_Cursor.SetCursorRay(pointerRayTransform);
			}
			OVRRaycaster oVRRaycaster = raycastResult2.module as OVRRaycaster;
			if ((bool)oVRRaycaster)
			{
				data.position = oVRRaycaster.GetScreenPosition(raycastResult2);
				if ((bool)m_Cursor && raycastResult2.gameObject.TryGetComponent<RectTransform>(out var component))
				{
					Vector3 worldPosition = raycastResult2.worldPosition;
					Vector3 rectTransformNormal = GetRectTransformNormal(component);
					m_Cursor.SetCursorStartDest(pointerRayTransform.position, worldPosition, rectTransformNormal);
				}
			}
			OVRPhysicsRaycaster oVRPhysicsRaycaster = raycastResult2.module as OVRPhysicsRaycaster;
			if ((bool)oVRPhysicsRaycaster)
			{
				Vector3 worldPosition2 = raycastResult2.worldPosition;
				if (performSphereCastForGazepointer)
				{
					List<RaycastResult> list = new List<RaycastResult>();
					oVRPhysicsRaycaster.Spherecast(data, list, m_SpherecastRadius);
					if (list.Count > 0 && list[0].distance < raycastResult2.distance)
					{
						worldPosition2 = list[0].worldPosition;
						rayData.IsOverCanvas |= list[0].isValid;
						rayData.DistanceToCanvas = list[0].distance;
						rayData.WorldPosition = list[0].worldPosition;
						rayData.WorldNormal = list[0].worldNormal;
					}
				}
				data.position = oVRPhysicsRaycaster.GetScreenPos(worldPosition2);
			}
		}
		GetPointerData(-2, out var data2, create: true);
		CopyFromTo(data, data2);
		data2.button = PointerEventData.InputButton.Right;
		GetPointerData(-3, out var data3, create: true);
		CopyFromTo(data, data3);
		data3.button = PointerEventData.InputButton.Middle;
		PointerEventData.FramePressState stateForMouseButton = PointerEventData.FramePressState.NotChanged;
		bool flag = inputSource.IsPressed();
		bool flag2 = inputSource.IsReleased();
		if (flag)
		{
			stateForMouseButton = (flag2 ? PointerEventData.FramePressState.PressedAndReleased : PointerEventData.FramePressState.Pressed);
		}
		else if (flag2)
		{
			stateForMouseButton = PointerEventData.FramePressState.Released;
		}
		rayData.IsActive = flag;
		inputSource.UpdatePointerRay(rayData);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Left, stateForMouseButton, data);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, data2);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, data3);
		return m_MouseState;
	}

	protected virtual MouseState GetMouseStateFromRaycast(Transform rayOrigin)
	{
		GetPointerData(-1, out var data, create: true);
		data.Reset();
		data.worldSpaceRay = new Ray(rayOrigin.position, rayOrigin.forward);
		data.scrollDelta = GetExtraScrollDelta();
		data.button = PointerEventData.InputButton.Left;
		data.useDragThreshold = true;
		base.eventSystem.RaycastAll(data, m_RaycastResultCache);
		RaycastResult raycastResult2 = (data.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache));
		m_RaycastResultCache.Clear();
		if ((bool)m_Cursor)
		{
			m_Cursor.SetCursorRay(rayOrigin);
		}
		OVRRaycaster oVRRaycaster = raycastResult2.module as OVRRaycaster;
		if ((bool)oVRRaycaster)
		{
			data.position = oVRRaycaster.GetScreenPosition(raycastResult2);
			if ((bool)m_Cursor && raycastResult2.gameObject.TryGetComponent<RectTransform>(out var component))
			{
				Vector3 worldPosition = raycastResult2.worldPosition;
				Vector3 rectTransformNormal = GetRectTransformNormal(component);
				m_Cursor.SetCursorStartDest(rayOrigin.position, worldPosition, rectTransformNormal);
			}
		}
		OVRPhysicsRaycaster oVRPhysicsRaycaster = raycastResult2.module as OVRPhysicsRaycaster;
		if ((bool)oVRPhysicsRaycaster)
		{
			Vector3 worldPosition2 = raycastResult2.worldPosition;
			if (performSphereCastForGazepointer)
			{
				List<RaycastResult> list = new List<RaycastResult>();
				oVRPhysicsRaycaster.Spherecast(data, list, m_SpherecastRadius);
				if (list.Count > 0 && list[0].distance < raycastResult2.distance)
				{
					worldPosition2 = list[0].worldPosition;
				}
			}
			data.position = oVRPhysicsRaycaster.GetScreenPos(worldPosition2);
			if ((bool)m_Cursor)
			{
				m_Cursor.SetCursorStartDest(rayOrigin.position, worldPosition2, raycastResult2.worldNormal);
			}
		}
		GetPointerData(-2, out var data2, create: true);
		CopyFromTo(data, data2);
		data2.button = PointerEventData.InputButton.Right;
		GetPointerData(-3, out var data3, create: true);
		CopyFromTo(data, data3);
		data3.button = PointerEventData.InputButton.Middle;
		m_MouseState.SetButtonState(PointerEventData.InputButton.Left, GetGazeButtonState(), data);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, data2);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, data3);
		return m_MouseState;
	}

	protected MouseState GetCanvasPointerData()
	{
		GetPointerData(-1, out PointerEventData data, create: true);
		data.Reset();
		data.position = Vector2.zero;
		data.button = PointerEventData.InputButton.Left;
		if ((bool)activeGraphicRaycaster)
		{
			activeGraphicRaycaster.RaycastPointer(data, m_RaycastResultCache);
			RaycastResult raycastResult2 = (data.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache));
			m_RaycastResultCache.Clear();
			OVRRaycaster oVRRaycaster = raycastResult2.module as OVRRaycaster;
			if ((bool)oVRRaycaster)
			{
				Vector2 screenPosition = oVRRaycaster.GetScreenPosition(raycastResult2);
				data.delta = screenPosition - data.position;
				data.position = screenPosition;
			}
		}
		GetPointerData(-2, out PointerEventData data2, create: true);
		CopyFromTo(data, data2);
		data2.button = PointerEventData.InputButton.Right;
		GetPointerData(-3, out PointerEventData data3, create: true);
		CopyFromTo(data, data3);
		data3.button = PointerEventData.InputButton.Middle;
		return m_MouseState;
	}

	private bool ShouldStartDrag(PointerEventData pointerEvent)
	{
		if (!pointerEvent.useDragThreshold)
		{
			return true;
		}
		if (!pointerEvent.IsVRPointer())
		{
			return (pointerEvent.pressPosition - pointerEvent.position).sqrMagnitude >= (float)(base.eventSystem.pixelDragThreshold * base.eventSystem.pixelDragThreshold);
		}
		Vector3 position = pointerEvent.pressEventCamera.transform.position;
		Vector3 normalized = (pointerEvent.pointerPressRaycast.worldPosition - position).normalized;
		Vector3 normalized2 = (pointerEvent.pointerCurrentRaycast.worldPosition - position).normalized;
		return Vector3.Dot(normalized, normalized2) < Mathf.Cos(MathF.PI / 180f * angleDragThreshold);
	}

	private static bool IsPointerMoving(PointerEventData pointerEvent)
	{
		if (pointerEvent.IsVRPointer())
		{
			return true;
		}
		return pointerEvent.IsPointerMoving();
	}

	protected Vector2 SwipeAdjustedPosition(Vector2 originalPosition, PointerEventData pointerEvent)
	{
		return originalPosition;
	}

	protected override void ProcessDrag(PointerEventData pointerEvent)
	{
		Vector2 position = pointerEvent.position;
		bool flag = IsPointerMoving(pointerEvent);
		if (flag && pointerEvent.pointerDrag != null && !pointerEvent.dragging && ShouldStartDrag(pointerEvent))
		{
			if (pointerEvent.IsVRPointer())
			{
				pointerEvent.position = SwipeAdjustedPosition(position, pointerEvent);
			}
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
			pointerEvent.dragging = true;
		}
		if (pointerEvent.dragging && flag && pointerEvent.pointerDrag != null)
		{
			if (pointerEvent.IsVRPointer())
			{
				pointerEvent.position = SwipeAdjustedPosition(position, pointerEvent);
			}
			if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
			}
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
		}
	}

	protected virtual PointerEventData.FramePressState GetGazeButtonState()
	{
		bool down = OVRInput.GetDown(joyPadClickButton);
		bool up = OVRInput.GetUp(joyPadClickButton);
		if (down && up)
		{
			return PointerEventData.FramePressState.PressedAndReleased;
		}
		if (down)
		{
			return PointerEventData.FramePressState.Pressed;
		}
		if (up)
		{
			return PointerEventData.FramePressState.Released;
		}
		return PointerEventData.FramePressState.NotChanged;
	}

	protected Vector2 GetExtraScrollDelta()
	{
		Vector2 result = default(Vector2);
		if (useRightStickScroll)
		{
			Vector2 result2 = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
			if (Mathf.Abs(result2.x) < rightStickDeadZone)
			{
				result2.x = 0f;
			}
			if (Mathf.Abs(result2.y) < rightStickDeadZone)
			{
				result2.y = 0f;
			}
			return result2;
		}
		return result;
	}

	public static void TrackInputSource(InputSource hand)
	{
		if ((bool)instance)
		{
			if (!instance._trackedInputSources.Contains(hand))
			{
				instance._trackedInputSources.Add(hand);
			}
		}
		else
		{
			_pendingInputSources.Add(hand);
		}
	}

	public static void UntrackInputSource(InputSource hand)
	{
		if ((bool)instance)
		{
			instance._trackedInputSources.Remove(hand);
		}
		else if (_pendingInputSources.Contains(hand))
		{
			_pendingInputSources.Remove(hand);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_trackedInputSources.Clear();
		_pendingInputSources.Clear();
	}
}
