using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

[DefaultExecutionOrder(-200)]
public abstract class UIInputModule : BaseInputModule
{
	[Header("Configuration")]
	[SerializeField]
	[FormerlySerializedAs("clickSpeed")]
	[Tooltip("The maximum time (in seconds) between two mouse presses for it to be consecutive click.")]
	private float m_ClickSpeed = 0.3f;

	[SerializeField]
	[FormerlySerializedAs("moveDeadzone")]
	[Tooltip("The absolute value required by a move action on either axis required to trigger a move event.")]
	private float m_MoveDeadzone = 0.6f;

	[SerializeField]
	[FormerlySerializedAs("repeatDelay")]
	[Tooltip("The Initial delay (in seconds) between an initial move action and a repeated move action.")]
	private float m_RepeatDelay = 0.5f;

	[FormerlySerializedAs("repeatRate")]
	[SerializeField]
	[Tooltip("The speed (in seconds) that the move action repeats itself once repeating.")]
	private float m_RepeatRate = 0.1f;

	[FormerlySerializedAs("trackedDeviceDragThresholdMultiplier")]
	[SerializeField]
	[Tooltip("Scales the EventSystem.pixelDragThreshold, for tracked devices, to make selection easier.")]
	private float m_TrackedDeviceDragThresholdMultiplier = 2f;

	[SerializeField]
	[Tooltip("Scales the scrollDelta in event data, for tracked devices, to scroll at an expected speed.")]
	private float m_TrackedScrollDeltaMultiplier = 5f;

	[SerializeField]
	[Tooltip("Disables sending events from Event System to UI Toolkit on behalf of this Input Module.")]
	private bool m_BypassUIToolkitEvents;

	private Camera m_UICamera;

	private Camera m_MainCameraCache;

	private AxisEventData m_CachedAxisEvent;

	private readonly Dictionary<int, PointerEventData> m_PointerEventByPointerId = new Dictionary<int, PointerEventData>();

	private readonly Dictionary<int, TrackedDeviceEventData> m_TrackedDeviceEventByPointerId = new Dictionary<int, TrackedDeviceEventData>();

	public float clickSpeed
	{
		get
		{
			return m_ClickSpeed;
		}
		set
		{
			m_ClickSpeed = value;
		}
	}

	public float moveDeadzone
	{
		get
		{
			return m_MoveDeadzone;
		}
		set
		{
			m_MoveDeadzone = value;
		}
	}

	public float repeatDelay
	{
		get
		{
			return m_RepeatDelay;
		}
		set
		{
			m_RepeatDelay = value;
		}
	}

	public float repeatRate
	{
		get
		{
			return m_RepeatRate;
		}
		set
		{
			m_RepeatRate = value;
		}
	}

	public float trackedDeviceDragThresholdMultiplier
	{
		get
		{
			return m_TrackedDeviceDragThresholdMultiplier;
		}
		set
		{
			m_TrackedDeviceDragThresholdMultiplier = value;
		}
	}

	public float trackedScrollDeltaMultiplier
	{
		get
		{
			return m_TrackedScrollDeltaMultiplier;
		}
		set
		{
			m_TrackedScrollDeltaMultiplier = value;
		}
	}

	public bool bypassUIToolkitEvents
	{
		get
		{
			return m_BypassUIToolkitEvents;
		}
		set
		{
			m_BypassUIToolkitEvents = value;
		}
	}

	public Camera uiCamera
	{
		get
		{
			if (m_UICamera != null)
			{
				return m_UICamera;
			}
			if (m_MainCameraCache == null || !m_MainCameraCache.isActiveAndEnabled)
			{
				m_MainCameraCache = Camera.main;
			}
			return m_MainCameraCache;
		}
		set
		{
			m_UICamera = value;
		}
	}

	public event Action<PointerEventData, List<RaycastResult>> finalizeRaycastResults;

	public event Action<GameObject, PointerEventData> pointerEnter;

	public event Action<GameObject, PointerEventData> pointerExit;

	public event Action<GameObject, PointerEventData> pointerDown;

	public event Action<GameObject, PointerEventData> pointerUp;

	public event Action<GameObject, PointerEventData> pointerClick;

	public event Action<GameObject, PointerEventData> pointerMove;

	public event Action<GameObject, PointerEventData> initializePotentialDrag;

	public event Action<GameObject, PointerEventData> beginDrag;

	public event Action<GameObject, PointerEventData> drag;

	public event Action<GameObject, PointerEventData> endDrag;

	public event Action<GameObject, PointerEventData> drop;

	public event Action<GameObject, PointerEventData> scroll;

	public event Action<GameObject, BaseEventData> updateSelected;

	public event Action<GameObject, AxisEventData> move;

	public event Action<GameObject, BaseEventData> submit;

	public event Action<GameObject, BaseEventData> cancel;

	protected virtual void Update()
	{
		if (base.eventSystem.IsActive() && base.eventSystem.currentInputModule == this && base.eventSystem == EventSystem.current)
		{
			DoProcess();
		}
	}

	protected virtual void DoProcess()
	{
		SendUpdateEventToSelectedObject();
	}

	public override void Process()
	{
	}

	protected bool SendUpdateEventToSelectedObject()
	{
		GameObject currentSelectedGameObject = base.eventSystem.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		this.updateSelected?.Invoke(currentSelectedGameObject, baseEventData);
		ExecuteEvents.Execute(currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
		if (bypassUIToolkitEvents)
		{
			EventSystem.SetUITookitEventSystemOverride(base.eventSystem, sendEvents: false, createPanelGameObjectsOnStart: false);
		}
		GameObject gameObject = base.eventSystem.currentSelectedGameObject;
		if (gameObject == null)
		{
			gameObject = base.eventSystem.firstSelectedGameObject;
		}
		base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
	}

	public GameObject GetCurrentGameObject(int pointerId)
	{
		if (pointerId < 0)
		{
			foreach (TrackedDeviceEventData value3 in m_TrackedDeviceEventByPointerId.Values)
			{
				if (value3 != null && value3.pointerEnter != null)
				{
					return value3.pointerEnter;
				}
			}
			foreach (PointerEventData value4 in m_PointerEventByPointerId.Values)
			{
				if (value4 != null && value4.pointerEnter != null)
				{
					return value4.pointerEnter;
				}
			}
		}
		else
		{
			if (m_TrackedDeviceEventByPointerId.TryGetValue(pointerId, out var value))
			{
				return value?.pointerEnter;
			}
			if (m_PointerEventByPointerId.TryGetValue(pointerId, out var value2))
			{
				return value2?.pointerEnter;
			}
		}
		return null;
	}

	public override bool IsPointerOverGameObject(int pointerId)
	{
		return GetCurrentGameObject(pointerId) != null;
	}

	private RaycastResult PerformRaycast(PointerEventData eventData)
	{
		if (eventData == null)
		{
			throw new ArgumentNullException("eventData");
		}
		base.eventSystem.RaycastAll(eventData, m_RaycastResultCache);
		this.finalizeRaycastResults?.Invoke(eventData, m_RaycastResultCache);
		RaycastResult result = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
		m_RaycastResultCache.Clear();
		return result;
	}

	private protected void ProcessPointerState(ref PointerModel pointerState)
	{
		if (pointerState.changedThisFrame)
		{
			PointerEventData orCreateCachedPointerEvent = GetOrCreateCachedPointerEvent(pointerState.pointerId);
			orCreateCachedPointerEvent.Reset();
			pointerState.CopyTo(orCreateCachedPointerEvent);
			orCreateCachedPointerEvent.pointerCurrentRaycast = PerformRaycast(orCreateCachedPointerEvent);
			MouseButtonModel leftButton = pointerState.leftButton;
			orCreateCachedPointerEvent.button = PointerEventData.InputButton.Left;
			leftButton.CopyTo(orCreateCachedPointerEvent);
			ProcessPointerButton(leftButton.lastFrameDelta, orCreateCachedPointerEvent);
			ProcessPointerMovement(orCreateCachedPointerEvent);
			ProcessScrollWheel(orCreateCachedPointerEvent);
			pointerState.CopyFrom(orCreateCachedPointerEvent);
			ProcessPointerButtonDrag(orCreateCachedPointerEvent, UIPointerType.MouseOrPen);
			leftButton.CopyFrom(orCreateCachedPointerEvent);
			pointerState.leftButton = leftButton;
			leftButton = pointerState.rightButton;
			orCreateCachedPointerEvent.button = PointerEventData.InputButton.Right;
			leftButton.CopyTo(orCreateCachedPointerEvent);
			ProcessPointerButton(leftButton.lastFrameDelta, orCreateCachedPointerEvent);
			ProcessPointerButtonDrag(orCreateCachedPointerEvent, UIPointerType.MouseOrPen);
			leftButton.CopyFrom(orCreateCachedPointerEvent);
			pointerState.rightButton = leftButton;
			leftButton = pointerState.middleButton;
			orCreateCachedPointerEvent.button = PointerEventData.InputButton.Middle;
			leftButton.CopyTo(orCreateCachedPointerEvent);
			ProcessPointerButton(leftButton.lastFrameDelta, orCreateCachedPointerEvent);
			ProcessPointerButtonDrag(orCreateCachedPointerEvent, UIPointerType.MouseOrPen);
			leftButton.CopyFrom(orCreateCachedPointerEvent);
			pointerState.middleButton = leftButton;
			pointerState.OnFrameFinished();
		}
	}

	private void ProcessPointerMovement(PointerEventData eventData)
	{
		GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
		bool flag = eventData.IsPointerMoving();
		if (flag)
		{
			for (int i = 0; i < eventData.hovered.Count; i++)
			{
				this.pointerMove?.Invoke(eventData.hovered[i], eventData);
				ExecuteEvents.Execute(eventData.hovered[i], eventData, ExecuteEvents.pointerMoveHandler);
			}
		}
		if (gameObject == null || eventData.pointerEnter == null)
		{
			foreach (GameObject item in eventData.hovered)
			{
				this.pointerExit?.Invoke(item, eventData);
				ExecuteEvents.Execute(item, eventData, ExecuteEvents.pointerExitHandler);
			}
			eventData.hovered.Clear();
			if (gameObject == null)
			{
				eventData.pointerEnter = null;
				return;
			}
		}
		if (eventData.pointerEnter == gameObject)
		{
			return;
		}
		GameObject gameObject2 = BaseInputModule.FindCommonRoot(eventData.pointerEnter, gameObject);
		if (eventData.pointerEnter != null)
		{
			Transform parent = eventData.pointerEnter.transform;
			while (parent != null && (!(gameObject2 != null) || !(gameObject2.transform == parent)))
			{
				GameObject gameObject3 = parent.gameObject;
				this.pointerExit?.Invoke(gameObject3, eventData);
				ExecuteEvents.Execute(gameObject3, eventData, ExecuteEvents.pointerExitHandler);
				eventData.hovered.Remove(gameObject3);
				parent = parent.parent;
			}
		}
		eventData.pointerEnter = gameObject;
		if (!(gameObject != null))
		{
			return;
		}
		Transform parent2 = gameObject.transform;
		while (parent2 != null && parent2.gameObject != gameObject2)
		{
			GameObject gameObject4 = parent2.gameObject;
			this.pointerEnter?.Invoke(gameObject4, eventData);
			ExecuteEvents.Execute(gameObject4, eventData, ExecuteEvents.pointerEnterHandler);
			if (flag)
			{
				this.pointerMove?.Invoke(gameObject4, eventData);
				ExecuteEvents.Execute(gameObject4, eventData, ExecuteEvents.pointerMoveHandler);
			}
			eventData.hovered.Add(gameObject4);
			parent2 = parent2.parent;
		}
	}

	private void ProcessPointerButton(ButtonDeltaState mouseButtonChanges, PointerEventData eventData, bool clickOnDown = false)
	{
		GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
		if ((mouseButtonChanges & ButtonDeltaState.Pressed) != 0)
		{
			eventData.eligibleForClick = true;
			eventData.delta = Vector2.zero;
			eventData.dragging = false;
			eventData.pressPosition = eventData.position;
			eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
			eventData.useDragThreshold = true;
			GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(gameObject);
			if (base.eventSystem.currentSelectedGameObject != null && eventHandler != base.eventSystem.currentSelectedGameObject)
			{
				base.eventSystem.SetSelectedGameObject(null, eventData);
			}
			this.pointerDown?.Invoke(gameObject, eventData);
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, eventData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == eventData.lastPress && unscaledTime - eventData.clickTime < m_ClickSpeed)
			{
				int clickCount = eventData.clickCount + 1;
				eventData.clickCount = clickCount;
			}
			else
			{
				eventData.clickCount = 1;
			}
			eventData.clickTime = unscaledTime;
			eventData.pointerPress = gameObject2;
			eventData.rawPointerPress = gameObject;
			GameObject gameObject3 = (eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject));
			if (gameObject3 != null)
			{
				this.initializePotentialDrag?.Invoke(gameObject3, eventData);
				ExecuteEvents.Execute(gameObject3, eventData, ExecuteEvents.initializePotentialDrag);
			}
			GameObject pointerPress = eventData.pointerPress;
			if (clickOnDown && CanTargetClickOnDown(pointerPress))
			{
				mouseButtonChanges = ButtonDeltaState.Released;
			}
		}
		if ((mouseButtonChanges & ButtonDeltaState.Released) != 0)
		{
			GameObject pointerPress2 = eventData.pointerPress;
			this.pointerUp?.Invoke(pointerPress2, eventData);
			ExecuteEvents.Execute(pointerPress2, eventData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler3 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			GameObject pointerDrag = eventData.pointerDrag;
			if (pointerPress2 == eventHandler3 && eventData.eligibleForClick)
			{
				this.pointerClick?.Invoke(pointerPress2, eventData);
				ExecuteEvents.Execute(pointerPress2, eventData, ExecuteEvents.pointerClickHandler);
			}
			else if (eventData.dragging && pointerDrag != null)
			{
				this.drop?.Invoke(gameObject, eventData);
				ExecuteEvents.ExecuteHierarchy(gameObject, eventData, ExecuteEvents.dropHandler);
			}
			eventData.eligibleForClick = false;
			eventData.pointerPress = null;
			eventData.rawPointerPress = null;
			if (eventData.dragging && pointerDrag != null)
			{
				this.endDrag?.Invoke(pointerDrag, eventData);
				ExecuteEvents.Execute(pointerDrag, eventData, ExecuteEvents.endDragHandler);
			}
			eventData.dragging = false;
			eventData.pointerDrag = null;
		}
	}

	private void ProcessPointerButtonDrag(PointerEventData eventData, UIPointerType pointerType, float pixelDragThresholdMultiplier = 1f)
	{
		if (!eventData.IsPointerMoving() || (pointerType == UIPointerType.MouseOrPen && Cursor.lockState == CursorLockMode.Locked) || eventData.pointerDrag == null)
		{
			return;
		}
		if (!eventData.dragging)
		{
			float num = (float)base.eventSystem.pixelDragThreshold * pixelDragThresholdMultiplier;
			if (!eventData.useDragThreshold || (eventData.pressPosition - eventData.position).sqrMagnitude >= num * num)
			{
				GameObject pointerDrag = eventData.pointerDrag;
				this.beginDrag?.Invoke(pointerDrag, eventData);
				ExecuteEvents.Execute(pointerDrag, eventData, ExecuteEvents.beginDragHandler);
				eventData.dragging = true;
			}
		}
		if (eventData.dragging)
		{
			GameObject pointerPress = eventData.pointerPress;
			if (pointerPress != eventData.pointerDrag)
			{
				this.pointerUp?.Invoke(pointerPress, eventData);
				ExecuteEvents.Execute(pointerPress, eventData, ExecuteEvents.pointerUpHandler);
				eventData.eligibleForClick = false;
				eventData.pointerPress = null;
				eventData.rawPointerPress = null;
			}
			this.drag?.Invoke(eventData.pointerDrag, eventData);
			ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
		}
	}

	private void ProcessScrollWheel(PointerEventData eventData)
	{
		if (!Mathf.Approximately(eventData.scrollDelta.sqrMagnitude, 0f))
		{
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.pointerEnter);
			this.scroll?.Invoke(eventHandler, eventData);
			ExecuteEvents.ExecuteHierarchy(eventHandler, eventData, ExecuteEvents.scrollHandler);
		}
	}

	private protected void ProcessTrackedDevice(ref TrackedDeviceModel deviceState, bool force = false)
	{
		if (!deviceState.changedThisFrame && !force)
		{
			return;
		}
		TrackedDeviceEventData orCreateCachedTrackedDeviceEvent = GetOrCreateCachedTrackedDeviceEvent(deviceState.pointerId);
		orCreateCachedTrackedDeviceEvent.Reset();
		deviceState.CopyTo(orCreateCachedTrackedDeviceEvent);
		orCreateCachedTrackedDeviceEvent.scrollDelta *= m_TrackedScrollDeltaMultiplier;
		orCreateCachedTrackedDeviceEvent.button = PointerEventData.InputButton.Left;
		Vector2 position = orCreateCachedTrackedDeviceEvent.position;
		Vector2 delta = orCreateCachedTrackedDeviceEvent.delta;
		orCreateCachedTrackedDeviceEvent.position = new Vector2(-1f, -1f);
		orCreateCachedTrackedDeviceEvent.delta = Vector2.zero;
		orCreateCachedTrackedDeviceEvent.pointerCurrentRaycast = PerformRaycast(orCreateCachedTrackedDeviceEvent);
		orCreateCachedTrackedDeviceEvent.position = position;
		orCreateCachedTrackedDeviceEvent.delta = delta;
		if (TryGetCamera(orCreateCachedTrackedDeviceEvent, out var screenPointCamera))
		{
			Vector2 vector;
			if (orCreateCachedTrackedDeviceEvent.pointerCurrentRaycast.isValid)
			{
				vector = screenPointCamera.WorldToScreenPoint(orCreateCachedTrackedDeviceEvent.pointerCurrentRaycast.worldPosition);
				if ((deviceState.selectDelta & ButtonDeltaState.Pressed) != 0)
				{
					orCreateCachedTrackedDeviceEvent.pressWorldPosition = orCreateCachedTrackedDeviceEvent.pointerCurrentRaycast.worldPosition;
				}
			}
			else
			{
				Vector3 position2 = ((orCreateCachedTrackedDeviceEvent.rayPoints.Count > 0) ? orCreateCachedTrackedDeviceEvent.rayPoints[orCreateCachedTrackedDeviceEvent.rayPoints.Count - 1] : Vector3.zero);
				vector = (orCreateCachedTrackedDeviceEvent.position = screenPointCamera.WorldToScreenPoint(position2));
			}
			Vector2 delta2 = vector - orCreateCachedTrackedDeviceEvent.position;
			orCreateCachedTrackedDeviceEvent.position = vector;
			orCreateCachedTrackedDeviceEvent.delta = delta2;
			ProcessPointerButton(deviceState.selectDelta, orCreateCachedTrackedDeviceEvent, deviceState.clickOnDown);
			ProcessPointerMovement(orCreateCachedTrackedDeviceEvent);
			ProcessScrollWheel(orCreateCachedTrackedDeviceEvent);
			if (orCreateCachedTrackedDeviceEvent.pressPosition != Vector2.zero)
			{
				orCreateCachedTrackedDeviceEvent.pressPosition = screenPointCamera.WorldToScreenPoint(orCreateCachedTrackedDeviceEvent.pressWorldPosition);
			}
			ProcessPointerButtonDrag(orCreateCachedTrackedDeviceEvent, UIPointerType.Tracked, m_TrackedDeviceDragThresholdMultiplier);
			GameObject pointerTarget = deviceState.implementationData.pointerTarget;
			deviceState.CopyFrom(orCreateCachedTrackedDeviceEvent);
			GameObject pointerTarget2 = deviceState.implementationData.pointerTarget;
			if (pointerTarget != pointerTarget2)
			{
				if (pointerTarget2 != null)
				{
					ISelectHandler componentInParent = pointerTarget2.GetComponentInParent<ISelectHandler>();
					IScrollHandler componentInParent2 = pointerTarget2.GetComponentInParent<IScrollHandler>();
					deviceState.selectableObject = (componentInParent as Component)?.gameObject;
					deviceState.isScrollable = componentInParent2 != null;
				}
				else
				{
					deviceState.selectableObject = null;
					deviceState.isScrollable = false;
				}
			}
		}
		deviceState.OnFrameFinished();
	}

	private bool TryGetCamera(PointerEventData eventData, out Camera screenPointCamera)
	{
		screenPointCamera = uiCamera;
		if (screenPointCamera != null)
		{
			return true;
		}
		BaseRaycaster module = eventData.pointerCurrentRaycast.module;
		if (module != null)
		{
			screenPointCamera = module.eventCamera;
			return screenPointCamera != null;
		}
		return false;
	}

	private protected void ProcessNavigationState(ref NavigationModel navigationState)
	{
		bool flag = SendUpdateEventToSelectedObject();
		if (!base.eventSystem.sendNavigationEvents)
		{
			return;
		}
		NavigationModel.ImplementationData implementationData = navigationState.implementationData;
		GameObject currentSelectedGameObject = base.eventSystem.currentSelectedGameObject;
		Vector2 moveVector = navigationState.move;
		if (!flag && (!Mathf.Approximately(moveVector.x, 0f) || !Mathf.Approximately(moveVector.y, 0f)))
		{
			float unscaledTime = Time.unscaledTime;
			MoveDirection moveDirection = MoveDirection.None;
			if (moveVector.sqrMagnitude > m_MoveDeadzone * m_MoveDeadzone)
			{
				moveDirection = ((!(Mathf.Abs(moveVector.x) > Mathf.Abs(moveVector.y))) ? ((moveVector.y > 0f) ? MoveDirection.Up : MoveDirection.Down) : ((moveVector.x > 0f) ? MoveDirection.Right : MoveDirection.Left));
			}
			if (moveDirection != implementationData.lastMoveDirection)
			{
				implementationData.consecutiveMoveCount = 0;
			}
			if (moveDirection != MoveDirection.None)
			{
				bool flag2 = true;
				if (implementationData.consecutiveMoveCount != 0)
				{
					flag2 = ((implementationData.consecutiveMoveCount <= 1) ? (unscaledTime > implementationData.lastMoveTime + m_RepeatDelay) : (unscaledTime > implementationData.lastMoveTime + m_RepeatRate));
				}
				if (flag2)
				{
					AxisEventData orCreateCachedAxisEvent = GetOrCreateCachedAxisEvent();
					orCreateCachedAxisEvent.Reset();
					orCreateCachedAxisEvent.moveVector = moveVector;
					orCreateCachedAxisEvent.moveDir = moveDirection;
					this.move?.Invoke(currentSelectedGameObject, orCreateCachedAxisEvent);
					ExecuteEvents.Execute(currentSelectedGameObject, orCreateCachedAxisEvent, ExecuteEvents.moveHandler);
					flag = orCreateCachedAxisEvent.used;
					implementationData.consecutiveMoveCount++;
					implementationData.lastMoveTime = unscaledTime;
					implementationData.lastMoveDirection = moveDirection;
				}
			}
			else
			{
				implementationData.consecutiveMoveCount = 0;
			}
		}
		else
		{
			implementationData.consecutiveMoveCount = 0;
		}
		if (!flag && currentSelectedGameObject != null)
		{
			BaseEventData baseEventData = GetBaseEventData();
			if ((navigationState.submitButtonDelta & ButtonDeltaState.Pressed) != 0)
			{
				this.submit?.Invoke(currentSelectedGameObject, baseEventData);
				ExecuteEvents.Execute(currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
			}
			if (!baseEventData.used && (navigationState.cancelButtonDelta & ButtonDeltaState.Pressed) != 0)
			{
				this.cancel?.Invoke(currentSelectedGameObject, baseEventData);
				ExecuteEvents.Execute(currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
			}
		}
		navigationState.implementationData = implementationData;
		navigationState.OnFrameFinished();
	}

	private protected void RemovePointerEventData(int pointerId)
	{
		if (!m_TrackedDeviceEventByPointerId.Remove(pointerId))
		{
			m_PointerEventByPointerId.Remove(pointerId);
		}
	}

	private PointerEventData GetOrCreateCachedPointerEvent(int pointerId)
	{
		if (!m_PointerEventByPointerId.TryGetValue(pointerId, out var value))
		{
			value = new PointerEventData(base.eventSystem);
			m_PointerEventByPointerId.Add(pointerId, value);
		}
		return value;
	}

	private TrackedDeviceEventData GetOrCreateCachedTrackedDeviceEvent(int pointerId)
	{
		if (!m_TrackedDeviceEventByPointerId.TryGetValue(pointerId, out var value))
		{
			value = new TrackedDeviceEventData(base.eventSystem);
			m_TrackedDeviceEventByPointerId.Add(pointerId, value);
		}
		return value;
	}

	private AxisEventData GetOrCreateCachedAxisEvent()
	{
		AxisEventData axisEventData = m_CachedAxisEvent;
		if (axisEventData == null)
		{
			axisEventData = (m_CachedAxisEvent = new AxisEventData(base.eventSystem));
		}
		return axisEventData;
	}

	private static bool CanTargetClickOnDown(GameObject clickOnDownTarget)
	{
		if (clickOnDownTarget == null || !clickOnDownTarget.TryGetComponent<Selectable>(out var component))
		{
			return false;
		}
		IScrollHandler scrollHandler = clickOnDownTarget.transform.parent?.GetComponentInParent<IScrollHandler>();
		if (scrollHandler != null)
		{
			if (!(scrollHandler is ScrollRect scrollRect))
			{
				return false;
			}
			if (scrollRect.IsActive())
			{
				if ((object)scrollRect.content == null)
				{
					return false;
				}
				Rect rect = scrollRect.content.rect;
				Rect rect2 = ((scrollRect.viewport != null) ? scrollRect.viewport.rect : ((RectTransform)scrollRect.transform).rect);
				if (scrollRect.vertical && rect.height > rect2.height)
				{
					return false;
				}
				if (scrollRect.horizontal && rect.width > rect2.width)
				{
					return false;
				}
			}
		}
		if (component is Button || component is Toggle || component is InputField || component is Dropdown)
		{
			return true;
		}
		if (component is TMP_InputField || component is TMP_Dropdown)
		{
			return true;
		}
		return false;
	}
}
