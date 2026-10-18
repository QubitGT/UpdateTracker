using System.Collections.Generic;
using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck;

public class LckDiscreetAudioController : MonoBehaviour
{
	private struct AudioClipAndVolume
	{
		public UnityEngine.AudioClip clip;

		public float volume;

		public AudioClipAndVolume(UnityEngine.AudioClip clip, float volume = 0.2f)
		{
			this.clip = clip;
			this.volume = volume;
		}
	}

	public enum AudioClip
	{
		RecordingStart,
		RecordingSaved,
		ClickDown,
		ClickUp,
		HoverSound,
		CameraShutterSound,
		ScreenshotBeepSound,
		StreamingStarted,
		StreamingStopped
	}

	[InjectLck]
	private ILckService _lckService;

	private Dictionary<AudioClip, AudioClipAndVolume> _allAudioClips = new Dictionary<AudioClip, AudioClipAndVolume>();

	[Header("Audio Clips")]
	[SerializeField]
	private UnityEngine.AudioClip _recordingStart;

	[SerializeField]
	private UnityEngine.AudioClip _recordingSaved;

	[SerializeField]
	private UnityEngine.AudioClip _clickDown;

	[SerializeField]
	private UnityEngine.AudioClip _clickUp;

	[SerializeField]
	private UnityEngine.AudioClip _hoverSound;

	[SerializeField]
	private UnityEngine.AudioClip _cameraShutterSound;

	[SerializeField]
	private UnityEngine.AudioClip _screenshotBeepSound;

	[SerializeField]
	private UnityEngine.AudioClip _streamingStarted;

	[SerializeField]
	private UnityEngine.AudioClip _streamingStopped;

	[Header("Audio Volumes")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _recordingStartVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _recordingSavedVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _clickDownVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _clickUpVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _hoverSoundVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _cameraShutterSoundVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _screenshotBeepSoundVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _streamingStartedVolume;

	[SerializeField]
	[Range(0f, 1f)]
	private float _streamingStoppedVolume;

	private void Awake()
	{
		InitializeAudioClipDictionary();
	}

	private void InitializeAudioClipDictionary()
	{
		_allAudioClips = new Dictionary<AudioClip, AudioClipAndVolume>
		{
			{
				AudioClip.RecordingStart,
				new AudioClipAndVolume(_recordingStart, _recordingStartVolume)
			},
			{
				AudioClip.RecordingSaved,
				new AudioClipAndVolume(_recordingSaved, _recordingSavedVolume)
			},
			{
				AudioClip.ClickDown,
				new AudioClipAndVolume(_clickDown, _clickDownVolume)
			},
			{
				AudioClip.ClickUp,
				new AudioClipAndVolume(_clickUp, _clickUpVolume)
			},
			{
				AudioClip.HoverSound,
				new AudioClipAndVolume(_hoverSound, _hoverSoundVolume)
			},
			{
				AudioClip.CameraShutterSound,
				new AudioClipAndVolume(_cameraShutterSound, _cameraShutterSoundVolume)
			},
			{
				AudioClip.ScreenshotBeepSound,
				new AudioClipAndVolume(_screenshotBeepSound, _screenshotBeepSoundVolume)
			},
			{
				AudioClip.StreamingStarted,
				new AudioClipAndVolume(_streamingStarted, _streamingStartedVolume)
			},
			{
				AudioClip.StreamingStopped,
				new AudioClipAndVolume(_streamingStopped, _streamingStoppedVolume)
			}
		};
	}

	private void Start()
	{
		foreach (KeyValuePair<AudioClip, AudioClipAndVolume> allAudioClip in _allAudioClips)
		{
			_lckService.PreloadDiscreetAudio(allAudioClip.Value.clip, allAudioClip.Value.volume);
		}
	}

	public void PlayDiscreetAudioClip(AudioClip clip)
	{
		_lckService.PlayDiscreetAudioClip(_allAudioClips[clip].clip);
	}
}
