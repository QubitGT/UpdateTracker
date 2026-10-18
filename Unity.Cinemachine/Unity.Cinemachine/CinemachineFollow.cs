using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Position Control/Cinemachine Follow")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Body)]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineFollow.html")]
public class CinemachineFollow : CinemachineComponentBase
{
	public TrackerSettings TrackerSettings = TrackerSettings.Default;

	[Tooltip("The distance vector that the camera will attempt to maintain from the tracking target")]
	public Vector3 FollowOffset = Vector3.back * 10f;

	private Tracker m_TargetTracker;

	internal Vector3 EffectiveOffset
	{
		get
		{
			Vector3 followOffset = FollowOffset;
			if (TrackerSettings.BindingMode == BindingMode.LazyFollow)
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

	private void OnValidate()
	{
		FollowOffset = EffectiveOffset;
		TrackerSettings.Validate();
	}

	private void Reset()
	{
		FollowOffset = Vector3.back * 10f;
		TrackerSettings = TrackerSettings.Default;
	}

	public override float GetMaxDampTime()
	{
		return TrackerSettings.GetMaxDampTime();
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		m_TargetTracker.InitStateInfo(this, deltaTime, TrackerSettings.BindingMode, curState.ReferenceUp);
		if (IsValid)
		{
			Vector3 effectiveOffset = EffectiveOffset;
			m_TargetTracker.TrackTarget(this, deltaTime, curState.ReferenceUp, effectiveOffset, in TrackerSettings, ref curState, out var outTargetPosition, out var outTargetOrient);
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
		m_TargetTracker.OnForceCameraPosition(this, TrackerSettings.BindingMode, ref newState);
	}

	internal Quaternion GetReferenceOrientation(Vector3 up)
	{
		CameraState cameraState = base.VcamState;
		return m_TargetTracker.GetReferenceOrientation(this, TrackerSettings.BindingMode, up, ref cameraState);
	}

	internal Vector3 GetDesiredCameraPosition(Vector3 worldUp)
	{
		if (!IsValid)
		{
			return Vector3.zero;
		}
		CameraState cameraState = base.VcamState;
		return base.FollowTargetPosition + m_TargetTracker.GetReferenceOrientation(this, TrackerSettings.BindingMode, worldUp, ref cameraState) * EffectiveOffset;
	}
}
