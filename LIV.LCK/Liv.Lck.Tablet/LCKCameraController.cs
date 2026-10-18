using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Smoothing;
using Liv.Lck.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Liv.Lck.Tablet;

[DefaultExecutionOrder(-890)]
public class LCKCameraController : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[Header("Options")]
	[SerializeField]
	[Tooltip("If true, the script will automatically manage layers and culling masks. It will move objects in the 'ObjectsHiddenFromSelfieCamera' list to the 'Tablet Rendering Layer' and adjust camera culling masks to hide this layer in Selfie mode and show it in other modes.")]
	private bool _modifyRenderLayerAndCullingMasks = true;

	[SerializeField]
	[Tooltip("The name of the Unity layer used to tag objects that should be hidden from the selfie camera. This layer must exist in the project's Tag and Layer Manager.")]
	private string _tabletRenderingLayer = "LCK Tablet";

	[FormerlySerializedAs("_objectsOnTabletRenderingLayer")]
	[SerializeField]
	[Tooltip("A list of all GameObjects (e.g., the tablet model itself) that should be made invisible to the selfie camera.")]
	private List<GameObject> _objectsHiddenFromSelfieCamera = new List<GameObject>();

	[SerializeReference]
	[Tooltip("A ScriptableObject that implements the ILckQualityConfig interface. This object defines the available quality levels (resolution, bitrate, etc.) for recording and streaming.")]
	private ScriptableObject _qualityConfig;

	[SerializeField]
	[Tooltip("The transform representing the user's head or HMD. This is the primary anchor for first-person and third-person camera positioning. If null, it will default to the main camera's transform.")]
	private Transform _hmdTransform;

	[SerializeField]
	[Tooltip("A multiplier applied to the value from the third-person distance UI button to determine the actual camera distance.")]
	private float _thirdPersonDistanceMultiplier = 0.75f;

	[SerializeField]
	[Tooltip("The default angle (in degrees) of the third-person camera, looking down at the player.")]
	private float _thirdPersonHeightAngle = 25f;

	[SerializeField]
	[Tooltip("Mode that is used to determine when the active camera's position is updated. Depending on update order / movement setup, changing this can fix tablet jitter in captures.")]
	private UpdateTimingMode _cameraPositionUpdateTimingMode = UpdateTimingMode.LateUpdate;

	[Header("Main References")]
	[SerializeField]
	private LCKSettingsButtonsController _settingsButtonsController;

	[SerializeField]
	private LckTopButtonsController _topButtonsController;

	[SerializeField]
	private RectTransform _monitorTransform;

	[SerializeField]
	private LckQualitySelector _qualitySelector;

	[SerializeField]
	private LckNotificationController _notificationController;

	[Header("Button References")]
	[Header("Selfie")]
	[SerializeField]
	private LckDoubleButton _selfieFOVDoubleButton;

	[SerializeField]
	private LckDoubleButton _selfieSmoothingDoubleButton;

	[Header("First Person")]
	[SerializeField]
	private LckDoubleButton _firstPersonFOVDoubleButton;

	[SerializeField]
	private LckDoubleButton _firstPersonSmoothingDoubleButton;

	[Header("Third Person")]
	[SerializeField]
	private LckDoubleButton _thirdPersonFOVDoubleButton;

	[SerializeField]
	private LckDoubleButton _thirdPersonSmoothingDoubleButton;

	[SerializeField]
	private LckDoubleButton _thirdPersonDistanceDoubleButton;

	[Header("Portrait Landscape Toggle")]
	[SerializeField]
	private LckButton _orientationButton;

	[Header("Camera Modes")]
	[Header("Selfie")]
	[SerializeField]
	private LckCamera _selfieCamera;

	[SerializeField]
	private LckStabilizer _selfieStabilizer;

	[Header("First Person")]
	[SerializeField]
	private LckCamera _firstPersonCamera;

	[SerializeField]
	private LckStabilizer _firstPersonStabilizer;

	[Header("Third Person")]
	[SerializeField]
	private LckCamera _thirdPersonCamera;

	[SerializeField]
	private LckStabilizer _thirdPersonStabilizer;

	[Header("Headset")]
	[SerializeField]
	private LckHeadsetCamera _headsetCamera;

	private float _thirdPersonDistance = 1f;

	private bool _isThirdPersonFront = true;

	private bool _isSelfieFront = true;

	private LckCameraOrientation _currentCameraOrientation = LckCameraOrientation.Landscape;

	private bool _justTransitioned;

	private bool _gameAudioRecordingEnabled = true;

	private CameraMode _currentCameraMode;

	public Action<CameraMode> OnCameraModeChanged;

	public static bool ColliderButtonsInUse;

	public Transform HmdTransform
	{
		get
		{
			if (_hmdTransform == null)
			{
				_hmdTransform = Camera.main.transform;
			}
			return _hmdTransform;
		}
		set
		{
			_hmdTransform = value;
		}
	}

	public UpdateTimingMode CameraPositionUpdateTimingMode
	{
		get
		{
			return _cameraPositionUpdateTimingMode;
		}
		set
		{
			_cameraPositionUpdateTimingMode = value;
		}
	}

	private void OnValidate()
	{
		if (_qualityConfig != null && !(_qualityConfig is ILckQualityConfig))
		{
			Debug.LogError("LCK Quality Config must implement ILckQualityConfig interface");
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			ColliderButtonsInUse = false;
		}
	}

	private void Start()
	{
		if (_modifyRenderLayerAndCullingMasks)
		{
			SetTabletLayer();
		}
		LckQualitySelector qualitySelector = _qualitySelector;
		qualitySelector.OnQualityOptionChanged = (Action<QualityOption>)Delegate.Combine(qualitySelector.OnQualityOptionChanged, new Action<QualityOption>(OnQualityOptionSelected));
		_qualitySelector.InitializeOptions((_qualityConfig as ILckQualityConfig).GetQualityOptionsForSystem());
		SetActiveLckCamera(_selfieCamera.CameraId);
		SetSelfieCameraOrientation(Vector3.zero, Vector3.zero);
		_lckService.OnRecordingStarted += OnCaptureStart;
		_lckService.OnRecordingStopped += OnCaptureStopped;
		_lckService.OnStreamingStarted += OnCaptureStart;
		_lckService.OnStreamingStopped += OnCaptureStopped;
	}

	private void Awake()
	{
		if (!LckDiContainer.Instance.HasService<ILckService>())
		{
			LckServiceInitializer.ConfigureServices(LckDiContainer.Instance, (LckQualityConfig)_qualityConfig);
			_lckService = LckDiContainer.Instance.GetService<ILckService>();
		}
	}

	private void SetTabletLayer()
	{
		int num = LayerMask.NameToLayer(_tabletRenderingLayer);
		if (num == -1)
		{
			return;
		}
		foreach (GameObject item in _objectsHiddenFromSelfieCamera)
		{
			item.layer = num;
		}
		_selfieCamera.GetCameraComponent().cullingMask &= ~(1 << num);
		_firstPersonCamera.GetCameraComponent().cullingMask |= 1 << num;
		_thirdPersonCamera.GetCameraComponent().cullingMask |= 1 << num;
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

	private CameraTrackDescriptor GetDescriptorForCurrentOrientation(CameraTrackDescriptor descriptor)
	{
		CameraResolutionDescriptor cameraResolutionDescriptor = descriptor.CameraResolutionDescriptor;
		descriptor.CameraResolutionDescriptor = cameraResolutionDescriptor.GetResolutionInOrientation(_currentCameraOrientation);
		return descriptor;
	}

	private void UpdateCameraPosition()
	{
		if (_lckService != null)
		{
			switch (_currentCameraMode)
			{
			case CameraMode.FirstPerson:
				ProcessFirstCameraPosition();
				break;
			case CameraMode.ThirdPerson:
				ProcessThirdCameraPosition();
				break;
			case CameraMode.Selfie:
			case CameraMode.Headset:
				break;
			}
		}
	}

	private void OnEnable()
	{
		LCKSettingsButtonsController settingsButtonsController = _settingsButtonsController;
		settingsButtonsController.OnCameraModeChanged = (Action<CameraMode>)Delegate.Combine(settingsButtonsController.OnCameraModeChanged, new Action<CameraMode>(CameraModeChanged));
		_selfieFOVDoubleButton.OnValueChanged += ProcessSelfieFov;
		_selfieSmoothingDoubleButton.OnValueChanged += ProcessSelfieSmoothness;
		_firstPersonFOVDoubleButton.OnValueChanged += ProcessFirstPersonFov;
		_firstPersonSmoothingDoubleButton.OnValueChanged += ProcessFirstPersonSmoothness;
		_thirdPersonFOVDoubleButton.OnValueChanged += ProcessThirdPersonFov;
		_thirdPersonSmoothingDoubleButton.OnValueChanged += ProcessThirdPersonSmoothness;
		_thirdPersonDistanceDoubleButton.OnValueChanged += ProcessThirdPersonDistance;
	}

	private void OnDisable()
	{
		LCKSettingsButtonsController settingsButtonsController = _settingsButtonsController;
		settingsButtonsController.OnCameraModeChanged = (Action<CameraMode>)Delegate.Remove(settingsButtonsController.OnCameraModeChanged, new Action<CameraMode>(CameraModeChanged));
		_selfieFOVDoubleButton.OnValueChanged -= ProcessSelfieFov;
		_selfieSmoothingDoubleButton.OnValueChanged -= ProcessSelfieSmoothness;
		_firstPersonFOVDoubleButton.OnValueChanged -= ProcessFirstPersonFov;
		_firstPersonSmoothingDoubleButton.OnValueChanged -= ProcessFirstPersonSmoothness;
		_thirdPersonFOVDoubleButton.OnValueChanged -= ProcessThirdPersonFov;
		_thirdPersonSmoothingDoubleButton.OnValueChanged -= ProcessThirdPersonSmoothness;
		_thirdPersonDistanceDoubleButton.OnValueChanged -= ProcessThirdPersonDistance;
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

	private void LateUpdate()
	{
		if (CameraPositionUpdateTimingMode == UpdateTimingMode.LateUpdate)
		{
			UpdateCameraPosition();
		}
	}

	private void Update()
	{
		if (CameraPositionUpdateTimingMode == UpdateTimingMode.Update)
		{
			UpdateCameraPosition();
		}
	}

	private void FixedUpdate()
	{
		if (CameraPositionUpdateTimingMode == UpdateTimingMode.FixedUpdate)
		{
			UpdateCameraPosition();
		}
	}

	private void SetSelfieCameraOrientation(Vector3 position, Vector3 rotation)
	{
		_selfieStabilizer.transform.localPosition = position;
		_selfieStabilizer.transform.localRotation = Quaternion.Euler(rotation);
		_selfieStabilizer.ReachTargetInstantly();
	}

	private void ProcessSelfieFov(float value)
	{
		_selfieCamera.GetCameraComponent().fieldOfView = CalculateCorrectFOV(value);
	}

	private void ProcessSelfieSmoothness(float value)
	{
		_selfieStabilizer.PositionalSmoothing = value * 0.1f * 0.3f;
		_selfieStabilizer.RotationalSmoothing = value * 0.1f * 0.8f;
	}

	private void ProcessFirstPersonFov(float value)
	{
		_firstPersonCamera.GetCameraComponent().fieldOfView = CalculateCorrectFOV(value);
	}

	private void ProcessFirstPersonSmoothness(float value)
	{
		_firstPersonStabilizer.PositionalSmoothing = value * 0.1f * 0.3f;
		_firstPersonStabilizer.RotationalSmoothing = value * 0.1f * 0.8f;
	}

	private void ProcessFirstCameraPosition()
	{
		_firstPersonStabilizer.transform.position = HmdTransform.position + HmdTransform.forward * 0.05f;
		_firstPersonStabilizer.transform.rotation = HmdTransform.rotation;
		if (_justTransitioned)
		{
			_firstPersonStabilizer.ReachTargetInstantly();
			_justTransitioned = false;
		}
	}

	private void ProcessThirdPersonFov(float value)
	{
		_thirdPersonCamera.GetCameraComponent().fieldOfView = CalculateCorrectFOV(value);
	}

	private void ProcessThirdPersonSmoothness(float value)
	{
		_thirdPersonStabilizer.PositionalSmoothing = value * 0.1f * 0.3f;
		_thirdPersonStabilizer.RotationalSmoothing = value * 0.1f * 0.8f;
	}

	private void ProcessThirdPersonDistance(float value)
	{
		_thirdPersonDistance = value;
		_justTransitioned = true;
	}

	private void ProcessThirdCameraPosition()
	{
		Vector3 to = new Vector3(HmdTransform.forward.x, 0f, HmdTransform.forward.z);
		to.Normalize();
		if (!_isThirdPersonFront)
		{
			to *= -1f;
		}
		Vector3 vector = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, to, Vector3.up), Vector3.up) * Quaternion.AngleAxis(_thirdPersonHeightAngle, -Vector3.right) * new Vector3(0f, 0f, _thirdPersonDistance * _thirdPersonDistanceMultiplier);
		_thirdPersonStabilizer.transform.position = HmdTransform.position + vector;
		_thirdPersonStabilizer.transform.LookAt(HmdTransform.position);
		if (_justTransitioned)
		{
			_thirdPersonStabilizer.ReachTargetInstantly();
			_justTransitioned = false;
		}
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
		case CameraMode.Headset:
			break;
		}
	}

	public void ToggleMicrophoneRecording(bool isMicOn)
	{
		if (_lckService == null)
		{
			LckLog.LogError("No Lck Service found when trying to set mic state to: " + isMicOn, "ToggleMicrophoneRecording", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 550);
			return;
		}
		LckResult lckResult = _lckService.SetMicrophoneCaptureActive(isMicOn);
		if (!lckResult.Success)
		{
			LckLog.LogError($"LCK Could not enable microphone capture: {lckResult.Error}", "ToggleMicrophoneRecording", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 557);
		}
	}

	public void ToggleGameAudio()
	{
		_gameAudioRecordingEnabled = !_gameAudioRecordingEnabled;
		_lckService.SetGameAudioCaptureActive(_gameAudioRecordingEnabled);
	}

	public void ToggleRecording()
	{
		if (_lckService != null)
		{
			if (_lckService.IsRecording().Result)
			{
				_lckService.StopRecording();
				return;
			}
			SetOrientationQualityAndTopButtonsIsDisabledState(state: true);
			_lckService.StartRecording();
		}
	}

	public void SaveEcho()
	{
		if (_lckService == null)
		{
			return;
		}
		LckResult<bool> lckResult = _lckService.IsEchoEnabled();
		if (lckResult == null || !lckResult.Success)
		{
			LckLog.LogError("Failed to get echo state from LCK service: " + ((lckResult == null) ? "No result returned" : lckResult.Message), "SaveEcho", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 602);
			return;
		}
		if (!lckResult.Result)
		{
			LckLog.LogError("Echo is disabled - saving echo is not possible.", "SaveEcho", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 608);
			return;
		}
		LckResult lckResult2 = _lckService.TriggerEchoSave();
		if (!lckResult2.Success)
		{
			LckLog.LogError($"Failed to save echo clip: {lckResult2.Error}", "SaveEcho", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 615);
		}
	}

	public void OnCaptureStart(LckResult result)
	{
		if (!result.Success)
		{
			SetOrientationQualityAndTopButtonsIsDisabledState(state: false);
		}
	}

	public void OnCaptureStopped(LckResult result)
	{
		SetOrientationQualityAndTopButtonsIsDisabledState(state: false);
	}

	public void SetOrientationQualityAndTopButtonsIsDisabledState(bool state)
	{
		_topButtonsController.SetTopButtonsIsDisabledState(state);
		_qualitySelector.SetQualityButtonIsDisabledState(state);
		_orientationButton.SetIsDisabled(state);
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
		LckResult lckResult = _lckService.SetEchoEnabled(enabled: true);
		if (!lckResult.Success)
		{
			LckLog.LogError("Failed to restart echo after settings change: " + lckResult.Message, "RestartEcho", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 676);
		}
	}

	public async void ToggleOrientation()
	{
		bool flag = await StopEchoIfActiveAsync();
		if (flag || !_lckService.IsCapturing().Result)
		{
			_currentCameraOrientation = ((_currentCameraOrientation != LckCameraOrientation.Landscape) ? LckCameraOrientation.Landscape : LckCameraOrientation.Portrait);
			_lckService.SetCameraOrientation(_currentCameraOrientation);
			_monitorTransform.sizeDelta = ((_currentCameraOrientation == LckCameraOrientation.Landscape) ? new Vector2(1109f, 624f) : new Vector2(352f, 624f));
			GetCurrentModeCamera().fieldOfView = CalculateCorrectFOV(GetCurrentModeFOV());
			if (flag)
			{
				RestartEcho();
			}
		}
	}

	private Camera GetCurrentModeCamera()
	{
		return _currentCameraMode switch
		{
			CameraMode.Selfie => _selfieCamera.GetCameraComponent(), 
			CameraMode.FirstPerson => _firstPersonCamera.GetCameraComponent(), 
			CameraMode.ThirdPerson => _thirdPersonCamera.GetCameraComponent(), 
			CameraMode.Headset => _headsetCamera.GetCameraComponent(), 
			_ => throw new Exception("Invalid Camera Mode"), 
		};
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

	private float GetCurrentModeFOV()
	{
		return _currentCameraMode switch
		{
			CameraMode.Selfie => _selfieFOVDoubleButton.Value, 
			CameraMode.FirstPerson => _firstPersonFOVDoubleButton.Value, 
			CameraMode.ThirdPerson => _thirdPersonFOVDoubleButton.Value, 
			CameraMode.Headset => 0f, 
			_ => throw new Exception("Invalid Camera Mode"), 
		};
	}

	public void ProcessSelfieFlip()
	{
		_isSelfieFront = !_isSelfieFront;
		SetMonitorScale(CameraMode.Selfie);
		if (_isSelfieFront)
		{
			SetSelfieCameraOrientation(Vector3.zero, Vector3.zero);
		}
		else
		{
			SetSelfieCameraOrientation(Vector3.zero, new Vector3(0f, 180f, 0f));
		}
		_selfieStabilizer.ReachTargetInstantly();
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
			_monitorTransform.localScale = (_isThirdPersonFront ? one : vector);
			break;
		case CameraMode.FirstPerson:
		case CameraMode.Headset:
			_monitorTransform.localScale = vector;
			break;
		}
	}

	public void ProcessThirdPersonPosition()
	{
		_isThirdPersonFront = !_isThirdPersonFront;
		_justTransitioned = true;
		SetMonitorScale(CameraMode.ThirdPerson);
	}

	private void CameraModeChanged(CameraMode mode)
	{
		if (_currentCameraMode == CameraMode.Headset && mode != CameraMode.Headset)
		{
			_notificationController.HideNotifications();
		}
		_currentCameraMode = mode;
		_justTransitioned = true;
		float fov = CalculateCorrectFOV(GetCurrentModeFOV());
		SetFOV(_currentCameraMode, fov);
		SetMonitorScale(mode);
		switch (mode)
		{
		case CameraMode.Selfie:
			SetActiveLckCamera(_selfieCamera.CameraId);
			break;
		case CameraMode.FirstPerson:
			SetActiveLckCamera(_firstPersonCamera.CameraId);
			break;
		case CameraMode.ThirdPerson:
			SetActiveLckCamera(_thirdPersonCamera.CameraId);
			break;
		case CameraMode.Headset:
			SetActiveLckCamera(_headsetCamera.CameraId);
			break;
		}
		if (mode == CameraMode.Headset)
		{
			_notificationController.ShowNotification(NotificationType.HeadsetView);
		}
		OnCameraModeChanged?.Invoke(_currentCameraMode);
	}

	private void SetActiveLckCamera(string cameraId)
	{
		if (_lckService == null)
		{
			LckLog.LogWarning("LCK: SetActiveLckCamera(\"" + cameraId + "\") called before LCK service is initialized - Active camera will not be set", "SetActiveLckCamera", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 875);
			return;
		}
		LckResult lckResult = _lckService.SetActiveCamera(cameraId);
		if (!lckResult.Success)
		{
			LckLog.LogError("LCK: Failed to set active camera (id=\"" + cameraId + "\"): " + lckResult.Message, "SetActiveLckCamera", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LCKCameraController.cs", 883);
		}
	}
}
