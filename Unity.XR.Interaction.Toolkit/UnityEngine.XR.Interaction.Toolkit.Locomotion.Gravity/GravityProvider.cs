using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.Events;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

[AddComponentMenu("XR/Locomotion/Gravity Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity.GravityProvider.html")]
[DefaultExecutionOrder(-207)]
public class GravityProvider : LocomotionProvider
{
	[SerializeField]
	[Tooltip("Apply gravity to the XR Origin.")]
	private bool m_UseGravity = true;

	[SerializeField]
	[Tooltip("Apply gravity based on the current Up vector of the XR Origin.")]
	private bool m_UseLocalSpaceGravity = true;

	[SerializeField]
	[Tooltip("Determines the maximum fall speed based on units per second.")]
	private float m_TerminalVelocity = 90f;

	[SerializeField]
	[Tooltip("Determines the speed at which a player reaches max gravity velocity.")]
	private float m_GravityAccelerationModifier = 1f;

	[SerializeField]
	[Tooltip("Sets the center of the character controller to match the local x and z positions of the player camera.")]
	private bool m_UpdateCharacterControllerCenterEachFrame = true;

	[SerializeField]
	[Tooltip("Buffer for the radius of the sphere cast used to check if the player is grounded.")]
	private float m_SphereCastRadius = 0.09f;

	[SerializeField]
	[Tooltip("Buffer for the distance of the sphere cast used to check if the player is grounded.")]
	private float m_SphereCastDistanceBuffer = -0.05f;

	[SerializeField]
	[Tooltip("The layer mask used for the sphere cast to check if the player is grounded.")]
	private LayerMask m_SphereCastLayerMask = -5;

	[SerializeField]
	[Tooltip("Whether trigger colliders are considered when using a sphere cast to determine if grounded. Use Global refers to the Queries Hit Triggers setting in Physics Project Settings.")]
	private QueryTriggerInteraction m_SphereCastTriggerInteraction = QueryTriggerInteraction.Ignore;

	[Tooltip("Event that is called when gravity lock is changed.")]
	[SerializeField]
	private UnityEvent<GravityOverride> m_OnGravityLockChanged = new UnityEvent<GravityOverride>();

	[Tooltip("Callback for anytime the grounded state changes.")]
	[SerializeField]
	private UnityEvent<bool> m_OnGroundedChanged = new UnityEvent<bool>();

	private bool m_IsGrounded;

	private readonly List<IGravityController> m_GravityControllers = new List<IGravityController>();

	private Transform m_HeadTransform;

	private readonly RaycastHit[] m_GroundedAllocHits = new RaycastHit[1];

	private Vector3 m_CurrentFallVelocity;

	private PhysicsScene m_LocalPhysicsScene;

	private CharacterController m_CharacterController;

	private bool m_AttemptedGetCharacterController;

	private readonly List<IGravityController> m_GravityForcedOnProviders = new List<IGravityController>();

	private readonly List<IGravityController> m_GravityForcedOffProviders = new List<IGravityController>();

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

	public bool useLocalSpaceGravity
	{
		get
		{
			return m_UseLocalSpaceGravity;
		}
		set
		{
			m_UseLocalSpaceGravity = value;
		}
	}

	public float terminalVelocity
	{
		get
		{
			return m_TerminalVelocity;
		}
		set
		{
			m_TerminalVelocity = value;
		}
	}

	public float gravityAccelerationModifier
	{
		get
		{
			return m_GravityAccelerationModifier;
		}
		set
		{
			m_GravityAccelerationModifier = value;
		}
	}

	public bool updateCharacterControllerCenterEachFrame
	{
		get
		{
			return m_UpdateCharacterControllerCenterEachFrame;
		}
		set
		{
			m_UpdateCharacterControllerCenterEachFrame = value;
		}
	}

	public float sphereCastRadius
	{
		get
		{
			return m_SphereCastRadius;
		}
		set
		{
			m_SphereCastRadius = value;
		}
	}

	public float sphereCastDistanceBuffer
	{
		get
		{
			return m_SphereCastDistanceBuffer;
		}
		set
		{
			m_SphereCastDistanceBuffer = value;
		}
	}

	public LayerMask sphereCastLayerMask
	{
		get
		{
			return m_SphereCastLayerMask;
		}
		set
		{
			m_SphereCastLayerMask = value;
		}
	}

	public QueryTriggerInteraction sphereCastTriggerInteraction
	{
		get
		{
			return m_SphereCastTriggerInteraction;
		}
		set
		{
			m_SphereCastTriggerInteraction = value;
		}
	}

	public UnityEvent<GravityOverride> onGravityLockChanged
	{
		get
		{
			return m_OnGravityLockChanged;
		}
		set
		{
			m_OnGravityLockChanged = value;
		}
	}

	public UnityEvent<bool> onGroundedChanged => m_OnGroundedChanged;

	public bool isGrounded => m_IsGrounded;

	public XROriginMovement transformation { get; set; } = new XROriginMovement();


	public List<IGravityController> gravityControllers => m_GravityControllers;

	protected override void Awake()
	{
		base.Awake();
		m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
		if (base.mediator != null)
		{
			base.mediator.GetComponentsInChildren(includeInactive: true, m_GravityControllers);
		}
	}

	protected virtual void Start()
	{
		FindHeadTransform();
	}

	protected virtual void Update()
	{
		CheckGrounded();
		if (m_IsGrounded && base.locomotionState == LocomotionState.Moving)
		{
			TryEndLocomotion();
			ResetFallForce();
		}
		if (TryProcessGravity(Time.deltaTime))
		{
			TryStartLocomotionImmediately();
			if (base.locomotionState == LocomotionState.Moving)
			{
				transformation.motion = m_CurrentFallVelocity * Time.deltaTime;
				TryQueueTransformation(transformation);
			}
		}
		if (m_HeadTransform != null && m_CharacterController != null && m_UpdateCharacterControllerCenterEachFrame)
		{
			m_CharacterController.center = new Vector3(m_HeadTransform.localPosition.x, m_CharacterController.center.y, m_HeadTransform.localPosition.z);
		}
	}

	protected virtual bool TryProcessGravity(float time)
	{
		if (IsGravityBlocked())
		{
			ResetFallForce();
			return false;
		}
		if (!Mathf.Approximately(m_CurrentFallVelocity.sqrMagnitude, m_TerminalVelocity * m_TerminalVelocity))
		{
			m_CurrentFallVelocity = Vector3.ClampMagnitude(m_CurrentFallVelocity + m_GravityAccelerationModifier * time * GetCurrentGravity(), m_TerminalVelocity);
		}
		return true;
	}

	public Vector3 GetCurrentUp()
	{
		if (!m_UseLocalSpaceGravity)
		{
			return -Physics.gravity.normalized;
		}
		return base.mediator.xrOrigin.Origin.transform.up;
	}

	private Vector3 GetCurrentGravity()
	{
		if (!m_UseLocalSpaceGravity)
		{
			return Physics.gravity;
		}
		return -base.mediator.xrOrigin.Origin.transform.up * Physics.gravity.magnitude;
	}

	public bool IsGravityBlocked()
	{
		if (m_UseGravity && !m_IsGrounded)
		{
			return !CanProcessGravity();
		}
		return true;
	}

	public void ResetFallForce()
	{
		m_CurrentFallVelocity = Vector3.zero;
	}

	private bool CanProcessGravity()
	{
		if (m_GravityForcedOffProviders.Count > 0)
		{
			return false;
		}
		if (m_GravityForcedOnProviders.Count > 0)
		{
			return true;
		}
		foreach (IGravityController gravityController in m_GravityControllers)
		{
			if (gravityController.canProcess && gravityController.gravityPaused)
			{
				return false;
			}
		}
		return true;
	}

	public bool TryLockGravity(IGravityController provider, GravityOverride gravityOverride)
	{
		if (m_GravityForcedOffProviders.Contains(provider) || m_GravityForcedOnProviders.Contains(provider))
		{
			Debug.LogWarning($"Gravity Provider is already being locked by {((provider is Object @object) ? ((object)@object.name) : ((object)provider))}. Unlock first before trying to lock again.", (provider as Object) ?? this);
			return false;
		}
		switch (gravityOverride)
		{
		case GravityOverride.ForcedOff:
			m_GravityForcedOffProviders.Add(provider);
			break;
		case GravityOverride.ForcedOn:
			m_GravityForcedOnProviders.Add(provider);
			break;
		}
		foreach (IGravityController gravityController in m_GravityControllers)
		{
			gravityController.OnGravityLockChanged(gravityOverride);
		}
		m_OnGravityLockChanged?.Invoke(gravityOverride);
		return true;
	}

	public void UnlockGravity(IGravityController provider)
	{
		m_GravityForcedOnProviders.Remove(provider);
		m_GravityForcedOffProviders.Remove(provider);
	}

	private void CheckGrounded()
	{
		bool num = m_IsGrounded;
		m_IsGrounded = m_LocalPhysicsScene.SphereCast(GetBodyHeadPosition(), m_SphereCastRadius, -GetCurrentUp(), m_GroundedAllocHits, GetLocalHeadHeight(), m_SphereCastLayerMask, m_SphereCastTriggerInteraction) > 0;
		if (num == m_IsGrounded)
		{
			return;
		}
		foreach (IGravityController gravityController in m_GravityControllers)
		{
			gravityController.OnGroundedChanged(m_IsGrounded);
		}
		m_OnGroundedChanged?.Invoke(m_IsGrounded);
	}

	private float GetLocalHeadHeight()
	{
		return base.mediator.xrOrigin.CameraInOriginSpaceHeight + m_SphereCastDistanceBuffer;
	}

	private Vector3 GetBodyHeadPosition()
	{
		if (m_CharacterController == null)
		{
			FindCharacterController();
		}
		if (m_HeadTransform == null)
		{
			FindHeadTransform();
			if (m_HeadTransform == null)
			{
				if (!(m_CharacterController != null))
				{
					return base.transform.position;
				}
				return m_CharacterController.bounds.center;
			}
		}
		if (m_CharacterController == null && m_UpdateCharacterControllerCenterEachFrame)
		{
			return m_HeadTransform.position;
		}
		Vector3 center = m_CharacterController.bounds.center;
		return new Vector3(center.x, m_HeadTransform.position.y, center.z);
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

	private void FindHeadTransform()
	{
		XROrigin xrOrigin = base.mediator.xrOrigin;
		if (xrOrigin != null)
		{
			Camera camera = xrOrigin.Camera;
			if (camera != null)
			{
				m_HeadTransform = camera.transform;
				return;
			}
			Debug.LogError("Camera is not set in XR Origin, cannot obtain Transform reference to use as the head position. Disabling Gravity Provider.", this);
			base.enabled = false;
		}
		else
		{
			Debug.LogError("XR Origin is not available through the Locomotion Mediator, cannot obtain Transform reference to use as the head position. Disabling Gravity Provider.", this);
			base.enabled = false;
		}
	}

	protected void OnDrawGizmosSelected()
	{
		if (Application.isPlaying && !(m_HeadTransform == null))
		{
			Color color2 = (Gizmos.color = (m_IsGrounded ? Color.green : Color.red));
			Vector3 bodyHeadPosition = GetBodyHeadPosition();
			Vector3 vector = bodyHeadPosition + -GetCurrentUp() * m_GroundedAllocHits[0].distance;
			Gizmos.DrawWireSphere(vector, m_SphereCastRadius);
			Gizmos.DrawSphere(m_GroundedAllocHits[0].point, 0.025f);
			Debug.DrawLine(bodyHeadPosition, vector, color2);
		}
	}
}
