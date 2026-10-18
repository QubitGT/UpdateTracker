using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Oculus.Interaction;

public class PointableCanvasModule : PointerInputModule
{
	public class Pointer
	{
		public int Identifier { get; }

		internal PointerEventData PointerEventData { get; set; }

		public event Action<PointerEventData> WhenUpdated = delegate
		{
		};

		public event Action WhenDisposed = delegate
		{
		};

		internal Pointer(int identifier)
		{
			Identifier = identifier;
		}

		internal void InvokeWhenUpdated()
		{
			this.WhenUpdated(PointerEventData);
		}

		internal void InvokeWhenDisposed()
		{
			this.WhenDisposed();
		}
	}

	private class PointerImpl : Pointer
	{
		private Canvas _canvas;

		private Vector3 _position;

		private Vector3 _targetPosition;

		private GameObject _hoveredSelectable;

		private bool _pressing;

		private bool _pressed;

		private bool _released;

		public bool MarkedForDeletion { get; private set; }

		public Canvas Canvas => _canvas;

		public Vector3 Position => _position;

		public GameObject HoveredSelectable => _hoveredSelectable;

		public PointerImpl(int identifier, Canvas canvas)
			: base(identifier)
		{
			_canvas = canvas;
			_pressed = (_released = false);
		}

		public void Press()
		{
			if (!_pressing)
			{
				_pressing = true;
				_pressed = true;
			}
		}

		public void Release()
		{
			if (_pressing)
			{
				_pressing = false;
				_released = true;
			}
		}

		public void ReadAndResetPressedReleased(out bool pressed, out bool released)
		{
			pressed = _pressed;
			released = _released;
			_pressed = (_released = false);
			_position = _targetPosition;
		}

		public void MarkForDeletion()
		{
			MarkedForDeletion = true;
			Release();
		}

		public void SetPosition(Vector3 position)
		{
			_targetPosition = position;
			if (!_released)
			{
				_position = position;
			}
		}

		public void SetHoveredSelectable(GameObject hoveredSelectable)
		{
			_hoveredSelectable = hoveredSelectable;
		}
	}

	[Tooltip("If true, the initial press position will be used as the drag start position, rather than the position when drag threshold is exceeded. This is used to prevent the pointer position shifting relative to the surface while dragging.")]
	[SerializeField]
	private bool _useInitialPressPositionForDrag = true;

	[Tooltip("If true, this module will disable other input modules in the event system and will be the only input module used in the scene.")]
	[SerializeField]
	private bool _exclusiveMode;

	private Camera _pointerEventCamera;

	private static PointableCanvasModule _instance;

	private Dictionary<int, PointerImpl> _pointerMap = new Dictionary<int, PointerImpl>();

	private List<RaycastResult> _raycastResultCache = new List<RaycastResult>();

	private List<PointerImpl> _pointersForDeletion = new List<PointerImpl>();

	private Dictionary<IPointableCanvas, Action<PointerEvent>> _pointerCanvasActionMap = new Dictionary<IPointableCanvas, Action<PointerEvent>>();

	private List<BaseInputModule> _inputModules = new List<BaseInputModule>();

	private PointerImpl[] _pointersToProcessScratch = Array.Empty<PointerImpl>();

	protected bool _started;

	public bool ExclusiveMode
	{
		get
		{
			return _exclusiveMode;
		}
		set
		{
			_exclusiveMode = value;
		}
	}

	private static PointableCanvasModule Instance => _instance;

	public static event Action<PointableCanvasEventArgs> WhenSelected;

	public static event Action<PointableCanvasEventArgs> WhenUnselected;

	public static event Action<PointableCanvasEventArgs> WhenSelectableHovered;

	public static event Action<PointableCanvasEventArgs> WhenSelectableUnhovered;

	public static event Action<Pointer> WhenPointerStarted;

	public static void RegisterPointableCanvas(IPointableCanvas pointerCanvas)
	{
		Instance.AddPointerCanvas(pointerCanvas);
	}

	public static void UnregisterPointableCanvas(IPointableCanvas pointerCanvas)
	{
		Instance?.RemovePointerCanvas(pointerCanvas);
	}

	private void AddPointerCanvas(IPointableCanvas pointerCanvas)
	{
		Action<PointerEvent> value = delegate(PointerEvent args)
		{
			HandlePointerEvent(pointerCanvas.Canvas, args);
		};
		_pointerCanvasActionMap.Add(pointerCanvas, value);
		pointerCanvas.WhenPointerEventRaised += value;
	}

	private void RemovePointerCanvas(IPointableCanvas pointerCanvas)
	{
		Action<PointerEvent> value = _pointerCanvasActionMap[pointerCanvas];
		_pointerCanvasActionMap.Remove(pointerCanvas);
		pointerCanvas.WhenPointerEventRaised -= value;
		foreach (int item in new List<int>(_pointerMap.Keys))
		{
			PointerImpl pointerImpl = _pointerMap[item];
			if (!(pointerImpl.Canvas != pointerCanvas.Canvas))
			{
				ClearPointerSelection(pointerImpl.PointerEventData);
				pointerImpl.MarkForDeletion();
				_pointersForDeletion.Add(pointerImpl);
				_pointerMap.Remove(item);
			}
		}
	}

	private void HandlePointerEvent(Canvas canvas, PointerEvent evt)
	{
		PointerImpl value;
		switch (evt.Type)
		{
		case PointerEventType.Hover:
			value = new PointerImpl(evt.Identifier, canvas);
			value.PointerEventData = new PointerEventData(base.eventSystem);
			value.SetPosition(evt.Pose.position);
			_pointerMap.Add(evt.Identifier, value);
			PointableCanvasModule.WhenPointerStarted?.Invoke(value);
			break;
		case PointerEventType.Unhover:
			if (_pointerMap.TryGetValue(evt.Identifier, out value))
			{
				_pointerMap.Remove(evt.Identifier);
				value.MarkForDeletion();
				_pointersForDeletion.Add(value);
			}
			break;
		case PointerEventType.Select:
			if (_pointerMap.TryGetValue(evt.Identifier, out value))
			{
				value.SetPosition(evt.Pose.position);
				value.Press();
			}
			break;
		case PointerEventType.Unselect:
			if (_pointerMap.TryGetValue(evt.Identifier, out value))
			{
				value.SetPosition(evt.Pose.position);
				value.Release();
			}
			break;
		case PointerEventType.Move:
			if (_pointerMap.TryGetValue(evt.Identifier, out value))
			{
				value.SetPosition(evt.Pose.position);
			}
			break;
		case PointerEventType.Cancel:
			if (_pointerMap.TryGetValue(evt.Identifier, out value))
			{
				_pointerMap.Remove(evt.Identifier);
				ClearPointerSelection(value.PointerEventData);
				value.MarkForDeletion();
				_pointersForDeletion.Add(value);
			}
			break;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_instance = this;
	}

	protected override void OnDestroy()
	{
		_instance = null;
		base.OnDestroy();
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		if (_exclusiveMode)
		{
			DisableOtherModules();
		}
		this.EndStart(ref _started);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_started)
		{
			_pointerEventCamera = base.gameObject.AddComponent<Camera>();
			_pointerEventCamera.nearClipPlane = 0.1f;
			_pointerEventCamera.enabled = false;
		}
	}

	protected override void OnDisable()
	{
		if (_started)
		{
			UnityEngine.Object.Destroy(_pointerEventCamera);
			_pointerEventCamera = null;
		}
		base.OnDisable();
	}

	private void DisableOtherModules()
	{
		GetComponents(_inputModules);
		foreach (BaseInputModule inputModule in _inputModules)
		{
			if (inputModule != this && inputModule.enabled)
			{
				inputModule.enabled = false;
				Debug.Log("PointableCanvasModule: Disabling " + inputModule.GetType().Name + ".");
			}
		}
	}

	public override void UpdateModule()
	{
		base.UpdateModule();
		if (_exclusiveMode && base.eventSystem.currentInputModule != null && base.eventSystem.currentInputModule != this)
		{
			DisableOtherModules();
		}
	}

	protected static RaycastResult FindFirstRaycastWithinCanvas(List<RaycastResult> candidates, Canvas canvas)
	{
		for (int i = 0; i < candidates.Count; i++)
		{
			GameObject gameObject = candidates[i].gameObject;
			if (!(gameObject == null))
			{
				Canvas componentInParent = gameObject.GetComponentInParent<Canvas>();
				if (!(componentInParent == null) && !(componentInParent.rootCanvas != canvas))
				{
					return candidates[i];
				}
			}
		}
		return default(RaycastResult);
	}

	private void UpdateRaycasts(PointerImpl pointer, out bool pressed, out bool released)
	{
		PointerEventData pointerEventData = pointer.PointerEventData;
		Vector2 position = pointerEventData.position;
		pointerEventData.Reset();
		Vector3 position2 = pointer.Position;
		pointer.ReadAndResetPressedReleased(out pressed, out released);
		if (pointer.MarkedForDeletion)
		{
			pointerEventData.pointerCurrentRaycast = default(RaycastResult);
			return;
		}
		Canvas canvas = pointer.Canvas;
		canvas.worldCamera = _pointerEventCamera;
		Vector3 position3 = Vector3.zero;
		Plane plane = new Plane(-1f * canvas.transform.forward, canvas.transform.position);
		Ray ray = new Ray(position2 - canvas.transform.forward, canvas.transform.forward);
		if (plane.Raycast(ray, out var enter))
		{
			position3 = ray.GetPoint(enter);
		}
		_pointerEventCamera.transform.position = position2 - canvas.transform.forward;
		_pointerEventCamera.transform.LookAt(position2, canvas.transform.up);
		Vector2 position4 = _pointerEventCamera.WorldToScreenPoint(position3);
		pointerEventData.position = position4;
		base.eventSystem.RaycastAll(pointerEventData, _raycastResultCache);
		RaycastResult pointerCurrentRaycast = FindFirstRaycastWithinCanvas(_raycastResultCache, canvas);
		pointer.PointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
		_raycastResultCache.Clear();
		_pointerEventCamera.transform.position = canvas.transform.position - canvas.transform.forward;
		_pointerEventCamera.transform.LookAt(canvas.transform.position, canvas.transform.up);
		position4 = _pointerEventCamera.WorldToScreenPoint(position3);
		pointerEventData.position = position4;
		if (pressed)
		{
			pointerEventData.delta = Vector2.zero;
		}
		else
		{
			pointerEventData.delta = pointerEventData.position - position;
		}
		pointerEventData.button = PointerEventData.InputButton.Left;
	}

	public override void Process()
	{
		ProcessPointers(_pointersForDeletion, clearAndReleasePointers: true);
		ProcessPointers(_pointerMap.Values, clearAndReleasePointers: false);
	}

	private void ProcessPointers(ICollection<PointerImpl> pointers, bool clearAndReleasePointers)
	{
		int count = pointers.Count;
		if (count == 0)
		{
			return;
		}
		if (count > _pointersToProcessScratch.Length)
		{
			_pointersToProcessScratch = new PointerImpl[count];
		}
		pointers.CopyTo(_pointersToProcessScratch, 0);
		if (clearAndReleasePointers)
		{
			pointers.Clear();
		}
		PointerImpl[] pointersToProcessScratch = _pointersToProcessScratch;
		foreach (PointerImpl pointerImpl in pointersToProcessScratch)
		{
			ProcessPointer(pointerImpl, clearAndReleasePointers);
			if (clearAndReleasePointers)
			{
				pointerImpl.InvokeWhenDisposed();
			}
		}
	}

	private void ProcessPointer(PointerImpl pointer, bool forceRelease = false)
	{
		bool pressed = false;
		bool released = false;
		bool dragging = pointer.PointerEventData.dragging;
		UpdateRaycasts(pointer, out pressed, out released);
		PointerEventData pointerEventData = pointer.PointerEventData;
		UpdatePointerEventData(pointerEventData, pressed, released);
		released = released || forceRelease;
		if (!released)
		{
			ProcessMove(pointerEventData);
			ProcessDrag(pointerEventData);
		}
		else
		{
			HandlePointerExitAndEnter(pointerEventData, null);
			RemovePointerData(pointerEventData);
		}
		HandleSelectableHover(pointer, dragging);
		HandleSelectablePress(pointer, pressed, released, dragging);
		pointer.InvokeWhenUpdated();
	}

	private void HandleSelectableHover(PointerImpl pointer, bool wasDragging)
	{
		bool dragging = pointer.PointerEventData.dragging || wasDragging;
		GameObject root = pointer.PointerEventData.pointerCurrentRaycast.gameObject;
		GameObject hoveredSelectable = pointer.HoveredSelectable;
		GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(root);
		pointer.SetHoveredSelectable(eventHandler);
		if (eventHandler != null && eventHandler != hoveredSelectable)
		{
			PointableCanvasModule.WhenSelectableHovered?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, pointer.HoveredSelectable, dragging));
		}
		else if (hoveredSelectable != null && eventHandler == null)
		{
			PointableCanvasModule.WhenSelectableUnhovered?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, pointer.HoveredSelectable, dragging));
		}
	}

	private void HandleSelectablePress(PointerImpl pointer, bool pressed, bool released, bool wasDragging)
	{
		bool dragging = pointer.PointerEventData.dragging || wasDragging;
		if (pressed)
		{
			PointableCanvasModule.WhenSelected?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, pointer.HoveredSelectable, dragging));
		}
		else if (released && !pointer.MarkedForDeletion)
		{
			GameObject hovered = ((pointer.HoveredSelectable != null && pointer.HoveredSelectable == pointer.PointerEventData.selectedObject) ? pointer.HoveredSelectable : null);
			PointableCanvasModule.WhenUnselected?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, hovered, dragging));
		}
	}

	protected void UpdatePointerEventData(PointerEventData pointerEvent, bool pressed, bool released)
	{
		GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
		if (pressed)
		{
			pointerEvent.eligibleForClick = true;
			pointerEvent.delta = Vector2.zero;
			pointerEvent.dragging = false;
			pointerEvent.useDragThreshold = true;
			pointerEvent.pressPosition = pointerEvent.position;
			pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
			DeselectIfSelectionChanged(gameObject, pointerEvent);
			if (pointerEvent.pointerEnter != gameObject)
			{
				HandlePointerExitAndEnter(pointerEvent, gameObject);
				pointerEvent.pointerEnter = gameObject;
			}
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == pointerEvent.lastPress)
			{
				if (unscaledTime - pointerEvent.clickTime < 0.3f)
				{
					int clickCount = pointerEvent.clickCount + 1;
					pointerEvent.clickCount = clickCount;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.clickTime = unscaledTime;
			}
			else
			{
				pointerEvent.clickCount = 1;
			}
			pointerEvent.pointerPress = gameObject2;
			pointerEvent.rawPointerPress = gameObject;
			pointerEvent.clickTime = unscaledTime;
			pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (pointerEvent.pointerDrag != null)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (released)
		{
			ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
			}
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.dropHandler);
			}
			pointerEvent.eligibleForClick = false;
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.dragging = false;
			pointerEvent.pointerDrag = null;
			ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
			pointerEvent.pointerEnter = null;
		}
	}

	protected override void ProcessDrag(PointerEventData pointerEvent)
	{
		if (!pointerEvent.IsPointerMoving() || pointerEvent.pointerDrag == null)
		{
			return;
		}
		if (!pointerEvent.dragging && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, base.eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
		{
			if (_useInitialPressPositionForDrag)
			{
				pointerEvent.position = pointerEvent.pressPosition;
			}
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
			pointerEvent.dragging = true;
		}
		if (pointerEvent.dragging)
		{
			if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
			{
				ClearPointerSelection(pointerEvent);
			}
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
		}
	}

	private void ClearPointerSelection(PointerEventData pointerEvent)
	{
		ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
		pointerEvent.eligibleForClick = false;
		pointerEvent.pointerPress = null;
		pointerEvent.rawPointerPress = null;
	}

	protected static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
	{
		if (!useDragThreshold)
		{
			return true;
		}
		return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
	}
}
