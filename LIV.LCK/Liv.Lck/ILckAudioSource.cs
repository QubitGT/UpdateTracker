using Liv.Lck.Collections;

namespace Liv.Lck;

public interface ILckAudioSource
{
	public delegate void AudioDataCallbackDelegate(AudioBuffer audioBuffer);

	void GetAudioData(AudioDataCallbackDelegate callback);

	void EnableCapture();

	void DisableCapture();

	bool IsCapturing();
}
