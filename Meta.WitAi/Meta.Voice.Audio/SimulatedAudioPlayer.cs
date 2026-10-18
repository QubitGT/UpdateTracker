using Meta.Voice.Logging;
using UnityEngine;

namespace Meta.Voice.Audio;

[LogCategory((LogCategory)25)]
public class SimulatedAudioPlayer : BaseAudioPlayer
{
	private float _elapsedTime;

	private bool _playing;

	public override bool IsPlaying => _playing;

	public override bool CanSetElapsedSamples => true;

	public override int ElapsedSamples
	{
		get
		{
			if (base.ClipStream != null)
			{
				return GetSamplesFromSeconds(_elapsedTime);
			}
			return 0;
		}
	}

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Audio);


	public override void Init()
	{
	}

	public override string GetPlaybackErrors()
	{
		return string.Empty;
	}

	protected override void Play(int offsetSamples = 0)
	{
		if (base.ClipStream == null)
		{
			Logger.Error("{0} cannot play null Audio clip stream", GetType().Name);
		}
		else
		{
			_elapsedTime = GetSecondsFromSamples(offsetSamples);
			_playing = true;
		}
	}

	public override void Pause()
	{
		if (IsPlaying)
		{
			_playing = false;
		}
	}

	public override void Resume()
	{
		if (!IsPlaying)
		{
			_playing = true;
		}
	}

	public override void Stop()
	{
		if (IsPlaying)
		{
			_playing = false;
		}
		base.Stop();
	}

	private void Update()
	{
		if (IsPlaying && base.ClipStream != null)
		{
			_elapsedTime += Time.deltaTime;
			if (base.ClipStream.IsComplete && _elapsedTime >= base.ClipStream.Length)
			{
				_playing = false;
			}
		}
	}

	private int GetSamplesFromSeconds(float elapsedSeconds)
	{
		return Mathf.FloorToInt(elapsedSeconds * (float)base.ClipStream.Channels * (float)base.ClipStream.SampleRate);
	}

	private float GetSecondsFromSamples(int samples)
	{
		return (float)samples / (float)(base.ClipStream.Channels * base.ClipStream.SampleRate);
	}
}
