using UnityEngine;

namespace Photon.Voice.Unity;

public class UnityAudioOut : AudioOutDelayControl<float>
{
	protected readonly AudioSource source;

	protected AudioClip clip;

	public override int OutPos
	{
		get
		{
			if (!source.clip)
			{
				return 0;
			}
			return source.timeSamples;
		}
	}

	public UnityAudioOut(AudioSource audioSource, PlayDelayConfig playDelayConfig, ILogger logger, string logPrefix, bool debugInfo)
		: base(processInService: true, playDelayConfig, logger, "[PV] [Unity] AudioOut" + ((logPrefix == "") ? "" : (" " + logPrefix)), debugInfo)
	{
		source = audioSource;
	}

	public override void OutCreate(int frequency, int channels, int bufferSamples)
	{
		Debug.Log("UnityAudioOut :: OutCreate " + source.gameObject.name, source);
		source.loop = true;
		clip = AudioClip.Create("UnityAudioOut", bufferSamples, channels, frequency, stream: false);
		source.clip = clip;
	}

	public override void OutStart()
	{
		if (source.clip != null)
		{
			source.Play();
		}
	}

	public override void OutWrite(float[] data, int offsetSamples)
	{
		clip.SetData(data, offsetSamples);
	}

	public override void Stop()
	{
		base.Stop();
		source.Stop();
		if (source != null)
		{
			source.clip = null;
			Object.Destroy(clip);
			clip = null;
		}
	}

	public override void ToggleAudioSource(bool toggle)
	{
		if (toggle)
		{
			source.clip = clip;
			source.Play();
		}
		else
		{
			source.Stop();
			source.clip = null;
		}
	}
}
