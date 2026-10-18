using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

public abstract class CinemachineExtension : MonoBehaviour
{
	protected class VcamExtraStateBase
	{
		public CinemachineVirtualCameraBase Vcam;
	}

	private CinemachineVirtualCameraBase m_VcamOwner;

	private Dictionary<CinemachineVirtualCameraBase, VcamExtraStateBase> m_ExtraState;

	protected const float Epsilon = 0.0001f;

	public CinemachineVirtualCameraBase ComponentOwner
	{
		get
		{
			if (m_VcamOwner == null)
			{
				TryGetComponent<CinemachineVirtualCameraBase>(out m_VcamOwner);
			}
			return m_VcamOwner;
		}
	}

	protected virtual void Awake()
	{
		ConnectToVcam(connect: true);
	}

	protected virtual void OnDestroy()
	{
		ConnectToVcam(connect: false);
	}

	protected virtual void OnEnable()
	{
	}

	internal void EnsureStarted()
	{
		ConnectToVcam(connect: true);
	}

	protected virtual void ConnectToVcam(bool connect)
	{
		if (ComponentOwner != null)
		{
			if (connect)
			{
				ComponentOwner.AddExtension(this);
			}
			else
			{
				ComponentOwner.RemoveExtension(this);
			}
		}
		m_ExtraState = null;
	}

	public virtual void PrePipelineMutateCameraStateCallback(CinemachineVirtualCameraBase vcam, ref CameraState curState, float deltaTime)
	{
	}

	public void InvokePostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		PostPipelineStageCallback(vcam, stage, ref state, deltaTime);
	}

	protected virtual void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
	}

	public virtual void OnTargetObjectWarped(CinemachineVirtualCameraBase vcam, Transform target, Vector3 positionDelta)
	{
	}

	public virtual void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
	}

	public virtual void ForceCameraPosition(CinemachineVirtualCameraBase vcam, Vector3 pos, Quaternion rot)
	{
	}

	public virtual bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		return false;
	}

	public virtual float GetMaxDampTime()
	{
		return 0f;
	}

	protected T GetExtraState<T>(CinemachineVirtualCameraBase vcam) where T : VcamExtraStateBase, new()
	{
		if (m_ExtraState == null)
		{
			m_ExtraState = new Dictionary<CinemachineVirtualCameraBase, VcamExtraStateBase>();
		}
		if (!m_ExtraState.TryGetValue(vcam, out var value))
		{
			VcamExtraStateBase vcamExtraStateBase2 = (m_ExtraState[vcam] = new T
			{
				Vcam = vcam
			});
			value = vcamExtraStateBase2;
		}
		return value as T;
	}

	protected void GetAllExtraStates<T>(List<T> list) where T : VcamExtraStateBase, new()
	{
		list.Clear();
		if (m_ExtraState != null)
		{
			Dictionary<CinemachineVirtualCameraBase, VcamExtraStateBase>.Enumerator enumerator = m_ExtraState.GetEnumerator();
			while (enumerator.MoveNext())
			{
				list.Add(enumerator.Current.Value as T);
			}
		}
	}
}
