using UnityEngine;

namespace Unity.Cinemachine;

[ExecuteAlways]
public abstract class CinemachineComponentBase : MonoBehaviour
{
	protected const float Epsilon = 0.0001f;

	private CinemachineVirtualCameraBase m_VcamOwner;

	public CinemachineVirtualCameraBase VirtualCamera
	{
		get
		{
			if (m_VcamOwner == null)
			{
				TryGetComponent<CinemachineVirtualCameraBase>(out m_VcamOwner);
			}
			if (m_VcamOwner == null && base.transform.parent != null)
			{
				base.transform.parent.TryGetComponent<CinemachineVirtualCameraBase>(out m_VcamOwner);
			}
			return m_VcamOwner;
		}
	}

	public Transform FollowTarget
	{
		get
		{
			CinemachineVirtualCameraBase virtualCamera = VirtualCamera;
			if (!(virtualCamera == null))
			{
				return virtualCamera.ResolveFollow(virtualCamera.Follow);
			}
			return null;
		}
	}

	public Transform LookAtTarget
	{
		get
		{
			CinemachineVirtualCameraBase virtualCamera = VirtualCamera;
			if (!(virtualCamera == null))
			{
				return virtualCamera.ResolveLookAt(virtualCamera.LookAt);
			}
			return null;
		}
	}

	public ICinemachineTargetGroup FollowTargetAsGroup
	{
		get
		{
			CinemachineVirtualCameraBase virtualCamera = VirtualCamera;
			if (!(virtualCamera == null))
			{
				return virtualCamera.FollowTargetAsGroup;
			}
			return null;
		}
	}

	public Vector3 FollowTargetPosition
	{
		get
		{
			CinemachineVirtualCameraBase followTargetAsVcam = VirtualCamera.FollowTargetAsVcam;
			if (followTargetAsVcam != null)
			{
				return followTargetAsVcam.State.GetFinalPosition();
			}
			Transform followTarget = FollowTarget;
			if (followTarget != null)
			{
				return TargetPositionCache.GetTargetPosition(followTarget);
			}
			return Vector3.zero;
		}
	}

	public Quaternion FollowTargetRotation
	{
		get
		{
			CinemachineVirtualCameraBase followTargetAsVcam = VirtualCamera.FollowTargetAsVcam;
			if (followTargetAsVcam != null)
			{
				return followTargetAsVcam.State.GetFinalOrientation();
			}
			Transform followTarget = FollowTarget;
			if (followTarget != null)
			{
				return TargetPositionCache.GetTargetRotation(followTarget);
			}
			return Quaternion.identity;
		}
	}

	public ICinemachineTargetGroup LookAtTargetAsGroup => VirtualCamera.LookAtTargetAsGroup;

	public Vector3 LookAtTargetPosition
	{
		get
		{
			CinemachineVirtualCameraBase lookAtTargetAsVcam = VirtualCamera.LookAtTargetAsVcam;
			if (lookAtTargetAsVcam != null)
			{
				return lookAtTargetAsVcam.State.GetFinalPosition();
			}
			Transform lookAtTarget = LookAtTarget;
			if (lookAtTarget != null)
			{
				return TargetPositionCache.GetTargetPosition(lookAtTarget);
			}
			return Vector3.zero;
		}
	}

	public Quaternion LookAtTargetRotation
	{
		get
		{
			CinemachineVirtualCameraBase lookAtTargetAsVcam = VirtualCamera.LookAtTargetAsVcam;
			if (lookAtTargetAsVcam != null)
			{
				return lookAtTargetAsVcam.State.GetFinalOrientation();
			}
			Transform lookAtTarget = LookAtTarget;
			if (lookAtTarget != null)
			{
				return TargetPositionCache.GetTargetRotation(lookAtTarget);
			}
			return Quaternion.identity;
		}
	}

	public CameraState VcamState
	{
		get
		{
			CinemachineVirtualCameraBase virtualCamera = VirtualCamera;
			if (!(virtualCamera == null))
			{
				return virtualCamera.State;
			}
			return CameraState.Default;
		}
	}

	public abstract bool IsValid { get; }

	public abstract CinemachineCore.Stage Stage { get; }

	public virtual bool BodyAppliesAfterAim => false;

	internal virtual bool CameraLooksAtTarget => false;

	protected virtual void OnEnable()
	{
		CinemachineCamera cinemachineCamera = VirtualCamera as CinemachineCamera;
		if (cinemachineCamera != null)
		{
			cinemachineCamera.InvalidatePipelineCache();
		}
	}

	protected virtual void OnDisable()
	{
		CinemachineCamera cinemachineCamera = VirtualCamera as CinemachineCamera;
		if (cinemachineCamera != null)
		{
			cinemachineCamera.InvalidatePipelineCache();
		}
	}

	public virtual void PrePipelineMutateCameraState(ref CameraState curState, float deltaTime)
	{
	}

	public abstract void MutateCameraState(ref CameraState curState, float deltaTime);

	public virtual bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		return false;
	}

	public virtual void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
	}

	public virtual void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
	}

	public virtual float GetMaxDampTime()
	{
		return 0f;
	}
}
