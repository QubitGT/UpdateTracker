using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;

[AddComponentMenu("XR/Locomotion/Jump Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump.JumpProvider.html")]
public class JumpProvider : LocomotionProvider, IGravityController
{
	[SerializeField]
	[Tooltip("Disable gravity during the jump. This will result in a more floaty jump.")]
	private bool m_DisableGravityDuringJump;

	[SerializeField]
	[Tooltip("Allow player to jump without being grounded.")]
	private bool m_UnlimitedInAirJumps;

	[SerializeField]
	[Tooltip("The number of times a player can jump before landing.")]
	private int m_InAirJumpCount = 1;

	[SerializeField]
	[Tooltip("The time window after leaving the ground that a jump can still be performed. Sometimes known as coyote time.")]
	private float m_JumpForgivenessWindow = 0.25f;

	[SerializeField]
	[Tooltip("The height (approximately in meters) the player will be when reaching the apex of the jump.")]
	private float m_JumpHeight = 1.25f;

	[SerializeField]
	[Tooltip("Allow the player to stop their jump early when input is released before reaching the maximum jump height.")]
	private bool m_VariableHeightJump = true;

	[SerializeField]
	[Tooltip("The minimum amount of time the jump will execute for.")]
	private float m_MinJumpHoldTime = 0.1f;

	[SerializeField]
	[Tooltip("The maximum time a player can hold down the jump button to increase altitude.")]
	private float m_MaxJumpHoldTime = 0.5f;

	[SerializeField]
	[Tooltip("The speed at which the jump will decelerate when the player releases the jump button early.")]
	private float m_EarlyOutDecelerationSpeed = 0.1f;

	[SerializeField]
	[Tooltip("Input data that will be used to perform a jump.")]
	private XRInputButtonReader m_JumpInput = new XRInputButtonReader("Jump");

	private bool m_IsJumping;

	private bool m_HasJumped;

	private float m_CurrentJumpForgivenessWindowTime;

	private float m_StoppingJumpTime;

	private float m_CurrentJumpForceThisFrame;

	private Vector3 m_JumpVector;

	private GravityProvider m_GravityProvider;

	private bool m_HasGravityProvider;

	private float m_CurrentJumpTimer;

	private int m_CurrentInAirJumpCount;

	public bool disableGravityDuringJump
	{
		get
		{
			return m_DisableGravityDuringJump;
		}
		set
		{
			m_DisableGravityDuringJump = value;
		}
	}

	public bool unlimitedInAirJumps
	{
		get
		{
			return m_UnlimitedInAirJumps;
		}
		set
		{
			m_UnlimitedInAirJumps = value;
		}
	}

	public int inAirJumpCount
	{
		get
		{
			return m_InAirJumpCount;
		}
		set
		{
			m_InAirJumpCount = Mathf.Max(0, value);
			m_CurrentInAirJumpCount = m_InAirJumpCount;
		}
	}

	public float jumpForgivenessWindow
	{
		get
		{
			return m_JumpForgivenessWindow;
		}
		set
		{
			m_JumpForgivenessWindow = value;
			m_CurrentJumpForgivenessWindowTime = m_JumpForgivenessWindow;
		}
	}

	public float jumpHeight
	{
		get
		{
			return m_JumpHeight;
		}
		set
		{
			m_JumpHeight = value;
		}
	}

	public bool variableHeightJump
	{
		get
		{
			return m_VariableHeightJump;
		}
		set
		{
			m_VariableHeightJump = value;
		}
	}

	public float minJumpHoldTime
	{
		get
		{
			return m_MinJumpHoldTime;
		}
		set
		{
			m_MinJumpHoldTime = value;
		}
	}

	public float maxJumpHoldTime
	{
		get
		{
			return m_MaxJumpHoldTime;
		}
		set
		{
			m_MaxJumpHoldTime = value;
		}
	}

	public float earlyOutDecelerationSpeed
	{
		get
		{
			return m_EarlyOutDecelerationSpeed;
		}
		set
		{
			m_EarlyOutDecelerationSpeed = value;
		}
	}

	public XRInputButtonReader jumpInput
	{
		get
		{
			return m_JumpInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_JumpInput, value, this);
		}
	}

	public XROriginMovement transformation { get; set; } = new XROriginMovement();


	public bool isJumping => m_IsJumping;

	public bool canProcess => base.isActiveAndEnabled;

	public bool gravityPaused { get; protected set; }

	protected virtual void OnValidate()
	{
		m_InAirJumpCount = Mathf.Max(0, m_InAirJumpCount);
	}

	protected override void Awake()
	{
		base.Awake();
		m_HasGravityProvider = ComponentLocatorUtility<GravityProvider>.TryFindComponent(out m_GravityProvider);
		if (!m_HasGravityProvider)
		{
			Debug.LogError("Could not find Gravity Provider component which is required by the Jump Provider component. Disabling component.", this);
			base.enabled = false;
		}
	}

	protected virtual void OnEnable()
	{
		m_JumpInput.EnableDirectActionIfModeUsed();
		m_CurrentInAirJumpCount = m_InAirJumpCount;
	}

	protected virtual void OnDisable()
	{
		m_JumpInput.DisableDirectActionIfModeUsed();
	}

	protected virtual void Update()
	{
		CheckJump();
	}

	private void CheckJump()
	{
		if (m_HasGravityProvider)
		{
			if (m_CurrentJumpForgivenessWindowTime > 0f)
			{
				m_CurrentJumpForgivenessWindowTime -= Time.deltaTime;
			}
			if (m_HasJumped && m_JumpInput.ReadWasCompletedThisFrame())
			{
				m_HasJumped = false;
			}
			if (!m_HasJumped && m_JumpInput.ReadIsPerformed())
			{
				Jump();
			}
			if (m_IsJumping)
			{
				UpdateJump();
			}
		}
	}

	public void Jump()
	{
		if (CanJump())
		{
			if (!m_GravityProvider.isGrounded)
			{
				m_CurrentInAirJumpCount--;
			}
			m_HasJumped = true;
			m_IsJumping = true;
			m_CurrentJumpTimer = 0f;
			m_StoppingJumpTime = m_MaxJumpHoldTime;
			m_CurrentJumpForgivenessWindowTime = 0f;
			m_CurrentJumpForceThisFrame = m_JumpHeight;
			if (m_DisableGravityDuringJump)
			{
				TryLockGravity(GravityOverride.ForcedOff);
			}
			m_GravityProvider.ResetFallForce();
		}
	}

	public bool CanJump()
	{
		if (!m_UnlimitedInAirJumps && m_CurrentInAirJumpCount <= 0 && !m_GravityProvider.isGrounded)
		{
			return m_CurrentJumpForgivenessWindowTime > 0f;
		}
		return true;
	}

	private void UpdateJump()
	{
		float deltaTime = Time.deltaTime;
		ProcessJumpForce(deltaTime);
		if (m_GravityProvider.useLocalSpaceGravity)
		{
			m_JumpVector = m_CurrentJumpForceThisFrame * deltaTime * m_GravityProvider.GetCurrentUp();
		}
		else
		{
			m_JumpVector.y = m_CurrentJumpForceThisFrame * deltaTime;
		}
		TryStartLocomotionImmediately();
		if (base.locomotionState == LocomotionState.Moving)
		{
			transformation.motion = m_JumpVector;
			TryQueueTransformation(transformation);
		}
	}

	private void ProcessJumpForce(float dt)
	{
		m_CurrentJumpTimer += dt;
		if (m_StoppingJumpTime == m_MaxJumpHoldTime && (m_MaxJumpHoldTime <= 0f || (m_VariableHeightJump && m_CurrentJumpTimer > m_MinJumpHoldTime && !m_JumpInput.ReadIsPerformed())))
		{
			m_StoppingJumpTime = Mathf.Min(m_CurrentJumpTimer + m_EarlyOutDecelerationSpeed, m_MaxJumpHoldTime);
		}
		m_CurrentJumpForceThisFrame = CalculateJumpForceForFrame(Mathf.Clamp01(m_CurrentJumpTimer / m_StoppingJumpTime));
		if (m_CurrentJumpTimer >= m_StoppingJumpTime)
		{
			StopJump();
		}
	}

	private float CalculateJumpForceForFrame(float normalizedJumpTime)
	{
		float b = 5f;
		float num = 4f;
		float num2 = Mathf.Lerp(7f, b, Mathf.Clamp01(m_JumpHeight / num));
		if (m_DisableGravityDuringJump)
		{
			num2 /= 1.5f;
		}
		return (1f - normalizedJumpTime) * m_JumpHeight * num2;
	}

	private void StopJump()
	{
		m_IsJumping = false;
		if (m_DisableGravityDuringJump)
		{
			RemoveGravityLock();
		}
	}

	private void StartCoyoteTimer()
	{
		m_CurrentJumpForgivenessWindowTime = m_JumpForgivenessWindow;
	}

	public bool IsPausingGravity()
	{
		if (m_IsJumping)
		{
			return m_DisableGravityDuringJump;
		}
		return false;
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
		gravityPaused = false;
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (!isGrounded)
		{
			if (!m_IsJumping)
			{
				StartCoyoteTimer();
			}
			return;
		}
		m_CurrentJumpForgivenessWindowTime = 0f;
		m_JumpVector = Vector3.zero;
		m_CurrentInAirJumpCount = m_InAirJumpCount;
		if (m_IsJumping)
		{
			StopJump();
		}
	}

	protected virtual void OnGravityLockChanged(GravityOverride gravityOverride)
	{
		if (gravityOverride == GravityOverride.ForcedOn)
		{
			gravityPaused = false;
		}
	}
}
