using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

public struct TrackedDeviceModel
{
	internal struct ImplementationData
	{
		public List<GameObject> hoverTargets { get; set; }

		public GameObject pointerTarget { get; set; }

		public bool isDragging { get; set; }

		public float pressedTime { get; set; }

		public Vector2 position { get; set; }

		public Vector2 pressedPosition { get; set; }

		public Vector3 pressedWorldPosition { get; set; }

		public RaycastResult pressedRaycast { get; set; }

		public GameObject pressedGameObject { get; set; }

		public GameObject pressedGameObjectRaw { get; set; }

		public GameObject draggedGameObject { get; set; }

		public void Reset()
		{
			isDragging = false;
			pressedTime = 0f;
			position = Vector2.zero;
			pressedPosition = Vector2.zero;
			pressedWorldPosition = Vector3.zero;
			pressedRaycast = default(RaycastResult);
			pressedGameObject = null;
			pressedGameObjectRaw = null;
			draggedGameObject = null;
			pointerTarget = null;
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

	private ImplementationData m_ImplementationData;

	private bool m_SelectDown;

	private bool m_ClickOnDown;

	private Vector3 m_Position;

	private Func<Vector3> m_PositionProvider;

	private Quaternion m_Orientation;

	private List<Vector3> m_RaycastPoints;

	private LayerMask m_RaycastLayerMask;

	private Vector2 m_ScrollDelta;

	private float m_PokeDepth;

	private UIInteractionType m_InteractionType;

	internal ImplementationData implementationData => m_ImplementationData;

	public int pointerId { get; }

	public bool select
	{
		get
		{
			return m_SelectDown;
		}
		set
		{
			if (m_SelectDown != value)
			{
				m_SelectDown = value;
				selectDelta |= (ButtonDeltaState)(value ? 1 : 2);
				changedThisFrame = true;
			}
		}
	}

	public bool clickOnDown
	{
		get
		{
			return m_ClickOnDown;
		}
		set
		{
			m_ClickOnDown = value;
		}
	}

	public ButtonDeltaState selectDelta { get; private set; }

	public bool changedThisFrame { get; private set; }

	public Vector3 position
	{
		get
		{
			return m_PositionProvider?.Invoke() ?? m_Position;
		}
		set
		{
			if (m_Position != value)
			{
				m_Position = value;
				changedThisFrame = true;
			}
		}
	}

	public Func<Vector3> positionProvider
	{
		get
		{
			return m_PositionProvider;
		}
		set
		{
			if (m_PositionProvider != value)
			{
				m_PositionProvider = value;
				changedThisFrame = true;
			}
		}
	}

	public Quaternion orientation
	{
		get
		{
			return m_Orientation;
		}
		set
		{
			if (m_Orientation != value)
			{
				m_Orientation = value;
				changedThisFrame = true;
			}
		}
	}

	public List<Vector3> raycastPoints
	{
		get
		{
			return m_RaycastPoints;
		}
		set
		{
			changedThisFrame |= m_RaycastPoints.Count != value.Count;
			m_RaycastPoints = value;
		}
	}

	public RaycastResult currentRaycast { get; private set; }

	public int currentRaycastEndpointIndex { get; private set; }

	public LayerMask raycastLayerMask
	{
		get
		{
			return m_RaycastLayerMask;
		}
		set
		{
			if ((int)m_RaycastLayerMask != (int)value)
			{
				changedThisFrame = true;
				m_RaycastLayerMask = value;
			}
		}
	}

	public Vector2 scrollDelta
	{
		get
		{
			return m_ScrollDelta;
		}
		set
		{
			if (m_ScrollDelta != value)
			{
				m_ScrollDelta = value;
				changedThisFrame = true;
			}
		}
	}

	public float pokeDepth
	{
		get
		{
			return m_PokeDepth;
		}
		set
		{
			if (m_PokeDepth != value)
			{
				m_PokeDepth = value;
				changedThisFrame = true;
			}
		}
	}

	public UIInteractionType interactionType
	{
		get
		{
			return m_InteractionType;
		}
		set
		{
			if (m_InteractionType != value)
			{
				m_InteractionType = value;
				changedThisFrame = true;
			}
		}
	}

	internal IUIInteractor interactor { get; set; }

	public GameObject selectableObject { get; set; }

	public bool isScrollable { get; set; }

	public static TrackedDeviceModel invalid { get; } = new TrackedDeviceModel(-1);


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

	internal void UpdatePokeSelectState()
	{
		if (m_InteractionType == UIInteractionType.Poke)
		{
			select = TrackedDeviceGraphicRaycaster.IsPokeSelectingWithUI(interactor);
		}
	}

	public TrackedDeviceModel(int pointerId)
	{
		this = default(TrackedDeviceModel);
		this.pointerId = pointerId;
		m_RaycastPoints = new List<Vector3>();
		m_ImplementationData = default(ImplementationData);
		Reset();
	}

	public void Reset(bool resetImplementation = true)
	{
		m_Orientation = Quaternion.identity;
		m_Position = Vector3.zero;
		m_PositionProvider = null;
		changedThisFrame = false;
		m_SelectDown = false;
		selectDelta = ButtonDeltaState.NoChange;
		m_RaycastPoints?.Clear();
		currentRaycastEndpointIndex = 0;
		m_RaycastLayerMask = -5;
		m_ScrollDelta = Vector2.zero;
		if (resetImplementation)
		{
			m_ImplementationData.Reset();
		}
	}

	public void OnFrameFinished()
	{
		selectDelta = ButtonDeltaState.NoChange;
		m_ScrollDelta = Vector2.zero;
		changedThisFrame = false;
	}

	public void CopyTo(TrackedDeviceEventData eventData)
	{
		eventData.rayPoints = m_RaycastPoints;
		eventData.layerMask = m_RaycastLayerMask;
		eventData.pointerId = pointerId;
		eventData.scrollDelta = m_ScrollDelta;
		eventData.pointerEnter = m_ImplementationData.pointerTarget;
		eventData.dragging = m_ImplementationData.isDragging;
		eventData.clickTime = m_ImplementationData.pressedTime;
		eventData.position = m_ImplementationData.position;
		eventData.pressPosition = m_ImplementationData.pressedPosition;
		eventData.pressWorldPosition = m_ImplementationData.pressedWorldPosition;
		eventData.pointerPressRaycast = m_ImplementationData.pressedRaycast;
		eventData.pointerPress = m_ImplementationData.pressedGameObject;
		eventData.rawPointerPress = m_ImplementationData.pressedGameObjectRaw;
		eventData.pointerDrag = m_ImplementationData.draggedGameObject;
		eventData.hovered.Clear();
		eventData.hovered.AddRange(m_ImplementationData.hoverTargets);
	}

	public void CopyFrom(TrackedDeviceEventData eventData)
	{
		m_ImplementationData.pointerTarget = eventData.pointerEnter;
		m_ImplementationData.isDragging = eventData.dragging;
		m_ImplementationData.pressedTime = eventData.clickTime;
		m_ImplementationData.position = eventData.position;
		m_ImplementationData.pressedPosition = eventData.pressPosition;
		m_ImplementationData.pressedWorldPosition = eventData.pressWorldPosition;
		m_ImplementationData.pressedRaycast = eventData.pointerPressRaycast;
		m_ImplementationData.pressedGameObject = eventData.pointerPress;
		m_ImplementationData.pressedGameObjectRaw = eventData.rawPointerPress;
		m_ImplementationData.draggedGameObject = eventData.pointerDrag;
		m_ImplementationData.hoverTargets.Clear();
		m_ImplementationData.hoverTargets.AddRange(eventData.hovered);
		currentRaycast = eventData.pointerCurrentRaycast;
		currentRaycastEndpointIndex = eventData.rayHitIndex;
	}
}
