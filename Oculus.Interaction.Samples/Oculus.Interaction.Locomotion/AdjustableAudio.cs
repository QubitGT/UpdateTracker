using UnityEngine;

namespace Oculus.Interaction.Locomotion;

[RequireComponent(typeof(AudioSource))]
public class AdjustableAudio : MonoBehaviour
{
	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private AudioClip _audioClip;

	[SerializeField]
	[Range(0f, 1f)]
	private float _volumeFactor = 1f;

	[SerializeField]
	private AnimationCurve _volumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _pitchCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f);

	protected bool _started;

	public AudioClip AudioClip
	{
		get
		{
			return _audioClip;
		}
		set
		{
			_audioClip = value;
		}
	}

	public float VolumeFactor
	{
		get
		{
			return _volumeFactor;
		}
		set
		{
			_volumeFactor = value;
		}
	}

	public AnimationCurve VolumeCurve
	{
		get
		{
			return _volumeCurve;
		}
		set
		{
			_volumeCurve = value;
		}
	}

	public AnimationCurve PitchCurve
	{
		get
		{
			return _pitchCurve;
		}
		set
		{
			_pitchCurve = value;
		}
	}

	protected virtual void Reset()
	{
		_audioSource = base.gameObject.GetComponent<AudioSource>();
		_audioClip = _audioSource.clip;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public void PlayAudio(float volumeT, float pitchT, float pan = 0f)
	{
		if (_audioSource.isActiveAndEnabled)
		{
			_audioSource.volume = _volumeCurve.Evaluate(volumeT) * VolumeFactor;
			_audioSource.pitch = _pitchCurve.Evaluate(pitchT);
			_audioSource.panStereo = pan;
			_audioSource.PlayOneShot(_audioClip);
		}
	}

	public void InjectAllAdjustableAudio(AudioSource audioSource)
	{
		InjectAudioSource(audioSource);
	}

	public void InjectAudioSource(AudioSource audioSource)
	{
		_audioSource = audioSource;
	}
}
