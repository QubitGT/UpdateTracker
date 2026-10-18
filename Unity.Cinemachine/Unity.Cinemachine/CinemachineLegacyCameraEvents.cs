using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Cinemachine;

[Obsolete("Please use CinemachineCameraEvents instead.")]
[AddComponentMenu("")]
public class CinemachineLegacyCameraEvents : MonoBehaviour
{
	[Serializable]
	public class OnCameraLiveEvent : UnityEvent<ICinemachineCamera, ICinemachineCamera>
	{
	}

	[Tooltip("This event fires when the CinemachineCamera goes Live")]
	public OnCameraLiveEvent OnCameraLive = new OnCameraLiveEvent();

	private CinemachineVirtualCameraBase m_Vcam;

	private void OnEnable()
	{
		TryGetComponent<CinemachineVirtualCameraBase>(out m_Vcam);
		if (m_Vcam != null)
		{
			CinemachineCore.CameraActivatedEvent.AddListener(OnCameraActivated);
		}
	}

	private void OnDisable()
	{
		CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraActivated);
	}

	private void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
	{
		if (evt.IncomingCamera == m_Vcam)
		{
			OnCameraLive.Invoke(evt.IncomingCamera, evt.OutgoingCamera);
		}
	}
}
