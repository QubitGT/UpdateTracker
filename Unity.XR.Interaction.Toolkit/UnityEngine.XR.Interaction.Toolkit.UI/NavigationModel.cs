using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal struct NavigationModel
{
	public struct ImplementationData
	{
		public int consecutiveMoveCount { get; set; }

		public MoveDirection lastMoveDirection { get; set; }

		public float lastMoveTime { get; set; }

		public void Reset()
		{
			consecutiveMoveCount = 0;
			lastMoveTime = 0f;
			lastMoveDirection = MoveDirection.None;
		}
	}

	private bool m_SubmitButtonDown;

	private bool m_CancelButtonDown;

	public Vector2 move { get; set; }

	public bool submitButtonDown
	{
		get
		{
			return m_SubmitButtonDown;
		}
		set
		{
			if (m_SubmitButtonDown != value)
			{
				submitButtonDelta = (value ? ButtonDeltaState.Pressed : ButtonDeltaState.Released);
				m_SubmitButtonDown = value;
			}
		}
	}

	internal ButtonDeltaState submitButtonDelta { get; private set; }

	public bool cancelButtonDown
	{
		get
		{
			return m_CancelButtonDown;
		}
		set
		{
			if (m_CancelButtonDown != value)
			{
				cancelButtonDelta = (value ? ButtonDeltaState.Pressed : ButtonDeltaState.Released);
				m_CancelButtonDown = value;
			}
		}
	}

	internal ButtonDeltaState cancelButtonDelta { get; private set; }

	internal ImplementationData implementationData { get; set; }

	public void Reset()
	{
		move = Vector2.zero;
		m_SubmitButtonDown = (m_CancelButtonDown = false);
		ButtonDeltaState buttonDeltaState2 = (cancelButtonDelta = ButtonDeltaState.NoChange);
		submitButtonDelta = buttonDeltaState2;
		implementationData.Reset();
	}

	public void OnFrameFinished()
	{
		submitButtonDelta = ButtonDeltaState.NoChange;
		cancelButtonDelta = ButtonDeltaState.NoChange;
	}
}
