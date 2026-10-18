using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Helpers/Cinemachine Camera Events")]
[SaveDuringPlay]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCameraEvents.html")]
public class CinemachineCameraEvents : MonoBehaviour
{
	[Tooltip("This is the object whose events are being monitored.  If null and the current GameObject has a CinemachineVirtualCameraBase component, that component will be used.")]
	public CinemachineVirtualCameraBase EventTarget;

	[Space]
	[Tooltip("This event will fire whenever a virtual camera becomes active in the context of a mixer.  If a blend is involved, then the event will fire on the first frame of the blend.")]
	public CinemachineCore.CameraEvent CameraActivatedEvent = new CinemachineCore.CameraEvent();

	[Tooltip("This event will fire whenever a virtual stops being live.  If a blend is involved, then the event will fire after the last frame of the blend.")]
	public CinemachineCore.CameraEvent CameraDeactivatedEvent = new CinemachineCore.CameraEvent();

	[Tooltip("This event will fire whenever a blend is created that involves this camera.  The handler can modify any settings in the blend, except the cameras themselves.")]
	public CinemachineCore.BlendEvent BlendCreatedEvent = new CinemachineCore.BlendEvent();

	[Tooltip("This event will fire whenever a virtual camera finishes blending in.  It will not fire if the blend length is zero.")]
	public CinemachineCore.CameraEvent BlendFinishedEvent = new CinemachineCore.CameraEvent();

	private void OnEnable()
	{
		if (EventTarget == null)
		{
			TryGetComponent<CinemachineVirtualCameraBase>(out EventTarget);
		}
		if (EventTarget != null)
		{
			CinemachineCore.CameraActivatedEvent.AddListener(OnCameraActivated);
			CinemachineCore.CameraDeactivatedEvent.AddListener(OnCameraDeactivated);
			CinemachineCore.BlendCreatedEvent.AddListener(OnBlendCreated);
			CinemachineCore.BlendFinishedEvent.AddListener(OnBlendFinished);
		}
	}

	private void OnDisable()
	{
		CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraActivated);
		CinemachineCore.CameraDeactivatedEvent.RemoveListener(OnCameraDeactivated);
		CinemachineCore.BlendCreatedEvent.RemoveListener(OnBlendCreated);
		CinemachineCore.BlendFinishedEvent.RemoveListener(OnBlendFinished);
	}

	private void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
	{
		if (evt.IncomingCamera == EventTarget)
		{
			CameraActivatedEvent.Invoke(evt.Origin, evt.IncomingCamera);
		}
	}

	private void OnBlendCreated(CinemachineCore.BlendEventParams evt)
	{
		if (evt.Blend.CamB == EventTarget)
		{
			BlendCreatedEvent.Invoke(evt);
		}
	}

	private void OnBlendFinished(ICinemachineMixer mixer, ICinemachineCamera cam)
	{
		if (cam == EventTarget)
		{
			BlendFinishedEvent.Invoke(mixer, cam);
		}
	}

	private void OnCameraDeactivated(ICinemachineMixer mixer, ICinemachineCamera cam)
	{
		if (cam == EventTarget)
		{
			CameraDeactivatedEvent.Invoke(mixer, cam);
		}
	}
}
