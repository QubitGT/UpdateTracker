using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal struct TouchModel
{
	internal struct ImplementationData
	{
		public List<GameObject> hoverTargets { get; set; }

		public GameObject pointerTarget { get; set; }

		public bool isDragging { get; set; }

		public float pressedTime { get; set; }

		public Vector2 pressedPosition { get; set; }

		public RaycastResult pressedRaycast { get; set; }

		public GameObject pressedGameObject { get; set; }

		public GameObject pressedGameObjectRaw { get; set; }

		public GameObject draggedGameObject { get; set; }

		public void Reset()
		{
			isDragging = false;
			pressedTime = 0f;
			pressedPosition = Vector2.zero;
			pressedRaycast = default(RaycastResult);
			GameObject gameObject2 = (draggedGameObject = null);
			GameObject gameObject4 = (pressedGameObjectRaw = gameObject2);
			pressedGameObject = gameObject4;
			if (hoverTargets == null)
			{
				hoverTargets = new List<GameObject>();
			}
			else
			{
				hoverTargets.Clear();
			}
		}
	}

	private TouchPhase m_SelectPhase;

	private Vector2 m_Position;

	private ImplementationData m_ImplementationData;

	public int pointerId { get; }

	public TouchPhase selectPhase
	{
		get
		{
			return m_SelectPhase;
		}
		set
		{
			if (m_SelectPhase != value)
			{
				if (value == TouchPhase.Began)
				{
					selectDelta |= ButtonDeltaState.Pressed;
				}
				if (value == TouchPhase.Ended || value == TouchPhase.Canceled)
				{
					selectDelta |= ButtonDeltaState.Released;
				}
				m_SelectPhase = value;
				changedThisFrame = true;
			}
		}
	}

	public ButtonDeltaState selectDelta { get; private set; }

	public bool changedThisFrame { get; private set; }

	public Vector2 position
	{
		get
		{
			return m_Position;
		}
		set
		{
			if (m_Position != value)
			{
				deltaPosition = value - m_Position;
				m_Position = value;
				changedThisFrame = true;
			}
		}
	}

	public Vector2 deltaPosition { get; private set; }

	public TouchModel(int pointerId)
	{
		this.pointerId = pointerId;
		Vector2 vector = (deltaPosition = Vector2.zero);
		m_Position = vector;
		m_SelectPhase = TouchPhase.Canceled;
		changedThisFrame = false;
		selectDelta = ButtonDeltaState.NoChange;
		m_ImplementationData = default(ImplementationData);
		m_ImplementationData.Reset();
	}

	public void Reset()
	{
		Vector2 vector = (deltaPosition = Vector2.zero);
		m_Position = vector;
		changedThisFrame = false;
		selectDelta = ButtonDeltaState.NoChange;
		m_ImplementationData.Reset();
	}

	public void OnFrameFinished()
	{
		deltaPosition = Vector2.zero;
		selectDelta = ButtonDeltaState.NoChange;
		changedThisFrame = false;
	}

	public void CopyTo(PointerEventData eventData)
	{
		eventData.pointerId = pointerId;
		eventData.position = position;
		eventData.delta = (((selectDelta & ButtonDeltaState.Pressed) != 0) ? Vector2.zero : deltaPosition);
		eventData.pointerEnter = m_ImplementationData.pointerTarget;
		eventData.dragging = m_ImplementationData.isDragging;
		eventData.clickTime = m_ImplementationData.pressedTime;
		eventData.pressPosition = m_ImplementationData.pressedPosition;
		eventData.pointerPressRaycast = m_ImplementationData.pressedRaycast;
		eventData.pointerPress = m_ImplementationData.pressedGameObject;
		eventData.rawPointerPress = m_ImplementationData.pressedGameObjectRaw;
		eventData.pointerDrag = m_ImplementationData.draggedGameObject;
		eventData.hovered.Clear();
		eventData.hovered.AddRange(m_ImplementationData.hoverTargets);
	}

	public void CopyFrom(PointerEventData eventData)
	{
		m_ImplementationData.pointerTarget = eventData.pointerEnter;
		m_ImplementationData.isDragging = eventData.dragging;
		m_ImplementationData.pressedTime = eventData.clickTime;
		m_ImplementationData.pressedPosition = eventData.pressPosition;
		m_ImplementationData.pressedRaycast = eventData.pointerPressRaycast;
		m_ImplementationData.pressedGameObject = eventData.pointerPress;
		m_ImplementationData.pressedGameObjectRaw = eventData.rawPointerPress;
		m_ImplementationData.draggedGameObject = eventData.pointerDrag;
		m_ImplementationData.hoverTargets.Clear();
		m_ImplementationData.hoverTargets.AddRange(eventData.hovered);
	}
}
