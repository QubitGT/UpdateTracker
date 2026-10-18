using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class ProxyCameraRig
{
	private OVRCameraRig _cameraRig;

	public Camera Camera { get; private set; }

	public Transform CameraTransform { get; private set; }

	public bool Refresh()
	{
		if (Camera != null && Camera.isActiveAndEnabled)
		{
			return true;
		}
		SearchForCamera();
		return Camera;
	}

	private void SearchForCamera()
	{
		if (RuntimeSettings.Instance.UseCustomIntegrationConfig)
		{
			Camera = CustomIntegrationConfig.GetCamera();
			CameraTransform = Camera?.gameObject.transform;
			return;
		}
		if (_cameraRig == null)
		{
			_cameraRig = Object.FindAnyObjectByType<OVRCameraRig>();
		}
		if (_cameraRig != null)
		{
			Camera = _cameraRig.leftEyeCamera;
			CameraTransform = _cameraRig.leftEyeAnchor;
		}
	}
}
