using System;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Recorder;
using TMPro;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtSaveEchoButton : MonoBehaviour
{
	private enum State
	{
		EchoStarting,
		Ready,
		LowStorage,
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

	private const string _echoStartingString = "ECHO STARTING...";

	private const string _lowStorageString = "LOW STORAGE";

	private const string _errorString = "ERROR";

	private const int EchoStartingPeriodSeconds = 2;

	private int _lastDisplayedSeconds = -1;

	private int _maxBufferSeconds;

	private bool _shouldPollEchoDuration;

	private bool _isDisabled;

	private Vector3 _defaultLocalPosition;

	private State _currentState;

	public event Action onPressed;

	private void Start()
	{
		_defaultLocalPosition = _visualsTrans.localPosition;
		if (_lckService != null)
		{
			_lckService.OnEchoEnabled += OnEchoEnabled;
			_lckService.OnEchoDisabled += OnEchoDisabled;
			_lckService.OnEchoSaved += OnEchoSaved;
			LckResult<bool> lckResult = _lckService.IsEchoEnabled();
			if (lckResult.Success && lckResult.Result)
			{
				StartEchoPolling();
			}
		}
	}

	private void UpdateVisualState()
	{
		_bodyRenderer.material = _settings.DefaultBodyMaterial;
		_label.color = _settings.PrimaryTextColor;
		switch (_currentState)
		{
		case State.EchoStarting:
			_label.text = "ECHO STARTING...";
			_label.color = _settings.DisabledTextColor;
			break;
		case State.LowStorage:
			_label.text = "LOW STORAGE";
			_label.color = _settings.DisabledTextColor;
			break;
		case State.Error:
			_label.text = "ERROR";
			_label.color = _settings.DisabledTextColor;
			break;
		case State.Ready:
			break;
		}
	}

	private void SetDisabled(bool isDisabled)
	{
		_isDisabled = isDisabled;
		_visualsTrans.localPosition = _defaultLocalPosition;
	}

	public void TapStarted()
	{
		if (!_isDisabled)
		{
			_visualsTrans.localPosition = _defaultLocalPosition + Vector3.forward * (0f - _settings.ActiveButtonOffset);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			this.onPressed?.Invoke();
		}
	}

	public void TapEnded()
	{
		if (!_isDisabled)
		{
			_visualsTrans.localPosition = _defaultLocalPosition;
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
		}
	}

	private void OnEchoEnabled(LckResult result)
	{
		if (result.Success)
		{
			StartEchoPolling();
		}
	}

	private void StartEchoPolling()
	{
		if (_currentState != State.Error)
		{
			LckResult<TimeSpan> echoMaxBufferDuration = _lckService.GetEchoMaxBufferDuration();
			_maxBufferSeconds = (echoMaxBufferDuration.Success ? ((int)echoMaxBufferDuration.Result.TotalSeconds) : 0);
			_currentState = State.EchoStarting;
			_lastDisplayedSeconds = -1;
			UpdateVisualState();
			SetDisabled(isDisabled: true);
			_shouldPollEchoDuration = true;
		}
	}

	private void OnEchoDisabled(LckResult result, EchoDisableReason reason)
	{
		switch (reason)
		{
		case EchoDisableReason.LowStorage:
			_shouldPollEchoDuration = false;
			_currentState = State.LowStorage;
			UpdateVisualState();
			SetDisabled(isDisabled: true);
			return;
		case EchoDisableReason.Error:
			OnError();
			return;
		}
		if (_currentState != State.Error && _currentState != State.LowStorage)
		{
			_shouldPollEchoDuration = false;
			_currentState = State.EchoStarting;
			_lastDisplayedSeconds = -1;
			UpdateVisualState();
			SetDisabled(isDisabled: true);
		}
	}

	private void OnError()
	{
		_shouldPollEchoDuration = false;
		_currentState = State.Error;
		UpdateVisualState();
		SetDisabled(isDisabled: true);
		ResetAfterError();
	}

	private async Task ResetAfterError()
	{
		await Task.Delay(2000);
		_lastDisplayedSeconds = -1;
		if (_lckService == null)
		{
			_currentState = State.EchoStarting;
			UpdateVisualState();
			SetDisabled(isDisabled: true);
			return;
		}
		_currentState = State.EchoStarting;
		UpdateVisualState();
		SetDisabled(isDisabled: true);
		if (!(await _lckService.SetEchoEnabledAsync(enabled: true)).Success)
		{
			OnError();
		}
	}

	private void UpdateBufferDurationText()
	{
		LckResult<TimeSpan> echoBufferDuration = _lckService.GetEchoBufferDuration();
		if (!echoBufferDuration.Success)
		{
			return;
		}
		int num = Math.Min((int)echoBufferDuration.Result.TotalSeconds, _maxBufferSeconds);
		if (num > _lastDisplayedSeconds)
		{
			_lastDisplayedSeconds = num;
			if (num < 2)
			{
				_currentState = State.EchoStarting;
				UpdateVisualState();
				SetDisabled(isDisabled: true);
			}
			else
			{
				_currentState = State.Ready;
				UpdateVisualState();
				_label.text = $"SAVE LAST\n{num} SECONDS";
				SetDisabled(isDisabled: false);
			}
			if (num >= _maxBufferSeconds)
			{
				_shouldPollEchoDuration = false;
			}
		}
	}

	private void OnEchoSaved(LckResult<RecordingData> result)
	{
		if (result.Success)
		{
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.RecordingSaved);
		}
	}

	private void OnEnable()
	{
		if (_lckService != null)
		{
			LckResult<bool> lckResult = _lckService.IsEchoEnabled();
			if (lckResult.Success && lckResult.Result)
			{
				LckResult<TimeSpan> echoMaxBufferDuration = _lckService.GetEchoMaxBufferDuration();
				_maxBufferSeconds = (echoMaxBufferDuration.Success ? ((int)echoMaxBufferDuration.Result.TotalSeconds) : 0);
				_lastDisplayedSeconds = -1;
				_shouldPollEchoDuration = true;
			}
		}
		UpdateVisualState();
		SetDisabled(_currentState != State.Ready);
	}

	private void OnDisable()
	{
		_shouldPollEchoDuration = false;
	}

	private void OnDestroy()
	{
		if (_lckService != null)
		{
			_lckService.OnEchoEnabled -= OnEchoEnabled;
			_lckService.OnEchoDisabled -= OnEchoDisabled;
			_lckService.OnEchoSaved -= OnEchoSaved;
		}
	}

	private void Update()
	{
		if (_shouldPollEchoDuration && _lckService != null)
		{
			UpdateBufferDurationText();
		}
	}
}
