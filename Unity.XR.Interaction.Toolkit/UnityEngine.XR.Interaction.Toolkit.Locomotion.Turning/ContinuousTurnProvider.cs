using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

[AddComponentMenu("XR/Locomotion/Continuous Turn Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.ContinuousTurnProvider.html")]
public class ContinuousTurnProvider : LocomotionProvider
{
	[SerializeField]
	[Tooltip("The number of degrees/second clockwise to rotate when turning clockwise.")]
	private float m_TurnSpeed = 60f;

	[SerializeField]
	[Tooltip("Controls whether to enable left & right continuous turns.")]
	private bool m_EnableTurnLeftRight = true;

	[SerializeField]
	[Tooltip("Controls whether to enable 180° snap turns on the South direction.")]
	private bool m_EnableTurnAround;

	[SerializeField]
	[Tooltip("Reads input data from the left hand controller. Input Action must be a Value action type (Vector 2).")]
	private XRInputValueReader<Vector2> m_LeftHandTurnInput = new XRInputValueReader<Vector2>("Left Hand Turn");

	[SerializeField]
	[Tooltip("Reads input data from the right hand controller. Input Action must be a Value action type (Vector 2).")]
	private XRInputValueReader<Vector2> m_RightHandTurnInput = new XRInputValueReader<Vector2>("Right Hand Turn");

	private bool m_IsTurningXROrigin;

	private bool m_TurnAroundActivated;

	public float turnSpeed
	{
		get
		{
			return m_TurnSpeed;
		}
		set
		{
			m_TurnSpeed = value;
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

	public XRBodyYawRotation transformation { get; set; } = new XRBodyYawRotation();


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
		m_IsTurningXROrigin = false;
		Vector2 vector = ReadInput();
		float turnAmount = GetTurnAmount(vector);
		TurnRig(turnAmount);
		if (!m_IsTurningXROrigin)
		{
			TryEndLocomotion();
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
		switch (CardinalUtility.GetNearestCardinal(input))
		{
		case Cardinal.South:
			if (m_EnableTurnAround && !m_TurnAroundActivated)
			{
				return 180f;
			}
			break;
		case Cardinal.East:
		case Cardinal.West:
			if (m_EnableTurnLeftRight)
			{
				return input.magnitude * (Mathf.Sign(input.x) * m_TurnSpeed * Time.deltaTime);
			}
			break;
		}
		return 0f;
	}

	protected void TurnRig(float turnAmount)
	{
		if (!Mathf.Approximately(turnAmount, 0f))
		{
			if (Mathf.Approximately(turnAmount, 180f))
			{
				m_TurnAroundActivated = true;
			}
			TryStartLocomotionImmediately();
			if (base.locomotionState == LocomotionState.Moving)
			{
				m_IsTurningXROrigin = true;
				transformation.angleDelta = turnAmount;
				TryQueueTransformation(transformation);
			}
		}
	}
}
