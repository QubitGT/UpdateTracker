using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal struct PointerModel
{
	internal struct InternalData
	{
		public List<GameObject> hoverTargets { get; set; }

		public GameObject pointerTarget { get; set; }

		public void Reset()
		{
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

	private int m_DisplayIndex;

	private Vector2 m_Position;

	private Vector2 m_ScrollDelta;

	private MouseButtonModel m_LeftButton;

	private MouseButtonModel m_RightButton;

	private MouseButtonModel m_MiddleButton;

	private InternalData m_InternalData;

	public int pointerId { get; }

	public bool changedThisFrame { get; private set; }

	public int displayIndex
	{
		get
		{
			return m_DisplayIndex;
		}
		set
		{
			if (m_DisplayIndex != value)
			{
				m_DisplayIndex = value;
				changedThisFrame = true;
			}
		}
	}

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

	public MouseButtonModel leftButton
	{
		get
		{
			return m_LeftButton;
		}
		set
		{
			changedThisFrame |= value.lastFrameDelta != ButtonDeltaState.NoChange;
			m_LeftButton = value;
		}
	}

	public bool leftButtonPressed
	{
		set
		{
			changedThisFrame |= m_LeftButton.isDown != value;
			m_LeftButton.isDown = value;
		}
	}

	public MouseButtonModel rightButton
	{
		get
		{
			return m_RightButton;
		}
		set
		{
			changedThisFrame |= value.lastFrameDelta != ButtonDeltaState.NoChange;
			m_RightButton = value;
		}
	}

	public bool rightButtonPressed
	{
		set
		{
			changedThisFrame |= m_RightButton.isDown != value;
			m_RightButton.isDown = value;
		}
	}

	public MouseButtonModel middleButton
	{
		get
		{
			return m_MiddleButton;
		}
		set
		{
			changedThisFrame |= value.lastFrameDelta != ButtonDeltaState.NoChange;
			m_MiddleButton = value;
		}
	}

	public bool middleButtonPressed
	{
		set
		{
			changedThisFrame |= m_MiddleButton.isDown != value;
			m_MiddleButton.isDown = value;
		}
	}

	public PointerModel(int pointerId)
	{
		this.pointerId = pointerId;
		changedThisFrame = false;
		m_DisplayIndex = 0;
		m_Position = Vector2.zero;
		deltaPosition = Vector2.zero;
		m_ScrollDelta = Vector2.zero;
		m_LeftButton = default(MouseButtonModel);
		m_RightButton = default(MouseButtonModel);
		m_MiddleButton = default(MouseButtonModel);
		m_LeftButton.Reset();
		m_RightButton.Reset();
		m_MiddleButton.Reset();
		m_InternalData = default(InternalData);
		m_InternalData.Reset();
	}

	public void OnFrameFinished()
	{
		changedThisFrame = false;
		deltaPosition = Vector2.zero;
		m_ScrollDelta = Vector2.zero;
		m_LeftButton.OnFrameFinished();
		m_RightButton.OnFrameFinished();
		m_MiddleButton.OnFrameFinished();
	}

	public void CopyTo(PointerEventData eventData)
	{
		eventData.pointerId = pointerId;
		eventData.displayIndex = m_DisplayIndex;
		eventData.position = position;
		eventData.delta = deltaPosition;
		eventData.scrollDelta = scrollDelta;
		eventData.pointerEnter = m_InternalData.pointerTarget;
		eventData.hovered.Clear();
		eventData.hovered.AddRange(m_InternalData.hoverTargets);
	}

	public void CopyFrom(PointerEventData eventData)
	{
		List<GameObject> hoverTargets = m_InternalData.hoverTargets;
		m_InternalData.hoverTargets.Clear();
		m_InternalData.hoverTargets.AddRange(eventData.hovered);
		m_InternalData.hoverTargets = hoverTargets;
		m_InternalData.pointerTarget = eventData.pointerEnter;
		m_DisplayIndex = eventData.displayIndex;
	}
}
