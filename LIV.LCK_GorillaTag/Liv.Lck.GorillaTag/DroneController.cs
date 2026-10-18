using System;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Recorder;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class DroneController : MonoBehaviour
{
	[Header("UI Style")]
	[SerializeField]
	private GUISkin _skin;

	[Header("Drone Parts")]
	[SerializeField]
	private Transform _droneTransform;

	[SerializeField]
	private Transform _gimbalTransform;

	[Header("Cameras")]
	[SerializeField]
	private LckCamera _lckCamera;

	private DroneDataModel _model;

	private DroneGeneralKeyboard _droneGeneralKeyboard;

	private DroneKeyboard _droneKeyboard;

	private DroneMouse _droneMouse;

	private DroneGamepad _droneGamepad;

	private DroneMovement _droneMovement;

	private DroneCamera _droneCamera;

	private DroneGUI _droneGUI;

	[InjectLck]
	private ILckService _lckService;

	private void Awake()
	{
		_model = new DroneDataModel(isDroneModeActive: false, useKeyboard: true, useMouse: false, useGamepad: false, 3f, 10f, 0.1f, 1f, 1f, 10f, 0.1f, 1f, 30f, 90f, 1f, 10f, 1f, 10f, 0.1f, 1f, 90f, 1f, 120f, 1f, 10f, 5f, 10f, 0.1f, 1f, snapAxis: false, useTiltAsDirection: true, isMouseInverted: false, showGUI: false);
		_droneGeneralKeyboard = new DroneGeneralKeyboard();
		_droneKeyboard = new DroneKeyboard();
		_droneMouse = new DroneMouse();
		_droneGamepad = new DroneGamepad();
		_droneMovement = new DroneMovement(_droneTransform, _gimbalTransform);
		_droneCamera = new DroneCamera(_lckCamera.GetCameraComponent());
		_droneGUI = new DroneGUI(_model, _skin);
		_droneMovement.SetMoveSpeedChanged(_model.MoveSpeed);
		_droneMovement.SetMoveSmoothness(_model.MoveSmoothness);
		_droneMovement.SetRotationSpeed(_model.RotationSpeed);
		_droneMovement.SetRotationSmoothness(_model.RotationSmoothness);
		_droneCamera.SetFov(_model.Fov);
		_droneCamera.SetSmoothness(_model.FovSmoothness);
		_droneMovement.SetSnapAxis(_model.SnapAxis);
		_droneMovement.SetUseTiltAsDirection(_model.UseTiltAsDirection);
		_droneMovement.SetIsMouseInverted(_model.IsMouseInverted);
	}

	private void OnEnable()
	{
		_droneKeyboard.OnMoveForward += _droneMovement.MoveForward;
		_droneKeyboard.OnMoveBackward += _droneMovement.MoveBackward;
		_droneKeyboard.OnMoveLeft += _droneMovement.MoveLeft;
		_droneKeyboard.OnMoveRight += _droneMovement.MoveRight;
		_droneKeyboard.OnMoveUp += _droneMovement.MoveUp;
		_droneKeyboard.OnMoveDown += _droneMovement.MoveDown;
		_droneKeyboard.OnRotateLeft += _droneMovement.RotateLeft;
		_droneKeyboard.OnRotateRight += _droneMovement.RotateRight;
		_droneKeyboard.OnTiltUp += _droneMovement.TiltUp;
		_droneKeyboard.OnTiltDown += _droneMovement.TiltDown;
		_droneKeyboard.OnBurstStarted += _model.BurstStarted;
		_droneKeyboard.OnBurstEnded += _model.BurstEnded;
		_droneGeneralKeyboard.OnShiftPressed += _model.MinimizeStepping;
		_droneGeneralKeyboard.OnShiftReleased += _model.MaximizeStepping;
		_droneGeneralKeyboard.OnShowUI += _model.ToggleShowGUI;
		_droneMouse.OnMouseMoveLeft += _droneMovement.TiltAndRotateMouse;
		_droneMouse.OnMouseMoveRight += _droneMovement.Roll;
		_droneMouse.OnReset += _droneMovement.ResetTillAndRoll;
		_droneMouse.OnMouseScrollUp += _model.IncreaseFov;
		_droneMouse.OnMouseScrollDown += _model.DecreaseFov;
		_droneGamepad.OnMove += _droneMovement.MoveForwardBackwardLeftRight;
		_droneGamepad.OnTiltAndRotate += _droneMovement.TiltAndRotateGamePad;
		_droneGamepad.OnMoveUpAndDown += _droneMovement.MoveUpAndDown;
		_model.OnIsDroneModeActive += ProcessDroneActiveState;
		_model.OnMoveSpeedChanged += _droneMovement.SetMoveSpeedChanged;
		_model.OnMoveSmoothnessChanged += SetProcessedMovementSmoothness;
		_model.OnRotationSpeedChanged += _droneMovement.SetRotationSpeed;
		_model.OnRotationSmoothnessChanged += SetProcessedRotationSmoothness;
		_model.OnFovChanged += _droneCamera.SetFov;
		_model.OnFovSmoothnessChanged += SetProcessedFovSmoothness;
		_model.OnSnapAxis += _droneMovement.SetSnapAxis;
		_model.OnUseTiltAsDirection += _droneMovement.SetUseTiltAsDirection;
		_model.OnIsMouseInverted += _droneMovement.SetIsMouseInverted;
		_model.OnRecordButtonPressed += ProcessRecordButtonBeingPressed;
		_model.DroneRecordingStateData.OnDroneRecordingStateChanged += _droneGUI.SetRecordButtonState;
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted += OnRecordingStarted;
			_lckService.OnRecordingStopped += OnRecordingStopped;
			_lckService.OnRecordingSaved += OnRecordingSaved;
		}
		else
		{
			Debug.LogError("Unable to get LckService on DroneController");
		}
	}

	private void OnDisable()
	{
		_droneKeyboard.OnMoveForward -= _droneMovement.MoveForward;
		_droneKeyboard.OnMoveBackward -= _droneMovement.MoveBackward;
		_droneKeyboard.OnMoveLeft -= _droneMovement.MoveLeft;
		_droneKeyboard.OnMoveRight -= _droneMovement.MoveRight;
		_droneKeyboard.OnMoveUp -= _droneMovement.MoveUp;
		_droneKeyboard.OnMoveDown -= _droneMovement.MoveDown;
		_droneKeyboard.OnRotateLeft -= _droneMovement.RotateLeft;
		_droneKeyboard.OnRotateRight -= _droneMovement.RotateRight;
		_droneKeyboard.OnTiltUp -= _droneMovement.TiltUp;
		_droneKeyboard.OnTiltDown -= _droneMovement.TiltDown;
		_droneKeyboard.OnBurstStarted -= _model.BurstStarted;
		_droneKeyboard.OnBurstEnded -= _model.BurstEnded;
		_droneGeneralKeyboard.OnShiftPressed -= _model.MinimizeStepping;
		_droneGeneralKeyboard.OnShiftReleased -= _model.MaximizeStepping;
		_droneGeneralKeyboard.OnShowUI -= _model.ToggleShowGUI;
		_droneMouse.OnMouseMoveLeft -= _droneMovement.TiltAndRotateMouse;
		_droneMouse.OnMouseMoveRight -= _droneMovement.Roll;
		_droneMouse.OnReset -= _droneMovement.ResetTillAndRoll;
		_droneMouse.OnMouseScrollUp -= _model.IncreaseFov;
		_droneMouse.OnMouseScrollDown -= _model.DecreaseFov;
		_droneGamepad.OnMove -= _droneMovement.MoveForwardBackwardLeftRight;
		_droneGamepad.OnTiltAndRotate -= _droneMovement.TiltAndRotateGamePad;
		_droneGamepad.OnMoveUpAndDown -= _droneMovement.MoveUpAndDown;
		_model.OnIsDroneModeActive -= ProcessDroneActiveState;
		_model.OnMoveSpeedChanged -= _droneMovement.SetMoveSpeedChanged;
		_model.OnMoveSmoothnessChanged -= SetProcessedMovementSmoothness;
		_model.OnRotationSpeedChanged -= _droneMovement.SetRotationSpeed;
		_model.OnRotationSmoothnessChanged -= SetProcessedRotationSmoothness;
		_model.OnFovChanged -= _droneCamera.SetFov;
		_model.OnFovSmoothnessChanged -= SetProcessedFovSmoothness;
		_model.OnSnapAxis -= _droneMovement.SetSnapAxis;
		_model.OnUseTiltAsDirection -= _droneMovement.SetUseTiltAsDirection;
		_model.OnIsMouseInverted -= _droneMovement.SetIsMouseInverted;
		_model.OnRecordButtonPressed -= ProcessRecordButtonBeingPressed;
		_model.DroneRecordingStateData.OnDroneRecordingStateChanged -= _droneGUI.SetRecordButtonState;
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnRecordingStarted;
			_lckService.OnRecordingStopped -= OnRecordingStopped;
			_lckService.OnRecordingSaved -= OnRecordingSaved;
		}
	}

	private void OnGUI()
	{
		if (_model.ShowGUI)
		{
			_droneGUI.Run();
		}
	}

	private void Update()
	{
		_droneGeneralKeyboard.Run();
		if (_model.IsDroneModeActive)
		{
			if (_model.UseKeyboard)
			{
				_droneKeyboard.Run();
			}
			if (_model.UseMouse)
			{
				_droneMouse.Run();
			}
			if (_model.UseGamepad)
			{
				_droneGamepad.Run();
			}
			_droneMovement.Run();
			_droneCamera.Run();
			if (_model.DroneRecordingStateData.State == RecordingState.Recording)
			{
				TimeSpan result = _lckService.GetRecordingDuration().Result;
				_model.DroneRecordingStateData.Span = result;
			}
		}
	}

	public LckCamera GetLckCamera()
	{
		return _lckCamera;
	}

	public DroneDataModel GetModel()
	{
		return _model;
	}

	public void SetDronePositionAndRotation(Vector3 position, Quaternion rotation)
	{
		_droneMovement.MoveAndRotateDroneInstantly(position, rotation);
	}

	private void OnRecordingStarted(LckResult result)
	{
		_model.DroneRecordingStateData.State = RecordingState.Recording;
		_model.DroneRecordingStateData.Span = TimeSpan.Zero;
	}

	private void OnRecordingStopped(LckResult result)
	{
		_model.DroneRecordingStateData.State = RecordingState.Saving;
	}

	private void OnRecordingSaved(LckResult<RecordingData> lckResult)
	{
		_model.DroneRecordingStateData.State = RecordingState.Idle;
		_model.DroneRecordingStateData.Span = TimeSpan.Zero;
	}

	private void ProcessRecordButtonBeingPressed()
	{
		if (_model.IsDroneModeActive)
		{
			if (_lckService.IsRecording().Result)
			{
				_lckService.StopRecording();
			}
			else if (_model.DroneRecordingStateData.State != RecordingState.Saving && _model.DroneRecordingStateData.State == RecordingState.Idle)
			{
				_lckService.StartRecording();
			}
		}
	}

	private void ProcessDroneActiveState(bool isActive)
	{
		if (!isActive && _lckService.IsRecording().Result)
		{
			_lckService.StopRecording();
		}
	}

	private void SetProcessedMovementSmoothness(float value)
	{
		_droneMovement.SetMoveSmoothness(value / _model.MaxMoveSmoothness * 0.5f);
	}

	private void SetProcessedRotationSmoothness(float value)
	{
		_droneMovement.SetRotationSmoothness(value / _model.MaxRotationSmoothness * 0.5f);
	}

	private void SetProcessedFovSmoothness(float value)
	{
		_droneCamera.SetSmoothness(value / _model.MaxFovSmoothness);
	}
}
