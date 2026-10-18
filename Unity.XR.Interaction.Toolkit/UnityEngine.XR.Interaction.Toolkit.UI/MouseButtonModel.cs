using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

public struct MouseButtonModel
{
	internal struct ImplementationData
	{
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
		}
	}

	private bool m_IsDown;

	private ImplementationData m_ImplementationData;

	public bool isDown
	{
		get
		{
			return m_IsDown;
		}
		set
		{
			if (m_IsDown != value)
			{
				m_IsDown = value;
				lastFrameDelta |= (ButtonDeltaState)(value ? 1 : 2);
			}
		}
	}

	internal ButtonDeltaState lastFrameDelta { get; private set; }

	public void Reset()
	{
		lastFrameDelta = ButtonDeltaState.NoChange;
		m_IsDown = false;
		m_ImplementationData.Reset();
	}

	public void OnFrameFinished()
	{
		lastFrameDelta = ButtonDeltaState.NoChange;
	}

	public void CopyTo(PointerEventData eventData)
	{
		eventData.dragging = m_ImplementationData.isDragging;
		eventData.clickTime = m_ImplementationData.pressedTime;
		eventData.pressPosition = m_ImplementationData.pressedPosition;
		eventData.pointerPressRaycast = m_ImplementationData.pressedRaycast;
		eventData.pointerPress = m_ImplementationData.pressedGameObject;
		eventData.rawPointerPress = m_ImplementationData.pressedGameObjectRaw;
		eventData.pointerDrag = m_ImplementationData.draggedGameObject;
	}

	public void CopyFrom(PointerEventData eventData)
	{
		m_ImplementationData.isDragging = eventData.dragging;
		m_ImplementationData.pressedTime = eventData.clickTime;
		m_ImplementationData.pressedPosition = eventData.pressPosition;
		m_ImplementationData.pressedRaycast = eventData.pointerPressRaycast;
		m_ImplementationData.pressedGameObject = eventData.pointerPress;
		m_ImplementationData.pressedGameObjectRaw = eventData.rawPointerPress;
		m_ImplementationData.draggedGameObject = eventData.pointerDrag;
	}
}
