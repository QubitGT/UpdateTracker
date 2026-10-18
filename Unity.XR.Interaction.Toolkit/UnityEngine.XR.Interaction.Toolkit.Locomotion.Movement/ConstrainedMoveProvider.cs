using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public abstract class ConstrainedMoveProvider : LocomotionProvider
{
	[Obsolete("GravityApplicationMode has been deprecated in XRI 3.0.0 and will be removed in a future version.")]
	public enum GravityApplicationMode
	{
		AttemptingMove,
		Immediately
	}

	[SerializeField]
	[Tooltip("Controls whether to enable unconstrained movement along the x-axis.")]
	private bool m_EnableFreeXMovement = true;

	[SerializeField]
	[Tooltip("Controls whether to enable unconstrained movement along the y-axis.")]
	private bool m_EnableFreeYMovement;

	[SerializeField]
	[Tooltip("Controls whether to enable unconstrained movement along the z-axis.")]
	private bool m_EnableFreeZMovement = true;

	private CharacterController m_CharacterController;

	private bool m_AttemptedGetCharacterController;

	private bool m_IsMovingXROrigin;

	private GravityProvider m_GravityProvider;

	[SerializeField]
	[Tooltip("Controls when gravity begins to take effect.")]
	[Obsolete("m_GravityApplicationMode has been deprecated in XRI 3.0.0 and will be removed in a future version.")]
	private GravityApplicationMode m_GravityApplicationMode;

	[SerializeField]
	[Tooltip("Controls whether gravity applies to constrained axes when a Character Controller is used. Ignored when a Gravity Provider component is found in the scene.")]
	[Obsolete("Controlling gravity directly in the move provider has been deprecated in XRI 3.1.0, use Gravity Provider instead.")]
	private bool m_UseGravity = true;

	[Obsolete("Controlling gravity directly in the move provider has been deprecated in XRI 3.1.0, use Gravity Provider instead.")]
	private Vector3 m_GravityDrivenVelocity;

	public bool enableFreeXMovement
	{
		get
		{
			return m_EnableFreeXMovement;
		}
		set
		{
			m_EnableFreeXMovement = value;
		}
	}

	public bool enableFreeYMovement
	{
		get
		{
			return m_EnableFreeYMovement;
		}
		set
		{
			m_EnableFreeYMovement = value;
		}
	}

	public bool enableFreeZMovement
	{
		get
		{
			return m_EnableFreeZMovement;
		}
		set
		{
			m_EnableFreeZMovement = value;
		}
	}

	public XROriginMovement transformation { get; set; } = new XROriginMovement();


	[Obsolete("gravityMode has been deprecated in XRI 3.0.0 and will be removed in a future version.")]
	public GravityApplicationMode gravityMode
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

	[Obsolete("Controlling gravity directly in the move provider has been deprecated in XRI 3.1.0, use Gravity Provider instead.")]
	public bool useGravity
	{
		get
		{
			return m_UseGravity;
		}
		set
		{
			m_UseGravity = value;
			if (Application.isPlaying && m_GravityProvider != null)
			{
				MigrateUseGravityToGravityProvider();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (ComponentLocatorUtility<GravityProvider>.TryFindComponent(out m_GravityProvider) && !m_UseGravity)
		{
			MigrateUseGravityToGravityProvider();
		}
	}

	protected void Update()
	{
		m_IsMovingXROrigin = false;
		if (base.mediator.xrOrigin?.Origin == null)
		{
			return;
		}
		bool attemptingMove;
		Vector3 translationInWorldSpace = ComputeDesiredMove(out attemptingMove);
		switch (m_GravityApplicationMode)
		{
		case GravityApplicationMode.Immediately:
			MoveRig(translationInWorldSpace);
			break;
		case GravityApplicationMode.AttemptingMove:
			if (attemptingMove || m_GravityDrivenVelocity != Vector3.zero)
			{
				MoveRig(translationInWorldSpace);
			}
			break;
		}
		if (!m_IsMovingXROrigin)
		{
			TryEndLocomotion();
		}
	}

	protected abstract Vector3 ComputeDesiredMove(out bool attemptingMove);

	protected virtual void MoveRig(Vector3 translationInWorldSpace)
	{
		FindCharacterController();
		Vector3 motion = translationInWorldSpace;
		if (!m_EnableFreeXMovement)
		{
			motion.x = 0f;
		}
		if (!m_EnableFreeYMovement)
		{
			motion.y = 0f;
		}
		if (!m_EnableFreeZMovement)
		{
			motion.z = 0f;
		}
		if (m_GravityProvider == null && m_CharacterController != null && m_CharacterController.enabled)
		{
			if (!m_UseGravity || m_CharacterController.isGrounded)
			{
				m_GravityDrivenVelocity = Vector3.zero;
			}
			else
			{
				m_GravityDrivenVelocity += Physics.gravity * Time.deltaTime;
				if (m_EnableFreeXMovement)
				{
					m_GravityDrivenVelocity.x = 0f;
				}
				if (m_EnableFreeYMovement)
				{
					m_GravityDrivenVelocity.y = 0f;
				}
				if (m_EnableFreeZMovement)
				{
					m_GravityDrivenVelocity.z = 0f;
				}
			}
			motion += m_GravityDrivenVelocity * Time.deltaTime;
		}
		TryStartLocomotionImmediately();
		if (base.locomotionState == LocomotionState.Moving)
		{
			m_IsMovingXROrigin = true;
			transformation.motion = motion;
			TryQueueTransformation(transformation);
		}
	}

	private void FindCharacterController()
	{
		GameObject gameObject = base.mediator.xrOrigin?.Origin;
		if (!(gameObject == null) && m_CharacterController == null && !m_AttemptedGetCharacterController)
		{
			if (!gameObject.TryGetComponent<CharacterController>(out m_CharacterController) && gameObject != base.mediator.xrOrigin.gameObject)
			{
				base.mediator.xrOrigin.TryGetComponent<CharacterController>(out m_CharacterController);
			}
			m_AttemptedGetCharacterController = true;
		}
	}

	[Obsolete("Private migration helper.")]
	private void MigrateUseGravityToGravityProvider()
	{
		if (m_GravityProvider.useGravity != m_UseGravity)
		{
			Debug.LogWarning("Use Gravity is deprecated on this locomotion component while Gravity Provider component is in scene." + $" Automatically setting Use Gravity to {m_UseGravity} on Gravity Provider." + " Gravity should be controlled on the Gravity Provider instead.", this);
			m_GravityProvider.useGravity = m_UseGravity;
		}
	}
}
