using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Helpers/Cinemachine Camera Manager Events")]
[SaveDuringPlay]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCameraManagerEvents.html")]
public class CinemachineCameraManagerEvents : CinemachineMixerEventsBase
{
	[Tooltip("This is the CinemachineCameraManager emitting the events.  If null and the current GameObject has a CinemachineCameraManager component, that component will be used.")]
	public CinemachineCameraManagerBase CameraManager;

	protected override ICinemachineMixer GetMixer()
	{
		return CameraManager;
	}

	private void OnEnable()
	{
		if (CameraManager == null)
		{
			TryGetComponent<CinemachineCameraManagerBase>(out CameraManager);
		}
		InstallHandlers(CameraManager);
	}

	private void OnDisable()
	{
		UninstallHandlers();
	}
}
