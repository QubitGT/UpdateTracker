using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unity.Cinemachine;

internal class BlendManager : CameraBlendStack
{
	private CinemachineBlend m_CurrentLiveCameras = new CinemachineBlend();

	private HashSet<ICinemachineCamera> m_PreviousLiveCameras = new HashSet<ICinemachineCamera>();

	private ICinemachineCamera m_PreviousActiveCamera;

	private bool m_WasBlending;

	public ICinemachineCamera ActiveVirtualCamera => DeepCamBFromBlend(m_CurrentLiveCameras);

	public CinemachineBlend ActiveBlend
	{
		get
		{
			if (m_CurrentLiveCameras.CamA == null || m_CurrentLiveCameras.IsComplete)
			{
				return null;
			}
			return m_CurrentLiveCameras;
		}
		set
		{
			SetRootBlend(value);
		}
	}

	public bool IsBlending => ActiveBlend != null;

	public string Description
	{
		get
		{
			if (ActiveVirtualCamera == null)
			{
				return "[(none)]";
			}
			StringBuilder stringBuilder = CinemachineDebug.SBFromPool();
			stringBuilder.Append("[");
			stringBuilder.Append(IsBlending ? ActiveBlend.Description : ActiveVirtualCamera.Name);
			stringBuilder.Append("]");
			string result = stringBuilder.ToString();
			CinemachineDebug.ReturnToPool(stringBuilder);
			return result;
		}
	}

	public CameraState CameraState => m_CurrentLiveCameras.State;

	public override void OnEnable()
	{
		base.OnEnable();
		m_PreviousLiveCameras.Clear();
		m_PreviousActiveCamera = null;
		m_WasBlending = false;
	}

	private static ICinemachineCamera DeepCamBFromBlend(CinemachineBlend blend)
	{
		ICinemachineCamera cinemachineCamera;
		for (cinemachineCamera = blend?.CamB; cinemachineCamera is NestedBlendSource nestedBlendSource; cinemachineCamera = nestedBlendSource.Blend.CamB)
		{
		}
		if (cinemachineCamera != null && cinemachineCamera.IsValid)
		{
			return cinemachineCamera;
		}
		return null;
	}

	public bool IsLiveInBlend(ICinemachineCamera cam)
	{
		if (cam != null)
		{
			if (cam == m_CurrentLiveCameras.CamA)
			{
				return true;
			}
			if (m_CurrentLiveCameras.CamA is NestedBlendSource nestedBlendSource && nestedBlendSource.Blend.Uses(cam))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsLive(ICinemachineCamera cam)
	{
		return m_CurrentLiveCameras.Uses(cam);
	}

	public void ComputeCurrentBlend()
	{
		ProcessOverrideFrames(ref m_CurrentLiveCameras, 0);
	}

	public void RefreshCurrentCameraState(Vector3 up, float deltaTime)
	{
		m_CurrentLiveCameras.UpdateCameraState(up, deltaTime);
	}

	public ICinemachineCamera ProcessActiveCamera(ICinemachineMixer mixer, Vector3 up, float deltaTime)
	{
		foreach (ICinemachineCamera previousLiveCamera in m_PreviousLiveCameras)
		{
			if (!IsLive(previousLiveCamera))
			{
				CinemachineCore.CameraDeactivatedEvent.Invoke(mixer, previousLiveCamera);
			}
		}
		ICinemachineCamera activeVirtualCamera = ActiveVirtualCamera;
		if (activeVirtualCamera != null && activeVirtualCamera.IsValid)
		{
			ICinemachineCamera cinemachineCamera = m_PreviousActiveCamera;
			if (cinemachineCamera != null && !cinemachineCamera.IsValid)
			{
				cinemachineCamera = null;
			}
			if (activeVirtualCamera == cinemachineCamera)
			{
				if (m_WasBlending && m_CurrentLiveCameras.CamA == null)
				{
					CinemachineCore.BlendFinishedEvent.Invoke(mixer, activeVirtualCamera);
				}
			}
			else
			{
				ICinemachineCamera cinemachineCamera2 = cinemachineCamera;
				if (IsBlending)
				{
					cinemachineCamera2 = new NestedBlendSource(ActiveBlend);
					cinemachineCamera2.UpdateCameraState(up, deltaTime);
				}
				ICinemachineCamera.ActivationEventParams activationEventParams = default(ICinemachineCamera.ActivationEventParams);
				activationEventParams.Origin = mixer;
				activationEventParams.OutgoingCamera = cinemachineCamera2;
				activationEventParams.IncomingCamera = activeVirtualCamera;
				activationEventParams.IsCut = !IsBlending;
				activationEventParams.WorldUp = up;
				activationEventParams.DeltaTime = deltaTime;
				ICinemachineCamera.ActivationEventParams activationEventParams2 = activationEventParams;
				activeVirtualCamera.OnCameraActivated(activationEventParams2);
				mixer.OnCameraActivated(activationEventParams2);
				CinemachineCore.CameraActivatedEvent.Invoke(activationEventParams2);
				activeVirtualCamera.UpdateCameraState(up, deltaTime);
			}
		}
		m_PreviousLiveCameras.Clear();
		CollectLiveCameras(m_CurrentLiveCameras, ref m_PreviousLiveCameras);
		m_PreviousActiveCamera = DeepCamBFromBlend(m_CurrentLiveCameras);
		m_WasBlending = m_CurrentLiveCameras.CamA != null;
		return activeVirtualCamera;
		static void CollectLiveCameras(CinemachineBlend blend, ref HashSet<ICinemachineCamera> cams)
		{
			if (blend.CamA is NestedBlendSource { Blend: not null } nestedBlendSource)
			{
				CollectLiveCameras(nestedBlendSource.Blend, ref cams);
			}
			else if (blend.CamA != null)
			{
				cams.Add(blend.CamA);
			}
			if (blend.CamB is NestedBlendSource { Blend: not null } nestedBlendSource2)
			{
				CollectLiveCameras(nestedBlendSource2.Blend, ref cams);
			}
			else if (blend.CamB != null)
			{
				cams.Add(blend.CamB);
			}
		}
	}
}
