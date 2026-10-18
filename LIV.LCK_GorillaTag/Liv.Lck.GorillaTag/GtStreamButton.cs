using System;
using System.Collections;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Streaming;
using TMPro;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtStreamButton : MonoBehaviour
{
	private enum State
	{
		Idle,
		WaitingForStreamingStart,
		Streaming,
		DoingStoppingAnimation,
		StoppingAnimationCompleted,
		WaitUntilTriggerExitOrDelay,
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

	[SerializeField]
	private LckStreamingController _streamingController;

	[Header("Parameters")]
	[SerializeField]
	private Color _defaultColor;

	[SerializeField]
	private Color _streamingColor;

	private const string _idleString = "GO LIVE";

	private Vector3 _defaultLocalPosition;

	private bool _isDisabled;

	private State _state;

	private void Start()
	{
		_defaultLocalPosition = _visualsTrans.localPosition;
		UpdateVisualState();
		if (_lckService != null)
		{
			_lckService.OnStreamingStarted += OnStreamingStarted;
			_lckService.OnStreamingStopped += OnStreamingStopped;
		}
	}

	private void Update()
	{
		if (_state == State.Streaming && _lckService != null)
		{
			UpdateStreamDurationText();
		}
	}

	private void UpdateStreamDurationText()
	{
		_label.text = "00:00";
		LckResult<TimeSpan> streamDuration = _lckService.GetStreamDuration();
		if (streamDuration.Success)
		{
			TimeSpan result = streamDuration.Result;
			int num = Mathf.FloorToInt(result.Hours);
			int num2 = Mathf.FloorToInt(result.Minutes);
			int num3 = Mathf.FloorToInt(result.Seconds);
			_label.text = ((num == 0) ? $"{num2:00}:{num3:00}" : $"{num:00}:{num2:00}:{num3:00}");
		}
	}

	private void UpdateVisualState()
	{
		switch (_state)
		{
		case State.Idle:
			_label.text = "GO LIVE";
			SetDefaultColor(_defaultColor);
			SetStreamingColor(_streamingColor);
			_label.color = _settings.PrimaryTextColor;
			SetStoppingAnimationValue(0f);
			break;
		case State.WaitingForStreamingStart:
			_label.text = "STARTING...";
			break;
		case State.Streaming:
			SetDefaultColor(_defaultColor);
			SetStreamingColor(_streamingColor);
			_label.color = _settings.SecondaryTextColor;
			break;
		case State.DoingStoppingAnimation:
			_label.text = "STOPPING...";
			_label.color = _settings.PrimaryTextColor;
			break;
		case State.StoppingAnimationCompleted:
			_label.text = "GO LIVE";
			SetDefaultColor(_defaultColor);
			SetStreamingColor(_streamingColor);
			_label.color = _settings.PrimaryTextColor;
			break;
		case State.Error:
			_label.text = "ERROR";
			SetDefaultColor(_streamingColor);
			SetStreamingColor(_streamingColor);
			_label.color = _settings.SecondaryTextColor;
			break;
		case State.WaitUntilTriggerExitOrDelay:
			break;
		}
	}

	private void SetDefaultColor(Color color)
	{
		_bodyRenderer.material.SetColor("_DefaultColor", color);
	}

	private void SetStreamingColor(Color color)
	{
		_bodyRenderer.material.SetColor("_StreamingColor", color);
	}

	private IEnumerator StoppingAnimationVisual()
	{
		float startTime = Time.time;
		float currentProgress = 1f;
		float stoppingDuration = 2.5f;
		_state = State.DoingStoppingAnimation;
		UpdateVisualState();
		while (currentProgress > 0f)
		{
			float num = Time.time - startTime;
			_label.color = Color.Lerp(_settings.PrimaryTextColor, _settings.SecondaryTextColor, currentProgress);
			currentProgress = Mathf.Lerp(1f, 0f, num / stoppingDuration);
			if (_bodyRenderer != null)
			{
				_bodyRenderer.material.SetFloat("_ProgressValue", currentProgress);
			}
			yield return null;
		}
		if (_bodyRenderer != null)
		{
			_bodyRenderer.material.SetFloat("_ProgressValue", 0f);
		}
		_state = State.StoppingAnimationCompleted;
		_streamingController.StopStreaming();
		UpdateVisualState();
	}

	private void SetStoppingAnimationValue(float value)
	{
		float value2 = Mathf.Clamp01(value);
		if (_bodyRenderer != null && _bodyRenderer.material != null)
		{
			_bodyRenderer.material.SetFloat("_ProgressValue", value2);
		}
	}

	private void SetDisabled(bool isDisabled)
	{
		_isDisabled = isDisabled;
		_visualsTrans.localPosition = _defaultLocalPosition;
	}

	[ContextMenu("test error")]
	public void OnError()
	{
		_state = State.Error;
		UpdateVisualState();
		_visualsTrans.localPosition = _defaultLocalPosition;
		SetDisabled(isDisabled: true);
		ResetAfterError();
	}

	private async Task ResetAfterError()
	{
		await Task.Delay(2000);
		if (_lckService != null)
		{
			if (_lckService.IsStreaming().Result)
			{
				_state = State.Streaming;
			}
			else
			{
				_state = State.Idle;
			}
		}
		else
		{
			_state = State.Idle;
		}
		SetDisabled(isDisabled: false);
		UpdateVisualState();
	}

	private void OnStreamingStarted(LckResult result)
	{
		if (!result.Success)
		{
			OnError();
			return;
		}
		SetStoppingAnimationValue(1f);
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.StreamingStarted);
		_state = State.Streaming;
		UpdateVisualState();
	}

	private void OnStreamingStopped(LckResult result)
	{
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.StreamingStopped);
		if (!result.Success || _state != State.StoppingAnimationCompleted)
		{
			_streamingController.GoToErrorState();
			OnError();
		}
		else
		{
			_state = State.WaitUntilTriggerExitOrDelay;
			WaitForTriggerExitOrDelay();
		}
	}

	private async Task WaitForTriggerExitOrDelay()
	{
		await Task.Delay(3000);
		if (_state == State.WaitUntilTriggerExitOrDelay)
		{
			_state = State.Idle;
			UpdateVisualState();
		}
	}

	private void OnDestroy()
	{
		if (_lckService != null)
		{
			_lckService.OnStreamingStarted -= OnStreamingStarted;
			_lckService.OnStreamingStopped -= OnStreamingStopped;
		}
	}

	public void TapStarted()
	{
		if (_state != State.Error && !_isDisabled)
		{
			if (_state == State.Streaming)
			{
				StopAllCoroutines();
				SetStoppingAnimationValue(1f);
				StartCoroutine(StoppingAnimationVisual());
			}
			else if (_state == State.Idle)
			{
				_state = State.WaitingForStreamingStart;
				_streamingController.StartStreaming();
				UpdateVisualState();
			}
			_visualsTrans.localPosition = _defaultLocalPosition + Vector3.forward * (0f - _settings.ActiveButtonOffset);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
		}
	}

	public void TapEnded()
	{
		if (_state != State.Error && !_isDisabled)
		{
			if (_state == State.DoingStoppingAnimation)
			{
				StopAllCoroutines();
				SetStoppingAnimationValue(1f);
				_state = State.Streaming;
			}
			if (_state == State.WaitUntilTriggerExitOrDelay)
			{
				_state = State.Idle;
			}
			UpdateVisualState();
			_visualsTrans.localPosition = _defaultLocalPosition;
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
		}
	}
}
