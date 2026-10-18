using UnityEngine;

namespace Unity.Cinemachine.TargetTracking;

internal struct Tracker
{
	private Quaternion m_TargetOrientationOnAssign;

	private Vector3 m_PreviousOffset;

	private Transform m_PreviousTarget;

	public Vector3 PreviousTargetPosition { get; private set; }

	public Quaternion PreviousReferenceOrientation { get; private set; }

	public void InitStateInfo(CinemachineComponentBase component, float deltaTime, BindingMode bindingMode, Vector3 up)
	{
		bool flag = deltaTime >= 0f && component.VirtualCamera.PreviousStateIsValid;
		if (m_PreviousTarget != component.FollowTarget || !flag)
		{
			m_PreviousTarget = component.FollowTarget;
			m_TargetOrientationOnAssign = component.FollowTargetRotation;
		}
		if (!flag)
		{
			PreviousTargetPosition = component.FollowTargetPosition;
			CameraState cameraState = component.VcamState;
			PreviousReferenceOrientation = GetReferenceOrientation(component, bindingMode, up, ref cameraState);
		}
	}

	public Quaternion GetReferenceOrientation(CinemachineComponentBase component, BindingMode bindingMode, Vector3 worldUp, ref CameraState cameraState)
	{
		if (bindingMode == BindingMode.WorldSpace)
		{
			return Quaternion.identity;
		}
		if (component.FollowTarget != null)
		{
			Quaternion followTargetRotation = component.FollowTargetRotation;
			switch (bindingMode)
			{
			case BindingMode.LockToTargetOnAssign:
				return m_TargetOrientationOnAssign;
			case BindingMode.LockToTargetWithWorldUp:
			{
				Vector3 vector2 = (followTargetRotation * Vector3.forward).ProjectOntoPlane(worldUp);
				if (!vector2.AlmostZero())
				{
					return Quaternion.LookRotation(vector2, worldUp);
				}
				break;
			}
			case BindingMode.LockToTargetNoRoll:
				return Quaternion.LookRotation(followTargetRotation * Vector3.forward, worldUp);
			case BindingMode.LockToTarget:
				return followTargetRotation;
			case BindingMode.LazyFollow:
			{
				Vector3 vector = (component.FollowTargetPosition - cameraState.RawPosition).ProjectOntoPlane(worldUp);
				if (!vector.AlmostZero())
				{
					return Quaternion.LookRotation(vector, worldUp);
				}
				break;
			}
			}
		}
		if (PreviousReferenceOrientation == new Quaternion(0f, 0f, 0f, 0f))
		{
			return Quaternion.identity;
		}
		return PreviousReferenceOrientation.normalized;
	}

	public void TrackTarget(CinemachineComponentBase component, float deltaTime, Vector3 up, Vector3 desiredCameraOffset, in TrackerSettings settings, ref CameraState cameraState, out Vector3 outTargetPosition, out Quaternion outTargetOrient)
	{
		Quaternion referenceOrientation = GetReferenceOrientation(component, settings.BindingMode, up, ref cameraState);
		Quaternion quaternion = referenceOrientation;
		bool flag = deltaTime >= 0f && component.VirtualCamera.PreviousStateIsValid;
		if (flag)
		{
			if (settings.AngularDampingMode == AngularDampingMode.Quaternion && settings.BindingMode == BindingMode.LockToTarget)
			{
				float t = component.VirtualCamera.DetachedFollowTargetDamp(1f, settings.QuaternionDamping, deltaTime);
				quaternion = Quaternion.Slerp(PreviousReferenceOrientation, referenceOrientation, t);
			}
			else if (settings.BindingMode != BindingMode.LazyFollow)
			{
				Vector3 eulerAngles = (Quaternion.Inverse(PreviousReferenceOrientation) * referenceOrientation).eulerAngles;
				for (int i = 0; i < 3; i++)
				{
					if (eulerAngles[i] > 180f)
					{
						eulerAngles[i] -= 360f;
					}
					if (Mathf.Abs(eulerAngles[i]) < 0.01f)
					{
						eulerAngles[i] = 0f;
					}
				}
				eulerAngles = component.VirtualCamera.DetachedFollowTargetDamp(eulerAngles, settings.GetEffectiveRotationDamping(), deltaTime);
				quaternion = PreviousReferenceOrientation * Quaternion.Euler(eulerAngles);
			}
		}
		PreviousReferenceOrientation = quaternion;
		Vector3 followTargetPosition = component.FollowTargetPosition;
		Vector3 vector = PreviousTargetPosition;
		Vector3 vector2 = (flag ? m_PreviousOffset : desiredCameraOffset);
		if ((desiredCameraOffset - vector2).sqrMagnitude > 0.01f)
		{
			Quaternion quaternion2 = UnityVectorExtensions.SafeFromToRotation(m_PreviousOffset, desiredCameraOffset, up);
			vector = followTargetPosition + quaternion2 * (PreviousTargetPosition - followTargetPosition);
		}
		m_PreviousOffset = desiredCameraOffset;
		Vector3 vector3 = followTargetPosition - vector;
		if (flag)
		{
			Quaternion obj = (desiredCameraOffset.AlmostZero() ? component.VcamState.RawOrientation : Quaternion.LookRotation(quaternion * desiredCameraOffset, up));
			Vector3 initial = Quaternion.Inverse(obj) * vector3;
			initial = component.VirtualCamera.DetachedFollowTargetDamp(initial, settings.GetEffectivePositionDamping(), deltaTime);
			vector3 = obj * initial;
		}
		vector += vector3;
		Vector3 vector5 = (PreviousTargetPosition = vector);
		outTargetPosition = vector5;
		outTargetOrient = quaternion;
	}

	public Vector3 GetOffsetForMinimumTargetDistance(CinemachineComponentBase component, Vector3 dampedTargetPos, Vector3 cameraOffset, Vector3 cameraFwd, Vector3 up, Vector3 actualTargetPos)
	{
		Vector3 vector = Vector3.zero;
		if (component.VirtualCamera.FollowTargetAttachment > 0.9999f)
		{
			cameraOffset = cameraOffset.ProjectOntoPlane(up);
			float num = cameraOffset.magnitude * 0.2f;
			if (num > 0f)
			{
				actualTargetPos = actualTargetPos.ProjectOntoPlane(up);
				dampedTargetPos = dampedTargetPos.ProjectOntoPlane(up);
				Vector3 vector2 = dampedTargetPos + cameraOffset;
				float num2 = Vector3.Dot(actualTargetPos - vector2, (dampedTargetPos - vector2).normalized);
				if (num2 < num)
				{
					Vector3 vector3 = actualTargetPos - dampedTargetPos;
					float magnitude = vector3.magnitude;
					if (magnitude < 0.01f)
					{
						vector3 = -cameraFwd.ProjectOntoPlane(up);
					}
					else
					{
						vector3 /= magnitude;
					}
					vector = vector3 * (num - num2);
				}
				PreviousTargetPosition += vector;
			}
		}
		return vector;
	}

	public void OnTargetObjectWarped(Vector3 positionDelta)
	{
		PreviousTargetPosition += positionDelta;
	}

	public void OnForceCameraPosition(CinemachineComponentBase component, BindingMode bindingMode, ref CameraState newState)
	{
		CameraState vcamState = component.VcamState;
		Vector3 vector = Quaternion.Inverse(vcamState.GetFinalOrientation()) * (PreviousTargetPosition - vcamState.GetFinalPosition());
		vector = newState.GetFinalOrientation() * vector;
		PreviousTargetPosition = newState.GetFinalPosition() + vector;
		PreviousReferenceOrientation = GetReferenceOrientation(component, bindingMode, newState.ReferenceUp, ref newState);
		m_PreviousOffset = -vector;
	}
}
