using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Liv.Lck;

[RequireComponent(typeof(Camera))]
public class LckCamera : MonoBehaviour, ILckCamera
{
	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private string _cameraId;

	public string CameraId => _cameraId;

	private void Awake()
	{
		if (string.IsNullOrEmpty(_cameraId))
		{
			_cameraId = Guid.NewGuid().ToString();
		}
		_camera.enabled = false;
		LckMediator.RegisterCamera(this);
		LckLog.Log("Configuring URP camera data for camera: " + _cameraId, "Awake", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCamera.cs", 31);
		if (!_camera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
		{
			component = _camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
		}
		component.allowXRRendering = false;
	}

	private void OnDestroy()
	{
		LckMediator.UnregisterCamera(this);
	}

	public void ActivateCamera(RenderTexture renderTexture)
	{
		_camera.enabled = true;
		_camera.targetTexture = renderTexture;
	}

	public void DeactivateCamera()
	{
		_camera.enabled = false;
		_camera.targetTexture = null;
	}

	public Camera GetCameraComponent()
	{
		return _camera;
	}
}
