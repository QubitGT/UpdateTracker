using System;
using System.Collections;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.WitAi.TTS.UX;

public class TTSSpeakerInput : MonoBehaviour
{
	[SerializeField]
	private TTSSpeaker _speaker;

	[SerializeField]
	private InputField _input;

	[SerializeField]
	private Button _stopButton;

	[SerializeField]
	private Button _pauseButton;

	[SerializeField]
	private Button _speakButton;

	[SerializeField]
	private Toggle _queueButton;

	[SerializeField]
	private Toggle _asyncToggle;

	[SerializeField]
	private AudioClip _asyncClip;

	[SerializeField]
	private string _dateId = "[DATE]";

	[SerializeField]
	private string[] _queuedText;

	private string _voice;

	private bool _loading;

	private bool _speaking;

	private bool _paused;

	private void OnEnable()
	{
		RefreshStopButton();
		RefreshPauseButton();
		_stopButton.onClick.AddListener(StopClick);
		_pauseButton.onClick.AddListener(PauseClick);
		_speakButton.onClick.AddListener(SpeakClick);
	}

	private void StopClick()
	{
		_speaker.Stop();
	}

	private void PauseClick()
	{
		if (_speaker.IsPaused)
		{
			_speaker.Resume();
		}
		else
		{
			_speaker.Pause();
		}
	}

	private void SpeakClick()
	{
		string text = FormatText(_input.text);
		bool flag = _queueButton != null && _queueButton.isOn;
		if (_asyncToggle != null && _asyncToggle.isOn)
		{
			StartCoroutine(SpeakAsync(text, flag));
		}
		else if (flag)
		{
			_speaker.SpeakQueued(text);
		}
		else
		{
			_speaker.Speak(text);
		}
		if (_queuedText != null && _queuedText.Length != 0 && flag)
		{
			string[] queuedText = _queuedText;
			foreach (string text2 in queuedText)
			{
				_speaker.SpeakQueued(FormatText(text2));
			}
		}
	}

	private IEnumerator SpeakAsync(string phrase, bool queued)
	{
		if (queued)
		{
			yield return _speaker.SpeakQueuedAsync(new string[1] { phrase });
		}
		else
		{
			yield return _speaker.SpeakAsync(phrase);
		}
		if (_asyncClip != null)
		{
			_speaker.AudioSource.PlayOneShot(_asyncClip);
		}
	}

	private string FormatText(string text)
	{
		string text2 = text;
		if (text2.Contains(_dateId))
		{
			DateTime utcNow = DateTime.UtcNow;
			string newValue = utcNow.ToLongDateString() + " at " + utcNow.ToLongTimeString();
			text2 = text.Replace(_dateId, newValue);
		}
		return text2;
	}

	private void OnDisable()
	{
		_stopButton.onClick.RemoveListener(StopClick);
		_speakButton.onClick.RemoveListener(SpeakClick);
	}

	private void Update()
	{
		if (!string.Equals(_voice, _speaker.VoiceID))
		{
			_voice = _speaker.VoiceID;
			_input.placeholder.GetComponent<Text>().text = "Write something to say in " + _voice + "'s voice";
		}
		if (_loading != _speaker.IsLoading)
		{
			RefreshStopButton();
		}
		if (_speaking != _speaker.IsSpeaking)
		{
			RefreshStopButton();
		}
		if (_paused != _speaker.IsPaused)
		{
			RefreshPauseButton();
		}
	}

	private void RefreshStopButton()
	{
		_loading = _speaker.IsLoading;
		_speaking = _speaker.IsSpeaking;
		_stopButton.interactable = _loading || _speaking;
	}

	private void RefreshPauseButton()
	{
		_paused = _speaker.IsPaused;
		_pauseButton.GetComponentInChildren<Text>().text = (_paused ? "Resume" : "Pause");
	}
}
