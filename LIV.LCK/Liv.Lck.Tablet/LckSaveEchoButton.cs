using System;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Recorder;
using Liv.Lck.UI;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckSaveEchoButton : MonoBehaviour
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

	[SerializeField]
	private LckButton _button;

	[SerializeField]
	private LckDiscreetAudioController _audioController;

	private const string _echoStartingString = "ECHO STARTING...";

	private const string _lowStorageString = "LOW STORAGE";

	private const string _errorString = "ERROR";

	private const int EchoStartingPeriodSeconds = 2;

	private int _lastDisplayedSeconds = -1;

	private int _maxBufferSeconds;

	private bool _shouldPollEchoDuration;

	private State _state;

	private void OnEchoEnabled(LckResult result)
	{
		if (result.Success)
		{
			StartEchoPolling();
		}
	}

	private void StartEchoPolling()
	{
		if (_state != State.Error)
		{
			LckResult<TimeSpan> echoMaxBufferDuration = _lckService.GetEchoMaxBufferDuration();
			_maxBufferSeconds = (echoMaxBufferDuration.Success ? ((int)echoMaxBufferDuration.Result.TotalSeconds) : 0);
			_state = State.EchoStarting;
			_lastDisplayedSeconds = -1;
			UpdateVisualState();
			_shouldPollEchoDuration = true;
		}
	}

	private void OnEchoDisabled(LckResult result, EchoDisableReason reason)
	{
		switch (reason)
		{
		case EchoDisableReason.LowStorage:
			_shouldPollEchoDuration = false;
			_state = State.LowStorage;
			UpdateVisualState();
			return;
		case EchoDisableReason.Error:
			OnError();
			return;
		}
		if (_state != State.Error && _state != State.LowStorage)
		{
			_shouldPollEchoDuration = false;
			_state = State.EchoStarting;
			_lastDisplayedSeconds = -1;
			UpdateVisualState();
		}
	}

	private void OnError()
	{
		_shouldPollEchoDuration = false;
		_state = State.Error;
		UpdateVisualState();
		ResetAfterError();
	}

	private async Task ResetAfterError()
	{
		await Task.Delay(2000);
		_lastDisplayedSeconds = -1;
		if (_lckService == null)
		{
			_state = State.EchoStarting;
			UpdateVisualState();
			return;
		}
		_state = State.EchoStarting;
		UpdateVisualState();
		if (!(await _lckService.SetEchoEnabledAsync(enabled: true)).Success)
		{
			OnError();
		}
	}

	private void UpdateVisualState()
	{
		if (!(_button == null))
		{
			switch (_state)
			{
			case State.EchoStarting:
				_button.SetLabelText("ECHO STARTING...");
				_button.SetIsDisabled(isDisabled: true);
				break;
			case State.Ready:
				_button.SetIsDisabled(isDisabled: false);
				break;
			case State.LowStorage:
				_button.SetLabelText("LOW STORAGE");
				_button.SetIsDisabled(isDisabled: true);
				break;
			case State.Error:
				_button.SetLabelText("ERROR");
				_button.SetIsDisabled(isDisabled: true);
				break;
			}
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
				_state = State.EchoStarting;
				UpdateVisualState();
			}
			else
			{
				_state = State.Ready;
				UpdateVisualState();
				_button.SetLabelText($"SAVE LAST\n{num} SECONDS");
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

	private void EnsureLckService()
	{
		if (_lckService == null)
		{
			LckLog.LogWarning("LCK Could not get Service", "EnsureLckService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckSaveEchoButton.cs", 189);
		}
	}

	private void Start()
	{
		EnsureLckService();
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
		EnsureLckService();
		if (_shouldPollEchoDuration && _lckService != null)
		{
			UpdateBufferDurationText();
		}
	}
}
