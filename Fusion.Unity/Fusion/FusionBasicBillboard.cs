using UnityEngine;

namespace Fusion;

[ScriptHelp(BackColor = ScriptHeaderBackColor.Olive)]
[ExecuteAlways]
public class FusionBasicBillboard : Behaviour
{
	[InlineHelp]
	public Camera Camera;

	private static float _lastCameraFindTime;

	private static Camera _currentCam;

	private Camera MainCamera
	{
		get
		{
			float time = Time.time;
			if (time == _lastCameraFindTime)
			{
				return _currentCam;
			}
			_lastCameraFindTime = time;
			return _currentCam = Camera.main;
		}
		set
		{
			_currentCam = value;
		}
	}

	private void OnEnable()
	{
		UpdateLookAt();
	}

	private void OnDisable()
	{
		base.transform.localRotation = default(Quaternion);
	}

	private void LateUpdate()
	{
		UpdateLookAt();
	}

	public void UpdateLookAt()
	{
		Camera camera = (Camera ? Camera : MainCamera);
		if ((bool)camera && base.enabled)
		{
			base.transform.rotation = camera.transform.rotation;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatics()
	{
		_currentCam = null;
		_lastCameraFindTime = 0f;
	}
}
