using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

public abstract class CinemachineCameraManagerBase : CinemachineVirtualCameraBase, ICinemachineMixer, ICinemachineCamera
{
	[Serializable]
	public struct DefaultTargetSettings
	{
		[Tooltip("If enabled, a default target will be available.  It will be used if a child rig needs a target and doesn't specify one itself.")]
		public bool Enabled;

		[NoSaveDuringPlay]
		[Tooltip("Default target for the camera children, which may be used if the child rig does not specify a target of its own.")]
		public CameraTarget Target;
	}

	[FoldoutWithEnabledButton("Enabled")]
	public DefaultTargetSettings DefaultTarget;

	[Tooltip("The blend which is used if you don't explicitly define a blend between two Virtual Camera children")]
	[FormerlySerializedAs("m_DefaultBlend")]
	public CinemachineBlendDefinition DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.5f);

	[Tooltip("This is the asset which contains custom settings for specific child blends")]
	[FormerlySerializedAs("m_CustomBlends")]
	[EmbeddedBlenderSettingsProperty]
	public CinemachineBlenderSettings CustomBlends;

	private List<CinemachineVirtualCameraBase> m_ChildCameras;

	private int m_ChildCountCache;

	private readonly BlendManager m_BlendManager = new BlendManager();

	private CameraState m_State = CameraState.Default;

	private ICinemachineCamera m_TransitioningFrom;

	public override string Description => m_BlendManager.Description;

	public override CameraState State => m_State;

	public List<CinemachineVirtualCameraBase> ChildCameras
	{
		get
		{
			UpdateCameraCache();
			return m_ChildCameras;
		}
	}

	public override bool PreviousStateIsValid
	{
		get
		{
			return base.PreviousStateIsValid;
		}
		set
		{
			base.PreviousStateIsValid = value;
			if (!value)
			{
				int num = 0;
				while (m_ChildCameras != null && num < m_ChildCameras.Count)
				{
					m_ChildCameras[num].PreviousStateIsValid = value;
					num++;
				}
			}
		}
	}

	public bool IsBlending => m_BlendManager.IsBlending;

	public CinemachineBlend ActiveBlend
	{
		get
		{
			if (!PreviousStateIsValid)
			{
				return null;
			}
			return m_BlendManager.ActiveBlend;
		}
		set
		{
			m_BlendManager.ActiveBlend = value;
		}
	}

	public ICinemachineCamera LiveChild
	{
		get
		{
			if (!PreviousStateIsValid)
			{
				return null;
			}
			return m_BlendManager.ActiveVirtualCamera;
		}
	}

	public override Transform LookAt
	{
		get
		{
			if (!DefaultTarget.Enabled)
			{
				return null;
			}
			return ResolveLookAt(DefaultTarget.Target.CustomLookAtTarget ? DefaultTarget.Target.LookAtTarget : DefaultTarget.Target.TrackingTarget);
		}
		set
		{
			DefaultTarget.Enabled = true;
			DefaultTarget.Target.CustomLookAtTarget = true;
			DefaultTarget.Target.LookAtTarget = value;
		}
	}

	public override Transform Follow
	{
		get
		{
			if (!DefaultTarget.Enabled)
			{
				return null;
			}
			return ResolveFollow(DefaultTarget.Target.TrackingTarget);
		}
		set
		{
			DefaultTarget.Enabled = true;
			DefaultTarget.Target.TrackingTarget = value;
		}
	}

	protected virtual void Reset()
	{
		Priority = default(PrioritySettings);
		OutputChannel = OutputChannels.Default;
		DefaultTarget = default(DefaultTargetSettings);
		InvalidateCameraCache();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_BlendManager.OnEnable();
		m_BlendManager.LookupBlendDelegate = LookupBlend;
		InvalidateCameraCache();
	}

	protected override void OnDisable()
	{
		m_BlendManager.OnDisable();
		base.OnDisable();
	}

	public virtual bool IsLiveChild(ICinemachineCamera cam, bool dominantChildOnly = false)
	{
		return m_BlendManager.IsLive(cam);
	}

	public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		UpdateTargetCache();
		UpdateCameraCache();
		if (!PreviousStateIsValid)
		{
			ResetLiveChild();
		}
		CinemachineVirtualCameraBase cinemachineVirtualCameraBase = ChooseCurrentCamera(worldUp, deltaTime);
		if (cinemachineVirtualCameraBase != null && !cinemachineVirtualCameraBase.gameObject.activeInHierarchy)
		{
			cinemachineVirtualCameraBase.gameObject.SetActive(value: true);
			cinemachineVirtualCameraBase.UpdateCameraState(worldUp, deltaTime);
		}
		SetLiveChild(cinemachineVirtualCameraBase, worldUp, deltaTime);
		if (m_TransitioningFrom != null && !IsBlending && LiveChild != null)
		{
			LiveChild.OnCameraActivated(new ICinemachineCamera.ActivationEventParams
			{
				Origin = this,
				OutgoingCamera = m_TransitioningFrom,
				IncomingCamera = LiveChild,
				IsCut = false,
				WorldUp = worldUp,
				DeltaTime = deltaTime
			});
		}
		FinalizeCameraState(deltaTime);
		m_TransitioningFrom = null;
		PreviousStateIsValid = true;
	}

	protected virtual CinemachineBlendDefinition LookupBlend(ICinemachineCamera outgoing, ICinemachineCamera incoming)
	{
		return CinemachineBlenderSettings.LookupBlend(outgoing, incoming, DefaultBlend, CustomBlends, this);
	}

	protected abstract CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime);

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		UpdateCameraCache();
		for (int i = 0; i < m_ChildCameras.Count; i++)
		{
			m_ChildCameras[i].OnTargetObjectWarped(target, positionDelta);
		}
		base.OnTargetObjectWarped(target, positionDelta);
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		UpdateCameraCache();
		for (int i = 0; i < m_ChildCameras.Count; i++)
		{
			m_ChildCameras[i].ForceCameraPosition(pos, rot);
		}
		base.ForceCameraPosition(pos, rot);
	}

	public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
		m_TransitioningFrom = fromCam;
		InvokeOnTransitionInExtensions(fromCam, worldUp, deltaTime);
		InternalUpdateCameraState(worldUp, deltaTime);
	}

	public void InvalidateCameraCache()
	{
		m_ChildCameras = null;
		PreviousStateIsValid = false;
	}

	protected virtual bool UpdateCameraCache()
	{
		int childCount = base.transform.childCount;
		if (m_ChildCameras != null && m_ChildCountCache == childCount)
		{
			return false;
		}
		PreviousStateIsValid = false;
		m_ChildCameras = new List<CinemachineVirtualCameraBase>();
		m_ChildCountCache = childCount;
		GetComponentsInChildren(includeInactive: true, m_ChildCameras);
		for (int num = m_ChildCameras.Count - 1; num >= 0; num--)
		{
			if (m_ChildCameras[num].transform.parent != base.transform)
			{
				m_ChildCameras.RemoveAt(num);
			}
		}
		return true;
	}

	protected virtual void OnTransformChildrenChanged()
	{
		InvalidateCameraCache();
	}

	protected void SetLiveChild(ICinemachineCamera activeCamera, Vector3 worldUp, float deltaTime)
	{
		m_BlendManager.UpdateRootFrame(this, activeCamera, worldUp, deltaTime);
		m_BlendManager.ComputeCurrentBlend();
		m_BlendManager.ProcessActiveCamera(this, worldUp, deltaTime);
	}

	protected void ResetLiveChild()
	{
		m_BlendManager.ResetRootFrame();
	}

	protected void FinalizeCameraState(float deltaTime)
	{
		m_State = m_BlendManager.CameraState;
		InvokePostPipelineStageCallback(this, CinemachineCore.Stage.Finalize, ref m_State, deltaTime);
	}
}
