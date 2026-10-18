using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("XR/Lazy Follow", 22)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.LazyFollow.html")]
public class LazyFollow : MonoBehaviour
{
	public enum PositionFollowMode
	{
		None,
		Follow
	}

	public enum RotationFollowMode
	{
		None,
		LookAt,
		LookAtWithWorldUp,
		Follow
	}

	private const float k_LowerSpeedVariance = 0f;

	private const float k_UpperSpeedVariance = 0.999f;

	[Header("Target Config")]
	[SerializeField]
	[Tooltip("(Optional) The object being followed. If not set, this will default to the main camera when this component is enabled.")]
	private Transform m_Target;

	[SerializeField]
	[Tooltip("The amount to offset the target's position when following. This position is relative/local to the target object.")]
	private Vector3 m_TargetOffset = new Vector3(0f, 0f, 0.5f);

	[Space]
	[SerializeField]
	[Tooltip("If true, read the local transform of the target to lazy follow, otherwise read the world transform. If using look at rotation follow modes, only world-space follow is supported.")]
	private bool m_FollowInLocalSpace;

	[SerializeField]
	[Tooltip("If true, apply the target offset in local space. If false, apply the target offset in world space.")]
	private bool m_ApplyTargetInLocalSpace;

	[Header("General Follow Params")]
	[SerializeField]
	[Tooltip("Movement speed used when smoothing to new target. Lower values mean the lazy follow lags further behind the target.")]
	private float m_MovementSpeed = 6f;

	[SerializeField]
	[Range(0f, 0.999f)]
	[Tooltip("Adjust movement speed based on distance from the target using a tolerance percentage. 0% for constant speed.")]
	private float m_MovementSpeedVariancePercentage = 0.25f;

	[SerializeField]
	[Tooltip("Snap to target position when this component is enabled.")]
	private bool m_SnapOnEnable = true;

	[Header("Position Follow Params")]
	[SerializeField]
	[Tooltip("Determines the follow mode used to determine a new rotation. Look At is best used with the target being the main camera.")]
	private PositionFollowMode m_PositionFollowMode = PositionFollowMode.Follow;

	[SerializeField]
	[Tooltip("Minimum distance from target before which a follow lazy follow starts.")]
	private float m_MinDistanceAllowed = 0.01f;

	[SerializeField]
	[Tooltip("Maximum distance from target before lazy follow targets, when time threshold is reached.")]
	private float m_MaxDistanceAllowed = 0.3f;

	[SerializeField]
	[Tooltip("Time required to elapse (in seconds) before the max distance allowed goes from the min distance to the max.")]
	private float m_TimeUntilThresholdReachesMaxDistance = 3f;

	[Header("Rotation Follow Params")]
	[SerializeField]
	[Tooltip("Determines the follow mode used to determine a new rotation. Look At is best used with the target being the main camera.")]
	private RotationFollowMode m_RotationFollowMode = RotationFollowMode.LookAt;

	[SerializeField]
	[Tooltip("Minimum angle offset (in degrees) from target before which lazy follow starts.")]
	private float m_MinAngleAllowed = 0.1f;

	[SerializeField]
	[Tooltip("Maximum angle offset (in degrees) from target before lazy follow targets, when time threshold is reached.")]
	private float m_MaxAngleAllowed = 5f;

	[SerializeField]
	[Tooltip("Time required to elapse (in seconds) before the max angle offset allowed goes from the min angle offset to the max.")]
	private float m_TimeUntilThresholdReachesMaxAngle = 3f;

	private float m_LowerMovementSpeed;

	private float m_UpperMovementSpeed;

	private readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

	private SmartFollowVector3TweenableVariable m_Vector3TweenableVariable;

	private SmartFollowQuaternionTweenableVariable m_QuaternionTweenableVariable;

	public Transform target
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
		}
	}

	public Vector3 targetOffset
	{
		get
		{
			return m_TargetOffset;
		}
		set
		{
			m_TargetOffset = value;
		}
	}

	public bool followInLocalSpace
	{
		get
		{
			return m_FollowInLocalSpace;
		}
		set
		{
			m_FollowInLocalSpace = value;
			ValidateFollowMode();
		}
	}

	public bool applyTargetInLocalSpace
	{
		get
		{
			return m_ApplyTargetInLocalSpace;
		}
		set
		{
			m_ApplyTargetInLocalSpace = value;
		}
	}

	public float movementSpeed
	{
		get
		{
			return m_MovementSpeed;
		}
		set
		{
			m_MovementSpeed = value;
			UpdateUpperAndLowerSpeedBounds();
		}
	}

	public float movementSpeedVariancePercentage
	{
		get
		{
			return m_MovementSpeedVariancePercentage;
		}
		set
		{
			m_MovementSpeedVariancePercentage = Mathf.Clamp(value, 0f, 0.999f);
			UpdateUpperAndLowerSpeedBounds();
		}
	}

	public bool snapOnEnable
	{
		get
		{
			return m_SnapOnEnable;
		}
		set
		{
			m_SnapOnEnable = value;
		}
	}

	public PositionFollowMode positionFollowMode
	{
		get
		{
			return m_PositionFollowMode;
		}
		set
		{
			m_PositionFollowMode = value;
		}
	}

	public float minDistanceAllowed
	{
		get
		{
			return m_MinDistanceAllowed;
		}
		set
		{
			m_MinDistanceAllowed = value;
			if (m_Vector3TweenableVariable != null)
			{
				m_Vector3TweenableVariable.minDistanceAllowed = value;
			}
		}
	}

	public float maxDistanceAllowed
	{
		get
		{
			return m_MaxDistanceAllowed;
		}
		set
		{
			m_MaxDistanceAllowed = value;
			if (m_Vector3TweenableVariable != null)
			{
				m_Vector3TweenableVariable.maxDistanceAllowed = value;
			}
		}
	}

	public float timeUntilThresholdReachesMaxDistance
	{
		get
		{
			return m_TimeUntilThresholdReachesMaxDistance;
		}
		set
		{
			m_TimeUntilThresholdReachesMaxDistance = value;
			if (m_Vector3TweenableVariable != null)
			{
				m_Vector3TweenableVariable.minToMaxDelaySeconds = value;
			}
		}
	}

	public RotationFollowMode rotationFollowMode
	{
		get
		{
			return m_RotationFollowMode;
		}
		set
		{
			m_RotationFollowMode = value;
			ValidateFollowMode();
		}
	}

	public float minAngleAllowed
	{
		get
		{
			return m_MinAngleAllowed;
		}
		set
		{
			m_MinAngleAllowed = value;
			if (m_QuaternionTweenableVariable != null)
			{
				m_QuaternionTweenableVariable.minAngleAllowed = value;
			}
		}
	}

	public float maxAngleAllowed
	{
		get
		{
			return m_MaxAngleAllowed;
		}
		set
		{
			m_MaxAngleAllowed = value;
			if (m_QuaternionTweenableVariable != null)
			{
				m_QuaternionTweenableVariable.maxAngleAllowed = value;
			}
		}
	}

	public float timeUntilThresholdReachesMaxAngle
	{
		get
		{
			return m_TimeUntilThresholdReachesMaxAngle;
		}
		set
		{
			m_TimeUntilThresholdReachesMaxAngle = value;
			if (m_QuaternionTweenableVariable != null)
			{
				m_QuaternionTweenableVariable.minToMaxDelaySeconds = value;
			}
		}
	}

	protected void OnValidate()
	{
		UpdateUpperAndLowerSpeedBounds();
		ValidateFollowMode();
		if (m_Vector3TweenableVariable != null)
		{
			m_Vector3TweenableVariable.minDistanceAllowed = m_MinDistanceAllowed;
			m_Vector3TweenableVariable.maxDistanceAllowed = m_MaxDistanceAllowed;
			m_Vector3TweenableVariable.minToMaxDelaySeconds = m_TimeUntilThresholdReachesMaxDistance;
		}
		if (m_QuaternionTweenableVariable != null)
		{
			m_QuaternionTweenableVariable.minAngleAllowed = m_MinAngleAllowed;
			m_QuaternionTweenableVariable.maxAngleAllowed = m_MaxAngleAllowed;
			m_QuaternionTweenableVariable.minToMaxDelaySeconds = m_TimeUntilThresholdReachesMaxAngle;
		}
	}

	protected void Awake()
	{
		m_Vector3TweenableVariable = new SmartFollowVector3TweenableVariable(m_MinDistanceAllowed, m_MaxDistanceAllowed, m_TimeUntilThresholdReachesMaxDistance);
		m_QuaternionTweenableVariable = new SmartFollowQuaternionTweenableVariable(m_MinAngleAllowed, m_MaxAngleAllowed, m_TimeUntilThresholdReachesMaxAngle);
		UpdateUpperAndLowerSpeedBounds();
		ValidateFollowMode();
	}

	protected void OnEnable()
	{
		if (m_Target == null)
		{
			Camera main = Camera.main;
			if (main != null)
			{
				m_Target = main.transform;
			}
		}
		Pose pose = (followInLocalSpace ? base.transform.GetLocalPose() : base.transform.GetWorldPose());
		m_Vector3TweenableVariable.target = pose.position;
		m_QuaternionTweenableVariable.target = pose.rotation;
		m_BindingsGroup.AddBinding(m_Vector3TweenableVariable.SubscribeAndUpdate(UpdatePosition));
		m_BindingsGroup.AddBinding(m_QuaternionTweenableVariable.SubscribeAndUpdate(UpdateRotation));
		if (m_SnapOnEnable)
		{
			if (m_PositionFollowMode != 0 && TryGetThresholdTargetPosition(out var newTarget))
			{
				m_Vector3TweenableVariable.target = newTarget;
			}
			if (m_RotationFollowMode != 0 && TryGetThresholdTargetRotation(out var newTarget2))
			{
				m_QuaternionTweenableVariable.target = newTarget2;
			}
			m_Vector3TweenableVariable.HandleTween(1f);
			m_QuaternionTweenableVariable.HandleTween(1f);
		}
	}

	protected void OnDisable()
	{
		m_BindingsGroup.Clear();
	}

	protected void OnDestroy()
	{
		m_Vector3TweenableVariable?.Dispose();
	}

	protected void LateUpdate()
	{
		if (m_Target == null)
		{
			return;
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (m_PositionFollowMode != 0)
		{
			if (TryGetThresholdTargetPosition(out var newTarget))
			{
				m_Vector3TweenableVariable.target = newTarget;
			}
			if (m_MovementSpeedVariancePercentage > 0f)
			{
				m_Vector3TweenableVariable.HandleSmartTween(unscaledDeltaTime, m_LowerMovementSpeed, m_UpperMovementSpeed);
			}
			else
			{
				m_Vector3TweenableVariable.HandleTween(unscaledDeltaTime * movementSpeed);
			}
		}
		if (m_RotationFollowMode != 0)
		{
			if (TryGetThresholdTargetRotation(out var newTarget2))
			{
				m_QuaternionTweenableVariable.target = newTarget2;
			}
			if (m_MovementSpeedVariancePercentage > 0f)
			{
				m_QuaternionTweenableVariable.HandleSmartTween(unscaledDeltaTime, m_LowerMovementSpeed, m_UpperMovementSpeed);
			}
			else
			{
				m_QuaternionTweenableVariable.HandleTween(unscaledDeltaTime * movementSpeed);
			}
		}
	}

	private void UpdatePosition(float3 position)
	{
		if (applyTargetInLocalSpace)
		{
			base.transform.localPosition = position;
		}
		else
		{
			base.transform.position = position;
		}
	}

	private void UpdateRotation(Quaternion rotation)
	{
		if (applyTargetInLocalSpace)
		{
			base.transform.localRotation = rotation;
		}
		else
		{
			base.transform.rotation = rotation;
		}
	}

	protected virtual bool TryGetThresholdTargetPosition(out Vector3 newTarget)
	{
		PositionFollowMode positionFollowMode = m_PositionFollowMode;
		if (positionFollowMode != 0)
		{
			if (positionFollowMode == PositionFollowMode.Follow)
			{
				if (followInLocalSpace)
				{
					newTarget = m_Target.localPosition + m_TargetOffset;
				}
				else
				{
					newTarget = m_Target.position + m_Target.TransformVector(m_TargetOffset);
				}
				return m_Vector3TweenableVariable.IsNewTargetWithinThreshold(newTarget);
			}
			Debug.LogError(string.Format("Unhandled {0}={1}", "PositionFollowMode", m_PositionFollowMode), this);
		}
		newTarget = (followInLocalSpace ? base.transform.localPosition : base.transform.position);
		return false;
	}

	protected virtual bool TryGetThresholdTargetRotation(out Quaternion newTarget)
	{
		switch (m_RotationFollowMode)
		{
		case RotationFollowMode.None:
			newTarget = (followInLocalSpace ? base.transform.localRotation : base.transform.rotation);
			return false;
		case RotationFollowMode.LookAt:
		{
			Vector3 forward = (base.transform.position - m_Target.position).normalized;
			Vector3 referenceUp = Vector3.up;
			BurstMathUtility.OrthogonalLookRotation(in forward, in referenceUp, out newTarget);
			break;
		}
		case RotationFollowMode.LookAtWithWorldUp:
		{
			Vector3 forward2 = (base.transform.position - m_Target.position).normalized;
			Vector3 referenceUp = Vector3.up;
			BurstMathUtility.LookRotationWithForwardProjectedOnPlane(in forward2, in referenceUp, out newTarget);
			break;
		}
		case RotationFollowMode.Follow:
			newTarget = (followInLocalSpace ? m_Target.localRotation : m_Target.rotation);
			break;
		default:
			Debug.LogError(string.Format("Unhandled {0}={1}", "RotationFollowMode", m_RotationFollowMode), this);
			goto case RotationFollowMode.None;
		}
		return m_QuaternionTweenableVariable.IsNewTargetWithinThreshold(newTarget);
	}

	private void ValidateFollowMode()
	{
		if (m_FollowInLocalSpace && (m_RotationFollowMode == RotationFollowMode.LookAt || m_RotationFollowMode == RotationFollowMode.LookAtWithWorldUp))
		{
			if (Application.isPlaying)
			{
				m_FollowInLocalSpace = false;
				XRLoggingUtils.LogWarning("Cannot follow in local space if Rotation Follow Mode set to look at the target. Turning off Follow In Local Space.", this);
			}
			else
			{
				XRLoggingUtils.LogWarning("Cannot follow in local space if Rotation Follow Mode set to look at the target.", this);
			}
		}
	}

	private void UpdateUpperAndLowerSpeedBounds()
	{
		if (m_MovementSpeedVariancePercentage > 0f)
		{
			m_LowerMovementSpeed = m_MovementSpeed - m_MovementSpeedVariancePercentage * m_MovementSpeed;
			m_UpperMovementSpeed = m_MovementSpeed * (1f + m_MovementSpeedVariancePercentage);
		}
		else
		{
			m_LowerMovementSpeed = m_MovementSpeed;
			m_UpperMovementSpeed = m_MovementSpeed;
		}
	}
}
