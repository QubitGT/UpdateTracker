using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Confiner 3D")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineConfiner3D.html")]
public class CinemachineConfiner3D : CinemachineExtension
{
	private class VcamExtraState : VcamExtraStateBase
	{
		public Vector3 PreviousDisplacement;

		public Vector3 PreviousCameraPosition;
	}

	[Tooltip("The volume within which the camera is to be contained")]
	public Collider BoundingVolume;

	[Tooltip("Size of the slow-down zone at the edge of the bounding volume.")]
	public float SlowingDistance;

	public bool IsValid
	{
		get
		{
			if (BoundingVolume != null && BoundingVolume.enabled)
			{
				return BoundingVolume.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	public bool CameraWasDisplaced(CinemachineVirtualCameraBase vcam)
	{
		return GetCameraDisplacementDistance(vcam) > 0f;
	}

	public float GetCameraDisplacementDistance(CinemachineVirtualCameraBase vcam)
	{
		return GetExtraState<VcamExtraState>(vcam).PreviousDisplacement.magnitude;
	}

	private void Reset()
	{
		BoundingVolume = null;
		SlowingDistance = 0f;
	}

	private void OnValidate()
	{
		SlowingDistance = Mathf.Max(0f, SlowingDistance);
	}

	public override float GetMaxDampTime()
	{
		return SlowingDistance * 0.2f;
	}

	public override void OnTargetObjectWarped(CinemachineVirtualCameraBase vcam, Transform target, Vector3 positionDelta)
	{
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		if (extraState.Vcam.Follow == target)
		{
			extraState.PreviousCameraPosition += positionDelta;
		}
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage != 0 || !IsValid)
		{
			return;
		}
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		Vector3 correctedPosition = state.GetCorrectedPosition();
		Vector3 vector = ConfinePoint(correctedPosition);
		if (SlowingDistance > 0.0001f && deltaTime >= 0f && vcam.PreviousStateIsValid)
		{
			Vector3 previousCameraPosition = extraState.PreviousCameraPosition;
			Vector3 vector2 = vector - previousCameraPosition;
			float magnitude = vector2.magnitude;
			if (magnitude > 0.0001f)
			{
				float num = GetDistanceFromEdge(previousCameraPosition, vector2 / magnitude, SlowingDistance) / SlowingDistance;
				vector = Vector3.Lerp(previousCameraPosition, vector, num * num * num + 0.05f);
			}
		}
		Vector3 vector3 = vector - correctedPosition;
		state.PositionCorrection += vector3;
		extraState.PreviousCameraPosition = state.GetCorrectedPosition();
		extraState.PreviousDisplacement = vector3;
	}

	private Vector3 ConfinePoint(Vector3 p)
	{
		MeshCollider meshCollider = BoundingVolume as MeshCollider;
		if (meshCollider != null && !meshCollider.convex)
		{
			return p;
		}
		return BoundingVolume.ClosestPoint(p);
	}

	private float GetDistanceFromEdge(Vector3 p, Vector3 dirUnit, float max)
	{
		p += dirUnit * max;
		return max - (ConfinePoint(p) - p).magnitude;
	}
}
