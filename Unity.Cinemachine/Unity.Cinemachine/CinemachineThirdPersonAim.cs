using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Third Person Aim")]
[ExecuteAlways]
[SaveDuringPlay]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineThirdPersonAim.html")]
public class CinemachineThirdPersonAim : CinemachineExtension
{
	[Header("Aim Target Detection")]
	[Tooltip("Objects on these layers will be detected")]
	public LayerMask AimCollisionFilter;

	[TagField]
	[Tooltip("Objects with this tag will be ignored.  It is a good idea to set this field to the target's tag")]
	public string IgnoreTag = string.Empty;

	[Tooltip("How far to project the object detection ray")]
	public float AimDistance;

	[Tooltip("If set, camera noise will be adjusted to stabilize target on screen")]
	public bool NoiseCancellation = true;

	public Vector3 AimTarget { get; private set; }

	private void OnValidate()
	{
		AimDistance = Mathf.Max(1f, AimDistance);
	}

	private void Reset()
	{
		AimCollisionFilter = 1;
		IgnoreTag = string.Empty;
		AimDistance = 200f;
		NoiseCancellation = true;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		switch (stage)
		{
		case CinemachineCore.Stage.Body:
			if (NoiseCancellation)
			{
				Transform follow2 = vcam.Follow;
				if (follow2 != null)
				{
					state.ReferenceLookAt = ComputeLookAtPoint(state.GetCorrectedPosition(), follow2, follow2.forward);
					AimTarget = ComputeAimTarget(state.ReferenceLookAt, follow2);
				}
			}
			break;
		case CinemachineCore.Stage.Finalize:
			if (NoiseCancellation)
			{
				Vector3 forward = state.ReferenceLookAt - state.GetFinalPosition();
				if (forward.sqrMagnitude > 0.01f)
				{
					state.RawOrientation = Quaternion.LookRotation(forward, state.ReferenceUp);
					state.OrientationCorrection = Quaternion.identity;
				}
			}
			else
			{
				Transform follow = vcam.Follow;
				if (follow != null)
				{
					state.ReferenceLookAt = ComputeLookAtPoint(state.GetCorrectedPosition(), follow, state.GetCorrectedOrientation() * Vector3.forward);
					AimTarget = ComputeAimTarget(state.ReferenceLookAt, follow);
				}
			}
			break;
		}
	}

	private Vector3 ComputeLookAtPoint(Vector3 camPos, Transform player, Vector3 fwd)
	{
		float num = AimDistance;
		Vector3 vector = Quaternion.Inverse(player.rotation) * (player.position - camPos);
		if (vector.z > 0f)
		{
			camPos += fwd * vector.z;
			num -= vector.z;
		}
		num = Mathf.Max(1f, num);
		if (!RuntimeUtility.RaycastIgnoreTag(new Ray(camPos, fwd), out var hitInfo, num, AimCollisionFilter, in IgnoreTag))
		{
			return camPos + fwd * num;
		}
		return hitInfo.point;
	}

	private Vector3 ComputeAimTarget(Vector3 cameraLookAt, Transform player)
	{
		Vector3 position = player.position;
		Vector3 direction = cameraLookAt - position;
		if (RuntimeUtility.RaycastIgnoreTag(new Ray(position, direction), out var hitInfo, direction.magnitude, AimCollisionFilter, in IgnoreTag))
		{
			return hitInfo.point;
		}
		return cameraLookAt;
	}
}
