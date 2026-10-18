using UnityEngine;

namespace Liv.Lck.Tablet;

public class LCKCameraModeHelper : MonoBehaviour
{
	[SerializeField]
	private CameraMode _cameraMode;

	[SerializeField]
	private LCKSettingsButtonsController _settingsButtonsController;

	public void SetCameraMode(bool isSelected)
	{
		if (isSelected)
		{
			_settingsButtonsController.SwitchCameraModes(_cameraMode);
		}
	}
}
