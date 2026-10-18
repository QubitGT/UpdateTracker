using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtSelector : MonoBehaviour
{
	[Header("Global Settings")]
	[SerializeField]
	private GtUiSettings _settings;

	[Space(10f)]
	[Header("Parameters")]
	[SerializeField]
	private CameraMode _mode;

	[SerializeField]
	private SelectorState _state;

	[Header("Elements")]
	[SerializeField]
	private MeshRenderer _bodyRenderer;

	[SerializeField]
	private SpriteRenderer _iconRenderer;

	[SerializeField]
	private TextMeshPro _textMesh;

	[SerializeField]
	private Transform _visualsTrans;

	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[HideInInspector]
	public UnityEvent<CameraMode> onCameraModeUpdate;

	private Vector3 _defaultLocalPosition;

	private void OnValidate()
	{
		EvaluateCameraMode(_mode);
	}

	private void Awake()
	{
		InitSetUp();
	}

	private void Start()
	{
		EvaluateState(_state);
		EvaluateCameraMode(_mode);
	}

	public void TapStarted()
	{
		onCameraModeUpdate.Invoke(_mode);
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	public void ListenToCameraModeChanged(CameraMode mode)
	{
		EvaluateState((mode == _mode) ? SelectorState.Selected : SelectorState.Default);
	}

	private void InitSetUp()
	{
		_defaultLocalPosition = _visualsTrans.localPosition;
	}

	private void EvaluateCameraMode(CameraMode mode)
	{
		switch (mode)
		{
		case CameraMode.Selfie:
			_iconRenderer.sprite = _settings.SelfieModeAsset.Icon;
			_textMesh.text = _settings.SelfieModeAsset.Name.ToUpper();
			break;
		case CameraMode.FirstPerson:
			_iconRenderer.sprite = _settings.FirstPersonModeAsset.Icon;
			_textMesh.text = _settings.FirstPersonModeAsset.Name.ToUpper();
			break;
		case CameraMode.ThirdPerson:
			_iconRenderer.sprite = _settings.ThirdPersonModeAsset.Icon;
			_textMesh.text = _settings.ThirdPersonModeAsset.Name.ToUpper();
			break;
		case CameraMode.Headset:
			_iconRenderer.sprite = _settings.HeadsetModeAsset.Icon;
			_textMesh.text = _settings.HeadsetModeAsset.Name?.ToUpper() ?? "ECO";
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void EvaluateState(SelectorState state)
	{
		switch (state)
		{
		case SelectorState.Default:
			SetDefaultState();
			break;
		case SelectorState.Selected:
			SetSelectedState();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetDefaultState()
	{
		_bodyRenderer.material = _settings.DefaultBodyMaterial;
		_iconRenderer.color = _settings.PrimaryIconColor;
		_textMesh.color = _settings.PrimaryTextColor;
		_visualsTrans.localPosition = _defaultLocalPosition;
	}

	private void SetSelectedState()
	{
		_bodyRenderer.material = _settings.SelectedBodyMaterial;
		_iconRenderer.color = _settings.PrimaryIconColor;
		_textMesh.color = _settings.PrimaryTextColor;
		_visualsTrans.localPosition = _defaultLocalPosition - new Vector3(0f, 0f, _settings.ActiveButtonOffset);
	}
}
