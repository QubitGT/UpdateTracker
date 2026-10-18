using System;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Recorder;
using Liv.Lck.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Liv.Lck.Tablet;

public class LckRecordButton : MonoBehaviour
{
	private enum State
	{
		Idle,
		Saving,
		Paused,
		Recording,
		Error
	}

	[InjectLck]
	private ILckService _lckService;

	[Header("References")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[SerializeField]
	private TMP_Text _recordButtonText;

	[SerializeField]
	private LckToggle _recordLckToggle;

	[SerializeField]
	private Toggle _recordToggle;

	[Header("Toggle collider when using Direct Tablet")]
	[SerializeField]
	private BoxCollider _collider;

	private State _state;

	private void Start()
	{
		EnsureLckService();
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted += OnRecordingStarted;
			_lckService.OnRecordingStopped += OnRecordingStopped;
			_lckService.OnRecordingPaused += OnRecordingPaused;
			_lckService.OnRecordingResumed += OnRecordingResumed;
			_lckService.OnRecordingSaved += OnRecordingSaved;
		}
	}

	private void Update()
	{
		EnsureLckService();
		if (_state == State.Recording && _lckService != null)
		{
			UpdateRecordDurationText();
		}
	}

	private void UpdateRecordDurationText()
	{
		LckResult<TimeSpan> recordingDuration = _lckService.GetRecordingDuration();
		if (recordingDuration.Success)
		{
			TimeSpan result = recordingDuration.Result;
			int num = Mathf.FloorToInt(result.Hours);
			int num2 = Mathf.FloorToInt(result.Minutes);
			int num3 = Mathf.FloorToInt(result.Seconds);
			_recordButtonText.text = ((num == 0) ? $"{num2:00}:{num3:00}" : $"{num:00}:{num2:00}:{num3:00}");
		}
	}

	private void OnError()
	{
		_state = State.Error;
		_recordButtonText.text = "ERROR";
		_recordLckToggle.enabled = false;
		_recordToggle.interactable = false;
		if ((bool)_collider)
		{
			_collider.enabled = false;
		}
		ResetAfterError();
	}

	private async Task ResetAfterError()
	{
		await Task.Delay(2000);
		_state = State.Idle;
		if ((bool)_collider)
		{
			_collider.enabled = true;
		}
		ResetButtonVisuals();
	}

	private void OnRecordingStarted(LckResult result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.RecordingStart);
		_state = State.Recording;
	}

	private void OnRecordingPaused(LckResult result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		_state = State.Paused;
		if (!(_recordButtonText == null) && !(_recordLckToggle == null))
		{
			_recordButtonText.text = "PAUSED";
		}
	}

	private void OnRecordingResumed(LckResult result)
	{
		if (!result.Success)
		{
			OnError();
		}
		else
		{
			_state = State.Recording;
		}
	}

	private void OnRecordingStopped(LckResult result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		_state = State.Saving;
		if (!(_recordButtonText == null) && !(_recordLckToggle == null))
		{
			_recordButtonText.text = "SAVING";
			_recordLckToggle.SetToggleVisualsOff();
			_recordLckToggle.enabled = false;
			_recordToggle.interactable = false;
		}
	}

	private void OnRecordingSaved(LckResult<RecordingData> result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		_state = State.Idle;
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.RecordingSaved);
		if (!(_recordButtonText == null) && !(_recordLckToggle == null))
		{
			ResetButtonVisuals();
		}
	}

	private void ResetButtonVisuals()
	{
		_recordButtonText.text = "RECORD";
		_recordLckToggle.enabled = true;
		_recordToggle.interactable = true;
		_recordLckToggle.SetToggleVisualsOff();
	}

	private void EnsureLckService()
	{
		if (_lckService == null)
		{
			LckLog.LogWarning("LCK Could not get Service", "EnsureLckService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckRecordButton.cs", 199);
		}
	}

	private void OnDestroy()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnRecordingStarted;
			_lckService.OnRecordingStopped -= OnRecordingStopped;
			_lckService.OnRecordingPaused -= OnRecordingPaused;
			_lckService.OnRecordingResumed -= OnRecordingResumed;
			_lckService.OnRecordingSaved -= OnRecordingSaved;
		}
	}
}
