using UnityEngine;

namespace Oculus.Interaction;

public class AudioTrigger : MonoBehaviour
{
	[SerializeField]
	private AudioSource _audioSource;

	[Tooltip("Audio clip arrays with a value greater than 1 will have randomized playback.")]
	[SerializeField]
	private AudioClip[] _audioClips;

	[Tooltip("Volume set here will override the volume set on the attached sound source component.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float _volume = 0.7f;

	[Tooltip("Check the 'Use Random Range' bool and adjust the min and max slider values for randomized volume level playback.")]
	[SerializeField]
	private MinMaxPair _volumeRandomization;

	[Tooltip("Pitch set here will override the volume set on the attached sound source component.")]
	[SerializeField]
	[Range(-3f, 3f)]
	[Space(10f)]
	private float _pitch = 1f;

	[Tooltip("Check the 'Use Random Range' bool and adjust the min and max slider values for randomized volume level playback.")]
	[SerializeField]
	private MinMaxPair _pitchRandomization;

	[Tooltip("True by default. Set to false for sounds to bypass the spatializer plugin. Will override settings on attached audio source.")]
	[SerializeField]
	[Space(10f)]
	private bool _spatialize = true;

	[Tooltip("False by default. Set to true to enable looping on this sound. Will override settings on attached audio source.")]
	[SerializeField]
	private bool _loop;

	[Tooltip("100% by default. Sets likelihood sample will actually play when called.")]
	[SerializeField]
	private float _chanceToPlay = 100f;

	[Tooltip("If enabled, audio will play automatically when this gameobject is enabled.")]
	[SerializeField]
	[Optional]
	private bool _playOnStart;

	private int _previousAudioClipIndex = -1;

	public float Volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = value;
		}
	}

	public MinMaxPair VolumeRandomization
	{
		get
		{
			return _volumeRandomization;
		}
		set
		{
			_volumeRandomization = value;
		}
	}

	public float Pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			_pitch = value;
		}
	}

	public MinMaxPair PitchRandomization
	{
		get
		{
			return _pitchRandomization;
		}
		set
		{
			_pitchRandomization = value;
		}
	}

	public bool Spatialize
	{
		get
		{
			return _spatialize;
		}
		set
		{
			_spatialize = value;
		}
	}

	public bool Loop
	{
		get
		{
			return _loop;
		}
		set
		{
			_loop = value;
		}
	}

	public float ChanceToPlay
	{
		get
		{
			return _chanceToPlay;
		}
		set
		{
			_chanceToPlay = value;
		}
	}

	protected virtual void Start()
	{
		if (_audioSource == null)
		{
			_audioSource = base.gameObject.GetComponent<AudioSource>();
		}
		if (_playOnStart)
		{
			PlayAudio();
		}
	}

	public void PlayAudio()
	{
		float num = Random.Range(0f, 100f);
		if (!(_chanceToPlay < 100f) || !(num > _chanceToPlay))
		{
			if (_volumeRandomization.UseRandomRange)
			{
				_audioSource.volume = Random.Range(_volumeRandomization.Min, _volumeRandomization.Max);
			}
			else
			{
				_audioSource.volume = _volume;
			}
			if (_pitchRandomization.UseRandomRange)
			{
				_audioSource.pitch = Random.Range(_pitchRandomization.Min, _pitchRandomization.Max);
			}
			else
			{
				_audioSource.pitch = _pitch;
			}
			_audioSource.spatialize = _spatialize;
			_audioSource.loop = _loop;
			_audioSource.clip = RandomClipWithoutRepeat();
			_audioSource.Play();
		}
	}

	private AudioClip RandomClipWithoutRepeat()
	{
		if (_audioClips.Length == 1)
		{
			return _audioClips[0];
		}
		int num = Random.Range(1, _audioClips.Length);
		int num2 = (_previousAudioClipIndex = (_previousAudioClipIndex + num) % _audioClips.Length);
		return _audioClips[num2];
	}

	public void InjectAllAudioTrigger(AudioSource audioSource, AudioClip[] audioClips)
	{
		InjectAudioSource(audioSource);
		InjectAudioClips(audioClips);
	}

	public void InjectAudioSource(AudioSource audioSource)
	{
		_audioSource = audioSource;
	}

	public void InjectAudioClips(AudioClip[] audioClips)
	{
		_audioClips = audioClips;
	}

	public void InjectOptionalPlayOnStart(bool playOnStart)
	{
		_playOnStart = playOnStart;
	}
}
