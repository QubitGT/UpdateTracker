using UnityEngine;

namespace Unity.Cinemachine;

[SaveDuringPlay]
public abstract class CinemachineMixerEventsBase : MonoBehaviour
{
	[Space]
	[Tooltip("This event will fire whenever a virtual camera goes live.  If a blend is involved, then the event will fire on the first frame of the blend.")]
	public CinemachineCore.CameraEvent CameraActivatedEvent = new CinemachineCore.CameraEvent();

	[Tooltip("This event will fire whenever a virtual stops being live.  If a blend is involved, then the event will fire after the last frame of the blend.")]
	public CinemachineCore.CameraEvent CameraDeactivatedEvent = new CinemachineCore.CameraEvent();

	[Tooltip("This event will fire whenever a blend is created in the root frame of this Brain.  The handler can modify any settings in the blend, except the cameras themselves.  Note: timeline tracks will not generate these events.")]
	public CinemachineCore.BlendEvent BlendCreatedEvent = new CinemachineCore.BlendEvent();

	[Tooltip("This event will fire whenever a virtual camera finishes blending in.  It will not fire if the blend length is zero.")]
	public CinemachineCore.CameraEvent BlendFinishedEvent = new CinemachineCore.CameraEvent();

	[Tooltip("This event is fired when there is a camera cut.  A camera cut is a camera activation with a zero-length blend.")]
	public CinemachineCore.CameraEvent CameraCutEvent = new CinemachineCore.CameraEvent();

	protected abstract ICinemachineMixer GetMixer();

	protected void InstallHandlers(ICinemachineMixer mixer)
	{
		if (mixer != null)
		{
			CinemachineCore.CameraActivatedEvent.AddListener(OnCameraActivated);
			CinemachineCore.CameraDeactivatedEvent.AddListener(OnCameraDeactivated);
			CinemachineCore.BlendCreatedEvent.AddListener(OnBlendCreated);
			CinemachineCore.BlendFinishedEvent.AddListener(OnBlendFinished);
		}
	}

	protected void UninstallHandlers()
	{
		CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraActivated);
		CinemachineCore.CameraDeactivatedEvent.RemoveListener(OnCameraDeactivated);
		CinemachineCore.BlendCreatedEvent.RemoveListener(OnBlendCreated);
		CinemachineCore.BlendFinishedEvent.RemoveListener(OnBlendFinished);
	}

	private void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
	{
		ICinemachineMixer mixer = GetMixer();
		if (evt.Origin == mixer)
		{
			CameraActivatedEvent.Invoke(mixer, evt.IncomingCamera);
			if (evt.IsCut)
			{
				CameraCutEvent.Invoke(mixer, evt.IncomingCamera);
			}
		}
	}

	private void OnCameraDeactivated(ICinemachineMixer mixer, ICinemachineCamera cam)
	{
		if (mixer == GetMixer())
		{
			CameraDeactivatedEvent.Invoke(mixer, cam);
		}
	}

	private void OnBlendCreated(CinemachineCore.BlendEventParams evt)
	{
		if (evt.Origin == GetMixer())
		{
			BlendCreatedEvent.Invoke(evt);
		}
	}

	private void OnBlendFinished(ICinemachineMixer mixer, ICinemachineCamera cam)
	{
		if (mixer == GetMixer())
		{
			BlendFinishedEvent.Invoke(mixer, cam);
		}
	}
}
