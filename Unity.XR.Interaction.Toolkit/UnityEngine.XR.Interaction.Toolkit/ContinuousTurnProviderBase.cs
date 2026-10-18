using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace UnityEngine.XR.Interaction.Toolkit;

[Obsolete("The ContinuousMoveProviderBase has been deprecated in XRI 3.0.0 and will be removed in a future version of XRI. Please use ContinuousTurnProvider instead.", false)]
public abstract class ContinuousTurnProviderBase : LocomotionProvider
{
	[SerializeField]
	[Tooltip("The number of degrees/second clockwise to rotate when turning clockwise.")]
	private float m_TurnSpeed = 60f;

	private bool m_IsTurningXROrigin;

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

	protected void Update()
	{
		m_IsTurningXROrigin = false;
		Vector2 input = ReadInput();
		float turnAmount = GetTurnAmount(input);
		TurnRig(turnAmount);
		switch (base.locomotionPhase)
		{
		case LocomotionPhase.Idle:
		case LocomotionPhase.Started:
			if (m_IsTurningXROrigin)
			{
				base.locomotionPhase = LocomotionPhase.Moving;
			}
			break;
		case LocomotionPhase.Moving:
			if (!m_IsTurningXROrigin)
			{
				base.locomotionPhase = LocomotionPhase.Done;
			}
			break;
		case LocomotionPhase.Done:
			base.locomotionPhase = (m_IsTurningXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle);
			break;
		}
	}

	protected abstract Vector2 ReadInput();

	protected virtual float GetTurnAmount(Vector2 input)
	{
		if (input == Vector2.zero)
		{
			return 0f;
		}
		Cardinal nearestCardinal = CardinalUtility.GetNearestCardinal(input);
		if ((uint)nearestCardinal > 1u && (uint)(nearestCardinal - 2) <= 1u)
		{
			return input.magnitude * (Mathf.Sign(input.x) * m_TurnSpeed * Time.deltaTime);
		}
		return 0f;
	}

	protected void TurnRig(float turnAmount)
	{
		if (!Mathf.Approximately(turnAmount, 0f) && CanBeginLocomotion() && BeginLocomotion())
		{
			XROrigin xrOrigin = base.system.xrOrigin;
			if (xrOrigin != null)
			{
				m_IsTurningXROrigin = true;
				xrOrigin.RotateAroundCameraUsingOriginUp(turnAmount);
			}
			EndLocomotion();
		}
	}
}
