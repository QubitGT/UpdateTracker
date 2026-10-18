using System;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[Obsolete("CinemachineOrbitalTransposer has been deprecated. Use CinemachineOrbitalFollow instead")]
[AddComponentMenu("")]
[SaveDuringPlay]
[CameraPipeline(CinemachineCore.Stage.Body)]
public class CinemachineOrbitalTransposer : CinemachineTransposer, AxisState.IRequiresInput
{
	[Serializable]
	public struct Heading
	{
		public enum HeadingDefinition
		{
			PositionDelta,
			Velocity,
			TargetForward,
			WorldForward
		}

		[FormerlySerializedAs("m_HeadingDefinition")]
		[Tooltip("How 'forward' is defined.  The camera will be placed by default behind the target.  PositionDelta will consider 'forward' to be the direction in which the target is moving.")]
		public HeadingDefinition m_Definition;

		[Range(0f, 10f)]
		[Tooltip("Size of the velocity sampling window for target heading filter.  This filters out irregularities in the target's movement.  Used only if deriving heading from target's movement (PositionDelta or Velocity)")]
		public int m_VelocityFilterStrength;

		[Range(-180f, 180f)]
		[FormerlySerializedAs("m_HeadingBias")]
		[Tooltip("Where the camera is placed when the X-axis value is zero.  This is a rotation in degrees around the Y axis.  When this value is 0, the camera will be placed behind the target.  Nonzero offsets will rotate the zero position around the target.")]
		public float m_Bias;

		public Heading(HeadingDefinition def, int filterStrength, float bias)
		{
			m_Definition = def;
			m_VelocityFilterStrength = filterStrength;
			m_Bias = bias;
		}
	}

	internal delegate float UpdateHeadingDelegate(CinemachineOrbitalTransposer orbital, float deltaTime, Vector3 up);

	[Space]
	[Tooltip("The definition of Forward.  Camera will follow behind.")]
	public Heading m_Heading = new Heading(Heading.HeadingDefinition.TargetForward, 4, 0f);

	[Tooltip("Automatic heading recentering.  The settings here defines how the camera will reposition itself in the absence of player input.")]
	public AxisState.Recentering m_RecenterToTargetHeading = new AxisState.Recentering(enabled: true, 1f, 2f);

	[Tooltip("Heading Control.  The settings here control the behaviour of the camera in response to the player's input.")]
	public AxisState m_XAxis = new AxisState(-180f, 180f, wrap: true, rangeLocked: false, 300f, 0.1f, 0.1f, "Mouse X", invert: true);

	private Tracker m_TargetTracker;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("m_Radius")]
	private float m_LegacyRadius = float.MaxValue;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("m_HeightOffset")]
	private float m_LegacyHeightOffset = float.MaxValue;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("m_HeadingBias")]
	private float m_LegacyHeadingBias = float.MaxValue;

	[FormerlySerializedAs("m_HeadingIsSlave")]
	[HideInInspector]
	[NoSaveDuringPlay]
	public bool m_HeadingIsDriven;

	internal UpdateHeadingDelegate HeadingUpdater = (CinemachineOrbitalTransposer orbital, float deltaTime, Vector3 up) => orbital.UpdateHeading(deltaTime, up, ref orbital.m_XAxis, ref orbital.m_RecenterToTargetHeading, CinemachineCore.IsLive(orbital.VirtualCamera));

	private Vector3 m_LastTargetPosition = Vector3.zero;

	private HeadingTracker mHeadingTracker;

	private Rigidbody m_TargetRigidBody;

	private Transform m_PreviousTarget;

	private Vector3 m_LastCameraPosition;

	private float m_LastHeading;

	protected override void OnValidate()
	{
		if (m_LegacyRadius != float.MaxValue && m_LegacyHeightOffset != float.MaxValue && m_LegacyHeadingBias != float.MaxValue)
		{
			m_FollowOffset = new Vector3(0f, m_LegacyHeightOffset, 0f - m_LegacyRadius);
			m_LegacyHeightOffset = (m_LegacyRadius = float.MaxValue);
			m_Heading.m_Bias = m_LegacyHeadingBias;
			m_XAxis.m_MaxSpeed /= 10f;
			m_XAxis.m_AccelTime /= 10f;
			m_XAxis.m_DecelTime /= 10f;
			m_LegacyHeadingBias = float.MaxValue;
			int heading = (int)m_Heading.m_Definition;
			if (m_RecenterToTargetHeading.LegacyUpgrade(ref heading, ref m_Heading.m_VelocityFilterStrength))
			{
				m_Heading.m_Definition = (Heading.HeadingDefinition)heading;
			}
		}
		m_XAxis.Validate();
		m_RecenterToTargetHeading.Validate();
		base.OnValidate();
	}

	public float UpdateHeading(float deltaTime, Vector3 up, ref AxisState axis)
	{
		return UpdateHeading(deltaTime, up, ref axis, ref m_RecenterToTargetHeading, isLive: true);
	}

	public float UpdateHeading(float deltaTime, Vector3 up, ref AxisState axis, ref AxisState.Recentering recentering, bool isLive)
	{
		if (m_BindingMode == BindingMode.LazyFollow)
		{
			axis.m_MinValue = -180f;
			axis.m_MaxValue = 180f;
		}
		if (deltaTime < 0f || !base.VirtualCamera.PreviousStateIsValid || !isLive)
		{
			axis.Reset();
			recentering.CancelRecentering();
		}
		else if (axis.Update(deltaTime))
		{
			recentering.CancelRecentering();
		}
		if (m_BindingMode == BindingMode.LazyFollow)
		{
			float value = axis.Value;
			axis.Value = 0f;
			return value;
		}
		CameraState cameraState = base.VcamState;
		float targetHeading = GetTargetHeading(axis.Value, m_TargetTracker.GetReferenceOrientation(this, m_BindingMode, up, ref cameraState));
		recentering.DoRecentering(ref axis, deltaTime, targetHeading);
		return axis.Value;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_PreviousTarget = null;
		m_LastTargetPosition = Vector3.zero;
		UpdateInputAxisProvider();
	}

	bool AxisState.IRequiresInput.RequiresInput()
	{
		return true;
	}

	internal void UpdateInputAxisProvider()
	{
		m_XAxis.SetInputAxisProvider(0, null);
		if (!m_HeadingIsDriven && base.VirtualCamera != null)
		{
			AxisState.IInputAxisProvider component = base.VirtualCamera.GetComponent<AxisState.IInputAxisProvider>();
			if (component != null)
			{
				m_XAxis.SetInputAxisProvider(0, component);
			}
		}
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		base.OnTargetObjectWarped(target, positionDelta);
		if (target == base.FollowTarget)
		{
			m_LastTargetPosition += positionDelta;
			m_LastCameraPosition += positionDelta;
		}
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		base.ForceCameraPosition(pos, rot);
		m_LastCameraPosition = pos;
		m_XAxis.Value = GetAxisClosestValue(pos, base.VirtualCamera.State.ReferenceUp);
	}

	public override bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		m_RecenterToTargetHeading.DoRecentering(ref m_XAxis, -1f, 0f);
		m_RecenterToTargetHeading.CancelRecentering();
		if (fromCam != null && m_BindingMode != BindingMode.LazyFollow && (base.VirtualCamera.State.BlendHint & CameraState.BlendHints.InheritPosition) != 0 && !CinemachineCore.IsLiveInBlend(base.VirtualCamera))
		{
			m_XAxis.Value = GetAxisClosestValue(fromCam.State.RawPosition, worldUp);
			return true;
		}
		return false;
	}

	public float GetAxisClosestValue(Vector3 cameraPos, Vector3 up)
	{
		CameraState cameraState = base.VcamState;
		Quaternion referenceOrientation = m_TargetTracker.GetReferenceOrientation(this, m_BindingMode, up, ref cameraState);
		if (!(referenceOrientation * Vector3.forward).ProjectOntoPlane(up).AlmostZero() && base.FollowTarget != null)
		{
			float num = 0f;
			if (m_BindingMode != BindingMode.LazyFollow)
			{
				num += m_Heading.m_Bias;
			}
			referenceOrientation *= Quaternion.AngleAxis(num, up);
			Vector3 followTargetPosition = base.FollowTargetPosition;
			Vector3 from = (followTargetPosition + referenceOrientation * base.EffectiveOffset - followTargetPosition).ProjectOntoPlane(up);
			Vector3 to = (cameraPos - followTargetPosition).ProjectOntoPlane(up);
			return Vector3.SignedAngle(from, to, up);
		}
		return m_LastHeading;
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		m_TargetTracker.InitStateInfo(this, deltaTime, m_BindingMode, curState.ReferenceUp);
		if (base.FollowTarget != m_PreviousTarget)
		{
			m_PreviousTarget = base.FollowTarget;
			m_TargetRigidBody = ((m_PreviousTarget == null) ? null : m_PreviousTarget.GetComponent<Rigidbody>());
			m_LastTargetPosition = ((m_PreviousTarget == null) ? Vector3.zero : m_PreviousTarget.position);
			mHeadingTracker = null;
		}
		m_LastHeading = HeadingUpdater(this, deltaTime, curState.ReferenceUp);
		float num = m_LastHeading;
		if (!IsValid)
		{
			return;
		}
		if (m_BindingMode != BindingMode.LazyFollow)
		{
			num += m_Heading.m_Bias;
		}
		Quaternion quaternion = Quaternion.AngleAxis(num, Vector3.up);
		Vector3 effectiveOffset = base.EffectiveOffset;
		Vector3 vector = quaternion * effectiveOffset;
		ref Tracker targetTracker = ref m_TargetTracker;
		Vector3 referenceUp = curState.ReferenceUp;
		Vector3 desiredCameraOffset = vector;
		TrackerSettings settings = base.TrackerSettings;
		targetTracker.TrackTarget(this, deltaTime, referenceUp, desiredCameraOffset, in settings, ref curState, out var outTargetPosition, out var outTargetOrient);
		vector = outTargetOrient * vector;
		curState.ReferenceUp = outTargetOrient * Vector3.up;
		Vector3 followTargetPosition = base.FollowTargetPosition;
		outTargetPosition += m_TargetTracker.GetOffsetForMinimumTargetDistance(this, outTargetPosition, vector, curState.RawOrientation * Vector3.forward, curState.ReferenceUp, followTargetPosition);
		curState.RawPosition = outTargetPosition + vector;
		if (deltaTime >= 0f && base.VirtualCamera.PreviousStateIsValid)
		{
			Vector3 vector2 = followTargetPosition;
			if (base.LookAtTarget != null)
			{
				vector2 = base.LookAtTargetPosition;
			}
			Vector3 v = m_LastCameraPosition - vector2;
			Vector3 v2 = curState.RawPosition - vector2;
			if (v.sqrMagnitude > 0.01f && v2.sqrMagnitude > 0.01f)
			{
				curState.RotationDampingBypass *= UnityVectorExtensions.SafeFromToRotation(v, v2, curState.ReferenceUp);
			}
		}
		m_LastTargetPosition = followTargetPosition;
		m_LastCameraPosition = curState.RawPosition;
	}

	internal override Vector3 GetTargetCameraPosition(Vector3 worldUp)
	{
		if (!IsValid)
		{
			return Vector3.zero;
		}
		float num = m_LastHeading;
		if (m_BindingMode != BindingMode.LazyFollow)
		{
			num += m_Heading.m_Bias;
		}
		CameraState cameraState = base.VcamState;
		Quaternion quaternion = Quaternion.AngleAxis(num, Vector3.up);
		quaternion = m_TargetTracker.GetReferenceOrientation(this, m_BindingMode, worldUp, ref cameraState) * quaternion;
		return quaternion * base.EffectiveOffset + m_LastTargetPosition;
	}

	private float GetTargetHeading(float currentHeading, Quaternion targetOrientation)
	{
		if (m_BindingMode == BindingMode.LazyFollow)
		{
			return 0f;
		}
		if (base.FollowTarget == null)
		{
			return currentHeading;
		}
		Heading.HeadingDefinition headingDefinition = m_Heading.m_Definition;
		if (headingDefinition == Heading.HeadingDefinition.Velocity && m_TargetRigidBody == null)
		{
			headingDefinition = Heading.HeadingDefinition.PositionDelta;
		}
		Vector3 zero = Vector3.zero;
		switch (headingDefinition)
		{
		case Heading.HeadingDefinition.Velocity:
			zero = m_TargetRigidBody.linearVelocity;
			break;
		case Heading.HeadingDefinition.PositionDelta:
			zero = base.FollowTargetPosition - m_LastTargetPosition;
			break;
		case Heading.HeadingDefinition.TargetForward:
			zero = base.FollowTargetRotation * Vector3.forward;
			break;
		default:
			return 0f;
		}
		Vector3 vector = targetOrientation * Vector3.up;
		zero = zero.ProjectOntoPlane(vector);
		if (headingDefinition != Heading.HeadingDefinition.TargetForward)
		{
			int num = m_Heading.m_VelocityFilterStrength * 5;
			if (mHeadingTracker == null || mHeadingTracker.FilterSize != num)
			{
				mHeadingTracker = new HeadingTracker(num);
			}
			mHeadingTracker.DecayHistory();
			if (!zero.AlmostZero())
			{
				mHeadingTracker.Add(zero);
			}
			zero = mHeadingTracker.GetReliableHeading();
		}
		if (!zero.AlmostZero())
		{
			return UnityVectorExtensions.SignedAngle(targetOrientation * Vector3.forward, zero, vector);
		}
		return currentHeading;
	}

	internal void UpgradeToCm3(CinemachineOrbitalFollow c)
	{
		c.TrackerSettings = base.TrackerSettings;
		c.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
		c.Radius = 0f - m_FollowOffset.z;
		c.HorizontalAxis.Range = new Vector2(m_XAxis.m_MinValue, m_XAxis.m_MaxValue);
		c.HorizontalAxis.Wrap = m_XAxis.m_Wrap;
		c.HorizontalAxis.Center = c.HorizontalAxis.ClampValue(0f);
		c.HorizontalAxis.Value = c.HorizontalAxis.ClampValue(m_XAxis.Value);
		c.HorizontalAxis.Recentering = new InputAxis.RecenteringSettings
		{
			Enabled = m_RecenterToTargetHeading.m_enabled,
			Time = m_RecenterToTargetHeading.m_RecenteringTime,
			Wait = m_RecenterToTargetHeading.m_WaitTime
		};
		c.VerticalAxis.Center = (c.VerticalAxis.Value = m_FollowOffset.y);
		c.VerticalAxis.Range = new Vector2(c.VerticalAxis.Center, c.VerticalAxis.Center);
		c.RadialAxis.Range = Vector2.one;
		c.RadialAxis.Center = (c.HorizontalAxis.Value = 1f);
	}
}
