using System;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace Unity.Cinemachine;

[Obsolete("CinemachineTransposer has been deprecated. Use CinemachineFollow instead")]
[AddComponentMenu("")]
[SaveDuringPlay]
[CameraPipeline(CinemachineCore.Stage.Body)]
public class CinemachineTransposer : CinemachineComponentBase
{
	[Tooltip("The coordinate space to use when interpreting the offset from the target.  This is also used to set the camera's Up vector, which will be maintained when aiming the camera.")]
	public BindingMode m_BindingMode = BindingMode.LockToTargetWithWorldUp;

	[Tooltip("The distance vector that the transposer will attempt to maintain from the Follow target")]
	public Vector3 m_FollowOffset = Vector3.back * 10f;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to maintain the offset in the X-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's x-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
	public float m_XDamping = 1f;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to maintain the offset in the Y-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's y-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
	public float m_YDamping = 1f;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to maintain the offset in the Z-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's z-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
	public float m_ZDamping = 1f;

	public AngularDampingMode m_AngularDampingMode;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to track the target rotation's X angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
	public float m_PitchDamping;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to track the target rotation's Y angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
	public float m_YawDamping;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to track the target rotation's Z angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
	public float m_RollDamping;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to track the target's orientation.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
	public float m_AngularDamping;

	private Tracker m_TargetTracker;

	protected TrackerSettings TrackerSettings
	{
		get
		{
			TrackerSettings result = default(TrackerSettings);
			result.BindingMode = m_BindingMode;
			result.PositionDamping = new Vector3(m_XDamping, m_YDamping, m_ZDamping);
			result.RotationDamping = new Vector3(m_PitchDamping, m_YawDamping, m_RollDamping);
			result.AngularDampingMode = m_AngularDampingMode;
			result.QuaternionDamping = m_AngularDamping;
			return result;
		}
	}

	internal bool HideOffsetInInspector { get; set; }

	public Vector3 EffectiveOffset
	{
		get
		{
			Vector3 followOffset = m_FollowOffset;
			if (m_BindingMode == BindingMode.LazyFollow)
			{
				followOffset.x = 0f;
				followOffset.z = 0f - Mathf.Abs(followOffset.z);
			}
			return followOffset;
		}
	}

	public override bool IsValid
	{
		get
		{
			if (base.enabled)
			{
				return base.FollowTarget != null;
			}
			return false;
		}
	}

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Body;

	protected virtual void OnValidate()
	{
		m_FollowOffset = EffectiveOffset;
	}

	public override float GetMaxDampTime()
	{
		return TrackerSettings.GetMaxDampTime();
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		m_TargetTracker.InitStateInfo(this, deltaTime, m_BindingMode, curState.ReferenceUp);
		if (IsValid)
		{
			Vector3 effectiveOffset = EffectiveOffset;
			ref Tracker targetTracker = ref m_TargetTracker;
			Vector3 referenceUp = curState.ReferenceUp;
			Vector3 desiredCameraOffset = effectiveOffset;
			TrackerSettings settings = TrackerSettings;
			targetTracker.TrackTarget(this, deltaTime, referenceUp, desiredCameraOffset, in settings, ref curState, out var outTargetPosition, out var outTargetOrient);
			effectiveOffset = outTargetOrient * effectiveOffset;
			curState.ReferenceUp = outTargetOrient * Vector3.up;
			Vector3 followTargetPosition = base.FollowTargetPosition;
			outTargetPosition += m_TargetTracker.GetOffsetForMinimumTargetDistance(this, outTargetPosition, effectiveOffset, curState.RawOrientation * Vector3.forward, curState.ReferenceUp, followTargetPosition);
			curState.RawPosition = outTargetPosition + effectiveOffset;
		}
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		base.OnTargetObjectWarped(target, positionDelta);
		if (target == base.FollowTarget)
		{
			m_TargetTracker.OnTargetObjectWarped(positionDelta);
		}
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		base.ForceCameraPosition(pos, rot);
		CameraState newState = base.VcamState;
		newState.RawPosition = pos;
		newState.RawOrientation = rot;
		newState.PositionCorrection = Vector3.zero;
		newState.OrientationCorrection = Quaternion.identity;
		m_TargetTracker.OnForceCameraPosition(this, m_BindingMode, ref newState);
	}

	internal Quaternion GetReferenceOrientation(Vector3 up)
	{
		CameraState cameraState = base.VcamState;
		return m_TargetTracker.GetReferenceOrientation(this, m_BindingMode, up, ref cameraState);
	}

	internal virtual Vector3 GetTargetCameraPosition(Vector3 worldUp)
	{
		if (!IsValid)
		{
			return Vector3.zero;
		}
		CameraState cameraState = base.VcamState;
		return base.FollowTargetPosition + m_TargetTracker.GetReferenceOrientation(this, m_BindingMode, worldUp, ref cameraState) * EffectiveOffset;
	}

	internal void UpgradeToCm3(CinemachineFollow c)
	{
		c.FollowOffset = m_FollowOffset;
		c.TrackerSettings = TrackerSettings;
	}
}
