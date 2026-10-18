using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

[AddComponentMenu("XR/Locomotion/Snap Turn Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.SnapTurnProvider.html")]
public class SnapTurnProvider : LocomotionProvider
{
	[SerializeField]
	[Tooltip("The number of degrees clockwise to rotate when snap turning clockwise.")]
	private float m_TurnAmount = 45f;

	[SerializeField]
	[Tooltip("The amount of time that the system will wait before starting another snap turn.")]
	private float m_DebounceTime = 0.5f;

	[SerializeField]
	[Tooltip("Controls whether to enable left & right snap turns.")]
	private bool m_EnableTurnLeftRight = true;

	[SerializeField]
	[Tooltip("Controls whether to enable 180° snap turns.")]
	private bool m_EnableTurnAround = true;

	[SerializeField]
	[Tooltip("The time (in seconds) to delay the first turn after receiving initial input for the turn.")]
	private float m_DelayTime;

	[SerializeField]
	[Tooltip("Reads input data from the left hand controller. Input Action must be a Value action type (Vector 2).")]
	private XRInputValueReader<Vector2> m_LeftHandTurnInput = new XRInputValueReader<Vector2>("Left Hand Snap Turn");

	[SerializeField]
	[Tooltip("Reads input data from the right hand controller. Input Action must be a Value action type (Vector 2).")]
	private XRInputValueReader<Vector2> m_RightHandTurnInput = new XRInputValueReader<Vector2>("Right Hand Snap Turn");

	private float m_CurrentTurnAmount;

	private float m_TimeStarted;

	private float m_DelayStartTime;

	private bool m_TurnAroundActivated;

	public float turnAmount
	{
		get
		{
			return m_TurnAmount;
		}
		set
		{
			m_TurnAmount = value;
		}
	}

	public float debounceTime
	{
		get
		{
			return m_DebounceTime;
		}
		set
		{
			m_DebounceTime = value;
		}
	}

	public bool enableTurnLeftRight
	{
		get
		{
			return m_EnableTurnLeftRight;
		}
		set
		{
			m_EnableTurnLeftRight = value;
		}
	}

	public bool enableTurnAround
	{
		get
		{
			return m_EnableTurnAround;
		}
		set
		{
			m_EnableTurnAround = value;
		}
	}

	public float delayTime
	{
		get
		{
			return m_DelayTime;
		}
		set
		{
			m_DelayTime = value;
		}
	}

	public override bool canStartMoving
	{
		get
		{
			if (!(m_DelayTime <= 0f))
			{
				return Time.time - m_DelayStartTime >= m_DelayTime;
			}
			return true;
		}
	}

	public XRBodyYawRotation transformation { get; set; } = new XRBodyYawRotation();


	public XRInputValueReader<Vector2> leftHandTurnInput
	{
		get
		{
			return m_LeftHandTurnInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_LeftHandTurnInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> rightHandTurnInput
	{
		get
		{
			return m_RightHandTurnInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_RightHandTurnInput, value, this);
		}
	}

	protected void OnEnable()
	{
		m_LeftHandTurnInput.EnableDirectActionIfModeUsed();
		m_RightHandTurnInput.EnableDirectActionIfModeUsed();
	}

	protected void OnDisable()
	{
		m_LeftHandTurnInput.DisableDirectActionIfModeUsed();
		m_RightHandTurnInput.DisableDirectActionIfModeUsed();
	}

	protected void Update()
	{
		if (m_TimeStarted > 0f && m_TimeStarted + m_DebounceTime < Time.time)
		{
			m_TimeStarted = 0f;
			return;
		}
		Vector2 vector = ReadInput();
		float num = GetTurnAmount(vector);
		if (Mathf.Abs(num) > 0f)
		{
			StartTurn(num);
		}
		else if (Mathf.Approximately(m_CurrentTurnAmount, 0f) && base.locomotionState == LocomotionState.Moving)
		{
			TryEndLocomotion();
		}
		if (base.locomotionState == LocomotionState.Moving && math.abs(m_CurrentTurnAmount) > 0f)
		{
			m_TimeStarted = Time.time;
			transformation.angleDelta = m_CurrentTurnAmount;
			TryQueueTransformation(transformation);
			m_CurrentTurnAmount = 0f;
			if (Mathf.Approximately(num, 0f))
			{
				TryEndLocomotion();
			}
		}
		if (vector == Vector2.zero)
		{
			m_TurnAroundActivated = false;
		}
	}

	private Vector2 ReadInput()
	{
		Vector2 vector = m_LeftHandTurnInput.ReadValue();
		Vector2 vector2 = m_RightHandTurnInput.ReadValue();
		return vector + vector2;
	}

	protected virtual float GetTurnAmount(Vector2 input)
	{
		if (input == Vector2.zero)
		{
			return 0f;
		}
		switch (CardinalUtility.GetNearestCardinal(input))
		{
		case Cardinal.South:
			if (m_EnableTurnAround && !m_TurnAroundActivated)
			{
				return 180f;
			}
			break;
		case Cardinal.East:
			if (m_EnableTurnLeftRight)
			{
				return m_TurnAmount;
			}
			break;
		case Cardinal.West:
			if (m_EnableTurnLeftRight)
			{
				return 0f - m_TurnAmount;
			}
			break;
		}
		return 0f;
	}

	protected void StartTurn(float amount)
	{
		if (m_TimeStarted > 0f)
		{
			return;
		}
		if (Mathf.Approximately(amount, 180f))
		{
			m_TurnAroundActivated = true;
		}
		if (base.locomotionState == LocomotionState.Idle)
		{
			if (m_DelayTime > 0f)
			{
				if (TryPrepareLocomotion())
				{
					m_DelayStartTime = Time.time;
				}
			}
			else
			{
				TryStartLocomotionImmediately();
			}
		}
		if (math.abs(amount) > 0f)
		{
			m_CurrentTurnAmount = amount;
		}
	}
}
