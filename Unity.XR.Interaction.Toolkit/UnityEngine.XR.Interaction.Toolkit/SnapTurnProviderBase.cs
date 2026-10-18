using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace UnityEngine.XR.Interaction.Toolkit;

[Obsolete("SnapTurnProviderBase has been deprecated in XRI 3.0.0 and will be removed in a future version of XRI. Please use SnapTurnProvider instead.", false)]
public abstract class SnapTurnProviderBase : LocomotionProvider
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

	protected override void Awake()
	{
		base.Awake();
		if (base.system != null && m_DelayTime > 0f && m_DelayTime > base.system.timeout)
		{
			Debug.LogWarning($"Delay Time ({m_DelayTime}) is longer than the Locomotion System's Timeout ({base.system.timeout}).", this);
		}
	}

	protected void Update()
	{
		if (m_TimeStarted > 0f && m_TimeStarted + m_DebounceTime < Time.time)
		{
			m_TimeStarted = 0f;
			return;
		}
		if (base.locomotionPhase == LocomotionPhase.Done)
		{
			base.locomotionPhase = LocomotionPhase.Idle;
		}
		Vector2 vector = ReadInput();
		float num = GetTurnAmount(vector);
		if (Mathf.Abs(num) > 0f || base.locomotionPhase == LocomotionPhase.Started)
		{
			StartTurn(num);
		}
		else if (Mathf.Approximately(m_CurrentTurnAmount, 0f) && base.locomotionPhase == LocomotionPhase.Moving)
		{
			base.locomotionPhase = LocomotionPhase.Done;
		}
		if (base.locomotionPhase == LocomotionPhase.Moving && Math.Abs(m_CurrentTurnAmount) > 0f && BeginLocomotion())
		{
			XROrigin xrOrigin = base.system.xrOrigin;
			if (xrOrigin != null)
			{
				xrOrigin.RotateAroundCameraUsingOriginUp(m_CurrentTurnAmount);
			}
			else
			{
				base.locomotionPhase = LocomotionPhase.Done;
			}
			m_CurrentTurnAmount = 0f;
			EndLocomotion();
			if (Mathf.Approximately(num, 0f))
			{
				base.locomotionPhase = LocomotionPhase.Done;
			}
		}
		if (vector == Vector2.zero)
		{
			m_TurnAroundActivated = false;
		}
	}

	protected abstract Vector2 ReadInput();

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
		if (!(m_TimeStarted > 0f) && CanBeginLocomotion())
		{
			if (Mathf.Approximately(amount, 180f))
			{
				m_TurnAroundActivated = true;
			}
			if (base.locomotionPhase == LocomotionPhase.Idle)
			{
				base.locomotionPhase = LocomotionPhase.Started;
				m_DelayStartTime = Time.time;
			}
			if (Math.Abs(amount) > 0f)
			{
				m_CurrentTurnAmount = amount;
			}
			if (!(m_DelayTime > 0f) || !(Time.time - m_DelayStartTime < m_DelayTime))
			{
				base.locomotionPhase = LocomotionPhase.Moving;
				m_TimeStarted = Time.time;
			}
		}
	}
}
