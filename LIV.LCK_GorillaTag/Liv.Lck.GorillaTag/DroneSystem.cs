using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class DroneSystem : MonoBehaviour
{
	public delegate void OnRequestDroneModeDelegate(bool isActive);

	[SerializeField]
	private GameObject _dronePrefab;

	private DroneController _droneController;

	[SerializeField]
	private GTLckController _gtController;

	[InjectLck]
	private ILckService _lckService;

	public event OnRequestDroneModeDelegate OnRequestDroneModeState;

	private void Awake()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			Object.Destroy(this);
			return;
		}
		_droneController = Object.Instantiate(_dronePrefab).GetComponent<DroneController>();
		_droneController.GetModel().OnIsDroneModeActive += ProcessDroneMode;
	}

	private void Start()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted += OnRecordingStarted;
		}
	}

	private void OnRecordingStarted(LckResult result)
	{
		if (result.Success)
		{
			_gtController.SetOrientationQualityAndTopButtonsIsDisabledState(state: true);
		}
	}

	private void ProcessDroneMode(bool value)
	{
		this.OnRequestDroneModeState?.Invoke(value);
	}

	internal void SetDronePositionAndRotation(Vector3 position, Quaternion rotation)
	{
		_droneController.SetDronePositionAndRotation(position, rotation);
	}

	internal ILckCamera GetLckCamera()
	{
		return _droneController.GetLckCamera();
	}

	private void OnDestroy()
	{
		if (_droneController != null)
		{
			Object.Destroy(_droneController.gameObject);
		}
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnRecordingStarted;
		}
	}
}
