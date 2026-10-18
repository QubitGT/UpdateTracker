using UnityEngine;

namespace Liv.Lck;

public interface ILckCamera
{
	string CameraId { get; }

	void ActivateCamera(RenderTexture renderTexture);

	void DeactivateCamera();

	Camera GetCameraComponent();
}
