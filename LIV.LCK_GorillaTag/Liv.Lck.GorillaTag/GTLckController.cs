using System;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Smoothing;
using Liv.Lck.Tablet;
using Liv.Lck.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

[DefaultExecutionOrder(-790)]
public class GTLckController : MonoBehaviour
{
	public delegate void CameraModeDelegate(CameraMode mode, ILckCamera camera);

	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private GtUiSettings _settings;

	[SerializeField]
	private GtSettingsSectionGroup _gtSettingsSectionGroup;

	[SerializeField]
	public GtColliderTriggerProcessorsGroup GtColliderTriggerProcessorsGroup;

	[Space(10f)]
	[Header("Camera Settings References")]
	[Header("Selfie")]
	[SerializeField]
	private GtCounter _selfieFovCounter;

	[SerializeField]
	private GtCounter _selfieSmoothnessCounter;

	[SerializeField]
	private GtScreenButton _selfieFlipButton;

	[SerializeField]
	private GtToggle _tabletFollowsPlayerToggle;

	[Header("First Person")]
	[SerializeField]
	private GtCounter _firstPersonFovCounter;

	[SerializeField]
	private GtCounter _firstPersonSmoothnessCounter;

	[Header("Third Person")]
	[SerializeField]
	private GtCounter _thirdPersonFovCounter;

	[SerializeField]
	private GtCounter _thirdPersonSmoothnessCounter;

	[SerializeField]
	private GtCounter _thirdPersonDistanceCounter;

	[SerializeField]
	private GtToggle _thirdPersonPositionToggle;

	[Space(10f)]
	[Header("Camera Modes")]
	[Header("Selfie")]
	[SerializeField]
	private LckCamera _selfieCamera;

	[SerializeField]
	private LckStabilizer _selfieStabilizer;

	[SerializeField]
	private GtCameraModeTransform _selfieFrontTransform;

	[SerializeField]
	private GtCameraModeTransform _selfieBackTransform;

	[SerializeField]
	private GtCameraModeTransform _selfieFollowModeTransform;

	[Header("First Person")]
	[SerializeField]
	private LckCamera _firstPersonCamera;

	[SerializeField]
	private LckStabilizer _firstPersonStabilizer;

	[Header("Third Person")]
	[SerializeField]
	private LckCamera _thirdPersonCamera;

	[SerializeField]
	private CoconutCamera _coconutCamera;

	[SerializeField]
	private GtThirdPersonCameraBehaviour _thirdPersonCameraBehaviour;

	[Header("Headset View")]
	[SerializeField]
	private LckHeadsetCamera _headsetCamera;

	[SerializeField]
	private GameObject[] _virtualCameraOnlyUI;

	public Action<CameraMode> OnFOVUpdated;

	[Header("Drone")]
	[SerializeField]
	private DroneSystem _droneSystem;

	[SerializeField]
	private GtDroneModeTabletUIAppearance _droneModeTabletUIAppearance;

	[SerializeField]
	private GtToggle _headsetEyeToggle;

	[SerializeField]
	private GtToggle _headsetCropModeToggle;

	[Space(10f)]
	[Header("Recording And Streaming Bar")]
	[Header("References")]
	[SerializeField]
	private GtRecordButton _recordButton;

	[SerializeField]
	private GtSaveEchoButton _saveEchoButton;

	[SerializeReference]
	private ScriptableObject _qualityConfig;

	[SerializeField]
	private LckQualitySelector _qualitySelector;

	[SerializeField]
	private GtButton _changeOrientation;

	[SerializeField]
	private GtAudioButton _microphoneButton;

	[SerializeField]
	private RectTransform _monitorTransform;

	[SerializeField]
	private LckTopButtonsController _topButtonsController;

	private bool _isHorizontalMode = true;

	[SerializeField]
	private LckNotificationController _notificationController;

	private CameraMode _currentCameraMode;

	private Camera _playerCamera;

	private Transform _playerHead;

	private bool _justTransitioned;

	private bool _isTabletFollowingPlayer;

	private float _selfieSmoothness;

	private bool _isSelfieFront = true;

	private LckCameraOrientation _currentCameraOrientation = LckCameraOrientation.Landscape;

	private Vector3 _selfieFollowModeOffset = Vector3.zero;

	private bool _micState = true;

	private float _thirdPersonHeightAngle = 25f;

	private CameraTrackDescriptor _currentTrackDescriptor;

	private bool _isOverlayActive;

	[field: SerializeField]
	[field: Header("Sections References")]
	[field: Space(10f)]
	public GtSelectorsGroup GTSelectorsGroup { get; private set; }

	[field: SerializeField]
	public float ThirdPersonHeightAngle { get; private set; } = 25f;


	[field: SerializeField]
	public float ThirdPersonSideAngle { get; private set; }

	public bool IsThirdPersonFront { get; private set; } = true;


	public bool HorizontalMode => _isHorizontalMode;

	public CameraMode CurrentCameraMode => _currentCameraMode;

	public bool IsTabletFollowingPlayer => _isTabletFollowingPlayer;

	public event CameraModeDelegate OnCameraModeChanged;

	public event UnityAction<bool> OnHorizontalModeChanged = delegate
	{
	};

	private void OnValidate()
	{
		if (_qualityConfig != null && !(_qualityConfig is ILckQualityConfig))
		{
			Debug.LogError("LCK Quality Config must implement ILckQualityConfig interface");
		}
	}

	private void OnEnable()
	{
		if (!LckDiContainer.Instance.HasService<ILckService>())
		{
			LckServiceInitializer.ConfigureServices(LckDiContainer.Instance, (ILckQualityConfig)_qualityConfig);
			_lckService = LckService.GetService().Result;
		}
		CheckMicPermission();
		GTSelectorsGroup.onCameraModeChanged.AddListener(_gtSettingsSectionGroup.EvaluateMode);
		GTSelectorsGroup.onCameraModeChanged.AddListener(ChangeCameraMode);
		_selfieFovCounter.onValueChanged.AddListener(ProcessSelfieFov);
		_selfieSmoothnessCounter.onValueChanged.AddListener(ProcessSelfieSmoothness);
		_selfieFlipButton.onTapStarted.AddListener(ProcessSelfieFlip);
		if ((bool)_tabletFollowsPlayerToggle)
		{
			_tabletFollowsPlayerToggle.onValueChanged.AddListener(SetFollowModeState);
		}
		_firstPersonFovCounter.onValueChanged.AddListener(ProcessFirstPersonFov);
		_firstPersonSmoothnessCounter.onValueChanged.AddListener(ProcessFirstPersonSmoothness);
		_thirdPersonFovCounter.onValueChanged.AddListener(ProcessThirdPersonFov);
		if ((bool)_thirdPersonSmoothnessCounter)
		{
			_thirdPersonSmoothnessCounter.onValueChanged.AddListener(ProcessThirdPersonSmoothness);
		}
		_thirdPersonDistanceCounter.onValueChanged.AddListener(ProcessThirdPersonDistance);
		if ((bool)_thirdPersonPositionToggle)
		{
			_thirdPersonPositionToggle.onValueChanged.AddListener(ProcessThirdPersonPosition);
		}
		if ((bool)_headsetEyeToggle)
		{
			_headsetEyeToggle.onValueChanged.AddListener(ProcessHeadsetEye);
		}
		if ((bool)_headsetCropModeToggle)
		{
			_headsetCropModeToggle.onValueChanged.AddListener(ProcessHeadsetCropMode);
		}
		_recordButton.onPressed += ToggleRecording;
		if ((bool)_saveEchoButton)
		{
			_saveEchoButton.onPressed += SaveEcho;
		}
		_changeOrientation.onTap.AddListener(ToggleOrientation);
		_microphoneButton.onTap.AddListener(ToggleMicrophoneRecording);
		if ((bool)_droneSystem)
		{
			_droneSystem.OnRequestDroneModeState += ProcessDroneModeStateChangeRequest;
		}
	}

	private async void OnQualityOptionSelected(QualityOption qualityOption)
	{
		bool num = await StopEchoIfActiveAsync();
		CameraTrackDescriptor descriptorForCurrentOrientation = GetDescriptorForCurrentOrientation(qualityOption.RecordingCameraTrackDescriptor);
		_lckService.SetTrackDescriptor(LckCaptureType.Recording, descriptorForCurrentOrientation);
		CameraTrackDescriptor descriptorForCurrentOrientation2 = GetDescriptorForCurrentOrientation(qualityOption.StreamingCameraTrackDescriptor);
		_lckService.SetTrackDescriptor(LckCaptureType.Streaming, descriptorForCurrentOrientation2);
		if (num)
		{
			RestartEcho();
		}
	}

	public void SetCameraMode(CameraMode mode)
	{
		ChangeCameraMode(mode);
	}

	private void ProcessDroneModeStateChangeRequest(bool isActive)
	{
		_droneModeTabletUIAppearance.IsDroneModeActive = isActive;
		ChangeCameraMode(isActive ? CameraMode.Drone : CameraMode.Selfie);
	}

	private CameraTrackDescriptor GetDescriptorForCurrentOrientation(CameraTrackDescriptor descriptor)
	{
		CameraResolutionDescriptor cameraResolutionDescriptor = descriptor.CameraResolutionDescriptor;
		descriptor.CameraResolutionDescriptor = cameraResolutionDescriptor.GetResolutionInOrientation(_currentCameraOrientation);
		return descriptor;
	}

	private bool IsQuest2()
	{
		return false;
	}

	private void OnDisable()
	{
		GTSelectorsGroup.onCameraModeChanged.RemoveListener(_gtSettingsSectionGroup.EvaluateMode);
		GTSelectorsGroup.onCameraModeChanged.RemoveListener(ChangeCameraMode);
		_selfieFovCounter.onValueChanged.RemoveListener(ProcessSelfieFov);
		_selfieSmoothnessCounter.onValueChanged.RemoveListener(ProcessSelfieSmoothness);
		_selfieFlipButton.onTapStarted.RemoveListener(ProcessSelfieFlip);
		if ((bool)_tabletFollowsPlayerToggle)
		{
			_tabletFollowsPlayerToggle.onValueChanged.RemoveListener(SetFollowModeState);
		}
		_firstPersonFovCounter.onValueChanged.RemoveListener(ProcessFirstPersonFov);
		_firstPersonSmoothnessCounter.onValueChanged.RemoveListener(ProcessFirstPersonSmoothness);
		_thirdPersonFovCounter.onValueChanged.RemoveListener(ProcessThirdPersonFov);
		if ((bool)_thirdPersonSmoothnessCounter)
		{
			_thirdPersonSmoothnessCounter.onValueChanged.RemoveListener(ProcessThirdPersonSmoothness);
		}
		_thirdPersonDistanceCounter.onValueChanged.RemoveListener(ProcessThirdPersonDistance);
		if ((bool)_thirdPersonPositionToggle)
		{
			_thirdPersonPositionToggle.onValueChanged.RemoveListener(ProcessThirdPersonPosition);
		}
		if ((bool)_headsetEyeToggle)
		{
			_headsetEyeToggle.onValueChanged.RemoveListener(ProcessHeadsetEye);
		}
		if ((bool)_headsetCropModeToggle)
		{
			_headsetCropModeToggle.onValueChanged.RemoveListener(ProcessHeadsetCropMode);
		}
		_recordButton.onPressed -= ToggleRecording;
		if ((bool)_saveEchoButton)
		{
			_saveEchoButton.onPressed -= SaveEcho;
		}
		_changeOrientation.onTap.RemoveListener(ToggleOrientation);
		_microphoneButton.onTap.RemoveListener(ToggleMicrophoneRecording);
		if ((bool)_droneSystem)
		{
			_droneSystem.OnRequestDroneModeState -= ProcessDroneModeStateChangeRequest;
		}
	}

	private void OnDestroy()
	{
		LckQualitySelector qualitySelector = _qualitySelector;
		qualitySelector.OnQualityOptionChanged = (Action<QualityOption>)Delegate.Remove(qualitySelector.OnQualityOptionChanged, new Action<QualityOption>(OnQualityOptionSelected));
		if (_lckService != null)
		{
			if (_lckService.IsRecording().Result)
			{
				_lckService.StopRecording();
			}
			_lckService.OnRecordingStarted -= OnCaptureStart;
			_lckService.OnRecordingStopped -= OnCaptureStopped;
			_lckService.OnStreamingStarted -= OnCaptureStart;
			_lckService.OnStreamingStopped -= OnCaptureStopped;
		}
	}

	private void Start()
	{
		LckQualitySelector qualitySelector = _qualitySelector;
		qualitySelector.OnQualityOptionChanged = (Action<QualityOption>)Delegate.Combine(qualitySelector.OnQualityOptionChanged, new Action<QualityOption>(OnQualityOptionSelected));
		_qualitySelector.InitializeOptions((_qualityConfig as ILckQualityConfig).GetQualityOptionsForSystem());
		SetupCamera();
		_lckService.OnRecordingStarted += OnCaptureStart;
		_lckService.OnRecordingStopped += OnCaptureStopped;
		_lckService.OnStreamingStarted += OnCaptureStart;
		_lckService.OnStreamingStopped += OnCaptureStopped;
	}

	private void SetupCamera()
	{
		FindPlayerReferences();
		SetUpSelfieCamera();
		SetSelfieCameraOrientation(_isSelfieFront ? _selfieFrontTransform : _selfieBackTransform);
		_monitorTransform.sizeDelta = new Vector2(1109f, 624f);
		_monitorTransform.localScale = Vector3.one;
		LckResult lckResult = _lckService.SetMicrophoneCaptureActive(isActive: true);
		if (!lckResult.Success)
		{
			Debug.LogError($"LCK Could not enable microphone capture: {lckResult.Error}");
		}
	}

	private void Update()
	{
		switch (_currentCameraMode)
		{
		case CameraMode.FirstPerson:
			ProcessFirstCameraPosition();
			break;
		case CameraMode.ThirdPerson:
			ProcessThirdCameraPosition();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case CameraMode.Selfie:
		case CameraMode.Headset:
		case CameraMode.Drone:
			break;
		}
	}

	private void SetSelfieCameraOrientation(GtCameraModeTransform t)
	{
		_selfieStabilizer.transform.localPosition = t.position + _selfieFollowModeOffset;
		_selfieStabilizer.transform.localRotation = Quaternion.Euler(t.rotation);
		_selfieStabilizer.ReachTargetInstantly();
	}

	private void ProcessFirstCameraPosition()
	{
		_firstPersonStabilizer.transform.position = _playerCamera.transform.position + _playerCamera.transform.forward * 0.05f;
		_firstPersonStabilizer.transform.rotation = _playerCamera.transform.rotation;
		if (_justTransitioned)
		{
			_firstPersonStabilizer.ReachTargetInstantly();
			_justTransitioned = false;
		}
	}

	public void UpdateThirdPersonHeightAngle(float value)
	{
		ThirdPersonHeightAngle = value;
	}

	public void UpdateThirdPersonSideAngle(float value)
	{
		ThirdPersonSideAngle = value;
	}

	private void ProcessThirdCameraPosition()
	{
		_thirdPersonCameraBehaviour.heightOffsetAngle = _thirdPersonHeightAngle;
		if (_justTransitioned)
		{
			_thirdPersonCameraBehaviour.UpdateCameraWithoutSmoothing();
			_justTransitioned = false;
		}
	}

	private void FindPlayerReferences()
	{
		_playerCamera = Camera.main;
		if (GtTag.TryGetTransform(GtTagType.Player, out var playerHead))
		{
			_playerHead = playerHead;
		}
		else
		{
			_playerHead = Camera.main.transform;
		}
	}

	private void CheckMicPermission()
	{
	}

	private void ChangeCameraMode(CameraMode newMode)
	{
		if (_currentCameraMode == CameraMode.Headset && newMode != CameraMode.Headset && (bool)_notificationController)
		{
			_notificationController.HideNotifications();
		}
		_currentCameraMode = newMode;
		_justTransitioned = true;
		float fov = CalculateCorrectFOV(GetCurrentModeFOV());
		SetFOV(_currentCameraMode, fov);
		SetMonitorScale(newMode);
		SetVirtualCameraUIActive(newMode != CameraMode.Headset);
		if ((bool)_coconutCamera)
		{
			_coconutCamera.SetVisualsActive(newMode == CameraMode.ThirdPerson);
		}
		switch (newMode)
		{
		case CameraMode.Selfie:
			SetUpSelfieCamera();
			this.OnCameraModeChanged?.Invoke(newMode, _selfieCamera);
			break;
		case CameraMode.FirstPerson:
			SetUpFirstPersonCamera();
			this.OnCameraModeChanged?.Invoke(newMode, _firstPersonCamera);
			break;
		case CameraMode.ThirdPerson:
			SetUpThirdPersonCamera();
			this.OnCameraModeChanged?.Invoke(newMode, _thirdPersonCamera);
			break;
		case CameraMode.Headset:
			SetUpHeadsetCamera();
			_notificationController.ShowNotification(NotificationType.HeadsetView);
			this.OnCameraModeChanged?.Invoke(CameraMode.Headset, _headsetCamera);
			break;
		case CameraMode.Drone:
			SetUpDroneCamera();
			this.OnCameraModeChanged?.Invoke(newMode, _droneSystem.GetLckCamera());
			break;
		default:
			throw new ArgumentOutOfRangeException("newMode", newMode, null);
		}
	}

	private void SetUpDroneCamera()
	{
		if (_droneSystem == null)
		{
			Debug.LogError("LCK Drone System is not set");
			return;
		}
		Vector3 normalized = Vector3.Scale(_playerCamera.transform.forward, new Vector3(1f, 0f, 1f)).normalized;
		Vector3 position = _playerCamera.transform.position + normalized * 2f;
		Quaternion rotation = Quaternion.LookRotation(-normalized, Vector3.up);
		_droneSystem.SetDronePositionAndRotation(position, rotation);
		SetActiveLckCamera(_droneSystem.GetLckCamera().CameraId);
		this.OnCameraModeChanged?.Invoke(CameraMode.Drone, _droneSystem.GetLckCamera());
	}

	private void SetUpSelfieCamera()
	{
		SetActiveLckCamera(_selfieCamera.CameraId);
	}

	private void SetUpFirstPersonCamera()
	{
		SetActiveLckCamera(_firstPersonCamera.CameraId);
	}

	private void SetUpThirdPersonCamera()
	{
		SetActiveLckCamera(_thirdPersonCamera.CameraId);
	}

	private void SetUpHeadsetCamera()
	{
		SetActiveLckCamera(_headsetCamera.CameraId);
	}

	private void SetActiveLckCamera(string cameraId)
	{
		if (_lckService == null)
		{
			Debug.LogError("LCK Could not get Service");
		}
		else
		{
			_lckService.SetActiveCamera(cameraId);
		}
	}

	private void SetMonitorScale(CameraMode mode)
	{
		Vector3 vector = new Vector3(-1f, 1f, 1f);
		Vector3 one = Vector3.one;
		switch (mode)
		{
		case CameraMode.Selfie:
			_monitorTransform.localScale = (_isSelfieFront ? one : vector);
			break;
		case CameraMode.ThirdPerson:
			_monitorTransform.localScale = (IsThirdPersonFront ? one : vector);
			break;
		case CameraMode.FirstPerson:
		case CameraMode.Headset:
		case CameraMode.Drone:
			_monitorTransform.localScale = vector;
			break;
		}
	}

	private void SetVirtualCameraUIActive(bool active)
	{
		if (_virtualCameraOnlyUI == null)
		{
			return;
		}
		GameObject[] virtualCameraOnlyUI = _virtualCameraOnlyUI;
		foreach (GameObject gameObject in virtualCameraOnlyUI)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(active);
			}
		}
	}

	public Camera GetActiveCamera()
	{
		return _currentCameraMode switch
		{
			CameraMode.Selfie => _selfieCamera.GetCameraComponent(), 
			CameraMode.FirstPerson => _firstPersonCamera.GetCameraComponent(), 
			CameraMode.ThirdPerson => _thirdPersonCamera.GetCameraComponent(), 
			CameraMode.Headset => _headsetCamera.GetCameraComponent(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void SetFollowModeState(bool isFollowing)
	{
		_isTabletFollowingPlayer = isFollowing;
		_selfieFollowModeOffset = (isFollowing ? _selfieFollowModeTransform.position : Vector3.zero);
		SetSelfieCameraOrientation(_isSelfieFront ? _selfieFrontTransform : _selfieBackTransform);
		_selfieStabilizer.AffectPosition = !isFollowing;
		_selfieStabilizer.AffectRotation = !isFollowing;
		if (!isFollowing)
		{
			ProcessSelfieSmoothness(_selfieSmoothnessCounter.Value);
			_selfieStabilizer.ReachTargetInstantly();
		}
	}

	private void ProcessSelfieFov(int value)
	{
		_selfieCamera.GetCameraComponent().fieldOfView = CalculateCorrectFOV(value);
		OnFOVUpdated?.Invoke(CameraMode.Selfie);
	}

	private void ProcessSelfieSmoothness(int value)
	{
		_selfieSmoothness = value;
		if (!_isTabletFollowingPlayer)
		{
			_selfieStabilizer.RotationalSmoothing = (200f + _selfieSmoothness) * 0.001f * 0.6f;
		}
	}

	private void ProcessSelfieFlip()
	{
		_isSelfieFront = !_isSelfieFront;
		SetMonitorScale(CameraMode.Selfie);
		SetSelfieCameraOrientation(_isSelfieFront ? _selfieFrontTransform : _selfieBackTransform);
	}

	private void ProcessFirstPersonFov(int value)
	{
		_firstPersonCamera.GetCameraComponent().fieldOfView = CalculateCorrectFOV(value);
		OnFOVUpdated?.Invoke(CameraMode.FirstPerson);
	}

	private void ProcessFirstPersonSmoothness(int value)
	{
		_firstPersonStabilizer.PositionalSmoothing = (float)value * 0.001f * 0.3f;
		_firstPersonStabilizer.RotationalSmoothing = (float)value * 0.001f * 0.8f;
	}

	private void ProcessThirdPersonFov(int value)
	{
		_thirdPersonCamera.GetCameraComponent().fieldOfView = CalculateCorrectFOV(value);
	}

	private void ProcessThirdPersonSmoothness(int value)
	{
		_thirdPersonCameraBehaviour.rotationalSmoothness = (float)value * 0.001f * 0.99f;
	}

	private void ProcessThirdPersonDistance(int value)
	{
		_thirdPersonCameraBehaviour.distance = value;
		_justTransitioned = true;
	}

	private void ProcessThirdPersonPosition(bool isFirstSelected)
	{
		if (isFirstSelected != _thirdPersonCameraBehaviour.front)
		{
			_justTransitioned = true;
		}
		_thirdPersonCameraBehaviour.front = isFirstSelected;
		SetMonitorScale(CameraMode.ThirdPerson);
	}

	private void ToggleRecording()
	{
		if (_lckService == null)
		{
			Debug.LogWarning("LCK Could not get Service");
			return;
		}
		if (_lckService.IsRecording().Result)
		{
			_lckService.StopRecording();
			return;
		}
		SetOrientationQualityAndTopButtonsIsDisabledState(state: true);
		_lckService.StartRecording();
	}

	private void ProcessHeadsetEye(bool isFirstSelected)
	{
		if (!(_headsetCamera == null))
		{
			_headsetCamera.Eye = ((!isFirstSelected) ? EyeSelection.Right : EyeSelection.Left);
		}
	}

	private void OnCaptureStart(LckResult result)
	{
		if (!result.Success)
		{
			SetOrientationQualityAndTopButtonsIsDisabledState(state: false);
		}
		else if (_lckService.IsStreaming().Result)
		{
			_coconutCamera.SetRecordingState(isRecording: true);
		}
	}

	private void ProcessHeadsetCropMode(bool isFirstSelected)
	{
		if (!(_headsetCamera == null))
		{
			_headsetCamera.CropMode = ((!isFirstSelected) ? HeadsetCropMode.ZoomFill : HeadsetCropMode.Fit);
		}
	}

	private void SaveEcho()
	{
		if (_lckService == null)
		{
			Debug.LogWarning("LCK Could not get Service");
		}
		else
		{
			_lckService.TriggerEchoSave();
		}
	}

	private void OnCaptureStopped(LckResult result)
	{
		SetOrientationQualityAndTopButtonsIsDisabledState(state: false);
		if (!_lckService.IsStreaming().Result)
		{
			_coconutCamera.SetRecordingState(isRecording: false);
		}
	}

	public void SetOrientationQualityAndTopButtonsIsDisabledState(bool state)
	{
		_topButtonsController.SetTopButtonsIsDisabledState(state);
		_qualitySelector.SetQualityButtonIsDisabledState(state);
		_changeOrientation.SetDisabled(state);
	}

	public bool StopRecording()
	{
		if (_lckService == null)
		{
			Debug.LogWarning("LCK Could not get Service");
			return false;
		}
		LckResult<bool> lckResult = _lckService.IsRecording();
		if (!lckResult.Success || !lckResult.Result)
		{
			return false;
		}
		ToggleRecording();
		return true;
	}

	public void ToggleMicrophoneRecording(UnityAction<bool> isOn)
	{
		if (_lckService == null)
		{
			Debug.LogError("LCK Could not get Service");
			return;
		}
		_micState = !_micState;
		LckResult lckResult = _lckService.SetMicrophoneCaptureActive(_micState);
		isOn(_micState);
		if (!lckResult.Success && lckResult.Error == LckError.MicrophonePermissionDenied)
		{
			_microphoneButton.SetActiveState(isActive: false);
		}
	}

	private async void ToggleOrientation()
	{
		bool flag = await StopEchoIfActiveAsync();
		if (flag || !_lckService.IsCapturing().Result)
		{
			_isHorizontalMode = !_isHorizontalMode;
			_currentCameraOrientation = ((_currentCameraOrientation != LckCameraOrientation.Landscape) ? LckCameraOrientation.Landscape : LckCameraOrientation.Portrait);
			_lckService.SetCameraOrientation(_currentCameraOrientation);
			_monitorTransform.sizeDelta = ((_currentCameraOrientation == LckCameraOrientation.Landscape) ? new Vector2(1109f, 624f) : new Vector2(352f, 624f));
			float fov = CalculateCorrectFOV(GetCurrentModeFOV());
			SetFOV(_currentCameraMode, fov);
			this.OnHorizontalModeChanged?.Invoke(_isHorizontalMode);
			if (flag)
			{
				RestartEcho();
			}
		}
	}

	private CameraTrackDescriptor GenerateVerticalCameraTrackDescriptor()
	{
		return new CameraTrackDescriptor(new CameraResolutionDescriptor(_currentTrackDescriptor.CameraResolutionDescriptor.Height, _currentTrackDescriptor.CameraResolutionDescriptor.Width), _currentTrackDescriptor.Bitrate, _currentTrackDescriptor.Framerate, _currentTrackDescriptor.AudioBitrate);
	}

	private float GetCurrentModeFOV()
	{
		return _currentCameraMode switch
		{
			CameraMode.Selfie => _selfieFovCounter.Value, 
			CameraMode.FirstPerson => _firstPersonFovCounter.Value, 
			CameraMode.ThirdPerson => _thirdPersonFovCounter.Value, 
			CameraMode.Drone => 0f, 
			CameraMode.Headset => 0f, 
			_ => throw new Exception("Invalid Camera Mode"), 
		};
	}

	private async Task<bool> StopEchoIfActiveAsync()
	{
		LckResult<bool> lckResult = _lckService.IsEchoEnabled();
		if (lckResult.Success && lckResult.Result)
		{
			await _lckService.SetEchoEnabledAsync(enabled: false);
			return true;
		}
		return false;
	}

	private void RestartEcho()
	{
		_lckService.SetEchoEnabled(enabled: true);
	}

	private void SetFOV(CameraMode mode, float fov)
	{
		switch (mode)
		{
		case CameraMode.Selfie:
			_selfieCamera.GetCameraComponent().fieldOfView = fov;
			break;
		case CameraMode.FirstPerson:
			_firstPersonCamera.GetCameraComponent().fieldOfView = fov;
			break;
		case CameraMode.ThirdPerson:
			_thirdPersonCamera.GetCameraComponent().fieldOfView = fov;
			break;
		}
		OnFOVUpdated?.Invoke(mode);
	}

	private float CalculateCorrectFOV(float incomingVerticalFOV)
	{
		if (_currentCameraOrientation == LckCameraOrientation.Landscape)
		{
			return incomingVerticalFOV;
		}
		CameraResolutionDescriptor cameraResolutionDescriptor = _lckService.GetDescriptor().Result.cameraTrackDescriptor.CameraResolutionDescriptor;
		float aspectRatio = (float)cameraResolutionDescriptor.Height / (float)cameraResolutionDescriptor.Width;
		return Camera.VerticalToHorizontalFieldOfView(incomingVerticalFOV, aspectRatio);
	}

	internal void SetOverlayEnabled(bool value)
	{
		_isOverlayActive = value;
		SetMonitorScale(_currentCameraMode);
	}

	public void ApplyCameraSettings(GtCameraDockSettings settings)
	{
		CameraMode enforcedMode = settings.GetEnforcedMode();
		if (settings.forceCameraFacing)
		{
			_isSelfieFront = settings.isFront;
		}
		GTSelectorsGroup.Select(enforcedMode);
		if (_isSelfieFront != settings.isFront)
		{
			_selfieFlipButton.onTapStarted.Invoke();
		}
		if (settings.forceOrientation && _isHorizontalMode != settings.landscapeMode)
		{
			ToggleOrientation();
		}
		if (settings.forceFov)
		{
			SetFOV(enforcedMode, settings.fov);
			if (enforcedMode == CameraMode.Selfie)
			{
				_selfieFovCounter.Value = (int)settings.fov;
			}
		}
		SetMonitorScale(enforcedMode);
	}
}
