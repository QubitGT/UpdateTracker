using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Helpers/Cinemachine Brain Events")]
[SaveDuringPlay]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBrainEvents.html")]
public class CinemachineBrainEvents : CinemachineMixerEventsBase
{
	[Tooltip("This is the CinemachineBrain emitting the events.  If null and the current GameObject has a CinemachineBrain component, that component will be used.")]
	public CinemachineBrain Brain;

	[Tooltip("This event will fire after the brain updates its Camera.")]
	public CinemachineCore.BrainEvent BrainUpdatedEvent = new CinemachineCore.BrainEvent();

	protected override ICinemachineMixer GetMixer()
	{
		return Brain;
	}

	private void OnEnable()
	{
		if (Brain == null)
		{
			TryGetComponent<CinemachineBrain>(out Brain);
		}
		if (Brain != null)
		{
			InstallHandlers(Brain);
			CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
		}
	}

	private void OnDisable()
	{
		UninstallHandlers();
		CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
	}

	private void OnCameraUpdated(CinemachineBrain brain)
	{
		if (brain == Brain)
		{
			BrainUpdatedEvent.Invoke(brain);
		}
	}
}
