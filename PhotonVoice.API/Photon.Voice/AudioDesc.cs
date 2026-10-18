using System;

namespace Photon.Voice;

public class AudioDesc : IAudioDesc, IDisposable
{
	public int SamplingRate { get; private set; }

	public int Channels { get; private set; }

	public string Error { get; private set; }

	public AudioDesc(int samplingRate, int channels, string error)
	{
		SamplingRate = samplingRate;
		Channels = channels;
		Error = error;
	}

	public void Dispose()
	{
	}
}
