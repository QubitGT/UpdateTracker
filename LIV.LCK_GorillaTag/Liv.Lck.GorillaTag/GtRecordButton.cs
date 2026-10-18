using System;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Recorder;
using TMPro;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtRecordButton : MonoBehaviour
{
	private enum State
	{
		Idle,
		Recording,
		Saving,
		Error
	}

	[InjectLck]
	private ILckService _lckService;

	[Space(10f)]
	[Header("Global Settings")]
	[SerializeField]
	private GtUiSettings _settings;

	[Space(10f)]
	[Header("Parameters")]
	[SerializeField]
	private string _name;

	[SerializeField]
	private TextMeshPro _label;

	[SerializeField]
	private MeshRenderer _bodyRenderer;

	[SerializeField]
	private Transform _visualsTrans;

	[SerializeField]
	private LckDiscreetAudioController _audioController;

	private const string _idleString = "RECORD";

	private const string _savingString = "SAVING";

	private const string _errorString = "ERROR";

	private bool _isDisabled;

	private Vector3 _defaultLocalPosition;

	private State _currentState;

	public event Action onPressed;

	private void Start()
	{
		InitSetUp();
	}

	private void InitSetUp()
	{
		_defaultLocalPosition = _visualsTrans.localPosition;
		SetUp();
		UpdateVisualState();
	}

	private void UpdateVisualState()
	{
		_bodyRenderer.material = ((_currentState == State.Recording) ? _settings.RecordingBodyMaterial : _settings.DefaultBodyMaterial);
		_label.color = ((_currentState == State.Recording) ? _settings.SecondaryTextColor : _settings.PrimaryTextColor);
		switch (_currentState)
		{
		case State.Idle:
			_label.text = "RECORD";
			break;
		case State.Saving:
			_label.text = "SAVING";
			break;
		case State.Error:
			_label.text = "ERROR";
			_label.color = _settings.SecondaryTextColor;
			_bodyRenderer.material = _settings.RecordingBodyMaterial;
			break;
		default:
			_label.text = "00:00";
			break;
		}
	}

	private void SetDisabled(bool isDisabled)
	{
		_isDisabled = isDisabled;
		_visualsTrans.localPosition = _defaultLocalPosition;
	}

	private void OnError()
	{
		_currentState = State.Error;
		UpdateVisualState();
		SetDisabled(isDisabled: true);
		ResetAfterError();
	}

	private async Task ResetAfterError()
	{
		await Task.Delay(2000);
		SetDisabled(isDisabled: false);
		if (_lckService != null)
		{
			if (_lckService.IsRecording().Result)
			{
				_currentState = State.Recording;
			}
			else
			{
				_currentState = State.Idle;
			}
		}
		else
		{
			_currentState = State.Idle;
		}
		UpdateVisualState();
	}

	private void SetUp()
	{
		UpdateVisualState();
		_label.text = _name.ToUpper();
		_lckService.OnRecordingStarted += OnRecordingStarted;
		_lckService.OnRecordingStopped += OnRecordingStopped;
		_lckService.OnRecordingSaved += OnRecordingSaved;
	}

	private void OnRecordingSaved(LckResult<RecordingData> result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		_currentState = State.Idle;
		UpdateVisualState();
		_visualsTrans.localPosition = _defaultLocalPosition;
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.RecordingSaved);
	}

	private void OnDestroy()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnRecordingStarted;
			_lckService.OnRecordingStopped -= OnRecordingStopped;
			_lckService.OnRecordingSaved -= OnRecordingSaved;
		}
	}

	private void OnRecordingStopped(LckResult result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		_currentState = State.Saving;
		UpdateVisualState();
		_visualsTrans.localPosition = _defaultLocalPosition + Vector3.forward * (0f - _settings.ActiveButtonOffset);
	}

	private void OnRecordingStarted(LckResult result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		_currentState = State.Recording;
		UpdateVisualState();
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.RecordingStart);
	}

	public void TapStarted()
	{
		if (_currentState != State.Saving && !_isDisabled)
		{
			_visualsTrans.localPosition = _defaultLocalPosition + Vector3.forward * (0f - _settings.ActiveButtonOffset);
			this.onPressed();
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
		}
	}

	public void TapEnded()
	{
		if (_currentState != State.Saving && !_isDisabled)
		{
			_visualsTrans.localPosition = _defaultLocalPosition;
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
		}
	}

	private void Update()
	{
		LckResult<bool> lckResult = _lckService.IsRecording();
		if (_currentState == State.Recording && lckResult.Success && lckResult.Result)
		{
			UpdateRecordDurationText();
		}
	}

	private void UpdateRecordDurationText()
	{
		TimeSpan result = _lckService.GetRecordingDuration().Result;
		int num = Mathf.FloorToInt(result.Hours);
		int num2 = Mathf.FloorToInt(result.Minutes);
		int num3 = Mathf.FloorToInt(result.Seconds);
		_label.text = ((num == 0) ? $"{num2:00}:{num3:00}" : $"{num:00}:{num2:00}:{num3:00}");
	}
}
