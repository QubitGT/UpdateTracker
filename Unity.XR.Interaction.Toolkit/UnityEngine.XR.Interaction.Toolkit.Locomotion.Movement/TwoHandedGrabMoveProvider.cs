using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[DefaultExecutionOrder(-209)]
[AddComponentMenu("XR/Locomotion/Two-Handed Grab Move Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.TwoHandedGrabMoveProvider.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TwoHandedGrabMoveProvider : ConstrainedMoveProvider
{
	[SerializeField]
	[Tooltip("The left hand grab move instance which will be used as one half of two-handed locomotion.")]
	private GrabMoveProvider m_LeftGrabMoveProvider;

	[SerializeField]
	[Tooltip("The right hand grab move instance which will be used as one half of two-handed locomotion.")]
	private GrabMoveProvider m_RightGrabMoveProvider;

	[SerializeField]
	[Tooltip("Controls whether to override the settings for individual handed providers with this provider's settings on initialization.")]
	private bool m_OverrideSharedSettingsOnInit = true;

	[SerializeField]
	[Tooltip("The ratio of actual movement distance to controller movement distance.")]
	private float m_MoveFactor = 1f;

	[SerializeField]
	[Tooltip("Controls whether translation requires both grab move inputs to be active.")]
	private bool m_RequireTwoHandsForTranslation;

	[SerializeField]
	[Tooltip("Controls whether to enable yaw rotation of the user.")]
	private bool m_EnableRotation = true;

	[SerializeField]
	[Tooltip("Controls whether to enable uniform scaling of the user.")]
	private bool m_EnableScaling;

	[SerializeField]
	[Tooltip("The minimum user scale allowed.")]
	private float m_MinimumScale = 0.2f;

	[SerializeField]
	[Tooltip("The maximum user scale allowed.")]
	private float m_MaximumScale = 100f;

	private bool m_IsMoving;

	private Vector3 m_PreviousMidpointBetweenControllers;

	private float m_InitialOriginYaw;

	private Vector3 m_InitialLeftToRightDirection;

	private Vector3 m_InitialLeftToRightOrthogonal;

	private float m_InitialOriginScale;

	private float m_InitialDistanceBetweenHands;

	public GrabMoveProvider leftGrabMoveProvider
	{
		get
		{
			return m_LeftGrabMoveProvider;
		}
		set
		{
			m_LeftGrabMoveProvider = value;
		}
	}

	public GrabMoveProvider rightGrabMoveProvider
	{
		get
		{
			return m_RightGrabMoveProvider;
		}
		set
		{
			m_RightGrabMoveProvider = value;
		}
	}

	public bool overrideSharedSettingsOnInit
	{
		get
		{
			return m_OverrideSharedSettingsOnInit;
		}
		set
		{
			m_OverrideSharedSettingsOnInit = value;
		}
	}

	public float moveFactor
	{
		get
		{
			return m_MoveFactor;
		}
		set
		{
			m_MoveFactor = value;
		}
	}

	public bool requireTwoHandsForTranslation
	{
		get
		{
			return m_RequireTwoHandsForTranslation;
		}
		set
		{
			m_RequireTwoHandsForTranslation = value;
		}
	}

	public bool enableRotation
	{
		get
		{
			return m_EnableRotation;
		}
		set
		{
			m_EnableRotation = value;
		}
	}

	public bool enableScaling
	{
		get
		{
			return m_EnableScaling;
		}
		set
		{
			m_EnableScaling = value;
		}
	}

	public float minimumScale
	{
		get
		{
			return m_MinimumScale;
		}
		set
		{
			m_MinimumScale = value;
		}
	}

	public float maximumScale
	{
		get
		{
			return m_MaximumScale;
		}
		set
		{
			m_MaximumScale = value;
		}
	}

	public XRBodyYawRotation rotateTransformation { get; set; } = new XRBodyYawRotation();


	public XRBodyScale scaleTransformation { get; set; } = new XRBodyScale();


	protected void OnEnable()
	{
		if (m_LeftGrabMoveProvider == null || m_RightGrabMoveProvider == null)
		{
			Debug.LogError("Left or Right Grab Move Provider is not set or has been destroyed.", this);
			base.enabled = false;
			return;
		}
		if (m_RequireTwoHandsForTranslation)
		{
			m_LeftGrabMoveProvider.canMove = false;
			m_RightGrabMoveProvider.canMove = false;
		}
		if (m_OverrideSharedSettingsOnInit)
		{
			m_LeftGrabMoveProvider.mediator = base.mediator;
			m_LeftGrabMoveProvider.enableFreeXMovement = base.enableFreeXMovement;
			m_LeftGrabMoveProvider.enableFreeYMovement = base.enableFreeYMovement;
			m_LeftGrabMoveProvider.enableFreeZMovement = base.enableFreeZMovement;
			m_LeftGrabMoveProvider.moveFactor = m_MoveFactor;
			m_RightGrabMoveProvider.mediator = base.mediator;
			m_RightGrabMoveProvider.enableFreeXMovement = base.enableFreeXMovement;
			m_RightGrabMoveProvider.enableFreeYMovement = base.enableFreeYMovement;
			m_RightGrabMoveProvider.enableFreeZMovement = base.enableFreeZMovement;
			m_RightGrabMoveProvider.moveFactor = m_MoveFactor;
			m_LeftGrabMoveProvider.useGravity = base.useGravity;
			m_RightGrabMoveProvider.useGravity = base.useGravity;
		}
	}

	protected void OnDisable()
	{
		if (m_LeftGrabMoveProvider != null)
		{
			m_LeftGrabMoveProvider.canMove = true;
		}
		if (m_RightGrabMoveProvider != null)
		{
			m_RightGrabMoveProvider.canMove = true;
		}
	}

	protected override Vector3 ComputeDesiredMove(out bool attemptingMove)
	{
		attemptingMove = false;
		bool isMoving = m_IsMoving;
		GameObject gameObject = base.mediator.xrOrigin?.Origin;
		m_IsMoving = m_LeftGrabMoveProvider.IsGrabbing() && m_RightGrabMoveProvider.IsGrabbing() && gameObject != null;
		if (!m_IsMoving)
		{
			if (!m_RequireTwoHandsForTranslation)
			{
				m_LeftGrabMoveProvider.canMove = true;
				m_RightGrabMoveProvider.canMove = true;
			}
			return Vector3.zero;
		}
		m_LeftGrabMoveProvider.canMove = false;
		m_RightGrabMoveProvider.canMove = false;
		Transform transform = gameObject.transform;
		Vector3 localPosition = m_LeftGrabMoveProvider.controllerTransform.localPosition;
		Vector3 localPosition2 = m_RightGrabMoveProvider.controllerTransform.localPosition;
		Vector3 vector = (localPosition + localPosition2) * 0.5f;
		if (!isMoving && m_IsMoving)
		{
			m_InitialOriginYaw = transform.eulerAngles.y;
			m_InitialLeftToRightDirection = localPosition2 - localPosition;
			m_InitialLeftToRightDirection.y = 0f;
			m_InitialLeftToRightOrthogonal = Quaternion.AngleAxis(90f, Vector3.down) * m_InitialLeftToRightDirection;
			m_InitialOriginScale = transform.localScale.x;
			m_InitialDistanceBetweenHands = Vector3.Distance(localPosition, localPosition2);
			m_PreviousMidpointBetweenControllers = vector;
			return Vector3.zero;
		}
		attemptingMove = true;
		Vector3 result = transform.TransformVector(m_PreviousMidpointBetweenControllers - vector) * m_MoveFactor;
		m_PreviousMidpointBetweenControllers = vector;
		return result;
	}

	protected override void MoveRig(Vector3 translationInWorldSpace)
	{
		base.MoveRig(translationInWorldSpace);
		GameObject gameObject = base.mediator.xrOrigin?.Origin;
		if (!(gameObject == null))
		{
			Transform transform = gameObject.transform;
			Vector3 localPosition = m_LeftGrabMoveProvider.controllerTransform.localPosition;
			Vector3 localPosition2 = m_RightGrabMoveProvider.controllerTransform.localPosition;
			if (m_EnableRotation)
			{
				Vector3 vector = localPosition2 - localPosition;
				vector.y = 0f;
				float num = Mathf.Sign(Vector3.Dot(m_InitialLeftToRightOrthogonal, vector));
				float num2 = m_InitialOriginYaw + Vector3.Angle(m_InitialLeftToRightDirection, vector) * num;
				rotateTransformation.angleDelta = num2 - transform.eulerAngles.y;
				TryQueueTransformation(rotateTransformation);
			}
			if (m_EnableScaling)
			{
				float num3 = Vector3.Distance(localPosition, localPosition2);
				float value = ((num3 != 0f) ? (m_InitialOriginScale * (m_InitialDistanceBetweenHands / num3)) : transform.localScale.x);
				value = Mathf.Clamp(value, m_MinimumScale, m_MaximumScale);
				scaleTransformation.uniformScale = value;
				TryQueueTransformation(scaleTransformation);
			}
		}
	}
}
