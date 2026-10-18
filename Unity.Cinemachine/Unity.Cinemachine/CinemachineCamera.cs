using System;
using UnityEngine;

namespace Unity.Cinemachine;

[DisallowMultipleComponent]
[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Cinemachine Camera")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCamera.html")]
public sealed class CinemachineCamera : CinemachineVirtualCameraBase
{
	[NoSaveDuringPlay]
	[Tooltip("Specifies the Tracking and LookAt targets for this camera.")]
	public CameraTarget Target;

	[Tooltip("Specifies the lens properties of this Virtual Camera.  This generally mirrors the Unity Camera's lens settings, and will be used to drive the Unity camera when the vcam is active.")]
	public LensSettings Lens = LensSettings.Default;

	[Tooltip("Hint for transitioning to and from this CinemachineCamera.  Hints can be combined, although not all combinations make sense.  In the case of conflicting hints, Cinemachine will make an arbitrary choice.")]
	public CinemachineCore.BlendHints BlendHint;

	private CameraState m_State = CameraState.Default;

	private CinemachineComponentBase[] m_Pipeline;

	public override CameraState State => m_State;

	public override Transform LookAt
	{
		get
		{
			return ResolveLookAt(Target.CustomLookAtTarget ? Target.LookAtTarget : Target.TrackingTarget);
		}
		set
		{
			Target.CustomLookAtTarget = true;
			Target.LookAtTarget = value;
		}
	}

	public override Transform Follow
	{
		get
		{
			return ResolveFollow(Target.TrackingTarget);
		}
		set
		{
			Target.TrackingTarget = value;
		}
	}

	internal bool PipelineCacheInvalidated => m_Pipeline == null;

	private void Reset()
	{
		Priority = default(PrioritySettings);
		OutputChannel = OutputChannels.Default;
		Target = default(CameraTarget);
		Lens = LensSettings.Default;
	}

	private void OnValidate()
	{
		Lens.Validate();
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		if (target == Follow)
		{
			base.transform.position += positionDelta;
			m_State.RawPosition += positionDelta;
		}
		UpdatePipelineCache();
		for (int i = 0; i < m_Pipeline.Length; i++)
		{
			if (m_Pipeline[i] != null)
			{
				m_Pipeline[i].OnTargetObjectWarped(target, positionDelta);
			}
		}
		base.OnTargetObjectWarped(target, positionDelta);
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		PreviousStateIsValid = false;
		UpdatePipelineCache();
		for (int i = 0; i < m_Pipeline.Length; i++)
		{
			if (m_Pipeline[i] != null)
			{
				m_Pipeline[i].ForceCameraPosition(pos, rot);
			}
		}
		m_State.RawPosition = pos;
		m_State.RawOrientation = rot;
		base.ForceCameraPosition(pos, rot);
	}

	public override float GetMaxDampTime()
	{
		float num = base.GetMaxDampTime();
		UpdatePipelineCache();
		for (int i = 0; i < m_Pipeline.Length; i++)
		{
			if (m_Pipeline[i] != null)
			{
				num = Mathf.Max(num, m_Pipeline[i].GetMaxDampTime());
			}
		}
		return num;
	}

	public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
		InvokeOnTransitionInExtensions(fromCam, worldUp, deltaTime);
		bool flag = false;
		if ((State.BlendHint & CameraState.BlendHints.InheritPosition) != 0 && fromCam != null && !CinemachineCore.IsLiveInBlend(this))
		{
			CameraState state = fromCam.State;
			ForceCameraPosition(state.GetFinalPosition(), state.GetFinalOrientation());
		}
		UpdatePipelineCache();
		for (int i = 0; i < m_Pipeline.Length; i++)
		{
			if (m_Pipeline[i] != null && m_Pipeline[i].OnTransitionFromCamera(fromCam, worldUp, deltaTime))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			UpdateCameraState(worldUp, deltaTime);
			return;
		}
		InternalUpdateCameraState(worldUp, deltaTime);
		InternalUpdateCameraState(worldUp, deltaTime);
	}

	public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		UpdateTargetCache();
		FollowTargetAttachment = 1f;
		LookAtTargetAttachment = 1f;
		if (deltaTime < 0f)
		{
			PreviousStateIsValid = false;
		}
		m_State = PullStateFromVirtualCamera(worldUp, ref Lens);
		Transform lookAt = LookAt;
		if (lookAt != null)
		{
			m_State.ReferenceLookAt = ((base.LookAtTargetAsVcam != null) ? base.LookAtTargetAsVcam.State.GetFinalPosition() : TargetPositionCache.GetTargetPosition(lookAt));
		}
		m_State.BlendHint = (CameraState.BlendHints)BlendHint;
		InvokeComponentPipeline(ref m_State, deltaTime);
		base.transform.ConservativeSetPositionAndRotation(m_State.RawPosition, m_State.RawOrientation);
		PreviousStateIsValid = true;
	}

	private CameraState InvokeComponentPipeline(ref CameraState state, float deltaTime)
	{
		InvokePrePipelineMutateCameraStateCallback(this, ref state, deltaTime);
		UpdatePipelineCache();
		for (int i = 0; i < m_Pipeline.Length; i++)
		{
			CinemachineComponentBase cinemachineComponentBase = m_Pipeline[i];
			if (cinemachineComponentBase != null && cinemachineComponentBase.IsValid)
			{
				cinemachineComponentBase.PrePipelineMutateCameraState(ref state, deltaTime);
			}
		}
		CinemachineComponentBase cinemachineComponentBase2 = null;
		for (int j = 0; j < m_Pipeline.Length; j++)
		{
			CinemachineCore.Stage stage = (CinemachineCore.Stage)j;
			CinemachineComponentBase cinemachineComponentBase3 = m_Pipeline[j];
			if (cinemachineComponentBase3 != null && cinemachineComponentBase3.IsValid)
			{
				if (stage == CinemachineCore.Stage.Body && cinemachineComponentBase3.BodyAppliesAfterAim)
				{
					cinemachineComponentBase2 = cinemachineComponentBase3;
					continue;
				}
				cinemachineComponentBase3.MutateCameraState(ref state, deltaTime);
			}
			InvokePostPipelineStageCallback(this, stage, ref state, deltaTime);
			if (stage == CinemachineCore.Stage.Aim && cinemachineComponentBase2 != null)
			{
				cinemachineComponentBase2.MutateCameraState(ref state, deltaTime);
				InvokePostPipelineStageCallback(this, CinemachineCore.Stage.Body, ref state, deltaTime);
			}
		}
		return state;
	}

	internal void InvalidatePipelineCache()
	{
		m_Pipeline = null;
	}

	internal Type PeekPipelineCacheType(CinemachineCore.Stage stage)
	{
		if (!(m_Pipeline[(int)stage] == null))
		{
			return m_Pipeline[(int)stage].GetType();
		}
		return null;
	}

	private void UpdatePipelineCache()
	{
		if (m_Pipeline != null && m_Pipeline.Length == 4)
		{
			return;
		}
		m_Pipeline = new CinemachineComponentBase[4];
		CinemachineComponentBase[] components = GetComponents<CinemachineComponentBase>();
		for (int i = 0; i < components.Length; i++)
		{
			if (m_Pipeline[(int)components[i].Stage] == null)
			{
				m_Pipeline[(int)components[i].Stage] = components[i];
			}
		}
	}

	public override CinemachineComponentBase GetCinemachineComponent(CinemachineCore.Stage stage)
	{
		UpdatePipelineCache();
		if (stage < CinemachineCore.Stage.Body || (int)stage >= m_Pipeline.Length)
		{
			return null;
		}
		return m_Pipeline[(int)stage];
	}
}
