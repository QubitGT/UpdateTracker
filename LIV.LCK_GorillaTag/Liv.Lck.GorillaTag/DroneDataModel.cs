using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class DroneDataModel
{
	public delegate void OnDroneModelEvent();

	public delegate void OnDroneModelBoolEvent(bool value);

	public delegate void OnDroneModelFloatEvent(float value);

	public delegate void OnRecordingStateDataEvent(DroneRecordingStateData value);

	private bool _isDroneModeActive;

	private bool _useKeyboard;

	private bool _useMouse;

	private bool _useGamepad;

	private float _moveSpeed;

	private readonly float _minMoveSpeedStep;

	private readonly float _maxMoveSpeedStep;

	private float _previousMoveSpeed;

	private float _moveSmoothness;

	private readonly float _minMoveSmoothnessStep;

	private readonly float _maxMoveSmoothnessStep;

	private float _rotationSpeed;

	private readonly float _minRotationSpeedStep;

	private readonly float _maxRotationSpeedStep;

	private float _rotationSmoothness;

	private readonly float _maxRotationSmoothnessStep;

	private readonly float _minRotationSmoothnessStep;

	private float _fov;

	private readonly float _minFovStep;

	private readonly float _maxFovStep;

	private float _fovSmoothness;

	private readonly float _minFovSmoothnessStep;

	private readonly float _maxFovSmoothnessStep;

	private bool _snapAxis;

	private bool _useTiltAsDirection;

	private bool _isMouseInverted;

	private bool _showGUI;

	public bool IsDroneModeActive
	{
		get
		{
			return _isDroneModeActive;
		}
		set
		{
			_isDroneModeActive = value;
			this.OnIsDroneModeActive?.Invoke(_isDroneModeActive);
		}
	}

	public bool UseKeyboard
	{
		get
		{
			return _useKeyboard;
		}
		set
		{
			_useKeyboard = value;
			this.OnUseKeyboard?.Invoke(_useKeyboard);
		}
	}

	public bool UseMouse
	{
		get
		{
			return _useMouse;
		}
		set
		{
			_useMouse = value;
			this.OnUseMouse?.Invoke(_useMouse);
		}
	}

	public bool UseGamepad
	{
		get
		{
			return _useGamepad;
		}
		set
		{
			_useGamepad = value;
			this.OnUseGamepad?.Invoke(_useGamepad);
		}
	}

	public float MoveSpeed
	{
		get
		{
			return _moveSpeed;
		}
		set
		{
			_moveSpeed = value;
			this.OnMoveSpeedChanged?.Invoke(_moveSpeed);
		}
	}

	public float MaxMoveSpeed { get; }

	public float MoveSpeedStep { get; private set; }

	public float MoveSmoothness
	{
		get
		{
			return _moveSmoothness;
		}
		set
		{
			_moveSmoothness = value;
			this.OnMoveSmoothnessChanged?.Invoke(_moveSmoothness);
			if (Mathf.Approximately(_moveSmoothness, 0f))
			{
				this.OnIsMoveSmooth?.Invoke(value: false);
			}
			else
			{
				this.OnIsMoveSmooth?.Invoke(value: true);
			}
		}
	}

	public float MaxMoveSmoothness { get; }

	public float MoveSmoothnessStep { get; private set; }

	public float RotationSpeed
	{
		get
		{
			return _rotationSpeed;
		}
		set
		{
			_rotationSpeed = value;
			this.OnRotationSpeedChanged?.Invoke(_rotationSpeed);
		}
	}

	public float MaxRotationSpeed { get; }

	public float RotationSpeedStep { get; private set; }

	public float RotationSmoothness
	{
		get
		{
			return _rotationSmoothness;
		}
		set
		{
			_rotationSmoothness = value;
			this.OnRotationSmoothnessChanged?.Invoke(_rotationSmoothness);
			if (Mathf.Approximately(_rotationSmoothness, 0f))
			{
				this.OnIsRotationSmooth?.Invoke(value: false);
			}
			else
			{
				this.OnIsRotationSmooth?.Invoke(value: true);
			}
		}
	}

	public float MaxRotationSmoothness { get; }

	public float RotationSmoothnessStep { get; private set; }

	public float Fov
	{
		get
		{
			return _fov;
		}
		set
		{
			_fov = value;
			this.OnFovChanged?.Invoke(_fov);
		}
	}

	public float MinFov { get; }

	public float MaxFov { get; }

	public float FovStep { get; private set; }

	public float FovSmoothness
	{
		get
		{
			return _fovSmoothness;
		}
		set
		{
			_fovSmoothness = value;
			this.OnFovSmoothnessChanged?.Invoke(_fovSmoothness);
		}
	}

	public float MaxFovSmoothness { get; }

	public float FovSmoothnessStep { get; private set; }

	public bool SnapAxis
	{
		get
		{
			return _snapAxis;
		}
		set
		{
			_snapAxis = value;
			this.OnSnapAxis?.Invoke(_snapAxis);
		}
	}

	public bool UseTiltAsDirection
	{
		get
		{
			return _useTiltAsDirection;
		}
		set
		{
			_useTiltAsDirection = value;
			this.OnUseTiltAsDirection?.Invoke(_useTiltAsDirection);
		}
	}

	public bool IsMouseInverted
	{
		get
		{
			return _isMouseInverted;
		}
		set
		{
			_isMouseInverted = value;
			this.OnIsMouseInverted?.Invoke(_isMouseInverted);
		}
	}

	public bool ShowGUI
	{
		get
		{
			return _showGUI;
		}
		set
		{
			_showGUI = value;
			this.OnShowGUI?.Invoke(_showGUI);
		}
	}

	public DroneRecordingStateData DroneRecordingStateData { get; set; }

	public event OnDroneModelBoolEvent OnIsDroneModeActive;

	public event OnDroneModelBoolEvent OnUseKeyboard;

	public event OnDroneModelBoolEvent OnUseMouse;

	public event OnDroneModelBoolEvent OnUseGamepad;

	public event OnDroneModelFloatEvent OnMoveSpeedChanged;

	public event OnDroneModelFloatEvent OnMoveSmoothnessChanged;

	public event OnDroneModelBoolEvent OnIsMoveSmooth;

	public event OnDroneModelFloatEvent OnRotationSpeedChanged;

	public event OnDroneModelFloatEvent OnRotationSmoothnessChanged;

	public event OnDroneModelBoolEvent OnIsRotationSmooth;

	public event OnDroneModelFloatEvent OnFovChanged;

	public event OnDroneModelFloatEvent OnFovSmoothnessChanged;

	public event OnDroneModelBoolEvent OnSnapAxis;

	public event OnDroneModelBoolEvent OnUseTiltAsDirection;

	public event OnDroneModelBoolEvent OnIsMouseInverted;

	public event OnDroneModelBoolEvent OnShowGUI;

	public event OnDroneModelEvent OnRecordButtonPressed;

	public event OnRecordingStateDataEvent OnRecordingStateChanged;

	public DroneDataModel(bool isDroneModeActive, bool useKeyboard, bool useMouse, bool useGamepad, float moveSpeed, float maxMoveSpeed, float minMoveSpeedStep, float maxMoveSpeedStep, float moveSmoothness, float maxMoveSmoothness, float minMoveSmoothnessStep, float maxMoveSmoothnessStep, float rotationSpeed, float maxRotationSpeed, float minRotationSpeedStep, float maxRotationSpeedStep, float rotationSmoothness, float maxRotationSmoothness, float minRotationSmoothnessStep, float maxRotationSmoothnessStep, float fov, float minFov, float maxFov, float minFovStep, float maxFovStep, float fovSmoothness, float maxFovSmoothness, float minFovSmoothnessStep, float maxFovSmoothnessStep, bool snapAxis, bool useTiltAsDirection, bool isMouseInverted, bool showGUI)
	{
		IsDroneModeActive = isDroneModeActive;
		UseKeyboard = useKeyboard;
		UseMouse = useMouse;
		UseGamepad = useGamepad;
		MoveSpeed = moveSpeed;
		MaxMoveSpeed = maxMoveSpeed;
		MoveSpeedStep = maxMoveSpeedStep;
		_minMoveSpeedStep = minMoveSpeedStep;
		_maxMoveSpeedStep = maxMoveSpeedStep;
		MoveSmoothness = moveSmoothness;
		MaxMoveSmoothness = maxMoveSmoothness;
		MoveSmoothnessStep = maxMoveSmoothnessStep;
		_minMoveSmoothnessStep = minMoveSmoothnessStep;
		_maxMoveSmoothnessStep = maxMoveSmoothnessStep;
		RotationSpeed = rotationSpeed;
		MaxRotationSpeed = maxRotationSpeed;
		RotationSpeedStep = maxRotationSpeedStep;
		_minRotationSpeedStep = minRotationSpeedStep;
		_maxRotationSpeedStep = maxRotationSpeedStep;
		RotationSmoothness = rotationSmoothness;
		MaxRotationSmoothness = maxRotationSmoothness;
		RotationSmoothnessStep = maxRotationSmoothnessStep;
		_minRotationSmoothnessStep = minRotationSmoothnessStep;
		_maxRotationSmoothnessStep = maxRotationSmoothnessStep;
		Fov = fov;
		MinFov = minFov;
		MaxFov = maxFov;
		FovStep = maxFovStep;
		_minFovStep = minFovStep;
		_maxFovStep = maxFovStep;
		_fovSmoothness = fovSmoothness;
		MaxFovSmoothness = maxFovSmoothness;
		FovSmoothnessStep = maxFovSmoothnessStep;
		_minFovSmoothnessStep = minFovSmoothnessStep;
		_maxFovSmoothnessStep = maxFovSmoothnessStep;
		SnapAxis = snapAxis;
		UseTiltAsDirection = useTiltAsDirection;
		IsMouseInverted = isMouseInverted;
		ShowGUI = showGUI;
		DroneRecordingStateData = new DroneRecordingStateData();
	}

	public void BurstStarted()
	{
		_previousMoveSpeed = MoveSpeed;
		MoveSpeed = MaxMoveSpeed;
	}

	public void BurstEnded()
	{
		MoveSpeed = _previousMoveSpeed;
	}

	public void IncreaseFov()
	{
		Fov += FovStep;
		Fov = Mathf.Clamp(Fov, MinFov, MaxFov);
	}

	public void DecreaseFov()
	{
		Fov -= FovStep;
		Fov = Mathf.Clamp(Fov, MinFov, MaxFov);
	}

	public void MinimizeStepping()
	{
		MoveSpeedStep = _minMoveSpeedStep;
		RotationSpeedStep = _minRotationSpeedStep;
		MoveSmoothnessStep = _minMoveSmoothnessStep;
		RotationSmoothnessStep = _minRotationSmoothnessStep;
		FovStep = _minFovStep;
		FovSmoothnessStep = _minFovSmoothnessStep;
	}

	public void MaximizeStepping()
	{
		MoveSpeedStep = _maxMoveSpeedStep;
		RotationSpeedStep = _maxRotationSpeedStep;
		MoveSmoothnessStep = _maxMoveSmoothnessStep;
		RotationSmoothnessStep = _maxRotationSmoothnessStep;
		FovStep = _maxFovStep;
		FovSmoothnessStep = _maxFovSmoothnessStep;
	}

	public void ToggleShowGUI()
	{
		ShowGUI = !ShowGUI;
	}

	public void RecordButtonPressed()
	{
		this.OnRecordButtonPressed?.Invoke();
	}
}
