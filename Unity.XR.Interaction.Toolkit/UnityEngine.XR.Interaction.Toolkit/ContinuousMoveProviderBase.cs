using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace UnityEngine.XR.Interaction.Toolkit;

[Obsolete("The ContinuousMoveProviderBase has been deprecated in XRI 3.0.0 and will be removed in a future version of XRI. Please use ContinuousMoveProvider instead.", false)]
public abstract class ContinuousMoveProviderBase : LocomotionProvider
{
	[Obsolete("GravityApplicationMode has been deprecated in XRI 3.0.0 and will be removed in a future version of XRI. Please use LocomotionMediator with a GravityProvider.", false)]
	public enum GravityApplicationMode
	{
		AttemptingMove,
		Immediately
	}

	[SerializeField]
	[Tooltip("The speed, in units per second, to move forward.")]
	private float m_MoveSpeed = 1f;

	[SerializeField]
	[Tooltip("Controls whether to enable strafing (sideways movement).")]
	private bool m_EnableStrafe = true;

	[SerializeField]
	[Tooltip("Controls whether to enable flying (unconstrained movement). This overrides the use of gravity.")]
	private bool m_EnableFly;

	[SerializeField]
	[Tooltip("Controls whether gravity affects this provider when a Character Controller is used and flying is disabled.")]
	private bool m_UseGravity = true;

	[SerializeField]
	[Tooltip("Controls when gravity begins to take effect.")]
	private GravityApplicationMode m_GravityApplicationMode;

	[SerializeField]
	[Tooltip("The source Transform to define the forward direction.")]
	private Transform m_ForwardSource;

	private CharacterController m_CharacterController;

	private bool m_AttemptedGetCharacterController;

	private bool m_IsMovingXROrigin;

	private Vector3 m_VerticalVelocity;

	public float moveSpeed
	{
		get
		{
			return m_MoveSpeed;
		}
		set
		{
			m_MoveSpeed = value;
		}
	}

	public bool enableStrafe
	{
		get
		{
			return m_EnableStrafe;
		}
		set
		{
			m_EnableStrafe = value;
		}
	}

	public bool enableFly
	{
		get
		{
			return m_EnableFly;
		}
		set
		{
			m_EnableFly = value;
		}
	}

	public bool useGravity
	{
		get
		{
			return m_UseGravity;
		}
		set
		{
			m_UseGravity = value;
		}
	}

	public GravityApplicationMode gravityApplicationMode
	{
		get
		{
			return m_GravityApplicationMode;
		}
		set
		{
			m_GravityApplicationMode = value;
		}
	}

	public Transform forwardSource
	{
		get
		{
			return m_ForwardSource;
		}
		set
		{
			m_ForwardSource = value;
		}
	}

	protected void Update()
	{
		m_IsMovingXROrigin = false;
		if (base.system.xrOrigin?.Origin == null)
		{
			return;
		}
		Vector2 vector = ReadInput();
		Vector3 translationInWorldSpace = ComputeDesiredMove(vector);
		switch (m_GravityApplicationMode)
		{
		case GravityApplicationMode.Immediately:
			MoveRig(translationInWorldSpace);
			break;
		case GravityApplicationMode.AttemptingMove:
			if (vector != Vector2.zero || m_VerticalVelocity != Vector3.zero)
			{
				MoveRig(translationInWorldSpace);
			}
			break;
		}
		switch (base.locomotionPhase)
		{
		case LocomotionPhase.Idle:
		case LocomotionPhase.Started:
			if (m_IsMovingXROrigin)
			{
				base.locomotionPhase = LocomotionPhase.Moving;
			}
			break;
		case LocomotionPhase.Moving:
			if (!m_IsMovingXROrigin)
			{
				base.locomotionPhase = LocomotionPhase.Done;
			}
			break;
		case LocomotionPhase.Done:
			base.locomotionPhase = (m_IsMovingXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle);
			break;
		}
	}

	protected abstract Vector2 ReadInput();

	protected virtual Vector3 ComputeDesiredMove(Vector2 input)
	{
		if (input == Vector2.zero)
		{
			return Vector3.zero;
		}
		XROrigin xrOrigin = base.system.xrOrigin;
		if (xrOrigin == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = Vector3.ClampMagnitude(new Vector3(m_EnableStrafe ? input.x : 0f, 0f, input.y), 1f);
		Transform transform = ((m_ForwardSource == null) ? xrOrigin.Camera.transform : m_ForwardSource);
		Vector3 vector2 = transform.forward;
		Transform transform2 = xrOrigin.Origin.transform;
		float num = m_MoveSpeed * Time.deltaTime * transform2.localScale.x;
		if (m_EnableFly)
		{
			Vector3 right = transform.right;
			return (vector.x * right + vector.z * vector2) * num;
		}
		Vector3 up = transform2.up;
		if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(vector2, up)), 1f))
		{
			vector2 = -transform.up;
		}
		Vector3 toDirection = Vector3.ProjectOnPlane(vector2, up);
		Vector3 direction = Quaternion.FromToRotation(transform2.forward, toDirection) * vector * num;
		return transform2.TransformDirection(direction);
	}

	protected virtual void MoveRig(Vector3 translationInWorldSpace)
	{
		GameObject gameObject = base.system.xrOrigin?.Origin;
		if (gameObject == null)
		{
			return;
		}
		FindCharacterController();
		Vector3 vector = translationInWorldSpace;
		if (m_CharacterController != null && m_CharacterController.enabled)
		{
			if (m_CharacterController.isGrounded || !m_UseGravity || m_EnableFly)
			{
				m_VerticalVelocity = Vector3.zero;
			}
			else
			{
				m_VerticalVelocity += Physics.gravity * Time.deltaTime;
			}
			vector += m_VerticalVelocity * Time.deltaTime;
			if (CanBeginLocomotion() && BeginLocomotion())
			{
				m_IsMovingXROrigin = true;
				m_CharacterController.Move(vector);
				EndLocomotion();
			}
		}
		else if (CanBeginLocomotion() && BeginLocomotion())
		{
			m_IsMovingXROrigin = true;
			gameObject.transform.position += vector;
			EndLocomotion();
		}
	}

	private void FindCharacterController()
	{
		GameObject gameObject = base.system.xrOrigin?.Origin;
		if (!(gameObject == null) && m_CharacterController == null && !m_AttemptedGetCharacterController)
		{
			if (!gameObject.TryGetComponent<CharacterController>(out m_CharacterController) && gameObject != base.system.xrOrigin.gameObject)
			{
				base.system.xrOrigin.TryGetComponent<CharacterController>(out m_CharacterController);
			}
			m_AttemptedGetCharacterController = true;
		}
	}
}
