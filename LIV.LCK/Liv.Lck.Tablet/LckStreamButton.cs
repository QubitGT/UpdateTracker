using System;
using System.Collections;
using System.Threading.Tasks;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Settings;
using Liv.Lck.Streaming;
using Liv.Lck.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Liv.Lck.Tablet;

public class LckStreamButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
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

	[Header("References")]
	[SerializeField]
	private LckStreamingController _streamingController;

	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[SerializeField]
	private TMP_Text _streamButtonText;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private RectTransform _visuals;

	[Header("Settings")]
	[SerializeField]
	private LckButtonColors _defaultColors;

	[SerializeField]
	private LckButtonColors _streamingColors;

	[SerializeField]
	private Vector3 _buttonPressedPosition = new Vector3(0f, 0f, 40f);

	private bool _collided;

	private GameObject _clickedObject;

	private State _state;

	private void Start()
	{
		if (_lckService != null)
		{
			_lckService.OnStreamingStarted += OnStreamingStarted;
			_lckService.OnStreamingStopped += OnStreamingStopped;
		}
		ValidateMeshColors();
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
		_streamButtonText.text = "00:00";
		LckResult<TimeSpan> streamDuration = _lckService.GetStreamDuration();
		if (streamDuration.Success)
		{
			TimeSpan result = streamDuration.Result;
			int num = Mathf.FloorToInt(result.Hours);
			int num2 = Mathf.FloorToInt(result.Minutes);
			int num3 = Mathf.FloorToInt(result.Seconds);
			_streamButtonText.text = ((num == 0) ? $"{num2:00}:{num3:00}" : $"{num:00}:{num2:00}:{num3:00}");
		}
	}

	[ContextMenu("test error")]
	public void OnError()
	{
		_state = State.Error;
		_streamButtonText.text = "ERROR";
		ValidateMeshColors();
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
				ValidateMeshColors();
			}
			else
			{
				_state = State.Idle;
				ValidateMeshColors();
				_streamButtonText.text = "GO LIVE";
			}
		}
		else
		{
			_state = State.Idle;
			ValidateMeshColors();
			_streamButtonText.text = "GO LIVE";
		}
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
	}

	private void OnStreamingStopped(LckResult result)
	{
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.StreamingStopped);
		if (!result.Success)
		{
			SetStoppingAnimationValue(0f);
			_streamingController.GoToErrorState();
			OnError();
			return;
		}
		SetStoppingAnimationValue(0f);
		if (_state == State.StoppingAnimationCompleted)
		{
			_state = State.WaitUntilTriggerExitOrDelay;
			WaitForTriggerExitOrDelay();
		}
		else
		{
			_state = State.Idle;
		}
		ValidateMeshColors();
		_streamButtonText.text = "GO LIVE";
	}

	private async Task WaitForTriggerExitOrDelay()
	{
		await Task.Delay(3000);
		if (_state == State.WaitUntilTriggerExitOrDelay)
		{
			_state = State.Idle;
			ValidateMeshColors();
			_streamButtonText.text = "GO LIVE";
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

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_state != State.Error)
		{
			ValidateMeshColors(isPressed: false, isHovering: true);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.HoverSound);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_state != State.Error)
		{
			if (_state == State.Streaming)
			{
				StopAllCoroutines();
				SetStoppingAnimationValue(1f);
				StartCoroutine(StoppingAnimationVisual());
			}
			ValidateMeshColors(isPressed: true);
			_visuals.anchoredPosition3D = _buttonPressedPosition;
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			if (eventData != null)
			{
				_clickedObject = eventData.pointerEnter;
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_state != State.Error)
		{
			if (_state == State.Idle)
			{
				_state = State.WaitingForStreamingStart;
				_streamButtonText.text = "STARTING...";
				_streamingController.StartStreaming();
			}
			else if (_state == State.DoingStoppingAnimation)
			{
				StopAllCoroutines();
				SetStoppingAnimationValue(1f);
				_state = State.Streaming;
			}
			else if (_state == State.WaitUntilTriggerExitOrDelay)
			{
				_state = State.Idle;
				ValidateMeshColors();
				_streamButtonText.text = "GO LIVE";
			}
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
			if (eventData != null && _clickedObject != eventData.pointerEnter)
			{
				ValidateMeshColors();
			}
			else
			{
				ValidateMeshColors(isPressed: false, isHovering: true);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_state != State.Error)
		{
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			if (_state == State.WaitUntilTriggerExitOrDelay)
			{
				_state = State.Idle;
				ValidateMeshColors();
				_streamButtonText.text = "GO LIVE";
			}
			ValidateMeshColors();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag(LckSettings.Instance.TriggerEnterTag) && IsValidTap(other.ClosestPoint(base.transform.position)) && !LCKCameraController.ColliderButtonsInUse)
		{
			LCKCameraController.ColliderButtonsInUse = true;
			_collided = true;
			OnPointerDown(null);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (_collided)
		{
			OnPointerUp(null);
			OnPointerExit(null);
			_collided = false;
			LCKCameraController.ColliderButtonsInUse = false;
		}
	}

	private bool IsValidTap(Vector3 tapPosition)
	{
		Vector3 to = tapPosition - base.transform.position;
		return Vector3.Angle(-base.transform.forward, to) < 90f;
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			_collided = false;
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			ValidateMeshColors();
		}
	}

	private void ValidateMeshColors(bool isPressed = false, bool isHovering = false)
	{
		if (!_renderer)
		{
			return;
		}
		if (_state == State.Error)
		{
			SetDefaultColor(_streamingColors.NormalColor);
			SetStreamingColor(_streamingColors.NormalColor);
		}
		else if (!isPressed)
		{
			if (!isHovering)
			{
				SetDefaultColor(_defaultColors.NormalColor);
				SetStreamingColor(_streamingColors.NormalColor);
			}
			else if (isHovering)
			{
				SetDefaultColor(_defaultColors.HighlightedColor);
				SetStreamingColor(_streamingColors.HighlightedColor);
			}
		}
		else if (isPressed)
		{
			SetDefaultColor(_defaultColors.PressedColor);
			SetStreamingColor(_streamingColors.PressedColor);
		}
	}

	private void SetStoppingAnimationValue(float value)
	{
		float value2 = Mathf.Clamp01(value);
		if (_renderer != null && _renderer.material != null)
		{
			_renderer.material.SetFloat("_ProgressValue", value2);
		}
	}

	private void SetDefaultColor(Color color)
	{
		_renderer.material.SetColor("_DefaultColor", color);
	}

	private void SetStreamingColor(Color color)
	{
		_renderer.material.SetColor("_StreamingColor", color);
	}

	private IEnumerator StoppingAnimationVisual()
	{
		float startTime = Time.time;
		float currentProgress = 1f;
		float stoppingDuration = 2f;
		_state = State.DoingStoppingAnimation;
		_streamButtonText.text = "STOPPING...";
		while (currentProgress > 0f)
		{
			float num = Time.time - startTime;
			currentProgress = Mathf.Lerp(1f, 0f, num / stoppingDuration);
			if (_renderer != null)
			{
				_renderer.material.SetFloat("_ProgressValue", currentProgress);
			}
			yield return null;
		}
		if (_renderer != null)
		{
			_renderer.material.SetFloat("_ProgressValue", 0f);
		}
		_state = State.StoppingAnimationCompleted;
		_streamingController.StopStreaming();
		ValidateMeshColors();
		_streamButtonText.text = "GO LIVE";
		_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
	}
}
