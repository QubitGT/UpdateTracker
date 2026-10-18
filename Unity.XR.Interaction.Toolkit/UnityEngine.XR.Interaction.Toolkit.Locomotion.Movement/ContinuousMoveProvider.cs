using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[AddComponentMenu("XR/Locomotion/Continuous Move Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider.html")]
public class ContinuousMoveProvider : LocomotionProvider, IGravityController
{
	[SerializeField]
	[Tooltip("The speed, in units per second, to move forward.")]
	private float m_MoveSpeed = 1f;

	[SerializeField]
	[Tooltip("Determines how much control the player has while in the air (0 = no control, 1 = full control).")]
	private float m_InAirControlModifier = 0.5f;

	[SerializeField]
	[Tooltip("Controls whether to enable strafing (sideways movement).")]
	private bool m_EnableStrafe = true;

	[SerializeField]
	[Tooltip("Controls whether to enable flying (unconstrained movement). This overrides the use of gravity.")]
	private bool m_EnableFly;

	[SerializeField]
	[Tooltip("The source Transform to define the forward direction.")]
	private Transform m_ForwardSource;

	[SerializeField]
	[Tooltip("Reads input data from the left hand controller. Input Action must be a Value action type (Vector 2).")]
	private XRInputValueReader<Vector2> m_LeftHandMoveInput = new XRInputValueReader<Vector2>("Left Hand Move");

	[SerializeField]
	[Tooltip("Reads input data from the right hand controller. Input Action must be a Value action type (Vector 2).")]
	private XRInputValueReader<Vector2> m_RightHandMoveInput = new XRInputValueReader<Vector2>("Right Hand Move");

	private GravityProvider m_GravityProvider;

	private CharacterController m_CharacterController;

	private bool m_AttemptedGetCharacterController;

	private bool m_IsMovingXROrigin;

	private Vector3 m_GravityDrivenVelocity;

	private Vector3 m_InAirVelocity;

	[SerializeField]
	[Tooltip("Controls whether gravity affects this provider when a Character Controller is used and flying is disabled. Ignored when a Gravity Provider component is found in the scene. Deprecated in XRI 3.1.0, use Gravity Provider instead.")]
	[Obsolete("Controlling gravity directly in the move provider has been deprecated in XRI 3.1.0, use Gravity Provider instead.")]
	private bool m_UseGravity = true;

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

	public float inAirControlModifier
	{
		get
		{
			return m_InAirControlModifier;
		}
		set
		{
			m_InAirControlModifier = value;
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

	public XROriginMovement transformation { get; set; } = new XROriginMovement();


	public XRInputValueReader<Vector2> leftHandMoveInput
	{
		get
		{
			return m_LeftHandMoveInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_LeftHandMoveInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> rightHandMoveInput
	{
		get
		{
			return m_RightHandMoveInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_RightHandMoveInput, value, this);
		}
	}

	public bool canProcess => base.isActiveAndEnabled;

	public bool gravityPaused => m_EnableFly;

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

	protected void OnEnable()
	{
		m_LeftHandMoveInput.EnableDirectActionIfModeUsed();
		m_RightHandMoveInput.EnableDirectActionIfModeUsed();
		m_GravityDrivenVelocity = Vector3.zero;
		m_InAirVelocity = Vector3.zero;
	}

	protected void OnDisable()
	{
		m_LeftHandMoveInput.DisableDirectActionIfModeUsed();
		m_RightHandMoveInput.DisableDirectActionIfModeUsed();
	}

	protected override void OnLocomotionStateChanging(LocomotionState state)
	{
		base.OnLocomotionStateChanging(state);
		switch (state)
		{
		case LocomotionState.Moving:
			TryLockGravity((!m_EnableFly) ? GravityOverride.ForcedOn : GravityOverride.ForcedOff);
			break;
		case LocomotionState.Ended:
			RemoveGravityLock();
			break;
		}
	}

	protected override void OnLocomotionStarting()
	{
		base.OnLocomotionStarting();
	}

	protected override void OnLocomotionEnding()
	{
		base.OnLocomotionEnding();
	}

	protected void Update()
	{
		m_IsMovingXROrigin = false;
		if (!(base.mediator.xrOrigin?.Origin == null))
		{
			Vector2 vector = ReadInput();
			Vector3 translationInWorldSpace = ComputeDesiredMove(vector);
			if (vector != Vector2.zero || m_GravityDrivenVelocity != Vector3.zero || m_InAirVelocity != Vector3.zero)
			{
				MoveRig(translationInWorldSpace);
			}
			if (!m_IsMovingXROrigin)
			{
				TryEndLocomotion();
			}
		}
	}

	private Vector2 ReadInput()
	{
		Vector2 vector = m_LeftHandMoveInput.ReadValue();
		Vector2 vector2 = m_RightHandMoveInput.ReadValue();
		return vector + vector2;
	}

	protected virtual Vector3 ComputeDesiredMove(Vector2 input)
	{
		if (input == Vector2.zero && m_InAirVelocity == Vector3.zero)
		{
			return Vector3.zero;
		}
		XROrigin xrOrigin = base.mediator.xrOrigin;
		if (xrOrigin == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = Vector3.ClampMagnitude(new Vector3(m_EnableStrafe ? input.x : 0f, 0f, input.y), 1f);
		float deltaTime = Time.deltaTime;
		if (m_GravityProvider == null || !m_GravityProvider.enabled || !m_GravityProvider.useGravity || m_GravityProvider.isGrounded)
		{
			m_InAirVelocity = vector;
		}
		else
		{
			m_InAirVelocity += deltaTime * m_InAirControlModifier * 10f * (vector - m_InAirVelocity);
		}
		Transform transform = ((m_ForwardSource == null) ? xrOrigin.Camera.transform : m_ForwardSource);
		Vector3 vector2 = transform.forward;
		Transform transform2 = xrOrigin.Origin.transform;
		float num = m_MoveSpeed * deltaTime * transform2.localScale.x;
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
		Vector3 direction = Quaternion.FromToRotation(transform2.forward, toDirection) * m_InAirVelocity * num;
		return transform2.TransformDirection(direction);
	}

	protected virtual void MoveRig(Vector3 translationInWorldSpace)
	{
		if (base.mediator.xrOrigin?.Origin == null)
		{
			return;
		}
		FindCharacterController();
		Vector3 motion = translationInWorldSpace;
		if (m_GravityProvider == null && m_CharacterController != null && m_CharacterController.enabled)
		{
			if (m_CharacterController.isGrounded || !m_UseGravity || m_EnableFly)
			{
				m_GravityDrivenVelocity = Vector3.zero;
			}
			else
			{
				m_GravityDrivenVelocity += Physics.gravity * Time.deltaTime;
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

	public bool TryLockGravity(GravityOverride gravityOverride)
	{
		if (m_GravityProvider != null)
		{
			return m_GravityProvider.TryLockGravity(this, gravityOverride);
		}
		return false;
	}

	public void RemoveGravityLock()
	{
		if (m_GravityProvider != null)
		{
			m_GravityProvider.UnlockGravity(this);
		}
	}

	void IGravityController.OnGroundedChanged(bool isGrounded)
	{
		OnGroundedChanged(isGrounded);
	}

	void IGravityController.OnGravityLockChanged(GravityOverride gravityOverride)
	{
		OnGravityLockChanged(gravityOverride);
	}

	protected virtual void OnGroundedChanged(bool isGrounded)
	{
	}

	protected virtual void OnGravityLockChanged(GravityOverride gravityOverride)
	{
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
