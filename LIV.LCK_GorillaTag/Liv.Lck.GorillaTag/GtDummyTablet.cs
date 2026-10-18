using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtDummyTablet : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private bool _isLCKWallCameraSpawner;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private GameObject _body;

	[SerializeField]
	private GameObject _ghostBody;

	[SerializeField]
	private Material _defaultMaterial;

	[SerializeField]
	private Material _recordingMaterial;

	private GameObject _cosmeticTablet;

	private GameObject _cosmeticEmobi;

	[SerializeField]
	private int _recordingButtonIndex;

	[SerializeField]
	private GameObject _recordingIndicator;

	private bool _isCapturing;

	private void OnEnable()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted += OnCaptureStarted;
			_lckService.OnRecordingStopped += OnCaptureStopped;
			_lckService.OnStreamingStarted += OnCaptureStarted;
			_lckService.OnStreamingStopped += OnCaptureStopped;
		}
	}

	public void OnTabletCosmeticSpawned(GameObject cosmetic)
	{
		if (_isLCKWallCameraSpawner)
		{
			cosmetic.SetActive(value: true);
		}
		else
		{
			cosmetic.SetActive(!_ghostBody.activeSelf);
		}
		_cosmeticTablet = cosmetic;
	}

	public void OnEmobiCosmeticSpawned(GameObject cosmetic)
	{
		if (_isLCKWallCameraSpawner)
		{
			cosmetic.SetActive(value: true);
		}
		else
		{
			cosmetic.SetActive(!_ghostBody.activeSelf);
		}
		_cosmeticEmobi = cosmetic;
	}

	public void SetDummyTabletBodyState(bool isActive)
	{
		if (!_isLCKWallCameraSpawner)
		{
			_ghostBody.SetActive(!isActive);
			if (_recordingIndicator != null)
			{
				_recordingIndicator.SetActive(isActive);
			}
			if (_cosmeticTablet != null && _cosmeticEmobi != null)
			{
				_body.SetActive(value: false);
				_cosmeticTablet.SetActive(isActive);
				_cosmeticEmobi.SetActive(isActive);
			}
			else
			{
				_body.SetActive(isActive);
			}
		}
	}

	private void OnDisable()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnCaptureStarted;
			_lckService.OnRecordingStopped -= OnCaptureStopped;
			_lckService.OnStreamingStarted -= OnCaptureStarted;
			_lckService.OnStreamingStopped -= OnCaptureStopped;
		}
	}

	private void Start()
	{
		SetState(_isCapturing);
	}

	public void SetState(bool isCapturing)
	{
		_isCapturing = isCapturing;
		Material[] materials = _renderer.materials;
		materials[_recordingButtonIndex] = (_isCapturing ? _recordingMaterial : _defaultMaterial);
		_renderer.materials = materials;
	}

	private void OnCaptureStarted(LckResult result)
	{
		if (result.Success)
		{
			SetState(isCapturing: true);
		}
	}

	private void OnCaptureStopped(LckResult result)
	{
		SetState(isCapturing: false);
	}
}
