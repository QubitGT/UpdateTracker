using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class DroneCamera
{
	private Camera _camera;

	private float _targetFov;

	private float _smoothness;

	public DroneCamera(Camera camera)
	{
		_camera = camera;
	}

	public void SetFov(float fov)
	{
		_targetFov = fov;
	}

	public void SetSmoothness(float smoothness)
	{
		_smoothness = smoothness;
	}

	public void Run()
	{
		if (!Mathf.Approximately(_camera.fieldOfView, _targetFov))
		{
			_camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFov, Time.deltaTime / _smoothness);
		}
	}
}
