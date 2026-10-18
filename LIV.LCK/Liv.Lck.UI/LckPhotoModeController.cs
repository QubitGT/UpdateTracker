using System.Collections;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Tablet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckPhotoModeController : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[Tooltip("The UI Image component that creates the 'flash' effect when a photo is taken. It should cover the entire screen.")]
	[SerializeField]
	private Image _photoFlash;

	[Tooltip("The parent GameObject for the countdown UI elements. This will be enabled and disabled by the controller.")]
	[SerializeField]
	private GameObject _countdownBG;

	[Tooltip("The TextMeshPro text element used to display the '3, 2, 1' countdown.")]
	[SerializeField]
	private TMP_Text _countdownText;

	[Tooltip("The duration in seconds for the flash effect to fade out.")]
	[SerializeField]
	private float _fadeOutDuration = 0.5f;

	[Tooltip("A short delay in seconds after the flash appears before it begins to fade out.")]
	[SerializeField]
	private float _delayBeforeFade = 0.3f;

	[Tooltip("A reference to the controller responsible for playing short, non-diegetic audio clips like beeps and shutter sounds.")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[Tooltip("A reference to the controller used to show the 'Photo Saved' notification after the sequence is complete.")]
	[SerializeField]
	private LckNotificationController _notificationController;

	[Tooltip("This event is invoked at the exact moment the photo is captured. Use it to temporarily disable UI buttons or trigger other game-specific logic such as a flash light being enabled.")]
	[SerializeField]
	private UnityEvent _onPhotoCaptured;

	[Tooltip("The starting alpha (opacity) of the flash effect. A value of 0.9 is recommended over 1.0 to avoid a harsh, fully opaque flash.")]
	private float _flashAlpha = 0.9f;

	private void Start()
	{
		_photoFlash.gameObject.SetActive(value: false);
		_countdownBG.SetActive(value: false);
	}

	private void OnEnable()
	{
		_lckService.OnRecordingStarted += OnRecordingStarted;
	}

	private void OnDisable()
	{
		_lckService.OnRecordingStarted -= OnRecordingStarted;
		StopAndResetSequence();
	}

	private void OnRecordingStarted(LckResult result)
	{
		StopAndResetSequence();
	}

	public void PlayPhotoSequence()
	{
		StopAndResetSequence();
		StartCoroutine(CountdownSequence());
	}

	public void StopAndResetSequence()
	{
		StopAllCoroutines();
		ResetFlashVisuals();
		ResetCountdownVisuals();
	}

	private void ResetFlashVisuals()
	{
		_photoFlash.gameObject.SetActive(value: false);
		Color color = _photoFlash.color;
		color.a = _flashAlpha;
		_photoFlash.color = color;
	}

	private void ResetCountdownVisuals()
	{
		_countdownBG.SetActive(value: false);
	}

	private IEnumerator CountdownSequence()
	{
		_countdownText.text = "3";
		_countdownBG.SetActive(value: true);
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ScreenshotBeepSound);
		yield return new WaitForSeconds(1f);
		_countdownText.text = "2";
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ScreenshotBeepSound);
		yield return new WaitForSeconds(1f);
		_countdownText.text = "1";
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ScreenshotBeepSound);
		yield return new WaitForSeconds(1f);
		_countdownBG.SetActive(value: false);
		_onPhotoCaptured.Invoke();
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(FadeSequence());
	}

	private IEnumerator FadeSequence()
	{
		_lckService.CapturePhoto();
		_photoFlash.gameObject.SetActive(value: true);
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.CameraShutterSound);
		yield return new WaitForSeconds(_delayBeforeFade);
		yield return StartCoroutine(FadeImageAlpha(_flashAlpha, 0f, _fadeOutDuration));
		_photoFlash.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(0.5f);
		_notificationController.ShowNotification(NotificationType.PhotoSaved);
	}

	private IEnumerator FadeImageAlpha(float startAlpha, float endAlpha, float duration)
	{
		float elapsedTime = 0f;
		Color currentColor = _photoFlash.color;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / duration);
			currentColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
			_photoFlash.color = currentColor;
			yield return null;
		}
		currentColor.a = endAlpha;
		_photoFlash.color = currentColor;
	}
}
